import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

import {
  CRIT_BASE,
  CRIT_HARD_CAP,
  CRIT_MULTIPLIER_BASE,
  canReceivePlayerDamage,
  clampCritChance,
  getContextualCritChance,
} from '../src/combatRules.ts';
import {
  LINK_BREAKER_COOLDOWN_SECONDS,
  LINK_BREAKER_DISABLED_SECONDS,
  LINK_BREAKER_TARGET_RANGE,
  LINK_BREAKER_TELEGRAPH_SECONDS,
} from '../src/eliteBalance.ts';

test('critical-hit contract keeps ordinary contextual bonuses under the hard cap', () => {
  assert.equal(CRIT_BASE, 0.05);
  assert.equal(CRIT_MULTIPLIER_BASE, 1.5);
  assert.equal(CRIT_HARD_CAP, 0.75);
  assert.equal(getContextualCritChance(0.65, { hunterMarked: true, architectTriangle: true }), 0.75);
  assert.equal(clampCritChance(3), 0.75);
  assert.equal(clampCritChance(Number.NaN), 0);
});

test('contact-damage grace is separate from Dash invulnerability', () => {
  assert.equal(canReceivePlayerDamage(0, 0), true);
  assert.equal(canReceivePlayerDamage(0.2, 0), false);
  assert.equal(canReceivePlayerDamage(0, 0.6), false);
  assert.equal(canReceivePlayerDamage(Number.NaN, Number.NaN), true);
});

test('Link Breaker has an explicit telegraph, disruption duration and cooldown contract', () => {
  assert.equal(LINK_BREAKER_TELEGRAPH_SECONDS, 0.75);
  assert.equal(LINK_BREAKER_DISABLED_SECONDS, 2);
  assert.equal(LINK_BREAKER_COOLDOWN_SECONDS, 6);
  assert.equal(LINK_BREAKER_TARGET_RANGE, 260);

  const engine = fs.readFileSync(new URL('../src/engineEnemies.ts', import.meta.url), 'utf8');
  const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
  assert.match(engine, /elitePulseTelegraphTimer/);
  assert.match(engine, /elitePulseTarget/);
  assert.match(engine, /LINK_BREAKER_TELEGRAPH_SECONDS/);
  assert.match(renderer, /elitePulseTelegraphTimer/);
  assert.match(renderer, /drawLinkBreakerTelegraph/);
  assert.match(renderer, /networkDisabledTimer/);
});
