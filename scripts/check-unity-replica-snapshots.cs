var assembly=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var type=assembly.GetType("EchoSphere.Presentation.SourceUiReplicaSnapshotCache");
var cache=System.Activator.CreateInstance(type,true);
var begin=type.GetMethod("Begin",flags);var end=type.GetMethod("End",flags);var apply=type.GetMethod("Apply",flags);
var replica=assembly.GetType("EchoSphere.Presentation.SourceUiReplica");int comparisons=0;
var source=new UnityEngine.UIElements.Label("snapshot text");
for(int pass=0;pass<40;pass++){
 source.style.width=83+pass;source.style.height=42+pass;source.style.opacity=pass/40f;
 source.style.color=new UnityEngine.Color(pass/40f,.2f,.7f);source.style.fontSize=12+pass;
 source.style.translate=new UnityEngine.UIElements.Translate(pass,-pass);source.style.rotate=new UnityEngine.UIElements.Rotate(pass);
 source.style.paddingLeft=pass;source.style.marginTop=pass;
 foreach(bool helper in new[]{false,true}){
  begin.Invoke(cache,null);
  for(int repeat=0;repeat<3;repeat++){
   var expected=new UnityEngine.UIElements.Label();var actual=new UnityEngine.UIElements.Label();
   replica.GetMethod(helper?"CopyInlineGeometry":"CopyAppearance",flags).Invoke(null,new object[]{source,expected.style});
   apply.Invoke(cache,new object[]{source,actual.style,helper});
   foreach(var p in typeof(UnityEngine.UIElements.IStyle).GetProperties())if(p.CanRead){
    if(!object.Equals(p.GetValue(expected.style),p.GetValue(actual.style)))throw new System.Exception("Snapshot mismatch: "+p.Name);comparisons++;
   }
  }
  end.Invoke(cache,null);
 }
}
begin.Invoke(cache,null);end.Invoke(cache,null);
if(((System.Collections.IDictionary)type.GetField("_values",flags).GetValue(cache)).Count!=0)throw new System.Exception("Stale sources retained");
return new{comparisons,result="PASS",scope="All IStyle properties, both source kinds, repeated reads, changed pass values, pruning"};
