namespace BlackBoxBuddy.Services;

public class DialogService : IDialogService
{
    private readonly Func<Avalonia.Controls.Window?> _ownerProvider;

    public DialogService(Func<Avalonia.Controls.Window?> ownerProvider)
        => _ownerProvider = ownerProvider;

    public async Task<bool> ShowConfirmAsync(
        string title, string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        bool isDestructive = false)
    {
        var owner = _ownerProvider();
        if (owner is null) return false;

        var dialog = new Views.ConfirmDialog();
        dialog.SetContent(title, message, confirmText, cancelText, isDestructive);
        return await dialog.ShowDialog<bool>(owner);
    }
}
