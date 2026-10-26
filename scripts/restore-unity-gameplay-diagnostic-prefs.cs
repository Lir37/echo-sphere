// Completes the local diagnostic transaction after Stop; never prints save values.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before restoring diagnostic preferences");
if(UnityEngine.Application.companyName!="Lir37"||UnityEngine.Application.productName!="ECHO SPHERE")throw new System.Exception("Preference helper requires the main ECHO SPHERE profile");
var json=UnityEditor.SessionState.GetString("EchoGameplayPrefsBackup","");if(string.IsNullOrEmpty(json))throw new System.Exception("No diagnostic preferences backup");
var rows=Newtonsoft.Json.Linq.JArray.Parse(json);var names=new System.Collections.Generic.HashSet<string>();foreach(var row in rows)names.Add((string)row["key"]);
using(var registry=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Unity\UnityEditor\Lir37\ECHO SPHERE"))foreach(var name in registry.GetValueNames()){var key=System.Text.RegularExpressions.Regex.Replace(name,@"_h\d+$","");if(key.StartsWith("echo")&&!names.Contains(key))UnityEngine.PlayerPrefs.DeleteKey(key);}
foreach(var row in rows){var key=(string)row["key"];var value=(string)row["value"];var kind=(string)row["kind"];if(kind=="float")UnityEngine.PlayerPrefs.SetFloat(key,float.Parse(value,System.Globalization.CultureInfo.InvariantCulture));else if(kind=="int")UnityEngine.PlayerPrefs.SetInt(key,int.Parse(value));else UnityEngine.PlayerPrefs.SetString(key,value);}
UnityEngine.PlayerPrefs.Save();int checkedKeys=0;
foreach(var row in rows){var key=(string)row["key"];var kind=(string)row["kind"];var value=kind=="float"?UnityEngine.PlayerPrefs.GetFloat(key).ToString("R",System.Globalization.CultureInfo.InvariantCulture):kind=="int"?UnityEngine.PlayerPrefs.GetInt(key).ToString():UnityEngine.PlayerPrefs.GetString(key);if(value!=(string)row["value"])throw new System.Exception("Diagnostic preferences restoration mismatch");checkedKeys++;}
UnityEngine.Application.runInBackground=false;UnityEditor.PlayerSettings.runInBackground=false;
return new{status="PASS restored original game preferences",checkedKeys};
