using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    // Retained, engine-independent authored UI tree. The TSX exporter preserves
    // element order, conditional content, classes, inline style and event handlers.
    public static class VisualDom
    {
        public sealed class Node
        {
            public string Tag;
            public V Properties;
            public readonly List<V> Children=new List<V>();
        }
        private sealed class ComponentState {public readonly List<V> Hooks=new List<V>();public readonly List<V> EffectSlots=new List<V>();public int Hook,Child;public string Path;public bool Used;}
        private static readonly Dictionary<string,ComponentState> States=new Dictionary<string,ComponentState>();
        private static readonly Stack<ComponentState> Stack=new Stack<ComponentState>();
        private static readonly List<Action> Effects=new List<Action>();
        public static Action Changed;
        public static void BeginFrame(){foreach(var state in States.Values)state.Used=false;Stack.Clear();Effects.Clear();Stack.Push(new ComponentState {Path="root"});}
        private static void Cleanup(ComponentState state){foreach(var slot in state.EffectSlots)if(slot["cleanup"].Raw is Func<V[],V>)slot["cleanup"].Call();}
        public static void EndFrame(){foreach(var key in new List<string>(States.Keys))if(!States[key].Used){Cleanup(States[key]);States.Remove(key);}foreach(var effect in Effects.ToArray())effect();Effects.Clear();}
        public static void Reset(){foreach(var state in States.Values){state.Used=false;Cleanup(state);}States.Clear();Stack.Clear();Effects.Clear();}
        public static V Component(string name,V function,V props)
        {
            var parent=Stack.Peek();var path=parent.Path+"/"+name+":"+(props["key"].Undefined?(parent.Child++).ToString():props["key"].Text);
            if(!States.TryGetValue(path,out var state)){state=new ComponentState {Path=path};States.Add(path,state);}
            state.Used=true;state.Hook=0;state.Child=0;Stack.Push(state);try{return function.Call(props);}finally{Stack.Pop();}
        }
        public static V Element(string tag,V props,params V[] children)
        {
            var node=new Node {Tag=tag,Properties=props};
            void Add(V value) {if(value.Null||value.Undefined||value.Raw is bool||value.Raw is string text&&text.Length==0)return;if(value.Raw is List<V>)foreach(var child in value.Items)Add(child);else node.Children.Add(value);}
            foreach(var child in children)Add(child);return new V(node);
        }
        public static V Props(V props,params V[] children) {props["children"]=VisualJs.Arr(children);return props;}
        private static V Hook(Func<V> initial)
        {
            var state=Stack.Peek();var index=state.Hook++;if(index>=state.Hooks.Count)state.Hooks.Add(initial());return state.Hooks[index];
        }
        public static V React(string name)
        {
            switch(name) {
                case "Fragment":return VisualJs.Fn(args=>args[0]["children"]);
                case "useState":return VisualJs.Fn(args=> {
                    var owner=Stack.Peek();var slot=Hook(()=>VisualJs.Obj("value",args[0].Raw is Func<V[],V>?args[0].Call():args[0]));
                    if(slot["setter"].Undefined)slot["setter"]=VisualJs.Fn(next=>{if(!owner.Used)return default;var value=next[0].Raw is Func<V[],V>?next[0].Call(slot["value"]):next[0];if(!SameValue(slot["value"],value)){slot["value"]=value;Changed?.Invoke();}return default;});
                    return VisualJs.Arr(slot["value"],slot["setter"]);
                });
                case "useMemo":return VisualJs.Fn(args=>{var slot=Hook(()=>VisualJs.Obj());var dependencies=VisualJs.Arg(args,1);if(slot["dependencies"].Undefined||dependencies.Undefined||!SameDependencies(slot["dependencies"],dependencies)){slot["value"]=args[0].Call();slot["dependencies"]=dependencies;}return slot["value"];});
                case "useRef":return VisualJs.Fn(args=>Hook(()=>VisualJs.Obj("current",args[0])));
                case "useEffect":return VisualJs.Fn(args=> {
                    var owner=Stack.Peek();var slot=Hook(()=>{var effect=VisualJs.Obj("dependencies",default(V),"cleanup",default(V));owner.EffectSlots.Add(effect);return effect;});var previous=slot["dependencies"];var current=VisualJs.Arg(args,1);var changed=previous.Undefined||current.Undefined;
                    if(!changed){var before=new List<V>(previous.Items);var after=new List<V>(current.Items);changed=before.Count!=after.Count;for(var i=0;!changed&&i<before.Count;i++)changed=before[i]!=after[i];}
                    if(changed){slot["dependencies"]=current;Effects.Add(()=>{var cleanup=slot["cleanup"];if(cleanup.Raw is Func<V[],V>)cleanup.Call();slot["cleanup"]=args[0].Call();});}
                    return default;
                });
                default:throw new InvalidOperationException("Unbound source UI hook "+name);
            }
        }
        private static bool SameValue(V a,V b){if(a.Raw is double x&&b.Raw is double y){if(double.IsNaN(x)&&double.IsNaN(y))return true;if(x==0d&&y==0d)return BitConverter.DoubleToInt64Bits(x)==BitConverter.DoubleToInt64Bits(y);}return a==b;}
        private static bool SameDependencies(V before,V after){var a=new List<V>(before.Items);var b=new List<V>(after.Items);if(a.Count!=b.Count)return false;for(var i=0;i<a.Count;i++)if(a[i]!=b[i])return false;return true;}
    }
    public static class VisualUiBindings
    {
        public static Func<string,string,V> Resolve;
        public static V Get(string module,string name)=>Resolve!=null?Resolve(module,name):VisualJs.Fn(args=>throw new InvalidOperationException("Unbound source UI state/action: "+module+"/"+name));
        public static V Icon(string name)=>VisualJs.Fn(args=>VisualSourceIcons.Create(name,args[0]));
    }
}
