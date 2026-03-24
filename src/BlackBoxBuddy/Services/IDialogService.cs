namespace BlackBoxBuddy.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        bool isDestructive = false);
}
