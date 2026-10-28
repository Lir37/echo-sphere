#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // CSS clips content after painting its own border and outer shadow. UIR
    // clips generateVisualContent too; keep box painting outside that clip.
    internal sealed class SourceUiFrame : VisualElement
    {
        internal readonly VisualElement Content;
        private readonly VisualElement _paintBounds;
        private Vector2 _flowOffset;
        private StyleTranslate _authoredTranslate;
        internal Matrix4x4 BrushMatrix {get;private set;}=Matrix4x4.identity;
        internal SourceUiFrame(VisualElement content)
        {
            Content=content;pickingMode=PickingMode.Ignore;
            _paintBounds=new VisualElement {pickingMode=PickingMode.Ignore,userData=SourceUiBackdrop.ReplicaTag};
            hierarchy.Add(_paintBounds);hierarchy.Add(content);
        }
        internal void ApplyOuterStyle(float contentRotation)
        {
            var value=Content.style;var outer=style;
            outer.position=value.position;outer.left=value.left;outer.top=value.top;outer.right=value.right;outer.bottom=value.bottom;
            outer.width=value.width;outer.height=value.height;outer.minWidth=value.minWidth;outer.minHeight=value.minHeight;outer.maxWidth=value.maxWidth;outer.maxHeight=value.maxHeight;outer.aspectRatio=value.aspectRatio;
            outer.marginLeft=value.marginLeft;outer.marginRight=value.marginRight;outer.marginTop=value.marginTop;outer.marginBottom=value.marginBottom;
            outer.flexGrow=value.flexGrow;outer.flexShrink=value.flexShrink;outer.flexBasis=value.flexBasis;outer.alignSelf=value.alignSelf;
            outer.display=value.display;outer.visibility=value.visibility;outer.opacity=value.opacity;
            _authoredTranslate=value.translate;ApplyFlowTranslate();outer.rotate=value.rotate;outer.scale=value.scale;outer.transformOrigin=value.transformOrigin;
            outer.borderTopLeftRadius=value.borderTopLeftRadius;outer.borderTopRightRadius=value.borderTopRightRadius;outer.borderBottomLeftRadius=value.borderBottomLeftRadius;outer.borderBottomRightRadius=value.borderBottomRightRadius;
            outer.overflow=Overflow.Visible;outer.flexDirection=FlexDirection.Column;outer.alignItems=Align.Stretch;
            value.position=Position.Relative;value.left=value.right=value.top=value.bottom=StyleKeyword.Auto;
            value.width=value.height=StyleKeyword.Auto;value.minWidth=value.minHeight=0;value.maxWidth=value.maxHeight=StyleKeyword.None;value.aspectRatio=StyleKeyword.Auto;
            value.marginLeft=value.marginRight=value.marginTop=value.marginBottom=0;value.flexGrow=1;value.flexShrink=1;value.flexBasis=StyleKeyword.Auto;value.alignSelf=Align.Stretch;
            value.translate=new Translate(0,0);value.rotate=new Rotate(contentRotation);value.scale=new Scale(Vector3.one);value.opacity=1;value.filter=StyleKeyword.Null;
            var origin=outer.transformOrigin.value;var pivot=new Vector3(origin.x.value,origin.y.value,origin.z);
            BrushMatrix=Matrix4x4.Translate(pivot)*Matrix4x4.Rotate(Quaternion.Euler(0,0,contentRotation))*Matrix4x4.Translate(-pivot);
        }
        // Keep native flex sizing and z-ordered painting/picking. This translation
        // changes no layout dimensions and composes with the authored transform.
        internal void SetFlowOffset(Vector2 offset)
        {
            if(_flowOffset==offset)return;
            _flowOffset=offset;ApplyFlowTranslate();
        }
        private void ApplyFlowTranslate()
        {
            if(_flowOffset==Vector2.zero){style.translate=_authoredTranslate;return;}
            var pose=_authoredTranslate.value;
            float Pixels(Length value,float size)=>value.unit==LengthUnit.Percent?value.value*size*.01f:value.value;
            style.translate=new Translate(Pixels(pose.x,layout.width)+_flowOffset.x,Pixels(pose.y,layout.height)+_flowOffset.y,pose.z);
        }
        internal void SetPaintBounds(Rect rect)
        {
            // UIR computes filter bounds from retained boxes, not emitted mesh
            // vertices. A non-painted child accounts for shadows/filtered planes.
            SourceUiBackdrop.LocalBox(_paintBounds,rect.x,rect.y,rect.width,rect.height);
            var pose=Content.style;var pivot=pose.transformOrigin.value;
            _paintBounds.style.rotate=pose.rotate;_paintBounds.style.transformOrigin=new TransformOrigin(pivot.x.value-rect.x,pivot.y.value-rect.y,pivot.z);
        }
    }
}

#endif
