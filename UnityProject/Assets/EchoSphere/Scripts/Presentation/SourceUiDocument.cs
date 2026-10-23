#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Profiling;

namespace EchoSphere.Presentation
{
    public sealed class SourceUiDocument : MonoBehaviour,INativeUiBackdrops
    {
        // These samples expose hot paths in the Unity Profiler without altering
        // the source UI's update cadence or adding frame-driven allocations.
        private static readonly ProfilerMarker UiBrushesMarker=new ProfilerMarker("EchoSphere.UI.PrepareBrushesAndSvg");
        private static readonly ProfilerMarker UiDepthMarker=new ProfilerMarker("EchoSphere.UI.PrepareDepth");
        private static readonly ProfilerMarker UiBackdropMarker=new ProfilerMarker("EchoSphere.UI.PrepareBackdrops");
        private static readonly ProfilerMarker UiSvgMarker=new ProfilerMarker("EchoSphere.UI.RasterizeSvg");
        private static readonly ProfilerMarker UiCssMarker=new ProfilerMarker("EchoSphere.UI.RasterizeCss");
        private static readonly ProfilerMarker UiBuildMarker=new ProfilerMarker("EchoSphere.UI.BuildTree");
        private static readonly ProfilerMarker UiProjectMarker=new ProfilerMarker("EchoSphere.UI.ProjectTree");
        private static readonly ProfilerMarker UiCssNodesMarker=new ProfilerMarker("EchoSphere.UI.BuildCssNodes");
        private static readonly ProfilerMarker UiInteractionMarker=new ProfilerMarker("EchoSphere.UI.CopyInteraction");
        private static readonly ProfilerMarker UiResolveCssMarker=new ProfilerMarker("EchoSphere.UI.ResolveCss");
        private static readonly ProfilerMarker UiReconcileMarker=new ProfilerMarker("EchoSphere.UI.Reconcile");
        private static readonly ProfilerMarker UiLayoutMarker=new ProfilerMarker("EchoSphere.UI.Layout");
        private static readonly ProfilerMarker UiFlowMarker=new ProfilerMarker("EchoSphere.UI.FlowOrder");
        private sealed class Entry
        {
            public VisualCssNode Node;
            public VisualElement Element;
            public SourceUiFrame Frame;
            public SourceUiElement Host;
            public readonly VisualCssAnimation Motion=new VisualCssAnimation();
            public readonly Dictionary<string,string> BaseStyle=new Dictionary<string,string>();
            // Reused each animation tick instead of allocating one dictionary per element per frame.
            public readonly Dictionary<string,string> MotionStyleSnapshot=new Dictionary<string,string>();
            public readonly HashSet<int> ActivePointers=new HashSet<int>();
            public string PaintKey,SvgKey;
            public Texture2D Paint;
            public Texture Svg;
            public readonly List<Label> SvgTexts=new List<Label>();
            public readonly List<VisualCssNode> SvgTextNodes=new List<VisualCssNode>();
            public readonly List<FilterFunction> EmptyFilters=new List<FilterFunction>();
            public byte[] PaintUploadPixels;
            public byte[] DepthClipUploadPixels;
            public readonly List<SourceUiDepth.PlaneImage> DepthImages=new List<SourceUiDepth.PlaneImage>();
            public readonly List<SourceUiDepth.PlaneImage> DepthImagePool=new List<SourceUiDepth.PlaneImage>();
            public int DepthImageCursor;
            public SourceUiDepth.PlaneImage RentDepthImage()
            {
                if(DepthImageCursor==DepthImagePool.Count)DepthImagePool.Add(new SourceUiDepth.PlaneImage());
                return DepthImagePool[DepthImageCursor++];
            }
            public Rect PaintRect;
            public Rect SvgRect;
            public SourceUiDepth Depth;
            public Texture DepthTexture;
            public string DepthKey;
            public Rect DepthRect;
            public SourceUiSurfaceFilters SurfaceFilters;
            public Texture2D GroupClip;
            public string GroupClipKey;
            public SourceUiFilters Filters;
            public SourceUiBackdrop Backdrop;
            public Action<MeshGenerationContext> PaintContent;
            public bool Used;
        }
        private UIDocument _document;
        private bool _presentationSuspended,_presentationWasEnabled;
        private StyleFloat _presentationOpacity;
        private StyleEnum<DisplayStyle> _presentationDisplay;
        private VisualElement _configuredRoot;
        public void SetPresentationSuspended(bool suspended)
        {
            if(_presentationSuspended==suspended)return;
            if(!suspended){_document.enabled=true;ConfigureDocumentRoot();}
            var root=_document.rootVisualElement;
            if(suspended){
                _presentationOpacity=root.style.opacity;_presentationDisplay=root.style.display;_presentationWasEnabled=root.enabledSelf;
                // An opaque native replacement owns all pixels/input here;
                // exclude this hidden tree from rendering while hooks tick.
                root.style.display=DisplayStyle.None;root.style.opacity=0;root.SetEnabled(false);
                OnApplicationFocus(false);
                // Removing the UIDocument releases its retained filter render
                // chain. Merely hiding its root leaves offscreen GPU work alive.
                _document.enabled=false;
            }else{
                root.style.display=_presentationDisplay;root.style.opacity=_presentationOpacity;root.SetEnabled(_presentationWasEnabled);Dirty=true;
            }
            _presentationSuspended=suspended;
        }
        private PanelSettings _panel;
        private VisualCss _sheet;
        private SourceUiLayout _layout;
        private readonly SourceUiFlow _flow=new SourceUiFlow();
        private readonly SourceUiReplicaSnapshotCache _replicaSnapshots=new SourceUiReplicaSnapshotCache();
        private readonly Dictionary<string,Entry> _entries=new Dictionary<string,Entry>();
        private readonly Dictionary<VisualElement,Entry> _elementEntries=new Dictionary<VisualElement,Entry>();
        private readonly Dictionary<VisualElement,Entry> _backdropForegrounds=new Dictionary<VisualElement,Entry>();
        private NativeUiBackdropRegistry _nativeBackdrops;
        public IDisposable RegisterNativeBackdrop(VisualElement element,float sigma)
        {if(_nativeBackdrops==null)throw new InvalidOperationException("UI backdrop provider is not initialized");return _nativeBackdrops.RegisterNativeBackdrop(element,sigma);}
        // Reuse the removal buffer across UI reconciliations to avoid per-pass LINQ arrays.
        private readonly List<string> _staleEntryKeys=new List<string>();
        // Non-native logical nodes share this scratch buffer during depth-first
        // motion updates; no nested call occurs before each comparison completes.
        private readonly Dictionary<string,string> _motionFallbackSnapshot=new Dictionary<string,string>();
        private readonly List<(VisualElement Element,double Order,double Z,int Index)> _paintOrderScratch=new List<(VisualElement,double,double,int)>();
        private VisualCssNode _root;
        private float _scale=1f;
        private Material _svgMaterial;
        public SourceUiEnvironment Environment {get;private set;}
        public Func<V> Render;
        internal Action<VisualCssNode> BeforePaint {get;set;}
        public Func<Texture> CanvasTexture;
        public Action AfterEvent;
        public bool Dirty=true;
        public bool Continuous;
        // Rebuild the source-derived logical tree at a bounded cadence while
        // keeping CSS animation, layout and paint-resource work frame-driven.
        public float RefreshInterval;
        private double _nextRenderAt;
        internal int FrameVersion {get;private set;}
        internal float Scale=>_scale;
        public void Initialize()
        {
            _nativeBackdrops=new NativeUiBackdropRegistry(ReplicaPaint,_replicaSnapshots);
            _sheet=new VisualCss(VisualJson.Parse(Resources.Load<TextAsset>("EchoSphere/SourceUiStyles").text));_layout=new SourceUiLayout(_sheet);
            VisualSourceIcons.Data=VisualJson.Parse(Resources.Load<TextAsset>("EchoSphere/SourceUiIcons").text);
            _svgMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceUiVector"));
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Source CSS viewport";_panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1000;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            ConfigureDocumentRoot();
            Environment=new SourceUiEnvironment(this);
            VisualDom.Changed=()=>Dirty=true;
        }
        private void ConfigureDocumentRoot()
        {
            var root=_document.rootVisualElement;if(root==null||ReferenceEquals(root,_configuredRoot))return;_configuredRoot=root;
            root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;root.pickingMode=PickingMode.Ignore;
            root.RegisterCallback<PointerDownEvent>(evt=>{if(evt.button==0)BeginActivePointer(evt.target as VisualElement,evt.pointerId);},TrickleDown.TrickleDown);
            root.RegisterCallback<PointerUpEvent>(evt=>EndActivePointer(evt.pointerId),TrickleDown.TrickleDown);
            root.RegisterCallback<PointerCancelEvent>(evt=>EndActivePointer(evt.pointerId),TrickleDown.TrickleDown);
        }
        public void Draw(float scale)
        {
            _scale=Mathf.Max(.1f,scale);_panel.scale=_scale;
            var width=Screen.width/_scale;var height=Screen.height/_scale;
            if(_sheet.Width!=width||_sheet.Height!=height){_sheet.Width=width;_sheet.Height=height;Dirty=true;Environment.Dispatch("resize");}
            var safe=Screen.safeArea;_sheet.SafeLeft=safe.xMin/_scale;_sheet.SafeRight=(Screen.width-safe.xMax)/_scale;_sheet.SafeTop=(Screen.height-safe.yMax)/_scale;_sheet.SafeBottom=safe.yMin/_scale;
            Environment.Tick(Time.unscaledTimeAsDouble*1000d);
            // An opaque native screen owns presentation and input. Keep hooks,
            // timers and viewport events alive without rebuilding hidden paint.
            if(_presentationSuspended)return;
            if(Render==null)return;
            var now=Time.unscaledTimeAsDouble;
            if(!Dirty&&!Continuous&&_root!=null&&RefreshInterval>0f&&now>=_nextRenderAt)Dirty=true;
            if(!Dirty&&!Continuous&&_root!=null){AdvanceMotion(_root);using(UiLayoutMarker.Auto())UpdateLayout(_root);PreparePaintResources(_root);_layout.Tracks(_root);return;}
            // Reconcile source state only when dirty or the configured projection
            // interval elapses. CSS motion/layout/paint still advance every frame.
            FrameVersion++;Dirty=false;_nextRenderAt=now+Math.Max(0d,RefreshInterval);VisualDom.BeginFrame();VisualCssNode nextRoot;
            using(UiBuildMarker.Auto()){
                V tree;using(UiProjectMarker.Auto())tree=Render();
                using(UiCssNodesMarker.Auto())nextRoot=VisualCssNode.Build(tree);
                using(UiInteractionMarker.Auto())CopyInteraction(_root,nextRoot);
                _root=nextRoot;using(UiResolveCssMarker.Auto())_sheet.Apply(_root);
            }
            foreach(var entry in _entries.Values)entry.Used=false;
            using(UiReconcileMarker.Auto())Reconcile(_root,_document.rootVisualElement,"root");
            _staleEntryKeys.Clear();
            foreach(var pair in _entries)if(!pair.Value.Used)_staleEntryKeys.Add(pair.Key);
            foreach(var key in _staleEntryKeys){var entry=_entries[key];Dispose(entry);_entries.Remove(key);}
            VisualDom.EndFrame();
            using(UiLayoutMarker.Auto())UpdateLayout(_root);PreparePaintResources(_root);_layout.Tracks(_root);
        }
        private void Reconcile(VisualCssNode node,VisualElement parent,string path)
        {
            var label=node.Tag=="#text"||SourceUiInline.CanMerge(_sheet,node);
            var scrollX=_sheet.Get(node,"overflow-x")=="auto"||_sheet.Get(node,"overflow")=="auto";var scrollY=_sheet.Get(node,"overflow-y")=="auto"||_sheet.Get(node,"overflow")=="auto";var scroll=scrollX||scrollY;
            var kind=node.Tag=="input"?node.Properties["type"].Text=="range"?"range":"input":scroll?"scroll":label?"label":"box";
            var key=path+":"+node.Tag+":"+kind;
            if(!_entries.TryGetValue(key,out var entry)) {
                var element=kind=="input"?(VisualElement)new TextField {isDelayed=false}:kind=="range"?new Slider():kind=="scroll"?new ScrollView(scrollX?scrollY?ScrollViewMode.VerticalAndHorizontal:ScrollViewMode.Horizontal:ScrollViewMode.Vertical):kind=="label"?new Label():new VisualElement();
                entry=new Entry {Element=element,Frame=new SourceUiFrame(element)};entry.Host=new SourceUiElement(this,node,element);_entries.Add(key,entry);_elementEntries.Add(element,entry);_elementEntries.Add(entry.Frame,entry);element.userData=entry.Host;entry.Frame.userData=entry.Host;
                InstallEvents(entry);
                entry.PaintContent=context=>Paint(entry,context);
                entry.Frame.generateVisualContent=entry.PaintContent;
                entry.Frame.RegisterCallback<GeometryChangedEvent>(evt=>{UpdateBounds(entry.Node,entry.Frame);FrameVersion++;});
            }
            entry.Used=true;entry.Node=node;entry.Host.Node=node;node.Native=entry.Frame;node.Active=entry.ActivePointers.Count>0;
            if(entry.Element is ScrollView scrolling)scrolling.mode=scrollX?scrollY?ScrollViewMode.VerticalAndHorizontal:ScrollViewMode.Horizontal:ScrollViewMode.Vertical;
            UpdateBounds(node,entry.Frame);if(node.Parent==null){node.Width=_sheet.Width;node.Height=_sheet.Height;}
            if(entry.Frame.parent!=parent)parent.Add(entry.Frame);
            entry.BaseStyle.Clear();foreach(var pair in node.Style)entry.BaseStyle[pair.Key]=pair.Value;
            node.InheritAnimatedValues();entry.Motion.Apply(_sheet,node,Time.unscaledTimeAsDouble);
            if(label||node.Tag=="svg")InheritSubtree(node);
            var contentRotation=_layout.Apply(node,entry.Element,entry.Frame.parent);entry.Frame.ApplyOuterStyle(contentRotation);
            if(entry.Element is TextField input){var value=node.Properties["value"];if(!value.Undefined&&input.value!=value.Text)input.SetValueWithoutNotify(value.Text);input.maxLength=node.Properties["maxLength"].Undefined?-1:(int)node.Properties["maxLength"].Number;input.multiline=false;input.textEdition.placeholder=node.Properties["placeholder"].Undefined?"":node.Properties["placeholder"].Text;input.textEdition.hidePlaceholderOnFocus=false;input.textSelection.selectAllOnFocus=false;input.textSelection.selectAllOnMouseUp=false;var textInput=input.Q<VisualElement>("unity-text-input");if(textInput!=null){textInput.style.backgroundColor=Color.clear;textInput.style.paddingLeft=0;textInput.style.paddingRight=0;textInput.style.paddingTop=0;textInput.style.paddingBottom=0;if(input.value.Length==0&&!node.Properties["placeholder"].Undefined){var placeholder=new VisualCssNode {Tag="input",Pseudo="::placeholder",Parent=node};_sheet.Apply(placeholder);var tint=VisualCssColor.Parse(_sheet.Get(placeholder,"color",_sheet.Get(node,"color","#000000")));textInput.style.color=new Color((float)tint.R,(float)tint.G,(float)tint.B,(float)tint.A);}else textInput.style.color=entry.Element.style.color;textInput.style.fontSize=entry.Element.style.fontSize;textInput.style.unityFontDefinition=entry.Element.style.unityFontDefinition;textInput.style.unityFontStyleAndWeight=entry.Element.style.unityFontStyleAndWeight;textInput.style.borderLeftWidth=textInput.style.borderRightWidth=textInput.style.borderTopWidth=textInput.style.borderBottomWidth=0;textInput.style.marginLeft=textInput.style.marginRight=textInput.style.marginTop=textInput.style.marginBottom=0;textInput.style.flexGrow=1;}}
            if(entry.Element is Slider range){range.lowValue=(float)node.Properties["min"].Number;range.highValue=(float)node.Properties["max"].Number;if(!node.Properties["value"].Undefined)range.SetValueWithoutNotify((float)node.Properties["value"].Number);}
            entry.Frame.SetEnabled(!node.Properties["disabled"].Truth);
            var reference=node.Properties["ref"];if(reference.Raw is Dictionary<string,V>)reference["current"]=new V(entry.Host);else if(reference.Raw is Func<V[],V>)reference.Call(new V(entry.Host));
            if(node.Tag=="svg"){SourceUiSvg.Text(node,_sheet,entry.Element,entry.SvgTexts,entry.SvgTextNodes);return;}
            if(label)return;
            var container=entry.Element is ScrollView view?view.contentContainer:entry.Element;var childIndex=0;
            foreach(var child in node.Children){var authoredKey=child.Properties["key"];var part=child.Pseudo??(!authoredKey.Undefined?"key="+authoredKey.Text:(childIndex++).ToString(CultureInfo.InvariantCulture));Reconcile(child,container,key+"/"+part);}
            // CSS order and z-index affect native paint / picking order without
            // changing the authored tree used by selectors and DOM queries.
            _paintOrderScratch.Clear();
            for(var i=0;i<node.Children.Count;i++)
            {
                var child=node.Children[i];
                if(!(child.Native is VisualElement element))continue;
                _paintOrderScratch.Add((element,_sheet.Length(_sheet.Get(child,"order","0"),1d),_sheet.Length(_sheet.Get(child,"z-index","0"),1d),i));
            }
            _paintOrderScratch.Sort((a,b)=>{
                var order=a.Order.CompareTo(b.Order);
                if(order!=0)return order;
                var z=a.Z.CompareTo(b.Z);
                return z!=0?z:a.Index.CompareTo(b.Index);
            });
            foreach(var item in _paintOrderScratch)item.Element.BringToFront();
        }
        private static void CopyInteraction(VisualCssNode before,VisualCssNode after)
        {
            if(before==null||before.Tag!=after.Tag)return;
            after.Hover=before.Hover;after.Active=before.Active;after.Focused=before.Focused;
            for(var index=0;index<after.Children.Count;index++)
            {
                var child=after.Children[index];var key=child.Properties["key"];VisualCssNode match=null;
                if(key.Undefined)
                {
                    var ordinal=0;
                    foreach(var candidate in before.Children)
                    {
                        if(candidate.Pseudo!=null)continue;
                        if(ordinal==index){match=candidate;break;}
                        ordinal++;
                    }
                }
                else
                {
                    foreach(var candidate in before.Children)
                    {
                        if(candidate.Pseudo!=null)continue;
                        if(candidate.Properties["key"]==key){match=candidate;break;}
                    }
                }
                CopyInteraction(match,child);
            }
        }
        private static void InheritSubtree(VisualCssNode node)
        {foreach(var child in node.Children){child.InheritAnimatedValues();InheritSubtree(child);}}
        private void AdvanceMotion(VisualCssNode node)
        {
            // Retained native entries make up the frame-driven UI motion path.
            // Preserve the original comparison, but reuse the snapshot storage.
            Entry entry=null;
            if(node.Native is VisualElement element)_elementEntries.TryGetValue(element,out entry);
            var before=entry!=null?entry.MotionStyleSnapshot:_motionFallbackSnapshot;
            before.Clear();foreach(var pair in node.Style)before[pair.Key]=pair.Value;
            if(entry!=null){
                node.Style.Clear();foreach(var pair in entry.BaseStyle)node.Style[pair.Key]=pair.Value;
                node.InheritAnimatedValues();entry.Motion.Apply(_sheet,node,Time.unscaledTimeAsDouble);
            }else node.InheritAnimatedValues();
            if(before.Count!=node.Style.Count||before.Any(pair=>!node.Style.TryGetValue(pair.Key,out var value)||value!=pair.Value))FrameVersion++;
            foreach(var child in node.Children)AdvanceMotion(child);
        }
        private void InstallEvents(Entry entry)
        {
            var element=entry.Element;
            element.RegisterCallback<PointerDownEvent>(evt=>entry.Host.Send("onPointerDown",evt));
            element.RegisterCallback<PointerMoveEvent>(evt=>entry.Host.Send("onPointerMove",evt));
            element.RegisterCallback<PointerUpEvent>(evt=>{EndActivePointer(evt.pointerId);entry.Host.Send("onPointerUp",evt);});
            element.RegisterCallback<PointerCancelEvent>(evt=>{EndActivePointer(evt.pointerId);entry.Host.Send("onPointerCancel",evt);});
            element.RegisterCallback<ClickEvent>(evt=>entry.Host.Send("onClick",evt));
            element.RegisterCallback<KeyDownEvent>(evt=>entry.Host.Send("onKeyDown",evt));
            element.RegisterCallback<NavigationSubmitEvent>(evt=>{if(entry.Node.Tag=="button"&&!entry.Node.Properties["disabled"].Truth){entry.Host.Send("onClick",evt);evt.StopPropagation();}});
            element.RegisterCallback<PointerEnterEvent>(evt=>{entry.Node.Hover=true;Dirty=true;});element.RegisterCallback<PointerLeaveEvent>(evt=>{entry.Node.Hover=false;Dirty=true;});
            element.RegisterCallback<FocusInEvent>(evt=>{entry.Node.Focused=true;Dirty=true;entry.Host.Send("onFocus",evt);});element.RegisterCallback<FocusOutEvent>(evt=>{entry.Node.Focused=false;Dirty=true;entry.Host.Send("onBlur",evt);});
            element.RegisterCallback<WheelEvent>(evt=>Environment.Dispatch("scroll"));
            if(element is TextField input)input.RegisterValueChangedCallback(evt=>{entry.Node.Properties["value"]=evt.newValue;entry.Host.Change(evt.newValue);});
            if(element is Slider slider)slider.RegisterValueChangedCallback(evt=>{var step=entry.Node.Properties["step"].Number;var value=step>0d?slider.lowValue+Math.Round((evt.newValue-slider.lowValue)/step)*step:evt.newValue;slider.SetValueWithoutNotify((float)value);entry.Node.Properties["value"]=value;entry.Host.Change(VisualJs.NumberText(value));});
        }
        private void BeginActivePointer(VisualElement target,int pointer)
        {for(var current=target;current!=null;current=current.parent)if(_elementEntries.TryGetValue(current,out var entry)){entry.ActivePointers.Add(pointer);entry.Node.Active=true;Dirty=true;}}
        private void EndActivePointer(int pointer)
        {foreach(var entry in _entries.Values)if(entry.ActivePointers.Remove(pointer)){entry.Node.Active=entry.ActivePointers.Count>0;Dirty=true;}}
        private void OnApplicationFocus(bool focused)
        {if(!focused)foreach(var entry in _entries.Values)if(entry.ActivePointers.Count>0){entry.ActivePointers.Clear();entry.Node.Active=false;Dirty=true;}}
        private void UpdateLayout(VisualCssNode node)
        {if(node.Native is VisualElement element){var oldBounds=(node.X,node.Y,node.Width,node.Height);UpdateBounds(node,element);if(oldBounds!=(node.X,node.Y,node.Width,node.Height))FrameVersion++;var entry=_elementEntries[element];var contentRotation=_layout.Apply(node,entry.Element,entry.Frame.parent);entry.Frame.ApplyOuterStyle(contentRotation);if(node.Tag=="svg")SourceUiSvg.Text(node,_sheet,entry.Element,entry.SvgTexts,entry.SvgTextNodes);if(DepthAncestor(node)==null&&(_sheet.Get(node,"filter","none").Trim()!="none"||_sheet.Get(node,"opacity","1")!="1")){if(entry.Filters==null)entry.Filters=new SourceUiFilters();entry.Filters.Apply(_sheet,node,element,_scale,IsDepthRoot(node));}else element.style.filter=entry.EmptyFilters;
                // Backdrop helpers are retained children, prepared after brushes
                // and depth surfaces. They issue no graphics commands at repaint.
                if(VisualBackdrop.Sigma(_sheet,node)>=0d){if(entry.Backdrop==null){entry.Backdrop=new SourceUiBackdrop(element,entry.PaintContent,ReplicaPaint,_replicaSnapshots);_backdropForegrounds.Add(entry.Backdrop.Foreground,entry);}}
                else if(entry.Backdrop!=null){_backdropForegrounds.Remove(entry.Backdrop.Foreground);entry.Backdrop.Dispose();entry.Backdrop=null;}
                element.MarkDirtyRepaint();}foreach(var child in node.Children)UpdateLayout(child);}
        private static void UpdateBounds(VisualCssNode node,VisualElement element)
        {var bounds=element is SourceUiFrame frame?frame.Content.worldBound:element.worldBound;node.X=bounds.x;node.Y=bounds.y;node.Width=float.IsNaN(element.layout.width)?0d:element.layout.width;node.Height=float.IsNaN(element.layout.height)?0d:element.layout.height;}
        private void Paint(Entry entry,MeshGenerationContext context)
        {
            var node=entry.Node;if(node.Width<=0d||node.Height<=0d)return;
            if(DepthAncestor(node)!=null)return;
            var matrix=entry.Frame.BrushMatrix;
            if(IsDepthRoot(node)){Quad(context,entry.DepthTexture,entry.DepthRect,matrix:matrix);return;}
            Quad(context,entry.Paint,entry.PaintRect,matrix:matrix);
            if(entry.Element is Slider slider)SourceUiRange.Paint(slider,context,matrix);
            if(node.Tag=="canvas")Quad(context,CanvasTexture?.Invoke(),new Rect(0,0,(float)node.Width,(float)node.Height),matrix:matrix);
            if(node.Tag=="svg")Quad(context,entry.Svg,entry.SvgRect,matrix:matrix);
        }
        // Texture uploads, SVG rasterization and depth RenderTexture work must
        // happen during the document update, not inside UI Toolkit's render pass.
        private void PreparePaintResources(VisualCssNode root)
        {
            using(UiFlowMarker.Auto())_flow.Apply(_sheet,root);
            BeforePaint?.Invoke(root);
            void Prepare(VisualCssNode node)
            {
                if(node.Native is VisualElement element&&_elementEntries.TryGetValue(element,out var entry)&&node.Width>0d&&node.Height>0d)
                {
                    EnsureBrush(entry);
                    if(entry.Element is Slider)SourceUiRange.Prepare();
                    if(node.Tag=="svg")PrepareSvg(entry);
                }
                foreach(var child in node.Children)Prepare(child);
            }
            using(UiBrushesMarker.Auto())Prepare(root);
            void PrepareDepthRoots(VisualCssNode node)
            {
                if(node.Native is VisualElement element&&_elementEntries.TryGetValue(element,out var entry)&&IsDepthRoot(node)&&DepthAncestor(node)==null)
                    PrepareDepth(entry);
                foreach(var child in node.Children)PrepareDepthRoots(child);
            }
            using(UiDepthMarker.Auto())PrepareDepthRoots(root);
            foreach(var entry in _entries.Values){var bounds=IsDepthRoot(entry.Node)?entry.DepthRect:entry.PaintRect;if(entry.Svg!=null)bounds=Rect.MinMaxRect(Mathf.Min(bounds.xMin,entry.SvgRect.xMin),Mathf.Min(bounds.yMin,entry.SvgRect.yMin),Mathf.Max(bounds.xMax,entry.SvgRect.xMax),Mathf.Max(bounds.yMax,entry.SvgRect.yMax));entry.Frame.SetPaintBounds(bounds);}
            // Native hierarchy order is the actual paint order (CSS z-index /
            // order and ScrollView containers included). Earlier backdrop copies
            // must be ready before a later panel takes its paint prefix.
            void Backdrops(VisualElement element)
            {
                if(_elementEntries.TryGetValue(element,out var entry)&&element==entry.Frame&&entry.Backdrop!=null){
                    var ancestor=entry.Node.Parent;while(ancestor!=null&&!VisualBackdrop.IsRoot(_sheet,ancestor))ancestor=ancestor.Parent;
                    var backdropRoot=ancestor?.Native as VisualElement??_document.rootVisualElement;
                    entry.Backdrop.Prepare(backdropRoot,(float)VisualBackdrop.Sigma(_sheet,entry.Node));
                }
                if(_nativeBackdrops.Contains(element)){
                    var backdropRoot=_document.rootVisualElement;
                    for(var parent=element.parent;parent!=null;parent=parent.parent)
                        if(_elementEntries.TryGetValue(parent,out var ancestor)&&VisualBackdrop.IsRoot(_sheet,ancestor.Node)){backdropRoot=ancestor.Frame;break;}
                    _nativeBackdrops.Prepare(element,backdropRoot);
                }
                // Helpers themselves contain replicas; never traverse them as
                // live DOM entries or turn a replica into another capture source.
                for(var index=0;index<element.hierarchy.childCount;index++){
                    var child=element.hierarchy[index];if(child.userData==SourceUiBackdrop.ReplicaTag||child.userData is SourceUiBackdrop.ClipTag)continue;Backdrops(child);
                }
            }
            using(UiBackdropMarker.Auto()){_replicaSnapshots.Begin();try{Backdrops(_document.rootVisualElement);}finally{_replicaSnapshots.End();}}
        }
        private Action<MeshGenerationContext> ReplicaPaint(VisualElement source)
        {
            if(source.userData is SourceUiReplica.PaintTag tag)return tag.Paint;
            if(_backdropForegrounds.TryGetValue(source,out var foreground))return foreground.PaintContent;
            if(_elementEntries.TryGetValue(source,out var entry))return source==entry.Frame&&entry.Backdrop==null?entry.PaintContent:null;
            // Native retained islands keep their own vector painters. Images and
            // text are copied by their native element, avoiding a second draw.
            return source is Image||source is TextElement?null:source.generateVisualContent;
        }
        private void EnsureBrush(Entry entry)
        {
            var node=entry.Node;
            VisualCssPaint.Viewport? localViewport=null;
            if(VisualCssPaint.ShadowOutset(_sheet,node)>Math.Max(_sheet.Width,_sheet.Height)){
                var inverse=entry.Element.worldTransform.inverse;var p0=inverse.MultiplyPoint3x4(Vector3.zero);var p1=inverse.MultiplyPoint3x4(new Vector3((float)_sheet.Width,0,0));var p2=inverse.MultiplyPoint3x4(new Vector3(0,(float)_sheet.Height,0));var p3=inverse.MultiplyPoint3x4(new Vector3((float)_sheet.Width,(float)_sheet.Height,0));
                localViewport=new VisualCssPaint.Viewport {Left=Math.Min(Math.Min(p0.x,p1.x),Math.Min(p2.x,p3.x)),Top=Math.Min(Math.Min(p0.y,p1.y),Math.Min(p2.y,p3.y)),Right=Math.Max(Math.Max(p0.x,p1.x),Math.Max(p2.x,p3.x)),Bottom=Math.Max(Math.Max(p0.y,p1.y),Math.Max(p2.y,p3.y))};
            }
            var viewportClipKey=localViewport.HasValue?localViewport.Value.Left.ToString("R",CultureInfo.InvariantCulture)+"/"+localViewport.Value.Top.ToString("R",CultureInfo.InvariantCulture)+"/"+localViewport.Value.Right.ToString("R",CultureInfo.InvariantCulture)+"/"+localViewport.Value.Bottom.ToString("R",CultureInfo.InvariantCulture):"";
            var fingerprint=viewportClipKey+"/"+node.Width.ToString("R",CultureInfo.InvariantCulture)+"/"+node.Height.ToString("R",CultureInfo.InvariantCulture)+"/"+_scale+"/"+string.Join(";",node.Style.Where(pair=>pair.Key.StartsWith("background",StringComparison.Ordinal)||pair.Key.StartsWith("border-",StringComparison.Ordinal)||pair.Key=="box-shadow"||pair.Key=="clip-path"||pair.Key=="mask-image"||pair.Key=="color"||pair.Key=="filter").OrderBy(pair=>pair.Key).Select(pair=>pair.Key+":"+pair.Value));
            if(entry.PaintKey!=fingerprint){
                using var cssSample=UiCssMarker.Auto();
                var raster=VisualCssPaint.Paint(_sheet,node,_scale,localViewport);if(entry.Paint==null||entry.Paint.width!=raster.Width||entry.Paint.height!=raster.Height){if(entry.Paint!=null)Destroy(entry.Paint);entry.Paint=new Texture2D(raster.Width,raster.Height,TextureFormat.RGBA32,false,false){name="Source CSS "+node.Tag,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};}
                // CSS raster rows run top-to-bottom; Unity texture rows bottom-to-top.
                if(entry.PaintUploadPixels==null||entry.PaintUploadPixels.Length!=raster.Pixels.Length)
                    entry.PaintUploadPixels=new byte[raster.Pixels.Length];
                var stride=raster.Width*4;for(var y=0;y<raster.Height;y++)Buffer.BlockCopy(raster.Pixels,y*stride,entry.PaintUploadPixels,(raster.Height-y-1)*stride,stride);entry.Paint.LoadRawTextureData(entry.PaintUploadPixels);entry.Paint.Apply(false,false);entry.PaintRect=new Rect((float)raster.Left,(float)raster.Top,(float)(raster.Width/raster.Scale),(float)(raster.Height/raster.Scale));entry.PaintKey=fingerprint;
            }
        }
        private void PrepareSvg(Entry entry)
        {
            var node=entry.Node;
            if(node.Tag=="svg"){
                var xml=SourceUiSvg.Serialize(node,_sheet);var key=xml+"/"+node.Width+"/"+node.Height+"/"+_scale;
                if(entry.SvgKey!=key){using var svgSample=UiSvgMarker.Auto();var previous=entry.Svg;var updated=SourceUiSvg.Rasterize(xml,(float)node.Width,(float)node.Height,_scale,_svgMaterial,_sheet.Get(node,"overflow","visible")=="visible",out entry.SvgRect,previous as RenderTexture);if(previous!=null&&previous!=updated)Destroy(previous);entry.Svg=updated;entry.SvgKey=key;}
            }
        }
        private bool IsDepthRoot(VisualCssNode node)=>_sheet.Get(node,"transform-style")=="preserve-3d";
        private VisualCssNode DepthAncestor(VisualCssNode node){VisualCssNode result=null;for(var parent=node.Parent;parent!=null;parent=parent.Parent)if(IsDepthRoot(parent))result=parent;return result;}
        private void PrepareDepth(Entry root)
        {
            var perspective=0f;Vector2? eye=null;for(var ancestor=root.Node;ancestor!=null;ancestor=ancestor.Parent){var value=_sheet.Get(ancestor,"perspective","none");if(value!="none"){
                perspective=(float)_sheet.Length(value,_sheet.Width);var origin=VisualCss.Split(_sheet.Get(ancestor,"perspective-origin","50% 50%"),' ').Where(part=>part.Length>0).ToArray();
                float Position(string part,double dimension)=>part=="center"?(float)dimension*.5f:part=="left"||part=="top"?0f:part=="right"||part=="bottom"?(float)dimension:(float)_sheet.Length(part,dimension);
                var point=new Vector3(Position(origin.Length>0?origin[0]:"50%",ancestor.Width),Position(origin.Length>1?origin[1]:"50%",ancestor.Height));if(ancestor.Native is VisualElement element)eye=root.Element.worldTransform.inverse.MultiplyPoint3x4(element.worldTransform.MultiplyPoint3x4(point));break;}}
            RenderDepthLayer(root,perspective,eye);
        }
        private bool ClipsDepth(VisualCssNode node)=>_sheet.Get(node,"overflow")=="hidden"||_sheet.Get(node,"overflow-x")=="hidden"||_sheet.Get(node,"overflow-y")=="hidden";
        private bool FlattensDepth(VisualCssNode node)=>ClipsDepth(node)||_sheet.Get(node,"transform-style","flat")!="preserve-3d"||_sheet.Get(node,"filter","none")!="none"||_sheet.Get(node,"opacity","1")!="1"||_sheet.Get(node,"isolation")=="isolate";
        private Texture2D DepthClip(Entry entry)
        {
            var node=entry.Node;var corners=new[]{"border-top-left-radius","border-top-right-radius","border-bottom-left-radius","border-bottom-right-radius"};var key=node.Width+"/"+node.Height+"/"+_scale+"/"+string.Join("/",corners.Select(corner=>_sheet.Get(node,corner,"0")));
            if(key!=entry.GroupClipKey){
                if(entry.GroupClip!=null)Destroy(entry.GroupClip);var clip=new VisualCssNode {Tag="div",Width=node.Width,Height=node.Height};clip.Style["background-color"]="#ffffff";foreach(var corner in corners)clip.Style[corner]=_sheet.Get(node,corner,"0");
                var raster=VisualCssPaint.Paint(_sheet,clip,_scale);if(entry.DepthClipUploadPixels==null||entry.DepthClipUploadPixels.Length!=raster.Pixels.Length)entry.DepthClipUploadPixels=new byte[raster.Pixels.Length];var stride=raster.Width*4;for(var y=0;y<raster.Height;y++)Buffer.BlockCopy(raster.Pixels,y*stride,entry.DepthClipUploadPixels,(raster.Height-y-1)*stride,stride);
                entry.GroupClip=new Texture2D(raster.Width,raster.Height,TextureFormat.RGBA32,false,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};entry.GroupClip.LoadRawTextureData(entry.DepthClipUploadPixels);entry.GroupClip.Apply(false,true);entry.GroupClipKey=key;
            }
            return entry.GroupClip;
        }
        private void RenderDepthLayer(Entry root,float perspective,Vector2? eye=null)
        {
            var images=root.DepthImages;images.Clear();root.DepthImageCursor=0;var clip=ClipsDepth(root.Node)?DepthClip(root):null;var clipRect=new Rect(0,0,(float)root.Node.Width,(float)root.Node.Height);
            void Append(Texture texture,Rect rect,Matrix4x4 matrix,float opacity,Texture mask,string blend)
            {
                var image=root.RentDepthImage();image.Texture=texture;image.Rect=rect;image.Matrix=matrix;image.Opacity=opacity;
                image.Clip=mask;image.ClipRect=clipRect;image.Blend=blend;images.Add(image);
            }
            void Add(VisualCssNode node,Matrix4x4 matrix,float opacity){
                if(!(node.Native is VisualElement element)||_sheet.Get(node,"display")=="none"||_sheet.Get(node,"visibility")=="hidden"||node.Width<=0||node.Height<=0)return;
                var entry=_elementEntries[element];var nested=node!=root.Node&&(node.Children.Any(child=>child.Native is VisualElement)&&FlattensDepth(node)||_sheet.Get(node,"filter","none")!="none");
                if(nested){RenderDepthLayer(entry,0f);Append(entry.DepthTexture,entry.DepthRect,matrix,opacity,clip,_sheet.Get(node,"mix-blend-mode","normal"));return;}
                EnsureBrush(entry);Append(entry.Paint,entry.PaintRect,matrix,opacity,node==root.Node?null:clip,_sheet.Get(node,"mix-blend-mode","normal"));
                foreach(var child in node.Children)if(child.Native is VisualElement native){var position=native.layout.position;var alpha=float.TryParse(_sheet.Get(child,"opacity","1"),NumberStyles.Float,CultureInfo.InvariantCulture,out var value)?value:1f;Add(child,matrix*Matrix4x4.Translate(new Vector3(position.x,position.y,0))*SourceUiDepth.Transform(_sheet,child),opacity*alpha);}
            }
            Add(root.Node,Matrix4x4.identity,1f);var fingerprint=perspective+"/"+eye+"/"+string.Join(";",images.Select(image=>(image.Texture!=null?image.Texture.GetInstanceID():0)+":"+image.Matrix.ToString("R")+":"+image.Opacity.ToString("R",CultureInfo.InvariantCulture)+":"+image.Blend+":"+(image.Clip!=null?image.Clip.GetInstanceID():0)));
            var filter=_sheet.Get(root.Node,"filter","none");fingerprint+="/"+filter+"/"+_sheet.Get(root.Node,"color")+"/"+_scale;
            if(root.Depth==null)root.Depth=new SourceUiDepth();if(root.DepthKey!=fingerprint){
                root.DepthTexture=root.Depth.Render(images,(float)root.Node.Width,(float)root.Node.Height,_scale,perspective,eye,!FlattensDepth(root.Node));root.DepthRect=root.Depth.Bounds;
                if(filter!="none"&&root.DepthTexture!=null){if(root.SurfaceFilters==null)root.SurfaceFilters=new SourceUiSurfaceFilters();root.DepthTexture=root.SurfaceFilters.Render(root.DepthTexture,root.DepthRect,filter,_sheet,root.Node,_scale);root.DepthRect=root.SurfaceFilters.Bounds;}
                root.DepthKey=fingerprint;
            }
        }
        internal static void Quad(MeshGenerationContext context,Texture texture,Rect rect,Rect? source=null,Matrix4x4? matrix=null)
        {
            if(texture==null)return;var mesh=context.Allocate(4,6,texture);var uv=source??new Rect(0,0,1,1);var color=new Color32(255,255,255,255);
            Vector3 Position(float x,float y){var point=new Vector3(x,y,Vertex.nearZ);return matrix.HasValue?matrix.Value.MultiplyPoint3x4(point):point;}
            mesh.SetNextVertex(new Vertex {position=Position(rect.xMin,rect.yMin),tint=color,uv=new Vector2(uv.xMin,uv.yMax)});mesh.SetNextVertex(new Vertex {position=Position(rect.xMax,rect.yMin),tint=color,uv=new Vector2(uv.xMax,uv.yMax)});mesh.SetNextVertex(new Vertex {position=Position(rect.xMax,rect.yMax),tint=color,uv=new Vector2(uv.xMax,uv.yMin)});mesh.SetNextVertex(new Vertex {position=Position(rect.xMin,rect.yMax),tint=color,uv=new Vector2(uv.xMin,uv.yMin)});
            foreach(var index in new ushort[]{0,1,2,2,3,0})mesh.SetNextIndex(index);
        }
        internal V Query(string selector,VisualCssNode start=null,bool all=false)
        {
            var results=new List<V>();void Visit(VisualCssNode node){if(node==null)return;if(node.Native is VisualElement element&&element.userData is SourceUiElement host&&VisualCss.Match(node,selector))results.Add(new V(host));foreach(var child in node.Children)Visit(child);}Visit(start??_root);return all?VisualJs.Arr(results.ToArray()):results.Count>0?results[0]:VisualJs.Null;
        }
        internal VisualCss Sheet=>_sheet;
        internal V ElementFromPoint(float x,float y){var element=_document.rootVisualElement?.panel?.Pick(new Vector2(x,y));var host=SourceUiElement.Target(element);return host==null?VisualJs.Null:new V(host);}
        internal void EventCompleted(){Dirty=true;AfterEvent?.Invoke();}
        internal void ShowConfirmation(string message,Action<bool> answer)
        {
            // Platform-dialog adapter for the source's window.confirm call. This
            // is separate from authored game screens, just as the browser dialog.
            var overlay=new VisualElement();overlay.style.position=Position.Absolute;overlay.style.left=0;overlay.style.right=0;overlay.style.top=0;overlay.style.bottom=0;overlay.style.alignItems=Align.Center;overlay.style.justifyContent=Justify.Center;overlay.style.backgroundColor=new Color(0,0,0,.35f);
            var box=new VisualElement();box.style.width=Math.Min(360f,(float)_sheet.Width-32f);box.style.paddingLeft=20;box.style.paddingRight=20;box.style.paddingTop=20;box.style.paddingBottom=16;box.style.backgroundColor=Color.white;box.style.color=Color.black;
            var text=new Label(message);text.style.whiteSpace=WhiteSpace.Normal;text.style.fontSize=14;box.Add(text);var buttons=new VisualElement();buttons.style.flexDirection=FlexDirection.Row;buttons.style.justifyContent=Justify.FlexEnd;buttons.style.marginTop=20;
            foreach(var choice in new[]{false,true}){var accepted=choice;var button=new Button(()=>{overlay.RemoveFromHierarchy();answer(accepted);}) {text=choice?"OK":Application.systemLanguage==SystemLanguage.Russian?"Отмена":"Cancel"};button.style.minWidth=72;button.style.height=30;button.style.marginLeft=8;button.style.backgroundColor=choice?new Color(35f/255f,100f/255f,205f/255f,1f):new Color(235f/255f,235f/255f,235f/255f,1f);button.style.color=choice?(Color)Color.white:(Color)Color.black;button.style.unityTextAlign=TextAnchor.MiddleCenter;buttons.Add(button);}
            box.Add(buttons);overlay.Add(box);_document.rootVisualElement.Add(overlay);overlay.BringToFront();
        }
        private void Dispose(Entry entry){_elementEntries.Remove(entry.Element);_elementEntries.Remove(entry.Frame);if(entry.Backdrop!=null)_backdropForegrounds.Remove(entry.Backdrop.Foreground);entry.Backdrop?.Dispose();entry.Frame.RemoveFromHierarchy();if(entry.Paint!=null)Destroy(entry.Paint);if(entry.Svg!=null)Destroy(entry.Svg);if(entry.GroupClip!=null)Destroy(entry.GroupClip);entry.Depth?.Dispose();entry.SurfaceFilters?.Dispose();entry.Filters?.Dispose();}
        private void OnDestroy(){VisualDom.Reset();VisualDom.Changed=null;Environment?.Dispose();_nativeBackdrops?.Dispose();foreach(var entry in _entries.Values)Dispose(entry);_entries.Clear();if(_svgMaterial!=null)Destroy(_svgMaterial);if(_panel!=null)Destroy(_panel);}
    }
}
#endif
