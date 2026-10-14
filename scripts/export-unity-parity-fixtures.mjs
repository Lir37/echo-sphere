import { readFileSync, writeFileSync, existsSync } from 'node:fs';
import { registerHooks } from 'node:module';
registerHooks({ resolve(specifier, context, nextResolve) {
  if (specifier.startsWith('.') && !/\.[a-z]+$/i.test(specifier)) {
    const url = new URL(specifier + '.ts', context.parentURL);
    if (existsSync(url)) return nextResolve(url.href, context);
  }
  return nextResolve(specifier, context);
}});
const { resolveSpaceCollisions } = await import('../src/spaceCollision.ts');
const { sphereModifiers, SPHERE_PROGRESSION, getSphereElementMasteryForBranch } = await import('../src/sphereProgression.ts');
const { getOrbitalRingCounts } = await import('../src/spheres/orbitalGeometry.ts');
const { createRunSeed } = await import('../src/rng.ts');
const fixtures = { collisions: [], levels: [], seeds: [], orbital: [], modifiers: [], mastery: [], killExperience: [] };
// engineCombat.ts rounds once, after applying the current combo to fractional region XP.
for (const reward of [1, 1.12, 2.16, 4.48, 5.6, 11.2, 22.4])
  for (const multiplier of [1, 1.5, 2, 3, 4]) fixtures.killExperience.push({ reward, multiplier, value: Math.round(reward * multiplier) });
let seed = 1;
const next = () => { seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0; return seed / 4294967296; };
for (let example = 0; example < 60; example++) {
  const state = { player: { pos: { x: (next() - .5) * 2500, y: (next() - .5) * 2500 }, formationFollowActive: example >= 30 },
    enemies: [], spheres: [], worldWidth: 2400, worldHeight: 2400 };
  for (let i = 0; i < 12; i++) {
    const type = ['normal', 'fast', 'tank', 'elite', 'boss'][i % 5];
    state.enemies.push({ pos: { x: state.player.pos.x + (next() - .5) * 170, y: state.player.pos.y + (next() - .5) * 170 },
      hp: 10, radius: type === 'fast' ? 15 : type === 'tank' ? 26 : type === 'boss' ? 34 : 19,
      speed: 100 + next() * 120, type, isBoss: type === 'boss', role: i % 3 ? 'grunt' : 'phase' });
  }
  for (let i = 0; i < 6; i++) state.spheres.push({ alive: true, pos: { x: state.player.pos.x + (next() - .5) * 240, y: state.player.pos.y + (next() - .5) * 240 } });
  const input = structuredClone(state); const dt = [0, .016, .05][example % 3];
  resolveSpaceCollisions(state, dt);
  fixtures.collisions.push({ input, dt, player: state.player.pos, enemies: state.enemies.map(e => e.pos) });
}
for (const type of ['standard', 'sniper', 'shotgun', 'chain', 'aura', 'orbital', 'prism', 'gravity', 'pulse', 'void'])
  for (let level = 1; level <= 7; level++) fixtures.levels.push({ type, level,
    mods: sphereModifiers({ player: { abilities: {}, sphereProgression: { [type]: level }, sphereBranches: {}, sphereMods: { rotationSpeed: 1 }, evolutions: [], artifacts: [] }, spheres: [] }, type) });
for (let level = 1; level <= 7; level++) for (let extras = 0; extras <= 2; extras++) fixtures.orbital.push({ level, extras, ...getOrbitalRingCounts(level, extras) });
for (const [type, progression] of Object.entries(SPHERE_PROGRESSION)) {
  const states = [{ branch: '', final: -1 }];
  for (const branch of progression.evolution4Choices) { states.push({ branch: branch.id, final: -1 }); branch.final.forEach((_, final) => states.push({ branch: branch.id, final })); }
  for (const branch of progression.evolution4Choices) for (let final = 0; final < 3; final++) fixtures.mastery.push({ branch: branch.id, final, id: getSphereElementMasteryForBranch(branch.id, final)?.id ?? null });
  for (let level = 0; level <= 7; level++) for (const state of states) {
    if (state.branch && level < 4 || state.final >= 0 && level < 7) continue;
    for (let variant = 0; variant <= 3; variant++) {
      const global = {};
      if (variant > 0) for (const key of ['pierce', 'multishot', 'ricochet', 'fire', 'freeze', 'poison', 'breach', 'overload', 'split', 'shatter', 'execute', 'mark', 'echo', 'anchor', 'phase', 'static', 'resonant', 'magnetic', 'vampiric', 'corrupt', 'drain', 'afterimage', 'impact', 'gravitic']) global[key] = Math.floor(next() * (variant + 1));
      const range = variant;
      const player = { abilities: { range }, sphereProgression: { [type]: level }, sphereBranches: state.branch ? { [type]: state.branch } : {}, sphereMods: global,
        evolutions: state.final >= 0 ? [`sphere:${type}:7:${state.final}`] : [], artifacts: [] };
      fixtures.modifiers.push({ type, level, ...state, global, range, mods: sphereModifiers({ player, spheres: [] }, type) });
    }
  }
}
const epoch = 1791975600123; const savedNow = Date.now; Date.now = () => epoch;
for (const player of ['', 'Антон', 'hunter', '✦']) for (const difficulty of ['easy', 'normal', 'hard', 'nightmare']) fixtures.seeds.push({ player, difficulty, epoch, seed: createRunSeed(player, difficulty) });
Date.now = savedNow;
const output = 'tools/EchoSphere.CoreChecks/source-parity-fixtures.json';
const text = JSON.stringify(fixtures, null, 2) + '\n';
if (process.argv.includes('--check')) {
  if (readFileSync(output, 'utf8').replaceAll('\r\n', '\n') !== text) throw new Error('Parity fixtures differ from source');
} else writeFileSync(output, text);
console.log(`Source parity fixtures: ${fixtures.collisions.length} collision scenarios, ${fixtures.levels.length} Sphere levels, ${fixtures.seeds.length} seeds, ${fixtures.orbital.length} Orbital rings`);
