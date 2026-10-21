// Editor content import from the already locked source definitions; no gameplay
// execution or save mutation. The resulting ScriptableObject is player data.
if(UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Import in Edit Mode");
const string path="Assets/Resources/EchoSphere/NativeProgressionCatalog.asset";
var catalog=UnityEditor.AssetDatabase.LoadAssetAtPath<EchoSphere.Presentation.NativeProgressionCatalog>(path);
bool created=catalog==null;if(created)catalog=UnityEngine.ScriptableObject.CreateInstance<EchoSphere.Presentation.NativeProgressionCatalog>();
catalog.SourceCommit="737ef2af4a877e0995db5f92331d536d2f29a934";
catalog.Achievements.Clear();
var sourceType=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.Visual_gameData",true);
var source=(EchoSphere.Presentation.V)sourceType.GetField("Scope").GetValue(null);
foreach(var row in source["ACHIEVEMENTS"].Items)catalog.Achievements.Add(new EchoSphere.Presentation.NativeProgressionCatalog.Achievement {Id=row["id"].Text,NameRu=row["name"]["ru"].Text,NameEn=row["name"]["en"].Text,DescriptionRu=row["desc"]["ru"].Text,DescriptionEn=row["desc"]["en"].Text});
if(created)UnityEditor.AssetDatabase.CreateAsset(catalog,path);else UnityEditor.EditorUtility.SetDirty(catalog);
UnityEditor.AssetDatabase.SaveAssets();return new {path,achievements=catalog.Achievements.Count};
