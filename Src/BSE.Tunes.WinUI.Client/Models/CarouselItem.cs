using Microsoft.UI.Xaml.Media.Imaging;

namespace BSE.Tunes.WinUI.Client.Models;

public class CarouselItem
{
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public BitmapSource? ImageSource => new BitmapImage
    {
        CreateOptions = IgnoreImageCache ? BitmapCreateOptions.IgnoreImageCache : BitmapCreateOptions.None,
        UriSource = ImagePath != null ? new Uri(ImagePath) : null
    };
    public string? ImagePath { get; set; }
    public bool IgnoreImageCache { get; set; } = false;
    public object? Data { get; set; }
}