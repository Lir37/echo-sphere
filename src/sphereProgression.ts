import type { CharacterId } from './characters';
import { getSphereArtifactModifiers } from './artifactSystem';
import type { SphereType, AbilityType } from './gameData';

export type SphereEvolutionId = 'standard_resonator' | 'standard_singularity' | 'standard_swarm' | 'sniper_oracle' | 'sniper_assassin' | 'sniper_beacon' | 'shotgun_burst' | 'shotgun_cataclysm' | 'shotgun_hail' | 'chain_web' | 'chain_storm' | 'chain_leech' | 'aura_sanctum' | 'aura_gravity' | 'aura_overgrowth';
export type AbilityEvolutionId = string;
export interface SphereUpgradeDef { level:number; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionDef { id:SphereEvolutionId; name:{ru:string;en:string}; desc:{ru:string;en:string}; }
export interface SphereEvolutionBranch extends SphereEvolutionDef { final:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]; }
export interface SphereDef { type:SphereType; name:{ru:string;en:string}; priority:Partial<Record<CharacterId,number>>; levels:SphereUpgradeDef[]; evolution4:SphereEvolutionDef; evolution7:SphereEvolutionDef; evolution4Choices:SphereEvolutionBranch[]; }
const lv=(a:string,b:string,c:string):SphereUpgradeDef[]=>[{level:1,name:{ru:'Ядро',en:'Core'},desc:{ru:a,en:a}},{level:2,name:{ru:'Механизм',en:'Mechanism'},desc:{ru:b,en:b}},{level:3,name:{ru:'Настройка',en:'Tuning'},desc:{ru:c,en:c}},{level:4,name:{ru:'Эволюция I',en:'Evolution I'},desc:{ru:'Выбор одной из трёх веток',en:'Choose one of three branches'}},{level:5,name:{ru:'Контур',en:'Circuit'},desc:{ru:'Усиление выбранной ветки',en:'Strengthens the selected branch'}},{level:6,name:{ru:'Стабилизатор',en:'Stabilizer'},desc:{ru:'Усиление специальной механики',en:'Strengthens the special mechanic'}},{level:7,name:{ru:'Эволюция II',en:'Evolution II'},desc:{ru:'Финальная специализация',en:'Final specialization'}}];
const e=(id:SphereEvolutionId,ru:string,desc:string):SphereEvolutionDef=>({id,name:{ru,en:ru},desc:{ru:desc,en:desc}});
const br=(id:SphereEvolutionId,ru:string,desc:string,fin:[SphereEvolutionDef,SphereEvolutionDef,SphereEvolutionDef]):SphereEvolutionBranch=>({...e(id,ru,desc),final:fin});
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
const abilityLevels=(a:string,b:string,c:string,d:string,e:string,f:string,g:string=''):SphereUpgradeDef[] => [
  {level:1,name:{ru:'Пробуждение',en:'Awakening'},desc:{ru:a,en:a}},
  {level:2,name:{ru:'Настройка',en:'Tuning'},desc:{ru:b,en:b}},
  {level:3,name:{ru:'Раскрытие',en:'Expansion'},desc:{ru:c,en:c}},
  {level:4,name:{ru:'Эволюция I',en:'Evolution I'},desc:{ru:'Выбор специализации',en:'Choose a specialization'}},
  {level:5,name:{ru:'Углубление',en:'Deepening'},desc:{ru:d,en:d}},
  {level:6,name:{ru:'Синхронизация',en:'Synchronization'},desc:{ru:e,en:e}},
  {level:7,name:{ru:'Эволюция II',en:'Evolution II'},desc:{ru:'Финальная форма способности',en:'Final ability form'}},
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
 blast:{ability:'blast',levels:abilityLevels('+20% урона','+15% радиуса','-10% КД','Волна получает дополнительный импульс при прохождении через сферу','Импульс может цепляться за соседнюю сферу','Радиус и сила цепочки увеличиваются'),evolution4:[ae('blast_resonance','Резонансный импульс','Импульс отскакивает от Standard-сфер'),ae('blast_network','Сетевой импульс','Каждая связанная сфера может передать волну дальше'),ae('blast_core','Ядро взрыва','Центр импульса наносит дополнительный урон')],evolution7:[ae('blast_echo_network','Echo Network','Импульс проходит по цепочке сфер и усиливается каждым узлом'),ae('blast_resonant_core','Resonant Core','Каждая Standard-сфера добавляет к импульсу дополнительную волну'),ae('blast_infinite_pulse','Infinite Pulse','Последний узел сети создаёт обратный импульс')]},
 shield:{ability:'shield',levels:abilityLevels('+1 заряд','+20% прочности','+1с длительности','Щит передаёт часть поглощённого урона ближайшим сферам','Сферы рядом получают защитный заряд','Защитный заряд усиливается после каждого поглощённого удара'),evolution4:[ae('shield_echo_guard','Эхо-барьер','Сферы принимают часть удара'),ae('shield_reflector','Отражающий контур','Часть поглощённого урона возвращается ближайшему врагу'),ae('shield_bastion','Бастион','Щит создаёт защитное кольцо вокруг ближайших сфер')],evolution7:[ae('shield_network_guard','Network Bastion','Щит соединяет сферы в единую защитную сеть'),ae('shield_iron_dome','Iron Dome','Сферы внутри сети получают щит при каждом полном цикле'),ae('shield_resonant_guard','Resonant Guard','Каждая защищённая сфера усиливает следующий заряд щита')]},
 teleport:{ability:'teleport',levels:abilityLevels('-2с КД','+1 точка назначения','+20% дальности','После прыжка ближайшая сфера получает импульс скорости атаки','Прыжок оставляет резонансный след','След может передавать заряд между сферами'),evolution4:[ae('teleport_echo_jump','Эхо-прыжок','Телепорт может притянуться к ближайшей сфере'),ae('teleport_beacon','Скачок-маяк','Телепорт помечает точку прыжка для следующей атаки'),ae('teleport_phase','Фазовый прыжок','Прыжок кратко делает игрока неуязвимым')],evolution7:[ae('teleport_spatial_network','Spatial Network','Прыжок соединяет исходную и целевую сферы'),ae('teleport_hunter_beacon','Hunter Beacon','Прыжок к Sniper усиливает следующую атаку по цели'),ae('teleport_phase_break','Phase Break','Каждая сфера на траектории создаёт дополнительный импульс')]},
 firetrail:{ability:'firetrail',levels:abilityLevels('+1с длительности','+20% урона','+20% ширины','Огненный след усиливает Fire-эффекты сфер','Поражённые враги передают горение соседям','Сферы рядом с огненным следом получают ускорение'),evolution4:[ae('firetrail_overdrive','Перегрев','След усиливает огненные сферы'),ae('firetrail_ignition','Воспламенитель','Огонь поджигает цели, уже поражённые статусом'),ae('firetrail_sanctum','Пылающее святилище','Огненный след создаёт зону усиления вокруг ауры')],evolution7:[ae('firetrail_network','Thermal Network','Огненный след соединяет сферы и передаёт перегрев'),ae('firetrail_catalyst','Catalyst Flame','Статусная реакция усиливает следующий огненный импульс'),ae('firetrail_inferno','Network Inferno','Каждый новый узел огня усиливает предыдущие')]},
 minion:{ability:'minion',levels:abilityLevels('+1 миньон','+20% урона','+2с жизни','Миньоны могут занимать узлы возле сфер','Миньон передаёт импульсы между соседними сферами','Каждый активный миньон усиливает сеть'),evolution4:[ae('minion_echo_drone','Эхо-дрон','Миньон становится частью сети'),ae('minion_relay_drone','Релейный дрон','Миньон передаёт срабатывания между сферами'),ae('minion_guardian','Дрон-страж','Миньон защищает ближайшую сферу')],evolution7:[ae('minion_echo_swarm','Echo Swarm','Дроны образуют дополнительные узлы сети'),ae('minion_network_nodes','Network Nodes','Каждый дрон связывает две ближайшие сферы'),ae('minion_sphere_guard','Sphere Guardians','Дроны усиливают сферы, которые защищают')]},
 lightning:{ability:'lightning',levels:abilityLevels('+1 цель','+20% урона','-15% КД','Разряд может пройти через Chain-сферу','Каждый переход усиливает следующий разряд','Связанные сферы сохраняют заряд дольше'),evolution4:[ae('lightning_echo_storm','Эхо-шторм','Молния проводится через Chain-сферу'),ae('lightning_relay','Релейный разряд','Разряд перескакивает между связанными сферами'),ae('lightning_overload','Перегрузка','Последний разряд наносит дополнительный урон')],evolution7:[ae('lightning_storm_network','Storm Network','Сеть создаёт последовательные разряды'),ae('lightning_thunder_chain','Thunder Network','Каждая Chain-сфера добавляет дополнительный переход'),ae('lightning_overload_core','Overload Core','Полный цикл сети заканчивается мощным разрядом')]},
 timestop:{ability:'timestop',levels:abilityLevels('+0.5с остановки','+20% радиуса','-10% КД','Сферы продолжают атаковать во время остановки','Замороженные враги получают больше урона от сфер','Каждая активная сфера продлевает окно на долю секунды'),evolution4:[ae('timestop_echo_phase','Фазовый разрыв','Сферы атакуют во время остановки'),ae('timestop_closed_time','Замкнутое время','Аура расширяется во время остановки'),ae('timestop_time_anchor','Якорь времени','Остановка фиксирует врагов вокруг ближайшей сферы')],evolution7:[ae('timestop_outside_time','Вне времени','Сферы продлевают остановку'),ae('timestop_closed_network','Closed Time Network','Сеть сфер сохраняет эффект остановки между узлами'),ae('timestop_temporal_core','Temporal Core','Последняя секунда остановки удваивает силу активной сети')]},
};

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