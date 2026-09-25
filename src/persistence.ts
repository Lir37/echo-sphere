import type { ShopState, LeaderEntry } from './engine';
import { CHARACTER_DEFS, CHARACTER_LIST, DEFAULT_CHARACTER_ID, type CharacterId, type CharacterProfile } from './characters';

const GOLD_KEY = 'echosphere_gold';
const SHOP_KEY = 'echosphere_shop';
const LEADER_KEY = 'echosphere_leaderboard';
const LANG_KEY = 'echosphere_lang';
const NAME_KEY = 'echosphere_name';
const HANDEDNESS_KEY = 'echosphere_handedness';
const CHARACTER_KEY = 'echosphere_character';
const CHARACTER_PROFILES_KEY = 'echosphere_character_profiles';
const KNOWLEDGE_KEY = 'echosphere_knowledge_v1';

export type Handedness = 'right' | 'left';

export const CHARACTER_MASTERY_THRESHOLDS = [0, 250, 750, 1500, 2500] as const;

type PersistedCharacterProfile = CharacterProfile & { masteryXp: number };

export function getCharacterMasteryLevelForXp(xp: number): number {
  let level = 1;
  for (let index = 1; index < CHARACTER_MASTERY_THRESHOLDS.length; index++) {
    if (xp >= CHARACTER_MASTERY_THRESHOLDS[index]) level = index + 1;
  }
  return level;
}

export function getCharacterMasteryNextThreshold(level: number): number | null {
  return level >= 5 ? null : CHARACTER_MASTERY_THRESHOLDS[level];
}



export type KnowledgeId = string;

export function loadKnowledge(): KnowledgeId[] {
  const raw = localStorage.getItem(KNOWLEDGE_KEY);
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw) as unknown;
    return Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : [];
  } catch {
    return [];
  }
}

export function saveKnowledge(ids: KnowledgeId[]): void {
  localStorage.setItem(KNOWLEDGE_KEY, JSON.stringify(Array.from(new Set(ids))));
}

export function unlockKnowledge(ids: KnowledgeId[]): boolean {
  if (ids.length === 0) return false;
  const current = new Set(loadKnowledge());
  let changed = false;
  for (const id of ids) {
    if (!current.has(id)) {
      current.add(id);
      changed = true;
    }
  }
  if (changed) saveKnowledge([...current]);
  return changed;
}

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

export function loadHandedness(): Handedness {
  const raw = localStorage.getItem(HANDEDNESS_KEY);
  return raw === 'left' ? 'left' : 'right';
}
export function saveHandedness(value: Handedness): void {
  localStorage.setItem(HANDEDNESS_KEY, value);
}

export function loadCharacterId(): CharacterId {
  const raw = localStorage.getItem(CHARACTER_KEY);
  return raw && raw in CHARACTER_DEFS ? raw as CharacterId : DEFAULT_CHARACTER_ID;
}

export function saveCharacterId(id: CharacterId): void {
  if (!(id in CHARACTER_DEFS)) return;
  localStorage.setItem(CHARACTER_KEY, id);
}

export function loadCharacterProfiles(): PersistedCharacterProfile[] {
  const raw = localStorage.getItem(CHARACTER_PROFILES_KEY);
  const defaults: PersistedCharacterProfile[] = CHARACTER_LIST.map((character) => ({
    id: character.id,
    masteryLevel: 1,
    masteryXp: 0,
    unlocked: character.id === DEFAULT_CHARACTER_ID,
  }));
  if (!raw) return defaults;
  try {
    const parsed = JSON.parse(raw) as unknown;
    if (!Array.isArray(parsed)) return defaults;
    const byId = new Map(defaults.map((profile) => [profile.id, profile]));
    for (const item of parsed) {
      if (!item || typeof item !== 'object') continue;
      const candidate = item as Partial<CharacterProfile> & { masteryXp?: unknown };
      if (!candidate.id || !(candidate.id in CHARACTER_DEFS)) continue;
      const base = byId.get(candidate.id as CharacterId);
      if (!base) continue;
      base.unlocked = candidate.id === DEFAULT_CHARACTER_ID || candidate.unlocked === true;

      const legacyLevel = Math.max(1, Math.min(5, Number(candidate.masteryLevel) || 1));
      const legacyXp = CHARACTER_MASTERY_THRESHOLDS[legacyLevel - 1];
      const storedXp = Number(candidate.masteryXp);
      base.masteryXp = Math.min(
        CHARACTER_MASTERY_THRESHOLDS[CHARACTER_MASTERY_THRESHOLDS.length - 1],
        Math.max(legacyXp, Number.isFinite(storedXp) ? Math.max(0, storedXp) : 0),
      );
      base.masteryLevel = Math.max(legacyLevel, getCharacterMasteryLevelForXp(base.masteryXp));
    }
    return defaults;
  } catch {
    return defaults;
  }
}

export function saveCharacterProfiles(profiles: CharacterProfile[]): void {
  localStorage.setItem(CHARACTER_PROFILES_KEY, JSON.stringify(profiles));
}

export function isCharacterUnlocked(id: CharacterId): boolean {
  return loadCharacterProfiles().some((profile) => profile.id === id && profile.unlocked);
}

export function unlockCharacter(id: CharacterId): boolean {
  const profiles = loadCharacterProfiles();
  const profile = profiles.find((item) => item.id === id);
  if (!profile || profile.unlocked) return false;
  profile.unlocked = true;
  saveCharacterProfiles(profiles);
  return true;
}

export function setCharacterMasteryLevel(id: CharacterId, level: number): void {
  const profiles = loadCharacterProfiles();
  const profile = profiles.find((item) => item.id === id);
  if (!profile) return;
  const clamped = Math.max(1, Math.min(5, Math.floor(level)));
  profile.masteryLevel = clamped;
  profile.masteryXp = Math.max(profile.masteryXp, CHARACTER_MASTERY_THRESHOLDS[clamped - 1]);
  saveCharacterProfiles(profiles);
}

export function addCharacterMasteryXp(id: CharacterId, amount: number): {
  gainedXp: number;
  totalXp: number;
  previousLevel: number;
  level: number;
} {
  const profiles = loadCharacterProfiles();
  const profile = profiles.find((item) => item.id === id);
  if (!profile) {
    return { gainedXp: 0, totalXp: 0, previousLevel: 1, level: 1 };
  }

  const maxXp = CHARACTER_MASTERY_THRESHOLDS[CHARACTER_MASTERY_THRESHOLDS.length - 1];
  const gainedXp = Math.max(0, Math.min(Math.floor(amount), maxXp - profile.masteryXp));
  const previousLevel = profile.masteryLevel;
  profile.masteryXp = Math.min(maxXp, profile.masteryXp + gainedXp);
  profile.masteryLevel = Math.max(profile.masteryLevel, getCharacterMasteryLevelForXp(profile.masteryXp));
  saveCharacterProfiles(profiles);

  return {
    gainedXp,
    totalXp: profile.masteryXp,
    previousLevel,
    level: profile.masteryLevel,
  };
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
  localStorage.removeItem(CHARACTER_KEY);
  localStorage.removeItem(CHARACTER_PROFILES_KEY);
  localStorage.removeItem(HANDEDNESS_KEY);
  localStorage.removeItem(LANG_KEY);
  localStorage.removeItem(NAME_KEY);
  localStorage.removeItem(KNOWLEDGE_KEY);
}
