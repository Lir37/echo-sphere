using System.Collections.Generic;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public interface ISourceSimulationAgent { void SimulationTick(float dt); }
    public sealed partial class EchoSphereRuntime
    {
        private bool _simulationFrame;
        public float SimulationDeltaTime { get; private set; }
        private readonly List<MonoBehaviour> _simulationAgents = new List<MonoBehaviour>();
        private readonly Dictionary<System.Type,List<MonoBehaviour>> _simulationPhases = new Dictionary<System.Type,List<MonoBehaviour>>();
        public void RegisterSimulationAgent(MonoBehaviour agent)
        {
            if (agent == null || _simulationAgents.Contains(agent)) return;
            _simulationAgents.Add(agent);
            if (agent is SourceParticle2D)
            {
                var count = 0;
                foreach (var item in _simulationAgents) if (item != null && item.gameObject.activeInHierarchy && item is SourceParticle2D) count++;
                foreach (var item in _simulationAgents)
                {
                    if (count <= 220) break;
                    if (item == null || !(item is SourceParticle2D) || !item.gameObject.activeInHierarchy) continue;
                    item.gameObject.SetActive(false); Destroy(item.gameObject); count--;
                }
            }
        }
        private void TickSimulationPhase<T>(float dt, bool reverse = false) where T : MonoBehaviour, ISourceSimulationAgent
        {
            _simulationAgents.RemoveAll(agent => agent == null);
            if (!_simulationPhases.TryGetValue(typeof(T), out var phase))
                _simulationPhases.Add(typeof(T), phase = new List<MonoBehaviour>());
            phase.Clear();
            // Snapshot BEFORE ticking: agents registered by this phase wait for
            // its next invocation, just as with FindAll. Later phases see them.
            foreach (var agent in _simulationAgents)
                if (agent is T && agent.gameObject.activeInHierarchy) phase.Add(agent);
            for (var i = reverse ? phase.Count - 1 : 0; reverse ? i >= 0 : i < phase.Count; i += reverse ? -1 : 1)
            {
                var item = phase[i];
                if (item != null && item.gameObject.activeInHierarchy) ((T)item).SimulationTick(dt);
            }
            phase.Clear();
        }
        public int GetSimulationOrdinal(MonoBehaviour target)
        {
            var index = 0;
            foreach (var agent in _simulationAgents)
            { if (agent == null || !agent.gameObject.activeInHierarchy || agent.GetType() != target.GetType()) continue; if (agent == target) return index; index++; }
            return index;
        }
    }
}
