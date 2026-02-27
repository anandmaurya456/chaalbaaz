import { AnalysisResult, GameSession } from './types';

export class ChaalbaazApiClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
  }

  async createSession(username: string): Promise<GameSession> {
    const res = await fetch(`${this.baseUrl}/api/v1/session`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ chessComUsername: username }),
    });

    if (!res.ok) throw new Error(`Failed to create session: ${res.status}`);
    const data = await res.json();
    return data.data as GameSession;
  }

  async analysePosition(sessionId: string, fen: string): Promise<AnalysisResult> {
    const res = await fetch(`${this.baseUrl}/api/v1/analysis/session/${sessionId}/analyse`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sessionId, fen }),
    });

    if (!res.ok) throw new Error(`Analysis failed: ${res.status}`);
    const data = await res.json();
    return data.data as AnalysisResult;
  }

  async healthCheck(): Promise<boolean> {
    try {
      const res = await fetch(`${this.baseUrl}/health`);
      return res.ok;
    } catch {
      return false;
    }
  }
}
