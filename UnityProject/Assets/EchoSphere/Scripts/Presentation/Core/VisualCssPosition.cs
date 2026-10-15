using System;

namespace EchoSphere.Presentation
{
    public static class VisualCssPosition
    {
        public static VisualCssNode ContainingBlock(VisualCss sheet,VisualCssNode node)
        {
            var fixedPosition=sheet.Get(node,"position")=="fixed";
            for(var parent=node.Parent;parent!=null;parent=parent.Parent){
                var transformed=sheet.Get(parent,"transform","none")!="none"||sheet.Get(parent,"perspective","none")!="none"||sheet.Get(parent,"filter","none")!="none"||sheet.Get(parent,"will-change","").Contains("transform");
                if(transformed||!fixedPosition&&sheet.Get(parent,"position","static")!="static")return parent;
            }return null;
        }
        public static double Width(VisualCss sheet,VisualCssNode block)=>block==null?sheet.Width:Math.Max(0d,block.Width-Border(sheet,block,"left")-Border(sheet,block,"right"));
        public static double Height(VisualCss sheet,VisualCssNode block)=>block==null?sheet.Height:Math.Max(0d,block.Height-Border(sheet,block,"top")-Border(sheet,block,"bottom"));
        public static double Border(VisualCss sheet,VisualCssNode block,string side)
        {var value=sheet.Length(sheet.Get(block,"border-"+side+"-width","0"),block.Width);return sheet.Get(block,"border-"+side+"-style","none")=="none"||double.IsNaN(value)?0d:value;}
    }
}
