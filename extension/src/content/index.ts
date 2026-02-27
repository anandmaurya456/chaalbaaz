import { MessageType, AnalysisResult } from '../utils/types';
import { ChaalbaazOverlay } from '../overlay/overlay';

class ChaalbaazContentScript {
  private overlay: ChaalbaazOverlay;
  private lastFen: string = '';
  private observer: MutationObserver | null = null;
  private pollInterval: ReturnType<typeof setInterval> | null = null;

  constructor() {
    this.overlay = new ChaalbaazOverlay();
    this.init();
  }

  private init(): void {
    console.log('♟️ Chaalbaaz content script loaded');

    // Listen for analysis results from background
    chrome.runtime.onMessage.addListener((message) => {
      if (message.type === MessageType.ANALYSIS_RESULT) {
        this.overlay.showSuggestion(message.payload as AnalysisResult);
      }
      if (message.type === MessageType.TOGGLE_OVERLAY) {
        this.overlay.toggle();
      }
    });

    // Wait for Chess.com board to load
    this.waitForBoard();
  }

  private waitForBoard(): void {
    const check = setInterval(() => {
      const board = this.getBoard();
      if (board) {
        clearInterval(check);
        console.log('♟️ Chaalbaaz: Chess.com board detected');
        this.overlay.mount(board);
        this.startMonitoring();
      }
    }, 1000);
  }

  private getBoard(): Element | null {
    // Chess.com uses chess-board component
    return (
      document.querySelector('chess-board') ||
      document.querySelector('.board') ||
      document.querySelector('[class*="board"]')
    );
  }

  private startMonitoring(): void {
    // Primary: MutationObserver watching piece moves
    const boardContainer = document.querySelector('.board-layout-main') ||
      document.querySelector('chess-board');

    if (boardContainer) {
      this.observer = new MutationObserver(() => {
        this.checkFenChange();
      });

      this.observer.observe(boardContainer, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class', 'style'],
      });
    }

    // Fallback: poll every 500ms
    this.pollInterval = setInterval(() => {
      this.checkFenChange();
    }, 500);
  }

  private checkFenChange(): void {
    const fen = this.extractFen();
    if (!fen || fen === this.lastFen) return;

    this.lastFen = fen;
    console.log('♟️ Chaalbaaz: FEN changed →', fen);

    this.overlay.showLoading();

    // Send to background for analysis
    chrome.runtime.sendMessage({
      type: MessageType.FEN_CHANGED,
      payload: fen,
    });
  }

  private extractFen(): string | null {
    // Method 1: Chess.com exposes FEN via board element attribute
    const board = document.querySelector('chess-board');
    if (board) {
      const fen = board.getAttribute('fen');
      if (fen) return fen;
    }

    // Method 2: Read from Chess.com's internal game object
    try {
      const gameObj = (window as any).ChessComGame ||
        (window as any).game ||
        (window as any).chessboard;

      if (gameObj?.getFen) return gameObj.getFen();
      if (gameObj?.fen) return gameObj.fen();
    } catch {
      // silent fail
    }

    // Method 3: Parse piece positions from DOM (fallback)
    return this.extractFenFromDom();
  }

  private extractFenFromDom(): string | null {
    // Map Chess.com piece classes to FEN characters
    const pieceMap: Record<string, string> = {
      'wp': 'P', 'wr': 'R', 'wn': 'N', 'wb': 'B', 'wq': 'Q', 'wk': 'K',
      'bp': 'p', 'br': 'r', 'bn': 'n', 'bb': 'b', 'bq': 'q', 'bk': 'k',
    };

    const board: Record<number, string> = {};

    document.querySelectorAll('[class*="piece"]').forEach((el) => {
      const classes = el.className.split(' ');
      const pieceClass = classes.find(c => pieceMap[c]);
      const squareClass = classes.find(c => /^square-\d{2}$/.test(c));

      if (!pieceClass || !squareClass) return;

      const sq = parseInt(squareClass.replace('square-', ''));
      const file = sq % 10;   // 1-8
      const rank = Math.floor(sq / 10); // 1-8
      const index = (8 - rank) * 8 + (file - 1);
      board[index] = pieceMap[pieceClass];
    });

    if (Object.keys(board).length === 0) return null;

    // Build FEN rank by rank
    let fen = '';
    for (let rank = 0; rank < 8; rank++) {
      let empty = 0;
      for (let file = 0; file < 8; file++) {
        const piece = board[rank * 8 + file];
        if (piece) {
          if (empty > 0) { fen += empty; empty = 0; }
          fen += piece;
        } else {
          empty++;
        }
      }
      if (empty > 0) fen += empty;
      if (rank < 7) fen += '/';
    }

    // Append minimal FEN suffix (turn detection TBD)
    return `${fen} w KQkq - 0 1`;
  }

  public destroy(): void {
    this.observer?.disconnect();
    if (this.pollInterval) clearInterval(this.pollInterval);
    this.overlay.unmount();
  }
}

// Boot
const chaalbaaz = new ChaalbaazContentScript();
