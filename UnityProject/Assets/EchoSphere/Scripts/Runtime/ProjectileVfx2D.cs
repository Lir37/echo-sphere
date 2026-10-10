using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped projectile body and tapered energy trail.
    /// Ported from src/spheres/mutationVisual.ts: renderSphereProjectileVfx,
    /// projectileTail and projectileCore. This component is presentation-only.
    /// </summary>
    public sealed class ProjectileVfx2D : MonoBehaviour
    {
        private const float R = 0.068f;
        private static Material _sharedMaterial;
        private readonly Painter _tail = new Painter(32);
        private readonly Painter _core = new Painter(128);
        private EchoSphereRuntime _runtime;
        private SphereAttackAgent _owner;
        private Transform _visualRoot;
        private Mesh _tailMesh, _coreMesh;
        private float _time, _phase;
        private float _speed;
        private Vector2 _direction;
        private Color _color;
        private SphereId _type;
        private string _branch, _final;

        public void Initialize(EchoSphereRuntime runtime, Vector2 direction, float speed, Color color, SphereAttackAgent owner)
        {
            _runtime = runtime;
            _owner = owner;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(0f, speed);
            _color = color;
            _type = owner != null ? owner.Type : SphereId.Standard;
            if (_runtime != null && owner != null)
            {
                _branch = _runtime.GetSphereBranch(_type);
                _final = _runtime.GetSphereFinal(_type);
            }
            _phase = ((int)_type + 1) * 0.6180339f;
            _visualRoot = new GameObject("Projectile Authored Visual Axis").transform;
            _visualRoot.SetParent(transform, false);
            _visualRoot.localPosition = Vector3.zero;
            _visualRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
            CreateLayer("Projectile Tapered Trail", 11, out _tailMesh);
            CreateLayer("Projectile Energy Core", 13, out _coreMesh);
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
                        _sharedMaterial.name = "ECHO SPHERE Projectile VFX (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private void CreateLayer(string name, int sortingOrder, out Mesh mesh)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(_visualRoot, false);
            mesh = new Mesh { name = name + " Mesh" };
            mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _visualRoot == null) return;
            _time += Time.deltaTime;
            if (_owner != null)
            {
                var branch = _runtime.GetSphereBranch(_owner.Type);
                if (branch != _branch)
                {
                    _branch = branch;
                    _final = _runtime.GetSphereFinal(_owner.Type);
                }
            }
            Rebuild();
        }

        private void Rebuild()
        {
            if (_tailMesh == null || _coreMesh == null) return;
            BuildTail();
            BuildCore();
            Upload(_tailMesh, _tail);
            Upload(_coreMesh, _core);
        }

        private void BuildTail()
        {
            _tail.Clear();
            var length = Mathf.Clamp(0.18f + _speed * 0.045f, 0.24f, 0.62f);
            var width = R * 0.82f;
            var rear = new Vector2(-length, 0f);
            var upperNear = new Vector2(-length * 0.35f, -width);
            var upperFar = new Vector2(-length * 0.86f, -width * 0.12f);
            var lowerFar = new Vector2(-length * 0.86f, width * 0.12f);
            var lowerNear = new Vector2(-length * 0.35f, width);
            _tail.Triangle(Vector2.zero, upperNear, upperFar, WithAlpha(_color, 0.17f));
            _tail.Triangle(Vector2.zero, upperFar, rear, WithAlpha(_color, 0.17f));
            _tail.Triangle(Vector2.zero, rear, lowerFar, WithAlpha(_color, 0.17f));
            _tail.Triangle(Vector2.zero, lowerFar, lowerNear, WithAlpha(_color, 0.17f));
            _tail.Line(new Vector2(-length * 0.88f, 0f), new Vector2(-R * 0.1f, 0f),
                Mathf.Max(0.004f, R * 0.65f), WithAlpha(_color, 0.10f));
        }

        private void BuildCore()
        {
            _core.Clear();
            var glow = WithAlpha(_color, 0.18f);
            _core.Circle(Vector2.zero, R * 1.65f, WithAlpha(_color, 0.18f), 16);
            _core.Circle(Vector2.zero, R * 0.65f, WithAlpha(_color, 0.12f), 12);

            var white = new Color(1f, 1f, 1f, 0.92f * _color.a);
            switch (_type)
            {
                case SphereId.Standard:
                    _core.Line(new Vector2(-R * 0.9f, 0f), new Vector2(R * 1.35f, 0f), Mathf.Max(0.003f, R * 0.17f), _color);
                    _core.Ring(new Vector2(R * 0.22f, 0f), R * 0.30f, R * 0.30f, 0f, 0f, Mathf.PI * 2f, Mathf.Max(0.003f, R * 0.17f), _color, 12);
                    if (_branch == "standard_singularity")
                    {
                        _core.Circle(Vector2.zero, R * 0.62f, new Color(0.025f, 0.008f, 0.055f, 0.94f), 12);
                        _core.Ring(Vector2.zero, R * 0.82f, R * 0.82f, 0f, 0.4f, 2.55f, R * 0.14f, new Color(0.84f, 0.70f, 1f, 0.9f), 12);
                    }
                    else if (_branch == "standard_swarm")
                    {
                        _core.Shard(new Vector2(-R * 0.42f, 0f), R * 0.25f, _color, -0.18f);
                        _core.Shard(Vector2.zero, R * 0.25f, _color, 0f);
                        _core.Shard(new Vector2(R * 0.42f, 0f), R * 0.25f, _color, 0.18f);
                    }
                    break;
                case SphereId.Sniper:
                    _core.Line(new Vector2(-R * 0.75f, 0f), new Vector2(R * 1.35f, 0f), Mathf.Max(0.003f, R * 0.14f), _color);
                    if (_branch == "sniper_oracle")
                        _core.Ring(new Vector2(R * 0.35f, 0f), R * 0.50f, R * 0.50f, 0f, 0f, Mathf.PI * 2f, R * 0.11f, _color, 12);
                    else if (_branch == "sniper_assassin")
                        _core.Fin(new Vector2(R * 0.18f, 0f), R * (0.85f + 0.15f * Mathf.Max(0, FinalIndex(_final))), R * 0.18f, _color, 0f);
                    else
                        _core.Ring(Vector2.zero, R, R, 0f, -0.8f, 0.8f, R * 0.12f, _color, 10);
                    break;
                case SphereId.Shotgun:
                    var pelletCount = _branch == "shotgun_cataclysm" ? 1 : _branch == "shotgun_hail" ? 4 : 3;
                    for (var i = 0; i < pelletCount; i++)
                    {
                        var y = (i - (pelletCount - 1) * 0.5f) * R * 0.34f;
                        _core.Line(new Vector2(-R * 0.70f, y), new Vector2(R * 0.92f, y), Mathf.Max(0.0025f, R * 0.10f), _color);
                    }
                    _core.Circle(new Vector2(R * 0.58f, 0f), R * 0.18f, white, 8);
                    break;
                case SphereId.Prism:
                    var shardCount = _branch == "prism_split" ? 3 : _branch == "prism_spectrum" ? 5 : 2;
                    for (var i = 0; i < shardCount; i++)
                    {
                        var y = (i - (shardCount - 1) * 0.5f) * R * 0.38f;
                        _core.Shard(new Vector2(0f, y), R * 0.34f, _color, 0f);
                    }
                    _core.Line(new Vector2(-R * 0.28f, 0f), new Vector2(R * 1.04f, 0f), R * 0.07f, white);
                    break;
                case SphereId.Void:
                    if (_branch == "void_hunger")
                    {
                        _core.Circle(Vector2.zero, R * 0.68f, new Color(0.025f, 0.008f, 0.055f, 0.96f), 12);
                        _core.Ring(Vector2.zero, R * 0.42f, R * 0.42f, 0f, 0.2f, 5.4f, R * 0.13f, _color, 18);
                    }
                    else if (_branch == "void_reaper")
                        _core.Fin(Vector2.zero, R * 0.95f, R * 0.24f, _color, 0f);
                    else
                    {
                        _core.Line(new Vector2(-R * 0.44f, 0f), new Vector2(R * 0.9f, 0f), R * 0.07f, _color);
                        _core.Line(new Vector2(R * 0.55f, -R * 0.25f), new Vector2(R * 0.9f, 0f), R * 0.07f, _color);
                        _core.Line(new Vector2(R * 0.9f, 0f), new Vector2(R * 0.55f, R * 0.25f), R * 0.07f, _color);
                    }
                    break;
                default:
                    _core.Line(new Vector2(-R * 0.5f, 0f), new Vector2(R * 0.65f, 0f), R * 0.10f, _color);
                    _core.Shard(new Vector2(R * 0.15f, 0f), R * 0.42f, _color, 0f);
                    break;
            }

            _core.Circle(new Vector2(R * 0.12f, 0f), R * 0.15f, WithAlpha(white, 0.78f), 8);
        }

        private static int FinalIndex(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            var separator = id.LastIndexOf('_');
            int result;
            return separator >= 0 && int.TryParse(id.Substring(separator + 1), out result) ? result : -1;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }

        private static void Upload(Mesh mesh, Painter painter)
        {
            mesh.Clear();
            mesh.SetVertices(painter.Vertices);
            mesh.SetColors(painter.Colors);
            mesh.SetTriangles(painter.Triangles, 0);
            mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_tailMesh != null) Destroy(_tailMesh);
            if (_coreMesh != null) Destroy(_coreMesh);
        }

        private sealed class Painter
        {
            public readonly List<Vector3> Vertices;
            public readonly List<Color> Colors;
            public readonly List<int> Triangles;

            public Painter(int capacity)
            {
                Vertices = new List<Vector3>(capacity);
                Colors = new List<Color>(capacity);
                Triangles = new List<int>(capacity * 3);
            }

            public void Clear() { Vertices.Clear(); Colors.Clear(); Triangles.Clear(); }

            public void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color)
            {
                var start = Vertices.Count;
                Vertices.Add(new Vector3(a.x, a.y, 0f));
                Vertices.Add(new Vector3(b.x, b.y, 0f));
                Vertices.Add(new Vector3(c.x, c.y, 0f));
                Colors.Add(color); Colors.Add(color); Colors.Add(color);
                Triangles.Add(start); Triangles.Add(start + 1); Triangles.Add(start + 2);
            }

            public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
            {
                Triangle(a, b, c, color);
                Triangle(a, c, d, color);
            }

            public void Line(Vector2 start, Vector2 end, float width, Color color)
            {
                var delta = end - start;
                if (delta.sqrMagnitude < 0.000001f) return;
                var normal = new Vector2(-delta.y, delta.x).normalized * (width * 0.5f);
                Quad(start - normal, start + normal, end + normal, end - normal, color);
            }

            public void Circle(Vector2 center, float radius, Color color, int segments)
            {
                if (radius <= 0f) return;
                var previous = center + new Vector2(radius, 0f);
                for (var i = 1; i <= segments; i++)
                {
                    var angle = i * Mathf.PI * 2f / segments;
                    var next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                    Triangle(center, previous, next, color);
                    previous = next;
                }
            }

            public void Ring(Vector2 center, float rx, float ry, float rotation, float startAngle, float endAngle, float width, Color color, int segments)
            {
                var step = (endAngle - startAngle) / Mathf.Max(1, segments);
                for (var i = 0; i < segments; i++)
                {
                    var a0 = startAngle + i * step;
                    var a1 = a0 + step;
                    var outer0 = EllipsePoint(center, rx + width * 0.5f, ry + width * 0.5f, rotation, a0);
                    var outer1 = EllipsePoint(center, rx + width * 0.5f, ry + width * 0.5f, rotation, a1);
                    var inner1 = EllipsePoint(center, Mathf.Max(0.001f, rx - width * 0.5f), Mathf.Max(0.001f, ry - width * 0.5f), rotation, a1);
                    var inner0 = EllipsePoint(center, Mathf.Max(0.001f, rx - width * 0.5f), Mathf.Max(0.001f, ry - width * 0.5f), rotation, a0);
                    Quad(outer0, outer1, inner1, inner0, color);
                }
            }

            public void Shard(Vector2 center, float size, Color color, float rotation)
            {
                var p0 = center + Rotate(new Vector2(size, 0f), rotation);
                var p1 = center + Rotate(new Vector2(0f, -size * 0.54f), rotation);
                var p2 = center + Rotate(new Vector2(-size * 0.68f, -size * 0.25f), rotation);
                var p3 = center + Rotate(new Vector2(-size * 0.44f, size * 0.28f), rotation);
                Triangle(p0, p1, p2, color);
                Triangle(p0, p2, p3, color);
                Line(p0, p1, Mathf.Max(0.001f, size * 0.08f), WithAlpha(color, 0.85f));
                Line(p1, p2, Mathf.Max(0.001f, size * 0.08f), WithAlpha(color, 0.85f));
                Line(p2, p3, Mathf.Max(0.001f, size * 0.08f), WithAlpha(color, 0.85f));
                Line(p3, p0, Mathf.Max(0.001f, size * 0.08f), WithAlpha(color, 0.85f));
            }

            public void Fin(Vector2 center, float length, float width, Color color, float rotation)
            {
                var direction = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
                var normal = new Vector2(-direction.y, direction.x);
                var root = center - direction * length * 0.32f;
                var mid = center + direction * length * 0.08f;
                var tip = center + direction * length * 0.68f;
                Triangle(root - normal * width * 0.20f, mid - normal * width * 0.48f, tip, color);
                Triangle(root + normal * width * 0.20f, tip, mid + normal * width * 0.48f, color);
                Line(root, tip, Mathf.Max(0.001f, width * 0.06f), WithAlpha(Color.white, 0.60f));
            }

            private static Vector2 Rotate(Vector2 p, float angle)
            {
                var sin = Mathf.Sin(angle); var cos = Mathf.Cos(angle);
                return new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos);
            }

            private static Vector2 EllipsePoint(Vector2 center, float rx, float ry, float rotation, float angle)
            {
                var x = Mathf.Cos(angle) * rx;
                var y = Mathf.Sin(angle) * ry;
                var p = Rotate(new Vector2(x, y), rotation);
                return center + p;
            }
        }
    }
}