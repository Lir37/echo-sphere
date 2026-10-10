using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>Cached procedural silhouettes for the migration prototype, not final production art.</summary>
    internal static class RuntimeSpriteFactory
    {
        private static Sprite _disc, _ring, _faceted, _diamond, _hexagon, _prism, _shard, _star;
        public static Sprite Disc { get { if (_disc == null) _disc = CreateSprite(false); return _disc; } }
        public static Sprite Ring { get { if (_ring == null) _ring = CreateSprite(true); return _ring; } }
        public static Sprite Faceted { get { if (_faceted == null) _faceted = CreatePolygonSprite("ES_Faceted", new[] { new Vector2(-0.68f,-0.72f),new Vector2(0.18f,-0.92f),new Vector2(0.83f,-0.35f),new Vector2(0.72f,0.54f),new Vector2(-0.12f,0.88f),new Vector2(-0.88f,0.22f) }); return _faceted; } }
        public static Sprite Diamond { get { if (_diamond == null) _diamond = CreatePolygonSprite("ES_Diamond", new[] { new Vector2(0f,0.98f),new Vector2(0.68f,0.12f),new Vector2(0.42f,-0.68f),new Vector2(0f,-0.92f),new Vector2(-0.42f,-0.68f),new Vector2(-0.68f,0.12f) }); return _diamond; } }
        public static Sprite Hexagon { get { if (_hexagon == null) _hexagon = CreatePolygonSprite("ES_Hexagon", RegularPolygon(6,0.88f,0.88f)); return _hexagon; } }
        public static Sprite Prism { get { if (_prism == null) _prism = CreatePolygonSprite("ES_Prism", new[] { new Vector2(0f,0.96f),new Vector2(0.86f,-0.62f),new Vector2(-0.86f,-0.62f) }); return _prism; } }
        public static Sprite Shard { get { if (_shard == null) _shard = CreatePolygonSprite("ES_Shard", new[] { new Vector2(-0.08f,0.98f),new Vector2(0.72f,0.34f),new Vector2(0.5f,-0.72f),new Vector2(-0.2f,-0.92f),new Vector2(-0.9f,-0.08f) }); return _shard; } }
        public static Sprite Star { get { if (_star == null) _star = CreatePolygonSprite("ES_Star", RegularPolygon(10,0.9f,0.52f)); return _star; } }

        private static Vector2[] RegularPolygon(int count,float outerRadius,float innerRadius)
        {
            var points=new Vector2[count];
            for(var i=0;i<count;i++){var angle=Mathf.PI*0.5f+i*Mathf.PI*2f/count;var radius=(count==6||i%2==0)?outerRadius:innerRadius;points[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
            return points;
        }
        private static Sprite CreateSprite(bool ring)
        {
            const int size=96;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=ring?"ES_Prototype_Ring":"ES_Prototype_Disc",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            var pixels=new Color32[size*size];
            for(var y=0;y<size;y++)for(var x=0;x<size;x++){var dx=((x+0.5f)/size)*2f-1f;var dy=((y+0.5f)/size)*2f-1f;var radius=Mathf.Sqrt(dx*dx+dy*dy);var alpha=ring?Mathf.Clamp01(1f-Mathf.Abs(radius-0.73f)/0.075f):Mathf.Clamp01((0.98f-radius)*38f);var shade=(byte)Mathf.RoundToInt(205f+Mathf.Clamp01(1f-radius)*50f);pixels[y*size+x]=new Color32(shade,shade,shade,(byte)Mathf.RoundToInt(alpha*255f));}
            texture.SetPixels32(pixels);texture.Apply(false,true);return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),size);
        }
        private static Sprite CreatePolygonSprite(string name,Vector2[] points)
        {
            const int size=96;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            var pixels=new Color32[size*size];
            for(var y=0;y<size;y++)for(var x=0;x<size;x++){
                var p=new Vector2(((x+0.5f)/size)*2f-1f,((y+0.5f)/size)*2f-1f);
                if(!InsidePolygon(p,points)){pixels[y*size+x]=new Color32(255,255,255,0);continue;}
                var sector=Mathf.FloorToInt((Mathf.Atan2(p.y,p.x)+Mathf.PI)/(Mathf.PI*2f)*points.Length);
                var shade=DistanceToPolygonEdge(p,points)<0.035f?255:Mathf.Clamp(Mathf.RoundToInt(192f+(1f-p.magnitude)*42f+((sector&1)==0?34f:-12f)),145,250);
                pixels[y*size+x]=new Color32((byte)shade,(byte)shade,(byte)shade,255);
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),size);
        }
        private static bool InsidePolygon(Vector2 p,Vector2[] poly)
        {
            var inside=false;
            for(var i=0,j=poly.Length-1;i<poly.Length;j=i++){var a=poly[i];var b=poly[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/((b.y-a.y)==0f?0.0001f:b.y-a.y)+a.x)inside=!inside;}
            return inside;
        }
        private static float DistanceToPolygonEdge(Vector2 p,Vector2[] poly)
        {
            var min=float.MaxValue;
            for(var i=0;i<poly.Length;i++){var a=poly[i];var b=poly[(i+1)%poly.Length];var segment=b-a;var len=segment.sqrMagnitude;var t=len<=0.000001f?0f:Mathf.Clamp01(Vector2.Dot(p-a,segment)/len);min=Mathf.Min(min,Vector2.Distance(p,a+segment*t));}
            return min;
        }
    }
}