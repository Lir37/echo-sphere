import {
  getCharacterId,
  getCharacterMoveSpeedMultiplier,
} from './characterRuntime';
import {
  getArtifactMoveSpeedMultiplier,
  getArtifactXpMultiplier,
  getArtifactSetProgress,
  getArtifactProtocolStates,
} from './artifactSystem';
import { ABILITIES, SPHERE_TYPES, type AbilityType, type SphereType } from './gameData';
import { analyzeSphereNetwork } from './network';
import { getActiveSphereAbilitySynergies, sphereLevel } from './sphereProgression';
import { BASE_PLAYER_SPEED } from './engineState';
import type { GameState } from './engineTypes';

export function getMoveSpeed(s: GameState): number {
  let sp = BASE_PLAYER_SPEED;
  const lvl = s.player.abilities.movespeed || 0;
  sp *= 1 + lvl * 0.1;
  sp *= 1 + (s.shopUpgrades.speed || 0) * 0.05;
  sp *= getArtifactMoveSpeedMultiplier(s);
  if (s.player.swiftBootsTimer > 0) sp *= 1.1;
  if (s.player.mutationStage >= 3) sp *= 1.2;
  sp *= getCharacterMoveSpeedMultiplier(s);
  if (s.player.hunterTrophyTimer > 0 && getCharacterId(s) === 'hunter') sp *= 1.1;
  if (getCharacterId(s) === 'berserker' && s.player.characterMasteryLevel >= 5 && s.player.buffTimer > 0) sp *= 1.05;
  if (s.player.abilities.momentum && s.player.dashTimer > 0) sp *= 1 + (s.player.abilities.momentum * 0.05);
  return sp;
}

export function getXpMult(s: GameState): number {
  let m = 1;
  m *= 1 + (s.shopUpgrades.xp || 0) * 0.05;
  m *= getArtifactXpMultiplier(s);
  return m;
}

export function getMagnetRadius(s: GameState): number {
  let r = 60;
  const lvl = s.player.abilities.magnet || 0;
  r *= 1 + lvl * 0.2;
  if (s.player.sphereMods.magnetic > 0) r *= 1 + 0.15 * s.player.sphereMods.magnetic;
  return r;
}


export interface BuildDiagnosticRow {
  id: 'offense' | 'control' | 'survival' | 'network' | 'resonance' | 'synergy';
  ru: string[];
  en: string[];
}

const diagnosticSphereNames = (s: GameState, types: readonly SphereType[]): { ru: string[]; en: string[] } => {
  const picked = types.filter((type) => sphereLevel(s, type) > 0);
  return {
    ru: picked.map((type) => `${SPHERE_TYPES[type].name.ru} VII`.replace(' VII', ` ${sphereLevel(s, type)}`)),
    en: picked.map((type) => `${SPHERE_TYPES[type].name.en} ${sphereLevel(s, type)}`),
  };
};

const hasAbility = (s: GameState, id: AbilityType): boolean => (s.player.abilities[id] || 0) > 0;

export function getBuildDiagnostics(s: GameState): BuildDiagnosticRow[] {
  const network = analyzeSphereNetwork(s.spheres);
  const activeSets = getArtifactSetProgress(s).filter((set) => set.complete);
  const activeProtocols = getArtifactProtocolStates(s).filter((protocol) => protocol.active);
  const charSynergies = getActiveSphereAbilitySynergies(s);

  const offense = diagnosticSphereNames(s, ['standard', 'sniper', 'shotgun', 'chain', 'pulse', 'void']);
  const control = diagnosticSphereNames(s, ['aura', 'gravity', 'prism']);
  const survivalSignalsRu: string[] = [];
  const survivalSignalsEn: string[] = [];
  if (hasAbility(s, 'vitality')) { survivalSignalsRu.push('Живучесть'); survivalSignalsEn.push('Vitality'); }
  if (hasAbility(s, 'shield')) { survivalSignalsRu.push('Барьер'); survivalSignalsEn.push('Barrier'); }
  if (hasAbility(s, 'dodge')) { survivalSignalsRu.push('Уклонение'); survivalSignalsEn.push('Dodge'); }
  if (hasAbility(s, 'vampire')) { survivalSignalsRu.push('Вампиризм'); survivalSignalsEn.push('Vampirism'); }
  if ((s.player.abilities.movespeed || 0) > 0) { survivalSignalsRu.push('Мобильность'); survivalSignalsEn.push('Mobility'); }
  const resonanceGeometry = String((s.player as any).resonanceGeometryKey || 'none');

  return [
    {
      id: 'offense',
      ru: [...offense.ru, ...((s.player.abilities.damage || 0) > 0 ? [`${ABILITIES.damage.name.ru} ${s.player.abilities.damage}`] : [])],
      en: [...offense.en, ...((s.player.abilities.damage || 0) > 0 ? [`${ABILITIES.damage.name.en} ${s.player.abilities.damage}`] : [])],
    },
    {
      id: 'control',
      ru: [...control.ru, ...((s.player.sphereMods.freeze || 0) > 0 ? ['Freeze'] : []), ...((s.player.sphereMods.gravitic || 0) > 0 ? ['Gravity'] : [])],
      en: [...control.en, ...((s.player.sphereMods.freeze || 0) > 0 ? ['Freeze'] : []), ...((s.player.sphereMods.gravitic || 0) > 0 ? ['Gravity'] : [])],
    },
    { id: 'survival', ru: survivalSignalsRu, en: survivalSignalsEn },
    {
      id: 'network',
      ru: [`Типов сфер: ${new Set(s.spheres.filter((x) => x.alive).map((x) => x.type)).size}`, `Связей: ${network.links.length}`, `Геометрия: ${resonanceGeometry}`],
      en: [`Sphere types: ${new Set(s.spheres.filter((x) => x.alive).map((x) => x.type)).size}`, `Links: ${network.links.length}`, `Geometry: ${resonanceGeometry}`],
    },
    {
      id: 'resonance',
      ru: [`Резонанс: ${Math.floor(s.player.resonanceCharge)}/100`, `Геометрия: ${resonanceGeometry}`],
      en: [`Resonance: ${Math.floor(s.player.resonanceCharge)}/100`, `Geometry: ${resonanceGeometry}`],
    },
    {
      id: 'synergy',
      ru: [
        ...activeSets.map((set) => set.name.ru),
        ...activeProtocols.map((protocol) => protocol.name.ru),
        ...(charSynergies.length ? [`Активных синергий сфер: ${charSynergies.length}`] : []),
      ],
      en: [
        ...activeSets.map((set) => set.name.en),
        ...activeProtocols.map((protocol) => protocol.name.en),
        ...(charSynergies.length ? [`Active Sphere synergies: ${charSynergies.length}`] : []),
      ],
    },
  ];
}
