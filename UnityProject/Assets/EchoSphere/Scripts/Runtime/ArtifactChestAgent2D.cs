using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ArtifactChestAgent2D : MonoBehaviour, ISourceSimulationAgent, IRunSaveColor
    {
        private Color _savedColor = new Color(1f,0.76f,0.3f);
        Color IRunSaveColor.SavedColor => _savedColor;
        void IRunSaveColor.RestoreSavedColor(Color color) => _savedColor = color;
        private EchoSphereRuntime _runtime;
        private Vector3 _origin;
        private bool _consumed;
        internal bool TryConsume(){if(_consumed)return false;_consumed=true;gameObject.SetActive(false);return true;}

        public void Initialize(EchoSphereRuntime runtime)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _origin = transform.position;
        }

        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _consumed) return;
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
