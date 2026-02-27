using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Core.Models;
using Chaalbaaz.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaalbaaz.Application.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IStockfishEngineClient _engine;
    private readonly IGameSessionRepository _sessions;
    private readonly IHubContext<ChessHub> _hubContext;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IStockfishEngineClient engine,
        IGameSessionRepository sessions,
        IHubContext<ChessHub> hubContext,
        ILogger<AnalysisService> logger)
    {
        _engine = engine;
        _sessions = sessions;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<AnalysisResult> AnalysePositionAsync(
        string fen,
        int? depth = null,
        int? topMoves = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Analysing position: {Fen}", fen);
        return await _engine.AnalyseAsync(fen, depth, topMoves, ct);
    }

    public async Task<AnalysisResult> AnalyseAndBroadcastAsync(
        string sessionId,
        string fen,
        CancellationToken ct = default)
    {
        // Update session FEN
        var session = await _sessions.GetByIdAsync(sessionId, ct);
        if (session is not null)
        {
            session.CurrentFen = fen;
            await _sessions.UpdateAsync(session, ct);
        }

        // Run analysis
        var result = await _engine.AnalyseAsync(fen, ct: ct);

        // Broadcast to all clients in this session group via SignalR
        await _hubContext.Clients
            .Group(sessionId)
            .SendAsync("ReceiveSuggestion", result, ct);

        _logger.LogInformation(
            "Broadcasted suggestion for session {SessionId}: {BestMove}",
            sessionId, result.BestMove.MoveSan);

        return result;
    }
}
