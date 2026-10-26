#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A read-only retained replica of the paint prefix. It shares prepared
    // textures/fonts with the live UI; it owns no GPU resources or input hooks.
    internal sealed class SourceUiReplica : IDisposable
    {
        internal sealed class PaintTag { internal Action<MeshGenerationContext> Paint; }
        private sealed class Copy { internal VisualElement Element,FlatFill;internal IStyle Style;internal Action<MeshGenerationContext> TextPaint,Painter;internal readonly PaintTag Tag=new PaintTag();internal bool Used,HasText; }
        private readonly Dictionary<VisualElement,Copy> _copies=new Dictionary<VisualElement,Copy>();
        private readonly Dictionary<VisualElement,int> _childOrder=new Dictionary<VisualElement,int>();
        private readonly List<VisualElement> _staleSources=new List<VisualElement>();
        private readonly Func<VisualElement,Action<MeshGenerationContext>> _paint;
        private readonly SourceUiReplicaSnapshotCache _snapshots;
        private readonly bool _boundToArea;
#if UNITY_EDITOR
        internal static bool DisableBoundsForValidation=false;
#endif
        private static bool HasFilter(VisualElement e){if(e.resolvedStyle.filter!=null)foreach(var unused in e.resolvedStyle.filter)return true;return false;}
        private static bool SimpleTransform(VisualElement e)
        {
            var s=e.resolvedStyle;var m=e.worldTransform;
            return m.m00>0&&m.m11>0&&Mathf.Abs(m.m01)<1e-6f&&Mathf.Abs(m.m10)<1e-6f&&Mathf.Abs(m.m20)+Mathf.Abs(m.m21)+Mathf.Abs(m.m30)+Mathf.Abs(m.m31)<1e-6f&&
                s.rotate.angle.value==0&&s.scale.value==Vector3.one;
        }
        private bool Outside(VisualElement e,Rect area)
        {
            if(e.hierarchy.childCount!=0||HasFilter(e))return false;
            if(e is Image bitmap&&bitmap.image==null||e.resolvedStyle.backgroundImage.vectorImage!=null)return false;
            var bounds=e.worldBound;float x=1,y=1;
            if(e is TextElement text){
                if(text.enableRichText&&text.text!=null&&text.text.Contains('<'))return false;
                var s=e.resolvedStyle;var shadow=e.style.textShadow.value;
                y=Mathf.Max(32,s.fontSize*4)+s.unityTextOutlineWidth+shadow.blurRadius*4+Mathf.Abs(shadow.offset.y);
                x=Mathf.Max(y,(text.text?.Length??0)*s.fontSize*4)+Mathf.Abs(shadow.offset.x);
            }else if(_paint(e)!=null)return false;
            var m=e.worldTransform;float dx=x*Mathf.Abs(m.m00)+y*Mathf.Abs(m.m01),dy=x*Mathf.Abs(m.m10)+y*Mathf.Abs(m.m11);
            bounds.xMin-=dx;bounds.xMax+=dx;bounds.yMin-=dy;bounds.yMax+=dy;return !bounds.Overlaps(area);
        }
        private static bool Unpainted(VisualElement source)
        {
            var s=source.resolvedStyle;var b=s.backgroundImage;
            return !(source is TextElement)&&!(source is Image)&&source.generateVisualContent==null&&s.backgroundColor.a==0&&b.texture==null&&b.renderTexture==null&&b.sprite==null&&b.vectorImage==null&&
                (s.borderLeftWidth==0||s.borderLeftColor.a==0)&&(s.borderRightWidth==0||s.borderRightColor.a==0)&&(s.borderTopWidth==0||s.borderTopColor.a==0)&&(s.borderBottomWidth==0||s.borderBottomColor.a==0)&&source.style.overflow.keyword==StyleKeyword.Undefined&&source.style.overflow.value==Overflow.Visible&&!HasFilter(source)&&SimpleTransform(source);
        }
        internal SourceUiReplica(Func<VisualElement,Action<MeshGenerationContext>> paint):this(paint,null){}
        private static bool FlatBackground(VisualElement e)
        {
            if(e is Image||e is TextElement||e.generateVisualContent!=null||!SimpleTransform(e)||HasFilter(e))return false;
            var s=e.resolvedStyle;var b=s.backgroundImage;
            return s.backgroundColor.a>0&&b.texture==null&&b.renderTexture==null&&b.sprite==null&&b.vectorImage==null&&
                s.borderLeftWidth+s.borderRightWidth+s.borderTopWidth+s.borderBottomWidth==0&&
                s.borderTopLeftRadius+s.borderTopRightRadius+s.borderBottomLeftRadius+s.borderBottomRightRadius==0;
        }
        internal SourceUiReplica(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool boundToArea=false){_paint=paint;_snapshots=snapshots;_boundToArea=boundToArea;}
        internal void RepaintDynamic()
        {
            foreach(var pair in _copies){
                _snapshots?.Touch(pair.Key);var copy=pair.Value;
                if(copy.Tag.Paint!=null||copy.Element is Image image&&image.image is RenderTexture)copy.Element.MarkDirtyRepaint();
            }
        }
        internal void Update(VisualElement root,VisualElement stop,VisualElement parent,Rect area)
        {
            bool bounded=_boundToArea;
#if UNITY_EDITOR
            if(DisableBoundsForValidation)bounded=false;
#endif
            _childOrder.Clear();
            foreach(var copy in _copies.Values)copy.Used=false;
            void CopyNode(VisualElement source,VisualElement container,bool isRoot=false)
            {
                if(!_copies.TryGetValue(source,out var copy)){
                    var element=source is TextElement?(VisualElement)new Label():source is Image?new Image():new VisualElement();
                    copy=new Copy {Element=element,Style=new SourceUiReplicaStyle(element.style),TextPaint=element.generateVisualContent};_copies.Add(source,copy);
                    var retained=copy;copy.Painter=context=>{retained.Tag.Paint?.Invoke(context);if(retained.HasText)retained.TextPaint?.Invoke(context);};
                    element.pickingMode=PickingMode.Ignore;element.focusable=false;
                    element.name=source.name;foreach(var name in source.GetClasses())element.AddToClassList(name);
                }
                copy.Used=true;var target=copy.Element;var style=copy.Style;
                // Retained replicas are non-interactive; avoid native enable-state
                // propagation through their descendants when the state is stable.
                if(target.enabledSelf!=source.enabledSelf)target.SetEnabled(source.enabledSelf);
                var helper=source.userData==SourceUiBackdrop.ReplicaTag||source.userData is SourceUiBackdrop.ClipTag||source.userData is PaintTag;
                if(helper){if(_snapshots==null)CopyInlineGeometry(source,style);else _snapshots.Apply(source,style,true);}
                else {
                    if(_snapshots==null)CopyAppearance(source,style);else _snapshots.Apply(source,style,false);
                    var bounds=source.layout;var border=source.parent?.resolvedStyle;
                    SourceUiBackdrop.LocalBox(style,isRoot?0:bounds.x-(border?.borderLeftWidth??0),isRoot?0:bounds.y-(border?.borderTopWidth??0),bounds.width,bounds.height);
                    var transform=source.resolvedStyle;style.translate=new Translate(transform.translate.x,transform.translate.y,transform.translate.z);style.rotate=transform.rotate;style.scale=transform.scale;style.transformOrigin=new TransformOrigin(transform.transformOrigin.x,transform.transformOrigin.y,transform.transformOrigin.z);
                }
                if(isRoot){style.translate=new Translate(0,0);style.rotate=new Rotate(0);style.scale=new Scale(Vector3.one);style.opacity=1;style.filter=StyleKeyword.Null;}
                // Preserve child coordinates while restricting flat document
                // backgrounds to the blur support region, like bitmap geometry.
                bool flat=bounded&&!helper&&FlatBackground(source)&&_paint(source)==null;
                if(flat){
                    if(copy.FlatFill==null)copy.FlatFill=new VisualElement {name="native-bounded-prefix-fill",pickingMode=PickingMode.Ignore};
                    if(copy.FlatFill.parent!=target||target.hierarchy[0]!=copy.FlatFill)target.hierarchy.Insert(0,copy.FlatFill);
                    _childOrder[target]=1;
                    var full=source.worldBound;var m=source.worldTransform;float density=Mathf.Max(.0001f,source.scaledPixelsPerPoint);
                    float left=Mathf.Max(0,Mathf.Floor((area.xMin-full.xMin)/m.m00*density)/density),top=Mathf.Max(0,Mathf.Floor((area.yMin-full.yMin)/m.m11*density)/density);
                    float right=Mathf.Min(source.layout.width,Mathf.Ceil((area.xMax-full.xMin)/m.m00*density)/density),bottom=Mathf.Min(source.layout.height,Mathf.Ceil((area.yMax-full.yMin)/m.m11*density)/density);
                    SourceUiBackdrop.LocalBox(copy.FlatFill.style,left,top,Mathf.Max(0,right-left),Mathf.Max(0,bottom-top));
                    copy.FlatFill.style.backgroundColor=source.resolvedStyle.backgroundColor;style.backgroundColor=Color.clear;
                }else if(copy.FlatFill!=null)copy.FlatFill.RemoveFromHierarchy();
                if(target is TextElement text&&source is TextElement original){
                    // Preserve live text and rich-text state without asking UI Toolkit
                    // to regenerate unchanged replica text on every backdrop pass.
                    if(text.text!=original.text)text.text=original.text;
                    if(text.enableRichText!=original.enableRichText)text.enableRichText=original.enableRichText;
                    if(text.selection.isSelectable)text.selection.isSelectable=false;
                }
                if(target is Image image&&source is Image originalImage){
                    if(originalImage.image!=null){if(image.image!=originalImage.image)image.image=originalImage.image;}
                    else if(originalImage.sprite!=null){if(image.sprite!=originalImage.sprite)image.sprite=originalImage.sprite;}
                    else if(originalImage.vectorImage!=null){if(image.vectorImage!=originalImage.vectorImage)image.vectorImage=originalImage.vectorImage;}
                    else {
                        if(image.image!=null)image.image=null;
                        if(image.sprite!=null)image.sprite=null;
                        if(image.vectorImage!=null)image.vectorImage=null;
                    }
                    if(image.uv!=originalImage.uv)image.uv=originalImage.uv;
                    if(image.scaleMode!=originalImage.scaleMode)image.scaleMode=originalImage.scaleMode;
                    if(image.tintColor!=originalImage.tintColor)image.tintColor=originalImage.tintColor;
                    // Restrict bitmap geometry, not its resolution. Otherwise
                    // UIR's filter bounds can include the whole field image even
                    // though its ancestor clips paint to a tiny backdrop ROI.
                    if(bounded&&!helper&&!isRoot&&originalImage.image!=null&&originalImage.scaleMode==ScaleMode.StretchToFill&&SimpleTransform(source)&&!HasFilter(source)){
                        var s=source.resolvedStyle;
                        var bg=s.backgroundImage;
                        if(s.backgroundColor.a==0&&bg.texture==null&&bg.renderTexture==null&&bg.sprite==null&&bg.vectorImage==null&&s.paddingLeft+s.paddingRight+s.paddingTop+s.paddingBottom+s.borderLeftWidth+s.borderRightWidth+s.borderTopWidth+s.borderBottomWidth==0){
                            var full=source.worldBound;
                            var crop=Rect.MinMaxRect(Mathf.Max(full.xMin,area.xMin),Mathf.Max(full.yMin,area.yMin),Mathf.Min(full.xMax,area.xMax),Mathf.Min(full.yMax,area.yMax));
                            if(full.width>0&&full.height>0&&crop.width>0&&crop.height>0){
                                var m=source.worldTransform;float density=Mathf.Max(.0001f,source.scaledPixelsPerPoint);
                                float localLeft=Mathf.Max(0,Mathf.Floor((crop.xMin-full.xMin)/m.m00*density)/density),localTop=Mathf.Max(0,Mathf.Floor((crop.yMin-full.yMin)/m.m11*density)/density);
                                float localRight=Mathf.Min(source.layout.width,Mathf.Ceil((crop.xMax-full.xMin)/m.m00*density)/density),localBottom=Mathf.Min(source.layout.height,Mathf.Ceil((crop.yMax-full.yMin)/m.m11*density)/density);
                                var uv=originalImage.uv;float left=localLeft/source.layout.width,right=localRight/source.layout.width,top=localTop/source.layout.height,bottom=localBottom/source.layout.height;
                                image.uv=new Rect(uv.x+left*uv.width,uv.y+(1-bottom)*uv.height,(right-left)*uv.width,(bottom-top)*uv.height);
                                var b=source.layout;var border=source.parent?.resolvedStyle;
                                SourceUiBackdrop.LocalBox(style,b.x-(border?.borderLeftWidth??0)+localLeft,b.y-(border?.borderTopWidth??0)+localTop,localRight-localLeft,localBottom-localTop);
                            }
                        }
                    }
                }
                // Absolute replica children already have resolved pixel boxes.
                // Empty layout containers must not inflate the filter's bounds.
                if(bounded&&!helper&&Unpainted(source)&&_paint(source)==null){style.width=0;style.height=0;}
                copy.Tag.Paint=_paint(source);copy.HasText=source.generateVisualContent!=null&&copy.TextPaint!=null;
                var nextTag=source.userData is SourceUiBackdrop.ClipTag?source.userData:copy.Tag;
                if(!ReferenceEquals(target.userData,nextTag))target.userData=nextTag;
                var nextPainter=copy.Tag.Paint==null&&!copy.HasText?null:copy.Painter;
                if(target.generateVisualContent!=nextPainter)target.generateVisualContent=nextPainter;
                // Preserve the paint-prefix order without rotating every retained
                // sibling through the end of its hierarchy on every frame.
                _childOrder.TryGetValue(container,out var childIndex);
                // The expected slot is known from the paint-prefix traversal. A direct
                // hierarchy lookup avoids a linear IndexOf scan for each retained
                // replica while preserving the exact same insertion condition.
                if(target.parent!=container||childIndex>=container.hierarchy.childCount||container.hierarchy[childIndex]!=target)
                    container.hierarchy.Insert(childIndex,target);
                _childOrder[container]=childIndex+1;
                // Native text/styles already invalidate their own retained mesh.
                // Custom painters may read a changing shared texture or Canvas,
                // so those still repaint every pass even when styles are stable.
                if(copy.Tag.Paint!=null)target.MarkDirtyRepaint();
            }
            // Stop BEFORE the target (its background, foreground, descendants
            // and this replica itself). The nearest backdrop root excludes its
            // own backdrop image and outer filter/opacity; those belong to the
            // live ancestor group. Use actual retained hierarchy paint order.
            // Explicit inline display:none suppresses the entire subtree in Unity.
            // Read current inline values, not last frame's resolved layout.
            // Native filtered overlays retain their tree with opacity zero.
            // Copying their previous resolved appearance would resurrect closed
            // cards for one frame after their filter resources were disposed.
            foreach(var item in VisualBackdrop.PaintPrefix(root,stop,element=>element.hierarchy.childCount,(element,index)=>element.hierarchy[index],
                element=>element.style.display==DisplayStyle.None||(element.style.opacity.keyword==StyleKeyword.Undefined&&element.style.opacity.value<=0)||element.userData is SourceUiBackdrop.ClipTag clip&&(clip.Owner==root||!SourceUiBackdrop.WorldBounds(clip.Owner).Overlaps(area))||bounded&&Outside(element,area)))
                CopyNode(item.Node,item.Parent==null?parent:_copies[item.Parent].Element,item.Parent==null);
            _staleSources.Clear();
            foreach(var pair in _copies)if(!pair.Value.Used)_staleSources.Add(pair.Key);
            foreach(var source in _staleSources){_copies[source].Element.RemoveFromHierarchy();_copies.Remove(source);}
            _staleSources.Clear();
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

#endif
