using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    public static class VisualSourceIcons
    {
        public static V Data;
        public static V Create(string name,V properties)
        {
            var template=Data[name];if(template.Undefined)throw new InvalidOperationException("Missing original Lucide icon: "+name);
            var props=VisualJs.Merge(template["properties"]);var values=(Dictionary<string,V>)props.Raw;
            var size=properties["size"].Undefined?new V(24d):properties["size"];
            props["width"]=size;props["height"]=size;
            if(!properties["className"].Undefined)props["className"]=(props["className"].Text.TrimEnd()+" "+properties["className"].Text).Trim();
            if(!properties["color"].Undefined)props["stroke"]=properties["color"];
            if(properties.Raw is Dictionary<string,V> supplied)foreach(var pair in supplied)if(pair.Key!="size"&&pair.Key!="className"&&pair.Key!="children"&&pair.Key!="color"&&pair.Key!="absoluteStrokeWidth")props[pair.Key]=pair.Value;
            if(properties["absoluteStrokeWidth"].Truth)props["strokeWidth"]=(properties["strokeWidth"].Undefined?new V(2d):properties["strokeWidth"])*24d/size;
            var children=new List<V>();foreach(var child in template["children"].Items)children.Add(Read(child));
            return VisualDom.Element("svg",props,children.ToArray());
        }
        private static V Read(V value)
        {
            if(value.Raw is Dictionary<string,V>&&!value["tag"].Undefined){var children=new List<V>();foreach(var child in value["children"].Items)children.Add(Read(child));return VisualDom.Element(value["tag"].Text,VisualJs.Merge(value["properties"]),children.ToArray());}
            return value;
        }
    }
}
