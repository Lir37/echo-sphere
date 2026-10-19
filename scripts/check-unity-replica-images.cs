var type=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly.GetType("EchoSphere.Presentation.SourceUiReplica");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var paint=new System.Func<UnityEngine.UIElements.VisualElement,System.Action<UnityEngine.UIElements.MeshGenerationContext>>(e=>null);
var replica=System.Activator.CreateInstance(type,flags,null,new object[]{paint},null);
var root=new UnityEngine.UIElements.VisualElement();var image=new UnityEngine.UIElements.Image();var stop=new UnityEngine.UIElements.VisualElement();var target=new UnityEngine.UIElements.VisualElement();root.Add(image);root.Add(stop);
var texture=new UnityEngine.Texture2D(4,4);var vector=UnityEngine.Resources.Load<UnityEngine.UIElements.VectorImage>("EchoSphere/NativeMenuIcons/Settings");var sprite=UnityEngine.Sprite.Create(texture,new UnityEngine.Rect(0,0,4,4),new UnityEngine.Vector2(.5f,.5f));
int checks=0;
try{
 for(int cycle=0;cycle<8;cycle++)for(int phase=0;phase<4;phase++){
  image.image=null;image.sprite=null;image.vectorImage=null;
  if(phase==0)image.image=texture;else if(phase==1)image.vectorImage=vector;else if(phase==2)image.sprite=sprite;
  image.tintColor=new UnityEngine.Color(.2f,.5f,.7f,.8f);image.scaleMode=UnityEngine.ScaleMode.ScaleToFit;image.uv=new UnityEngine.Rect(.1f,.2f,.7f,.6f);
  type.GetMethod("Update",flags).Invoke(replica,new object[]{root,stop,target,new UnityEngine.Rect(0,0,100,100)});
  var copies=(System.Collections.IDictionary)type.GetField("_copies",flags).GetValue(replica);var copy=copies[image];var e=(UnityEngine.UIElements.Image)copy.GetType().GetField("Element",flags).GetValue(copy);
  if(e.image!=image.image||e.sprite!=image.sprite||e.vectorImage!=image.vectorImage||e.tintColor!=image.tintColor||e.scaleMode!=image.scaleMode||e.uv!=image.uv)throw new System.Exception("Native image replica stale at cycle "+cycle+" phase "+phase);checks+=6;
 }
}finally{((System.IDisposable)replica).Dispose();UnityEngine.Object.Destroy(sprite);UnityEngine.Object.Destroy(texture);}
return new{status="PASS",checks,scope="Retained Texture2D / VectorImage / Sprite / empty transitions; tint, scaling and UV"};
