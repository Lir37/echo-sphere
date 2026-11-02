// Actual production StepGameplay + actual acquisition/AI/boss/economy paths.
// Bot outcomes are strategy diagnostics, not human win rates or enjoyment.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using EchoSphere.Core;
using EchoSphere.Runtime;
using Newtonsoft.Json;

public static class FullRunSimulation
{
    const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static readonly Type R=typeof(EchoSphereRuntime);
    static readonly Type Prefs=R.GetNestedType("PlayerPrefs",BindingFlags.NonPublic);
    static readonly Type Lifetime=R.Assembly.GetType("EchoSphere.Runtime.NativeSimulationLifetime");
    static readonly Dictionary<string,FieldInfo> Fields=new Dictionary<string,FieldInfo>();
    static FieldInfo Field(Type t,string n){var k=t.FullName+":"+n;if(!Fields.TryGetValue(k,out var f))Fields[k]=f=t.GetField(n,F);if(f==null)throw new Exception("Missing field "+k);return f;}
    static T Get<T>(object o,string n)=>(T)Field(o.GetType(),n).GetValue(o);
    static void Set(object o,string n,object v)=>Field(o.GetType(),n).SetValue(o,v);
    static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,F).Invoke(o,args);
    static object Static(Type t,string n,params object[] args)=>t.GetMethod(n,F).Invoke(null,args);
    static string Output=>Path.GetFullPath("Library/full-run-balance");
    static void Write(string name,object value){Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,name),JsonConvert.SerializeObject(value,Formatting.Indented));}
    static void Require(bool value,string message){if(!value)throw new Exception(message);}

    public static object IsolationProbe()
    {
        Require(Prefs!=null&&Lifetime!=null,"Refresh/recompile Editor: simulation types not loaded");
        Require(R.GetMethod("StepGameplay",F).GetParameters().Length==5,"Refresh/recompile Editor: stale gameplay assembly");
        string key="echosphere_active_run_v1";bool present=UnityEngine.PlayerPrefs.HasKey(key);string before=UnityEngine.PlayerPrefs.GetString(key,"absent");int checks=0;
        using((IDisposable)Static(Prefs,"Isolate")) {
            Require(!(bool)Static(Prefs,"HasKey",key),"Sandbox read real save");checks++;
            Static(Prefs,"SetString",key,"sandbox");Static(Prefs,"SetInt","integer",7);Static(Prefs,"SetFloat","float",.5f);Static(Prefs,"Save");
            Require((string)Static(Prefs,"GetString",key,"")=="sandbox"&&(int)Static(Prefs,"GetInt","integer",0)==7&&(float)Static(Prefs,"GetFloat","float",0f)==.5f,"Typed sandbox values");checks++;
            Require((int)Static(Prefs,"GetInt",key,91)==91,"Typed fallback");checks++;
            try{using((IDisposable)Static(Prefs,"Isolate")){Require(!(bool)Static(Prefs,"HasKey",key),"Nested scope inherited state");checks++;throw new InvalidOperationException("probe");}}catch(InvalidOperationException){}
            Require((string)Static(Prefs,"GetString",key,"")=="sandbox","Scope did not restore on exception");checks++;
            Static(Prefs,"DeleteKey","integer");Require(!(bool)Static(Prefs,"HasKey","integer"),"Delete escaped sandbox");checks++;
        }
        Require(present==UnityEngine.PlayerPrefs.HasKey(key)&&before==UnityEngine.PlayerPrefs.GetString(key,"absent"),"Live save changed");checks++;
        using((IDisposable)Static(Lifetime,"Isolate")) {
            var go=new GameObject("Frame lifetime probe");Static(Lifetime,"Destroy",go);Static(Lifetime,"Destroy",go);
            Require(go!=null,"Destroy ran before virtual frame end");checks++;Static(Lifetime,"Flush");Require(go==null,"Frame end did not destroy queued object");checks++;
        }
        var result=new{result="PASS",checks,scope="Isolated typed reads/writes/delete/save, nested exception restoration, real active-save unchanged, duplicate deferred destroy boundary."};Write("isolation.json",result);return result;
    }
    public sealed class Request { public int region;public uint seed=17;public string policy="collector";public int family;public int character;public float seconds=1800;public bool fastCosmetics=true; }
    sealed class World : IDisposable
    {
        readonly Scene previous,scene;readonly IDisposable preferences,lifetime;readonly bool audio;
        bool audioCaptured,disposed;
        public EchoSphereRuntime Runtime;public PlayerCoreController2D Core;
        public List<SphereAttackAgent> Spheres;public List<EnemyAgent2D> Enemies;public List<MonoBehaviour> Agents;
        public Vector2 Steering;
        public Action<float,Vector2,bool,bool,Action> Step;
        public World(Request request) {
            preferences=(IDisposable)Static(Prefs,"Isolate");
            try {
            lifetime=(IDisposable)Static(Lifetime,"Isolate");
            previous=SceneManager.GetActiveScene();scene=SceneManager.CreateScene("Isolated full run "+Guid.NewGuid().ToString("N"));SceneManager.SetActiveScene(scene);
            typeof(SourceAudio2D).GetMethod("EnsureInstance",F).Invoke(null,null);audio=SourceAudio2D.IsEnabled;audioCaptured=true;SourceAudio2D.SetEnabled(false);
            var go=new GameObject("Isolated campaign runtime");go.SetActive(false);Runtime=go.AddComponent<EchoSphereRuntime>();Runtime.enabled=false;go.SetActive(true);
            // Access only. No real clear flags, meta purchases or user saves.
            foreach(EchoRegionId region in Enum.GetValues(typeof(EchoRegionId)))Static(Prefs,"SetInt",(string)Static(R,"RegionStabilizedKey",region),1);
            Static(Prefs,"SetInt","echosphere_tutorial_completed_v1",1);Static(Prefs,"SetString","echosphere_difficulty","normal");
            Set(Runtime,"_sphereCharacterId",(SphereCharacterId)request.character);Set(Runtime,"_characterMasteryLevel",1);Set(Runtime,"_selectedRegion",(EchoRegionId)request.region);
            Call(Runtime,"CreatePlayer");Core=Get<PlayerCoreController2D>(Runtime,"_playerCore");Call(Runtime,"CreateSpheres");
            var field=go.AddComponent<RegionFieldVisuals>();field.enabled=false;field.Initialize(Runtime,(EchoRegionId)request.region);Set(Runtime,"_regionFieldVisuals",field);
            Call(Runtime,"BeginSelectedRegionRun");Require(!Get<bool>(Runtime,"_mainMenu")&&Get<int>(Runtime,"_tutorialStep")==-1,"Campaign did not start");
            Get<SeededRng>(Runtime,"_rng").RestoreState(request.seed);Set(Runtime,"_runEconomy",RunEconomyRules.Create(request.seed));
            Spheres=Get<List<SphereAttackAgent>>(Runtime,"_spheres");Enemies=Get<List<EnemyAgent2D>>(Runtime,"_enemies");Agents=Get<List<MonoBehaviour>>(Runtime,"_simulationAgents");
            Step=(Action<float,Vector2,bool,bool,Action>)Delegate.CreateDelegate(typeof(Action<float,Vector2,bool,bool,Action>),Runtime,R.GetMethod("StepGameplay",F));
            }catch{Dispose();throw;}
        }
        public void EndFrame(bool fast) {
            if(fast)foreach(var a in Agents)if(a is SourceParticle2D)Call(Runtime,"RetireNativeTransient",a);
            var pool=Get<object>(Runtime,"_nativeTransientPools");if(pool!=null)Call(pool,"Flush");
            Static(Lifetime,"Flush");
            // Consumed chests are inactive and Destroy is requested by runtime.
            foreach(var a in Agents)if(a!=null&&!a.gameObject.activeInHierarchy&&(a is ArtifactChestAgent2D||a is StellaChestAgent2D))UnityEngine.Object.DestroyImmediate(a.gameObject);
        }
        public void Dispose() {
            if(disposed)return;disposed=true;
            var trace=Application.GetStackTraceLogType(LogType.Log);Application.SetStackTraceLogType(LogType.Log,StackTraceLogType.None);
            try {
                if(scene.IsValid()&&scene.isLoaded)foreach(var go in scene.GetRootGameObjects())if(go!=null&&(Runtime==null||go!=Runtime.gameObject))UnityEngine.Object.DestroyImmediate(go);
                if(Runtime!=null)UnityEngine.Object.DestroyImmediate(Runtime.gameObject);
            }finally{
                try{lifetime?.Dispose();if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(scene.IsValid()&&scene.isLoaded)SceneManager.UnloadSceneAsync(scene);if(audioCaptured)SourceAudio2D.SetEnabled(audio);}
                finally{preferences.Dispose();Application.SetStackTraceLogType(LogType.Log,trace);}
            }
        }
    }
    static readonly RunUpgradeKind[] Active={RunUpgradeKind.Blast,RunUpgradeKind.Shield,RunUpgradeKind.Lightning,RunUpgradeKind.Minion,RunUpgradeKind.TimeStop,RunUpgradeKind.FireTrail,RunUpgradeKind.Teleport,RunUpgradeKind.DarkRitual};
    static float Score(RunUpgradeChoice c,World w,Request r) {
        if(c.Kind==RunUpgradeKind.Sphere) {
            bool placed=w.Spheres.Any(s=>s!=null&&s.Type==c.Sphere);
            return c.Sphere==(SphereId)r.family?150-c.CurrentLevel:placed?115-c.CurrentLevel:c.Sphere==SphereId.Standard?80:25;
        }
        int rank=Array.IndexOf(Active,c.Kind);if(rank>=0)rank=(rank+r.family)%Active.Length;return rank<0?0:100-rank*4+c.CurrentLevel;
    }
    static void Choices(World w,Request r,List<object> history) {
        for(int guard=0;guard<100;guard++) {
            if(Get<bool>(w.Runtime,"_choosingEvolution")) {
                var choices=Get<List<SphereEvolutionOption>>(w.Runtime,"_evolutionChoices");int at=(int)((r.seed+(uint)r.family)%choices.Count);
                history.Add(new{time=w.Runtime.CurrentRunTime,evolution=choices[at].Id});Call(w.Runtime,"ChooseEvolution",at);continue;
            }
            if(Get<bool>(w.Runtime,"_stellaRewardPending")) {
                var choices=Get<List<ArtifactRewardDefinition>>(w.Runtime,"_stellaRewardChoices");int at=Enumerable.Range(0,choices.Count).OrderByDescending(i=>ArtifactScore(choices[i])).First();
                history.Add(new{time=w.Runtime.CurrentRunTime,artifact=choices[at].Id});Call(w.Runtime,"ChooseStellaReward",at);continue;
            }
            if(Get<bool>(w.Runtime,"_levelUp")) {
                var choices=Get<List<RunUpgradeChoice>>(w.Runtime,"_levelUpChoices");Require(choices.Count>0,"Level-up paused without choices");
                int at=Enumerable.Range(0,choices.Count).OrderByDescending(i=>Score(choices[i],w,r)).First();var c=choices[at];
                history.Add(new{time=w.Runtime.CurrentRunTime,upgrade=c.Kind.ToString(),sphere=c.Sphere.ToString(),level=c.NextLevel});Call(w.Runtime,"ChooseUpgrade",at);continue;
            }
            return;
        }
        throw new Exception("Choice loop did not resume gameplay");
    }
    static float ArtifactScore(ArtifactRewardDefinition a)=>a.GetEffect("sphereDamage")*100+a.GetEffect("regen")*4+a.GetEffect("maxHp")*.1f-a.GetEffect("sphereDelay")*70+(int)a.Rarity;
    static void Build(World w,Request r) {
        int cap=(int)Call(w.Runtime,"GetCurrentMaxSpheres");if(w.Spheres.Count>=cap)return;
        var levels=Get<Dictionary<SphereId,int>>(w.Runtime,"_sphereLevels");
        var candidates=new[]{(SphereId)r.family,SphereId.Standard,SphereId.Aura,SphereId.Chain,SphereId.Pulse,SphereId.Gravity,SphereId.Orbital,SphereId.Shotgun,SphereId.Sniper,SphereId.Prism,SphereId.Void};
        foreach(var family in candidates) {
            // The physical placement routine assumes its UI already checked unlock.
            if(family!=SphereId.Standard&&(!levels.TryGetValue(family,out int l)||l<=0))continue;
            if(w.Spheres.Count(s=>s!=null&&s.Type==family)>=2)continue;
            for(int i=0;i<24;i++) {
                var pos=w.Runtime.PlayerPosition+new Vector2(Mathf.Cos(i*Mathf.PI/12),Mathf.Sin(i*Mathf.PI/12))*100*SpherePlacementRules.SourcePixelsToWorld;
                if(!(bool)Call(w.Runtime,"CanPlaceSphereAt",pos,null))continue;
                Set(w.Runtime,"_selectedSphereType",family);Call(w.Runtime,"TryPlaceOrRemoveSphereAt",pos);return;
            }
        }
    }
    static void Shop(World w,List<object> history) {
        var economy=Get<RunEconomyState>(w.Runtime,"_runEconomy");
        if(!economy.Open&&economy.Pending&&(bool)R.GetProperty("CanOpenNativeMerchant",F).GetValue(w.Runtime))Call(w.Runtime,"OpenNativeMerchant");
        if(!economy.Open)return;
        foreach(int i in Enumerable.Range(0,economy.Offers.Count).OrderByDescending(i=>ArtifactScore(ArtifactRewardRules.Get(economy.Offers[i].ArtifactId)))) {
            int before=economy.MerchantPurchases;Call(w.Runtime,"BuyNativeMerchant",i);
            if(economy.MerchantPurchases>before)history.Add(new{time=w.Runtime.CurrentRunTime,bought=economy.Offers[i].ArtifactId,price=economy.Offers[i].Price});
        }
        Call(w.Runtime,"CloseNativeMerchant");
    }
    static Vector2 Move(World w,Request r,int frame) {
        if(r.policy=="tactical")return TacticalMove(w,frame);
        var p=w.Runtime.PlayerPosition;Vector2 goal=r.policy=="anchor"?Vector2.zero:p+new Vector2(Mathf.Cos(frame/240f),Mathf.Sin(frame/240f))*2;
        if(r.policy=="collector") {
            EnemyAgent2D target=null;float closest=6.3f;
            foreach(var e in w.Enemies)if(e!=null&&e.IsAlive){float d=Vector2.Distance(p,e.transform.position);if(d<closest){closest=d;target=e;}}
            if(target!=null){var delta=(Vector2)target.transform.position-p;goal=(Vector2)target.transform.position-delta.normalized*1.2f;}
            else goal=Vector2.zero;
        }
        float nearest=6.3f*6.3f;
        foreach(var a in w.Agents)if(a!=null&&a.gameObject.activeInHierarchy&&(a is ExperienceOrbAgent||a is ArtifactChestAgent2D||a is StellaChestAgent2D||a is HealthPackAgent2D||a is RuneAgent2D)) {
            float distance=((Vector2)a.transform.position-p).sqrMagnitude;
            float score=distance*(a is ArtifactChestAgent2D||a is StellaChestAgent2D?.35f:1);
            if(score<nearest){nearest=score;goal=a.transform.position;}
        }
        if(r.policy=="anchor"&&(goal-p).sqrMagnitude>2.5f*2.5f)goal=Vector2.zero;
        var direction=(goal-p).normalized;Vector2 repel=Vector2.zero;
        foreach(var e in w.Enemies)if(e!=null&&e.IsAlive) {
            var delta=p-(Vector2)e.transform.position;float d=delta.magnitude;
            float avoid=r.policy=="collector"?1.5f:2.5f;
            if(d<avoid)repel+=delta.normalized*(avoid-d)/Mathf.Max(.35f,d);
        }
        if(r.policy=="collector")direction=(direction+repel*(w.Core.CurrentHp>w.Core.MaxHp*.7f?.6f:1.5f)).normalized;
        else if(r.policy=="kite")direction=(direction*.6f+repel*3f+new Vector2(-p.y,p.x).normalized*.4f).normalized;
        if(Mathf.Abs(p.x)>19)direction.x=-Mathf.Sign(p.x);if(Mathf.Abs(p.y)>19)direction.y=-Mathf.Sign(p.y);
        return direction.normalized;
    }
    // Controller only: local visible targets, no future RNG or world teleport.
    // Hold a direction for 100ms to avoid perfect-frame input and steering jitter.
    static Vector2 TacticalMove(World w,int frame) {
        if(frame%6!=0)return w.Steering;
        Vector2 p=w.Runtime.PlayerPosition,goal=p;float bestGoal=float.PositiveInfinity;
        var threats=w.Enemies.Where(e=>e!=null&&e.IsAlive&&Vector2.Distance(p,e.transform.position)<6.3f).ToArray();
        foreach(var a in w.Agents)if(a!=null&&a.gameObject.activeInHierarchy&&(a is ExperienceOrbAgent||a is ArtifactChestAgent2D||a is StellaChestAgent2D||a is RuneAgent2D||a is HealthPackAgent2D&&w.Core.CurrentHp<w.Core.MaxHp*.85f)) {
            Vector2 at=a.transform.position;float d=Vector2.Distance(p,at);if(d>6.3f)continue;
            float score=d*(a is ArtifactChestAgent2D||a is StellaChestAgent2D?.6f:1);
            foreach(var e in threats)score+=Mathf.Max(0,1.2f-Vector2.Distance(at,e.transform.position))*3;
            if(score<bestGoal){bestGoal=score;goal=at;}
        }
        if(float.IsPositiveInfinity(bestGoal)&&threats.Length>0){var nearest=threats.OrderBy(e=>Vector2.Distance(p,e.transform.position)).First();Vector2 at=nearest.transform.position;goal=at+(p-at).normalized*1.85f;}
        Vector2 best=Vector2.zero;float bestScore=float.NegativeInfinity;
        for(int i=-1;i<24;i++) {
            Vector2 dir=i<0?Vector2.zero:new Vector2(Mathf.Cos(i*Mathf.PI/12),Mathf.Sin(i*Mathf.PI/12));Vector2 next=p+dir*.5f;
            float score=(Vector2.Distance(p,goal)-Vector2.Distance(next,goal))*3+Vector2.Dot(w.Steering,dir)*.03f;
            foreach(var e in threats){float d=Vector2.Distance(next,e.transform.position);score-=Mathf.Pow(Mathf.Max(0,1.3f-d),2)*16;}
            score-=Mathf.Max(0,Mathf.Abs(next.x)-19)*10+Mathf.Max(0,Mathf.Abs(next.y)-19)*10;
            if(score>bestScore){bestScore=score;best=dir;}
        }
        w.Steering=best;return best;
    }
    static string Signature(World w)=>JsonConvert.SerializeObject(new {
        time=w.Runtime.CurrentRunTime,hp=w.Core.CurrentHp,position=new[]{w.Runtime.PlayerPosition.x,w.Runtime.PlayerPosition.y},
        level=Get<int>(w.Runtime,"_level"),xp=Get<float>(w.Runtime,"_xp"),kills=Get<int>(w.Runtime,"_kills"),wave=Get<int>(w.Runtime,"_wave"),
        spawning=Get<int>(w.Runtime,"_waveEnemiesToSpawn"),spawnTimer=Get<float>(w.Runtime,"_spawnTimer"),resonance=Get<float>(w.Runtime,"_resonanceCharge"),
        rng=Get<SeededRng>(w.Runtime,"_rng").State,
        spheres=w.Spheres.Where(s=>s!=null).Select(s=>new{type=s.Type.ToString(),hp=s.ProgressionLevel,timer=s.AttackTimer,shots=s.PresentationShotSerial,xy=new[]{s.transform.position.x,s.transform.position.y}}).ToArray(),
        enemies=w.Enemies.Where(e=>e!=null).Select(e=>new{hp=e.CurrentHp,alive=e.IsAlive,frozen=e.IsFrozen,xy=new[]{e.transform.position.x,e.transform.position.y}}).ToArray(),
        agents=w.Agents.Where(a=>a!=null&&a.gameObject.activeInHierarchy).Select(a=>new{type=a.GetType().Name,xy=new[]{a.transform.position.x,a.transform.position.y}}).ToArray()
    });
    public static object FrameEquivalence()
    {
        Require(UnityEditor.EditorApplication.isPlaying,"Play Mode required");
        Require(Input.touchCount==0&&!Input.GetMouseButton(0)&&Math.Abs(Input.GetAxisRaw("Horizontal"))<.001&&Math.Abs(Input.GetAxisRaw("Vertical"))<.001,"Leave Editor inputs idle during equivalence probe");
        float dt=Mathf.Min(.05f,Time.deltaTime);Require(dt>0,"Nonzero Editor delta required");int checks=0;
        foreach(uint seed in new uint[]{17,29,71}) {
            var reference=new List<string>();
            foreach(bool native in new[]{true,false})using(var w=new World(new Request{seed=seed})) {
                var camera=new GameObject("Isolated camera").AddComponent<Camera>();camera.enabled=false;Set(w.Runtime,"_camera",camera);
                for(int i=0;i<2048;i++) {
                    if(w.Core.IsDead)break;
                    if(native)Call(w.Runtime,"Update");else w.Step(dt,Vector2.zero,false,false,null);
                    w.EndFrame(false);string state=Signature(w);
                    if(native)reference.Add(state);else{Require(i<reference.Count&&reference[i]==state,"Production Update/headless difference at seed "+seed+" frame "+i);checks++;}
                }
            }
        }
        var result=new{result="PASS",checks,seeds=3,delta=dt,scope="Per-frame production Update versus shared headless step with idle input: HP/position, progression, wave timers, RNG, sphere cadence, enemy states, active agent birth/death/positions; camera presentation excluded."};Write("frame-equivalence.json",result);return result;
    }
    public static object Main() {
        Require(UnityEditor.EditorApplication.isPlaying,"Play Mode required");
        foreach(var live in UnityEngine.Object.FindObjectsByType<EchoSphereRuntime>())Require(!live.enabled||Get<bool>(live,"_mainMenu"),"Stop the live battle before an isolated run");
        var request=File.Exists(Path.Combine(Output,"request.json"))?JsonConvert.DeserializeObject<Request>(File.ReadAllText(Path.Combine(Output,"request.json"))):new Request{seconds=120};
        return Execute(request);
    }
    public static object SnapshotGuardProbe() {
        int checks=0;
        using(var w=new World(new Request())) {
            var excluded=(HashSet<string>)R.GetField("NonRunFields",F).GetValue(null);
            Set(w.Runtime,"_sourceTrianglePulseActive",true);
            try {
                var save=Call(w.Runtime,"CaptureRunSnapshot");
                var snapshot=Get<SnapshotValue>(save,"runtime");
                Require(!snapshot.nodes.Any(n=>n.name=="_sourceTrianglePulseActive"),"Reentrant hit guard leaked into save");checks++;
                Require(snapshot.nodes.Any(n=>n.name=="_runTime")&&snapshot.nodes.Any(n=>n.name=="_rng"),"Gameplay snapshot unexpectedly empty");checks++;
            }finally{Call(Get<object>(w.Runtime,"_runSaveBuffers"),"End");Set(w.Runtime,"_sourceTrianglePulseActive",false);}
            Set(w.Runtime,"_sourceTrianglePulseActive",true);
            var historical=RunSnapshotCodec.Capture(w.Runtime,f=>f.Name=="_sourceTrianglePulseActive");
            Set(w.Runtime,"_sourceTrianglePulseActive",false);
            RunSnapshotCodec.Restore(historical,R,w.Runtime,f=>!excluded.Contains(f.Name));
            Require(!Get<bool>(w.Runtime,"_sourceTrianglePulseActive"),"Historical saved guard restored into combat");checks++;
        }
        var result=new{result="PASS",checks,scope="Actual CaptureRunSnapshot excludes the transient Triangle reentrancy guard, retains gameplay/RNG; production field filter ignores the guard in historical snapshots."};Write("snapshot-guard.json",result);return result;
    }
    public static object BossScalingProbe() {
        var rows=new List<object>();
        foreach(float time in new[]{480f,960f,1620f})foreach(int wave in new[]{2,20,40})using(var w=new World(new Request{region=1})) {
            Set(w.Runtime,"_runTime",time);Set(w.Runtime,"_wave",wave);
            var enemy=(EnemyAgent2D)Call(w.Runtime,"SpawnBossForRegionEvent","shooter",wave);
            Require(enemy!=null,"Campaign boss did not spawn");
            rows.Add(new{time,wave,hp=Get<float>(enemy,"_maxHp"),damage=Get<float>(enemy.GetComponent<BossCombatAgent2D>(),"_damage"),speed=enemy.SourceSpeed});
        }
        var result=new{result="MEASURED",scope="Actual campaign boss creation with fixed wave versus fixed clock; diagnostics only, no stat overrides or new tuning.",rows};Write("boss-scaling.json",result);return result;
    }
    public static object FastEquivalence() {
        var request=new Request{seconds=300,policy="collector",fastCosmetics=false};
        var regular=Newtonsoft.Json.Linq.JObject.FromObject(Execute(request));
        request.fastCosmetics=true;var fast=Newtonsoft.Json.Linq.JObject.FromObject(Execute(request));
        foreach(var value in new[]{regular,fast}){value.Remove("elapsedMs");((Newtonsoft.Json.Linq.JObject)value["request"]).Remove("fastCosmetics");}
        if(!Newtonsoft.Json.Linq.JToken.DeepEquals(regular,fast)) {
            Write("cosmetics-regular.json",regular);Write("cosmetics-fast.json",fast);
            throw new Exception("Cosmetic acceleration changed a full-run outcome; inspect saved projections.");
        }
        var result=new{result="PASS",checks=1,seconds=(float)regular["seconds"],kills=(int)regular["kills"],level=(int)regular["level"],scope="Actual collector world/economy/progression, up to 300s or death: identical game RNG/outcomes/history/samples/positions with regular vs early-retired cosmetic particles; no player quality setting changed."};Write("cosmetics-equivalence.json",result);return result;
    }
    static object Execute(Request request) {
        Require(request.region>=0&&request.region<6&&request.family>=0&&request.family<10&&request.character>=0&&request.character<10&&request.seconds>0&&request.seconds<=2100&&new[]{"anchor","collector","kite","tactical"}.Contains(request.policy),"Invalid request");
        var history=new List<object>();var samples=new List<object>();var clock=System.Diagnostics.Stopwatch.StartNew();object result;
        using(var w=new World(request)) {
            int peak=0,casts=0;float distance=0;int steps=0;
            Action castInput=()=>{foreach(var kind in Active)if((int)Call(w.Runtime,"GetAbilityLevel",kind)>0) {
                string field="_"+char.ToLowerInvariant(kind.ToString()[0])+kind.ToString().Substring(1)+"Cooldown";
                if(Get<float>(w.Runtime,field)<=0){Call(w.Runtime,"ActivateActiveAbility",kind);if(Get<float>(w.Runtime,field)>0)casts++;}
            }};
            for(int frame=0;frame<Mathf.CeilToInt(request.seconds*61)&&w.Runtime.CurrentRunTime<request.seconds;frame++) {
                if(w.Core.IsDead||Get<bool>(w.Runtime,"_gameOver"))break;
                Choices(w,request,history);
                if(frame%30==0){Shop(w,history);Build(w,request);Call(w.Runtime,"SetFormationFollowMode",true,false);}
                var move=Move(w,request,frame);
                var before=w.Runtime.PlayerPosition;float beforeTime=w.Runtime.CurrentRunTime;
                w.Step(1f/60,move,false,false,frame%15==0?castInput:null);
                Require(w.Runtime.CurrentRunTime>beforeTime||w.Core.IsDead||Get<bool>(w.Runtime,"_gameOver"),"Unresolved gameplay pause in bot controller");
                distance+=Vector2.Distance(before,w.Runtime.PlayerPosition);w.EndFrame(request.fastCosmetics);steps++;
                peak=Math.Max(peak,w.Enemies.Count(e=>e!=null&&e.IsAlive));
                if(frame%600==0) {
                    samples.Add(new{time=w.Runtime.CurrentRunTime,hp=w.Core.CurrentHp,level=Get<int>(w.Runtime,"_level"),kills=Get<int>(w.Runtime,"_kills"),enemies=w.Enemies.Count(e=>e!=null&&e.IsAlive),artifacts=Get<List<string>>(w.Runtime,"_ownedArtifacts").Count});
                    Write("progress.json",new{request,time=w.Runtime.CurrentRunTime,steps,peak,elapsedMs=clock.ElapsedMilliseconds});
                }
            }
            Choices(w,request,history);
            bool won=Get<bool>(w.Runtime,"_regionStabilized"),dead=w.Core.IsDead;
            if(dead){Set(w.Runtime,"_gameOver",true);Set(w.Runtime,"_paused",true);Call(w.Runtime,"FinalizeRunResult");}
            var economy=Get<RunEconomyState>(w.Runtime,"_runEconomy");
            result=new{result="COMPLETE",request,seconds=w.Runtime.CurrentRunTime,steps,outcome=won?"stabilized":dead?"dead":"time_limit",hp=w.Core.CurrentHp,level=Get<int>(w.Runtime,"_level"),xp=Get<float>(w.Runtime,"_xp"),wave=Get<int>(w.Runtime,"_wave"),kills=Get<int>(w.Runtime,"_kills"),bosses=Get<int>(w.Runtime,"_bossesDefeated"),peakEnemies=peak,casts,distance,
                collectedEcho=economy.CollectedEcho,spentEcho=economy.SpentEcho,purchases=economy.MerchantPurchases,artifacts=Get<List<string>>(w.Runtime,"_ownedArtifacts").ToArray(),
                spheres=w.Spheres.Where(s=>s!=null).Select(s=>new{type=s.Type.ToString(),level=s.ProgressionLevel,branch=w.Runtime.GetSphereBranch(s.Type),final=w.Runtime.GetSphereFinal(s.Type),position=new[]{s.transform.position.x,s.transform.position.y},shots=s.PresentationShotSerial,range=s.AttackRange}).ToArray(),
                debugEnemies=w.Enemies.Where(e=>e!=null&&e.IsAlive).Take(64).Select(e=>new{hp=e.CurrentHp,position=new[]{e.transform.position.x,e.transform.position.y}}).ToArray(),
                debugXp=w.Agents.Where(a=>a!=null&&a.gameObject.activeInHierarchy&&a is ExperienceOrbAgent).Take(64).Select(a=>new[]{a.transform.position.x,a.transform.position.y}).ToArray(),
                rng=Get<SeededRng>(w.Runtime,"_rng").State,history,samples,elapsedMs=clock.ElapsedMilliseconds,
                scope="Production gameplay phases at 60Hz, normal difficulty, empty meta upgrades/mastery I, isolated access flags, legal offered upgrades/placements, actual pickups/merchant, bot input; not human win rate, player enjoyment or Android performance."};
        }
        Write("last-run.json",result);return result;
    }
}
