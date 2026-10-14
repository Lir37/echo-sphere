using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ArtifactChestAgent2D : MonoBehaviour
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
            var pulse = 1f + Mathf.Sin(Time.time * 2.8f) * 0.06f;
            transform.position = _origin + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.02f);
            transform.localScale = Vector3.one * (0.28f * pulse);
            if (Vector2.Distance(transform.position, _runtime.PlayerPosition) <= 0.36f)
            {
                SourceAudio2D.Play("chest");
                _runtime.CollectArtifactChest(this);
            }
        }
    }
}
