using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EchoSphere.Presentation
{
    // Scalar formatting and cosmetic randomness used by native Unity views.
    // This contains no emulated objects, callables, DOM or generated execution.
    public static class NativePresentationMath
    {
        private static readonly Random CosmeticRandom=new Random();
        internal static double NextCosmeticValue()=>CosmeticRandom.NextDouble();
        public static string NumberText(double number)
        {
            if(double.IsNaN(number))return "NaN";if(double.IsPositiveInfinity(number))return "Infinity";if(double.IsNegativeInfinity(number))return "-Infinity";if(number==0d)return "0";
            var text=number.ToString("R",CultureInfo.InvariantCulture);var index=text.IndexOf('E');if(index<0)return text;
            var power=int.Parse(text.Substring(index+1),CultureInfo.InvariantCulture);var sign=number<0?"-":"";var coefficient=text.Substring(sign.Length,index-sign.Length);
            if(Math.Abs(number)>=1e-6d&&Math.Abs(number)<1e21d)
            {
                var dot=coefficient.IndexOf('.');var position=(dot<0?coefficient.Length:dot)+power;var digits=coefficient.Replace(".","");
                return sign+(position<=0?"0."+new string('0',-position)+digits:position>=digits.Length?digits+new string('0',position-digits.Length):digits.Insert(position,"."));
            }
            return text.Substring(0,index)+"e"+(power>=0?"+":"")+power.ToString(CultureInfo.InvariantCulture);
        }
    }
#if UNITY_EDITOR
    // Editor-only value model for mechanically translated reference expressions.
    // The Android player does not compile these types or their callable objects.
    public readonly struct V : IEquatable<V>
    {
        public readonly object Raw;
        public V(object value) { Raw=value is float||value is int||value is long||value is short||value is byte||value is uint||value is ulong||value is decimal?Convert.ToDouble(value,CultureInfo.InvariantCulture):value; }
        public bool Undefined => Raw==null;
        public bool Null => Raw==VisualJs.NullToken;
        public double Number => Raw==null?double.NaN:Null?0d:Raw is double d?d:Raw is bool b?b?1d:0d:Raw is string s&&s.Trim().Length==0?0d:double.TryParse(Text,NumberStyles.Float,CultureInfo.InvariantCulture,out var n)?n:double.NaN;
        public string Text => Raw==null?"undefined":Null?"null":Raw is string s?s:Raw is bool b?b?"true":"false":Raw is double d?VisualJs.NumberText(d):Raw is List<V> a?string.Join(",",a.Select(x=>x.Null||x.Undefined?"":x.Text)):"[object Object]";
        public bool Truth => Raw!=null&&!Null&&(!(Raw is bool b)||b)&&(!(Raw is double d)||(d!=0d&&!double.IsNaN(d)))&&(!(Raw is string s)||s.Length!=0);
        public V this[V key]
        {
            get
            {
                if(Raw is IVisualHost host) return host.Get(key.Text);
                if(Raw is Dictionary<string,V> map) return map.TryGetValue(key.Text,out var value)?value:VisualJs.ObjectMethod(this,key.Text);
                if(Raw is List<V> list) return key.Text=="length"?new V((double)list.Count):VisualJs.IsIndex(key,out var i)?i<list.Count?list[i]:default:VisualJs.ArrayMethod(list,key.Text);
                if(Raw is string text) return key.Text=="length"?new V((double)text.Length):VisualJs.IsIndex(key,out var i)?i<text.Length?new V(text[i].ToString()):default:VisualJs.StringMethod(text,key.Text);
                if(Raw is double number) return VisualJs.NumberMethod(number,key.Text);
                if(Raw is VisualJs.KeyMap keys) return keys.Get(key.Text);
                return default;
            }
            set
            {
                if(Raw is IVisualHost host) { host.Set(key.Text,value); return; }
                if(Raw is Dictionary<string,V> map) { map[key.Text]=value; return; }
                if(Raw is List<V> list&&VisualJs.IsIndex(key,out var i)) { while(list.Count<=i) list.Add(default); list[i]=value; return; }
                throw new InvalidOperationException("Cannot set visual member "+key.Text);
            }
        }
        public V Call(params V[] args) => Raw is Func<V[],V> fn?fn(args):Raw is IVisualCallable callable?callable.Call(args):throw new InvalidOperationException("Visual expression is not callable: "+Text);
        public IEnumerable<V> Items => Raw is List<V> list?list:Raw is string text?text.Select(x=>new V(x.ToString())):Raw is VisualJs.KeyMap map?map.Keys:Array.Empty<V>();
        public bool Equals(V other) => Raw==null?other.Raw==null:Raw is double d?other.Raw is double n&&d==n:Raw.Equals(other.Raw);
        public override bool Equals(object other) => other is V v&&Equals(v);
        public override int GetHashCode() => Raw?.GetHashCode()??0;
        public static implicit operator V(double value)=>new V(value);
        public static implicit operator V(int value)=>new V((double)value);
        public static implicit operator V(string value)=>value==null?VisualJs.Null:new V(value);
        public static implicit operator V(bool value)=>new V(value);
        public static V operator +(V a,V b)=>a.Raw is string||b.Raw is string?new V(a.Text+b.Text):new V(a.Number+b.Number);
        public static V operator -(V a,V b)=>new V(a.Number-b.Number);
        public static V operator *(V a,V b)=>new V(a.Number*b.Number);
        public static V operator /(V a,V b)=>new V(a.Number/b.Number);
        public static V operator %(V a,V b)=>new V(a.Number%b.Number);
        public static V operator -(V a)=>new V(-a.Number);
        public static V operator +(V a)=>new V(a.Number);
        public static V operator !(V a)=>new V(!a.Truth);
        public static V operator ++(V a)=>new V(a.Number+1d);
        public static V operator --(V a)=>new V(a.Number-1d);
        public static bool operator ==(V a,V b)=>a.Equals(b);
        public static bool operator !=(V a,V b)=>!a.Equals(b);
        public static bool operator <(V a,V b)=>a.Number<b.Number;
        public static bool operator >(V a,V b)=>a.Number>b.Number;
        public static bool operator <=(V a,V b)=>a.Number<=b.Number;
        public static bool operator >=(V a,V b)=>a.Number>=b.Number;
        public static V operator &(V a,V b)=>new V((double)(VisualJs.Int32(a.Number)&VisualJs.Int32(b.Number)));
        public static V operator |(V a,V b)=>new V((double)(VisualJs.Int32(a.Number)|VisualJs.Int32(b.Number)));
        public static V operator ^(V a,V b)=>new V((double)(VisualJs.Int32(a.Number)^VisualJs.Int32(b.Number)));
        public static V operator >>(V a,int b)=>new V((double)(VisualJs.Int32(a.Number)>>(b&31)));
        public static V operator <<(V a,int b)=>new V((double)(VisualJs.Int32(a.Number)<<(b&31)));
        public override string ToString()=>Text;
    }
    public interface IVisualHost { V Get(string name); void Set(string name,V value); }
    public interface IVisualCallable {V Call(V[] args);}
    public static class VisualJs
    {
        public static readonly object NullToken=new object();
        public static readonly V Null=new V(NullToken);
        private static readonly System.Diagnostics.Stopwatch Clock=System.Diagnostics.Stopwatch.StartNew();
        internal static double NextCosmeticValue()=>NativePresentationMath.NextCosmeticValue();
        public static readonly V PerformanceObject=Obj("now",Fn(a=>Clock.Elapsed.TotalMilliseconds));
        public static readonly V ConsoleObject=Obj("info",Fn(a=>default));
        private static readonly Dictionary<string,SourceRegex> Regexes=new Dictionary<string,SourceRegex>();
        private sealed class SourceRegex {public System.Text.RegularExpressions.Regex Pattern;public bool Global;}
        public static V Regex(string literal){if(!Regexes.TryGetValue(literal,out var regex)){var end=literal.LastIndexOf('/');var flags=literal.Substring(end+1);regex=new SourceRegex {Global=flags.Contains("g"),Pattern=new System.Text.RegularExpressions.Regex(literal.Substring(1,end-1),System.Text.RegularExpressions.RegexOptions.ECMAScript|System.Text.RegularExpressions.RegexOptions.CultureInvariant|(flags.Contains("i")?System.Text.RegularExpressions.RegexOptions.IgnoreCase:0)|(flags.Contains("m")?System.Text.RegularExpressions.RegexOptions.Multiline:0))};Regexes.Add(literal,regex);}return new V(regex);}
        public static V Fn(Func<V[],V> fn)=>new V(fn);
        public static V Arg(V[] args,int index)=>index<args.Length?args[index]:default;
        public static V Obj(params V[] pairs) { var map=new Dictionary<string,V>(); for(var i=0;i<pairs.Length;i+=2) map[pairs[i].Text]=pairs[i+1]; return new V(map); }
        public static V Arr(params V[] values)=>new V(new List<V>(values));
        public static V Concat(params V[] lists)=>Arr(lists.SelectMany(x=>x.Items).ToArray());
        public static V Has(V target,V key)=>target.Raw is Dictionary<string,V> map&&map.ContainsKey(key.Text);
        public static readonly V ArrayObject=Obj("isArray",Fn(a=>Arg(a,0).Raw is List<V>),"from",Fn(a=>{var values=a[0].Raw is Dictionary<string,V> map&&map.ContainsKey("length")?Enumerable.Range(0,Math.Max(0,(int)map["length"].Number)).Select(i=>a[0][i]).ToArray():a[0].Items.ToArray();return a.Length>1?Arr(values.Select((x,i)=>a[1].Call(x,i)).ToArray()):Arr(values);}));
        public static Func<V> ImageFactory=()=>Obj("complete",false,"naturalWidth",0,"naturalHeight",0,"width",0,"height",0);
        public static V CreateImage()=>ImageFactory();
        public static V NewSet(V values) { var map=new KeyMap(false);foreach(var value in values.Items)map.Add(value);return new V(map); }
        public static V CreateMap(V values,bool weak=false) {var map=new KeyMap(weak);foreach(var pair in values.Items)map.Get("set").Call(pair[0],pair[1]);return new V(map);}
        public static V TypeOf(V value)=>value.Undefined?"undefined":value.Raw is string?"string":value.Raw is bool?"boolean":value.Raw is double?"number":value.Raw is Func<V[],V>||value.Raw is IVisualCallable?"function":"object";
        public static V Merge(params V[] values) { var result=Obj();foreach(var value in values)if(value.Raw is Dictionary<string,V> map)foreach(var pair in map)result[pair.Key]=pair.Value;return result; }
        public static V Or(V a,Func<V> b)=>a.Truth?a:b();
        public static V And(V a,Func<V> b)=>a.Truth?b():a;
        public static V Coalesce(V a,Func<V> b)=>a.Undefined||a.Null?b():a;
        public static V Optional(V a,V key)=>a.Undefined||a.Null?default:a[key];
        public static V Chain(V value,Func<V,V> continuation)=>value.Undefined||value.Null?default:continuation(value);
        public static V Assign(V target,V key,string op,V value)
        { if(op!="=") { var old=target[key];value=op=="+="?old+value:op=="-="?old-value:op=="*="?old*value:old/value; }target[key]=value;return value; }
        public static int Int32(double number)=>double.IsNaN(number)||double.IsInfinity(number)?0:unchecked((int)(long)Math.Truncate(number));
        internal static bool IsIndex(V key,out int index) { var n=key.Number; index=(int)n; return !double.IsNaN(n)&&n>=0&&n==index; }
        public static string NumberText(double number)=>NativePresentationMath.NumberText(number);
        public static V MathObject=Obj("PI",Math.PI,"E",Math.E,
            "sin",Fn(a=>Math.Sin(Arg(a,0).Number)),"cos",Fn(a=>Math.Cos(Arg(a,0).Number)),"tan",Fn(a=>Math.Tan(Arg(a,0).Number)),
            "atan2",Fn(a=>Math.Atan2(Arg(a,0).Number,Arg(a,1).Number)),"sqrt",Fn(a=>Math.Sqrt(Arg(a,0).Number)),"abs",Fn(a=>Math.Abs(Arg(a,0).Number)),
            "floor",Fn(a=>Math.Floor(Arg(a,0).Number)),"ceil",Fn(a=>Math.Ceiling(Arg(a,0).Number)),"round",Fn(a=>Math.Floor(Arg(a,0).Number+.5d)),
            "min",Fn(a=>a.Length==0?double.PositiveInfinity:a.Min(x=>x.Number)),"max",Fn(a=>a.Length==0?double.NegativeInfinity:a.Max(x=>x.Number)),
            "pow",Fn(a=>Math.Pow(Arg(a,0).Number,Arg(a,1).Number)),"exp",Fn(a=>Math.Exp(Arg(a,0).Number)),"hypot",Fn(a=>Math.Sqrt(a.Sum(x=>x.Number*x.Number))),"random",Fn(a=>NativePresentationMath.NextCosmeticValue()));
        public static V NumberObject=Obj("isFinite",Fn(a=>Arg(a,0).Raw is double d&&!double.IsInfinity(d)&&!double.IsNaN(d)),
            "isInteger",Fn(a=>Arg(a,0).Raw is double d&&!double.IsInfinity(d)&&!double.IsNaN(d)&&d==Math.Truncate(d)),
            "parseInt",Fn(a=> { try { return new V((double)Convert.ToInt64(Arg(a,0).Text,a.Length>1?(int)a[1].Number:10)); } catch { return new V(double.NaN); } }));
        public static V ObjectObject=Obj("entries",Fn(a=>Arr(((Dictionary<string,V>)a[0].Raw).Select(x=>Arr(x.Key,x.Value)).ToArray())),
            "fromEntries",Fn(a=>{var result=Obj();foreach(var pair in a[0].Items)result[pair[0]]=pair[1];return result;}),
            "keys",Fn(a=>Arr(((Dictionary<string,V>)a[0].Raw).Keys.Select(x=>new V(x)).ToArray())),"values",Fn(a=>Arr(((Dictionary<string,V>)a[0].Raw).Values.ToArray())),
            "freeze",Fn(a=>a[0]),"assign",Fn(a=>{for(var i=1;i<a.Length;i++)if(a[i].Raw is Dictionary<string,V> map)foreach(var pair in map)Assign(a[0],pair.Key,"=",pair.Value);return a[0];}));
        public static V Number(V value)=>new V(value.Number);
        internal static V NumberMethod(double number,string key)=>key=="toFixed"?Fn(a=>number.ToString("F"+(a.Length==0?0:(int)a[0].Number),CultureInfo.InvariantCulture)):key=="toString"?Fn(a=>a.Length>0&&(int)a[0].Number==16?Convert.ToString((long)number,16):NumberText(number)):default;
        internal static V StringMethod(string text,string key)
        {
            switch(key)
            {
                case "charCodeAt":return Fn(a=>{var index=a.Length==0?0:(int)a[0].Number;return index>=0&&index<text.Length?new V((double)text[index]):new V(double.NaN);});
                case "slice": case "substring": return Fn(a=> { var start=(int)Arg(a,0).Number; var end=a.Length>1?(int)a[1].Number:text.Length; if(key=="slice") { if(start<0)start+=text.Length; if(end<0)end+=text.Length; } start=Math.Max(0,Math.Min(text.Length,start)); end=Math.Max(start,Math.Min(text.Length,end)); return text.Substring(start,end-start); });
                case "split": return Fn(a=>Arr((a.Length==0?new[]{text}:a[0].Raw is SourceRegex regex?regex.Pattern.Split(text):a[0].Text.Length==0?text.Select(c=>c.ToString()).ToArray():text.Split(new[]{a[0].Text},StringSplitOptions.None)).Select(x=>new V(x)).ToArray()));
                case "replace":return Fn(a=> {if(a[0].Raw is SourceRegex regex)return regex.Pattern.Replace(text,a[1].Text,regex.Global?int.MaxValue:1);var start=text.IndexOf(a[0].Text,StringComparison.Ordinal);return start<0?text:text.Substring(0,start)+a[1].Text+text.Substring(start+a[0].Text.Length);});
                case "startsWith": return Fn(a=>text.StartsWith(a[0].Text,StringComparison.Ordinal));
                case "endsWith": return Fn(a=>text.EndsWith(a[0].Text,StringComparison.Ordinal));
                case "includes": return Fn(a=>text.Contains(a[0].Text));
                case "toLowerCase": return Fn(a=>text.ToLowerInvariant());
                case "toUpperCase": return Fn(a=>text.ToUpperInvariant());
                case "trim": return Fn(a=>text.Trim());
                case "padStart":return Fn(a=>{var length=(int)a[0].Number;var fill=a.Length>1?a[1].Text:" ";if(fill.Length==0||text.Length>=length)return text;var result="";while(result.Length<length-text.Length)result+=fill;return result.Substring(0,length-text.Length)+text;});
                default: return default;
            }
        }
        internal static V ArrayMethod(List<V> list,string key)
        {
            switch(key)
            {
                case "includes": return Fn(a=>list.Contains(a[0]));
                case "indexOf": return Fn(a=>list.IndexOf(a[0]));
                case "find": return Fn(a=> { for(var i=0;i<list.Count;i++)if(a[0].Call(list[i],i,new V(list)).Truth)return list[i]; return default; });
                case "findIndex":return Fn(a=>{for(var i=0;i<list.Count;i++)if(a[0].Call(list[i],i,new V(list)).Truth)return i;return -1;});
                case "reverse":return Fn(a=>{list.Reverse();return new V(list);});
                case "concat":return Fn(a=>{var result=new List<V>(list);foreach(var value in a)if(value.Raw is List<V> items)result.AddRange(items);else result.Add(value);return new V(result);});
                case "filter": return Fn(a=>Arr(list.Where((x,i)=>a[0].Call(x,i,new V(list)).Truth).ToArray()));
                case "map": return Fn(a=>Arr(list.Select((x,i)=>a[0].Call(x,i,new V(list))).ToArray()));
                case "flatMap": return Fn(a=>Arr(list.SelectMany((x,i)=>{var value=a[0].Call(x,i,new V(list));return value.Raw is List<V>?value.Items:new[]{value};}).ToArray()));
                case "forEach": return Fn(a=> { for(var i=0;i<list.Count;i++) a[0].Call(list[i],i,new V(list)); return default; });
                case "some": return Fn(a=>list.Where((x,i)=>a[0].Call(x,i,new V(list)).Truth).Any());
                case "every": return Fn(a=>list.Select((x,i)=>a[0].Call(x,i,new V(list)).Truth).All(x=>x));
                case "push": return Fn(a=> { list.AddRange(a); return list.Count; });
                case "pop": return Fn(a=> { if(list.Count==0)return default; var result=list[list.Count-1];list.RemoveAt(list.Count-1);return result; });
                case "shift":return Fn(a=>{if(list.Count==0)return default;var result=list[0];list.RemoveAt(0);return result;});
                case "reduce":return Fn(a=>{var result=a.Length>1?a[1]:list[0];for(var i=a.Length>1?0:1;i<list.Count;i++)result=a[0].Call(result,list[i],i,new V(list));return result;});
                case "sort":return Fn(a=>{var sorted=list.Select((x,i)=>new KeyValuePair<int,V>(i,x)).ToList();sorted.Sort((x,y)=>{var difference=a.Length>0?a[0].Call(x.Value,y.Value).Number:string.CompareOrdinal(x.Value.Text,y.Value.Text);var order=difference<0?-1:difference>0?1:0;return order==0?x.Key.CompareTo(y.Key):order;});list.Clear();foreach(var x in sorted)list.Add(x.Value);return new V(list);});
                case "join": return Fn(a=>string.Join(a.Length>0?a[0].Text:",",list.Select(x=>x.Text)));
                case "slice": return Fn(a=> { var first=a.Length>0?(int)a[0].Number:0; var end=a.Length>1?(int)a[1].Number:list.Count; if(first<0)first+=list.Count;if(end<0)end+=list.Count;first=Math.Max(0,Math.Min(list.Count,first));end=Math.Max(first,Math.Min(list.Count,end));return Arr(list.GetRange(first,end-first).ToArray()); });
                default:return default;
            }
        }
        internal static V ObjectMethod(V value,string key)=>default;
        public sealed class KeyMap : IVisualHost
        {
            private readonly Dictionary<V,V> _map=new Dictionary<V,V>();
            public IEnumerable<V> Keys=>_map.Keys;
            private sealed class Box {public V Value;}
            private readonly System.Runtime.CompilerServices.ConditionalWeakTable<object,Box> _weak;
            public KeyMap(bool weak=false) {if(weak)_weak=new System.Runtime.CompilerServices.ConditionalWeakTable<object,Box>();}
            public void Add(V key) {if(_weak==null)_map[key]=key;else { _weak.Remove(key.Raw);_weak.Add(key.Raw,new Box{Value=key}); }}
            public V Get(string key)
            {
                switch(key)
                {
                    case "get":return Fn(a=>_weak!=null?_weak.TryGetValue(a[0].Raw,out var box)?box.Value:default:_map.TryGetValue(a[0],out var value)?value:default);
                    case "set":return Fn(a=>{if(_weak==null)_map[a[0]]=a[1];else{_weak.Remove(a[0].Raw);_weak.Add(a[0].Raw,new Box{Value=a[1]});}return new V(this);});
                    case "has":return Fn(a=>_weak==null?_map.ContainsKey(a[0]):_weak.TryGetValue(a[0].Raw,out _));
                    case "delete":return Fn(a=>_weak==null?_map.Remove(a[0]):_weak.Remove(a[0].Raw));
                    case "clear":return Fn(a=>{if(_weak!=null)throw new InvalidOperationException("WeakMap has no clear method");_map.Clear();return default;});
                    case "add":return Fn(a=>{Add(a[0]);return new V(this);});
                    case "size":return _map.Count;
                    default:return default;
                }
            }
            public void Set(string name,V value)=>throw new InvalidOperationException("Cannot set map property");
        }
    }
#endif

}
