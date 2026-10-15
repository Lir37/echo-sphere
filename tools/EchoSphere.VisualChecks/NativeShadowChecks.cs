using EchoSphere.Presentation;
using System.IO.Compression;

static class NativeShadowChecks
{
 public static void Run(){using var input=new GZipStream(File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-canvas-shadow-fixtures.json.gz")),CompressionMode.Decompress);using var reader=new StreamReader(input);var fixtures=VisualJson.Parse(reader.ReadToEnd());var count=0;var worst=0d;
  foreach(var f in fixtures.Items){var w=(int)f["width"].Number;var h=(int)f["height"].Number;var dpr=f["dpr"].Number;var blur=f["blur"].Number;var dx=f["dx"].Number;var dy=f["dy"].Number;var viewport=new VisualCanvasShadowViewport(w,h,VisualCanvasShadowViewport.Padding(blur,dx),VisualCanvasShadowViewport.Padding(blur,dy));
   var mask=new double[viewport.Width*viewport.Height];var x0=f["x"].Number*dpr;var y0=f["y"].Number*dpr;var x1=x0+f["w"].Number*dpr;var y1=y0+f["h"].Number*dpr;
   double Shape(int x,int y)=>Math.Max(0d,Math.Min(x+1d,x1)-Math.Max(x,x0))*Math.Max(0d,Math.Min(y+1d,y1)-Math.Max(y,y0));
   for(var y=0;y<viewport.Height;y++)for(var x=0;x<viewport.Width;x++)mask[y*viewport.Width+x]=Shape(x-viewport.PaddingX,y-viewport.PaddingY);
   var sigma=VisualCanvasShadowViewport.Sigma(blur);var radius=(int)Math.Ceiling(4d*sigma);var weights=Enumerable.Range(-radius,radius*2+1).Select(i=>Math.Exp(-.5d*i*i/(sigma*sigma))).ToArray();var total=weights.Sum();for(var i=0;i<weights.Length;i++)weights[i]/=total;
   // CPU execution of the native separable shader math, with the same padded
   // viewport and crop. It checks Canvas semantics; it does not execute GPU code.
   var horizontal=new double[mask.Length];var vertical=new double[mask.Length];
   for(var y=0;y<viewport.Height;y++)for(var x=0;x<viewport.Width;x++)for(var k=-radius;k<=radius;k++)if(x+k>=0&&x+k<viewport.Width)horizontal[y*viewport.Width+x]+=mask[y*viewport.Width+x+k]*weights[k+radius];
   for(var y=0;y<viewport.Height;y++)for(var x=0;x<viewport.Width;x++)for(var k=-radius;k<=radius;k++)if(y+k>=0&&y+k<viewport.Height)vertical[y*viewport.Width+x]+=horizontal[(y+k)*viewport.Width+x]*weights[k+radius];
   var errors=new List<double>();for(var y=0;y<h;y++)for(var x=0;x<w;x++){var px=x+viewport.PaddingX-(int)dx;var py=y+viewport.PaddingY-(int)dy;var shadow=px<0||py<0||px>=viewport.Width||py>=viewport.Height?0d:vertical[py*viewport.Width+px];var shape=Shape(x,y);var alpha=shape+shadow*(1d-shape);var expected=f["alpha"][y*w+x].Number;if(expected>0||alpha>.5d/255d)errors.Add(Math.Abs(alpha*255d-expected));}
   var mean=errors.DefaultIfEmpty(0d).Average();worst=Math.Max(worst,mean);if(mean>1.5d||errors.Count(e=>e>8d)>errors.Count*.04d)throw new InvalidOperationException("Canvas shadow differs at DPR "+dpr+" / "+f["x"].Text+","+f["y"].Text+": mean="+mean+", max="+errors.Max());count++;
  }Console.WriteLine($"Browser Canvas edge shadows → native padded shader math: PASS ({count} cases; maximum supported-pixel mean {worst:F3}/255). No GPU execution.");
 }
}
