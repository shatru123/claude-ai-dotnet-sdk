namespace ClaudeAI.SDK.Models;

/// <summary>Represents an inbound request to the Claude AI integration layer.</summary>
public class ClaudeRequest
{
    /// <summary>The user's message or prompt.</summary>
    public required string UserMessage { get; init; }

    /// <summary>Optional system prompt override. Replaces the default from config.</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>Conversation history for multi-turn interactions.</summary>
    public List<ConversationMessage> History { get; init; } = [];

    /// <summary>Extra context injected into the system prompt automatically.</summary>
    public string? Context { get; init; }
}

/// <summary>Represents the result of a Claude AI request.</summary>
public class ClaudeResponse
{
    public bool Success { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? Error { get; init; }
    public TokenUsage? Usage { get; init; }
    public string? Model { get; init; }
}

/// <summary>A single message turn in a conversation history.</summary>
public record ConversationMessage(string Role, string Content);

/// <summary>Token consumption reported by the Anthropic API.</summary>
public record TokenUsage(int InputTokens, int OutputTokens)
{
    public int TotalTokens => InputTokens + OutputTokens;
}
