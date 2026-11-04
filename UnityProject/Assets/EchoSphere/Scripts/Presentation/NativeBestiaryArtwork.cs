using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Presentation {
 // Bake visible portraits from the production anatomy, never from UI paint callbacks.
 internal sealed class NativeBestiaryArtwork : IDisposable {
  private readonly Dictionary<string,RenderTexture> _cache=new Dictionary<string,RenderTexture>();
  private readonly Dictionary<string,BestiaryAppearance> _sources=new Dictionary<string,BestiaryAppearance>();
  private GameObject _owner;private NativeWorldSurface _surface;private NativeWorldCreatures _creatures;private int _built;
  internal int Count=>_cache.Count;
  internal int BakeCount {get;private set;}
  internal void BeginFrame()=>_built=0;
  internal Texture Resolve(BestiaryAppearance a){
   if(a==null)return null;
   if(_cache.TryGetValue(a.Id,out var image)&&image.IsCreated()&&_sources.TryGetValue(a.Id,out var old)&&old.Region==a.Region&&old.Role==a.Role&&old.Variant==a.Variant&&old.ColorHex==a.ColorHex&&old.Boss==a.Boss&&old.Elite==a.Elite)return image;
   if(_built++>0)return null;
   if(_owner==null){_owner=new GameObject("Archive anatomy portrait baker"){hideFlags=HideFlags.HideAndDontSave};_surface=_owner.AddComponent<NativeWorldSurface>();_surface.Initialize();_surface.SetViewport(192,192,1);_creatures=new NativeWorldCreatures();}
   ColorUtility.TryParseHtmlString(a.ColorHex??"#4a7a8a",out var color);
   var view=new NativeWorldCreatures.EnemyView{Identity=a,Radius=a.Boss?32:44,HasRotation=true,Rotation=-.25,Boss=a.Boss,Elite=a.Elite,Variant=a.Variant,Role=a.Role,RegionStyle=a.Region,Fleet=a.Region==1,Color=color,Portrait=true,Hp=1,MaxHp=1,BossName=""};
   _surface.Begin(Vector2.zero);_surface.ResetTransform();_creatures.BeginFrame();_creatures.DrawEnemy(_surface,view,1.7);_surface.End();
   if(image==null){image=new RenderTexture(192,192,0,RenderTextureFormat.ARGB32){name="Archive "+a.Id,hideFlags=HideFlags.DontSave};_cache[a.Id]=image;}if(!image.IsCreated())image.Create();Graphics.Blit(_surface.UiTexture,image);_sources[a.Id]=new BestiaryAppearance{Id=a.Id,Region=a.Region,Role=a.Role,Variant=a.Variant,ColorHex=a.ColorHex,Boss=a.Boss,Elite=a.Elite};BakeCount++;return image;
  }
  public void Dispose(){foreach(var image in _cache.Values)if(image!=null)UnityEngine.Object.Destroy(image);_cache.Clear();_sources.Clear();_creatures?.Dispose();_creatures=null;if(_owner!=null)UnityEngine.Object.Destroy(_owner);_owner=null;_surface=null;}
 }
}
