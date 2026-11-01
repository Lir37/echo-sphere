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

## Четвёртый блок — условные эффекты на конкретном узле
ArtifactDamageContext и ConditionalDamageMultiplier заменяют скрытыеобщие
прибавки новогозабега наусловия описаний. Legacy ветка остаётся прежней.
Дополнительно исправлены13ID(всего16):formation_compass,zero_sphere,
unified_mind,lone_bastion,fivefold_resonance,relay_matrix,network_relay,
network_coil,geometry_die,mirror_network,singularity_engine,triangle_circuit,
resonance_core. Изусловныхпредметов убраны placeholder sphereDamage;
компас8%одинраз вместо16.64%,Zero35%вместо35%плюсскрытые20%семейства.
Рядом/вTriangle проверяется атакующий SphereAttackAgent,не первый экземпляр типа.
RelayMatrix требует другой VII узел;два VII узла могутусиливатьдругдруга.
Fivefold учитываетпервыйтип;GeometryDie принимаетактивнуюSecondary,ноне
неактивнуюформацию.Уровни/множители взяты изсуществующихописаний/обработчиков.

Core:935artifactbehavioralchecks(492legacy + неизменённые66 + correctedcases),
всего13623assertions и47389sourcecomparisonsPASS. Изменениечислаassertions
обусловленопереносом13IDизпроверкиunchangedвпроверкиcorrected,не числомдефектов.
ActualEditor:21checksPASS на4узлах,втомчисленесколькоStdкопий сразнойдальностью,
Triangleinside/outside,VIIself/other,uniquecomposition,Secondaryinactive,
Compasslegacy;беззаписивprefs. 272Node/preflight/5C#profilesPASS,Console0errors.
Inventory теперь16CORRECTED_NEW_RUNS_AND_TESTED/66REQUIRES_BEHAVIORAL_REVIEW;
не считатьвсе66нерабочими. Общаябалансная иAndroidприёмкапо-прежнемуOPEN.
Третийкоммит6d1aea65:CI930SUCCESS
https://github.com/Lir37/echo-sphere/actions/runs/40924187017

## Пятый блок — ещё7эффектов,достоверныеописания и один источник UIстатистики
Новыезабеги:chain_battery +10%Chain вместообщего6%,shotgun_shell +10%Shotgun,
aura_mist +10%AuraRadius,folded_core +8%общегоурона вместо4%.
Overclock: -12%delay с -5%damage;5% — явноавторскийстартовыйcost в рамках
разрешённого пользователем дизайна,а не число найденное вBlueprint.
quantum_fold/void_star лишены скрытых12%damage/3%crit;ихспециальныеэффекты
сохранены. QuantumFold теперь даёт4варианта ивпозднейnonlegendaryСтелле;
legacy поздняяСтелла оставлена3варианта,ранняя4какраньше.

8description-onlyуточненийRU/EN сохраняют работающийbehavior:VampireRing(+5HP/kill
помимолайфстила),PredatorClaw(каждый5йhit отдельнойсферы double+crit),ForesightEye
(bossdirection),Mirror(20%counter125%,не20%отражённогоурона),VoidMark/VoidLantern
(порог35%HP),GravityHook(+10%pull/-8%interval),PulseDriver(-10%interval).
НоваяточнаякарточкаVoidStarуточняет executionbranch,+8п.п.,cap45%.
Подключено кmerchant/reward/pause/setdetail/archive;legacy/referenceтексты
обычныхкарточек не изменены. СтарыйimportedSOкаталог не переписывался:
export305exactstringsPASS;современныеdeltaописания находятся вCoreadapter.
Эти8описаний НЕ выданы за полныйbehavioral/runtimeaudit соответствующих предметов.

Найден активныйдубликатформулвNativeLivePlannerSphereDamage/Delay/Dps и
NativeLiveArtifactEffect:пауза могла показывать старыеgeneric эффекты.
Новыезабеги читаютDamage изGetSourceSphereAttackDamage,дальностьизAttackRange,
интервализread-only EffectiveAttackInterval(тажеcadence,что Tick),DPS изагента.
Legacy/referenceформулы сохранены.Изменениеgetterне меняет шагсимуляции/RNG.

Editor:22artifactchecksPASS(включаяliveRU/ENdescriptiongate),4actualСтеллаcases
(early/late×legacy/modern),50actualpausecardchecksна10семействахссовместным
наборомартефактов. Console0errors. Core927artifactchecks,всего13615assertions
и47389sourcecomparisonsPASS;272Node/preflight/5C#profilesPASS.
Inventory:23corrected/tested;8descriptionclarified_RUNTIME_REVIEW_PENDING;
51остальных,всего59требуютдальнейшегоbehavioralreview.Не считатьихсломавшимися.
Четвёртыйкоммит1d702b92:CI931SUCCESS
https://github.com/Lir37/echo-sphere/actions/runs/40949084036
Общаяприёмка/баланс/Android/остальныерегионы и полныйплан ещё OPEN.

## Шестой блок — сетевые предметы и радиус орбит
Ещё9исправлений новыхзабегов(всего32):triangle/ring/lattice/fractal_engine
усиливаютчисловыеэффектысоответствующейформациина12%,lattice_chip/fractal_seed
на10%;старыеgeneric8%/4%damage и-3%cooldown удалены изновыхrules.
EchoThread даёт5%урона только атакующему узлу сдействующей связью.
SignalKnot увеличивает скоростьотсчётаnetworkDisabledTimer в1.05раза,включая
ужеидущуюдезактивацию и FracturedNetwork;не меняет authoredStability/базовую
длительностьсобытий. Legacy timer speed1.
MirrorDust даётдобавочные8п.п.шансаконтратаки(самостоятельно8%,сMirror28%),
вместоскрытого4%общегоурона. Используетсуществующий counterattack125% path;
дополнительныхRNGdrawsнет.УточнениеRU/EN подключено кобычнымновымкарточкам.

GetFormationBonus новыхзабегов отсекаетнеактивнуюSecondary иприменяет
артефактныйкоэффициент безизмененияраспознавания/приоритетаформ.
Орбитальнаявизуализация новыхзабегов получаетbase radius изтого же
GetOrbitalLiveRadiusPixels иnetworkMultiplier,что реальныеcontactanchors;
legacy/referenceвизуализация прежняя. Этоисправлениевладенияпроизводнымичислами,
не новый внешнийдизайн.

Core904artifactchecks,всего13592assertions+47389sourcecomparisonsPASS;
272Node/preflight/5C#profilesPASS. ActualEditor14checksPASS:RingEngine,
связанный/изолированныйузел,реальныйtimertick0.5s modern/legacy,
радиусorbitalcontactanchor противrender projection,неактивнаяSecondary,
Secondaryкоэффициент0.56сengine. Console0errors;безprefswrite.
Inventory32corrected,8descriptionclarified-runtimepending,42остальных;
всего50ещётребуютbehavioralreview,не считатьихсломавшимися.
Пятыйкоммитa52f6f65:CI932SUCCESS
https://github.com/Lir37/echo-sphere/actions/runs/40951044446
Android/полныйбаланс/остальнойполныйплан OPEN.


## 2026-11-01 — unchanged artifact consumer acceptance

Preserved 27 working artifacts rather than rewriting them. Actual Unity Play Mode
acceptance passed 95 checks: runtime stat consumers, XP collection, HP acquisition
and seeded critical-hit outcomes. Regen scalar wiring was checked; timed healing
and Android acceptance remain open. Evidence: VisualReferences/RunEconomy-2026-10-29/artifact-data-consumers-acceptance.json.
Inventory now separates 32 corrected artifacts, 27 reviewed unchanged consumers,
and 23 pending behavioral reviews (including 8 clarified descriptions). These
scoped checks do not mean full balance or all special interactions are accepted.
Baseline 48ea3b3291d3cba9672f3561855e0ea63e9ac75c: CI 933 SUCCESS.
Next: review remaining special handlers against their cards before adding content.


## 2026-11-01 — Prism artifact card contracts
Prism Filter does not add 10% pierce: the existing combat handler enables one
network relay at 42% attack damage. Prism Crown adds one relay only when the
filter or mirror branch enables that path; linked Spheres are required.
Modern RU/EN descriptions now explain these existing conditions. Combat logic,
legacy cards and imported catalog remain unchanged. Core PASS: 13600 assertions,
47389 source comparisons; artifact checks 912. Full Editor integration remains
open because no reachable Unity Pipeline is running. These two items are
description-clarified, not fully runtime-accepted. No APK built.


## 2026-11-01 — Pulse crown and mutation DPS consistency
Confirmed that pause DPS counted the Crown's secondary wave as 100% damage,
whereas actual area combat uses 46% (and burst branch adjustments). It also
omitted burst's extra wave. Extracted existing wave count and damage arithmetic
to SphereEvolutionCombatRules; combat calls the same formulas, without changing
wave order, radius, damage, effects or RNG. Modern pause estimates sum those
actual wave multipliers. This remains potential DPS assuming targets inside
the smaller secondary-wave radius, not a measured damage rate. Legacy estimates
retain their prior path. Added 192 cases covering branches, final modes, power
and Crown combinations against the previous combat expression. Core PASS:
13792 assertions, 47389 source comparisons. Updated Editor acceptance fixture
to request the modern estimator. Actual Editor/phone acceptance remains open.
Next: remaining special artifacts and full gameplay roadmap; do not count this
stat display correction as completing artifact behavior review or game balance.


## 2026-11-01 — authored special artifact effects for new runs
Three generic placeholder implementations replaced under the approved authority
to complete missing rules/balance. These are authored starting values, not
recovered original formulas, and require gameplay balance acceptance:
- Echo Weaver: +1% Sphere damage per active undirected Network link, capped +10%.
  Uses existing analyzed active Links.Count, no per-hit collection allocation.
- Axiom Core: +5% Sphere damage/radius, -5% Sphere attack delay/ability cooldown.
  Removed hidden +15% damage/-8% cooldown for modern runs.
- Time Splitter: +15% Sphere damage for 5 seconds after Time Stop actually ends.
  Removed unconditional +12% damage/+3pp critical chance for modern runs.
Timer is simulation-owned and captured by existing runtime snapshot codec; reset
on new run and before restoring missing old fields. Pause does not tick it.
Legacy null-economy runs keep original effects and do not start this timer.
Descriptions RU/EN state exact new values. Core PASS: 13793 assertions,
47389 source comparisons. Inventory: 35 corrected,27 unchanged reviewed,20
remaining reviews. Runtime Editor and Android acceptance not yet performed.


## 2026-11-01 — Stasis Mandala damage-to-shield implementation
Replaced the modern placeholder global damage/critical buff with actual
damage-to-shield conversion. Authored starting balance: every 50 HP actually
lost grants one shield charge; at most one per damage event; carry-over bounded
to 49 HP; existing 5-charge cap. Damage while capped is not banked. Dodges,
blocked hits and incoming damage before armor do not contribute. Lethal damage
does not grant a saving charge. Both direct and DoT damage feed the same helper.
Accumulator lives in runtime model, saved by existing snapshot codec, reset on
new run and before restoring a snapshot with missing field. Legacy runs retain
old effects. RU/EN card explicitly states thresholds and limits.
Core PASS: 13799 assertions,47389 source comparisons. Pure contracts cover partial
loss,threshold,large hit,cap,blocked damage,death and legacy. Actual Editor damage
execution,save roundtrip and Android balance remain open. No APK built.


## 2026-11-01 — Resonance resource artifacts
Resonant Leaf and Resonance Lattice now affect the real player-level Resonance
resource in new runs rather than hidden unconditional Sphere damage/cooldown.
Authored completion decision under approved balance authority: +10% and +15%
charge gained, additive to each other (1.25 combined), multiplicative with
existing ability/region multipliers. RU/EN explicitly say charge gained rather
than claiming all heterogeneous event effects are scaled. Existing event damage,
shield counts, geometry, thresholds and RNG remain unchanged. All charge sources
converge at AddResonanceCharge; multiplier applies once, after event-active gate.
Legacy runs retain old metadata effects and unchanged charge. Five focused
contracts plus full suite PASS:13792 assertions,47389 source comparisons.
Assertion count changes because two IDs leave unchanged-metadata comparison
loop; their legacy compatibility comparisons remain. Editor and Android
execution/balance remain open. Next: Universal Fold and remaining behavioral
reviews, then abilities and regional roadmap.
