UnityEngine.Application.runInBackground=true;
UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
var doc=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var entries=(System.Collections.IDictionary)doc.GetType().GetField("_entries",flags).GetValue(doc);
var cacheType=doc.GetType().Assembly.GetType("EchoSphere.Presentation.SourceUiReplicaStyle");
var root=doc.GetType().GetField("_root",flags).GetValue(doc);
var prepare=doc.GetType().GetMethod("PreparePaintResources",flags);
int copies=0;
var snapshots=doc.GetType().GetField("_replicaSnapshots",flags).GetValue(doc);
void Mode(bool cached){
 copies=0;
 foreach(System.Collections.DictionaryEntry p in entries){
  var backdrop=p.Value.GetType().GetField("Backdrop",flags).GetValue(p.Value);if(backdrop==null)continue;
  var replica=backdrop.GetType().GetField("_replica",flags).GetValue(backdrop);
  replica.GetType().GetField("_snapshots",flags).SetValue(replica,cached?snapshots:null);
  copies+=((System.Collections.IDictionary)replica.GetType().GetField("_copies",flags).GetValue(replica)).Count;
 }
}
var properties=typeof(UnityEngine.UIElements.IStyle).GetProperties();
var before=new System.Collections.Generic.Dictionary<UnityEngine.UIElements.VisualElement,object[]>();int comparisons=0;
try{
 Mode(false);prepare.Invoke(doc,new[]{root});
 foreach(System.Collections.DictionaryEntry p in entries){
  var backdrop=p.Value.GetType().GetField("Backdrop",flags).GetValue(p.Value);if(backdrop==null)continue;
  var replica=backdrop.GetType().GetField("_replica",flags).GetValue(backdrop);
  var map=(System.Collections.IDictionary)replica.GetType().GetField("_copies",flags).GetValue(replica);
  foreach(System.Collections.DictionaryEntry c in map){
   var element=(UnityEngine.UIElements.VisualElement)c.Value.GetType().GetField("Element",flags).GetValue(c.Value);
   var values=new object[properties.Length];for(int i=0;i<properties.Length;i++)values[i]=properties[i].GetValue(element.style);before.Add(element,values);
  }
 }
 Mode(true);prepare.Invoke(doc,new[]{root});
 foreach(var pair in before)for(int i=0;i<properties.Length;i++){
  if(!object.Equals(pair.Value[i],properties[i].GetValue(pair.Key.style)))throw new System.Exception("Live backdrop mismatch: "+properties[i].Name);comparisons++;
 }
}finally{Mode(true);}
return new{comparisons,copies=before.Count,result="PASS",scope="Same-frame live backdrop styles with/without shared snapshots"};
