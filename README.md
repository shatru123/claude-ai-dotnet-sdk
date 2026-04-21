# 🤖 ClaudeAI.DotNet SDK

> **Enterprise-grade .NET 8 SDK for Anthropic's Claude AI — with Skills, Streaming, Function Calling & fluent DI.**

[![NuGet](https://img.shields.io/nuget/v/ClaudeAI.DotNet?color=blue&logo=nuget&label=NuGet)](https://www.nuget.org/packages/ClaudeAI.DotNet)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ClaudeAI.DotNet?color=green)](https://www.nuget.org/packages/ClaudeAI.DotNet)
[![Build](https://img.shields.io/github/actions/workflow/status/shatru123/claude-ai-dotnet-sdk/ci-cd.yml?label=CI)](https://github.com/shatru123/claude-ai-dotnet-sdk/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com)
[![Stars](https://img.shields.io/github/stars/shatru123/claude-ai-dotnet-sdk?style=social)](https://github.com/shatru123/claude-ai-dotnet-sdk/stargazers)

---

## ✨ Why ClaudeAI.DotNet?

Claude is one of the most capable AI models — but using it in .NET meant wiring raw HttpClients, serializing JSON by hand, and zero structure. **ClaudeAI.DotNet** brings Claude into the .NET ecosystem as a first-class citizen with clean architecture, fluent DI, and built-in skills.

```csharp
// One line setup
builder.Services.AddClaudeAI(opt => opt.ApiKey = config["Claude:ApiKey"])
    .WithSkill<SummarizationSkill>()
    .WithSkill<CodeReviewSkill>();

// Simple usage
var summary = await _claude.With()
    .Skill<SummarizationSkill>()
    .SendAsync("Your long document here...");
```

---

## 📦 Installation

```bash
dotnet add package ClaudeAI.DotNet
```

---

## 🚀 Quick Start

### 1. Configure in `Program.cs`

```csharp
builder.Services.AddClaudeAI(options =>
{
    options.ApiKey = builder.Configuration["Claude:ApiKey"];
    options.Model = ClaudeModel.Sonnet;     // or ClaudeModel.Opus
    options.MaxTokens = 4096;
    options.SystemPrompt = "You are a helpful assistant.";
});
```

Or use `appsettings.json`:

```json
{
  "Claude": {
    "ApiKey": "your-api-key-here",
    "Model": "claude-sonnet-4-5",
    "MaxTokens": 4096
  }
}
```

```csharp
builder.Services.AddClaudeAI(builder.Configuration);
```

### 2. Inject and Use

```csharp
public class MyService
{
    private readonly IClaudeClient _claude;

    public MyService(IClaudeClient claude) => _claude = claude;

    public async Task<string> AskAsync(string question)
        => await _claude.SendAsync(question);
}
```

---

## 🎯 Features

### 💬 Simple Send

```csharp
var response = await _claude.SendAsync("Explain dependency injection in simple terms.");
```

### 🧠 Built-in Skills

```csharp
// Summarization
var summary = await _claude.SendWithSkillAsync(longText, new SummarizationSkill());

// Code Review
var review = await _claude.SendWithSkillAsync(myCode, new CodeReviewSkill());

// Translation
var french = await _claude.SendWithSkillAsync(text, new TranslationSkill("French"));

// Sentiment Analysis
var sentiment = await _claude.SendWithSkillAsync(review, new SentimentAnalysisSkill());
```

### 🔄 Fluent Builder

```csharp
var result = await _claude
    .With()
    .Skill<SummarizationSkill>()
    .SendAsync("Your content here...");
```

### 📡 Streaming

```csharp
await foreach (var chunk in _claude.StreamAsync("Write a poem about .NET"))
{
    if (!chunk.IsFinal)
        Console.Write(chunk.Delta);
}
```

### 💬 Multi-turn Conversations

```csharp
var history = new List<ClaudeMessage>
{
    ClaudeMessage.User("My name is Shatrughna"),
    ClaudeMessage.Assistant("Nice to meet you, Shatrughna!"),
    ClaudeMessage.User("What's my name?")
};

var reply = await _claude.ChatAsync(history);
// → "Your name is Shatrughna."
```

### ⚡ CQRS Command Pattern

```csharp
var result = await _claude.ExecuteAsync(new ClaudeCommand
{
    Prompt = "Analyze this customer feedback...",
    Skill = new SentimentAnalysisSkill(),
    MaxTokensOverride = 512
});

if (result.IsSuccess)
{
    Console.WriteLine(result.Content);
    Console.WriteLine($"Tokens used: {result.Usage?.TotalTokens}");
}
```

### 🔧 Function Calling (Tool Use)

```csharp
public class MyTools
{
    [ClaudeTool("get_weather", "Gets the current weather for a given city")]
    public async Task<string> GetWeatherAsync(string city)
    {
        return $"It's sunny in {city}, 28°C";
    }
}

// Register tools
builder.Services.AddClaudeAI(opt => opt.ApiKey = "...")
    .WithTools<MyTools>();
```

---

## 🏗️ Architecture

```
ClaudeAI.DotNet/
├── Configuration/     ← ClaudeOptions, ClaudeModel constants
├── Models/            ← Request, Response, Message, StreamChunk DTOs
├── Core/              ← HTTP pipeline, retry logic, streaming
├── Skills/            ← ISkill, BaseSkill, built-in skill library
├── Commands/          ← ClaudeCommand, ClaudeCommandResult (CQRS)
├── Tools/             ← [ClaudeTool] attribute, ToolRegistry
├── Services/          ← IClaudeClient, ClaudeClient, RequestBuilder
└── Extensions/        ← AddClaudeAI(), ClaudeAIBuilder (DI)
```

---

## 🧩 Built-in Skills

| Skill | Description |
|-------|-------------|
| `SummarizationSkill` | Concise, structured summaries |
| `CodeReviewSkill` | Detailed code review with ratings |
| `TranslationSkill` | Translate to any target language |
| `SentimentAnalysisSkill` | Returns structured JSON sentiment |
| `DocumentationSkill` | XML doc comments generation |

### Building Custom Skills

```csharp
public class MyCustomSkill : BaseSkill
{
    public override string Name => "MySkill";

    public override string GetSystemPrompt() =>
        "You are an expert in my specific domain...";

    public override string TransformPrompt(string userPrompt) =>
        $"Process this with my domain knowledge: {userPrompt}";
}

// Register it
builder.Services.AddClaudeAI(opt => ...)
    .WithSkill<MyCustomSkill>();
```

---

## ⚙️ Configuration Reference

| Option | Default | Description |
|--------|---------|-------------|
| `ApiKey` | Required | Your Anthropic API key |
| `Model` | `claude-sonnet-4-5` | Model to use |
| `MaxTokens` | `4096` | Max response tokens |
| `Temperature` | `0.7` | Creativity (0.0–1.0) |
| `MaxRetries` | `3` | Retry attempts on failure |
| `TimeoutSeconds` | `120` | HTTP timeout |
| `SystemPrompt` | `null` | Default system prompt |

---

## 🤝 Contributing

```bash
git clone https://github.com/shatru123/claude-ai-dotnet-sdk
cd claude-ai-dotnet-sdk
dotnet restore
dotnet test
```

PRs welcome! Please open an issue first for major changes.

---

## 📄 License

MIT License — free for personal and commercial use.

---

## ⭐ Support the Project

If ClaudeAI.DotNet saves you time, **please give it a star** — it helps the project reach more .NET developers!

[![GitHub stars](https://img.shields.io/github/stars/shatru123/claude-ai-dotnet-sdk?style=for-the-badge)](https://github.com/shatru123/claude-ai-dotnet-sdk/stargazers)

---

Built with ❤️ by [Shatrughna](https://github.com/shatru123) | Pune, India 🇮🇳
