using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Compare display-only DTOs on screen entry, never by serializing them or
    // comparing list identities. Callback owners are refreshed by Show.
    internal static class NativeMenuRetainedState
    {
        private static readonly Dictionary<Type,FieldInfo[]> Fields=new Dictionary<Type,FieldInfo[]>();
        internal static bool Same(object a,object b)
        {
            if(ReferenceEquals(a,b))return true;
            if(a==null||b==null||a.GetType()!=b.GetType())return false;
            var type=a.GetType();
            if(type.IsValueType||a is string)return a.Equals(b);
            if(a is IList left&&b is IList right){if(left.Count!=right.Count)return false;for(int i=0;i<left.Count;i++)if(!Same(left[i],right[i]))return false;return true;}
            if(type.Namespace!="EchoSphere.Presentation"||!type.IsNested)return a.Equals(b);
            if(!Fields.TryGetValue(type,out var fields)){fields=type.GetFields(BindingFlags.Instance|BindingFlags.Public);Fields.Add(type,fields);}
            foreach(var field in fields)if(!Same(field.GetValue(a),field.GetValue(b)))return false;
            return true;
        }
        internal static void Visible(VisualElement root,bool visible)
        {
            // Keep filter allocations attached while a screen is inactive. Unity
            // 6.3 can retain filter texture IDs across display:none transitions.
            root.style.visibility=visible?Visibility.Visible:Visibility.Hidden;
            root.style.opacity=visible?1:0;root.SetEnabled(visible);
        }
    }
}
