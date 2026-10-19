// Run before diagnostic Play entry; never prints preference or save values.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Back up preferences in Edit Mode");
using(var registry=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Unity\UnityEditor\Lir37\ECHO SPHERE")){
 var rows=new System.Collections.Generic.List<object>();
 if(registry!=null)foreach(var name in registry.GetValueNames()){
  var key=System.Text.RegularExpressions.Regex.Replace(name,@"_h\d+$","");if(!key.StartsWith("echo"))continue;
  // EchoSphereSourceSettings is the only project SetFloat caller.
  var kind=key=="echosphere_interface_scale"?"float":registry.GetValueKind(name)==Microsoft.Win32.RegistryValueKind.DWord?"int":"string";
  rows.Add(new{key,kind,value=kind=="float"?UnityEngine.PlayerPrefs.GetFloat(key).ToString("R",System.Globalization.CultureInfo.InvariantCulture):kind=="int"?UnityEngine.PlayerPrefs.GetInt(key).ToString():UnityEngine.PlayerPrefs.GetString(key)});
 }
 UnityEditor.SessionState.SetString("EchoGameplayPrefsBackup",Newtonsoft.Json.JsonConvert.SerializeObject(rows));
 return new{savedKeys=rows.Count};
}
