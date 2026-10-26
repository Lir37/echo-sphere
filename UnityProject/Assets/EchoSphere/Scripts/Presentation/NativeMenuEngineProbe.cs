#if UNITY_EDITOR
using System;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation
{
    public sealed class NativeMenuEngineProbe:MonoBehaviour
    {
        public static NativeMenuEngineProbe Current;
        public readonly RenderTexture[] Outputs=new RenderTexture[2];
        private readonly NativeMainMenuScreen[] _screens=new NativeMainMenuScreen[2];
        private NativeMainMenuView.State _state;private Color _color;private float _density;
        public int Frames {get;private set;}
        public static void StartProbe(EchoSphereRuntime runtime,float density)
        {
            if(Current!=null)throw new InvalidOperationException("Dispose previous menu probe");
            Current=new GameObject("Unity native main menu comparison").AddComponent<NativeMenuEngineProbe>();
            Current.Build(runtime,density);
        }
        private void Build(EchoSphereRuntime runtime,float density)
        {
            _density=density;_state=runtime.ReadNativeMainMenuState();
            var field=runtime.GetType().GetField("_sphereCharacterId",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var id=(SphereCharacterId)field.GetValue(runtime);ColorUtility.TryParseHtmlString(CharacterRules.Get(id).ColorHex,out _color);
            try {for(int i=0;i<2;i++){
                NativeBackdropValidation.UseLegacy=i==0;
                var owner=new GameObject(i==0?"Menu legacy reference":"Menu native candidate");owner.transform.SetParent(transform,false);
                var screen=_screens[i]=owner.AddComponent<NativeMainMenuScreen>();screen.Initialize(runtime.CreateNativeMainMenuView);
                var panel=owner.GetComponent<UIDocument>().panelSettings;
                Outputs[i]=new RenderTexture(Mathf.RoundToInt(640*density),Mathf.RoundToInt(960*density),24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name=owner.name};Outputs[i].Create();panel.targetTexture=Outputs[i];panel.clearColor=true;panel.colorClearValue=Color.clear;
                screen.Composition.SetCharacter(CharacterRules.ToSourceId(id));
            }} finally {NativeBackdropValidation.UseLegacy=false;}
        }
        private void LateUpdate(){foreach(var screen in _screens)screen.Show(_state,_density,_color);Frames++;}
        private void OnDestroy(){for(int i=0;i<2;i++)if(_screens[i]!=null){var doc=_screens[i].GetComponent<UIDocument>();if(doc!=null&&doc.panelSettings!=null)doc.panelSettings.targetTexture=null;}foreach(var target in Outputs)if(target!=null){target.Release();Destroy(target);}Current=null;}
    }
}
#endif
