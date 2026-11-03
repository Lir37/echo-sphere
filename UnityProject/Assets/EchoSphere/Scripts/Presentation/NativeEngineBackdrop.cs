using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
namespace EchoSphere.Presentation
{
    // Unity owns capture, clipping, dependency ordering and temporary textures.
    // This owner only attaches/restores a native style for the lifetime of a view.
    internal sealed class NativeEngineBackdrop:IDisposable
    {
        private static readonly HashSet<NativeEngineBackdrop> Live=new HashSet<NativeEngineBackdrop>();
        private readonly VisualElement _element;
        private readonly StyleList<FilterFunction> _original,_effect;
        private bool _enabled=true,_requested=true,_disposed;
        private readonly float _sigma;
#if UNITY_EDITOR
        internal static bool DisablePairedSamplingForValidation=false;
#endif
        internal NativeEngineBackdrop(VisualElement element,float sigma)
        {
            _element=element??throw new ArgumentNullException(nameof(element));
            if(!float.IsFinite(sigma)||sigma<0)throw new ArgumentOutOfRangeException(nameof(sigma));
            _original=element.style.backdropFilter;
            _sigma=sigma;
            var blur=new FilterFunction(FilterFunctionType.Blur);blur.AddParameter(new FilterParameter(sigma));
            _effect=new List<FilterFunction>{blur};element.style.backdropFilter=_effect;Live.Add(this);
        }
        // Unity still executes backdrop passes on retained hidden overlays.
        // Synchronize after all screens have applied their visibility for this
        // frame, including early-return menu paths and nested modal contents.
        internal static void RefreshAllVisibility(){foreach(var effect in Live)effect.RefreshVisibility();}
        internal void SetVisible(bool visible){if(_disposed)return;_requested=visible;RefreshVisibility();}
        private void RefreshVisibility()
        {
            bool visible=_requested&&ActuallyVisible(_element);if(_enabled==visible)return;
            _enabled=visible;_element.style.backdropFilter=visible?_effect:_original;
        }
        private static bool ActuallyVisible(VisualElement element)
        {
            if(element.panel==null)return false;
            bool visibilitySpecified=false;
            for(var node=element;node!=null;node=node.parent){
                // Inline changes have not necessarily reached resolvedStyle yet.
                var display=node.style.display;var opacity=node.style.opacity;
                if((display.keyword==StyleKeyword.Undefined?display.value:node.resolvedStyle.display)==DisplayStyle.None)return false;
                // Keep the filter during a visible fade to zero; attaching it
                // at the start of a fade from zero must not wait another frame.
                if(node.resolvedStyle.opacity<=0&&(opacity.keyword!=StyleKeyword.Undefined||opacity.value<=0))return false;
                if(!visibilitySpecified&&node.style.visibility.keyword==StyleKeyword.Undefined){
                    if(node.style.visibility.value==Visibility.Hidden)return false;visibilitySpecified=true;
                }
            }
            return visibilitySpecified||element.resolvedStyle.visibility!=Visibility.Hidden;
        }
        public void Dispose(){if(_disposed)return;_disposed=true;Live.Remove(this);_element.style.backdropFilter=_original;}
    }
#if UNITY_EDITOR
    public static class NativeBackdropValidation
    {
        public static bool UseLegacy;
    }
#endif
}
