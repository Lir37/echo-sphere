// Decode existing frozen references for Editor diagnostics; never launch a browser.
import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
const root=process.cwd(),directory=path.join(root,'UnityProject/Temp/native-glass-goldens');
const fixtures=JSON.parse(zlib.gunzipSync(fs.readFileSync(path.join(root,'tools/EchoSphere.VisualChecks/source-backdrop-fixtures.json.gz'))));
fs.mkdirSync(directory,{recursive:true});
const metadata=fixtures.map(({source,expected,...fixture},index)=>{
 fs.writeFileSync(path.join(directory,`${index}-source.rgba`),Buffer.from(source));
 fs.writeFileSync(path.join(directory,`${index}-expected.rgba`),Buffer.from(expected));
 return fixture;
});
fs.writeFileSync(path.join(directory,'metadata.json'),JSON.stringify(metadata));
console.log(`Decoded ${metadata.length} existing GPU diagnostic fixtures`);
