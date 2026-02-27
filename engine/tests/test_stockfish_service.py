import pytest
from unittest.mock import MagicMock, patch
from src.stockfish_service import StockfishService
from src.config import Settings


STARTING_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
AFTER_E4_FEN = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
CHECKMATE_FEN = "rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3"


@pytest.fixture
def settings():
    return Settings(stockfish_depth=5, stockfish_top_moves=3)


class TestStockfishService:

    def test_analyse_starting_position(self, settings):
        service = StockfishService(settings)
        result = service.analyse(STARTING_FEN, depth=5, top_moves=3)

        assert result.fen == STARTING_FEN
        assert result.turn == "white"
        assert result.is_checkmate is False
        assert result.is_stalemate is False
        assert result.best_move.move is not None
        assert len(result.top_moves) > 0
        service.close()

    def test_analyse_after_e4(self, settings):
        service = StockfishService(settings)
        result = service.analyse(AFTER_E4_FEN, depth=5)

        assert result.turn == "black"
        assert result.best_move.move is not None
        service.close()

    def test_analyse_checkmate_position(self, settings):
        service = StockfishService(settings)
        result = service.analyse(CHECKMATE_FEN, depth=5)

        assert result.is_checkmate is True
        assert result.best_move.move == "none"
        service.close()

    def test_invalid_fen_raises(self, settings):
        service = StockfishService(settings)
        with pytest.raises(Exception):
            service.analyse("not_a_valid_fen", depth=5)
        service.close()

    def test_top_moves_count(self, settings):
        service = StockfishService(settings)
        result = service.analyse(STARTING_FEN, depth=5, top_moves=3)

        assert len(result.top_moves) <= 3
        service.close()
