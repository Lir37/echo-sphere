namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private void DrawNativeViewVoidAmbient(double width,double height,double time)=>_nativeWorldAmbient.Draw(_worldSurface,(float)width,(float)height,time);
        private void DrawNativeViewVoidField(double width,double height,double x,double y,double playerX,double playerY)=>_nativeWorldField.Draw(_worldSurface,(float)width,(float)height,(float)x,(float)y,(float)playerX,(float)playerY);
        private void DrawNativeViewRegionLayer()=>DrawNativeRegionWorld();
        private void DrawNativeViewFormationMemory()=>DrawNativeFormationMemory();
        private void DrawNativeViewSphereNetwork()=>DrawNativeNetwork();
        private void DrawNativeViewGroundShadow(double width,double height,double unused)=>_nativeWorldCreatures.DrawGroundShadow(_worldSurface,width,height);
    }
}
