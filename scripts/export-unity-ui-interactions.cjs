// Runs the unchanged component with React's actual hook/effect implementation.
// Native gameplay boundaries are recorded separately from UI state transitions.
const fs=require('node:fs'),path=require('node:path'),Module=require('node:module'),zlib=require('node:zlib');
const from=Module.createRequire(path.resolve(process.env.ECHO_UI_DEPENDENCIES||'.','package.json'));const esbuild=Module.createRequire(from.resolve('vite'))('esbuild'),ts=from('typescript'),React=from('react'),Test=from('react-test-renderer');
const actions=new Set(['placeSphere','repositionSphere','setSphereType','setFormationFollow','activateDash','activateByKey']);
const output='tools/EchoSphere.VisualChecks/source-ui-interactions.json.gz';
function clean(value){if(typeof value==='function')return '$function';if(Array.isArray(value))return value.map(clean);if(value&&typeof value==='object')return Object.fromEntries(Object.keys(value).filter(key=>!['key','ref'].includes(key)&&value[key]!==undefined).sort().map(key=>[key,clean(value[key])]));return value;}
function tree(value){if(Array.isArray(value))return value.map(tree);if(value&&typeof value==='object')return{tag:value.type,properties:clean(value.props),children:(value.children||[]).map(tree)};return value;}
async function main(){
 let clock=1000,nextTimer=0,timers=new Map(),calls=[],reads={},haptics=[],currentState,reference,ids=new WeakMap(),nextEntity=0;
 const storage=new Map([['echosphere_interface_scale','1'],['echosphere_lang','ru']]);globalThis.localStorage={getItem:key=>storage.get(key)??null,setItem:(key,value)=>storage.set(key,String(value)),removeItem:key=>storage.delete(key)};
 class Element {constructor(control=false){this.control=control;this.width=390;this.height=844;}closest(){return this.control?this:null;}setPointerCapture(){}releasePointerCapture(){}hasPointerCapture(){return true;}getBoundingClientRect(){return{left:0,top:0,x:0,y:0,width:390,height:844,right:390,bottom:844};}}
 globalThis.Element=Element;Object.defineProperty(globalThis,'navigator',{configurable:true,value:{vibrate:duration=>{haptics.push(duration);return true;}}});
 const schedule=(fn,delay,frame=false,interval=false)=>{const id=++nextTimer;timers.set(id,{id,fn,at:clock+(frame?0:Number(delay)||0),period:interval?Math.max(1,Number(delay)||0):0,frame});return id;};const cancel=id=>timers.delete(id);
 globalThis.window={innerWidth:390,innerHeight:844,setTimeout:(fn,delay)=>schedule(fn,delay),clearTimeout:cancel,setInterval:(fn,delay)=>schedule(fn,delay,false,true),clearInterval:cancel,addEventListener(){},removeEventListener(){}};
 globalThis.requestAnimationFrame=fn=>schedule(fn,0,true);globalThis.cancelAnimationFrame=cancel;globalThis.performance={now:()=>clock};Date.now=()=>clock;
 globalThis.document={body:{},querySelector:()=>null,querySelectorAll:()=>[]};globalThis.MutationObserver=class{observe(){}disconnect(){}};
 function enrich(){currentState.nativeNetwork=reference.Engine.getNetworkFrame(currentState);currentState.nativeSphereModifiers=Object.fromEntries(Object.keys(reference.Spheres.SPHERE_PROGRESSION).map(type=>[type,reference.Spheres.sphereModifiers(currentState,type)]));}
 function snapshot(){enrich();return{state:clean(currentState),entities:currentState.spheres.map(sphere=>{if(!ids.has(sphere))ids.set(sphere,++nextEntity);return ids.get(sphere);})};}
 globalThis.__sourceInteractionArguments=args=>clean(args.slice(1));
 globalThis.__sourceInteraction=(file,name,args,result,before)=>{if(actions.has(name))calls.push({module:file,name,args:before,result:clean(result),after:snapshot()});else reads[file+':'+name]=clean(result);};
 const plugin={name:'reference-native-boundaries',setup(build){build.onLoad({filter:/src[\\/].*\.tsx?$/},args=>{let code=fs.readFileSync(args.path,'utf8');const file=path.basename(args.path),source=ts.createSourceFile(args.path,code,ts.ScriptTarget.Latest,true);for(const node of source.statements)if(ts.isFunctionDeclaration(node)&&node.name&&(actions.has(node.name.text)||node.name.text==='canActivateFormationFollow')){const name=node.name.text;code+=`\nconst __reference_${name}=${name};${name}=(...args)=>{const before=globalThis.__sourceInteractionArguments(args);const result=__reference_${name}(...args);globalThis.__sourceInteraction(${JSON.stringify(file)},${JSON.stringify(name)},args,result,before);return result;};\n`;}return{contents:code,loader:file.endsWith('.tsx')?'tsx':'ts',resolveDir:path.dirname(args.path)};});}};
 const result=await esbuild.build({stdin:{contents:"import MobileControls from './src/MobileControls';import * as Engine from './src/engine';import * as Spheres from './src/sphereProgression';import {translations} from './src/i18n';export {MobileControls,Engine,Spheres,translations};",resolveDir:process.cwd(),loader:'ts'},bundle:true,platform:'node',format:'cjs',write:false,jsx:'automatic',external:['react','react/jsx-runtime','react-dom','lucide-react'],nodePaths:[path.resolve(process.env.ECHO_UI_DEPENDENCIES||'.','node_modules')],plugins:[plugin],logLevel:'error'});
 const filename=path.resolve(process.env.ECHO_UI_DEPENDENCIES||'.','source-ui-interaction-reference.cjs');const compiled=new Module(filename,module);compiled.filename=filename;compiled.paths=Module._nodeModulePaths(path.dirname(filename));compiled._compile(result.outputFiles[0].text,filename);reference=compiled.exports;
 const fixtures=[];
 for(const lang of ['ru','en'])for(const handedness of ['right','left']){
  clock=1000;timers.clear();nextTimer=0;ids=new WeakMap();nextEntity=0;calls=[];reads={};haptics=[];storage.set('echosphere_lang',lang);
  currentState=reference.Engine.createInitialState({upgrades:{}},'Interaction reference','normal',123);const initial=snapshot();let propCalls=[];
  const props={lang,handedness,t:key=>reference.translations[lang][key],tutorialStep:null,stateRef:{current:currentState},canvasRef:{current:new Element()},onPause:()=>propCalls.push('onPause')};let renderer;
  Test.act(()=>{renderer=Test.create(React.createElement(reference.MobileControls,props),{createNodeMock:()=>new Element()});});
  const fixture={label:'MobileControls/'+lang+'/'+handedness,lang,handedness,initial,frames:[]};
  function capture(step){fixture.frames.push({step,time:clock,tree:tree(renderer.toJSON()),state:snapshot(),calls:clean(calls),reads:clean(reads),haptics:[...haptics],props:[...propCalls],timers:timers.size});calls=[];haptics=[];propCalls=[];}
  capture({kind:'mount'});
  function event(step){clock+=16;const selector=step.selector||{root:true};let element=selector.root?renderer.root.find(node=>node.type==='div'&&node.props.className?.startsWith('es-mobile-controls')):renderer.root.findAllByType('button').filter(node=>selector.aria?node.props['aria-label']===selector.aria:node.props['aria-label']?.includes(selector.ariaContains))[selector.index||0];if(!element)throw Error('Missing interaction target '+JSON.stringify(step));
   if(step.sphere!==undefined){step.x=195+currentState.spheres[step.sphere].pos.x;step.y=422+currentState.spheres[step.sphere].pos.y;}
   const host=new Element(!selector.root),evt={clientX:step.x||0,clientY:step.y||0,pointerId:step.pointer||1,target:host,currentTarget:host,pointerType:'touch',button:0,buttons:1,stopPropagation(){},preventDefault(){}};evt.nativeEvent=evt;
   Test.act(()=>{element.props[step.event](evt);renderer.update(React.createElement(reference.MobileControls,props));});capture(step);
  }
  function advance(milliseconds){clock+=milliseconds;Test.act(()=>{for(const timer of [...timers.values()].sort((a,b)=>a.id-b.id))if(timer.at<=clock&&timers.has(timer.id)){if(timer.period)timer.at=clock+timer.period;else timers.delete(timer.id);timer.fn(timer.frame?clock:undefined);}renderer.update(React.createElement(reference.MobileControls,props));});capture({kind:'advance',milliseconds});}
  const joy=handedness==='right'?300:90,placement=handedness==='right'?55:335;
  event({kind:'event',event:'onPointerDown',x:joy,y:600});event({kind:'event',event:'onPointerMove',x:joy+50,y:570});event({kind:'event',event:'onPointerUp',x:joy+50,y:570});
  event({kind:'event',event:'onPointerDown',selector:{ariaContains:' — ',index:3}});
  event({kind:'event',event:'onPointerDown',x:placement,y:430,pointer:2});event({kind:'event',event:'onPointerUp',x:placement,y:430,pointer:2});advance(449);advance(2);
  event({kind:'event',event:'onPointerDown',sphere:0,pointer:3});event({kind:'event',event:'onPointerMove',x:135,y:300,pointer:3});event({kind:'event',event:'onPointerUp',x:135,y:300,pointer:3});
  event({kind:'event',event:'onPointerDown',sphere:0,pointer:4});event({kind:'event',event:'onPointerUp',sphere:0,pointer:4});advance(450);advance(500);
  event({kind:'event',event:'onPointerDown',selector:{aria:reference.translations[lang].pause}});
  Test.act(()=>renderer.unmount());fixture.unmounted={keys:clean(currentState.keys),timers:timers.size};fixtures.push(fixture);
 }
 const buffer=zlib.gzipSync(JSON.stringify(fixtures)+'\n');if(process.argv.includes('--check')){if(!fs.readFileSync(output).equals(buffer))throw Error('React interaction reference changed');}else fs.writeFileSync(output,buffer);console.log(`Original React UI interactions: ${fixtures.length} language/handedness sequences / ${fixtures.reduce((n,x)=>n+x.frames.length,0)} frames.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
