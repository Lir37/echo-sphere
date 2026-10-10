import assert from 'node:assert/strict';
import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

const root = resolve('UnityProject');
const required = [
  'ProjectSettings/ProjectVersion.txt',
  'Packages/manifest.json',
  'Assets/Editor/EchoSphereProjectBootstrap.cs',
  'Assets/EchoSphere/Scripts/Core/Vec2.cs',
  'Assets/EchoSphere/Scripts/Core/SeededRng.cs',
  'Assets/EchoSphere/Scripts/Core/CombatRules.cs',
  'Assets/EchoSphere/Scripts/Core/FormationFollowRules.cs',
  'Assets/EchoSphere/Scripts/Core/GameCatalog.cs',
  'Assets/EchoSphere/Scripts/Core/RewardedAdsRules.cs',
  'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs',
  'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs',
  'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs',
  'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs',
];
assert.ok(existsSync(root), 'UnityProject directory exists');
for (const relative of required) assert.ok(existsSync(join(root, relative)), `missing Unity project file: ${relative}`);

const versionText = readFileSync(join(root, 'ProjectSettings/ProjectVersion.txt'), 'utf8');
const version = versionText.match(/m_EditorVersion:\s*(\S+)/)?.[1] ?? '';
assert.ok(/^6000\.3\./.test(version), `Unity editor pin must stay on 6000.3 stream, got ${version}`);

const manifest = JSON.parse(readFileSync(join(root, 'Packages/manifest.json'), 'utf8'));
assert.ok(manifest.dependencies?.['com.unity.render-pipelines.universal'], 'URP dependency is required');
assert.equal(manifest.dependencies['com.unity.render-pipelines.universal'], '17.3.0', 'review URP pin if the editor stream changes');

const scriptRoot = join(root, 'Assets/EchoSphere/Scripts');
function walk(directory) {
  const files = [];
  for (const name of readdirSync(directory)) {
    const path = join(directory, name);
    if (statSync(path).isDirectory()) files.push(...walk(path));
    else if (path.endsWith('.cs')) files.push(path);
  }
  return files;
}
const runtimeFiles = walk(scriptRoot);
assert.ok(runtimeFiles.length >= 10, 'runtime/core script coverage unexpectedly small');
for (const file of runtimeFiles) {
  const source = readFileSync(file, 'utf8');
  assert.ok(!/using\s+UnityEditor\b/.test(source), `UnityEditor dependency leaked into runtime: ${file}`);
}
const bootstrap = readFileSync(join(root, 'Assets/Editor/EchoSphereProjectBootstrap.cs'), 'utf8');
assert.ok(bootstrap.includes('EchoSphere_Prototype.unity'), 'first-open scene generation contract missing');
assert.ok(bootstrap.includes('com.lir37.echosphere'), 'Android application id contract missing');

// Regression guard for the real Editor failure observed on 2026-10-09:
// the URP asset can exist while its renderer-data slot is missing or its
// default index is invalid. This is a source-level guard, not a substitute
// for opening the project in Unity Editor.
assert.ok(bootstrap.includes('m_RendererDataList'), 'URP renderer list repair is missing');
assert.ok(bootstrap.includes('m_DefaultRendererIndex'), 'URP default renderer index validation is missing');
assert.ok(bootstrap.includes('UniversalRendererData'), 'bootstrap must be able to create/load a Universal Renderer Data asset');
assert.ok(bootstrap.includes('selectedRendererIsValid'), 'bootstrap must validate the selected default renderer slot');
assert.ok(bootstrap.includes('ApplyModifiedPropertiesWithoutUndo'), 'URP serialized renderer repairs must be applied');

const runtime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
const spriteFactory = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/RuntimeSpriteFactory.cs'), 'utf8');
assert.ok(runtime.includes('DrawSphereRoster'), 'active Sphere roster HUD is missing');
assert.ok(runtime.includes('DrawSpriteIcon'), 'Sphere choice UI must render visual icons');
assert.ok(runtime.includes('ApplySphereVisual'), 'Sphere archetypes must use distinct visual silhouettes');
assert.ok(runtime.includes('SetUserPaused'), 'pause/resume state handling is missing');
assert.ok(runtime.includes('RUN PAUSED') && runtime.includes('RESTART RUN'), 'pause overlay controls are missing');
assert.ok(spriteFactory.includes('CreatePolygonSprite'), 'procedural faceted silhouette generation is missing');
assert.ok(spriteFactory.includes('ES_Diamond') && spriteFactory.includes('ES_Prism') && spriteFactory.includes('ES_Shard'), 'distinct archetype silhouettes are missing');
console.log(`Unity project preflight: PASS (Unity ${version}, ${runtimeFiles.length} C# source files, URP renderer regression guard)`);

const evolutionRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs'), 'utf8');
const sphereAttack = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
const projectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
assert.ok(evolutionRules.includes('ShouldTriggerStandardResonatorPulse'), 'Standard Resonator combat rules are missing');
assert.ok(evolutionRules.includes('GetSniperAssassinDamageMultiplier'), 'Sniper Assassin combat rules are missing');
assert.ok(evolutionRules.includes('GetSniperBeaconRadius'), 'Sniper Beacon combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunHailChance'), 'Shotgun Hail combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunCataclysmSplash'), 'Shotgun Cataclysm combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunBurstDamageMultiplier'), 'Shotgun Burst close-range rules are missing');
assert.ok(evolutionRules.includes('GetShotgunCataclysmPierce'), 'Shotgun Cataclysm pierce rule is missing');
assert.ok(evolutionRules.includes('GetShotgunHailBonusPellets'), 'Shotgun Hail multishot rule is missing');
assert.ok(sphereAttack.includes('ShouldShotgunBurstApplySlow'), 'Shotgun Burst Impact slow is missing');
assert.ok(evolutionRules.includes('GetChainStormRadius') && evolutionRules.includes('GetChainLeechHealRatio'), 'Chain evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetAuraSanctumSlowDuration') && evolutionRules.includes('GetAuraOvergrowthAttackTimerReduction'), 'Aura evolution combat rules are missing');
assert.ok(sphereAttack.includes('OnProjectileHit') && sphereAttack.includes('standard_swarm'), 'Standard evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('sniper_oracle') && sphereAttack.includes('sniper_beacon'), 'Sniper evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('shotgun_cataclysm') && sphereAttack.includes('shotgun_hail'), 'Shotgun evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('chain_web') && sphereAttack.includes('chain_storm') && sphereAttack.includes('chain_leech'), 'Chain evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('aura_sanctum') && sphereAttack.includes('aura_gravity') && sphereAttack.includes('aura_overgrowth'), 'Aura evolution runtime effects are missing');
assert.ok(projectile.includes('out var wasCritical'), 'projectile hit must expose critical-hit state to evolution mechanics');
assert.ok(projectile.includes('_evolutionOwner.OnProjectileHit'), 'projectile hit callbacks must reach selected evolution behavior');
assert.ok(runtime.includes('TriggerStandardResonatorPulse') && runtime.includes('SpawnStandardSwarmShards'), 'Standard evolution runtime effects are missing');
