namespace ClaudeAI.SDK.Configuration;

/// <summary>
/// Strongly-typed configuration for Claude AI.
/// Bind from appsettings.json under the "Claude" section.
/// </summary>
public class ClaudeOptions
{
    public const string SectionName = "Claude";

    /// <summary>API key from https://console.anthropic.com</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Claude model to use. Defaults to claude-sonnet-4-6.</summary>
    public string Model { get; set; } = "claude-sonnet-4-6";

    /// <summary>Maximum tokens in the response.</summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>Temperature — 0.0 = deterministic, 1.0 = creative.</summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>Default system prompt applied to every request unless overridden per skill.</summary>
    public string DefaultSystemPrompt { get; set; } =
        "You are a helpful AI assistant integrated into an enterprise .NET application. " +
        "Be concise, accurate, and professional.";

    /// <summary>HTTP request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>Number of automatic retries on transient failures.</summary>
    public int MaxRetries { get; set; } = 2;
}
