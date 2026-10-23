using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {

        private string NativeMobileText(string key)=>Visual_i18n.Scope["translations"][IsRussianUi?"ru":"en"][key].Text;
        private void NativeMobileControl(string id)
        {
            var s=NativeMobileScene;var player=s["player"];
            if(id=="pause"){NativeMobileHaptic(6);if(_tutorialStep>=0||s["pendingUpgrade"].Truth||s["pendingArtifact"].Truth)return;bool next=!s["paused"].Truth;s["paused"]=next;SetUserPaused(next);}
            else if(id=="lock"){NativeSetSphereMovementLocked(!player["sphereMovementLocked"].Truth);NativeMobileHaptic(6);}
            else if(id=="follow"){
                if(s["gameOver"].Truth)return;
                if(player["formationFollowActive"].Truth){NativeSetFormationFollow(false);NativeMobileFlash("FOLLOW отключён","FOLLOW OFF",.7,"#7f9bb8");NativeMobileHaptic(6);}
                else if(!NativeCanActivateFollow()||!NativeSetFormationFollow(true)){NativeMobileFlash("Подойдите к центру формации","Move closer to Formation Centroid",1,"#ffb84d");NativeMobileHaptic(22);}
                else {NativeMobileFlash("FORMATION FOLLOW","FORMATION FOLLOW",.8,"#63e6ff");NativeMobileHaptic(12);}
            }
            else if(id=="dash"){
                if(s["gameOver"].Truth||s["paused"].Truth||player["formationFollowActive"].Truth)return;NativeMobileClearKeys();var keys=s["keys"];if(_nativeMobileLastY<-.2)keys["w"]=true;if(_nativeMobileLastY>.2)keys["s"]=true;if(_nativeMobileLastX<-.2)keys["a"]=true;if(_nativeMobileLastX>.2)keys["d"]=true;NativeMobileHaptic(16);NativeActivateDash();NativeMobileClearKeys();
            }
            else if(id.StartsWith("sphere:",StringComparison.Ordinal)){NativeMobileHaptic(6);NativeSelectSphere(id.Substring(7));}
            else if(id.StartsWith("ability:",StringComparison.Ordinal)){
                if(s["gameOver"].Truth||s["paused"].Truth||s["pendingUpgrade"].Truth||s["pendingArtifact"].Truth)return;NativeMobileHaptic(12);NativeActivateHotkey(id.Substring(8).Split('-')[0]);
            }
            NativeMobileEventCompleted();
        }
        private static double NativeMobileCooldown(V player,string ability)
        {
            string field;
            switch(ability){case "blast":field="blastCooldown";break;case "shield":field="shieldCooldown";break;case "teleport":field="teleportCooldown";break;case "firetrail":field="fireTrailCooldown";break;case "minion":field="minionCooldown";break;case "lightning":field="lightningCooldown";break;case "timestop":field="timestopCooldown";break;case "darkritual":field="darkritualCooldown";break;default:return 0;}
            var value=player[field];return value.Truth?value.Number:0;
        }
        private static double NativeMobileMaxCooldown(string ability)
        {switch(ability){case "blast":case "minion":case "darkritual":return 30;case "shield":case "lightning":return 20;case "teleport":return 15;case "firetrail":return 25;case "timestop":return 40;default:return 1;}}
        private string NativeMobileDashLabel(V player,bool russian)
        {double cooldown=player["dashCooldown"].Number;return cooldown>0?NativeMobileText("dashOnCooldown").Replace("{sec}",VisualJs.NumberText(Math.Ceiling(cooldown))):russian?"Рывок":"Dash";}
        private void ReadNativeMobileState()
        {
            var s=NativeMobileScene;var player=s["player"];string lang=IsRussianUi?"ru":"en";bool follow=player["formationFollowActive"].Truth,locked=player["sphereMovementLocked"].Truth;
            var state=new NativeMobileControlsView.State {Right=IsLeftHanded};
            state.Controls.Add(new NativeMobileControlsView.Control {Id="pause",Icon="Pause"});
            state.Controls.Add(new NativeMobileControlsView.Control {Id="lock",Icon=locked?"Lock":"Unlock",Name=locked?(IsRussianUi?"ЗАБЛОКИРОВАНО":"LOCKED"):(IsRussianUi?"ПЕРЕМЕЩЕНИЕ":"MOVE")});
            state.Controls.Add(new NativeMobileControlsView.Control {Id="follow",Icon="Move",Name=follow?(IsRussianUi?"СЛЕД":"FOLLOWING"):"FOLLOW",Disabled=!follow&&!NativeCanActivateFollow()});
            foreach(var pair in (Dictionary<string,V>)s["activeKeyMap"].Raw){
                V ability=pair.Value;double cd=NativeMobileCooldown(player,ability.Text),max=NativeMobileMaxCooldown(ability.Text);
                var name=Visual_sphereProgression.Scope["getAbilityDisplayName"].Call(s,ability,lang);string label=name.Truth?name.Text:Visual_gameData.Scope["ABILITIES"][ability]["name"][lang].Text;
                state.Controls.Add(new NativeMobileControlsView.Control {Id="ability:"+pair.Key+"-"+ability.Text,Name=label,Detail=cd>0?VisualJs.NumberText(Math.Ceiling(cd))+"s":NativeMobileText("ready"),Cooldown=cd>0?(float)Math.Min(100,cd/max*100):0,Disabled=follow&&ability.Text=="teleport"});
            }
            if(follow)state.Strain=(IsRussianUi?"НАГРУЗКА СЕТИ":"NETWORK STRAIN")+" "+VisualJs.NumberText(Math.Floor((player["formationFollowStrain"].Truth?player["formationFollowStrain"].Number:0)+.5))+"%";
            state.Controls.Add(new NativeMobileControlsView.Control {Id="dash",Icon="Zap",Name=NativeMobileDashLabel(player,IsRussianUi),Disabled=follow});
            var definitions=Visual_gameData.Scope["SPHERE_TYPES"];var selected=definitions[s["selectedSphereType"].Truth?s["selectedSphereType"]:new V("standard")];state.Name=selected["name"][lang].Text;state.Description=selected["desc"][lang].Text;
            foreach(var pair in (Dictionary<string,V>)definitions.Raw)state.Controls.Add(new NativeMobileControlsView.Control {Id="sphere:"+pair.Key,Color=pair.Value["color"].Text});
            _nativeMobileDockState=state;
            _nativeJoystickState=_nativeMobileJoystickId.HasValue?new NativeMobileFeedbackView.State {Joystick=true,KnobX=34+(float)_nativeMobileDx,KnobY=34+(float)_nativeMobileDy,Active=_nativeMobileJoystickActive}:null;
            _nativePlacementState=_nativeMobilePlacementColor!=null?new NativeMobileFeedbackView.State {Id=VisualJs.NumberText(_nativeMobilePlacementAt),Color=_nativeMobilePlacementColor}:null;
            ReadNativeMobileNetworkState();
        }
        private void TickNativeMobileTransients()
        {
            if(!_nativeMobileMounted)return;double now=Time.unscaledTimeAsDouble;
            if(_nativeMobilePlacementColor!=null&&now-_nativeMobilePlacementAt>=.45){_nativeMobilePlacementColor=null;_sourceUi.Dirty=true;}
            if(_nativeMobileMemory!=null){if(now-_nativeMobileMemoryAt>=.9)_nativeMobileMemory=null;_sourceUi.Dirty=true;}
        }
        private void ReadNativeMobileNetworkState()
        {
            _nativeGhostNetworkState=_nativeFormationMemoryState=null;var canvas=_sourceUiCanvasRef["current"];if(canvas.Null||canvas.Undefined)return;var rect=canvas["getBoundingClientRect"].Call();var camera=NativeMobileScene["camera"];
            Vector2 ScreenPoint(double x,double y)=>new Vector2((float)(rect["left"].Number+(x-camera["x"].Number+canvas["width"].Number/2)*(rect["width"].Number/canvas["width"].Number)),(float)(rect["top"].Number+(y-camera["y"].Number+canvas["height"].Number/2)*(rect["height"].Number/canvas["height"].Number)));
            var ghost=_nativeMobileGhost;if(ghost!=null){
                double scale=rect["width"].Number/canvas["width"].Number;var center=ScreenPoint(ghost.X,ghost.Y);ColorUtility.TryParseHtmlString(Visual_gameData.Scope["SPHERE_TYPES"][NativeMobileScene["selectedSphereType"]]["color"].Text,out var color);
                var state=new NativeMobileNetworkView.State {Center=center,Valid=ghost.Valid,SphereColor=color,Radius=(float)(25*scale),RingWidth=(float)(2*scale),FormationWidth=(float)(2*scale),FontSize=(float)(11*scale),CaptionPosition=center-new Vector2(0,(float)(31*scale))};
                foreach(var link in ghost.Network["links"].Items){int a=(int)link["a"].Number,b=(int)link["b"].Number;if(a!=ghost.Index&&b!=ghost.Index)continue;var other=NativeMobileScene["spheres"][a==ghost.Index?b:a];if(!other.Truth)continue;state.Links.Add(new NativeMobileNetworkView.Link(center,ScreenPoint(other["pos"]["x"].Number,other["pos"]["y"].Number),(float)(2.2*scale)));}
                var points=new List<Vector2>();if(ghost.Formation.Truth)foreach(var index in ghost.Formation["nodes"].Items){var p=(int)index.Number==ghost.Index?VisualJs.Obj("x",ghost.X,"y",ghost.Y):NativeMobileScene["spheres"][index]["pos"];if(p.Truth)points.Add(ScreenPoint(p["x"].Number,p["y"].Number));}state.Points=points.ToArray();
                string type=ghost.Formation.Truth?ghost.Formation["type"].Text:null;
                string[] ids={"line","triangle","square","cluster","ring","lattice"},ru={"ЛИНИЯ","ТРЕУГОЛЬНИК","КВАДРАТ","КЛАСТЕР","КОЛЬЦО","РЕШЁТКА"},en={"LINE","TRIANGLE","SQUARE","CLUSTER","RING","LATTICE"};int i=Array.IndexOf(ids,type);
                state.Caption=!ghost.Valid?(IsRussianUi?"НЕДОСТУПНО":"INVALID"):!ghost.Formation.Truth?(IsRussianUi?"СЕТЬ":"NETWORK"):i>=0?(IsRussianUi?ru[i]:en[i]):(IsRussianUi?"ФРАКТАЛ":"FRACTAL");_nativeGhostNetworkState=state;
            }
            if(_nativeMobileMemory!=null)_nativeFormationMemoryState=new NativeMobileNetworkView.State {Memory=true,Closed=_nativeMobileMemoryClosed,Alpha=(float)Math.Min(.55,Math.Max(0,.9-(Time.unscaledTimeAsDouble-_nativeMobileMemoryAt))/.9),Points=_nativeMobileMemory.Select(p=>ScreenPoint(p.x,p.y)).ToArray()};
        }
    }
}
