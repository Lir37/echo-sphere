using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Private inline-style writer for owned backdrop copies. Unity retains layout,
    // rendering and resource ownership. All writes to these copies go through
    // this instance, so unchanged values need no native inline-style lookup/write.
    // Mutable lists remain pass-through; their contents can change in place.
    // Getters read Unity's canonical value (e.g. an empty background can read
    // back as Null). Fields remember requests only, not resolved/native values.
    internal sealed class SourceUiReplicaStyle : IStyle
    {
        private readonly IStyle _target;
        internal SourceUiReplicaStyle(IStyle target)
        {
            _target=target;
            Read();
        }
        public void Clear(bool clearInherited)
        {
            _target.Clear(clearInherited);
            Read();
        }
        private void Read()
        {
            var target=_target;
            _alignContent=target.alignContent;
            _alignItems=target.alignItems;
            _alignSelf=target.alignSelf;
            _aspectRatio=target.aspectRatio;
            _backgroundColor=target.backgroundColor;
            _backgroundImage=target.backgroundImage;
            _backgroundPositionX=target.backgroundPositionX;
            _backgroundPositionY=target.backgroundPositionY;
            _backgroundRepeat=target.backgroundRepeat;
            _backgroundSize=target.backgroundSize;
            _borderBottomColor=target.borderBottomColor;
            _borderBottomLeftRadius=target.borderBottomLeftRadius;
            _borderBottomRightRadius=target.borderBottomRightRadius;
            _borderBottomWidth=target.borderBottomWidth;
            _borderLeftColor=target.borderLeftColor;
            _borderLeftWidth=target.borderLeftWidth;
            _borderRightColor=target.borderRightColor;
            _borderRightWidth=target.borderRightWidth;
            _borderTopColor=target.borderTopColor;
            _borderTopLeftRadius=target.borderTopLeftRadius;
            _borderTopRightRadius=target.borderTopRightRadius;
            _borderTopWidth=target.borderTopWidth;
            _bottom=target.bottom;
            _color=target.color;
            _cursor=target.cursor;
            _display=target.display;
            _flexBasis=target.flexBasis;
            _flexDirection=target.flexDirection;
            _flexGrow=target.flexGrow;
            _flexShrink=target.flexShrink;
            _flexWrap=target.flexWrap;
            _fontSize=target.fontSize;
            _height=target.height;
            _justifyContent=target.justifyContent;
            _left=target.left;
            _letterSpacing=target.letterSpacing;
            _marginBottom=target.marginBottom;
            _marginLeft=target.marginLeft;
            _marginRight=target.marginRight;
            _marginTop=target.marginTop;
            _maxHeight=target.maxHeight;
            _maxWidth=target.maxWidth;
            _minHeight=target.minHeight;
            _minWidth=target.minWidth;
            _opacity=target.opacity;
            _overflow=target.overflow;
            _paddingBottom=target.paddingBottom;
            _paddingLeft=target.paddingLeft;
            _paddingRight=target.paddingRight;
            _paddingTop=target.paddingTop;
            _position=target.position;
            _right=target.right;
            _rotate=target.rotate;
            _scale=target.scale;
            _textOverflow=target.textOverflow;
            _textShadow=target.textShadow;
            _top=target.top;
            _transformOrigin=target.transformOrigin;
            _translate=target.translate;
            _unityBackgroundImageTintColor=target.unityBackgroundImageTintColor;
            _unityEditorTextRenderingMode=target.unityEditorTextRenderingMode;
            _unityFont=target.unityFont;
            _unityFontDefinition=target.unityFontDefinition;
            _unityFontStyleAndWeight=target.unityFontStyleAndWeight;
            _unityMaterial=target.unityMaterial;
            _unityOverflowClipBox=target.unityOverflowClipBox;
            _unityParagraphSpacing=target.unityParagraphSpacing;
            _unitySliceBottom=target.unitySliceBottom;
            _unitySliceLeft=target.unitySliceLeft;
            _unitySliceRight=target.unitySliceRight;
            _unitySliceScale=target.unitySliceScale;
            _unitySliceTop=target.unitySliceTop;
            _unitySliceType=target.unitySliceType;
            _unityTextAlign=target.unityTextAlign;
            _unityTextAutoSize=target.unityTextAutoSize;
            _unityTextGenerator=target.unityTextGenerator;
            _unityTextOutlineColor=target.unityTextOutlineColor;
            _unityTextOutlineWidth=target.unityTextOutlineWidth;
            _unityTextOverflowPosition=target.unityTextOverflowPosition;
            _visibility=target.visibility;
            _whiteSpace=target.whiteSpace;
            _width=target.width;
            _wordSpacing=target.wordSpacing;
        }
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> _alignContent;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignContent {get=>_target.alignContent;set {if(_alignContent.Equals(value))return;_target.alignContent=value;_alignContent=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> _alignItems;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignItems {get=>_target.alignItems;set {if(_alignItems.Equals(value))return;_target.alignItems=value;_alignItems=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> _alignSelf;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Align> alignSelf {get=>_target.alignSelf;set {if(_alignSelf.Equals(value))return;_target.alignSelf=value;_alignSelf=value;}}
        private UnityEngine.UIElements.StyleRatio _aspectRatio;
        public UnityEngine.UIElements.StyleRatio aspectRatio {get=>_target.aspectRatio;set {if(_aspectRatio.Equals(value))return;_target.aspectRatio=value;_aspectRatio=value;}}
        private UnityEngine.UIElements.StyleColor _backgroundColor;
        public UnityEngine.UIElements.StyleColor backgroundColor {get=>_target.backgroundColor;set {if(_backgroundColor.Equals(value))return;_target.backgroundColor=value;_backgroundColor=value;}}
        private UnityEngine.UIElements.StyleBackground _backgroundImage;
        public UnityEngine.UIElements.StyleBackground backgroundImage {get=>_target.backgroundImage;set {if(_backgroundImage.Equals(value))return;_target.backgroundImage=value;_backgroundImage=value;}}
        private UnityEngine.UIElements.StyleBackgroundPosition _backgroundPositionX;
        public UnityEngine.UIElements.StyleBackgroundPosition backgroundPositionX {get=>_target.backgroundPositionX;set {if(_backgroundPositionX.Equals(value))return;_target.backgroundPositionX=value;_backgroundPositionX=value;}}
        private UnityEngine.UIElements.StyleBackgroundPosition _backgroundPositionY;
        public UnityEngine.UIElements.StyleBackgroundPosition backgroundPositionY {get=>_target.backgroundPositionY;set {if(_backgroundPositionY.Equals(value))return;_target.backgroundPositionY=value;_backgroundPositionY=value;}}
        private UnityEngine.UIElements.StyleBackgroundRepeat _backgroundRepeat;
        public UnityEngine.UIElements.StyleBackgroundRepeat backgroundRepeat {get=>_target.backgroundRepeat;set {if(_backgroundRepeat.Equals(value))return;_target.backgroundRepeat=value;_backgroundRepeat=value;}}
        private UnityEngine.UIElements.StyleBackgroundSize _backgroundSize;
        public UnityEngine.UIElements.StyleBackgroundSize backgroundSize {get=>_target.backgroundSize;set {if(_backgroundSize.Equals(value))return;_target.backgroundSize=value;_backgroundSize=value;}}
        private UnityEngine.UIElements.StyleColor _borderBottomColor;
        public UnityEngine.UIElements.StyleColor borderBottomColor {get=>_target.borderBottomColor;set {if(_borderBottomColor.Equals(value))return;_target.borderBottomColor=value;_borderBottomColor=value;}}
        private UnityEngine.UIElements.StyleLength _borderBottomLeftRadius;
        public UnityEngine.UIElements.StyleLength borderBottomLeftRadius {get=>_target.borderBottomLeftRadius;set {if(_borderBottomLeftRadius.Equals(value))return;_target.borderBottomLeftRadius=value;_borderBottomLeftRadius=value;}}
        private UnityEngine.UIElements.StyleLength _borderBottomRightRadius;
        public UnityEngine.UIElements.StyleLength borderBottomRightRadius {get=>_target.borderBottomRightRadius;set {if(_borderBottomRightRadius.Equals(value))return;_target.borderBottomRightRadius=value;_borderBottomRightRadius=value;}}
        private UnityEngine.UIElements.StyleFloat _borderBottomWidth;
        public UnityEngine.UIElements.StyleFloat borderBottomWidth {get=>_target.borderBottomWidth;set {if(_borderBottomWidth.Equals(value))return;_target.borderBottomWidth=value;_borderBottomWidth=value;}}
        private UnityEngine.UIElements.StyleColor _borderLeftColor;
        public UnityEngine.UIElements.StyleColor borderLeftColor {get=>_target.borderLeftColor;set {if(_borderLeftColor.Equals(value))return;_target.borderLeftColor=value;_borderLeftColor=value;}}
        private UnityEngine.UIElements.StyleFloat _borderLeftWidth;
        public UnityEngine.UIElements.StyleFloat borderLeftWidth {get=>_target.borderLeftWidth;set {if(_borderLeftWidth.Equals(value))return;_target.borderLeftWidth=value;_borderLeftWidth=value;}}
        private UnityEngine.UIElements.StyleColor _borderRightColor;
        public UnityEngine.UIElements.StyleColor borderRightColor {get=>_target.borderRightColor;set {if(_borderRightColor.Equals(value))return;_target.borderRightColor=value;_borderRightColor=value;}}
        private UnityEngine.UIElements.StyleFloat _borderRightWidth;
        public UnityEngine.UIElements.StyleFloat borderRightWidth {get=>_target.borderRightWidth;set {if(_borderRightWidth.Equals(value))return;_target.borderRightWidth=value;_borderRightWidth=value;}}
        private UnityEngine.UIElements.StyleColor _borderTopColor;
        public UnityEngine.UIElements.StyleColor borderTopColor {get=>_target.borderTopColor;set {if(_borderTopColor.Equals(value))return;_target.borderTopColor=value;_borderTopColor=value;}}
        private UnityEngine.UIElements.StyleLength _borderTopLeftRadius;
        public UnityEngine.UIElements.StyleLength borderTopLeftRadius {get=>_target.borderTopLeftRadius;set {if(_borderTopLeftRadius.Equals(value))return;_target.borderTopLeftRadius=value;_borderTopLeftRadius=value;}}
        private UnityEngine.UIElements.StyleLength _borderTopRightRadius;
        public UnityEngine.UIElements.StyleLength borderTopRightRadius {get=>_target.borderTopRightRadius;set {if(_borderTopRightRadius.Equals(value))return;_target.borderTopRightRadius=value;_borderTopRightRadius=value;}}
        private UnityEngine.UIElements.StyleFloat _borderTopWidth;
        public UnityEngine.UIElements.StyleFloat borderTopWidth {get=>_target.borderTopWidth;set {if(_borderTopWidth.Equals(value))return;_target.borderTopWidth=value;_borderTopWidth=value;}}
        private UnityEngine.UIElements.StyleLength _bottom;
        public UnityEngine.UIElements.StyleLength bottom {get=>_target.bottom;set {if(_bottom.Equals(value))return;_target.bottom=value;_bottom=value;}}
        private UnityEngine.UIElements.StyleColor _color;
        public UnityEngine.UIElements.StyleColor color {get=>_target.color;set {if(_color.Equals(value))return;_target.color=value;_color=value;}}
        private UnityEngine.UIElements.StyleCursor _cursor;
        public UnityEngine.UIElements.StyleCursor cursor {get=>_target.cursor;set {if(_cursor.Equals(value))return;_target.cursor=value;_cursor=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.DisplayStyle> _display;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.DisplayStyle> display {get=>_target.display;set {if(_display.Equals(value))return;_target.display=value;_display=value;}}
        public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.FilterFunction> filter {get=>_target.filter;set=>_target.filter=value;}
        private UnityEngine.UIElements.StyleLength _flexBasis;
        public UnityEngine.UIElements.StyleLength flexBasis {get=>_target.flexBasis;set {if(_flexBasis.Equals(value))return;_target.flexBasis=value;_flexBasis=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.FlexDirection> _flexDirection;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.FlexDirection> flexDirection {get=>_target.flexDirection;set {if(_flexDirection.Equals(value))return;_target.flexDirection=value;_flexDirection=value;}}
        private UnityEngine.UIElements.StyleFloat _flexGrow;
        public UnityEngine.UIElements.StyleFloat flexGrow {get=>_target.flexGrow;set {if(_flexGrow.Equals(value))return;_target.flexGrow=value;_flexGrow=value;}}
        private UnityEngine.UIElements.StyleFloat _flexShrink;
        public UnityEngine.UIElements.StyleFloat flexShrink {get=>_target.flexShrink;set {if(_flexShrink.Equals(value))return;_target.flexShrink=value;_flexShrink=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Wrap> _flexWrap;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Wrap> flexWrap {get=>_target.flexWrap;set {if(_flexWrap.Equals(value))return;_target.flexWrap=value;_flexWrap=value;}}
        private UnityEngine.UIElements.StyleLength _fontSize;
        public UnityEngine.UIElements.StyleLength fontSize {get=>_target.fontSize;set {if(_fontSize.Equals(value))return;_target.fontSize=value;_fontSize=value;}}
        private UnityEngine.UIElements.StyleLength _height;
        public UnityEngine.UIElements.StyleLength height {get=>_target.height;set {if(_height.Equals(value))return;_target.height=value;_height=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Justify> _justifyContent;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Justify> justifyContent {get=>_target.justifyContent;set {if(_justifyContent.Equals(value))return;_target.justifyContent=value;_justifyContent=value;}}
        private UnityEngine.UIElements.StyleLength _left;
        public UnityEngine.UIElements.StyleLength left {get=>_target.left;set {if(_left.Equals(value))return;_target.left=value;_left=value;}}
        private UnityEngine.UIElements.StyleLength _letterSpacing;
        public UnityEngine.UIElements.StyleLength letterSpacing {get=>_target.letterSpacing;set {if(_letterSpacing.Equals(value))return;_target.letterSpacing=value;_letterSpacing=value;}}
        private UnityEngine.UIElements.StyleLength _marginBottom;
        public UnityEngine.UIElements.StyleLength marginBottom {get=>_target.marginBottom;set {if(_marginBottom.Equals(value))return;_target.marginBottom=value;_marginBottom=value;}}
        private UnityEngine.UIElements.StyleLength _marginLeft;
        public UnityEngine.UIElements.StyleLength marginLeft {get=>_target.marginLeft;set {if(_marginLeft.Equals(value))return;_target.marginLeft=value;_marginLeft=value;}}
        private UnityEngine.UIElements.StyleLength _marginRight;
        public UnityEngine.UIElements.StyleLength marginRight {get=>_target.marginRight;set {if(_marginRight.Equals(value))return;_target.marginRight=value;_marginRight=value;}}
        private UnityEngine.UIElements.StyleLength _marginTop;
        public UnityEngine.UIElements.StyleLength marginTop {get=>_target.marginTop;set {if(_marginTop.Equals(value))return;_target.marginTop=value;_marginTop=value;}}
        private UnityEngine.UIElements.StyleLength _maxHeight;
        public UnityEngine.UIElements.StyleLength maxHeight {get=>_target.maxHeight;set {if(_maxHeight.Equals(value))return;_target.maxHeight=value;_maxHeight=value;}}
        private UnityEngine.UIElements.StyleLength _maxWidth;
        public UnityEngine.UIElements.StyleLength maxWidth {get=>_target.maxWidth;set {if(_maxWidth.Equals(value))return;_target.maxWidth=value;_maxWidth=value;}}
        private UnityEngine.UIElements.StyleLength _minHeight;
        public UnityEngine.UIElements.StyleLength minHeight {get=>_target.minHeight;set {if(_minHeight.Equals(value))return;_target.minHeight=value;_minHeight=value;}}
        private UnityEngine.UIElements.StyleLength _minWidth;
        public UnityEngine.UIElements.StyleLength minWidth {get=>_target.minWidth;set {if(_minWidth.Equals(value))return;_target.minWidth=value;_minWidth=value;}}
        private UnityEngine.UIElements.StyleFloat _opacity;
        public UnityEngine.UIElements.StyleFloat opacity {get=>_target.opacity;set {if(_opacity.Equals(value))return;_target.opacity=value;_opacity=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Overflow> _overflow;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Overflow> overflow {get=>_target.overflow;set {if(_overflow.Equals(value))return;_target.overflow=value;_overflow=value;}}
        private UnityEngine.UIElements.StyleLength _paddingBottom;
        public UnityEngine.UIElements.StyleLength paddingBottom {get=>_target.paddingBottom;set {if(_paddingBottom.Equals(value))return;_target.paddingBottom=value;_paddingBottom=value;}}
        private UnityEngine.UIElements.StyleLength _paddingLeft;
        public UnityEngine.UIElements.StyleLength paddingLeft {get=>_target.paddingLeft;set {if(_paddingLeft.Equals(value))return;_target.paddingLeft=value;_paddingLeft=value;}}
        private UnityEngine.UIElements.StyleLength _paddingRight;
        public UnityEngine.UIElements.StyleLength paddingRight {get=>_target.paddingRight;set {if(_paddingRight.Equals(value))return;_target.paddingRight=value;_paddingRight=value;}}
        private UnityEngine.UIElements.StyleLength _paddingTop;
        public UnityEngine.UIElements.StyleLength paddingTop {get=>_target.paddingTop;set {if(_paddingTop.Equals(value))return;_target.paddingTop=value;_paddingTop=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Position> _position;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Position> position {get=>_target.position;set {if(_position.Equals(value))return;_target.position=value;_position=value;}}
        private UnityEngine.UIElements.StyleLength _right;
        public UnityEngine.UIElements.StyleLength right {get=>_target.right;set {if(_right.Equals(value))return;_target.right=value;_right=value;}}
        private UnityEngine.UIElements.StyleRotate _rotate;
        public UnityEngine.UIElements.StyleRotate rotate {get=>_target.rotate;set {if(_rotate.Equals(value))return;_target.rotate=value;_rotate=value;}}
        private UnityEngine.UIElements.StyleScale _scale;
        public UnityEngine.UIElements.StyleScale scale {get=>_target.scale;set {if(_scale.Equals(value))return;_target.scale=value;_scale=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflow> _textOverflow;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflow> textOverflow {get=>_target.textOverflow;set {if(_textOverflow.Equals(value))return;_target.textOverflow=value;_textOverflow=value;}}
        private UnityEngine.UIElements.StyleTextShadow _textShadow;
        public UnityEngine.UIElements.StyleTextShadow textShadow {get=>_target.textShadow;set {if(_textShadow.Equals(value))return;_target.textShadow=value;_textShadow=value;}}
        private UnityEngine.UIElements.StyleLength _top;
        public UnityEngine.UIElements.StyleLength top {get=>_target.top;set {if(_top.Equals(value))return;_target.top=value;_top=value;}}
        private UnityEngine.UIElements.StyleTransformOrigin _transformOrigin;
        public UnityEngine.UIElements.StyleTransformOrigin transformOrigin {get=>_target.transformOrigin;set {if(_transformOrigin.Equals(value))return;_target.transformOrigin=value;_transformOrigin=value;}}
        public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.TimeValue> transitionDelay {get=>_target.transitionDelay;set=>_target.transitionDelay=value;}
        public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.TimeValue> transitionDuration {get=>_target.transitionDuration;set=>_target.transitionDuration=value;}
        public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.StylePropertyName> transitionProperty {get=>_target.transitionProperty;set=>_target.transitionProperty=value;}
        public UnityEngine.UIElements.StyleList<UnityEngine.UIElements.EasingFunction> transitionTimingFunction {get=>_target.transitionTimingFunction;set=>_target.transitionTimingFunction=value;}
        private UnityEngine.UIElements.StyleTranslate _translate;
        public UnityEngine.UIElements.StyleTranslate translate {get=>_target.translate;set {if(_translate.Equals(value))return;_target.translate=value;_translate=value;}}
        private UnityEngine.UIElements.StyleColor _unityBackgroundImageTintColor;
        public UnityEngine.UIElements.StyleColor unityBackgroundImageTintColor {get=>_target.unityBackgroundImageTintColor;set {if(_unityBackgroundImageTintColor.Equals(value))return;_target.unityBackgroundImageTintColor=value;_unityBackgroundImageTintColor=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.EditorTextRenderingMode> _unityEditorTextRenderingMode;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.EditorTextRenderingMode> unityEditorTextRenderingMode {get=>_target.unityEditorTextRenderingMode;set {if(_unityEditorTextRenderingMode.Equals(value))return;_target.unityEditorTextRenderingMode=value;_unityEditorTextRenderingMode=value;}}
        private UnityEngine.UIElements.StyleFont _unityFont;
        public UnityEngine.UIElements.StyleFont unityFont {get=>_target.unityFont;set {if(_unityFont.Equals(value))return;_target.unityFont=value;_unityFont=value;}}
        private UnityEngine.UIElements.StyleFontDefinition _unityFontDefinition;
        public UnityEngine.UIElements.StyleFontDefinition unityFontDefinition {get=>_target.unityFontDefinition;set {if(_unityFontDefinition.Equals(value))return;_target.unityFontDefinition=value;_unityFontDefinition=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.FontStyle> _unityFontStyleAndWeight;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.FontStyle> unityFontStyleAndWeight {get=>_target.unityFontStyleAndWeight;set {if(_unityFontStyleAndWeight.Equals(value))return;_target.unityFontStyleAndWeight=value;_unityFontStyleAndWeight=value;}}
        private UnityEngine.UIElements.StyleMaterialDefinition _unityMaterial;
        public UnityEngine.UIElements.StyleMaterialDefinition unityMaterial {get=>_target.unityMaterial;set {if(_unityMaterial.Equals(value))return;_target.unityMaterial=value;_unityMaterial=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.OverflowClipBox> _unityOverflowClipBox;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.OverflowClipBox> unityOverflowClipBox {get=>_target.unityOverflowClipBox;set {if(_unityOverflowClipBox.Equals(value))return;_target.unityOverflowClipBox=value;_unityOverflowClipBox=value;}}
        private UnityEngine.UIElements.StyleLength _unityParagraphSpacing;
        public UnityEngine.UIElements.StyleLength unityParagraphSpacing {get=>_target.unityParagraphSpacing;set {if(_unityParagraphSpacing.Equals(value))return;_target.unityParagraphSpacing=value;_unityParagraphSpacing=value;}}
        private UnityEngine.UIElements.StyleInt _unitySliceBottom;
        public UnityEngine.UIElements.StyleInt unitySliceBottom {get=>_target.unitySliceBottom;set {if(_unitySliceBottom.Equals(value))return;_target.unitySliceBottom=value;_unitySliceBottom=value;}}
        private UnityEngine.UIElements.StyleInt _unitySliceLeft;
        public UnityEngine.UIElements.StyleInt unitySliceLeft {get=>_target.unitySliceLeft;set {if(_unitySliceLeft.Equals(value))return;_target.unitySliceLeft=value;_unitySliceLeft=value;}}
        private UnityEngine.UIElements.StyleInt _unitySliceRight;
        public UnityEngine.UIElements.StyleInt unitySliceRight {get=>_target.unitySliceRight;set {if(_unitySliceRight.Equals(value))return;_target.unitySliceRight=value;_unitySliceRight=value;}}
        private UnityEngine.UIElements.StyleFloat _unitySliceScale;
        public UnityEngine.UIElements.StyleFloat unitySliceScale {get=>_target.unitySliceScale;set {if(_unitySliceScale.Equals(value))return;_target.unitySliceScale=value;_unitySliceScale=value;}}
        private UnityEngine.UIElements.StyleInt _unitySliceTop;
        public UnityEngine.UIElements.StyleInt unitySliceTop {get=>_target.unitySliceTop;set {if(_unitySliceTop.Equals(value))return;_target.unitySliceTop=value;_unitySliceTop=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.SliceType> _unitySliceType;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.SliceType> unitySliceType {get=>_target.unitySliceType;set {if(_unitySliceType.Equals(value))return;_target.unitySliceType=value;_unitySliceType=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.TextAnchor> _unityTextAlign;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.TextAnchor> unityTextAlign {get=>_target.unityTextAlign;set {if(_unityTextAlign.Equals(value))return;_target.unityTextAlign=value;_unityTextAlign=value;}}
        private UnityEngine.UIElements.StyleTextAutoSize _unityTextAutoSize;
        public UnityEngine.UIElements.StyleTextAutoSize unityTextAutoSize {get=>_target.unityTextAutoSize;set {if(_unityTextAutoSize.Equals(value))return;_target.unityTextAutoSize=value;_unityTextAutoSize=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.TextGeneratorType> _unityTextGenerator;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.TextGeneratorType> unityTextGenerator {get=>_target.unityTextGenerator;set {if(_unityTextGenerator.Equals(value))return;_target.unityTextGenerator=value;_unityTextGenerator=value;}}
        private UnityEngine.UIElements.StyleColor _unityTextOutlineColor;
        public UnityEngine.UIElements.StyleColor unityTextOutlineColor {get=>_target.unityTextOutlineColor;set {if(_unityTextOutlineColor.Equals(value))return;_target.unityTextOutlineColor=value;_unityTextOutlineColor=value;}}
        private UnityEngine.UIElements.StyleFloat _unityTextOutlineWidth;
        public UnityEngine.UIElements.StyleFloat unityTextOutlineWidth {get=>_target.unityTextOutlineWidth;set {if(_unityTextOutlineWidth.Equals(value))return;_target.unityTextOutlineWidth=value;_unityTextOutlineWidth=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflowPosition> _unityTextOverflowPosition;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.TextOverflowPosition> unityTextOverflowPosition {get=>_target.unityTextOverflowPosition;set {if(_unityTextOverflowPosition.Equals(value))return;_target.unityTextOverflowPosition=value;_unityTextOverflowPosition=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Visibility> _visibility;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.Visibility> visibility {get=>_target.visibility;set {if(_visibility.Equals(value))return;_target.visibility=value;_visibility=value;}}
        private UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.WhiteSpace> _whiteSpace;
        public UnityEngine.UIElements.StyleEnum<UnityEngine.UIElements.WhiteSpace> whiteSpace {get=>_target.whiteSpace;set {if(_whiteSpace.Equals(value))return;_target.whiteSpace=value;_whiteSpace=value;}}
        private UnityEngine.UIElements.StyleLength _width;
        public UnityEngine.UIElements.StyleLength width {get=>_target.width;set {if(_width.Equals(value))return;_target.width=value;_width=value;}}
        private UnityEngine.UIElements.StyleLength _wordSpacing;
        public UnityEngine.UIElements.StyleLength wordSpacing {get=>_target.wordSpacing;set {if(_wordSpacing.Equals(value))return;_target.wordSpacing=value;_wordSpacing=value;}}
        // This legacy alias changes several background properties at once.
        public UnityEngine.UIElements.StyleEnum<UnityEngine.ScaleMode> unityBackgroundScaleMode {get=>_target.unityBackgroundScaleMode;set {_target.unityBackgroundScaleMode=value;Read();}}
    }
}
