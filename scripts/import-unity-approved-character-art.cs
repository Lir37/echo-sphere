// Import user-approved character cutouts as native Unity Sprites. No gameplay data.
var ids=new[]{"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"};
foreach(var id in ids){
 var path="Assets/Resources/EchoSphere/CharacterArt/"+id+".png";
 if(!System.IO.File.Exists(path))throw new System.Exception("Missing character art: "+id);
 UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
 var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
 importer.textureType=UnityEditor.TextureImporterType.Sprite;importer.spriteImportMode=UnityEditor.SpriteImportMode.Single;
 var settings=new UnityEditor.TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=UnityEngine.SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;settings.spriteAlignment=(int)UnityEngine.SpriteAlignment.Center;settings.spritePivot=new UnityEngine.Vector2(.5f,.5f);importer.SetTextureSettings(settings);
 importer.alphaIsTransparency=true;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.isReadable=false;importer.maxTextureSize=2048;importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;importer.filterMode=UnityEngine.FilterMode.Trilinear;importer.SaveAndReimport();
 if(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(path)==null)throw new System.Exception("Sprite import failed: "+id);
}
return new {status="PASS",sprites=ids.Length};
