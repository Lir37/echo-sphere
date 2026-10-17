// Run through Unity MCP eval_file in the actual Editor.
var assembly=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var methods=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;
var cacheType=assembly.GetType("EchoSphere.Presentation.SourceUiReplicaStyle");
var replicaType=assembly.GetType("EchoSphere.Presentation.SourceUiReplica");
var copy=replicaType.GetMethod("CopyInlineGeometry",methods);
var appearance=replicaType.GetMethod("CopyAppearance",methods);
var original=new UnityEngine.UIElements.Label();
var retained=new UnityEngine.UIElements.Label();
retained.style.width=71;retained.style.color=UnityEngine.Color.red;
var cached=(UnityEngine.UIElements.IStyle)System.Activator.CreateInstance(cacheType,flags,null,new object[]{retained.style},null);
var properties=typeof(UnityEngine.UIElements.IStyle).GetProperties();
int checks=0;
void Equal(UnityEngine.UIElements.IStyle a,UnityEngine.UIElements.IStyle b)
{
    foreach(var property in properties){
        if(!object.Equals(property.GetValue(a),property.GetValue(b)))throw new System.Exception("Style mismatch at check "+checks+": "+property.Name+" expected="+property.GetValue(a)+" actual="+property.GetValue(b));
        checks++;
    }
}
Equal(cached,retained.style);
for(int pass=0;pass<60;pass++){
    var source=new UnityEngine.UIElements.Label("Replica "+pass);
    var s=source.style;
    s.width=pass%3==0?new UnityEngine.UIElements.StyleLength(UnityEngine.UIElements.StyleKeyword.Auto):new UnityEngine.UIElements.StyleLength(100+pass);
    s.height=40+pass;s.opacity=pass/60f;s.color=pass%2==0?UnityEngine.Color.cyan:UnityEngine.Color.yellow;
    s.paddingLeft=pass;s.fontSize=12+pass;s.rotate=new UnityEngine.UIElements.Rotate(pass);
    s.translate=new UnityEngine.UIElements.Translate(pass,-pass);s.scale=new UnityEngine.UIElements.Scale(new UnityEngine.Vector3(1+pass*.01f,1,1));
    s.borderTopWidth=pass%4;s.borderBottomColor=UnityEngine.Color.magenta;
    s.display=pass%4==0?UnityEngine.UIElements.DisplayStyle.None:UnityEngine.UIElements.DisplayStyle.Flex;
    s.filter=new System.Collections.Generic.List<UnityEngine.UIElements.FilterFunction>();
    s.textShadow=new UnityEngine.UIElements.TextShadow {offset=new UnityEngine.Vector2(pass,2),blurRadius=pass,color=UnityEngine.Color.red};
    for(int repeat=0;repeat<3;repeat++){
        copy.Invoke(null,new object[]{source,original.style});copy.Invoke(null,new object[]{source,cached});
        Equal(original.style,retained.style);Equal(cached,retained.style);
        appearance.Invoke(null,new object[]{source,original.style});appearance.Invoke(null,new object[]{source,cached});
        Equal(original.style,retained.style);Equal(cached,retained.style);
    }
    if(pass%5==0){original.style.Clear(pass%2==0);cached.Clear(pass%2==0);Equal(original.style,retained.style);Equal(cached,retained.style);}
}
return new {result="PASS",checks,scope="All native IStyle properties, repeated writes, value/keyword changes and Clear(true/false)"};
