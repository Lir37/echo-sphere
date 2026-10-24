using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // One appearance traversal per root/frame, shared by every glass button.
    // Helpers are outputs, never inputs: including them would invalidate their
    // own cache and would recursively copy backdrop replicas.
    internal static class NativeMenuPaintVersion
    {
        private sealed class Version {internal int Frame=-1;internal ulong Hash;internal uint Paint;}
        private static readonly ConditionalWeakTable<VisualElement,Version> Roots=new ConditionalWeakTable<VisualElement,Version>();
        internal static ulong Read(VisualElement root)
        {
            var value=Roots.GetValue(root,_=>new Version());
            if(value.Frame==Time.frameCount&&value.Paint==NativeHudSurface.PaintVersion)return value.Hash;
            ulong hash=14695981039346656037UL;
            void Add(int number){unchecked{hash=(hash^(uint)number)*1099511628211UL;}}
            void Visit(VisualElement e)
            {
                if(e.userData is SourceUiBackdrop.ClipTag||ReferenceEquals(e.userData,SourceUiBackdrop.ReplicaTag))return;
                var s=e.resolvedStyle;Add(e.GetHashCode());Add((int)s.display);Add((int)s.visibility);Add(s.opacity.GetHashCode());
                if(s.display==DisplayStyle.None||s.visibility==Visibility.Hidden||s.opacity<=0)return;
                Add(e.worldTransform.GetHashCode());Add(e.layout.GetHashCode());Add(s.backgroundColor.GetHashCode());Add(s.backgroundImage.GetHashCode());Add(s.unityBackgroundImageTintColor.GetHashCode());
                Add(s.borderLeftColor.GetHashCode());Add(s.borderRightColor.GetHashCode());Add(s.borderTopColor.GetHashCode());Add(s.borderBottomColor.GetHashCode());
                Add(s.borderLeftWidth.GetHashCode());Add(s.borderRightWidth.GetHashCode());Add(s.borderTopWidth.GetHashCode());Add(s.borderBottomWidth.GetHashCode());
                Add(s.borderTopLeftRadius.GetHashCode());Add(s.borderTopRightRadius.GetHashCode());Add(s.borderBottomLeftRadius.GetHashCode());Add(s.borderBottomRightRadius.GetHashCode());Add(e.style.overflow.GetHashCode());
                if(s.filter!=null)foreach(var filter in s.filter)Add(filter.GetHashCode());
                if(e is TextElement text){Add(text.text?.GetHashCode()??0);Add(s.color.GetHashCode());Add(s.fontSize.GetHashCode());Add(s.unityFontDefinition.GetHashCode());Add((int)s.unityFontStyleAndWeight);Add(s.letterSpacing.GetHashCode());Add(s.wordSpacing.GetHashCode());Add(e.style.textShadow.value.GetHashCode());Add(s.unityTextOutlineColor.GetHashCode());Add(s.unityTextOutlineWidth.GetHashCode());}
                else if(e is Image image){Add(image.image==null?0:image.image.GetInstanceID());Add(image.sprite==null?0:image.sprite.GetInstanceID());Add(image.vectorImage==null?0:image.vectorImage.GetInstanceID());Add(image.uv.GetHashCode());Add(image.tintColor.GetHashCode());Add((int)image.scaleMode);if(image.image!=null)Add((int)image.image.updateCount);}
                else if(e.generateVisualContent!=null)Add(Time.frameCount); // Unknown animated native painter.
                for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i]);
            }
            Visit(root);Add((int)NativeHudSurface.PaintVersion);value.Frame=Time.frameCount;value.Paint=NativeHudSurface.PaintVersion;value.Hash=hash;return hash;
        }
    }
}
