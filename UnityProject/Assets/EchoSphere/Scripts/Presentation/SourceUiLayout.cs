#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Maps authored CSS layout values to Unity's retained layout system. Grid
    // tracks and gaps are explicit adapters because USS does not implement them.
    internal sealed class SourceUiLayout
    {
        public readonly VisualCss Sheet;
        public SourceUiLayout(VisualCss sheet){Sheet=sheet;}
        private string S(VisualCssNode node,string property,string fallback="")=>Sheet.Get(node,property,fallback);
        private float F(string value,float fallback=0f)=>float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var result)?result:fallback;
        internal float FontSize(VisualCssNode node)=>(float)Sheet.Length(S(node,"font-size","16px"),16d);
        internal float Length(VisualCssNode node,string property,double reference,float fallback=0f)
        {var result=Sheet.Length(S(node,property),reference,FontSize(node));return double.IsNaN(result)?fallback:(float)result;}
        private StyleLength Dimension(VisualCssNode node,string property,double reference)
        {
            var value=S(node,property,"auto");if(value=="auto"||value=="initial"||value=="none"||value=="max-content"||value=="min-content"||value=="fit-content")return new StyleLength(StyleKeyword.Auto);
            if(Regex.IsMatch(value,@"^[-\d.]+%$")&&S(node,"position")!="absolute"&&S(node,"position")!="fixed")return new StyleLength(new Length(F(value.TrimEnd('%')),LengthUnit.Percent));
            var number=Sheet.Length(value,reference,FontSize(node));return double.IsNaN(number)?new StyleLength(StyleKeyword.Auto):new StyleLength((float)number);
        }
        internal float Apply(VisualCssNode node,VisualElement element,VisualElement layoutParent=null)
        {
            var positioned=S(node,"position")=="absolute"||S(node,"position")=="fixed";var block=positioned?VisualCssPosition.ContainingBlock(Sheet,node):null;
            var parentWidth=positioned?VisualCssPosition.Width(Sheet,block):node.Parent?.Width??Sheet.Width;var parentHeight=positioned?VisualCssPosition.Height(Sheet,block):node.Parent?.Height??Sheet.Height;var style=element.style;
            style.width=Dimension(node,"width",parentWidth);style.height=Dimension(node,"height",parentHeight);
            if(S(node,"display")=="grid"&&!NativeAutoColumn(node)&&!NativeCenteredCell(node)&&S(node,"height","auto")=="auto"&&element.userData is SourceUiElement gridHost&&!float.IsNaN(gridHost.GridHeight))style.height=gridHost.GridHeight;
            style.minWidth=Dimension(node,"min-width",parentWidth);style.minHeight=Dimension(node,"min-height",parentHeight);
            style.maxWidth=Dimension(node,"max-width",parentWidth);style.maxHeight=Dimension(node,"max-height",parentHeight);
            var ratio=S(node,"aspect-ratio","auto");var ratioParts=ratio.Split('/');style.aspectRatio=ratio=="auto"?new StyleRatio(StyleKeyword.Auto):new StyleRatio(new Ratio(F(ratioParts[0],1f)/(ratioParts.Length>1?F(ratioParts[1],1f):1f)));
            style.marginLeft=Dimension(node,"margin-left",parentWidth);style.marginRight=Dimension(node,"margin-right",parentWidth);style.marginTop=Dimension(node,"margin-top",parentWidth);style.marginBottom=Dimension(node,"margin-bottom",parentWidth);
            if(!node.Style.ContainsKey("margin-left"))style.marginLeft=0;if(!node.Style.ContainsKey("margin-right"))style.marginRight=0;if(!node.Style.ContainsKey("margin-top"))style.marginTop=0;if(!node.Style.ContainsKey("margin-bottom"))style.marginBottom=0;
            style.paddingLeft=Length(node,"padding-left",parentWidth);style.paddingRight=Length(node,"padding-right",parentWidth);style.paddingTop=Length(node,"padding-top",parentWidth);style.paddingBottom=Length(node,"padding-bottom",parentWidth);
            float Border(string side)=>S(node,"border-"+side+"-style","none")=="none"?0f:Length(node,"border-"+side+"-width",parentWidth);
            style.borderLeftWidth=Border("left");style.borderRightWidth=Border("right");style.borderTopWidth=Border("top");style.borderBottomWidth=Border("bottom");
            style.borderLeftColor=style.borderRightColor=style.borderTopColor=style.borderBottomColor=Color.clear;
            style.borderTopLeftRadius=Dimension(node,"border-top-left-radius",node.Width);style.borderTopRightRadius=Dimension(node,"border-top-right-radius",node.Width);style.borderBottomLeftRadius=Dimension(node,"border-bottom-left-radius",node.Width);style.borderBottomRightRadius=Dimension(node,"border-bottom-right-radius",node.Width);
            style.backgroundColor=Color.clear;style.backgroundImage=new StyleBackground(StyleKeyword.None);
            var flexItem=node.Parent!=null&&(S(node.Parent,"display")=="flex"||S(node.Parent,"display")=="inline-flex");
            style.flexGrow=flexItem?F(S(node,"flex-grow","0")):0f;style.flexShrink=flexItem?F(S(node,"flex-shrink","1")):0f;style.flexBasis=flexItem?Dimension(node,"flex-basis",parentWidth):new StyleLength(StyleKeyword.Auto);
            var display=S(node,"display","block");style.display=display=="none"?DisplayStyle.None:DisplayStyle.Flex;
            var direction=S(node,"flex-direction",display=="flex"||display=="inline-flex"?"row":"column");
            if(display=="inline")direction="row";
            if(display=="grid")direction=GridColumns(node)>1?"row":"column";
            style.flexDirection=direction=="row"?FlexDirection.Row:direction=="row-reverse"?FlexDirection.RowReverse:direction=="column-reverse"?FlexDirection.ColumnReverse:FlexDirection.Column;
            style.flexWrap=S(node,"flex-wrap")=="wrap"||display=="grid"&&GridColumns(node)>1||display=="inline"?Wrap.Wrap:Wrap.NoWrap;
            style.alignItems=Alignment(S(node,"align-items",display=="grid"?S(node,"place-items","stretch"):"stretch"));
            style.alignSelf=S(node,"align-self","auto")=="auto"?Align.Auto:Alignment(S(node,"align-self"));
            style.alignContent=Alignment(S(node,"align-content","stretch"));
            var justify=S(node,"justify-content",display=="grid"?S(node,"place-items","start"):"start");
            style.justifyContent=justify=="center"?Justify.Center:justify=="end"||justify=="flex-end"?Justify.FlexEnd:justify=="space-between"?Justify.SpaceBetween:justify=="space-around"?Justify.SpaceAround:Justify.FlexStart;
            if(NativeCenteredCell(node)){style.flexDirection=FlexDirection.Column;style.alignItems=Align.Center;style.justifyContent=Justify.Center;}
            style.position=positioned?Position.Absolute:Position.Relative;
            if(positioned){
                // Yoga's containing element is the immediate retained parent.
                // CSS instead uses a positioned ancestor, or the viewport for
                // fixed elements (unless an ancestor establishes a transform).
                var offset=Vector2.zero;var parent=layoutParent??element.parent;
                if(parent!=null){var containing=block?.Native as VisualElement;var coordinateBox=containing is SourceUiFrame frame?frame.Content:containing;var world=coordinateBox!=null?coordinateBox.worldTransform.MultiplyPoint3x4(new Vector3((float)VisualCssPosition.Border(Sheet,block,"left"),(float)VisualCssPosition.Border(Sheet,block,"top"),0f)):Vector3.zero;var local=parent.worldTransform.inverse.MultiplyPoint3x4(world);offset=new Vector2(local.x-parent.resolvedStyle.borderLeftWidth,local.y-parent.resolvedStyle.borderTopWidth);}
                StyleLength Inset(string property,double reference,float extra){var value=Sheet.Length(S(node,property,"auto"),reference,FontSize(node));return double.IsNaN(value)?new StyleLength(StyleKeyword.Auto):new StyleLength((float)value+extra);}
                var nativeWidth=parent?.layout.width??(float)Sheet.Width;var nativeHeight=parent?.layout.height??(float)Sheet.Height;
                if(parent!=null){nativeWidth-=parent.resolvedStyle.borderLeftWidth+parent.resolvedStyle.borderRightWidth;nativeHeight-=parent.resolvedStyle.borderTopWidth+parent.resolvedStyle.borderBottomWidth;}
                style.left=Inset("left",parentWidth,offset.x);style.right=Inset("right",parentWidth,nativeWidth-offset.x-(float)parentWidth);style.top=Inset("top",parentHeight,offset.y);style.bottom=Inset("bottom",parentHeight,nativeHeight-offset.y-(float)parentHeight);
                if(S(node,"width").EndsWith("%",StringComparison.Ordinal))style.width=Length(node,"width",parentWidth);if(S(node,"height").EndsWith("%",StringComparison.Ordinal))style.height=Length(node,"height",parentHeight);
            }else if(S(node,"position")=="relative"){style.left=Dimension(node,"left",parentWidth);style.right=Dimension(node,"right",parentWidth);style.top=Dimension(node,"top",parentHeight);style.bottom=Dimension(node,"bottom",parentHeight);}else {style.left=StyleKeyword.Auto;style.right=StyleKeyword.Auto;style.top=StyleKeyword.Auto;style.bottom=StyleKeyword.Auto;}
            style.overflow=S(node,"overflow")=="hidden"||S(node,"overflow-x")=="hidden"||S(node,"overflow-y")=="hidden"?Overflow.Hidden:Overflow.Visible;
            style.visibility=S(node,"visibility")=="hidden"?Visibility.Hidden:Visibility.Visible;
            SourceUiCursor.Apply(style,S(node,"cursor","auto"));
            style.opacity=F(S(node,"opacity","1"),1f);
            element.pickingMode=S(node,"pointer-events")=="none"?PickingMode.Ignore:PickingMode.Position;
            element.focusable=node.Pseudo==null&&(node.Tag=="input"||node.Tag=="button"||node.Properties["tabIndex"].Number>=0);
            var color=VisualCssColor.Parse(S(node,"color","#000000"));style.color=new Color((float)color.R,(float)color.G,(float)color.B,(float)color.A);
            var fontSize=FontSize(node);style.fontSize=fontSize;
            SourceUiTypography.Apply(Sheet,node,element,fontSize);
            style.letterSpacing=Length(node,"letter-spacing",fontSize);style.whiteSpace=S(node,"white-space")=="nowrap"||S(node,"white-space")=="pre"?WhiteSpace.NoWrap:WhiteSpace.Normal;
            style.unityTextAlign=S(node,"text-align")=="center"?TextAnchor.MiddleCenter:S(node,"text-align")=="right"?TextAnchor.MiddleRight:TextAnchor.MiddleLeft;
            style.textOverflow=S(node,"text-overflow")=="ellipsis"?TextOverflow.Ellipsis:TextOverflow.Clip;
            var shadow=S(node,"text-shadow","none");if(shadow!="none"){
                var words=VisualCss.Split(shadow,' ').Where(value=>value.Length>0).ToArray();var lengths=words.Where(value=>Regex.IsMatch(value,@"^[-+\d.]")).Select(value=>(float)Sheet.Length(value,fontSize,fontSize)).ToArray();
                var tint=VisualCssColor.Parse(words.FirstOrDefault(value=>value.StartsWith("#",StringComparison.Ordinal)||value.Contains("(")||value=="currentColor")??S(node,"color","#000000"),S(node,"color","#000000"));
                style.textShadow=new TextShadow {offset=new Vector2(lengths.Length>0?lengths[0]:0f,lengths.Length>1?lengths[1]:0f),blurRadius=lengths.Length>2?lengths[2]:0f,color=new Color((float)tint.R,(float)tint.G,(float)tint.B,(float)tint.A)};
            }else style.textShadow=new TextShadow();
            if(node.Parent==null){style.width=(float)Sheet.Width;style.height=(float)Sheet.Height;style.flexShrink=0;}
            if(node.Tag=="svg"){if(!node.Style.ContainsKey("width")&&!node.Properties["width"].Undefined)style.width=(float)node.Properties["width"].Number;if(!node.Style.ContainsKey("height")&&!node.Properties["height"].Undefined)style.height=(float)node.Properties["height"].Number;style.flexShrink=0;}
            if(element is TextElement text){
                var insideButton=false;for(var ancestor=node;ancestor!=null;ancestor=ancestor.Parent)if(ancestor.Tag=="button"){insideButton=true;break;}
                text.selection.isSelectable=!insideButton&&S(node,"user-select","auto")!="none"&&S(node,"pointer-events","auto")!="none";
                text.focusable=node.Properties["tabIndex"].Number>=0||node.Tag=="button";
                if(SourceUiInline.CanMerge(Sheet,node)&&node.Children.Any(child=>child.Tag!="#text")){text.enableRichText=true;text.text=SourceUiInline.Text(Sheet,node);}
                else {var content=node.Tag=="#text"?node.Text:string.Concat(node.Children.Where(child=>child.Tag=="#text").Select(child=>child.Text));if(S(node,"white-space")!="pre"&&S(node,"white-space")!="pre-wrap")content=Regex.Replace(content,@"\s+"," ");if(S(node,"text-transform")=="uppercase")content=content.ToUpperInvariant();else if(S(node,"text-transform")=="lowercase")content=content.ToLowerInvariant();text.text=content;text.enableRichText=false;}
            }
            if(S(node,"min-width")=="max-content")style.minWidth=IntrinsicWidth(node);
            var contentRotation=ApplyTransform(node,element);
            if(element is Slider range)SourceUiRange.Apply(range);
            if(element is ScrollView scroll){
                var hidden=S(node,"scrollbar-width")=="none";var both=S(node,"overflow")=="auto";
                scroll.verticalScrollerVisibility=hidden||!both&&S(node,"overflow-y")!="auto"?ScrollerVisibility.Hidden:ScrollerVisibility.Auto;
                scroll.horizontalScrollerVisibility=hidden||!both&&S(node,"overflow-x")!="auto"?ScrollerVisibility.Hidden:ScrollerVisibility.Auto;
                // ScrollView's authored children live in its contentContainer.
                // Padding belongs to that scrolling content, and its layout
                // direction/alignment must match the original DOM element.
                var content=scroll.contentContainer.style;content.paddingLeft=style.paddingLeft;content.paddingRight=style.paddingRight;content.paddingTop=style.paddingTop;content.paddingBottom=style.paddingBottom;
                style.paddingLeft=style.paddingRight=style.paddingTop=style.paddingBottom=0;
                content.flexDirection=style.flexDirection;content.flexWrap=style.flexWrap;content.alignItems=style.alignItems;content.alignContent=style.alignContent;content.justifyContent=style.justifyContent;content.flexGrow=0;content.flexShrink=0;
            }
            return contentRotation;
        }
        private float IntrinsicWidth(VisualCssNode node)
        {
            var reference=node.Parent?.Width??Sheet.Width;var authored=Sheet.Length(S(node,"width","auto"),reference,FontSize(node));if(!double.IsNaN(authored))return (float)authored;
            var edges=Length(node,"padding-left",reference)+Length(node,"padding-right",reference)+Length(node,"border-left-width",reference)+Length(node,"border-right-width",reference);
            var content=node.Native is VisualElement native&&native.userData is SourceUiElement host?host.Element:node.Native;
            if(content is TextElement text)return text.MeasureTextSize(text.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined).x+edges;
            if(node.Tag=="svg")return (float)(node.Properties["width"].Undefined?node.Width:node.Properties["width"].Number);
            var widths=node.Children.Where(child=>S(child,"display")!="none"&&S(child,"position")!="absolute"&&S(child,"position")!="fixed").Select(child=>IntrinsicWidth(child)+Length(child,"margin-left",reference)+Length(child,"margin-right",reference)).ToArray();
            var display=S(node,"display");var horizontal=S(node,"flex-direction",display=="flex"||display=="inline-flex"?"row":"column").StartsWith("row",StringComparison.Ordinal);
            var gap=VisualCss.Split(S(node,"gap","0"),' ').Where(part=>part.Length>0).ToArray();var columnGap=gap.Length>1?gap[1]:gap.Length>0?gap[0]:"0";var spacing=Sheet.Length(S(node,"column-gap",columnGap),reference,FontSize(node));
            return edges+(horizontal?widths.Sum()+(float)(double.IsNaN(spacing)?0d:spacing)*Math.Max(0,widths.Length-1):widths.DefaultIfEmpty(0f).Max());
        }
        private static Align Alignment(string value)=>value=="center"?Align.Center:value=="start"||value=="flex-start"?Align.FlexStart:value=="end"||value=="flex-end"?Align.FlexEnd:value=="baseline"?Align.FlexStart:Align.Stretch;
        internal int GridColumns(VisualCssNode node)
        {
            return VisualCssGrid.Columns(Sheet,node);
        }
        // A single full-width column with intrinsic rows is exactly a native
        // vertical flow. Do not feed its previous assigned heights back into
        // auto-track sizing: that freezes initial or stretched measurements.
        internal bool NativeAutoColumn(VisualCssNode node)
        {
            if(S(node,"display")!="grid"||S(node,"grid-template-columns","none")!="1fr"||
                S(node,"grid-template-rows","auto")!="auto"||S(node,"height","auto")!="auto"||
                S(node,"min-height","0")!="0"||S(node,"max-height","none")!="none"||
                S(node,"align-items","stretch")!="stretch"||S(node,"justify-items","stretch")!="stretch"||
                S(node,"place-items","stretch")!="stretch"||S(node,"align-content","normal")!="normal"||
                S(node,"justify-content","start")!="start"||S(node,"flex-direction","column")!="column")return false;
            foreach(var child in node.Children){
                if(S(child,"display")=="none"||S(child,"position")=="absolute"||S(child,"position")=="fixed")continue;
                if(S(child,"align-self","auto")!="auto"||S(child,"justify-self","auto")!="auto"||
                    S(child,"grid-row","auto")!="auto"||S(child,"grid-column","auto")!="auto"||
                    (S(child,"z-index","auto")!="auto"&&S(child,"z-index")!="0"))return false;
            }
            return true;
        }
        // A single centered intrinsic item needs no explicit grid track. Native
        // alignment remeasures it after resize instead of feeding a previously
        // assigned absolute height back into the next intrinsic calculation.
        internal bool NativeCenteredCell(VisualCssNode node)
        {
            if(S(node,"display")!="grid"||
                S(node,"align-items",S(node,"place-items"))!="center"||S(node,"justify-items",S(node,"place-items"))!="center"||
                S(node,"grid-auto-rows","auto")!="auto"||S(node,"grid-auto-columns","auto")!="auto")return false;
            var columns=S(node,"grid-template-columns","none");var rows=S(node,"grid-template-rows","none");
            if((columns!="none"&&columns!="1fr")||(rows!="none"&&rows!="auto"&&rows!="1fr"))return false;
            var align=S(node,"align-content","normal");var justify=S(node,"justify-content","normal");
            if((align!="normal"&&align!="stretch"&&align!="center")||(justify!="normal"&&justify!="stretch"&&justify!="center"))return false;
            var count=0;
            foreach(var child in node.Children){
                if(S(child,"display")=="none"||S(child,"position")=="absolute"||S(child,"position")=="fixed")continue;
                if(++count>1||S(child,"align-self","auto")!="auto"||S(child,"justify-self","auto")!="auto"||
                    S(child,"grid-row","auto")!="auto"||S(child,"grid-column","auto")!="auto")return false;
            }
            return count==1;
        }
        internal void Tracks(VisualCssNode node)
        {
            if(!(node.Native is VisualElement element))return;
            if(S(node,"display")=="grid"&&!NativeAutoColumn(node)&&!NativeCenteredCell(node)){
                // Auto tracks need the first native intrinsic measurement.
                // Projecting the initial unresolved bounds as zero makes those
                // children absolute/zero-height before Yoga can measure them.
                if(float.IsNaN(element.layout.width)||float.IsNaN(element.layout.height)||
                    node.Children.Any(child=>child.Native is VisualElement pending&&
                        S(child,"display")!="none"&&float.IsNaN(pending.layout.height))){
                    foreach(var child in node.Children)Tracks(child);
                    return;
                }
                var grid=VisualCssGrid.Compute(Sheet,node);var borderLeft=Length(node,"border-left-width",node.Width);var borderTop=Length(node,"border-top-width",node.Width);
                foreach(var cell in grid.Cells)if(cell.Node.Native is VisualElement child){var parent=child.parent;child.style.position=Position.Absolute;child.style.left=(float)cell.X-(parent?.resolvedStyle.borderLeftWidth??borderLeft);child.style.top=(float)cell.Y-(parent?.resolvedStyle.borderTopWidth??borderTop);child.style.right=StyleKeyword.Auto;child.style.bottom=StyleKeyword.Auto;child.style.width=(float)cell.Width;child.style.height=(float)cell.Height;child.style.marginLeft=0;child.style.marginRight=0;child.style.marginTop=0;child.style.marginBottom=0;}
                if(S(node,"height","auto")=="auto"){var height=(float)grid.Height;element.style.height=height;if(element.userData is SourceUiElement host)host.GridHeight=height;}
                foreach(var child in node.Children)Tracks(child);return;
            }
            var children=node.Children.Where(child=>child.Native is VisualElement&&S(child,"display")!="none"&&S(child,"position")!="absolute"&&S(child,"position")!="fixed").ToList();
            var contentWidth=Math.Max(0d,node.Width-Length(node,"padding-left",node.Width)-Length(node,"padding-right",node.Width)-Length(node,"border-left-width",node.Width)-Length(node,"border-right-width",node.Width));
            var gap=VisualCss.Split(S(node,"gap","0"),' ').Where(part=>part.Length>0).ToArray();var rowGap=gap.Length>0?(float)Sheet.Length(gap[0],contentWidth,FontSize(node)):0f;var columnGap=gap.Length>1?(float)Sheet.Length(gap[1],contentWidth,FontSize(node)):rowGap;
            if(float.IsNaN(rowGap))rowGap=0;if(float.IsNaN(columnGap))columnGap=0;
            var usedWidth=0f;var row=0;var column=0;
            void LeadingGap(VisualCssNode child,VisualCssNode previous,bool horizontal,float spacing){
                var leading=horizontal?"margin-left":"margin-top";var trailing=horizontal?"margin-right":"margin-bottom";var native=(VisualElement)child.Native;
                if(S(child,leading)=="auto"){
                    if(horizontal)native.style.marginLeft=StyleKeyword.Auto;else native.style.marginTop=StyleKeyword.Auto;
                    // Gap remains fixed space; an auto margin still consumes
                    // free space. Put that gap on the preceding fixed margin.
                    if(previous?.Native is VisualElement before&&spacing>0f){if(horizontal)before.style.marginRight=Length(previous,trailing,contentWidth)+spacing;else before.style.marginBottom=Length(previous,trailing,contentWidth)+spacing;}
                }else if(horizontal)native.style.marginLeft=Length(child,leading,contentWidth)+spacing;else native.style.marginTop=Length(child,leading,contentWidth)+spacing;
            }
            for(var index=0;index<children.Count;index++) {
                var child=children[index];var native=(VisualElement)child.Native;
                if(S(node,"flex-direction",S(node,"display")=="flex"||S(node,"display")=="inline-flex"?"row":"column").StartsWith("column",StringComparison.Ordinal)){if(index>0)LeadingGap(child,children[index-1],false,rowGap);}
                else {var width=native.layout.width+Length(child,"margin-left",contentWidth)+Length(child,"margin-right",contentWidth);if(S(node,"flex-wrap")=="wrap"&&column>0&&usedWidth+columnGap+width>contentWidth+.001d){row++;column=0;usedWidth=0f;}LeadingGap(child,column>0?children[index-1]:null,true,column>0?columnGap:0f);if(S(child,"margin-top")!="auto")native.style.marginTop=Length(child,"margin-top",contentWidth)+(row>0?rowGap:0f);usedWidth+=width+(column>0?columnGap:0f);column++;}
            }
            foreach(var child in node.Children)Tracks(child);
        }
        private float ApplyTransform(VisualCssNode node,VisualElement element)
        {
            // Multiply in CSS list order before decomposing the authored TRS.
            // Summing translations separately incorrectly turns scale().translate()
            // into translate().scale(). Intersecting 3D planes use the full matrix
            // in SourceUiDepth; the retained element receives the matching TRS.
            var source=VisualCssTransform.Parse(Sheet,node);var matrix=new Matrix4x4();for(var column=0;column<4;column++)for(var row=0;row<4;row++)matrix[row,column]=(float)source[column*4+row];
            var position=new Vector3(matrix.m03,matrix.m13,matrix.m23);
            var origin=VisualCss.Split(S(node,"transform-origin","50% 50%"),' ').Where(part=>part.Length>0).ToArray();
            float Origin(string value,double reference)=>value=="center"?(float)reference*.5f:value=="left"||value=="top"?0f:value=="right"||value=="bottom"?(float)reference:(float)Sheet.Length(value,reference,FontSize(node));
            element.style.transformOrigin=new TransformOrigin(new Length(origin.Length>0?Origin(origin[0],node.Width):(float)node.Width*.5f),new Length(origin.Length>1?Origin(origin[1],node.Height):(float)node.Height*.5f),0f);
            element.style.translate=new Translate(position.x,position.y,position.z);
            if(VisualCssTransform.Is2D(source)){var split=VisualCssTransform.Native2D(source);element.style.rotate=new Rotate((float)(split.Rotation*Mathf.Rad2Deg));element.style.scale=new Scale(new Vector3((float)split.X,(float)split.Y,1));return (float)(split.ContentRotation*Mathf.Rad2Deg);}
            var rotation=matrix.rotation;rotation.ToAngleAxis(out var angle,out var axis);element.style.rotate=new Rotate(new Angle(angle,AngleUnit.Degree),axis);element.style.scale=new Scale(matrix.lossyScale);return 0f;
        }
    }
}
#endif
