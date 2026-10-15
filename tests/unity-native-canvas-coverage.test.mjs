import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync,readdirSync} from 'node:fs';

const files=directory=>readdirSync(directory,{withFileTypes:true}).flatMap(entry=>entry.isDirectory()?files(directory+'/'+entry.name):[directory+'/'+entry.name]);
test('native Canvas implements every ctx command and property used by new-desing',()=>{
 const source=files('src').filter(file=>/\.tsx?$/.test(file)&&!file.endsWith('/audio.ts')).map(file=>readFileSync(file,'utf8')).join('\n');
 const calls=new Set([...source.matchAll(/\bctx\.([A-Za-z0-9_]+)\s*\(/g)].map(match=>match[1]));
 const writes=new Set([...source.matchAll(/\bctx\.([A-Za-z0-9_]+)\s*=(?!=)/g)].map(match=>match[1]));
 const native=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
 const implemented=new Set([...native.matchAll(/case\s+"([^"]+)"\s*:/g)].map(match=>match[1]));
 assert.ok(calls.size>20&&writes.size>10,'source Canvas coverage unexpectedly disappeared');
 for(const name of [...calls,...writes])assert.ok(implemented.has(name),'missing native Canvas API: ctx.'+name);
});
