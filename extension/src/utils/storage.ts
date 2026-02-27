import { ExtensionSettings } from './types';

export const DEFAULT_SETTINGS: ExtensionSettings = {
  apiBaseUrl: 'http://localhost:5000',
  enabled: true,
  username: '',
  analysisDepth: 20,
  topMovesCount: 3,
  showEvalBar: true,
  showArrows: true,
};

export async function getSettings(): Promise<ExtensionSettings> {
  return new Promise((resolve) => {
    chrome.storage.sync.get(DEFAULT_SETTINGS, (items) => {
      resolve(items as ExtensionSettings);
    });
  });
}

export async function saveSettings(settings: Partial<ExtensionSettings>): Promise<void> {
  return new Promise((resolve) => {
    chrome.storage.sync.set(settings, resolve);
  });
}

export async function getSessionId(): Promise<string | null> {
  return new Promise((resolve) => {
    chrome.storage.session.get(['sessionId'], (items) => {
      resolve(items['sessionId'] ?? null);
    });
  });
}

export async function saveSessionId(sessionId: string): Promise<void> {
  return new Promise((resolve) => {
    chrome.storage.session.set({ sessionId }, resolve);
  });
}
