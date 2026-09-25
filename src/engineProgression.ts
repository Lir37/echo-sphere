import { ABILITIES, ACTIVE_KEYS, SPHERE_TYPES } from './gameData';
import type { AbilityType, SphereType, ArtifactId } from './gameData';
import { playSound } from './audio';
import { CHARACTER_DEFS } from './characters';
import { SPHERE_PROGRESSION, ABILITY_PROGRESSION, sphereLevel } from './sphereProgression';
import { nextRandom } from './rng';
import type { GameState, SphereMods, SphereUpgradeChoice, UpgradeChoice } from './engineTypes';

const SPHERE_MODIFIER_CHOICES: ReadonlyArray<{
  id: keyof SphereMods;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}> = [
  {
    id: 'multishot',
    name: { ru: 'Мультивыстрел', en: 'Multishot' },
    desc: { ru: 'Каждый выстрел выпускает ещё один снаряд.', en: 'Each shot fires one additional projectile.' },
  },
  {
    id: 'pierce',
    name: { ru: 'Пробитие', en: 'Pierce' },
    desc: { ru: 'Снаряд проходит ещё через одного врага.', en: 'Projectiles pass through one additional enemy.' },
  },
  {
    id: 'ricochet',
    name: { ru: 'Рикошет', en: 'Ricochet' },
    desc: { ru: 'После попадания снаряд может перейти к новой цели.', en: 'After a hit, the projectile can redirect to a new target.' },
  },
  {
    id: 'fire',
    name: { ru: 'Огонь', en: 'Fire' },
    desc: { ru: 'Попадания поджигают врагов и наносят урон со временем.', en: 'Hits ignite enemies and deal damage over time.' },
  },
  {
    id: 'freeze',
    name: { ru: 'Заморозка', en: 'Freeze' },
    desc: { ru: 'Попадания замедляют врага полной остановкой на короткое время.', en: 'Hits briefly freeze the enemy.' },
  },
  {
    id: 'poison',
    name: { ru: 'Яд', en: 'Poison' },
    desc: { ru: 'Попадания отравляют врагов и наносят урон со временем.', en: 'Hits poison enemies and deal damage over time.' },
  },
];

function pickArtifacts(s: GameState): ArtifactId[] {
  return pickArtifactChoices(s, 3, false, () => nextRandom(s));
}


export function assignHotkey(s: GameState, ability: AbilityType): string {
  // already mapped?
  for (const k of Object.keys(s.activeKeyMap)) {
    if (s.activeKeyMap[k] === ability) return k;
  }
  for (const k of ACTIVE_KEYS) {
    if (!(k in s.activeKeyMap)) {
      s.activeKeyMap[k] = ability;
      return k;
    }
  }
  return '';
}
function getSphereEvolutionChoices(s:GameState, type:SphereType, level:4|7):UpgradeChoice[] {
  const def=SPHERE_PROGRESSION[type];
  if(level===4){
    return def.evolution4Choices.slice(0,3).map((branch)=>({
      type:'sphere' as const,
      sphereType:type,
      sphereBranch:branch.id,
      sphereStage:'branch' as const,
      name:{ru:branch.name.ru,en:branch.name.en},
      desc:branch.desc,
      currentLevel:4,
      newLevel:4,
    }));
  }
  const branchId=s.player.sphereBranches[type];
  const branch=def.evolution4Choices.find((x)=>x.id===branchId);
  if(!branch) return [];
  return branch.final.slice(0,3).map((finalChoice,index)=>({
    type:'sphere' as const,
    sphereType:type,
    sphereBranch:branch.id,
    sphereFinalIndex:index,
    sphereStage:'final' as const,
    name:finalChoice.name,
    desc:finalChoice.desc,
    currentLevel:7,
    newLevel:7,
  }));
}

function getAbilityEvolutionChoices(s:GameState, ability:AbilityType, level:4|7):UpgradeChoice[] {
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression) return [];
  const pool=level===4?progression.evolution4:progression.evolution7;
  return pool.slice(0,3).map((evolution,index)=>({
    type:'ability' as const,
    ability,
    abilityEvolutionIndex:index,
    abilityStage:level===4?'branch' as const:'final' as const,
    name:evolution.name,
    desc:evolution.desc,
    currentLevel:level,
    newLevel:level,
  }));
}

function weightedShuffle<T>(s: GameState, items: T[], getWeight: (item: T) => number): T[] {
  const pool = [...items];
  const result: T[] = [];
  while (pool.length > 0) {
    let totalWeight = 0;
    for (const item of pool) totalWeight += Math.max(0.01, getWeight(item));
    let roll = nextRandom(s) * totalWeight;
    let selectedIndex = pool.length - 1;
    for (let index = 0; index < pool.length; index++) {
      roll -= Math.max(0.01, getWeight(pool[index]));
      if (roll <= 0) {
        selectedIndex = index;
        break;
      }
    }
    result.push(pool.splice(selectedIndex, 1)[0]);
  }
  return result;
}

type UpgradeSource = 'ability' | 'sphere' | 'modifier';

function getUpgradeChoiceSource(choice: UpgradeChoice): UpgradeSource {
  return choice.type;
}

/**
 * Level-Up protection layer:
 * - pity increases when a source is repeatedly not selected;
 * - underrepresented build systems receive a small pressure bonus;
 * - item-level affinity remains authoritative on top of the source pressure.
 *
 * Reroll/Lock/Ban stay separate progression features and are not silently
 * invented here.
 */
export function getUpgradeSourceWeight(s: GameState, source: UpgradeSource): number {
  const pity = Math.min(4, Math.max(0, s.levelUpPity?.[source] || 0));
  const pityWeight = 1 + pity * 0.20;

  const sphereScore =
    s.spheres.filter((sphere) => sphere.alive).length +
    Object.values(s.player.sphereProgression || {}).reduce((sum, level) => sum + (level || 0) * 0.15, 0);
  const abilityScore = Object.values(s.player.abilities || {}).filter((level) => (level || 0) > 0).length;
  const modifierScore = Object.values(s.player.sphereMods || {}).filter((level) => (level || 0) > 0).length;

  const scores: Record<UpgradeSource, number> = { sphere: sphereScore, ability: abilityScore, modifier: modifierScore };
  const minimum = Math.min(scores.sphere, scores.ability, scores.modifier);
  const underrepresentedWeight = scores[source] <= minimum + 0.001 ? 1.15 : 1;

  return pityWeight * underrepresentedWeight;
}

function isLiveUpgradeChoice(s: GameState, choice: UpgradeChoice): boolean {
  if (choice.type === 'sphere' && choice.sphereType) {
    return sphereLevel(s, choice.sphereType) < 7;
  }

  if (choice.type === 'modifier' && choice.modifier) {
    return (s.player.sphereMods[choice.modifier] || 0) <= 0;
  }

  if (choice.type === 'ability' && choice.ability) {
    const current = s.player.abilities[choice.ability] || 0;
    const def = ABILITIES[choice.ability];
    if (!def || current >= def.maxLevel) return false;
    if (def.category === 'active' && current === 0) {
      const activeCount = Object.keys(s.activeKeyMap || {}).length;
      if (activeCount >= s.player.activeAbilitySlots) return false;
    }
    return true;
  }

  return false;
}

function recordLevelUpSourcePick(s: GameState, choice: UpgradeChoice): void {
  const selected = getUpgradeChoiceSource(choice);
  for (const source of ['ability', 'sphere', 'modifier'] as UpgradeSource[]) {
    s.levelUpPity[source] = source === selected
      ? 0
      : Math.min(4, (s.levelUpPity[source] || 0) + 1);
  }
}

export function getSphereUpgradeChoiceWeight(s: GameState, type: SphereType): number {
  const level = sphereLevel(s, type);
  const activeCopies = s.spheres.filter((sphere) => sphere.alive && sphere.type === type).length;
  const levelPressure = (7 - level) * 0.25;
  const activeBuildPressure = activeCopies > 0 ? 1.5 : 0;
  const characterAffinity = CHARACTER_DEFS[s.player.characterId]?.preferredSphereTypes.includes(type) ? 0.65 : 0;
  return getUpgradeSourceWeight(s, 'sphere') * (1 + levelPressure + activeBuildPressure + characterAffinity);
}

function getModifierUpgradeChoiceWeight(s: GameState, modifier: keyof SphereMods): number {
  return getUpgradeSourceWeight(s, 'modifier') * (
    1 + (CHARACTER_DEFS[s.player.characterId]?.preferredSphereMods.includes(modifier) ? 0.55 : 0)
  );
}

function getAbilityUpgradeChoiceWeight(s: GameState, choice: UpgradeChoice): number {
  if (!choice.ability) return getUpgradeSourceWeight(s, 'ability');
  const unfinishedPressure = choice.currentLevel === 0 ? 1.35 : 1.15;
  const characterAffinity = CHARACTER_DEFS[s.player.characterId]?.preferredAbilities.includes(choice.ability) ? 0.75 : 0;
  const activeAffinity = ABILITIES[choice.ability].category === 'active' ? 0.08 : 0;
  return getUpgradeSourceWeight(s, 'ability') * (unfinishedPressure + characterAffinity + activeAffinity);
}

export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {
  const sphereTypes = (Object.keys(SPHERE_PROGRESSION) as SphereType[]).filter((type) => type in SPHERE_TYPES);
  const availableSpheres = sphereTypes.filter((type) => sphereLevel(s, type) < 7);

  const sphereChoices: UpgradeChoice[] = availableSpheres.map((type) => {
    const currentLevel = sphereLevel(s, type);
    const nextLevel = currentLevel + 1;
    const def = SPHERE_PROGRESSION[type];
    const levelDef = def.levels[nextLevel - 1];
    const branchId = s.player.sphereBranches[type];
    const branch = branchId ? def.evolution4Choices.find((x) => x.id === branchId) : null;
    const branchProgress = (nextLevel === 5 || nextLevel === 6) && !!branch;
    const isFirstMutation = nextLevel === 4;
    const isFinalMutation = nextLevel === 7;

    let nameRu = def.name.ru + ' — уровень ' + nextLevel;
    let nameEn = def.name.en + ' — level ' + nextLevel;
    let descRu = levelDef.desc.ru;
    let descEn = levelDef.desc.en;

    if (isFirstMutation) {
      nameRu += ': Мутация I';
      nameEn += ': Mutation I';
      descRu = 'Повышает сферу до IV уровня. После выбора откроется отдельное окно с 3 мутациями, из которых можно выбрать одну.';
      descEn = 'Raises the sphere to level IV. After this choice, a separate window opens with 3 mutations and you choose one.';
    } else if (isFinalMutation) {
      nameRu += ': Мутация II';
      nameEn += ': Mutation II';
      descRu = 'Повышает сферу до VII уровня. После этого откроется отдельное окно с 3 финальными специализациями.';
      descEn = 'Raises the sphere to level VII. After this choice, a separate window opens with 3 final specializations.';
    } else if (branchProgress) {
      descRu = nextLevel === 5 ? branch!.level5.ru : branch!.level6.ru;
      descEn = nextLevel === 5 ? branch!.level5.en : branch!.level6.en;
      nameRu += ': ветка «' + branch!.name.ru + '»';
      nameEn += ': branch “' + branch!.name.en + '”';
    }

    return {
      type: 'sphere' as const,
      sphereType: type,
      sphereBranch: branchId,
      sphereStage: 'upgrade' as const,
      currentLevel,
      newLevel: nextLevel,
      name: { ru: nameRu, en: nameEn },
      desc: { ru: descRu, en: descEn },
    };
  });

  const modifierChoices: UpgradeChoice[] = SPHERE_MODIFIER_CHOICES
    .map((modifier) => ({
      type: 'modifier' as const,
      modifier: modifier.id,
      currentLevel: 0,
      newLevel: 1,
      name: modifier.name,
      desc: modifier.desc,
    }))
    .filter((choice) => isLiveUpgradeChoice(s, choice));

  // Abilities are real Level-Up choices. The old first-slice gate left the
  // 21-definition Ability system effectively unreachable during a normal run.
  // Dash remains free and does not consume these slots.
  const activeCount = Object.keys(s.activeKeyMap || {}).length;
  const activePool = (Object.keys(ABILITIES) as AbilityType[])
    .filter((id) => ABILITIES[id].category === 'active')
    .filter((id) => (s.player.abilities[id] || 0) < ABILITIES[id].maxLevel)
    .filter((id) => (s.player.abilities[id] || 0) > 0 || activeCount < s.player.activeAbilitySlots)
    .map((id) => ({
      type: 'ability' as const,
      ability: id,
      currentLevel: s.player.abilities[id] || 0,
      newLevel: Math.min(ABILITIES[id].maxLevel, (s.player.abilities[id] || 0) + 1),
      name: ABILITIES[id].name,
      desc: {
        ru: ABILITY_PROGRESSION[id]?.levels[(s.player.abilities[id] || 0)]?.desc.ru || ABILITIES[id].desc.ru((s.player.abilities[id] || 0) + 1),
        en: ABILITY_PROGRESSION[id]?.levels[(s.player.abilities[id] || 0)]?.desc.en || ABILITIES[id].desc.en((s.player.abilities[id] || 0) + 1),
      },
    }));

  const passivePool = (Object.keys(ABILITIES) as AbilityType[])
    .filter((id) => ABILITIES[id].category === 'passive')
    .filter((id) => (s.player.abilities[id] || 0) < ABILITIES[id].maxLevel)
    .map((id) => ({
      type: 'ability' as const,
      ability: id,
      currentLevel: s.player.abilities[id] || 0,
      newLevel: Math.min(ABILITIES[id].maxLevel, (s.player.abilities[id] || 0) + 1),
      name: ABILITIES[id].name,
      desc: {
        ru: ABILITY_PROGRESSION[id]?.levels[(s.player.abilities[id] || 0)]?.desc.ru || ABILITIES[id].desc.ru((s.player.abilities[id] || 0) + 1),
        en: ABILITY_PROGRESSION[id]?.levels[(s.player.abilities[id] || 0)]?.desc.en || ABILITIES[id].desc.en((s.player.abilities[id] || 0) + 1),
      },
    }));

  const abilityPool = weightedShuffle(s, [...activePool, ...passivePool].filter((choice) => isLiveUpgradeChoice(s, choice)), (choice) => getAbilityUpgradeChoiceWeight(s, choice));
  const spherePool = weightedShuffle(s, sphereChoices, (choice) => choice.sphereType ? getSphereUpgradeChoiceWeight(s, choice.sphereType) : 1);
  const modifierPool = weightedShuffle(s, modifierChoices, (choice) => choice.modifier ? getModifierUpgradeChoiceWeight(s, choice.modifier) : 1);

  // Preserve source diversity first, then use the seeded weighted pool to fill
  // the remaining slots. This keeps Level-Up choices useful without forcing a
  // specific build.
  const sourcePools = [abilityPool, spherePool, modifierPool];
  const mixedPool: UpgradeChoice[] = [];
  for (const pool of sourcePools) {
    if (pool[0] && mixedPool.length < 3) mixedPool.push(pool[0]);
  }

  const seen = new Set(mixedPool.map((choice) => {
    if (choice.type === 'ability') return 'ability:' + choice.ability;
    if (choice.type === 'sphere') return 'sphere:' + choice.sphereType;
    return 'modifier:' + choice.modifier;
  }));
  const candidates = sourcePools.flatMap((pool) => pool.slice(0, 4));
  for (const choice of weightedShuffle(s, candidates, () => 1)) {
    const key = choice.type === 'ability'
      ? 'ability:' + choice.ability
      : choice.type === 'sphere'
        ? 'sphere:' + choice.sphereType
        : 'modifier:' + choice.modifier;
    if (seen.has(key)) continue;
    seen.add(key);
    mixedPool.push(choice);
    if (mixedPool.length >= 3) break;
  }
  return mixedPool;
}

export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {
  const hadPendingChoice = Boolean(s.pendingUpgrade);
  s.pendingUpgrade=null;
  if (hadPendingChoice) recordLevelUpSourcePick(s, choice);

  if (choice.type === 'modifier' && choice.modifier) {
    if ((s.player.sphereMods[choice.modifier] || 0) > 0) return;
    s.player.sphereMods[choice.modifier] = 1;
    const modifier = SPHERE_MODIFIER_CHOICES.find((item) => item.id === choice.modifier);
    s.flashText = { text: modifier?.name.ru ?? 'Модификатор', life: 1.2, color: '#39d8ff' };
    playSound('place');
    return;
  }

  if(choice.type==='sphere'&&choice.sphereType){
    const type=choice.sphereType;
    const current=sphereLevel(s,type);

    // Mutation choices are a second, separate decision. They do not advance
    // the level again because the normal upgrade already moved the sphere to IV/VII.
    if(choice.sphereStage==='branch'&&choice.sphereBranch){
      if(current!==4) return;
      s.player.sphereBranches[type]=choice.sphereBranch;
      s.player.evolutions.push('sphere:'+type+':4:'+choice.sphereBranch);
      s.evolutionsThisRun++;
      s.flashText={text:choice.name?.ru??'Мутация сферы I',life:2.2,color:'#d4943d'};
      playSound('evolve');
      return;
    }

    if(choice.sphereStage==='final'){
      if(current!==7) return;
      s.player.evolutions.push('sphere:'+type+':7:'+(choice.sphereBranch??'unknown')+':'+(choice.sphereFinalIndex??0));
      s.evolutionsThisRun++;
      s.flashText={text:choice.name?.ru??'Мутация сферы II',life:2.2,color:'#c4453d'};
      playSound('evolve');
      return;
    }

    if(current>=7) return;
    const next=current+1;
    s.player.sphereProgression[type]=next;
    for(const sphere of s.spheres) if(sphere.type===type) sphere.visualTier=next;

    if(next===4){
      s.pendingUpgrade=getSphereEvolutionChoices(s,type,4);
    } else if(next===7){
      s.pendingUpgrade=getSphereEvolutionChoices(s,type,7);
    }
    return;
  }

  if(choice.type==='ability'&&choice.ability){
    const ability=choice.ability;
    const current=s.player.abilities[ability]||0;

    if(choice.abilityStage==='branch'){
      if(current!==4) return;
      const progression=ABILITY_PROGRESSION[ability];
      const evolution=progression?.evolution4[Math.max(0,Math.min((progression?.evolution4.length||1)-1,choice.abilityEvolutionIndex??0))];
      if(evolution){
        const marker='ability:'+ability+':4:'+evolution.id;
        s.player.evolutions.push(marker);
        s.evolutionsThisRun++;
        s.flashText={text:evolution.name.ru,life:2.2,color:'#d4943d'};
        playSound('evolve');
      }
      return;
    }

    if(choice.abilityStage==='final'){
      if(current!==7) return;
      const progression=ABILITY_PROGRESSION[ability];
      const evolution=progression?.evolution7[Math.max(0,Math.min((progression?.evolution7.length||1)-1,choice.abilityEvolutionIndex??0))];
      if(evolution){
        const marker='ability:'+ability+':7:'+evolution.id;
        s.player.evolutions.push(marker);
        s.evolutionsThisRun++;
        s.flashText={text:evolution.name.ru,life:2.2,color:'#c4453d'};
        playSound('evolve');
      }
      return;
    }

    if(current>=7) return;
    const next=Math.min(7,current+1);
    s.player.abilities[ability]=next;
    const def=ABILITIES[ability];
    if (def.category === 'active' && current === 0) assignHotkey(s, ability);
    if (ability === 'vitality') {
      const hpGain = 20;
      s.player.maxHp += hpGain;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + hpGain);
    }

    if(next===4){
      s.pendingUpgrade=getAbilityEvolutionChoices(s,ability,4);
    } else if(next===7){
      s.pendingUpgrade=getAbilityEvolutionChoices(s,ability,7);
    }
    return;
  }
}

export function applySphereUpgrade(s: GameState, choice: SphereUpgradeChoice): void {
  const type = (choice as SphereUpgradeChoice & { sphereType?: SphereType }).sphereType;
  if (!type) return;
  applyUpgrade(s, { type: 'sphere', sphereType: type, sphereStage: 'upgrade', sphereBranch: s.player.sphereBranches[type], currentLevel: sphereLevel(s, type), newLevel: sphereLevel(s, type) + 1 });
}