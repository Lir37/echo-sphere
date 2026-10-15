using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class SourceUiDocument : MonoBehaviour
    {
        private sealed class Entry
        {
            public VisualCssNode Node;
            public VisualElement Element;
            public SourceUiElement Host;
            public readonly VisualCssAnimation Motion=new VisualCssAnimation();
            public readonly Dictionary<string,string> BaseStyle=new Dictionary<string,string>();
            public readonly HashSet<int> ActivePointers=new HashSet<int>();
            public string PaintKey,SvgKey;
            public Texture2D Paint;
            public Texture Svg;
            public readonly List<Label> SvgTexts=new List<Label>();
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
            public bool Used;
        }
        private UIDocument _document;
        private PanelSettings _panel;
        private RenderTexture _backdropCapture;
        private VisualCss _sheet;
        private SourceUiLayout _layout;
        private readonly Dictionary<string,Entry> _entries=new Dictionary<string,Entry>();
        private readonly Dictionary<VisualElement,Entry> _elementEntries=new Dictionary<VisualElement,Entry>();
        private VisualCssNode _root;
        private float _scale=1f;
        private Material _svgMaterial;
        public SourceUiEnvironment Environment {get;private set;}
        public Func<V> Render;
        public Func<Texture> CanvasTexture;
        public Action AfterEvent;
        public bool Dirty=true;
        public bool Continuous;
        internal int FrameVersion {get;private set;}
        internal float Scale=>_scale;
        public void Initialize()
        {
            _sheet=new VisualCss(VisualJson.Parse(Resources.Load<TextAsset>("EchoSphere/SourceUiStyles").text));_layout=new SourceUiLayout(_sheet);
            VisualSourceIcons.Data=VisualJson.Parse(Resources.Load<TextAsset>("EchoSphere/SourceUiIcons").text);
            _svgMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceUiVector"));
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Source CSS viewport";_panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1000;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            _document.rootVisualElement.style.position=Position.Absolute;_document.rootVisualElement.style.left=0;_document.rootVisualElement.style.right=0;_document.rootVisualElement.style.top=0;_document.rootVisualElement.style.bottom=0;
            _document.rootVisualElement.pickingMode=PickingMode.Ignore;
            _document.rootVisualElement.RegisterCallback<PointerDownEvent>(evt=>{if(evt.button==0)BeginActivePointer(evt.target as VisualElement,evt.pointerId);},TrickleDown.TrickleDown);
            _document.rootVisualElement.RegisterCallback<PointerUpEvent>(evt=>EndActivePointer(evt.pointerId),TrickleDown.TrickleDown);
            _document.rootVisualElement.RegisterCallback<PointerCancelEvent>(evt=>EndActivePointer(evt.pointerId),TrickleDown.TrickleDown);
            Environment=new SourceUiEnvironment(this);
            VisualDom.Changed=()=>Dirty=true;
        }
        public void Draw(float scale)
        {
            _scale=Mathf.Max(.1f,scale);_panel.scale=_scale;
            var width=Screen.width/_scale;var height=Screen.height/_scale;
            if(_sheet.Width!=width||_sheet.Height!=height){_sheet.Width=width;_sheet.Height=height;Dirty=true;Environment.Dispatch("resize");}
            var safe=Screen.safeArea;_sheet.SafeLeft=safe.xMin/_scale;_sheet.SafeRight=(Screen.width-safe.xMax)/_scale;_sheet.SafeTop=(Screen.height-safe.yMax)/_scale;_sheet.SafeBottom=safe.yMin/_scale;
            Environment.Tick(Time.unscaledTimeAsDouble*1000d);
            if(Render==null)return;
            if(!Dirty&&!Continuous&&_root!=null){AdvanceMotion(_root);UpdateLayout(_root);_layout.Tracks(_root);return;}
            // Game values change each frame; stable native elements preserve focus,
            // scroll positions, pointer capture and CSS animation start times.
            FrameVersion++;Dirty=false;VisualDom.BeginFrame();var tree=Render();var nextRoot=VisualCssNode.Build(tree);CopyInteraction(_root,nextRoot);_root=nextRoot;_sheet.Apply(_root);
            foreach(var entry in _entries.Values)entry.Used=false;
            Reconcile(_root,_document.rootVisualElement,"root");
            foreach(var pair in _entries.Where(pair=>!pair.Value.Used).ToArray()){Dispose(pair.Value);_entries.Remove(pair.Key);}
            VisualDom.EndFrame();
            UpdateLayout(_root);_layout.Tracks(_root);
        }
        private void Reconcile(VisualCssNode node,VisualElement parent,string path)
        {
            var label=node.Tag=="#text"||SourceUiInline.CanMerge(_sheet,node);
            var scrollX=_sheet.Get(node,"overflow-x")=="auto"||_sheet.Get(node,"overflow")=="auto";var scrollY=_sheet.Get(node,"overflow-y")=="auto"||_sheet.Get(node,"overflow")=="auto";var scroll=scrollX||scrollY;
            var kind=node.Tag=="input"?node.Properties["type"].Text=="range"?"range":"input":scroll?"scroll":label?"label":"box";
            var key=path+":"+node.Tag+":"+kind;
            if(!_entries.TryGetValue(key,out var entry)) {
                var element=kind=="input"?(VisualElement)new TextField {isDelayed=false}:kind=="range"?new Slider():kind=="scroll"?new ScrollView(scrollX?scrollY?ScrollViewMode.VerticalAndHorizontal:ScrollViewMode.Horizontal:ScrollViewMode.Vertical):kind=="label"?new Label():new VisualElement();
                entry=new Entry {Element=element};entry.Host=new SourceUiElement(this,node,element);_entries.Add(key,entry);_elementEntries.Add(element,entry);element.userData=entry.Host;
                InstallEvents(entry);
                element.generateVisualContent+=context=>Paint(entry,context);
                element.RegisterCallback<GeometryChangedEvent>(evt=>{UpdateBounds(entry.Node,entry.Element);entry.PaintKey=null;FrameVersion++;});
            }
            entry.Used=true;entry.Node=node;entry.Host.Node=node;node.Native=entry.Element;node.Active=entry.ActivePointers.Count>0;
            if(entry.Element is ScrollView scrolling)scrolling.mode=scrollX?scrollY?ScrollViewMode.VerticalAndHorizontal:ScrollViewMode.Horizontal:ScrollViewMode.Vertical;
            UpdateBounds(node,entry.Element);if(node.Parent==null){node.Width=_sheet.Width;node.Height=_sheet.Height;}
            if(entry.Element.parent!=parent)parent.Add(entry.Element);
            entry.BaseStyle.Clear();foreach(var pair in node.Style)entry.BaseStyle[pair.Key]=pair.Value;
            node.InheritAnimatedValues();entry.Motion.Apply(_sheet,node,Time.unscaledTimeAsDouble);
            if(label||node.Tag=="svg")InheritSubtree(node);
            _layout.Apply(node,entry.Element);
            if(entry.Element is TextField input){var value=node.Properties["value"];if(!value.Undefined&&input.value!=value.Text)input.SetValueWithoutNotify(value.Text);input.maxLength=node.Properties["maxLength"].Undefined?-1:(int)node.Properties["maxLength"].Number;input.multiline=false;input.textEdition.placeholder=node.Properties["placeholder"].Undefined?"":node.Properties["placeholder"].Text;input.textEdition.hidePlaceholderOnFocus=false;input.textSelection.selectAllOnFocus=false;input.textSelection.selectAllOnMouseUp=false;var textInput=input.Q<VisualElement>("unity-text-input");if(textInput!=null){textInput.style.backgroundColor=Color.clear;textInput.style.paddingLeft=0;textInput.style.paddingRight=0;textInput.style.paddingTop=0;textInput.style.paddingBottom=0;if(input.value.Length==0&&!node.Properties["placeholder"].Undefined){var placeholder=new VisualCssNode {Tag="input",Pseudo="::placeholder",Parent=node};_sheet.Apply(placeholder);var tint=VisualCssColor.Parse(_sheet.Get(placeholder,"color",_sheet.Get(node,"color","#000000")));textInput.style.color=new Color((float)tint.R,(float)tint.G,(float)tint.B,(float)tint.A);}else textInput.style.color=entry.Element.style.color;textInput.style.fontSize=entry.Element.style.fontSize;textInput.style.unityFontDefinition=entry.Element.style.unityFontDefinition;textInput.style.unityFontStyleAndWeight=entry.Element.style.unityFontStyleAndWeight;textInput.style.borderLeftWidth=textInput.style.borderRightWidth=textInput.style.borderTopWidth=textInput.style.borderBottomWidth=0;textInput.style.marginLeft=textInput.style.marginRight=textInput.style.marginTop=textInput.style.marginBottom=0;textInput.style.flexGrow=1;}}
            if(entry.Element is Slider range){range.lowValue=(float)node.Properties["min"].Number;range.highValue=(float)node.Properties["max"].Number;if(!node.Properties["value"].Undefined)range.SetValueWithoutNotify((float)node.Properties["value"].Number);}
            entry.Element.SetEnabled(!node.Properties["disabled"].Truth);
            var reference=node.Properties["ref"];if(reference.Raw is Dictionary<string,V>)reference["current"]=new V(entry.Host);else if(reference.Raw is Func<V[],V>)reference.Call(new V(entry.Host));
            if(node.Tag=="svg"){SourceUiSvg.Text(node,_sheet,entry.Element,entry.SvgTexts);return;}
            if(label)return;
            var container=entry.Element is ScrollView view?view.contentContainer:entry.Element;var childIndex=0;
            foreach(var child in node.Children){var authoredKey=child.Properties["key"];var part=child.Pseudo??(!authoredKey.Undefined?"key="+authoredKey.Text:(childIndex++).ToString(CultureInfo.InvariantCulture));Reconcile(child,container,key+"/"+part);}
            // CSS order and z-index affect native paint / picking order without
            // changing the authored tree used by selectors and DOM queries.
            foreach(var child in node.Children.Where(child=>child.Native is VisualElement).OrderBy(child=>_sheet.Length(_sheet.Get(child,"order","0"),1d)).ThenBy(child=>_sheet.Length(_sheet.Get(child,"z-index","0"),1d))){var native=(VisualElement)child.Native;_elementEntries[native].Backdrop?.BringToFront();native.BringToFront();}
        }
        private static void CopyInteraction(VisualCssNode before,VisualCssNode after)
        {if(before==null||before.Tag!=after.Tag)return;after.Hover=before.Hover;after.Active=before.Active;after.Focused=before.Focused;var previous=before.Children.Where(child=>child.Pseudo==null).ToList();for(var index=0;index<after.Children.Count;index++){var child=after.Children[index];var key=child.Properties["key"];var match=key.Undefined?index<previous.Count?previous[index]:null:previous.FirstOrDefault(candidate=>candidate.Properties["key"]==key);CopyInteraction(match,child);}}
        private static void InheritSubtree(VisualCssNode node)
        {foreach(var child in node.Children){child.InheritAnimatedValues();InheritSubtree(child);}}
        private void AdvanceMotion(VisualCssNode node)
        {
            var before=new Dictionary<string,string>(node.Style);
            if(node.Native is VisualElement element&&_elementEntries.TryGetValue(element,out var entry)){
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
        {if(node.Native is VisualElement element){var oldBounds=(node.X,node.Y,node.Width,node.Height);UpdateBounds(node,element);if(oldBounds!=(node.X,node.Y,node.Width,node.Height))FrameVersion++;_layout.Apply(node,element);var entry=_elementEntries[element];if(node.Tag=="svg")SourceUiSvg.Text(node,_sheet,element,entry.SvgTexts);if(DepthAncestor(node)==null&&(_sheet.Get(node,"filter","none").Trim()!="none"||_sheet.Get(node,"opacity","1")!="1")){if(entry.Filters==null)entry.Filters=new SourceUiFilters();entry.Filters.Apply(_sheet,node,element,_scale,IsDepthRoot(node));}else element.style.filter=new List<FilterFunction>();var backdrop=System.Text.RegularExpressions.Regex.Match(_sheet.Get(node,"backdrop-filter","none"),@"blur\(([^)]+)\)");if(backdrop.Success){if(entry.Backdrop==null)entry.Backdrop=new SourceUiBackdrop(this);if(entry.Backdrop.parent!=element.parent)element.parent.Insert(element.parent.IndexOf(element),entry.Backdrop);entry.Backdrop.Update(node,element,(float)_sheet.Length(backdrop.Groups[1].Value,node.Width));}else if(entry.Backdrop!=null){entry.Backdrop.Dispose();entry.Backdrop=null;}element.MarkDirtyRepaint();}foreach(var child in node.Children)UpdateLayout(child);}
        private static void UpdateBounds(VisualCssNode node,VisualElement element)
        {var bounds=element.worldBound;node.X=bounds.x;node.Y=bounds.y;node.Width=float.IsNaN(element.layout.width)?0d:element.layout.width;node.Height=float.IsNaN(element.layout.height)?0d:element.layout.height;}
        private void Paint(Entry entry,MeshGenerationContext context)
        {
            var node=entry.Node;if(node.Width<=0d||node.Height<=0d)return;
            if(DepthAncestor(node)!=null)return;
            if(IsDepthRoot(node)){PaintDepth(entry,context);return;}
            if(entry.Backdrop!=null)Quad(context,entry.Backdrop.Texture,new Rect(0,0,(float)node.Width,(float)node.Height));
            EnsureBrush(entry);
            Quad(context,entry.Paint,entry.PaintRect);
            if(entry.Element is Slider slider)SourceUiRange.Paint(slider,context);
            if(node.Tag=="canvas")Quad(context,CanvasTexture?.Invoke(),new Rect(0,0,(float)node.Width,(float)node.Height));
            PaintSvg(entry,context);
        }
        private void EnsureBrush(Entry entry)
        {
            var node=entry.Node;
            var viewportClipKey=_sheet.Get(node,"box-shadow","none")=="none"?"":node.X.ToString("R",CultureInfo.InvariantCulture)+"/"+node.Y.ToString("R",CultureInfo.InvariantCulture);
            var fingerprint=viewportClipKey+"/"+node.Width.ToString("R",CultureInfo.InvariantCulture)+"/"+node.Height.ToString("R",CultureInfo.InvariantCulture)+"/"+_scale+"/"+string.Join(";",node.Style.Where(pair=>pair.Key.StartsWith("background",StringComparison.Ordinal)||pair.Key.StartsWith("border-",StringComparison.Ordinal)||pair.Key=="box-shadow"||pair.Key=="clip-path"||pair.Key=="mask-image"||pair.Key=="color"||pair.Key=="filter").OrderBy(pair=>pair.Key).Select(pair=>pair.Key+":"+pair.Value));
            if(entry.PaintKey!=fingerprint){
                if(entry.Paint!=null)Destroy(entry.Paint);var raster=VisualCssPaint.Paint(_sheet,node,_scale);entry.Paint=new Texture2D(raster.Width,raster.Height,TextureFormat.RGBA32,false,false){name="Source CSS "+node.Tag,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
                // CSS raster rows run top-to-bottom; Unity texture rows bottom-to-top.
                var pixels=new byte[raster.Pixels.Length];var stride=raster.Width*4;for(var y=0;y<raster.Height;y++)Buffer.BlockCopy(raster.Pixels,y*stride,pixels,(raster.Height-y-1)*stride,stride);entry.Paint.LoadRawTextureData(pixels);entry.Paint.Apply(false,true);entry.PaintRect=new Rect((float)raster.Left,(float)raster.Top,(float)(raster.Width/raster.Scale),(float)(raster.Height/raster.Scale));entry.PaintKey=fingerprint;
            }
        }
        private void PaintSvg(Entry entry,MeshGenerationContext context)
        {
            var node=entry.Node;
            if(node.Tag=="svg"){
                var xml=SourceUiSvg.Serialize(node,_sheet);var key=xml+"/"+node.Width+"/"+node.Height+"/"+_scale;
                if(entry.SvgKey!=key){if(entry.Svg!=null)Destroy(entry.Svg);entry.Svg=SourceUiSvg.Rasterize(xml,(float)node.Width,(float)node.Height,_scale,_svgMaterial,_sheet.Get(node,"overflow","visible")=="visible",out entry.SvgRect);entry.SvgKey=key;}
                Quad(context,entry.Svg,entry.SvgRect);
            }
        }
        private bool IsDepthRoot(VisualCssNode node)=>_sheet.Get(node,"transform-style")=="preserve-3d";
        private VisualCssNode DepthAncestor(VisualCssNode node){VisualCssNode result=null;for(var parent=node.Parent;parent!=null;parent=parent.Parent)if(IsDepthRoot(parent))result=parent;return result;}
        private void PaintDepth(Entry root,MeshGenerationContext context)
        {
            var perspective=0f;Vector2? eye=null;for(var ancestor=root.Node;ancestor!=null;ancestor=ancestor.Parent){var value=_sheet.Get(ancestor,"perspective","none");if(value!="none"){
                perspective=(float)_sheet.Length(value,_sheet.Width);var origin=VisualCss.Split(_sheet.Get(ancestor,"perspective-origin","50% 50%"),' ').Where(part=>part.Length>0).ToArray();
                float Position(string part,double dimension)=>part=="center"?(float)dimension*.5f:part=="left"||part=="top"?0f:part=="right"||part=="bottom"?(float)dimension:(float)_sheet.Length(part,dimension);
                var point=new Vector3(Position(origin.Length>0?origin[0]:"50%",ancestor.Width),Position(origin.Length>1?origin[1]:"50%",ancestor.Height));if(ancestor.Native is VisualElement element)eye=root.Element.worldTransform.inverse.MultiplyPoint3x4(element.worldTransform.MultiplyPoint3x4(point));break;}}
            RenderDepthLayer(root,perspective,eye);
            Quad(context,root.DepthTexture,root.DepthRect);
        }
        private bool ClipsDepth(VisualCssNode node)=>_sheet.Get(node,"overflow")=="hidden"||_sheet.Get(node,"overflow-x")=="hidden"||_sheet.Get(node,"overflow-y")=="hidden";
        private bool FlattensDepth(VisualCssNode node)=>ClipsDepth(node)||_sheet.Get(node,"transform-style","flat")!="preserve-3d"||_sheet.Get(node,"filter","none")!="none"||_sheet.Get(node,"opacity","1")!="1"||_sheet.Get(node,"isolation")=="isolate";
        private Texture2D DepthClip(Entry entry)
        {
            var node=entry.Node;var corners=new[]{"border-top-left-radius","border-top-right-radius","border-bottom-left-radius","border-bottom-right-radius"};var key=node.Width+"/"+node.Height+"/"+_scale+"/"+string.Join("/",corners.Select(corner=>_sheet.Get(node,corner,"0")));
            if(key!=entry.GroupClipKey){
                if(entry.GroupClip!=null)Destroy(entry.GroupClip);var clip=new VisualCssNode {Tag="div",Width=node.Width,Height=node.Height};clip.Style["background-color"]="#ffffff";foreach(var corner in corners)clip.Style[corner]=_sheet.Get(node,corner,"0");
                var raster=VisualCssPaint.Paint(_sheet,clip,_scale);var pixels=new byte[raster.Pixels.Length];var stride=raster.Width*4;for(var y=0;y<raster.Height;y++)Buffer.BlockCopy(raster.Pixels,y*stride,pixels,(raster.Height-y-1)*stride,stride);
                entry.GroupClip=new Texture2D(raster.Width,raster.Height,TextureFormat.RGBA32,false,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};entry.GroupClip.LoadRawTextureData(pixels);entry.GroupClip.Apply(false,true);entry.GroupClipKey=key;
            }
            return entry.GroupClip;
        }
        private void RenderDepthLayer(Entry root,float perspective,Vector2? eye=null)
        {
            var images=new List<SourceUiDepth.PlaneImage>();var clip=ClipsDepth(root.Node)?DepthClip(root):null;var clipRect=new Rect(0,0,(float)root.Node.Width,(float)root.Node.Height);
            void Add(VisualCssNode node,Matrix4x4 matrix,float opacity){
                if(!(node.Native is VisualElement element)||_sheet.Get(node,"display")=="none"||_sheet.Get(node,"visibility")=="hidden"||node.Width<=0||node.Height<=0)return;
                var entry=_elementEntries[element];var nested=node!=root.Node&&(node.Children.Any(child=>child.Native is VisualElement)&&FlattensDepth(node)||_sheet.Get(node,"filter","none")!="none");
                if(nested){RenderDepthLayer(entry,0f);images.Add(new SourceUiDepth.PlaneImage {Texture=entry.DepthTexture,Rect=entry.DepthRect,Matrix=matrix,Opacity=opacity,Clip=clip,ClipRect=clipRect,Blend=_sheet.Get(node,"mix-blend-mode","normal")});return;}
                EnsureBrush(entry);images.Add(new SourceUiDepth.PlaneImage {Texture=entry.Paint,Rect=entry.PaintRect,Matrix=matrix,Opacity=opacity,Clip=node==root.Node?null:clip,ClipRect=clipRect,Blend=_sheet.Get(node,"mix-blend-mode","normal")});
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
        internal static void Quad(MeshGenerationContext context,Texture texture,Rect rect,Rect? source=null)
        {
            if(texture==null)return;var mesh=context.Allocate(4,6,texture);var uv=mesh.uvRegion;if(source.HasValue){var part=source.Value;uv=new Rect(uv.x+part.x*uv.width,uv.y+part.y*uv.height,part.width*uv.width,part.height*uv.height);}var color=new Color32(255,255,255,255);
            mesh.SetNextVertex(new Vertex {position=new Vector3(rect.xMin,rect.yMin,Vertex.nearZ),tint=color,uv=new Vector2(uv.xMin,uv.yMax)});mesh.SetNextVertex(new Vertex {position=new Vector3(rect.xMax,rect.yMin,Vertex.nearZ),tint=color,uv=new Vector2(uv.xMax,uv.yMax)});mesh.SetNextVertex(new Vertex {position=new Vector3(rect.xMax,rect.yMax,Vertex.nearZ),tint=color,uv=new Vector2(uv.xMax,uv.yMin)});mesh.SetNextVertex(new Vertex {position=new Vector3(rect.xMin,rect.yMax,Vertex.nearZ),tint=color,uv=new Vector2(uv.xMin,uv.yMin)});
            foreach(var index in new ushort[]{0,1,2,2,3,0})mesh.SetNextIndex(index);
        }
        internal V Query(string selector,VisualCssNode start=null,bool all=false)
        {
            var results=new List<V>();void Visit(VisualCssNode node){if(node==null)return;if(node.Native is VisualElement element&&element.userData is SourceUiElement host&&VisualCss.Match(node,selector))results.Add(new V(host));foreach(var child in node.Children)Visit(child);}Visit(start??_root);return all?VisualJs.Arr(results.ToArray()):results.Count>0?results[0]:VisualJs.Null;
        }
        internal VisualCss Sheet=>_sheet;
        internal RenderTexture BackdropCapture()
        {
            if(_backdropCapture==null||_backdropCapture.width!=Screen.width||_backdropCapture.height!=Screen.height){
                if(_backdropCapture!=null){_backdropCapture.Release();Destroy(_backdropCapture);}
                _backdropCapture=new RenderTexture(Screen.width,Screen.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Source shared backdrop capture",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};_backdropCapture.Create();
            }
            return _backdropCapture;
        }
        internal V ElementFromPoint(float x,float y){var element=_document.rootVisualElement.panel?.Pick(new Vector2(x,y));var host=SourceUiElement.Target(element);return host==null?VisualJs.Null:new V(host);}
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
        private void Dispose(Entry entry){_elementEntries.Remove(entry.Element);entry.Backdrop?.Dispose();entry.Element.RemoveFromHierarchy();if(entry.Paint!=null)Destroy(entry.Paint);if(entry.Svg!=null)Destroy(entry.Svg);if(entry.GroupClip!=null)Destroy(entry.GroupClip);entry.Depth?.Dispose();entry.SurfaceFilters?.Dispose();entry.Filters?.Dispose();}
        private void OnDestroy(){VisualDom.Reset();VisualDom.Changed=null;Environment?.Dispose();foreach(var entry in _entries.Values)Dispose(entry);_entries.Clear();if(_backdropCapture!=null){_backdropCapture.Release();Destroy(_backdropCapture);}if(_svgMaterial!=null)Destroy(_svgMaterial);if(_panel!=null)Destroy(_panel);}
    }
}
