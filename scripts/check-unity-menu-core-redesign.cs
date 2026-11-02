using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using EchoSphere.Presentation;

public static class MenuCoreRedesignCheck
{
    public static object Main()
    {
        if(!EditorApplication.isPlaying)throw new Exception("Play Mode required");
        var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
        if(runtime==null)throw new Exception("Runtime missing");
        string directory=Path.GetFullPath("Library/menu-core-redesign");Directory.CreateDirectory(directory);
        var state=runtime.ReadNativeMainMenuState();
        bool background=Application.runInBackground;Application.runInBackground=true;
        GameObject owner=null;PanelSettings panel=null;RenderTexture target=null;
        NativeMainMenuView view=null;NativeMainMenuComposition composition=null;
        int phase=0,frame=-1,settled=0,lastPaint=-1,actions=0,recorded=0;double start=EditorApplication.timeSinceStartup,nextCapture=0;
        string firstFrameHash=null,lastFrameHash=null;Texture retained=null;int retainedPaints=0,retainedCompositionPaints=0;
        var results=new System.Collections.Generic.List<object>();
        void Release(){composition?.Dispose();composition=null;view?.Dispose();view=null;if(panel!=null)panel.targetTexture=null;if(owner!=null)UnityEngine.Object.Destroy(owner);if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}if(panel!=null)UnityEngine.Object.Destroy(panel);}
        void Setup(int width,int height){
            owner=new GameObject("Menu core redesign preview");panel=ScriptableObject.CreateInstance<PanelSettings>();
            panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.forceGammaRendering=true;
            panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();panel.targetTexture=target;
            var document=owner.AddComponent<UIDocument>();document.panelSettings=panel;
            var root=document.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;
            view=new NativeMainMenuView(root,_=>actions++,_=>actions++,_=>actions++);view.Show(state);
            composition=new NativeMainMenuComposition(view);composition.SetCharacter("spherist");settled=0;lastPaint=-1;
        }
        Setup(540,960);
        EditorApplication.CallbackFunction tick=null;
        void Finish(string result){EditorApplication.update-=tick;Release();Application.runInBackground=background;SessionState.SetString("EchoMenuCoreRedesign",result);File.WriteAllText(Path.Combine(directory,"result.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{result,actions,results},Newtonsoft.Json.Formatting.Indented));}
        tick=()=>{
            if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup-start>90){Finish("FAIL deadline/play ended");return;}
            if(frame==Time.frameCount)return;frame=Time.frameCount;
            try{
                if(!composition.Prepare(1,Color.cyan))return;
                if(lastPaint==composition.PrepareCount)settled++;else settled=0;lastPaint=composition.PrepareCount;if(settled<5)return;
                var art=composition.Artwork;int paints=art.PaintCount;var texture=art.Texture;
                for(int i=0;i<120;i++)if(!art.Prepare(1))throw new Exception("Core not ready");
                if(art.PaintCount!=paints||!ReferenceEquals(texture,art.Texture))throw new Exception("Core target reallocated/layout repainted");
                if(actions!=0)throw new Exception("Preview invoked game action");
                var box=view.Artwork.worldBound;if(Mathf.Abs(box.width-box.height)>.1f||box.xMin<0||box.xMax>target.width)throw new Exception("Core geometry exceeds viewport");
                if(phase==0&&recorded<60){
                    if(EditorApplication.timeSinceStartup<nextCapture)return;nextCapture=EditorApplication.timeSinceStartup+1.0/15;
                    if(retained==null){retained=art.Texture;retainedPaints=art.PaintCount;retainedCompositionPaints=composition.PrepareCount;}
                    if(!ReferenceEquals(retained,art.Texture)||retainedPaints!=art.PaintCount)throw new Exception("Animation reallocated/rebuilt core");
                    if(composition.PrepareCount!=retainedCompositionPaints)throw new Exception("Core animation invalidated menu glass");
                    var crop=new Rect(Mathf.Floor(box.xMin),target.height-Mathf.Ceil(box.yMax),Mathf.Ceil(box.width),Mathf.Ceil(box.height));
                    var old=RenderTexture.active;var shot=new Texture2D((int)crop.width,(int)crop.height,TextureFormat.RGBA32,false,false);
                    try{RenderTexture.active=target;shot.ReadPixels(crop,0,0);shot.Apply();var bytes=shot.EncodeToPNG();var frames=Path.Combine(directory,"frames");Directory.CreateDirectory(frames);File.WriteAllBytes(Path.Combine(frames,recorded.ToString("D3")+".png"),bytes);lastFrameHash=Hash128.Compute(bytes).ToString();if(recorded==0)firstFrameHash=lastFrameHash;recorded++;}
                    finally{RenderTexture.active=old;UnityEngine.Object.Destroy(shot);}
                    return;
                }
                if(phase==0&&firstFrameHash==lastFrameHash)throw new Exception("Animation did not change pixels");
                var previous=RenderTexture.active;var read=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false,false);
                try{RenderTexture.active=target;read.ReadPixels(new Rect(0,0,target.width,target.height),0,0);read.Apply();File.WriteAllBytes(Path.Combine(directory,phase==0?"portrait.png":"landscape.png"),read.EncodeToPNG());}
                finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(read);}
                results.Add(new{width=target.width,height=target.height,core=box.ToString(),corePaints=paints,compositionPrepares=composition.PrepareCount,animationFrames=art.AnimationFrameCount,cachedPrepares=120,recordedFrames=phase==0?recorded:0});
                Release();if(phase++==0){Setup(960,540);return;}Finish("PASS");
            }catch(Exception error){Finish("FAIL "+error);}
        };
        EditorApplication.update+=tick;SessionState.SetString("EchoMenuCoreRedesign","RUNNING");return "Started portrait/landscape native menu preview and cache checks";
    }
}
