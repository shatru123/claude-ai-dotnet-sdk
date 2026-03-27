# Claude AI .NET SDK — Integration Guide

A clean, enterprise-ready .NET integration for Anthropic's Claude AI.
Implements the layered architecture from [shatru123/claude-ai-dotnet-sdk](https://github.com/shatru123/claude-ai-dotnet-sdk).

---

## Project Structure

```
ClaudeAI.SDK/
├── Configuration/
│   └── ClaudeOptions.cs          # Strongly-typed config (binds to appsettings.json)
├── Models/
│   └── ClaudeModels.cs           # ClaudeRequest, ClaudeResponse, TokenUsage
├── Core/
│   └── ClaudeService.cs          # Direct Claude API client (HTTP + streaming)
├── Skills/
│   └── ClaudeSkills.cs           # Reusable behaviors: Summarize, CodeExplain, Review…
├── Commands/
│   └── ClaudeCommands.cs         # Intent layer: SummarizeCommand, ReviewCommand…
├── Tools/
│   └── ClaudeTools.cs            # Deterministic tools: Validator, Sanitizer, Formatter
├── Services/
│   └── ClaudeApplicationService.cs  # High-level facade for your app to consume
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # AddClaudeAI() DI registration
└── Example/
    ├── ClaudeController.cs       # ASP.NET Core API controller example
    └── SETUP.cs                  # Setup comments and usage examples
```

---

## Quick Start

### 1. Install (no third-party packages needed)

```xml
<!-- .csproj -->
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

### 2. Configure `appsettings.json`

```json
{
  "Claude": {
    "ApiKey": "sk-ant-YOUR_KEY_HERE",
    "Model": "claude-sonnet-4-6",
    "MaxTokens": 1024,
    "Temperature": 0.7,
    "TimeoutSeconds": 60,
    "MaxRetries": 2,
    "DefaultSystemPrompt": "You are a helpful AI assistant."
  }
}
```

> Get your API key from [console.anthropic.com](https://console.anthropic.com)

### 3. Register in `Program.cs`

```csharp
using ClaudeAI.SDK.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddClaudeAI(builder.Configuration);  // One line!

var app = builder.Build();
app.MapControllers();
app.Run();
```

### 4. Inject and Use

```csharp
public class MyService
{
    private readonly IClaudeApplicationService _claude;

    public MyService(IClaudeApplicationService claude) => _claude = claude;

    public async Task<string> SummarizeReport(string text)
    {
        var result = await _claude.SummarizeAsync(text);
        return result.Success ? result.Content : "Failed";
    }

    public async Task<string> ReviewCode(string code)
    {
        var result = await _claude.ReviewAsync(code, "csharp");
        return result.Content;
    }
}
```

---

## Available Capabilities

| Method | Skill Used | Description |
|---|---|---|
| `SummarizeAsync(text)` | SummarizationSkill | Concise bullet-point summary |
| `ExplainCodeAsync(code, language)` | CodeExplanationSkill | Code walkthrough + risks |
| `GenerateDocumentationAsync(input)` | DocumentationSkill | API / technical docs |
| `ReviewAsync(artifact)` | ReviewSkill | Structured code review |
| `DecisionSupportAsync(context, options)` | DecisionSupportSkill | Pros/cons + recommendation |
| `ChatAsync(message, history)` | Core | Multi-turn conversation |
| `StreamChatAsync(message)` | Core | Server-Sent Events streaming |

---

## REST API Endpoints (via ClaudeController)

```
POST /api/claude/summarize       { "content": "...", "context": "..." }
POST /api/claude/explain-code    { "code": "...", "language": "csharp" }
POST /api/claude/generate-docs   { "input": "...", "docType": "API" }
POST /api/claude/review          { "artifact": "...", "reviewType": "code" }
POST /api/claude/chat            { "message": "...", "history": [] }
GET  /api/claude/stream?message= (SSE streaming)
```

---

## Architecture Layers

```
Controller / App Code
        │
        ▼
IClaudeApplicationService   ← Use this in your business logic
        │
        ├── Commands         ← Intent: what the user wants
        │       └── Skills   ← Behavior: how Claude performs it
        │
        ├── Tools            ← Deterministic: validate / sanitize / format
        │
        └── IClaudeService   ← Core: HTTP calls to Anthropic API
```

---

## Models

| Model | Use Case |
|---|---|
| `claude-sonnet-4-6` | Best balance of speed + intelligence (default) |
| `claude-opus-4-6` | Most powerful, for complex reasoning |
| `claude-haiku-4-5-20251001` | Fastest, for high-throughput/simple tasks |

---

## Security Notes

- **Never commit your API key**. Use `dotnet user-secrets` locally and environment variables in production.
- Store the key in `ANTHROPIC_API_KEY` environment variable, and read it in config: `"ApiKey": ""` with env override.
- Content is sanitized and validated before being sent to Claude.

---

## License

MIT — Free for personal and commercial use.
