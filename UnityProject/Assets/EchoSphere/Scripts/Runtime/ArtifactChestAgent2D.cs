using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ArtifactChestAgent2D : MonoBehaviour, ISourceSimulationAgent
    {
        private EchoSphereRuntime _runtime;
        private Vector3 _origin;

        public void Initialize(EchoSphereRuntime runtime)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _origin = transform.position;
        }

        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var pulse = 1f + Mathf.Sin(Time.time * 2.8f) * 0.06f;
            transform.position = _origin;
            transform.localScale = Vector3.one * (0.28f * pulse);
            if (Vector2.Distance(transform.position, _runtime.PlayerPosition) < 30f * EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld)
            {
                SourceAudio2D.Play("chest");
                _runtime.CollectArtifactChest(this);
            }
        }
    }
}
