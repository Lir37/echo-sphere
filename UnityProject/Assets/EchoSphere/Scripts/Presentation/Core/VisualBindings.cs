#if UNITY_EDITOR
using System;

namespace EchoSphere.Presentation
{
    public static class VisualBindings
    {
        public static string Locale="en";
        // Gameplay calculations remain in the authoritative Unity Core. The renderer
        // receives their already computed snapshots, just as renderer.ts receives a
        // network frame and the current sphere modifiers from the source engine.
        public static V Get(V module,V name)
        {
            switch(name.Text)
            {
                case "getLocale":return VisualJs.Fn(args=>Locale);
                case "getNetworkFrame":return VisualJs.Fn(args=>args[0]["nativeNetwork"]);
                case "getRenderViewport":return VisualJs.Fn(args=>args[0]["nativeViewport"]);
                case "sphereModifiers":return VisualJs.Fn(args=>args[0]["nativeSphereModifiers"][args[1]]);
                case "getCharacterFormation":return VisualJs.Fn(args=> {
                    var dominant=args[0]["nativeNetwork"]["dominantFormation"];
                    var type=dominant["type"].Text;
                    return args[0]["player"]["characterId"].Text=="architect"&&dominant.Truth&&(type=="line"||type=="triangle"||type=="square"||type=="cluster")
                        ?VisualJs.Obj("type",dominant["type"],"strength",dominant["strength"]):VisualJs.Obj("type","none","strength",0);
                });
                default:throw new InvalidOperationException("Unbound native rendering dependency: "+module.Text+"/"+name.Text);
            }
        }
    }
}
#endif
