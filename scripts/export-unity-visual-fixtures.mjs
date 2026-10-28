import { registerHooks } from 'node:module';
import { existsSync, writeFileSync, readFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
registerHooks({resolve(specifier,context,next){ if(specifier.startsWith('.')&&!/\.[a-z]+$/i.test(specifier)){const url=new URL(specifier+'.ts',context.parentURL);if(existsSync(url))return next(url.href,context);}return next(specifier,context);}});
const {SPHERE_PROGRESSION}=await import('../src/sphereProgression.ts');
const {createInitialState}=await import('../src/engineState.ts');
const {getNetworkFrame}=await import('../src/engineRuntime.ts');
const {sphereModifiers}=await import('../src/sphereProgression.ts');
const sourceArt=JSON.parse(readFileSync('UnityProject/Assets/Resources/EchoSphere/CanvasArt/source-art-manifest.json','utf8'));
globalThis.Image=class {complete=false;naturalWidth=0;naturalHeight=0;_src='';get src(){return this._src;}set src(value){this._src=value;const asset=sourceArt.assets.find(a=>a.url===value);this.complete=!!asset?.loaded;this.naturalWidth=asset?.width||0;this.naturalHeight=asset?.height||0;}};
globalThis.performance={now:()=>0};
globalThis.localStorage={getItem:()=>null,setItem:()=>{}};
Math.random=()=>.5;
const modules={};
for(const file of ['aura','chain','elemental','gravity','modifier','mutation','orbital','prism','pulse','shotgun','sniper','void'])modules[file]=await import('../src/spheres/'+file+'Visual.ts');
modules.enemy=await import('../src/enemies/enemyVisual.ts');
modules.renderer=await import('../src/renderer.ts');
class Gradient {constructor(kind,args){this.kind=kind;this.coordinates=args;this.stops=[];}addColorStop(...stop){this.stops.push(stop);}}
function context(){
  let trace='',count=0; const defaults={globalAlpha:1,globalCompositeOperation:'source-over',fillStyle:'#000000',strokeStyle:'#000000',lineWidth:1,lineCap:'butt',lineJoin:'miter',shadowBlur:0,shadowColor:'rgba(0,0,0,0)'};let properties={...defaults};const stack=[];
  function value(v){if(typeof v==='number')return 'N'+(Number.isFinite(v)?Math.floor(v*1000000+.5).toFixed(0):String(v))+'|';if(typeof v==='string'){if(/^rgba?\(/.test(v))return 'COLOR['+v.slice(v.indexOf('(')+1,-1).split(',').map(Number).map(value).join('')+']|';return 'S'+v.length+':'+v+'|';}if(typeof v==='boolean')return v?'T|':'F|';if(v instanceof Gradient)return 'G'+v.kind+'['+v.coordinates.map(value).join('')+v.stops.flatMap(stop=>stop).map(value).join('')+']|';if(v&&typeof v.src==='string')return 'IMAGE['+value(v.src)+']|';if(Array.isArray(v))return '['+v.map(value).join('')+']|';return v===null?'null|':'undefined|';}
  const record=(kind,name,args)=>{count++;trace+=kind+':'+name+'|'+args.map(value).join('')+'\n';};
   return {ctx:new Proxy({}, {get(_,name){if(name==='canvas')return {clientWidth:390,clientHeight:844,width:390,height:844};if(name in properties)return properties[name];return (...args)=>{if(name==='save')stack.push({...properties});if(name==='restore'&&stack.length)properties=stack.pop();record('call',name,args);if(name==='measureText')return {width:String(args[0]).length*7};if(name==='createRadialGradient'||name==='createLinearGradient')return new Gradient(name,args);};},set(_,name,v){properties[name]=v;record('set',name,[v]);return true;}}),result(){return {count,sha:createHash('sha256').update(trace).digest('hex')};},raw(){return trace;},clear(){trace='';count=0;}};
}
const scenarios=[];
function add(label,module,name,args){const c=context();modules[module][name](c.ctx,...structuredClone(args));scenarios.push({label,module,name,args,...c.result()});if(process.env.ECHO_VISUAL_TRACE_LABEL===label)writeFileSync(process.env.ECHO_VISUAL_TRACE_PATH,c.raw());}
const emptyMods=Object.fromEntries(['multishot','pierce','ricochet','fire','freeze','poison'].map(x=>[x,0]));
for(const [type,def] of Object.entries(SPHERE_PROGRESSION)) {
  const states=[{level:0,branch:'',final:-1},{level:1,branch:'',final:-1},{level:3,branch:'',final:-1}];
  for(const branch of def.evolution4Choices){for(const level of [4,5,6])states.push({level,branch:branch.id,final:-1});for(let final=0;final<3;final++)states.push({level:7,branch:branch.id,final});}
  for(const state of states)for(const variant of [0,1,2]) {
    const time=[.37,1.25,3.6][variant],scale=Math.min(25,130*.19+state.level*.8)/24;
    const sphere={type,alive:variant!==2,pos:{x:123.5,y:-46.25},radius:130,visualTier:state.level,rotation:1.125,attackTimer:.027,attackDelay:.85,auraTimer:.17,networkDisabledTimer:variant===1?1.5:0};
    const player={sphereProgression:{[type]:state.level},sphereBranches:state.branch?{[type]:state.branch}:{},evolutions:state.final>=0?[`sphere:${type}:7:${state.branch}:${state.final}`]:[],sphereMods:variant===0?emptyMods:{...emptyMods,multishot:2,pierce:2,ricochet:2,fire:2,freeze:2,poison:2},artifacts:variant===0?['orbital_crown']:[],resonanceEventActive:variant===0};
    const label=`${type}/${state.level}/${state.branch}/${state.final}/${variant}`;
    if(type!=='standard')add(label+'/family',type,`render${type[0].toUpperCase()+type.slice(1)}SphereRuntimeVfx`,type==='orbital'?[sphere,player,time,scale,[{hp:10,pos:{x:155,y:-28}}],1.75]:[sphere,player,time,scale]);
    add(label+'/mutation','mutation','renderSphereMutationVfx',[sphere,player,time,scale]);
    add(label+'/element','elemental','renderSphereElementalVfx',[sphere,player,time,scale]);
    add(label+'/modifier','modifier','renderSphereModifierVfx',[sphere,player,time,scale]);
  }
}
for(const variant of ['wisp','leech','serpent','skitter','moth','beetle','brute','prism','stalker','linkbreaker'])for(const time of [0,.37,2.5])add(variant+'/'+time,'enemy','drawEnemyCreature',[{radius:19,color:'#4a7a8a',visualVariant:variant},time]);
for(const bossType of ['shooter','charger','summoner','aura','conductor','architect','null','stella_warden'])for(const time of [0,.37,2.5])add(bossType+'/'+time,'enemy','drawBossCreature',[{radius:34,color:'#c4453d',bossType},time]);
const initialState=createInitialState({upgrades:{}},'Visual parity','normal',123);
for(const [type,definition] of Object.entries(SPHERE_PROGRESSION))for(const branch of definition.evolution4Choices)for(let final=0;final<3;final++) {
  const state=structuredClone(initialState);state.time=3.7;state.player.level=25;state.player.characterMasteryLevel=7;state.player.sphereProgression[type]=7;state.player.sphereBranches[type]=branch.id;state.player.evolutions=[`sphere:${type}:7:${branch.id}:${final}`];
  state.player.sphereMods={...state.player.sphereMods,multishot:2,pierce:2,ricochet:2,fire:2,freeze:2,poison:2};
  state.player.shieldCharges=3;state.player.shieldTimer=2;state.player.shieldVisualPulse=.5;state.player.resonanceEventActive=true;
  state.spheres=[{...state.spheres[0],type,visualTier:7,attackTimer:.02,auraTimer:.2,rotation:1.3},
    {...state.spheres[0],pos:{x:-110,y:-95},type:'standard',visualTier:3},
    {...state.spheres[0],pos:{x:110,y:-95},type:'standard',visualTier:3}];
  state.enemies=[{pos:{x:130,y:-20},hp:70,maxHp:100,radius:19,color:'#4a7a8a',type:'normal',visualVariant:'wisp',isElite:true,eliteVariant:'linkbreaker',rotation:.7,tier:3,hitFlash:.07,freezeTimer:1,poisonTimer:1,fireTimer:1,elitePulseTarget:{pos:{x:0,y:-95}},elitePulseTelegraphTimer:.7,bossProjectiles:[]}];
  state.minions=[{pos:{x:35,y:-40},radius:10,rotation:.5,life:5,anchorType:type}];
  state.sphereProjectiles=[{pos:{x:95,y:-40},vel:{x:100,y:20},radius:5,color:'#39d8ff',life:2,sourceSphere:{type},effect:'fire',alive:true}];
  state.lightnings=[{from:{x:0,y:-95},to:{x:130,y:-20},life:.2,sourceSphere:{type}}];
  state.runes=['overdrive','phase','harvest','purge','resonance','fortify','hunt','echo','gravity'].map((type,i)=>({pos:{x:-100+i*25,y:45},type,alive:true}));
  state.xpOrbs=[{pos:{x:0,y:70},radius:4,rotation:.3}];state.healthPacks=[{pos:{x:25,y:70}}];state.chests=[{pos:{x:-50,y:70},alive:true}];state.stellaChests=[{pos:{x:-100,y:70},alive:true}];
  state.damageNumbers=[{pos:{x:130,y:-45},value:18,life:.4,maxLife:.8,crit:true,sourceSphereType:type}];
  state.particles=[{pos:{x:120,y:-25},vel:{x:15,y:-30},life:.2,maxLife:.4,size:3,color:'#4a7a8a'}];
  state.formationMemory={type:'triangle',nodes:state.spheres.map(s=>({...s.pos})),expiresAt:4.2};
  state.flashText={text:'BOUNDARY FOLD',color:'#39d8ff',kind:'boundary',life:.7};
  state.nativeNetwork=getNetworkFrame(state);state.nativeSphereModifiers=Object.fromEntries(Object.keys(SPHERE_PROGRESSION).map(type=>[type,sphereModifiers(state,type)]));
  add(`battle/${type}/${branch.id}/${final}`,'renderer','render',[state,390,844]);
}
for(const characterId of ['spherist','hunter','engineer','berserker','architect','alchemist','conductor','oracle','fractal','voidwalker'])for(const time of [0,.37,2.5]) {
  const state=structuredClone(initialState);state.player.characterId=characterId;state.time=time;
  state.nativeNetwork=getNetworkFrame(state);state.nativeSphereModifiers=Object.fromEntries(Object.keys(SPHERE_PROGRESSION).map(type=>[type,sphereModifiers(state,type)]));
  add(`scene/${characterId}/${time}`,'renderer','render',[state,390,844]);
}
for(const bossType of ['shooter','charger','summoner','aura','conductor','architect','null','stella_warden'])for(const phase of [0,1,2]){
  const state=structuredClone(initialState);state.time=2.31+phase*.2;
  state.enemies=[{pos:{x:100,y:-45},hp:120,maxHp:250,radius:34,color:'#c4453d',type:'boss',bossType,isBoss:true,isElite:false,rotation:.6,tier:2,hitFlash:phase===1?.1:0,freezeTimer:0,fireTimer:0,poisonTimer:0,bossShootTimer:phase===0?.08:phase===1?.5:2,summonTimer:.3,chargeTimer:phase===0?.6:.1,isCharging:phase===2,chargeDir:{x:1,y:0},auraRadius:150,bossProjectiles:[{pos:{x:40,y:-30},vel:{x:120,y:20},radius:7,visualType:bossType}]}];
  state.nativeNetwork=getNetworkFrame(state);state.nativeSphereModifiers=Object.fromEntries(Object.keys(SPHERE_PROGRESSION).map(type=>[type,sphereModifiers(state,type)]));add(`boss-scene/${bossType}/${phase}`,'renderer','render',[state,390,844]);
}
{
  const state=structuredClone(initialState);state.enemies=[{pos:{x:80,y:-30},hp:70,maxHp:100,radius:19,color:'#4a7a8a',type:'normal',visualVariant:'skitter',isElite:false,isBoss:false,rotation:.7,tier:1,hitFlash:0,freezeTimer:0,poisonTimer:0,fireTimer:0,bossProjectiles:[]}];
  state.nativeNetwork=getNetworkFrame(state);state.nativeSphereModifiers=Object.fromEntries(Object.keys(SPHERE_PROGRESSION).map(type=>[type,sphereModifiers(state,type)]));
  const fixture={label:'stateful/player-animation-enemy-facing',module:'renderer',name:'render',args:[structuredClone(state),390,844],frames:[]};const c=context();
  for(const patch of [
    {time:0}, {time:.016,'player.pos':{x:3,y:0},'enemies.0.pos':{x:79,y:-30}},
    {time:.032,'player.pos':{x:6,y:0},'enemies.0.pos':{x:79,y:-27}},
    {time:.048,'player.pos':{x:3,y:0},'enemies.0.pos':{x:82,y:-27}},
    {time:.064,'player.pos':{x:0,y:0},'enemies.0.pos':{x:82.1,y:-27}},
    {time:.08,'player.dashTimer':.12,'player.dashDir':{x:-1,y:0},'player.pos':{x:-8,y:0}},
    {time:.096,'player.dashTimer':0,'player.deathTimer':.8,'player.hp':0},
    {time:.5,'player.deathTimer':.4},
  ]){
    for(const[key,value]of Object.entries(patch)){const parts=key.split('.');const last=parts.pop();let owner=state;for(const part of parts)owner=owner[part];owner[last]=value;}
    c.clear();modules.renderer.render(c.ctx,state,390,844);fixture.frames.push({patch,...c.result()});
  }
  scenarios.push(fixture);
}
function cs(value) {if(value===null)return 'VisualJs.Null';if(Array.isArray(value))return 'VisualJs.Arr('+value.map(cs).join(',')+')';if(typeof value==='object')return 'VisualJs.Obj('+Object.entries(value).flatMap(([k,v])=>[cs(k),cs(v)]).join(',')+')';return typeof value==='number'?'new V('+value+'d)':typeof value==='string'?'new V('+JSON.stringify(value)+')':'new V('+value+')';}
const statePath='UnityProject/Assets/EchoSphere/Scripts/Presentation/Generated/VisualInitialState.cs';
const stateCode='#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS\n// Generated from engineState.ts:createInitialState, not hand-authored defaults.\nnamespace EchoSphere.Presentation { public static class VisualInitialState { public static V Create()=>'+cs(initialState)+'; } }\n#endif\n';
if(process.argv.includes('--check')){if(readFileSync(statePath,'utf8').replaceAll('\r\n','\n')!==stateCode)throw new Error('Source initial render state changed');}else writeFileSync(statePath,stateCode);
const output='tools/EchoSphere.VisualChecks/source-visual-fixtures.json';const text=JSON.stringify(scenarios)+'\n';
if(process.argv.includes('--check')) {if(readFileSync(output,'utf8').replaceAll('\r\n','\n')!==text)throw new Error('Source visual command fixtures changed');}else writeFileSync(output,text);
console.log(`Source visual fixtures: ${scenarios.length} complete drawing command streams.`);
