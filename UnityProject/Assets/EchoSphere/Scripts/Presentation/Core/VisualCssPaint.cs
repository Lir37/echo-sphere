using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    // Native CSS brush. Colors remain in the authored sRGB space; interpolation
    // and source-over use premultiplied alpha, including transparent gradient stops.
    public static class VisualCssPaint
    {
        public sealed class Raster {public int Width,Height;public double Left,Top,Scale;public byte[] Pixels;}
        private sealed class Gradient
        {
            public string Kind;public bool Repeat;public double X,Y,Rx,Ry,Angle,Extent;
            public readonly List<Tuple<double,VisualCssColor>> Stops=new List<Tuple<double,VisualCssColor>>();
            public VisualCssColor At(double x,double y)
            {
                var position=Kind=="linear"?((x-X)*Math.Sin(Angle)-(y-Y)*Math.Cos(Angle))/Extent+.5d:Kind=="conic"?((Math.Atan2(x-X,-(y-Y))-Angle)/(Math.PI*2d)%1d+1d)%1d:Math.Sqrt((x-X)*(x-X)/(Rx*Rx)+(y-Y)*(y-Y)/(Ry*Ry));
                var first=Stops[0];var last=Stops[Stops.Count-1];if(Repeat&&last.Item1>first.Item1)position=first.Item1+((position-first.Item1)%(last.Item1-first.Item1)+(last.Item1-first.Item1))%(last.Item1-first.Item1);
                if(position<first.Item1)return first.Item2;
                for(var i=1;i<Stops.Count;i++)if(position<Stops[i].Item1){var before=Stops[i-1];var after=Stops[i];return VisualCssColor.Lerp(before.Item2,after.Item2,(position-before.Item1)/(after.Item1-before.Item1));}
                return last.Item2;
            }
        }
        private sealed class Shadow {public double X,Y,Blur,Spread;public VisualCssColor Color;public bool Inset;}
        private sealed class Layer {public Gradient Gradient;public double Width,Height,X,Y;public bool RepeatX,RepeatY;public string Clip;}
        private sealed class DashPath
        {
            private readonly List<(double X,double Y,double Length)> _points=new List<(double,double,double)>();
            public double Length=>_points[_points.Count-1].Length;
            public void Add(double x,double y){var length=0d;if(_points.Count>0){var previous=_points[_points.Count-1];length=previous.Length+Math.Sqrt((x-previous.X)*(x-previous.X)+(y-previous.Y)*(y-previous.Y));}_points.Add((x,y,length));}
            public double Position(double x,double y){var closest=double.PositiveInfinity;var position=0d;for(var i=1;i<_points.Count;i++){var a=_points[i-1];var b=_points[i];var dx=b.X-a.X;var dy=b.Y-a.Y;var square=dx*dx+dy*dy;if(square==0d)continue;var t=Math.Max(0d,Math.Min(1d,((x-a.X)*dx+(y-a.Y)*dy)/square));var px=a.X+dx*t;var py=a.Y+dy*t;var distance=(x-px)*(x-px)+(y-py)*(y-py);if(distance<closest){closest=distance;position=a.Length+(b.Length-a.Length)*t;}}return position;}
        }
        private static double DashCoverage(double position,double length,double thickness,bool closed,double scale)
        {
            var dash=thickness*(thickness>=3d?2d:3d);var gap=thickness*(thickness>=3d?1d:2d);
            if(length<=dash*2d)return 1d;
            var two=2d*dash+(closed?2d:1d)*gap;
            if(length<=two){var ratio=length/two;dash*=ratio;gap*=ratio;}
            else {var count=Math.Floor((length+(closed?0d:gap))/(dash+gap));var first=(length-count*dash)/(closed?count:count-1d);var second=(length-(count+1d)*dash)/(closed?count+1d:count);gap=second<=0d||Math.Abs(first-gap)<Math.Abs(second-gap)?first:second;}
            var phase=(position%(dash+gap)+(dash+gap))%(dash+gap);var distance=phase<=dash?-Math.Min(phase,dash-phase):Math.Min(phase-dash,dash+gap-phase);
            return Math.Max(0d,Math.Min(1d,.5d-distance*scale));
        }
        private static double Number(string value)=>double.Parse(value,NumberStyles.Float,CultureInfo.InvariantCulture);
        private static string[] Words(string value)=>VisualCss.Split(value.Trim(),' ').Where(word=>word.Length>0).ToArray();
        public static double Angle(string value)=>value.EndsWith("turn",StringComparison.Ordinal)?Number(value.Substring(0,value.Length-4))*Math.PI*2d:value.EndsWith("rad",StringComparison.Ordinal)?Number(value.Substring(0,value.Length-3)):Number(value.Replace("deg",""))*Math.PI/180d;
        private static bool Color(string value)=>value.StartsWith("#",StringComparison.Ordinal)||value.Contains("(")&&!value.StartsWith("calc(",StringComparison.Ordinal)||new[]{"transparent","currentColor","black","white"}.Contains(value);
        private static Gradient ParseGradient(VisualCss sheet,VisualCssNode node,string value,double width,double height)
        {
            var start=value.IndexOf('(');var kind=value.Substring(0,start);var args=VisualCss.Split(value.Substring(start+1,value.LastIndexOf(')')-start-1),',');
            var result=new Gradient {Kind=kind.Contains("linear")?"linear":kind.Contains("conic")?"conic":"radial",Repeat=kind.StartsWith("repeating-",StringComparison.Ordinal),X=width*.5d,Y=height*.5d,Rx=width*.5d,Ry=height*.5d,Angle=Math.PI,Extent=height};
            var offset=Color(Words(args[0])[0])?0:1;var spec=offset==1?args[0].Trim():"";
            if(result.Kind=="linear") {
                if(spec.StartsWith("to ",StringComparison.Ordinal)){var horizontal=spec.Contains("right")?1d:spec.Contains("left")?-1d:0d;var vertical=spec.Contains("bottom")?1d:spec.Contains("top")?-1d:0d;result.Angle=horizontal!=0d&&vertical!=0d?Math.Atan2(horizontal*height,-vertical*width):Math.Atan2(horizontal,-vertical);}
                else if(spec.Length>0)result.Angle=Angle(spec);
                result.Extent=Math.Abs(width*Math.Sin(result.Angle))+Math.Abs(height*Math.Cos(result.Angle));
            } else {
                var at=spec.IndexOf("at ",StringComparison.Ordinal);var positions=Words(at<0?"50% 50%":spec.Substring(at+3));
                double Position(string text,double dimension)=>text=="center"?dimension*.5d:text=="left"||text=="top"?0d:text=="right"||text=="bottom"?dimension:sheet.Length(text,dimension);
                result.X=Position(positions[0],width);result.Y=Position(positions.Length>1?positions[1]:"50%",height);
                if(result.Kind=="conic"){var from=Regex.Match(spec,@"from\s+([^\s]+)");result.Angle=from.Success?Angle(from.Groups[1].Value):0d;result.Extent=1d;}
                else {
                    var left=Math.Abs(result.X);var right=Math.Abs(width-result.X);var top=Math.Abs(result.Y);var bottom=Math.Abs(height-result.Y);var closest=spec.Contains("closest");
                    var rx=closest?Math.Min(left,right):Math.Max(left,right);var ry=closest?Math.Min(top,bottom):Math.Max(top,bottom);
                    if(spec.StartsWith("circle",StringComparison.Ordinal)){
                        var radius=spec.Contains("side")?(closest?Math.Min(rx,ry):Math.Max(rx,ry)):Math.Sqrt(rx*rx+ry*ry);var explicitSize=Regex.Match(spec,@"^circle\s+([\d.]+(?:px|%))");if(explicitSize.Success)radius=sheet.Length(explicitSize.Groups[1].Value,Math.Sqrt(width*width+height*height)/Math.Sqrt(2d));result.Rx=result.Ry=Math.Max(.00001d,radius);
                    } else {
                        if(!spec.Contains("side")){rx*=Math.Sqrt(2d);ry*=Math.Sqrt(2d);}var explicitSize=Regex.Match(spec,@"^(?:ellipse\s+)?([\d.]+(?:px|%))\s+([\d.]+(?:px|%))");if(explicitSize.Success){rx=sheet.Length(explicitSize.Groups[1].Value,width);ry=sheet.Length(explicitSize.Groups[2].Value,height);}result.Rx=Math.Max(.00001d,rx);result.Ry=Math.Max(.00001d,ry);
                    }
                    result.Extent=result.Rx;
                }
            }
            var stops=new List<Tuple<double?,VisualCssColor>>();
            for(var i=offset;i<args.Count;i++) {
                var words=Words(args[i]);var color=VisualCssColor.Parse(words[0],sheet.Get(node,"color","#000000"));
                double Stop(string text)=>result.Kind=="conic"?text.EndsWith("%",StringComparison.Ordinal)?Number(text.TrimEnd('%'))*.01d:Angle(text)/(Math.PI*2d):sheet.Length(text,result.Extent)/result.Extent;
                if(words.Length==1)stops.Add(Tuple.Create((double?)null,color));else foreach(var word in words.Skip(1))stops.Add(Tuple.Create((double?)Stop(word),color));
            }
            if(stops.Count==0)throw new FormatException("CSS gradient has no color stops: "+value);
            if(!stops[0].Item1.HasValue)stops[0]=Tuple.Create((double?)0d,stops[0].Item2);if(!stops[stops.Count-1].Item1.HasValue)stops[stops.Count-1]=Tuple.Create((double?)1d,stops[stops.Count-1].Item2);
            var maximum=double.NegativeInfinity;for(var i=0;i<stops.Count;i++)if(stops[i].Item1.HasValue){maximum=Math.Max(maximum,stops[i].Item1.Value);stops[i]=Tuple.Create((double?)maximum,stops[i].Item2);}
            for(var i=0;i<stops.Count;i++)if(!stops[i].Item1.HasValue){var end=i;while(!stops[end].Item1.HasValue)end++;var previous=stops[i-1].Item1.Value;var next=stops[end].Item1.Value;var count=end-i+1;for(var j=i;j<end;j++)stops[j]=Tuple.Create((double?)(previous+(next-previous)*(j-i+1)/count),stops[j].Item2);i=end;}
            foreach(var stop in stops)result.Stops.Add(Tuple.Create(stop.Item1.Value,stop.Item2));return result;
        }
        private static VisualCssColor Over(VisualCssColor under,VisualCssColor over)
        {var alpha=over.A+under.A*(1d-over.A);return alpha<=0d?new VisualCssColor(0,0,0,0):new VisualCssColor((over.R*over.A+under.R*under.A*(1d-over.A))/alpha,(over.G*over.A+under.G*under.A*(1d-over.A))/alpha,(over.B*over.A+under.B*under.A*(1d-over.A))/alpha,alpha);}
        public static Raster Paint(VisualCss sheet,VisualCssNode node,double scale)
        {
            var width=node.Width;var height=node.Height;scale=Math.Max(.1d,scale);
            string S(string property,string fallback="")=>sheet.Get(node,property,fallback);
            double L(string value,double reference)=>sheet.Length(value,reference,sheet.Length(S("font-size","16px"),16d));
            var current=S("color","#000000");var shadows=new List<Shadow>();var outset=0d;
            foreach(var source in VisualCss.Split(S("box-shadow","none"),',')) {
                if(source.Trim()=="none"||source.Trim().Length==0)continue;var words=Words(source);var lengths=words.Where(word=>word!="inset"&&!Color(word)).Select(word=>L(word,width)).ToArray();var color=words.FirstOrDefault(Color)??current;
                var shadow=new Shadow {Inset=words.Contains("inset"),X=lengths.Length>0?lengths[0]:0d,Y=lengths.Length>1?lengths[1]:0d,Blur=lengths.Length>2?lengths[2]:0d,Spread=lengths.Length>3?lengths[3]:0d,Color=VisualCssColor.Parse(color,current)};shadows.Add(shadow);
                if(!shadow.Inset)outset=Math.Max(outset,Math.Max(Math.Abs(shadow.X),Math.Abs(shadow.Y))+Math.Max(0d,shadow.Spread)+shadow.Blur*2d);
            }
            // Huge tutorial cutout shadows are clipped to the viewport, as in the DOM.
            var left=Math.Min(outset,Math.Max(0d,node.X));var top=Math.Min(outset,Math.Max(0d,node.Y));var right=Math.Min(outset,Math.Max(0d,sheet.Width-node.X-width));var bottom=Math.Min(outset,Math.Max(0d,sheet.Height-node.Y-height));
            var raster=new Raster {Width=Math.Max(1,(int)Math.Ceiling((width+left+right)*scale)),Height=Math.Max(1,(int)Math.Ceiling((height+top+bottom)*scale)),Left=-left,Top=-top,Scale=scale};
            var count=checked(raster.Width*raster.Height);var pixels=new VisualCssColor[count];var ownPixels=new VisualCssColor[count];var borderPixels=new VisualCssColor[count];var shapes=new double[count];var innerShapes=new double[count];
            var cornerNames=new[]{"top-left","top-right","bottom-right","bottom-left"};
            var corners=cornerNames.Select(corner=>L(Words(S("border-"+corner+"-radius","0"))[0],width)).ToArray();
            var cornersY=cornerNames.Select(corner=>{var words=Words(S("border-"+corner+"-radius","0"));return L(words.Length>1?words[1]:words[0],height);}).ToArray();
            var factor=Math.Min(1d,Math.Min(Math.Min(width/Math.Max(.00001d,corners[0]+corners[1]),width/Math.Max(.00001d,corners[3]+corners[2])),Math.Min(height/Math.Max(.00001d,cornersY[0]+cornersY[3]),height/Math.Max(.00001d,cornersY[1]+cornersY[2]))));for(var i=0;i<4;i++){corners[i]=Math.Max(0d,corners[i]*factor);cornersY[i]=Math.Max(0d,cornersY[i]*factor);}
            double Shape(double x,double y,double insetX=0d,double insetY=0d,double spread=0d){
                var px=x-insetX+spread;var py=y-insetY+spread;var w=width+spread*2d;var h=height+spread*2d;if(w<=0d||h<=0d)return 0d;
                var corner=py<h*.5d?(px<w*.5d?0:1):px<w*.5d?3:2;var rx=Math.Max(0d,corners[corner]+spread);var ry=Math.Max(0d,cornersY[corner]+spread);
                var cx=corner==0||corner==3?rx:w-rx;var cy=corner<2?ry:h-ry;
                var distance=Math.Max(Math.Max(-px,px-w),Math.Max(-py,py-h));
                if(rx>0d&&ry>0d&&(corner==0||corner==3?px<rx:px>w-rx)&&(corner<2?py<ry:py>h-ry)){
                    var dx=px-cx;var dy=py-cy;var normalized=Math.Sqrt(dx*dx/(rx*rx)+dy*dy/(ry*ry));var gradient=normalized>0?Math.Sqrt(dx*dx/(rx*rx*rx*rx)+dy*dy/(ry*ry*ry*ry))/normalized:1d/Math.Min(rx,ry);distance=(normalized-1d)/Math.Max(.00001d,gradient);
                }
                return Math.Max(0d,Math.Min(1d,.5d-distance*scale));
            }
            double X(int index)=>(index%raster.Width+.5d)/scale-left;double Y(int index)=>(index/raster.Width+.5d)/scale-top;
            void DrawShadow(Shadow shadow){
                // The blur samples the shifted hole beyond the element bounds.
                // Padding the mask avoids adding a false shadow on the opposite
                // edge when an inset shadow is offset towards that edge.
                var paddingPixels=(int)Math.Ceiling(shadow.Blur*2d*scale);var maskWidth=raster.Width+paddingPixels*2;var maskHeight=raster.Height+paddingPixels*2;var alpha=new double[maskWidth*maskHeight];
                for(var y=0;y<maskHeight;y++)for(var x=0;x<maskWidth;x++){var px=(x-paddingPixels+.5d)/scale-left;var py=(y-paddingPixels+.5d)/scale-top;alpha[y*maskWidth+x]=shadow.Inset?1d-Shape(px,py,shadow.X,shadow.Y,-shadow.Spread):Shape(px,py,shadow.X,shadow.Y,shadow.Spread);}
                if(shadow.Blur>0d)Blur(alpha,maskWidth,maskHeight,shadow.Blur*.5d*scale,shadow.Inset?1d:0d);
                for(var i=0;i<count;i++){var clip=shadow.Inset?innerShapes[i]/Math.Max(.00001d,shapes[i]):1d-Shape(X(i),Y(i));var c=shadow.Color;var at=(i/raster.Width+paddingPixels)*maskWidth+i%raster.Width+paddingPixels;var tint=new VisualCssColor(c.R,c.G,c.B,c.A*alpha[at]*clip);if(shadow.Inset)ownPixels[i]=Over(ownPixels[i],tint);else pixels[i]=Over(pixels[i],tint);}
            }

            foreach(var shadow in shadows.Where(value=>!value.Inset).Reverse())DrawShadow(shadow);
            var borders=new[]{"top","right","bottom","left"}.Select(side=>S("border-"+side+"-style","none")=="none"?0d:L(S("border-"+side+"-width","0"),width)).ToArray();var borderColors=new[]{"top","right","bottom","left"}.Select(side=>VisualCssColor.Parse(S("border-"+side+"-color",current),current)).ToArray();
            var padding=new[]{"top","right","bottom","left"}.Select(side=>L(S("padding-"+side,"0"),width)).ToArray();
            DashPath dashPath=null;
            if(corners.Any(radius=>radius>0d)&&new[]{"top","right","bottom","left"}.Any(side=>S("border-"+side+"-style")=="dashed")){
                dashPath=new DashPath();var ix=Math.Floor(borders[3]*.5d);var iy=Math.Floor(borders[0]*.5d);var iw=width-ix-Math.Floor(borders[1]*.5d);var ih=height-iy-Math.Floor(borders[2]*.5d);
                var rx=corners.Select((radius,i)=>Math.Max(0d,radius-Math.Floor(borders[i==0||i==3?3:1]*.5d))).ToArray();var ry=cornersY.Select((radius,i)=>Math.Max(0d,radius-Math.Floor(borders[i<2?0:2]*.5d))).ToArray();
                dashPath.Add(ix+rx[0],iy);
                foreach(var cornerIndex in new[]{1,2,3,0}){var cx=ix+(cornerIndex==0||cornerIndex==3?rx[cornerIndex]:iw-rx[cornerIndex]);var cy=iy+(cornerIndex<2?ry[cornerIndex]:ih-ry[cornerIndex]);var start=(cornerIndex-2)*Math.PI*.5d;
                    for(var step=0;step<=64;step++){var angle=start+step*Math.PI/128d;dashPath.Add(cx+rx[cornerIndex]*Math.Cos(angle),cy+ry[cornerIndex]*Math.Sin(angle));}}
            }
            var background=VisualCssColor.Parse(S("background-color","transparent"),current);var layers=new List<Layer>();
            var imageSources=VisualCss.Split(S("background-image","none"),',');var sizes=VisualCss.Split(S("background-size","auto"),',');var positions=VisualCss.Split(S("background-position","0% 0%"),',');var repeats=VisualCss.Split(S("background-repeat","repeat"),',');
            var origins=VisualCss.Split(S("background-origin","padding-box"),',');var clips=VisualCss.Split(S("background-clip","border-box"),',');
            for(var index=0;index<imageSources.Count;index++) {
                var image=imageSources[index].Trim();if(image=="none")continue;
                if(!image.Contains("gradient("))throw new InvalidOperationException("Authored CSS image requires a source image brush: "+image);
                var origin=origins[index%origins.Count].Trim();var ox=origin=="border-box"?0d:borders[3]+(origin=="content-box"?padding[3]:0d);var oy=origin=="border-box"?0d:borders[0]+(origin=="content-box"?padding[0]:0d);
                var ow=Math.Max(.00001d,width-ox-(origin=="border-box"?0d:borders[1]+(origin=="content-box"?padding[1]:0d)));var oh=Math.Max(.00001d,height-oy-(origin=="border-box"?0d:borders[2]+(origin=="content-box"?padding[2]:0d)));
                var size=Words(sizes[index%sizes.Count]);var w=size.Length>0&&size[0]!="auto"?L(size[0],ow):ow;var h=size.Length>1&&size[1]!="auto"?L(size[1],oh):oh;
                var position=Words(positions[index%positions.Count]);double Offset(string value,double available)=>value=="center"?available*.5d:value=="right"||value=="bottom"?available:value=="left"||value=="top"?0d:L(value,available);
                var repeat=Words(repeats[index%repeats.Count]);layers.Add(new Layer {Gradient=ParseGradient(sheet,node,image,w,h),Width=w,Height=h,X=ox+Offset(position.Length>0?position[0]:"0%",ow-w),Y=oy+Offset(position.Length>1?position[1]:"50%",oh-h),RepeatX=repeat[0]=="repeat"||repeat[0]=="repeat-x",RepeatY=repeat[0]=="repeat"||repeat[0]=="repeat-y",Clip=clips[index%clips.Count].Trim()});
            }
            bool Clipped(string clip,double x,double y){if(clip=="border-box")return false;var content=clip=="content-box";return x<borders[3]+(content?padding[3]:0d)||x>width-borders[1]-(content?padding[1]:0d)||y<borders[0]+(content?padding[0]:0d)||y>height-borders[2]-(content?padding[2]:0d);}
            var polygon=ParsePolygon(S("clip-path","none"),width,height,sheet);Gradient mask=S("mask-image","none")=="none"?null:ParseGradient(sheet,node,S("mask-image"),width,height);
            for(var index=0;index<count;index++) {
                var x=X(index);var y=Y(index);var shape=Shape(x,y);if(shape<=0d)continue;var color=Clipped(clips[(imageSources.Count-1)%clips.Count].Trim(),x,y)?new VisualCssColor(0,0,0,0):background;
                foreach(var layer in layers.AsEnumerable().Reverse()){if(Clipped(layer.Clip,x,y))continue;var px=x-layer.X;var py=y-layer.Y;if(layer.RepeatX)px=(px%layer.Width+layer.Width)%layer.Width;if(layer.RepeatY)py=(py%layer.Height+layer.Height)%layer.Height;if(px>=0d&&py>=0d&&px<layer.Width&&py<layer.Height)color=Over(color,layer.Gradient.At(px,py));}
                ownPixels[index]=color;shapes[index]=shape;
                var distances=new[]{y/Math.Max(.00001d,borders[0]),(width-x)/Math.Max(.00001d,borders[1]),(height-y)/Math.Max(.00001d,borders[2]),x/Math.Max(.00001d,borders[3])};var edge=Array.IndexOf(distances,distances.Min());
                var innerWidth=width-borders[1]-borders[3];var innerHeight=height-borders[0]-borders[2];var innerX=x-borders[3];var innerY=y-borders[0];
                var corner=innerY<innerHeight*.5d?(innerX<innerWidth*.5d?0:1):innerX<innerWidth*.5d?3:2;
                var rx=Math.Max(0d,corners[corner]-(corner==0||corner==3?borders[3]:borders[1]));var ry=Math.Max(0d,cornersY[corner]-(corner<2?borders[0]:borders[2]));
                var cx=corner==0||corner==3?rx:innerWidth-rx;var cy=corner<2?ry:innerHeight-ry;
                var innerDistance=Math.Max(Math.Max(-innerX,innerX-innerWidth),Math.Max(-innerY,innerY-innerHeight));
                if(rx>0d&&ry>0d&&(innerX<rx||innerX>innerWidth-rx)&&(innerY<ry||innerY>innerHeight-ry)){var dx=innerX-cx;var dy=innerY-cy;var normalized=Math.Sqrt(dx*dx/(rx*rx)+dy*dy/(ry*ry));var gradient=normalized>0d?Math.Sqrt(dx*dx/(rx*rx*rx*rx)+dy*dy/(ry*ry*ry*ry))/normalized:1d/Math.Min(rx,ry);innerDistance=(normalized-1d)/Math.Max(.00001d,gradient);}
                var inner=innerWidth<=0d||innerHeight<=0d?0d:Math.Max(0d,Math.Min(1d,.5d-innerDistance*scale));var borderCoverage=Math.Max(0d,shape-inner);innerShapes[index]=inner;
                if(borders[edge]>0d&&borderCoverage>0d){var border=borderColors[edge];var side=new[]{"top","right","bottom","left"}[edge];var dashed=S("border-"+side+"-style")=="dashed";var dashPosition=dashPath!=null?dashPath.Position(x,y):edge%2==0?x:y;var dashLength=dashPath!=null?Math.Floor(dashPath.Length):edge%2==0?width:height;var coverage=dashed?DashCoverage(dashPosition,dashLength,borders[edge],dashPath!=null,scale):1d;borderPixels[index]=new VisualCssColor(border.R,border.G,border.B,border.A*coverage*borderCoverage/Math.Max(.00001d,shape));}
            }
            foreach(var shadow in shadows.Where(value=>value.Inset).Reverse())DrawShadow(shadow);
            for(var index=0;index<count;index++){var own=Over(ownPixels[index],borderPixels[index]);pixels[index]=Over(pixels[index],new VisualCssColor(own.R,own.G,own.B,own.A*shapes[index]));}
            raster.Pixels=new byte[count*4];for(var i=0;i<count;i++){var c=pixels[i];var x=X(i);var y=Y(i);var alpha=c.A*(mask==null?1d:mask.At(x,y).A)*(polygon==null||Inside(polygon,x,y)?1d:0d);byte Byte(double value)=>(byte)Math.Max(0d,Math.Min(255d,Math.Round(value*255d)));raster.Pixels[i*4]=Byte(c.R);raster.Pixels[i*4+1]=Byte(c.G);raster.Pixels[i*4+2]=Byte(c.B);raster.Pixels[i*4+3]=Byte(alpha);}return raster;
        }
        public static void Blur(double[] values,int width,int height,double sigma,double outside=0d)
        {
            if(sigma<=0d)return;var radius=Math.Max(1,(int)Math.Ceiling(sigma*4d));var kernel=new double[radius*2+1];var sum=0d;for(var i=-radius;i<=radius;i++){var weight=Math.Exp(-i*(double)i/(2d*sigma*sigma));kernel[i+radius]=weight;sum+=weight;}for(var i=0;i<kernel.Length;i++)kernel[i]/=sum;
            var temporary=new double[values.Length];for(var y=0;y<height;y++)for(var x=0;x<width;x++){var value=0d;for(var dx=-radius;dx<=radius;dx++)value+=(x+dx<0||x+dx>=width?outside:values[y*width+x+dx])*kernel[dx+radius];temporary[y*width+x]=value;}
            for(var y=0;y<height;y++)for(var x=0;x<width;x++){var value=0d;for(var dy=-radius;dy<=radius;dy++)value+=(y+dy<0||y+dy>=height?outside:temporary[(y+dy)*width+x])*kernel[dy+radius];values[y*width+x]=value;}
        }
        public static Raster FilterBlur(Raster source,double sigma)
        {
            if(sigma<=0d)return source;var padding=(int)Math.Ceiling(sigma*source.Scale*4d);var width=source.Width+padding*2;var height=source.Height+padding*2;
            var channels=Enumerable.Range(0,4).Select(channel=>new double[width*height]).ToArray();
            for(var y=0;y<source.Height;y++)for(var x=0;x<source.Width;x++){var input=(y*source.Width+x)*4;var output=(y+padding)*width+x+padding;var alpha=source.Pixels[input+3]/255d;channels[3][output]=alpha;for(var color=0;color<3;color++)channels[color][output]=source.Pixels[input+color]/255d*alpha;}
            foreach(var channel in channels)Blur(channel,width,height,sigma*source.Scale);
            var pixels=new byte[width*height*4];for(var index=0;index<width*height;index++){var alpha=channels[3][index];for(var color=0;color<3;color++)pixels[index*4+color]=(byte)Math.Max(0d,Math.Min(255d,Math.Floor((alpha>0d?channels[color][index]/alpha:0d)*255d+.5d)));pixels[index*4+3]=(byte)Math.Max(0d,Math.Min(255d,Math.Floor(alpha*255d+.5d)));}
            return new Raster {Width=width,Height=height,Left=source.Left-padding/source.Scale,Top=source.Top-padding/source.Scale,Scale=source.Scale,Pixels=pixels};
        }
        private static double[][] ParsePolygon(string value,double width,double height,VisualCss sheet)=>value.StartsWith("polygon(",StringComparison.Ordinal)?VisualCss.Split(value.Substring(8,value.Length-9),',').Select(point=>{var parts=Words(point);return new[]{sheet.Length(parts[0],width),sheet.Length(parts[1],height)};}).ToArray():null;
        private static bool Inside(double[][] points,double x,double y){var inside=false;for(int i=0,j=points.Length-1;i<points.Length;j=i++){var a=points[i];var b=points[j];if((a[1]>y)!=(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])inside=!inside;}return inside;}
    }
}
