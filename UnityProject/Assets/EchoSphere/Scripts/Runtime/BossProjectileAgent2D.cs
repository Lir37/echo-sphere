using System.Collections.Generic;
using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>Source-mapped projectile body from renderer.ts:drawBossProjectile.</summary>
    public sealed class BossProjectileAgent2D : MonoBehaviour, ISourceSimulationAgent
    {
        private static Material _sharedMaterial;
        private readonly List<Vector3> _vertices = new List<Vector3>(180);
        private readonly List<Color> _colors = new List<Color>(180);
        private readonly List<int> _triangles = new List<int>(300);
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private Mesh _mesh;
        private Vector2 _velocity;
        private float _damage, _hitRadius, _time;
        private Color _color;
        private string _bossType = "";
        private EnemyAgent2D _sourceOwner;
        private bool _hasSourceOwner, _removed;
        public bool HasSourceOwner => _hasSourceOwner;
        public EnemyAgent2D SourceOwner => _sourceOwner;
        public void SetSourceOwner(EnemyAgent2D owner) { _sourceOwner = owner; _hasSourceOwner = owner != null; }

        public void Initialize(EchoSphereRuntime runtime, Transform player, Vector2 velocity,
            float sourceRadius, float damage, string bossType, Color color)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _player = player;
            _velocity = velocity;
            _damage = Mathf.Max(0f, damage);
            _color = color;
            _bossType = bossType ?? "";
            _hitRadius = Mathf.Max(0.02f, sourceRadius / BossCombatRules.SourceUnitsPerWorldUnit) + BossCombatRules.PlayerRadiusWorld;
            transform.localScale = Vector3.one * Mathf.Max(0.04f, sourceRadius / BossCombatRules.SourceUnitsPerWorldUnit);
            if (_velocity.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg);

            _mesh = new Mesh { name = "ECHO SPHERE Boss Projectile Mesh" };
            _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = 6;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
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
                        _sharedMaterial = new Material(shader);
                        _sharedMaterial.name = "ECHO SPHERE Boss Projectile (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        public void SimulationTick(float dt)
        {
            if (_removed || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            _time += dt;
            transform.position += (Vector3)(_velocity * dt);
            if (Vector2.Distance(transform.position, _player.position) < _hitRadius)
            {
                _runtime.TryDamagePlayer(_damage);
                _removed = true;
                Destroy(gameObject);
                return;
            }

            var delta = (Vector2)transform.position - (Vector2)_player.position;
            if (Mathf.Abs(delta.x) > 1000f * SpherePlacementRules.SourcePixelsToWorld || Mathf.Abs(delta.y) > 1000f * SpherePlacementRules.SourcePixelsToWorld)
            {
                _removed = true;
                Destroy(gameObject);
                return;
            }
            Rebuild();
        }

        private void Rebuild()
        {
            if (_mesh == null) return;
            _vertices.Clear(); _colors.Clear(); _triangles.Clear();
            AddLine(new Vector2(-3.8f, 0f), new Vector2(-0.35f, 0f), 0.42f, new Color(_color.r, _color.g, _color.b, 0.22f));
            AddLine(new Vector2(-3.2f, 0f), new Vector2(-0.4f, 0f), 0.08f, new Color(0.96f, 0.99f, 1f, 0.66f));

            if (_bossType == "shooter")
            {
                AddPolygon(new[] { new Vector2(1.35f, 0f), new Vector2(0.25f, -0.75f), new Vector2(-0.8f, -0.42f), new Vector2(-0.45f, 0.42f), new Vector2(0.25f, 0.75f) },
                    new Color(0.02f, 0.06f, 0.11f, 0.98f), new Color(_color.r, _color.g, _color.b, 0.95f), 0.10f);
                for (var i = 0; i < 3; i++)
                {
                    var a = _time * 4f + i * 2.1f;
                    AddArc(0f, 0f, 1.05f + i * 0.18f, a, 1.1f, new Color(_color.r, _color.g, _color.b, 0.55f), 0.05f);
                }
            }
            else if (_bossType == "charger")
            {
                AddPolygon(new[] { new Vector2(1.5f, 0f), new Vector2(-0.45f, -0.62f), new Vector2(-0.95f, 0f), new Vector2(-0.45f, 0.62f) },
                    new Color(0.08f, 0.04f, 0.02f, 0.98f), new Color(_color.r, _color.g, _color.b, 0.95f), 0.11f);
                AddLine(new Vector2(-0.1f, -0.25f), new Vector2(0.9f, -0.55f), 0.07f, new Color(1f, 0.91f, 0.75f, 0.72f));
                AddLine(new Vector2(-0.1f, 0.25f), new Vector2(0.9f, 0.55f), 0.07f, new Color(1f, 0.91f, 0.75f, 0.72f));
            }
            else if (_bossType == "summoner")
            {
                AddCircle(0f, 0f, 0.65f, new Color(0.04f, 0.02f, 0.09f, 0.98f), new Color(_color.r, _color.g, _color.b, 0.95f), 0.10f);
                for (var i = 0; i < 5; i++)
                {
                    var a = _time * 2.6f + i * Mathf.PI * 2f / 5f;
                    var p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.15f;
                    AddCircle(p.x, p.y, 0.12f, new Color(0.85f, 0.78f, 1f, 0.96f), new Color(_color.r, _color.g, _color.b, 0.7f), 0.035f);
                }
            }
            else
            {
                AddCircle(0f, 0f, 0.72f, new Color(0.07f, 0.02f, 0.07f, 0.98f), new Color(_color.r, _color.g, _color.b, 0.95f), 0.10f);
                for (var i = 0; i < 4; i++)
                {
                    var a = _time * 3f + i * Mathf.PI * 0.5f;
                    AddArc(0f, 0f, 0.95f + i * 0.22f, a, 0.95f, new Color(_color.r, _color.g, _color.b, 0.68f), 0.055f);
                }
            }
            AddCircle(0.35f, 0f, 0.22f * (1f + 0.12f * Mathf.Sin(_time * 10f)),
                new Color(_color.r, _color.g, _color.b, 0.70f), Color.white, 0.025f);
            _mesh.Clear(); _mesh.SetVertices(_vertices); _mesh.SetColors(_colors); _mesh.SetTriangles(_triangles, 0); _mesh.RecalculateBounds();
        }

        private void AddLine(Vector2 a, Vector2 b, float width, Color color)
        {
            var d = b - a; if (d.sqrMagnitude < 0.0000001f || width <= 0f) return;
            var n = new Vector2(-d.y, d.x).normalized * width * 0.5f;
            var i = _vertices.Count;
            Vertex(a + n, color); Vertex(a - n, color); Vertex(b - n, color); Vertex(b + n, color);
            Triangle(i, i + 1, i + 2); Triangle(i, i + 2, i + 3);
        }

        private void AddPolygon(Vector2[] points, Color fill, Color outline, float width)
        {
            if (points == null || points.Length < 3) return;
            var center = Vector2.zero; for (var i = 0; i < points.Length; i++) center += points[i];
            center /= points.Length;
            var ci = Vertex(center, fill); var first = _vertices.Count;
            for (var i = 0; i < points.Length; i++) Vertex(points[i], fill);
            for (var i = 0; i < points.Length; i++) Triangle(ci, first + i, first + (i + 1) % points.Length);
            for (var i = 0; i < points.Length; i++) AddLine(points[i], points[(i + 1) % points.Length], width, outline);
        }

        private void AddCircle(float x, float y, float radius, Color fill, Color outline, float width)
        {
            const int n = 20;
            var points = new Vector2[n];
            for (var i = 0; i < n; i++) { var a = Mathf.PI * 2f * i / n; points[i] = new Vector2(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius); }
            AddPolygon(points, fill, outline, width);
        }

        private void AddArc(float x, float y, float radius, float start, float sweep, Color color, float width)
        {
            const int max = 16;
            var count = Mathf.Max(2, Mathf.CeilToInt(max * Mathf.Abs(sweep) / (Mathf.PI * 2f)));
            var from = new Vector2(x + Mathf.Cos(start) * radius, y + Mathf.Sin(start) * radius);
            for (var i = 1; i <= count; i++)
            {
                var a = start + sweep * i / count;
                var to = new Vector2(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius);
                AddLine(from, to, width, color); from = to;
            }
        }

        private int Vertex(Vector2 p, Color c) { var i = _vertices.Count; _vertices.Add(new Vector3(p.x, p.y, 0f)); _colors.Add(c); return i; }
        private void Triangle(int a, int b, int c) { _triangles.Add(a); _triangles.Add(b); _triangles.Add(c); }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying) Destroy(_mesh);
                else DestroyImmediate(_mesh);
            }
        }
    }
}
