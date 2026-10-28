# Завершение архитектуры Unity — 2026-10-27

Ветка unity-migration. Начальный baseline da74c8eadaa45ac44c06a622305383305ebfd327.
Цель: сохранить существующие механики, RNG, сохранения, утверждённый визуал;
убрать остатки миграции и повторную инфраструктурную работу. Незаметные различия
пикселей допустимы. Полный план и Android-приёмка пока НЕ ЗАВЕРШЕНЫ.

## Группа 1 — данные сущностей и удаление отключённых представлений

EnemyAgent2D хранит собственный SourceVisualVariant. Проекция Android, Editor
reference и SavedEntity.variant читают его напрямую. Две отключённые реализации
EnemyCreatureVisual2D / BossPresentationVfx2D удалены вместе с вызовами пустых
визуальных setters. BossCombat по-прежнему предоставляет состояние активному
NativeWorldCreatures. Таймер hit flash .15, фазы, формулы и RNG сохранены.

SourceParticle2D хранит точное прежнее значение SavedEntity.color в данных;
рождение сохраняет исходный цвет, tick — исходный fade. Новые pooled частицы
и восстановленные частицы не создают SpriteRenderer. Старые JSON загружаются
через прежний SavedEntity.color; _savedColor и _visualVariant исключены из
снимка полей, поэтому новые внутренние поля не расширяют формат сохранений.

Preflight обновлён: проверяет действующий NativeWorldEnemyView вместо требования
существования удалённых компонентов. Проверки общего игрового ядра сохранены.

### Проверка

- C# Android player + Editor runtime: 349 файлов, 0 ошибок / предупреждений;
  Editor assembly 6 файлов, 0 ошибок / предупреждений.
- Core 1383 assertions, TS/C# parity 47389 comparisons PASS.
- project preflight PASS.
- Реальный Editor после AssetDatabase.Refresh: owned fixture, 30 проверок PASS:
  нормализация variant, отсутствие старых компонентов, исключение полей из JSON,
  сохранение variant после restore, отсутствие Renderer у частицы, цвет birth /
  fade / restore / следующий tick. Тест не записывает PlayerPrefs.
- Первый probe запускался до фактического refresh Editor и не нашёл новый метод;
  owned fixture удалена, после refresh проверка прошла. Это ошибка диагностики,
  не подтверждённая ошибка продукта.
- Телефон не определяется adb; FPS, GC кадра и Android-приёмка не измерены.

## Осталось

Меню: обновление карточек без замены дерева, резидентность портретов, переходы.
Рендер: локальные общие маски/атласы и сокращение проходов; постоянные ресурсы
сущностей; динамические контуры и текст; пространственные запросы; дальнейшая
очистка Editor/reference. Белая пауза Android остаётся открытой.

Отдельные статусы: compile PASS; fixture PASS; полный Play lifecycle pending;
Android performance pending; CI фиксируется по фактическому опубликованному SHA.

## Группа 2 — постоянные представления и меню

Реализован реестр mutable view resources по владельцу врага: сегменты,
динамическая геометрия, property blocks и boss labels больше не смещаются между
врагами при удалении соседа. Общие неизменяемые формы остаются общими. Умершие
владельцы освобождаются на следующей границе кадров; запас до 64 наборов
переиспользуется, избыток уничтожается. Новое рождение получает новый owner.
Цифры урона закреплены за SourceDamageNumber; отработанные labels возвращаются
в пул (до 256 неактивных), вместо создания Font/Mesh/Material на каждое событие.

Карточки персонажей обновляют selected/unlocked, XP, progress, action и toggle
на месте. Раскрытое mastery дерево сохраняется при сворачивании; меняющиеся
mastery данные обновляют только этот раздел. Тяжёлая GPU-подготовка по-прежнему
ограничивается NativeHudSurface.InViewport; все десять лёгких контейнеров остаются.

Портреты имеют явные leases у главного меню, игрового персонажа и карточек.
Selected владельцы удерживают ресурс; offscreen/hide карточки снимают ссылки.
Последний owner удаляет cache entry и запрашивает Resources.UnloadAsset для
индивидуального Sprite/Texture. Async completion без владельцев также очищается.
Проверка ownership/cache прошла, но реальная освобождённая память телефона не
измерена. NativeWorldSprites очищает mesh entries выгруженных Sprite.

Регион запускает SceneManager.LoadSceneAsync через существующего владельца
маршрута с защитой повторного запуска. PlayerPrefs handoff и создание нового
забега прежние. Полный телефонный cold region launch остаётся открытым.

### Проверка группы 2

- Реальный Editor Play: 16 проверок retained cards, selection/XP/mastery,
  hide/show и сохранения тех же экземпляров карточек PASS.
- 4 проверки portrait lease: sharing, удержание вторым владельцем, удаление
  cache entry последним, повторная загрузка PASS. Native wrapper Sprite может
  жить после UnloadAsset; это не проверка точного времени освобождения GPU.
- 7 проверок ресурсов: identity, переживший сосед, event label identity,
  retirement и повторное использование набора PASS.
- 140 GPU enemy/culling comparisons max delta 0/255, 350 envelopes,
  1 904 832 vertices проверены. Текущие immutable geometry/визуал сохранены.
- Production pointer 53 checks PASS; populated modal lifecycle 420 checks /
  270 frames / 9 stages PASS, 0 новых Console errors.
- 272 Node regression tests PASS; C# Android/editor 0 errors/warnings.
- Первая публикация группы 1 имела CI #886 failure из-за старого имени property
  в source regression test. Тест актуализирован; edc6dcf4 прошёл CI #887 SUCCESS:
  https://github.com/Lir37/echo-sphere/actions/runs/40582198788 .

Полная деформация с сохранённой топологией, атласы теней, spatial queries,
чистка оставшегося reference слоя и device acceptance всё ещё OPEN.

## Группа 3 — локальные маски и пакетные атласы (2026-10-28)

Для ограниченных solid-мешей с blur>=2 маска готовится в локальной системе,
без мировой трансляции. Перемещение с дробными координатами больше не меняет
ключ кэша. Полная область ядра сохраняется и у края экрана; rotation/scale,
geometry version, alpha/opacity и blur по-прежнему инвалидируют маску. Большие,
неизвестные и неразмытые формы сохраняют прежний viewport path. Это не снижение
разрешения, MSAA, числа эффектов или частоты анимации.

Неподготовленные локальные маски группируются в transient atlas pages по sigma:
каждая страница до 512x512, до восьми страниц; избыток идёт прежним индивидуальным
путём. Геометрия каждой маски рисуется в собственный viewport/scissor. Полный
запас kernel+bilinear/MSAA отделяет содержимое соседних областей. Два Gaussian
прохода выполняются на страницу; alpha sampling конечного composite ограничено
областью конкретной маски. Наложение теней и геометрии остаётся в исходном порядке
normal/additive/screen. Retained cache получает CopyTexture конкретного региона.

Новые counters GraphShadowAtlasPages / GraphShadowBlurPasses / GraphShadowBlurPixels
отдельны от ShadowPassCount (число масок), чтобы не выдавать его за число GPU-pass.
В RenderGraph пока остаются UnsafePass/global bindings; raster API migration
не объявляется выполненной. Static cache ещё использует отдельные retained RT;
в атласах сейчас готовится dynamic/miss слой. Все текущие работы — кандидаты
до реальной Android-проверки.

### Проверка группы 3

- 216 GPU-сравнений локальных масок с прежней растеризацией: DPR 1/1.75,
  три угла, normal/additive/screen, смещения, край/вне экрана, дробные позиции.
  max alpha difference 2/255, max RGB*alpha difference 3/255; максимальное
  среднее взвешенное отличие 0.0121/255 на пиксель. Raw RGB на почти прозрачных
  пикселях отличается до 250/255; это НЕ побитовое совпадение.
- 162 проверки реального cache hit после дробного перемещения PASS.
- Actual URP atlas fixture: 25 перекрывающихся теней, 6 размеров/поворотов/DPR,
  normal/additive/screen. Вместо 50 blur passes выполняется 2 или 4; вместо
  75 transient textures — 3 или 6. Это числа команд/ресурсов, не FPS/GPU ms.
  Пять случаев побитово совпали с individual path; шестой: alpha и RGB*alpha
  max 1/255, raw RGB max42/255 на слабопрозрачных пикселях.
- Atlas-to-retained CopyTexture и следующий reuse: 12 GPU сравнений, max delta
  0/255. Последующие 25 масок: 25 cache hits, 0 новых transient textures / blur.
- Существующий enemy/culling GPU fixture: 140 comparisons, max delta0/255.
- C# Android player/editor, 350 runtime +6 Editor: 0 errors/warnings.
- Предыдущая группа 20f96709 прошла CI #888 SUCCESS:
  https://github.com/Lir37/echo-sphere/actions/runs/40583178176 .

Открыто: эффективность live backdrop Gaussian меню; деформируемая topology;
spatial queries и явные адаптеры остальных save entities; дальнейшая очистка;
белая Android-пауза; итоговые FPS/GC/память и 15-минутный device test.

## Группа 4 — стекло и удаление неиспользуемых портов (2026-10-28)

NativeBackdropGaussian объединяет соседние Gaussian taps только в линейном
texel-centred внутреннем участке при gamma-compatible чтении. Reflection,
границы атласа, дробная фаза и conversion сохраняют прежние отдельные выборки.
Радиус, kernel weights, нормализация, зеркальные края и два прохода сохранены.
Это уменьшает число texture reads в допустимых участках, но ускорение на Mali
и общие FPS/GC ещё не измерены. Нельзя обещать двукратный выигрыш всего фильтра.

Реальный UI Toolkit: 9 GPU сравнений, density 1/1.75/2.75, sigma 2/6/12,
дробные позиции, повёрнутые панели, текст/alpha, соседний фильтр. max raw RGB
1/255, alpha0/255, взвешенныйRGB1/255. Positive control с отключением основного
blur изменил 26115–183413 пикселей: эффект действительно работал.

Android shader asset bundle для NativeBackdropGaussian/SourceCanvasShadow
SUCCESS, 11.64 sec, после финальной правки single-return PairRead. Первоначальные
предупреждения uninitialized PairRead устранены; текущий ShaderUtil возвращает
пустые messages для обеих shader. Export имеет отдельные предупреждения, что
AssetBundle/Manifest module выключен в Player; runtime-загрузку bundles игра
не использует, модуль не включался. Это shader compilation, НЕ APK/device test.
Автодобавленный Nintendo Switch2 platform quality entry восстановлен побайтово
к проверенному до сборки HEAD; пользовательские настройки не менялись.

Удалены ещё 9 отключённых/невызываемых runtime-портов и их .meta:
SourcePlayerVisual2D, SourceRegionVisual2D, SourceElementalVisual2D,
SphereElementalVfx2D, SphereRuntimeVfx2D, StandardSphereAttackVfx2D,
ProjectileVfx2D, CombatArcVfx2D, SourceCanvasMesh. Вместе с группой1 это 11 старых
реализаций. Для каждого GUID проверены сцены/prefabs/assets/json: ссылок нет.
Удалены AddComponent и вызовы setters, ранее обслуживавшие отключённые объекты.
Живые типизированные native views, gameplay timer/state/RNG сохранены.

Preflight больше не требует наличия удалённого мёртвого кода (47 старых
source-string assertions заменены проверками active views/retirement). Общие
272 regression tests, Core1383/parity47389 и Unity fixtures остаются.
C# Android player/editor: 341 runtime +6 Editor, 0 errors/warnings.
Live Play pools: 21 checks, 50 reuse cycles и cap220 PASS. Retirement/lifetime
fixture: удалённые types отсутствуют; 210 world-space stars уничтожаются с owner.

Shader/GPU evidence: Documents/UnityEvaluations/ECHO-SPHERE-architecture-20261028.
Новая пользовательская _Recovery/0 (7).unity и её .meta оставлены нетронутыми.
Группа3 b8e70940 прошла CI #889 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40585054541 .

Остаток: пространственные запросы, save adapters остальных сущностей,
деформируемые контуры/rig topology, пакетная геометрия частиц; raster API и
reference isolation, полный lifecycle/память20циклов и device acceptance.
План целиком, P1/P2 и 60FPS НЕ объявляются DONE.

## Группа 5 — spatial broad phase и буферы столкновений (2026-10-28)

Группа4 опубликована как 2fdbceeb4514009d17af1a961673f7deb4400867,
CI #890 SUCCESS: https://github.com/Lir37/echo-sphere/actions/runs/40633757560 .

OrderedSpatialIndex предварительно отбирает кандидатов для FindNearestEnemy,
FindNearestEnemies, FindEnemiesInRadius и TryHitEnemy. Окончательные squared
distance predicates, строгие/нестрогие границы, ignored IDs, сортировка и выбор
равнозначных целей остаются прежними. Кандидаты возвращаются в исходном порядке
списка врагов; перемещение между ячейками не меняет порядок. Большой радиус и
нечисловые координаты возвращают полный список для прежней точной проверки.
Публичные методы со списком по-прежнему возвращают отдельный принадлежащий
вызывающему список: он может пережить вложенные запросы при обработке урона.
Unbounded FindNearestUnhitEnemy и циклы с побочными эффектами не заменялись.

Индекс обновляется при birth/restore, движении EnemyAgent, притяжении/отталкивании,
гравитационном ударе дрона, раздвигании тел, движении/рывке/телепорте босса и
изменении радиуса; Destroy удаляет запись. Restore пересобирает индекс из модели.
Индекс, его scratch list и верхняя граница радиуса не входят в формат сохранения.
Нет Unity Physics, FixedUpdate или изменения RNG/порядка игровых фаз.

ResolveSourceSpaceCollisions теперь повторно использует списки агентов/башен,
описания CollisionBody и grid buckets двух исходных проходов. Каждое используемое
поле тела заново заполняется. Scratch исключён из runtime snapshot. Решатель
сохранил обход соседей/пар, массу, два прохода и clamp; IList foreach заменён
индексным обходом того же порядка, чтобы не выделять enumerators.

Проверки:
- Core: 12462 assertions PASS, включая 3605 spatial и 7474 collision checks;
  TypeScript→C# parity 47389 PASS. 2000 разнесённых объектов: локальный запрос
  возвращает менее20 кандидатов; все точные результаты/порядок сохраняются.
- 240 collision fixtures сравнивают свежие и retained buffers при изменении
  числа тел/башен, follow, границ и массы; позиции совпадают точно.
- .NET GC.GetAllocatedBytesForCurrentThread, 100 прогретых вызовов решателя:
  текущий путь со свежей сеткой 718400 bytes; retained workspace 0 bytes.
  Это отдельный CPU fixture, НЕ GC всего Editor/Android кадра и НЕ FPS.
- Реальный Editor runtime fixture: 4826 проверки исходных linear predicates,
  nearest group/ties, перемещений, изменения радиуса, birth/removal, раздвигания
  тел, рывка/телепорта босса и отсутствия scratch в save schema — PASS.
  При сравнении Unity Transform значения явно сохраняются в float Vector3,
  как в production; расширенная точность промежуточных Mono locals не является
  изменением игровой формулы. Fixture не записывает PlayerPrefs.
- C# player/editor: 343 runtime +6 Editor, 0 errors/compiler warnings.
  272 Node regression tests и Unity project preflight PASS.
- Editor Play запущен; production UI pointer/callback fixture 53 PASS,
  runtime query/collision fixture PASS; 0 новых Console errors. Сохранение
  enemy/particle ownership отдельно 30 PASS. Это не полный tutorial/device run.

Остаток: явные save adapters остальных сущностей; retained topology/rig для
деформируемых контуров и дальнейшая пакетная геометрия/частицы; raster RenderGraph
и изоляция Editor reference; полный cold region/tutorial и память20 переходов;
белая пауза и финальная Android-приёмка/15минут. Телефон не подключён, APK в этой
группе не собирался. Качество, визуальные эффекты и настройки пользователя
не снижались; new-desing не менялся. Общий план остаётся IN PROGRESS.

## Группа 6 — данные pickup/save и удаление скрытых мешей (2026-10-28)

Группа5 8d211009e37912847cf8e31afffac6713eb062f2: CI #891 SUCCESS.

Подтверждены два дополнительных старых построителя: BossProjectileAgent2D
создавал Mesh/Material и Rebuild после каждого движения, StellaChestAgent2D —
Mesh/Material/TextMesh и Rebuild каждый tick. Камера production имеет cullingMask0,
а фактические виды уже рисуются EchoSphereNativeEntityLayers через native
projectile/pickable views. Старые построители, массивы и material/label lifetime
удалены из агентов. Поля движения, damage/radius, source owner, time/collection,
порядок simulation и пороги сохранены. В baseline реальные скрытые меши имели
258 и268 вершин соответственно; теперь оба агента не создают Renderer/MeshFilter.

Массивы старых мешей сериализовались RunSnapshotCodec. На снятых до правки
состояниях снаряд имел2784 snapshot nodes, Stella2917; после удаления presentation
полей —15 и3 соответственно. Это измерение размера структур, НЕ задержек save,
GC целого кадра или FPS. Старые state nodes неизвестных теперь полей штатно
пропускаются Restore. Версия2, ключ PlayerPrefs, SavedEntity envelope/идентификаторы,
игровые поля и sync save/focus policy сохранены. Presentation-only nodes больше
не записываются; нельзя называть новый JSON побайтово идентичным старому.

IRunSaveColor переносит прежний SavedEntity.color в данные XP/health/artifact
pickup, дрона, руны и частицы. Руна хранит прежнее затухание alpha. Новый цвет
исключён из reflected entity state: используется существующее поле envelope.
При restore даже нестандартный цвет старого envelope сохраняется до следующего
обычного обновления. Рождение/restore этих объектов больше не создаёт скрытые
SpriteRenderer, XP outline/highlight дочерние объекты тоже удалены. Реальная
геометрия и цвета NativeWorldPickups/NativeWorldPickableViews не менялись.
Enemy/Sphere ещё используют старые оболочки: их граница данных остаётся открытой.

Проверки:
- Editor запущен заново Unity6000.6.1f1, Pipeline7800 подтверждён.
- Entity data fixture49 PASS в Editor и Play: birth colors, legacy envelope,
  renderer-free restore/tick, rune fade, отсутствие второго color в state.
  Загружены настоящие snapshots с массивами мешей, снятые до изменения кода;
  скорость/урон/радиус/time восстановлены. PlayerPrefs тест не записывает.
- Native transient projection35 PASS: source-owner порядок, counts, поля
  projectile view совпадают с reference; transient V dictionaries отсутствуют.
- Enemy/particle ownership30 PASS; Core12462/parity47389 и Node272 PASS.
- Player/editor compile344 runtime+6 Editor: 0 errors/compiler warnings.
- Реальный URP graph6 GPU cases PASS, max pixel delta0, errors0.
  Это проверка compositor, а не полная визуальная/телефонная приёмка всех pickup.
- Preflight больше не требует мёртвых Stella/XP mesh paths; проверки перенесены
  на активные native outline/highlight/6-point shape и chest layer.

Evidence: Documents/UnityEvaluations/ECHO-SPHERE-architecture-20261028,
retired-mesh-baseline.json и entity-data-boundary.json. Отсутствующие после очистки
Library baseline можно взять оттуда; fixture явно выводит legacyBaseline.
Recovery-файлы пользователя сохранены. APK не собирался, телефона нет.
Остаток: Enemy/Sphere data boundary и снятие оставшихся старых presentation
зависимостей; retained deform topology/particle batching; raster graph/reference
изоляция; cold tutorial/full save-run roundtrip/20 transitions memory и Android.
Общий план остаётся IN PROGRESS.

## Группа 7 — оставшиеся оболочки сущностей и POI data (2026-10-28)

База f75e3d9db25a8602fad4377db5be4a8a9da5fcca, CI892 SUCCESS.
CreatePlayer/CreateSphere, обычные/elite враги, боссы, minions, splitter children
и restore врагов создают только логические корни. Удалены невидимые Glow/Shell/
Inner Frame/Core Light, старые Standard/Orbital sprite construction и невызываемый
TickSphereArtAnimation. Действующие NativeWorldPlayer/StandardSphere/Families/
EnemyView не менялись. Удалён старый LinkBreaker LineRenderer-построитель;
цель, таймеры, сетевое отключение и активный NativeWorldEnemyView сохранены.

Enemy _baseColor теперь получает исходный tint явно от всех production factories,
не читает Shell. Sphere/Enemy присоединились к IRunSaveColor. Проверены прежние
alpha .89 для Standard, .11 Orbital и .18 остальных сфер/врагов; RGB сфер берётся
из существующего GetColor. Restore принимает прежний envelope color отдельно.
SaveCurrentRun вообще больше не обращается к SpriteRenderer: для оставшихся
Projectile/BossProjectile/Stella прежним значением был white, он сохранён.
Orbital collision anchors остаются Transform по существующему публичному
контракту; удалены только их отключённые renderer и glow child. Позиции и sweep
calculation не менялись.

RegionFieldVisuals сохраняет deterministic POI model/планирование и существующий
формат PoiSnapshot. Удалены скрытые pocket ring/seams/markers и POI root/glow/
diamond/TextMesh, их покадровые обновления. Данные больше не зависят от наличия
Sprites/Default shader. До правки fixture обнаруживал78 скрытых Renderer;
после ни Renderer, ни вспомогательных объектов региона нет. Старый IMGUI radar
остаётся только в Editor и выделяет текстуры лишь по явному вызову; обычный
Editor/Android путь их не загружает. Живой NativeWorldRegion/HUD не изменён.

Проверки: снятые до изменений baseline обоих регионов и всех10 сфер/minion:
44 shell checks PASS (точные gameplay snapshots и legacy colors); 56 POI checks
PASS (14 временных/региональных состояний, cadence/position/expiry/restore).
4826 query/collision/movement checks PASS. Реальный Editor Play native enemy
GPU/culling:350 envelopes/1904832 vertices/140 comparisons, maxdelta0. Core12462,
TS/C#47389, Node272 и обновлённый preflight PASS. C#344+6:0 errors/warnings.
Предыдущие статические guards старых sprite construction заменены guards
активных Standard/family/player/region views; игровые проверки сохранены.

Baseline/evidence в Documents/UnityEvaluations/ECHO-SPHERE-architecture-20261028:
entity-shell-*, region-data-*. Нет новых APK/device FPS или полной phone приёмки.
Открыты: оставшиеся transient effect builders (Impact/Pulse и др.), packet geometry/
retained topology, save scratch, raster graph/reference isolation и полная
проверка lifecycle/сохранённого забега/Android. План НЕ DONE.

## Группа 8 — raster geometry, topology и save buffers (2026-10-28)

5a1b3685 (группа7) прошёл CI893 SUCCESS.

Clear, ordered geometry, одиночные и атласные shadow masks теперь записываются
через AddRasterRenderPass/SetRenderAttachment и RasterCommandBuffer. Старые
Unsafe реализации этих операций удалены. Matrix state остаётся явным: меши
используют Unity view/projection matrices; ни render-target binding, ни
texture lifetime этих проходов больше не устанавливаются вручную. Общая
подготовка instancing используется обоими backend без повторения алгоритма.
Blur/copy/composite/present пока остаются Unsafe с прежними shader bindings;
нельзя объявлять весь RenderGraph переведённым на raster или все globals удалёнными.

Реальный URP GPU parity:6 cases, каждый действительно выполнил4 raster nodes,
maxdelta0. Дополнительные12 atlas/cache comparisons после raster AtlasMask:
maxdelta0, новых Console errors нет; прогретый retained кадр без blur/transientRT.
Прозрачный порядок, blend, kernels, padding, разрешение/MSAA сохранены.

NativeWorldRegion сохраняет triangle topology, пока индексы/число вершин не
изменились; обновляет vertices/colors/UV и bounds. Его live/removed collections
переиспользуются. 361 checks на120 меняющихся геометриях: точное равенство
vertices/colors/UV/indices с принудительным полным rebuild; reset/empty cases PASS.
Это устранение лишней topology upload, не доказательство отсутствия всех GC.

Удалён ещё один неиспользуемый renderer-контур сети: LineRenderer glow/link/
highlight и SpriteRenderer comets. NativeWorldNetwork с исходными packets/trails
и модель _networkState не менялись. Native network projection1440 и core view1215
checks PASS; preflight теперь проверяет активные packet speed/trail/width.

Save buffers повторно используют SavedRun/SavedEntity envelopes и entity list;
каждый record сбрасывает variant/auxiliary/owner, неиспользуемые записи освобождают
ссылки на старые snapshots. CaptureRunSnapshot отделён от записи PlayerPrefs,
но SaveCurrentRun по-прежнему синхронно сериализует/записывает, с прежними ключом,
версией2, частотой и pause/focus гарантиями. Проверка14 PASS: object reuse,
стабильный JSON, owner indices, смена типа record, очистка spare references.

RestoreRunSnapshot отделён от чтения PlayerPrefs. Удаляются только активные
сущности собственного simulation registry в прежнем ObjectId-порядке; прежний
global FindObjectsByType затрагивал бы второй runtime. Полный actual Play
capture→JSON→restore→capture:114 checks,12 entities всех11 типов, gameplay states,
envelopes, игрок, POI, RNG и отдельный foreign owner PASS. Null/empty strings
сравниваются после стандартной JsonUtility канонизации, без изменения codec.
Fixture не читает/перезаписывает пользовательский run save. Это не тест записи
PlayerPrefs на телефон и не полный старый APK-save/end-to-end tutorial.

Production pointer53 и populated modal420 checks/270frames PASS без новых
Console errors. Core12462/parity47389/Node272/preflight PASS;344+6C# чисто.
Остаток: transient effect paths (особенно SpherePulse), remaining fullscreen
graph bindings, reference isolation, full cold tutorial/20 transitions memory
и Android/white pause/15-minute acceptance. План IN PROGRESS.

## Группа 9 — source pulse и неактивные scene effects (2026-10-28)

ae8b2f87 прошёл CI894 SUCCESS.
Подтверждено по src/engineCombat.ts:1145: emitSpherePulse после урона создаёт14
частиц с одним RNG angle каждая, velocity90, life/maxLife .35, size3. Unity
вместо этого создавал непроецируемый SpherePulseVfx2D MeshRenderer. Теперь
рождаются те же source particles в существующем пуле/typed native frame, с
конверсией source Y и прежними14 RNG draws. Damage/slow и порядок вызовов не
менялись. Это восстановление подтверждённого исходником эффекта, не новый VFX
или новая игровая механика. Частицы теперь участвуют в обычной simulation/save
как в source; старая отдельная Update-анимация больше не используется.

Удалены SpherePulseVfx2D и уже невызываемые HunterParticleVfx2D/EngineerRelayVfx2D
с .meta. GUID-ссылки в сценах/prefab/assets/json проверены — отсутствуют. Hunter
уже использовал SourceParticle; Engineer — AddSourceLightningVisual/native bolts.
Их source shape/motion/range/timing guards перенесены на активный код.

Дополнительные Unity-only ImpactVfx2D/PulseEffect2D rings находились вне camera
cullingMask и не читались native frame. Они, их создание и Cataclysm scene ring
теперь доступны только при UNITY_EDITOR && ECHO_SPHERE_SCENE_REFERENCE. В обычном
Editor и Player нет этих types, per-hit GameObject/Mesh/Update allocations.
Публичный SpawnImpact сохранён для API совместимости, обычные native hit flash
и EmitSourceHitFeedback не отключены. Это начало изоляции scene-preview средств,
НЕ утверждение, что весь V/generated reference уже выделен в отдельную assembly.

Actual fixture76 PASS:14 pooled particles, exact seed/order/velocity, birth center,
size/life, exactly14 draws; scene preview types отсутствуют. 50 hit calls дают
прежний HP и flash .15, не создают новых GameObject. Play pools21/50reuse/cap220
и полный run-data roundtrip114 PASS. C#341+6:0 errors/compiler warnings.
Новый APK/device FPS не измерен. Остаток: fullscreen graph globals, комплексная
reference isolation и финальная lifecycle/tutorial/device приёмка; общий план
по-прежнему не объявляется DONE.

## Группа 10 — изоляция reference и retained particles (2026-10-28)

383764a5: CI895 SUCCESS. Старый V/VisualJs/generated UI/canvas interpreter
теперь компилируется только с UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS.
Обычный Editor использует production native UI, как Android; фактически V и
Visual_* types отсутствуют. Сравнительные средства сохранены как opt-in build
configuration до окончательной приёмки, не выданы за отдельную готовую DLL.
Генераторы и provenance manifest обновлены вместе с guards. Android JNI остаётся
под !UNITY_EDITOR, native diagnostics доступны в обычном Editor. Typecheck
проверяет пять конфигураций, включая оба opt-in reference assemblies: 0 errors/
warnings,341 runtime +6 Editor C#. Provenance52modules/13outputs и preflight PASS.

Particle feedback использует один retained diamond mesh и переиспользуемые
properties. Реальные GPU18 cases:64 последовательные совместимые частицы дают
1 instanced draw вместо64. Mesh строится один раз.9 случаев pixel-exact; в9
отличается один edge pixel (MSAA rounding при matrix scaling), максимум63/255
на этом пикселе, full-frame mean<=.0006364/255. Не заявляется maxdelta0.
Проверены normal/additive/screen и запрошенные DPR1/1.75/2.75 (cap1.75 сохранён).
Форма, цвета, alpha, скорость/жизнь/seed не менялись.

Actual Editor tutorial fixture исправлен для ожидания однократной async scene
activation; проверяет новый runtime, а не старый region screen в pending frame.
Добавлен routed touch вариант input, существующий mouse вариант сохранён.
20 последовательных полных touch-сценариев PASS: запуск региона, tutorial,
Up/Down, точный tap/drag, upgrade, pause/resume, chest/reward, duplicate command,
death/result, intended region/home. Ни одного нового Console error в этих
проходах. Это Editor callbacks, не физический touchscreen/Android приёмка.
До этого mouse серия остановилась на10-м ходе: input Right вместо Down;
влияние внешнего mouse не доказано. Touch серия полностью повторена20 раз.

После каждого цикла UnloadUnusedAssets + GC: game-named textures34,meshes34,
materials55; runtime/world owners1/1 и RenderTextures96 остаются постоянными.
Все textures колебались1318..1327 без монотонного роста. Это выборка native
owners и game-named ресурсов, не доказательство отсутствия любых утечек памяти.
Evidence: UnityEvaluations/ECHO-SPHERE-architecture-20261028/twenty-native-lifecycles.json.
Все10 исходных Editor PlayerPrefs восстановлены и проверены; фоновые настройки
возвращены. Новые checks не меняют пользовательское сохранение.

Остаток: fullscreen graph blit/copy/composite globals и финальная Android
визуальная/white-pause/15-minute производительность; план IN PROGRESS.

## Группа 11 — raster filters без общих shader uniforms (2026-10-28)

После изоляции reference CI896/897 выявил две пропущенные зависимости проверок:
VisualChecks должен явно включать ECHO_SPHERE_REFERENCE_VISUALS; manifest
generated outputs/index должен учитывать изменившиеся guards. Исправлено без
изменения source/assets/выражений;52 output bodies сверены с383764a5 (разница
только header guard). 39af483b: CI898 SUCCESS. Локально полный VisualChecks и
все восемь exporter/provenance checks PASS.

Horizontal/vertical blur, atlas blur, ordered shadow composite и world->UI
present переведены с Unsafe+CommandBuffer.Blit на raster attachment passes.
Их source texture, sigma, cached weights, direction, tint, offset, rectangle и
atlas bounds передаются через переиспользуемый MaterialPropertyBlock. Нет
SetGlobal для параметров фильтра. Сохранены normal/additive/screen, MSAA4x,
разрешение, gaussian и ограничение области composite. Fullscreen quad сохраняется
у владельца и уничтожается с ним; его UV следуют graphicsUVStartsAtTop.
Shader vertex branch включается только native draw property block; прежние
Blit callers/reference сохраняют vert_img path.

Точный CopyTexture/subregion из atlas в retained mask оставлен отдельным transfer
Unsafe-pass (API копирования), с явными read/write dependencies, без изменения
global state. Это не фильтр и не скрытый fullscreen blit. Geometry использует
матрицы Unity; Present восстанавливает camera matrices. Не утверждается полное
отсутствие global state: matrices нужны существующим mesh shaders.

Реальный Editor GPU:6 world comparisons maxdelta0,8..11 raster nodes и0..1
copy transfer; каждый node учтён. Дополнительно намеренно испорчены глобальные
filter uniforms: изображение то же, глобальные значения остаются неизменными.
12 atlas/retention comparisons PASS:10 exact,2 с alpha/premultiplied delta1/255
(raw unpremultiplied RGB max76/255 у почти прозрачного края). Прогретые25 mask
cache hits,0 blur passes/0 transient RT. Не выдано за универсальный maxdelta0.

После финальных фильтров actual pointer53, modal420/270frames и полный routed
touch tutorial261 checks PASS; run-data JSON roundtrip114 PASS, Console новых
errors0. Исходные10 prefs восстановлены. Backup/restore helpers теперь также
сохраняют исходные background settings, не принуждают их в false.
Core12462/parity47389/Node272 PASS;341+6 C# пять конфигураций чисто.
Android FPS/GC и white pause пока не проверены. Общая приёмка IN PROGRESS.
