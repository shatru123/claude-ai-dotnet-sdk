using ClaudeAI.DotNet.Commands;
using ClaudeAI.DotNet.Extensions;
using ClaudeAI.DotNet.Models;
using ClaudeAI.DotNet.Services;
using ClaudeAI.DotNet.Skills;
using ClaudeAI.DotNet.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ============================================================
//  ClaudeAI.DotNet — Sample Console Application
//  Demonstrates all major SDK features
// ============================================================

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║     ClaudeAI.DotNet SDK Samples      ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();

// ── Setup DI ─────────────────────────────────────────────────
var services = new ServiceCollection();

services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));

services.AddClaudeAI(options =>
{
    // Set your API key here or via environment variable
    options.ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
        ?? throw new InvalidOperationException(
            "Please set the ANTHROPIC_API_KEY environment variable.");
    options.MaxTokens = 1024;
    options.SystemPrompt = "You are a helpful, concise assistant. Keep responses brief.";
})
.WithSkill<SummarizationSkill>()
.WithSkill<CodeReviewSkill>()
.WithSkill<SentimentAnalysisSkill>()
.WithTools<WeatherTools>();

var provider = services.BuildServiceProvider();
var claude = provider.GetRequiredService<IClaudeClient>();

// ── Sample 1: Simple Send ─────────────────────────────────────
await RunSampleAsync("1. Simple Send", async () =>
{
    var response = await claude.SendAsync(
        "What are the top 3 benefits of using dependency injection in .NET?");
    Console.WriteLine(response);
});

// ── Sample 2: Summarization Skill ────────────────────────────
await RunSampleAsync("2. Summarization Skill", async () =>
{
    var longText = """
        Dependency injection (DI) is a software design pattern that implements 
        Inversion of Control (IoC) for resolving dependencies. In .NET, the built-in 
        DI container (Microsoft.Extensions.DependencyInjection) allows developers to 
        register services and their lifetimes (transient, scoped, singleton), then 
        have them automatically injected into constructors. This promotes loose coupling, 
        testability, and maintainability. ASP.NET Core uses DI extensively — controllers, 
        middleware, services, and even the framework itself are wired through the DI 
        container. Popular third-party containers like Autofac, Ninject, and Castle 
        Windsor offer additional features like property injection, decorators, 
        and convention-based registration.
        """;

    var summary = await claude.SendWithSkillAsync(longText, new SummarizationSkill());
    Console.WriteLine(summary);
});

// ── Sample 3: Code Review Skill ──────────────────────────────
await RunSampleAsync("3. Code Review Skill", async () =>
{
    var code = """
        public string GetUser(int id) {
            var conn = new SqlConnection("Server=.;Database=MyDb;");
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Users WHERE Id=" + id, conn);
            var reader = cmd.ExecuteReader();
            reader.Read();
            return reader["Name"].ToString();
        }
        """;

    var review = await claude.SendWithSkillAsync(code, new CodeReviewSkill());
    Console.WriteLine(review);
});

// ── Sample 4: Fluent Builder ──────────────────────────────────
await RunSampleAsync("4. Fluent Builder with Skill", async () =>
{
    var result = await claude
        .With()
        .Skill<SummarizationSkill>()
        .SendAsync("The .NET ecosystem includes frameworks like ASP.NET Core, MAUI, " +
                   "Blazor, WPF, and WinForms covering web, mobile, desktop, and cloud workloads.");
    Console.WriteLine(result);
});

// ── Sample 5: Streaming ───────────────────────────────────────
await RunSampleAsync("5. Streaming Response", async () =>
{
    Console.Write("Claude says: ");
    await foreach (var chunk in claude.StreamAsync("Write a one-sentence tagline for a .NET AI SDK."))
    {
        if (!chunk.IsFinal)
            Console.Write(chunk.Delta);
    }
    Console.WriteLine();
});

// ── Sample 6: Multi-turn Chat ─────────────────────────────────
await RunSampleAsync("6. Multi-turn Conversation", async () =>
{
    var history = new List<ClaudeMessage>
    {
        ClaudeMessage.User("My favorite language is C#."),
        ClaudeMessage.Assistant("Great choice! C# is a powerful, elegant language."),
        ClaudeMessage.User("What's my favorite language?")
    };

    var reply = await claude.ChatAsync(history);
    Console.WriteLine(reply);
});

// ── Sample 7: CQRS Command Pattern ───────────────────────────
await RunSampleAsync("7. CQRS Command Execution", async () =>
{
    var command = new ClaudeCommand
    {
        Prompt = "The new restaurant had amazing food and friendly staff. Will definitely go back!",
        Skill = new SentimentAnalysisSkill(),
        MaxTokensOverride = 256
    };

    var result = await claude.ExecuteAsync(command);

    if (result.IsSuccess)
    {
        Console.WriteLine($"Skill Used : {result.SkillUsed}");
        Console.WriteLine($"Tokens Used: {result.Usage?.TotalTokens}");
        Console.WriteLine($"Response   : {result.Content}");
    }
    else
    {
        Console.WriteLine($"Error: {result.ErrorMessage}");
    }
});

// ── Sample 8: Custom Skill ────────────────────────────────────
await RunSampleAsync("8. Custom Skill", async () =>
{
    var haiku = await claude.SendWithSkillAsync(
        "a developer debugging at midnight",
        new HaikuSkill());
    Console.WriteLine(haiku);
});

Console.WriteLine();
Console.WriteLine("✅ All samples completed!");

// ── Helper ───────────────────────────────────────────────────
static async Task RunSampleAsync(string title, Func<Task> action)
{
    Console.WriteLine($"┌─ {title} {'─'.ToString().PadRight(40 - title.Length, '─')}");
    try { await action(); }
    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    Console.WriteLine();
}

// ── Custom Skill Example ─────────────────────────────────────
public class HaikuSkill : BaseSkill
{
    public override string Name => "Haiku";

    public override string GetSystemPrompt() =>
        """
        You are a haiku poet. Write only in haiku format (5-7-5 syllables).
        No explanation, no preamble — just the haiku.
        """;

    public override string TransformPrompt(string prompt) =>
        $"Write a haiku about: {prompt}";
}

// ── Tool Example ─────────────────────────────────────────────
public class WeatherTools
{
    [ClaudeTool("get_weather", "Returns current weather for a city")]
    public Task<string> GetWeatherAsync(string city)
    {
        // Simulated - replace with real weather API
        return Task.FromResult($"Sunny, 28°C in {city}");
    }

    [ClaudeTool("get_forecast", "Returns 3-day weather forecast for a city")]
    public Task<string> GetForecastAsync(string city)
    {
        return Task.FromResult($"Forecast for {city}: Mon 28°C, Tue 26°C, Wed 30°C");
    }
}
