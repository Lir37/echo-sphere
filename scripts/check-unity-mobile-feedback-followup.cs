using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using EchoSphere.Presentation;
public static class MobileFeedbackFollowup {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  // Weapon acceptance now lives in check-unity-mechanical-weapons.cs (retained articulated rigs).
  var go=new GameObject("Portrait pause typography fixture");var panel=ScriptableObject.CreateInstance<PanelSettings>();var texture=new RenderTexture(960,1440,24);texture.Create();panel.targetTexture=texture;panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.sortingOrder=2400;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
  var document=go.AddComponent<UIDocument>();document.panelSettings=panel;var parent=document.rootVisualElement;parent.style.width=390;parent.style.height=800;
  var screen=new NativePauseScreenView(parent,700);screen.Body.Add(new VisualElement{style={height=1000}});
  var chrome=new NativePauseChromeView(screen.Header,screen.Footer,()=>{},()=>{},_=>{});chrome.UseFlowLayout(true);
  var cases=new[]{(292,true),(320,true),(360,true),(390,true),(320,false),(640,false)};int stage=0,frames=0,last=-1;var rows=new List<object>();double deadline=UnityEditor.EditorApplication.timeSinceStartup+100;bool background=Application.runInBackground;Application.runInBackground=true;
  void Bind(){var c=cases[stage];parent.style.width=c.Item1;chrome.Bind(new NativePauseChromeState{Eyebrow=c.Item2?"БИЛД ЗАБЕГА":"RUN BUILD",Title=c.Item2?"Пауза · Кондуктор":"Pause · Conductor",Subtitle=c.Item2?"Сетевой резонанс · уровень 35 · Волна 30":"Network resonance · level 35 · Wave 30",Tabs=c.Item2?new[]{"Показатели","Способности","Артефакты","Синергии"}:new[]{"Stats","Abilities","Artifacts","Synergies"},Resume=c.Item2?"Продолжить":"Resume",Return=c.Item2?"Вернуться в меню":"Return to menu",SelectedTab=0});frames=0;}
  UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;chrome.Dispose();screen.Dispose();panel.targetTexture=null;UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(panel);texture.Release();UnityEngine.Object.Destroy(texture);Application.runInBackground=background;var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,rows,scope="Portrait pause labels and footer at 320/360/390/640, ru/en. No physical vibration acceptance."});System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/mobile-feedback-followup.json"),json);UnityEditor.SessionState.SetString("EchoMobileFollowup",json);}
  Bind();tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Play/deadline");if(last==Time.frameCount)return;last=Time.frameCount;chrome.Prepare();screen.Prepare(800,chrome.Ready);if(++frames<15)return;
   foreach(var label in chrome.Header.Query<Label>().ToList().Concat(chrome.Footer.Query<Label>().ToList())){
    Button button=null;for(var p=label.parent;p!=null;p=p.parent)if(p is Button b){button=b;break;}
    var bounds=label.worldBound;if(button!=null)Check(bounds.xMin>=button.worldBound.xMin-.5&&bounds.xMax<=button.worldBound.xMax+.5&&bounds.yMin>=button.worldBound.yMin-.5&&bounds.yMax<=button.worldBound.yMax+.5,"Label outside button: "+label.name+" width="+cases[stage].Item1);
    if(button!=null&&label.resolvedStyle.whiteSpace==WhiteSpace.NoWrap){var full=label.MeasureTextSize(label.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);Check(full.x<=label.contentRect.width+1,"Full word does not fit: "+label.name);}
    if(label.resolvedStyle.whiteSpace==WhiteSpace.Normal){var needed=label.MeasureTextSize(label.text,label.contentRect.width,VisualElement.MeasureMode.AtMost,0,VisualElement.MeasureMode.Undefined);Check(needed.y<=label.contentRect.height+1,"Wrapped text height clipped: "+label.name);}
   }
   Check(screen.Footer.worldBound.yMax<=screen.Root.worldBound.yMax+1,"Footer outside pause shell");
   rows.Add(new{width=cases[stage].Item1,russian=cases[stage].Item2,headerHeight=screen.Header.layout.height,footerHeight=screen.Footer.layout.height});
   if(++stage==cases.Length){Finish("PASS");return;}Bind();
  }catch(Exception error){Finish("FAIL "+error);}};
  UnityEditor.SessionState.SetString("EchoMobileFollowup","RUNNING");UnityEditor.EditorApplication.update+=tick;return new{result="Started",checks};
 }
}
