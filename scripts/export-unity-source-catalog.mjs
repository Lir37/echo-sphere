import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { ABILITIES, SPHERE_TYPES, ARTIFACTS, BOSS_TYPES } from '../src/gameData.ts';
import { SPHERE_PROGRESSION, ABILITY_PROGRESSION, SPHERE_ABILITY_SYNERGIES, getSphereElementMasteryForBranch, getSphereElementForBranch } from '../src/sphereProgression.ts';
import { ARTIFACT_SYNERGIES, ARTIFACT_SETS } from '../src/artifactSystem.ts';
import { REGION_CONFIGS, getRegionPocketBrief } from '../src/region.ts';

// Transfer the actual current source metadata. Never synthesize a missing description.
const source = readFileSync('src/KnowledgeBase.tsx', 'utf8').replaceAll('\r\n', '\n');
function objectLiteral(label, opener, closer) {
  const begin = source.indexOf(opener, source.indexOf(label));
  const end = source.indexOf(closer, begin);
  return Function('return (' + source.slice(begin + 2, end + 1) + ')')();
}
const enemies = objectLiteral('const ENEMY_DEFS', '= [', '];');
const bosses = objectLiteral('const BOSS_INFO', '= {', '};');
const entries = [];
function card(section, id, name, desc, details = { ru: '', en: '' }, group = '') {
  entries.push({ section, id, group, nameRu: name.ru, nameEn: name.en, descRu: desc.ru, descEn: desc.en,
    detailsRu: details.ru, detailsEn: details.en });
}
for (const match of source.matchAll(/<KnowledgeCard unlocked title=\{isRu \? '([^']*)' : '([^']*)'\}\s*desc=\{isRu\s*\? '([^']*)'\s*: '([^']*)'\}/g))
  card('basics', '', { ru: match[1], en: match[2] }, { ru: match[3], en: match[4] });
// Preserve the explanatory paragraphs and the four Core concept cards verbatim.
const basics = source.slice(source.indexOf("if (section === 'basics')"), source.indexOf("if (section === 'spheres')"));
const concepts = [...basics.matchAll(/\[isRu \? '([^']*)' : '([^']*)', isRu \? '([^']*)' : '([^']*)'\]/g)];
entries[0].detailsRu = concepts.map(x => x[1] + ': ' + x[3]).join('\n');
entries[0].detailsEn = concepts.map(x => x[2] + ': ' + x[4]).join('\n');
const paragraphs = [...basics.matchAll(/<p[^>]*>\{isRu \? '([^']*)' : '([^']*)'\}/g)];
entries[1].detailsRu = paragraphs.map(x => x[1]).join('\n\n');
entries[1].detailsEn = paragraphs.map(x => x[2]).join('\n\n');
for (const [id, sphere] of Object.entries(SPHERE_TYPES)) {
  const roles = { standard: 'универсальный урон', sniper: 'дальняя одиночная цель', chain: 'перенос урона между целями', shotgun: 'ближний мощный залп', aura: 'локальное поле' };
  card('spheres', `sphere:${id}`, sphere.name, { ru: `Роль: ${roles[id] || 'специализированный сетевой узел'}.`, en: `Role: ${id} Sphere combat behavior.` },
    { ru: 'Эта запись открывается, когда сфера впервые появляется в твоём забеге.', en: 'This entry unlocks when the Sphere first appears in your run.' });
}
for (const enemy of enemies) card('monsters', `monster:${enemy.id}`, enemy.name, enemy.desc, enemy.rule);
for (const [id, boss] of Object.entries(BOSS_TYPES)) card('bosses', `boss:${id}`, boss.name, bosses[id]);
for (const [id, ability] of Object.entries(ABILITIES)) card('abilities', `ability:${id}`, ability.name, { ru: ability.desc.ru(1), en: ability.desc.en(1) });
for (const artifact of ARTIFACTS) card('artifacts', `artifact:${artifact.id}`, artifact.name, artifact.desc);
for (const sphere of Object.values(SPHERE_PROGRESSION)) {
  for (const branch of sphere.evolution4Choices) {
    const name = { ru: sphere.name.ru + ' · ' + branch.name.ru, en: sphere.name.en + ' · ' + branch.name.en };
    card('evolutions', `sphere-branch:${branch.id}`, name, branch.desc, undefined, 'sphere-branches');
    branch.final.forEach((final, index) => card('evolutions', `sphere-final:${sphere.type}:${branch.id}:${index}`,
      { ru: sphere.name.ru + ' · ' + final.name.ru, en: sphere.name.en + ' · ' + final.name.en }, final.desc, undefined, 'sphere-finals'));
  }
}
for (const ability of Object.values(ABILITY_PROGRESSION))
  for (const evo of [...(ability.evolution4 || []), ...(ability.evolution7 || [])]) card('evolutions', `ability-evolution:${evo.id}`, evo.name, evo.desc, undefined, 'ability-evolutions');
const files = ['src/KnowledgeBase.tsx', 'src/gameData.ts', 'src/sphereProgression.ts', 'src/artifactSystem.ts', 'src/engineProgression.ts', 'src/region.ts'];
const digest = createHash('sha256');
for (const file of files) digest.update(readFileSync(file, 'utf8').replaceAll('\r\n', '\n'));
const planner = [];
function plannerEntry(category, id, name, desc, extra = {}) {
  planner.push({ category, id, nameRu: name.ru, nameEn: name.en, descRu: desc.ru, descEn: desc.en, ...extra });
}
for (const [id, sphere] of Object.entries(SPHERE_TYPES)) plannerEntry('sphere', id, sphere.name, sphere.desc);
for (const [id, ability] of Object.entries(ABILITIES)) {
  plannerEntry(ability.category, id, ability.name, { ru: ability.desc.ru(1), en: ability.desc.en(1) }, {
    levelsRu: Array.from({ length: 7 }, (_, index) => ability.desc.ru(index + 1)),
    levelsEn: Array.from({ length: 7 }, (_, index) => ability.desc.en(index + 1)),
  });
}
for (const sphere of Object.values(SPHERE_PROGRESSION)) for (const branch of sphere.evolution4Choices) {
  plannerEntry('sphere-branch', branch.id, branch.name, branch.desc);
  const element = getSphereElementForBranch(branch.id);
  const icon = element === 'fire' ? '🔥' : element === 'freeze' ? '❄️' : '☠️';
  const elementRu = element === 'fire' ? 'Огонь' : element === 'freeze' ? 'Заморозка' : 'Яд';
  const elementEn = element === 'fire' ? 'Fire' : element === 'freeze' ? 'Freeze' : 'Poison';
  plannerEntry('evolution-choice', branch.id, branch.name, element ? { ru: `${branch.desc.ru} ${icon} Стихия: ${elementRu}.`, en: `${branch.desc.en} ${icon} Element: ${elementEn}.` } : branch.desc);
  branch.final.forEach((final, index) => {
    plannerEntry('sphere-final', `${branch.id}_final_${index + 1}`, final.name, final.desc);
    const mastery = getSphereElementMasteryForBranch(branch.id, index);
    plannerEntry('evolution-choice', `${branch.id}_final_${index + 1}`,
      mastery ? { ru: `${final.name.ru} • ${mastery.name.ru}`, en: `${final.name.en} • ${mastery.name.en}` } : final.name,
      mastery ? { ru: `${final.desc.ru} ${mastery.desc.ru}`, en: `${final.desc.en} ${mastery.desc.en}` } : final.desc);
  });
}
for (const [type, sphere] of Object.entries(SPHERE_PROGRESSION)) for (let level = 1; level <= 7; level++) {
  for (const branch of [null, ...sphere.evolution4Choices]) {
    if (branch && level !== 5 && level !== 6) continue;
    let nameRu = sphere.name.ru + ' — уровень ' + level, nameEn = sphere.name.en + ' — level ' + level;
    let desc = sphere.levels[level - 1].desc;
    if (level === 4) { nameRu += ': Мутация I'; nameEn += ': Mutation I'; desc = { ru: 'Повышает сферу до IV уровня. После выбора откроется отдельное окно с 3 мутациями, из которых можно выбрать одну.', en: 'Raises the sphere to level IV. After this choice, a separate window opens with 3 mutations and you choose one.' }; }
    else if (level === 7) { nameRu += ': Мутация II'; nameEn += ': Mutation II'; desc = { ru: 'Повышает сферу до VII уровня. После этого откроется отдельное окно с 3 финальными специализациями.', en: 'Raises the sphere to level VII. After this choice, a separate window opens with 3 final specializations.' }; }
    else if (branch) { desc = level === 5 ? branch.level5 : branch.level6; nameRu += ': ветка «' + branch.name.ru + '»'; nameEn += ': branch “' + branch.name.en + '”'; }
    plannerEntry('sphere-upgrade', `${type}|${level}|${branch?.id || ''}`, { ru: nameRu, en: nameEn }, desc);
  }
}
for (const [id, ability] of Object.entries(ABILITIES)) for (let level = 1; level <= 7; level++)
  plannerEntry('ability-upgrade', `${id}|${level}`, ability.name, ABILITY_PROGRESSION[id]?.levels[level - 1]?.desc || { ru: ability.desc.ru(level), en: ability.desc.en(level) });
for (const ability of Object.values(ABILITY_PROGRESSION)) {
  const choices = [...(ability.evolution4 || []), ...(ability.evolution7 || []), ...Object.values(ability.evolution7ByBranch || {}).flat()];
  for (const choice of choices) if (!planner.some(entry => entry.id === choice.id)) plannerEntry('ability-evolution', choice.id, choice.name, choice.desc);
}
for (const synergy of SPHERE_ABILITY_SYNERGIES) plannerEntry('sphere-synergy', synergy.id, synergy.name, synergy.desc, {
  sphere: synergy.sphere, sphereBranch: synergy.sphereBranch, ability: synergy.ability, abilityFinal: synergy.abilityFinal,
});
for (const synergy of ARTIFACT_SYNERGIES) plannerEntry('artifact-synergy', synergy.id, synergy.name, synergy.desc, { requires: synergy.requires });
for (const set of ARTIFACT_SETS) plannerEntry('artifact-set', set.id, set.name, set.desc, { requires: set.synergyIds });
for (const region of REGION_CONFIGS) {
  plannerEntry('region', region.id, region.name, region.subtitle, { color: region.accent, requires: region.challenges.map(challenge => challenge.id) });
  for (const pocket of region.pockets) plannerEntry('region-pocket', `${region.id}|${pocket.id}`, pocket.name, getRegionPocketBrief(pocket), { color: pocket.accent });
  for (const challenge of region.challenges) plannerEntry('region-challenge', challenge.id, challenge.name, challenge.desc);
}
const text = JSON.stringify({ sourceCommit: '737ef2af4a877e0995db5f92331d536d2f29a934', sourceDigest: digest.digest('hex'), entries, planner }, null, 2) + '\n';
const output = 'UnityProject/Assets/Editor/EchoSphereReference/SourceCatalog.json';
if (process.argv.includes('--check')) {
  if (readFileSync(output, 'utf8').replaceAll('\r\n', '\n') !== text) throw new Error('Unity catalog differs from new-desing source metadata');
} else { mkdirSync('UnityProject/Assets/Editor/EchoSphereReference', { recursive: true }); writeFileSync(output, text); }
console.log(`Unity source catalog: ${entries.length} entries, exact source strings`);
