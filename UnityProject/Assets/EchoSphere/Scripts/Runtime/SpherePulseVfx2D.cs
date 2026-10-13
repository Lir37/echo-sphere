using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped radial particles emitted by emitSpherePulse in src/engineCombat.ts.
    /// Presentation only: seeded angles are supplied by EchoSphereRuntime.
    /// </summary>
    public sealed class SpherePulseVfx2D : MonoBehaviour
    {
        private const float SourceLife = 0.35f;
        private static Material _sharedMaterial;
        private readonly List<Vector3> _vertices = new List<Vector3>(56);
        private readonly List<Color> _colors = new List<Color>(56);
        private readonly List<int> _triangles = new List<int>(84);
        private Mesh _mesh;
        private Color _color;
        private Vector2[] _velocities;
        private float _elapsed;
        private float _sourceScale;

        public void Initialize(Color color, float sourceScale, float[] angles)
        {
            _color = color;
            _sourceScale = Mathf.Max(0.0001f, sourceScale);
            var count = angles == null ? 0 : angles.Length;
            _velocities = new Vector2[count];
            for (var i = 0; i < count; i++)
                _velocities[i] = new Vector2(Mathf.Cos(angles[i]), Mathf.Sin(angles[i])) * (90f * _sourceScale);

            _mesh = new Mesh { name = "ECHO SPHERE Source Pulse Particles" };
            _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = 18;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material SharedMaterial
        {
            get
            {
                if (_sharedMaterial == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        _sharedMaterial = new Material(shader);
                        _sharedMaterial.name = "ECHO SPHERE Source Pulse Particles (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / SourceLife);
            Rebuild(t);
            if (t >= 1f) Destroy(gameObject);
        }

        private void Rebuild(float t)
        {
            if (_mesh == null || _velocities == null) return;
            _vertices.Clear();
            _colors.Clear();
            _triangles.Clear();
            var alpha = (1f - t) * 0.82f;
            var color = _color;
            color.a *= alpha;
            for (var i = 0; i < _velocities.Length; i++)
            {
                var velocity = _velocities[i];
                var position = velocity * _elapsed;
                var direction = velocity.sqrMagnitude > 0.000001f ? velocity.normalized : Vector2.right;
                var perpendicular = new Vector2(-direction.y, direction.x);
                var particleSize = 3f * _sourceScale * (0.55f + (1f - t) * 0.65f);
                var tip = position + direction * (particleSize * 1.8f);
                var upper = position - perpendicular * (particleSize * 0.55f);
                var tail = position - direction * (particleSize * 1.2f);
                var lower = position + perpendicular * (particleSize * 0.55f);
                var start = _vertices.Count;
                _vertices.Add(tip); _vertices.Add(upper); _vertices.Add(tail); _vertices.Add(lower);
                for (var v = 0; v < 4; v++) _colors.Add(color);
                _triangles.Add(start); _triangles.Add(start + 1); _triangles.Add(start + 2);
                _triangles.Add(start); _triangles.Add(start + 2); _triangles.Add(start + 3);
            }
            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
