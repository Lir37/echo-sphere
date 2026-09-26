// Echo Sphere artifact system v1
// Artifact architecture: weighted rarities, build-defining mechanics, and sphere/network synergies.

import type { ArtifactId } from './gameData';

export type ArtifactRarity = 'common' | 'rare' | 'epic' | 'special' | 'legendary';

export interface ArtifactEffects {
  moveSpeed?: number;
  maxHp?: number;
  xpGain?: number;
  regen?: number;
  sphereRadius?: number;
  sphereDamage?: number;
  sphereDelay?: number;
  cooldown?: number;
  damageTakenReduction?: number;
  critChance?: number;
  dodgeChance?: number;
  vampire?: number;
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
  orbitalDamage?: number;
  prismDamage?: number;
  gravityDamage?: number;
  gravityRadius?: number;
  pulseDamage?: number;
  pulseRadius?: number;
  voidDamage?: number;
  voidWeakened?: number;
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
  meta('network_relay', 'rare', { sphereDamage: 0.06 }, 'network'),
  meta('chaos_orb', 'rare', {}),
  meta('resonance_core', 'epic', { sphereDamage: 0.05 }, 'resonance'),
  meta('lone_bastion', 'epic', { sphereDamage: 0.08 }, 'lone'),
  meta('fivefold_resonance', 'epic', { sphereDamage: 0.02 }, 'fivefold'),
  meta('relay_matrix', 'epic', { sphereDamage: 0.04 }, 'relay'),
  meta('triangle_circuit', 'epic', { sphereDamage: 0.04 }, 'triangle'),
  meta('overclock', 'epic', { sphereDamage: 0.03, sphereDelay: -0.12 }, 'overclock'),
  meta('soul_engine', 'epic', { sphereDamage: 0.05, xpGain: 0.10 }),
  meta('time_anchor', 'epic', { cooldown: -0.12 }),
  meta('void_contract', 'special', { sphereDamage: 0.22, damageTakenReduction: -0.15 }),
  meta('mirror_network', 'special', { sphereDamage: 0.08 }, 'network'),
  meta('singularity_engine', 'special', { sphereDamage: 0.10 }, 'singularity'),
  meta('quantum_core', 'special', { sphereDamage: 0.08, dodgeChance: 0.08 }),
  meta('zero_sphere', 'legendary', { sphereDamage: 0.35 }, 'zero'),
  meta('unified_mind', 'legendary', { sphereDamage: 0.16 }, 'unified'),
];

// Extended catalog: the first pass gives every new artifact a stable rarity and baseline effect.
// Individual mechanic handlers can specialize these entries without changing the catalog contract.
ARTIFACT_METADATA.push(
  meta('network_anchor', 'common', {"sphereDamage":0.04}),
  meta('pulse_lens', 'common', {"pulseRadius":0.10}),
  meta('orbit_charm', 'common', {"orbitalDamage":0.12}),
  meta('prism_shard', 'common', {"prismDamage":0.10}),
  meta('gravity_bead', 'common', {}),
  meta('void_ink', 'common', {"voidDamage":0.10}),
  meta('echo_thread', 'common', {"sphereDamage":0.04}),
  meta('folded_core', 'common', {"sphereDamage":0.04}),
  meta('paper_ward', 'common', {"sphereDamage":0.04}),
  meta('mirror_dust', 'common', {"sphereDamage":0.04}),
  meta('resonant_leaf', 'common', {"sphereDamage":0.04}),
  meta('signal_knot', 'common', {"sphereDamage":0.04}),
  meta('lattice_chip', 'common', {"sphereDamage":0.04}),
  meta('fractal_seed', 'common', {"sphereDamage":0.04}),
  meta('dash_relay', 'common', {"sphereDamage":0.04}),
  meta('sniper_scope', 'rare', {"sphereDamage":0.06}),
  meta('chain_battery', 'rare', {"sphereDamage":0.06}),
  meta('shotgun_shell', 'rare', {"sphereDamage":0.06}),
  meta('aura_mist', 'rare', {"sphereDamage":0.06}),
  meta('orbital_blade', 'rare', {"orbitalDamage":0.10}),
  meta('prism_filter', 'rare', {}),
  meta('gravity_hook', 'rare', {}),
  meta('pulse_driver', 'rare', {}),
  meta('void_mark', 'rare', {"voidWeakened":0.10}),
  meta('formation_compass', 'rare', {"sphereDamage":0.06}),
  meta('geometry_die', 'rare', {"sphereDamage":0.06}),
  meta('network_coil', 'rare', {"sphereDamage":0.06}),
  meta('crit_sigil', 'rare', {"sphereDamage":0.06}),
  meta('tempo_ring', 'rare', {"sphereDamage":0.06}),
  meta('stella_map', 'rare', {"sphereDamage":0.06}),
  meta('triangle_engine', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('ring_engine', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('lattice_engine', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('fractal_engine', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('resonance_lattice', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('sphere_forge', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('echo_weaver', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('overdrive_matrix', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('gravity_crown', 'epic', {"gravityRadius":0.20}),
  meta('void_lantern', 'epic', {}),
  meta('prism_crown', 'epic', {}),
  meta('orbital_crown', 'epic', {}),
  meta('pulse_crown', 'epic', {}),
  meta('chain_crown', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('sniper_crown', 'epic', {"sphereDamage":0.08,"cooldown":-0.03}),
  meta('singularity_seed', 'legendary', {"sphereDamage":0.12,"critChance":0.03}),
  meta('time_splitter', 'legendary', {"sphereDamage":0.12,"critChance":0.03}),
  meta('stasis_mandala', 'legendary', {"sphereDamage":0.12,"critChance":0.03}),
  meta('echo_archive', 'special', {"sphereDamage":0.12,"critChance":0.03}),
  meta('quantum_fold', 'special', {"sphereDamage":0.12,"critChance":0.03}),
  meta('zero_point_relay', 'special', {"sphereDamage":0.12,"critChance":0.03}),
  meta('void_star', 'special', {"critChance":0.03}),
  meta('axiom_core', 'legendary', {"sphereDamage":0.15,"cooldown":-0.08}),
  meta('infinite_loop', 'legendary', {"sphereDamage":0.15,"cooldown":-0.08}),
  meta('universal_fold', 'legendary', {"sphereDamage":0.15,"cooldown":-0.08})
);

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
  { id: 'fortress_network', requires: ['heavy_core', 'resonance_core'], name: { ru: 'Крепостная сеть', en: 'Fortress Network' }, desc: { ru: 'Соседние сферы получают +10% урона.', en: 'Nearby spheres gain +10% damage.' } },
  { id: 'echo_relay', requires: ['echo_conductor', 'network_relay'], name: { ru: 'Эхо-реле', en: 'Echo Relay' }, desc: { ru: 'Сеть из 2+ типов сфер получает ещё +8% урона.', en: 'A network with 2+ sphere types gains another +8% damage.' } },
  { id: 'glass_cannon', requires: ['overclock', 'void_contract'], name: { ru: 'Стеклянная пушка', en: 'Glass Cannon' }, desc: { ru: 'сферы получают +12% урона, но персонаж получает ещё +5% входящего урона.', en: 'spheres gain +12% damage, but the player takes 5% more damage.' } },
  { id: 'singularity', requires: ['lone_bastion', 'singularity_engine'], name: { ru: 'Сингулярность', en: 'Singularity' }, desc: { ru: 'При 1 типе сферы её урон увеличивается ещё на 20%.', en: 'With one sphere type, its damage is increased by another 20%.' } },
  { id: 'perfect_network', requires: ['fivefold_resonance', 'triangle_circuit'], name: { ru: 'Идеальная сеть', en: 'Perfect Network' }, desc: { ru: 'При 3+ типах сфер геометрические связи дают +10% урона.', en: 'With 3+ sphere types, geometric links grant +10% damage.' } },
  { id: 'unified_core', requires: ['unified_mind', 'zero_sphere'], name: { ru: 'Единое ядро', en: 'Unified Core' }, desc: { ru: 'Максимально прокачанная сфера усиливает остальные на 5% за уровень.', en: 'The strongest sphere boosts the others by 5% per level.' } },
  { id: 'geometry_loop', requires: ['triangle_engine', 'ring_engine'], name: { ru: 'Контур петли', en: 'Geometry Loop' }, desc: { ru: 'смена Geometry быстрее заряжает Resonance.', en: 'Geometry changes charge Resonance faster.' } },
  { id: 'lattice_memory', requires: ['lattice_engine', 'fractal_seed'], name: { ru: 'Память решётки', en: 'Lattice Memory' }, desc: { ru: 'Lattice и Fractal сохраняют часть силы после перестройки.', en: 'Lattice and Fractal retain part of their strength after rewiring.' } },
  { id: 'void_horizon', requires: ['void_ink', 'void_mark'], name: { ru: 'Горизонт пустоты', en: 'Void Horizon' }, desc: { ru: 'Void сильнее работает по элитным и ослабленным целям.', en: 'Void is stronger against elites and weakened targets.' } },
  { id: 'orbital_prism', requires: ['orbital_blade', 'prism_filter'], name: { ru: 'Орбитальная призма', en: 'Orbital Prism' }, desc: { ru: 'Orbital создаёт дополнительные отражённые лучи.', en: 'Orbital creates additional reflected beams.' } },
  { id: 'gravity_pulse', requires: ['gravity_hook', 'pulse_driver'], name: { ru: 'Гравитационный импульс', en: 'Gravity Pulse' }, desc: { ru: 'Pulse наносит больше урона собранным врагам.', en: 'Pulse deals more damage to grouped enemies.' } },
  { id: 'network_memory', requires: ['network_anchor', 'echo_weaver'], name: { ru: 'Память сети', en: 'Network Memory' }, desc: { ru: 'первая перестройка сети после Geometry Event сохраняет бонус.', en: 'The first network rebuild after a Geometry Event preserves its bonus.' } },
  { id: 'temporal_echo', requires: ['time_splitter', 'stasis_mandala'], name: { ru: 'Временное эхо', en: 'Temporal Echo' }, desc: { ru: 'после контроля времени следующий Resonance Event усиливается.', en: 'The next Resonance Event after time control is empowered.' } },
  { id: 'axiom_fold', requires: ['axiom_core', 'universal_fold'], name: { ru: 'Аксиоматический сгиб', en: 'Axiom Fold' }, desc: { ru: 'разные системы билда усиливают друг друга.', en: 'Different build systems reinforce one another.' } },
];

export function getActiveArtifactSynergies(s: { player: { artifacts: ArtifactId[] } }): ArtifactSynergy[] {
  const owned = new Set(s.player.artifacts);
  return ARTIFACT_SYNERGIES.filter((synergy) => synergy.requires.every((id) => owned.has(id)));
}


export function getArtifactSynergiesAfterPick(s: { player: { artifacts: ArtifactId[] } }, id: ArtifactId): ArtifactSynergy[] {
  const owned = new Set(s.player.artifacts);
  owned.add(id);
  return ARTIFACT_SYNERGIES.filter((synergy) => synergy.requires.every((requiredId) => owned.has(requiredId)));
}


export interface ArtifactSetDef {
  id: 'resonance_grid' | 'echo_architecture' | 'singularity_path' | 'geometry_craft' | 'void_horizon' | 'temporal_fold';
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  synergyIds: string[];
}

export interface ArtifactSetProgress {
  id: ArtifactSetDef['id'];
  name: ArtifactSetDef['name'];
  desc: ArtifactSetDef['desc'];
  activeSynergies: number;
  totalSynergies: number;
  complete: boolean;
}

// Phase 5.2 framework: existing pair synergies are grouped into three
// data-driven Sets. No additional completion gameplay is granted yet.
// Set UI and completion effects are separate roadmap tasks.
export const ARTIFACT_SETS: ArtifactSetDef[] = [
  {
    id: 'resonance_grid',
    name: { ru: 'Резонансная решётка', en: 'Resonance Grid' },
    desc: { ru: 'Сеть, построенная вокруг соседства сфер и геометрических связей.', en: 'A network built around sphere proximity and geometric links.' },
    synergyIds: ['fortress_network', 'perfect_network'],
  },
  {
    id: 'echo_architecture',
    name: { ru: 'Архитектура Эха', en: 'Echo Architecture' },
    desc: { ru: 'Развитие сети через реле и передачу силы от ведущей сферы.', en: 'Network growth through relay links and power transfer from the leading Sphere.' },
    synergyIds: ['echo_relay', 'unified_core'],
  },
  {
    id: 'singularity_path',
    name: { ru: 'Путь сингулярности', en: 'Singularity Path' },
    desc: { ru: 'Специализированный билд, усиливающий концентрацию силы и риск.', en: 'A specialised build that concentrates power and risk.' },
    synergyIds: ['glass_cannon', 'singularity'],
  },
  {
    id: 'geometry_craft',
    name: { ru: 'Мастерская геометрии', en: 'Geometry Craft' },
    desc: { ru: 'Сет вокруг Ring, Lattice и Fractal.', en: 'A set built around Ring, Lattice and Fractal.' },
    synergyIds: ['geometry_loop', 'lattice_memory', 'network_memory'],
  },
  {
    id: 'void_horizon',
    name: { ru: 'Горизонт пустоты', en: 'Void Horizon' },
    desc: { ru: 'Агрессивный билд добивания и контроля плотности.', en: 'An aggressive execution and density-control build.' },
    synergyIds: ['void_horizon', 'orbital_prism', 'gravity_pulse'],
  },
  {
    id: 'temporal_fold',
    name: { ru: 'Временной сгиб', en: 'Temporal Fold' },
    desc: { ru: 'Рискованный билд вокруг времени и универсальных синергий.', en: 'A risky build around time and universal synergies.' },
    synergyIds: ['temporal_echo', 'axiom_fold'],
  },
];

export function getArtifactSetProgress(
  s: { player: { artifacts: ArtifactId[] } },
): ArtifactSetProgress[] {
  const activeSynergyIds = new Set(getActiveArtifactSynergies(s).map((synergy) => synergy.id));
  return ARTIFACT_SETS.map((set) => {
    const activeSynergies = set.synergyIds.filter((id) => activeSynergyIds.has(id)).length;
    return {
      id: set.id,
      name: set.name,
      desc: set.desc,
      activeSynergies,
      totalSynergies: set.synergyIds.length,
      complete: activeSynergies === set.synergyIds.length,
    };
  });
}

export function getCompletedArtifactSets(
  s: { player: { artifacts: ArtifactId[] } },
): ArtifactSetProgress[] {
  return getArtifactSetProgress(s).filter((set) => set.complete);
}


export type ArtifactProtocolKind = 'triangle' | 'sphere' | 'event';

export interface ArtifactProtocol {
  id: string;
  kind: ArtifactProtocolKind;
  setId: ArtifactSetDef['id'];
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  requires: string[];
}

export const ARTIFACT_PROTOCOLS: ArtifactProtocol[] = [
  {
    id: 'triangle_resonance',
    kind: 'triangle',
    setId: 'resonance_grid',
    name: { ru: 'Треугольный резонанс', en: 'Triangle Resonance' },
    desc: { ru: 'Активируется при завершённой Резонансной решётке и рабочем треугольнике.', en: 'Activates with a completed Resonance Grid and an active triangle.' },
    requires: ['resonance_grid'],
  },
  {
    id: 'sphere_relay',
    kind: 'sphere',
    setId: 'echo_architecture',
    name: { ru: 'Релейный контур', en: 'Sphere Relay' },
    desc: { ru: 'Усиливает сеть при наличии двух и более типов сфер.', en: 'Strengthens the network when two or more Sphere types are present.' },
    requires: ['echo_architecture'],
  },
  {
    id: 'critical_echo',
    kind: 'event',
    setId: 'singularity_path',
    name: { ru: 'Критическое Эхо', en: 'Critical Echo' },
    desc: { ru: 'Даёт дополнительный импульс урона во время активной серии убийств.', en: 'Adds a damage pulse during an active kill streak.' },
    requires: ['singularity_path'],
  },
];

export interface ArtifactProtocolState extends ArtifactProtocol {
  discovered: boolean;
  active: boolean;
}

function protocolSetComplete(s: { player: { artifacts: ArtifactId[] } }, setId: ArtifactSetDef['id']): boolean {
  return getArtifactSetProgress(s).some((set) => set.id === setId && set.complete);
}

export function getArtifactProtocolStates(s: {
  player: { artifacts: ArtifactId[]; combo: number };
  spheres?: Array<{ type: string; alive?: boolean; pos: { x: number; y: number } }>;
}): ArtifactProtocolState[] {
  const sphereTypes = new Set((s.spheres || []).filter((x) => x.alive !== false).map((x) => x.type));
  const triangle = (s.spheres || []).filter((x) => x.alive !== false).length >= 3;
  return ARTIFACT_PROTOCOLS.map((protocol) => {
    const discovered = protocolSetComplete(s, protocol.setId);
    const active =
      protocol.id === 'triangle_resonance'
        ? discovered && triangle
        : protocol.id === 'sphere_relay'
          ? discovered && sphereTypes.size >= 2
          : discovered && s.player.combo >= 5;
    return { ...protocol, discovered, active };
  });
}

export type ArtifactSetBehavior = 'resonance_grid' | 'echo_architecture' | 'singularity_path';

export function hasCompletedArtifactSet(
  s: { player: { artifacts: ArtifactId[] } },
  setId: ArtifactSetDef['id'],
): boolean {
  return getCompletedArtifactSets(s).some((set) => set.id === setId);
}

export function getArtifactSetBehavior(s: { player: { artifacts: ArtifactId[] } }): {
  resonanceGrid: boolean;
  echoArchitecture: boolean;
  singularityPath: boolean;
} {
  return {
    resonanceGrid: hasCompletedArtifactSet(s, 'resonance_grid'),
    echoArchitecture: hasCompletedArtifactSet(s, 'echo_architecture'),
    singularityPath: hasCompletedArtifactSet(s, 'singularity_path'),
  };
}

export function getArtifactSetCompletionPulse(s: { player: { artifacts: ArtifactId[] } }): number {
  return Math.min(1, getCompletedArtifactSets(s).length / ARTIFACT_SETS.length);
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
export function getArtifactSphereDelayMultiplier(s: any): number { return Math.max(0.5, 1 + effectSum(s, 'sphereDelay')); }
export function getArtifactDamageTakenMultiplier(s: any): number {
  let multiplier = Math.max(0.45, 1 - effectSum(s, 'damageTakenReduction'));
  if (getActiveArtifactSynergies(s).some((x) => x.id === 'glass_cannon')) multiplier *= 1.05;
  return multiplier;
}
export function getArtifactCritChanceBonus(s: any): number { return effectSum(s, 'critChance'); }
export function getArtifactDodgeChanceBonus(s: any): number { return effectSum(s, 'dodgeChance'); }
export function getArtifactVampireBonus(s: any): number { return effectSum(s, 'vampire'); }
export function getArtifactReflectChance(s: any): number { return hasArtifact(s, 'mirror') ? 0.20 : 0; }

function uniquesphereCount(s: any): number {
  return new Set((s.spheres || []).filter((sphere: any) => sphere.alive !== false).map((sphere: any) => sphere.type)).size;
}

function hasNearbysphere(s: any, sphere: any, maxDistance: number): boolean {
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

export function getSphereArtifactModifiers(s: any, type: string, sphere?: any): { damage: number; delay: number; radius: number } {
  // Global sphere stats are applied once in engine.ts. This helper contains
  // only type-specific artifact modifiers and interaction/synergy effects.
  let damage = 1;
  const delay = 1;
  let radius = 1;
  const effects: keyof ArtifactEffects = `${type}Damage` as keyof ArtifactEffects;
  const radiusEffect: keyof ArtifactEffects = `${type}Radius` as keyof ArtifactEffects;
  damage *= 1 + effectSum(s, effects);
  radius *= 1 + effectSum(s, radiusEffect);

  const unique = uniquesphereCount(s);
  const synergies = getActiveArtifactSynergies(s);
  if (synergies.some((x) => x.id === 'fortress_network') && sphere && hasNearbysphere(s, sphere, 190)) damage *= 1.10;
  if (synergies.some((x) => x.id === 'echo_relay') && unique >= 2) damage *= 1.08;
  if (synergies.some((x) => x.id === 'glass_cannon')) damage *= 1.12;
  if (synergies.some((x) => x.id === 'singularity') && unique <= 1) damage *= 1.20;
  if (synergies.some((x) => x.id === 'perfect_network') && unique >= 3 && sphere && formsTriangle(s, sphere)) damage *= 1.10;
  if (hasArtifact(s, 'lone_bastion') && unique <= 1) damage *= 1.30;
  if (hasArtifact(s, 'fivefold_resonance') && unique > 1) damage *= 1 + Math.min(5, unique) * 0.04;
  if (hasArtifact(s, 'network_relay') && unique >= 2) damage *= 1.05;
  if (hasArtifact(s, 'relay_matrix') && Object.values(s.player.sphereProgression || {}).some((level: any) => level >= 7)) damage *= 1.10;
  if (hasArtifact(s, 'singularity_engine') && unique <= 2) damage *= 1.18;
  if (hasArtifact(s, 'mirror_network') && unique >= 2) damage *= 1.12;


  if (synergies.some((x) => x.id === 'unified_core') && sphere) {
    const levels = Object.values(s.player.sphereProgression || {}) as number[];
    const strongest = Math.max(0, ...levels);
    const sphereLevel = Number(s.player.sphereProgression?.[sphere.type] || 0);
    if (sphereLevel < strongest) damage *= 1 + strongest * 0.05;
  }
  return { damage, delay, radius };
}

export function getSphereArtifactDamageMultiplier(s: any, sphere: any): number {
  let multiplier = 1;
  if (hasArtifact(s, 'resonance_core') && hasNearbysphere(s, sphere, 190)) multiplier *= 1.12;
  if (hasArtifact(s, 'triangle_circuit') && formsTriangle(s, sphere)) multiplier *= 1.15;
  if (hasArtifact(s, 'unified_mind')) {
    const levels = Object.values(s.player.sphereProgression || {}) as number[];
    const strongest = Math.max(0, ...levels);
    multiplier *= 1 + strongest * 0.03;
  }
  if (hasArtifact(s, 'zero_sphere')) multiplier *= 1.20;

  const synergies = getActiveArtifactSynergies(s);
  if (synergies.some((x) => x.id === 'void_horizon') && sphere?.type === 'void') {
    if (sphere && (s.enemies || []).some((enemy: any) => enemy.isElite && enemy.hp > 0)) multiplier *= 1.15;
  }
  if (synergies.some((x) => x.id === 'orbital_prism') && sphere?.type === 'orbital') multiplier *= 1.15;
  if (synergies.some((x) => x.id === 'gravity_pulse') && sphere?.type === 'pulse') {
    const grouped = (s.enemies || []).filter((enemy: any) => enemy.hp > 0 && (s.spheres || []).some((other: any) => other.alive !== false && Math.hypot(enemy.pos.x - other.pos.x, enemy.pos.y - other.pos.y) < 140));
    if (grouped.length >= 3) multiplier *= 1.15;
  }
  return multiplier;
}

const rarityWeight = (rarity: ArtifactRarity): number => ({ common: 62, rare: 24, epic: 9, special: 4, legendary: 1 }[rarity]);

function randomWeighted<T>(items: T[], weight: (item: T) => number, random: () => number = Math.random): T {
  const total = items.reduce((sum, item) => sum + Math.max(0, weight(item)), 0);
  if (total <= 0) return items[Math.floor(random() * items.length)];
  let roll = random() * total;
  for (const item of items) {
    roll -= Math.max(0, weight(item));
    if (roll <= 0) return item;
  }
  return items[items.length - 1];
}

export function pickArtifactChoices(
  s: { player: { artifacts: ArtifactId[] } },
  count = 3,
  includeLegendary = false,
  random: () => number = Math.random,
): ArtifactId[] {
  const owned = new Set(s.player.artifacts);
  const available = ARTIFACT_METADATA.filter((item) => !owned.has(item.id) && (includeLegendary || item.rarity !== 'legendary'));
  const result: ArtifactId[] = [];
  const pool = [...available];
  while (result.length < count && pool.length > 0) {
    const chosen = randomWeighted(pool, (item) => rarityWeight(item.rarity), random);
    result.push(chosen.id);
    pool.splice(pool.indexOf(chosen), 1);
  }
  return result;
}

export function pickStellaArtifactChoices(
  s: { player: { artifacts: ArtifactId[] } },
  count = 3,
  random: () => number = Math.random,
): ArtifactId[] {
  const owned = new Set(s.player.artifacts);
  const pool = ARTIFACT_METADATA.filter((item) => item.rarity === 'legendary' && !owned.has(item.id));
  const result: ArtifactId[] = [];
  while (result.length < count && pool.length > 0) {
    const chosen = randomWeighted(pool, () => 1, random);
    result.push(chosen.id);
    pool.splice(pool.indexOf(chosen), 1);
  }
  return result;
}

export function pickStellaArtifactChoice(
  s: { player: { artifacts: ArtifactId[] } },
  random: () => number = Math.random,
): ArtifactId | null {
  return pickStellaArtifactChoices(s, 1, random)[0] || null;
}
