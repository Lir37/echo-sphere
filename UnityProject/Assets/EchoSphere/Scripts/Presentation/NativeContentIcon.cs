using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Retained vector art shared by rewards, inventory, merchant and archive.
 internal sealed class NativeContentIcon : VisualElement {
  internal string ContentId {get;private set;} private Color _color;
  internal NativeContentIcon(string id,Color color,float size=36){name="content-icon";pickingMode=PickingMode.Ignore;style.width=style.height=size;style.flexShrink=0;Bind(id,color);generateVisualContent+=Draw;}
  internal void Bind(string id,Color color){if(ContentId==id&&_color==color)return;ContentId=id??"";_color=color;MarkDirtyRepaint();}
  internal static uint Signature(string id){uint h=2166136261;foreach(char c in id??""){h^=c;h*=16777619;}return h;}
  private void Draw(MeshGenerationContext context){var p=context.painter2D;float size=Mathf.Min(contentRect.width,contentRect.height);if(size<=1)return;float scale=size/32;Vector2 origin=contentRect.center;
   Vector2 V(float x,float y)=>origin+new Vector2(x,y)*scale;
   void Line(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.Stroke();}
   void Ring(float x,float y,float r){p.BeginPath();p.Arc(V(x,y),r*scale,0,360);p.ClosePath();p.Stroke();}
   void Poly(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.ClosePath();p.Stroke();}
   void Gem(float x,float y,float r){Poly(V(x,y-r),V(x+r*.68f,y),V(x,y+r),V(x-r*.68f,y));Line(V(x,y-r),V(x,y+r));}
   string id=ContentId;int colon=id.LastIndexOf(':');string key=colon<0?id:id.Substring(colon+1);uint sig=Signature(id);p.strokeColor=_color;p.lineWidth=1.4f*scale;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
   if(id=="locked"){Poly(V(-7,-1),V(7,-1),V(7,10),V(-7,10));p.BeginPath();p.Arc(V(0,-2),5*scale,180,360);p.Stroke();Ring(0,4,1);}
   else if(key.Contains("heart")||key.Contains("hp")||key.Contains("blood")){Poly(V(0,10),V(-10,0),V(-9,-6),V(-4,-9),V(0,-5),V(4,-9),V(9,-6),V(10,0));Line(V(-7,0),V(-3,0),V(-1,-3),V(2,4),V(4,0),V(7,0));}
   else if(key.Contains("claw")||key.Contains("blade")||key.Contains("shell")){for(int i=-1;i<=1;i++)Poly(V(i*6-2,9),V(i*6-1,-3),V(i*6+4,-10),V(i*6+2,4));}
   else if(key.Contains("eye")||key.Contains("scope")||key.Contains("lens")){Poly(V(-12,0),V(-6,-6),V(5,-6),V(12,0),V(5,6),V(-6,6));Ring(0,0,4);Line(V(0,-11),V(0,-8));Line(V(0,8),V(0,11));}
   else if(key.Contains("clock")||key.Contains("time")||key.Contains("tempo")||key.Contains("stasis")){Ring(0,0,10);Line(V(0,-7),V(0,0),V(5,3));Line(V(-4,-13),V(4,-13));Line(V(-12,-8),V(-9,-10));}
   else if(key.Contains("crown")){Poly(V(-10,-7),V(-5,-2),V(0,-10),V(5,-2),V(10,-7),V(8,7),V(-8,7));Line(V(-7,10),V(7,10));Gem(0,1,2.5f);}
   else if(key.Contains("boot")||key.Contains("speed")||key.Contains("kinetic")){Poly(V(-4,-10),V(4,-10),V(3,2),V(10,6),V(9,10),V(-5,10));Line(V(-11,-4),V(-7,-4));Line(V(-13,1),V(-8,1));}
   else if(key.Contains("mirror")||key.Contains("glass")||key.Contains("prism")){Poly(V(-7,-9),V(4,-11),V(9,-4),V(7,9),V(-5,11),V(-10,2));Line(V(-6,5),V(4,-5));Line(V(-2,7),V(6,-1));}
   else if(key.Contains("coil")||key.Contains("chain")||key.Contains("conductor")||key.Contains("overclock")||key.Contains("battery")){Line(V(2,-12),V(-7,1),V(0,1),V(-2,12),V(8,-2),V(1,-2),V(2,-12));Line(V(-11,-7),V(-7,-7));Line(V(7,7),V(11,7));}
   else if(key.Contains("network")||key.Contains("relay")||key.Contains("lattice")||key.Contains("triangle")||key.Contains("formation")||key.Contains("geometry")){Poly(V(0,-10),V(10,7),V(-10,7));Line(V(0,-10),V(0,2),V(10,7));Line(V(0,2),V(-10,7));Ring(0,-10,2);Ring(-10,7,2);Ring(10,7,2);}
   else if(key.Contains("ring")||key.Contains("orbit")||key.Contains("charm")){Ring(0,0,8);Ring(0,0,11);Gem(0,-9,3);}
   else if(key.Contains("gravity")||key.Contains("singularity")||key.Contains("void")||key.Contains("zero")){Ring(0,0,5);p.BeginPath();p.Arc(origin,10*scale,25,290);p.Stroke();Line(V(-11,4),V(-8,10),V(-3,9));Line(V(11,-4),V(8,-10),V(3,-9));}
   else if(key.Contains("aura")||key.Contains("pulse")||key.Contains("mist")){Ring(0,0,3);for(int i=0;i<4;i++){p.BeginPath();p.Arc(origin,8*scale,i*90+12,i*90+72);p.Stroke();}Line(V(0,-13),V(0,-11));Line(V(0,11),V(0,13));}
   else if(key.Contains("leaf")||key.Contains("seed")||key.Contains("living")){Poly(V(-8,8),V(-8,-2),V(1,-10),V(10,-10),V(10,-1),V(2,8));Line(V(-10,10),V(6,-6));Line(V(-3,3),V(-4,-3));}
   else if(id.StartsWith("monster:")||id.StartsWith("boss:")){Poly(V(0,-10),V(9,-5),V(11,5),V(4,10),V(-4,10),V(-11,5),V(-9,-5));Line(V(-7,-1),V(-3,1));Line(V(7,-1),V(3,1));Line(V(-3,6),V(3,6));}
   else if(key.Contains("pendant")||key.Contains("talisman")||key.Contains("ward")||key.Contains("amulet")){Line(V(-8,-12),V(-4,-4),V(4,-4),V(8,-12));Gem(0,3,8);}
   else {Gem(0,0,10);Ring(0,0,3);Line(V(-12,5),V(-9,8));Line(V(9,-8),V(12,-5));}
   if(id!="locked"){p.lineWidth=.8f*scale;var accent=_color;accent.a=.55f;p.strokeColor=accent;for(int i=0;i<6;i++){float x=-9+i*3.3f;float y=11+(int)((sig>>(i*2))&3)*.65f;Line(V(x,y),V(x+1.4f,y));}}
  }
 }
}
