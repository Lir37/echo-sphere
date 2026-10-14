using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped transient muzzle/charge visual from src/renderer.ts:
    /// drawStandardAttackVfx. This is presentation-only and never touches combat state.
    /// </summary>
    public sealed class StandardSphereAttackVfx2D : MonoBehaviour
    {
        private const float Tau = Mathf.PI * 2f;
        private static readonly Color Cyan = new Color(99f / 255f, 230f / 255f, 1f, 1f);
        private static readonly Color White = Color.white;
        private static readonly List<Vector3> Vertices = new List<Vector3>(512);
        private static readonly List<Color> Colors = new List<Color>(512);
        private static readonly List<int> Triangles = new List<int>(1024);
        private static Material _material;

        private SphereAttackAgent _agent;
        private Mesh _mesh;
        private MeshRenderer _renderer;

        public void Initialize(SphereAttackAgent agent)
        {
            _agent = agent;
            var layer = new GameObject("Standard Sphere Attack Charge");
            layer.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Standard Sphere Attack Charge Mesh" };
            _mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = layer.AddComponent<MeshRenderer>();
            _renderer.sharedMaterial = SharedMaterial;
            _renderer.sortingOrder = 13;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            Rebuild();
        }

        private static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        _material = new Material(shader);
                        _material.name = "ECHO SPHERE Standard Attack Charge (Unlit)";
                        _material.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _material;
            }
        }

        private void LateUpdate()
        {
            if (_agent == null || _mesh == null) return;
            Rebuild();
        }

        private void Rebuild()
        {
            Vertices.Clear();
            Colors.Clear();
            Triangles.Clear();
            if (_agent == null || _mesh == null)
            {
                if (_mesh != null) _mesh.Clear();
                return;
            }

            // drawStandardAttackVfx uses the source's fixed 1.2 s charge window.
            var charge = Mathf.Clamp01(1f - _agent.AttackTimer / 1.2f);
            if (charge < 0.04f)
            {
                _mesh.Clear();
                if (_renderer != null) _renderer.enabled = false;
                return;
            }
            if (_renderer != null) _renderer.enabled = true;

            var rootScale = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
            var px = SpherePlacementRules.SourcePixelsToWorld / rootScale;
            var sourceRadius = Mathf.Clamp(130f * 0.19f + Mathf.Max(0, _agent.ProgressionLevel) * 0.8f, 15f, 25f);
            var r = sourceRadius * px;
            var reach = r * (1f + 0.02f + charge * 0.84f);
            var angle = _agent.VisualRotation;
            var alpha1 = 0.20f + charge * 0.24f;
            var width1 = (1.35f + charge * 0.45f) * px;
            var alpha2 = 0.34f + charge * 0.32f;
            var alpha3 = 0.24f + charge * 0.28f;

            Quadratic(new Vector2(reach * .68f, -r * .18f),
                new Vector2(reach * .90f, -r * .32f), new Vector2(reach, -r * .08f), angle, Cyan, width1, alpha1);
            Quadratic(new Vector2(reach, -r * .08f),
                new Vector2(reach * 1.08f, 0f), new Vector2(reach, r * .08f), angle, Cyan, width1, alpha1);
            Quadratic(new Vector2(reach, r * .08f),
                new Vector2(reach * .90f, r * .32f), new Vector2(reach * .68f, r * .18f), angle, Cyan, width1, alpha1);

            var width2 = (1.35f + charge * 0.45f) * px;
            Quadratic(new Vector2(reach * .74f, -r * .13f),
                new Vector2(reach * .94f, 0f), new Vector2(reach * 1.02f, -r * .03f), angle, Cyan, width2, alpha2);
            Quadratic(new Vector2(reach * 1.02f, -r * .03f),
                new Vector2(reach * 1.10f, 0f), new Vector2(reach * 1.02f, r * .03f), angle, Cyan, width2, alpha2);
            Quadratic(new Vector2(reach * 1.02f, r * .03f),
                new Vector2(reach * .94f, 0f), new Vector2(reach * .74f, r * .13f), angle, Cyan, width2, alpha2);

            Arc(new Vector2(reach, 0f), r * (.15f + charge * .08f),
                -Mathf.PI * .72f, Mathf.PI * 1.44f, angle, Cyan, (1.35f + charge * .45f) * px, alpha3);
            FilledCircle(Rotate(new Vector2(reach * .98f, 0f), angle),
                r * (.065f + charge * .035f), White, 0.72f + charge * 0.18f);

            _mesh.Clear();
            _mesh.SetVertices(Vertices);
            _mesh.SetColors(Colors);
            _mesh.SetTriangles(Triangles, 0);
            _mesh.RecalculateBounds();
        }

        private static Vector2 Rotate(Vector2 point, float angle)
        {
            var c = Mathf.Cos(angle);
            var s = Mathf.Sin(angle);
            return new Vector2(point.x * c - point.y * s, point.x * s + point.y * c);
        }

        private static void Quadratic(Vector2 a, Vector2 control, Vector2 b, float angle, Color color, float width, float alpha)
        {
            var previous = a;
            const int segments = 8;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var p = (1f - t) * (1f - t) * a + 2f * (1f - t) * t * control + t * t * b;
                Line(Rotate(previous, angle), Rotate(p, angle), color, width, alpha);
                previous = p;
            }
        }

        private static void Arc(Vector2 center, float radius, float start, float sweep, float angle, Color color, float width, float alpha)
        {
            const int segments = 24;
            var previous = center + new Vector2(Mathf.Cos(start), Mathf.Sin(start)) * radius;
            for (var i = 1; i <= segments; i++)
            {
                var a = start + sweep * i / segments;
                var next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Line(Rotate(previous, angle), Rotate(next, angle), color, width, alpha);
                previous = next;
            }
        }

        private static void Line(Vector2 a, Vector2 b, Color color, float width, float alpha)
        {
            var delta = b - a;
            if (delta.sqrMagnitude < 0.0000001f || width <= 0f) return;
            var normal = new Vector2(-delta.y, delta.x).normalized * width * 0.5f;
            var tint = new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a * alpha));
            var start = Vertices.Count;
            Vertices.Add(new Vector3(a.x + normal.x, a.y + normal.y, 0f));
            Vertices.Add(new Vector3(a.x - normal.x, a.y - normal.y, 0f));
            Vertices.Add(new Vector3(b.x - normal.x, b.y - normal.y, 0f));
            Vertices.Add(new Vector3(b.x + normal.x, b.y + normal.y, 0f));
            for (var i = 0; i < 4; i++) Colors.Add(tint);
            Triangles.Add(start); Triangles.Add(start + 1); Triangles.Add(start + 2);
            Triangles.Add(start); Triangles.Add(start + 2); Triangles.Add(start + 3);
        }

        private static void FilledCircle(Vector2 center, float radius, Color color, float alpha)
        {
            const int segments = 20;
            var tint = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
            var start = Vertices.Count;
            Vertices.Add(new Vector3(center.x, center.y, 0f));
            Colors.Add(tint);
            for (var i = 0; i <= segments; i++)
            {
                var a = i * Tau / segments;
                Vertices.Add(new Vector3(center.x + Mathf.Cos(a) * radius, center.y + Mathf.Sin(a) * radius, 0f));
                Colors.Add(tint);
            }
            for (var i = 0; i < segments; i++)
            {
                Triangles.Add(start);
                Triangles.Add(start + i + 1);
                Triangles.Add(start + i + 2);
            }
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
