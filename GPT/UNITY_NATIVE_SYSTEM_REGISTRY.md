# Реестр действующих систем Unity — 2026-10-28

Ветка unity-migration; журнал реализации и проверок:
UNITY_ARCHITECTURE_COMPLETION_2026-10-27.md. Это реестр владельцев, не акт Android
приёмки. Не добавлять контент/механики из будущих разделов Blueprint.

| Система | Действующий владелец / путь | Граница |
|---|---|---|
| Игровые правила | Core + EchoSphereRuntime + simulation agents | Прежние фазы, dt, RNG; не Physics2D/FixedUpdate |
| Контент | GameCatalog, NativeRunCatalog, NativeArchiveCatalog, существующие SO | Не вводить второй редактируемый баланс |
| Сцены/переходы | NativeBattleSceneRoot, native run commands, async selected-region launch | Pending команда принимается один раз |
| UI | NativeRunDocument + Native* screen/presenter classes | UI Toolkit, retained элементы; не JSX/DOM interpreter |
| Персонажи UI | NativeCharacterScreen + portrait leases | Лёгкие10 карточек; PNG1254 из того же contour body, async/viewport leases |
| Тело игрока | NativeWorldPlayer + NativeCharacterContours | 10 retained contour rigs с цветными вставками, без sprite тела/movement mirror; слабый presentation-only trail; радиус/статусы прежние |
| Стекло | NativeMenuGlass / NativeEngineBackdrop, Unity6.6 backdrop filters | Живое содержимое; hidden owners не фильтруют |
| Мир | NativeWorldPresentation (scene owner) | Lazy resources, BeginFrame, ReleaseResources/OnDestroy |
| Порядок кадра | EchoSphereNativeFrame typed inputs | Сохраняет исходный прозрачный порядок слоёв |
| GPU | NativeWorldSurface + NativeWorldRenderGraphFeature | Unity Mesh/Material, raster passes; не JS Canvas sink |
| Тени | NativeWorldShadowCache + ShadowAtlas | Local masks, retained immutable masks, isolated atlas cells |
| Копии масок | Exact CopyTexture transfer | Единственный Unsafe RG path; не blur, без global uniforms |
| Существа | NativeWorldCreatures entity ViewResources | Owner-bound mutable geometry/labels, shared immutable art |
| Сферы/игрок/сеть/POI | NativeWorld* views + model projection | Renderer не хранит игровые данные |
| Боевые части всех10семейств | NativeWorldSphereWeapons + Additional, owner NativeWorldPresentation | 130 retained rigs; rigid pose от PresentationShotTime/Serial; поле без aim/отдачи; не save state |
| Орбитальные контактные части | NativeWorldSphereFamilies → shared NativeWorldSphereWeapons | Прежние позиции/числа/две встречные орбиты; наружная ось +X, tangential mirror внешнего кольца; без снаряда |
| Тестовый уровень | NativeMobileControl → штатный OpenLevelUpIfReady | Только Editor/Development/ECHO_SPHERE_TEST_CONTROLS; в обычном APK флаг включается явно |
| Частицы/снаряды | NativeTransientPools + существующие agents | Отложенное retirement до LateUpdate; порядок фаз прежний |
| Геометрия частиц | NativeWorldFeedback retained mesh + instance runs | Один diamond, совместимые последовательные draws |
| Цифры урона | Owner SourceDamageNumber + native label pool | Lifetime события, не индекс изменяемого списка |
| Пространственные запросы | Enemy query index / collision scratch | Кандидаты ускорены, итоговый порядок/проверки прежние |
| Сохранения | EchoSphereRunPersistence + RunSaveBuffers + RunSnapshotCodec | Version2/ключи прежние; retained nodes/metadata/envelopes |
| Цвет/variant save | IRunSaveColor / EnemyAgent2D.SourceVisualVariant | Model data, без чтения скрытых Renderer |

## Отдельно включаемые сравнительные средства

V/VisualJs/Visual_* и старые SourceUi*/SourceCanvas* доступны только при
UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS. Scene-preview impact/pulse —
UNITY_EDITOR && ECHO_SPHERE_SCENE_REFERENCE. Обычный Editor и Android эти types
не содержат. VisualChecks .NET явно включает reference flag; UnityTypeChecks
проверяет обычные и reference конфигурации раздельно. Это конфигурации сборки,
не физически выделенный самостоятельный reference plugin.

Не вызываемые private IMGUI helpers прежнего migration UI ещё находятся в
исходниках; OnGUI entry отсутствует. Они не служат вторым активным UI и не
хранят состояние сохранения. Окончательное удаление сравнительных/заменённых
средств отложено до визуальной приёмки, как предусмотрено согласованным планом.

## Удалённые пути и общие ресурсы

EnemyCreatureVisual2D/BossPresentationVfx2D и прежние sphere/projectile/region
порты удалены. XP/health/rune/artifact/drone/enemy/sphere/player больше не рождают
скрытые sprite shells; Orbital collision anchors остаются без графики.
SourceBackdrop2D, его211 потенциальных renderer objects (quad+210stars; bounds
отдельно) и shader удалены после проверки callers, строковых путей и GUID.
Его factory всегда возвращал до создания при UsesSourcePresentation=true;
удаление балласта не выдано за измеренное ускорение.

CanvasArt не является целиком мусором: NativeWorldArtCatalog прямо ссылается
на его player/standard sphere PNG GUID. Эти shared assets сохранены. Имена
SourceCanvasPresent/Shadow у shader ресурсов также не доказывают старый JS-путь:
их использует действующий native compositor. Удалять только по имени нельзя.
Recovery пользователя и прежние артефакты не чистились.

## Открытая приёмка

- Xiaomi11T: diagnostic и обычный IL2CPP в одинаковом сценарии, FPS/GC/память,
  p95/p99,15 минут включая насыщенный бой; пока не выполнено.
- Android visual/white pause и полный набор всех мутаций/боссов в движении.
- После приёмки: физически удалить заменённые reference/IMGUI средства,
  сохранив игровые контрактные проверки; повторить build dependency audit.
- Editor20 tutorial/бой/home циклов и GPU comparisons выполнены; они не
  заменяют перечисленную приёмку. Общий план не помечен DONE.

2026-10-29: UI skins имеют3-frame retirement и generation-based восстановление потерянногоRT;вторичные меню scale0.86DPR. Player comet — собственная retained38-vertex mesh/материал,не игровая частица. Призма primary color больше не берётся из elemental status. См.UNITY_HUD_PROJECTILES_MUTATIONS_2026-10-29.md.

2026-10-29 gameplay: RunEconomyRules/State owns new-run Echo/merchant stock and timers, persists via RunSnapshotCodec;legacy null unchanged. NativeMerchantView owns retained shop UI. NativeWorldFleet owns Rift hulls/rigid parts;NativeWorldCreatures still owns common status/telegraph overlays. Selected Region.Pockets now owns drawn pocket boundaries. See UNITY_GAMEPLAY_COMPLETION_2026-10-29.md.

2026-10-29 gameplay stats: modern pause damage/range/cadence/DPS now read simulation/agent getters rather than the imported duplicate formulas. ArtifactGameplayRules owns new-run effect deltas/conditional node context/descriptions; legacy catalog and old save behavior preserved. See UNITY_GAMEPLAY_COMPLETION_2026-10-29.md.


2026-11-01 artifact campaign ownership:Core ArtifactRewardRules.Catalog(true)90,
Catalog(false)82;CampaignArtifactSets8supplements+12source sets;NativeRunCatalog
GetSets/GetSynergies caches modern projections without mutating imported assets.
EchoSphereArtifactProtocolProcs owns20bounded hit counters in runtime snapshot.
Only positive authored hits enter;legacy null-economy and secondary hits gated.
EnemyAgent2D.ReceiveArtifactProtocolDamage takes already-resolved damage with
owner credit and no recursive procs/double global buff.

2026-11-01 regional extension:Core.CampaignRegions owns four additional profiles,
20pockets and20ecology tables; legacy Basin/Rift definitions retained. Native
catalog projections,RegionFieldVisuals and HUD bind new profile data. EnemyView
RegionStyle selects four retained native anatomies; RegionalGeometry/RegionalEllipse
cache geometry outside hot drawing. Old art dictionary owns meshes;Dispose clears
regional reference caches. Stable IDs0/1 preserved,new IDs2–5 appended.


2026-11-01 point4: Core.CampaignBossRules/State own deterministic campaign boss
patterns and phase,including the phase of an already warned attack.
Runtime.BossCampaignCombat executes existing combat APIs; UsesCampaignCombat
is explicitly null-economy legacy policy. Boss auxiliary snapshot owns all
new state,including locked aim and remaining dash duration/speed.
NativeWorldEnemyEffects draws retained warning geometry from typed state,
without body facing/scale affecting attack coordinates. Legacy path remains
active only for legacy runs. Details: UNITY_ENEMY_BOSS_COMPLETION_2026-11-01.md.


2026-11-01 point5:CampaignRegionContent owns18contracts/18regional objectives
and support ecology;CampaignRegionLore owns30RUENrecords;ConvergenceRules/State
own six-ecology rotation and25/40min/post40boss schedule. Runtime regional
progress and enemy faction tags are model data in existing snapshots. Native
region/archive views project these definitions; mobile and HUD share placement
capacity. RegionFieldVisuals owns persisted monotonic POI IDs, not derived from
the live list. Convergence ecology does not rewrite existing enemy views.


2026-11-01 point6: Core.RunEconomyState owns exploration reservations/cycle,
versioned new-run prices and Echo telemetry. Core.RuneRunState owns temporary
boosts,gravity center,Echo and deferred Cell reward. EnemyAgent2D owns persisted
Rune Hunt expiry,independent of runtime Unity-object references. Cast execution
remains in existing SphereAttackAgent; a rune requests exactly one repeat.
Native pickable geometry owns Cell glyph; existing archive projects10RuneRules
definitions. Old null-economy policy retains legacy behavior. Source catalogs
remain read-only imports; Unity source count361. Details/acceptance limits in
UNITY_ECONOMY_RUNES_COMPLETION_2026-11-01.md.


2026-11-01 point7: Core.MetaCurrencyRules/MetaWallet own three persistent
balances and transaction receipts; Echo remains exclusively RunEconomyState.
Runtime.EchoSphereMetaCurrencies owns versioned PlayerPrefs JSON/cache/backup
and paid-entitlement recovery. EchoSphereMetaProgression owns approved initial
Shard/Data rates, one-time legacy backfill, optional existing-mastery research
and lore research. Known-record grants batch into the existing knowledge save.
Meta wallet/cache/pending discoveries are NonRunFields; only the run receipt
and per-run reward counters belong to run snapshots. Native UI projects wallet
state and expected mastery level; no Renderer is used as financial storage.
Runtime source count364; see UNITY_META_CURRENCIES_COMPLETION_2026-11-01.md.
