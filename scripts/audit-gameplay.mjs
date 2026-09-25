import fs from 'node:fs/promises';

const root = new URL('../', import.meta.url);
const read = async (file) => fs.readFile(new URL(file, root), 'utf8');

function quotedUnion(source, typeName) {
  const re = new RegExp(`export type ${typeName} = ([\\s\\S]*?);`);
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
  const match = source.match(/export type AbilityType = ([^;]+);/);
  if (!match) throw new Error('AbilityType union not found');
  return [...match[1].matchAll(/'([^']+)'/g)].map((m) => m[1]);
}

function runeIds(source) {
  const start = source.indexOf('export const RUNE_DEFS');
  return [...source.slice(start).matchAll(/^\s{2}([a-z][a-z0-9_]*)\s*:/gm)].map((m) => m[1]);
}

function modifierIds(source) {
  const start = source.indexOf('export interface SphereMods');
  const end = source.indexOf('export interface SphereUpgradeChoice', start);
  return [...source.slice(start, end).matchAll(/^\s{2}([a-z][a-z0-9_]*)\s*:/gm)].map((m) => m[1]);
}

function compare(finalItems, currentItems) {
  const current = new Set(currentItems);
  const implemented = finalItems.filter((id) => current.has(id));
  return {
    finalCount: finalItems.length,
    currentCount: currentItems.length,
    implemented,
    missing: finalItems.filter((id) => !current.has(id)),
    coverage: finalItems.length ? Number((implemented.length / finalItems.length).toFixed(3)) : 1,
  };
}

function integration(itemIds, source) {
  const integrated = itemIds.filter((id) => source.includes(id));
  return {
    count: itemIds.length,
    integrated,
    missing: itemIds.filter((id) => !source.includes(id)),
    coverage: itemIds.length ? Number((integrated.length / itemIds.length).toFixed(3)) : 1,
  };
}

const FINAL_ACTIVE_ABILITIES = [
  ['ECHO PULSE', 'blast'],
  ['SPHERE BARRIER', 'shield'],
  ['ECHO JUMP', 'teleport'],
  ['OVERHEAT', 'firetrail'],
  ['ECHO DRONE', 'minion'],
  ['CHAIN LIGHTNING', 'lightning'],
  ['ECHO FREEZE', 'timestop'],
  ['OVERLOAD', 'darkritual'],
  ['NOVA', null],
  ['GRAVITY WELL', null],
  ['RESONANCE PULSE', null],
  ['NODE SWAP', null],
  ['PHASE SHELL', null],
  ['VOID LANCE', null],
  ['TIME FRACTURE', null],
];

const FINAL_PASSIVE_ABILITIES = [
  ['SPHERE RADIUS', 'radius'],
  ['SPHERE DAMAGE', 'damage'],
  ['ATTACK SPEED', 'attackspeed'],
  ['MAX SPHERES', 'maxspheres'],
  ['MOVE SPEED', 'movespeed'],
  ['ENEMY SLOW', 'slow'],
  ['VITALITY', 'vitality'],
  ['VAMPIRISM', 'vampire'],
  ['DODGE', 'dodge'],
  ['CRITICAL', 'crit'],
  ['XP MAGNET', 'magnet'],
  ['SPHERE BOOST', 'sphereboost'],
  ['NETWORK CAPACITY', null],
  ['LINK STABILITY', null],
  ['RESONANCE CHARGE', null],
];

const FINAL_SPHERES = ['STANDARD','SNIPER','CHAIN','SHOTGUN','AURA','ORBITAL','PRISM','GRAVITY','PULSE','VOID'];
const FINAL_MODIFIERS = ['MULTISHOT','PIERCE','RICOCHET','FIRE','FREEZE','POISON','BREACH','OVERLOAD','SPLIT','SHATTER','EXECUTE','MARK','ECHO','ANCHOR','PHASE','STATIC','RESONANT','MAGNETIC','VAMPIRIC','CORRUPT','DRAIN','AFTERIMAGE','IMPACT','GRAVITIC'];
const FINAL_RUNES = ['overdrive','phase','harvest','purge','resonance','fortify','hunt','echo','gravity','runic_cell'];

const [blueprint, gameData, engine, network, progression, runes, artifacts, mobileControls, gap] = await Promise.all([
  read('GPT/ECHO_SPHERE_MASTER_FINAL_GAMEPLAY_BLUEPRINT_v1.2.txt'),
  read('src/gameData.ts'),
  read('src/engine.ts'),
  read('src/network.ts'),
  read('src/sphereProgression.ts'),
  read('src/runes.ts'),
  read('src/artifactSystem.ts'),
  read('src/MobileControls.tsx'),
  read('GPT/ECHO_SPHERE_DEVELOPMENT_STATE_AND_GAP_REPORT_v1.0.txt'),
]);

const currentSpheres = quotedUnion(gameData, 'SphereType');
const currentAbilities = abilityIds(gameData);
const currentModifiers = modifierIds(engine);
const currentArtifacts = artifactIds(gameData);
const currentRunes = runeIds(runes);
const currentGeometry = quotedUnion(network, 'NetworkFormation').filter((id) => id !== 'none');
const abilityEvolutionIds = [...new Set([...progression.matchAll(/ae\('([^']+)'/g)].map((m) => m[1]))];
const sphereBranchIds = [...new Set([...progression.matchAll(/br\('([^']+)'/g)].map((m) => m[1]))];

const finalAbilityReport = (items) => items.map(([name, id]) => ({
  name,
  blueprintPresent: blueprint.includes(name),
  codeId: id,
  status: id && currentAbilities.includes(id) ? 'CODE+INTEGRATED' : 'MISSING',
  engineReferenced: Boolean(id && engine.includes(id)),
  progressionPresent: Boolean(id && progression.includes("ability:'" + id + "'")),
}));

const audit = {
  sourceOfTruth: {
    blueprint: 'GPT/ECHO_SPHERE_MASTER_FINAL_GAMEPLAY_BLUEPRINT_v1.2.txt',
    gapReport: 'GPT/ECHO_SPHERE_DEVELOPMENT_STATE_AND_GAP_REPORT_v1.0.txt',
    branch: 'new-desing',
  },
  spheres: {
    finalCount: FINAL_SPHERES.length,
    current: currentSpheres,
    implemented: FINAL_SPHERES.filter((name) => currentSpheres.includes(name.toLowerCase())),
    missing: FINAL_SPHERES.filter((name) => !currentSpheres.includes(name.toLowerCase())),
    levelUpSourceUsesVerticalSliceTypes: engine.includes('VERTICAL_SLICE_SPHERE_TYPES.filter'),
    mobileSelectionUsesVerticalSliceTypes: mobileControls.includes('VERTICAL_SLICE_SPHERE_TYPES'),
  },
  abilities: {
    finalActive: finalAbilityReport(FINAL_ACTIVE_ABILITIES),
    finalPassive: finalAbilityReport(FINAL_PASSIVE_ABILITIES),
    evolutionIds: integration(abilityEvolutionIds, engine),
    sphereBranchIds: integration(sphereBranchIds, engine),
  },
  modifiers: compare(FINAL_MODIFIERS, currentModifiers),
  geometry: {
    finalCount: 7,
    current: currentGeometry,
    implemented: ['line','triangle','cluster','square','ring','lattice','fractal'].filter((id) => currentGeometry.includes(id)),
    missing: ['line','triangle','cluster','square','ring','lattice','fractal'].filter((id) => !currentGeometry.includes(id)),
  },
  runes: compare(FINAL_RUNES, currentRunes),
  artifacts: {
    currentCount: currentArtifacts.length,
    finalTargetMin: 90,
    finalTargetMax: 120,
    gapReportMentionsTarget: /90-120/.test(gap),
  },
  semanticReviewFlags: [
    {
      id: 'shield_echo_guard_description_alignment',
      severity: 'INFO',
      status: engine.includes("branch === 'shield_echo_guard'"),
      note: 'Shield evolution descriptions now match the runtime: nearby Spheres contribute to the player network shield rather than storing per-Sphere shield state.',
    },
    {
      id: 'minion_network_is_runtime_nodes',
      severity: 'INFO',
      status: engine.includes('buildRuntimeNetworkNodes(s.spheres, s.minions'),
      note: 'Current implementation does add Echo Drones to the runtime Network graph, so the earlier criticism that Drones were never true nodes is stale.',
    },
    {
      id: 'visual_randomness_not_seeded',
      severity: 'LOW',
      status: !engine.includes('Math.random()'),
      note: 'Engine gameplay RNG is seeded; renderer-only lightning jitter may remain intentionally non-deterministic.',
    },
  ],
  tooling: {
    auditScriptVersion: 'coverage-v2',
    strictMode: process.argv.includes('--strict'),
  },
};

if (process.argv.includes('--json')) {
  console.log(JSON.stringify(audit, null, 2));
} else {
  console.log('ECHO SPHERE gameplay coverage audit v2');
  console.log('=====================================');
  console.log(`Spheres: ${audit.spheres.implemented.length}/${audit.spheres.finalCount} implemented; missing: ${audit.spheres.missing.join(', ') || 'none'}`);
  const allAbilities = [...audit.abilities.finalActive, ...audit.abilities.finalPassive];
  const abilityCode = allAbilities.filter((x) => x.status !== 'MISSING').length;
  const abilityIntegrated = allAbilities.filter((x) => x.status === 'CODE+INTEGRATED' && x.engineReferenced).length;
  console.log(`Abilities: ${abilityCode}/30 vocabulary mapped to code; ${abilityIntegrated}/30 engine-referenced; missing: ${allAbilities.filter((x) => x.status === 'MISSING').map((x) => x.name).join(', ') || 'none'}`);
  console.log(`Ability evolution references: ${audit.abilities.evolutionIds.integrated.length}/${audit.abilities.evolutionIds.count}`);
  console.log(`Sphere branch references: ${audit.abilities.sphereBranchIds.integrated.length}/${audit.abilities.sphereBranchIds.count}`);
  console.log(`Modifiers: ${audit.modifiers.implemented.length}/${audit.modifiers.finalCount}; missing: ${audit.modifiers.missing.join(', ') || 'none'}`);
  console.log(`Geometry: ${audit.geometry.current.length}/7; missing: ${audit.geometry.missing.join(', ') || 'none'}`);
  console.log(`Runes: ${audit.runes.implemented.length}/${audit.runes.finalCount}; missing: ${audit.runes.missing.join(', ') || 'none'}`);
  console.log(`Artifacts: ${audit.artifacts.currentCount} current; final target ${audit.artifacts.finalTargetMin}-${audit.artifacts.finalTargetMax}`);
  console.log('Semantic review flags:');
  for (const flag of audit.semanticReviewFlags.filter((item) => item.severity !== 'INFO')) {
    console.log(`  [${flag.severity}] ${flag.id}: ${flag.note}`);
  }
}

if (process.argv.includes('--strict')) {
  const allAbilities = [...audit.abilities.finalActive, ...audit.abilities.finalPassive];
  const problems = [
    ...audit.spheres.missing,
    ...allAbilities.filter((x) => x.status === 'MISSING').map((x) => x.name),
    ...audit.modifiers.missing,
    ...audit.geometry.missing,
    ...audit.runes.missing,
    ...audit.abilities.evolutionIds.missing,
    ...audit.abilities.sphereBranchIds.missing,
  ];
  if (problems.length) process.exitCode = 1;
}
