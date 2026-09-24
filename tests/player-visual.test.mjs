import assert from 'node:assert/strict';
import fs from 'node:fs';

const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
const mobileControls = fs.readFileSync(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

assert.equal(renderer.includes('function drawBerserkerRange'), false, 'legacy player-centered dashed range renderer must stay removed');
assert.equal(renderer.includes("if (characterId === 'berserker') drawBerserkerRange"), false, 'Berserker range must not render a player-centered ring');
const playerStart = renderer.indexOf('function drawPlayer(');
const playerEnd = renderer.indexOf('function drawSpikes(', playerStart);
assert.ok(playerStart >= 0 && playerEnd > playerStart, 'drawPlayer function should exist');
const playerBody = renderer.slice(playerStart, playerEnd);
assert.equal(playerBody.includes('ctx.ellipse('), false, 'player rendering must not add circular status/shield overlays');
assert.match(playerBody, /Do not draw containment spheres, orbit rings, selection rings or status ellipses around it/);
assert.match(
  mobileControls,
  /data-character-avatar-overlay="disabled"/,
  'the duplicate center character overlay must remain explicitly disabled'
);
assert.match(
  mobileControls,
  /display: 'none'/,
  'the duplicate center character overlay must not be visible above the Canvas player'
);

