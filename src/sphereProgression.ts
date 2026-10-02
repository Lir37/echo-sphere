import type { CharacterId } from './characters';
import { getSphereArtifactModifiers } from './artifactSystem';
import { ABILITIES, SPHERE_TYPES, type SphereType, type AbilityType, sphereUsesProjectileModifiers } from './gameData';
import type { SphereMods } from './engineTypes';

export type SphereEvolutionId = 'standard_resonator' | 'standard_singularity' | 'standard_swarm' | 'sniper_oracle' | 'sniper_assassin' | 'sniper_beacon' | 'shotgun_burst' | 'shotgun_cataclysm' | 'shotgun_hail' | 'chain_web' | 'chain_storm' | 'chain_leech' | 'aura_sanctum' | 'aura_gravity' | 'aura_overgrowth' | 'orbital_dance' | 'orbital_halo' | 'orbital_blade' | 'prism_split' | 'prism_spectrum' | 'prism_mirror' | 'gravity_well' | 'gravity_tide' | 'gravity_collapse' | 'pulse_wave' | 'pulse_resonator' | 'pulse_burst' | 'void_hunger' | 'void_reaper' | 'void_execution';
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
const lv=(a:string,b:string,c:string):SphereUpgradeDef[]=>[{level:1,name:{ru:'Ядро',en:'Core'},desc:{ru:a,en:a}},{level:2,name:{ru:'Механизм',en:'Mechanism'},desc:{ru:b,en:b}},{level:3,name:{ru:'Настройка',en:'Tuning'},desc:{ru:c,en:c}},{level:4,name:{ru:'Эволюция I',en:'Evolution I'},desc:{ru:'Выбор одной из трёх веток',en:'Choose one of three branches'}},{level:5,name:{ru:'Контур',en:'Circuit'},desc:{ru:'Усиление выбранной ветки',en:'Strengthens the selected branch'}},{level:6,name:{ru:'Стабилизатор',en:'Stabilizer'},desc:{ru:'Усиление специальной механики',en:'Strengthens the special mechanic'}},{level:7,name:{ru:'Эволюция II',en:'Evolution II'},desc:{ru:'Финальная специализация',en:'Final specialization'}}];
const e=(id:string,ru:string,en:string,descRu:string,descEn:string):SphereEvolutionDef=>({id,name:{ru,en},desc:{ru:descRu,en:descEn}});
const BRANCH_LEVEL_DETAILS:Partial<Record<SphereEvolutionId,{level5:string;level6:string}>>={
  standard_resonator:{level5:'Каждое 3-е попадание создаёт импульс вокруг цели. На V уровне импульс становится частью основного цикла атак.',level6:'Импульс срабатывает стабильнее: после каждого третьего попадания сфера усиливает массовое поражение перед финальной формой.'},
  standard_singularity:{level5:'Каждое попадание замедляет врага и начинает стягивать ближайших противников к точке удара.',level6:'Стягивание становится сильнее: сфера лучше собирает группу врагов в одной зоне для последующих попаданий.'},
  standard_swarm:{level5:'Попадания выпускают дополнительный боковой осколок, расширяя покрытие по площади.',level6:'Боковые осколки становятся частью постоянного паттерна атаки и закрывают больше направлений вокруг сферы.'},
  sniper_oracle:{level5:'По отмеченной цели срабатывает усиленный выстрел, превращая Метку в главный источник урона.',level6:'Усиленный выстрел по Метке получает ещё один шаг мощности и становится надёжнее против приоритетной цели.'},
  sniper_assassin:{level5:'Враги ниже 35% HP получают резко повышенный урон, чтобы снайперская сфера добивала ослабленные цели.',level6:'Добивающий урон ещё сильнее наказывает цели с низким запасом здоровья, подготавливая финальную форму.'},
  sniper_beacon:{level5:'Попадание создаёт вокруг цели зону Метки и замедляет ближайших врагов.',level6:'Зона Метки становится заметнее и охватывает больше целей, усиливая контроль пространства.'},
  shotgun_burst:{level5:'Чем ближе враг к сфере, тем выше урон центральных дробин.',level6:'Ближний бой становится ещё опаснее: бонус урона работает на более широкой дистанции и сильнее вознаграждает сближение.'},
  shotgun_cataclysm:{level5:'Попадание вызывает взрыв по соседним врагам и частично пробивает толпу.',level6:'Взрыв получает дополнительную мощность и лучше работает против плотных групп противников.'},
  shotgun_hail:{level5:'Попадания с шансом вызывают дополнительный град осколков по области вокруг цели.',level6:'Град появляется чаще и накрывает большую зону, добавляя постоянное давление по группе.'},
  chain_web:{level5:'Цепь замедляет поражённых врагов, превращая серию перескоков в зону контроля.',level6:'Замедление усиливается, поэтому одна цепная атака дольше удерживает группу врагов в сети.'},
  chain_storm:{level5:'Каждый переход цепи создаёт дополнительный электрический всплеск по ближайшим врагам.',level6:'Всплеск становится мощнее и добавляет ещё больше урона всей цепной последовательности.'},
  chain_leech:{level5:'Каждое попадание цепи возвращает часть нанесённого урона в HP игрока.',level6:'Лечение от цепи увеличивается, превращая длительную серию попаданий в устойчивый источник восстановления.'},
  aura_sanctum:{level5:'Импульс ауры сильно замедляет врагов внутри области и удерживает их возле сферы.',level6:'Контроль становится надёжнее: враги дольше остаются внутри ауры под действием замедления.'},
  aura_gravity:{level5:'Аура периодически притягивает врагов к центру, собирая толпу для массового урона.',level6:'Гравитация действует сильнее и на большей зоне, плотнее стягивая врагов к центру.'},
  aura_overgrowth:{level5:'Сферы рядом с Аурой получают ускорение атак и чаще выпускают свои снаряды.',level6:'Зона усиления расширяется, позволяя большему числу сфер одновременно пользоваться ускорением.'
  },
};
const NEW_BRANCH_LEVEL_DETAILS:Partial<Record<SphereEvolutionId,{level5:string;level6:string}>>={
  orbital_dance:{level5:'Орбиты ускоряются и чаще пересекают врагов.',level6:'Ускорение орбит усиливается, а окно между боевыми проходами сокращается.'},
  orbital_halo:{level5:'Орбита создаёт защитный резонанс для ближайших сфер.',level6:'Защитный резонанс распространяется на большее число связанных сфер.'},
  orbital_blade:{level5:'Орбитальные лезвия наносят повышенный урон по врагам на траектории.',level6:'Урон лезвий растёт, а эффективная зона прохода становится шире.'},
  prism_split:{level5:'Луч делится на дополнительную цель после основного попадания.',level6:'Разделённый луч поражает дополнительную цель с меньшей потерей мощности.'},
  prism_spectrum:{level5:'Луч переносит активный статусный эффект на поражённую цель.',level6:'Статусная передача становится стабильнее и сильнее взаимодействует с реакциями.'},
  prism_mirror:{level5:'Связанные сферы создают вторичные отражённые лучи.',level6:'Отражённые лучи получают дополнительную дальность и стабильность.'},
  gravity_well:{level5:'Зона притяжения становится плотнее и сильнее замедляет врагов.',level6:'Стягивание усиливается к центру и лучше удерживает плотные группы.'},
  gravity_tide:{level5:'Гравитация чередует фазы стягивания и обратного толчка.',level6:'Фазы становятся мощнее и расширяют контроль пространства.'},
  gravity_collapse:{level5:'Плотно собранные враги получают дополнительный импульсный урон.',level6:'Коллапс сильнее наказывает большие скопления и ослабленные цели.'},
  pulse_wave:{level5:'Каждая волна получает увеличенный радиус и отбрасывает врагов от узла.',level6:'Волна распространяется дальше и дольше сохраняет контроль.'},
  pulse_resonator:{level5:'Импульс дополнительно подпитывает Resonance при работающей сети.',level6:'Связанный импульс чаще создаёт резонансный всплеск.'},
  pulse_burst:{level5:'После основной волны возникает короткий второй разряд по центру.',level6:'Второй разряд становится сильнее и лучше работает против плотных групп.'},
  void_hunger:{level5:'Урон растёт пропорционально потерянному здоровью цели.',level6:'Ослабленные цели получают ещё более высокий множитель добивания.'},
  void_reaper:{level5:'Убийство Void возвращает немного HP и создаёт осколки пустоты.',level6:'Осколки получают повышенный урон и чаще поддерживают цепь убийств.'},
  void_execution:{level5:'Порог исполнения повышается, превращая слабые цели в приоритет.',level6:'Порог исполнения растёт ещё сильнее, а добивание наносит больше урона боссам.'},
};

const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id] ?? NEW_BRANCH_LEVEL_DETAILS[id] ?? { level5: 'Усиление выбранной ветки.', level6: 'Дополнительное усиление уникальной механики.' };
  return {
    ...e(id,ru,desc),
    final:fin,
    level5:{ru:details.level5,en:details.level5},
    level6:{ru:details.level6,en:details.level6},
  };
};
const BRANCH_EN:Partial<Record<SphereEvolutionId,[string,string]>>={
  standard_resonator:['Standard Resonator','Develops the Standard Resonator branch and its signature behavior.'],
  standard_singularity:['Standard Singularity','Develops the Standard Singularity branch and its signature behavior.'],
  standard_swarm:['Standard Swarm','Develops the Standard Swarm branch and its signature behavior.'],
  sniper_oracle:['Sniper Oracle','Develops the Sniper Oracle branch and its signature behavior.'],
  sniper_assassin:['Sniper Assassin','Develops the Sniper Assassin branch and its signature behavior.'],
  sniper_beacon:['Sniper Beacon','Develops the Sniper Beacon branch and its signature behavior.'],
  shotgun_burst:['Shotgun Burst','Develops the Shotgun Burst branch and its signature behavior.'],
  shotgun_cataclysm:['Shotgun Cataclysm','Develops the Shotgun Cataclysm branch and its signature behavior.'],
  shotgun_hail:['Shotgun Hail','Develops the Shotgun Hail branch and its signature behavior.'],
  chain_web:['Chain Web','Develops the Chain Web branch and its signature behavior.'],
  chain_storm:['Chain Storm','Develops the Chain Storm branch and its signature behavior.'],
  chain_leech:['Chain Leech','Develops the Chain Leech branch and its signature behavior.'],
  aura_sanctum:['Aura Sanctum','Develops the Aura Sanctum branch and its signature behavior.'],
  aura_gravity:['Aura Gravity','Develops the Aura Gravity branch and its signature behavior.'],
  aura_overgrowth:['Aura Overgrowth','Develops the Aura Overgrowth branch and its signature behavior.'],
  orbital_dance:['Orbital Dance','Develops the Orbital Dance branch and its signature behavior.'],
  orbital_halo:['Orbital Halo','Develops the Orbital Halo branch and its signature behavior.'],
  orbital_blade:['Orbital Blade','Develops the Orbital Blade branch and its signature behavior.'],
  prism_split:['Prism Split','Develops the Prism Split branch and its signature behavior.'],
  prism_spectrum:['Prism Spectrum','Develops the Prism Spectrum branch and its signature behavior.'],
  prism_mirror:['Prism Mirror','Develops the Prism Mirror branch and its signature behavior.'],
  gravity_well:['Gravity Well','Develops the Gravity Well branch and its signature behavior.'],
  gravity_tide:['Gravity Tide','Develops the Gravity Tide branch and its signature behavior.'],
  gravity_collapse:['Gravity Collapse','Develops the Gravity Collapse branch and its signature behavior.'],
  pulse_wave:['Pulse Wave','Develops the Pulse Wave branch and its signature behavior.'],
  pulse_resonator:['Pulse Resonator','Develops the Pulse Resonator branch and its signature behavior.'],
  pulse_burst:['Pulse Burst','Develops the Pulse Burst branch and its signature behavior.'],
  void_hunger:['Void Hunger','Develops the Void Hunger branch and its signature behavior.'],
  void_reaper:['Void Reaper','Develops the Void Reaper branch and its signature behavior.'],
  void_execution:['Void Execution','Develops the Void Execution branch and its signature behavior.'],
};

const SPHERE_FINAL_VARIANTS:Record<SphereEvolutionId,[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]>={
  standard_resonator:[
    f('standard_resonator_final_1','Резонатор • Ядро','Standard Resonator • Core','Финальная форма ветки «Резонатор»: Ядро.','Final form of the Standard Resonator branch: Core.'),
    f('standard_resonator_final_2','Резонатор • Каскад','Standard Resonator • Cascade','Финальная форма ветки «Резонатор»: Каскад.','Final form of the Standard Resonator branch: Cascade.'),
    f('standard_resonator_final_3','Резонатор • Апекс','Standard Resonator • Apex','Финальная форма ветки «Резонатор»: Апекс.','Final form of the Standard Resonator branch: Apex.'),
  ],
  standard_singularity:[
    f('standard_singularity_final_1','Сингулярность • Ядро','Standard Singularity • Core','Финальная форма ветки «Сингулярность»: Ядро.','Final form of the Standard Singularity branch: Core.'),
    f('standard_singularity_final_2','Сингулярность • Каскад','Standard Singularity • Cascade','Финальная форма ветки «Сингулярность»: Каскад.','Final form of the Standard Singularity branch: Cascade.'),
    f('standard_singularity_final_3','Сингулярность • Апекс','Standard Singularity • Apex','Финальная форма ветки «Сингулярность»: Апекс.','Final form of the Standard Singularity branch: Apex.'),
  ],
  standard_swarm:[
    f('standard_swarm_final_1','Рой • Ядро','Standard Swarm • Core','Финальная форма ветки «Рой»: Ядро.','Final form of the Standard Swarm branch: Core.'),
    f('standard_swarm_final_2','Рой • Каскад','Standard Swarm • Cascade','Финальная форма ветки «Рой»: Каскад.','Final form of the Standard Swarm branch: Cascade.'),
    f('standard_swarm_final_3','Рой • Апекс','Standard Swarm • Apex','Финальная форма ветки «Рой»: Апекс.','Final form of the Standard Swarm branch: Apex.'),
  ],
  sniper_oracle:[
    f('sniper_oracle_final_1','Оракул • Ядро','Sniper Oracle • Core','Финальная форма ветки «Оракул»: Ядро.','Final form of the Sniper Oracle branch: Core.'),
    f('sniper_oracle_final_2','Оракул • Каскад','Sniper Oracle • Cascade','Финальная форма ветки «Оракул»: Каскад.','Final form of the Sniper Oracle branch: Cascade.'),
    f('sniper_oracle_final_3','Оракул • Апекс','Sniper Oracle • Apex','Финальная форма ветки «Оракул»: Апекс.','Final form of the Sniper Oracle branch: Apex.'),
  ],
  sniper_assassin:[
    f('sniper_assassin_final_1','Убийца • Ядро','Sniper Assassin • Core','Финальная форма ветки «Убийца»: Ядро.','Final form of the Sniper Assassin branch: Core.'),
    f('sniper_assassin_final_2','Убийца • Каскад','Sniper Assassin • Cascade','Финальная форма ветки «Убийца»: Каскад.','Final form of the Sniper Assassin branch: Cascade.'),
    f('sniper_assassin_final_3','Убийца • Апекс','Sniper Assassin • Apex','Финальная форма ветки «Убийца»: Апекс.','Final form of the Sniper Assassin branch: Apex.'),
  ],
  sniper_beacon:[
    f('sniper_beacon_final_1','Маяк • Ядро','Sniper Beacon • Core','Финальная форма ветки «Маяк»: Ядро.','Final form of the Sniper Beacon branch: Core.'),
    f('sniper_beacon_final_2','Маяк • Каскад','Sniper Beacon • Cascade','Финальная форма ветки «Маяк»: Каскад.','Final form of the Sniper Beacon branch: Cascade.'),
    f('sniper_beacon_final_3','Маяк • Апекс','Sniper Beacon • Apex','Финальная форма ветки «Маяк»: Апекс.','Final form of the Sniper Beacon branch: Apex.'),
  ],
  shotgun_burst:[
    f('shotgun_burst_final_1','Разрыв • Ядро','Shotgun Burst • Core','Финальная форма ветки «Разрыв»: Ядро.','Final form of the Shotgun Burst branch: Core.'),
    f('shotgun_burst_final_2','Разрыв • Каскад','Shotgun Burst • Cascade','Финальная форма ветки «Разрыв»: Каскад.','Final form of the Shotgun Burst branch: Cascade.'),
    f('shotgun_burst_final_3','Разрыв • Апекс','Shotgun Burst • Apex','Финальная форма ветки «Разрыв»: Апекс.','Final form of the Shotgun Burst branch: Apex.'),
  ],
  shotgun_cataclysm:[
    f('shotgun_cataclysm_final_1','Осада • Ядро','Shotgun Cataclysm • Core','Финальная форма ветки «Осада»: Ядро.','Final form of the Shotgun Cataclysm branch: Core.'),
    f('shotgun_cataclysm_final_2','Осада • Каскад','Shotgun Cataclysm • Cascade','Финальная форма ветки «Осада»: Каскад.','Final form of the Shotgun Cataclysm branch: Cascade.'),
    f('shotgun_cataclysm_final_3','Осада • Апекс','Shotgun Cataclysm • Apex','Финальная форма ветки «Осада»: Апекс.','Final form of the Shotgun Cataclysm branch: Apex.'),
  ],
  shotgun_hail:[
    f('shotgun_hail_final_1','Град • Ядро','Shotgun Hail • Core','Финальная форма ветки «Град»: Ядро.','Final form of the Shotgun Hail branch: Core.'),
    f('shotgun_hail_final_2','Град • Каскад','Shotgun Hail • Cascade','Финальная форма ветки «Град»: Каскад.','Final form of the Shotgun Hail branch: Cascade.'),
    f('shotgun_hail_final_3','Град • Апекс','Shotgun Hail • Apex','Финальная форма ветки «Град»: Апекс.','Final form of the Shotgun Hail branch: Apex.'),
  ],
  chain_web:[
    f('chain_web_final_1','Паутина • Ядро','Chain Web • Core','Финальная форма ветки «Паутина»: Ядро.','Final form of the Chain Web branch: Core.'),
    f('chain_web_final_2','Паутина • Каскад','Chain Web • Cascade','Финальная форма ветки «Паутина»: Каскад.','Final form of the Chain Web branch: Cascade.'),
    f('chain_web_final_3','Паутина • Апекс','Chain Web • Apex','Финальная форма ветки «Паутина»: Апекс.','Final form of the Chain Web branch: Apex.'),
  ],
  chain_storm:[
    f('chain_storm_final_1','Шторм • Ядро','Chain Storm • Core','Финальная форма ветки «Шторм»: Ядро.','Final form of the Chain Storm branch: Core.'),
    f('chain_storm_final_2','Шторм • Каскад','Chain Storm • Cascade','Финальная форма ветки «Шторм»: Каскад.','Final form of the Chain Storm branch: Cascade.'),
    f('chain_storm_final_3','Шторм • Апекс','Chain Storm • Apex','Финальная форма ветки «Шторм»: Апекс.','Final form of the Chain Storm branch: Apex.'),
  ],
  chain_leech:[
    f('chain_leech_final_1','Паразит • Ядро','Chain Leech • Core','Финальная форма ветки «Паразит»: Ядро.','Final form of the Chain Leech branch: Core.'),
    f('chain_leech_final_2','Паразит • Каскад','Chain Leech • Cascade','Финальная форма ветки «Паразит»: Каскад.','Final form of the Chain Leech branch: Cascade.'),
    f('chain_leech_final_3','Паразит • Апекс','Chain Leech • Apex','Финальная форма ветки «Паразит»: Апекс.','Final form of the Chain Leech branch: Apex.'),
  ],
  aura_sanctum:[
    f('aura_sanctum_final_1','Святилище • Ядро','Aura Sanctum • Core','Финальная форма ветки «Святилище»: Ядро.','Final form of the Aura Sanctum branch: Core.'),
    f('aura_sanctum_final_2','Святилище • Каскад','Aura Sanctum • Cascade','Финальная форма ветки «Святилище»: Каскад.','Final form of the Aura Sanctum branch: Cascade.'),
    f('aura_sanctum_final_3','Святилище • Апекс','Aura Sanctum • Apex','Финальная форма ветки «Святилище»: Апекс.','Final form of the Aura Sanctum branch: Apex.'),
  ],
  aura_gravity:[
    f('aura_gravity_final_1','Гравитация • Ядро','Aura Gravity • Core','Финальная форма ветки «Гравитация»: Ядро.','Final form of the Aura Gravity branch: Core.'),
    f('aura_gravity_final_2','Гравитация • Каскад','Aura Gravity • Cascade','Финальная форма ветки «Гравитация»: Каскад.','Final form of the Aura Gravity branch: Cascade.'),
    f('aura_gravity_final_3','Гравитация • Апекс','Aura Gravity • Apex','Финальная форма ветки «Гравитация»: Апекс.','Final form of the Aura Gravity branch: Apex.'),
  ],
  aura_overgrowth:[
    f('aura_overgrowth_final_1','Живая сеть • Ядро','Aura Overgrowth • Core','Финальная форма ветки «Живая сеть»: Ядро.','Final form of the Aura Overgrowth branch: Core.'),
    f('aura_overgrowth_final_2','Живая сеть • Каскад','Aura Overgrowth • Cascade','Финальная форма ветки «Живая сеть»: Каскад.','Final form of the Aura Overgrowth branch: Cascade.'),
    f('aura_overgrowth_final_3','Живая сеть • Апекс','Aura Overgrowth • Apex','Финальная форма ветки «Живая сеть»: Апекс.','Final form of the Aura Overgrowth branch: Apex.'),
  ],
  orbital_dance:[
    f('orbital_dance_final_1','Танец • Ядро','Orbital Dance • Core','Финальная форма ветки «Танец»: Ядро.','Final form of the Orbital Dance branch: Core.'),
    f('orbital_dance_final_2','Танец • Каскад','Orbital Dance • Cascade','Финальная форма ветки «Танец»: Каскад.','Final form of the Orbital Dance branch: Cascade.'),
    f('orbital_dance_final_3','Танец • Апекс','Orbital Dance • Apex','Финальная форма ветки «Танец»: Апекс.','Final form of the Orbital Dance branch: Apex.'),
  ],
  orbital_halo:[
    f('orbital_halo_final_1','Ореол • Ядро','Orbital Halo • Core','Финальная форма ветки «Ореол»: Ядро.','Final form of the Orbital Halo branch: Core.'),
    f('orbital_halo_final_2','Ореол • Каскад','Orbital Halo • Cascade','Финальная форма ветки «Ореол»: Каскад.','Final form of the Orbital Halo branch: Cascade.'),
    f('orbital_halo_final_3','Ореол • Апекс','Orbital Halo • Apex','Финальная форма ветки «Ореол»: Апекс.','Final form of the Orbital Halo branch: Apex.'),
  ],
  orbital_blade:[
    f('orbital_blade_final_1','Клинок • Ядро','Orbital Blade • Core','Финальная форма ветки «Клинок»: Ядро.','Final form of the Orbital Blade branch: Core.'),
    f('orbital_blade_final_2','Клинок • Каскад','Orbital Blade • Cascade','Финальная форма ветки «Клинок»: Каскад.','Final form of the Orbital Blade branch: Cascade.'),
    f('orbital_blade_final_3','Клинок • Апекс','Orbital Blade • Apex','Финальная форма ветки «Клинок»: Апекс.','Final form of the Orbital Blade branch: Apex.'),
  ],
  prism_split:[
    f('prism_split_final_1','Расщепление • Ядро','Prism Split • Core','Финальная форма ветки «Расщепление»: Ядро.','Final form of the Prism Split branch: Core.'),
    f('prism_split_final_2','Расщепление • Каскад','Prism Split • Cascade','Финальная форма ветки «Расщепление»: Каскад.','Final form of the Prism Split branch: Cascade.'),
    f('prism_split_final_3','Расщепление • Апекс','Prism Split • Apex','Финальная форма ветки «Расщепление»: Апекс.','Final form of the Prism Split branch: Apex.'),
  ],
  prism_spectrum:[
    f('prism_spectrum_final_1','Спектр • Ядро','Prism Spectrum • Core','Финальная форма ветки «Спектр»: Ядро.','Final form of the Prism Spectrum branch: Core.'),
    f('prism_spectrum_final_2','Спектр • Каскад','Prism Spectrum • Cascade','Финальная форма ветки «Спектр»: Каскад.','Final form of the Prism Spectrum branch: Cascade.'),
    f('prism_spectrum_final_3','Спектр • Апекс','Prism Spectrum • Apex','Финальная форма ветки «Спектр»: Апекс.','Final form of the Prism Spectrum branch: Apex.'),
  ],
  prism_mirror:[
    f('prism_mirror_final_1','Зеркало • Ядро','Prism Mirror • Core','Финальная форма ветки «Зеркало»: Ядро.','Final form of the Prism Mirror branch: Core.'),
    f('prism_mirror_final_2','Зеркало • Каскад','Prism Mirror • Cascade','Финальная форма ветки «Зеркало»: Каскад.','Final form of the Prism Mirror branch: Cascade.'),
    f('prism_mirror_final_3','Зеркало • Апекс','Prism Mirror • Apex','Финальная форма ветки «Зеркало»: Апекс.','Final form of the Prism Mirror branch: Apex.'),
  ],
  gravity_well:[
    f('gravity_well_final_1','Колодец • Ядро','Gravity Well • Core','Финальная форма ветки «Колодец»: Ядро.','Final form of the Gravity Well branch: Core.'),
    f('gravity_well_final_2','Колодец • Каскад','Gravity Well • Cascade','Финальная форма ветки «Колодец»: Каскад.','Final form of the Gravity Well branch: Cascade.'),
    f('gravity_well_final_3','Колодец • Апекс','Gravity Well • Apex','Финальная форма ветки «Колодец»: Апекс.','Final form of the Gravity Well branch: Apex.'),
  ],
  gravity_tide:[
    f('gravity_tide_final_1','Прилив • Ядро','Gravity Tide • Core','Финальная форма ветки «Прилив»: Ядро.','Final form of the Gravity Tide branch: Core.'),
    f('gravity_tide_final_2','Прилив • Каскад','Gravity Tide • Cascade','Финальная форма ветки «Прилив»: Каскад.','Final form of the Gravity Tide branch: Cascade.'),
    f('gravity_tide_final_3','Прилив • Апекс','Gravity Tide • Apex','Финальная форма ветки «Прилив»: Апекс.','Final form of the Gravity Tide branch: Apex.'),
  ],
  gravity_collapse:[
    f('gravity_collapse_final_1','Коллапс • Ядро','Gravity Collapse • Core','Финальная форма ветки «Коллапс»: Ядро.','Final form of the Gravity Collapse branch: Core.'),
    f('gravity_collapse_final_2','Коллапс • Каскад','Gravity Collapse • Cascade','Финальная форма ветки «Коллапс»: Каскад.','Final form of the Gravity Collapse branch: Cascade.'),
    f('gravity_collapse_final_3','Коллапс • Апекс','Gravity Collapse • Apex','Финальная форма ветки «Коллапс»: Апекс.','Final form of the Gravity Collapse branch: Apex.'),
  ],
  pulse_wave:[
    f('pulse_wave_final_1','Волна • Ядро','Pulse Wave • Core','Финальная форма ветки «Волна»: Ядро.','Final form of the Pulse Wave branch: Core.'),
    f('pulse_wave_final_2','Волна • Каскад','Pulse Wave • Cascade','Финальная форма ветки «Волна»: Каскад.','Final form of the Pulse Wave branch: Cascade.'),
    f('pulse_wave_final_3','Волна • Апекс','Pulse Wave • Apex','Финальная форма ветки «Волна»: Апекс.','Final form of the Pulse Wave branch: Apex.'),
  ],
  pulse_resonator:[
    f('pulse_resonator_final_1','Резонатор • Ядро','Pulse Resonator • Core','Финальная форма ветки «Резонатор»: Ядро.','Final form of the Pulse Resonator branch: Core.'),
    f('pulse_resonator_final_2','Резонатор • Каскад','Pulse Resonator • Cascade','Финальная форма ветки «Резонатор»: Каскад.','Final form of the Pulse Resonator branch: Cascade.'),
    f('pulse_resonator_final_3','Резонатор • Апекс','Pulse Resonator • Apex','Финальная форма ветки «Резонатор»: Апекс.','Final form of the Pulse Resonator branch: Apex.'),
  ],
  pulse_burst:[
    f('pulse_burst_final_1','Вспышка • Ядро','Pulse Burst • Core','Финальная форма ветки «Вспышка»: Ядро.','Final form of the Pulse Burst branch: Core.'),
    f('pulse_burst_final_2','Вспышка • Каскад','Pulse Burst • Cascade','Финальная форма ветки «Вспышка»: Каскад.','Final form of the Pulse Burst branch: Cascade.'),
    f('pulse_burst_final_3','Вспышка • Апекс','Pulse Burst • Apex','Финальная форма ветки «Вспышка»: Апекс.','Final form of the Pulse Burst branch: Apex.'),
  ],
  void_hunger:[
    f('void_hunger_final_1','Голод • Ядро','Void Hunger • Core','Финальная форма ветки «Голод»: Ядро.','Final form of the Void Hunger branch: Core.'),
    f('void_hunger_final_2','Голод • Каскад','Void Hunger • Cascade','Финальная форма ветки «Голод»: Каскад.','Final form of the Void Hunger branch: Cascade.'),
    f('void_hunger_final_3','Голод • Апекс','Void Hunger • Apex','Финальная форма ветки «Голод»: Апекс.','Final form of the Void Hunger branch: Apex.'),
  ],
  void_reaper:[
    f('void_reaper_final_1','Жнец • Ядро','Void Reaper • Core','Финальная форма ветки «Жнец»: Ядро.','Final form of the Void Reaper branch: Core.'),
    f('void_reaper_final_2','Жнец • Каскад','Void Reaper • Cascade','Финальная форма ветки «Жнец»: Каскад.','Final form of the Void Reaper branch: Cascade.'),
    f('void_reaper_final_3','Жнец • Апекс','Void Reaper • Apex','Финальная форма ветки «Жнец»: Апекс.','Final form of the Void Reaper branch: Apex.'),
  ],
  void_execution:[
    f('void_execution_final_1','Экзекуция • Ядро','Void Execution • Core','Финальная форма ветки «Экзекуция»: Ядро.','Final form of the Void Execution branch: Core.'),
    f('void_execution_final_2','Экзекуция • Каскад','Void Execution • Cascade','Финальная форма ветки «Экзекуция»: Каскад.','Final form of the Void Execution branch: Cascade.'),
    f('void_execution_final_3','Экзекуция • Апекс','Void Execution • Apex','Финальная форма ветки «Экзекуция»: Апекс.','Final form of the Void Execution branch: Apex.'),
  ],
};

const finalsFor=(id:SphereEvolutionId):[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]=>{
  const finals=SPHERE_FINAL_VARIANTS[id];
  if(!finals||finals.length!==3) throw new Error('Sphere branch must expose exactly three final forms: '+id);
  return finals;
};

const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id] ?? NEW_BRANCH_LEVEL_DETAILS[id] ?? { level5: 'Усиление выбранной ветки.', level6: 'Дополнительное усиление уникальной механики.' };
  const [nameEn,descEn]=BRANCH_EN[id] ?? [ru,desc];
  return { ...e(id,ru,nameEn,desc,descEn), id, final:fin, level5:{ru:details.level5,en:details.level5}, level6:{ru:details.level6,en:details.level6} };
};
const sphere=(type:SphereType,name:string,priority:Partial<Record<CharacterId,number>>,l:[string,string,string],branches:[SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch]):SphereDef=>({type,name:SPHERE_TYPES[type]?.name ?? {ru:name,en:name},priority,levels:lv(...l),evolution4:branches[0],evolution7:branches[0].final[0],evolution4Choices:branches});
const genericSphereBranches = (type: SphereType, names: [string,string,string], ids: [SphereEvolutionId,SphereEvolutionId,SphereEvolutionId]): [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch] =>
  ids.map((id, i) => br(id, names[i], 'Развивает уникальную механику сферы '+type+'.', finals(names[i], names[i], ids))) as [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch];

export const SPHERE_PROGRESSION:Record<SphereType,SphereDef>={
 standard:sphere('standard','Стандартная',{spherist:1,engineer:.9,berserker:.8,architect:.7,hunter:.4,alchemist:.4},['+15% урона','+1 пробитие','-10% задержки'],[br('standard_resonator','Резонатор','Каждое третье попадание выпускает импульс',finalsFor('Резонатор')),br('standard_singularity','Сингулярность','Попадания притягивают врагов',finalsFor('Сингулярность')),br('standard_swarm','Рой','Попадания выпускают осколки',finalsFor('Рой'))]),
 sniper:sphere('sniper','Снайперская',{hunter:1,architect:.9,spherist:.4,engineer:.4,berserker:.3,alchemist:.3},['+25% урона','+15% дальности','+15% крита'],[br('sniper_oracle','Оракул','Усиливает критический урон по отмеченным целям',finalsFor('Оракул')),br('sniper_assassin','Убийца','Усиливает урон по слабым целям',finalsFor('Убийца')),br('sniper_beacon','Маяк','Помечает цель для всей сети',finalsFor('Маяк'))]),
 shotgun:sphere('shotgun','Дробовик',{berserker:1,alchemist:.8,spherist:.6,engineer:.4,hunter:.3,architect:.3},['+1 дробь','+20% урона вблизи','-12% разброса'],[br('shotgun_burst','Разрыв','Ближние попадания наносят повышенный урон',finalsFor('Разрыв')),br('shotgun_cataclysm','Осада','Тяжёлые пробивные снаряды',finalsFor('Осада')),br('shotgun_hail','Град','Много дополнительных снарядов',finalsFor('Град'))]),
 chain:sphere('chain','Цепная',{spherist:1,hunter:.95,engineer:.9,alchemist:.8,architect:.5,berserker:.4},['+1 цель цепи','+10% урона цепи','+15% скорости перехода'],[br('chain_web','Паутина','Поражённые цели получают усиленное замедление',finalsFor('Паутина')),br('chain_storm','Шторм','Каждый переход усиливает следующий',finalsFor('Шторм')),br('chain_leech','Паразит','Цепь возвращает часть урона',finalsFor('Паразит'))]),
 aura:sphere('aura','Аура',{engineer:1,alchemist:1,architect:.9,spherist:.7,hunter:.4,berserker:.4},['+20% радиуса ауры','-10% интервала импульса','+10% урона ауры'],[br('aura_sanctum','Святилище','Замедляет врагов и усиливает сферы',finalsFor('Святилище')),br('aura_gravity','Гравитация','Стягивает врагов к центру',finalsFor('Гравитация')),br('aura_overgrowth','Живая сеть','Усиливает сферы внутри ауры',finalsFor('Живая сеть'))]),
 orbital:sphere('orbital','Орбитальная',{spherist:1,engineer:.8,architect:.8},['+15% орбитального урона','+15% радиуса орбиты','-12% интервала'],[br('orbital_dance','Танец','Спутники вращаются быстрее и наносят урон при каждом пересечении траектории с врагом.',finalsFor('Танец')),br('orbital_halo','Ореол','Спутники создают защитный ореол: ближайшие сферы получают усиление после прохода орбиты.',finalsFor('Ореол')),br('orbital_blade','Клинок','Спутники превращаются в боевые лезвия и наносят повышенный урон по траектории.',finalsFor('Клинок'))]),
 prism:sphere('prism','Призма',{hunter:1,architect:.9,spherist:.7},['+20% урона луча','+15% дальности','+1 направление'],[br('prism_split','Расщепление','Луч после попадания делится на дополнительные лучи по другим целям.',finalsFor('Расщепление')),br('prism_spectrum','Спектр','Луч передаёт активный статусный эффект и усиливает реакцию на цели.',finalsFor('Спектр')),br('prism_mirror','Зеркало','Связанные сферы создают отражённые лучи, повторяющие основной удар.',finalsFor('Зеркало'))]),
 gravity:sphere('gravity','Гравитационная',{alchemist:1,architect:1,engineer:.8},['+20% силы притяжения','+15% радиуса','-15% интервала импульса'],[br('gravity_well','Колодец','Притяжение становится постоянным: чем ближе враг к центру, тем сильнее его тянет.',finalsFor('Колодец')),br('gravity_tide','Прилив','Поле плавно меняет силу притяжения и периодически создаёт обратную волну.',finalsFor('Прилив')),br('gravity_collapse','Коллапс','Собранные в плотную группу враги получают дополнительный урон от сжатия.',finalsFor('Коллапс'))]),
 pulse:sphere('pulse','Импульсная',{engineer:1,spherist:.9,architect:.8},['+20% импульсного урона','+15% радиуса','-12% интервала'],[br('pulse_wave','Волна','Каждый импульс становится шире и отбрасывает врагов от сферы.',finalsFor('Волна')),br('pulse_resonator','Резонатор','Импульсы подпитывают Resonance и усиливают сеть при активной геометрии.',finalsFor('Резонатор')),br('pulse_burst','Вспышка','После основной волны возникает дополнительный разряд по центру.',finalsFor('Вспышка'))]),
 void:sphere('void','Пустотная',{hunter:1,alchemist:.8,architect:.7},['+20% урона по ослабленным','+10% шанс критического добивания','+15% дальности'],[br('void_hunger','Голод','Урон растёт по мере потери здоровья целью, превращая Void в добивающую сферу.',finalsFor('Голод')),br('void_reaper','Жнец','Убийства Void возвращают HP и создают осколки пустоты для продолжения атаки.',finalsFor('Жнец')),br('void_execution','Экзекуция','Слабые цели получают шанс на мгновенное добивание, а финальная форма повышает порог исполнения.',finalsFor('Экзекуция'))])
};
const abilityLevels=(a:string,b:string,c:string,d:string,e:string,f:string):SphereUpgradeDef[] => [
  {level:1,name:{ru:'Пробуждение',en:'Awakening'},desc:{ru:a,en:a}},
  {level:2,name:{ru:'Настройка',en:'Tuning'},desc:{ru:b,en:b}},
  {level:3,name:{ru:'Раскрытие',en:'Expansion'},desc:{ru:c,en:c}},
  {level:4,name:{ru:'Мутация I',en:'Mutation I'},desc:{ru:'Следующий выбор откроет одну из трёх веток способности',en:'The next choice opens one of three ability branches'}},
  {level:5,name:{ru:'Развитие ветки',en:'Branch Development'},desc:{ru:d,en:d}},
  {level:6,name:{ru:'Синхронизация ветки',en:'Branch Synchronization'},desc:{ru:e,en:e}},
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
}

const ae=(id:string,ru:string,desc:string):AbilityEvolutionChoice=>({id,name:{ru,en:ru},desc:{ru:desc,en:desc}});

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
      ae('timestop_echo_phase','Фазовый разрыв','Сферы продолжают атаковать во время остановки.'),
      ae('timestop_closed_time','Замкнутое время','Зона остановки расширяется через сеть.'),
      ae('timestop_time_anchor','Якорь времени','Остановка фиксирует врагов вокруг ближайшей сферы.')
    ],
    evolution7:[
      ae('timestop_outside_time','Вне времени','Попадания сфер обновляют заморозку врагов во время остановки.'),
      ae('timestop_closed_network','Closed Time Network','Сеть сохраняет эффект остановки между узлами.'),
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

export function getAbilityEvolutionChoice(s:any, ability:AbilityType, stage:4|7):AbilityEvolutionChoice|null {
  const prefix='ability:'+ability+':'+stage+':';
  const marker=(s.player.evolutions||[]).find((x:string)=>x.startsWith(prefix));
  if(!marker) return null;
  const id=marker.slice(prefix.length);
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression) return null;
  const pool=stage===4?progression.evolution4:progression.evolution7;
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

export type SphereAbilitySynergy = {
  character: CharacterId;
  sphere: SphereType;
  ability: AbilityType;
  name:{ru:string;en:string};
  desc:{ru:string;en:string};
  effect:'damage'|'attackSpeed'|'radius'|'chain'|'defense';
};

export const SPHERE_ABILITY_SYNERGIES: SphereAbilitySynergy[] = [
  {character:'spherist',sphere:'pulse',ability:'blast',name:{ru:'Резонансная волна',en:'Resonance Wave'},desc:{ru:'Blast усиливает следующую Pulse-волну.',en:'Blast empowers the next Pulse wave.'},effect:'damage'},
  {character:'spherist',sphere:'orbital',ability:'shield',name:{ru:'Орбитальный купол',en:'Orbital Dome'},desc:{ru:'Shield усиливает орбитальный проход.',en:'Shield empowers an orbital pass.'},effect:'defense'},
  {character:'hunter',sphere:'void',ability:'crit',name:{ru:'Палач пустоты',en:'Void Executioner'},desc:{ru:'Void сильнее добивает отмеченные цели.',en:'Void executes marked targets harder.'},effect:'damage'},
  {character:'hunter',sphere:'prism',ability:'crit',name:{ru:'Призматический прицел',en:'Prism Scope'},desc:{ru:'Критический Prism-луч усиливается по приоритетной цели.',en:'Critical Prism beams hit priority targets harder.'},effect:'damage'},
  {character:'engineer',sphere:'orbital',ability:'minion',name:{ru:'Орбитальное реле',en:'Orbital Relay'},desc:{ru:'Дроны ускоряют следующий орбитальный проход.',en:'Drones accelerate the next orbital pass.'},effect:'attackSpeed'},
  {character:'engineer',sphere:'pulse',ability:'lightning',name:{ru:'Импульсный проводник',en:'Pulse Conductor'},desc:{ru:'Разряд усиливает следующую Pulse-волну.',en:'Lightning empowers the next Pulse wave.'},effect:'damage'},
  {character:'alchemist',sphere:'gravity',ability:'firetrail',name:{ru:'Алхимический колодец',en:'Alchemy Well'},desc:{ru:'Gravity удерживает врагов внутри статусной зоны.',en:'Gravity keeps enemies inside the status field.'},effect:'radius'},
  {character:'alchemist',sphere:'prism',ability:'firetrail',name:{ru:'Призматический катализатор',en:'Prism Catalyst'},desc:{ru:'Статусы Prism сильнее запускают реакции.',en:'Prism statuses trigger stronger reactions.'},effect:'damage'},
  {character:'architect',sphere:'prism',ability:'timestop',name:{ru:'Геометрический луч',en:'Geometric Ray'},desc:{ru:'Time Stop усиливает отражения Prism.',en:'Time Stop empowers Prism reflections.'},effect:'radius'},
  {character:'architect',sphere:'gravity',ability:'timestop',name:{ru:'Матрица притяжения',en:'Gravity Matrix'},desc:{ru:'Time Stop расширяет контроль Gravity.',en:'Time Stop expands Gravity control.'},effect:'radius'},
  {character:'berserker',sphere:'orbital',ability:'shield',name:{ru:'Боевой ореол',en:'Battle Halo'},desc:{ru:'Shield превращает Orbital в более устойчивый источник урона.',en:'Shield makes Orbital a sturdier damage source.'},effect:'defense'},
  {character:'berserker',sphere:'void',ability:'darkritual',name:{ru:'Пустой ритуал',en:'Void Ritual'},desc:{ru:'Dark Ritual усиливает Void по ослабленным целям.',en:'Dark Ritual empowers Void against weakened targets.'},effect:'damage'},
  {character:'spherist',sphere:'standard',ability:'blast',name:{ru:'Резонансное ядро',en:'Resonant Core'},desc:{ru:'Blast проходит через Standard и передаёт импульс ближайшей сфере.',en:'Blast travels through Standard and relays to the nearest sphere.'},effect:'damage'},
  {character:'spherist',sphere:'chain',ability:'lightning',name:{ru:'Грозовая сеть',en:'Thunder Network'},desc:{ru:'Lightning проходит по Chain-сферам и усиливается на каждом узле.',en:'Lightning travels through Chain spheres and grows at each node.'},effect:'chain'},
  {character:'hunter',sphere:'sniper',ability:'crit',name:{ru:'Executioner',en:'Executioner'},desc:{ru:'Sniper-криты по отмеченным целям наносят дополнительный урон.',en:'Sniper criticals against marked targets deal extra damage.'},effect:'damage'},
  {character:'hunter',sphere:'chain',ability:'teleport',name:{ru:'Predator Chain',en:'Predator Chain'},desc:{ru:'Телепорт к Chain переносит Метку добычи на всю цепь.',en:'Teleporting to Chain spreads Prey Mark through the chain.'},effect:'chain'},
  {character:'engineer',sphere:'standard',ability:'blast',name:{ru:'Network Core',en:'Network Core'},desc:{ru:'Blast становится релейным узлом сети Standard.',en:'Blast becomes a relay node for the Standard network.'},effect:'damage'},
  {character:'engineer',sphere:'chain',ability:'lightning',name:{ru:'Relay Storm',en:'Relay Storm'},desc:{ru:'Lightning активирует соседние Chain-узлы.',en:'Lightning activates adjacent Chain nodes.'},effect:'chain'},
  {character:'alchemist',sphere:'aura',ability:'firetrail',name:{ru:'Catalyst Field',en:'Catalyst Field'},desc:{ru:'Firetrail внутри Aura усиливает статусные реакции.',en:'Firetrail inside Aura amplifies status reactions.'},effect:'radius'},
  {character:'alchemist',sphere:'chain',ability:'lightning',name:{ru:'Toxic Network',en:'Toxic Network'},desc:{ru:'Разряды по Chain-целям продлевают их статусы.',en:'Chain strikes extend status effects.'},effect:'chain'},
  {character:'architect',sphere:'standard',ability:'blast',name:{ru:'Geometric Core',en:'Geometric Core'},desc:{ru:'Blast использует геометрические связи Standard-сфер.',en:'Blast uses geometric links between Standard spheres.'},effect:'damage'},
  {character:'architect',sphere:'aura',ability:'timestop',name:{ru:'Field Matrix',en:'Field Matrix'},desc:{ru:'Time Stop расширяет активную геометрическую формацию.',en:'Time Stop expands the active geometric formation.'},effect:'radius'},
  {character:'berserker',sphere:'shotgun',ability:'shield',name:{ru:'Barrier Core',en:'Barrier Core'},desc:{ru:'Shield превращает Shotgun-сферы в ударный барьер.',en:'Shield turns Shotgun spheres into an impact barrier.'},effect:'defense'},
  {character:'berserker',sphere:'standard',ability:'darkritual',name:{ru:'Blood Resonance',en:'Blood Resonance'},desc:{ru:'Dark Ritual усиливает Standard в ближнем бою.',en:'Dark Ritual empowers Standard at close range.'},effect:'damage'},
];

export const CHARACTER_SPHERE_PRIORITY:Record<CharacterId,SphereType[]>={
  spherist:['standard','chain','orbital','pulse'],
  hunter:['sniper','chain','void','prism'],
  engineer:['aura','orbital','pulse','gravity'],
  berserker:['shotgun','standard','orbital','void'],
  alchemist:['aura','chain','gravity','pulse'],
  architect:['prism','sniper','gravity','orbital'],
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
  standard_swarm:{split:1},
  sniper_oracle:{mark:1},
  sniper_assassin:{execute:1,breach:1},
  sniper_beacon:{anchor:1,mark:1},
  shotgun_burst:{multishot:1,impact:1},
  shotgun_cataclysm:{shatter:1,impact:1},
  shotgun_hail:{split:1,multishot:1},
  chain_web:{anchor:1},
  chain_storm:{static:1,resonant:1},
  chain_leech:{vampiric:1},
  aura_sanctum:{freeze:1,anchor:1},
  aura_gravity:{gravitic:1,anchor:1},
  aura_overgrowth:{resonant:1},
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
  pulse_burst:{shatter:1,impact:1},
  void_hunger:{corrupt:1},
  void_reaper:{vampiric:1},
  void_execution:{execute:1,phase:1},
};

const AUTHORED_FINAL_MODIFIERS: Partial<Record<SphereEvolutionId, Partial<Record<0|1|2, AuthoredModifierLevels>>>> = {
  standard_resonator:{0:{shatter:1},1:{impact:1},2:{anchor:1}},
  standard_singularity:{0:{gravitic:2},1:{impact:1},2:{gravitic:2}},
  standard_swarm:{0:{split:2},1:{split:2},2:{split:2}},
  sniper_oracle:{0:{mark:2},1:{mark:2,resonant:1},2:{mark:2,execute:1}},
  sniper_assassin:{0:{execute:2},1:{execute:2},2:{execute:2}},
  sniper_beacon:{0:{anchor:2},1:{mark:2},2:{anchor:2,mark:2}},
  shotgun_burst:{0:{multishot:2},1:{impact:2},2:{multishot:2,impact:2}},
  shotgun_cataclysm:{0:{shatter:2},1:{impact:2},2:{shatter:2,impact:2}},
  shotgun_hail:{0:{split:2},1:{multishot:2},2:{split:2,multishot:2}},
  chain_web:{0:{anchor:2},1:{static:1},2:{anchor:2,static:1}},
  chain_storm:{0:{static:2},1:{resonant:2},2:{static:2,resonant:2}},
  chain_leech:{0:{vampiric:2},1:{drain:1},2:{vampiric:2,drain:1}},
  aura_sanctum:{0:{freeze:2},1:{anchor:2},2:{freeze:2,anchor:2}},
  aura_gravity:{0:{gravitic:2},1:{anchor:2},2:{gravitic:2,anchor:2}},
  aura_overgrowth:{0:{resonant:2},1:{resonant:2},2:{resonant:2}},
  orbital_dance:{0:{afterimage:2},1:{afterimage:2},2:{afterimage:2}},
  orbital_halo:{0:{resonant:1},1:{resonant:1},2:{resonant:2}},
  orbital_blade:{0:{impact:2},1:{afterimage:2},2:{impact:2,afterimage:2}},
  prism_split:{0:{multishot:2},1:{multishot:2},2:{multishot:2}},
  prism_spectrum:{0:{fire:1},1:{freeze:1},2:{poison:1}},
  prism_mirror:{0:{ricochet:2},1:{ricochet:2},2:{ricochet:2}},
  gravity_well:{0:{anchor:2},1:{gravitic:2},2:{anchor:2,gravitic:2}},
  gravity_tide:{0:{impact:2},1:{gravitic:2},2:{impact:2,gravitic:2}},
  gravity_collapse:{0:{execute:2},1:{gravitic:2},2:{execute:2,gravitic:2}},
  pulse_wave:{0:{impact:2},1:{freeze:1},2:{impact:2,freeze:1}},
  pulse_resonator:{0:{resonant:2},1:{resonant:2},2:{resonant:2,shatter:1}},
  pulse_burst:{0:{shatter:2},1:{impact:2},2:{shatter:2,impact:2}},
  void_hunger:{0:{corrupt:2},1:{corrupt:2},2:{corrupt:2,drain:1}},
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

export function getActiveSphereAbilitySynergies(s:any): SphereAbilitySynergy[] {
  const character = s.player.characterId as CharacterId;
  return SPHERE_ABILITY_SYNERGIES.filter(link =>
    link.character === character &&
    sphereLevel(s, link.sphere) >= 7 &&
    (s.player.abilities?.[link.ability] || 0) >= 7
  );
}

export function sphereModifiers(s:any,type:SphereType,sphere?:any){
  const l=sphereLevel(s,type), branch=s.player.sphereBranches?.[type], final=sphereFinalIndex(s,type), artifact=getSphereArtifactModifiers(s,type,sphere);
  const authored=getAuthoredSphereModifierLevels(s.player,type);
  const modifierLevel=(kind:keyof SphereMods):number=>Math.max(Number(s.player.sphereMods?.[kind] ?? 0),Number(authored[kind] ?? 0));
  let damage=1, radius=1, delay=1, pierce=0, multishot=0, chainTargets=1, auraRadius=1, auraPulse=.5;
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
  if(type==='aura'&&branch==='aura_gravity'){auraRadius*=1.10;if(final===0)auraRadius*=1.18;}
  if(type==='aura'&&branch==='aura_overgrowth'){damage*=1.06;auraRadius*=1.08;if(final===0)damage*=1.15;}

  // NEW FIVE: their level 1-3 upgrades must modify real combat parameters.
  if(type==='orbital'){
    // Every level visibly adds one rotating satellite. Cadence is controlled by angular speed.
    if(l>=1) damage*=1.15;
    if(l>=2) radius*=1.15;
    // Orbital satellites are native to the Sphere, not the projectile MULTISHOT modifier.
    if(branch==='orbital_dance'){ delay*=final===0?0.80:0.90; }
    if(branch==='orbital_halo'){ damage*=0.92; }
    if(branch==='orbital_blade'){ damage*=final===0?1.15:1.05; }
  }
  if(type==='prism'){
    if(l>=1) damage*=1.20;
    if(l>=2) radius*=1.15;
    if(l>=3) multishot+=1;
    if(branch==='prism_split'){ multishot+=final===1?1:0; }
    if(branch==='prism_spectrum'){ damage*=0.98; }
    if(branch==='prism_mirror'){ radius*=1.08; }
  }
  if(type==='gravity'){
    if(l>=1) damage*=1.20;
    if(l>=2) radius*=1.15;
    if(l>=3) auraPulse*=0.85;
    if(branch==='gravity_well'){ auraRadius*=final===1?1.20:1.08; }
    if(branch==='gravity_tide'){ auraPulse*=0.90; }
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
    if(l>=1) damage*=1.20;
    if(l>=3) radius*=1.15;
    if(branch==='void_hunger'){ damage*=1.05; }
    if(branch==='void_reaper'){ damage*=1.03; }
    if(branch==='void_execution'){ damage*=final===2?1.10:1.04; }
  }

  for (const link of getActiveSphereAbilitySynergies(s)) {
    if (link.sphere !== type) continue;
    if (link.effect === 'damage') damage *= 1.12;
    if (link.effect === 'attackSpeed') delay *= 0.88;
    if (link.effect === 'radius') radius *= 1.12;
    if (link.effect === 'chain') chainTargets += 1;
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
    afterimage: modifierLevel('afterimage'),
    impact: modifierLevel('impact'),
    gravitic: modifierLevel('gravitic'),
  };
}