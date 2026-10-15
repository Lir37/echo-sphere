using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed class SourceParticle2D : MonoBehaviour, ISourceSimulationAgent
    {
        private EchoSphereRuntime _runtime;
        private Vector2 _velocity;
        private float _life, _maxLife, _sourceSize;
        private Color _color;
        private SpriteRenderer _renderer;
        public void Initialize(EchoSphereRuntime runtime, Vector2 velocity, float life, Color color, float maxLife = 0f,float sourceSize=0f)
        { _sourceSize=sourceSize; _runtime = runtime;
            _runtime.RegisterSimulationAgent(this); _velocity = velocity; _life = life; _maxLife = maxLife > 0f ? maxLife : life; _color = color; _renderer = GetComponent<SpriteRenderer>(); }
        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            transform.position += (Vector3)(_velocity * dt); _velocity *= 0.95f; _life -= dt;
            if (_life <= 0f) { Destroy(gameObject); return; }
            _renderer.color = new Color(_color.r, _color.g, _color.b, _color.a * Mathf.Clamp01(_life / _maxLife));
        }
    }
}
