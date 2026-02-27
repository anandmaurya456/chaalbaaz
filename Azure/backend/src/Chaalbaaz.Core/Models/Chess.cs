namespace Chaalbaaz.Core.Models;

public class GameSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChessComUsername { get; set; } = string.Empty;
    public string CurrentFen { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public GameStatus Status { get; set; } = GameStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class MoveEvaluation
{
    public string Move { get; set; } = string.Empty;        // UCI: e2e4
    public string MoveSan { get; set; } = string.Empty;     // SAN: e4
    public int? CentipawnScore { get; set; }
    public int? MateIn { get; set; }
    public int Depth { get; set; }
    public List<string> PrincipalVariation { get; set; } = new();
}

public class AnalysisResult
{
    public string Fen { get; set; } = string.Empty;
    public MoveEvaluation BestMove { get; set; } = new();
    public List<MoveEvaluation> TopMoves { get; set; } = new();
    public string Turn { get; set; } = string.Empty;
    public bool IsCheck { get; set; }
    public bool IsCheckmate { get; set; }
    public bool IsStalemate { get; set; }
    public bool Cached { get; set; }
    public DateTime AnalysedAt { get; set; } = DateTime.UtcNow;
}

public enum GameStatus
{
    Active,
    Completed,
    Abandoned
}
