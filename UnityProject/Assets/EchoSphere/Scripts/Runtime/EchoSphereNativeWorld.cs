using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldField _nativeWorldField;
        private NativeWorldAmbient _nativeWorldAmbient;
        private void RenderNativeWorldFrame(float width,float height)
        {
            if(_nativeWorldField==null)_nativeWorldField=new NativeWorldField();
            if(_nativeWorldAmbient==null)_nativeWorldAmbient=new NativeWorldAmbient();
            var sourceField=Visual_renderer.Scope["drawVoidField"];
            var sourceAmbient=Visual_renderer.Scope["drawVoidAmbient"];
            var sourceRegion=Visual_renderer.Scope["drawRegionLayer"];
            try{
                Visual_renderer.Scope["drawVoidField"]=VisualJs.Fn(args=>{_nativeWorldField.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[4].Number,(float)args[5].Number,(float)args[6].Number,(float)args[7].Number);return default;});
                Visual_renderer.Scope["drawVoidAmbient"]=VisualJs.Fn(args=>{_nativeWorldAmbient.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,args[3].Number);return default;});
                Visual_renderer.Scope["drawRegionLayer"]=VisualJs.Fn(args=>{DrawNativeRegionWorld(args[1]);return default;});
                Visual_renderer.Scope["render"].Call(_sourceCanvas.Canvas.Value,_sourceRenderState,width,height);
            }finally{Visual_renderer.Scope["drawVoidField"]=sourceField;Visual_renderer.Scope["drawVoidAmbient"]=sourceAmbient;Visual_renderer.Scope["drawRegionLayer"]=sourceRegion;}
        }
        private void DisposeNativeWorld(){_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;}
    }
}
