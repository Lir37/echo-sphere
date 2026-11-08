using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A detail page owns exactly one illustration. Lists keep their production portraits.
    internal sealed class NativeArchiveDetailArtwork : IDisposable
    {
        private Image _target;
        private Texture2D _texture;
        private string _path;
        private int _version;

        internal static string ResourcePath(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) throw new ArgumentException(nameof(entryId));
            return "EchoSphere/ArchiveIllustrations/" + entryId.Replace(':', '-');
        }

        internal void Show(string entryId, Image target)
        {
            Dispose();
            _target = target;
            string path = _path = ResourcePath(entryId);
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
