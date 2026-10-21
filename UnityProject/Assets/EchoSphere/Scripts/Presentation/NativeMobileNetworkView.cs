using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native geometry for the two authored mobile network overlays. Coordinates
    // are display data, already projected by the existing gesture/controller code.
    internal sealed class NativeMobileNetworkView : IDisposable
    {
        internal readonly struct Link {internal readonly Vector2 A,B;internal readonly float Width;internal Link(Vector2 a,Vector2 b,float width){A=a;B=b;Width=width;}}
        internal sealed class State
        {
            internal bool Memory,Closed,Valid;internal float Alpha,Radius,RingWidth,FormationWidth,FontSize;
            internal Vector2 Center,CaptionPosition;internal Color SphereColor;internal string Caption;
            internal readonly List<Link> Links=new List<Link>();internal Vector2[] Points=Array.Empty<Vector2>();
        }
        internal VisualElement Root {get;}
        internal bool Ready {get;private set;}
        private readonly VisualElement _geometry;private readonly Label _caption;private State _state;private readonly int _created;
        private static Color C(string html){ColorUtility.TryParseHtmlString(html,out var c);return c;}
        internal NativeMobileNetworkView(VisualElement parent)
        {
            Root=new VisualElement {name="native-mobile-network",pickingMode=PickingMode.Ignore};Fill(Root);Root.style.opacity=0;parent.Add(Root);
            _geometry=new VisualElement {name="network-geometry",pickingMode=PickingMode.Ignore};Fill(_geometry);Root.Add(_geometry);_geometry.generateVisualContent+=Draw;
            _caption=NativePauseStatsView.Text(Root,"network-caption",11,"#dcecff","700",1.2,.192f);_caption.style.position=Position.Absolute;_caption.style.unityTextAlign=TextAnchor.UpperLeft;_created=Time.frameCount;
        }
        internal void Bind(State state)
        {
            if(ReferenceEquals(state,_state))return;_state=state;_geometry.MarkDirtyRepaint();_caption.style.display=state.Memory?DisplayStyle.None:DisplayStyle.Flex;
            if(!state.Memory){_caption.text=state.Caption;_caption.style.fontSize=state.FontSize;_caption.style.letterSpacing=.192f*100/state.FontSize;_caption.style.color=C(state.Valid?"#dcecff":"#ff7a86");}
        }
        internal void Prepare()
        {
            if(_state==null||Root.layout.width<=0||Root.layout.height<=0)return;
            if(!_state.Memory){var font=_caption.resolvedStyle.unityFontDefinition.fontAsset;if(font==null)return;var face=font.faceInfo;var measured=_caption.MeasureTextSize(_caption.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);_caption.style.left=_state.CaptionPosition.x-measured.x*.5f;_caption.style.top=_state.CaptionPosition.y-_state.FontSize*face.ascentLine/face.pointSize;_caption.style.width=measured.x+1;_caption.style.height=Mathf.Max(measured.y,_state.FontSize*(face.ascentLine-face.descentLine)/face.pointSize);}
            if(Time.frameCount-_created>=2){Ready=true;Root.style.opacity=1;}
        }
        private void Draw(MeshGenerationContext context)
        {
            var s=_state;if(s==null)return;var p=context.painter2D;p.lineCap=LineCap.Butt;p.lineJoin=LineJoin.Miter;p.dashOffset=0;
            Color Alpha(Color c,float alpha){c.a*=alpha;return c;}
            void Line(Vector2 a,Vector2 b,float width,Color color,float[] dash){p.strokeColor=color;p.lineWidth=width;p.SetDashPattern(dash);p.BeginPath();p.MoveTo(a);p.LineTo(b);p.Stroke();}
            void Polyline(Vector2[] points,float width,Color color,float[] dash){if(points.Length<2)return;p.strokeColor=color;p.lineWidth=width;p.SetDashPattern(dash);p.BeginPath();p.MoveTo(points[0]);for(int i=1;i<points.Length;i++)p.LineTo(points[i]);p.Stroke();}
            void Circle(Vector2 center,float radius,float width,Color stroke,Color fill,float[] dash){p.SetDashPattern(dash);p.lineWidth=width;p.strokeColor=stroke;p.fillColor=fill;p.BeginPath();p.Arc(center,radius,0,360);p.ClosePath();if(fill.a>0)p.Fill();p.Stroke();}
            if(s.Memory){
                var color=Alpha(C("#b6c9de"),s.Alpha);Polyline(s.Points,2,color,new[]{4f,6f});
                if(s.Closed&&s.Points.Length>2)Line(s.Points[s.Points.Length-1],s.Points[0],1.5f,Alpha(color,.75f),new[]{4f,6f});
                foreach(var point in s.Points)Circle(point,9,1.5f,color,Color.clear,Array.Empty<float>());
            }else{
                var color=s.Valid?s.SphereColor:C("#ff4d5d");foreach(var link in s.Links)Line(link.A,link.B,link.Width,Alpha(color,.9f),new[]{7f,5f});
                Polyline(s.Points,s.FormationWidth,Alpha(C(s.Valid?"#ffb84d":"#ff4d5d"),.82f),new[]{6f,5f});
                var fill=color;fill.a=s.Valid?24/255f:.12f;Circle(s.Center,s.Radius,s.RingWidth,color,fill,new[]{5f,4f});
            }
        }
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        public void Dispose(){Root.RemoveFromHierarchy();}
    }
}
