using ClaudeAI.SDK.Core;
using ClaudeAI.SDK.Models;

namespace ClaudeAI.SDK.Skills;

// ── Skill contract ───────────────────────────────────────────────────────────

/// <summary>
/// A Skill encapsulates a specific Claude behavior (e.g. Summarize, ExplainCode).
/// Skills are reusable across multiple Commands and contain no business logic.
/// </summary>
public interface IClaudeSkill
{
    string SkillName { get; }
    Task<ClaudeResponse> ExecuteAsync(string input, string? context = null, CancellationToken ct = default);
}

// ── Abstract base ────────────────────────────────────────────────────────────

public abstract class ClaudeSkillBase : IClaudeSkill
{
    protected readonly IClaudeService Claude;

    protected ClaudeSkillBase(IClaudeService claude) => Claude = claude;

    public abstract string SkillName { get; }
    protected abstract string SystemPrompt { get; }

    public virtual Task<ClaudeResponse> ExecuteAsync(
        string input, string? context = null, CancellationToken ct = default)
        => Claude.SendAsync(new ClaudeRequest
        {
            UserMessage = input,
            SystemPrompt = SystemPrompt,
            Context = context
        }, ct);
}

// ── Built-in skills ──────────────────────────────────────────────────────────

/// <summary>Condenses long content into a structured, concise summary.</summary>
public sealed class SummarizationSkill : ClaudeSkillBase
{
    public SummarizationSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "Summarization";
    protected override string SystemPrompt =>
        "You are an expert summarizer. " +
        "Produce a concise, well-structured summary of the provided content. " +
        "Use bullet points for key points. Keep under 200 words unless instructed otherwise.";
}

/// <summary>Walks through source code and explains what it does and how.</summary>
public sealed class CodeExplanationSkill : ClaudeSkillBase
{
    public CodeExplanationSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "CodeExplanation";
    protected override string SystemPrompt =>
        "You are a senior software engineer. " +
        "Explain the provided code clearly. " +
        "Format your response with sections: Overview, How it Works, Key Points, Potential Issues.";
}

/// <summary>Generates technical or API documentation from code or descriptions.</summary>
public sealed class DocumentationSkill : ClaudeSkillBase
{
    public DocumentationSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "Documentation";
    protected override string SystemPrompt =>
        "You are a technical writer specializing in software documentation. " +
        "Generate clear, professional documentation. " +
        "Include: Purpose, Parameters/Inputs, Return Values, Examples, Remarks.";
}

/// <summary>Reviews code or designs and returns structured, prioritised feedback.</summary>
public sealed class ReviewSkill : ClaudeSkillBase
{
    public ReviewSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "Review";
    protected override string SystemPrompt =>
        "You are a senior software architect conducting a thorough review. " +
        "Evaluate: correctness, performance, security, maintainability, and best practices. " +
        "Format: Summary, Issues (Critical / Major / Minor), Suggestions.";
}

/// <summary>Provides structured analysis and recommendations to support decisions.</summary>
public sealed class DecisionSupportSkill : ClaudeSkillBase
{
    public DecisionSupportSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "DecisionSupport";
    protected override string SystemPrompt =>
        "You are a strategic analyst. Provide a balanced analysis to aid decision making. " +
        "Format: Context, Options (pros/cons for each), Recommendation, Risks.";
}
