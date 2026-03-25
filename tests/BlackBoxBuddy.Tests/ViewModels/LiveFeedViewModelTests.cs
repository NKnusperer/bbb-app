using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class LiveFeedViewModelTests
{
    private readonly IDashcamDevice _device;
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IDeviceService _deviceService;
    private readonly object _fakePlayer = new();

    public LiveFeedViewModelTests()
    {
        _device = Substitute.For<IDashcamDevice>();
        _mediaPlayerService = Substitute.For<IMediaPlayerService>();
        _deviceService = Substitute.For<IDeviceService>();

        _device.GetStreamUriAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Uri("rtsp://192.168.0.1/front"));
        _mediaPlayerService.CreatePlayer().Returns(_fakePlayer);
        _deviceService.ConnectionState.Returns(ConnectionState.Connected);
    }

    private LiveFeedViewModel CreateSut() =>
        new(_device, _mediaPlayerService, _deviceService);

    [Fact]
    public async Task StartLiveFeed_CallsGetStreamUriWithSelectedCamera()
    {
        var sut = CreateSut();
        sut.SelectedCamera = "rear";

        await sut.StartLiveFeedAsync();

        await _device.Received(1).GetStreamUriAsync("rear", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartLiveFeed_DefaultCameraIsFront()
    {
        var sut = CreateSut();

        await sut.StartLiveFeedAsync();

        await _device.Received(1).GetStreamUriAsync("front", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartLiveFeed_CreatesPlayerAndPlays()
    {
        var sut = CreateSut();

        await sut.StartLiveFeedAsync();

        _mediaPlayerService.Received(1).CreatePlayer();
        _mediaPlayerService.Received(1).Play(_fakePlayer, Arg.Any<Uri>());
    }

    [Fact]
    public async Task StartLiveFeed_SetsIsLoadingTrueThenFalse()
    {
        var loadingStates = new List<bool>();
        var sut = CreateSut();
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LiveFeedViewModel.IsLoading))
                loadingStates.Add(sut.IsLoading);
        };

        await sut.StartLiveFeedAsync();

        loadingStates.Should().ContainInOrder(true, false);
    }

    [Fact]
    public async Task StartLiveFeed_WhenUriIsNull_SetsIsConnectionLost()
    {
        _device.GetStreamUriAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Uri?)null);
        var sut = CreateSut();

        await sut.StartLiveFeedAsync();

        sut.IsConnectionLost.Should().BeTrue();
        sut.IsStreamActive.Should().BeFalse();
    }

    [Fact]
    public async Task StartLiveFeed_WhenUriIsNull_IsLoadingFalse()
    {
        _device.GetStreamUriAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Uri?)null);
        var sut = CreateSut();

        await sut.StartLiveFeedAsync();

        sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void StopLiveFeed_StopsPlayer()
    {
        var sut = CreateSut();
        // Set up internal player via reflection workaround: just expose through Player property
        // We need to start feed first to get a player created
        _ = sut.StartLiveFeedAsync(); // fire and forget to set up player

        sut.StopLiveFeed();

        _mediaPlayerService.Received().Stop(Arg.Any<object>());
    }

    [Fact]
    public async Task StopLiveFeed_SetsIsStreamActiveFalse()
    {
        var sut = CreateSut();
        await sut.StartLiveFeedAsync();

        sut.StopLiveFeed();

        sut.IsStreamActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleCamera_SetsSelectedCameraAndRestarts()
    {
        var sut = CreateSut();

        await sut.ToggleCameraAsync("rear");

        sut.SelectedCamera.Should().Be("rear");
        await _device.Received().GetStreamUriAsync("rear", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retry_CallsStartLiveFeedAgain()
    {
        _device.GetStreamUriAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Uri?)null);
        var sut = CreateSut();
        await sut.StartLiveFeedAsync(); // sets IsConnectionLost

        await sut.RetryAsync();

        await _device.Received(2).GetStreamUriAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispose_StopsStreamAndDisposesPlayer()
    {
        var sut = CreateSut();
        await sut.StartLiveFeedAsync();

        sut.Dispose();

        _mediaPlayerService.Received(1).Stop(Arg.Any<object>());
        _mediaPlayerService.Received(1).DisposePlayer(_fakePlayer);
    }
}
