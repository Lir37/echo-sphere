using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    public sealed class VisualCssNode
    {
        public string Tag,Text,Pseudo;
        public V Properties=VisualJs.Obj();
        public VisualCssNode Parent;
        public readonly List<VisualCssNode> Children=new List<VisualCssNode>();
        public readonly Dictionary<string,string> Style=new Dictionary<string,string>();
        public readonly HashSet<string> Important=new HashSet<string>();
        public readonly HashSet<string> InheritedProperties=new HashSet<string>();
        public void InheritAnimatedValues()
        {
            if(Parent==null)return;
            foreach(var property in InheritedProperties)if(Parent.Style.TryGetValue(property,out var value))Style[property]=value;
        }
        public bool Hover,Active,Focused;
        public object Native;
        public double X,Y,Width,Height;
        private string _classText;
        private HashSet<string> _classes;
        internal bool HasClass(string value){var text=Attribute("className")??"";if(text!=_classText){_classText=text;_classes=new HashSet<string>(text.Split((char[])null,StringSplitOptions.RemoveEmptyEntries));}return _classes.Contains(value);}
        internal IEnumerable<string> Classes {get{HasClass("");return _classes;}}
        public string Attribute(string name) {var value=Properties[name=="class"?"className":name];if(value.Undefined)return null;if(value.Raw is bool flag&&!flag&&new[]{"disabled","checked","hidden","multiple","readonly","required","selected","autofocus"}.Contains(name))return null;return value.Text;}
        public int ElementIndex=>Parent==null?0:Parent.Children.Where(child=>child.Tag!="#text"&&child.Pseudo==null).ToList().IndexOf(this);
        public static VisualCssNode Build(V tree)
        {
            var root=new VisualCssNode {Tag="html"};var body=new VisualCssNode {Tag="body",Parent=root};root.Children.Add(body);
            var appRoot=new VisualCssNode {Tag="div",Properties=VisualJs.Obj("id","root"),Parent=body};body.Children.Add(appRoot);
            var app=new VisualCssNode {Tag="div",Properties=VisualJs.Obj("className","es-app min-h-screen w-full text-[#dcecff] overflow-hidden flex items-center justify-center"),Parent=appRoot};appRoot.Children.Add(app);
            Append(tree,app);return root;
        }
        internal static void Append(V value,VisualCssNode parent)
        {
            if(value.Undefined||value.Null||value.Raw is bool)return;
            if(value.Raw is List<V>){foreach(var child in value.Items)Append(child,parent);return;}
            if(value.Raw is VisualDom.Node element) {
                if(element.Tag=="fragment"){foreach(var child in element.Children)Append(child,parent);return;}
                var node=new VisualCssNode {Tag=element.Tag,Properties=element.Properties,Parent=parent};parent.Children.Add(node);
                foreach(var child in element.Children)Append(child,node);return;
            }
            if(value.Text.Length>0)parent.Children.Add(new VisualCssNode {Tag="#text",Text=value.Text,Parent=parent});
        }
    }
    public sealed class VisualCss
    {
        private sealed class Rule {public string Selector;public V Conditions,Declarations;public int Specificity,Order;public bool Enabled;}
        private sealed class ParsedSelector {public List<string> Parts;public List<char> Relations;}
        private static readonly Dictionary<string,ParsedSelector> SelectorCache=new Dictionary<string,ParsedSelector>();
        private readonly List<Rule> _rules=new List<Rule>();
        private readonly List<Rule> _universalRules=new List<Rule>();
        private readonly Dictionary<string,List<Rule>> _classRules=new Dictionary<string,List<Rule>>(),_tagRules=new Dictionary<string,List<Rule>>();
        public readonly V Keyframes;
        public double Width,Height,SafeLeft,SafeRight,SafeTop,SafeBottom;
        private double _conditionsWidth=double.NaN,_conditionsHeight=double.NaN;
        private static readonly HashSet<string> Inherited=new HashSet<string>{"color","font-family","font-size","font-weight","font-style","line-height","letter-spacing","word-spacing","text-align","text-transform","text-shadow","white-space","visibility","cursor","pointer-events","user-select","overflow-wrap","font-feature-settings","font-variation-settings"};
        public VisualCss(V data)
        {
            Keyframes=data["keyframes"];var order=0;
            foreach(var entry in data["rules"].Items)foreach(var selector in Split(entry["selector"].Text,',')){
                var rule=new Rule {Selector=selector.Trim(),Conditions=entry["conditions"],Declarations=entry["declarations"],Specificity=Specificity(selector),Order=order++};_rules.Add(rule);
                var plain=rule.Selector.Split(new[]{"::"},StringSplitOptions.None)[0];if(plain.Length==0){_universalRules.Add(rule);continue;}
                Match(new VisualCssNode {Tag="__index__"},plain);var compound=SelectorCache[plain].Parts.Last();var index=0;
                if(compound.StartsWith(".",StringComparison.Ordinal)){index=1;var name=Identifier(compound,ref index);if(!_classRules.TryGetValue(name,out var candidates)){candidates=new List<Rule>();_classRules.Add(name,candidates);}candidates.Add(rule);}
                else if(char.IsLetter(compound[0])){var tag=Identifier(compound,ref index);if(!_tagRules.TryGetValue(tag,out var candidates)){candidates=new List<Rule>();_tagRules.Add(tag,candidates);}candidates.Add(rule);}
                else _universalRules.Add(rule);
            }
        }
        public void Apply(VisualCssNode root)
        {
            if(_conditionsWidth!=Width||_conditionsHeight!=Height){foreach(var rule in _rules)rule.Enabled=Conditions(rule.Conditions);_conditionsWidth=Width;_conditionsHeight=Height;}
            ApplyNode(root);
        }
        private void ApplyNode(VisualCssNode node)
        {
            node.Style.Clear();
            node.Important.Clear();
            node.InheritedProperties.Clear();
            if(node.Parent!=null){foreach(var pair in node.Parent.Style)if(pair.Key.StartsWith("--",StringComparison.Ordinal)||Inherited.Contains(pair.Key)){node.Style[pair.Key]=pair.Value;node.InheritedProperties.Add(pair.Key);}}
            else {node.Style["font-size"]="16px";node.Style["font-family"]="system-ui,sans-serif";node.Style["line-height"]="normal";node.Style["color"]="#000000";}
            if(node.Tag=="button"&&node.Pseudo==null){node.Style["text-align"]="center";node.InheritedProperties.Remove("text-align");}
            node.Style["display"]=node.Pseudo!=null||new[]{"#text","span","b","small","i","strong","label","path","circle","ellipse","rect","line","polygon","polyline","g","defs","linearGradient","radialGradient","stop"}.Contains(node.Tag)?"inline":node.Tag=="svg"||node.Tag=="input"||node.Tag=="button"?"inline-block":"block";
            var owner=node.Pseudo==null?node:node.Parent;var candidates=new List<Rule>(_universalRules);if(_tagRules.TryGetValue(owner.Tag,out var tagRules))candidates.AddRange(tagRules);foreach(var name in owner.Classes)if(_classRules.TryGetValue(name,out var classRules))candidates.AddRange(classRules);
            var matches=candidates.Where(rule=>rule.Enabled&&Match(node,rule.Selector)).ToList();
            var declarations=new List<Tuple<int,int,int,string,string>>();
            foreach(var rule in matches)foreach(var d in rule.Declarations.Items)declarations.Add(Tuple.Create(d["important"].Truth?1:0,rule.Specificity,rule.Order,d["property"].Text,d["value"].Text));
            if(node.Properties["style"].Raw is Dictionary<string,V> inline)foreach(var pair in inline){var name=Regex.Replace(pair.Key,"[A-Z]",m=>"-"+m.Value.ToLowerInvariant());var value=pair.Value.Raw is double&&!name.StartsWith("--",StringComparison.Ordinal)&&name!="opacity"&&name!="font-weight"&&name!="z-index"&&name!="flex"&&name!="flex-grow"&&name!="flex-shrink"&&name!="order"&&name!="line-height"?pair.Value.Text+"px":pair.Value.Text;declarations.Add(Tuple.Create(0,100000,int.MaxValue,name,value));}
            declarations=declarations.OrderBy(d=>d.Item1).ThenBy(d=>d.Item2).ThenBy(d=>d.Item3).ToList();
            foreach(var d in declarations)if(d.Item4.StartsWith("--",StringComparison.Ordinal)){node.Style[d.Item4]=d.Item5;node.InheritedProperties.Remove(d.Item4);}
            foreach(var d in declarations)if(!d.Item4.StartsWith("--",StringComparison.Ordinal)){var expanded=new Dictionary<string,string>();Expand(expanded,d.Item4,Variables(d.Item5,node.Style));foreach(var property in expanded){node.Style[property.Key]=property.Value;if(property.Value=="inherit")node.InheritedProperties.Add(property.Key);else node.InheritedProperties.Remove(property.Key);}if(d.Item1==1){node.Important.Add(d.Item4);foreach(var property in expanded.Keys)node.Important.Add(property);}}
            foreach(var key in node.Style.Keys.ToArray())if(node.Style[key]=="inherit")node.Style[key]=node.Parent!=null&&node.Parent.Style.TryGetValue(key,out var inherited)?inherited:"initial";
            var parentFont=node.Parent==null?16d:Length(Get(node.Parent,"font-size","16px"),16d);var font=Length(Get(node,"font-size","16px"),parentFont,parentFont);if(double.IsNaN(font))font=16d;
            node.Style["font-size"]=VisualJs.NumberText(font)+"px";
            var spacing=Get(node,"letter-spacing","normal");if(spacing!="normal")node.Style["letter-spacing"]=VisualJs.NumberText(Length(spacing,font,font))+"px";
            var height=Get(node,"line-height","normal");if(height!="normal"&&!Regex.IsMatch(height,@"^[\d.]+$"))node.Style["line-height"]=VisualJs.NumberText(Length(height,font,font))+"px";
            var weight=Get(node,"font-weight","normal");var parentWeight=node.Parent!=null&&double.TryParse(Get(node.Parent,"font-weight","400"),out var numericWeight)?numericWeight:400d;
            node.Style["font-weight"]=weight=="normal"?"400":weight=="bold"?"700":weight=="bolder"?parentWeight<350?"400":parentWeight<550?"700":"900":weight=="lighter"?parentWeight<550?"100":parentWeight<750?"400":"700":weight;
            if(Get(node,"position")=="absolute"||Get(node,"position")=="fixed"||node.Parent!=null&&new[]{"flex","inline-flex","grid","inline-grid"}.Contains(Get(node.Parent,"display"))){if(node.Style["display"]=="inline"||node.Style["display"]=="inline-block")node.Style["display"]="block";else if(node.Style["display"]=="inline-flex")node.Style["display"]="flex";else if(node.Style["display"]=="inline-grid")node.Style["display"]="grid";}
            foreach(var child in node.Children.ToArray())ApplyNode(child);
            if(node.Pseudo==null&&node.Tag!="#text"&&!new[]{"svg","path","circle","rect","line","g","input","canvas"}.Contains(node.Tag))foreach(var pseudo in new[]{"::before","::after"}) {
                var candidate=node.Children.FirstOrDefault(child=>child.Pseudo==pseudo)??new VisualCssNode {Tag=node.Tag,Pseudo=pseudo,Parent=node};
                ApplyNode(candidate);
                if(candidate.Style.TryGetValue("content",out var content)&&content!="none"&&content!="normal"){
                    candidate.Text=content.Trim('"','\'');if(!node.Children.Contains(candidate)){if(pseudo=="::before")node.Children.Insert(0,candidate);else node.Children.Add(candidate);}
                }else node.Children.Remove(candidate);
            }
        }
        public string Get(VisualCssNode node,string name,string fallback="")=>node.Style.TryGetValue(name,out var value)?value:fallback;
        public string Variables(string text,Dictionary<string,string> values)
        {
            for(var attempt=0;attempt<32;attempt++) {
                var start=text.LastIndexOf("var(",StringComparison.Ordinal);if(start<0)break;var end=Close(text,start+3);if(end<0)break;
                var parts=Split(text.Substring(start+4,end-start-4),',');var key=parts[0].Trim();var replacement=values.TryGetValue(key,out var value)?value:parts.Count>1?string.Join(",",parts.Skip(1)).Trim():"";
                text=text.Substring(0,start)+replacement+text.Substring(end+1);
            }
            return text;
        }
        private bool Conditions(V conditions)
        {
            foreach(var condition in conditions.Items) {
                if(condition["kind"].Text!="media")continue;
                foreach(Match match in Regex.Matches(condition["value"].Text,@"\((min|max)-(width|height)\s*:\s*([\d.]+)px\)")) {
                    var limit=double.Parse(match.Groups[3].Value,CultureInfo.InvariantCulture);var current=match.Groups[2].Value=="width"?Width:Height;
                    if(match.Groups[1].Value=="max"?current>limit:current<limit)return false;
                }
            }
            return true;
        }
        public static List<string> Split(string text,char separator)
        {
            var parts=new List<string>();var start=0;var depth=0;char quote='\0';var escape=false;
            for(var i=0;i<text.Length;i++){var c=text[i];if(escape){escape=false;continue;}if(c=='\\'){escape=true;continue;}if(quote!='\0'){if(c==quote)quote='\0';continue;}if(c=='\''||c=='"'){quote=c;continue;}if(c=='('||c=='[')depth++;if(c==')'||c==']')depth--;if(c==separator&&depth==0){parts.Add(text.Substring(start,i-start));start=i+1;}}
            parts.Add(text.Substring(start));return parts;
        }
        private static int Close(string text,int start)
        {var depth=0;for(var i=start;i<text.Length;i++){if(text[i]=='(')depth++;else if(text[i]==')'&&--depth==0)return i;}return -1;}
        private static int AttributeEnd(string text,int start){char quote='\0';var escape=false;for(var i=start;i<text.Length;i++){var c=text[i];if(escape){escape=false;continue;}if(c=='\\'){escape=true;continue;}if(quote!='\0'){if(c==quote)quote='\0';continue;}if(c=='\''||c=='"'){quote=c;continue;}if(c==']')return i;}return -1;}
        private static string Identifier(string text,ref int index)
        {
            var value=new StringBuilder();while(index<text.Length){var c=text[index];if(c=='\\'&&index+1<text.Length){index++;value.Append(text[index++]);continue;}if(char.IsLetterOrDigit(c)||c=='_'||c=='-'||c>127){value.Append(c);index++;continue;}break;}return value.ToString();
        }
        private static int Specificity(string selector)
        {
            selector=Regex.Replace(selector,@":where\([^)]*\)","");var score=0;
            for(var i=0;i<selector.Length;i++){var c=selector[i];if(c=='\\'){i++;continue;}if(c=='#'){score+=100;var j=i+1;Identifier(selector,ref j);i=j-1;}else if(c=='.'){score+=10;var j=i+1;Identifier(selector,ref j);i=j-1;}else if(c=='['){score+=10;while(i<selector.Length&&selector[i]!=']')i++;}else if(c==':'){if(i+1<selector.Length&&selector[i+1]==':'){score++;i++;}else score+=10;}else if(char.IsLetter(c)&&(i==0||char.IsWhiteSpace(selector[i-1])||selector[i-1]=='>'||selector[i-1]=='+'||selector[i-1]=='~')){score++;var j=i;Identifier(selector,ref j);i=j-1;}}
            return score;
        }
        public static bool Match(VisualCssNode node,string selector)
        {
            if(node.Tag=="#text")return false;
            var pseudo=selector.IndexOf("::",StringComparison.Ordinal);
            if(node.Pseudo!=null){if(pseudo<0||selector.Substring(pseudo)!=node.Pseudo)return false;selector=pseudo==0?"*":selector.Substring(0,pseudo);node=node.Parent;}
            else if(pseudo>=0)return false;
            if(!SelectorCache.TryGetValue(selector,out var parsed)){
            var parts=new List<string>();var relations=new List<char>();var current=new StringBuilder();var depth=0;var pending=' ';var escape=false;
            for(var i=0;i<selector.Length;i++) {
                var c=selector[i];if(escape){current.Append(c);escape=false;continue;}if(c=='\\'){current.Append(c);escape=true;continue;}if(c=='('||c=='[')depth++;if(c==')'||c==']')depth--;
                if(depth==0&&(char.IsWhiteSpace(c)||c=='>'||c=='+'||c=='~')){if(current.Length>0){if(parts.Count>0)relations.Add(pending);parts.Add(current.ToString());current.Clear();pending=' ';}if(c=='>'||c=='+'||c=='~')pending=c;continue;}current.Append(c);
            }
            if(current.Length>0){if(parts.Count>0)relations.Add(pending);parts.Add(current.ToString());}
            parsed=new ParsedSelector {Parts=parts,Relations=relations};SelectorCache[selector]=parsed;
            }
            var selectorParts=parsed.Parts;var selectorRelations=parsed.Relations;
            bool At(VisualCssNode element,int index) {
                if(element==null||!Compound(element,selectorParts[index]))return false;if(index==0)return true;var relation=selectorRelations[index-1];
                if(relation=='>')return At(element.Parent,index-1);
                if(relation=='+'||relation=='~'){if(element.Parent==null)return false;var siblings=element.Parent.Children.Where(child=>child.Tag!="#text"&&child.Pseudo==null).ToList();var position=siblings.IndexOf(element);if(relation=='+')return position>0&&At(siblings[position-1],index-1);for(var p=position-1;p>=0;p--)if(At(siblings[p],index-1))return true;return false;}
                for(var parent=element.Parent;parent!=null;parent=parent.Parent)if(At(parent,index-1))return true;return false;
            }
            return selectorParts.Count>0&&At(node,selectorParts.Count-1);
        }
        private static bool Compound(VisualCssNode node,string selector)
        {
            var i=0;if(selector.Length==0)return false;
            if(selector[i]=='*')i++;else if(char.IsLetter(selector[i])){var tag=Identifier(selector,ref i);if(tag!=node.Tag)return false;}
            while(i<selector.Length) {
                var c=selector[i++];
                if(c=='.'){var name=Identifier(selector,ref i);if(!node.HasClass(name))return false;}
                else if(c=='#'){if(node.Attribute("id")!=Identifier(selector,ref i))return false;}
                else if(c=='['){var end=AttributeEnd(selector,i);if(end<0)return false;var expression=selector.Substring(i,end-i);i=end+1;var match=Regex.Match(expression,@"^([^\s=~^$*|]+)\s*(?:(~=|\^=|\$=|\*=|\|=|=)\s*(.*))?$");if(!match.Success)return false;var key=match.Groups[1].Value;var actual=node.Attribute(key);if(actual==null)return false;if(match.Groups[2].Success){var expected=match.Groups[3].Value.Trim().Trim('\'', '"');switch(match.Groups[2].Value){case "=":if(actual!=expected)return false;break;case "~=":if(!actual.Split(' ').Contains(expected))return false;break;case "^=":if(!actual.StartsWith(expected,StringComparison.Ordinal))return false;break;case "$=":if(!actual.EndsWith(expected,StringComparison.Ordinal))return false;break;case "*=":if(!actual.Contains(expected))return false;break;}}}
                else if(c==':') {
                    var name=Identifier(selector,ref i);var argument="";if(i<selector.Length&&selector[i]=='('){var end=Close(selector,i);if(end<0)return false;argument=selector.Substring(i+1,end-i-1);i=end+1;}
                    switch(name) {
                        case "root":if(node.Parent!=null)return false;break;
                        case "where":case "is":if(!Split(argument,',').Any(part=>Match(node,part.Trim())))return false;break;
                        case "not":if(Split(argument,',').Any(part=>Match(node,part.Trim())))return false;break;
                        case "first-child":if(node.ElementIndex!=0)return false;break;
                        case "last-child":if(node.Parent==null||node.ElementIndex!=node.Parent.Children.Count(child=>child.Tag!="#text"&&child.Pseudo==null)-1)return false;break;
                        case "nth-child":var ordinal=node.ElementIndex+1;if(int.TryParse(argument,out var wanted)){if(ordinal!=wanted)return false;}else if(argument=="odd"?ordinal%2!=1:argument=="even"?ordinal%2!=0:true)return false;break;
                        case "hover":if(!node.Hover)return false;break;case "active":if(!node.Active)return false;break;case "focus":case "focus-visible":if(!node.Focused)return false;break;
                        case "disabled":if(!node.Properties["disabled"].Truth)return false;break;
                        case "host":return false;
                        default:return false;
                    }
                } else return false;
            }
            return true;
        }
        private static void Expand(Dictionary<string,string> style,string property,string value)
        {
            var words=Split(value,' ').Where(word=>word.Length>0).ToArray();
            if(property=="transition") {
                var properties=new List<string>();var durations=new List<string>();var timings=new List<string>();var delays=new List<string>();
                foreach(var item in Split(value,',')){
                    var parts=Split(item.Trim(),' ').Where(part=>part.Length>0).ToArray();
                    bool Time(string part)=>Regex.IsMatch(part,@"^[-\d.]+m?s$");
                    bool Timing(string part)=>new[]{"linear","ease","ease-in","ease-out","ease-in-out","step-start","step-end"}.Contains(part)||part.StartsWith("cubic-bezier(",StringComparison.Ordinal)||part.StartsWith("steps(",StringComparison.Ordinal);
                    var times=parts.Where(Time).ToArray();properties.Add(parts.FirstOrDefault(part=>!Time(part)&&!Timing(part))??"all");durations.Add(times.Length>0?times[0]:"0s");delays.Add(times.Length>1?times[1]:"0s");timings.Add(parts.FirstOrDefault(Timing)??"ease");
                }
                style[property]=value;style["transition-property"]=string.Join(", ",properties);style["transition-duration"]=string.Join(", ",durations);style["transition-timing-function"]=string.Join(", ",timings);style["transition-delay"]=string.Join(", ",delays);return;
            }
            if(property=="background") {
                var layers=Split(value,',');var images=layers.Where(layer=>layer.Contains("gradient(")||layer.Contains("url(")).ToArray();
                style["background-image"]=images.Length==0?"none":string.Join(",",images);style["background-color"]="transparent";style["background-size"]="auto";style["background-position"]="0% 0%";style["background-repeat"]="repeat";
                foreach(var word in Split(layers[layers.Count-1].Trim(),' '))if(word.StartsWith("#",StringComparison.Ordinal)||word.StartsWith("rgb",StringComparison.Ordinal)||word.StartsWith("color-mix(",StringComparison.Ordinal)||new[]{"transparent","black","white","currentColor"}.Contains(word))style["background-color"]=word;
                return;
            }
            if(property=="margin"||property=="padding"||property=="inset"||property=="border-width"||property=="border-color"||property=="border-style") {
                if(words.Length==0)return;var suffix=property.StartsWith("border-",StringComparison.Ordinal)?property.Substring(7):"";var prefix=suffix.Length>0?"border-":property=="inset"?"":property+"-";
                var positions=new[]{"top","right","bottom","left"};var indices=new[]{0,words.Length>1?1:0,words.Length>2?2:0,words.Length>3?3:words.Length>1?1:0};
                for(var i=0;i<4;i++)style[prefix+positions[i]+(suffix.Length>0?"-"+suffix:"")]=words[indices[i]];return;
            }
            if(property=="border"||property=="border-top"||property=="border-right"||property=="border-bottom"||property=="border-left") {
                var edges=property=="border"?new[]{"top","right","bottom","left"}:new[]{property.Substring(7)};
                var width=words.FirstOrDefault(word=>Regex.IsMatch(word,@"^\d|^thin$|^medium$|^thick$"))??"medium";var kind=words.FirstOrDefault(word=>new[]{"none","solid","dashed","dotted","double","hidden"}.Contains(word))??"none";
                var color=words.FirstOrDefault(word=>word!=width&&word!=kind)??"currentColor";
                foreach(var edge in edges){style["border-"+edge+"-width"]=width;style["border-"+edge+"-style"]=kind;style["border-"+edge+"-color"]=color;}return;
            }
            if(property=="border-radius") {if(words.Length==0)return;var corners=new[]{"top-left","top-right","bottom-right","bottom-left"};var indices=new[]{0,words.Length>1?1:0,words.Length>2?2:0,words.Length>3?3:words.Length>1?1:0};for(var i=0;i<4;i++)style["border-"+corners[i]+"-radius"]=words[indices[i]];return;}
            if(property=="place-items"){style["align-items"]=value;style["justify-items"]=value;return;}
            if(property=="flex") {if(value=="none"){style["flex-grow"]="0";style["flex-shrink"]="0";style["flex-basis"]="auto";}else if(value=="auto"){style["flex-grow"]="1";style["flex-shrink"]="1";style["flex-basis"]="auto";}else {style["flex-grow"]=words[0];style["flex-shrink"]=words.Length>1?words[1]:"1";style["flex-basis"]=words.Length>2?words[2]:"0%";}return;}
            if(property=="font") {
                var match=Regex.Match(value,@"^(?:(\d+|bold|normal)\s+)?([\d.]+(?:px|rem|em))(?:/([^\s]+))?\s+(.+)$");
                if(match.Success){style["font-weight"]=match.Groups[1].Success?match.Groups[1].Value:"normal";style["font-size"]=match.Groups[2].Value;style["line-height"]=match.Groups[3].Success?match.Groups[3].Value:"normal";style["font-family"]=match.Groups[4].Value;return;}
            }
            style[property]=value;
        }
        public double Length(string value,double reference,double font=16d)
        {
            if(value==null||value=="auto"||value=="none"||value=="normal"||value=="initial"||value=="fit-content"||value=="min-content"||value=="max-content")return double.NaN;
            value=value.Trim();if(value.StartsWith("calc(",StringComparison.Ordinal))return Sum(value.Substring(5,value.Length-6),reference,font);
            foreach(var function in new[]{"min","max","clamp"})if(value.StartsWith(function+"(",StringComparison.Ordinal)){var args=Split(value.Substring(function.Length+1,value.Length-function.Length-2),',').Select(arg=>Length(arg.Trim(),reference,font)).ToArray();return function=="min"?args.Min():function=="max"?args.Max():Math.Max(args[0],Math.Min(args[2],args[1]));}
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
}
