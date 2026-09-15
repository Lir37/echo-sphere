import type { ShopState, LeaderEntry } from './engine';

const GOLD_KEY = 'echosphere_gold';
const SHOP_KEY = 'echosphere_shop';
const LEADER_KEY = 'echosphere_leaderboard';
const LANG_KEY = 'echosphere_lang';
const NAME_KEY = 'echosphere_name';

export function loadGold(): number {
  return Number(localStorage.getItem(GOLD_KEY) || 0);
}
export function saveGold(g: number): void {
  localStorage.setItem(GOLD_KEY, String(Math.floor(g)));
}

export function loadShop(): ShopState {
  const raw = localStorage.getItem(SHOP_KEY);
  if (raw) {
    try {
      const parsed = JSON.parse(raw);
      return { gold: loadGold(), upgrades: parsed.upgrades || {} };
    } catch { /* ignore */ }
  }
  return { gold: loadGold(), upgrades: {} };
}
export function saveShop(shop: ShopState): void {
  localStorage.setItem(SHOP_KEY, JSON.stringify({ upgrades: shop.upgrades }));
}

export function loadLeaderboard(): LeaderEntry[] {
  const raw = localStorage.getItem(LEADER_KEY);
  if (raw) {
    try { return JSON.parse(raw); } catch { /* ignore */ }
  }
  return [];
}
export function saveLeaderboard(entries: LeaderEntry[]): void {
  localStorage.setItem(LEADER_KEY, JSON.stringify(entries));
}
export function addLeaderEntry(entry: LeaderEntry): { entries: LeaderEntry[]; rank: number; isNewRecord: boolean } {
  const entries = loadLeaderboard();
  const isNewRecord = entries.length === 0 || entry.time > entries[0].time;
  entries.push(entry);
  entries.sort((a, b) => b.time - a.time);
  const top = entries.slice(0, 10);
  saveLeaderboard(top);
  const rank = top.findIndex(e => e === entry) + 1;
  return { entries: top, rank, isNewRecord };
}

export function loadLang(): 'ru' | 'en' {
  const raw = localStorage.getItem(LANG_KEY);
  if (raw === 'ru' || raw === 'en') return raw;
  return navigator.language.startsWith('en') ? 'en' : 'ru';
}
export function saveLang(lang: 'ru' | 'en'): void {
  localStorage.setItem(LANG_KEY, lang);
}

export function loadName(): string {
  return localStorage.getItem(NAME_KEY) || '';
}
export function saveName(name: string): void {
  localStorage.setItem(NAME_KEY, name);
}

const ACH_KEY = 'echosphere_achievements';
const DIFF_KEY = 'echosphere_difficulty';
const SOUND_KEY = 'echosphere_sound';

export function loadAchievements(): string[] {
  const raw = localStorage.getItem(ACH_KEY);
  if (raw) {
    try { return JSON.parse(raw); } catch { /* ignore */ }
  }
  return [];
}
export function saveAchievements(ids: string[]): void {
  localStorage.setItem(ACH_KEY, JSON.stringify(ids));
}
export function unlockAchievement(id: string): boolean {
  const list = loadAchievements();
  if (list.includes(id)) return false;
  list.push(id);
  saveAchievements(list);
  return true;
}

export function loadDifficulty(): string {
  return localStorage.getItem(DIFF_KEY) || 'normal';
}
export function saveDifficulty(d: string): void {
  localStorage.setItem(DIFF_KEY, d);
}

export function loadSound(): boolean {
  const raw = localStorage.getItem(SOUND_KEY);
  return raw === null ? true : raw === '1';
}
export function saveSound(on: boolean): void {
  localStorage.setItem(SOUND_KEY, on ? '1' : '0');
}

export function resetAll(): void {
  localStorage.removeItem(GOLD_KEY);
  localStorage.removeItem(SHOP_KEY);
  localStorage.removeItem(LEADER_KEY);
  localStorage.removeItem(ACH_KEY);
  localStorage.removeItem(DIFF_KEY);
  localStorage.removeItem(SOUND_KEY);
}
