// Existing frozen pixel fixtures only. No browser launch/export or game writes.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var dataDirectory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-glass-goldens");
string json=System.IO.File.ReadAllText(System.IO.Path.Combine(dataDirectory,"metadata.json"));
var fixtures=Newtonsoft.Json.Linq.JArray.Parse(json);
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var type=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly.GetType("EchoSphere.Presentation.NativeMenuGlass");
var objects=new System.Collections.Generic.List<UnityEngine.Object>();object glass=null;UnityEngine.UIElements.VisualElement root=null;
UnityEngine.RenderTexture target=null;UnityEngine.Texture2D read=null;float dpr=1,sigma=8,filterDpr=0;bool passed=true;int index=0,frame=UnityEngine.Time.frameCount,settled=0;var results=new System.Collections.Generic.List<object>();
void Box(UnityEngine.UIElements.VisualElement e,float x,float y,float w,float h){e.style.position=UnityEngine.UIElements.Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=w;e.style.height=h;e.style.flexShrink=0;}
void Clear(){if(glass!=null){((System.IDisposable)glass).Dispose();glass=null;}foreach(var item in objects){if(item is UnityEngine.GameObject owner)owner.GetComponent<UnityEngine.UIElements.UIDocument>().enabled=false;UnityEngine.Object.Destroy(item);}objects.Clear();}
void Begin(){
 var fixture=fixtures[index];dpr=(float)fixture["dpr"];sigma=(float)fixture["sigma"];int width=(int)fixture["sourceWidth"],height=(int)fixture["sourceHeight"];
 target=new UnityEngine.RenderTexture(width,height,24,UnityEngine.RenderTextureFormat.ARGB32,UnityEngine.RenderTextureReadWrite.sRGB){filterMode=UnityEngine.FilterMode.Point};target.Create();objects.Add(target);
 var settings=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();objects.Add(settings);settings.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;settings.scale=dpr;settings.sortingOrder=2200;settings.forceGammaRendering=true;settings.clearColor=true;settings.colorClearValue=UnityEngine.Color.clear;settings.targetTexture=target;settings.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
 var owner=new UnityEngine.GameObject("Native glass golden acceptance");objects.Add(owner);var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=settings;root=document.rootVisualElement;Box(root,0,0,width/dpr,height/dpr);
 var texture=new UnityEngine.Texture2D(width,height,UnityEngine.TextureFormat.RGBA32,false,false){filterMode=UnityEngine.FilterMode.Point};objects.Add(texture);var bytes=System.IO.File.ReadAllBytes(System.IO.Path.Combine(dataDirectory,index+"-source.rgba"));var pixels=new UnityEngine.Color32[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++){int at=(y*width+x)*4;pixels[(height-y-1)*width+x]=new UnityEngine.Color32(bytes[at],bytes[at+1],bytes[at+2],bytes[at+3]);}texture.SetPixels32(pixels);texture.Apply(false,false);
 var image=new UnityEngine.UIElements.Image{image=texture,scaleMode=UnityEngine.ScaleMode.StretchToFill};Box(image,0,0,width/dpr,height/dpr);root.Add(image);
 var element=new UnityEngine.UIElements.VisualElement();Box(element,(float)fixture["x"]/dpr,(float)fixture["y"]/dpr,(float)fixture["width"]/dpr,(float)fixture["height"]/dpr);root.Add(element);
 var paint=new System.Func<UnityEngine.UIElements.VisualElement,System.Action<UnityEngine.UIElements.MeshGenerationContext>>(e=>null);glass=System.Activator.CreateInstance(type,flags,null,new object[]{element,sigma},null);
 filterDpr=dpr;
 read=new UnityEngine.Texture2D(width,height,UnityEngine.TextureFormat.RGBA32,false,false);objects.Add(read);settled=0;
}
var paused=UnityEditor.EditorApplication.isPaused;var background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;var start=UnityEditor.EditorApplication.timeSinceStartup;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){UnityEditor.EditorApplication.update-=tick;Clear();UnityEngine.Application.runInBackground=background;UnityEditor.EditorApplication.isPaused=paused;UnityEditor.SessionState.SetString("EchoNativeMenuGlassGoldens",result);var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-glass-acceptance");System.IO.Directory.CreateDirectory(directory);System.IO.File.WriteAllText(System.IO.Path.Combine(directory,"goldens.json"),result);}
UnityEditor.SessionState.SetString("EchoNativeMenuGlassGoldens","RUNNING");Begin();
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>240){Finish("FAIL Play ended/deadline "+index);return;}if(UnityEngine.Time.frameCount==frame)return;frame=UnityEngine.Time.frameCount;
 try{
  type.GetMethod("Prepare",flags).Invoke(glass,new object[]{root,sigma,dpr});if(settled++<8)return;
  var previous=UnityEngine.RenderTexture.active;try{UnityEngine.RenderTexture.active=target;read.ReadPixels(new UnityEngine.Rect(0,0,target.width,target.height),0,0,false);read.Apply(false,false);}finally{UnityEngine.RenderTexture.active=previous;}
  var actual=read.GetPixels32();var fixture=fixtures[index];var expected=System.IO.File.ReadAllBytes(System.IO.Path.Combine(dataDirectory,index+"-expected.rgba"));int width=(int)fixture["width"],height=(int)fixture["height"],left=(int)fixture["x"],top=(int)fixture["y"],maximum=0;long total=0;
  for(int y=0;y<height;y++)for(int x=0;x<width;x++){var color=actual[(target.height-top-y-1)*target.width+left+x];int at=(y*width+x)*4;int[] channels={color.r,color.g,color.b,color.a};for(int c=0;c<4;c++){var value=c==3?(int)expected[at+c]:(int)System.Math.Round((int)expected[at+c]*(int)expected[at+3]/255d);int delta=System.Math.Abs(channels[c]-value);maximum=System.Math.Max(maximum,delta);total+=delta;}}
  double mean=total/(double)(width*height*4);passed&=mean<2&&maximum<10;var corner=actual[(target.height-top-1)*target.width+left];results.Add(new{index,dpr,filterDpr,sigma,transparent=(bool)fixture["transparent"],maximum,mean,actualCorner=corner.ToString(),expectedCorner=expected[0]+","+expected[1]+","+expected[2]+","+expected[3]});
  if(index==8){var dir=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-glass-acceptance");System.IO.Directory.CreateDirectory(dir);System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir,"dpr175.png"),UnityEngine.ImageConversion.EncodeToPNG(read));}
  Clear();index++;if(index<fixtures.Count){Begin();return;}
  Finish(Newtonsoft.Json.JsonConvert.SerializeObject(new{status=passed?"PASS 24 native GPU frozen pixel fixtures":"FAIL native GPU pixels",gate="Existing backdrop pixel gate: mean < 2/255, max < 10/255; premultiplied RGBA comparison",results},Newtonsoft.Json.Formatting.Indented));
 }catch(System.Exception e){Finish("FAIL "+e);}
};UnityEditor.EditorApplication.update+=tick;return "Started 24 existing pixel goldens through native cropped GPU glass";
