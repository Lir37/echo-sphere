import test from 'node:test';
import assert from 'node:assert/strict';
import { selectSphereTarget } from '../src/targeting.ts';

const sphere = { x: 0, y: 0 };

test('nearest targeting chooses the closest living enemy', () => {
  const close = { id: 'close', pos: { x: 20, y: 0 }, hp: 10 };
  const far = { id: 'far', pos: { x: 80, y: 0 }, hp: 10 };
  const result = selectSphereTarget(sphere, 100, [far, close], 'nearest');
  assert.equal(result?.id, 'close');
});

test('sniper targeting prefers a high-value target over a closer normal', () => {
  const closeNormal = { id: 'normal', pos: { x: 20, y: 0 }, hp: 10, type: 'normal' };
  const farElite = { id: 'elite', pos: { x: 80, y: 0 }, hp: 10, isElite: true };
  const result = selectSphereTarget(sphere, 100, [closeNormal, farElite], 'high_value_far');
  assert.equal(result?.id, 'elite');
});

test('sniper targeting chooses the farthest target inside the same value tier', () => {
  const nearTank = { id: 'near-tank', pos: { x: 30, y: 0 }, hp: 10, type: 'tank' };
  const farTank = { id: 'far-tank', pos: { x: 90, y: 0 }, hp: 10, type: 'tank' };
  const result = selectSphereTarget(sphere, 100, [nearTank, farTank], 'high_value_far');
  assert.equal(result?.id, 'far-tank');
});

test('target selection ignores dead and out-of-range enemies', () => {
  const dead = { id: 'dead', pos: { x: 10, y: 0 }, hp: 0 };
  const outside = { id: 'outside', pos: { x: 120, y: 0 }, hp: 10 };
  const inside = { id: 'inside', pos: { x: 60, y: 0 }, hp: 10 };
  const result = selectSphereTarget(sphere, 100, [dead, outside, inside], 'nearest');
  assert.equal(result?.id, 'inside');
});
test('sniper targeting recognises the explicit elite enemy role', () => {
  const closeNormal = { id: 'normal', pos: { x: 25, y: 0 }, hp: 10, type: 'normal' };
  const elite = { id: 'elite', pos: { x: 75, y: 0 }, hp: 10, type: 'elite' };
  const result = selectSphereTarget(sphere, 100, [closeNormal, elite], 'high_value_far');
  assert.equal(result?.id, 'elite');
});


test('targeting supports highest and lowest HP priorities', () => {
  const enemies = [
    { pos: { x: 20, y: 0 }, hp: 10, type: 'normal' },
    { pos: { x: 30, y: 0 }, hp: 90, type: 'normal' },
    { pos: { x: 40, y: 0 }, hp: 40, type: 'normal' },
  ];
  assert.equal(selectSphereTarget({x:0,y:0}, 100, enemies, 'highest_hp'), enemies[1]);
  assert.equal(selectSphereTarget({x:0,y:0}, 100, enemies, 'lowest_hp'), enemies[0]);
});
