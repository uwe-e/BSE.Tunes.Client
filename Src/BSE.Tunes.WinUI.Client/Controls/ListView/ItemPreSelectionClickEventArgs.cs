using System;

namespace BSE.Tunes.WinUI.Client.Controls;

public class ItemPreSelectionClickEventArgs : EventArgs
{
    public object? Source { get; set; }
    public object? SelectedItem { get; set; }
}