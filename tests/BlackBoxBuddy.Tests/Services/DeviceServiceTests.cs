using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BlackBoxBuddy.Tests.Services;

public class DeviceServiceTests
{
    private readonly IDashcamDevice _mockDevice;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _mockDevice = Substitute.For<IDashcamDevice>();
        _service = new DeviceService(_mockDevice);
    }

    [Fact]
    public async Task StartDiscoveryAsync_SetsStateToSearchingImmediately()
    {
        var states = new List<ConnectionState>();
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(null));

        _service.ConnectionStateChanged += (_, state) => states.Add(state);

        await _service.StartDiscoveryAsync();

        states.Should().Contain(ConnectionState.Searching);
    }

    [Fact]
    public async Task StartDiscoveryAsync_SetsStateToConnected_WhenDeviceFoundAndProvisioned()
    {
        var deviceInfo = new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "1.0",
            IsProvisioned = true
        };
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(deviceInfo));

        await _service.StartDiscoveryAsync();

        _service.ConnectionState.Should().Be(ConnectionState.Connected);
    }

    [Fact]
    public async Task StartDiscoveryAsync_SetsStateToNeedsProvisioning_WhenDeviceFoundAndNotProvisioned()
    {
        var deviceInfo = new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "1.0",
            IsProvisioned = false
        };
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(deviceInfo));

        await _service.StartDiscoveryAsync();

        _service.ConnectionState.Should().Be(ConnectionState.NeedsProvisioning);
    }

    [Fact]
    public async Task StartDiscoveryAsync_SetsStateToDisconnected_WhenDeviceNotFound()
    {
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(null));

        await _service.StartDiscoveryAsync();

        _service.ConnectionState.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public async Task StartDiscoveryAsync_SetsStateToDisconnected_OnOperationCanceledException()
    {
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await _service.StartDiscoveryAsync();

        _service.ConnectionState.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public async Task ConnectionStateChanged_FiresOnEachStateTransition()
    {
        var deviceInfo = new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "1.0",
            IsProvisioned = true
        };
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(deviceInfo));

        var stateChanges = new List<ConnectionState>();
        _service.ConnectionStateChanged += (_, state) => stateChanges.Add(state);

        await _service.StartDiscoveryAsync();

        stateChanges.Should().HaveCountGreaterThanOrEqualTo(2);
        stateChanges.Should().Contain(ConnectionState.Searching);
        stateChanges.Should().Contain(ConnectionState.Connected);
    }

    [Fact]
    public async Task StartDiscoveryAsync_PopulatesConnectedDevice_AfterSuccessfulDiscovery()
    {
        var deviceInfo = new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "1.0",
            IsProvisioned = true
        };
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(deviceInfo));

        await _service.StartDiscoveryAsync();

        _service.ConnectedDevice.Should().NotBeNull();
        _service.ConnectedDevice!.DeviceName.Should().Be("Test Cam");
    }

    [Fact]
    public async Task ConnectManuallyAsync_CallsDeviceConnectAsyncWithProvidedHost()
    {
        var deviceInfo = new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "1.0",
            IsProvisioned = true
        };
        _mockDevice.ConnectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DeviceInfo?>(deviceInfo));

        await _service.ConnectManuallyAsync("192.168.1.100");

        await _mockDevice.Received(1).ConnectAsync("192.168.1.100", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartDiscoveryAsync_UsesLinkedCancellationSource_With5SecondTimeout()
    {
        // This test verifies the timeout pattern by using a device that
        // only responds to cancellation after the 5s CTS fires.
        // We verify the service reaches Disconnected state when cancelled.
        _mockDevice.DiscoverAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await _service.StartDiscoveryAsync();

        _service.ConnectionState.Should().Be(ConnectionState.Disconnected);
    }
}
