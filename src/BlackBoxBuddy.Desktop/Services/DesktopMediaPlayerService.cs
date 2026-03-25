using BlackBoxBuddy.Services;
using LibVLCSharp.Shared;

namespace BlackBoxBuddy.Desktop.Services;

/// <summary>
/// LibVLCSharp implementation of IMediaPlayerService for desktop platforms.
/// Registered as a singleton in Program.cs platform services.
/// </summary>
public class DesktopMediaPlayerService : IMediaPlayerService
{
    private readonly LibVLC _libVlc;

    public DesktopMediaPlayerService()
    {
        Core.Initialize(); // Required LibVLCSharp initialization — must be called before any LibVLC usage
        _libVlc = new LibVLC("--no-video-title-show");
    }

    public object CreatePlayer() => new MediaPlayer(_libVlc);

    public void Play(object player, Uri mediaUri)
    {
        if (player is MediaPlayer mp)
        {
            var media = new Media(_libVlc, mediaUri);
            mp.Media = media;
            mp.Play();
        }
    }

    public void Pause(object player)
    {
        if (player is MediaPlayer mp) mp.Pause();
    }

    public void Stop(object player)
    {
        if (player is MediaPlayer mp) mp.Stop();
    }

    public void Seek(object player, float position)
    {
        if (player is MediaPlayer mp) mp.Position = position;
    }

    public void SetRate(object player, float rate)
    {
        if (player is MediaPlayer mp) mp.SetRate(rate);
    }

    public void NextFrame(object player)
    {
        if (player is MediaPlayer mp) mp.NextFrame();
    }

    public void PreviousFrame(object player)
    {
        // LibVLC does not have a PreviousFrame API.
        // Implement as seek back ~33ms (one frame at 30fps).
        if (player is MediaPlayer mp && mp.Time > 33)
            mp.Time -= 33;
    }

    public void DisposePlayer(object player)
    {
        if (player is MediaPlayer mp) mp.Dispose();
    }

    public void Dispose() => _libVlc.Dispose();
}
