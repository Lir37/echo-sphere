using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    // Diagnostic composition only until glyph/detail/input parity is accepted.
    public sealed class NativePauseArtifactsView : IDisposable
    {
        private sealed class Card
        {
            internal VisualElement Root,Sets;
            internal Label Name,Rarity,Description;
            internal NativeArtifactGlyph Glyph;
            internal NativeHudSurface Surface;
            internal string Border;
            internal readonly List<NativeArtifactSetBadge> Badges=new List<NativeArtifactSetBadge>();
        }
        private readonly VisualElement _root,_cards,_empty;
        private readonly Label _heading,_emptyText;
        private readonly NativeHudSurface _emptySurface;
        private readonly List<Card> _items=new List<Card>();
        private readonly NativeUiLineBoxes _lineBoxes=new NativeUiLineBoxes();
        private readonly Action<string> _selectSet;
        private bool _bound,_disposed,_flow;
        private int _geometryFrame;
        public VisualElement Root=>_root;
        public bool Ready {get;private set;}
        private static Color C(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
        private static NativeMenuBrush.Paint Paint(string border,float width)=>new NativeMenuBrush.Paint {Radius=3,Border=width,BorderColor=C(border),Fill=new Color(6/255f,17/255f,29/255f,.78f)};
        private static void Frame(VisualElement root,float padding,float border)
        {
            root.style.paddingLeft=root.style.paddingRight=root.style.paddingTop=root.style.paddingBottom=padding;
            root.style.borderLeftWidth=root.style.borderRightWidth=root.style.borderTopWidth=root.style.borderBottomWidth=border;
            root.style.borderLeftColor=root.style.borderRightColor=root.style.borderTopColor=root.style.borderBottomColor=Color.clear;
        }
        public NativePauseArtifactsView(VisualElement parent,Action<string> selectSet)
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));_selectSet=selectSet;
            _root=Box(parent,"native-pause-artifacts");_root.style.position=Position.Absolute;_root.style.left=_root.style.top=0;_root.style.width=Length.Percent(100);_root.style.opacity=0;
            _heading=Text(_root,"pause-artifacts-heading",10,"#64849b","700",spacing:.5f);
            _cards=Box(_root,"pause-artifact-cards");_cards.style.marginTop=12;
            _empty=Box(_root,"pause-artifacts-empty");_empty.style.marginTop=12;Frame(_empty,20,1);var emptyPaint=Paint("#000000",1);emptyPaint.BorderColor=new Color(76/255f,165/255f,214/255f,.16f);_emptySurface=new NativeHudSurface(_empty,emptyPaint);
            _emptyText=Text(_empty,"pause-artifacts-empty-text",14,"#64849b",line:20d/14,wrap:true);_emptyText.style.unityTextAlign=TextAnchor.UpperCenter;
            _root.RegisterCallback<GeometryChangedEvent>(GeometryChanged);_geometryFrame=Time.frameCount;
        }
        private void GeometryChanged(GeometryChangedEvent evt){_geometryFrame=Time.frameCount;}
        private Card AddCard(int index)
        {
            var prefix="pause-artifact-"+index;var card=new Card {Root=Box(_cards,prefix)};Frame(card.Root,12,2);if(index>0)card.Root.style.marginTop=12;card.Surface=new NativeHudSurface(card.Root,Paint("#243b55",2));
            var row=Box(card.Root,prefix+"-row",true);row.style.alignItems=Align.Center;
            var glyphSlot=Box(row,prefix+"-glyph");glyphSlot.style.marginRight=12;card.Glyph=new NativeArtifactGlyph(glyphSlot,54,16);
            var body=Box(row,prefix+"-body");body.style.flexGrow=1;body.style.flexShrink=1;
            var heading=Box(body,prefix+"-heading",true);heading.style.alignItems=Align.Center;heading.style.justifyContent=Justify.SpaceBetween;
            card.Name=Text(heading,prefix+"-name",14,"#dcecff","700",20d/14);card.Name.style.flexShrink=1;card.Name.style.marginRight=8;card.Name.style.overflow=Overflow.Hidden;card.Name.style.textOverflow=TextOverflow.Ellipsis;
            card.Rarity=Text(heading,prefix+"-rarity",9,"#64849b",spacing:.45f);
            card.Description=Text(body,prefix+"-description",10,"#cfe7f3",wrap:true);card.Description.style.marginTop=4;
            card.Sets=Box(card.Root,prefix+"-sets",true);card.Sets.style.flexWrap=Wrap.Wrap;
            // Unity 6.3 exposes no flex gap. Compensated outside margins retain
            // the source8px wrap gap and12px first-row offset without a layout engine.
            card.Sets.style.marginRight=-8;card.Sets.style.marginTop=4;
            _items.Add(card);return card;
        }
        public void Bind(NativePauseArtifactsState state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseArtifactsView));if(state==null)throw new ArgumentNullException(nameof(state));_bound=true;
            Set(_heading,state.Heading.ToUpperInvariant());Set(_emptyText,state.Empty);_empty.style.display=state.Artifacts.Count==0?DisplayStyle.Flex:DisplayStyle.None;_cards.style.display=state.Artifacts.Count==0?DisplayStyle.None:DisplayStyle.Flex;
            while(_items.Count<state.Artifacts.Count)AddCard(_items.Count);
            for(int i=0;i<_items.Count;i++){
                var card=_items[i];card.Root.style.display=i<state.Artifacts.Count?DisplayStyle.Flex:DisplayStyle.None;if(i>=state.Artifacts.Count)continue;var model=state.Artifacts[i];
                Set(card.Name,model.Name);Set(card.Rarity,model.RarityLabel.ToUpperInvariant());Set(card.Description,model.Description);card.Glyph.Bind(model.Glyph,model.GlyphColor);
                if(card.Border!=model.BorderColor){card.Border=model.BorderColor;card.Surface.SetPaint(Paint(model.BorderColor,2));}
                card.Sets.style.display=model.Sets.Count==0?DisplayStyle.None:DisplayStyle.Flex;
                while(card.Badges.Count<model.Sets.Count){var badge=new NativeArtifactSetBadge(card.Sets,_selectSet);badge.Root.style.marginRight=8;badge.Root.style.marginTop=8;card.Badges.Add(badge);}
                for(int j=0;j<card.Badges.Count;j++){card.Badges[j].Root.style.display=j<model.Sets.Count?DisplayStyle.Flex:DisplayStyle.None;if(j<model.Sets.Count)card.Badges[j].Bind(model.Sets[j]);}
            }
        }
        public void UseFlowLayout(bool flow){if(_flow==flow)return;_flow=flow;_root.style.position=flow?Position.Relative:Position.Absolute;if(flow)_root.style.left=_root.style.top=StyleKeyword.Auto;else _root.style.left=_root.style.top=0;}
        public void Prepare(float scale)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseArtifactsView));var bounds=_root.layout;if(!_bound||!float.IsFinite(bounds.width+bounds.height)||bounds.width<=0||bounds.height<=0)return;
            if(_empty.resolvedStyle.display!=DisplayStyle.None){if(_lineBoxes.Fit(_emptyText,20))_geometryFrame=Time.frameCount;_emptySurface.Prepare(scale);}
            foreach(var card in _items)if(card.Root.resolvedStyle.display!=DisplayStyle.None){if(_lineBoxes.Fit(card.Description,15))_geometryFrame=Time.frameCount;card.Glyph.Prepare(scale);card.Surface.Prepare(scale);}
            if(!Ready&&Time.frameCount-_geometryFrame>=2){Ready=true;_root.style.opacity=1;}
        }
        public void Dispose(){if(_disposed)return;_disposed=true;_root.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);_emptySurface.Dispose();foreach(var card in _items){foreach(var badge in card.Badges)badge.Dispose();card.Glyph.Dispose();card.Surface.Dispose();}_root.RemoveFromHierarchy();}
    }
}
