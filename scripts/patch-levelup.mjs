import fs from 'node:fs';

const enginePath = 'src/engine.ts';
const appPath = 'src/App.tsx';

let engine = fs.readFileSync(enginePath, 'utf8');
let app = fs.readFileSync(appPath, 'utf8');

function replaceSection(text, startMarker, endMarker, replacement) {
  const start = text.indexOf(startMarker);
  const end = text.indexOf(endMarker, start);
  if (start < 0 || end < 0) throw new Error(`Could not locate section: ${startMarker}`);
  return text.slice(0, start) + replacement + text.slice(end);
}

// Player/choice types. These replacements are idempotent.
if (!engine.includes("type: 'ability' | 'evolve' | 'tower';")) {
  engine = engine.replace(
    /export interface UpgradeChoice \{[\s\S]*?\n\}/,
    `export interface UpgradeChoice {
  type: 'ability' | 'evolve' | 'tower';
  ability?: AbilityType;
  evolution?: string;
  sphereType?: SphereType;
  sphereBranch?: import('./sphereProgression').SphereEvolutionId;
  sphereFinalIndex?: number;
  sphereStage?: 'upgrade' | 'branch' | 'final';
  name?: { ru: string; en: string };
  desc?: { ru: string; en: string };
  currentLevel: number;
  newLevel: number;
}`,
  );
}
if (!engine.includes('sphereBranches: Partial<Record<SphereType')) {
  engine = engine.replace(
    '  sphereProgression: Partial<Record<SphereType, number>>;',
    "  sphereProgression: Partial<Record<SphereType, number>>;\n  sphereBranches: Partial<Record<SphereType, import('./sphereProgression').SphereEvolutionId>>;",
  );
}
if (!engine.includes('sphereBranches: {},')) {
  engine = engine.replace(
    '    sphereProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },',
    '    sphereProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },\n    sphereBranches: {},',
  );
}

// One unified level-up generator.
const generator = `export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {
  const sphereTypes = Object.keys(SPHERE_PROGRESSION) as SphereType[];

  // Level 4: a tower at Lv.3 forces its own three branch choices.
  // Level 7: a tower at Lv.6 forces the three finals of the selected branch.
  const milestone = sphereTypes.find(type => sphereLevel(s, type) === 3 || sphereLevel(s, type) === 6);
  if (milestone) {
    const level = sphereLevel(s, milestone);
    const def = SPHERE_PROGRESSION[milestone];
    if (level === 3) {
      return def.evolution4Choices.map(branch => ({
        type: 'tower' as const,
        sphereType: milestone,
        sphereBranch: branch.id,
        sphereStage: 'branch' as const,
        name: branch.name,
        desc: branch.desc,
        currentLevel: 3,
        newLevel: 4,
      }));
    }
    const branchId = s.player.sphereBranches[milestone];
    const branch = def.evolution4Choices.find(x => x.id === branchId) ?? def.evolution4Choices[0];
    return branch.final.map((finalChoice, index) => ({
      type: 'tower' as const,
      sphereType: milestone,
      sphereBranch: branch.id,
      sphereFinalIndex: index,
      sphereStage: 'final' as const,
      name: finalChoice.name,
      desc: finalChoice.desc,
      currentLevel: 6,
      newLevel: 7,
    }));
  }

  const choices: UpgradeChoice[] = [];
  const abilityPool = (Object.keys(ABILITIES) as AbilityType[])
    .filter(id => (s.player.abilities[id] || 0) < ABILITIES[id].maxLevel)
    .sort(() => Math.random() - 0.5);

  // Every ordinary level-up contains at least one ability/passive/active option.
  if (abilityPool.length) {
    const id = abilityPool[0];
    const currentLevel = s.player.abilities[id] || 0;
    choices.push({ type: 'ability', ability: id, currentLevel, newLevel: currentLevel + 1 });
  }

  const towerPool = sphereTypes
    .filter(type => sphereLevel(s, type) < 7)
    .map(type => {
      const currentLevel = sphereLevel(s, type);
      const nextLevel = currentLevel + 1;
      const def = SPHERE_PROGRESSION[type];
      const levelDef = def.levels[nextLevel - 1];
      const branchId = s.player.sphereBranches[type];
      const branch = branchId ? def.evolution4Choices.find(x => x.id === branchId) : null;
      const branchProgress = nextLevel === 5 || nextLevel === 6;
      const name = branchProgress && branch
        ? { ru: `${branch.name.ru} — уровень ${nextLevel}`, en: `${branch.name.en} — level ${nextLevel}` }
        : levelDef.name;
      const desc = branchProgress && branch
        ? {
            ru: nextLevel === 5
              ? `Развитие ветки «${branch.name.ru}»: ${branch.desc.ru}`
              : `Углубление механики ветки «${branch.name.ru}»`,
            en: nextLevel === 5
              ? `Develop the “${branch.name.en}” branch: ${branch.desc.en}`
              : `Deepen the “${branch.name.en}” branch mechanic`,
          }
        : levelDef.desc;
      return {
        type: 'tower' as const,
        sphereType: type,
        sphereBranch: branchId,
        sphereStage: 'upgrade' as const,
        currentLevel,
        newLevel: nextLevel,
        name,
        desc,
      };
    })
    .sort(() => Math.random() - 0.5);

  const abilityRest = abilityPool.slice(1).map(id => {
    const currentLevel = s.player.abilities[id] || 0;
    return { type: 'ability' as const, ability: id, currentLevel, newLevel: currentLevel + 1 };
  });

  for (const choice of [...towerPool, ...abilityRest].sort(() => Math.random() - 0.5)) {
    if (choices.length >= 3) break;
    choices.push(choice);
  }
  return choices.slice(0, 3);
}

`;
engine = replaceSection(engine, 'export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {', 'function checkEvolution(s: GameState): string | null {', generator);

// Unified tower/ability application. Lv.4 stores the branch; Lv.5-6 keep it; Lv.7 stores the final specialization.
const apply = `export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {
  if (choice.type === 'tower' && choice.sphereType) {
    const type = choice.sphereType;
    const current = sphereLevel(s, type);
    if (current >= 7) return;
    const next = current + 1;
    s.player.sphereProgression[type] = next;
    for (const sphere of s.spheres) if (sphere.type === type) sphere.visualTier = next;

    if (choice.sphereStage === 'branch' && choice.sphereBranch) {
      s.player.sphereBranches[type] = choice.sphereBranch;
      s.player.evolutions.push(\`tower:\${type}:4:\${choice.sphereBranch}\`);
      s.evolutionsThisRun++;
      s.flashText = { text: choice.name?.ru ?? 'Эволюция сферы', life: 2.2, color: '#d4943d' };
      playSound('evolve');
    } else if (choice.sphereStage === 'final') {
      s.player.evolutions.push(\`tower:\${type}:7:\${choice.sphereBranch ?? 'unknown'}:\${choice.sphereFinalIndex ?? 0}\`);
      s.evolutionsThisRun++;
      s.flashText = { text: choice.name?.ru ?? 'Финальная специализация', life: 2.2, color: '#c4453d' };
      playSound('evolve');
    }
    return;
  }

  if (choice.type === 'ability' && choice.ability) {
    s.player.abilities[choice.ability] = Math.min(7, choice.newLevel);
    const progression = ABILITY_PROGRESSION[choice.ability];
    if (progression && (choice.newLevel === 4 || choice.newLevel === 7)) {
      const evolution = choice.newLevel === 4 ? progression.evolution4 : progression.evolution7;
      const marker = \`ability:\${choice.ability}:\${choice.newLevel}\`;
      if (!s.player.evolutions.includes(marker)) {
        s.player.evolutions.push(marker);
        s.evolutionsThisRun++;
        s.flashText = { text: evolution.name.ru, life: 1.8, color: choice.newLevel === 7 ? '#c4453d' : '#d4943d' };
        playSound('evolve');
      }
    }
    const def = ABILITIES[choice.ability];
    if (def.category === 'active' && choice.currentLevel === 0) assignHotkey(s, choice.ability);
    if (choice.ability === 'vitality') {
      s.player.maxHp += 20;
      s.player.hp += 20;
    }
    return;
  }

  if (choice.type === 'evolve' && choice.evolution) {
    const def = EVOLUTION_MAP[choice.evolution];
    if (!def) return;
    s.player.abilities[def.a] = undefined;
    s.player.abilities[def.b] = undefined;
    s.player.evolutions.push(choice.evolution);
    s.evolutionsThisRun++;
    playSound('evolve');
  }
}

`;
engine = replaceSection(engine, 'export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {', 'export function applyTowerUpgrade(s: GameState, choice: SphereUpgradeChoice): void {', apply);

// Legacy API stays available for any old caller, but no separate progression modal is generated anymore.
const legacyTower = `export function applyTowerUpgrade(s: GameState, choice: SphereUpgradeChoice): void {
  const type = (choice as SphereUpgradeChoice & { sphereType?: SphereType }).sphereType;
  if (!type) return;
  applyUpgrade(s, { type: 'tower', sphereType: type, sphereStage: 'upgrade', sphereBranch: s.player.sphereBranches[type], currentLevel: sphereLevel(s, type), newLevel: sphereLevel(s, type) + 1 });
}
`;
engine = replaceSection(engine, 'export function applyTowerUpgrade(s: GameState, choice: SphereUpgradeChoice): void {', 'function generateSphereProgressionChoices', legacyTower);

engine = engine.replace('if (s.player.level > 1) s.pendingSphereUpgrade = generateSphereProgressionChoices(s);', 's.pendingSphereUpgrade = null;');

// App: one modal for all level-up choices. Remove the old second tower modal/import.
app = app.replace('  applyTowerUpgrade, openChest,', '  openChest,');
app = app.replace('  type SphereUpgradeChoice, MAP_THEMES, type MapTheme,', '  MAP_THEMES, type MapTheme,');
app = app.replace(/\n\s*\{st\.pendingSphereUpgrade && <TowerUpgradeModal[\s\S]*?\n\s*\}\}\n/, '\n');
app = app.replace(/\nfunction TowerUpgradeModal\([\s\S]*?\n\}\n\n\/\/ ===== Artifact Modal =====/, '\n// ===== Artifact Modal =====');
app = app.replace(/\s*\|\| st\.pendingSphereUpgrade/g, '');
app = app.replace(/\s*&& !st\.pendingSphereUpgrade/g, '');

const modal = `function UpgradeModal({ lang, t, st, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; st: GameState; onPick: (c: UpgradeChoice) => void;
}) {
  const choices = st.pendingUpgrade || [];
  const first = choices[0];
  const title = first?.sphereStage === 'branch'
    ? (lang === 'ru' ? 'Эволюция сферы I' : 'Tower Evolution I')
    : first?.sphereStage === 'final'
      ? (lang === 'ru' ? 'Финальная специализация' : 'Final Specialization')
      : t('chooseUpgrade');

  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-2xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-6 text-[#4a7a8a]">{title}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((c, i) => {
            if (c.type === 'tower') {
              const label = c.sphereStage === 'branch'
                ? (lang === 'ru' ? 'ВЕТКА сферы' : 'TOWER BRANCH')
                : c.sphereStage === 'final'
                  ? (lang === 'ru' ? 'ФИНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ' : 'FINAL SPECIALIZATION')
                  : (lang === 'ru' ? 'УЛУЧШЕНИЕ сферы' : 'TOWER UPGRADE');
              return (
                <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] border border-[#5a8c4a]/30 hover:border-[#5a8c4a]/60 hover:scale-105 transition-all text-left">
                  <div className="text-[#5a8c4a] text-[10px] uppercase tracking-wider mb-1">{label}</div>
                  <div className="font-bold text-lg mb-2">{c.name?.[lang] || 'Tower'}</div>
                  <div className="text-sm text-[#5a4a32] mb-2">{c.desc?.[lang] || ''}</div>
                  <div className="text-xs text-[#8a7a5a]/70">{t('level')} {c.currentLevel} → {c.newLevel}</div>
                </button>
              );
            }
            if (c.type === 'evolve' && c.evolution) {
              const evo = EVOLUTION_MAP[c.evolution];
              return (
                <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] border border-[#d4943d]/40 hover:border-[#d4943d]/60 hover:scale-105 transition-all text-left">
                  <div className="text-[#d4943d] text-xs uppercase mb-1">{t('evolution')}</div>
                  <div className="font-bold text-lg mb-2">{evo.name[lang]}</div>
                  <div className="text-sm text-[#5a4a32]">{evo.desc[lang]}</div>
                </button>
              );
            }
            const def = ABILITIES[c.ability!];
            return (
              <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] border border-[#4a7a8a]/30 hover:border-[#4a7a8a]/60 hover:scale-105 transition-all text-left">
                <div className="text-[#4a7a8a] text-xs uppercase mb-1">{def.category === 'active' ? t('active') : t('passive')}</div>
                <div className="font-bold text-lg mb-2">{def.name[lang]}</div>
                <div className="text-sm text-[#5a4a32] mb-2">{def.desc[lang](c.newLevel)}</div>
                <div className="text-xs text-[#8a7a5a]/70">{t('level')} {c.currentLevel} → {c.newLevel} / {def.maxLevel}</div>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

// ===== Artifact Modal =====`;
app = replaceSection(app, 'function UpgradeModal({ lang, t, st, onPick }: {', '// ===== Artifact Modal =====', modal);

fs.writeFileSync(enginePath, engine);
fs.writeFileSync(appPath, app);
console.log('Unified level-up progression and modal migration applied.');
