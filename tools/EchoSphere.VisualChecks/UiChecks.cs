using System;
using System.Collections.Generic;
using System.IO;
using EchoSphere.Presentation;

internal static class UiChecks
{
    private static V Normalize(V value)
    {
        if(value.Raw is Func<V[],V>)return "$function";
        if(value.Raw is VisualDom.Node node)return VisualJs.Obj("tag",node.Tag,"properties",Properties(node.Properties),"children",Normalize(new V(node.Children)));
        if(value.Raw is List<V> list){var children=new List<V>();foreach(var child in list){if(child.Undefined||child.Null||child.Raw is bool||child.Raw is string text&&text.Length==0)continue;var normalized=Normalize(child);if(normalized.Raw is List<V>)children.AddRange(normalized.Items);else children.Add(normalized);}return VisualJs.Arr(children.ToArray());}
        if(value.Raw is Dictionary<string,V> map){var result=VisualJs.Obj();foreach(var pair in map)if(pair.Key!="key"&&pair.Key!="ref"&&!pair.Value.Undefined)result[pair.Key]=Normalize(pair.Value);return result;}
        return value;
    }
    private static V Properties(V value)
    {var result=VisualJs.Obj();foreach(var pair in (Dictionary<string,V>)value.Raw)if(pair.Key!="children"&&pair.Key!="key"&&pair.Key!="ref"&&!pair.Value.Undefined)result[pair.Key]=Normalize(pair.Value);return result;}
    public static int Run()
    {
        VisualSourceIcons.Data=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiIcons.json")));
        var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-ui-fixtures.json")));
        var count=0;
        foreach(var fixture in fixtures.Items) {
            VisualDom.Reset();VisualDom.BeginFrame();var calls=new List<V>(fixture["calls"].Items);var cursor=0;
            VisualUiBindings.Resolve=(module,name)=>module=="global"?name switch {
                "window"=>VisualJs.Obj("innerWidth",390,"innerHeight",844),
                "localStorage"=>VisualJs.Obj("getItem",VisualJs.Fn(args=>VisualJs.Null)),
                "Date"=>new V(new VisualDateHost()),
                _=>throw new InvalidOperationException("Unexpected global UI read: "+name)
            }:VisualJs.Fn(args=> {
                if(cursor>=calls.Count)throw new InvalidOperationException("Unexpected UI state read: "+module+"/"+name);
                var call=calls[cursor++];if(call["module"].Text!=module||call["name"].Text!=name)throw new InvalidOperationException("UI state reads differ: "+module+"/"+name+" vs "+call["module"].Text+"/"+call["name"].Text);
                return call["value"];
            });
            var props=fixture["props"];
            foreach(var pair in new List<KeyValuePair<string,V>>((Dictionary<string,V>)props.Raw))if(pair.Value.Text=="$function")props[pair.Key]=VisualJs.Fn(args=>default);
            props["t"]=VisualJs.Fn(args=>fixture["translations"][args[0]]);
            var module=fixture["module"].Text switch {"App"=>Visual_App.Scope,"CharacterSelect"=>Visual_CharacterSelect.Scope,"KnowledgeBase"=>Visual_KnowledgeBase.Scope,"EchoMapScreen"=>Visual_EchoMapScreen.Scope,"MobileControls"=>Visual_MobileControls.Scope,"TutorialOverlay"=>Visual_TutorialOverlay.Scope,_=>throw new InvalidOperationException("Unknown source UI root")};
            V tree;try{tree=VisualDom.Component(fixture["name"].Text,module[fixture["name"]],props);}catch(Exception error){throw new InvalidOperationException(fixture["label"].Text+": "+error.Message,error);}
            var actual=VisualJson.Stringify(Normalize(tree),true);var expected=VisualJson.Stringify(fixture["tree"],true);
            if(actual!=expected||cursor!=calls.Count){File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-ui-actual.json"),actual);File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-ui-expected.json"),expected);throw new InvalidOperationException(fixture["label"].Text+": original TSX and native retained tree differ; state reads "+cursor+" / "+calls.Count);}
            count++;
        }
        Console.WriteLine($"Original TSX → native C# retained UI trees: PASS ({count} complete screen/language/tab scenarios).");return count;
    }
}
