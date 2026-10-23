if(UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Import in Edit Mode");
const string path="Assets/Resources/EchoSphere/NativeWorldArtCatalog.asset";
var catalog=UnityEditor.AssetDatabase.LoadAssetAtPath<EchoSphere.Presentation.NativeWorldArtCatalog>(path);
bool created=catalog==null;if(created)catalog=UnityEngine.ScriptableObject.CreateInstance<EchoSphere.Presentation.NativeWorldArtCatalog>();
catalog.SourceCommit="737ef2af4a877e0995db5f92331d536d2f29a934";
catalog.Entries.Clear();
var source=System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../../src/renderer.ts"));
var block=System.Text.RegularExpressions.Regex.Match(source,@"const ART_PATHS:[^=]+=[ ]*\{([\s\S]*?)\};").Groups[1].Value;
foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(block,@"(?:'([^']+)'|([a-zA-Z][\w-]*)):[ ]*'([^']+)'")){
    string key=match.Groups[1].Success?match.Groups[1].Value:match.Groups[2].Value;string sourcePath=match.Groups[3].Value;
    var resource=EchoSphere.Presentation.SourceCanvasArtIndex.Resources[sourcePath];var size=EchoSphere.Presentation.SourceCanvasArtIndex.Dimensions[sourcePath];
    var sprite=UnityEngine.Resources.Load<UnityEngine.Sprite>(resource);if(sprite==null)throw new System.InvalidOperationException("Missing sprite "+resource);
    catalog.Entries.Add(new EchoSphere.Presentation.NativeWorldArtCatalog.Entry {Key=key,Asset=sprite,SourceWidth=size[0],SourceHeight=size[1]});
}
if(catalog.Entries.Count!=17)throw new System.InvalidOperationException("Incomplete world artwork catalog "+catalog.Entries.Count);
if(created)UnityEditor.AssetDatabase.CreateAsset(catalog,path);else UnityEditor.EditorUtility.SetDirty(catalog);
UnityEditor.AssetDatabase.SaveAssets();return new {path,entries=catalog.Entries.Count};
