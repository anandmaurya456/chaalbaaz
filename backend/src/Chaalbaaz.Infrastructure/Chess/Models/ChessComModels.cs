namespace Chaalbaaz.Infrastructure.Chess.Models;

// ---- Player ----

public record ChessComPlayer(
    string Username,
    string PlayerId,
    string Title,
    int Rating,
    string Status,
    long JoinDate,
    long LastOnline,
    string Country,
    string Avatar
);

public record ChessComPlayerStats(
    ChessComRatingStats? ChessRapid,
    ChessComRatingStats? ChessBlitz,
    ChessComRatingStats? ChessBullet,
    ChessComRatingStats? ChessDaily
);

public record ChessComRatingStats(
    ChessComRatingRecord? Last,
    ChessComRatingRecord? Best,
    ChessComRecord? Record
);

public record ChessComRatingRecord(
    int Rating,
    long Date,
    string? Rd
);

public record ChessComRecord(
    int Win,
    int Loss,
    int Draw
);

// ---- Games ----

public record ChessComGameArchive(
    List<string> Archives
);

public record ChessComGamesResponse(
    List<ChessComGame> Games
);

public record ChessComGame(
    string Url,
    string Pgn,
    long TimeControl,
    long EndTime,
    bool Rated,
    string Fen,
    string TimeClass,
    string Rules,
    ChessComGamePlayer White,
    ChessComGamePlayer Black
);

public record ChessComGamePlayer(
    string Rating,
    string Result,
    string Id,
    string Username,
    string? Uuid
);

// ---- Live Game (undocumented) ----

public record ChessComLiveGame(
    string Fen,
    string Turn,
    int MoveNumber,
    string Pgn,
    bool IsFinished,
    string? Result
);
