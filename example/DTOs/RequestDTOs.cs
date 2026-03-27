using ClaudeAI.SDK.Models;

namespace ClaudeAI.SDK.Example.DTOs;

/// <summary>Request for text summarization.</summary>
public record TextRequest(string Content, string? Context = null);

/// <summary>Request for code explanation.</summary>
public record CodeRequest(string Code, string? Language = null);

/// <summary>Request for documentation generation.</summary>
public record DocRequest(string Input, string DocType = "API");

/// <summary>Request for code or design review.</summary>
public record ReviewRequest(string Artifact, string ReviewType = "code");

/// <summary>Request for decision support analysis.</summary>
public record DecisionRequest(string Context, string[]? Options = null);

/// <summary>Request for a chat message, with optional conversation history.</summary>
public record ChatRequest(string Message, List<ConversationMessage>? History = null);
