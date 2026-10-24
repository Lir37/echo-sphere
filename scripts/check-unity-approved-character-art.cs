// Actual native GPU roster draw, without changing the live player or preferences.
try {
 var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
 object Call(object o,string n,params object[] p)=>o.GetType().GetMethod(n,f).Invoke(o,p);
 var go=new UnityEngine.GameObject("Approved character roster check");var surface=go.AddComponent(a.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",1000f,500f,1f);Call(surface,"Begin",new UnityEngine.Vector2(500,250));
 var player=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldPlayer"),true);var sprites=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldSprites"),true);var shadows=System.Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldCreatures"),true);
 Call(player,"BeginFrame");Call(sprites,"BeginFrame");Call(shadows,"BeginFrame");
 var stateType=a.GetType("EchoSphere.Presentation.NativeWorldPlayer+State");var draw=player.GetType().GetMethod("Draw",f);var resolver=new System.Func<string,EchoSphere.Presentation.INativeWorldSprite>(_=>null);
 var ids=new[]{"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"};int checks=0;
 for(int i=0;i<ids.Length;i++){
  var sprite=UnityEngine.Resources.Load<UnityEngine.Sprite>("EchoSphere/CharacterArt/"+ids[i]);if(sprite==null)throw new System.Exception("Missing "+ids[i]);checks++;
  var state=System.Activator.CreateInstance(stateType);void Set(string n,object v)=>stateType.GetField(n,f).SetValue(state,v);
  Set("Character",ids[i]);Set("X",100d+(i%5)*200d);Set("Y",125d+(i/5)*250d);Set("Radius",55d);Set("Time",1d);Set("Hp",10d);Set("Color",UnityEngine.Color.cyan);
  draw.Invoke(player,new object[]{surface,state,sprites,shadows,resolver});
 }
 Call(surface,"End");var target=(UnityEngine.RenderTexture)surface.GetType().GetProperty("UiTexture",f).GetValue(surface);var old=UnityEngine.RenderTexture.active;UnityEngine.RenderTexture.active=target;var tex=new UnityEngine.Texture2D(target.width,target.height,UnityEngine.TextureFormat.RGBA32,false);tex.ReadPixels(new UnityEngine.Rect(0,0,target.width,target.height),0,0);tex.Apply();UnityEngine.RenderTexture.active=old;
 for(int i=0;i<10;i++){int visible=0;for(int y=0;y<210;y++)for(int x=0;x<180;x++){var c=tex.GetPixel(i%5*200+10+x,i/5*250+20+y);if(c.a>.1f&&c.maxColorComponent>.1f)visible++;}if(visible<100)throw new System.Exception("Empty roster cell "+i);checks++;}
 System.IO.File.WriteAllBytes("C:/Users/toxik/Documents/GitHub/echo/UnityProject/Library/approved-character-roster-native.png",tex.EncodeToPNG());
 ((System.IDisposable)player).Dispose();((System.IDisposable)sprites).Dispose();((System.IDisposable)shadows).Dispose();UnityEngine.Object.Destroy(tex);UnityEngine.Object.Destroy(go);
 return new {status="PASS",checks,scope="All ten approved Sprites rendered through actual NativeWorldPlayer GPU path; no live player/profile changes"};
}catch(System.Exception e){return e.ToString();}
