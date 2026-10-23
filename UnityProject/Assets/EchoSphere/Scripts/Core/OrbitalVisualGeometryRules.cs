using System;

namespace EchoSphere.Core
{
    // Source orbitalGeometry display geometry. Combat rules are unchanged.
    public static class OrbitalVisualGeometryRules
    {
        public static (int Inner,int Outer) RingCounts(double level,double extraElements=0)
        {
            double effective=Math.Max(1,Math.Min(7,Math.Floor(level)));int inner=1,outer=1;
            for(int current=2;current<=effective;current++)if(current%2==0)outer++;else inner++;
            for(int extra=0;extra<Math.Max(0,Math.Floor(extraElements));extra++){
                if(outer<inner)outer++;else if(inner<outer)inner++;else if(extra%2==0)outer++;else inner++;
            }
            return (inner,outer);
        }
        public static double ElementAngle(double rotation,bool outer,int index,int count)
            =>rotation*(outer?-1:1)+index*((Math.PI*2)/Math.Max(1,count));
    }
}
