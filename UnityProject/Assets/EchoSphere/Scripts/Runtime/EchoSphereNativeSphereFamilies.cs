using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldSphereFamilies _nativeWorldSphereFamilies;
        private void DrawNativeOrbitalSphere(V sphere,V player,double time,double scale,double radiusScale,bool attackers)
        {
            bool disabled=sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth;if(attackers&&disabled)return;
            var state=new NativeWorldSphereFamilies.State {Family="orbital",X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Rotation=sphere["rotation"].Number,Time=time,Scale=scale,Disabled=disabled,Resonance=player["resonanceEventActive"].Truth};
            var input=new NativeWorldSphereFamilies.OrbitalInput {Identity=sphere.Raw,View=state,AuraTimer=sphere["auraTimer"].Number,RadiusScale=radiusScale};
            if(attackers){var branch=player["sphereBranches"]["orbital"];input.Blade=branch.Text=="orbital_blade";input.Element=NativeSphereElement(sphere,player);if(input.Element!=null)ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(input.Element).Color,out input.ElementColor);
                bool crown=false,dance=false;foreach(var artifact in player["artifacts"].Items)if(artifact.Text=="orbital_crown"){crown=true;break;}if(branch.Text=="orbital_dance")foreach(var evolution in player["evolutions"].Items)if(evolution.Text=="sphere:orbital:7:orbital_dance:2"){dance=true;break;}
                var tier=sphere["visualTier"];double level=System.Math.Max(1,System.Math.Min(7,tier.Truth?tier.Number:1));var counts=EchoSphere.Core.OrbitalVisualGeometryRules.RingCounts(level,(crown?1:0)+(dance?1:0));input.InnerAngles=new double[counts.Inner];input.OuterAngles=new double[counts.Outer];for(int i=0;i<input.InnerAngles.Length;i++)input.InnerAngles[i]=EchoSphere.Core.OrbitalVisualGeometryRules.ElementAngle(sphere["rotation"].Number,false,i,input.InnerAngles.Length);for(int i=0;i<input.OuterAngles.Length;i++)input.OuterAngles[i]=EchoSphere.Core.OrbitalVisualGeometryRules.ElementAngle(sphere["rotation"].Number,true,i,input.OuterAngles.Length);
                var enemies=new System.Collections.Generic.List<NativeWorldSphereFamilies.EnemyPoint>();foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive)continue;var point=NativeSourcePixels(enemy.transform.position);enemies.Add(new NativeWorldSphereFamilies.EnemyPoint {X=point.x,Y=point.y,Hp=enemy.CurrentHp});}input.Enemies=enemies.ToArray();
            }
            _nativeWorldSphereFamilies.DrawOrbital(_worldSurface,_nativeWorldStandardSphere,input,attackers);
        }
        private static string NativeSphereElement(V sphere,V player)
        {
            var branch=player["sphereBranches"][sphere["type"]];string element=NativeRunCatalog.Instance.GetSphereBranch(branch.Text)?.Element;
            if(sphere["type"].Text=="prism"&&branch.Text=="prism_spectrum"){double final=double.NaN;foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:prism:7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');final=VisualJs.Number(parts[parts.Length-1]).Number;break;}return final==1?"freeze":final==2?"poison":"fire";}
            return element;
        }
        private void DrawNativeSphereElements(V sphere,V player,double time,double scale)
        {
            string element=NativeSphereElement(sphere,player);if(element==null)return;ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(element).Color,out var color);double rotation=sphere["rotation"].Number;if(double.IsNaN(rotation)||double.IsInfinity(rotation))rotation=0;
            _nativeWorldSphereFamilies.DrawElements(_worldSurface,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,rotation,time,scale,sphere["type"].Text,element,color,sphere["alive"].Truth&&sphere["networkDisabledTimer"].Number<=0);
        }
        private void DrawNativeSphereModifiers(V sphere,V player,double time,double scale)
        {
            if(!player["sphereMods"].Truth)return;string branch=player["sphereBranches"][sphere["type"]].Text;string finalId=null;foreach(var marker in player["evolutions"].Items){if(!marker.Text.StartsWith("sphere:"+sphere["type"].Text+":7:",System.StringComparison.Ordinal))continue;var parts=marker.Text.Split(':');double index=VisualJs.Number(parts[parts.Length-1]).Number;if(index==0||index==1||index==2)finalId=branch+"_final_"+(index+1);break;}bool projectile=NativeRunCatalog.Instance.GetSphere(sphere["type"].Text).Projectile;
            string[] kinds={"multishot","pierce","ricochet","fire","freeze","poison"};var levels=new double[6];for(int i=0;i<6;i++){if(i<3&&!projectile)continue;double mod=player["sphereMods"][kinds[i]].Number;mod=double.IsNaN(mod)||double.IsInfinity(mod)?0:System.Math.Max(0,mod);var kind=(EchoSphere.Core.SphereAuthoredModifier)i;levels[i]=System.Math.Max(mod,EchoSphere.Core.SphereAuthoredModifierRules.GetLevel(branch,finalId,kind));}
            _nativeWorldSphereFamilies.DrawModifiers(_worldSurface,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,time,scale,sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,levels);
        }
        private void DrawNativeSphereFamily(string family,V sphere,V player,double time,double scale)
        {
            
            var state=new NativeWorldSphereFamilies.State {Family=family,X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Rotation=sphere["rotation"].Truth?sphere["rotation"].Number:0,Time=time,Scale=scale,Disabled=sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,Resonance=player["resonanceEventActive"].Truth};
            if(family=="prism"&&player["sphereBranches"]["prism"].Text=="prism_spectrum"){
                double final=double.NaN;foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:prism:7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');final=VisualJs.Number(parts[parts.Length-1]).Number;break;}
                string element=final==1?"freeze":final==2?"poison":"fire";state.HasPrismElement=ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetElement(element).Color,out state.PrismElement);
            }
            _nativeWorldSphereFamilies.DrawFamily(_worldSurface,_nativeWorldStandardSphere,state);
        }
    }
}
