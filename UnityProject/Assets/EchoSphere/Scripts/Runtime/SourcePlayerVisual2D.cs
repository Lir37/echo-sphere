using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    // renderer.ts: drawPlayer, getSpheristAnimationState, drawCharacterVfx and drawPlayerShield.
    public sealed class SourcePlayerVisual2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private PlayerCoreController2D _core;
        private readonly SourceCanvasMesh _body = new SourceCanvasMesh(), _shield = new SourceCanvasMesh();
        private Mesh _bodyMesh, _shieldMesh, _spriteMesh;
        private Material _material, _spriteMaterial;
        private MeshRenderer _spriteRenderer;
        private TextMesh _charges;
        private Vector2 _lastPosition;
        private float _lastTime = -1f;
        public void Initialize(EchoSphereRuntime runtime, PlayerCoreController2D core)
        {
            _runtime=runtime; _core=core; _material=new Material(Shader.Find("Sprites/Default")); _spriteMaterial=new Material(Shader.Find("Sprites/Default"));
            CreateLayer("Source Character Body",10,_material,out _bodyMesh); CreateLayer("Source Shield",8,_material,out _shieldMesh);
            _spriteRenderer=CreateLayer("Authored Spherist",10,_spriteMaterial,out _spriteMesh);
            var label=new GameObject("Shield Charges"); label.transform.SetParent(transform,false); _charges=label.AddComponent<TextMesh>();
            _charges.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); _charges.fontSize=32; _charges.characterSize=9f*SpherePlacementRules.SourcePixelsToWorld/32f;
            _charges.anchor=TextAnchor.MiddleCenter; _charges.alignment=TextAlignment.Center; _charges.color=new Color32(223,250,255,255);
            label.GetComponent<MeshRenderer>().sharedMaterial=_charges.font.material; label.GetComponent<MeshRenderer>().sortingOrder=9;
            foreach(var name in new[]{"Shell","Glow","Inner Frame","Core Light"}) { var child=transform.Find(name); if(child!=null) child.gameObject.SetActive(false); }
            LateUpdate();
        }
        private MeshRenderer CreateLayer(string name,int order,Material material,out Mesh mesh)
        {
            var child=new GameObject(name); child.transform.SetParent(transform,false); mesh=new Mesh { name=name }; mesh.MarkDynamic(); child.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=child.AddComponent<MeshRenderer>(); renderer.sharedMaterial=material; renderer.sortingOrder=order; return renderer;
        }
        private static Color Alpha(Color color,float alpha) => new Color(color.r,color.g,color.b,alpha);
        private static Vector2 Rotate(Vector2 point,float angle) => new Vector2(point.x*Mathf.Cos(angle)-point.y*Mathf.Sin(angle),point.x*Mathf.Sin(angle)+point.y*Mathf.Cos(angle));
        private void LateUpdate()
        {
            if(_runtime==null||_core==null) return;
            var t=_runtime.CurrentRunTime; var id=_runtime.SourcePlayerCharacter; ColorUtility.TryParseHtmlString(CharacterRules.Get(id).ColorHex,out var tint);
            var position=_runtime.PlayerPosition; var dt=_lastTime>=0f?Mathf.Max(.001f,t-_lastTime):.016f; var velocity=(position-_lastPosition)/SpherePlacementRules.SourcePixelsToWorld/dt;
            _lastPosition=position; _lastTime=t; var pulse=1f+Mathf.Sin(t*5.5f)*.025f; var baseY=-1.6f+Mathf.Sin(t*2.4f)*.8f;
            _body.Clear(); _body.Pose=point=>point*pulse+new Vector2(0f,baseY);
            _body.Ellipse(new Vector2(0f,5f),15.2f,4f,new Color(0f,0f,0f,.34f*.42f)); _body.Ellipse(new Vector2(0f,5f),15.2f*.72f,4f*.68f,new Color(0f,0f,0f,.34f*.16f));
            if(id==SphereCharacterId.Spherist)
            {
                var moving=!_core.IsDead&&_core.ContactGraceTimer<=0f&&_runtime.SourcePlayerShieldTimer<=0f&&_core.ShieldCharges<=0&&(_core.DashTimer>0f||velocity.magnitude>.06f);
                var sprite=RuntimeSpriteFactory.LoadAuthored(moving?"character-spherist-3q":"character-spherist-front");
                var progress=_core.IsDead?_runtime.SourcePlayerDeathProgress:0f; var fold=progress*progress;
                Vector2 Pose(Vector2 point)
                {
                    if(_core.IsDead)
                    {
                        point*=pulse*(1f-fold*.18f); point=new Vector2(point.x*(1f-fold*.45f)-point.y*.10f*fold,point.y*(1f+fold*.06f)); point.y-=progress*1.6f;
                    }
                    else point*=pulse;
                    return point*pulse+new Vector2(0f,baseY);
                }
                _spriteRenderer.enabled=sprite!=null;
                if(sprite!=null)
                {
                    var source=sprite.vertices; var points=new Vector3[source.Length]; var colors=new Color[source.Length];
                    var scale=46.4f/Mathf.Max(sprite.rect.width/sprite.pixelsPerUnit,sprite.rect.height/sprite.pixelsPerUnit);
                    for(var i=0;i<points.Length;i++)
                    {
                        var point=new Vector2(source[i].x,-source[i].y)*scale; if(moving&&velocity.x>=0f) point.x=-point.x; if(_core.IsDead) point=Rotate(point,fold*.25f);
                        point=Pose(point)*SpherePlacementRules.SourcePixelsToWorld; points[i]=new Vector3(point.x,-point.y,0f); colors[i]=new Color(1f,1f,1f,1f-progress*.92f);
                    }
                    var indices=new int[sprite.triangles.Length]; for(var i=0;i<indices.Length;i++) indices[i]=sprite.triangles[i];
                    _spriteMesh.Clear(); _spriteMesh.vertices=points; _spriteMesh.colors=colors; _spriteMesh.uv=sprite.uv; _spriteMesh.triangles=indices; _spriteMesh.RecalculateBounds(); _spriteMaterial.mainTexture=sprite.texture;
                    _body.Pose=Pose;
                    if(_core.IsDead)
                    {
                        var fragments=Mathf.Max(0f,(progress-.22f)/.78f);
                        for(var i=0;i<8;i++) { var a=i*Mathf.PI/4f+.18f; var center=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*16f*(.10f+fragments*.72f); var size=2f+fragments*2f;
                            var shape=new[]{new Vector2(0f,-size),new Vector2(size*.75f,0f),new Vector2(0f,size),new Vector2(-size*.75f,0f)}; for(var j=0;j<shape.Length;j++) shape[j]=center+Rotate(shape[j],a+fragments*1.3f);
                            _body.Polygon(shape,new Color(223f/255f,250f/255f,1f,(1f-fragments)*.72f),Color.clear,0f); }
                    }
                    else
                    {
                        var spark=.45f+.35f*Mathf.Sin(t*5.5f); _body.Arc(Vector2.zero,16f*1.18f,-.55f+t*.7f,.55f+t*.7f,new Color(100f/255f,232f/255f,1f,.24f+spark*.18f),1f);
                        for(var i=0;i<4;i++) { var a=t*.9f+i*Mathf.PI/2f; var radius=16f*(1.08f+.08f*Mathf.Sin(t*3f+i)); var center=new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.70f);
                            _body.Polygon(new[]{center+new Vector2(0f,-1.8f),center+new Vector2(1.8f,0f),center+new Vector2(0f,1.8f),center+new Vector2(-1.8f,0f)},new Color(220f/255f,252f/255f,1f,.34f+spark*.20f),Color.clear,0f); }
                    }
                }
                else DrawFoldedCharacter(id,t,tint);
            }
            else { _spriteRenderer.enabled=false; DrawFoldedCharacter(id,t,tint); }
            _body.Upload(_bodyMesh); DrawShield(t); _shield.Upload(_shieldMesh);
        }
        private void DrawFoldedCharacter(SphereCharacterId id,float t,Color tint)
        {
            Vector2 P(float x,float y)=>new Vector2(x*16f,y*16f); Vector2[] points;
            if(id==SphereCharacterId.Hunter) points=new[]{P(0,-1),P(.48f,-.12f),P(.34f,.72f),P(0,.44f),P(-.34f,.72f),P(-.48f,-.12f)};
            else if(id==SphereCharacterId.Engineer) points=new[]{P(-.72f,-.56f),P(.72f,-.56f),P(.72f,.56f),P(-.72f,.56f)};
            else if(id==SphereCharacterId.Berserker) points=new[]{P(-.82f,.40f),P(-.58f,-.62f),P(-.18f,-.92f),P(0,-.56f),P(.18f,-.92f),P(.58f,-.62f),P(.82f,.40f),P(.34f,.78f),P(-.34f,.78f)};
            else if(id==SphereCharacterId.Alchemist) points=new[]{P(-.48f,-.82f),P(.48f,-.82f),P(.62f,-.28f),P(.42f,.70f),P(0,.90f),P(-.42f,.70f),P(-.62f,-.28f)};
            else if(id==SphereCharacterId.Architect) points=new[]{P(0,-1),P(.78f,-.30f),P(.58f,.72f),P(0,.46f),P(-.58f,.72f),P(-.78f,-.30f)};
            else points=new[]{P(0,-1.02f),P(.74f,-.42f),P(.62f,.48f),P(0,.78f),P(-.62f,.48f),P(-.74f,-.42f)};
            _body.Polygon(points,new Color32(7,19,33,255),Alpha(tint,.90f),1.5f);
            if(id==SphereCharacterId.Hunter) { _body.Line(P(-.92f,-.34f),P(0,-.72f),Alpha(tint,.46f),1f); _body.Line(P(0,-.72f),P(.92f,-.34f),Alpha(tint,.46f),1f); }
            else if(id==SphereCharacterId.Engineer)
            { _body.Line(P(-.72f,0),P(.72f,0),Alpha(tint,.52f),1f); _body.Line(P(0,-.56f),P(0,.56f),Alpha(tint,.52f),1f);
                for(var i=0;i<4;i++) { var a=t*.6f+i*Mathf.PI/2f; var center=P(Mathf.Cos(a)*.86f,Mathf.Sin(a)*.62f); Rect(center,2f,tint); } }
            else if(id==SphereCharacterId.Alchemist) for(var i=0;i<3;i++) { var a=t*.9f+i*2.1f; _body.Ellipse(P(Mathf.Cos(a)*.9f,Mathf.Sin(a)*.45f),2+i,2+i,Alpha(tint,.55f)); }
            else if(id==SphereCharacterId.Architect)
            { _body.Line(P(0,-1),P(0,.46f),Alpha(tint,.52f),1.1f); _body.Line(P(-.78f,-.30f),P(.58f,.72f),Alpha(tint,.52f),1.1f); _body.Line(P(.78f,-.30f),P(-.58f,.72f),Alpha(tint,.52f),1.1f); }
            else if(id!=SphereCharacterId.Hunter&&id!=SphereCharacterId.Berserker&&id!=SphereCharacterId.Engineer&&id!=SphereCharacterId.Alchemist)
                for(var i=0;i<3;i++) { var a=t*1.3f+i*Mathf.PI*2f/3f; _body.Line(P(Mathf.Cos(a)*.35f,Mathf.Sin(a)*.30f),P(Mathf.Cos(a),Mathf.Sin(a)*.62f),Alpha(tint,.4f),1f); }
            _body.RadialCore(tint); _body.Pose=point=>point+new Vector2(0f,-1.6f);
            if(id==SphereCharacterId.Hunter) { var scan=Mathf.Sin(t*3.5f)*.5f+.5f; _body.Line(P(-.95f,.15f-scan*.25f),P(.95f,.15f-scan*.25f),Alpha(tint,.25f+scan*.3f),1.2f); }
            else if(id==SphereCharacterId.Engineer) for(var i=0;i<4;i++) { var a=t*.6f+i*Mathf.PI/2f; Rect(P(Mathf.Cos(a)*1.05f,Mathf.Sin(a)*.72f),3f,Alpha(tint,.34f)); }
            else if(id==SphereCharacterId.Berserker) { var pulse=1f+Mathf.Sin(t*7f)*.08f; _body.Line(P(-.9f,.55f*pulse),P(0,.85f*pulse),Alpha(tint,.58f),2f); _body.Line(P(0,.85f*pulse),P(.9f,.55f*pulse),Alpha(tint,.58f),2f); }
            else if(id==SphereCharacterId.Alchemist) for(var i=0;i<5;i++) { var a=t*.8f+i*1.2f; var radius=16f*(.85f+.15f*Mathf.Sin(t*2f+i)); _body.Ellipse(new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.6f),2.2f+i%2,2.2f+i%2,Alpha(tint,.38f)); }
            else if(id==SphereCharacterId.Architect) for(var i=0;i<4;i++) { var a=t*.7f+i*Mathf.PI/2f; var center=P(Mathf.Cos(a)*1.15f,Mathf.Sin(a)*.72f); _body.Line(center-new Vector2(5,0),center+new Vector2(5,0),Alpha(tint,.46f),1.2f); _body.Line(center-new Vector2(0,5),center+new Vector2(0,5),Alpha(tint,.46f),1.2f); }
        }
        private void Rect(Vector2 center,float half,Color color) => _body.Polygon(new[]{center+new Vector2(-half,-half),center+new Vector2(half,-half),center+new Vector2(half,half),center+new Vector2(-half,half)},color,Color.clear,0f);
        private void DrawShield(float t)
        {
            _shield.Clear(); var charges=_core.ShieldCharges; var active=charges>0&&_runtime.SourcePlayerShieldTimer>0f; _charges.gameObject.SetActive(active); if(!active) return;
            var pulse=1f+Mathf.Sin(t*8f)*.035f; const float radius=16f*1.38f; _shield.Pose=point=>point*pulse;
            var points=new Vector2[8]; for(var i=0;i<8;i++) { var a=-Mathf.PI/8f+i*Mathf.PI/4f+Mathf.Sin(t*1.8f)*.03f; var rr=radius*(1f+Mathf.Sin(t*4f+i)*.012f); points[i]=new Vector2(Mathf.Cos(a)*rr,Mathf.Sin(a)*rr*.88f); }
            _shield.Polygon(points,new Color(64f/255f,214f/255f,1f,.08f),new Color(99f/255f,230f/255f,1f,.88f),2.2f);
            _shield.Arc(Vector2.zero,radius*.86f,0f,Mathf.PI*2f,new Color(225f/255f,251f/255f,1f,.72f),1f);
            var recast=Mathf.Clamp01(_runtime.SourcePlayerShieldPulse/.85f); if(recast>.02f)
            { var wave=1f+(1f-recast)*.55f; var alpha=.20f+recast*.65f; _shield.Arc(Vector2.zero,radius*wave,0,Mathf.PI*2f,new Color(223f/255f,250f/255f,1f,alpha),2.2f+recast*1.8f); _shield.Arc(Vector2.zero,radius*(wave+.08f),0,Mathf.PI*2f,new Color(223f/255f,250f/255f,1f,alpha*.42f),5f); }
            for(var i=0;i<charges;i++) { var a=-Mathf.PI/2f+i*Mathf.PI*2f/charges+t*.45f; _shield.Ellipse(new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.88f),3.2f,3.2f,new Color32(223,250,255,255)); }
            _charges.text=charges.ToString(); _charges.transform.localPosition=new Vector3(0f,-3f*pulse*SpherePlacementRules.SourcePixelsToWorld,0f);
        }
        private void OnDestroy() { if(_bodyMesh!=null) Destroy(_bodyMesh); if(_shieldMesh!=null) Destroy(_shieldMesh); if(_spriteMesh!=null) Destroy(_spriteMesh); if(_material!=null) Destroy(_material); if(_spriteMaterial!=null) Destroy(_spriteMaterial); }
    }
    public sealed partial class EchoSphereRuntime
    {
        public SphereCharacterId SourcePlayerCharacter=>_sphereCharacterId;
        public float SourcePlayerShieldTimer=>_shieldTimer;
        public float SourcePlayerShieldPulse=>_shieldVisualPulse;
        public float SourcePlayerDeathProgress=>_playerCore!=null&&_playerCore.IsDead?Mathf.Clamp01(1f-_deathTimer/.90f):0f;
    }
}
