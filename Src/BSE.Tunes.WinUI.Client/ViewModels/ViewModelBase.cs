using BSE.Tunes.WinUI.Client.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public abstract class ViewModelBase : ObservableRecipient, INavigationAware
    {
        public virtual void OnNavigatedFrom()
        {
        }

        public virtual void OnNavigatedTo(object parameter)
        {
        }
    }
}