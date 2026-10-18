Shader "Hidden/EchoSphere/NativePrefixProducerProbe"
{
 Properties { _MainTex("Animated pattern",2D)="black"{} _LabelTex("Static native label",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex,_LabelTex;
   float4 frag(v2f_img i):SV_Target {
    float4 background=tex2D(_MainTex,i.uv),label=tex2D(_LabelTex,i.uv);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     background.rgb=LinearToGammaSpace(background.rgb);
     label.rgb=LinearToGammaSpace(label.rgb);
    #endif
    float4 result=label+background*(1-label.a);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     result.rgb=GammaToLinearSpace(result.rgb);
    #endif
    return result;
   }
   ENDCG
  }
 }
}
