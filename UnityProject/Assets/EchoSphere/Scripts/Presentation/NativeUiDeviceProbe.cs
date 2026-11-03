#if UNITY_INCLUDE_INSTRUMENTATION && UNITY_ANDROID
using System;
using EchoSphere;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Explicit file-triggered device probe. Excluded from ordinary release players.
 internal sealed class NativeUiDeviceProbe:MonoBehaviour {
  float _next;string DirectoryPath=>Path.Combine(Application.persistentDataPath,"ui-probe");
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void StartProbe(){if(Application.isEditor||!Debug.isDebugBuild)return;var g=new GameObject("Explicit UI device probe");DontDestroyOnLoad(g);g.AddComponent<NativeUiDeviceProbe>();}
  void Update(){if(Time.realtimeSinceStartup<_next)return;_next=Time.realtimeSinceStartup+1;var request=Path.Combine(DirectoryPath,"request.txt");if(!File.Exists(request))return;try{string command=File.ReadAllText(request).Trim();File.Delete(request);Run(command);}catch(Exception e){Debug.LogException(e);}}
  void Run(string command){var lines=new List<string>();var seen=new HashSet<Texture>();int index=0;
   void Visit(VisualElement e,string path){path+="/"+e.name;
    if(command=="filters-off"){e.style.filter=new List<FilterFunction>();e.style.backdropFilter=new List<FilterFunction>();}
    if(command=="clip-off")e.style.overflow=Overflow.Visible;
    if(e is Image image&&image.image!=null){var texture=image.image;lines.Add(path+" | "+e.worldBound+" | "+texture.name+" #"+texture.ExistingObjectId()+" "+texture.width+"x"+texture.height);
     if(command=="rebind"){image.image=null;image.image=texture;image.MarkDirtyRepaint();}
     if(command=="capture"&&texture is RenderTexture rt&&rt.IsCreated()&&seen.Add(texture)&&(path.Contains("hud-right")||path.Contains("hud-hp-bar-fill")||path.Contains("pause-shell"))){var previous=RenderTexture.active;Texture2D pixels=null;try{RenderTexture.active=rt;pixels=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);pixels.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(DirectoryPath,"texture-"+(index++)+".png"),pixels.EncodeToPNG());lines.Add("CENTER "+pixels.GetPixel(rt.width/2,rt.height/2));}finally{RenderTexture.active=previous;if(pixels!=null)Destroy(pixels);}}
    }for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i],path);
   }
   foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>())if(document.isActiveAndEnabled)Visit(document.rootVisualElement,document.name);
   File.WriteAllLines(Path.Combine(DirectoryPath,command+".txt"),lines);Debug.Log("[EchoSphere.UiProbe] "+command+" images="+lines.Count);
  }
 }
}
#endif
