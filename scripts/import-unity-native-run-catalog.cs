if(UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Import in Edit Mode");
const string path="Assets/Resources/EchoSphere/NativeRunCatalog.asset";
var catalog=UnityEditor.AssetDatabase.LoadAssetAtPath<EchoSphere.Presentation.NativeRunCatalog>(path);bool created=catalog==null;
if(created)catalog=UnityEngine.ScriptableObject.CreateInstance<EchoSphere.Presentation.NativeRunCatalog>();
catalog.SourceCommit="737ef2af4a877e0995db5f92331d536d2f29a934";
catalog.Spheres.Clear();catalog.Abilities.Clear();catalog.Artifacts.Clear();catalog.Synergies.Clear();catalog.Sets.Clear();catalog.Elements.Clear();catalog.Rarities.Clear();catalog.SphereAbilityLinks.Clear();
EchoSphere.Presentation.V Scope(string name)=>(EchoSphere.Presentation.V)typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.Visual_"+name,true).GetField("Scope").GetValue(null);
var data=Scope("gameData");var progression=Scope("sphereProgression");var artifacts=Scope("artifactSystem");
void Fill(EchoSphere.Presentation.NativeRunCatalog.Text row,EchoSphere.Presentation.V source){row.NameRu=source["name"]["ru"].Text;row.NameEn=source["name"]["en"].Text;row.DescriptionRu=source["desc"]["ru"].Text;row.DescriptionEn=source["desc"]["en"].Text;}
EchoSphere.Presentation.NativeRunCatalog.Choice Choice(EchoSphere.Presentation.V source){var row=new EchoSphere.Presentation.NativeRunCatalog.Choice {Id=source["id"].Text};Fill(row,source);return row;}
foreach(var pair in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)data["SPHERE_TYPES"].Raw){
 var row=new EchoSphere.Presentation.NativeRunCatalog.Sphere {Id=pair.Key,Color=pair.Value["color"].Text,RangeMultiplier=pair.Value["rangeMult"].Number,Projectile=data["sphereUsesProjectileModifiers"].Call(pair.Key).Truth};Fill(row,pair.Value);
 foreach(var item in progression["SPHERE_PROGRESSION"][pair.Key]["evolution4Choices"].Items){var branch=Choice(item);var element=progression["getSphereElementForBranch"].Call(branch.Id);branch.Element=element.Truth?element.Text:null;foreach(var final in item["final"].Items)branch.Finals.Add(Choice(final));row.Branches.Add(branch);}catalog.Spheres.Add(row);
}
foreach(var pair in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)data["ABILITIES"].Raw){
 var row=new EchoSphere.Presentation.NativeRunCatalog.Ability {Id=pair.Key,Category=pair.Value["category"].Text};Fill(row,pair.Value);
 for(int level=0;level<=7;level++)row.BaseLevels.Add(new EchoSphere.Presentation.NativeRunCatalog.Text {DescriptionRu=pair.Value["desc"]["ru"].Call(level).Text,DescriptionEn=pair.Value["desc"]["en"].Call(level).Text});
 var def=progression["ABILITY_PROGRESSION"][pair.Key];row.HasProgression=def.Truth;
 if(def.Truth){
  foreach(var item in def["levels"].Items){var text=new EchoSphere.Presentation.NativeRunCatalog.Text();Fill(text,item);row.Levels.Add(text);}
  foreach(var item in def["evolution4"].Items){var branch=Choice(item);var pool=progression["ABILITY_FINAL_POOLS"][pair.Key][branch.Id];branch.HasFinalPool=!pool.Null&&!pool.Undefined;foreach(var final in pool.Items)branch.Finals.Add(Choice(final));row.Branches.Add(branch);}
  foreach(var item in def["evolution7"].Items)row.FallbackFinals.Add(Choice(item));
 }catalog.Abilities.Add(row);
}
foreach(var item in data["ARTIFACTS"].Items){
 var id=item["id"].Text;var meta=artifacts["ARTIFACT_META"][id];var row=new EchoSphere.Presentation.NativeRunCatalog.Artifact {Id=id,Rarity=meta["rarity"].Text,Mechanic=meta["mechanic"].Text};Fill(row,item);
 foreach(var effect in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)meta["effects"].Raw)row.Effects.Add(new EchoSphere.Presentation.NativeRunCatalog.Effect {Id=effect.Key,Value=effect.Value.Number});catalog.Artifacts.Add(row);
}
foreach(var pair in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)artifacts["RARITY_LABELS"].Raw)catalog.Rarities.Add(new EchoSphere.Presentation.NativeRunCatalog.Definition {Id=pair.Key,NameRu=pair.Value["ru"].Text,NameEn=pair.Value["en"].Text});
foreach(var item in artifacts["ARTIFACT_SYNERGIES"].Items){var row=new EchoSphere.Presentation.NativeRunCatalog.Synergy {Id=item["id"].Text};Fill(row,item);foreach(var id in item["requires"].Items)row.Requires.Add(id.Text);catalog.Synergies.Add(row);}
foreach(var item in artifacts["ARTIFACT_SETS"].Items){
 var row=new EchoSphere.Presentation.NativeRunCatalog.Set {Id=item["id"].Text};Fill(row,item);var seen=new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
 foreach(var id in item["synergyIds"].Items){row.SynergyIds.Add(id.Text);foreach(var synergy in catalog.Synergies)if(synergy.Id==id.Text)foreach(var required in synergy.Requires)if(seen.Add(required))row.ArtifactIds.Add(required);}catalog.Sets.Add(row);
}
foreach(var pair in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)progression["SPHERE_ELEMENT_META"].Raw){var row=new EchoSphere.Presentation.NativeRunCatalog.Element {Id=pair.Key,Icon=pair.Value["icon"].Text,Color=pair.Value["color"].Text};Fill(row,pair.Value);catalog.Elements.Add(row);}
foreach(var item in progression["SPHERE_ABILITY_SYNERGIES"].Items){var row=new EchoSphere.Presentation.NativeRunCatalog.SphereAbilityLink {Id=item["id"].Text,Sphere=item["sphere"].Text,Branch=item["sphereBranch"].Text,Ability=item["ability"].Text,AbilityFinal=item["abilityFinal"].Text};Fill(row,item);catalog.SphereAbilityLinks.Add(row);}
if(catalog.Spheres.Count!=10||catalog.Abilities.Count<20||catalog.Artifacts.Count<40||catalog.Sets.Count!=12)throw new System.InvalidOperationException("Incomplete native run catalog");
if(created)UnityEditor.AssetDatabase.CreateAsset(catalog,path);else UnityEditor.EditorUtility.SetDirty(catalog);UnityEditor.AssetDatabase.SaveAssets();
return new {path,spheres=catalog.Spheres.Count,abilities=catalog.Abilities.Count,artifacts=catalog.Artifacts.Count,synergies=catalog.Synergies.Count,sets=catalog.Sets.Count};
