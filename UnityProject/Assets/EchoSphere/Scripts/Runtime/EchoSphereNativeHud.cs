using System;
using System.Globalization;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativeHudPilot=true;
        private NativeHudView _nativeHudIsland;
        private int _nativeHudBoundVersion=-1;
        private bool NativeHudRequested=>_nativeHudPilot&&!_mainMenu&&!_gameOver&&_playerCore!=null;
        public bool UsesNativeHud=>NativeHudRequested&&_nativeHudIsland!=null&&_nativeHudIsland.Ready;
        // Native production default; retained override for existing diagnostics.
        public void SetNativeHudPilot(bool enabled)
        {
            if(_nativeHudPilot==enabled)return;
            _nativeHudPilot=enabled;
            if(!enabled)DisposeNativeHudIsland();
            if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
        private void DisposeNativeHudIsland()
        {
            _nativeHudIsland?.Dispose();_nativeHudIsland=null;_nativeHudBoundVersion=-1;
        }

        private void PrepareNativeHudIsland(NativeRunUiAnchors anchors)
        {
            if(!NativeHudRequested){DisposeNativeHudIsland();return;}
            var leftBox=anchors.HudLeft.Element;var rightBox=anchors.HudRight.Element;
            if(leftBox==null||rightBox==null||leftBox.parent==null||leftBox.parent!=rightBox.parent){DisposeNativeHudIsland();return;}
            var parent=leftBox.parent;
            if(_nativeHudIsland==null||_nativeHudIsland.Root.parent!=parent){DisposeNativeHudIsland();_nativeHudIsland=CreateNativeHudCandidate(parent);}
            // The native island shares the original pane and paint-prefix order:
            // field/mobile controls below HUD, combo/modal/tutorial layers above.
            _nativeHudIsland.Root.PlaceInFront(rightBox);
            if(_nativeHudBoundVersion!=anchors.Version){_nativeHudIsland.Bind(ReadNativeHudState());_nativeHudBoundVersion=anchors.Version;}
            _nativeHudIsland.SetInterfaceScale((float)NativeInterfaceScale);
            var dpr=SourceDevicePixelRatio;var safe=Screen.safeArea;
            _nativeHudIsland.SetSafeArea(safe.xMin/dpr,(Screen.width-safe.xMax)/dpr,(Screen.height-safe.yMax)/dpr);
            var ready=_nativeHudIsland.Ready;_nativeHudIsland.Prepare(dpr);
            if(_nativeHudIsland.Ready){leftBox.style.visibility=rightBox.style.visibility=Visibility.Hidden;if(!ready)anchors.Invalidate?.Invoke();}
        }
        // Candidate factory only. SourceGameView remains the production owner until
        // HUD, overlays, controls, effects and navigation pass their acceptance.
        public NativeHudView CreateNativeHudCandidate(VisualElement parent)
        {
            return new NativeHudView(parent,NativeSwapFormationSlots,ReadNativeHudTooltip);
        }
        public NativeHudView.State ReadNativeHudState()=>ProjectNativeLiveHudState(IsRussianUi);
        private NativeHudView.State ProjectNativeLiveHudState(bool ru)
        {
            if(_playerCore==null)throw new InvalidOperationException("Native player required");
            string T(string key)=>NativeText(key,ru);string N(double value)=>value.ToString(CultureInfo.InvariantCulture);float Percent(double value)=>(float)Math.Max(0,Math.Min(100,value));
            NativeHudView.Formation Formation(EchoSphere.Core.SphereNetworkShape shape){if(shape==null)return null;string id=shape.Type.ToString().ToLowerInvariant();return new NativeHudView.Formation {Id=id,Name=NativeFormationName(id,ru),Inactive=!shape.Active};}
            var enemies=new System.Collections.Generic.List<NativeHudRadarView.Enemy>();bool boss=false;
            foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive){var point=NativeSourcePixels(enemy.transform.position);enemies.Add(new NativeHudRadarView.Enemy(point.x,point.y,enemy.CurrentHp>0,enemy.IsBoss,enemy.IsElite));boss|=enemy.IsBoss;}
            var pois=new System.Collections.Generic.List<NativeHudRadarView.Poi>();foreach(var poi in _nativeRegionPois){var point=NativeSourcePixels(poi.Position);pois.Add(new NativeHudRadarView.Poi(point.x,point.y,true,poi.Type));}
            int spheres=0;foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)spheres++;
            var network=_networkState;var position=NativeSourcePixels(PlayerPosition);double time=_runTime,maxSpheres=Math.Min(5+GetAbilityLevel(EchoSphere.Core.RunUpgradeKind.MaxSpheres)+GetShopUpgradeLevel("spheres"),8);
            var config=NativeRunCatalog.Instance.GetRegion(RegionSourceId(_selectedRegion));var pocket=config.Pockets[0];foreach(var item in config.Pockets)if(item.Id==_regionHudPocketId){pocket=item;break;}
            string mode=_regionEndlessMode?(ru?"БЕСКОНЕЧНЫЙ":"ENDLESS"):(ru?"СТАБИЛИЗАЦИЯ":"STABILIZATION");
            return new NativeHudView.State {
                Russian=ru,Level=T("level")+" "+N(_level),Hp=N(Math.Ceiling(_playerCore.CurrentHp))+"/"+N(Math.Ceiling(_playerCore.MaxHp)),
                XpPercent=Percent((double)_xp/_xpRequired*100),HpPercent=Percent((double)_playerCore.CurrentHp/_playerCore.MaxHp*100),
                ResonancePercent=Percent(_resonanceCharge),ResonanceReady=_resonanceCharge>=100,Resonance=N(Math.Floor(_resonanceCharge))+"/100",
                Spheres=T("spheres").ToUpperInvariant()+" <color=#cde8f6><b>"+N(spheres)+"/"+N(maxSpheres)+"</b></color>",Wave=T("wave").ToUpperInvariant()+" <color=#cde8f6><b>"+N(_wave)+"</b></color>",Links=N(network==null?0:network.Links.Count),
                Timer=N(Math.Floor(time/60)).PadLeft(2,'0')+":"+N(Math.Floor(time%60)).PadLeft(2,'0'),Dominant=Formation(network?.DominantFormation),Secondary=Formation(network?.SecondaryFormation),HasRegion=true,
                PlayerX=position.x,PlayerY=position.y,Enemies=enemies,Pois=pois,Boss=boss,BossLabel=T("bossWave").ToUpperInvariant(),Region=config.Name(ru),Phase=NativeRunCatalog.Instance.RegionPhases[NativeRegionPhaseIndex(time)].Name(ru),Pocket=pocket.Name(ru)+" · "+mode
            };
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        // Explicit display inputs allow source parity checks without changing
        // runtime state, language preferences or the authoritative simulation.
        internal static NativeHudView.State ProjectNativeHudState(V scene,bool ru)
        {
            var player=scene["player"];var network=scene["nativeNetwork"];var lang=ru?"ru":"en";
            string T(string key)=>NativeText(key,lang=="ru");
            string N(double value)=>value.ToString(CultureInfo.InvariantCulture);
            float Percent(double value)=>(float)Math.Max(0,Math.Min(100,value));
            NativeHudView.Formation Formation(V item)=>!item.Truth?null:new NativeHudView.Formation {Id=item["type"].Text,Name=NativeFormationName(item["type"].Text,ru),Inactive=item["active"].Raw is bool active&&!active};
            var enemies=new System.Collections.Generic.List<NativeHudRadarView.Enemy>();
            foreach(var enemy in scene["enemies"].Items)enemies.Add(new NativeHudRadarView.Enemy(enemy["pos"]["x"].Number,enemy["pos"]["y"].Number,enemy["hp"].Number>0,enemy["isBoss"].Truth,enemy["isElite"].Truth));
            var pois=new System.Collections.Generic.List<NativeHudRadarView.Poi>();var region=scene["region"];
            if(region.Truth)foreach(var poi in region["pois"].Items)pois.Add(new NativeHudRadarView.Poi(poi["pos"]["x"].Number,poi["pos"]["y"].Number,poi["alive"].Truth,poi["type"].Text));
            var state=new NativeHudView.State {
                Russian=ru,Level=T("level")+" "+player["level"].Text,Hp=N(Math.Ceiling(player["hp"].Number))+"/"+N(Math.Ceiling(player["maxHp"].Number)),
                XpPercent=Percent(player["xp"].Number/player["xpToNext"].Number*100),HpPercent=Percent(player["hp"].Number/player["maxHp"].Number*100),
                ResonancePercent=Percent(player["resonanceCharge"].Number),ResonanceReady=player["resonanceCharge"].Number>=100,Resonance=N(Math.Floor(player["resonanceCharge"].Number))+"/100",
                Spheres=T("spheres").ToUpperInvariant()+" <color=#cde8f6><b>"+scene["spheres"]["length"].Text+"/"+NativePresentationMath.NumberText(NativeMaximumSphereCount(scene))+"</b></color>",
                Wave=T("wave").ToUpperInvariant()+" <color=#cde8f6><b>"+scene["wave"].Text+"</b></color>",Links=network["links"]["length"].Text,
                Timer=N(Math.Floor(scene["time"].Number/60)).PadLeft(2,'0')+":"+N(Math.Floor(scene["time"].Number%60)).PadLeft(2,'0'),
                Dominant=Formation(network["dominantFormation"]),Secondary=Formation(network["secondaryFormation"]),HasRegion=region.Truth,
                PlayerX=player["pos"]["x"].Number,PlayerY=player["pos"]["y"].Number,Enemies=enemies,Pois=pois,Boss=scene["bossActive"].Truth,BossLabel=T("bossWave").ToUpperInvariant()
            };
            if(region.Truth){
                var config=NativeRunCatalog.Instance.GetRegion(region["id"].Text);var pocket=config.Pockets[0];foreach(var item in config.Pockets)if(item.Id==region["pocketId"].Text){pocket=item;break;}
                var mode=region["mode"].Text=="stabilization"?(ru?"СТАБИЛИЗАЦИЯ":"STABILIZATION"):(ru?"БЕСКОНЕЧНЫЙ":"ENDLESS");
                state.Region=config.Name(ru);state.Phase=region["phaseName"][lang].Text;state.Pocket=pocket.Name(ru)+" · "+mode;
            }
            return state;
        }
#endif
        private NativeHudView.Tooltip ReadNativeHudTooltip(string type)
        {
            var network=_networkState;
            var secondary=network?.SecondaryFormation?.Type.ToString().ToLowerInvariant()==type;
            var inactive=network?.DominantFormation?.Type.ToString().ToLowerInvariant()==type&&network.DominantFormation.Active==false;
            var activeTypes=new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)activeTypes.Add(sphere.Type.ToString().ToLowerInvariant());
            var lines=new System.Collections.Generic.List<NativeHudView.TooltipLine>();
            foreach(var line in NativeNetworkTooltipLines(type.ToUpperInvariant(),IsRussianUi,activeTypes,secondary,inactive))lines.Add(line);
            return new NativeHudView.Tooltip {Title=NativeFormationName(type,IsRussianUi),Secondary=secondary,Inactive=inactive,Effectiveness=inactive?(IsRussianUi?"ВРЕМЕННО ОТКЛ.":"TEMP OFF"):secondary?"50%":"100%",Lines=lines};
        }
    }
}
