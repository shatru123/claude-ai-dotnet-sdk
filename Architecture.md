# Claude AI .NET SDK — Architecture

A clean, enterprise-ready .NET integration for Anthropic's Claude AI API.  
This document describes the design decisions, component responsibilities, and patterns used across the SDK.

---

## Table of Contents

- [High-Level Overview](#high-level-overview)
- [Folder Structure](#folder-structure)
- [Layer Breakdown](#layer-breakdown)
  - [Configuration Layer](#1-configuration-layer)
  - [Models Layer](#2-models-layer)
  - [Core Layer](#3-core-layer)
  - [Skills Layer](#4-skills-layer)
  - [Commands Layer](#5-commands-layer)
  - [Tools Layer](#6-tools-layer)
  - [Services Layer](#7-services-layer)
  - [Extensions Layer](#8-extensions-layer)
- [Data Flow](#data-flow)
- [Available Capabilities](#available-capabilities)
- [REST API Endpoints](#rest-api-endpoints)
- [Configuration Reference](#configuration-reference)
- [Model Selection Guide](#model-selection-guide)
- [Security Considerations](#security-considerations)
- [Extensibility Guide](#extensibility-guide)
- [Quick Start](#quick-start)

---

## High-Level Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Client / UI / API Consumer               │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Controller                         │
│              (ClaudeController.cs — example layer)          │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                 IClaudeApplicationService                   │
│         High-level facade — the single surface your         │
│             application code should depend on               │
└────────┬──────────────────────────────────────┬────────────┘
         │                                      │
         ▼                                      ▼
┌────────────────────┐               ┌──────────────────────┐
│     Commands       │               │        Tools         │
│  (Intent layer)    │               │  (Execution layer)   │
│                    │               │                      │
│ SummarizeCommand   │               │  InputValidatorTool  │
│ ExplainCodeCommand │               │ ContentSanitizerTool │
│ ReviewCommand      │               │  TokenEstimatorTool  │
│ DocumentationCmd   │               │ ResponseFormatter    │
│ DecisionSupportCmd │               └──────────────────────┘
└────────┬───────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│                        Skills                               │
│                   (Behaviour layer)                         │
│                                                             │
│   SummarizationSkill  │  CodeExplanationSkill              │
│   DocumentationSkill  │  ReviewSkill                       │
│   DecisionSupportSkill                                      │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                    IClaudeService                           │
│               (Core layer — HTTP client)                    │
│         Typed HttpClient → Anthropic Messages API           │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │   Anthropic Claude   │
                    │   AI  (REST API)     │
                    └──────────────────────┘
```

---

## Folder Structure

```
claude-ai-dotnet-sdk/
│
├── src/
│   └── ClaudeAI.SDK/
│       ├── Configuration/
│       │   └── ClaudeOptions.cs            # Strongly-typed settings
│       │
│       ├── Models/
│       │   └── ClaudeModels.cs             # Request, Response, TokenUsage, ConversationMessage
│       │
│       ├── Core/
│       │   └── ClaudeService.cs            # Direct Anthropic API client (HTTP + streaming)
│       │
│       ├── Skills/
│       │   └── ClaudeSkills.cs             # Reusable AI behaviour units
│       │
│       ├── Commands/
│       │   └── ClaudeCommands.cs           # Intent-driven orchestration
│       │
│       ├── Tools/
│       │   └── ClaudeTools.cs              # Deterministic utilities
│       │
│       ├── Services/
│       │   └── ClaudeApplicationService.cs # High-level application facade
│       │
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs  # DI registration
│
├── example/
│   ├── Controllers/
│   │   └── ClaudeController.cs             # ASP.NET Core Web API example
│   ├── DTOs/
│   │   └── RequestDTOs.cs                  # Request/Response data contracts
│   ├── Program.cs                          # Wiring example
│   └── appsettings.json                    # Configuration template
│
└── ARCHITECTURE.md                         # This file
```

---

## Layer Breakdown

### 1. Configuration Layer

**File:** `Configuration/ClaudeOptions.cs`

Provides strongly-typed configuration that binds directly from `appsettings.json` under the `"Claude"` key.

| Property | Default | Description |
|---|---|---|
| `ApiKey` | _(required)_ | Anthropic API key from console.anthropic.com |
| `Model` | `claude-sonnet-4-6` | Model identifier |
| `MaxTokens` | `1024` | Max tokens in response |
| `Temperature` | `0.7` | Creativity (0.0–1.0) |
| `DefaultSystemPrompt` | Enterprise prompt | Applied to all requests unless overridden |
| `TimeoutSeconds` | `60` | HTTP timeout |
| `MaxRetries` | `2` | Retries on transient failure |

**Design rationale:** Using `IOptions<ClaudeOptions>` decouples configuration access from the source (file, environment variable, secrets manager) and makes it easily testable.

---

### 2. Models Layer

**File:** `Models/ClaudeModels.cs`

Shared data contracts used across all layers. No logic lives here.

| Type | Purpose |
|---|---|
| `ClaudeRequest` | Input to the integration layer — message, history, context, optional system prompt |
| `ClaudeResponse` | Output — content, success flag, error, token usage, model name |
| `ConversationMessage` | Single turn in a multi-turn conversation (`role` + `content`) |
| `TokenUsage` | Input/output token counts returned by the API |

---

### 3. Core Layer

**File:** `Core/ClaudeService.cs`

The only layer that communicates with the Anthropic REST API. Uses a typed `HttpClient` registered via `AddHttpClient<IClaudeService, ClaudeService>()`.

**Responsibilities:**
- Construct the JSON payload from `ClaudeRequest`
- Inject the API key and anthropic-version header
- Handle non-streaming (`POST /v1/messages`) responses
- Handle streaming (`stream: true`) via Server-Sent Events using `IAsyncEnumerable<string>`
- Log token usage at `Debug` level
- Return structured `ClaudeResponse` on both success and failure

**Interfaces exposed:** `IClaudeService` with two methods:

```csharp
Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken ct);
IAsyncEnumerable<string> StreamAsync(ClaudeRequest request, CancellationToken ct);
```

---

### 4. Skills Layer

**File:** `Skills/ClaudeSkills.cs`

Skills define **how Claude performs a task** through a carefully engineered system prompt. They are reusable across multiple commands and contain no business logic.

Each skill extends `ClaudeSkillBase` and only needs to provide a `SystemPrompt` string.

| Skill | System Prompt Focus |
|---|---|
| `SummarizationSkill` | Concise bullet-point summaries under 200 words |
| `CodeExplanationSkill` | Overview, How it Works, Key Points, Potential Issues |
| `DocumentationSkill` | Purpose, Parameters, Return Values, Examples, Remarks |
| `ReviewSkill` | Summary, Issues (Critical/Major/Minor), Suggestions |
| `DecisionSupportSkill` | Context, Options (pros/cons), Recommendation, Risks |

**Adding a new skill:**

```csharp
public sealed class TranslationSkill : ClaudeSkillBase
{
    public TranslationSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "Translation";
    protected override string SystemPrompt =>
        "You are a professional translator. Translate the provided text accurately, " +
        "preserving tone and meaning. Specify the target language in your input.";
}
```

---

### 5. Commands Layer

**File:** `Commands/ClaudeCommands.cs`

Commands represent **what the user wants to achieve**. They are intent-driven, contain no AI logic, and orchestrate one or more skills.

Each command implements `IClaudeCommand<ClaudeResponse>`.

| Command | Skill Used | Intent |
|---|---|---|
| `SummarizeCommand` | SummarizationSkill | Condense content |
| `ExplainCodeCommand` | CodeExplanationSkill | Understand code |
| `GenerateDocumentationCommand` | DocumentationSkill | Write docs |
| `ReviewCommand` | ReviewSkill | Evaluate quality |
| `DecisionSupportCommand` | DecisionSupportSkill | Analyse options |

**Design rationale:** Separating commands from skills means the same skill can be used by multiple commands, and the same command API can swap underlying skills without breaking callers.

---

### 6. Tools Layer

**File:** `Tools/ClaudeTools.cs`

Tools are **deterministic** — they do not call Claude. They pre-process inputs and post-process outputs.

| Tool | Direction | Purpose |
|---|---|---|
| `InputValidatorTool` | Pre-process | Reject empty or oversized inputs |
| `ContentSanitizerTool` | Pre-process | Normalise whitespace, truncate if needed |
| `TokenEstimatorTool` | Pre-process | Estimate tokens (~4 chars/token) before calling API |
| `ResponseFormatterTool` | Post-process | Convert Claude's markdown to PlainText, HTML, or JSON |

**Design rationale:** Keeping deterministic logic in tools (not in skills) makes them independently testable without any API calls.

---

### 7. Services Layer

**File:** `Services/ClaudeApplicationService.cs`

The **high-level facade** that application code should depend on. It:

1. Validates and sanitizes input via Tools
2. Builds and executes the appropriate Command (which delegates to a Skill)
3. Returns a `ClaudeResponse` to the caller
4. Catches and logs all exceptions — callers never see unhandled exceptions

```csharp
public interface IClaudeApplicationService
{
    Task<ClaudeResponse> SummarizeAsync(string content, string? context, CancellationToken ct);
    Task<ClaudeResponse> ExplainCodeAsync(string code, string? language, CancellationToken ct);
    Task<ClaudeResponse> GenerateDocumentationAsync(string input, string docType, CancellationToken ct);
    Task<ClaudeResponse> ReviewAsync(string artifact, string reviewType, CancellationToken ct);
    Task<ClaudeResponse> DecisionSupportAsync(string context, params string[] options);
    Task<ClaudeResponse> ChatAsync(string message, List<ConversationMessage>? history, CancellationToken ct);
    IAsyncEnumerable<string> StreamChatAsync(string message, CancellationToken ct);
}
```

---

### 8. Extensions Layer

**File:** `Extensions/ServiceCollectionExtensions.cs`

Provides `AddClaudeAI(IConfiguration)` — a single call that registers the entire SDK into the .NET DI container.

```csharp
// Program.cs
builder.Services.AddClaudeAI(builder.Configuration);
```

**Lifetime decisions:**

| Component | Lifetime | Reason |
|---|---|---|
| `IClaudeService` | Transient (via `AddHttpClient`) | HttpClient managed by IHttpClientFactory |
| Skills | Scoped | One per request, no shared state |
| Tools | Singleton | Stateless, safe to share across requests |
| `IClaudeApplicationService` | Scoped | Depends on scoped skills |

---

## Data Flow

### Non-Streaming Request

```
Controller
  → IClaudeApplicationService.SummarizeAsync(text)
      → InputValidatorTool.ExecuteAsync(text)        [validate]
      → ContentSanitizerTool.ExecuteAsync(text)      [sanitize]
      → SummarizeCommand.ExecuteAsync()
          → SummarizationSkill.ExecuteAsync(prompt)
              → IClaudeService.SendAsync(ClaudeRequest)
                  → POST https://api.anthropic.com/v1/messages
                  ← ClaudeApiResponse (JSON)
              ← ClaudeResponse { Content, Usage, Model }
          ← ClaudeResponse
      ← ClaudeResponse
  ← 200 OK { content, usage, model }
```

### Streaming Request

```
Controller (GET /api/claude/stream)
  → IClaudeApplicationService.StreamChatAsync(message)
      → IClaudeService.StreamAsync(ClaudeRequest)
          → POST /v1/messages  { stream: true }
          ← SSE: data: {"delta":{"text":"Hello"}}
          ← SSE: data: {"delta":{"text":" world"}}
          ← SSE: data: [DONE]
      ← IAsyncEnumerable<string>
  → Response.WriteAsync("data: Hello\n\n")
  → Response.WriteAsync("data:  world\n\n")
```

---

## Available Capabilities

| Method | Endpoint | Description |
|---|---|---|
| `SummarizeAsync` | `POST /api/claude/summarize` | Bullet-point summary of any text |
| `ExplainCodeAsync` | `POST /api/claude/explain-code` | Code walkthrough with overview and risks |
| `GenerateDocumentationAsync` | `POST /api/claude/generate-docs` | API or technical documentation |
| `ReviewAsync` | `POST /api/claude/review` | Structured code review (Critical/Major/Minor) |
| `DecisionSupportAsync` | `POST /api/claude/decision` | Options analysis with recommendation |
| `ChatAsync` | `POST /api/claude/chat` | Multi-turn conversation with history |
| `StreamChatAsync` | `GET /api/claude/stream` | Server-Sent Events streaming response |

---

## REST API Endpoints

### POST `/api/claude/summarize`
```json
{ "content": "Long document text...", "context": "Q4 Financial Report" }
```

### POST `/api/claude/explain-code`
```json
{ "code": "public void Foo() { ... }", "language": "csharp" }
```

### POST `/api/claude/generate-docs`
```json
{ "input": "public Task<T> SendAsync(...)", "docType": "API" }
```

### POST `/api/claude/review`
```json
{ "artifact": "public class PaymentService { ... }", "reviewType": "code" }
```

### POST `/api/claude/decision`
```json
{
  "context": "We need to choose a message broker for our microservices.",
  "options": ["RabbitMQ", "Azure Service Bus", "Kafka"]
}
```

### POST `/api/claude/chat`
```json
{
  "message": "What are the tradeoffs between REST and gRPC?",
  "history": [
    { "role": "user", "content": "Let's discuss API design." },
    { "role": "assistant", "content": "Happy to! What aspect interests you?" }
  ]
}
```

### GET `/api/claude/stream?message=Explain+CQRS`
Returns `text/event-stream` (Server-Sent Events).

---

## Configuration Reference

```json
{
  "Claude": {
    "ApiKey": "sk-ant-YOUR_KEY_HERE",
    "Model": "claude-sonnet-4-6",
    "MaxTokens": 1024,
    "Temperature": 0.7,
    "TimeoutSeconds": 60,
    "MaxRetries": 2,
    "DefaultSystemPrompt": "You are a helpful AI assistant..."
  }
}
```

> **Never commit your API key.** Use `dotnet user-secrets` locally and environment variables in CI/CD:
> ```
> CLAUDE__APIKEY=sk-ant-...
> ```

---

## Model Selection Guide

| Model | Speed | Intelligence | Best For |
|---|---|---|---|
| `claude-haiku-4-5-20251001` | Fastest | Good | High-volume, simple tasks, classification |
| `claude-sonnet-4-6` _(default)_ | Balanced | Excellent | General enterprise use, code, analysis |
| `claude-opus-4-6` | Slowest | Best | Complex reasoning, long documents, research |

---

## Security Considerations

- **API key isolation:** The key is read once in `ClaudeService` and added to headers. It is never logged or returned to callers.
- **Input validation:** All inputs are validated (min/max length) before reaching Claude.
- **Content sanitization:** Inputs are normalized and optionally truncated to prevent oversized payloads.
- **No prompt injection surface:** System prompts are constructed by code (skills), not user input. User input only populates the `user` message role.
- **Controlled tool access:** Tools are deterministic and have no network access — they cannot escalate privileges or call external services.
- **No hardcoded secrets:** Configuration is entirely injected via `IOptions<ClaudeOptions>`.

---

## Extensibility Guide

### Add a new Skill

```csharp
// 1. Create the skill
public sealed class TranslationSkill : ClaudeSkillBase
{
    public TranslationSkill(IClaudeService s) : base(s) { }
    public override string SkillName => "Translation";
    protected override string SystemPrompt => "Translate accurately, preserving tone.";
}

// 2. Register in ServiceCollectionExtensions.cs
services.AddScoped<TranslationSkill>();

// 3. Inject and use anywhere
```

### Add a new Command

```csharp
public sealed class TranslateCommand : IClaudeCommand<ClaudeResponse>
{
    private readonly TranslationSkill _skill;
    private readonly string _text;
    private readonly string _targetLanguage;

    public TranslateCommand(TranslationSkill skill, string text, string targetLanguage)
    { _skill = skill; _text = text; _targetLanguage = targetLanguage; }

    public Task<ClaudeResponse> ExecuteAsync(CancellationToken ct = default)
        => _skill.ExecuteAsync($"Translate to {_targetLanguage}:\n\n{_text}", ct: ct);
}
```

### Add a new Tool

```csharp
public sealed class LanguageDetectorTool : IClaudeTool<string, string>
{
    public string ToolName => "LanguageDetector";

    public Task<string> ExecuteAsync(string input, CancellationToken ct = default)
    {
        // Deterministic language detection logic
        var isCode = input.Contains('{') && input.Contains('}');
        return Task.FromResult(isCode ? "code" : "natural-language");
    }
}
```

---

## Quick Start

**1. Reference the SDK files in your .NET 8+ project.**

**2. Add NuGet dependencies:**
```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

**3. Configure `appsettings.json`:**
```json
{
  "Claude": {
    "ApiKey": "sk-ant-YOUR_KEY",
    "Model": "claude-sonnet-4-6"
  }
}
```

**4. Register in `Program.cs`:**
```csharp
builder.Services.AddClaudeAI(builder.Configuration);
```

**5. Inject `IClaudeApplicationService` wherever you need it:**
```csharp
public class ReportService
{
    private readonly IClaudeApplicationService _claude;

    public ReportService(IClaudeApplicationService claude) => _claude = claude;

    public async Task<string> SummarizeReport(string reportText)
    {
        var result = await _claude.SummarizeAsync(reportText, context: "Monthly Sales Report");
        return result.Success ? result.Content : "Summarization failed.";
    }
}
```

---

## License

MIT — Free for personal and commercial use.

---

_Built for [CompileCare](https://github.com/CompileCare) · Powered by [Anthropic Claude](https://www.anthropic.com)_
