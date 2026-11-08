# ECHO SPHERE — illustrated archive, 2026-11-08

Baseline: `unity-migration` / `65a3f28fdc7ae58b2ea06c18b2c4a875556a65e9`. Unity `6000.6.1f1`; CLI beta.15 retained as requested. `new-desing` untouched.

## Delivered scope

188 individual generated narrative pictures: all 178 reachable bestiary identities and all 10 characters. Regional bestiary counts: Basin27, Rift32, Bastion31, Folds28, Storm29, Null Archive31. The second region is the Kern fleet, not the organic Basin creatures. [Fleet overview](VisualReferences/ArchiveIllustrations-2026-11-08/region-2.jpg).

Direction follows the user's corrections: realistic cinematic creature anatomy and materials; game silhouettes/colors are resemblance references rather than literal primitive anatomy. Each scene is staged from its existing individual/region lore with varied framing and activity. No elite rings, plus signs, targeting indicators or other gameplay annotations. One unrelated background human in Omission's picture was removed after review. Character pictures retain spherical Core identities and personal-story environments. Lore text, combat appearances and game rules are unchanged.

The new images appear ONLY when a discovered enemy/boss/character entry is opened. List thumbnails retain the production game appearance. Locked discovery conditions are unchanged. Sphere, ability and artifact detail views keep their existing content.

## Integration and ownership

`NativeArchiveDetailArtwork` requests a single detail image asynchronously through Unity Resources. A version guard prevents obsolete completions replacing the current entry; closing, switching, hiding or destroying the owner clears its Image and unloads its Texture2D. A rapid A/B/A reopen protects the outstanding request for the current path. There is no preload of all188 pictures.

Detail art reserves a responsive3:2 box before loading. Assets are delivered as opaque768x512 JPEGs (24,789,662 source bytes in total). Import policy: default Texture2D, sRGB, no mipmaps or CPU-readable copy, clamp/bilinear, Android ASTC6x6. Full generation masters stay outside tracked production assets. All188 masters were1536x1024; delivery does not crop their composition. `catalog.json` records exact file hashes and lore association.

## Verification

- Actual Editor compile: up to date, no compile errors.
- Read-only asset/catalog/import check: PASS1128 assertions for188 unique files.
- Actual attached UI/GPU captures: RU enemy and EN Hunter detail,420px width;20 rapid pending A/B switches per scenario; current-entry image identity, aspect/bounds, wrapped text, close and locked-entry guard PASS. Fixtures do not replace player saves.
- Visual review: all seven overview sheets plus representative full-size scenes and actual detail screenshots. This is a technical/editorial review, not the user's final art acceptance.
- Project preflight: PASS389 runtime C# sources. No gameplay/Core implementation changes.
- Console raw count after tests:1 collapsed Pipeline compatibility error for `editor.playmode`; CLI completed via the existing `editor_play` fallback. No archive/runtime exception found. Console was not cleared; do not describe it as empty.

CI and APK delivery are recorded after the source commit. Android visual acceptance/performance have not been measured in this artwork cycle. No claim of gameplay/performance equivalence is derived from CI.

## Continue

Use the current `unity-migration` HEAD and this report. Complete/read the delivery entry, then inspect illustrated discovered entries on Android, including the32 fleet records. Keep battle models, mechanics, discovery conditions and save format unchanged.
