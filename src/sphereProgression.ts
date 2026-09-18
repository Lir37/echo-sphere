import type { CharacterId } from './characters';
import { getSphereArtifactModifiers } from './artifactSystem';
import { ABILITIES, type SphereType, type AbilityType } from './gameData';

export type SphereEvolutionId = 'standard_resonator' | 'standard_singularity' | 'standard_swarm' | 'sniper_oracle' | 'sniper_assassin' | 'sniper_beacon' | 'shotgun_burst' | 'shotgun_cataclysm' | 'shotgun_hail' | 'chain_web' | 'chain_storm' | 'chain_leech' | 'aura_sanctum' | 'aura_gravity' | 'aura_overgrowth';
export type AbilityEvolutionId = string;
export interface SphereUpgradeDef { level:number; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionDef { id:SphereEvolutionId; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionBranch extends SphereEvolutionDef {
  final:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef];
  level5:{ru:string;en:string};
  level6:{ru:string;en:string};
}
export interface SphereDef { type:SphereType; name:{ru:string;en:string}; priority:Partial<Record<CharacterId,number>>; levels:SphereUpgradeDef[]; evolution4:SphereEvolutionDef; evolution7:SphereEvolutionDef; evolution4Choices:SphereEvolutionBranch[]; }
const lv=(a:string,b:string,c:string):SphereUpgradeDef[]=>[{level:1,name:{ru:'Ядро',en:'Core'},desc:{ru:a,en:a}},{level:2,name:{ru:'Механизм',en:'Mechanism'},desc:{ru:b,en:b}},{level:3,name:{ru:'Настройка',en:'Tuning'},desc:{ru:c,en:c}},{level:4,name:{ru:'Эволюция I',en:'Evolution I'},desc:{ru:'Выбор одной из трёх веток',en:'Choose one of three branches'}},{level:5,name:{ru:'Контур',en:'Circuit'},desc:{ru:'Усиление выбранной ветки',en:'Strengthens the selected branch'}},{level:6,name:{ru:'Стабилизатор',en:'Stabilizer'},desc:{ru:'Усиление специальной механики',en:'Strengthens the special mechanic'}},{level:7,name:{ru:'Эволюция II',en:'Evolution II'},desc:{ru:'Финальная специализация',en:'Final specialization'}}];
const e=(id:SphereEvolutionId,ru:string,desc:string):SphereEvolutionDef=>({id,name:{ru,en:ru},desc:{ru:desc,en:desc}});
const BRANCH_LEVEL_DETAILS:Record<SphereEvolutionId,{level5:string;level6:string}>={
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
const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id];
  return {
    ...e(id,ru,desc),
    final:fin,
    level5:{ru:details.level5,en:details.level5},
    level6:{ru:details.level6,en:details.level6},
  };
};
const finals=(base:string,prefix:string,ids:[SphereEvolutionId,SphereEvolutionId,SphereEvolutionId]):[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]=>{
  const descriptions:Partial<Record<SphereEvolutionId,[string,string,string]>>={
    standard_resonator:['Каждое третье попадание создаёт мощный импульс по группе врагов','Импульс становится шире и отбрасывает врагов','Импульс накладывает замедление на поражённых врагов'],
    standard_singularity:['Попадания создают гравитационный коллапс и стягивают врагов','Сильнее замедляет врагов и дольше удерживает их','Стягивает врагов и наносит дополнительный урон ослабленным целям'],
    standard_swarm:['Периодически выпускает дополнительные боковые осколки','Чаще выпускает боковые осколки','Выпускает два боковых осколка при каждом срабатывании'],
    sniper_oracle:['По отмеченной цели наносится значительно усиленный урон','Усиленный урон становится стабильнее и выше','Усиленный выстрел дополнительно задевает соседних врагов'],
    sniper_assassin:['Урон по врагам ниже 35% здоровья резко возрастает','Низкоуровневые цели получают ещё более сильный добивающий урон','Добивающие попадания дополнительно восстанавливают HP'],
    sniper_beacon:['Попадание создаёт зону метки, замедляющую ближайших врагов','Зона метки становится значительно больше и дольше действует','Метка сильнее замедляет ближайших врагов'],
    shotgun_burst:['Ближние попадания получают дополнительный множитель урона','Бонус ближнего боя становится сильнее и работает дальше','Ближние попадания дополнительно замедляют врагов'],
    shotgun_cataclysm:['Попадание вызывает взрыв, поражающий соседних врагов','Взрыв становится значительно больше и сильнее','Взрыв дополнительно замедляет поражённых врагов'],
    shotgun_hail:['Попадания периодически создают дополнительный град осколков','Град срабатывает чаще и поражает большую область','Град становится сильнее и усиливает прямой урон'],
    chain_web:['Цели цепи получают длительное замедление, формируя паутину','Замедление становится значительно сильнее и дольше','По отмеченным цепью целям наносится дополнительный урон'],
    chain_storm:['Каждый переход цепи вызывает дополнительный электрический всплеск','Всплеск становится сильнее и поражает большую область','Цепь получает дополнительный множитель урона'],
    chain_leech:['Цепь возвращает больше здоровья при нанесении урона','Цепь восстанавливает ещё больше здоровья','По ослабленным целям цепь наносит дополнительный урон'],
    aura_sanctum:['Импульс ауры накладывает особо сильное замедление','Замедление становится очень сильным и длительным','Импульс дополнительно усиливает собственный урон'],
    aura_gravity:['Импульс создаёт мощный гравитационный толчок к центру','Гравитация действует на большую область и сильнее стягивает врагов','Сфера получает дополнительный урон вместо притяжения'],
    aura_overgrowth:['Ближайшие сферы получают заметное ускорение атак','Сферы в большем радиусе получают ещё большее ускорение','Ускорение атак сопровождается бонусом собственного урона'],
  };
  return ids.map((id,i)=>e(id,prefix+' '+['I','II','III'][i],(descriptions[id]||['Финально усиливает ветку «'+base+'»','Расширяет механику ветки «'+base+'»','Даёт альтернативную специализацию ветки «'+base+'»'])[i])) as [SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef];
};
const sphere=(type:SphereType,name:string,priority:Partial<Record<CharacterId,number>>,l:[string,string,string],branches:[SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch]):SphereDef=>({type,name:{ru:name,en:name},priority,levels:lv(...l),evolution4:branches[0],evolution7:branches[0].final[0],evolution4Choices:branches});
export const SPHERE_PROGRESSION:Record<SphereType,SphereDef>={
 standard:sphere('standard','Стандартная',{spherist:1,engineer:.9,berserker:.8,architect:.7,hunter:.4,alchemist:.4},['+15% урона','+1 пробитие','-10% задержки'],[br('standard_resonator','Резонатор','Каждое третье попадание выпускает импульс',finals('Резонатор','Гиперрезонатор',['standard_resonator','standard_singularity','standard_swarm'])),br('standard_singularity','Сингулярность','Попадания притягивают врагов',finals('Сингулярность','Коллапс',['standard_singularity','standard_resonator','standard_swarm'])),br('standard_swarm','Рой','Попадания выпускают осколки',finals('Рой','Каскад',['standard_swarm','standard_singularity','standard_resonator']))]),
 sniper:sphere('sniper','Снайперская',{hunter:1,architect:.9,spherist:.4,engineer:.4,berserker:.3,alchemist:.3},['+25% урона','+15% дальности','+15% крита'],[br('sniper_oracle','Оракул','Критует отмеченные цели',finals('Оракул','Провидец',['sniper_oracle','sniper_assassin','sniper_beacon'])),br('sniper_assassin','Убийца','Усиливает урон по слабым целям',finals('Убийца','Казнь',['sniper_assassin','sniper_oracle','sniper_beacon'])),br('sniper_beacon','Маяк','Помечает цель для всей сети',finals('Маяк','Всевидящее око',['sniper_beacon','sniper_assassin','sniper_oracle']))]),
 shotgun:sphere('shotgun','Дробовик',{berserker:1,alchemist:.8,spherist:.6,engineer:.4,hunter:.3,architect:.3},['+1 дробь','+20% урона вблизи','-12% разброса'],[br('shotgun_burst','Разрыв','Центральные дробины сильнее',finals('Разрыв','Катаклизм',['shotgun_burst','shotgun_cataclysm','shotgun_hail'])),br('shotgun_cataclysm','Осада','Тяжёлые пробивные снаряды',finals('Осада','Удар',['shotgun_cataclysm','shotgun_burst','shotgun_hail'])),br('shotgun_hail','Град','Много дополнительных снарядов',finals('Град','Ливень',['shotgun_hail','shotgun_burst','shotgun_cataclysm']))]),
 chain:sphere('chain','Цепная',{spherist:1,hunter:.95,engineer:.9,alchemist:.8,architect:.5,berserker:.4},['+1 цель цепи','+10% урона цепи','+15% скорости перехода'],[br('chain_web','Паутина','Цели остаются связанными',finals('Паутина','Сеть Эха',['chain_web','chain_storm','chain_leech'])),br('chain_storm','Шторм','Каждый переход усиливает следующий',finals('Шторм','Разряд',['chain_storm','chain_web','chain_leech'])),br('chain_leech','Паразит','Цепь возвращает часть урона',finals('Паразит','Пожиратель',['chain_leech','chain_storm','chain_web']))]),
 aura:sphere('aura','Аура',{engineer:1,alchemist:1,architect:.9,spherist:.7,hunter:.4,berserker:.4},['+20% радиуса ауры','-10% интервала импульса','+10% урона ауры'],[br('aura_sanctum','Святилище','Замедляет врагов и усиливает сферы',finals('Святилище','Эхо-святилище',['aura_sanctum','aura_gravity','aura_overgrowth'])),br('aura_gravity','Гравитация','Стягивает врагов к центру',finals('Гравитация','Сингулярность',['aura_gravity','aura_sanctum','aura_overgrowth'])),br('aura_overgrowth','Живая сеть','Усиливает сферы внутри ауры',finals('Живая сеть','Рост',['aura_overgrowth','aura_sanctum','aura_gravity']))])
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
      'Волна наносит 40 урона всем врагам в радиусе 200 px. Перезарядка: 30 с.',
      'Урон волны: 50. Радиус: 200 px. Перезарядка: 28 с.',
      'Урон волны: 60. Радиус: 200 px. Перезарядка: 26 с.',
      'Волна начинает взаимодействовать со сферами и выбранной веткой мутации.',
      'Ветка усиливается: импульсы и взаимодействия с сетью сфер происходят чаще и сильнее.',
      'Механика выбранной ветки получает финальное усиление перед второй мутацией.'
    ),
    evolution4:[
      ae('blast_resonance','Резонансный импульс','Импульс отскакивает от Standard-сфер и возвращается в сеть.'),
      ae('blast_network','Сетевой импульс','Каждая связанная сфера становится узлом передачи волны, позволяя продолжить цепочку.'),
      ae('blast_core','Ядро взрыва','Центральная точка волны получает дополнительный урон и становится главным источником взрыва.')
    ],
    evolution7:[
      ae('blast_echo_network','Echo Network','Волна проходит по цепочке сфер и усиливается на каждом посещённом узле.'),
      ae('blast_resonant_core','Resonant Core','Каждая Standard-сфера добавляет к волне дополнительный резонансный импульс.'),
      ae('blast_infinite_pulse','Infinite Pulse','Последний узел цепочки создаёт обратный импульс, возвращающий волну в сеть.')
    ]
  },
  shield:{
    ability:'shield',
    levels:abilityLevels(
      'Щит поглощает 1 удар. Длительность: 10 с. Перезарядка: 20 с.',
      'Щит поглощает 1 удар. Длительность: 10 с. Перезарядка: 20 с.',
      'Щит поглощает 2 удара. Длительность: 10 с. Перезарядка: 20 с.',
      'Щит начинает взаимодействовать с соседними сферами и выбранной веткой мутации.',
      'Защитная механика выбранной ветки усиливается после полученных ударов.',
      'Щит и связанная сеть получают дополнительное финальное усиление перед второй мутацией.'
    ),
    evolution4:[
      ae('shield_echo_guard','Эхо-барьер','Часть защитного эффекта щита передаётся ближайшим сферам.'),
      ae('shield_reflector','Отражающий контур','Поглощённый удар частично возвращается ближайшему врагу.'),
      ae('shield_bastion','Бастион','Щит создаёт защитный контур вокруг ближайших сфер.')
    ],
    evolution7:[
      ae('shield_network_guard','Network Bastion','Щит соединяет сферы в единую защитную сеть.'),
      ae('shield_iron_dome','Iron Dome','Сферы внутри сети получают защитный заряд при полном цикле.'),
      ae('shield_resonant_guard','Resonant Guard','Каждая защищённая сфера усиливает следующий заряд щита.')
    ]
  },
  teleport:{
    ability:'teleport',
    levels:abilityLevels(
      'Телепорт перемещает игрока в случайную точку в пределах ±300 px. Перезарядка: 15 с.',
      'Перезарядка: 13 с. Радиус случайного прыжка: до 300 px.',
      'Перезарядка: 11 с. Радиус случайного прыжка: до 300 px.',
      'Прыжок начинает взаимодействовать со сферами и выбранной веткой мутации.',
      'След телепорта и его взаимодействие с сетью сфер усиливаются.',
      'Выбранная ветка получает финальное усиление перед второй мутацией.'
    ),
    evolution4:[
      ae('teleport_echo_jump','Эхо-прыжок','Прыжок может использовать ближайшую сферу как резонансную точку.'),
      ae('teleport_beacon','Скачок-маяк','Точка прыжка помечается и усиливает следующую атаку.'),
      ae('teleport_phase','Фазовый прыжок','После телепорта игрок получает краткое окно неуязвимости.')
    ],
    evolution7:[
      ae('teleport_spatial_network','Spatial Network','Телепорт соединяет исходную и целевую сферу как новый узел сети.'),
      ae('teleport_hunter_beacon','Hunter Beacon','Прыжок к Sniper-сфере усиливает следующую атаку по выбранной цели.'),
      ae('teleport_phase_break','Phase Break','Каждая сфера на траектории создаёт дополнительный импульс.')
    ]
  },
  firetrail:{
    ability:'firetrail',
    levels:abilityLevels(
      'Оставляет огненный след на 5 с. Перезарядка: 25 с. След наносит 6 DPS.',
      'След длится 6 с и наносит 8 DPS. Перезарядка: 25 с.',
      'След длится 7 с и наносит 10 DPS. Перезарядка: 25 с.',
      'Огонь начинает взаимодействовать со статусами и выбранной веткой мутации сфер.',
      'Статусные реакции и заражение соседних целей усиливаются.',
      'Выбранная огненная ветка получает финальное усиление перед второй мутацией.'
    ),
    evolution4:[
      ae('firetrail_overdrive','Перегрев','След усиливает огненные эффекты рядом с ним.'),
      ae('firetrail_ignition','Воспламенитель','Огонь поджигает цели, уже находящиеся под статусным эффектом.'),
      ae('firetrail_sanctum','Пылающее святилище','След создаёт зону усиления вокруг Ауры.')
    ],
    evolution7:[
      ae('firetrail_network','Thermal Network','Огненный след соединяет сферы и передаёт перегрев между узлами.'),
      ae('firetrail_catalyst','Catalyst Flame','Статусная реакция усиливает следующий огненный импульс.'),
      ae('firetrail_inferno','Network Inferno','Каждый новый огненный узел усиливает предыдущие.')
    ]
  },
  minion:{
    ability:'minion',
    levels:abilityLevels(
      'Призывает 1 миньона на 10 с. Урон: 10. Перезарядка: 30 с.',
      'Призывает 1 миньона на 10 с. Урон: 12. Перезарядка: 30 с.',
      'Призывает 2 миньонов на 10 с. Урон каждого: 14. Перезарядка: 30 с.',
      'Миньоны начинают взаимодействовать с узлами сфер и выбранной веткой.',
      'Миньон начинает передавать импульсы между ближайшими сферами.',
      'Количество и взаимодействие миньонов с сетью получают финальное усиление.'
    ),
    evolution4:[
      ae('minion_echo_drone','Эхо-дрон','Миньон становится частью сети и может занимать её узел.'),
      ae('minion_relay_drone','Релейный дрон','Миньон передаёт срабатывания между соседними сферами.'),
      ae('minion_guardian','Дрон-страж','Миньон защищает ближайшую сферу.')
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
      'Молния поражает 1 цель на 55 урона. Перезарядка: 20 с.',
      'Молния поражает 1 цель на 70 урона. Перезарядка: 20 с.',
      'Молния поражает 2 цели на 85 урона каждая. Перезарядка: 20 с.',
      'Разряд начинает взаимодействовать с Chain-сферами и выбранной веткой.',
      'Переходы между узлами усиливают следующий разряд.',
      'Связанная сеть дольше сохраняет заряд перед второй мутацией.'
    ),
    evolution4:[
      ae('lightning_echo_storm','Эхо-шторм','Молния проводится через Chain-сферы, превращая их в проводники.'),
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
      'Останавливает всех врагов на 3 с. Перезарядка: 40 с.',
      'Останавливает всех врагов на 4 с. Перезарядка: 40 с.',
      'Останавливает всех врагов на 5 с. Перезарядка: 40 с.',
      'Во время остановки времени сферы начинают взаимодействовать с выбранной веткой.',
      'Урон по замороженным врагам и время удержания сети усиливаются.',
      'Финальная форма превращает остановку времени в часть сети сфер.'
    ),
    evolution4:[
      ae('timestop_echo_phase','Фазовый разрыв','Сферы продолжают атаковать во время остановки.'),
      ae('timestop_closed_time','Замкнутое время','Во время остановки зона эффекта расширяется через сеть.'),
      ae('timestop_time_anchor','Якорь времени','Остановка фиксирует врагов вокруг ближайшей сферы.')
    ],
    evolution7:[
      ae('timestop_outside_time','Вне времени','Сферы продлевают остановку при каждом взаимодействии.'),
      ae('timestop_closed_network','Closed Time Network','Сеть сфер сохраняет эффект остановки между узлами.'),
      ae('timestop_temporal_core','Temporal Core','Последняя секунда остановки удваивает силу активной сети.')
    ]
  },
  darkritual:{
    ability:'darkritual',
    levels:abilityLevels(
      'Жертвуете 20% максимального HP и наносите 75 урона врагам в радиусе 500 px. Перезарядка: 30 с.',
      'Жертвуете 20% максимального HP и наносите 100 урона врагам в радиусе 500 px. Перезарядка: 30 с.',
      'Жертвуете 20% максимального HP и наносите 125 урона врагам в радиусе 500 px. Перезарядка: 30 с.',
      'Жертва HP начинает взаимодействовать с выбранной веткой и ближайшими сферами.',
      'Риск и награда ветки становятся сильнее: ритуал глубже вмешивается в сеть.',
      'Финальное усиление закрепляет выбранную ветку перед второй мутацией.'
    ),
    evolution4:[
      ae('darkritual_blood_link','Кровавая связь','Ритуал передаёт часть своей силы ближайшей Standard-сфере.'),
      ae('darkritual_sacrifice','Жертвенный круг','Часть HP превращается в дополнительный импульс вокруг игрока.'),
      ae('darkritual_void_pact','Договор пустоты','Чем меньше HP, тем сильнее следующий ритуал.')
    ],
    evolution7:[
      ae('darkritual_blood_network','Blood Network','Ритуал передаёт заряд между связанными Standard-сферами.'),
      ae('darkritual_sacrifice_core','Sacrifice Core','Каждая потеря HP усиливает следующий импульс сети.'),
      ae('darkritual_void_engine','Void Engine','Критически низкое HP превращает ритуал в усиленный сетевой взрыв.')
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

export const CHARACTER_SPHERE_PRIORITY:Record<CharacterId,SphereType[]>={spherist:['standard','chain'],hunter:['sniper','chain'],engineer:['aura','standard','chain'],berserker:['shotgun','standard'],alchemist:['aura','chain'],architect:['sniper','aura','standard']};

export function spherePriority(character:CharacterId,type:SphereType){const list=CHARACTER_SPHERE_PRIORITY[character]||[];const i=list.indexOf(type);return i<0?.25:1-i*.18;}
export function sphereLevel(s:any,type:SphereType){return s.player.sphereProgression?.[type]||0;}
function sphereFinalIndex(s:any,type:SphereType):number|null{
  const id=(s.player.evolutions||[]).find((x:string)=>x.startsWith('sphere:'+type+':7:'));
  if(!id)return null;
  const n=Number(id.split(':').pop());
  return Number.isFinite(n)?n:null;
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
  let damage=1+l*.08+(l>=4?.12:0)+(l>=7?.18:0), radius=1+(l>=2?.05:0)+(l>=5?.06:0)+(l>=7?.08:0), delay=Math.max(.48,1-l*.045);
  let pierce=l>=2?1:0, multishot=type==='shotgun'&&l>=1?1:0, chainTargets=type==='chain'?Math.max(1,l+1):0, auraRadius=l>=2?1.08:1, auraPulse=l>=4?.75:.5;
  if(type==='standard'&&branch==='standard_resonator'){damage*=1.08;if(final===0)radius*=1.12;}
  if(type==='standard'&&branch==='standard_singularity'){damage*=1.06;if(final===0)auraRadius=1.15;}
  if(type==='standard'&&branch==='standard_swarm'){multishot+=1;if(final===0)multishot+=1;}
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

  // A sphere + active ability synergy unlocks only when both reach VII.
  // This is a build-defining rule change, not another generic stat bonus.
  for (const link of getActiveSphereAbilitySynergies(s)) {
    if (link.sphere !== type) continue;
    if (link.effect === 'damage') damage *= 1.12;
    if (link.effect === 'attackSpeed') delay *= 0.88;
    if (link.effect === 'radius') radius *= 1.12;
    if (link.effect === 'chain') chainTargets += 1;
  }

  return {damage:damage*artifact.damage,radius:radius*artifact.radius,delay:delay*artifact.delay,pierce,multishot,chainTargets,auraRadius,auraPulse};
}