import assert from 'node:assert/strict';
import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

const root = resolve('UnityProject');
const required = [
  'ProjectSettings/ProjectVersion.txt',
  'Packages/manifest.json',
  'Assets/Editor/EchoSphereProjectBootstrap.cs',
  'Assets/Editor/EchoSphereCiBuild.cs',
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
  'Assets/EchoSphere/Scripts/Core/RegionBossScheduleRules.cs',
  'Assets/EchoSphere/Scripts/Core/RegionUnlockRules.cs',
  'Assets/EchoSphere/Scripts/Core/PersistentUpgradeRules.cs',
  'Assets/EchoSphere/Scripts/Core/DifficultyRules.cs',
  'Assets/EchoSphere/Scripts/Core/ArtifactRewardRules.cs',
  'Assets/EchoSphere/Scripts/Runtime/StellaChestAgent2D.cs',
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
assert.equal(
  (runtimeArt.split('public int GetSphereAuthoredModifierLevel(SphereId type, SphereAuthoredModifier modifier) =>').length - 1),
  1,
  'GetSphereAuthoredModifierLevel must have exactly one declaration (CS0111 regression guard)',
);

const creatureVisualPath = 'Assets/EchoSphere/Scripts/Runtime/EnemyCreatureVisual2D.cs';
assert.ok(existsSync(join(root, creatureVisualPath)), 'source-mapped enemy creature visual port is missing');
const creatureVisual = readFileSync(join(root, creatureVisualPath), 'utf8');
const sourceEnemyRenderer = readFileSync(join(root, '..', 'src/renderer.ts'), 'utf8');
assert.ok(creatureVisual.includes('src/enemies/enemyVisual.ts') && creatureVisual.includes('DrawCreature') && creatureVisual.includes('DrawBoss'), 'enemy visual must retain explicit source mapping');
const sourceCreatureVisual = readFileSync(join(root, '..', 'src/enemies/enemyVisual.ts'), 'utf8');
assert.ok(sourceEnemyRenderer.includes('e.radius*(e.isBoss?1.65:1.15)') && sourceEnemyRenderer.includes('alpha * 0.16') && sourceEnemyRenderer.includes('alpha * 0.55') && sourceCreatureVisual.includes('glow(ctx,r*2.15,color,.065)') && sourceCreatureVisual.includes('glow(ctx,r*2.55,color,.085)'), 'active Canvas enemy renderer must retain the authored outer and creature glow contracts');
assert.ok(creatureVisual.includes('private const float HitFlashDuration = 0.15f;') && creatureVisual.includes('_hitFlash = HitFlashDuration;') && creatureVisual.includes('var hit = Mathf.Clamp01(_hitFlash / HitFlashDuration);'), 'Unity enemy hit feedback must preserve the source 0.15-second flash window');
assert.ok(creatureVisual.includes('transform.position.x / SpherePlacementRules.SourcePixelsToWorld') && creatureVisual.includes('Mathf.Sin(_time * 5f + sourceX * 0.01f) * 0.025f') && creatureVisual.includes('hit * 0.05f'), 'enemy presentation must reproduce source breathing and hit scale without changing gameplay transforms');
assert.ok(creatureVisual.includes('DrawSourceOuterGlow(_p, R * 1.65f, c, 0.21f)') && creatureVisual.includes('DrawSourceOuterGlow(_p, R * 1.15f, c, 0.10f)') && creatureVisual.includes('if (hit > 0f) DrawHitFlash(_p, hit);') && !creatureVisual.includes('_hitFlash / 0.09f'), 'Unity enemy glow/hit overlay must follow active renderer variants rather than tinting the full body');
assert.ok(sourceEnemyRenderer.includes('function drawEnemyStatusVfx') && sourceEnemyRenderer.includes('if(e.fireTimer>0){') && sourceEnemyRenderer.includes('if(e.poisonTimer>0){') && sourceEnemyRenderer.includes('if(e.freezeTimer>0){'), 'active Canvas enemy renderer must retain all three elemental status VFX paths');
assert.ok(creatureVisual.includes('SetStatusVfx(float fireRemaining, float poisonRemaining, float freezeRemaining)') && creatureVisual.includes('DrawElementalStatusVfx') && creatureVisual.includes('if (burning)') && creatureVisual.includes('if (poisoned)') && creatureVisual.includes('if (frozen)'), 'Unity enemy visuals must render fire, poison and freeze only while the corresponding source status is active');
assert.ok(creatureVisual.includes('new Color(105f / 255f, 214f / 255f, 1f, 1f)') && creatureVisual.includes('r * 1.12f') && creatureVisual.includes('r * .72f') && creatureVisual.includes('r * (2.2f + pulse * 1.4f) / pixelsPerRadius'), 'Unity status VFX must retain source freeze tint, ice arc ellipse and fire particle scaling');
const enemyAgentVfx = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(enemyAgentVfx.includes('RefreshElementalStatusVfx()') && enemyAgentVfx.includes('_creatureVisual.SetStatusVfx(_fireTimer, _poisonTimer, _freezeTimer)'), 'elemental status VFX must follow live combat state and clear when Alchemist reactions consume statuses');
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
assert.ok(creatureVisual.includes('DrawBoss(_p, R, c, _time, _bossType)') && !creatureVisual.includes('DrawBoss(_p, R * 1.15f'), 'boss visual radius must not add a second scale over the source-calibrated 34 px root radius');
assert.ok(bossPresentation.includes('Mathf.Abs(transform.localScale.x) * 0.48f') && !bossPresentation.includes('* 0.48f * 1.15f') && bossPresentation.includes('radius + 0.60f'), 'boss HUD/telegraph scale and nameplate offset must follow the corrected source boss radius');
for (const boss of ['shooter', 'charger', 'summoner', 'aura']) assert.ok(bossPresentation.includes('"' + boss + '"'), 'live-source boss telegraph missing: ' + boss);
for (const label of ['VOID LANCER', 'BREACHER', 'BROOD MIND', 'AURA TITAN']) assert.ok(bossPresentation.includes(label), 'boss nameplate mapping missing: ' + label);
assert.ok(bossPresentation.includes('SetCombatState') && bossPresentation.includes('MaxHp') && bossPresentation.includes('CurrentHp'), 'boss HUD must receive runtime attack state and health');
assert.ok(bossPresentation.includes('MarkDynamic') && bossPresentation.includes('_vertices.Clear()') && bossPresentation.includes('_triangles.Clear()'), 'boss overlay must reuse dynamic-mesh buffers');
assert.equal((bossPresentation.match(/transform\.localScale\.x\) \* 0\.48f/g) || []).length, 2, 'boss nameplate and HP bar must use the source-calibrated 34 px root radius');
assert.ok(!bossPresentation.includes('* 0.48f * 1.15f'), 'boss overlays must not keep the removed prototype radius multiplier');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(bossPresentation), 'boss presentation must not consume gameplay RNG');

const sphereVisualPath = 'Assets/EchoSphere/Scripts/Runtime/SphereRuntimeVfx2D.cs';
assert.ok(existsSync(join(root, sphereVisualPath)), 'source-mapped Sphere family VFX component is missing');
const sphereVisual = readFileSync(join(root, sphereVisualPath), 'utf8');
assert.ok(sphereVisual.includes('src/spheres/*Visual.ts') && sphereVisual.includes('src/spheres/visualHelpers.ts'), 'Sphere VFX source mapping is required');
for (const family of ['Sniper','Chain','Shotgun','Aura','Orbital','Prism','Gravity','Pulse','Void']) assert.ok(sphereVisual.includes('case SphereId.' + family), 'Sphere VFX family dispatch missing: ' + family);
assert.ok(sphereVisual.includes('DrawSourceOrbit(') && sphereVisual.includes('OrbitPoint(') &&
  sphereVisual.includes('DrawDiamond') && sphereVisual.includes('ResonanceEventActive'),
  'Sphere family source-orbit geometry, outward accents and Resonance state must be wired');
assert.ok(sphereVisual.includes('private Mesh _orbitMesh, _coreMesh, _detailMesh') &&
  sphereVisual.includes('RadialGlow(') && sphereVisual.includes('CoreOutline') &&
  sphereVisual.includes('private void DisablePrototypeSphereLayers()'),
  'source-matched glowing core must replace the prototype SVG body');
assert.ok(sphereVisual.includes('private static void DrawSourceOrbit(') &&
  sphereVisual.includes('mode == SphereId.Chain') && sphereVisual.includes('mode == SphereId.Void') &&
  sphereVisual.includes('mode == SphereId.Prism'),
  'Sphere families must retain source-specific orbit silhouettes');
assert.ok(sphereVisual.includes('DrawEmitter(_detail, R, color, facing, 0, 0f, disabled, _time)') &&
  sphereVisual.includes('var shapeAlpha = disabled ? .14f') &&
  sphereVisual.includes('var scan = .10f + .05f * Mathf.Sin(time * 7f)'),
  'source Sniper emitter must stay at its forward anchor and dim accurately when disabled');
assert.ok(!sphereVisual.includes('var angles = new[]'),
  'source-matched per-frame Sphere rendering must not allocate a new tick-angle array');
assert.ok(runtimeArt.includes('position, 1.05f, SphereColor(type)') &&
  runtimeArt.includes('NormalEnemyVisualVariants = { "wisp", "leech", "serpent", "stalker", "prism" }') &&
  runtimeArt.includes('FastEnemyVisualVariants = { "moth", "skitter", "stalker" }') &&
  runtimeArt.includes('_rng.NextInt(variants.Length)'),
  'Sphere world size and per-archetype source enemy silhouette variety regressed');
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
assert.ok(readMovement.includes('Screen.width * 0.5f') && !readMovement.includes('_uiWidth'), 'touch/mouse movement threshold must use the original right-half device input region');
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
assert.ok(runtime.includes('GetWaveSpawnInterval') && runtime.includes('GetEnemiesPerWave') && runtime.includes('StartNextWave'), 'source wave count and spawn cadence controls are missing');
assert.ok(runtime.includes('_waveEnemiesToSpawn > 0') && runtime.includes('CountLivingNonBossEnemies() == 0') && runtime.includes('HasActiveBoss()'), 'source wave progression must wait for the current wave and active boss to finish');
assert.ok(runtime.includes('profile.XpReward') && runtime.includes('GetEnemySprite') && runtime.includes('Initialize(this, _player, xpReward)'), 'enemy role visuals/XP pickup wiring is missing');
assert.ok(runtime.includes('GetFormationKey') && runtime.includes('AddResonanceChargeFromSource'), 'new-geometry Resonance charging is missing');

const sphereAttack = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
const spherePlacementRulesPath = 'Assets/EchoSphere/Scripts/Core/SpherePlacementRules.cs';
assert.ok(existsSync(join(root, spherePlacementRulesPath)), 'source-mapped Sphere placement safety rules are missing');
const spherePlacementRules = readFileSync(join(root, spherePlacementRulesPath), 'utf8');
for (const required of ['src/spaceCollision.ts','PlayerBuildGap','TowerPlacementGap','EnemyPlacementGap','HasRouteToOutside','GetMaxSpheres','MaxSameSphereCopies',
  'SphereCharacterId','GetSignatureSphere','GetCharacterSphereCopyCap','GetSameTypeCap','GetCopyDamageMultiplier','SphereCharacterId.Fractal'])
  assert.ok(spherePlacementRules.includes(required), 'source Sphere placement/copy-cap contract missing: ' + required);
assert.ok(runtime.includes('_maxSphereAbilityLevel') && runtime.includes('GetCurrentMaxSpheres') &&
  runtime.includes('SpherePlacementRules.GetMaxSpheres(_maxSphereAbilityLevel, PersistentUpgradeRules.GetExtraSpheres(GetShopUpgradeLevel("spheres")))') &&
  runtime.includes('LevelUpAbilityRules.CreateChoice(RunUpgradeKind.MaxSpheres, _maxSphereAbilityLevel)') &&
  runtime.includes('RunUpgradeKind.MaxSpheres'),
  'source maxspheres passive ability must use the Level-Up catalog and live persistent-shop capacity');
assert.ok(runtime.includes('SpherePlacementRules.GetSameTypeCap') &&
  runtime.includes('_sphereCharacterId') && runtime.includes('_characterMasteryLevel') &&
  !runtime.includes('CountSpheresOfType(_selectedSphereType) >= SpherePlacementRules.MaxSameSphereCopies'),
  'signature Sphere copy limits must follow source character/mastery rules rather than a fixed two-copy runtime cap');
assert.ok(runtime.includes('GetSphereCopyOutputMultiplier') &&
  sphereAttack.includes('_runtime.GetSphereCopyOutputMultiplier(this)') &&
  spherePlacementRules.includes('sameTypeCopyIndex == 2 ? 0.85f : 0.70f'),
  'source duplicate signature Sphere output scaling is not applied to combat damage');
assert.ok(spherePlacementRules.includes('ClampReposition') && runtime.includes('SpherePlacementRules.ClampReposition'),
  'dragged Sphere coordinates must clamp only on source-style reposition commit');
assert.ok(runtime.includes('HandleSpherePointerInput()') && runtime.includes('BeginSpherePointer') && runtime.includes('FinishSpherePointer'), 'Sphere placement/drag pointer lifecycle is not wired into the runtime');
assert.ok(runtime.includes('DrawSpherePicker') && runtime.includes('GetSpherePickerRect') && runtime.includes('SetSelectedSphereType') === false && runtime.includes('_selectedSphereType'),
  'all ten authored Sphere types must be selectable through the live Unity picker');
assert.ok(runtime.includes('TryPlaceOrRemoveSphereAt') && runtime.includes('RemoveSphereAgent') && runtime.includes('CanPlaceSphereAt'),
  'Sphere placement/removal and collision validation are not connected to live runtime');
assert.ok(runtime.includes('BuildSphereNetworkPreview') && runtime.includes('DrawSpherePlacementOverlay') &&
  runtime.includes('CaptureSphereFormationMemoryIfChanged') && runtime.includes('DrawSphereGuiDashedLine') &&
  runtime.includes('DrawSphereGuiDashedCircle') && runtime.includes('GetSphereFormationDisplayName'),
  'Ghost Snap candidate links, predicted formation contour, or formation-memory presentation is not wired');
assert.ok(runtime.includes('DrawSpherePickerGlyph') && runtime.includes('SphereId.Sniper') &&
  runtime.includes('SphereId.Shotgun') && runtime.includes('SphereId.Chain') && runtime.includes('SphereId.Aura'),
  'Sphere picker must port the actual MobileControls glyphs instead of invented text abbreviations');
assert.ok(runtime.includes('GetSpherePickerDescriptionHeight') && runtime.includes('definition.DescriptionEn.Length'),
  'Sphere picker must expand for the source Orbital description instead of clipping it');
const sphereElementalRulesPath = 'Assets/EchoSphere/Scripts/Core/SphereElementalRules.cs';
assert.ok(existsSync(join(root, sphereElementalRulesPath)), 'source Sphere elemental gameplay rules are missing');
const sphereElementalRules = readFileSync(join(root, sphereElementalRulesPath), 'utf8');
for (const required of ['standard_resonator','sniper_oracle','chain_storm','aura_sanctum','gravity_tide','pulse_burst',
  'ResolveElement','GetMasteryId','fire_tempo','freeze_impact','poison_tempo','conduction_rate','field_frequency',
  'GetConductionDamageMultiplier','GetFieldCooldown'])
  assert.ok(sphereElementalRules.includes(required), 'source elemental Mutation rule missing: ' + required);
const sphereEnemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const sphereProjectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
const sphereElementalVfx = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereElementalVfx2D.cs'), 'utf8');
assert.ok(sphereEnemy.includes('ApplyElementalStatus') && sphereEnemy.includes('_fireTimer') &&
  sphereEnemy.includes('_poisonTimer') && sphereEnemy.includes('_freezeTimer') &&
  sphereEnemy.includes('TryAdvanceElementalConduction') && sphereEnemy.includes('TryBeginFieldElementalReaction'),
  'elemental statuses and per-target Chain/Field cooldowns must run on live enemies');
assert.ok(runtime.includes('GetSphereStatusDurationMultiplier') && runtime.includes('GetSphereStatusDamageMultiplier') &&
  sphereAttack.includes('OnSphereHitApplied') && sphereProjectile.includes('_evolutionOwner.OnSphereHitApplied(enemy)'),
  'element status rules must connect through Sphere attacks and projectile impacts');
assert.ok(sphereElementalVfx.includes('return index == 2 ? 2 : index == 3 ? 3 : 1;'),
  'Prism Spectrum presentation must match authored Fire / Freeze / Poison finals');
const spherePickerStart = runtime.indexOf('private void DrawSpherePicker()');
const spherePickerEnd = runtime.indexOf('private void DrawSpherePickerGlyph(', spherePickerStart);
assert.ok(spherePickerStart >= 0 && spherePickerEnd > spherePickerStart, 'source Sphere picker implementation is missing');
const spherePickerImplementation = runtime.slice(spherePickerStart, spherePickerEnd);
assert.ok(!spherePickerImplementation.includes('SphereShortName(') && !spherePickerImplementation.includes('"STD"') && !spherePickerImplementation.includes('"SNP"'),
  'source Sphere picker must use glyphs rather than text abbreviations');
assert.ok(runtime.includes('CreateSphere(SphereId.Standard, 0,') && runtime.includes('_follow = false;'),
  'source initial Standard Sphere/progression state and Formation Follow default are missing');
assert.ok(!runtime.includes('if (choice.CurrentLevel == 0) CreateSphere(choice.Sphere, choice.NextLevel)'),
  'level-up must not spawn a physical Sphere; placement is an independent source action');
assert.ok(sphereAttack.includes('Mathf.Clamp(level, 0, SphereProgressionRules.MaxLevel)'),
  'source unlevelled Sphere state (level 0) is not preserved');
const sphereNetworkRulesPath = 'Assets/EchoSphere/Scripts/Core/SphereNetworkRules.cs';
const sphereNetworkRules = readFileSync(join(root, sphereNetworkRulesPath), 'utf8');
assert.ok(sphereNetworkRules.includes('DefaultLinkDistance = 3.85f'),
  'source 220 px Network link distance must use the current 720/12.6 camera calibration');

console.log(`Unity project preflight: PASS (Unity ${version}, ${runtimeFiles.length} C# source files, URP, authored SVG and responsive HUD guards)`);

const evolutionRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs'), 'utf8');
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
const artifactRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ArtifactRewardRules.cs'), 'utf8');
assert.ok(artifactRules.includes('StellaLegendaryCutoffSeconds = 1500f') && artifactRules.includes('62f') && artifactRules.includes('24f') && artifactRules.includes('9f') && artifactRules.includes('4f'), 'source artifact rarity weights/cutoff missing');
for (const id of ['zero_sphere','unified_mind','time_splitter','stasis_mandala','axiom_core','universal_fold']) assert.ok(artifactRules.includes('"' + id + '"'), 'source Stella legendary missing: ' + id);
assert.equal((artifactRules.match(/new ArtifactRewardDefinition\(/g) || []).length, 82, 'source artifact metadata catalog must contain all 82 artifacts');
const stellaChest = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/StellaChestAgent2D.cs'), 'utf8');
assert.ok(stellaChest.includes('renderer.ts:drawStellaChest') && stellaChest.includes('engineLoop.ts chest proximity collection'), 'Stella chest must map to live source renderer/pickup path');
assert.ok(stellaChest.includes('BossCombatRules.PlayerRadiusSourceUnits+28f') && stellaChest.includes('CollectStellaChest(this)'), 'Stella source pickup distance/event hook missing');
assert.ok(stellaChest.includes('MarkDynamic') && stellaChest.includes('_vertices.Clear()') && stellaChest.includes('_triangles.Clear()'), 'Stella dynamic mesh must reuse buffers');
assert.ok(runtimeArt.includes('SpawnStellaChest(position)') && runtimeArt.includes('CollectStellaChest(StellaChestAgent2D chest)'), 'boss death must produce a collectible Stella cache');
assert.ok(runtimeArt.includes('ArtifactRewardRules.IsLegendaryStellaWindow') && runtimeArt.includes('PickChoices(_ownedArtifacts, _rng.NextFloat, true') && runtimeArt.includes('PickChoices(_ownedArtifacts, _rng.NextFloat, false'), 'Stella rarity-window selection/fallback is missing');
assert.ok(runtimeArt.includes('GetStellaChoiceCount(_ownedArtifacts)') && artifactRules.includes('Contains(owned,"quantum_fold")'), 'Quantum Fold must add a fourth Stella choice');
assert.ok(runtimeArt.includes('ChooseStellaReward(i)') && runtimeArt.includes('_stellaRewardPending'), 'Stella rewards must wait for explicit player selection');
assert.ok(runtimeArt.includes('HealCoreFromDamage(damage)') && runtimeArt.includes('GetArtifactSphereFamilyDamageMultiplier') && runtimeArt.includes('SetDashCooldownMultiplier'), 'artifact combat/stat hooks are not connected');
const playerCore = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/PlayerCoreController2D.cs'), 'utf8');
assert.ok(playerCore.includes('CanReceiveDamageNow') && playerCore.includes('IncreaseMaxHp') && playerCore.includes('SetDashCooldownMultiplier'), 'player artifact stat hooks are missing');
const enemyRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(!enemyRuntime.includes('HealCoreFromDamage(appliedDamage)'), 'vampirism must not heal from arbitrary enemy/status damage');
assert.ok(runtime.includes('if (GUI.Button(button, IsRussianUi ? "ВЫБРАТЬ ЗАБЕГ" : "SELECT RUN", _button))'),
  'localized main-menu button must retain valid IMGUI call syntax');
const regionFieldNamespaceCheck = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/RegionFieldVisuals.cs'), 'utf8');
assert.ok(regionFieldNamespaceCheck.includes('using EchoSphere.Core;'), 'Region field must import the source Region id types');
const persistentUpgradeRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/PersistentUpgradeRules.cs'), 'utf8');
for (const token of ['"dmg"', '"radius"', '"speed"', '"spheres"', '"hp"', '"crit"', '"xp"', 'Math.Pow(3d, currentLevel)'])
  assert.ok(persistentUpgradeRules.includes(token), 'source persistent-shop contract missing: ' + token);
const difficultyRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/DifficultyRules.cs'), 'utf8');
for (const token of ['0.7f, 0.8f, 0.7f, 0.8f, 0.8f', '1.4f, 1.2f, 1.3f, 1.5f, 1.2f', '2f, 1.4f, 1.6f, 2f, 1.5f'])
  assert.ok(difficultyRules.includes(token), 'source Difficulty multipliers missing: ' + token);
assert.ok(runtime.includes('GetShopSphereRadiusMultiplier()') && runtime.includes('PersistentUpgradeRules.GetStartingHpBonus') &&
  runtime.includes('GetShopUpgradeLevel("xp")') && runtime.includes('difficulty.EnemyHpMultiplier') &&
  runtime.includes('difficulty.EnemySpeedMultiplier') && runtime.includes('CurrentDifficulty.SpawnRateMultiplier') &&
  runtime.includes('CurrentDifficulty.EnemyDamageMultiplier'),
  'persistent shop and Difficulty must affect live runtime paths');
const sphereAttackShopRadiusCheck = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
assert.ok(sphereAttackShopRadiusCheck.includes('_runtime.GetShopSphereRadiusMultiplier()'), 'persistent Sphere Radius must affect live Sphere range');
const shopDashCounterCheck = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/PlayerCoreController2D.cs'), 'utf8');
assert.ok(shopDashCounterCheck.includes('DashCount++'), 'Dash counter must follow successful Dash runtime');
const regionUnlockRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/RegionUnlockRules.cs'), 'utf8');
assert.ok(regionUnlockRules.includes('resonanceBasinStabilized') && regionUnlockRules.includes('EchoRegionId.SpectralRift'),
  'Rift unlock must follow source Basin stabilization');
assert.ok(runtime.includes('BeginSelectedRegionRun()') && runtime.includes('RegionUnlockRules.IsAvailable') &&
  runtime.includes('BasinStabilizedPreferenceKey') && runtime.includes('SelectedRegionPreferenceKey') &&
  runtime.includes('_regionSchedule = RegionBossScheduleRules.Create(_selectedRegion, EchoRegionMode.Stabilization)'),
  'Region selector must preserve the selected available Region, persist Basin unlock and use its matching boss schedule');
const regionFieldVisuals = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/RegionFieldVisuals.cs'), 'utf8');
for (const pocket of ['Prism Shift', 'Mirror Shoal', 'Ember Fault', 'Silent Lattice', 'Null Garden', 'Axis Node', 'Glass Flow', 'Pressure Field'])
  assert.ok(regionFieldVisuals.includes(pocket), 'source Region pocket visual missing: ' + pocket);
assert.ok(regionFieldVisuals.includes('SetRegion(EchoRegionId regionId)') && runtime.includes('_regionFieldVisuals?.SetRegion(_selectedRegion)'),
  'selected Region must rebuild the matching source pocket field');
const regionSchedule = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/RegionBossScheduleRules.cs'), 'utf8');
for (const sourceBoss of ['conductor','architect','null','aura','stella_warden']) assert.ok(regionSchedule.includes('"' + sourceBoss + '"'), 'Region schedule source boss missing: ' + sourceBoss);
assert.ok(regionSchedule.includes('480f') && regionSchedule.includes('960f') && regionSchedule.includes('1620f') && regionSchedule.includes('1800f'), 'source Region boss/stabilization timing contract missing');
assert.ok(runtimeArt.includes('AdvanceRegionBossSchedule') && runtimeArt.includes('RegionBossScheduleRules.Advance') && runtimeArt.includes('StartNextWave'), 'Region signal and source wave loop must be wired in live runtime');
assert.ok(runtimeArt.includes('CountLivingNonBossEnemies') && runtimeArt.includes('HasActiveBoss'), 'next source wave must wait for living enemies and boss completion');
const levelUpAbilities = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/LevelUpAbilityRules.cs'), 'utf8');
assert.ok(levelUpAbilities.includes('RoutineAbilityKinds') && levelUpAbilities.includes('GetNetworkDisableDuration'), 'source Level-Up Ability catalog is missing');
for (const token of ['0.93d', '0.15f', '0.10f', '0.16f', 'SourcePixelsToWorld', 'GetDamageTakenMultiplier'])
  assert.ok(levelUpAbilities.includes(token), 'source Level-Up Ability formula is missing: ' + token);
assert.ok(runtime.includes('LevelUpAbilityRules.RoutineAbilityKinds') && runtime.includes('RecordLevelUpChoice(choice)') &&
  runtime.includes('GetSphereAffinityWeight(choice.Sphere)') && runtime.includes('GetAbilityLevel(RunUpgradeKind.Slow)'),
  'Level-Up weighted generation, choice recording and live Ability effects must be wired');
assert.ok(runtime.includes('IncreaseMaxHp(6f)') && runtime.includes('IncreaseMaxHp(20f)') &&
  runtime.includes('GetSphereDamageMultiplier(GetAbilityLevel(RunUpgradeKind.Damage))') &&
  !runtime.includes('REPAIR PROTOCOL') && !runtime.includes('+18% damage for all Spheres'),
  'Level-Up must use source per-level effects and must not offer placeholder repair/damage cards');
const experienceOrb = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ExperienceOrbAgent.cs'), 'utf8');
assert.ok(experienceOrb.includes('GetExperienceMagnetRadiusWorld()'), 'Magnet Level-Up must affect XP orb attraction');
const enemyAbilitySlowCheck = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(enemyAbilitySlowCheck.includes('GetEnemyAbilitySpeedMultiplier()'), 'Slow Level-Up must affect live enemy movement');
const balanceRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/RunBalanceRules.cs'), 'utf8');
assert.ok(balanceRules.includes('GetEnemiesPerWave') && balanceRules.includes('GetWaveSpawnInterval'), 'source wave count and spawn interval rules missing');
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
assert.ok(evolutionRules.includes('GetPrismBeamCount') && evolutionRules.includes('GetPrismRangeWorld') &&
  evolutionRules.includes('GetPrismBranchPower') && evolutionRules.includes('GetPrismBeamDamageMultiplier') &&
  evolutionRules.includes('GetPrismReflectionCount') && evolutionRules.includes('GetPrismMirrorRicochetCount'),
  'Prism direct-beam, source-range and Network-refraction rules are missing');
assert.ok(evolutionRules.includes('GetPulseWaveKnockbackDistance') && evolutionRules.includes('GetVoidExecutionChance'), 'Pulse/Void evolution combat rules are missing');
assert.ok(sphereAttack.includes('OnProjectileHit') && sphereAttack.includes('standard_swarm'), 'Standard evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('sniper_oracle') && sphereAttack.includes('sniper_beacon'), 'Sniper evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('shotgun_cataclysm') && sphereAttack.includes('shotgun_hail'), 'Shotgun evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('chain_web') && sphereAttack.includes('chain_storm') && sphereAttack.includes('chain_leech'), 'Chain evolution hit dispatch is missing');
assert.ok(sphereAttack.includes('aura_sanctum') && sphereAttack.includes('aura_gravity') && sphereAttack.includes('aura_overgrowth'), 'Aura evolution runtime effects are missing');
assert.ok(sphereAttack.includes('gravity_well') && sphereAttack.includes('gravity_tide') && sphereAttack.includes('gravity_collapse'), 'Gravity evolution runtime effects are missing');
assert.ok(sphereAttack.includes('UpdateOrbitalElements') && sphereAttack.includes('UpdateOrbitalContacts'), 'Orbital satellite combat loop is missing');
assert.ok(sphereAttack.includes('prism_mirror') &&
  sphereAttack.includes('GetPrismSourceRangeWorld') &&
  sphereAttack.includes('ResolveElement(branch, finalId)') &&
  sphereAttack.includes('ResolvePrismNetworkRefraction') &&
  sphereAttack.includes('GetPrismBeamDamageMultiplier'),
  'Prism direct-beam source range/branch/status/refraction dispatch is missing');
assert.ok(evolutionRules.includes('130f * SpherePlacementRules.SourcePixelsToWorld') &&
  sphereAttack.includes('GetPrismSourceRangeWorld()') &&
  !sphereAttack.includes('var beamRange = AttackRange'),
  'Prism must use source-calibrated direct-beam range rather than the placeholder 6.8-world range');
assert.ok(sphereAttack.includes('pulse_wave') && sphereAttack.includes('pulse_burst') && sphereAttack.includes('void_execution'), 'Pulse/Void evolution runtime effects are missing');
assert.ok(runtime.includes('SpawnVoidShards'), 'Void Reaper shard spawning is missing');
assert.ok(runtime.includes('AddResonanceCharge') && runtime.includes('TriggerResonanceEvent') && runtime.includes('TickResonanceRing'), 'Resonance runtime event loop is missing');
assert.ok(sphereAttack.includes('HasNetworkCandidateForSphere(SphereNetworkFormation.Triangle') &&
  sphereAttack.includes('ResonanceRules.NetworkCharge') && sphereAttack.includes('ChargeResonanceFromSource'),
  'Prism Spectrum triangle Network charge is missing');
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
assert.ok(sphereAttack.includes('src/engineSpheres.ts:updatePrismSphere') &&
  sphereAttack.includes('GetPrismBeamCount') && sphereAttack.includes('highestHp') &&
  sphereAttack.includes('ReceiveSphereDamage(SphereId.Prism') && sphereAttack.includes('0.10f'),
  'Prism attacks must use source-selected highest-HP targets and immediate direct beams');
assert.ok(sphereAttack.includes('GetPrismFormationDamageMultiplier') &&
  sphereAttack.includes('GetPrismReflectionCount') && sphereAttack.includes('GetLinkedSpheres') &&
  sphereAttack.includes('GetPrismNetworkRelayDamageMultiplier') &&
  sphereAttack.includes('GetPrismRicochetRangeWorld') &&
  sphereAttack.includes('GetPrismRicochetDamageMultiplier') &&
  sphereAttack.includes('HasArtifact("prism_filter")') && sphereAttack.includes('HasArtifact("prism_crown")'),
  'Prism beam damage, linked reflections, Relay and Ricochet must follow source rules');
assert.ok(!sphereAttack.includes('_runtime.SpawnProjectile(origin, direction, beamDamage') &&
  !runtime.includes('TriggerPrismRicochet'),
  'Prism must not fall back to the former travelling-projectile approximation');
const alchemistReactionPath = 'Assets/EchoSphere/Scripts/Core/AlchemistReactionRules.cs';
assert.ok(existsSync(join(root, alchemistReactionPath)), 'source Alchemist elemental reaction rules are missing');
const alchemistReactionRules = readFileSync(join(root, alchemistReactionPath), 'utf8');
for (const token of ['ToxinIgnition', 'Cryotoxin', 'ThermalShock', 'GetBurstMultiplier', 'GetReactionRadiusSourcePixels', 'GetSplashTargetCount', 'GetCatalystDurationSeconds'])
  assert.ok(alchemistReactionRules.includes(token), 'Alchemist source reaction contract missing: ' + token);
assert.ok(runtime.includes('TryTriggerAlchemistReaction') && runtime.includes('AlchemistReactionRules.GetBaseDamage(_level)') &&
  runtime.includes('GetSphereStatusDurationMultiplier(SphereElement element)') &&
  runtime.includes('_alchemistCatalystTimer = Mathf.Max(0f, _alchemistCatalystTimer - dt)'),
  'Alchemist reaction, status-duration Catalyst and runtime timer are not connected');
assert.ok(enemy.includes('GetAlchemistReactionKind') && enemy.includes('ConsumeAlchemistReaction') &&
  enemy.includes('_runtime.TryTriggerAlchemistReaction(this)'),
  'elemental status application must trigger and consume actual Alchemist reactions');
assert.ok(sphereAttack.includes('GetSphereStatusDurationMultiplier(element)'),
  'Sphere elemental status application must use Alchemist Catalyst/mastery duration rules');

const authoredModifierPath = 'Assets/EchoSphere/Scripts/Core/SphereAuthoredModifierRules.cs';
assert.ok(existsSync(join(root, authoredModifierPath)), 'source-authored Sphere modifier catalogue is missing');
const authoredModifiers = readFileSync(join(root, authoredModifierPath), 'utf8');
assert.ok(authoredModifiers.includes('AUTHORED_BASE_MODIFIERS') === false && authoredModifiers.includes('BaseLevels') && authoredModifiers.includes('FinalLevels'), 'source branch/final modifier tables are missing');
assert.ok(authoredModifiers.includes('standard_resonator') && authoredModifiers.includes('void_execution_final_3') === false && authoredModifiers.includes('SphereAuthoredModifier.Gravitic'), 'canonical Sphere modifier mapping is incomplete');
assert.ok(authoredModifiers.includes('BranchCount => BaseLevels.Count') && authoredModifiers.includes('FinalChoiceCount'), 'complete source modifier catalog counts must remain guarded');
assert.ok(runtime.includes('GetSphereAuthoredDamageMultiplier') && runtime.includes('GetSphereAuthoredModifierLevel'), 'source-authored modifier catalogue is not exposed by the live runtime');
assert.ok(runtime.includes('SphereAuthoredModifierRules.GetDamageMultiplier(branch, finalId)') &&
  runtime.includes('ResolveAuthoredSphereHitDamage') && enemy.includes('ReceiveSphereDamage(SphereAttackAgent owner'),
  'source Overload/Shatter/Mark damage must be resolved in the shared Sphere hit pipeline');

assert.ok(enemy.includes('ResolveAuthoredSphereHitDamage') && enemy.includes('ApplyAuthoredSphereHitEffects') &&
  enemy.includes('ReceiveSphereDamage(SphereAttackAgent owner'),
  'source-authored hit modifiers are not wired through the common enemy damage receiver');
assert.ok(runtime.includes('SphereAuthoredModifierRules.GetExecuteDamageMultiplier') &&
  runtime.includes('SphereAuthoredModifierRules.GetCorruptDamageMultiplier') &&
  runtime.includes('SphereAuthoredModifierRules.GetImpactDistancePixels') &&
  runtime.includes('SphereAuthoredModifierRules.GetAnchorSlowMultiplier') &&
  runtime.includes('SphereAuthoredModifierRules.GetVampiricHealFraction') &&
  runtime.includes('SphereAuthoredModifierRules.GetShatterDamageMultiplier'),
  'Execute/Mark/Corrupt or deterministic on-hit modifier effects are missing from runtime');
assert.ok(enemy.includes('RegisterEnemyDeath(this, transform.position, _xpReward, sourceSphere)') &&
  runtime.includes('GetSphereAuthoredModifierLevel(sourceSphere.Value, SphereAuthoredModifier.Drain)'),
  'Drain kill healing must retain the Sphere that delivered the lethal damage');

const projectileAgent = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
assert.ok(authoredModifiers.includes('GetSplitChance') && authoredModifiers.includes('GetEchoChance') &&
  authoredModifiers.includes('GetStaticChance') && authoredModifiers.includes('GetAfterimageChance') &&
  authoredModifiers.includes('GetProjectilePierceBonus'),
  'source random/projection modifier formulas are missing');
assert.ok(runtime.includes('FindNearestUnhitEnemy') && runtime.includes('GetSphereProjectileRicochetCount') &&
  runtime.includes('GetSphereProjectilePierceBonus') && runtime.includes('GetAfterimageDamageMultiplier(afterimageLevel)'),
  'authored projectile pierce/ricochet and Afterimage routing is missing');
assert.ok(projectileAgent.includes('TrySplitAfterHit') && projectileAgent.includes('CopyIgnoredEnemiesFrom') &&
  projectileAgent.includes('350f * SpherePlacementRules.SourcePixelsToWorld') &&
  projectileAgent.includes('_remainingPierces > 0') && projectileAgent.includes('_remainingRicochets > 0'),
  'projectile hit/split/pierce/ricochet source order or hit exclusion is missing');
assert.ok(runtime.includes('GetStaticChance(staticLevel)') && runtime.includes('GetEchoChance(echoLevel)') &&
  runtime.includes('SphereAuthoredModifier.Static'),
  'source Echo/Static hit procs are not connected');
assert.ok(sphereAttack.includes('SpawnAuthoredMainProjectile') &&
  sphereAttack.includes('SphereAuthoredModifier.Multishot') &&
  sphereAttack.includes('GetSphereProjectilePierceBonus(_type)') &&
  sphereAttack.includes('GetPrismBeamCount'),
  'Sphere projectile emission and Prism direct-beam emission must retain their source modifier paths');
assert.ok(evolutionRules.includes('GetProjectilePierceBonus("void_execution", finalId)') &&
  evolutionRules.includes('GetPrismBeamCount') &&
  evolutionRules.includes('GetPrismMirrorRicochetCount') &&
  !evolutionRules.includes('branch == "prism_mirror" && finalId == "prism_mirror_final_2" ? 1'),
  'Void projectile pierce and Prism beam/ricochet rules must use source-authored modifier data');


// Source menu/persistence migration contracts. These guard actual runtime call paths,
// not just the presence of similarly named screen types.
const menuRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
assert.ok(menuRuntime.includes('DrawShopScreen') && menuRuntime.includes('DrawSettingsScreen') &&
  menuRuntime.includes('DrawLeaderboardScreen') && menuRuntime.includes('DrawAchievementsScreen'),
  'the source main-menu Shop/Settings/Leaderboard/Achievements destinations must be reachable from Unity IMGUI');
assert.ok(menuRuntime.includes('MainMenuPage.Shop') && menuRuntime.includes('MainMenuPage.Settings') &&
  menuRuntime.includes('MainMenuPage.Leaderboard') && menuRuntime.includes('MainMenuPage.Achievements'),
  'home navigation must route each implemented source menu destination');
assert.ok(menuRuntime.includes('PersistentUpgradeRules.GetCost(id, level)') &&
  menuRuntime.includes('SaveGold(GetGold() - cost)') && menuRuntime.includes('SaveShopState()'),
  'persistent shop purchases must use source cost/cap rules, deduct gold and persist the level');
assert.ok(menuRuntime.includes('JsonUtility.FromJson<LeaderboardSaveData>') &&
  menuRuntime.includes('SaveLeaderboardEntries()') && menuRuntime.includes('LeaderboardPreferenceKey'),
  'source leaderboard array must be loaded, sorted, capped and saved by the runtime');
assert.ok(menuRuntime.includes('JsonUtility.FromJson<AchievementSaveData>') &&
  menuRuntime.includes('SaveUnlockedAchievementIds()') && menuRuntime.includes('SourceAchievements'),
  'source achievement IDs and the achievement catalogue must be persisted and displayed');
assert.ok(menuRuntime.includes('FinalizeRunResult()') && menuRuntime.includes('_runResultSaved') &&
  menuRuntime.includes('Math.Floor(goldFormula * difficulty.GoldMultiplier)') &&
  menuRuntime.includes('RegionBossScheduleRules.GetBaseGoldReward(_selectedRegion)') &&
  menuRuntime.includes('SaveLeaderboardEntries()'),
  'game-over must award source-formula gold and persist one leaderboard result per run');
assert.ok(menuRuntime.includes('UnlockAchievementIf(_kills >= 500, "kills_500")') &&
  menuRuntime.includes('UnlockAchievementIf(_playerCore != null && _playerCore.DashCount >= 50, "dash_50")') &&
  menuRuntime.includes('UnlockAchievementIf(_runChestOpens >= 5, "chest_5")'),
  'ported source achievement thresholds must consume actual run counters');
assert.ok(menuRuntime.includes('ResetPersistentProgress()') && menuRuntime.includes('PlayerPrefs.DeleteKey(LeaderboardPreferenceKey)') &&
  menuRuntime.includes('PlayerPrefs.DeleteKey(AchievementPreferenceKey)'),
  'settings reset must clear the source persistent progress groups');

// Source character selection/profile migration contracts. These validate selected
// runtime behavior and the complete source data catalogue, not only class names.
const characterRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/CharacterRules.cs'), 'utf8');
const characterPlayerCore = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/PlayerCoreController2D.cs'), 'utf8');
const characterSphereAttackRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
assert.ok(menuRuntime.includes('MainMenuPage.Characters') && menuRuntime.includes('DrawCharacterSelectScreen()') &&
  menuRuntime.includes('SelectOrUnlockCharacter(definition.Id)'),
  'Character Select must be reachable from the home menu and its cards must call the select/unlock flow');
for (const token of [
  'echosphere_character',
  'echosphere_character_profiles',
  'EnsureCharacterProfilesLoaded()',
  'SaveCharacterProfiles()',
  'CharacterRules.GetNextMasteryThreshold(profile.masteryLevel)',
  'CharacterRules.GetMasteryThreshold(mastery.Level)',
  'definition.UnlockCost',
  'SaveGold(GetGold() - definition.UnlockCost)',
]) {
  assert.ok(menuRuntime.includes(token), `source character persistence/select flow missing: ${token}`);
}
for (const token of [
  'SphereCharacterId.Spherist', 'SphereCharacterId.Hunter', 'SphereCharacterId.Engineer',
  'SphereCharacterId.Berserker', 'SphereCharacterId.Alchemist', 'SphereCharacterId.Architect',
  'SphereCharacterId.Conductor', 'SphereCharacterId.Oracle', 'SphereCharacterId.Voidwalker',
  'SphereCharacterId.Fractal', 'MasteryThresholds = { 0, 250, 750, 1500, 2500, 4000, 6000, 8500, 11500, 15000 }',
  'UnlockCost', 'GetBerserkerFuryDamageBonus', 'GetSpheristAttackSpeedBonus',
]) {
  assert.ok(characterRules.includes(token), `source character rules/data missing: ${token}`);
}
assert.ok((characterRules.match(/new CharacterDefinition\(/g) || []).length === 10 &&
  (characterRules.match(/new CharacterMasteryDefinition\(10,/g) || []).length === 10,
  'CharacterRules must retain all ten source characters and all ten levels of mastery per character');
assert.ok(menuRuntime.includes('GetCharacterGlobalSphereDamageMultiplier()') &&
  menuRuntime.includes('GetCharacterAttackSpeedMultiplier()') &&
  menuRuntime.includes('GetCharacterMoveSpeedMultiplier()') &&
  menuRuntime.includes('GetCharacterDamageTakenMultiplier()') &&
  menuRuntime.includes('GetCharacterSphereAffinityWeight(_sphereCharacterId, type)'),
  'selected character base combat modifiers must be connected to actual Unity runtime call paths');
assert.ok(characterPlayerCore.includes('ApplyBaseMaxHpMultiplier(1f + CharacterRules.Get(_sphereCharacterId).MaxHp)') ||
  (menuRuntime.includes('ApplyBaseMaxHpMultiplier(1f + CharacterRules.Get(_sphereCharacterId).MaxHp)') &&
   characterPlayerCore.includes('ApplyBaseMaxHpMultiplier(float multiplier)')),
  'selected character maximum HP modifier must change the actual player Core');
assert.ok(characterSphereAttackRuntime.includes('GetCharacterSphereRadiusMultiplier()') &&
  characterSphereAttackRuntime.includes('_runtime.GetCharacterSphereRadiusMultiplier()'),
  'selected character sphere-radius modifier must affect live Sphere attack range');
assert.ok(menuRuntime.includes('PlayerPrefs.SetString(CharacterPreferenceKey, CharacterRules.ToSourceId(_sphereCharacterId))') &&
  menuRuntime.includes('ResetCharacterProgress()'),
  'selected character and its persistent profiles must use source keys and participate in progress reset');

// CI-only workflow guards. The Unity Editor/APK cloud build is intentionally
// disabled; this workflow must keep validating contracts and project structure
// without requiring Unity license secrets or invoking GameCI.
const ciBuildPath = 'Assets/Editor/EchoSphereCiBuild.cs';
const ciBuild = readFileSync(join(root, ciBuildPath), 'utf8');
assert.ok(ciBuild.includes('BuildAndroidDevelopmentApk'), 'Unity Android build entry point should remain available for a future explicitly approved build');
assert.ok(ciBuild.includes('EchoSphereProjectBootstrap.EnsurePrototypeScene()'), 'future Android build entry point must generate/register the bootstrap scene');
assert.ok(ciBuild.includes('BuildPipeline.BuildPlayer') && ciBuild.includes('BuildTarget.Android'), 'future Android build entry point must use Unity BuildPipeline');

const ciWorkflowPath = join(root, '..', '.github', 'workflows', 'unity-android-apk.yml');
assert.ok(existsSync(ciWorkflowPath), 'Unity checks workflow is missing');
const ciWorkflow = readFileSync(ciWorkflowPath, 'utf8');
for (const token of [
  'workflow_dispatch:',
  'dotnet run --project tools/EchoSphere.CoreChecks/EchoSphere.CoreChecks.csproj --configuration Release',
  'node scripts/check-unity-project.mjs',
]) {
  assert.ok(ciWorkflow.includes(token), `Unity CI-only workflow missing expected check: ${token}`);
}
for (const forbidden of [
  'game-ci/unity-builder@v4',
  'UNITY_LICENSE',
  'UNITY_EMAIL',
  'UNITY_PASSWORD',
  'echo-sphere-unity-debug-apk',
]) {
  assert.ok(!ciWorkflow.includes(forbidden), `APK build must remain disabled in the CI-only workflow: ${forbidden}`);
}
