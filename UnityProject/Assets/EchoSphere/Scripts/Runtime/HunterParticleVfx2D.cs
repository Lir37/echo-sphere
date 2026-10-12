using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-sized diamond particle used by Hunter Mark and Hunt. Values arrive in
    /// source logical pixels, matching the particle records pushed by engineCombat.ts.
    /// </summary>
    public sealed class HunterParticleVfx2D : MonoBehaviour
    {
        private static Material _sharedMaterial;
        private readonly List<Vector3> _vertices = new List<Vector3>(4);
        private readonly List<Color> _colors = new List<Color>(4);
        private readonly List<int> _triangles = new List<int>(6);
        private EchoSphereRuntime _runtime;
        private Mesh _mesh;
        private Vector2 _velocityWorld;
        private float _life;
        private float _maxLife;
        private float _sizeWorld;
        private float _angle;
        private Color _color;

        public void Initialize(EchoSphereRuntime runtime, Vector2 sourceVelocityPixels, float lifetime,
            float sourceSizePixels, Color color)
        {
            _runtime = runtime;
            _velocityWorld = sourceVelocityPixels * SpherePlacementRules.SourcePixelsToWorld;
            _life = _maxLife = Mathf.Max(0.01f, lifetime);
            _sizeWorld = Mathf.Max(0f, sourceSizePixels) * SpherePlacementRules.SourcePixelsToWorld;
            _angle = Mathf.Atan2(_velocityWorld.y, Mathf.Abs(_velocityWorld.x) < 0.0001f ? 1f : _velocityWorld.x);
            _color = color;

            _mesh = new Mesh { name = "Hunter Source Diamond Particle" };
            _mesh.MarkDynamic();
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = _mesh;
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = SharedMaterial;
            meshRenderer.sortingOrder = 19;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            Rebuild();
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
                        _sharedMaterial = new Material(shader)
                        {
                            name = "ECHO SPHERE Hunter Particles (Source)",
                            hideFlags = HideFlags.HideAndDontSave
                        };
                    }
                }
                return _sharedMaterial;
            }
        }

        private void Update()
        {
            if (_runtime != null && _runtime.IsCombatSimulationPaused) return;
            var dt = Time.deltaTime;
            _life -= dt;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(_velocityWorld * dt);
            Rebuild();
        }

        private void Rebuild()
        {
            if (_mesh == null) return;
            var alpha = Mathf.Clamp01(_life / _maxLife);
            var size = _sizeWorld * (0.55f + alpha * 0.65f);
            var cosine = Mathf.Cos(_angle);
            var sine = Mathf.Sin(_angle);

            _vertices.Clear();
            _colors.Clear();
            _triangles.Clear();
            AddVertex(new Vector2(size * 1.8f, 0f), cosine, sine, alpha * 0.82f);
            AddVertex(new Vector2(0f, -size * 0.55f), cosine, sine, alpha * 0.82f);
            AddVertex(new Vector2(-size * 1.2f, 0f), cosine, sine, alpha * 0.82f);
            AddVertex(new Vector2(0f, size * 0.55f), cosine, sine, alpha * 0.82f);
            _triangles.Add(0); _triangles.Add(1); _triangles.Add(2);
            _triangles.Add(0); _triangles.Add(2); _triangles.Add(3);

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void AddVertex(Vector2 point, float cosine, float sine, float alpha)
        {
            var rotated = new Vector2(point.x * cosine - point.y * sine, point.x * sine + point.y * cosine);
            _vertices.Add(new Vector3(rotated.x, rotated.y, 0f));
            _colors.Add(new Color(_color.r, _color.g, _color.b, alpha));
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
