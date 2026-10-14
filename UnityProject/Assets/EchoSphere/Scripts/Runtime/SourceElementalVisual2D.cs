using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    // Exact paths and animation constants from src/spheres/elementalVisual.ts.
    public sealed class SourceElementalVisual2D : MonoBehaviour
    {
        private const float Tau = Mathf.PI*2f;
        private EchoSphereRuntime _runtime;
        private SphereAttackAgent _agent;
        private readonly SourceCanvasMesh _canvas = new SourceCanvasMesh();
        private Mesh _mesh;
        private Material _material;
        private MeshRenderer _renderer;
        private Transform _layer;
        public void Initialize(EchoSphereRuntime runtime, SphereAttackAgent agent)
        {
            _runtime=runtime; _agent=agent;
            var layer=new GameObject("Source Elemental Layer"); layer.transform.SetParent(transform,false); _layer=layer.transform;
            _mesh=new Mesh { name="Source Elemental Trace" }; _mesh.MarkDynamic(); layer.AddComponent<MeshFilter>().sharedMesh=_mesh;
            _material=new Material(Shader.Find("Sprites/Default")); _renderer=layer.AddComponent<MeshRenderer>(); _renderer.sharedMaterial=_material; _renderer.sortingOrder=11;
        }
        private static Color Alpha(Color color,float alpha) { color.a=alpha; return color; }
        private static Vector2 Direction(float a) => new Vector2(Mathf.Cos(a),Mathf.Sin(a));
        private static Vector2 Rotate(Vector2 p,float a) => new Vector2(p.x*Mathf.Cos(a)-p.y*Mathf.Sin(a),p.x*Mathf.Sin(a)+p.y*Mathf.Cos(a));
        private void LateUpdate()
        {
            if(_runtime==null||_agent==null) return;
            var element=SphereElementalRules.ResolveElement(_runtime.GetSphereBranch(_agent.Type),_runtime.GetSphereFinal(_agent.Type));
            _renderer.enabled=element!=SphereElement.None; if(element==SphereElement.None) return;
            var parentScale=transform.lossyScale; _layer.localScale=new Vector3(1f/parentScale.x,1f/parentScale.y,1f/parentScale.z);
            _canvas.Clear(); var time=_runtime.CurrentRunTime;
            var body=Mathf.Clamp(130f*.19f+_agent.ProgressionLevel*.8f,15f,25f); var scale=body/24f;
            var radius=body*(_agent.Type==SphereId.Orbital?1.46f:1.34f);
            ColorUtility.TryParseHtmlString(element==SphereElement.Fire?"#ff743d":element==SphereElement.Freeze?"#d9f6ff":"#72f08e",out var color);
            var active=!_agent.NetworkDisabled; var alpha=active?.78f:.20f; var width=Mathf.Max(1f,radius*.04f);
            var rx=radius*1.16f; var ry=radius*.34f; var spin=-time*.18f; var particleSpin=-time*.34f;
            Ellipse(rx,ry,spin,0f,Tau,Alpha(color,alpha*.18f),Mathf.Max(2f,width*3f));
            Ellipse(rx,ry,spin,Mathf.PI,Tau,Alpha(color,alpha*.58f),width);
            Ellipse(rx,ry,spin,0f,Mathf.PI,Alpha(color,alpha),width);
            Ellipse(rx*.995f,ry*.995f,spin,particleSpin%Tau,particleSpin%Tau+.72f,Alpha(color,alpha*.72f),Mathf.Max(.75f,width*.55f));
            for(var i=0;i<6;i++)
            {
                var a=particleSpin+i*Tau/6f; var depth=.38f+.62f*((Mathf.Sin(a)+1f)*.5f);
                Glyph(element,new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry+Mathf.Sin(time*2.8f+i*1.73f)*radius*.018f),Mathf.Max(2.2f,radius*(.055f+i%3*.008f)),a+Mathf.PI*.5f,i%3,Alpha(color,alpha*(.72f+depth*.28f)),width);
                if(i%2==0) { var mote=a+(i%4==0?.16f:-.16f); var mr=Mathf.Max(.65f,radius*.018f); _canvas.Ellipse(new Vector2(Mathf.Cos(mote)*rx*1.035f,Mathf.Sin(mote)*ry*1.18f),mr,mr,Alpha(color,alpha*(.28f+depth*.24f))); }
            }
            Special(element,radius,time,active,color);
            if(_agent.Type!=SphereId.Orbital)
            {
                var direction=-_agent.VisualRotation; var pulse=.75f+.25f*Mathf.Sin(time*8f+transform.position.x/SpherePlacementRules.SourcePixelsToWorld*.01f);
                var at=Direction(direction)*body*1.05f; var accent=Alpha(color,(active?.72f:.18f)*pulse);
                Glyph(element,at,body*(element==SphereElement.Fire?.12f:element==SphereElement.Freeze?.11f:.10f),direction,0,accent,Mathf.Max(.85f,1.1f*scale));
                _canvas.Ellipse(at+Direction(direction)*body*.10f,body*.10f,body*.10f,Alpha(color,accent.a*.16f*.56f));
            }
            _canvas.Upload(_mesh);
        }
        private void Ellipse(float rx,float ry,float spin,float from,float to,Color color,float width)
        {
            var previous=Rotate(new Vector2(Mathf.Cos(from)*rx,Mathf.Sin(from)*ry),spin); var count=Mathf.Max(4,Mathf.CeilToInt(Mathf.Abs(to-from)*16f));
            for(var i=1;i<=count;i++) { var a=Mathf.Lerp(from,to,i/(float)count); var next=Rotate(new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),spin); _canvas.Line(previous,next,color,width); previous=next; }
        }
        private void Quadratic(Vector2 start,Vector2 control,Vector2 end,Color color,float width)
        {
            var previous=start; for(var i=1;i<=12;i++) { var t=i/12f; var next=(1f-t)*(1f-t)*start+2f*(1f-t)*t*control+t*t*end; _canvas.Line(previous,next,color,width); previous=next; }
        }
        private void Glyph(SphereElement element,Vector2 center,float size,float angle,int variant,Color color,float width)
        {
            var pose=_canvas.Pose; _canvas.Pose=p=>center+Rotate(p,angle);
            if(element==SphereElement.Fire)
            {
                var lean=(variant-1)*size*.12f; var first=new Vector2(lean,-size); var right=new Vector2(size*(.44f-variant*.05f),size*.55f); var left=new Vector2(-size*(.46f+variant*.03f),size*.58f);
                Quadratic(first,new Vector2(size*(.92f+variant*.06f),-size*.20f),right,color,width);
                Quadratic(right,new Vector2(lean,size*.92f),left,color,width);
                Quadratic(left,new Vector2(-size*(.90f-variant*.04f),-size*.18f),first,color,width);
                var inner=new Vector2(lean,-size*.34f); var bottom=new Vector2(lean+size*.04f,size*.46f);
                Quadratic(inner,new Vector2(size*.40f,-size*.02f),bottom,color,width); Quadratic(bottom,new Vector2(-size*.30f,size*.18f),inner,color,width);
            }
            else if(element==SphereElement.Freeze)
            {
                var arms=variant==1?4:6;
                for(var i=0;i<arms;i++) { var a=i*Tau/arms; _canvas.Line(Direction(a)*size*.14f,Direction(a)*size,color,width);
                    if(variant!=2) { var tip=variant==1?.68f:.62f; var branch=variant==1?.42f:.34f; _canvas.Line(Direction(a)*size*tip,Direction(a+branch)*size*.88f,color,width); _canvas.Line(Direction(a)*size*tip,Direction(a-branch)*size*.88f,color,width); } }
                _canvas.Arc(Vector2.zero,size*(variant==2?.28f:.20f),0f,Tau,color,width);
            }
            else
            {
                var drift=(variant-1)*size*.08f;
                var a=new Vector2(-size*.34f+drift,size*.10f); var b=new Vector2(0f,-size*(.12f+variant*.04f)); var c=new Vector2(size*.34f+drift,size*.08f);
                var br=size*(.42f-variant*.03f); var cr=size*(.29f+variant*.02f);
                _canvas.Arc(a,size*.34f,0f,Tau,color,width); _canvas.Line(a+Vector2.right*size*.34f,b+Vector2.right*br,color,width);
                _canvas.Arc(b,br,0f,Tau,color,width); _canvas.Line(b+Vector2.right*br,c+Vector2.right*cr,color,width); _canvas.Arc(c,cr,0f,Tau,color,width);
                var d=new Vector2(size*(.10f-variant*.04f),-size*(.50f+variant*.04f)); var dr=size*(.11f+variant*.02f); var e=new Vector2(-size*.22f,-size*.42f);
                _canvas.Arc(d,dr,0f,Tau,color,width); _canvas.Line(d+Vector2.right*dr,e+Vector2.right*size*.07f,color,width); _canvas.Arc(e,size*.07f,0f,Tau,color,width);
            }
            _canvas.Pose=pose;
        }
        private void Special(SphereElement element,float radius,float time,bool active,Color color)
        {
            var alpha=active?.42f:.10f; var width=Mathf.Max(.8f,radius*.024f);
            if(_agent.Type==SphereId.Chain) for(var i=0;i<3;i++) { var a=time*.9f+i*Tau/3f; var p=new Vector2(Mathf.Cos(a)*radius*1.02f,Mathf.Sin(a)*radius*.30f); _canvas.Line(p,p+new Vector2(Mathf.Cos(a+1.2f)*radius*.18f,Mathf.Sin(a+1.2f)*radius*.12f),color,width); }
            else if(_agent.Type==SphereId.Aura)
            {
                Ellipse(radius*1.85f,radius*.58f,0f,0f,Tau,Alpha(color,alpha*.72f),width);
                for(var i=0;i<3;i++) { var a=-time*.45f+i*Tau/3f; Glyph(element,new Vector2(Mathf.Cos(a)*radius*1.55f,Mathf.Sin(a)*radius*.48f),radius*.065f,a+Mathf.PI*.5f,i,Alpha(color,alpha*.65f),width); }
            }
            else if(_agent.Type==SphereId.Gravity) for(var i=0;i<3;i++) { var q=Mathf.Repeat(time*.35f+i/3f,1f); _canvas.Arc(Vector2.zero,radius*(1.72f-q*.72f),-.55f+i*2f,.55f+i*2f,Alpha(color,alpha*(1f-q)*.8f),width); }
            else if(_agent.Type==SphereId.Pulse) { var q=Mathf.Repeat(time*.72f,1f); _canvas.Arc(Vector2.zero,radius*(.72f+q*1.15f),0f,Tau,Alpha(color,alpha*(1f-q)),width); }
        }
        private void OnDestroy() { if(_mesh!=null) Destroy(_mesh); if(_material!=null) Destroy(_material); }
    }
}
