using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // User-approved roster artwork, separate from the immutable source-art catalog.
    internal static class NativeCharacterArtwork
    {
        private static readonly Dictionary<string,NativeWorldArtCatalog.Entry> Cache=new Dictionary<string,NativeWorldArtCatalog.Entry>(StringComparer.Ordinal);
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
