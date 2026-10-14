using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private string _sourceFlashColor = "#39d8ff", _sourceFlashKind;
        public float SourceFlashLife => _messageTimer;
        public string SourceFlashColor => _sourceFlashColor;
        public bool SourceFlashBoundary => _sourceFlashKind == "boundary" && _messageTimer > 0f;
        private void DrawSourceFlashText()
        {
            if (_messageTimer <= 0f || string.IsNullOrEmpty(_message)) return;
            var font = Mathf.Clamp(_uiWidth*.032f,10f,12f);
            var style = new GUIStyle(_label) { fontSize = Mathf.RoundToInt(font), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            var maxWidth = Mathf.Min(_uiWidth-44f,224f);
            var lines = new List<string>(); var current = "";
            foreach (var word in _message.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var next = current.Length == 0 ? word : current+" "+word;
                if (current.Length == 0 || style.CalcSize(new GUIContent(next)).x <= maxWidth) current=next;
                else { lines.Add(current); current=word; }
            }
            if (current.Length > 0) lines.Add(current);
            if (lines.Count > 2) { var rest=string.Join(" ",lines.GetRange(1,lines.Count-1)); lines=new List<string> { lines[0], rest.Substring(0,Mathf.Min(34,rest.Length))+"…" }; }
            var widest=104f; foreach (var line in lines) widest=Mathf.Max(widest,style.CalcSize(new GUIContent(line)).x);
            var height=lines.Count*(font+2f)+9f; var width=Mathf.Min(_uiWidth-22f,Mathf.Max(122f,widest+16f));
            var top=Mathf.Max(142f,Mathf.Min(_uiHeight*.205f,178f))-height+3f;
            var panel=new Rect((_uiWidth-width)*.5f,top,width,height); var previous=GUI.color;
            GUI.color=new Color(previous.r,previous.g,previous.b,previous.a*Mathf.Clamp01(_messageTimer/.82f));
            var fade=Mathf.Clamp01(_messageTimer/.82f);
            SourceUiRect(panel,new Color(2f/255f,7f/255f,13f/255f,.92f*fade)); ColorUtility.TryParseHtmlString(_sourceFlashColor,out var color); color.a=fade;
            SourceUiRect(new Rect(panel.x,panel.y,panel.width,1f),color); SourceUiRect(new Rect(panel.x,panel.yMax-1f,panel.width,1f),color);
            SourceUiRect(new Rect(panel.x,panel.y,1f,panel.height),color); SourceUiRect(new Rect(panel.xMax-1f,panel.y,1f,panel.height),color);
            color.a=1f; style.normal.textColor=color;
            for(var i=0;i<lines.Count;i++) GUI.Label(new Rect(panel.x,panel.y+3f+i*(font+2f),width,font+4f),lines[i],style);
            GUI.color=previous;
        }
    }
}
