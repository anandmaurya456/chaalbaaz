using Chaalbaaz.Core.DTOs;
using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chaalbaaz.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class SessionController : ControllerBase
{
    private readonly IGameSessionRepository _sessions;
    private readonly ILogger<SessionController> _logger;

    public SessionController(IGameSessionRepository sessions, ILogger<SessionController> logger)
    {
        _sessions = sessions;
        _logger = logger;
    }

    /// <summary>
    /// Create a new game session for a Chess.com username.
    /// Returns a sessionId the extension uses for SignalR + analysis.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GameSessionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken ct)
    {
        // Check if active session already exists
        var existing = await _sessions.GetByUsernameAsync(request.ChessComUsername, ct);
        if (existing is not null)
        {
            return Ok(ApiResponse<GameSessionDto>.Ok(MapToDto(existing)));
        }

        var session = new GameSession
        {
            ChessComUsername = request.ChessComUsername,
            GameId = request.GameId ?? string.Empty,
        };

        var created = await _sessions.CreateAsync(session, ct);
        _logger.LogInformation("Session created for {Username}: {SessionId}",
            created.ChessComUsername, created.Id);

        return CreatedAtAction(
            nameof(GetSession),
            new { sessionId = created.Id },
            ApiResponse<GameSessionDto>.Ok(MapToDto(created))
        );
    }

    /// <summary>
    /// Get an existing session by ID.
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<GameSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(string sessionId, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(sessionId, ct);
        if (session is null)
            return NotFound(ApiResponse<GameSessionDto>.Fail("Session not found"));

        return Ok(ApiResponse<GameSessionDto>.Ok(MapToDto(session)));
    }

    /// <summary>
    /// Delete / end a session.
    /// </summary>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSession(string sessionId, CancellationToken ct)
    {
        await _sessions.DeleteAsync(sessionId, ct);
        return NoContent();
    }

    private static GameSessionDto MapToDto(GameSession s) => new(
        s.Id,
        s.ChessComUsername,
        s.CurrentFen,
        s.GameId,
        s.Status.ToString(),
        s.CreatedAt
    );
}
