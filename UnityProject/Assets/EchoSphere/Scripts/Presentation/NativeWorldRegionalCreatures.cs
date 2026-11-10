using System;using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private readonly System.Collections.Generic.Dictionary<(double,double,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art> _regionalEllipses=new System.Collections.Generic.Dictionary<(double,double,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art>();
  private static Color RegionalInk(Color color,double strength)=>new Color(color.r*(float)strength+.012f,color.g*(float)strength+.012f,color.b*(float)strength+.018f,1);
  private Art BuildRegionalEllipse(double rx,double ry,double width)=>Get(0,rx,ry,width,()=>Oval(rx,ry));
  private void RegionalEllipse(Matrix4x4 m,double rx,double ry,Color? fill,Color? stroke=null,double width=0){var key=(rx,ry,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_regionalEllipses.TryGetValue(key,out var art)){art=BuildRegionalEllipse(rx,ry,width);_regionalEllipses.Add(key,art);}Paint(art,m,fill,stroke);}
  private void ClearRegionalArt(){_regionalEllipses.Clear();}
 }
}
