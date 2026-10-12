using UnityEngine;
using UnityEngine.Rendering;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped Engineer relay bolt from src/renderer.ts:drawEnergyBolt.
    /// Cosmetic only; this effect does not consume gameplay RNG.
    /// </summary>
    public sealed class EngineerRelayVfx2D : MonoBehaviour
    {
        private const int Segments = 7;
        private const float SourceAlphaLifetime = 0.30f;
        private static Material _sharedMaterial;
        private readonly Vector3[] _positions = new Vector3[Segments + 1];
        private EchoSphereRuntime _runtime;
        private LineRenderer _glow;
        private LineRenderer _core;
        private Vector2 _from;
        private Vector2 _to;
        private float _elapsed;
        private const float Duration = 0.12f;

        public void Initialize(EchoSphereRuntime runtime, Vector2 from, Vector2 to)
        {
            _runtime = runtime;
            _from = from;
            _to = to;
            _glow = CreateLine("Relay Glow", 17, 7f * SpherePlacementRules.SourcePixelsToWorld);
            _core = CreateLine("Relay Core", 18, 1.5f * SpherePlacementRules.SourcePixelsToWorld);
            Rebuild(0.4f);
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
                        _sharedMaterial.name = "ECHO SPHERE Engineer Relay (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        private LineRenderer CreateLine(string layerName, int sortingOrder, float width)
        {
            var layer = new GameObject(layerName);
            layer.transform.SetParent(transform, false);
            var line = layer.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = Segments + 1;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 3;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.sharedMaterial = SharedMaterial;
            line.sortingOrder = sortingOrder;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.startColor = Color.white;
            line.endColor = Color.white;
            return line;
        }

        private void Update()
        {
            if (_runtime != null && _runtime.IsPresentationPaused) return;
            _elapsed += Time.deltaTime;
            var life = Duration - _elapsed;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            Rebuild(Mathf.Clamp01(life / SourceAlphaLifetime));
        }

        private void Rebuild(float alpha)
        {
            var delta = _to - _from;
            var length = delta.magnitude;
            var normal = length > 0.0001f ? new Vector2(-delta.y, delta.x) / length : Vector2.zero;
            var wobble = Mathf.Min(26f * SpherePlacementRules.SourcePixelsToWorld, length * 0.08f);
            _positions[0] = _from;
            for (var i = 1; i < Segments; i++)
            {
                var p = i / (float)Segments;
                var wave = Mathf.Sin(i * 3.7f + _elapsed * 33.3333333f) * wobble * 0.35f;
                _positions[i] = _from + delta * p + normal * wave;
            }
            _positions[Segments] = _to;
            _glow.SetPositions(_positions);
            _core.SetPositions(_positions);
            _glow.startColor = _glow.endColor =
                new Color(105f / 255f, 235f / 255f, 1f, 0.28f * alpha);
            _core.startColor = _core.endColor =
                new Color(235f / 255f, 252f / 255f, 1f, 0.90f * alpha);
        }
    }
}
