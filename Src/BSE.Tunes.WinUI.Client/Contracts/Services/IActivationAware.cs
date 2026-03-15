namespace BSE.Tunes.WinUI.Client.Contracts.Services
{
    public interface IActivationAware
    {
        Task OnActivatedAsync(object? parameter = null);
    }
}
