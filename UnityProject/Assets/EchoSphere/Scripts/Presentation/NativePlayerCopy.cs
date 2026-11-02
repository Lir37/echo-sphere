using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Player-facing wording is separate from imported IDs, balance and save data.
    // Exact lookups only: no per-frame regex, tree walk or mutation of source assets.
    public static class NativePlayerCopy
    {
        [Serializable] public sealed class Revision { public string Source, Text; }
        [Serializable] public sealed class AbilityText { public string Id, Ru, En; }
        [Serializable] public sealed class Catalog
        {
            public Revision[] Ru, En;
            public AbilityText[] Abilities;
        }
        private static Dictionary<string, string> _ru, _en;
        private static Dictionary<string, AbilityText> _abilities;
        private static void EnsureLoaded()
        {
            if (_ru != null) return;
            var asset = Resources.Load<TextAsset>("EchoSphere/PlayerCopy");
            if (asset == null) throw new InvalidOperationException("Player copy catalog missing");
            var data = JsonUtility.FromJson<Catalog>(asset.text);
            var ru = Index(data.Ru); var en = Index(data.En);
            var abilities = new Dictionary<string, AbilityText>(StringComparer.Ordinal);
            foreach (var row in data.Abilities) abilities.Add(row.Id, row);
            _en = en; _abilities = abilities; _ru = ru;
        }
        private static Dictionary<string, string> Index(Revision[] rows)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var row in rows) result.Add(row.Source, row.Text);
            return result;
        }
        public static string Text(string value, bool russian)
        {
            if (string.IsNullOrEmpty(value)) return value;
            EnsureLoaded();
            return (russian ? _ru : _en).TryGetValue(value, out var text) ? text : value;
        }
        public static string AbilityDescription(string id, bool russian, string fallback)
        {
            EnsureLoaded();
            return _abilities.TryGetValue(id, out var row) ? russian ? row.Ru : row.En : Text(fallback, russian);
        }
    }
}
