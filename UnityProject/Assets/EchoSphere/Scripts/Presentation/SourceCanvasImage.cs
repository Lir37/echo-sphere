using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Exact ART_PATHS images, rasterized by the reference Canvas implementation.
    // This retains SVG filters and Chromium's actual transparent PNG decode result.
    public sealed class SourceCanvasImage : IVisualHost
    {
        public Sprite Sprite {get;private set;}
        private string _source;
        private double _width,_height;
        public V Get(string name)
        {
            switch(name) {
                case "complete":return true;case "src":return _source;
                case "naturalWidth":case "width":return Sprite==null?0d:_width;
                case "naturalHeight":case "height":return Sprite==null?0d:_height;
                default:return default;
            }
        }
        public void Set(string name,V value)
        {
            if(name=="decoding")return;
            if(name!="src")throw new InvalidOperationException("Unknown authored image property "+name);
            _source=value.Text;
            Sprite=SourceCanvasArtIndex.Resources.TryGetValue(_source,out var resource)?UnityEngine.Resources.Load<Sprite>(resource):null;
            if(SourceCanvasArtIndex.Dimensions.TryGetValue(_source,out var dimensions)){_width=dimensions[0];_height=dimensions[1];}
        }
    }
}
