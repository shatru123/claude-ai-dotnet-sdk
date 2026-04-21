# Changelog

All notable changes to **ClaudeAI.DotNet** are documented here.

This project follows [Semantic Versioning](https://semver.org/) and [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [1.0.0] — 2026-04-21

### 🎉 Initial Release

This is the first stable release of ClaudeAI.DotNet — an enterprise-grade .NET 8 SDK for Anthropic's Claude AI.

### Added

**Core**
- `IClaudeClient` and `ClaudeClient` — high-level client with full async support
- `IClaudeHttpClient` and `ClaudeHttpClient` — low-level HTTP pipeline with automatic retry logic
- Exponential backoff retry strategy (configurable via `MaxRetries`)
- Full cancellation token support throughout

**Skills System**
- `ISkill` interface and `BaseSkill` base class for building custom skills
- `SummarizationSkill` — structured, concise text summarization
- `CodeReviewSkill` — detailed code review with ratings and suggestions
- `TranslationSkill` — translate to any target language
- `SentimentAnalysisSkill` — structured JSON sentiment output
- `DocumentationSkill` — XML doc comment generation for .NET

**Streaming**
- `StreamAsync()` — real-time token streaming via `IAsyncEnumerable<StreamChunk>`
- Server-Sent Events (SSE) parsing for Anthropic's streaming API

**Commands (CQRS)**
- `ClaudeCommand` — intent-based command with skill, model, and token overrides
- `ClaudeCommandResult` — structured result with success/failure, usage stats, and skill info
- `IClaudeClient.ExecuteAsync()` — safe command execution with exception handling

**Tools (Function Calling)**
- `[ClaudeTool]` attribute for marking methods as Claude-callable tools
- `ToolRegistry` — automatic method discovery and invocation by name
- Async tool support via `Task<T>` return types

**Dependency Injection**
- `AddClaudeAI(Action<ClaudeOptions>)` — configure with code
- `AddClaudeAI(IConfiguration)` — configure from `appsettings.json`
- `ClaudeAIBuilder` — fluent builder with `.WithSkill<T>()` and `.WithTools<T>()`
- `ClaudeRequestBuilder` — fluent per-request builder via `_claude.With().Skill<T>().SendAsync()`

**Configuration**
- `ClaudeOptions` — full configuration: ApiKey, Model, MaxTokens, Temperature, Timeout, Retries, SystemPrompt
- `ClaudeModel` — constants for Sonnet, Opus, and Haiku model identifiers

**CI/CD**
- GitHub Actions workflow for build, test, and automated NuGet publishing on git tags
- Symbol package (`.snupkg`) publishing for debugging support

**Tests**
- Unit tests for `ClaudeClient` using xUnit, Moq, and FluentAssertions
- Tests covering: SendAsync, SendWithSkillAsync, ExecuteAsync (success + failure), ChatAsync
- Skill-specific tests for prompt transformation

---

## [Unreleased]

### Planned
- Agent workflows (multi-step autonomous task execution)
- Prompt template versioning system
- RAG (Retrieval-Augmented Generation) skill
- Embeddings support
- Azure Functions integration
- Source generator for automatic tool binding
- Observability — OpenTelemetry tracing and token usage metrics
- MediatR integration package (`ClaudeAI.DotNet.MediatR`)

---

[1.0.0]: https://github.com/shatru123/claude-ai-dotnet-sdk/releases/tag/v1.0.0
