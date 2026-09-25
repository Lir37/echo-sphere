import type { CharacterId } from './characters';
import { getSphereArtifactModifiers } from './artifactSystem';
import { ABILITIES, type SphereType, type AbilityType } from './gameData';

export type SphereEvolutionId = 'standard_resonator' | 'standard_singularity' | 'standard_swarm' | 'sniper_oracle' | 'sniper_assassin' | 'sniper_beacon' | 'shotgun_burst' | 'shotgun_cataclysm' | 'shotgun_hail' | 'chain_web' | 'chain_storm' | 'chain_leech' | 'aura_sanctum' | 'aura_gravity' | 'aura_overgrowth' | 'orbital_dance' | 'orbital_halo' | 'orbital_blade' | 'prism_split' | 'prism_spectrum' | 'prism_mirror' | 'gravity_well' | 'gravity_tide' | 'gravity_collapse' | 'pulse_wave' | 'pulse_resonator' | 'pulse_burst' | 'void_hunger' | 'void_reaper' | 'void_execution';
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
const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id] ?? { level5: 'Усиление выбранной ветки.', level6: 'Дополнительное усиление уникальной механики.' };
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
const genericSphereBranches = (type: SphereType, names: [string,string,string], ids: [SphereEvolutionId,SphereEvolutionId,SphereEvolutionId]): [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch] =>
  ids.map((id, i) => br(id, names[i], 'Развивает уникальную механику сферы '+type+'.', finals(names[i], names[i], ids))) as [SphereEvolutionBranch,SphereEvolutionBranch,SphereEvolutionBranch];

export const SPHERE_PROGRESSION:Record<SphereType,SphereDef>={
 standard:sphere('standard','Стандартная',{spherist:1,engineer:.9,berserker:.8,architect:.7,hunter:.4,alchemist:.4},['+15% урона','+1 пробитие','-10% задержки'],[br('standard_resonator','Резонатор','Каждое третье попадание выпускает импульс',finals('Резонатор','Гиперрезонатор',['standard_resonator','standard_singularity','standard_swarm'])),br('standard_singularity','Сингулярность','Попадания притягивают врагов',finals('Сингулярность','Коллапс',['standard_singularity','standard_resonator','standard_swarm'])),br('standard_swarm','Рой','Попадания выпускают осколки',finals('Рой','Каскад',['standard_swarm','standard_singularity','standard_resonator']))]),
 sniper:sphere('sniper','Снайперская',{hunter:1,architect:.9,spherist:.4,engineer:.4,berserker:.3,alchemist:.3},['+25% урона','+15% дальности','+15% крита'],[br('sniper_oracle','Оракул','Усиливает критический урон по отмеченным целям',finals('Оракул','Провидец',['sniper_oracle','sniper_assassin','sniper_beacon'])),br('sniper_assassin','Убийца','Усиливает урон по слабым целям',finals('Убийца','Казнь',['sniper_assassin','sniper_oracle','sniper_beacon'])),br('sniper_beacon','Маяк','Помечает цель для всей сети',finals('Маяк','Всевидящее око',['sniper_beacon','sniper_assassin','sniper_oracle']))]),
 shotgun:sphere('shotgun','Дробовик',{berserker:1,alchemist:.8,spherist:.6,engineer:.4,hunter:.3,architect:.3},['+1 дробь','+20% урона вблизи','-12% разброса'],[br('shotgun_burst','Разрыв','Ближние попадания наносят повышенный урон',finals('Разрыв','Катаклизм',['shotgun_burst','shotgun_cataclysm','shotgun_hail'])),br('shotgun_cataclysm','Осада','Тяжёлые пробивные снаряды',finals('Осада','Удар',['shotgun_cataclysm','shotgun_burst','shotgun_hail'])),br('shotgun_hail','Град','Много дополнительных снарядов',finals('Град','Ливень',['shotgun_hail','shotgun_burst','shotgun_cataclysm']))]),
 chain:sphere('chain','Цепная',{spherist:1,hunter:.95,engineer:.9,alchemist:.8,architect:.5,berserker:.4},['+1 цель цепи','+10% урона цепи','+15% скорости перехода'],[br('chain_web','Паутина','Поражённые цели получают усиленное замедление',finals('Паутина','Сеть Эха',['chain_web','chain_storm','chain_leech'])),br('chain_storm','Шторм','Каждый переход усиливает следующий',finals('Шторм','Разряд',['chain_storm','chain_web','chain_leech'])),br('chain_leech','Паразит','Цепь возвращает часть урона',finals('Паразит','Пожиратель',['chain_leech','chain_storm','chain_web']))]),
 aura:sphere('aura','Аура',{engineer:1,alchemist:1,architect:.9,spherist:.7,hunter:.4,berserker:.4},['+20% радиуса ауры','-10% интервала импульса','+10% урона ауры'],[br('aura_sanctum','Святилище','Замедляет врагов и усиливает сферы',finals('Святилище','Эхо-святилище',['aura_sanctum','aura_gravity','aura_overgrowth'])),br('aura_gravity','Гравитация','Стягивает врагов к центру',finals('Гравитация','Сингулярность',['aura_gravity','aura_sanctum','aura_overgrowth'])),br('aura_overgrowth','Живая сеть','Усиливает сферы внутри ауры',finals('Живая сеть','Рост',['aura_overgrowth','aura_sanctum','aura_gravity']))]),
 orbital:sphere('orbital','Орбитальная',{spherist:1,engineer:.8,architect:.8},['+15% орбитального урона','+15% радиуса орбиты','-12% интервала'],genericSphereBranches('orbital',['Танец','Ореол','Клинок'],['orbital_dance','orbital_halo','orbital_blade'])),
 prism:sphere('prism','Призма',{hunter:1,architect:.9,spherist:.7},['+20% урона луча','+15% дальности','+1 направление'],genericSphereBranches('prism',['Расщепление','Спектр','Зеркало'],['prism_split','prism_spectrum','prism_mirror'])),
 gravity:sphere('gravity','Гравитационная',{alchemist:1,architect:1,engineer:.8},['+20% силы притяжения','+15% радиуса','-15% интервала импульса'],genericSphereBranches('gravity',['Колодец','Прилив','Коллапс'],['gravity_well','gravity_tide','gravity_collapse'])),
 pulse:sphere('pulse','Импульсная',{engineer:1,spherist:.9,architect:.8},['+20% импульсного урона','+15% радиуса','-12% интервала'],genericSphereBranches('pulse',['Волна','Резонатор','Вспышка'],['pulse_wave','pulse_resonator','pulse_burst'])),
 void:sphere('void','Пустотная',{hunter:1,alchemist:.8,architect:.7},['+20% урона по ослабленным','+10% шанс критического добивания','+15% дальности'],genericSphereBranches('void',['Голод','Жнец','Экзекуция'],['void_hunger','void_reaper','void_execution']))
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
  let damage=1, radius=1, delay=1, pierce=0, multishot=0, chainTargets=1, auraRadius=1, auraPulse=.5;
  let spreadMult=1;

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

  for (const link of getActiveSphereAbilitySynergies(s)) {
    if (link.sphere !== type) continue;
    if (link.effect === 'damage') damage *= 1.12;
    if (link.effect === 'attackSpeed') delay *= 0.88;
    if (link.effect === 'radius') radius *= 1.12;
    if (link.effect === 'chain') chainTargets += 1;
  }

  return {damage:damage*artifact.damage,radius:radius*artifact.radius,delay:delay*artifact.delay,pierce,multishot,chainTargets,auraRadius,auraPulse,spreadMult};
}