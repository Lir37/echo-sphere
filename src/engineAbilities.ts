import { playSound } from './audio';
import type { AbilityType } from './gameData';
import type { GameState, SphereEntity, Vec } from './engineTypes';
import {
  dist, rand, clamp, getNetworkFrame, getAbilityBranchId, getNearestSphere
} from './engineRuntime';
import {
  dealDamageToEnemy, getCooldownMult, getVampirePercent, emitSpherePulse
} from './engineCombat';
import { getLinkedNodeIndexes } from './network';
import { getActiveSphereAbilitySynergies, getAbilityEvolutionChoice } from './sphereProgression';


function synergyStrength(index:number):number {
  return 1 / (1 + index * 0.18);
}

function applySphereAbilitySynergyRiders(s:GameState, ability:AbilityType):void {
  const links = getActiveSphereAbilitySynergies(s)
    .filter((link)=>link.ability===ability)
    .sort((a,b)=>a.id.localeCompare(b.id));

  for (let index=0; index<links.length; index++) {
    const link=links[index];
    const power=synergyStrength(index);
    const lvl=Number(s.player.abilities[ability]||1);

    switch(link.behavior) {
      case 'blast_standard_resonator':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='standard'))
          emitSpherePulse(s,sphere,(26+lvl*8)*0.30*power,92*power,'#d4943d');
        break;
      case 'blast_shotgun_cataclysm':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='shotgun'))
          emitSpherePulse(s,sphere,(30+(lvl-1)*10)*0.42*power,88,'#ffd06a');
        break;
      case 'blast_prism_split': {
        const targets=s.enemies.filter(e=>e.hp>0).sort((a,b)=>dist(a.pos,s.player.pos)-dist(b.pos,s.player.pos)).slice(0,3);
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='prism')) for(const enemy of targets){
          s.lightnings.push({from:{...sphere.pos},to:{...enemy.pos},life:0.16});
          dealDamageToEnemy(s,enemy,(30+(lvl-1)*10)*0.28*power);
        }
        break;
      }
      case 'blast_gravity_collapse': {
        const gravity=s.spheres.find(v=>v.alive&&v.type==='gravity');
        if(gravity) {
          for(const enemy of s.enemies) {
            if(enemy.hp<=0||dist(enemy.pos,gravity.pos)>145) continue;
            const dx=gravity.pos.x-enemy.pos.x,dy=gravity.pos.y-enemy.pos.y,d=Math.hypot(dx,dy)||1;
            enemy.pos.x+=dx/d*48*power; enemy.pos.y+=dy/d*48*power;
            dealDamageToEnemy(s,enemy,(30+(lvl-1)*10)*0.25*power);
          }
          emitSpherePulse(s,gravity,(30+(lvl-1)*10)*0.22*power,82*power,'#9b7cff');
        }
        break;
      }
      case 'blast_pulse_wave':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='pulse')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>92) continue;
          const dx=enemy.pos.x-sphere.pos.x,dy=enemy.pos.y-sphere.pos.y,d=Math.hypot(dx,dy)||1;
          enemy.pos.x+=dx/d*46*power; enemy.pos.y+=dy/d*46*power;
        }
        break;

      case 'timestop_standard_singularity': {
        const standard=s.spheres.find(v=>v.alive&&v.type==='standard');
        if(standard) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,standard.pos)>150) continue;
          const dx=standard.pos.x-enemy.pos.x,dy=standard.pos.y-enemy.pos.y,d=Math.hypot(dx,dy)||1;
          enemy.pos.x+=dx/d*42*power; enemy.pos.y+=dy/d*42*power;
          enemy.freezeTimer=Math.max(enemy.freezeTimer,s.player.timestopTimer+0.45*power);
        }
        break;
      }
      case 'timestop_chain_web':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='chain')) for(const enemy of s.enemies)
          if(enemy.hp>0&&dist(enemy.pos,sphere.pos)<150) enemy.freezeTimer=Math.max(enemy.freezeTimer,s.player.timestopTimer+0.75*power);
        break;
      case 'timestop_gravity_tide':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='gravity')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>155) continue;
          const dx=enemy.pos.x-sphere.pos.x,dy=enemy.pos.y-sphere.pos.y,d=Math.hypot(dx,dy)||1;
          enemy.pos.x+=dx/d*58*power; enemy.pos.y+=dy/d*58*power;
          dealDamageToEnemy(s,enemy,(12+lvl*3)*0.55*power);
        }
        break;

      case 'minion_standard_swarm':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='standard'))
          emitSpherePulse(s,sphere,(10+lvl*2)*0.55*power,72,'#72f08e');
        break;
      case 'minion_orbital_dance':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='orbital')) {
          sphere.attackTimer=Math.max(0,sphere.attackTimer-0.65*power);
          emitSpherePulse(s,sphere,(8+lvl*2)*0.5*power,62,'#68d9ff');
        }
        break;
      case 'minion_void_reaper': {
        let harvested=0;
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='void')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>92) continue;
          const before=enemy.hp;
          dealDamageToEnemy(s,enemy,(10+lvl*2)*0.55*power);
          if(before>0&&enemy.hp<=0) harvested++;
        }
        if(harvested>0) s.player.hp=Math.min(s.player.maxHp,s.player.hp+harvested*2*power);
        break;
      }

      case 'teleport_sniper_oracle': {
        const sniper=getNearestSphere(s,s.player.pos,v=>v.type==='sniper');
        if(sniper) {
          const target=s.enemies.filter(e=>e.hp>0).sort((a,b)=>dist(a.pos,sniper.pos)-dist(b.pos,sniper.pos))[0];
          if(target) {
            s.player.hunterMarkTarget=target;
            s.player.hunterMarkTimer=Math.max(s.player.hunterMarkTimer,5);
            dealDamageToEnemy(s,target,(18+lvl*5)*0.75*power);
          }
        }
        break;
      }
      case 'teleport_aura_gravity':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='aura')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>125) continue;
          const dx=sphere.pos.x-enemy.pos.x,dy=sphere.pos.y-enemy.pos.y,d=Math.hypot(dx,dy)||1;
          enemy.pos.x+=dx/d*58*power; enemy.pos.y+=dy/d*58*power;
        }
        break;
      case 'teleport_prism_mirror': {
        const targets=s.enemies.filter(e=>e.hp>0).sort((a,b)=>dist(a.pos,s.player.pos)-dist(b.pos,s.player.pos)).slice(0,3);
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='prism')) for(const target of targets) {
          s.lightnings.push({from:{...sphere.pos},to:{...target.pos},life:0.20});
          dealDamageToEnemy(s,target,(18+lvl*5)*0.55*power);
        }
        break;
      }
      case 'teleport_void_execution':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='void')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>155) continue;
          const hpRatio=enemy.hp/Math.max(1,enemy.maxHp);
          if(hpRatio<=0.30&&!enemy.isBoss) enemy.hp=0;
          else dealDamageToEnemy(s,enemy,(20+lvl*5)*0.70*power);
        }
        break;

      case 'firetrail_sniper_beacon':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='sniper')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,sphere.pos)>115) continue;
          enemy.fireTimer=Math.max(enemy.fireTimer,2.2);
          enemy.fireDps=Math.max(enemy.fireDps,6+lvl*2);
          enemy.slowTimer=Math.max(enemy.slowTimer,2.0);
          enemy.slowFactor=Math.min(enemy.slowFactor,0.72);
        }
        break;
      case 'firetrail_aura_overgrowth':
        for(const aura of s.spheres.filter(v=>v.alive&&v.type==='aura')) for(const sphere of s.spheres)
          if(sphere.alive&&dist(sphere.pos,aura.pos)<=150) sphere.attackTimer=Math.max(0,sphere.attackTimer-0.50*power);
        break;
      case 'firetrail_prism_spectrum':
        for(const prism of s.spheres.filter(v=>v.alive&&v.type==='prism')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,prism.pos)>105) continue;
          const hadOther=enemy.freezeTimer>0||enemy.poisonTimer>0;
          enemy.fireTimer=Math.max(enemy.fireTimer,1.8);
          enemy.fireDps=Math.max(enemy.fireDps,6+lvl*2);
          if(hadOther) dealDamageToEnemy(s,enemy,(16+lvl*4)*0.85*power);
        }
        break;
      case 'firetrail_gravity_well':
        for(const gravity of s.spheres.filter(v=>v.alive&&v.type==='gravity')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,gravity.pos)>125) continue;
          enemy.fireTimer=Math.max(enemy.fireTimer,3.0);
          enemy.fireDps=Math.max(enemy.fireDps,6+lvl*2);
          enemy.slowTimer=Math.max(enemy.slowTimer,2.5);
          enemy.slowFactor=Math.min(enemy.slowFactor,0.60);
        }
        break;

      case 'shield_shotgun_burst':
        for(const shotgun of s.spheres.filter(v=>v.alive&&v.type==='shotgun')) for(const enemy of s.enemies) {
          if(enemy.hp<=0||dist(enemy.pos,shotgun.pos)>95) continue;
          const dx=enemy.pos.x-shotgun.pos.x,dy=enemy.pos.y-shotgun.pos.y,d=Math.hypot(dx,dy)||1;
          enemy.pos.x+=dx/d*62*power; enemy.pos.y+=dy/d*62*power;
        }
        break;
      case 'shield_aura_sanctum': {
        const auras=s.spheres.filter(v=>v.alive&&v.type==='aura');
        s.player.shieldCharges=Math.min(5,s.player.shieldCharges+Math.min(2,auras.length));
        for(const aura of auras) for(const enemy of s.enemies) if(enemy.hp>0&&dist(enemy.pos,aura.pos)<105) {
          enemy.slowTimer=Math.max(enemy.slowTimer,s.player.shieldTimer+0.5);
          enemy.slowFactor=Math.min(enemy.slowFactor,0.68);
        }
        break;
      }
      case 'shield_orbital_halo':
        s.player.shieldCharges=Math.min(5,s.player.shieldCharges+Math.min(2,s.spheres.filter(v=>v.alive&&v.type==='orbital').length));
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='orbital')) emitSpherePulse(s,sphere,(10+lvl*2)*0.45*power,72,'#b9e8ff');
        break;
      case 'shield_pulse_burst':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='pulse')) emitSpherePulse(s,sphere,(10+lvl*2)*0.65*power,92,'#fff0a9');
        break;

      case 'lightning_shotgun_hail': {
        const targets=s.enemies.filter(e=>e.hp>0);
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='shotgun')) {
          for(const enemy of targets.filter(e=>dist(e.pos,sphere.pos)<250).sort((a,b)=>dist(a.pos,sphere.pos)-dist(b.pos,sphere.pos)).slice(0,3)) {
            s.lightnings.push({from:{...sphere.pos},to:{...enemy.pos},life:0.14});
            dealDamageToEnemy(s,enemy,(20+lvl*5)*0.40*power);
          }
        }
        break;
      }
      case 'lightning_chain_storm':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='chain')) for(const enemy of s.enemies.filter(e=>e.hp>0).filter(e=>dist(e.pos,sphere.pos)<95).slice(0,2)) {
          dealDamageToEnemy(s,enemy,(8+lvl*3)*0.80*power);
          s.lightnings.push({from:{...sphere.pos},to:{...enemy.pos},life:0.12});
        }
        break;
      case 'lightning_pulse_resonator':
        chargeResonance(s,'network',dealDamageToEnemy,0.45*power);
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='pulse')) sphere.attackTimer=Math.max(0,sphere.attackTimer-0.45*power);
        break;

      case 'darkritual_sniper_assassin': {
        const target=s.enemies.filter(e=>e.hp>0).sort((a,b)=>(a.hp/a.maxHp)-(b.hp/b.maxHp))[0];
        if(target) dealDamageToEnemy(s,target,Math.min(target.hp,Math.max(18,target.maxHp*0.08))*power);
        break;
      }
      case 'darkritual_chain_leech': {
        const refund=Math.min(s.player.maxHp*0.08,s.player.maxHp*0.20*0.20*power);
        s.player.hp=Math.min(s.player.maxHp,s.player.hp+refund);
        break;
      }
      case 'darkritual_orbital_blade':
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='orbital')) emitSpherePulse(s,sphere,(22+lvl*5)*0.48*power,85,'#ffbf69');
        break;
      case 'darkritual_void_hunger': {
        const missing=1-s.player.hp/Math.max(1,s.player.maxHp);
        for(const sphere of s.spheres.filter(v=>v.alive&&v.type==='void'))
          emitSpherePulse(s,sphere,(18+lvl*5)*(0.65+missing*0.75)*power,92,'#c58cff');
        break;
      }
    }
  }
}

function emitAbilityMutationVfx(s: GameState, ability: AbilityType): void {
  const branch = getAbilityEvolutionChoice(s, ability, 4);
  if (!branch) return;
  const final = getAbilityEvolutionChoice(s, ability, 7);
  const palette:Record<string,string> = {
    blast:'#d4943d', shield:'#5ac8ff', teleport:'#7ce38b', firetrail:'#ff7a3d',
    minion:'#68d9ff', lightning:'#9b7cff', timestop:'#b9a7ff', darkritual:'#c4453d',
  };
  const color = palette[ability] || '#63e6ff';
  const branchPhase = (branch.id.split('_').pop()?.charCodeAt(0) || 65) - 65;
  const finalPhase = final ? (final.id.length % 3) : 0;
  const count = final ? 18 : 10;
  for (let i=0;i<count;i++) {
    const a = (i / count) * Math.PI * 2 + branchPhase * 0.7 + s.time * (final ? 1.8 : 1.2);
    const radius = 22 + (final ? 10 : 4) + (i % 3) * 7;
    const pos = { x:s.player.pos.x + Math.cos(a)*radius, y:s.player.pos.y + Math.sin(a)*radius };
    s.particles.push({
      pos, vel:{x:Math.cos(a)*18,y:Math.sin(a)*18},
      life:final ? 0.72 : 0.5, maxLife:final ? 0.72 : 0.5,
      color, size:final ? 4.5 : 3.2,
    });
  }
  if (final) {
    const nearby = s.spheres.filter((sphere)=>sphere.alive).slice(0, Math.min(3, s.spheres.length));
    for (let i=0;i<nearby.length;i++) {
      const target=nearby[i];
      s.lightnings.push({ from:{...s.player.pos}, to:{...target.pos}, life:0.18 + finalPhase * 0.02 });
    }
  }
}

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
      const networkState = getNetworkFrame(s);
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
    let strength = 1;
    for (const sphere of ordered) {
      emitSpherePulse(s, sphere, baseDamage * strength, radius, '#c46d3d', final === 'blast_resonant_core');
      if (branch === 'blast_resonance' && sphere.type === 'standard') {
        emitSpherePulse(s, sphere, baseDamage * 0.4, radius * 0.72, '#d4943d');
      }
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

  if (branch === 'blast_core') {
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, s.player.pos) <= 100) dealDamageToEnemy(s, e, baseDamage * 0.5);
    }
  }
  if (branch === 'blast_resonance') {
    const standard = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'standard');
    for (const sphere of standard) emitSpherePulse(s, sphere, baseDamage * 0.25, 90, '#d4943d');
  }

    applySphereAbilitySynergyRiders(s, 'blast');
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
  const networkState = final === 'shield_network_guard' ? getNetworkFrame(s) : null;
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
  s.player.shieldCharges = Math.min(5, 1 + Math.floor((lvl - 1) / 2) + networkBonus + branchBonus + bastionBonus + networkGuardBonus);
  s.player.shieldTimer = 10 + (lvl >= 5 ? 2 : 0);
  s.player.shieldVisualPulse = 0.85;
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
    const networkState = getNetworkFrame(s);
    for (const link of networkState.links) {
      if (link.a >= s.spheres.length || link.b >= s.spheres.length) continue;
      const first = s.spheres[link.a];
      const second = s.spheres[link.b];
      if (!first.alive || !second.alive) continue;
      s.lightnings.push({ from: { ...first.pos }, to: { ...second.pos }, life: 0.22 });
    }
  }
  applySphereAbilitySynergyRiders(s, 'shield');
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
    if (final === 'teleport_spatial_network') {
      s.lightnings.push({ from: origin, to: target, life: 0.5 });
      const networkState = getNetworkFrame(s);
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
  applySphereAbilitySynergyRiders(s, 'teleport');
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
    const networkState = getNetworkFrame(s);
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
  applySphereAbilitySynergyRiders(s, 'firetrail');
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
      anchorType: anchor?.type || 'standard',
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
  if (branch === 'minion_guardian') {
    for (const drone of s.minions.slice(-count)) {
      const anchor = getNearestSphere(s, drone.pos);
      if (anchor) {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - 0.35);
        s.particles.push({ pos: { ...anchor.pos }, vel: { x: 0, y: 0 }, life: 0.55, maxLife: 0.55, color: '#68d9ff', size: 5 });
      }
    }
  }
  if (relayMode) {
    const network = getNetworkFrame(s);
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
    const networkState = getNetworkFrame(s);
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
  applySphereAbilitySynergyRiders(s, 'minion');
  s.flashText = { text: 'ECHO DRONE', life: 0.9, color: '#4a7a8a' };
}


function activateLightning(s: GameState): void {
  const lvl = s.player.abilities.lightning || 0;
  if (lvl === 0 || s.player.lightningCooldown > 0) return;
  s.player.lightningCooldown = 20 * getCooldownMult(s);
  const branch = getAbilityBranchId(s, 'lightning', 4);
  const final = getAbilityBranchId(s, 'lightning', 7);
  const networkSpheres = s.spheres.filter((sphere) => sphere.alive);
  let ordered = [...networkSpheres].sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  const networkState = getNetworkFrame(s);

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
          .filter((index) => remaining.has(s.spheres[index]))
          .sort((a, b) => dist(s.spheres[a].pos, current!.pos) - dist(s.spheres[b].pos, current!.pos))[0]
        : undefined;
      current = nextIndex === undefined ? null : (s.spheres[nextIndex] ?? null);
    }

    ordered = [...networkOrder, ...ordered.filter((sphere) => remaining.has(sphere))];
  }
  const targets = s.enemies.filter((e) => e.hp > 0).sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  if (targets.length === 0) return;
  const maxTargets = 1 + Math.floor((lvl - 1) / 2);
  let previous: Vec = ordered.length > 0 ? { ...ordered[0].pos } : { ...s.player.pos };
  let jump = 0;
  for (let networkIndex = 0; networkIndex < ordered.length; networkIndex++) {
    const sphere = ordered[networkIndex];
    if (networkIndex > 0) {
      s.lightnings.push({ from: { ...previous }, to: { ...sphere.pos }, life: 0.24 });
    }
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
          .filter((index) => s.spheres[index].alive)
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
  applySphereAbilitySynergyRiders(s, 'lightning');
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
  const radius = nearest ? 520 : Infinity;
  for (const e of s.enemies) {
    if (e.hp <= 0) continue;
    if (!nearest || dist(e.pos, nearest.pos) <= radius) {
      e.freezeTimer = s.player.timestopTimer + (branch === 'timestop_time_anchor' ? 1 : 0);
    }
  }
  if (branch === 'timestop_closed_time' || final === 'timestop_closed_network') {
    const networkState = getNetworkFrame(s);
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
  applySphereAbilitySynergyRiders(s, 'timestop');
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
  if (branch === 'darkritual_blood_link' || final === 'darkritual_blood_network') {
    const networkState = getNetworkFrame(s);
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
  applySphereAbilitySynergyRiders(s, 'darkritual');
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
  emitAbilityMutationVfx(s, ability);
}

// ===== Upgrade generation =====
