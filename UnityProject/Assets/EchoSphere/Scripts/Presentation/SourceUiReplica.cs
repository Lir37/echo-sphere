using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A read-only retained replica of the paint prefix. It shares prepared
    // textures/fonts with the live UI; it owns no GPU resources or input hooks.
    internal sealed class SourceUiReplica : IDisposable
    {
        internal sealed class PaintTag { internal Action<MeshGenerationContext> Paint; }
        private sealed class Copy { internal VisualElement Element;internal IStyle Style;internal Action<MeshGenerationContext> TextPaint,Painter;internal readonly PaintTag Tag=new PaintTag();internal bool Used,HasText; }
        private readonly Dictionary<VisualElement,Copy> _copies=new Dictionary<VisualElement,Copy>();
        private readonly Dictionary<VisualElement,int> _childOrder=new Dictionary<VisualElement,int>();
        private readonly Func<VisualElement,Action<MeshGenerationContext>> _paint;
        private readonly SourceUiReplicaSnapshotCache _snapshots;
        internal SourceUiReplica(Func<VisualElement,Action<MeshGenerationContext>> paint):this(paint,null){}
        internal SourceUiReplica(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots){_paint=paint;_snapshots=snapshots;}
        internal void Update(VisualElement root,VisualElement stop,VisualElement parent,Rect area)
        {
            _childOrder.Clear();
            foreach(var copy in _copies.Values)copy.Used=false;
            void CopyNode(VisualElement source,VisualElement container,bool isRoot=false)
            {
                if(!_copies.TryGetValue(source,out var copy)){
                    var element=source is TextElement?(VisualElement)new Label():new VisualElement();
                    copy=new Copy {Element=element,Style=new SourceUiReplicaStyle(element.style),TextPaint=element.generateVisualContent};_copies.Add(source,copy);
                    var retained=copy;copy.Painter=context=>{retained.Tag.Paint?.Invoke(context);if(retained.HasText)retained.TextPaint?.Invoke(context);};
                    element.pickingMode=PickingMode.Ignore;element.focusable=false;
                    element.name=source.name;foreach(var name in source.GetClasses())element.AddToClassList(name);
                }
                copy.Used=true;var target=copy.Element;var style=copy.Style;target.SetEnabled(source.enabledSelf);
                var helper=source.userData==SourceUiBackdrop.ReplicaTag||source.userData is SourceUiBackdrop.ClipTag||source.userData is PaintTag;
                if(helper){if(_snapshots==null)CopyInlineGeometry(source,style);else _snapshots.Apply(source,style,true);}
                else {
                    if(_snapshots==null)CopyAppearance(source,style);else _snapshots.Apply(source,style,false);
                    var bounds=source.layout;var border=source.parent?.resolvedStyle;
                    SourceUiBackdrop.LocalBox(style,isRoot?0:bounds.x-(border?.borderLeftWidth??0),isRoot?0:bounds.y-(border?.borderTopWidth??0),bounds.width,bounds.height);
                    var transform=source.resolvedStyle;style.translate=new Translate(transform.translate.x,transform.translate.y,transform.translate.z);style.rotate=transform.rotate;style.scale=transform.scale;style.transformOrigin=new TransformOrigin(transform.transformOrigin.x,transform.transformOrigin.y,transform.transformOrigin.z);
                }
                if(isRoot){style.translate=new Translate(0,0);style.rotate=new Rotate(0);style.scale=new Scale(Vector3.one);style.opacity=1;style.filter=StyleKeyword.Null;}
                if(target is TextElement text&&source is TextElement original){text.text=original.text;text.enableRichText=original.enableRichText;text.selection.isSelectable=false;}
                copy.Tag.Paint=_paint(source);copy.HasText=source.generateVisualContent!=null&&copy.TextPaint!=null;
                target.userData=source.userData is SourceUiBackdrop.ClipTag?source.userData:copy.Tag;
                target.generateVisualContent=copy.Tag.Paint==null&&!copy.HasText?null:copy.Painter;
                // Preserve the paint-prefix order without rotating every retained
                // sibling through the end of its hierarchy on every frame.
                _childOrder.TryGetValue(container,out var childIndex);
                if(target.parent!=container||container.hierarchy.IndexOf(target)!=childIndex)
                    container.hierarchy.Insert(childIndex,target);
                _childOrder[container]=childIndex+1;
                target.MarkDirtyRepaint();
            }
            // Stop BEFORE the target (its background, foreground, descendants
            // and this replica itself). The nearest backdrop root excludes its
            // own backdrop image and outer filter/opacity; those belong to the
            // live ancestor group. Use actual retained hierarchy paint order.
            // Explicit inline display:none suppresses the entire subtree in Unity.
            // Read the current inline value, not last frame's resolved layout.
            foreach(var item in VisualBackdrop.PaintPrefix(root,stop,element=>element.hierarchy.childCount,(element,index)=>element.hierarchy[index],
                element=>element.style.display==DisplayStyle.None||element.userData is SourceUiBackdrop.ClipTag clip&&(clip.Owner==root||!SourceUiBackdrop.WorldBounds(clip.Owner).Overlaps(area))))
                CopyNode(item.Node,item.Parent==null?parent:_copies[item.Parent].Element,item.Parent==null);
            foreach(var item in _copies.Where(pair=>!pair.Value.Used).ToArray()){item.Value.Element.RemoveFromHierarchy();_copies.Remove(item.Key);}
        }
        internal static void Appearance(VisualElement source,VisualElement target)
            =>CopyAppearance(source,target.style);
        internal static void CopyAppearance(VisualElement source,IStyle style)
        {
            var value=source.resolvedStyle;
            style.display=value.display;style.visibility=value.visibility;style.opacity=value.opacity;style.filter=source.style.filter;style.overflow=source.style.overflow;
            style.backgroundColor=value.backgroundColor;style.backgroundImage=value.backgroundImage;style.backgroundPositionX=value.backgroundPositionX;style.backgroundPositionY=value.backgroundPositionY;style.backgroundRepeat=value.backgroundRepeat;style.backgroundSize=value.backgroundSize;
            style.borderLeftWidth=value.borderLeftWidth;style.borderRightWidth=value.borderRightWidth;style.borderTopWidth=value.borderTopWidth;style.borderBottomWidth=value.borderBottomWidth;
            style.borderLeftColor=value.borderLeftColor;style.borderRightColor=value.borderRightColor;style.borderTopColor=value.borderTopColor;style.borderBottomColor=value.borderBottomColor;
            style.borderTopLeftRadius=value.borderTopLeftRadius;style.borderTopRightRadius=value.borderTopRightRadius;style.borderBottomLeftRadius=value.borderBottomLeftRadius;style.borderBottomRightRadius=value.borderBottomRightRadius;
            style.paddingLeft=value.paddingLeft;style.paddingRight=value.paddingRight;style.paddingTop=value.paddingTop;style.paddingBottom=value.paddingBottom;
            style.color=value.color;style.fontSize=value.fontSize;style.letterSpacing=value.letterSpacing;style.wordSpacing=value.wordSpacing;style.whiteSpace=value.whiteSpace;
            style.unityFont=value.unityFont;style.unityFontDefinition=value.unityFontDefinition;style.unityFontStyleAndWeight=value.unityFontStyleAndWeight;style.unityTextAlign=value.unityTextAlign;
            style.unityParagraphSpacing=value.unityParagraphSpacing;style.unityTextGenerator=value.unityTextGenerator;style.unityTextOutlineColor=value.unityTextOutlineColor;style.unityTextOutlineWidth=value.unityTextOutlineWidth;style.textShadow=source.style.textShadow;style.textOverflow=value.textOverflow;style.unityTextOverflowPosition=value.unityTextOverflowPosition;
            style.unityBackgroundImageTintColor=value.unityBackgroundImageTintColor;style.unitySliceLeft=value.unitySliceLeft;style.unitySliceRight=value.unitySliceRight;style.unitySliceTop=value.unitySliceTop;style.unitySliceBottom=value.unitySliceBottom;style.unitySliceScale=value.unitySliceScale;style.unitySliceType=value.unitySliceType;style.unityMaterial=value.unityMaterial;
        }
        private static void InlineGeometry(VisualElement source,VisualElement target)
            =>CopyInlineGeometry(source,target.style);
        internal static void CopyInlineGeometry(VisualElement source,IStyle style)
        {
            // Helper styles were prepared this frame, before Yoga's next pass.
            // Reading helper.layout here would introduce a one-frame blur lag.
            var value=source.style;
            style.position=value.position;style.left=value.left;style.top=value.top;style.right=value.right;style.bottom=value.bottom;style.width=value.width;style.height=value.height;
            style.minWidth=value.minWidth;style.minHeight=value.minHeight;style.maxWidth=value.maxWidth;style.maxHeight=value.maxHeight;style.aspectRatio=value.aspectRatio;style.flexShrink=0;
            style.marginLeft=style.marginRight=style.marginTop=style.marginBottom=0;
            style.translate=value.translate;style.rotate=value.rotate;style.scale=value.scale;style.transformOrigin=value.transformOrigin;
            // Also use this frame's values for the retained Gaussian/filter chain.
            style.filter=value.filter;style.overflow=value.overflow;style.opacity=value.opacity;style.display=value.display;
            style.visibility=value.visibility;
            style.backgroundColor=value.backgroundColor;style.backgroundImage=value.backgroundImage;style.backgroundPositionX=value.backgroundPositionX;style.backgroundPositionY=value.backgroundPositionY;style.backgroundRepeat=value.backgroundRepeat;style.backgroundSize=value.backgroundSize;
            style.borderLeftWidth=value.borderLeftWidth;style.borderRightWidth=value.borderRightWidth;style.borderTopWidth=value.borderTopWidth;style.borderBottomWidth=value.borderBottomWidth;
            style.borderLeftColor=value.borderLeftColor;style.borderRightColor=value.borderRightColor;style.borderTopColor=value.borderTopColor;style.borderBottomColor=value.borderBottomColor;
            style.borderTopLeftRadius=value.borderTopLeftRadius;style.borderTopRightRadius=value.borderTopRightRadius;style.borderBottomLeftRadius=value.borderBottomLeftRadius;style.borderBottomRightRadius=value.borderBottomRightRadius;
            style.paddingLeft=value.paddingLeft;style.paddingRight=value.paddingRight;style.paddingTop=value.paddingTop;style.paddingBottom=value.paddingBottom;
            style.color=value.color;style.fontSize=value.fontSize;style.letterSpacing=value.letterSpacing;style.wordSpacing=value.wordSpacing;style.whiteSpace=value.whiteSpace;
            style.unityFont=value.unityFont;style.unityFontDefinition=value.unityFontDefinition;style.unityFontStyleAndWeight=value.unityFontStyleAndWeight;style.unityTextAlign=value.unityTextAlign;
            style.unityParagraphSpacing=value.unityParagraphSpacing;style.unityTextGenerator=value.unityTextGenerator;style.unityTextOutlineColor=value.unityTextOutlineColor;style.unityTextOutlineWidth=value.unityTextOutlineWidth;style.textShadow=value.textShadow;style.textOverflow=value.textOverflow;style.unityTextOverflowPosition=value.unityTextOverflowPosition;
            style.unityBackgroundImageTintColor=value.unityBackgroundImageTintColor;style.unitySliceLeft=value.unitySliceLeft;style.unitySliceRight=value.unitySliceRight;style.unitySliceTop=value.unitySliceTop;style.unitySliceBottom=value.unitySliceBottom;style.unitySliceScale=value.unitySliceScale;style.unitySliceType=value.unitySliceType;style.unityMaterial=value.unityMaterial;
        }
        public void Dispose(){foreach(var copy in _copies.Values)copy.Element.RemoveFromHierarchy();_copies.Clear();}
    }
}
