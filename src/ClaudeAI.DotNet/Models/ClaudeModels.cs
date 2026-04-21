using System.Text.Json.Serialization;

namespace ClaudeAI.DotNet.Models;

/// <summary>
/// Represents a message in a Claude conversation.
/// </summary>
public class ClaudeMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    public static ClaudeMessage User(string content) => new() { Role = "user", Content = content };
    public static ClaudeMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}

/// <summary>
/// Request payload sent to the Claude API.
/// </summary>
public class ClaudeRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 4096;

    [JsonPropertyName("messages")]
    public List<ClaudeMessage> Messages { get; set; } = new();

    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Stream { get; set; }
}

/// <summary>
/// Response received from the Claude API.
/// </summary>
public class ClaudeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public List<ClaudeContent> Content { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }

    [JsonPropertyName("usage")]
    public ClaudeUsage? Usage { get; set; }

    /// <summary>Convenience property to get the text of the first content block.</summary>
    public string Text => Content.FirstOrDefault()?.Text ?? string.Empty;
}

/// <summary>
/// A content block within a Claude response.
/// </summary>
public class ClaudeContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Token usage information from the API response.
/// </summary>
public class ClaudeUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    public int TotalTokens => InputTokens + OutputTokens;
}

/// <summary>
/// A chunk received during streaming.
/// </summary>
public class StreamChunk
{
    public string Delta { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? StopReason { get; set; }
}
