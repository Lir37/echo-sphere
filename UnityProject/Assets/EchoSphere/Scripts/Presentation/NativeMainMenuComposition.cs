using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Typed menu composition. Unity owns retained layout, text, images, meshes
    // and GPU passes. Native image/solid prefixes and Gaussian passes run before
    // UI rendering; no DOM projection or recursive filter replicas are needed.
    public sealed class NativeMainMenuComposition : IDisposable
    {
        private static readonly ProfilerMarker PrepareMarker=new ProfilerMarker("EchoSphere.NativeMenu.PrepareComposition");
        private readonly NativeMainMenuView _view;
        private readonly NativeMainMenuBackground _background;
        private readonly NativeMenuArtwork _artwork;
        private readonly NativeMenuBrush _avatar=new NativeMenuBrush();
        private readonly Image _avatarImage;
        private readonly Label _avatarGlyph;
        private readonly Image _characterPortrait;
        private string _portraitId;
        private readonly List<Surface> _surfaces=new List<Surface>();
        private readonly List<VisualElement> _observed=new List<VisualElement>();
        private float _scale,_avatarWidth,_avatarHeight;
        private Color _characterColor;
        private int _dirtyFrames=2;
        private int _geometryFrame=-10;
        private bool _disposed;
        public int PrepareCount {get;private set;}
        public NativeMenuArtwork Artwork=>_artwork;
        public NativeMainMenuBackground Background=>_background;
        private enum Kind {Loadout,Character,Difficulty,Continue,Navigation,NavigationButton}
        private sealed class Surface
        {
            internal VisualElement Element,Root;
            internal Kind Kind;
            internal float Sigma,Width,Height,Scale;
            internal bool Hover,Selected,Painted;
            internal readonly NativeMenuBrush Brush=new NativeMenuBrush();
            internal NativeMenuGlass Glass;
            internal Image Image;
        }
        public NativeMainMenuComposition(NativeMainMenuView view)
        {
            _view=view??throw new ArgumentNullException(nameof(view));
            // Observe original nodes, never the compositor's replica helpers.
            Observe(view.Root);
            ApplyTypography(view.Root);
            _background=new NativeMainMenuBackground(view.Background);
            _artwork=new NativeMenuArtwork(view.Artwork);
            _avatarImage=new Image {name="native-menu-avatar-image",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};
            _avatarImage.style.position=Position.Absolute;view.Avatar.Add(_avatarImage);
            _avatarGlyph=new Label("✦") {name="native-menu-avatar-glyph",pickingMode=PickingMode.Ignore};
            _avatarGlyph.style.position=Position.Absolute;_avatarGlyph.style.left=_avatarGlyph.style.right=_avatarGlyph.style.top=_avatarGlyph.style.bottom=0;
            _avatarGlyph.style.fontSize=16;_avatarGlyph.style.letterSpacing=.192f;_avatarGlyph.style.color=Color.white;_avatarGlyph.style.unityTextAlign=TextAnchor.MiddleCenter;
            Font(_avatarGlyph,"400",1.5);view.Avatar.Add(_avatarGlyph);
            _characterPortrait=new Image {name="native-menu-character-portrait",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.ScaleToFit};
            _characterPortrait.style.position=Position.Absolute;_characterPortrait.style.left=_characterPortrait.style.right=_characterPortrait.style.top=_characterPortrait.style.bottom=0;view.Avatar.Add(_characterPortrait);
            Add(view.Root.Q(className:"loadout"),Kind.Loadout,0,view.Root);
            Add(view.Root.Q("open-character"),Kind.Character,8,view.Root);
            foreach(var id in new[]{"easy","normal","hard","nightmare"})Add(view.Root.Q("difficulty-"+id),Kind.Difficulty,8,view.Root);
            Add(view.Root.Q("continue-run"),Kind.Continue,8,view.Root);
            Add(view.Navigation,Kind.Navigation,12,view.Root);
            for(int i=0;i<6;i++)Add(view.Root.Q("nav-"+i),Kind.NavigationButton,8,view.Navigation);
        }
        public void SetCharacter(string id)
        {
            if(_portraitId==id)return;_portraitId=id;
            var art=NativeCharacterArtwork.Resolve(id);_characterPortrait.sprite=art?.Sprite;
            _avatarGlyph.style.display=art==null?DisplayStyle.Flex:DisplayStyle.None;
        }
        private void Observe(VisualElement node)
        {
            _observed.Add(node);node.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            if(node is Button){node.RegisterCallback<PointerEnterEvent>(PointerChanged);node.RegisterCallback<PointerLeaveEvent>(PointerChanged);}
            for(int i=0;i<node.hierarchy.childCount;i++)Observe(node.hierarchy[i]);
        }
        private void GeometryChanged(GeometryChangedEvent evt){_geometryFrame=Time.frameCount;Invalidate();}
        private void PointerChanged(EventBase evt)=>Invalidate();
        public void Invalidate(){if(!_disposed)_dirtyFrames=2;}
        private void Add(VisualElement element,Kind kind,float sigma,VisualElement root)
        {
            if(element==null)throw new InvalidOperationException("Native menu surface missing: "+kind);
            var surface=new Surface {Element=element,Kind=kind,Sigma=sigma,Root=root};_surfaces.Add(surface);
            element.style.backgroundColor=Color.clear;element.style.borderLeftColor=element.style.borderRightColor=element.style.borderTopColor=element.style.borderBottomColor=Color.clear;
            surface.Image=new Image {pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};surface.Image.style.position=Position.Absolute;element.hierarchy.Insert(0,surface.Image);
            if(sigma>0)surface.Glass=new NativeMenuGlass(element,sigma);
        }
        // Called from the owner before rendering, after model binding. Never
        // from generateVisualContent or a filter callback.
        public bool Prepare(float scale,Color characterColor)
        {
            // Yoga may need a second pass after responsive classes change.
            // Never mix old child geometry with a newly sized viewport.
            if(Time.frameCount-_geometryFrame<=1)return false;
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMainMenuComposition));
            var size=_view.Root.layout;var avatar=_view.Avatar.layout;
            if(size.width<=0||size.height<=0||avatar.width<=0||avatar.height<=0||!float.IsFinite(size.width+size.height+avatar.width+avatar.height))return false;
            using var sample=PrepareMarker.Auto();
            int backgroundCount=_background.PaintCount,artCount=_artwork.PaintCount;
            _background.SetViewport(size.width,size.height,scale);if(!_artwork.Prepare(scale))return false;
            if(backgroundCount!=_background.PaintCount||artCount!=_artwork.PaintCount||_scale!=scale)Invalidate();
            if(_avatarWidth!=avatar.width||_avatarHeight!=avatar.height||_scale!=scale||_characterColor!=characterColor){
                _avatar.Render(avatar.width,avatar.height,scale,NativeMenuArtwork.AvatarPaint(characterColor));
                _avatarImage.image=_avatar.Texture;var r=_avatar.Bounds;_avatarImage.style.left=r.x;_avatarImage.style.top=r.y;_avatarImage.style.width=r.width;_avatarImage.style.height=r.height;
                _avatarWidth=avatar.width;_avatarHeight=avatar.height;_characterColor=characterColor;Invalidate();
            }
            _scale=scale;
            foreach(var surface in _surfaces){
                var box=surface.Element.layout;if(!float.IsFinite(box.width+box.height)||box.width<=0||box.height<=0||surface.Element.resolvedStyle.display==DisplayStyle.None)continue;
                bool hover=surface.Element.hasHoverPseudoState,selected=surface.Element.ClassListContains("is-active");
                if(!surface.Painted||surface.Width!=box.width||surface.Height!=box.height||surface.Scale!=scale||surface.Hover!=hover||surface.Selected!=selected){
                    surface.Brush.Render(box.width,box.height,scale,Paint(surface.Kind,hover,selected));
                    surface.Width=box.width;surface.Height=box.height;surface.Scale=scale;surface.Hover=hover;surface.Selected=selected;surface.Painted=true;Invalidate();
                    if(surface.Image!=null){var r=surface.Brush.Bounds;var border=surface.Element.resolvedStyle;surface.Image.image=surface.Brush.Texture;surface.Image.style.left=r.x-border.borderLeftWidth;surface.Image.style.top=r.y-border.borderTopWidth;surface.Image.style.width=r.width;surface.Image.style.height=r.height;}
                }
            }
            if(_dirtyFrames<=0)return true;
            _dirtyFrames--;PrepareCount++;
            foreach(var surface in _surfaces)if(surface.Glass!=null&&surface.Painted){
                surface.Glass.Prepare(surface.Root,surface.Sigma,scale);
            }
            return true;
        }
        public void SetVisible(bool visible){foreach(var surface in _surfaces)surface.Glass?.Visible(visible);if(visible)Invalidate();}
        private static Color R(float r,float g,float b,float a=1)=>new Color(r/255,g/255,b/255,a);
        private static NativeMenuBrush.Shadow Shadow(float blur,float x,float y,Color color)=>new NativeMenuBrush.Shadow(blur,new Vector2(x,y),color);
        private static NativeMenuBrush.Paint Paint(Kind kind,bool hover,bool selected)
        {
            var p=new NativeMenuBrush.Paint();
            switch(kind){
                case Kind.Loadout:p.Radius=8;p.Border=1;p.BorderColor=R(86,177,226,.15f);p.Fill=R(4,11,19,.70f);p.Inset=Shadow(25,0,0,R(36,123,172,.035f));break;
                case Kind.Character:p.Radius=4;p.Border=1;p.BorderColor=hover?R(91,220,255,.44f):R(77,181,235,.18f);p.Fill=R(5,15,26,.78f);p.Inset=Shadow(0,0,1,R(170,235,255,.025f));p.OuterA=Shadow(30,0,12,R(0,0,0,.18f));break;
                case Kind.Difficulty:p.Radius=4;p.Border=1;p.BorderColor=selected?R(80,213,255,.43f):R(82,170,220,.13f);p.Fill=selected?R(32,106,135,.25f):R(8,18,30,.76f);if(selected)p.Inset=Shadow(14,0,0,R(75,208,255,.06f));break;
                case Kind.Continue:p.Radius=7;p.Border=1;p.BorderColor=hover?R(107,230,255,.68f):R(99,222,255,.42f);p.Linear=true;p.Colors=hover?new[]{R(37,130,158,.36f),R(10,35,53,.68f)}:new[]{R(37,111,140,.28f),R(8,27,42,.60f)};p.Stops=new[]{0f,1f};p.OuterA=Shadow(18,0,0,R(69,206,255,.08f));p.Inset=Shadow(15,0,0,R(92,221,255,.04f));break;
                case Kind.Navigation:p.Border=1;p.TopBorderOnly=true;p.BorderColor=R(76,162,211,.13f);p.Fill=R(2,7,13,.88f);p.Inset=Shadow(0,0,1,R(170,235,255,.025f));p.OuterA=Shadow(30,0,12,R(0,0,0,.18f));break;
                case Kind.NavigationButton:p.Radius=5;p.Border=1;p.BorderColor=hover?R(77,201,247,.18f):Color.clear;p.Fill=hover?R(31,89,119,.12f):Color.clear;break;
            }
            return p;
        }
        private const string Family="Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif";
        private static void Font(VisualElement element,string weight,double line,string family=Family)
        {var font=SourceUiTypography.Resolve(family,weight,false,line);if(font!=null)element.style.unityFontDefinition=FontDefinition.FromSDFFont(font);element.style.unityFontStyleAndWeight=FontStyle.Normal;}
        private static void ApplyTypography(VisualElement root)
        {
            Font(root,"400",1.5);root.style.fontSize=16;root.style.letterSpacing=.192f;
            Font(root.Q("menu-logo"),"500",1.5);Font(root.Q("core-title"),"900",1.5);
            Font(root.Q("core-caption"),"700",1,"system-ui,sans-serif");Font(root.Q("core-copy"),"400",1.45);
            Font(root.Q("character-name"),"700",1.5);Font(root.Q("continue-title"),"900",1.5);
            Font(root.Q(className:"name-row"),"700",double.NaN,"system-ui, sans-serif");
            root.Q("menu-logo").style.textShadow=new TextShadow {color=R(79,211,255,.2f),blurRadius=22,offset=Vector2.zero};
            root.Q("core-title").style.textShadow=new TextShadow {color=R(92,220,255,.12f),blurRadius=18,offset=Vector2.zero};
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
            foreach(var node in _observed){node.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);if(node is Button){node.UnregisterCallback<PointerEnterEvent>(PointerChanged);node.UnregisterCallback<PointerLeaveEvent>(PointerChanged);}}
            foreach(var surface in _surfaces){surface.Glass?.Dispose();surface.Image?.RemoveFromHierarchy();surface.Brush.Dispose();}
            _avatarImage.RemoveFromHierarchy();_avatarGlyph.RemoveFromHierarchy();_avatar.Dispose();_artwork.Dispose();_background.Dispose();
        }
    }
}
