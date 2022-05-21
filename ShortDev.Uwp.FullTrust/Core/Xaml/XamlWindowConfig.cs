﻿namespace ShortDev.Uwp.FullTrust.Core.Xaml
{
    public sealed class XamlWindowConfig
    {
        public XamlWindowConfig(string title)
            => this.Title = title;

        public string Title { get; }
        public bool HasTransparentBackground { get; set; } = true;
        public bool HasWin32Frame { get; set; } = true;
        public bool HasWin32TitleBar { get; set; } = true;
        public bool IsTopMost { get; set; } = false;
        public bool IsVisible { get; set; } = true;
    }
}