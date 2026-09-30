import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root=process.cwd();
const read=(rel)=>fs.readFileSync(path.join(root,rel),'utf8');

test('Standard has no active ground shadow',()=>{
  const c=read('src/renderer.ts');
  const a=c.indexOf('function drawStandardSphereAssembly(');
  const b=c.indexOf('function getSpheristAnimationState',a);
  const block=c.slice(a,b>0?b:a+5000);
  assert.equal(block.includes('drawGroundShadow('),false);
});

test('Chain uses lightning only',()=>{
  const c=read('src/engineSpheres.ts');
  const a=c.indexOf('// Chain attack is electrical and instant: no projectile object is spawned.');
  const b=c.indexOf('triggerEngineerRelay(s, sphere);',a);
  const block=c.slice(a,b);
  assert.equal(block.includes('s.sphereProjectiles.push'),false);
  assert.match(block,/s\.lightnings\.push/);
});

test('Orbital attackers are composited after enemies',()=>{
  const c=read('src/renderer.ts');
  const enemy=c.indexOf('for (const e of s.enemies) drawModernEnemy');
  const attacker=c.indexOf('renderOrbitalSphereAttackersVfx(ctx', enemy);
  assert.ok(enemy>=0 && attacker>enemy);
});

test('Network comets use smooth normalized motion',()=>{
  const c=read('src/renderer.ts');
  const a=c.indexOf('function drawWavyNetworkLink(');
  const b=c.indexOf('function drawSphereNetwork(',a);
  const block=c.slice(a,b);
  assert.match(block,/const p=packet%2===0\?raw:1-raw;/);
  assert.equal(block.includes('Math.floor(phaseP*steps)'),false);
});

test('HUD tooltip is timed, tappable and non-selectable',()=>{
  const c=read('src/App.tsx');
  assert.match(c,/networkTooltipAutoHideRef/);
  assert.match(c,/5000/);
  assert.ok(c.includes('onPointerDown={hideNetworkTooltip}'));
  assert.match(c,/userSelect: 'none'/);
});

test('Directed Sphere families contain weapon geometry',()=>{
  for(const [file,token] of [
    ['src/spheres/sniperVisual.ts','drawSniperTip'],
    ['src/spheres/shotgunVisual.ts','drawShotgunTip'],
    ['src/spheres/prismVisual.ts','drawPrismEmitter'],
    ['src/spheres/voidVisual.ts','drawVoidEmitter'],
  ]) assert.ok(read(file).includes(token),file);
});


test('Sphere family shares Standard-derived 2.5D core and orbit grammar',()=>{
  const h=read('src/spheres/visualHelpers.ts');
  assert.match(h,/Family 2\.5D core based on the approved Standard Sphere core/);
  assert.equal(h.includes("ctx.fillStyle = WHITE;"),false);
  for(const file of [
    'src/spheres/sniperVisual.ts','src/spheres/shotgunVisual.ts',
    'src/spheres/chainVisual.ts','src/spheres/auraVisual.ts',
    'src/spheres/prismVisual.ts','src/spheres/gravityVisual.ts',
    'src/spheres/pulseVisual.ts','src/spheres/voidVisual.ts'
  ]){
    const c=read(file);
    assert.ok(c.includes('drawSphereOrbit'),file);
    assert.ok(c.includes('core(ctx'),file);
  }
});

test('Orbital combat satellites use a true screen-space circular orbit',()=>{
  const c=read('src/spheres/orbitalVisual.ts');
  assert.match(c,/const orbitRadius = r \* 1\.68/);
  assert.match(c,/const x = Math\.cos\(a\) \* orbitRadius/);
  assert.match(c,/const y = Math\.sin\(a\) \* orbitRadius/);
  assert.equal(c.includes('Math.cos(a)*r*1.24'),false);
});


test('Directed weapons are compact tips without a shaft back to the core',()=>{
  const cases=[
    ['src/spheres/sniperVisual.ts','r * 1.22'],
    ['src/spheres/shotgunVisual.ts','r * 1.18'],
    ['src/spheres/prismVisual.ts','r * 1.18'],
    ['src/spheres/voidVisual.ts','r * 1.20'],
  ];
  for(const [file,anchor] of cases){
    const c=read(file);
    assert.ok(c.includes(anchor),file);
    assert.equal(/lineTo\(r\*1\./.test(c) && c.includes('drawWeapon'),false,file);
  }
});


test('All Sphere cores reuse the exact Standard core geometry',()=>{
  const h=read('src/spheres/visualHelpers.ts');
  assert.ok(h.includes('const glow = ctx.createRadialGradient(-radius * 0.08, -radius * 0.10, 1, 0, 0, radius * 0.62);'));
  assert.ok(h.includes("ctx.arc(0, 0, radius * 0.58 * (1 + pulse * 0.035)"));
  assert.ok(h.includes('ctx.arc(0, 0, radius * 0.42, 0, Math.PI * 2)'));
  for(const file of [
    'src/spheres/sniperVisual.ts','src/spheres/shotgunVisual.ts',
    'src/spheres/chainVisual.ts','src/spheres/auraVisual.ts',
    'src/spheres/prismVisual.ts','src/spheres/gravityVisual.ts',
    'src/spheres/pulseVisual.ts','src/spheres/voidVisual.ts'
  ]){
    const c=read(file);
    assert.ok(c.includes('const coreR = r;'),file);
  }
});

test('Orbital combat speed and Blade mutation are explicit',()=>{
  const c=read('src/spheres/orbitalVisual.ts');
  assert.match(c,/orbital_dance'\n\s*\? 3\.15/);
  assert.match(c,/orbital_halo'\n\s*\? 1\.55/);
  assert.match(c,/orbital_blade'\n\s*\? 2\.75/);
  assert.match(c,/const bladeMutation = branch === 'orbital_blade'/);
  assert.match(c,/drawSatellite\(ctx, x, y, r \* \.20, color, depth, a, bladeMutation\)/);
  assert.match(c,/ctx\.lineTo\(size \* \.78, 0\)/);
});

test('Enemy production renderer uses authored creature families and boss-specific bodies',()=>{
  const c=read('src/enemies/enemyVisual.ts');
  for(const token of ['drawVeilRipper','drawGraveLeech','drawFangedCoil','drawCarrionSkitter','drawUmbralMoth','drawRiftScarab','drawBonebackBrute','drawGlassHound','drawHollowStalker','drawCableWidow']) assert.ok(c.includes(token),token);
  for(const token of ['drawVoidLancerBoss','drawDreadChargerBoss','drawBroodMatriarchBoss','drawAbyssalLeviathanBoss']) assert.ok(c.includes(token),token);
});

test('Modern renderer delegates enemy bodies to the dedicated production layer',()=>{
  const c=read('src/renderer.ts');
  assert.ok(c.includes("from './enemies/enemyVisual'"));
  assert.ok(c.includes('drawEnemyCreature(ctx,e,t);'));
  assert.ok(c.includes('drawBossCreature(ctx,e,t);'));
  assert.equal(c.includes('drawEnemySilhouette(ctx,e,color,t);'),false);
  assert.equal(c.includes('drawModernBossBody(ctx,e,color,t);'),false);
});

test('Enemy visual variants are seeded and creature-diverse by role',()=>{
  const c=read('src/engineEnemies.ts');
  for(const token of ['moth','skitter','stalker','beetle','brute','prism','wisp','leech','serpent']) assert.ok(c.includes("'"+token+"'"),token);
});


test('Sphere mutations use a dedicated per-branch visual layer',()=>{
  const m=read('src/spheres/mutationVisual.ts');
  const ids=['standard_resonator','standard_singularity','standard_swarm','sniper_oracle','sniper_assassin','sniper_beacon','shotgun_burst','shotgun_cataclysm','shotgun_hail','chain_web','chain_storm','chain_leech','aura_sanctum','aura_gravity','aura_overgrowth','orbital_dance','orbital_halo','orbital_blade','prism_split','prism_spectrum','prism_mirror','gravity_well','gravity_tide','gravity_collapse','pulse_wave','pulse_resonator','pulse_burst','void_hunger','void_reaper','void_execution'];
  for(const id of ids) assert.ok(m.includes(id),id);
  assert.ok(m.includes('renderSphereProjectileVfx'));
  assert.ok(m.includes('renderChainLightningVfx'));
});

test('Projectile-only modifiers are filtered by Sphere type',()=>{
  const h=read('src/gameData.ts'),m=read('src/spheres/modifierVisual.ts'),e=read('src/engineSpheres.ts'),p=read('src/sphereProgression.ts');
  assert.ok(h.includes('sphereUsesProjectileModifiers'));
  assert.match(m,/PROJECTILE_ONLY/);
  assert.ok(m.includes('sphereUsesProjectileModifiers(sphere.type)'));
  assert.ok(e.includes('sphereUsesProjectileModifiers(sphere.type)'));
  assert.ok(p.includes('const projectileMods = sphereUsesProjectileModifiers(type)'));
  assert.ok(e.includes("sphereLevel(s, 'orbital') + 1"));
});

test('Renderer composes mutation and projectile-specific attack visuals',()=>{
  const c=read('src/renderer.ts');
  assert.ok(c.includes("./spheres/mutationVisual"));
  assert.ok(c.includes('renderSphereMutationVfx(ctx, sphere, s.player, time, scale);'));
  assert.ok(c.includes('renderSphereProjectileVfx(ctx, p, s.player, s.time);'));
  assert.ok(c.includes('renderChainLightningVfx(ctx, l, s.player, a, s.time)'));
  assert.equal(c.includes('drawModernProjectile(ctx'),false);
  assert.equal(c.includes('drawStandardMutationVfx(ctx'),false);
});
