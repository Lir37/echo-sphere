import test from 'node:test';
import assert from 'node:assert/strict';

import {
  BOSS_CHARGER_COMMIT_SECONDS,
  BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS,
  BOSS_CHARGER_WINDUP_SECONDS,
  getBossChargerPhase,
  BOSS_TELEGRAPH_WINDOWS,
  shouldShowBossTelegraph,
} from '../src/bossBalance.ts';

import fs from 'node:fs';

const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
const artifacts = fs.readFileSync(new URL('../src/artifactSystem.ts', import.meta.url), 'utf8');

test('charger telegraph contract has separate wind-up and committed dash phases', () => {
  assert.equal(BOSS_CHARGER_WINDUP_SECONDS, 0.45);
  assert.equal(BOSS_CHARGER_COMMIT_SECONDS, 0.35);
  assert.equal(BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS, 0.8);
  assert.equal(getBossChargerPhase(0.8), 'windup');
  assert.equal(getBossChargerPhase(0.35), 'committed-dash');
  assert.equal(getBossChargerPhase(0), 'ready');
});

test('boss telegraph timing is a shared runtime contract', () => {
  assert.deepEqual(BOSS_TELEGRAPH_WINDOWS, { shooter: 0.7, summoner: 0.9, aura: 0.8 });
  assert.equal(shouldShowBossTelegraph('shooter', 0.7), true);
  assert.equal(shouldShowBossTelegraph('shooter', 0.71), false);
  assert.equal(shouldShowBossTelegraph('summoner', 0.9), true);
  assert.equal(shouldShowBossTelegraph('aura', 0.81), false);
  assert.ok(renderer.includes('BOSS_TELEGRAPH_WINDOWS.shooter'));
  assert.ok(renderer.includes('BOSS_TELEGRAPH_WINDOWS.summoner'));
  assert.ok(renderer.includes('BOSS_TELEGRAPH_WINDOWS.aura'));
});

test('boss defeat spawns a physical Stella chest without an ordinary artifact roll first', () => {
  const combat = fs.readFileSync(new URL('../src/engineCombat.ts', import.meta.url), 'utf8');
  assert.match(combat, /if \(enemy\.isBoss\) \{[\s\S]*?s\.pendingStella = false;[\s\S]*?s\.stellaChests\.push\([\s\S]*?kind: 'stella'/);
  assert.match(combat, /s\.pendingArtifact = null;/);
});

test('Stella selection keeps Legendary access behind the configured cutoff', () => {
  const loop = fs.readFileSync(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
  const combat = fs.readFileSync(new URL('../src/engineCombat.ts', import.meta.url), 'utf8');
  const runtime = loop + '\n' + combat;
  assert.match(loop, /s\.time < STELLA_LEGENDARY_CUTOFF_SECONDS/);
  assert.match(loop, /pickStellaArtifactChoices\(s, [^\n]*3/);
  assert.match(runtime, /pickArtifacts\(s\)/);
  assert.ok(artifacts.includes("rarity === 'legendary'"));
  assert.ok(artifacts.includes('!owned.has(item.id)'));
  assert.ok(artifacts.includes("rarity !== 'legendary'"));
});
