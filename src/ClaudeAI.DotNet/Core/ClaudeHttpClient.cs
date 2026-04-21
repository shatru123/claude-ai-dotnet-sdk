using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using ClaudeAI.DotNet.Configuration;
using ClaudeAI.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaudeAI.DotNet.Core;

/// <summary>
/// Low-level HTTP client for the Anthropic API.
/// </summary>
public interface IClaudeHttpClient
{
    Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StreamChunk> StreamAsync(ClaudeRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public class ClaudeHttpClient : IClaudeHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ClaudeHttpClient(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending request to Claude API. Model: {Model}, Tokens: {MaxTokens}", 
            request.Model, request.MaxTokens);

        var attempts = 0;
        while (true)
        {
            try
            {
                attempts++;
                var response = await _httpClient.PostAsJsonAsync(
                    "/v1/messages", request, JsonOptions, cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(
                    JsonOptions, cancellationToken);

                _logger.LogDebug("Claude API response received. Tokens used: {Tokens}", 
                    result?.Usage?.TotalTokens);

                return result ?? throw new InvalidOperationException("Empty response from Claude API.");
            }
            catch (HttpRequestException ex) when (attempts < _options.MaxRetries)
            {
                _logger.LogWarning(ex, "Claude API request failed (attempt {Attempt}/{Max}). Retrying...",
                    attempts, _options.MaxRetries);

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)), cancellationToken);
            }
        }
    }

    public async IAsyncEnumerable<StreamChunk> StreamAsync(
        ClaudeRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;

        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/messages") { Content = content };
        using var response = await _httpClient.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") yield break;

            StreamChunk? chunk = null;
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var typeEl))
                {
                    var type = typeEl.GetString();
                    if (type == "content_block_delta" && root.TryGetProperty("delta", out var delta))
                    {
                        chunk = new StreamChunk { Delta = delta.GetProperty("text").GetString() ?? "" };
                    }
                    else if (type == "message_stop")
                    {
                        chunk = new StreamChunk { IsFinal = true };
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse stream chunk: {Data}", data);
            }

            if (chunk != null) yield return chunk;
        }
    }
}
