Shader "EchoSphere/SourceUiFilters"
{
 Properties { _MainTex("Subtree",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  HLSLINCLUDE
  #include "UnityCG.cginc"
  #include "UnityUIEFilter.cginc"
  UNITY_DECLARE_TEX2D(_MainTex);float4 _MainTex_ST,_MainTex_TexelSize;float _Amount,_OffsetX,_OffsetY,_Sigma,_CssDpr,_CssReadsGamma;float4 _CssColor;
  float4 _UiShadowWeights[128];int _UiShadowWeightCount,_UiShadowPairing;float _UiShadowNormalization;
  #if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
   SamplerState shadow_linear_clamp_sampler;
  #endif
  struct effectOutput {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;nointerpolation uint rectIndex:TEXCOORD1;};
  effectOutput effectVertex(FilterVertexInput v){effectOutput o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.rectIndex=GetFilterRectIndex(v);return o;}
  float4 Finish(float4 c){
   #if defined(_UIE_OUTPUT_LINEAR) && !defined(UNITY_COLORSPACE_GAMMA)
    c.rgb=GammaToLinearSpace(c.rgb);
   #endif
   return c;
  }
  float Alpha(float2 uv,float4 rect){return any(uv<rect.xy)||any(uv>rect.xy+rect.zw)?0:UNITY_SAMPLE_TEX2D(_MainTex,uv).a;}
  float4 Read(float2 uv){float4 c=UNITY_SAMPLE_TEX2D(_MainTex,uv);if(_CssReadsGamma<.5)c.rgb=c.a>0?LinearToGammaSpace(c.rgb/c.a)*c.a:0;return c;}
  float UiWeight(int offset){int i=abs(offset);return _UiShadowWeights[i>>2][i&3];}
  float CachedAlpha(float2 origin,float4 rect,int radius){
   float alpha=0;
   #if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
    float2 phase=frac(origin/_MainTex_TexelSize.xy-.5);
    if(_UiShadowPairing!=0&&all(min(phase,1-phase)<.001)){
     // Four adjacent weighted texels are one bilinear read at texel centers.
     // Keep individual reads at atlas edges; another filter can occupy the neighbour.
     [loop]for(int y=-radius;y<=radius;y+=2){float ya=UiWeight(y),yb=y<radius?UiWeight(y+1):0,wy=ya+yb;if(wy<=0)continue;
      [loop]for(int x=-radius;x<=radius;x+=2){float xa=UiWeight(x),xb=x<radius?UiWeight(x+1):0,wx=xa+xb;if(wx<=0)continue;
       float2 a=origin+float2(x,y)*_MainTex_TexelSize.xy;
       float2 b=origin+float2(x+(x<radius?1:0),y+(y<radius?1:0))*_MainTex_TexelSize.xy;
       float2 lo=min(a,b),hi=max(a,b);
       if(any(hi<rect.xy)||any(lo>rect.xy+rect.zw))continue;
       if(all(lo>=rect.xy)&&all(hi<=rect.xy+rect.zw)){
        float2 uv=origin+float2(x+xb/wx,y+yb/wy)*_MainTex_TexelSize.xy;
        alpha+=_MainTex.SampleLevel(shadow_linear_clamp_sampler,uv,0).a*wx*wy;
       }else{
        alpha+=Alpha(a,rect)*xa*ya;
        if(xb>0)alpha+=Alpha(a+float2(_MainTex_TexelSize.x,0),rect)*xb*ya;
        if(yb>0)alpha+=Alpha(a+float2(0,_MainTex_TexelSize.y),rect)*xa*yb;
        if(xb>0&&yb>0)alpha+=Alpha(b,rect)*xb*yb;
       }
      }
     }
     return alpha/max(.000001,_UiShadowNormalization);
    }
   #endif
   [loop]for(int y=-radius;y<=radius;y++){float wy=UiWeight(y);[loop]for(int x=-radius;x<=radius;x++)alpha+=Alpha(origin+float2(x,y)*_MainTex_TexelSize.xy,rect)*wy*UiWeight(x);}
   return alpha/max(.000001,_UiShadowNormalization);
  }
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentSaturate
   #pragma target 3.5
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentSaturate(effectOutput i):SV_Target {float4 c=Read(i.uv);float3 rgb=c.a>0?c.rgb/c.a:0;float luma=dot(rgb,float3(.213,.715,.072));c.rgb=saturate(lerp(luma.xxx,rgb,_Amount))*c.a;return Finish(c);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentBrightness
   #pragma target 3.5
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentBrightness(effectOutput i):SV_Target {float4 c=Read(i.uv);c.rgb=min(c.a,max(0,c.rgb*_Amount));return Finish(c);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentShadow
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   #pragma target 3.5
   float4 fragmentShadow(effectOutput i):SV_Target {
    float4 rect=GetFilterUVRect(i.rectIndex);float4 original=Read(i.uv);float sigma=max(0,_Sigma*_CssDpr);float2 origin=i.uv-float2(_OffsetX,-_OffsetY)*_CssDpr*_MainTex_TexelSize.xy;float alpha=0;
    if(sigma<.001)alpha=Alpha(origin,rect);
    else if(_UiShadowWeightCount>(int)ceil(4*sigma))alpha=CachedAlpha(origin,rect,(int)ceil(4*sigma));
    else {
     int radius=(int)ceil(4*sigma);float normalization=0;
     [loop]for(int y=-radius;y<=radius;y++){
      float wy=exp(-y*(float)y/(2*sigma*sigma));
      [loop]for(int x=-radius;x<=radius;x++){float weight=wy*exp(-x*(float)x/(2*sigma*sigma));alpha+=Alpha(origin+float2(x,y)*_MainTex_TexelSize.xy,rect)*weight;normalization+=weight;}
     }
     alpha/=max(.000001,normalization);
    }
    float4 color=_CssColor;
    // The adapter supplies the authored CSS color as an sRGB vector.
    alpha*=color.a;float4 shadow=float4(color.rgb*alpha,alpha);return Finish(original+shadow*(1-original.a));
   }
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentContrast
   #pragma target 3.5
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentContrast(effectOutput i):SV_Target {float4 c=Read(i.uv);float3 rgb=c.a>0?c.rgb/c.a:0;c.rgb=saturate((rgb-.5)*_Amount+.5)*c.a;return Finish(c);}
   ENDHLSL
  }
 }
}
