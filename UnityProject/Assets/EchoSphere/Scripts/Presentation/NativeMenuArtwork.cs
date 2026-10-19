using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Final conceptStyle.css artwork, retained as native layout, GPU brushes and
    // depth-split Unity Mesh planes. Important transforms override the old CSS
    // animation tracks: adding orbit rotation here would change the actual menu.
    public sealed class NativeMenuArtwork : IDisposable
    {
        private readonly VisualElement _parent;
        private readonly Image _image;
        private readonly VisualElement[] _rings=new VisualElement[5],_dots=new VisualElement[5];
        private readonly VisualElement _body,_highlight;
        private readonly NativeMenuBrush[] _ringPaint=new NativeMenuBrush[5],_dotPaint=new NativeMenuBrush[5];
        private readonly NativeMenuBrush _bodyPaint=new NativeMenuBrush(),_highlightPaint=new NativeMenuBrush();
        private readonly SourceUiDepth _depth=new SourceUiDepth(),_bodyDepth=new SourceUiDepth(),_highlightDepth=new SourceUiDepth();
        private readonly SourceUiSurfaceFilters _blur=new SourceUiSurfaceFilters();
        private readonly List<SourceUiDepth.PlaneImage> _planes=new List<SourceUiDepth.PlaneImage>();
        private readonly List<SourceUiDepth.PlaneImage> _single=new List<SourceUiDepth.PlaneImage>(1);
        private bool _dirty=true,_disposed;private float _scale;
        private static readonly float[] Insets={9,22,31,15,39},Tilt={64,62,68,64,65},Rotation={-16,24,-34,-16,-24};
        public Texture Texture {get;private set;}
        public Rect Bounds {get;private set;}
        public int PaintCount {get;private set;}
        public NativeMenuArtwork(VisualElement parent)
        {
            _parent=parent??throw new ArgumentNullException(nameof(parent));
            _image=new Image {name="native-menu-core-image",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};_image.style.position=Position.Absolute;parent.Add(_image);
            for(int i=0;i<5;i++){
                var ring=Box("native-menu-orbit-"+i);_rings[i]=ring;ring.style.left=ring.style.right=ring.style.top=ring.style.bottom=Length.Percent(Insets[i]);
                ring.style.borderLeftWidth=ring.style.borderRightWidth=ring.style.borderTopWidth=ring.style.borderBottomWidth=1;
                ring.style.borderLeftColor=ring.style.borderRightColor=ring.style.borderTopColor=ring.style.borderBottomColor=Color.clear;
                parent.Add(ring);_ringPaint[i]=new NativeMenuBrush();
                var dot=Box("native-menu-orbit-dot-"+i);_dots[i]=dot;dot.style.width=dot.style.height=7;ring.Add(dot);_dotPaint[i]=new NativeMenuBrush();
                // Final legacy rules replace the dot paint, but retain the
                // original orbit-specific positions from the earlier rules.
                if(i==0){dot.style.left=Length.Percent(70);dot.style.top=Length.Percent(12);}
                else if(i==1){dot.style.left=Length.Percent(16);dot.style.bottom=Length.Percent(3);}
                else if(i==2){dot.style.left=Length.Percent(48);dot.style.top=Length.Percent(4);}
            }
            _body=Box("native-menu-core-body");_body.style.left=_body.style.right=_body.style.top=_body.style.bottom=Length.Percent(28);parent.Add(_body);
            _highlight=Box("native-menu-core-highlight");_highlight.style.left=Length.Percent(34);_highlight.style.top=Length.Percent(31);_highlight.style.width=Length.Percent(24);_highlight.style.height=Length.Percent(18);parent.Add(_highlight);
            parent.RegisterCallback<GeometryChangedEvent>(Changed);
        }
        private VisualElement Box(string name)
        {var box=new VisualElement {name=name,pickingMode=PickingMode.Ignore};box.style.position=Position.Absolute;box.RegisterCallback<GeometryChangedEvent>(Changed);return box;}
        private void Changed(GeometryChangedEvent evt)=>_dirty=true;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var color);return color;}
        public static NativeMenuBrush.Paint BodyPaint()=>new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=new Color(186f/255,240f/255,1,.24f),Origin=new Vector2(.33f,.27f),Colors=new[]{C("#ffffff"),C("#ffffff"),C("#c7f8ff"),C("#5bdcff"),C("#26558c"),C("#070d19"),C("#010309")},Stops=new[]{0f,.05f,.13f,.29f,.53f,.73f,1f},OuterA=new NativeMenuBrush.Shadow(22,Vector2.zero,new Color(65f/255,218f/255,1,.45f)),OuterB=new NativeMenuBrush.Shadow(54,Vector2.zero,new Color(88f/255,96f/255,1,.15f)),Inset=new NativeMenuBrush.Shadow(18,new Vector2(-12,-14),new Color(0,0,0,.48f))};
        public static NativeMenuBrush.Paint RingPaint(int index)
        {var colors=new[]{new Color(100f/255,216f/255,1,.22f),new Color(163f/255,124f/255,1,.20f),new Color(1,184f/255,90f/255,.18f),new Color(190f/255,242f/255,1,.24f),new Color(78f/255,218f/255,1,.22f)};return new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=colors[index]};}
        public static NativeMenuBrush.Paint DotPaint()=>new NativeMenuBrush.Paint {Ellipse=true,Fill=C("#bffaff"),OuterA=new NativeMenuBrush.Shadow(14,Vector2.zero,C("#55dfff"))};
        public static NativeMenuBrush.Paint HighlightPaint()=>new NativeMenuBrush.Paint {Ellipse=true,Fill=new Color(1,1,1,.55f)};
        public static NativeMenuBrush.Paint AvatarPaint(Color color)=>new NativeMenuBrush.Paint {Ellipse=true,Origin=new Vector2(.35f,.30f),Colors=new[]{Color.white,Color.white,color,C("#163a5c"),C("#02060d")},Stops=new[]{0f,.06f,.20f,.52f,.76f},OuterA=new NativeMenuBrush.Shadow(13,Vector2.zero,new Color(color.r,color.g,color.b,.3f))};
        private static Matrix4x4 Pose(VisualElement node,float x,float z)
        {var r=node.layout;var center=new Vector3(r.width*.5f,r.height*.5f,0);return Matrix4x4.Translate(new Vector3(r.x,r.y,0)+center)*Matrix4x4.Rotate(Quaternion.AngleAxis(x,Vector3.right))*Matrix4x4.Rotate(Quaternion.AngleAxis(z,Vector3.forward))*Matrix4x4.Translate(-center);}
        private static SourceUiDepth.PlaneImage Plane(Texture texture,Rect rect,Matrix4x4 matrix)=>new SourceUiDepth.PlaneImage {Texture=texture,Rect=rect,Matrix=matrix,Opacity=1,Blend="normal"};
        public bool Prepare(float scale)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMenuArtwork));
            if(!_dirty&&_scale==scale)return true;
            var bounds=_parent.layout;if(bounds.width<=0||bounds.height<=0||_body.layout.width<=0)return false;
            _planes.Clear();
            for(int i=0;i<5;i++){
                var ring=_rings[i];var dot=_dots[i];if(ring.layout.width<=0||dot.layout.width<=0)return false;
                _ringPaint[i].Render(ring.layout.width,ring.layout.height,scale,RingPaint(i));_dotPaint[i].Render(dot.layout.width,dot.layout.height,scale,DotPaint());
                var pose=Pose(ring,Tilt[i],Rotation[i]);_planes.Add(Plane(_ringPaint[i].Texture,_ringPaint[i].Bounds,pose));
                _planes.Add(Plane(_dotPaint[i].Texture,_dotPaint[i].Bounds,pose*Matrix4x4.Translate(new Vector3(dot.layout.x,dot.layout.y,0))));
            }
            _bodyPaint.Render(_body.layout.width,_body.layout.height,scale,BodyPaint());_single.Clear();_single.Add(Plane(_bodyPaint.Texture,_bodyPaint.Bounds,Matrix4x4.identity));
            var body=_bodyDepth.Render(_single,_body.layout.width,_body.layout.height,scale,0,null,false);_planes.Add(Plane(body,_bodyDepth.Bounds,Pose(_body,0,0)));
            _highlightPaint.Render(_highlight.layout.width,_highlight.layout.height,scale,HighlightPaint());_single.Clear();_single.Add(Plane(_highlightPaint.Texture,_highlightPaint.Bounds,Matrix4x4.identity));
            var highlight=_highlightDepth.Render(_single,_highlight.layout.width,_highlight.layout.height,scale,0,null,false);
            var filtered=_blur.Blur(highlight,_highlightDepth.Bounds,4,scale);_planes.Add(Plane(filtered,_blur.Bounds,Pose(_highlight,0,-28)));
            Texture=_depth.Render(_planes,bounds.width,bounds.height,scale,0,null,true);Bounds=_depth.Bounds;
            _image.image=Texture;_image.style.left=Bounds.x;_image.style.top=Bounds.y;_image.style.width=Bounds.width;_image.style.height=Bounds.height;_image.MarkDirtyRepaint();
            _dirty=false;_scale=scale;PaintCount++;return true;
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;_parent.UnregisterCallback<GeometryChangedEvent>(Changed);_image.RemoveFromHierarchy();
            foreach(var node in _rings){node.UnregisterCallback<GeometryChangedEvent>(Changed);node.RemoveFromHierarchy();}foreach(var node in _dots)node.UnregisterCallback<GeometryChangedEvent>(Changed);
            _body.UnregisterCallback<GeometryChangedEvent>(Changed);_highlight.UnregisterCallback<GeometryChangedEvent>(Changed);_body.RemoveFromHierarchy();_highlight.RemoveFromHierarchy();
            foreach(var paint in _ringPaint)paint.Dispose();foreach(var paint in _dotPaint)paint.Dispose();_bodyPaint.Dispose();_highlightPaint.Dispose();_depth.Dispose();_bodyDepth.Dispose();_highlightDepth.Dispose();_blur.Dispose();
        }
    }
}
