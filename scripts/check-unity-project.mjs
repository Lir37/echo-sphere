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
console.log(`Unity project preflight: PASS (Unity ${version}, ${runtimeFiles.length} C# source files)`);
