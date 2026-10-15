using EchoSphere.Presentation;
static class NativeImageChecks
{
 public static void Run(){var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-canvas-image-fixtures.json")));var count=0;var worst=0d;
  foreach(var f in fixtures.Items){var width=(int)f["width"].Number;var height=(int)f["height"].Number;
   VisualCssColor Pixel(int x,int y){var i=(Math.Clamp(y,0,1)*2+Math.Clamp(x,0,1))*4;return new VisualCssColor(f["pixels"][i].Number/255d,f["pixels"][i+1].Number/255d,f["pixels"][i+2].Number/255d,f["pixels"][i+3].Number/255d);}
   for(var y=0;y<height;y++)for(var x=0;x<width;x++){var px=(x+.5d)*2d/width-.5d;var py=(y+.5d)*2d/height-.5d;var bx=(int)Math.Floor(px);var by=(int)Math.Floor(py);var tx=px-bx;var ty=py-by;
    // Same premultiplied interpolation as imageSample/imagePixel in the native
    // shader, compared with real drawImage output; no GPU execution is implied.
    var a=VisualCssColor.Lerp(Pixel(bx,by),Pixel(bx+1,by),tx);var b=VisualCssColor.Lerp(Pixel(bx,by+1),Pixel(bx+1,by+1),tx);var c=VisualCssColor.Lerp(a,b,ty);var alpha=c.A*f["alpha"].Number;var i=(y*width+x)*4;var expectedAlpha=f["expected"][i+3].Number/255d;
    var error=Math.Max(Math.Abs(alpha-expectedAlpha),new[]{c.R,c.G,c.B}.Select((v,k)=>Math.Abs(v*alpha-f["expected"][i+k].Number/255d*expectedAlpha)).Max())*255d;worst=Math.Max(worst,error);if(error>2.5d)throw new InvalidOperationException("Canvas image premultiplied sample differs at "+x+","+y+": "+error);count++;
   }
  }Console.WriteLine($"Browser Canvas image interpolation → native shader math: PASS ({count} pixels; maximum channel error {worst:F3}/255). No GPU execution.");
 }
}
