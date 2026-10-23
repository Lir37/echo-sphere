using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldFeedback _nativeWorldFeedback;
        private static double _nativeRenderLastLog=-10;
        private static readonly UnityEngine.Color NativeWorldBackground=new UnityEngine.Color(7/255f,16/255f,29/255f,1);
        private static readonly UnityEngine.Color NativeWorldDarkBackground=new UnityEngine.Color(4/255f,9/255f,18/255f,1);
        private static readonly UnityEngine.Color NativeWorldBorder=new UnityEngine.Color(100/255f,180/255f,1,.24f);
        private static readonly UnityEngine.Color NativeWorldBorderShadow=new UnityEngine.Color(57/255f,216/255f,1,.22f);
        private void NativeWorldTranslate(V x,V y)=>_worldSurface.Translate(x.Number,y.Number);
        private UnityEngine.Color NativeFrameColor(V value)=>NativeMutationColor(value.Text);
        // Authored frame orchestration, replacing Visual_renderer.render. Existing
        // game helpers remain authoritative while their V data adapter is retired separately.
        private void RenderNativeFrameContents(float backingWidth,float backingHeight)
        {
            var s=_sourceRenderState;var ctx=VisualJs.Null;var player=s["player"];double time=s["time"].Number;
            double w=backingWidth,h=backingHeight,started=VisualJs.PerformanceObject["now"].Call().Number;
            _worldSurface.ResetTransform();_nativeWorldFeedback.Background(w,h,NativeWorldBackground,NativeWorldDarkBackground);
            DrawNativeViewVoidAmbient(w,h,time);
            double load=s["enemies"]["length"].Number+s["sphereProjectiles"]["length"].Number*.55+s["particles"]["length"].Number*.28+s["damageNumbers"]["length"].Number*.42+s["lightnings"]["length"].Number*.75;
            int fxStride=load>220?3:load>150?2:1,damageStride=s["damageNumbers"]["length"].Number>36?2:1,lightningStride=s["lightnings"]["length"].Number>18?2:1;
            double shakeX=0,shakeY=0;_worldSurface.Save();
            if(s["screenShake"].Number>0){shakeX=(VisualJs.MathObject["random"].Call().Number-.5)*s["screenShake"].Number*20;shakeY=(VisualJs.MathObject["random"].Call().Number-.5)*s["screenShake"].Number*20;}
            NativeWorldTranslate(w/2-s["camera"]["x"].Number+shakeX,h/2-s["camera"]["y"].Number+shakeY);
            DrawNativeViewVoidField(s["worldWidth"],s["worldHeight"],VisualJs.Null,-s["worldWidth"].Number/2,-s["worldHeight"].Number/2,player["pos"]["x"],player["pos"]["y"]);DrawNativeViewRegionLayer(s);
            _worldSurface.Shadow=NativeWorldBorderShadow;_worldSurface.ShadowBlur=0;_worldSurface.SetDash(new[]{10f,8f});
            _nativeWorldFeedback.Rectangle(-s["worldWidth"].Number/2,-s["worldHeight"].Number/2,s["worldWidth"].Number,s["worldHeight"].Number,1.5,NativeWorldBorder);_worldSurface.SetDash(Array.Empty<float>());
            DrawNativeViewCharacterWorldIndicators(s);
            foreach(var ft in s["fireTrails"].Items){double alpha=Math.Max(0,Math.Min(1,ft["life"].Number/ft["maxLife"].Number));_worldSurface.Save();NativeWorldTranslate(ft["pos"]["x"],ft["pos"]["y"]);_nativeWorldFeedback.FireTrail(ft["pos"]["x"].Number,time,alpha,_nativeWorldCreatures);_worldSurface.Restore();}
            DrawNativeViewFormationMemory(s);DrawNativeViewSphereNetwork(s);
            foreach(var orb in s["xpOrbs"].Items)DrawNativeViewModernXp(orb["pos"]["x"],orb["pos"]["y"],orb["radius"],"#63e6ff",orb["rotation"]);
            foreach(var hp in s["healthPacks"].Items)DrawNativeViewModernHealth(hp["pos"]["x"],hp["pos"]["y"],"#ff5c72");
            foreach(var rune in s["runes"].Items)if(rune["alive"].Truth)DrawNativeViewRune(rune);
            foreach(var chest in s["chests"].Items)if(chest["alive"].Truth)DrawNativeViewChest(chest);
            foreach(var chest in s["stellaChests"].Items)if(chest["alive"].Truth)DrawNativeViewStellaChest(chest);
            foreach(var sphere in s["spheres"].Items)DrawNativeSphereFrame(s,sphere,time);
            foreach(var p in s["sphereProjectiles"].Items)DrawNativeSphereProjectile(new V[]{ctx,p,player,time});
            foreach(var m in s["minions"].Items){string color=NativeRunCatalog.Instance.GetSphere(m["anchorType"].Text)?.Color;DrawNativeViewModernMinion(m["pos"]["x"],m["pos"]["y"],m["radius"],m["rotation"],new V(string.IsNullOrEmpty(color)?"#ffb84d":color));}
            foreach(var e in s["enemies"].Items)DrawNativeViewModernEnemy(e,player["pos"]);
            foreach(var sphere in s["spheres"].Items)if(sphere["alive"].Truth&&sphere["type"].Text=="orbital"){
                double tier=sphere["visualTier"].Truth?sphere["visualTier"].Number:0,scale=Math.Max(15,Math.Min(25,sphere["radius"].Number*.19+tier*.8))/24;
                var network=s["nativeNetwork"];var index=s["spheres"]["indexOf"].Call(sphere);var profile=Visual_network.Scope["getSphereNetworkProfile"].Call(network,index);
                double cluster=Visual_network.Scope["getFormationBonusMultiplier"].Call(network,"cluster",index).Number,ring=Visual_network.Scope["getFormationBonusMultiplier"].Call(network,"ring",index).Number;
                double multiplier=1+(cluster>0?.08*cluster:0)+(ring>0?.12*ring:0)+Math.Min(.20,profile["linkedNeighbours"].Number*.03),radius=Visual_engineSpheres.Scope["getSphereRadius"].Call(s,sphere).Number/Math.Max(1,sphere["radius"].Number)*multiplier;
                DrawNativeOrbitalSphere(new V[]{ctx,sphere,player,time,scale,s["enemies"],radius},true);
            }
            DrawNativeViewPlayerShield(s);DrawNativeViewPlayer(player);DrawNativeViewCharacterTargetIndicators(s);
            foreach(var e in s["enemies"].Items)foreach(var bp in e["bossProjectiles"].Items){string type=bp["visualType"].Text;if(type=="enemy_ranged"||type=="enemy_sniper")DrawNativeViewEnemyProjectile(bp["pos"]["x"],bp["pos"]["y"],bp["radius"],bp["vel"],type=="enemy_sniper");else DrawNativeViewBossProjectile(bp["pos"]["x"],bp["pos"]["y"],bp["radius"],e["color"],e["bossType"],bp["vel"]);}
            int particleIndex=0;foreach(var p in s["particles"].Items){if(particleIndex++%fxStride!=0)continue;double alpha=p["life"].Number/p["maxLife"].Number;_worldSurface.Shadow=NativeFrameColor(p["color"]);_worldSurface.ShadowBlur=0;var vx=p["vel"]["x"];var vy=p["vel"]["y"];_nativeWorldFeedback.Particle(p["pos"]["x"].Number,p["pos"]["y"].Number,p["size"].Number,alpha,vx.Truth?vx.Number:1,vy.Truth?vy.Number:0,NativeMutationColor(p["color"].Text));}
            _worldSurface.SetOpacity(1);int damageIndex=0;
            foreach(var dn in s["damageNumbers"].Items){int i=damageIndex++;if(i%damageStride==0&&dn["sourceSphereType"].Text!="orbital")DrawNativeDamageNumber(dn);}
            foreach(var dn in s["damageNumbers"].Items)if(dn["sourceSphereType"].Text=="orbital")DrawNativeDamageNumber(dn);_worldSurface.SetOpacity(1);
            int lightningIndex=0;foreach(var l in s["lightnings"].Items){if(lightningIndex++%lightningStride!=0)continue;double alpha=Math.Max(0,Math.Min(1,l["life"].Number/.3));if(!DrawNativeChainLightning(new V[]{ctx,l,player,alpha,time}).Truth)DrawNativeViewEnergyBolt(l["from"],l["to"],alpha);_worldSurface.SetOpacity(1);}
            _worldSurface.Restore();DrawNativeFrameOverlay(s,w,h,shakeX,shakeY);
            double cost=VisualJs.PerformanceObject["now"].Call().Number-started;
            if(cost>10&&time-_nativeRenderLastLog>=3){_nativeRenderLastLog=time;VisualJs.ConsoleObject["info"].Call("[ECHO_RENDER_PERF]",VisualJs.Obj("ms",VisualJs.Number(new V(cost)["toFixed"].Call(2)),"enemies",s["enemies"]["length"],"spheres",s["spheres"]["length"],"projectiles",s["sphereProjectiles"]["length"],"particles",s["particles"]["length"],"damageNumbers",s["damageNumbers"]["length"],"lightnings",s["lightnings"]["length"]));}
        }
        private void DrawNativeDamageNumber(V dn)
        {
            bool crit=dn["crit"].Truth;string element=dn["element"].Text,color=crit?"#ffd166":element=="fire"?"#ff5a36":element=="freeze"?"#69d6ff":element=="poison"?"#72f08e":"#e8f6ff";
            _worldSurface.SetOpacity(Math.Min(1,dn["life"].Number/dn["maxLife"].Number*1.5));_worldSurface.Shadow=NativeFrameColor(crit?"#b8860b":color);_worldSurface.ShadowBlur=0;
            _nativeWorldFeedback.Text(NativeCharacterCanvasText(dn["value"].Text),dn["pos"]["x"].Number,dn["pos"]["y"].Number,NativeMutationColor(color),crit?20:14);
        }
        private void DrawNativeFrameOverlay(V s,double w,double h,double shakeX,double shakeY)
        {
            var arrow=s["bossArrow"];if(arrow.Truth){_worldSurface.Save();_worldSurface.Shadow=NativeFrameColor("#ff4d70");_worldSurface.ShadowBlur=0;_nativeWorldFeedback.BossArrow(w/2+arrow["x"].Number*(w/2-40),h/2+arrow["y"].Number*(h/2-40),Math.Atan2(arrow["y"].Number,arrow["x"].Number));_worldSurface.Restore();}
            // drawCharacterHud is intentionally empty in the authoritative source.
            var flash=s["flashText"];if(!flash.Truth)return;
            if(flash["kind"].Text=="boundary"){
                double progress=1-Math.Min(1,flash["life"].Number/1.35),fade=1-progress,radius=NativePlayerRadiusPixels;var color=NativeMutationColor(flash["color"].Text);
                _worldSurface.Save();NativeWorldTranslate(w/2+s["player"]["pos"]["x"].Number-s["camera"]["x"].Number+shakeX,h/2+s["player"]["pos"]["y"].Number-s["camera"]["y"].Number+shakeY);_worldSurface.Blend="lighter";
                _worldSurface.SetOpacity(.46*fade);_nativeWorldFeedback.Boundary(radius,progress,color);_worldSurface.SetOpacity(.20*fade);_nativeWorldFeedback.BoundaryOuter(radius,progress,color);_worldSurface.SetOpacity(.50*fade);_nativeWorldFeedback.BoundaryShards(radius,progress,color);_worldSurface.Restore();
            }
            _worldSurface.Save();double maxWidth=Math.Min(w-44,224),fontSize=Math.Max(10,Math.Min(12,w*.032));var lines=new List<string>();string current="";
            foreach(string word in Regex.Split(flash["text"].Text,@"\s+")){string next=current.Length>0?current+" "+word:word;if(_nativeWorldFeedback.Measure(next,(float)fontSize)<=maxWidth||current.Length==0)current=next;else{lines.Add(current);current=word;}}if(current.Length>0)lines.Add(current);
            if(lines.Count>2){string tail=string.Join(" ",lines.Skip(1));lines=new List<string>{lines[0],tail.Substring(0,Math.Min(34,tail.Length))+"…"};}
            double gap=fontSize+2,boxHeight=lines.Count*gap+9,widest=104;foreach(var line in lines)widest=Math.Max(widest,_nativeWorldFeedback.Measure(line,(float)fontSize));double boxWidth=Math.Min(w-22,Math.Max(122,widest+16)),bannerY=Math.Max(142,Math.Min(h*.205,178)),top=bannerY-boxHeight+3;
            _worldSurface.SetOpacity(Math.Min(1,Math.Max(0,flash["life"].Number/.82)));_nativeWorldFeedback.Rectangle(w/2-boxWidth/2,top,boxWidth,boxHeight,0,NativeMutationColor("rgba(2,7,13,.92)"));_nativeWorldFeedback.Rectangle(w/2-boxWidth/2,top,boxWidth,boxHeight,1,NativeMutationColor(flash["color"].Text));
            for(int i=0;i<lines.Count;i++)_nativeWorldFeedback.Text(NativeCharacterCanvasText(lines[i]),w/2,top+12+i*gap,NativeMutationColor(flash["color"].Text),(float)fontSize);_worldSurface.Restore();
        }
    }
}
