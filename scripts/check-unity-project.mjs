import assert from 'node:assert/strict';
import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

const root = resolve('UnityProject');
const readRuntime = () => readdirSync(join(root, 'Assets/EchoSphere/Scripts/Runtime'))
  .filter(name => /^EchoSphere.*\.cs$/.test(name))
  .map(name => readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime', name), 'utf8').replaceAll('\r\n', '\n')).join('\n');
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
  'Assets/EchoSphere/Scripts/Core/EngineerRelayRules.cs',
  'Assets/EchoSphere/Scripts/Core/EngineerRelayRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ConductorOverdriveRules.cs',
  'Assets/EchoSphere/Scripts/Core/ConductorOverdriveRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/LevelUpChoiceRules.cs',
  'Assets/EchoSphere/Scripts/Core/LevelUpChoiceRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/FormationHitRules.cs',
  'Assets/EchoSphere/Scripts/Core/FormationHitRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ArtifactSynergyRules.cs',
  'Assets/EchoSphere/Scripts/Core/ArtifactSynergyRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ResonanceGeometryMemoryRules.cs',
  'Assets/EchoSphere/Scripts/Core/ResonanceGeometryMemoryRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ArtifactSetRules.cs',
  'Assets/EchoSphere/Scripts/Core/ArtifactSetRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ArtifactProtocolRules.cs',
  'Assets/EchoSphere/Scripts/Core/ArtifactProtocolRules.cs.meta',
  'Assets/EchoSphere/Scripts/Core/ComboRules.cs',
  'Assets/EchoSphere/Scripts/Core/ComboRules.cs.meta',
  'Assets/EchoSphere/Scripts/Runtime/StellaChestAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/RuneAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/HealthPackAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/ArtifactChestAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/BossCombatAgent2D.cs',
  'Assets/EchoSphere/Scripts/Runtime/BossProjectileAgent2D.cs',
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
  'Assets/Resources/EchoSphere/Art/standard-sphere/stabilization-ring.png',
  'Assets/Resources/EchoSphere/Art/enemy-boss.svg',
];
assert.ok(existsSync(root), 'UnityProject directory exists');
for (const relative of required) assert.ok(existsSync(join(root, relative)), `missing Unity project file: ${relative}`);

const invalidUpstreamPngs = [
  'Assets/Resources/EchoSphere/Art/character-spherist-shield.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/energy-core.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/lower-crystal.png',
  'Assets/Resources/EchoSphere/Art/standard-sphere/upper-crystal.png',
];
// These upstream blobs fail strict PNG/DEFLATE validation. The active CanvasArt
// derivatives preserve their measured browser result (fully transparent pixels)
// without asking Unity to import the malformed originals.
for (const relative of invalidUpstreamPngs)
  assert.ok(!existsSync(join(root, relative)), `corrupt upstream PNG must not be imported by Unity: ${relative}`);

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
assert.equal(version, '6000.6.1f1', 'native backdropFilter requires the approved Unity 6.6 pin');

const manifest = JSON.parse(readFileSync(join(root, 'Packages/manifest.json'), 'utf8'));
// UnityEngine.TextRenderingModule is an engine assembly, not a resolvable
// com.unity.modules.textrendering UPM package. The latter blocks project import.
assert.ok(!Object.hasOwn(manifest.dependencies ?? {}, 'com.unity.modules.textrendering'),
  'invalid UPM dependency: com.unity.modules.textrendering does not exist; UnityEngine.TextRenderingModule is an engine assembly');
assert.ok(manifest.dependencies?.['com.unity.render-pipelines.universal'], 'URP dependency is required');
assert.equal(manifest.dependencies['com.unity.render-pipelines.universal'], '17.6.0', 'URP pin must match the approved editor stream');
assert.equal(manifest.dependencies['com.unity.vectorgraphics'], '3.0.0-preview.7', 'preserve the verified SVG importer');

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
assert.ok(bootstrap.includes('private const string ScenePath = "Assets/Scenes/EchoSphere_NativeBattleSlice.unity"'), 'authored native scene must be the bootstrap entry');
const buildSettings = readFileSync(join(root, 'ProjectSettings/EditorBuildSettings.asset'), 'utf8');
assert.equal([...buildSettings.matchAll(/path: (Assets\/Scenes\/[^\r\n]+)/g)][0]?.[1], 'Assets/Scenes/EchoSphere_NativeBattleSlice.unity', 'native scene must occupy build index 0');
assert.ok(!/enabled: 1\s+path: Assets\/Scenes\/EchoSphere_Prototype\.unity/.test(buildSettings), 'prototype entry must stay disabled');
assert.ok(bootstrap.includes('com.lir37.echosphere'), 'Android application id contract missing');
assert.ok(bootstrap.includes('SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP)'), 'Android ARM64 requires the IL2CPP backend');
assert.ok(bootstrap.includes('AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64'), 'Android must retain ARMv7 support and include ARM64');
assert.ok(bootstrap.includes('SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard)'), 'Android runtime must use the typechecked .NET Standard profile');
const sourceLinker = readFileSync(join(root, 'Assets/EchoSphere/link.xml'), 'utf8');
for (const space of ['Core', 'Runtime', 'Presentation'])
  assert.ok(sourceLinker.includes(`fullname="EchoSphere.${space}.*" preserve="all"`), `Retain the existing IL2CPP reflection/snapshot preservation until a player stripping test is available: ${space}`);
const uiFilterShader = readFileSync(join(root, 'Assets/Resources/EchoSphere/SourceUiFilters.shader'), 'utf8');
for (const program of uiFilterShader.matchAll(/HLSLPROGRAM([\s\S]*?)ENDHLSL/g))
  assert.ok(program[1].includes('#pragma target 3.5'), 'UI filter rectangle indices require an explicit GLES3-compatible shader target');

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


const runtimeArt = readRuntime();
assert.equal(
  (runtimeArt.split('public int GetSphereAuthoredModifierLevel(SphereId type, SphereAuthoredModifier modifier) =>').length - 1),
  1,
  'GetSphereAuthoredModifierLevel must have exactly one declaration (CS0111 regression guard)',
);

const nativeEnemyView = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Presentation/NativeWorldEnemyView.cs'), 'utf8');
const nativeCreatures = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Presentation/NativeWorldCreatures.cs'), 'utf8');
const nativeWorld = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeWorld.cs'), 'utf8');
assert.ok(nativeEnemyView.includes('DrawEnemy') && nativeEnemyView.includes('DrawBossSignal') && nativeEnemyView.includes('DrawStatus'), 'active enemy view must retain body, boss and status presentation');
assert.ok(nativeEnemyView.includes('e.HitFlash/.15') && nativeCreatures.includes('NativeWorldShapes.Tessellate'), 'active enemy view must preserve hit timing and authored vector geometry');
assert.ok(nativeWorld.includes('enemy.SourceVisualVariant'), 'enemy variant must come from the gameplay entity');
for (const retired of ['EnemyCreatureVisual2D.cs', 'BossPresentationVfx2D.cs']) assert.ok(!existsSync(join(root, 'Assets/EchoSphere/Scripts/Runtime', retired)), 'retired presentation must not return: ' + retired);
const sphereAttackRuntimeVfx = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
assert.ok(runtimeArt.includes('CreateEntityRoot(definition.DisplayName + " Sphere", position, 1.05f)') &&
  runtimeArt.includes('NormalEnemyVisualVariants = { "wisp", "leech", "serpent", "stalker", "prism" }') &&
  runtimeArt.includes('FastEnemyVisualVariants = { "moth", "skitter", "stalker" }') &&
  runtimeArt.includes('_rng.NextInt(variants.Length)'),
  'Sphere world size and per-archetype source enemy silhouette variety regressed');

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
const regionNativeArt=readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldRegion.cs'),'utf8');
assert.ok(regionNativeArt.includes('i<6') && regionNativeArt.includes('i<8') && regionNativeArt.includes('_pockets.Circle') && regionNativeArt.includes('_pockets.Triangle'), 'active region fold seams and markers required');
assert.ok(!regionVisuals.includes('AddComponent<LineRenderer>') && !regionVisuals.includes('AddComponent<SpriteRenderer>'), 'region gameplay must not allocate hidden renderer objects');
assert.ok(!/\b_rng\b|UnityEngine\.Random|Random\.Range/.test(regionVisuals), 'presentation-only region visuals must not consume gameplay RNG');
assert.ok(readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldPlayer.cs'),'utf8').includes('NativeCharacterContours'), 'active player must use the approved contour character renderer');
const sourceSphereRenderer = readFileSync(join(root, '..', 'src/renderer.ts'), 'utf8');
const activeStandardAssembly = sourceSphereRenderer.slice(
  sourceSphereRenderer.indexOf('function drawStandardSphereAssembly('),
  sourceSphereRenderer.indexOf('function drawStandardAttackVfx('),
);
assert.ok(activeStandardAssembly.includes('drawStandardSphereCore(ctx, coreRadius, energyPulse)') &&
  activeStandardAssembly.includes('drawStandardSphereRing(ctx, ringRadius, ringRotation') &&
  (activeStandardAssembly.match(/'sphere-standard-upper-crystal'/g) || []).length === 2 &&
  /false,\s*true,/.test(activeStandardAssembly) &&
  !activeStandardAssembly.includes("'sphere-standard-core'") &&
  !activeStandardAssembly.includes("'sphere-standard-ring'") &&
  !activeStandardAssembly.includes("'sphere-standard-lower-crystal'"),
  'active source Standard Sphere assembly is core/ring procedural and draws the upper crystal twice, mirrored');
const retainedStandard = readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldStandardSphere.cs'),'utf8');
const retainedFamilies = readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldSphereFamilyViews.cs'),'utf8')+readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldSphereElements.cs'),'utf8');
const retainedPlayer = readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldPlayer.cs'),'utf8');
assert.equal((retainedStandard.match(/resolve\("sphere-standard-upper-crystal"\)/g)||[]).length,2,'native Standard must draw the same upper crystal twice');
assert.ok(retainedStandard.includes('true,false,true,root') && retainedStandard.includes('Mathf.Max(.9f,r*.045f)') && retainedStandard.includes('Mathf.Max(.55f,r*.022f)'), 'native Standard must retain mirrored lower crystal and ring thickness');
assert.ok(retainedFamilies.includes('orbital') && retainedStandard.includes('Math.Sin(time'), 'active Sphere animation and Orbital view required');

assert.ok(!runtimeArt.includes('LoadAuthored("sphere-standard")') && !runtimeArt.includes('LoadAuthored("sphere-orbital")'), 'the flat prototype SVG substitutions must not be used as source art');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-standard.svg')), 'obsolete flattened Standard Sphere SVG must be removed');
assert.ok(!existsSync(join(root, 'Assets/Resources/EchoSphere/Art/sphere-orbital.svg')), 'obsolete static Orbital mock SVG must be removed');

assert.ok(bootstrap.includes('ApplyModifiedPropertiesWithoutUndo'), 'URP serialized renderer repairs must be applied');

const runtime = readRuntime();
assert.ok(runtime.includes('DrawNativeSphereFrame') && runtime.includes('public bool ResonanceEventActive =>'), 'Sphere VFX must be connected to live gameplay presentation');
assert.ok(runtime.includes('GetEnemyVisualVariant(archetype)') && runtime.includes('GetEnemyVisualVariant(EnemyArchetype archetype)'), 'enemy spawn must wire source-mapped silhouette variants');
const spriteFactory = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/RuntimeSpriteFactory.cs'), 'utf8');
assert.ok(runtime.includes('DrawSphereRoster'), 'active Sphere roster HUD is missing');
assert.ok(runtime.includes('DrawSpriteIcon'), 'Sphere choice UI must render visual icons');
assert.ok(retainedFamilies.includes('sniper') && retainedFamilies.includes('orbital'), 'active Sphere archetypes must use distinct views');
assert.ok(runtime.includes('SetUserPaused'), 'pause/resume state handling is missing');
const networkRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereNetworkRules.cs'), 'utf8');
assert.ok(networkRules.includes('Analyze') && networkRules.includes('FindFractal') && networkRules.includes('GetFormationBonusMultiplier'), 'Network/Geometry analysis contract is missing');
assert.ok(runtime.includes('RefreshNetworkState') && runtime.includes('DrawNativeNetwork') && runtime.includes('GEOMETRY '), 'live Network links and Geometry HUD are missing');
const activeNetworkArt=readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldNetwork.cs'),'utf8');
assert.ok(activeNetworkArt.includes('active?.34:.27') && activeNetworkArt.includes('active?.075:.06') &&
  activeNetworkArt.includes('active?5.8:3.6') && activeNetworkArt.includes('packet/3.0'),
  'active native network must retain source packet speed, trails and widths');

assert.ok(runtime.includes('DrawSourcePausePlanner()') && runtime.includes('BuildPauseStats') && runtime.includes('BuildPauseAbilities') && runtime.includes('BuildPauseArtifacts') && runtime.includes('BuildPauseSynergies') && runtime.includes('SaveCurrentRun(); SceneManager.LoadScene'), 'source four-tab pause planner, resume and save/return controls are missing');
const pausePlannerUi = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSpherePausePlanner.cs'), 'utf8').replaceAll('\\r\\n', '\\n');
assert.ok(pausePlannerUi.includes('DrawPauseTabIcon') && pausePlannerUi.includes('var statGridCount = _pausePlannerTab == 0') && pausePlannerUi.includes('PauseCard(cards, "HP"') && pausePlannerUi.includes('PauseCard(cards, IsRussianUi ? "Уклонение" : "Dodge"'), 'pause planner must preserve source line-art tabs and compact character stat grid');
assert.ok(runtime.includes('DrawSourceGameplayHud') && runtime.includes('DrawProgressBar') && runtime.includes('_xp / _xpRequired') && runtime.includes('_uiScale'), 'source HP/XP/Resonance HUD bars and responsive layout are missing');
const readMovement = runtime.slice(runtime.indexOf('private Vector2 ReadMovement()'), runtime.indexOf('private void ConfigureCamera()'));
assert.ok(runtime.includes('Screen.width * 0.5f') && runtime.includes('HandleSourcePointer(touch.fingerId') && runtime.includes('drag.magnitude >= 12f') && readMovement.includes('_sourceJoystickDirection'), 'source handedness, independent pointer IDs and quantized joystick movement are missing');
assert.ok(runtime.includes('GetAuthoredSphereArtwork(type)') && runtime.includes('return authored'), 'Sphere icon UI must prefer authored source art');
assert.ok(spriteFactory.includes('CreatePolygonSprite'), 'procedural faceted silhouette generation is missing');
assert.ok(spriteFactory.includes('Resources.Load<Sprite>') && runtime.includes('GetAuthoredSphereArtwork'), 'authored SVG runtime binding is missing');
const svgImportSettings = readFileSync(join(root, 'Assets/Editor/EchoSphereSvgImportSettings.cs'), 'utf8');
assert.ok(svgImportSettings.includes('SVGType.TexturedSprite') && svgImportSettings.includes('void OnPreprocessAsset()'), 'SVG artwork must be configured as textured Sprites before import');
assert.ok(!svgImportSettings.includes('SaveAndReimport') && !svgImportSettings.includes('OnPostprocessAllAssets'), 'SVG importer must not recursively reimport assets from a postprocess callback');
assert.ok(retainedPlayer.includes('_contours.Draw(surface,p.Character') && retainedPlayer.includes('DrawPortrait') &&
  !retainedPlayer.includes('NativeCharacterArtwork') && !retainedPlayer.includes('sprites.Draw') && !retainedPlayer.includes('INativeWorldSprite'),
  'live character bodies must share native contour geometry with UI portrait baking, without a second bitmap body path');
const characterContours=readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeCharacterContours.cs'),'utf8');
for(const id of ['spherist','hunter','engineer','berserker','alchemist','architect','conductor','oracle','voidwalker','fractal']) {
  assert.ok(characterContours.includes('case "'+id+'"'), 'missing approved contour identity: '+id);
  assert.ok(existsSync(join(root,'Assets/Resources/EchoSphere/CharacterArt',id+'.png')), 'missing matching menu portrait: '+id);
}
assert.ok(!/\b_rng\b|UnityEngine\.Random|Random\.Range/.test(characterContours), 'character redesign must not consume gameplay RNG');
assert.ok(retainedStandard.includes('CorePath=EllipseContour') && retainedStandard.includes('ArcPath=EllipseContour') && retainedFamilies.includes('orbital'), 'active Standard core/rings and Orbital views required');
assert.ok(sourceSphereRenderer.includes('ctx.lineWidth = Math.max(0.9, r * 0.045)') &&
  sourceSphereRenderer.includes('ctx.lineWidth = Math.max(0.55, r * 0.022)') &&
  sourceSphereRenderer.includes('ctx.ellipse(0, 0, r * 0.88, r * 0.27, 0, Math.PI * 0.10, Math.PI * 1.92)') &&
  spriteFactory.includes('outerHalfWidth = 0.0142f') && spriteFactory.includes('innerHalfWidth = 0.0069f'),
  'the Standard Sphere ring strokes must preserve source radii, thickness and partial-arc gap');
assert.ok(retainedStandard.includes('time*(.22+tier*.018)') && retainedStandard.includes('.72+targetPulse*.20') &&
  retainedStandard.includes('Math.Sin(time*2.2+x*.013)*.028') && retainedStandard.includes('Math.Sin(time*4.6+y*.009)') &&
  retainedStandard.includes('Math.Sin(time*11)'), 'active Standard must retain source tier rotation, breathing and target pulse');
assert.ok(spriteFactory.includes('ES_Diamond') && spriteFactory.includes('ES_Prism') && spriteFactory.includes('ES_Shard'), 'distinct archetype silhouettes are missing');
const resonanceRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ResonanceRules.cs'), 'utf8');
assert.ok(resonanceRules.includes('BaseCap = 100f') && resonanceRules.includes('OverflowCap = 150f'), 'Resonance charge contract is missing');
assert.ok(runtime.includes('TriggerResonanceEvent') && runtime.includes('TickResonanceRing') && runtime.includes('Mathf.FloorToInt(_resonanceCharge)'), 'formation-aware Resonance event runtime/HUD is missing');
const enemySpawnRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/EnemySpawnRules.cs'), 'utf8');
assert.ok(enemySpawnRules.includes('SelectBase(int wave, float roll)') &&
  enemySpawnRules.includes('ShouldBeElite(int wave, float roll)') &&
  enemySpawnRules.includes('18f, 5.5f, 68f, 1.8f, 7f, 0.30f, 19f') &&
  enemySpawnRules.includes('11f, 3.4f, 125f, 2.4f, 8f, 0.32f, 15f') &&
  enemySpawnRules.includes('42f, 10.5f, 44f, 0.8f, 11f, 0.38f, 26f'),
  'source base archetype formulas and separate elite conversion are missing');
assert.ok(runtime.includes('GetWaveSpawnInterval') && runtime.includes('GetEnemiesPerWave') && runtime.includes('StartNextWave'), 'source wave count and spawn cadence controls are missing');
assert.ok(runtime.includes('_waveEnemiesToSpawn > 0') && runtime.includes('CountLivingNonBossEnemies() == 0') && runtime.includes('HasActiveBoss()'), 'source wave progression must wait for the current wave and active boss to finish');
assert.ok(runtime.includes('profile.XpReward * pocket.Xp * challengeXp') &&
  runtime.includes('profile.HpBase + _wave * profile.HpPerWave') &&
  runtime.includes('profile.SpeedBase + _wave * profile.SpeedPerWave') &&
  runtime.includes('profile.DamageBase + _wave * profile.DamagePerWave') &&
  runtime.includes('EnemySpawnRules.ShouldBeElite(_wave, _rng.NextFloat())') &&
  runtime.includes('hp *= 3f') && runtime.includes('damage *= 1.5f') &&
  runtime.includes('xpReward *= 5f') &&
  runtime.includes('ComboRules.GetExperienceOrbValue(xpReward, _comboMultiplier)') &&
  runtime.includes('Initialize(this, _player, comboExperience, orbVelocity, orbRotation, Mathf.Max(1,Mathf.RoundToInt(xpReward)))'),
  'source enemy spawn stats, elite conversion or Combo-adjusted XP pickup wiring is missing');
assert.ok(runtime.includes('GetFormationKey') && runtime.includes('AddResonanceChargeFromSource'), 'new-geometry Resonance charging is missing');

const sphereAttack = ['SphereAttackAgent.cs', 'SphereSourceHitRules.cs', 'SphereSourceAreaRules.cs']
  .map(name => readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime', name), 'utf8').replaceAll('\r\n', '\n')).join('\n');
const spherePlacementRulesPath = 'Assets/EchoSphere/Scripts/Core/SpherePlacementRules.cs';
assert.ok(existsSync(join(root, spherePlacementRulesPath)), 'source-mapped Sphere placement safety rules are missing');
const spherePlacementRules = readFileSync(join(root, spherePlacementRulesPath), 'utf8');
for (const required of ['src/spaceCollision.ts','PlayerBuildGap','TowerPlacementGap','EnemyPlacementGap','HasRouteToOutside','GetMaxSpheres','MaxSameSphereCopies',
  'SphereCharacterId','GetSignatureSphere','GetCharacterSphereCopyCap','GetSameTypeCap','GetCopyDamageMultiplier','SphereCharacterId.Fractal'])
  assert.ok(spherePlacementRules.includes(required), 'source Sphere placement/copy-cap contract missing: ' + required);
assert.ok(runtime.includes('GetCurrentMaxSpheres') &&
  runtime.includes('SpherePlacementRules.GetMaxSpheres(GetAbilityLevel(RunUpgradeKind.MaxSpheres), PersistentUpgradeRules.GetExtraSpheres(GetShopUpgradeLevel("spheres")))') &&
  !runtime.includes('LevelUpAbilityRules.CreateChoice(RunUpgradeKind.MaxSpheres, _maxSphereAbilityLevel)'),
  'routine Level-Up excludes maxspheres, while source capacity still accounts for any saved passive ability level and the shop');
assert.ok(runtime.includes('SpherePlacementRules.GetSameTypeCap') &&
  runtime.includes('_sphereCharacterId') && runtime.includes('_characterMasteryLevel') &&
  !runtime.includes('CountSpheresOfType(_selectedSphereType) >= SpherePlacementRules.MaxSameSphereCopies'),
  'signature Sphere copy limits must follow source character/mastery rules rather than a fixed two-copy runtime cap');
assert.ok(runtime.includes('GetSphereCopyOutputMultiplier') &&
  sphereAttack.includes('_runtime.GetSourceSphereAttackDamage(this)') && runtime.includes('modifiers.damage * GetSphereCopyOutputMultiplier(sphere)') &&
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
  runtime.includes('const float buttonHeight = 58f') && runtime.includes('circleRadius = Mathf.Min(rect.width, rect.height) * 0.44f') &&
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
assert.ok(sphereEnemy.includes('ApplyElementalStatus') && sphereEnemy.includes('_fireTimer') &&
  sphereEnemy.includes('_poisonTimer') && sphereEnemy.includes('_freezeTimer') &&
  sphereEnemy.includes('TryAdvanceElementalConduction') && sphereEnemy.includes('TryBeginFieldElementalReaction'),
  'elemental statuses and per-target Chain/Field cooldowns must run on live enemies');
assert.ok(runtime.includes('GetSphereStatusDurationMultiplier') && runtime.includes('GetSphereStatusDamageMultiplier') &&
  sphereAttack.includes('OnSphereHitApplied') && sphereProjectile.includes('_evolutionOwner.OnSphereHitApplied(enemy, _evolutionOwner.Type, _sourceElement)'),
  'element status rules must connect through Sphere attacks and projectile impacts');
const liveSphereFamilies=readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeSphereFamilies.cs'), 'utf8');
assert.ok(liveSphereFamilies.includes('DrawNativeSphereElements') && liveSphereFamilies.includes('GetElement(element)') &&
  !liveSphereFamilies.includes('HasPrismElement'),
  'Prism retains element accents/effects separately; user-approved base core/orbits must not be recolored by mutation');
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


const evolutionRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereEvolutionCombatRules.cs'), 'utf8');
assert.ok(sphereAttack.includes('VisualRotation') && sphereAttack.includes('targetDelta.y'), 'Sphere emitter presentation must track target direction without altering combat');
const projectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
const enemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(enemy.includes('SourceProjectionHitFlash') && enemy.includes('_sourceHitFlash = .15f') && enemy.includes('IsMarked'), 'enemy must expose live mark/hit state to its retained view');
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
assert.ok(!stellaChest.includes('new Mesh') && !stellaChest.includes('TextMesh') && readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeEntityLayers.cs'), 'utf8').includes('_nativeWorldPickableViews.DrawChest'), 'Stella must use the active retained presentation without a hidden mesh');
assert.ok(runtimeArt.includes('SpawnStellaChest(position)') && runtimeArt.includes('CollectStellaChest(StellaChestAgent2D chest)'), 'boss death must produce a collectible Stella cache');
assert.ok(runtimeArt.includes('ArtifactRewardRules.IsLegendaryStellaWindow') && runtimeArt.includes('PickChoices(_ownedArtifacts, _rng.NextFloat, true') && runtimeArt.includes('PickChoices(_ownedArtifacts, _rng.NextFloat, false'), 'Stella rarity-window selection/fallback is missing');
assert.ok(runtimeArt.includes('GetStellaChoiceCount(_ownedArtifacts)') && artifactRules.includes('Contains(owned,"quantum_fold")'), 'Quantum Fold must add a fourth Stella choice');
assert.ok(runtimeArt.includes('ChooseStellaReward(i)') && runtimeArt.includes('_stellaRewardPending'), 'Stella rewards must wait for explicit player selection');
assert.ok(readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8').includes('HealCoreFromDamage(actual)') && runtimeArt.includes('GetArtifactSphereFamilyDamageMultiplier') && runtimeArt.includes('SetDashCooldownMultiplier'), 'artifact combat/stat hooks are not connected');
const playerCore = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/PlayerCoreController2D.cs'), 'utf8');
assert.ok(playerCore.includes('CanReceiveDamageNow') && playerCore.includes('IncreaseMaxHp') && playerCore.includes('SetDashCooldownMultiplier'), 'player artifact stat hooks are missing');
const enemyRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
assert.ok(!enemyRuntime.includes('HealCoreFromDamage(appliedDamage)'), 'vampirism must not heal from arbitrary enemy/status damage');
assert.ok(runtime.includes('DrawSourceMainHome();') && runtime.includes('"ВЫБРАТЬ ЗАБЕГ" : "SELECT RUN"'),
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
  runtime.includes('difficulty.EnemyDamageMultiplier * GetRegionEnemyDamageMultiplier()') &&
  !runtime.includes('GetArtifactDamageTakenMultiplier() * CurrentDifficulty.EnemyDamageMultiplier'),
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
const regionClearWindow = runtimeArt.slice(runtimeArt.indexOf('ResolveSourceSpaceCollisions(dt);'), runtimeArt.indexOf('finally { _simulationFrame = false; }'));
assert.ok(regionClearWindow.includes('if (_regionStabilized)') && regionClearWindow.includes('_gameOver = true;') &&
  regionClearWindow.includes('_paused = true;') && regionClearWindow.includes('FinalizeRunResult();'),
  'source Region clear must end the run and persist its final result instead of continuing gameplay');
assert.ok(runtimeArt.includes('CountLivingNonBossEnemies') && runtimeArt.includes('HasActiveBoss'), 'next source wave must wait for living enemies and boss completion');
const regionPoiRuntime = readRuntime();
const enemyPoiRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const regionResonanceRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/RegionBossScheduleRules.cs'), 'utf8');
assert.ok(regionResonanceRules.includes('GetResonanceMultiplier') &&
  regionPoiRuntime.includes('RegionBossScheduleRules.GetResonanceMultiplier('),
  'source region resonance surge and Volatile Core multiplier must affect live resonance gains');
assert.ok(regionPoiRuntime.includes('TickRegionMastery(dt)') &&
  regionPoiRuntime.includes('GetMasteryChallengesCompleted') &&
  regionPoiRuntime.includes('GetRegionGoldReward(_selectedRegion, _runEliteKills, _bossesDefeated)') &&
  regionPoiRuntime.includes('_regionDamageTaken += _regionLastHp - hp'),
  'source region mastery counters and completion reward metadata must be tracked');
assert.ok(regionPoiRuntime.includes('case "resonance_cache":') && regionPoiRuntime.includes('_resonanceCharge + 28f') &&
  regionPoiRuntime.includes('_resonanceCharge + 45f') && regionPoiRuntime.includes('ScaleLivingEnemiesNearPlayer(260f * radiusScale, 0.72f)') &&
  regionPoiRuntime.includes('ScaleLivingEnemiesNearPlayer(220f * radiusScale, 0.62f)') &&
  regionPoiRuntime.includes('_regionXpReward += 12') && regionPoiRuntime.includes('case "elite_nest":') &&
  enemyPoiRuntime.includes('ScaleCurrentHp(float multiplier)') &&
  !regionPoiRuntime.includes('AddExperience(RegionBossScheduleRules.GetRegionXpReward(_level))'),
  'region POI gameplay effects must match source and region XP metadata must not be converted into player XP');
const levelUpAbilities = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/LevelUpAbilityRules.cs'), 'utf8');
assert.ok(levelUpAbilities.includes('SourcePassiveCatalog') &&
  levelUpAbilities.includes('kind == RunUpgradeKind.MaxSpheres ? 3 : 7;') &&
  levelUpAbilities.includes('RunUpgradeKind.MaxSpheres, RunUpgradeKind.MoveSpeed') &&
  levelUpAbilities.includes('GetNetworkDisableDuration'),
  'complete source passive catalog metadata and source level caps are missing');
assert.ok(levelUpAbilities.includes('GetSourceActiveAbilityName') &&
  levelUpAbilities.includes('GetSourceActiveAbilityDescription') &&
  runtimeArt.includes('LevelUpAbilityRules.CreateChoice(kind, current, IsRussianUi)') &&
  !levelUpAbilities.includes('ECHO PULSE / '),
  'routine active Ability cards must preserve source names, localized level descriptions and mutation-stage text');
for (const token of ['0.93d', '0.15f', '0.10f', '0.16f', 'SourcePixelsToWorld', 'GetDamageTakenMultiplier'])
  assert.ok(levelUpAbilities.includes(token), 'source Level-Up Ability formula is missing: ' + token);
assert.ok(!levelUpAbilities.includes('GetFortuneDropBonus') &&
  levelUpAbilities.includes('rare drop chance') &&
  !runtime.includes('GetFortuneDropBonus(GetAbilityLevel(RunUpgradeKind.Fortune))'),
  'Fortune may retain its source catalog description but must not add an unsupported cache-drop formula');
assert.ok(!runtime.includes('LevelUpAbilityRules.RoutineAbilityKinds') && runtime.includes('RecordLevelUpChoice(choice)') &&
  runtime.includes('GetSphereAffinityWeight(choice.Sphere)') &&
  runtime.includes('foreach (var kind in new[] { RunUpgradeKind.Teleport, RunUpgradeKind.TimeStop, RunUpgradeKind.Blast'),
  'routine Level-Up must use source Sphere and active-Ability pools, not the constructed-but-unused passive pool');
assert.ok(runtime.includes('IncreaseMaxHp(6f)') && runtime.includes('IncreaseMaxHp(20f)') &&
  runtime.includes('GetSphereDamageMultiplier(GetAbilityLevel(RunUpgradeKind.Damage))') &&
  !runtime.includes('GetFortuneDropBonus(GetAbilityLevel(RunUpgradeKind.Fortune))') &&
  !runtime.includes('REPAIR PROTOCOL') && !runtime.includes('+18% damage for all Spheres'),
  'Level-Up must retain source per-level HP behavior, exclude unselected passive choices, and not add Fortune to the cache-drop formula');
const experienceOrb = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ExperienceOrbAgent.cs'), 'utf8');
assert.ok(experienceOrb.includes('GetExperienceMagnetRadiusWorld()'), 'Magnet Level-Up must affect XP orb attraction');
const xpOrbVisualRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs'), 'utf8');
const xpNativeArt = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Presentation/NativeWorldPickups.cs'), 'utf8');
const xpNativeLayer = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeEntityLayers.cs'), 'utf8');
assert.ok(xpNativeLayer.includes('6,NativeExperienceColor,orb.SourceProjectionRotation') &&
  xpNativeLayer.includes('NativeMutationColor("#63e6ff")') && xpNativeArt.includes('A(230,253,255,.82f)') &&
  xpNativeArt.includes('A(255,255,255,.58f)'), 'active XP view must retain fixed size, cyan body, white outline and highlight');
assert.ok(xpNativeArt.includes('r*.62f,-r*.35f') && xpNativeArt.includes('r*.52f,r*.68f') &&
  xpNativeArt.includes('-r*.52f,r*.68f') && xpNativeArt.includes('-r*.62f,-r*.35f'),
  'active XP shard must retain the six-point source geometry');
const enemyAbilitySlowCheck = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const enemySlowCombatRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/CombatRules.cs'), 'utf8');
assert.ok(enemyAbilitySlowCheck.includes('GetEnemyAbilitySpeedMultiplier(distanceToPlayer, _slowMultiplier)') &&
  enemySlowCombatRules.includes('GetEnemySpeedMultiplier') &&
  enemyAbilitySlowCheck.includes('distanceToPlayer'),
  'Slow Level-Up must use the source radius and min(status slow, ability slow) for live enemy movement');
const balanceRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/RunBalanceRules.cs'), 'utf8');
assert.ok(balanceRules.includes('GetEnemiesPerWave') && balanceRules.includes('GetWaveSpawnInterval'), 'source wave count and spawn interval rules missing');
const bossProjectile = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/BossProjectileAgent2D.cs'), 'utf8');
assert.ok(bossProjectile.includes('renderer.ts:drawBossProjectile') && bossProjectile.includes('_velocity * dt'), 'source boss projectile motion/VFX missing');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(bossProjectile), 'boss projectile presentation must not consume gameplay RNG');
const sphereAttackRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/SphereAttackAgent.cs'), 'utf8');
assert.ok(sphereAttackRuntime.includes('NetworkDisabled') && sphereAttackRuntime.includes('ApplyNetworkDisable') && sphereAttackRuntime.includes('DelayNextAttack'), 'boss-to-Sphere disruption contract missing');
assert.ok(runtime.includes('GetSourceSphereModifiers(source).resonantCharge / 2f') && sphereAttack.includes('ShouldOrbitalGrantShieldCharge'), 'Orbital Halo must use source modifier charge and grant a shield on each qualifying contact');
assert.ok(evolutionRules.includes('ShouldTriggerStandardResonatorPulse'), 'Standard Resonator combat rules are missing');
assert.ok(evolutionRules.includes('GetSniperAssassinDamageMultiplier'), 'Sniper Assassin combat rules are missing');
assert.ok(evolutionRules.includes('GetSniperBeaconRadius'), 'Sniper Beacon combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunHailChance'), 'Shotgun Hail combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunCataclysmSplash'), 'Shotgun Cataclysm combat rules are missing');
assert.ok(evolutionRules.includes('GetShotgunBurstDamageMultiplier'), 'Shotgun Burst close-range rules are missing');
assert.ok(evolutionRules.includes('GetShotgunCataclysmPierce'), 'Shotgun Cataclysm pierce rule is missing');
assert.ok(evolutionRules.includes('GetShotgunHailBonusPellets'), 'Shotgun Hail multishot rule is missing');
assert.ok(sphereAttack.includes('mode == 2 && distance < 90f') && sphereAttack.includes('enemy.ExtendSlowStatus(.4f)'), 'Shotgun Burst final III must extend the existing slow timer at source close range');
assert.ok(evolutionRules.includes('GetChainStormRadius') && evolutionRules.includes('GetChainLeechHealRatio'), 'Chain evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetAuraSanctumSlowDuration') && evolutionRules.includes('GetAuraOvergrowthAttackTimerReduction'), 'Aura evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetGravityWellPullDistance') && evolutionRules.includes('GetGravityTideDistance'), 'Gravity evolution combat rules are missing');
assert.ok(evolutionRules.includes('GetOrbitalInnerCount') && evolutionRules.includes('GetOrbitalAngularSpeed'), 'Orbital ring progression rules are missing');
assert.ok(evolutionRules.includes('GetPrismBeamCount') && evolutionRules.includes('GetPrismRangeWorld') &&
  evolutionRules.includes('GetPrismBranchPower') && evolutionRules.includes('GetPrismBeamDamageMultiplier') &&
  evolutionRules.includes('GetPrismReflectionCount') && evolutionRules.includes('GetPrismMirrorRicochetCount'),
  'Prism direct-beam, source-range and Network-refraction rules are missing');
assert.ok(evolutionRules.includes('GetPulseWaveKnockbackDistance') && evolutionRules.includes('GetVoidExecutionChance'), 'Pulse/Void evolution combat rules are missing');
assert.ok(sphereAttack.includes('ApplySourceBranchHit') && sphereAttack.includes('standard_swarm') && enemy.includes('owner.ApplySourceBranchHit'), 'common primary hit path must dispatch Standard evolution effects');
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
assert.ok(sphereAttack.includes('pulse_wave') && sphereAttack.includes('pulse_burst') && runtime.includes('ResolveSourceVoidHit') && runtime.includes('void_execution'), 'Pulse and shared primary/secondary Void hit effects must be wired');
assert.ok(runtime.includes('SpawnVoidShards'), 'Void Reaper shard spawning is missing');
assert.ok(runtime.includes('AddResonanceCharge') && runtime.includes('TriggerResonanceEvent') && runtime.includes('TickResonanceRing'), 'Resonance runtime event loop is missing');
assert.ok(sphereAttack.includes('HasNetworkCandidateForSphere(SphereNetworkFormation.Triangle') &&
  sphereAttack.includes('ResonanceRules.NetworkCharge') && sphereAttack.includes('ChargeResonanceFromSource'),
  'Prism Spectrum triangle Network charge is missing');
assert.ok(sphereAttack.includes('.20f * cluster') && sphereAttack.includes('branch == "chain_storm"'), 'Gravity Cluster and Chain Storm routing are missing');
assert.ok(sphereAttack.includes('HasNetworkCandidateForSphere(SphereNetworkFormation.Cluster, index)') && sphereAttack.includes('ChargeResonanceFromSource(ResonanceRules.GeometryCharge'), 'Pulse Burst Geometry charge is missing');

assert.ok(enemy.includes('RegisterSphereHit(owner, this, actual)') && enemy.includes('ResonanceRules.SphereHitCharge'), 'all Sphere hits, including recursive hits, must charge Resonance in the common damage receiver');
assert.ok(enemy.includes('ApplyDamageOverTime'), 'Timed status damage support is missing');
assert.ok(enemy.includes('out wasCritical') && enemy.includes('ref wasCritical'), 'common damage path must pass critical state through Predator and branch mechanics');
assert.ok(projectile.includes('enemy.ReceiveSphereDamage(_evolutionOwner.Type, _damage, false, _allowAuthoredProcs, _evolutionOwner)'), 'projectiles must use the common damage receiver with their real owner and proc flag');
assert.ok(runtime.includes('TriggerStandardResonatorPulse') && runtime.includes('SpawnStandardSwarmShards'), 'Standard evolution runtime effects are missing');
for (const branch of ['standard_resonator','standard_singularity','standard_swarm','sniper_oracle','sniper_assassin','sniper_beacon','shotgun_burst','shotgun_cataclysm','shotgun_hail','orbital_dance','orbital_halo','orbital_blade','prism_split','prism_spectrum','prism_mirror','void_hunger','void_reaper','void_execution','chain_web','chain_storm','chain_leech','aura_sanctum','aura_gravity','aura_overgrowth','gravity_well','gravity_tide','gravity_collapse','pulse_wave','pulse_resonator','pulse_burst']) assert.ok(sphereElementalRules.includes('"' + branch + '"'), 'elemental source branch mapping missing: ' + branch);

const impactVisualPath = 'Assets/EchoSphere/Scripts/Runtime/ImpactVfx2D.cs';
assert.ok(existsSync(join(root, impactVisualPath)), 'source-mapped hit impact component is missing');
const impactVisual = readFileSync(join(root, impactVisualPath), 'utf8');
assert.ok(impactVisual.includes('src/renderer.ts') && impactVisual.includes('Impact Ring and Shards') && impactVisual.includes('MarkDynamic'), 'hit impact must retain the source particle/ring mapping');
assert.ok(!/UnityEngine\.Random|Random\.Range|_rng/.test(impactVisual), 'impact presentation must not consume gameplay RNG');
assert.ok(impactVisual.startsWith('#if UNITY_EDITOR && ECHO_SPHERE_SCENE_REFERENCE') && nativeEnemyView.includes('e.HitFlash/.15') && enemyRuntime.includes('EmitSourceHitFeedback'), 'legacy scene preview must be isolated while active hit flash/particles remain connected');
const sourcePresentation=readFileSync(join(root,'Assets/EchoSphere/Scripts/Runtime/EchoSphereSourcePresentation.cs'),'utf8');
assert.ok(sourcePresentation.includes('AddSourceLightningVisual')&&readFileSync(join(root,'Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeFrame.cs'),'utf8').includes('foreach(var lightning in _sourceLightningVisuals)'), 'chain-lightning events must enter the native component frame');
assert.ok(runtime.includes('SpawnCombatArc(Vector2 from, Vector2 to') && runtime.includes('AddSourceLightningVisual(from,to,duration,family)'), 'combat arc events must feed the original Canvas drawing path');
assert.ok(sphereAttack.includes('_runtime.SpawnCombatArc(i == 0 ? origin : (Vector2)primary.transform.position') && sphereAttack.includes('150f * SpherePlacementRules.SourcePixelsToWorld'), 'Chain must select sequential nearby jumps and connect them to the source-mapped arcs');
assert.ok(runtime.includes('SpawnCombatArc(center, targets[i].transform.position, SphereId.Chain') && runtime.includes('SpawnCombatArc(primary.transform.position, target.transform.position, SphereId.Chain'), 'Chain Storm secondary and extra strikes need visual arcs');
assert.ok(sphereAttack.includes('src/engineSpheres.ts:updatePrismSphere') &&
  sphereAttack.includes('1 + (int)_sourceModifiers.multishot') && sphereAttack.includes('highestHp') &&
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
assert.ok(runtime.includes('SourceSphereModifiersRules.Get') && runtime.includes('SourceSphereGlobalModifiers.Merge') && sphereAttack.includes('_initialDamage * _sourceModifiers.damage') &&
  runtime.includes('ResolveAuthoredSphereHitDamage') && enemy.includes('ReceiveSphereDamage(SphereAttackAgent owner'),
  'source Overload/Shatter/Mark stats must be transferred once before shared conditional hit modifiers');

assert.ok(enemy.includes('ResolveAuthoredSphereHitDamage') && enemy.includes('ApplyAuthoredSphereHitEffects') &&
  enemy.includes('ReceiveSphereDamage(SphereAttackAgent owner'),
  'source-authored hit modifiers are not wired through the common enemy damage receiver');
assert.ok(runtime.includes('SphereAuthoredModifierRules.GetExecuteDamageMultiplier') &&
  runtime.includes('SphereAuthoredModifierRules.GetCorruptDamageMultiplier') &&
  runtime.includes('SphereAuthoredModifierRules.GetImpactDistancePixels') &&
  runtime.includes('SphereAuthoredModifierRules.GetAnchorSlowMultiplier') &&
  runtime.includes('damage * GetSourceSphereModifiers(source).healOnHit') &&
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
assert.ok(runtime.includes('GetSourceSphereModifiers(source).staticChance') && runtime.includes('GetSourceSphereModifiers(source).echoChance') &&
  runtime.includes('SphereAuthoredModifier.Static'),
  'source Echo/Static hit procs are not connected');
assert.ok(sphereAttack.includes('SpawnAuthoredMainProjectile') &&
  sphereAttack.includes('_sourceModifiers.multishot') &&
  sphereAttack.includes('(int)_sourceModifiers.pierce') &&
  sphereAttack.includes('1 + (int)_sourceModifiers.multishot'),
  'Sphere projectile emission and Prism direct-beam emission must retain their source modifier paths');
assert.ok(evolutionRules.includes('GetProjectilePierceBonus("void_execution", finalId)') &&
  evolutionRules.includes('GetPrismBeamCount') &&
  evolutionRules.includes('GetPrismMirrorRicochetCount') &&
  !evolutionRules.includes('branch == "prism_mirror" && finalId == "prism_mirror_final_2" ? 1'),
  'Void projectile pierce and Prism beam/ricochet rules must use source-authored modifier data');


// Source menu/persistence migration contracts. These guard actual runtime call paths,
// not just the presence of similarly named screen types.
const menuRuntime = readRuntime();
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
  menuRuntime.includes('CharacterRules.GetSphereAffinityWeight(_sphereCharacterId, type)') &&
  menuRuntime.includes('GetArchitectFormationCritBonus()') &&
  menuRuntime.includes('0.12f * GetArchitectFormationBonusScale(SphereNetworkFormation.Square)') &&
  menuRuntime.includes('_runTime - _architectFormationChangedAt <= 2f'),
  'selected character base combat modifiers and Architect formation bonuses must be connected to actual Unity runtime call paths');
assert.ok(menuRuntime.includes('TickCharacterMastery();') &&
  menuRuntime.includes('Mathf.Clamp(_runTime - _characterMasteryLastTime, 0f, 0.1f)') &&
  menuRuntime.includes('CharacterRules.GetUpdatedMasteryXp(characterProfile.masteryXp, masteryRunXp)') &&
  menuRuntime.includes('SaveCharacterProfiles();'),
  'source character mastery must accumulate during the run and persist once at run completion');
for (const token of [
  'if (activeSphereCount >= 4) _characterMasteryRunXp += dt * 0.85f',
  'eliteDelta * 20f + bossDelta * 45f',
  'largestNetwork >= 3',
  'hpRatio <= 0.6f',
  'statusTargets * dt * 0.07f',
  'formation.Strength >= 0.9f',
  'resonanceDelta * 28f',
  'sphere.NetworkDisabled',
]) {
  assert.ok(menuRuntime.includes(token), `source mastery XP rule missing from runtime: ${token}`);
}
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

// Hunter Mark/Hunt combat rules are pure contracts and must also be connected to live hit/death paths.
const hunterRules = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/HunterCombatRules.cs'), 'utf8');
const hunterEnemyRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const hunterProjectileRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/ProjectileAgent.cs'), 'utf8');
assert.ok(hunterEnemyRuntime.includes('RegisterCharacterSphereHit(source, this)') &&
  hunterEnemyRuntime.includes('GetHunterMarkedTargetDamageMultiplier(source, this)') &&
  menuRuntime.includes('GetHunterCriticalChanceBonus(target)'),
  'Hunter Mark and damage/critical multipliers must run through live enemy damage');
assert.ok(hunterEnemyRuntime.includes('_archetype = archetype') && hunterEnemyRuntime.includes('public bool IsTank => _archetype == EnemyArchetype.Tank') &&
  menuRuntime.includes('visualVariant, "", isElite, archetype') &&
  menuRuntime.includes('SelectEliteVariant(out eliteVariant)'),
  'Hunter target eligibility must use the actual selected enemy archetype and elite variant visuals');
assert.ok(hunterProjectileRuntime.includes('enemy.ReceiveSphereDamage(_evolutionOwner.Type, _damage') &&
  hunterEnemyRuntime.indexOf('RegisterCharacterSphereHit(source, this)') < hunterEnemyRuntime.indexOf('out wasCritical, this'),
  'common Sphere hit receiver must apply Hunter marks before resolving the marked-target critical roll');
for (const token of [
    'sphere == SphereId.Sniper', 'sphere == SphereId.Chain', 'sphere == SphereId.Prism', 'sphere == SphereId.Void',
  'archetype == EnemyArchetype.Tank', 'masteryLevel >= 2 ? 6f : 5f',
  'masteryLevel >= 4 ? 4 : 5', 'masteryLevel >= 6 ? 1.25f : 1.20f',
  'masteryLevel >= 8 ? 1.35f : 1.30f', 'masteryLevel >= 3 ? 0.02f : 0f',
  'masteryLevel >= 5 ? 0.10f : 0f',
]) {
  assert.ok(hunterRules.includes(token), `source Hunter rule missing: ${token}`);
}
assert.ok(menuRuntime.includes('if (enemy.IsElite && _characterMasteryLevel >= 5') &&
  menuRuntime.includes('_hunterTrophyTimer = 4f') && menuRuntime.includes('_hunterMarkTarget == enemy'),
  'Hunter mastery V Trophy requires killing the active marked Elite and lasts four seconds');
const hunterParticleRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Presentation/NativeWorldFeedback.cs'), 'utf8');
for (const token of [
  'new Color32(184, 71, 90, 255)', 'i < 10', '0.35f, 2.5f',
  'new Color32(196, 69, 61, 255)', 'i < 18', '0.5f, 3f',
  'new Vector2(0f, -25f)', 'Mathf.Min(0.16f, _hunterScreenShakeIntensity + 0.04f)',
  'Mathf.Max(0f, _hunterScreenShakeIntensity - dt)',
  'shakePixelsX * SpherePlacementRules.SourcePixelsToWorld',
]) {
  assert.ok(menuRuntime.includes(token) || hunterParticleRuntime.includes(token),
    `source Hunter visual/shake behavior missing: ${token}`);
}
assert.ok(hunterParticleRuntime.includes('P(s*1.8,0)') && hunterParticleRuntime.includes('P(0,-s*.55)') && menuRuntime.includes('SpawnSourceParticle(worldPosition'),
  'active Hunter particles must retain elongated diamonds and simulation-owned pixel conversion');

// Engineer Relay source contracts: duration/range, exact consumer Sphere set,
// one-shot 1.25x damage, and a Canvas-scaled drawEnergyBolt presentation.
const engineerRelayRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/EngineerRelayRules.cs'), 'utf8');
const engineerRelayVfxSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Presentation/NativeWorldEnergyBolts.cs'), 'utf8');
assert.ok(engineerRelayRulesSource.includes('masteryLevel >= 6 ? 260 : masteryLevel >= 2 ? 240 : 220') &&
  engineerRelayRulesSource.includes('masteryLevel >= 4 ? 0.55f : 0.40f') &&
  engineerRelayRulesSource.includes('1.25f'),
  'Engineer link range, relay duration and bonus must retain source values');
assert.ok(engineerRelayRulesSource.includes('SphereId.Standard') &&
  engineerRelayRulesSource.includes('SphereId.Sniper') &&
  engineerRelayRulesSource.includes('SphereId.Shotgun') &&
  engineerRelayRulesSource.includes('SphereId.Chain') &&
  engineerRelayRulesSource.includes('SphereId.Void') &&
  engineerRelayRulesSource.includes('SphereId.Orbital') &&
  engineerRelayRulesSource.includes('SphereId.Prism') &&
  engineerRelayRulesSource.includes('SphereId.Gravity') &&
  engineerRelayRulesSource.includes('SphereId.Pulse') &&
  engineerRelayRulesSource.includes('SphereId.Aura'),
  'Engineer relay consumers must preserve the source generic/special Sphere split');
const conductorRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ConductorOverdriveRules.cs'), 'utf8');
assert.ok(conductorRulesSource.includes('masteryLevel >= 2 ? 2.5f : 2f') &&
  conductorRulesSource.includes('masteryLevel >= 8 ? 0.28f : masteryLevel >= 4 ? 0.23f : 0.18f') &&
  conductorRulesSource.includes('masteryLevel >= 6 && resonanceEventCount > 0 && resonanceEventCount % 3 == 0') &&
  conductorRulesSource.includes('masteryLevel >= 9 ? 3 : masteryLevel >= 7 ? 2 : 1'),
  'Conductor Overdrive duration, speed tiers and Cascade thresholds must preserve source values');
assert.ok(characterSphereAttackRuntime.includes('TriggerResonancePulse(float duration)') &&
  characterSphereAttackRuntime.includes('_resonancePulseTimer = Mathf.Max(0f, _resonancePulseTimer - dt)') &&
  menuRuntime.includes('sphere.TriggerResonancePulse(0.42f)') &&
  menuRuntime.includes('_spheres[nodeIndex].TriggerResonancePulse(0.38f)'),
  'source Resonance pulse timers must be stored and connected to Conductor Cluster, Triangle and Sphere formation events');
assert.ok(menuRuntime.includes('ApplyConductorResonanceEffects(type, center, baseDamage)') &&
  menuRuntime.includes('ConductorOverdriveRules.GetOverdriveDurationSeconds(_characterMasteryLevel)') &&
  menuRuntime.includes('ConductorOverdriveRules.GetAttackSpeedBonus(_characterMasteryLevel)') &&
  menuRuntime.includes('_conductorOverdriveTimer = Mathf.Max(0f, _conductorOverdriveTimer - dt)') &&
  menuRuntime.includes('if (_conductorOverdriveTimer > 0f) _characterMasteryRunXp += dt * 1.1f'),
  'Conductor Overdrive must activate on real Resonance events, tick down, affect attack speed and award source mastery XP');
assert.ok(menuRuntime.includes('ConductorOverdriveRules.ShouldTriggerCascade(_characterMasteryLevel, _resonanceEventsTriggered)') &&
  menuRuntime.includes('ConductorOverdriveRules.GetTriangleDischargeDamageMultiplier(_characterMasteryLevel)') &&
  menuRuntime.includes('ConductorOverdriveRules.GetClusterAttackAdvanceSeconds(_characterMasteryLevel)') &&
  menuRuntime.includes('ConductorOverdriveRules.GetSquareShieldChargeBonus(_characterMasteryLevel)'),
  'Conductor events must apply source geometry supercharge and every-third-event Cascade');

assert.ok(menuRuntime.includes('TickEngineerRelay(dt)') &&
  menuRuntime.includes('TriggerEngineerRelay(SphereAttackAgent source)') &&
  menuRuntime.includes('SpawnEngineerRelayLightning(sourcePosition, neighbour.transform.position)') &&
  menuRuntime.includes('ConsumeEngineerRelayBonus(SphereAttackAgent consumer)') &&
  menuRuntime.includes('if (_engineerRelayTimer > 0f) _characterMasteryRunXp += dt * 0.75f;'),
  'Engineer relay state must tick, activate from a source Sphere, link neighbours and award source mastery XP');
assert.ok(sphereAttack.includes('EngineerRelayRules.ConsumesOnAttack(_type)') &&
  sphereAttack.includes('_runtime.ConsumeEngineerRelayBonus(this)') &&
  sphereAttack.includes('_runtime.TriggerEngineerRelay(this)') &&
  sphereAttack.includes('private bool DamageSourceArea') &&
  sphereAttack.includes('if (hitSomething) _runtime.TriggerEngineerRelay(this)'),
  'Engineer relay must be applied once to eligible attacks and trigger from actual field/orbital activity');
assert.ok(menuRuntime.includes('SpawnEngineerRelayLightning(from, to, 0.28f)') &&
  engineerRelayVfxSource.includes('Math.Min(26,len*.08)') && engineerRelayVfxSource.includes('.28*alpha') &&
  engineerRelayVfxSource.includes('.9*alpha'), 'active Engineer bolt must retain source wobble and alpha layers');

// CI-only workflow guards. The Unity Editor/APK cloud build is intentionally
// disabled; this workflow must keep validating contracts and project structure
// without requiring Unity license secrets or invoking GameCI.
const ciBuildPath = 'Assets/Editor/EchoSphereCiBuild.cs';
const ciBuild = readFileSync(join(root, ciBuildPath), 'utf8');
assert.ok(ciBuild.includes('BuildAndroidDevelopmentApk'), 'Unity Android build entry point should remain available for a future explicitly approved build');
assert.ok(ciBuild.includes('EchoSphereProjectBootstrap.EnsureNativeScene()'), 'future Android build must register the authored native entry');
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


// Source-mapped Level-Up Lock/Reroll and Oracle Forecast behavior.
const levelUpChoiceRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/LevelUpChoiceRules.cs'), 'utf8');
assert.ok(levelUpChoiceRulesSource.includes('masteryLevel >= 4 ? 2 : 1') &&
  levelUpChoiceRulesSource.includes('masteryLevel >= 9 ? 2 : 1') &&
  levelUpChoiceRulesSource.includes('masteryLevel >= 6 && !rerollUsed && !hasLockedChoice && hasForecastedChoices') &&
  levelUpChoiceRulesSource.includes('masteryLevel >= 7 && hasChoices'),
  'Oracle Forecast mastery thresholds must preserve source rules');
assert.ok(menuRuntime.includes('_levelUpRerollsRemaining = 1') &&
  menuRuntime.includes('_levelUpLocksRemaining = 1') &&
  menuRuntime.includes('private bool LockLevelUpChoice(int choiceIndex)') &&
  menuRuntime.includes('private bool RerollLevelUpChoices()') &&
  menuRuntime.includes('BuildLevelUpChoices()') &&
  menuRuntime.includes('_levelUpChoices.Add(pool[lockedChoiceIndex])'),
  'Level-Up must initialise one Lock and one Reroll and carry reserved choices into the offer');
assert.ok(menuRuntime.includes('if (_levelUpLockChoiceKey == selectedChoiceKey) _levelUpLockChoiceKey = null') &&
  menuRuntime.includes('_oracleForecastKeys.Contains(choiceKey)') &&
  menuRuntime.includes('RefreshOracleForecast(abilitySourceWeight, sphereSourceWeight)') &&
  menuRuntime.includes('GetLevelUpChoiceBaseWeight'),
  'Lock reservation must survive other picks; Oracle forecast markers use the live logical card keys');
assert.ok(menuRuntime.includes('GetOracleRerollPreservationCount(_characterMasteryLevel)') &&
  menuRuntime.includes('oracleForecastChoices.Count > 0 && lockedChoiceIndex < 0') &&
  menuRuntime.includes('_oracleForecastRerollUsed = true') &&
  menuRuntime.includes('rerollLabel, _button)') && menuRuntime.includes('RerollLevelUpChoices(); _upgradePickerScroll = Vector2.zero'),
  'Oracle preserves forecasted choices on its first eligible unlocked reroll; UI exposes functional Lock/Reroll controls');


// Triangle formation Canvas overlay: source radius, pulse expansion, 3 ticks,
// active/inactive alpha, low-alpha triangle fill and center core pulse.
assert.ok(menuRuntime.includes('DrawTriangleResonanceOverlay();') &&
  menuRuntime.includes('private void DrawTriangleResonanceOverlay()') &&
  menuRuntime.includes('_networkState.FormationCandidates') &&
  menuRuntime.includes('25f + Mathf.Sin(time * 5f + sphereIndex) * 1.5f') &&
  menuRuntime.includes('radius + 5f + (1f - flash) * 10f') &&
  menuRuntime.includes('pulseTimer / 0.45f') &&
  menuRuntime.includes('private void DrawSphereGuiTriangleFill') &&
  menuRuntime.includes('private void DrawSphereGuiFilledCircle'),
  'Triangle Resonance rings, pulse expansion, triangular fill and pulsing core must be connected to active Unity runtime');


// Rune of Resonance and Echo Architecture are the two source writers for
// formationHitCount. Check their runtime paths and the real charge-ring renderer.
const formationHitRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/FormationHitRules.cs'), 'utf8');
assert.ok(formationHitRulesSource.includes('ResonanceRuneHitIncrement = 2') &&
  formationHitRulesSource.includes('ResonanceRunePulseSeconds = 0.5f') &&
  formationHitRulesSource.includes('EchoArchitectureRelayHitIncrement = 2') &&
  formationHitRulesSource.includes('EchoArchitectureRelayPulseSeconds = 0.35f') &&
  formationHitRulesSource.includes('EchoArchitectureRelayLightningSeconds = 0.20f') &&
  formationHitRulesSource.includes('formationHitCount > 0 && formationHitCount % 5 == 0'),
  'Formation charge writer constants and Echo Architecture counter condition must match source');
assert.ok(menuRuntime.includes('ActivateResonanceRune()') &&
  menuRuntime.includes('AddFormationHitCount(FormationHitRules.ResonanceRuneHitIncrement)') &&
  menuRuntime.includes('ApplyEchoArchitectureRelayOnSphereHit(owner)') &&
  menuRuntime.includes('FormationHitRules.HasCompletedEchoArchitecture(_ownedArtifacts)') &&
  menuRuntime.includes('relay.AddFormationHitCount(FormationHitRules.EchoArchitectureRelayHitIncrement)') &&
  menuRuntime.includes('FormationHitRules.EchoArchitectureRelayLightningSeconds'),
  'Rune and Echo Architecture must write actual live Sphere counters/pulses and use the real linked-node relay path');
assert.ok(sphereAttack.includes('public int FormationHitCount => _formationHitCount') &&
  sphereAttack.includes('public void AddFormationHitCount(int amount)') &&
  menuRuntime.includes('var charge = sphere.FormationHitCount % 3') &&
  menuRuntime.includes('DrawSphereGuiArc(point, radius, startAngle, endAngle'),
  'Triangle charge arcs and ticks must render from the actual Sphere counter');


// Echo Architecture source pair synergies have distinct active combat effects:
// Echo Relay +8% on mixed networks, completed Set +6% within 220 px, and
// Unified Core +5% per strongest-Sphere level only on lower-level Spheres.
assert.ok(formationHitRulesSource.includes('GetEchoRelayDamageMultiplier(bool hasSynergy, int uniqueTypeCount)') &&
  formationHitRulesSource.includes('hasSynergy && uniqueTypeCount >= 2 ? 1.08f : 1f') &&
  formationHitRulesSource.includes('setComplete && uniqueTypeCount >= 2 && hasNearbySphere ? 1.06f : 1f') &&
  formationHitRulesSource.includes('1f + strongestSphereLevel * 0.05f'),
  'Echo Architecture pair-synergy combat multipliers must preserve source percentages and gates');
assert.ok(menuRuntime.includes('GetArtifactSphereSynergyDamageMultiplier(sphere)') &&
  menuRuntime.includes('HasNearbyActiveSphere(sphere, 220f)') &&
  menuRuntime.includes('SpherePlacementRules.SourcePixelsToWorld') &&
  menuRuntime.includes('SphereProgressionRules.GetLevel(_sphereLevels, sphere.Type)') &&
  menuRuntime.includes('FormationHitRules.HasEchoRelaySynergy(_ownedArtifacts)') &&
  menuRuntime.includes('FormationHitRules.HasUnifiedCoreSynergy(_ownedArtifacts)'),
  'Echo Relay, completed Echo Architecture proximity and Unified Core must affect actual live attack damage');


// Source-mapped live Artifact Synergy pair effects: Fortress Network, Glass
// Cannon, Singularity and Core Relay. Base artifact effects stay separate.
const artifactSynergyRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ArtifactSynergyRules.cs'), 'utf8');
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "heavy_core", "resonance_core")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "overclock", "void_contract")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "lone_bastion", "singularity_engine")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "unified_mind", "echo_thread")'),
  'Artifact Synergy activation must require exact source pairs');
assert.ok(artifactSynergyRulesSource.includes('hasSynergy && hasNearbySphere ? 1.10f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy ? 1.12f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy ? 1.05f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy && uniqueTypeCount <= 1 ? 1.20f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy && uniqueTypeCount >= 2 ? 1.08f : 1f'),
  'Artifact Synergy multipliers and gates must match source values');
assert.ok(menuRuntime.includes('ArtifactSynergyRules.GetFortressNetworkDamageMultiplier') &&
  menuRuntime.includes('HasNearbyActiveSphere(sphere, 190f)') &&
  menuRuntime.includes('ArtifactSynergyRules.GetGlassCannonDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetSingularityDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetCoreRelayDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetGlassCannonDamageTakenMultiplier'),
  'Artifact Synergy pair effects must be connected to real Sphere damage and incoming-player-damage runtime paths');


// Source-mapped target-state Artifact Synergies: Void Horizon checks for any
// living Elite; Orbital Prism buffs Orbital only; Gravity Pulse counts living
// enemies with any living Sphere strictly within 140 source pixels.
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "void_ink", "void_mark")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "orbital_blade", "prism_filter")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "gravity_hook", "pulse_driver")') &&
  artifactSynergyRulesSource.includes('hasSynergy && isVoidSphere && hasLivingElite ? 1.15f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy && isOrbitalSphere ? 1.15f : 1f') &&
  artifactSynergyRulesSource.includes('hasSynergy && isPulseSphere && groupedLivingEnemyCount >= 3 ? 1.15f : 1f'),
  'Target-state Artifact Synergies must match source pair IDs, Sphere categories and thresholds');
assert.ok(menuRuntime.includes('HasLivingEliteEnemy()') &&
  menuRuntime.includes('CountLivingEnemiesNearAnySphere(140f)') &&
  menuRuntime.includes('delta.sqrMagnitude < radiusSq') &&
  menuRuntime.includes('ArtifactSynergyRules.GetVoidHorizonDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetOrbitalPrismDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetGravityPulseDamageMultiplier'),
  'Void Horizon, Orbital Prism and Gravity Pulse must use live combat entities in the actual damage path');


// Perfect Network source geometry is formsTriangle(): a qualifying active
// triplet around the attacker with every side <= 300 logical pixels. Do not
// substitute the stricter resonance-network TriangleStrength threshold.
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "fivefold_resonance", "triangle_circuit")') &&
  artifactSynergyRulesSource.includes('hasSynergy && uniqueTypeCount >= 3 && sphereFormsTriangle ? 1.10f : 1f') &&
  artifactSynergyRulesSource.includes('abDistancePixels <= 300f && asDistancePixels <= 300f && bsDistancePixels <= 300f'),
  'Perfect Network must preserve the source pair, three-type threshold and 300 px maximum side');
assert.ok(menuRuntime.includes('SphereFormsTriangleWithinSourceRange(sphere)') &&
  menuRuntime.includes('ArtifactSynergyRules.IsTriangleWithinSourceRange(abDistance, asDistance, bsDistance)') &&
  menuRuntime.includes('var sourceDistanceScale = SpherePlacementRules.SourcePixelsToWorld'),
  'Perfect Network must use the source formsTriangle distance test in the live damage multiplier rather than the unrelated Unity Resonance Triangle score');


// Source pair synergies with live target/build gates.
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "foresight_eye", "sniper_scope")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "pulse_driver", "pulse_lens")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "echo_weaver", "folded_core")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "sniper_crown", "long_lens")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "pulse_crown", "tempo_ring")'),
  'Hunter Doctrine, Pulse Engineering, Core Forge, Hunter Focus and Pulse Overload require the source artifact pairs');
assert.ok(menuRuntime.includes('HasLivingEliteOrBossEnemy()') &&
  menuRuntime.includes('CountLivingEnemiesInRun()') &&
  menuRuntime.includes('ArtifactSynergyRules.GetHunterDoctrineDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetPulseEngineeringDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetCoreForgeDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetHunterFocusDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetPulseOverloadDamageMultiplier'),
  'Five conditional pair synergies must be wired into live Sphere damage using enemy/build state');


// Axiom Fold: source artifact pair and at least three distinct active Sphere types.
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "axiom_core", "universal_fold")') &&
  artifactSynergyRulesSource.includes('hasSynergy && uniqueTypeCount >= 3 ? 1.06f : 1f') &&
  menuRuntime.includes('ArtifactSynergyRules.GetAxiomFoldDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.HasAxiomFoldSynergy(_ownedArtifacts)'),
  'Axiom Fold must apply +6% only when its exact pair and three-type condition are met');


// Geometry Loop and formation-memory synergies read the source-equivalent
// resonance geometry key/candidate set, not the separate pointer-drag preview.
const resonanceGeometryMemoryRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ResonanceGeometryMemoryRules.cs'), 'utf8');
assert.ok(menuRuntime.includes('TrackResonanceGeometryState()') &&
  menuRuntime.includes('ResonanceGeometryMemoryRules.ShouldRefreshGeometry') &&
  menuRuntime.includes('_resonanceGeometryCandidateKeys') &&
  menuRuntime.includes('_resonanceGeometryMemoryTimer = ResonanceGeometryMemoryRules.MemoryDurationSeconds') &&
  menuRuntime.includes('DrawResonanceGeometryMemoryOverlay()') &&
  menuRuntime.includes('ArtifactSynergyRules.GetGeometryLoopDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetLatticeMemoryDamageMultiplier') &&
  menuRuntime.includes('ArtifactSynergyRules.GetNetworkMemoryDamageMultiplier'),
  'Resonance geometry key/candidate changes must capture their own 0.9 s memory and drive the three actual combat bonuses');
assert.ok(artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "triangle_engine", "ring_engine")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "lattice_engine", "fractal_seed")') &&
  artifactSynergyRulesSource.includes('HasBoth(ownedArtifacts, "network_anchor", "echo_weaver")') &&
  resonanceGeometryMemoryRulesSource.includes('MemoryDurationSeconds = 0.9f'),
  'Geometry Loop/Lattice Memory/Network Memory must require source artifact pairs and source memory duration');


// Completed Artifact Set bonuses (independent from their pair-synergy bonuses).
const artifactSetRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ArtifactSetRules.cs'), 'utf8');
assert.ok(menuRuntime.includes('ArtifactSetRules.HasCompletedResonanceGrid') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedSingularityPath') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedGeometryCraft') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedVoidHorizonSet') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedHunterDoctrineSet') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedPulseEngineeringSet') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedCoreForgeSet') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedPrismDominionSet') &&
  menuRuntime.includes('ArtifactSetRules.HasCompletedNetworkLegacySet'),
  'Nine simple completed Artifact Sets must be wired into live damage calculations');
assert.ok(artifactSetRulesSource.includes('fortressNetwork && perfectNetwork') &&
  artifactSetRulesSource.includes('glassCannon && singularity') &&
  artifactSetRulesSource.includes('geometryLoop && latticeMemory && networkMemory') &&
  artifactSetRulesSource.includes('voidHorizon && orbitalPrism && gravityPulse') &&
  artifactSetRulesSource.includes('hunterDoctrine && hunterFocus') &&
  artifactSetRulesSource.includes('pulseEngineering && pulseOverload') &&
  artifactSetRulesSource.includes('coreForge && coreRelay') &&
  artifactSetRulesSource.includes('orbitalPrism && statusCircuit') &&
  artifactSetRulesSource.includes('networkMemory && perfectNetwork'),
  'Completed Artifact Sets must require every source-listed pair synergy');
assert.ok(artifactSetRulesSource.includes('complete && hasNearbySphere && hasActiveGeometry ? 1.06f : 1f') &&
  artifactSetRulesSource.includes('complete && uniqueTypeCount <= 1 ? 1.08f : 1f') &&
  artifactSetRulesSource.includes('complete && isVoidSphere ? 1.10f : 1f') &&
  artifactSetRulesSource.includes('complete && isPrismOrOrbital ? 1.08f : 1f'),
  'Completed Artifact Set multipliers must retain their numeric gates');


// Artifact Protocol combat bonuses are active runtime multipliers, not only UI state.
const artifactProtocolRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ArtifactProtocolRules.cs'), 'utf8');
assert.ok(menuRuntime.includes('ArtifactProtocolRules.GetTriangleResonanceMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetSphereRelayMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetCriticalEchoMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetGeometryCircuitMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetVoidHorizonProtocolMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetHunterDoctrineProtocolMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetPulseEngineeringProtocolMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetCoreForgeProtocolMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetPrismDominionProtocolMultiplier') &&
  menuRuntime.includes('ArtifactProtocolRules.GetNetworkLegacyProtocolMultiplier'),
  'Nine completed-set Artifact Protocols must feed the real Sphere damage multiplier');
assert.ok(artifactProtocolRulesSource.includes('completedSingularityPath && combo >= 5') &&
  artifactProtocolRulesSource.includes('IsCriticalEchoActive(completedSingularityPath, combo) ? 1.06f : 1f') &&
  artifactProtocolRulesSource.includes('completedGrid && sphereFormsTriangle ? 1.05f : 1f') &&
  artifactProtocolRulesSource.includes('completedEchoArchitecture && uniqueTypeCount >= 2 && hasNearbySphere ? 1.05f : 1f') &&
  artifactProtocolRulesSource.includes('completedVoidHorizon && isVoidSphere && hasLivingEliteOrBoss ? 1.08f : 1f') &&
  artifactProtocolRulesSource.includes('completedHunterDoctrine && isSniperSphere && hasLivingEliteOrBoss ? 1.08f : 1f') &&
  artifactProtocolRulesSource.includes('completedPulseEngineering && isPulseSphere && livingEnemyCount >= 4 ? 1.08f : 1f') &&
  artifactProtocolRulesSource.includes('completedPrismDominion && isPrismOrbitalOrVoid && refractionPressure ? 1.06f : 1f'),
  'Artifact Protocol bonuses must preserve source values and active-state gates');


// Artifact Protocol status is exposed both in the run and in the persistent encyclopedia.
assert.equal((artifactProtocolRulesSource.match(/new ArtifactProtocolDefinition\(/g) || []).length, 12,
  'the protocol catalog must contain all twelve source definitions');
assert.ok(runtimeArt.includes('MainMenuPage.Knowledge: DrawKnowledgeBaseScreen(); return;') &&
  runtimeArt.includes('MainMenuPage.Knowledge') && runtimeArt.includes('DrawSourceKnowledgeBaseScreen') &&
  runtimeArt.includes('private void DrawKnowledgeBaseScreen()'),
  'the main menu must open the Artifact Protocol encyclopedia');
assert.ok(runtimeArt.includes('private void DrawArtifactProtocolOverlay()') &&
  runtimeArt.includes('IsRussianUi ? "АРТЕФАКТЫ / ПРОТОКОЛЫ" : "ARTIFACTS / PROTOCOLS"') &&
  runtimeArt.includes('IsArtifactProtocolCurrentlyActive(protocol)'),
  'the run must show protocol discovery and live active state');
assert.ok(runtimeArt.includes('RecordDiscoveredArtifactProtocols();') &&
  runtimeArt.includes('ArtifactProtocolKnowledgePreferenceKey') &&
  runtimeArt.includes('PlayerPrefs.SetString(ArtifactProtocolKnowledgePreferenceKey'),
  'completed Artifact Sets must persist protocol encyclopedia unlocks');
const comboRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/ComboRules.cs'), 'utf8');
assert.ok(runtimeArt.includes('ComboRules.IncrementOnKill') &&
  runtimeArt.includes('ComboRules.TickTimer') &&
  runtimeArt.includes('ComboRules.GetExperienceMultiplier') &&
  runtimeArt.includes('ComboRules.GetExperienceOrbValue') &&
  runtimeArt.includes('ResetCombo();') &&
  runtimeArt.includes('UnlockAchievementIf(_combo >= 50, "combo_50")'),
  'source Combo timer, kill, XP, damage-reset and achievement paths must be wired');
for (const threshold of ['>= 10', '>= 25', '>= 50', '>= 100'])
  assert.ok(comboRulesSource.includes(threshold), `source Combo XP multiplier threshold missing: ${threshold}`);
assert.ok(runtimeArt.includes('private GUIStyle _menuTitle, _comboTitleStyle, _comboMultiplierStyle;') &&
  runtimeArt.includes('_comboTitleStyle = new GUIStyle(GUI.skin.label)') &&
  runtimeArt.includes('_comboMultiplierStyle = new GUIStyle(GUI.skin.label)') &&
  runtimeArt.includes('if (_combo >= 5)') &&
  runtimeArt.includes('var comboTitle = (IsRussianUi ? "КОМБО" : "COMBO") + " x" + _combo;') &&
  runtimeArt.includes('_combo >= 50') && runtimeArt.includes('_combo >= 25') &&
  runtimeArt.includes('_comboMultiplier.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)') &&
  runtimeArt.includes('Mathf.Clamp01(_comboTimer / ComboRules.WindowSeconds)') &&
  runtimeArt.includes('const float comboTop = 64f;') &&
  runtimeArt.includes('(_uiWidth - comboWidth) * 0.5f') &&
  runtimeArt.includes('new Color(1f, 0.72156864f, 0.3019608f, 1f)') &&
  runtimeArt.includes('new Color(1f, 0.41960785f, 0.41960785f, 1f)') &&
  runtimeArt.includes('new Color(1f, 0.3019608f, 0.3647059f, 1f)'),
  'combo HUD must bind live runtime combo/multiplier/timer, preserve source labels and colors, and stay center-aligned in the portrait layout');
assert.ok(runtimeArt.includes('critical_echo') &&
  runtimeArt.includes('ArtifactProtocolRules.IsCriticalEchoActive(discovered, _combo)') &&
  runtimeArt.includes('temporal_fold_protocol') &&
  runtimeArt.includes('status_circuit_protocol') &&
  runtimeArt.includes('protocol.HasUnmappedRuntimeState') &&
  runtimeArt.includes('conditionSatisfied = _timeStopTimer > 0f || _teleportDamageBuffTimer > 0f') &&
  runtimeArt.includes('conditionSatisfied = HasActiveSphereType(SphereId.Prism) && HasAnyGlobalSphereModifier()'),
  'protocol activation must use source-mapped combo, time-control and Prism status-build state while retaining the generic unmapped-state guard');
assert.ok(artifactProtocolRulesSource.includes('HasUnmappedRuntimeState') &&
  artifactProtocolRulesSource.includes('"critical_echo"') &&
  artifactProtocolRulesSource.includes('GetCriticalEchoMultiplier') &&
  artifactProtocolRulesSource.includes('"temporal_fold_protocol"') &&
  artifactProtocolRulesSource.includes('"status_circuit_protocol"') &&
  artifactProtocolRulesSource.includes('GetTemporalFoldProtocolMultiplier') &&
  artifactProtocolRulesSource.includes('GetStatusCircuitProtocolMultiplier'),
  'the protocol catalog must expose source-mapped Temporal Fold and Status Circuit multipliers');


// Mobile orientation is part of the ECHO SPHERE baseline: fixed upright portrait.
assert.ok(bootstrap.includes('PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;'),
  'Unity Android must default to upright portrait orientation');
assert.ok(bootstrap.includes('PlayerSettings.allowedAutorotateToPortrait = true;') &&
  bootstrap.includes('PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;'),
  'Unity Android must allow upright portrait and reject upside-down portrait');
assert.ok(bootstrap.includes('PlayerSettings.allowedAutorotateToLandscapeLeft = false;') &&
  bootstrap.includes('PlayerSettings.allowedAutorotateToLandscapeRight = false;') &&
  !bootstrap.includes('UIOrientation.LandscapeLeft'),
  'Unity Android must not force or auto-rotate to landscape');


// Temporal Fold / Status Circuit prerequisite state: source timer names and global sphereMods schema.
assert.ok(runtimeArt.includes('private float _timeStopTimer, _teleportDamageBuffTimer;') &&
  runtimeArt.includes('_timeStopTimer = Mathf.Max(0f, _timeStopTimer - dt);') &&
  runtimeArt.includes('_teleportDamageBuffTimer = Mathf.Max(0f, _teleportDamageBuffTimer - dt);') &&
  runtimeArt.includes('SetTimeStopTimer(float duration)') &&
  runtimeArt.includes('SetTeleportDamageBuffTimer(float duration)'),
  'Temporal Fold must use source-equivalent timers that decay and are only set by mapped ability handlers');
for (const modifier of [
  'multishot', 'pierce', 'ricochet', 'fire', 'freeze', 'poison', 'breach',
  'overload', 'split', 'shatter', 'execute', 'mark', 'echo', 'anchor', 'phase',
  'static', 'resonant', 'magnetic', 'vampiric', 'corrupt', 'drain',
  'afterimage', 'impact', 'gravitic', 'rotationSpeed'
]) assert.ok(runtimeArt.includes('{ "' + modifier + '",'),
  'source player.sphereMods field must exist: ' + modifier);
assert.ok(runtimeArt.includes('{ "rotationSpeed", 1 }') &&
  runtimeArt.includes('GetGlobalSphereModifierLevel') &&
  runtimeArt.includes('SetGlobalSphereModifierLevel') &&
  runtimeArt.includes('HasAnyGlobalSphereModifier'),
  'global Sphere modifiers must preserve the source initial rotationSpeed and expose typed state access');
assert.ok(artifactProtocolRulesSource.includes('GetTemporalFoldProtocolMultiplier') &&
  artifactProtocolRulesSource.includes('GetStatusCircuitProtocolMultiplier') &&
  runtimeArt.includes('ArtifactSetRules.GetTemporalFoldDamageMultiplier') &&
  runtimeArt.includes('ArtifactSetRules.GetStatusCircuitSetDamageMultiplier') &&
  runtimeArt.includes('ArtifactSynergyRules.GetTemporalEchoDamageMultiplier') &&
  runtimeArt.includes('ArtifactSynergyRules.GetStatusCircuitDamageMultiplier') &&
  runtimeArt.includes('ArtifactSynergyRules.GetStatusLoopDamageMultiplier'),
  'Temporal Fold and Status Circuit set, synergy and protocol bonuses must be wired to the source time-control and sphereMods predicates');


// Time Stop / Teleport activation parity: level-gated slots, manual source-key activation,
// cooldowns, timer production and target-range behavior. Ability branches/finals remain gated.
const activeAbilityEnemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const sphereProgressionRulesSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Core/SphereProgressionRules.cs'), 'utf8');
assert.ok(sphereProgressionRulesSource.includes('Teleport, TimeStop') &&
  levelUpAbilities.includes('case RunUpgradeKind.Teleport:') &&
  levelUpAbilities.includes('case RunUpgradeKind.TimeStop:') &&
  runtimeArt.includes('private void ActivateTimeStop()') &&
  runtimeArt.includes('private void ActivateTeleport()') &&
  runtimeArt.includes('ActivateActiveAbility(binding.Key)'),
  'Time Stop and Teleport must be actual selectable, manually activated run abilities');
assert.ok(runtimeArt.includes('_level >= 20 ? 3 : _level >= 12 ? 2 : _level >= 5 ? 1 : 0') &&
  runtimeArt.includes('SourceActiveAbilityKeys = { KeyCode.E, KeyCode.Q, KeyCode.R, KeyCode.F, KeyCode.G }') &&
  runtimeArt.includes('AssignActiveAbilityHotkey(choice.Kind)'),
  'active ability slots and hotkey assignment must follow source progression');
// All eight source active abilities must be selectable, manually activatable and level/evolution wired.
for (const ability of ['Blast', 'Shield', 'Teleport', 'FireTrail', 'Minion', 'Lightning', 'TimeStop', 'DarkRitual']) {
  assert.ok(sphereProgressionRulesSource.includes(ability) &&
    levelUpAbilities.includes('case RunUpgradeKind.' + ability + ':') &&
    runtimeArt.includes('RunUpgradeKind.' + ability) &&
    runtimeArt.includes('Activate' + ability + '()'),
    'source active ability ' + ability + ' must have progression and a runtime activation path');
}
assert.ok(runtimeArt.includes('GetSphereRangeMultiplier()') &&
  runtimeArt.includes('case RunUpgradeKind.Range:') && runtimeArt.includes('_spheres[i].ApplyProgressionLevel(_spheres[i].ProgressionLevel)'),
  'Range upgrades refresh live Sphere range');
assert.ok(runtimeArt.includes('IsCharacterPreferredActiveAbility') &&
  runtimeArt.includes('var characterAffinity = IsCharacterPreferredActiveAbility(choice.Kind) ? 0.75f : 0f;') &&
  runtimeArt.includes('levelWeight + characterAffinity + 0.08f') &&
  !runtimeArt.includes('_sphereCharacterId == SphereCharacterId.Oracle && choice.Kind == RunUpgradeKind.Crit'),
  'Level-Up active Ability weighting must match source character preferences and active-choice affinity without an Oracle-only rule');
assert.ok(!runtimeArt.includes('if (_maxSphereAbilityLevel > 0) abilityScore += 1f;'),
  'Level-Up source weighting must count actual acquired abilities only, not a separate Max Spheres pseudo-ability');
assert.ok(runtimeArt.includes('GetSphereRadiusMultiplier()') &&
  runtimeArt.includes('SphereBoostDamageBonus') &&
  runtimeArt.includes('GetActiveAbilityCooldownMultiplier') &&
  runtimeArt.includes('_playerCore.DashTimer > 0f'),
  'Radius, Sphere Boost, Chrono Core and Momentum must affect live combat state');
assert.ok(runtimeArt.includes('AddComponent<AbilityDroneAgent2D>()') &&
  runtimeArt.includes('_fireTrailTimer > 0f') &&
  runtimeArt.includes('_darkRitualOverloadTimer > 0f') &&
  levelUpAbilities.includes('firetrail_overdrive') &&
  levelUpAbilities.includes('lightning_echo_storm') &&
  levelUpAbilities.includes('darkritual_blood_link'),
  'Fire Trail and Dark Ritual must modify runtime combat, Minion must spawn active drones, and new active abilities must expose evolution branches');
const abilityDroneSource = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/AbilityDroneAgent2D.cs'), 'utf8');
assert.ok(runtimeArt.includes('GetAbilityFinalArchetype(RunUpgradeKind.TimeStop)') &&
  runtimeArt.includes('GetAbilityFinalArchetype(RunUpgradeKind.Teleport)') &&
  runtimeArt.includes('if (final == "minion_sphere_guard")') &&
  runtimeArt.includes('sphere.AccelerateNextAttack(0.35f)') &&
  abilityDroneSource.includes('_final == "minion_sphere_guard"') &&
  abilityDroneSource.includes('anchor.AccelerateNextAttack(dt * 0.12f)'),
  'canonical final archetypes must activate source-mapped Time Stop, Teleport and Minion final riders');
assert.ok(runtimeArt.includes('var effectiveDamage = damage * synergyMultiplier;') &&
  !runtimeArt.includes('var sourceDamage = damage * synergyMultiplier * GetSphereDamageMultiplier(source.Type);'),
  'active ability pulses must match source damage calls without a Sphere source and must not inherit Sphere-hit modifiers');
assert.ok(runtimeArt.includes('private readonly List<AbilityDroneAgent2D> _abilityDrones') &&
  runtimeArt.includes('GetAbilityLevel(RunUpgradeKind.Minion) >= 3') &&
  runtimeArt.includes('drone.NetworkPosition') &&
  runtimeArt.includes('buffer.Positions') &&
  runtimeArt.includes('TryGetNetworkNodePosition(formation.NodeIndexes[i], out var position)') &&
  runtimeArt.includes('drone.IsNetworkNodeAlive'),
  'source runtime network must append live Minions from Ability level III and preserve drone links/formation centers');
assert.ok(runtimeArt.includes('RefreshNetworkState();') &&
  runtimeArt.includes('var droneIndex = _spheres.Count + _abilityDrones.IndexOf(drone);') &&
  runtimeArt.includes('SphereNetworkRules.GetLinkedNodeIndexes(_networkState, droneIndex)') &&
  runtimeArt.includes('linkedSphere.AccelerateNextAttack(0.22f)') &&
  runtimeArt.includes('linkedSphere.NetworkDisabled'),
  'Minion Echo Swarm must use the refreshed live network graph and accelerate only linked, active, enabled Spheres');
assert.ok(runtimeArt.includes('GetGlobalSphereModifierLevel("fire") <= 0') &&
  !runtimeArt.includes('!IsFireAlignedSphere(sphere.Type) continue'),
  'Fire Trail activation must use the source global player fire modifier, not sphere-authored fire affinity');
assert.ok(runtimeArt.includes('var networkOrdered = new List<SphereAttackAgent>(networkOrder)') &&
  runtimeArt.includes('if (remaining.Contains(sphere)) networkOrdered.Add(sphere)'),
  'Lightning network traversal must append disconnected Spheres in the original distance-sorted order');

assert.ok(runtimeArt.includes('40f * GetActiveAbilityCooldownMultiplier()') &&
  runtimeArt.includes('3f + Mathf.Min(2, level - 1)') &&
  runtimeArt.includes('(15f - Mathf.Min(4, (level - 1) * 2)) * GetActiveAbilityCooldownMultiplier()') &&
  runtimeArt.includes('220f') && runtimeArt.includes('700f') && runtimeArt.includes('300f'),
  'Time Stop / Teleport must preserve source cooldowns, duration scaling and level-III target range');
assert.ok(activeAbilityEnemy.includes('public void ApplyFreeze(float duration)') &&
  activeAbilityEnemy.includes('public void SetFreezeDuration(float duration)') &&
  runtimeArt.includes('enemy.SetFreezeDuration(freezeDuration)') &&
  runtimeArt.includes('EmitSourceSpherePulse(nearest, nearest.transform.position, pulseRadius, pulseDamage') &&
  runtimeArt.includes('for(var i=0;i<14;i++)') &&
  runtimeArt.includes('float angle=_rng.NextFloat()*Mathf.PI*2f;') &&
  runtimeArt.includes('SpawnSourceParticle(center,new Vector2(Mathf.Cos(angle),-Mathf.Sin(angle))*90f,.35f,3f,htmlColor,.35f)') &&
  !runtimeArt.includes('for (var i = 0; i < 28; i++) _rng.NextFloat();') &&
  runtimeArt.includes('enemy.ApplySlow(1.2f, 0.6f)') &&
  runtimeArt.includes('ShowTimedCombatMessage("ECHO FREEZE", 1.2f)'),
  'Time Stop must preserve source freeze semantics, seeded 14-particle pulse behavior, slow rider support and feedback');
assert.ok(runtimeArt.includes('GetActiveAbilityEvolutionChoices') &&
  runtimeArt.includes('_abilityBranches') && runtimeArt.includes('_abilityFinals') &&
  runtimeArt.includes('teleport_echo_jump_f1') && runtimeArt.includes('teleport_beacon_f2') &&
  runtimeArt.includes('teleport_phase_f3') && runtimeArt.includes('timestop_closed_time_f2') &&
  runtimeArt.includes('timestop_time_anchor_f3') && runtimeArt.includes('GrantInvulnerability') &&
  levelUpAbilities.includes('GetActiveAbilityEvolutionChoices') &&
  levelUpAbilities.includes('teleport_echo_jump_f1') && levelUpAbilities.includes('timestop_echo_phase_f1') &&
  artifactProtocolRulesSource.includes('"temporal_fold_protocol"') &&
  artifactProtocolRulesSource.includes('"status_circuit_protocol"'),
  'active ability branch-specific final variants must use source IDs and have real choices/riders; artifact protocols remain independently gated until their full predicates are wired');

const sourceAbilitySynergyLinks = [
  ['std_res_blast', 'blast_resonance_f1'],
  ['std_singularity_timestop', 'timestop_echo_phase_f1'],
  ['std_swarm_minion', 'minion_echo_drone_f1'],
  ['sniper_oracle_teleport', 'teleport_echo_jump_f1'],
  ['sniper_assassin_darkritual', 'darkritual_blood_link_f1'],
  ['sniper_beacon_firetrail', 'firetrail_overdrive_f1'],
  ['shotgun_burst_shield', 'shield_echo_guard_f1'],
  ['shotgun_cataclysm_blast', 'blast_network_f2'],
  ['shotgun_hail_lightning', 'lightning_echo_storm_f1'],
  ['chain_web_timestop', 'timestop_closed_time_f2'],
  ['chain_storm_lightning', 'lightning_relay_f2'],
  ['chain_leech_darkritual', 'darkritual_sacrifice_f2'],
  ['aura_sanctum_shield', 'shield_reflector_f2'],
  ['aura_gravity_teleport', 'teleport_beacon_f2'],
  ['aura_overgrowth_firetrail', 'firetrail_sanctum_f3'],
  ['orbital_dance_minion', 'minion_relay_drone_f2'],
  ['orbital_halo_shield', 'shield_bastion_f3'],
  ['orbital_blade_darkritual', 'darkritual_void_pact_f3'],
  ['prism_split_blast', 'blast_core_f3'],
  ['prism_spectrum_firetrail', 'firetrail_ignition_f2'],
  ['prism_mirror_teleport', 'teleport_phase_f3'],
  ['gravity_well_firetrail', 'firetrail_overdrive_f3'],
  ['gravity_tide_timestop', 'timestop_time_anchor_f3'],
  ['gravity_collapse_blast', 'blast_network_f1'],
  ['pulse_wave_blast', 'blast_core_f2'],
  ['pulse_resonator_lightning', 'lightning_echo_storm_f3'],
  ['pulse_burst_shield', 'shield_echo_guard_f2'],
  ['void_hunger_darkritual', 'darkritual_blood_link_f2'],
  ['void_reaper_minion', 'minion_guardian_f3'],
  ['void_execution_teleport', 'teleport_echo_jump_f2']
];
assert.ok(runtimeArt.includes('ApplyActiveAbilityFinalVariantRiders') &&
  runtimeArt.includes('ActiveAbilitySynergyLinks') &&
  runtimeArt.includes('1f / (1f + i * 0.18f)'),
  'active Sphere-Ability synergies must use source final-variant riders and source synergy strength');
for (const [linkId, finalId] of sourceAbilitySynergyLinks) {
  assert.ok(runtimeArt.includes('"' + linkId + '"'), 'source active synergy link is missing: ' + linkId);
  assert.ok(levelUpAbilities.includes('"' + finalId + '"'), 'source active final variant is missing: ' + finalId);
}



// Link Breaker source parity: authored range/timers, disabled-target filtering and Stability scaling.
const linkBreakerEnemy = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), 'utf8');
const linkBreakerRuntime = readRuntime();
assert.ok(linkBreakerEnemy.includes('260f') && linkBreakerEnemy.includes('0.75f') &&
  linkBreakerEnemy.includes('0.30f') && linkBreakerEnemy.includes('6f') &&
  linkBreakerEnemy.includes('GetNetworkDisableDuration(2f)'),
  'Link Breaker must retain source range, telegraph/cooldown timing and base disable duration');
assert.ok(linkBreakerEnemy.includes('SpherePlacementRules.SourcePixelsToWorld') &&
  linkBreakerEnemy.includes('NETWORK BREAK') && nativeEnemyView.includes('private void LinkBreaker('),
  'Link Breaker must use source distance calibration and render telegraph/impact cues');
assert.ok(linkBreakerRuntime.includes('FindNearestLinkBreakerTarget') &&
  linkBreakerRuntime.includes('!sphere.NetworkDisabled') &&
  linkBreakerRuntime.includes('GetAbilityLevel(RunUpgradeKind.Stability)') &&
  linkBreakerRuntime.includes('ShowTimedCombatMessage'),
  'Link Breaker must skip disabled Spheres and use the source Stability duration modifier');

const sourceHomeUi = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceHome.cs'), 'utf8').replaceAll('\r\n', '\n');
assert.ok(sourceHomeUi.includes('var radii = new[] { .41f, .28f, .19f, .35f, .11f }') &&
  sourceHomeUi.includes('var verticalScales = new[] { .391f, .469f, .375f, .438f, .423f }') &&
  sourceHomeUi.includes('var baseRotations = new[] { 18f, 24f, -34f, -16f, -24f }') &&
  sourceHomeUi.includes('Time.unscaledTime * 360f / periods[ring]') &&
  sourceHomeUi.includes('new Color(163f / 255f, 124f / 255f, 255f / 255f, .20f)'),
  'main-menu core orbit/ring geometry, tilt, color and rotation must match final source CSS');
assert.ok(sourceHomeUi.includes('DrawSourceMenuNavIcon') &&
  ['UserRound', 'Store', 'Trophy', 'Award', 'BookOpen', 'Settings'].every(icon => sourceHomeUi.includes(icon)) &&
  sourceHomeUi.includes('SourceUiNavCircle') && sourceHomeUi.includes('SourceUiNavStroke') &&
  sourceHomeUi.includes('rect.center.x - 10f') && sourceHomeUi.includes('rect.y + 27f') &&
  sourceHomeUi.includes('var narrowPhone = _uiWidth <= 1100f') &&
  sourceHomeUi.includes('var navColumns = narrowPhone ? 3 : 6') &&
  sourceHomeUi.includes('var navButtonHeight = narrowPhone ? 46f : 42f'),
  'main-menu navigation must preserve all six source icon identities as line-art alongside the live destinations');
const knowledgeRuntime = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereKnowledge.cs'), 'utf8').replaceAll('\r\n', '\n');
const sourceCatalogPath = join(root, 'Assets/Editor/EchoSphereReference/SourceCatalog.json');
assert.ok(existsSync(sourceCatalogPath) &&
  ['nameRu', 'nameEn', 'descRu', 'descEn', 'detailsRu', 'detailsEn'].every(name => knowledgeRuntime.includes(name)) &&
  knowledgeRuntime.includes('Resources.Load<EchoSphere.Presentation.NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog")') &&
  ['basics', 'spheres', 'monsters', 'bosses', 'abilities', 'artifacts', 'evolutions'].every(section => knowledgeRuntime.includes(section)),
  'Unity knowledge base must load the native catalog with source-authored localized names/descriptions/details');
for (const name of ['SourceCatalog', 'SourceUiStyles', 'SourceUiIcons']) {
  assert.ok(existsSync(join(root, 'Assets/Editor/EchoSphereReference', name + '.json')) &&
    !existsSync(join(root, 'Assets/Resources/EchoSphere', name + '.json')),
    'Retired reference data must stay Editor-only, not in player Resources: ' + name);
}
const regionSelectUi = readFileSync(join(root, 'Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceRegionSelect.cs'), 'utf8').replaceAll('\\r\\n', '\\n');
assert.ok(regionSelectUi.includes('var cardHeight = available ? 367f : 150f') &&
  regionSelectUi.includes('DrawRegionLockIcon') &&
  regionSelectUi.includes('var baseRect = new Rect(12f, y + 80f, contentWidth - 24f, 52f)') &&
  regionSelectUi.includes('var challengeRect = new Rect(12f, y + 164f + j * 43f, contentWidth - 24f, 38f)'),
  'region selection must preserve source card, challenge and line-icon proportions');
// Runtime-created materials alone are insufficient evidence for preserving
// instancing variants in a player build. Keep an explicit Resources material
// and matching variant collection, without changing the user's global settings.
const instancesPath = join(root, 'Assets/Resources/EchoSphere/NativeWorldSolidInstances');
const instanceGuid = readFileSync(instancesPath + '.shader.meta', 'utf8').match(/^guid: (\w+)/m)?.[1];
const instanceMaterial = readFileSync(instancesPath + '.mat', 'utf8');
const instanceVariants = readFileSync(join(root, 'Assets/Resources/EchoSphere/NativeWorldSolidInstanceVariants.shadervariants'), 'utf8');
assert.ok(instanceGuid && instanceMaterial.includes('guid: ' + instanceGuid) && /m_EnableInstancingVariants:\s*1/.test(instanceMaterial) &&
  instanceVariants.includes('guid: ' + instanceGuid) && /keywords:\s*INSTANCING_ON/.test(instanceVariants),
  'Native solid instancing requires the matching build-visible material and retained shader variant');
console.log(`Unity project preflight: PASS (Unity ${version}, ${runtimeFiles.length} C# source files, URP, authored SVG and responsive HUD guards)`);

// Production geometry is owned by retained native views; retired component
// implementations are not required as executable copies of the same artwork.
const retiredNativePorts=['SourcePlayerVisual2D','SourceRegionVisual2D','SourceElementalVisual2D','SphereElementalVfx2D','SphereRuntimeVfx2D','StandardSphereAttackVfx2D','ProjectileVfx2D','CombatArcVfx2D','SourceCanvasMesh'];
for(const name of retiredNativePorts)assert.ok(!existsSync(join(root,'Assets/EchoSphere/Scripts/Runtime',name+'.cs')),'retired native port returned: '+name);
const activeFamilyViews=readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldSphereFamilyViews.cs'),'utf8');
for(const family of ['sniper','shotgun','chain','aura','prism','gravity','pulse','void'])assert.ok(activeFamilyViews.includes('case "'+family+'"'),'active native family missing: '+family);
assert.ok(activeFamilyViews.includes('s.Disabled')&&activeFamilyViews.includes('DrawOrbit')&&activeFamilyViews.includes('DrawCore'),'native family disabled/core/orbit presentation missing');
const activeSphereElements=readFileSync(join(root,'Assets/EchoSphere/Scripts/Presentation/NativeWorldSphereElements.cs'),'utf8');
assert.ok(activeSphereElements.includes('DrawElements')&&activeSphereElements.includes('ElementGlyph'),'native elemental view missing');
