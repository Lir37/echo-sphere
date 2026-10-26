using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal static class NativeGaussianWeights
    {
        private static readonly Dictionary<float,Vector4[]> Kernels=new Dictionary<float,Vector4[]>();
        private static readonly Dictionary<float,float> UiNormalizations=new Dictionary<float,float>();
        private static readonly Queue<float> Order=new Queue<float>();
        internal static bool TryGet(float sigma,out Vector4[] weights)
        {
            weights=null;if(!float.IsFinite(sigma)||sigma<.01f)return false;
            int radius=Mathf.CeilToInt(sigma*4);if(radius>=512)return false;
            if(Kernels.TryGetValue(sigma,out weights))return true;
            weights=new Vector4[128];
            for(int i=0;i<=radius;i++){var packed=weights[i/4];packed[i%4]=Mathf.Exp(-.5f*i*i/(sigma*sigma));weights[i/4]=packed;}
            if(Kernels.Count==32){var expired=Order.Dequeue();Kernels.Remove(expired);UiNormalizations.Remove(expired);}Kernels.Add(sigma,weights);Order.Enqueue(sigma);return true;
        }
        internal static void BindUiShadow(MaterialPropertyBlock properties,float sigma)
        {
            bool cached=TryGet(sigma,out var weights);int radius=cached?Mathf.CeilToInt(4*sigma):0;
            properties.SetInt("_UiShadowWeightCount",cached?radius+1:0);if(!cached)return;
            if(!UiNormalizations.TryGetValue(sigma,out float normalization)){
                float sum=weights[0].x;for(int i=1;i<=radius;i++)sum+=2*weights[i>>2][i&3];
                normalization=sum*sum;UiNormalizations.Add(sigma,normalization);
            }
            properties.SetVectorArray("_UiShadowWeights",weights);properties.SetFloat("_UiShadowNormalization",normalization);properties.SetInt("_UiShadowPairing",1);
        }
    }
}
