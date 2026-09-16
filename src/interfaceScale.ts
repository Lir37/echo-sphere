const INTERFACE_SCALE_KEY = 'echosphere_interface_scale';
const DEFAULT_SCALE = 1;
const MIN_SCALE = 0.75;
const MAX_SCALE = 1.25;
const STEP = 0.05;

export function loadInterfaceScale(): number {
  const raw = Number(localStorage.getItem(INTERFACE_SCALE_KEY));
  if (!Number.isFinite(raw)) return DEFAULT_SCALE;
  return Math.min(MAX_SCALE, Math.max(MIN_SCALE, raw));
}

export function saveInterfaceScale(value: number): void {
  const clamped = Math.min(MAX_SCALE, Math.max(MIN_SCALE, value));
  localStorage.setItem(INTERFACE_SCALE_KEY, String(clamped));
}

export function formatInterfaceScale(value: number): string {
  return `${Math.round(value * 100)}%`;
}

function installSettingsControl(): void {
  if (typeof document === 'undefined') return;

  const tryInstall = () => {
    if (document.querySelector('[data-interface-scale-settings="true"]')) return;

    const handednessLabel = Array.from(document.querySelectorAll('label')).find(
      (element) => element.textContent?.trim() === 'Расположение управления' || element.textContent?.trim() === 'Control layout',
    );
    if (!handednessLabel?.parentElement) return;

    const section = document.createElement('div');
    section.setAttribute('data-interface-scale-settings', 'true');
    section.innerHTML = `
      <label class="text-xs text-[#8a7a5a] uppercase tracking-wider mb-2 block">
        <span data-interface-scale-title></span>
      </label>
      <div class="bg-[#e8dcc0] rounded-xl p-4 border border-[#c4b890]">
        <div class="flex items-center gap-3">
          <span class="text-xs text-[#8a7a5a]">75%</span>
          <input
            data-interface-scale-range
            type="range"
            min="0.75"
            max="1.25"
            step="0.05"
            class="flex-1 accent-[#4a7a8a]"
          />
          <span class="text-xs text-[#8a7a5a]">125%</span>
        </div>
        <div class="text-center text-sm font-bold text-[#3a2e1f] mt-2" data-interface-scale-value></div>
      </div>
    `;

    const controlsLabel = Array.from(document.querySelectorAll('label')).find(
      (element) => element.textContent?.trim() === 'Управление' || element.textContent?.trim() === 'Controls',
    );
    const parent = controlsLabel?.parentElement?.parentElement;
    if (parent && parent.parentElement === handednessLabel.parentElement.parentElement) {
      parent.parentElement.insertBefore(section, parent);
    } else {
      handednessLabel.parentElement.parentElement.after(section);
    }

    const range = section.querySelector<HTMLInputElement>('[data-interface-scale-range]');
    const value = section.querySelector<HTMLElement>('[data-interface-scale-value]');
    const title = section.querySelector<HTMLElement>('[data-interface-scale-title]');
    if (!range || !value || !title) return;

    const updateUi = () => {
      const scale = loadInterfaceScale();
      range.value = String(scale);
      value.textContent = formatInterfaceScale(scale);
      const isEnglish = localStorage.getItem('echosphere_lang') === 'en';
      title.textContent = isEnglish ? 'Interface scale' : 'Масштаб интерфейса';
    };

    updateUi();
    range.addEventListener('input', () => {
      saveInterfaceScale(Number(range.value));
      updateUi();
    });
  };

  const observer = new MutationObserver(() => tryInstall());
  observer.observe(document.body, { childList: true, subtree: true });
  tryInstall();
}

installSettingsControl();
