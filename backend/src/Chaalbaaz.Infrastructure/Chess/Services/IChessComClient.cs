using Chaalbaaz.Infrastructure.Chess.Models;

namespace Chaalbaaz.Core.Interfaces;

public interface IChessComClient
{
    /// <summary>Get public player profile</summary>
    Task<ChessComPlayer?> GetPlayerAsync(string username, CancellationToken ct = default);

    /// <summary>Get player ratings across all time controls</summary>
    Task<ChessComPlayerStats?> GetPlayerStatsAsync(string username, CancellationToken ct = default);

    /// <summary>Get list of monthly game archive URLs</summary>
    Task<List<string>> GetGameArchivesAsync(string username, CancellationToken ct = default);

    /// <summary>Get all games for a specific month</summary>
    Task<List<ChessComGame>> GetMonthlyGamesAsync(string username, int year, int month, CancellationToken ct = default);

    /// <summary>Get last N games for a player</summary>
    Task<List<ChessComGame>> GetRecentGamesAsync(string username, int count = 10, CancellationToken ct = default);

    /// <summary>Get current live game FEN (if any)</summary>
    Task<ChessComLiveGame?> GetCurrentLiveGameAsync(string username, CancellationToken ct = default);
}
