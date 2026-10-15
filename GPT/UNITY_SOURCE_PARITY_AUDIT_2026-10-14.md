# Аудит переноса new-desing → unity-migration

Исходник: `737ef2af4a877e0995db5f92331d536d2f29a934`.
Целевая ветка до работы: `8a400e16d32c207938737e7e3dc05b1921978abc`.
Сравнивается фактическая игра, а не запланированные функции из Blueprint.
Исходные `src`, `public`, тесты и настройки веб-игры в обеих ветках идентичны.

## Состояние до изменений

| Система | Состояние | Доказательства / ограничения |
| --- | --- | --- |
| RNG, базовые формулы, уровни, каталог сфер | Перенесены в C# | `Core/SeededRng`, `RunBalanceRules`, `GameCatalog`, `SphereLevelRules`; выбор семени нового забега пока заменён константой. |
| 10 сфер, 30 веток, 90 финалов | Перенесены, точность требует проверки взаимодействий | `SphereAttackAgent`, `SphereEvolutionCatalog`, `SphereEvolutionCombatRules`, обработчики в `EchoSphereRuntime`. Наличие ID не доказывает идентичность поведения. |
| 8 активных способностей, выборы, слоты, эволюции, синергии | Перенесены | `LevelUpAbilityRules` и обработчики `Activate*`; точность VFX и порядка обработки отдельно не подтверждена. |
| Network, Geometry, Resonance, Formation Follow | Перенесены частично | Есть граф, геометрии, бонусы, эффекты и управление; обновление графа раз в 0.12 сек отличается от исходника, который обновляет его каждый кадр. |
| Персонажи и mastery | Перенесены | Каталог/выбор/сохранение и специальные обработчики присутствуют; комментарий об отсутствии выбора устарел. |
| Обычные враги, роли, элиты, боссы | Перенесены частично | Есть движение, статусы, атаки и расписания. Смерть splitter / splitter_prime / bomber не воспроизводит исходные эффекты. |
| Regions, POI, испытания, Endless | Перенесены частично | Есть два региона, фазы, карманы, расписания, POI, награды и unlock. Общая последовательность кадра отличается из-за независимых MonoBehaviour.Update. |
| Артефакты, наборы, протоколы | Перенесены частично | Каталоги и обработчики присутствуют; Chaos Orb отсутствует. Защита, отражение и вампиризм имеют подтверждённые расхождения. |
| Движение и рывок | Неидентичны | Исходник: dash 0.2 сек, 600 px/sec плюс обычное движение, направление текущих клавиш или вверх, cooldown 3 сек. Unity: 0.14 сек, speed×4.2, последнее направление, cooldown с бонусом артефактов. |
| Столкновения и границы поля | Не перенесены полностью | `spaceCollision.ts` выполняет два прохода tower/enemy/player separation, sweep и ограничение поля 2400×2400. Unity содержит placement validation, но не эквивалент всей физики. |
| XP и подбор | Частично совпадают; внешний вид расходится | В актуальном `ExperienceOrbAgent` вращение `dt*1.4`, перенос по velocity, затухание velocity `*=0.9`, притяжение `min(1,20*dt)` и радиус проверки `PLAYER_RADIUS+6` соответствуют формулам `updateXpOrbs()` по структуре. `AddExperience(int)` добавляет результат умножения в float `_xp` без явного округления после множителя. Отдельно сверить combo-value/радиус спавна. Визуально Unity использует Disc/Hexagon/Star, а исходник `drawModernXp()` рисует шестигранный осколок с белой обводкой и бликом. |
| Мутации игрока | Не перенесены | Уровни 10/20/30/40 дают damage/radius/speed/regen; Unity явно оставляет эти состояния отсутствующими. |
| Смерть игрока | Неидентична | Исходник замораживает симуляцию на deathTimer=0.90 перед результатом; Unity немедленно завершает забег. |
| Сохранение / продолжение забега | Не перенесено | Нет эквивалента `runPersistence.ts`, Continue, автосохранения раз в секунду и pause/save при потере фокуса. |
| Магазин, настройки, records, achievements, tutorial | Перенесены | Есть экраны и PlayerPrefs, но визуальное совпадение со всеми React/CSS экранами не подтверждено. |
| База знаний | Перенесена неверно / частично | Исходник имеет Core/Spheres/Monsters/Bosses/Abilities/Artifacts/Evolutions, поиск и открытия. Unity экран заменён списком протоколов. |
| Меню, HUD, pause planner, карта, modal UI | Частично | Используются IMGUI-экраны; нет доказательства идентичности композиции CSS, всех вкладок planner, radar и подсказок. |
| Игровая графика, VFX, audio | Частично | Есть нативные визуальные классы и синтез звуков. Точное визуальное совпадение требует ручной проверки. |
| Standard Sphere texture | Повреждённый исходный ресурс | Все найденные версии upper-crystal имеют blob `7278d58e78b327bce816b493e3f338287c7f2d19`. Исходный `getArtImage()` не возвращает непрочитанное изображение; процедурные core/ring рисуются независимо. Новую картинку придумывать нельзя. |

## Неисправности проверок до изменений

- `check-unity-project.mjs` использует LF-зависимое текстовое сравнение и падает на Windows CRLF.
- CoreChecks не компилируется: два вызова `GetOrbitalAngularSpeed` не передают обязательный `finalId` (Program.cs:564).
- Только Unity Editor может проверить UnityEngine-часть. По просьбе пользователя сборка Unity/APK в этом цикле не запускается; её он проверит вручную.

## Правила этого цикла

Исправлять только доказанные расхождения с указанным исходником. Не менять веб-игру, не добавлять задуманные, но отсутствующие функции, не заменять исходный рисунок новым. Итоговый статус фиксировать по фактическим проверкам, не объявлять полную идентичность по текстовому preflight.

## Выполненные изменения

| Область исходника | Реализация переноса / исправления | Проверка |
| --- | --- | --- |
| `engineLoop.ts`, `engineState.ts`, `rng.ts` | Единый порядок кадра, ограничение dt, исходный выбор seed, живой обход массива врагов, рывок, мутации 10/20/30/40, Chaos Orb, задержка смерти 0.90. | C# Core и прямые TS → C# fixtures; runtime — статическая сверка. |
| `spaceCollision.ts` | Два прохода разделения, swept collision, массы, Follow, границы 2400×2400, преобразование оси Y. | 60 сценариев из исполняемого исходника, в том числе 30 с Follow. |
| `sphereProgression.ts` | 290 комбинаций уровней/веток/финалов, по 39 параметров; объединение глобальных модификаторов, стихии и 90 mastery ID. | 1160 наборов модификаторов, 70 базовых уровней, 90 mastery ID; генераторы в режиме `--check`. |
| `engineSpheres.ts`, `engineCombat.ts` | Формула урона, порядок критов/процедур/status/HP/смерти, ветки всех 10 сфер, Aura/Gravity/Pulse loops, Chain, Prism, Orbital sweep и latch, Void, геометрии и сетевые множители, размер/стихия/рикошет снарядов. | Проверки Core и статическая сверка live runtime; целый бой в Unity не исполнялся. |
| XP / враги / боссы | Дробный XP до округления после комбо, скорость орбов, подбор и притяжение; обработка нескольких уровней/орбов за кадр; splitter, splitter_prime, bomber; снаряды принадлежат врагу и обновляются в его ходе; параметры призванных существ без дополнительного усиления сложности. | Fixtures включают 35 комбинаций дробного XP/комбо. |
| `region.ts` | Раннее обновление состояния региона, отложенный spawn босса, порядок испытаний/mastery, один первый доступный POI за кадр, исходные тексты перехода между карманами. | Core/fixtures для правил; порядок runtime сверён по исходнику. |
| `runPersistence.ts` | Автосохранение раз в секунду, pause/save при потере фокуса, Continue в паузе, восстановление состояния/коллекций/RNG/сущностей; сброс временных объектных ссылок. | Roundtrip коллекций, nullable/readonly/value types и RNG со значением 0. Формат хранения нативный, не обменный с localStorage веб-версии. |
| `KnowledgeBase.tsx`, `App.tsx`, экранные компоненты | 305 записей базы знаний, поиск и открытия; исходные bilingual choice/card тексты, меню, HUD, radar, подсказки сети, pause planner, артефакты/наборы/синергии, выбор регионов, настройки, результаты/records. | Генератор каталога проверяет содержимое относительно исходника; визуальное совпадение не подтверждено запуском Unity. |
| `MobileControls.tsx`, `TutorialOverlay.tsx` | Одновременные pointer/touch состояния, отдельный владелец джойстика, handedness/interface scale, размещение/перетаскивание/удаление, подтверждение «Далее», spotlight и первый выбор улучшения с изменением HP/XP threshold. | Статическая сверка; touch на устройстве не проверен. |
| `renderer.ts`, `elementalVisual.ts` | Источник фона/звёзд/границ, облики персонажей и Сферист, щит/смерть, исходные региональные линии/волна границы и flash banner; стихии с финалами Prism Spectrum и исходными flame/snowflake/poison paths. | C# syntax/member audit; Unity mesh/shader отрисовка и blend/font отличия требуют ручной проверки. |
| Unity project / проверки | Стабильные `.meta` с сохранением имеющихся GUID, IL2CPP preservation для snapshot reflection, Windows CRLF исправления preflight/одного исходного теста, CI с проверкой генераторов и исходных тестов. | `git diff --check`, статический preflight, локальные автотесты. |

## Особенности исходника, сохранённые без переосмысления

- Реальные routine Level-Up choices используют active abilities и spheres. Рассчитанный, но не добавленный в `sourcePools` passive pool не превращён в новую игровую функцию.
- `renderer.ts` рисует `REGION_POCKETS` (alias Basin) также в Rift; gameplay использует реальные карманы выбранного региона. Этот факт сохранён.
- POI `elite_nest` вызывает обычный `spawnEnemy` два раза; `boss_trace` показывает текст, не создаёт босса. Перенос не подменяет эти действия ожидаемыми по названию.
- У Orbital Blade действительно одновременно присутствуют множитель в `sphereModifiers` и множитель контакта. Оба сохранены.
- Призрачные ссылки Hunter/Engineer/Void, sourceSphere и hitEnemies не переносятся через сохранение, как и в исходном JSON.
- Персистентные обращения экрана результата к Basin по умолчанию и начальный interface scale 0.75 сохранены по выполняемому коду исходника.
- GLB/ранние 3D эксперименты не активированы: рабочий источник использует выключенный 3D renderer.

## Проверки после изменений

- Исходная веб-игра: **231/231 тест прошёл**. Единственное изменение теста — разрешить CRLF в проверке исходного текста; проверяемое поведение сохранено.
- C# CoreChecks: **1193 assertions прошли**.
- Исполняемый TypeScript → C# fixtures: **47 339 сравнений прошли**, плюс snapshot collections/RNG roundtrip. Используются оговорённые числовые допуски; это не утверждение битовой идентичности float/double.
- Три генератора: `--check` прошёл, каталог и параметры совпадают с исходными данными.
- Unity project preflight: прошёл. Это текстовая/структурная проверка, не Unity compiler.
- Дополнительный локальный Roslyn syntax/member audit: ошибок не обнаружено. Он не заменяет разрешение типов UnityEngine или импорт пакетов.
- `src` и `public` не изменялись. Источник `new-desing` не изменяется этим переносом.

## Оставшиеся ограничения проверки

По прямому указанию пользователя не запускались Unity Editor, сборка Unity/APK и устройство. Поэтому полная игровая и визуальная идентичность ещё **не подтверждена**: вручную нужно проверить импорт SVG/пакетов, компиляцию runtime/шейдера, сцены, анимацию, звук, touch и продолжение забега на фактическом Unity/Android. Автотесты не исполняют весь runtime и все комбинации взаимодействий.

Проверка PNG IDAT через zlib подтвердила `invalid distance too far back` у `upper-crystal.png`, `lower-crystal.png`, `energy-core.png` и `spherist-shield-96.png` непосредственно в исходнике. `stabilization-ring.png` распаковывается. Исправных замен не найдено; новый рисунок не создавался. Используется выполняемый procedural fallback исходника. Это ограничение источника, а не основание объявлять восстановленными отсутствующие данные.

Предыдущие 80–85% были приблизительной оценкой реализации по аудиту. Число не является измерением покрытия тестами или доказательством готовности Unity-сборки. Этот документ разделяет внесённый код и непроведённую проверку в редакторе.


## Визуальный цикл 2026-10-14 (после загрузки сравнительных скриншотов)

Исходник не менялся: `new-desing` = `737ef2af4a877e0995db5f92331d536d2f29a934`.

Внесены два узких исправления в `unity-migration`:
- `SphereRuntimeVfx2D.cs`: у Standard была общая сборка ядра/кольца, но отсутствовали детали верхнего и нижнего кристаллов. Добавлены две фасеточные терминальные детали, чтобы восстановить читаемость исходной двухкристальной сборки. Повреждённые PNG исходника не импортировались и не заменялись новой графикой.
- `EchoSphereSourceHome.cs`: размер центрального ядра меню ограничен доступной высотой с учётом шапки, нижней навигации и состояния Continue. Это предотвращает уход содержимого под нижнюю навигацию на коротких экранах.

Коммиты этого прохода:
- `931a14300e63be8d1a2f665ed5eec7e62d1e962a` — Standard Sphere crystal silhouette.
- `3603f05e2327d43e4ef9e2394cbbd9a3c9a8aeda` — short-screen home layout.

Проверка предыдущего визуального прохода: GitHub Actions Unity Migration Contracts #568 для HEAD `3603f05e2327d43e4ef9e2394cbbd9a3c9a8aeda` завершился SUCCESS. Это структурные/Core-проверки, не проверка визуального совпадения на устройстве. Визуальная проверка APK не проводилась; точное совпадение меню/регионов/HUD и силуэтов остальных сфер не объявляется подтверждённым.

Следующий визуальный проход: сопоставить по скриншотам размеры и расположение HUD/селектора сфер, композицию выбора регионов, а также все 10 силуэтов и оружейные детали строго по `new-desing`. Для Standard Sphere наличие отдельного оружейного элемента в активном renderer-коде пока не подтверждено; не дорисовывать его без найденного источника.


## Визуальный цикл 2026-10-14 — мобильная навигация главного меню

Сравнение сохранённых скриншотов `GPT/new-desing2.jpg` и `GPT/unity-migration 1.jpg` подтвердило отдельное расхождение: на узком экране исходник размещает шесть разделов нижней навигации в сетке 3×2, а Unity показывала все шесть в одной тесной строке. Из-за этого подписи были мельче, а область навигации заметно ниже по высоте.

Исправлено в `UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceHome.cs`:
- для узкого UI viewport (`_uiWidth <= 650`) нижняя навигация теперь использует три колонки и две строки;
- высота навигации резервируется до расчёта доступного пространства центрального блока, чтобы элементы главного меню не накладывались на навигацию;
- для узких экранов центральная графическая область ограничена мобильным размером исходного CSS (`210` UI units вместо общего предела `270`).

Коммит реализации: `c6d28598f933bfd47ef277b53f2146b20c6362d5`.

Ограничения этого шага:
- фактический Unity APK после изменения не запускался, поэтому визуальное совпадение не подтверждено;
- иконки шести разделов пока остаются текстовыми подписями, тогда как веб-исходник использует пиктограммы;
- сравнение боевого скриншота `GPT/new-desing 1.jpg` / `GPT/unity-migration 3.jpg` оставляет открытыми проверку радиусов/яркости региональных колец, размеров сфер относительно ядра, отображения орудий и читаемости врагов. До исправления требуется проследить активный рендер-путь и масштаб преобразования, не подгонять числа наугад.

Следующий шаг: проверить CI для `c6d28598f933bfd47ef277b53f2146b20c6362d5`, затем сопоставить исходный `drawRegionLayer()` и Unity `RegionFieldVisuals`/`SourceRegionVisual2D` с учётом реального масштаба камеры. Исправлять только подтверждённые отличия прозрачности/ширины; затем отдельно сверить все десять визуальных семейств сфер с активными source-visual функциями.


## Визуальный цикл 2026-10-14 — устранение двойного слоя региональных границ

По сравнению `GPT/new-desing 1.jpg` и `GPT/unity-migration 3.jpg` обнаружен источник чрезмерно заметных региональных колец. В исходнике `src/renderer.ts:688-720` слой карманов рисуется один раз. Unity одновременно рисовала те же границы, швы и маркеры в `RegionFieldVisuals` и повторно в `SourceRegionVisual2D.LateUpdate()`.

Исправлено:
- `SourceRegionVisual2D` больше не рисует второй экземпляр постоянной геометрии карманов;
- временный эффект `SourceFlashBoundary` сохранён в этом компоненте;
- удалены ставшие неиспользуемыми массивы центров, ID и цветов карманов.

Коммиты:
- `dd12dec3d6567c09d236a3920d9347d9a6f9bdc8` — убрать дублирующийся слой границ;
- `6b5c004dbb07c997ade42f107f3713fc3ed434bf` — убрать неиспользуемые данные.

Проверки: Unity Migration Contracts #571 для HEAD `6b5c004dbb07c997ade42f107f3713fc3ed434bf` — SUCCESS. Запуск Unity Editor, сборка APK и ручная проверка экрана после изменений не выполнялись; это подтверждает только CI-контракты, а не визуальную идентичность. Более ранний запуск #570 для промежуточного коммита ещё отображался в очереди, но #571 прошёл на более новом HEAD, содержащем итоговое состояние кода.

Остаётся открытым: ручная проверка, насколько уменьшилась яркость/толщина колец; мобильные иконки нижней навигации ещё не портированы; экраны выбора региона и паузы требуют покомпонентной сверки; все десять сфер, орудия, относительные размеры врагов и HUD требуют следующих адресных проходов. Следующий шаг — проверить активную геометрию сфер и эмиттеры в `SphereRuntimeVfx2D.cs` против соответствующих `src/spheres/*Visual.ts`, затем перейти к экрану выбора региона. Не менять радиусы/масштабы врагов без подтверждения по source-to-world преобразованию.


## Проверка Level-Up и XP — 2026-10-14 (продолжение)

Текущая ветка проверена по API: `unity-migration` HEAD до этого документального уточнения — `ff1dc197d820435c6d2742330a05ac6275d041fc`; `new-desing` источник — `737ef2af4a877e0995db5f92331d536d2f29a934`. Код исходника не изменялся.

### XP

- `ExperienceOrbAgent.SimulationTick()` уже содержит начальную velocity, вращение `dt * 1.4`, перемещение по velocity, затухание `velocity *= 0.9`, магнитное притяжение через `min(1, 20 * dt)` и проверку радиуса до начисления. По форме эти шаги соответствуют `src/engineLoop.ts:updateXpOrbs()`; прежнее описание Unity как притягивающей осколки с постоянной скоростью устарело.
- `EchoSphereRuntime.AddExperience(int)` не округляет результат после применения множителя: `amount * multiplier` добавляется в float `_xp`. Сам параметр и результат `ComboRules.GetExperienceOrbValue(...)` целочисленные; точное соответствие формулы combo-value и базового значения XP ещё требуется проверить отдельно.
- Подтверждено визуальное расхождение: исходник `src/renderer.ts:drawModernXp()` рисует шеститочечный вытянутый осколок, тонкую белую обводку и белый блик; Unity при создании XP выбирает общие `Disc`, `Hexagon` или `Star` и цвет по `xpReward`. Это место для узкого визуального переноса, без изменения награды/правил.

### Level-Up

- Исходник формирует обычный пул из активных способностей и улучшений сфер; рассчитанный `passivePool` намеренно не включён. Unity также формирует эти два источника, и оба ограничивают получение новой активной способности свободным слотом.
- Сверены формулы базовых весов: Unity содержит аналоги source pressure/pity, level pressure, active-copy pressure, предпочтений персонажа, незавершённого уровня способности и разнообразия источников. Поэтому само наличие трёх карточек сфер не доказывает дефект весов.
- Остаётся проверка фактических игровых наблюдений: для доказательства ошибки нужны уровень игрока, активные слоты/способности и показанные карточки. Без этого вероятности и слотную механику не менять.

В этом диагностическом цикле игровые файлы не менялись. Unity Editor/APK не запускались. Следующий адресный шаг: сверить исходное создание XP-орба и `ComboRules.GetExperienceOrbValue`, затем перенести только подтверждённое отличие формы XP-осколка; параллельно не менять Level-Up без воспроизводимого расхождения карточек.


## XP pickup source-parity pass — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation base: `unity-migration` `fbe71bdc27b06a6bb47d9d27364b7268a97c3cc1`.

### Combo XP calculation verified

- Source `src/engineCombat.ts` creates each orb with `Math.round(enemy.xpValue * s.player.comboMult)`.
- Unity passes the enemy's floating-point XP reward and current combo multiplier to `ComboRules.GetExperienceOrbValue`; the helper computes `floor(max(0, reward) * max(0, multiplier) + 0.5)`, matching JavaScript `Math.round` for the non-negative reward domain. Existing parity fixtures and CoreChecks cover fractional XP cases.
- XP awarded on pickup remains a separate float multiplier in `AddExperience`, as in source `gainXp`; this pass does not change reward, combo, pickup radius, motion, or seeded RNG consumption.

### Confirmed visual correction

- Replaced XP-value-dependent Disc/Hexagon/Star sprites and colors with the source's fixed cyan `#63e6ff` six-point shard.
- Ported the source vertex proportions, near-white `rgba(230,253,255,.82)` outline and `rgba(255,255,255,.58)` highlight as separate tint-safe render layers.
- All XP shards now use the source fixed radius of 6 pixels (with the existing source-pixels-to-world conversion); XP value no longer changes visual size or silhouette.
- Added structural checks so the Unity migration guard verifies the source-specific shard, outline, highlight and fixed-size path.

No Level-Up rules were changed. Unity Editor/APK and device rendering remain unverified; structural CI does not establish full visual parity. `new-desing` was not modified.

## Standard Sphere charge-weapon visual — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation base: `unity-migration` `7666334e8dca871f9b545600c7c661752bb6b9ac`.

- Compared active `src/renderer.ts:drawModernSphere()` with `EchoSphereRuntime.CreateSphere()` and `SphereRuntimeVfx2D`.
- Confirmed source Standard has a separate `drawStandardAttackVfx()`: a cyan, rotation-facing charge muzzle, two curved energy contours, an arc and a white emitter highlight, shown as the existing `attackTimer` approaches its 1.2-second firing point.
- Unity previously created no equivalent visual component for Standard (unlike the other Sphere families). Added presentation-only `StandardSphereAttackVfx2D`, driven by the existing `SphereAttackAgent.AttackTimer` and `VisualRotation`. It does not alter combat cadence, damage, target selection, RNG or state.
- Added a structural guard for the source mapping and live runtime wiring.
- The Standard crystal PNG is unavailable in the Unity art folder and the existing code explicitly documents the source image payload as corrupt, so this change does not invent or substitute crystal artwork. The source core/ring fallback remains unchanged.

Unity Editor/APK and device rendering remain unverified; contract CI does not prove visual identity. `new-desing` was not modified.

## Orbital satellite visual parity — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation base: `8ba034f7738e0834f66dfb3d9e2416b46d83ae05`.

- Compared `src/spheres/orbitalVisual.ts:renderOrbitalSphereAttackersVfx/drawSatellite` and `drawOrbitalElementTrail` against `SphereAttackAgent.UpdateOrbitalElements/EnsureOrbitalElements` and `SphereRuntimeVfx2D`.
- Confirmed mismatch: Unity's live Orbital combat elements were oversized generic Disc sprites with large Disc glows, while the source draws compact faceted dark satellites with a cyan/element-colored outline and white highlight; Blade uses its narrow blade silhouette.
- Replaced only the Orbital element presentation with source-scaled mesh geometry at the already-simulated element transforms. The existing combat element positions, count, collision/contact logic, hit cadence, damage, RNG and branch rules are unchanged.
- Ported the source's Fire/Freeze/Poison satellite accents and trails using existing `SphereElementalRules.ResolveElement` mapping and source colors.
- Added a structural check to guard the source-mapped satellite renderer and live transform access.

Remaining visual checks: compare the complete Orbital body/core and intersecting ring ellipses at device scale; verify the source's brief contact flash timing against Unity's existing orbital hit events. Unity Editor/APK/device rendering are not verified by this structural change. `new-desing` was not modified.


## Orbital core pulse and attack flash — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.

- Compared `src/spheres/orbitalVisual.ts` and `src/spheres/visualHelpers.ts` with `SphereRuntimeVfx2D.cs` and `SphereAttackAgent.cs`.
- Corrected the Orbital core pulse frequency from Unity's shared 4.2 rad/s pulse to the source Orbital pulse at 3.0 rad/s. Other Sphere families retain their existing pulse.
- Added a presentation-only 0.28 s Orbital attack flash on the existing Orbital cadence edge, matching the source's eased strike position, short white streak, and white flash near the closest living enemy within the source visual threshold. It reads current element position/radius and live enemies; no attack cadence, collision, damage, RNG, or branch logic is changed.
- The two main orbit ellipses and the partial inner trace use the source's same radius, rotation, opacity, and line-width coefficients in the current implementation; no speculative geometry change was made.
- Added a structural guard for the pulse rate and flash wiring.

Verification limitation: Unity Editor/APK/device rendering is not verified by the structural CI. The source branch was not modified.


## Multi-family source animation phase pass — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.

- Audited active source renderers for Sniper, Shotgun, Chain, Aura, Orbital, Prism, Gravity, Pulse and Void against `SphereRuntimeVfx2D.cs`.
- Confirmed Unity had introduced a type-derived arbitrary phase offset to the non-Orbital orbit rotations and Sniper scan. The source families use their authored time-based rotation directly. Removed that extra offset without changing their source rotation rates.
- Replaced the shared 4.2 rad/s pulse with the source `stateColor` pulse: 3.4 rad/s plus source-pixel position phase (`x * .008 + y * .006)); disabled families now hold pulse at .5, matching `visualHelpers.ts`.
- Orbital keeps its distinct source phase (`x * .011 + y * .007), wrapped to one turn), its 3.0 rad/s core/orbit pulse, and its separate outer/inner orbit rotations. Sniper scan no longer receives an unsupported phase offset.
- Checked Sniper, Shotgun and Prism emitter vertex proportions, local offsets, facing rotation, and white highlights against their active source visual functions; the existing geometry matches the source coefficients, so no duplicate redesign was made.
- Confirmed the radius path already includes the source Chaos Orb radius buff (+20% while active) and Mutation Stage II (+15%) through `GetSphereRadiusMultiplier`; no duplicate multiplier was added.

Unity Editor/APK/device rendering is not verified by structural CI. `new-desing` was not modified.


## Sphere core glow radius pulse — 2026-10-14

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation base: `eada83f5e92f452d19961835f4ef6df894199a26`.

- Compared `src/spheres/visualHelpers.ts:core()` with `SphereRuntimeVfx2D.Rebuild()` and the active Orbital renderer.
- Confirmed source glow disc radius is `radius * 0.58 * (1 + pulse * 0.035)`; Unity had used a fixed `coreRadius * 0.62`, losing the source's small pulse-driven radius expansion and using a mismatched base radius.
- Updated only the mesh glow radius expression to the source coefficient and pulse factor. The source pulse amplitude, Orbital 3.0 rad/s phase, disabled-state hold, core disk radius, outline, orbit paths, combat, damage and RNG remain unchanged.
- Added a structural regression assertion for the radius formula and Orbital-specific core pulse selection.

Unity Editor/APK/device rendering remains unverified; structural CI is not proof of full visual parity. `new-desing` was not modified.

## Standard Sphere attack VFX compile fix — 2026-10-14

- Fixed the missing `EchoSphere.Core` namespace import in `StandardSphereAttackVfx2D.cs`. The class uses `SpherePlacementRules.SourcePixelsToWorld`, whose declaration is in the `EchoSphere.Core` namespace; without the import, C# reports CS0103 for the unresolved identifier.
- Added a structural regression guard for the namespace import and source-pixel calibration reference.
- This is a compile-scope correction only. Charge timing, muzzle geometry, rotation, colors, attack logic and damage are unchanged.

Unity Editor compilation and APK/device validation still require their own verification. Structural CI does not prove Unity compilation. `new-desing` remains unchanged.

## Main-menu navigation icons and source descriptions — 2026-10-15

Source baseline: `new-desing` `737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation base: `unity-migration` `c6e0890aba92a21705a3fe4bd6a4217af5d021d0`.

- Confirmed source menu in `src/App.tsx` uses six Lucide icons in order: `UserRound`, `Store`, `Trophy`, `Award`, `BookOpen`, `Settings`. Unity's six destinations already worked, but displayed text-only navigation. Added corresponding thin line-art icons and stacked them above the labels, matching the source's column layout; page targets, responsive breakpoints and click behavior are preserved.
- The Unity knowledge runtime loads `Assets/Resources/EchoSphere/SourceCatalog.json`; its schema carries localized `nameRu/nameEn`, `descRu/descEn`, `detailsRu/detailsEn` across Core, Spheres, Monsters, Bosses, Abilities, Artifacts and Evolutions. These source descriptions are preserved rather than paraphrased or invented.
- Existing XP shard and Standard/Orbital VFX corrections are recorded in the preceding audit sections and were not rewritten in this pass.

Verification boundary: structural preflight and CI can check source wiring, not Unity GUI appearance. Unity Editor, APK and device rendering have not been run in this cycle. `new-desing` remains unchanged. Remaining visual/system parity gaps listed earlier in this report are still open; this commit does not claim complete migration.

## Main-menu core orbit parity — 2026-10-15

Compared the final `.es-main-menu-legacy` overrides in `src/conceptStyle.css` with `DrawSourceMenuCore()`.

- Confirmed discrepancies in the Unity menu core: orbit B radius and orbit A/B rotation angles differed from the final CSS; all five tracks shared one vertical flattening factor; orbit B was cyan instead of violet; and all tracks were static while the source rotates them at 12/15/18/8/10-second periods.
- Ported the source's five radii (`9%, 22%, 31%, 15%, 39%` insets), per-track ellipse tilt, base rotations, colors/alpha and direction/speed. The center core, layout, navigation targets and gameplay state are unchanged.
- Added a structural guard for the source-mapped radii, tilt factors, rotations, violet orbit and live animation time.

Unity Editor/APK/device rendering remains unverified; CI validates the code contracts but cannot prove the visual result on the actual phone. `new-desing` remains unchanged.
