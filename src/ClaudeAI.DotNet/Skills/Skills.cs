namespace ClaudeAI.DotNet.Skills;

/// <summary>
/// Defines a reusable AI skill that modifies prompt behavior.
/// </summary>
public interface ISkill
{
    /// <summary>Unique name of the skill.</summary>
    string Name { get; }

    /// <summary>
    /// Returns the system prompt instructions for this skill.
    /// </summary>
    string GetSystemPrompt();

    /// <summary>
    /// Optionally transforms the user prompt before sending.
    /// </summary>
    string TransformPrompt(string userPrompt) => userPrompt;
}

/// <summary>
/// Base class for skills with common defaults.
/// </summary>
public abstract class BaseSkill : ISkill
{
    public abstract string Name { get; }
    public abstract string GetSystemPrompt();
    public virtual string TransformPrompt(string userPrompt) => userPrompt;
}

/// <summary>
/// Skill for summarizing large blocks of text.
/// </summary>
public class SummarizationSkill : BaseSkill
{
    public override string Name => "Summarization";

    public override string GetSystemPrompt() =>
        """
        You are an expert summarization assistant.
        Your task is to produce clear, concise, and accurate summaries.
        - Capture the key points and main ideas
        - Preserve important details and nuance
        - Use clear, professional language
        - Structure the summary with bullet points if the content is complex
        - Keep it significantly shorter than the original
        """;

    public override string TransformPrompt(string userPrompt) =>
        $"Please summarize the following:\n\n{userPrompt}";
}

/// <summary>
/// Skill for reviewing and improving code.
/// </summary>
public class CodeReviewSkill : BaseSkill
{
    public override string Name => "CodeReview";

    public override string GetSystemPrompt() =>
        """
        You are a senior software engineer conducting a thorough code review.
        For each review, provide:
        1. **Summary** - Brief overview of what the code does
        2. **Issues** - Bugs, logic errors, or security concerns
        3. **Improvements** - Performance, readability, or maintainability suggestions
        4. **Best Practices** - Industry standards and patterns that apply
        5. **Overall Rating** - Score from 1-10 with justification
        Be constructive, specific, and actionable.
        """;

    public override string TransformPrompt(string userPrompt) =>
        $"Please review the following code:\n\n```\n{userPrompt}\n```";
}

/// <summary>
/// Skill for translating text to a target language.
/// </summary>
public class TranslationSkill : BaseSkill
{
    private readonly string _targetLanguage;

    public TranslationSkill(string targetLanguage = "Spanish")
    {
        _targetLanguage = targetLanguage;
    }

    public override string Name => "Translation";

    public override string GetSystemPrompt() =>
        $"""
        You are a professional translator specializing in accurate, natural translations.
        Translate all content to {_targetLanguage}.
        - Preserve the original tone and style
        - Use natural, idiomatic expressions
        - Maintain technical terminology where appropriate
        - Do not add explanations unless asked
        """;

    public override string TransformPrompt(string userPrompt) =>
        $"Translate the following to {_targetLanguage}:\n\n{userPrompt}";
}

/// <summary>
/// Skill for performing sentiment analysis on text.
/// </summary>
public class SentimentAnalysisSkill : BaseSkill
{
    public override string Name => "SentimentAnalysis";

    public override string GetSystemPrompt() =>
        """
        You are a sentiment analysis expert.
        Analyze the sentiment of the provided text and return a JSON response with:
        {
          "sentiment": "positive" | "negative" | "neutral" | "mixed",
          "score": 0.0 to 1.0,
          "confidence": 0.0 to 1.0,
          "emotions": ["joy", "anger", "sadness", etc.],
          "summary": "Brief explanation"
        }
        Always respond with valid JSON only.
        """;
}

/// <summary>
/// Skill for generating documentation from code.
/// </summary>
public class DocumentationSkill : BaseSkill
{
    public override string Name => "Documentation";

    public override string GetSystemPrompt() =>
        """
        You are a technical documentation expert.
        Generate clear, comprehensive documentation including:
        - Purpose and overview
        - Parameters and return values
        - Usage examples
        - Edge cases and error handling
        - Any important notes or warnings
        Format output as proper XML documentation comments for .NET code.
        """;

    public override string TransformPrompt(string userPrompt) =>
        $"Generate documentation for:\n\n{userPrompt}";
}
