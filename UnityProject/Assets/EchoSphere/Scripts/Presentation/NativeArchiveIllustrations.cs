using EchoSphere.Core;using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Semantic vector emblems: distinct base silhouettes; mutations retain their parent plus branch/final anatomy.
 internal static class NativeArchiveIllustrations {
  internal sealed class Spec {internal string Family,Key,Color,Mutation;internal int Branch=-1,Final=-1,Chapter=-1;internal bool Fusion;}
  static readonly Dictionary<string,Spec> Cache=new Dictionary<string,Spec>(StringComparer.Ordinal);
  internal static Spec Resolve(string id){if(Cache.TryGetValue(id,out var saved))return saved;Spec s=null;var run=NativeRunCatalog.Instance;
   if(id.StartsWith("sphere:")){var row=run.GetSphere(id.Substring(7));if(row!=null)s=new Spec{Family="sphere",Key=row.Id,Color=row.Color};}
   else if(id.StartsWith("ability:")){var row=run.GetAbility(id.Substring(8));if(row!=null)s=new Spec{Family="ability",Key=row.Id,Color="#8bcfff"};}
   else if(id.StartsWith("sphere-branch:")||id.StartsWith("sphere-final:")){foreach(var sphere in run.Spheres)for(int b=0;b<sphere.Branches.Count;b++){var branch=sphere.Branches[b];string prefix="sphere-final:"+sphere.Id+":"+branch.Id+":";if(id=="sphere-branch:"+branch.Id||id.StartsWith(prefix,StringComparison.Ordinal)){int final=-1;if(id.StartsWith(prefix,StringComparison.Ordinal))int.TryParse(id.Substring(prefix.Length),out final);s=new Spec{Family="sphere",Key=sphere.Id,Color=sphere.Color,Mutation=branch.Id,Branch=b,Final=final};}}}
   else if(id.StartsWith("ability-evolution:")){string key=id.Substring(18);foreach(var ability in run.Abilities)for(int b=0;b<ability.Branches.Count;b++){var branch=ability.Branches[b];if(key==branch.Id)s=new Spec{Family="ability",Key=ability.Id,Color="#bfa3ff",Mutation=branch.Id,Branch=b};var finals=branch.HasFinalPool?branch.Finals:ability.FallbackFinals;for(int f=0;f<finals.Count;f++)if(key==finals[f].Id)s=new Spec{Family="ability",Key=ability.Id,Color="#bfa3ff",Mutation=branch.Id,Branch=b,Final=f};}}
   else if(id.StartsWith("synergy:")){foreach(var link in run.SphereAbilityLinks)if(id=="synergy:"+link.Id){var parent=Resolve("sphere-branch:"+link.Branch);if(parent!=null)s=new Spec{Family=parent.Family,Key=parent.Key,Color=parent.Color,Mutation=parent.Mutation,Branch=parent.Branch,Fusion=true};break;}}
   else if(id.StartsWith("rune:")){string key=id.Substring(5);s=new Spec{Family="rune",Key=key,Color=run.GetRune(key).Color};}
   else if(id.StartsWith("region-lore:")){var parts=id.Split(':');if(parts.Length==3){int.TryParse(parts[2],out int chapter);string[] names={"basin","rift","bastion","folds","storm","archive"};string regionKey=CampaignRegions.TryParse(parts[1],out var region)?names[(int)region]:parts[1];s=new Spec{Family="region",Key=regionKey,Chapter=chapter,Color=RegionColor(regionKey)};}}
   else if(id.StartsWith("basic:")||id.StartsWith("currency:")){int split=id.IndexOf(':');s=new Spec{Family=id.Substring(0,split),Key=id.Substring(split+1),Color=id.EndsWith("Credits")?"#ffcf6c":id.EndsWith("ResonanceShards")?"#c7a4ff":id.EndsWith("ArchiveData")?"#6ae8d4":"#67ddf2"};}
   Cache[id]=s;return s;
  }
  static string RegionColor(string key)=>key=="basin"?"#f49370":key=="rift"?"#6bdcff":key=="bastion"?"#b5df78":key=="folds"?"#85cfff":key=="storm"?"#ffdb70":"#c4a4ff";
  internal static bool Draw(Painter2D p,string id,Vector2 origin,float scale){var spec=Resolve(id);if(spec==null)return false;ColorUtility.TryParseHtmlString(spec.Color,out var color);p.strokeColor=color;p.fillColor=new Color(color.r*.16f,color.g*.16f,color.b*.16f,.95f);p.lineWidth=1.25f*scale;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
   Vector2 V(float x,float y)=>origin+new Vector2(x,y)*scale;
   void Line(params float[] points){p.BeginPath();p.MoveTo(V(points[0],points[1]));for(int i=2;i<points.Length;i+=2)p.LineTo(V(points[i],points[i+1]));p.Stroke();}
   void Poly(params float[] points){p.BeginPath();p.MoveTo(V(points[0],points[1]));for(int i=2;i<points.Length;i+=2)p.LineTo(V(points[i],points[i+1]));p.ClosePath();p.Fill();p.Stroke();}
   void Ring(float x,float y,float radius,bool fill=false){p.BeginPath();p.Arc(V(x,y),radius*scale,0,360);p.ClosePath();if(fill)p.Fill();p.Stroke();}
   void Arc(float radius,float start,float end){p.BeginPath();p.Arc(origin,radius*scale,start,end);p.Stroke();}
   void Gem(float x,float y,float r){Poly(x,y-r,x+r*.65f,y,x,y+r,x-r*.65f,y);Line(x,y-r,x,y+r);}
   void Bolt(){Poly(2,-10,-6,1,0,1,-2,10,7,-2,1,-2);}
   void Shield(){Poly(0,-11,9,-7,8,3,0,11,-8,3,-9,-7);Line(-4,0,-1,4,5,-4);}
   void Clock(){Ring(0,0,9);Line(0,-6,0,0,5,2);Line(-3,-12,3,-12);}
   void Wing(float sign){Poly(sign*2,0,sign*11,-7,sign*9,1,sign*4,5);Line(sign*5,1,sign*9,-3);}
   string key=spec.Key;
   if(spec.Family=="sphere"){
    Ring(0,2,5,true);Ring(0,2,7);p.fillColor=new Color(color.r*.45f,color.g*.45f,color.b*.45f,1);
    switch(key){case "standard":Poly(-5,-6,-6,-11,-2,-13,2,-13,6,-11,5,-6);Line(-9,-8,-7,-10);Line(9,-8,7,-10);Line(-2,-10,2,-10);break;
    case "sniper":Poly(-3,-6,-3,-14,0,-16,3,-14,3,-6);Line(-9,-7,-5,-10,-5,-14);Line(9,-7,5,-10,5,-14);break;
    case "shotgun":Poly(-9,-6,-10,-11,-4,-14,4,-14,10,-11,9,-6);Line(-6,-9,-4,-12);Line(0,-9,0,-13);Line(6,-9,4,-12);break;
    case "chain":Line(-11,-7,-6,-11,-8,-3);Line(11,-7,6,-11,8,-3);Line(-12,4,-8,9,-10,13);Line(12,4,8,9,10,13);break;
    case "aura":for(int i=0;i<4;i++){float a=i*Mathf.PI/2;float x=Mathf.Cos(a)*11,y=2+Mathf.Sin(a)*11;Gem(x,y,2);}Arc(13,20,70);Arc(13,110,160);Arc(13,200,250);Arc(13,290,340);break;
    case "orbital":Arc(12,0,150);Arc(12,180,330);Poly(-14,-4,-8,-3,-13,1);Poly(14,8,8,7,13,3);break;
    case "prism":Poly(-6,-6,-9,-11,0,-15,9,-11,6,-6);Line(-5,-10,0,-13,5,-10);Line(0,-13,0,-7);break;
    case "gravity":Arc(12,10,140);Arc(10,190,320);Line(-13,9,-9,10,-10,6);Line(13,-6,10,-9,9,-5);break;
    case "pulse":Arc(11,195,345);Arc(14,205,335);Line(-12,7,-10,10);Line(12,7,10,10);break;
    case "void":Poly(-6,-6,-10,-9,-6,-14,-2,-9);Poly(6,-6,10,-9,6,-14,2,-9);Arc(11,15,165);Ring(0,2,2);break;}
   }else if(spec.Family=="ability"){
    switch(key){case "radius":Ring(0,0,5,true);Arc(12,10,160);Arc(12,190,340);Line(-14,0,-9,-3,-9,3,-14,0);Line(14,0,9,-3,9,3,14,0);break;
    case "damage":Poly(-2,-11,8,-9,4,-1,9,5,0,11,-8,5,-5,-2);Line(-8,-10,-4,-6);Line(8,10,4,6);break;
    case "attackspeed":Bolt();Line(-13,-5,-8,-5);Line(-13,0,-9,0);Line(-12,5,-7,5);break;
    case "maxspheres":Ring(-6,3,5,true);Ring(6,3,5,true);Ring(0,-6,5,true);Line(9,-11,9,-5);Line(6,-8,12,-8);break;
    case "blast":Ring(0,0,3,true);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Line(Mathf.Cos(a)*6,Mathf.Sin(a)*6,Mathf.Cos(a)*13,Mathf.Sin(a)*13);}Arc(10,15,75);Arc(10,105,165);Arc(10,195,255);Arc(10,285,345);break;
    case "shield":Shield();Arc(14,195,345);break;
    case "teleport":Ring(-7,0,5);Ring(7,0,5);Line(-3,-4,3,4);Line(-3,4,3,-4);break;
    case "movespeed":Wing(-1);Wing(1);Line(-3,8,0,11,3,8);break;
    case "slow":Ring(0,0,8);Line(-11,-8,-11,8);Line(-14,5,-11,9,-8,5);Line(-4,-4,4,4);break;
    case "vitality":Poly(0,10,-10,0,-9,-7,-4,-10,0,-6,4,-10,9,-7,10,0);Line(-6,0,-2,0,0,-4,2,4,4,0,7,0);break;
    case "firetrail":Poly(-9,9,-11,2,-7,-4,-5,1,0,-12,3,-3,8,-7,11,2,8,9);Line(-4,8,-3,2,1,-2,3,5,5,8);break;
    case "minion":Ring(0,0,5,true);Wing(-1);Wing(1);Line(-4,-9,0,-12,4,-9);Line(-2,9,2,9);break;
    case "vampire":Poly(-10,-7,-3,8,-1,-3,1,-3,3,8,10,-7,6,-10,-6,-10);Ring(0,10,2);break;
    case "lightning":Bolt();Arc(13,30,130);Arc(13,210,310);break;
    case "dodge":Poly(0,-10,4,0,0,10,-4,0);Line(-12,-5,-7,0,-12,5);Line(12,-5,7,0,12,5);break;
    case "crit":Ring(0,0,7);Line(-12,0,-4,0);Line(4,0,12,0);Line(0,-12,0,-4);Line(0,4,0,12);Gem(0,0,3);break;
    case "timestop":Clock();Line(-3,-3,-3,3);Line(3,-3,3,3);break;
    case "magnet":Line(-9,-9,-9,4,-5,9,5,9,9,4,9,-9);Poly(-12,-12,-6,-12,-6,-6,-12,-6);Poly(6,-12,12,-12,12,-6,6,-6);break;
    case "sphereboost":Ring(0,2,6,true);Line(-8,-6,0,-13,8,-6);Line(-6,-1,0,-6,6,-1);break;
    case "darkritual":Poly(0,-12,11,7,-11,7);Ring(0,1,4);Line(0,-5,0,7);Gem(0,12,2);break;
    case "armor":Poly(-9,-8,0,-12,9,-8,8,9,-8,9);Line(-8,-3,8,-3);Line(0,-10,0,9);break;
    case "regen":Ring(0,0,4,true);Arc(11,20,300);Line(3,-11,8,-11,8,-6);Line(-2,0,2,0);Line(0,-2,0,2);break;
    case "resonance":Ring(-6,0,5);Ring(6,0,5);Line(-2,-9,2,-9);Line(-2,9,2,9);Gem(0,0,3);break;
    case "cooldown":Clock();Line(-13,1,-10,5,-6,5);break;
    case "critpower":Poly(0,-13,4,-4,13,0,4,4,0,13,-4,4,-13,0,-4,-4);Ring(0,0,3);break;
    case "range":Poly(-10,-5,3,-5,8,0,3,5,-10,5);Line(8,0,14,0);Line(11,-3,14,0,11,3);Ring(-7,0,2);break;
    case "proj_speed":Poly(11,0,2,-5,2,5);Line(-12,-5,-1,-5);Line(-14,0,-1,0);Line(-12,5,-1,5);break;
    case "momentum":Poly(-7,-11,2,-7,7,2,2,11,-7,7,-3,0);Line(-13,-5,-9,-5);Line(-13,5,-10,5);break;
    case "fortune":Poly(0,-12,10,-6,10,6,0,12,-10,6,-10,-6);Ring(-4,-3,1);Ring(4,3,1);Ring(0,0,1);break;
    case "stability":Poly(-11,8,0,-11,11,8);Line(-7,3,7,3);Ring(0,3,2);Line(-13,12,13,12);break;
    default:return false;}
   }else if(spec.Family=="rune"){
    if(key=="phase"){Poly(0,-15,8,-8,15,0,8,8,0,15,-8,8,-15,0,-8,-8);Line(-6,-6,6,6);Line(6,-6,-6,6);}
    else if(key=="harvest"){Poly(0,-15,11,-6,8,8,0,15,-8,8,-11,-6);for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Line(0,0,Mathf.Cos(a)*8,Mathf.Sin(a)*8);}}
    else if(key=="purge"){p.BeginPath();for(int i=0;i<10;i++){float a=-Mathf.PI/2+i*Mathf.PI/5,r=i%2==0?15:7;var v=V(Mathf.Cos(a)*r,Mathf.Sin(a)*r);if(i==0)p.MoveTo(v);else p.LineTo(v);}p.ClosePath();p.Fill();p.Stroke();Ring(0,0,3);}
    else if(key=="resonance"){Ring(0,0,14);Ring(0,0,6);Line(4,0,12,0);}
    else if(key=="fortify")Shield();else if(key=="hunt"){Line(-14,-8,0,0,-14,8);Line(14,-8,0,0,14,8);Ring(0,0,5,true);}
    else if(key=="echo"){Ring(0,0,4);Ring(0,0,9);Ring(0,0,14);}
    else if(key=="gravity"){for(int i=0;i<4;i++){float a=i*90;p.BeginPath();p.Arc(origin,(5+i*3)*scale,a,a+109);p.Stroke();}Ring(0,0,2,true);}
    else if(key=="runic_cell"){Poly(0,-15,11,-6,11,6,0,15,-11,6,-11,-6);Ring(0,0,5,true);}
    else{Poly(0,-14,12,0,0,14,-12,0);Ring(0,0,3,true);}
   }else if(spec.Family=="region"){
    if(key=="basin"){Poly(-13,6,-9,-6,-3,-2,0,-11,4,-3,9,-7,13,6);Line(-12,9,12,9);Ring(0,3,3);}
    else if(key=="rift"){Poly(0,-14,5,-3,13,8,3,5,0,11,-3,5,-13,8,-5,-3);Line(-9,-11,-6,-6);Line(9,-11,6,-6);}
    else if(key=="bastion"){Poly(-11,-9,0,-13,11,-9,9,8,0,13,-9,8);Line(0,-10,0,11);Line(-8,-3,8,-3);Line(-7,4,7,4);}
    else if(key=="folds"){Gem(0,0,12);Line(-9,-8,7,4);Line(-7,8,9,-4);Gem(0,0,4);}
    else if(key=="storm"){Bolt();Arc(13,20,145);Arc(13,200,325);}
    else{Poly(-11,-9,8,-9,11,-6,11,9,-11,9);Line(-7,-5,7,-5);Line(-7,0,1,0);Line(-7,5,7,5);Ring(7,0,2);}
    p.strokeColor=new Color(1,.9f,.67f);switch(spec.Chapter){case 1:Ring(-10,10,3);Line(-12,10,-8,10);break;case 2:Line(-14,11,-10,7,-6,11);break;case 3:Poly(-14,10,-10,6,-6,10,-10,14);break;case 4:Line(-14,9,-12,13,-8,13,-6,9);break;}
   }else if(spec.Family=="basic"){
    if(key=="network"){Poly(0,-11,11,8,-11,8);Ring(0,-11,3,true);Ring(-11,8,3,true);Ring(11,8,3,true);Ring(0,2,4,true);Line(0,-8,0,-2);Line(-8,6,-3,3);Line(8,6,3,3);}
    else{Ring(0,0,4,true);Arc(9,0,250);Arc(14,90,340);Line(-10,-10,-7,-7);Line(10,10,7,7);}
   }else if(spec.Family=="currency"){
    if(key=="Credits"){Ring(0,0,12,true);Poly(0,-8,6,-4,6,4,0,8,-6,4,-6,-4);Line(-2,-4,2,-4);Line(-2,4,2,4);}
    else if(key=="ResonanceShards"){Gem(-5,2,9);Gem(7,-3,6);}
    else if(key=="ArchiveData"){Poly(-10,-11,6,-11,10,-7,10,11,-10,11);Line(-6,-5,5,-5);Line(-6,0,6,0);Line(-6,5,2,5);}
    else{Ring(0,0,10);Line(-13,0,-9,0,-5,-5,0,6,5,-5,9,0,13,0);}
   }
   if(spec.Branch>=0){
    // Small mechanic emblem sits inside the sphere core (or in an ability corner), rather than a random ID mark.
    float mx=spec.Family=="sphere"?0:10,my=spec.Family=="sphere"?2:-10;string mutation=spec.Mutation??"";
    var under=p.fillColor;p.fillColor=new Color(.025f,.045f,.075f,1);Ring(mx,my,3.7f,true);p.lineWidth=.8f*scale;p.strokeColor=new Color(.9f,.97f,1);
    if(mutation.Contains("singular")||mutation.Contains("gravity")||mutation.Contains("well")||mutation.Contains("collapse")){Ring(mx,my,1.2f);Line(mx-2.8f,my-2,mx-1.6f,my-1.2f);Line(mx+2.8f,my+2,mx+1.6f,my+1.2f);}
    else if(mutation.Contains("swarm")||mutation.Contains("split")||mutation.Contains("hail")||mutation.Contains("burst")){Gem(mx-1.8f,my+1,1.1f);Gem(mx+1.8f,my+1,1.1f);Gem(mx,my-1.5f,1.1f);}
    else if(mutation.Contains("leech")||mutation.Contains("hunger")||mutation.Contains("blood")||mutation.Contains("sacrifice")){Poly(mx-2.5f,my-1.8f,mx-1,my+2,mx-.2f,my-1,mx+.2f,my-1,mx+1,my+2,mx+2.5f,my-1.8f);}
    else if(mutation.Contains("execution")||mutation.Contains("oracle")||mutation.Contains("beacon")||mutation.Contains("anchor")){Ring(mx,my,1.4f);Line(mx-3,my,mx-1,my);Line(mx+1,my,mx+3,my);Line(mx,my-3,mx,my-1);}
    else if(mutation.Contains("web")||mutation.Contains("network")||mutation.Contains("relay")||mutation.Contains("linked")){Line(mx,my-2.5f,mx-2.5f,my+1.8f,mx+2.5f,my+1.8f,mx,my-2.5f);Ring(mx,my-2.5f,.6f);}
    else if(mutation.Contains("storm")||mutation.Contains("overload")||mutation.Contains("ignition")){Line(mx+1,my-2.8f,mx-1.7f,my,mx+.8f,my,mx-1,my+2.8f);}
    else if(mutation.Contains("sanctum")||mutation.Contains("guardian")||mutation.Contains("guard")||mutation.Contains("bastion")||mutation.Contains("phase")){Poly(mx,my-2.8f,mx+2.4f,my-1.4f,mx+1.7f,my+1.4f,mx,my+2.8f,mx-1.7f,my+1.4f,mx-2.4f,my-1.4f);}
    else if(mutation.Contains("mirror")||mutation.Contains("spectrum")||mutation.Contains("alchemy")||mutation.Contains("rift")){Gem(mx,my,2.8f);Line(mx-2,my+1,mx+2,my-1);}
    else if(mutation.Contains("blade")||mutation.Contains("breach")){Line(mx-2.5f,my+2.5f,mx+2.5f,my-2.5f);Line(mx-.7f,my+2,mx-2,my+.7f);}
    else if(mutation.Contains("growth")){Line(mx,my+2.8f,mx,my-2.5f);Line(mx,my,mx-2.5f,my-1.5f);Line(mx,my+1,mx+2.5f,my-1);}
    else{Ring(mx,my,1);Ring(mx,my,2.7f);}
    p.fillColor=under;
    p.strokeColor=spec.Branch==0?new Color(1,.64f,.35f):spec.Branch==1?new Color(.65f,.72f,1):new Color(.52f,1,.7f);p.lineWidth=1.4f*scale;
    // Mutation anatomy: paired braces, split vanes, or articulated outer crown. Final choices have distinct structural appendages.
    if(spec.Branch==0){Line(-14,-5,-12,-10,-9,-12);Line(14,-5,12,-10,9,-12);}else if(spec.Branch==1){Poly(-14,-7,-10,-5,-13,1);Poly(14,-7,10,-5,13,1);}else{Line(-14,4,-14,10,-9,13);Line(14,4,14,10,9,13);Ring(-14,0,1.4f);Ring(14,0,1.4f);}
    if(spec.Final==0){Line(-11,13,-5,13,-2,10);Line(11,13,5,13,2,10);}else if(spec.Final==1){Gem(-11,11,3);Gem(11,11,3);Line(-7,14,7,14);}else if(spec.Final==2){Poly(-14,9,-10,6,-6,14);Poly(14,9,10,6,6,14);Ring(0,13,2);}
   }
   if(spec.Fusion){p.strokeColor=new Color(.75f,1,.91f);p.lineWidth=1.3f*scale;Arc(15,18,118);Arc(15,198,298);Line(-13,-10,-10,-13,-8,-11);Line(13,10,10,13,8,11);}
   return true;
  }
 }
}
