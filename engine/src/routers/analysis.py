from fastapi import APIRouter, Depends, HTTPException
import logging

from ..models import AnalysisRequest, AnalysisResponse
from ..stockfish_service import StockfishService, get_stockfish_service
from ..cache_service import CacheService, get_cache_service
from ..config import Settings

logger = logging.getLogger(__name__)
router = APIRouter()
settings = Settings()


@router.post("/analyse", response_model=AnalysisResponse)
async def analyse_position(
    request: AnalysisRequest,
    stockfish: StockfishService = Depends(get_stockfish_service),
    cache: CacheService = Depends(get_cache_service),
) -> AnalysisResponse:
    """
    Analyse a chess position from a FEN string.

    Returns the best move + top N moves with Stockfish evaluations.
    Results are cached in Redis by FEN + depth + top_moves.
    """
    depth = request.depth or settings.stockfish_depth
    top_moves = request.top_moves or settings.stockfish_top_moves

    # Check cache first
    cached = await cache.get(request.fen, depth, top_moves)
    if cached:
        cached.cached = True
        return cached

    # Run Stockfish analysis
    try:
        result = stockfish.analyse(
            fen=request.fen,
            depth=depth,
            top_moves=top_moves,
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Stockfish analysis failed: {e}")
        raise HTTPException(status_code=500, detail="Engine analysis failed")

    # Cache result
    await cache.set(request.fen, depth, top_moves, result)

    return result
