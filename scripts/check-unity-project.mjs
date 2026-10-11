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
  'Assets/EchoSphere/Scripts/Core/BossCombatRules.cs',
  'Assets/EchoSphere/Scripts/Runtime/BossCombatAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/BossProjectileAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/BossPresentationVfx2D.cs',
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


const runtimeArt = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');

const creatureVisualPath = 'Assets/EchoSphere/Scripts/Runtime/EnemyCreatureVisual2D.cs';
assert.ok(existsSync(join(root, creatureVisualPath)), 'source-mapped enemy creature visual port is missing');
const creatureVisual = readFileSync(join(root, creatureVisualPath), 'utf8');
assert.ok(creatureVisual.includes('src/enemies/enemyVisual.ts') && creatureVisual.includes('DrawCreature') && creatureVisual.includes('DrawBoss'), 'enemy visual must retain explicit source mapping');
for (const variant of ['wisp', 'leech', 'serpent', 'skitter', 'moth', 'beetle', 'brute', 'prism', 'stalker', 'linkbreaker']) assert.ok(creatureVisual.includes('case "' + variant + '"'), 'source creature variant missing: ' + variant);
for (const boss of ['shooter', 'charger', 'summoner']) assert.ok(creatureVisual.includes('case "' + boss + '"'), 'source boss silhouette missing: ' + boss);
assert.ok(creatureVisual.includes('Mesh') && creatureVisual.includes('Legs(') && creatureVisual.includes('Gradient('), 'creatures require mesh paths, gait and layered glow');
assert.ok(creatureVisual.includes('PathScratch') && creatureVisual.includes('_shapeScratch') && creatureVisual.includes('MarkDynamic'), 'animated creature mesh must reuse path/shape buffers');
assert.equal((creatureVisual.match(/new List<Vector2>/g) || []).length, 2, 'per-frame creature rendering must keep exactly two reusable vector buffers');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(creatureVisual), 'presentation animation must not consume run RNG');

const bossPresentationPath = 'Assets/EchoSphere/Scripts/Runtime/BossPresentationVfx2D.cs';
assert.ok(existsSync(join(root, bossPresentationPath)), 'source-mapped boss presentation VFX is missing');
const bossPresentation = readFileSync(join(root, bossPresentationPath), 'utf8');
assert.ok(bossPresentation.includes('renderer.ts') && bossPresentation.includes('drawBossAttackTelegraph'), 'boss overlay must map to the live Canvas renderer path');
for (const boss of ['shooter', 'charger', 'summoner', 'aura']) assert.ok(bossPresentation.includes('"' + boss + '"'), 'live-source boss telegraph missing: ' + boss);
for (const label of ['VOID LANCER', 'BREACHER', 'BROOD MIND', 'AURA TITAN']) assert.ok(bossPresentation.includes(label), 'boss nameplate mapping missing: ' + label);
assert.ok(bossPresentation.includes('SetCombatState') && bossPresentation.includes('MaxHp') && bossPresentation.includes('CurrentHp'), 'boss HUD must receive runtime attack state and health');
assert.ok(bossPresentation.includes('MarkDynamic') && bossPresentation.includes('_vertices.Clear()') && bossPresentation.includes('_triangles.Clear()'), 'boss overlay must reuse dynamic-mesh buffers');
assert.ok((bossPresentation.match(/transform\.localScale\.x\) \* 0\.48f \* 1\.15f/g) || []).length >= 2, 'boss nameplate and HP bar must match scaled enemy silhouette radius');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(bossPresentation), 'boss presentation must not consume gameplay RNG');

const sphereVisualPath = 'Assets/EchoSphere/Scripts/Runtime/SphereRuntimeVfx2D.cs';
assert.ok(existsSync(join(root, sphereVisualPath)), 'source-mapped Sphere family VFX component is missing');
const sphereVisual = readFileSync(join(root, sphereVisualPath), 'utf8');
assert.ok(sphereVisual.includes('src/spheres/*Visual.ts') && sphereVisual.includes('src/spheres/visualHelpers.ts'), 'Sphere VFX source mapping is required');
for (const family of ['Sniper','Chain','Shotgun','Aura','Orbital','Prism','Gravity','Pulse','Void']) assert.ok(sphereVisual.includes('case SphereId.' + family), 'Sphere VFX family dispatch missing: ' + family);
assert.ok(sphereVisual.includes('Ellipse(Vector2.zero') && sphereVisual.includes('DrawDiamond') && sphereVisual.includes('ResonanceEventActive'), 'Sphere family rings, accents and Resonance state must be wired');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(sphereVisual), 'Sphere presentation must not consume gameplay RNG');
assert.equal((sphereVisual.match(/new List<Vector2>/g) || []).length, 1, 'Sphere VFX must use a reusable vector path buffer');

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
assert.ok(!/\b_rng\b|UnityEngine\.Random|Random\.Range/.test(regionVisuals), 'presentation-only region visuals must not consume gameplay RNG');
assert.ok(runtimeArt.includes('LoadAuthored("character-spherist-3q")'), 'the active player must use the source Spherist character, not the Core glyph');
assert.ok(runtimeArt.includes('standard-sphere/upper-crystal') && runtimeArt.includes('standard-sphere/energy-core') && runtimeArt.includes('standard-sphere/stabilization-ring') && runtimeArt.includes('standard-sphere/lower-crystal'), 'Standard Sphere must use the four authored source parts');
assert.ok(runtimeArt.includes('TickSphereArtAnimation') && runtimeArt.includes('ApplyOrbitalSphereVisual'), 'authored Sphere animation and the native Orbital construction are required');
assert.ok(!runtimeArt.includes('LoadAuthored("sphere-standard")') && !runtimeArt.includes('LoadAuthored("sphere-orbital")'), 'the flat prototype SVG substitutions must not be used as source art');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-standard.svg')), 'obsolete flattened Standard Sphere SVG must be removed');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-orbital.svg')), 'obsolete static Orbital mock SVG must be removed');

assert.ok(bootstrap.includes('ApplyModifiedPropertiesWithoutUndo'), 'URP serialized renderer repairs must be applied');

const runtime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
assert.ok(runtime.includes('runtimeVfx.Initialize(this, agent)') && runtime.includes('public bool ResonanceEventActive =>'), 'Sphere VFX must be connected to live gameplay presentation');
assert.ok(runtime.includes('GetEnemyVisualVariant(archetype)') && runtime.includes('GetEnemyVisualVariant(EnemyArchetype archetype)'), 'enemy spawn must wire source-mapped silhouette variants');
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
assert.ok(sphereAttack.includes('VisualRotation') && sphereAttack.includes('targetDelta.y'), 'Sphere emitter presentation must track target direction without altering combat');
const projectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
const enemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(enemy.includes('SetFacing(target - (Vector2)transform.position)') && enemy.includes('SetMarked(true)') && enemy.includes('PulseHit()'), 'enemy visual must track facing and source mark/hit feedback');
assert.ok(enemy.includes('_bossPresentation.Initialize(this, bossType, _baseColor)') && enemy.includes('SetBossPresentationState'), 'boss presentation must bind to EnemyAgent2D');
const bossRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/BossCombatRules.cs'), 'utf8');
for (const boss of ['shooter','charger','summoner','aura','conductor','architect','null','stella_warden']) assert.ok(bossRules.includes('"' + boss + '"'), 'canonical boss roster missing: ' + boss);
assert.ok(bossRules.includes('GetTypeForDefeatedCount') && bossRules.includes('GetMaxHp') && bossRules.includes('GetMoveSpeedWorldUnitsPerSecond'), 'source boss balance rules missing');
const bossAgent = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/BossCombatAgent2D.cs'), 'utf8');
assert.ok(bossAgent.includes('src/engineEnemies.ts') && bossAgent.includes('SpawnBossProjectile') && bossAgent.includes('SpawnBossMinion'), 'boss runtime source mapping/attack spawns missing');
for (const method of ['TickFan','TickRadial','DisableNearestNetworkSpheres','DrainResonance','DelayAllSphereAttacks']) assert.ok(bossAgent.includes(method), 'boss behaviour hook missing: ' + method);
assert.ok(runtimeArt.includes('SpawnBossForRegionEvent') && runtimeArt.includes('BossCombatRules.GetTypeForDefeatedCount'), 'boss spawn must be an explicit Region event contract');
const bossProjectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/BossProjectileAgent2D.cs'), 'utf8');
assert.ok(bossProjectile.includes('renderer.ts:drawBossProjectile') && bossProjectile.includes('_velocity * dt'), 'source boss projectile motion/VFX missing');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(bossProjectile), 'boss projectile presentation must not consume gameplay RNG');
const sphereAttackRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
assert.ok(sphereAttackRuntime.includes('NetworkDisabled') && sphereAttackRuntime.includes('ApplyNetworkDisable') && sphereAttackRuntime.includes('DelayNextAttack'), 'boss-to-Sphere disruption contract missing');
const sphereDisabledVfx = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereRuntimeVfx2D.cs'), 'utf8');
assert.ok(sphereDisabledVfx.includes('_agent.NetworkDisabled') && sphereDisabledVfx.includes('new Color(1f, 0.22f, 0.39f'), 'network-disabled Sphere overlay missing');
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


const projectileVisualPath = 'Assets/EchoSphere/Scripts/Runtime/ProjectileVfx2D.cs';
assert.ok(existsSync(join(root, projectileVisualPath)), 'source-mapped projectile VFX component is missing');
const projectileVisual = readFileSync(join(root, projectileVisualPath), 'utf8');
assert.ok(projectileVisual.includes('src/spheres/mutationVisual.ts') && projectileVisual.includes('renderSphereProjectileVfx') && projectileVisual.includes('projectileTail') && projectileVisual.includes('projectileCore'), 'projectile VFX must retain explicit Canvas source mapping');
for (const family of ['Standard','Sniper','Shotgun','Prism','Void']) assert.ok(projectileVisual.includes('case SphereId.' + family), 'source projectile body dispatch missing: ' + family);
for (const branch of ['standard_singularity','standard_swarm','sniper_oracle','sniper_assassin','sniper_beacon','shotgun_hail','shotgun_cataclysm','prism_split','prism_spectrum','void_hunger','void_reaper','void_execution']) assert.ok(projectileVisual.includes('"' + branch + '"'), 'source projectile mutation path missing: ' + branch);
assert.ok(projectileVisual.includes('MarkDynamic') && projectileVisual.includes('new List<Vector3>') && projectileVisual.includes('new List<Color>'), 'projectile mesh buffers must be dynamic and reused');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(projectileVisual), 'projectile presentation must not consume gameplay RNG');

const elementalVfxPath = 'Assets/EchoSphere/Scripts/Runtime/SphereElementalVfx2D.cs';
assert.ok(existsSync(join(root, elementalVfxPath)), 'source-mapped elemental trace component is missing');
const elementalVfx = readFileSync(join(root, elementalVfxPath), 'utf8');
assert.ok(elementalVfx.includes('src/spheres/elementalVisual.ts') && elementalVfx.includes('src/sphereProgression.ts') && elementalVfx.includes('ResolveElement'), 'elemental visuals must retain their source branch mapping');
for (const branch of ['standard_resonator','standard_singularity','standard_swarm','sniper_oracle','sniper_assassin','sniper_beacon','shotgun_burst','shotgun_cataclysm','shotgun_hail','orbital_dance','orbital_halo','orbital_blade','prism_split','prism_spectrum','prism_mirror','void_hunger','void_reaper','void_execution','chain_web','chain_storm','chain_leech','aura_sanctum','aura_gravity','aura_overgrowth','gravity_well','gravity_tide','gravity_collapse','pulse_wave','pulse_resonator','pulse_burst']) assert.ok(elementalVfx.includes('"' + branch + '"'), 'elemental source branch mapping missing: ' + branch);
assert.ok(elementalVfx.includes('#ff743d') && elementalVfx.includes('#d9f6ff') && elementalVfx.includes('#72f08e') && elementalVfx.includes('MarkDynamic'), 'Fire/Freeze/Poison elemental trace signatures are missing');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(elementalVfx), 'elemental presentation must not consume gameplay RNG');

const impactVisualPath = 'Assets/EchoSphere/Scripts/Runtime/ImpactVfx2D.cs';
assert.ok(existsSync(join(root, impactVisualPath)), 'source-mapped hit impact component is missing');
const impactVisual = readFileSync(join(root, impactVisualPath), 'utf8');
assert.ok(impactVisual.includes('src/renderer.ts') && impactVisual.includes('Impact Ring and Shards') && impactVisual.includes('MarkDynamic'), 'hit impact must retain the source particle/ring mapping');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(impactVisual), 'impact presentation must not consume gameplay RNG');
assert.ok(runtime.includes('projectileVfx.Initialize(this, direction, speed, color, evolutionOwner)'), 'ProjectileAgent spawn path must connect the source-mapped visual');
assert.ok(runtime.includes('elementalVfx.Initialize(this, agent)'), 'all Sphere types must connect branch-aware elemental traces');
assert.ok(runtime.includes('impactVfx.Initialize(color)'), 'hit resolution must connect the layered impact visual');

const combatArcPath = 'Assets/EchoSphere/Scripts/Runtime/CombatArcVfx2D.cs';
assert.ok(existsSync(join(root, combatArcPath)), 'source-mapped chain lightning / Prism refraction component is missing');
const combatArc = readFileSync(join(root, combatArcPath), 'utf8');
assert.ok(combatArc.includes('src/spheres/mutationVisual.ts') && combatArc.includes('renderChainLightningVfx') && combatArc.includes('src/renderer.ts'), 'combat arcs must keep the Canvas source mapping');
assert.ok(combatArc.includes('Chain Lightning Arc') || runtime.includes('Chain Lightning Arc'), 'chain-lightning runtime identity is missing');
for (const branch of ['chain_web','chain_storm','chain_leech','prism_split','prism_mirror']) assert.ok(combatArc.includes('"' + branch + '"'), 'source-mapped arc branch missing: ' + branch);
assert.ok(combatArc.includes('MarkDynamic') && combatArc.includes('new List<Vector2>') && combatArc.includes('new List<Vector3>'), 'combat arc mesh and geometry buffers must be reused');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(combatArc), 'combat arc presentation must not consume gameplay RNG');
assert.ok(runtime.includes('SpawnCombatArc(Vector2 from, Vector2 to') && runtime.includes('arc.Initialize(this, from, to, family, branch, finalId, color, duration)'), 'combat arc visual entry point is missing');
assert.ok(sphereAttack.includes('_runtime.SpawnCombatArc(origin, target.transform.position, SphereId.Chain'), 'Chain targets must connect to the live source-mapped arc');
assert.ok(runtime.includes('SpawnCombatArc(center, targets[i].transform.position, SphereId.Chain') && runtime.includes('SpawnCombatArc(primary.transform.position, target.transform.position, SphereId.Chain'), 'Chain Storm secondary and extra strikes need visual arcs');
assert.ok(runtime.includes('SpawnCombatArc(origin, target.transform.position, SphereId.Prism'), 'Prism Mirror ricochets must connect the source-mapped refraction visual');