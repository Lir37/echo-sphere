using EchoSphere.Presentation;
using System.IO.Compression;

internal static class UiInteractionChecks
{
    private sealed class Element : IVisualHost
    {
        public bool Control;
        public V Get(string key)=>key switch {
            "width"=>390,"height"=>844,
            "getBoundingClientRect"=>VisualJs.Fn(args=>VisualJs.Obj("x",0,"y",0,"left",0,"top",0,"right",390,"bottom",844,"width",390,"height",844)),
            "closest"=>VisualJs.Fn(args=>Control?new V(this):VisualJs.Null),
            "setPointerCapture" or "releasePointerCapture"=>VisualJs.Fn(args=>default),"hasPointerCapture"=>VisualJs.Fn(args=>true),_=>default
        };
        public void Set(string key,V value)=>throw new InvalidOperationException("Unexpected test DOM write "+key);
    }
    private static V Normalize(V value,bool child=false)
    {
        if(value.Raw is Func<V[],V>)return "$function";
        if(value.Raw is double number)return child?new V(VisualJs.NumberText(number)):new V(Math.Round(number*1e9)/1e9);
        if(value.Raw is VisualDom.Node node){
            var children=Normalize(new V(node.Children),true);if(node.Tag=="fragment")return children;
            var properties=VisualJs.Obj();foreach(var pair in (Dictionary<string,V>)node.Properties.Raw)if(pair.Key!="key"&&pair.Key!="ref"&&pair.Key!="children"&&!pair.Value.Undefined)properties[pair.Key]=Normalize(pair.Value);
            return VisualJs.Obj("tag",node.Tag,"properties",properties,"children",children);
        }
        if(value.Raw is List<V> list){var items=new List<V>();foreach(var item in list){if(child&&(item.Null||item.Undefined||item.Raw is bool))continue;var normalized=Normalize(item,child);if(child&&normalized.Raw is List<V>)items.AddRange(normalized.Items);else items.Add(normalized);}return VisualJs.Arr(items.ToArray());}
        if(value.Raw is Dictionary<string,V> map){var result=VisualJs.Obj();foreach(var pair in map)if(pair.Key!="key"&&pair.Key!="ref"&&!pair.Value.Undefined)result[pair.Key]=Normalize(pair.Value,pair.Key=="children");return result;}
        return value;
    }
    private static string Canonical(V value)=>VisualJson.Stringify(Normalize(value),true);
    private static IEnumerable<VisualDom.Node> Nodes(V value)
    {if(value.Raw is VisualDom.Node node){yield return node;foreach(var child in node.Children)foreach(var nested in Nodes(child))yield return nested;}else if(value.Raw is List<V>)foreach(var child in value.Items)foreach(var nested in Nodes(child))yield return nested;}
    private static void Merge(V target,V source)
    {foreach(var pair in (Dictionary<string,V>)source.Raw){if(pair.Value.Raw is Dictionary<string,V>&&target[pair.Key].Raw is Dictionary<string,V>)Merge(target[pair.Key],pair.Value);else target[pair.Key]=pair.Value;}}
    private static V StateView(V state)=>VisualJs.Obj("keys",state["keys"],"selectedSphereType",state["selectedSphereType"],"locked",state["player"]["sphereMovementLocked"],"follow",state["player"]["formationFollowActive"],"flashText",state["flashText"],"spheres",VisualJs.Arr(state["spheres"].Items.Select(sphere=>VisualJs.Obj("type",sphere["type"],"alive",sphere["alive"],"pos",sphere["pos"])).ToArray()));
    public static void Run()
    {
        using var compressed=new GZipStream(File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-ui-interactions.json.gz")),CompressionMode.Decompress);using var reader=new StreamReader(compressed);var fixtures=VisualJson.Parse(reader.ReadToEnd());var count=0;
        VisualSourceIcons.Data=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiIcons.json")));
        foreach(var fixture in fixtures.Items){
            VisualDom.Reset();var timers=new VisualTimers();var clock=1000d;timers.Tick(clock);var date=new V(new VisualDateHost(()=>clock));var state=fixture["initial"]["state"];var entityIds=new Dictionary<int,V>();var sphereArray=state["spheres"].Items.ToArray();var initialIds=fixture["initial"]["entities"].Items.ToArray();for(var i=0;i<sphereArray.Length;i++)entityIds[(int)initialIds[i].Number]=sphereArray[i];
            V currentFrame=default;var actionCursor=0;var haptics=new List<V>();var propCalls=new List<V>();
            void ApplySnapshot(V snapshot){
                var after=snapshot["state"];var spheres=after["spheres"].Items.ToArray();var ids=snapshot["entities"].Items.ToArray();var entities=new List<V>();
                for(var i=0;i<spheres.Length;i++){var id=(int)ids[i].Number;if(!entityIds.TryGetValue(id,out var entity)){entity=VisualJs.Obj();entityIds.Add(id,entity);}Merge(entity,spheres[i]);entities.Add(entity);}
                // Match the native bridge: stable entities, replaced list; the
                // player and GameState objects held by UI refs stay alive.
                foreach(var pair in (Dictionary<string,V>)after.Raw)if(pair.Key!="spheres"){if(pair.Value.Raw is Dictionary<string,V>&&state[pair.Key].Raw is Dictionary<string,V>)Merge(state[pair.Key],pair.Value);else state[pair.Key]=pair.Value;}
                state["spheres"]=VisualJs.Arr(entities.ToArray());
            }
            var window=VisualJs.Obj("innerWidth",390,"innerHeight",844,"setTimeout",VisualJs.Fn(args=>timers.Schedule(args,false,false)),"clearTimeout",VisualJs.Fn(timers.Cancel));
            var storage=VisualJs.Obj("getItem",VisualJs.Fn(args=>args[0].Text=="echosphere_interface_scale"?new V("1"):args[0].Text=="echosphere_lang"?fixture["lang"]:VisualJs.Null));
            VisualJs.PerformanceObject["now"]=VisualJs.Fn(args=>clock);
            VisualUiBindings.Resolve=(module,name)=>module=="global"?name switch {
                "window"=>window,"localStorage"=>storage,"Date"=>date,"navigator"=>VisualJs.Obj("vibrate",VisualJs.Fn(args=>{haptics.Add(args[0]);return true;})),
                "isElement"=>VisualJs.Fn(args=>args[0].Raw is Element),"requestAnimationFrame"=>VisualJs.Fn(args=>timers.Schedule(args,true,false)),"cancelAnimationFrame"=>VisualJs.Fn(timers.Cancel),
                _=>throw new InvalidOperationException("Unbound interaction API "+name)
            }:VisualJs.Fn(args=>{
                var calls=currentFrame["calls"].Items.ToArray();if(!new[]{"placeSphere","repositionSphere","setSphereType","setFormationFollow","activateDash","activateByKey"}.Contains(name))return currentFrame["reads"][module+":"+name];
                if(actionCursor>=calls.Length)throw new InvalidOperationException("Unexpected interaction action "+name);var call=calls[actionCursor++];
                if(call["module"].Text!=module||call["name"].Text!=name||Canonical(VisualJs.Arr(args.Skip(1).ToArray()))!=Canonical(call["args"]))throw new InvalidOperationException("Source interaction action / arguments differ: "+module+"/"+name);
                ApplySnapshot(call["after"]);return call["result"];
            });
            var lang=fixture["lang"].Text;var props=VisualJs.Obj("lang",lang,"handedness",fixture["handedness"],"tutorialStep",VisualJs.Null,"stateRef",VisualJs.Obj("current",state),"canvasRef",VisualJs.Obj("current",new V(new Element())),"t",VisualJs.Fn(args=>Visual_i18n.Scope["translations"][lang][args[0]]),"onPause",VisualJs.Fn(args=>{propCalls.Add("onPause");return default;}));
            V tree=default;void Render(){VisualDom.BeginFrame();tree=VisualDom.Component("MobileControls",Visual_MobileControls.Scope["MobileControls"],props);VisualDom.EndFrame();}
            foreach(var frame in fixture["frames"].Items){
                currentFrame=frame;clock=frame["time"].Number;actionCursor=0;haptics.Clear();propCalls.Clear();var step=frame["step"];
                if(step["kind"].Text=="advance")timers.Tick(clock);
                else if(step["kind"].Text=="event"){
                    var selector=step["selector"];var root=selector.Undefined||selector["root"].Truth;
                    var candidates=Nodes(tree).Where(node=>root?node.Tag=="div"&&node.Properties["className"].Text.StartsWith("es-mobile-controls",StringComparison.Ordinal):node.Tag=="button"&&(!selector["aria"].Undefined?node.Properties["aria-label"]==selector["aria"]:node.Properties["aria-label"].Text.Contains(selector["ariaContains"].Text))).ToArray();var index=selector["index"].Undefined?0:(int)selector["index"].Number;var target=candidates[index];var host=new V(new Element {Control=!root});
                    var evt=VisualJs.Obj("clientX",step["x"].Undefined?new V(0d):step["x"],"clientY",step["y"].Undefined?new V(0d):step["y"],"pointerId",step["pointer"].Undefined?new V(1d):step["pointer"],"pointerType","touch","button",0,"buttons",1,"target",host,"currentTarget",host,"stopPropagation",VisualJs.Fn(args=>default),"preventDefault",VisualJs.Fn(args=>default));evt["nativeEvent"]=evt;
                    target.Properties[step["event"]].Call(evt);
                }
                // A Unity frame also updates the shared timer clock before the
                // render. Source RAF callbacks themselves run only on advances.
                if(step["kind"].Text!="advance")timers.Tick(clock);
                Render();var actual=Canonical(tree);var expected=Canonical(frame["tree"]);
                if(actual!=expected||Canonical(StateView(state))!=Canonical(StateView(frame["state"]["state"]))||actionCursor!=frame["calls"].Items.Count()||Canonical(VisualJs.Arr(haptics.ToArray()))!=Canonical(frame["haptics"])||Canonical(VisualJs.Arr(propCalls.ToArray()))!=Canonical(frame["props"])||timers.Count!=frame["timers"].Number){
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-ui-interaction-actual.json"),actual);File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-ui-interaction-expected.json"),expected);
                    throw new InvalidOperationException(fixture["label"].Text+" frame "+count+" "+step["kind"].Text+" "+step["event"].Text+": UI tree/state/actions/effects differ; timers "+timers.Count+"/"+frame["timers"].Text);
                }
                count++;
            }
            VisualDom.Reset();if(timers.Count!=fixture["unmounted"]["timers"].Number||Canonical(state["keys"])!=Canonical(fixture["unmounted"]["keys"]))throw new InvalidOperationException("Source UI cleanup differs");
        }
        Console.WriteLine($"Actual React events/hooks → native UI transitions: PASS ({count} frames, ru/en and both handedness layouts).");
    }
}
