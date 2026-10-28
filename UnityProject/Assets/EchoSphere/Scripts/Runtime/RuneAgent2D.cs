using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class RuneAgent2D : MonoBehaviour, ISourceSimulationAgent, IRunSaveColor
    {
        internal string SourceProjectionType => _type;
        internal float SourceProjectionLife => _life;
        public static readonly string[] AllTypes =
        {
            "overdrive", "phase", "harvest", "purge", "resonance",
            "fortify", "hunt", "echo", "gravity"
        };

        private Color _savedColor = Color.white;
        Color IRunSaveColor.SavedColor => _savedColor;
        void IRunSaveColor.RestoreSavedColor(Color color) => _savedColor = color;
        private EchoSphereRuntime _runtime;
        private string _type;
        private float _life;
        private Vector3 _origin;

        public void Initialize(EchoSphereRuntime runtime, string type, Vector2 position, float lifetime)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _type = type;
            _life = Mathf.Max(0f, lifetime);
            _origin = new Vector3(position.x, position.y, transform.position.z);
            _savedColor = GetRuneColor(type);
        }

        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            _life -= _runtime.SimulationDeltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            var phase = (_runtime.CurrentRunTime + _runtime.GetSimulationOrdinal(this)) * 3f;
            transform.position -= Vector3.up * (Mathf.Sin(phase) * dt * 3f * EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld);
            var pulse = 1f + Mathf.Sin(phase * 1.35f) * 0.08f;
            transform.localScale = Vector3.one * (0.26f * pulse);
            var tint = GetRuneColor(_type);
            tint.a = Mathf.Clamp01(Mathf.Min(1f, _life / 2f));
            _savedColor = tint;
            if (Vector2.Distance(transform.position, _runtime.PlayerPosition) <= 31f * EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld)
            {
                _runtime.ActivateRunePickup(_type, transform.position);
                Destroy(gameObject);
            }
        }

        public static Color GetRuneColor(string type)
        {
            switch (type)
            {
                case "overdrive": return new Color(1f, 0.89f, 0.42f);
                case "phase": return new Color(0.55f, 0.94f, 1f);
                case "harvest": return new Color(0.39f, 0.9f, 1f);
                case "purge": return new Color(1f, 0.4f, 0.55f);
                case "resonance": return new Color(0.72f, 0.55f, 1f);
                case "fortify": return new Color(0.42f, 0.72f, 1f);
                case "hunt": return new Color(1f, 0.62f, 0.3f);
                case "echo": return new Color(0.85f, 0.97f, 1f);
                case "gravity": return new Color(0.65f, 0.48f, 1f);
                default: return Color.white;
            }
        }

        public static string GetRuneNameRussian(string type)
        {
            switch (type)
            {
                case "overdrive": return "РУНА РАЗГОНА";
                case "phase": return "РУНА ФАЗЫ";
                case "harvest": return "РУНА СБОРА";
                case "purge": return "РУНА ОЧИЩЕНИЯ";
                case "resonance": return "РУНА РЕЗОНАНСА";
                case "fortify": return "РУНА УКРЕПЛЕНИЯ";
                case "hunt": return "РУНА ОХОТЫ";
                case "echo": return "РУНА ЭХА";
                case "gravity": return "РУНА ГРАВИТАЦИИ";
                default: return "РУНА";
            }
        }
    }
}
