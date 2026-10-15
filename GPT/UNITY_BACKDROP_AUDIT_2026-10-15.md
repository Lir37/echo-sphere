# Безопасное размытие подложки панелей

Последующий общий аудит и исправления коробок, clipping и трансформаций:
[2026-10-16](UNITY_NATIVE_VISUAL_REAUDIT_2026-10-16.md).

Источник: `new-desing`, `737ef2af4a877e0995db5f92331d536d2f29a934`.
Начальное состояние `unity-migration`: `1f370ef`.
Unity Editor, Unity ShaderCompiler и Unity/Android-сборки не запускались.

## Найдено

В `SourceUiDocument.UpdateLayout` размытие было полностью отключено после
ручного обнаружения ошибок D3D12 render pass. Старый `SourceUiBackdrop`
захватывал `CurrentActive` и отправлял команды blit из `ImmediateRepaint`.
Повторный захват предыдущего кадра также непригоден: в подложку попадали бы
сама панель и её надписи, с задержкой и накоплением изображения.

Полный обход исходных CSS/TSX-эталонов обнаружил не только 6/8/12 px из
`conceptStyle.css`, но и 4 px из используемых Tailwind-классов: 828 поверхностей
на 228 вариантах экран/viewport, максимум 13 в одном варианте. Исходные
объявления, их каскад и `none` сохранены.

## Исправление

`SourceUiBackdrop` теперь строит retained-подложку из подготовленных элементов,
которые находятся перед панелью в реальном порядке рисования UI Toolkit:

1. Выбирается ближайший CSS Backdrop Root. Его внешняя opacity/filter не
   применяется второй раз. Transform, z-index и fixed сами по себе не
   обрывают подложку.
2. Копируется только префикс дерева до панели. Текст, SVG, Canvas игрового мира,
   прокрутка, ранее нарисованные панели и готовые 3D-поверхности используют те же
   ресурсы. Собственные фон, текст и потомки панели в этот префикс не входят.
3. Два штатных `PostProcessingPass` UI Toolkit выполняют Gaussian blur в
   экранных CSS-координатах при исходном DPR. На границе преобразованного border
   quad используется mirror, как у браузерного `backdrop-filter`, затем
   применяется rounded border clip. Альфа обрабатывается premultiplied, цвета —
   с учётом sRGB и `FilterPassContext`.
4. Подложка, собственная кисть/текст и потомки панели остаются внутри одной
   исходной opacity/filter группы. Оригинальные элементы сохранены для layout,
   измерения текста, фокуса и событий. Копии не принимают ввод.

В коде подложки больше нет `ImmediateModeElement`, захвата framebuffer,
`Graphics`/`GL`, собственных RenderTexture, загрузки/применения Texture2D или
`CommandBuffer`. Границами GPU-проходов и промежуточными текстурами управляет
UI Toolkit. Подготовка всех существующих кистей, SVG и depth-поверхностей
по-прежнему выполняется до отрисовки UI. Части Slider также загружаются при
подготовке, а не при генерации mesh.

Дополнительно исправлена передача параметров существующих CSS-фильтров:
официальный `UIRRenderTreeCompositor` Unity 6.3 вызывает `applySettingsCallback`
**вместо** обработки `parameterBindings`. Радиус нового blur и параметры
brightness/contrast/saturate/drop-shadow теперь задаются явно. Цвет drop-shadow
передаётся как исходный sRGB-вектор без повторного преобразования.
Для `forceGammaRendering` промежуточные проходы сохраняют gamma; линейный
вывод включается только штатным `_UIE_OUTPUT_LINEAR` для последнего прохода.
Преобразуется сам premultiplied RGB, чтобы запись/чтение sRGB RenderTexture
не меняли его значения и не создавали ореол на прозрачных краях.

Опорный API проверен по официальным исходникам Unity:
[UIRRenderTreeCompositor](https://github.com/Unity-Technologies/UnityCsReference/blob/6000.3/Modules/UIElements/Core/Renderer/UIRRenderTreeCompositor.cs),
[FilterFunctionDefinition](https://github.com/Unity-Technologies/UnityCsReference/blob/6000.3/Modules/UIElements/Core/Style/FilterFunctionDefinition.cs).
Правила вывода цвета: [Custom filter and force gamma considerations](https://docs.unity3d.com/6000.3/Documentation/Manual/ui-systems/custom-filters.html).
Порядок композиции сверялся с [CSS Filter Effects 2](https://drafts.csswg.org/filter-effects-2/).

## Android и ресурсы

Разрешение, радиусы и число слоёв не уменьшены. Область фильтра ограничена
панелью и полным 4σ запасом; отдельный полноэкранный захват для каждой кнопки
не создаётся. Копии retained-элементов и делегаты переиспользуются, кисти/шрифты/
SVG/Canvas-текстуры разделяются с исходным UI. Подложки предыдущих панелей вне
области выборки не копируются. Два симметричных отсчёта используют один расчёт
веса Gaussian. Удалены старые capture/mask/blur RenderTexture и их жизненный цикл.
Это свойства реализации, а не измерение FPS/памяти на Android.

## Проверки

| Проверка | Результат |
| --- | --- |
| Настоящие DLL Unity 6000.3.24f1, Android player | 191 C#-файл, 0 ошибок, 0 предупреждений |
| Android runtime при `UNITY_EDITOR` | 191 C#-файл, 0 ошибок, 0 предупреждений |
| Отдельная Editor assembly | 3 C#-файла, 0 ошибок, 0 предупреждений |
| TypeScript app и node | Оба typecheck проходят |
| Порядок подложки / Backdrop Root | Сам элемент, его потомки и последующие слои исключены; группы сохранены |
| Affine mapping | 5000 матриц, включая shear, reflection и вырожденные случаи |
| Независимые Chromium backdrop-filter пиксели | 24 случая, 321 024 пикселя; 4/6/8/12 px, DPR 1/1.75/3, непрозрачная и прозрачная подложки; худшая средняя ошибка 0,871/255 |
| Core | 1193 assertions, 47 339 сравнений с TypeScript |
| Полные визуальные регрессии | 2663 потока команд, 76 деревьев UI, 674 640 CSS-свойств, геометрия/Grid, 189 keyframes, 2682 интерполяции, 60 transition-кадров, 99 кистей, Canvas-тени/изображения, 68 кадров событий |
| Node-регрессии | 234 теста, 0 ошибок, включая запрет capture/upload в UI repaint и правила вывода цвета |
| Preflight / generated source / metadata / provenance | Проходят; исходные `src/`, `public/`, package/lock и tsconfig не изменены |

Браузерные эталоны воспроизводит `scripts/export-unity-backdrop-fixtures.cjs`.
Проверка `--backdrop-only` входит также в обычный запуск VisualChecks и CI.
Пиксельный тест проверяет математику фильтра и alpha-композицию на CPU;
побитового совпадения с Skia он не утверждает.

## Границы результата

Отключённый путь blur восстановлен нативной retained-композицией, а прежний
опасный захват удалён. Проверки типов и автотесты проходят. Не выполнялись
импорт Unity, компиляция шейдеров его компилятором, работа UI Toolkit на GPU,
IL2CPP или запуск на Android. Поэтому отсутствие всех фактических render-pass
предупреждений и итоговое отображение в Unity требуют ручной проверки на
компьютере с Editor; это не заменяется C# typecheck или браузерными эталонами.
