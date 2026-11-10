using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 public sealed partial class NativeArchiveScreen {
  private readonly Stack<Card> _detailParents=new Stack<Card>();
  private readonly List<NativeHudSurface> _detailPaint=new List<NativeHudSurface>();
  private Card _detailCard;
  private void RemoveDetail(){
   _detailArtwork.Dispose();
   foreach(var paint in _detailPaint)paint.Dispose();_detailPaint.Clear();
   if(_detail==null)return;
   for(int i=_portraits.Count-1;i>=0;i--)if(_detail.Contains(_portraits[i].Image)){_portraits[i].Image.image=null;_portraits[i].Image.sprite=null;_portraits.RemoveAt(i);}
   _detail.RemoveFromHierarchy();_detail=null;
  }
  private void CloseDetail(){RemoveDetail();_detailParents.Clear();_detailCard=null;}
  private void BackDetail(){if(_detailParents.Count==0)CloseDetail();else ShowDetail(_detailParents.Pop(),true);}
  private void OpenNested(Card card){if(!card.Open)return;if(_detailCard!=null)_detailParents.Push(_detailCard);ShowDetail(card,true);}
  private void OpenDetail(Card card)=>ShowDetail(card,false);
  private void ShowDetail(Card card,bool nested){
   if(!card.Open)return;if(!nested)_detailParents.Clear();RemoveDetail();_detailCard=card;
   _detail=new VisualElement{name="archive-entry-detail"};Fill(_detail);_detail.style.backgroundColor=C("#02070ff5");_detail.style.alignItems=Align.Center;_detail.style.justifyContent=Justify.Center;Pad(_detail,12);_root.Add(_detail);
   var panel=new ScrollView(ScrollViewMode.Vertical){name="archive-detail-scroll"};NativePauseScreenView.ConfigureMobileScroll(panel);panel.style.width=Length.Percent(100);panel.style.maxWidth=600;panel.style.maxHeight=Length.Percent(100);panel.style.flexShrink=1;panel.style.minHeight=panel.style.minWidth=0;panel.style.backgroundColor=C("#0a1421");_detail.Add(panel);var body=panel.contentContainer;Pad(body,16);
   var back=new Button(BackDetail){name="archive-detail-close",text=_detailParents.Count>0?L("← Назад","← Back"):L("← К записям","← Back to entries")};back.style.minHeight=36;back.style.backgroundColor=C("#102938");back.style.color=C("#a5edf4");back.style.whiteSpace=WhiteSpace.Normal;body.Add(back);
   var art=Box(body,"archive-detail-art");art.style.width=Length.Percent(100);art.style.height=240;art.style.marginTop=12;art.style.flexShrink=0;art.style.overflow=Overflow.Hidden;art.style.alignItems=Align.Center;art.style.justifyContent=Justify.Center;
   art.RegisterCallback<GeometryChangedEvent>(e=>{if(e.newRect.width>0)art.style.height=e.newRect.width*2f/3f;});
   var fallback=new NativeContentIcon(card.Id,C("#a5f3fc"),112);fallback.style.position=Position.Absolute;art.Add(fallback);
   var illustration=new Image{name="archive-detail-illustration",scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};illustration.style.width=illustration.style.height=Length.Percent(100);art.Add(illustration);_detailArtwork.Show(card.Id,illustration);
   Text(body,"archive-detail-title",card.Title,23,"#dff5ff","700",true).style.marginTop=10;
   Text(body,"archive-detail-behavior",card.Description,13,"#82d9e4",wrap:true,line:1.6).style.marginTop=10;
   string lore=_state.Russian?card.Details:card.DetailsEnglish??card.Details;
   if(string.IsNullOrWhiteSpace(lore))lore=ArchiveEntryLore.Get(card.Id,_state.Russian);
   if(!string.IsNullOrWhiteSpace(lore))Text(body,"archive-detail-lore",lore,14,"#c5d3e1",wrap:true,line:1.7).style.marginTop=18;
   if(card.Progression!=null)ProgressionDetails(body,card);
   if(card.Builds!=null)ArtifactBuildDetails(body,card);
   string region=_state.Russian?card.RegionLore:card.RegionLoreEnglish;
   if(!string.IsNullOrEmpty(region)){Text(body,"archive-detail-region-title",L("ИЗ ИСТОРИИ РЕГИОНА","REGIONAL HISTORY"),11,"#9389c4","700",true).style.marginTop=24;Text(body,"archive-detail-region",region,13,"#98abc1",wrap:true,line:1.7).style.marginTop=8;}
  }
  private void ProgressionChoiceView(VisualElement parent,ProgressionChoice choice,int level,ProgressionDetail children=null){
   var box=Box(parent,level==4?"archive-progression-branch":"archive-progression-final");Pad(box,12);Surface(box,true);box.style.marginTop=12;
   box.Add(new NativeContentIcon(choice.Open?choice.IconId:"locked",C("#a5f3fc"),40));
   Text(box,"archive-progression-stage",(level==4?"IV":"VII")+" · "+(choice.Open?L("ОТКРЫТО","DISCOVERED"):L("НЕ ИЗУЧЕНО","UNDISCOVERED")),10,choice.Open?"#9389c4":"#64748b",wrap:true).style.marginTop=5;
   Text(box,"archive-progression-name",choice.Open?choice.Title:L("Неизученная мутация","Undiscovered mutation"),16,choice.Open?"#dff5ff":"#7e91a7","700",true).style.marginTop=5;
   if(!choice.Open){Text(box,"archive-progression-locked",L("Выберите этот вариант в забеге, чтобы открыть запись.","Choose this option in a run to unlock its entry."),12,"#64748b",wrap:true).style.marginTop=8;return;}
   box.pickingMode=PickingMode.Position;
   box.RegisterCallback<ClickEvent>(evt=>{evt.StopPropagation();OpenNested(new Card{Id=choice.IconId,Title=choice.Title,Description=choice.Description,Open=true,Progression=children});});
   Text(box,"archive-progression-open",L("Открыть запись →","Read entry →"),12,"#67dbe8",wrap:true).style.marginTop=8;
  }
  private void ProgressionDetails(VisualElement parent,Card card){
   if(card.Progression.Branches.Count>0||card.Progression.Finals.Count>0){
    Text(parent,"archive-progression-title",L("РАЗВИТИЕ","PROGRESSION"),12,"#f5cf87","700",true).style.marginTop=22;
    Text(parent,"archive-progression-hint",L("Каждый выбранный в забеге вариант открывает свою запись.","Every option chosen in a run unlocks its own entry."),12,"#98abc1",wrap:true).style.marginTop=8;
   }
   foreach(var branch in card.Progression.Branches){var children=new ProgressionDetail();children.Finals.AddRange(branch.Finals);ProgressionChoiceView(parent,branch,4,children);}
   foreach(var final in card.Progression.Finals)ProgressionChoiceView(parent,final,7);
   if(card.Progression.Links.Count==0)return;
   Text(parent,"archive-progression-synergies",L("СИНЕРГИИ","SYNERGIES"),12,"#f5cf87","700",true).style.marginTop=24;
   foreach(var link in card.Progression.Links){
    var box=Box(parent,"archive-progression-link");Pad(box,12);Surface(box,true);box.style.marginTop=12;
    box.Add(new NativeContentIcon(link.Open?link.IconId:"locked",C("#a5f3fc"),40));
    Text(box,"archive-progression-link-name",link.Open?link.Title:L("Синергия не изучена","Undiscovered synergy"),16,link.Open?"#dff5ff":"#7e91a7","700",true).style.marginTop=8;
    Text(box,"archive-progression-link-requirements",link.Requirements,12,"#98abc1",wrap:true,line:1.6).style.marginTop=10;
    if(link.Open){box.pickingMode=PickingMode.Position;box.RegisterCallback<ClickEvent>(evt=>{evt.StopPropagation();OpenNested(new Card{Id=link.IconId,Title=link.Title,Description=link.Description,Open=true,Details=ArchiveEntryLore.Get(link.IconId,true),DetailsEnglish=ArchiveEntryLore.Get(link.IconId,false)});});Text(box,"archive-synergy-open",L("Открыть запись →","Read entry →"),12,"#67dbe8",wrap:true).style.marginTop=8;}
   }
  }
  private void ArtifactBuildDetails(VisualElement parent,Card card){
   Text(parent,"archive-artifact-builds-title",L("СБОРКИ И СОЧЕТАНИЯ","BUILDS AND COMBINATIONS"),12,"#f5cf87","700",true).style.marginTop=22;
   if(card.Builds.Count==0){Text(parent,"archive-artifact-no-build",L("Не входит в особые сборки. Его эффект действует самостоятельно.","Not part of a special build. Its effect works independently."),13,"#98abc1",wrap:true).style.marginTop=12;return;}
   foreach(var build in card.Builds){
    var box=Box(parent,"archive-artifact-build");Pad(box,12);Surface(box,true);box.style.marginTop=12;
    box.Add(new NativeContentIcon(build.Id,C("#a5f3fc"),40));
    Text(box,"archive-build-kind",build.Combination?L("СОЧЕТАНИЕ","COMBINATION"):L("СБОРКА","BUILD"),10,"#9389c4",wrap:true);
    Text(box,"archive-build-name",build.Title,17,"#dff5ff","700",true).style.marginTop=4;
    Text(box,"archive-build-effect",build.Description,13,"#82d9e4",wrap:true,line:1.6).style.marginTop=8;
    Text(box,"archive-build-requirements",L("Нужны все артефакты:","Requires all artifacts:"),12,"#98abc1",wrap:true).style.marginTop=12;
    foreach(var member in build.Members){var row=Box(box,"archive-build-member",true);row.style.alignItems=Align.Center;row.style.marginTop=8;var icon=new NativeContentIcon(member.Id,C("#a5f3fc"),32);icon.style.marginRight=10;row.Add(icon);var text=Text(row,"archive-build-member-name",member.Name+(member.Selected?L(" · этот артефакт"," · this artifact"):""),12,member.Selected?"#f5cf87":"#c5d3e1",wrap:true);text.style.flexShrink=1;text.style.minWidth=0;}
    if(build.Open){box.pickingMode=PickingMode.Position;box.RegisterCallback<ClickEvent>(evt=>{evt.StopPropagation();OpenNested(new Card{Id=build.Id,Title=build.Title,Description=build.Description,Open=true});});Text(box,"archive-build-open",L("История сборки →","Build record →"),12,"#67dbe8",wrap:true).style.marginTop=12;}
   }
  }
 }
}