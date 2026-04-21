using ClaudeAI.DotNet.Commands;
using ClaudeAI.DotNet.Configuration;
using ClaudeAI.DotNet.Core;
using ClaudeAI.DotNet.Models;
using ClaudeAI.DotNet.Services;
using ClaudeAI.DotNet.Skills;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClaudeAI.DotNet.Tests;

public class ClaudeClientTests
{
    private readonly Mock<IClaudeHttpClient> _mockHttpClient;
    private readonly ClaudeOptions _options;
    private readonly ClaudeClient _client;

    public ClaudeClientTests()
    {
        _mockHttpClient = new Mock<IClaudeHttpClient>();
        _options = new ClaudeOptions
        {
            ApiKey = "test-api-key",
            Model = ClaudeModel.Sonnet,
            MaxTokens = 1024
        };

        _client = new ClaudeClient(
            _mockHttpClient.Object,
            Options.Create(_options),
            NullLogger<ClaudeClient>.Instance);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResponseText()
    {
        // Arrange
        var expectedText = "Hello from Claude!";
        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<ClaudeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaudeResponse
            {
                Content = [new ClaudeContent { Type = "text", Text = expectedText }]
            });

        // Act
        var result = await _client.SendAsync("Hello!");

        // Assert
        result.Should().Be(expectedText);
        _mockHttpClient.Verify(x => x.SendAsync(
            It.Is<ClaudeRequest>(r => r.Messages.Any(m => m.Content == "Hello!")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendWithSkillAsync_ShouldApplySkillTransformations()
    {
        // Arrange
        var skill = new SummarizationSkill();
        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<ClaudeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaudeResponse
            {
                Content = [new ClaudeContent { Type = "text", Text = "Summary result" }]
            });

        // Act
        var result = await _client.SendWithSkillAsync("Long text here", skill);

        // Assert
        result.Should().Be("Summary result");
        _mockHttpClient.Verify(x => x.SendAsync(
            It.Is<ClaudeRequest>(r =>
                r.System != null &&
                r.System.Contains("summarization") &&
                r.Messages.Any(m => m.Content.Contains("Please summarize"))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessResult_WhenRequestSucceeds()
    {
        // Arrange
        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<ClaudeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaudeResponse
            {
                Content = [new ClaudeContent { Type = "text", Text = "Result" }],
                Usage = new ClaudeUsage { InputTokens = 10, OutputTokens = 20 }
            });

        var command = new ClaudeCommand { Prompt = "Test prompt" };

        // Act
        var result = await _client.ExecuteAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().Be("Result");
        result.Usage!.TotalTokens.Should().Be(30);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailureResult_WhenRequestFails()
    {
        // Arrange
        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<ClaudeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API error"));

        var command = new ClaudeCommand { Prompt = "Test prompt" };

        // Act
        var result = await _client.ExecuteAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API error");
    }

    [Fact]
    public async Task ChatAsync_ShouldSendAllMessages()
    {
        // Arrange
        _mockHttpClient
            .Setup(x => x.SendAsync(It.IsAny<ClaudeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaudeResponse
            {
                Content = [new ClaudeContent { Type = "text", Text = "Chat response" }]
            });

        var messages = new List<ClaudeMessage>
        {
            ClaudeMessage.User("Hello"),
            ClaudeMessage.Assistant("Hi there!"),
            ClaudeMessage.User("How are you?")
        };

        // Act
        var result = await _client.ChatAsync(messages);

        // Assert
        result.Should().Be("Chat response");
        _mockHttpClient.Verify(x => x.SendAsync(
            It.Is<ClaudeRequest>(r => r.Messages.Count == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class SkillTests
{
    [Fact]
    public void SummarizationSkill_ShouldTransformPromptCorrectly()
    {
        var skill = new SummarizationSkill();
        var result = skill.TransformPrompt("Some long text");
        result.Should().Contain("Please summarize");
        result.Should().Contain("Some long text");
    }

    [Fact]
    public void CodeReviewSkill_ShouldWrapCodeInFences()
    {
        var skill = new CodeReviewSkill();
        var result = skill.TransformPrompt("var x = 1;");
        result.Should().Contain("```");
        result.Should().Contain("var x = 1;");
    }

    [Fact]
    public void TranslationSkill_ShouldIncludeTargetLanguage()
    {
        var skill = new TranslationSkill("French");
        var systemPrompt = skill.GetSystemPrompt();
        systemPrompt.Should().Contain("French");
    }
}
