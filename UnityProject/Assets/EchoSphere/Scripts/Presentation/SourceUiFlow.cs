using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // UI Toolkit uses one sibling order for both Yoga and painting. In a single
    // flex line, retain native sizing while restoring the logical positions.
    // Reorder the measured occupied spans, not the hierarchy or size calculation.
    // Complex flows stay on the existing path until independently supported.
    internal sealed class SourceUiFlow
    {
        private struct Item
        {
            internal SourceUiFrame Frame;
            internal double Order;
            internal int Index,PaintIndex;
            internal float Position,Size,Leading,Trailing;
        }
        private readonly List<Item> _items=new List<Item>();
        internal void Apply(VisualCss sheet,VisualCssNode node)
        {
            if(!ApplyParent(sheet,node))
                foreach(var child in node.Children)if(child.Native is SourceUiFrame frame)frame.SetFlowOffset(Vector2.zero);
            // The scratch list is reused only after this parent has been applied.
            foreach(var child in node.Children)Apply(sheet,child);
        }
        private bool ApplyParent(VisualCss sheet,VisualCssNode node)
        {
            var display=sheet.Get(node,"display");
            if((display!="flex"&&display!="inline-flex")||sheet.Get(node,"flex-wrap","nowrap")!="nowrap")return false;
            for(var ancestor=node;ancestor!=null;ancestor=ancestor.Parent)
                if(sheet.Get(ancestor,"transform-style")=="preserve-3d")return false;
            var direction=sheet.Get(node,"flex-direction","row");
            var horizontal=direction=="row"||direction=="row-reverse";
            var reverse=direction=="row-reverse"||direction=="column-reverse";
            var leading=horizontal?"margin-left":"margin-top";
            var trailing=horizontal?"margin-right":"margin-bottom";
            _items.Clear();VisualElement parent=null;
            for(var index=0;index<node.Children.Count;index++){
                var child=node.Children[index];if(!(child.Native is SourceUiFrame frame))continue;
                var position=sheet.Get(child,"position","static");
                if(sheet.Get(child,"display")=="none"||position=="absolute"||position=="fixed"){
                    frame.SetFlowOffset(Vector2.zero);continue;
                }
                // Auto margins and relative insets require their own measured
                // allocation policy. Never infer it from unresolved style data.
                if(sheet.Get(child,leading)=="auto"||sheet.Get(child,trailing)=="auto")return false;
                if(position=="relative"&&(sheet.Get(child,horizontal?"left":"top","auto")!="auto"||sheet.Get(child,horizontal?"right":"bottom","auto")!="auto"))return false;
                if(frame.parent==null||(parent!=null&&frame.parent!=parent))return false;
                parent=frame.parent;var bounds=frame.layout;var style=frame.resolvedStyle;
                var justify=parent.resolvedStyle.justifyContent;
                if(justify==Justify.SpaceBetween||justify==Justify.SpaceAround)return false;
                // Growing siblings can receive different rounded pixel sizes
                // after permutation. This projection must not approximate them.
                if(style.flexGrow!=0)return false;
                var item=new Item {Frame=frame,Order=sheet.Length(sheet.Get(child,"order","0"),1d),Index=index,
                    PaintIndex=parent.hierarchy.IndexOf(frame),Position=horizontal?bounds.x:bounds.y,Size=horizontal?bounds.width:bounds.height,
                    Leading=horizontal?style.marginLeft:style.marginTop,Trailing=horizontal?style.marginRight:style.marginBottom};
                if(!Finite(item.Position)||!Finite(item.Size)||!Finite(item.Leading)||!Finite(item.Trailing)||
                    item.Size<0||item.Leading<0||item.Trailing<0||style.display==DisplayStyle.None)return false;
                _items.Add(item);
            }
            if(_items.Count<2)return false;
            _items.Sort((a,b)=>{var order=a.Order.CompareTo(b.Order);return order!=0?order:a.Index.CompareTo(b.Index);});
            var reordered=false;
            for(var i=1;i<_items.Count;i++)if(_items[i-1].PaintIndex>_items[i].PaintIndex){reordered=true;break;}
            if(!reordered)return false;
            if(reverse)_items.Reverse();
            float start=float.PositiveInfinity,end=float.NegativeInfinity,occupied=0;
            foreach(var item in _items){start=Mathf.Min(start,item.Position-item.Leading);end=Mathf.Max(end,item.Position+item.Size+item.Trailing);occupied+=item.Leading+item.Size+item.Trailing;}
            // Retain the native occupied interval. Distributed justification is
            // excluded above because its per-gap pixel rounding is order-dependent.
            var spacing=(end-start-occupied)/(_items.Count-1);
            if(!Finite(spacing)||spacing<-.05f)return false;
            spacing=Mathf.Max(0,spacing);var cursor=start;
            foreach(var item in _items){
                var offset=cursor+item.Leading-item.Position;
                item.Frame.SetFlowOffset(horizontal?new Vector2(offset,0):new Vector2(0,offset));
                cursor+=item.Leading+item.Size+item.Trailing+spacing;
            }
            return true;
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
