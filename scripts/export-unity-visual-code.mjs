import { readFileSync, writeFileSync, mkdirSync, existsSync, readdirSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { createRequire } from 'node:module';
import { dirname, basename, resolve, relative } from 'node:path';

// Source expressions are translated mechanically. Geometry/constants/branches are never
// substituted with common silhouettes. Unsupported syntax stops the export.
const sourceFiles=['src/spheres/visualHelpers.ts',...readdirSync('src/spheres').filter(x=>/Visual\.ts$/.test(x)&&x!=='visualHelpers.ts').map(x=>'src/spheres/'+x),'src/enemies/enemyVisual.ts','src/renderer.ts'];
const uiRoots=new Map([
  ['src/App.tsx',['Menu','SourceGameView','SourceInterfaceScaleMarkup','PausePlanner','Hud','ArtifactModal','UpgradeModal','ShopScreen','LeaderboardScreen','AchievementsScreen','SettingsScreen']],
  ['src/CharacterSelect.tsx',['CharacterSelect']],['src/KnowledgeBase.tsx',['KnowledgeBase']],['src/EchoMapScreen.tsx',['EchoMapScreen']],
  ['src/MobileControls.tsx',['MobileControls']],['src/TutorialOverlay.tsx',['TutorialOverlay']],
  ['src/i18n.ts',['translations']],
  ['src/engineProgression.ts',['getSphereEvolutionChoices','getAbilityEvolutionChoices']],['src/interfaceScale.ts',['loadInterfaceScale','saveInterfaceScale','formatInterfaceScale']],['src/region.ts',['getRegionPhase']]
]);
const withUi=!process.argv.includes('--canvas-only');if(withUi)sourceFiles.push(...uiRoots.keys());
const rendererRoots=['render','drawModernSphere','drawModernEnemy','drawPlayer','drawPlayerShield','drawCharacterVfx','drawCharacterWorldIndicators','drawCharacterTargetIndicators','drawSphereNetwork','drawFormationMemory','drawModernXp','drawModernHealth','drawModernMinion','drawStellaChest','drawRune','drawEnergyBolt','drawRegionLayer','drawVoidField','drawVoidAmbient'];
sourceFiles.push('src/canvasLocale.ts','src/renderScale.ts');
const nativeBindings=new Set(['engineRuntime.ts:getNetworkFrame','renderScale.ts:getRenderViewport','sphereProgression.ts:sphereModifiers','characterRuntime.ts:getCharacterFormation']);
nativeBindings.add('canvasLocale.ts:getLocale');
const uiActions=new Set(['activateByKey','activateDash','placeSphere','setSphereType','canActivateFormationFollow','setFormationFollow','repositionSphere']);
function isNative(file,name) {return nativeBindings.has(basename(file)+':'+name)||(withUi&&((basename(file)==='persistence.ts'&&/^(load|save|reset|unlock|add)/.test(name))||(basename(file)==='region.ts'&&/^(load|save)/.test(name))||(basename(file)==='runPersistence.ts')||(basename(file)==='audio.ts')||(basename(file)!=='engine.ts'&&(['applyUpgrade','applyArtifact','rerollUpgradeChoices','lockUpgradeChoice'].includes(name)||uiActions.has(name)))));}
function dependency(file,specifier) {if(!specifier.startsWith('.'))return specifier;const base=resolve(dirname(file),specifier);if(/\.tsx?$/.test(base))return base;return existsSync(base+'.ts')?base+'.ts':base+'.tsx';}
const target='UnityProject/Assets/EchoSphere/Scripts/Presentation/Generated';
const manifestPath=target+'/source-visual-manifest.json';
const hash=text=>createHash('sha256').update(text.replaceAll('\r\n','\n')).digest('hex');
const repositoryPath=file=>relative(process.cwd(),resolve(file)).replaceAll('\\','/');
if(process.argv.includes('--check')) {
  const manifest=JSON.parse(readFileSync(manifestPath,'utf8'));
  for(const [file,digest] of Object.entries(manifest.sources)) if(digest!==hash(readFileSync(file,'utf8')))throw new Error('Source visual changed: '+file);
  for(const [file,digest] of Object.entries(manifest.outputs))if(hash(readFileSync(file,'utf8'))!==digest)throw new Error('Generated visual changed: '+file);
  console.log(`Unity native visual code: ${Object.keys(manifest.sources).length} source modules, provenance PASS`);process.exit(0);
}
const require=createRequire(import.meta.url);
const ts=require(process.env.ECHO_TYPESCRIPT_PATH||'typescript');
const compilerOptions={target:ts.ScriptTarget.ESNext,module:ts.ModuleKind.ESNext,moduleResolution:ts.ModuleResolutionKind.Bundler,allowImportingTsExtensions:true,skipLibCheck:true,noEmit:true};
const host=ts.createCompilerHost(compilerOptions),readSource=host.readFile;
// Hoist only GameScreen's authored return tree; Unity owns the simulation loop.
// Discover captured locals through the checker, rather than maintaining a second
// hand-written HUD / overlay / game-over template.
const referenceProgram=ts.createProgram(sourceFiles,compilerOptions);
const sourceApp=referenceProgram.getSourceFile('src/App.tsx');
let gameViewSource='';
if(withUi){
  gameViewSource=require('./source-ui-view.cjs')(ts,referenceProgram);
  host.readFile=file=>{const source=readSource(file);return file.replaceAll('\\','/').endsWith('/src/App.tsx')||file==='src/App.tsx'?source+gameViewSource:source;};
}
const program=ts.createProgram(sourceFiles,compilerOptions,host);
const checker=program.getTypeChecker();
const units=new Map();
const dependencyParents=new Map();
const id=file=>'Visual_'+basename(file).replace(/\.tsx?$/,'').replace(/[^a-zA-Z0-9_]/g,'_');
const quote=JSON.stringify;
let serial=0;
function failure(node,message='unsupported syntax') { const pos=node.getSourceFile().getLineAndCharacterOfPosition(node.getStart());throw new Error(`${node.getSourceFile().fileName}:${pos.line+1}:${pos.character+1}: ${message}: ${node.getText().slice(0,160)}`); }
const literal=text=>`new V(${text})`;
const scope=(parent)=>({ parent,names:new Map() });
function lookup(name,s) { for(let p=s;p;p=p.parent)if(p.names.has(name))return p.names.get(name);return null; }
function name(node,s) { if(!ts.isIdentifier(node))failure(node,'binding');const result='v_'+node.text+'_'+serial++;s.names.set(node.text,result);return result; }
function bind(pattern,value,s) {
  if(ts.isIdentifier(pattern))return `V ${name(pattern,s)} = ${value};\n`;
  const temp='binding_'+serial++;let result=`V ${temp} = ${value};\n`;
  if(ts.isArrayBindingPattern(pattern))return result+pattern.elements.map((element,index)=>ts.isOmittedExpression(element)?'':bind(element.name,`${temp}[new V(${index}d)]`,s)).join('');
  if(ts.isObjectBindingPattern(pattern))return result+pattern.elements.map(element=>bind(element.name,`${temp}[new V(${quote((element.propertyName||element.name).text)})]`,s)).join('');
  failure(pattern,'binding pattern');
}
function optionalChain(n,s,continuation) {
  if(ts.isPropertyAccessExpression(n)||ts.isElementAccessExpression(n))return optionalChain(n.expression,s,value=> {
    const key=ts.isPropertyAccessExpression(n)?literal(quote(n.name.text)):expr(n.argumentExpression,s);
    if(!n.questionDotToken)return continuation(`${value}[${key}]`);
    const temporary='optional_'+serial++;return `VisualJs.Chain(${value},${temporary}=>${continuation(`${temporary}[${key}]`)})`;
  });
  if(ts.isCallExpression(n)&&n.flags&ts.NodeFlags.OptionalChain)return optionalChain(n.expression,s,value=> {
    const args=n.arguments.map(a=>expr(a,s)).join(', ');
    if(!n.questionDotToken)return continuation(`${value}.Call(${args})`);
    const temporary='optional_'+serial++;return `VisualJs.Chain(${value},${temporary}=>${continuation(`${temporary}.Call(${args})`)})`;
  });
  return continuation(expr(n,s));
}
function expr(n,s) {
  if(!n)return 'default(V)';
  if(ts.isJsxElement(n)||ts.isJsxSelfClosingElement(n)) {
    const opening=ts.isJsxElement(n)?n.openingElement:n;const tag=opening.tagName.getText();
    const attributes=opening.attributes.properties.map(p=>ts.isJsxSpreadAttribute(p)?expr(p.expression,s):`VisualJs.Obj(${literal(quote(p.name.getText()))},${p.initializer?ts.isJsxExpression(p.initializer)?expr(p.initializer.expression,s):expr(p.initializer,s):literal('true')})`);
    const props=`VisualJs.Merge(${attributes.join(', ')})`;const children=ts.isJsxElement(n)?n.children.map(c=>expr(c,s)):[];
    return /^[a-z]/.test(tag)?`VisualDom.Element(${quote(tag)},${props}${children.length?', '+children.join(', '):''})`:`VisualDom.Component(${quote(tag)},${expr(opening.tagName,s)},VisualDom.Props(${props}${children.length?', '+children.join(', '):''}))`;
  }
  if(ts.isJsxFragment(n))return `VisualDom.Element("fragment",VisualJs.Obj()${n.children.length?', '+n.children.map(c=>expr(c,s)).join(', '):''})`;
  if(ts.isJsxExpression(n))return expr(n.expression,s);
  if(ts.isJsxText(n)) {
    const lines=n.text.replaceAll('\r','').split('\n');const text=lines.map((line,i)=>line.replaceAll('\t',' ').replace(i===0?/$^/:/^ +/,'').replace(i===lines.length-1?/$^/:/ +$/,'')).filter(Boolean).join(' ');
    return literal(quote(text));
  }
  if(n.flags&ts.NodeFlags.OptionalChain)return optionalChain(n,s,value=>value);
  if(ts.isParenthesizedExpression(n)||ts.isAsExpression(n)||ts.isNonNullExpression(n)||ts.isTypeAssertionExpression(n)||ts.isSatisfiesExpression(n))return `(${expr(n.expression,s)})`;
  if(ts.isNumericLiteral(n))return literal(Number(n.text)+'d');
  if(ts.isRegularExpressionLiteral(n))return `VisualJs.Regex(${quote(n.text)})`;
  if(ts.isStringLiteral(n)||ts.isNoSubstitutionTemplateLiteral(n))return literal(quote(n.text));
  if(n.kind===ts.SyntaxKind.TrueKeyword)return literal('true');if(n.kind===ts.SyntaxKind.FalseKeyword)return literal('false');if(n.kind===ts.SyntaxKind.NullKeyword)return 'VisualJs.Null';
  if(ts.isIdentifier(n)) {
    const local=lookup(n.text,s);if(local)return local;
    if(n.text==='undefined')return 'default(V)';if(n.text==='Math')return 'VisualJs.MathObject';if(n.text==='Number')return 'VisualJs.NumberObject';if(n.text==='Object')return 'VisualJs.ObjectObject';if(n.text==='Array')return 'VisualJs.ArrayObject';
    if(n.text==='Boolean')return 'VisualJs.Fn(a=>new V(VisualJs.Arg(a,0).Truth))';if(n.text==='String')return 'VisualJs.Fn(a=>new V(VisualJs.Arg(a,0).Text))';
    if(n.text==='performance')return 'VisualJs.PerformanceObject';if(n.text==='console')return 'VisualJs.ConsoleObject';
    if(withUi&&['setTimeout','clearTimeout','setInterval','clearInterval','requestAnimationFrame','cancelAnimationFrame','window','document','localStorage','Date','confirm','navigator','location'].includes(n.text))return `VisualUiBindings.Get("global",${quote(n.text)})`;
    if(n.text==='NaN')return literal('double.NaN');if(n.text==='Infinity')return literal('double.PositiveInfinity');
    if(n.text==='parseInt')return 'VisualJs.NumberObject[new V("parseInt")]';
    failure(n,'unbound visual identifier');
  }
  if(ts.isPropertyAccessExpression(n))return n.questionDotToken?`VisualJs.Optional(${expr(n.expression,s)},${literal(quote(n.name.text))})`:`${expr(n.expression,s)}[${literal(quote(n.name.text))}]`;
  if(ts.isElementAccessExpression(n))return n.questionDotToken?`VisualJs.Optional(${expr(n.expression,s)},${expr(n.argumentExpression,s)})`:`${expr(n.expression,s)}[${expr(n.argumentExpression,s)}]`;
  if(ts.isCallExpression(n)) {
    if(ts.isIdentifier(n.expression)&&n.expression.text==='Number')return `VisualJs.Number(${expr(n.arguments[0],s)})`;
    if(ts.isIdentifier(n.expression)&&n.expression.text==='Boolean')return `new V(${expr(n.arguments[0],s)}.Truth)`;
    if(ts.isIdentifier(n.expression)&&n.expression.text==='String')return `new V(${expr(n.arguments[0],s)}.Text)`;
    if(n.arguments.some(ts.isSpreadElement))return `${expr(n.expression,s)}.Call(VisualJs.Concat(${n.arguments.map(a=>ts.isSpreadElement(a)?expr(a.expression,s):`VisualJs.Arr(${expr(a,s)})`).join(', ')}).Items.ToArray())`;
    return `${expr(n.expression,s)}.Call(${n.arguments.map(a=>expr(a,s)).join(', ')})`;
  }
  if(ts.isNewExpression(n)) {
    if(['WeakMap','Map'].includes(n.expression.getText()))return `VisualJs.CreateMap(${n.arguments?.length?expr(n.arguments[0],s):'default(V)'},${n.expression.getText()==='WeakMap'})`;
    if(n.expression.getText()==='Image')return 'VisualJs.CreateImage()';
    if(n.expression.getText()==='Set')return `VisualJs.NewSet(${n.arguments?.length?expr(n.arguments[0],s):'default(V)'})`;
    if(withUi&&n.expression.getText()==='Date')return `VisualUiBindings.Get("global","Date").Call(${(n.arguments||[]).map(a=>expr(a,s)).join(',')})`;
    failure(n,'constructor');
  }
  if(ts.isArrayLiteralExpression(n))return n.elements.some(ts.isSpreadElement)?`VisualJs.Concat(${n.elements.map(a=>ts.isSpreadElement(a)?expr(a.expression,s):`VisualJs.Arr(${expr(a,s)})`).join(', ')})`:`VisualJs.Arr(${n.elements.map(x=>expr(x,s)).join(', ')})`;
  if(ts.isObjectLiteralExpression(n)&&n.properties.some(ts.isSpreadAssignment))return `VisualJs.Merge(${n.properties.map(p=>ts.isSpreadAssignment(p)?expr(p.expression,s):`VisualJs.Obj(${ts.isComputedPropertyName(p.name)?expr(p.name.expression,s):literal(quote(p.name.text))},${expr(ts.isShorthandPropertyAssignment(p)?p.name:p.initializer,s)})`).join(', ')})`;
  if(ts.isObjectLiteralExpression(n))return `VisualJs.Obj(${n.properties.flatMap(p=>{
    if(ts.isShorthandPropertyAssignment(p))return [literal(quote(p.name.text)),expr(p.name,s)];
    if(ts.isPropertyAssignment(p))return [ts.isComputedPropertyName(p.name)?expr(p.name.expression,s):literal(quote(p.name.text)),expr(p.initializer,s)];failure(p,'object member');
  }).join(', ')})`;
  if(ts.isConditionalExpression(n))return `(${expr(n.condition,s)}.Truth ? ${expr(n.whenTrue,s)} : ${expr(n.whenFalse,s)})`;
  if(ts.isTemplateExpression(n))return '('+literal(quote(n.head.text))+n.templateSpans.map(span=>' + '+expr(span.expression,s)+' + '+literal(quote(span.literal.text))).join('')+')';
  if(ts.isBinaryExpression(n)) {
    if(n.operatorToken.kind===ts.SyntaxKind.InstanceOfKeyword&&n.right.getText()==='Element')return `VisualUiBindings.Get("global","isElement").Call(${expr(n.left,s)})`;
    const op=n.operatorToken.getText(),a=expr(n.left,s),b=expr(n.right,s);
    if(op==='||')return `VisualJs.Or(${a}, () => ${b})`;if(op==='&&')return `VisualJs.And(${a}, () => ${b})`;if(op==='??')return `VisualJs.Coalesce(${a}, () => ${b})`;
    if(['||=','&&=','??='].includes(op))return `${a} = VisualJs.${op==='||='?'Or':op==='&&='?'And':'Coalesce'}(${a}, () => ${b})`;
    if(op==='**')return `new V(System.Math.Pow(${a}.Number,${b}.Number))`;
    if(op==='in')return `VisualJs.Has(${b},${a})`;
    if(['===','==','!==','!=','<','>','<=','>='].includes(op))return `new V(${a} ${op.replace('===','==').replace('!==','!=')} ${b})`;
    if(op==='>>'||op==='<<')return `(${a} ${op} (int)${b}.Number)`;
    if(['+','-','*','/','%','&','|','^'].includes(op))return `(${a} ${op} ${b})`;
    if(['=','+=','-=','*=','/='].includes(op)) {
      if(ts.isPropertyAccessExpression(n.left))return `VisualJs.Assign(${expr(n.left.expression,s)},${literal(quote(n.left.name.text))},${quote(op)},${b})`;
      if(ts.isElementAccessExpression(n.left))return `VisualJs.Assign(${expr(n.left.expression,s)},${expr(n.left.argumentExpression,s)},${quote(op)},${b})`;
      if(ts.isIdentifier(n.left)&&a.startsWith('Scope['))return `VisualJs.Assign(Scope,${literal(quote(n.left.text))},${quote(op)},${b})`;
      return `${a} ${op} ${b}`;
    }
    failure(n,'operator '+op);
  }
  if(ts.isPrefixUnaryExpression(n)) {const op=ts.tokenToString(n.operator);return `(${op}${expr(n.operand,s)})`;}
  if(ts.isPostfixUnaryExpression(n))return `${expr(n.operand,s)}${ts.tokenToString(n.operator)}`;
  if(ts.isArrowFunction(n)||ts.isFunctionExpression(n)) {
    const inner=scope(s),args='args_'+serial++;let setup='';
    n.parameters.forEach((p,i)=>{setup+=bind(p.name,p.initializer?`VisualJs.Arg(${args},${i}).Undefined?${expr(p.initializer,inner)}:VisualJs.Arg(${args},${i})`:`VisualJs.Arg(${args}, ${i})`,inner);});
    const body=ts.isBlock(n.body)?functionBody(n.body,inner):`return ${expr(n.body,inner)};`;
    return `VisualJs.Fn(${args} => { ${setup}${body} })`;
  }
  if(ts.isTypeOfExpression(n))return `VisualJs.TypeOf(${expr(n.expression,s)})`;
  failure(n);
}
function declarations(list,s) { return list.declarations.map(d=>{
  if(ts.isObjectBindingPattern(d.name)||ts.isArrayBindingPattern(d.name))return bind(d.name,expr(d.initializer,s),s);
  const variable=name(d.name,s);return d.initializer&&(ts.isArrowFunction(d.initializer)||ts.isFunctionExpression(d.initializer))?`V ${variable} = default; ${variable} = ${expr(d.initializer,s)};`:`V ${variable} = ${expr(d.initializer,s)};`;
}).join('\n'); }
// A fallback return is required only when the authored function can reach its
// end. Appending one after an unconditional source return creates CS0162 in Unity.
function endsFunction(node) {
  if(ts.isReturnStatement(node)||ts.isThrowStatement(node))return true;
  if(ts.isBlock(node))return node.statements.some(endsFunction);
  if(ts.isIfStatement(node))return !!node.elseStatement&&endsFunction(node.thenStatement)&&endsFunction(node.elseStatement);
  if(ts.isSwitchStatement(node))return node.caseBlock.clauses.some(ts.isDefaultClause)&&node.caseBlock.clauses.every(clause=>clause.statements.some(endsFunction));
  return false;
}
function functionBody(block,s) {return block.statements.map(node=>stmt(node,s)).join('\n')+(endsFunction(block)?'':'\nreturn default;');}
function stmt(n,s) {
  if(ts.isFunctionDeclaration(n)&&n.name?.text==='drawRune')return ''; // Hoisted, unchanged source function.
  if(ts.isBlock(n)) {const inner=scope(s);return '{\n'+n.statements.map(x=>stmt(x,inner)).join('\n')+'\n}';}
  if(ts.isVariableStatement(n))return declarations(n.declarationList,s);
  if(ts.isExpressionStatement(n))return expr(n.expression,s)+';';
  if(ts.isIfStatement(n))return `if (${expr(n.expression,s)}.Truth) ${stmt(n.thenStatement,s)}${n.elseStatement?' else '+stmt(n.elseStatement,s):''}`;
  if(ts.isReturnStatement(n))return `return ${n.expression?expr(n.expression,s):'default'};`;
  if(ts.isWhileStatement(n))return `while (${expr(n.expression,s)}.Truth) ${stmt(n.statement,s)}`;
  if(ts.isDoStatement(n))return `do ${stmt(n.statement,s)} while (${expr(n.expression,s)}.Truth);`;
  if(ts.isForStatement(n)) {const inner=scope(s);const init=n.initializer?ts.isVariableDeclarationList(n.initializer)?declarations(n.initializer,inner).replace(/;$/,''):expr(n.initializer,inner):'';return `for (${init}; ${n.condition?expr(n.condition,inner)+'.Truth':'true'}; ${n.incrementor?expr(n.incrementor,inner):''}) ${stmt(n.statement,inner)}`;}
  if(ts.isForOfStatement(n)) {
    const inner=scope(s),binding=n.initializer.declarations[0].name;
    if(ts.isArrayBindingPattern(binding)) {const variable='item_'+serial++;const setup=binding.elements.map((e,i)=>`V ${name(e.name,inner)} = ${variable}[new V(${i}d)];`).join('\n');return `foreach (V ${variable} in ${expr(n.expression,s)}.Items) {\n${setup}\n${stmt(n.statement,inner)}\n}`;}
    const variable=name(binding,inner);return `foreach (V ${variable} in ${expr(n.expression,s)}.Items) ${stmt(n.statement,inner)}`;
  }
  if(ts.isLabeledStatement(n))return `${stmt(n.statement,s)}\nlabel_${n.label.text}: ;`;
  if(ts.isContinueStatement(n)){if(n.label)failure(n,'labelled continue');return 'continue;';}if(ts.isBreakStatement(n))return n.label?`goto label_${n.label.text};`:'break;';
  if(ts.isSwitchStatement(n))return `switch (${expr(n.expression,s)}.Text) {\n`+n.caseBlock.clauses.map(c=>(ts.isDefaultClause(c)?'default:':'case '+quote(c.expression.text)+':')+'\n'+c.statements.map(x=>stmt(x,s)).join('\n')).join('\n')+'\n}';
  if(ts.isEmptyStatement(n)||ts.isTypeAliasDeclaration(n)||ts.isInterfaceDeclaration(n))return '';
  failure(n,'statement');
}
function load(file) {
  const path=resolve(file);if(units.has(path))return units.get(path);
  const tree=program.getSourceFile(file)||program.getSourceFile(path)||ts.createSourceFile(file,readFileSync(file,'utf8'),ts.ScriptTarget.Latest,true);
  const definitions=new Map(),imports=new Map();
  for(const n of tree.statements) {
    if(ts.isFunctionDeclaration(n))definitions.set(n.name.text,n);
    if(ts.isVariableStatement(n))for(const d of n.declarationList.declarations)if(ts.isIdentifier(d.name))definitions.set(d.name.text,d);
    if(ts.isImportDeclaration(n)&&n.importClause&&!n.importClause.isTypeOnly) {
      for(const binding of n.importClause.namedBindings?.elements||[])if(!binding.isTypeOnly)imports.set(binding.name.text,{file:dependency(file,n.moduleSpecifier.text),name:(binding.propertyName||binding.name).text});
      if(n.importClause.name)imports.set(n.importClause.name.text,{file:dependency(file,n.moduleSpecifier.text),name:n.importClause.name.text});
    }
    if(ts.isExportDeclaration(n)&&!n.isTypeOnly&&n.moduleSpecifier&&n.exportClause)for(const binding of n.exportClause.elements||[])if(!binding.isTypeOnly)imports.set(binding.name.text,{file:dependency(file,n.moduleSpecifier.text),name:(binding.propertyName||binding.name).text});
  }
  if(file.endsWith('renderer.ts')) {function nested(n){if(ts.isFunctionDeclaration(n)&&n.name?.text==='drawRune')definitions.set('drawRune',n);else ts.forEachChild(n,nested);}ts.forEachChild(tree,nested);}
  const unit={file,tree,id:id(file),definitions,imports,wanted:new Set()};units.set(path,unit);return unit;
}
function include(unit,name) {
  if(unit.wanted.has(name))return;unit.wanted.add(name);
  if(isNative(unit.file,name))return;
  if(unit.file==='react'||unit.file==='lucide-react')return;
  if(unit.imports.has(name)) {const imported=unit.imports.get(name);if(imported.file==='react'||imported.file==='lucide-react')return;dependencyParents.set(imported.file+':'+imported.name,resolve(unit.file)+':'+name);include(load(imported.file),imported.name);return;}
  if(process.env.ECHO_VISUAL_DEBUG&&unit.file.endsWith('rng.ts')&&name==='nextRandom'){let key=resolve(unit.file)+':'+name;const path=[];while(key&&!path.includes(key)){path.push(key);key=dependencyParents.get(key);}console.log(path.join('\n  <- '));}
  const node=unit.definitions.get(name);if(!node)throw new Error('Missing visual dependency '+unit.file+': '+name);
  function walk(n) {
    if(ts.isTypeNode(n))return;
    if(ts.isIdentifier(n)&&!(ts.isPropertyAccessExpression(n.parent)&&n.parent.name===n)&&!(ts.isPropertyAssignment(n.parent)&&n.parent.name===n)) {
      const symbol=checker.getSymbolAtLocation(n);
      const topLevel=symbol?.declarations?.some(d=>d===unit.definitions.get(n.text)||ts.isImportSpecifier(d)||ts.isExportSpecifier(d));
      if(topLevel&&n.text!==name&&(unit.definitions.has(n.text)||unit.imports.has(n.text))) {dependencyParents.set(resolve(unit.file)+':'+n.text,resolve(unit.file)+':'+name);include(unit,n.text);}
    }
    ts.forEachChild(n,walk);
  }
  ts.forEachChild(node,walk);
}
for(const file of sourceFiles) {const unit=load(file);for(const name of file==='src/renderer.ts'?rendererRoots:file==='src/canvasLocale.ts'?['localizeCanvasText']:file==='src/renderScale.ts'?['clampDpr','calculateBackingSize']:uiRoots.get(file)||unit.definitions.keys())include(unit,name);}
// Preserve source initialization passes (e.g. authored ability final variants).
// Include a pass only when it reads or writes a retained top-level declaration.
let expanded=true;
while(expanded) {
  expanded=false;
  for(const unit of units.values()) {
    unit.effects??=new Set();
    for(const statement of unit.tree.statements) {
      if(ts.isImportDeclaration(statement)||ts.isExportDeclaration(statement)||ts.isExportAssignment(statement)||ts.isFunctionDeclaration(statement)||ts.isVariableStatement(statement)||ts.isInterfaceDeclaration(statement)||ts.isTypeAliasDeclaration(statement)||unit.effects.has(statement))continue;
      const referenced=new Set();function walk(n){if(ts.isTypeNode(n))return;if(ts.isIdentifier(n)){const symbol=checker.getSymbolAtLocation(n);if(symbol?.declarations?.some(d=>d===unit.definitions.get(n.text)||ts.isImportSpecifier(d)))referenced.add(n.text);}ts.forEachChild(n,walk);}walk(statement);
      if(![...referenced].some(name=>unit.wanted.has(name)))continue;
      unit.effects.add(statement);expanded=true;for(const name of referenced)if(unit.definitions.has(name)||unit.imports.has(name))include(unit,name);
    }
  }
}
function importTarget(file,name) {
  if(file==='react')return `VisualDom.React(${quote(name)})`;
  if(file==='lucide-react')return `VisualUiBindings.Icon(${quote(name)})`;
  if(isNative(file,name))return nativeBindings.has(basename(file)+':'+name)?`VisualBindings.Get(new V(${quote(basename(file))}),new V(${quote(name)}))`:`VisualUiBindings.Get(${quote(basename(file))},${quote(name)})`;
  const unit=units.get(resolve(file));if(!unit)return null;
  if(unit.imports.has(name)&&!unit.definitions.has(name)){const next=unit.imports.get(name);return importTarget(next.file,next.name);}
  return `${unit.id}.Scope[new V(${quote(name)})]`;
}
const outputs={};
for(const unit of units.values()) {
  const s=scope(null),initializers=[],methods=[];serial=0;
  for(const name of unit.definitions.keys())s.names.set(name,isNative(unit.file,name)?importTarget(unit.file,name):`Scope[new V(${quote(name)})]`);
  for(const n of unit.tree.statements) {
    if(ts.isFunctionDeclaration(n)||ts.isVariableStatement(n)) {
      const names=ts.isFunctionDeclaration(n)?[n.name]:n.declarationList.declarations.map(d=>d.name);
      for(const binding of names) {if(!ts.isIdentifier(binding))failure(binding,'top-level binding');s.names.set(binding.text,isNative(unit.file,binding.text)?importTarget(unit.file,binding.text):`Scope[new V(${quote(binding.text)})]`);}
    }
    if(ts.isImportDeclaration(n)&&n.importClause&&!n.importClause.isTypeOnly) {
      const path=dependency(unit.file,n.moduleSpecifier.text);
      for(const b of n.importClause.namedBindings?.elements||[]) {
        if(b.isTypeOnly)continue;
        const target=importTarget(path,(b.propertyName||b.name).text)||`VisualBindings.Get(new V(${quote(n.moduleSpecifier.text)}),new V(${quote((b.propertyName||b.name).text)}))`;
        s.names.set(b.name.text,target);
      }
    }
  }
  for(const [binding,target] of unit.imports)if(!s.names.has(binding))s.names.set(binding,importTarget(target.file,target.name));
  for(const n of unit.definitions.values()) {
    if(ts.isFunctionDeclaration(n)&&unit.wanted.has(n.name.text)&&!isNative(unit.file,n.name.text)) {
      const inner=scope(s),fn='F_'+n.name.text;let body='';
      n.parameters.forEach((p,i)=>{body+=bind(p.name,p.initializer?`VisualJs.Arg(args,${i}).Undefined?${expr(p.initializer,inner)}:VisualJs.Arg(args,${i})`:`VisualJs.Arg(args,${i})`,inner);});
      body+=functionBody(n.body,inner);
      methods.push(`private static V ${fn}(V[] args) {\n${body}\n}`);
      initializers.push(`${lookup(n.name.text,s)} = VisualJs.Fn(${fn});`);
    }
  }
  for(const n of unit.tree.statements) {
    if(ts.isVariableStatement(n))for(const d of n.declarationList.declarations)if(unit.wanted.has(d.name.text)&&!isNative(unit.file,d.name.text))initializers.push(`${lookup(d.name.text,s)} = ${expr(d.initializer,s)};`);
    if(unit.effects?.has(n))initializers.push(stmt(n,s));
  }
  const output=target+'/'+unit.id+'.cs';
  const code=`// Generated from ${repositoryPath(unit.file)}; sha256 ${hash(readFileSync(unit.file,'utf8'))}.\n// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.\nusing System;\nusing System.Linq;\nnamespace EchoSphere.Presentation {\ninternal static class ${unit.id} {\npublic static readonly V Scope=VisualJs.Obj();\nstatic ${unit.id}() {\n${initializers.join('\n')}\n}\n${methods.join('\n')}\n}\n}\n`;
  mkdirSync(dirname(output),{recursive:true});writeFileSync(output,code);outputs[output]=hash(code);
}
 writeFileSync(manifestPath,JSON.stringify({sources:Object.fromEntries([...new Set([...units.values()].map(({file})=>repositoryPath(file)).concat(['scripts/export-unity-visual-code.mjs','scripts/source-ui-view.cjs','package-lock.json']))].map(file=>[file,hash(readFileSync(file,'utf8'))])),outputs},null,2)+'\n');
console.log(`Translated ${units.size} source visual modules directly to native C#.`);
