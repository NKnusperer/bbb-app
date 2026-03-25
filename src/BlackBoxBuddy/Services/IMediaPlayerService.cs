namespace BlackBoxBuddy.Services;

public interface IMediaPlayerService : IDisposable
{
    object CreatePlayer();
    void Play(object player, Uri mediaUri);
    void Pause(object player);
    void Stop(object player);
    void Seek(object player, float position);
    void SetRate(object player, float rate);
    void NextFrame(object player);
    void PreviousFrame(object player);
    void DisposePlayer(object player);
}
