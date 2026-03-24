namespace BSE.Tunes.WinUI.Client.Models;

public class CarouselItem
{
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string ImageSource { get; set; } = string.Empty;
    public object? Data { get; set; }
}