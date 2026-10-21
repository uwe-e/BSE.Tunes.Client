using BSE.Tunes.WinUI.Client.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class CreatePlaylistDialogViewModel : ObservableValidator
    {
        private readonly IDataService _dataService;
        private readonly IResourceService _resourceService;
        
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Please enter a playlist name")]
        [MaxLength(100)]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        [NotifyCanExecuteChangedFor(nameof(SavePlaylistCommand))]
        private string _playlistName = string.Empty;

        [ObservableProperty]
        private bool _isValid;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private Playlist? _createdPlaylist;

        [ObservableProperty]
        private bool _cancel;

        public CreatePlaylistDialogViewModel(
            IDataService dataService,
            IResourceService resourceService)
        {
            _dataService = dataService;
            _resourceService = resourceService;
        }

        partial void OnPlaylistNameChanged(string value)
        {
            ValidatePlaylistName();
        }

        private void ValidatePlaylistName()
        {
            ValidateProperty(PlaylistName, nameof(PlaylistName));
            
            var errors = GetErrors(nameof(PlaylistName));
            var errorsList = errors?.Cast<ValidationResult>().ToList();
            
            IsValid = !HasErrors && !string.IsNullOrWhiteSpace(PlaylistName);

            if (errorsList?.Any() == true)
            {
                var firstError = errorsList.First();
                // Override MaxLength error with localized message
                if (PlaylistName?.Length > 100)
                {
                    ErrorMessage = _resourceService.GetString("Playlist_MaxLength_ErrorMessage");
                }
                else
                {
                    ErrorMessage = firstError.ErrorMessage;
                }
            }
            else
            {
                ErrorMessage = null;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSavePlaylist))]
        private async Task SavePlaylistAsync()
        {
            ValidatePlaylistName();
            
            if (!IsValid)
            {
                Cancel = true;
                return;
            }

            try
            {
                CreatedPlaylist = await _dataService.CreatePlaylistAsync(PlaylistName);
                if (CreatedPlaylist == null)
                {
                    Cancel = true;
                    ErrorMessage = "Failed to create playlist";
                }
                else
                {
                    Cancel = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create playlist: {ex.Message}";
                Cancel = true;
            }
        }

        private bool CanSavePlaylist()
        {
            return !string.IsNullOrWhiteSpace(PlaylistName);
        }
    }
}