using System;using System.IO;using System.Reflection;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class RegionalEnemyAcceptance {
 public static object Main(){const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;int checks=0;void A(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 var go=new GameObject("Temporary regional policy acceptance");go.hideFlags=HideFlags.HideAndDontSave;var e=go.AddComponent<EnemyAgent2D>();var type=e.GetType();
 void Set(string n,object v)=>type.GetField(n,F).SetValue(e,v);object Call(string n,params object[] args)=>type.GetMethod(n,F).Invoke(e,args);
 try{for(int region=0;region<6;region++)foreach(string role in new[]{"grunt","sniper","tank_guard","healer","charger","corruptor"}){
 e.SetCampaignRegion((EchoRegionId)region);Set("_role",role);Set("_regionalWindup",0f);Set("_regionalRecovery",0f);
 var p=RegionalEnemyRules.Get(region,role);var far=(Vector2)Call("RegionalMovement",.016f,Vector2.right,1000*SpherePlacementRules.SourcePixelsToWorld);A(float.IsFinite(far.x)&&far.x>0,"approach finite");
 if(p.Range>0){var near=(Vector2)Call("RegionalMovement",.016f,Vector2.right,10*SpherePlacementRules.SourcePixelsToWorld);A(near.x>=0,"ranged never retreats");var band=(Vector2)Call("RegionalMovement",.016f,Vector2.right,(p.Range+p.Minimum)*.5f*SpherePlacementRules.SourcePixelsToWorld);A(Mathf.Abs(band.x)<.001f,"no ramming in attack band");}
 Set("_regionalCooldown",0f);
 if(p.Range>0){Call("TickRegionalAttack",.016f,new Vector2(3,4),(p.Range+1)*SpherePlacementRules.SourcePixelsToWorld);A((float)type.GetField("_regionalWindup",F).GetValue(e)==0,"no early shot beyond authored range");}
 Call("TickRegionalAttack",.016f,new Vector2(3,4),1*SpherePlacementRules.SourcePixelsToWorld);
 if(p.Interval>0){A((float)type.GetField("_regionalWindup",F).GetValue(e)>0,"attack starts windup");A((Vector2)Call("RegionalMovement",.016f,Vector2.right,1)==Vector2.zero,"windup stops movement");
 var state=RunSnapshotCodec.Capture(e);Set("_regionalWindup",0f);Set("_regionalAim",Vector2.zero);RunSnapshotCodec.Restore(state,type,e);A((float)type.GetField("_regionalWindup",F).GetValue(e)>0,"pending attack restored");A((Vector2)type.GetField("_regionalAim",F).GetValue(e)==new Vector2(3,4),"locked aim restored");}
 }
 var result=new{result="PASS",checks,scope="Actual EnemyAgent2D movement, windup and snapshot restoration. Isolated Editor object; no PlayerPrefs or combat scene modified. No Android or visual acceptance."};File.WriteAllText("Library/regional-enemy-acceptance.json",JsonUtility.ToJson(new Record{result="PASS",checks=checks},true));return result;
 }finally{UnityEngine.Object.DestroyImmediate(go);}
 }
 [Serializable]public class Record{public string result;public int checks;}
}
