using System;
using System.Collections.Generic;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Existing accesses in this partial class resolve to this nested type.
        // Shipping players forward directly to Unity's unchanged storage API.
        // Editor simulations isolate BOTH reads and writes, never falling back
        // to a player's real progression. The scope is synchronous/thread-local.
        private static class PlayerPrefs
        {
#if UNITY_EDITOR
            [ThreadStatic] private static Dictionary<string, object> _memory;
            internal static IDisposable Isolate()
            {
                var previous = _memory;
                var current = new Dictionary<string, object>(StringComparer.Ordinal);
                _memory = current;
                return new PreferenceScope(previous, current);
            }
            private sealed class PreferenceScope : IDisposable
            {
                private readonly Dictionary<string, object> _previous, _current;
                private bool _disposed;
                internal PreferenceScope(Dictionary<string, object> previous, Dictionary<string, object> current)
                { _previous = previous; _current = current; }
                public void Dispose()
                {
                    if (_disposed) return;
                    if (!ReferenceEquals(_memory, _current)) throw new InvalidOperationException("Simulation preference scopes must close on their thread in reverse order.");
                    _memory = _previous; _disposed = true;
                }
            }
#endif
            internal static int GetInt(string key, int fallback = 0)
            {
#if UNITY_EDITOR
                if (_memory != null) return _memory.TryGetValue(key, out var value) && value is int number ? number : fallback;
#endif
                return UnityEngine.PlayerPrefs.GetInt(key, fallback);
            }
            internal static float GetFloat(string key, float fallback = 0f)
            {
#if UNITY_EDITOR
                if (_memory != null) return _memory.TryGetValue(key, out var value) && value is float number ? number : fallback;
#endif
                return UnityEngine.PlayerPrefs.GetFloat(key, fallback);
            }
            internal static string GetString(string key, string fallback = "")
            {
#if UNITY_EDITOR
                if (_memory != null) return _memory.TryGetValue(key, out var value) && value is string text ? text : fallback;
#endif
                return UnityEngine.PlayerPrefs.GetString(key, fallback);
            }
            internal static bool HasKey(string key)
            {
#if UNITY_EDITOR
                if (_memory != null) return _memory.ContainsKey(key);
#endif
                return UnityEngine.PlayerPrefs.HasKey(key);
            }
            internal static void SetInt(string key, int value)
            {
#if UNITY_EDITOR
                if (_memory != null) { _memory[key] = value; return; }
#endif
                UnityEngine.PlayerPrefs.SetInt(key, value);
            }
            internal static void SetFloat(string key, float value)
            {
#if UNITY_EDITOR
                if (_memory != null) { _memory[key] = value; return; }
#endif
                UnityEngine.PlayerPrefs.SetFloat(key, value);
            }
            internal static void SetString(string key, string value)
            {
#if UNITY_EDITOR
                if (_memory != null) { _memory[key] = value; return; }
#endif
                UnityEngine.PlayerPrefs.SetString(key, value);
            }
            internal static void DeleteKey(string key)
            {
#if UNITY_EDITOR
                if (_memory != null) { _memory.Remove(key); return; }
#endif
                UnityEngine.PlayerPrefs.DeleteKey(key);
            }
            internal static void Save()
            {
#if UNITY_EDITOR
                if (_memory != null) return;
#endif
                UnityEngine.PlayerPrefs.Save();
            }
        }
    }
}
