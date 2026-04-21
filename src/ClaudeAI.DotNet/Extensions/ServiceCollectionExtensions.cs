using ClaudeAI.DotNet.Configuration;
using ClaudeAI.DotNet.Core;
using ClaudeAI.DotNet.Services;
using ClaudeAI.DotNet.Skills;
using ClaudeAI.DotNet.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeAI.DotNet.Extensions;

/// <summary>
/// Extension methods for registering Claude AI services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Claude AI services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure Claude options.</param>
    /// <returns>A builder for further configuration.</returns>
    public static ClaudeAIBuilder AddClaudeAI(
        this IServiceCollection services,
        Action<ClaudeOptions> configure)
    {
        services.Configure(configure);
        return services.AddClaudeAICoreServices();
    }

    /// <summary>
    /// Adds Claude AI services using configuration section (e.g., appsettings.json "Claude" section).
    /// </summary>
    public static ClaudeAIBuilder AddClaudeAI(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<ClaudeOptions>(configuration.GetSection(ClaudeOptions.SectionName));
        return services.AddClaudeAICoreServices();
    }

    private static ClaudeAIBuilder AddClaudeAICoreServices(this IServiceCollection services)
    {
        services.AddHttpClient<IClaudeHttpClient, ClaudeHttpClient>();
        services.AddSingleton<ToolRegistry>();
        services.AddScoped<IClaudeClient, ClaudeClient>();

        return new ClaudeAIBuilder(services);
    }
}

/// <summary>
/// Fluent builder for configuring Claude AI services.
/// </summary>
public class ClaudeAIBuilder
{
    private readonly IServiceCollection _services;

    internal ClaudeAIBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    /// Registers a skill as a singleton service.
    /// </summary>
    public ClaudeAIBuilder WithSkill<TSkill>() where TSkill : class, ISkill
    {
        _services.AddSingleton<TSkill>();
        _services.AddSingleton<ISkill, TSkill>();
        return this;
    }

    /// <summary>
    /// Registers a tool provider whose [ClaudeTool] methods will be discovered.
    /// </summary>
    public ClaudeAIBuilder WithTools<TTools>() where TTools : class
    {
        _services.AddSingleton<TTools>();
        _services.AddSingleton(sp =>
        {
            var registry = sp.GetRequiredService<ToolRegistry>();
            var tools = sp.GetRequiredService<TTools>();
            registry.Register(tools);
            return registry;
        });
        return this;
    }

    /// <summary>Returns the service collection for chaining.</summary>
    public IServiceCollection Services => _services;
}
