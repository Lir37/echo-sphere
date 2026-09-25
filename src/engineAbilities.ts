import { playSound } from './audio';
import type { AbilityType } from './gameData';
import type { GameState, SphereEntity } from './engineTypes';
import {
  dist, rand, clamp, getNetworkNodes, getAbilityBranchId, getNearestSphere
} from './engineRuntime';
import {
  dealDamageToEnemy, getCooldownMult, getVampirePercent, emitSpherePulse, triggerEngineerRelay
} from './engineCombat';
import { analyzeSphereNetwork, getLinkedNodeIndexes } from './network';
import { getActiveSphereAbilitySynergies } from './sphereProgression';

function activateBlast(s: GameState): void {
  const lvl = s.player.abilities.blast || 0;
  if (lvl === 0 || s.player.blastCooldown > 0) return;
  s.player.blastCooldown = Math.max(8, (30 - (lvl - 1) * 2) * getCooldownMult(s));

  const spheres = s.spheres.filter((sphere) => sphere.alive);
  const baseDamage = 30 + (lvl - 1) * 10;
  const radius = 125 + lvl * 12;
  const branch = getAbilityBranchId(s, 'blast', 4);
  const final = getAbilityBranchId(s, 'blast', 7);

  if (spheres.length === 0) {
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, s.player.pos) <= 180) dealDamageToEnemy(s, e, baseDamage);
    }
  } else {
    let ordered = [...spheres].sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
    if (branch === 'blast_network' || final === 'blast_echo_network' || final === 'blast_infinite_pulse') {
      const networkState = analyzeSphereNetwork(getNetworkNodes(s));
      const orderedNetwork: SphereEntity[] = [];
      const remaining = new Set(ordered);
      let current: SphereEntity | null = ordered[0] ?? null;

      while (current) {
        orderedNetwork.push(current);
        remaining.delete(current);
        const currentIndex = s.spheres.indexOf(current);
        const nextIndex = currentIndex >= 0
          ? getLinkedNodeIndexes(networkState, currentIndex)
            .filter((index) => index >= 0 && index < s.spheres.length)
            .filter((index) => remaining.has(s.spheres[index]))
            .sort((a, b) => dist(s.spheres[a].pos, current!.pos) - dist(s.spheres[b].pos, current!.pos))[0]
          : undefined;
        current = nextIndex === undefined ? null : (s.spheres[nextIndex] ?? null);
      }

      ordered = orderedNetwork.length > 0 ? orderedNetwork : ordered;
    }
    const networkCore = getActiveSphereAbilitySynergies(s).some((link) =>
      link.character === 'engineer' && link.sphere === 'standard' && link.ability === 'blast'
    );
    let strength = 1;
    for (const sphere of ordered) {
      emitSpherePulse(s, sphere, baseDamage * strength, radius, '#c46d3d', final === 'blast_resonant_core');
      if (branch === 'blast_resonance' && sphere.type === 'standard') {
        emitSpherePulse(s, sphere, baseDamage * 0.4, radius * 0.72, '#d4943d');
      }
      if (networkCore && sphere.type === 'standard') triggerEngineerRelay(s, sphere);
      if (branch === 'blast_core') strength *= 1.12;
      if (branch === 'blast_network') strength *= 1.08;
      if (final === 'blast_echo_network') strength *= 1.15;
      if (final === 'blast_resonant_core' && sphere.type === 'standard') {
        emitSpherePulse(s, sphere, baseDamage * 0.45, radius * 0.75, '#d4943d');
      }
    }
    if (final === 'blast_infinite_pulse') {
      for (const sphere of [...ordered].reverse()) {
        emitSpherePulse(s, sphere, baseDamage * 0.35, radius * 0.75, '#d4943d');
      }
    }
  }

  const geometricCore = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'architect' && link.sphere === 'standard' && link.ability === 'blast'
  );
  if (geometricCore) {
    const standards = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'standard');
    for (let i = 1; i < standards.length; i++) {
      s.lightnings.push({ from: { ...standards[i - 1].pos }, to: { ...standards[i].pos }, life: 0.3 });
    }
  }
  if (branch === 'blast_core') {
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, s.player.pos) <= 100) dealDamageToEnemy(s, e, baseDamage * 0.5);
    }
  }
  if (branch === 'blast_resonance') {
    const standard = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'standard');
    for (const sphere of standard) emitSpherePulse(s, sphere, baseDamage * 0.25, 90, '#d4943d');
  }

  s.screenShake = 0.18;
  s.flashText = { text: 'ECHO PULSE', life: 0.9, color: '#c46d3d' };
}


function activateShield(s: GameState): void {
  const lvl = s.player.abilities.shield || 0;
  if (lvl === 0 || s.player.shieldCooldown > 0) return;
  s.player.shieldCooldown = 20 * getCooldownMult(s);
  const nearby = s.spheres.filter((sphere) => sphere.alive && dist(sphere.pos, s.player.pos) <= 260).length;
  const networkBonus = lvl >= 3 ? Math.min(2, Math.floor(nearby / 2)) : lvl >= 2 ? Math.min(1, Math.floor(nearby / 2)) : 0;
  const branch = getAbilityBranchId(s, 'shield', 4);
  const final = getAbilityBranchId(s, 'shield', 7);
  const branchBonus = branch === 'shield_echo_guard' ? Math.min(2, Math.floor(nearby / 2)) : 0;
  const bastionBonus = branch === 'shield_bastion' ? 1 : 0;
  const networkState = final === 'shield_network_guard' ? analyzeSphereNetwork(getNetworkNodes(s)) : null;
  const connectedSphereCount = networkState
    ? s.spheres.reduce((count, sphere, index) => (
      sphere.alive && getLinkedNodeIndexes(networkState, index).some((linked) => linked >= 0 && linked < s.spheres.length)
        ? count + 1
        : count
    ), 0)
    : 0;
  const networkGuardBonus = final === 'shield_network_guard'
    ? Math.min(2, Math.floor(connectedSphereCount / 2))
    : 0;
  const barrierCore = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'berserker' && link.sphere === 'shotgun' && link.ability === 'shield' && link.effect === 'defense'
  );
  s.player.shieldCharges = Math.min(5, 1 + Math.floor((lvl - 1) / 2) + networkBonus + branchBonus + bastionBonus + networkGuardBonus + (barrierCore ? 1 : 0));
  s.player.shieldTimer = 10 + (lvl >= 5 ? 2 : 0);
  if (branch === 'shield_echo_guard' || final === 'shield_network_guard') {
    for (const sphere of s.spheres) {
      if (sphere.alive && dist(sphere.pos, s.player.pos) <= 260) {
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.35);
      }
    }
  }
  if (branch === 'shield_bastion' || final === 'shield_iron_dome' || final === 'shield_resonant_guard') {
    if (branch === 'shield_bastion') s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
    if (final === 'shield_iron_dome') {
      const protectedCount = s.spheres.filter((sphere) => sphere.alive && dist(sphere.pos, s.player.pos) <= 300).length;
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + Math.min(2, protectedCount));
    }
    for (const sphere of s.spheres) {
      if (sphere.alive && dist(sphere.pos, s.player.pos) <= 260) {
        s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.8, maxLife: 0.8, color: '#4a7a8a', size: 5 });
      }
    }
  }
  if (final === 'shield_network_guard') {
    const networkState = analyzeSphereNetwork(getNetworkNodes(s));
    for (const link of networkState.links) {
      if (link.a >= s.spheres.length || link.b >= s.spheres.length) continue;
      const first = s.spheres[link.a];
      const second = s.spheres[link.b];
      if (!first.alive || !second.alive) continue;
      s.lightnings.push({ from: { ...first.pos }, to: { ...second.pos }, life: 0.22 });
    }
  }
  s.flashText = { text: 'SPHERE BARRIER', life: 0.9, color: '#4a7a8a' };
}


function doTeleportTo(s: GameState, target: Vec): void {
  const from = { ...s.player.pos };
  for (let i = 0; i < 16; i++) {
    s.particles.push({ pos: { ...from }, vel: { x: rand(s,-140, 140), y: rand(s,-140, 140) }, life: 0.45, maxLife: 0.45, color: '#5a8c4a', size: 3 });
  }
  s.player.pos.x = clamp(target.x, -s.worldWidth / 2, s.worldWidth / 2);
  s.player.pos.y = clamp(target.y, -s.worldHeight / 2, s.worldHeight / 2);
  for (let i = 0; i < 16; i++) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(s,-140, 140), y: rand(s,-140, 140) }, life: 0.45, maxLife: 0.45, color: '#5a8c4a', size: 3 });
  }
  const branch = getAbilityBranchId(s, 'teleport', 4);
  const final = getAbilityBranchId(s, 'teleport', 7);
  if (branch === 'teleport_phase' || final === 'teleport_phase_break') {
    s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 0.8);
  }
  if (final === 'teleport_phase_break') {
    const origin = from;
    const dx = s.player.pos.x - origin.x, dy = s.player.pos.y - origin.y;
    const distanceTravelled = Math.hypot(dx, dy);
    if (distanceTravelled > 140) {
      const steps = Math.max(1, Math.floor(distanceTravelled / 140));
      for (let i = 1; i < steps; i++) {
        const point = { x: origin.x + dx * (i / steps), y: origin.y + dy * (i / steps) };
        for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, point) < 70) dealDamageToEnemy(s, enemy, 14, undefined);
        s.particles.push({ pos: point, vel: { x: 0, y: 0 }, life: 0.35, maxLife: 0.35, color: '#5a8c4a', size: 5 });
      }
    }
  }
}


function activateTeleport(s: GameState): void {
  const lvl = s.player.abilities.teleport || 0;
  if (lvl === 0 || s.player.teleportCooldown > 0) return;
  const cd = (15 - Math.min(4, (lvl - 1) * 2)) * getCooldownMult(s);
  s.player.teleportCooldown = Math.max(5, cd);

  const branch = getAbilityBranchId(s, 'teleport', 4);
  const final = getAbilityBranchId(s, 'teleport', 7);
  const targetSphere = final === 'teleport_hunter_beacon' || branch === 'teleport_echo_jump'
    ? getNearestSphere(s, s.player.pos)
    : getNearestSphere(s, s.player.pos, (sphere) => dist(sphere.pos, s.player.pos) <= 700);

  if (targetSphere) {
    const target = { ...targetSphere.pos };
    const origin = { ...s.player.pos };
    doTeleportTo(s, target);
    if (branch === 'teleport_beacon') s.player.teleportDamageBuffTimer = 4;
    if (branch === 'teleport_phase') s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 1.25);
    if (final === 'teleport_hunter_beacon' && targetSphere.type === 'sniper') s.player.teleportDamageBuffTimer = 5;
    const predatorChain = getActiveSphereAbilitySynergies(s).some((link) =>
      link.character === 'hunter' && link.sphere === 'chain' && link.ability === 'teleport'
    );
    if (predatorChain && targetSphere.type === 'chain') {
      const chainSpheres = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'chain');
      const prey = s.enemies
        .filter((enemy) => enemy.hp > 0)
        .sort((a, b) => {
          const da = chainSpheres.length ? Math.min(...chainSpheres.map((sphere) => dist(a.pos, sphere.pos))) : dist(a.pos, s.player.pos);
          const db = chainSpheres.length ? Math.min(...chainSpheres.map((sphere) => dist(b.pos, sphere.pos))) : dist(b.pos, s.player.pos);
          return da - db;
        })[0];
      if (prey) {
        s.player.hunterMarkTarget = prey;
        s.player.hunterMarkTimer = 4;
        for (const sphere of chainSpheres) {
          s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.5, maxLife: 0.5, color: '#c4453d', size: 4 });
        }
      }
    }
    if (final === 'teleport_spatial_network') {
      s.lightnings.push({ from: origin, to: target, life: 0.5 });
      const networkState = analyzeSphereNetwork(getNetworkNodes(s));
      const destinationIndex = s.spheres.indexOf(targetSphere);
      if (destinationIndex >= 0) {
        for (const linkedIndex of getLinkedNodeIndexes(networkState, destinationIndex)) {
          if (linkedIndex < 0 || linkedIndex >= s.spheres.length) continue;
          const linkedSphere = s.spheres[linkedIndex];
          if (!linkedSphere.alive) continue;
          s.lightnings.push({ from: { ...targetSphere.pos }, to: { ...linkedSphere.pos }, life: 0.24 });
          for (const enemy of s.enemies) {
            if (enemy.hp > 0 && dist(enemy.pos, linkedSphere.pos) < 70) dealDamageToEnemy(s, enemy, 12);
          }
        }
      }
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, target) < 90) dealDamageToEnemy(s, enemy, 22);
    }
  } else {
    doTeleportTo(s, { x: rand(s,s.player.pos.x - 300, s.player.pos.x + 300), y: rand(s,s.player.pos.y - 300, s.player.pos.y + 300) });
  }
  s.flashText = { text: 'ECHO JUMP', life: 0.9, color: '#5a8c4a' };
}


function activateFireTrail(s: GameState): void {
  const lvl = s.player.abilities.firetrail || 0;
  if (lvl === 0 || s.player.fireTrailCooldown > 0) return;
  s.player.fireTrailCooldown = 25 * getCooldownMult(s);
  s.player.fireTrailTimer = 5 + Math.min(3, lvl - 1);
  const branch = getAbilityBranchId(s, 'firetrail', 4);
  const final = getAbilityBranchId(s, 'firetrail', 7);
  const fireAligned = s.spheres.filter((sphere) => sphere.alive && s.player.sphereMods.fire > 0);
  for (const sphere of fireAligned) {
    sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.5);
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.9, maxLife: 0.9, color: '#c46d3d', size: 6 });
  }
  if (branch === 'firetrail_overdrive') {
    for (const sphere of fireAligned) {
      sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.7);
      s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.45, maxLife: 0.45, color: '#f0b35a', size: 5 });
    }
  }
  if (branch === 'firetrail_sanctum') {
    for (const sphere of s.spheres) {
      if (sphere.alive && sphere.type === 'aura') sphere.attackTimer = 0;
    }
  }
  const catalystField = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'alchemist' && link.sphere === 'aura' && link.ability === 'firetrail'
  );
  if (catalystField) s.player.alchemistCatalystTimer = 4;
  if (branch === 'firetrail_ignition' || final === 'firetrail_catalyst') {
    for (const e of s.enemies) {
      if (e.hp > 0 && (e.fireTimer > 0 || e.poisonTimer > 0 || e.freezeTimer > 0)) {
        e.fireTimer = Math.max(e.fireTimer, 2);
        e.fireDps = Math.max(e.fireDps, 5 + lvl * 2);
      }
    }
  }
  if (final === 'firetrail_catalyst') s.player.fireCatalystTimer = 4;
  if (final === 'firetrail_network' || final === 'firetrail_inferno') {
    const networkState = analyzeSphereNetwork(getNetworkNodes(s));
    const fireIndexes = new Set(fireAligned.map((sphere) => s.spheres.indexOf(sphere)));
    const processedPairs = new Set<string>();

    for (const sourceIndex of fireIndexes) {
      if (sourceIndex < 0) continue;
      for (const targetIndex of getLinkedNodeIndexes(networkState, sourceIndex)) {
        if (targetIndex < 0 || targetIndex >= s.spheres.length || !fireIndexes.has(targetIndex)) continue;
        const key = sourceIndex < targetIndex ? `${sourceIndex}:${targetIndex}` : `${targetIndex}:${sourceIndex}`;
        if (processedPairs.has(key)) continue;
        processedPairs.add(key);
        const source = s.spheres[sourceIndex];
        const target = s.spheres[targetIndex];
        const boost = final === 'firetrail_inferno' ? 0.32 : 0.2;
        s.lightnings.push({ from: { ...source.pos }, to: { ...target.pos }, life: 0.18 });
        source.attackTimer = Math.max(0, source.attackTimer - boost);
        target.attackTimer = Math.max(0, target.attackTimer - boost);
      }
    }
  }
  s.flashText = { text: 'OVERHEAT', life: 0.9, color: '#c46d3d' };
}


function activateMinion(s: GameState): void {
  const lvl = s.player.abilities.minion || 0;
  if (lvl === 0 || s.player.minionCooldown > 0) return;
  s.player.minionCooldown = 30 * getCooldownMult(s);
  const count = 1 + Math.floor((lvl - 1) / 2);
  const branch = getAbilityBranchId(s, 'minion', 4);
  const final = getAbilityBranchId(s, 'minion', 7);
  const relayMode = branch === 'minion_relay_drone' || final === 'minion_network_nodes';
  const liveSpheres = s.spheres.filter((sphere) => sphere.alive);

  for (let i = 0; i < count; i++) {
    const anchor = getNearestSphere(s, s.player.pos);
    const angle = (i / Math.max(1, count)) * Math.PI * 2;
    let spawnPos = anchor
      ? { x: anchor.pos.x + Math.cos(angle) * 42, y: anchor.pos.y + Math.sin(angle) * 42 }
      : { ...s.player.pos };

    // Relay/Network Nodes are positioned between two nearby Spheres so the
    // solver can produce actual Sphere -> Drone -> Sphere links.
    if (relayMode && liveSpheres.length >= 2) {
      let bestA: SphereEntity | null = null;
      let bestB: SphereEntity | null = null;
      let bestDistance = Infinity;
      for (let a = 0; a < liveSpheres.length - 1; a++) {
        for (let b = a + 1; b < liveSpheres.length; b++) {
          const d = dist(liveSpheres[a].pos, liveSpheres[b].pos);
          if (d < bestDistance) {
            bestDistance = d;
            bestA = liveSpheres[a];
            bestB = liveSpheres[b];
          }
        }
      }
      if (bestA && bestB && bestDistance <= 400) {
        spawnPos = {
          x: (bestA.pos.x + bestB.pos.x) / 2,
          y: (bestA.pos.y + bestB.pos.y) / 2,
        };
      }
    }

    s.minions.push({
      pos: spawnPos,
      hp: 1, attackTimer: 0, life: 10 + (lvl >= 5 ? 2 : 0), radius: 12, damage: 6 + Math.max(0, lvl - 1) * 2, rotation: 0,
    });
    if (anchor) {
      s.particles.push({ pos: { ...anchor.pos }, vel: { x: 0, y: 0 }, life: 0.7, maxLife: 0.7, color: '#4a7a8a', size: 5 });
    }
  }
  if (branch === 'minion_echo_drone') {
    for (const drone of s.minions.slice(-count)) {
      const anchor = getNearestSphere(s, drone.pos);
      if (anchor) {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - 0.45);
        s.particles.push({ pos: { ...anchor.pos }, vel: { x: 0, y: 0 }, life: 0.45, maxLife: 0.45, color: '#4a7a8a', size: 5 });
      }
    }
  }
  if (relayMode) {
    const network = analyzeSphereNetwork(getNetworkNodes(s));
    for (let i = 0; i < count; i++) {
      const minionIndex = s.minions.length - 1 - i;
      const drone = s.minions[minionIndex];
      if (!drone) continue;
      const nodeIndex = s.spheres.length + minionIndex;
      const linkedSpheres = network.links
        .filter((link) => link.a === nodeIndex || link.b === nodeIndex)
        .map((link) => (link.a === nodeIndex ? link.b : link.a))
        .filter((index) => index >= 0 && index < s.spheres.length)
        .filter((index, listIndex, list) => list.indexOf(index) === listIndex);
      if (linkedSpheres.length >= 2) {
        const first = s.spheres[linkedSpheres[0]];
        const second = s.spheres[linkedSpheres[1]];
        s.lightnings.push({ from: { ...first.pos }, to: { ...drone.pos }, life: 0.16 });
        s.lightnings.push({ from: { ...drone.pos }, to: { ...second.pos }, life: 0.16 });
      }
    }
  }
  if (final === 'minion_echo_swarm') {
    const networkState = analyzeSphereNetwork(getNetworkNodes(s));
    for (const drone of s.minions.slice(-count)) {
      s.particles.push({ pos: { ...drone.pos }, vel: { x: 0, y: 0 }, life: 0.8, maxLife: 0.8, color: '#d4943d', size: 6 });
      const droneIndex = s.spheres.length + s.minions.indexOf(drone);
      const linkedSphereIndexes = getLinkedNodeIndexes(networkState, droneIndex)
        .filter((index) => index >= 0 && index < s.spheres.length);
      for (const linkedIndex of linkedSphereIndexes) {
        const sphere = s.spheres[linkedIndex];
        if (sphere.alive) sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.22);
      }
    }
  }
  if (final === 'minion_sphere_guard') {
    for (const sphere of s.spheres) {
      if (sphere.alive && dist(sphere.pos, s.player.pos) < 300) sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.35);
    }
  }
  s.flashText = { text: 'ECHO DRONE', life: 0.9, color: '#4a7a8a' };
}


function activateLightning(s: GameState): void {
  const lvl = s.player.abilities.lightning || 0;
  if (lvl === 0 || s.player.lightningCooldown > 0) return;
  s.player.lightningCooldown = 20 * getCooldownMult(s);
  const branch = getAbilityBranchId(s, 'lightning', 4);
  const final = getAbilityBranchId(s, 'lightning', 7);
  const chainSpheres = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'chain');
  let ordered = [...chainSpheres].sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  const networkState = analyzeSphereNetwork(getNetworkNodes(s));

  if ((branch === 'lightning_relay' || final === 'lightning_storm_network') && ordered.length > 0) {
    const remaining = new Set(ordered);
    const networkOrder: SphereEntity[] = [];
    let current: SphereEntity | null = ordered[0] ?? null;

    while (current) {
      networkOrder.push(current);
      remaining.delete(current);
      const currentIndex = s.spheres.indexOf(current);
      const nextIndex = currentIndex >= 0
        ? getLinkedNodeIndexes(networkState, currentIndex)
          .filter((index) => index >= 0 && index < s.spheres.length)
          .filter((index) => s.spheres[index].type === 'chain')
          .filter((index) => remaining.has(s.spheres[index]))
          .sort((a, b) => dist(s.spheres[a].pos, current!.pos) - dist(s.spheres[b].pos, current!.pos))[0]
        : undefined;
      current = nextIndex === undefined ? null : (s.spheres[nextIndex] ?? null);
    }

    ordered = [...networkOrder, ...ordered.filter((sphere) => remaining.has(sphere))];
  }
  const targets = s.enemies.filter((e) => e.hp > 0).sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  const toxicNetwork = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'alchemist' && link.sphere === 'chain' && link.ability === 'lightning'
  );
  if (targets.length === 0) return;
  const maxTargets = 1 + Math.floor((lvl - 1) / 2);
  let previous: Vec = { ...s.player.pos };
  let jump = 0;
  for (const sphere of ordered) {
    s.lightnings.push({ from: { ...previous }, to: { ...sphere.pos }, life: 0.24 });
    if (branch === 'lightning_echo_storm') {
      for (const enemy of s.enemies) {
        if (enemy.hp > 0 && dist(enemy.pos, sphere.pos) < 55) {
          dealDamageToEnemy(s, enemy, 10 + lvl * 3);
        }
      }
    }
    previous = { ...sphere.pos };
    jump++;
  }
  const relayStorm = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'engineer' && link.sphere === 'chain' && link.ability === 'lightning'
  );
  if (relayStorm) {
    for (const sphere of ordered) {
      triggerEngineerRelay(s, sphere);
    }
  }
  const finalBonusTargets = final === 'lightning_thunder_chain' ? ordered.length : 0;
  const targetCount = Math.min(targets.length, maxTargets + finalBonusTargets);
  for (let i = 0; i < targetCount; i++) {
    const target = targets[i];
    s.lightnings.push({ from: { ...previous }, to: { ...target.pos }, life: 0.3 });
    const damage = (40 + lvl * 15) * (1 + jump * 0.12);
    dealDamageToEnemy(s, target, damage);
    if (toxicNetwork) {
      target.fireTimer = Math.max(target.fireTimer || 0, 1.5);
      target.poisonTimer = Math.max(target.poisonTimer || 0, 1.5);
    }
    previous = { ...target.pos };
    jump++;
    if (branch === 'lightning_relay' && ordered.length > 0) {
      const sourceSphere = ordered[i % ordered.length];
      const sourceIndex = s.spheres.indexOf(sourceSphere);
      const nextIndex = sourceIndex >= 0
        ? getLinkedNodeIndexes(networkState, sourceIndex)
          .filter((index) => index >= 0 && index < s.spheres.length)
          .filter((index) => s.spheres[index].type === 'chain' && s.spheres[index].alive)
          .sort((a, b) => dist(s.spheres[a].pos, sourceSphere.pos) - dist(s.spheres[b].pos, sourceSphere.pos))[0]
        : undefined;
      if (nextIndex !== undefined) {
        s.lightnings.push({ from: { ...target.pos }, to: { ...s.spheres[nextIndex].pos }, life: 0.22 });
      }
    }
    if (final === 'lightning_thunder_chain') {
      const next = targets[(i + 1) % targets.length];
      if (next && next !== target) {
        s.lightnings.push({ from: { ...target.pos }, to: { ...next.pos }, life: 0.22 });
        dealDamageToEnemy(s, next, damage * 0.25);
      }
    }
  }
  if (branch === 'lightning_overload' || final === 'lightning_overload_core') {
    const last = targets[Math.min(maxTargets, targets.length) - 1];
    if (last) dealDamageToEnemy(s, last, 30 + lvl * 10);
  }
  if (final === 'lightning_storm_network' && ordered.length > 0) {
    const chainIndexes = new Set(ordered.map((sphere) => s.spheres.indexOf(sphere)));
    const processedPairs = new Set<string>();
    let edgeCount = 0;

    for (const sourceIndex of chainIndexes) {
      if (sourceIndex < 0) continue;
      for (const targetIndex of getLinkedNodeIndexes(networkState, sourceIndex)) {
        if (targetIndex < 0 || targetIndex >= s.spheres.length || !chainIndexes.has(targetIndex)) continue;
        const key = sourceIndex < targetIndex ? `${sourceIndex}:${targetIndex}` : `${targetIndex}:${sourceIndex}`;
        if (processedPairs.has(key)) continue;
        processedPairs.add(key);
        edgeCount++;
        const from = s.spheres[sourceIndex].pos;
        const to = s.spheres[targetIndex].pos;
        s.lightnings.push({ from: { ...from }, to: { ...to }, life: 0.18 });
        for (const enemy of s.enemies) {
          if (enemy.hp > 0 && dist(enemy.pos, to) < 85) {
            dealDamageToEnemy(s, enemy, (35 + lvl * 8) * 0.35);
          }
        }
      }
    }

    if (edgeCount === 0) {
      for (let i = ordered.length - 1; i >= 0; i--) {
        const from = i > 0 ? ordered[i - 1].pos : s.player.pos;
        const to = ordered[i].pos;
        s.lightnings.push({ from: { ...from }, to: { ...to }, life: 0.18 });
        for (const enemy of s.enemies) {
          if (enemy.hp > 0 && dist(enemy.pos, to) < 85) dealDamageToEnemy(s, enemy, (35 + lvl * 8) * 0.35);
        }
      }
    }
  }
  s.flashText = { text: 'CHAIN LIGHTNING', life: 0.9, color: '#4a7a8a' };
}


function activateTimeStop(s: GameState): void {
  const lvl = s.player.abilities.timestop || 0;
  if (lvl === 0 || s.player.timestopCooldown > 0) return;
  s.player.timestopCooldown = 40 * getCooldownMult(s);
  s.player.timestopTimer = 3 + Math.min(2, lvl - 1);
  const nearest = getNearestSphere(s, s.player.pos);
  const branch = getAbilityBranchId(s, 'timestop', 4);
  const final = getAbilityBranchId(s, 'timestop', 7);
  const fieldMatrix = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'architect' && link.sphere === 'aura' && link.ability === 'timestop'
  );
  const radius = nearest
    ? 520 * (fieldMatrix ? 1.25 : 1)
    : Infinity;
  for (const e of s.enemies) {
    if (e.hp <= 0) continue;
    if (!nearest || dist(e.pos, nearest.pos) <= radius) {
      e.freezeTimer = s.player.timestopTimer + (branch === 'timestop_time_anchor' ? 1 : 0);
    }
  }
  if (branch === 'timestop_closed_time' || final === 'timestop_closed_network') {
    const networkState = analyzeSphereNetwork(getNetworkNodes(s));
    const startIndex = nearest ? s.spheres.indexOf(nearest) : -1;
    const visited = new Set<number>();
    const queue = startIndex >= 0 ? [startIndex] : [];

    while (queue.length > 0) {
      const sphereIndex = queue.shift()!;
      if (visited.has(sphereIndex) || sphereIndex < 0 || sphereIndex >= s.spheres.length) continue;
      const sphere = s.spheres[sphereIndex];
      if (!sphere.alive) continue;
      visited.add(sphereIndex);

      for (const e of s.enemies) {
        if (e.hp > 0 && dist(e.pos, sphere.pos) < 260) {
          e.freezeTimer = Math.max(e.freezeTimer, s.player.timestopTimer);
        }
      }

      for (const linkedIndex of getLinkedNodeIndexes(networkState, sphereIndex)) {
        if (linkedIndex >= 0 && linkedIndex < s.spheres.length && !visited.has(linkedIndex)) {
          queue.push(linkedIndex);
        }
      }
    }
  }
  if (branch === 'timestop_echo_phase') {
    for (const sphere of s.spheres) {
      if (sphere.alive) sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.65);
    }
  }
  if (nearest) {
    const pulseDamage = final === 'timestop_temporal_core' ? 10 + lvl * 8 : 10 + lvl * 4;
    emitSpherePulse(s, nearest, pulseDamage, 150, '#4a7a8a');
  }
  s.flashText = { text: 'ECHO FREEZE', life: 1.2, color: '#4a7a8a' };
}


function activateDarkRitual(s: GameState): void {
  const lvl = s.player.abilities.darkritual || 0;
  if (lvl === 0 || s.player.darkritualCooldown > 0) return;
  const hpCost = s.player.maxHp * 0.2;
  if (s.player.hp <= hpCost) return;
  s.player.darkritualCooldown = 30 * getCooldownMult(s);
  s.player.hp -= hpCost;
  s.player.overloadTimer = 5 + Math.min(3, lvl - 1);
  const branch = getAbilityBranchId(s, 'darkritual', 4);
  const final = getAbilityBranchId(s, 'darkritual', 7);
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.8);
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 1, maxLife: 1, color: '#8a5a8a', size: 7 });
  }
  const bloodResonance = getActiveSphereAbilitySynergies(s).some((link) =>
    link.character === 'berserker' && link.sphere === 'standard' && link.ability === 'darkritual'
  );
  if (bloodResonance) {
    for (const sphere of s.spheres) {
      if (sphere.alive && sphere.type === 'standard') {
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.8);
        emitSpherePulse(s, sphere, 10 + lvl * 3, 75, '#c4453d');
      }
    }
  }
  if (branch === 'darkritual_blood_link' || final === 'darkritual_blood_network') {
    const networkState = analyzeSphereNetwork(getNetworkNodes(s));
    const standardIndexes = s.spheres
      .map((sphere, index) => ({ sphere, index }))
      .filter(({ sphere }) => sphere.alive && sphere.type === 'standard')
      .map(({ index }) => index);

    const target = getNearestSphere(s, s.player.pos, (sphere) => sphere.type === 'standard');
    if (branch === 'darkritual_blood_link' && target) {
      target.attackTimer = Math.max(0, target.attackTimer - 1.4);
    }

    if (final === 'darkritual_blood_network') {
      const linkedStandardIndexes = standardIndexes.filter((index) =>
        getLinkedNodeIndexes(networkState, index).some((neighbor) => standardIndexes.includes(neighbor))
      );
      for (const index of linkedStandardIndexes) {
        const sphere = s.spheres[index];
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.8);
      }
    }
  }
  if (branch === 'darkritual_void_pact' || final === 'darkritual_void_engine') {
    const ratio = s.player.hp / s.player.maxHp;
    if (ratio < 0.35) s.player.overloadTimer += 2;
  }
  if (branch === 'darkritual_sacrifice' || final === 'darkritual_sacrifice_core') {
    const sacrificeMultiplier = final === 'darkritual_sacrifice_core' ? 1.35 : 1;
    emitSpherePulse(
      s,
      getNearestSphere(s, s.player.pos) || { pos: { ...s.player.pos } } as SphereEntity,
      (20 + lvl * 5) * sacrificeMultiplier,
      final === 'darkritual_sacrifice_core' ? 145 : 130,
      '#8a5a8a',
    );
  }
  if (final === 'darkritual_void_engine' && s.player.hp / s.player.maxHp < 0.2) {
    for (const sphere of s.spheres) {
      if (sphere.alive) emitSpherePulse(s, sphere, 18 + lvl * 6, 110, '#8a5a8a');
    }
  }
  s.screenShake = 0.22;
  s.flashText = { text: 'OVERLOAD', life: 1.2, color: '#8a5a8a' };
}


export function activateByKey(s: GameState, key: string): void {
  const ability = s.activeKeyMap[key];
  if (!ability) return;
  switch (ability) {
    case 'blast': activateBlast(s); break;
    case 'shield': activateShield(s); break;
    case 'teleport': activateTeleport(s); break;
    case 'firetrail': activateFireTrail(s); break;
    case 'minion': activateMinion(s); break;
    case 'lightning': activateLightning(s); break;
    case 'timestop': activateTimeStop(s); break;
    case 'darkritual': activateDarkRitual(s); break;
  }
}

// ===== Upgrade generation =====
