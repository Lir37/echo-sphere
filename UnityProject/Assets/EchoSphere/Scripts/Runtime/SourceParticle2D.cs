using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed class SourceParticle2D : MonoBehaviour, ISourceSimulationAgent
    {
        internal float SourceProjectionLife => _life;
        internal float SourceProjectionMaxLife => _maxLife;
        internal Vector2 SourceProjectionVelocity => _velocity;
        internal Color SourceProjectionColor => _color;
        internal float SourceProjectionSize => _sourceSize;
        private EchoSphereRuntime _runtime;
        private Vector2 _velocity;
        private float _life, _maxLife, _sourceSize;
        private Color _color;
        private Color _savedColor;
        internal Color SavedColor => _savedColor;
        internal void RestoreSavedColor(Color color) => _savedColor = color;
        public void Initialize(EchoSphereRuntime runtime, Vector2 velocity, float life, Color color, float maxLife = 0f,float sourceSize=0f)
        { _sourceSize=sourceSize; _runtime = runtime;
            _runtime.RegisterSimulationAgent(this); _velocity = velocity; _life = life; _maxLife = maxLife > 0f ? maxLife : life; _color = color; _savedColor = color; }
        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            transform.position += (Vector3)(_velocity * dt); _velocity *= 0.95f; _life -= dt;
            if (_life <= 0f) { _runtime.RetireNativeTransient(this); return; }
            // Preserve the legacy SavedEntity.color without a hidden Renderer.
            _savedColor = new Color(_color.r, _color.g, _color.b, _color.a * Mathf.Clamp01(_life / _maxLife));
        }
        internal void ResetForNativePool(){_runtime=null;_savedColor=default;}
    }
}
