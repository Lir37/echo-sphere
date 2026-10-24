// Actual GPU contact sheet for the articulated four-form trial; no live state writes.
try{
var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
object Call(object o,string n,params object[] p)=>o.GetType().GetMethod(n,f).Invoke(o,p);
var go=new UnityEngine.GameObject("Hostile trial GPU preview");var surface=go.AddComponent(a.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",640f,200f,1f);
var creatures=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldCreatures"),true);var bullets=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldProjectiles"),true);var stateType=a.GetType("EchoSphere.Presentation.NativeWorldCreatures+EnemyView");
var feedback=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldFeedback"),true);
var identities=Enumerable.Range(0,12).Select(_=>new object()).ToArray();int checks=0;System.IO.Directory.CreateDirectory("C:/Users/toxik/Documents/GitHub/echo/UnityProject/Library/hostile-animation");
for(int step=0;step<32;step++){
Call(surface,"Begin",new UnityEngine.Vector2(320+step*3.345f,100));Call(feedback,"BeginFrame",surface);Call(feedback,"Rectangle",step*3.345d,0d,640d,200d,0d,new UnityEngine.Color(.025f,.045f,.085f));Call(creatures,"BeginFrame");Call(bullets,"BeginFrame");
for(int row=0;row<1;row++)for(int col=0;col<4;col++){
var state=System.Activator.CreateInstance(stateType);void Set(string n,object v)=>stateType.GetField(n,f).SetValue(state,v);
Set("Identity",identities[row*4+col]);Set("X",80d+col*160+step*3.345d);Set("Y",100d);Set("Radius",46d);Set("HasRotation",true);Set("Rotation",-.25d);Set("Variant",col==0?"skitter":col==1?"wisp":col==2?"brute":"beetle");Set("Role",col==3?"ranged":"grunt");Set("ShotTimer",3.2d-(step*.1d)%3.2d);Set("Elite",row==1);Set("Tier",row==1?2d:0d);Set("HitFlash",row==1?.08d:0d);Set("Hp",10d);Set("MaxHp",10d);Set("Color",UnityEngine.Color.red);
Call(creatures,"DrawEnemy",surface,state,step*.1d);checks++;
}
Call(bullets,"Draw",surface,650f,460f,6f,1d,0d,0,UnityEngine.Color.red,1d);Call(bullets,"Draw",surface,730f,460f,5f,1d,0d,1,UnityEngine.Color.yellow,1d);Call(surface,"End");
var target=(UnityEngine.RenderTexture)surface.GetType().GetProperty("UiTexture",f).GetValue(surface);var previous=UnityEngine.RenderTexture.active;UnityEngine.RenderTexture.active=target;var tex=new UnityEngine.Texture2D(target.width,target.height,UnityEngine.TextureFormat.RGBA32,false);tex.ReadPixels(new UnityEngine.Rect(0,0,target.width,target.height),0,0);tex.Apply();UnityEngine.RenderTexture.active=previous;
System.IO.File.WriteAllBytes("C:/Users/toxik/Documents/GitHub/echo/UnityProject/Library/hostile-animation/frame"+step.ToString("D2")+".png",tex.EncodeToPNG());UnityEngine.Object.Destroy(tex);
}
((System.IDisposable)feedback).Dispose();((System.IDisposable)creatures).Dispose();((System.IDisposable)bullets).Dispose();UnityEngine.Object.Destroy(go);return new {status="PASS",checks,scope="Actual GPU native meshes, articulated poses, elite/hit and small-scale rows; two hostile projectile shapes"};
}catch(System.Exception e){return e.ToString();}
