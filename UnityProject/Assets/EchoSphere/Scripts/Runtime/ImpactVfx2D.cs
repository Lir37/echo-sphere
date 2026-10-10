using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Layered source-mapped hit flash and shard burst, based on the native diamond particles
    /// in src/renderer.ts. Deterministic presentation only; no gameplay RNG or damage routing.
    /// </summary>
    public sealed class ImpactVfx2D : MonoBehaviour
    {
        private const float Tau = Mathf.PI * 2f;
        private static Material _sharedMaterial;
        private readonly Painter _painter = new Painter();
        private Mesh _mesh;
        private Color _color;
        private float _elapsed, _duration = 0.22f, _phase;

        public void Initialize(Color color, float duration = 0.22f)
        {
            _color = color;
            _duration = Mathf.Max(0.04f, duration);
            _phase = Mathf.Repeat(Mathf.Abs(transform.position.x * 0.713f + transform.position.y * 0.419f + color.r * 17f), Tau);
            var layer = new GameObject("Impact Ring and Shards");
            layer.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Impact Ring and Shards Mesh" };
            _mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = layer.AddComponent<MeshRenderer>();
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
                        _sharedMaterial.name = "ECHO SPHERE Impact VFX (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            Rebuild(t);
            if (t >= 1f) Destroy(gameObject);
        }

        private void Rebuild(float t)
        {
            if (_mesh == null) return;
            _painter.Clear();
            var fade = 1f - t;
            var radius = 0.12f + t * 0.50f;
            var baseColor = WithAlpha(_color, fade * 0.68f);
            var bright = WithAlpha(Color.Lerp(_color, Color.white, 0.38f), fade * 0.78f);
            _painter.Ring(Vector2.zero, radius, Mathf.Max(0.004f, 0.018f * fade), baseColor, 32);
            _painter.Ring(Vector2.zero, 0.07f + t * 0.34f, Mathf.Max(0.003f, 0.010f * fade), WithAlpha(_color, fade * 0.38f), 24, _phase + t * 0.72f, _phase + t * 0.72f + Mathf.PI * 1.62f);
            for (var i = 0; i < 8; i++)
            {
                var angle = _phase + i * Tau / 8f;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var point = direction * (0.12f + t * 0.30f);
                _painter.Line(point - direction * 0.055f, point + direction * (0.035f * fade), Mathf.Max(0.002f, 0.009f * fade), bright);
                _painter.Shard(point, Mathf.Max(0.004f, 0.035f * fade), _color, angle);
            }
            _painter.Circle(Vector2.zero, 0.018f * fade, bright, 10);
            Upload();
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }

        private void Upload()
        {
            _mesh.Clear();
            _mesh.SetVertices(_painter.Vertices);
            _mesh.SetColors(_painter.Colors);
            _mesh.SetTriangles(_painter.Triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }

        private sealed class Painter
        {
            public readonly List<Vector3> Vertices = new List<Vector3>(256);
            public readonly List<Color> Colors = new List<Color>(256);
            public readonly List<int> Triangles = new List<int>(768);

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
                    var angle = i * Tau / segments;
                    var next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                    Triangle(center, previous, next, color);
                    previous = next;
                }
            }

            public void Ring(Vector2 center, float radius, float width, Color color, int segments, float start = 0f, float end = Tau)
            {
                var step = (end - start) / Mathf.Max(1, segments);
                for (var i = 0; i < segments; i++)
                {
                    var a0 = start + i * step;
                    var a1 = a0 + step;
                    var outer0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * (radius + width * 0.5f);
                    var outer1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * (radius + width * 0.5f);
                    var inner1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * Mathf.Max(0.001f, radius - width * 0.5f);
                    var inner0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * Mathf.Max(0.001f, radius - width * 0.5f);
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
            }

            private static Vector2 Rotate(Vector2 p, float angle)
            {
                var sin = Mathf.Sin(angle); var cos = Mathf.Cos(angle);
                return new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos);
            }
        }
    }
}