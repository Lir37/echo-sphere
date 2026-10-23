using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Typed host contract. A native root can bind these slots directly without
    // exposing DOM nodes or CSS selector rules to any native presenter.
    internal sealed class NativeRunUiAnchors
    {
        internal sealed class Slot
        {
            internal VisualElement Element,Content;internal bool IsBound;
            private readonly List<VisualElement> _legacy=new List<VisualElement>();
            internal void Clear(){Element=Content=null;IsBound=false;_legacy.Clear();}
            internal void Bind(VisualElement element,VisualElement content,IEnumerable<VisualElement> legacy)
            {IsBound=true;Element=element;Content=content;_legacy.Clear();if(legacy!=null)foreach(var child in legacy)if(child!=null)_legacy.Add(child);}
            internal void HideLegacy(bool collapse=false)
            {foreach(var child in _legacy)if(collapse)child.style.display=DisplayStyle.None;else child.style.visibility=Visibility.Hidden;}
        }
        internal readonly Slot HudLeft=new Slot(),HudRight=new Slot(),PauseShell=new Slot(),PauseHeader=new Slot(),PauseFooter=new Slot(),PauseBody=new Slot(),PauseDetail=new Slot(),Result=new Slot(),Upgrade=new Slot(),Rewards=new Slot(),Tutorial=new Slot(),Mobile=new Slot(),Dock=new Slot(),Joystick=new Slot(),Placement=new Slot(),Ghost=new Slot(),Memory=new Slot();
        private readonly Slot[] _slots;
        internal NativeRunUiAnchors(){_slots=new[]{HudLeft,HudRight,PauseShell,PauseHeader,PauseFooter,PauseBody,PauseDetail,Result,Upgrade,Rewards,Tutorial,Mobile,Dock,Joystick,Placement,Ghost,Memory};}
        internal Func<int> ReadVersion;internal int Version=>ReadVersion?.Invoke()??0;internal INativeUiBackdrops Backdrops;internal Action Invalidate;internal Func<string,Rect?> TutorialTarget;
        internal void Clear()
        {foreach(var slot in _slots)slot.Clear();}
    }
}
