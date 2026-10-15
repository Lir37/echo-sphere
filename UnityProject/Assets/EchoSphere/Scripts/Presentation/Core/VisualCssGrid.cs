using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    public static class VisualCssGrid
    {
        public sealed class Cell {public VisualCssNode Node;public double X,Y,Width,Height;public int Row,Column,Span;}
        public sealed class Result {public readonly List<Cell> Cells=new List<Cell>();public double Height;}
        private static string[] Tracks(string value)
        {
            var result=new List<string>();foreach(var part in VisualCss.Split(value,' ').Where(part=>part.Length>0)){
                var repeat=Regex.Match(part,@"^repeat\(\s*(\d+)\s*,(.*)\)$");
                if(repeat.Success){var tracks=Tracks(repeat.Groups[2].Value.Trim());for(var index=0;index<int.Parse(repeat.Groups[1].Value);index++)result.AddRange(tracks);}else if(part!="none")result.Add(part);
            }return result.Count>0?result.ToArray():new[]{"auto"};
        }
        public static int Columns(VisualCss sheet,VisualCssNode node)
        {return Tracks(sheet.Get(node,"grid-template-columns","none")).Length;}
        private static double[] ColumnSizes(VisualCss sheet,VisualCssNode node,string[] tracks,double available)
        {
            var sizes=new double[tracks.Length];var fractions=new double[tracks.Length];
            for(var index=0;index<tracks.Length;index++){
                var track=tracks[index];var minimum="0";var maximum=track;
                if(track.StartsWith("minmax(",StringComparison.Ordinal)){var parts=VisualCss.Split(track.Substring(7,track.Length-8),',');minimum=parts[0].Trim();maximum=parts[1].Trim();}
                var fraction=Regex.Match(maximum,@"^([\d.]+)fr$");
                if(fraction.Success){fractions[index]=double.Parse(fraction.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);var min=sheet.Length(minimum,available);sizes[index]=double.IsNaN(min)?0d:Math.Max(0d,min);}
                else {var fixedSize=sheet.Length(maximum,available);sizes[index]=double.IsNaN(fixedSize)?0d:Math.Max(0d,fixedSize);}
            }
            // A minmax() track freezes at its minimum when that exceeds its
            // fractional share; distribute the rest among the remaining frs.
            var active=Enumerable.Range(0,tracks.Length).Where(index=>fractions[index]>0).ToList();var remaining=available-Enumerable.Range(0,tracks.Length).Where(index=>fractions[index]==0).Sum(index=>sizes[index]);
            while(active.Count>0){var unit=Math.Max(0d,remaining)/active.Sum(index=>fractions[index]);var frozen=active.Where(index=>sizes[index]>unit*fractions[index]).ToArray();if(frozen.Length==0){foreach(var index in active)sizes[index]=unit*fractions[index];break;}foreach(var index in frozen){remaining-=sizes[index];active.Remove(index);}}
            var autos=Enumerable.Range(0,tracks.Length).Where(index=>tracks[index]=="auto").ToArray();if(autos.Length>0){var extra=Math.Max(0d,available-sizes.Sum())/autos.Length;foreach(var index in autos)sizes[index]+=extra;}
            return sizes;
        }
        public static Result Compute(VisualCss sheet,VisualCssNode node)
        {
            string S(VisualCssNode target,string property,string fallback="")=>sheet.Get(target,property,fallback);
            double L(VisualCssNode target,string property,double reference,double fallback=0d){var value=sheet.Length(S(target,property),reference,sheet.Length(S(target,"font-size","16px"),16));return double.IsNaN(value)?fallback:value;}
            var result=new Result();var columns=Columns(sheet,node);var left=L(node,"border-left-width",node.Width)+L(node,"padding-left",node.Width);var right=L(node,"border-right-width",node.Width)+L(node,"padding-right",node.Width);var top=L(node,"border-top-width",node.Width)+L(node,"padding-top",node.Width);var bottom=L(node,"border-bottom-width",node.Width)+L(node,"padding-bottom",node.Width);
            var gap=VisualCss.Split(S(node,"gap","0"),' ').Where(value=>value.Length>0).ToArray();var rowGap=sheet.Length(S(node,"row-gap",gap.Length>0?gap[0]:"0"),node.Width);var columnGap=sheet.Length(S(node,"column-gap",gap.Length>1?gap[1]:gap.Length>0?gap[0]:"0"),node.Width);
            var availableWidth=Math.Max(0,node.Width-left-right);var columnWidths=ColumnSizes(sheet,node,Tracks(S(node,"grid-template-columns","none")),Math.Max(0,availableWidth-columnGap*(columns-1)));var row=0;var column=0;
            foreach(var child in node.Children.Where(child=>child.Tag!="#text"&&S(child,"display")!="none"&&S(child,"position")!="absolute"&&S(child,"position")!="fixed").OrderBy(child=>L(child,"order",1))) {
                var spanMatch=Regex.Match(S(child,"grid-column"),@"span\s+(\d+)");var span=Math.Min(columns,spanMatch.Success?int.Parse(spanMatch.Groups[1].Value):1);if(column+span>columns){column=0;row++;}
                result.Cells.Add(new Cell {Node=child,Column=column,Row=row,Span=span,Width=columnWidths.Skip(column).Take(span).Sum()+columnGap*(span-1)});column+=span;if(column>=columns){column=0;row++;}
            }
            var count=result.Cells.Count==0?0:result.Cells.Max(cell=>cell.Row)+1;var heights=new double[count];var tracks=Tracks(S(node,"grid-template-rows","auto"));var fractions=new double[count];
            for(var index=0;index<count;index++){
                var track=index<tracks.Length?tracks[index]:"auto";var fractional=Regex.Match(track,@"([\d.]+)fr");
                if(fractional.Success)fractions[index]=double.Parse(fractional.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);
                else if(track!="auto"){var fixedHeight=sheet.Length(track,node.Height);if(!double.IsNaN(fixedHeight))heights[index]=fixedHeight;}
                if(track=="auto")heights[index]=result.Cells.Where(cell=>cell.Row==index).Select(cell=>cell.Node.Height+L(cell.Node,"margin-top",availableWidth)+L(cell.Node,"margin-bottom",availableWidth)).DefaultIfEmpty(0d).Max();
            }
            var totalFraction=fractions.Sum();var availableHeight=Math.Max(0,node.Height-top-bottom-rowGap*Math.Max(0,count-1)-heights.Sum());if(totalFraction>0)for(var index=0;index<count;index++)if(fractions[index]>0)heights[index]=availableHeight*fractions[index]/totalFraction;
            var autoRows=Enumerable.Range(0,count).Where(index=>index>=tracks.Length||tracks[index]=="auto").ToArray();
            if(totalFraction==0&&autoRows.Length>0&&(S(node,"height","auto")!="auto"||L(node,"min-height",node.Height)>0d)&&new[]{"normal","stretch"}.Contains(S(node,"align-content","normal")))foreach(var index in autoRows)heights[index]+=availableHeight/autoRows.Length;
            var placement=VisualCss.Split(S(node,"place-items","stretch"),' ').Where(value=>value.Length>0).ToArray();
            foreach(var cell in result.Cells){var child=cell.Node;var ml=L(child,"margin-left",availableWidth);var mr=L(child,"margin-right",availableWidth);var mt=L(child,"margin-top",availableWidth);var mb=L(child,"margin-bottom",availableWidth);var align=S(child,"align-self",S(node,"align-items",placement[0]));var justify=S(child,"justify-self",S(node,"justify-items",placement.Length>1?placement[1]:placement[0]));
                var cellWidth=cell.Width;var explicitWidth=L(child,"width",cellWidth,double.NaN);var explicitHeight=L(child,"height",heights[cell.Row],double.NaN);
                var width=double.IsNaN(explicitWidth)?justify=="stretch"?cellWidth-ml-mr:child.Width:explicitWidth;var height=double.IsNaN(explicitHeight)?align=="stretch"?heights[cell.Row]-mt-mb:child.Height:explicitHeight;
                width=Math.Max(L(child,"min-width",cellWidth),Math.Min(width,L(child,"max-width",cellWidth,double.PositiveInfinity)));height=Math.Max(L(child,"min-height",heights[cell.Row]),Math.Min(height,L(child,"max-height",heights[cell.Row],double.PositiveInfinity)));
                cell.X=left+columnWidths.Take(cell.Column).Sum()+cell.Column*columnGap+ml+(justify=="center"?(cellWidth-width-ml-mr)*.5d:justify=="end"?cellWidth-width-ml-mr:0d);
                cell.Y=top+heights.Take(cell.Row).Sum()+rowGap*cell.Row+mt+(align=="center"?(heights[cell.Row]-height-mt-mb)*.5d:align=="end"?heights[cell.Row]-height-mt-mb:0d);
                cell.Width=Math.Max(0,width);cell.Height=Math.Max(0,height);
            }
            result.Height=top+bottom+heights.Sum()+rowGap*Math.Max(0,count-1);return result;
        }
    }
}
