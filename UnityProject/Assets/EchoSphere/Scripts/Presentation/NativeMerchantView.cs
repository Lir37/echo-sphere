using System;
using System.Collections.Generic;
using System.Text;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained battle UI. Geometry is rebuilt only when stock, wallet or language changes.
    internal sealed class NativeMerchantView
    {
        private readonly VisualElement _root,_overlay,_panel;
        private readonly Button _wallet,_close;
        private readonly Label _title,_balance;
        private readonly ScrollView _stock;
        private readonly Action<int> _buy;
        private string _signature;
        internal NativeMerchantView(VisualElement parent,Action open,Action close,Action<int> buy)
        {
            _buy=buy;_root=new VisualElement {name="native-merchant"};_root.style.position=Position.Absolute;
            _root.style.left=0;_root.style.right=0;_root.style.top=0;_root.style.bottom=0;_root.pickingMode=PickingMode.Ignore;parent.Add(_root);
            _wallet=new Button(open);_wallet.style.position=Position.Absolute;_wallet.style.bottom=14;_wallet.style.width=155;_wallet.style.height=44;StyleButton(_wallet);_root.Add(_wallet);
            _overlay=new VisualElement {name="merchant-overlay"};_overlay.style.position=Position.Absolute;
            _overlay.style.left=0;_overlay.style.right=0;_overlay.style.top=0;_overlay.style.bottom=0;
            _overlay.style.backgroundColor=new Color(0,.01f,.03f,.92f);_overlay.style.alignItems=Align.Center;_overlay.style.justifyContent=Justify.Center;
            _overlay.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());_overlay.RegisterCallback<PointerUpEvent>(e=>e.StopPropagation());_root.Add(_overlay);
            _panel=new VisualElement();_panel.style.width=new Length(94,LengthUnit.Percent);_panel.style.maxWidth=720;_panel.style.height=new Length(90,LengthUnit.Percent);
            _panel.style.paddingLeft=12;_panel.style.paddingRight=12;_panel.style.paddingTop=12;_panel.style.paddingBottom=12;
            _panel.style.backgroundColor=new Color(.025f,.06f,.10f,1);_overlay.Add(_panel);
            _title=new Label();_title.style.fontSize=22;_title.style.color=new Color(.76f,.94f,1);_panel.Add(_title);
            _balance=new Label();_balance.style.fontSize=16;_balance.style.color=new Color(.4f,.9f,1);_balance.style.marginTop=6;_balance.style.marginBottom=10;_panel.Add(_balance);
            _stock=new ScrollView(ScrollViewMode.Vertical);_stock.style.flexGrow=1;_stock.horizontalScrollerVisibility=ScrollerVisibility.Hidden;_stock.verticalScrollerVisibility=ScrollerVisibility.Hidden;_panel.Add(_stock);
            _close=new Button(close);_close.style.height=44;_close.style.marginTop=10;StyleButton(_close);_panel.Add(_close);
        }
        private static void StyleButton(Button button)
        {
            button.style.backgroundColor=new Color(.025f,.12f,.18f,1);button.style.color=new Color(.65f,.92f,1);button.style.fontSize=14;
            button.style.whiteSpace=WhiteSpace.Normal;button.style.borderTopColor=button.style.borderBottomColor=button.style.borderLeftColor=button.style.borderRightColor=new Color(.16f,.52f,.65f);
            button.style.borderTopWidth=button.style.borderBottomWidth=button.style.borderLeftWidth=button.style.borderRightWidth=1;
        }
        internal void Bind(RunEconomyState state,IList<string> owned,bool ru,double time,bool visible,bool canOpen,bool leftHanded)
        {
            _root.style.display=visible&&state!=null?DisplayStyle.Flex:DisplayStyle.None;if(!visible||state==null)return;
            _root.BringToFront();if(leftHanded){_wallet.style.left=12;_wallet.style.right=StyleKeyword.Auto;}else{_wallet.style.right=12;_wallet.style.left=StyleKeyword.Auto;}
            _wallet.text="Echo: "+state.Echo+(state.Pending?"\n"+(ru?"Торговец":"Merchant")+" · "+Math.Max(0,(int)Math.Ceiling(state.ArrivedAt+40-time))+"s":"");
            _wallet.SetEnabled(canOpen);_overlay.style.display=state.Open?DisplayStyle.Flex:DisplayStyle.None;if(!state.Open)return;
            _title.text=ru?"ТОРГОВЕЦ ЭХА":"ECHO MERCHANT";_balance.text=(ru?"Доступно: ":"Available: ")+state.Echo+" Echo";_close.text=ru?"Завершить торговлю":"Finish trading";
            var key=new StringBuilder().Append(ru).Append('|').Append(state.NextMerchant).Append('|').Append(state.Echo);
            foreach(var row in state.Offers)key.Append('|').Append(row.ArtifactId).Append(':').Append(row.Price).Append(':').Append(row.Sold).Append(':').Append(owned.Contains(row.ArtifactId));
            var signature=key.ToString();if(_signature==signature)return;_signature=signature;_stock.Clear();
            if(state.Offers.Count==0)_stock.Add(new Label(ru?"Все доступные артефакты уже получены.":"All available artifacts have been collected."));
            for(int i=0;i<state.Offers.Count;i++){
                int slot=i;var row=state.Offers[i];var item=ArtifactRewardRules.Get(row.ArtifactId);if(item==null)continue;
                var card=new VisualElement();card.style.paddingTop=10;card.style.paddingBottom=10;card.style.marginBottom=8;card.style.borderBottomWidth=1;card.style.borderBottomColor=new Color(.1f,.24f,.3f);
                var title=new Label(ru?item.NameRussian:item.NameEnglish);title.style.fontSize=17;title.style.color=new Color(.85f,.94f,1);title.style.whiteSpace=WhiteSpace.Normal;card.Add(title);
                var description=new Label(ArtifactGameplayRules.DescriptionOverride(item.Id,ru)??(ru?item.DescriptionRussian:item.DescriptionEnglish));description.style.fontSize=13;description.style.color=new Color(.55f,.68f,.78f);description.style.whiteSpace=WhiteSpace.Normal;card.Add(description);
                bool sold=row.Sold||owned.Contains(row.ArtifactId);
                var button=new Button(()=>_buy(slot)){text=sold?(ru?"Получено":"Owned"):(ru?"Купить · ":"Buy · ")+row.Price+" Echo"+(row.Discount?" · −25%":"")};
                button.style.height=40;button.style.marginTop=5;StyleButton(button);button.SetEnabled(RunEconomyRules.CanBuy(state,slot,owned));card.Add(button);_stock.Add(card);
            }
        }
    }
}
