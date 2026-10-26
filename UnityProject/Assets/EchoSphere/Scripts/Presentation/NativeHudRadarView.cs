using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // App.tsx RadarHud: native retained UI, shader fills and Gaussian shadows.
    // Input coordinates are the existing renderer's logical source pixels.
    public sealed class NativeHudRadarView : IDisposable
    {
        public readonly struct Enemy
        {
            public readonly double X,Y;public readonly bool Alive,Boss,Elite;
            public Enemy(double x,double y,bool alive,bool boss,bool elite){X=x;Y=y;Alive=alive;Boss=boss;Elite=elite;}
        }
        public readonly struct Poi
        {
            public readonly double X,Y;public readonly bool Alive;public readonly string Type;
            public Poi(double x,double y,bool alive,string type){X=x;Y=y;Alive=alive;Type=type;}
        }
        private sealed class Dot
        {
            internal VisualElement Element;
            internal NativeHudSurface Surface;
            internal NativeHudPoiArrow Arrow;
        }
        private readonly VisualElement _root,_content;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly Dot[] _enemies=new Dot[40],_pois=new Dot[12];
        private readonly NativeMenuBrush.Paint[] _enemyPaint=new NativeMenuBrush.Paint[3],_poiPaint=new NativeMenuBrush.Paint[3];
        private readonly Color[] _poiColors={C("#55e6c1"),C("#ff6b6b"),C("#ffb84d")};
        private readonly List<FilterFunction>[] _arrowFilters=new List<FilterFunction>[3];
        private readonly FilterFunctionDefinition[] _shadowDefinitions=new FilterFunctionDefinition[3];
        private readonly Material _shadowMaterial;
        private bool _disposed;
        public VisualElement Root=>_root;
        public int VisibleEnemies {get;private set;}
        public int VisiblePois {get;private set;}
        private static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
        private static NativeMenuBrush.Paint Glow(Color color,float blur,float alpha,bool ellipse=true)=>new NativeMenuBrush.Paint {Fill=color,Ellipse=ellipse,OuterA=new NativeMenuBrush.Shadow(blur,Vector2.zero,new Color(color.r,color.g,color.b,alpha))};
        public NativeHudRadarView(VisualElement parent)
        {
            _root=new VisualElement {name="native-hud-radar",pickingMode=PickingMode.Ignore};
            _root.style.width=_root.style.height=62;_root.style.flexShrink=0;
            _root.style.borderTopLeftRadius=_root.style.borderTopRightRadius=_root.style.borderBottomLeftRadius=_root.style.borderBottomRightRadius=4;
            Border(_root,1);parent.Add(_root);
            // Background, border and outer shadow paint before overflow clipping.
            // Unity clips the retained contents in the inner padding box.
            _content=new VisualElement {name="radar-clip",pickingMode=PickingMode.Ignore};_content.style.position=Position.Absolute;
            _content.style.left=_content.style.right=_content.style.top=_content.style.bottom=0;_content.style.overflow=Overflow.Hidden;
            _content.style.borderTopLeftRadius=_content.style.borderTopRightRadius=_content.style.borderBottomLeftRadius=_content.style.borderBottomRightRadius=3;_root.Add(_content);
            AddSurface(_root,new NativeMenuBrush.Paint {Fill=new Color(1/255f,6/255f,13/255f,.92f),Radius=4,Border=1,BorderColor=new Color(100/255f,209/255f,1,.44f),Colors=new[]{new Color(62/255f,155/255f,1,.13f),new Color(2/255f,8/255f,16/255f,.9f)},Stops=new[]{0f,.68f},Inset=new NativeMenuBrush.Shadow(18,Vector2.zero,new Color(53/255f,190/255f,1,.07f)),OuterA=new NativeMenuBrush.Shadow(18,Vector2.zero,new Color(53/255f,190/255f,1,.05f))});
            for(int i=0;i<2;i++){
                var ring=Absolute("radar-ring-"+i);var inset=i==0?18f:36f;ring.style.left=ring.style.right=ring.style.top=ring.style.bottom=Length.Percent(inset);Border(ring,1);
                AddSurface(ring,new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=new Color(113/255f,218/255f,1,i==0?.17f:.12f)});
            }
            var horizontal=Absolute("radar-cross-h");horizontal.style.left=horizontal.style.right=Length.Percent(8);horizontal.style.top=Length.Percent(50);horizontal.style.height=1;horizontal.style.backgroundColor=new Color(113/255f,218/255f,1,.11f);
            var vertical=Absolute("radar-cross-v");vertical.style.top=vertical.style.bottom=Length.Percent(8);vertical.style.left=Length.Percent(50);vertical.style.width=1;vertical.style.backgroundColor=new Color(113/255f,218/255f,1,.11f);
            _enemyPaint[0]=Glow(C("#6be9ff"),5,.7f);_enemyPaint[1]=Glow(C("#e66cff"),7,.72f);_enemyPaint[2]=Glow(C("#ff6a5f"),10,.78f);
            _shadowMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceUiFilters"));
            for(int i=0;i<3;i++){
                var color=_poiColors[i];_poiPaint[i]=Glow(color,7,1,false);
                var definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();definition.name="Native radar arrow shadow "+i;definition.filterName="native-radar-shadow-"+i;definition.parameters=Array.Empty<FilterParameterDeclaration>();
                definition.passes=new[]{new PostProcessingPass {material=_shadowMaterial,passIndex=2,computeRequiredReadMarginsCallback=f=>new PostProcessingMargins {left=16,right=16,top=16,bottom=16},computeRequiredWriteMarginsCallback=f=>new PostProcessingMargins {left=16,right=16,top=16,bottom=16},applySettingsCallback=(properties,context)=>{properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);properties.SetFloat("_OffsetX",0);properties.SetFloat("_OffsetY",0);properties.SetFloat("_Sigma",4);properties.SetVector("_CssColor",color);NativeGaussianWeights.BindUiShadow(properties,4*context.scaledPixelsPerPoint);}}};
                _shadowDefinitions[i]=definition;_arrowFilters[i]=new List<FilterFunction>{new FilterFunction(definition)};
            }
            for(int i=0;i<_enemies.Length;i++)_enemies[i]=CreateDot("radar-enemy-"+i,_enemyPaint[0]);
            for(int i=0;i<_pois.Length;i++){var dot=CreateDot("radar-poi-"+i,_poiPaint[0]);dot.Arrow=new NativeHudPoiArrow();dot.Element.Add(dot.Arrow);_pois[i]=dot;}
            var player=CreateDot("radar-player",Glow(Color.white,9,1));player.Surface.SetPaint(new NativeMenuBrush.Paint {Ellipse=true,Fill=Color.white,OuterA=new NativeMenuBrush.Shadow(9,Vector2.zero,new Color(107/255f,233/255f,1,.95f))});Place(player,50,50,4,0);
        }
        private static void Border(VisualElement e,float width){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=width;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;}
        private VisualElement Absolute(string name){var e=new VisualElement {name=name,pickingMode=PickingMode.Ignore};e.style.position=Position.Absolute;_content.Add(e);return e;}
        private NativeHudSurface AddSurface(VisualElement e,NativeMenuBrush.Paint paint){var surface=new NativeHudSurface(e,paint);_surfaces.Add(surface);return surface;}
        private Dot CreateDot(string name,NativeMenuBrush.Paint paint){var e=Absolute(name);e.style.display=DisplayStyle.None;return new Dot {Element=e,Surface=AddSurface(e,paint)};}
        private static void Place(Dot dot,double x,double y,float size,float angle)
        {
            var s=dot.Element.style;s.display=DisplayStyle.Flex;s.left=Length.Percent((float)x);s.top=Length.Percent((float)y);s.width=s.height=size;
            s.translate=new Translate(Length.Percent(-50),Length.Percent(-50));s.rotate=new Rotate(new Angle(angle,AngleUnit.Radian));
        }
        public void Bind(double playerX,double playerY,IReadOnlyList<Enemy> enemies,IReadOnlyList<Poi> pois)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeHudRadarView));
            int cursor=0;
            for(int i=0;i<enemies.Count&&cursor<40;i++){
                var enemy=enemies[i];if(!enemy.Alive)continue;var dx=enemy.X-playerX;var dy=enemy.Y-playerY;
                var angle=Math.Atan2(dy,dx);var radius=Math.Min(1,Math.Sqrt(dx*dx+dy*dy)/900)*25;int kind=enemy.Boss?2:enemy.Elite?1:0;
                var dot=_enemies[cursor++];dot.Surface.SetPaint(_enemyPaint[kind]);Place(dot,50+Math.Cos(angle)*radius,50+Math.Sin(angle)*radius,kind==2?5:kind==1?3:2,0);
            }
            VisibleEnemies=cursor;while(cursor<40)_enemies[cursor++].Element.style.display=DisplayStyle.None;
            cursor=0;
            for(int i=0;i<pois.Count&&cursor<12;i++){
                var poi=pois[i];if(!poi.Alive)continue;var dx=poi.X-playerX;var dy=poi.Y-playerY;var distance=Math.Sqrt(dx*dx+dy*dy);var angle=Math.Atan2(dy,dx);var outside=distance>900;var radius=outside?42:Math.Min(25,distance/900*25);
                int color=poi.Type=="boss_trace"?2:poi.Type=="elite_nest"||poi.Type=="rupture"?1:0;
                var dot=_pois[cursor++];dot.Surface.SetPaint(_poiPaint[color]);dot.Surface.SetVisible(!outside);dot.Arrow.style.display=outside?DisplayStyle.Flex:DisplayStyle.None;if(outside)dot.Arrow.Set(_poiColors[color],_arrowFilters[color]);Place(dot,50+Math.Cos(angle)*radius,50+Math.Sin(angle)*radius,outside?10:7,(float)(outside?angle+Math.PI/2:Math.PI/4));
            }
            VisiblePois=cursor;while(cursor<12)_pois[cursor++].Element.style.display=DisplayStyle.None;
        }
        public void Prepare(float scale){foreach(var surface in _surfaces)surface.Prepare(scale);}
        public void Dispose(){if(_disposed)return;_disposed=true;_root.RemoveFromHierarchy();foreach(var surface in _surfaces)surface.Dispose();foreach(var definition in _shadowDefinitions)DestroyOwned(definition);DestroyOwned(_shadowMaterial);}
        private static void DestroyOwned(UnityEngine.Object value){if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
    }
}
