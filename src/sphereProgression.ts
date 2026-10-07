import type { CharacterId } from './characters.ts';
import { getSphereArtifactModifiers } from './artifactSystem.ts';
import { ABILITIES, SPHERE_TYPES, type SphereType, type AbilityType, sphereUsesProjectileModifiers } from './gameData.ts';
import type { SphereMods } from './engineTypes.ts';

export type SphereEvolutionId = 'standard_resonator' | 'standard_singularity' | 'standard_swarm' | 'sniper_oracle' | 'sniper_assassin' | 'sniper_beacon' | 'shotgun_burst' | 'shotgun_cataclysm' | 'shotgun_hail' | 'chain_web' | 'chain_storm' | 'chain_leech' | 'aura_sanctum' | 'aura_gravity' | 'aura_overgrowth' | 'orbital_dance' | 'orbital_halo' | 'orbital_blade' | 'prism_split' | 'prism_spectrum' | 'prism_mirror' | 'gravity_well' | 'gravity_tide' | 'gravity_collapse' | 'pulse_wave' | 'pulse_resonator' | 'pulse_burst' | 'void_hunger' | 'void_reaper' | 'void_execution';

export type SphereElement = 'fire' | 'freeze' | 'poison';

export const ELEMENTAL_SPHERE_TYPES: readonly SphereType[] = [
  'standard', 'sniper', 'shotgun', 'orbital', 'prism', 'void',
];

export interface SphereElementMeta {
  color: string;
  icon: string;
  name: { ru: string; en: string };
}

export const SPHERE_ELEMENT_META: Record<SphereElement, SphereElementMeta> = {
  fire: { color: '#ff743d', icon: '🔥', name: { ru: 'Огонь', en: 'Fire' } },
  freeze: { color: '#d9f6ff', icon: '❄️', name: { ru: 'Заморозка', en: 'Freeze' } },
  poison: { color: '#72f08e', icon: '☠️', name: { ru: 'Яд', en: 'Poison' } },
};

export const SPHERE_BRANCH_ELEMENTS: Partial<Record<SphereEvolutionId, SphereElement>> = {
  standard_resonator: 'fire',
  standard_singularity: 'freeze',
  standard_swarm: 'poison',
  sniper_oracle: 'poison',
  sniper_assassin: 'fire',
  sniper_beacon: 'freeze',
  shotgun_burst: 'fire',
  shotgun_cataclysm: 'poison',
  shotgun_hail: 'freeze',
  orbital_dance: 'poison',
  orbital_halo: 'freeze',
  orbital_blade: 'fire',
  prism_split: 'poison',
  prism_spectrum: 'fire',
  prism_mirror: 'freeze',
  void_hunger: 'poison',
  void_reaper: 'fire',
  void_execution: 'freeze',

  // Special elemental archetypes use the same Fire / Freeze / Poison
  // vocabulary, but resolve it through Conduction or Field reactions.
  chain_web: 'freeze',
  chain_storm: 'fire',
  chain_leech: 'poison',
  aura_sanctum: 'freeze',
  aura_gravity: 'fire',
  aura_overgrowth: 'poison',
  gravity_well: 'freeze',
  gravity_tide: 'fire',
  gravity_collapse: 'poison',
  pulse_wave: 'freeze',
  pulse_resonator: 'fire',
  pulse_burst: 'poison',
};

export interface SphereElementMasteryDef {
  id: string;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const SPHERE_ELEMENT_MASTERY: Record<SphereElement, readonly SphereElementMasteryDef[]> = {
  fire: [
    { id: 'fire_power', name: { ru: 'Раскалённое пламя', en: 'Incineration' }, desc: { ru: '+35% урона горения.', en: '+35% burn damage.' } },
    { id: 'fire_tempo', name: { ru: 'Быстрое горение', en: 'Rapid Combustion' }, desc: { ru: 'Горение наносит свой урон более частыми тиками.', en: 'Burn deals its damage in more frequent ticks.' } },
    { id: 'fire_duration', name: { ru: 'Неугасаемый огонь', en: 'Endless Flame' }, desc: { ru: '+45% длительности горения.', en: '+45% burn duration.' } },
  ],
  freeze: [
    { id: 'freeze_duration', name: { ru: 'Глубокий холод', en: 'Deep Frost' }, desc: { ru: '+40% длительности заморозки.', en: '+40% freeze duration.' } },
    { id: 'freeze_impact', name: { ru: 'Холодный удар', en: 'Cryo Impact' }, desc: { ru: 'Замороженные цели получают +22% урона от той же сферы.', en: 'Frozen targets take +22% damage from the same Sphere.' } },
    { id: 'freeze_permafrost', name: { ru: 'Вечная мерзлота', en: 'Permafrost' }, desc: { ru: 'После разморозки цель остаётся замедленной ещё 1 с.', en: 'After thawing, the target remains slowed for 1s.' } },
  ],
  poison: [
    { id: 'poison_power', name: { ru: 'Токсичность', en: 'Toxicity' }, desc: { ru: '+35% урона яда.', en: '+35% poison damage.' } },
    { id: 'poison_tempo', name: { ru: 'Быстрый токсин', en: 'Rapid Toxin' }, desc: { ru: 'Яд наносит свой урон более частыми тиками.', en: 'Poison deals its damage in more frequent ticks.' } },
    { id: 'poison_duration', name: { ru: 'Долгий токсин', en: 'Lingering Venom' }, desc: { ru: '+45% длительности яда.', en: '+45% poison duration.' } },
  ],
};


export const SPHERE_CHAIN_ELEMENT_MASTERY: Record<SphereElement, readonly SphereElementMasteryDef[]> = {
  fire: [
    { id: 'conduction_power', name: { ru: 'Проводимость • Сила', en: 'Conduction • Power' }, desc: { ru: '+35% силы огненной реакции цепи.', en: '+35% Fire chain-reaction power.' } },
    { id: 'conduction_rate', name: { ru: 'Проводимость • Частота', en: 'Conduction • Rate' }, desc: { ru: 'Огненная реакция требует на один переход меньше и восстанавливается быстрее.', en: 'Fire reaction needs one fewer jump and recovers faster.' } },
    { id: 'conduction_duration', name: { ru: 'Проводимость • Длительность', en: 'Conduction • Duration' }, desc: { ru: '+45% длительности горения от реакции цепи.', en: '+45% burn duration from the Chain reaction.' } },
  ],
  freeze: [
    { id: 'conduction_power', name: { ru: 'Проводимость • Сила', en: 'Conduction • Power' }, desc: { ru: '+35% силы холодовой реакции цепи.', en: '+35% Freeze chain-reaction power.' } },
    { id: 'conduction_rate', name: { ru: 'Проводимость • Частота', en: 'Conduction • Rate' }, desc: { ru: 'Холодовая реакция требует на один переход меньше и восстанавливается быстрее.', en: 'Freeze reaction needs one fewer jump and recovers faster.' } },
    { id: 'conduction_duration', name: { ru: 'Проводимость • Длительность', en: 'Conduction • Duration' }, desc: { ru: '+45% длительности заморозки от реакции цепи.', en: '+45% freeze duration from the Chain reaction.' } },
  ],
  poison: [
    { id: 'conduction_power', name: { ru: 'Проводимость • Сила', en: 'Conduction • Power' }, desc: { ru: '+35% силы токсичной реакции цепи.', en: '+35% Poison chain-reaction power.' } },
    { id: 'conduction_rate', name: { ru: 'Проводимость • Частота', en: 'Conduction • Rate' }, desc: { ru: 'Токсичная реакция требует на один переход меньше и восстанавливается быстрее.', en: 'Poison reaction needs one fewer jump and recovers faster.' } },
    { id: 'conduction_duration', name: { ru: 'Проводимость • Длительность', en: 'Conduction • Duration' }, desc: { ru: '+45% длительности яда от реакции цепи.', en: '+45% poison duration from the Chain reaction.' } },
  ],
};

export const SPHERE_FIELD_ELEMENT_MASTERY: Record<SphereElement, readonly SphereElementMasteryDef[]> = {
  fire: [
    { id: 'field_power', name: { ru: 'Поле • Сила', en: 'Field • Power' }, desc: { ru: '+35% урона горения от элементной реакции поля.', en: '+35% burn damage from the Field reaction.' } },
    { id: 'field_frequency', name: { ru: 'Поле • Частота', en: 'Field • Frequency' }, desc: { ru: 'Элементная реакция поля срабатывает по цели чаще.', en: 'Field elemental reactions recover faster per target.' } },
    { id: 'field_duration', name: { ru: 'Поле • Длительность', en: 'Field • Duration' }, desc: { ru: '+45% длительности горения от элементной реакции поля.', en: '+45% burn duration from the Field reaction.' } },
  ],
  freeze: [
    { id: 'field_power', name: { ru: 'Поле • Сила', en: 'Field • Power' }, desc: { ru: '+35% длительности холодовой реакции поля.', en: '+35% Freeze reaction duration.' } },
    { id: 'field_frequency', name: { ru: 'Поле • Частота', en: 'Field • Frequency' }, desc: { ru: 'Холодовая реакция поля срабатывает по цели чаще.', en: 'Field Freeze reactions recover faster per target.' } },
    { id: 'field_duration', name: { ru: 'Поле • Длительность', en: 'Field • Duration' }, desc: { ru: '+45% длительности холодовой реакции поля.', en: '+45% Freeze reaction duration.' } },
  ],
  poison: [
    { id: 'field_power', name: { ru: 'Поле • Сила', en: 'Field • Power' }, desc: { ru: '+35% урона яда от элементной реакции поля.', en: '+35% poison damage from the Field reaction.' } },
    { id: 'field_frequency', name: { ru: 'Поле • Частота', en: 'Field • Frequency' }, desc: { ru: 'Токсичная реакция поля срабатывает по цели чаще.', en: 'Field Poison reactions recover faster per target.' } },
    { id: 'field_duration', name: { ru: 'Поле • Длительность', en: 'Field • Duration' }, desc: { ru: '+45% длительности яда от элементной реакции поля.', en: '+45% poison duration from the Field reaction.' } },
  ],
};

export function getSphereElementForBranch(branch?: SphereEvolutionId | null): SphereElement | null {
  return branch ? (SPHERE_BRANCH_ELEMENTS[branch] ?? null) : null;
}

export function getSphereElementMasteryForBranch(
  branch?: SphereEvolutionId | null,
  finalIndex?: number | null,
): SphereElementMasteryDef | null {
  if (finalIndex === null || finalIndex === undefined) return null;
  let element = getSphereElementForBranch(branch);
  if (branch === 'prism_spectrum') {
    element = finalIndex === 1 ? 'freeze' : finalIndex === 2 ? 'poison' : 'fire';
  }
  if (!element) return null;
  const specialChain = Boolean(branch && branch.startsWith('chain_'));
  const specialField = Boolean(branch && (
    branch.startsWith('aura_') ||
    branch.startsWith('gravity_') ||
    branch.startsWith('pulse_')
  ));
  if (specialChain) return SPHERE_CHAIN_ELEMENT_MASTERY[element][finalIndex] ?? null;
  if (specialField) return SPHERE_FIELD_ELEMENT_MASTERY[element][finalIndex] ?? null;
  return SPHERE_ELEMENT_MASTERY[element][finalIndex] ?? null;
}
export type AbilityEvolutionId = string;
export interface SphereUpgradeDef { level:number; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionDef { id:string; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionBranch extends SphereEvolutionDef {
  id:SphereEvolutionId;
  final:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef];
  level5:{ru:string;en:string};
  level6:{ru:string;en:string};
}
export interface SphereDef { type:SphereType; name:{ru:string;en:string}; priority:Partial<Record<CharacterId,number>>; levels:SphereUpgradeDef[]; evolution4:SphereEvolutionDef; evolution7:SphereEvolutionDef; evolution4Choices:SphereEvolutionBranch[]; }
const SPHERE_LEVEL_EN:Record<string,string>={
  "+15% урона": "+15% damage",
  "+1 пробитие": "+1 pierce",
  "-10% задержки": "-10% attack delay",
  "+25% урона": "+25% damage",
  "+15% дальности": "+15% range",
  "+15% крита": "+15% crit chance",
  "+1 дробь": "+1 pellet",
  "+20% урона вблизи": "+20% close-range damage",
  "-12% разброса": "-12% spread",
  "+1 цель цепи": "+1 chain target",
  "+10% урона цепи": "+10% chain damage",
  "-15% интервала атаки": "-15% attack interval",
  "+20% радиуса ауры": "+20% Aura radius",
  "-10% интервала импульса": "-10% pulse interval",
  "+10% урона ауры": "+10% Aura damage",
  "+15% орбитального урона": "+15% orbital damage",
  "+15% радиуса орбиты": "+15% orbit radius",
  "+15% скорости вращения боевых элементов": "+15% combat element rotation speed",
  "+20% урона луча": "+20% beam damage",
  "+1 направление": "+1 beam direction",
  "+20% силы притяжения": "+20% pull strength",
  "+15% радиуса": "+15% radius",
  "-15% интервала импульса": "-15% pulse interval",
  "+20% импульсного урона": "+20% pulse damage",
  "+20% урона по ослабленным": "+20% damage to weakened targets",
  "+10% шанс критического добивания": "+10% execution crit chance"
};
const sphereLevelEn=(text:string):string=>SPHERE_LEVEL_EN[text] ?? text;

const lv=(a:string,b:string,c:string)=>[{level:1,name:{ru:'Ядро',en:'Core'},desc:{ru:a,en:sphereLevelEn(a)}},{level:2,name:{ru:'Механизм',en:'Mechanism'},desc:{ru:b,en:sphereLevelEn(b)}},{level:3,name:{ru:'Настройка',en:'Tuning'},desc:{ru:c,en:sphereLevelEn(c)}},{level:4,name:{ru:'Эволюция I',en:'Evolution I'},desc:{ru:'Выбор одной из трёх веток',en:'Choose one of three branches'}},{level:5,name:{ru:'Контур',en:'Circuit'},desc:{ru:'Усиление выбранной ветки',en:'Strengthens the selected branch'}},{level:6,name:{ru:'Стабилизатор',en:'Stabilizer'},desc:{ru:'Усиление специальной механики',en:'Strengthens the special mechanic'}},{level:7,name:{ru:'Эволюция II',en:'Evolution II'},desc:{ru:'Финальная специализация',en:'Final specialization'}}];
const e=(id:string,ru:string,en:string,descRu:string,descEn:string):SphereEvolutionDef=>({id,name:{ru,en},desc:{ru:descRu,en:descEn}});
const BRANCH_LEVEL_DETAILS:Partial<Record<SphereEvolutionId,{level5:string;level6:string}>>={
  standard_resonator:{level5:'Каждое 3-е попадание создаёт импульс вокруг цели. На V уровне импульс становится частью основного цикла атак.',level6:'Импульс срабатывает стабильнее: после каждого третьего попадания сфера усиливает массовое поражение перед финальной формой.'},
  standard_singularity:{level5:'Каждое попадание замедляет врага и начинает стягивать ближайших противников к точке удара.',level6:'Стягивание становится сильнее: сфера лучше собирает группу врагов в одной зоне для последующих попаданий.'},
  standard_swarm:{level5:'Осколок получает повышенный урон, усиливая каждое срабатывание Роя.',level6:'Каждое попадание выпускает два боковых осколка, расширяя постоянный рисунок атаки.'},
  sniper_oracle:{level5:'По отмеченной цели срабатывает усиленный выстрел, увеличивая урон по приоритетной цели.',level6:'Усиленный выстрел по Метке получает ещё один шаг мощности и наносит ещё больше урона приоритетной цели.'},
  sniper_assassin:{level5:'Враги ниже 35% HP получают резко повышенный урон, чтобы снайперская сфера добивала ослабленные цели.',level6:'Добивающий урон ещё сильнее наказывает цели с низким запасом здоровья, подготавливая финальную форму.'},
  sniper_beacon:{level5:'Попадание создаёт вокруг цели зону Метки и замедляет ближайших врагов.',level6:'Зона Метки становится заметнее и охватывает больше целей, усиливая контроль пространства.'},
  shotgun_burst:{level5:'Бонус ближнего боя действует на более широкой дистанции и усиливается.',level6:'Зона усиленного ближнего боя расширяется ещё сильнее, позволяя стабильно получать повышенный урон дальше от сферы.'},
  shotgun_cataclysm:{level5:'Попадание вызывает взрыв по соседним врагам и частично пробивает толпу.',level6:'Взрыв получает дополнительную мощность и лучше работает против плотных групп противников.'},
  shotgun_hail:{level5:'Град срабатывает чаще и накрывает более широкую область вокруг цели.',level6:'Град срабатывает ещё чаще и расширяет область вторичного поражения, усиливая давление по группе.'},
  chain_web:{level5:'Цепь замедляет поражённых врагов, превращая серию перескоков в зону контроля.',level6:'Замедление усиливается, поэтому одна цепная атака дольше удерживает группу врагов в сети.'},
  chain_storm:{level5:'Каждый переход цепи создаёт дополнительный электрический всплеск по ближайшим врагам.',level6:'Всплеск становится мощнее и добавляет ещё больше урона всей цепной последовательности.'},
  chain_leech:{level5:'Каждое попадание цепи возвращает часть нанесённого урона в HP игрока.',level6:'Лечение от цепи увеличивается, превращая длительную серию попаданий в устойчивый источник восстановления.'},
  aura_sanctum:{level5:'Импульс ауры сильно замедляет врагов внутри области и удерживает их возле сферы.',level6:'Контроль становится надёжнее: враги дольше остаются внутри ауры под действием замедления.'},
  aura_gravity:{level5:'Аура периодически притягивает врагов к центру, собирая толпу для массового урона.',level6:'Гравитация действует сильнее и на большей зоне, плотнее стягивая врагов к центру.'},
  aura_overgrowth:{level5:'Сферы рядом с Аурой получают ускорение атак и чаще выпускают свои снаряды.',level6:'Зона усиления расширяется, позволяя большему числу сфер одновременно пользоваться ускорением.'
  },
};
const BRANCH_LEVEL_DETAILS_EN:Partial<Record<SphereEvolutionId,{level5:string;level6:string}>>={
  standard_resonator:{level5:"Every 3rd hit creates a pulse around the target. At level V the pulse becomes part of the main attack cycle.",level6:"The pulse triggers more consistently: every third hit strengthens area damage before the final form."},
  standard_singularity:{level5:"Each hit slows the enemy and begins pulling nearby enemies toward the impact point.",level6:"The pull becomes stronger, grouping enemies more effectively for follow-up hits."},
  standard_swarm:{level5:"Side-shard damage increases, strengthening each Swarm trigger.",level6:"Every hit launches two side shards, expanding the permanent attack pattern."},
  sniper_oracle:{level5:"Marked targets trigger an empowered shot, increasing damage against the priority target.",level6:"The empowered Mark shot gains another power step and deals more damage to the priority target."},
  sniper_assassin:{level5:"Enemies below 35% HP take sharply increased damage, allowing the Sniper Sphere to finish weakened targets.",level6:"Execution damage punishes low-health targets even harder, preparing the final form."},
  sniper_beacon:{level5:"A hit creates a Mark zone around the target and slows nearby enemies.",level6:"The Mark zone becomes more pronounced and covers more targets, strengthening area control."},
  shotgun_burst:{level5:"The close-range bonus works over a wider distance and becomes stronger.",level6:"The enhanced close-range zone expands again, keeping the damage bonus effective farther from the Sphere."},
  shotgun_cataclysm:{level5:"A hit causes an explosion against nearby enemies and partially pierces the crowd.",level6:"The explosion gains additional power and works better against dense enemy groups."},
  shotgun_hail:{level5:"Hail triggers more often and covers a wider area around the target.",level6:"Hail triggers even more often and expands its secondary damage area, increasing group pressure."},
  chain_web:{level5:"The Chain slows affected enemies, turning repeated jumps into a control zone.",level6:"The slow becomes stronger, so one Chain attack holds enemies in the network longer."},
  chain_storm:{level5:"Every Chain jump creates an additional electrical surge against nearby enemies.",level6:"The surge becomes stronger and adds more damage to the whole Chain sequence."},
  chain_leech:{level5:"Each Chain hit returns part of its dealt damage as player HP.",level6:"Chain healing increases, turning long hit sequences into a sustained recovery source."},
  aura_sanctum:{level5:"The Aura pulse heavily slows enemies inside the area and keeps them near the Sphere.",level6:"Control becomes more reliable: enemies remain inside the Aura longer under the slow."},
  aura_gravity:{level5:"The Aura periodically pulls enemies toward its center, gathering the crowd for area damage.",level6:"Gravity acts more strongly and over a larger area, compressing enemies toward the center."},
  aura_overgrowth:{level5:"Spheres near the Aura gain attack acceleration and fire their attacks more often.",level6:"The empowerment zone expands, allowing more Spheres to use the acceleration at once."},
  orbital_dance:{level5:"Orbits accelerate and cross enemies more often.",level6:"Rotation speed increases again, so the orbital elements complete more combat passes per second."},
  orbital_halo:{level5:"Each successful orbital pass grants additional Resonance.",level6:"Successful orbital passes grant stronger Resonance charge."},
  orbital_blade:{level5:"Orbital blade damage is further increased on every trajectory.",level6:"Blade damage rises again, Afterimage trails become stronger, and the effective pass zone becomes wider."},
  prism_split:{level5:"Split beams retain more power after the beam divides.",level6:"Split beams retain even more power and stay effective against additional targets."},
  prism_spectrum:{level5:"The beam transfers an active status effect to the hit target.",level6:"Status transfer becomes more consistent and interacts with reactions more strongly."},
  prism_mirror:{level5:"Reflected beams gain greater range for their follow-up ricochets.",level6:"Reflected beams gain a second ricochet, extending the reflection chain through the network."},
  gravity_well:{level5:"The pull zone becomes denser and slows enemies more strongly.",level6:"Pull strengthens toward the center and holds dense groups more effectively."},
  gravity_tide:{level5:"Gravity alternates between pull and a counter-push phase.",level6:"The phases become stronger and expand spatial control."},
  gravity_collapse:{level5:"Tightly grouped enemies take additional collapse damage.",level6:"Collapse punishes large clusters and weakened targets more strongly."},
  pulse_wave:{level5:"Each wave gains increased radius and pushes enemies away from the node.",level6:"The wave gains a wider control radius and stronger knockback."},
  pulse_resonator:{level5:"The pulse additionally feeds Resonance when compatible active geometry is present: Triangle, Lattice, or Ring.",level6:"In those geometries the linked pulse creates resonance surges more often."},
  pulse_burst:{level5:"The second discharge becomes stronger while keeping the Burst double-wave pattern.",level6:"The second discharge becomes even stronger and punishes dense groups more effectively."},
  void_hunger:{level5:"Damage increases in proportion to the target's lost health.",level6:"Weakened targets receive an even higher finishing multiplier."},
  void_reaper:{level5:"A Void kill restores a small amount of HP and creates Void shards.",level6:"Shards gain increased damage and more often sustain the kill chain."},
  void_execution:{level5:"The execution threshold rises, making weak targets the priority.",level6:"The execution threshold rises further, and finishing damage against bosses increases."},
};
const NEW_BRANCH_LEVEL_DETAILS:Partial<Record<SphereEvolutionId,{level5:string;level6:string}>>={
  orbital_dance:{level5:'Орбиты ускоряются и чаще пересекают врагов.',level6:'Скорость вращения снова увеличивается, поэтому боевые элементы совершают больше проходов по траектории за секунду.'},
  orbital_halo:{level5:'Каждый успешный проход орбиты дополнительно подпитывает Resonance.',level6:'Резонансный заряд от успешных проходов становится сильнее.'},
  orbital_blade:{level5:'Урон орбитальных лезвий дополнительно усиливается на каждой траектории.',level6:'Урон лезвий усиливается ещё сильнее, Afterimage-следы становятся мощнее, а эффективная зона прохода шире.'},
  prism_split:{level5:'Разделённые лучи сохраняют больше мощности после расщепления.',level6:'Разделённые лучи сохраняют ещё больше мощности и стабильнее проходят по дополнительным целям.'},
  prism_spectrum:{level5:'Луч переносит активный статусный эффект на поражённую цель.',level6:'Статусная передача становится стабильнее и сильнее взаимодействует с реакциями.'},
  prism_mirror:{level5:'Отражённый луч получает большую дальность последующих рикошетов.',level6:'Отражённый луч получает второй рикошет, продлевая цепочку отражения по сети.'},
  gravity_well:{level5:'Зона притяжения становится плотнее и сильнее замедляет врагов.',level6:'Стягивание усиливается к центру и лучше удерживает плотные группы.'},
  gravity_tide:{level5:'Гравитация чередует фазы стягивания и обратного толчка.',level6:'Фазы становятся мощнее и расширяют контроль пространства.'},
  gravity_collapse:{level5:'Плотно собранные враги получают дополнительный урон от коллапса.',level6:'Коллапс сильнее наказывает большие скопления и ослабленные цели.'},
  pulse_wave:{level5:'Каждая волна получает увеличенный радиус и отбрасывает врагов от узла.',level6:'Волна получает больший радиус контроля и более сильное отбрасывание.'},
  pulse_resonator:{level5:'Импульс дополнительно подпитывает Resonance при активной совместимой геометрии: Треугольник, Решётка или Кольцо.',level6:'В этих геометриях связанный импульс создаёт резонансные всплески чаще.'},
  pulse_burst:{level5:'Второй разряд становится мощнее, сохраняя двойную волну Burst.',level6:'Второй разряд становится ещё мощнее и сильнее наказывает плотные группы.'},
  void_hunger:{level5:'Урон растёт пропорционально потерянному здоровью цели.',level6:'Ослабленные цели получают ещё более высокий множитель добивания.'},
  void_reaper:{level5:'Убийство Void возвращает немного HP и создаёт осколки пустоты.',level6:'Осколки получают повышенный урон и чаще поддерживают цепь убийств.'},
  void_execution:{level5:'Порог исполнения повышается, превращая слабые цели в приоритет.',level6:'Порог исполнения растёт ещё сильнее, а добивание наносит больше урона боссам.'},
};

const BRANCH_EN:Partial<Record<SphereEvolutionId,[string,string]>>={
  standard_resonator:['Standard Resonator','Every third hit releases a pulse, while hits also feed Resonance.'],
  standard_singularity:['Standard Singularity','Develops the Standard Singularity branch and its signature behavior.'],
  standard_swarm:[
    f('standard_swarm_final_1','Рой • Охотничий осколок','Standard Swarm • Hunter Shard','После попадания выпускает один осколок, который ищет ближайшую другую цель.','After a hit, one shard seeks the nearest different target.'),
    f('standard_swarm_final_2','Рой • Перекрёстный огонь','Standard Swarm • Crossfire','После попадания создаёт два боковых осколка по расходящимся траекториям.','After a hit, creates two side shards on diverging trajectories.'),
    f('standard_swarm_final_3','Рой • Осколочная сеть','Standard Swarm • Shard Web','После попадания создаёт три осколка, расходящихся веером и насыщащих зону вокруг цели.','After a hit, creates three fan-shaped shards that saturate the area around the target.'),
  ],
  standard_singularity:[
    f('standard_singularity_final_1','Сингулярность • Сжатие','Standard Singularity • Compression','Попадание сильнее стягивает ближайших врагов и удерживает цель замедленной на 1.2 с.','Hits pull nearby enemies harder and keep the target slowed for 1.2s.'),
    f('standard_singularity_final_2','Сингулярность • Стазис','Standard Singularity • Stasis','Попадание удерживает цель замедленной дольше, до 1.8 с, усиливая контроль плотной группы.','Hits keep the target slowed for longer, up to 1.8s, strengthening control of dense groups.'),
    f('standard_singularity_final_3','Сингулярность • Коллапс','Standard Singularity • Collapse','Попадание сильнее стягивает врагов; цели ниже 50% HP получают +15% урона.','Hits pull enemies harder, and targets below 50% HP take 15% more damage.'),
  ],
  standard_swarm:[
    f('standard_swarm_final_1','Рой • Осколочный залп','Standard Swarm • Shard Volley','При попадании с вероятностью 35% выпускается 1 осколок, наносящий 50% урона исходного попадания.','On hit, there is a 35% chance to launch 1 shard dealing 50% of the original hit damage.'),
    f('standard_swarm_final_2','Рой • Шрапнель','Standard Swarm • Shrapnel','При попадании с вероятностью 55% выпускается 1 осколок, наносящий 50% урона исходного попадания.','On hit, there is a 55% chance to launch 1 shard dealing 50% of the original hit damage.'),
    f('standard_swarm_final_3','Рой • Рой осколков','Standard Swarm • Shard Swarm','Каждое попадание выпускает 2 осколка, каждый наносит 50% урона исходного попадания.','Every hit launches 2 shards, each dealing 50% of the original hit damage.'),
  ],
  sniper_oracle:[
    f('sniper_oracle_final_1','Оракул • Критический фокус','Sniper Oracle • Critical Focus','Крит по отмеченной цели наносит +50% урона.','Critical hits against marked targets deal 50% more damage.'),
    f('sniper_oracle_final_2','Оракул • Резонансный прицел','Sniper Oracle • Resonant Sight','Крит по отмеченной цели наносит +30% урона, а каждое попадание по отмеченной цели дополнительно заряжает Resonance.','Critical hits against marked targets deal 30% more damage, and every hit against the marked target grants additional Resonance.'),
    f('sniper_oracle_final_3','Оракул • Взрывной крит','Sniper Oracle • Explosive Crit','Крит по отмеченной цели наносит +22% урона, дополнительно задевает врагов рядом с целью и получает усиление Execute по целям ниже 30% HP.','Critical hits against marked targets deal 22% more damage, also damage nearby enemies, and gain stronger Execute pressure below 30% HP.'),
  ],
  sniper_assassin:[
    f('sniper_assassin_final_1','Убийца • Добивание','Sniper Assassin • Execution','По цели ниже 35% HP урон увеличен примерно в 1.7 раза.','Targets below 35% HP take roughly 1.7x damage.'),
    f('sniper_assassin_final_2','Убийца • Смертельный приговор','Sniper Assassin • Death Sentence','По цели ниже 35% HP урон увеличен примерно в 2.2 раза.','Targets below 35% HP take roughly 2.2x damage.'),
    f('sniper_assassin_final_3','Убийца • Кровавое добивание','Sniper Assassin • Blood Execution','По цели ниже 35% HP урон увеличен примерно в 1.45 раза; попадание восстанавливает небольшое количество HP.','Targets below 35% HP take roughly 1.45x damage, and the hit restores a small amount of HP.'),
  ],
  sniper_beacon:[
    f('sniper_beacon_final_1','Маяк • Якорный','Sniper Beacon • Anchor','Помеченная цель замедляется сильнее, а зона контроля вокруг неё получает усиливающийся эффект Anchor.','Marked targets are slowed, and the area around them gains a stable network anchor.'),
    f('sniper_beacon_final_2','Маяк • Прожектор','Sniper Beacon • Spotlight','Метка держится дольше, распространяет контроль на более широкую область и усиливает урон маркировки.','The mark lasts longer, affects a wider area, and strengthens the marking damage effect.'),
    f('sniper_beacon_final_3','Маяк • Сетевой','Sniper Beacon • Network Beacon','Метка и Anchor работают вместе, распространяя замедление на область вокруг цели.','The mark and network anchor work together, spreading slow across the area around the target.'),
  ],
  shotgun_burst:[
    f('shotgun_burst_final_1','Разрыв • Ближний','Shotgun Burst • Close Burst','При дистанции до 150 наносится +30% урона и атака получает ещё 2 дополнительных снаряда.','At up to 150 range, the hit deals 30% more damage and gains 2 additional projectiles.'),
    f('shotgun_burst_final_2','Разрыв • Осадный','Shotgun Burst • Siege Burst','При дистанции до 180 наносится +50% урона.','At up to 180 range, the hit deals 50% more damage.'),
    f('shotgun_burst_final_3','Разрыв • Ударный','Shotgun Burst • Impact Burst','При дистанции до 150 наносится +22% урона, атака получает ещё 2 дополнительных снаряда, а цели ближе 90 дополнительно замедляются.','At up to 150 range, the hit deals 22% more damage, gains 2 additional projectiles, and slows targets within 90 range.'),
  ],
  shotgun_cataclysm:[
    f('shotgun_cataclysm_final_1','Осада • Осколочный взрыв','Shotgun Cataclysm • Fragment Blast','Попадание создаёт взрыв радиусом 60, наносящий соседним врагам 45% урона; взрыв получает Shatter.','Hits create a blast with radius 60, dealing 45% of the hit damage to nearby enemies; the blast gains Shatter.'),
    f('shotgun_cataclysm_final_2','Осада • Тяжёлый взрыв','Shotgun Cataclysm • Heavy Blast','Попадание создаёт большой взрыв радиусом 85, наносящий соседним врагам 65% урона и получает Impact.','Hits create a large blast with radius 85, dealing 65% of the hit damage to nearby enemies and gains Impact.'),
    f('shotgun_cataclysm_final_3','Осада • Удерживающий взрыв','Shotgun Cataclysm • Lockdown Blast','Попадание создаёт взрыв радиусом 55 с 35% вторичного урона, Shatter/Impact и замедляет поражённых врагов.','Hits create a blast with radius 55, dealing 35% secondary damage, Shatter/Impact, and slowing affected enemies.'),
  ],
  shotgun_hail:[
    f('shotgun_hail_final_1','Град • Осколочный','Shotgun Hail • Fragment Hail','С вероятностью 25% попадание выпускает 6 осколков вокруг цели.','On hit, there is a 25% chance to launch 6 shards around the target.'),
    f('shotgun_hail_final_2','Град • Плотный','Shotgun Hail • Dense Hail','С вероятностью 40% попадание выпускает 8 осколков, а атака получает ещё 2 дополнительных снаряда и сильнее насыщает область.','On hit, there is a 40% chance to launch 8 shards and saturate the area more heavily.'),
    f('shotgun_hail_final_3','Град • Комбинированный','Shotgun Hail • Combined Hail','С вероятностью 32% выпускает 6 осколков, атака получает ещё 2 дополнительных снаряда, а основное попадание усиливается на 8%.','On hit, there is a 32% chance to launch 6 shards and increase the main hit damage by 8%.'),
  ],
  chain_web:[
    f('chain_web_final_1','Паутина • Якорная','Chain Web • Anchor Web','Цепь усиливает замедление цели, а Anchor дополнительно удерживает её в зоне контроля.','Chain hits apply stronger slow, while the network anchor stabilizes the control zone.'),
    f('chain_web_final_2','Паутина • Статическая','Chain Web • Static Web','Цепь дольше удерживает замедление и получает шанс дополнительно перекинуть статический разряд на соседнюю цель.','Chain hits keep slow active longer and can also relay a static strike to a nearby target.'),
    f('chain_web_final_3','Паутина • Парализующая','Chain Web • Paralysis Web','Anchor и статический разряд работают вместе, превращая цепь в плотную зону контроля.','The anchor and static strike work together, turning the chain into a dense control zone.'),
  ],
  chain_storm:[
    f('chain_storm_final_1','Шторм • Дуговой','Chain Storm • Arc Storm','Каждый переход может дать дополнительный удар по области радиусом 70.','Each chain transition can add an extra area hit with radius 70.'),
    f('chain_storm_final_2','Шторм • Резонансный','Chain Storm • Resonant Storm','Дополнительный удар работает в радиусе 100, а переходы дополнительно заряжают Resonance.','Extra hits use radius 100, and transitions also grant additional Resonance.'),
    f('chain_storm_final_3','Шторм • Перегруженный','Chain Storm • Overloaded Storm','Дополнительные удары и Resonance работают вместе, а финальный удар серии наносит ещё +12% урона.','Extra hits and Resonance work together, and the final hit of the sequence deals 12% more damage.'),
  ],
  chain_leech:[
    f('chain_leech_final_1','Паразит • Кровоток','Chain Leech • Bloodflow','Каждое попадание возвращает часть нанесённого урона в HP.','Each hit returns part of the damage dealt as HP.'),
    f('chain_leech_final_2','Паразит • Сбор урожая','Chain Leech • Harvest','Убийства цепной сферой возвращают HP.','Kills with the Chain Sphere restore HP.'),
    f('chain_leech_final_3','Паразит • Хищный цикл','Chain Leech • Predator Cycle','Попадания лечат, убийства лечат сильнее, а цели ниже 40% HP получают дополнительный урон.','Hits heal, kills heal more, and targets below 40% HP take additional damage.'),
  ],
  aura_sanctum:[
    f('aura_sanctum_final_1','Святилище • Холод','Aura Sanctum • Cold Sanctum','Аура дольше и сильнее замедляет врагов в зоне.','The Aura slows enemies in its zone for longer and with greater strength.'),
    f('aura_sanctum_final_2','Святилище • Удержание','Aura Sanctum • Locking Sanctum','Аура получает более сильный Anchor и удерживает врагов до 2 с.','The Aura gains a stronger network anchor and can hold enemies for up to 2s.'),
    f('aura_sanctum_final_3','Святилище • Контроль','Aura Sanctum • Control Sanctum','Freeze и Anchor работают вместе, а сама аура получает +12% урона.','Slow and anchor work together, and the Aura gains 12% damage.'),
  ],
  aura_gravity:[
    f('aura_gravity_final_1','Гравитация • Тяга','Aura Gravity • Pull','Аура стягивает врагов к центру с усиленной силой притяжения.','The Aura pulls enemies toward its center with increased force.'),
    f('aura_gravity_final_2','Гравитация • Колодец','Aura Gravity • Well','Аура получает ещё более сильную тягу и расширяет область удержания.','The Aura gains even stronger pull and expands its control area.'),
    f('aura_gravity_final_3','Гравитация • Удар','Aura Gravity • Impact','Сильная тяга и Anchor объединяются; аура получает +18% урона.','Strong pull and network anchor combine, and the Aura gains 18% damage.'),
  ],
  aura_overgrowth:[
    f('aura_overgrowth_final_1','Живая сеть • Ускоритель','Living Network • Accelerator','Сферы внутри ауры получают постоянное ускорение следующего цикла атак.','Spheres inside the Aura gain persistent acceleration for their next attack cycle.'),
    f('aura_overgrowth_final_2','Живая сеть • Перелив','Living Network • Overflow','Когда сфера внутри ауры атакует, ускорение передаётся следующей ближайшей сфере в сети.','When a Sphere inside the Aura attacks, its acceleration transfers to the next nearest Sphere in the network.'),
    f('aura_overgrowth_final_3','Живая сеть • Синхрон','Living Network • Synchrony','Все сферы внутри ауры одновременно сокращают задержку и выпускают следующую атаку почти синхронно.','All Spheres inside the Aura reduce delay together and fire their next attack in near-synchrony.'),
  ],
  orbital_dance:[
    f('orbital_dance_final_1','Танец • Ритм','Orbital Dance • Rhythm','Орбитальные спутники вращаются заметно быстрее и Afterimage создаёт повторный след-удар.','Orbital satellites rotate faster and gain an Afterimage effect that creates a repeating trail strike.'),
    f('orbital_dance_final_2','Танец • Гиперцикл','Orbital Dance • Hypercycle','Спутники вращаются в 1.55 раза быстрее базового темпа ветки, шире ловят врагов на траектории и усиливают Afterimage.','Satellites rotate at 1.55x the branch base tempo, have a wider hit window along the path, and strengthen Afterimage.'),
    f('orbital_dance_final_3','Танец • Рой','Orbital Dance • Swarm','К орбитальной атаке добавляется ещё один спутник, а успешные попадания получают усиление Afterimage.','The orbital attack gains one more satellite, while its trails gain an Afterimage effect.'),
  ],
  orbital_halo:[
    f('orbital_halo_final_1','Ореол • Резонанс','Orbital Halo • Resonance','Проход спутника дополнительно заряжает Resonance.','Passing satellites grant additional Resonance charge.'),
    f('orbital_halo_final_2','Ореол • Расширение','Orbital Halo • Expansion','Окно попадания спутников расширяется, делая орбитальный проход стабильнее.','The satellite hit window becomes wider, making orbital passes more reliable.'),
    f('orbital_halo_final_3','Ореол • Защита','Orbital Halo • Guard','Резонанс заряжается сильнее, а успешный орбитальный проход восстанавливает заряд щита.','Resonance charges faster, and a successful orbital pass restores a shield charge.'),
  ],
  orbital_blade:[
    f('orbital_blade_final_1','Клинок • Удар','Orbital Blade • Impact','Боевые лезвия получают усиление Impact и сильнее бьют по траектории.','Combat blades gain an Impact effect and hit harder along their path.'),
    f('orbital_blade_final_2','Клинок • Эхо-след','Orbital Blade • Afterimage','След лезвия получает эффект Afterimage, усиливая повторные атаки орбитали.','Blade trails gain an Afterimage effect, strengthening repeated orbital attacks.'),
    f('orbital_blade_final_3','Клинок • Разрез сети','Orbital Blade • Network Cut','Impact и Afterimage работают вместе, а урон лезвий возрастает ещё сильнее.','Impact and Afterimage work together, and blade damage increases further.'),
  ],
  prism_split:[
    f('prism_split_final_1','Расщепление • Веер','Prism Split • Fan','Луч создаёт веер дополнительных направлений, покрывая несколько целей одновременно.','The beam creates a fan of additional directions to cover several targets at once.'),
    f('prism_split_final_2','Расщепление • Пробой','Prism Split • Pierce','После расщепления основной луч сохраняет энергию и пробивает первую цель насквозь.','After splitting, the main beam keeps its energy and pierces through the first target.'),
    f('prism_split_final_3','Расщепление • Отражение','Prism Split • Ricochet','После расщепления лучи рикошетят и могут сменить цель после первого попадания.','After splitting, the beams ricochet and can retarget after the first hit.'),
  ],
  prism_spectrum:[
    f('prism_spectrum_final_1','Спектр • Пламя','Prism Spectrum • Flame','Призма получает огненный статус для боевых реакций.','Prism gains a Fire status for combat reactions.'),
    f('prism_spectrum_final_2','Спектр • Холод','Prism Spectrum • Frost','Призма получает замораживающий статус для боевых реакций.','Prism gains a Freeze status for combat reactions.'),
    f('prism_spectrum_final_3','Спектр • Токсин','Prism Spectrum • Toxin','Призма получает токсический статус для боевых реакций.','Prism gains a Poison status for combat reactions.'),
  ],
  prism_mirror:[
    f('prism_mirror_final_1','Зеркало • Отражение','Prism Mirror • Reflection','Отражённый луч получает первый уровень Ricochet и переходит на новую ближайшую цель.','The reflected beam gains level 1 Ricochet and jumps to a new nearby target.'),
    f('prism_mirror_final_2','Зеркало • Двойной отскок','Prism Mirror • Double Ricochet','Зеркало получает усиленный Ricochet для повторных ударов по связанной сети.','The mirror gains stronger Ricochet for repeated hits through the linked network.'),
    f('prism_mirror_final_3','Зеркало • Зеркальная сеть','Prism Mirror • Mirror Network','Усиленный Ricochet, дополнительный Echo-эффект и до двух отражений по связанной сети.','Stronger Ricochet combines with an additional Echo effect and up to two linked reflections.'),
  ],
  gravity_well:[
    f('gravity_well_final_1','Колодец • Якорь','Gravity Well • Anchor','Сфера получает сильный сетевой якорь, удерживающий врагов в зоне притяжения.','The Sphere gains a strong network anchor that holds enemies inside the pull zone.'),
    f('gravity_well_final_2','Колодец • Усиление тяги','Gravity Well • Pull Boost','Сила притяжения дополнительно усиливается, увеличивая контроль центра.','Pull strength is increased further, improving control of the center.'),
    f('gravity_well_final_3','Колодец • Полный колодец','Gravity Well • Full Well','Якорь и усиленная тяга работают вместе, превращая центр в стабильную контрольную точку.','Anchor and stronger pull work together, turning the center into a stable control point.'),
  ],
  gravity_tide:[
    f('gravity_tide_final_1','Прилив • Обратный толчок','Gravity Tide • Recoil','Каждый импульс резко отбрасывает врагов от Гравитационной сферы.','Each pulse strongly knocks enemies away from the Gravity Sphere.'),
    f('gravity_tide_final_2','Прилив • Стягивание','Gravity Tide • Draw','Каждый импульс притягивает врагов к Гравитационной сфере и уплотняет группу.','Each pulse pulls enemies toward the Gravity Sphere and compresses the group.'),
    f('gravity_tide_final_3','Прилив • Маятник','Gravity Tide • Pendulum','Импульсы чередуют притяжение и отбрасывание, формируя ритм контроля пространства.','Pulses alternate between pull and knockback, creating a spatial-control rhythm.'),
  ],
  gravity_collapse:[
    f('gravity_collapse_final_1','Коллапс • Экзекуция','Gravity Collapse • Execution','Собранные враги получают более сильное добивание, когда их HP уже снижено.','Grouped enemies are easier to execute when their HP is already reduced.'),
    f('gravity_collapse_final_2','Коллапс • Сжатие','Gravity Collapse • Compression','Гравитационная тяга дополнительно усиливает плотность группы перед коллапсом.','Gravitational pull further increases group density before the collapse.'),
    f('gravity_collapse_final_3','Коллапс • Нулевой импульс','Gravity Collapse • Zero Pulse','Экзекуция и сжатие работают вместе; плотная группа дополнительно получает импульсный взрыв.','Execution and compression work together, and dense groups also trigger a pulse blast.'),
  ],
  pulse_wave:[
    f('pulse_wave_final_1','Волна • Отбой','Pulse Wave • Rebound','Каждый импульс сильнее отбрасывает врагов от сферы.','Each pulse knocks enemies away more strongly.'),
    f('pulse_wave_final_2','Волна • Заморозка','Pulse Wave • Freeze','Импульс получает замораживающий статус и расширяет радиус волны.','The pulse gains a Freeze status and a wider wave radius.'),
    f('pulse_wave_final_3','Волна • Разрушение','Pulse Wave • Shatter','Отбрасывание и заморозка работают вместе, превращая волну в инструмент контроля.','Knockback and Freeze work together, turning the wave into a control tool.'),
  ],
  pulse_resonator:[
    f('pulse_resonator_final_1','Резонатор • Заряд','Pulse Resonator • Charge','Импульсы сильнее заряжают Resonance при активной геометрии.','Pulse attacks generate more Resonance while active geometry is present.'),
    f('pulse_resonator_final_2','Резонатор • Удар','Pulse Resonator • Impact','Повышенный Resonance-синергический эффект дополняется усилением Impact для контроля врагов.','The stronger Resonance interaction is combined with an Impact effect for better control.'),
    f('pulse_resonator_final_3','Резонатор • Раскол','Pulse Resonator • Shatter','Повышенный Resonance-синергический эффект дополняется Shatter для усиления боевого импульса.','The stronger Resonance interaction is combined with Shatter for stronger pulse damage.'),
  ],
  pulse_burst:[
    f('pulse_burst_final_1','Вспышка • Раскол','Pulse Burst • Shatter','Основной импульс получает усиление Shatter.','The main pulse gains a Shatter effect.'),
    f('pulse_burst_final_2','Вспышка • Удар','Pulse Burst • Impact','Основной импульс получает усиление Impact и лучше отбрасывает врагов.','The main pulse gains an Impact effect and knocks enemies away more effectively.'),
    f('pulse_burst_final_3','Вспышка • Двойной разряд','Pulse Burst • Double Pulse','Shatter и Impact работают вместе, а Burst создаёт дополнительную волну.','Shatter and Impact combine, and Burst creates an additional wave.'),
  ],
  void_hunger:[
    f('void_hunger_final_1','Голод • Порча','Void Hunger • Corruption','Пустота получает усиление Corrupt для работы по ослабленным целям.','Void gains a Corrupt effect for working against weakened targets.'),
    f('void_hunger_final_2','Голод • Резонанс','Void Hunger • Resonance','Порча сохраняется, а попадания дополнительно заряжают Resonance.','Corruption remains active, and hits also grant additional Resonance.'),
    f('void_hunger_final_3','Голод • Поглощение','Void Hunger • Devour','Порча сочетается с восстановлением HP от успешных убийств, а попадания по целям ниже 30% HP дополнительно усиливаются на 12%.','Corruption combines with HP restoration on kills, and hits against targets below 30% HP gain an additional 12% multiplier.'),
  ],
  void_reaper:[
    f('void_reaper_final_1','Жнец • Кровь','Void Reaper • Blood','Попадания и убийства сильнее поддерживают восстановление HP.','Hits and kills provide stronger HP sustain.'),
    f('void_reaper_final_2','Жнец • Урожай','Void Reaper • Harvest','Убийства заметно сильнее лечат и поддерживают цикл Пустоты.','Kills restore significantly more HP and sustain the Void loop.'),
    f('void_reaper_final_3','Жнец • Пожирание','Void Reaper • Devouring Reaper','Восстановление от попаданий и убийств объединяется, а добивание дополнительно создаёт больше осколков пустоты.','Hit healing and kill healing combine, and executions create more Void shards.'),
  ],
  void_execution:[
    f('void_execution_final_1','Экзекуция • Порог','Void Execution • Threshold','Шанс исполнения и усиление урона по ослабленным целям повышаются.','Execute chance and damage against weakened targets are increased.'),
    f('void_execution_final_2','Экзекуция • Фаза','Void Execution • Phase','Сфера получает Phase, добавляющий пробитие атакам.','The Sphere gains Phase, adding penetration to its attacks.'),
    f('void_execution_final_3','Экзекуция • Абсолют','Void Execution • Absolute','Execute и Phase объединяются; Phase добавляет пробитие, а порог исполнения повышается до 30% HP.','Execute and Phase combine; Phase adds penetration, and the execution threshold rises to 30% HP.'),
  ],
};

const finalsFor=(id:SphereEvolutionId):[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]=>SPHERE_FINAL_VARIANTS[id];

const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id] ?? NEW_BRANCH_LEVEL_DETAILS[id] ?? { level5: 'Усиление выбранной ветки.', level6: 'Дополнительное усиление уникальной механики.' };
  const detailsEn=BRANCH_LEVEL_DETAILS_EN[id];
  const [nameEn,descEn]=BRANCH_EN[id] ?? [ru,desc];
  return {
    ...e(id,ru,nameEn,desc,descEn),
    id,
    final:fin,
    level5:{ru:details.level5,en:detailsEn?.level5 ?? details.level5},
    level6:{ru:details.level6,en:detailsEn?.level6 ?? details.level6},
  };
};
const sphere=(type:SphereType,name:string,priority:Partial<Record<CharacterId,number>>,l:[string,string,string],branches:[SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch]):SphereDef=>({type,name:SPHERE_TYPES[type]?.name ?? {ru:name,en:name},priority,levels:lv(...l),evolution4:branches[0],evolution7:branches[0].final[0],evolution4Choices:branches});
const genericSphereBranches = (type: SphereType, names: [string,string,string], ids: [SphereEvolutionId,SphereEvolutionId,SphereEvolutionId]): [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch] =>
  ids.map((id, i) => br(id, names[i], 'Развивает уникальную механику сферы '+type+'.', finalsFor(id))) as [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch];

export const SPHERE_PROGRESSION:Record<SphereType,SphereDef>={
 standard:sphere('standard','Стандартная',{spherist:1,engineer:.9,berserker:.8,architect:.7,hunter:.4,alchemist:.4},['+15% урона','+1 пробитие','-10% задержки'],[br('standard_resonator','Резонатор','Каждое третье попадание выпускает импульс; попадания также подпитывают Resonance',finalsFor('standard_resonator')),br('standard_singularity','Сингулярность','Попадания притягивают врагов',finalsFor('standard_singularity')),br('standard_swarm','Рой','Попадания выпускают осколки',finalsFor('standard_swarm'))]),
 sniper:sphere('sniper','Снайперская',{hunter:1,architect:.9,spherist:.4,engineer:.4,berserker:.3,alchemist:.3},['+25% урона','+15% дальности','+15% крита'],[br('sniper_oracle','Оракул','Усиливает критический урон по отмеченным целям',finalsFor('sniper_oracle')),br('sniper_assassin','Убийца','Усиливает урон по слабым целям',finalsFor('sniper_assassin')),br('sniper_beacon','Маяк','Помечает цель для всей сети',finalsFor('sniper_beacon'))]),
 shotgun:sphere('shotgun','Дробовик',{berserker:1,alchemist:.8,spherist:.6,engineer:.4,hunter:.3,architect:.3},['+1 дробь','+20% урона вблизи','-12% разброса'],[br('shotgun_burst','Разрыв','Ближние попадания наносят повышенный урон и получают дополнительную дробь',finalsFor('shotgun_burst')),br('shotgun_cataclysm','Осада','Тяжёлые пробивные снаряды',finalsFor('shotgun_cataclysm')),br('shotgun_hail','Град','Много дополнительных снарядов',finalsFor('shotgun_hail'))]),
 chain:sphere('chain','Цепная',{spherist:1,hunter:.95,engineer:.9,alchemist:.8,architect:.5,berserker:.4},['+1 цель цепи','+10% урона цепи','-15% интервала атаки'],[br('chain_web','Паутина','Поражённые цели получают усиленное замедление',finalsFor('chain_web')),br('chain_storm','Шторм','Каждый переход создаёт электрический всплеск и дополнительно заряжает Resonance',finalsFor('chain_storm')),br('chain_leech','Паразит','Цепь возвращает часть урона',finalsFor('chain_leech'))]),
 aura:sphere('aura','Аура',{engineer:1,alchemist:1,architect:.9,spherist:.7,hunter:.4,berserker:.4},['+20% радиуса ауры','-10% интервала импульса','+10% урона ауры'],[br('aura_sanctum','Святилище','Замедляет врагов и удерживает их, усиливая ближайшие сферы',finalsFor('aura_sanctum')),br('aura_gravity','Гравитация','Стягивает врагов к центру',finalsFor('aura_gravity')),br('aura_overgrowth','Живая сеть','Усиливает сферы внутри ауры',finalsFor('aura_overgrowth'))]),
 orbital:sphere('orbital','Орбитальная',{spherist:1,engineer:.8,architect:.8},['+15% орбитального урона','+15% радиуса орбиты','+15% скорости вращения боевых элементов'],[br('orbital_dance','Танец','Спутники вращаются быстрее, наносят урон при пересечении траектории и оставляют Afterimage-следы.',finalsFor('orbital_dance')),br('orbital_halo','Ореол','Каждый успешный проход орбиты дополнительно заряжает Resonance.',finalsFor('orbital_halo')),br('orbital_blade','Клинок','Спутники превращаются в боевые лезвия, отбрасывают поражённых и могут оставлять Afterimage-следы.',finalsFor('orbital_blade'))]),
 prism:sphere('prism','Призма',{hunter:1,architect:.9,spherist:.7},['+20% урона луча','+15% дальности','+1 направление'],[br('prism_split','Расщепление','Луч после попадания делится на дополнительные лучи по другим целям.',finalsFor('prism_split')),br('prism_spectrum','Спектр','Луч передаёт активный статусный эффект, усиливает реакции и дополнительно заряжает Resonance.',finalsFor('prism_spectrum')),br('prism_mirror','Зеркало','Связанные сферы создают отражённые лучи, повторяющие основной удар.',finalsFor('prism_mirror'))]),
 gravity:sphere('gravity','Гравитационная',{alchemist:1,architect:1,engineer:.8},['+20% силы притяжения','+15% радиуса','-15% интервала импульса'],[br('gravity_well','Колодец','Притяжение становится постоянным: чем ближе враг к центру, тем сильнее его тянет.',finalsFor('gravity_well')),br('gravity_tide','Прилив','Поле плавно меняет силу притяжения и периодически создаёт обратную волну.',finalsFor('gravity_tide')),br('gravity_collapse','Коллапс','Собранные в плотную группу враги получают дополнительный урон от сжатия.',finalsFor('gravity_collapse'))]),
 pulse:sphere('pulse','Импульсная',{engineer:1,spherist:.9,architect:.8},['+20% импульсного урона','+15% радиуса','-12% интервала'],[br('pulse_wave','Волна','Каждый импульс становится шире и отбрасывает врагов от сферы.',finalsFor('pulse_wave')),br('pulse_resonator','Резонатор','Импульсы подпитывают Resonance и усиливают сеть при активной геометрии.',finalsFor('pulse_resonator')),br('pulse_burst','Вспышка','После основной волны возникает дополнительный разряд по центру.',finalsFor('pulse_burst'))]),
 void:sphere('void','Пустотная',{hunter:1,alchemist:.8,architect:.7},['+20% урона по ослабленным','+10% шанс критического добивания','+15% дальности'],[br('void_hunger','Голод','Урон растёт по мере потери здоровья целью; Corrupt дополнительно усиливает последующие попадания.',finalsFor('void_hunger')),br('void_reaper','Жнец','Убийства Void возвращают HP и создают осколки пустоты для продолжения атаки.',finalsFor('void_reaper')),br('void_execution','Экзекуция','Слабые цели получают шанс на мгновенное добивание, а атаки получают Phase-пробитие; финальная форма повышает порог исполнения.',finalsFor('void_execution'))])
};
const ABILITY_LEVEL_EN:Record<string,string>={
  "Импульс проходит через все активные сферы и наносит 30 урона вокруг каждой. Перезарядка: 30 с.": "Pulse travels through all active Spheres and deals 30 damage around each. Cooldown: 30s.",
  "Импульс наносит 40 урона вокруг каждой сферы. Перезарядка: 28 с.": "Pulse deals 40 damage around each Sphere. Cooldown: 28s.",
  "Импульс наносит 50 урона вокруг каждой сферы. Перезарядка: 26 с.": "Pulse deals 50 damage around each Sphere. Cooldown: 26s.",
  "Импульс начинает усиливать выбранную ветку сети.": "Pulse begins empowering the selected network branch.",
  "Передача импульса между сферами становится стабильнее и мощнее.": "Pulse transfer between Spheres becomes more stable and powerful.",
  "Финальная форма превращает импульс в полноценную сетевую атаку.": "The final form turns the pulse into a full network attack.",
  "Сферы создают защитный контур вокруг игрока и поглощают 1 удар. Длительность: 10 с. Перезарядка: 20 с.": "Spheres create a defensive circuit around the player and absorb 1 hit. Duration: 10s. Cooldown: 20s.",
  "Ближайшие сферы добавляют защитные заряды. Перезарядка: 20 с.": "Nearby Spheres add shield charges. Cooldown: 20s.",
  "Сеть создаёт до 2 защитных зарядов. Длительность: 10 с. Перезарядка: 20 с.": "The network creates up to 2 shield charges. Duration: 10s. Cooldown: 20s.",
  "Защита распространяется на ближайшие сферы и выбранную ветку.": "Defense spreads to nearby Spheres and the selected branch.",
  "Защитный контур получает дополнительные заряды и усиливает сеть.": "The defensive circuit gains additional charges and strengthens the network.",
  "Финальная форма превращает защиту в часть сетевого построения.": "The final form turns defense into part of the network formation.",
  "Телепорт перемещает игрока к ближайшей активной сфере. Перезарядка: 15 с.": "Teleport moves the player to the nearest active Sphere. Cooldown: 15s.",
  "Прыжок к сфере происходит чаще. Перезарядка: 13 с.": "Jumping to a Sphere happens more often. Cooldown: 13s.",
  "Прыжок работает на более дальнюю сферу сети. Перезарядка: 11 с.": "The jump can target a farther Sphere in the network. Cooldown: 11s.",
  "Телепорт начинает использовать сферу как точку назначения выбранной ветки.": "Teleport begins using the Sphere as the selected branch's destination point.",
  "Переход между узлами создаёт дополнительный сетевой эффект.": "Moving between nodes creates an additional network effect.",
  "Финальная форма превращает перемещение между сферами в часть боевой системы.": "The final form turns movement between Spheres into part of the combat system.",
  "Перегружает сферы с огненным эффектом на 5 с: +20% урона. Перезарядка: 25 с.": "Overheats fire-aligned Spheres for 5s: +20% damage. Cooldown: 25s.",
  "Перегрев даёт +25% урона и ускоряет атаки огненных сфер. Перезарядка: 25 с.": "Overheat grants +25% damage and speeds up fire-aligned attacks. Cooldown: 25s.",
  "Перегрев даёт +30% урона и ещё сильнее ускоряет огненные сферы. Перезарядка: 25 с.": "Overheat grants +30% damage and speeds up fire-aligned Spheres even more. Cooldown: 25s.",
  "Перегрев начинает взаимодействовать со статусами и выбранной веткой.": "Overheat begins interacting with statuses and the selected branch.",
  "Перегрев передаётся через сеть и усиливает реакции.": "Overheat is transmitted through the network and strengthens reactions.",
  "Финальная форма превращает перегрев в сетевую механику.": "The final form turns Overheat into a network mechanic.",
  "Создаёт 1 Echo Drone на 10 с, который подключается к ближайшей сфере. Перезарядка: 30 с.": "Creates 1 Echo Drone for 10s that connects to the nearest Sphere. Cooldown: 30s.",
  "Дрон связывает соседние сферы и наносит 8 урона врагам. Перезарядка: 30 с.": "The drone links neighboring Spheres and deals 8 damage to enemies. Cooldown: 30s.",
  "Создаёт 2 Echo Drone на 10 с. Дроны становятся дополнительными узлами сети. Перезарядка: 30 с.": "Creates 2 Echo Drones for 10s. Drones become additional network nodes. Cooldown: 30s.",
  "Дроны начинают передавать импульсы между ближайшими сферами.": "Drones begin transferring pulses between nearby Spheres.",
  "Дроны чаще соединяют узлы и ускоряют их следующий выстрел.": "Drones connect nodes more often and accelerate their next shot.",
  "Финальная форма превращает дроны в полноценные временные узлы сети.": "The final form turns drones into full temporary network nodes.",
  "Разряд проходит от игрока через Chain-сферы к 1 цели и наносит 55 урона. Перезарядка: 20 с.": "The discharge travels from the player through Chain Spheres to 1 target and deals 55 damage. Cooldown: 20s.",
  "Разряд наносит 70 урона и усиливается на каждом узле Chain. Перезарядка: 20 с.": "The discharge deals 70 damage and grows stronger at each Chain node. Cooldown: 20s.",
  "Разряд поражает до 2 целей, проходя через сеть Chain. Урон: 85 каждой. Перезарядка: 20 с.": "The discharge hits up to 2 targets through the Chain network. Damage: 85 each. Cooldown: 20s.",
  "Каждая Chain-сфера становится проводником разряда.": "Each Chain Sphere becomes a conductor for the discharge.",
  "Переходы между узлами усиливают следующий разряд.": "Transitions between nodes strengthen the next discharge.",
  "Финальная форма превращает всю Chain-сеть в последовательный разряд.": "The final form turns the entire Chain network into a sequential discharge.",
  "Сеть останавливает врагов вокруг ближайшей сферы на 3 с. Перезарядка: 40 с.": "The network stops enemies around the nearest Sphere for 3s. Cooldown: 40s.",
  "Сеть удерживает остановку 4 с. Перезарядка: 40 с.": "The network holds the stop for 4s. Cooldown: 40s.",
  "Сеть удерживает остановку 5 с и продолжает атаковать через сферы. Перезарядка: 40 с.": "The network holds the stop for 5s and continues attacking through Spheres. Cooldown: 40s.",
  "Во время остановки сферы продолжают атаковать.": "Spheres continue attacking during the stop.",
  "Остановка распространяется между узлами сети и усиливает контроль.": "The stop spreads between network nodes and strengthens control.",
  "Финальная форма связывает длительность остановки с активной сетью.": "The final form links stop duration to the active network.",
  "Жертвуете 20% максимального HP и перегружаете всю сеть на 5 с. Сферы получают +29% урона. Перезарядка: 30 с.": "You sacrifice 20% max HP and overload the entire network for 5s. Spheres gain +29% damage. Cooldown: 30s.",
  "Жертвуете 20% максимального HP. Перегрузка даёт +33% урона и ускоряет сферы. Перезарядка: 30 с.": "You sacrifice 20% max HP. Overload grants +33% damage and speeds up Spheres. Cooldown: 30s.",
  "Жертвуете 20% максимального HP. Перегрузка даёт +37% урона и ускоряет сеть сильнее. Перезарядка: 30 с.": "You sacrifice 20% max HP. Overload grants +37% damage and speeds up the network further. Cooldown: 30s.",
  "Потеря HP становится топливом для выбранной ветки и ближайших сфер.": "Lost HP becomes fuel for the selected branch and nearby Spheres.",
  "Перегрузка глубже взаимодействует с сетью и усиливает риск/награду.": "Overload interacts more deeply with the network and strengthens risk/reward.",
  "Финальная форма превращает HP в ресурс управления мощностью сети.": "The final form turns HP into a resource for controlling network power."
};
const abilityLevelEn=(text:string):string=>ABILITY_LEVEL_EN[text] ?? text;

const abilityLevels=(a:string,b:string,c:string,d:string,e:string,f:string):SphereUpgradeDef[] => [
  {level:1,name:{ru:'Пробуждение',en:'Awakening'},desc:{ru:a,en:abilityLevelEn(a)}},
  {level:2,name:{ru:'Настройка',en:'Tuning'},desc:{ru:b,en:abilityLevelEn(b)}},
  {level:3,name:{ru:'Раскрытие',en:'Expansion'},desc:{ru:c,en:abilityLevelEn(c)}},
  {level:4,name:{ru:'Мутация I',en:'Mutation I'},desc:{ru:'Следующий выбор откроет одну из трёх веток способности',en:'The next choice opens one of three ability branches'}},
  {level:5,name:{ru:'Развитие ветки',en:'Branch Development'},desc:{ru:d,en:abilityLevelEn(d)}},
  {level:6,name:{ru:'Синхронизация ветки',en:'Branch Synchronization'},desc:{ru:e,en:abilityLevelEn(e)}},
  {level:7,name:{ru:'Мутация II',en:'Mutation II'},desc:{ru:'Следующий выбор откроет одну из трёх финальных форм',en:'The next choice opens one of three final forms'}},
];

export interface AbilityEvolutionChoice {
  id:string;
  name:{ru:string;en:string};
  desc:{ru:string;en:string};
}

export interface AbilityProgressionDef {
  ability:AbilityType;
  levels:SphereUpgradeDef[];
  evolution4:AbilityEvolutionChoice[];
  evolution7:AbilityEvolutionChoice[];
  /** Final choices are contextualized by the Mutation I branch. The three runtime final archetypes remain stable, while card names/descriptions explicitly continue the chosen branch. */
  evolution7ByBranch?:Record<string, AbilityEvolutionChoice[]>;
}

const ABILITY_EVOLUTION_EN:Record<string,{name:string;desc:string}>={
  blast_resonance:{name:"Resonance Pulse",desc:"After passing through a Standard Sphere, it releases an additional pulse."},
  blast_network:{name:"Network Pulse",desc:"The pulse travels sequentially through the nearest linked Spheres."},
  blast_core:{name:"Explosion Core",desc:"The network center gets an empowered pulse and extra damage to nearby enemies."},
  blast_echo_network:{name:"Echo Network",desc:"The wave travels along the Sphere chain and grows stronger at each visited node."},
  blast_resonant_core:{name:"Resonant Core",desc:"Each Standard Sphere adds an additional resonance pulse to the wave."},
  blast_infinite_pulse:{name:"Infinite Pulse",desc:"The last node creates a reverse pulse that sends the wave back through the network."},
  shield_echo_guard:{name:"Echo Barrier",desc:"Every two nearby Spheres add one charge to the network shield."},
  shield_reflector:{name:"Reflective Circuit",desc:"A blocked hit is partially reflected through the nearest Sphere."},
  shield_bastion:{name:"Bastion",desc:"Spheres around the player form a unified defensive circuit."},
  shield_network_guard:{name:"Network Bastion",desc:"Linked Spheres increase the shield charge capacity."},
  shield_iron_dome:{name:"Iron Dome",desc:"The number of nearby Spheres increases the shield's additional charge capacity."},
  shield_resonant_guard:{name:"Resonant Guard",desc:"A blocked hit restores one shield charge, keeping the defense in resonance."},
  teleport_echo_jump:{name:"Echo Jump",desc:"The jump uses the nearest Sphere as the destination."},
  teleport_beacon:{name:"Beacon Jump",desc:"Jumping to a Sphere empowers the next attack."},
  teleport_phase:{name:"Phase Jump",desc:"After the jump, the player gains a brief invulnerability window."},
  teleport_spatial_network:{name:"Spatial Network",desc:"Teleport connects the origin and destination nodes with a visual network relay."},
  teleport_hunter_beacon:{name:"Hunter Beacon",desc:"Jumping to a Sniper Sphere empowers the next attack."},
  teleport_phase_break:{name:"Phase Break",desc:"Spheres between the jump points create additional pulses."},
  firetrail_overdrive:{name:"Overheat",desc:"Fire-aligned Spheres gain additional attack speed and damage."},
  firetrail_ignition:{name:"Igniter",desc:"Overheat instantly ignites enemies that already have another status."},
  firetrail_sanctum:{name:"Burning Sanctuary",desc:"Overheat instantly empowers an Aura Sphere and its next pulse."},
  firetrail_network:{name:"Thermal Network",desc:"Overheat is transmitted between linked nodes."},
  firetrail_catalyst:{name:"Catalyst Flame",desc:"A status reaction empowers the next fire pulse."},
  firetrail_inferno:{name:"Network Inferno",desc:"Each newly overheated node strengthens the previous ones."},
  minion_echo_drone:{name:"Echo Drone",desc:"A temporary additional Sphere connects to the nearest network node."},
  minion_relay_drone:{name:"Relay Drone",desc:"The drone regularly transfers a pulse from one Sphere to another."},
  minion_guardian:{name:"Guardian Drone",desc:"The drone stays near the nearest Sphere and accelerates its next cycle."},
  minion_echo_swarm:{name:"Echo Swarm",desc:"Drones form additional network nodes."},
  minion_network_nodes:{name:"Network Nodes",desc:"Each drone links the two nearest Spheres."},
  minion_sphere_guard:{name:"Sphere Guardians",desc:"Drones empower the Spheres they protect."},
  lightning_echo_storm:{name:"Echo Storm",desc:"The discharge travels through every Chain Sphere before striking the target."},
  lightning_relay:{name:"Relay Discharge",desc:"The discharge jumps between linked Spheres."},
  lightning_overload:{name:"Overload",desc:"The last discharge in the sequence deals additional damage."},
  lightning_storm_network:{name:"Storm Network",desc:"The network creates sequential discharges between nodes."},
  lightning_thunder_chain:{name:"Thunder Network",desc:"Each Chain Sphere adds an additional jump."},
  lightning_overload_core:{name:"Overload Core",desc:"A complete network cycle ends with a powerful discharge."},
  timestop_echo_phase:{name:"Phase Break",desc:"Spheres continue attacking during the time stop."},
  timestop_closed_time:{name:"Closed Time",desc:"The stop zone expands through the network."},
  timestop_time_anchor:{name:"Time Anchor",desc:"The stop anchors enemies around the nearest Sphere."},
  timestop_outside_time:{name:"Outside Time",desc:"Sphere hits refresh enemy freeze during the stop."},
  timestop_closed_network:{name:"Closed Time Network",desc:"The network preserves the time-stop effect between nodes."},
  timestop_temporal_core:{name:"Temporal Core",desc:"The final second of the stop doubles the active network's strength."},
  darkritual_blood_link:{name:"Blood Link",desc:"Part of the overload is transferred to the nearest Standard Sphere."},
  darkritual_sacrifice:{name:"Sacrificial Circle",desc:"Lost HP creates an additional pulse around the nearest Sphere."},
  darkritual_void_pact:{name:"Void Pact",desc:"The lower the HP, the longer the network overload lasts."},
  darkritual_blood_network:{name:"Blood Network",desc:"The overload transfers charge between linked Standard Spheres."},
  darkritual_sacrifice_core:{name:"Sacrifice Core",desc:"HP sacrifice creates an empowered pulse around the nearest Sphere."},
  darkritual_void_engine:{name:"Void Engine",desc:"At low HP, the overload lasts longer and increases the damage of the entire network."},
};

const ae=(id:string,ru:string,desc:string):AbilityEvolutionChoice=>{ const en=ABILITY_EVOLUTION_EN[id]; return {id,name:{ru,en:en?.name ?? ru},desc:{ru:desc,en:en?.desc ?? desc}}; };

export const ABILITY_PROGRESSION:Partial<Record<AbilityType,AbilityProgressionDef>>={
  blast:{
    ability:'blast',
    levels:abilityLevels(
      'Импульс проходит через все активные сферы и наносит 30 урона вокруг каждой. Перезарядка: 30 с.',
      'Импульс наносит 40 урона вокруг каждой сферы. Перезарядка: 28 с.',
      'Импульс наносит 50 урона вокруг каждой сферы. Перезарядка: 26 с.',
      'Импульс начинает усиливать выбранную ветку сети.',
      'Передача импульса между сферами становится стабильнее и мощнее.',
      'Финальная форма превращает импульс в полноценную сетевую атаку.'
    ),
    evolution4:[
      ae('blast_resonance','Резонансный импульс','После прохождения Standard-сферы она выпускает дополнительный импульс.'),
      ae('blast_network','Сетевой импульс','Импульс последовательно проходит через ближайшие связанные сферы.'),
      ae('blast_core','Ядро взрыва','Центр сети получает усиленный импульс и дополнительный урон по врагам рядом с игроком.')
    ],
    evolution7:[
      ae('blast_echo_network','Echo Network','Волна проходит по цепочке сфер и усиливается на каждом посещённом узле.'),
      ae('blast_resonant_core','Resonant Core','Каждая Standard-сфера добавляет к волне дополнительный резонансный импульс.'),
      ae('blast_infinite_pulse','Infinite Pulse','Последний узел создаёт обратный импульс, возвращающий волну через сеть.')
    ]
  },
  shield:{
    ability:'shield',
    levels:abilityLevels(
      'Сферы создают защитный контур вокруг игрока и поглощают 1 удар. Длительность: 10 с. Перезарядка: 20 с.',
      'Ближайшие сферы добавляют защитные заряды. Перезарядка: 20 с.',
      'Сеть создаёт до 2 защитных зарядов. Длительность: 10 с. Перезарядка: 20 с.',
      'Защита распространяется на ближайшие сферы и выбранную ветку.',
      'Защитный контур получает дополнительные заряды и усиливает сеть.',
      'Финальная форма превращает защиту в часть сетевого построения.'
    ),
    evolution4:[
      ae('shield_echo_guard','Эхо-барьер','Каждые две ближайшие сферы добавляют один заряд защитному щиту сети.'),
      ae('shield_reflector','Отражающий контур','Поглощённый удар частично отражается через ближайшую сферу.'),
      ae('shield_bastion','Бастион','Сферы вокруг игрока образуют единый защитный контур.')
    ],
    evolution7:[
      ae('shield_network_guard','Network Bastion','Связанные сферы увеличивают запас зарядов защитного щита.'),
      ae('shield_iron_dome','Iron Dome','Количество ближайших сфер увеличивает дополнительный запас зарядов щита.'),
      ae('shield_resonant_guard','Resonant Guard','Поглощённый удар восстанавливает один заряд щита, удерживая защиту в резонансе.')
    ]
  },
  teleport:{
    ability:'teleport',
    levels:abilityLevels(
      'Телепорт перемещает игрока к ближайшей активной сфере. Перезарядка: 15 с.',
      'Прыжок к сфере происходит чаще. Перезарядка: 13 с.',
      'Прыжок работает на более дальнюю сферу сети. Перезарядка: 11 с.',
      'Телепорт начинает использовать сферу как точку назначения выбранной ветки.',
      'Переход между узлами создаёт дополнительный сетевой эффект.',
      'Финальная форма превращает перемещение между сферами в часть боевой системы.'
    ),
    evolution4:[
      ae('teleport_echo_jump','Эхо-прыжок','Прыжок использует ближайшую сферу как точку назначения.'),
      ae('teleport_beacon','Скачок-маяк','Прыжок к сфере усиливает следующую атаку.'),
      ae('teleport_phase','Фазовый прыжок','После прыжка игрок получает краткое окно неуязвимости.')
    ],
    evolution7:[
      ae('teleport_spatial_network','Spatial Network','Телепорт соединяет исходный и целевой узлы визуальным сетевым реле.'),
      ae('teleport_hunter_beacon','Hunter Beacon','Прыжок к Sniper-сфере усиливает следующую атаку.'),
      ae('teleport_phase_break','Phase Break','Сферы между точками прыжка создают дополнительные импульсы.')
    ]
  },
  firetrail:{
    ability:'firetrail',
    levels:abilityLevels(
      'Перегружает сферы с огненным эффектом на 5 с: +20% урона. Перезарядка: 25 с.',
      'Перегрев даёт +25% урона и ускоряет атаки огненных сфер. Перезарядка: 25 с.',
      'Перегрев даёт +30% урона и ещё сильнее ускоряет огненные сферы. Перезарядка: 25 с.',
      'Перегрев начинает взаимодействовать со статусами и выбранной веткой.',
      'Перегрев передаётся через сеть и усиливает реакции.',
      'Финальная форма превращает перегрев в сетевую механику.'
    ),
    evolution4:[
      ae('firetrail_overdrive','Перегрев','Огненные сферы получают дополнительное ускорение атак и урона.'),
      ae('firetrail_ignition','Воспламенитель','Перегрев мгновенно поджигает врагов, уже имеющих другой статус.'),
      ae('firetrail_sanctum','Пылающее святилище','Перегрев мгновенно усиливает Aura-сферу и её следующий импульс.')
    ],
    evolution7:[
      ae('firetrail_network','Thermal Network','Перегрев передаётся между связанными узлами.'),
      ae('firetrail_catalyst','Catalyst Flame','Статусная реакция усиливает следующий огненный импульс.'),
      ae('firetrail_inferno','Network Inferno','Каждый новый перегретый узел усиливает предыдущие.')
    ]
  },
  minion:{
    ability:'minion',
    levels:abilityLevels(
      'Создаёт 1 Echo Drone на 10 с, который подключается к ближайшей сфере. Перезарядка: 30 с.',
      'Дрон связывает соседние сферы и наносит 8 урона врагам. Перезарядка: 30 с.',
      'Создаёт 2 Echo Drone на 10 с. Дроны становятся дополнительными узлами сети. Перезарядка: 30 с.',
      'Дроны начинают передавать импульсы между ближайшими сферами.',
      'Дроны чаще соединяют узлы и ускоряют их следующий выстрел.',
      'Финальная форма превращает дроны в полноценные временные узлы сети.'
    ),
    evolution4:[
      ae('minion_echo_drone','Эхо-дрон','Временная дополнительная сфера подключается к ближайшему узлу сети.'),
      ae('minion_relay_drone','Релейный дрон','Дрон регулярно передаёт импульс от одной сферы к другой.'),
      ae('minion_guardian','Дрон-страж','Дрон удерживается возле ближайшей сферы и ускоряет её следующий цикл.')
    ],
    evolution7:[
      ae('minion_echo_swarm','Echo Swarm','Дроны образуют дополнительные узлы сети.'),
      ae('minion_network_nodes','Network Nodes','Каждый дрон связывает две ближайшие сферы.'),
      ae('minion_sphere_guard','Sphere Guardians','Дроны усиливают сферы, которые защищают.')
    ]
  },
  lightning:{
    ability:'lightning',
    levels:abilityLevels(
      'Разряд проходит от игрока через Chain-сферы к 1 цели и наносит 55 урона. Перезарядка: 20 с.',
      'Разряд наносит 70 урона и усиливается на каждом узле Chain. Перезарядка: 20 с.',
      'Разряд поражает до 2 целей, проходя через сеть Chain. Урон: 85 каждой. Перезарядка: 20 с.',
      'Каждая Chain-сфера становится проводником разряда.',
      'Переходы между узлами усиливают следующий разряд.',
      'Финальная форма превращает всю Chain-сеть в последовательный разряд.'
    ),
    evolution4:[
      ae('lightning_echo_storm','Эхо-шторм','Разряд проходит через каждую Chain-сферу перед ударом по цели.'),
      ae('lightning_relay','Релейный разряд','Разряд перескакивает между связанными сферами.'),
      ae('lightning_overload','Перегрузка','Последний разряд серии наносит дополнительный урон.')
    ],
    evolution7:[
      ae('lightning_storm_network','Storm Network','Сеть создаёт последовательные разряды между узлами.'),
      ae('lightning_thunder_chain','Thunder Network','Каждая Chain-сфера добавляет дополнительный переход.'),
      ae('lightning_overload_core','Overload Core','Полный цикл сети заканчивается мощным разрядом.')
    ]
  },
  timestop:{
    ability:'timestop',
    levels:abilityLevels(
      'Сеть останавливает врагов вокруг ближайшей сферы на 3 с. Перезарядка: 40 с.',
      'Сеть удерживает остановку 4 с. Перезарядка: 40 с.',
      'Сеть удерживает остановку 5 с и продолжает атаковать через сферы. Перезарядка: 40 с.',
      'Во время остановки сферы продолжают атаковать.',
      'Остановка распространяется между узлами сети и усиливает контроль.',
      'Финальная форма связывает длительность остановки с активной сетью.'
    ),
    evolution4:[
      ae('timestop_echo_phase','Фазовый залп','При запуске остановки все Сферы получают немедленный залп; во время остановки их урон увеличен на 18%.'),
      ae('timestop_closed_time','Замкнутое время','Зона остановки расширяется через сеть.'),
      ae('timestop_time_anchor','Якорь времени','Остановка фиксирует врагов вокруг ближайшей сферы.')
    ],
    evolution7:[
      ae('timestop_outside_time','Вне времени','Попадания сфер обновляют заморозку врагов во время остановки.'),
      ae('timestop_closed_network','Замкнутая сеть времени','Сеть сохраняет эффект остановки между узлами.'),
      ae('timestop_temporal_core','Temporal Core','Последняя секунда остановки удваивает силу активной сети.')
    ]
  },
  darkritual:{
    ability:'darkritual',
    levels:abilityLevels(
      'Жертвуете 20% максимального HP и перегружаете всю сеть на 5 с. Сферы получают +29% урона. Перезарядка: 30 с.',
      'Жертвуете 20% максимального HP. Перегрузка даёт +33% урона и ускоряет сферы. Перезарядка: 30 с.',
      'Жертвуете 20% максимального HP. Перегрузка даёт +37% урона и ускоряет сеть сильнее. Перезарядка: 30 с.',
      'Потеря HP становится топливом для выбранной ветки и ближайших сфер.',
      'Перегрузка глубже взаимодействует с сетью и усиливает риск/награду.',
      'Финальная форма превращает HP в ресурс управления мощностью сети.'
    ),
    evolution4:[
      ae('darkritual_blood_link','Кровавая связь','Часть перегрузки передаётся ближайшей Standard-сфере.'),
      ae('darkritual_sacrifice','Жертвенный круг','Потерянное HP создаёт дополнительный импульс вокруг ближайшей сферы.'),
      ae('darkritual_void_pact','Договор пустоты','Чем меньше HP, тем дольше длится перегрузка сети.')
    ],
    evolution7:[
      ae('darkritual_blood_network','Blood Network','Перегрузка передаёт заряд между связанными Standard-сферами.'),
      ae('darkritual_sacrifice_core','Sacrifice Core','Жертва HP создаёт усиленный импульс вокруг ближайшей сферы.'),
      ae('darkritual_void_engine','Void Engine','При низком HP перегрузка продлевается и усиливает урон всей сети.')
    ]
  },

};


type AbilityFinalSpec = [id:string, ru:string, en:string, descRu:string, descEn:string];

const ABILITY_FINAL_VARIANT_SPECS: Record<string, Record<string, AbilityFinalSpec[]>> = {
  blast: {
    blast_resonance: [
      ['blast_resonance_f1','Резонанс • Цветение','Resonance • Bloom','Каждый третий импульс создаёт вторичный импульс от последней Standard-сферы, который проходит по ближайшей цели.','Every third pulse creates a secondary pulse from the last Standard Sphere and seeks the nearest target.'],
      ['blast_resonance_f2','Резонанс • Волна','Resonance • Wave','После прохождения Standard импульс расширяется: ближайшие связанные сферы получают короткий дополнительный всплеск.','After passing through Standard, the pulse expands and nearby linked Spheres emit a brief extra surge.'],
      ['blast_resonance_f3','Резонанс • Возврат','Resonance • Return','Финальный узел возвращает часть импульса обратно по активной сети, создавая второй проход с уменьшенной силой.','The final node sends part of the pulse back through the active network for a second pass at reduced strength.'],
    ],
    blast_network: [
      ['blast_network_f1','Сеть • Эстафета','Network • Relay','Волна последовательно активирует связанные сферы, каждая передаёт её следующему узлу.','The wave activates linked Spheres in sequence, each passing it to the next node.'],
      ['blast_network_f2','Сеть • Разрыв','Network • Fracture','Каждый второй посещённый узел создаёт боковой взрыв по врагам вокруг себя.','Every second visited node creates a side blast against enemies around it.'],
      ['blast_network_f3','Сеть • Обратный контур','Network • Backflow','После последнего узла волна меняет направление и возвращается к началу сети.','After the final node, the wave reverses direction and returns to the start of the network.'],
    ],
    blast_core: [
      ['blast_core_f1','Ядро • Детонация','Core • Detonation','Центр сети создаёт отдельную мощную детонацию, если рядом находятся враги.','The network center creates a separate powerful detonation when enemies are nearby.'],
      ['blast_core_f2','Ядро • Сжатие','Core • Compression','Перед взрывом центр кратко стягивает врагов к себе, собирая их в зоне импульса.','Before exploding, the core briefly pulls enemies inward and gathers them into the pulse zone.'],
      ['blast_core_f3','Ядро • Перегрузка','Core • Overdrive','После детонации ближайшие сферы мгновенно сокращают задержку следующей атаки.','After detonation, nearby Spheres instantly reduce the delay before their next attack.'],
    ],
  },
  shield: {
    shield_echo_guard: [
      ['shield_echo_guard_f1','Барьер • Резерв','Barrier • Reserve','Каждая пара ближайших сфер формирует дополнительный резервный заряд щита.','Each pair of nearby Spheres forms an additional reserve shield charge.'],
      ['shield_echo_guard_f2','Барьер • Реле','Barrier • Relay','Поглощённый удар передаёт короткий защитный импульс ближайшим сферам и ускоряет их следующий выстрел.','A blocked hit sends a brief defensive pulse to nearby Spheres and accelerates their next attack.'],
      ['shield_echo_guard_f3','Барьер • Обратный контур','Barrier • Feedback','Когда расходуется последний заряд, ближайшие сферы получают короткий защитный контур до следующего щита.','When the last charge is spent, nearby Spheres gain a brief defensive circuit until the next shield activation.'],
    ],
    shield_reflector: [
      ['shield_reflector_f1','Отражатель • Контрудар','Reflector • Counterstrike','Поглощённый удар направляет отражённый разряд в ближайшую цель с повышенным уроном.','A blocked hit sends a reflected discharge into the nearest enemy for increased damage.'],
      ['shield_reflector_f2','Отражатель • Рикошет','Reflector • Ricochet','Отражённый разряд может перейти от первой поражённой цели к следующей соседней цели.','The reflected discharge can jump from the first target to a second nearby enemy.'],
      ['shield_reflector_f3','Отражатель • Разряд сети','Reflector • Network Discharge','Отражение проходит через ближайшую связанную сферу и затем поражает врагов рядом с ней.','The reflection travels through a nearby linked Sphere before striking enemies around it.'],
    ],
    shield_bastion: [
      ['shield_bastion_f1','Бастион • Стена','Bastion • Wall','Пока щит активен, сферы вокруг игрока образуют защитную стену и снижают входящий урон после исчерпания зарядов.','While the shield is active, nearby Spheres form a defensive wall that reduces incoming damage after charges are exhausted.'],
      ['shield_bastion_f2','Бастион • Контур','Bastion • Circuit','При получении удара все защищающие сферу узлы одновременно создают защитный импульс, отталкивающий врагов.','When hit, all protected Sphere nodes emit a defensive pulse that pushes enemies away.'],
      ['shield_bastion_f3','Бастион • Периметр','Bastion • Perimeter','Каждая сфера в защитном периметре создаёт независимую зону перехвата вокруг игрока.','Each Sphere in the defensive perimeter creates an independent interception zone around the player.'],
    ],
  },
  teleport: {
    teleport_echo_jump: [
      ['teleport_echo_jump_f1','Прыжок • Дальняя дуга','Jump • Long Arc','Телепорт предпочитает ближайшую доступную дальнюю сферу за пределами ближней зоны.','Teleport prefers the nearest available Sphere beyond the close-range zone.'],
      ['teleport_echo_jump_f2','Прыжок • Цепь','Jump • Chain','Телепорт оставляет реле между исходной и целевой сферами, временно связывая их.','Teleport leaves a relay between origin and destination, temporarily linking them.'],
      ['teleport_echo_jump_f3','Прыжок • Возврат','Jump • Return','После прыжка следующий телепорт в течение короткого окна может вернуть игрока к исходному узлу.','For a brief window after the jump, the next teleport can return the player to the origin node.'],
    ],
    teleport_beacon: [
      ['teleport_beacon_f1','Маяк • Метка','Beacon • Mark','Приземление на сферу помечает ближайшего врага и усиливает следующий удар по нему.','Landing at a Sphere marks the nearest enemy and empowers the next hit against it.'],
      ['teleport_beacon_f2','Маяк • Волна','Beacon • Wave','Приземление выпускает импульс вокруг целевой сферы и временно замедляет врагов.','Landing emits a pulse around the destination Sphere and temporarily slows enemies.'],
      ['teleport_beacon_f3','Маяк • Разветвление','Beacon • Split','Приземление у сферы создаёт второй прыжковый ориентир и расширяет выбор следующего узла.','Landing at a Sphere creates a second jump beacon and expands the next destination choice.'],
    ],
    teleport_phase: [
      ['teleport_phase_f1','Фаза • Сквозь сеть','Phase • Throughline','Во время прыжка игрок игнорирует столкновения с врагами и получает короткое окно после приземления.','During the jump, the player ignores enemy collisions and gains a brief post-arrival window.'],
      ['teleport_phase_f2','Фаза • Разрыв','Phase • Rift','Траектория прыжка режет врагов между исходным и целевым узлами.','The jump path cuts through enemies between the origin and destination nodes.'],
      ['teleport_phase_f3','Фаза • Укрытие','Phase • Shelter','После приземления ближайшая сфера создаёт краткую область, в которой игрок неуязвим.','After arrival, the nearest Sphere creates a brief area of invulnerability around the player.'],
    ],
  },
  firetrail: {
    firetrail_overdrive: [
      ['firetrail_overdrive_f1','Перегрев • Вспышка','Overheat • Flash','Перегрев мгновенно усиливает следующую атаку каждой огненной сферы.','Overheat instantly empowers the next attack of each Fire-aligned Sphere.'],
      ['firetrail_overdrive_f2','Перегрев • Конвекция','Overheat • Convection','После усиления скорость атаки огненных сфер повышается ступенчато, пока перегрев не закончится.','After empowerment, Fire-aligned Spheres gain stacking attack speed until Overheat ends.'],
      ['firetrail_overdrive_f3','Перегрев • Выплеск','Overheat • Vent','Когда перегрев заканчивается, каждая усиленная сфера создаёт короткий огненный всплеск вокруг себя.','When Overheat ends, each empowered Sphere releases a brief fire burst around itself.'],
    ],
    firetrail_ignition: [
      ['firetrail_ignition_f1','Воспламенитель • Реакция','Igniter • Reaction','Враг с другим статусом при воспламенении получает дополнительный урон от реакции.','An enemy carrying another status takes bonus reaction damage when ignited.'],
      ['firetrail_ignition_f2','Воспламенитель • Цепь','Igniter • Chain','Воспламенение переносится на ближайшего врага с активным статусом.','Ignition jumps to the nearest enemy carrying an active status.'],
      ['firetrail_ignition_f3','Воспламенитель • Вулкан','Igniter • Volcano','Если реакция убивает цель, на её месте возникает вторичный огненный всплеск.','If the reaction kills the target, a secondary fire burst erupts at its position.'],
    ],
    firetrail_sanctum: [
      ['firetrail_sanctum_f1','Святилище • Поджиг','Sanctum • Kindle','Перегрев усиливает следующий импульс Aura и поджигает врагов внутри её зоны.','Overheat empowers the next Aura pulse and ignites enemies inside its area.'],
      ['firetrail_sanctum_f2','Святилище • Жаровня','Sanctum • Brazier','Пылающая Aura периодически выпускает огненный импульс, пока действует перегрев.','The burning Aura periodically emits a fire pulse while Overheat is active.'],
      ['firetrail_sanctum_f3','Святилище • Цикл','Sanctum • Cycle','Когда Aura получает усиление, ближайшая другая сфера получает короткий огненный перегрев.','When the Aura is empowered, a nearby different Sphere receives a brief Fire overheat.'],
    ],
  },
  minion: {
    minion_echo_drone: [
      ['minion_echo_drone_f1','Дрон • Осколки','Drone • Shards','Каждый Эхо-дрон создаёт один дополнительный осколочный импульс при подключении к сети.','Each Echo Drone releases an extra shard pulse when it joins the network.'],
      ['minion_echo_drone_f2','Дрон • Узлы','Drone • Nodes','Дроны считаются временными узлами и увеличивают число возможных сетевых связей.','Drones count as temporary nodes and increase the number of available network links.'],
      ['minion_echo_drone_f3','Дрон • Стая','Drone • Pack','Дроны группируются вокруг ближайшей сферы и усиливают её следующий боевой цикл.','Drones cluster around the nearest Sphere and empower its next combat cycle.'],
    ],
    minion_relay_drone: [
      ['minion_relay_drone_f1','Реле • Передача','Relay • Transfer','Дрон передаёт импульс по кратчайшему пути между двумя связанными сферами.','The drone transfers a pulse along the shortest path between two linked Spheres.'],
      ['minion_relay_drone_f2','Реле • Расщепление','Relay • Split','Передаваемый импульс разделяется на два более слабых импульса на следующем узле.','The transferred pulse splits into two weaker pulses at the next node.'],
      ['minion_relay_drone_f3','Реле • Перезапуск','Relay • Restart','После успешной передачи одна выбранная сфера мгновенно готовится к следующей атаке.','After a successful transfer, one linked Sphere is instantly readied for its next attack.'],
    ],
    minion_guardian: [
      ['minion_guardian_f1','Страж • Щит','Guardian • Shield','Дрон-страж периодически создаёт дополнительный защитный заряд у ближайшей сферы.','The Guardian Drone periodically creates an extra protective charge for the nearest Sphere.'],
      ['minion_guardian_f2','Страж • Перехват','Guardian • Intercept','Дрон-страж притягивает ближайшего врага к себе и отвлекает его от игрока.','The Guardian Drone draws the nearest enemy toward itself and away from the player.'],
      ['minion_guardian_f3','Страж • Атака','Guardian • Assault','После защиты сферы дрон выпускает контратаку по ближайшей цели.','After protecting a Sphere, the drone counterattacks the nearest target.'],
    ],
  },
  lightning: {
    lightning_echo_storm: [
      ['lightning_echo_storm_f1','Шторм • Прогон','Storm • Run','Разряд проходит через все Chain-сферы и усиливает каждый следующий перескок.','The discharge passes through all Chain Spheres, strengthening every subsequent jump.'],
      ['lightning_echo_storm_f2','Шторм • Гром','Storm • Thunder','После прохождения последней Chain-сферы возникает громовой взрыв вокруг конечной цели.','After the last Chain Sphere, a thunder burst detonates around the final target.'],
      ['lightning_echo_storm_f3','Шторм • Возврат','Storm • Return','После конечного удара часть заряда возвращается к первой Chain-сфере и создаёт второй слабый разряд.','After the final strike, part of the charge returns to the first Chain Sphere for a weaker second discharge.'],
    ],
    lightning_relay: [
      ['lightning_relay_f1','Реле • Перескок','Relay • Leap','Разряд всегда выбирает связанный узел, сохраняя непрерывную цепь.','The discharge always selects a linked node, preserving a continuous chain.'],
      ['lightning_relay_f2','Реле • Двойная дуга','Relay • Twin Arc','На каждом релейном переходе создаётся дополнительная короткая дуга к соседнему врагу.','Each relay transition creates a short extra arc to a nearby enemy.'],
      ['lightning_relay_f3','Реле • Перенос заряда','Relay • Charge Transfer','Последний переход сохраняет заряд и сокращает время до следующего Chain-цикла.','The final transition preserves charge and shortens the time until the next Chain cycle.'],
    ],
    lightning_overload: [
      ['lightning_overload_f1','Перегрузка • Финал','Overload • Finisher','Последний разряд серии получает мощный одиночный импульс по основной цели.','The final discharge of the sequence gains a powerful single-target burst.'],
      ['lightning_overload_f2','Перегрузка • Разрядка','Overload • Discharge','Последний разряд взрывается и поражает всех врагов рядом с целью.','The final discharge detonates and strikes all enemies near the target.'],
      ['lightning_overload_f3','Перегрузка • Перенапряжение','Overload • Overcurrent','После финального разряда одна Chain-сфера мгновенно запускает дополнительный укороченный разряд.','After the final discharge, one Chain Sphere immediately starts a shortened extra discharge.'],
    ],
  },
  timestop: {
    timestop_echo_phase: [
      ['timestop_echo_phase_f1','Фаза • Залп','Phase • Volley','При запуске остановки все сферы дают немедленный залп, а первый удар после остановки создаёт дополнительный импульс.','When Time Stop starts, all Spheres fire an immediate volley and the first hit after the stop creates an extra pulse.'],
      ['timestop_echo_phase_f2','Фаза • Окно','Phase • Window','Во время остановки сферы атакуют заметно чаще, но эффект сразу прекращается после возобновления времени.','During the stop, Spheres attack much more often, then the effect ends immediately when time resumes.'],
      ['timestop_echo_phase_f3','Фаза • Шрам','Phase • Scar','Каждая поражённая во время остановки цель получает метку, усиливающую следующий удар по ней.','Each enemy hit during the stop receives a mark that empowers the next hit against it.'],
    ],
    timestop_closed_time: [
      ['timestop_closed_time_f1','Замкнутое время • Кольцо','Closed Time • Ring','Зона остановки распространяется по активному кольцу связанных сфер.','The time-stop zone propagates through the active ring of linked Spheres.'],
      ['timestop_closed_time_f2','Замкнутое время • Коридор','Closed Time • Corridor','Между связанными узлами возникает коридор остановленного времени, замедляющий входящих врагов.','A corridor of stopped time forms between linked nodes and slows enemies entering it.'],
      ['timestop_closed_time_f3','Замкнутое время • Клетка','Closed Time • Cage','Связанные сферы замыкают вокруг группы врагов временную клетку, ограничивая выход.','Linked Spheres close a temporary time cage around a group, restricting escape.'],
    ],
    timestop_time_anchor: [
      ['timestop_time_anchor_f1','Якорь • Фиксация','Anchor • Lock','Ближайшая сфера фиксирует несколько целей вокруг себя на всё время остановки.','The nearest Sphere locks several targets around it for the full stop duration.'],
      ['timestop_time_anchor_f2','Якорь • Перенос','Anchor • Transfer','После снятия остановки зафиксированные цели передают остаток эффекта ближайшим врагам.','When the stop ends, anchored enemies pass the remaining effect to nearby enemies.'],
      ['timestop_time_anchor_f3','Якорь • Разрыв','Anchor • Break','При снятии якоря зафиксированные цели получают короткий импульс и замедление.','When the anchor breaks, locked targets receive a short pulse and slow.'],
    ],
  },
  darkritual: {
    darkritual_blood_link: [
      ['darkritual_blood_link_f1','Кровь • Передача','Blood • Transfer','Часть перегрузки передаётся ближайшей Standard-сфере и мгновенно ускоряет её атаку.','Part of the overload transfers to the nearest Standard Sphere and instantly accelerates its attack.'],
      ['darkritual_blood_link_f2','Кровь • Цепь','Blood • Chain','Заряд проходит через связанные Standard-сферы, ослабевая на каждом следующем узле.','Charge travels through linked Standard Spheres, weakening at each subsequent node.'],
      ['darkritual_blood_link_f3','Кровь • Возмездие','Blood • Vengeance','Если игрок получает урон от жертвы, ближайшая Standard-сфера выпускает ответный импульс.','If the player takes damage from the sacrifice, the nearest Standard Sphere releases a retaliatory pulse.'],
    ],
    darkritual_sacrifice: [
      ['darkritual_sacrifice_f1','Жертва • Импульс','Sacrifice • Pulse','Потерянное HP превращается в дополнительный круговой импульс вокруг ближайшей сферы.','Lost HP becomes an extra radial pulse around the nearest Sphere.'],
      ['darkritual_sacrifice_f2','Жертва • Шипы','Sacrifice • Thorns','Потеря HP создаёт короткие направленные шипы от ближайшей сферы к окружающим врагам.','HP sacrifice creates short directed spikes from the nearest Sphere toward nearby enemies.'],
      ['darkritual_sacrifice_f3','Жертва • Регенерация','Sacrifice • Reprieve','После импульса следующая серия убийств частично возвращает потраченное HP.','After the pulse, the next kill streak partially refunds the HP spent.'],
    ],
    darkritual_void_pact: [
      ['darkritual_void_pact_f1','Пустота • Низший предел','Void • Low Threshold','Чем ниже HP игрока, тем сильнее перегрузка, но только до безопасного предела.','The lower the player HP, the stronger the overload becomes, up to a safe threshold.'],
      ['darkritual_void_pact_f2','Пустота • Последний шанс','Void • Last Stand','При критически низком HP перегрузка даёт краткую неуязвимость после активации.','At critical HP, Overload grants brief invulnerability after activation.'],
      ['darkritual_void_pact_f3','Пустота • Экстремум','Void • Extremum','При низком HP перегрузка увеличивает урон и длится дольше, но не усиливает стоимость жертвы.','At low HP, Overload deals more damage and lasts longer without increasing the sacrifice cost.'],
    ],
  },
};

function buildAuthoredAbilityFinalVariants(): Record<string, Record<string, AbilityEvolutionChoice[]>> {
  const out: Record<string, Record<string, AbilityEvolutionChoice[]>> = {};
  for (const [ability, branches] of Object.entries(ABILITY_FINAL_VARIANT_SPECS)) {
    out[ability] = {};
    for (const [branch, specs] of Object.entries(branches)) {
      out[ability][branch] = specs.map(([id, ru, en, descRu, descEn]) => ({
        id,
        name: { ru, en },
        desc: { ru: descRu, en: descEn },
      }));
    }
  }
  return out;
}

const AUTHORED_ABILITY_FINAL_VARIANTS = buildAuthoredAbilityFinalVariants();


for (const [ability, branches] of Object.entries(AUTHORED_ABILITY_FINAL_VARIANTS)) {
  const progression = ABILITY_PROGRESSION[ability as AbilityType];
  if (progression) progression.evolution7ByBranch = branches;
}

const ABILITY_FINAL_POOLS = Object.fromEntries(
  Object.entries(ABILITY_PROGRESSION).map(([ability, progression]) => [
    ability,
    progression?.evolution7ByBranch ?? {},
  ]),
) as Record<string, Record<string, AbilityEvolutionChoice[]>>;

export function getAbilityFinalArchetype(s:any, ability:AbilityType): string | null {
  const selected = getAbilityBranchId(s, ability, 7);
  if (!selected) return null;
  const match = /_f([1-3])$/.exec(selected);
  if (!match) return null;
  const index = Number(match[1]) - 1;
  return ABILITY_PROGRESSION[ability]?.evolution7?.[index]?.id ?? null;
}


export function getAbilityEvolutionPool(s:any, ability:AbilityType, stage:4|7):AbilityEvolutionChoice[] {
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression) return [];
  if(stage===4) return progression.evolution4;
  const branch=getAbilityEvolutionChoice(s,ability,4);
  return branch ? (ABILITY_FINAL_POOLS[ability]?.[branch.id] ?? progression.evolution7) : [];
}


export function getAbilityEvolutionChoice(s:any, ability:AbilityType, stage:4|7):AbilityEvolutionChoice|null {
  const prefix='ability:'+ability+':'+stage+':';
  const marker=(s.player.evolutions||[]).find((x:string)=>x.startsWith(prefix));
  if(!marker) return null;
  const id=marker.slice(prefix.length);
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression) return null;
  const pool=stage===4?progression.evolution4:getAbilityEvolutionPool(s,ability,7);
  return pool.find((choice)=>choice.id===id)||null;
}

export function getAbilityDisplayName(s:any, ability:AbilityType, lang:'ru'|'en'):string {
  const base=ABILITIES[ability].name[lang];
  const level=s.player.abilities?.[ability]||0;
  if(level>=7){
    const final=getAbilityEvolutionChoice(s,ability,7);
    if(final) return base+' · '+final.name[lang];
  }
  if(level>=4){
    const branch=getAbilityEvolutionChoice(s,ability,4);
    if(branch) return base+' · '+branch.name[lang];
  }
  return base;
}

export function getAbilityDisplayDesc(s:any, ability:AbilityType, lang:'ru'|'en'):string {
  const level=s.player.abilities?.[ability]||0;
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression||level<=0) return '';
  if(level>=7){
    const final=getAbilityEvolutionChoice(s,ability,7);
    if(final) return final.desc[lang];
  }
  const branch=getAbilityEvolutionChoice(s,ability,4);
  if(level>=5&&branch) {
    const levelDef=progression.levels[level-1];
    return levelDef?.desc[lang]||branch.desc[lang];
  }
  if(level>=4&&branch) return branch.desc[lang];
  return progression.levels[level-1]?.desc[lang]||'';
}

export interface SphereAbilitySynergy {
  id:string;
  sphere:SphereType;
  sphereBranch:SphereEvolutionId;
  ability:AbilityType;
  abilityFinal:string;
  name:{ru:string;en:string};
  desc:{ru:string;en:string};
  behavior:string;
}

/**
 * Every Sphere Mutation I branch has exactly one future Ability synergy.
 * The pairing is branch-global rather than character-locked. Several
 * different Spheres may enhance the same Ability, but each rider targets a
 * distinct authored Mutation II final. It becomes active only when Sphere VII
 * + the selected branch + the exact Ability VII final are present.
 */
export const SPHERE_ABILITY_SYNERGIES: SphereAbilitySynergy[] = [
  {id:'std_res_blast',sphere:'standard',sphereBranch:'standard_resonator',abilityFinal:'blast_resonance_f1',ability:'blast',name:{ru:'Резонансное ядро',en:'Resonant Core'},desc:{ru:'Эхо-импульс создаёт дополнительный импульс от Стандартной и слегка подпитывает Резонанс.',en:'Blast emits an extra pulse from Standard and lightly feeds Resonance.'},behavior:'blast_standard_resonator'},
  {id:'std_singularity_timestop',sphere:'standard',sphereBranch:'standard_singularity',abilityFinal:'timestop_echo_phase_f1',ability:'timestop',name:{ru:'Сингулярный стазис',en:'Singularity Stasis'},desc:{ru:'Эхо-заморозка стягивает врагов к Стандартной и удерживает их дольше.',en:'Time Stop pulls enemies toward Standard and holds them longer.'},behavior:'timestop_standard_singularity'},
  {id:'std_swarm_minion',sphere:'standard',sphereBranch:'standard_swarm',abilityFinal:'minion_echo_drone_f1',ability:'minion',name:{ru:'Роевой запуск',en:'Swarm Launch'},desc:{ru:'Активация Эхо-дрона выпускает дополнительный осколочный импульс от Стандартной.',en:'Echo Drone activation releases an extra shard pulse from Standard.'},behavior:'minion_standard_swarm'},

  {id:'sniper_oracle_teleport',sphere:'sniper',sphereBranch:'sniper_oracle',abilityFinal:'teleport_echo_jump_f1',ability:'teleport',name:{ru:'Прицел прыжка',en:'Jumping Scope'},desc:{ru:'Телепорт к Снайперской помечает ближайшую цель и усиливает следующий удар.',en:'Teleporting to Sniper marks a nearby target and empowers the next hit.'},behavior:'teleport_sniper_oracle'},
  {id:'sniper_assassin_darkritual',sphere:'sniper',sphereBranch:'sniper_assassin',abilityFinal:'darkritual_blood_link_f1',ability:'darkritual',name:{ru:'Кровавый приговор',en:'Blood Sentence'},desc:{ru:'Перегрузка наносит дополнительный удар по самой ослабленной цели.',en:'Dark Ritual adds a finishing strike to the weakest target.'},behavior:'darkritual_sniper_assassin'},
  {id:'sniper_beacon_firetrail',sphere:'sniper',sphereBranch:'sniper_beacon',abilityFinal:'firetrail_overdrive_f1',ability:'firetrail',name:{ru:'Горящий маяк',en:'Burning Beacon'},desc:{ru:'Перегрев поджигает и замедляет врагов вокруг Снайперской.',en:'Firetrail ignites and slows enemies around Sniper.'},behavior:'firetrail_sniper_beacon'},

  {id:'shotgun_burst_shield',sphere:'shotgun',sphereBranch:'shotgun_burst',abilityFinal:'shield_echo_guard_f1',ability:'shield',name:{ru:'Ударный контур',en:'Impact Circuit'},desc:{ru:'Сферный барьер выпускает от Дробовика волну отбрасывания.',en:'Shield releases a knockback wave from Shotgun.'},behavior:'shield_shotgun_burst'},
  {id:'shotgun_cataclysm_blast',sphere:'shotgun',sphereBranch:'shotgun_cataclysm',abilityFinal:'blast_network_f2',ability:'blast',name:{ru:'Катастрофический импульс',en:'Cataclysmic Pulse'},desc:{ru:'Эхо-импульс детонирует дополнительный взрыв в каждой Дробовик-сфере.',en:'Blast detonates an extra explosion at each Shotgun Sphere.'},behavior:'blast_shotgun_cataclysm'},
  {id:'shotgun_hail_lightning',sphere:'shotgun',sphereBranch:'shotgun_hail',abilityFinal:'lightning_echo_storm_f1',ability:'lightning',name:{ru:'Электрический град',en:'Electric Hail'},desc:{ru:'Цепной разряд создаёт короткие дополнительные разряды вокруг Дробовика.',en:'Lightning creates short extra strikes around Shotgun.'},behavior:'lightning_shotgun_hail'},

  {id:'chain_web_timestop',sphere:'chain',sphereBranch:'chain_web',abilityFinal:'timestop_closed_time_f2',ability:'timestop',name:{ru:'Стазисная паутина',en:'Stasis Web'},desc:{ru:'Эхо-заморозка распространяется дальше по Цепной и длится дольше на узлах.',en:'Time Stop spreads farther through Chain and lasts longer on nodes.'},behavior:'timestop_chain_web'},
  {id:'chain_storm_lightning',sphere:'chain',sphereBranch:'chain_storm',abilityFinal:'lightning_relay_f2',ability:'lightning',name:{ru:'Грозовой резонанс',en:'Storm Resonance'},desc:{ru:'Каждый узел Цепной усиливает Цепной разряд и создаёт искровой всплеск.',en:'Each Chain node empowers Lightning and creates a spark burst.'},behavior:'lightning_chain_storm'},
  {id:'chain_leech_darkritual',sphere:'chain',sphereBranch:'chain_leech',abilityFinal:'darkritual_sacrifice_f2',ability:'darkritual',name:{ru:'Паразитический обмен',en:'Parasitic Exchange'},desc:{ru:'Dark Ritual возвращает часть потраченного HP через Chain.',en:'Dark Ritual refunds part of its HP cost through Chain.'},behavior:'darkritual_chain_leech'},

  {id:'aura_sanctum_shield',sphere:'aura',sphereBranch:'aura_sanctum',abilityFinal:'shield_reflector_f2',ability:'shield',name:{ru:'Щит святилища',en:'Sanctum Shield'},desc:{ru:'Сферный барьер создаёт защитный импульс внутри каждой Ауры.',en:'Shield creates a protective pulse inside each Aura.'},behavior:'shield_aura_sanctum'},
  {id:'aura_gravity_teleport',sphere:'aura',sphereBranch:'aura_gravity',abilityFinal:'teleport_beacon_f2',ability:'teleport',name:{ru:'Прыжок в гравитацию',en:'Gravity Jump'},desc:{ru:'Эхо-прыжок к Ауре притягивает ближайших врагов к центру.',en:'Teleporting to Aura pulls nearby enemies toward its center.'},behavior:'teleport_aura_gravity'},
  {id:'aura_overgrowth_firetrail',sphere:'aura',sphereBranch:'aura_overgrowth',abilityFinal:'firetrail_sanctum_f3',ability:'firetrail',name:{ru:'Перегрев сети',en:'Network Overgrowth'},desc:{ru:'Перегрев ускоряет следующий цикл сфер внутри Ауры.',en:'Firetrail accelerates the next cycle of Spheres inside Aura.'},behavior:'firetrail_aura_overgrowth'},

  {id:'orbital_dance_minion',sphere:'orbital',sphereBranch:'orbital_dance',abilityFinal:'minion_relay_drone_f2',ability:'minion',name:{ru:'Орбитальное реле',en:'Orbital Relay'},desc:{ru:'Эхо-дрон ускоряет ближайшую Орбитальную и запускает дополнительный проход.',en:'Echo Drone accelerates the nearest Orbital and triggers an extra pass.'},behavior:'minion_orbital_dance'},
  {id:'orbital_halo_shield',sphere:'orbital',sphereBranch:'orbital_halo',abilityFinal:'shield_bastion_f3',ability:'shield',name:{ru:'Защитный ореол',en:'Guardian Halo'},desc:{ru:'Сферный барьер добавляет заряд и выпускает защитный импульс от Орбитальной.',en:'Shield adds a charge and emits a defensive Orbital pulse.'},behavior:'shield_orbital_halo'},
  {id:'orbital_blade_darkritual',sphere:'orbital',sphereBranch:'orbital_blade',abilityFinal:'darkritual_void_pact_f3',ability:'darkritual',name:{ru:'Клинок жертвы',en:'Sacrificial Blades'},desc:{ru:'Перегрузка выпускает режущую волну из каждой Орбитальной.',en:'Dark Ritual releases a cutting wave from each Orbital.'},behavior:'darkritual_orbital_blade'},

  {id:'prism_split_blast',sphere:'prism',sphereBranch:'prism_split',abilityFinal:'blast_core_f3',ability:'blast',name:{ru:'Призматический раскол',en:'Prismatic Split'},desc:{ru:'Эхо-импульс превращается в серию отражённых лучей от Призмы.',en:'Blast becomes a series of reflected Prism beams.'},behavior:'blast_prism_split'},
  {id:'prism_spectrum_firetrail',sphere:'prism',sphereBranch:'prism_spectrum',abilityFinal:'firetrail_ignition_f2',ability:'firetrail',name:{ru:'Спектральный катализ',en:'Spectrum Catalyst'},desc:{ru:'Перегрев запускает усиленный статусный импульс вокруг Призмы.',en:'Firetrail triggers an empowered status pulse around Prism.'},behavior:'firetrail_prism_spectrum'},
  {id:'prism_mirror_teleport',sphere:'prism',sphereBranch:'prism_mirror',abilityFinal:'teleport_phase_f3',ability:'teleport',name:{ru:'Зеркальный прыжок',en:'Mirror Jump'},desc:{ru:'Эхо-прыжок к Призме запускает отражённый луч по нескольким целям.',en:'Teleporting to Prism fires a reflected beam at several targets.'},behavior:'teleport_prism_mirror'},

  {id:'gravity_well_firetrail',sphere:'gravity',sphereBranch:'gravity_well',abilityFinal:'firetrail_overdrive_f3',ability:'firetrail',name:{ru:'Огненный колодец',en:'Flame Well'},desc:{ru:'Перегрев дольше держит врагов в гравитационном поле и продлевает горение.',en:'Firetrail keeps enemies in the gravity field longer and extends burn.'},behavior:'firetrail_gravity_well'},
  {id:'gravity_tide_timestop',sphere:'gravity',sphereBranch:'gravity_tide',abilityFinal:'timestop_time_anchor_f3',ability:'timestop',name:{ru:'Обратная волна',en:'Reverse Tide'},desc:{ru:'Эхо-заморозка запускает обратную волну от Гравитационной, отбрасывающую врагов.',en:'Time Stop triggers a reverse Gravity wave that pushes enemies away.'},behavior:'timestop_gravity_tide'},
  {id:'gravity_collapse_blast',sphere:'gravity',sphereBranch:'gravity_collapse',abilityFinal:'blast_network_f1',ability:'blast',name:{ru:'Имплозия',en:'Implosion'},desc:{ru:'Эхо-импульс сжимает группу врагов к Гравитационной и завершает сжатие дополнительным импульсом.',en:'Blast compresses enemies toward Gravity and adds an extra implosion pulse.'},behavior:'blast_gravity_collapse'},

  {id:'pulse_wave_blast',sphere:'pulse',sphereBranch:'pulse_wave',abilityFinal:'blast_core_f2',ability:'blast',name:{ru:'Ударная волна',en:'Shockwave'},desc:{ru:'Эхо-импульс запускает дополнительную волну от каждой Импульсной.',en:'Blast launches an extra wave from each Pulse.'},behavior:'blast_pulse_wave'},
  {id:'pulse_resonator_lightning',sphere:'pulse',sphereBranch:'pulse_resonator',abilityFinal:'lightning_echo_storm_f3',ability:'lightning',name:{ru:'Заряженный резонатор',en:'Charged Resonator'},desc:{ru:'Цепной разряд подпитывает Резонанс и ускоряет следующий цикл Импульсной.',en:'Lightning feeds Resonance and accelerates the next Pulse cycle.'},behavior:'lightning_pulse_resonator'},
  {id:'pulse_burst_shield',sphere:'pulse',sphereBranch:'pulse_burst',abilityFinal:'shield_echo_guard_f2',ability:'shield',name:{ru:'Щитовой импульс',en:'Shield Pulse'},desc:{ru:'Сферный барьер выпускает дополнительную ударную волну от каждой Импульсной.',en:'Shield releases an extra shock pulse from each Pulse.'},behavior:'shield_pulse_burst'},

  {id:'void_hunger_darkritual',sphere:'void',sphereBranch:'void_hunger',abilityFinal:'darkritual_blood_link_f2',ability:'darkritual',name:{ru:'Голод ритуала',en:'Ritual Hunger'},desc:{ru:'Dark Ritual превращает потерянное HP игрока в дополнительный Void-удар.',en:'Dark Ritual converts lost player HP into an extra Void strike.'},behavior:'darkritual_void_hunger'},
  {id:'void_reaper_minion',sphere:'void',sphereBranch:'void_reaper',abilityFinal:'minion_guardian_f3',ability:'minion',name:{ru:'Жнец душ',en:'Soul Reaper'},desc:{ru:'Активация Echo Drone запускает жатву вокруг Void и возвращает HP за добивания.',en:'Echo Drone activation starts a harvest around Void and restores HP from kills.'},behavior:'minion_void_reaper'},
  {id:'void_execution_teleport',sphere:'void',sphereBranch:'void_execution',abilityFinal:'teleport_echo_jump_f2',ability:'teleport',name:{ru:'Разрыв казни',en:'Execution Rift'},desc:{ru:'Эхо-прыжок к Пустотной прорезает ослабленных врагов и добивает самых уязвимых.',en:'Teleporting to Void cuts through weakened enemies and executes the most vulnerable.'},behavior:'teleport_void_execution'},
];

export function getActiveSphereAbilitySynergies(s:any): SphereAbilitySynergy[] {
  return SPHERE_ABILITY_SYNERGIES.filter(link =>
    sphereLevel(s, link.sphere) >= 7 &&
    s.player.sphereBranches?.[link.sphere] === link.sphereBranch &&
    (s.player.abilities?.[link.ability] || 0) >= 7 &&
    getAbilityBranchId(s, link.ability, 7) === link.abilityFinal
  );
}

export function hasActiveSphereAbilitySynergy(
  s:any,
  sphere:SphereType,
  branch:SphereEvolutionId,
  ability:AbilityType,
):boolean {
  return getActiveSphereAbilitySynergies(s).some(link =>
    link.sphere === sphere && link.sphereBranch === branch && link.ability === ability
  );
}

export function getSphereMutationSynergyHints(
  s:any,
  sphere:SphereType,
  branch:SphereEvolutionId,
): SphereAbilitySynergy[] {
  return SPHERE_ABILITY_SYNERGIES.filter(link =>
    link.sphere === sphere &&
    link.sphereBranch === branch
  );
}

export const CHARACTER_SPHERE_PRIORITY:Record<CharacterId,SphereType[]>={
  spherist:['standard','chain','orbital','pulse'],
  hunter:['sniper','chain','void','prism'],
  engineer:['aura','orbital','pulse','gravity'],
  berserker:['shotgun','standard','orbital','void'],
  alchemist:['aura','chain','gravity','pulse'],
  architect:['prism','sniper','gravity','orbital'],
  conductor:['pulse','chain'],
  oracle:['prism','pulse'],
  voidwalker:['void','gravity'],
  fractal:['orbital','aura'],
};

export function spherePriority(character:CharacterId,type:SphereType){
  const def=SPHERE_PROGRESSION[type];
  return def?.priority?.[character] ?? 0.25;
}
export function sphereLevel(s:any,type:SphereType){return s.player.sphereProgression?.[type]||0;}
function sphereFinalIndex(s:any,type:SphereType):number|null{
  const id=(s.player.evolutions||[]).find((x:string)=>x.startsWith('sphere:'+type+':7:'));
  if(!id)return null;
  const n=Number(id.split(':').pop());
  return Number.isFinite(n)?n:null;
}
type AuthoredModifierLevels = Partial<Record<keyof import('./engineTypes').SphereMods, number>>;

const AUTHORED_BASE_MODIFIERS: Partial<Record<SphereEvolutionId, AuthoredModifierLevels>> = {
  standard_resonator:{resonant:1},
  standard_singularity:{anchor:1,gravitic:1},
  standard_swarm:{},
  sniper_oracle:{mark:1},
  sniper_assassin:{},
  sniper_beacon:{anchor:1,mark:1},
  shotgun_burst:{multishot:1},
  shotgun_cataclysm:{shatter:1},
  shotgun_hail:{multishot:1},
  chain_web:{anchor:1},
  chain_storm:{static:1,resonant:1},
  chain_leech:{vampiric:1},
  aura_sanctum:{freeze:1,anchor:1},
  aura_gravity:{gravitic:1,anchor:1},
  aura_overgrowth:{},
  orbital_dance:{afterimage:1},
  orbital_halo:{resonant:1},
  orbital_blade:{impact:1,afterimage:1},
  prism_split:{multishot:1},
  prism_spectrum:{resonant:1},
  prism_mirror:{ricochet:1},
  gravity_well:{anchor:1,gravitic:1},
  gravity_tide:{impact:1,gravitic:1},
  gravity_collapse:{execute:1,gravitic:1},
  pulse_wave:{impact:1},
  pulse_resonator:{resonant:1},
  pulse_burst:{shatter:1},
  void_hunger:{corrupt:1},
  void_reaper:{vampiric:1},
  void_execution:{execute:1,phase:1},
};

const AUTHORED_FINAL_MODIFIERS: Partial<Record<SphereEvolutionId, Partial<Record<0|1|2, AuthoredModifierLevels>>>> = {
  standard_resonator:{0:{},1:{impact:1},2:{}},
  standard_singularity:{0:{gravitic:2},1:{},2:{gravitic:2}},
  standard_swarm:{0:{},1:{},2:{}},
  sniper_oracle:{0:{mark:2},1:{mark:2,resonant:1},2:{mark:2,execute:1}},
  sniper_assassin:{0:{},1:{},2:{}},
  sniper_beacon:{0:{anchor:2},1:{mark:2},2:{anchor:2,mark:2}},
  shotgun_burst:{0:{multishot:2},1:{},2:{multishot:2}},
  shotgun_cataclysm:{0:{shatter:2},1:{impact:2},2:{shatter:2,impact:2}},
  shotgun_hail:{0:{},1:{multishot:2},2:{multishot:2}},
  chain_web:{0:{anchor:2},1:{static:1},2:{anchor:2,static:1}},
  chain_storm:{0:{static:2},1:{resonant:2},2:{static:2,resonant:2}},
  chain_leech:{0:{vampiric:2},1:{drain:1},2:{vampiric:2,drain:1}},
  aura_sanctum:{0:{freeze:2},1:{anchor:2},2:{freeze:2,anchor:2}},
  aura_gravity:{0:{gravitic:2},1:{anchor:2},2:{gravitic:2,anchor:2}},
  aura_overgrowth:{0:{},1:{},2:{}},
  orbital_dance:{0:{afterimage:2},1:{afterimage:3},2:{afterimage:3}},
  orbital_halo:{0:{resonant:1},1:{},2:{resonant:2}},
  orbital_blade:{0:{impact:2},1:{afterimage:2},2:{impact:2,afterimage:3}},
  prism_split:{0:{multishot:2},1:{multishot:1,pierce:2},2:{multishot:1,ricochet:1}},
  prism_spectrum:{0:{fire:1},1:{freeze:1},2:{poison:1}},
  prism_mirror:{0:{ricochet:1},1:{ricochet:2},2:{ricochet:2,echo:1}},
  gravity_well:{0:{anchor:2},1:{gravitic:2},2:{anchor:2,gravitic:2}},
  gravity_tide:{0:{impact:2},1:{gravitic:2},2:{impact:1,gravitic:1}},
  gravity_collapse:{0:{execute:2},1:{gravitic:2},2:{execute:2,gravitic:2}},
  pulse_wave:{0:{impact:2},1:{freeze:1},2:{impact:2,freeze:1}},
  pulse_resonator:{0:{resonant:2},1:{resonant:2,impact:1},2:{resonant:2,shatter:1}},
  pulse_burst:{0:{shatter:2},1:{impact:2},2:{shatter:2,impact:2}},
  void_hunger:{0:{corrupt:2},1:{corrupt:2,resonant:1},2:{corrupt:2,drain:1}},
  void_reaper:{0:{vampiric:2},1:{drain:2},2:{vampiric:2,drain:2}},
  void_execution:{0:{execute:2},1:{phase:1},2:{execute:2,phase:2}},
};

export function getAuthoredSphereModifierLevels(source:any, type:SphereType): AuthoredModifierLevels {
  const branch = source?.sphereBranches?.[type] as SphereEvolutionId | undefined;
  const levels: AuthoredModifierLevels = { ...(branch ? (AUTHORED_BASE_MODIFIERS[branch] ?? {}) : {}) };
  if (!branch) return levels;
  const finalId = (source?.evolutions || []).find((id:string)=>id.startsWith('sphere:'+type+':7:'));
  if (!finalId) return levels;
  const finalIndex = Number(finalId.split(':').pop());
  if (finalIndex !== 0 && finalIndex !== 1 && finalIndex !== 2) return levels;
  const final = AUTHORED_FINAL_MODIFIERS[branch]?.[finalIndex as 0|1|2] ?? {};
  for (const [kind, value] of Object.entries(final) as [keyof AuthoredModifierLevels, number][]) {
    levels[kind] = Math.max(levels[kind] ?? 0, value);
  }
  return levels;
}

export function sphereModifiers(s:any,type:SphereType,sphere?:any){
  const l=sphereLevel(s,type), branch=s.player.sphereBranches?.[type], final=sphereFinalIndex(s,type), artifact=getSphereArtifactModifiers(s,type,sphere);
  const authored=getAuthoredSphereModifierLevels(s.player,type);
  const modifierLevel=(kind:keyof SphereMods):number=>Math.max(Number(s.player.sphereMods?.[kind] ?? 0),Number(authored[kind] ?? 0));
  let damage=1, radius=1, delay=1, pierce=0, multishot=0, chainTargets=1, auraRadius=1, auraPulse=.5, rotationSpeed=1;
  let spreadMult=1;
  const abilityRange = Number(s.player.abilities.range || 0);
  if (abilityRange > 0) {
    radius *= 1 + 0.05 * abilityRange;
    auraRadius *= 1 + 0.05 * abilityRange;
  }
  let splitChance=0, echoChance=0, staticChance=0, resonantCharge=0;
  let healOnHit=0, healOnKill=0, knockback=0, gravitic=0, magnetic=0;

  // Levels I-III mirror their written upgrades instead of silently stacking generic bonuses.
  if(type==='standard'){
    if(l>=1) damage*=1.15;
    if(l>=2) pierce+=1;
    if(l>=3) delay*=.9;
  }
  if(type==='sniper'){
    if(l>=1) damage*=1.25;
    if(l>=2) radius*=1.15;
  }
  if(type==='shotgun'){
    if(l>=1) multishot+=1;
    if(l>=3) spreadMult*=.88;
  }
  if(type==='chain'){
    if(l>=1) chainTargets+=1;
    if(l>=2) damage*=1.10;
    if(l>=3) delay*=0.85;
  }
  if(type==='aura'){
    if(l>=1) auraRadius*=1.20;
    if(l>=2) auraPulse*=.9;
    if(l>=3) damage*=1.10;
  }

  // Final modifier catalogue: every entry has a concrete combat parameter.
  if(modifierLevel('breach') > 0) pierce += modifierLevel('breach');
  if(modifierLevel('overload') > 0) damage *= 1 + 0.08 * modifierLevel('overload');
  if(modifierLevel('pierce') > 0 && sphereUsesProjectileModifiers(type)) pierce += modifierLevel('pierce');
  if(modifierLevel('split') > 0) splitChance = Math.min(0.45, 0.18 * modifierLevel('split'));
  if(modifierLevel('shatter') > 0) damage *= 1 + 0.04 * modifierLevel('shatter');
  // Execute is applied conditionally in the authoritative damage path, not as a global damage multiplier.
  if(modifierLevel('mark') > 0) damage *= 1 + 0.04 * modifierLevel('mark');
  if(modifierLevel('echo') > 0) echoChance = Math.min(0.35, 0.12 * modifierLevel('echo'));
  if(modifierLevel('anchor') > 0) auraRadius *= 1 + 0.05 * modifierLevel('anchor');
  if(modifierLevel('phase') > 0) pierce += modifierLevel('phase');
  if(modifierLevel('static') > 0) staticChance = Math.min(0.35, 0.12 * modifierLevel('static'));
  if(modifierLevel('resonant') > 0) resonantCharge = 2 * modifierLevel('resonant');
  if(modifierLevel('magnetic') > 0) magnetic = modifierLevel('magnetic');
  if(modifierLevel('vampiric') > 0) healOnHit = 0.006 * modifierLevel('vampiric');
  if(modifierLevel('drain') > 0) healOnKill = 0.03 * modifierLevel('drain');
  if(modifierLevel('impact') > 0) knockback = modifierLevel('impact');
  if(modifierLevel('gravitic') > 0) gravitic = modifierLevel('gravitic');

  // Shared post-evolution scaling.
  if(l>=4) damage*=1.12;
  if(l>=5){ damage*=1.06; radius*=1.06; }
  if(l>=7){ damage*=1.18; radius*=1.08; }

  if(type==='standard'&&branch==='standard_resonator'){damage*=1.08;if(final===0)radius*=1.12;}
  if(type==='standard'&&branch==='standard_singularity'){damage*=1.06;if(final===0)auraRadius*=1.15;}
  // Standard Swarm is implemented as side shards in the combat proc,
  // not as hidden permanent Multishot. This keeps the branch distinct.
  if(type==='sniper'&&branch==='sniper_oracle'){damage*=1.10;if(final===0)damage*=1.18;}
  if(type==='sniper'&&branch==='sniper_assassin'){damage*=1.12;if(final===0)damage*=1.20;}
  if(type==='sniper'&&branch==='sniper_beacon'){radius*=1.15;if(final===0)radius*=1.20;}
  if(type==='shotgun'&&branch==='shotgun_burst'){damage*=1.08;if(final===0)damage*=1.20;}
  if(type==='shotgun'&&branch==='shotgun_cataclysm'){damage*=1.12;pierce+=2;if(final===0)pierce+=2;}
  if(type==='shotgun'&&branch==='shotgun_hail'){multishot+=1;if(final===0)multishot+=1;}
  if(type==='chain'&&branch==='chain_web'){chainTargets+=2;if(final===0)chainTargets+=2;}
  if(type==='chain'&&branch==='chain_storm'){damage*=1.08;chainTargets+=1;if(final===0)damage*=1.18;}
  if(type==='chain'&&branch==='chain_leech'){damage*=1.05;if(final===0)damage*=1.15;}
  if(type==='aura'&&branch==='aura_sanctum'){auraRadius*=1.15;if(final===0)auraPulse*=0.8;}
  if(type==='aura'&&branch==='aura_gravity'){
    auraRadius*=1.10;
    if(final===0) auraRadius*=1.18;
    if(final===1) auraRadius*=1.28;
    if(final===null && l>=5) auraRadius*=l>=6?1.15:1.08;
  }
  if(type==='aura'&&branch==='aura_overgrowth'){damage*=1.06;auraRadius*=1.08;if(final===0)damage*=1.15;}

  // NEW FIVE: their level 1-3 upgrades must modify real combat parameters.
  if(type==='orbital'){
    // Orbital's third core upgrade is rotation speed, not a hidden attack-delay stat.
    if(l>=1) damage*=1.15;
    if(l>=2) radius*=1.15;
    if(l>=3) rotationSpeed*=1.15;
    // Orbital satellites are native to the Sphere, not projectile MULTISHOT.
    if(branch==='orbital_dance'){
      rotationSpeed*=final===0?1.28:final===1?1.55:1.28;
      if(final===null && l>=5) rotationSpeed*=l>=6?1.20:1.10;
    }
    if(branch==='orbital_halo'){
      rotationSpeed*=1.08;
      if(final===null && l>=5) resonantCharge*=l>=6?1.25:1.12;
    }
    if(branch==='orbital_blade'){
      damage*=final===0 ? 1.15 : final===1 ? 1.05 : 1.20;
    }
  }
  if(type==='prism'){
    if(l>=1) damage*=1.20;
    if(l>=2) radius*=1.15;
    if(l>=3) multishot+=1;
    if(branch==='prism_split'){ multishot+=final===1?1:0; }
    if(branch==='prism_spectrum'){ /* Elemental status and Resonance define this branch. */ }
    if(branch==='prism_mirror'){ radius*=1.08; }
  }
  if(type==='gravity'){
    // Level I is a control-strength upgrade; the runtime consumes it as pull strength.
    if(l>=2) radius*=1.15;
    if(l>=3) auraPulse*=0.85;
    if(branch==='gravity_well'){ auraRadius*=final===1?1.20:1.08; }
    if(branch==='gravity_tide'){
      auraPulse*=0.90;
      if(final===null && l>=5) auraRadius*=l>=6?1.14:1.07;
    }
    if(branch==='gravity_collapse'){ damage*=final===2?1.18:1.08; }
  }
  if(type==='pulse'){
    if(l>=1) damage*=1.20;
    if(l>=2) radius*=1.15;
    if(l>=3) auraPulse*=0.88;
    if(branch==='pulse_wave'){ radius*=final===1?1.22:1.10; }
    if(branch==='pulse_resonator'){ damage*=1.05; }
    if(branch==='pulse_burst'){ damage*=final===2?1.12:1.04; }
  }
  if(type==='void'){
    // Level I's +20% applies conditionally to weakened targets in engineCombat.
    if(l>=3) radius*=1.15;
    if(branch==='void_hunger'){ damage*=1.05; }
    if(branch==='void_reaper'){ damage*=1.03; }
    if(branch==='void_execution'){ damage*=final===2?1.10:1.04; }
  }

  const projectileMods = sphereUsesProjectileModifiers(type);
  if (projectileMods) multishot += modifierLevel('multishot');
  return {
    damage:damage*artifact.damage,radius:radius*artifact.radius,delay:delay*artifact.delay,
    pierce: projectileMods ? pierce : 0,
    multishot: projectileMods ? multishot : 0,
    chainTargets,auraRadius,auraPulse,spreadMult,
    splitChance,echoChance,staticChance,resonantCharge,healOnHit,healOnKill,
    knockback,
    rotationSpeed,
    fire: modifierLevel('fire'),
    freeze: modifierLevel('freeze'),
    poison: modifierLevel('poison'),
    breach: modifierLevel('breach'),
    overload: modifierLevel('overload'),
    split: modifierLevel('split'),
    shatter: modifierLevel('shatter'),
    execute: modifierLevel('execute'),
    ricochet: projectileMods ? modifierLevel('ricochet') : 0,
    mark: modifierLevel('mark'),
    echo: modifierLevel('echo'),
    anchor: modifierLevel('anchor'),
    phase: modifierLevel('phase'),
    static: modifierLevel('static'),
    resonant: modifierLevel('resonant'),
    magnetic: modifierLevel('magnetic'),
    vampiric: modifierLevel('vampiric'),
    corrupt: modifierLevel('corrupt'),
    drain: modifierLevel('drain'),
    afterimage: Math.max(
      modifierLevel('afterimage'),
      branch === 'orbital_blade' && final === null && l >= 5 ? (l >= 6 ? 3 : 2) : 0,
    ),
    impact: modifierLevel('impact'),
    gravitic: modifierLevel('gravitic'),
  };
}