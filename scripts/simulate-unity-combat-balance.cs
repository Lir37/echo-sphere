// Run with Unity CLI run_script, entry CombatBalanceSimulation.Main, in Play Mode.
// Controlled production-code arenas, NOT a full-run win-rate or player-fun model.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
using Newtonsoft.Json;

public static class CombatBalanceSimulation
{
    const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static readonly Type R = typeof(EchoSphereRuntime);
    static readonly SphereId[] Families = (SphereId[])Enum.GetValues(typeof(SphereId));
    static readonly RunUpgradeKind[] Abilities = { RunUpgradeKind.Teleport, RunUpgradeKind.TimeStop,
        RunUpgradeKind.Blast, RunUpgradeKind.Shield, RunUpgradeKind.FireTrail, RunUpgradeKind.Minion,
        RunUpgradeKind.Lightning, RunUpgradeKind.DarkRitual };
    static void Set(object o, string n, object v) => o.GetType().GetField(n,F).SetValue(o,v);
    static T Get<T>(object o,string n) => (T)o.GetType().GetField(n,F).GetValue(o);
    static object Call(object o,string n,params object[] v) => o.GetType().GetMethod(n,F).Invoke(o,v);
    sealed class State { public SphereId Family; public int Level; public string Branch,Final;
        public override string ToString() => Family+":"+Level+":"+Branch+":"+Final; }
    static List<State> States()
    {
        var result=new List<State>();
        foreach(var family in Families) {
            // Level 0 is physically placeable in the production game too.
            for(int l=0;l<4;l++) result.Add(new State{Family=family,Level=l});
            foreach(var b in SphereEvolutionCatalog.GetBranches(family)) {
                for(int l=4;l<7;l++)result.Add(new State{Family=family,Level=l,Branch=b.Id});
                foreach(var f in SphereEvolutionCatalog.GetFinals(b.Id))
                    result.Add(new State{Family=family,Level=7,Branch=b.Id,Final=f.Id});
            }
        }
        return result;
    }
    // Each fixture owns a fresh disabled runtime. Start/Update, autosave, reward
    // settlement and meta progression are never called. Existing scene state is untouched.
    sealed class Arena : IDisposable
    {
        public EchoSphereRuntime Runtime; public PlayerCoreController2D Core;
        public bool FastParticles, NetworkTick, TracePhases;
        void Trace(string phase){if(TracePhases)File.WriteAllText(Path.GetFullPath("Library/combat-balance/active-phase.txt"),phase);}
        public List<SphereAttackAgent> Spheres; public List<EnemyAgent2D> Enemies;
        readonly List<GameObject> owned=new List<GameObject>();
        public Arena(int region=0,uint seed=1) {
            var go=Make("Isolated balance runtime");go.SetActive(false);
            Runtime=go.AddComponent<EchoSphereRuntime>();Runtime.enabled=false;go.SetActive(true);
            var p=Make("Balance player");Core=p.AddComponent<PlayerCoreController2D>();
            Core.SetMaxHpForNewRun(100000);
            Set(Runtime,"_player",p.transform);Set(Runtime,"_playerCore",Core);
            Set(Runtime,"_mainMenu",false);Set(Runtime,"_paused",false);Set(Runtime,"_tutorialStep",-1);
            Set(Runtime,"_runEconomy",RunEconomyRules.Create(seed));Set(Runtime,"_selectedRegion",(EchoRegionId)region);
            // Explicitly bypass loading the user's persistent upgrades.
            Set(Runtime,"_shopSaveLoaded",true);
            Set(Runtime,"_shopSave",Activator.CreateInstance(R.GetNestedType("ShopSaveData",BindingFlags.NonPublic)));
            Get<SeededRng>(Runtime,"_rng").RestoreState(seed);
            Spheres=Get<List<SphereAttackAgent>>(Runtime,"_spheres");
            Enemies=Get<List<EnemyAgent2D>>(Runtime,"_enemies");
        }
        GameObject Make(string name){var go=new GameObject(name);owned.Add(go);return go;}
        public SphereAttackAgent Sphere(State s) {
            Get<Dictionary<SphereId,int>>(Runtime,"_sphereLevels")[s.Family]=s.Level;
            var branches=Get<Dictionary<SphereId,string>>(Runtime,"_sphereBranches");branches[s.Family]=s.Branch;
            var finals=Get<Dictionary<SphereId,string>>(Runtime,"_sphereFinals");finals[s.Family]=s.Final;
            var agent=Make("Balance sphere").AddComponent<SphereAttackAgent>();
            Spheres.Add(agent);agent.Initialize(Runtime,Core.transform,s.Family,12);agent.ApplyProgressionLevel(s.Level);return agent;
        }
        public EnemyAgent2D Enemy(Vector2 pos,int region,string role="grunt",float speed=0) {
            var go=Make("Balance target");go.transform.position=pos;
            var e=go.AddComponent<EnemyAgent2D>();
            e.Initialize(Runtime,Core.transform,1000000,speed,1,"wisp","",false,EnemyArchetype.Normal,false,role,1,1,"",0);
            e.SetCampaignRegion((EchoRegionId)region);Enemies.Add(e);Call(Runtime,"RegisterEnemyQuery",e);return e;
        }
        public void Tick(float dt, bool tickSpheres=true) {
            Trace("time/core");
            Set(Runtime,"_runTime",Runtime.CurrentRunTime+dt);
            R.GetProperty("SimulationDeltaTime",F).SetValue(Runtime,dt);
            Core.Tick(dt);
            if(NetworkTick){Trace("network analysis");Call(Runtime,"RefreshNetworkState");Trace("resonance ring");Call(Runtime,"TickResonanceRing",dt);}
            if(tickSpheres)foreach(var s in Spheres){Trace("sphere "+s.Type);s.SimulationTick(dt);}
            Trace("projectiles");
            Phase(typeof(ProjectileAgent),dt,true);
            Trace("drones");
            Phase(typeof(AbilityDroneAgent2D),dt,true);
            Trace("enemies");
            for(int i=Enemies.Count-1;i>=0;i--)Enemies[i].SimulationTick(dt);
            Trace("enemy shots");
            Phase(typeof(BossProjectileAgent2D),dt,true);
            Trace("particles");
            Phase(typeof(SourceParticle2D),dt,true);
            Trace("lightning");
            Call(Runtime,"TickSourceLightningVisuals",dt);
            // Optional headless-only retirement of cosmetic particles. Their
            // production creation/RNG still runs; no Android setting is changed.
            if(FastParticles)foreach(var agent in Get<List<MonoBehaviour>>(Runtime,"_simulationAgents"))
                if(agent is SourceParticle2D)Call(Runtime,"RetireNativeTransient",agent);
            var pool=Get<object>(Runtime,"_nativeTransientPools");if(pool!=null)Call(pool,"Flush");
            Trace("tick complete");
        }
        public void CheckPlacement() {
            if(Spheres.Count>(int)Call(Runtime,"GetCurrentMaxSpheres"))throw new Exception("Fixture exceeds actual sphere capacity");
            foreach(var s in Spheres)
                if(!(bool)Call(Runtime,"CanPlaceSphereAt",(Vector2)s.transform.position,s))
                    throw new Exception("Invalid production placement in fixture: "+s.Type+" "+s.transform.position);
        }
        void Phase(Type t,float dt,bool reverse) => R.GetMethod("TickSimulationPhase",F).MakeGenericMethod(t).Invoke(Runtime,new object[]{dt,reverse});
        public void Dispose() {
            // Include inactive pooled objects, which are no longer in simulation lists.
            var all=new HashSet<GameObject>(owned);
            foreach(var agent in Get<List<MonoBehaviour>>(Runtime,"_simulationAgents"))if(agent!=null)all.Add(agent.gameObject);
            var pool=Get<object>(Runtime,"_nativeTransientPools");
            if(pool!=null)foreach(var a in Get<HashSet<MonoBehaviour>>(pool,"_owned"))if(a!=null)all.Add(a.gameObject);
            foreach(var go in all)if(go!=null && go!=Runtime.gameObject)UnityEngine.Object.DestroyImmediate(go);
            if(Runtime!=null) {
                // Thousands of fixture route-destroy messages need no normal-log
                // stack traces. Error/warning reporting remains unchanged.
                var trace=Application.GetStackTraceLogType(LogType.Log);
                try {Application.SetStackTraceLogType(LogType.Log,StackTraceLogType.None);UnityEngine.Object.DestroyImmediate(Runtime.gameObject);}
                finally {Application.SetStackTraceLogType(LogType.Log,trace);}
            }
        }
    }
    static void Write(string path,object value)=>File.WriteAllText(path,JsonConvert.SerializeObject(value,Formatting.Indented));
    public static object HailProbe() {
        var rows=new List<object>();
        foreach(bool modern in new[]{false,true})foreach(int mode in new[]{1,2,3})using(var a=new Arena(0,71)) {
            if(!modern)Set(a.Runtime,"_runEconomy",null);
            var s=a.Sphere(new State{Family=SphereId.Shotgun,Level=7,Branch="shotgun_hail",Final="shotgun_hail_final_"+mode});
            s.ApplyNetworkDisable(1000);var e=a.Enemy(Vector2.right*60*SpherePlacementRules.SourcePixelsToWorld,0);
            int attempts=0;
            while(Get<List<MonoBehaviour>>(a.Runtime,"_simulationAgents").OfType<ProjectileAgent>().Count()==0 && attempts++<100)
                s.ApplySourceBranchHit(e,100,false);
            int fragments=Get<List<MonoBehaviour>>(a.Runtime,"_simulationAgents").OfType<ProjectileAgent>().Count();
            if(fragments!=(mode==2?8:6))throw new Exception("Expected fragment fixture");
            var neighbor=a.Enemy(Vector2.right*90*SpherePlacementRules.SourcePixelsToWorld,0);
            a.Tick(1f/60,false);float firstTickDamage=1000000-e.CurrentHp;
            for(int i=1;i<40;i++)a.Tick(1f/60,false);
            float damage=1000000-e.CurrentHp,other=1000000-neighbor.CurrentHp;
            // Later secondary effects may legitimately return from the neighbor;
            // exclude only the immediate fragment hit, not the rest of the build.
            if(modern && (firstTickDamage!=0||other<=0))throw new Exception("Fragments must scatter: mode="+mode+" origin="+damage+" other="+other+" first="+firstTickDamage);
            if(!modern&&damage<=0)throw new Exception("Legacy fragment behavior changed");
            rows.Add(new{modern,mode,fragments,attempts,firstTickDamage,originTargetDamage=damage,neighborDamage=other});
        }
        Write(Path.GetFullPath("Library/combat-balance/hail-probe.json"),rows);return rows;
    }
    public static object TuningIsolation() {
        int checks=0;
        foreach(var s in States())using(var a=new Arena()) {
            var sphere=a.Sphere(s);var economy=Get<object>(a.Runtime,"_runEconomy");
            float modern=a.Runtime.GetSourceSphereAttackDamage(sphere);
            Set(a.Runtime,"_runEconomy",null);float legacy=a.Runtime.GetSourceSphereAttackDamage(sphere);
            float expected=s.Family==SphereId.Pulse?.60f+.20f*(Mathf.Clamp(s.Level,1,7)-1)/6:1;
            if(Math.Abs(modern/legacy-expected)>.00001)throw new Exception("Campaign isolation "+s);
            Set(a.Runtime,"_runEconomy",economy);checks++;
        }
        var result=new{result="PASS",checks,scope="Actual runtime damage, every valid sphere state, campaign-only Pulse tuning and neutral other families/legacy."};
        Write(Path.GetFullPath("Library/combat-balance/tuning-isolation.json"),result);return result;
    }
    public static object TriangleProbe() {
        var rows=new List<object>();
        foreach(var fixture in new[]{(modern:true,targets:5),(modern:true,targets:2),(modern:false,targets:2)})using(var a=new Arena()) {
            if(!fixture.modern)Set(a.Runtime,"_runEconomy",null);
            SphereAttackAgent owner=null;int at=0;
            foreach(var family in new[]{SphereId.Standard,SphereId.Chain,SphereId.Aura}) {
                var s=a.Sphere(new State{Family=family,Level=1});if(owner==null)owner=s;
                s.transform.position=new Vector2(Mathf.Cos(at*2*Mathf.PI/3),Mathf.Sin(at*2*Mathf.PI/3))*90*SpherePlacementRules.SourcePixelsToWorld;at++;
            }
            Call(a.Runtime,"RefreshNetworkState");
            if(a.Runtime.GetFormationBonus(SphereNetworkFormation.Triangle,0)<=0)throw new Exception("Triangle fixture not active");
            for(int i=0;i<fixture.targets;i++)a.Enemy(Vector2.right*170*SpherePlacementRules.SourcePixelsToWorld,0);
            for(int i=0;i<3;i++)a.Enemies[0].ReceiveSphereDamage(owner,10,true,false);
            int counter=Get<int>(owner,"_sourceTriangleHits"),expected=fixture.modern?3:4;
            if(counter!=expected||a.Enemies.Skip(1).Any(e=>e.CurrentHp>=1000000))throw new Exception("Triangle bounded cascade or legacy mismatch");
            var neighborHp=a.Enemies.Skip(1).Select(e=>e.CurrentHp).ToArray();
            a.Enemies[0].ReceiveSphereDamage(owner,10,true,false);
            if(!neighborHp.SequenceEqual(a.Enemies.Skip(1).Select(e=>e.CurrentHp))||Get<int>(owner,"_sourceTriangleHits")!=expected+1||Get<bool>(a.Runtime,"_sourceTrianglePulseActive"))throw new Exception("Triangle guard did not restore between primary hits");
            rows.Add(new{fixture.modern,fixture.targets,primaryHits=4,triangleCount=Get<int>(owner,"_sourceTriangleHits"),neighborDamage=1000000-a.Enemies[1].CurrentHp});
        }
        var result=new{result="PASS",checks=rows.Count,cases=rows,scope="Actual damage pipeline: campaign dense/sparse Triangle pulses terminate, next primary still counts, sparse legacy recursion/cadence unchanged."};
        Write(Path.GetFullPath("Library/combat-balance/triangle-regression.json"),result);return result;
    }
    public static object Effects() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");Directory.CreateDirectory(output);
        var rows=new List<object>();int casts=0;
        foreach(var kind in Abilities) {
            var variants=new List<(int level,string branch,string final)>();
            for(int l=1;l<4;l++)variants.Add((l,null,null));
            foreach(var b in LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(kind,false)) {
                for(int l=4;l<7;l++)variants.Add((l,b.Id,null));
                foreach(var f in LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(kind,true,b.Id))variants.Add((7,b.Id,f.Id));
            }
            for(int region=0;region<6;region++)foreach(var v in variants)using(var a=new Arena(region,23)) {
                a.Core.SetMaxHpForNewRun(100);
                Get<Dictionary<RunUpgradeKind,int>>(a.Runtime,"_abilityLevels")[kind]=v.level;
                Get<Dictionary<RunUpgradeKind,string>>(a.Runtime,"_abilityBranches")[kind]=v.branch;
                Get<Dictionary<RunUpgradeKind,string>>(a.Runtime,"_abilityFinals")[kind]=v.final;
                int i=0;
                foreach(var family in new[]{SphereId.Standard,SphereId.Chain,SphereId.Aura}) {
                    var b=SphereEvolutionCatalog.GetBranches(family)[0];var f=SphereEvolutionCatalog.GetFinals(b.Id)[0];
                    var sphere=a.Sphere(new State{Family=family,Level=7,Branch=b.Id,Final=f.Id});
                    sphere.transform.position=new Vector2(Mathf.Cos(i*2*Mathf.PI/3),Mathf.Sin(i*2*Mathf.PI/3))*1.6f;
                    sphere.ApplyNetworkDisable(1000);i++;
                }
                Call(a.Runtime,"RefreshNetworkState");
                for(i=0;i<8;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*60*SpherePlacementRules.SourcePixelsToWorld,region);
                var before=a.Core.transform.position;float raw=a.Runtime.GetSourceSphereAttackDamage(a.Spheres[0]);
                Call(a.Runtime,"ActivateActiveAbility",kind);
                string name=char.ToLowerInvariant(kind.ToString()[0])+kind.ToString().Substring(1);
                float cooldown=Get<float>(a.Runtime,"_"+name+"Cooldown");
                if(cooldown<=0||float.IsInfinity(cooldown)||float.IsNaN(cooldown))throw new Exception("Cast failed "+kind+" "+v);
                float buff=a.Runtime.GetSourceSphereAttackDamage(a.Spheres[0])/raw;
                int shields=a.Core.ShieldCharges;float invulnerable=a.Core.InvulnerableTimer;
                float hp=a.Core.CurrentHp;
                // Cast + first 2s only; sphere attack phases are omitted (network
                // disable alone does not suppress the sphere's own weapon).
                // Buff/cooldown expiration is not advanced;
                // this is an activation/early-effect probe, not sustained DPS.
                for(i=0;i<120;i++)a.Tick(1f/60,false);
                rows.Add(new{kind=kind.ToString(),v.level,v.branch,v.final,region,cooldown,
                    damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp),hpCost=100-hp,shields,invulnerable,
                    displacement=(a.Core.transform.position-before).magnitude,buff,
                    frozen=a.Enemies.Count(e=>e.IsFrozen),agents=Get<List<MonoBehaviour>>(a.Runtime,"_simulationAgents").Count});casts++;
            }
        }
        Write(output+"/abilities.json",rows);
        var result=new{result="COMPLETE",casts,scope="All legal active ability levels/branches/finals, six regional fixtures, cast and first 2s with three linked disabled spheres and eight high-HP targets. No sustained cooldown/uptime or full run."};
        Write(output+"/abilities-summary.json",result);return result;
    }
    public static object Regions() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");Directory.CreateDirectory(output);var rows=new List<object>();
        string[] roles={"grunt","ranged","sniper","bomber","corruptor","healer","tank_guard","charger","phase","anchor","leech","disruptor","splitter"};
        for(int region=0;region<6;region++)foreach(string role in roles)foreach(bool mobile in new[]{false,true})using(var a=new Arena(region,31)) {
            var e=a.Enemy(Vector2.right*220*SpherePlacementRules.SourcePixelsToWorld,region,role,70*SpherePlacementRules.SourcePixelsToWorld);
            Set(e,"_damage",9f);float min=float.MaxValue,max=0;int retreat=0;
            for(int i=0;i<1800;i++) {
                if(mobile)a.Core.transform.position=new Vector2(Mathf.Cos(i/60f*.5f),Mathf.Sin(i/60f*.5f))*50*SpherePlacementRules.SourcePixelsToWorld;
                var before=(Vector2)e.transform.position;var toward=a.Runtime.PlayerPosition-before;a.Tick(1f/60);
                if(Vector2.Dot((Vector2)e.transform.position-before,toward)<-.00001f)retreat++;
                float d=Vector2.Distance(e.transform.position,a.Runtime.PlayerPosition)/SpherePlacementRules.SourcePixelsToWorld;
                min=Math.Min(min,d);max=Math.Max(max,d);
            }
            // Outward displacement includes the source's 10px contact push;
            // it is not a count of AI retreat commands.
            rows.Add(new{region,role,mobile,seconds=30,damageTaken=100000-a.Core.CurrentHp,minDistance=min,maxDistance=max,outwardDisplacementFrames=retreat,
                projectiles=Get<List<MonoBehaviour>>(a.Runtime,"_simulationAgents").Count(x=>x is BossProjectileAgent2D)});
        }
        Write(output+"/regions.json",rows);var result=new{result="COMPLETE",arenas=rows.Count,
            scope="Six regions, thirteen isolated roles, stationary and circular-moving player, 30s at 60Hz, real movement/attacks/contact/shots; no waves, bosses, hazards or survival win-rate."};
        Write(output+"/regions-summary.json",result);return result;
    }
    public static object ArtifactCombat() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        FastVisualIsolation();
        string output=Path.GetFullPath("Library/combat-balance");Directory.CreateDirectory(output);
        var sets=JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText(output+"/artifact-builds.json"));
        var states=States().Where(s=>s.Level==7).ToArray();var rows=new List<object>();
        // Every pair has a real combat probe, distributed across families, finals
        // and regions. This is explicitly NOT each pair x every combat context.
        int start=File.Exists(output+"/artifact-start.txt")?int.Parse(File.ReadAllText(output+"/artifact-start.txt")):0;
        int end=Math.Min(start+512,Math.Min(sets.Count,4096+128));
        for(int index=start;index<end;index++) {
            int region=index%6;var s=states[index%states.Length];
            using(var a=new Arena(region,(uint)(101+index))) {
                a.FastParticles=true;
                Get<List<string>>(a.Runtime,"_ownedArtifacts").AddRange(sets[index]);var sphere=a.Sphere(s);
                for(int i=0;i<8;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*60*SpherePlacementRules.SourcePixelsToWorld,region);
                for(int i=0;i<1200;i++) {
                    a.Tick(1f/60);Call(a.Runtime,"TickSourceLatePlayerState",1f/60,Mathf.Min(4f,a.Core.MaxHp*.03f)/60);
                    Call(a.Runtime,"TickSourceDamageNumbers",1f/60);
                }
                var damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);
                if(double.IsNaN(damage)||double.IsInfinity(damage)||damage<0)throw new Exception("Invalid combat "+index);
                rows.Add(new{build=index,artifacts=sets[index],state=s.ToString(),region,seconds=20,damage,dps=damage/20,
                    frozen=a.Enemies.Count(e=>e.IsFrozen),shots=sphere.PresentationShotSerial});
            }
            if(index%50==0)Write(output+"/artifact-combat-progress.json",new{tested=index+1});
        }
        Write(output+"/artifact-combat-"+start+".json",rows);var result=new{result="COMPLETE",start,end,arenas=rows.Count,
            scope="Each singleton and pair plus first 128 sampled larger subsets, one assigned family/final/region per build, 20s eight-target arenas. No economy triggers, kills, full-run survival or exhaustive contextual interaction coverage."};
        Write(output+"/artifact-combat-summary-"+start+".json",result);return result;
    }
    public static object FastVisualIsolation() {
        int checks=0;
        foreach(var family in Families) {
            string reference=null;
            foreach(bool fast in new[]{false,true})using(var a=new Arena(0,53)) {
                a.FastParticles=fast;Get<List<string>>(a.Runtime,"_ownedArtifacts").AddRange(ArtifactRewardRules.Catalog(true).Select(x=>x.Id));
                var b=SphereEvolutionCatalog.GetBranches(family)[0];var f=SphereEvolutionCatalog.GetFinals(b.Id)[0];
                var s=a.Sphere(new State{Family=family,Level=7,Branch=b.Id,Final=f.Id});
                for(int i=0;i<8;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*60*SpherePlacementRules.SourcePixelsToWorld,0);
                for(int i=0;i<300;i++){a.Tick(1f/60);Call(a.Runtime,"TickSourceLatePlayerState",1f/60,Mathf.Min(4f,a.Core.MaxHp*.03f)/60);}
                string state=JsonConvert.SerializeObject(new{hp=a.Enemies.Select(e=>e.CurrentHp).ToArray(),
                    positions=a.Enemies.Select(e=>new[]{e.transform.position.x,e.transform.position.y}).ToArray(),
                    rng=Get<SeededRng>(a.Runtime,"_rng").State,timer=s.AttackTimer,player=a.Core.CurrentHp});
                if(!fast)reference=state;else {if(reference!=state)throw new Exception("Cosmetic isolation failed "+family);checks++;}
            }
        }
        var result=new{result="PASS",checks,scope="All ten families, first final, all artifacts as a stress fixture, 5s: exact target HP/positions, RNG, attack timer/player HP with normal vs early-retired cosmetic particles. Not a legal balance build."};
        Write(Path.GetFullPath("Library/combat-balance/visual-isolation.json"),result);return result;
    }
    public static object Wounded() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");var rows=new List<object>();
        foreach(var s in States().Where(s=>s.Level==7))foreach(int count in new[]{1,8})foreach(uint seed in new uint[]{17,29,71})using(var a=new Arena(0,seed)) {
            a.Sphere(s);
            for(int i=0;i<count;i++) {
                var e=a.Enemy(new Vector2(Mathf.Cos(i*2*Mathf.PI/count),Mathf.Sin(i*2*Mathf.PI/count))*60*SpherePlacementRules.SourcePixelsToWorld,0);
                // Boss-tagged training targets prevent ordinary Void instant kills
                // from invoking persistent death/reward hooks in an isolated probe.
                Set(e,"_bossType","balance_training_target");Set(e,"_hp",100000f);
            }
            var positions=a.Enemies.Select(e=>e.transform.position).ToArray();
            for(int i=0;i<1200;i++) {
                for(int k=0;k<a.Enemies.Count;k++){a.Enemies[k].transform.position=positions[k];Call(a.Runtime,"RefreshEnemyQuery",a.Enemies[k]);}
                a.Tick(1f/60);
            }
            if(a.Enemies.Any(e=>!e.IsAlive))throw new Exception("Target death invalidates non-persistent arena");
            double damage=a.Enemies.Sum(e=>100000d-e.CurrentHp);
            rows.Add(new{state=s.ToString(),targets=count,seed,seconds=20,hpFraction=.1,damage,dps=damage/20});
        }
        Write(output+"/wounded.json",rows);var result=new{result="COMPLETE",arenas=rows.Count,
            scope="Every final sphere branch, one/eight anchored boss-tagged training targets at 10% HP, three seeds, 20s. No boss controller, deaths/kill procs or full-run survival."};
        Write(output+"/wounded-summary.json",result);return result;
    }
    sealed class AbilityState { public RunUpgradeKind Kind;public string Branch,Final; }
    static IEnumerable<AbilityState[]> Loadouts() {
        var catalog=Abilities.Select(k=>LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(k,false)
            .SelectMany(b=>LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(k,true,b.Id)
                .Select(f=>new AbilityState{Kind=k,Branch=b.Id,Final=f.Id})).ToArray()).ToArray();
        for(int i=0;i<catalog.Length;i++)foreach(var a in catalog[i]) {
            yield return new[]{a};
            for(int j=i+1;j<catalog.Length;j++)foreach(var b in catalog[j]) {
                yield return new[]{a,b};
                for(int k=j+1;k<catalog.Length;k++)foreach(var c in catalog[k])yield return new[]{a,b,c};
            }
        }
    }
    public static object AbilityLoadouts() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");int tested=0,ordinal=0;double maxBurst=0;string[] maxLoadout=null;
        int start=File.Exists(output+"/ability-start.txt")?int.Parse(File.ReadAllText(output+"/ability-start.txt")):0;
        int end=Math.Min(start+4096,43164);
        using(var file=new StreamWriter(output+"/ability-loadouts-"+start+".jsonl"))foreach(var loadout in Loadouts()) {
            int at=ordinal++;if(at<start)continue;if(at>=end)break;
            using(var a=new Arena(at%6,(uint)(at+101))) {
            a.Core.SetMaxHpForNewRun(100);
            foreach(var v in loadout) {
                Get<Dictionary<RunUpgradeKind,int>>(a.Runtime,"_abilityLevels")[v.Kind]=7;
                Get<Dictionary<RunUpgradeKind,string>>(a.Runtime,"_abilityBranches")[v.Kind]=v.Branch;
                Get<Dictionary<RunUpgradeKind,string>>(a.Runtime,"_abilityFinals")[v.Kind]=v.Final;
            }
            int index=0;
            foreach(var family in new[]{SphereId.Standard,SphereId.Chain,SphereId.Aura}) {
                var b=SphereEvolutionCatalog.GetBranches(family)[0];var f=SphereEvolutionCatalog.GetFinals(b.Id)[0];
                var s=a.Sphere(new State{Family=family,Level=7,Branch=b.Id,Final=f.Id});
                s.transform.position=new Vector2(Mathf.Cos(index*2*Mathf.PI/3),Mathf.Sin(index*2*Mathf.PI/3))*1.6f;index++;
            }
            Call(a.Runtime,"RefreshNetworkState");
            for(int i=0;i<8;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*60*SpherePlacementRules.SourcePixelsToWorld,at%6);
            foreach(var v in loadout) {
                Call(a.Runtime,"ActivateActiveAbility",v.Kind);
                string name=char.ToLowerInvariant(v.Kind.ToString()[0])+v.Kind.ToString().Substring(1);
                if(Get<float>(a.Runtime,"_"+name+"Cooldown")<=0)throw new Exception("Loadout cast rejected "+v.Final);
            }
            double damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);var ids=loadout.Select(v=>v.Final).ToArray();
            if(double.IsNaN(damage)||double.IsInfinity(damage)||a.Enemies.Any(e=>!e.IsAlive))throw new Exception("Invalid loadout probe");
            if(damage>maxBurst){maxBurst=damage;maxLoadout=ids;}
            file.WriteLine(JsonConvert.SerializeObject(new{loadout=ids,region=at%6,immediateDamage=damage,hp=a.Core.CurrentHp,
                shields=a.Core.ShieldCharges,invulnerable=a.Core.InvulnerableTimer}));
            tested++;if(tested%1000==0)Write(output+"/ability-loadout-progress.json",new{tested});
            }
        }
        if(tested!=end-start)throw new Exception("Unexpected final loadout count "+tested);
        var result=new{result="COMPLETE",start,end,loadouts=tested,maxBurst,maxLoadout,
            scope="All unordered one/two/three distinct active-ability final loadouts, one canonical cast order and assigned region, three fixed sphere branches/eight targets, immediate activation only. No alternative orders, sustained DPS, upgrade affordability or win rates."};
        Write(output+"/ability-loadouts-summary-"+start+".json",result);return result;
    }
    public static object SpherePairs() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        PlacementProbe();
        string output=Path.GetFullPath("Library/combat-balance");var states=States().Where(s=>s.Level==7).ToArray();
        int start=File.Exists(output+"/sphere-pair-start.txt")?int.Parse(File.ReadAllText(output+"/sphere-pair-start.txt")):0;
        int end=Math.Min(start+512,3735),ordinal=0;var rows=new List<object>();
        for(int i=0;i<states.Length;i++)for(int j=i;j<states.Length;j++) {
            // Mutation choices are per family: copies cannot choose conflicting branches.
            if(i!=j&&states[i].Family==states[j].Family)continue;
            int at=ordinal++;if(at<start||at>=end)continue;
            using(var a=new Arena(at%6,(uint)(101+at))) {
                a.FastParticles=true;a.NetworkTick=true;
                Set(a.Runtime,"_level",20);Set(a.Runtime,"_mutationStage",PlayerLoopRules.MutationStage(20));
                a.Sphere(states[i]).transform.position=Vector2.left*70*SpherePlacementRules.SourcePixelsToWorld;
                a.Sphere(states[j]).transform.position=Vector2.right*70*SpherePlacementRules.SourcePixelsToWorld;
                for(int k=0;k<8;k++)a.Enemy(new Vector2(Mathf.Cos(k*Mathf.PI/4),Mathf.Sin(k*Mathf.PI/4))*140*SpherePlacementRules.SourcePixelsToWorld,at%6);
                a.CheckPlacement();
                for(int k=0;k<300;k++)a.Tick(1f/60);
                double damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);
                if(a.Enemies.Any(e=>!e.IsAlive)||double.IsNaN(damage)||double.IsInfinity(damage))throw new Exception("Invalid pair arena");
                rows.Add(new{build=at,left=states[i].ToString(),right=states[j].ToString(),region=at%6,seconds=5,damage,dps=damage/5,
                    resonance=Get<float>(a.Runtime,"_resonanceCharge")});
            }
            if(at%100==0)Write(output+"/sphere-pair-progress.json",new{tested=at+1});
        }
        if(ordinal!=3735||rows.Count!=end-start)throw new Exception("Pair coverage "+ordinal+" "+rows.Count);
        Write(output+"/sphere-pairs-"+start+".json",rows);
        var result=new{result="COMPLETE",start,end,arenas=rows.Count,
            scope="Every compatible unordered pair of final sphere states, including two identical copies: level-20 fixture, one assigned seed/region, legal fixed positions, eight free high-HP targets, network/Resonance ticked, first 5s. No full run, all placements or all artifact cross-products."};
        Write(output+"/sphere-pairs-summary-"+start+".json",result);return result;
    }
    public static object PlacementProbe() {
        int checks=0;
        for(int count=4;count<=8;count++)using(var a=new Arena()) {
            Set(Get<object>(Get<object>(a.Runtime,"_shopSave"),"upgrades"),"spheres",3);
            for(int i=0;i<count;i++)a.Sphere(new State{Family=Families[i],Level=1}).transform.position=
                new Vector2(Mathf.Cos(i*1.5f*Mathf.PI/(count-1)+Mathf.PI/4),Mathf.Sin(i*1.5f*Mathf.PI/(count-1)+Mathf.PI/4))*90*SpherePlacementRules.SourcePixelsToWorld;
            for(int i=0;i<12;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/6),Mathf.Sin(i*Mathf.PI/6))*160*SpherePlacementRules.SourcePixelsToWorld,0);
            a.CheckPlacement();checks++;
        }
        var result=new{result="PASS",checks,scope="Every ensemble size, actual placement/path/capacity checks; open 270-degree arc."};
        Write(Path.GetFullPath("Library/combat-balance/placement-probe.json"),result);return result;
    }
    public static object Ensembles() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");var random=new System.Random(20261102);var rows=new List<object>();
        var ids=ArtifactRewardRules.Catalog(true).Select(x=>x.Id).ToArray();
        int start=File.Exists(output+"/ensemble-start.txt")?int.Parse(File.ReadAllText(output+"/ensemble-start.txt")):0;
        int batchSize=File.Exists(output+"/ensemble-count.txt")?int.Parse(File.ReadAllText(output+"/ensemble-count.txt")):32;
        if(start<0||start>=128||batchSize<1||batchSize>32)throw new Exception("Invalid ensemble batch");
        int end=Math.Min(start+batchSize,128);
        for(int build=0;build<128;build++) {
            var chosen=Families.OrderBy(_=>random.Next()).Take(4+random.Next(5)).Select(f=>{
                var b=SphereEvolutionCatalog.GetBranches(f)[random.Next(3)];var final=SphereEvolutionCatalog.GetFinals(b.Id)[random.Next(3)];
                return new State{Family=f,Level=7,Branch=b.Id,Final=final.Id};}).ToArray();
            var artifacts=ids.OrderBy(_=>random.Next()).Take(3+random.Next(10)).ToArray();
            if(build<start||build>=end)continue;
            foreach(uint seed in new uint[]{17,29,71})using(var a=new Arena(build%6,seed)) {
                Write(output+"/active-ensemble.json",new{build,seed,phase="setup",spheres=chosen.Select(s=>s.ToString()).ToArray(),artifacts});
                a.FastParticles=true;a.NetworkTick=true;
                Set(a.Runtime,"_level",60);Set(a.Runtime,"_mutationStage",PlayerLoopRules.MutationStage(60));
                // Only the existing persistent capacity upgrade; no damage/meta boosts.
                Set(Get<object>(Get<object>(a.Runtime,"_shopSave"),"upgrades"),"spheres",3);
                Get<List<string>>(a.Runtime,"_ownedArtifacts").AddRange(artifacts);
                for(int i=0;i<chosen.Length;i++)a.Sphere(chosen[i]).transform.position=
                    new Vector2(Mathf.Cos(i*1.5f*Mathf.PI/(chosen.Length-1)+Mathf.PI/4),Mathf.Sin(i*1.5f*Mathf.PI/(chosen.Length-1)+Mathf.PI/4))*90*SpherePlacementRules.SourcePixelsToWorld;
                for(int i=0;i<12;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/6),Mathf.Sin(i*Mathf.PI/6))*160*SpherePlacementRules.SourcePixelsToWorld,build%6);
                a.CheckPlacement();
                for(int i=0;i<600;i++){
                    a.TracePhases=i==0;
                    if(i%60==0)Write(output+"/active-ensemble.json",new{build,seed,phase="simulation",tick=i,spheres=chosen.Select(s=>s.ToString()).ToArray(),artifacts});
                    a.Tick(1f/60);Call(a.Runtime,"TickSourceLatePlayerState",1f/60,Mathf.Min(4f,a.Core.MaxHp*.03f)/60);
                }
                if(a.Enemies.Any(e=>!e.IsAlive))throw new Exception("Target died in ensemble");
                double damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);
                rows.Add(new{build,seed,region=build%6,seconds=10,spheres=chosen.Select(s=>s.ToString()).ToArray(),artifacts,damage,dps=damage/10,
                    resonance=Get<float>(a.Runtime,"_resonanceCharge")});
                Write(output+"/active-ensemble.json",new{build,seed,phase="dispose",spheres=chosen.Select(s=>s.ToString()).ToArray(),artifacts});
                Write(output+"/ensembles-"+start+"-partial.json",rows);
            }
            if(build%10==0)Write(output+"/ensemble-progress.json",new{builds=build+1});
        }
        if(rows.Count!=(end-start)*3)throw new Exception("Ensemble batch coverage");
        Write(output+"/ensembles-"+start+".json",rows);var result=new{result="COMPLETE",start,end,builds=end-start,arenas=rows.Count,
            scope="Seeded level-60 fixtures: 4-8 distinct final spheres plus 3-12 artifacts, capacity-only shop upgrade III, three seeds, assigned region, production-validated open-arc placement, twelve targets, network/Resonance ticked, 10s. No active casts, waves, kill procs, affordability or survival outcome."};
        Write(output+"/ensembles-summary-"+start+".json",result);
        if(end==128) {
            var all=new Newtonsoft.Json.Linq.JArray();
            foreach(int batch in new[]{0,32,64,96})foreach(var row in Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(output+"/ensembles-"+batch+".json")))all.Add(row);
            if(all.Count!=384||all.Select(r=>(int)r["build"]+":"+(uint)r["seed"]).Distinct().Count()!=384)throw new Exception("Combined ensemble coverage");
            Write(output+"/ensembles.json",all);
            Write(output+"/ensembles-summary.json",new{result="COMPLETE",builds=128,arenas=384,scope=result.scope});
        }
        return result;
    }
    public static object Replays() {
        var cases=new[]{
            (SphereId.Standard,"standard_swarm","standard_swarm_final_3",new[]{"predator_claw","lone_bastion"}),
            (SphereId.Shotgun,"shotgun_burst","shotgun_burst_final_2",new[]{"lone_bastion","zero_sphere"}),
            (SphereId.Gravity,"gravity_tide","gravity_tide_final_3",new[]{"dragon_heart","gravity_hook"}),
            (SphereId.Pulse,"pulse_wave","pulse_wave_final_2",new[]{"crystal_speed","pulse_crown"}),
            (SphereId.Sniper,"sniper_assassin","sniper_assassin_final_1",new[]{"overclock","void_contract"})};
        var rows=new List<object>();
        foreach(var c in cases)foreach(uint seed in new uint[]{17,29,71})foreach(bool equipped in new[]{false,true})using(var a=new Arena(0,seed)) {
            a.FastParticles=true;if(equipped)Get<List<string>>(a.Runtime,"_ownedArtifacts").AddRange(c.Item4);
            var s=a.Sphere(new State{Family=c.Item1,Level=7,Branch=c.Item2,Final=c.Item3});
            for(int i=0;i<8;i++)a.Enemy(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*60*SpherePlacementRules.SourcePixelsToWorld,0);
            for(int i=0;i<1200;i++){a.Tick(1f/60);Call(a.Runtime,"TickSourceLatePlayerState",1f/60,Mathf.Min(4f,a.Core.MaxHp*.03f)/60);}
            double damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);
            rows.Add(new{state=c.Item3,artifacts=c.Item4,seed,equipped,seconds=20,damage,dps=damage/20,
                damageTakenMultiplier=(float)Call(a.Runtime,"GetArtifactOnlyDamageTakenMultiplier"),
                endDistance=a.Enemies.Average(e=>e.transform.position.magnitude/SpherePlacementRules.SourcePixelsToWorld)});
        }
        Write(Path.GetFullPath("Library/combat-balance/replays.json"),rows);var result=new{result="COMPLETE",arenas=rows.Count,
            scope="Five suspected strong/weak artifact pairs replayed with/without equipment on the same three seeds, free targets, 20s. Conditional control effects and defensive/economy value must not be ranked by DPS alone."};
        Write(Path.GetFullPath("Library/combat-balance/replays-summary.json"),result);return result;
    }
    // This sweep evaluates production stat getters. Conditional procs, aiming,
    // target health and geometry are not represented by this analytic score.
    public static object Stats() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        string output=Path.GetFullPath("Library/combat-balance");Directory.CreateDirectory(output);
        var ids=ArtifactRewardRules.Catalog(true).Select(x=>x.Id).ToArray();
        var sets=new List<string[]>{Array.Empty<string>()};
        foreach(var id in ids)sets.Add(new[]{id});
        for(int i=0;i<ids.Length;i++)for(int j=i+1;j<ids.Length;j++)sets.Add(new[]{ids[i],ids[j]});
        int pairs=ids.Length*(ids.Length-1)/2;
        var random=new System.Random(20261102);
        // Unique subsets, sampled without replacement inside each build.
        var unique=new HashSet<string>(sets.Select(x=>string.Join("|",x.OrderBy(y=>y))));
        while(sets.Count<1+ids.Length+pairs+1024) {
            var values=ids.OrderBy(_=>random.Next()).Take(3+random.Next(21)).OrderBy(x=>x).ToArray();
            if(unique.Add(string.Join("|",values)))sets.Add(values);
        }
        int checks=0;var extremes=new List<object>();
        using(var file=new StreamWriter(output+"/stats.csv")) {
            file.WriteLine("state,build,artifacts,damage,interval,range,analyticDps");
            foreach(var s in States())using(var a=new Arena()) {
                var sphere=a.Sphere(s);var owned=Get<List<string>>(a.Runtime,"_ownedArtifacts");
                double min=double.PositiveInfinity,max=0;int minBuild=0,maxBuild=0;
                for(int i=0;i<sets.Count;i++) {
                    owned.Clear();owned.AddRange(sets[i]);sphere.ApplyProgressionLevel(s.Level);
                    float damage=a.Runtime.GetSourceSphereAttackDamage(sphere), interval=sphere.EffectiveAttackInterval;
                    double dps=sphere.GetSourceDpsEstimate(damage,interval,true);
                    if(float.IsNaN(damage)||float.IsInfinity(damage)||damage<=0||interval<=0||float.IsNaN(interval)||float.IsInfinity(interval)||double.IsNaN(dps)||double.IsInfinity(dps)||dps<0)
                        throw new Exception("Invalid stats "+s+" build "+i);
                    if(dps<min){min=dps;minBuild=i;}if(dps>max){max=dps;maxBuild=i;}
                    file.WriteLine(FormattableString.Invariant($"{s},{i},{sets[i].Length},{damage:R},{interval:R},{sphere.AttackRange:R},{dps:R}"));checks++;
                }
                extremes.Add(new{state=s.ToString(),minimum=min,minBuild,weak=sets[minBuild],maximum=max,maxBuild,strong=sets[maxBuild]});
                Write(output+"/stats-progress.json",new{checks,state=s.ToString()});
            }
        }
        Write(output+"/artifact-builds.json",sets);Write(output+"/stat-extremes.json",extremes);
        var result=new{result="COMPLETE",checks,artifacts=ids.Length,pairs,largeSubsets=1024,states=States().Count,
            scope="Analytic production stats, one sphere, no network/active buffs, every singleton and pair for every legal sphere state, plus deterministic larger subsets. NOT combat outcomes."};
        Write(output+"/stats-summary.json",result);return result;
    }
    public static object Main() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
        bool paused=UnityEditor.EditorApplication.isPaused;UnityEditor.EditorApplication.isPaused=true;
        string output=Path.GetFullPath("Library/combat-balance");Directory.CreateDirectory(output);
        try {
            string mode=File.Exists(output+"/mode.txt")?File.ReadAllText(output+"/mode.txt").Trim():"smoke";
            var states=States();var rows=new List<object>();int tested=0;
            // Exhaustive legal family/level/evolution states. Independent regional
            // movement arenas below avoid pretending identical targets are six trials.
            IEnumerable<State> selected=states;
            if(mode=="smoke")selected=states.Where(s=>s.Level==1);
            if(mode=="changed")selected=states.Where(s=>s.Family==SphereId.Pulse||s.Family==SphereId.Shotgun);
            if(mode=="unlevelled")selected=states.Where(s=>s.Level==0);
            foreach(var s in selected) {
                foreach(int count in new[]{1,8}) foreach(bool anchored in mode=="smoke"?new[]{false}:new[]{false,true}) foreach(uint seed in mode=="smoke"?new uint[]{17}:new uint[]{17,29,71}) {
                    using(var a=new Arena(0,seed)) {
                        var sphere=a.Sphere(s);
                        for(int i=0;i<count;i++) {
                            // Close, distributed targets exercise both area and contact weapons.
                            float angle=count==1?0:2*Mathf.PI*i/count;
                            float radius=60;
                            a.Enemy(new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*SpherePlacementRules.SourcePixelsToWorld,0);
                        }
                        var positions=a.Enemies.Select(e=>e.transform.position).ToArray();
                        const float dt=1f/60;const int ticks=1200;
                        for(int i=0;i<ticks;i++) {
                            if(anchored)for(int k=0;k<a.Enemies.Count;k++) {a.Enemies[k].transform.position=positions[k];Call(a.Runtime,"RefreshEnemyQuery",a.Enemies[k]);}
                            a.Tick(dt);
                        }
                        double damage=a.Enemies.Sum(e=>1000000d-e.CurrentHp);
                        rows.Add(new{state=s.ToString(),targets=count,anchored,seed,seconds=ticks*dt,damage,dps=damage/(ticks*dt),
                            interval=sphere.EffectiveAttackInterval,range=sphere.AttackRange/SpherePlacementRules.SourcePixelsToWorld,
                            shots=sphere.PresentationShotSerial,remainingHp=a.Enemies.Select(e=>e.CurrentHp).ToArray(),
                            endDistance=a.Enemies.Average(e=>e.transform.position.magnitude/SpherePlacementRules.SourcePixelsToWorld)});
                    }
                    tested++;if(tested%20==0)Write(output+"/progress.json",new{tested,state=s.ToString()});
                }
            }
            if(mode=="changed") {
                var old=Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(output+"/before/arenas.json"));
                foreach(var row in old)if(!((string)row["state"]).StartsWith("Shotgun:")&&!((string)row["state"]).StartsWith("Pulse:"))rows.Add(row);
            }
            if(mode=="unlevelled") {
                var old=Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(output+"/arenas.json"));
                foreach(var row in old)if(((string)row["state"]).Split(':')[1]!="0")rows.Add(row);
            }
            Write(output+"/arenas.json",rows);
            var result=new{result="COMPLETE",mode,arenas=tested,legalSphereStates=states.Count,
                scope="Production SimulationTick in isolated 20s arenas at 60Hz: sphere, projectiles, drones, enemies, enemy shots and particles. No waves, input, drops, economy or full-run win-rate."};
            Write(output+"/summary.json",result);return result;
        } finally {UnityEditor.EditorApplication.isPaused=paused;}
    }

    public static object Threat() {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
        string dir="Library/combat-balance/threat";Directory.CreateDirectory(dir);
        string label=File.Exists(dir+"/label.txt")?File.ReadAllText(dir+"/label.txt").Trim():"before";
        var rows=new List<object>();
        foreach(int region in Enumerable.Range(0,6))foreach(int stage in new[]{0,900,1700})foreach(uint seed in new uint[]{0x9e3779b9u,0x3c6ef372u,0xdaa66d2bu,0x78dde6e4u,0x1715609du,0xb54cda56u})foreach(bool strong in new[]{false,true})foreach(var center in new[]{Vector2.zero,Vector2.right*9,Vector2.left*9,Vector2.up*9,Vector2.down*9})using(var a=new Arena(region,seed)) {
            a.Core.transform.position=center;Set(a.Runtime,"_wave",stage/20+1);Set(a.Runtime,"_runTime",(float)stage);Set(a.Runtime,"_knowledgeLoaded",true);
            var known=Get<HashSet<string>>(a.Runtime,"_knownKnowledgeIds");foreach(var id in new[]{"resonance_basin","spectral_rift","fractured_bastion","gravity_folds","resonance_storm","null_archive"})for(int i=0;i<5;i++)known.Add("region-lore:"+id+":"+i);
            Call(a.Runtime,"SpawnEnemy");var e=a.Enemies.Last();a.Sphere(new State{Family=SphereId.Standard,Level=strong?7:1,Branch=strong?SphereEvolutionCatalog.GetBranches(SphereId.Standard)[0].Id:null,Final=strong?SphereEvolutionCatalog.GetFinals(SphereEvolutionCatalog.GetBranches(SphereId.Standard)[0].Id)[0].Id:null}).transform.position=(Vector2)a.Core.transform.position+Vector2.right*70*SpherePlacementRules.SourcePixelsToWorld;
            string role=e.Role,elite=e.EliteVariant;float hp=e.CurrentHp,speed=e.SourceSpeed,startDistance=Vector2.Distance(e.transform.position,a.Core.transform.position)/SpherePlacementRules.SourcePixelsToWorld;
            var p=RegionalEnemyRules.Get(region,role);float possible=-1,first=-1,killed=-1,min=startDistance;int attacks=0;const float dt=1f/60;
            for(int i=0;i<1200;i++) {if(e==null||!e.IsAlive){killed=i*dt;break;}float d=Vector2.Distance(e.transform.position,a.Core.transform.position)/SpherePlacementRules.SourcePixelsToWorld;min=Math.Min(min,d);if(possible<0&&p.Range>0&&d<p.Range)possible=i*dt;float wind=Get<float>(e,"_regionalWindup");a.Tick(dt);if(e!=null&&p.Range>0&&wind>0&&Get<float>(e,"_regionalWindup")<=0){attacks++;if(first<0&&p.Speed>0)first=(i+1)*dt;}}
            rows.Add(new{region,stage,seed,strong,centerX=center.x,centerY=center.y,role,elite,hp,speed,range=p.Range,interval=p.Interval,windup=p.Windup,projectileSpeed=p.Speed,startDistance,firstPossible=possible,firstShot=first,attacks,killedAt=killed,damage=100000-a.Core.CurrentHp,minDistance=min});
        }
        Write(dir+"/"+label+".json",rows);return new{status="PASS",arenas=rows.Count,label,scope="actual spawns and C# combat; isolated one enemy vs one Standard I/VII, 20s; three time strata and six seeds; not full runs or all build combinations"};
    }

    public static object ThreatPacks(){
      if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");var rows=new List<object>();
      foreach(int region in Enumerable.Range(0,6))foreach(int stage in new[]{0,900,1700})foreach(uint seed in new uint[]{17,719,3109})foreach(bool strong in new[]{false,true})foreach(bool moving in new[]{false,true})using(var a=new Arena(region,seed)){
        a.Core.SetMaxHpForNewRun(100);a.NetworkTick=true;a.FastParticles=true;Set(a.Runtime,"_wave",stage/20+1);Set(a.Runtime,"_runTime",(float)stage);Set(a.Runtime,"_level",20);Set(a.Runtime,"_knowledgeLoaded",true);var known=Get<HashSet<string>>(a.Runtime,"_knownKnowledgeIds");foreach(var id in new[]{"resonance_basin","spectral_rift","fractured_bastion","gravity_folds","resonance_storm","null_archive"})for(int j=0;j<5;j++)known.Add("region-lore:"+id+":"+j);
        int index=0;foreach(var family in new[]{SphereId.Standard,SphereId.Chain,SphereId.Aura}){var branch=SphereEvolutionCatalog.GetBranches(family)[0].Id;var sphere=a.Sphere(new State{Family=family,Level=strong?7:2,Branch=strong?branch:null,Final=strong?SphereEvolutionCatalog.GetFinals(branch)[0].Id:null});float angle=index++*Mathf.PI*2/3;sphere.transform.position=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*70*SpherePlacementRules.SourcePixelsToWorld;}a.CheckPlacement();
        for(int j=0;j<6;j++){Call(a.Runtime,"SpawnEnemy");float angle=j*Mathf.PI*2/6;a.Enemies.Last().transform.position=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*220*SpherePlacementRules.SourcePixelsToWorld;Call(a.Runtime,"RefreshEnemyQuery",a.Enemies.Last());}
        var initial=a.Enemies.ToArray();int peakSummons=0,steps=0;for(;steps<1200&&!a.Core.IsDead;steps++){if(moving){var target=new Vector2(Mathf.Cos(steps/60f),Mathf.Sin(steps/60f))*60*SpherePlacementRules.SourcePixelsToWorld;var direction=(target-(Vector2)a.Core.transform.position).normalized;a.Core.transform.position+=(Vector3)(a.Core.GetVelocity(direction,180*SpherePlacementRules.SourcePixelsToWorld,1)/60f);}a.Tick(1f/60);int summons=0;foreach(var e in a.Enemies)if(e!=null&&e.IsAlive&&Get<int>(e,"_summonParentId")!=0)summons++;peakSummons=Math.Max(peakSummons,summons);if(summons>RegionalCombatTuning.GlobalBroodCap)throw new Exception("pack summon cap exceeded");}
        rows.Add(new{region,stage,seed,strong,moving,seconds=steps/60f,survived=!a.Core.IsDead,damage=100-a.Core.CurrentHp,killed=initial.Count(e=>e==null||!e.IsAlive),peakSummons,initialRoles=initial.Select(e=>e==null?"destroyed":e.Role).ToArray()});
      }
      Directory.CreateDirectory("Library/combat-balance/threat");Write("Library/combat-balance/threat/packs-after.json",rows);return new{status="PASS",arenas=rows.Count,scope="actual six-enemy packs vs Standard/Chain/Aura II or VII, legal three-node placement, real network and100HP core, stationary/scripted movement,20s; not acquisition-balanced or full runs"};
    }
}
