import { MessageType, ExtensionMessage, AnalysisResult } from '../utils/types';
import { getSettings, getSessionId, saveSessionId } from '../utils/storage';
import { ChaalbaazApiClient } from '../utils/api';

let apiClient: ChaalbaazApiClient | null = null;
let sessionId: string | null = null;

// ---- Initialise on install / startup ----
chrome.runtime.onInstalled.addListener(async () => {
  console.log('♟️ Chaalbaaz installed');
  await initSession();
});

chrome.runtime.onStartup.addListener(async () => {
  await initSession();
});

// ---- Message handler from content script ----
chrome.runtime.onMessage.addListener(
  (message: ExtensionMessage, _sender, sendResponse) => {
    handleMessage(message, sendResponse);
    return true; // keep channel open for async response
  }
);

async function initSession(): Promise<void> {
  const settings = await getSettings();
  if (!settings.username || !settings.enabled) return;

  apiClient = new ChaalbaazApiClient(settings.apiBaseUrl);

  // Reuse existing session if available
  const existing = await getSessionId();
  if (existing) {
    sessionId = existing;
    console.log(`♟️ Chaalbaaz resumed session: ${sessionId}`);
    return;
  }

  try {
    const session = await apiClient.createSession(settings.username);
    sessionId = session.id;
    await saveSessionId(sessionId);
    console.log(`♟️ Chaalbaaz session created: ${sessionId}`);
  } catch (err) {
    console.error('Chaalbaaz: failed to create session', err);
  }
}

async function handleMessage(
  message: ExtensionMessage,
  sendResponse: (response: unknown) => void
): Promise<void> {
  switch (message.type) {

    case MessageType.FEN_CHANGED: {
      const fen = message.payload as string;
      if (!fen || !sessionId || !apiClient) {
        sendResponse({ success: false, error: 'Not initialised' });
        return;
      }

      try {
        const result = await apiClient.analysePosition(sessionId, fen);
        // Broadcast result back to content script
        const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
        if (tabs[0]?.id) {
          chrome.tabs.sendMessage(tabs[0].id, {
            type: MessageType.ANALYSIS_RESULT,
            payload: result,
          });
        }
        sendResponse({ success: true, data: result });
      } catch (err) {
        console.error('Chaalbaaz analysis error:', err);
        sendResponse({ success: false, error: String(err) });
      }
      break;
    }

    case MessageType.GET_SETTINGS: {
      const settings = await getSettings();
      sendResponse({ success: true, data: settings });
      break;
    }

    case MessageType.SETTINGS_UPDATED: {
      // Re-init session with new settings
      sessionId = null;
      await initSession();
      sendResponse({ success: true });
      break;
    }

    default:
      sendResponse({ success: false, error: 'Unknown message type' });
  }
}
