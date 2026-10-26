using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    // Transient source boundary-flash VFX; RegionFieldVisuals owns persistent pocket geometry.
    public sealed class SourceRegionVisual2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private readonly SourceCanvasMesh _canvas = new SourceCanvasMesh();
        private Mesh _mesh;
        private Material _material;
        public void Initialize(EchoSphereRuntime runtime)
        {
            // The native frame already owns boundary flash/shards. Retain the
            // gameplay region owner, but do not build/upload a second visual mesh.
            if(runtime!=null&&runtime.UsesSourcePresentation){enabled=false;return;}
            _runtime = runtime; _mesh = new Mesh { name = "Source Region Seams" }; _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _material = new Material(Shader.Find("Sprites/Default"));
            var renderer = gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = _material; renderer.sortingOrder = -18;
            LateUpdate();
        }
        private void LateUpdate()
        {
            if (_runtime == null) return;
            // RegionFieldVisuals already owns the source-mapped pocket boundaries,
            // fold seams and boundary markers. Drawing the same pocket layer here
            // duplicated those rings and made them much brighter/thicker than renderer.ts.
            // Keep only the transient source boundary-flash effect in this component.
            _canvas.Clear();
            if (_runtime.SourceFlashBoundary)
            {
                var progress=1f-Mathf.Min(1f,_runtime.SourceFlashLife/1.35f); var fade=1f-progress;
                var player=_runtime.PlayerPosition/SpherePlacementRules.SourcePixelsToWorld; var at=new Vector2(player.x,-player.y);
                ColorUtility.TryParseHtmlString(_runtime.SourceFlashColor,out var color);
                color.a=.46f*fade; _canvas.Arc(at,24f+progress*58f,0f,Mathf.PI*2f,color,2.2f);
                color.a=.20f*fade; _canvas.Arc(at,36f+progress*96f,0f,Mathf.PI*2f,color,1f);
                color.a=.50f*fade;
                for(var i=0;i<4;i++)
                {
                    var a=i*Mathf.PI*.5f+progress*1.7f; var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a)); var normal=new Vector2(-direction.y,direction.x);
                    var point=at+direction*(30f+progress*62f); var size=5f+progress*4f;
                    _canvas.Polygon(new[] { point+direction*size,point+normal*size*.6f,point-normal*size*.6f },color,Color.clear,0f);
                }
            }
            _canvas.Upload(_mesh);
        }
        private void OnDestroy() { if (_mesh != null) Destroy(_mesh); if (_material != null) Destroy(_material); }
    }
    public sealed partial class EchoSphereRuntime
    {
        public string SourceRegionPocketId => _regionHudPocketId;
    }
}
