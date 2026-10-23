using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldSphereFamilies _nativeWorldSphereFamilies;
        private void DrawNativeOrbitalSphere(V[] args,bool attackers)
        {
            var sphere=args[1];var player=args[2];var scale=VisualJs.Arg(args,4);var radiusScale=VisualJs.Arg(args,6);bool disabled=sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth;if(attackers&&disabled)return;
            var state=new NativeWorldSphereFamilies.State {Family="orbital",X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Rotation=sphere["rotation"].Number,Time=args[3].Number,Scale=scale.Undefined?1:scale.Number,Disabled=disabled,Resonance=player["resonanceEventActive"].Truth};
            var input=new NativeWorldSphereFamilies.OrbitalInput {Identity=sphere.Raw,View=state,AuraTimer=sphere["auraTimer"].Number,RadiusScale=radiusScale.Undefined?1:radiusScale.Number};
            if(attackers){var branch=player["sphereBranches"]["orbital"];input.Blade=branch.Text=="orbital_blade";input.Element=NativeSphereElement(sphere,player);if(input.Element!=null)ColorUtility.TryParseHtmlString(Visual_sphereProgression.Scope["SPHERE_ELEMENT_META"][input.Element]["color"].Text,out input.ElementColor);
                bool crown=false,dance=false;foreach(var artifact in player["artifacts"].Items)if(artifact.Text=="orbital_crown"){crown=true;break;}if(branch.Text=="orbital_dance")foreach(var evolution in player["evolutions"].Items)if(evolution.Text=="sphere:orbital:7:orbital_dance:2"){dance=true;break;}
                var tier=sphere["visualTier"];double level=System.Math.Max(1,System.Math.Min(7,tier.Truth?tier.Number:1));var counts=Visual_orbitalGeometry.Scope["getOrbitalRingCounts"].Call(level,(crown?1:0)+(dance?1:0));input.InnerAngles=new double[(int)counts["inner"].Number];input.OuterAngles=new double[(int)counts["outer"].Number];for(int i=0;i<input.InnerAngles.Length;i++)input.InnerAngles[i]=Visual_orbitalGeometry.Scope["orbitalElementAngle"].Call(sphere["rotation"],"inner",i,input.InnerAngles.Length).Number;for(int i=0;i<input.OuterAngles.Length;i++)input.OuterAngles[i]=Visual_orbitalGeometry.Scope["orbitalElementAngle"].Call(sphere["rotation"],"outer",i,input.OuterAngles.Length).Number;
                var enemies=new System.Collections.Generic.List<NativeWorldSphereFamilies.EnemyPoint>();foreach(var enemy in VisualJs.Arg(args,5).Items){if(!enemy.Truth)continue;enemies.Add(new NativeWorldSphereFamilies.EnemyPoint {X=enemy["pos"]["x"].Number,Y=enemy["pos"]["y"].Number,Hp=enemy["hp"].Number});}input.Enemies=enemies.ToArray();
            }
            _nativeWorldSphereFamilies.DrawOrbital(_worldSurface,_nativeWorldStandardSphere,input,attackers);
        }
        private static string NativeSphereElement(V sphere,V player)
        {
            var branch=player["sphereBranches"][sphere["type"]];var element=Visual_sphereProgression.Scope["getSphereElementForBranch"].Call(branch);
            if(sphere["type"].Text=="prism"&&branch.Text=="prism_spectrum"){double final=double.NaN;foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:prism:7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');final=VisualJs.Number(parts[parts.Length-1]).Number;break;}return final==1?"freeze":final==2?"poison":"fire";}
            return element.Truth?element.Text:null;
        }
        private void DrawNativeSphereElements(V[] args)
        {
            var sphere=args[1];string element=NativeSphereElement(sphere,args[2]);if(element==null)return;ColorUtility.TryParseHtmlString(Visual_sphereProgression.Scope["SPHERE_ELEMENT_META"][element]["color"].Text,out var color);var scale=VisualJs.Arg(args,4);double rotation=sphere["rotation"].Number;if(double.IsNaN(rotation)||double.IsInfinity(rotation))rotation=0;
            _nativeWorldSphereFamilies.DrawElements(_worldSurface,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,rotation,args[3].Number,scale.Undefined?1:scale.Number,sphere["type"].Text,element,color,sphere["alive"].Truth&&sphere["networkDisabledTimer"].Number<=0);
        }
        private void DrawNativeSphereModifiers(V[] args)
        {
            var sphere=args[1];var player=args[2];if(!player["sphereMods"].Truth)return;var authored=Visual_sphereProgression.Scope["getAuthoredSphereModifierLevels"].Call(player,sphere["type"]);bool projectile=Visual_gameData.Scope["sphereUsesProjectileModifiers"].Call(sphere["type"]).Truth;
            string[] kinds={"multishot","pierce","ricochet","fire","freeze","poison"};var levels=new double[6];for(int i=0;i<6;i++){if(i<3&&!projectile)continue;double mod=player["sphereMods"][kinds[i]].Number;mod=double.IsNaN(mod)||double.IsInfinity(mod)?0:System.Math.Max(0,mod);var a=authored[kinds[i]];levels[i]=System.Math.Max(mod,a.Undefined||a.Null?0:a.Number);}
            var scale=VisualJs.Arg(args,4);_nativeWorldSphereFamilies.DrawModifiers(_worldSurface,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,args[3].Number,scale.Undefined?1:scale.Number,sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,levels);
        }
        private void DrawNativeSphereFamily(string family,V[] args)
        {
            var sphere=args[1];var player=args[2];var scale=VisualJs.Arg(args,4);
            var state=new NativeWorldSphereFamilies.State {Family=family,X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Rotation=sphere["rotation"].Truth?sphere["rotation"].Number:0,Time=args[3].Number,Scale=scale.Undefined?1:scale.Number,Disabled=sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,Resonance=player["resonanceEventActive"].Truth};
            if(family=="prism"&&player["sphereBranches"]["prism"].Text=="prism_spectrum"){
                double final=double.NaN;foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:prism:7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');final=VisualJs.Number(parts[parts.Length-1]).Number;break;}
                string element=final==1?"freeze":final==2?"poison":"fire";state.HasPrismElement=ColorUtility.TryParseHtmlString(Visual_sphereProgression.Scope["SPHERE_ELEMENT_META"][element]["color"].Text,out state.PrismElement);
            }
            _nativeWorldSphereFamilies.DrawFamily(_worldSurface,_nativeWorldStandardSphere,state);
        }
    }
}
