import { AnalysisResult, MoveEvaluation } from '../utils/types';

export class ChaalbaazOverlay {
  private container: HTMLElement | null = null;
  private visible: boolean = true;
  private boardEl: Element | null = null;

  public mount(board: Element): void {
    this.boardEl = board;

    // Create overlay container
    this.container = document.createElement('div');
    this.container.id = 'chaalbaaz-overlay';
    this.container.innerHTML = this.renderInitial();

    // Inject next to board
    board.parentElement?.appendChild(this.container);
    console.log('♟️ Chaalbaaz overlay mounted');
  }

  public unmount(): void {
    this.container?.remove();
    this.container = null;
  }

  public toggle(): void {
    this.visible = !this.visible;
    if (this.container) {
      this.container.style.display = this.visible ? 'flex' : 'none';
    }
  }

  public showLoading(): void {
    const body = this.container?.querySelector('#chaalbaaz-body');
    if (body) {
      body.innerHTML = `
        <div class="chaalbaaz-loading">
          <div class="chaalbaaz-spinner"></div>
          <span>Analysing position...</span>
        </div>`;
    }
  }

  public showSuggestion(result: AnalysisResult): void {
    if (!this.container) return;

    const body = this.container.querySelector('#chaalbaaz-body');
    if (!body) return;

    if (result.isCheckmate) {
      body.innerHTML = this.renderCheckmate(result.turn);
      return;
    }

    if (result.isStalemate) {
      body.innerHTML = `<div class="chaalbaaz-status">½ Stalemate</div>`;
      return;
    }

    body.innerHTML = `
      ${this.renderBestMove(result.bestMove, result.turn)}
      ${this.renderEvalBar(result.bestMove)}
      ${this.renderTopMoves(result.topMoves)}
      ${result.isCheck ? '<div class="chaalbaaz-check">⚠️ Check!</div>' : ''}
      ${result.cached ? '<div class="chaalbaaz-cached">⚡ Cached</div>' : ''}
    `;
  }

  private renderInitial(): string {
    return `
      <div class="chaalbaaz-header">
        <span class="chaalbaaz-logo">♟️ Chaalbaaz</span>
        <button class="chaalbaaz-close" id="chaalbaaz-close">✕</button>
      </div>
      <div class="chaalbaaz-body" id="chaalbaaz-body">
        <div class="chaalbaaz-waiting">Waiting for game to start...</div>
      </div>
      <div class="chaalbaaz-footer">
        <span class="chaalbaaz-disclaimer">⚠️ Practice games only</span>
      </div>
    `;
  }

  private renderBestMove(move: MoveEvaluation, turn: string): string {
    const evalDisplay = move.mateIn !== null
      ? `M${Math.abs(move.mateIn)}`
      : move.centipawnScore !== null
        ? `${move.centipawnScore > 0 ? '+' : ''}${(move.centipawnScore / 100).toFixed(2)}`
        : '?';

    const turnColor = turn === 'white' ? '♔' : '♚';

    return `
      <div class="chaalbaaz-best-move">
        <div class="chaalbaaz-best-label">${turnColor} Best Move</div>
        <div class="chaalbaaz-best-san">${move.moveSan}</div>
        <div class="chaalbaaz-eval ${move.centipawnScore && move.centipawnScore > 0 ? 'positive' : 'negative'}">
          ${evalDisplay}
        </div>
      </div>
      ${move.principalVariation.length > 0
        ? `<div class="chaalbaaz-pv">Line: ${move.principalVariation.slice(0, 5).join(' ')}</div>`
        : ''}
    `;
  }

  private renderEvalBar(move: MoveEvaluation): string {
    if (move.mateIn !== null) return '';

    const score = move.centipawnScore ?? 0;
    // Clamp to -1000/+1000 range, map to 0-100%
    const clamped = Math.max(-1000, Math.min(1000, score));
    const whitePercent = Math.round(((clamped + 1000) / 2000) * 100);

    return `
      <div class="chaalbaaz-eval-bar">
        <div class="chaalbaaz-eval-white" style="width: ${whitePercent}%"></div>
        <div class="chaalbaaz-eval-black" style="width: ${100 - whitePercent}%"></div>
      </div>
    `;
  }

  private renderTopMoves(moves: MoveEvaluation[]): string {
    if (moves.length <= 1) return '';

    const rows = moves.map((m, i) => {
      const eval_ = m.mateIn !== null
        ? `M${Math.abs(m.mateIn)}`
        : m.centipawnScore !== null
          ? `${m.centipawnScore > 0 ? '+' : ''}${(m.centipawnScore / 100).toFixed(2)}`
          : '?';

      return `
        <div class="chaalbaaz-move-row ${i === 0 ? 'best' : ''}">
          <span class="chaalbaaz-move-rank">${i + 1}</span>
          <span class="chaalbaaz-move-san">${m.moveSan}</span>
          <span class="chaalbaaz-move-eval">${eval_}</span>
          <span class="chaalbaaz-move-depth">d${m.depth}</span>
        </div>`;
    }).join('');

    return `<div class="chaalbaaz-top-moves">${rows}</div>`;
  }

  private renderCheckmate(turn: string): string {
    const winner = turn === 'white' ? 'Black' : 'White';
    return `<div class="chaalbaaz-status">♚ ${winner} wins by checkmate!</div>`;
  }
}
