using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped Fire / Freeze / Poison orbit, particles and directional weapon accent.
    /// Ported from src/spheres/elementalVisual.ts and branch mappings in src/sphereProgression.ts.
    /// </summary>
    public sealed class SphereElementalVfx2D : MonoBehaviour
    {
        private const float Tau = Mathf.PI * 2f;
        private const float R = 0.40f;
        private static Material _sharedMaterial;
        private readonly Painter _painter = new Painter(512);
        private EchoSphereRuntime _runtime;
        private SphereAttackAgent _agent;
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private string _branch;
        private int _element;
        private float _time, _phase;

        public void Initialize(EchoSphereRuntime runtime, SphereAttackAgent agent)
        {
            _runtime = runtime;
            _agent = agent;
            _phase = ((int)agent.Type + 1) * 0.6180339f;
            var layer = new GameObject("Sphere Elemental Trace");
            layer.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Sphere Elemental Trace Mesh" };
            _mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = layer.AddComponent<MeshRenderer>();
            _renderer.sharedMaterial = SharedMaterial;
            _renderer.sortingOrder = 11;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.enabled = false;
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
                        _sharedMaterial.name = "ECHO SPHERE Elemental Traces (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private void LateUpdate()
        {
            if (_runtime == null || _agent == null || _runtime.IsPresentationPaused || _mesh == null) return;
            _time += Time.deltaTime;
            var branch = _runtime.GetSphereBranch(_agent.Type);
            if (branch != _branch)
            {
                _branch = branch;
                _element = ResolveElement(branch, _runtime.GetSphereFinal(_agent.Type));
                if (_renderer != null) _renderer.enabled = _element != 0;
            }
            if (_element == 0) return;
            Rebuild();
        }

        private static int ResolveElement(string branch, string final)
        {
            if (string.IsNullOrEmpty(branch)) return 0;
            if (branch == "prism_spectrum")
            {
                var index = FinalIndex(final);
                return index == 1 ? 2 : index == 2 ? 3 : 1;
            }
            switch (branch)
            {
                case "standard_resonator":
                case "sniper_assassin":
                case "shotgun_burst":
                case "orbital_blade":
                case "prism_spectrum":
                case "chain_storm":
                case "aura_gravity":
                case "gravity_tide":
                case "pulse_resonator":
                case "void_reaper": return 1;
                case "standard_singularity":
                case "sniper_beacon":
                case "shotgun_hail":
                case "orbital_halo":
                case "prism_mirror":
                case "chain_web":
                case "aura_sanctum":
                case "gravity_well":
                case "pulse_wave":
                case "void_execution": return 2;
                case "standard_swarm":
                case "sniper_oracle":
                case "shotgun_cataclysm":
                case "orbital_dance":
                case "prism_split":
                case "chain_leech":
                case "aura_overgrowth":
                case "gravity_collapse":
                case "pulse_burst":
                case "void_hunger": return 3;
                default: return 0;
            }
        }

        private static int FinalIndex(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            var separator = id.LastIndexOf('_');
            int result;
            return separator >= 0 && int.TryParse(id.Substring(separator + 1), out result) ? result : -1;
        }

        private void Rebuild()
        {
            _painter.Clear();
            var color = ElementColor(_element);
            var alpha = 0.20f + 0.58f * (0.5f + 0.5f * Mathf.Sin(_time * 2.4f + _phase));
            var ringRx = R * 1.16f;
            var ringRy = R * 0.34f;
            var ringSpin = -_time * 0.18f;
            var particleSpin = -_time * 0.34f;

            // Soft bed, rear half, front half and moving highlight match the Canvas layer order.
            _painter.EllipseRing(Vector2.zero, ringRx, ringRy, ringSpin, 0f, Tau, Mathf.Max(0.012f, R * 0.12f), WithAlpha(color, alpha * 0.18f), 48);
            _painter.EllipseRing(Vector2.zero, ringRx, ringRy, ringSpin, Mathf.PI, Tau, Mathf.Max(0.008f, R * 0.04f), WithAlpha(color, alpha * 0.58f), 24);
            _painter.EllipseRing(Vector2.zero, ringRx, ringRy, ringSpin, 0f, Mathf.PI, Mathf.Max(0.008f, R * 0.04f), WithAlpha(color, alpha), 24);
            var highlightStart = Mathf.Repeat(particleSpin, Tau);
            _painter.EllipseRing(Vector2.zero, ringRx * 0.995f, ringRy * 0.995f, ringSpin, highlightStart, highlightStart + 0.72f, Mathf.Max(0.004f, R * 0.022f), WithAlpha(color, alpha * 0.72f), 8);

            for (var i = 0; i < 6; i++)
            {
                var angle = particleSpin + i * Tau / 6f;
                var depth = 0.38f + 0.62f * ((Mathf.Sin(angle) + 1f) * 0.5f);
                var bob = Mathf.Sin(_time * 2.8f + i * 1.73f) * R * 0.018f;
                var center = new Vector2(Mathf.Cos(angle) * ringRx, Mathf.Sin(angle) * ringRy + bob);
                var glyphSize = Mathf.Max(0.018f, R * (0.055f + (i % 3) * 0.008f));
                DrawElementGlyph(_painter, _element, center, glyphSize, angle, WithAlpha(color, alpha * (0.72f + depth * 0.28f)));
                if (i % 2 == 0)
                {
                    var moteAngle = angle + (i % 4 == 0 ? 0.16f : -0.16f);
                    var mote = new Vector2(Mathf.Cos(moteAngle) * ringRx * 1.035f, Mathf.Sin(moteAngle) * ringRy * 1.18f);
                    _painter.Circle(mote, Mathf.Max(0.004f, R * 0.018f), WithAlpha(color, alpha * (0.28f + depth * 0.24f)), 6);
                }
            }

            // A separate directional mark belongs to the Sphere weapon axis, not its orbit.
            var direction = _agent.VisualRotation;
            var weaponCenter = new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * (R * 1.05f);
            DrawElementGlyph(_painter, _element, weaponCenter, R * 0.105f, direction + Mathf.PI * 0.5f, WithAlpha(color, alpha * 0.72f));
            Upload();
        }

        private static Color ElementColor(int element)
        {
            switch (element)
            {
                case 1: return Hex("#ff743d");
                case 2: return Hex("#d9f6ff");
                case 3: return Hex("#72f08e");
                default: return Color.white;
            }
        }

        private static void DrawElementGlyph(Painter painter, int element, Vector2 center, float size, float angle, Color color)
        {
            if (element == 1)
            {
                var direction = new Vector2(Mathf.Cos(angle - Mathf.PI * 0.5f), Mathf.Sin(angle - Mathf.PI * 0.5f));
                var normal = new Vector2(-direction.y, direction.x);
                var tip = center + direction * size;
                var shoulder = center - direction * size * 0.12f;
                var left = shoulder - normal * size * 0.48f;
                var right = shoulder + normal * size * 0.48f;
                painter.Triangle(tip, left, right, color);
                painter.Line(center - direction * size * 0.40f, center + direction * size * 0.42f, Mathf.Max(0.002f, size * 0.10f), WithAlpha(Color.white, color.a * 0.7f));
                painter.Line(left, center + direction * size * 0.25f, Mathf.Max(0.0015f, size * 0.055f), WithAlpha(color, 0.72f));
                painter.Line(right, center + direction * size * 0.25f, Mathf.Max(0.0015f, size * 0.055f), WithAlpha(color, 0.72f));
            }
            else if (element == 2)
            {
                for (var i = 0; i < 6; i++)
                {
                    var a = angle + i * Mathf.PI / 3f;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    painter.Line(center + dir * size * 0.14f, center + dir * size, Mathf.Max(0.0015f, size * 0.08f), color);
                    var branchCenter = center + dir * size * 0.64f;
                    var branchAngle = a + (i % 2 == 0 ? 0.42f : -0.42f);
                    var branch = new Vector2(Mathf.Cos(branchAngle), Mathf.Sin(branchAngle));
                    painter.Line(branchCenter, branchCenter + branch * size * 0.30f, Mathf.Max(0.0012f, size * 0.055f), color);
                }
                painter.EllipseRing(center, size * 0.24f, size * 0.24f, 0f, 0f, Tau, Mathf.Max(0.0015f, size * 0.07f), color, 8);
            }
            else
            {
                painter.EllipseRing(center + new Vector2(-size * 0.30f, size * 0.06f), size * 0.34f, size * 0.34f, 0f, 0f, Tau, Mathf.Max(0.0015f, size * 0.08f), color, 8);
                painter.EllipseRing(center + new Vector2(size * 0.20f, -size * 0.12f), size * 0.42f, size * 0.42f, 0f, 0f, Tau, Mathf.Max(0.0015f, size * 0.08f), color, 8);
                painter.EllipseRing(center + new Vector2(size * 0.42f, size * 0.30f), size * 0.12f, size * 0.12f, 0f, 0f, Tau, Mathf.Max(0.001f, size * 0.06f), color, 6);
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }

        private static Color Hex(string value)
        {
            Color result;
            return ColorUtility.TryParseHtmlString(value, out result) ? result : Color.white;
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
            public readonly List<Vector3> Vertices = new List<Vector3>(512);
            public readonly List<Color> Colors = new List<Color>(512);
            public readonly List<int> Triangles = new List<int>(1536);

            public Painter(int ignoredCapacity) { }

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
                var previous = center + new Vector2(radius, 0f);
                for (var i = 1; i <= segments; i++)
                {
                    var angle = i * Tau / segments;
                    var next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                    Triangle(center, previous, next, color);
                    previous = next;
                }
            }

            public void EllipseRing(Vector2 center, float rx, float ry, float rotation, float startAngle, float endAngle, float width, Color color, int segments)
            {
                var step = (endAngle - startAngle) / Mathf.Max(1, segments);
                for (var i = 0; i < segments; i++)
                {
                    var a0 = startAngle + i * step;
                    var a1 = a0 + step;
                    var outer0 = Point(center, rx + width * 0.5f, ry + width * 0.5f, rotation, a0);
                    var outer1 = Point(center, rx + width * 0.5f, ry + width * 0.5f, rotation, a1);
                    var inner1 = Point(center, Mathf.Max(0.001f, rx - width * 0.5f), Mathf.Max(0.001f, ry - width * 0.5f), rotation, a1);
                    var inner0 = Point(center, Mathf.Max(0.001f, rx - width * 0.5f), Mathf.Max(0.001f, ry - width * 0.5f), rotation, a0);
                    Quad(outer0, outer1, inner1, inner0, color);
                }
            }

            private static Vector2 Point(Vector2 center, float rx, float ry, float rotation, float angle)
            {
                var x = Mathf.Cos(angle) * rx;
                var y = Mathf.Sin(angle) * ry;
                var sin = Mathf.Sin(rotation); var cos = Mathf.Cos(rotation);
                return center + new Vector2(x * cos - y * sin, x * sin + y * cos);
            }
        }
    }
}