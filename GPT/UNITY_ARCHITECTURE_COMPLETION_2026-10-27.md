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
