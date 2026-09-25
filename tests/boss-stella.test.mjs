import test from 'node:test';
import assert from 'node:assert/strict';

import {
  BOSS_CHARGER_COMMIT_SECONDS,
  BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS,
  BOSS_CHARGER_WINDUP_SECONDS,
  getBossChargerPhase,
} from '../src/engine.ts';

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

test('boss renderer has pre-attack telegraphs for all four boss types', () => {
  assert.match(renderer, /function drawBossAttackTelegraph\(/);
  assert.match(renderer, /e\.bossType === 'charger'/);
  assert.match(renderer, /e\.bossType === 'shooter'/);
  assert.match(renderer, /e\.bossType === 'summoner'/);
  assert.match(renderer, /e\.bossType === 'aura'/);
  assert.match(renderer, /e\.bossShootTimer <= 0\.7/);
  assert.match(renderer, /e\.summonTimer <= 0\.9/);
  assert.match(renderer, /e\.bossShootTimer <= 0\.8/);
  assert.match(renderer, /drawBossAttackTelegraph\(ctx, e, playerPos/);
});

test('boss defeat hands control to Stella without an ordinary artifact roll first', () => {
  assert.match(renderer, /drawBossAttackTelegraph\(ctx, e, playerPos/);
  assert.ok(artifacts.includes("rarity === 'legendary'"));
  assert.ok(artifacts.includes('!owned.has(item.id)'));
  assert.ok(artifacts.includes("rarity !== 'legendary'"));
});
