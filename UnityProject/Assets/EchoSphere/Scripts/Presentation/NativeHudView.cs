using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained App.tsx Hud candidate. Production selection waits for full acceptance.
    // Models contain display values only; this class never computes combat rules.
    public sealed class NativeHudView : IDisposable
    {
        public sealed class Formation
        {
            public string Id,Name;public bool Inactive;
        }
        public sealed class State
        {
            public bool Russian,HasRegion,Boss;
            public string Level,Hp,Resonance,Spheres,Wave,Links,Timer,Region,Phase,Pocket,BossLabel;
            public float XpPercent,HpPercent,ResonancePercent;
            public bool ResonanceReady;
            public Formation Dominant,Secondary;
            public double PlayerX,PlayerY;
            public IReadOnlyList<NativeHudRadarView.Enemy> Enemies=Array.Empty<NativeHudRadarView.Enemy>();
            public IReadOnlyList<NativeHudRadarView.Poi> Pois=Array.Empty<NativeHudRadarView.Poi>();
        }
        public readonly struct TooltipLine
        {
            public readonly string Active,Rest;
            public TooltipLine(string active,string rest){Active=active;Rest=rest;}
        }
        public sealed class Tooltip
        {
            public string Title,Effectiveness;public bool Secondary,Inactive;
            public IReadOnlyList<TooltipLine> Lines=Array.Empty<TooltipLine>();
        }
        private const string Family="Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif";
        private readonly VisualElement _parent,_root,_left,_right,_rightClip,_region,_formationRow,_tooltip,_tooltipLines,_tooltipHeading;
        private readonly Label _level,_hp,_resonanceTitle,_resonance,_spheres,_wave,_linksTitle,_links,_noFormation,_timer,_regionTitle,_phase,_pocket,_boss,_plus,_tooltipTitle,_tooltipEffect;
        private readonly VisualElement _xpFill,_hpFill,_resonanceFill;
        private readonly Button[] _formationButtons=new Button[2];
        private readonly Label[] _formationNames=new Label[2];
        private readonly string[] _formationIds=new string[2];
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly List<Label> _tooltipRows=new List<Label>();
        private readonly NativeHudSurface[] _formationSurfaces=new NativeHudSurface[2];
        private readonly NativeMenuBrush.Paint _dominantPaint,_inactivePaint,_secondaryPaint,_resonanceNormal,_resonanceReady;
        private readonly NativeHudDashBorder _inactiveBorder;
        private readonly Material _filterMaterial;
        private readonly FilterFunctionDefinition _saturation;
        private readonly List<FilterFunction> _inactiveFilters=new List<FilterFunction>(),_noFilters=new List<FilterFunction>();
        private readonly NativeHudSurface _resonanceSurface;
        private readonly Func<string,Tooltip> _readTooltip;
        private readonly Func<bool> _swap;
        private string _tooltipId;
        private int _pointerId=-1,_dragSlot=-1;
        private bool _disposed,_bound;
        private int _geometryFrame;
        private float _safeLeft,_safeRight,_safeTop;
        private IVisualElementScheduledItem _hideTimer;
        public VisualElement Root=>_root;
        public bool Ready {get;private set;}
        public NativeHudRadarView Radar {get;}
        private static Color C(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        private static void Border(VisualElement e,float width){e.style.borderTopWidth=e.style.borderBottomWidth=e.style.borderLeftWidth=e.style.borderRightWidth=width;e.style.borderTopColor=e.style.borderBottomColor=e.style.borderLeftColor=e.style.borderRightColor=Color.clear;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)
        {
            var e=new VisualElement {name=name,pickingMode=PickingMode.Ignore};e.style.flexShrink=0;if(row)e.style.flexDirection=FlexDirection.Row;parent.Add(e);return e;
        }
        private static Label Text(VisualElement parent,string name,float size,string color,string weight="400",double line=1.5,float spacing=.192f)
        {
            var label=new Label {name=name,pickingMode=PickingMode.Ignore,enableRichText=false};label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;label.style.flexShrink=0;
            label.style.fontSize=size;label.style.color=C(color);label.style.whiteSpace=WhiteSpace.NoWrap;label.style.height=(float)(size*line);
            var font=SourceUiTypography.Resolve(Family,weight,false,line);if(font!=null)label.style.unityFontDefinition=FontDefinition.FromSDFFont(font);label.style.unityFontStyleAndWeight=FontStyle.Normal;
            SetSpacing(label,spacing,size);
            parent.Add(label);return label;
        }
        private static void SetSpacing(Label label,float pixels,float size)
        {
            // Standard TextCore characterSpacing uses hundredths of an em.
            // Preserve authored CSS pixels independently of font size. The
            // tracking fixture checks measured advances at 8/12/24px.
            label.style.letterSpacing=size>0?pixels*100/size:pixels;
        }
        private NativeHudSurface Surface(VisualElement e,NativeMenuBrush.Paint paint){var s=new NativeHudSurface(e,paint);_surfaces.Add(s);return s;}
        private VisualElement Progress(VisualElement parent,string name,float height,float margin,float radius,Color fill,Color border,NativeMenuBrush.Paint foreground,out NativeHudSurface surface)
        {
            var track=Box(parent,name);track.style.height=height;track.style.marginTop=margin;Border(track,1);
            Surface(track,new NativeMenuBrush.Paint {Fill=fill,Border=1,BorderColor=border,Radius=radius});
            var clip=Box(track,name+"-clip");clip.style.position=Position.Absolute;clip.style.left=clip.style.right=clip.style.top=clip.style.bottom=0;clip.style.overflow=Overflow.Hidden;
            float innerRadius=Mathf.Max(0,Mathf.Min(radius,height*.5f)-1);clip.style.borderTopLeftRadius=clip.style.borderTopRightRadius=clip.style.borderBottomLeftRadius=clip.style.borderBottomRightRadius=innerRadius;
            var bar=Box(clip,name+"-fill");bar.style.height=Length.Percent(100);bar.style.width=Length.Percent(0);surface=Surface(bar,foreground);return bar;
        }
        public NativeHudView(VisualElement parent,Func<bool> swap,Func<string,Tooltip> readTooltip)
        {
            _parent=parent??throw new ArgumentNullException(nameof(parent));_swap=swap??throw new ArgumentNullException(nameof(swap));_readTooltip=readTooltip??throw new ArgumentNullException(nameof(readTooltip));
            _root=Box(parent,"native-gameplay-hud");_root.style.position=Position.Absolute;_root.style.left=_root.style.right=_root.style.top=_root.style.bottom=0;_root.style.opacity=0;_geometryFrame=Time.frameCount;
            _left=Box(_root,"hud-left");_right=Box(_root,"hud-right");
            foreach(var panel in new[]{_left,_right}){
                panel.style.position=Position.Absolute;Border(panel,1);
                Surface(panel,new NativeMenuBrush.Paint {Radius=2,Fill=A(2,8,15,.84f),Border=1,BorderColor=A(91,199,255,.2f),Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),A(255,255,255,.035f)),OuterA=new NativeMenuBrush.Shadow(34,new Vector2(0,12),A(0,0,0,.34f))});
            }
            _rightClip=Box(_right,"hud-right-clip");_rightClip.style.position=Position.Absolute;_rightClip.style.left=_rightClip.style.right=_rightClip.style.top=_rightClip.style.bottom=0;_rightClip.style.overflow=Overflow.Hidden;
            _rightClip.style.borderTopLeftRadius=_rightClip.style.borderTopRightRadius=_rightClip.style.borderBottomLeftRadius=_rightClip.style.borderBottomRightRadius=1;
            var heading=Box(_left,"hud-heading",true);heading.style.alignItems=Align.Center;heading.style.justifyContent=Justify.SpaceBetween;
            _level=Text(heading,"hud-level",8,"#6ae2ff",spacing:1.76f);_level.style.color=A(106,226,255,.78f);
            _hp=Text(heading,"hud-hp",11,"#e4f5ff","700");
            _xpFill=Progress(_left,"hud-xp-bar",5,6,2,A(3,11,20,.92f),A(102,204,252,.18f),new NativeMenuBrush.Paint {Radius=2,Linear=true,Horizontal=true,Colors=new[]{C("#2c7cff"),C("#68e9ff")},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(14,Vector2.zero,A(82,221,255,.22f))},out _);
            _hpFill=Progress(_left,"hud-hp-bar",4,4,999,A(2,8,15,.92f),A(255,103,111,.18f),new NativeMenuBrush.Paint {Radius=999,Linear=true,Horizontal=true,Colors=new[]{C("#b22f63"),C("#ff6c74")},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(10,Vector2.zero,A(255,90,110,.18f))},out _);
            var resonance=Box(_left,"hud-resonance-label",true);resonance.style.marginTop=6;resonance.style.justifyContent=Justify.SpaceBetween;
            _resonanceTitle=Text(resonance,"hud-resonance-title",7,"#7f9bb8",spacing:.84f);_resonance=Text(resonance,"hud-resonance-value",7,"#dcecff","700",spacing:.84f);
            _resonanceNormal=new NativeMenuBrush.Paint {Fill=C("#39d8ff")};_resonanceReady=new NativeMenuBrush.Paint {Fill=C("#ffb84d")};
            _resonanceFill=Progress(_left,"hud-resonance-bar",6,2,999,C("#13243a"),C("#243b55"),_resonanceNormal,out _resonanceSurface);
            var meta=Box(_left,"hud-meta",true);meta.style.marginTop=8;_spheres=Text(meta,"hud-spheres",7,"#66859f",spacing:.91f);_wave=Text(meta,"hud-wave",7,"#66859f",spacing:.91f);_spheres.enableRichText=_wave.enableRichText=true;_spheres.style.flexBasis=_wave.style.flexBasis=0;_spheres.style.flexGrow=_wave.style.flexGrow=1;_spheres.style.minWidth=_wave.style.minWidth=0;_spheres.style.marginRight=4;_wave.style.marginLeft=4;
            var links=Box(_left,"hud-links",true);links.style.marginTop=5;links.style.justifyContent=Justify.SpaceBetween;_linksTitle=Text(links,"hud-links-title",6,"#6f8da4","800",spacing:.72f);_links=Text(links,"hud-links-value",6,"#dcecff","900",spacing:.72f);
            var stack=Box(_left,"hud-formation-stack");stack.style.marginTop=3;stack.pickingMode=PickingMode.Position;
            _noFormation=Text(stack,"hud-no-formation",7,"#7f9bb8");_formationRow=Box(stack,"hud-formations",true);_formationRow.style.alignItems=Align.Center;
            _dominantPaint=new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=A(255,207,122,.82f),Linear=true,Colors=new[]{A(94,63,20,.32f),A(8,18,28,.90f)},Stops=new[]{0f,1f},Inset=new NativeMenuBrush.Shadow(14,Vector2.zero,A(255,184,77,.09f)),OuterA=new NativeMenuBrush.Shadow(14,Vector2.zero,A(255,184,77,.08f))};
            _inactivePaint=new NativeMenuBrush.Paint {Radius=3,Border=1,Linear=true,Colors=_dominantPaint.Colors,Stops=_dominantPaint.Stops,Inset=_dominantPaint.Inset,OuterA=_dominantPaint.OuterA};
            _secondaryPaint=new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=A(103,213,255,.33f),Fill=A(6,18,29,.70f)};
            _filterMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceUiFilters"));
            _saturation=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_saturation.name="Native HUD inactive formation saturation";_saturation.filterName="native-hud-saturation";_saturation.parameters=Array.Empty<FilterParameterDeclaration>();
            _saturation.passes=new[]{new PostProcessingPass {material=_filterMaterial,passIndex=0,applySettingsCallback=(properties,context)=>{properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);properties.SetFloat("_Amount",.65f);}}};
            _inactiveFilters.Add(new FilterFunction(_saturation));
            for(int i=0;i<2;i++){
                if(i==1){_plus=Text(_formationRow,"hud-formation-plus",13,"#d2e5f2","300",1);_plus.style.color=A(210,229,242,.34f);_plus.style.marginLeft=_plus.style.marginRight=5;}
                var index=i;var button=new Button {name="hud-formation-"+i};button.style.marginLeft=button.style.marginRight=button.style.marginTop=button.style.marginBottom=0;button.style.paddingLeft=button.style.paddingRight=7;button.style.paddingTop=button.style.paddingBottom=6;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.flexDirection=FlexDirection.Row;button.style.alignItems=Align.Center;button.style.backgroundColor=Color.clear;Border(button,1);_formationRow.Add(button);
                // The source uses pointer capture/hold, with no click action.
                // A Button's default Clickable would release capture before our drag ends.
                button.clickable=null;
                button.style.transitionProperty=new List<StylePropertyName>{new StylePropertyName("opacity"),new StylePropertyName("translate")};button.style.transitionDuration=new List<TimeValue>{new TimeValue(.15f,TimeUnit.Second)};button.style.transitionTimingFunction=new List<EasingFunction>{new EasingFunction(EasingMode.Ease)};
                _formationButtons[i]=button;_formationSurfaces[i]=Surface(button,i==0?_dominantPaint:_secondaryPaint);
                if(i==0){_inactiveBorder=new NativeHudDashBorder();button.Add(_inactiveBorder);_inactiveBorder.style.display=DisplayStyle.None;}
                _formationNames[i]=Text(button,"hud-formation-name-"+i,9,i==0?"#ffcf7a":"#79dcff","800",spacing:.72f);_formationNames[i].style.flexShrink=1;_formationNames[i].style.minWidth=0;_formationNames[i].style.overflow=Overflow.Hidden;_formationNames[i].style.textOverflow=TextOverflow.Ellipsis;
                if(i==0)_formationNames[i].style.textShadow=new TextShadow {offset=Vector2.zero,blurRadius=10,color=A(255,184,77,.18f)};
                button.RegisterCallback<PointerDownEvent>(evt=>{if(evt.button!=0)return;button.focusController?.IgnoreEvent(evt);_dragSlot=index;_pointerId=evt.pointerId;button.CapturePointer(evt.pointerId);OpenTooltip(_formationIds[index]);});
                button.RegisterCallback<PointerUpEvent>(evt=>{var source=_dragSlot;if(_pointerId!=evt.pointerId)return;_dragSlot=-1;_pointerId=-1;if(button.HasPointerCapture(evt.pointerId))button.ReleasePointer(evt.pointerId);var picked=_root.panel?.Pick(evt.position);int target=Slot(picked);if(target>=0&&source>=0&&source!=target&&_swap())HideTooltip();});
                button.RegisterCallback<PointerCancelEvent>(evt=>CancelDrag());button.RegisterCallback<PointerCaptureOutEvent>(evt=>{if(_pointerId==evt.pointerId)CancelDrag();});
                button.RegisterCallback<PointerEnterEvent>(evt=>{button.style.translate=new Translate(0,-1);if(index==1)button.style.opacity=.82f;});button.RegisterCallback<PointerLeaveEvent>(evt=>{button.style.translate=new Translate(0,0);if(index==1)button.style.opacity=.72f;});
            }
            _tooltip=Box(stack,"hud-network-tooltip");_tooltip.style.position=Position.Absolute;_tooltip.style.left=0;_tooltip.style.top=Length.Percent(100);_tooltip.style.marginTop=4;_tooltip.style.paddingLeft=_tooltip.style.paddingRight=9;_tooltip.style.paddingTop=_tooltip.style.paddingBottom=8;Border(_tooltip,1);_tooltip.pickingMode=PickingMode.Position;_tooltip.focusable=true;
            Surface(_tooltip,new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=A(95,199,244,.24f),Fill=A(2,8,15,.97f),OuterA=new NativeMenuBrush.Shadow(34,new Vector2(0,16),A(0,0,0,.42f)),Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),A(255,255,255,.03f))});
            var tipHeading=Box(_tooltip,"hud-tooltip-heading",true);_tooltipHeading=tipHeading;tipHeading.style.justifyContent=Justify.SpaceBetween;tipHeading.style.paddingBottom=4;tipHeading.style.marginBottom=5;tipHeading.style.borderBottomWidth=1;tipHeading.style.borderBottomColor=A(143,223,255,.12f);
            _tooltipTitle=Text(tipHeading,"hud-tooltip-title",8,"#ffcf7a","900",spacing:1.12f);_tooltipEffect=Text(tipHeading,"hud-tooltip-effect",7,"#ffcf7a","900",spacing:.56f);_tooltipEffect.style.opacity=.72f;_tooltipLines=Box(_tooltip,"hud-tooltip-lines");
            _tooltip.RegisterCallback<PointerDownEvent>(evt=>HideTooltip());_tooltip.RegisterCallback<KeyDownEvent>(evt=>{if(evt.keyCode==KeyCode.Return||evt.keyCode==KeyCode.KeypadEnter||evt.keyCode==KeyCode.Space)HideTooltip();});HideTooltip();
            var radarRow=Box(_rightClip,"hud-radar-row",true);radarRow.style.justifyContent=Justify.FlexEnd;Radar=new NativeHudRadarView(radarRow);
            var timerRow=Box(_rightClip,"hud-timer-row",true);timerRow.style.marginTop=2;timerRow.style.justifyContent=Justify.FlexEnd;
            var timer=Box(timerRow,"hud-timer",true);timer.style.alignItems=Align.Center;
            for(int i=0;i<3;i++)if(i==1){_timer=Text(timer,"hud-time",11,"#eaf7ff","500",spacing:1.1f);_timer.style.marginLeft=_timer.style.marginRight=5;}else{var dot=Box(timer,"hud-timer-dot-"+i);dot.style.width=dot.style.height=3;Surface(dot,new NativeMenuBrush.Paint {Ellipse=true,Fill=C("#6be9ff"),OuterA=new NativeMenuBrush.Shadow(8,Vector2.zero,A(107,233,255,.65f))});}
            _region=Box(_rightClip,"hud-region");_region.style.marginTop=7;_regionTitle=Text(_region,"hud-region-title",8,"#66dfc7","900",1.15,1.12f);_phase=Text(_region,"hud-region-phase",9,"#e0eff5","400",1.18);_pocket=Text(_region,"hud-region-pocket",7,"#7a96aa","400",1.35);_phase.style.marginTop=_pocket.style.marginTop=2;
            foreach(var label in new[]{_regionTitle,_phase,_pocket}){label.style.whiteSpace=WhiteSpace.Normal;label.style.height=StyleKeyword.Auto;}
            _boss=Text(_rightClip,"hud-boss",7,"#ff7376",spacing:.98f);_boss.style.marginTop=6;_boss.style.paddingTop=5;_boss.style.borderTopWidth=1;_boss.style.borderTopColor=A(255,102,103,.15f);_boss.style.unityTextAlign=TextAnchor.UpperRight;
            _parent.RegisterCallback<GeometryChangedEvent>(Resize);Layout(_parent.contentRect.width);
        }
        private int Slot(VisualElement picked){for(var e=picked;e!=null;e=e.parent){for(int i=0;i<2;i++)if(e==_formationButtons[i])return i;}return -1;}
        private void CancelDrag(){var pointer=_pointerId;_pointerId=-1;_dragSlot=-1;if(pointer<0)return;foreach(var button in _formationButtons)if(button.HasPointerCapture(pointer))button.ReleasePointer(pointer);}
        private void Resize(GeometryChangedEvent evt)=>Layout(_parent.contentRect.width);
        private void Layout(float width)
        {
            Ready=false;_root.style.opacity=0;_geometryFrame=Time.frameCount;if(width<=0)return;
            var small=width<=700;var panelWidth=small?Mathf.Min(width*.5f-18,255):Mathf.Clamp(width*.31f,164,300);panelWidth=Mathf.Min(panelWidth,width*.5f-(width<=380?12:14));
            _left.style.left=Mathf.Max(8,_safeLeft);_right.style.right=Mathf.Max(8,_safeRight);
            foreach(var panel in new[]{_left,_right}){panel.style.top=Mathf.Max(8,_safeTop);panel.style.width=panelWidth;panel.style.height=small?136:140;}
            _left.style.paddingTop=_left.style.paddingBottom=width<=380?5:8;_left.style.paddingLeft=_left.style.paddingRight=width<=380?5:10;
            _right.style.paddingTop=_right.style.paddingBottom=7;_right.style.paddingLeft=_right.style.paddingRight=9;
            _rightClip.style.paddingTop=_rightClip.style.paddingBottom=7;_rightClip.style.paddingLeft=_rightClip.style.paddingRight=9;
            _tooltip.style.width=Mathf.Min(250,width*.74f);
            foreach(var button in _formationButtons){button.style.paddingTop=button.style.paddingBottom=width<=640?5:6;button.style.paddingLeft=button.style.paddingRight=width<=640?6:7;}
            foreach(var label in _formationNames){float size=width<=640?8:9;label.style.fontSize=size;label.style.height=size*1.5f;SetSpacing(label,width<=640?.64f:.72f,size);}
        }
        public void SetSafeArea(float left,float right,float top){if(left==_safeLeft&&right==_safeRight&&top==_safeTop)return;_safeLeft=left;_safeRight=right;_safeTop=top;Layout(_parent.contentRect.width);}
        private static void Set(Label label,string text){if(label.text!=text)label.text=text??"";}
        public void Bind(State state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeHudView));
            _bound=true;
            Set(_level,state.Level?.ToUpperInvariant());Set(_hp,state.Hp);Set(_resonanceTitle,state.Russian?"РЕЗОНАНС":"RESONANCE");Set(_resonance,state.Resonance);Set(_spheres,state.Spheres);Set(_wave,state.Wave);Set(_linksTitle,state.Russian?"АКТИВНЫЕ СВЯЗИ":"ACTIVE LINKS");Set(_links,state.Links);Set(_noFormation,state.Russian?"ФОРМАЦИЯ НЕ АКТИВНА":"NO FORMATION");Set(_timer,state.Timer);Set(_regionTitle,state.Region);Set(_phase,state.Phase);Set(_pocket,state.Pocket);Set(_boss,state.BossLabel);
            _xpFill.style.width=Length.Percent(state.XpPercent);_hpFill.style.width=Length.Percent(state.HpPercent);_resonanceFill.style.width=Length.Percent(state.ResonancePercent);_resonance.style.color=state.ResonanceReady?C("#ffb84d"):C("#dcecff");_resonanceSurface.SetPaint(state.ResonanceReady?_resonanceReady:_resonanceNormal);
            int count=0;for(int slot=0;slot<2;slot++){
                var item=slot==0?state.Dominant:state.Secondary;var button=_formationButtons[slot];button.style.display=item==null?DisplayStyle.None:DisplayStyle.Flex;_formationIds[slot]=item?.Id;
                if(item==null)continue;count++;Set(_formationNames[slot],item.Name);button.style.opacity=item.Inactive&&slot==0?.48f:slot==1?(button.hasHoverPseudoState?.82f:.72f):1;
                if(slot==0){_formationSurfaces[0].SetPaint(item.Inactive?_inactivePaint:_dominantPaint);_inactiveBorder.style.display=item.Inactive?DisplayStyle.Flex:DisplayStyle.None;button.style.filter=item.Inactive?_inactiveFilters:_noFilters;}
            }
            _formationRow.style.display=count==0?DisplayStyle.None:DisplayStyle.Flex;_noFormation.style.display=count==0?DisplayStyle.Flex:DisplayStyle.None;_plus.style.display=count==2?DisplayStyle.Flex:DisplayStyle.None;
            _region.style.display=state.HasRegion?DisplayStyle.Flex:DisplayStyle.None;_boss.style.display=state.Boss?DisplayStyle.Flex:DisplayStyle.None;
            Radar.Bind(state.PlayerX,state.PlayerY,state.Enemies,state.Pois);
            if(_tooltipId!=null)BindTooltip(_readTooltip(_tooltipId));
        }
        private void OpenTooltip(string id){HideTooltip();if(id==null)return;var tip=_readTooltip(id);if(tip==null||tip.Lines.Count==0)return;_tooltipId=id;BindTooltip(tip);_tooltip.style.display=DisplayStyle.Flex;_tooltip.BringToFront();_hideTimer=_root.schedule.Execute(HideTooltip).StartingIn(5000);}
        private void BindTooltip(Tooltip tip)
        {
            if(tip==null)return;Set(_tooltipTitle,tip.Title);Set(_tooltipEffect,tip.Effectiveness);var color=tip.Inactive?C("#ff8a9f"):tip.Secondary?C("#79dcff"):C("#ffcf7a");_tooltipTitle.style.color=_tooltipEffect.style.color=color;
            _tooltipHeading.style.borderBottomColor=tip.Inactive?A(255,77,112,.25f):tip.Secondary?A(121,220,255,.18f):A(143,223,255,.12f);
            while(_tooltipRows.Count<tip.Lines.Count){var line=Text(_tooltipLines,"hud-tooltip-line-"+_tooltipRows.Count,8,"#7f9bb8","400",1.45);line.enableRichText=true;line.style.whiteSpace=WhiteSpace.Normal;line.style.height=StyleKeyword.Auto;line.style.marginTop=2;_tooltipRows.Add(line);}
            for(int i=0;i<_tooltipRows.Count;i++){var line=_tooltipRows[i];line.style.display=i<tip.Lines.Count?DisplayStyle.Flex:DisplayStyle.None;if(i<tip.Lines.Count){var value=tip.Lines[i];Set(line,"<color="+(tip.Secondary?"#79dcff":"#ffcf7a")+"><font-weight=900><noparse>"+value.Active+"</noparse></font-weight></color><color=#7f9bb8bd><noparse>"+value.Rest+"</noparse></color>");}}
        }
        public void HideTooltip(){_hideTimer?.Pause();_hideTimer=null;_tooltipId=null;_tooltip.style.display=DisplayStyle.None;}
        public void Prepare(float scale){if(_disposed)throw new ObjectDisposedException(nameof(NativeHudView));if(!_bound||_parent.contentRect.width<=0)return;foreach(var surface in _surfaces)surface.Prepare(scale);Radar.Prepare(scale);if(Time.frameCount-_geometryFrame<2)return;if(!Ready){Ready=true;_root.style.opacity=1;}}
        public void Dispose(){if(_disposed)return;_disposed=true;CancelDrag();HideTooltip();_parent.UnregisterCallback<GeometryChangedEvent>(Resize);Radar.Dispose();foreach(var surface in _surfaces)surface.Dispose();_root.RemoveFromHierarchy();if(Application.isPlaying){UnityEngine.Object.Destroy(_saturation);UnityEngine.Object.Destroy(_filterMaterial);}else{UnityEngine.Object.DestroyImmediate(_saturation);UnityEngine.Object.DestroyImmediate(_filterMaterial);}}
    }
}
