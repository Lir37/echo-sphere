using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {

        private string NativeMobileText(string key)=>NativeText(key,IsRussianUi);
        private void NativeMobileControl(string id)
        {
            if(id=="pause"){if(_tutorialStep>=0||_levelUp||_stellaRewardPending)return;bool next=!_paused;SetUserPaused(next);RefreshNativeCommandModels();}
            else if(id=="lock"){NativeSetSphereMovementLocked(!_sphereMovementLocked);}
            else if(id=="follow"){
                if(_gameOver)return;
                if(_follow){NativeSetFormationFollow(false);NativeMobileFlash("FOLLOW отключён","FOLLOW OFF",.7,"#7f9bb8");}
                else if(!NativeCanActivateFollow()||!NativeSetFormationFollow(true)){NativeMobileFlash("Подойдите к центру формации","Move closer to Formation Centroid",1,"#ffb84d");}
                else {NativeMobileFlash("FORMATION FOLLOW","FORMATION FOLLOW",.8,"#63e6ff");}
            }
            else if(id=="dash"){
                if(_gameOver||_paused||_follow)return;NativeMobileClearKeys();NativeMobileSetDirection(_nativeMobileLastX,_nativeMobileLastY);NativeActivateDash();NativeMobileClearKeys();
            }
            else if(id.StartsWith("sphere:",StringComparison.Ordinal)){NativeSelectSphere(id.Substring(7));}
            else if(id.StartsWith("ability:",StringComparison.Ordinal)){
                if(_gameOver||_paused||_levelUp||_stellaRewardPending)return;NativeActivateHotkey(id.Substring(8).Split('-')[0]);
            }
            NativeMobileEventCompleted();
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        private static double NativeMobileCooldown(V player,string ability)
        {
            string field;
            switch(ability){case "blast":field="blastCooldown";break;case "shield":field="shieldCooldown";break;case "teleport":field="teleportCooldown";break;case "firetrail":field="fireTrailCooldown";break;case "minion":field="minionCooldown";break;case "lightning":field="lightningCooldown";break;case "timestop":field="timestopCooldown";break;case "darkritual":field="darkritualCooldown";break;default:return 0;}
            var value=player[field];return value.Truth?value.Number:0;
        }
#endif
        private static double NativeMobileMaxCooldown(string ability)
        {switch(ability){case "blast":case "minion":case "darkritual":return 30;case "shield":case "lightning":return 20;case "teleport":return 15;case "firetrail":return 25;case "timestop":return 40;default:return 1;}}
        private string NativeMobileDashLabel(double cooldown,bool russian)
        {return cooldown>0?NativeMobileText("dashOnCooldown").Replace("{sec}",NativePresentationMath.NumberText(Math.Ceiling(cooldown))):russian?"Рывок":"Dash";}
        private void ReadNativeMobileState()
        {
            bool follow=_follow,locked=_sphereMovementLocked;
            var state=new NativeMobileControlsView.State {Right=IsLeftHanded};
            state.Controls.Add(new NativeMobileControlsView.Control {Id="pause",Icon="Pause"});
            state.Controls.Add(new NativeMobileControlsView.Control {Id="lock",Icon=locked?"Lock":"Unlock",Name=IsRussianUi?"Перенос":"Move",Detail=locked?(IsRussianUi?"Стоп":"Locked"):(IsRussianUi?"Вкл":"On"),Active=locked,Tooltip=IsRussianUi?"Блокировка перемещения отдельных сфер":"Lock individual sphere movement"});
            var followControl=new NativeMobileControlsView.Control {Id="follow",Icon="Move",Name=IsRussianUi?"Следом":"Follow",Detail=follow?(NativePresentationMath.NumberText(Math.Floor((double)_formationStrain+.5))+"%"):!NativeCanActivateFollow()?"—":(IsRussianUi?"Выкл":"Off"),Tooltip=IsRussianUi?"Следование всей формации за персонажем":"Follow player with the whole formation",Active=follow,Disabled=!follow&&!NativeCanActivateFollow()};state.Controls.Add(followControl);
            foreach(var pair in _activeAbilityHotkeys){
                string ability=SourceAbilityId(pair.Key),key=pair.Value.ToString().Replace("Alpha","").ToLowerInvariant();double cd=GetActiveAbilityCooldown(pair.Key),max=NativeMobileMaxCooldown(ability);
                string label=NativeMobileAbilityName(pair.Key,IsRussianUi);
                state.Controls.Add(new NativeMobileControlsView.Control {Id="ability:"+key+"-"+ability,Name=label,Detail=cd>0?NativePresentationMath.NumberText(Math.Ceiling(cd))+"s":NativeMobileText("ready"),Cooldown=cd>0?(float)Math.Min(100,cd/max*100):0,Disabled=follow&&ability=="teleport"});
            }
            if(follow)followControl.Tooltip=(IsRussianUi?"НАГРУЗКА СЕТИ":"NETWORK STRAIN")+" "+NativePresentationMath.NumberText(Math.Floor((double)_formationStrain+.5))+"%";
            state.Controls.Add(new NativeMobileControlsView.Control {Id="dash",Icon="Zap",Name=IsRussianUi?"Рывок":"Dash",Detail=follow?(IsRussianUi?"Недоступно":"Unavailable"):_playerCore.DashCooldown>0?NativeMobileDashLabel(_playerCore.DashCooldown,IsRussianUi):NativeMobileText("ready"),Disabled=follow,Cooldown=0});
            var definitions=NativeRunCatalog.Instance.Spheres;var selected=NativeRunCatalog.Instance.GetSphere(_selectedSphereType.ToString().ToLowerInvariant());state.Name=selected.Name(IsRussianUi);state.Description=selected.Description(IsRussianUi);
            foreach(var sphere in definitions)state.Controls.Add(new NativeMobileControlsView.Control {Id="sphere:"+sphere.Id,Color=sphere.Color,Active=sphere.Id==selected.Id,Tooltip=sphere.Name(IsRussianUi)});
            _nativeMobileDockState=state;
            _nativeJoystickState=_nativeMobileJoystickId.HasValue?new NativeMobileFeedbackView.State {Joystick=true,KnobX=34+(float)_nativeMobileDx,KnobY=34+(float)_nativeMobileDy,Active=_nativeMobileJoystickActive}:null;
            _nativePlacementState=_nativeMobilePlacementColor!=null?new NativeMobileFeedbackView.State {Id=NativePresentationMath.NumberText(_nativeMobilePlacementAt),Color=_nativeMobilePlacementColor}:null;
            ReadNativeMobileNetworkState();
        }
        private string NativeMobileAbilityName(EchoSphere.Core.RunUpgradeKind kind,bool ru)=>NativeLiveAbilityName(kind,ru);
        private void TickNativeMobileTransients()
        {
            if(!_nativeMobileMounted)return;double now=Time.unscaledTimeAsDouble;
            if(_nativeMobilePlacementColor!=null&&now-_nativeMobilePlacementAt>=.45){_nativeMobilePlacementColor=null;_sourceUi.Dirty=true;}
            if(_nativeMobileMemory!=null){if(now-_nativeMobileMemoryAt>=.9)_nativeMobileMemory=null;_sourceUi.Dirty=true;}
        }
        private void ReadNativeMobileNetworkState()
        {
            _nativeGhostNetworkState=_nativeFormationMemoryState=null;if(_nativeRunField==null)return;var rect=_nativeRunField.worldBound;var size=_worldSurface.LogicalSize;double pixelWidth=size.x,pixelHeight=size.y;var camera=NativeSourcePixels((Vector2)(_camera.transform.position-_lastHunterScreenShakeOffset));
            Vector2 ScreenPoint(double x,double y)=>new Vector2((float)((double)rect.xMin+(x-camera.x+pixelWidth/2)*((double)rect.width/pixelWidth)),(float)((double)rect.yMin+(y-camera.y+pixelHeight/2)*((double)rect.height/pixelHeight)));
            var ghost=_nativeMobileGhost;if(ghost!=null){
                double scale=(double)rect.width/pixelWidth;var center=ScreenPoint(ghost.X,ghost.Y);ColorUtility.TryParseHtmlString(NativeRunCatalog.Instance.GetSphere(_selectedSphereType.ToString().ToLowerInvariant()).Color,out var color);
                var state=new NativeMobileNetworkView.State {Center=center,Valid=ghost.Valid,SphereColor=color,Radius=(float)(25*scale),RingWidth=(float)(2*scale),FormationWidth=(float)(2*scale),FontSize=(float)(11*scale),CaptionPosition=center-new Vector2(0,(float)(31*scale))};
                foreach(var link in ghost.Network.Links){int a=link.A,b=link.B;if(a!=ghost.Index&&b!=ghost.Index)continue;if(!NativeMobileSpherePoint(a==ghost.Index?b:a,out double ox,out double oy))continue;state.Links.Add(new NativeMobileNetworkView.Link(center,ScreenPoint(ox,oy),(float)(2.2*scale)));}
                var points=new List<Vector2>();if(ghost.Formation!=null)foreach(var index in ghost.Formation.NodeIndexes){if(index==ghost.Index)points.Add(ScreenPoint(ghost.X,ghost.Y));else if(NativeMobileSpherePoint(index,out double px,out double py))points.Add(ScreenPoint(px,py));}state.Points=points.ToArray();
                string type=ghost.Formation?.Type.ToString().ToLowerInvariant();
                string[] ids={"line","triangle","square","cluster","ring","lattice"},ru={"ЛИНИЯ","ТРЕУГОЛЬНИК","КВАДРАТ","КЛАСТЕР","КОЛЬЦО","РЕШЁТКА"},en={"LINE","TRIANGLE","SQUARE","CLUSTER","RING","LATTICE"};int i=Array.IndexOf(ids,type);
                state.Caption=!ghost.Valid?(IsRussianUi?"НЕДОСТУПНО":"INVALID"):ghost.Formation==null?(IsRussianUi?"СЕТЬ":"NETWORK"):i>=0?(IsRussianUi?ru[i]:en[i]):(IsRussianUi?"ФРАКТАЛ":"FRACTAL");_nativeGhostNetworkState=state;
            }
            if(_nativeMobileMemory!=null)_nativeFormationMemoryState=new NativeMobileNetworkView.State {Memory=true,Closed=_nativeMobileMemoryClosed,Alpha=(float)Math.Min(.55,Math.Max(0,.9-(Time.unscaledTimeAsDouble-_nativeMobileMemoryAt))/.9),Points=_nativeMobileMemory.Select(p=>ScreenPoint(p.x,p.y)).ToArray()};
        }
    }
}
