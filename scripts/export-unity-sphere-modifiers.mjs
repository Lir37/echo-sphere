import { readFileSync, writeFileSync, existsSync } from 'node:fs';
import { registerHooks } from 'node:module';
registerHooks({ resolve(specifier, context, nextResolve) {
  if (specifier.startsWith('.') && !/\.[a-z]+$/i.test(specifier)) {
    const url = new URL(specifier + '.ts', context.parentURL);
    if (existsSync(url)) return nextResolve(url.href, context);
  }
  return nextResolve(specifier, context);
}});
const { sphereModifiers, SPHERE_PROGRESSION, getAuthoredSphereModifierLevels } = await import('../src/sphereProgression.ts');
const rows = [];
for (const [type, progression] of Object.entries(SPHERE_PROGRESSION)) {
  const states = [{ branch: '', final: -1 }];
  for (const branch of progression.evolution4Choices) {
    states.push({ branch: branch.id, final: -1 });
    branch.final.forEach((_, final) => states.push({ branch: branch.id, final }));
  }
  for (let level = 0; level <= 7; level++) for (const state of states) {
    if (state.branch && level < 4 || state.final >= 0 && level < 7) continue;
    const player = { abilities: {}, sphereProgression: { [type]: level }, sphereBranches: state.branch ? { [type]: state.branch } : {},
      sphereMods: {}, evolutions: state.final >= 0 ? [`sphere:${type}:7:${state.final}`] : [], artifacts: [] };
    rows.push({ key: `${type}|${level}|${state.branch}|${state.final}`, mods: sphereModifiers({ player, spheres: [] }, type), authored: getAuthoredSphereModifierLevels(player, type) });
  }
}
const fields = Object.keys(rows[0].mods);
const f = value => Number(value).toPrecision(10).replace(/(?:\.0+|(?<=\.[0-9]*?)0+)$/, '') + 'f';
const lines = ['// Generated from new-desing/src/sphereProgression.ts by export-unity-sphere-modifiers.mjs.',
  '// Do not edit the numeric values by hand. Global modifiers are combined at runtime.',
  'using System;', 'using System.Collections.Generic;', 'namespace EchoSphere.Core', '{',
  '    public sealed class SourceSphereModifiers', '    {',
  '        public float ' + fields.map(key => key === 'static' ? '@static' : key).join(', ') + ';',
  '        public SourceSphereModifiers Clone() => (SourceSphereModifiers)MemberwiseClone();', '    }',
  '    public static class SourceSphereModifiersRules', '    {',
  '        private static readonly Dictionary<string, SourceSphereModifiers> Values = new Dictionary<string, SourceSphereModifiers>(StringComparer.Ordinal)', '        {'];
for (const row of rows) lines.push(`            [${JSON.stringify(row.key)}] = new SourceSphereModifiers { ${fields.map(key => `${key === 'static' ? '@static' : key} = ${f(row.mods[key])}`).join(', ')} },`);
lines.push('        };', '        public static SourceSphereModifiers Get(SphereId type, int level, string branch, string final)', '        {',
  '            level = Math.Max(0, Math.Min(7, level));', '            var index = final == branch + "_final_1" ? 0 : final == branch + "_final_2" ? 1 : final == branch + "_final_3" ? 2 : -1;',
  '            var key = type.ToString().ToLowerInvariant() + "|" + level + "|" + (branch ?? "") + "|" + index;',
  '            if (!Values.TryGetValue(key, out var value)) value = Values[type.ToString().ToLowerInvariant() + "|" + level + "||-1"];',
  '            return value.Clone();', '        }', '    }', '}');
const output = 'UnityProject/Assets/EchoSphere/Scripts/Core/SourceSphereModifiersRules.cs';
const text = lines.join('\n') + '\n';
if (process.argv.includes('--check')) {
  if (readFileSync(output, 'utf8').replaceAll('\r\n', '\n') !== text) throw Error('Unity Sphere modifier values differ from source');
} else writeFileSync(output, text);
console.log(`Unity Sphere modifier transfer: ${rows.length} authored progression states, ${fields.length} parameters each`);
