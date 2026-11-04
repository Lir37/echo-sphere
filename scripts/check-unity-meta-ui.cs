using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Core;using EchoSphere.Runtime;using EchoSphere.Presentation;
public static class MetaUiAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var type=runtime.GetType();object Call(string n)=>type.GetMethod(n,F).Invoke(runtime,null);var section=type.GetField("_knowledgeSection",F);var query=type.GetField("_knowledgeQuery",F);var page=type.GetField("_mainMenuPage",F);var oldSection=section.GetValue(runtime);var oldQuery=query.GetValue(runtime);var oldPage=page.GetValue(runtime);bool had=PlayerPrefs.HasKey("echosphere_lang");string langBefore=PlayerPrefs.GetString("echosphere_lang");
  var chars=new NativeCharacterScreen.State[2];var archive=new NativeArchiveScreen.State[2];var shops=new NativeProgressionScreen.State[2];var results=new NativeRunResultView.State[2];
  try{for(int language=0;language<2;language++){
   bool ru=language==0;PlayerPrefs.SetString("echosphere_lang",ru?"ru":"en");chars[language]=(NativeCharacterScreen.State)Call("ReadNativeCharacterState");var hero=chars[language].Characters.First(c=>c.Id=="spherist");hero.Expanded=hero.Unlocked=true;chars[language].Characters=new[]{hero};chars[language].Gold=MetaCurrencyRules.Name(CurrencyId.ResonanceShards,ru)+": 2147483647";
   section.SetValue(runtime,"regions");query.SetValue(runtime,"");archive[language]=(NativeArchiveScreen.State)Call("ReadNativeArchiveState");foreach(var card in archive[language].Cards){card.Open=false;if(!string.IsNullOrEmpty(card.ResearchAction))card.CanResearch=true;}archive[language].WalletSummary=string.Join(" · ",new[]{CurrencyId.Credits,CurrencyId.ResonanceShards,CurrencyId.ArchiveData}.Select(id=>MetaCurrencyRules.Name(id,ru)+": 2147483647"));
   page.SetValue(runtime,Enum.Parse(page.FieldType,"Shop"));shops[language]=(NativeProgressionScreen.State)Call("ReadNativeProgressionState");shops[language].Gold="2147483647 "+(ru?"КР":"CR");
   results[language]=(NativeRunResultView.State)Call("ReadNativeRunResultState");results[language].Title=ru?"РЕГИОН СТАБИЛИЗИРОВАН":"REGION STABILIZED";results[language].Clear=true;
   var rows=results[language].Rows.ToList();rows.Add(new NativeRunResultView.Row(ru?"ИСПЫТАНИЯ":"CHALLENGES","3/3"));rows.Add(new NativeRunResultView.Row(ru?"МАСТЕРСТВО":"MASTERY","3/3"));rows.Add(new NativeRunResultView.Row(ru?"БЕСКОНЕЧНЫЙ":"ENDLESS",ru?"ОТКРЫТ":"UNLOCKED"));results[language].Rows=rows;
  }}finally{section.SetValue(runtime,oldSection);query.SetValue(runtime,oldQuery);page.SetValue(runtime,oldPage);if(had)PlayerPrefs.SetString("echosphere_lang",langBefore);else PlayerPrefs.DeleteKey("echosphere_lang");}
  const float scale=2.75f;int checks=0,frame=0,last=-1,stage=-1,loreClicks=0;GameObject go=null;MonoBehaviour screen=null;PanelSettings panel=null;VisualElement root=null;RenderTexture target=null;NativeRunResultView resultView=null;IDisposable backdrop=null;bool ownPanel=false;
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/meta-ui"));Directory.CreateDirectory(dir);double deadline=UnityEditor.EditorApplication.timeSinceStartup+90;UnityEditor.EditorApplication.CallbackFunction tick=null;
  void A(bool v,string why){checks++;if(!v)throw new Exception(why);}
  void Cleanup(){resultView?.Dispose();resultView=null;backdrop?.Dispose();backdrop=null;if(panel!=null)panel.targetTexture=null;if(go!=null)UnityEngine.Object.DestroyImmediate(go);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(ownPanel&&panel!=null)UnityEngine.Object.DestroyImmediate(panel);go=null;screen=null;panel=null;target=null;root=null;ownPanel=false;}
  void Bind(int current){int language=current/4,kind=current%4;
   if(kind==0)((NativeCharacterScreen)screen).Show(chars[language],scale,()=>{},_=>{},_=>{});
   if(kind==1)((NativeArchiveScreen)screen).Show(archive[language],scale,()=>{},_=>{},_=>{},_=>loreClicks++);
   if(kind==2)((NativeProgressionScreen)screen).Show(shops[language],scale,()=>{},_=>{});
   if(kind==3)resultView.Prepare(scale);
   if(screen!=null){var background=screen.GetType().GetField("_background",F).GetValue(screen);background.GetType().GetMethod("SetViewport",F).Invoke(background,new object[]{1080/scale,2400/scale,scale});}
  }
  void Setup(int current){Cleanup();go=new GameObject("Meta UI portrait "+current);int kind=current%4,language=current/4;
   if(kind==0)screen=go.AddComponent<NativeCharacterScreen>();if(kind==1)screen=go.AddComponent<NativeArchiveScreen>();if(kind==2)screen=go.AddComponent<NativeProgressionScreen>();
   if(kind==3){ownPanel=true;panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=scale;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");var doc=go.AddComponent<UIDocument>();doc.panelSettings=panel;root=doc.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.top=0;root.style.backgroundColor=new Color(.02f,.04f,.07f,1);backdrop=(IDisposable)Activator.CreateInstance(type.Assembly.GetType("EchoSphere.Presentation.NativeUiBackdropRegistry"),true);resultView=new NativeRunResultView(root,results[language],(INativeUiBackdrops)backdrop,()=>{});}
   else{Bind(current);panel=(PanelSettings)screen.GetType().GetField("_panel",F).GetValue(screen);root=(VisualElement)screen.GetType().GetField("_root",F).GetValue(screen);}
   target=new RenderTexture(1080,2400,24);target.Create();panel.targetTexture=target;panel.clearColor=panel.clearDepthStencil=true;panel.colorClearValue=Color.black;root.style.width=1080/scale;root.style.height=2400/scale;
  }
  void Capture(string name){var old=RenderTexture.active;var image=new Texture2D(1080,2400,TextureFormat.RGBA32,false);try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1080,2400),0,0);image.Apply();A(image.GetPixels32().Count(p=>p.r>100&&p.g>100)>1000,"actual UI text visible");
    if(name.EndsWith("-focus.png")&&stage%4==1){var caption=root.Query<Label>().ToList().First(l=>l.name==(stage%4==0?"mastery-research-copy":"archive-research-copy")&&l.worldBound.Overlaps(root.worldBound));var rect=caption.worldBound;int visible=0;
     for(int y=Mathf.Max(0,Mathf.FloorToInt((root.worldBound.yMax-rect.yMax)*scale));y<Mathf.Min(2400,Mathf.CeilToInt((root.worldBound.yMax-rect.yMin)*scale));y++)for(int x=Mathf.Max(0,Mathf.FloorToInt(rect.xMin*scale));x<Mathf.Min(1080,Mathf.CeilToInt(rect.xMax*scale));x++)if(image.GetPixel(x,y).g>.4f)visible++;
     A(visible>20,"research text remains visible above its native button paint");}
    File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
  void Finish(string status){UnityEditor.EditorApplication.update-=tick;Cleanup();var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result=status,checks,languages=new[]{"ru","en"},screens=4,scope="Actual portrait1080x2400 UI Toolkit currency labels,research buttons/callbacks,scroll reachability and result bounds;synthetic displayed balances,no purchases or phone writes"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoMetaUi",json);}
  tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("UI timeout");if(Time.frameCount==last)return;last=Time.frameCount;int current=frame/18,local=frame%18;if(current>=8){A(loreClicks==2,"both language lore callbacks executed");Finish("PASS");return;}if(stage!=current){stage=current;Setup(current);}Bind(current);int kind=current%4,language=current/4;
   if(local==10){
    foreach(var label in root.Query<Label>().ToList().Where(l=>l.name=="character-gold"||l.name=="archive-wallet"||l.name=="shop-gold"||l.name=="result-label"||l.name=="result-value"))A(label.worldBound.xMin>=root.worldBound.xMin-1&&label.worldBound.xMax<=root.worldBound.xMax+1,"currency label inside horizontal bounds "+label.name);
    foreach(var scroll in root.Query<ScrollView>().ToList())A(scroll.horizontalScrollerVisibility==ScrollerVisibility.Hidden&&scroll.verticalScrollerVisibility==ScrollerVisibility.Hidden,"gesture-only scroll");
    Capture("view-"+kind+"-"+(language==0?"ru":"en")+"-top.png");
    if(kind==0)A(root.Q("mastery-research-spherist")==null,"XP-only mastery has no purchase button");
    if(kind==1){var scroll=(ScrollView)screen.GetType().GetField("_scroll",F).GetValue(screen);scroll.ScrollTo(root.Query<Button>().ToList().First(b=>b.name.StartsWith("archive-research-")));}
   }
   if(local==15){
    if(kind==1){var button=root.Query<Button>().ToList().First(b=>b.name.StartsWith("archive-research-"));var scroll=(ScrollView)screen.GetType().GetField("_scroll",F).GetValue(screen);A(button.worldBound.Overlaps(scroll.contentViewport.worldBound),"research reachable by vertical scroll");A(button.worldBound.xMin>=root.worldBound.xMin&&button.worldBound.xMax<=root.worldBound.xMax,"research inside horizontal bounds");using(var evt=NavigationSubmitEvent.GetPooled()){evt.target=button;button.SendEvent(evt);}}
    if(kind==3){foreach(var row in root.Query<VisualElement>().ToList().Where(e=>e.name.StartsWith("result-row-"))){var label=row.Q<Label>("result-label");var value=row.Q<Label>("result-value");A(label.worldBound.xMax+6<=value.worldBound.xMin,"result label/value separation");}A(root.Q<Label>("result-title").worldBound.height<=72,"clear title stays on two lines");}
    if(kind==3)A(root.Q("result-return").worldBound.yMax<=root.worldBound.yMax&&root.Q("native-result-content").worldBound.yMin>=root.worldBound.yMin,"extended result fits portrait");
    Capture("view-"+kind+"-"+(language==0?"ru":"en")+"-focus.png");
   }frame++;
  }catch(Exception e){Finish("FAIL "+e);}};UnityEditor.SessionState.SetString("EchoMetaUi","RUNNING");UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
