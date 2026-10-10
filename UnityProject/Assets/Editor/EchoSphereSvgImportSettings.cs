using System;
using UnityEditor;
using Unity.VectorGraphics.Editor;

namespace EchoSphere.Editor
{
    /// <summary>
    /// Applies SVG import settings before the SVG importer runs.
    /// Never call SaveAndReimport from an asset-postprocess callback.
    /// </summary>
    internal sealed class EchoSphereSvgImportSettings : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Resources/EchoSphere/Art/";
        private const float PixelsPerUnit = 128f;

        private void OnPreprocessAsset()
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.StartsWith(ArtRoot, StringComparison.OrdinalIgnoreCase) ||
                !assetPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = assetImporter as SVGImporter;
            if (importer == null) return;

            // These values are applied to the importer before parsing begins.
            // Mutating importer settings here avoids recursive reimport loops.
            importer.SvgType = SVGType.TexturedSprite;
            importer.UseSVGPixelsPerUnit = true;
            importer.SvgPixelsPerUnit = PixelsPerUnit;
            importer.GradientResolution = 64;
        }
    }
}
