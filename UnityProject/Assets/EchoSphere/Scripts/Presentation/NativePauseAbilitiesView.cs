using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    public sealed class NativePauseAbilitiesView : IDisposable
    {
        private sealed class Card
        {
            internal VisualElement Root,Progression;
            internal Label Name,Level,Description,Branch,Final;
            internal NativeHudSurface Surface;
        }
        private readonly VisualElement _root,_active,_passive;
        private readonly Label _activeTitle,_passiveTitle;
        private readonly List<Card> _activeCards=new List<Card>(),_passiveCards=new List<Card>();
        private readonly Dictionary<Label,(string Text,float Width)> _lineBoxes=new Dictionary<Label,(string,float)>();
        private readonly NativeMenuBrush.Paint _locked=new NativeMenuBrush.Paint {Radius=3,Border=1,Fill=new Color(6/255f,17/255f,29/255f,.78f),BorderColor=new Color(76/255f,165/255f,214/255f,.16f)};
        private readonly NativeMenuBrush.Paint _acquired=new NativeMenuBrush.Paint {Radius=3,Border=1,Fill=new Color(6/255f,17/255f,29/255f,.78f),BorderColor=new Color(57/255f,216/255f,1,.3f)};
        private readonly NativeMenuBrush.Paint _passivePaint=new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=new Color(6/255f,17/255f,29/255f,.78f),BorderColor=new Color(76/255f,165/255f,214/255f,.16f)};
        private bool _bound,_disposed,_flow;
        private int _geometryFrame;
        public VisualElement Root=>_root;
        public bool Ready {get;private set;}
        public NativePauseAbilitiesView(VisualElement parent)
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));
            _root=Box(parent,"native-pause-abilities");_root.style.position=Position.Absolute;_root.style.left=_root.style.top=0;_root.style.width=Length.Percent(100);_root.style.opacity=0;
            _activeTitle=Text(_root,"pause-active-title",10,"#64849b","700",spacing:.5f);_activeTitle.style.marginBottom=8;
            _active=Box(_root,"pause-active-cards");_active.style.marginTop=4; // collapsed source heading/card gap is 12px
            _passive=Box(_root,"pause-passive-section");_passive.style.marginTop=12;_passive.style.paddingTop=8;
            _passiveTitle=Text(_passive,"pause-passive-title",10,"#64849b","700",spacing:.5f);_passiveTitle.style.marginBottom=8;
            _root.RegisterCallback<GeometryChangedEvent>(GeometryChanged);_geometryFrame=Time.frameCount;
        }
        private void GeometryChanged(GeometryChangedEvent evt){_geometryFrame=Time.frameCount;}
        private Card AddCard(bool passive,int index)
        {
            var prefix=(passive?"pause-passive-":"pause-active-")+index;
            var root=Box(passive?_passive:_active,prefix);if(index>0)root.style.marginTop=passive?8:12;
            root.style.paddingLeft=root.style.paddingRight=root.style.paddingTop=root.style.paddingBottom=12;
            root.style.borderLeftWidth=root.style.borderRightWidth=root.style.borderTopWidth=root.style.borderBottomWidth=1;
            root.style.borderLeftColor=root.style.borderRightColor=root.style.borderTopColor=root.style.borderBottomColor=Color.clear;
            var card=new Card {Root=root,Surface=new NativeHudSurface(root,passive?_passivePaint:_locked)};
            var heading=Box(root,prefix+"-heading",true);heading.style.justifyContent=Justify.SpaceBetween;heading.style.alignItems=passive?Align.Stretch:Align.Center;
            card.Name=Text(heading,prefix+"-name",passive?12:14,"#dcecff","700",passive?16d/12:20d/14,wrap:true);card.Name.style.flexShrink=1;card.Name.style.marginRight=8;
            card.Level=Text(heading,prefix+"-level",passive?12:10,passive?"#dcecff":"#39d8ff","700",passive?16d/12:1.5);
            card.Description=Text(root,prefix+"-description",10,"#64849b",wrap:true);card.Description.style.marginTop=4;
            if(!passive){card.Progression=Box(root,prefix+"-progression");card.Progression.style.marginTop=8;card.Branch=Text(card.Progression,prefix+"-branch",10,"#dcecff",wrap:true);card.Final=Text(card.Progression,prefix+"-final",10,"#dcecff",wrap:true);card.Final.style.marginTop=6;card.Branch.enableRichText=card.Final.enableRichText=true;}
            (passive?_passiveCards:_activeCards).Add(card);return card;
        }
        private static string Evolution(string label,string text,string color)=>"<color="+color+"><b>"+label+"</b></color> <noparse>"+Regex.Replace(text??"",@"\s+"," ")+"</noparse>";
        public void Bind(NativePauseAbilitiesState state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseAbilitiesView));if(state==null)throw new ArgumentNullException(nameof(state));_bound=true;
            Set(_activeTitle,state.ActiveHeading.ToUpperInvariant());Set(_passiveTitle,state.PassiveHeading.ToUpperInvariant());_passive.style.display=state.Passive.Count==0?DisplayStyle.None:DisplayStyle.Flex;
            void Cards(bool passive,IReadOnlyList<NativePauseAbilitiesState.Ability> models,List<Card> cards){
                while(cards.Count<models.Count)AddCard(passive,cards.Count);
                for(int i=0;i<cards.Count;i++){var card=cards[i];card.Root.style.display=i<models.Count?DisplayStyle.Flex:DisplayStyle.None;if(i>=models.Count)continue;var model=models[i];Set(card.Name,model.Name);Set(card.Level,model.Level);Set(card.Description,model.Description);
                    if(!passive){card.Root.style.opacity=model.Acquired?1:.75f;card.Surface.SetPaint(model.Acquired?_acquired:_locked);card.Progression.style.display=model.HasProgression?DisplayStyle.Flex:DisplayStyle.None;var branch=Evolution("IV:",model.Branch,"#8064a8");var final=Evolution("VII:",model.Final,"#ff6b6b");if(card.Branch.text!=branch)card.Branch.text=branch;if(card.Final.text!=final)card.Final.text=final;}
                }
            }
            Cards(false,state.Active,_activeCards);Cards(true,state.Passive,_passiveCards);
        }
        public void UseFlowLayout(bool flow){if(_flow==flow)return;_flow=flow;_root.style.position=flow?Position.Relative:Position.Absolute;if(flow)_root.style.left=_root.style.top=StyleKeyword.Auto;else _root.style.left=_root.style.top=0;}
        public void Prepare(float scale)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseAbilitiesView));var bounds=_root.layout;if(!_bound||!float.IsFinite(bounds.width+bounds.height)||bounds.width<=0||bounds.height<=0)return;
            void PrepareCard(Card card,bool passive){
                FitLineBox(card.Name,passive?16:20);FitLineBox(card.Description,15);if(!passive){FitLineBox(card.Branch,15);FitLineBox(card.Final,15);}card.Surface.Prepare(scale);
            }
            foreach(var card in _activeCards)PrepareCard(card,false);foreach(var card in _passiveCards)PrepareCard(card,true);
            if(!Ready&&Time.frameCount-_geometryFrame>=2){Ready=true;_root.style.opacity=1;}
        }
        private void FitLineBox(Label label,float lineHeight)
        {
            float width=label.contentRect.width;if(width<=0||!float.IsFinite(width))return;
            if(_lineBoxes.TryGetValue(label,out var previous)&&previous.Text==label.text&&Mathf.Abs(previous.Width-width)<.01f)return;
            // TextCore supplies wrapping, while the authored CSS supplies line
            // boxes. Its intrinsic single-line extent includes extra leading.
            var one=label.MeasureTextSize("M",0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);
            var all=label.MeasureTextSize(label.text,width,VisualElement.MeasureMode.Exactly,0,VisualElement.MeasureMode.Undefined);
            if(one.y<=0||!float.IsFinite(one.y+all.y))return;
            label.style.height=Mathf.Max(1,Mathf.RoundToInt(all.y/one.y))*lineHeight;
            _geometryFrame=Time.frameCount;
            _lineBoxes[label]=(label.text,width);
        }
        public void Dispose(){if(_disposed)return;_disposed=true;_root.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);foreach(var card in _activeCards)card.Surface.Dispose();foreach(var card in _passiveCards)card.Surface.Dispose();_root.RemoveFromHierarchy();}
    }
}
