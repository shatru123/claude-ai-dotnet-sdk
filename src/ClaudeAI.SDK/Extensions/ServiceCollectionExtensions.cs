using ClaudeAI.SDK.Configuration;
using ClaudeAI.SDK.Core;
using ClaudeAI.SDK.Services;
using ClaudeAI.SDK.Skills;
using ClaudeAI.SDK.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeAI.SDK.Extensions;

/// <summary>
/// IServiceCollection extensions for registering the Claude AI SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Claude AI SDK services.
    /// Reads configuration from IConfiguration under the "Claude" section.
    /// </summary>
    /// <example>
    /// // Program.cs
    /// builder.Services.AddClaudeAI(builder.Configuration);
    /// </example>
    public static IServiceCollection AddClaudeAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind options from appsettings.json "Claude" section
        services.Configure<ClaudeOptions>(
            configuration.GetSection(ClaudeOptions.SectionName));

        // Core HTTP client — typed client manages lifetime automatically
        services.AddHttpClient<IClaudeService, ClaudeService>();

        // Skills (Scoped — one per HTTP request)
        services.AddScoped<SummarizationSkill>();
        services.AddScoped<CodeExplanationSkill>();
        services.AddScoped<DocumentationSkill>();
        services.AddScoped<ReviewSkill>();
        services.AddScoped<DecisionSupportSkill>();

        // Tools (Singleton — stateless, safe to share)
        services.AddSingleton<InputValidatorTool>();
        services.AddSingleton<ContentSanitizerTool>();
        services.AddSingleton<TokenEstimatorTool>();
        services.AddSingleton<ResponseFormatterTool>();

        // High-level facade
        services.AddScoped<IClaudeApplicationService, ClaudeApplicationService>();

        return services;
    }
}
