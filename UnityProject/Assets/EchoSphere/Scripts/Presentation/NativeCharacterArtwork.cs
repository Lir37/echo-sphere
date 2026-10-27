using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // User-approved roster artwork, separate from the immutable source-art catalog.
    internal static class NativeCharacterArtwork
    {
        private static readonly Dictionary<string,NativeWorldArtCatalog.Entry> Cache=new Dictionary<string,NativeWorldArtCatalog.Entry>(StringComparer.Ordinal);
        private static ResourceRequest _request;private static string _requestId;
        private static readonly Dictionary<string,int> Owners=new Dictionary<string,int>(StringComparer.Ordinal);
        internal sealed class Lease : IDisposable
        {
            private readonly HashSet<string> _ids=new HashSet<string>(StringComparer.Ordinal);
            internal INativeWorldSprite Retain(string id,bool async=false)
            {
                if(string.IsNullOrEmpty(id))return null;
                if(_ids.Add(id)){Owners.TryGetValue(id,out int count);Owners[id]=count+1;}
                return async?ResolveAsync(id):Resolve(id);
            }
            internal INativeWorldSprite Select(string id)
            {
                // Acquire the next sprite before dropping the preceding owner.
                var result=Retain(id);Trim(id);return result;
            }
            private void Trim(string keep){var release=new List<string>();foreach(var id in _ids)if(id!=keep)release.Add(id);foreach(var id in release)Release(id);}
            internal void Release(string id)
            {
                if(!_ids.Remove(id))return;
                int remaining=Owners[id]-1;if(remaining>0){Owners[id]=remaining;return;}
                Owners.Remove(id);ReleaseUnused(id);
            }
            public void Dispose(){Trim(null);}
        }
        private static void ReleaseUnused(string id)
        {
            if(Owners.ContainsKey(id)||!Cache.TryGetValue(id,out var entry))return;
            Cache.Remove(id);if(entry?.Asset==null)return;
            // These portraits are individual Resources PNGs, not shared atlases.
            // All production consumers hold a Lease while their image is used.
            var texture=entry.Asset.texture;Resources.UnloadAsset(entry.Asset);
            if(texture!=null)Resources.UnloadAsset(texture);
        }
        internal static INativeWorldSprite ResolveAsync(string id)
        {
            if(_request!=null&&_request.isDone){
                var sprite=_request.asset as Sprite;
                Cache[_requestId]=sprite==null?null:new NativeWorldArtCatalog.Entry {Key=_requestId,Asset=sprite,SourceWidth=sprite.rect.width,SourceHeight=sprite.rect.height};
                _request=null;_requestId=null;
            }
            if(string.IsNullOrEmpty(id))return null;
            if(Cache.TryGetValue(id,out var entry))return entry;
            // A single outstanding upload bounds first-visit pressure. The
            // caller requests visible cards first, then the scroll neighbours.
            if(_request==null){_requestId=id;_request=Resources.LoadAsync<Sprite>("EchoSphere/CharacterArt/"+id);
                _request.completed+=operation=>{var request=(ResourceRequest)operation;if(!ReferenceEquals(_request,request))return;var completedId=_requestId;var sprite=request.asset as Sprite;
                    Cache[completedId]=sprite==null?null:new NativeWorldArtCatalog.Entry {Key=completedId,Asset=sprite,SourceWidth=sprite.rect.width,SourceHeight=sprite.rect.height};
                    _request=null;_requestId=null;ReleaseUnused(completedId);};}
            return null;
        }
        internal static INativeWorldSprite Resolve(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            if(Cache.TryGetValue(id,out var entry))return entry;
            var sprite=Resources.Load<Sprite>("EchoSphere/CharacterArt/"+id);
            if(sprite==null)return null;
            entry=new NativeWorldArtCatalog.Entry {Key=id,Asset=sprite,SourceWidth=sprite.rect.width,SourceHeight=sprite.rect.height};
            Cache.Add(id,entry);return entry;
        }
    }
}
