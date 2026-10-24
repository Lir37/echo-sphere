// Numeric regression for actual shared two-link gait solver.
var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var type=a.GetType("EchoSphere.Presentation.NativeWorldCreatures");var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;var instance=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;var method=type.GetMethod("SolveOrganicLeg",flags);int checks=0;
void Require(bool ok,string why){checks++;if(!ok)throw new System.Exception(why);}
var configs=new[]{new {Length=.78,Body=.24,Hips=new[]{-.42,-.08,.26}},new {Length=.30,Body=.20,Hips=new[]{-.36,-.02}},new {Length=.55,Body=.24,Hips=new[]{-.34,.12}},new {Length=.72,Body=.30,Hips=new[]{-.45,-.12,.20,.48}},new {Length=.60,Body=.30,Hips=new[]{-.34,.10}},new {Length=.90,Body=.24,Hips=new[]{-.22,.16}},new {Length=.58,Body=.18,Hips=new[]{-.36,-.12,.12,.36}},new {Length=1.0,Body=.38,Hips=new[]{-.34,.18}}};
object Pose(double r,double hip,int side,double body,double length,double spread,double phase,double wave=0,bool flying=false)=>method.Invoke(null,new object[]{r,hip,side,body,length,spread,phase,1d,wave,flying});
UnityEngine.Vector2 Point(object p,string n)=>(UnityEngine.Vector2)p.GetType().GetField(n,instance).GetValue(p);
foreach(var c in configs)for(int i=0;i<c.Hips.Length;i++)for(int side=-1;side<=1;side+=2)for(int frame=0;frame<48;frame++){
 double r=19,spread=(i-(c.Hips.Length-1)*.5)*.17,phase=frame/48d*Math.PI*2,wave=c.Length==.30?Math.Sin(phase)*r*.14:0;var pose=Pose(r,c.Hips[i],side,c.Body,c.Length,spread,phase,wave);var hip=Point(pose,"Hip");var knee=Point(pose,"Knee");var foot=Point(pose,"Foot");
 Require(Math.Abs(UnityEngine.Vector2.Distance(hip,knee)-c.Length*r*.72)<.0001,"Upper bone length");Require(Math.Abs(UnityEngine.Vector2.Distance(knee,foot)-c.Length*r*.80)<.0001,"Lower bone length");Require(foot.y*side>0&&knee.y*side>0,"Anatomical side preserved");
 var first=Pose(r,c.Hips[i],side,c.Body,c.Length,spread,.10*Math.PI*2);var second=Pose(r,c.Hips[i],side,c.Body,c.Length,spread,.12*Math.PI*2);double bodyDistance=r*(2*Math.Min(.30,c.Length*.60))/.64*.02;
 Require(Math.Abs(Point(second,"Foot").x+bodyDistance-Point(first,"Foot").x)<.0001,"Planted foot stays in world position");
}
return new {status="PASS",checks,scope="Fixed bone lengths, anatomical sides, planted-foot world lock across actual ground anatomies"};
