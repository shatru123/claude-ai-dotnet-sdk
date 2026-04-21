using ClaudeAI.DotNet.Commands;
using ClaudeAI.DotNet.Configuration;
using ClaudeAI.DotNet.Core;
using ClaudeAI.DotNet.Models;
using ClaudeAI.DotNet.Skills;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaudeAI.DotNet.Services;

/// <summary>
/// High-level client for interacting with Claude AI.
/// </summary>
public interface IClaudeClient
{
    /// <summary>
    /// Sends a simple prompt and returns the response text.
    /// </summary>
    Task<string> SendAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt with a specific skill applied.
    /// </summary>
    Task<string> SendWithSkillAsync(string prompt, ISkill skill, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command and returns a structured result.
    /// </summary>
    Task<ClaudeCommandResult> ExecuteAsync(ClaudeCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a response token by token.
    /// </summary>
    IAsyncEnumerable<StreamChunk> StreamAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a multi-turn conversation.
    /// </summary>
    Task<string> ChatAsync(List<ClaudeMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a fluent builder for configuring a request.
    /// </summary>
    ClaudeRequestBuilder With();
}

/// <inheritdoc />
public class ClaudeClient : IClaudeClient
{
    private readonly IClaudeHttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeClient> _logger;

    public ClaudeClient(
        IClaudeHttpClient httpClient,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest([ClaudeMessage.User(prompt)]);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.Text;
    }

    public async Task<string> SendWithSkillAsync(
        string prompt, ISkill skill, CancellationToken cancellationToken = default)
    {
        var transformedPrompt = skill.TransformPrompt(prompt);
        var request = BuildRequest([ClaudeMessage.User(transformedPrompt)], skill.GetSystemPrompt());
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.Text;
    }

    public async Task<ClaudeCommandResult> ExecuteAsync(
        ClaudeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = command.Skill != null
                ? command.Skill.TransformPrompt(command.Prompt)
                : command.Prompt;

            var systemPrompt = command.SystemPromptOverride
                ?? command.Skill?.GetSystemPrompt()
                ?? _options.SystemPrompt;

            var messages = new List<ClaudeMessage>(command.ConversationHistory)
            {
                ClaudeMessage.User(prompt)
            };

            var request = BuildRequest(messages, systemPrompt);

            if (command.ModelOverride != null) request.Model = command.ModelOverride;
            if (command.MaxTokensOverride.HasValue) request.MaxTokens = command.MaxTokensOverride.Value;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return ClaudeCommandResult.Success(response, command.Skill?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Claude command");
            return ClaudeCommandResult.Failure(ex.Message);
        }
    }

    public IAsyncEnumerable<StreamChunk> StreamAsync(
        string prompt, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest([ClaudeMessage.User(prompt)]);
        return _httpClient.StreamAsync(request, cancellationToken);
    }

    public async Task<string> ChatAsync(
        List<ClaudeMessage> messages, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.Text;
    }

    public ClaudeRequestBuilder With() => new(this);

    private ClaudeRequest BuildRequest(List<ClaudeMessage> messages, string? systemPrompt = null) => new()
    {
        Model = _options.Model,
        MaxTokens = _options.MaxTokens,
        Temperature = _options.Temperature,
        System = systemPrompt ?? _options.SystemPrompt,
        Messages = messages
    };
}

/// <summary>
/// Fluent builder for constructing Claude requests.
/// </summary>
public class ClaudeRequestBuilder
{
    private readonly IClaudeClient _client;
    private ISkill? _skill;

    internal ClaudeRequestBuilder(IClaudeClient client) => _client = client;

    public ClaudeRequestBuilder Skill(ISkill skill)
    {
        _skill = skill;
        return this;
    }

    public ClaudeRequestBuilder Skill<TSkill>() where TSkill : ISkill, new()
    {
        _skill = new TSkill();
        return this;
    }

    public Task<string> SendAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return _skill != null
            ? _client.SendWithSkillAsync(prompt, _skill, cancellationToken)
            : _client.SendAsync(prompt, cancellationToken);
    }
}
