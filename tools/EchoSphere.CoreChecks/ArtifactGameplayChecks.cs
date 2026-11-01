using EchoSphere.Core;
internal static class ArtifactGameplayChecks
{
 internal static int Run(){int checks=0;void Near(float value,float expected,string why){checks++;if(Math.Abs(value-expected)>.00001f)throw new Exception("Artifact contract: "+why+" "+value+" expected "+expected);}
  foreach(bool ru in new[]{false,true})foreach(var id in new[]{"prism_filter","prism_crown"}){var text=ArtifactGameplayRules.DescriptionOverride(id,ru);Near(string.IsNullOrWhiteSpace(text)?0:1,1,"prism contract localized "+id);}
  Near(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_split","",false,true,false),0,"crown requires reflection path");
  Near(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_split","",true,false,false),1,"filter enables one relay");
  Near(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_split","",true,true,false),2,"crown adds one relay");
  Near(SphereEvolutionCombatRules.GetPrismNetworkRelayDamageMultiplier(),.42f,"relay actual damage");
  foreach(var branch in new[]{"pulse_wave","pulse_burst","pulse_resonator"})foreach(int mode in new[]{-1,0,1,2})foreach(float power in new[]{1f,1.15f,1.3f})foreach(bool hasCrown in new[]{false,true}){
   int count=1+(hasCrown?1:0)+(branch=="pulse_burst"&&(mode==2||mode<0)?1:0);
   Near(SphereEvolutionCombatRules.GetPulseWaveCount(branch,mode,hasCrown),count,"pulse authored wave count");
   for(int wave=0;wave<count;wave++)Near(SphereEvolutionCombatRules.GetPulseWaveDamageMultiplier(branch,mode,power,wave),wave==0?1f:.46f+(branch=="pulse_burst"?.14f:0f)*(branch=="pulse_burst"&&mode<0?power:1f)*power,"pulse extraction preserves combat");
  }
  foreach(int links in new[]{-1,0,1,5,10,50})Near(ArtifactGameplayRules.ConditionalDamageMultiplier(new List<string>{"echo_weaver"},new ArtifactDamageContext{ActiveNetworkLinks=links}),1+.01f*Math.Clamp(links,0,10),"weaver active link cap");
  foreach(var key in new[]{"sphereDamage","sphereRadius","sphereDelay","cooldown","critChance"})Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("axiom_core"),key,true),key=="sphereDamage"||key=="sphereRadius"?.05f:key=="sphereDelay"||key=="cooldown"?-.05f:0f,"axiom explicit effects");
  Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("echo_weaver"),"sphereDamage",true),0,"weaver no unconditional damage");
  Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("echo_weaver"),"cooldown",true),0,"weaver no hidden cooldown");
  Near(ArtifactGameplayRules.TickTimeSplitter(0,.1f,0,.2f,true),5,"time splitter starts at stop end");
  Near(ArtifactGameplayRules.TickTimeSplitter(5,0,0,1,true),4,"time splitter counts down");
  Near(ArtifactGameplayRules.TickTimeSplitter(1,0,0,2,true),0,"time splitter expires");
  Near(ArtifactGameplayRules.TickTimeSplitter(5,.1f,0,.2f,false),0,"legacy no time splitter timer");
  Near(ArtifactGameplayRules.TickTimeSplitter(0,2,1,1,true),0,"time splitter not during stop");
  Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("time_splitter"),"critChance",true),0,"no hidden time splitter crit");
  float stored=0;
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,30,0,true,true)?1:0,0,"mandala partial");Near(stored,30,"mandala accumulates HP loss");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,20,0,true,true)?1:0,1,"mandala threshold");Near(stored,0,"mandala consumes threshold");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,300,0,true,true)?1:0,1,"mandala at most one per hit");Near(stored,49,"mandala bounded remainder");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,20,5,true,true)?1:0,0,"mandala full shield cap");Near(stored,49,"full shields do not bank damage");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,100,0,false,true)?1:0,0,"mandala cannot prevent death");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,0,0,true,true)?1:0,0,"blocked hits no charge");
  Near(ArtifactGameplayRules.AccumulateMandala(ref stored,100,0,true,false)?1:0,0,"legacy no mandala");Near(stored,0,"disabled clears accumulation");
  Near(ArtifactGameplayRules.ResonanceChargeMultiplier(null,true),1,"empty resonance inventory");
  Near(ArtifactGameplayRules.ResonanceChargeMultiplier(new List<string>{"resonant_leaf"},true),1.10f,"leaf charge");
  Near(ArtifactGameplayRules.ResonanceChargeMultiplier(new List<string>{"resonance_lattice"},true),1.15f,"lattice charge");
  Near(ArtifactGameplayRules.ResonanceChargeMultiplier(new List<string>{"resonant_leaf","resonance_lattice"},true),1.25f,"additive resonance gain");
  Near(ArtifactGameplayRules.ResonanceChargeMultiplier(new List<string>{"resonant_leaf","resonance_lattice"},false),1,"legacy resonance charge");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(null,true),1,"no fold inventory");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(new List<string>{"universal_fold"},true),1,"fold needs completed synergy");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(new List<string>{"universal_fold","heavy_core","resonance_core"},true),1.02f,"fold counts completed pair");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(new List<string>{"universal_fold","heavy_core"},true),1,"fold ignores partial pair");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(ArtifactRewardRules.All.Select(x=>x.Id).ToList(),true),1.20f,"fold caps ten pairs");
  Near(ArtifactGameplayRules.UniversalFoldMultiplier(ArtifactRewardRules.All.Select(x=>x.Id).ToList(),false),1,"legacy fold path");
  var keys=new[]{"sphereDamage","cooldown","sniperDamage","sniperRadius","critChance","sphereRadius"};
  foreach(var artifact in ArtifactRewardRules.All)foreach(var key in keys)Near(ArtifactGameplayRules.Effect(artifact,key,false),artifact.GetEffect(key),"legacy "+artifact.Id+"/"+key);
  foreach(var artifact in ArtifactRewardRules.All.Where(x=>!ArtifactGameplayRules.IsCorrected(x.Id)))foreach(var key in keys)Near(ArtifactGameplayRules.Effect(artifact,key,true),artifact.GetEffect(key),"unrelated "+artifact.Id+"/"+key);
  var scope=new List<string>{"sniper_scope"};Near(ArtifactGameplayRules.EffectTotal(scope,"sniperDamage",true),.1f,"scope sniper damage");Near(ArtifactGameplayRules.EffectTotal(scope,"sphereDamage",true),0,"scope no hidden global buff");Near(ArtifactGameplayRules.EffectTotal(scope,"sphereDamage",false),.06f,"scope legacy global buff");
  var crown=new List<string>{"sniper_crown"};Near(ArtifactGameplayRules.EffectTotal(crown,"sniperRadius",true),.1f,"crown range");Near(ArtifactGameplayRules.EffectTotal(crown,"sphereDamage",true),0,"crown no hidden damage");Near(ArtifactGameplayRules.EffectTotal(crown,"cooldown",true),0,"crown no hidden cooldown");
  var chain=new List<string>{"chain_crown"};Near(ArtifactGameplayRules.ExtraChainJumps(chain,true),1,"chain extra jump");Near(ArtifactGameplayRules.ExtraChainJumps(chain,false),0,"legacy chain unchanged");Near(ArtifactGameplayRules.EffectTotal(chain,"sphereDamage",true),0,"chain no global buff");Near(ArtifactGameplayRules.EffectTotal(chain,"cooldown",true),0,"chain no cooldown");
  var all=new List<string>{"sniper_scope","sniper_crown","chain_crown","heavy_core"};Near(ArtifactGameplayRules.EffectTotal(all,"standardDamage",true),.15f,"other artifact survives adapter");
  Near(ArtifactGameplayRules.EffectTotal(null,"sphereDamage",true),0,"empty inventory");Near(ArtifactGameplayRules.ExtraChainJumps(null,true),0,"empty chain inventory");
  foreach(var id in new[]{"unified_mind","lone_bastion","fivefold_resonance","relay_matrix","network_relay","network_coil","geometry_die","mirror_network","singularity_engine","triangle_circuit","resonance_core"})Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get(id),"sphereDamage",true),0,"no unconditional placeholder "+id);
  float Conditional(string id,ArtifactDamageContext c)=>ArtifactGameplayRules.ConditionalDamageMultiplier(new List<string>{id},c);
  Near(Conditional("lone_bastion",new ArtifactDamageContext {UniqueTypes=1}),1.3f,"single type");Near(Conditional("lone_bastion",new ArtifactDamageContext {UniqueTypes=2}),1,"mixed type no bastion");
  Near(Conditional("fivefold_resonance",new ArtifactDamageContext {UniqueTypes=1}),1.04f,"first distinct type counts");Near(Conditional("fivefold_resonance",new ArtifactDamageContext {UniqueTypes=10}),1.2f,"fivefold cap");
  Near(Conditional("relay_matrix",new ArtifactDamageContext {AnotherLevelSeven=true}),1.1f,"other VII relay");Near(Conditional("relay_matrix",new ArtifactDamageContext()),1,"self VII not enough");
  foreach(var pair in new[]{("network_relay",1.05f),("network_coil",1.08f),("mirror_network",1.12f)}){Near(Conditional(pair.Item1,new ArtifactDamageContext {UniqueTypes=2}),pair.Item2,pair.Item1+" eligible");Near(Conditional(pair.Item1,new ArtifactDamageContext {UniqueTypes=1}),1,pair.Item1+" ineligible");}
  Near(Conditional("unified_mind",new ArtifactDamageContext {StrongestLevel=7}),1.21f,"highest level exact");
  Near(Conditional("singularity_engine",new ArtifactDamageContext {UniqueTypes=2}),1.18f,"small build");Near(Conditional("singularity_engine",new ArtifactDamageContext {UniqueTypes=3}),1,"large build");
  Near(Conditional("triangle_circuit",new ArtifactDamageContext {TriangleActive=true}),1.15f,"triangle node");Near(Conditional("triangle_circuit",new ArtifactDamageContext()),1,"outside triangle");
  Near(Conditional("resonance_core",new ArtifactDamageContext {NearOtherSphere=true}),1.12f,"near another node");Near(Conditional("resonance_core",new ArtifactDamageContext()),1,"isolated node");
  Near(Conditional("geometry_die",new ArtifactDamageContext {AnyActiveGeometry=true}),1.08f,"active secondary also qualifies");Near(Conditional("geometry_die",new ArtifactDamageContext()),1,"no active geometry");
  Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("zero_sphere"),"sphereDamage",true),.35f,"zero exact global35");Near(ArtifactGameplayRules.Effect(ArtifactRewardRules.Get("formation_compass"),"sphereRadius",true),.08f,"compass single8");
  foreach(var pair in new[]{("chain_battery","chainDamage"),("shotgun_shell","shotgunDamage"),("aura_mist","auraRadius")}){Near(ArtifactGameplayRules.EffectTotal(new List<string>{pair.Item1},pair.Item2,true),.1f,"family-specific "+pair.Item1);Near(ArtifactGameplayRules.EffectTotal(new List<string>{pair.Item1},"sphereDamage",true),0,"no hidden global "+pair.Item1);}
  Near(ArtifactGameplayRules.EffectTotal(new List<string>{"folded_core"},"sphereDamage",true),.08f,"folded core exact8");Near(ArtifactGameplayRules.EffectTotal(new List<string>{"overclock"},"sphereDamage",true),-.05f,"overclock actual cost");Near(ArtifactGameplayRules.EffectTotal(new List<string>{"overclock"},"sphereDelay",true),-.12f,"overclock cadence");
  foreach(var id in new[]{"quantum_fold","void_star"}){Near(ArtifactGameplayRules.EffectTotal(new List<string>{id},"critChance",true),0,"no hidden crit "+id);Near(ArtifactGameplayRules.EffectTotal(new List<string>{id},"sphereDamage",true),0,"no hidden damage "+id);}
  Near(ArtifactRewardRules.GetStellaChoiceCount(new List<string>{"quantum_fold"}),4,"quantum extra choice preserved");
  foreach(var id in new[]{"vampire_ring","predator_claw","foresight_eye","mirror","void_mark","void_lantern","gravity_hook","pulse_driver","overclock","void_star"})foreach(bool ru in new[]{false,true}){Near(string.IsNullOrWhiteSpace(ArtifactGameplayRules.DescriptionOverride(id,ru))?0:1,1,"complete localized explanation "+id);}
  foreach(var pair in new[]{("triangle_engine",SphereNetworkFormation.Triangle),("ring_engine",SphereNetworkFormation.Ring),("lattice_engine",SphereNetworkFormation.Lattice),("fractal_engine",SphereNetworkFormation.Fractal)}){var ids=new List<string>{pair.Item1};Near(ArtifactGameplayRules.FormationMultiplier(ids,pair.Item2,true),1.12f,"geometry engine "+pair.Item1);Near(ArtifactGameplayRules.FormationMultiplier(ids,SphereNetworkFormation.Line,true),1,"unrelated geometry unchanged");Near(ArtifactGameplayRules.FormationMultiplier(ids,pair.Item2,false),1,"legacy engine unchanged");Near(ArtifactGameplayRules.EffectTotal(ids,"sphereDamage",true),0,"engine no placeholder damage");Near(ArtifactGameplayRules.EffectTotal(ids,"cooldown",true),0,"engine no placeholder cooldown");}
  Near(ArtifactGameplayRules.FormationMultiplier(new List<string>{"lattice_chip","lattice_engine"},SphereNetworkFormation.Lattice,true),1.232f,"lattice components compose");Near(ArtifactGameplayRules.FormationMultiplier(new List<string>{"fractal_seed","fractal_engine"},SphereNetworkFormation.Fractal,true),1.232f,"fractal components compose");
  Near(ArtifactGameplayRules.RecoveryDurationMultiplier(new List<string>{"signal_knot"},true),1/1.05f,"recovery speed to duration conversion");Near(ArtifactGameplayRules.RecoveryDurationMultiplier(new List<string>{"signal_knot"},false),1,"legacy recovery unchanged");
  Near(Conditional("echo_thread",new ArtifactDamageContext {HasActiveLink=true}),1.05f,"linked sphere thread");Near(Conditional("echo_thread",new ArtifactDamageContext()),1,"isolated sphere thread off");
  Near(ArtifactGameplayRules.CounterattackChance(new List<string>{"mirror","mirror_dust"},true),.28f,"reflection chances compose");Near(ArtifactGameplayRules.CounterattackChance(new List<string>{"mirror_dust"},true),.08f,"dust standalone chance");Near(ArtifactGameplayRules.CounterattackChance(new List<string>{"mirror_dust"},false),0,"legacy dust no counter");Near(ArtifactGameplayRules.CounterattackChance(new List<string>{"mirror","mirror_dust"},false),.20f,"legacy mirror unchanged");Near(ArtifactGameplayRules.EffectTotal(new List<string>{"mirror_dust"},"sphereDamage",true),0,"dust no hidden damage");
  Console.WriteLine("Artifact gameplay corrections: "+checks+" behavioral checks PASS");return checks;
 }
}
