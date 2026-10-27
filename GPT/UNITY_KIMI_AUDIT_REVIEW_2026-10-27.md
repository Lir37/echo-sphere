# Сверка внешнего аудита с фактическим кодом

## Свежая сборка для последующей проверки

Source 14f319a4e17e9501b20101cfa0d33c7dced03712, CI #884 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40570188552.
APK собран реальным Editor за 328.662 s, 0 errors / 5 warnings.
Файл C:\Users\toxik\Downloads\ECHO-SPHERE-14f319a4-2026-10-27.apk,
232218457 bytes, SHA256
b322baa7236bfe6406fc830a4c945484bc918e2f07bfb33729e6ae4ec4befe6d.
Подпись совпадает с известным предыдущим APK; package и ARM64/ARMv7 проверены.
Телефон отключён, установки/новых device measurements не было.
Содержит исправления портрета, cold pause layout и frame allocations.

Warnings: отсутствует RuntimePipelineConfig инструмента Pipeline; большой
Core static initializer вынесен IL2CPP в отдельный cpp; 3 shader warnings
SourceUiSurfaceFilters Blur (GLES3/Vulkan/DX12). Shader уже явно инициализирует
result/total; причина warnings не установлена. Он используется также в native
menu artwork, поэтому считать его неиспользуемым только по имени Source нельзя.
Эти warnings не выданы за причину белой панели/тормозов.
Изменённые сборкой QualitySettings восстановлены до точных pre-build bytes;
два tracked configure_fingerprint.bin из пользовательского .utmp коммита
сохранены в evidence и восстановлены до pre-build Git версии. Пользовательский
коммит a50bbbf4 и его содержимое не удалялись. Core 1383 / parity 47389 PASS.

Проверена unity-migration после пользовательского коммита a50bbbf4
(новые изменения/артефакты не откатывались). Внешний аудитор явно не прочитал
NativeWorldSurface, transient pools и настройки; его гипотезы не являются замерами.

| Тезис | Фактическое состояние |
|---|---|
| Generated Visual/динамический V работают только в Editor | Подтверждено; Android retirement/typecheck проверяет исключение старого слоя. |
| Мир рисуется Canvas/UGUI с rebuild на каждый примитив | Не соответствует NativeWorldSurface: собственный ordered mesh compositor, CommandBuffer/URP RenderGraph, пул Batch/State, отсечение, retained shadow cache, DrawMeshInstanced для последовательных совместимых одинаковых mesh. Бatching не всеобщий; количество draw calls нужно измерять по сценариям. |
| В боевом кадре есть повторные аллокации | Подтверждены SetDash(new[]), LINQ Count, Regex.Split/List/строки баннера. Исправления ниже. |
| Текст полностью перестраивается каждый кадр | Частично: NativeWorldLabel переиспользует mesh/списки, dirty по тексту/размеру/шрифту/baseline и atlas rebuild. Feedback использует пул labels. Динамический текст и смена слотов остаются кандидатами на нагрузку; баннер Measure ранее повторялся. |
| Пул надо проверить | NativeTransientPools использует Unity ObjectPool для projectile/particle с отложенным retirement в LateUpdate, reset и Release. Само наличие пула не доказывает достаточную производительность. |
| 120 FPS вместо 60 | Application.targetFrameRate=60 уже есть в EchoSphereRuntime.Start. Троттлинг не доказан; прежние короткие device-снимки имели thermal status 0. |
| Возможно Mono вместо IL2CPP | Android scriptingBackend=1; bootstrap задаёт IL2CPP, APK содержит ARM64 и ARMv7. На ARM64 телефоне наличие второй ABI не означает исполнение ARMv7. |
| Ненастроенный URP / нужно отключить эффекты | URP asset имеет HDR, pipeline MSAA=1; мир отдельно использует MSAA4. Это разные параметры. Качество/эффекты/разрешение по гипотезе не понижались. |

Утверждения о паре draw calls браузера, тысячах на устройстве и преимуществе
Adreno в 2–3 раза не подтверждены приведённым аудитом. Боевое построение кадра
не вызывается в native main menu, поэтому его LINQ/Regex не объясняют низкий FPS меню.
Приоритет реального CPU/GPU capture и проверки UI/backdrop/overdraw сохраняется.

## Реализованные правки подтверждённых горячих участков

EchoSphereNativeFrame: неизменяемый массив штрихов переиспользуется; Count
заменён последовательным обходом с тем же условием IsAlive; разбиение строк и
геометрия баннера кэшируются по message/width/fontSize. Прежние Regex, порядок
слов, обрезка хвоста до 34 UTF-16 code units, размеры, fade/цвет/время сохранены.
Кэш не меняет gameplay/RNG и не отключает эффекты.

Editor, реальные Font/Measure: 50 комбинаций Unicode/пустых/многострочных/длинных
сообщений и пяти ширин совпали с прежним алгоритмом по строкам и размерам.
ProfilerRecorder GC.Alloc, current thread: 10000 cache hits — 0 bytes;
1000 вызовов прежнего алгоритма — 761300 bytes. Это локальная синхронная проба,
не GC кадра/Android и не измеренный прирост FPS. GetAllocatedBytesForCurrentThread
в этой Unity возвращал 0 даже для контрольного алгоритма и исключён из измерений.
C# player/editor 351 runtime + 6 Editor, 0 errors/warnings. Phone disconnected;
белая панель Android и низкая производительность остаются открытыми.
Доказательства: Documents/UnityEvaluations/ECHO-SPHERE-pause-20261027-182821/banner-parity.json.
