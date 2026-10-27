// Compare the production scissor with the former full-target composite using
// actual GPU output. No gameplay or preferences are touched.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;
var wt=assembly.GetType("EchoSphere.Presentation.NativeWorldSurface");
var shapes=assembly.GetType("EchoSphere.Presentation.NativeWorldShapes");
object Call(object target,string name,params object[] input){var type=target as System.Type??target.GetType();var method=type.GetMethods(flags).Single(m=>m.Name==name);var parameters=method.GetParameters();var args=new object[parameters.Length];for(int i=0;i<args.Length;i++)args[i]=i<input.Length?input[i]:parameters[i].DefaultValue;return method.Invoke(target is System.Type?null:target,args);}
var go=new UnityEngine.GameObject("World shadow composite GPU acceptance");var world=go.AddComponent(wt);
var material=new UnityEngine.Material(UnityEngine.Resources.Load<UnityEngine.Shader>("EchoSphere/NativeWorldSolid"));
var mesh=(UnityEngine.Mesh)Call(shapes,"Rectangle",-13f,-9f,26f,18f);
int cases=0,maxDelta=0,maxAlpha=0,maxWeighted=0,cacheChecks=0;double maxMean=0;long fullPixels=0,boundedPixels=0;var details=new System.Collections.Generic.List<object>();
try{
Call(world,"Initialize");
foreach(float angle in new[]{0f,23f,-40f})foreach(float dpr in new[]{1f,1.75f})foreach(string blend in new[]{"source-over","lighter","screen"})foreach(var offset in new[]{UnityEngine.Vector2.zero,new UnityEngine.Vector2(11,-9),new UnityEngine.Vector2(-8,6)})foreach(var position in new[]{new UnityEngine.Vector2(80.37f,47.63f),new UnityEngine.Vector2(1.25f,110.75f),new UnityEngine.Vector2(317,237),new UnityEngine.Vector2(-60,-60)}){
 Call(world,"SetViewport",320f,240f,dpr);
 UnityEngine.Color32[] Render(bool full,UnityEngine.Vector2? moving=null){var at=moving??position;wt.GetField("FullShadowCompositeForValidation",flags).SetValue(world,full);wt.GetField("DisableShadowCacheForValidation",flags).SetValue(world,full);wt.GetField("DisableBatchCullingForValidation",flags).SetValue(world,full);Call(world,"Begin",new UnityEngine.Vector2(160,120));Call(world,"ResetTransform");
 var properties=new UnityEngine.MaterialPropertyBlock();properties.SetVector("_NativeColor",new UnityEngine.Vector4(.4f,.7f,.9f,.7f));
 wt.GetProperty("Blend",flags).SetValue(world,blend);
 Call(world,"DrawNativeMesh",mesh,material,properties,(UnityEngine.Matrix4x4?)UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(at.x,at.y,0),UnityEngine.Quaternion.Euler(0,0,angle),UnityEngine.Vector3.one),(UnityEngine.Color?)new UnityEngine.Color(.8f,.2f,.7f,.65f),7f,true);
 var batches=(System.Collections.IList)wt.GetField("_batches",flags).GetValue(world);var batch=batches[0];batch.GetType().GetField("X",flags).SetValue(batch,offset.x);batch.GetType().GetField("Y",flags).SetValue(batch,offset.y);
 Call(world,"End");var texture=(UnityEngine.RenderTexture)wt.GetProperty("UiTexture",flags).GetValue(world);var old=UnityEngine.RenderTexture.active;var copy=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false);try{UnityEngine.RenderTexture.active=texture;copy.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);copy.Apply();return copy.GetPixels32();}finally{UnityEngine.RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(copy);}}
 var reference=Render(true);long before=(long)wt.GetProperty("ShadowCompositePixelCount",flags).GetValue(world);var actual=Render(false);long after=(long)wt.GetProperty("ShadowCompositePixelCount",flags).GetValue(world);
 int delta=0,visible=0,alphaDelta=0,weighted=0;double sum=0;for(int i=0;i<reference.Length;i++){var a=reference[i];var b=actual[i];alphaDelta=System.Math.Max(alphaDelta,System.Math.Abs(a.a-b.a));int wd=System.Math.Max(System.Math.Max(System.Math.Abs(a.r*a.a-b.r*b.a),System.Math.Abs(a.g*a.a-b.g*b.a)),System.Math.Abs(a.b*a.a-b.b*b.a))/255;weighted=System.Math.Max(weighted,wd);sum+=wd;if(a.a>0)visible++;delta=System.Math.Max(delta,System.Math.Max(System.Math.Max(System.Math.Abs(a.r-b.r),System.Math.Abs(a.g-b.g)),System.Math.Max(System.Math.Abs(a.b-b.b),System.Math.Abs(a.a-b.a))));}if(position.x>=0&&visible==0)throw new System.Exception("Empty GPU reference");maxDelta=System.Math.Max(maxDelta,delta);cases++;fullPixels+=before;boundedPixels+=after;details.Add(new{angle,dpr,blend,x=position.x,y=position.y,delta,alphaDelta,weighted,mean=sum/reference.Length,before,after,visible,culled=wt.GetProperty("CulledBatchCount",flags).GetValue(world)});

 maxAlpha=System.Math.Max(maxAlpha,alphaDelta);maxWeighted=System.Math.Max(maxWeighted,weighted);maxMean=System.Math.Max(maxMean,sum/reference.Length);
 if(alphaDelta>4||weighted>5||sum/reference.Length>.05)throw new System.Exception("Visible local-shadow regression");
 if(position.x>=0){Render(false);Render(false,position+new UnityEngine.Vector2(.37f,.21f));if((int)wt.GetProperty("ShadowCacheHits",flags).GetValue(world)<1)throw new System.Exception("Local mask not retained");cacheChecks++;}
}
if(boundedPixels>=fullPixels)throw new System.Exception("Shadow composite coverage did not decrease");
var result=new{result="PASS",cases,maxDelta,maxAlpha,maxWeighted,maxMean,cacheChecks,fullPixels,boundedPixels,details};System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/world-local-shadow-comparison.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result="PASS",cases,maxDelta,maxAlpha,maxWeighted,maxMean,cacheChecks};
}finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(material);}
