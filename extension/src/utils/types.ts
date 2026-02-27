// ============================================
// Chaalbaaz Extension — Shared Types
// ============================================

export interface MoveEvaluation {
  move: string;        // UCI: e2e4
  moveSan: string;     // SAN: e4
  centipawnScore: number | null;
  mateIn: number | null;
  depth: number;
  principalVariation: string[];
}

export interface AnalysisResult {
  fen: string;
  bestMove: MoveEvaluation;
  topMoves: MoveEvaluation[];
  turn: 'white' | 'black';
  isCheck: boolean;
  isCheckmate: boolean;
  isStalemate: boolean;
  cached: boolean;
  analysedAt: string;
}

export interface GameSession {
  id: string;
  chessComUsername: string;
  currentFen: string;
  gameId: string;
  status: string;
  createdAt: string;
}

export interface ExtensionSettings {
  apiBaseUrl: string;
  enabled: boolean;
  username: string;
  analysisDepth: number;
  topMovesCount: number;
  showEvalBar: boolean;
  showArrows: boolean;
}

export interface ExtensionMessage {
  type: MessageType;
  payload?: unknown;
}

export enum MessageType {
  FEN_CHANGED = 'FEN_CHANGED',
  ANALYSIS_RESULT = 'ANALYSIS_RESULT',
  SESSION_CREATED = 'SESSION_CREATED',
  SETTINGS_UPDATED = 'SETTINGS_UPDATED',
  TOGGLE_OVERLAY = 'TOGGLE_OVERLAY',
  GET_SETTINGS = 'GET_SETTINGS',
}
