using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ExperienceOrbAgent : MonoBehaviour, ISourceSimulationAgent, IRunSaveColor
    {
        internal float SourceProjectionRotation => _rotation;
        private Color _savedColor = new Color(99f/255f,230f/255f,1f);
        Color IRunSaveColor.SavedColor => _savedColor;
        void IRunSaveColor.RestoreSavedColor(Color color) => _savedColor = color;
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private int _amount = 1;
        private int _echoAmount = -1;
        private bool _collected;
        private Vector2 _velocity;
        private float _rotation;
        public void Initialize(EchoSphereRuntime runtime, Transform player, int amount, Vector2 sourceVelocity = default, float rotation = 0f, int echoAmount = -1)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _player = player;
            _amount = Mathf.Max(1, amount);
            _echoAmount=echoAmount;
            _velocity = sourceVelocity * EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;
            _rotation = rotation;
        }
        public void CollectForRune()
        {
            if (_collected || _runtime == null) return;
            _collected = true;
            _runtime.CollectExperienceOrb(_amount,_echoAmount<0?_amount:_echoAmount);
            Destroy(gameObject);
        }

        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            if (_collected) return;
            _rotation += dt * 1.4f;
            transform.rotation = Quaternion.Euler(0f, 0f, -_rotation * Mathf.Rad2Deg);
            transform.position += (Vector3)(_velocity * dt);
            _velocity *= 0.9f;
            var distance = Vector2.Distance(transform.position, _player.position);
            if (distance < _runtime.GetExperienceMagnetRadiusWorld()) transform.position += (_player.position - transform.position) * Mathf.Min(1f, 20f * dt);
            if (distance < EchoSphere.Core.PlayerLoopRules.OrbPickupRadiusPixels * EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld)
            {
                _collected = true;
                SourceAudio2D.Play("pickup");
                _runtime.CollectExperienceOrb(_amount,_echoAmount<0?_amount:_echoAmount);
                Destroy(gameObject);
            }
        }
    }
}
