using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldSphereFamilies _nativeWorldSphereFamilies;
        private void DrawNativeSphereModifiers(V[] args)
        {
            var sphere=args[1];var player=args[2];if(!player["sphereMods"].Truth)return;var authored=Visual_sphereProgression.Scope["getAuthoredSphereModifierLevels"].Call(player,sphere["type"]);bool projectile=Visual_gameData.Scope["sphereUsesProjectileModifiers"].Call(sphere["type"]).Truth;
            string[] kinds={"multishot","pierce","ricochet","fire","freeze","poison"};var levels=new double[6];for(int i=0;i<6;i++){if(i<3&&!projectile)continue;double mod=player["sphereMods"][kinds[i]].Number;mod=double.IsNaN(mod)||double.IsInfinity(mod)?0:System.Math.Max(0,mod);var a=authored[kinds[i]];levels[i]=System.Math.Max(mod,a.Undefined||a.Null?0:a.Number);}
            var scale=VisualJs.Arg(args,4);_nativeWorldSphereFamilies.DrawModifiers(_sourceCanvas,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,args[3].Number,scale.Undefined?1:scale.Number,sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,levels);
        }
        private void DrawNativeSphereFamily(string family,V[] args)
        {
            var sphere=args[1];var player=args[2];var scale=VisualJs.Arg(args,4);
            var state=new NativeWorldSphereFamilies.State {Family=family,X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number,Rotation=sphere["rotation"].Truth?sphere["rotation"].Number:0,Time=args[3].Number,Scale=scale.Undefined?1:scale.Number,Disabled=sphere["networkDisabledTimer"].Number>0||!sphere["alive"].Truth,Resonance=player["resonanceEventActive"].Truth};
            if(family=="prism"&&player["sphereBranches"]["prism"].Text=="prism_spectrum"){
                double final=double.NaN;foreach(var evolution in player["evolutions"].Items){if(!evolution.Text.StartsWith("sphere:prism:7:",System.StringComparison.Ordinal))continue;var parts=evolution.Text.Split(':');final=VisualJs.Number(parts[parts.Length-1]).Number;break;}
                string element=final==1?"freeze":final==2?"poison":"fire";state.HasPrismElement=ColorUtility.TryParseHtmlString(Visual_sphereProgression.Scope["SPHERE_ELEMENT_META"][element]["color"].Text,out state.PrismElement);
            }
            _nativeWorldSphereFamilies.DrawFamily(_sourceCanvas,_nativeWorldStandardSphere,state);
        }
    }
}
