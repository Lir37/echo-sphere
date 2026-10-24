// GPU result versus the existing source CSS paint, without any browser.
UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMainMenuBackground.shader",UnityEditor.ImportAssetOptions.ForceUpdate);
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Editor/EchoSphereReference/SourceUiStyles.json").text));
var parent=new UnityEngine.UIElements.VisualElement();
var background=new EchoSphere.Presentation.NativeMainMenuBackground(parent);
var cases=new[]{new UnityEngine.Vector3(320,240,1),new UnityEngine.Vector3(480,854,1),new UnityEngine.Vector3(1260,542,1),new UnityEngine.Vector3(320,240,1.75f),new UnityEngine.Vector3(480,854,.75f)};
var results=new System.Collections.Generic.List<object>();int index=0;
var originalTarget=UnityEngine.RenderTexture.active;var originalGamma=UnityEngine.GL.sRGBWrite;
UnityEditor.SessionState.SetString("EchoNativeMenuBackground","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 UnityEngine.Texture2D read=null;
 try{
  var item=cases[index];sheet.Width=item.x;sheet.Height=item.y;
  var node=new EchoSphere.Presentation.VisualCssNode {Tag="div",Properties=EchoSphere.Presentation.VisualJs.Obj("className","es-main-menu es-main-menu-legacy"),Width=item.x,Height=item.y};
  sheet.Apply(node);var pseudo=node.Children.Find(child=>child.Pseudo=="::before");
  if(pseudo==null)throw new System.Exception("Source grid pseudo missing");pseudo.Width=item.x;pseudo.Height=item.y;
  var source=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,node,item.z);
  var grid=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,pseudo,item.z);
  var opacity=double.Parse(sheet.Get(pseudo,"opacity","1"),System.Globalization.CultureInfo.InvariantCulture);
  background.SetViewport(item.x,item.y,item.z);var texture=background.Texture;
  if(texture.width!=source.Width||texture.height!=source.Height||grid.Width!=source.Width||grid.Height!=source.Height)throw new System.Exception("Backing pixel dimensions differ");
  var paints=background.PaintCount;var identity=background.Texture;
  for(int i=0;i<120;i++)background.SetViewport(item.x,item.y,item.z);
  if(background.PaintCount!=paints||background.Texture!=identity)throw new System.Exception("Stable viewport repainted/reallocated");
  UnityEngine.RenderTexture.active=texture;
  read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false,false);
  read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();
  var pixels=read.GetPixels32();long total=0;int maximum=0;int worstX=-1,worstY=-1;string worst="";var expected=new UnityEngine.Color32[pixels.Length];
  for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++){
   int at=(y*source.Width+x)*4;int output=(source.Height-1-y)*source.Width+x;double alpha=grid.Pixels[at+3]/255d*opacity;
   double ownAlpha=source.Pixels[at+3]/255d;double combinedAlpha=alpha+ownAlpha*(1-alpha);
   byte r=(byte)System.Math.Round((source.Pixels[at]*ownAlpha*(1-alpha)+grid.Pixels[at]*alpha)/combinedAlpha);
   byte g=(byte)System.Math.Round((source.Pixels[at+1]*ownAlpha*(1-alpha)+grid.Pixels[at+1]*alpha)/combinedAlpha);
   byte b=(byte)System.Math.Round((source.Pixels[at+2]*ownAlpha*(1-alpha)+grid.Pixels[at+2]*alpha)/combinedAlpha);
   byte a=(byte)System.Math.Round(combinedAlpha*255);
   expected[output]=new UnityEngine.Color32(r,g,b,a);var actual=pixels[output];
   foreach(var difference in new[]{System.Math.Abs(r-actual.r),System.Math.Abs(g-actual.g),System.Math.Abs(b-actual.b),System.Math.Abs(a-actual.a)}){total+=difference;if(difference>maximum){maximum=difference;worstX=x;worstY=y;worst="expected="+expected[output]+", actual="+actual+", sourceAlpha="+ownAlpha+", gridAlpha="+alpha;}}
  }
  var mean=total/(pixels.Length*4d);results.Add(new{width=item.x,height=item.y,dpr=item.z,mean,maximum,worstX,worstY,worst,pixels=pixels.Length});
  if(maximum>2||mean>.5)throw new System.Exception("Native background paint differs: "+Newtonsoft.Json.JsonConvert.SerializeObject(results));
  var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-menu-background");System.IO.Directory.CreateDirectory(directory);
  System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"native-"+index+".png"),UnityEngine.ImageConversion.EncodeToPNG(read));
  read.SetPixels32(expected);read.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"source-"+index+".png"),UnityEngine.ImageConversion.EncodeToPNG(read));
  UnityEngine.RenderTexture.active=originalTarget;UnityEngine.Object.DestroyImmediate(read);read=null;
  index++;if(index<cases.Length)return;
  background.Dispose();background.Dispose();if(parent.childCount!=0)throw new System.Exception("Native background retained image after Dispose");
  UnityEditor.EditorApplication.update-=tick;
  UnityEditor.SessionState.SetString("EchoNativeMenuBackground",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS GPU source background; static viewport reuses texture/no repaint; cleanup",results}));
 }catch(System.Exception e){UnityEditor.EditorApplication.update-=tick;UnityEngine.RenderTexture.active=originalTarget;background.Dispose();UnityEditor.SessionState.SetString("EchoNativeMenuBackground","FAIL "+e);}
 finally{UnityEngine.RenderTexture.active=originalTarget;UnityEngine.GL.sRGBWrite=originalGamma;if(read!=null)UnityEngine.Object.DestroyImmediate(read);}
};
UnityEditor.EditorApplication.update+=tick;
return "Started five-viewport native GPU/source CSS background acceptance";
