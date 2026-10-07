import assert from 'node:assert/strict';
import fs from 'node:fs';

const renderer = fs.readFileSync(new URL('../src/renderer.ts', import.meta.url), 'utf8');
const mobileControls = fs.readFileSync(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');
const conceptStyle = fs.readFileSync(new URL('../src/conceptStyle.css', import.meta.url), 'utf8');

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
assert.match(conceptStyle, /\.es-core-halo\{display:none!important\}/, 'the diffuse menu halo must stay removed');
assert.match(conceptStyle, /\.es-menu-orbit-system\{overflow:visible!important\}/, 'menu orbit container must not clip orbiting controls');
assert.match(conceptStyle, /\.es-menu-orbit-item\{[^}]*border-radius:7px!important/, 'menu controls should remain technical modules, not orb buttons');
assert.match(conceptStyle, /\.es-menu-orbit-item::before,\s*\.es-menu-orbit-item::after\{display:none!important\}/, 'menu controls must not add spherical shells');
assert.match(conceptStyle, /\.es-core-body\{[^}]*transform:none!important/, 'menu Core must remain geometrically centered');
assert.match(renderer, /const boundaryX = canvasW \/ 2 \+ \(s\.player\.pos\.x - s\.camera\.x\) \+ shakeX/, 'boundary FX must be anchored to the player in screen space');


assert.doesNotMatch(conceptStyle, /\.es-main-core-art\{[^}]*filter:drop-shadow/, 'menu Core must not use a broad compositor filter');
assert.match(conceptStyle, /\.es-main-menu\{background:#02050c!important/, 'menu background must not add a centered haze');
assert.match(conceptStyle, /\.es-core-halo,\.es-core-surface,\.es-core-highlight\{display:none!important/, 'obsolete Core haze/surface layers must stay hidden');
assert.match(conceptStyle, /\.es-core-body\{[^}]*inset:8%!important/, 'Core body should occupy the centered Core container evenly');
