using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private const double NativePlayerRadiusPixels=16;
        private string NativeCharacterCanvasText(string text)=>_worldSurface.LocalizeCanvasText?NativeLocalizationCatalog.Instance.CanvasText(text,IsRussianUi):text;
#if UNITY_EDITOR
        private static string NativeCharacterWorldId(V scene)
        {var raw=scene["player"]["characterId"];return raw.Truth&&CharacterRules.TryParseSourceId(raw.Text,out _)?raw.Text:"spherist";}
#endif
        private NativeWorldCharacterIndicators.Point[] NativeAliveSpherePoints()
        {
            var points=new List<NativeWorldCharacterIndicators.Point>();foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){var point=NativeSourcePixels(sphere.transform.position);points.Add(new NativeWorldCharacterIndicators.Point {X=point.x,Y=point.y});}return points.ToArray();
        }
        private void DrawNativeCharacterWorldIndicators()
        {
            var point=NativeSourcePixels(PlayerPosition);
            switch(_sphereCharacterId){
                case SphereCharacterId.Engineer:_nativeWorldCharacterIndicators.DrawEngineer(_worldSurface,NativeAliveSpherePoints(),EngineerRelayRules.GetNetworkRangePixels(_characterMasteryLevel));break;
                case SphereCharacterId.Architect:_nativeWorldCharacterIndicators.DrawFormation(_worldSurface,NativeWorldArchitectFormation(),NativeAliveSpherePoints(),new NativeWorldCharacterIndicators.Point {X=point.x,Y=point.y});break;
                case SphereCharacterId.Berserker:_nativeWorldCharacterIndicators.DrawBerserker(_worldSurface,point.x,point.y,_playerCore.CurrentHp,_playerCore.MaxHp);break;
            }
        }
        private string NativeWorldArchitectFormation()
        {
            var kind=_networkState?.DominantFormation?.Type;return kind==SphereNetworkFormation.Line||kind==SphereNetworkFormation.Triangle||kind==SphereNetworkFormation.Square||kind==SphereNetworkFormation.Cluster?kind.Value.ToString().ToLowerInvariant():"none";
        }
        private void DrawNativeCharacterTargetIndicators()
        {
            if(_sphereCharacterId==SphereCharacterId.Hunter){
                var target=_hunterMarkTarget;if(target==null||!(target.CurrentHp>0)||!(_hunterMarkTimer>0))return;bool hunt=_hunterHuntTarget==target&&_hunterHuntTimer>0;var point=NativeSourcePixels(target.transform.position);_nativeWorldCharacterIndicators.DrawHunter(_worldSurface,point.x,point.y,target.SourceRadius,hunt,NativeCharacterCanvasText(hunt?"HUNT":"MARK"));
            }else if(_sphereCharacterId==SphereCharacterId.Alchemist){
                var reactions=new List<NativeWorldCharacterIndicators.Reaction>();foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive)continue;bool fire=enemy.SourceProjectionFireTimer>0,freeze=enemy.SourceProjectionFreezeTimer>0,poison=enemy.SourceProjectionPoisonTimer>0;if((fire?1:0)+(freeze?1:0)+(poison?1:0)<2)continue;string symbol=fire&&poison?"TP":freeze&&poison?"CP":fire&&freeze?"TS":"✦";var point=NativeSourcePixels(enemy.transform.position);reactions.Add(new NativeWorldCharacterIndicators.Reaction {X=point.x,Y=point.y,Radius=enemy.SourceRadius,Symbol=NativeCharacterCanvasText(symbol)});}_nativeWorldCharacterIndicators.DrawAlchemist(_worldSurface,reactions.ToArray());
            }
        }
        private void DrawNativePlayerShield()
        {
            var point=NativeSourcePixels(PlayerPosition);double count=_playerCore.ShieldCharges;
            _nativeWorldCharacterIndicators.DrawShield(_worldSurface,new NativeWorldCharacterIndicators.Shield {X=point.x,Y=point.y,PlayerRadius=NativePlayerRadiusPixels,Charges=count,Timer=System.Math.Max(0,(double)_shieldTimer),Pulse=_shieldVisualPulse,Time=_runTime,Spherist=_sphereCharacterId==SphereCharacterId.Spherist,Label=NativeCharacterCanvasText(NativePresentationMath.NumberText(count))},_nativeWorldSprites,ResolveNativePlayerArt);
        }
    }
}
