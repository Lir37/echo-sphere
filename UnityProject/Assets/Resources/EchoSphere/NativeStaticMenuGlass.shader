Shader "EchoSphere/NativeStaticMenuGlass" {
 Properties {_MainTex("Source",2D)="black"{} _Overlay("Paint",2D)="black"{}}
 SubShader { Cull Off ZWrite Off ZTest Always Blend Off
 HLSLINCLUDE
 #include "UnityCG.cginc"
 sampler2D _MainTex,_Overlay;float4 _MainTex_TexelSize,_SourceRect,_LayerRect;
 float _Radius,_Density,_Sigma;float4 _Weights[128];int _WeightCount;
 float2 Mirror(float2 uv){return 1-abs(1-frac(uv*.5)*2);}
 float4 Crop(v2f_img i):SV_Target{return tex2D(_MainTex,_SourceRect.xy+i.uv*_SourceRect.zw);}
 float4 Layer(v2f_img i):SV_Target {
  float4 under=tex2D(_MainTex,i.uv);float2 uv=(i.uv-_LayerRect.xy)/_LayerRect.zw;
  if(any(uv<0)||any(uv>1))return under;
  float4 over=tex2D(_Overlay,uv);
  if(_Radius>0){float2 size=abs(_LayerRect.zw)*_SourceRect.zw;float2 p=uv*size;float2 q=abs(p-size*.5)-(size*.5-_Radius);float d=length(max(q,0))+min(max(q.x,q.y),0)-_Radius;over.a*=saturate(.5-d*_Density);}
  return float4(over.rgb*over.a+under.rgb*(1-over.a),over.a+under.a*(1-over.a));
 }
 float4 Blur(v2f_img i,float2 axis){float4 sum=tex2D(_MainTex,i.uv);float total=1;int radius=(int)ceil(4*_Sigma);
  [loop]for(int j=1;j<=radius;j++){float w=_WeightCount>radius?_Weights[j>>2][j&3]:exp(-j*(float)j/(2*_Sigma*_Sigma));float2 delta=axis*_MainTex_TexelSize.xy*j;sum+=(tex2D(_MainTex,Mirror(i.uv+delta))+tex2D(_MainTex,Mirror(i.uv-delta)))*w;total+=2*w;}
  return sum/total;
 }
 float4 Horizontal(v2f_img i):SV_Target{return Blur(i,float2(1,0));}
 float4 Vertical(v2f_img i):SV_Target{return Blur(i,float2(0,1));}
 ENDHLSL
 Pass { HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex vert_img
 #pragma fragment Crop
 ENDHLSL }
 Pass { HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex vert_img
 #pragma fragment Layer
 ENDHLSL }
 Pass { HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex vert_img
 #pragma fragment Horizontal
 ENDHLSL }
 Pass { HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex vert_img
 #pragma fragment Vertical
 ENDHLSL }
 }
}
