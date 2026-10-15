using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    // Column-major CSS matrices. Shared by portable browser comparisons and the
    // native compositor, so tests exercise the actual transform calculation.
    public static class VisualCssTransform
    {
        public static double[] Identity()=>new[]{1d,0d,0d,0d,0d,1d,0d,0d,0d,0d,1d,0d,0d,0d,0d,1d};
        public static double[] Multiply(double[] a,double[] b)
        {var result=new double[16];for(var c=0;c<4;c++)for(var r=0;r<4;r++)for(var k=0;k<4;k++)result[c*4+r]+=a[k*4+r]*b[c*4+k];return result;}
        public static bool Is2D(double[] value)=>value[2]==0d&&value[3]==0d&&value[6]==0d&&value[7]==0d&&value[8]==0d&&value[9]==0d&&value[10]==1d&&value[11]==0d&&value[14]==0d&&value[15]==1d;
        public static (double Rotation,double X,double Y,double ContentRotation) Native2D(double[] matrix)
        {
            var a=matrix[0];var b=matrix[1];var c=matrix[4];var d=matrix[5];var x=Math.Sqrt(a*a+b*b);var y=Math.Sqrt(c*c+d*d);
            // A single TRS is exact for orthogonal columns, including mirrors.
            // Nonuniform scale BEFORE rotation needs a second retained rotation.
            if(Math.Abs(a*c+b*d)<=1e-12*Math.Max(1d,x*y))return x>1e-12?(Math.Atan2(b,a),x,(a*d-b*c)/x,0d):(Math.Atan2(-c,d),0d,y,0d);
            var split=VisualBackdrop.Decompose(a,b,c,d);return (split.First,split.X,split.Y,split.Second);
        }
        public static double[] Parse(VisualCss sheet,VisualCssNode node,string declaration=null,bool origin=false)
        {
            var matrix=Identity();
            foreach(Match match in Regex.Matches(declaration??sheet.Get(node,"transform","none"),@"([A-Za-z0-9]+)\(([^()]*(?:\([^()]*\)[^()]*)*)\)")){
                var name=match.Groups[1].Value;var args=VisualCss.Split(match.Groups[2].Value,',').Select(value=>value.Trim()).ToArray();
                double N(int i,double fallback=0d)=>i<args.Length&&double.TryParse(args[i],NumberStyles.Float,CultureInfo.InvariantCulture,out var v)?v:fallback;
                double L(int i,bool y=false)=>i<args.Length?sheet.Length(args[i],y?node.Height:node.Width):0d;
                var next=Identity();
                if(name=="translate"||name=="translate3d"){next[12]=L(0);next[13]=L(1,true);next[14]=L(2);}
                else if(name=="translateX")next[12]=L(0);else if(name=="translateY")next[13]=L(0,true);else if(name=="translateZ")next[14]=L(0);
                else if(name=="scale"||name=="scale3d"){next[0]=N(0,1);next[5]=N(1,N(0,1));next[10]=name=="scale3d"?N(2,1):1;}
                else if(name=="scaleX")next[0]=N(0,1);else if(name=="scaleY")next[5]=N(0,1);else if(name=="scaleZ")next[10]=N(0,1);
                else if(name=="rotate"||name=="rotateZ"||name=="rotateX"||name=="rotateY"){
                    var angle=VisualCssPaint.Angle(args[0]);var c=Math.Cos(angle);var s=Math.Sin(angle);
                    if(name=="rotateX"){next[5]=c;next[6]=s;next[9]=-s;next[10]=c;}
                    else if(name=="rotateY"){next[0]=c;next[2]=-s;next[8]=s;next[10]=c;}
                    else{next[0]=c;next[1]=s;next[4]=-s;next[5]=c;}
                }
                else if(name=="matrix"){next[0]=N(0,1);next[1]=N(1);next[4]=N(2);next[5]=N(3,1);next[12]=N(4);next[13]=N(5);}
                else if(name=="matrix3d"){for(var i=0;i<16;i++)next[i]=N(i,i%5==0?1:0);}
                else if(name=="skewX")next[4]=Math.Tan(VisualCssPaint.Angle(args[0]));else if(name=="skewY")next[1]=Math.Tan(VisualCssPaint.Angle(args[0]));
                else if(name=="perspective")next[11]=-1d/L(0);
                else throw new InvalidOperationException("Unimplemented authored CSS transform "+name);
                matrix=Multiply(matrix,next);
            }
            if(!origin)return matrix;
            var parts=VisualCss.Split(sheet.Get(node,"transform-origin","50% 50%"),' ').Where(value=>value.Length>0).ToArray();
            double Position(string value,double size)=>value=="center"?size*.5d:value=="left"||value=="top"?0d:value=="right"||value=="bottom"?size:sheet.Length(value,size);
            var x=Position(parts.Length>0?parts[0]:"50%",node.Width);var y=Position(parts.Length>1?parts[1]:"50%",node.Height);var z=parts.Length>2?sheet.Length(parts[2],node.Width):0d;
            var before=Identity();before[12]=x;before[13]=y;before[14]=z;var after=Identity();after[12]=-x;after[13]=-y;after[14]=-z;return Multiply(Multiply(before,matrix),after);
        }
    }
}
