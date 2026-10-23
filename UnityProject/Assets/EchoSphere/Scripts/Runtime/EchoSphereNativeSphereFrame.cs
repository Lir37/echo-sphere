using System;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private void DrawNativeSphereFrame(V scene,V sphere,double time)
        {
            var tier=sphere["visualTier"];double r=Math.Max(15,Math.Min(25,sphere["radius"].Number*.19+(tier.Truth?tier.Number:0)*.8)),scale=r/24;var player=scene["player"];string type=sphere["type"].Text;
            if(type=="standard"){
                if(!DrawNativeStandardAssembly(sphere,r,time)){
                    DrawNativeViewGroundShadow(r*1.05,r*.24,4);_worldSurface.Save();NativeWorldTranslate(sphere["pos"]["x"],sphere["pos"]["y"]);_nativeWorldFeedback.StandardFallback(r);_worldSurface.Restore();
                }
                DrawNativeSphereMutation(sphere,player,time,scale);DrawNativeViewStandardAttackVfx(sphere,r,time);DrawNativeSphereElements(sphere,player,time,scale);DrawNativeSphereModifiers(sphere,player,time,scale);DrawNativeViewNetworkDisabledIndicator(sphere,time);return;
            }
            if(type=="orbital"){
                double radius=NativePlannerSphereRadius(scene,sphere)/Math.Max(1,sphere["radius"].Number);DrawNativeOrbitalSphere(sphere,player,time,scale,radius,false);
            }else if(type=="sniper"||type=="shotgun"||type=="prism"||type=="void"){
                DrawNativeSphereMutation(sphere,player,time,scale);DrawNativeSphereFamily(type,sphere,player,time,scale);
            }else if(type=="chain"||type=="aura"||type=="gravity"||type=="pulse"){
                DrawNativeSphereFamily(type,sphere,player,time,scale);DrawNativeSphereMutation(sphere,player,time,scale);
            }else{
                DrawNativeViewNetworkDisabledIndicator(sphere,time);
                // The catalogue contains only the ten supported types. Invalid
                // type data failed the original definition/color lookup as well.
                throw new InvalidOperationException("Unknown sphere presentation type: "+type);
            }
            DrawNativeSphereElements(sphere,player,time,scale);DrawNativeSphereModifiers(sphere,player,time,scale);DrawNativeViewNetworkDisabledIndicator(sphere,time);
        }
    }
}
