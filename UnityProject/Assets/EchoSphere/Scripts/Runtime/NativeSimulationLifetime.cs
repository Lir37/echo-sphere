using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    // Unity normally drains Destroy at frame end. Synchronous Editor probes
    // advance virtual frames without yielding to the player loop, so their
    // scoped queue drains at the same gameplay boundary. Shipping behavior is
    // the ordinary Unity API; no pooled entity or save representation changes.
    internal static class NativeSimulationLifetime
    {
#if UNITY_EDITOR
        [ThreadStatic] private static List<GameObject> _pending;
        internal static IDisposable Isolate()
        {
            var previous = _pending;
            var current = new List<GameObject>();
            _pending = current;
            return new LifetimeScope(previous, current);
        }
        internal static void Flush()
        {
            if (_pending == null) throw new InvalidOperationException("No isolated simulation lifetime.");
            for (int i = 0; i < _pending.Count; i++)
                if (_pending[i] != null) UnityEngine.Object.DestroyImmediate(_pending[i]);
            _pending.Clear();
        }
        private sealed class LifetimeScope : IDisposable
        {
            private readonly List<GameObject> _previous, _current;
            private bool _disposed;
            internal LifetimeScope(List<GameObject> previous, List<GameObject> current)
            { _previous = previous; _current = current; }
            public void Dispose()
            {
                if (_disposed) return;
                if (!ReferenceEquals(_pending, _current)) throw new InvalidOperationException("Simulation lifetime scopes must close in reverse order on their thread.");
                try { Flush(); } finally { _pending = _previous; _disposed = true; }
            }
        }
#endif
        internal static void Destroy(GameObject target)
        {
#if UNITY_EDITOR
            if (_pending != null) { if (target != null) _pending.Add(target); return; }
#endif
            UnityEngine.Object.Destroy(target);
        }
    }
}
