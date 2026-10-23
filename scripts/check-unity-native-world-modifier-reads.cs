var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var source=(EchoSphere.Presentation.V)assembly.GetType("EchoSphere.Presentation.Visual_sphereProgression",true).GetField("Scope").GetValue(null);int checks=0;
foreach(var sphere in EchoSphere.Presentation.NativeRunCatalog.Instance.Spheres)foreach(var branch in sphere.Branches)foreach(int final in new[]{-1,0,1,2}){
 var markers=final<0?EchoSphere.Presentation.VisualJs.Arr():EchoSphere.Presentation.VisualJs.Arr(new EchoSphere.Presentation.V("sphere:"+sphere.Id+":7:"+final));
 var player=EchoSphere.Presentation.VisualJs.Obj("sphereBranches",EchoSphere.Presentation.VisualJs.Obj(sphere.Id,branch.Id),"evolutions",markers);
 var original=source["getAuthoredSphereModifierLevels"].Call(player,sphere.Id);
 foreach(var kind in EchoSphere.Core.SphereAuthoredModifierRules.AllModifiers){var key=kind.ToString();key=char.ToLowerInvariant(key[0])+key.Substring(1);var value=original[key];double expected=value.Truth?value.Number:0;int actual=EchoSphere.Core.SphereAuthoredModifierRules.GetLevel(branch.Id,final<0?null:branch.Id+"_final_"+(final+1),kind);if(actual!=expected)throw new System.Exception(branch.Id+"/"+final+"/"+key);checks++;}
}
return new {checks};
