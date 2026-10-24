// Actual GPU contact sheet for the articulated four-form trial; no live state writes.
try{
var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
object Call(object o,string n,params object[] p)=>o.GetType().GetMethod(n,f).Invoke(o,p);
var go=new UnityEngine.GameObject("Hostile trial GPU preview");var surface=go.AddComponent(a.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",800f,500f,1f);
var creatures=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldCreatures"),true);var bullets=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldProjectiles"),true);var stateType=a.GetType("EchoSphere.Presentation.NativeWorldCreatures+EnemyView");
var identities=Enumerable.Range(0,12).Select(_=>new object()).ToArray();int checks=0;
for(int step=0;step<2;step++){
Call(surface,"Begin",new UnityEngine.Vector2(400,250));Call(creatures,"BeginFrame");Call(bullets,"BeginFrame");
for(int row=0;row<3;row++)for(int col=0;col<4;col++){
var state=System.Activator.CreateInstance(stateType);void Set(string n,object v)=>stateType.GetField(n,f).SetValue(state,v);
Set("Identity",identities[row*4+col]);Set("X",100d+col*200+(step==0?-row*28:0));Set("Y",80d+row*150);Set("Radius",row==2?22d:57d);Set("HasRotation",true);Set("Rotation",-.25d);Set("Variant",col==0?"skitter":col==1?"wisp":col==2?"brute":"beetle");Set("Role",col==3?"ranged":"grunt");Set("ShotTimer",row==1?3.17d:.2d);Set("Elite",row==1);Set("Tier",row==1?2d:0d);Set("HitFlash",row==1?.08d:0d);Set("Hp",10d);Set("MaxHp",10d);Set("Color",UnityEngine.Color.red);
Call(creatures,"DrawEnemy",surface,state,1d+row*.6);checks++;
}
Call(bullets,"Draw",surface,650f,460f,6f,1d,0d,0,UnityEngine.Color.red,1d);Call(bullets,"Draw",surface,730f,460f,5f,1d,0d,1,UnityEngine.Color.yellow,1d);Call(surface,"End");
}
var target=(UnityEngine.RenderTexture)surface.GetType().GetProperty("UiTexture",f).GetValue(surface);var previous=UnityEngine.RenderTexture.active;UnityEngine.RenderTexture.active=target;var tex=new UnityEngine.Texture2D(target.width,target.height,UnityEngine.TextureFormat.RGBA32,false);tex.ReadPixels(new UnityEngine.Rect(0,0,target.width,target.height),0,0);tex.Apply();UnityEngine.RenderTexture.active=previous;

for(int row=0;row<3;row++)for(int col=0;col<4;col++){int visible=0;for(int y=0;y<100;y++)for(int x=0;x<140;x++){var c=tex.GetPixel(col*200+30+x,500-(row*150+30+y));if(c.a>.1f)visible++;}if(visible<40)throw new System.Exception("Empty hostile preview cell "+row+","+col);}
var retained=(System.Collections.IDictionary)creatures.GetType().GetField("_hostileMeshes",f).GetValue(creatures);if(retained.Count!=7)throw new System.Exception("Expected seven shared immutable part meshes, got "+retained.Count);
int sharedPartMeshes=retained.Count;System.IO.File.WriteAllBytes("C:/Users/toxik/Documents/GitHub/echo/UnityProject/Library/simple-hostiles-native-preview.png",tex.EncodeToPNG());((System.IDisposable)creatures).Dispose();((System.IDisposable)bullets).Dispose();UnityEngine.Object.Destroy(tex);UnityEngine.Object.Destroy(go);return new {status="PASS",drawSamples=checks,pixelCoverageCells=12,sharedPartMeshes,scope="Actual GPU native meshes, articulated poses, elite/hit and small-scale rows; two hostile projectile shapes"};
}catch(System.Exception e){return e.ToString();}
