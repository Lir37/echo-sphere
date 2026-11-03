using System;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private readonly System.Collections.Generic.Dictionary<(int,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art[]> _regionalGeometry=new System.Collections.Generic.Dictionary<(int,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art[]>();
  private readonly System.Collections.Generic.Dictionary<(double,double,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art> _regionalEllipses=new System.Collections.Generic.Dictionary<(double,double,double,Unity.VectorGraphics.PathEnding,Unity.VectorGraphics.PathCorner,float),Art>();
  private Art[] RegionalGeometry(int style,double r){var key=(style,r,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_regionalGeometry.TryGetValue(key,out var parts)){parts=BuildRegionalGeometry(style,r);_regionalGeometry.Add(key,parts);}return parts;}
  private Art[] BuildRegionalGeometry(int style,double r){double w=Math.Max(.8,r*.055);var parts=new Art[24];
   Art Shape(int id,Func<Unity.VectorGraphics.BezierContour> make,double width=0)=>Get(20000+style*1000+id,r,0,width,make);
   if(style==2){
    parts[1]=Shape(1,()=>Poly(P(.08*r,-.08*r),P(-.28*r,-.02*r),P(-.70*r,.22*r),P(-.62*r,.31*r),P(-.26*r,.12*r),P(.05*r,.10*r)),w*.7);
    parts[2]=Shape(2,()=>Quadratic(P(.04*r,-.05*r),true,P(-.20*r,.02*r),P(-.36*r,.33*r),P(-.12*r,.24*r),P(-.05*r,.10*r),P(.07*r,.07*r),P(.04*r,-.05*r)),w*.7);
    parts[0]=Shape(0,()=>Quadratic(P(.68*r,0),true,P(.45*r,-.54*r),P(-.14*r,-.59*r),P(-.88*r,-.56*r),P(-1.01*r,0),P(-.88*r,.56*r),P(-.14*r,.59*r),P(.45*r,.54*r),P(.68*r,0)),w);
    parts[3]=Shape(3,()=>Straight(P(-.68*r,0),P(.45*r,0)),w*.8);
    parts[4]=Shape(4,()=>Quadratic(P(0,-.08*r),true,P(.34*r,-.23*r),P(.63*r,-.10*r),P(.84*r,.05*r),P(.59*r,.23*r),P(.70*r,.02*r),P(.42*r,.02*r),P(.16*r,.10*r),P(0,-.08*r)),w*.7);
   }
   if(style==3){
    parts[0]=Shape(0,()=>Poly(P(.88*r,-.04*r),P(.30*r,-.23*r),P(.12*r,-.15*r),P(-.32*r,-.10*r),P(-.43*r,.10*r),P(.16*r,.21*r),P(.53*r,.12*r)),w);
    parts[1]=Shape(1,()=>Straight(P(-.2*r,0),P(.65*r,0)),w*.65);
    parts[2]=Shape(2,()=>Poly(P(.65*r,0),P(.37*r,-.29*r),P(-.09*r,-.49*r),P(-.52*r,-.19*r),P(-.61*r,.18*r),P(-.13*r,.46*r),P(.33*r,.30*r)),w);
    parts[3]=Shape(3,()=>Poly(P(.37*r,0),P(0,-.32*r),P(0,.32*r)));
   }
   if(style==4){
    parts[0]=Shape(0,()=>Quadratic(P(.6*r,0),false,P(.2*r,-.85*r),P(-.9*r,-.4*r),P(-1.3*r,.25*r),P(-.6*r,.6*r),P(.3*r,.65*r),P(.2*r,-.1*r)),w*1.2);
    parts[1]=Shape(1,()=>Oval(r*.33,r*.5,Math.PI*1.6),w);
   }
   if(style==5){
    parts[0]=Shape(0,()=>Quadratic(P(.83*r,0),true,P(.52*r,-.25*r),P(.39*r,-.62*r),P(-.08*r,-.47*r),P(-.49*r,-.79*r),P(-.29*r,-.28*r),P(-.84*r,-.10*r),P(-.48*r,.14*r),P(-.51*r,.69*r),P(-.08*r,.41*r),P(.28*r,.60*r),P(.51*r,.22*r),P(.83*r,0)),w);
    parts[1]=Shape(1,()=>Poly(P(0,-.07*r),P(-.75*r,-.24*r),P(-1.15*r,.1*r),P(-.45*r,.03*r)),w*.8);
    parts[2]=Shape(2,()=>Poly(P(.15*r,0),P(-.1*r,-.15*r),P(.03*r,-.05*r),P(-.15*r,.2*r)),w*.5);
   }
   if(style==2){
    parts[16]=Shape(16,()=>Quadratic(P(.42*r,0),true,P(.18*r,-.27*r),P(-.30*r,-.40*r),P(-.88*r,-.47*r),P(-.92*r,-.06*r),P(-.52*r,.04*r),P(.42*r,0)),w*.7);
    parts[17]=Shape(17,()=>Quadratic(P(.72*r,0),true,P(.57*r,-.34*r),P(.22*r,-.31*r),P(.02*r,-.17*r),P(.09*r,0),P(.04*r,.23*r),P(.28*r,.32*r),P(.62*r,.28*r),P(.72*r,0)),w*.75);
    parts[18]=Shape(18,()=>Quadratic(P(-.70*r,-.20*r),false,P(-.39*r,-.38*r),P(.08*r,-.16*r)),w*.55);
    parts[19]=Shape(19,()=>Poly(P(.14*r,0),P(-.08*r,-.11*r),P(-.22*r,0),P(-.08*r,.11*r)),w*.55);
    parts[20]=Shape(20,()=>Quadratic(P(-.08*r,0),true,P(.04*r,-.10*r),P(.17*r,-.07*r),P(.24*r,0),P(.17*r,.07*r),P(.04*r,.10*r),P(-.08*r,0)),w*.5);
   }else if(style==3){
    parts[16]=Shape(16,()=>Poly(P(.53*r,0),P(.09*r,-.29*r),P(-.39*r,-.16*r),P(-.15*r,.13*r),P(.23*r,.22*r)),w*.55);
    parts[17]=Shape(17,()=>Poly(P(-.15*r,0),P(.11*r,-.24*r),P(.38*r,-.11*r),P(.72*r,-.33*r),P(.52*r,.07*r),P(.16*r,.13*r)),w*.65);
    parts[18]=Shape(18,()=>Poly(P(.22*r,0),P(-.05*r,-.05*r),P(-.22*r,0),P(-.05*r,.06*r)));
    parts[19]=Shape(19,()=>Open(P(-.25*r,-.04*r),P(-.10*r,.09*r),P(.05*r,-.03*r),P(.24*r,.06*r)),w*.5);
    parts[20]=Shape(20,()=>Poly(P(.30*r,-.07*r),P(.08*r,-.13*r),P(-.07*r,-.03*r),P(.16*r,.02*r)),w*.5);
   }else if(style==4){
    parts[16]=Shape(16,()=>Quadratic(P(.83*r,0),true,P(.29*r,-.25*r),P(.27*r,-.51*r),P(-.23*r,-.23*r),P(-.50*r,-.39*r),P(-.31*r,.03*r),P(-.55*r,.41*r),P(.05*r,.16*r),P(.24*r,.51*r),P(.36*r,.14*r),P(.83*r,0)),w*.75);
    parts[17]=Shape(17,()=>Quadratic(P(.22*r,0),true,P(.12*r,-.24*r),P(-.30*r,-.23*r),P(-.94*r,-.56*r),P(-.99*r,-.02*r),P(-.57*r,-.22*r),P(-.24*r,.07*r),P(.08*r,.16*r),P(.22*r,0)),w*.6);
    parts[18]=Shape(18,()=>Quadratic(P(-.20*r,-.10*r),false,P(.05*r,-.20*r),P(.39*r,-.04*r)),w*.6);
    parts[19]=Shape(19,()=>Poly(P(.19*r,0),P(0,-.14*r),P(-.13*r,0),P(0,.14*r)),w*.5);
    parts[20]=Shape(20,()=>Quadratic(P(-.22*r,0),true,P(0,-.15*r),P(.36*r,0),P(0,.13*r),P(-.22*r,0)),w*.6);
   }else{
    parts[16]=Shape(16,()=>Quadratic(P(.45*r,-.07*r),true,P(.26*r,-.30*r),P(-.21*r,-.35*r),P(-.59*r,-.62*r),P(-.36*r,-.09*r),P(-.15*r,-.18*r),P(.45*r,-.07*r)),w*.65);
    parts[17]=Shape(17,()=>Quadratic(P(.42*r,0),true,P(.18*r,-.29*r),P(-.04*r,-.28*r),P(-.36*r,-.05*r),P(-.18*r,.21*r),P(.16*r,.20*r),P(.42*r,0)),w*.8);
    parts[18]=Shape(18,()=>Quadratic(P(0,0),false,P(.18*r,-.12*r),P(.38*r,-.07*r)),w*.65);
    parts[19]=Shape(19,()=>Poly(P(.06*r,-.13*r),P(.16*r,0),P(.06*r,.05*r),P(-.02*r,-.03*r)));
    parts[20]=Shape(20,()=>Quadratic(P(-.13*r,0),true,P(.03*r,-.11*r),P(.28*r,-.06*r),P(.17*r,.10*r),P(-.13*r,0)),w*.45);
   }
   // Rigid detail meshes are built once per style/radius, never per pose.
   parts[12]=Shape(12,()=>Straight(P(0,0),P(.32*r,-.12*r)),w*.65);
   parts[13]=Shape(13,()=>Poly(P(0,-.05*r),P(.28*r,-.11*r),P(.42*r,0),P(.28*r,.11*r),P(0,.05*r)),w*.65);
   parts[14]=Shape(14,()=>Poly(P(.19*r,0),P(0,-.12*r),P(-.19*r,0),P(0,.12*r)),w*.6);
   parts[15]=Shape(15,()=>Straight(P(-.15*r,0),P(.15*r,0)),w*.6);
   parts[10]=Shape(10,()=>Poly(P(.4*r,0),P(0,-.14*r),P(-.5*r,0),P(0,.14*r)),w);
   parts[11]=Shape(11,()=>Poly(P(.4*r,-.08*r),P(1.15*r,0),P(.4*r,.08*r)),w*.7);
   return parts;
  }
  private static Color RegionalInk(Color color,double strength)=>new Color(color.r*(float)strength+.012f,color.g*(float)strength+.012f,color.b*(float)strength+.018f,1);
  private Art BuildRegionalEllipse(double rx,double ry,double width)=>Get(0,rx,ry,width,()=>Oval(rx,ry));
  private void RegionalEllipse(Matrix4x4 m,double rx,double ry,Color? fill,Color? stroke=null,double width=0){var key=(rx,ry,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_regionalEllipses.TryGetValue(key,out var art)){art=BuildRegionalEllipse(rx,ry,width);_regionalEllipses.Add(key,art);}Paint(art,m,fill,stroke);}
  private void ClearRegionalArt(){_regionalGeometry.Clear();_regionalEllipses.Clear();}
  // Retained flat anatomy: articulated insects, crystal clusters, energy filaments,
  // and asymmetric Void hybrids. Geometry is independent of animation time.
  private void DrawRegionalCreature(EnemyView e,double time,Matrix4x4 root){
   double r=e.Radius,w=Math.Max(.8,r*.055),attack=Clamp(e.AttackPose);int style=e.RegionStyle;
   string role=e.Boss?e.Variant:e.Role;
   int kind=role=="sniper"||role=="ranged"||role=="shooter"?1:role=="tank_guard"||role=="anchor"||role=="architect"?2:role=="healer"||role=="aura"||role=="conductor"?3:role=="splitter"||role=="summoner"?4:role=="phase"||role=="null"?5:role=="bomber"||role=="stella_warden"?6:0;
   var color=e.Freeze?C(105,214,255):style==2?C(174,210,108):style==3?C(123,219,245):style==4?C(255,207,100):C(192,123,230);var dark=RegionalInk(color,.14);var shellColor=RegionalInk(color,.42);var pale=Color.Lerp(color,Color.white,.48f);
   if(DrawRegionalRoleBody(e,time,root,color))return;
   var parts=RegionalGeometry(style,r);
   if(style==2){
    var leg=parts[1];
    var foot=parts[2];
    for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){
     double swing=Math.Sin(time*8+i*2.1+side*1.57)*.32*_gaitBlend;
     var joint=root*T((i-1)*r*.45,side*r*.4,side*(.3+i*.22)+swing,sx:1,sy:side);
     Paint(leg,joint,shellColor,color);Paint(foot,joint*T(-.65*r,.25*r,angle:-swing*1.4),shellColor,A(color,.8));
    }
    var shell=parts[0];
    Paint(shell,root,dark,color);
    var headPlate=root*T(.19*r,0,sx:kind==1?1.15:1,sy:kind==2?1.12:.88);
    Paint(parts[17],headPlate,shellColor,pale);
    for(int side=-1;side<=1;side+=2){
     var cover=root*T(-.02*r,side*.03*r,side*(.03+attack*.11),sx:kind==2?1.06:1,sy:side*(kind==2?1.12:1));
     Paint(parts[16],cover,shellColor,color);Paint(parts[18],cover,null,A(pale,.45));
     for(int eye=0;eye<(kind==1?2:1);eye++)Paint(parts[20],root*T((.47-eye*.17)*r,side*(.15+eye*.04)*r,side*.18),C(238,247,161),null);
    }
    var seam=parts[3];Paint(seam,root,null,A(color,.65));
    for(int i=0;i<3;i++)RegionalEllipse(root*T(-.5*r+i*r*.3),r*.1,r*.42,A(color,.18),A(color,.55),w*.6);
    var claw=parts[4];
    for(int side=-1;side<=1;side+=2){var head=root*T(.52*r,side*.25*r,side*(.15+.08*Math.Sin(time*3)-attack*.28),sx:1,sy:side);Paint(claw,head,dark,color);RegionalEllipse(root*T(.45*r,side*.28*r),r*.075,r*.075,C(240,255,180),null);}
    for(int side=-1;side<=1;side+=2){
     double sweep=side*(.28+.12*Math.Sin(time*3.4+side*.6)-attack*.25);
     var antenna=root*T(.60*r,side*.20*r,sweep,sx:1,sy:side);
     Paint(parts[12],antenna,null,color);
     Paint(parts[12],antenna*T(.32*r,-.12*r,angle:-.3+.16*Math.Sin(time*4)),null,A(color,.75));
     for(int i=0;i<2;i++)Paint(parts[13],root*T((-.42+i*.35)*r,side*.38*r,side*(.28+.05*Math.Sin(time*4+i)+attack*.22),sx:.6,sy:side*.6),dark,A(color,.85));
    }
   }else if(style==3){
    var shard=parts[0];
    int n=kind==2?8:kind==1?4:6;
    for(int i=0;i<n;i++){double a=i*Math.PI*2/n;var local=root*T(Math.Cos(a)*r*(.5+attack*.16),Math.Sin(a)*r*(.5+attack*.16),a+.08*Math.Sin(time*2+i));Paint(shard,local,shellColor,color);Paint(parts[16],local*T(sx:.67,sy:.64),RegionalInk(color,.68),null);var facet=parts[1];Paint(facet,local,null,A(pale,.55));}
    var core=parts[2];Paint(core,root,C(13,34,48),C(220,251,255));
    var facetCore=parts[3];Paint(facetCore,root,RegionalInk(color,.60),null);
    for(int side=-1;side<=1;side+=2){var cheek=root*T(-.13*r,side*.13*r,side*(.25+attack*.14),sx:kind==2?1.1:.85,sy:side);Paint(parts[17],cheek,shellColor,color);Paint(parts[20],root*T(.23*r,side*.15*r,sy:side),C(235,255,255),null);}
    Paint(parts[19],root*T(-.08*r),null,A(pale,.8));
    for(int side=-1;side<=1;side+=2){
     var orbit=root*T(angle:side*(time*.30+attack*.18));
     Paint(parts[14],orbit*T(0,side*r*.72,sx:.7,sy:.7),A(color,.16),C(210,250,255));
     Paint(parts[15],orbit*T(0,side*r*.47,angle:side*Math.PI*.5),null,A(color,.48));
    }
   }else if(style==4){
    for(int i=0;i<3;i++){var mantle=root*T(angle:i*Math.PI*2/3+time*.18+attack*.08);Paint(parts[17],mantle,RegionalInk(color,.34),A(color,.85));Paint(parts[18],mantle,null,A(pale,.5));}
    Paint(parts[16],root,RegionalInk(color,.58),color);
    Paint(parts[20],root*T(.19*r),C(37,10,21),pale);Paint(parts[19],root*T(.17*r,0,sx:.4,sy:.45),C(255,255,220),null);
    var filament=parts[0];
    for(int i=0;i<3;i++){var local=root*T(angle:i*Math.PI*2/3+time*.65+attack*.18);Paint(filament,local,null,A(color,.84));RegionalEllipse(local*T(-.8*r,-.35*r),r*.07,r*.07,C(255,246,200,.75),null);}
    var vortex=parts[1];Paint(vortex,root*T(angle:-time*.8),null,C(255,245,205));RegionalEllipse(root,r*.15,r*.25,A(color,.5),C(255,255,235),w*.6);
    for(int i=0;i<3;i++){
     double a=i*Math.PI*2/3-time*.48;
     var link=root*T(Math.Cos(a)*r*(.45+attack*.12),Math.Sin(a)*r*(.45+attack*.12),a+.20*Math.Sin(time*3+i));
     Paint(parts[13],link,A(color,.12),A(color,.78));
     Paint(parts[14],link*T(.42*r,0,angle:-time*.7,sx:.5,sy:.5),A(color,.3),C(255,245,205,.8));
    }
   }else{
    var body=parts[0];Paint(body,root,dark,color);
    for(int side=-1;side<=1;side+=2){var mask=root*T(.02*r,side*.03*r,side*(.08+attack*.12),sy:side);Paint(parts[16],mask,shellColor,color);for(int rib=0;rib<3;rib++)Paint(parts[18],root*T((-.38+rib*.18)*r,side*.12*r,side*.12,sy:side),null,A(pale,.48));}
    Paint(parts[17],root*T(.32*r),C(10,5,20),A(pale,.85));
    for(int side=-1;side<=1;side+=2)for(int tooth=0;tooth<3;tooth++)Paint(parts[19],root*T((.16+tooth*.13)*r,side*.14*r,sy:side),pale,null);
    var blade=parts[1];
    for(int side=-1;side<=1;side+=2)for(int i=0;i<2;i++){double a=side*(.6+i*.5)+Math.Sin(time*4+i+side)*.12-side*attack*.22;Paint(blade,root*T(-.15*r,side*.4*r,a,sx:1,sy:side),shellColor,A(color,.85));}
    RegionalEllipse(root*T(-.12*r),r*.31,r*.4,C(1,3,9),A(color,.7),w);
    for(int side=-1;side<=1;side+=2)RegionalEllipse(root*T(.4*r,side*.18*r),r*.12,r*.045,C(241,211,255),null);
    var fissure=parts[2];Paint(fissure,root,null,C(232,185,255));
   }
   if(style==5){
    for(int side=-1;side<=1;side+=2){
     var joint=root*T(-.38*r,side*.30*r,Math.PI+side*(.28+.12*Math.Sin(time*3)-attack*.30),sx:1,sy:side);
     Paint(parts[13],joint,dark,color);
     Paint(parts[13],joint*T(.42*r,0,angle:.25+.16*Math.Sin(time*3.8+side)),dark,A(color,.72));
     Paint(parts[14],root*T(.32*r,side*.16*r,angle:side*(.4-attack*.35),sx:.65,sy:.65),dark,C(232,185,255));
    }
   }
   // Role anatomy is part of the silhouette, not a floating role icon.
   if(style==2&&kind==4)for(int i=0;i<3;i++)RegionalEllipse(root*T((-.65+i*.20)*r,(i%2==0?-.13:.13)*r),r*.20,r*.22,RegionalInk(color,.65),pale,w*.5);
   if(kind==1){
    if(style==2){RegionalEllipse(root*T(.38*r),r*.27,r*.23,RegionalInk(color,.58),pale,w*.65);Paint(parts[4],root*T(.55*r,0,sx:.8,sy:.7),shellColor,pale);}
    else if(style==3){Paint(parts[0],root*T(.48*r,0,sx:.9,sy:.65),RegionalInk(color,.60),pale);Paint(parts[18],root*T(.78*r),pale,null);}
    else if(style==4)Paint(parts[17],root*T(.33*r,0,Math.PI,sx:.7,sy:.8),RegionalInk(color,.50),pale);
    else Paint(parts[17],root*T(.55*r,0,sx:.85,sy:.65),shellColor,pale);
   }
   if(kind==2||e.Elite||e.Boss){for(int side=-1;side<=1;side+=2){var armor=root*T(-.12*r,side*.48*r,side*.20,sy:side);Paint(style==3?parts[17]:style==2?parts[16]:parts[10],armor,shellColor,color);}}
   if(style==5&&(kind==4||kind==6))for(int side=-1;side<=1;side+=2)Paint(parts[16],root*T(-.25*r,side*.17*r,side*(.22+attack*.10),sx:1.15,sy:side*1.15),RegionalInk(color,.55),color);
   if(kind==3){for(int i=0;i<3;i++){double a=i*Math.PI*2/3+time*.5;RegionalEllipse(root*T(Math.Cos(a)*r*1.2,Math.Sin(a)*r*1.2),r*.1,r*.1,A(color,.45),color,w*.7);}}
   if(kind==1){var tip=parts[11];Paint(tip,root,A(color,.35),color);}
  }
 }
}
