using System.Reflection;
using System.Text.Json.Serialization;

namespace ClaudeAI.DotNet.Tools;

/// <summary>
/// Marks a method as a Claude-callable tool (function calling).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ClaudeToolAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public ClaudeToolAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Represents the definition of a tool that Claude can call.
/// </summary>
public class ClaudeToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("input_schema")]
    public object InputSchema { get; set; } = new { type = "object", properties = new { } };
}

/// <summary>
/// Registry for discovering and managing Claude tools.
/// </summary>
public class ToolRegistry
{
    private readonly Dictionary<string, (MethodInfo Method, object Instance)> _tools = new();

    /// <summary>
    /// Registers all methods decorated with [ClaudeTool] from the given instance.
    /// </summary>
    public void Register(object instance)
    {
        var type = instance.GetType();
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = method.GetCustomAttribute<ClaudeToolAttribute>();
            if (attr == null) continue;
            _tools[attr.Name] = (method, instance);
        }
    }

    /// <summary>
    /// Gets tool definitions for sending to the Claude API.
    /// </summary>
    public IEnumerable<ClaudeToolDefinition> GetDefinitions()
    {
        foreach (var (name, (method, _)) in _tools)
        {
            var attr = method.GetCustomAttribute<ClaudeToolAttribute>()!;
            yield return new ClaudeToolDefinition
            {
                Name = name,
                Description = attr.Description
            };
        }
    }

    /// <summary>
    /// Invokes a registered tool by name.
    /// </summary>
    public async Task<object?> InvokeAsync(string toolName, Dictionary<string, object> parameters)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
            throw new InvalidOperationException($"Tool '{toolName}' is not registered.");

        var (method, instance) = tool;
        var paramValues = method.GetParameters()
            .Select(p => parameters.TryGetValue(p.Name!, out var val) ? val : null)
            .ToArray();

        var result = method.Invoke(instance, paramValues);

        if (result is Task task)
        {
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task);
        }

        return result;
    }

    public bool HasTools => _tools.Count > 0;
}
