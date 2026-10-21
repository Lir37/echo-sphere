using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private V BuildNativeMobileComponent(V[] args)
        {
            _nativeMobilePresent=true;
            if(!_nativeMobileMounted){_nativeMobileMounted=true;_nativeMobileHandedness=IsLeftHanded;NativeMobileClearKeys();}
            else if(_nativeMobileHandedness!=IsLeftHanded){ResetNativeMobileGestures(false);_nativeMobileHandedness=IsLeftHanded;}
            ReadNativeMobileState();var children=new List<V>();
            V Box(string css,V style,params V[] content)=>VisualDom.Element("div",VisualJs.Obj("className",css,"style",style),content);
            double scale=Visual_interfaceScale.Scope["loadInterfaceScale"].Call().Number;
            V FeedbackStyle(double left,double top,int size)=>VisualJs.Obj("left",left,"top",top,"width",size,"height",size,"transform","scale("+VisualJs.NumberText(scale)+")","transformOrigin","center");
            if(_tutorialStep==1)children.Add(VisualDom.Element("div",VisualJs.Obj("data-tutorial-target","movement","className","absolute pointer-events-none z-30 rounded-full border border-[#63e6ff]/20","style",VisualJs.Obj("width",116,"height",116,"left",IsLeftHanded?"calc(25% - 58px)":"calc(75% - 58px)","bottom",44))));
            if(_nativeGhostNetworkState!=null)children.Add(Box("absolute inset-0 pointer-events-none z-10 overflow-visible",VisualJs.Obj()));
            if(_nativeFormationMemoryState!=null)children.Add(Box("absolute inset-0 pointer-events-none z-9 overflow-visible",VisualJs.Obj()));
            if(_nativePlacementState!=null)children.Add(VisualDom.Element("div",VisualJs.Obj("key",_nativeMobilePlacementAt,"className","absolute pointer-events-none","style",FeedbackStyle(_nativeMobilePlacement.x-26,_nativeMobilePlacement.y-26,52))));
            if(_nativeJoystickState!=null)children.Add(Box("absolute pointer-events-none",FeedbackStyle(_nativeMobileJoystickX-58,_nativeMobileJoystickY-58,116)));
            children.Add(Box("absolute bottom-4 "+(IsLeftHanded?"right-3 flex flex-col items-end":"left-3 flex flex-col items-start")+" gap-2 pointer-events-none",VisualJs.Obj("transform","scale("+VisualJs.NumberText(scale)+")","transformOrigin",IsLeftHanded?"right bottom":"left bottom")));
            return VisualDom.Element("div",VisualJs.Obj("className","es-mobile-controls absolute inset-0 z-20 overflow-hidden select-none","style",VisualJs.Obj("touchAction","none","userSelect","none","WebkitUserSelect","none")),VisualJs.Arr(children.ToArray()));
        }
        private string NativeMobileText(string key)=>Visual_i18n.Scope["translations"][IsRussianUi?"ru":"en"][key].Text;
        private void NativeMobileControl(string id)
        {
            var s=NativeMobileScene;var player=s["player"];
            if(id=="pause"){NativeMobileHaptic(6);if(_tutorialStep>=0||s["pendingUpgrade"].Truth||s["pendingArtifact"].Truth)return;bool next=!s["paused"].Truth;s["paused"]=next;SetUserPaused(next);}
            else if(id=="lock"){player["sphereMovementLocked"]=!player["sphereMovementLocked"].Truth;NativeMobileHaptic(6);}
            else if(id=="follow"){
                if(s["gameOver"].Truth)return;
                if(player["formationFollowActive"].Truth){NativeMobileAction("setFormationFollow",false);NativeMobileFlash("FOLLOW отключён","FOLLOW OFF",.7,"#7f9bb8");NativeMobileHaptic(6);}
                else if(!SourceUiBinding("engine.ts","canActivateFormationFollow").Call(s).Truth||!SourceUiBinding("engine.ts","setFormationFollow").Call(s,true).Truth){NativeMobileFlash("Подойдите к центру формации","Move closer to Formation Centroid",1,"#ffb84d");NativeMobileHaptic(22);}
                else {NativeMobileFlash("FORMATION FOLLOW","FORMATION FOLLOW",.8,"#63e6ff");NativeMobileHaptic(12);}
            }
            else if(id=="dash"){
                if(s["gameOver"].Truth||s["paused"].Truth||player["formationFollowActive"].Truth)return;NativeMobileClearKeys();var keys=s["keys"];if(_nativeMobileLastY<-.2)keys["w"]=true;if(_nativeMobileLastY>.2)keys["s"]=true;if(_nativeMobileLastX<-.2)keys["a"]=true;if(_nativeMobileLastX>.2)keys["d"]=true;NativeMobileHaptic(16);NativeMobileAction("activateDash");NativeMobileClearKeys();
            }
            else if(id.StartsWith("sphere:",StringComparison.Ordinal)){NativeMobileHaptic(6);NativeMobileAction("setSphereType",id.Substring(7));}
            else if(id.StartsWith("ability:",StringComparison.Ordinal)){
                if(s["gameOver"].Truth||s["paused"].Truth||s["pendingUpgrade"].Truth||s["pendingArtifact"].Truth)return;NativeMobileHaptic(12);NativeMobileAction("activateByKey",id.Substring(8).Split('-')[0]);
            }
            NativeMobileEventCompleted();
        }
        private void ReadNativeMobileState()
        {
            var s=NativeMobileScene;var player=s["player"];string lang=IsRussianUi?"ru":"en";bool follow=player["formationFollowActive"].Truth,locked=player["sphereMovementLocked"].Truth;
            var state=new NativeMobileControlsView.State {Right=IsLeftHanded};
            state.Controls.Add(new NativeMobileControlsView.Control {Id="pause",Icon="Pause"});
            state.Controls.Add(new NativeMobileControlsView.Control {Id="lock",Icon=locked?"Lock":"Unlock",Name=locked?(IsRussianUi?"ЗАБЛОКИРОВАНО":"LOCKED"):(IsRussianUi?"ПЕРЕМЕЩЕНИЕ":"MOVE")});
            state.Controls.Add(new NativeMobileControlsView.Control {Id="follow",Icon="Move",Name=follow?(IsRussianUi?"СЛЕД":"FOLLOWING"):"FOLLOW",Disabled=!follow&&!SourceUiBinding("engine.ts","canActivateFormationFollow").Call(s).Truth});
            foreach(var pair in (Dictionary<string,V>)s["activeKeyMap"].Raw){
                V ability=pair.Value;double cd=Visual_MobileControls.Scope["getAbilityCooldown"].Call(s,ability).Number,max=Visual_MobileControls.Scope["getAbilityMaxCooldown"].Call(ability).Number;
                var name=Visual_sphereProgression.Scope["getAbilityDisplayName"].Call(s,ability,lang);string label=name.Truth?name.Text:Visual_gameData.Scope["ABILITIES"][ability]["name"][lang].Text;
                state.Controls.Add(new NativeMobileControlsView.Control {Id="ability:"+pair.Key+"-"+ability.Text,Name=label,Detail=cd>0?VisualJs.NumberText(Math.Ceiling(cd))+"s":NativeMobileText("ready"),Cooldown=cd>0?(float)Math.Min(100,cd/max*100):0,Disabled=follow&&Visual_formationFollow.Scope["isFormationFollowMovementAbility"].Call(ability).Truth});
            }
            if(follow)state.Strain=(IsRussianUi?"НАГРУЗКА СЕТИ":"NETWORK STRAIN")+" "+VisualJs.MathObject["round"].Call(player["formationFollowStrain"].Truth?player["formationFollowStrain"]:new V(0)).Text+"%";
            state.Controls.Add(new NativeMobileControlsView.Control {Id="dash",Icon="Zap",Name=Visual_MobileControls.Scope["stDashLabel"].Call(s,lang,VisualJs.Fn(a=>NativeMobileText(a[0].Text))).Text,Disabled=follow});
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
