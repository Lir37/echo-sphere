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
            _nativeWorldSphereFamilies.DrawMutation(_worldSurface,new NativeWorldSphereFamilies.MutationView {X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Radius=24*(scale.Undefined?1:scale.Number),Time=args[3].Number,Rotation=rotation,FiniteRotation=finite,Final=NativeMutationFinal(player,sphere["type"].Text),AttackDelay=sphere["attackDelay"].Truth?sphere["attackDelay"].Number:1,AttackTimer=sphere["attackTimer"].Number,Branch=branch.Text,Color=NativeMutationColor(NativeRunCatalog.Instance.GetSphere(sphere["type"].Text).Color),Directed=NativeRunCatalog.Instance.GetSphere(sphere["type"].Text).Projectile,Emitter=(NativeRunCatalog.Instance.GetSphere(branch.Text.Split('_')[0])?.Projectile??false)});
        }
        private double NativeSphereFinalIndex(EchoSphere.Core.SphereId type)
        {
            string final=GetSphereFinal(type);if(string.IsNullOrEmpty(final))return -1;var choices=EchoSphere.Core.SphereEvolutionCatalog.GetFinals(GetSphereBranch(type));for(int index=0;index<choices.Count;index++)if(choices[index].Id==final)return index;throw new System.InvalidOperationException("Unknown native sphere final: "+final);
        }
        internal NativeWorldSphereFamilies.ProjectileView ProjectNativeProjectile(ProjectileAgent projectile)
        {
            var point=NativeSourcePixels(projectile.transform.position);var velocity=NativeSourcePixels(projectile.SourceProjectionDirection*projectile.SourceProjectionSpeed);var owner=projectile.SourceProjectionEvolutionOwner;ColorUtility.TryParseHtmlString("#"+ColorUtility.ToHtmlStringRGB(projectile.SourceProjectionImpactColor),out var color);
            return new NativeWorldSphereFamilies.ProjectileView {X=point.x,Y=point.y,Vx=velocity.x,Vy=velocity.y,Radius=projectile.SourceProjectionRadius,Color=color,Family=owner==null?null:owner.Type.ToString().ToLowerInvariant(),Branch=owner==null?null:GetSphereBranch(owner.Type),Final=owner==null?-1:NativeSphereFinalIndex(owner.Type)};
        }
        private void DrawNativeProjectileLayer(){foreach(var agent in _simulationAgents)if(agent is ProjectileAgent projectile&&projectile!=null&&projectile.gameObject.activeInHierarchy)_nativeWorldSphereFamilies.DrawProjectile(_worldSurface,ProjectNativeProjectile(projectile));}
#if UNITY_EDITOR
        internal static NativeWorldSphereFamilies.ProjectileView ProjectLegacyProjectile(V projectile,V player)
        {
            var type=projectile["sourceSphere"]["type"];ColorUtility.TryParseHtmlString(projectile["color"].Text,out var color);return new NativeWorldSphereFamilies.ProjectileView {X=projectile["pos"]["x"].Number,Y=projectile["pos"]["y"].Number,Vx=projectile["vel"]["x"].Number,Vy=projectile["vel"]["y"].Number,Radius=projectile["radius"].Number,Color=color,Family=type.Truth?type.Text:null,Branch=type.Truth?player["sphereBranches"][type].Text:null,Final=type.Truth?NativeMutationFinal(player,type.Text):-1};
        }
#endif

    }
}
