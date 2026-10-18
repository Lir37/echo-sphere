var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var type=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly.GetType("EchoSphere.Presentation.SourceUiReplica");
var replica=System.Activator.CreateInstance(type,flags,null,new object[]{new System.Func<UnityEngine.UIElements.VisualElement,System.Action<UnityEngine.UIElements.MeshGenerationContext>>(e=>null)},null);
var source=new UnityEngine.UIElements.Label("before");source.style.width=180;source.style.height=32;source.style.fontSize=18;
var stop=new UnityEngine.UIElements.VisualElement();var root=new UnityEngine.UIElements.VisualElement();root.Add(source);root.Add(stop);
var host=new UnityEngine.UIElements.VisualElement();host.style.position=UnityEngine.UIElements.Position.Absolute;host.style.left=8;host.style.top=8;host.style.width=200;host.style.height=48;
UnityEngine.Object.FindFirstObjectByType<UnityEngine.UIElements.UIDocument>().rootVisualElement.Add(host);
root.style.position=UnityEngine.UIElements.Position.Absolute;root.style.left=8;root.style.top=80;root.style.width=200;root.style.height=48;UnityEngine.Object.FindFirstObjectByType<UnityEngine.UIElements.UIDocument>().rootVisualElement.Add(root);
var update=type.GetMethod("Update",flags);void Apply()=>update.Invoke(replica,new object[]{root,stop,host,new UnityEngine.Rect(0,0,200,48)});
Apply();var copies=(System.Collections.IDictionary)type.GetField("_copies",flags).GetValue(replica);var copy=copies[source];
var target=(UnityEngine.UIElements.Label)copy.GetType().GetField("Element",flags).GetValue(copy);
var native=(System.Action<UnityEngine.UIElements.MeshGenerationContext>)copy.GetType().GetField("TextPaint",flags).GetValue(copy);int paints=0;
copy.GetType().GetField("TextPaint",flags).SetValue(copy,new System.Action<UnityEngine.UIElements.MeshGenerationContext>(ctx=>{paints++;native?.Invoke(ctx);}));
int phase=0,frame=UnityEngine.Time.frameCount,previous=0;
UnityEditor.SessionState.SetString("EchoRetainedTextCheck","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(UnityEngine.Time.frameCount<frame+3)return;
 try{
  if(phase==0){previous=paints;source.text="after";Apply();phase=1;frame=UnityEngine.Time.frameCount;return;}
  if(phase==1){if(target.text!="after"||paints<=previous)throw new System.Exception("Native text invalidation missing");previous=paints;source.style.fontSize=24;source.style.color=UnityEngine.Color.cyan;Apply();phase=2;frame=UnityEngine.Time.frameCount;return;}
  if(paints<=previous)throw new System.Exception("Native font/style invalidation missing");
  UnityEditor.SessionState.SetString("EchoRetainedTextCheck","PASS native text and font/style repaint, "+paints+" native paint callbacks");
 }catch(System.Exception e){UnityEditor.SessionState.SetString("EchoRetainedTextCheck","FAIL "+e);}
 UnityEditor.EditorApplication.update-=tick;((System.IDisposable)replica).Dispose();host.RemoveFromHierarchy();root.RemoveFromHierarchy();UnityEditor.EditorApplication.isPaused=true;
};
UnityEditor.EditorApplication.update+=tick;UnityEditor.EditorApplication.isPaused=false;return "Started native retained text invalidation check";
