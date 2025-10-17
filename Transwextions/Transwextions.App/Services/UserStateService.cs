using Transwextions.App.Services.Interfaces;

namespace Transwextions.App.Services;

public class UserStateService : IUserStateService
{
    private string? username;

    public string Username
    {
        get => username ?? string.Empty;
        set
        {
            username = value;
            NotifyStateChanged();
        }
    }

    private bool? isLoggedIn;

    public bool IsLoggedIn
    {
        get => isLoggedIn ?? false;
        set
        {
            isLoggedIn = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}