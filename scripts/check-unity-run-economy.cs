using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using EchoSphere.Core;
public static class RunEconomyAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();
  var go=new GameObject("Merchant UI acceptance");var panel=ScriptableObject.CreateInstance<PanelSettings>();
  var target=new RenderTexture(1080,2400,24);target.Create();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=2.75f;panel.forceGammaRendering=true;panel.targetTexture=target;panel.clearColor=true;panel.clearDepthStencil=true;panel.colorClearValue=Color.black;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
  var doc=go.AddComponent<UIDocument>();doc.panelSettings=panel;var root=doc.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;
  var state=RunEconomyRules.Create(123);var owned=new System.Collections.Generic.List<string>();RunEconomyRules.AdvanceMerchant(state,390);RunEconomyRules.Prepare(state,owned);state.Open=true;state.Echo=500;
  var viewType=type.Assembly.GetType("EchoSphere.Presentation.NativeMerchantView");int clicks=0,checks=0;Action<int> buy=i=>{if(RunEconomyRules.TryBuy(state,i,owned))clicks++;};
  var view=Activator.CreateInstance(viewType,F,null,new object[]{root,(Action)(()=>state.Open=true),(Action)(()=>RunEconomyRules.Close(state)),buy},null);
  void Bind(bool ru)=>viewType.GetMethod("Bind",F).Invoke(view,new object[]{state,owned,ru,430d,true,false,false});
  Bind(true);int frame=0,last=-1;double deadline=UnityEditor.EditorApplication.timeSinceStartup+30;UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
  var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/run-economy"));Directory.CreateDirectory(dir);
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;UnityEngine.Object.Destroy(go);target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(panel);var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,offers=8,languages=new[]{"ru","en"},scope="Actual retained UI Toolkit render and pure purchase; no PlayerPrefs writes"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoRunEconomyAcceptance",json);}
  UnityEditor.SessionState.SetString("EchoRunEconomyAcceptance","RUNNING");
  tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out");frame++;
   if(frame==12){
    var overlay=root.Q("merchant-overlay");Check(overlay.worldBound.width>300&&overlay.worldBound.height>700,"Portrait geometry");
    Check(root.Query<Button>().ToList().Count==10,"Eight buy buttons, wallet and close");
    var card=root.Query<Label>().ToList().First(x=>x.text.Contains("ТОРГОВЕЦ"));Check(card.worldBound.width<root.worldBound.width,"Title fits");
    var previous=RenderTexture.active;var image=new Texture2D(1080,2400,TextureFormat.RGBA32,false);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1080,2400),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,"merchant-ru.png"),image.EncodeToPNG());RenderTexture.active=previous;UnityEngine.Object.Destroy(image);
    int before=state.Echo;buy(0);Check(clicks==1&&state.Echo<before,"Purchase accepted");int after=state.Echo;buy(0);Check(clicks==1&&state.Echo==after,"Repeat purchase rejected");Bind(false);
   }
   if(frame==22){Check(root.Query<Label>().ToList().Any(x=>x.text=="ECHO MERCHANT"),"English UI");Check(root.Query<Button>().ToList().Any(x=>x.text=="Owned"),"Sold row refreshed");RunEconomyRules.Close(state);Bind(false);Check(root.Q("merchant-overlay").style.display==DisplayStyle.None,"Closed overlay hidden");Finish("PASS");}
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
