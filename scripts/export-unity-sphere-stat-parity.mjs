import {registerHooks} from 'node:module';
import {existsSync,writeFileSync} from 'node:fs';
registerHooks({resolve(specifier,context,next){if(specifier.startsWith('.')&&!/\.[a-z]+$/i.test(specifier)){const url=new URL(specifier+'.ts',context.parentURL);if(existsSync(url))return next(url.href,context);}return next(specifier,context);}});
const {SPHERE_TYPES,ARTIFACTS}=await import('../src/gameData.ts');
const {CHARACTER_DEFS}=await import('../src/characters.ts');
const {sphereModifiers}=await import('../src/sphereProgression.ts');
const {getSphereRadius,getSphereDelay,getSphereDamage,getSphereDpsEstimate}=await import('../src/engineSpheres.ts');
const fixtures=[];
for(const type of Object.keys(SPHERE_TYPES))for(const character of Object.keys(CHARACTER_DEFS))for(let variant=0;variant<3;variant++){
 const spheres=Array.from({length:4},(_,i)=>({type,alive:true,pos:{x:70+i%2*70,y:30+Math.floor(i/2)*70}}));const sphere=spheres[variant===2?2:0];
 const player={characterId:character,characterMasteryLevel:variant===0?1:10,pos:{x:0,y:0},hp:20,maxHp:100,abilities:{damage:3,attackspeed:4,radius:2,sphereboost:3,darkritual:7,firetrail:6},artifacts:[],sphereProgression:Object.fromEntries(Object.keys(SPHERE_TYPES).map(id=>[id,variant===0?2:7])),sphereBranches:{orbital:'orbital_dance'},sphereMods:{},evolutions:['echoaccumulator','ability:firetrail:4:firetrail_overdrive','ability:timestop:7:timestop_temporal_core','sphere:orbital:7:orbital_dance:2'],chaosOrbBuff:'dmg',chaosOrbBuffTimer:1,mutationStage:3,teleportDamageBuffTimer:1,overloadTimer:1,fireTrailTimer:1,fireCatalystTimer:1,timestopTimer:.5,sphereXpAccumulator:7,kills:250,conductorOverdriveTimer:1,architectFormationType:'cluster',architectFormationChangedAt:1};
 const network={dominantFormation:{type:'cluster',nodes:[0,1,2,3]},secondaryFormation:null,formationEfficiency:.75,links:[]};
 const scene={player,spheres,enemies:Array.from({length:4},(_,i)=>({hp:30,isElite:true,pos:{x:70+i*10,y:40}})),time:2,shopUpgrades:{dmg:4,radius:3},nativeNetwork:network,networkFrame:{frameId:1,network},networkFrameId:1};
 scene.nativeSphereModifiers={[type]:sphereModifiers(scene,type,sphere)};
 const expected={radius:getSphereRadius(scene,sphere),delay:getSphereDelay(scene,sphere),damage:getSphereDamage(scene,sphere),dps:getSphereDpsEstimate(scene,sphere)};
 delete scene.networkFrame;delete scene.networkFrameId;
 fixtures.push({name:`${type}/${character}/${variant}`,index:variant===2?2:0,scene,expected});
}
writeFileSync('UnityProject/Library/native-sphere-stat-parity.json',JSON.stringify({source:'Original TypeScript engineSpheres.ts; artifact-free states to keep native cached modifier input identical to the source instance modifiers',fixtures}));console.log(`Original sphere stat fixtures: ${fixtures.length}`);
