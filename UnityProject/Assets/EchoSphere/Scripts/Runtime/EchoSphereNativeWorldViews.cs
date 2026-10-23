using System;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private V DrawNativeViewVoidAmbient(V arg1,V arg2,V arg3)
        {_nativeWorldAmbient.Draw(_worldSurface,(float)arg1.Number,(float)arg2.Number,arg3.Number);return default;}
        private V DrawNativeViewVoidField(V arg1,V arg2,V arg3,V arg4,V arg5,V arg6,V arg7)
        {_nativeWorldField.Draw(_worldSurface,(float)arg1.Number,(float)arg2.Number,(float)arg4.Number,(float)arg5.Number,(float)arg6.Number,(float)arg7.Number);return default;}
        private V DrawNativeViewRegionLayer(V arg1)
        {DrawNativeRegionWorld(arg1);return default;}
        private V DrawNativeViewFormationMemory(V arg1)
        {DrawNativeFormationMemory(arg1);return default;}
        private V DrawNativeViewSphereNetwork(V arg1,V arg2)
        {DrawNativeNetwork(arg1,arg2);return default;}
        private V DrawNativeViewModernXp(V arg1,V arg2,V arg3,V arg4,V arg5)
        {ColorUtility.TryParseHtmlString(arg4.Text,out var color);_nativeWorldPickups.DrawXp(_worldSurface,(float)arg1.Number,(float)arg2.Number,(float)arg3.Number,color,(float)arg5.Number);return default;}
        private V DrawNativeViewModernHealth(V arg1,V arg2,V arg3)
        {ColorUtility.TryParseHtmlString(arg3.Text,out var color);_nativeWorldPickups.DrawHealth(_worldSurface,(float)arg1.Number,(float)arg2.Number,color,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewRune(V arg1)
        {var rune=arg1;ColorUtility.TryParseHtmlString(Visual_runes.Scope["RUNE_DEFS"][rune["type"]]["color"].Text,out var color);_nativeWorldPickableViews.DrawRune(_worldSurface,rune["pos"]["x"].Number,rune["pos"]["y"].Number,rune["type"].Text,color,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewChest(V arg1)
        {var chest=arg1;_nativeWorldPickableViews.DrawChest(_worldSurface,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,false,null);return default;}
        private V DrawNativeViewStellaChest(V arg1)
        {var chest=arg1;string label="STELLA";if(_worldSurface.LocalizeCanvasText)label=NativeLocalizationCatalog.Instance.CanvasText(label,IsRussianUi);_nativeWorldPickableViews.DrawChest(_worldSurface,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,true,label);return default;}
        private V DrawNativeViewModernMinion(V arg1,V arg2,V arg3,V arg4,V arg5)
        {ColorUtility.TryParseHtmlString(arg5.Text,out var color);_nativeWorldMinions.Draw(_worldSurface,(float)arg1.Number,(float)arg2.Number,(float)arg3.Number,(float)arg4.Number,color,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewModernEnemy(V arg1,V arg2)
        {DrawNativeEnemyWorld(arg1,arg2);return default;}
        private V DrawNativeViewPlayer(V arg1)
        {_nativeWorldPlayer.Draw(_worldSurface,NativePlayerState(arg1),_nativeWorldSprites,_nativeWorldCreatures,ResolveNativePlayerArt);return default;}
        private V DrawNativeViewEnemyProjectile(V arg1,V arg2,V arg3,V arg4,V arg5)
        {bool sniper=arg5.Truth;ColorUtility.TryParseHtmlString(sniper?"#ffe066":"#ff6b55",out var color);_nativeWorldProjectiles.Draw(_worldSurface,(float)arg1.Number,(float)arg2.Number,(float)arg3.Number,arg4["x"].Number,arg4["y"].Number,sniper?1:0,color,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewBossProjectile(V arg1,V arg2,V arg3,V arg4,V arg5,V arg6)
        {ColorUtility.TryParseHtmlString(arg4.Text,out var color);string type=arg5.Text;_nativeWorldProjectiles.Draw(_worldSurface,(float)arg1.Number,(float)arg2.Number,(float)arg3.Number,arg6["x"].Number,arg6["y"].Number,type=="shooter"?2:type=="charger"?3:type=="summoner"?4:5,color,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewEnergyBolt(V arg1,V arg2,V arg3)
        {_nativeWorldEnergyBolts.Draw(_worldSurface,arg1["x"].Number,arg1["y"].Number,arg2["x"].Number,arg2["y"].Number,arg3.Number,_sourceRenderState["time"].Number);return default;}
        private V DrawNativeViewGroundShadow(V arg1,V arg2,V arg3)
        {_nativeWorldCreatures.DrawGroundShadow(_worldSurface,arg1.Number,arg2.Number);return default;}
        private V DrawNativeViewNetworkDisabledIndicator(V arg1,V arg2)
        {var timer=arg1["networkDisabledTimer"];_nativeWorldCreatures.DrawDisabledNetwork(_worldSurface,timer.Truth?timer.Number:0,Visual_eliteBalance.Scope["LINK_BREAKER_DISABLED_SECONDS"].Number,arg2.Number);return default;}
        private V DrawNativeViewStandardAttackVfx(V arg1,V arg2,V arg3)
        {var sphere=arg1;_nativeWorldStandardSphere.DrawAttack(_worldSurface,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,sphere["rotation"].Truth?sphere["rotation"].Number:0,arg2.Number,sphere["attackTimer"].Number);return default;}
        private V DrawNativeViewCharacterWorldIndicators(V scene)
        {DrawNativeCharacterIndicator("drawCharacterWorldIndicators",scene);return default;}
        private V DrawNativeViewCharacterTargetIndicators(V scene)
        {DrawNativeCharacterIndicator("drawCharacterTargetIndicators",scene);return default;}
        private V DrawNativeViewPlayerShield(V scene)
        {DrawNativeCharacterIndicator("drawPlayerShield",scene);return default;}
    }
}
