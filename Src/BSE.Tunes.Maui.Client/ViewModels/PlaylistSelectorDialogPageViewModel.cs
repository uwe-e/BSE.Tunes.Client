        using BSE.Tunes.Maui.Client.Events;
        using BSE.Tunes.Maui.Client.Models;
        using BSE.Tunes.Maui.Client.Models.Contract;
        using BSE.Tunes.Maui.Client.Services;
        using System.Collections.ObjectModel;
        using System.Windows.Input;

        namespace BSE.Tunes.Maui.Client.ViewModels
        {
            public class PlaylistSelectorDialogPageViewModel(
                INavigationService navigationService,
                IDataService dataService,
                IImageService imageService,
                IEventAggregator eventAggregator) : ViewModelBase(navigationService)
            {
                private int _pageSize;
                private int _pageNumber;
                private bool _hasItems;
                private ObservableCollection<FlyoutItemViewModel> _playlistFlyoutItems;
                private PlaylistActionContext _playlistActionContext;
                private ICommand _cancelCommand;
                private ICommand _openNewPlaylistDialogCommand;
                private ICommand _remainingItemsThresholdReachedCommand;
                private readonly IDataService _dataService = dataService;
                private readonly IImageService _imageService = imageService;
                private readonly IEventAggregator _eventAggregator = eventAggregator;

                public ICommand RemainingItemsThresholdReachedCommand => _remainingItemsThresholdReachedCommand ??= new DelegateCommand(async () =>
                {
                    if (IsBusy || !_hasItems)
                    {
                        return;
                    }

                    await FetchPlaylistFlyoutItemsAsync();
                });
                public ICommand CancelCommand =>
                    _cancelCommand ??= new DelegateCommand(async () => await CloseDialog());

                public ICommand OpenNewPlaylistDialogCommand =>
                    _openNewPlaylistDialogCommand ??= new DelegateCommand(async () => await NewPlaylistDialog());

                public virtual ObservableCollection<FlyoutItemViewModel> PlaylistFlyoutItems =>
                    _playlistFlyoutItems ??= [];

                public override async void OnNavigatedTo(INavigationParameters parameters)
                {
                    _playlistActionContext = parameters.GetValue<PlaylistActionContext>("source");
            
                    _pageSize = 20;
                    _pageNumber = 1;
                    _hasItems = true;
                    IsBusy = false;
                    await FetchPlaylistFlyoutItemsAsync();

                    base.OnNavigatedTo(parameters);
                }

                private async Task FetchPlaylistFlyoutItemsAsync()
                {
                    if (IsBusy || !_hasItems)
                    {
                        return;
                    }

                    IsBusy = true;
                    try
                    {
                        PagedResult<Playlist> pagedResult = await _dataService.GetPagedPlaylistsByOwnerAsync(_pageNumber, _pageSize);
                        if (pagedResult?.Items == null || pagedResult.Items.Count == 0)
                        {
                            _hasItems = false;
                            return;
                        }

                        bool isLastPage = pagedResult.TotalPages == _pageNumber;
                        _hasItems = !isLastPage;

                        if (pagedResult.HasNextPage)
                        {
                            _pageNumber++;
                        }

                        int itemCount = pagedResult.Items.Count;
                        for (int i = 0; i < itemCount; i++)
                        {
                            Playlist playlist = pagedResult.Items[i];
                            if (playlist != null)
                            {
                                var flyoutItem = new FlyoutItemViewModel
                                {
                                    Text = playlist.Name,
                                    ImageSource = await _imageService.GetStitchedBitmapSourceAsync(playlist.Id, playlist.CoverAlbumIds, 50, true),
                                    Data = playlist
                                };
                                flyoutItem.ItemClicked += OnFlyoutItemClicked;
                                PlaylistFlyoutItems.Add(flyoutItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        if (_pageNumber > 1)
                        {
                            _pageNumber--; // Rollback on failure during lazy loading
                        }
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }

                private async void OnFlyoutItemClicked(object sender, EventArgs e)
                {
                    if (sender is FlyoutItemViewModel flyoutItem)
                    {
                        await CloseDialog();

                        _playlistActionContext.PlaylistTo = flyoutItem.Data as Playlist;
                        _playlistActionContext.ActionMode = PlaylistActionMode.AddToPlaylist;
                        _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
                    }
                }

                private async Task CloseDialog()
                {
                    var navigationParams = new NavigationParameters
                    {
                        { KnownNavigationParameters.UseModalNavigation, true}
                    };
                    await NavigationService.GoBackAsync(navigationParams);
                }

                private async Task NewPlaylistDialog()
                {
                    await CloseDialog();

                    _playlistActionContext.ActionMode = PlaylistActionMode.CreatePlaylist;
                    _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
                }
            }
        }
