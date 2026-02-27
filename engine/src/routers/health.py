from fastapi import APIRouter
import chess.engine
import logging

from ..models import HealthResponse
from ..config import Settings

logger = logging.getLogger(__name__)
router = APIRouter()
settings = Settings()


@router.get("/health", response_model=HealthResponse)
async def health_check() -> HealthResponse:
    """Liveness + readiness check — verifies Stockfish is reachable."""
    stockfish_ok = False
    try:
        engine = chess.engine.SimpleEngine.popen_uci(settings.stockfish_path)
        engine.quit()
        stockfish_ok = True
    except Exception as e:
        logger.warning(f"Stockfish health check failed: {e}")

    return HealthResponse(
        status="ok" if stockfish_ok else "degraded",
        stockfish_available=stockfish_ok,
        version="0.1.0",
    )
