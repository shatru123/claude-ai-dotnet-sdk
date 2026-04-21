# 📬 Newsletter Cold Outreach — ClaudeAI.DotNet SDK

---

## 1. .NET Weekly (dotnetweekly.com)

**To:** editor@dotnetweekly.com  
**Subject:** Open-source submission: ClaudeAI.DotNet — Enterprise .NET 8 SDK for Claude AI

Hi there,

I'd love to submit an open-source project for consideration in .NET Weekly.

**ClaudeAI.DotNet** is a .NET 8 SDK for integrating Anthropic's Claude AI into enterprise applications. It fills a real gap — developers using Claude had to hand-roll HTTP clients with no structure. This SDK brings Claude into the .NET ecosystem properly.

**What it offers:**
- Fluent `AddClaudeAI()` DI registration for ASP.NET Core
- Built-in Skills system (Summarization, Code Review, Translation, Sentiment Analysis)
- Streaming via `IAsyncEnumerable<StreamChunk>`
- Function Calling with `[ClaudeTool]` attribute
- CQRS command pattern with structured results
- CI/CD with automated NuGet publishing on git tags

```csharp
builder.Services.AddClaudeAI(opt => opt.ApiKey = "...")
    .WithSkill<SummarizationSkill>()
    .WithStreaming();

var summary = await _claude.With()
    .Skill<SummarizationSkill>()
    .SendAsync("Long document here...");
```

🔗 GitHub: https://github.com/shatru123/claude-ai-dotnet-sdk

Happy to provide any additional information. Thanks for the great newsletter!

Best,  
Shatrughna  
github.com/shatru123

---

## 2. C# Digest (csharpdigest.net)

**To:** submit@csharpdigest.net  
**Subject:** Link submission — ClaudeAI.DotNet: Open-source SDK for Claude AI in .NET 8

Hi,

I'd like to submit a link for C# Digest.

**ClaudeAI.DotNet** — https://github.com/shatru123/claude-ai-dotnet-sdk

A clean, enterprise-grade .NET 8 SDK for Anthropic's Claude AI, featuring fluent DI registration, a built-in Skills system, streaming support, function calling with `[ClaudeTool]` attribute, and a CQRS command pattern. Built with clean architecture and shipped with full unit tests and CI/CD.

It's the missing Claude SDK for .NET developers — install via `dotnet add package ClaudeAI.DotNet`.

Thanks for your consideration!

Shatrughna 

---

## 3. The Morning Brew / Console.NET

**To:** links@themorningbrew.net  
**Subject:** Open Source Link — ClaudeAI.DotNet SDK

Hi,

Quick submission for The Morning Brew: an open-source .NET 8 SDK for Claude AI that brings enterprise-grade DI integration, built-in AI skills, streaming, and function calling to .NET developers.

🔗 https://github.com/shatru123/claude-ai-dotnet-sdk

It ships with a samples project, full test suite, and automated NuGet publishing via GitHub Actions.

Cheers,  
Shatrughna

---

## 4. Dev.to — Article Pitch (Editor / Tag Moderator)

**To:** (Post as article directly, no email needed)  
**Title:** I built the missing Claude AI SDK for .NET 8 — here's how it works

**Intro paragraph:**

Every major AI provider has a proper .NET SDK — except Claude. Developers integrating Anthropic's Claude into ASP.NET Core had to write raw HttpClient code, manage retries manually, and figure out streaming from scratch. So I built **ClaudeAI.DotNet** — a clean, production-ready .NET 8 SDK that makes Claude feel like a first-class citizen in the .NET ecosystem. Here's what it does and how it works.

*(Continue with README content, code samples, and architecture diagram)*

**Tags:** `dotnet` `csharp` `ai` `opensource`

---

## 5. Reddit — r/dotnet + r/csharp Posts

### r/dotnet

**Title:** I built ClaudeAI.DotNet — an open-source .NET 8 SDK for Anthropic's Claude AI (Skills, Streaming, Function Calling, DI)

Hey r/dotnet,

There was no proper .NET SDK for Claude AI — just raw HttpClient wrappers floating around. So I built one with clean architecture in mind.

**ClaudeAI.DotNet** gives you:
- `AddClaudeAI()` fluent DI extension (one-line setup)
- Built-in Skills: Summarization, Code Review, Translation, Sentiment Analysis
- Streaming via `IAsyncEnumerable`
- Function Calling with `[ClaudeTool]` attribute
- CQRS `ClaudeCommand` pattern
- Full unit test suite + CI/CD + auto NuGet publish

```csharp
// Setup
builder.Services.AddClaudeAI(opt => opt.ApiKey = "...")
    .WithSkill<CodeReviewSkill>();

// Usage
var review = await _claude.SendWithSkillAsync(myCode, new CodeReviewSkill());
```

🔗 https://github.com/shatru123/claude-ai-dotnet-sdk

Any feedback on the architecture appreciated — especially around the Skills abstraction and tool binding. PRs very welcome!

---

### r/csharp  

**Title:** Open-source: ClaudeAI.DotNet — clean .NET 8 SDK for Claude AI with DI, Skills, and Streaming

Hi r/csharp,

Sharing an SDK I built for integrating Claude AI into .NET 8 / ASP.NET Core apps. Claude is one of the most capable models but had no proper .NET SDK — this fills that gap.

Key features:
- Fluent DI: `AddClaudeAI().WithSkill<T>().WithTools<T>()`
- Built-in skills for common AI tasks
- Real streaming support via `IAsyncEnumerable<StreamChunk>`
- `[ClaudeTool]` attribute for function calling
- CQRS-friendly command pattern
- xUnit tests, GitHub Actions CI, auto NuGet publish on tags

`dotnet add package ClaudeAI.DotNet`

🔗 https://github.com/shatru123/claude-ai-dotnet-sdk

⭐ Stars help the project reach more devs — thanks!

---

## 6. Hacker News (Show HN)

**Title:** Show HN: ClaudeAI.DotNet — Open-source .NET 8 SDK for Anthropic's Claude API

Hi HN,

I built **ClaudeAI.DotNet** because there was no proper .NET SDK for Claude AI — just raw HTTP wrappers.

The SDK has an 8-layer clean architecture:
- Configuration → Models → Core → Skills → Commands → Tools → Services → Extensions

Key capabilities:
- `AddClaudeAI()` for ASP.NET Core DI with fluent builder
- Built-in Skills system — plug-and-play AI task modules
- Streaming via `IAsyncEnumerable` with SSE parsing
- Function calling via `[ClaudeTool]` method attribute + `ToolRegistry`
- CQRS command dispatch with structured success/failure results
- Automatic retry with exponential backoff

I'd particularly love feedback on the Skills abstraction — it's the most novel part of the design. Happy to answer architecture questions.

🔗 https://github.com/shatru123/claude-ai-dotnet-sdk

---

## 7. Awesome-dotnet PR (GitHub)

**Repo:** https://github.com/quozd/awesome-dotnet  
**PR Title:** Add ClaudeAI.DotNet to AI section

**PR Description:**

Adding **ClaudeAI.DotNet** to the AI/Machine Learning section.

**ClaudeAI.DotNet** - Enterprise-grade .NET 8 SDK for Anthropic's Claude AI with Skills, Streaming, Function Calling, and fluent ASP.NET Core DI registration.

- ⭐ GitHub: https://github.com/shatru123/claude-ai-dotnet-sdk
- 📦 NuGet: https://www.nuget.org/packages/ClaudeAI.DotNet
- 📄 License: MIT
- 🏗️ Target: .NET 8

Follows the repo's format. Happy to adjust if needed.
