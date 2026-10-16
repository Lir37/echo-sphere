import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiTypography.cs','utf8');
test('cold native font initialization has a profiler marker after the retained cache fast path',()=>{
 assert.match(s,/FontResolveMarker=new ProfilerMarker\("EchoSphere\.UI\.FontResolve"\)/);
 const lookup=s.indexOf('if(Fonts.TryGetValue(key,out var existing))return existing;');
 const marker=s.indexOf('using var fontResolveSample=FontResolveMarker.Auto();');
 const create=s.indexOf('FontAsset.CreateFontAsset(');
 assert.ok(lookup>=0&&marker>lookup&&create>marker);
});
