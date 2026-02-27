using Chaalbaaz.Application.Services;
using Chaalbaaz.Core.DTOs;
using Chaalbaaz.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chaalbaaz.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlayerController : ControllerBase
{
    private readonly IChessComClient _chessComClient;
    private readonly PlayerHistoryService _historyService;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(
        IChessComClient chessComClient,
        PlayerHistoryService historyService,
        ILogger<PlayerController> logger)
    {
        _chessComClient = chessComClient;
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// Get Chess.com player profile and ratings.
    /// </summary>
    [HttpGet("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayer(string username, CancellationToken ct)
    {
        var player = await _chessComClient.GetPlayerAsync(username, ct);
        if (player is null)
            return NotFound(ApiResponse<object>.Fail($"Player '{username}' not found on Chess.com"));

        var stats = await _chessComClient.GetPlayerStatsAsync(username, ct);

        return Ok(ApiResponse<object>.Ok(new { player, stats }));
    }

    /// <summary>
    /// Get player's personalised profile — openings, win rate, time control preference.
    /// Used by the extension to personalise move suggestions.
    /// </summary>
    [HttpGet("{username}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerProfile(string username, CancellationToken ct)
    {
        try
        {
            var profile = await _historyService.BuildProfileAsync(username, ct);
            return Ok(ApiResponse<PlayerProfile>.Ok(profile));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PlayerProfile>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get recent games for a player.
    /// </summary>
    [HttpGet("{username}/games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentGames(
        string username,
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 50);
        var games = await _chessComClient.GetRecentGamesAsync(username, count, ct);
        return Ok(ApiResponse<object>.Ok(new { games, total = games.Count }));
    }

    /// <summary>
    /// Check if a player has an active live game.
    /// Called by the extension on startup to detect ongoing games.
    /// </summary>
    [HttpGet("{username}/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveGame(string username, CancellationToken ct)
    {
        var liveGame = await _chessComClient.GetCurrentLiveGameAsync(username, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            hasActiveGame = liveGame is not null,
            game = liveGame
        }));
    }
}
