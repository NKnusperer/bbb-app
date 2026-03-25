// Ported from LibVLCSharp 3.x branch.
// Original: https://code.videolan.org/videolan/LibVLCSharp/-/blob/3.x/src/LibVLCSharp.Avalonia/VideoView.cs
// Changes:
//   - Namespace: LibVLCSharp.Avalonia → BlackBoxBuddy.Desktop.Controls
//   - VisualRoot → TopLevel.GetTopLevel(this) (Avalonia 12 RC1 API)
//   - Removed [Content] attribute and Avalonia.Metadata using (causes CS0246 on Avalonia 12)

using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace BlackBoxBuddy.Desktop.Controls;

/// <summary>
/// Avalonia 12 compatible VideoView for LibVLCSharp 3.x.
/// Replaces the LibVLCSharp.Avalonia package VideoView with Avalonia 12-compatible API calls.
/// </summary>
public class VideoView : NativeControlHost
{
    private IPlatformHandle? _platformHandle;
    private MediaPlayer? _mediaPlayer;
    private object? _videoOverlay;

    public static readonly DirectProperty<VideoView, MediaPlayer?> MediaPlayerProperty =
        AvaloniaProperty.RegisterDirect<VideoView, MediaPlayer?>(
            nameof(MediaPlayer),
            o => o.MediaPlayer,
            (o, v) => o.MediaPlayer = v);

    /// <summary>Gets or sets the LibVLC MediaPlayer to display in this view.</summary>
    public MediaPlayer? MediaPlayer
    {
        get => _mediaPlayer;
        set
        {
            if (_mediaPlayer == value) return;
            var oldPlayer = _mediaPlayer;
            SetAndRaise(MediaPlayerProperty, ref _mediaPlayer, value);
            if (oldPlayer is not null)
                DetachPlayer(oldPlayer);
            if (value is not null && _platformHandle is not null)
                AttachPlayer(value, _platformHandle);
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _platformHandle = base.CreateNativeControlCore(parent);
        if (_mediaPlayer is not null)
            AttachPlayer(_mediaPlayer, _platformHandle);
        return _platformHandle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_mediaPlayer is not null)
            DetachPlayer(_mediaPlayer);
        base.DestroyNativeControlCore(control);
        _platformHandle = null;
    }

    private void AttachPlayer(MediaPlayer player, IPlatformHandle handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            player.Hwnd = handle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            player.XWindow = (uint)handle.Handle.ToInt64();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            player.NsObject = handle.Handle;
    }

    private void DetachPlayer(MediaPlayer player)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            player.Hwnd = IntPtr.Zero;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            player.XWindow = 0;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            player.NsObject = IntPtr.Zero;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InitializeNativeOverlay();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window visualRoot)
            visualRoot.PositionChanged -= OnParentWindowPositionChanged;
    }

    private void InitializeNativeOverlay()
    {
        if (TopLevel.GetTopLevel(this) is not Window visualRoot) return;
        visualRoot.PositionChanged += OnParentWindowPositionChanged;
    }

    private void ShowNativeOverlay()
    {
        if (TopLevel.GetTopLevel(this) is not Window) return;
    }

    private void OnParentWindowPositionChanged(object? sender, PixelPointEventArgs e)
    {
        Dispatcher.UIThread.Post(ShowNativeOverlay, DispatcherPriority.Render);
    }
}
