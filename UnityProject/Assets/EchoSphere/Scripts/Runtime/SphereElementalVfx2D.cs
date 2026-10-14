using UnityEngine;

namespace EchoSphere.Runtime
{
    // src/spheres/elementalVisual.ts, with src/sphereProgression.ts ResolveElement mappings.
    public sealed class SphereElementalVfx2D : MonoBehaviour
    {
        public void Initialize(EchoSphereRuntime runtime, SphereAttackAgent agent)
        { gameObject.AddComponent<SourceElementalVisual2D>().Initialize(runtime, agent); }
    }
}
