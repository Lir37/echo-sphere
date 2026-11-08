using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Individual regional textures: coalesced async requests, explicit shared owners.
    internal static class NativeRegionBackdropAssets
    {
        internal sealed class Entry
        {
            internal int Region, Owners;
            internal ResourceRequest Request;
            internal Texture2D Texture;
        }
        internal sealed class Lease : IDisposable
        {
            private Entry _entry;
            internal Lease(Entry entry){_entry=entry;entry.Owners++;}
            internal Texture2D Texture=>_entry?.Texture;
            internal int Region=>_entry?.Region??-1;
            public void Dispose(){if(_entry==null)return;var old=_entry;_entry=null;old.Owners--;ReleaseUnused(old);}
        }
        private static readonly Dictionary<int,Entry> Entries=new Dictionary<int,Entry>();
        internal static string PathFor(int region)=>"EchoSphere/RegionBackdrops/"+CampaignRegions.SourceId((EchoRegionId)Mathf.Clamp(region,0,5));
        internal static Lease Acquire(int region)
        {
            region=Mathf.Clamp(region,0,5);
            if(!Entries.TryGetValue(region,out var entry))
            {
                entry=new Entry{Region=region};Entries.Add(region,entry);
                var lease=new Lease(entry);
                var pending=Resources.LoadAsync<Texture2D>(PathFor(region));entry.Request=pending;
                pending.completed+=_=>
                {
                    entry.Texture=pending.asset as Texture2D;entry.Request=null;
                    if(entry.Texture==null)Debug.LogWarning("Regional backdrop missing: "+PathFor(region));
                    ReleaseUnused(entry);
                };
                return lease;
            }
            return new Lease(entry);
        }
        private static void ReleaseUnused(Entry entry)
        {
            if(entry.Owners!=0||entry.Request!=null)return;
            if(Entries.TryGetValue(entry.Region,out var current)&&ReferenceEquals(current,entry))Entries.Remove(entry.Region);
            if(entry.Texture!=null)Resources.UnloadAsset(entry.Texture);
            entry.Texture=null;
        }
        internal static int OwnerCount{get{int n=0;foreach(var entry in Entries.Values)n+=entry.Owners;return n;}}
        internal static int ResidentCount{get{int n=0;foreach(var entry in Entries.Values)if(entry.Texture!=null)n++;return n;}}
    }
}
