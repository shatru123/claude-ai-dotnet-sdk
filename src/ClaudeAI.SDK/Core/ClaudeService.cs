using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClaudeAI.SDK.Configuration;
using ClaudeAI.SDK.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaudeAI.SDK.Core;

/// <summary>
/// Core service that communicates directly with the Anthropic Messages API.
/// All higher-level components (skills, commands) delegate here.
/// </summary>
public interface IClaudeService
{
    Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamAsync(ClaudeRequest request, CancellationToken ct = default);
}

public sealed class ClaudeService : IClaudeService
{
    private readonly HttpClient _http;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeService> _logger;

    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ApiVersion = "2023-06-01";

    public ClaudeService(
        HttpClient http,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;

        _http.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
        _http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    // ── Non-streaming ────────────────────────────────────────────────────────

    public async Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken ct = default)
    {
        var payload = BuildPayload(request, stream: false);
        _logger.LogDebug("Sending request to Claude. Model: {Model}", _options.Model);

        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(ApiUrl, content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, body);
                return Fail($"HTTP {response.StatusCode}: {body}");
            }

            var apiResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(body, JsonOptions);
            var text = apiResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;

            _logger.LogDebug("Claude OK — input: {In} tokens, output: {Out} tokens",
                apiResponse?.Usage?.InputTokens, apiResponse?.Usage?.OutputTokens);

            return new ClaudeResponse
            {
                Success = true,
                Content = text,
                Model = apiResponse?.Model,
                Usage = apiResponse?.Usage is { } u
                    ? new TokenUsage(u.InputTokens, u.OutputTokens)
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Claude API");
            return Fail(ex.Message);
        }
    }

    // ── Streaming (Server-Sent Events) ───────────────────────────────────────

    public async IAsyncEnumerable<string> StreamAsync(
        ClaudeRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = BuildPayload(request, stream: true);
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };
        using var response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") yield break;

            var evt = JsonSerializer.Deserialize<StreamEvent>(data, JsonOptions);
            if (evt?.Delta?.Text is { Length: > 0 } chunk)
                yield return chunk;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private object BuildPayload(ClaudeRequest request, bool stream) => new
    {
        model = _options.Model,
        max_tokens = _options.MaxTokens,
        temperature = _options.Temperature,
        stream,
        system = BuildSystemPrompt(request),
        messages = BuildMessages(request)
    };

    private string BuildSystemPrompt(ClaudeRequest request)
    {
        var sb = new StringBuilder(request.SystemPrompt ?? _options.DefaultSystemPrompt);
        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            sb.AppendLine().AppendLine("## Additional Context").Append(request.Context);
        }
        return sb.ToString();
    }

    private static List<object> BuildMessages(ClaudeRequest request)
    {
        var messages = request.History
            .Select(h => (object)new { role = h.Role, content = h.Content })
            .ToList();

        messages.Add(new { role = "user", content = request.UserMessage });
        return messages;
    }

    private static ClaudeResponse Fail(string error) =>
        new() { Success = false, Error = error };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ── Internal Anthropic API shapes ────────────────────────────────────────

    private record ClaudeApiResponse(string? Model, List<ContentBlock>? Content, ApiUsage? Usage);
    private record ContentBlock(string? Text);

    private record ApiUsage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);

    private record StreamEvent(StreamDelta? Delta);
    private record StreamDelta(string? Text);
}
