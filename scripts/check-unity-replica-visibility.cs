var type=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly.GetType("EchoSphere.Presentation.SourceUiReplica");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var paint=new System.Func<UnityEngine.UIElements.VisualElement,System.Action<UnityEngine.UIElements.MeshGenerationContext>>(e=>null);
var replica=System.Activator.CreateInstance(type,flags,null,new object[]{paint},null);
var update=type.GetMethod("Update",flags);
var root=new UnityEngine.UIElements.VisualElement();var hidden=new UnityEngine.UIElements.VisualElement();var child=new UnityEngine.UIElements.Label("child");
var sibling=new UnityEngine.UIElements.VisualElement();var stop=new UnityEngine.UIElements.VisualElement();var target=new UnityEngine.UIElements.VisualElement();
root.Add(hidden);hidden.Add(child);root.Add(sibling);root.Add(stop);int checks=0;
try{
 for(int pass=0;pass<60;pass++){
  bool none=pass%3==0;hidden.style.display=none?UnityEngine.UIElements.DisplayStyle.None:UnityEngine.UIElements.DisplayStyle.Flex;
  hidden.style.visibility=UnityEngine.UIElements.Visibility.Hidden;child.style.visibility=UnityEngine.UIElements.Visibility.Visible;
  update.Invoke(replica,new object[]{root,stop,target,new UnityEngine.Rect(0,0,100,100)});
  var copies=(System.Collections.IDictionary)type.GetField("_copies",flags).GetValue(replica);
  if(copies.Contains(hidden)==none||copies.Contains(child)==none||!copies.Contains(root)||!copies.Contains(sibling)||copies.Contains(stop))throw new System.Exception("Display/visibility subtree regression");
  var rootCopy=copies[root];var e=(UnityEngine.UIElements.VisualElement)rootCopy.GetType().GetField("Element",flags).GetValue(rootCopy);
  if(e.childCount!=(none?1:2))throw new System.Exception("Stale hidden copies retained");
  checks+=6;
 }
}finally{((System.IDisposable)replica).Dispose();}
return new{checks,result="PASS",scope="Repeated hide/show subtree removal and restoration; visibility:hidden descendant override retained"};
