using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private void DrawNativeOrbitalSphere(NativeSphereVisual sphere,double time,double scale,double radiusScale,bool attackers)
        {
            bool disabled=sphere.DisabledTimer>0||!sphere.Alive;if(attackers&&disabled)return;
            var state=new NativeWorldSphereFamilies.State {Family="orbital",X=sphere.X,Y=sphere.Y,Rotation=sphere.Rotation,Time=time,Scale=scale,Disabled=disabled,Resonance=sphere.Resonance,AttackActive=sphere.AttackActive};
            var input=new NativeWorldSphereFamilies.OrbitalInput {Identity=sphere.Identity,View=state,AuraTimer=sphere.AuraTimer,RadiusScale=radiusScale};
            if(attackers){var branch=sphere.Branch;input.Blade=branch=="orbital_blade";input.Element=sphere.Element;if(input.Element!=null)ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(input.Element).Color,out input.ElementColor);
                bool crown=_ownedArtifacts.Contains("orbital_crown"),dance=branch=="orbital_dance"&&sphere.Final==2;
                double level=System.Math.Max(1,System.Math.Min(7,sphere.Tier!=0?sphere.Tier:1));var counts=EchoSphere.Core.OrbitalVisualGeometryRules.RingCounts(level,(crown?1:0)+(dance?1:0));input.InnerAngles=new double[counts.Inner];input.OuterAngles=new double[counts.Outer];for(int i=0;i<input.InnerAngles.Length;i++)input.InnerAngles[i]=EchoSphere.Core.OrbitalVisualGeometryRules.ElementAngle(sphere.Rotation,false,i,input.InnerAngles.Length);for(int i=0;i<input.OuterAngles.Length;i++)input.OuterAngles[i]=EchoSphere.Core.OrbitalVisualGeometryRules.ElementAngle(sphere.Rotation,true,i,input.OuterAngles.Length);
                var enemies=new System.Collections.Generic.List<NativeWorldSphereFamilies.EnemyPoint>();foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive)continue;var point=NativeSourcePixels(enemy.transform.position);enemies.Add(new NativeWorldSphereFamilies.EnemyPoint {X=point.x,Y=point.y,Hp=enemy.CurrentHp});}input.Enemies=enemies.ToArray();
            }
            _nativeWorldSphereFamilies.DrawOrbital(_worldSurface,_nativeWorldStandardSphere,input,attackers);
        }
        private void DrawNativeSphereElements(NativeSphereVisual sphere,double time,double scale)
        {
            string element=sphere.Element;if(element==null)return;ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(element).Color,out var color);double rotation=sphere.Rotation;if(double.IsNaN(rotation)||double.IsInfinity(rotation))rotation=0;
            _nativeWorldSphereFamilies.DrawElements(_worldSurface,sphere.X,sphere.Y,rotation,time,scale,sphere.Family,element,color,sphere.Alive&&sphere.DisabledTimer<=0);
        }
        private void DrawNativeSphereModifiers(NativeSphereVisual sphere,double time,double scale)
        {
            string branch=sphere.Branch,finalId=sphere.Final>=0&&sphere.Final<=2?branch+"_final_"+(sphere.Final+1):null;bool projectile=NativeRunCatalog.Instance.GetSphere(sphere.Family).Projectile;
            string[] kinds={"multishot","pierce","ricochet","fire","freeze","poison"};var levels=new double[6];for(int i=0;i<6;i++){if(i<3&&!projectile)continue;double mod=_sphereMods.TryGetValue(kinds[i],out var value)?System.Math.Max(0,value):0;levels[i]=System.Math.Max(mod,EchoSphere.Core.SphereAuthoredModifierRules.GetLevel(branch,finalId,(EchoSphere.Core.SphereAuthoredModifier)i));}
            _nativeWorldSphereFamilies.DrawModifiers(_worldSurface,sphere.X,sphere.Y,time,scale,sphere.DisabledTimer>0||!sphere.Alive,levels);
        }
        private void DrawNativeSphereFamily(NativeSphereVisual sphere,double time,double scale)
        {
            var state=new NativeWorldSphereFamilies.State {Family=sphere.Family,X=sphere.X,Y=sphere.Y,Rotation=double.IsNaN(sphere.Rotation)?0:sphere.Rotation,Time=time,Scale=scale,Disabled=sphere.DisabledTimer>0||!sphere.Alive,Resonance=sphere.Resonance,AttackActive=sphere.AttackActive};
            if(sphere.Type==EchoSphere.Core.SphereId.Prism&&sphere.Branch=="prism_spectrum")state.HasPrismElement=ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(sphere.Element).Color,out state.PrismElement);
            _nativeWorldSphereFamilies.DrawFamily(_worldSurface,_nativeWorldStandardSphere,state);
        }
    }
}
