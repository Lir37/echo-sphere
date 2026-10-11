using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped 2.5D runtime accents for src/spheres/*Visual.ts, including src/spheres/visualHelpers.ts.
    /// Authored SVGs remain the main Sphere bodies; this adds animated orbit geometry,
    /// family-specific emitters, anchors, plates, terminals and resonance feedback.
    /// </summary>
    public sealed class SphereRuntimeVfx2D : MonoBehaviour
    {
        private const float R = 0.40f;
        private const float Tau = Mathf.PI * 2f;
        private static readonly Color Resonance = new Color(1f, 0.82f, 0.40f);
        private static readonly List<Vector2> Path = new List<Vector2>(32);
        private static Material _material;
        private EchoSphereRuntime _runtime;
        private SphereAttackAgent _agent;
        private SphereId _type;
        private Mesh _orbitMesh, _detailMesh;
        private Painter _orbit, _detail;
        private Transform _glow;
        private SpriteRenderer _glowSprite;
        private Color _baseColor;
        private float _time, _phase;

        public void Initialize(EchoSphereRuntime runtime, SphereAttackAgent agent)
        {
            _runtime = runtime; _agent = agent; _type = agent.Type;
            _baseColor = ColorFor(_type);
            _phase = ((int)_type + 1) * 0.6180339f;
            _glow = transform.Find("Glow");
            if (_glow != null)
            {
                _glowSprite = _glow.GetComponent<SpriteRenderer>();
                _glow.localScale = Vector3.one * 2.35f;
            }
            _orbit = new Painter(); _detail = new Painter();
            CreateLayer("Sphere Runtime Orbit", 7, out _orbitMesh);
            CreateLayer("Sphere Runtime Details", 10, out _detailMesh);
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
                        _material.name = "ECHO SPHERE Family VFX (Unlit)";
                        _material.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _material;
            }
        }

        private void CreateLayer(string name, int order, out Mesh mesh)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(transform, false);
            mesh = new Mesh { name = name + " Mesh" };
            mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (_runtime == null || _agent == null || _runtime.IsPresentationPaused) return;
            _time += Time.deltaTime;
            Rebuild();
        }

        private void Rebuild()
        {
            if (_orbit == null || _detail == null || _orbitMesh == null || _detailMesh == null) return;
            var resonance = _runtime != null && _runtime.ResonanceEventActive;
            var color = resonance ? Resonance : _baseColor;
            var pulse = 0.5f + 0.5f * Mathf.Sin(_time * 3.4f + _phase);
            if (_glow != null) _glow.localScale = Vector3.one * (2.35f * (1f + pulse * .035f));
            if (_glowSprite != null) _glowSprite.color = new Color(color.r, color.g, color.b, .10f + pulse * .045f);

            _orbit.Clear();
            var spin = _time * OrbitSpeed(_type) + _phase;
            if (_type == SphereId.Orbital)
            {
                _orbit.Ellipse(Vector2.zero, R * 1.20f, R * .54f, spin, color, .012f, .10f, .97f);
                _orbit.Ellipse(Vector2.zero, R * 1.08f, R * .46f, -spin * 1.18f, color, .009f, .16f, .88f);
            }
            else
            {
                var flat = _type == SphereId.Sniper ? .28f : _type == SphereId.Gravity ? .38f : .34f;
                _orbit.Ellipse(Vector2.zero, R * 1.03f, R * flat, spin, color, .010f, .09f, _type == SphereId.Chain ? .78f : .66f);
                if (_type == SphereId.Pulse)
                {
                    var q = Mathf.Repeat(_time * .42f, 1f);
                    _orbit.Ellipse(Vector2.zero, R * (1.02f + q * .86f), R * (.34f + q * .28f), 0f, color, .008f, 0f, (1f - q) * .36f);
                }
                else if (_type == SphereId.Gravity || _type == SphereId.Aura || _type == SphereId.Chain)
                {
                    var q = .5f + .5f * Mathf.Sin(_time * (_type == SphereId.Chain ? 10f : _type == SphereId.Aura ? 2.7f : 3.1f));
                    _orbit.Ellipse(Vector2.zero, R * (1.04f + q * .04f), R * (flat + q * .025f), 0f, color, .006f, 0f, .16f + q * .08f);
                }
            }
            Upload(_orbitMesh, _orbit);

            _detail.Clear();
            var facing = _agent.VisualRotation;
            switch (_type)
            {
                case SphereId.Sniper:
                    DrawEmitter(_detail, R, color, facing, 0, pulse);
                    var scan = _time * .46f + _phase;
                    DrawReticle(_detail, EllipsePoint(R * .34f, R * .12f, scan), R * .10f, color, .62f);
                    break;
                case SphereId.Chain:
                    for (var i = 0; i < 4; i++) DrawConductor(_detail, R, i * Mathf.PI * .5f + _time * .28f, color, .56f);
                    break;
                case SphereId.Shotgun:
                    for (var i = -1; i <= 1; i++) DrawEmitter(_detail, R, color, facing, 1, i * R * .25f);
                    for (var i = 0; i < 3; i++)
                    {
                        var a = _time * (.34f + i * .035f) + i * Tau / 3f;
                        DrawCross(_detail, EllipsePoint(R * .34f, R * .12f, a), R * .10f, color, .48f * Depth(a));
                    }
                    if (Mathf.Sin(_time * 8f) > .6f) _detail.Arc(new Vector2(R * 1.43f, 0f), R * .11f, R * .11f, 0f, Tau, color, .008f, .22f);
                    break;
                case SphereId.Aura:
                    for (var i = 0; i < 6; i++)
                    {
                        var a = i * Mathf.PI / 3f + _time * .10f;
                        DrawDiamond(_detail, EllipsePoint(R * 1.08f, R * .36f, a), R * .14f, color, .48f * Depth(a), a + Mathf.PI / 2f);
                    }
                    break;
                case SphereId.Orbital:
                    DrawTerminal(_detail, new Vector2(0f, -R * 1.02f), R, color, .76f, _time);
                    DrawTerminal(_detail, new Vector2(0f, R * 1.02f), R, color, .62f, -_time);
                    if (resonance) _detail.Arc(Vector2.zero, R * 1.34f, R * 1.34f, _time * .65f, Mathf.PI * 1.15f, Resonance, .010f, .42f);
                    break;
                case SphereId.Prism:
                    DrawEmitter(_detail, R, color, facing, 2, 0f);
                    for (var i = 0; i < 2; i++)
                    {
                        var a = _time * (.30f + i * .05f) + i * Mathf.PI;
                        DrawDiamond(_detail, EllipsePoint(R * .34f, R * .12f, a), R * .09f, color, .42f * Depth(a), a + Mathf.PI / 2f);
                    }
                    break;
                case SphereId.Gravity:
                    for (var i = 0; i < 5; i++)
                    {
                        var a = i * Tau / 5f + _time * .08f;
                        DrawDiamond(_detail, EllipsePoint(R * 1.08f, R * .36f, a), R * .12f, color, .44f * Depth(a), a);
                    }
                    break;
                case SphereId.Pulse:
                    for (var i = 0; i < 4; i++)
                    {
                        var a = _time * .28f + i * Mathf.PI / 2f;
                        DrawPlate(_detail, EllipsePoint(R * .34f, R * .11f, a), R * .17f, color,
                            .36f + .20f * (.5f + .5f * Mathf.Sin(_time * 2.8f + i)), a + Mathf.PI / 2f);
                    }
                    break;
                case SphereId.Void:
                    DrawEmitter(_detail, R, color, facing, 3, 0f);
                    for (var i = 0; i < 4; i++)
                    {
                        var a = -_time * (.22f + i * .025f) + i * Mathf.PI / 2f;
                        var rr = R * (.07f + i * .012f);
                        _detail.Arc(EllipsePoint(R * .34f, R * .12f, a), rr, rr, Mathf.PI * .2f, Mathf.PI * 1.6f, color, .008f, .32f * Depth(a));
                    }
                    break;
            }
            if (_agent.NetworkDisabled)
            {
                var disabled = new Color(1f, 0.22f, 0.39f, 0.94f);
                _detail.Line(new Vector2(-R * 1.18f, -R * 0.54f), new Vector2(-R * 0.52f, -R * 0.02f), disabled, 0.025f);
                _detail.Line(new Vector2(-R * 1.18f, R * 0.54f), new Vector2(-R * 0.52f, R * 0.02f), disabled, 0.025f);
                _detail.Line(new Vector2(R * 1.18f, -R * 0.54f), new Vector2(R * 0.52f, -R * 0.02f), disabled, 0.025f);
                _detail.Line(new Vector2(R * 1.18f, R * 0.54f), new Vector2(R * 0.52f, R * 0.02f), disabled, 0.025f);
                _detail.Arc(Vector2.zero, R * 1.25f, R * 0.82f, _time * 2.1f, Mathf.PI * 1.4f, disabled, 0.016f, 0.62f);
            }
            Upload(_detailMesh, _detail);
        }

        private void Upload(Mesh mesh, Painter painter)
        {
            mesh.Clear(); mesh.SetVertices(painter.Vertices); mesh.SetColors(painter.Colors);
            mesh.SetTriangles(painter.Triangles, 0); mesh.RecalculateBounds();
        }

        private static float OrbitSpeed(SphereId t)
        {
            switch (t)
            {
                case SphereId.Sniper: return .20f; case SphereId.Chain: return .17f;
                case SphereId.Shotgun: return .18f; case SphereId.Aura: return .14f;
                case SphereId.Orbital: return .18f; case SphereId.Prism: return .16f;
                case SphereId.Gravity: return -.12f; case SphereId.Pulse: return .143f;
                case SphereId.Void: return .11f; default: return .15f;
            }
        }

        private static Color ColorFor(SphereId t)
        {
            switch (t)
            {
                case SphereId.Sniper: return Hex("#e86cff"); case SphereId.Chain: return Hex("#ffe25b");
                case SphereId.Shotgun: return Hex("#ff8f3d"); case SphereId.Aura: return Hex("#57e6b4");
                case SphereId.Orbital: return Hex("#8ef0ff"); case SphereId.Prism: return Hex("#ff8de1");
                case SphereId.Gravity: return Hex("#a58cff"); case SphereId.Pulse: return Hex("#ffd35a");
                case SphereId.Void: return Hex("#c28cff"); default: return Color.white;
            }
        }
        private static Color Hex(string v) { Color c; return ColorUtility.TryParseHtmlString(v, out c) ? c : Color.white; }
        private static Vector2 EllipsePoint(float x, float y, float a) { return new Vector2(Mathf.Cos(a) * x, Mathf.Sin(a) * y); }
        private static float Depth(float a) { return .74f + .26f * ((Mathf.Sin(a) + 1f) * .5f); }
        private static Vector2 Rotate(Vector2 v, float a) { var s = Mathf.Sin(a); var c = Mathf.Cos(a); return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c); }

        // kind: 0 sniper needle, 1 shotgun muzzle, 2 prism emitter, 3 void emitter.
        private static void DrawEmitter(Painter p, float r, Color c, float facing, int kind, float y)
        {
            var center = new Vector2(r * (kind == 0 ? 1.22f : kind == 3 ? 1.20f : 1.18f), y);
            Path.Clear();
            if (kind == 0)
            {
                Path.Add(new Vector2(0f, -r * .24f)); Path.Add(new Vector2(r * .18f, -r * .05f));
                Path.Add(new Vector2(r * .13f, r * .24f)); Path.Add(new Vector2(0f, r * .34f));
                Path.Add(new Vector2(-r * .13f, r * .24f)); Path.Add(new Vector2(-r * .18f, -r * .05f));
            }
            else if (kind == 1)
            {
                Path.Add(new Vector2(-r * .02f, -r * .10f)); Path.Add(new Vector2(r * .22f, -r * .11f));
                Path.Add(new Vector2(r * .30f, 0f)); Path.Add(new Vector2(r * .22f, r * .11f)); Path.Add(new Vector2(-r * .02f, r * .10f));
            }
            else if (kind == 2)
            {
                Path.Add(new Vector2(0f, -r * .20f)); Path.Add(new Vector2(r * .17f, -r * .06f));
                Path.Add(new Vector2(r * .27f, 0f)); Path.Add(new Vector2(r * .17f, r * .06f));
                Path.Add(new Vector2(0f, r * .20f)); Path.Add(new Vector2(r * .08f, 0f));
            }
            else
            {
                Path.Add(new Vector2(-r * .01f, -r * .19f)); Path.Add(new Vector2(r * .21f, -r * .08f));
                Path.Add(new Vector2(r * .27f, 0f)); Path.Add(new Vector2(r * .21f, r * .08f));
                Path.Add(new Vector2(-r * .01f, r * .19f)); Path.Add(new Vector2(r * .07f, 0f));
            }
            var rot = (kind == 0 ? -Mathf.PI / 2f : 0f) + facing;
            for (var i = 0; i < Path.Count; i++) Path[i] = center + Rotate(Path[i], rot);
            var fill = new Color(.027f, .067f, .105f, kind == 3 ? .97f : .94f);
            p.Polygon(Path, fill, c, .009f);
            if (kind == 0)
            {
                p.Line(center + Rotate(new Vector2(0f, -r * .18f), rot), center + Rotate(new Vector2(0f, r * .16f), rot), Color.white, .006f, .70f);
                p.Circle(center, r * .12f, c, .10f, 0f);
            }
            else if (kind == 1) p.Circle(center + Rotate(new Vector2(r * .25f, 0f), rot), r * .055f, Color.white, .70f, .005f);
            else if (kind == 2)
            {
                p.Line(center + Rotate(new Vector2(r * .09f, -r * .10f), rot), center + Rotate(new Vector2(r * .21f, 0f), rot), Color.white, .006f, .68f);
                p.Line(center + Rotate(new Vector2(r * .21f, 0f), rot), center + Rotate(new Vector2(r * .09f, r * .10f), rot), Color.white, .006f, .68f);
            }
            else p.Arc(center + Rotate(new Vector2(r * .18f, 0f), rot), r * .07f, r * .07f, 0f, Tau, new Color(.92f, .875f, 1f), .006f, .65f);
        }

        private static void DrawTerminal(Painter p, Vector2 pos, float r, Color c, float alpha, float phase)
        {
            Path.Clear();
            Path.Add(new Vector2(0f, -r * .28f)); Path.Add(new Vector2(r * .16f, -r * .05f));
            Path.Add(new Vector2(r * .11f, r * .24f)); Path.Add(new Vector2(0f, r * .31f));
            Path.Add(new Vector2(-r * .11f, r * .24f)); Path.Add(new Vector2(-r * .16f, -r * .05f));
            for (var i = 0; i < Path.Count; i++) Path[i] = pos + Rotate(Path[i], phase * .08f);
            p.Polygon(Path, new Color(.027f, .067f, .105f, alpha), new Color(c.r, c.g, c.b, alpha), .009f);
            Path.Clear();
            Path.Add(pos + Rotate(new Vector2(0f, -r * .16f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(r * .07f, 0f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(0f, r * .14f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(-r * .07f, 0f), phase * .08f));
            p.Polygon(Path, new Color(1f, 1f, 1f, alpha * .72f), Color.white, 0f);
        }

        private static void DrawConductor(Painter p, float r, float angle, Color c, float alpha)
        {
            var pos = EllipsePoint(r * .34f, r * .12f, angle); var rot = angle + Mathf.PI / 2f;
            var a = pos + Rotate(new Vector2(-r * .08f, 0f), rot);
            var b = pos + Rotate(new Vector2(-r * .01f, -r * .07f), rot);
            var d = pos + Rotate(new Vector2(r * .07f, r * .03f), rot);
            var e = pos + Rotate(new Vector2(r * .14f, -r * .02f), rot);
            var f = pos + Rotate(new Vector2(r * .21f, r * .06f), rot);
            var al = alpha * Depth(angle);
            p.Line(a,b,c,.009f,al); p.Line(b,d,c,.009f,al); p.Line(d,e,c,.009f,al); p.Line(e,f,c,.009f,al);
        }
        private static void DrawDiamond(Painter p, Vector2 at, float r, Color c, float alpha, float angle)
        {
            Path.Clear(); Path.Add(at + Rotate(new Vector2(0f,-r),angle)); Path.Add(at + Rotate(new Vector2(r*.72f,0f),angle));
            Path.Add(at + Rotate(new Vector2(0f,r),angle)); Path.Add(at + Rotate(new Vector2(-r*.72f,0f),angle));
            p.Polygon(Path,new Color(.027f,.067f,.105f,alpha*.82f),new Color(c.r,c.g,c.b,alpha),.008f);
        }
        private static void DrawPlate(Painter p, Vector2 at, float r, Color c, float alpha, float angle)
        {
            Path.Clear(); Path.Add(at+Rotate(new Vector2(-r*.48f,-r*.60f),angle)); Path.Add(at+Rotate(new Vector2(r*.48f,-r*.35f),angle));
            Path.Add(at+Rotate(new Vector2(r*.48f,r*.35f),angle)); Path.Add(at+Rotate(new Vector2(-r*.48f,r*.60f),angle));
            p.Polygon(Path,new Color(.027f,.067f,.105f,alpha*.85f),new Color(c.r,c.g,c.b,alpha),.007f);
        }
        private static void DrawReticle(Painter p, Vector2 at, float r, Color c, float alpha)
        {
            p.Arc(at,r,r,0f,Tau,c,.006f,alpha);
            p.Line(at+Vector2.left*r*1.8f,at+Vector2.right*r*1.8f,c,.005f,alpha);
            p.Line(at+Vector2.down*r*1.8f,at+Vector2.up*r*1.8f,c,.005f,alpha);
        }
        private static void DrawCross(Painter p, Vector2 at, float r, Color c, float alpha)
        {
            p.Line(at+Vector2.left*r,at+Vector2.right*r,c,.006f,alpha);
        }

        private sealed class Painter
        {
            public readonly List<Vector3> Vertices = new List<Vector3>(600);
            public readonly List<Color> Colors = new List<Color>(600);
            public readonly List<int> Triangles = new List<int>(1200);
            public void Clear() { Vertices.Clear(); Colors.Clear(); Triangles.Clear(); }
            private int V(Vector2 p,Color c){var i=Vertices.Count;Vertices.Add(new Vector3(p.x,p.y,0f));Colors.Add(c);return i;}
            private void T(int a,int b,int c){Triangles.Add(a);Triangles.Add(b);Triangles.Add(c);}
            public void Polygon(List<Vector2> pts,Color fill,Color outline,float width)
            {
                if(pts==null||pts.Count<3)return;
                var center=Vector2.zero;for(var i=0;i<pts.Count;i++)center+=pts[i];center/=pts.Count;
                var ci=V(center,fill);var start=Vertices.Count;for(var i=0;i<pts.Count;i++)V(pts[i],fill);
                for(var i=0;i<pts.Count;i++)T(ci,start+i,start+(i+1)%pts.Count);
                if(width>0f)for(var i=0;i<pts.Count;i++)Line(pts[i],pts[(i+1)%pts.Count],outline,width);
            }
            public void Line(Vector2 a,Vector2 b,Color c,float width,float alpha=1f)
            {
                var d=b-a;if(d.sqrMagnitude<.0000001f||width<=0f)return;
                var n=new Vector2(-d.y,d.x).normalized*width*.5f;var tint=new Color(c.r,c.g,c.b,Mathf.Clamp01(c.a*alpha));
                var i=Vertices.Count;V(a+n,tint);V(a-n,tint);V(b-n,tint);V(b+n,tint);T(i,i+1,i+2);T(i,i+2,i+3);
            }
            public void Circle(Vector2 at,float radius,Color c,float alpha,float outline)
            {
                Path.Clear();for(var i=0;i<20;i++){var a=i*Tau/20f;Path.Add(at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
                Polygon(Path,new Color(c.r,c.g,c.b,alpha),c,outline);
            }
            public void Ellipse(Vector2 center,float rx,float ry,float angle,Color c,float width,float gap,float alpha)
            {
                const int count=64;
                for(var i=0;i<count;i++)
                {
                    var q=i/(float)count;if(gap>0f&&q>.46f-gap*.5f&&q<.46f+gap*.5f)continue;
                    var a0=i*Tau/count;var a1=(i+1)*Tau/count;
                    var a=center+Rotate(new Vector2(Mathf.Cos(a0)*rx,Mathf.Sin(a0)*ry),angle);
                    var b=center+Rotate(new Vector2(Mathf.Cos(a1)*rx,Mathf.Sin(a1)*ry),angle);
                    Line(a,b,c,width,alpha);
                }
            }
            public void Arc(Vector2 center,float rx,float ry,float start,float sweep,Color c,float width,float alpha)
            {
                const int max=36;var count=Mathf.Max(2,Mathf.CeilToInt(max*Mathf.Abs(sweep)/Tau));
                var from=center+new Vector2(Mathf.Cos(start)*rx,Mathf.Sin(start)*ry);
                for(var i=1;i<=count;i++){var a=start+sweep*i/count;var to=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Line(from,to,c,width,alpha);from=to;}
            }
        }
    }
}
