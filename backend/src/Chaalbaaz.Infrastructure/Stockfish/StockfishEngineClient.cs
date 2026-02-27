using System.Net.Http.Json;
using System.Text.Json;
using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Core.Models;
using Microsoft.Extensions.Logging;

namespace Chaalbaaz.Infrastructure.Stockfish;

public class StockfishEngineClient : IStockfishEngineClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockfishEngineClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public StockfishEngineClient(HttpClient httpClient, ILogger<StockfishEngineClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AnalysisResult> AnalyseAsync(
        string fen,
        int? depth = null,
        int? topMoves = null,
        CancellationToken ct = default)
    {
        var payload = new
        {
            fen,
            depth,
            top_moves = topMoves
        };

        _logger.LogInformation("Sending FEN to engine: {Fen}", fen);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/analyse", payload, JsonOptions, ct);

        response.EnsureSuccessStatusCode();

        var engineResponse = await response.Content
            .ReadFromJsonAsync<EngineAnalysisResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Engine returned null response");

        return MapToAnalysisResult(engineResponse);
    }

    private static AnalysisResult MapToAnalysisResult(EngineAnalysisResponse r) => new()
    {
        Fen = r.Fen,
        Turn = r.Turn,
        IsCheck = r.IsCheck,
        IsCheckmate = r.IsCheckmate,
        IsStalemate = r.IsStalemate,
        Cached = r.Cached,
        BestMove = MapMove(r.BestMove),
        TopMoves = r.TopMoves.Select(MapMove).ToList(),
    };

    private static MoveEvaluation MapMove(EngineMoveResponse m) => new()
    {
        Move = m.Move,
        MoveSan = m.MoveSan,
        CentipawnScore = m.CentipawnScore,
        MateIn = m.MateIn,
        Depth = m.Depth,
        PrincipalVariation = m.Pv,
    };

    // ---- Internal response models (mirrors Python engine schema) ----

    private record EngineAnalysisResponse(
        string Fen,
        EngineMoveResponse BestMove,
        List<EngineMoveResponse> TopMoves,
        string Turn,
        bool IsCheck,
        bool IsCheckmate,
        bool IsStalemate,
        bool Cached
    );

    private record EngineMoveResponse(
        string Move,
        string MoveSan,
        int? CentipawnScore,
        int? MateIn,
        int Depth,
        List<string> Pv
    );
}
