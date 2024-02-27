using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models.Contract;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls
{
    public partial class AudioPlayer : ContentView, IPlayerController, IPlayerElement
    {
        public static readonly BindableProperty SelectTrackCommandProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.SelectTrackCommand),
                typeof(ICommand),
                typeof(IPlayerElement),
                null,
                propertyChanging: OnSelectTrackCommandChanging,
                propertyChanged: OnSelectTrackCommandChanged);

        public ICommand SelectTrackCommand
        {
            get => (ICommand)GetValue(SelectTrackCommandProperty);
            set => SetValue(SelectTrackCommandProperty, value);
        }

        public static readonly BindableProperty SelectTrackCommandParameterProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.SelectTrackCommandParameter),
                typeof(object),
                typeof(IPlayerElement),
                null,
                propertyChanged: (bindable, oldvalue, newvalue) => SelectTrackCommandCanExecuteChanged(bindable, EventArgs.Empty));

        public object SelectTrackCommandParameter
        {
            get => GetValue(SelectTrackCommandParameterProperty);
            set => SetValue(SelectTrackCommandParameterProperty, value);
        }

        public static readonly BindableProperty PlayCommandProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PlayCommand),
                typeof(ICommand),
                typeof(IPlayerElement),
                null,
                propertyChanging: OnPlayCommandChanging,
                propertyChanged: OnPlayCommandChanged);

        public ICommand PlayCommand
        {
            get => (ICommand)GetValue(PlayCommandProperty);
            set => SetValue(PlayCommandProperty, value);
        }

        public static readonly BindableProperty PlayCommandParameterProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PlayCommandParameter),
                typeof(object),
                typeof(IPlayerElement),
                null,
                propertyChanged: (bindable, oldvalue, newvalue) => PlayCommandCanExecuteChanged(bindable, EventArgs.Empty));

        public object PlayCommandParameter
        {
            get => GetValue(PlayCommandParameterProperty);
            set => SetValue(PlayCommandParameterProperty, value);
        }

        public static readonly BindableProperty PauseCommandProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PauseCommand),
                typeof(ICommand),
                typeof(IPlayerElement),
                null,
                propertyChanging: OnPauseCommandChanging,
                propertyChanged: OnPauseCommandChanged);

        public ICommand PauseCommand
        {
            get => (ICommand)GetValue(PauseCommandProperty);
            set => SetValue(PauseCommandProperty, value);
        }

        public static readonly BindableProperty PauseCommandParameterProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PauseCommandParameter),
                typeof(object),
                typeof(IPlayerElement),
                null,
                propertyChanged: (bindable, oldvalue, newvalue) => PauseCommandCanExecuteChanged(bindable, EventArgs.Empty));

        public object PauseCommandParameter
        {
            get => GetValue(PauseCommandParameterProperty);
            set => SetValue(PauseCommandParameterProperty, value);
        }

        public static readonly BindableProperty PlayPreviousCommandProperty
           = BindableProperty.Create(
               nameof(IPlayerElement.PlayPreviousCommand),
               typeof(ICommand),
               typeof(IPlayerElement),
               null,
               propertyChanging: OnPlayPreviousCommandChanging,
               propertyChanged: OnPlayPreviousCommandChanged);

        public ICommand PlayPreviousCommand
        {
            get => (ICommand)GetValue(PlayPreviousCommandProperty);
            set => SetValue(PlayPreviousCommandProperty, value);
        }

        public static readonly BindableProperty PlayPreviousCommandParameterProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PlayPreviousCommandParameter),
                typeof(object),
                typeof(IPlayerElement),
                null,
                propertyChanged: (bindable, oldvalue, newvalue) => PlayPreviousCommandCanExecuteChanged(bindable, EventArgs.Empty));

        public object PlayPreviousCommandParameter
        {
            get => GetValue(PlayPreviousCommandParameterProperty);
            set => SetValue(PlayPreviousCommandParameterProperty, value);
        }

        public static readonly BindableProperty PlayNextCommandProperty
           = BindableProperty.Create(
               nameof(IPlayerElement.PlayNextCommand),
               typeof(ICommand),
               typeof(IPlayerElement),
               null,
               propertyChanging: OnPlayNextCommandChanging,
               propertyChanged: OnPlayNextCommandChanged);

        public ICommand PlayNextCommand
        {
            get => (ICommand)GetValue(PlayNextCommandProperty);
            set => SetValue(PlayNextCommandProperty, value);
        }

        public static readonly BindableProperty PlayNextCommandParameterProperty
            = BindableProperty.Create(
                nameof(IPlayerElement.PlayNextCommandParameter),
                typeof(object),
                typeof(IPlayerElement),
                null,
                propertyChanged: (bindable, oldvalue, newvalue) => PlayNextCommandCanExecuteChanged(bindable, EventArgs.Empty));

        public object PlayNextCommandParameter
        {
            get => GetValue(PlayNextCommandParameterProperty);
            set => SetValue(PlayNextCommandParameterProperty, value);
        }

        public static readonly BindableProperty ProgressProperty
                  = BindableProperty.Create(
                      nameof(IPlayerElement.Progress),
                      typeof(double),
                      typeof(IPlayerElement), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly BindableProperty AudioPlayerStateProperty
                    = BindableProperty.Create(
                        nameof(AudioPlayerState),
                        typeof(AudioPlayerState),
                        typeof(AudioPlayer),
                        AudioPlayerState.Closed);

        public static readonly BindableProperty ProgressColorProperty =
            BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(AudioPlayer), default(Color));

        public Color ProgressColor
        {
            get => (Color)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        public AudioPlayerState AudioPlayerState
        {
            get => (AudioPlayerState)GetValue(AudioPlayerStateProperty);
            set => SetValue(AudioPlayerStateProperty, value);
        }

        public static readonly BindableProperty TrackProperty
            = BindableProperty.Create(nameof(Track),
                typeof(Track),
                typeof(AudioPlayer),
                default);

        public Track Track
        {
            get => (Track)GetValue(TrackProperty);
            set => SetValue(TrackProperty, value);
        }


        public static readonly BindableProperty CoverProperty
            = BindableProperty.Create(nameof(Cover),
                typeof(ImageSource),
                typeof(AudioPlayer),
                default(ImageSource));

        public ImageSource Cover
        {
            get => (ImageSource)GetValue(CoverProperty);
            set => SetValue(CoverProperty, value);
        }

        public static readonly BindableProperty IsPlayPreviousEnabledProperty
            = BindableProperty.Create(nameof(IsPlayPreviousEnabled),
                typeof(bool),
                typeof(AudioPlayer),
                default);

        public bool IsPlayPreviousEnabled
        {
            get => (bool)GetValue(IsPlayPreviousEnabledProperty);
            set => SetValue(IsPlayPreviousEnabledProperty, value);
        }

        public static readonly BindableProperty IsPlayNextEnabledProperty
            = BindableProperty.Create(nameof(IsPlayNextEnabled),
                typeof(bool),
                typeof(AudioPlayer),
                default);

        public bool IsPlayNextEnabled
        {
            get => (bool)GetValue(IsPlayNextEnabledProperty);
            set => SetValue(IsPlayNextEnabledProperty, value);
        }

        public void SendPauseClicked()
        {
            PauseCommand?.Execute(PauseCommandParameter);
        }

        public void SendPlayClicked()
        {
            PlayCommand?.Execute(PlayCommandParameter);
        }

        public void SendPlayNextClicked()
        {
            PlayNextCommand?.Execute(PlayNextCommandParameter);
        }

        public void SendPlayPreviousClicked()
        {
            PlayPreviousCommand?.Execute(PlayPreviousCommandParameter);
        }

        public void SendSelectTrackClicked()
        {
            SelectTrackCommand?.Execute(SelectTrackCommandParameter);
        }

        public AudioPlayer()
        {
            InitializeComponent();
        }

        private static void OnSelectTrackCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (newValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += player.OnSelectTrackCommandCanExecuteChanged;
            }
            if (player.PlayCommand != null)
            {
                SelectTrackCommandCanExecuteChanged(player, EventArgs.Empty);
            }
        }

        private static void OnSelectTrackCommandChanging(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (oldValue != null)
            {
                (oldValue as ICommand).CanExecuteChanged -= player.OnSelectTrackCommandCanExecuteChanged;
            }
        }

        private static void SelectTrackCommandCanExecuteChanged(object sender, EventArgs empty)
        {
            if (sender is AudioPlayer control)
            {
                control.SelectTrackCommand?.CanExecute(control.SelectTrackCommandParameter);
            }
        }

        public void OnSelectTrackCommandCanExecuteChanged(object sender, EventArgs e)
        {
            if (sender is AudioPlayer control)
            {
                control.PlayCommand?.CanExecute(control.SelectTrackCommandParameter);
            }
        }

        private static void OnPlayCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (newValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += player.OnPlayCommandCanExecuteChanged;
            }
            if (player.PlayCommand != null)
            {
                PlayCommandCanExecuteChanged(player, EventArgs.Empty);
            }
        }

        private static void OnPlayCommandChanging(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (oldValue != null)
            {
                (oldValue as ICommand).CanExecuteChanged -= player.OnPlayCommandCanExecuteChanged;
            }
        }

        private static void PlayCommandCanExecuteChanged(object sender, EventArgs e)
        {
            if (sender is AudioPlayer control)
            {
                control.PlayCommand?.CanExecute(control.PlayCommandParameter);
            }
        }

        public void OnPlayCommandCanExecuteChanged(object sender, EventArgs e)
        {
            AudioPlayer.PlayCommandCanExecuteChanged(this, EventArgs.Empty);
        }

        private static void OnPauseCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (newValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += player.OnPauseCommandCanExecuteChanged;
            }
            if (player.PauseCommand != null)
            {
                PauseCommandCanExecuteChanged(player, EventArgs.Empty);
            }
        }

        private static void OnPauseCommandChanging(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (oldValue != null)
            {
                (oldValue as ICommand).CanExecuteChanged -= player.OnPauseCommandCanExecuteChanged;
            }
        }

        private static void PauseCommandCanExecuteChanged(object sender, EventArgs e)
        {
            if (sender is AudioPlayer control)
            {
                control.PauseCommand?.CanExecute(control.PauseCommandParameter);
            }
        }

        public void OnPauseCommandCanExecuteChanged(object sender, EventArgs e)
        {
            AudioPlayer.PauseCommandCanExecuteChanged(this, EventArgs.Empty);
        }

        private static void OnPlayPreviousCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (newValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += player.OnPlayPreviousCommandCanExecuteChanged;
            }
            if (player.PlayPreviousCommand != null)
            {
                PlayPreviousCommandCanExecuteChanged(player, EventArgs.Empty);
            }
        }

        private static void OnPlayPreviousCommandChanging(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (oldValue != null)
            {
                (oldValue as ICommand).CanExecuteChanged -= player.OnPlayPreviousCommandCanExecuteChanged;
            }
        }

        private static void PlayPreviousCommandCanExecuteChanged(object sender, EventArgs empty)
        {
            if (sender is AudioPlayer player)
            {
                if (player.PlayPreviousCommand != null)
                {
                    player.IsPlayPreviousEnabled = player.PlayPreviousCommand.CanExecute(player.PlayNextCommandParameter);
                }
            }
        }

        public void OnPlayPreviousCommandCanExecuteChanged(object sender, EventArgs e)
        {
            AudioPlayer.PlayPreviousCommandCanExecuteChanged(this, EventArgs.Empty);
        }

        private static void OnPlayNextCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (newValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += player.OnPlayNextCommandCanExecuteChanged;
            }
            if (player.PlayNextCommand != null)
            {
                PlayNextCommandCanExecuteChanged(player, EventArgs.Empty);
            }
        }

        private static void OnPlayNextCommandChanging(BindableObject bindable, object oldValue, object newValue)
        {
            IPlayerElement player = (IPlayerElement)bindable;
            if (oldValue != null)
            {
                (oldValue as ICommand).CanExecuteChanged -= player.OnPlayNextCommandCanExecuteChanged;
            }
        }

        private static void PlayNextCommandCanExecuteChanged(object sender, EventArgs empty)
        {
            if (sender is AudioPlayer player)
            {
                if (player.PlayNextCommand != null)
                {
                    player.IsPlayNextEnabled = player.PlayNextCommand.CanExecute(player.PlayNextCommandParameter);
                }
            }
        }

        public void OnPlayNextCommandCanExecuteChanged(object sender, EventArgs e)
        {
            AudioPlayer.PlayNextCommandCanExecuteChanged(this, EventArgs.Empty);
        }

    }
}
