#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Text;

namespace EchoSphere.Presentation
{
    // Records the full command stream, including property writes, transforms and gradient
    // stops. The same recorder contract is used against the original TypeScript modules.
    public sealed class VisualTrace : IVisualCanvasSink
    {
        public readonly StringBuilder Trace=new StringBuilder();
        public int Count;
        public void Clear() {Trace.Clear();Count=0;}
        public V Call(string name,V[] args)
        {
            Record("call",name,args);
            if(name=="createRadialGradient"||name=="createLinearGradient")return new V(new VisualGradient(name,args));
            if(name=="measureText")return VisualJs.Obj("width",args[0].Text.Length*7);
            return default;
        }
        public void Set(string name,V value)=>Record("set",name,new[]{value});
        private void Record(string kind,string name,V[] args)
        {Count++;Trace.Append(kind).Append(':').Append(name).Append('|');foreach(var arg in args)Value(arg);Trace.Append('\n');}
        private void Value(V value)
        {
            if(value.Raw is double number) { Trace.Append('N').Append(double.IsNaN(number)?"NaN":double.IsPositiveInfinity(number)?"Infinity":double.IsNegativeInfinity(number)?"-Infinity":Math.Floor(number*1000000d+.5d).ToString("0",CultureInfo.InvariantCulture)).Append('|');return; }
            if(value.Raw is string text) {
                if(text.StartsWith("rgb",StringComparison.Ordinal)&&text.EndsWith(")",StringComparison.Ordinal)) {
                    Trace.Append("COLOR[");var start=text.IndexOf('(');
                    foreach(var component in text.Substring(start+1,text.Length-start-2).Split(','))Value(double.Parse(component,CultureInfo.InvariantCulture));
                    Trace.Append("]|");return;
                }
                Trace.Append('S').Append(text.Length).Append(':').Append(text).Append('|');return;
            }
            if(value.Raw is bool boolean) { Trace.Append(boolean?"T|":"F|");return; }
            if(value.Raw is IVisualHost host&&host.Get("src").Raw is string source) {Trace.Append("IMAGE[");Value(source);Trace.Append("]|");return;}
            if(value.Raw is VisualGradient gradient)
            {
                Trace.Append('G').Append(gradient.Kind).Append('[');foreach(var coordinate in gradient.Coordinates)Value(coordinate);
                foreach(var stop in gradient.Stops) {Value(stop.Key);Value(stop.Value);}Trace.Append("]|");return;
            }
            if(value.Raw is System.Collections.Generic.List<V> list) {Trace.Append('[');foreach(var item in list)Value(item);Trace.Append("]|");return;}
            Trace.Append(value.Null?"null|":"undefined|");
        }
    }
}
#endif
