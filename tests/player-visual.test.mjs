import assert from 'node:assert/strict';
import fs from 'node:fs';

const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
const mobileControls = fs.readFileSync(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

assert.equal(renderer.includes('function drawBerserkerRange'), true, 'Berserker range mechanic must remain preserved');
assert.equal(renderer.includes("if (characterId === 'berserker') drawBerserkerRange"), true, 'Berserker range indicator must remain wired');
const playerStart = renderer.indexOf('function drawPlayer(');
const playerEnd = renderer.indexOf('function drawSpikes(', playerStart);
assert.ok(playerStart >= 0 && playerEnd > playerStart, 'drawPlayer function should exist');
const playerBody = renderer.slice(playerStart, playerEnd);
assert.equal(playerBody.includes('ctx.ellipse('), false, 'player rendering must not add circular status/shield overlays');
assert.match(playerBody, /Do not draw containment spheres, orbit rings, selection rings or status ellipses around it/);
assert.equal(mobileControls.includes('CharacterAvatarOverlay'), false, 'the duplicate center character overlay must remain removed from mobile controls');
assert.equal(mobileControls.includes('data-character-avatar-overlay'), false, 'the obsolete avatar overlay marker must remain removed');

