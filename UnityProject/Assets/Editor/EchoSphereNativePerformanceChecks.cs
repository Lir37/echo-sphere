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
        var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var documents=new System.Collections.Generic.List<object>();
        foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None)){
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
        foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None)){
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
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{UnityEngine.Object.DestroyImmediate(image);}
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
            try{image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}finally{UnityEngine.Object.DestroyImmediate(image);}
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
}

public sealed class EchoSphereSimulationPhaseProbe : MonoBehaviour,EchoSphere.Runtime.ISourceSimulationAgent
{
    public Action Tick;
    public void SimulationTick(float dt)=>Tick?.Invoke();
}
