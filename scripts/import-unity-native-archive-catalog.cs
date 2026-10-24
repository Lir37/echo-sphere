// Materialize authored archive content as a native Unity asset. No save mutation.
if(UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Import in Edit Mode");
const string path="Assets/Resources/EchoSphere/NativeArchiveCatalog.asset";
var catalog=UnityEditor.AssetDatabase.LoadAssetAtPath<EchoSphere.Presentation.NativeArchiveCatalog>(path);
bool created=catalog==null;if(created)catalog=UnityEngine.ScriptableObject.CreateInstance<EchoSphere.Presentation.NativeArchiveCatalog>();
var transfer=UnityEngine.JsonUtility.FromJson<EchoSphere.Presentation.NativeArchiveCatalog.Transfer>(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Editor/EchoSphereReference/SourceCatalog.json").text);
catalog.SourceCommit=transfer.sourceCommit;catalog.SourceDigest=transfer.sourceDigest;catalog.Entries=transfer.entries;catalog.Planner=transfer.planner;
var sourceType=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.Visual_sphereProgression",true);
var source=(EchoSphere.Presentation.V)sourceType.GetField("Scope").GetValue(null);
var progression=source["ABILITY_PROGRESSION"];
foreach(var entry in catalog.Entries){if(entry.section!="abilities")continue;var definition=progression[entry.id.Substring("ability:".Length)];if(!definition.Truth)continue;var desc=definition["levels"][0]["desc"];if(desc.Truth){entry.descRu=desc["ru"].Text;entry.descEn=desc["en"].Text;}entry.detailsRu="Максимум: 7 уровней. На IV и VII открывается отдельный выбор эволюции.";entry.detailsEn="Max level: 7. Separate evolution choices appear at IV and VII.";}
if(created)UnityEditor.AssetDatabase.CreateAsset(catalog,path);else UnityEditor.EditorUtility.SetDirty(catalog);
UnityEditor.AssetDatabase.SaveAssets();return new {path,entries=catalog.Entries.Count,planner=catalog.Planner.Count};
