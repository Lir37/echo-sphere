# Unity 6.6 Editor acceptance and preference compatibility — 2026-10-26

Branch unity-migration; starting HEAD 722a0f3de0fe05cfb540393225802594d8b03a79,
CI #867 SUCCESS. Actual main Editor 6000.6.1f1, Android target, DX12/Gamma.
Read with the three mandatory gameplay documents and modernization progress.

## Confirmed defect and correction

The old IMGUI settings screen stores echosphere_interface_scale with SetFloat;
native settings store the same key with SetString. NativeInterfaceScale only read
strings. An actual PlayerPrefs fixture with a legacy float 1.25 resolved to 0.75;
the native string "1.25" correctly resolved to 1.25. The native reader now honors
the existing float representation without rewriting it. Native string parsing,
absent/empty behavior, non-finite fallback and the 0.75–1.25 limits are preserved.
Fifteen actual Editor storage/reader cases pass, including read-only checks.

The diagnostic backup helper also assumed that scale was always a float. It now
distinguishes the Windows DWORD float from native binary UTF-8 string storage.
Actual string and float backup/restore round trips both pass. Backup/restore
helpers reject a different product namespace, protecting the separate evaluation
copy from being confused with the main game profile.

No gameplay rules, formulas, RNG, save payload schema, artwork or quality changed.

## Actual Editor checks

- Populated upgrade/reward, six tutorial states and result: nine stages / 270
  frames / 420 assertions pass at both 1260x542 and portrait 480x800. Three real
  catalog cards, hover callbacks, layout and hidden-overlay picking checked;
  these fixtures freeze simulation and restore injected lists/flags.
- Portrait production pointer matrix: right/left layouts at 75%, 100%, 125%,
  six cases / 318 assertions. Four joystick directions, release cleanup, all
  sphere selectors, pause interception and tutorial movement cutout pass.
- Public Touch packet event path also passes 53 assertions at 1186x542, using a
  non-mouse pointer ID. The public factory takes already-normalized panel
  coordinates; the diagnostic was corrected to avoid a second Y inversion.
  Gesture cleanup now releases all test pointer identities. Hardware touch and
  simultaneous multiple fingers are not claimed as accepted.
- Real tutorial run at 480x800: actual gameplay Update, Up/Down movement,
  placement and drag through picked controls, real generated upgrade choice,
  completion and pause pass. Tap and drag commit match the intended world point
  within 0.0001 world units. The initial tower blocks a straight Up route; the
  scenario verifies Up then moves Down into free space. Collision rules unchanged.
- Extended run on left-handed 125% layout passes 233 assertions: full tutorial,
  pause/resume, spawn/collect a real Stella cache, choose its generated reward,
  repeat input without duplicate grant, apply damage through the actual runtime,
  death/result, explicit Return to region screen, then Back to home. No unsolicited
  menu/region transition during the run. Spawn/damage are diagnostic stimuli;
  this is not natural combat, distribution or long-run performance acceptance.
- Real reward badge opens set details; Close returns to the unconsumed reward:
  13 assertions. Parent reward callback is retained, not replaced by a fake;
  owned artifacts and pending choices stay unchanged. Screenshot visually checked.
- Console ground truth after checks: 0 errors / 0 warnings. Startup deprecation
  notices remain separate. C# checks against pinned Linux 6.6 APIs: all three
  configurations 0 errors / 0 compiler warnings.

The checks live in scripts/check-unity-native-{tutorial-flow,interface-scale,
production-pointer,populated-modal-lifecycle,reward-set-detail}.cs. New Console
errors are counted through public Application.logMessageReceived, without private
Unity Console reflection. These interactive checks are separate from CI execution.

## Preservation and remaining work

All ten original preference/save keys restored and verified after Stop, including
their original scale representation. User local settings/recovery/experimental
files remain untouched. Original Remote (Not Connected) Game View preset restored.
No APK built, no device capture, FPS, GC or GPU timing reported.

Editor acceptance now covers these presentation/control paths. Remaining device
acceptance includes physical touch/focus interruptions, multitouch, Android GPU
output and extended combat/performance. Earlier transparent-edge AA / arbitrary
transform / Linear-HDR limits remain in the evaluation report. P1/P2 overall are
NOT DONE; do not call CI or these Editor checks Xiaomi 11T performance acceptance.

Continuation: verify current HEAD/CI, read this report and modernization progress.
Native transfer and the checked Editor routes are in place; old/new scale storage
compatibility is fixed. Continue with remaining Android acceptance and measured
performance work, preserving user saves/settings and the agreed APK boundary.
