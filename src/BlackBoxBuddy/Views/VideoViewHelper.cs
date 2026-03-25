using Avalonia.Controls;

namespace BlackBoxBuddy.Views;

/// <summary>
/// Creates and wires the Desktop-only VideoView (NativeControlHost) via reflection.
/// VideoView lives in BlackBoxBuddy.Desktop.Controls — the shared project cannot
/// reference it at compile time. Both LiveFeedPage and RecordingDetailPage use this.
/// </summary>
public static class VideoViewHelper
{
    private const string VideoViewTypeName =
        "BlackBoxBuddy.Desktop.Controls.VideoView, BlackBoxBuddy.Desktop";

    /// <summary>
    /// Creates a VideoView and places it inside the given host ContentControl.
    /// Returns the VideoView instance (as Control) or null if not on Desktop.
    /// </summary>
    public static Control? CreateInHost(ContentControl host)
    {
        if (host.Content is Control existing) return existing;

        var type = Type.GetType(VideoViewTypeName);
        if (type is null) return null;

        var videoView = Activator.CreateInstance(type) as Control;
        if (videoView is null) return null;

        host.Content = videoView;
        return videoView;
    }

    /// <summary>
    /// Sets the MediaPlayer property on a VideoView instance via reflection.
    /// Both the VideoView and player are typed as object to avoid compile-time
    /// dependency on LibVLCSharp from the shared project.
    /// </summary>
    public static void SetMediaPlayer(object? videoView, object? player)
    {
        if (videoView is null || player is null) return;
        videoView.GetType().GetProperty("MediaPlayer")?.SetValue(videoView, player);
    }
}
