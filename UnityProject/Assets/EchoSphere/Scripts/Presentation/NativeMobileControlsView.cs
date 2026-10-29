using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained combat dock. The native runtime controller owns pointer actions;
    // this view owns only layout, text and paint.
    internal sealed class NativeMobileControlsView : IDisposable
    {
        internal sealed class Control {internal string Id,Name,Detail,Icon,Color;internal bool Disabled,Active;internal string Tooltip;internal float Cooldown;}
        internal sealed class State {internal bool Right;internal string Name,Description;internal readonly List<Control> Controls=new List<Control>();}
        private sealed class Item {internal Button Button;internal Label Name,Detail;internal VisualElement Icon,Bar;internal NativeHudSurface Surface;internal NativeMenuBrush.Paint Normal,Pressed,ActivePaint;internal bool Disabled,Active,IsPressed;}
        internal VisualElement Root {get;}
        internal bool Ready {get;private set;}
        private readonly Action<Button,string> _bindInput;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly Dictionary<string,Item> _items=new Dictionary<string,Item>();
        private readonly NativeUiAlphaShadow _glyphShadow=new NativeUiAlphaShadow(7,C("#dff7ff"));
        private VisualElement _abilities;private Label _name,_description;
        private string _keys;private int _geometryFrame;private bool _bound;
        private static Color C(string text){ColorUtility.TryParseHtmlString(text,out var c);return c;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        internal NativeMobileControlsView(VisualElement parent,Action<Button,string> bindInput)
        {
            _bindInput=bindInput;Root=Box(parent,"native-mobile-dock");Root.style.width=244;Root.style.position=Position.Absolute;Root.style.opacity=0;
            Root.RegisterCallback<GeometryChangedEvent>(_=>_geometryFrame=Time.frameCount);
        }
        private static VisualElement Box(VisualElement parent,string name,bool row=false){var e=new VisualElement {name=name,pickingMode=PickingMode.Ignore};e.style.flexShrink=0;if(row)e.style.flexDirection=FlexDirection.Row;parent.Add(e);return e;}
        private static Label Text(VisualElement parent,string name,float size,string color,string weight="400",bool wrap=false,double line=1.5)=>NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);
        private NativeHudSurface Surface(VisualElement e,NativeMenuBrush.Paint p){var s=new NativeHudSurface(e,p);_surfaces.Add(s);return s;}
        private static NativeMenuBrush.Paint Paint(bool pressed,bool dash,bool sphere)=>new NativeMenuBrush.Paint {
            Radius=sphere?22:2,Ellipse=sphere,Border=1,BorderColor=pressed?A(105,232,255,.56f):dash?A(255,196,104,.55f):A(105,224,255,.24f),LinearAngle=180,
            Colors=dash?new[]{A(90,55,35,.92f),A(34,17,18,.95f)}:new[]{A(10,25,40,.90f),A(3,9,17,.94f)},Stops=new[]{0f,1f},
            Inset=pressed?new NativeMenuBrush.Shadow(22,Vector2.zero,A(72,208,255,.10f)):new NativeMenuBrush.Shadow(0,new Vector2(0,1),A(255,255,255,.055f)),
            OuterA=pressed?new NativeMenuBrush.Shadow(26,Vector2.zero,A(71,208,255,.12f)):new NativeMenuBrush.Shadow(24,new Vector2(0,10),A(0,0,0,.28f))};
        private Item Button(VisualElement parent,Control model,float width,float height,bool row=false)
        {
            var b=new Button {name="native-mobile-"+model.Id,clickable=null};parent.Add(b);b.style.width=width;b.style.height=height;b.style.flexShrink=0;b.style.alignItems=Align.Center;b.style.justifyContent=Justify.Center;
            b.style.flexDirection=row?FlexDirection.Row:FlexDirection.Column;b.style.marginLeft=b.style.marginRight=b.style.marginTop=b.style.marginBottom=0;
            b.style.paddingLeft=b.style.paddingRight=model.Id.StartsWith("ability:")?4:0;b.style.paddingTop=b.style.paddingBottom=0;
            b.style.borderLeftWidth=b.style.borderRightWidth=b.style.borderTopWidth=b.style.borderBottomWidth=0;b.style.backgroundColor=Color.clear;
            bool sphere=model.Id.StartsWith("sphere:"),dash=model.Id=="dash";var item=new Item {Button=b,Normal=Paint(false,dash,sphere),Pressed=Paint(true,dash,sphere),ActivePaint=Paint(true,dash,sphere)};if(sphere&&model.Color!=null)item.ActivePaint.BorderColor=C(model.Color);item.Surface=Surface(b,item.Normal);_items.Add(model.Id,item);
            _bindInput(b,model.Id);
            void Press(bool value){if(item.Disabled)value=false;b.style.scale=new Scale(new Vector3(value ? .95f : 1,value ? .95f : 1,1));item.IsPressed=value;item.Surface.SetPaint(value?item.Pressed:item.Active?item.ActivePaint:item.Normal);}
            b.RegisterCallback<PointerDownEvent>(e=>{if(!item.Disabled){b.CapturePointer(e.pointerId);Press(true);}});
            b.RegisterCallback<PointerUpEvent>(e=>{b.ReleasePointer(e.pointerId);Press(false);});b.RegisterCallback<PointerCancelEvent>(e=>{b.ReleasePointer(e.pointerId);Press(false);});b.RegisterCallback<PointerCaptureOutEvent>(_=>Press(false));
            if(sphere){Glyph(b,model.Id.Substring(7),model.Color);var orbit=Box(b,"orbit-squash");orbit.style.position=Position.Absolute;orbit.style.left=orbit.style.top=7;orbit.style.right=orbit.style.bottom=7;orbit.style.scale=new Scale(new Vector3(1,.42f,1));var ring=Box(orbit,"orbit");ring.style.flexGrow=1;ring.style.rotate=new Rotate(-18);ring.style.opacity=.16f;Border(ring,1,C("#dff7ff"),100);}
            else {
                bool toggle=model.Id=="lock"||model.Id=="follow",action=model.Id.StartsWith("ability:")||dash;
                if(model.Icon!=null){item.Icon=Box(b,"icon");item.Icon.style.width=item.Icon.style.height=model.Id=="pause"?18:dash?18:14;item.Icon.style.unityBackgroundImageTintColor=C("#dff7ff");if(row)item.Icon.style.marginRight=4;}
                if(model.Id!="pause"){
                    var labels=Box(b,"labels");labels.style.minWidth=0;labels.style.width=toggle?68:action?(dash?89:111):width;labels.style.maxWidth=Length.Percent(100);labels.style.flexShrink=1;
                    item.Name=Text(labels,"label",9,"#dff7ff","700");item.Name.style.whiteSpace=WhiteSpace.NoWrap;item.Name.style.overflow=Overflow.Hidden;item.Name.style.textOverflow=TextOverflow.Ellipsis;item.Name.style.minWidth=0;item.Name.style.unityTextAlign=TextAnchor.MiddleCenter;
                    item.Detail=Text(labels,"detail",8,"#b6c9de99");item.Detail.style.whiteSpace=WhiteSpace.NoWrap;item.Detail.style.overflow=Overflow.Hidden;item.Detail.style.textOverflow=TextOverflow.Ellipsis;item.Detail.style.minWidth=0;item.Detail.style.unityTextAlign=TextAnchor.MiddleCenter;
                }
                if(action){item.Bar=Box(b,"cooldown-progress");item.Bar.style.position=Position.Absolute;item.Bar.style.bottom=0;item.Bar.style.left=0;item.Bar.style.height=4;item.Bar.style.backgroundColor=A(255,77,93,.7f);b.style.overflow=Overflow.Hidden;}
            }
            return item;
        }
        private void Build(State state)
        {
            foreach(var s in _surfaces)s.Dispose();_surfaces.Clear();_items.Clear();_glyphShadow.Clear();Root.Clear();
            Control Find(string id)=>state.Controls.Find(c=>c.Id==id);
            var toolbar=Box(Root,"toggles",true);toolbar.style.width=244;toolbar.style.flexDirection=state.Right?FlexDirection.RowReverse:FlexDirection.Row;
            var pause=Button(toolbar,Find("pause"),44,44).Button;
            var locked=Button(toolbar,Find("lock"),94,44,true).Button;
            Button(toolbar,Find("follow"),94,44,true);
            if(state.Right){pause.style.marginLeft=6;locked.style.marginLeft=6;}else {pause.style.marginRight=6;locked.style.marginRight=6;}
            _abilities=Box(Root,"abilities");_abilities.style.marginTop=8;_abilities.style.width=244;
            int i=0;VisualElement actionRow=null;
            void ActionButton(Control c){if(i%2==0){actionRow=Box(_abilities,"action-row-"+(i/2),true);actionRow.style.width=244;actionRow.style.height=44;actionRow.style.position=Position.Absolute;actionRow.style.top=(i/2)*50;}
                var b=Button(actionRow,c,119,44,c.Id=="dash").Button;b.style.position=Position.Absolute;b.style.left=(i%2)*125;i++;}
            foreach(var c in state.Controls)if(c.Id.StartsWith("ability:"))ActionButton(c);
            ActionButton(Find("dash"));_abilities.style.height=((i+1)/2)*50-6;

            var info=Box(Root,"sphere-info");info.style.width=220;info.style.marginTop=8;info.style.paddingLeft=info.style.paddingRight=12;info.style.paddingTop=info.style.paddingBottom=8;
            Surface(info,new NativeMenuBrush.Paint {Radius=12,Border=1,BorderColor=C("#243b55"),Fill=A(13,23,38,.9f),OuterA=new NativeMenuBrush.Shadow(15,new Vector2(0,10),A(0,0,0,.1f),-3),OuterB=new NativeMenuBrush.Shadow(6,new Vector2(0,4),A(0,0,0,.1f),-4)});
            _name=Text(info,"name",14,"#dcecff","700",true);_description=Text(info,"description",10,"#7f9bb8",wrap:true,line:1.25);_description.style.marginTop=2;_name.style.unityTextAlign=_description.style.unityTextAlign=TextAnchor.UpperCenter;
            var spheres=Box(Root,"spheres");spheres.style.width=244;spheres.style.marginTop=8;spheres.style.height=94;i=0;VisualElement sphereRow=null;
            foreach(var c in state.Controls)if(c.Id.StartsWith("sphere:")){
                if(i%5==0){sphereRow=Box(spheres,"sphere-row-"+(i/5),true);sphereRow.style.width=244;sphereRow.style.height=44;sphereRow.style.position=Position.Absolute;sphereRow.style.top=(i/5)*50;}
                var b=Button(sphereRow,c,44,44).Button;b.style.position=Position.Absolute;b.style.left=(i%5)*50;i++;
            }

        }
        internal void Bind(State state)
        {
            string keys=(state.Right?"right|":"left|")+string.Join("|",state.Controls.ConvertAll(c=>c.Id));if(keys!=_keys){_keys=keys;Build(state);}
            Root.style.alignItems=state.Right?Align.FlexEnd:Align.FlexStart;_name.text=state.Name;_description.text=state.Description;
            foreach(var c in state.Controls){var item=_items[c.Id];item.Disabled=c.Disabled;string tooltip=c.Tooltip??c.Name;if(item.Button.tooltip!=tooltip)item.Button.tooltip=tooltip;item.Button.SetEnabled(!c.Disabled);if(item.Active!=c.Active){item.Active=c.Active;item.Surface.SetPaint(item.IsPressed?item.Pressed:item.Active?item.ActivePaint:item.Normal);}item.Button.style.opacity=c.Id=="follow"&&c.Disabled ? .4f : 1;
                if(item.Name!=null)item.Name.text=c.Name;if(item.Detail!=null)item.Detail.text=c.Detail;if(item.Icon!=null)item.Icon.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+c.Icon));if(item.Bar!=null)item.Bar.style.width=Length.Percent(c.Cooldown);}
            _bound=true;
        }
        internal void Prepare(float scale)
        {
            if(!_bound||!float.IsFinite(Root.layout.height)||Root.layout.height<=0)return;foreach(var s in _surfaces)s.Prepare(scale);
            if(!Ready&&Time.frameCount-_geometryFrame>=2){Ready=true;Root.style.opacity=1;Root.style.position=Position.Relative;Root.style.left=Root.style.top=StyleKeyword.Auto;}
        }
        private static void Border(VisualElement e,float width,Color color,float radius){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=width;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=color;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        private void Glyph(VisualElement parent,string type,string html)
        {
            var color=C(html);VisualElement Part(VisualElement p,float w,float h,float radius,float alpha,Action<VisualElement> configure=null)=>_glyphShadow.Build(p,w,h,layer=>{var e=Box(layer,"sphere-glyph");e.style.width=w;e.style.height=h;Border(e,2,color,radius);var fill=color;fill.a=alpha;e.style.backgroundColor=fill;configure?.Invoke(e);});
            if(type=="shotgun")_glyphShadow.Build(parent,20,16,group=>{var a=Part(group,12,8,2,.2f);a.style.position=Position.Absolute;a.style.left=0;a.style.top=4;var b=Part(group,8,16,0,0,e=>{e.style.borderLeftWidth=0;e.style.borderTopRightRadius=e.style.borderBottomRightRadius=4;});b.style.position=Position.Absolute;b.style.left=12;b.style.top=0;});
            else if(type=="chain")_glyphShadow.Build(parent,34,10,group=>{group.style.flexDirection=FlexDirection.Row;for(int i=0;i<3;i++){var e=Part(group,10,10,100,.2f);e.style.marginRight=i<2?2:0;}},intrinsic:true);
            else if(type=="aura")Part(parent,20,20,100,0,e=>Surface(e,new NativeMenuBrush.Paint {Ellipse=true,Border=2,BorderColor=color,OuterA=new NativeMenuBrush.Shadow(0,Vector2.zero,new Color(color.r,color.g,color.b,34/255f),3)}));
            else {var e=Part(parent,16,16,type=="sniper"?0:100,type=="sniper" ? .2f : 68/255f);if(type=="sniper")e.style.rotate=new Rotate(45);}
        }
        public void Dispose(){foreach(var s in _surfaces)s.Dispose();_glyphShadow.Dispose();Root.RemoveFromHierarchy();}
    }
}
