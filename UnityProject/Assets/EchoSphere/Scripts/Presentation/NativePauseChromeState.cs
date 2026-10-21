using System;

namespace EchoSphere.Presentation
{
    public sealed class NativePauseChromeState
    {
        public string Eyebrow, Title, Subtitle, Resume, Return;
        public string[] Tabs = Array.Empty<string>();
        public int SelectedTab;
    }
}
