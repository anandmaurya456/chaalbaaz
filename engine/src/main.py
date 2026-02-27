from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import logging

from .routers import analysis, health
from .config import Settings

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

settings = Settings()


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("🚀 Chaalbaaz Engine starting up...")
    logger.info(f"   Stockfish depth  : {settings.stockfish_depth}")
    logger.info(f"   Stockfish threads: {settings.stockfish_threads}")
    yield
    logger.info("🛑 Chaalbaaz Engine shutting down...")


app = FastAPI(
    title="Chaalbaaz Engine",
    description="Stockfish-powered chess move analysis API",
    version="0.1.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.allowed_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(health.router, tags=["Health"])
app.include_router(analysis.router, prefix="/api/v1", tags=["Analysis"])
