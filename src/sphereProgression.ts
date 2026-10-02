import type { CharacterId } from './characters';
import { getSphereArtifactModifiers } from './artifactSystem';
import { ABILITIES, SPHERE_TYPES, type SphereType, type AbilityType, sphereUsesProjectileModifiers } from './gameData';
import type { SphereMods } from './engineTypes';

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
  standard_resonator:['Resonator','Every third hit releases a pulse around the target.'],
  standard_singularity:['Singularity','Hits slow enemies and pull nearby targets toward the impact point.'],
  standard_swarm:['Swarm','Hits release side shards that widen battlefield coverage.'],
  sniper_oracle:['Oracle','Marked targets receive amplified sniper hits.'],
  sniper_assassin:['Assassin','The branch specializes in finishing weakened targets.'],
  sniper_beacon:['Beacon','Hits mark targets and extend the effect through the network.'],
  shotgun_burst:['Burst','Close-range hits gain stronger impact.'],
  shotgun_cataclysm:['Cataclysm','Heavy shots emphasize penetration and explosive impact.'],
  shotgun_hail:['Hail','Additional projectiles create persistent area pressure.'],
  chain_web:['Web','Chain hits build a control web around affected targets.'],
  chain_storm:['Storm','Chain jumps create electrical surges between targets.'],
  chain_leech:['Leech','Chain hits return part of their damage to the player.'],
  aura_sanctum:['Sanctum','The Aura focuses on slowing and controlling enemies.'],
  aura_gravity:['Gravity','The Aura pulls enemies toward its center.'],
  aura_overgrowth:['Overgrowth','The Aura empowers nearby Spheres inside its field.'],
  orbital_dance:['Dance','Orbiting satellites accelerate and pressure enemies on each pass.'],
  orbital_halo:['Halo','The orbit creates a protective resonance for nearby Spheres.'],
  orbital_blade:['Blade','Orbiting satellites become dedicated cutting weapons.'],
  prism_split:['Split','The beam divides across additional targets.'],
  prism_spectrum:['Spectrum','The beam interacts with active status effects.'],
  prism_mirror:['Mirror','Linked Spheres reflect additional beam damage.'],
  gravity_well:['Well','The field creates sustained pull toward its center.'],
  gravity_tide:['Tide','The field alternates between pull and release waves.'],
  gravity_collapse:['Collapse','Dense enemy groups take amplified compression damage.'],
  pulse_wave:['Wave','The pulse grows wider and pushes enemies away.'],
  pulse_resonator:['Resonator','Pulse attacks feed Resonance and network pressure.'],
  pulse_burst:['Burst','A secondary discharge follows the main pulse.'],
  void_hunger:['Hunger','Damage rises as the target loses health.'],
  void_reaper:['Reaper','Void kills restore health and create follow-up shards.'],
  void_execution:['Execution','Weakened targets become vulnerable to lethal finishing hits.'],
};

const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>{
  const details=BRANCH_LEVEL_DETAILS[id] ?? NEW_BRANCH_LEVEL_DETAILS[id] ?? { level5: 'Усиление выбранной ветки.', level6: 'Дополнительное усиление уникальной механики.' };
  const [nameEn,descEn]=BRANCH_EN[id] ?? [ru,desc];
  return { ...e(id,ru,nameEn,desc,descEn), final:fin, level5:{ru:details.level5,en:details.level5}, level6:{ru:details.level6,en:details.level6} };
};
const f=(id:string,nameRu:string,nameEn:string,descRu:string,descEn:string):SphereEvolutionDef=>e(id,nameRu,nameEn,descRu,descEn);

const SPHERE_FINAL_VARIANTS:Record<SphereEvolutionId,[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]>={
  standard_resonator:[
    f('standard_resonator_final_1','Ядро Resonator','Core Resonator','Every third hit releases a pulse around the target. Финальная форма закрепляет главный акцент ветки.','Every third hit releases a pulse around the target. The final form reinforces the branch's primary combat identity.'),
    f('standard_resonator_final_2','Каскад Resonator','Cascade Resonator','Every third hit releases a pulse around the target. Финальная форма расширяет основной боевой паттерн ветки.','Every third hit releases a pulse around the target. The final form extends the branch's core combat pattern.'),
    f('standard_resonator_final_3','Апекс Resonator','Apex Resonator','Every third hit releases a pulse around the target. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Every third hit releases a pulse around the target. The final form strengthens the branch's signature final pattern.'),
  ],
  standard_singularity:[
    f('standard_singularity_final_1','Ядро Singularity','Core Singularity','Hits slow enemies and pull nearby targets toward the impact point. Финальная форма закрепляет главный акцент ветки.','Hits slow enemies and pull nearby targets toward the impact point. The final form reinforces the branch's primary combat identity.'),
    f('standard_singularity_final_2','Каскад Singularity','Cascade Singularity','Hits slow enemies and pull nearby targets toward the impact point. Финальная форма расширяет основной боевой паттерн ветки.','Hits slow enemies and pull nearby targets toward the impact point. The final form extends the branch's core combat pattern.'),
    f('standard_singularity_final_3','Апекс Singularity','Apex Singularity','Hits slow enemies and pull nearby targets toward the impact point. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Hits slow enemies and pull nearby targets toward the impact point. The final form strengthens the branch's signature final pattern.'),
  ],
  standard_swarm:[
    f('standard_swarm_final_1','Ядро Swarm','Core Swarm','Hits release side shards that widen battlefield coverage. Финальная форма закрепляет главный акцент ветки.','Hits release side shards that widen battlefield coverage. The final form reinforces the branch's primary combat identity.'),
    f('standard_swarm_final_2','Каскад Swarm','Cascade Swarm','Hits release side shards that widen battlefield coverage. Финальная форма расширяет основной боевой паттерн ветки.','Hits release side shards that widen battlefield coverage. The final form extends the branch's core combat pattern.'),
    f('standard_swarm_final_3','Апекс Swarm','Apex Swarm','Hits release side shards that widen battlefield coverage. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Hits release side shards that widen battlefield coverage. The final form strengthens the branch's signature final pattern.'),
  ],
  sniper_oracle:[
    f('sniper_oracle_final_1','Ядро Oracle','Core Oracle','Marked targets receive amplified sniper hits. Финальная форма закрепляет главный акцент ветки.','Marked targets receive amplified sniper hits. The final form reinforces the branch's primary combat identity.'),
    f('sniper_oracle_final_2','Каскад Oracle','Cascade Oracle','Marked targets receive amplified sniper hits. Финальная форма расширяет основной боевой паттерн ветки.','Marked targets receive amplified sniper hits. The final form extends the branch's core combat pattern.'),
    f('sniper_oracle_final_3','Апекс Oracle','Apex Oracle','Marked targets receive amplified sniper hits. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Marked targets receive amplified sniper hits. The final form strengthens the branch's signature final pattern.'),
  ],
  sniper_assassin:[
    f('sniper_assassin_final_1','Ядро Assassin','Core Assassin','The branch specializes in finishing weakened targets. Финальная форма закрепляет главный акцент ветки.','The branch specializes in finishing weakened targets. The final form reinforces the branch's primary combat identity.'),
    f('sniper_assassin_final_2','Каскад Assassin','Cascade Assassin','The branch specializes in finishing weakened targets. Финальная форма расширяет основной боевой паттерн ветки.','The branch specializes in finishing weakened targets. The final form extends the branch's core combat pattern.'),
    f('sniper_assassin_final_3','Апекс Assassin','Apex Assassin','The branch specializes in finishing weakened targets. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The branch specializes in finishing weakened targets. The final form strengthens the branch's signature final pattern.'),
  ],
  sniper_beacon:[
    f('sniper_beacon_final_1','Ядро Beacon','Core Beacon','Hits mark targets and extend the effect through the network. Финальная форма закрепляет главный акцент ветки.','Hits mark targets and extend the effect through the network. The final form reinforces the branch's primary combat identity.'),
    f('sniper_beacon_final_2','Каскад Beacon','Cascade Beacon','Hits mark targets and extend the effect through the network. Финальная форма расширяет основной боевой паттерн ветки.','Hits mark targets and extend the effect through the network. The final form extends the branch's core combat pattern.'),
    f('sniper_beacon_final_3','Апекс Beacon','Apex Beacon','Hits mark targets and extend the effect through the network. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Hits mark targets and extend the effect through the network. The final form strengthens the branch's signature final pattern.'),
  ],
  shotgun_burst:[
    f('shotgun_burst_final_1','Ядро Burst','Core Burst','Close-range hits gain stronger impact. Финальная форма закрепляет главный акцент ветки.','Close-range hits gain stronger impact. The final form reinforces the branch's primary combat identity.'),
    f('shotgun_burst_final_2','Каскад Burst','Cascade Burst','Close-range hits gain stronger impact. Финальная форма расширяет основной боевой паттерн ветки.','Close-range hits gain stronger impact. The final form extends the branch's core combat pattern.'),
    f('shotgun_burst_final_3','Апекс Burst','Apex Burst','Close-range hits gain stronger impact. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Close-range hits gain stronger impact. The final form strengthens the branch's signature final pattern.'),
  ],
  shotgun_cataclysm:[
    f('shotgun_cataclysm_final_1','Ядро Cataclysm','Core Cataclysm','Heavy shots emphasize penetration and explosive impact. Финальная форма закрепляет главный акцент ветки.','Heavy shots emphasize penetration and explosive impact. The final form reinforces the branch's primary combat identity.'),
    f('shotgun_cataclysm_final_2','Каскад Cataclysm','Cascade Cataclysm','Heavy shots emphasize penetration and explosive impact. Финальная форма расширяет основной боевой паттерн ветки.','Heavy shots emphasize penetration and explosive impact. The final form extends the branch's core combat pattern.'),
    f('shotgun_cataclysm_final_3','Апекс Cataclysm','Apex Cataclysm','Heavy shots emphasize penetration and explosive impact. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Heavy shots emphasize penetration and explosive impact. The final form strengthens the branch's signature final pattern.'),
  ],
  shotgun_hail:[
    f('shotgun_hail_final_1','Ядро Hail','Core Hail','Additional projectiles create persistent area pressure. Финальная форма закрепляет главный акцент ветки.','Additional projectiles create persistent area pressure. The final form reinforces the branch's primary combat identity.'),
    f('shotgun_hail_final_2','Каскад Hail','Cascade Hail','Additional projectiles create persistent area pressure. Финальная форма расширяет основной боевой паттерн ветки.','Additional projectiles create persistent area pressure. The final form extends the branch's core combat pattern.'),
    f('shotgun_hail_final_3','Апекс Hail','Apex Hail','Additional projectiles create persistent area pressure. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Additional projectiles create persistent area pressure. The final form strengthens the branch's signature final pattern.'),
  ],
  chain_web:[
    f('chain_web_final_1','Ядро Web','Core Web','Chain hits build a control web around affected targets. Финальная форма закрепляет главный акцент ветки.','Chain hits build a control web around affected targets. The final form reinforces the branch's primary combat identity.'),
    f('chain_web_final_2','Каскад Web','Cascade Web','Chain hits build a control web around affected targets. Финальная форма расширяет основной боевой паттерн ветки.','Chain hits build a control web around affected targets. The final form extends the branch's core combat pattern.'),
    f('chain_web_final_3','Апекс Web','Apex Web','Chain hits build a control web around affected targets. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Chain hits build a control web around affected targets. The final form strengthens the branch's signature final pattern.'),
  ],
  chain_storm:[
    f('chain_storm_final_1','Ядро Storm','Core Storm','Chain jumps create electrical surges between targets. Финальная форма закрепляет главный акцент ветки.','Chain jumps create electrical surges between targets. The final form reinforces the branch's primary combat identity.'),
    f('chain_storm_final_2','Каскад Storm','Cascade Storm','Chain jumps create electrical surges between targets. Финальная форма расширяет основной боевой паттерн ветки.','Chain jumps create electrical surges between targets. The final form extends the branch's core combat pattern.'),
    f('chain_storm_final_3','Апекс Storm','Apex Storm','Chain jumps create electrical surges between targets. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Chain jumps create electrical surges between targets. The final form strengthens the branch's signature final pattern.'),
  ],
  chain_leech:[
    f('chain_leech_final_1','Ядро Leech','Core Leech','Chain hits return part of their damage to the player. Финальная форма закрепляет главный акцент ветки.','Chain hits return part of their damage to the player. The final form reinforces the branch's primary combat identity.'),
    f('chain_leech_final_2','Каскад Leech','Cascade Leech','Chain hits return part of their damage to the player. Финальная форма расширяет основной боевой паттерн ветки.','Chain hits return part of their damage to the player. The final form extends the branch's core combat pattern.'),
    f('chain_leech_final_3','Апекс Leech','Apex Leech','Chain hits return part of their damage to the player. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Chain hits return part of their damage to the player. The final form strengthens the branch's signature final pattern.'),
  ],
  aura_sanctum:[
    f('aura_sanctum_final_1','Ядро Sanctum','Core Sanctum','The Aura focuses on slowing and controlling enemies. Финальная форма закрепляет главный акцент ветки.','The Aura focuses on slowing and controlling enemies. The final form reinforces the branch's primary combat identity.'),
    f('aura_sanctum_final_2','Каскад Sanctum','Cascade Sanctum','The Aura focuses on slowing and controlling enemies. Финальная форма расширяет основной боевой паттерн ветки.','The Aura focuses on slowing and controlling enemies. The final form extends the branch's core combat pattern.'),
    f('aura_sanctum_final_3','Апекс Sanctum','Apex Sanctum','The Aura focuses on slowing and controlling enemies. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The Aura focuses on slowing and controlling enemies. The final form strengthens the branch's signature final pattern.'),
  ],
  aura_gravity:[
    f('aura_gravity_final_1','Ядро Gravity','Core Gravity','The Aura pulls enemies toward its center. Финальная форма закрепляет главный акцент ветки.','The Aura pulls enemies toward its center. The final form reinforces the branch's primary combat identity.'),
    f('aura_gravity_final_2','Каскад Gravity','Cascade Gravity','The Aura pulls enemies toward its center. Финальная форма расширяет основной боевой паттерн ветки.','The Aura pulls enemies toward its center. The final form extends the branch's core combat pattern.'),
    f('aura_gravity_final_3','Апекс Gravity','Apex Gravity','The Aura pulls enemies toward its center. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The Aura pulls enemies toward its center. The final form strengthens the branch's signature final pattern.'),
  ],
  aura_overgrowth:[
    f('aura_overgrowth_final_1','Ядро Overgrowth','Core Overgrowth','The Aura empowers nearby Spheres inside its field. Финальная форма закрепляет главный акцент ветки.','The Aura empowers nearby Spheres inside its field. The final form reinforces the branch's primary combat identity.'),
    f('aura_overgrowth_final_2','Каскад Overgrowth','Cascade Overgrowth','The Aura empowers nearby Spheres inside its field. Финальная форма расширяет основной боевой паттерн ветки.','The Aura empowers nearby Spheres inside its field. The final form extends the branch's core combat pattern.'),
    f('aura_overgrowth_final_3','Апекс Overgrowth','Apex Overgrowth','The Aura empowers nearby Spheres inside its field. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The Aura empowers nearby Spheres inside its field. The final form strengthens the branch's signature final pattern.'),
  ],
  orbital_dance:[
    f('orbital_dance_final_1','Ядро Dance','Core Dance','Orbiting satellites accelerate and pressure enemies on each pass. Финальная форма закрепляет главный акцент ветки.','Orbiting satellites accelerate and pressure enemies on each pass. The final form reinforces the branch's primary combat identity.'),
    f('orbital_dance_final_2','Каскад Dance','Cascade Dance','Orbiting satellites accelerate and pressure enemies on each pass. Финальная форма расширяет основной боевой паттерн ветки.','Orbiting satellites accelerate and pressure enemies on each pass. The final form extends the branch's core combat pattern.'),
    f('orbital_dance_final_3','Апекс Dance','Apex Dance','Orbiting satellites accelerate and pressure enemies on each pass. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Orbiting satellites accelerate and pressure enemies on each pass. The final form strengthens the branch's signature final pattern.'),
  ],
  orbital_halo:[
    f('orbital_halo_final_1','Ядро Halo','Core Halo','The orbit creates a protective resonance for nearby Spheres. Финальная форма закрепляет главный акцент ветки.','The orbit creates a protective resonance for nearby Spheres. The final form reinforces the branch's primary combat identity.'),
    f('orbital_halo_final_2','Каскад Halo','Cascade Halo','The orbit creates a protective resonance for nearby Spheres. Финальная форма расширяет основной боевой паттерн ветки.','The orbit creates a protective resonance for nearby Spheres. The final form extends the branch's core combat pattern.'),
    f('orbital_halo_final_3','Апекс Halo','Apex Halo','The orbit creates a protective resonance for nearby Spheres. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The orbit creates a protective resonance for nearby Spheres. The final form strengthens the branch's signature final pattern.'),
  ],
  orbital_blade:[
    f('orbital_blade_final_1','Ядро Blade','Core Blade','Orbiting satellites become dedicated cutting weapons. Финальная форма закрепляет главный акцент ветки.','Orbiting satellites become dedicated cutting weapons. The final form reinforces the branch's primary combat identity.'),
    f('orbital_blade_final_2','Каскад Blade','Cascade Blade','Orbiting satellites become dedicated cutting weapons. Финальная форма расширяет основной боевой паттерн ветки.','Orbiting satellites become dedicated cutting weapons. The final form extends the branch's core combat pattern.'),
    f('orbital_blade_final_3','Апекс Blade','Apex Blade','Orbiting satellites become dedicated cutting weapons. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Orbiting satellites become dedicated cutting weapons. The final form strengthens the branch's signature final pattern.'),
  ],
  prism_split:[
    f('prism_split_final_1','Ядро Split','Core Split','The beam divides across additional targets. Финальная форма закрепляет главный акцент ветки.','The beam divides across additional targets. The final form reinforces the branch's primary combat identity.'),
    f('prism_split_final_2','Каскад Split','Cascade Split','The beam divides across additional targets. Финальная форма расширяет основной боевой паттерн ветки.','The beam divides across additional targets. The final form extends the branch's core combat pattern.'),
    f('prism_split_final_3','Апекс Split','Apex Split','The beam divides across additional targets. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The beam divides across additional targets. The final form strengthens the branch's signature final pattern.'),
  ],
  prism_spectrum:[
    f('prism_spectrum_final_1','Ядро Spectrum','Core Spectrum','The beam interacts with active status effects. Финальная форма закрепляет главный акцент ветки.','The beam interacts with active status effects. The final form reinforces the branch's primary combat identity.'),
    f('prism_spectrum_final_2','Каскад Spectrum','Cascade Spectrum','The beam interacts with active status effects. Финальная форма расширяет основной боевой паттерн ветки.','The beam interacts with active status effects. The final form extends the branch's core combat pattern.'),
    f('prism_spectrum_final_3','Апекс Spectrum','Apex Spectrum','The beam interacts with active status effects. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The beam interacts with active status effects. The final form strengthens the branch's signature final pattern.'),
  ],
  prism_mirror:[
    f('prism_mirror_final_1','Ядро Mirror','Core Mirror','Linked Spheres reflect additional beam damage. Финальная форма закрепляет главный акцент ветки.','Linked Spheres reflect additional beam damage. The final form reinforces the branch's primary combat identity.'),
    f('prism_mirror_final_2','Каскад Mirror','Cascade Mirror','Linked Spheres reflect additional beam damage. Финальная форма расширяет основной боевой паттерн ветки.','Linked Spheres reflect additional beam damage. The final form extends the branch's core combat pattern.'),
    f('prism_mirror_final_3','Апекс Mirror','Apex Mirror','Linked Spheres reflect additional beam damage. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Linked Spheres reflect additional beam damage. The final form strengthens the branch's signature final pattern.'),
  ],
  gravity_well:[
    f('gravity_well_final_1','Ядро Well','Core Well','The field creates sustained pull toward its center. Финальная форма закрепляет главный акцент ветки.','The field creates sustained pull toward its center. The final form reinforces the branch's primary combat identity.'),
    f('gravity_well_final_2','Каскад Well','Cascade Well','The field creates sustained pull toward its center. Финальная форма расширяет основной боевой паттерн ветки.','The field creates sustained pull toward its center. The final form extends the branch's core combat pattern.'),
    f('gravity_well_final_3','Апекс Well','Apex Well','The field creates sustained pull toward its center. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The field creates sustained pull toward its center. The final form strengthens the branch's signature final pattern.'),
  ],
  gravity_tide:[
    f('gravity_tide_final_1','Ядро Tide','Core Tide','The field alternates between pull and release waves. Финальная форма закрепляет главный акцент ветки.','The field alternates between pull and release waves. The final form reinforces the branch's primary combat identity.'),
    f('gravity_tide_final_2','Каскад Tide','Cascade Tide','The field alternates between pull and release waves. Финальная форма расширяет основной боевой паттерн ветки.','The field alternates between pull and release waves. The final form extends the branch's core combat pattern.'),
    f('gravity_tide_final_3','Апекс Tide','Apex Tide','The field alternates between pull and release waves. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The field alternates between pull and release waves. The final form strengthens the branch's signature final pattern.'),
  ],
  gravity_collapse:[
    f('gravity_collapse_final_1','Ядро Collapse','Core Collapse','Dense enemy groups take amplified compression damage. Финальная форма закрепляет главный акцент ветки.','Dense enemy groups take amplified compression damage. The final form reinforces the branch's primary combat identity.'),
    f('gravity_collapse_final_2','Каскад Collapse','Cascade Collapse','Dense enemy groups take amplified compression damage. Финальная форма расширяет основной боевой паттерн ветки.','Dense enemy groups take amplified compression damage. The final form extends the branch's core combat pattern.'),
    f('gravity_collapse_final_3','Апекс Collapse','Apex Collapse','Dense enemy groups take amplified compression damage. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Dense enemy groups take amplified compression damage. The final form strengthens the branch's signature final pattern.'),
  ],
  pulse_wave:[
    f('pulse_wave_final_1','Ядро Wave','Core Wave','The pulse grows wider and pushes enemies away. Финальная форма закрепляет главный акцент ветки.','The pulse grows wider and pushes enemies away. The final form reinforces the branch's primary combat identity.'),
    f('pulse_wave_final_2','Каскад Wave','Cascade Wave','The pulse grows wider and pushes enemies away. Финальная форма расширяет основной боевой паттерн ветки.','The pulse grows wider and pushes enemies away. The final form extends the branch's core combat pattern.'),
    f('pulse_wave_final_3','Апекс Wave','Apex Wave','The pulse grows wider and pushes enemies away. Финальная форма усиливает сигнатурный финальный паттерн ветки.','The pulse grows wider and pushes enemies away. The final form strengthens the branch's signature final pattern.'),
  ],
  pulse_resonator:[
    f('pulse_resonator_final_1','Ядро Resonator','Core Resonator','Pulse attacks feed Resonance and network pressure. Финальная форма закрепляет главный акцент ветки.','Pulse attacks feed Resonance and network pressure. The final form reinforces the branch's primary combat identity.'),
    f('pulse_resonator_final_2','Каскад Resonator','Cascade Resonator','Pulse attacks feed Resonance and network pressure. Финальная форма расширяет основной боевой паттерн ветки.','Pulse attacks feed Resonance and network pressure. The final form extends the branch's core combat pattern.'),
    f('pulse_resonator_final_3','Апекс Resonator','Apex Resonator','Pulse attacks feed Resonance and network pressure. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Pulse attacks feed Resonance and network pressure. The final form strengthens the branch's signature final pattern.'),
  ],
  pulse_burst:[
    f('pulse_burst_final_1','Ядро Burst','Core Burst','A secondary discharge follows the main pulse. Финальная форма закрепляет главный акцент ветки.','A secondary discharge follows the main pulse. The final form reinforces the branch's primary combat identity.'),
    f('pulse_burst_final_2','Каскад Burst','Cascade Burst','A secondary discharge follows the main pulse. Финальная форма расширяет основной боевой паттерн ветки.','A secondary discharge follows the main pulse. The final form extends the branch's core combat pattern.'),
    f('pulse_burst_final_3','Апекс Burst','Apex Burst','A secondary discharge follows the main pulse. Финальная форма усиливает сигнатурный финальный паттерн ветки.','A secondary discharge follows the main pulse. The final form strengthens the branch's signature final pattern.'),
  ],
  void_hunger:[
    f('void_hunger_final_1','Ядро Hunger','Core Hunger','Damage rises as the target loses health. Финальная форма закрепляет главный акцент ветки.','Damage rises as the target loses health. The final form reinforces the branch's primary combat identity.'),
    f('void_hunger_final_2','Каскад Hunger','Cascade Hunger','Damage rises as the target loses health. Финальная форма расширяет основной боевой паттерн ветки.','Damage rises as the target loses health. The final form extends the branch's core combat pattern.'),
    f('void_hunger_final_3','Апекс Hunger','Apex Hunger','Damage rises as the target loses health. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Damage rises as the target loses health. The final form strengthens the branch's signature final pattern.'),
  ],
  void_reaper:[
    f('void_reaper_final_1','Ядро Reaper','Core Reaper','Void kills restore health and create follow-up shards. Финальная форма закрепляет главный акцент ветки.','Void kills restore health and create follow-up shards. The final form reinforces the branch's primary combat identity.'),
    f('void_reaper_final_2','Каскад Reaper','Cascade Reaper','Void kills restore health and create follow-up shards. Финальная форма расширяет основной боевой паттерн ветки.','Void kills restore health and create follow-up shards. The final form extends the branch's core combat pattern.'),
    f('void_reaper_final_3','Апекс Reaper','Apex Reaper','Void kills restore health and create follow-up shards. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Void kills restore health and create follow-up shards. The final form strengthens the branch's signature final pattern.'),
  ],
  void_execution:[
    f('void_execution_final_1','Ядро Execution','Core Execution','Weakened targets become vulnerable to lethal finishing hits. Финальная форма закрепляет главный акцент ветки.','Weakened targets become vulnerable to lethal finishing hits. The final form reinforces the branch's primary combat identity.'),
    f('void_execution_final_2','Каскад Execution','Cascade Execution','Weakened targets become vulnerable to lethal finishing hits. Финальная форма расширяет основной боевой паттерн ветки.','Weakened targets become vulnerable to lethal finishing hits. The final form extends the branch's core combat pattern.'),
    f('void_execution_final_3','Апекс Execution','Apex Execution','Weakened targets become vulnerable to lethal finishing hits. Финальная форма усиливает сигнатурный финальный паттерн ветки.','Weakened targets become vulnerable to lethal finishing hits. The final form strengthens the branch's signature final pattern.'),
  ],
};

const finalsFor=(id:SphereEvolutionId):[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]=>{
  const finals=SPHERE_FINAL_VARIANTS[id];
  if(!finals||finals.length!==3) throw new Error('Sphere branch must expose exactly three final forms: '+id);
  return finals;
};

const ABILITY_PROGRESSION:Partial<Record<AbilityType,AbilityProgressionDef>>={
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