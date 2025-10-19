using Radzen;
using Transwextions.Data.Models;

namespace Transwextions.App.Components.Modals;

public partial class UsernameInputComponent
{
    private UserModel Model { get; set; } = new();
    private List<string> AvatarImageNamesData { get; set; }
    private int SelectedAvatarIndex { get; set; } = 0;

    public UsernameInputComponent()
    {
        AvatarImageNamesData = GetAvatarImageNamesData();
        Model.AvatarImagePath = AvatarImageNamesData[0];
    }

    private List<string> GetAvatarImageNamesData()
    {
        var result = new List<string>();

        for (int i = 1; i <= 20; i++)
        {
            result.Add($"images/avatars/avatar-{i.ToString()}.png");
        }

        return result;
    }

    private void OnSubmit()
    {
        if (string.IsNullOrWhiteSpace(Model.Username) || Model.Username.Length < 1 || Model.Username.Length > 20)
        {
            _notificationService.Notify(NotificationSeverity.Warning, "Name must be at least 1 character and at most 20 characters.");
            return;
        }

        _dialogService.Close(Model);
    }

    private void OnInvalidSubmit()
    {
        _notificationService.Notify(NotificationSeverity.Warning, "Please correct the errors in the form.");
    }
    private void AvatarCarousel_OnChange(int value)
    {
        SelectedAvatarIndex = value;

        Model.AvatarImagePath = AvatarImageNamesData[SelectedAvatarIndex];
    }
}