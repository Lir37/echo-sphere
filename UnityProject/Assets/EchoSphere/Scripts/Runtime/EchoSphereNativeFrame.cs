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
        private static readonly Unity.Profiling.ProfilerMarker NativeWorldFrameMarker=new Unity.Profiling.ProfilerMarker("EchoSphere.World.BuildFrame");
        private static readonly UnityEngine.Color NativeWorldBackground=new UnityEngine.Color(7/255f,16/255f,29/255f,1);
        private static readonly UnityEngine.Color NativeWorldDarkBackground=new UnityEngine.Color(4/255f,9/255f,18/255f,1);
        private static readonly UnityEngine.Color NativeWorldBorder=new UnityEngine.Color(100/255f,180/255f,1,.24f);
        private static readonly UnityEngine.Color NativeWorldBorderShadow=new UnityEngine.Color(57/255f,216/255f,1,.22f);
        private void NativeWorldTranslate(double x,double y)=>_worldSurface.Translate(x,y);
        private UnityEngine.Color NativeFrameColor(string value)=>NativeMutationColor(value);
        internal struct NativeFrameInputs
        {
            internal double Time,CameraX,CameraY,PlayerX,PlayerY,WorldWidth,WorldHeight,Shake,MessageLife,ArrowX,ArrowY;
            internal int EnemyCount;internal bool HasMessage,HasArrow;internal string Message,Kind,Color;
        }
        internal NativeFrameInputs ProjectNativeFrameInputs()
        {
            var camera=NativeSourcePixels((UnityEngine.Vector2)(_camera.transform.position-_lastHunterScreenShakeOffset));var player=NativeSourcePixels(PlayerPosition);
            var frame=new NativeFrameInputs {Time=_runTime,CameraX=camera.x,CameraY=camera.y,PlayerX=player.x,PlayerY=player.y,WorldWidth=2400,WorldHeight=2400,Shake=UnityEngine.Mathf.Max(_hunterScreenShakeIntensity,_characterScreenShakeIntensity),EnemyCount=_enemies.Count(enemy=>enemy!=null&&enemy.IsAlive),HasMessage=_messageTimer>0,Message=_message??"null",MessageLife=_messageTimer,Kind=_sourceFlashKind??"null",Color=_sourceFlashColor??"null"};
            if(_ownedArtifacts.Contains("foresight_eye"))foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive&&enemy.IsBoss){var delta=(UnityEngine.Vector2)enemy.transform.position-PlayerPosition;if(delta.sqrMagnitude>0){var arrow=NativeSourcePixels(delta.normalized*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld);frame.HasArrow=true;frame.ArrowX=arrow.x;frame.ArrowY=arrow.y;}break;}return frame;
        }
        // Native frame orchestration. The original fireTrails segment list has
        // no producer in current source/native gameplay; provenance guards enforce
        // that invariant. Its native drawing primitive remains available.
        private void RenderNativeFrameContents(float backingWidth,float backingHeight)
        {
            using var frameSample=NativeWorldFrameMarker.Auto();
            PrepareNativeEntityFrame();
            var frame=ProjectNativeFrameInputs();double time=frame.Time;
            double w=backingWidth,h=backingHeight;
            _worldSurface.ResetTransform();_nativeWorldFeedback.Background(w,h,NativeWorldBackground,NativeWorldDarkBackground);
            DrawNativeViewVoidAmbient(w,h,time);
            double load=frame.EnemyCount+_nativeProjectileCount*.55+_nativeParticleCount*.28+_damageNumbers.Count*.42+_sourceLightningVisuals.Count*.75;
            int fxStride=load>220?3:load>150?2:1,damageStride=_damageNumbers.Count>36?2:1,lightningStride=_sourceLightningVisuals.Count>18?2:1;
            double shakeX=0,shakeY=0;_worldSurface.Save();
            if(frame.Shake>0){shakeX=(VisualJs.NextCosmeticValue()-.5)*frame.Shake*20;shakeY=(VisualJs.NextCosmeticValue()-.5)*frame.Shake*20;}
            NativeWorldTranslate(w/2-frame.CameraX+shakeX,h/2-frame.CameraY+shakeY);
            DrawNativeViewVoidField(frame.WorldWidth,frame.WorldHeight,-frame.WorldWidth/2,-frame.WorldHeight/2,frame.PlayerX,frame.PlayerY);DrawNativeViewRegionLayer();
            _worldSurface.Shadow=NativeWorldBorderShadow;_worldSurface.ShadowBlur=0;_worldSurface.SetDash(new[]{10f,8f});
            _nativeWorldFeedback.Rectangle(-frame.WorldWidth/2,-frame.WorldHeight/2,frame.WorldWidth,frame.WorldHeight,1.5,NativeWorldBorder);_worldSurface.SetDash(Array.Empty<float>());
            DrawNativeCharacterWorldIndicators();
            DrawNativeViewFormationMemory();DrawNativeViewSphereNetwork();
            DrawNativeExperienceLayer();
            DrawNativeHealthLayer();
            DrawNativeRuneLayer();
            DrawNativeChestLayer(false);
            DrawNativeChestLayer(true);
            foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)DrawNativeSphereFrame(sphere,time);
            DrawNativeProjectileLayer();
            DrawNativeDroneLayer();
            foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive)DrawNativeEnemyWorld(enemy);
            int orbitalIndex=-1;foreach(var component in _spheres){if(component==null||!component.gameObject.activeInHierarchy)continue;orbitalIndex++;if(component.Type!=EchoSphere.Core.SphereId.Orbital)continue;var sphere=ProjectNativeSphereVisual(component);double radius=NativeOrbitalRadiusScale(sphere)*NativeOrbitalNetworkMultiplier(_networkState,orbitalIndex);DrawNativeOrbitalSphere(sphere,time,NativeSphereScale(sphere),radius,true);}
            DrawNativePlayerShield();_nativeWorldPlayer.Draw(_worldSurface,NativePlayerState(),_nativeWorldSprites,_nativeWorldCreatures,ResolveNativePlayerArt);DrawNativeCharacterTargetIndicators();
            DrawNativeEnemyProjectileLayer();
            DrawNativeParticleLayer(fxStride);
            _worldSurface.SetOpacity(1);int damageIndex=0;
            foreach(var dn in _damageNumbers){int i=damageIndex++;if(i%damageStride==0&&dn.sourceSphereType!="orbital")DrawNativeDamageNumber(dn);}
            foreach(var dn in _damageNumbers)if(dn.sourceSphereType=="orbital")DrawNativeDamageNumber(dn);_worldSurface.SetOpacity(1);
            int lightningIndex=0;foreach(var lightning in _sourceLightningVisuals){if(lightningIndex++%lightningStride!=0)continue;double alpha=Math.Max(0,Math.Min(1,(double)lightning.life/.3));var from=NativeSourcePixels(lightning.from);var to=NativeSourcePixels(lightning.to);string branch=GetSphereBranch(EchoSphere.Core.SphereId.Chain);bool chain=lightning.sphereType=="chain"&&!string.IsNullOrEmpty(branch)&&_nativeWorldSphereFamilies.DrawChainLightning(_worldSurface,from.x,from.y,to.x,to.y,NativeMutationColor(NativeRunCatalog.Instance.GetSphere("chain").Color),branch,NativeSphereFinalIndex(EchoSphere.Core.SphereId.Chain),alpha,time);if(!chain)_nativeWorldEnergyBolts.Draw(_worldSurface,from.x,from.y,to.x,to.y,alpha,time);_worldSurface.SetOpacity(1);}
            _worldSurface.Restore();DrawNativeFrameOverlay(frame,w,h,shakeX,shakeY);
        }
        private void DrawNativeDamageNumber(SourceDamageNumber number)
        {
            bool crit=number.critical;string element=number.element,color=crit?"#ffd166":element=="fire"?"#ff5a36":element=="freeze"?"#69d6ff":element=="poison"?"#72f08e":"#e8f6ff";var point=NativeSourcePixels(number.position);
            _worldSurface.SetOpacity(Math.Min(1,(double)number.life/number.maxLife*1.5));_worldSurface.Shadow=NativeFrameColor(crit?"#b8860b":color);_worldSurface.ShadowBlur=0;
            _nativeWorldFeedback.Text(NativeCharacterCanvasText(VisualJs.NumberText(number.value)),point.x,point.y,NativeMutationColor(color),crit?20:14);
        }
        private void DrawNativeFrameOverlay(NativeFrameInputs frame,double w,double h,double shakeX,double shakeY)
        {
            if(frame.HasArrow){_worldSurface.Save();_worldSurface.Shadow=NativeFrameColor("#ff4d70");_worldSurface.ShadowBlur=0;_nativeWorldFeedback.BossArrow(w/2+frame.ArrowX*(w/2-40),h/2+frame.ArrowY*(h/2-40),Math.Atan2(frame.ArrowY,frame.ArrowX));_worldSurface.Restore();}
            // drawCharacterHud is empty in the authoritative source.
            if(!frame.HasMessage)return;
            if(frame.Kind=="boundary"){
                double progress=1-Math.Min(1,frame.MessageLife/1.35),fade=1-progress,radius=NativePlayerRadiusPixels;var color=NativeMutationColor(frame.Color);
                _worldSurface.Save();NativeWorldTranslate(w/2+frame.PlayerX-frame.CameraX+shakeX,h/2+frame.PlayerY-frame.CameraY+shakeY);_worldSurface.Blend="lighter";
                _worldSurface.SetOpacity(.46*fade);_nativeWorldFeedback.Boundary(radius,progress,color);_worldSurface.SetOpacity(.20*fade);_nativeWorldFeedback.BoundaryOuter(radius,progress,color);_worldSurface.SetOpacity(.50*fade);_nativeWorldFeedback.BoundaryShards(radius,progress,color);_worldSurface.Restore();
            }
            _worldSurface.Save();double maxWidth=Math.Min(w-44,224),fontSize=Math.Max(10,Math.Min(12,w*.032));var lines=new List<string>();string current="";
            foreach(string word in Regex.Split(frame.Message,@"\s+")){string next=current.Length>0?current+" "+word:word;if(_nativeWorldFeedback.Measure(next,(float)fontSize)<=maxWidth||current.Length==0)current=next;else{lines.Add(current);current=word;}}if(current.Length>0)lines.Add(current);
            if(lines.Count>2){string tail=string.Join(" ",lines.Skip(1));lines=new List<string>{lines[0],tail.Substring(0,Math.Min(34,tail.Length))+"…"};}
            double gap=fontSize+2,boxHeight=lines.Count*gap+9,widest=104;foreach(var line in lines)widest=Math.Max(widest,_nativeWorldFeedback.Measure(line,(float)fontSize));double boxWidth=Math.Min(w-22,Math.Max(122,widest+16)),bannerY=Math.Max(142,Math.Min(h*.205,178)),top=bannerY-boxHeight+3;
            _worldSurface.SetOpacity(Math.Min(1,Math.Max(0,frame.MessageLife/.82)));_nativeWorldFeedback.Rectangle(w/2-boxWidth/2,top,boxWidth,boxHeight,0,NativeMutationColor("rgba(2,7,13,.92)"));_nativeWorldFeedback.Rectangle(w/2-boxWidth/2,top,boxWidth,boxHeight,1,NativeMutationColor(frame.Color));
            for(int i=0;i<lines.Count;i++)_nativeWorldFeedback.Text(NativeCharacterCanvasText(lines[i]),w/2,top+12+i*gap,NativeMutationColor(frame.Color),(float)fontSize);_worldSurface.Restore();
        }
    }
}
