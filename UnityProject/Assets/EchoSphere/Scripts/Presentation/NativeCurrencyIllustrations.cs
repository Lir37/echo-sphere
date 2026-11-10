using UnityEngine;using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 internal static class NativeCurrencyIllustrations {
  internal static bool Draw(Painter2D p,string id,Vector2 origin,float scale){
   if(id!="currency:Credits"&&id!="currency:Echo"&&id!="currency:ResonanceShards")return false;
   Vector2 V(float x,float y)=>origin+new Vector2(x,y)*scale;
   void Line(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.Stroke();}
   void Shape(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.ClosePath();p.Fill();p.Stroke();}
   p.lineWidth=1.65f*scale;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
   Color c=id=="currency:Credits"?new Color(1,.79f,.42f):id=="currency:Echo"?new Color(.40f,.90f,.96f):new Color(.77f,.64f,1);
   p.strokeColor=c;p.fillColor=new Color(c.r*.15f,c.g*.15f,c.b*.15f,.95f);
   if(id=="currency:Credits"){Shape(V(-11,-8),V(1,-12),V(11,-5),V(6,-1),V(11,5),V(-1,12),V(-11,8),V(-5,3),V(-11,-2));Line(V(-3,-6),V(4,-3),V(-2,2),V(4,5));}
   else if(id=="currency:Echo"){p.BeginPath();p.Arc(V(-3,0),10*scale,35,300);p.Stroke();p.BeginPath();p.Arc(V(4,0),8*scale,215,480);p.Stroke();Line(V(-7,0),V(-3,-3),V(2,4),V(6,-1));}
   else{Shape(V(-11,-9),V(-1,-6),V(-5,0),V(2,4),V(-1,12),V(-10,5));Shape(V(3,-12),V(11,-5),V(6,1),V(11,7),V(5,10),V(0,2),V(5,-4));}
   return true;
  }
 }
}