#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained composition only. Unity owns all filter render targets and pass
    // boundaries. Never grab CurrentActive or submit graphics work from repaint.
    internal sealed class SourceUiBackdrop : IDisposable
    {
        internal sealed class ClipTag { internal VisualElement Owner; }
        internal static readonly object ReplicaTag=new object();
        private readonly VisualElement _original,_clip,_placement,_placementRotation,_crop,_rootPlacement,_rootRotation,_foreground;
        private readonly Action<MeshGenerationContext> _normalPaint;
        private readonly SourceUiReplica _replica;
        private readonly Material _material;
        private readonly FilterFunctionDefinition _definition;
        private float _sigma=-1f;
        private Rect _area;
        private Matrix4x4 _toPanel,_toScreen;
        private Vector2 _panelSize;
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        private Vector4 _reportedKernel=new Vector4(-1,-1,-1,-1);
#endif
#if UNITY_EDITOR
        internal static bool DisableKernelCacheForValidation=false;
#endif
        internal VisualElement Foreground=>_foreground;
        internal void SetVisible(bool visible){_clip.style.visibility=visible?Visibility.Visible:Visibility.Hidden;_foreground.style.visibility=visible?Visibility.Visible:Visibility.Hidden;}
        internal void RepaintDynamic(){if(_normalPaint!=null)_foreground.MarkDirtyRepaint();_replica.RepaintDynamic();}
        internal SourceUiBackdrop(VisualElement original,Action<MeshGenerationContext> paint,Func<VisualElement,Action<MeshGenerationContext>> replicaPaint,SourceUiReplicaSnapshotCache snapshots,bool boundReplica=false)
        {
            _original=original;_normalPaint=original.generateVisualContent;
            _clip=Box();_clip.name="Source backdrop border clip";_clip.userData=new ClipTag {Owner=original};_clip.style.overflow=Overflow.Hidden;
            _placement=Box();_placementRotation=Box();_crop=Box();_rootPlacement=Box();_rootRotation=Box();
            _crop.name="Source backdrop paint prefix";_crop.style.overflow=Overflow.Hidden;
            _clip.Add(_placement);_placement.Add(_placementRotation);_placementRotation.Add(_crop);_crop.Add(_rootPlacement);_rootPlacement.Add(_rootRotation);
            _foreground=original is TextElement?(VisualElement)new Label():Box();_foreground.userData=ReplicaTag;_foreground.pickingMode=PickingMode.Ignore;
            var textPaint=_foreground.generateVisualContent;
            _foreground.generateVisualContent=context=>{paint(context);textPaint?.Invoke(context);};
            // Keep the original text/control for measurement, input and focus.
            // Paint its foreground after the backdrop, within the SAME opacity /
            // filter group. Absolute helper children never participate in Yoga.
            original.generateVisualContent=null;
            original.hierarchy.Insert(0,_clip);original.hierarchy.Insert(1,_foreground);
            _replica=new SourceUiReplica(replicaPaint,snapshots,boundReplica);
            _material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiBackdrop"));
            _definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_definition.name=_definition.filterName="source-backdrop-blur";
            _definition.parameters=new[]{new FilterParameterDeclaration {name="sigma",interpolationDefaultValue=new FilterParameter(0f)}};
            var passes=new PostProcessingPass[2];
            for(var axis=0;axis<2;axis++){
                // The crop already includes the complete 4-sigma support, so
                // filter margins stay zero. The shader derives CSS coordinates
                // from quad pixels, independently of the atlas UV allocation.
                passes[axis]=new PostProcessingPass {material=_material,passIndex=axis,applySettingsCallback=ApplySettings};
            }
            _definition.passes=passes;
        }
        internal void Prepare(VisualElement root,float sigma)
        {
            var box=_original.layout;var resolved=_original.resolvedStyle;
            if(!Finite(box.width)||!Finite(box.height)||box.width<=0||box.height<=0){_clip.style.display=DisplayStyle.None;_foreground.style.display=DisplayStyle.None;return;}
            SourceUiReplica.Appearance(_original,_foreground);
            LocalBox(_foreground,-resolved.borderLeftWidth,-resolved.borderTopWidth,box.width,box.height);
            _foreground.style.translate=new Translate(0,0);_foreground.style.rotate=new Rotate(0);_foreground.style.scale=new Scale(Vector3.one);_foreground.style.opacity=1;_foreground.style.filter=StyleKeyword.Null;
            _foreground.style.overflow=Overflow.Visible;
            if(_foreground is TextElement text&&_original is TextElement source){text.text=source.text;text.enableRichText=source.enableRichText;}
            _foreground.MarkDirtyRepaint();
            LocalBox(_clip,-resolved.borderLeftWidth,-resolved.borderTopWidth,box.width,box.height);
            _clip.style.borderTopLeftRadius=resolved.borderTopLeftRadius;_clip.style.borderTopRightRadius=resolved.borderTopRightRadius;_clip.style.borderBottomLeftRadius=resolved.borderBottomLeftRadius;_clip.style.borderBottomRightRadius=resolved.borderBottomRightRadius;
            var frame=_original as SourceUiFrame;var matrix=_original.worldTransform*(frame?.BrushMatrix??Matrix4x4.identity);
            var determinant=matrix.determinant;if(!Finite(determinant)||Mathf.Abs(determinant)<1e-8f){_clip.style.display=DisplayStyle.None;return;}
            _clip.style.display=DisplayStyle.Flex;
            // Blur in screen CSS coordinates, THEN invert the element transform
            // and clip to its rounded border box (CSS Filter Effects 2 §2.1).
            if(frame!=null){_clip.style.rotate=frame.Content.style.rotate;_clip.style.transformOrigin=frame.Content.style.transformOrigin;}
            var area=WorldBounds(_original);var padding=Mathf.Ceil(4f*sigma);area.xMin=Mathf.Floor(area.xMin-padding);area.yMin=Mathf.Floor(area.yMin-padding);area.xMax=Mathf.Ceil(area.xMax+padding);area.yMax=Mathf.Ceil(area.yMax+padding);
            _area=area;_toScreen=matrix;_toPanel=matrix.inverse;_panelSize=new Vector2(box.width,box.height);
            SetMatrix(_placement,_placementRotation,matrix.inverse*Matrix4x4.Translate(new Vector3(area.x,area.y,0)));
            LocalBox(_crop,0,0,area.width,area.height);
            SetMatrix(_rootPlacement,_rootRotation,Matrix4x4.Translate(new Vector3(-area.x,-area.y,0))*root.worldTransform);
            if(_sigma!=sigma){var filter=new FilterFunction(_definition);filter.AddParameter(new FilterParameter(sigma));_crop.style.filter=new List<FilterFunction> {filter};_sigma=sigma;}
            _replica.Update(root,_original,_rootRotation,area);
        }
        private void ApplySettings(MaterialPropertyBlock properties,FilterPassContext context)
        {
            // Unity 6.3 does NOT apply parameterBindings when this callback is
            // present. Set sigma explicitly as well as the color/DPI context.
            float sigma=context.filterFunction.GetParameter(0).floatValue,density=context.scaledPixelsPerPoint;
            properties.SetFloat("_Sigma",sigma);properties.SetFloat("_CssDpr",density);properties.SetFloat("_CssReadsGamma",context.readsGamma?1f:0f);
            float kernelSigma=Mathf.Max(0,sigma*density);bool cached=NativeGaussianWeights.TryGet(kernelSigma,out var weights);
#if UNITY_EDITOR
            if(DisableKernelCacheForValidation)cached=false;
#endif
            properties.SetInt("_BackdropWeightCount",cached?Mathf.CeilToInt(4*kernelSigma)+1:0);if(cached)properties.SetVectorArray("_BackdropWeights",weights);
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
            var diagnostic=new Vector4(sigma,density,_area.width,_area.height);
            if(NativeDeviceReplay.Active&&_reportedKernel!=diagnostic){_reportedKernel=diagnostic;Debug.Log("[EchoSphere.Replay] backdrop="+_original.name+" sigma="+sigma+" density="+density+" area="+_area.width+"x"+_area.height+" cached="+cached);}
#endif
            properties.SetVector("_Roi",new Vector4(_area.x,_area.y,_area.width,_area.height));properties.SetVector("_PanelSize",new Vector4(_panelSize.x,_panelSize.y,0,0));
            properties.SetVector("_ToPanelX",new Vector4(_toPanel.m00,_toPanel.m01,_toPanel.m03,0));properties.SetVector("_ToPanelY",new Vector4(_toPanel.m10,_toPanel.m11,_toPanel.m13,0));
            properties.SetVector("_ToScreenX",new Vector4(_toScreen.m00,_toScreen.m01,_toScreen.m03,0));properties.SetVector("_ToScreenY",new Vector4(_toScreen.m10,_toScreen.m11,_toScreen.m13,0));
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        internal static Rect WorldBounds(VisualElement element)=>element is SourceUiFrame frame?frame.Content.worldBound:element.worldBound;
        private static VisualElement Box(){var box=new VisualElement {pickingMode=PickingMode.Ignore,userData=ReplicaTag};LocalBox(box,0,0,0,0);box.style.overflow=Overflow.Visible;return box;}
        internal static void LocalBox(VisualElement element,float x,float y,float width,float height)
            =>LocalBox(element.style,x,y,width,height);
        internal static void LocalBox(IStyle style,float x,float y,float width,float height)
        {
            // Newly attached / display:none nodes can be unlaid out. Do not
            // forward NaN Yoga bounds into inline styles or native render data.
            if(!Finite(x)||!Finite(y)||!Finite(width)||!Finite(height)){style.display=DisplayStyle.None;x=y=width=height=0;}
            style.position=Position.Absolute;style.left=x;style.top=y;style.right=StyleKeyword.Auto;style.bottom=StyleKeyword.Auto;style.width=width;style.height=height;
            style.minWidth=0;style.minHeight=0;style.maxWidth=StyleKeyword.None;style.maxHeight=StyleKeyword.None;style.aspectRatio=StyleKeyword.Auto;style.flexShrink=0;
            style.marginLeft=style.marginRight=style.marginTop=style.marginBottom=0;
        }
        internal static void SetMatrix(VisualElement first,VisualElement second,Matrix4x4 matrix)
        {
            var parts=VisualBackdrop.Decompose(matrix.m00,matrix.m10,matrix.m01,matrix.m11);
            first.style.translate=new Translate(matrix.m03,matrix.m13);first.style.transformOrigin=new TransformOrigin(0,0);first.style.rotate=new Rotate((float)(parts.First*Mathf.Rad2Deg));first.style.scale=new Scale(new Vector3((float)parts.X,(float)parts.Y,1));
            second.style.transformOrigin=new TransformOrigin(0,0);second.style.rotate=new Rotate((float)(parts.Second*Mathf.Rad2Deg));
        }
        public void Dispose()
        {
            _original.generateVisualContent=_normalPaint;_clip.RemoveFromHierarchy();_foreground.RemoveFromHierarchy();_replica.Dispose();UnityEngine.Object.Destroy(_definition);UnityEngine.Object.Destroy(_material);
        }
    }
}

#endif
