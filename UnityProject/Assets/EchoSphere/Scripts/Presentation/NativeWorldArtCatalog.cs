using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    public interface INativeWorldSprite
    {
        Sprite Sprite {get;}
        double Width {get;}
        double Height {get;}
    }
    [CreateAssetMenu(menuName="ECHO SPHERE/World artwork catalog")]
    public sealed class NativeWorldArtCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry : INativeWorldSprite
        {
            public string Key;
            public Sprite Asset;
            public double SourceWidth,SourceHeight;
            public Sprite Sprite=>Asset;
            public double Width=>SourceWidth;
            public double Height=>SourceHeight;
        }
        public string SourceCommit;
        public List<Entry> Entries=new List<Entry>();
    }
    internal static class NativeWorldArtLibrary
    {
        private static NativeWorldArtCatalog _catalog;
        private static readonly Dictionary<string,NativeWorldArtCatalog.Entry> _entries=new Dictionary<string,NativeWorldArtCatalog.Entry>(StringComparer.Ordinal);
        private static readonly HashSet<string> _requested=new HashSet<string>(StringComparer.Ordinal);
        internal static INativeWorldSprite Resolve(string key)
        {
            if(_catalog==null){
                _catalog=Resources.Load<NativeWorldArtCatalog>("EchoSphere/NativeWorldArtCatalog");
                if(_catalog==null)throw new InvalidOperationException("Native world artwork catalog missing");
                _entries.Clear();foreach(var entry in _catalog.Entries)_entries.Add(entry.Key,entry);
            }
            // Preserve the original first-request fallback and static lifetime.
            bool first=_requested.Add(key);
            return !first&&_entries.TryGetValue(key,out var image)&&image.Sprite!=null&&image.Width>0?image:null;
        }
    }
}
