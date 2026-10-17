// Execute in the real Editor through eval_file. No scene or gameplay state changes.
var type=typeof(EchoSphere.Presentation.SourceUiDocument).Assembly.GetType("EchoSphere.Presentation.SourceCanvasGeometryCache");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
object Cache(int count=32,int vertices=100000,int indices=300000,int words=100000)=>System.Activator.CreateInstance(type,flags,null,new object[]{count,vertices,indices,words},null);
var cache=Cache();var get=type.GetMethod("GetOrCreate");int checks=0;
var options=new Unity.VectorGraphics.VectorUtils.TessellationOptions {StepDistance=float.MaxValue,MaxCordDeviation=.02f,MaxTanAngleDeviation=.08f,SamplingStepSize=.01f};
Unity.VectorGraphics.BezierContour Contour(float shift=0)=>new Unity.VectorGraphics.BezierContour {Closed=true,Segments=new[]{
    new Unity.VectorGraphics.BezierPathSegment {P0=new UnityEngine.Vector2(shift,0),P1=new UnityEngine.Vector2(shift+2,7),P2=new UnityEngine.Vector2(shift+8,9)},
    new Unity.VectorGraphics.BezierPathSegment {P0=new UnityEngine.Vector2(shift+10,0),P1=new UnityEngine.Vector2(shift+10,-5),P2=new UnityEngine.Vector2(shift+2,-5)},
    new Unity.VectorGraphics.BezierPathSegment {P0=new UnityEngine.Vector2(shift,0),P1=new UnityEngine.Vector2(shift,0),P2=new UnityEngine.Vector2(shift,0)}}};
Unity.VectorGraphics.Shape Filled()=>new Unity.VectorGraphics.Shape {Contours=new[]{Contour(),Contour(3)},Fill=new Unity.VectorGraphics.SolidFill {Color=UnityEngine.Color.white,Mode=Unity.VectorGraphics.FillMode.OddEven}};
void Equal<T>(T a,T b,string name){checks++;if(!object.Equals(a,b))throw new System.Exception("Geometry mismatch "+name+": "+a+" vs "+b);}
void Arrays<T>(T[] a,T[] b,string name){Equal(a==null,b==null,name+" null");if(a==null)return;Equal(a.Length,b.Length,name+" length");for(int i=0;i<a.Length;i++)Equal(a[i],b[i],name+"["+i+"]");}
void Check(object target,Unity.VectorGraphics.Shape shape,Unity.VectorGraphics.VectorUtils.TessellationOptions opt)
{
    var expected=Unity.VectorGraphics.VectorUtils.TessellateScene(new Unity.VectorGraphics.Scene {Root=new Unity.VectorGraphics.SceneNode {Shapes=new System.Collections.Generic.List<Unity.VectorGraphics.Shape>{shape}}},opt);
    var actual=(System.Collections.Generic.List<Unity.VectorGraphics.VectorUtils.Geometry>)get.Invoke(target,new object[]{shape,opt});
    Equal(expected.Count,actual.Count,"count");
    for(int i=0;i<expected.Count;i++){var a=expected[i];var b=actual[i];Arrays(a.Vertices,b.Vertices,"vertices");Arrays(a.Indices,b.Indices,"indices");Arrays(a.UVs,b.UVs,"UVs");Equal(a.Color,b.Color,"color");Equal(a.WorldTransform,b.WorldTransform,"world");Equal(a.FillTransform,b.FillTransform,"fill transform");Equal(a.UnclippedBounds,b.UnclippedBounds,"bounds");Equal(a.SettingIndex,b.SettingIndex,"setting");if(a.Fill is Unity.VectorGraphics.SolidFill af){var bf=(Unity.VectorGraphics.SolidFill)b.Fill;Equal(af.Color,bf.Color,"fill color");Equal(af.Mode,bf.Mode,"fill mode");Equal(af.Opacity,bf.Opacity,"opacity");}}
}
var shape=Filled();Check(cache,shape,options);Check(cache,Filled(),options);
Equal((long)type.GetProperty("Hits").GetValue(cache),1L,"initial exact hit");
var initial=get.Invoke(cache,new object[]{shape,options});
var segment=shape.Contours[0].Segments[0];segment.P1.x+=.00001f;shape.Contours[0].Segments[0]=segment;Check(cache,shape,options);
Equal((long)type.GetProperty("Misses").GetValue(cache),2L,"control point exact miss");
var again=get.Invoke(cache,new object[]{Filled(),options});Equal(object.ReferenceEquals(initial,again),true,"input mutation did not corrupt cache");
((Unity.VectorGraphics.SolidFill)shape.Fill).Mode=Unity.VectorGraphics.FillMode.NonZero;Check(cache,shape,options);
((Unity.VectorGraphics.SolidFill)shape.Fill).Opacity=.4f;Check(cache,shape,options);
shape.Contours[0].Closed=false;Check(cache,shape,options);
shape.FillTransform=Unity.VectorGraphics.Matrix2D.Translate(new UnityEngine.Vector2(4,2));Check(cache,shape,options);
shape.IsConvex=true;Check(cache,shape,options);
for(int pass=0;pass<10;pass++){
    var stroke=new Unity.VectorGraphics.Stroke {Color=UnityEngine.Color.white,HalfThickness=.7f+pass*.1f,Pattern=new[]{2f,1f},PatternOffset=pass*.2f,TippedCornerLimit=2+pass};
    var stroked=new Unity.VectorGraphics.Shape {Contours=new[]{Contour()},PathProps=new Unity.VectorGraphics.PathProperties {Stroke=stroke,Head=pass%2==0?Unity.VectorGraphics.PathEnding.Round:Unity.VectorGraphics.PathEnding.Square,Tail=Unity.VectorGraphics.PathEnding.Chop,Corners=pass%2==0?Unity.VectorGraphics.PathCorner.Round:Unity.VectorGraphics.PathCorner.Beveled}};
    Check(cache,stroked,options);Check(cache,stroked,options);stroke.Pattern[0]+=.125f;Check(cache,stroked,options);
}
for(int i=0;i<4;i++){var opt=options;if(i==0)opt.StepDistance=3;if(i==1)opt.MaxCordDeviation=.1f;if(i==2)opt.MaxTanAngleDeviation=.2f;if(i==3)opt.SamplingStepSize=.04f;Check(cache,Filled(),opt);Check(cache,Filled(),opt);}
var limited=Cache(2,20000,60000,20000);for(int i=0;i<8;i++){var value=Filled();value.Contours[0]=Contour(i);Check(limited,value,options);if((int)type.GetProperty("Count").GetValue(limited)>2)throw new System.Exception("Entry bound exceeded");}
var tiny=Cache(2,1,1,10);Check(tiny,Filled(),options);Equal((int)type.GetProperty("Count").GetValue(tiny),0,"oversize not retained");
var native=Unity.VectorGraphics.VectorUtils.TessellateScene(new Unity.VectorGraphics.Scene {Root=new Unity.VectorGraphics.SceneNode {Shapes=new System.Collections.Generic.List<Unity.VectorGraphics.Shape>{Filled()}}},options);
int vertexBudget=0,indexBudget=0;foreach(var g in native){vertexBudget+=g.Vertices.Length;indexBudget+=g.Indices.Length;}
var bounded=Cache(32,System.Math.Max(1,vertexBudget),System.Math.Max(1,indexBudget),20000);
for(int i=0;i<6;i++){var value=Filled();value.Contours[0]=Contour(i*.25f);Check(bounded,value,options);if((int)type.GetProperty("RetainedVertices").GetValue(bounded)>vertexBudget||(int)type.GetProperty("RetainedIndices").GetValue(bounded)>indexBudget)throw new System.Exception("Geometry budget exceeded");}
type.GetMethod("Clear").Invoke(cache,null);Equal((int)type.GetProperty("Count").GetValue(cache),0,"clear count");Equal((int)type.GetProperty("RetainedVertices").GetValue(cache),0,"clear vertices");Equal((int)type.GetProperty("RetainedIndices").GetValue(cache),0,"clear indices");
return "PASS exact native Canvas geometry cache: "+checks+" comparisons; curves, compound fills, strokes, dashes, mutations, options, eviction and oversized bypass";
