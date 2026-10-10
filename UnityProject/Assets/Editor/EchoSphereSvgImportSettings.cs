using System;
using UnityEditor;
using UnityEngine;
using Unity.VectorGraphics.Editor;

namespace EchoSphere.Editor
{
    /// <summary>
    /// Applies safe import settings before source SVG/PNG artwork is imported.
    /// Never manually reimports an asset from a postprocessor callback.
    /// </summary>
    internal sealed class EchoSphereSvgImportSettings : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Resources/EchoSphere/Art/";
        private const float PixelsPerUnit = 128f;

        private void OnPreprocessAsset()
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.StartsWith(ArtRoot, StringComparison.OrdinalIgnoreCase))
                return;

            if (assetPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var svgImporter = assetImporter as SVGImporter;
                if (svgImporter == null) return;

                svgImporter.SvgType = SVGType.TexturedSprite;
                svgImporter.UseSVGPixelsPerUnit = true;
                svgImporter.SvgPixelsPerUnit = PixelsPerUnit;
                svgImporter.GradientResolution = 64;
                return;
            }

            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;
            var textureImporter = assetImporter as TextureImporter;
            if (textureImporter == null) return;

            // The Standard Sphere is assembled from four authored transparent
            // layers; import them as Sprite assets so Resources.Load<Sprite>()
            // works without manual Inspector changes.
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePixelsPerUnit = PixelsPerUnit;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
