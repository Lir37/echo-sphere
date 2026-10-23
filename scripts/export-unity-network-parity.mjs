import {analyzeSphereNetwork} from '../src/network.ts';
import {writeFileSync} from 'node:fs';
const scale=Math.fround(12.6/720);
const cases=[
 ['line',[[-100,0],[0,0],[100,0]]],
 ['square100',[[0,0],[100,0],[100,100],[0,100]]],
 ['square50',[[0,0],[50,0],[50,50],[0,50]]],
 ['triangle60',[[0,0],[60,0],[30,60*Math.sqrt(3)/2]]],
 ['triangle70',[[0,0],[70,0],[35,70*Math.sqrt(3)/2]]],
 ['five_node_regression',[...Array.from({length:4},(_,i)=>[Math.cos(i*1.17)*140,Math.sin(i*1.17)*140]),[43,-28]]],
 ['lattice4',[[0,0],[100,0],[50,86.60254],[150,86.60254]]],
 ['composite6',[[0,0],[100,0],[50,86.60254],[150,86.60254],[200,0],[250,86.60254]]],
 ['sparse_line',Array.from({length:6},(_,i)=>[i*250,0])],
 ['dense40',[[0,0],[40,0],[40,40],[0,40]]],
];
const fixtures=cases.map(([name,points])=>{
 const nodes=points.map(([x,y])=>({worldX:Math.fround(Math.fround(x)*scale),worldY:Math.fround(Math.fround(y)*scale)}));
 const state=analyzeSphereNetwork(nodes.map(n=>({pos:{x:n.worldX/scale,y:n.worldY/scale},alive:true})),220);
 return {name,nodes,links:state.links.map(({a,b})=>({a,b})),dominant:state.dominantFormation?.type??'none',secondary:state.secondaryFormation?.type??'none',candidates:state.formationCandidates.map(s=>({type:s.type,strength:s.strength,nodes:s.nodes}))};
});
writeFileSync('tools/EchoSphere.CoreChecks/source-network-parity-fixtures.json',JSON.stringify({source:'src/network.ts@737ef2af4a877e0995db5f92331d536d2f29a934',fixtures},null,2)+'\n');
console.log(`Original network fixtures: ${fixtures.length}`);
