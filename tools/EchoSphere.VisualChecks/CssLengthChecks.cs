using EchoSphere.Presentation;
using System.Globalization;
using System.Text.RegularExpressions;

// Frozen pre-cache evaluator: compare behavior, not cache implementation.
internal static class CssLengthChecks
{
    private sealed class Legacy
    {
        public double Width,Height,SafeLeft,SafeRight,SafeTop,SafeBottom;
        public double Length(string value,double reference,double font=16d)
        {
            if(value==null||value=="auto"||value=="none"||value=="normal"||value=="initial"||value=="fit-content"||value=="min-content"||value=="max-content")return double.NaN;
            value=value.Trim();if(value.StartsWith("calc(",StringComparison.Ordinal))return Sum(value.Substring(5,value.Length-6),reference,font);
            foreach(var function in new[]{"min","max","clamp"})if(value.StartsWith(function+"(",StringComparison.Ordinal)){var args=VisualCss.Split(value.Substring(function.Length+1,value.Length-function.Length-2),',').Select(arg=>Length(arg.Trim(),reference,font)).ToArray();return function=="min"?args.Min():function=="max"?args.Max():Math.Max(args[0],Math.Min(args[2],args[1]));}
            if(value.StartsWith("env(",StringComparison.Ordinal)){var name=value.Substring(4,value.Length-5);return name=="safe-area-inset-left"?SafeLeft:name=="safe-area-inset-right"?SafeRight:name=="safe-area-inset-top"?SafeTop:SafeBottom;}
            var match=Regex.Match(value,@"^([-+]?(?:\d*\.\d+|\d+\.?\d*)(?:[eE][-+]?\d+)?)(px|rem|em|vw|vh|dvh|svh|lvh|vmin|vmax|%)?$");if(!match.Success)return double.NaN;
            var number=double.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture);switch(match.Groups[2].Value){case "%":return number*reference*.01d;case "rem":return number*16d;case "em":return number*font;case "vw":return number*Width*.01d;case "vh":case "dvh":case "svh":case "lvh":return number*Height*.01d;case "vmin":return number*Math.Min(Width,Height)*.01d;case "vmax":return number*Math.Max(Width,Height)*.01d;default:return number;}
        }
        private double Sum(string value,double reference,double font)
        {
            var depth=0;for(var i=value.Length-1;i>0;i--){if(value[i]==')')depth++;if(value[i]=='(')depth--;if(depth==0&&(value[i]=='+'||value[i]=='-')&&char.IsWhiteSpace(value[i-1]))return value[i]=='+'?Sum(value.Substring(0,i),reference,font)+Sum(value.Substring(i+1),reference,font):Sum(value.Substring(0,i),reference,font)-Sum(value.Substring(i+1),reference,font);}
            depth=0;for(var i=value.Length-1;i>0;i--){if(value[i]==')')depth++;if(value[i]=='(')depth--;if(depth==0&&(value[i]=='*'||value[i]=='/'))return value[i]=='*'?Sum(value.Substring(0,i),reference,font)*Sum(value.Substring(i+1),reference,font):Sum(value.Substring(0,i),reference,font)/Sum(value.Substring(i+1),reference,font);}
            return Length(value.Trim(),reference,font);
        }

    }
    public static void Run()
    {
        var sheet=new VisualCss(VisualJson.Parse("{\"rules\":[],\"keyframes\":{}}"));
        var old=new Legacy();int checks=0;
        string[] units={"","px","%","rem","em","vw","vh","dvh","svh","lvh","vmin","vmax"};
        string[] specials={null,"auto","none","normal","initial","fit-content","min-content","max-content"," auto ","garbage","1PX","1e999px","calc(100% - 50px)","calc(2em * 3 + 10vw)","min(520px,88vw)","max(2em,50%)","clamp(12px,3vw,40px)","env(safe-area-inset-left)","env(safe-area-inset-right)","env(safe-area-inset-top)","env(safe-area-inset-bottom)"};
        void Check(string value,double reference,double font){
            var a=old.Length(value,reference,font);var b=sheet.Length(value,reference,font);
            if(!a.Equals(b))throw new Exception("CSS length mismatch: "+value+" "+a+" != "+b);checks++;
        }
        for(int pass=0;pass<5;pass++){
            sheet.Width=old.Width=pass%2==0?480:1260;sheet.Height=old.Height=pass%2==0?854:542;
            sheet.SafeLeft=old.SafeLeft=pass*2;sheet.SafeRight=old.SafeRight=pass*3;sheet.SafeTop=old.SafeTop=pass*4;sheet.SafeBottom=old.SafeBottom=pass*5;
            foreach(var reference in new[]{0d,123.75d,double.NaN,double.PositiveInfinity})foreach(var font in new[]{12d,27.5d}){
                foreach(var value in specials)Check(value,reference,font);
                foreach(var unit in units)foreach(var number in new[]{"0","-0","+.5","-21.75","1e-3","3E+2"})Check(" "+number+unit+" ",reference,font);
            }
        }
        // More distinct animated literals than the retention bound; then reuse
        // old values with changed live references to catch stale evaluated sizes.
        for(int i=0;i<5000;i++)Check(i.ToString(CultureInfo.InvariantCulture)+".25vw",311,19);
        sheet.Width=old.Width=720;foreach(var value in specials)Check(value,411,31);
        Console.WriteLine("CSS length syntax cache: PASS ("+checks+" legacy comparisons; live viewport/font/safe-area changes and eviction).");
    }
}
