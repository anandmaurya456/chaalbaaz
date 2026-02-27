using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Infrastructure.Chess.Models;
using Microsoft.Extensions.Logging;

namespace Chaalbaaz.Infrastructure.Chess;

public class ChessComClient : IChessComClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChessComClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private const string BaseUrl = "https://api.chess.com/pub";

    public ChessComClient(HttpClient httpClient, ILogger<ChessComClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        // Chess.com requires a User-Agent header
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chaalbaaz/0.1 (https://github.com/anandmaurya456/chaalbaaz)");
        _logger = logger;
    }

    public async Task<ChessComPlayer?> GetPlayerAsync(string username, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching Chess.com profile for {Username}", username);
            return await _httpClient.GetFromJsonAsync<ChessComPlayer>(
                $"/player/{username.ToLower()}", JsonOptions, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Chess.com player not found: {Username}", username);
            return null;
        }
    }

    public async Task<ChessComPlayerStats?> GetPlayerStatsAsync(string username, CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ChessComPlayerStats>(
                $"/player/{username.ToLower()}/stats", JsonOptions, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch stats for {Username}", username);
            return null;
        }
    }

    public async Task<List<string>> GetGameArchivesAsync(string username, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ChessComGameArchive>(
                $"/player/{username.ToLower()}/games/archives", JsonOptions, ct);
            return response?.Archives ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch archives for {Username}", username);
            return new List<string>();
        }
    }

    public async Task<List<ChessComGame>> GetMonthlyGamesAsync(
        string username, int year, int month, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching {Year}/{Month} games for {Username}", year, month, username);

            var response = await _httpClient.GetFromJsonAsync<ChessComGamesResponse>(
                $"/player/{username.ToLower()}/games/{year}/{month:D2}", JsonOptions, ct);

            return response?.Games ?? new List<ChessComGame>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch games for {Username} {Year}/{Month}", username, year, month);
            return new List<ChessComGame>();
        }
    }

    public async Task<List<ChessComGame>> GetRecentGamesAsync(
        string username, int count = 10, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var allGames = new List<ChessComGame>();

        // Fetch current month first, go back if needed
        for (int monthOffset = 0; monthOffset <= 3 && allGames.Count < count; monthOffset++)
        {
            var target = now.AddMonths(-monthOffset);
            var games = await GetMonthlyGamesAsync(username, target.Year, target.Month, ct);

            // Most recent first
            allGames.AddRange(games.OrderByDescending(g => g.EndTime));

            if (monthOffset == 0 && !games.Any()) break;
        }

        return allGames
            .OrderByDescending(g => g.EndTime)
            .Take(count)
            .ToList();
    }

    public async Task<ChessComLiveGame?> GetCurrentLiveGameAsync(
        string username, CancellationToken ct = default)
    {
        // Chess.com doesn't have an official live game API
        // We check the most recent game and see if it's still ongoing
        try
        {
            var recentGames = await GetRecentGamesAsync(username, 1, ct);
            var latest = recentGames.FirstOrDefault();

            if (latest is null) return null;

            // If the game ended within the last 30 seconds, treat as live
            var endTime = DateTimeOffset.FromUnixTimeSeconds(latest.EndTime);
            var isRecent = (DateTimeOffset.UtcNow - endTime).TotalSeconds < 30;

            if (!isRecent) return null;

            return new ChessComLiveGame(
                Fen: latest.Fen,
                Turn: latest.Fen.Split(' ').ElementAtOrDefault(1) == "w" ? "white" : "black",
                MoveNumber: int.Parse(latest.Fen.Split(' ').ElementAtOrDefault(5) ?? "1"),
                Pgn: latest.Pgn,
                IsFinished: false,
                Result: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get live game for {Username}", username);
            return null;
        }
    }
}
