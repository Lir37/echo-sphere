using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    // CSS keyframes retain the authored offsets, easing, duration, iteration,
    // direction and !important precedence. Time is supplied by the native host.
    public sealed class VisualCssAnimation
    {
        private sealed class Motion {public string Property,From,To,Timing,ReversingStart;public double Start,Duration,Delay,Shortening=1d;}
        private readonly Dictionary<string,string> _previous=new Dictionary<string,string>();
        private readonly List<Motion> _transitions=new List<Motion>();
        private string _animation;
        private double _started;
        private static readonly Regex Tokens=new Regex(@"#[a-fA-F0-9]{3,8}\b|rgba?\([^)]*\)|color\([^)]*\)|[-+]?(?:\d*\.\d+|\d+\.?\d*)(?:e[-+]?\d+)?(?:px|%|deg|rad|turn|em|rem|vw|vh)?",RegexOptions.CultureInvariant);
        public void Apply(VisualCss sheet,VisualCssNode node,double now)
        {
            var style=node.Style;var transition=sheet.Get(node,"transition","none");
            // Computed initial values also participate when a pseudo class adds
            // a property for the first time (or removes it on pointer leave).
            foreach(var property in new[]{"transform","filter","box-shadow","text-shadow"})if(!style.ContainsKey(property))style[property]="none";
            if(!style.ContainsKey("opacity"))style["opacity"]="1";
            if(!style.ContainsKey("background-color"))style["background-color"]="transparent";
            foreach(var property in new[]{"box-shadow","text-shadow"})if(style[property]!="none")style[property]=Shadow(style[property],property=="box-shadow",sheet.Get(node,"color","#000000"));
            foreach(var declaration in VisualCss.Split(transition,',')) {
                var words=Words(declaration);var property=words.FirstOrDefault(word=>!IsTime(word)&&!IsTiming(word))??"all";
                var times=words.Where(IsTime).Select(Seconds).ToArray();var duration=times.Length>0?times[0]:0d;var delay=times.Length>1?times[1]:0d;var timing=words.FirstOrDefault(IsTiming)??"ease";
                if(duration<=0d||property=="none")continue;
                foreach(var pair in style.ToArray())if((property=="all"||property==pair.Key||property=="background"&&pair.Key.StartsWith("background-",StringComparison.Ordinal)||property=="border-color"&&pair.Key.StartsWith("border-",StringComparison.Ordinal)&&pair.Key.EndsWith("-color",StringComparison.Ordinal))&&_previous.TryGetValue(pair.Key,out var before)&&before!=pair.Value) {
                    var active=_transitions.LastOrDefault(motion=>motion.Property==pair.Key);var from=active==null?before:Sample(active,now);
                    var shortening=1d;var reversingStart=from;
                    if(active!=null&&active.ReversingStart==pair.Value){var progress=Math.Max(0d,Math.Min(1d,(now-active.Start-active.Delay)/active.Duration));shortening=Math.Min(1d,Math.Abs(Ease(active.Timing,progress)*active.Shortening+1d-active.Shortening));reversingStart=active.To;}
                    _transitions.RemoveAll(motion=>motion.Property==pair.Key);_transitions.Add(new Motion {Property=pair.Key,From=from,To=pair.Value,ReversingStart=reversingStart,Shortening=shortening,Start=now,Duration=duration*shortening,Delay=delay<0d?delay*shortening:delay,Timing=timing});
                }
            }
            _previous.Clear();foreach(var pair in style)_previous[pair.Key]=pair.Value;
            var animation=sheet.Get(node,"animation","none");
            var signature=animation+"|"+sheet.Get(node,"animation-duration")+"|"+sheet.Get(node,"animation-direction")+"|"+sheet.Get(node,"animation-delay");
            if(signature!=_animation){_animation=signature;_started=now;}
            var animations=VisualCss.Split(animation,',');
            for(var index=0;index<animations.Count;index++) {
                var words=Words(animations[index]);var name=words.FirstOrDefault(word=>!sheet.Keyframes[word].Undefined);
                if(name==null)continue;
                var times=words.Where(IsTime).Select(Seconds).ToArray();
                string Override(string property,string fallback){var entries=VisualCss.Split(sheet.Get(node,property,fallback),',');return entries[index%entries.Count].Trim();}
                var duration=Seconds(Override("animation-duration",times.Length>0?VisualJs.NumberText(times[0])+"s":"0s"));
                var delay=Seconds(Override("animation-delay",times.Length>1?VisualJs.NumberText(times[1])+"s":"0s"));
                var timing=Override("animation-timing-function",words.FirstOrDefault(IsTiming)??"ease");
                var direction=Override("animation-direction",words.FirstOrDefault(word=>new[]{"reverse","alternate","alternate-reverse","normal"}.Contains(word))??"normal");
                var fill=Override("animation-fill-mode",words.FirstOrDefault(word=>new[]{"both","backwards","forwards"}.Contains(word))??"none");
                var iterationsText=Override("animation-iteration-count",words.FirstOrDefault(word=>word=="infinite"||double.TryParse(word,NumberStyles.Float,CultureInfo.InvariantCulture,out _))??"1");
                var iterations=iterationsText=="infinite"?double.PositiveInfinity:Parse(iterationsText);var elapsed=now-_started-delay;
                if(elapsed<0d&&fill!="backwards"&&fill!="both")continue;
                var finished=duration<=0d||elapsed>=duration*iterations;
                if(finished&&fill!="forwards"&&fill!="both")continue;
                var iteration=duration<=0?0d:Math.Max(0d,Math.Floor(elapsed/duration));
                var fraction=duration<=0d?1d:Math.Max(0d,elapsed/duration-iteration);
                if(finished){iteration=Math.Max(0d,Math.Ceiling(iterations)-1d);fraction=iterations-Math.Floor(iterations);if(fraction==0d)fraction=1d;}
                if(direction=="reverse"||direction=="alternate"&&iteration%2d==1d||direction=="alternate-reverse"&&iteration%2d==0d)fraction=1d-fraction;
                var tracks=new Dictionary<string,SortedDictionary<double,string>>();
                foreach(var frame in sheet.Keyframes[name].Items)foreach(var offset in VisualCss.Split(frame["offset"].Text,',')) {
                    var at=offset.Trim()=="from"?0d:offset.Trim()=="to"?1d:Parse(offset.Trim().TrimEnd('%'))*.01d;
                    foreach(var declaration in frame["declarations"].Items){var property=declaration["property"].Text;if(node.Important.Contains(property))continue;if(!tracks.TryGetValue(property,out var track)){track=new SortedDictionary<double,string>();tracks[property]=track;}track[at]=sheet.Variables(declaration["value"].Text,style);}
                }
                foreach(var pair in tracks) {
                    var track=pair.Value;var underlying=sheet.Get(node,pair.Key,pair.Key=="opacity"?"1":"none");if(!track.ContainsKey(0d))track[0d]=underlying;if(!track.ContainsKey(1d))track[1d]=underlying;
                    var before=track.Last(entry=>entry.Key<=fraction);var after=track.First(entry=>entry.Key>=fraction);
                    var progress=after.Key==before.Key?0d:(fraction-before.Key)/(after.Key-before.Key);
                    style[pair.Key]=Interpolate(before.Value,after.Value,Ease(timing,progress),pair.Key);
                }
            }
            // CSS transitions take precedence over animations, including important
            // declarations, for the duration of an active transition.
            foreach(var motion in _transitions.ToArray()){style[motion.Property]=Sample(motion,now);if(now>=motion.Start+motion.Delay+motion.Duration)_transitions.Remove(motion);}
        }
        private static string Sample(Motion motion,double now)=>motion.Duration<=0d?motion.To:Interpolate(motion.From,motion.To,Ease(motion.Timing,Math.Max(0d,Math.Min(1d,(now-motion.Start-motion.Delay)/motion.Duration))),motion.Property);
        private static string[] Words(string value)=>VisualCss.Split(value,' ').Where(word=>word.Length>0).ToArray();
        private static bool IsTime(string value)=>Regex.IsMatch(value,@"^[-\d.]+m?s$");
        private static bool IsTiming(string value)=>new[]{"linear","ease","ease-in","ease-out","ease-in-out","step-start","step-end"}.Contains(value)||value.StartsWith("cubic-bezier(",StringComparison.Ordinal)||value.StartsWith("steps(",StringComparison.Ordinal);
        private static double Parse(string value)=>double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var result)?result:0d;
        private static double Seconds(string value)=>value.EndsWith("ms",StringComparison.Ordinal)?Parse(value.Substring(0,value.Length-2))*.001d:Parse(value.TrimEnd('s'));
        public static double Ease(string timing,double t)
        {
            if(t<=0d||t>=1d||timing=="linear")return t;
            if(timing=="step-start")return 1d;if(timing=="step-end")return 0d;
            if(timing.StartsWith("steps(",StringComparison.Ordinal)){var parts=VisualCss.Split(timing.Substring(6,timing.Length-7),',');var count=Math.Max(1d,Parse(parts[0]));return Math.Min(1d,(Math.Floor(t*count)+(parts.Count>1&&parts[1].Trim()=="start"?1d:0d))/count);}
            var p=timing=="ease-in"?new[]{.42d,0d,1d,1d}:timing=="ease-out"?new[]{0d,0d,.58d,1d}:timing=="ease-in-out"?new[]{.42d,0d,.58d,1d}:new[]{.25d,.1d,.25d,1d};
            if(timing.StartsWith("cubic-bezier(",StringComparison.Ordinal))p=VisualCss.Split(timing.Substring(13,timing.Length-14),',').Select(Parse).ToArray();
            double Curve(double x,double a,double b)=>3d*(1d-x)*(1d-x)*x*a+3d*(1d-x)*x*x*b+x*x*x;
            var low=0d;var high=1d;for(var i=0;i<40;i++){var middle=(low+high)*.5d;if(Curve(middle,p[0],p[2])<t)low=middle;else high=middle;}return Curve((low+high)*.5d,p[1],p[3]);
        }
        public static string Interpolate(string from,string to,double t,string property="")
        {
            if(from==to||t<=0d)return from;if(t>=1d)return to;
            if(property.EndsWith("color",StringComparison.Ordinal))return VisualCssColor.Lerp(VisualCssColor.Parse(from),VisualCssColor.Parse(to),t).Canvas;
            if(property=="box-shadow"||property=="text-shadow"){
                var left=from=="none"?new List<string>():VisualCss.Split(from,',');var right=to=="none"?new List<string>():VisualCss.Split(to,',');
                string Empty(string shadow)=>Regex.Replace(Tokens.Replace(shadow,match=>match.Value.StartsWith("#",StringComparison.Ordinal)||match.Value.Contains("(")?"rgba(0,0,0,0)":"0px"),@"\b(?:transparent|black|white|currentColor)\b","rgba(0,0,0,0)");
                while(left.Count<right.Count)left.Add(Empty(right[left.Count]));while(right.Count<left.Count)right.Add(Empty(left[right.Count]));
                if(Enumerable.Range(0,left.Count).Any(i=>left[i].Contains("inset")!=right[i].Contains("inset")))return t<.5d?from:to;
                return string.Join(", ",Enumerable.Range(0,left.Count).Select(i=>Interpolate(left[i].Trim(),right[i].Trim(),t)));
            }
            if(from=="none")from=Identity(to);if(to=="none")to=Identity(from);
            // CSS pads a shorter compatible transform list with identity
            // functions. The source often adds rotateZ(360deg) only at `to`.
            var fromFunctions=Functions(from);var toFunctions=Functions(to);
            var common=Math.Min(fromFunctions.Length,toFunctions.Length);
            if(common>0&&Enumerable.Range(0,common).All(i=>fromFunctions[i].Name==toFunctions[i].Name)){
                if(fromFunctions.Length<toFunctions.Length)from+=" "+string.Join(" ",toFunctions.Skip(common).Select(item=>Identity(item.Value)));
                else if(toFunctions.Length<fromFunctions.Length)to+=" "+string.Join(" ",fromFunctions.Skip(common).Select(item=>Identity(item.Value)));
            }
            var a=Tokens.Matches(from);var b=Tokens.Matches(to);if(a.Count!=b.Count)return t<.5d?from:to;
            var result="";var cursor=0;
            for(var i=0;i<a.Count;i++) {
                var first=a[i];var second=b[i];var prefixA=from.Substring(cursor,first.Index-cursor);var previousB=i==0?0:b[i-1].Index+b[i-1].Length;
                if(prefixA!=to.Substring(previousB,second.Index-previousB))return t<.5d?from:to;
                result+=prefixA;
                if(first.Value.StartsWith("#",StringComparison.Ordinal)||first.Value.StartsWith("rgb",StringComparison.Ordinal)||first.Value.StartsWith("color(",StringComparison.Ordinal))result+=VisualCssColor.Lerp(VisualCssColor.Parse(first.Value),VisualCssColor.Parse(second.Value),t).Canvas;
                else {var x=Regex.Match(first.Value,@"^([-+\d.e]+)(.*)$");var y=Regex.Match(second.Value,@"^([-+\d.e]+)(.*)$");if(x.Groups[2].Value!=y.Groups[2].Value&&Parse(x.Groups[1].Value)!=0d&&Parse(y.Groups[1].Value)!=0d)return t<.5d?from:to;result+=VisualJs.NumberText(Parse(x.Groups[1].Value)+(Parse(y.Groups[1].Value)-Parse(x.Groups[1].Value))*t)+(y.Groups[2].Value.Length>0?y.Groups[2].Value:x.Groups[2].Value);}
                cursor=first.Index+first.Length;
            }
            return result+from.Substring(cursor);
        }
        private static (string Name,string Value)[] Functions(string value)=>Regex.Matches(value,@"([A-Za-z0-9-]+)\(([^()]*)\)").Select(match=>(match.Groups[1].Value,match.Value)).ToArray();
        private static string Shadow(string value,bool box,string current)
        {
            return string.Join(", ",VisualCss.Split(value,',').Select(part=>{
                var words=VisualCss.Split(part.Trim(),' ').Where(word=>word.Length>0).ToArray();var lengths=words.Where(word=>Regex.IsMatch(word,@"^[-+\d.]")).Select(word=>word=="0"?"0px":word).ToList();
                while(lengths.Count<(box?4:3))lengths.Add("0px");var color=words.FirstOrDefault(word=>word!="inset"&&!Regex.IsMatch(word,@"^[-+\d.]"))??current;
                return VisualCssColor.Parse(color,current).Canvas+" "+string.Join(" ",lengths)+(words.Contains("inset")?" inset":"");
            }));
        }
        private static string Identity(string value)=>string.Join(" ",VisualCss.Split(value,' ').Where(part=>part.Length>0).Select(part=>IdentityFunction(part)));
        private static string IdentityFunction(string value)
        {
            if(value.StartsWith("drop-shadow(",StringComparison.Ordinal))return "drop-shadow("+Tokens.Replace(value.Substring(12,value.Length-13),match=>match.Value.StartsWith("#",StringComparison.Ordinal)||match.Value.Contains("(")?"rgba(0,0,0,0)":"0px")+")";
            return Regex.Replace(value,@"([A-Za-z0-9-]+)\(([^()]*)\)",match=> {
            var function=match.Groups[1].Value;var identity=function.StartsWith("scale",StringComparison.Ordinal)||function=="brightness"||function=="contrast"||function=="saturate"?"1":"0";
            return function+"("+Regex.Replace(match.Groups[2].Value,@"[-+]?(?:\d*\.\d+|\d+\.?\d*)",identity)+")";
            });
        }
    }
}
