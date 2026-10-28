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
            internal double X,Y;internal bool Moved,Joystick,Placing,Repositioned,Locked;internal NativeMobileSphereHandle Sphere;
        }
        // Captured pointer identity remains valid after deactivation/destruction,
        // as the former retained view did. Its last projected position is kept.
        private sealed class NativeMobileSphereHandle
        {
            internal SphereAttackAgent Agent;internal double X,Y;internal string Family;
        }
        private NativeMobileSphereHandle NativeMobileHandle(SphereAttackAgent agent)
        {if(agent==null)return null;var point=NativeSourcePixels(agent.transform.position);return new NativeMobileSphereHandle {Agent=agent,X=point.x,Y=point.y,Family=agent.Type.ToString().ToLowerInvariant()};}
        private void RefreshNativeMobileSphereHandles()
        {foreach(var pointer in _nativeMobilePointers.Values){var handle=pointer.Sphere;if(handle==null||handle.Agent==null||!handle.Agent.gameObject.activeInHierarchy||!_spheres.Contains(handle.Agent))continue;var point=NativeSourcePixels(handle.Agent.transform.position);handle.X=point.x;handle.Y=point.y;handle.Family=handle.Agent.Type.ToString().ToLowerInvariant();}}
        private int NativeMobileSphereCount=>_spheres.Count(s=>s!=null&&s.gameObject.activeInHierarchy);
        private SphereAttackAgent NativeMobileNear(double x,double y,double radius)
        {foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){var point=NativeSourcePixels(sphere.transform.position);if(NativeMobileDistance(point.x-x,point.y-y)<radius)return sphere;}return null;}
        private sealed class NativeMobileGhost
        {
            internal int Index;internal double X,Y;internal bool Valid;internal EchoSphere.Core.SphereNetworkState Network;internal EchoSphere.Core.SphereNetworkShape Formation;
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
        private void NativeMobileHaptic(int duration)
        {
            // The existing navigator adapter also ignored duration and invoked
            // this exact Unity API only on Android/iOS. Preserve that behavior.
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
        private Vector2 _nativeMobileMoveInput;
        private Vector2 NativeMobileMovement=>_nativeMobileMoveInput;
        private void NativeMobileClearKeys()=>_nativeMobileMoveInput=Vector2.zero;
        private void NativeMobileSetDirection(double x,double y)=>_nativeMobileMoveInput=new Vector2((x>.2?1f:0f)-(x<-.2?1f:0f),(y<-.2?1f:0f)-(y>.2?1f:0f));
        private void ResetNativeMobileGestures(bool unmount)
        {
            NativeMobileClearKeys();if(_nativeMobileGestureRoot!=null)foreach(int id in _nativeMobilePointers.Keys)_nativeMobileGestureRoot.ReleasePointer(id);
            _nativeMobilePointers.Clear();_nativeMobileJoystickId=null;_nativeMobileDx=_nativeMobileDy=0;_nativeMobileJoystickActive=false;
            if(unmount){_nativeMobileGhost=null;_nativeMobileMemory=null;_nativeMobilePlacementColor=null;_nativeMobileMounted=false;_nativeMobileLastX=0;_nativeMobileLastY=-1;}
        }
        private bool NativeMobileAllowed=>_playerCore!=null&&!_gameOver&&(!_paused||_tutorialStep==1||_tutorialStep==2);
        private bool NativeMobileWorld(double x,double y,out double wx,out double wy)
        {
            wx=wy=0;if(_nativeRunField==null)return false;var rect=_nativeRunField.worldBound;if(rect.width<=0||rect.height<=0)return false;
            var camera=NativeSourcePixels((Vector2)(_camera.transform.position-_lastHunterScreenShakeOffset));var size=_worldSurface!=null?_worldSurface.LogicalSize:rect.size;wx=(x-rect.xMin)/rect.width*size.x-size.x/2+camera.x;wy=(y-rect.yMin)/rect.height*size.y-size.y/2+camera.y;return true;
        }
        private static double NativeMobileDistance(double x,double y)=>Math.Sqrt(x*x+y*y);
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        private static double NativeMobileDistance(V pos,double x,double y)=>NativeMobileDistance(pos["x"].Number-x,pos["y"].Number-y);
#endif
        // Preview and commit share the existing native placement gate.
        private bool NativeMobileCanPlace(double x,double y)=>CanPlaceSphereAt(new Vector2((float)x,-(float)y)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,null);
        private bool NativeMobileCanMove(NativeMobileSphereHandle sphere,double x,double y){var agent=sphere?.Agent;if(agent==null||!_spheres.Contains(agent))return false;var point=NativeSourcePixels(agent.transform.position);sphere.X=point.x;sphere.Y=point.y;return CanPlaceSphereAt(new Vector2((float)x,-(float)y)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,agent);}
        private int NativeMobileMaximum=>Math.Min(5+GetAbilityLevel(EchoSphere.Core.RunUpgradeKind.MaxSpheres)+GetShopUpgradeLevel("spheres"),8);
        internal struct NativeMobileNode {internal double X,Y,DisabledTimer;internal bool Alive;}
        private List<NativeMobileNode> ProjectNativeMobileNodes()
        {
            var nodes=new List<NativeMobileNode>();foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){var point=NativeSourcePixels(sphere.transform.position);nodes.Add(new NativeMobileNode {X=point.x,Y=point.y,Alive=true,DisabledTimer=sphere.SourceProjectionNetworkDisabledTimer});}return nodes;
        }
        private EchoSphere.Core.SphereNetworkState NativeMobileNetwork()
        {
            var nodes=ProjectNativeMobileNodes().Select(node=>new EchoSphere.Core.SphereNetworkNode(new EchoSphere.Core.Vec2((float)node.X*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,(float)node.Y*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld),node.Alive)).ToList();return EchoSphere.Core.SphereNetworkRules.Analyze(nodes,EchoSphere.Core.SphereNetworkRules.DefaultLinkDistance);
        }
        internal static EchoSphere.Core.SphereNetworkState NativeMobilePreviewGraph(IReadOnlyList<NativeMobileNode> spheres,double x,double y,int movingIndex,out int candidateIndex)
        {
            int count=spheres.Count;candidateIndex=movingIndex>=0&&movingIndex<count?movingIndex:count;if(count==0)return null;var nodes=new List<EchoSphere.Core.SphereNetworkNode>();
            for(int i=0;i<count;i++){var sphere=spheres[i];nodes.Add(new EchoSphere.Core.SphereNetworkNode(new EchoSphere.Core.Vec2((float)(i==candidateIndex?x:sphere.X)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,(float)(i==candidateIndex?y:sphere.Y)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld),sphere.Alive&&sphere.DisabledTimer<=0));}
            if(candidateIndex==count)nodes.Add(new EchoSphere.Core.SphereNetworkNode(new EchoSphere.Core.Vec2((float)x*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,(float)y*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld)));return EchoSphere.Core.SphereNetworkRules.Analyze(nodes,EchoSphere.Core.SphereNetworkRules.DefaultLinkDistance);
        }
        private bool NativeMobileSpherePoint(int index,out double x,out double y)
        {
            int current=0;foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){if(current++!=index)continue;var point=NativeSourcePixels(sphere.transform.position);x=point.x;y=point.y;return true;}x=y=double.NaN;return false;
        }
        private static EchoSphere.Core.SphereNetworkShape NativeMobileFormation(EchoSphere.Core.SphereNetworkState network)=>network?.DominantFormation;
        private void NativeMobileCaptureMemory(EchoSphere.Core.SphereNetworkState network)
        {
            var formation=NativeMobileFormation(network);if(formation==null)return;_nativeMobileMemory=formation.NodeIndexes.Select(i=>{NativeMobileSpherePoint(i,out double x,out double y);return new Vector2((float)x,(float)y);}).ToArray();_nativeMobileMemoryClosed=formation.Type!=EchoSphere.Core.SphereNetworkFormation.Line;_nativeMobileMemoryAt=Time.unscaledTimeAsDouble;
        }
        private void NativeMobileRememberChange(EchoSphere.Core.SphereNetworkState oldNetwork)
        {
            var before=NativeMobileFormation(oldNetwork);var after=NativeMobileFormation(NativeMobileNetwork());if(before!=null&&(after==null||before.Type!=after.Type||!before.NodeIndexes.SequenceEqual(after.NodeIndexes)))NativeMobileCaptureMemory(oldNetwork);
        }
        private void NativeMobileFlash(string ru,string en,double life,string color){string text=IsRussianUi?ru:en;if(text!=_message)ShowTimedCombatMessage(text,(float)life,color);}
        private void NativeMobileTap(double x,double y)
        {
            if(!NativeMobileAllowed||_levelUp||_stellaRewardPending||!NativeMobileWorld(x,y,out double wx,out double wy))return;
            var near=NativeMobileNear(wx,wy,26);
            if(_tutorialStep==2&&near!=null)return;
            if(near!=null){NativeMobileCaptureMemory(NativeMobileNetwork());int before=NativeMobileSphereCount;NativePlaceOrRemoveSphere(wx,wy);if(NativeMobileSphereCount<before)NativeMobileHaptic(10);return;}
            if(NativeMobileSphereCount>=NativeMobileMaximum)return;
            if(!NativeMobileCanPlace(wx,wy)){NativeMobileFlash("Путь перекрыт","Path blocked",.8,"#ff4d5d");NativeMobileHaptic(35);return;}
            int count=NativeMobileSphereCount;NativePlaceOrRemoveSphere(wx,wy);
            if(NativeMobileSphereCount>count){NativeMobileHaptic(8);_nativeMobilePlacement=new Vector2((float)x,(float)y);_nativeMobilePlacementAt=Time.unscaledTimeAsDouble;_nativeMobilePlacementColor=NativeRunCatalog.Instance.GetSphere(_selectedSphereType.ToString().ToLowerInvariant()).Color;}
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal static V NativeMobilePreviewNetwork(V spheres,double x,double y,int movingIndex,out int candidateIndex)
        {
            int count=(int)spheres["length"].Number;candidateIndex=movingIndex>=0&&movingIndex<count?movingIndex:count;if(count==0)return VisualJs.Null;
            var nodes=new List<V>();int index=0;
            foreach(var sphere in spheres.Items){var position=sphere["pos"];var alive=sphere["alive"];var disabled=sphere["networkDisabledTimer"];nodes.Add(VisualJs.Obj("pos",VisualJs.Obj("x",index==candidateIndex?new V(x):position["x"],"y",index==candidateIndex?new V(y):position["y"]),"alive",alive.Truth?new V((disabled.Truth?disabled.Number:0)<=0):alive,"networkDisabledTimer",0));index++;}
            if(candidateIndex==count)nodes.Add(VisualJs.Obj("pos",VisualJs.Obj("x",x,"y",y),"alive",true,"networkDisabledTimer",0));
            return RenderNativeNetworkSnapshot(NativeAnalyzeSphereNetwork(VisualJs.Arr(nodes.ToArray())));
        }
#endif
        private NativeMobileGhost NativeMobilePreview(NativeMobileSphereHandle sphere,double x,double y)
        {
            int index=-1,current=0;foreach(var candidate in _spheres)if(candidate!=null&&candidate.gameObject.activeInHierarchy){if(sphere!=null&&ReferenceEquals(candidate,sphere.Agent))index=current;current++;}
            var preview=NativeMobilePreviewGraph(ProjectNativeMobileNodes(),x,y,index,out int candidateIndex);
            if(sphere==null&&preview==null)return null;var network=preview??NativeMobileNetwork();
            return new NativeMobileGhost {Index=sphere!=null?index:candidateIndex,X=x,Y=y,Valid=sphere!=null?NativeMobileCanMove(sphere,x,y):NativeMobileCanPlace(x,y),Network=network,Formation=preview!=null?NativeMobileFormation(network):null};
        }
        private void NativeMobileDown(PointerDownEvent e)
        {
            for(var target=e.target as VisualElement;target!=null&&target!=_nativeMobileGestureRoot;target=target.parent)if(target.ClassListContains("native-mobile-control"))return;
            if(!NativeMobileAllowed)return;_nativeMobileGestureRoot.CapturePointer(e.pointerId);
            double x=e.position.x,y=e.position.y;var p=new NativeMobilePointer {X=x,Y=y};
            if(NativeMobileWorld(x,y,out double wx,out double wy)){
                var sphere=NativeMobileHandle(NativeMobileNear(wx,wy,34));
                if(sphere!=null&&_tutorialStep!=1&&_tutorialStep!=2){if(_sphereMovementLocked)p.Locked=true;else p.Sphere=sphere;}
                else {double width=Screen.width/SourceDevicePixelRatio;bool zone=IsLeftHanded?x>=0&&x<width*.5:x>=width*.5&&x<width;p.Joystick=_tutorialStep!=2&&!_nativeMobileJoystickId.HasValue&&zone;p.Placing=(_tutorialStep==2||_tutorialStep!=1&&!p.Joystick)&&NativeMobileSphereCount<NativeMobileMaximum;}
            }
            _nativeMobilePointers[e.pointerId]=p;if(p.Joystick){_nativeMobileJoystickId=e.pointerId;_nativeMobileJoystickX=x;_nativeMobileJoystickY=y;_nativeMobileDx=_nativeMobileDy=0;_nativeMobileJoystickActive=false;}NativeMobileEventCompleted();
        }
        private void NativeMobileMove(PointerMoveEvent e)
        {
            if(!_nativeMobilePointers.TryGetValue(e.pointerId,out var p))return;double dx=e.position.x-p.X,dy=e.position.y-p.Y,distance=NativeMobileDistance(dx,dy);if(p.Locked)return;
            if(p.Sphere!=null||p.Placing){if(distance>12){p.Moved=true;if(NativeMobileWorld(e.position.x,e.position.y,out double x,out double y)){var preview=NativeMobilePreview(p.Sphere,x,y);if(preview!=null){p.Repositioned=preview.Valid;_nativeMobileGhost=preview;}}}NativeMobileEventCompleted();return;}
            if(distance>12)p.Moved=true;
            if(_nativeMobileJoystickId==e.pointerId){_nativeMobileJoystickActive=distance>12;double length=distance==0?1:distance,scale=Math.Min(1,Math.Max(0,length/58));_nativeMobileDx=dx/length*58*scale;_nativeMobileDy=dy/length*58*scale;
                if(distance<12)NativeMobileClearKeys();else{_nativeMobileLastX=dx/distance;_nativeMobileLastY=dy/distance;NativeMobileSetDirection(_nativeMobileLastX,_nativeMobileLastY);}}
            NativeMobileEventCompleted();
        }
        private void NativeMobileEnd(int id,double x,double y)
        {
            _nativeMobilePointers.TryGetValue(id,out var p);_nativeMobilePointers.Remove(id);
            if(p!=null&&p.Sphere!=null){
                if(!p.Moved){NativePlaceOrRemoveSphere(p.Sphere.X,p.Sphere.Y);_nativeMobileGhost=null;NativeMobileHaptic(10);}
                else if(p.Repositioned){var old=NativeMobileNetwork();if(NativeMobileWorld(x,y,out double wx,out double wy)&&NativeMobileCanMove(p.Sphere,wx,wy)){NativeRepositionSphere(p.Sphere.Agent,wx,wy);NativeMobileRememberChange(old);NativeMobileFlash("Сфера перемещена","Sphere moved",.8,NativeRunCatalog.Instance.GetSphere(p.Sphere.Family).Color);}_nativeMobileGhost=null;NativeMobileHaptic(12);}
                else {NativeMobileFlash("Недоступная позиция","Invalid position",.65,"#ff4d5d");_nativeMobileGhost=null;NativeMobileHaptic(22);}return;
            }
            if(p!=null&&p.Placing){
                if(!p.Moved){NativeMobileTap(x,y);_nativeMobileGhost=null;return;}
                if(p.Repositioned){if(NativeMobileWorld(x,y,out double wx,out double wy)&&NativeMobileCanPlace(wx,wy)){var old=NativeMobileNetwork();int before=NativeMobileSphereCount;NativePlaceOrRemoveSphere(wx,wy);if(NativeMobileSphereCount>before){NativeMobileRememberChange(old);NativeMobileHaptic(8);}}_nativeMobileGhost=null;return;}
                NativeMobileFlash("Недоступная позиция","Invalid position",.65,"#ff4d5d");_nativeMobileGhost=null;NativeMobileHaptic(22);return;
            }
            if(_nativeMobileJoystickId==id){_nativeMobileJoystickId=null;NativeMobileClearKeys();if(p!=null&&!p.Moved)NativeMobileTap(x,y);}else if(p!=null&&!p.Moved)NativeMobileTap(x,y);
        }
        private void NativeMobileUp(PointerUpEvent e){NativeMobileEnd(e.pointerId,e.position.x,e.position.y);_nativeMobileGestureRoot.ReleasePointer(e.pointerId);NativeMobileEventCompleted();}
        private void NativeMobileCancel(PointerCancelEvent e){NativeMobileEnd(e.pointerId,e.position.x,e.position.y);_nativeMobileGestureRoot.ReleasePointer(e.pointerId);NativeMobileEventCompleted();}
        private void NativeMobileEventCompleted(){if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void PrepareNativeMobileGestures(NativeRunUiAnchors anchors)
        {
            var element=anchors.Mobile.Content;if(element==_nativeMobileGestureRoot)return;DetachNativeMobileGestures();_nativeMobileGestureRoot=element;if(element==null)return;
            element.RegisterCallback<PointerDownEvent>(NativeMobileDown);element.RegisterCallback<PointerMoveEvent>(NativeMobileMove);element.RegisterCallback<PointerUpEvent>(NativeMobileUp);element.RegisterCallback<PointerCancelEvent>(NativeMobileCancel);
        }
        private void DetachNativeMobileGestures()
        {
            if(_nativeMobileGestureRoot==null)return;_nativeMobileGestureRoot.UnregisterCallback<PointerDownEvent>(NativeMobileDown);_nativeMobileGestureRoot.UnregisterCallback<PointerMoveEvent>(NativeMobileMove);_nativeMobileGestureRoot.UnregisterCallback<PointerUpEvent>(NativeMobileUp);_nativeMobileGestureRoot.UnregisterCallback<PointerCancelEvent>(NativeMobileCancel);ResetNativeMobileGestures(true);_nativeMobileGestureRoot=null;
        }
    }
}
