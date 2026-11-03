# Enemy dynamics, bosses and sphere arrangement — 2026-11-04

Baseline unity-migration de65798a0596e4fe24f4cfa0e962952b4a6987dc; Unity6000.6.1f1.
Scope: approved attached brief (regional attacks/summoners, movement tuning, transient drag freeze). No device connection or device save changes permitted in this cycle. No manual targeting, quality reduction or new rewards.

## Stage: arrangement simulation gate
Actual native drag state now gates the authoritative StepGameplay before any phase or game-time advance. IsGameplayPaused/IsPresentationPaused expose that state without setting ordinary pause or Time Stop. Tap/joystick/Follow do not activate it. Preview and UI remain unscaled. Active ability/dash buttons are blocked while arranging. Desktop fallback input is processed before the simulation gate so PointerUp still works.
PointerCancel previously called the same commit routine as PointerUp; now it aborts. Lost capture, screen/end/pause/focus transitions and inactive/destroyed spheres clear transient state. One sphere/placement gesture owns arrangement at a time; an independent joystick cannot latch the freeze. Restore cancels input; transient gesture fields remain excluded from snapshot.

Actual isolated Play acceptance20 checks PASS:120 blocked StepGameplay calls, unchanged time/core/cooldowns/Time Stop, cancellation without removal, lost capture, new placement, multitouch+joystick, deactivation, ordinary pause, tutorial focus loss and menu/end transitions. This does not yet constitute physical Android gesture acceptance or full network-preview/commit interaction acceptance. Existing five real-API compile profiles and272Node tests pass. Core259533assertions PASS. Full task remains OPEN.

## Baseline combat experiment
Added Threat entry to existing real-C# combat arena tool.1080actual-spawn arenas:6regions, times0/900/1700 (explicit wave=stage/20+1),6seeds,5player field locations, StandardI/VII, up to20s at60Hz. Observed12roles; this is not a full-wave, all-role or survival trial. Raw baseline is in ignored Library/combat-balance/threat/before.json. Ranged cases/fired/died before firing: Basin108/91/17; fleet180/159/21; brood72/60/12; crystal72/66/6; storm36/35/1; null36/34/2. Censored enemies alive at20s are not deaths. Later source tuning needs matched rerun. No performance or subjective-fun claim.

Initial arrangement CI passed both typechecks; its preflight expected the old exact IsPresentationPaused expression and rejected the additional arrangement pause. Updated that assertion to explicitly require the new arrangement term, retaining the main-menu/ordinary-pause guards.
