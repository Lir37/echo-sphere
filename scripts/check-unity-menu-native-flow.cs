// Actual menu comparison against Unity laying out the same elements in source
// flow order. Temporarily suspends source projection, restores it in finally.
var doc=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var root=(EchoSphere.Presentation.VisualCssNode)doc.GetType().GetField("_root",flags).GetValue(doc);
EchoSphere.Presentation.VisualCssNode menu=null;
void Find(EchoSphere.Presentation.VisualCssNode n){if(n.Properties["className"].Text=="es-main-core")menu=n;foreach(var c in n.Children)Find(c);}
Find(root);if(menu==null)throw new System.Exception("Main menu not visible");
var frames=new System.Collections.Generic.List<UnityEngine.UIElements.VisualElement>();
var before=new System.Collections.Generic.List<UnityEngine.Rect>();
foreach(var c in menu.Children)if(c.Native is UnityEngine.UIElements.VisualElement e){frames.Add(e);before.Add(e.worldBound);}
var parent=frames[0].parent;
var paintOrder=new System.Collections.Generic.List<UnityEngine.UIElements.VisualElement>();
for(int i=0;i<parent.hierarchy.childCount;i++)paintOrder.Add(parent.hierarchy[i]);
var render=doc.Render;doc.Render=null;
foreach(var frame in frames){frame.GetType().GetMethod("SetFlowOffset",flags).Invoke(frame,new object[]{UnityEngine.Vector2.zero});frame.BringToFront();}
int ticks=0;
UnityEditor.SessionState.SetString("EchoMenuFlowCheck","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
    if(++ticks<5)return;
    try{
        float max=0;var differences=new System.Collections.Generic.List<string>();
        for(int i=0;i<frames.Count;i++){
            if(frames[i].resolvedStyle.display==UnityEngine.UIElements.DisplayStyle.None)continue;
            var native=frames[i].worldBound;
            var delta=UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(before[i].x-native.x),UnityEngine.Mathf.Abs(before[i].y-native.y),UnityEngine.Mathf.Abs(before[i].width-native.width),UnityEngine.Mathf.Abs(before[i].height-native.height));
            max=UnityEngine.Mathf.Max(max,delta);
            differences.Add("item="+i+" delta="+delta+" projected="+before[i]+" native="+native);
        }
        if(max>.075f)throw new System.Exception(string.Join("; ",differences));
        UnityEditor.SessionState.SetString("EchoMenuFlowCheck","PASS "+frames.Count+" menu elements, max delta="+max+", "+UnityEngine.Screen.width+"x"+UnityEngine.Screen.height);
    }catch(System.Exception e){UnityEditor.SessionState.SetString("EchoMenuFlowCheck","FAIL "+e);}
    finally{
        foreach(var frame in paintOrder)frame.BringToFront();doc.Render=render;doc.Dirty=true;
        UnityEditor.EditorApplication.update-=tick;
    }
};
UnityEditor.EditorApplication.update+=tick;
return "Started actual menu comparison; poll EchoMenuFlowCheck";
