// Retained topology must equal the former allocation-based projection, including
// out-of-range links, missing shapes, disabled nodes and in-place shape changes.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var runtime=typeof(EchoSphere.Runtime.EchoSphereRuntime);var assembly=runtime.Assembly;
var type=runtime.GetNestedType("NativeNetworkProjectionBuffers",flags);var owner=System.Activator.CreateInstance(type,true);
var networkType=assembly.GetType("EchoSphere.Presentation.NativeWorldNetwork");var nodeType=networkType.GetNestedType("Node",flags);
var topology=(System.Action<EchoSphere.Core.SphereNetworkState,int>)System.Delegate.CreateDelegate(typeof(System.Action<EchoSphere.Core.SphereNetworkState,int>),owner,type.GetMethod("Topology",flags));
object Field(object value,string name)=>value.GetType().GetField(name,flags).GetValue(value);
string Fingerprint(object value){if(value==null)return "null";if(value is System.Array array)return "["+string.Join(",",array.Cast<object>().Select(Fingerprint))+"]";if(value.GetType().IsPrimitive)return value.ToString();return "{"+string.Join(",",value.GetType().GetFields(flags).Where(f=>!f.IsStatic).OrderBy(f=>f.Name).Select(f=>f.Name+":"+Fingerprint(f.GetValue(value))))+"}";}
int checks=0;var state=new EchoSphere.Core.SphereNetworkState();
foreach(int i in new[]{0,1,2,7,-1})state.Nodes.Add(i);
state.Links.Add(new EchoSphere.Core.SphereNetworkLink(0,1,1));state.Links.Add(new EchoSphere.Core.SphereNetworkLink(1,7,2));state.Links.Add(new EchoSphere.Core.SphereNetworkLink(-1,0,2));
foreach(var kind in System.Enum.GetValues(typeof(EchoSphere.Core.SphereNetworkFormation)).Cast<EchoSphere.Core.SphereNetworkFormation>())foreach(int count in new[]{0,1,3,8}){
 var shape=new EchoSphere.Core.SphereNetworkShape(kind,1,new System.Collections.Generic.List<int>{0,1,2,7,-1});typeof(EchoSphere.Core.SphereNetworkState).GetProperty("DominantFormation").SetValue(state,shape);
 foreach(bool active in new[]{true,false}){
 shape.Active=active;var nodes=System.Array.CreateInstance(nodeType,count);var result=runtime.GetMethod("ProjectNativeNetworkState",flags).Invoke(null,new object[]{state,nodes});topology(state,count);
 var actual=Field(owner,"Network");actual.GetType().GetField("Spheres",flags).SetValue(actual,nodes);
 if(Fingerprint(Field(result,"Item1"))!=Fingerprint(actual)||Fingerprint(Field(result,"Item2"))!=Fingerprint(Field(owner,"External")))throw new System.Exception("Projection mismatch "+kind+" / "+count+" / "+active);checks++;
 }}
topology(state,8);long before=System.GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)topology(state,8);long allocated=System.GC.GetAllocatedBytesForCurrentThread()-before;
if(allocated!=0)throw new System.Exception("Stable topology allocated "+allocated+" bytes");
return new{result="PASS",checks,stableTopologyAllocBytes=allocated,iterations=1000};
