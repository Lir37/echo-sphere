#if UNITY_INCLUDE_INSTRUMENTATION
namespace EchoSphere.Presentation {
 internal static class NativeWorldDiagnostics {
  internal static readonly Unity.Profiling.ProfilerMarker Standard=new Unity.Profiling.ProfilerMarker("EchoSphere.World.StandardRig");
  internal static readonly Unity.Profiling.ProfilerMarker Family=new Unity.Profiling.ProfilerMarker("EchoSphere.World.FamilyRig");
  internal static readonly Unity.Profiling.ProfilerMarker Enemy=new Unity.Profiling.ProfilerMarker("EchoSphere.World.EnemyRig");
  internal static readonly Unity.Profiling.ProfilerMarker Player=new Unity.Profiling.ProfilerMarker("EchoSphere.World.PlayerRig");
  internal static readonly Unity.Profiling.ProfilerMarker Network=new Unity.Profiling.ProfilerMarker("EchoSphere.World.NetworkRig");
 }
}
#endif
