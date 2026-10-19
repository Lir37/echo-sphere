# Native main menu production checkpoint

Final lifecycle verification: 2026-10-19; checkpoint filenames retain 2026-10-18.

Entry baseline: `26765b508a15ad3155b62e681af91d505e5cf7e8`, branch `unity-migration`.
Protected source: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`.

Home now selects the native UIDocument by default. Its UXML/USS controls,
VectorImage icons, GPU background, core/orbits, avatar, shadows and glass are
owned by NativeMainMenuScreen/Composition. Cold Home creates neither the source
DOM/CSS document nor the source Canvas. Existing navigation, save/continue,
name persistence, difficulty IDs and character selection remain authoritative.

P1/P2 remain PARTIAL. This is bounded Editor acceptance of the menu migration,
not Android device acceptance or completion of gameplay presentation.

## Confirmed causes and implementation

- Source Home repeatedly projected/layouted/prepared its DOM/CSS presentation.
  Native controls retain their tree and bind model/font/brush changes only.
- A hidden legacy UIDocument retained full-screen filter work even with zero
  opacity or display:none. Disabling the document releases its render chain;
  hooks/timers still tick. Resume initializes the recreated root and reconciles
  retained entries into it, including root pointer callbacks.
- Separate prefix/blur UIDocument targets read an older frame and also exposed
  premultiplied/straight-alpha interchange errors. The accepted image/solid
  prefix and separable Gaussian run directly on the GPU before UI rendering.
  The later Image receives straight RGBA; no previous-frame panel capture is used.
- Text, text-shadow, vectors or other unsupported prefix paints are never omitted.
  Where they overlap the glass, the existing native UI Toolkit retained prefix
  preserves the complete paint. It copies native images/text, not source DOM/CSS.
  That filter is disposed when hidden. Common image-only regions use cropped,
  cached GPU targets without recursive replicas.
- Yoga differs from CSS automatic minimum sizes and nested flex measurement.
  Loadout grid-equivalent rows keep intrinsic height; difficulty options receive
  the remaining row width. Native stage retains the authored
  `height:calc(100% - 50px)` inside its allocated grid track. Those 50 px were a
  real surviving source declaration, not a source-grid projection bug.
- Initial presentation waits for stable layout/paint. Original orbit-dot
  placements and effective character/navigation styles were checked in source.

No gameplay, RNG, formulas, effects or quality settings were changed. No APK.

## Measurements

Actual paired Unity Profiler capture: Windows Editor 6000.3.24f1, Radeon 760M,
Direct3D12, Game View 1260x542, same run/source state, after warmup:

| Path | Valid frames | Mean frame time | Reciprocal FPS | Main-thread GC.Alloc/frame |
| --- | ---: | ---: | ---: | ---: |
| Source Home | 14 | 150.3738 ms | 6.6501 | 1,944,778 B |
| Native Home | 17 | 16.6375 ms | 60.1052 | 2,640.6 B |

Native composition preparation averaged 0.01328 ms. These short Editor samples
include the complete selected composition and its retained-prefix cases. They
are not Android measurements. Earlier intermediate 1,370 B/frame captures used
an earlier composition; use the final table above for this checkpoint.

## Verification

- Unity recompile succeeds with zero compiler errors/warnings.
- Core: 1,193 assertions and 47,339 TypeScript/C# comparisons pass.
- Node: 272 tests pass. Full VisualChecks pass against existing frozen references.
- Project preflight, visual provenance and script/icon metadata checks pass.
- Actual native controls pass nine navigation callbacks, four difficulty IDs,
  native name editing/UTF-16 length limit, silent binding, RU/EN, saved-run
  visibility, retained hierarchy and portrait/landscape restoration.
- Actual runtime buttons and authored Back controls pass all seven destinations
  and Home returns. Six source pages recreate/populate their document; Settings
  uses its existing native owner. Saved preferences are unchanged by the fixture.
- Complete composition captures pass 1260x542@1, 480x854@1, 360x800@1.75 with safe
  insets, 854x480@3, 800x360@3 and 320x568@2. Difficulty/character controls do not
  overlap. Source-authored short-height overflow is retained, not redesigned.
- Native image-prefix phase fixture: 32 changing frames, zero channel difference
  from corresponding fixed frames. Overlapping native-text/retained-prefix
  fixture also passes 32 frames with zero difference from the complete prefix.
- Native GPU glass: all 24 existing frozen references pass at DPR 1/1.75/3,
  sigma 4/6/8/12, opaque/transparent. Existing gate remains mean <2/255 and
  maximum <10/255; measured worst mean 0.5635/255, maximum 4/255. Comparison is
  premultiplied RGBA, matching the render-target representation.
- Image replicas pass 192 Texture2D/VectorImage/Sprite/empty/tint/UV/scaling checks;
  retained subtree visibility passes 360 checks.
- Native artwork geometry/depth/blur/cache/resize/DPR/disposal fixture passes.

Clean Play/Stop resource verification: 78 owned native targets while open, zero
after Stop. 55 earlier hot-reload orphans were released separately; they did not
recur during the clean cycle.

Historical Console entries include diagnostic CLI timeouts and unsupported
in-Play assembly-reload state from earlier experiments. Cold Play Mode is the
acceptance path. Do not call those historical entries a clean Console or claim
that the intermittent texture-index-3 symptom is universally resolved. CI is a
separate gate: verify the resulting HEAD's workflow independently.

Next: typed native gameplay/HUD presentation, beginning with the measured path
construction/tessellation and UI tree costs. Preserve source topology, colors,
shadows, draw order and gameplay. Compare equal active scenarios in the Editor;
keep device parity and P1/P2 completion open until their own gates pass.
