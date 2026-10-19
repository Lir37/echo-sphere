// Compare cropped native panels against the established GPU backdrop, including
// a texture changed every rendered frame. Uses no browser or gameplay actions.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var assembly=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly;
var glassType=assembly.GetType("EchoSphere.Presentation.NativeMenuGlass");
var referenceType=assembly.GetType("EchoSphere.Presentation.SourceUiBackdrop");
var objects=new System.Collections.Generic.List<UnityEngine.Object>();
void Box(UnityEngine.UIElements.VisualElement e,float x,float y,float w,float h){e.style.position=UnityEngine.UIElements.Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=w;e.style.height=h;e.style.flexShrink=0;}
UnityEngine.RenderTexture Target(){var t=new UnityEngine.RenderTexture(256,256,24,UnityEngine.RenderTextureFormat.ARGB32,UnityEngine.RenderTextureReadWrite.sRGB);t.filterMode=UnityEngine.FilterMode.Point;t.Create();objects.Add(t);return t;}
var referenceTarget=Target();var candidateTarget=Target();
UnityEngine.UIElements.VisualElement Panel(int order,UnityEngine.RenderTexture target){
 var settings=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();objects.Add(settings);settings.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.sortingOrder=order;settings.forceGammaRendering=true;settings.clearColor=true;settings.colorClearValue=UnityEngine.Color.clear;settings.targetTexture=target;settings.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
 var owner=new UnityEngine.GameObject("Native glass acceptance "+order);objects.Add(owner);var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=settings;var root=document.rootVisualElement;Box(root,0,0,256,256);return root;
}
var referenceRoot=Panel(2100,referenceTarget);var candidateRoot=Panel(2200,candidateTarget);
var pattern=new UnityEngine.Texture2D(256,256,UnityEngine.TextureFormat.RGBA32,false,false){filterMode=UnityEngine.FilterMode.Point};objects.Add(pattern);
void Background(UnityEngine.UIElements.VisualElement root){var image=new UnityEngine.UIElements.Image{image=pattern,scaleMode=UnityEngine.ScaleMode.StretchToFill};Box(image,0,0,256,256);root.Add(image);}
Background(referenceRoot);Background(candidateRoot);
void Text(UnityEngine.UIElements.VisualElement root){var label=new UnityEngine.UIElements.Label("ECHO native prefix");Box(label,60,80,140,28);label.style.fontSize=12;label.style.color=UnityEngine.Color.white;root.Add(label);}
Text(referenceRoot);Text(candidateRoot);
UnityEngine.UIElements.VisualElement Surface(UnityEngine.UIElements.VisualElement root){var e=new UnityEngine.UIElements.VisualElement();Box(e,48,64,160,120);e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=12;root.Add(e);return e;}
var referenceElement=Surface(referenceRoot);var candidateElement=Surface(candidateRoot);
var paint=new System.Func<UnityEngine.UIElements.VisualElement,System.Action<UnityEngine.UIElements.MeshGenerationContext>>(e=>null);
var reference=System.Activator.CreateInstance(referenceType,flags,null,new object[]{referenceElement,(System.Action<UnityEngine.UIElements.MeshGenerationContext>)(c=>{}),paint,null},null);
var candidate=System.Activator.CreateInstance(glassType,flags,null,new object[]{candidateElement,8f},null);
var readA=new UnityEngine.Texture2D(256,256,UnityEngine.TextureFormat.RGBA32,false,false);var readB=new UnityEngine.Texture2D(256,256,UnityEngine.TextureFormat.RGBA32,false,false);objects.Add(readA);objects.Add(readB);
var pixels=new UnityEngine.Color32[256*256];
void Pattern(int phase){for(int y=0;y<256;y++)for(int x=0;x<256;x++)pixels[y*256+x]=((x/16+y/16+phase)&1)!=0?new UnityEngine.Color32(22,68,118,255):new UnityEngine.Color32(111,32,71,255);pattern.SetPixels32(pixels);pattern.Apply(false,false);}
UnityEngine.Color32[] Read(UnityEngine.RenderTexture target,UnityEngine.Texture2D texture){var previous=UnityEngine.RenderTexture.active;try{UnityEngine.RenderTexture.active=target;texture.ReadPixels(new UnityEngine.Rect(0,0,256,256),0,0,false);texture.Apply(false,false);return texture.GetPixels32();}finally{UnityEngine.RenderTexture.active=previous;}}
var paused=UnityEditor.EditorApplication.isPaused;var background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
var start=UnityEditor.EditorApplication.timeSinceStartup;int last=UnityEngine.Time.frameCount,frames=0,samples=0,maximum=0;double worstMean=0;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){UnityEditor.EditorApplication.update-=tick;var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-glass-text-acceptance");System.IO.Directory.CreateDirectory(directory);System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"reference.png"),UnityEngine.ImageConversion.EncodeToPNG(readA));System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"candidate.png"),UnityEngine.ImageConversion.EncodeToPNG(readB));System.IO.File.WriteAllText(System.IO.Path.Combine(directory,"result.json"),result);((System.IDisposable)candidate).Dispose();((System.IDisposable)reference).Dispose();foreach(var o in objects){if(o is UnityEngine.GameObject owner)owner.GetComponent<UnityEngine.UIElements.UIDocument>().enabled=false;UnityEngine.Object.Destroy(o);}UnityEngine.Application.runInBackground=background;UnityEditor.EditorApplication.isPaused=paused;UnityEditor.SessionState.SetString("EchoNativeMenuGlassText",result);}
UnityEditor.SessionState.SetString("EchoNativeMenuGlassText","RUNNING");Pattern(0);
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>120){Finish("FAIL Play ended/deadline");return;}if(UnityEngine.Time.frameCount==last)return;last=UnityEngine.Time.frameCount;
 try{
  if(frames++>12){var a=Read(referenceTarget,readA);var b=Read(candidateTarget,readB);long total=0;int max=0;for(int i=0;i<a.Length;i++){int[] delta={System.Math.Abs(a[i].r-b[i].r),System.Math.Abs(a[i].g-b[i].g),System.Math.Abs(a[i].b-b[i].b),System.Math.Abs(a[i].a-b[i].a)};foreach(var d in delta){total+=d;max=System.Math.Max(max,d);}}maximum=System.Math.Max(maximum,max);worstMean=System.Math.Max(worstMean,total/(double)(a.Length*4));samples++;if(samples==32){Finish(Newtonsoft.Json.JsonConvert.SerializeObject(new{status=maximum<=1&&worstMean<=.01?"PASS":"FAIL GPU pixel parity",samples,maximum,worstMean,scope="256x256; overlapping native text + changing image; complete retained native prefix"}));return;}}
  Pattern(frames);referenceType.GetMethod("Prepare",flags).Invoke(reference,new object[]{referenceRoot,8f});glassType.GetMethod("Prepare",flags).Invoke(candidate,new object[]{candidateRoot,8f,1f});
 }catch(System.Exception e){Finish("FAIL "+e);}
};UnityEditor.EditorApplication.update+=tick;return "Started cropped native glass GPU pixel/phase acceptance";
