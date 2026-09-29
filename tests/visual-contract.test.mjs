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
  const attacker=c.indexOf('renderOrbitalSphereAttackersVfx');
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
  assert.match(c,/onPointerDown=\{hideNetworkTooltip\}/);
  assert.match(c,/userSelect: 'none'/);
});

test('Directed Sphere families contain weapon geometry',()=>{
  for(const [file,token] of [
    ['src/spheres/sniperVisual.ts','drawWeapon'],
    ['src/spheres/shotgunVisual.ts','barrel'],
    ['src/spheres/prismVisual.ts','prismWeapon'],
    ['src/spheres/voidVisual.ts','voidWeapon'],
  ]) assert.ok(read(file).includes(token),file);
});
