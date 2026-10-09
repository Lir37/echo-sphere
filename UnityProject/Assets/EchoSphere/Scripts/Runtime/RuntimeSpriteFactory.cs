using UnityEngine;

namespace EchoSphere.Runtime
{
    internal static class RuntimeSpriteFactory
    {
        private static Sprite _disc;
        private static Sprite _ring;
        public static Sprite Disc { get { if (_disc == null) _disc = CreateSprite(false); return _disc; } }
        public static Sprite Ring { get { if (_ring == null) _ring = CreateSprite(true); return _ring; } }

        private static Sprite CreateSprite(bool ring)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = ring ? "ES_Prototype_Ring" : "ES_Prototype_Disc",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = ((x + 0.5f) / size) * 2f - 1f;
                var dy = ((y + 0.5f) / size) * 2f - 1f;
                var radius = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = ring ? Mathf.Clamp01(1f - Mathf.Abs(radius - 0.73f) / 0.075f) : Mathf.Clamp01((0.98f - radius) * 38f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
