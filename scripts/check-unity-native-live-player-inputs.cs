if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
UnityEditor.EditorApplication.isPaused=true;
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
string[] keys={"level","mutationStage","buffTimer","shieldTimer","shieldVisualPulse","deathTimer","chaosOrbBuff","chaosOrbBuffTimer","teleportDamageBuffTimer","fireCatalystTimer","architectFormationType","architectFormationChangedAt"};
string[] names={"_level","_mutationStage","_buffTimer","_shieldTimer","_shieldVisualPulse","_deathTimer","_chaosOrbBuff","_chaosOrbBuffTimer","_teleportDamageBuffTimer","_fireCatalystTimer","_architectFormationType","_architectFormationChangedAt"};
object[] values={9,3,4.25f,6.5f,.7f,.8f,"dmg",2.5f,3.5f,4.5f,EchoSphere.Core.SphereNetworkFormation.Cluster,125f};
var saved=names.Select(name=>type.GetField(name,flags).GetValue(runtime)).ToArray();int checks=0;
try{
 for(int variant=0;variant<2;variant++){
  for(int i=0;i<names.Length;i++)type.GetField(names[i],flags).SetValue(runtime,variant==0?values[i]:saved[i]);
  type.GetMethod("UpdateSourceRenderState",flags).Invoke(runtime,null);var scene=(EchoSphere.Presentation.V)type.GetField("_sourceRenderState",flags).GetValue(runtime);var player=scene["player"];
  for(int i=0;i<keys.Length;i++){var raw=type.GetField(names[i],flags).GetValue(runtime);var actual=player[keys[i]];if(raw==null){if(!actual.Null)throw new Exception(keys[i]);}else if(raw is string text){if(actual.Text!=text)throw new Exception(keys[i]);}else if(raw is Enum){if(actual.Text!=raw.ToString().ToLowerInvariant())throw new Exception(keys[i]);}else if(!actual.Number.Equals(Convert.ToDouble(raw)))throw new Exception(keys[i]);checks++;}
  var expected=type.GetMethod("LegacyPlayerState",flags).Invoke(runtime,new object[]{player});var actualPlayer=type.GetMethod("NativePlayerState",flags).Invoke(runtime,null);
  foreach(var field in actualPlayer.GetType().GetFields(flags|System.Reflection.BindingFlags.Public)){if(!object.Equals(field.GetValue(actualPlayer),field.GetValue(expected)))throw new Exception("typed player "+field.Name);checks++;}
  var hud=runtime.ReadNativeHudState();string suffix=" "+player["level"].Text;if(!hud.Level.EndsWith(suffix,StringComparison.Ordinal))throw new Exception("live HUD level");checks++;
 }
}finally{for(int i=0;i<names.Length;i++)type.GetField(names[i],flags).SetValue(runtime,saved[i]);type.GetMethod("UpdateSourceRenderState",flags).Invoke(runtime,null);}
return new {checks,scope="Twelve live display bindings, typed player fields and HUD level; runtime values restored; no simulation/save mutation"};
