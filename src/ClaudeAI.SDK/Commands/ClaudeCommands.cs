using ClaudeAI.SDK.Models;
using ClaudeAI.SDK.Skills;

namespace ClaudeAI.SDK.Commands;

// ── Command contract ─────────────────────────────────────────────────────────

/// <summary>
/// A Command represents what the user wants to achieve.
/// Commands are intent-driven, contain no AI logic, and delegate to Skills.
/// </summary>
public interface IClaudeCommand<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken ct = default);
}

// ── SummarizeCommand ─────────────────────────────────────────────────────────

public sealed class SummarizeCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly SummarizationSkill _skill;
    private readonly string _content;
    private readonly string? _context;

    public SummarizeCommand(SummarizationSkill skill, string content, string? context = null)
    {
        _skill = skill;
        _content = content;
        _context = context;
    }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
        => _skill.ExecuteAsync($"Please summarize the following:\n\n{_content}", _context, ct);
}

// ── ExplainCodeCommand ───────────────────────────────────────────────────────

public sealed class ExplainCodeCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly CodeExplanationSkill _skill;
    private readonly string _code;
    private readonly string? _language;

    public ExplainCodeCommand(CodeExplanationSkill skill, string code, string? language = null)
    {
        _skill = skill;
        _code = code;
        _language = language;
    }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
    {
        var lang = _language ?? string.Empty;
        var prompt = $"Explain the following {lang} code:\n\n```{lang}\n{_code}\n```";
        return _skill.ExecuteAsync(prompt, ct: ct);
    }
}

// ── GenerateDocumentationCommand ─────────────────────────────────────────────

public sealed class GenerateDocumentationCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly DocumentationSkill _skill;
    private readonly string _input;
    private readonly string _docType;

    public GenerateDocumentationCommand(DocumentationSkill skill, string input, string docType = "API")
    {
        _skill = skill;
        _input = input;
        _docType = docType;
    }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
        => _skill.ExecuteAsync($"Generate {_docType} documentation for:\n\n{_input}", ct: ct);
}

// ── ReviewCommand ────────────────────────────────────────────────────────────

public sealed class ReviewCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly ReviewSkill _skill;
    private readonly string _artifact;
    private readonly string _reviewType;

    public ReviewCommand(ReviewSkill skill, string artifact, string reviewType = "code")
    {
        _skill = skill;
        _artifact = artifact;
        _reviewType = reviewType;
    }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
        => _skill.ExecuteAsync($"Please review the following {_reviewType}:\n\n{_artifact}", ct: ct);
}

// ── DecisionSupportCommand ───────────────────────────────────────────────────

public sealed class DecisionSupportCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly DecisionSupportSkill _skill;
    private readonly string _decisionContext;
    private readonly string[] _options;

    public DecisionSupportCommand(DecisionSupportSkill skill, string decisionContext, params string[] options)
    {
        _skill = skill;
        _decisionContext = decisionContext;
        _options = options;
    }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
    {
        var optionBlock = _options.Length > 0
            ? "\n\nOptions to evaluate:\n" + string.Join("\n", _options.Select((o, i) => $"{i + 1}. {o}"))
            : string.Empty;

        return _skill.ExecuteAsync($"{_decisionContext}{optionBlock}", ct: ct);
    }
}
