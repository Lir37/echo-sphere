using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Individual two-tone emblems. Rarity belongs to the surrounding card, not the illustration.
 internal static class NativeArtifactIllustrations {
  internal readonly struct Design {internal readonly string Main,Detail;internal readonly int Index;internal Design(string main,string detail,int index){Main=main;Detail=detail;Index=index;}internal string Geometry=>Main+"/"+Detail+"/"+Index%7;}
  private static readonly Dictionary<string,Design> Designs=Build();
  internal static int Count=>Designs.Count;
  internal static string GeometryId(string id)=>Designs.TryGetValue(id,out var d)?d.Geometry:null;
  private static Dictionary<string,Design> Build(){var map=new Dictionary<string,Design>(StringComparer.Ordinal);foreach(var row in Data.Split('\n')){var line=row.Trim();if(line.Length==0)continue;var c=line.Split('|');map.Add(c[0],new Design(c[1],c[2],map.Count));}return map;}
  internal static bool Draw(Painter2D p,string id,Vector2 origin,float scale){
   if(!Designs.TryGetValue(id,out var d))return false;
   Color primary=Color.HSVToRGB(Mathf.Repeat(.49f+d.Index*.61803398875f,1),.48f+(d.Index%3)*.08f,.98f),secondary=Color.HSVToRGB(Mathf.Repeat(.67f+d.Index*.61803398875f,1),.35f,.98f);
   Vector2 V(float x,float y)=>origin+new Vector2(x,y)*scale;
   p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;p.lineWidth=1.05f*scale;p.strokeColor=new Color(primary.r,primary.g,primary.b,.48f);
   void Stroke(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.Stroke();}
   switch(d.Index%7){
    case 0:Stroke(V(-13,-5),V(-13,-11),V(-6,-13));Stroke(V(5,13),V(12,10),V(13,3));break;
    case 1:p.BeginPath();p.Arc(origin,14*scale,145,290);p.Stroke();p.BeginPath();p.Arc(origin,14*scale,-25,85);p.Stroke();break;
    case 2:Stroke(V(-13,7),V(-13,-9),V(-8,-13));Stroke(V(6,13),V(13,7),V(13,-6));break;
    case 3:Stroke(V(-14,-7),V(-10,-11),V(9,-11),V(13,-7));Stroke(V(-12,5),V(-8,12),V(4,12));break;
    case 4:Stroke(V(-13,3),V(-9,-12),V(8,-12));Stroke(V(13,-2),V(9,12),V(-5,12));break;
    case 5:Stroke(V(-14,-2),V(-10,-8),V(-6,-11));Stroke(V(4,13),V(11,10),V(14,4));break;
    default:Stroke(V(-13,8),V(-13,-8),V(-9,-12));Stroke(V(0,13),V(10,13),V(13,9),V(13,-1));break;
   }
   Glyph(p,d.Main,origin+new Vector2(-1.8f,-1.8f)*scale,.82f*scale,primary);
   // The secondary object is part of the silhouette, not a microscopic ID stamp.
   Glyph(p,d.Detail,origin+new Vector2(7.6f,7.3f)*scale,.40f*scale,secondary);
   return true;
  }
  private static void Glyph(Painter2D p,string kind,Vector2 o,float s,Color color){
   Vector2 V(float x,float y)=>o+new Vector2(x,y)*s;
   p.strokeColor=color;p.fillColor=new Color(color.r*.28f,color.g*.28f,color.b*.28f,.9f);p.lineWidth=1.7f*s;
   void Line(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.Stroke();}
   void Shape(params Vector2[] q){p.BeginPath();p.MoveTo(q[0]);for(int i=1;i<q.Length;i++)p.LineTo(q[i]);p.ClosePath();p.Fill();p.Stroke();}
   void Ring(float x,float y,float r){p.BeginPath();p.Arc(V(x,y),r*s,0,360);p.ClosePath();p.Stroke();}
   void Gem(float x,float y,float r){Shape(V(x,y-r),V(x+r*.65f,y-1),V(x+r*.35f,y+r),V(x-r*.35f,y+r),V(x-r*.65f,y-1));Line(V(x,y-r),V(x-2,y),V(x,y+r));}
   switch(kind){
    case "gem":Gem(0,0,11);break;
    case "boot":Shape(V(-5,-10),V(3,-10),V(3,1),V(10,5),V(10,9),V(-7,9));Line(V(-6,5),V(8,5));break;
    case "heart":Shape(V(0,10),V(-10,0),V(-9,-7),V(-3,-9),V(0,-5),V(3,-9),V(9,-7),V(10,0));Line(V(-6,0),V(-2,0),V(0,-3),V(3,3),V(6,0));break;
    case "ring":Ring(0,1,9);Gem(0,-8,4);break;
    case "leaf":Shape(V(-8,9),V(-8,-3),V(0,-10),V(10,-10),V(9,2),V(0,9));Line(V(-9,10),V(6,-6));Line(V(-3,4),V(-4,-2));break;
    case "eye":Shape(V(-12,0),V(-6,-6),V(4,-6),V(12,0),V(4,6),V(-6,6));Ring(0,0,4);break;
    case "bolt":Shape(V(2,-12),V(-8,1),V(-1,1),V(-3,12),V(9,-2),V(2,-2));break;
    case "crown":Shape(V(-10,-8),V(-5,-2),V(0,-11),V(5,-2),V(10,-8),V(8,9),V(-8,9));Gem(0,3,3);break;
    case "network":Shape(V(0,-10),V(10,8),V(-10,8));Line(V(0,-10),V(0,2),V(10,8));Line(V(0,2),V(-10,8));Ring(0,-10,2);Ring(-10,8,2);Ring(10,8,2);break;
    case "shield":Shape(V(-10,-9),V(10,-9),V(9,2),V(0,11),V(-9,2));Line(V(-5,-3),V(0,5),V(5,-3));break;
    case "anchor":Line(V(0,-9),V(0,10));Ring(0,-10,2);Line(V(-9,3),V(-7,7),V(0,10),V(7,7),V(9,3));Line(V(-5,-3),V(5,-3));break;
    case "orb":Ring(0,0,8);p.BeginPath();p.Arc(V(0,0),12*s,25,245);p.Stroke();Gem(8,-8,3);break;
    case "clock":Ring(0,0,10);Line(V(0,-7),V(0,0),V(5,3));Line(V(-4,-13),V(4,-13));break;
    case "hook":Line(V(6,-10),V(-3,-10),V(-4,6),V(-1,10),V(5,8),V(7,3));Shape(V(7,3),V(3,5),V(7,-1));break;
    case "lens":Ring(0,0,8);Ring(0,0,11);Line(V(-11,0),V(-6,0));Line(V(6,0),V(11,0));Line(V(0,-11),V(0,-6));Line(V(0,6),V(0,11));break;
    case "dice":Shape(V(0,-11),V(10,-5),V(10,6),V(0,12),V(-10,6),V(-10,-5));Line(V(-10,-5),V(0,1),V(10,-5));Line(V(0,1),V(0,12));Ring(0,-4,1);Ring(-5,4,1);Ring(5,4,1);break;
    case "gear":for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(V(Mathf.Cos(a)*8,Mathf.Sin(a)*8),V(Mathf.Cos(a)*12,Mathf.Sin(a)*12));}Ring(0,0,8);Ring(0,0,3);break;
    case "coil":for(int i=0;i<4;i++){float x=-7+i*4;p.BeginPath();p.Arc(V(x,0),6*s,-100,100);p.Stroke();}Line(V(-12,-6),V(-7,-6));Line(V(6,6),V(12,6));break;
    case "fan":for(int i=-1;i<=1;i++)Shape(V(-4,8),V(i*7-3,-10),V(i*7+3,-10),V(4,8));break;
    case "page":Shape(V(-8,-11),V(3,-11),V(9,-5),V(9,11),V(-8,11));Line(V(3,-11),V(3,-5),V(9,-5));Line(V(-4,0),V(5,0));Line(V(-4,5),V(2,5));break;
    case "compass":Ring(0,0,10);Shape(V(0,-13),V(4,0),V(0,13),V(-4,0));Line(V(-12,0),V(12,0));break;
    case "weave":for(int i=-1;i<=1;i++){Line(V(-10,i*5-3),V(0,i*5+2),V(10,i*5-3));Line(V(i*5-3,-10),V(i*5+2,0),V(i*5-3,10));}break;
    case "lamp":Shape(V(-7,-8),V(7,-8),V(9,9),V(-9,9));Ring(0,-10,3);Gem(0,1,5);break;
    case "sun":Ring(0,0,5);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(V(Mathf.Cos(a)*8,Mathf.Sin(a)*8),V(Mathf.Cos(a)*12,Mathf.Sin(a)*12));}break;
    case "split":Line(V(-9,-10),V(-3,-4),V(-3,6),V(-8,11));Line(V(9,-10),V(3,-4),V(3,6),V(8,11));Ring(0,0,11);break;
    case "blade":Shape(V(-10,10),V(-3,-6),V(10,-12),V(7,2),V(-5,8));Line(V(-7,7),V(7,-8));break;
    case "bottle":Shape(V(-4,-11),V(4,-11),V(4,-5),V(9,3),V(7,10),V(-7,10),V(-9,3),V(-4,-5));Line(V(-7,2),V(7,2));break;
    case "fang":Shape(V(-10,-7),V(-3,-5),V(0,10),V(3,-5),V(10,-7),V(7,5),V(0,12),V(-7,5));break;
    case "wing":Shape(V(-9,8),V(-8,-2),V(-1,-10),V(11,-11),V(4,-5),V(9,-5),V(1,1),V(5,1),V(-3,7));break;
    case "star":Shape(V(0,-12),V(3,-4),V(11,-3),V(5,2),V(7,11),V(0,6),V(-7,11),V(-5,2),V(-11,-3),V(-3,-4));break;
    case "arrow":Shape(V(-10,4),V(3,-6),V(-1,-10),V(11,-11),V(10,1),V(6,-3),V(-5,9));break;
    case "cross":Shape(V(-3,-10),V(3,-10),V(3,-3),V(10,-3),V(10,3),V(3,3),V(3,10),V(-3,10),V(-3,3),V(-10,3),V(-10,-3),V(-3,-3));break;
    case "rune":Line(V(-8,10),V(-8,-10),V(8,0),V(-8,6));Line(V(-1,-5),V(7,-11));Line(V(-1,5),V(7,11));break;
    case "mask":Shape(V(-10,-9),V(10,-9),V(8,6),V(0,11),V(-8,6));Line(V(-7,-1),V(-3,1));Line(V(7,-1),V(3,1));break;
    case "prism":Shape(V(0,-12),V(11,8),V(-11,8));Line(V(0,-12),V(0,3),V(11,8));Line(V(0,3),V(-11,8));break;
    case "lattice":Shape(V(-9,-9),V(9,-9),V(9,9),V(-9,9));Line(V(-9,0),V(9,0));Line(V(0,-9),V(0,9));Line(V(-9,-9),V(9,9));break;
    default:throw new ArgumentException("Unknown artifact illustration motif: "+kind);
   }
  }
  private const string Data=@"
crystal_speed|gem|wing
amulet_hp|heart|shield
ring_xp|ring|star
regen_stone|gem|cross
radius_shard|gem|orb
vampire_ring|ring|fang
swift_boots|boot|wing
luck_talisman|rune|dice
mage_pendant|gem|rune
long_lens|lens|arrow
stasis_core|orb|clock
dragon_heart|heart|wing
mirror|page|eye
predator_claw|blade|fang
foresight_eye|eye|star
echo_conductor|bolt|network
heavy_core|orb|anchor
scattering_matrix|fan|lattice
aura_lens|lens|sun
network_relay|network|bolt
chaos_orb|orb|dice
resonance_core|gem|coil
lone_bastion|shield|orb
fivefold_resonance|sun|network
relay_matrix|lattice|coil
triangle_circuit|network|lattice
overclock|clock|bolt
soul_engine|gear|mask
time_anchor|anchor|clock
void_contract|page|mask
mirror_network|eye|network
singularity_engine|gear|orb
quantum_core|orb|split
zero_sphere|ring|mask
unified_mind|mask|weave
network_anchor|anchor|network
pulse_lens|lens|coil
orbit_charm|ring|blade
prism_shard|prism|gem
gravity_bead|orb|hook
void_ink|bottle|mask
echo_thread|weave|ring
folded_core|page|orb
paper_ward|page|shield
mirror_dust|bottle|eye
resonant_leaf|leaf|bolt
signal_knot|weave|rune
lattice_chip|lattice|gem
fractal_seed|leaf|network
sniper_scope|lens|blade
chain_battery|coil|bolt
shotgun_shell|bottle|fan
aura_mist|bottle|sun
orbital_blade|blade|ring
prism_filter|prism|lens
gravity_hook|hook|anchor
pulse_driver|gear|sun
void_mark|rune|mask
formation_compass|compass|network
geometry_die|dice|network
network_coil|coil|network
crit_sigil|rune|blade
tempo_ring|ring|clock
triangle_engine|gear|network
ring_engine|gear|ring
lattice_engine|gear|lattice
fractal_engine|gear|leaf
resonance_lattice|lattice|sun
echo_weaver|weave|coil
gravity_crown|crown|anchor
void_lantern|lamp|mask
prism_crown|crown|prism
orbital_crown|crown|ring
pulse_crown|crown|sun
chain_crown|crown|bolt
sniper_crown|crown|lens
time_splitter|clock|split
stasis_mandala|sun|clock
quantum_fold|page|split
void_star|star|mask
axiom_core|gem|compass
universal_fold|page|weave
glass_capacitor|bottle|bolt
kinetic_brace|shield|arrow
dense_lens|lens|gem
rapid_coil|coil|arrow
living_alloy|shield|leaf
ritual_clock|clock|rune
blood_crystal|gem|heart
unstable_sigil|rune|split
";
 }
}
