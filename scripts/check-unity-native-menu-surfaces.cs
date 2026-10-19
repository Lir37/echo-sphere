UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMenuBrush.shader",UnityEditor.ImportAssetOptions.ForceUpdate);
var cases=new[]{("Loadout",false,false,1f),("Character",false,false,1f),("Character",true,false,1f),("Difficulty",false,false,1f),("Difficulty",false,true,1f),("Continue",false,false,1f),("Continue",true,false,1f),("Navigation",false,false,1f),("NavigationButton",false,false,1f),("NavigationButton",true,false,1f),("Continue",false,false,1.75f),("Navigation",false,false,1.75f)};
var type=typeof(EchoSphere.Presentation.NativeMainMenuComposition);var kindType=type.GetNestedType("Kind",System.Reflection.BindingFlags.NonPublic);var method=type.GetMethod("Paint",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse(UnityEngine.Resources.Load<UnityEngine.TextAsset>("EchoSphere/SourceUiStyles").text)){Width=1260,Height=542};
var brush=new EchoSphere.Presentation.NativeMenuBrush();int index=0;var results=new System.Collections.Generic.List<object>();
UnityEditor.SessionState.SetString("EchoNativeMenuSurfaces","RUNNING");UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 UnityEngine.Texture2D read=null;var previous=UnityEngine.RenderTexture.active;
 try{
  var item=cases[index];var kind=item.Item1;
  var root=new EchoSphere.Presentation.VisualCssNode{Tag="div",Properties=EchoSphere.Presentation.VisualJs.Obj("className","es-main-menu es-main-menu-legacy")};
  var parent=new EchoSphere.Presentation.VisualCssNode{Tag="div",Parent=root,Properties=EchoSphere.Presentation.VisualJs.Obj("className",kind=="Difficulty"?"es-loadout-options":kind=="NavigationButton"?"es-main-nav":"")};root.Children.Add(parent);
  var node=new EchoSphere.Presentation.VisualCssNode{Tag=kind=="Loadout"||kind=="Navigation"?"div":"button",Parent=parent,Width=kind=="Difficulty"?68:240,Height=kind=="Loadout"?90:kind=="Difficulty"?24:48,Hover=item.Item2,Properties=EchoSphere.Presentation.VisualJs.Obj("className",kind=="Loadout"?"es-main-loadout":kind=="Character"?"es-loadout-character":kind=="Continue"?"es-main-continue":kind=="Navigation"?"es-main-nav":item.Item3?"is-active":"")};parent.Children.Add(node);sheet.Apply(root);
  var paint=(EchoSphere.Presentation.NativeMenuBrush.Paint)method.Invoke(null,new object[]{System.Enum.Parse(kindType,kind),item.Item2,item.Item3});
  var expected=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,node,item.Item4);brush.Render((float)node.Width,(float)node.Height,item.Item4,paint);
  // An authored transparent box uses a compact 1px brush. The native typed
  // surface retains its layout-sized empty target; compare its actual pixels.
  bool empty=expected.Width==1&&expected.Height==1&&expected.Pixels[3]==0;
  var texture=brush.Texture;if(!empty&&(texture.width!=expected.Width||texture.height!=expected.Height))throw new System.Exception("Bounds differ "+kind+" "+texture.width+"x"+texture.height+" / "+expected.Width+"x"+expected.Height);
  read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false,false);UnityEngine.RenderTexture.active=texture;read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();var pixels=read.GetPixels32();double maximum=0,total=0;int raw=0,alpha=0;string worst="";
  for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++){
   var actual=pixels[(texture.height-1-y)*texture.width+x];int at=empty?0:(y*texture.width+x)*4;var e=expected.Pixels;
   alpha=System.Math.Max(alpha,System.Math.Abs(e[at+3]-actual.a));var differences=new[]{System.Math.Abs(e[at]*e[at+3]/255d-actual.r*actual.a/255d),System.Math.Abs(e[at+1]*e[at+3]/255d-actual.g*actual.a/255d),System.Math.Abs(e[at+2]*e[at+3]/255d-actual.b*actual.a/255d),System.Math.Abs(e[at+3]-actual.a)};
   foreach(var d in differences){total+=d;if(d>maximum){maximum=d;worst=x+","+y+" expected="+e[at]+","+e[at+1]+","+e[at+2]+","+e[at+3]+" actual="+actual;}}
   if(e[at+3]>=128||actual.a>=128)raw=System.Math.Max(raw,System.Math.Max(System.Math.Abs(e[at]-actual.r),System.Math.Max(System.Math.Abs(e[at+1]-actual.g),System.Math.Abs(e[at+2]-actual.b))));
  }
  var mean=total/(pixels.Length*4);results.Add(new{kind,hover=item.Item2,selected=item.Item3,dpr=item.Item4,maximum,mean,raw,alpha,worst});
  // Flat alpha can straddle a half-byte after float GPU versus double CPU
  // arithmetic over many pixels. Bound stored visible RGB and alpha by 1 byte;
  // their premultiplied product can differ by up to 2 bytes. Report the mean.
  if(maximum>2||raw>1||alpha>1)throw new System.Exception("Native surface mismatch "+Newtonsoft.Json.JsonConvert.SerializeObject(results));
  UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(read);read=null;
  if(++index<cases.Length)return;UnityEditor.EditorApplication.update-=tick;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeMenuSurfaces",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS native GPU menu surface/source paint",results}));
 }catch(System.Exception e){UnityEditor.EditorApplication.update-=tick;UnityEngine.RenderTexture.active=previous;brush.Dispose();UnityEditor.SessionState.SetString("EchoNativeMenuSurfaces","FAIL "+e);}
 finally{UnityEngine.RenderTexture.active=previous;if(read!=null)UnityEngine.Object.DestroyImmediate(read);}
};UnityEditor.EditorApplication.update+=tick;return "Started 12 native menu surface GPU/source cases";
