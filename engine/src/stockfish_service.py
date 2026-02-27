import chess
import chess.engine
import logging
import asyncio
from typing import List, Optional
from functools import lru_cache

from .models import AnalysisResponse, MoveEvaluation
from .config import Settings

logger = logging.getLogger(__name__)


class StockfishService:
    """
    Wraps python-chess + Stockfish engine.
    Handles FEN analysis, move evaluation, and top-N suggestions.
    """

    def __init__(self, settings: Settings):
        self.settings = settings
        self._engine: Optional[chess.engine.SimpleEngine] = None

    def _get_engine(self) -> chess.engine.SimpleEngine:
        """Lazy-init Stockfish engine (singleton per process)."""
        if self._engine is None:
            logger.info(f"Initialising Stockfish at: {self.settings.stockfish_path}")
            self._engine = chess.engine.SimpleEngine.popen_uci(
                self.settings.stockfish_path
            )
            self._engine.configure({
                "Threads": self.settings.stockfish_threads,
                "Hash": self.settings.stockfish_hash_mb,
            })
            logger.info("✅ Stockfish engine ready")
        return self._engine

    def analyse(
        self,
        fen: str,
        depth: Optional[int] = None,
        top_moves: Optional[int] = None,
    ) -> AnalysisResponse:
        """
        Analyse a board position from a FEN string.
        Returns best move + top N moves with evaluations.
        """
        search_depth = depth or self.settings.stockfish_depth
        num_moves = top_moves or self.settings.stockfish_top_moves

        board = chess.Board(fen)

        # Board state flags
        is_check = board.is_check()
        is_checkmate = board.is_checkmate()
        is_stalemate = board.is_stalemate()
        turn = "white" if board.turn == chess.WHITE else "black"

        if is_checkmate or is_stalemate:
            # No moves to suggest
            return AnalysisResponse(
                fen=fen,
                best_move=MoveEvaluation(
                    move="none",
                    move_san="none",
                    centipawn_score=None,
                    mate_in=0 if is_checkmate else None,
                    depth=0,
                    pv=[],
                ),
                top_moves=[],
                turn=turn,
                is_check=is_check,
                is_checkmate=is_checkmate,
                is_stalemate=is_stalemate,
            )

        engine = self._get_engine()

        # Get top N moves using MultiPV
        infos = engine.analyse(
            board,
            chess.engine.Limit(depth=search_depth),
            multipv=num_moves,
        )

        evaluations = []
        for info in infos:
            move = info.get("pv", [None])[0]
            if not move:
                continue

            pv_moves = [m.uci() for m in info.get("pv", [])]
            score = info.get("score")

            centipawn = None
            mate = None

            if score:
                relative = score.relative
                if relative.is_mate():
                    mate = relative.mate()
                else:
                    centipawn = relative.score()

            # Convert UCI move to SAN for human readability
            san = board.san(move)

            evaluations.append(MoveEvaluation(
                move=move.uci(),
                move_san=san,
                centipawn_score=centipawn,
                mate_in=mate,
                depth=info.get("depth", search_depth),
                pv=pv_moves,
            ))

        best = evaluations[0] if evaluations else None

        if not best:
            raise ValueError(f"Stockfish returned no moves for FEN: {fen}")

        return AnalysisResponse(
            fen=fen,
            best_move=best,
            top_moves=evaluations,
            turn=turn,
            is_check=is_check,
            is_checkmate=is_checkmate,
            is_stalemate=is_stalemate,
        )

    def close(self):
        if self._engine:
            self._engine.quit()
            self._engine = None
            logger.info("Stockfish engine closed")


@lru_cache(maxsize=1)
def get_stockfish_service() -> StockfishService:
    """FastAPI dependency — returns singleton StockfishService."""
    settings = Settings()
    return StockfishService(settings)
