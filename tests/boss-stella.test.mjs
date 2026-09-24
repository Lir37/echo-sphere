import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const engine = fs.readFileSync(new URL('../src/engine.ts', import.meta.url), 'utf8');
const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
const artifacts = fs.readFileSync(new URL('../src/artifactSystem.ts', import.meta.url), 'utf8');

test('Phase 6 boss charger has a readable wind-up before the committed dash', () => {
  assert.match(engine, /e\.chargeTimer = 0\.8; \/\/ 0\.45s wind-up \+ 0\.35s committed dash/);
  assert.match(engine, /if \(e\.chargeTimer <= 0\.35\)/);
  assert.match(engine, /e\.isCharging = false;\n            e\.chargeTimer = 4;/);
});

test('Phase 6 renderer has pre-attack telegraphs for all four boss types', () => {
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

test('Phase 6 boss defeat hands control to Stella without creating an ordinary artifact roll first', () => {
  assert.match(engine, /if \(enemy\.isBoss\) \{[\s\S]*?s\.pendingStella = true;[\s\S]*?s\.pendingArtifact = null;/);
});

test('Phase 6 Stella scarcity remains gated by the configurable Endless threshold', () => {
  assert.match(engine, /s\.time < STELLA_LEGENDARY_CUTOFF_SECONDS/);
  assert.match(engine, /\? pickStellaArtifactChoice\(s\)/);
  assert.match(engine, /: pickArtifacts\(s\)/);
  assert.match(artifacts, /rarity === 'legendary'/);
  assert.match(artifacts, /!owned\.has\(item\.id\)/);
  assert.match(artifacts, /rarity !== 'legendary'/);
});
