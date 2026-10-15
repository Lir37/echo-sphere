import {existsSync,readdirSync,readFileSync,writeFileSync,statSync} from 'node:fs';
import {createHash} from 'node:crypto';
import {extname} from 'node:path';
const roots=['UnityProject/Assets/EchoSphere/Scripts/Presentation','UnityProject/Assets/Resources/EchoSphere/CanvasArt','UnityProject/Assets/Resources/EchoSphere/SourceUiControls'];
const singles=['UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourcePresentation.cs','UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs',...readdirSync('UnityProject/Assets/Resources/EchoSphere').filter(file=>/^(?:SourceCanvas|SourceUi)/.test(file)&&!file.endsWith('.meta')&&statSync('UnityProject/Assets/Resources/EchoSphere/'+file).isFile()).map(file=>'UnityProject/Assets/Resources/EchoSphere/'+file)];
const check=process.argv.includes('--check');let count=0;
function meta(file,folder=false){
 const guid=createHash('sha256').update('echo-sphere/source-presentation/'+file).digest('hex').slice(0,32);const target=file+'.meta';
 if(existsSync(target))return;const extension=extname(file).toLowerCase();let importer;
 if(folder)importer='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n';
 else if(extension==='.cs')importer='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n';
 else if(extension==='.shader')importer='ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n';
 else if(extension==='.tss')importer='ScriptedImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 2\n  script: {fileID: 12388, guid: 0000000000000000e000000000000000, type: 0}\n  disableValidation: 0\n  unsupportedSelectorAction: 0\n';
 else if(extension==='.png'){
  const sprite=file.includes('/CanvasArt/');importer=`TextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 13\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n    sRGBTexture: 1\n  isReadable: 0\n  streamingMipmaps: 0\n  textureFormat: 1\n  maxTextureSize: 4096\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  alphaUsage: 1\n  alphaIsTransparency: 1\n  spriteMode: ${sprite?1:0}\n  spriteExtrude: 1\n  spriteMeshType: 0\n  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}\n  spritePixelsToUnits: 100\n  textureType: ${sprite?8:0}\n  textureShape: 1\n  platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 4096\n    resizeAlgorithm: 0\n    textureFormat: -1\n    textureCompression: 0\n    compressionQuality: 100\n    crunchedCompression: 0\n    allowsAlphaSplitting: 0\n    overridden: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: ${createHash('sha256').update(guid+'sprite').digest('hex').slice(0,32)}\n    internalID: 0\n    vertices: []\n    indices:\n    edges: []\n    weights: []\n    secondaryTextures: []\n    nameFileIdTable: {}\n`;
 }else importer='TextScriptImporter:\n  externalObjects: {}\n';
 if(check)throw Error('Presentation asset has no stable Unity metadata: '+file);
 writeFileSync(target,'fileFormatVersion: 2\nguid: '+guid+'\n'+importer+'  userData:\n  assetBundleName:\n  assetBundleVariant:\n');count++;
}
function walk(directory){meta(directory,true);for(const entry of readdirSync(directory,{withFileTypes:true})){const file=directory+'/'+entry.name;if(entry.isDirectory())walk(file);else if(!file.endsWith('.meta'))meta(file);}}
roots.forEach(walk);singles.forEach(file=>meta(file));
console.log(check?'Presentation asset metadata: PASS':`Presentation metadata: ${count} stable asset / folder GUIDs added; existing GUIDs retained.`);
