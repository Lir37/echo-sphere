var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;
EchoSphere.Presentation.V Scope(string name)=>(EchoSphere.Presentation.V)assembly.GetType("EchoSphere.Presentation.Visual_"+name,true).GetField("Scope").GetValue(null);
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;var runtime=typeof(EchoSphere.Runtime.EchoSphereRuntime);var catalog=EchoSphere.Presentation.NativeRunCatalog.Instance;var original=Scope("sphereProgression");int checks=0;
void Equal(string actual,string expected,string context){if(actual!=expected)throw new System.Exception(context+": "+actual+" != "+expected);checks++;}
foreach(var ability in catalog.Abilities)foreach(bool ru in new[]{true,false})foreach(int level in new[]{0,1,2,3,4,5,6,7}){
 var markers=new System.Collections.Generic.List<string[]> {new string[0],new[]{"ability:"+ability.Id+":4:invalid"}};
 foreach(var branch in ability.Branches){markers.Add(new[]{"ability:"+ability.Id+":4:"+branch.Id});foreach(var final in branch.HasFinalPool?branch.Finals:ability.FallbackFinals)markers.Add(new[]{"ability:"+ability.Id+":4:"+branch.Id,"ability:"+ability.Id+":7:"+final.Id});}
 foreach(var selection in markers){var scene=EchoSphere.Presentation.VisualJs.Obj("player",EchoSphere.Presentation.VisualJs.Obj("abilities",EchoSphere.Presentation.VisualJs.Obj(ability.Id,level),"evolutions",EchoSphere.Presentation.VisualJs.Arr(System.Linq.Enumerable.Select(selection,id=>new EchoSphere.Presentation.V(id)).ToArray())));string lang=ru?"ru":"en";
  Equal((string)runtime.GetMethod("NativeAbilityName",flags).Invoke(null,new object[]{scene,ability.Id,ru}),original["getAbilityDisplayName"].Call(scene,ability.Id,lang).Text,"name/"+ability.Id);
  Equal((string)runtime.GetMethod("NativeAbilityDescription",flags).Invoke(null,new object[]{scene,ability.Id,ru}),original["getAbilityDisplayDesc"].Call(scene,ability.Id,lang).Text,"desc/"+ability.Id);
 }
 Equal((string)runtime.GetMethod("NativeBaseAbilityDescription",flags).Invoke(null,new object[]{ability,(double)level,ru}),Scope("gameData")["ABILITIES"][ability.Id]["desc"][ru?"ru":"en"].Call(level).Text,"base/"+ability.Id);
}
var artifactsSource=Scope("artifactSystem");
foreach(var set in catalog.Sets)foreach(bool ru in new[]{true,false})foreach(bool complete in new[]{false,true}){
 var ids=complete?set.ArtifactIds:new System.Collections.Generic.List<string>();
 var values=System.Linq.Enumerable.Select(ids,id=>new EchoSphere.Presentation.V(id)).ToArray();
 var scene=EchoSphere.Presentation.VisualJs.Obj("player",EchoSphere.Presentation.VisualJs.Obj("artifacts",EchoSphere.Presentation.VisualJs.Arr(values)));
 var owned=new System.Collections.Generic.HashSet<string>(ids);var result=(EchoSphere.Presentation.NativePauseArtifactsState)runtime.GetMethod("ProjectNativePauseArtifactsState",flags).Invoke(null,new object[]{scene,ru});
 foreach(var row in result.Artifacts){var originalArtifact=Scope("gameData")["ARTIFACT_MAP"][row.Id];Equal(row.Name,originalArtifact["name"][ru?"ru":"en"].Text,"artifact name");Equal(row.Description,originalArtifact["desc"][ru?"ru":"en"].Text,"artifact desc");Equal(row.Rarity,artifactsSource["artifactRarity"].Call(row.Id).Text,"rarity");
  var originalSets=System.Linq.Enumerable.ToList(artifactsSource["getArtifactSetsForArtifact"].Call(scene,row.Id).Items);if(row.Sets.Count!=originalSets.Count)throw new System.Exception("set membership");
  for(int i=0;i<row.Sets.Count;i++){Equal(row.Sets[i].Id,originalSets[i]["setId"].Text,"set id");Equal(row.Sets[i].Count,originalSets[i]["owned"].Text+"/"+originalSets[i]["total"].Text,"set progress");}
 }
 var native=(System.Collections.Generic.IReadOnlyList<EchoSphere.Presentation.NativeRunCatalog.Synergy>)runtime.GetMethod("NativeArtifactSynergies",flags).Invoke(null,new object[]{scene,null});
 var expected=artifactsSource["getActiveArtifactSynergies"].Call(scene);Equal(string.Join("|",System.Linq.Enumerable.Select(native,row=>row.Id)),string.Join("|",System.Linq.Enumerable.Select(expected.Items,row=>row["id"].Text)),"active synergies");
}
return new {checks,spheres=catalog.Spheres.Count,abilities=catalog.Abilities.Count,artifacts=catalog.Artifacts.Count,sets=catalog.Sets.Count};
