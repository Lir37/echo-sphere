using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class EnemyQueryAcceptance
{
    public static object Main()
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        var root=new GameObject("Enemy query acceptance");
        var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;
        var rt=typeof(EchoSphereRuntime);
        rt.GetField("_mainMenu",Flags).SetValue(runtime,false);
        rt.GetField("_paused",Flags).SetValue(runtime,false);
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(2000,2000,0);
        rt.GetField("_player",Flags).SetValue(runtime,player.transform);
        var enemies=(List<EnemyAgent2D>)rt.GetField("_enemies",Flags).GetValue(runtime);
        int checks=0;void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
        var random=new System.Random(41871);
        EnemyAgent2D Add(float x,float y){var go=new GameObject("Fixture enemy");go.transform.SetParent(root.transform);go.transform.position=new Vector3(x,y,0);var enemy=go.AddComponent<EnemyAgent2D>();enemy.Initialize(runtime,player.transform,100,1);enemies.Add(enemy);return enemy;}
        void Compare(Vector2 center,float radius)
        {
            var expected=new List<EnemyAgent2D>();EnemyAgent2D nearest=null,hit=null;float distance=radius*radius;
            var ignored=new HashSet<int>();if(enemies.Count>0&&enemies[0]!=null)ignored.Add(unchecked((int)(EntityId.ToULong(enemies[0].GetEntityId())&uint.MaxValue)));
            foreach(var enemy in enemies){if(enemy==null||!enemy.IsAlive)continue;float d=((Vector2)enemy.transform.position-center).sqrMagnitude;
                if(d<=radius*radius)expected.Add(enemy);
                if(d<distance){nearest=enemy;distance=d;}
                float combined=radius+enemy.SourceRadius*SpherePlacementRules.SourcePixelsToWorld;
                if(hit==null&&!ignored.Contains(unchecked((int)(EntityId.ToULong(enemy.GetEntityId())&uint.MaxValue)))&&d<combined*combined)hit=enemy;
            }
            Check(expected.SequenceEqual(runtime.FindEnemiesInRadius(center,radius)),"ordered radius result");
            expected.Sort((a,b)=>((Vector2)a.transform.position-center).sqrMagnitude.CompareTo(((Vector2)b.transform.position-center).sqrMagnitude));
            if(expected.Count>7)expected.RemoveRange(7,expected.Count-7);
            Check(expected.SequenceEqual(runtime.FindNearestEnemies(center,radius,7)),"ordered nearest group including ties");
            Check(nearest==runtime.FindNearestEnemy(center,radius),"nearest including ties");
            Check(hit==runtime.TryHitEnemy(center,radius,ignored),"first hit including radius/ignored IDs");
        }
        try
        {
            for(int i=0;i<120;i++)Add((float)(random.NextDouble()*50-25),(float)(random.NextDouble()*50-25));
            Add(0,0);Add(0,0);Add(4,0);Add(-4,0);
            foreach(float radius in new[]{0f,-4f,4f,float.NaN,float.PositiveInfinity,float.MaxValue})Compare(Vector2.zero,radius);
            for(int i=0;i<500;i++)
            {
                var enemy=enemies[i%enemies.Count];var center=new Vector2((float)(random.NextDouble()*40-20),(float)(random.NextDouble()*40-20));
                switch(i%6){case 0:enemy.MoveSourceRadial(center,5);break;case 1:enemy.PullByDistance(center,8);break;case 2:enemy.PullToward(center,6);break;case 3:enemy.KnockBackFrom(center,3);break;case 4:enemy.SimulationTick(.016f);break;case 5:enemy.SetSourceRadius(10+i%35);break;}
                Compare(center,i%9==0?-3:4);
                // Query immediately at the changed position to detect a stale cell.
                Compare(enemy.transform.position,.001f);
            }
            Add(1,1);Compare(Vector2.one,1);
            var removed=enemies[3];enemies.RemoveAt(3);UnityEngine.Object.DestroyImmediate(removed.gameObject);Compare(Vector2.zero,30);
            var index=rt.GetField("_enemyQueries",Flags).GetValue(runtime);var next=index.GetType().GetField("_next",Flags);
            long sequence=(long)next.GetValue(index);
            UnityEngine.Object.DestroyImmediate(enemies[4].gameObject);Compare(Vector2.zero,30);
            Check(sequence==(long)next.GetValue(index),"destroy tombstone rebuilt index before source-list cleanup");
            enemies.RemoveAt(4);Compare(Vector2.zero,30);
            var core=player.AddComponent<PlayerCoreController2D>();rt.GetField("_playerCore",Flags).SetValue(runtime,core);
            float scale=SpherePlacementRules.SourcePixelsToWorld;
            for(int frame=0;frame<6;frame++){
                var bodies=enemies.Select(e=>new CollisionBody {Position=new Vec2(e.transform.position.x/scale,-e.transform.position.y/scale),Radius=e.SourceRadius,Speed=e.SourceSpeed,Mass=e.CollisionMass,FollowBreach=e.Role=="charger"||e.Role=="phase"}).ToList();
                var expected=SpaceCollisionRules.Resolve(new Vec2(player.transform.position.x/scale,-player.transform.position.y/scale),bodies,new List<Vec2>(),.016f,false);
                rt.GetMethod("ResolveSourceSpaceCollisions",Flags).Invoke(runtime,new object[]{.016f});
                var expectedPosition=new Vector3(expected.X*scale,-expected.Y*scale,player.transform.position.z);
                Check(player.transform.position.Equals(expectedPosition),"runtime collision player after float storage");
                for(int i=0;i<enemies.Count;i++){
                    expectedPosition=new Vector3(bodies[i].Position.X*scale,-bodies[i].Position.Y*scale,enemies[i].transform.position.z);
                    Check(enemies[i].transform.position.Equals(expectedPosition),"runtime retained body after float storage");
                }
                Compare(enemies[frame].transform.position,.001f);
            }
            player.transform.position=new Vector3(2000,2000,0);
            var bossEnemy=Add(50,50);var boss=bossEnemy.gameObject.AddComponent<BossCombatAgent2D>();
            boss.Initialize(runtime,player.transform,bossEnemy,"charger",0,5,1,Color.white);bossEnemy.SetBossCombat(boss);
            Compare(Vector2.zero,2);
            boss.Tick(.016f);Compare(boss.transform.position,.001f);
            typeof(BossCombatAgent2D).GetField("_isCharging",Flags).SetValue(boss,true);
            typeof(BossCombatAgent2D).GetField("_chargeTimer",Flags).SetValue(boss,.1f);
            boss.Tick(.016f);Compare(boss.transform.position,.001f);
            boss.Initialize(runtime,player.transform,bossEnemy,"architect",0,5,1,Color.white);
            typeof(BossCombatAgent2D).GetField("_summonTimer",Flags).SetValue(boss,0f);
            boss.Tick(.016f);Compare(boss.transform.position,.001f);
            var nonRun=(HashSet<string>)rt.GetField("NonRunFields",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            var snapshot=RunSnapshotCodec.Capture(runtime,field=>!nonRun.Contains(field.Name));
            Check(!snapshot.nodes.Any(n=>n.name=="_enemyQueries"||n.name=="_enemyQueryCandidates"||n.name=="_maximumEnemyQueryRadius"||n.name=="_enemyQueryListCount"||n.name!=null&&n.name.StartsWith("_spaceCollision")),"scratch/index leaked into save schema");
            var result=new {result="PASS",checks,scope="actual Editor runtime versus original predicates/collision; radial/pull/knockback/tick/radius/birth/removal/boss charge/teleport; no PlayerPrefs writes"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/enemy-query-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
