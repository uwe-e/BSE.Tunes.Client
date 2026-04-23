namespace BSE.Tunes.WinUI.Client.Models;

/// <summary>
/// Model for dynamic flyout menu items
/// </summary>
public class FlyoutItem
{
    public string Text { get; set; } = string.Empty;
    public string? Glyph { get; set; }
    public bool IsSeparator { get; set; }
    public object? Data { get; set; }
}