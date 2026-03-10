namespace BSE.Tunes.WinUI.Client.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
