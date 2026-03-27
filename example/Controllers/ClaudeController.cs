using ClaudeAI.SDK.Models;
using ClaudeAI.SDK.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeAI.SDK.Example.Controllers;

/// <summary>
/// Example ASP.NET Core Web API controller demonstrating all SDK capabilities.
/// Copy this into your Web API project and register IClaudeApplicationService via AddClaudeAI().
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClaudeController : ControllerBase
{
    private readonly IClaudeApplicationService _claude;

    public ClaudeController(IClaudeApplicationService claude) => _claude = claude;

    /// <summary>Summarizes the provided text content.</summary>
    // POST /api/claude/summarize
    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize(
        [FromBody] TextRequest request, CancellationToken ct)
    {
        var result = await _claude.SummarizeAsync(request.Content, request.Context, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Explains the provided source code.</summary>
    // POST /api/claude/explain-code
    [HttpPost("explain-code")]
    public async Task<IActionResult> ExplainCode(
        [FromBody] CodeRequest request, CancellationToken ct)
    {
        var result = await _claude.ExplainCodeAsync(request.Code, request.Language, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Generates technical documentation from code or descriptions.</summary>
    // POST /api/claude/generate-docs
    [HttpPost("generate-docs")]
    public async Task<IActionResult> GenerateDocs(
        [FromBody] DocRequest request, CancellationToken ct)
    {
        var result = await _claude.GenerateDocumentationAsync(request.Input, request.DocType, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Reviews code or a design and returns structured feedback.</summary>
    // POST /api/claude/review
    [HttpPost("review")]
    public async Task<IActionResult> Review(
        [FromBody] ReviewRequest request, CancellationToken ct)
    {
        var result = await _claude.ReviewAsync(request.Artifact, request.ReviewType, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Provides structured decision support with pros, cons, and a recommendation.</summary>
    // POST /api/claude/decision
    [HttpPost("decision")]
    public async Task<IActionResult> Decision([FromBody] DecisionRequest request)
    {
        var result = await _claude.DecisionSupportAsync(
            request.Context, request.Options ?? []);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Multi-turn chat with optional conversation history.</summary>
    // POST /api/claude/chat
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request, CancellationToken ct)
    {
        var result = await _claude.ChatAsync(request.Message, request.History, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Streams a chat response as Server-Sent Events.</summary>
    // GET /api/claude/stream?message=your+question
    [HttpGet("stream")]
    public async Task Stream([FromQuery] string message, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var chunk in _claude.StreamChatAsync(message, ct))
        {
            await Response.WriteAsync($"data: {chunk}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
