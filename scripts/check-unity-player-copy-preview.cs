using System;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.UIElements;using UnityEditor;using EchoSphere.Presentation;using Newtonsoft.Json.Linq;
public static class PlayerCopyPreview {
 public static object Main(){
  if(!EditorApplication.isPlaying)throw new Exception("Play Mode required");
  string output=Path.GetFullPath("Library/player-copy/preview");Directory.CreateDirectory(output);
  var models=JArray.Parse(File.ReadAllText("Library/player-copy/runtime-states.json"));
  var states=models.Where(x=>(string)x["screen"]=="ReadNativeMainMenuState").Select(x=>x["state"].ToObject<NativeMainMenuView.State>()).ToArray();
  var before=Application.runInBackground;Application.runInBackground=true;EditorApplication.isPaused=false;EditorApplication.ExecuteMenuItem("Window/General/Game");
  GameObject owner=null;PanelSettings panel=null;RenderTexture target=null;NativeMainMenuView view=null;NativeMainMenuComposition composition=null;int phase=0,frames=0,last=-1;double start=EditorApplication.timeSinceStartup;
  void Release(){composition?.Dispose();view?.Dispose();if(panel!=null)panel.targetTexture=null;if(owner!=null)UnityEngine.Object.Destroy(owner);if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}if(panel!=null)UnityEngine.Object.Destroy(panel);}
  void Setup(){int width=phase%2==0?540:960,height=phase%2==0?960:540;owner=new GameObject("Copy preview");panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();panel.targetTexture=target;var doc=owner.AddComponent<UIDocument>();doc.panelSettings=panel;var root=doc.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;view=new NativeMainMenuView(root,_=>{},_=>{},_=>{});view.Show(states[phase/2]);composition=new NativeMainMenuComposition(view);composition.SetCharacter("spherist");frames=0;}
  Setup();EditorApplication.CallbackFunction tick=null;
  void Finish(string status){EditorApplication.update-=tick;Release();Application.runInBackground=before;File.WriteAllText(Path.Combine(output,"result.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{result=status,images=phase}));}
  tick=()=>{try{if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup-start>60){Finish("FAIL deadline");return;}if(last==Time.frameCount)return;last=Time.frameCount;if(!composition.Prepare(1,Color.cyan)||++frames<15)return;var prev=RenderTexture.active;var shot=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false,false);try{RenderTexture.active=target;shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,(phase/2==0?"ru":"en")+(phase%2==0?"-portrait":"-landscape")+".png"),shot.EncodeToPNG());}finally{RenderTexture.active=prev;UnityEngine.Object.Destroy(shot);}Release();if(++phase==4){composition=null;view=null;owner=null;panel=null;target=null;Finish("PASS");}else Setup();}catch(Exception e){Finish("FAIL "+e);}};
  EditorApplication.update+=tick;return "Scheduled four native UI previews";
 }
}
