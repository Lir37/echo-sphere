using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    // renderer.ts: canonical background, drawVoidAmbient, drawVoidField and world bounds.
    public sealed class SourceBackdrop2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Camera _camera;
        private Mesh _mesh;
        private Material _material;
        private Mesh _boundsMesh;
        private Material _boundsMaterial;
        private GameObject _worldBounds;
        private GameObject _starsRoot;
        private readonly List<SpriteRenderer> _ambient = new List<SpriteRenderer>();
        private Vector2 _viewport;
        public void Initialize(EchoSphereRuntime runtime, Camera camera)
        {
            _runtime = runtime; _camera = camera;
            var shader = Resources.Load<Shader>("EchoSphere/SourceBackdrop");
            if (shader == null) return;
            _starsRoot = new GameObject("Source Backdrop Stars");
            _material = new Material(shader); _mesh = new Mesh { name = "Source Backdrop" };
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = _material; renderer.sortingOrder = -52;
            for (var i = 0; i < 120; i++)
            {
                var star = CreateStar("World Star", -47); var unit = SpherePlacementRules.SourcePixelsToWorld;
                star.transform.position = new Vector3((-1200f + i * 137.31f % 2400f) * unit, -(-1200f + i * 71.93f % 2400f) * unit, 2f);
                star.transform.localScale = Vector3.one * ((.35f + i % 3 * .18f) * 2f * unit);
                star.color = new Color(190f/255f, 230f/255f, 1f, .12f + i * 17 % 60 / 600f);
            }
            for (var i = 0; i < 90; i++) _ambient.Add(CreateStar("Ambient Star", -48));
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            // Canvas [10,8] dashed bounds in one mesh, keeping source pixel width.
            for (var edge = 0; edge < 4; edge++) for (var position = 0f; position < 2400f; position += 18f)
            {
                var a = edge == 0 ? new Vector2(-1200f + position, -1200f) : edge == 1 ? new Vector2(1200f, -1200f + position) : edge == 2 ? new Vector2(1200f - position, 1200f) : new Vector2(-1200f, 1200f - position);
                var length = Mathf.Min(10f, 2400f - position); var b = a + (edge == 0 ? Vector2.right : edge == 1 ? Vector2.up : edge == 2 ? Vector2.left : Vector2.down) * length;
                var delta = (b - a).normalized; var normal = new Vector2(-delta.y, delta.x) * .75f;
                var index = vertices.Count; vertices.Add(ToWorld(a-normal)); vertices.Add(ToWorld(a+normal)); vertices.Add(ToWorld(b+normal)); vertices.Add(ToWorld(b-normal));
                for(var i=0;i<4;i++) colors.Add(new Color(100f/255f,180f/255f,1f,.24f));
                triangles.AddRange(new[] {index,index+1,index+2,index,index+2,index+3});
            }
            _boundsMesh=new Mesh { name="Source Dashed Bounds" }; _boundsMesh.SetVertices(vertices); _boundsMesh.SetColors(colors); _boundsMesh.SetTriangles(triangles,0); _boundsMesh.RecalculateBounds();
            _boundsMaterial=new Material(Shader.Find("Sprites/Default")); _worldBounds=new GameObject("Source World Bounds");
            _worldBounds.AddComponent<MeshFilter>().sharedMesh=_boundsMesh; var boundsRenderer=_worldBounds.AddComponent<MeshRenderer>(); boundsRenderer.sharedMaterial=_boundsMaterial; boundsRenderer.sortingOrder=-46;
            LateUpdate();
        }
        private static Vector3 ToWorld(Vector2 point) => new Vector3(point.x * SpherePlacementRules.SourcePixelsToWorld, -point.y * SpherePlacementRules.SourcePixelsToWorld, 2f);
        private SpriteRenderer CreateStar(string name, int order)
        { var renderer = new GameObject(name).AddComponent<SpriteRenderer>(); renderer.transform.SetParent(_starsRoot.transform, false); renderer.sprite = RuntimeSpriteFactory.Disc; renderer.sortingOrder = order; return renderer; }
        private void LateUpdate()
        {
            if (_runtime == null || _camera == null || _material == null) return;
            var unit = SpherePlacementRules.SourcePixelsToWorld;
            var view = new Vector2(_camera.orthographicSize * 2f * _camera.aspect / unit, _camera.orthographicSize * 2f / unit);
            if (view != _viewport)
            {
                _viewport = view; var half = view * unit * .5f;
                _mesh.vertices = new[] { new Vector3(-half.x,-half.y,0f), new Vector3(half.x,-half.y,0f), new Vector3(half.x,half.y,0f), new Vector3(-half.x,half.y,0f) };
                _mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up }; _mesh.triangles = new[] { 0,1,2,0,2,3 }; _mesh.RecalculateBounds();
                _material.SetVector("_Viewport", new Vector4(view.x, view.y, 0f, 0f));
            }
            transform.position = new Vector3(_camera.transform.position.x, _camera.transform.position.y, 3f);
            var focus = new Vector2(_runtime.PlayerPosition.x / unit, -_runtime.PlayerPosition.y / unit);
            _material.SetVector("_Focus", new Vector4(focus.x, focus.y, 0f, 0f));
            var count = Mathf.Min(90, Mathf.FloorToInt(view.x * view.y / 11000f));
            for (var i = 0; i < _ambient.Count; i++)
            {
                var star = _ambient[i]; star.gameObject.SetActive(i < count); if (i >= count) continue;
                var x = i * 97.13f % view.x; var y = (i * 53.71f + _runtime.CurrentRunTime * (2f + i % 3)) % view.y;
                var point = new Vector2(_camera.transform.position.x / unit + x - view.x * .5f, -_camera.transform.position.y / unit + y - view.y * .5f);
                star.transform.position = ToWorld(point); star.transform.localScale = Vector3.one * ((.7f + i % 3 * .35f) * 2f * unit);
                var attenuation = 1f;
                if (Mathf.Abs(point.x) <= 1200f && Mathf.Abs(point.y) <= 1200f)
                {
                    var distance = Vector2.Distance(point, focus - new Vector2(0f, 336f)); var t = Mathf.Clamp01((distance - 10f) / 1862f);
                    var alpha = t <= .26f ? Mathf.Lerp(.18f, .10f, t / .26f) : t <= .62f ? Mathf.Lerp(.10f, .18f, (t - .26f) / .36f) : Mathf.Lerp(.18f, .82f, (t - .62f) / .38f);
                    attenuation *= 1f - alpha;
                }
                star.color = new Color(120f / 255f, 190f / 255f, 1f, (.28f + .22f * Mathf.Sin(_runtime.CurrentRunTime * 1.4f + i)) * .16f * attenuation);
            }
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); if (_mesh != null) Destroy(_mesh); if (_boundsMesh!=null) Destroy(_boundsMesh); if (_boundsMaterial!=null) Destroy(_boundsMaterial); if (_worldBounds!=null) Destroy(_worldBounds); if (_starsRoot!=null) Destroy(_starsRoot); }
    }
}
