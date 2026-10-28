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
