using Radzen;
using Transwextions.App.Components.Modals;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.App.Components.Layout;

public partial class MainLayout : IDisposable
{
    protected readonly IUserStateService _userStateService;

    public MainLayout(IUserStateService userStateService)
    {
        _userStateService = userStateService;
        _userStateService.OnChange += OnUserStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Username = _userStateService.Username;
            IsLoggedIn = _userStateService.IsLoggedIn;
            await ShowGetUsernameModal();
            await InvokeAsync(StateHasChanged);
        }
    }
    private bool IsLoggedIn { get; set; } = false;
    private string Username { get; set; } = string.Empty;
    private bool SidebarExpanded = true;

    private string GetWelcomeText()
    {
        var result = "Hello!";

        if (IsLoggedIn && !string.IsNullOrWhiteSpace(Username))
        {
            result = $"Hello, {Username}!";
        }

        return result;
    }

    private async Task ShowGetUsernameModal()
    {
        if (IsLoggedIn)
            return;

        UserModel result = await _dialogService.OpenAsync<UsernameInputComponent>(string.Empty,
        options: new DialogOptions()
        {
            ShowTitle = false
        });

        if (result != null)
        {
            IsLoggedIn = true;
            Username = result.Username;
            _userStateService.Username = result.Username;
            _userStateService.IsLoggedIn = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void PayNowButton_OnClick()
    {
        var result = await _dialogService.OpenAsync<PayNowComponent>("Make a Payment", 
            options: new DialogOptions() 
            { 
               ShowTitle = false
            });
    }

    private void OnUserStateChanged()
    {
        Username = _userStateService.Username;
        IsLoggedIn = _userStateService.IsLoggedIn;
        StateHasChanged();
    }

    public void Dispose()
    {
        _userStateService.OnChange -= OnUserStateChanged;
    }
}