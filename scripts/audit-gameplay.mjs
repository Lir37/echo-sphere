import fs from 'node:fs/promises';

const root = new URL('../', import.meta.url);
const read = async (file) => fs.readFile(new URL(file, root), 'utf8');

function section(source, startMarker, endMarker) {
  const start = source.indexOf(startMarker);
  const end = source.indexOf(endMarker, start + startMarker.length);
  if (start < 0 || end < 0) throw new Error(`Section not found: ${startMarker}`);
  return source.slice(start + startMarker.length, end);
}

function numberedNames(source) {
  return [...source.matchAll(/^\d{2}\s+([A-Z][A-Z ]+)$/gm)].map((m) => m[1].trim());
}

function quotedUnion(source, typeName) {
  const re = new RegExp(`export type ${typeName} = ([^;]+);`);
  const match = source.match(re);
  if (!match) throw new Error(`Union not found: ${typeName}`);
  return [...match[1].matchAll(/'([^']+)'/g)].map((m) => m[1]);
}

function artifactIds(source) {
  const start = source.indexOf('export const ARTIFACTS: ArtifactDef[] = [');
  const end = source.indexOf('export const ARTIFACT_MAP', start);
  return [...source.slice(start, end).matchAll(/id:\s*'([^']+)'/g)].map((m) => m[1]);
}

function abilityIds(source) {
  const start = source.indexOf('export const ABILITIES');
  const end = source.indexOf('export const ARTIFACTS', start);
  return [...source.slice(start, end).matchAll(/id:\s*'([^']+)'/g)].map((m) => m[1]);
}

function runeIds(source) {
  const start = source.indexOf('export const RUNE_DEFS');
  return [...source.slice(start).matchAll(/^\s{2}([a-z][a-z0-9_]*)\s*:/gm)].map((m) => m[1]);
}

function modifiers(source) {
  const start = source.indexOf('export interface SphereMods');
  const end = source.indexOf('export interface SphereUpgradeChoice', start);
  return [...source.slice(start, end).matchAll(/^\s{2}([a-z][a-z0-9_]*)\s*:/gm)].map((m) => m[1]);
}

function networkFormations(source) {
  const match = source.match(/export type NetworkFormation = ([^;]+);/);
  if (!match) throw new Error('NetworkFormation union not found');
  return [...match[1].matchAll(/'([^']+)'/g)].map((m) => m[1]).filter((x) => x !== 'none');
}

function compare(finalItems, currentItems) {
  const current = new Set(currentItems);
  return {
    finalCount: finalItems.length,
    currentCount: currentItems.length,
    implemented: finalItems.filter((id) => current.has(id)),
    missing: finalItems.filter((id) => !current.has(id)),
    coverage: finalItems.length ? Number((finalItems.filter((id) => current.has(id)).length / finalItems.length).toFixed(3)) : 1,
  };
}

const [blueprint, gameData, engine, progression, network, gap] = await Promise.all([
  read('GPT/ECHO_SPHERE_MASTER_FINAL_GAMEPLAY_BLUEPRINT_v1.1.txt'),
  read('src/gameData.ts'),
  read('src/engine.ts'),
  read('src/sphereProgression.ts'),
  read('src/network.ts'),
  read('GPT/ECHO_SPHERE_DEVELOPMENT_STATE_AND_GAP_REPORT_v1.0.txt'),
]);

const sphereFinal = numberedNames(
  section(blueprint, 'FINAL CORE SPHERE ROSTER:', 'Sphere architecture is data-driven.'),
);
const activeSection = section(
  blueprint,
  'FINAL ACTIVE ABILITIES:',
  'FINAL PASSIVE ABILITIES:',
);
const passiveSection = section(
  blueprint,
  'FINAL PASSIVE ABILITIES:',
  'ACTIVE-SLOT RULE:',
);
const abilityFinal = [...numberedNames(activeSection), ...numberedNames(passiveSection)];

const modifierFinal = numberedNames(
  section(blueprint, 'FINAL MODIFIER ROSTER:', 'Design rule:'),
);
const currentSphere = quotedUnion(gameData, 'SphereType');
const currentAbility = abilityIds(gameData);
const currentModifier = modifiers(engine);

const abilityNameById = {
  blast: 'ECHO PULSE',
  shield: 'SPHERE BARRIER',
  teleport: 'ECHO JUMP',
  firetrail: 'OVERHEAT',
  minion: 'ECHO DRONE',
  lightning: 'CHAIN LIGHTNING',
  timestop: 'ECHO FREEZE',
  darkritual: 'OVERLOAD',
  radius: 'SPHERE RADIUS',
  damage: 'SPHERE DAMAGE',
  attackspeed: 'ATTACK SPEED',
  maxspheres: 'MAX SPHERES',
  movespeed: 'MOVE SPEED',
  slow: 'ENEMY SLOW',
  vitality: 'VITALITY',
  vampire: 'VAMPIRISM',
  dodge: 'DODGE',
  crit: 'CRITICAL',
  magnet: 'XP MAGNET',
  sphereboost: 'SPHERE BOOST',
};
const currentArtifacts = artifactIds(gameData);
const currentRunes = runeIds(await read('src/runes.ts'));
const currentGeometry = networkFormations(network);

const summary = {
  spheres: compare(sphereFinal, currentSphere.map((x) => x.toUpperCase())),
  abilities: compare(abilityFinal, currentAbility.map((x) => abilityNameById[x] || x.toUpperCase().replace(/_/g, ' '))),
  modifiers: compare(modifierFinal, currentModifier.map((x) => x.toUpperCase())),
  geometry: {
    finalCount: 7,
    currentCount: currentGeometry.length,
    current: currentGeometry,
    missingFinal: ['line','triangle','cluster','square','ring','lattice','fractal']
      .filter((id) => !new Set(currentGeometry).has(id)),
  },
  artifacts: {
    currentCount: currentArtifacts.length,
    gapReportCount: Number(gap.match(/Current Artifact definition count:\n(\d+)/)?.[1] || 0),
    target: gap.match(/Final target:\n90-120/) ? '90-120' : 'unknown',
  },
  runes: {
    currentCount: currentRunes.length,
    ids: currentRunes,
  },
};

if (process.argv.includes('--json')) {
  console.log(JSON.stringify(summary, null, 2));
} else {
  console.log('ECHO SPHERE gameplay coverage audit');
  console.log('==================================');
  for (const [name, value] of Object.entries(summary)) {
    if (name === 'geometry') {
      console.log(`Geometry: ${value.currentCount}/${value.finalCount} implemented -> ${value.current.join(', ')}`);
      continue;
    }
    if (name === 'artifacts') {
      console.log(`Artifacts: ${value.currentCount} current, Gap Report says ${value.gapReportCount}, final target ${value.target}`);
      continue;
    }
    if (name === 'runes') {
      console.log(`Runes: ${value.currentCount} current -> ${value.ids.join(', ')}`);
      continue;
    }
    console.log(`${name[0].toUpperCase() + name.slice(1)}: ${value.currentCount}/${value.finalCount} covered (${Math.round(value.coverage * 100)}%)`);
    if (value.missing.length) console.log(`  Missing from final vocabulary: ${value.missing.join(', ')}`);
  }
}
