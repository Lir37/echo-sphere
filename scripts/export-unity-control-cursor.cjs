const fs=require('node:fs'),crypto=require('node:crypto'),{spawnSync}=require('node:child_process');
const {encodePng}=require('./source-css-browser.cjs');
const directory='UnityProject/Assets/Resources/EchoSphere/SourceUiControls',manifestPath=directory+'/cursor-reference.json',file='cursor-pointer.png';
const hash=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
const sourceFiles=['src/conceptStyle.css','src/index.css','tools/EchoSphere.NativeControlArt/Program.cs','tools/EchoSphere.NativeControlArt/EchoSphere.NativeControlArt.csproj'];
const sources=()=>Object.fromEntries(sourceFiles.map(file=>[file,hash(fs.readFileSync(file,'utf8').replaceAll('\r\n','\n'))]));
if(process.argv.includes('--check')){
 const manifest=JSON.parse(fs.readFileSync(manifestPath));
 if(JSON.stringify(manifest.sources)!==JSON.stringify(sources()))throw Error('System cursor reference inputs changed');
 if(hash(fs.readFileSync(directory+'/'+file))!==manifest.sha256)throw Error('System cursor reference pixels changed');
 console.log('Source CSS platform hand cursor provenance: PASS');
}else{
 const dotnet=process.env.ECHO_DOTNET_PATH||'dotnet';
 const capture=spawnSync(dotnet,['run','--project','tools/EchoSphere.NativeControlArt','--configuration','Release','--verbosity','quiet'],{encoding:'utf8',windowsHide:true});
 if(capture.status!==0)throw Error(capture.stderr||capture.stdout||'System cursor capture failed');
 const raster=JSON.parse(capture.stdout.trim().split(/\r?\n/).at(-1));
 const bytes=encodePng(raster.width,raster.height,Buffer.from(raster.pixels,'base64'));
 fs.writeFileSync(directory+'/'+file,bytes);
 fs.writeFileSync(manifestPath,JSON.stringify({reference:'Windows browser CSS cursor:pointer / user32 IDC_HAND (32649)',file,width:raster.width,height:raster.height,hotspotX:raster.hotspotX,hotspotY:raster.hotspotY,sha256:hash(bytes),sources:sources()},null,2)+'\n');
 console.log(`Source CSS platform hand cursor: ${raster.width}×${raster.height}, hotspot ${raster.hotspotX},${raster.hotspotY}.`);
}
