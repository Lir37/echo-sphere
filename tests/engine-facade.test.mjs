import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

test('engine facade stays thin and runtime modules do not depend on the facade', () => {
  const engine = fs.readFileSync(new URL('../src/engine.ts', import.meta.url), 'utf8');
  const loop = fs.readFileSync(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
  const state = fs.readFileSync(new URL('../src/engineState.ts', import.meta.url), 'utf8');
  const stats = fs.readFileSync(new URL('../src/engineStats.ts', import.meta.url), 'utf8');

  assert.ok(engine.split(/\r?\n/).length < 180, 'engine.ts should remain a small public facade');
  assert.doesNotMatch(engine, /export function update\(/);
  assert.match(engine, /export \{\s*claimStella, applyArtifact, update, activateDash,\s*\} from '\.\/engineLoop';/s);
  assert.match(engine, /export \{\s*getMoveSpeed, getXpMult, getMagnetRadius\s*\} from '\.\/engineStats';/s);
  assert.match(engine, /createInitialState/);
  assert.match(engine, /getCooldownMult/);
  assert.doesNotMatch(loop, /from '\.\/engine';/);
  assert.doesNotMatch(state, /from '\.\/engine';/);
  assert.doesNotMatch(stats, /from '\.\/engine';/);
});
