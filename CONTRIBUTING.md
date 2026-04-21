# Contributing to ClaudeAI.DotNet

First off — thank you for considering a contribution! 🙏 Every PR, issue, and discussion makes this SDK better for the entire .NET community.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Ways to Contribute](#ways-to-contribute)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Adding a New Skill](#adding-a-new-skill)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Reporting Bugs](#reporting-bugs)
- [Feature Requests](#feature-requests)

---

## Code of Conduct

Be kind. Be constructive. We're all here to build something useful together. Harassment, discrimination, or disrespectful behavior of any kind will not be tolerated.

---

## Ways to Contribute

You don't have to write code to contribute! Here's how you can help:

- ⭐ **Star the repo** — helps with discoverability
- 🐛 **Report bugs** — open a detailed GitHub issue
- 💡 **Suggest features** — open a feature request issue
- 📝 **Improve docs** — fix typos, add examples, clarify things
- 🧠 **Add a new Skill** — the easiest code contribution
- 🔧 **Fix bugs** — pick an open issue labeled `good first issue`
- 🧪 **Add tests** — improve coverage for existing features
- 📣 **Spread the word** — blog posts, tweets, Reddit posts

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Git
- Any IDE: Visual Studio 2022, VS Code with C# extension, or JetBrains Rider

### Setup

```bash
# Fork the repo on GitHub, then:
git clone https://github.com/YOUR_USERNAME/claude-ai-dotnet-sdk
cd claude-ai-dotnet-sdk

# Restore dependencies
dotnet restore

# Run tests to verify everything works
dotnet test

# Build
dotnet build
```

---

## Development Workflow

```bash
# 1. Create a branch from main
git checkout -b feat/my-awesome-feature

# 2. Make your changes

# 3. Run tests
dotnet test --verbosity normal

# 4. Build in Release mode
dotnet build --configuration Release

# 5. Commit with a meaningful message
git commit -m "feat: add EmbeddingsSkill for semantic search"

# 6. Push and open a PR
git push origin feat/my-awesome-feature
```

### Commit Message Format

We follow [Conventional Commits](https://www.conventionalcommits.org/):

| Prefix | When to use |
|--------|-------------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `docs:` | Documentation only |
| `test:` | Adding or updating tests |
| `refactor:` | Code change that's not a bug fix or feature |
| `chore:` | Build process, dependencies |

Examples:
```
feat: add EmbeddingsSkill for semantic vector search
fix: retry logic not applying correct backoff delay
docs: add multi-turn conversation example to README
test: add unit tests for TranslationSkill
```

---

## Coding Standards

- **Nullable reference types** are enabled — use `?` appropriately
- **Async/await** for all I/O operations
- **XML doc comments** on all public APIs
- **`CancellationToken`** on all async public methods
- Follow existing naming conventions (PascalCase for types/methods, camelCase for locals)
- Keep classes focused — single responsibility
- No magic strings — use constants

---

## Adding a New Skill

Adding a skill is the easiest way to contribute! Here's how:

1. Create a new class in `src/ClaudeAI.DotNet/Skills/`
2. Extend `BaseSkill`
3. Implement `Name` and `GetSystemPrompt()`
4. Optionally override `TransformPrompt()`
5. Add unit tests in `tests/ClaudeAI.DotNet.Tests/`
6. Add it to the Built-in Skills table in `README.md`

**Example — adding a `GrammarCorrectionSkill`:**

```csharp
public class GrammarCorrectionSkill : BaseSkill
{
    public override string Name => "GrammarCorrection";

    public override string GetSystemPrompt() =>
        """
        You are an expert editor specializing in grammar and style.
        Correct any grammar, spelling, punctuation, and style issues.
        Preserve the author's voice and intent.
        Return only the corrected text without explanations.
        """;

    public override string TransformPrompt(string userPrompt) =>
        $"Please correct the grammar in the following text:\n\n{userPrompt}";
}
```

Then add a test:

```csharp
[Fact]
public void GrammarCorrectionSkill_ShouldHaveCorrectName()
{
    var skill = new GrammarCorrectionSkill();
    skill.Name.Should().Be("GrammarCorrection");
    skill.GetSystemPrompt().Should().Contain("grammar");
}
```

---

## Submitting a Pull Request

1. Make sure all tests pass: `dotnet test`
2. Build succeeds in Release: `dotnet build -c Release`
3. PR title follows conventional commits format
4. Fill in the PR template completely
5. Link to any related issues (`Closes #123`)
6. Be responsive to review feedback — we aim to review within 48 hours

---

## Reporting Bugs

Please open a GitHub issue with:

- **Title**: Short, descriptive summary
- **Environment**: OS, .NET version, SDK version
- **Steps to reproduce**: Minimal code snippet
- **Expected behavior**
- **Actual behavior**
- **Stack trace** (if applicable)

---

## Feature Requests

Open an issue with the `enhancement` label and describe:

- **What** you want to add
- **Why** it would be useful
- **How** you'd expect to use it (code example if possible)

---

## Questions?

Open a [GitHub Discussion](https://github.com/shatru123/claude-ai-dotnet-sdk/discussions) — we're happy to help!

---

Built with ❤️ by [CompileCare](https://github.com/compilecare) | Pune, India 🇮🇳
