// Edit Mode check of the actual native preference reader. Preserve storage type.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Edit Mode required");
var rows=Newtonsoft.Json.Linq.JArray.Parse(UnityEditor.SessionState.GetString("EchoGameplayPrefsBackup",""));
const string key="echosphere_interface_scale";
var original=rows.FirstOrDefault(row=>(string)row["key"]==key);
var property=typeof(EchoSphere.Runtime.EchoSphereRuntime).GetProperty("NativeInterfaceScale",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
int checks=0;
void Check(double expected){double actual=(double)property.GetValue(null);checks++;if(System.Math.Abs(actual-expected)>.000001)throw new System.Exception("Native interface scale mismatch: "+actual+" expected "+expected);}
try{
 foreach(float value in new[]{.5f,.75f,1f,1.25f,1.5f}){
  UnityEngine.PlayerPrefs.SetFloat(key,value);Check(System.Math.Clamp((double)value,.75,1.25));
  if(UnityEngine.PlayerPrefs.GetFloat(key)!=value)throw new System.Exception("Reader rewrote legacy float storage");
 }
 foreach(var row in new[]{("",.75),("0.5",.75),("0.75",.75),("1",1d),("1.25",1.25),("1.5",1.25),("NaN",1d),("Infinity",1d),("invalid",1d)}){
  UnityEngine.PlayerPrefs.SetString(key,row.Item1);Check(row.Item2);
  if(UnityEngine.PlayerPrefs.GetString(key)!=row.Item1)throw new System.Exception("Reader rewrote native string storage");
 }
 UnityEngine.PlayerPrefs.DeleteKey(key);Check(.75);
}finally{
 if(original==null)UnityEngine.PlayerPrefs.DeleteKey(key);
 else if((string)original["kind"]=="float")UnityEngine.PlayerPrefs.SetFloat(key,float.Parse((string)original["value"],System.Globalization.CultureInfo.InvariantCulture));
 else UnityEngine.PlayerPrefs.SetString(key,(string)original["value"]);
 UnityEngine.PlayerPrefs.Save();
}
return new{result="PASS native and legacy scale storage, clamping and read-only compatibility",checks};
