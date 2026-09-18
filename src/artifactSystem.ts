// Echo Sphere artifact system v1
// Artifact architecture: weighted rarities, build-defining mechanics, and tower/network synergies.

import type { ArtifactId } from './gameData';

export type ArtifactRarity = 'common' | 'rare' | 'epic' | 'special' | 'legendary';

export interface ArtifactEffects {
  moveSpeed?: number;
  maxHp?: number;
  xpGain?: number;
  regen?: number;
  sphereRadius?: number;
  sphereDamage?: number;
  cooldown?: number;
  damageTakenReduction?: number;
  critChance?: number;
  dodgeChance?: number;
  vampire?: number;
  towerDamage?: number;
  towerDelay?: number;
  towerRadius?: number;
  standardDamage?: number;
  sniperDamage?: number;
  shotgunDamage?: number;
  chainDamage?: number;
  auraDamage?: number;
  standardRadius?: number;
  sniperRadius?: number;
  shotgunRadius?: number;
  chainRadius?: number;
  auraRadius?: number;
}

export interface ArtifactMeta {
  id: ArtifactId;
  rarity: ArtifactRarity;
  effects: ArtifactEffects;
  mechanic?:
    | 'swift'
    | 'mirror'
    | 'resonance'
    | 'lone'
    | 'fivefold'
    | 'relay'
    | 'triangle'
    | 'overclock'
    | 'network'
    | 'singularity'
    | 'zero'
    | 'unified';
}

const meta = (id: ArtifactId, rarity: ArtifactRarity, effects: ArtifactEffects = {}, mechanic?: ArtifactMeta['mechanic']): ArtifactMeta => ({ id, rarity, effects, mechanic });

export const ARTIFACT_METADATA: ArtifactMeta[] = [
  meta('crystal_speed', 'common', { moveSpeed: 0.15 }),
  meta('amulet_hp', 'common', { maxHp: 30 }),
  meta('ring_xp', 'common', { xpGain: 0.20 }),
  meta('regen_stone', 'common', { regen: 1 }),
  meta('radius_shard', 'common', { sphereRadius: 0.10 }),
  meta('vampire_ring', 'common', { vampire: 0.03 }),
  meta('swift_boots', 'common', {}, 'swift'),
  meta('luck_talisman', 'common', { critChance: 0.15 }),
  meta('veil_cloak', 'common', {}),
  meta('mage_pendant', 'common', { cooldown: -0.10 }),
  meta('long_lens', 'common', { sniperRadius: 0.18 }),
  meta('stasis_core', 'common', {}),
  meta('dragon_heart', 'rare', { maxHp: 50, moveSpeed: -0.10 }),
  meta('mirror', 'rare', {}, 'mirror'),
  meta('predator_claw', 'rare', { sphereDamage: 0.08 }),
  meta('foresight_eye', 'rare', { critChance: 0.10, sniperDamage: 0.08 }),
  meta('echo_conductor', 'rare', { chainDamage: 0.12 }),
  meta('heavy_core', 'rare', { standardDamage: 0.15 }),
  meta('scattering_matrix', 'rare', { shotgunDamage: 0.12 }),
  meta('aura_lens', 'rare', { auraRadius: 0.18 }),
  meta('network_relay', 'rare', { towerDamage: 0.06 }, 'network'),
  meta('chaos_orb', 'rare', {}),
  meta('resonance_core', 'epic', { towerDamage: 0.05 }, 'resonance'),
  meta('lone_bastion', 'epic', { towerDamage: 0.08 }, 'lone'),
  meta('fivefold_resonance', 'epic', { towerDamage: 0.02 }, 'fivefold'),
  meta('relay_matrix', 'epic', { towerDamage: 0.04 }, 'relay'),
  meta('triangle_circuit', 'epic', { towerDamage: 0.04 }, 'triangle'),
  meta('overclock', 'epic', { towerDamage: 0.03, towerDelay: -0.12 }, 'overclock'),
  meta('soul_engine', 'epic', { sphereDamage: 0.05, xpGain: 0.10 }),
  meta('time_anchor', 'epic', { cooldown: -0.12 }),
  meta('void_contract', 'special', { sphereDamage: 0.12, towerDamage: 0.10, damageTakenReduction: -0.15 }),
  meta('mirror_network', 'special', { towerDamage: 0.08 }, 'network'),
  meta('singularity_engine', 'special', { towerDamage: 0.10 }, 'singularity'),
  meta('quantum_core', 'special', { sphereDamage: 0.08, dodgeChance: 0.08 }),
  meta('zero_sphere', 'legendary', { towerDamage: 0.15, sphereDamage: 0.10 }, 'zero'),
  meta('unified_mind', 'legendary', { towerDamage: 0.08, sphereDamage: 0.08 }, 'unified'),
];

export const ARTIFACT_META: Record<ArtifactId, ArtifactMeta> = Object.fromEntries(
  ARTIFACT_METADATA.map((item) => [item.id, item]),
) as Record<ArtifactId, ArtifactMeta>;

export const RARITY_ORDER: ArtifactRarity[] = ['common', 'rare', 'epic', 'special', 'legendary'];
export const RARITY_LABELS: Record<ArtifactRarity, { ru: string; en: string }> = {
  common: { ru: 'Обычный', en: 'Common' },
  rare: { ru: 'Редкий', en: 'Rare' },
  epic: { ru: 'Эпический', en: 'Epic' },
  special: { ru: 'Особый', en: 'Special' },
  legendary: { ru: 'Легендарный', en: 'Legendary' },
};

export function getArtifactMeta(id: ArtifactId): ArtifactMeta {
  return ARTIFACT_META[id];
}

export interface ArtifactSynergy {
  id: string;
  requires: ArtifactId[];
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const ARTIFACT_SYNERGIES: ArtifactSynergy[] = [
  { id: 'fortress_network', requires: ['heavy_core', 'resonance_core'], name: { ru: 'Крепостная сеть', en: 'Fortress Network' }, desc: { ru: 'Соседние башни получают +10% урона.', en: 'Nearby towers gain +10% damage.' } },
  { id: 'echo_relay', requires: ['echo_conductor', 'network_relay'], name: { ru: 'Эхо-реле', en: 'Echo Relay' }, desc: { ru: 'Сеть из 2+ типов башен получает ещё +8% урона.', en: 'A network with 2+ tower types gains another +8% damage.' } },
  { id: 'glass_cannon', requires: ['overclock', 'void_contract'], name: { ru: 'Стеклянная пушка', en: 'Glass Cannon' }, desc: { ru: 'Башни получают +12% урона, но персонаж получает ещё +5% входящего урона.', en: 'Towers gain +12% damage, but the player takes 5% more damage.' } },
  { id: 'singularity', requires: ['lone_bastion', 'singularity_engine'], name: { ru: 'Сингулярность', en: 'Singularity' }, desc: { ru: 'При 1 типе башни её урон увеличивается ещё на 20%.', en: 'With one tower type, its damage is increased by another 20%.' } },
  { id: 'perfect_network', requires: ['fivefold_resonance', 'triangle_circuit'], name: { ru: 'Идеальная сеть', en: 'Perfect Network' }, desc: { ru: 'При 3+ типах башен геометрические связи дают +10% урона.', en: 'With 3+ tower types, geometric links grant +10% damage.' } },
  { id: 'unified_core', requires: ['unified_mind', 'zero_sphere'], name: { ru: 'Единое ядро', en: 'Unified Core' }, desc: { ru: 'Максимально прокачанная башня усиливает остальные на 5% за уровень.', en: 'The strongest tower boosts the others by 5% per level.' } },
];

export function getActiveArtifactSynergies(s: { player: { artifacts: ArtifactId[] } }): ArtifactSynergy[] {
  const owned = new Set(s.player.artifacts);
  return ARTIFACT_SYNERGIES.filter((synergy) => synergy.requires.every((id) => owned.has(id)));
}


export function artifactRarity(id: ArtifactId): ArtifactRarity {
  return ARTIFACT_META[id].rarity;
}

export function hasArtifact(s: { player: { artifacts: ArtifactId[] } }, id: ArtifactId): boolean {
  return s.player.artifacts.includes(id);
}

function effectSum(s: { player: { artifacts: ArtifactId[] } }, key: keyof ArtifactEffects): number {
  let total = 0;
  for (const id of s.player.artifacts) total += ARTIFACT_META[id]?.effects[key] || 0;
  return total;
}

export function getArtifactMoveSpeedMultiplier(s: any): number { return Math.max(0.5, 1 + effectSum(s, 'moveSpeed')); }
export function getArtifactMaxHpBonus(id: ArtifactId): number { return ARTIFACT_META[id]?.effects.maxHp || 0; }
export function getArtifactXpMultiplier(s: any): number { return Math.max(0.5, 1 + effectSum(s, 'xpGain')); }
export function getArtifactRegenPerSecond(s: any): number { return Math.max(0, effectSum(s, 'regen')); }
export function getArtifactSphereRadiusMultiplier(s: any): number { return Math.max(0.5, 1 + effectSum(s, 'sphereRadius')); }
export function getArtifactSphereDamageMultiplier(s: any): number { return Math.max(0.25, 1 + effectSum(s, 'sphereDamage')); }
export function getArtifactCooldownMultiplier(s: any): number { return Math.max(0.55, 1 + effectSum(s, 'cooldown')); }
export function getArtifactSphereDelayMultiplier(s: any): number { return Math.max(0.5, 1 + effectSum(s, 'towerDelay')); }
export function getArtifactDamageTakenMultiplier(s: any): number {\n  let multiplier = Math.max(0.45, 1 - effectSum(s, 'damageTakenReduction'));\n  if (getActiveArtifactSynergies(s).some((x) => x.id === 'glass_cannon')) multiplier *= 1.05;\n  return multiplier;\n}
export function getArtifactCritChanceBonus(s: any): number { return effectSum(s, 'critChance'); }
export function getArtifactDodgeChanceBonus(s: any): number { return effectSum(s, 'dodgeChance'); }
export function getArtifactVampireBonus(s: any): number { return effectSum(s, 'vampire'); }
export function getArtifactReflectChance(s: any): number { return hasArtifact(s, 'mirror') ? 0.20 : 0; }

function uniqueTowerCount(s: any): number {
  return new Set((s.spheres || []).filter((sphere: any) => sphere.alive !== false).map((sphere: any) => sphere.type)).size;
}

function hasNearbyTower(s: any, sphere: any, maxDistance: number): boolean {
  if (!sphere) return false;
  return (s.spheres || []).some((other: any) => other !== sphere && other.alive !== false && Math.hypot(other.pos.x - sphere.pos.x, other.pos.y - sphere.pos.y) <= maxDistance);
}

function formsTriangle(s: any, sphere: any): boolean {
  const others = (s.spheres || []).filter((other: any) => other !== sphere && other.alive !== false);
  for (let i = 0; i < others.length; i++) {
    for (let j = i + 1; j < others.length; j++) {
      const a = others[i];
      const b = others[j];
      const ab = Math.hypot(a.pos.x - b.pos.x, a.pos.y - b.pos.y);
      const as = Math.hypot(a.pos.x - sphere.pos.x, a.pos.y - sphere.pos.y);
      const bs = Math.hypot(b.pos.x - sphere.pos.x, b.pos.y - sphere.pos.y);
      if (ab <= 300 && as <= 300 && bs <= 300) return true;
    }
  }
  return false;
}

export function getTowerArtifactModifiers(s: any, type: string, sphere?: any): { damage: number; delay: number; radius: number } {
  let damage = 1 + effectSum(s, 'towerDamage');
  let delay = Math.max(0.55, 1 + effectSum(s, 'towerDelay'));
  let radius = Math.max(0.6, 1 + effectSum(s, 'towerRadius'));
  const effects: keyof ArtifactEffects = `${type}Damage` as keyof ArtifactEffects;
  const radiusEffect: keyof ArtifactEffects = `${type}Radius` as keyof ArtifactEffects;
  damage *= 1 + effectSum(s, effects);
  radius *= 1 + effectSum(s, radiusEffect);

  const unique = uniqueTowerCount(s);
  const synergies = getActiveArtifactSynergies(s);
  if (synergies.some((x) => x.id === 'fortress_network') && sphere && hasNearbyTower(s, sphere, 190)) damage *= 1.10;
  if (synergies.some((x) => x.id === 'echo_relay') && unique >= 2) damage *= 1.08;
  if (synergies.some((x) => x.id === 'glass_cannon')) damage *= 1.12;
  if (synergies.some((x) => x.id === 'singularity') && unique <= 1) damage *= 1.20;
  if (synergies.some((x) => x.id === 'perfect_network') && unique >= 3 && sphere && formsTriangle(s, sphere)) damage *= 1.10;
  if (hasArtifact(s, 'lone_bastion') && unique <= 1) damage *= 1.30;
  if (hasArtifact(s, 'fivefold_resonance') && unique > 1) damage *= 1 + Math.min(5, unique) * 0.04;
  if (hasArtifact(s, 'network_relay') && unique >= 2) damage *= 1.05;
  if (hasArtifact(s, 'relay_matrix') && Object.values(s.player.towerProgression || {}).some((level: any) => level >= 7)) damage *= 1.10;
  if (hasArtifact(s, 'singularity_engine') && unique <= 2) damage *= 1.18;
  if (hasArtifact(s, 'mirror_network') && unique >= 2) damage *= 1.12;
  if (hasArtifact(s, 'overclock')) { damage *= 0.99; delay *= 0.88; }\n\n  if (synergies.some((x) => x.id === 'unified_core') && sphere) {\n    const levels = Object.values(s.player.towerProgression || {}) as number[];\n    const strongest = Math.max(0, ...levels);\n    const sphereLevel = Number(s.player.towerProgression?.[sphere.type] || 0);\n    if (sphereLevel < strongest) damage *= 1 + strongest * 0.05;\n  }
  return { damage, delay, radius };
}

export function getSphereArtifactDamageMultiplier(s: any, sphere: any): number {
  let multiplier = 1;
  if (hasArtifact(s, 'resonance_core') && hasNearbyTower(s, sphere, 190)) multiplier *= 1.12;
  if (hasArtifact(s, 'triangle_circuit') && formsTriangle(s, sphere)) multiplier *= 1.15;
  if (hasArtifact(s, 'unified_mind')) {
    const levels = Object.values(s.player.towerProgression || {}) as number[];
    const strongest = Math.max(0, ...levels);
    multiplier *= 1 + strongest * 0.03;
  }
  if (hasArtifact(s, 'zero_sphere')) multiplier *= 1.20;
  return multiplier;
}

const rarityWeight = (rarity: ArtifactRarity): number => ({ common: 62, rare: 24, epic: 9, special: 4, legendary: 1 }[rarity]);

function randomWeighted<T>(items: T[], weight: (item: T) => number): T {
  const total = items.reduce((sum, item) => sum + Math.max(0, weight(item)), 0);
  if (total <= 0) return items[Math.floor(Math.random() * items.length)];
  let roll = Math.random() * total;
  for (const item of items) {
    roll -= Math.max(0, weight(item));
    if (roll <= 0) return item;
  }
  return items[items.length - 1];
}

export function pickArtifactChoices(s: { player: { artifacts: ArtifactId[] } }, count = 3): ArtifactId[] {
  const owned = new Set(s.player.artifacts);
  const available = ARTIFACT_METADATA.filter((item) => !owned.has(item.id));
  const result: ArtifactId[] = [];
  const pool = [...available];
  while (result.length < count && pool.length > 0) {
    const chosen = randomWeighted(pool, (item) => rarityWeight(item.rarity));
    result.push(chosen.id);
    pool.splice(pool.indexOf(chosen), 1);
  }
  return result;
}
