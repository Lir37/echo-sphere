using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A detail page owns exactly one illustration. Lists keep their production portraits.
    internal sealed class NativeArchiveDetailArtwork : IDisposable
    {
        [Serializable] private sealed class Manifest {public List<string> Paths=new List<string>();}
        private static HashSet<string> _available;
        private static bool Available(string path){if(_available==null){var asset=Resources.Load<TextAsset>("EchoSphere/ArchiveIllustrationManifest");if(asset==null)return true;_available=new HashSet<string>(JsonUtility.FromJson<Manifest>(asset.text).Paths,StringComparer.Ordinal);}return _available.Contains(path);}
        private Image _target;
        private Texture2D _texture;
        private string _path;
        private int _version;

        internal static string ResourcePath(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) throw new ArgumentException(nameof(entryId));
            if(entryId.StartsWith("region-lore:",StringComparison.Ordinal)){var parts=entryId.Split(':');if(parts.Length==3)return "EchoSphere/RegionBackdrops/"+parts[1];}
            return "EchoSphere/ArchiveIllustrations/" + entryId.Replace(':', '-');
        }

        internal void Show(string entryId, Image target)
        {
            Dispose();
            _target = target;
            string path = _path = ResourcePath(entryId);
            // Some records intentionally use their native emblem until their painting exists.
            // Declared paintings still warn if missing; absent records never issue a failing request.
            if(!Available(path))return;
            int version = _version;
            var request = Resources.LoadAsync<Texture2D>(path);
            request.completed += _ =>
            {
                var texture = request.asset as Texture2D;
                if (version != _version || _target == null)
                {
                    // Rapid A -> B -> A may share the outstanding A request. Its new owner
                    // must finish binding before anybody unloads that same asset.
                    if (texture != null && _path != path) Resources.UnloadAsset(texture);
                    return;
                }
                if (_target.panel == null)
                {
                    Dispose();
                    if (texture != null) Resources.UnloadAsset(texture);
                    return;
                }
                _texture = texture;
                _target.image = texture;
                if (texture == null) Debug.LogWarning("Archive illustration missing: " + entryId);
            };
        }

        public void Dispose()
        {
            ++_version;
            _path = null;
            if (_target != null) _target.image = null;
            _target = null;
            if (_texture != null) Resources.UnloadAsset(_texture);
            _texture = null;
        }
    }
}
