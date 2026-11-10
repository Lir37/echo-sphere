using System;using System.Collections.Generic;using UnityEngine;using Unity.VectorGraphics;using static EchoSphere.Presentation.NativeWorldShapes;
namespace EchoSphere.Presentation { internal sealed partial class NativeWorldNetwork {
 private struct GeometryEdge {internal Color Color;internal bool Active;}
 private readonly Dictionary<(int,int),GeometryEdge> _geometryEdges=new Dictionary<(int,int),GeometryEdge>();
 private readonly List<int> _formationOrder=new List<int>(8);
 private Mesh _cometHead,_cometHalo;
 private static bool Inside(Formation f,int a,int b)=>f!=null&&Has(f,a)&&Has(f,b);
 private static bool FormedPair(Network n,int a,int b)=>Inside(n.Line,a,b)||Inside(n.Triangle,a,b)||Inside(n.Square,a,b)||Inside(n.Cluster,a,b)||Inside(n.Ring,a,b)||Inside(n.Lattice,a,b)||Inside(n.Fractal,a,b);
 private static Color FormationColor(int kind){
  Color c=kind==0?C(99,230,255):kind==1?C(255,184,77):kind==2?C(105,183,255):kind==3?C(179,140,255):kind==4?C(85,230,154):kind==5?C(57,216,255):C(255,107,157);
  float luminance=.2126f*c.r+.7152f*c.g+.0722f*c.b;
  return luminance>.70f?new Color(c.r*.70f/luminance,c.g*.70f/luminance,c.b*.70f/luminance):Color.Lerp(c,Color.white,(.70f-luminance)/(1-luminance));
 }
 private void CollectFormation(Network n,Formation f,int kind){
  if(f?.Nodes==null||f.Nodes.Length<2)return;_formationOrder.Clear();foreach(int i in f.Nodes)if(i>=0&&i<n.Spheres.Length)_formationOrder.Add(i);if(_formationOrder.Count<2)return;
  var center=Center(f,n.Spheres);double Angle(int index)=>Math.Atan2(n.Spheres[index].Y-center.Y,n.Spheres[index].X-center.X);
  double minX=double.MaxValue,maxX=double.MinValue,minY=double.MaxValue,maxY=double.MinValue;foreach(int index in _formationOrder){var p=n.Spheres[index];minX=Math.Min(minX,p.X);maxX=Math.Max(maxX,p.X);minY=Math.Min(minY,p.Y);maxY=Math.Max(maxY,p.Y);}bool vertical=kind==0&&maxY-minY>maxX-minX;
  // Insertion sort avoids a per-frame comparison delegate and temporary node array.
  for(int i=1;i<_formationOrder.Count;i++){int value=_formationOrder[i],j=i-1;double key=kind==0?n.Spheres[value].X:Angle(value);
   if(vertical)key=n.Spheres[value].Y;
   while(j>=0&&(kind==0?(vertical?n.Spheres[_formationOrder[j]].Y:n.Spheres[_formationOrder[j]].X):Angle(_formationOrder[j]))>key){_formationOrder[j+1]=_formationOrder[j];j--;}_formationOrder[j+1]=value;
  }
  void Add(int a,int b){if(a==b)return;var key=a<b?(a,b):(b,a);if(!_geometryEdges.TryGetValue(key,out var old)||f.Active&&!old.Active)_geometryEdges[key]=new GeometryEdge{Color=FormationColor(kind),Active=f.Active};}
  if(kind>=5){foreach(var link in n.Links)if(Has(f,link.A)&&Has(f,link.B))Add(link.A,link.B);}
  else{for(int i=1;i<_formationOrder.Count;i++)Add(_formationOrder[i-1],_formationOrder[i]);if(kind!=0)Add(_formationOrder[_formationOrder.Count-1],_formationOrder[0]);}
 }
 private void DrawComet(Node head,double angle,Color color,double alpha){
  if(_cometHead==null){
   var path=new BezierContour{Closed=true,Segments=new[]{
    new BezierPathSegment{P0=P(3.2,0),P1=P(3.2,-2.4),P2=P(-1.5,-3.0)},
    new BezierPathSegment{P0=P(-4.5,-.3),P1=P(-7.0,-.1),P2=P(-10,0)},
    new BezierPathSegment{P0=P(-13,0),P1=P(-8,0),P2=P(-6,.1)},
    new BezierPathSegment{P0=P(-4.5,.3),P1=P(-1.5,3),P2=P(3.2,2.4)}}};
   _cometHead=Tessellate(path,0);_cometHalo=NativeWorldShapes.Circle(Vector2.zero,1);
  }
  void Part(Mesh mesh,Matrix4x4 transform,Color tint,float opacity){if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_propertyUsed++];p.Clear();p.SetColor("_NativeColor",tint);_surface.DrawNativeMesh(mesh,_material,p,transform,opacity:opacity,inheritShadow:false,shadow:Color.clear);}
  var root=T(head.X,head.Y,angle:angle);Part(_cometHalo,root*Matrix4x4.Scale(new Vector3(4.5f,3.2f,1)),color,(float)(alpha*.10));Part(_cometHead,root,color,(float)(alpha*.9));Part(_cometHalo,root*Matrix4x4.Scale(new Vector3(1.6f,1.3f,1)),C(238,255,255),(float)(alpha*.85));
 }
 private void TriangleCharges(Node[] nodes,Formation f){var color=FormationColor(1);foreach(int i in f.Nodes){var s=nodes[i];double charge=s.Charge%3;if(charge<=0&&s.Pulse<=0)continue;double radius=25;Circle(s.X,s.Y,radius,1.3,color,.48,Matrix4x4.identity,start:-Math.PI/2,end:-Math.PI/2+Math.PI*2*charge/3);}}
}}
