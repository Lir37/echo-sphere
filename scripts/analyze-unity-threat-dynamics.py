"""Summarize matched production C# threat probes; never infer full-run balance."""
import json,hashlib,statistics,sys
from pathlib import Path
root=Path(sys.argv[1]);out=Path(sys.argv[2]);before=json.loads((root/'before.json').read_text());after=json.loads((root/'after.json').read_text());
def key(x):return tuple(x[k] for k in ('region','stage','seed','strong','centerX','centerY'))
b={key(x):x for x in before};a={key(x):x for x in after};assert len(b)==len(a)==1080 and b.keys()==a.keys()
def summary(rows):
 shots=[x['firstShot'] for x in rows if x['firstShot']>=0];dead=[x['killedAt'] for x in rows if x['killedAt']>=0]
 return dict(cases=len(rows),fired=len(shots),deadBeforeFirstShot=sum(x['killedAt']>=0 and x['projectileSpeed']>0 and x['firstShot']<0 for x in rows),meanFirstShotSeconds=statistics.mean(shots) if shots else None,meanKilledSeconds=statistics.mean(dead) if dead else None,killedCases=len(dead),meanDamage=statistics.mean(x['damage'] for x in rows) if rows else None,meanSpeed=statistics.mean(x['speed'] for x in rows) if rows else None,meanPreparedExecutions=statistics.mean(x['attacks'] for x in rows) if rows else None)
result={'scope':'Matched actual C# isolated 20-second spawn-versus-Standard arenas. Not full runs, all builds or human acceptance. Dead-time statistics exclude survivors at20s.','casesBefore':len(b),'casesAfter':len(a),'hashes':{p:hashlib.sha256((root/p).read_bytes()).hexdigest() for p in ('before.json','after.json')},'regions':[],'roleBuckets':[]}
for region in range(6):
 keys=[k for k in b if b[k]['region']==region and b[k]['projectileSpeed']>0 and a[k]['projectileSpeed']>0 and a[k]['role']==b[k]['role']]
 common=[k for k in keys if b[k]['firstShot']>=0 and a[k]['firstShot']>=0]
 result['regions'].append(dict(region=region,matchedExistingShooters={'before':summary([b[k] for k in keys]),'after':summary([a[k] for k in keys])},commonFiredCases=len(common),meanFirstShotChangeSeconds=statistics.mean(a[k]['firstShot']-b[k]['firstShot'] for k in common) if common else None))
for label,roles in [('fast_roles',{'swarmer','charger','phase','scavenger'}),('heavy_roles',{'tank_guard','anchor'}),('other_roles',set())]:
 keys=[k for k in b if b[k]['role']==a[k]['role'] and (b[k]['role'] in roles if roles else b[k]['role'] not in {'swarmer','charger','phase','scavenger','tank_guard','anchor'})]
 result['roleBuckets'].append(dict(label=label,before=summary([b[k] for k in keys]),after=summary([a[k] for k in keys])))
keys=[k for k in b if b[k]['elite'] and a[k]['elite'] and b[k]['role']==a[k]['role']];result['eliteCases']={'before':summary([b[k] for k in keys]),'after':summary([a[k] for k in keys])}
if (root/'packs-after.json').exists():
 packs=json.loads((root/'packs-after.json').read_text());result['packs']={'cases':len(packs),'scope':'Six actual spawns vs three legal Standard/Chain/Aura nodes with actual network,100HP,20s, scripted movement or stationary; no waves/terrain/acquisition budget. Fast cosmetics use existing arena policy, not a production quality setting.','sha256':hashlib.sha256((root/'packs-after.json').read_bytes()).hexdigest(),'regions':[{'region':r,'strong':v,'cases':len(q),'survived':sum(x['survived'] for x in q),'meanDamage':statistics.mean(x['damage'] for x in q),'meanInitialKills':statistics.mean(x['killed'] for x in q),'peakSummons':max(x['peakSummons'] for x in q)} for r in range(6) for v in (False,True) for q in [[x for x in packs if x['region']==r and x['strong']==v]]]}
out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(result,indent=2),encoding='utf-8');print(json.dumps(result,indent=2))
