using System;
using System.Collections.Generic;
using System.Text;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 internal sealed class NativeMerchantView {
  private readonly VisualElement _root,_overlay,_panel,_grid,_details;
  private readonly Label _status,_title,_balance,_name,_description;
  private readonly Button _close,_purchase;
  private readonly ScrollView _stock;
  private readonly Action<int> _buy;private string _signature;private int _selected;private RunEconomyState _state;private IList<string> _owned;private bool _ru;
  internal NativeMerchantView(VisualElement parent,Action open,Action close,Action<int> buy){
   _buy=buy;_root=new VisualElement{name="native-merchant",pickingMode=PickingMode.Ignore};Fill(_root);parent.Add(_root);
   _status=new Label{name="merchant-status",pickingMode=PickingMode.Ignore};_status.style.position=Position.Absolute;_status.style.top=88;_status.style.left=Length.Percent(50);_status.style.marginLeft=-100;_status.style.width=200;_status.style.fontSize=11;_status.style.unityTextAlign=TextAnchor.MiddleCenter;_status.style.color=new Color(.65f,.86f,.9f);_root.Add(_status);
   _overlay=new VisualElement{name="merchant-overlay"};Fill(_overlay);_overlay.style.backgroundColor=new Color(.005f,.015f,.025f,.95f);_overlay.style.alignItems=Align.Center;_overlay.style.justifyContent=Justify.Center;_overlay.style.paddingLeft=_overlay.style.paddingRight=12;_overlay.style.paddingTop=_overlay.style.paddingBottom=24;_overlay.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());_overlay.RegisterCallback<PointerUpEvent>(e=>e.StopPropagation());_root.Add(_overlay);
   _panel=new VisualElement{name="merchant-panel"};_panel.style.width=Length.Percent(100);_panel.style.maxWidth=560;_panel.style.maxHeight=Length.Percent(100);_panel.style.minHeight=0;_panel.style.flexShrink=1;_panel.style.paddingLeft=_panel.style.paddingRight=12;_panel.style.paddingTop=_panel.style.paddingBottom=12;_panel.style.backgroundColor=new Color(.025f,.045f,.07f);_overlay.Add(_panel);
   var header=new VisualElement();header.style.flexDirection=FlexDirection.Row;header.style.alignItems=Align.Center;_panel.Add(header);
   _title=Text(header,"",18);_title.style.flexGrow=1;_balance=Text(header,"",13);_balance.style.color=new Color(.85f,.75f,.45f);
   _stock=new ScrollView(ScrollViewMode.Vertical){name="merchant-stock"};NativePauseScreenView.ConfigureMobileScroll(_stock);_stock.style.maxHeight=Length.Percent(48);_stock.style.minHeight=80;_stock.style.flexShrink=1;_stock.style.marginTop=12;_panel.Add(_stock);
   _grid=new VisualElement{name="merchant-grid"};_grid.style.flexDirection=FlexDirection.Row;_grid.style.flexWrap=Wrap.Wrap;_stock.Add(_grid);
   var info=new ScrollView(ScrollViewMode.Vertical){name="merchant-item-details"};NativePauseScreenView.ConfigureMobileScroll(info);info.style.maxHeight=Length.Percent(28);info.style.flexShrink=1;info.style.minHeight=64;_panel.Add(info);_details=info.contentContainer;_details.style.paddingTop=8;
   _name=Text(_details,"",16);_description=Text(_details,"",13);_description.style.color=new Color(.64f,.72f,.8f);_description.style.marginTop=6;
   _purchase=new Button(()=>{if(_state!=null)_buy(_selected);}){name="merchant-buy"};StyleButton(_purchase);_purchase.style.marginTop=12;_panel.Add(_purchase);
   _close=new Button(close){name="merchant-close"};StyleButton(_close);_close.style.marginTop=8;_panel.Add(_close);
  }
  private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
  private static Label Text(VisualElement p,string value,int size){var l=new Label(value);l.style.fontSize=size;l.style.whiteSpace=WhiteSpace.Normal;l.style.minWidth=0;l.style.flexShrink=1;l.style.color=new Color(.85f,.93f,.98f);p.Add(l);return l;}
  private static void StyleButton(Button b){b.style.minHeight=42;b.style.whiteSpace=WhiteSpace.Normal;b.style.fontSize=13;b.style.backgroundColor=new Color(.04f,.10f,.14f);b.style.color=new Color(.74f,.94f,1);b.style.borderTopWidth=b.style.borderBottomWidth=b.style.borderLeftWidth=b.style.borderRightWidth=1;b.style.borderTopColor=b.style.borderBottomColor=b.style.borderLeftColor=b.style.borderRightColor=new Color(.18f,.32f,.4f);}
  internal void Bind(RunEconomyState state,IList<string> owned,bool ru,double time,bool visible,bool canOpen,bool leftHanded){
   _root.style.display=visible&&state!=null?DisplayStyle.Flex:DisplayStyle.None;if(!visible||state==null)return;_state=state;_owned=owned;_ru=ru;NativeUiOrder.Last(_root);
   _status.style.display=state.Open?DisplayStyle.None:DisplayStyle.Flex;
   if(state.Pending)_status.text=ru?"Торговец рядом · подойдите":"Merchant nearby · approach";else{int left=(int)Math.Max(0,Math.Ceiling(RunEconomyRules.MerchantAt(state.NextMerchant)-time));_status.text=(ru?"Торговец ":"Merchant ")+left/60+":"+(left%60).ToString("00");}
   _overlay.style.display=state.Open?DisplayStyle.Flex:DisplayStyle.None;if(!state.Open)return;
   _title.text=ru?"Торговец Эха":"Echo Merchant";_balance.text=state.Echo+(ru?" Эхо":" Echo");_close.text=ru?"Продолжить путь":"Continue";
   var key=new StringBuilder().Append(ru).Append('|').Append(state.NextMerchant).Append('|').Append(state.Echo);foreach(var row in state.Offers)key.Append('|').Append(row.ArtifactId).Append(row.Sold).Append(owned.Contains(row.ArtifactId));string signature=key.ToString();
   if(_signature!=signature){_signature=signature;_grid.Clear();_selected=Mathf.Clamp(_selected,0,Math.Max(0,state.Offers.Count-1));
    for(int i=0;i<state.Offers.Count;i++){int slot=i;var row=state.Offers[i];var item=ArtifactRewardRules.Get(row.ArtifactId);if(item==null)continue;var cell=new VisualElement();cell.style.width=Length.Percent(25);cell.style.minWidth=0;cell.style.paddingLeft=cell.style.paddingRight=3;cell.style.paddingBottom=6;_grid.Add(cell);
     var button=new Button(()=>{_selected=slot;BindSelection();}){name="merchant-slot-"+i};StyleButton(button);button.style.height=88;button.style.paddingLeft=button.style.paddingRight=0;button.style.alignItems=Align.Center;button.style.justifyContent=Justify.Center;button.style.opacity=row.Sold||owned.Contains(row.ArtifactId)?.4f:1;cell.Add(button);Color color=item.Rarity==ArtifactRarity.Epic?new Color(.8f,.55f,1):item.Rarity==ArtifactRarity.Rare?new Color(.4f,.8f,1):new Color(.65f,.85f,.7f);var icon=new NativeContentIcon("artifact:"+row.ArtifactId,color,34);icon.style.flexShrink=0;button.Add(icon);var price=Text(button,row.Sold||owned.Contains(row.ArtifactId)?(ru?"Есть":"Owned"):row.Price.ToString(),11);price.name="merchant-price";price.style.minWidth=32;price.style.fontSize=10;price.style.width=Length.Percent(100);price.style.paddingLeft=price.style.paddingRight=0;price.style.textOverflow=TextOverflow.Clip;price.style.alignSelf=Align.Stretch;price.style.unityTextAlign=TextAnchor.MiddleCenter;price.style.flexShrink=0;price.style.whiteSpace=WhiteSpace.NoWrap;price.style.marginLeft=price.style.marginRight=0;
    }
   }BindSelection();
  }
  private void BindSelection(){if(_state.Offers.Count==0){_name.text=_ru?"Всё уже найдено":"Everything collected";_description.text="";_purchase.SetEnabled(false);_purchase.text="—";return;}
   var row=_state.Offers[_selected];var item=ArtifactRewardRules.Get(row.ArtifactId);bool sold=row.Sold||_owned.Contains(row.ArtifactId);_name.text=NativePlayerCopy.Text(_ru?item.NameRussian:item.NameEnglish,_ru);_description.text=NativePlayerCopy.Text(ArtifactGameplayRules.DescriptionOverride(item.Id,_ru)??(_ru?item.DescriptionRussian:item.DescriptionEnglish),_ru);_purchase.text=sold?(_ru?"Получено":"Owned"):(_ru?"Купить · ":"Buy · ")+row.Price+(_ru?" Эхо":" Echo")+(row.Discount?"  −25%":"");_purchase.SetEnabled(RunEconomyRules.CanBuy(_state,_selected,_owned));
   for(int i=0;i<_grid.childCount;i++){var b=_grid[i].Q<Button>();if(b!=null)b.style.borderBottomColor=b.style.borderTopColor=b.style.borderLeftColor=b.style.borderRightColor=i==_selected?new Color(.65f,.9f,1):new Color(.18f,.32f,.4f);}
  }
 }
}
