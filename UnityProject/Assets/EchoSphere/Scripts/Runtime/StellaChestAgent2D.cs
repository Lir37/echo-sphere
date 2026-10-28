using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped Stella world pickup: renderer.ts:drawStellaChest and
    /// engineLoop.ts chest proximity collection. Rendering is presentation-only.
    /// </summary>
    public sealed class StellaChestAgent2D : MonoBehaviour, ISourceSimulationAgent
    {
        private EchoSphereRuntime _runtime;
        private float _time;
        private bool _collected;

        public void Initialize(EchoSphereRuntime runtime, Vector2 position)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            transform.position=position;
        }

        public void SimulationTick(float dt)
        {
            if(_collected||_runtime==null||_runtime.IsGameplayPaused)return;
            _time+=_runtime.SimulationDeltaTime;
            var pulse=1f+Mathf.Sin(_time*2.6f)*0.08f;
            transform.localScale=Vector3.one*pulse;
            // Source threshold is player radius (16 px) + Stella radius (28 px).
            if(Vector2.Distance(transform.position,_runtime.PlayerPosition) <
                BossCombatRules.ToWorldUnits(BossCombatRules.PlayerRadiusSourceUnits+28f))
            {
                _collected=true;
                _runtime.CollectStellaChest(this);
            }
        }

    }
}
