using System.ComponentModel.DataAnnotations;

namespace Chaalbaaz.Core.DTOs;

// ---- Requests ----

public record AnalyseRequest(
    [Required] string Fen,
    int? Depth,
    int? TopMoves
);

public record CreateSessionRequest(
    [Required] string ChessComUsername,
    string? GameId
);

public record UpdateFenRequest(
    [Required] string SessionId,
    [Required] string Fen
);

// ---- Responses ----

public record MoveEvaluationDto(
    string Move,
    string MoveSan,
    int? CentipawnScore,
    int? MateIn,
    int Depth,
    List<string> PrincipalVariation
);

public record AnalysisResultDto(
    string Fen,
    MoveEvaluationDto BestMove,
    List<MoveEvaluationDto> TopMoves,
    string Turn,
    bool IsCheck,
    bool IsCheckmate,
    bool IsStalemate,
    bool Cached,
    DateTime AnalysedAt
);

public record GameSessionDto(
    string Id,
    string ChessComUsername,
    string CurrentFen,
    string GameId,
    string Status,
    DateTime CreatedAt
);

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Error = null
)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);
    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}
