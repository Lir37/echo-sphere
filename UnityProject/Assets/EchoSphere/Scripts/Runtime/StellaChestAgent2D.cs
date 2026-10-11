using System.Collections.Generic;
using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped Stella world pickup: renderer.ts:drawStellaChest and
    /// engineLoop.ts chest proximity collection. Rendering is presentation-only.
    /// </summary>
    public sealed class StellaChestAgent2D : MonoBehaviour
    {
        private static Material _material;
        private readonly List<Vector3> _vertices = new List<Vector3>(360);
        private readonly List<Color> _colors = new List<Color>(360);
        private readonly List<int> _triangles = new List<int>(540);
        private readonly List<Vector2> _shape = new List<Vector2>(28);
        private EchoSphereRuntime _runtime;
        private Mesh _mesh;
        private TextMesh _label;
        private float _time;
        private bool _collected;

        public void Initialize(EchoSphereRuntime runtime, Vector2 position)
        {
            _runtime=runtime;
            transform.position=position;
            _mesh=new Mesh { name="ECHO SPHERE Stella Cache Mesh" };
            _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh=_mesh;
            var renderer=gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=SharedMaterial;
            renderer.sortingOrder=12;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;

            var labelObject=new GameObject("STELLA Label");
            labelObject.transform.SetParent(transform,false);
            labelObject.transform.localPosition=new Vector3(0f,-0.49f,-0.01f);
            _label=labelObject.AddComponent<TextMesh>();
            _label.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
            _label.fontSize=32;
            _label.characterSize=0.014f;
            _label.anchor=TextAnchor.MiddleCenter;
            _label.alignment=TextAlignment.Center;
            _label.text="STELLA";
            _label.color=new Color(1f,0.82f,0.42f,0.96f);
            var labelRenderer=labelObject.GetComponent<MeshRenderer>();
            labelRenderer.sortingOrder=13;
            labelRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            labelRenderer.receiveShadows=false;
            Rebuild();
        }

        private static Material SharedMaterial
        {
            get
            {
                if(_material==null)
                {
                    var shader=Shader.Find("Sprites/Default");
                    if(shader!=null)
                    {
                        _material=new Material(shader);
                        _material.name="ECHO SPHERE Stella Cache / Unlit";
                        _material.hideFlags=HideFlags.HideAndDontSave;
                    }
                }
                return _material;
            }
        }

        private void Update()
        {
            if(_collected||_runtime==null||_runtime.IsGameplayPaused)return;
            _time+=Time.deltaTime;
            var pulse=1f+Mathf.Sin(_time*2.6f)*0.08f;
            transform.localScale=Vector3.one*pulse;
            Rebuild();
            // Source threshold is player radius (16 px) + Stella radius (28 px).
            if(Vector2.Distance(transform.position,_runtime.PlayerPosition) <
                BossCombatRules.ToWorldUnits(BossCombatRules.PlayerRadiusSourceUnits+28f))
            {
                _collected=true;
                _runtime.CollectStellaChest(this);
            }
        }

        private void Rebuild()
        {
            if(_mesh==null)return;
            _vertices.Clear();_colors.Clear();_triangles.Clear();
            AddEllipse(Vector2.zero,new Vector2(0.43f,0.12f),new Color(0.01f,0.025f,0.05f,0.40f),0.018f);
            var body=new Color(0.094f,0.047f,0.008f,0.98f);
            var gold=new Color(1f,0.72f,0.25f,0.98f);
            _shape.Clear();
            _shape.Add(new Vector2(0f,-0.40f));
            _shape.Add(new Vector2(0.31f,-0.20f));
            _shape.Add(new Vector2(0.34f,0.16f));
            _shape.Add(new Vector2(0f,0.39f));
            _shape.Add(new Vector2(-0.34f,0.16f));
            _shape.Add(new Vector2(-0.31f,-0.20f));
            AddPolygon(_shape,body,gold,0.023f);

            AddLine(new Vector2(-0.25f,-0.16f),new Vector2(0f,-0.02f),0.018f,new Color(1f,0.72f,0.25f,0.72f));
            AddLine(new Vector2(0f,-0.02f),new Vector2(0.25f,-0.16f),0.018f,new Color(1f,0.72f,0.25f,0.72f));
            AddLine(new Vector2(-0.23f,0.12f),new Vector2(0.23f,-0.12f),0.013f,new Color(1f,0.91f,0.62f,0.72f));
            AddLine(new Vector2(-0.16f,0.17f),new Vector2(0.16f,-0.17f),0.012f,new Color(1f,0.91f,0.62f,0.50f));
            var corePulse=0.07f*(1f+0.12f*Mathf.Sin(_time*8f));
            AddCircle(Vector2.zero,corePulse,new Color(1f,0.97f,0.83f,1f),Color.white,0.012f);
            AddLine(new Vector2(-0.32f,0.01f),new Vector2(-0.20f,0.01f),0.014f,new Color(1f,0.72f,0.25f,0.82f));
            AddLine(new Vector2(0.20f,0.01f),new Vector2(0.32f,0.01f),0.014f,new Color(1f,0.72f,0.25f,0.82f));
            _mesh.Clear();
            _mesh.SetVertices(_vertices);_mesh.SetColors(_colors);_mesh.SetTriangles(_triangles,0);
            _mesh.RecalculateBounds();
        }

        private void AddEllipse(Vector2 center,Vector2 radii,Color color,float width)
        {
            const int count=28;
            var previous=center+new Vector2(radii.x,0f);
            for(var i=1;i<=count;i++)
            {
                var a=i*Mathf.PI*2f/count;
                var next=center+new Vector2(Mathf.Cos(a)*radii.x,Mathf.Sin(a)*radii.y);
                AddLine(previous,next,width,color);
                previous=next;
            }
        }

        private void AddCircle(Vector2 center,float radius,Color fill,Color outline,float width)
        {
            _shape.Clear();
            const int count=20;
            for(var i=0;i<count;i++)
            {
                var a=i*Mathf.PI*2f/count;
                _shape.Add(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);
            }
            AddPolygon(_shape,fill,outline,width);
        }

        private void AddPolygon(List<Vector2> points,Color fill,Color outline,float width)
        {
            if(points==null||points.Count<3)return;
            var center=Vector2.zero;
            for(var i=0;i<points.Count;i++)center+=points[i];
            center/=points.Count;
            var ci=Vertex(center,fill);
            var first=_vertices.Count;
            for(var i=0;i<points.Count;i++)Vertex(points[i],fill);
            for(var i=0;i<points.Count;i++)Triangle(ci,first+i,first+(i+1)%points.Count);
            if(width>0f)
                for(var i=0;i<points.Count;i++)AddLine(points[i],points[(i+1)%points.Count],width,outline);
        }

        private void AddLine(Vector2 a,Vector2 b,float width,Color color)
        {
            var d=b-a;
            if(d.sqrMagnitude<0.0000001f||width<=0f)return;
            var n=new Vector2(-d.y,d.x).normalized*width*0.5f;
            var i=_vertices.Count;
            Vertex(a+n,color);Vertex(a-n,color);Vertex(b-n,color);Vertex(b+n,color);
            Triangle(i,i+1,i+2);Triangle(i,i+2,i+3);
        }

        private int Vertex(Vector2 p,Color color)
        {
            var index=_vertices.Count;
            _vertices.Add(new Vector3(p.x,p.y,0f));
            _colors.Add(color);
            return index;
        }
        private void Triangle(int a,int b,int c){_triangles.Add(a);_triangles.Add(b);_triangles.Add(c);}

        private void OnDestroy()
        {
            if(_mesh!=null){if(Application.isPlaying)Destroy(_mesh);else DestroyImmediate(_mesh);}
        }
    }
}
