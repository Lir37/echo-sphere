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
        private V DrawNativeViewSphereNetwork(V arg1)
        {DrawNativeNetwork(arg1);return default;}
        private V DrawNativeViewPlayer(V arg1)
        {_nativeWorldPlayer.Draw(_worldSurface,NativePlayerState(arg1),_nativeWorldSprites,_nativeWorldCreatures,ResolveNativePlayerArt);return default;}
        private V DrawNativeViewGroundShadow(V arg1,V arg2,V arg3)
        {_nativeWorldCreatures.DrawGroundShadow(_worldSurface,arg1.Number,arg2.Number);return default;}
        private V DrawNativeViewNetworkDisabledIndicator(V arg1,V arg2)
        {var timer=arg1["networkDisabledTimer"];_nativeWorldCreatures.DrawDisabledNetwork(_worldSurface,timer.Truth?timer.Number:0,2,arg2.Number);return default;}
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
