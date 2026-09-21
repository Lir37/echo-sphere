import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const read = (file) => fs.readFileSync(path.join(root, file), 'utf8');

const pkg = JSON.parse(read('package.json'));
assert.equal(typeof pkg.scripts['assets:3d'], 'string');
assert.equal(typeof pkg.scripts['validate:glb'], 'string');

const visual = read('src/visual3d.ts');
assert.match(visual, /GLTFLoader/);
assert.match(visual, /art3d/);
assert.doesNotMatch(visual, /rotation\.y\s*\+=/);
assert.doesNotMatch(visual, /new\s+THREE\./);

assert.ok(fs.existsSync(path.join(root, 'scripts', 'generate_art3d.py')));
assert.ok(fs.existsSync(path.join(root, 'scripts', 'validate_art3d.py')));

console.log('Echo Sphere production smoke checks passed.');
