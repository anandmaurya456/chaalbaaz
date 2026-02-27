import redis.asyncio as redis
import json
import hashlib
import logging
from typing import Optional

from .config import Settings
from .models import AnalysisResponse

logger = logging.getLogger(__name__)


class CacheService:
    """
    Redis-backed cache for FEN analysis results.
    Cache key = SHA256(fen + depth + top_moves)
    """

    def __init__(self, settings: Settings):
        self.settings = settings
        self._client: Optional[redis.Redis] = None

    async def _get_client(self) -> redis.Redis:
        if self._client is None:
            self._client = redis.from_url(
                self.settings.redis_url,
                encoding="utf-8",
                decode_responses=True,
            )
        return self._client

    def _make_key(self, fen: str, depth: int, top_moves: int) -> str:
        raw = f"{fen}:{depth}:{top_moves}"
        return f"chaalbaaz:analysis:{hashlib.sha256(raw.encode()).hexdigest()}"

    async def get(
        self, fen: str, depth: int, top_moves: int
    ) -> Optional[AnalysisResponse]:
        try:
            client = await self._get_client()
            key = self._make_key(fen, depth, top_moves)
            data = await client.get(key)
            if data:
                logger.debug(f"Cache HIT for key: {key[:20]}...")
                return AnalysisResponse(**json.loads(data))
        except Exception as e:
            logger.warning(f"Cache GET failed: {e}")
        return None

    async def set(
        self,
        fen: str,
        depth: int,
        top_moves: int,
        response: AnalysisResponse,
    ) -> None:
        try:
            client = await self._get_client()
            key = self._make_key(fen, depth, top_moves)
            await client.setex(
                key,
                self.settings.cache_ttl_seconds,
                response.model_dump_json(),
            )
            logger.debug(f"Cache SET for key: {key[:20]}...")
        except Exception as e:
            logger.warning(f"Cache SET failed: {e}")

    async def close(self):
        if self._client:
            await self._client.aclose()


_cache_instance: Optional[CacheService] = None


def get_cache_service() -> CacheService:
    global _cache_instance
    if _cache_instance is None:
        _cache_instance = CacheService(Settings())
    return _cache_instance
