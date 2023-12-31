using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls
{
    interface IPlayerElement
    {
        object SelectTrackCommandParameter { get; set; }
        ICommand SelectTrackCommand { get; set; }
        void OnSelectTrackCommandCanExecuteChanged(object sender, EventArgs e);

        object PlayCommandParameter { get; set; }
        ICommand PlayCommand { get; set; }
        void OnPlayCommandCanExecuteChanged(object sender, EventArgs e);

        object PauseCommandParameter { get; set; }
        ICommand PauseCommand { get; set; }
        void OnPauseCommandCanExecuteChanged(object sender, EventArgs e);

        bool IsPlayNextEnabled { get; set; }

        object PlayNextCommandParameter { get; set; }
        ICommand PlayNextCommand { get; set; }
        void OnPlayNextCommandCanExecuteChanged(object sender, EventArgs e);

        bool IsPlayPreviousEnabled { get; set; }

        object PlayPreviousCommandParameter { get; set; }
        ICommand PlayPreviousCommand { get; set; }
        void OnPlayPreviousCommandCanExecuteChanged(object sender, EventArgs e);

        double Progress { get; set; }
    }
}
