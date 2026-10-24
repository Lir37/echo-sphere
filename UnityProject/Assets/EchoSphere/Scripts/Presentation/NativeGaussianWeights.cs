using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal static class NativeGaussianWeights
    {
        private static readonly Dictionary<float,Vector4[]> Kernels=new Dictionary<float,Vector4[]>();
        private static readonly Queue<float> Order=new Queue<float>();
        internal static bool TryGet(float sigma,out Vector4[] weights)
        {
            weights=null;if(!float.IsFinite(sigma)||sigma<.01f)return false;
            int radius=Mathf.CeilToInt(sigma*4);if(radius>=512)return false;
            if(Kernels.TryGetValue(sigma,out weights))return true;
            weights=new Vector4[128];
            for(int i=0;i<=radius;i++){var packed=weights[i/4];packed[i%4]=Mathf.Exp(-.5f*i*i/(sigma*sigma));weights[i/4]=packed;}
            if(Kernels.Count==32)Kernels.Remove(Order.Dequeue());Kernels.Add(sigma,weights);Order.Enqueue(sigma);return true;
        }
    }
}
