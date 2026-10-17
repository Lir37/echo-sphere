// Run in Play Mode via Unity CLI eval_file. Poll SessionState EchoFlowCheck.
// Two native Yoga trees per fixture: authored flow vs permuted paint hierarchy.
UnityEngine.Application.runInBackground=true;
UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
var doc=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
if(doc==null)throw new System.Exception("Open the menu in Play Mode first");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var assembly=doc.GetType().Assembly;
var frameType=assembly.GetType("EchoSphere.Presentation.SourceUiFrame");
var flowType=assembly.GetType("EchoSphere.Presentation.SourceUiFlow");
var flow=System.Activator.CreateInstance(flowType,true);
var apply=flowType.GetMethod("Apply",flags);
var outer=frameType.GetMethod("ApplyOuterStyle",flags);
var paintBounds=frameType.GetMethod("SetPaintBounds",flags);
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse("{\"rules\":[],\"keyframes\":{}}"));
var holder=new UnityEngine.UIElements.VisualElement();holder.style.position=UnityEngine.UIElements.Position.Absolute;
holder.style.left=-20000;holder.style.top=0;holder.style.visibility=UnityEngine.UIElements.Visibility.Hidden;
doc.GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement.Add(holder);
var cases=new System.Collections.Generic.List<(EchoSphere.Presentation.VisualCssNode Node,UnityEngine.UIElements.VisualElement Reference,UnityEngine.UIElements.VisualElement Painted,UnityEngine.UIElements.VisualElement[] A,UnityEngine.UIElements.VisualElement[] B,bool Eligible)>();
var directions=new[]{UnityEngine.UIElements.FlexDirection.Column,UnityEngine.UIElements.FlexDirection.Row,UnityEngine.UIElements.FlexDirection.ColumnReverse,UnityEngine.UIElements.FlexDirection.RowReverse};
var names=new[]{"column","row","column-reverse","row-reverse"};
var justifications=new[]{UnityEngine.UIElements.Justify.FlexStart,UnityEngine.UIElements.Justify.Center,UnityEngine.UIElements.Justify.FlexEnd,UnityEngine.UIElements.Justify.SpaceBetween,UnityEngine.UIElements.Justify.SpaceAround};
for(int direction=0;direction<4;direction++)for(int justify=0;justify<5;justify++)for(int flex=0;flex<2;flex++){
    var node=new EchoSphere.Presentation.VisualCssNode {Tag="div"};node.Style["display"]="flex";node.Style["flex-direction"]=names[direction];
    var containers=new UnityEngine.UIElements.VisualElement[2];var frames=new UnityEngine.UIElements.VisualElement[2][];
    for(int side=0;side<2;side++){
        var container=new UnityEngine.UIElements.VisualElement();container.style.position=UnityEngine.UIElements.Position.Absolute;
        container.style.width=direction%2==0?240:480;container.style.height=direction%2==0?480:240;
        container.style.flexDirection=directions[direction];container.style.justifyContent=justifications[justify];
        container.style.paddingTop=7;container.style.paddingLeft=11;container.style.paddingRight=9;container.style.paddingBottom=5;
        holder.Add(container);containers[side]=container;frames[side]=new UnityEngine.UIElements.VisualElement[5];
        for(int i=0;i<5;i++){
            var content=new UnityEngine.UIElements.Label("Native text "+i+" can wrap across lines");
            content.style.width=65+i*9;content.style.height=i==2?new UnityEngine.UIElements.StyleLength(UnityEngine.UIElements.StyleKeyword.Auto):new UnityEngine.UIElements.StyleLength(23+i*13);
            content.style.minHeight=0;content.style.minWidth=0;content.style.fontSize=13;
            content.style.marginTop=i+1;content.style.marginBottom=2*i;content.style.marginLeft=2+i;content.style.marginRight=i;
            content.style.flexGrow=flex==1?i%2:0;content.style.flexShrink=flex==1?1:0;
            content.style.translate=new UnityEngine.UIElements.Translate(i*2,-i);
            content.style.rotate=new UnityEngine.UIElements.Rotate(i*3);
            var frame=(UnityEngine.UIElements.VisualElement)System.Activator.CreateInstance(frameType,flags,null,new object[]{content},null);
            outer.Invoke(frame,new object[]{0f});paintBounds.Invoke(frame,new object[]{new UnityEngine.Rect(0,0,0,0)});
            container.Add(frame);frames[side][i]=frame;
            if(side==1){var child=new EchoSphere.Presentation.VisualCssNode {Tag="div",Parent=node,Native=frame};child.Style["order"]=(i%2).ToString();node.Children.Add(child);}
        }
        // CSS order remains logical; z-index is represented by the paint shuffle.
        foreach(int i in side==0?new[]{0,2,4,1,3}:new[]{4,0,3,2,1})frames[side][i].BringToFront();
    }
    node.Native=containers[1];cases.Add((node,containers[0],containers[1],frames[0],frames[1],flex==0&&justify<3));
}
int ticks=0,phase=0,checks=0;
UnityEditor.SessionState.SetString("EchoFlowCheck","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
    if(++ticks<5)return;ticks=0;
    try{
        foreach(var fixture in cases){
            apply.Invoke(flow,new object[]{sheet,fixture.Node});
            for(int i=0;i<5;i++){
                var expected=fixture.A[i].worldBound;var actual=fixture.B[i].worldBound;
                if(phase<2&&fixture.Eligible&&(float.IsNaN(expected.x)||float.IsNaN(actual.x)||UnityEngine.Vector2.Distance(expected.position,actual.position)>.075f||UnityEngine.Vector2.Distance(expected.size,actual.size)>.075f))
                    throw new System.Exception("Geometry mismatch phase="+phase+" fixture="+cases.IndexOf(fixture)+" item="+i+" expected="+expected+" actual="+actual);
                if(phase==2||!fixture.Eligible){var offset=(UnityEngine.Vector2)frameType.GetField("_flowOffset",flags).GetValue(fixture.B[i]);if(offset!=UnityEngine.Vector2.zero)throw new System.Exception("Unsupported flow must retain the existing path");}
                checks++;
            }
            var paintOrder=new[]{4,0,3,2,1};
            for(int i=0;i<5;i++){if(fixture.Painted[i]!=fixture.B[paintOrder[i]])throw new System.Exception("Paint hierarchy changed");checks++;}
        }
        if(phase==0){
            foreach(var fixture in cases){fixture.Reference.style.width=180;fixture.Painted.style.width=180;fixture.Reference.style.height=210;fixture.Painted.style.height=210;fixture.A[0].style.height=41;fixture.B[0].style.height=41;}
            phase++;
            return;
        }
        if(phase==1){foreach(var fixture in cases)fixture.Node.Style["flex-wrap"]="wrap";phase++;return;}
        UnityEditor.SessionState.SetString("EchoFlowCheck","PASS "+checks+" comparisons; 12 native geometry + 28 grow/justify guard fixtures, resize, text, transforms, paint order, reset when wrapping");
    }catch(System.Exception e){UnityEditor.SessionState.SetString("EchoFlowCheck","FAIL "+e);}
    holder.RemoveFromHierarchy();UnityEditor.EditorApplication.update-=tick;
};
UnityEditor.EditorApplication.update+=tick;
return "Started 40 native flex fixtures; poll EchoFlowCheck";
