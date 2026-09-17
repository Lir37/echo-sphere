# Echo Sphere — Progression Design

## 1. Core principle

Echo Sphere progression is built around the player's **combat construction** rather than generic character stat inflation.

- The player does not normally attack directly.
- Towers/spheres are the primary persistent source of damage.
- Abilities support, alter or temporarily overcharge the sphere network.
- Character identity determines which tower families and abilities are more likely to appear, but does not hard-lock the build.
- A run should produce a build with a clear identity and meaningful trade-offs.

The system is inspired by the *structure* of games such as Magic Survival — levelled weapons, major evolutions and restricted combinations — while using Echo Sphere's own sphere/network mechanics and origami presentation.

## 2. Tower progression

Each tower has 7 levels. Levels 4 and 7 are major evolutions.

Every upgrade belongs to the selected tower only. There are no generic "+20% damage to all towers" upgrades in the main tower progression.

### Standard — reliable emitter

| Level | Upgrade |
|---|---|
| I | +15% damage |
| II | +1 pierce |
| III | +15% attack speed |
| IV | **Evolution: Resonant Emitter** — projectile becomes a stronger folded bolt; kills briefly accelerate nearby Standard towers |
| V | +25% damage |
| VI | Every third shot fires a secondary echo bolt at a nearby enemy |
| VII | **Evolution: Echo Core** — shots create short-lived resonance pulses; pulses can relay through nearby allied spheres |

### Sniper — long-range precision

| Level | Upgrade |
|---|---|
| I | +25% range |
| II | +35% projectile damage |
| III | -20% attack delay |
| IV | **Evolution: Paper Lance** — larger folded projectile, strong single-target hit and elite/boss bonus |
| V | +50% critical damage |
| VI | Projectile gains +1 pierce against marked/elite targets |
| VII | **Evolution: Horizon Eye** — periodically charges a penetrating shot that travels through the entire visible lane |

### Shotgun — close-range burst

| Level | Upgrade |
|---|---|
| I | +1 projectile |
| II | +20% damage per projectile |
| III | +20% attack speed |
| IV | **Evolution: Scatter Fan** — wider origami fan, short-range knockback |
| V | +1 projectile |
| VI | Central projectile deals +60% damage |
| VII | **Evolution: Paper Tempest** — volleys leave cutting paper fragments that damage enemies crossing them |

### Chain — network / chaining

| Level | Upgrade |
|---|---|
| I | +1 chain target |
| II | +15% chain damage |
| III | +20% chain range |
| IV | **Evolution: Conductor** — chain jumps can originate from another nearby allied sphere |
| V | +1 chain target |
| VI | Each successful jump reduces the next attack delay slightly |
| VII | **Evolution: Resonance Network** — chains can propagate through linked spheres and temporarily create a damage network between them |

### Aura — area support

| Level | Upgrade |
|---|---|
| I | +15% aura radius |
| II | +15% aura damage |
| III | +15% status duration for enemies inside aura |
| IV | **Evolution: Amplifier** — aura also boosts allied spheres inside it |
| V | +20% aura radius |
| VI | Enemies entering the aura receive a short vulnerability debuff |
| VII | **Evolution: Resonant Field** — aura periodically emits a pulse that applies its current effects to the whole local sphere network |

## 3. Character priorities

Character preferences are weighted priorities, not restrictions.

Suggested high-priority tower families:

| Character | Primary | Secondary | Identity |
|---|---|---|---|
| Spherist | Standard | Chain | quantity / resonance |
| Hunter | Sniper | Chain | focus / marked targets |
| Engineer | Aura | Standard / Chain | linked network |
| Berserker | Shotgun | Standard | close-range risk |
| Alchemist | Aura | Chain | status reactions |
| Architect | Standard | Sniper / Aura | formations / geometry |

The upgrade generator should use these priorities to increase offer weight, not guarantee the tower.

Example weighting:

- primary tower: x2.5
- secondary tower: x1.8
- neutral tower: x1.0
- already heavily invested tower: additional anti-duplicate weighting may be applied so the player is not forced into one tower every run.

## 4. Tower upgrade selection

At normal level-ups, the player receives 3 offers.

Offers can be:

1. a new tower;
2. an upgrade to an existing tower;
3. an ability upgrade;
4. a rare synergy/evolution opportunity when requirements are satisfied.

For an existing tower, the next level is selected from that tower's progression rather than from a global stat pool.

The exact implementation should avoid showing more than 3 cards on mobile.

## 5. Ability progression

Abilities move to the same 7-level model.

The important change is that abilities stop being mostly generic stat sticks. Their later levels should progressively change how the ability interacts with the sphere system.

### Example: Echo Pulse (replacement concept for Blast Wave)

| Level | Upgrade |
|---|---|
| I | +25% pulse damage |
| II | +20% radius |
| III | -15% cooldown |
| IV | **Evolution: Relay Pulse** — pulse travels through nearby allied spheres |
| V | +1 relay jump |
| VI | Relayed pulses deal +30% damage |
| VII | **Evolution: Resonant Cataclysm** — every linked sphere emits a synchronized pulse |

### Example: Sphere Barrier (replacement concept for Shield)

| Level | Upgrade |
|---|---|
| I | +1 absorbed hit |
| II | +20% duration |
| III | +15% radius |
| IV | **Evolution: Linked Barrier** — nearby spheres contribute protection |
| V | +1 linked sphere |
| VI | Blocking damage releases a small echo shock |
| VII | **Evolution: Fortress Network** — linked spheres form a temporary defensive circuit |

### Ability design rule

For every important ability, progression should answer three questions:

- What does this ability do at levels I–III?
- How does its level-IV evolution change the play pattern?
- How does its level-VII evolution create a build-defining mechanic?

## 6. Tower + ability synergy

A level-7 tower and a level-7 compatible ability can unlock a special combination.

This is deliberately **not** a universal matrix. Each tower should have only a small number of compatible abilities.

Example pairs:

| Tower | Ability | Synergy concept |
|---|---|---|
| Standard VII | Echo Pulse VII | **Resonant Core** — Standard shots and pulses amplify each other |
| Sniper VII | Hunter/Mark ability VII | **Execution Line** — marked targets create a piercing execution shot |
| Shotgun VII | Overload ability VII | **Paper Tempest** — close-range volleys trigger radial bursts |
| Chain VII | Lightning VII | **Storm Circuit** — lightning follows the tower network |
| Aura VII | Slow/Time Freeze VII | **Still Field** — enemies inside the field become progressively locked |
| Aura VII | Status ability VII | **Catalyst Field** — status combinations can propagate through the aura |

The final game should target roughly **2–3 meaningful synergy partners per tower**, not dozens.

## 7. Synergy requirements

A synergy requires:

- tower level = 7;
- ability level = 7;
- both components belong to the same run;
- the pair is explicitly compatible;
- optional character-specific modifier may unlock an enhanced variant.

Once unlocked, the two components should become one build element rather than simply granting another passive percentage.

## 8. Character-specific interaction with progression

Characters should influence progression in three layers:

### Layer A — offer weighting

Preferred towers and abilities appear more often.

### Layer B — unique interaction

The character mechanic changes how the preferred tower behaves.

Examples:

- Spherist + Standard/Chain: more nearby spheres means stronger resonance.
- Hunter + Sniper/Chain: marked targets create additional chain/precision effects.
- Engineer + Aura/Chain: network links become physically meaningful.
- Berserker + Shotgun: close-range damage scales with missing HP.
- Alchemist + Aura/Chain: status reactions propagate through the network.
- Architect + Standard/Sniper/Aura: tower geometry unlocks formation bonuses.

### Layer C — mastery

Existing character mastery 1–5 remains a separate meta/progression layer. It should enhance the character mechanic rather than replace tower progression.

## 9. Visual progression

Tower level must be visible without opening a menu.

Suggested visual milestones:

- Levels I–III: same core origami silhouette, increasingly complex folds/details.
- Level IV: unmistakable visual evolution — new silhouette, projectile/effect, animation.
- Levels V–VI: add secondary folds, moving pieces and stronger effects.
- Level VII: second unmistakable evolution — new silhouette, new effect language and a unique combat animation.

The visual language should remain paper/origami rather than turning into generic fantasy magic.

## 10. Technical model

Do not hard-code progression into the game loop.

Introduce data-driven definitions along these lines:

```ts
export type TowerUpgradeKind =
  | 'stat'
  | 'behavior'
  | 'evolution';

export interface TowerUpgradeDef {
  tower: SphereType;
  level: 1 | 2 | 3 | 4 | 5 | 6 | 7;
  kind: TowerUpgradeKind;
  id: string;
  name: LocalizedText;
  description: LocalizedText;
  effects: TowerEffect[];
}
```

The runtime should track tower progression independently for every placed tower type. A Standard tower at level 4 and a Chain tower at level 2 are separate progression states.

The offer generator should operate on these definitions and character priority weights.

## 11. Important implementation constraint

Do not delete the existing combat systems until the new progression model has a working adapter.

Recommended migration order:

1. Add data structures for 7-level tower progression.
2. Add data structures for 7-level ability progression.
3. Add character tower-priority weights.
4. Add progression state to runtime/save state.
5. Replace generic tower upgrades in the level-up generator.
6. Implement Standard and Chain completely as the first vertical slice.
7. Implement level-IV and level-VII visual evolution for those two.
8. Implement one complete tower+ability synergy.
9. Migrate remaining towers.
10. Migrate remaining abilities and remove obsolete generic progression.

The goal is to keep the game playable after every migration step.
