using System;
using System.Globalization;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Candidate factory only. SourceGameView remains the production owner until
        // HUD, overlays, controls, effects and navigation pass their acceptance.
        public NativeHudView CreateNativeHudCandidate(VisualElement parent)
        {
            return new NativeHudView(parent,()=>{
                var swapped=Visual_App.Scope["swapNetworkFormationSlots"].Call(_sourceRenderState).Truth;if(swapped)ApplySourceUiWrites();return swapped;
            },ReadNativeHudTooltip);
        }
        public NativeHudView.State ReadNativeHudState()
        {
            if(_sourceRenderState.Undefined)throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativeHudState(_sourceRenderState,IsRussianUi);
        }
        // Explicit display inputs allow source parity checks without changing
        // runtime state, language preferences or the authoritative simulation.
        internal static NativeHudView.State ProjectNativeHudState(V scene,bool ru)
        {
            var player=scene["player"];var network=scene["nativeNetwork"];var lang=ru?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            string N(double value)=>value.ToString(CultureInfo.InvariantCulture);
            float Percent(double value)=>(float)Math.Max(0,Math.Min(100,value));
            NativeHudView.Formation Formation(V item)=>!item.Truth?null:new NativeHudView.Formation {Id=item["type"].Text,Name=Visual_App.Scope["formationDisplayName"].Call(item["type"],lang).Text,Inactive=item["active"].Raw is bool active&&!active};
            var enemies=new System.Collections.Generic.List<NativeHudRadarView.Enemy>();
            foreach(var enemy in scene["enemies"].Items)enemies.Add(new NativeHudRadarView.Enemy(enemy["pos"]["x"].Number,enemy["pos"]["y"].Number,enemy["hp"].Number>0,enemy["isBoss"].Truth,enemy["isElite"].Truth));
            var pois=new System.Collections.Generic.List<NativeHudRadarView.Poi>();var region=scene["region"];
            if(region.Truth)foreach(var poi in region["pois"].Items)pois.Add(new NativeHudRadarView.Poi(poi["pos"]["x"].Number,poi["pos"]["y"].Number,poi["alive"].Truth,poi["type"].Text));
            var state=new NativeHudView.State {
                Russian=ru,Level=T("level")+" "+player["level"].Text,Hp=N(Math.Ceiling(player["hp"].Number))+"/"+N(Math.Ceiling(player["maxHp"].Number)),
                XpPercent=Percent(player["xp"].Number/player["xpToNext"].Number*100),HpPercent=Percent(player["hp"].Number/player["maxHp"].Number*100),
                ResonancePercent=Percent(player["resonanceCharge"].Number),ResonanceReady=player["resonanceCharge"].Number>=100,Resonance=N(Math.Floor(player["resonanceCharge"].Number))+"/100",
                Spheres=T("spheres").ToUpperInvariant()+" <color=#cde8f6><b>"+scene["spheres"]["length"].Text+"/"+Visual_engineSpheres.Scope["getMaxSpheres"].Call(scene).Text+"</b></color>",
                Wave=T("wave").ToUpperInvariant()+" <color=#cde8f6><b>"+scene["wave"].Text+"</b></color>",Links=network["links"]["length"].Text,
                Timer=N(Math.Floor(scene["time"].Number/60)).PadLeft(2,'0')+":"+N(Math.Floor(scene["time"].Number%60)).PadLeft(2,'0'),
                Dominant=Formation(network["dominantFormation"]),Secondary=Formation(network["secondaryFormation"]),HasRegion=region.Truth,
                PlayerX=player["pos"]["x"].Number,PlayerY=player["pos"]["y"].Number,Enemies=enemies,Pois=pois,Boss=scene["bossActive"].Truth,BossLabel=T("bossWave").ToUpperInvariant()
            };
            if(region.Truth){
                var config=Visual_region.Scope["getRegionConfig"].Call(region["id"]);var pockets=config["pockets"];var pocket=pockets[0];foreach(var item in pockets.Items)if(item["id"].Text==region["pocketId"].Text){pocket=item;break;}
                var mode=region["mode"].Text=="stabilization"?(ru?"СТАБИЛИЗАЦИЯ":"STABILIZATION"):(ru?"БЕСКОНЕЧНЫЙ":"ENDLESS");
                state.Region=config["name"][lang].Text;state.Phase=region["phaseName"][lang].Text;state.Pocket=pocket["name"][lang].Text+" · "+mode;
            }
            return state;
        }
        private NativeHudView.Tooltip ReadNativeHudTooltip(string type)
        {
            var scene=_sourceRenderState;var network=scene["nativeNetwork"];var lang=IsRussianUi?"ru":"en";
            var secondary=network["secondaryFormation"]["type"].Text==type;
            var inactive=network["dominantFormation"]["type"].Text==type&&network["dominantFormation"]["active"].Raw is bool active&&!active;
            var lines=new System.Collections.Generic.List<NativeHudView.TooltipLine>();
            foreach(var line in Visual_App.Scope["getNetworkTooltipLines"].Call(type.ToUpperInvariant(),scene,lang,network).Items)lines.Add(new NativeHudView.TooltipLine(line["active"].Text,line["rest"].Text));
            return new NativeHudView.Tooltip {Title=Visual_App.Scope["formationDisplayName"].Call(type,lang).Text,Secondary=secondary,Inactive=inactive,Effectiveness=inactive?(IsRussianUi?"ВРЕМЕННО ОТКЛ.":"TEMP OFF"):secondary?"50%":"100%",Lines=lines};
        }
    }
}
