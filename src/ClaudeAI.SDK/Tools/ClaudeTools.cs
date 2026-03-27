using System.Text.Json;
using System.Text.RegularExpressions;

namespace ClaudeAI.SDK.Tools;

// ── Tool contract ────────────────────────────────────────────────────────────

/// <summary>
/// Tools perform deterministic operations: validation, sanitization, formatting.
/// They do NOT call Claude. They pre/post-process inputs and outputs.
/// </summary>
public interface IClaudeTool<TInput, TOutput>
{
    string ToolName { get; }
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default);
}

// ── InputValidatorTool ───────────────────────────────────────────────────────

public record ValidationResult(bool IsValid, string? ErrorMessage = null);

/// <summary>Validates user input before it reaches Claude.</summary>
public sealed class InputValidatorTool : IClaudeTool<string, ValidationResult>
{
    private const int MinLength = 3;
    private const int MaxLength = 50_000;

    public string ToolName => "InputValidator";

    public Task<ValidationResult> ExecuteAsync(string input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Task.FromResult(new ValidationResult(false, "Input cannot be empty."));

        if (input.Length < MinLength)
            return Task.FromResult(new ValidationResult(false, $"Input must be at least {MinLength} characters."));

        if (input.Length > MaxLength)
            return Task.FromResult(new ValidationResult(false, $"Input exceeds the maximum of {MaxLength} characters."));

        return Task.FromResult(new ValidationResult(true));
    }
}

// ── ContentSanitizerTool ─────────────────────────────────────────────────────

public record SanitizeRequest(string Content, int MaxLength = 10_000);
public record SanitizeResult(string Content, bool WasTruncated, int OriginalLength);

/// <summary>Normalises and optionally truncates content before sending to Claude.</summary>
public sealed class ContentSanitizerTool : IClaudeTool<SanitizeRequest, SanitizeResult>
{
    public string ToolName => "ContentSanitizer";

    public Task<SanitizeResult> ExecuteAsync(SanitizeRequest input, CancellationToken ct = default)
    {
        var cleaned = input.Content.Replace("\r\n", "\n").Trim();
        var original = cleaned.Length;
        var truncated = false;

        if (cleaned.Length > input.MaxLength)
        {
            cleaned = cleaned[..input.MaxLength] + "\n\n[Content truncated due to length]";
            truncated = true;
        }

        return Task.FromResult(new SanitizeResult(cleaned, truncated, original));
    }
}

// ── TokenEstimatorTool ───────────────────────────────────────────────────────

/// <summary>
/// Rough token estimate (~4 chars per token).
/// Use before sending to Claude to avoid exceeding context limits.
/// </summary>
public sealed class TokenEstimatorTool : IClaudeTool<string, int>
{
    public string ToolName => "TokenEstimator";

    public Task<int> ExecuteAsync(string input, CancellationToken ct = default)
        => Task.FromResult(Math.Max(1, input.Length / 4));
}

// ── ResponseFormatterTool ────────────────────────────────────────────────────

public enum ResponseFormat { Markdown, PlainText, Html, Json }
public record FormatRequest(string Content, ResponseFormat Format);

/// <summary>Post-processes Claude's response into the desired output format.</summary>
public sealed class ResponseFormatterTool : IClaudeTool<FormatRequest, string>
{
    public string ToolName => "ResponseFormatter";

    public Task<string> ExecuteAsync(FormatRequest input, CancellationToken ct = default)
    {
        var result = input.Format switch
        {
            ResponseFormat.PlainText => StripMarkdown(input.Content),
            ResponseFormat.Html => WrapHtml(input.Content),
            ResponseFormat.Json => WrapJson(input.Content),
            _ => input.Content  // Markdown — pass through
        };
        return Task.FromResult(result);
    }

    private static string StripMarkdown(string md) =>
        Regex.Replace(md, @"(\*\*|__|##|#|\*|_|`)", string.Empty).Trim();

    private static string WrapHtml(string md) =>
        $"<div class=\"ai-response\">{System.Web.HttpUtility.HtmlEncode(md)}</div>";

    private static string WrapJson(string content) =>
        JsonSerializer.Serialize(
            new { response = content, generatedAt = DateTime.UtcNow },
            new JsonSerializerOptions { WriteIndented = true });
}
