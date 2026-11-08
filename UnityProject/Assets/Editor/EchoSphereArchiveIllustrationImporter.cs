using UnityEditor;
using UnityEngine;

// Archive detail art is opaque, never sampled in the battle or loaded as a whole atlas.
internal sealed class EchoSphereArchiveIllustrationImporter : AssetPostprocessor
{
    internal const string Prefix = "Assets/Resources/EchoSphere/ArchiveIllustrations/";
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Prefix, System.StringComparison.Ordinal)) return;
        var texture = (TextureImporter)assetImporter;
        texture.textureType = TextureImporterType.Default;
        texture.sRGBTexture = true;
        texture.mipmapEnabled = false;
        texture.isReadable = false;
        texture.npotScale = TextureImporterNPOTScale.None;
        texture.maxTextureSize = 1024;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.alphaSource = TextureImporterAlphaSource.None;
        texture.textureCompression = TextureImporterCompression.CompressedHQ;
        var android = texture.GetPlatformTextureSettings("Android");
        android.overridden = true;
        android.maxTextureSize = 1024;
        android.format = TextureImporterFormat.ASTC_6x6;
        texture.SetPlatformTextureSettings(android);
    }
}
