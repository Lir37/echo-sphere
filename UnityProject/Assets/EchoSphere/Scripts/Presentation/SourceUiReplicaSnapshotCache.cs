using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Shared only within one ordered backdrop pass. Earlier paint-prefix sources
    // are fully prepared before later backdrops read them. Nothing survives as
    // a cached value across passes; retained records avoid allocation churn.
    internal sealed class SourceUiReplicaSnapshotCache
    {
        private readonly Dictionary<VisualElement,Snapshot> _values=new Dictionary<VisualElement,Snapshot>();
        private readonly List<VisualElement> _stale=new List<VisualElement>();
        internal void Begin(){foreach(var value in _values.Values)value.Used=false;}
        internal void End(){_stale.Clear();foreach(var pair in _values)if(!pair.Value.Used)_stale.Add(pair.Key);foreach(var key in _stale)_values.Remove(key);_stale.Clear();}
        internal void Apply(VisualElement source,IStyle target,bool helper)
        {
            if(!_values.TryGetValue(source,out var value)){value=new Snapshot();_values.Add(source,value);}
            if(!value.Used){if(helper)SourceUiReplica.CopyInlineGeometry(source,value);else SourceUiReplica.CopyAppearance(source,value);value.Used=true;}
            value.Apply(target,helper);
        }
        private sealed class Snapshot : IStyle
        {
            internal bool Used;
            public void Clear(bool clearInherited)=>throw new NotSupportedException("Read-only pass snapshot");
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignContent { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignItems { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignSelf { get; set; }
            public UnityEngine.UIElements.StyleRatio aspectRatio { get; set; }
            public UnityEngine.UIElements.StyleColor backgroundColor { get; set; }
            public UnityEngine.UIElements.StyleBackground backgroundImage { get; set; }
            public UnityEngine.UIElements.StyleBackgroundPosition backgroundPositionX { get; set; }
            public UnityEngine.UIElements.StyleBackgroundPosition backgroundPositionY { get; set; }
            public UnityEngine.UIElements.StyleBackgroundRepeat backgroundRepeat { get; set; }
            public UnityEngine.UIElements.StyleBackgroundSize backgroundSize { get; set; }
            public UnityEngine.UIElements.StyleColor borderBottomColor { get; set; }
            public UnityEngine.UIElements.StyleLength borderBottomLeftRadius { get; set; }
            public UnityEngine.UIElements.StyleLength borderBottomRightRadius { get; set; }
            public UnityEngine.UIElements.StyleFloat borderBottomWidth { get; set; }
            public UnityEngine.UIElements.StyleColor borderLeftColor { get; set; }
            public UnityEngine.UIElements.StyleFloat borderLeftWidth { get; set; }
            public UnityEngine.UIElements.StyleColor borderRightColor { get; set; }
            public UnityEngine.UIElements.StyleFloat borderRightWidth { get; set; }
            public UnityEngine.UIElements.StyleColor borderTopColor { get; set; }
            public UnityEngine.UIElements.StyleLength borderTopLeftRadius { get; set; }
            public UnityEngine.UIElements.StyleLength borderTopRightRadius { get; set; }
            public UnityEngine.UIElements.StyleFloat borderTopWidth { get; set; }
            public UnityEngine.UIElements.StyleLength bottom { get; set; }
            public UnityEngine.UIElements.StyleColor color { get; set; }
            public UnityEngine.UIElements.StyleCursor cursor { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.DisplayStyle> display { get; set; }
            public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.FilterFunction> filter { get; set; }
            public UnityEngine.UIElements.StyleLength flexBasis { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.FlexDirection> flexDirection { get; set; }
            public UnityEngine.UIElements.StyleFloat flexGrow { get; set; }
            public UnityEngine.UIElements.StyleFloat flexShrink { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Wrap> flexWrap { get; set; }
            public UnityEngine.UIElements.StyleLength fontSize { get; set; }
            public UnityEngine.UIElements.StyleLength height { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Justify> justifyContent { get; set; }
            public UnityEngine.UIElements.StyleLength left { get; set; }
            public UnityEngine.UIElements.StyleLength letterSpacing { get; set; }
            public UnityEngine.UIElements.StyleLength marginBottom { get; set; }
            public UnityEngine.UIElements.StyleLength marginLeft { get; set; }
            public UnityEngine.UIElements.StyleLength marginRight { get; set; }
            public UnityEngine.UIElements.StyleLength marginTop { get; set; }
            public UnityEngine.UIElements.StyleLength maxHeight { get; set; }
            public UnityEngine.UIElements.StyleLength maxWidth { get; set; }
            public UnityEngine.UIElements.StyleLength minHeight { get; set; }
            public UnityEngine.UIElements.StyleLength minWidth { get; set; }
            public UnityEngine.UIElements.StyleFloat opacity { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Overflow> overflow { get; set; }
            public UnityEngine.UIElements.StyleLength paddingBottom { get; set; }
            public UnityEngine.UIElements.StyleLength paddingLeft { get; set; }
            public UnityEngine.UIElements.StyleLength paddingRight { get; set; }
            public UnityEngine.UIElements.StyleLength paddingTop { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Position> position { get; set; }
            public UnityEngine.UIElements.StyleLength right { get; set; }
            public UnityEngine.UIElements.StyleRotate rotate { get; set; }
            public UnityEngine.UIElements.StyleScale scale { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflow> textOverflow { get; set; }
            public UnityEngine.UIElements.StyleTextShadow textShadow { get; set; }
            public UnityEngine.UIElements.StyleLength top { get; set; }
            public UnityEngine.UIElements.StyleTransformOrigin transformOrigin { get; set; }
            public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.TimeValue> transitionDelay { get; set; }
            public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.TimeValue> transitionDuration { get; set; }
            public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.StylePropertyName> transitionProperty { get; set; }
            public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.EasingFunction> transitionTimingFunction { get; set; }
            public UnityEngine.UIElements.StyleTranslate translate { get; set; }
            public UnityEngine.UIElements.StyleColor unityBackgroundImageTintColor { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.EditorTextRenderingMode> unityEditorTextRenderingMode { get; set; }
            public UnityEngine.UIElements.StyleFont unityFont { get; set; }
            public UnityEngine.UIElements.StyleFontDefinition unityFontDefinition { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.FontStyle> unityFontStyleAndWeight { get; set; }
            public UnityEngine.UIElements.StyleMaterialDefinition unityMaterial { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.OverflowClipBox> unityOverflowClipBox { get; set; }
            public UnityEngine.UIElements.StyleLength unityParagraphSpacing { get; set; }
            public UnityEngine.UIElements.StyleInt unitySliceBottom { get; set; }
            public UnityEngine.UIElements.StyleInt unitySliceLeft { get; set; }
            public UnityEngine.UIElements.StyleInt unitySliceRight { get; set; }
            public UnityEngine.UIElements.StyleFloat unitySliceScale { get; set; }
            public UnityEngine.UIElements.StyleInt unitySliceTop { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.SliceType> unitySliceType { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.TextAnchor> unityTextAlign { get; set; }
            public UnityEngine.UIElements.StyleTextAutoSize unityTextAutoSize { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.TextGeneratorType> unityTextGenerator { get; set; }
            public UnityEngine.UIElements.StyleColor unityTextOutlineColor { get; set; }
            public UnityEngine.UIElements.StyleFloat unityTextOutlineWidth { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflowPosition> unityTextOverflowPosition { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Visibility> visibility { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.WhiteSpace> whiteSpace { get; set; }
            public UnityEngine.UIElements.StyleLength width { get; set; }
            public UnityEngine.UIElements.StyleLength wordSpacing { get; set; }
            public UnityEngine.UIElements.StyleEnum<UnityEngine.ScaleMode> unityBackgroundScaleMode { get; set; }
            internal void Apply(IStyle target,bool helper)
            {
                if(helper){
                target.aspectRatio=aspectRatio;
                target.backgroundColor=backgroundColor;
                target.backgroundImage=backgroundImage;
                target.backgroundPositionX=backgroundPositionX;
                target.backgroundPositionY=backgroundPositionY;
                target.backgroundRepeat=backgroundRepeat;
                target.backgroundSize=backgroundSize;
                target.borderBottomColor=borderBottomColor;
                target.borderBottomLeftRadius=borderBottomLeftRadius;
                target.borderBottomRightRadius=borderBottomRightRadius;
                target.borderBottomWidth=borderBottomWidth;
                target.borderLeftColor=borderLeftColor;
                target.borderLeftWidth=borderLeftWidth;
                target.borderRightColor=borderRightColor;
                target.borderRightWidth=borderRightWidth;
                target.borderTopColor=borderTopColor;
                target.borderTopLeftRadius=borderTopLeftRadius;
                target.borderTopRightRadius=borderTopRightRadius;
                target.borderTopWidth=borderTopWidth;
                target.bottom=bottom;
                target.color=color;
                target.display=display;
                target.filter=filter;
                target.flexShrink=flexShrink;
                target.fontSize=fontSize;
                target.height=height;
                target.left=left;
                target.letterSpacing=letterSpacing;
                target.marginBottom=marginBottom;
                target.marginLeft=marginLeft;
                target.marginRight=marginRight;
                target.marginTop=marginTop;
                target.maxHeight=maxHeight;
                target.maxWidth=maxWidth;
                target.minHeight=minHeight;
                target.minWidth=minWidth;
                target.opacity=opacity;
                target.overflow=overflow;
                target.paddingBottom=paddingBottom;
                target.paddingLeft=paddingLeft;
                target.paddingRight=paddingRight;
                target.paddingTop=paddingTop;
                target.position=position;
                target.right=right;
                target.rotate=rotate;
                target.scale=scale;
                target.textOverflow=textOverflow;
                target.textShadow=textShadow;
                target.top=top;
                target.transformOrigin=transformOrigin;
                target.translate=translate;
                target.unityBackgroundImageTintColor=unityBackgroundImageTintColor;
                target.unityFont=unityFont;
                target.unityFontDefinition=unityFontDefinition;
                target.unityFontStyleAndWeight=unityFontStyleAndWeight;
                target.unityMaterial=unityMaterial;
                target.unityParagraphSpacing=unityParagraphSpacing;
                target.unitySliceBottom=unitySliceBottom;
                target.unitySliceLeft=unitySliceLeft;
                target.unitySliceRight=unitySliceRight;
                target.unitySliceScale=unitySliceScale;
                target.unitySliceTop=unitySliceTop;
                target.unitySliceType=unitySliceType;
                target.unityTextAlign=unityTextAlign;
                target.unityTextGenerator=unityTextGenerator;
                target.unityTextOutlineColor=unityTextOutlineColor;
                target.unityTextOutlineWidth=unityTextOutlineWidth;
                target.unityTextOverflowPosition=unityTextOverflowPosition;
                target.visibility=visibility;
                target.whiteSpace=whiteSpace;
                target.width=width;
                target.wordSpacing=wordSpacing;
                }else{
                target.backgroundColor=backgroundColor;
                target.backgroundImage=backgroundImage;
                target.backgroundPositionX=backgroundPositionX;
                target.backgroundPositionY=backgroundPositionY;
                target.backgroundRepeat=backgroundRepeat;
                target.backgroundSize=backgroundSize;
                target.borderBottomColor=borderBottomColor;
                target.borderBottomLeftRadius=borderBottomLeftRadius;
                target.borderBottomRightRadius=borderBottomRightRadius;
                target.borderBottomWidth=borderBottomWidth;
                target.borderLeftColor=borderLeftColor;
                target.borderLeftWidth=borderLeftWidth;
                target.borderRightColor=borderRightColor;
                target.borderRightWidth=borderRightWidth;
                target.borderTopColor=borderTopColor;
                target.borderTopLeftRadius=borderTopLeftRadius;
                target.borderTopRightRadius=borderTopRightRadius;
                target.borderTopWidth=borderTopWidth;
                target.color=color;
                target.display=display;
                target.filter=filter;
                target.fontSize=fontSize;
                target.letterSpacing=letterSpacing;
                target.opacity=opacity;
                target.overflow=overflow;
                target.paddingBottom=paddingBottom;
                target.paddingLeft=paddingLeft;
                target.paddingRight=paddingRight;
                target.paddingTop=paddingTop;
                target.textOverflow=textOverflow;
                target.textShadow=textShadow;
                target.unityBackgroundImageTintColor=unityBackgroundImageTintColor;
                target.unityFont=unityFont;
                target.unityFontDefinition=unityFontDefinition;
                target.unityFontStyleAndWeight=unityFontStyleAndWeight;
                target.unityMaterial=unityMaterial;
                target.unityParagraphSpacing=unityParagraphSpacing;
                target.unitySliceBottom=unitySliceBottom;
                target.unitySliceLeft=unitySliceLeft;
                target.unitySliceRight=unitySliceRight;
                target.unitySliceScale=unitySliceScale;
                target.unitySliceTop=unitySliceTop;
                target.unitySliceType=unitySliceType;
                target.unityTextAlign=unityTextAlign;
                target.unityTextGenerator=unityTextGenerator;
                target.unityTextOutlineColor=unityTextOutlineColor;
                target.unityTextOutlineWidth=unityTextOutlineWidth;
                target.unityTextOverflowPosition=unityTextOverflowPosition;
                target.visibility=visibility;
                target.whiteSpace=whiteSpace;
                target.wordSpacing=wordSpacing;
                }
            }
        }
    }
}
