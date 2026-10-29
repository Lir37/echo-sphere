# ECHO SPHERE — завершение геймплея, 2026-10-29

Baseline:e0d48cbfba3565e47646e75db0139ac981e1ae99,unity-migration.
Пользователь утвердил полное завершение игры по GPT,шесть регионов,RU/EN,
заглушки рекламы;разрешил разработать недостающие правила/баланс в рамках GPT.
Исторические Canvas/TypeScript-инструкции не возвращают production из Unity.

## Согласованная кампания
1.Resonance Basin — текущие органические монстры,освоение сети.
2.Spectral Rift — космические корабли,манёвры и фазовые угрозы.
3.Fractured Bastion — насекомые,панцири,рои и осада.
4.Gravity Folds — кристаллические существа,контроль пространства.
5.Resonance Storm — энергетические сущности,ритм резонанса и давление на связи.
6.Null Archive — гибриды Пустоты,смешанные угрозы и финал.
Это рабочие названия4новыхрегионов. У каждого кардинально другие силуэты,
части,движения,снаряды и боссовые облики;перекраска не считается переносом.
Общая плоская2.5Dэстетика сохраняется. После стабилизаций — Convergence.
Сначала принятие Basin/Rift,затем четыре новых региона.

## Первый внедрённый блок: экономика новых забегов
- Подбор Echo Shard даёт XP и независимое Echo. Echo payload записан в самой
  сущности доcombo/XP-бонусов,оба пути подбора защищены _collected.
- RunEconomyState сохраняется существующим RunSnapshotCodec/version2;старые
  snapshots без поля явно оставляют null и прежние случайные сундуки/дропы.
  Новый seed не расходует gameplay RNG;merchant stock использует отдельный state.
- Обычные сундуки новых забегов:80,190,340,530,690,880,1090,1310,1520,1720сек.
  В Endless каждые30мин повторяется рисунок расписания;прогресс сохранён.
  Старые забеги не переводятся на расписание. Boss Stella прежняя,дублирующий
  обычный boss chest в новой экономике исключён вместе с random death chests.
- Торговец:390,810,1230,1560сек,далее +420сек без cutoff.40сек предупреждения,
  раннее открытие по кнопке или автоматическое после освобождения модальных gates.
  Бой приостановлен;фон/возврат и Escape не снимают pause владельца магазина.
-8разных товаров Common/Rare/Epic/Special,веса редкостей30/35/25/10,
  legendary/owned исключены.Цены100/180/300/500 ×1.35^visit,одна скидка25%.
  Можно купить несколько,безrefresh.Оплата/owned/sold сохраняются совместно.
  Получение использует общий эффект acquisition с обычными наградами.
-UI Toolkit retained окно RU/EN,вертикальный жест безскроллбаров,безhorizontal
  scroll;кнопкакошелька на стороне напротив боевых controls;paused world reused.
  Каталог исчерпан — корректное пустое окно,не бесконечный reroll.
-Цены и доход ещё НЕ проверены полным30минзабегом.Не выдавать стартовые значения
  за законченный баланс.Продажи/тайминги не проверялись на телефоне,APK не собран.

## Проверки первого блока
Core:226новыхbehavioralchecks;всего12688assertionsPASS+47389TS/C#comparisons.
Editor actual runtime:9checksPASS,включаяпокупку/двойнойтап/PlayerPrefs receipt,
pause/focus gate,legacy/null и независимыйEcho payload;оригинальныйprefs восстановлен.
Editor actual UI Toolkit:8checksPASS,RU/EN,portrait1080x2400,перестроение sold,
закрытие;снимок в VisualReferences/RunEconomy-2026-10-29.
Node272/272PASS;preflightPASS;5C#profilesPASS(346runtime/6editor).
CI фиксируется после коммита.Приёмка Android и 30мин экономики OPEN.

## Оставшиеся этапы полного плана
A.Матрица фактических эффектов82артефактов,способностей,сетов/протоколов;
  исторические D-class замечания не считать автоматически действующими.
B.Экономика:измерить сбор,проверить10scheduled+3boss+покупки+до2exploration
  за30мин;целевой диапазон20–23не является hardcap;добавитьbudget exploration.
  Руны пока сохранены;аудит полезности/частоты и Runic Cell специальныхрежимов.
C.15active+15canonicalpassive;пассивные снова в Ability LevelUp;3actives кромеDash,
  открытия5/12/20;дляmajoractives3веткиIV×3финалаVII.7missingactives:
  Nova,Gravity Well,Resonance Pulse,Node Swap,Phase Shell,Void Lance,Time Fracture.
  Legacy IDs остаются читаемыми;10сфер/30IV/90VII не переписывать без дефекта.
D.90–120реальныхартефактов,20сетов,>=12содержательныхпротоколов послеисправления
  существующих.Не заменять поведение дополнительнымипроцентами.
E.Один regionprofile дляsim/render/radar;исправитьобщиеBasinpockets у Rift;
  кораблиRift и разный состав/волны/поведение.Затемостальные4региона,>=15ролей,
  >=10eliteвариантов,8полноценныхbossсемейств(>=3атаки,phase transition).
F.Каждыйрегион:~30мин,4–6pockets,7POIтипов,3контракта,3mastery,5lore,Endless.
  Convergence после6стабилизаций;системнаяэскалация без однихHPмножителей.
G.Четыревалюты:Echo/run,Credits/meta,Shards/unlocks,ArchiveData/collection.
  Староеgold->Credits1:1,покупки/мастерство/открытия не сбрасывать.
H.ПолныеRU/EN/описания/обучение/звук;заглушкиrewardedrevive/double безSDK.

Послеэтапов:compile,Console,contracts,EditorPlay,commit,CIgreen,отчёт.
Androidгейты отдельно:долгийHUD/управление/RUEN/визуал/FPS,p95<=20ms,p99<=33.3ms.
Полныйплан НЕ DONE.Не изменятьnew-desing,QualitySettings/Recovery пользователя.

## Второй блок — корабли Spectral Rift
NativeWorldFleet:7корпусов,rigidкрылья/двигатели,особые рельсы стрелков,
захваты сборщиков/пиявок/осквернителей,выносные опоры Anchor,bridge/podsboss.
Сменаизображениятолько Spectral Rift;Basin остаётся органическим.
Отрисовка использует сохранённые меши:48art entries стабильны на5кадрах.
Сохранены существующие радиусы,поведение,статусы,telegraphs,elite/HP overlay.
Это первая реализация флота,не законченная художественная/Androidприёмка.
16regular/elite states+8bossfamilies отрисованы реальнымUnityGPU;5checksPASS.
Снимок с непрозрачнымфоном в VisualReferences/RegionalFleet-2026-10-29;
прозрачныйPNG при просмотре без alpha давал ложные яркие диски свечения,
поэтомувfixtureфон рисуется самимUnity,productionalphaне менялась.
NativeRegionWorld теперь читает выбранный Region.Pockets,а не общий Basin alias.
NativeRunCatalog остаётся источником координат;геометриязонне придумывалась.
Node272PASS,preflightPASS,5C#profilesPASS(347runtime/6editor);EditorConsole0errors.
Первыйэкономическийкоммит2d99ae72:CI928SUCCESS
https://github.com/Lir37/echo-sphere/actions/runs/40920972625
Остальные4региона/ростеры,различиясимуляцииRift,полныйartifactaudit и остальной
план пока OPEN.Телефон/APK в этихдвухблоках не проверялись/не собирались.

## Третий блок — подтверждённые артефактные расхождения
ArtifactGameplayRules:в новыхзабегах sniper_scope даёт+10%sniperDamage вместо
старого+6%общегоsphereDamage;sniper_crown даёт+10%sniperRadius вместо
+8%общегоурона/-3%cooldown;chain_crown даёт+1вторичныйперескок вместо
этихжеобщихбонусов. ChainTargets и DPS используют одно дискретноезначение.
Весь остальной каталог не переосмыслялся. Старые забеги(null economy) сохраняют
первоначальные эффекты.Общийadapter применяется к бою,acquisition иpause stats.
Core979behavioralchecksPASS,включаявесь82каталогlegacy и всеостальные79безизменений;
всегоCore13667assertions+47389sourcecomparisonsPASS. Editoractual11checksPASS:
familydamage/radius/globalmultiplier/ChainDPS иlegacyisolation,безprefswrite.
Повторноeconomyruntime9checksPASS,исходныйPrefsвосстановлен;Console0errors.
Node272PASS,preflightPASS,5C#profilesPASS(348runtime/6editor).

audit-unity-gameplay.mjs формирует воспроизводимую Unity inventory82артефактов:
имена/обещанныеописания/legacyпараметры/строковыессылкиruntime иsourcehash.
3исправленныхпомеченыCORRECTED_NEW_RUNS_AND_TESTED;79покаREQUIRES_BEHAVIORAL_REVIEW,
не MISSING и не BROKEN. Наличиестроковойссылки не выдано заинтеграцию.
JSON:UNITY_GAMEPLAY_ARTIFACT_INVENTORY_2026-10-29.json.
Второйкоммит1c9621a1:CI929SUCCESS
https://github.com/Lir37/echo-sphere/actions/runs/40922574455

Следующийэтап:сверкаоставшихся79предметов и измерениедоходаEcho/цен на30минBasin;
различияRiftсимуляции,остальные4региона/ростеры,15+15способностей,сеты/протоколы,
мета/Convergence/RUEN/audio/adsзаглушки остаются OPEN.Не считать полныйплан DONE.
