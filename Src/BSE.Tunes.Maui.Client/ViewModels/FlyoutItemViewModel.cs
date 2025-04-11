using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class FlyoutItemViewModel : BindableBase
    {
        private string _text;

        private ICommand _selectFlyoutItemCommand;

        public event EventHandler<EventArgs> ItemClicked;

        public ICommand SelectFlyoutItemCommand =>
            _selectFlyoutItemCommand ?? (_selectFlyoutItemCommand = new DelegateCommand<object>(SelectFlyoutItem));

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string ImageSource
        {
            get;
            set;
        }

        public dynamic Data
        {
            get;
            set;
        }

        private void SelectFlyoutItem(object obj)
        {
            ItemClicked?.Invoke(obj, EventArgs.Empty);
        }
    }
}
