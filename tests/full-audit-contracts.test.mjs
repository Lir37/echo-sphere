import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const read = (path) => fs.readFileSync(new URL('../' + path, import.meta.url), 'utf8');

test('canonical systems are structurally represented', () => {
  const data = read('src/gameData.ts');
  const spheres = [...data.matchAll(/export type SphereType =\s*([^;]+);/gs)][0][1].match(/'[^']+'/g) || [];
  const abilities = [...data.matchAll(/export type AbilityType =\s*([^;]+);/gs)][0][1].match(/'[^']+'/g) || [];
  assert.equal(spheres.length, 10);
  assert.equal(abilities.length, 30);
  const bossBlock = data.slice(data.indexOf('export const BOSS_TYPES'));
  assert.equal((bossBlock.match(/id: '(?:shooter|charger|summoner|aura|conductor|architect|null|stella_warden)'/g) || []).length, 8);
  assert.doesNotMatch(data, /echosphere_map|MAP_THEMES|MapTheme|parchment|bamboo|sunset|ocean/);
});

test('Sphere progression and synergy coverage are complete', () => {
  const progression = read('src/sphereProgression.ts');
  const branches = [...progression.matchAll(/br\('([^']+)'/g)].map((m) => m[1]);
  const finals = [...progression.matchAll(/f\('([^']+)'/g)].map((m) => m[1]);
  const synergies = [...progression.matchAll(/\{id:'([^']+)',sphere:'([^']+)',sphereBranch:'([^']+)',ability:'([^']+)'/g)];
  assert.equal(new Set(branches).size, 30);
  assert.equal(new Set(finals).size, 90);
  assert.equal(synergies.length, 30);
  assert.equal(new Set(synergies.map((m) => m[3])).size, 30);
});

test('gameplay randomness stays seeded outside the renderer', () => {
  const runtime = read('src/characterRuntime.ts');
  const combat = read('src/engineCombat.ts');
  const abilities = read('src/engineAbilities.ts');
  const spheres = read('src/engineSpheres.ts');
  const enemies = read('src/engineEnemies.ts');
  assert.doesNotMatch(runtime, /Math\.random\(\)/);
  for (const source of [combat, abilities, spheres, enemies]) assert.doesNotMatch(source, /Math\.random\(\)/);
  assert.match(runtime, /nextRandom\(s\)/);
});

test('legacy debugging and menu theme controls are gone', () => {
  const engine = read('src/engine.ts');
  const app = read('src/App.tsx');
  const css = read('src/conceptStyle.css');
  const i18n = read('src/i18n.ts');
  assert.doesNotMatch(engine, /debugLevelUp/);
  assert.doesNotMatch(app, /setMapTheme|mapTheme|MAP_THEMES/);
  assert.doesNotMatch(app, /echosphere_map/);
  assert.doesNotMatch(css, /\.es-main-tools|\.es-tech-button/);
  assert.doesNotMatch(i18n, /chooseMap/);
});

test('first-battle tutorial is wired, persistent, and replayable', () => {
  const persistence = read('src/persistence.ts');
  const state = read('src/engineState.ts');
  const loop = read('src/engineLoop.ts');
  const app = read('src/App.tsx');
  const overlay = read('src/TutorialOverlay.tsx');
  const mobile = read('src/MobileControls.tsx');
  assert.match(persistence, /echosphere_tutorial_completed_v1/);
  assert.match(state, /tutorialMode: false/);
  assert.match(loop, /export function openTutorialUpgrade/);
  assert.match(loop, /if \(!s\.tutorialMode\)/);
  assert.match(app, /loadTutorialCompleted/);
  assert.match(app, /onReplayTutorial/);
  assert.match(app, /tutorialStep === 4/);
  assert.match(app, /data-tutorial-target="network"/);
  assert.match(overlay, /ОБУЧЕНИЕ ·/);
  assert.match(overlay, /Выполни действие, чтобы продолжить/);
  assert.match(mobile, /tutorialStep !== 1 && tutorialStep !== 2/);
});

test('Russian user-facing text does not reintroduce known English system terms', () => {
  const sources = [
    read('src/characters.ts'),
    read('src/gameData.ts'),
    read('src/sphereProgression.ts'),
    read('src/runes.ts'),
    read('src/artifactSystem.ts'),
    read('src/KnowledgeBase.tsx'),
    read('src/App.tsx'),
  ];
  const forbidden = /\bru:\s*['"`]([^'"`\n]*(?:\bNetwork\b|\bGeometry\b|\bOverdrive\b|\bCascade\b|\bLevel-Up\b|\bLock\b|\bReroll\b|\bPhantom Node\b|\bRecursive Echo\b|\bSniper\/Chain\b|\bFire\+Freeze\b|\bFire\+Poison\b|\bFreeze\+Poison\b|\bRing\/Lattice\/Fractal\b|\bElite\/Boss\b|\bFormation Memory\b)[^'"`\n]*)['"`]/;
  for (const source of sources) assert.doesNotMatch(source, forbidden);
});

test('main menu exposes Settings as the single home for language and sound', () => {
  const app = read('src/App.tsx');
  const menuStart = app.indexOf('function Menu(');
  const settingsStart = app.indexOf('function SettingsScreen(');
  const menu = app.slice(menuStart, settingsStart);
  assert.doesNotMatch(menu, /setLang|setSoundOn|Volume2|VolumeX|Globe/);
  assert.match(app, /function SettingsScreen\(\{ lang, setLang/);
  assert.match(app, /function SettingsScreen\(\{ lang, setLang, t, soundOn, setSoundOn/);
});
