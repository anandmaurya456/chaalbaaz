from pydantic_settings import BaseSettings
from typing import List


class Settings(BaseSettings):
    # Stockfish
    stockfish_path: str = "/usr/games/stockfish"
    stockfish_depth: int = 20
    stockfish_threads: int = 2
    stockfish_hash_mb: int = 128
    stockfish_top_moves: int = 3

    # Redis
    redis_url: str = "redis://localhost:6379"
    cache_ttl_seconds: int = 60

    # CORS
    allowed_origins: List[str] = ["*"]

    # App
    environment: str = "development"
    log_level: str = "INFO"

    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
