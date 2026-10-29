using EchoSphere.Core;
internal static class ArtifactGameplayChecks
{
 internal static int Run(){int checks=0;void Near(float value,float expected,string why){checks++;if(Math.Abs(value-expected)>.00001f)throw new Exception("Artifact contract: "+why+" "+value+" expected "+expected);}
  var keys=new[]{"sphereDamage","cooldown","sniperDamage","sniperRadius","critChance","sphereRadius"};
  foreach(var artifact in ArtifactRewardRules.All)foreach(var key in keys)Near(ArtifactGameplayRules.Effect(artifact,key,false),artifact.GetEffect(key),"legacy "+artifact.Id+"/"+key);
  foreach(var artifact in ArtifactRewardRules.All.Where(x=>x.Id!="sniper_scope"&&x.Id!="sniper_crown"&&x.Id!="chain_crown"))foreach(var key in keys)Near(ArtifactGameplayRules.Effect(artifact,key,true),artifact.GetEffect(key),"unrelated "+artifact.Id+"/"+key);
  var scope=new List<string>{"sniper_scope"};Near(ArtifactGameplayRules.EffectTotal(scope,"sniperDamage",true),.1f,"scope sniper damage");Near(ArtifactGameplayRules.EffectTotal(scope,"sphereDamage",true),0,"scope no hidden global buff");Near(ArtifactGameplayRules.EffectTotal(scope,"sphereDamage",false),.06f,"scope legacy global buff");
  var crown=new List<string>{"sniper_crown"};Near(ArtifactGameplayRules.EffectTotal(crown,"sniperRadius",true),.1f,"crown range");Near(ArtifactGameplayRules.EffectTotal(crown,"sphereDamage",true),0,"crown no hidden damage");Near(ArtifactGameplayRules.EffectTotal(crown,"cooldown",true),0,"crown no hidden cooldown");
  var chain=new List<string>{"chain_crown"};Near(ArtifactGameplayRules.ExtraChainJumps(chain,true),1,"chain extra jump");Near(ArtifactGameplayRules.ExtraChainJumps(chain,false),0,"legacy chain unchanged");Near(ArtifactGameplayRules.EffectTotal(chain,"sphereDamage",true),0,"chain no global buff");Near(ArtifactGameplayRules.EffectTotal(chain,"cooldown",true),0,"chain no cooldown");
  var all=new List<string>{"sniper_scope","sniper_crown","chain_crown","heavy_core"};Near(ArtifactGameplayRules.EffectTotal(all,"standardDamage",true),.15f,"other artifact survives adapter");
  Near(ArtifactGameplayRules.EffectTotal(null,"sphereDamage",true),0,"empty inventory");Near(ArtifactGameplayRules.ExtraChainJumps(null,true),0,"empty chain inventory");
  Console.WriteLine("Artifact gameplay corrections: "+checks+" behavioral checks PASS");return checks;
 }
}
