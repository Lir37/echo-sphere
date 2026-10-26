using System;
using System.Reflection;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.UIElements;

// Explicit Editor checks: no player build, save mutation or scene asset writes.
public static class EchoSphereNativePerformanceChecks
{
    private const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object MenuSnapshot()
    {
        var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var documents=new System.Collections.Generic.List<object>();
        foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>()){
            var root=document.rootVisualElement;int nodes=0,portraits=0;var buttons=new System.Collections.Generic.List<string>();
            void Visit(VisualElement e){if(e.userData!=null&&e.userData.GetType().Name=="ClipTag")return;nodes++;if(e is Button button)buttons.Add(button.name);if(e is Image image&&image.name=="character-portrait"&&image.sprite!=null)portraits++;for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i]);}
            Visit(root);documents.Add(new {name=document.gameObject.name,nodes,portraits,buttons,visibility=root.style.visibility.ToString(),opacity=root.style.opacity.ToString()});
        }
        object Field(string name)=>runtime==null?null:runtime.GetType().GetField(name,Hidden)?.GetValue(runtime);
        var screen=Field("_nativeCharacterScreen");int glassCount=0,prepares=0;
        if(screen!=null)foreach(var glass in (System.Collections.IList)screen.GetType().GetField("_glass",Hidden).GetValue(screen)){glassCount++;prepares+=(int)glass.GetType().GetProperty("PrepareCount",Hidden).GetValue(glass);}
        var world=Field("_worldSurface");long shadowPixels=world==null?0:(long)world.GetType().GetProperty("ShadowPixelCount",Hidden).GetValue(world);
        return new {frame=Time.frameCount,width=Screen.width,height=Screen.height,menu=Field("_mainMenu"),region=Field("_regionSelect"),page=Field("_mainMenuPage"),tutorial=Field("_tutorialStep"),documents,glassCount,prepares,shadowPixels};
    }
    public static object Click(string name)
    {
        Button found=null;
        foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>()){
            void Visit(VisualElement e){if(e.style.visibility==Visibility.Hidden||e.style.display==DisplayStyle.None||(e.style.opacity.keyword==StyleKeyword.Undefined&&e.style.opacity.value<=0))return;if(e is Button button&&button.name==name&&button.enabledInHierarchy)found=button;for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i]);}
            Visit(document.rootVisualElement);
        }
        if(found==null)throw new InvalidOperationException("No visible enabled button: "+name);
        using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=found;found.SendEvent(submit);}
        return new {clicked=name,frame=Time.frameCount};
    }
    public static object ShadowRoi()
    {
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeWorldSurface",true);
        var owner=new GameObject("Shadow ROI validation");var surface=owner.AddComponent(type);
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-19.3f,-11.7f),new Vector3(22.8f,-11.7f),new Vector3(22.8f,14.2f),new Vector3(-19.3f,14.2f)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();
        var material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var props=new MaterialPropertyBlock();props.SetVector("_NativeColor",new Vector4(.3f,.7f,.9f,.63f));
        var previous=RenderTexture.active;int cases=0,worst=0;long roiPixels=0,fullPixels=0;
        object Call(string method,params object[] args)=>type.GetMethod(method,Hidden).Invoke(surface,args);
        Color32[] Render(bool full,float density,Vector2 position,float blur,bool label,int blend)
        {
            material.shader=Resources.Load<Shader>(label?"EchoSphere/NativeWorldLabel":"EchoSphere/NativeWorldSolid");props.SetTexture("_Glyphs",Texture2D.whiteTexture);props.SetVector("_Glyphs_TexelSize",new Vector4(1,1,1,1));props.SetVector("_LabelOffset",new Vector4(17,-7,0,0));props.SetVector("_LabelColor",new Vector4(.3f,.7f,.9f,.63f));
            type.GetField("FullShadowViewportForValidation",Hidden).SetValue(surface,full);
            Call("SetViewport",320f,192f,density);Call("Begin",new Vector2(160,96));
            var method=type.GetMethod("DrawNativeMesh",Hidden);var parameters=method.GetParameters();var args=new object[parameters.Length];
            for(int i=0;i<args.Length;i++)args[i]=parameters[i].DefaultValue;
            args[0]=mesh;args[1]=material;args[2]=props;args[3]=Matrix4x4.TRS(new Vector3(position.x,position.y),Quaternion.Euler(0,0,23),new Vector3(1.2f,.8f,1));args[4]=new Color(.7f,.2f,.8f,.7f);args[5]=blur;
            args[9]=blend==2;args[10]=blend==1;
            method.Invoke(surface,args);Call("End");
            long pixels=(long)type.GetProperty("ShadowPixelCount",Hidden).GetValue(surface);if(full)fullPixels+=pixels;else roiPixels+=pixels;
            var target=(RenderTexture)type.GetField("_target",Hidden).GetValue(surface);RenderTexture.active=target;
            var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false,true);
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
        }
        try
        {
            Call("Initialize");
            foreach(bool label in new[]{false,true})for(int blend=0;blend<3;blend++)foreach(float density in new[]{1f,1.75f})foreach(float blur in new[]{4f,16f,48f})foreach(var position in new[]{new Vector2(0,0),new Vector2(160.317f,96.193f),new Vector2(319,191),new Vector2(-15,96),new Vector2(330,90)}){
                var full=Render(true,density,position,blur,label,blend);var roi=Render(false,density,position,blur,label,blend);int peak=0;
                for(int i=0;i<full.Length;i++){peak=Math.Max(peak,Math.Abs(full[i].r-roi[i].r));peak=Math.Max(peak,Math.Abs(full[i].g-roi[i].g));peak=Math.Max(peak,Math.Abs(full[i].b-roi[i].b));peak=Math.Max(peak,Math.Abs(full[i].a-roi[i].a));}
                worst=Math.Max(worst,peak);cases++;if(peak>2)throw new InvalidOperationException($"Shadow ROI changed GPU pixels: density={density}, blur={blur}, position={position}, peak={peak}/255");
            }
            return new {cases,worstChannelError=worst,roiPixels,fullPixels,backend=SystemInfo.graphicsDeviceType.ToString()};
        }
        finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(material);}
    }

    public static object NetworkCache()
    {
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeNetworkAnalysisCache",true);
        var cache=Activator.CreateInstance(type,true);var nodes=(System.Collections.Generic.List<SphereNetworkNode>)type.GetField("Nodes",Hidden).GetValue(cache);var method=type.GetMethod("Analyze",Hidden);
        SphereNetworkState state=null;int cases=0,hits=0;
        foreach(var previous in (SphereNetworkFormation[])Enum.GetValues(typeof(SphereNetworkFormation)))for(int step=0;step<20;step++){
            nodes.Clear();for(int i=0;i<6;i++)nodes.Add(new SphereNetworkNode(new Vec2(i*1.4f+(step%3==0?.00001f:0),i%2),step%5!=0||i!=3));
            var expected=SphereNetworkRules.Analyze(nodes,SphereNetworkRules.DefaultLinkDistance,previous);
            var actual=(SphereNetworkState)method.Invoke(cache,new object[]{state,previous,SphereNetworkFormation.None,SphereNetworkFormation.None});
            if(actual.Nodes.Count!=expected.Nodes.Count||actual.Links.Count!=expected.Links.Count||actual.FormationCandidates.Count!=expected.FormationCandidates.Count||actual.DominantFormation?.Type!=expected.DominantFormation?.Type||actual.SecondaryFormation?.Type!=expected.SecondaryFormation?.Type)throw new InvalidOperationException("Network cache changed formations");
            for(int i=0;i<actual.Links.Count;i++)if(actual.Links[i].A!=expected.Links[i].A||actual.Links[i].B!=expected.Links[i].B||actual.Links[i].Distance!=expected.Links[i].Distance)throw new InvalidOperationException("Network cache changed links");
            if(ReferenceEquals(actual,method.Invoke(cache,new object[]{actual,previous,SphereNetworkFormation.None,SphereNetworkFormation.None})))hits++;else throw new InvalidOperationException("Exact repeated input was not cached");
            state=actual;cases++;
        }
        return new {cases,hits};
    }

    public static object SimulationPhases()
    {
        var owner=new GameObject("Simulation phase snapshot validation");var runtime=owner.AddComponent<EchoSphere.Runtime.EchoSphereRuntime>();runtime.enabled=false;
        var a=owner.AddComponent<EchoSphereSimulationPhaseProbe>();
        var bOwner=new GameObject("phase B");var b=bOwner.AddComponent<EchoSphereSimulationPhaseProbe>();
        var cOwner=new GameObject("phase C");var c=cOwner.AddComponent<EchoSphereSimulationPhaseProbe>();
        var calls=new System.Collections.Generic.List<string>();
        var method=runtime.GetType().GetMethod("TickSimulationPhase",Hidden).MakeGenericMethod(typeof(EchoSphereSimulationPhaseProbe));
        runtime.RegisterSimulationAgent(a);runtime.RegisterSimulationAgent(b);
        a.Tick=()=>{calls.Add("a");runtime.RegisterSimulationAgent(c);bOwner.SetActive(false);};b.Tick=()=>calls.Add("b");c.Tick=()=>calls.Add("c");
        try{
            method.Invoke(runtime,new object[]{.016f,false});if(string.Join("",calls)!="a")throw new InvalidOperationException("Phase admitted a newly spawned or deactivated agent");
            calls.Clear();a.Tick=()=>{calls.Add("a");bOwner.SetActive(true);};
            method.Invoke(runtime,new object[]{.016f,false});if(string.Join("",calls)!="ac")throw new InvalidOperationException("Phase admitted an agent activated after the snapshot");
            calls.Clear();method.Invoke(runtime,new object[]{.016f,true});if(string.Join("",calls)!="cba")throw new InvalidOperationException("Reverse phase changed source order");
            return new {scenarios=3,snapshotOrder=true};
        }finally{UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(bOwner);UnityEngine.Object.DestroyImmediate(cOwner);}
    }

    public static object SharedSkins()
    {
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeMenuBrushCache",true);
        var acquire=type.GetMethod("Acquire",BindingFlags.Static|BindingFlags.NonPublic);var release=type.GetMethod("Release",BindingFlags.Static|BindingFlags.NonPublic);
        var paint=new EchoSphere.Presentation.NativeMenuBrush.Paint {Radius=6,Fill=Color.cyan,Colors=new[]{Color.red,Color.blue},Stops=new[]{0f,1f}};
        var a=acquire.Invoke(null,new object[]{48f,24f,1f,paint});var b=acquire.Invoke(null,new object[]{48f,24f,1f,paint});
        if(!ReferenceEquals(a,b))throw new InvalidOperationException("Identical skins did not share a texture");
        paint.Colors[0]=Color.green;var c=acquire.Invoke(null,new object[]{48f,24f,1f,paint});
        try{if(ReferenceEquals(a,c))throw new InvalidOperationException("A mutated paint reused an incompatible skin");return new {shared=true,immutableKey=true};}
        finally{release.Invoke(null,new[]{a});release.Invoke(null,new[]{b});release.Invoke(null,new[]{c});}
    }

    public static object MenuBlur()
    {
        var brush=new EchoSphere.Presentation.NativeMenuBrush();var flag=brush.GetType().GetField("AnalyticBlurForValidation",Hidden);var previous=RenderTexture.active;int cases=0,worst=0;
        Color32[] Render(bool analytic,float scale,EchoSphere.Presentation.NativeMenuBrush.Paint paint){
            flag.SetValue(brush,analytic);brush.Render(91.3f,47.7f,scale,paint);var target=brush.Texture;RenderTexture.active=target;
            var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
        }
        try{
            foreach(float scale in new[]{1f,1.75f,3f})foreach(float blur in new[]{.03f,1f,7f,28f})foreach(bool inset in new[]{false,true}){
                var shadow=new EchoSphere.Presentation.NativeMenuBrush.Shadow(blur,new Vector2(-3,5),new Color(.2f,.6f,.9f,.37f),2);
                var paint=new EchoSphere.Presentation.NativeMenuBrush.Paint {Radius=12,Border=1,Fill=new Color(.02f,.05f,.08f,.91f),BorderColor=Color.cyan,OuterA=inset?(EchoSphere.Presentation.NativeMenuBrush.Shadow?)null:shadow,Inset=inset?shadow:(EchoSphere.Presentation.NativeMenuBrush.Shadow?)null};
                var old=Render(true,scale,paint);var fast=Render(false,scale,paint);int peak=0;
                for(int i=0;i<old.Length;i++){peak=Math.Max(peak,Math.Abs(old[i].r-fast[i].r));peak=Math.Max(peak,Math.Abs(old[i].g-fast[i].g));peak=Math.Max(peak,Math.Abs(old[i].b-fast[i].b));peak=Math.Max(peak,Math.Abs(old[i].a-fast[i].a));}
                worst=Math.Max(worst,peak);cases++;if(peak>2)throw new InvalidOperationException($"Paired menu blur changed GPU pixels: scale={scale}, blur={blur}, inset={inset}, peak={peak}/255");
            }
            return new {cases,worstChannelError=worst,backend=SystemInfo.graphicsDeviceType.ToString()};
        }finally{RenderTexture.active=previous;brush.Dispose();}
    }

    public static object ShadowCache()
    {
        var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;
        var type=assembly.GetType("EchoSphere.Presentation.NativeWorldSurface",true);var shapes=assembly.GetType("EchoSphere.Presentation.NativeWorldShapes",true);
        object Optional(MethodInfo method,object target,params object[] first){var p=method.GetParameters();var args=new object[p.Length];for(int i=0;i<args.Length;i++)args[i]=i<first.Length?first[i]:p[i].DefaultValue;return method.Invoke(target,args);}
        var mesh=(Mesh)shapes.GetMethod("Rectangle",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{-20f,-15f,40f,30f});
        var owner=new GameObject("Retained shadow validation");var surface=owner.AddComponent(type);var material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var props=new MaterialPropertyBlock();var previous=RenderTexture.active;
        var draw=type.GetMethod("DrawNativeMesh",Hidden);var disabled=type.GetField("DisableShadowCacheForValidation",Hidden);int cases=0,hits=0;long bytes=0;
        object Call(string method,params object[] args)=>type.GetMethod(method,Hidden).Invoke(surface,args);
        Color32[] Render(bool cache,int step){
            disabled.SetValue(surface,!cache);float width=step>=6?360:320;
            Call("SetViewport",width,192f,step>=7?1.75f:1f);Call("Begin",new Vector2(width*.5f,96));
            props.SetVector("_NativeColor",new Vector4(step>=4?.8f:.3f,.7f,.9f,step>=2?.37f:.63f));
            var matrix=Matrix4x4.TRS(new Vector3(step>=1?155.37f:160,96),Quaternion.Euler(0,0,17),Vector3.one);
            Optional(draw,surface,mesh,material,props,matrix,new Color(step>=4?.1f:.7f,.2f,.8f,.7f),step>=3?24f:16f);
            Call("End");if(cache){hits+=(int)type.GetProperty("ShadowCacheHits",Hidden).GetValue(surface);bytes=(long)type.GetProperty("ShadowCacheBytes",Hidden).GetValue(surface);}
            var target=(RenderTexture)type.GetField("_target",Hidden).GetValue(surface);RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false,true);
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
        }
        try{
            Call("Initialize");
            for(int step=0;step<8;step++){
                if(step==5){
                    var contour=shapes.GetMethod("PolylineContour",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{true,new[]{new Vector2(-20,0),new Vector2(0,-15),new Vector2(20,0),new Vector2(0,15)}});
                    Optional(shapes.GetMethod("Tessellate",BindingFlags.Static|BindingFlags.NonPublic),null,contour,0f,mesh);
                }
                Render(true,step);Render(true,step);var actual=Render(true,step);var expected=Render(false,step);
                for(int i=0;i<actual.Length;i++)if(!actual[i].Equals(expected[i]))throw new InvalidOperationException("Retained shadow differs from uncached GPU result at step "+step);
                if((int)type.GetProperty("ShadowPassCount",Hidden).GetValue(surface)!=1)throw new InvalidOperationException("Uncached reference did not execute blur");cases++;
            }
            if(hits<8||bytes<=0||bytes>16*1024*1024)throw new InvalidOperationException("Shadow cache was not reused or exceeded its budget");
            return new {cases,hits,residentBytes=bytes,bitExact=true,geometryMutation=true,resize=true};
        }finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(material);}
    }

    public static object Instancing()
    {
        if(!SystemInfo.supportsInstancing)throw new InvalidOperationException("Validation backend does not support instancing");
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeWorldSurface",true);
        var owner=new GameObject("Ordered instance validation");var surface=owner.AddComponent(type);var mesh=new Mesh();var triangle=new Mesh();
        mesh.vertices=new[]{new Vector3(-12,-9),new Vector3(12,-9),new Vector3(12,9),new Vector3(-12,9)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();
        triangle.vertices=new[]{new Vector3(-12,-9),new Vector3(12,-9),new Vector3(0,9)};triangle.triangles=new[]{0,1,2};triangle.RecalculateBounds();
        var solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var label=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldLabel"));
        var properties=new MaterialPropertyBlock[140];for(int i=0;i<properties.Length;i++){var p=properties[i]=new MaterialPropertyBlock();p.SetVector("_NativeColor",new Vector4((i%5)/5f,(i%7)/7f,.73f,.08f+i%3*.09f));p.SetVector("_LabelColor",new Vector4(.4f,.2f,.6f,.23f));p.SetTexture("_Glyphs",Texture2D.whiteTexture);p.SetVector("_Glyphs_TexelSize",Vector4.one);}
        var previous=RenderTexture.active;var draw=type.GetMethod("DrawNativeMesh",Hidden);var parameters=draw.GetParameters();int cases=0,worst=0,regularCalls=0,instanceCalls=0;
        object Call(string method,params object[] args)=>type.GetMethod(method,Hidden).Invoke(surface,args);
        Color32[] Render(bool instances,float density,int scenario,int blend){
            type.GetField("DisableInstancingForValidation",Hidden).SetValue(surface,!instances);Call("SetViewport",320f,192f,density);Call("Begin",new Vector2(160,96));
            for(int i=0;i<properties.Length;i++){
                var args=new object[parameters.Length];for(int j=0;j<args.Length;j++)args[j]=parameters[j].DefaultValue;
                args[0]=scenario==3&&i%17==0?triangle:mesh;args[1]=scenario==3&&i%19==0?label:solid;args[2]=properties[i];
                args[3]=Matrix4x4.TRS(new Vector3(100+i%10*11.317f,50+i%7*12.193f),Quaternion.Euler(0,0,i*7.37f),new Vector3(.6f+i%3*.2f,1.3f,1));
                if(scenario==2&&i%17==0){args[4]=new Color(.2f,.4f,.8f,.4f);args[5]=6f;}
                args[7]=.35f+i%4*.15f;int mode=scenario==1?i/11%3:blend;args[9]=mode==2;args[10]=mode==1;draw.Invoke(surface,args);
            }
            Call("End");int calls=(int)type.GetProperty("GeometryDrawCalls",Hidden).GetValue(surface);if(instances)instanceCalls+=calls;else regularCalls+=calls;
            if(scenario==0&&instances&&(calls!=3||(int)type.GetProperty("InstancedPrimitiveCount",Hidden).GetValue(surface)!=140))throw new InvalidOperationException("Compatible runs did not become three 64-instance draws");
            var target=(RenderTexture)type.GetField("_target",Hidden).GetValue(surface);RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false,true);
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
        }
        try{
            Call("Initialize");
            for(int scenario=0;scenario<4;scenario++)for(int blend=0;blend<3;blend++)foreach(float density in new[]{1f,1.75f}){
                var expected=Render(false,density,scenario,blend);var actual=Render(true,density,scenario,blend);int peak=0;
                for(int i=0;i<actual.Length;i++){peak=Math.Max(peak,Math.Abs(actual[i].r-expected[i].r));peak=Math.Max(peak,Math.Abs(actual[i].g-expected[i].g));peak=Math.Max(peak,Math.Abs(actual[i].b-expected[i].b));peak=Math.Max(peak,Math.Abs(actual[i].a-expected[i].a));}
                worst=Math.Max(worst,peak);if(peak>2)throw new InvalidOperationException($"Instancing changed GPU pixels: scenario={scenario}, blend={blend}, density={density}, peak={peak}/255");cases++;
            }
            return new {cases,worstChannelError=worst,regularCalls,instanceCalls,stableOrder=true};
        }finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(triangle);UnityEngine.Object.DestroyImmediate(solid);UnityEngine.Object.DestroyImmediate(label);}
    }

    public static object TessellationBuffers()
    {
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeWorldShapes",true);
        var tessellate=type.GetMethod("Tessellate",BindingFlags.Static|BindingFlags.NonPublic);var version=type.GetMethod("GeometryVersion",BindingFlags.Static|BindingFlags.NonPublic);
        var polyline=type.GetMethod("PolylineContour",BindingFlags.Static|BindingFlags.NonPublic);Mesh retained=null;int cases=0;
        Mesh Build(object contour,float width,Mesh target,int frame){
            var parameters=tessellate.GetParameters();var args=new object[parameters.Length];for(int i=0;i<args.Length;i++)args[i]=parameters[i].DefaultValue;
            args[0]=contour;args[1]=width;args[2]=target;args[3]=frame%2==0;
            var corners=Enum.GetValues(typeof(Unity.VectorGraphics.PathCorner));args[4]=corners.GetValue(frame%corners.Length);
            args[7]=frame%5==0?new[]{(Unity.VectorGraphics.BezierContour)contour,(Unity.VectorGraphics.BezierContour)polyline.Invoke(null,new object[]{true,new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(0,1)}})}:null;
            args[8]=frame%3==0?new[]{3f,2f}:null;args[9]=frame*.13f;return (Mesh)tessellate.Invoke(null,args);
        }
        try{
            for(int frame=0;frame<48;frame++){
                float wave=Mathf.Sin(frame*.37f);var contour=polyline.Invoke(null,new object[]{frame%2==0,new[]{new Vector2(-12,-8),new Vector2(3+wave,-12),new Vector2(12,2),new Vector2(wave,5),new Vector2(-8,10)}});
                float width=frame%4==0?0:1.5f+frame%3*.5f;
                var fresh=Build(contour,width,null,frame);try{
                    retained=Build(contour,width,retained,frame);var a=fresh.vertices;var b=retained.vertices;var x=fresh.triangles;var y=retained.triangles;
                    if(a.Length!=b.Length||x.Length!=y.Length)throw new InvalidOperationException("Reused tessellation changed geometry size");
                    for(int i=0;i<a.Length;i++)if(!a[i].Equals(b[i]))throw new InvalidOperationException("Reused tessellation changed vertices");
                    for(int i=0;i<x.Length;i++)if(x[i]!=y[i])throw new InvalidOperationException("Reused tessellation changed triangle order");
                    ulong before=(ulong)version.Invoke(null,new object[]{retained});Build(contour,width,retained,frame);
                    if(before!=(ulong)version.Invoke(null,new object[]{retained}))throw new InvalidOperationException("Unchanged geometry invalidated a shadow mask");
                    var path=(Unity.VectorGraphics.BezierContour)contour;var point=path.Segments[0];point.P1+=new Vector2(.25f,0);path.Segments[0]=point;
                    Build(path,width,retained,frame);if(before==(ulong)version.Invoke(null,new object[]{retained}))throw new InvalidOperationException("In-place curve mutation reused a stale mask");cases++;
                }finally{UnityEngine.Object.DestroyImmediate(fresh);}
            }
            return new {cases,exactVertices=true,exactIndices=true,inPlaceInvalidation=true};
        }finally{if(retained!=null)UnityEngine.Object.DestroyImmediate(retained);}
    }

    public static object ShadowCacheBudget()
    {
        var type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeWorldShadowCache",true);var keyType=type.GetNestedType("Key",BindingFlags.NonPublic);
        var cache=Activator.CreateInstance(type,true);var begin=type.GetMethod("Begin",Hidden);var probe=type.GetMethod("Probe",Hidden);var mesh=new Mesh();int largeEntries=0,smallEntries=0;long peak=0;
        void Fill(int size){begin.Invoke(cache,null);for(int i=0;i<160;i++){
            var key=Activator.CreateInstance(keyType,Hidden,null,new object[]{mesh,1UL,Matrix4x4.Translate(new Vector3(i,0,0)),new Rect(0,0,320,192),new RectInt(i,0,size,size),1f,1f,8f},null);
            for(int repeat=0;repeat<2;repeat++)probe.Invoke(cache,new object[]{key,size,size,null});
            long bytes=(long)type.GetProperty("ResidentBytes",Hidden).GetValue(cache);int count=(int)type.GetProperty("Count",Hidden).GetValue(cache);peak=Math.Max(peak,bytes);
            if(bytes>16*1024*1024||count>128)throw new InvalidOperationException("Native shadow cache exceeded its resource limit");
        }}
        try{
            Fill(400);largeEntries=(int)type.GetProperty("Count",Hidden).GetValue(cache);
            if(largeEntries==0||largeEntries>=160)throw new InvalidOperationException("Byte budget was not exercised");
            begin.Invoke(cache,null);begin.Invoke(cache,null);if((int)type.GetProperty("Count",Hidden).GetValue(cache)!=0)throw new InvalidOperationException("Unused masks were not evicted under pressure");
            ((IDisposable)cache).Dispose();Fill(32);smallEntries=(int)type.GetProperty("Count",Hidden).GetValue(cache);if(smallEntries!=128)throw new InvalidOperationException("Entry-count budget was not exercised");
            var entries=(System.Collections.IDictionary)type.GetField("_entries",Hidden).GetValue(cache);
            foreach(System.Collections.DictionaryEntry pair in entries){((RenderTexture)pair.Value.GetType().GetField("Texture",Hidden).GetValue(pair.Value)).Release();break;}
            begin.Invoke(cache,null);if((int)type.GetProperty("Count",Hidden).GetValue(cache)!=127)throw new InvalidOperationException("Lost render target remained in the mask cache");
            UnityEngine.Object.DestroyImmediate(mesh);begin.Invoke(cache,null);if((int)type.GetProperty("Count",Hidden).GetValue(cache)!=0)throw new InvalidOperationException("Destroyed geometry retained GPU mask resources");
            return new {largeEntries,smallEntries,peakBytes=peak,unusedEviction=true,destroyedMeshEviction=true,lostTargetEviction=true};
        }finally{((IDisposable)cache).Dispose();if(mesh!=null)UnityEngine.Object.DestroyImmediate(mesh);}
    }
}

public sealed class EchoSphereSimulationPhaseProbe : MonoBehaviour,EchoSphere.Runtime.ISourceSimulationAgent
{
    public Action Tick;
    public void SimulationTick(float dt)=>Tick?.Invoke();
}
