#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    public interface IVisualCanvasSink
    {
        V Call(string name,V[] args);
        void Set(string name,V value);
    }
    public sealed class VisualCanvas : IVisualHost
    {
        public readonly IVisualCanvasSink Sink;
        private readonly Dictionary<string,V> _properties=new Dictionary<string,V>();
        private readonly Dictionary<string,V> _methods=new Dictionary<string,V>();
        private readonly Stack<Dictionary<string,V>> _states=new Stack<Dictionary<string,V>>();
        public VisualCanvas(IVisualCanvasSink sink)
        {
            Sink=sink;
            _properties["globalAlpha"]=1d; _properties["globalCompositeOperation"]="source-over";
            _properties["fillStyle"]="#000000"; _properties["strokeStyle"]="#000000";
            _properties["lineWidth"]=1d;_properties["lineCap"]="butt";_properties["lineJoin"]="miter";
            _properties["shadowBlur"]=0d;_properties["shadowColor"]="rgba(0,0,0,0)";
            _properties["shadowOffsetX"]=0d;_properties["shadowOffsetY"]=0d;_properties["lineDashOffset"]=0d;_properties["miterLimit"]=10d;
            _properties["font"]="10px sans-serif";_properties["textAlign"]="start";_properties["textBaseline"]="alphabetic";
        }
        public V Value=>new V(this);
        public V Get(string name)
        {
            if(_properties.TryGetValue(name,out var property)) return property;
            if(_methods.TryGetValue(name,out var method)) return method;
            method=VisualJs.Fn(args=> {
                if(name=="save") _states.Push(new Dictionary<string,V>(_properties));
                else if(name=="restore"&&_states.Count>0) { var saved=_states.Pop();_properties.Clear();foreach(var item in saved)_properties.Add(item.Key,item.Value); }
                return Sink.Call(name,args);
            });_methods[name]=method;return method;
        }
        public void Set(string name,V value) { _properties[name]=value;Sink.Set(name,value); }
    }
    public sealed class VisualGradient : IVisualHost
    {
        public readonly string Kind;
        public readonly double[] Coordinates;
        public readonly List<KeyValuePair<double,string>> Stops=new List<KeyValuePair<double,string>>();
        public VisualGradient(string kind,V[] args)
        { Kind=kind;Coordinates=new double[args.Length];for(var i=0;i<args.Length;i++)Coordinates[i]=args[i].Number; }
        public V Get(string name)=>name=="addColorStop"?VisualJs.Fn(args=> {Stops.Add(new KeyValuePair<double,string>(args[0].Number,args[1].Text));return default;}):default;
        public void Set(string name,V value)=>throw new InvalidOperationException("Unknown gradient property "+name);
    }
}
#endif
