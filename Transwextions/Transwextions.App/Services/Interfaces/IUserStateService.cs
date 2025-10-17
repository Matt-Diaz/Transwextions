namespace Transwextions.App.Services.Interfaces;

public interface IUserStateService
{
    public event Action? OnChange;
    public string Username { get; set; }
    public bool IsLoggedIn { get; set; }
}