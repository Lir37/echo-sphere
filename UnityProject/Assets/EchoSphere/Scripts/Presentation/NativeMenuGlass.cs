using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation
{
    internal sealed class NativeMenuGlass:IDisposable
    {
        private NativeEngineBackdrop _effect;
#if UNITY_EDITOR
        private LegacyMenuGlass _legacy;
#endif
        internal int PrepareCount=>0;
        internal RenderTexture Texture=>null;
        internal bool UsesRetainedPrefix=>false;
        internal NativeMenuGlass(VisualElement element,float sigma)
        {
#if UNITY_EDITOR
            if(NativeBackdropValidation.UseLegacy){_legacy=new LegacyMenuGlass(element,sigma);return;}
#endif
            _effect=new NativeEngineBackdrop(element,sigma);
        }
        internal void Prepare(VisualElement root,float sigma,float scale)
        {
#if UNITY_EDITOR
            _legacy?.Prepare(root,sigma,scale);
#endif
        }
        internal void Visible(bool visible)
        {
#if UNITY_EDITOR
            if(_legacy!=null){_legacy.Visible(visible);return;}
#endif
            _effect?.SetVisible(visible);
        }
        public void Dispose()
        {
#if UNITY_EDITOR
            _legacy?.Dispose();_legacy=null;
#endif
            _effect?.Dispose();_effect=null;
        }
    }
}
