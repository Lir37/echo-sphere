using EchoSphere;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Capture appearance once; query only paint influencing each blur region.
    // Structure/resource changes invalidate all copies, including off-panel ones.
    internal static class NativeMenuPaintVersion
    {
        private const ulong Basis=14695981039346656037UL;
        private struct PaintNode {internal VisualElement Element;internal Rect Coverage;internal ulong Hash;internal bool Paint,Unbounded;}
        private sealed class Version {internal int Frame=-1,LayoutFrame=-1;internal ulong Hash,LayoutHash,Ownership;internal uint Paint;internal readonly List<PaintNode> Nodes=new List<PaintNode>();}
        private static readonly ConditionalWeakTable<VisualElement,Version> Roots=new ConditionalWeakTable<VisualElement,Version>();
        private static ulong Mix(ulong hash,ulong value){unchecked{return (hash^value)*1099511628211UL;}}
        internal static ulong Read(VisualElement root)=>Capture(root,true).Hash;
        internal static ulong ReadLayout(VisualElement root)=>Capture(root,false).LayoutHash;
        internal static ulong ReadLayoutPrefix(VisualElement root,VisualElement stop,float sigma)
        {
            var version=Capture(root,false);var area=SourceUiBackdrop.WorldBounds(stop);float margin=Mathf.Ceil(4*sigma);
            var transform=root.worldTransform;float mx=margin*(Mathf.Abs(transform.m00)+Mathf.Abs(transform.m01)),my=margin*(Mathf.Abs(transform.m10)+Mathf.Abs(transform.m11));
            area.xMin-=mx;area.xMax+=mx;area.yMin-=my;area.yMax+=my;
            ulong hash=Mix(version.Ownership,(uint)sigma.GetHashCode());
            foreach(var node in version.Nodes){
                if(ReferenceEquals(node.Element,stop))return Mix(hash,node.Hash);
                if(node.Paint&&(node.Unbounded||node.Coverage.Overlaps(area)))hash=Mix(hash,node.Hash);
            }
            return version.LayoutHash;
        }
        private static Version Capture(VisualElement root,bool pixels)
        {
            var value=Roots.GetValue(root,_=>new Version());
            if(pixels&&value.Frame==Time.frameCount&&value.Paint==NativeHudSurface.PaintVersion||!pixels&&value.LayoutFrame==Time.frameCount)return value;
            ulong hash=Basis,ownership=Basis;if(!pixels)value.Nodes.Clear();
            void Visit(VisualElement e,ulong ancestors,bool filtered)
            {
                if(e.userData is SourceUiBackdrop.ClipTag||ReferenceEquals(e.userData,SourceUiBackdrop.ReplicaTag))return;
                var s=e.resolvedStyle;bool hidden=s.display==DisplayStyle.None||s.visibility==Visibility.Hidden||s.opacity<=0;
                ulong own=Fingerprint(e,s,pixels);hash=Mix(hash,own);
                if(!pixels){
                    ownership=Mix(ownership,(uint)e.GetHashCode());ownership=Mix(ownership,(uint)e.hierarchy.childCount);ownership=Mix(ownership,hidden?1UL:0UL);
                    ownership=Mix(ownership,(uint)s.backgroundImage.GetHashCode());ownership=Mix(ownership,(uint)s.unityMaterial.GetHashCode());ownership=Mix(ownership,(uint)(e.generateVisualContent?.GetHashCode()??0));
                    if(e is Image image){ownership=Mix(ownership,(uint)(image.image==null?0:image.image.ExistingObjectId()));ownership=Mix(ownership,(uint)(image.sprite==null?0:image.sprite.ExistingObjectId()));ownership=Mix(ownership,(uint)(image.vectorImage==null?0:image.vectorImage.ExistingObjectId()));}
                    if(e is TextElement){ownership=Mix(ownership,(uint)s.unityFontDefinition.GetHashCode());ownership=Mix(ownership,(uint)(s.unityFont==null?0:s.unityFont.ExistingObjectId()));}
                    if(s.filter!=null)foreach(var filter in s.filter)ownership=Mix(ownership,(uint)filter.GetHashCode());
                }
                if(hidden)return;
                ulong inherited=Mix(ancestors,own);bool hasFilter=false;if(s.filter!=null)foreach(var unused in s.filter){hasFilter=true;break;}
                if(!pixels){
                    bool custom=!(e is Image)&&!(e is TextElement)&&e.generateVisualContent!=null;
                    var coverage=e.worldBound;
                    if(e is TextElement){var shadow=e.style.textShadow.value;float reach=32+s.unityTextOutlineWidth+(shadow.color.a>0?shadow.blurRadius*2+Mathf.Max(Mathf.Abs(shadow.offset.x),Mathf.Abs(shadow.offset.y)):0);var matrix=e.worldTransform;float rx=reach*(Mathf.Abs(matrix.m00)+Mathf.Abs(matrix.m01)),ry=reach*(Mathf.Abs(matrix.m10)+Mathf.Abs(matrix.m11));coverage.xMin-=rx;coverage.xMax+=rx;coverage.yMin-=ry;coverage.yMax+=ry;}
                    var bg=s.backgroundImage;
                    bool paints=e is Image||e is TextElement||custom||s.backgroundColor.a>0||bg.texture!=null||bg.renderTexture!=null||bg.sprite!=null||bg.vectorImage!=null||s.borderLeftWidth>0&&s.borderLeftColor.a>0||s.borderRightWidth>0&&s.borderRightColor.a>0||s.borderTopWidth>0&&s.borderTopColor.a>0||s.borderBottomWidth>0&&s.borderBottomColor.a>0;
                    value.Nodes.Add(new PaintNode {Element=e,Coverage=coverage,Hash=inherited,Paint=paints,Unbounded=filtered||hasFilter||custom});
                }
                for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i],inherited,filtered||hasFilter);
            }
            Visit(root,Basis,false);
            if(pixels){value.Frame=Time.frameCount;value.Paint=NativeHudSurface.PaintVersion;value.Hash=Mix(hash,NativeHudSurface.PaintVersion);}else{value.LayoutFrame=Time.frameCount;value.LayoutHash=hash;value.Ownership=ownership;}
            return value;
        }
        private static ulong Fingerprint(VisualElement e,IResolvedStyle s,bool pixels)
        {
            ulong hash=Basis;void Add(int n){hash=Mix(hash,(uint)n);}
                Add(e.GetHashCode());Add(e.enabledSelf?1:0);Add((int)s.display);Add((int)s.visibility);Add(s.opacity.GetHashCode());
                if(s.display==DisplayStyle.None||s.visibility==Visibility.Hidden||s.opacity<=0)return hash;
                Add(e.worldTransform.GetHashCode());Add(e.layout.GetHashCode());Add(s.backgroundColor.GetHashCode());Add(s.backgroundImage.GetHashCode());Add(s.unityBackgroundImageTintColor.GetHashCode());
                Add(s.borderLeftColor.GetHashCode());Add(s.borderRightColor.GetHashCode());Add(s.borderTopColor.GetHashCode());Add(s.borderBottomColor.GetHashCode());
                Add(s.borderLeftWidth.GetHashCode());Add(s.borderRightWidth.GetHashCode());Add(s.borderTopWidth.GetHashCode());Add(s.borderBottomWidth.GetHashCode());
                Add(s.borderTopLeftRadius.GetHashCode());Add(s.borderTopRightRadius.GetHashCode());Add(s.borderBottomLeftRadius.GetHashCode());Add(s.borderBottomRightRadius.GetHashCode());Add(e.style.overflow.GetHashCode());
                if(s.filter!=null)foreach(var filter in s.filter)Add(filter.GetHashCode());
                if(!pixels){Add(s.paddingLeft.GetHashCode());Add(s.paddingRight.GetHashCode());Add(s.paddingTop.GetHashCode());Add(s.paddingBottom.GetHashCode());Add(s.backgroundPositionX.GetHashCode());Add(s.backgroundPositionY.GetHashCode());Add(s.backgroundRepeat.GetHashCode());Add(s.backgroundSize.GetHashCode());Add(s.unitySliceLeft);Add(s.unitySliceRight);Add(s.unitySliceTop);Add(s.unitySliceBottom);Add(s.unitySliceScale.GetHashCode());Add(s.unitySliceType.GetHashCode());Add(s.unityMaterial.GetHashCode());}
                if(e is TextElement text){Add(text.text?.GetHashCode()??0);Add(text.enableRichText?1:0);Add(s.color.GetHashCode());Add(s.fontSize.GetHashCode());Add(s.unityFontDefinition.GetHashCode());Add((int)s.unityFontStyleAndWeight);Add(s.letterSpacing.GetHashCode());Add(s.wordSpacing.GetHashCode());Add(e.style.textShadow.value.GetHashCode());Add(s.unityTextOutlineColor.GetHashCode());Add(s.unityTextOutlineWidth.GetHashCode());if(!pixels){Add(s.unityFont==null?0:s.unityFont.ExistingObjectId());Add(s.unityTextAlign.GetHashCode());Add(s.whiteSpace.GetHashCode());Add(s.unityParagraphSpacing.GetHashCode());Add(s.unityTextGenerator.GetHashCode());Add(s.textOverflow.GetHashCode());Add(s.unityTextOverflowPosition.GetHashCode());}}
                else if(e is Image image){Add(image.image==null?0:image.image.ExistingObjectId());Add(image.sprite==null?0:image.sprite.ExistingObjectId());Add(image.vectorImage==null?0:image.vectorImage.ExistingObjectId());Add(image.uv.GetHashCode());Add(image.tintColor.GetHashCode());Add((int)image.scaleMode);if(pixels&&image.image!=null)Add((int)image.image.updateCount);}
                else if(e.generateVisualContent!=null)Add(pixels?Time.frameCount:e.generateVisualContent.GetHashCode());
            return hash;
        }
    }
}

#endif
