// Web Audio API sound system — no external assets needed

let ctx: AudioContext | null = null;
let masterGain: GainNode | null = null;
let enabled = true;

function ensureCtx(): AudioContext | null {
  if (!ctx) {
    try {
      ctx = new (window.AudioContext || (window as any).webkitAudioContext)();
      masterGain = ctx.createGain();
      masterGain.gain.value = 0.3;
      masterGain.connect(ctx.destination);
    } catch {
      return null;
    }
  }
  if (ctx.state === 'suspended') ctx.resume();
  return ctx;
}

export function setAudioEnabled(v: boolean): void {
  enabled = v;
  if (masterGain) masterGain.gain.value = v ? 0.3 : 0;
}

export function isAudioEnabled(): boolean {
  return enabled;
}

type SoundName =
  | 'hit' | 'crit' | 'kill' | 'levelup' | 'boss' | 'bosshit'
  | 'place' | 'damage' | 'pickup' | 'health' | 'evolve'
  | 'dash' | 'chest' | 'wave' | 'gameover' | 'elite'
  | 'shoot' | 'explosion';

export function playSound(name: SoundName): void {
  if (!enabled) return;
  const c = ensureCtx();
  if (!c || !masterGain) return;
  const now = c.currentTime;

  const osc = c.createOscillator();
  const gain = c.createGain();
  osc.connect(gain);
  gain.connect(masterGain);

  switch (name) {
    case 'hit': {
      // Short paper-like snap with a softer body tone.
      osc.type = 'square';
      osc.frequency.setValueAtTime(260, now);
      osc.frequency.exponentialRampToValueAtTime(105, now + 0.05);
      gain.gain.setValueAtTime(0.055, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.06);
      osc.start(now); osc.stop(now + 0.07);

      const body = c.createOscillator();
      const bodyGain = c.createGain();
      body.connect(bodyGain); bodyGain.connect(masterGain);
      body.type = 'triangle';
      body.frequency.setValueAtTime(520, now);
      body.frequency.exponentialRampToValueAtTime(250, now + 0.045);
      bodyGain.gain.setValueAtTime(0.018, now);
      bodyGain.gain.exponentialRampToValueAtTime(0.001, now + 0.055);
      body.start(now); body.stop(now + 0.06);
      break;
    }
    case 'shoot':
      osc.type = 'sine';
      osc.frequency.setValueAtTime(400, now);
      osc.frequency.exponentialRampToValueAtTime(200, now + 0.04);
      gain.gain.setValueAtTime(0.03, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.05);
      osc.start(now); osc.stop(now + 0.06);
      break;
    case 'crit': {
      // Two-tone impact so a crit is immediately distinguishable from a normal hit.
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(720, now);
      osc.frequency.exponentialRampToValueAtTime(145, now + 0.1);
      gain.gain.setValueAtTime(0.105, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
      osc.start(now); osc.stop(now + 0.13);

      const ping = c.createOscillator();
      const pingGain = c.createGain();
      ping.connect(pingGain); pingGain.connect(masterGain);
      ping.type = 'sine';
      ping.frequency.setValueAtTime(980, now);
      ping.frequency.exponentialRampToValueAtTime(560, now + 0.12);
      pingGain.gain.setValueAtTime(0.045, now);
      pingGain.gain.exponentialRampToValueAtTime(0.001, now + 0.16);
      ping.start(now); ping.stop(now + 0.17);
      break;
    }
    case 'kill': {
      // A low snap plus a tiny upward chime gives the kill a clearer resolution.
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(165, now);
      osc.frequency.exponentialRampToValueAtTime(65, now + 0.08);
      gain.gain.setValueAtTime(0.055, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.1);
      osc.start(now); osc.stop(now + 0.11);

      const chime = c.createOscillator();
      const chimeGain = c.createGain();
      chime.connect(chimeGain); chimeGain.connect(masterGain);
      chime.type = 'sine';
      chime.frequency.setValueAtTime(360, now + 0.015);
      chime.frequency.linearRampToValueAtTime(520, now + 0.1);
      chimeGain.gain.setValueAtTime(0.022, now + 0.015);
      chimeGain.gain.exponentialRampToValueAtTime(0.001, now + 0.16);
      chime.start(now + 0.015); chime.stop(now + 0.18);
      break;
    }
    case 'levelup': {
      osc.type = 'sine';
      osc.frequency.setValueAtTime(440, now);
      osc.frequency.linearRampToValueAtTime(880, now + 0.15);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);
      osc.start(now); osc.stop(now + 0.35);
      // second tone
      const osc2 = c.createOscillator();
      const g2 = c.createGain();
      osc2.connect(g2); g2.connect(masterGain);
      osc2.type = 'sine';
      osc2.frequency.setValueAtTime(660, now + 0.1);
      osc2.frequency.linearRampToValueAtTime(1320, now + 0.25);
      g2.gain.setValueAtTime(0.08, now + 0.1);
      g2.gain.exponentialRampToValueAtTime(0.001, now + 0.35);
      osc2.start(now + 0.1); osc2.stop(now + 0.4);
      break;
    }
    case 'boss':
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(80, now);
      osc.frequency.linearRampToValueAtTime(40, now + 0.5);
      gain.gain.setValueAtTime(0.15, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.6);
      osc.start(now); osc.stop(now + 0.7);
      break;
    case 'bosshit':
      osc.type = 'square';
      osc.frequency.setValueAtTime(100, now);
      osc.frequency.exponentialRampToValueAtTime(50, now + 0.08);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.1);
      osc.start(now); osc.stop(now + 0.12);
      break;
    case 'place': {
      // Clean confirmation tone for placing a sphere/tower.
      osc.type = 'sine';
      osc.frequency.setValueAtTime(280, now);
      osc.frequency.linearRampToValueAtTime(520, now + 0.1);
      gain.gain.setValueAtTime(0.065, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);
      osc.start(now); osc.stop(now + 0.2);

      const harmonic = c.createOscillator();
      const harmonicGain = c.createGain();
      harmonic.connect(harmonicGain); harmonicGain.connect(masterGain);
      harmonic.type = 'triangle';
      harmonic.frequency.setValueAtTime(840, now + 0.035);
      harmonic.frequency.linearRampToValueAtTime(620, now + 0.12);
      harmonicGain.gain.setValueAtTime(0.018, now + 0.035);
      harmonicGain.gain.exponentialRampToValueAtTime(0.001, now + 0.17);
      harmonic.start(now + 0.035); harmonic.stop(now + 0.19);
      break;
    }
    case 'damage':
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(200, now);
      osc.frequency.exponentialRampToValueAtTime(50, now + 0.15);
      gain.gain.setValueAtTime(0.12, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);
      osc.start(now); osc.stop(now + 0.25);
      break;
    case 'pickup':
      osc.type = 'sine';
      osc.frequency.setValueAtTime(600, now);
      osc.frequency.linearRampToValueAtTime(900, now + 0.05);
      gain.gain.setValueAtTime(0.04, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);
      osc.start(now); osc.stop(now + 0.1);
      break;
    case 'health':
      osc.type = 'sine';
      osc.frequency.setValueAtTime(500, now);
      osc.frequency.linearRampToValueAtTime(800, now + 0.1);
      gain.gain.setValueAtTime(0.08, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);
      osc.start(now); osc.stop(now + 0.25);
      break;
    case 'evolve':
      osc.type = 'sine';
      osc.frequency.setValueAtTime(330, now);
      osc.frequency.linearRampToValueAtTime(990, now + 0.3);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.5);
      osc.start(now); osc.stop(now + 0.6);
      break;
    case 'dash':
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(800, now);
      osc.frequency.exponentialRampToValueAtTime(200, now + 0.1);
      gain.gain.setValueAtTime(0.08, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
      osc.start(now); osc.stop(now + 0.15);
      break;
    case 'chest':
      osc.type = 'sine';
      osc.frequency.setValueAtTime(440, now);
      osc.frequency.linearRampToValueAtTime(880, now + 0.2);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);
      osc.start(now); osc.stop(now + 0.35);
      break;
    case 'wave':
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(220, now);
      osc.frequency.linearRampToValueAtTime(330, now + 0.15);
      gain.gain.setValueAtTime(0.06, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);
      osc.start(now); osc.stop(now + 0.25);
      break;
    case 'gameover':
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(300, now);
      osc.frequency.linearRampToValueAtTime(50, now + 0.8);
      gain.gain.setValueAtTime(0.12, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 1);
      osc.start(now); osc.stop(now + 1.1);
      break;
    case 'elite':
      osc.type = 'square';
      osc.frequency.setValueAtTime(150, now);
      osc.frequency.linearRampToValueAtTime(300, now + 0.15);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);
      osc.start(now); osc.stop(now + 0.25);
      break;
    case 'explosion': {
      // noise burst
      const bufferSize = c.sampleRate * 0.3;
      const buffer = c.createBuffer(1, bufferSize, c.sampleRate);
      const data = buffer.getChannelData(0);
      for (let i = 0; i < bufferSize; i++) data[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / bufferSize, 2);
      const noise = c.createBufferSource();
      noise.buffer = buffer;
      const noiseGain = c.createGain();
      noise.connect(noiseGain); noiseGain.connect(masterGain);
      noiseGain.gain.setValueAtTime(0.15, now);
      noiseGain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);
      noise.start(now); noise.stop(now + 0.35);
      // also osc
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(120, now);
      osc.frequency.exponentialRampToValueAtTime(30, now + 0.2);
      gain.gain.setValueAtTime(0.1, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.25);
      osc.start(now); osc.stop(now + 0.3);
      break;
    }
  }
}
