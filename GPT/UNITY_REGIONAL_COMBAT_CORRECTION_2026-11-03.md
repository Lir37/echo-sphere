# ECHO SPHERE — regional combat and sphere rendering correction, 2026-11-03

Baseline: unity-migration d0516a9be01aac7cf270a2600a1b628f76002c1b.

User observations: fleet ships ram, region-one design changed without request, attacks are inconspicuous, placing a sphere slows combat, player-centred arena glow unwanted.

Implemented:
- Fleet profiles now give every regular hull an onboard weapon: short-range scouts, orbiting fighters, holding heavy hulls/snipers and spread volleys. Ranges 60–105 source pixels remain inside base standard sphere range. Separation cannot push a holding fleet ship toward the player; no retreat is introduced.
- Region-one bodies remain on the existing organic renderer. Removed generic added arms/emitter/ray overlay from region one. No new redesign of those enemies.
- Brood corruptors launch visible webs that settle into a bounded single-hit trap; spores retain their existing corrosive patch. Web drawing has six radial strands and concentric rings. Crystal regular shooters use three fragments; energy/void shooters retain distinct paired projectiles. Preparation for the changed ranged attacks is 0.5 seconds (fleet sniper 0.65); melee roles outside the fleet remain melee. Existing attack poses and muzzle feedback remain active.
- Removed the player-centred full-arena gradient. World-fixed nebulae, stars and region geometry remain.
- Standard sphere core no longer tessellates its pulsating disc each frame: a retained detailed mesh and shader coordinate scale preserve the authored gradient.
- Standard-family ring/arc Gaussian glow is prepared once per retained rig at full world density; rotation/translation/opacity reuse the premultiplied texture. Lost RT is regenerated; rig owns and releases the surface, textures and mesh. No MSAA/resolution/effect-count reduction.

Validation:
- Core contracts: 259533 assertions PASS. Node: 272 tests PASS. Unity preflight and visual provenance PASS.
- Unity compile and clean Play Console: 0 errors, 0 warnings after fresh Play start.
- Actual regional movement/snapshot checks: 159 PASS; actual attacks: 73 PASS including seven ordinary fleet roles firing and web settling; independent projectile flight: 35 PASS; faction render fixture 96 samples PASS.
- Retained sphere GPU check: 120 animated frames, zero repeated sphere shadow passes; comparison with old ring path mean RGB difference 0.150081/255 across the full image, max per-pixel RGB-sum difference 135. This is approximate visual equivalence, not bitwise identity. Local screenshots inspected.
- Release APK local BuildPipeline Succeeded; IL2CPP; regions open through ECHO_REGION_TEST_ACCESS, save unlock flags unchanged.
- APK: UnityProject/Builds/Android/ECHO-SPHERE-regional-combat-fix.apk, 57243546 bytes, SHA256 8f3a4bc08f46e5d18d4fb277daaab4b3567b4ba858bb3c51e9f3c0cdb4e29ad4.
- adb install -r: Success. Device profile freshly backed up privately before interaction. New APK gameplay/performance acceptance pending screen unlock. No FPS claim for this correction yet.

Remaining: confirm fleet shooting and traps in natural phone combat; measure sphere-placement cost in matched scene; confirm removal of requested glow and pause/HUD re-entry. Do not replace future player progress using an older profile backup.
