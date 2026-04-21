namespace ClaudeAI.DotNet.Configuration;

/// <summary>
/// Configuration options for the Claude AI SDK.
/// </summary>
public class ClaudeOptions
{
    public const string SectionName = "Claude";

    /// <summary>
    /// Your Anthropic API key. Required.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The Claude model to use. Defaults to Claude Sonnet.
    /// </summary>
    public string Model { get; set; } = ClaudeModel.Sonnet;

    /// <summary>
    /// Maximum number of tokens in the response. Defaults to 4096.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for response creativity (0.0 - 1.0). Defaults to 0.7.
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Base URL for the Anthropic API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    /// <summary>
    /// Anthropic API version header value.
    /// </summary>
    public string ApiVersion { get; set; } = "2023-06-01";

    /// <summary>
    /// HTTP request timeout in seconds. Defaults to 120.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Number of retry attempts on transient failures. Defaults to 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Optional system prompt to prepend to all requests.
    /// </summary>
    public string? SystemPrompt { get; set; }
}
