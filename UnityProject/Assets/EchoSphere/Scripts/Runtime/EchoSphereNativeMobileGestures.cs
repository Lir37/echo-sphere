using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private sealed class NativeMobilePointer
        {
            internal double X,Y;internal bool Moved,Joystick,Placing,Repositioned,Locked;internal V Sphere;
        }
        private sealed class NativeMobileGhost
        {
            internal int Index;internal double X,Y;internal bool Valid;internal V Network,Formation;
        }
        private readonly Dictionary<int,NativeMobilePointer> _nativeMobilePointers=new Dictionary<int,NativeMobilePointer>();
        private int? _nativeMobileJoystickId;
        private bool _nativeMobileJoystickActive;
        private double _nativeMobileJoystickX,_nativeMobileJoystickY,_nativeMobileDx,_nativeMobileDy;
        private double _nativeMobileLastX,_nativeMobileLastY=-1;
        private NativeMobileGhost _nativeMobileGhost;
        private Vector2[] _nativeMobileMemory;
        private bool _nativeMobileMemoryClosed,_nativeMobileMounted,_nativeMobileHandedness,_nativeMobilePresent;
        private double _nativeMobileMemoryAt,_nativeMobilePlacementAt;
        private string _nativeMobilePlacementColor;
        private Vector2 _nativeMobilePlacement;
        private VisualElement _nativeMobileGestureRoot;
        private V NativeMobileScene=>_sourceRenderState;
        private void NativeMobileHaptic(int duration)=>Visual_MobileControls.Scope["haptic"].Call(duration);
        private void NativeMobileAction(string name,params V[] args)=>SourceUiBinding("engine.ts",name).Call(new[]{NativeMobileScene}.Concat(args).ToArray());
        private void NativeMobileClearKeys(){var keys=NativeMobileScene["keys"];if(!(keys.Raw is Dictionary<string,V>))return;foreach(var key in new[]{"w","a","s","d","arrowup","arrowleft","arrowdown","arrowright"})keys[key]=false;}
        private void ResetNativeMobileGestures(bool unmount)
        {
            NativeMobileClearKeys();if(_nativeMobileGestureRoot!=null)foreach(int id in _nativeMobilePointers.Keys)_nativeMobileGestureRoot.ReleasePointer(id);
            _nativeMobilePointers.Clear();_nativeMobileJoystickId=null;_nativeMobileDx=_nativeMobileDy=0;_nativeMobileJoystickActive=false;
            if(unmount){_nativeMobileGhost=null;_nativeMobileMemory=null;_nativeMobilePlacementColor=null;_nativeMobileMounted=false;_nativeMobileLastX=0;_nativeMobileLastY=-1;}
        }
        private bool NativeMobileAllowed=>NativeMobileScene.Truth&&!NativeMobileScene["gameOver"].Truth&&(!NativeMobileScene["paused"].Truth||_tutorialStep==1||_tutorialStep==2);
        private bool NativeMobileWorld(double x,double y,out double wx,out double wy)
        {
            wx=wy=0;var canvas=_sourceUiCanvasRef["current"];if(canvas.Null||canvas.Undefined)return false;var rect=canvas["getBoundingClientRect"].Call();if(rect["width"].Number<=0||rect["height"].Number<=0)return false;
            wx=x-rect["left"].Number-rect["width"].Number/2+NativeMobileScene["camera"]["x"].Number;wy=y-rect["top"].Number-rect["height"].Number/2+NativeMobileScene["camera"]["y"].Number;return true;
        }
        private static double NativeMobileDistance(double x,double y)=>Math.Sqrt(x*x+y*y);
        private static double NativeMobileDistance(V pos,double x,double y)=>NativeMobileDistance(pos["x"].Number-x,pos["y"].Number-y);
        private bool NativeMobileCanPlace(double x,double y)=>Visual_spaceCollision.Scope["canPlaceSphere"].Call(NativeMobileScene,x,y).Truth;
        private bool NativeMobileCanMove(V sphere,double x,double y)=>Visual_spaceCollision.Scope["canRepositionSphere"].Call(NativeMobileScene,sphere,x,y).Truth;
        private int NativeMobileMaximum=>(int)Visual_engineSpheres.Scope["getMaxSpheres"].Call(NativeMobileScene).Number;
        private V NativeMobileNetwork()=>Visual_network.Scope["analyzeSphereNetwork"].Call(NativeMobileScene["spheres"]);
        private static V NativeMobileFormation(V network)=>Visual_networkPreview.Scope["getGhostSnapFormation"].Call(network);
        private void NativeMobileCaptureMemory(V network)
        {
            var formation=NativeMobileFormation(network);if(!formation.Truth)return;_nativeMobileMemory=formation["nodes"].Items.Select(i=>{var p=NativeMobileScene["spheres"][i]["pos"];return new Vector2((float)p["x"].Number,(float)p["y"].Number);}).ToArray();_nativeMobileMemoryClosed=formation["type"].Text!="line";_nativeMobileMemoryAt=Time.unscaledTimeAsDouble;
        }
        private void NativeMobileRememberChange(V oldNetwork)
        {
            var before=NativeMobileFormation(oldNetwork);var after=NativeMobileFormation(NativeMobileNetwork());if(before.Truth&&(!after.Truth||before["type"]!=after["type"]||string.Join(",",before["nodes"].Items.Select(i=>i.Text))!=string.Join(",",after["nodes"].Items.Select(i=>i.Text))))NativeMobileCaptureMemory(oldNetwork);
        }
        private void NativeMobileFlash(string ru,string en,double life,string color){_sourceRenderState["flashText"]=VisualJs.Obj("text",IsRussianUi?ru:en,"life",life,"color",color);}
        private void NativeMobileTap(double x,double y)
        {
            if(!NativeMobileAllowed||NativeMobileScene["pendingUpgrade"].Truth||NativeMobileScene["pendingArtifact"].Truth||!NativeMobileWorld(x,y,out double wx,out double wy))return;
            var spheres=NativeMobileScene["spheres"];V near=spheres.Items.FirstOrDefault(s=>s["alive"].Truth&&NativeMobileDistance(s["pos"],wx,wy)<26);
            if(_tutorialStep==2&&near.Truth)return;
            if(near.Truth){NativeMobileCaptureMemory(NativeMobileNetwork());int before=(int)spheres["length"].Number;NativeMobileAction("placeSphere",wx,wy);if(NativeMobileScene["spheres"]["length"].Number<before)NativeMobileHaptic(10);return;}
            if(spheres["length"].Number>=NativeMobileMaximum)return;
            if(!NativeMobileCanPlace(wx,wy)){NativeMobileFlash("Путь перекрыт","Path blocked",.8,"#ff4d5d");NativeMobileHaptic(35);return;}
            int count=(int)spheres["length"].Number;NativeMobileAction("placeSphere",wx,wy);
            if(NativeMobileScene["spheres"]["length"].Number>count){NativeMobileHaptic(8);_nativeMobilePlacement=new Vector2((float)x,(float)y);_nativeMobilePlacementAt=Time.unscaledTimeAsDouble;_nativeMobilePlacementColor=Visual_gameData.Scope["SPHERE_TYPES"][NativeMobileScene["selectedSphereType"]]["color"].Text;}
        }
        private NativeMobileGhost NativeMobilePreview(V sphere,double x,double y)
        {
            var spheres=NativeMobileScene["spheres"];int index=sphere.Truth?spheres.Items.ToList().IndexOf(sphere):-1;
            var preview=Visual_networkPreview.Scope["buildGhostSnapPreview"].Call(spheres,VisualJs.Obj("x",x,"y",y),default,index<0?default:new V(index));
            if(!sphere.Truth&&!preview.Truth)return null;
            var network=preview.Truth?preview["network"]:NativeMobileNetwork();
            return new NativeMobileGhost {Index=sphere.Truth?index:(int)preview["candidateIndex"].Number,X=x,Y=y,Valid=sphere.Truth?NativeMobileCanMove(sphere,x,y):NativeMobileCanPlace(x,y),Network=network,Formation=preview.Truth&&preview["formation"].Truth&&NativeMobileFormation(network).Truth?preview["formation"]:VisualJs.Null};
        }
        private void NativeMobileDown(PointerDownEvent e)
        {
            for(var target=e.target as VisualElement;target!=null&&target!=_nativeMobileGestureRoot;target=target.parent)if(target.ClassListContains("native-mobile-control"))return;
            if(!NativeMobileAllowed)return;_nativeMobileGestureRoot.CapturePointer(e.pointerId);
            double x=e.position.x,y=e.position.y;var p=new NativeMobilePointer {X=x,Y=y};
            if(NativeMobileWorld(x,y,out double wx,out double wy)){
                V sphere=NativeMobileScene["spheres"].Items.FirstOrDefault(s=>s["alive"].Truth&&NativeMobileDistance(s["pos"],wx,wy)<34);
                if(sphere.Truth&&_tutorialStep!=1&&_tutorialStep!=2){if(NativeMobileScene["player"]["sphereMovementLocked"].Truth)p.Locked=true;else p.Sphere=sphere;}
                else {double width=Screen.width/SourceDevicePixelRatio;bool zone=IsLeftHanded?x>=0&&x<width*.5:x>=width*.5&&x<width;p.Joystick=_tutorialStep!=2&&!_nativeMobileJoystickId.HasValue&&zone;p.Placing=(_tutorialStep==2||_tutorialStep!=1&&!p.Joystick)&&NativeMobileScene["spheres"]["length"].Number<NativeMobileMaximum;}
            }
            _nativeMobilePointers[e.pointerId]=p;if(p.Joystick){_nativeMobileJoystickId=e.pointerId;_nativeMobileJoystickX=x;_nativeMobileJoystickY=y;_nativeMobileDx=_nativeMobileDy=0;_nativeMobileJoystickActive=false;}NativeMobileEventCompleted();
        }
        private void NativeMobileMove(PointerMoveEvent e)
        {
            if(!_nativeMobilePointers.TryGetValue(e.pointerId,out var p))return;double dx=e.position.x-p.X,dy=e.position.y-p.Y,distance=NativeMobileDistance(dx,dy);if(p.Locked)return;
            if(p.Sphere.Truth||p.Placing){if(distance>12){p.Moved=true;if(NativeMobileWorld(e.position.x,e.position.y,out double x,out double y)){var preview=NativeMobilePreview(p.Sphere,x,y);if(preview!=null){p.Repositioned=preview.Valid;_nativeMobileGhost=preview;}}}NativeMobileEventCompleted();return;}
            if(distance>12)p.Moved=true;
            if(_nativeMobileJoystickId==e.pointerId){_nativeMobileJoystickActive=distance>12;double length=distance==0?1:distance,scale=Math.Min(1,Math.Max(0,length/58));_nativeMobileDx=dx/length*58*scale;_nativeMobileDy=dy/length*58*scale;
                if(distance<12)NativeMobileClearKeys();else{_nativeMobileLastX=dx/distance;_nativeMobileLastY=dy/distance;var keys=NativeMobileScene["keys"];keys["w"]=_nativeMobileLastY<-.2;keys["s"]=_nativeMobileLastY>.2;keys["a"]=_nativeMobileLastX<-.2;keys["d"]=_nativeMobileLastX>.2;}}
            NativeMobileEventCompleted();
        }
        private void NativeMobileEnd(int id,double x,double y)
        {
            _nativeMobilePointers.TryGetValue(id,out var p);_nativeMobilePointers.Remove(id);
            if(p!=null&&p.Sphere.Truth){
                if(!p.Moved){NativeMobileAction("placeSphere",p.Sphere["pos"]["x"],p.Sphere["pos"]["y"]);_nativeMobileGhost=null;NativeMobileHaptic(10);}
                else if(p.Repositioned){var old=NativeMobileNetwork();if(NativeMobileWorld(x,y,out double wx,out double wy)&&NativeMobileCanMove(p.Sphere,wx,wy)){NativeMobileAction("repositionSphere",p.Sphere,wx,wy);NativeMobileRememberChange(old);NativeMobileFlash("Сфера перемещена","Sphere moved",.8,Visual_gameData.Scope["SPHERE_TYPES"][p.Sphere["type"]]["color"].Text);}_nativeMobileGhost=null;NativeMobileHaptic(12);}
                else {NativeMobileFlash("Недоступная позиция","Invalid position",.65,"#ff4d5d");_nativeMobileGhost=null;NativeMobileHaptic(22);}return;
            }
            if(p!=null&&p.Placing){
                if(!p.Moved){NativeMobileTap(x,y);_nativeMobileGhost=null;return;}
                if(p.Repositioned){if(NativeMobileWorld(x,y,out double wx,out double wy)&&NativeMobileCanPlace(wx,wy)){var old=NativeMobileNetwork();int before=(int)NativeMobileScene["spheres"]["length"].Number;NativeMobileAction("placeSphere",wx,wy);if(NativeMobileScene["spheres"]["length"].Number>before){NativeMobileRememberChange(old);NativeMobileHaptic(8);}}_nativeMobileGhost=null;return;}
                NativeMobileFlash("Недоступная позиция","Invalid position",.65,"#ff4d5d");_nativeMobileGhost=null;NativeMobileHaptic(22);return;
            }
            if(_nativeMobileJoystickId==id){_nativeMobileJoystickId=null;NativeMobileClearKeys();if(p!=null&&!p.Moved)NativeMobileTap(x,y);}else if(p!=null&&!p.Moved)NativeMobileTap(x,y);
        }
        private void NativeMobileUp(PointerUpEvent e){NativeMobileEnd(e.pointerId,e.position.x,e.position.y);_nativeMobileGestureRoot.ReleasePointer(e.pointerId);NativeMobileEventCompleted();}
        private void NativeMobileCancel(PointerCancelEvent e){NativeMobileEnd(e.pointerId,e.position.x,e.position.y);_nativeMobileGestureRoot.ReleasePointer(e.pointerId);NativeMobileEventCompleted();}
        private void NativeMobileEventCompleted(){ApplySourceUiWrites();if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void PrepareNativeMobileGestures(VisualCssNode root)
        {
            VisualCssNode mobile=null;void Find(VisualCssNode n){if(n.HasClass("es-mobile-controls"))mobile=n;foreach(var c in n.Children)Find(c);}Find(root);
            var element=(mobile?.Native as SourceUiFrame)?.Content;if(element==_nativeMobileGestureRoot)return;DetachNativeMobileGestures();_nativeMobileGestureRoot=element;if(element==null)return;
            element.RegisterCallback<PointerDownEvent>(NativeMobileDown);element.RegisterCallback<PointerMoveEvent>(NativeMobileMove);element.RegisterCallback<PointerUpEvent>(NativeMobileUp);element.RegisterCallback<PointerCancelEvent>(NativeMobileCancel);
        }
        private void DetachNativeMobileGestures()
        {
            if(_nativeMobileGestureRoot==null)return;_nativeMobileGestureRoot.UnregisterCallback<PointerDownEvent>(NativeMobileDown);_nativeMobileGestureRoot.UnregisterCallback<PointerMoveEvent>(NativeMobileMove);_nativeMobileGestureRoot.UnregisterCallback<PointerUpEvent>(NativeMobileUp);_nativeMobileGestureRoot.UnregisterCallback<PointerCancelEvent>(NativeMobileCancel);ResetNativeMobileGestures(true);_nativeMobileGestureRoot=null;
        }
    }
}
