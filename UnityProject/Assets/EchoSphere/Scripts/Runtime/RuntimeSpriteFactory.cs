using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>Cached procedural silhouettes for the migration prototype, not final production art.</summary>
    internal static class RuntimeSpriteFactory
    {
        private static Sprite _disc, _ring, _standardCoreGlow, _standardCoreOutline, _standardOrbitRing, _faceted, _diamond, _hexagon, _prism, _shard, _star, _experienceShard, _experienceShardOutline, _networkCometHead, _networkCometTail;
        private static readonly Dictionary<string, Sprite> AuthoredCache = new Dictionary<string, Sprite>();
        public static Sprite Disc { get { if (_disc == null) _disc = CreateSprite(false); return _disc; } }
        public static Sprite Ring { get { if (_ring == null) _ring = CreateSprite(true); return _ring; } }
        public static Sprite StandardCoreGlow { get { if (_standardCoreGlow == null) _standardCoreGlow = CreateStandardCoreGlow(); return _standardCoreGlow; } }
        public static Sprite StandardCoreOutline { get { if (_standardCoreOutline == null) _standardCoreOutline = CreateThinRing("ES_StandardCoreOutline", 0.73f, 0.0304f); return _standardCoreOutline; } }
        public static Sprite StandardOrbitRing { get { if (_standardOrbitRing == null) _standardOrbitRing = CreateStandardOrbitRing(); return _standardOrbitRing; } }
        public static Sprite Faceted { get { if (_faceted == null) _faceted = CreatePolygonSprite("ES_Faceted", new[] { new Vector2(-0.68f,-0.72f),new Vector2(0.18f,-0.92f),new Vector2(0.83f,-0.35f),new Vector2(0.72f,0.54f),new Vector2(-0.12f,0.88f),new Vector2(-0.88f,0.22f) }); return _faceted; } }
        public static Sprite Diamond { get { if (_diamond == null) _diamond = CreatePolygonSprite("ES_Diamond", new[] { new Vector2(0f,0.98f),new Vector2(0.68f,0.12f),new Vector2(0.42f,-0.68f),new Vector2(0f,-0.92f),new Vector2(-0.42f,-0.68f),new Vector2(-0.68f,0.12f) }); return _diamond; } }
        public static Sprite Hexagon { get { if (_hexagon == null) _hexagon = CreatePolygonSprite("ES_Hexagon", RegularPolygon(6,0.88f,0.88f)); return _hexagon; } }
        public static Sprite Prism { get { if (_prism == null) _prism = CreatePolygonSprite("ES_Prism", new[] { new Vector2(0f,0.96f),new Vector2(0.86f,-0.62f),new Vector2(-0.86f,-0.62f) }); return _prism; } }
        public static Sprite Shard { get { if (_shard == null) _shard = CreatePolygonSprite("ES_Shard", new[] { new Vector2(-0.08f,0.98f),new Vector2(0.72f,0.34f),new Vector2(0.5f,-0.72f),new Vector2(-0.2f,-0.92f),new Vector2(-0.9f,-0.08f) }); return _shard; } }
        public static Sprite Star { get { if (_star == null) _star = CreatePolygonSprite("ES_Star", RegularPolygon(10,0.9f,0.52f)); return _star; } }
        public static Sprite ExperienceShard { get { if (_experienceShard == null) _experienceShard = CreateFlatPolygonSprite("ES_ExperienceShard", ExperienceShardPoints()); return _experienceShard; } }
        public static Sprite ExperienceShardOutline { get { if (_experienceShardOutline == null) _experienceShardOutline = CreatePolygonOutlineSprite("ES_ExperienceShardOutline", ExperienceShardPoints(), 0.045f); return _experienceShardOutline; } }
        public static Sprite NetworkCometHead { get { if (_networkCometHead == null) _networkCometHead = CreateFlatPolygonSprite("ES_NetworkCometHead", new[] { new Vector2(1f, 0f), new Vector2(-0.496f, -1f), new Vector2(-1f, 0f), new Vector2(-0.496f, 1f) }); return _networkCometHead; } }
        public static Sprite NetworkCometTail { get { if (_networkCometTail == null) _networkCometTail = CreateFlatPolygonSprite("ES_NetworkCometTail", new[] { new Vector2(1f, 0f), new Vector2(-0.333f, -1f), new Vector2(-1f, 0f), new Vector2(-0.333f, 1f) }); return _networkCometTail; } }

        /// <summary>Loads an authored SVG sprite from Resources. A missing import allows the procedural fallback to work.</summary>
        public static Sprite LoadAuthored(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) return null;
            if (AuthoredCache.TryGetValue(assetName, out var cached)) return cached;
            var loaded = Resources.Load<Sprite>("EchoSphere/Art/" + assetName);
            AuthoredCache[assetName] = loaded;
            return loaded;
        }

        private static Vector2[] RegularPolygon(int count,float outerRadius,float innerRadius)
        {
            var points=new Vector2[count];
            for(var i=0;i<count;i++){var angle=Mathf.PI*0.5f+i*Mathf.PI*2f/count;var radius=(count==6||i%2==0)?outerRadius:innerRadius;points[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
            return points;
        }
        private static Sprite CreateThinRing(string name, float radius, float halfWidth)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nx = ((x + 0.5f) / size) * 2f - 1f;
                var ny = ((y + 0.5f) / size) * 2f - 1f;
                var distance = Mathf.Abs(Mathf.Sqrt(nx * nx + ny * ny) - radius);
                var alpha = Mathf.Clamp01((halfWidth - distance) * size * 0.5f + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // Reproduces both thin elliptical strokes from drawStandardSphereRing().
        // A second partial arc has the source's 0.55 opacity and 18-degree gap.
        private static Sprite CreateStandardOrbitRing()
        {
            const int size = 128;
            const float outerRadius = 0.73f;
            const float outerHalfWidth = 0.0142f;
            const float innerRadius = outerRadius * 0.88f / 0.98f;
            const float innerHalfWidth = 0.0069f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "ES_StandardOrbitRing",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nx = ((x + 0.5f) / size) * 2f - 1f;
                var ny = ((y + 0.5f) / size) * 2f - 1f;
                var radius = Mathf.Sqrt(nx * nx + ny * ny);
                var alpha = 0f;
                if (Mathf.Abs(radius - outerRadius) <= outerHalfWidth) alpha = 1f;

                var angle = Mathf.Atan2(ny, nx);
                if (angle < 0f) angle += Mathf.PI * 2f;
                var onSourceArc = angle >= Mathf.PI * 0.10f && angle <= Mathf.PI * 1.92f;
                if (onSourceArc && Mathf.Abs(radius - innerRadius) <= innerHalfWidth)
                    alpha = Mathf.Max(alpha, 0.55f);

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // Matches drawStandardSphereCore's radial-gradient stops from src/renderer.ts.
        // The texture is generated in Unity because the upstream PNG parts are corrupt in both branches.
        private static Sprite CreateStandardCoreGlow()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "ES_StandardCoreGlow",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nx = ((x + 0.5f) / size) * 2f - 1f;
                var ny = ((y + 0.5f) / size) * 2f - 1f;
                var clipRadius = Mathf.Sqrt(nx * nx + ny * ny);
                if (clipRadius > 1f)
                {
                    pixels[y * size + x] = new Color32(255, 255, 255, 0);
                    continue;
                }

                // Canvas gradient center is (-0.08r, -0.10r); Unity texture Y is upward.
                var gx = nx + 0.08f / 0.58f;
                var gy = ny - 0.10f / 0.58f;
                var t = Mathf.Sqrt(gx * gx + gy * gy) * (0.58f / 0.62f);
                var color = EvaluateStandardCoreGradient(t);
                pixels[y * size + x] = new Color32(
                    (byte)Mathf.RoundToInt(color.r * 255f),
                    (byte)Mathf.RoundToInt(color.g * 255f),
                    (byte)Mathf.RoundToInt(color.b * 255f),
                    (byte)Mathf.RoundToInt(color.a * 255f));
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Color EvaluateStandardCoreGradient(float t)
        {
            if (t <= 0.20f) return Color.Lerp(new Color(1f, 1f, 1f, 0.98f), new Color(176f / 255f, 244f / 255f, 1f, 0.96f), t / 0.20f);
            if (t <= 0.48f) return Color.Lerp(new Color(176f / 255f, 244f / 255f, 1f, 0.96f), new Color(38f / 255f, 170f / 255f, 1f, 0.78f), (t - 0.20f) / 0.28f);
            if (t <= 0.82f) return Color.Lerp(new Color(38f / 255f, 170f / 255f, 1f, 0.78f), new Color(20f / 255f, 92f / 255f, 220f / 255f, 0.34f), (t - 0.48f) / 0.34f);
            if (t <= 1f) return Color.Lerp(new Color(20f / 255f, 92f / 255f, 220f / 255f, 0.34f), new Color(0f, 30f / 255f, 90f / 255f, 0f), (t - 0.82f) / 0.18f);
            return new Color(0f, 30f / 255f, 90f / 255f, 0f);
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
        // Geometry follows renderer.ts:drawModernXp() with its 6 px source radius.
        // Canvas Y is down, so the local sprite coordinates are vertically inverted.
        private static Vector2[] ExperienceShardPoints() => new[]
        {
            new Vector2(0f, 1f),
            new Vector2(0.62f / 1.1f, 0.35f / 1.1f),
            new Vector2(0.52f / 1.1f, -0.68f / 1.1f),
            new Vector2(0f, -1.05f / 1.1f),
            new Vector2(-0.52f / 1.1f, -0.68f / 1.1f),
            new Vector2(-0.62f / 1.1f, 0.35f / 1.1f),
        };
        private static Sprite CreateFlatPolygonSprite(string name, Vector2[] points)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var point = new Vector2(((x + 0.5f) / size) * 2f - 1f, ((y + 0.5f) / size) * 2f - 1f);
                pixels[y * size + x] = InsidePolygon(point, points)
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        private static Sprite CreatePolygonOutlineSprite(string name, Vector2[] points, float halfWidth)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var point = new Vector2(((x + 0.5f) / size) * 2f - 1f, ((y + 0.5f) / size) * 2f - 1f);
                var alpha = Mathf.Clamp01((halfWidth - DistanceToPolygonEdge(point, points)) * size * 0.5f + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        private static bool InsidePolygon(Vector2 p,Vector2[] poly)
        {
            var inside=false;
            for(int i=0,j=poly.Length-1;i<poly.Length;j=i++){var a=poly[i];var b=poly[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/((b.y-a.y)==0f?0.0001f:b.y-a.y)+a.x)inside=!inside;}
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