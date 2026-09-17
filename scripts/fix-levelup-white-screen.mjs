import fs from 'node:fs';

const appPath = 'src/App.tsx';
let app = fs.readFileSync(appPath, 'utf8');

app = app.replace(/\n\s*\{st\.pendingTowerUpgrade && <TowerUpgradeModal[\s\S]*?\n\s*\}\n/, '\n');
app = app.replace(/\s*pendingTowerUpgrade\s*\|\|\s*/g, '');
app = app.replace(/\s*pendingTowerUpgrade\s*\|\|\s*/g, '');

const oldAbility = "const def = ABILITIES[c.ability!];\n            return <button key={i} onClick={() => onPick(c)} className=\"p-5 rounded-xl bg-[#e8dcc0] border border-[#4a7a8a]/30 hover:border-[#4a7a8a]/60 hover:scale-105 transition-all text-left\"><div className=\"text-[#4a7a8a] text-xs uppercase mb-1\">{def.category === 'active' ? t('active') : t('passive')}</div><div className=\"font-bold text-lg mb-2\">{def.name[lang]}</div><div className=\"text-sm text-[#5a4a32] mb-2\">{def.desc[lang](c.newLevel)}</div><div className=\"text-xs text-[#8a7a5a]/70\">{t('level')} {c.currentLevel} → {c.newLevel} / {def.maxLevel}</div></button>;";
const newAbility = "const def = c.ability ? ABILITIES[c.ability] : undefined;\n            if (!def) return null;\n            const desc = typeof def.desc[lang] === 'function' ? def.desc[lang](c.newLevel) : String(def.desc[lang] ?? '');\n            return <button key={i} onClick={() => onPick(c)} className=\"p-5 rounded-xl bg-[#e8dcc0] border border-[#4a7a8a]/30 hover:border-[#4a7a8a]/60 hover:scale-105 transition-all text-left\"><div className=\"text-[#4a7a8a] text-xs uppercase mb-1\">{def.category === 'active' ? t('active') : t('passive')}</div><div className=\"font-bold text-lg mb-2\">{def.name[lang]}</div><div className=\"text-sm text-[#5a4a32] mb-2\">{desc}</div><div className=\"text-xs text-[#8a7a5a]/70\">{t('level')} {c.currentLevel} → {c.newLevel} / {def.maxLevel}</div></button>;";
if (app.includes(oldAbility)) app = app.replace(oldAbility, newAbility);

fs.writeFileSync(appPath, app);

const enginePath = 'src/engine.ts';
let engine = fs.readFileSync(enginePath, 'utf8');
engine = engine.replace(/\n\s*s\.pendingTowerUpgrade = null;/g, '');
engine = engine.replace(/\s*\|\|\s*s\.pendingTowerUpgrade/g, '');
fs.writeFileSync(enginePath, engine);

console.log('Level-up runtime hardening applied');
