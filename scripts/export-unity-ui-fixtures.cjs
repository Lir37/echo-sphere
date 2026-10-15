const fs=require('node:fs'),path=require('node:path'),{createRequire}=require('node:module'),{createHash}=require('node:crypto');
const from=createRequire(path.resolve(process.env.ECHO_UI_DEPENDENCIES||'.','package.json'));
const viteRequire=createRequire(from.resolve('vite'));const esbuild=viteRequire('esbuild');const ts=from('typescript');
const roots=['Menu','SourceGameView','PausePlanner','Hud','ArtifactModal','UpgradeModal','ShopScreen','LeaderboardScreen','AchievementsScreen','SettingsScreen'];
const native=(file,name)=>(file==='persistence.ts'&&/^(load|save|reset|unlock|add)/.test(name))||(file==='region.ts'&&/^(load|save)/.test(name))||file==='runPersistence.ts'||file==='audio.ts'||['applyUpgrade','applyArtifact','rerollUpgradeChoices','lockUpgradeChoice','activateByKey','activateDash','placeSphere','setSphereType','canActivateFormationFollow','setFormationFollow','repositionSphere'].includes(name);
const referenceProgram=ts.createProgram(['src/App.tsx'],{target:ts.ScriptTarget.ESNext,module:ts.ModuleKind.ESNext,moduleResolution:ts.ModuleResolutionKind.Bundler,skipLibCheck:true,noEmit:true});
const gameViewSource=require('./source-ui-view.cjs')(ts,referenceProgram);
const react=`const el=(tag,props,...children)=>globalThis.__sourceUiElement(tag,props,...children);export const createElement=el,Fragment='fragment',forwardRef=render=>Object.assign(props=>render(props,null),{displayName:'Icon'}),useState=initial=>[typeof initial==='function'?initial():initial,()=>{}],useMemo=fn=>fn(),useRef=initial=>({current:initial}),useEffect=()=>{};export default {createElement,Fragment,forwardRef,useState,useMemo,useRef,useEffect};`;
const plugin={name:'reference-ui-tree',setup(build){
  build.onResolve({filter:/^react$/},()=>({path:'react',namespace:'source-ui-mock'}));build.onLoad({filter:/.*/,namespace:'source-ui-mock'},()=>({contents:react,loader:'js'}));
  build.onLoad({filter:/src[\\/].*\.tsx?$/},args=>{
    let contents=fs.readFileSync(args.path,'utf8');const file=path.basename(args.path),tree=ts.createSourceFile(args.path,contents,ts.ScriptTarget.Latest,true);
    if(file==='App.tsx')contents+=gameViewSource+'\nexport {'+roots.join(',')+'};\n';
    for(const node of tree.statements)if(ts.isFunctionDeclaration(node)&&node.name&&native(file,node.name.text)){
      const name=node.name.text;contents+=`\nconst __original_${name}=${name};${name}=(...args)=>{const capture=globalThis.__uiCapture,outer=(globalThis.__uiDepth||0)===0;globalThis.__uiDepth=(globalThis.__uiDepth||0)+1;try{const value=__original_${name}(...args);if(capture&&outer)capture.push({module:${JSON.stringify(file)},name:${JSON.stringify(name)},value});return value;}finally{globalThis.__uiDepth--;}};\n`;
    }
    return {contents,loader:file.endsWith('.tsx')?'tsx':'ts',resolveDir:path.dirname(args.path)};
  });
}};
function clean(value){
  if(typeof value==='function')return '$function';if(Array.isArray(value))return value.filter(x=>x!==undefined&&x!==null&&typeof x!=='boolean'&&x!=='').flatMap(x=>Array.isArray(x)?clean(x):[clean(x)]);
  if(value&&typeof value==='object')return Object.fromEntries(Object.keys(value).filter(k=>!['key','ref'].includes(k)&&value[k]!==undefined).sort().map(k=>[k,clean(value[k])]));return value;
}
function element(tag,props,...children){if(typeof tag==='function')return tag({...props,children});const {children:attributeChildren,...attributes}=props||{};return {tag,properties:clean(attributes),children:clean(children.length?children:attributeChildren===undefined?[]:[attributeChildren])};}
async function main(){
  const result=await esbuild.build({stdin:{contents:`import * as App from './src/App';import CharacterSelect from './src/CharacterSelect';import KnowledgeBase from './src/KnowledgeBase';import EchoMapScreen from './src/EchoMapScreen';import MobileControls from './src/MobileControls';import TutorialOverlay from './src/TutorialOverlay';import * as Icons from 'lucide-react';import * as Engine from './src/engine';import {translations} from './src/i18n';import {CHARACTER_DEFS} from './src/characters';import {sphereModifiers} from './src/sphereProgression';export {App,CharacterSelect,KnowledgeBase,EchoMapScreen,MobileControls,TutorialOverlay,Icons,Engine,translations,CHARACTER_DEFS,sphereModifiers};`,resolveDir:process.cwd(),loader:'ts'},bundle:true,platform:'node',format:'cjs',write:false,jsx:'transform',jsxFactory:'globalThis.__sourceUiElement',jsxFragment:'"fragment"',nodePaths:[path.resolve(process.env.ECHO_UI_DEPENDENCIES,'node_modules')],plugins:[plugin],metafile:true,logLevel:'error'});
  globalThis.__sourceUiElement=element;globalThis.localStorage={getItem:()=>null,setItem:()=>{},removeItem:()=>{}};globalThis.window={innerWidth:390,innerHeight:844};globalThis.location={reload:()=>{}};
  const module={exports:{}};new Function('module','exports','require',result.outputFiles[0].text)(module,module.exports,require);const reference=module.exports;
  const fixtures=[];const noOp=()=>{};
  for(const lang of ['ru','en'])for(const id of [...roots,'CharacterSelect','KnowledgeBase','EchoMapScreen','MobileControls','TutorialOverlay']) {
    const state=reference.Engine.createInitialState({upgrades:{}},'Visual source','normal',123);state.nativeNetwork=reference.Engine.getNetworkFrame(state);
    state.nativeSphereModifiers=Object.fromEntries(Object.keys(reference.CHARACTER_DEFS).length?['standard','sniper','shotgun','chain','aura','orbital','prism','gravity','pulse','void'].map(type=>[type,reference.sphereModifiers(state,type)]):[]);
    state.pendingUpgrade=reference.Engine.generateUpgradeChoices(state);
    const tabs=id==='PausePlanner'?['stats','skills','artifacts','synergies']:id==='TutorialOverlay'?['0','1','2','3','4','5']:id==='MobileControls'?Object.keys(reference.CHARACTER_DEFS):id==='SourceGameView'?['playing','pause','combo','upgrade','artifact','result','clear']:['stats'];
    for(const tab of tabs) {
      const callbacks=Object.fromEntries(['setDifficulty','onContinue','onOpenMap','onShop','onCharacters','onLeader','onSettings','onAchievements','onKnowledge','onBack','onGoldChange','onSelected','onStartRun','setShop','setLang','setSoundOn','setHandedness','onReplayTutorial','onPick','onLock','onReroll','setTab','onResume','onExit','setSelectedArtifactSetId','onPause','onNext','onSkip','setPaused','setPauseTab','transitionTutorial','setTutorialStep'].map(key=>[key,noOp]));
      if(id==='MobileControls')state.player.characterId=tab;
      if(id==='SourceGameView'){state.pendingUpgrade=tab==='upgrade'?reference.Engine.generateUpgradeChoices(state):null;state.pendingArtifact=tab==='artifact'?['crystal_speed','radius_shard','resonance_core']:null;state.player.combo=tab==='combo'?60:0;}
      const props={...callbacks,lang,t:key=>reference.translations[lang][key],difficulty:'normal',savedRunTime:83,gold:1000,shop:{gold:1000,upgrades:{}},soundOn:true,handedness:'right',st:state,tab,selectedArtifactSetId:null,choices:['crystal_speed','radius_shard','resonance_core'],stateRef:{current:state},canvasRef:{current:null},tutorialStep:null,step:id==='TutorialOverlay'?Number(tab):0,canAdvance:true,gameOverData:id==='SourceGameView'&&['result','clear'].includes(tab)?{regionClear:tab==='clear',isNewRecord:true,time:167,wave:12,kills:300,gold:110,challengeProgress:2,endlessUnlocked:false,masteryChallengesCompleted:1,rank:2}:null,paused:id==='SourceGameView'&&tab==='pause',pauseTab:'stats',tutorialStartPosRef:{current:{x:0,y:0}},tutorialStepRef:{current:null}};
      globalThis.__uiCapture=[];const root=roots.includes(id)?reference.App[id]:reference[id];const tree=clean(element(root,props));const calls=JSON.parse(JSON.stringify(globalThis.__uiCapture));globalThis.__uiCapture=null;
      fixtures.push({label:id+'/'+lang+'/'+tab,module:roots.includes(id)?'App':id,name:id,props:JSON.parse(JSON.stringify(props,(_key,value)=>typeof value==='function'?'$function':value)),translations:reference.translations[lang],calls,tree});
    }
  }
  const icons={};const text=['App','CharacterSelect','KnowledgeBase','EchoMapScreen','MobileControls','TutorialOverlay'].map(name=>fs.readFileSync('src/'+name+'.tsx','utf8')).join('\n');
  for(const match of text.matchAll(/<([A-Z][A-Za-z0-9]*)(?:\s|\/>)/g)){const name=match[1];if(reference.Icons[name])icons[name]=clean(element(reference.Icons[name],{}));}
  for(const imported of text.matchAll(/import\s*\{([^}]+)\}\s*from\s*'lucide-react'/g))for(const entry of imported[1].split(',')){const name=entry.trim().split(/\s+as\s+/)[0];if(reference.Icons[name])icons[name]=clean(element(reference.Icons[name],{}));}
  const iconPath='UnityProject/Assets/Resources/EchoSphere/SourceUiIcons.json';const fixturePath='tools/EchoSphere.VisualChecks/source-ui-fixtures.json';
  for(const [file,value] of [[iconPath,icons],[fixturePath,fixtures]]){const data=JSON.stringify(value)+'\n';if(process.argv.includes('--check')){if(fs.readFileSync(file,'utf8').replaceAll('\r\n','\n')!==data)throw new Error('Source UI fixture changed: '+file);}else fs.writeFileSync(file,data);}
  console.log(`Source TSX: ${fixtures.length} complete retained UI trees; ${Object.keys(icons).length} original Lucide icons.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
