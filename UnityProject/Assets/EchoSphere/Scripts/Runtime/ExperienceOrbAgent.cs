using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class ExperienceOrbAgent : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private int _amount = 1;
        public void Initialize(EchoSphereRuntime runtime, Transform player, int amount)
        {
            _runtime = runtime;
            _player = player;
            _amount = Mathf.Max(1, amount);
        }
        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            var distance = Vector2.Distance(transform.position, _player.position);
            if (distance < 1.8f) transform.position = Vector2.MoveTowards(transform.position, _player.position, 4.5f * Time.deltaTime);
            if (distance < 0.32f)
            {
                _runtime.AddExperience(_amount);
                Destroy(gameObject);
            }
        }
    }
}
