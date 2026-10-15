using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EchoSphere.Presentation
{
    // Small AOT-safe JSON reader for the exported CSS and source UI metadata.
    public static class VisualJson
    {
        public static V Parse(string text) {var reader=new Reader(text);var value=reader.Value();reader.Space();if(!reader.End)throw new FormatException("Trailing source JSON");return value;}
        private sealed class Reader
        {
            private readonly string _text;private int _index;
            public Reader(string text){_text=text;}
            public bool End=>_index>=_text.Length;
            public void Space(){while(!End&&char.IsWhiteSpace(_text[_index]))_index++;}
            private char Take(){if(End)throw new FormatException("Unexpected end of source JSON");return _text[_index++];}
            private void Expect(char c){Space();if(Take()!=c)throw new FormatException("Invalid source JSON at "+_index);}
            public V Value()
            {
                Space();if(End)throw new FormatException("Missing source JSON value");var first=_text[_index];
                if(first=='"')return String();
                if(first=='{'){_index++;var value=VisualJs.Obj();Space();if(!End&&_text[_index]=='}'){_index++;return value;}do{Space();var key=String();Expect(':');value[key]=Value();Space();var next=Take();if(next=='}')return value;if(next!=',')throw new FormatException("Invalid source JSON object");}while(true);}
                if(first=='['){_index++;var values=new List<V>();Space();if(!End&&_text[_index]==']'){_index++;return VisualJs.Arr();}do{values.Add(Value());Space();var next=Take();if(next==']')return VisualJs.Arr(values.ToArray());if(next!=',')throw new FormatException("Invalid source JSON array");}while(true);}
                foreach(var word in new[]{"true","false","null"})if(_text.Length-_index>=word.Length&&string.CompareOrdinal(_text,_index,word,0,word.Length)==0){_index+=word.Length;return word=="null"?VisualJs.Null:new V(word=="true");}
                var start=_index;while(!End&&"0123456789.eE+-".IndexOf(_text[_index])>=0)_index++;
                return new V(double.Parse(_text.Substring(start,_index-start),NumberStyles.Float,CultureInfo.InvariantCulture));
            }
            private string String()
            {
                Expect('"');var result=new StringBuilder();
                while(true){var c=Take();if(c=='"')return result.ToString();if(c!='\\'){result.Append(c);continue;}c=Take();switch(c){case '"':case '\\':case '/':result.Append(c);break;case 'b':result.Append('\b');break;case 'f':result.Append('\f');break;case 'n':result.Append('\n');break;case 'r':result.Append('\r');break;case 't':result.Append('\t');break;case 'u':result.Append((char)int.Parse(_text.Substring(_index,4),NumberStyles.HexNumber,CultureInfo.InvariantCulture));_index+=4;break;default:throw new FormatException("Invalid source JSON escape");}}
            }
        }
        public static string Stringify(V value,bool sortKeys=false)
        {
            var result=new StringBuilder();Write(value,result,sortKeys);return result.ToString();
        }
        private static void Quote(string text,StringBuilder result)
        {
            result.Append('"');foreach(var c in text)switch(c){case '"':result.Append("\\\"");break;case '\\':result.Append("\\\\");break;case '\n':result.Append("\\n");break;case '\r':result.Append("\\r");break;case '\t':result.Append("\\t");break;default:if(c<32)result.Append("\\u").Append(((int)c).ToString("x4"));else result.Append(c);break;}result.Append('"');
        }
        private static void Write(V value,StringBuilder result,bool sorted)
        {
            if(value.Null||value.Undefined){result.Append("null");return;}if(value.Raw is string text){Quote(text,result);return;}if(value.Raw is bool boolean){result.Append(boolean?"true":"false");return;}
            if(value.Raw is double number){result.Append(double.IsNaN(number)||double.IsInfinity(number)?"null":VisualJs.NumberText(number));return;}
            if(value.Raw is List<V> list){result.Append('[');for(var i=0;i<list.Count;i++){if(i>0)result.Append(',');Write(list[i],result,sorted);}result.Append(']');return;}
            if(value.Raw is Dictionary<string,V> map){var keys=new List<string>(map.Keys);if(sorted)keys.Sort(StringComparer.Ordinal);result.Append('{');var first=true;foreach(var key in keys){if(map[key].Undefined||map[key].Raw is Func<V[],V>)continue;if(!first)result.Append(',');first=false;Quote(key,result);result.Append(':');Write(map[key],result,sorted);}result.Append('}');return;}
            throw new InvalidOperationException("Unsupported source JSON value: "+value.Raw.GetType().Name);
        }
    }
}
