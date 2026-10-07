import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import { addResonanceCharge } from '../src/resonance.ts';
import { createRegionState, getRegionPhase, REGION_POCKETS, REGION_CHALLENGES } from '../src/region.ts';

const mobileControlsSource = await fs.readFile(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

test('expanded Sphere roster is the gameplay data source of truth', async () => {
  const gameDataSource = await fs.readFile(new URL('../src/gameData.ts', import.meta.url), 'utf8');
  const expected = ['standard', 'sniper', 'shotgun', 'chain', 'aura', 'orbital', 'prism', 'gravity', 'pulse', 'void'];
  for (const type of expected) {
    assert.match(gameDataSource, new RegExp(`['"]${type}['"]`));
  }
  assert.match(gameDataSource, /export const SPHERE_TYPES/);
});

test('mobile sphere selector uses the complete Sphere roster', () => {
  assert.match(
    mobileControlsSource,
    /Object\.keys\(SPHERE_TYPES\) as SphereType\[\]/,
  );
});

test('global Resonance uses a player-level resource separate from local formation cadence', () => {
  const player = { resonanceCharge: 0 };
  const sphere = { formationHitCount: 0 };

  addResonanceCharge(player, 5);
  sphere.formationHitCount += 1;

  assert.equal(player.resonanceCharge, 5);
  assert.equal(sphere.formationHitCount, 1);
});

test('Build Diagnostics is descriptive and does not expose a ranking score', async () => {
  const statsSource = await fs.readFile(new URL('../src/engineStats.ts', import.meta.url), 'utf8');
  const appSource = await fs.readFile(new URL('../src/App.tsx', import.meta.url), 'utf8');
  assert.match(statsSource, /export function getBuildDiagnostics\(s: GameState\): BuildDiagnosticRow\[\]/);
  assert.match(statsSource, /id: 'offense'/);
  assert.match(statsSource, /id: 'control'/);
  assert.match(statsSource, /id: 'survival'/);
  assert.match(statsSource, /id: 'network'/);
  assert.match(statsSource, /id: 'resonance'/);
  assert.match(statsSource, /id: 'synergy'/);
  assert.doesNotMatch(statsSource, /score|rank|rating/i);
  assert.match(appSource, /getBuildDiagnostics\(st\)/);
});


test('Character Mastery exposes the ten-level progression defined by the Blueprint', async () => {
  const charactersSource = await fs.readFile(new URL('../src/characters.ts', import.meta.url), 'utf8');
  const persistenceSource = await fs.readFile(new URL('../src/persistence.ts', import.meta.url), 'utf8');
  const validationSource = await fs.readFile(new URL('../src/characterDataValidation.ts', import.meta.url), 'utf8');
  assert.match(charactersSource, /level: 1 \| 2 \| 3 \| 4 \| 5 \| 6 \| 7 \| 8 \| 9 \| 10/);
  assert.match(persistenceSource, /CHARACTER_MASTERY_THRESHOLDS = \[0, 250, 750, 1500, 2500, 4000, 6000, 8500, 11500, 15000\]/);
  assert.match(persistenceSource, /return level >= 10 \? null : CHARACTER_MASTERY_THRESHOLDS\[level\]/);
  assert.match(validationSource, /expected exactly 10 mastery levels/);
});


test('Elite roster contains ten distinct authored variants beyond the base Elite shell', async () => {
  const source = await fs.readFile(new URL('../src/engineEnemies.ts', import.meta.url), 'utf8');
  assert.match(source, /export const ELITE_VARIANTS/);
  const expected = ['linkbreaker', 'resonance_leech', 'phantom_hunter', 'geometry_shifter', 'splitter_prime', 'mirror_warden', 'stasis_warden', 'nullifier', 'pyroclast', 'scavenger_prime'];
  for (const variant of expected) assert.match(source, new RegExp(`['"]${variant}['"]`));
});


test('Boss roster contains the eight Blueprint families and preserves the four legacy families', async () => {
  const source = await fs.readFile(new URL('../src/gameData.ts', import.meta.url), 'utf8');
  for (const id of ['shooter', 'charger', 'summoner', 'aura', 'conductor', 'architect', 'null', 'stella_warden']) {
    assert.match(source, new RegExp(`['"]${id}['"]`));
  }
});


test('Enemy roster contains fifteen authored roles', async () => {
  const source = await fs.readFile(new URL('../src/engineTypes.ts', import.meta.url), 'utf8');
  const roles = ['grunt', 'swarmer', 'charger', 'tank_guard', 'ranged', 'splitter', 'healer', 'bomber', 'leech', 'sniper', 'disruptor', 'anchor', 'phase', 'scavenger', 'corruptor'];
  for (const role of roles) assert.match(source, new RegExp(`['"]${role}['"]`));
});


test('Region runtime contract exposes 5 pockets, 7 POIs, 3 contracts and 30-minute clear schedule',()=>{assert.equal(REGION_POCKETS.length,5);assert.equal(REGION_CHALLENGES.length,3);assert.equal(getRegionPhase(1800).id,5);const r=createRegionState('stabilization','fractured_network');assert.equal(r.id,'resonance_basin');});
test('Region player-facing contract is represented by the interactive star map and POI runtime',async()=>{const app=await fs.readFile(new URL('../src/EchoMapScreen.tsx',import.meta.url),'utf8');const loop=await fs.readFile(new URL('../src/engineLoop.ts',import.meta.url),'utf8');const locale=await fs.readFile(new URL('../src/canvasLocale.ts',import.meta.url),'utf8');assert.match(app,/es-map-region-body/);assert.match(app,/es-map-orbit-action/);assert.match(app,/onPointerMove/);assert.match(app,/БЕСКОНЕЧНОЕ ЯДРО|endless/);for(const token of ['RESONANCE CACHE','BREACH NODE','ECHO RELAY','LOST SIGNAL','ELITE NEST','RUPTURE','BOSS TRACE'])assert.match(loop,new RegExp(token));assert.match(locale,/ТАЙНИК РЕЗОНАНСА/);assert.match(locale,/СЛЕД БОССА/);});
test('Region Endless unlock requires all three challenge satellites',async()=>{const region=await fs.readFile(new URL('../src/region.ts',import.meta.url),'utf8');assert.match(region,/getRegionChallengeProgress\(\)>=3/);assert.match(region,/markRegionChallengeComplete/);assert.match(region,/persistRegionStabilized\(true\)/);});


test('Echo Map uses a pannable star overview and gates challenge satellites behind Standard', async () => {
  const app = await fs.readFile(new URL('../src/EchoMapScreen.tsx', import.meta.url), 'utf8');
  assert.match(app, /regionOpen/);
  assert.match(app, /const locked = !stabilized/);
  assert.match(app, /is-locked/);
  assert.match(app, /loadRegionStabilized/);
  assert.match(app, /Бесконечное|Endless/);
});

test('End-of-run statistics expose kills and field XP HUD is absent', async () => {
  const app = await fs.readFile(new URL('../src/App.tsx', import.meta.url), 'utf8');
  assert.match(app, /kills: number/);
  assert.match(app, /kills: st\.player\.kills/);
  assert.match(app, /gameOverData\.kills/);
  assert.doesNotMatch(app, /ОПЫТ НА ПОЛЕ|XP ON FIELD/);
});
