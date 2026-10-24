UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMenuBrush.shader",UnityEditor.ImportAssetOptions.ForceUpdate);
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Editor/EchoSphereReference/SourceUiStyles.json").text)){Width=1260,Height=542};
var cases=new[]{("body",68.2f,68.2f,1f),("ring",127f,127f,1f),("dot",7f,7f,1f),("highlight",37f,28f,1f),("avatar",29f,29f,1f),("body",68.2f,68.2f,.75f),("avatar",29f,29f,1.75f),("ring",127f,127f,1.75f),("ring-b",87f,87f,1f),("ring-c",59f,59f,1f),("ring-d",109f,109f,1f),("ring-e",35f,35f,1f)};
var results=new System.Collections.Generic.List<object>();var brush=new EchoSphere.Presentation.NativeMenuBrush();int index=0;
UnityEditor.SessionState.SetString("EchoNativeMenuBrushes","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 UnityEngine.Texture2D read=null;var previous=UnityEngine.RenderTexture.active;
 try{
  var item=cases[index];var root=new EchoSphere.Presentation.VisualCssNode {Tag="div",Properties=EchoSphere.Presentation.VisualJs.Obj("className","es-main-menu es-main-menu-legacy"),Width=155,Height=155};
  var name=item.Item1;int ringIndex=name=="ring-b"?1:name=="ring-c"?2:name=="ring-d"?3:name=="ring-e"?4:0;var ringClass=new[]{"es-core-orbit orbit-a","es-core-orbit orbit-b","es-core-orbit orbit-c","es-core-ring ring-a","es-core-ring ring-b"}[ringIndex];var node=new EchoSphere.Presentation.VisualCssNode {Tag="div",Parent=root,Width=item.Item2,Height=item.Item3,Properties=EchoSphere.Presentation.VisualJs.Obj("className",name=="body"?"es-core-body":name.StartsWith("ring")||name=="dot"?ringClass:name=="highlight"?"es-core-highlight":"es-loadout-avatar","style",EchoSphere.Presentation.VisualJs.Obj("--character-color","#55dfff"))};root.Children.Add(node);sheet.Apply(root);
  if(name=="dot"){node=node.Children.Find(child=>child.Pseudo=="::after");node.Width=item.Item2;node.Height=item.Item3;}
  var paint=name=="body"?EchoSphere.Presentation.NativeMenuArtwork.BodyPaint():name.StartsWith("ring")?EchoSphere.Presentation.NativeMenuArtwork.RingPaint(ringIndex):name=="dot"?EchoSphere.Presentation.NativeMenuArtwork.DotPaint():name=="highlight"?EchoSphere.Presentation.NativeMenuArtwork.HighlightPaint():EchoSphere.Presentation.NativeMenuArtwork.AvatarPaint(new UnityEngine.Color(85f/255,223f/255,1));
  var expected=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,node,item.Item4);brush.Render(item.Item2,item.Item3,item.Item4,paint);var texture=brush.Texture;
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
  var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-menu-brushes");System.IO.Directory.CreateDirectory(directory);System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,index+"-"+name+".png"),UnityEngine.ImageConversion.EncodeToPNG(read));
  if(maximum>2||mean>.1||visibleRaw>2)throw new System.Exception("GPU/source brush mismatch "+Newtonsoft.Json.JsonConvert.SerializeObject(results));
  UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(read);read=null;index++;
  if(index<cases.Length)return;
  UnityEditor.EditorApplication.update-=tick;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeMenuBrushes",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS bounded native artwork brush GPU/source paint",results}));
 }catch(System.Exception e){UnityEditor.EditorApplication.update-=tick;UnityEngine.RenderTexture.active=previous;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeMenuBrushes","FAIL "+e);}
 finally{UnityEngine.RenderTexture.active=previous;if(read!=null)UnityEngine.Object.DestroyImmediate(read);}
};
UnityEditor.EditorApplication.update+=tick;return "Started twelve GPU/source artwork brush cases";
