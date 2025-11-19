using System;

namespace BSE.Tunes.Maui.Client.Controls
{
    public sealed class SizeAllocatedEventArgs : EventArgs
    {
        public double Width { get; }
        public double Height { get; }

        public SizeAllocatedEventArgs(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}