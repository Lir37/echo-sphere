using System.Runtime.InteropServices;
using System.Text.Json;

// CSS cursor:pointer uses the platform's hand. Read that existing system
// resource without changing the desktop cursor or drawing replacement artwork.
if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Capture the Windows browser reference on Windows.");
var cursor = Native.LoadCursor(IntPtr.Zero, new IntPtr(32649)); // IDC_HAND
if (cursor == IntPtr.Zero || !Native.GetIconInfo(cursor, out var icon)) throw new InvalidOperationException("Cannot read the system hand cursor.");
try
{
    if (Native.GetObject(icon.Color != IntPtr.Zero ? icon.Color : icon.Mask, Marshal.SizeOf<Native.Bitmap>(), out var bitmap) == 0) throw new InvalidOperationException("Cannot read cursor dimensions.");
    var width = bitmap.Width;
    var height = icon.Color != IntPtr.Zero ? bitmap.Height : bitmap.Height / 2;
    var black = Raster(0);
    var white = Raster(255);
    var rgba = new byte[width * height * 4];
    for (var i = 0; i < rgba.Length; i += 4)
    {
        var alpha = 255 - (white[i] + white[i + 1] + white[i + 2] - black[i] - black[i + 1] - black[i + 2]) / 3;
        rgba[i + 3] = (byte)Math.Clamp(alpha, 0, 255);
        for (var channel = 0; channel < 3; channel++) rgba[i + channel] = alpha > 0 ? (byte)Math.Clamp((black[i + 2 - channel] * 255 + alpha / 2) / alpha, 0, 255) : (byte)0;
    }
    Console.WriteLine(JsonSerializer.Serialize(new { width, height, hotspotX = icon.HotspotX, hotspotY = icon.HotspotY, pixels = Convert.ToBase64String(rgba) }));

    byte[] Raster(byte background)
    {
        var dc = Native.CreateCompatibleDC(IntPtr.Zero);
        var info = new Native.BitmapInfo { Size = (uint)Marshal.SizeOf<Native.BitmapInfo>(), Width = width, Height = -height, Planes = 1, BitCount = 32 };
        var surface = Native.CreateDIBSection(dc, ref info, 0, out var pixels, IntPtr.Zero, 0);
        if (dc == IntPtr.Zero || surface == IntPtr.Zero) throw new InvalidOperationException("Cannot allocate cursor reference surface.");
        var previous = Native.SelectObject(dc, surface);
        try
        {
            var data = Enumerable.Repeat(background, width * height * 4).ToArray();
            Marshal.Copy(data, 0, pixels, data.Length);
            if (!Native.DrawIconEx(dc, 0, 0, cursor, width, height, 0, IntPtr.Zero, 3)) throw new InvalidOperationException("Cannot rasterize the system hand cursor.");
            Native.GdiFlush();
            Marshal.Copy(pixels, data, 0, data.Length);
            return data;
        }
        finally { Native.SelectObject(dc, previous); Native.DeleteObject(surface); Native.DeleteDC(dc); }
    }
}
finally { if (icon.Color != IntPtr.Zero) Native.DeleteObject(icon.Color); if (icon.Mask != IntPtr.Zero) Native.DeleteObject(icon.Mask); }

static class Native
{
    [StructLayout(LayoutKind.Sequential)] internal struct IconInfo { internal int IsIcon; internal uint HotspotX, HotspotY; internal IntPtr Mask, Color; }
    [StructLayout(LayoutKind.Sequential)] internal struct Bitmap { internal int Type, Width, Height, WidthBytes; internal ushort Planes, BitsPixel; internal IntPtr Bits; }
    [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo { internal uint Size; internal int Width, Height; internal ushort Planes, BitCount; internal uint Compression, SizeImage; internal int XPels, YPels; internal uint ColorsUsed, ColorsImportant; }
    [DllImport("user32.dll", EntryPoint = "LoadCursorW")] internal static extern IntPtr LoadCursor(IntPtr instance, IntPtr name);
    [DllImport("user32.dll")] internal static extern bool GetIconInfo(IntPtr icon, out IconInfo info);
    [DllImport("user32.dll")] internal static extern bool DrawIconEx(IntPtr dc, int x, int y, IntPtr icon, int width, int height, uint step, IntPtr brush, uint flags);
    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")] internal static extern int GetObject(IntPtr obj, int size, out Bitmap bitmap);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateDIBSection(IntPtr dc, ref BitmapInfo info, uint usage, out IntPtr bits, IntPtr section, uint offset);
    [DllImport("gdi32.dll")] internal static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern bool GdiFlush();
}
