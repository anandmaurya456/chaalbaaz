import { MessageType } from '../utils/types';
import { getSettings, saveSettings } from '../utils/storage';
import { ChaalbaazApiClient } from '../utils/api';

async function initPopup(): Promise<void> {
  const settings = await getSettings();

  // Populate fields
  (document.getElementById('enabled') as HTMLInputElement).checked = settings.enabled;
  (document.getElementById('showEvalBar') as HTMLInputElement).checked = settings.showEvalBar;
  (document.getElementById('username') as HTMLInputElement).value = settings.username;
  (document.getElementById('apiBaseUrl') as HTMLInputElement).value = settings.apiBaseUrl;
  (document.getElementById('analysisDepth') as HTMLInputElement).value = String(settings.analysisDepth);

  // Check API health
  await checkHealth(settings.apiBaseUrl);

  // Save button
  document.getElementById('save-btn')?.addEventListener('click', async () => {
    const newSettings = {
      enabled: (document.getElementById('enabled') as HTMLInputElement).checked,
      showEvalBar: (document.getElementById('showEvalBar') as HTMLInputElement).checked,
      username: (document.getElementById('username') as HTMLInputElement).value.trim(),
      apiBaseUrl: (document.getElementById('apiBaseUrl') as HTMLInputElement).value.trim(),
      analysisDepth: parseInt((document.getElementById('analysisDepth') as HTMLInputElement).value),
    };

    await saveSettings(newSettings);

    // Notify background to re-init session
    chrome.runtime.sendMessage({ type: MessageType.SETTINGS_UPDATED });

    // Show saved message
    const msg = document.getElementById('saved-msg') as HTMLElement;
    msg.style.display = 'block';
    setTimeout(() => msg.style.display = 'none', 2000);

    // Re-check health with new URL
    await checkHealth(newSettings.apiBaseUrl);
  });
}

async function checkHealth(apiBaseUrl: string): Promise<void> {
  const dot = document.getElementById('status-dot') as HTMLElement;
  const text = document.getElementById('status-text') as HTMLElement;

  dot.className = 'status-dot';
  text.textContent = 'Checking connection...';

  const client = new ChaalbaazApiClient(apiBaseUrl);
  const healthy = await client.healthCheck();

  if (healthy) {
    dot.classList.add('connected');
    text.textContent = 'Connected to Chaalbaaz API ✓';
  } else {
    dot.classList.add('error');
    text.textContent = 'API unreachable — check URL';
  }
}

initPopup();
