// The battlefield loop remains in Unity. Hoist the unmodified authored return
// expression into a presentation-only function with its captured locals as props.
module.exports=function sourceGameView(ts,program) {
  const source=program.getSourceFile('src/App.tsx');const checker=program.getTypeChecker();
  const game=source.statements.find(node=>ts.isFunctionDeclaration(node)&&node.name?.text==='GameScreen');
  const returned=game.body.statements.find(ts.isReturnStatement);const captures=new Set();
  function within(node,ancestor){for(let current=node;current;current=current.parent)if(current===ancestor)return true;return false;}
  function collect(node){
    if(ts.isTypeNode(node))return;
    if(ts.isIdentifier(node)&&!(ts.isPropertyAccessExpression(node.parent)&&node.parent.name===node)&&!(ts.isPropertyAssignment(node.parent)&&node.parent.name===node)){
      const declarations=checker.getSymbolAtLocation(node)?.declarations||[];
      if(declarations.some(declaration=>within(declaration,game)&&!within(declaration,returned)))captures.add(node.text);
    }
    ts.forEachChild(node,collect);
  }
  collect(returned);
  const interfaceScale=require('node:fs').readFileSync('src/interfaceScale.ts','utf8');
  const markup=interfaceScale.match(/section\.innerHTML\s*=\s*`([\s\S]*?)`;/)?.[1];if(!markup)throw new Error('Cannot find authored interface-scale settings markup');
  return `\nfunction SourceGameView({${[...captures].join(',')}}: any) {\n${returned.getText(source)}\n}\nfunction SourceInterfaceScaleMarkup(){return (<div data-interface-scale-settings="true">${markup.replace(/\bclass=/g,'className=')}</div>);}\n`;
};
