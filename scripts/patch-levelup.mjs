import fs from 'node:fs';

const path = 'src/engine.ts';
let s = fs.readFileSync(path, 'utf8');

// This script is intentionally a local migration helper. It is not executed automatically.
// It migrates the old split tower/ability level-up flow to the unified progression model.

s = s.replace(
  "export interface UpgradeChoice {\n  type: 'ability' | 'evolve';\n  ability?: AbilityType;\n  evolution?: string;\n  currentLevel: number;\n  newLevel: number;\n}",
  "export interface UpgradeChoice {\n  type: 'ability' | 'evolve' | 'tower';\n  ability?: AbilityType;\n  evolution?: string;\n  towerType?: SphereType;\n  towerBranch?: import('./towerProgression').TowerEvolutionId;\n  towerFinalIndex?: number;\n  towerStage?: 'upgrade' | 'branch' | 'final';\n  name?: { ru: string; en: string };\n  desc?: { ru: string; en: string };\n  currentLevel: number;\n  newLevel: number;\n}",
);

s = s.replace(
  "  towerProgression: Partial<Record<SphereType, number>>;",
  "  towerProgression: Partial<Record<SphereType, number>>;\n  towerBranches: Partial<Record<SphereType, import('./towerProgression').TowerEvolutionId>>;",
);

s = s.replace(
  "    towerProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },",
  "    towerProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },\n    towerBranches: {},",
);

const start = s.indexOf('export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {');
const towerStart = s.indexOf('function generateTowerUpgradeChoices(s: GameState): TowerUpgradeChoice[] {', start);
const checkStart = s.indexOf('function checkEvolution(s: GameState): string | null {', start);
if (start < 0 || towerStart < 0 || checkStart < 0) throw new Error('Could not locate level-up functions');

const newGenerator = `export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {
  const towerTypes = Object.keys(TOWER_PROGRESSION) as SphereType[];

  // A tower at Lv.3 or Lv.6 gets its dedicated milestone choice first.
  const milestone = towerTypes.find(type => towerLevel(s, type) === 3 || towerLevel(s, type) === 6);
  if (milestone) {
    const level = towerLevel(s, milestone);
    const def = TOWER_PROGRESSION[milestone];
    if (level === 3) {
      return def.evolution4Choices.map(branch => ({
        type: 'tower' as const,
        towerType: milestone,
        towerBranch: branch.id,
        towerStage: 'branch' as const,
        name: branch.name,
        desc: branch.desc,
        currentLevel: 3,
        newLevel: 4,
      }));
    }
    const branchId = s.player.towerBranches[milestone];
    const branch = def.evolution4Choices.find(x => x.id === branchId) ?? def.evolution4Choices[0];
    return branch.final.map((finalChoice, index) => ({
      type: 'tower' as const,
      towerType: milestone,
      towerBranch: branch.id,
      towerFinalIndex: index,
      towerStage: 'final' as const,
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

  // Guarantee one ability/passive slot whenever the pool has anything available.
  if (abilityPool.length) {
    const id = abilityPool[0];
    const currentLevel = s.player.abilities[id] || 0;
    choices.push({ type: 'ability', ability: id, currentLevel, newLevel: currentLevel + 1 });
  }

  const towerPool = towerTypes
    .filter(type => towerLevel(s, type) < 7)
    .map(type => {
      const currentLevel = towerLevel(s, type);
      const nextLevel = currentLevel + 1;
      const def = TOWER_PROGRESSION[type];
      const levelDef = def.levels[nextLevel - 1];
      return {
        type: 'tower' as const,
        towerType: type,
        towerStage: 'upgrade' as const,
        currentLevel,
        newLevel: nextLevel,
        name: levelDef.name,
        desc: levelDef.desc,
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
s = s.slice(0, start) + newGenerator + s.slice(checkStart);

s = s.replace(
  "    s.pendingUpgrade = generateUpgradeChoices(s);\n    if (s.player.level > 1) s.pendingTowerUpgrade = generateTowerProgressionChoices(s);",
  "    s.pendingUpgrade = generateUpgradeChoices(s);\n    s.pendingTowerUpgrade = null;",
);

const applyStart = s.indexOf('export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {');
const applyTowerStart = s.indexOf('export function applyTowerUpgrade(s: GameState, choice: TowerUpgradeChoice): void {', applyStart);
if (applyStart < 0 || applyTowerStart < 0) throw new Error('Could not locate apply functions');

const applyBody = `export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {
  if (choice.type === 'tower' && choice.towerType) {
    const type = choice.towerType;
    const current = towerLevel(s, type);
    if (current >= 7) return;
    const next = current + 1;
    s.player.towerProgression[type] = next;
    for (const sphere of s.spheres) if (sphere.type === type) sphere.visualTier = next;

    if (choice.towerStage === 'branch' && choice.towerBranch) {
      s.player.towerBranches[type] = choice.towerBranch;
      s.player.evolutions.push(\`tower:\${type}:4:\${choice.towerBranch}\`);
      s.evolutionsThisRun++;
      playSound('evolve');
    }
    if (choice.towerStage === 'final') {
      s.player.evolutions.push(\`tower:\${type}:7:\${choice.towerBranch ?? 'unknown'}:\${choice.towerFinalIndex ?? 0}\`);
      s.evolutionsThisRun++;
      s.flashText = { text: choice.name?.ru ?? 'Эволюция башни', life: 2.2, color: '#c4453d' };
      playSound('evolve');
    }
    s.pendingUpgrade = null;
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
s = s.slice(0, applyStart) + applyBody + s.slice(applyTowerStart);

// Keep the legacy API callable by older UI code, but route it into the new system.
const towerBodyStart = s.indexOf('export function applyTowerUpgrade(s: GameState, choice: TowerUpgradeChoice): void {');
const towerBodyEnd = s.indexOf('\n}', towerBodyStart) + 2;
if (towerBodyStart >= 0 && towerBodyEnd > towerBodyStart) {
  s = s.slice(0, towerBodyStart) + `export function applyTowerUpgrade(s: GameState, choice: TowerUpgradeChoice): void {\n  const type = (choice as TowerUpgradeChoice & { towerType?: SphereType }).towerType;\n  if (!type) return;\n  applyUpgrade(s, { type: 'tower', towerType: type, towerStage: 'upgrade', currentLevel: towerLevel(s, type), newLevel: towerLevel(s, type) + 1 });\n}` + s.slice(towerBodyEnd);
}

fs.writeFileSync(path, s);
console.log('Level-up migration applied to src/engine.ts');
