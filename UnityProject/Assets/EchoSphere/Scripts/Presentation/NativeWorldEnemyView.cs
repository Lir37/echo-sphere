using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        internal struct EnemyView
        {
            internal object Identity;
            internal double X,Y,Radius,Rotation,HitFlash,Tier,Hp,MaxHp,LinkTimer,LinkX,LinkY,LinkWindow;
            internal bool HasRotation,Boss,Elite,Fire,Poison,Freeze,LinkTarget;
            internal string Variant,BossName;
            internal Color Color;
            internal BossSignal Signal;
        }
        private sealed class Facing {internal double X,Y,Angle,Travel,Time,AnatomyTime,Motion;}
        private readonly ConditionalWeakTable<object,Facing> _facing=new ConditionalWeakTable<object,Facing>();
        private readonly List<NativeWorldLabel> _bossLabels=new List<NativeWorldLabel>();
        private int _labelUsed;
        private static double Clamp(double value)=>Math.Max(0,Math.Min(1,value));
        private Art Multi(int kind,double radius,double width,Func<BezierContour[]> create)
        {
            var key=(kind,radius,0d,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);
            if(!_art.TryGetValue(key,out var art)){var contours=create();art=new Art();art.Build(contours[0],(float)width,key.Item5,key.Item6,key.Item7,false,contours);_art.Add(key,art);}return art;
        }
        private static BezierContour Open(params Vector2[] points)
        {
            var segments=new BezierPathSegment[points.Length];for(int i=0;i<points.Length;i++){var a=points[i];var b=points[Math.Min(i+1,points.Length-1)];segments[i]=new BezierPathSegment {P0=a,P1=Vector2.Lerp(a,b,1f/3),P2=Vector2.Lerp(a,b,2f/3)};}return new BezierContour {Segments=segments,Closed=false};
        }
        private double FacingAngle(EnemyView e,double time)
        {
            if(!_facing.TryGetValue(e.Identity,out var previous)){
                previous=new Facing {Angle=e.HasRotation?e.Rotation:0,Time=time,AnatomyTime=time,X=e.X,Y=e.Y};_facing.Add(e.Identity,previous);
            }else{
                double dx=e.X-previous.X,dy=e.Y-previous.Y,distance=Math.Sqrt(dx*dx+dy*dy),dt=Math.Max(0,time-previous.Time);
                if(distance>.2)previous.Angle=Math.Atan2(dy,dx);
                if(!e.Freeze&&dt>0){previous.Travel+=distance/Math.Max(1,e.Radius);previous.AnatomyTime+=dt;double speed=distance>.00001?1:0;previous.Motion+=(speed-previous.Motion)*(1-Math.Exp(-dt*14));}
            }
            previous.X=e.X;previous.Y=e.Y;previous.Time=time;
            _gaitTravel=previous.Travel;_gaitBlend=previous.Motion;_gaitActive=true;
            return previous.Angle;
        }
        private void LinkBreaker(Matrix4x4 root,EnemyView e,double t)
        {
            if(!e.Elite||!e.LinkTarget||e.LinkTimer<=0)return;double remaining=Clamp(e.LinkTimer/e.LinkWindow),pulse=.65+.35*Math.Sin(t*16),dx=e.LinkX-e.X,dy=e.LinkY-e.Y;var red=C(255,77,112);
            var line=Dynamic(Straight(Vector2.zero,P(dx,dy)),2.5,false);Paint(line,root,null,red,(float)(.30+(1-remaining)*.55),forceOver:true,shadow:red,shadowBlur:4);
            var glyph=Multi(41,0,2,()=>new[]{Open(P(-16,-10),P(-5,0),P(-16,10)),Open(P(16,-10),P(5,0),P(16,10))});var local=root*T(dx,dy,Math.Atan2(dy,dx));Paint(glyph,local,null,red,(float)(.55+.35*pulse),forceOver:true,shadow:red,shadowBlur:4);
            var bars=Multi(42,0,1,()=>new[]{Straight(P(-9,-15),P(9,-15)),Straight(P(-9,15),P(9,15))});Paint(bars,local,null,red,.32f,forceOver:true,shadow:red,shadowBlur:4);
        }
        internal void DrawDisabledNetwork(NativeWorldSurface surface,double timer,double window,double t)
        {
            if(timer<=0)return;_surface=surface;double remaining=Clamp(timer/window),pulse=.6+.4*Math.Sin(t*13);var red=C(255,77,112);
            var glyph=Multi(43,0,2.2,()=>new[]{Open(P(-17,-10),P(-4,0),P(-17,10)),Open(P(17,-10),P(4,0),P(17,10))});Paint(glyph,Matrix4x4.identity,null,red,(float)(.55+.35*pulse),forceOver:true,shadow:red,shadowBlur:4);
            // The source arc retains a zero-length path when remaining == 1.
            var arc=Dynamic(Oval(27,27,(1-remaining)*Math.PI*2),1.2,false);Paint(arc,T(angle:-Math.PI/2),null,red,.28f,forceOver:true,shadow:red,shadowBlur:4);
        }
        internal void DrawEnemy(NativeWorldSurface surface,EnemyView e,double t)
        {
            _surface=surface;double facing=FacingAngle(e,t),poseTime=_facing.GetOrCreateValue(e.Identity).AnatomyTime,hit=Clamp(e.HitFlash/.15),pulse=1+Math.Sin(poseTime*5+e.X*.01)*.025,scale=pulse+hit*.05;var root=T(e.X,e.Y,facing,scale,scale);var statusColor=e.Freeze?C(105,214,255):e.Color;
            DrawOuterGlow(surface,e.Radius*(e.Boss?1.65:1.15),statusColor,e.Boss?.21:.10,root);
            if(e.Boss){DrawBossSignal(surface,e.Signal,t,root);Draw(surface,e.Radius,e.Color,poseTime,e.Variant,true,root);}
            else{
                Draw(surface,e.Radius,e.Color,poseTime,e.Variant,false,root);if(e.Elite)DrawElite(surface,e.Radius,e.Color,t,root);
                if(e.Tier>0){double n=Math.Min(3,e.Tier);var local=root*T(0,-e.Radius*1.55,t*.35);var ray=Get(44,e.Radius,0,1.2,()=>Straight(P(e.Radius*.10,0),P(e.Radius*.38,0)),false);for(int i=0;i<n;i++)Paint(ray,local*T(angle:i*Math.PI*2/n),null,A(statusColor,.46));}
                LinkBreaker(root,e,t);if(e.Fire||e.Poison||e.Freeze)DrawStatus(surface,e.Radius,t,e.Fire,e.Poison,e.Freeze,root);
                if(hit>0){var cross=Multi(45,e.Radius,2,()=>new[]{Straight(P(-e.Radius*.8,-e.Radius*.8),P(e.Radius*.8,e.Radius*.8)),Straight(P(e.Radius*.8,-e.Radius*.8),P(-e.Radius*.8,e.Radius*.8))});Paint(cross,root,null,C(255,255,255,hit*.75));}
            }

            if(e.Boss){double w=Math.Max(110,e.Radius*1.55),ratio=Clamp(e.Hp/Math.Max(1,e.MaxHp)),x=e.X-w/2,y=e.Y-e.Radius-30;var bar=Get(46,w,6,1,()=>Poly(Vector2.zero,P(w,0),P(w,6),P(0,6)));Paint(bar,T(x,y),C(2,7,18,.88),null,shadow:statusColor);Paint(bar,T(x,y,sx:ratio),statusColor,null,shadow:statusColor);Paint(bar,T(x,y),null,C(220,250,255,.68),shadow:statusColor);
                if(_labelUsed==_bossLabels.Count)_bossLabels.Add(new NativeWorldLabel());_bossLabels[_labelUsed++].Draw(surface,e.BossName,P(e.X,y-4),C(230,250,255,.78),statusColor,true);}

        }
    }
}
