using System;
using UnityEditor;
using UnityEngine;

internal sealed class EchoSphereRegionBackdropImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/EchoSphere/RegionBackdrops/",StringComparison.Ordinal))return;
        var texture=(TextureImporter)assetImporter;
        texture.textureType=TextureImporterType.Default;texture.sRGBTexture=true;
        texture.mipmapEnabled=true;texture.isReadable=false;texture.npotScale=TextureImporterNPOTScale.None;
        texture.maxTextureSize=2048;texture.wrapMode=TextureWrapMode.Clamp;
        texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=1;
        texture.alphaSource=TextureImporterAlphaSource.None;texture.textureCompression=TextureImporterCompression.CompressedHQ;
        var android=texture.GetPlatformTextureSettings("Android");android.overridden=true;
        android.maxTextureSize=2048;android.format=TextureImporterFormat.ASTC_6x6;
        texture.SetPlatformTextureSettings(android);
    }
}
