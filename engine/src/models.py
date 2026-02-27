from pydantic import BaseModel, Field, field_validator
from typing import List, Optional


class AnalysisRequest(BaseModel):
    fen: str = Field(
        ...,
        description="FEN string representing the current board position",
        example="rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
    )
    depth: Optional[int] = Field(
        default=None,
        ge=1,
        le=30,
        description="Stockfish search depth (overrides server default)"
    )
    top_moves: Optional[int] = Field(
        default=None,
        ge=1,
        le=5,
        description="Number of top moves to return"
    )

    @field_validator("fen")
    @classmethod
    def validate_fen(cls, v: str) -> str:
        parts = v.strip().split(" ")
        if len(parts) < 4:
            raise ValueError("Invalid FEN string — must have at least 4 parts")
        return v.strip()


class MoveEvaluation(BaseModel):
    move: str = Field(..., description="Move in UCI notation e.g. e2e4")
    move_san: str = Field(..., description="Move in SAN notation e.g. e4")
    centipawn_score: Optional[int] = Field(
        None, description="Evaluation in centipawns (100 = 1 pawn advantage)"
    )
    mate_in: Optional[int] = Field(
        None, description="Mate in N moves (negative = opponent mates)"
    )
    depth: int = Field(..., description="Search depth used")
    pv: List[str] = Field(
        default_factory=list,
        description="Principal variation — best line of moves"
    )


class AnalysisResponse(BaseModel):
    fen: str
    best_move: MoveEvaluation
    top_moves: List[MoveEvaluation]
    turn: str = Field(..., description="'white' or 'black'")
    is_check: bool
    is_checkmate: bool
    is_stalemate: bool
    cached: bool = False


class HealthResponse(BaseModel):
    status: str
    stockfish_available: bool
    version: str
