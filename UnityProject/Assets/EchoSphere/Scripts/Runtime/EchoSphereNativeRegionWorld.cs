using System;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldRegion _nativeWorldRegion;
        internal static int NativeRegionPhaseIndex(double time)=>time<300?0:time<600?1:time<900?2:time<1500?3:time<1620?4:5;
        private void DrawNativeRegionWorld(V scene)
        {
            var region=scene["region"];if(!region.Truth)return;if(_nativeWorldRegion==null)_nativeWorldRegion=new NativeWorldRegion();
            var state=new NativeWorldRegion.State {Time=scene["time"].Number,Active=region["pocketId"].Text};
            // renderer.ts deliberately consumes REGION_POCKETS (the existing
            // alias), not a new region-specific visual rule chosen here.
            foreach(var pocket in NativeRunCatalog.Instance.WorldPockets){ColorUtility.TryParseHtmlString(pocket.Accent,out var accent);state.Pockets.Add(new NativeWorldRegion.Pocket {Id=pocket.Id,Center=new Vector2((float)pocket.X,(float)pocket.Y),Radius=(float)pocket.Radius,Accent=accent});}
            foreach(var poi in region["pois"].Items){if(!poi["alive"].Truth)continue;string type=poi["type"].Text,label;
                switch(type){case "resonance_cache":label="RESONANCE CACHE";break;case "breach_node":label="BREACH NODE";break;case "echo_relay":label="ECHO RELAY";break;case "lost_signal":label="LOST SIGNAL";break;case "elite_nest":label="ELITE NEST";break;case "rupture":label="RUPTURE";break;case "boss_trace":label="BOSS TRACE";break;default:label="undefined";break;}
                if(_worldSurface.LocalizeCanvasText)label=NativeLocalizationCatalog.Instance.CanvasText(label,IsRussianUi);
                ColorUtility.TryParseHtmlString(type=="elite_nest"||type=="rupture"?"#ff6b6b":type=="boss_trace"?"#ffb84d":"#55e6c1",out var color);
                state.Pois.Add(new NativeWorldRegion.Poi {Id=(int)poi["id"].Number,Position=new Vector2((float)poi["pos"]["x"].Number,(float)poi["pos"]["y"].Number),Color=color,Label=label});
            }
            _nativeWorldRegion.Draw(_worldSurface,state);

        }
    }
}
