using ClaudeAI.DotNet.Models;
using ClaudeAI.DotNet.Skills;

namespace ClaudeAI.DotNet.Commands;

/// <summary>
/// Represents a command to send to Claude AI.
/// </summary>
public class ClaudeCommand
{
    /// <summary>The user's prompt or input text.</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Optional conversation history for multi-turn conversations.</summary>
    public List<ClaudeMessage> ConversationHistory { get; set; } = new();

    /// <summary>Optional skill to apply to this command.</summary>
    public ISkill? Skill { get; set; }

    /// <summary>Override the default system prompt for this command.</summary>
    public string? SystemPromptOverride { get; set; }

    /// <summary>Override the default model for this command.</summary>
    public string? ModelOverride { get; set; }

    /// <summary>Override the default max tokens for this command.</summary>
    public int? MaxTokensOverride { get; set; }
}

/// <summary>
/// Result of a Claude command execution.
/// </summary>
public class ClaudeCommandResult
{
    public bool IsSuccess { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public ClaudeUsage? Usage { get; set; }
    public string? Model { get; set; }
    public string? SkillUsed { get; set; }

    public static ClaudeCommandResult Success(ClaudeResponse response, string? skillName = null) => new()
    {
        IsSuccess = true,
        Content = response.Text,
        Usage = response.Usage,
        Model = response.Model,
        SkillUsed = skillName
    };

    public static ClaudeCommandResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}
