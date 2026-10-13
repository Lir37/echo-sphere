using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class HealthPackAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Vector3 _origin;

        public void Initialize(EchoSphereRuntime runtime)
        {
            _runtime = runtime;
            _origin = transform.position;
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.07f;
            transform.position = _origin + Vector3.up * (Mathf.Sin(Time.time * 2.5f) * 0.025f);
            transform.localScale = Vector3.one * (0.22f * pulse);
            if (Vector2.Distance(transform.position, _runtime.PlayerPosition) <= 0.34f)
            {
                _runtime.CollectHealthPack();
                Destroy(gameObject);
            }
        }
    }
}
