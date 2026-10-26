using EchoSphere;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoSphere.Presentation
{
    // Final alpha masks only, never UI filter allocations. Admit a shape after
    // an exact repeated input; moving/one-shot paths do not allocate GPU entries.
    internal sealed class NativeWorldShadowCache : IDisposable
    {
        internal enum Result { Miss, Write, Hit }
        internal readonly struct Key : IEquatable<Key>
        {
            internal readonly Mesh Mesh;private readonly ulong _version;
            private readonly Matrix4x4 _matrix;private readonly Rect _viewport;private readonly RectInt _area;
            private readonly float _alpha,_opacity,_blur;private readonly int _hash;
            internal Key(Mesh mesh,ulong version,Matrix4x4 matrix,Rect viewport,RectInt area,float alpha,float opacity,float blur)
            {Mesh=mesh;_version=version;_matrix=matrix;_viewport=viewport;_area=area;_alpha=alpha;_opacity=opacity;_blur=blur;var h=new HashCode();h.Add(mesh.ExistingObjectId());h.Add(version);h.Add(matrix);h.Add(viewport);h.Add(area);h.Add(alpha);h.Add(opacity);h.Add(blur);_hash=h.ToHashCode();}
            public override int GetHashCode()=>_hash;
            public override bool Equals(object other)=>other is Key key&&Equals(key);
            public bool Equals(Key other)=>ReferenceEquals(Mesh,other.Mesh)&&_version==other._version&&_matrix.Equals(other._matrix)&&_viewport.Equals(other._viewport)&&_area.Equals(other._area)&&_alpha.Equals(other._alpha)&&_opacity.Equals(other._opacity)&&_blur.Equals(other._blur);
        }
        private sealed class Entry {internal RenderTexture Texture;internal long Bytes;internal ulong Used;}
        private const long Budget=16*1024*1024,MaxEntry=1024*1024;
        private const int MaxEntries=128,MaxCandidates=4096;
        private readonly Dictionary<Key,Entry> _entries=new Dictionary<Key,Entry>();
        private readonly List<Key> _stale=new List<Key>();
        private HashSet<Key> _previous=new HashSet<Key>(),_current=new HashSet<Key>();private ulong _frame;
        internal long ResidentBytes {get;private set;}
        internal int Count=>_entries.Count;
        internal void Begin()
        {
            _frame++;var swap=_previous;_previous=_current;_current=swap;_current.Clear();_stale.Clear();
            ulong age=_entries.Count>=MaxEntries||ResidentBytes>Budget-MaxEntry?1UL:120UL;
            foreach(var pair in _entries)if(pair.Key.Mesh==null||!pair.Value.Texture.IsCreated()||_frame-pair.Value.Used>age)_stale.Add(pair.Key);
            // Evict BEFORE recording this frame: no queued command can refer to
            // a released entry from an earlier primitive in the same frame.
            foreach(var key in _stale){var entry=_entries[key];Release(entry.Texture);ResidentBytes-=entry.Bytes;_entries.Remove(key);}
        }
        internal Result Probe(Key key,int width,int height,out RenderTexture texture)
        {
            if(_entries.TryGetValue(key,out var entry)){entry.Used=_frame;texture=entry.Texture;return Result.Hit;}
            texture=null;long bytes=(long)width*height*4;
            if(bytes>MaxEntry||width<=0||height<=0||(SystemInfo.copyTextureSupport&CopyTextureSupport.Basic)==0)return Result.Miss;
            bool repeat=_previous.Contains(key)||_current.Contains(key);if(_current.Count<MaxCandidates)_current.Add(key);
            if(!repeat||_entries.Count>=MaxEntries||ResidentBytes+bytes>Budget)return Result.Miss;
            texture=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Retained native world shadow",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            if(!texture.Create()){Release(texture);texture=null;return Result.Miss;}
            _entries.Add(key,new Entry {Texture=texture,Bytes=bytes,Used=_frame});ResidentBytes+=bytes;return Result.Write;
        }
        private static void Release(RenderTexture texture){texture.Release();if(Application.isPlaying)UnityEngine.Object.Destroy(texture);else UnityEngine.Object.DestroyImmediate(texture);}
        public void Dispose(){foreach(var entry in _entries.Values)Release(entry.Texture);_entries.Clear();_previous.Clear();_current.Clear();_stale.Clear();ResidentBytes=0;}
    }
}
