using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels.Provisioning;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class ProvisioningViewModelTests
{
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;
    private readonly ProvisioningViewModel _viewModel;

    public ProvisioningViewModelTests()
    {
        _deviceService = Substitute.For<IDeviceService>();
        _navigationService = Substitute.For<INavigationService>();
        _deviceService.ConnectedDevice.Returns(new DeviceInfo
        {
            DeviceName = "Test Cam",
            FirmwareVersion = "2.0.0",
            IsProvisioned = false
        });
        _viewModel = new ProvisioningViewModel(_deviceService, _navigationService);
    }

    [Fact]
    public void InitialCurrentStep_IsZero()
    {
        _viewModel.CurrentStep.Should().Be(0);
    }

    [Fact]
    public void NextCommand_AdvancesCurrentStepFrom0To1()
    {
        _viewModel.NextCommand.Execute(null);

        _viewModel.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void NextCommand_AdvancesCurrentStepFrom1To2()
    {
        _viewModel.NextCommand.Execute(null); // 0 -> 1
        _viewModel.NextCommand.Execute(null); // 1 -> 2

        _viewModel.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void BackCommand_DecrementsCurrentStepFrom1To0()
    {
        _viewModel.NextCommand.Execute(null); // 0 -> 1

        _viewModel.BackCommand.Execute(null); // 1 -> 0

        _viewModel.CurrentStep.Should().Be(0);
    }

    [Fact]
    public void BackCommand_IsDisabledWhenCurrentStepIsZero()
    {
        _viewModel.BackCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task CompleteCommand_CallsProvisionAsync_WithSelectedWifiSettings()
    {
        _deviceService.ProvisionAsync(Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _navigationService.PopToRootAsync().Returns(Task.CompletedTask);

        _viewModel.SelectedWifiMode = "ap";
        _viewModel.NetworkName = "MyNetwork";
        _viewModel.NetworkPassword = "secret";

        await _viewModel.CompleteCommand.ExecuteAsync(null);

        await _deviceService.Received(1).ProvisionAsync(
            Arg.Is<Dictionary<string, object>>(d =>
                d["wifiMode"].ToString() == "ap" &&
                d["networkName"].ToString() == "MyNetwork" &&
                d["networkPassword"].ToString() == "secret"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteCommand_CallsPopToRootAsync_AfterSuccessfulProvisioning()
    {
        _deviceService.ProvisionAsync(Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _navigationService.PopToRootAsync().Returns(Task.CompletedTask);

        await _viewModel.CompleteCommand.ExecuteAsync(null);

        await _navigationService.Received(1).PopToRootAsync();
    }

    [Fact]
    public void DeviceName_ReturnsConnectedDeviceName()
    {
        _viewModel.DeviceName.Should().Be("Test Cam");
    }

    [Fact]
    public void FirmwareVersion_ReturnsConnectedDeviceFirmwareVersion()
    {
        _viewModel.FirmwareVersion.Should().Be("2.0.0");
    }

    [Fact]
    public void SelectedWifiMode_DefaultsToAp()
    {
        _viewModel.SelectedWifiMode.Should().Be("ap");
    }

    [Fact]
    public void IsWelcomeStep_TrueWhenCurrentStepIsZero()
    {
        _viewModel.IsWelcomeStep.Should().BeTrue();
        _viewModel.IsWifiStep.Should().BeFalse();
        _viewModel.IsConfirmationStep.Should().BeFalse();
    }

    [Fact]
    public void IsWifiStep_TrueWhenCurrentStepIsOne()
    {
        _viewModel.NextCommand.Execute(null);

        _viewModel.IsWelcomeStep.Should().BeFalse();
        _viewModel.IsWifiStep.Should().BeTrue();
        _viewModel.IsConfirmationStep.Should().BeFalse();
    }
}
