using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped short-lived attack arcs. Chain lightning follows
    /// src/spheres/mutationVisual.ts:renderChainLightningVfx; Prism refraction
    /// follows the energy-bolt presentation in src/renderer.ts.
    /// All geometry is cosmetic and consumes no gameplay RNG.
    /// </summary>
    public sealed class CombatArcVfx2D : MonoBehaviour
    {
        private const float Tau = Mathf.PI * 2f;
        private static Material _sharedMaterial;
        private readonly Painter _painter = new Painter(192);
        private readonly List<Vector2> _points = new List<Vector2>(16);
        private EchoSphereRuntime _runtime;
        private Mesh _mesh;
        private SphereId _family;
        private string _branch;
        private string _final;
        private Vector2 _end;
        private Color _color;
        private float _elapsed;
        private float _duration = 0.30f;
        private float _phase;

        public void Initialize(
            EchoSphereRuntime runtime,
            Vector2 from,
            Vector2 to,
            SphereId family,
            string branch,
            string finalId,
            Color color,
            float duration = 0.30f)
        {
            _runtime = runtime;
            _family = family;
            _branch = branch ?? string.Empty;
            _final = finalId ?? string.Empty;
            _end = to - from;
            _color = color;
            _duration = Mathf.Max(0.08f, duration);
            _phase = Mathf.Repeat(Mathf.Abs(from.x * 0.713f + from.y * 0.419f + _end.sqrMagnitude * 1.73f), Tau);
            transform.position = from;

            var layer = new GameObject("Combat Arc Mesh");
            layer.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Combat Arc Mesh" };
            _mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = 19;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Rebuild(1f);
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
                        _sharedMaterial.name = "ECHO SPHERE Combat Arc VFX (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private void Update()
        {
            if (_runtime != null && _runtime.IsPresentationPaused) return;
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            Rebuild(1f - t);
            if (t >= 1f) Destroy(gameObject);
        }

        private void Rebuild(float fade)
        {
            if (_mesh == null) return;
            _painter.Clear();
            _points.Clear();

            var length = _end.magnitude;
            if (length < 0.025f)
            {
                Upload();
                return;
            }

            var direction = _end / length;
            var normal = new Vector2(-direction.y, direction.x);
            var prism = _family == SphereId.Prism;
            var segments = prism ? 5 : _branch == "chain_storm" ? 8 : _branch == "chain_web" ? 10 : 12;
            var amplitudeRatio = _branch == "chain_storm" ? 0.12f
                : _branch == "chain_leech" ? 0.06f : prism ? 0.025f : 0.09f;
            var amplitude = Mathf.Min(length * 0.20f, Mathf.Max(0.025f, length * amplitudeRatio));
            var flicker = 0.76f + 0.24f * Mathf.Sin(_elapsed * 22f + _phase);
            _points.Add(Vector2.zero);
            for (var i = 1; i < segments; i++)
            {
                var q = i / (float)segments;
                var alternating = (i & 1) == 0 ? -1f : 1f;
                var offset = alternating * amplitude * flicker;
                if (prism) offset += Mathf.Sin(_phase + i * 1.7f) * amplitude * 0.26f;
                _points.Add(_end * q + normal * offset);
            }
            _points.Add(_end);

            var glowWidth = Mathf.Max(0.045f, length * (prism ? 0.010f : 0.015f));
            var coreWidth = Mathf.Max(0.016f, length * (prism ? 0.0037f : 0.0045f));
            var glowColor = WithAlpha(_color, fade * (prism ? 0.26f : 0.24f));
            var coreColor = WithAlpha(new Color(0.96f, 0.99f, 1f, 1f), fade * 0.90f);

            for (var i = 1; i < _points.Count; i++)
            {
                var a = _points[i - 1];
                var b = _points[i];
                _painter.Line(a, b, glowWidth * (0.96f + 0.04f * flicker), glowColor);
                _painter.Line(a, b, coreWidth, coreColor);

                if (prism)
                {
                    var delta = b - a;
                    if (delta.sqrMagnitude > 0.0001f)
                    {
                        var side = new Vector2(-delta.y, delta.x).normalized * 0.030f;
                        var rail = WithAlpha(_color, fade * 0.32f);
                        _painter.Line(a + side, b + side, Mathf.Max(0.008f, coreWidth * 0.55f), rail);
                        _painter.Line(a - side, b - side, Mathf.Max(0.008f, coreWidth * 0.55f), rail);
                    }
                }
                else if (_branch == "chain_storm" && i >= 2 && i < _points.Count - 1 && (i & 1) == 0)
                {
                    var delta = _points[i + 1] - a;
                    var side = delta.sqrMagnitude > 0.0001f
                        ? new Vector2(-delta.y, delta.x).normalized : normal;
                    var sign = i % 4 == 0 ? 1f : -1f;
                    var forkEnd = b + side * (length * 0.085f * sign) + direction * (length * 0.045f);
                    _painter.Line(b, forkEnd, Mathf.Max(0.012f, coreWidth * 0.70f), WithAlpha(_color, fade * 0.78f));
                    _painter.Line(b, forkEnd, Mathf.Max(0.006f, coreWidth * 0.28f), WithAlpha(Color.white, fade * 0.84f));
                }
            }

            DrawEndpoint(fade, prism);
            Upload();
        }

        private void DrawEndpoint(float fade, bool prism)
        {
            var radius = prism ? 0.070f : _branch == "chain_storm" ? 0.075f : 0.055f;
            var pale = WithAlpha(new Color(1f, 0.98f, 0.78f, 1f), fade * 0.92f);
            _painter.Ring(_end, radius, Mathf.Max(0.008f, radius * 0.18f), WithAlpha(_color, fade * 0.88f), 12,
                _phase * 0.2f, _phase * 0.2f + Tau);
            _painter.Circle(_end, radius * (prism ? 0.23f : 0.20f), pale, 8);

            if (_family == SphereId.Chain && _branch == "chain_web")
            {
                for (var i = 0; i < 4; i++)
                {
                    var angle = _phase + i * Tau / 4f + _elapsed * 2f;
                    var ray = new Vector2(Mathf.Cos(angle) * 0.13f, Mathf.Sin(angle) * 0.10f);
                    _painter.Line(_end, _end + ray, 0.012f, WithAlpha(new Color(1f, 0.95f, 0.58f, 1f), fade * 0.70f));
                }
            }
            else if (_family == SphereId.Chain && _branch == "chain_storm")
            {
                _painter.Ring(_end, radius * 1.75f, 0.010f, WithAlpha(new Color(1f, 0.94f, 0.60f, 1f), fade * 0.76f),
                    14, _elapsed * 1.8f, _elapsed * 1.8f + Tau * 0.88f);
            }
            else if (prism)
            {
                var direction = _end.sqrMagnitude > 0.0001f ? _end.normalized : Vector2.right;
                var side = new Vector2(-direction.y, direction.x);
                var facet = WithAlpha(new Color(1f, 0.93f, 1f, 1f), fade * 0.86f);
                _painter.Line(_end - side * 0.075f, _end + direction * 0.075f, 0.014f, facet);
                _painter.Line(_end + direction * 0.075f, _end + side * 0.075f, 0.014f, facet);
                if (_branch == "prism_mirror")
                {
                    _painter.Line(_end, _end + direction * 0.12f + side * 0.10f, 0.012f, WithAlpha(_color, fade * 0.78f));
                    _painter.Line(_end, _end + direction * 0.12f - side * 0.10f, 0.012f, WithAlpha(_color, fade * 0.78f));
                }
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
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
            public readonly List<Vector3> Vertices;
            public readonly List<Color> Colors;
            public readonly List<int> Triangles;

            public Painter(int capacity)
            {
                Vertices = new List<Vector3>(capacity);
                Colors = new List<Color>(capacity);
                Triangles = new List<int>(capacity * 3);
            }

            public void Clear()
            {
                Vertices.Clear();
                Colors.Clear();
                Triangles.Clear();
            }

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

            public void Line(Vector2 from, Vector2 to, float width, Color color)
            {
                var delta = to - from;
                if (delta.sqrMagnitude < 0.000001f) return;
                var normal = new Vector2(-delta.y, delta.x).normalized * (width * 0.5f);
                Quad(from - normal, from + normal, to + normal, to - normal, color);
            }

            public void Circle(Vector2 center, float radius, Color color, int segments)
            {
                for (var i = 0; i < segments; i++)
                {
                    var a = i * Tau / segments;
                    var b = (i + 1) * Tau / segments;
                    var p0 = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    var p1 = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius;
                    Triangle(center, p0, p1, color);
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
        }
    }
}