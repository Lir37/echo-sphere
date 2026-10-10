using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.VectorGraphics.Editor;
using UnityEngine;

namespace EchoSphere.Editor
{
    /// <summary>Imports the authored SVG library as textured Sprites for both SpriteRenderer and IMGUI.</summary>
    [InitializeOnLoad]
    internal sealed class EchoSphereSvgImportSettings : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Resources/EchoSphere/Art/";
        private const float PixelsPerUnit = 128f;
        private static bool _applying;

        static EchoSphereSvgImportSettings()
        {
            EditorApplication.delayCall += ConfigureExistingAssets;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (_applying) return;
            ConfigurePaths(importedAssets);
        }

        private static void ConfigureExistingAssets()
        {
            var guids = AssetDatabase.FindAssets(string.Empty, new[] { ArtRoot.TrimEnd('/') });
            var paths = new List<string>(guids.Length);
            foreach (var guid in guids) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            ConfigurePaths(paths.ToArray());
        }

        private static void ConfigurePaths(string[] paths)
        {
            if (_applying || paths == null) return;
            try
            {
                _applying = true;
                foreach (var path in paths)
                {
                    if (string.IsNullOrEmpty(path) ||
                        !path.StartsWith(ArtRoot, StringComparison.OrdinalIgnoreCase) ||
                        !path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                        continue;
                    var importer = AssetImporter.GetAtPath(path) as SVGImporter;
                    if (importer == null) continue;
                    var changed = false;
                    if (importer.SvgType != SVGType.TexturedSprite) { importer.SvgType = SVGType.TexturedSprite; changed = true; }
                    if (!importer.UseSVGPixelsPerUnit) { importer.UseSVGPixelsPerUnit = true; changed = true; }
                    if (Mathf.Abs(importer.SvgPixelsPerUnit - PixelsPerUnit) > 0.01f) { importer.SvgPixelsPerUnit = PixelsPerUnit; changed = true; }
                    if (importer.GradientResolution != 64) { importer.GradientResolution = 64; changed = true; }
                    if (changed) importer.SaveAndReimport();
                }
            }
            finally { _applying = false; }
        }
    }
}
