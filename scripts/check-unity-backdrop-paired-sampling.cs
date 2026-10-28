using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
public static class BackdropPairAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  var flags=BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var et=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeEngineBackdrop");var toggle=et.GetField("DisablePairedSamplingForValidation",flags);bool original=(bool)toggle.GetValue(null);
  GameObject go=null;PanelSettings panel=null;RenderTexture target=null;IDisposable effect=null,neighbour=null;VisualElement pane=null;int stage=0,phase=0,frames=0,last=-1,maxDelta=0,alphaMax=0,weightedMax=0;Color32[] reference=null;var rows=new List<object>();int errors=0;
  var deadline=UnityEditor.EditorApplication.timeSinceStartup+120;bool background=Application.runInBackground;Application.runInBackground=true;
  Application.LogCallback onError=(m,s,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors++;};Application.logMessageReceived+=onError;
  void Dispose(){effect?.Dispose();neighbour?.Dispose();effect=neighbour=null;if(panel!=null)panel.targetTexture=null;if(go!=null)UnityEngine.Object.Destroy(go);if(panel!=null)UnityEngine.Object.Destroy(panel);if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}go=null;panel=null;target=null;}
  void Build(){
   float dpr=new[]{1f,1.75f,2.75f}[stage%3];float sigma=new[]{2f,6f,12f}[stage/3];
   go=new GameObject("Backdrop paired GPU acceptance");target=new RenderTexture(960,720,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();
   panel=ScriptableObject.CreateInstance<PanelSettings>();panel.targetTexture=target;panel.scale=dpr;panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.sortingOrder=2300;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
   var doc=go.AddComponent<UIDocument>();doc.panelSettings=panel;var root=doc.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;root.style.backgroundColor=new Color(.02f,.07f,.11f,1);
   float w=960/dpr,h=720/dpr;
   for(int i=0;i<48;i++){var tile=new VisualElement();tile.style.position=Position.Absolute;tile.style.left=(i%8)*w/8;tile.style.top=(i/8)*h/6;tile.style.width=w/8;tile.style.height=h/6;tile.style.backgroundColor=new Color((i%5)/5f,((i*3)%7)/7f,((i*5)%9)/9f,.25f+(i%4)*.2f);root.Add(tile);}
   var label=new Label("ECHO 123 Ж — alpha + gradients");label.style.fontSize=28;label.style.color=Color.white;label.style.position=Position.Absolute;label.style.top=h*.35f;root.Add(label);
   pane=new VisualElement();pane.style.position=Position.Absolute;pane.style.left=w*.22f+.37f;pane.style.top=h*.18f+.63f;pane.style.width=w*.53f;pane.style.height=h*.51f;pane.style.borderTopLeftRadius=pane.style.borderTopRightRadius=pane.style.borderBottomLeftRadius=pane.style.borderBottomRightRadius=18;if(stage%2==1)pane.style.rotate=new Rotate(new Angle(17,AngleUnit.Degree));root.Add(pane);
   effect=(IDisposable)Activator.CreateInstance(et,flags,null,new object[]{pane,sigma},null);
   var other=new VisualElement();other.style.position=Position.Absolute;other.style.left=w*.08f;other.style.top=h*.72f;other.style.width=w*.2f;other.style.height=h*.12f;root.Add(other);neighbour=(IDisposable)Activator.CreateInstance(et,flags,null,new object[]{other,4f},null);
   toggle.SetValue(null,true);frames=0;
  }
  Color32[] Read(){var old=RenderTexture.active;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);return image.GetPixels32();}finally{RenderTexture.active=old;UnityEngine.Object.Destroy(image);}}
  UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;Application.logMessageReceived-=onError;toggle.SetValue(null,original);Application.runInBackground=background;Dispose();var report=new{result,stage,maxDelta,alphaMax,weightedMax,errors,rows};System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/backdrop-pair-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(report));UnityEditor.SessionState.SetString("EchoBackdropPair",Newtonsoft.Json.JsonConvert.SerializeObject(report));}
  tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Play/deadline");if(last==Time.frameCount)return;last=Time.frameCount;if(errors>0)throw new Exception("new Console errors "+errors);
   if(go==null){Build();phase=0;return;}pane.MarkDirtyRepaint();if(++frames<10)return;
   if(phase==0){reference=Read();toggle.SetValue(null,false);phase=1;frames=0;return;}
   if(phase==2){var control=Read();int changed=0;for(int i=0;i<control.Length;i++)if(Math.Abs(control[i].r-reference[i].r)+Math.Abs(control[i].g-reference[i].g)+Math.Abs(control[i].b-reference[i].b)>5)changed++;if(changed<100)throw new Exception("Blur positive control was empty");rows.Add(new{stage,positiveControlPixels=changed});Dispose();if(++stage==9){Finish("PASS");return;}return;}
   var actual=Read();int delta=0,alpha=0,weighted=0,visible=0;long sum=0;
   for(int i=0;i<actual.Length;i++){var a=reference[i];var b=actual[i];if(a.a>0)visible++;delta=Math.Max(delta,Math.Max(Math.Max(Math.Abs(a.r-b.r),Math.Abs(a.g-b.g)),Math.Max(Math.Abs(a.b-b.b),Math.Abs(a.a-b.a))));alpha=Math.Max(alpha,Math.Abs(a.a-b.a));int v=Math.Max(Math.Max(Math.Abs(a.r*a.a-b.r*b.a),Math.Abs(a.g*a.a-b.g*b.a)),Math.Abs(a.b*a.a-b.b*b.a))/255;weighted=Math.Max(weighted,v);sum+=v;}
   rows.Add(new{stage,delta,alpha,weighted,mean=(double)sum/actual.Length,visible});maxDelta=Math.Max(maxDelta,delta);alphaMax=Math.Max(alphaMax,alpha);weightedMax=Math.Max(weightedMax,weighted);
   if(visible==0||alpha>2||weighted>3)throw new Exception("GPU mismatch "+delta+" alpha="+alpha+" weighted="+weighted);
   effect.Dispose();effect=null;phase=2;frames=0;return;
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.SessionState.SetString("EchoBackdropPair","RUNNING");UnityEditor.EditorApplication.update+=tick;return "queued";
 }
}
