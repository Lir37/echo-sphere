using System;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private void DrawNativeSphereFrame(V scene,V sphere,double time)
        {
            var tier=sphere["visualTier"];double r=Math.Max(15,Math.Min(25,sphere["radius"].Number*.19+(tier.Truth?tier.Number:0)*.8)),scale=r/24;var ctx=VisualJs.Null;var args=new V[]{ctx,sphere,scene["player"],time,scale};string type=sphere["type"].Text;
            if(type=="standard"){
                if(!DrawNativeStandardAssembly(new V[]{ctx,scene,sphere,r,time}).Truth){
                    NativeFrameView("drawGroundShadow",r*1.05,r*.24,4);_worldSurface.Save();NativeWorldTranslate(sphere["pos"]["x"],sphere["pos"]["y"]);_nativeWorldFeedback.StandardFallback(r);_worldSurface.Restore();
                }
                DrawNativeSphereMutation(args);NativeFrameView("drawStandardAttackVfx",sphere,r,time);DrawNativeSphereElements(args);DrawNativeSphereModifiers(args);NativeFrameView("drawNetworkDisabledIndicator",sphere,time);return;
            }
            if(type=="orbital"){
                double radius=Visual_engineSpheres.Scope["getSphereRadius"].Call(scene,sphere).Number/Math.Max(1,sphere["radius"].Number);DrawNativeOrbitalSphere(new V[]{ctx,sphere,scene["player"],time,scale,scene["enemies"],radius},false);
            }else if(type=="sniper"||type=="shotgun"||type=="prism"||type=="void"){
                DrawNativeSphereMutation(args);DrawNativeSphereFamily(type,args);
            }else if(type=="chain"||type=="aura"||type=="gravity"||type=="pulse"){
                DrawNativeSphereFamily(type,args);DrawNativeSphereMutation(args);
            }else{
                NativeFrameView("drawNetworkDisabledIndicator",sphere,time);
                // The catalogue contains only the ten supported types. Invalid
                // type data failed the original definition/color lookup as well.
                throw new InvalidOperationException("Unknown sphere presentation type: "+type);
            }
            DrawNativeSphereElements(args);DrawNativeSphereModifiers(args);NativeFrameView("drawNetworkDisabledIndicator",sphere,time);
        }
    }
}
