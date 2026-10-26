Shader "EchoSphere/NativeUiAlphaShadow"
{
 Properties { _MainTex("Unity filter input",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  HLSLINCLUDE
  #include "UnityCG.cginc"
  #include "UnityUIEFilter.cginc"
  sampler2D _MainTex;float4 _MainTex_ST,_MainTex_TexelSize,_CssColor;
  float _Sigma,_CssDpr,_CssReadsGamma;float4 _Weights[128];int _WeightCount;
  struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;nointerpolation uint rectIndex:TEXCOORD1;};
  output vertex(FilterVertexInput v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.rectIndex=GetFilterRectIndex(v);return o;}
  float4 Finish(float4 c){
   #if defined(_UIE_OUTPUT_LINEAR) && !defined(UNITY_COLORSPACE_GAMMA)
    c.rgb=GammaToLinearSpace(c.rgb);
   #endif
   return c;
  }
  float4 Raw(float2 uv,float4 rect){if(any(uv<rect.xy)||any(uv>rect.xy+rect.zw))return float4(0,0,0,0);return tex2Dlod(_MainTex,float4(uv,0,0));}
  float4 Source(float2 uv,float4 rect){float4 c=Raw(uv,rect);if(_CssReadsGamma<.5)c.rgb=c.a>0?LinearToGammaSpace(c.rgb/c.a)*c.a:0;return c;}
  float Blur(float2 uv,float4 rect,float2 direction){
   float sigma=max(0,_Sigma*_CssDpr),sum=Raw(uv,rect).a,normalization=1;
   if(sigma<.001)return sum;int radius=(int)ceil(4*sigma);
   [loop]for(int i=1;i<=radius;i++){float weight=_WeightCount>radius?_Weights[i>>2][i&3]:exp(-i*(float)i/(2*sigma*sigma));float2 delta=i*direction*_MainTex_TexelSize.xy;sum+=(Raw(uv+delta,rect).a+Raw(uv-delta,rect).a)*weight;normalization+=2*weight;}
   return sum/normalization;
  }
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vertex
   #pragma fragment horizontal
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 horizontal(output i):SV_Target {float a=Blur(i.uv,GetFilterUVRect(i.rectIndex),float2(1,0));return float4(0,0,0,a);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vertex
   #pragma fragment vertical
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 vertical(output i):SV_Target {float a=Blur(i.uv,GetFilterUVRect(i.rectIndex),float2(0,1))*_CssColor.a;return Finish(float4(_CssColor.rgb*a,a));}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vertex
   #pragma fragment identity
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 identity(output i):SV_Target {return Finish(Source(i.uv,GetFilterUVRect(i.rectIndex)));}
   ENDHLSL
  }
 }
}
