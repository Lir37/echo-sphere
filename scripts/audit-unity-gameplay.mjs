import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const base=path.join(root,'UnityProject/Assets/EchoSphere/Scripts');
async function files(dir){const result=[];for(const entry of await fs.readdir(dir,{withFileTypes:true})){const name=path.join(dir,entry.name);if(entry.isDirectory())result.push(...await files(name));else if(entry.name.endsWith('.cs'))result.push(name);}return result;}
const rulesPath=path.join(base,'Core/ArtifactRewardRules.cs');const rules=await fs.readFile(rulesPath,'utf8');
const runtime=await Promise.all((await files(path.join(base,'Runtime'))).map(async file=>({file:path.relative(root,file).replaceAll('\\','/'),lines:(await fs.readFile(file,'utf8')).split(/\r?\n/)})));
const adapted=await fs.readFile(path.join(base,'Core/ArtifactGameplayRules.cs'),'utf8');
const correctedIds=[...adapted.match(/CorrectedIds = \{([^}]+)\}/)[1].matchAll(/"([^"]+)"/g)].map(m=>m[1]);
const definitions=rules.split(/\r?\n/).filter(line=>line.includes('new ArtifactRewardDefinition('));
const artifacts=definitions.map(line=>{
 const values=[...line.matchAll(/"((?:[^"\\]|\\.)*)"/g)].map(m=>m[1]);const [id,nameEn,nameRu,descriptionEn,descriptionRu]=values;
 const effects=[...line.matchAll(/\{\s*"([^"]+)"\s*,\s*([-.\d]+)f?\s*\}/g)].map(m=>({key:m[1],legacyValue:Number(m[2])}));
 const literalReferences=[];for(const source of runtime)source.lines.forEach((text,index)=>{if(text.includes('"'+id+'"'))literalReferences.push({file:source.file,line:index+1,text:text.trim()});});
 const corrected=correctedIds.includes(id);
 return {id,nameRu,nameEn,descriptionRu,descriptionEn,legacyEffects:effects,literalRuntimeReferences:literalReferences,status:corrected?'CORRECTED_NEW_RUNS_AND_TESTED':'REQUIRES_BEHAVIORAL_REVIEW',verification:corrected?'ArtifactGameplayChecks + check-unity-artifact-contracts.cs':null};
});
if(artifacts.length!==82||new Set(artifacts.map(x=>x.id)).size!==82)throw new Error('Unexpected artifact catalog: review parser and baseline');
const catalog=await fs.readFile(path.join(root,'UnityProject/Assets/Resources/EchoSphere/NativeRunCatalog.asset'),'utf8');
const counts={};let section='';for(const line of catalog.split(/\r?\n/)){const key=line.match(/^  ([A-Za-z]+):/);if(key){section=key[1];counts[section]??=0;}if(line.startsWith('  - '))counts[section]++;}
const report={schemaVersion:1,scope:'Static inventory for Unity production. Literal references do not prove invocation, integration, correct semantics or acceptance. Unreviewed entries are not claimed broken.',baselineCommit:execFileSync('git',['rev-parse','HEAD'],{cwd:root,encoding:'utf8'}).trim(),artifactCatalogSha256:crypto.createHash('sha256').update(rules).digest('hex'),catalogCounts:counts,verifiedArtifactCorrections:correctedIds.length,remainingArtifactReviews:artifacts.length-correctedIds.length,artifacts};
const at=process.argv.indexOf('--output');if(at>=0){if(!process.argv[at+1])throw new Error('--output path required');await fs.writeFile(path.resolve(root,process.argv[at+1]),JSON.stringify(report,null,2)+'\n');console.log(`Unity gameplay inventory: ${artifacts.length} artifacts; ${correctedIds.length} corrected/tested, ${artifacts.length-correctedIds.length} require behavioral review.`);}else console.log(JSON.stringify(report,null,2));
