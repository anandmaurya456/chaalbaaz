using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Infrastructure.Chess.Models;
using Microsoft.Extensions.Logging;

namespace Chaalbaaz.Application.Services;

/// <summary>
/// Analyses a player's Chess.com game history to identify:
/// - Most played openings (ECO codes)
/// - Win/loss/draw rates by time control
/// - Common blunder patterns
/// - Weakest phases (opening/middlegame/endgame)
/// </summary>
public class PlayerHistoryService
{
    private readonly IChessComClient _chessComClient;
    private readonly ILogger<PlayerHistoryService> _logger;

    public PlayerHistoryService(IChessComClient chessComClient, ILogger<PlayerHistoryService> logger)
    {
        _chessComClient = chessComClient;
        _logger = logger;
    }

    public async Task<PlayerProfile> BuildProfileAsync(string username, CancellationToken ct = default)
    {
        _logger.LogInformation("Building player profile for {Username}", username);

        var (player, stats, recentGames) = await FetchPlayerDataAsync(username, ct);

        if (player is null)
            throw new InvalidOperationException($"Chess.com player not found: {username}");

        var profile = new PlayerProfile
        {
            Username = username,
            Rating = GetCurrentRating(stats),
            GamesAnalysed = recentGames.Count,
            OpeningStats = AnalyseOpenings(recentGames, username),
            WinRate = CalculateWinRate(recentGames, username),
            TimeControlPreference = DetermineTimeControl(stats),
        };

        _logger.LogInformation(
            "Profile built for {Username}: Rating={Rating}, Games={Games}",
            username, profile.Rating, profile.GamesAnalysed);

        return profile;
    }

    private async Task<(ChessComPlayer?, ChessComPlayerStats?, List<ChessComGame>)> FetchPlayerDataAsync(
        string username, CancellationToken ct)
    {
        var playerTask = _chessComClient.GetPlayerAsync(username, ct);
        var statsTask = _chessComClient.GetPlayerStatsAsync(username, ct);
        var gamesTask = _chessComClient.GetRecentGamesAsync(username, 50, ct);

        await Task.WhenAll(playerTask, statsTask, gamesTask);

        return (await playerTask, await statsTask, await gamesTask);
    }

    private static int GetCurrentRating(ChessComPlayerStats? stats)
    {
        if (stats is null) return 0;
        return stats.ChessRapid?.Last?.Rating
            ?? stats.ChessBlitz?.Last?.Rating
            ?? stats.ChessBullet?.Last?.Rating
            ?? stats.ChessDaily?.Last?.Rating
            ?? 0;
    }

    private static List<OpeningStat> AnalyseOpenings(List<ChessComGame> games, string username)
    {
        return games
            .Where(g => !string.IsNullOrEmpty(g.Pgn))
            .Select(g => new
            {
                Eco = ExtractEco(g.Pgn),
                Opening = ExtractOpeningName(g.Pgn),
                IsWin = IsWin(g, username),
            })
            .Where(x => x.Eco != null)
            .GroupBy(x => x.Eco!)
            .Select(grp => new OpeningStat(
                EcoCode: grp.Key,
                Name: grp.First().Opening ?? grp.Key,
                Played: grp.Count(),
                WinRate: (double)grp.Count(x => x.IsWin) / grp.Count() * 100
            ))
            .OrderByDescending(x => x.Played)
            .Take(10)
            .ToList();
    }

    private static double CalculateWinRate(List<ChessComGame> games, string username)
    {
        if (!games.Any()) return 0;
        var wins = games.Count(g => IsWin(g, username));
        return (double)wins / games.Count * 100;
    }

    private static string DetermineTimeControl(ChessComPlayerStats? stats)
    {
        if (stats is null) return "rapid";
        var ratings = new[]
        {
            ("bullet", stats.ChessBullet?.Record?.Win + stats.ChessBullet?.Record?.Loss ?? 0),
            ("blitz", stats.ChessBlitz?.Record?.Win + stats.ChessBlitz?.Record?.Loss ?? 0),
            ("rapid", stats.ChessRapid?.Record?.Win + stats.ChessRapid?.Record?.Loss ?? 0),
            ("daily", stats.ChessDaily?.Record?.Win + stats.ChessDaily?.Record?.Loss ?? 0),
        };
        return ratings.OrderByDescending(x => x.Item2).First().Item1;
    }

    private static bool IsWin(ChessComGame game, string username)
    {
        var uname = username.ToLower();
        if (game.White.Username.ToLower() == uname) return game.White.Result == "win";
        if (game.Black.Username.ToLower() == uname) return game.Black.Result == "win";
        return false;
    }

    private static string? ExtractEco(string pgn)
    {
        var match = System.Text.RegularExpressions.Regex.Match(pgn, @"\[ECO ""([^""]+)""\]");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractOpeningName(string pgn)
    {
        var match = System.Text.RegularExpressions.Regex.Match(pgn, @"\[Opening ""([^""]+)""\]");
        return match.Success ? match.Groups[1].Value : null;
    }
}

// ---- Domain models ----

public record PlayerProfile
{
    public string Username { get; init; } = string.Empty;
    public int Rating { get; init; }
    public int GamesAnalysed { get; init; }
    public double WinRate { get; init; }
    public string TimeControlPreference { get; init; } = "rapid";
    public List<OpeningStat> OpeningStats { get; init; } = new();
}

public record OpeningStat(
    string EcoCode,
    string Name,
    int Played,
    double WinRate
);
