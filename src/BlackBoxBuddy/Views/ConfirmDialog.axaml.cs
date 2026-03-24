using Avalonia.Controls;

namespace BlackBoxBuddy.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public void SetContent(string title, string message,
        string confirmText, string cancelText, bool isDestructive)
    {
        var titleBlock = this.FindControl<TextBlock>("TitleBlock");
        var messageBlock = this.FindControl<TextBlock>("MessageBlock");
        var confirmBtn = this.FindControl<Button>("ConfirmButton");
        var cancelBtn = this.FindControl<Button>("CancelButton");

        if (titleBlock != null) titleBlock.Text = title;
        if (messageBlock != null) messageBlock.Text = message;
        if (confirmBtn != null)
        {
            confirmBtn.Content = confirmText;
            confirmBtn.Classes.Clear();
            confirmBtn.Classes.Add(isDestructive ? "destructive" : "accent");
            confirmBtn.Click += (_, _) => Close(true);
        }
        if (cancelBtn != null)
        {
            cancelBtn.Content = cancelText;
            cancelBtn.Click += (_, _) => Close(false);
        }

        Title = title;
    }
}
