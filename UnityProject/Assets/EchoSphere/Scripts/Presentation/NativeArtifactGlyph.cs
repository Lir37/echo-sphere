using System;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    // Candidate for the existing ArtifactGlyph artwork. The caller supplies the
    // inherited font size: the source glyph is 1.45em, not proportional to size.
    public sealed class NativeArtifactGlyph : IDisposable
    {
        private readonly VisualElement _root,_clip,_geometry;
        private readonly Label _core;
        private readonly NativeHudSurface _base,_highlight;
        private Color _color;
        private string _glyph,_colorText;
        private bool _disposed;
        private readonly float[] _dashes=new float[2];
        public VisualElement Root=>_root;
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        public NativeArtifactGlyph(VisualElement parent,float size,float inheritedFontSize)
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));if(size<=2||inheritedFontSize<=0)throw new ArgumentOutOfRangeException(nameof(size));
            _root=Box(parent,"native-artifact-glyph");_root.style.position=Position.Relative;_root.style.width=_root.style.height=size;
            _base=new NativeHudSurface(_root,new NativeMenuBrush.Paint());
            _clip=Box(_root,"artifact-glyph-clip");_clip.style.position=Position.Absolute;_clip.style.left=_clip.style.right=_clip.style.top=_clip.style.bottom=1;
            _clip.style.borderTopLeftRadius=_clip.style.borderTopRightRadius=_clip.style.borderBottomLeftRadius=_clip.style.borderBottomRightRadius=999;
            _clip.style.overflow=Overflow.Hidden;_clip.style.alignItems=Align.Center;_clip.style.justifyContent=Justify.Center;
            _highlight=new NativeHudSurface(_clip,new NativeMenuBrush.Paint {Ellipse=true,Origin=new Vector2(.5f,.45f),Colors=new[]{A(255,255,255,.1f),Color.clear},Stops=new[]{0f,.32f}});
            _geometry=Box(_clip,"artifact-glyph-lines");_geometry.style.position=Position.Absolute;_geometry.style.left=_geometry.style.right=_geometry.style.top=_geometry.style.bottom=0;_geometry.generateVisualContent+=Draw;
            _core=Text(_clip,"artifact-glyph-core",inheritedFontSize*1.45f,"#7f9bb8",line:1);
        }
        public void Bind(string glyph,string color)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeArtifactGlyph));if(_glyph==glyph&&_colorText==color)return;
            if(!ColorUtility.TryParseHtmlString(color,out var parsed))throw new ArgumentException("Original glyph color required",nameof(color));
            _glyph=glyph;_colorText=color;_color=parsed;Set(_core,glyph);_core.style.color=parsed;_core.style.textShadow=new TextShadow {color=parsed,blurRadius=12,offset=Vector2.zero};
            var border=parsed;border.a=136/255f;var shadow=parsed;shadow.a=24/255f;
            _base.SetPaint(new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=border,LinearAngle=145,
                Colors=new[]{A(15,30,48,.95f),A(2,8,15,.98f)},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(24,Vector2.zero,shadow)});
            _geometry.MarkDirtyRepaint();
        }
        private void Draw(MeshGenerationContext context)
        {
            var w=_geometry.contentRect.width;var h=_geometry.contentRect.height;if(w<=1||h<=1)return;
            var p=context.painter2D;var center=new Vector2(w*.5f,h*.5f);
            Vector2 Rotate(Vector2 point,float degrees,Vector2 origin){var angle=degrees*Mathf.Deg2Rad;var v=point-origin;return origin+new Vector2(v.x*Mathf.Cos(angle)-v.y*Mathf.Sin(angle),v.x*Mathf.Sin(angle)+v.y*Mathf.Cos(angle));}
            void Rectangle(float inset,float degrees,float alpha){float left=w*inset+.5f,top=h*inset+.5f,right=w*(1-inset)-.5f,bottom=h*(1-inset)-.5f;p.lineWidth=1;p.strokeColor=new Color(1,1,1,alpha);p.BeginPath();p.MoveTo(Rotate(new Vector2(left,top),degrees,center));p.LineTo(Rotate(new Vector2(right,top),degrees,center));p.LineTo(Rotate(new Vector2(right,bottom),degrees,center));p.LineTo(Rotate(new Vector2(left,bottom),degrees,center));p.ClosePath();p.Stroke();}
            Rectangle(.11f,45,.08f);Rectangle(.24f,15,.10f);
            float radius=Mathf.Min(w,h)*.33f-.5f;var perimeter=2*Mathf.PI*radius;int count=Mathf.Max(1,Mathf.RoundToInt(perimeter/5));_dashes[0]=3;_dashes[1]=Mathf.Max(.01f,perimeter/count-3);var orbit=_color;orbit.a=85/255f;p.strokeColor=orbit;p.lineWidth=1;p.SetDashPattern(_dashes);p.dashOffset=0;p.BeginPath();p.Arc(center,radius,-112,-112+360);p.ClosePath();p.Stroke();p.SetDashPattern(Array.Empty<float>());
            float cutWidth=w*.28f,cutHeight=2,leftCut=w*(1-.12f)-cutWidth,topCut=h*(1-.22f)-cutHeight;var cutCenter=new Vector2(leftCut+cutWidth*.5f,topCut+cutHeight*.5f);var cut=_color;cut.a*=.45f;p.fillColor=cut;p.BeginPath();p.MoveTo(Rotate(new Vector2(leftCut,topCut),-35,cutCenter));p.LineTo(Rotate(new Vector2(leftCut+cutWidth,topCut),-35,cutCenter));p.LineTo(Rotate(new Vector2(leftCut+cutWidth,topCut+cutHeight),-35,cutCenter));p.LineTo(Rotate(new Vector2(leftCut,topCut+cutHeight),-35,cutCenter));p.ClosePath();p.Fill();
        }
        public void Prepare(float scale){if(_disposed)throw new ObjectDisposedException(nameof(NativeArtifactGlyph));if(_glyph==null)throw new InvalidOperationException("Bind an original glyph first");_base.Prepare(scale);_highlight.Prepare(scale);}
        public void Dispose(){if(_disposed)return;_disposed=true;_geometry.generateVisualContent-=Draw;_base.Dispose();_highlight.Dispose();_root.RemoveFromHierarchy();}
    }
}
