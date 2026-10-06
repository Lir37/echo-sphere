import type { GameState, Vec, EnemyRole } from './engineTypes';
export type RegionMode='stabilization'|'endless';
export type RegionChallengeId='none'|'fractured_network'|'overload'|'low_gravity';
export type RegionPoiType='resonance_cache'|'breach_node'|'echo_relay'|'lost_signal'|'elite_nest'|'rupture'|'boss_trace';
export interface RegionPocket{id:string;name:{ru:string;en:string};center:Vec;radius:number;ecology:'flow'|'swarm'|'pressure'|'fracture'|'hollow';accent:string}
export interface RegionPoi{id:number;type:RegionPoiType;pos:Vec;radius:number;alive:boolean;major:boolean;expiresAt:number}
export interface RegionChallenge{id:RegionChallengeId;name:{ru:string;en:string};desc:{ru:string;en:string}}
export interface RegionState{id:'resonance_basin';mode:RegionMode;pocketId:string;phaseId:number;phaseName:{ru:string;en:string};pois:RegionPoi[];nextPoiAt:number;nextBossAt:number;majorBossesSpawned:number;finalBossSpawned:boolean;finalBossDefeated:boolean;cleared:boolean;endlessUnlocked:boolean;challengeId:RegionChallengeId;challengeContractsCompleted:number;masteryGeometryTypes:string[];masteryDominantSeconds:number;loreDiscovered:string[];damageTaken:number;lastHp:number;regionGoldReward:number;regionXpReward:number}
export const REGION_STABILIZATION_SECONDS=1800,REGION_FINAL_PREP_SECONDS=1620;
export const REGION_BOSS_TIMES=[480,960] as const;
export const REGION_POI_TYPES:readonly RegionPoiType[]=['resonance_cache','breach_node','echo_relay','lost_signal','elite_nest','rupture','boss_trace'];
export const REGION_CHALLENGES:readonly RegionChallenge[]=[
{id:'fractured_network',name:{ru:'РАЗЛОМ СЕТИ',en:'FRACTURED NETWORK'},desc:{ru:'Каждые 45с случайная Сфера временно выпадает из Сети.',en:'Every 45s a random Sphere temporarily leaves the Network.'}},
{id:'overload',name:{ru:'ПЕРЕГРУЗКА',en:'OVERLOAD'},desc:{ru:'Враги быстрее и прочнее, но дают больше XP.',en:'Enemies are faster and tougher, but grant more XP.'}},
{id:'low_gravity',name:{ru:'НИЗКАЯ ГРАВИТАЦИЯ',en:'LOW GRAVITY'},desc:{ru:'Отбрасывание слабее, зато Core движется быстрее.',en:'Repulsion is weaker, while the Core moves faster.'}}];
export const REGION_POCKETS:readonly RegionPocket[]=[
{id:'axis',name:{ru:'ОСЕВОЙ УЗЕЛ',en:'AXIS NODE'},center:{x:0,y:0},radius:340,ecology:'flow',accent:'#39d8ff'},
{id:'glass',name:{ru:'СТЕКЛЯННЫЙ ПОТОК',en:'GLASS FLOW'},center:{x:520,y:-260},radius:330,ecology:'swarm',accent:'#7cf7d4'},
{id:'fracture',name:{ru:'РАЗЛОМ',en:'FRACTURE'},center:{x:500,y:430},radius:330,ecology:'fracture',accent:'#ff6b6b'},
{id:'hollow',name:{ru:'ПУСТОЙ КОНТУР',en:'HOLLOW CONTOUR'},center:{x:-480,y:430},radius:330,ecology:'hollow',accent:'#9b7cff'},
{id:'pressure',name:{ru:'ЗОНА ДАВЛЕНИЯ',en:'PRESSURE FIELD'},center:{x:-520,y:-300},radius:330,ecology:'pressure',accent:'#ffb84d'}];
export function createRegionState(mode:RegionMode='stabilization',challengeId:RegionChallengeId='none'):RegionState{return{id:'resonance_basin',mode,pocketId:'axis',phaseId:0,phaseName:{ru:'ОРИЕНТАЦИЯ',en:'ORIENTATION'},pois:[],nextPoiAt:70,nextBossAt:480,majorBossesSpawned:0,finalBossSpawned:false,finalBossDefeated:false,cleared:false,endlessUnlocked:false,challengeId,challengeContractsCompleted:0,masteryGeometryTypes:[],masteryDominantSeconds:0,loreDiscovered:[],damageTaken:0,lastHp:0,regionGoldReward:250,regionXpReward:0}}
export function getRegionPocket(s:GameState):RegionPocket{return REGION_POCKETS.find(p=>p.id===s.region?.pocketId)||REGION_POCKETS[0]}
export function getRegionPhase(t:number){if(t<300)return{id:0,name:{ru:'ОРИЕНТАЦИЯ',en:'ORIENTATION'}};if(t<600)return{id:1,name:{ru:'ПЕРВЫЙ РЕЗОНАНС',en:'FIRST RESONANCE'}};if(t<900)return{id:2,name:{ru:'СПЕЦИАЛИЗАЦИЯ',en:'SPECIALIZATION'}};if(t<1500)return{id:3,name:{ru:'ЭСКАЛАЦИЯ',en:'ESCALATION'}};if(t<1620)return{id:4,name:{ru:'СХОДИМОСТЬ',en:'CONVERGENCE'}};return{id:5,name:{ru:'СТАБИЛИЗАЦИЯ',en:'STABILIZATION'}}}
export function getRegionEnemyRole(_s:GameState,fallback:EnemyRole):EnemyRole{return fallback}
export function getRegionEnemyMultipliers(_s:GameState){return{hp:1,speed:1,damage:1,xp:1}}
export function getRegionMovementMultiplier(_s:GameState){return 1}
export function getRegionRepulsionMultiplier(_s:GameState){return 1}
export function getRegionResonanceMultiplier(_s:GameState){return 1}
export function collectRegionPoi(_s:GameState):RegionPoiType|null{return null}
export function updateRegion(_s:GameState,_dt:number){return{spawnBoss:false,finalBoss:false}}
export function persistRegionEndlessUnlock(_v:boolean){}
export function loadRegionEndlessUnlock(){return false}
