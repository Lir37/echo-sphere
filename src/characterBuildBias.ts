import { ABILITIES, type AbilityType } from './gameData';
import { CHARACTER_DEFS, type CharacterId } from './characters';
import { loadCharacterId } from './persistence';

const INSTALL_KEY = '__echosphere_character_build_bias_installed__';
const PREFERRED_WEIGHT = 3;
const TOWER_MOD_IDS = new Set(['multishot', 'pierce', 'ricochet', 'fire', 'freeze', 'poison']);
const RANDOM_SHUFFLE_PATTERN = /Math\.random\(\)\s*-\s*0\.5/;

type TowerModId = 'multishot' | 'pierce' | 'ricochet' | 'fire' | 'freeze' | 'poison';

interface CharacterDefWithPreferences {
  preferredAbilities: AbilityType[];
  preferredTowerMods: TowerModId[];
}

function getCharacter(): CharacterDefWithPreferences | null {
  const id = loadCharacterId() as CharacterId;
  return CHARACTER_DEFS[id] || null;
}

function weightedPermutation<T>(items: T[], isPreferred: (item: T) => boolean): T[] {
  const ranked = items.map((item) => {
    const weight = isPreferred(item) ? PREFERRED_WEIGHT : 1;
    const u = Math.max(Number.EPSILON, Math.random());
    return { item, key: -Math.log(u) / weight };
  });

  ranked.sort((a, b) => a.key - b.key);
  const result = ranked.map(({ item }) => item);
  for (let index = 0; index < result.length; index++) items[index] = result[index];
  return items;
}

function isRandomShuffle(compareFn?: unknown): boolean {
  if (typeof compareFn !== 'function') return false;
  return RANDOM_SHUFFLE_PATTERN.test(Function.prototype.toString.call(compareFn));
}

function isAbilityPool(value: unknown[]): value is AbilityType[] {
  return value.length > 1 && value.every((item) => typeof item === 'string' && item in ABILITIES);
}

function isTowerPool(value: unknown[]): boolean {
  return value.length > 1 && value.every((item) => {
    if (!item || typeof item !== 'object') return false;
    const id = (item as { id?: unknown }).id;
    return typeof id === 'string' && TOWER_MOD_IDS.has(id);
  });
}

export function installCharacterBuildBias(): void {
  if (typeof window === 'undefined') return;

  const prototype = Array.prototype as Array<unknown> & Record<string, unknown>;
  if (prototype[INSTALL_KEY]) return;

  const originalSort = Array.prototype.sort;
  Array.prototype.sort = function <T>(this: T[], compareFn?: (a: T, b: T) => number): T[] {
    if (compareFn && isRandomShuffle(compareFn)) {
      const character = getCharacter();

      if (character && isAbilityPool(this as unknown[])) {
        return weightedPermutation(this, (item) => character.preferredAbilities.includes(item as AbilityType));
      }

      if (character && isTowerPool(this as unknown[])) {
        return weightedPermutation(this, (item) => {
          const id = (item as { id?: TowerModId }).id;
          return !!id && character.preferredTowerMods.includes(id);
        });
      }
    }

    return originalSort.call(this, compareFn);
  };

  prototype[INSTALL_KEY] = true;
}

installCharacterBuildBias();
