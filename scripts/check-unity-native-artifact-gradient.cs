UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMenuBrush.shader",UnityEditor.ImportAssetOptions.ForceUpdate);
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse(UnityEngine.Resources.Load<UnityEngine.TextAsset>("EchoSphere/SourceUiStyles").text)){Width=1260,Height=542};
var cases=new[]{("a145",54f,54f,1f),("a145",54f,54f,2f),("a0",83f,37f,1f),("a90",83f,37f,1.75f),("a225",83f,37f,1f)};
var results=new System.Collections.Generic.List<object>();var brush=new EchoSphere.Presentation.NativeMenuBrush();int index=0;
UnityEditor.SessionState.SetString("EchoNativeArtifactGradient","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 UnityEngine.Texture2D read=null;var previous=UnityEngine.RenderTexture.active;
 try{
  var item=cases[index];var name=item.Item1;float angle=float.Parse(name.Substring(1),System.Globalization.CultureInfo.InvariantCulture);var root=new EchoSphere.Presentation.VisualCssNode {Tag="div",Width=200,Height=200};var node=new EchoSphere.Presentation.VisualCssNode {Tag="div",Parent=root,Width=item.Item2,Height=item.Item3,Properties=EchoSphere.Presentation.VisualJs.Obj("style",EchoSphere.Presentation.VisualJs.Obj("background-image","linear-gradient("+angle.ToString(System.Globalization.CultureInfo.InvariantCulture)+"deg, rgba(15,30,48,.95), rgba(2,8,15,.98))"))};root.Children.Add(node);sheet.Apply(root);var paint=new EchoSphere.Presentation.NativeMenuBrush.Paint {LinearAngle=angle,Colors=new[]{new UnityEngine.Color(15/255f,30/255f,48/255f,.95f),new UnityEngine.Color(2/255f,8/255f,15/255f,.98f)},Stops=new[]{0f,1f}};  var expected=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,node,item.Item4);brush.Render(item.Item2,item.Item3,item.Item4,paint);var texture=brush.Texture;
  if(texture.width!=expected.Width||texture.height!=expected.Height)throw new System.Exception("Brush bounds differ "+name+": "+texture.width+"x"+texture.height+" vs "+expected.Width+"x"+expected.Height);
  read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false,false);UnityEngine.RenderTexture.active=texture;read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();var pixels=read.GetPixels32();
  double total=0,maximum=0;int visibleRaw=0,worstX=-1,worstY=-1;string worst="";
  for(int y=0;y<expected.Height;y++)for(int x=0;x<expected.Width;x++){
   int at=(y*expected.Width+x)*4;var actual=pixels[(expected.Height-1-y)*expected.Width+x];var e=expected.Pixels;
   var diffs=new[]{System.Math.Abs(e[at]*e[at+3]/255d-actual.r*actual.a/255d),System.Math.Abs(e[at+1]*e[at+3]/255d-actual.g*actual.a/255d),System.Math.Abs(e[at+2]*e[at+3]/255d-actual.b*actual.a/255d),System.Math.Abs(e[at+3]-actual.a)};
   foreach(var difference in diffs){total+=difference;if(difference>maximum){maximum=difference;worstX=x;worstY=y;worst="expected="+e[at]+","+e[at+1]+","+e[at+2]+","+e[at+3]+" actual="+actual;}}
   if(e[at+3]>=128||actual.a>=128)visibleRaw=System.Math.Max(visibleRaw,System.Math.Max(System.Math.Abs(e[at]-actual.r),System.Math.Max(System.Math.Abs(e[at+1]-actual.g),System.Math.Abs(e[at+2]-actual.b))));
  }
  var mean=total/(pixels.Length*4d);results.Add(new{name,width=item.Item2,height=item.Item3,dpr=item.Item4,maximum,mean,visibleRaw,worstX,worstY,worst});
  var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-artifact-gradient");System.IO.Directory.CreateDirectory(directory);System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,index+"-"+name+".png"),UnityEngine.ImageConversion.EncodeToPNG(read));
  if(maximum>2||mean>.1||visibleRaw>2)throw new System.Exception("GPU/source brush mismatch "+Newtonsoft.Json.JsonConvert.SerializeObject(results));
  UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(read);read=null;index++;
  if(index<cases.Length)return;
  UnityEditor.EditorApplication.update-=tick;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeArtifactGradient",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS bounded native artwork brush GPU/source paint",results}));
 }catch(System.Exception e){UnityEditor.EditorApplication.update-=tick;UnityEngine.RenderTexture.active=previous;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeArtifactGradient","FAIL "+e);}
 finally{UnityEngine.RenderTexture.active=previous;if(read!=null)UnityEngine.Object.DestroyImmediate(read);}
};
UnityEditor.EditorApplication.update+=tick;return "Started five native arbitrary-angle/source gradient cases";
