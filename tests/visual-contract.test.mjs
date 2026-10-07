import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root=process.cwd();
const read=(rel)=>fs.readFileSync(path.join(root,rel),'utf8');

test('Standard keeps the approved multi-line attack muzzle',()=>{
  const c=read('src/renderer.ts');
  assert.match(c,/function drawStandardAttackVfx/);
  assert.match(c,/drawStandardAttackVfx\(ctx, sphere, r, time\)/);
});

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

test('Orbital combat satellites use dual true screen-space circular orbits',()=>{
  const c=read('src/spheres/orbitalVisual.ts');
  assert.match(c,/const innerOrbitRadius = r \* 1\.68/);
  assert.match(c,/const outerOrbitRadius = r \* 2\.02/);
  assert.match(c,/const localX = Math\.cos\(a\) \* radius/);
  assert.match(c,/const localY = Math\.sin\(a\) \* radius/);
  const g=read('src/spheres/orbitalGeometry.ts');
  assert.match(g,/ring === 'inner' \? 1 : -1/);
  assert.match(g,/let inner = 1/);
  assert.match(g,/let outer = 1/);
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

test('Orbital combat motion and all three mutation visuals are explicit',()=>{
  const c=read('src/spheres/orbitalVisual.ts');
  assert.match(c,/const bladeMutation = branch === 'orbital_blade'/);
  assert.match(c,/drawSatellite\(ctx, localX, localY, r \* \.20, satelliteBaseColor, depth, a, bladeMutation\)/);
  assert.match(c,/const innerOrbitRadius = r \* 1\.68/);
  assert.match(c,/const outerOrbitRadius = r \* 2\.02/);
  assert.match(c,/getOrbitalRingCounts/);
  assert.match(c,/orbitalRadiusScale/);
  const m=read('src/spheres/mutationVisual.ts');
  assert.match(m,/branch==='orbital_dance'/);
  assert.match(m,/branch==='orbital_halo'/);
  assert.match(m,/branch==='orbital_blade'/);
  const e=read('src/engineSpheres.ts');
  assert.match(e,/const angularSpeed = \(1\.8 \+ Math\.min\(2\.4/);
  assert.match(e,/angularSpeed \*= mods\.rotationSpeed/);
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
  assert.ok(e.includes("getOrbitalRingCounts"));
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


test('Enemy creatures are composed from articulated moving sub-elements',()=>{const c=read('src/enemies/enemyVisual.ts');for(const token of ['function joint','function seg','function articulatedLeg','function pairedLegs','function tail','function wing'])assert.ok(c.includes(token),token);assert.match(c,/Math\.sin\(phase\)/);assert.match(c,/Math\.sin\(t\*3\.0/);});
test('Enemy and boss gameplay-scale sizes follow the visual readability pass',()=>{const c=read('src/engineEnemies.ts');assert.ok(c.includes('let radius = 19;'));assert.ok(c.includes('radius = 15;'));assert.ok(c.includes('radius = 26;'));assert.ok(c.includes('radius: 34,'));});
test('Boss projectile renderer has boss-specific visual families and trails',()=>{const c=read('src/renderer.ts');assert.ok(c.includes('function drawBossProjectile('));for(const token of ["bossType==='shooter'","bossType==='charger'","bossType==='summoner'"])assert.ok(c.includes(token),token);assert.ok(c.includes('bp.vel'));assert.ok(c.includes('r*3.8'));});


test('Enemy anatomy keeps tails rear, limbs paired on body sides, and boss facing follows travel',()=>{
  const c=read('src/enemies/enemyVisual.ts'),r=read('src/renderer.ts');
  assert.match(c,/Tail origin is explicitly at the rear/);
  assert.ok(c.includes('pairedLegs(ctx,r,[-.30,.18],.34,.62'));
  assert.ok(c.includes('pairedLegs(ctx,r,[-.34,.20],.35,.82'));
  assert.match(c,/hip\/knee\/ankle\/foot kinematics|hip\/knee\/ankle/);
  assert.equal(r.includes('const facing=e.isBoss?0:getEnemyFacingAngle(e);'),false);
  assert.ok(r.includes('const facing=getEnemyFacingAngle(e);'));
});
test('Level 7 mutation visuals stay persistent between attacks and differ by branch',()=>{
  const c=read('src/spheres/mutationVisual.ts');
  assert.ok(c.includes('function mutationSilhouette'));
  assert.ok(c.includes('mutationSilhouette(ctx,r,c,time,branch,final);'));
  for(const token of ['sniper_oracle','sniper_assassin','sniper_beacon','shotgun_burst','shotgun_cataclysm','shotgun_hail']) assert.ok(c.includes(token),token);
  assert.ok(c.includes('Mutation silhouette is persistent'));
});


test('Standard Sphere has no separate cannon in authored assembly',()=>{const c=read('src/renderer.ts');const a=c.indexOf('function drawStandardSphereAssembly(');const b=c.indexOf('function getSpheristAnimationState',a);const block=c.slice(a,b>0?b:a+5000);assert.equal(block.includes('cannonAngle'),false);assert.ok(block.includes('drawStandardSphereCore'));assert.ok(block.includes('drawStandardSphereRing'));});

test('Standard Sphere has no persistent barrel between the core and attack tip',()=>{
  const c=read('src/renderer.ts');
  assert.equal(c.includes('function drawStandardPersistentWeapon('),false);
  assert.equal(c.includes('Persistent weapon stays visible through Lv4/Lv7 mutation silhouettes.'),false);
  assert.equal(c.includes('cannonAngle'),false);
  assert.equal(c.includes("roundRect(-r * 0.12"),false);
});
test('Enemy anatomy uses mirrored side legs and rearward tails',()=>{
  const c=read('src/enemies/enemyVisual.ts');
  assert.ok(c.includes('function articulatedLeg('));
  assert.ok(c.includes('function pairedLegs('));
  assert.ok(c.includes('for(const side of [-1,1])'));
  assert.ok(c.includes('Tail origin is explicitly at the rear'));
  assert.ok(c.includes('sx-len*q1'));
});

test('Directed mutation weapon attachments share the live firing axis and render under the main weapon',()=>{
  const m=read('src/spheres/mutationVisual.ts');
  const r=read('src/renderer.ts');
  assert.match(m,/sphereUsesProjectileModifiers\(sphere\.type\)/);
  assert.match(m,/if\(directedWeapon\) ctx\.rotate\(sphere\.rotation\|\|0\)/);
  for(const visual of ['renderSniperSphereRuntimeVfx','renderShotgunSphereRuntimeVfx','renderPrismSphereRuntimeVfx','renderVoidSphereRuntimeVfx']){
    const mi=r.indexOf('renderSphereMutationVfx(ctx, sphere, s.player, time, scale);');
    const vi=r.indexOf(visual,mi);
    assert.ok(mi>=0 && vi>mi,visual+' must be composed after the mutation attachment layer');
  }
});

test('Shield VFX persists and replays a visible pulse on every application',()=>{
  const types=read('src/engineTypes.ts'),state=read('src/engineState.ts'),loop=read('src/engineLoop.ts'),abilities=read('src/engineAbilities.ts'),renderer=read('src/renderer.ts');
  assert.match(types,/shieldVisualPulse: number/);
  assert.match(state,/shieldVisualPulse: 0/);
  assert.match(abilities,/shieldVisualPulse = 0\.85/);
  assert.match(loop,/shieldVisualPulse > 0/);
  assert.match(renderer,/player-spherist-shield/);
  assert.match(renderer,/if \(recast > 0\.02\)/);
  assert.match(renderer,/recastWave/);
});

test('Network HUD exposes factual numeric effects for active Geometry',()=>{
  const a=read('src/App.tsx'),c=read('src/engineCombat.ts'),s=read('src/engineSpheres.ts');
  assert.match(a,/getNetworkTooltipLines/);
  assert.match(a,/percent\(10\)/);
  assert.match(a,/\+1/);
  assert.match(a,/percent\(8\)/);
  assert.match(a,/percent\(6\)/);
  assert.match(a,/percent\(15\)/);
  assert.match(c,/getFormationBonusMultiplier\(getNetworkFrame\(s\), 'line', sphereIndex\)/);
  assert.match(c,/lineBonus > 0\) actual \*= 1 \+ 0\.10 \* lineBonus/);
  assert.match(c,/getFormationBonusMultiplier\(getNetworkFrame\(s\), 'fractal', sphereIndex\)/);
  assert.match(c,/fractalBonus > 0\) actual \*= 1 \+ 0\.15 \* fractalBonus/);
  assert.match(s,/clusterBonus > 0\) pullStrength \*= 1 \+ 0\.20 \* clusterBonus/);
});

test('Level 7 has concrete descriptions for all 90 branch/final variants',()=>{
  const p=read('src/sphereProgression.ts');
  const entries=[...p.matchAll(/f\('([a-z0-9_]+_final_[123])'/g)].map(m=>m[1]);
  assert.equal(entries.length,90,'expected 30 branches × 3 finals');
  assert.equal(new Set(entries).size,90,'final IDs must be unique');
  assert.equal((p.match(/Final form of the [A-Za-z ]+ branch: (Core|Cascade|Apex)\./g)||[]).length,0);
  // Level VII descriptions are gameplay-driven. Core/Cascade/Apex remains
  // only the presentation-layer expansion vocabulary in mutationVisual.ts.
  assert.match(p,/Каждое третье попадание создаёт усиленный импульс вокруг цели/);
  assert.match(p,/После попадания выпускает один осколок, который ищет ближайшую другую цель/);
  assert.match(p,/Призма получает огненный статус для боевых реакций/);
});

test('Level 7 mutation adds an explicit additive expansion for Core/Cascade/Apex',()=>{
  const m=read('src/spheres/mutationVisual.ts');
  assert.match(m,/function drawFinalExpansion/);
  assert.match(m,/type FinalExpansionMode = 'core' \| 'cascade' \| 'apex'/);
  assert.match(m,/Level VII is additive/);
  assert.match(m,/drawFinalExpansion\(ctx,r,c,time,branch,final,sphere\)/);
  const silhouette=m.indexOf('mutationSilhouette(ctx,r,c,time,branch,final);');
  const expansion=m.indexOf('drawFinalExpansion(ctx,r,c,time,branch,final,sphere);');
  assert.ok(silhouette>=0 && expansion>silhouette);
  for(const token of ['standard_resonator','standard_singularity','standard_swarm','sniper_oracle','sniper_assassin','sniper_beacon','shotgun_burst','shotgun_cataclysm','shotgun_hail','chain_web','chain_storm','chain_leech','aura_sanctum','aura_gravity','aura_overgrowth','orbital_dance','orbital_halo','orbital_blade','prism_split','prism_spectrum','prism_mirror','gravity_well','gravity_tide','gravity_collapse','pulse_wave','pulse_resonator','pulse_burst','void_hunger','void_reaper','void_execution']) assert.ok(m.includes(token),token);
});

test('XP crystals keep independent initial rotation and individual animation phase',()=>{
  const t=read('src/engineTypes.ts'),c=read('src/engineCombat.ts'),l=read('src/engineLoop.ts'),r=read('src/renderer.ts');
  assert.match(t,/export interface XPOrb \{[\s\S]*rotation: number;/);
  assert.match(c,/rotation: nextRandom\(s\) \* Math\.PI \* 2/);
  assert.match(l,/orb\.rotation \+= dt \* 1\.4/);
  assert.match(r,/drawModernXp\(ctx, orb\.pos\.x, orb\.pos\.y, orb\.radius, '#63e6ff', orb\.rotation\)/);
  assert.match(r,/function drawModernXp\([^)]*rotation:number/);
  assert.match(r,/const rot=rotation;/);
});

test('Network HUD uses direct visual hierarchy and pointer drag swapping',()=>{
  const a=read('src/App.tsx'),css=read('src/conceptStyle.css');
  assert.equal(a.includes('ДОМИНАНТА'),false);
  assert.equal(a.includes('ДОП.'),false);
  assert.equal(a.includes('Перетащите одну формацию'),false);
  assert.match(a,/setPointerCapture\(event\.pointerId\)/);
  assert.match(a,/document\.elementFromPoint\(event\.clientX, event\.clientY\)/);
  assert.match(a,/networkFormationSelection = \{ dominant: secondary, secondary: dominant \}/);
  assert.match(css,/\.es-network-formation-chip\.is-dominant \.es-network-formation-name/);
  assert.match(css,/\.es-network-formation-chip\.is-secondary \.es-network-formation-name/);
  assert.match(css,/\.es-network-tooltip-active\.is-secondary/);
});
