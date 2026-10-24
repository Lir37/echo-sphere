using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // User-approved four-form trial. Immutable faceted meshes, articulated through
    // matrices; no textures, per-frame triangulation, gameplay state or RNG writes.
    internal sealed partial class NativeWorldCreatures
    {
        private Material _hostileFacet;
        private readonly Dictionary<int,Mesh> _hostileMeshes=new Dictionary<int,Mesh>();
        private readonly List<MaterialPropertyBlock> _hostileProperties=new List<MaterialPropertyBlock>();
        private int _hostileUsed;
        private static string SimpleHostileKind(EnemyView e)
        {
            if(e.Boss)return null;
            if(e.Role=="ranged"||e.Role=="sniper")return "shooter";
            return e.Variant=="skitter"?"runner":e.Variant=="wisp"?"floater":e.Variant=="brute"?"heavy":null;
        }
        private Mesh HostileMesh(int kind)
        {
            if(_hostileMeshes.TryGetValue(kind,out var existing))return existing;
            Vector2[] points;
            switch(kind){
                case 0:points=new[]{P(-.62,-.43),P(.12,-.49),P(.98,0),P(.12,.49),P(-.62,.43)};break;
                case 1:points=new[]{P(-.32,-.47),P(.22,-.58),P(.70,-.24),P(.76,.22),P(.20,.50),P(-.44,.32)};break;
                case 2:points=new[]{P(-.48,-.34),P(.24,-.45),P(.73,-.13),P(.56,.28),P(-.18,.42),P(-.59,.16)};break;
                case 3:points=new[]{P(-.34,-.10),P(.18,-.15),P(.45,.32),P(.28,.68),P(.10,.68),P(.14,.28)};break;
                case 4:points=new[]{P(-.55,-.18),P(.50,-.47),P(.65,-.04),P(.52,.37),P(-.55,.19)};break;
                case 5:points=new[]{P(-.25,-.34),P(.12,-.40),P(.34,-.14),P(.29,.22),P(-.08,.37),P(-.31,.14)};break;
                default:points=new[]{P(-.5,0),P(0,-.5),P(.5,0),P(0,.5)};break;
            }
            var vertices=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();
            void Triangle(Vector2 a,Vector2 b,Vector2 c,Color tint,Color? centerTint=null){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(tint);colors.Add(tint);colors.Add(centerTint??tint);indices.Add(n);indices.Add(n+1);indices.Add(n+2);}
            var center=new Vector2(-.06f,-.12f);
            for(int i=0;i<points.Length;i++){
                var a=points[i];var b=points[(i+1)%points.Length];var ia=a*.79f+new Vector2(0,-.07f);var ib=b*.79f+new Vector2(0,-.07f);
                float light=points[i].y<0?.19f:.10f;var side=new Color(light*.72f,light*.88f,light*1.35f,1);
                Triangle(a,b,ia,side);Triangle(b,ib,ia,side);
                var top=new Color(.16f,.20f,.30f,1);Triangle(ia,ib,center,top,new Color(.29f,.35f,.48f,1));
            }
            var mesh=new Mesh {name="Hostile articulated facet "+kind,hideFlags=HideFlags.DontSave};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();_hostileMeshes.Add(kind,mesh);return mesh;
        }
        private void HostilePart(NativeWorldSurface surface,int kind,Matrix4x4 matrix,Color accent,double hit=0,double opacity=1,bool emissive=false)
        {
            if(_hostileFacet==null)_hostileFacet=new Material(Resources.Load<Shader>("EchoSphere/NativeHostileFacet")){name="Articulated hostile volumes",hideFlags=HideFlags.DontSave};
            if(_hostileUsed==_hostileProperties.Count)_hostileProperties.Add(new MaterialPropertyBlock());var p=_hostileProperties[_hostileUsed++];p.Clear();p.SetVector("_NativeColor",new Vector4(accent.r,accent.g,accent.b,1));p.SetFloat("_Hit",(float)hit);
            if(emissive){surface.DrawNativeMesh(HostileMesh(6),_solid,Properties(accent),matrix,opacity:(float)opacity,additive:true);return;}
            surface.DrawNativeMesh(HostileMesh(kind),_hostileFacet,p,matrix,opacity:(float)opacity);
        }
        private bool DrawSimpleHostile(NativeWorldSurface surface,EnemyView e,double t,Matrix4x4 root,double travel)
        {
            var kind=SimpleHostileKind(e);if(kind==null)return false;
            double hit=Clamp(e.HitFlash/.15),phase=travel*2.7;
            Color accent=kind=="floater"?C(201,95,245):kind=="heavy"?C(255,182,67):C(255,85,94);
            if(e.Freeze)accent=C(135,225,255);
            if(e.Elite)accent=Color.Lerp(accent,C(255,188,72),.6f);
            var m=root*T(sx:e.Radius,sy:e.Radius);
            DrawGroundShadow(surface,e.Radius*.84,e.Radius*.43,root);
            if(kind=="runner"||kind=="heavy"){
                for(int side=-1;side<=1;side+=2)for(int leg=0;leg<2;leg++){
                    double stride=Math.Sin(phase+leg*Math.PI+(side<0?Math.PI:0)),lift=Math.Max(0,Math.Cos(phase+leg*Math.PI+(side<0?Math.PI:0)));
                    var limb=m*T(leg==0?-.39:.30,side*.29,side*stride*.26,sx:kind=="heavy"?1.05:.82,sy:side*(1-lift*.11));HostilePart(surface,3,limb,accent,hit*.4);
                }
            }
            if(kind=="runner"){
                HostilePart(surface,0,m*T(y:-.04-Math.Sin(phase*2)*.025),accent,hit);
                HostilePart(surface,6,m*T(.58,-.025,sx:.58,sy:.085),accent,emissive:true);
            }else if(kind=="heavy"){
                double shift=Math.Sin(phase)*.035;
                HostilePart(surface,2,m*T(-.34,-.04+shift,sx:.86,sy:.91),accent,hit*.65);
                HostilePart(surface,2,m*T(.30,.02-shift,sx:.90,sy:.95),accent,hit*.65);
                HostilePart(surface,1,m*T(0,-.10,sx:.94,sy:.9),accent,hit);
                HostilePart(surface,6,m*T(.44,.20,sx:.38,sy:.065),accent,emissive:true);
            }else if(kind=="floater"){
                double bob=e.Freeze?0:Math.Sin(t*2.4+e.X*.01)*.055,open=e.Freeze?0:.08*Math.Sin(t*2.1);
                HostilePart(surface,5,m*T(y:bob,sx:1.1,sy:1.1),accent,hit);
                HostilePart(surface,6,m*T(.04,bob,sx:.09,sy:.58),accent,emissive:true);
                for(int side=-1;side<=1;side+=2){var wing=m*T(-.08,side*(.54+open)+bob,side*open,sx:.97,sy:side*.83);HostilePart(surface,4,wing,accent,hit);HostilePart(surface,6,wing*T(-.30,0,sx:.06,sy:.50),accent,emissive:true);}
            }else{
                double period=e.Role=="sniper"?2.2:3.2;
                double recoil=Clamp(1-(period-e.ShotTimer)/.16),charge=e.ShotTimer>0?Clamp(1-e.ShotTimer/.32):0;
                if(e.Freeze){recoil=0;charge=0;}
                var gun=m*T(-recoil*.12,0);
                HostilePart(surface,5,gun*T(sx:1.5,sy:1.5),accent,hit);
                for(int side=-1;side<=1;side+=2)HostilePart(surface,4,gun*T(-.10,side*(.30+charge*.065),side*.12,sx:.98,sy:side*.66),accent,hit);
                HostilePart(surface,6,gun*T(.48,0,sx:.22+charge*.12,sy:.20+charge*.06),accent,opacity:.65+charge*.35,emissive:true);
                if(recoil>.05)HostilePart(surface,6,m*T(.79,0,sx:.35*recoil,sy:.10*recoil),C(255,226,170),opacity:recoil,emissive:true);
            }
            if(e.Elite){HostilePart(surface,2,m*T(-.1,-.27,sx:.6,sy:.52),accent,hit);HostilePart(surface,6,m*T(-.05,-.48,sx:.45,sy:.065),C(255,193,83),emissive:true);}
            if(e.Tier>0)for(int i=0;i<Math.Min(3,e.Tier);i++)HostilePart(surface,6,m*T(-.42+i*.22,-.31,sx:.09,sy:.06),accent,emissive:true);
            // Surface-local status cues, driven solely by existing status flags.
            if(e.Fire||e.Poison||e.Freeze){var status=e.Freeze?C(154,232,255):e.Fire?C(255,132,56):C(119,224,118);for(int i=0;i<3;i++)HostilePart(surface,6,m*T(-.32+i*.25,-.16+i%2*.25,.6,sx:.30,sy:.04),status,opacity:.7,emissive:true);}
            if(hit>0){double q=1-hit;for(int i=0;i<3;i++){double angle=i*2.1+.5;HostilePart(surface,6,m*T(Math.Cos(angle)*(.35+q*.5),Math.Sin(angle)*(.35+q*.5),angle,sx:.17*hit,sy:.045*hit),C(255,231,190),opacity:hit,emissive:true);}}
            return true;
        }
        private void DisposeSimpleHostiles(){foreach(var mesh in _hostileMeshes.Values)Destroy(mesh);_hostileMeshes.Clear();Destroy(_hostileFacet);_hostileFacet=null;_hostileProperties.Clear();}
    }
}
