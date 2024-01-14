using BSE.Tunes.Maui.Client.Models;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class TracklistBaseViewModel : ViewModelBase
    {

        private ObservableCollection<GridPanel>? _items;
        private string? _imageSource;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public string? ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                SetProperty<string>(ref _imageSource, value);
            }
        }

        public TracklistBaseViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}
