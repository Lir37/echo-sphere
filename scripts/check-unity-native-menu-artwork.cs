// Actual retained Yoga geometry, depth meshes and GPU blur in Play Mode.
// Does not select the view or invoke any game action/preferences.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var owner=new UnityEngine.GameObject("Native menu artwork acceptance");
var panel=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
panel.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.sortingOrder=2200;
panel.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=panel;
var parent=new UnityEngine.UIElements.VisualElement();parent.style.position=UnityEngine.UIElements.Position.Absolute;parent.style.width=155;parent.style.height=155;document.rootVisualElement.Add(parent);
var art=new EchoSphere.Presentation.NativeMenuArtwork(parent);
var frame=UnityEngine.Time.frameCount;var start=UnityEditor.EditorApplication.timeSinceStartup;int phase=0;
var wasPaused=UnityEditor.EditorApplication.isPaused;var background=UnityEngine.Application.runInBackground;
UnityEditor.EditorApplication.isPaused=false;UnityEngine.Application.runInBackground=true;
UnityEditor.SessionState.SetString("EchoNativeMenuArtwork","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Require(bool condition,string reason){if(!condition)throw new System.Exception(reason);}
void Finish(string result){UnityEditor.EditorApplication.update-=tick;art.Dispose();UnityEngine.Object.Destroy(owner);UnityEngine.Object.Destroy(panel);UnityEngine.Application.runInBackground=background;UnityEditor.EditorApplication.isPaused=wasPaused;UnityEditor.SessionState.SetString("EchoNativeMenuArtwork",result);}
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>90){Finish("FAIL Play Mode ended/deadline");return;}
 if(UnityEngine.Time.frameCount-frame<5)return;frame=UnityEngine.Time.frameCount;
 try{
  Require(art.Prepare(phase==2?1.75f:1),"Geometry not ready");Require(art.Texture!=null&&art.Texture.width>155,"Artwork glow bounds missing");
  if(phase==0){
   var texture=art.Texture;int count=art.PaintCount;for(int i=0;i<120;i++)Require(art.Prepare(1),"Cached prepare");
   Require(count==art.PaintCount&&object.ReferenceEquals(texture,art.Texture),"Unchanged artwork repainted/reallocated");
   var previous=UnityEngine.RenderTexture.active;var read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false,false);
   try{UnityEngine.RenderTexture.active=(UnityEngine.RenderTexture)texture;read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();var path=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-menu-artwork.png");System.IO.File.WriteAllBytes(path,UnityEngine.ImageConversion.EncodeToPNG(read));}finally{UnityEngine.RenderTexture.active=previous;UnityEngine.Object.Destroy(read);}
   parent.style.width=210;parent.style.height=210;phase=1;return;
  }
  if(phase==1){Require(art.PaintCount==2,"Resize must repaint once: "+art.PaintCount);Require(art.Prepare(1.75f)&&art.PaintCount==3,"DPR must repaint once");phase=2;return;}
  Require(art.PaintCount==3,"Settled artwork repainted");Finish("PASS native geometry/depth/blur/cache/resize/DPR/disposal; full source composition parity remains separate");
 }catch(System.Exception e){Finish("FAIL "+e);}
};
UnityEditor.EditorApplication.update+=tick;return "Started native menu artwork acceptance";
