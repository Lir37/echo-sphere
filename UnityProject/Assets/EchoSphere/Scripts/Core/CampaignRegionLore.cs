namespace EchoSphere.Core
{
 public sealed class RegionLoreEntry
 {
  public readonly EchoRegionId Region;public readonly int Index;public readonly string Ru,En;
  public string Id=>"region-lore:"+CampaignRegions.SourceId(Region)+":"+Index;
  public RegionLoreEntry(EchoRegionId region,int index,string ru,string en){Region=region;Index=index;Ru=ru;En=en;}
 }
 public static class CampaignRegionLore
 {
  public static readonly RegionLoreEntry[] All={
new RegionLoreEntry((EchoRegionId)0,0,"Здесь сеть впервые ответила ядру. Пять карманов удерживают разные ритмы одного поля.","The Network first answered the Core here. Five pockets sustain different rhythms of one field."),
new RegionLoreEntry((EchoRegionId)0,1,"Стеклянный поток сохранил следы сетей, которые меняли форму быстрее угроз.","Glass Flow preserves traces of networks that changed shape faster than threats."),
new RegionLoreEntry((EchoRegionId)0,2,"Потерянный сигнал повторяет маршрут через пустой контур. Его отправитель не вернулся.","A lost signal repeats a route through the Hollow Contour. Its sender never returned."),
new RegionLoreEntry((EchoRegionId)0,3,"Органические стаи сходятся к резонансу. Лекари и прерыватели делают даже слабую группу опасной.","Organic packs converge on Resonance. Healers and disruptors make weak groups dangerous."),
new RegionLoreEntry((EchoRegionId)0,4,"Страж Стеллы охраняет узел стабилизации, а не границу карты.","The Stella Warden guards stabilization, not the edge of the map."),
new RegionLoreEntry((EchoRegionId)1,0,"Обломки флота продолжают маневрировать без экипажей. Их строй заменил органический рой.","The fleet wrecks still maneuver without crews. Their formation replaced the organic swarm."),
new RegionLoreEntry((EchoRegionId)1,1,"В зеркальной отмели один импульс отражается от нескольких корпусов.","One impulse reflects from several hulls in Mirror Shoal."),
new RegionLoreEntry((EchoRegionId)1,2,"Бортовой журнал оборван во время фазового дрейфа. Координаты остались точными.","The flight log ends during phase drift. Its coordinates remain exact."),
new RegionLoreEntry((EchoRegionId)1,3,"Стрелковые корабли удерживают дистанцию, пока фазовые корпуса отвлекают ядро.","Gunships keep their distance while phase hulls distract the Core."),
new RegionLoreEntry((EchoRegionId)1,4,"Нуль и Титан Ауры прикрывают путь к Стелле разными полями давления.","Null and Aura Titan guard the approach to Stella with different pressure fields."),
new RegionLoreEntry((EchoRegionId)2,0,"Бастион вырос из сомкнутых панцирей. Пять карманов похожи на органы единого улья.","The Bastion grew from interlocked shells. Its five pockets resemble organs of one hive."),
new RegionLoreEntry((EchoRegionId)2,1,"У выводка нет тыла: расколотая особь становится двумя новыми угрозами.","The brood has no rear: a broken body becomes two new threats."),
new RegionLoreEntry((EchoRegionId)2,2,"Следы осады направлены внутрь. Панцирь пытался удержать то, что вылупилось под ним.","The siege tracks lead inward. The shell tried to contain what hatched beneath it."),
new RegionLoreEntry((EchoRegionId)2,3,"Лекари прячутся за тяжёлыми особями; быстрые выводки обходят неподвижную сеть.","Healers shelter behind heavy bodies while fast broods surround a stationary Network."),
new RegionLoreEntry((EchoRegionId)2,4,"Архитектор меняет место боя, пока Роевой Разум наполняет его потомством.","The Architect moves the fight while Brood Mind fills it with offspring."),
new RegionLoreEntry((EchoRegionId)3,0,"Кристаллы удерживают складки пространства. Движение меняет линии огня, но не снимает давление якорей.","Crystals hold folds in space. Movement changes fire lanes but does not remove anchor pressure."),
new RegionLoreEntry((EchoRegionId)3,1,"Линза собирает дальние выстрелы в узкие направления. Между ними остаётся путь.","The Lens gathers distant fire into narrow directions. A path remains between them."),
new RegionLoreEntry((EchoRegionId)3,2,"Потерянная запись измеряет глубину колодца расстоянием между звеньями сети.","A lost record measures the Well by the distance between Network links."),
new RegionLoreEntry((EchoRegionId)3,3,"Якоря замедляют натиск, пока стрелки и лекари поддерживают тяжёлую группу.","Anchors lead a slow advance while shooters and healers support the heavy group."),
new RegionLoreEntry((EchoRegionId)3,4,"Архитектор, Титан и Нуль поочерёдно оспаривают пространство вокруг ядра.","Architect, Titan and Null take turns contesting the space around the Core."),
new RegionLoreEntry((EchoRegionId)4,0,"Шторм состоит из живых разрядов. Его фронты несут прерыватели, стрелков и взрывные группы.","The Storm consists of living discharges. Its fronts carry disruptors, shooters and explosive groups."),
new RegionLoreEntry((EchoRegionId)4,1,"В глазу шторма сигналы различимы по ритму, а не по громкости.","In the Eye, signals are distinguished by rhythm rather than volume."),
new RegionLoreEntry((EchoRegionId)4,2,"Последняя запись советует сохранить геометрию, когда очередное звено умолкнет.","The last record advises preserving Geometry when another link falls silent."),
new RegionLoreEntry((EchoRegionId)4,3,"Фазовые угрозы сближаются под прикрытием дальних разрядов.","Phase threats close in under covering ranged discharges."),
new RegionLoreEntry((EchoRegionId)4,4,"Проводник размыкает сеть; Титан превращает близость в опасную зону.","The Conductor opens the Network; the Titan makes proximity dangerous."),
new RegionLoreEntry((EchoRegionId)5,0,"Архив хранит не вещи, а способы нападения. Здесь смешались фазовые формы и гибриды Пустоты.","The Archive stores ways to attack rather than objects. Phase forms and Void hybrids mingle here."),
new RegionLoreEntry((EchoRegionId)5,1,"Карман Памяти повторяет знакомые силуэты с чужими повадками.","Memory repeats familiar silhouettes with unfamiliar habits."),
new RegionLoreEntry((EchoRegionId)5,2,"Потерянный сигнал не просит помощи. Он перечисляет те связи, которые ещё отвечают.","The lost signal asks for no rescue. It lists the links that still respond."),
new RegionLoreEntry((EchoRegionId)5,3,"Пиявки отнимают ресурс, пока осквернители и фазовые особи давят на сеть.","Leeches drain resources while corruptors and phase bodies pressure the Network."),
new RegionLoreEntry((EchoRegionId)5,4,"За Нулём и Роевым Разумом остаётся Страж: последняя запись перед Схождением.","Beyond Null and Brood Mind stands the Warden: the last record before Convergence.")
  };
 }
}
