using System;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Direct native presentation dispatch. No per-frame mutation of translated scopes.
        private V DrawNativeWorldView(string name,V[] args)
        {
            switch(name){
                case "drawVoidAmbient":{_nativeWorldAmbient.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,args[3].Number);return default;}
                case "drawVoidField":{_nativeWorldField.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[4].Number,(float)args[5].Number,(float)args[6].Number,(float)args[7].Number);return default;}
                case "drawRegionLayer":{DrawNativeRegionWorld(args[1]);return default;}
                case "drawFormationMemory":{DrawNativeFormationMemory(args[1]);return default;}
                case "drawSphereNetwork":{DrawNativeNetwork(args[1],args[2]);return default;}
                case "drawModernXp":{ColorUtility.TryParseHtmlString(args[4].Text,out var color);_nativeWorldPickups.DrawXp(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,color,(float)args[5].Number);return default;}
                case "drawModernHealth":{ColorUtility.TryParseHtmlString(args[3].Text,out var color);_nativeWorldPickups.DrawHealth(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,color,_sourceRenderState["time"].Number);return default;}
                case "drawRune":{var rune=args[1];ColorUtility.TryParseHtmlString(Visual_runes.Scope["RUNE_DEFS"][rune["type"]]["color"].Text,out var color);_nativeWorldPickableViews.DrawRune(_sourceCanvas,rune["pos"]["x"].Number,rune["pos"]["y"].Number,rune["type"].Text,color,_sourceRenderState["time"].Number);return default;}
                case "drawChest":{var chest=args[1];_nativeWorldPickableViews.DrawChest(_sourceCanvas,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,false,null);return default;}
                case "drawStellaChest":{var chest=args[1];string label="STELLA";if(_sourceCanvas.LocalizeCanvasText)label=Visual_canvasLocale.Scope["localizeCanvasText"].Call(label).Text;_nativeWorldPickableViews.DrawChest(_sourceCanvas,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,true,label);return default;}
                case "drawModernMinion":{ColorUtility.TryParseHtmlString(args[5].Text,out var color);_nativeWorldMinions.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,(float)args[4].Number,color,_sourceRenderState["time"].Number);return default;}
                case "drawModernEnemy":{DrawNativeEnemyWorld(args[1],args[2]);return default;}
                case "drawPlayer":{_nativeWorldPlayer.Draw(_sourceCanvas,NativePlayerState(args[1]),_nativeWorldSprites,_nativeWorldCreatures,ResolveNativePlayerArt);return default;}
                case "drawEnemyProjectile":{bool sniper=args[5].Truth;ColorUtility.TryParseHtmlString(sniper?"#ffe066":"#ff6b55",out var color);_nativeWorldProjectiles.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,args[4]["x"].Number,args[4]["y"].Number,sniper?1:0,color,_sourceRenderState["time"].Number);return default;}
                case "drawBossProjectile":{ColorUtility.TryParseHtmlString(args[4].Text,out var color);string type=args[5].Text;_nativeWorldProjectiles.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,args[6]["x"].Number,args[6]["y"].Number,type=="shooter"?2:type=="charger"?3:type=="summoner"?4:5,color,_sourceRenderState["time"].Number);return default;}
                case "drawEnergyBolt":{_nativeWorldEnergyBolts.Draw(_sourceCanvas,args[1]["x"].Number,args[1]["y"].Number,args[2]["x"].Number,args[2]["y"].Number,args[3].Number,_sourceRenderState["time"].Number);return default;}
                case "drawGroundShadow":{_nativeWorldCreatures.DrawGroundShadow(_sourceCanvas,args[1].Number,args[2].Number);return default;}
                case "drawNetworkDisabledIndicator":{var timer=args[1]["networkDisabledTimer"];_nativeWorldCreatures.DrawDisabledNetwork(_sourceCanvas,timer.Truth?timer.Number:0,Visual_eliteBalance.Scope["LINK_BREAKER_DISABLED_SECONDS"].Number,args[2].Number);return default;}
                case "drawStandardAttackVfx":{var sphere=args[1];_nativeWorldStandardSphere.DrawAttack(_sourceCanvas,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,sphere["rotation"].Truth?sphere["rotation"].Number:0,args[2].Number,sphere["attackTimer"].Number);return default;}
                case "drawCharacterWorldIndicators":case "drawCharacterTargetIndicators":case "drawPlayerShield":DrawNativeCharacterIndicator(name,args[1]);return default;
                default:throw new InvalidOperationException("Unmapped native world view: "+name);
            }
        }
    }
}
