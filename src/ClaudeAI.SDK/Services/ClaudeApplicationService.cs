using ClaudeAI.SDK.Commands;
using ClaudeAI.SDK.Core;
using ClaudeAI.SDK.Models;
using ClaudeAI.SDK.Skills;
using ClaudeAI.SDK.Tools;
using Microsoft.Extensions.Logging;

namespace ClaudeAI.SDK.Services;

/// <summary>
/// High-level facade that application code (controllers, services) should
/// depend on. Orchestrates skills, commands, and tools internally.
/// </summary>
public interface IClaudeApplicationService
{
    Task<ClaudeResponse> SummarizeAsync(string content, string? context = null, CancellationToken ct = default);
    Task<ClaudeResponse> ExplainCodeAsync(string code, string? language = null, CancellationToken ct = default);
    Task<ClaudeResponse> GenerateDocumentationAsync(string input, string docType = "API", CancellationToken ct = default);
    Task<ClaudeResponse> ReviewAsync(string artifact, string reviewType = "code", CancellationToken ct = default);
    Task<ClaudeResponse> DecisionSupportAsync(string context, params string[] options);
    Task<ClaudeResponse> ChatAsync(string message, List<ConversationMessage>? history = null, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamChatAsync(string message, CancellationToken ct = default);
}

public sealed class ClaudeApplicationService : IClaudeApplicationService
{
    private readonly IClaudeService _claude;
    private readonly SummarizationSkill _summarization;
    private readonly CodeExplanationSkill _codeExplanation;
    private readonly DocumentationSkill _documentation;
    private readonly ReviewSkill _review;
    private readonly DecisionSupportSkill _decisionSupport;
    private readonly InputValidatorTool _validator;
    private readonly ContentSanitizerTool _sanitizer;
    private readonly ILogger<ClaudeApplicationService> _logger;

    public ClaudeApplicationService(
        IClaudeService claude,
        SummarizationSkill summarization,
        CodeExplanationSkill codeExplanation,
        DocumentationSkill documentation,
        ReviewSkill review,
        DecisionSupportSkill decisionSupport,
        InputValidatorTool validator,
        ContentSanitizerTool sanitizer,
        ILogger<ClaudeApplicationService> logger)
    {
        _claude = claude;
        _summarization = summarization;
        _codeExplanation = codeExplanation;
        _documentation = documentation;
        _review = review;
        _decisionSupport = decisionSupport;
        _validator = validator;
        _sanitizer = sanitizer;
        _logger = logger;
    }

    public async Task<ClaudeResponse> SummarizeAsync(string content, string? context = null, CancellationToken ct = default)
    {
        var clean = await PrepareInputAsync(content);
        if (clean is null) return Error("Invalid or empty input.");
        return await Safe(new SummarizeCommand(_summarization, clean, context), ct);
    }

    public async Task<ClaudeResponse> ExplainCodeAsync(string code, string? language = null, CancellationToken ct = default)
    {
        var clean = await PrepareInputAsync(code);
        if (clean is null) return Error("Invalid or empty code input.");
        return await Safe(new ExplainCodeCommand(_codeExplanation, clean, language), ct);
    }

    public async Task<ClaudeResponse> GenerateDocumentationAsync(string input, string docType = "API", CancellationToken ct = default)
    {
        var clean = await PrepareInputAsync(input);
        if (clean is null) return Error("Invalid or empty input.");
        return await Safe(new GenerateDocumentationCommand(_documentation, clean, docType), ct);
    }

    public async Task<ClaudeResponse> ReviewAsync(string artifact, string reviewType = "code", CancellationToken ct = default)
    {
        var clean = await PrepareInputAsync(artifact);
        if (clean is null) return Error("Invalid or empty input.");
        return await Safe(new ReviewCommand(_review, clean, reviewType), ct);
    }

    public async Task<ClaudeResponse> DecisionSupportAsync(string context, params string[] options)
    {
        var clean = await PrepareInputAsync(context);
        if (clean is null) return Error("Invalid or empty context.");
        return await Safe(new DecisionSupportCommand(_decisionSupport, clean, options));
    }

    public Task<ClaudeResponse> ChatAsync(string message, List<ConversationMessage>? history = null, CancellationToken ct = default)
        => _claude.SendAsync(new ClaudeRequest { UserMessage = message, History = history ?? [] }, ct);

    public IAsyncEnumerable<string> StreamChatAsync(string message, CancellationToken ct = default)
        => _claude.StreamAsync(new ClaudeRequest { UserMessage = message }, ct);

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<string?> PrepareInputAsync(string input)
    {
        var validation = await _validator.ExecuteAsync(input);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Error}", validation.ErrorMessage);
            return null;
        }
        var sanitized = await _sanitizer.ExecuteAsync(new SanitizeRequest(input));
        return sanitized.Content;
    }

    private async Task<ClaudeResponse> Safe<T>(IClaudeCommand<T> cmd, CancellationToken ct = default)
        where T : ClaudeResponse
    {
        try { return await cmd.ExecuteAsync(ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed");
            return Error(ex.Message);
        }
    }

    private static ClaudeResponse Error(string message) =>
        new() { Success = false, Error = message };
}
