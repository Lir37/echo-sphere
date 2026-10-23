using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private static Color NativeMutationColor(string text){var c=VisualCssColor.Parse(text);return new Color(Mathf.Clamp01((float)c.R),Mathf.Clamp01((float)c.G),Mathf.Clamp01((float)c.B),Mathf.Clamp01((float)c.A));}
        private static double NativeMutationFinal(V player,string family)
        {
            foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:"+family+":7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');double n=VisualJs.Number(parts[parts.Length-1]).Number;return double.IsNaN(n)||double.IsInfinity(n)?-1:n;}return -1;
        }
        private void DrawNativeSphereMutation(V[] args)
        {
            var sphere=args[1];var player=args[2];var branch=player["sphereBranches"][sphere["type"]];if(!branch.Truth)return;var scale=VisualJs.Arg(args,4);double rotation=sphere["rotation"].Truth?sphere["rotation"].Number:0;double finite=sphere["rotation"].Raw is double&&!double.IsNaN(rotation)&&!double.IsInfinity(rotation)?rotation:0;
            _nativeWorldSphereFamilies.DrawMutation(_worldSurface,new NativeWorldSphereFamilies.MutationView {X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Radius=24*(scale.Undefined?1:scale.Number),Time=args[3].Number,Rotation=rotation,FiniteRotation=finite,Final=NativeMutationFinal(player,sphere["type"].Text),AttackDelay=sphere["attackDelay"].Truth?sphere["attackDelay"].Number:1,AttackTimer=sphere["attackTimer"].Number,Branch=branch.Text,Color=NativeMutationColor(Visual_gameData.Scope["SPHERE_TYPES"][sphere["type"]]["color"].Text),Directed=Visual_gameData.Scope["sphereUsesProjectileModifiers"].Call(sphere["type"]).Truth,Emitter=Visual_gameData.Scope["sphereUsesProjectileModifiers"].Call(branch.Text.Split('_')[0]).Truth});
        }
        private void DrawNativeSphereProjectile(V[] args)
        {
            var projectile=args[1];var player=args[2];var type=projectile["sourceSphere"]["type"];ColorUtility.TryParseHtmlString(projectile["color"].Text,out var color);_nativeWorldSphereFamilies.DrawProjectile(_worldSurface,new NativeWorldSphereFamilies.ProjectileView {X=projectile["pos"]["x"].Number,Y=projectile["pos"]["y"].Number,Vx=projectile["vel"]["x"].Number,Vy=projectile["vel"]["y"].Number,Radius=projectile["radius"].Number,Color=color,Family=type.Truth?type.Text:null,Branch=type.Truth?player["sphereBranches"][type].Text:null,Final=type.Truth?NativeMutationFinal(player,type.Text):-1});
        }
        private V DrawNativeChainLightning(V[] args)
        {
            var bolt=args[1];var player=args[2];var sphere=bolt["sourceSphere"];var branch=player["sphereBranches"]["chain"];if(!sphere.Truth||sphere["type"].Text!="chain"||!branch.Truth)return false;ColorUtility.TryParseHtmlString(Visual_gameData.Scope["SPHERE_TYPES"]["chain"]["color"].Text,out var color);return _nativeWorldSphereFamilies.DrawChainLightning(_worldSurface,bolt["from"]["x"].Number,bolt["from"]["y"].Number,bolt["to"]["x"].Number,bolt["to"]["y"].Number,color,branch.Text,NativeMutationFinal(player,"chain"),args[3].Number,args[4].Number);
        }
        private static double MutationArg(V[] args,int index,double fallback){var v=VisualJs.Arg(args,index);return v.Undefined?fallback:v.Number;}
        private void DrawNativeMutationPrimitive(string name,V[] a)
        {
            Color color;
            switch(name){
                case "fin":color=NativeMutationColor(a[5].Text);_nativeWorldSphereFamilies.MutationFin(_worldSurface,a[1].Number,a[2].Number,a[3].Number,a[4].Number,color,a[6].Number,MutationArg(a,7,.75));break;
                case "shard":color=NativeMutationColor(a[4].Text);_nativeWorldSphereFamilies.MutationShard(_worldSurface,a[1].Number,a[2].Number,a[3].Number,color,a[5].Number,MutationArg(a,6,.8));break;
                case "arc":color=NativeMutationColor(a[2].Text);_nativeWorldSphereFamilies.MutationArc(_worldSurface,a[1].Number,color,a[3].Number,a[4].Number,a[5].Number,MutationArg(a,6,.65));break;
                case "tether":color=NativeMutationColor(a[6].Text);_nativeWorldSphereFamilies.MutationTether(_worldSurface,a[1].Number,a[2].Number,a[3].Number,a[4].Number,a[5].Number,color,a[7].Number,MutationArg(a,8,.65),MutationArg(a,9,0));break;
                case "eye":color=NativeMutationColor(a[4].Text);_nativeWorldSphereFamilies.MutationEye(_worldSurface,a[1].Number,a[2].Number,a[3].Number,color,MutationArg(a,5,0));break;
                case "glow":color=NativeMutationColor(a[2].Text);_nativeWorldSphereFamilies.MutationGlow(_worldSurface,a[1].Number,color,MutationArg(a,3,.07));break;
                case "pulseRing":color=NativeMutationColor(a[2].Text);double r=a[1].Number,q=(a[3].Number*.62)%1;_nativeWorldSphereFamilies.MutationArc(_worldSurface,r*(.72+q*1.02),color,System.Math.Max(.7,r*.028),0,System.Math.PI*2,.44*(1-q)*MutationArg(a,4,1));break;
            }
        }
    }
}
