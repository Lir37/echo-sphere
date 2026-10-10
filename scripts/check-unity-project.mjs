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
  'Assets/EchoSphere/Scripts/Core/SphereNetworkRules.cs',
  'Assets/EchoSphere/Scripts/Core/GameCatalog.cs',
  'Assets/EchoSphere/Scripts/Core/RewardedAdsRules.cs',
  'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs',
  'Assets/EchoSphere/Scripts/Core/ResonanceRules.cs',
  'Assets/EchoSphere/Scripts/Core/EnemySpawnRules.cs',
  'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs',
  'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs',
  'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs',
  'Assets/Editor/EchoSphereSvgImportSettings.cs',
  'Assets/Resources/EchoSphere/Art/player.svg',
  'Assets/Resources/EchoSphere/Art/sphere-aura.svg',
  'Assets/Resources/EchoSphere/Art/sphere-chain.svg',
  'Assets/Resources/EchoSphere/Art/sphere-gravity.svg',
  'Assets/Resources/EchoSphere/Art/sphere-prism.svg',
  'Assets/Resources/EchoSphere/Art/sphere-pulse.svg',
  'Assets/Resources/EchoSphere/Art/sphere-shotgun.svg',
  'Assets/Resources/EchoSphere/Art/sphere-sniper.svg',
  'Assets/Resources/EchoSphere/Art/sphere-void.svg',
  'Assets/Resources/EchoSphere/Art/character-spherist-front.svg',
  'Assets/Resources/EchoSphere/Art/character-spherist-3q.svg',
  'Assets/Resources/EchoSphere/Art/character-spherist-shield.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/energy-core.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/lower-crystal.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/stabilization-ring.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/upper-crystal.png',
  'Assets/Resources/EchoSphere/Art/enemy-boss.svg',
];
assert.ok(existsSync(root), 'UnityProject directory exists');
for (const relative of required) assert.ok(existsSync(join(root, relative)), `missing Unity project file: ${relative}`);

const svgArtPaths = required.filter((relative) =>
  relative.startsWith('Assets/Resources/EchoSphere/Art/') && relative.endsWith('.svg'));
for (const relative of svgArtPaths) {
  const svg = readFileSync(join(root, relative), 'utf8');
  assert.ok(svg.trim().startsWith('<svg') && svg.includes('</svg>'), `invalid SVG document: ${relative}`);
  assert.ok(!/\bcurrentColor\b/i.test(svg), `Unity SVG importer does not support currentColor: ${relative}`);
  assert.ok(!/<filter\b|<fe[A-Za-z]/i.test(svg), `unsupported SVG filter effect: ${relative}`);
}

const versionText = readFileSync(join(root, 'ProjectSettings/ProjectVersion.txt'), 'utf8');
const version = versionText.match(/m_EditorVersion:\s*(\S+)/)?.[1] ?? '';
assert.ok(/^6000\.3\./.test(version), `Unity editor pin must stay on 6000.3 stream, got ${version}`);

const manifest = JSON.parse(readFileSync(join(root, 'Packages/manifest.json'), 'utf8'));
assert.ok(manifest.dependencies?.['com.unity.render-pipelines.universal'], 'URP dependency is required');
assert.equal(manifest.dependencies['com.unity.render-pipelines.universal'], '17.3.0', 'review URP pin if the editor stream changes');
assert.equal(manifest.dependencies['com.unity.vectorgraphics'], '3.0.0-preview.7', 'authored SVG Sprite importer pin must match Unity 6.3');

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

const svgImporterSettings = readFileSync(join(root, 'Assets/Editor/EchoSphereSvgImportSettings.cs'), 'utf8');
assert.ok(svgImporterSettings.includes('void OnPreprocessAsset()'), 'source assets must be configured before import');
assert.ok(svgImporterSettings.includes('TextureImporterType.Sprite'), 'production transparent PNG parts must be imported as Sprites');
assert.ok(svgImporterSettings.includes('alphaIsTransparency = true'), 'authored transparent PNG artwork must preserve alpha');
assert.ok(!svgImporterSettings.includes('SaveAndReimport') && !svgImporterSettings.includes('OnPostprocessAllAssets'), 'asset postprocessor must not start recursive import loops');


const regionVisualsPath = 'Assets/EchoSphere/Scripts/Runtime/RegionFieldVisuals.cs';
assert.ok(existsSync(join(root, regionVisualsPath)), 'source-mapped Region Field visual layer is missing');
const regionVisuals = readFileSync(join(root, regionVisualsPath), 'utf8');
assert.ok(runtimeArt.includes('AddComponent<RegionFieldVisuals>()') && runtimeArt.includes('IsPresentationPaused => _mainMenu || _paused'), 'Region Field must be wired into the active runtime and pause with the presentation');
assert.ok(regionVisuals.includes('src/region.ts') && regionVisuals.includes('renderer.ts'), 'Region Field source mapping must remain explicit');
for (const pocket of [
  'AddPocket("Axis Node", 0f, 0f, 340f, "#39d8ff", true)',
  'AddPocket("Glass Flow", 520f, -260f, 330f, "#7cf7d4", false)',
  'AddPocket("Fracture", 500f, 430f, 330f, "#ff6b6b", false)',
  'AddPocket("Hollow Contour", -480f, 430f, 330f, "#9b7cff", false)',
  'AddPocket("Pressure Field", -520f, -300f, 330f, "#ffb84d", false)',
]) assert.ok(regionVisuals.includes(pocket), `canonical source region pocket missing: ${pocket}`);
assert.ok(regionVisuals.includes('CircleSegments = 96') && regionVisuals.includes('SeamSegments = 10'), 'Region Field ring/seam geometry resolution regressed');
assert.ok(regionVisuals.includes('pocket.Seams.Add') && regionVisuals.includes('pocket.Markers.Add'), 'Region Field fold seams and boundary markers are required');
assert.ok(!/\\b_rng\\b|UnityEngine\\.Random|Random\\.Range/.test(regionVisuals), 'presentation-only region visuals must not consume gameplay RNG');

const runtimeArt = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
assert.ok(runtimeArt.includes('LoadAuthored("character-spherist-3q")'), 'the active player must use the source Spherist character, not the Core glyph');
assert.ok(runtimeArt.includes('standard-sphere/upper-crystal') && runtimeArt.includes('standard-sphere/energy-core') && runtimeArt.includes('standard-sphere/stabilization-ring') && runtimeArt.includes('standard-sphere/lower-crystal'), 'Standard Sphere must use the four authored source parts');
assert.ok(runtimeArt.includes('TickSphereArtAnimation') && runtimeArt.includes('ApplyOrbitalSphereVisual'), 'authored Sphere animation and the native Orbital construction are required');
assert.ok(!runtimeArt.includes('LoadAuthored("sphere-standard")') && !runtimeArt.includes('LoadAuthored("sphere-orbital")'), 'the flat prototype SVG substitutions must not be used as source art');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-standard.svg')), 'obsolete flattened Standard Sphere SVG must be removed');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-orbital.svg')), 'obsolete static Orbital mock SVG must be removed');

assert.ok(bootstrap.includes('ApplyModifiedPropertiesWithoutUndo'), 'URP serialized renderer repairs must be applied');

const runtime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
const spriteFactory = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/RuntimeSpriteFactory.cs'), 'utf8');
assert.ok(runtime.includes('DrawSphereRoster'), 'active Sphere roster HUD is missing');
assert.ok(runtime.includes('DrawSpriteIcon'), 'Sphere choice UI must render visual icons');
assert.ok(runtime.includes('ApplySphereVisual'), 'Sphere archetypes must use distinct visual silhouettes');
assert.ok(runtime.includes('SetUserPaused'), 'pause/resume state handling is missing');
const networkRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereNetworkRules.cs'), 'utf8');
assert.ok(networkRules.includes('Analyze') && networkRules.includes('FindFractal') && networkRules.includes('GetFormationBonusMultiplier'), 'Network/Geometry analysis contract is missing');
assert.ok(runtime.includes('RefreshNetworkState') && runtime.includes('UpdateNetworkLinkVisuals') && runtime.includes('GEOMETRY '), 'live Network links and Geometry HUD are missing');

assert.ok(runtime.includes('RUN PAUSED') && runtime.includes('RESTART RUN'), 'pause overlay controls are missing');
assert.ok(runtime.includes('DrawProgressBar') && runtime.includes('EXPERIENCE') && runtime.includes('_uiScale'), 'responsive color-coded HP/XP/Resonance HUD is missing');
const readMovement = runtime.slice(runtime.indexOf('private Vector2 ReadMovement()'), runtime.indexOf('private void ConfigureCamera()'));
assert.ok(readMovement.includes('Screen.width * 0.58f') && !readMovement.includes('_uiWidth'), 'touch/mouse movement threshold must use device input coordinates, not GUI-scaled coordinates');
assert.ok(runtime.includes('GetAuthoredSphereArtwork(type)') && runtime.includes('return authored'), 'Sphere icon UI must prefer authored source art');
assert.ok(spriteFactory.includes('CreatePolygonSprite'), 'procedural faceted silhouette generation is missing');
assert.ok(spriteFactory.includes('Resources.Load<Sprite>') && runtime.includes('GetAuthoredSphereArtwork'), 'authored SVG runtime binding is missing');
const svgImportSettings = readFileSync(join(root, 'Assets/Editor/EchoSphereSvgImportSettings.cs'), 'utf8');
assert.ok(svgImportSettings.includes('SVGType.TexturedSprite') && svgImportSettings.includes('void OnPreprocessAsset()'), 'SVG artwork must be configured as textured Sprites before import');
assert.ok(!svgImportSettings.includes('SaveAndReimport') && !svgImportSettings.includes('OnPostprocessAllAssets'), 'SVG importer must not recursively reimport assets from a postprocess callback');
assert.ok(runtime.includes('LoadAuthored("character-spherist-3q")') && runtime.includes('character-spherist-3q'), 'the playable Spherist must use the canonical authored character sprite, not the Core glyph');
assert.ok(runtime.includes('ApplyStandardSphereArtwork') && runtime.includes('ApplyOrbitalSphereVisual') && runtime.includes('standard-sphere/energy-core'), 'production Standard assembly and native Orbital 2.5D art paths are missing');
assert.ok(spriteFactory.includes('ES_Diamond') && spriteFactory.includes('ES_Prism') && spriteFactory.includes('ES_Shard'), 'distinct archetype silhouettes are missing');
const resonanceRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ResonanceRules.cs'), 'utf8');
assert.ok(resonanceRules.includes('BaseCap = 100f') && resonanceRules.includes('OverflowCap = 150f'), 'Resonance charge contract is missing');
assert.ok(runtime.includes('TriggerResonanceEvent') && runtime.includes('TickResonanceRing') && runtime.includes('Mathf.FloorToInt(_resonanceCharge)'), 'formation-aware Resonance event runtime/HUD is missing');
const enemySpawnRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/EnemySpawnRules.cs'), 'utf8');
assert.ok(enemySpawnRules.includes('Select(float elapsedSeconds, float roll)') && enemySpawnRules.includes('EnemyArchetype.Elite'), 'deterministic enemy archetype selection is missing');
assert.ok(runtime.includes('GetEnemyPopulationCap') && runtime.includes('GetEnemySpawnInterval'), 'time-scaled enemy pressure controls are missing');
assert.ok(runtime.includes('profile.XpReward') && runtime.includes('GetEnemySprite') && runtime.includes('Initialize(this, _player, xpReward)'), 'enemy role visuals/XP pickup wiring is missing');
assert.ok(runtime.includes('GetFormationKey') && runtime.includes('AddResonanceChargeFromSource'), 'new-geometry Resonance charging is missing');
console.log(`Unity project preflight: PASS (Unity ${version}, ${runtimeFiles.length} C# source files, URP, authored SVG and responsive HUD guards)`);

const evolutionRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs'), 'utf8');
const sphereAttack = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
const projectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
const enemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(sphereAttack.includes('GetOrbitalResonanceBonus') && sphereAttack.includes('ShouldOrbitalGrantShieldCharge'), 'Orbital Halo Resonance/Guard behavior is missing');
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
assert.ok(evolutionRules.includes('GetGravityWellPullDistance') && evolutionRules.includes('GetGravityTideDistance'), 'Gravity evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetOrbitalInnerCount') && evolutionRules.includes('GetOrbitalAngularSpeed'), 'Orbital ring progression rules are missing');
assert.ok(evolutionRules.includes('GetPrismMirrorBounces') && evolutionRules.includes('GetPrismPierce'), 'Prism evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetPulseWaveKnockbackDistance') && evolutionRules.includes('GetVoidExecutionChance'), 'Pulse/Void evolution combat rules are missing');
assert.ok(sphereAttack.includes('OnProjectileHit') && sphereAttack.includes('standard_swarm'), 'Standard evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('sniper_oracle') && sphereAttack.includes('sniper_beacon'), 'Sniper evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('shotgun_cataclysm') && sphereAttack.includes('shotgun_hail'), 'Shotgun evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('chain_web') && sphereAttack.includes('chain_storm') && sphereAttack.includes('chain_leech'), 'Chain evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('aura_sanctum') && sphereAttack.includes('aura_gravity') && sphereAttack.includes('aura_overgrowth'), 'Aura evolution runtime effects are missing');
assert.ok(sphereAttack.includes('gravity_well') && sphereAttack.includes('gravity_tide') && sphereAttack.includes('gravity_collapse'), 'Gravity evolution runtime effects are missing');
assert.ok(sphereAttack.includes('UpdateOrbitalElements') && sphereAttack.includes('UpdateOrbitalContacts'), 'Orbital satellite combat loop is missing');
assert.ok(sphereAttack.includes('prism_split') && sphereAttack.includes('prism_spectrum') && sphereAttack.includes('prism_mirror'), 'Prism evolution runtime effects are missing');
assert.ok(sphereAttack.includes('pulse_wave') && sphereAttack.includes('pulse_burst') && sphereAttack.includes('void_execution'), 'Pulse/Void evolution runtime effects are missing');
assert.ok(runtime.includes('SpawnVoidShards'), 'Void Reaper shard spawning is missing');
assert.ok(runtime.includes('AddResonanceCharge') && runtime.includes('TriggerResonanceEvent') && runtime.includes('TickResonanceRing'), 'Resonance runtime event loop is missing');
assert.ok(sphereAttack.includes('hasTriangleProfile') && sphereAttack.includes('ResonanceRules.NetworkCharge'), 'Prism Spectrum triangle Network charge is missing');
assert.ok(sphereAttack.includes('GetGravityClusterPullMultiplier') && runtime.includes('TriggerChainStorm(SphereAttackAgent source'), 'Gravity Cluster and Chain Storm Network routing are missing');
assert.ok(sphereAttack.includes('clusterCandidate') && sphereAttack.includes('ChargeResonanceFromSource(ResonanceRules.GeometryCharge'), 'Pulse Burst Geometry charge is missing');

assert.ok(sphereAttack.includes('AddResonanceCharge(1f)') && projectile.includes('AddResonanceCharge(1f)'), 'Sphere-hit Resonance charge routing is missing');
assert.ok(enemy.includes('ApplyDamageOverTime'), 'Timed status damage support is missing');
assert.ok(projectile.includes('out var wasCritical'), 'projectile hit must expose critical-hit state to evolution mechanics');
assert.ok(projectile.includes('_evolutionOwner.OnProjectileHit'), 'projectile hit callbacks must reach selected evolution behavior');
assert.ok(runtime.includes('TriggerStandardResonatorPulse') && runtime.includes('SpawnStandardSwarmShards'), 'Standard evolution runtime effects are missing');
