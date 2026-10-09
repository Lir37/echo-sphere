using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class PulseEffect2D : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Color _color;
        private float _duration = 0.18f;
        private float _elapsed;
        private float _finalScale = 1.1f;

        public void Initialize(Color color, float duration = 0.18f, float finalScale = 1.1f)
        {
            _color = color;
            _duration = Mathf.Max(0.01f, duration);
            _finalScale = finalScale;
            transform.localScale = Vector3.one * 0.2f;
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _renderer.color = _color;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, _finalScale, t);
            if (_renderer != null)
            {
                var current = _color;
                current.a = 1f - t;
                _renderer.color = current;
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
