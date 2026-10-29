using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Shared immutable skins. Only live surfaces retain entries; the last owner
    // releases the texture/material, so visiting screens cannot grow this cache
    // independently of their actual retained UI.
    internal static class NativeMenuBrushCache
    {
        internal sealed class Entry
        {
            internal NativeMenuBrush Brush;internal int References;internal Key Skin;
        }
        internal sealed class Key : IEquatable<Key>
        {
            private readonly float _width,_height,_scale;private readonly NativeMenuBrush.Paint _paint;private readonly int _hash;
            internal Key(float width,float height,float scale,NativeMenuBrush.Paint paint)
            {
                _width=width;_height=height;_scale=scale;
                _paint=new NativeMenuBrush.Paint {Ellipse=paint.Ellipse,Linear=paint.Linear,Horizontal=paint.Horizontal,TopBorderOnly=paint.TopBorderOnly,Radius=paint.Radius,Border=paint.Border,LinearAngle=paint.LinearAngle,Fill=paint.Fill,BorderColor=paint.BorderColor,Origin=paint.Origin,Colors=(Color[])paint.Colors.Clone(),Stops=(float[])paint.Stops.Clone(),OuterA=paint.OuterA,OuterB=paint.OuterB,Inset=paint.Inset,InsetB=paint.InsetB};
                var hash=new HashCode();hash.Add(width);hash.Add(height);hash.Add(scale);hash.Add(paint.Ellipse);hash.Add(paint.Linear);hash.Add(paint.Horizontal);hash.Add(paint.TopBorderOnly);hash.Add(paint.Radius);hash.Add(paint.Border);hash.Add(paint.LinearAngle);hash.Add(paint.Fill);hash.Add(paint.BorderColor);hash.Add(paint.Origin);hash.Add(paint.OuterA);hash.Add(paint.OuterB);hash.Add(paint.Inset);hash.Add(paint.InsetB);
                foreach(var color in paint.Colors)hash.Add(color);foreach(var stop in paint.Stops)hash.Add(stop);_hash=hash.ToHashCode();
            }
            public override int GetHashCode()=>_hash;
            public override bool Equals(object other)=>other is Key key&&Equals(key);
            public bool Equals(Key other)
            {
                if(other==null||_hash!=other._hash||_width!=other._width||_height!=other._height||_scale!=other._scale)return false;
                var a=_paint;var b=other._paint;
                if(a.Ellipse!=b.Ellipse||a.Linear!=b.Linear||a.Horizontal!=b.Horizontal||a.TopBorderOnly!=b.TopBorderOnly||a.Radius!=b.Radius||a.Border!=b.Border||a.LinearAngle!=b.LinearAngle||!a.Fill.Equals(b.Fill)||!a.BorderColor.Equals(b.BorderColor)||!a.Origin.Equals(b.Origin)||!a.OuterA.Equals(b.OuterA)||!a.OuterB.Equals(b.OuterB)||!a.Inset.Equals(b.Inset)||!a.InsetB.Equals(b.InsetB)||a.Colors.Length!=b.Colors.Length||a.Stops.Length!=b.Stops.Length)return false;
                for(int i=0;i<a.Colors.Length;i++)if(!a.Colors[i].Equals(b.Colors[i]))return false;
                for(int i=0;i<a.Stops.Length;i++)if(a.Stops[i]!=b.Stops[i])return false;
                return true;
            }
        }
        private static readonly Dictionary<Key,Entry> Entries=new Dictionary<Key,Entry>();
        private readonly struct RetiredSkin
        {
            internal readonly Entry Entry;internal readonly int Frame;
            internal RetiredSkin(Entry entry){Entry=entry;Frame=Time.frameCount;}
        }
        private static readonly List<RetiredSkin> Retired=new List<RetiredSkin>();
        private static int _maintainedFrame=-1;
        static NativeMenuBrushCache(){Application.quitting+=FlushRetired;
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload+=FlushRetired;
            UnityEditor.EditorApplication.playModeStateChanged+=state=>{if(state==UnityEditor.PlayModeStateChange.ExitingPlayMode)FlushRetired();};
#endif
        }
        // An Image change only invalidates retained UI geometry. Its preceding
        // render commands may still reference the old texture this/next frame.
        // Do not Release the native resource synchronously from Prepare().
        internal static void Maintain()
        {
            if(_maintainedFrame==Time.frameCount)return;_maintainedFrame=Time.frameCount;
            for(int i=Retired.Count-1;i>=0;i--)if(Time.frameCount-Retired[i].Frame>=3){Retired[i].Entry.Brush.Dispose();Retired.RemoveAt(i);}
        }
        private static void FlushRetired(){foreach(var item in Retired)item.Entry.Brush.Dispose();Retired.Clear();_maintainedFrame=-1;}
        internal static int RetiredCount=>Retired.Count;
        internal static Entry Acquire(float width,float height,float scale,NativeMenuBrush.Paint paint)
        {
            Maintain();var key=new Key(width,height,scale,paint);
            if(!Entries.TryGetValue(key,out var entry)){
                var brush=new NativeMenuBrush();try{brush.Render(width,height,scale,paint);}catch{brush.Dispose();throw;}
                entry=new Entry {Brush=brush,Skin=key};Entries.Add(key,entry);
            }
            entry.Brush.RestoreIfLost();entry.References++;return entry;
        }
        internal static void Release(Entry entry)
        {
            if(entry==null||--entry.References>0)return;
            Entries.Remove(entry.Skin);if(Application.isPlaying)Retired.Add(new RetiredSkin(entry));else entry.Brush.Dispose();
        }
    }
}
