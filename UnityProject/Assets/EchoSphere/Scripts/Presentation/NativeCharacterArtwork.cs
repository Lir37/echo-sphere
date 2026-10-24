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
            if(_request==null){_requestId=id;_request=Resources.LoadAsync<Sprite>("EchoSphere/CharacterArt/"+id);}
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
