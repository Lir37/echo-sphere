using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ExperienceOrbAgent : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private int _amount = 1;
        private bool _collected;
        public void Initialize(EchoSphereRuntime runtime, Transform player, int amount)
        {
            _runtime = runtime;
            _player = player;
            _amount = Mathf.Max(1, amount);
        }
        public void CollectForRune()
        {
            if (_collected || _runtime == null) return;
            _collected = true;
            SourceAudio2D.Play("pickup");
            _runtime.AddExperience(_amount);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            var distance = Vector2.Distance(transform.position, _player.position);
            if (distance < _runtime.GetExperienceMagnetRadiusWorld()) transform.position = Vector2.MoveTowards(transform.position, _player.position, 4.5f * Time.deltaTime);
            if (distance < 0.32f)
            {
                SourceAudio2D.Play("pickup");
                _runtime.AddExperience(_amount);
                Destroy(gameObject);
            }
        }
    }
}
