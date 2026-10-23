using System.Collections.Generic;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldCharacterIndicators _nativeWorldCharacterIndicators;
        private const double NativePlayerRadiusPixels=16;
        private string NativeCharacterCanvasText(string text)=>_worldSurface.LocalizeCanvasText?NativeLocalizationCatalog.Instance.CanvasText(text,IsRussianUi):text;
        private static string NativeCharacterWorldId(V scene)
        {var raw=scene["player"]["characterId"];return raw.Truth&&EchoSphere.Core.CharacterRules.TryParseSourceId(raw.Text,out _)?raw.Text:"spherist";}
        private static NativeWorldCharacterIndicators.Point[] NativeAliveSpherePoints(V scene)
        {
            var points=new List<NativeWorldCharacterIndicators.Point>();foreach(var sphere in scene["spheres"].Items)if(sphere["alive"].Truth)points.Add(new NativeWorldCharacterIndicators.Point {X=sphere["pos"]["x"].Number,Y=sphere["pos"]["y"].Number});return points.ToArray();
        }
        private void DrawNativeCharacterIndicator(string name,V scene)
        {
            var player=scene["player"];
            if(name=="drawCharacterWorldIndicators"){string id=NativeCharacterWorldId(scene);if(id=="engineer")DrawNativeCharacterIndicator("drawEngineerLinks",scene);else if(id=="architect")DrawNativeCharacterIndicator("drawArchitectFormation",scene);else if(id=="berserker")DrawNativeCharacterIndicator("drawBerserkerRange",scene);return;}
            if(name=="drawCharacterTargetIndicators"){string id=NativeCharacterWorldId(scene);if(id=="hunter")DrawNativeCharacterIndicator("drawHunterTarget",scene);else if(id=="alchemist")DrawNativeCharacterIndicator("drawAlchemistReactions",scene);return;}
            if(name=="drawEngineerLinks")_nativeWorldCharacterIndicators.DrawEngineer(_worldSurface,NativeAliveSpherePoints(scene),EchoSphere.Core.EngineerRelayRules.GetNetworkRangePixels((int)scene["player"]["characterMasteryLevel"].Number));
            else if(name=="drawArchitectFormation"){
                _nativeWorldCharacterIndicators.DrawFormation(_worldSurface,NativeCharacterFormationKind(scene),NativeAliveSpherePoints(scene),new NativeWorldCharacterIndicators.Point {X=player["pos"]["x"].Number,Y=player["pos"]["y"].Number});
            }else if(name=="drawHunterTarget"){
                var target=player["hunterMarkTarget"];if(!target.Truth||!(target["hp"].Number>0)||!(player["hunterMarkTimer"].Number>0))return;bool hunt=player["hunterHuntTarget"].Equals(target)&&player["hunterHuntTimer"].Number>0;_nativeWorldCharacterIndicators.DrawHunter(_worldSurface,target["pos"]["x"].Number,target["pos"]["y"].Number,target["radius"].Number,hunt,NativeCharacterCanvasText(hunt?"HUNT":"MARK"));
            }else if(name=="drawAlchemistReactions"){
                var reactions=new List<NativeWorldCharacterIndicators.Reaction>();foreach(var enemy in scene["enemies"].Items){if(enemy["hp"].Number<=0)continue;bool fire=enemy["fireTimer"].Number>0,freeze=enemy["freezeTimer"].Number>0,poison=enemy["poisonTimer"].Number>0;if((fire?1:0)+(freeze?1:0)+(poison?1:0)<2)continue;string symbol=fire&&poison?"TP":freeze&&poison?"CP":fire&&freeze?"TS":"✦";reactions.Add(new NativeWorldCharacterIndicators.Reaction {X=enemy["pos"]["x"].Number,Y=enemy["pos"]["y"].Number,Radius=enemy["radius"].Number,Symbol=NativeCharacterCanvasText(symbol)});}_nativeWorldCharacterIndicators.DrawAlchemist(_worldSurface,reactions.ToArray());
            }else if(name=="drawBerserkerRange")_nativeWorldCharacterIndicators.DrawBerserker(_worldSurface,player["pos"]["x"].Number,player["pos"]["y"].Number,player["hp"].Number,player["maxHp"].Number);
            else if(name=="drawPlayerShield"){
                var charges=player["shieldCharges"];var timer=player["shieldTimer"];var pulse=player["shieldVisualPulse"];double count=charges.Truth?charges.Number:0;_nativeWorldCharacterIndicators.DrawShield(_worldSurface,new NativeWorldCharacterIndicators.Shield {X=player["pos"]["x"].Number,Y=player["pos"]["y"].Number,PlayerRadius=NativePlayerRadiusPixels,Charges=count,Timer=System.Math.Max(0,timer.Truth?timer.Number:0),Pulse=pulse.Truth?pulse.Number:0,Time=_sourceRenderState["time"].Number,Spherist=NativeCharacterWorldId(scene)=="spherist",Label=NativeCharacterCanvasText(VisualJs.NumberText(count))},_nativeWorldSprites,ResolveNativePlayerArt);
            }
        }
    }
}
