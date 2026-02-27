using Chaalbaaz.Core.Models;

namespace Chaalbaaz.Core.Interfaces;

public interface IStockfishEngineClient
{
    Task<AnalysisResult> AnalyseAsync(string fen, int? depth = null, int? topMoves = null, CancellationToken ct = default);
}

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(string sessionId, CancellationToken ct = default);
    Task<GameSession?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<GameSession> CreateAsync(GameSession session, CancellationToken ct = default);
    Task<GameSession> UpdateAsync(GameSession session, CancellationToken ct = default);
    Task DeleteAsync(string sessionId, CancellationToken ct = default);
}

public interface IAnalysisService
{
    Task<AnalysisResult> AnalysePositionAsync(string fen, int? depth = null, int? topMoves = null, CancellationToken ct = default);
    Task<AnalysisResult> AnalyseAndBroadcastAsync(string sessionId, string fen, CancellationToken ct = default);
}
