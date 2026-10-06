import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

import {
  FOLLOW_ACTIVATE_RADIUS,
  getFormationCentroid,
  canActivateFormationFollow,
  setFormationFollow,
  applyCoreDisplacement,
  updateSphereFollowOffset,
} from '../src/formationFollow.ts';
import { analyzeSphereNetwork } from '../src/network.ts';
import { repositionSphere } from '../src/spaceCollision.ts';

const sphere = (x, y, alive = true, disabled = 0) => ({
  pos: { x, y },
  alive,
  networkDisabledTimer: disabled,
});

function state(overrides = {}) {
  return {
    player: {
      pos: { x: 0, y: 0 },
      formationFollowActive: false,
      sphereMovementLocked: false,
    },
    spheres: [
      sphere(0, -95),
      sphere(95, 0),
      sphere(0, 95),
      sphere(-95, 0),
    ],
    worldWidth: 2400,
    worldHeight: 2400,
    enemies: [],
    ...overrides,
  };
}

test('Formation Centroid uses Network-active Spheres and ignores temporarily disrupted nodes', () => {
  const s = state({
    spheres: [sphere(100, 0), sphere(0, 100), sphere(200, 0, true, 2)],
  });
  assert.deepEqual(getFormationCentroid(s), { x: 50, y: 50 });
});

test('FOLLOW eligibility is independent and uses the configured centroid radius', () => {
  const s = state({
    spheres: [sphere(100, 0), sphere(0, 100)],
  });
  assert.equal(FOLLOW_ACTIVATE_RADIUS, 160);
  assert.equal(canActivateFormationFollow(s), true);
  s.player.pos = { x: 250, y: 50 };
  assert.equal(canActivateFormationFollow(s), false);
});

test('FOLLOW preserves local Sphere offsets while translating the whole formation', () => {
  const s = state();
  assert.equal(setFormationFollow(s, true), true);

  const before = s.spheres.map((sp) => ({ x: sp.pos.x, y: sp.pos.y }));
  applyCoreDisplacement(s, 140, -70);

  assert.deepEqual(s.player.pos, { x: 140, y: -70 });
  for (let i = 0; i < s.spheres.length; i++) {
    assert.deepEqual(
      { x: s.spheres[i].pos.x, y: s.spheres[i].pos.y },
      { x: before[i].x + 140, y: before[i].y - 70 },
    );
  }
});

test('FREE mode leaves Sphere world positions untouched when Core moves', () => {
  const s = state();
  const before = s.spheres.map((sp) => ({ ...sp.pos }));
  applyCoreDisplacement(s, 90, 40);
  assert.deepEqual(s.player.pos, { x: 90, y: 40 });
  assert.deepEqual(s.spheres.map((sp) => sp.pos), before);
});

test('manual reposition updates the local FOLLOW offset', () => {
  const s = state();
  const target = s.spheres[0];
  assert.equal(setFormationFollow(s, true), true);

  // LOCK is intentionally independent from the FOLLOW runtime contract.
  s.player.sphereMovementLocked = true;
  assert.equal(repositionSphere(s, target, 40, -160), true);

  applyCoreDisplacement(s, 30, 20);
  assert.deepEqual(target.pos, { x: 70, y: -140 });
  assert.deepEqual(target.formationFollowOffset, { x: 40, y: -160 });
});

test('rigid FOLLOW translation preserves Network topology and Geometry identity', () => {
  const s = state({
    spheres: [
      sphere(-100, -100),
      sphere(100, -100),
      sphere(100, 100),
      sphere(-100, 100),
    ],
  });
  const before = analyzeSphereNetwork(s.spheres.map((sp) => ({ pos: { ...sp.pos }, alive: sp.alive })));
  setFormationFollow(s, true);
  applyCoreDisplacement(s, 320, 180);
  const after = analyzeSphereNetwork(s.spheres.map((sp) => ({ pos: { ...sp.pos }, alive: sp.alive })));

  assert.deepEqual(after.links, before.links);
  assert.equal(after.dominantFormation?.type, before.dominantFormation?.type);
  assert.deepEqual(after.dominantFormation?.nodes, before.dominantFormation?.nodes);
});

test('movement, Dash, Teleport, collision sync and UI all route through the FOLLOW contract', () => {
  const loop = fs.readFileSync(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
  const abilities = fs.readFileSync(new URL('../src/engineAbilities.ts', import.meta.url), 'utf8');
  const collisions = fs.readFileSync(new URL('../src/spaceCollision.ts', import.meta.url), 'utf8');
  const controls = fs.readFileSync(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

  assert.match(loop, /applyCoreDisplacement\(s, s\.player\.dashDir\.x \* 600 \* dt, s\.player\.dashDir\.y \* 600 \* dt\)/);
  assert.match(loop, /applyCoreDisplacement\(s, mx \* sp \* dt \* followMovementMultiplier, my \* sp \* dt \* followMovementMultiplier\)/);
  assert.match(abilities, /applyCoreDisplacement\(s, target\.x - s\.player\.pos\.x, target\.y - s\.player\.pos\.y\)/);
  assert.match(collisions, /syncFormationFollow\(s\)/);
  assert.match(controls, /data-game-control="formation-follow"/);
  assert.match(controls, /sphereMovementLocked/);
  assert.match(controls, /setFormationFollow/);
});

test('FOLLOW balance layer enforces movement lock, steering inertia and strain floors', () => {
  const formation = fs.readFileSync(new URL('../src/formationFollow.ts', import.meta.url), 'utf8');
  const loop = fs.readFileSync(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
  const abilities = fs.readFileSync(new URL('../src/engineAbilities.ts', import.meta.url), 'utf8');
  const controls = fs.readFileSync(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');
  const collisions = fs.readFileSync(new URL('../src/spaceCollision.ts', import.meta.url), 'utf8');
  assert.match(formation, /FOLLOW_MIN_NETWORK_EFFICIENCY = 0\.72/);
  assert.match(formation, /FOLLOW_MIN_RESONANCE_EFFICIENCY = 0\.65/);
  assert.match(loop, /FOLLOW: DASH LOCKED/);
  assert.match(loop, /getFormationFollowMovementMultiplier/);
  assert.match(loop, /updateFormationFollowMotion/);
  assert.match(abilities, /FOLLOW: TELEPORT LOCKED/);
  assert.match(abilities, /isFormationFollowMovementAbility/);
  assert.match(controls, /isFormationFollowMovementAbility/);
  assert.match(controls, /formationFollowStrain/);
  assert.match(collisions, /FOLLOW_BREACH_ROLES/);
});
