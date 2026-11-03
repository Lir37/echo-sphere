using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using EchoSphere.Presentation;
public static class MenuGlassCacheAcceptance {
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    [Serializable] public class Case { public int width=540,height=960;public float density=1,safeTop,safeBottom;public bool russian=true,saved;public string id="portrait"; }
    static Case config;static int W=>config.width;static int H=>config.height;static float D=>config.density;static string Folder=>"Library/menu-glass-cache/"+config.id;
    static GameObject[] owners=new GameObject[2];static PanelSettings[] panels=new PanelSettings[2];static RenderTexture[] targets=new RenderTexture[2];
    static NativeMainMenuView[] views=new NativeMainMenuView[2];static NativeMainMenuComposition[] comps=new NativeMainMenuComposition[2];
    static NativeMainMenuView.State state;static int frame,last=-1,bakes,anim,actions,retainedBefore,restoredBakes,fallbackCount;static double started;static RenderTexture lost;
    static int errors;static string firstError;static void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;if(firstError==null)firstError=message;}}
    static readonly List<object> rows=new List<object>();
    static IEnumerable Surfaces()=>(IEnumerable)typeof(NativeMainMenuComposition).GetField("_surfaces",F).GetValue(comps[1]);
    static object Field(object target,string name)=>target.GetType().GetField(name,F).GetValue(target);
    public static object Main(){
        if(!EditorApplication.isPlaying)throw new Exception("Play required");
        config=File.Exists("Library/menu-glass-case.json")?JsonUtility.FromJson<Case>(File.ReadAllText("Library/menu-glass-case.json")):new Case();Directory.CreateDirectory(Folder);
        state=new NativeMainMenuView.State(config.russian,config.russian?"Игрок":"Player",config.russian?"Сферист":"Spherist",config.russian?"Универсал":"Generalist","normal",config.russian?"Сложность":"Difficulty",config.russian?"Имя":"Name",config.russian?"Магазин":"Shop",config.russian?"Рекорды":"Leaderboard",config.russian?"Достижения":"Achievements",config.russian?"Настройки":"Settings",config.saved?(double?)51:null);
        for(int i=0;i<2;i++){
            owners[i]=new GameObject("Menu glass comparison "+i);panels[i]=ScriptableObject.CreateInstance<PanelSettings>();panels[i].scaleMode=PanelScaleMode.ConstantPixelSize;panels[i].scale=D;panels[i].forceGammaRendering=true;panels[i].clearColor=true;panels[i].clearDepthStencil=true;panels[i].colorClearValue=Color.clear;panels[i].themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            targets[i]=new RenderTexture(W,H,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);targets[i].Create();panels[i].targetTexture=targets[i];
            var doc=owners[i].AddComponent<UIDocument>();doc.panelSettings=panels[i];var root=doc.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;
            views[i]=new NativeMainMenuView(root,_=>actions++,_=>actions++,_=>actions++);views[i].SetSafeArea(0,config.safeTop,config.safeBottom);views[i].Show(state);comps[i]=new NativeMainMenuComposition(views[i]);comps[i].SetCharacter("spherist");comps[i].SetStaticGlassPilot(i==1);
        }
        started=EditorApplication.timeSinceStartup;Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;return "RUNNING";
    }
    static void Finish(string result){
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
        for(int i=0;i<2;i++){comps[i]?.Dispose();views[i]?.Dispose();if(panels[i]!=null)panels[i].targetTexture=null;if(owners[i]!=null){owners[i].GetComponent<UIDocument>().enabled=false;UnityEngine.Object.Destroy(owners[i]);}if(targets[i]!=null){targets[i].Release();UnityEngine.Object.Destroy(targets[i]);}UnityEngine.Object.Destroy(panels[i]);}
        File.WriteAllText(Folder+"/result.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{result,frame,actions,errors,firstError,bakes,restoredBakes,fallbackCount,config,rows,scope="Actual native UI live/cache pixels, animated core with stable cache, real model rebind, GPU target loss/recreation, overlap fallback; no gameplay or preferences writes"},Newtonsoft.Json.Formatting.Indented));
    }
    static Color32[] Capture(int i){
        var old=RenderTexture.active;var shot=new Texture2D(W,H,TextureFormat.RGBA32,false);
        try{RenderTexture.active=targets[i];shot.ReadPixels(new Rect(0,0,W,H),0,0);shot.Apply();File.WriteAllBytes(Folder+"/"+(i==0?"live":"retained")+"-"+frame+".png",shot.EncodeToPNG());return shot.GetPixels32();}
        finally{RenderTexture.active=old;UnityEngine.Object.Destroy(shot);}
    }
    static void Compare(){
        var pixels=new[]{Capture(0),Capture(1)};
        foreach(var surface in Surfaces()){
            if(!(bool)Field(surface,"StaticActive"))continue;var e=(VisualElement)Field(surface,"Element");var logical=e.worldBound;var r=new Rect(logical.x*D,logical.y*D,logical.width*D,logical.height*D);long sum=0;int max=0,n=0;
            for(int y=Math.Max(0,(int)r.yMin+2);y<Math.Min(H,(int)r.yMax-2);y++)for(int x=Math.Max(0,(int)r.xMin+2);x<Math.Min(W,(int)r.xMax-2);x++){int index=(H-1-y)*W+x;var a=pixels[0][index];var b=pixels[1][index];int dr=Math.Abs(a.r-b.r),dg=Math.Abs(a.g-b.g),db=Math.Abs(a.b-b.b);sum+=dr+dg+db;max=Math.Max(max,Math.Max(dr,Math.Max(dg,db)));n+=3;}
            double mean=n>0?(double)sum/n:0;rows.Add(new{frame,e.name,mean,max});if(mean>3)throw new Exception("Glass color/quality drift "+e.name+" mean="+mean);
        }
    }
    static void Tick(){try{
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup-started>120)throw new Exception("Timeout/play ended");EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;
        if(!comps[0].Prepare(D,Color.cyan)||!comps[1].Prepare(D,Color.cyan))return;frame++;
        if(frame==30){if(comps[1].RetainedGlassCount<8)throw new Exception("Only "+comps[1].RetainedGlassCount+" surfaces retained");bakes=comps[1].RetainedGlassBakes;anim=comps[1].Artwork.AnimationFrameCount;Compare();}
        if(frame==100){
            if(comps[1].RetainedGlassBakes!=bakes)throw new Exception("Animation invalidated static glass");if(comps[1].Artwork.AnimationFrameCount<=anim+50)throw new Exception("Core animation throttled");if(actions!=0)throw new Exception("Gameplay action invoked");retainedBefore=comps[1].RetainedGlassCount;
            foreach(var surface in Surfaces()){if(!(bool)Field(surface,"StaticActive"))continue;var e=(VisualElement)Field(surface,"Element");if(e is Button){var picked=e.panel.Pick(e.worldBound.center);if(picked!=e&&(picked==null||!e.Contains(picked)))throw new Exception("Glass intercepts button picking: "+e.name);}if(e.resolvedStyle.backdropFilter!=null&&e.resolvedStyle.backdropFilter.Any())throw new Exception("Retained surface still runs live filter");var cache=Field(surface,"StaticGlass");if(lost==null)lost=(RenderTexture)cache.GetType().GetProperty("Texture",F).GetValue(cache);}lost.Release();
        }
        if(frame==105){var s=state;state=new NativeMainMenuView.State(s.Russian,s.Name,s.CharacterName,s.CharacterRole,"hard",s.DifficultyTitle,s.NamePlaceholder,s.ShopTitle,s.LeaderboardTitle,s.AchievementsTitle,s.SettingsTitle,s.SavedRunTime);for(int i=0;i<2;i++){views[i].Show(state);comps[i].Invalidate();}}
        if(frame==110){
            restoredBakes=comps[1].RetainedGlassBakes;if(!lost.IsCreated()||restoredBakes<=bakes)throw new Exception("Lost target not rebuilt");if(restoredBakes>bakes+2)throw new Exception("Foreground-only change invalidated unrelated glass");Compare();
            views[1].Artwork.style.position=Position.Absolute;views[1].Artwork.style.left=views[1].Artwork.style.top=0;views[1].Artwork.style.width=views[1].Root.layout.width;views[1].Artwork.style.height=views[1].Root.layout.height;comps[1].Invalidate();
        }
        if(frame==140){if(errors!=0)throw new Exception("New Console errors: "+firstError);fallbackCount=comps[1].RetainedGlassCount;if(fallbackCount>=retainedBefore)throw new Exception("Animated overlap did not restore live filters");Finish("PASS");}
    }catch(Exception e){Finish("FAIL "+e.GetBaseException().Message);}}
}
