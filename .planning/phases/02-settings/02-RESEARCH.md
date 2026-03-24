# Phase 2: Settings - Research

**Researched:** 2026-03-24
**Domain:** Avalonia 12 RC1 settings UI, MVVM dirty-state tracking, typed device models, DI refactor, dialog patterns
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Strongly-typed C# models per settings category (records/classes with enums): `WifiSettings`, `RecordingSettings`, `ChannelSettings`, `CameraSettings`, `SensorSettings`, `SystemSettings`, `OverlaySettings`
- **D-02:** `IDeviceCommands` interface changes to use strongly-typed models directly — drop the `Dictionary<string, object>` approach for both `GetSettingsAsync` and `ApplySettingsAsync`
- **D-03:** MockDashcamDevice must be updated to return/accept the new typed models with realistic default values for all settings categories
- **D-04:** Single scrollable page with visual section headers per category (WiFi, Recording, Channels, Camera, Sensors, System, Overlays, Danger Zone)
- **D-05:** All sections visible by scrolling — no accordion, no sub-pages
- **D-06:** Danger Zone section visually separated at the bottom with distinct styling to signal destructiveness
- **D-07:** All settings loaded at once via a single call on Settings page entry
- **D-08:** Typed models mapped from device response and bound to ViewModel properties
- **D-09:** Explicit Save button — sticky/floating at bottom of the page
- **D-10:** Save button only visible when there are unsaved changes (dirty state tracking)
- **D-11:** All pending changes batched into a single `ApplySettingsAsync` call
- **D-12:** Success/failure feedback shown after save attempt
- **D-13:** If user navigates away with unsaved changes, show a warning dialog: "You have unsaved settings." with Discard / Save & Leave options
- **D-14:** Factory reset and SD card wipe use confirmation dialogs with clear warning text, Cancel and destructive-action Confirm buttons
- **D-15:** `IDeviceCommands` needs a new `WipeSdCardAsync` method (currently only has `FactoryResetAsync`)
- **D-16:** No static service locator calls (`Ioc.Default.GetRequiredService`) in Views or ViewModels — use proper constructor-based DI everywhere
- **D-17:** Existing Phase 1 code that uses `Ioc.Default.GetRequiredService` in View constructors must be refactored as part of this phase
- **D-18:** Comprehensive test coverage for every unit with meaningful logic
- **D-19:** Tests must cover ValueConverters, Views (via Avalonia.Headless.XUnit), and Controls in addition to ViewModels and Services
- **D-20:** Automated testing is a priority — no untested business logic

### Claude's Discretion

- Exact section header styling and spacing
- Control types for each setting (ComboBox, ToggleSwitch, Slider, RadioButton, etc.)
- Settings mapper implementation details (extension methods, dedicated service, etc.)
- Loading indicator while settings are being fetched
- Error handling when device returns unexpected/missing settings
- Exact dialog wording for danger zone confirmations

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| WIFI-01 | User can configure WiFi band (2.4 GHz or 5 GHz) | `WifiSettings.Band` enum with `TwoPointFourGHz` / `FiveGHz`; ComboBox or RadioButton in XAML |
| WIFI-02 | User can connect device to an existing access point | `WifiSettings.Mode = ClientMode`; network name + password fields |
| WIFI-03 | User can configure device to act as an access point | `WifiSettings.Mode = AccessPointMode`; custom SSID field |
| RMOD-01 | User can set driving mode to Standard | `RecordingSettings.DrivingMode` enum; `Standard` = front+rear 30fps |
| RMOD-02 | User can set driving mode to Racing | `RecordingSettings.DrivingMode.Racing` = front-only 60fps |
| RMOD-03 | User can set parking mode to Standard | `RecordingSettings.ParkingMode.Standard` = radar monitoring, medium sensitivity |
| RMOD-04 | User can set parking mode to Event Only | `RecordingSettings.ParkingMode.EventOnly` = vibration-triggered |
| CHAN-01 | User can configure recording to front camera only | `ChannelSettings.Channels` enum; `FrontOnly` |
| CHAN-02 | User can configure recording to front and rear cameras | `ChannelSettings.Channels.FrontAndRear` |
| CAMR-01 | User can set rear camera orientation to 0 degrees | `CameraSettings.RearOrientation` enum; `Normal` |
| CAMR-02 | User can set rear camera orientation to 180 degrees | `CameraSettings.RearOrientation.Flipped` |
| SENS-01 | User can configure driving shock sensor sensitivity (1-5) | `SensorSettings.DrivingShockSensitivity`; Slider 1-5 or NumericUpDown |
| SENS-02 | User can configure parking shock sensor sensitivity (1-5) | `SensorSettings.ParkingShockSensitivity` |
| SENS-03 | User can configure radar sensor sensitivity (1-5) | `SensorSettings.RadarSensitivity` |
| SYST-01 | User can enable/disable GPS | `SystemSettings.GpsEnabled`; ToggleSwitch |
| SYST-02 | User can enable/disable microphone | `SystemSettings.MicrophoneEnabled`; ToggleSwitch |
| SYST-03 | User can set speaker volume (disabled, 1-5) | `SystemSettings.SpeakerVolume`; 0 = disabled, Slider 0-5 |
| OVRL-01 | User can enable/disable date overlay | `OverlaySettings.DateEnabled`; ToggleSwitch |
| OVRL-02 | User can enable/disable time overlay | `OverlaySettings.TimeEnabled`; ToggleSwitch |
| OVRL-03 | User can enable/disable GPS position overlay | `OverlaySettings.GpsPositionEnabled`; ToggleSwitch |
| OVRL-04 | User can enable/disable speed overlay | `OverlaySettings.SpeedEnabled`; ToggleSwitch |
| OVRL-05 | User can choose speed display unit (km/h or mph) | `OverlaySettings.SpeedUnit` enum; `KilometersPerHour` / `MilesPerHour` |
| DNGR-01 | User can perform factory reset on device | `IDeviceCommands.FactoryResetAsync` (exists); confirmation dialog required |
| DNGR-02 | User can wipe SD card | `IDeviceCommands.WipeSdCardAsync` (new method); confirmation dialog required |
</phase_requirements>

---

## Summary

Phase 2 builds out the Settings page from its current placeholder state into a fully functional, single-scroll settings form covering 7 categories plus a Danger Zone. The primary technical challenges are: (1) refactoring `IDeviceCommands` from `Dictionary<string, object>` to typed models, (2) implementing dirty-state tracking with a floating Save button, (3) integrating an `IDialogService` pattern for confirmation dialogs (danger zone and unsaved-changes guard), and (4) fixing the cross-cutting DI antipattern where 4 View constructors call `Ioc.Default.GetRequiredService`.

The architecture is MVVM-pure throughout: `SettingsViewModel` holds all pending-change properties, tracks dirty state, and delegates device I/O via `IDeviceService` → `IDashcamDevice` → `IDeviceCommands`. The View is a `ContentPage` wrapping a `Grid` with a `ScrollViewer` body and a sticky Save bar at the bottom — positioned outside the scroll area using a `Grid.RowDefinitions` split, not a floating overlay.

No new NuGet packages are needed for this phase. All required functionality is covered by the existing stack (CommunityToolkit.Mvvm, Avalonia 12 RC1 built-in controls, standard .NET dialog patterns). Confirmation dialogs use a thin `IDialogService` abstraction so ViewModels remain testable without Avalonia UI thread in unit tests.

**Primary recommendation:** Model every settings category as an immutable C# `record` with enums for all discrete choices. Store the "loaded" snapshot and a "pending" copy in the ViewModel; dirty state is `pending != loaded`. On Save, apply pending via device, then update loaded to match.

---

## Standard Stack

### Core (already in project — no new packages needed)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CommunityToolkit.Mvvm | 8.4.1 | `[ObservableProperty]`, `[RelayCommand]`, `[NotifyCanExecuteChangedFor]` | Locked by CLAUDE.md |
| Avalonia 12 RC1 | 12.0.0-rc1 | All UI controls (ToggleSwitch, Slider, ComboBox, ScrollViewer, Grid) | Locked by CLAUDE.md |
| xunit.v3 mtp-v2 | current | Test runner | Locked by CLAUDE.md |
| NSubstitute | 5.3.0 | Mock `IDeviceCommands`, `IDialogService` in ViewModel tests | Already referenced |
| FluentAssertions | 8.9.0 | BDD assertions | Already referenced |
| Bogus | 35.6.5 | Realistic settings data in test builders | Already referenced |
| Avalonia.Headless.XUnit | current | View/converter tests with `[AvaloniaFact]` | Already referenced |

**No new packages required for this phase.**

### Supporting Controls (Avalonia 12 built-in)

| Control | Avalonia Class | Usage |
|---------|----------------|-------|
| Toggle | `ToggleSwitch` | GPS, Microphone, Overlay toggles (SYST-01/02, OVRL-01..04) |
| Segmented/Radio | `RadioButton` | Driving mode, Parking mode, WiFi band, Channels, Camera orientation |
| Slider | `Slider` | Sensor sensitivity 1-5 (SENS-01..03), Speaker volume 0-5 (SYST-03) |
| Dropdown | `ComboBox` | Speed unit (OVRL-05), WiFi band alternative |
| Scrollable body | `ScrollViewer` | Page body wrapping all sections |
| Sticky row | `Grid` (outer) | Row 0 = ScrollViewer body, Row 1 = Save bar |
| Dialog | `Window.ShowDialog<bool>()` | Danger zone confirmations, unsaved changes guard |

---

## Architecture Patterns

### Recommended Project Structure

```
src/BlackBoxBuddy/
├── Models/
│   ├── Settings/
│   │   ├── WifiSettings.cs         # record with Band enum, Mode enum, Ssid, Password
│   │   ├── RecordingSettings.cs    # record with DrivingMode enum, ParkingMode enum
│   │   ├── ChannelSettings.cs      # record with Channels enum
│   │   ├── CameraSettings.cs       # record with RearOrientation enum
│   │   ├── SensorSettings.cs       # record with DrivingShock/ParkingShock/Radar int (1-5)
│   │   ├── SystemSettings.cs       # record with GpsEnabled, MicEnabled, SpeakerVolume
│   │   └── OverlaySettings.cs      # record with bools + SpeedUnit enum
├── Device/
│   └── IDeviceCommands.cs          # REFACTORED: typed GetSettings / ApplySettings / WipeSdCard
├── Services/
│   └── IDialogService.cs           # new: ShowConfirmAsync(title, message) → bool
├── ViewModels/
│   └── SettingsViewModel.cs        # BUILT OUT: all settings props, dirty tracking, save command
└── Views/
    └── SettingsPage.axaml          # BUILT OUT: scrollable sections, sticky save bar
```

```
tests/BlackBoxBuddy.Tests/
├── ViewModels/
│   └── SettingsViewModelTests.cs
├── Device/
│   └── MockDashcamDeviceTests.cs   # extended with typed settings coverage
└── Converters/
    └── (any new converters go here)
```

### Pattern 1: Typed Settings Records

Define each settings category as a C# `record` with enums for discrete choices. Records provide structural equality for free — dirty state is `_pending != _loaded`.

```csharp
// Source: C# 9+ records — value equality by default
public enum WifiBand { TwoPointFourGHz, FiveGHz }
public enum WifiMode  { AccessPoint, Client }

public record WifiSettings(
    WifiBand Band,
    WifiMode Mode,
    string Ssid,
    string Password);

public record SensorSettings(
    int DrivingShockSensitivity,   // 1-5
    int ParkingShockSensitivity,   // 1-5
    int RadarSensitivity);         // 1-5
```

### Pattern 2: SettingsViewModel Dirty State

Store a `_loaded` snapshot (what the device currently has) and a set of `[ObservableProperty]` fields for pending edits. Dirty = any pending value differs from loaded.

```csharp
// Source: CommunityToolkit.Mvvm ObservableObject pattern
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDialogService _dialogService;

    // Loaded snapshot — set after successful GetSettings
    private WifiSettings? _loadedWifi;
    private SensorSettings? _loadedSensors;
    // ... other loaded snapshots

    // Pending edit properties (bound to controls)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private WifiBand _wifiBand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private int _drivingShockSensitivity = 3;

    // ... (one [ObservableProperty] per individual setting)

    public bool HasUnsavedChanges =>
        _loadedWifi is not null && (
            WifiBand != _loadedWifi.Band ||
            WifiMode != _loadedWifi.Mode /* ... */);

    [RelayCommand(CanExecute = nameof(HasUnsavedChanges))]
    private async Task SaveAsync() { /* batch apply */ }
}
```

> Important: `[NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]` ensures the Save button's `CanExecute` re-evaluates whenever any setting changes. This requires Save command to also use `[NotifyCanExecuteChangedFor]` or call `SaveCommand.NotifyCanExecuteChanged()` from `HasUnsavedChanges`.

**Alternative dirty tracking approach** (simpler for many properties): override `OnPropertyChanged` in the VM and set a `bool _isDirty` flag there, then expose as `HasUnsavedChanges`. This avoids per-property `[NotifyPropertyChangedFor]` attributes and is cleaner when there are 20+ settings properties.

```csharp
private bool _isDirty;
public bool HasUnsavedChanges => _isDirty && _loaded;

protected override void OnPropertyChanged(PropertyChangedEventArgs e)
{
    base.OnPropertyChanged(e);
    if (_loaded && e.PropertyName != nameof(HasUnsavedChanges) && /* guard list */)
    {
        _isDirty = true;
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
```

### Pattern 3: Sticky Save Bar Layout

`ScrollViewer` clips its content — placing a button "over" it from inside doesn't work. Use a parent `Grid` with two rows: the scroll body and a fixed-height Save row beneath it.

```xml
<!-- Source: Avalonia ScrollViewer docs — sticky elements must live outside ScrollViewer -->
<Grid RowDefinitions="*,Auto">
    <!-- Row 0: scrollable settings body -->
    <ScrollViewer Grid.Row="0">
        <StackPanel Margin="16" Spacing="24">
            <!-- WiFi section, Recording section, ... Danger Zone section -->
        </StackPanel>
    </ScrollViewer>

    <!-- Row 1: sticky Save bar — only visible when dirty (D-09, D-10) -->
    <Border Grid.Row="1"
            IsVisible="{Binding HasUnsavedChanges}"
            Background="{DynamicResource SystemControlBackgroundAltHighBrush}"
            Padding="16,8">
        <Button Content="Save Changes"
                Command="{Binding SaveCommand}"
                HorizontalAlignment="Right"
                Classes="accent"/>
    </Border>
</Grid>
```

### Pattern 4: IDialogService for Testable Dialogs

ViewModels must not call `Window.ShowDialog<>()` directly — that couples them to the UI thread and makes them untestable. Define a thin service interface:

```csharp
// Source: Avalonia dialog best-practice from official docs (service-based pattern)
public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel");
}
```

The production implementation creates and shows a `Window` child:

```csharp
public class DialogService : IDialogService
{
    private readonly Func<Window?> _ownerProvider;
    public DialogService(Func<Window?> ownerProvider) => _ownerProvider = ownerProvider;

    public async Task<bool> ShowConfirmAsync(string title, string message, string confirmText, string cancelText)
    {
        var dlg = new ConfirmDialog { Title = title, Message = message,
                                      ConfirmText = confirmText, CancelText = cancelText };
        var owner = _ownerProvider();
        if (owner is null) return false;
        return await dlg.ShowDialog<bool>(owner);
    }
}
```

In tests, substitute `IDialogService` with NSubstitute and return `true` or `false` to test both confirmation paths.

### Pattern 5: Danger Zone Usage

```csharp
[RelayCommand]
private async Task FactoryResetAsync()
{
    var confirmed = await _dialogService.ShowConfirmAsync(
        "Factory Reset",
        "This will erase all device settings and cannot be undone.",
        confirmText: "Reset Device",
        cancelText: "Cancel");

    if (!confirmed) return;

    IsLoading = true;
    var success = await _device.FactoryResetAsync();
    IsLoading = false;
    // Show success/error feedback
}
```

### Pattern 6: DI Fix for View Constructors (D-16/D-17)

**Current antipattern** (in all 4 tab Views):
```csharp
// WRONG — couples View to static service locator; untestable
public SettingsPage()
{
    InitializeComponent();
    DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
}
```

**Correct pattern** — inject ViewModel via constructor; `ViewLocator` builds the View and the ViewModel is set by the DataContext binding chain:

The Avalonia `ViewLocator` (`ViewLocator.cs`) already maps `SettingsViewModel → SettingsPage`. The ViewLocator calls `new SettingsPage()`. Since the `TabbedPage` children are declared directly in XAML (not via ViewLocator), the Views are instantiated by the XAML parser. The correct fix is to pass the ViewModel from the XAML parent data context or use `DataTemplates`.

**Practical fix for tab pages**: Remove `Ioc.Default` from the View code-behind. Instead, bind `DataContext` in XAML from the parent (AppShellViewModel holds references to tab ViewModels), or register Views in DI and resolve them with their ViewModel already set.

The simplest testable fix:
1. `AppShellViewModel` holds properties for each tab VM (`SettingsViewModel SettingsVm { get; }`) — injected via constructor
2. `AppShellView.axaml` passes `DataContext="{Binding SettingsVm}"` to `<views:SettingsPage>`
3. View code-behind becomes simply `InitializeComponent()` — no `Ioc.Default` call

```csharp
// Clean View code-behind after DI fix
public partial class SettingsPage : ContentPage
{
    public SettingsPage() => InitializeComponent();
}
```

```xml
<!-- AppShellView.axaml — pass ViewModel from parent -->
<views:SettingsPage DataContext="{Binding SettingsVm}" .../>
```

```csharp
// AppShellViewModel — receives tab VMs via DI
public partial class AppShellViewModel : ViewModelBase
{
    public SettingsViewModel SettingsVm { get; }
    public DashboardViewModel DashboardVm { get; }
    // ...
    public AppShellViewModel(IDeviceService ds, INavigationService ns,
                             SettingsViewModel settingsVm, DashboardViewModel dashboardVm, ...)
    { SettingsVm = settingsVm; DashboardVm = dashboardVm; /* ... */ }
}
```

### Pattern 7: Settings Loading on Page Entry

```csharp
[ObservableProperty] private bool _isLoading;
[ObservableProperty] private string? _loadError;

// Called when page becomes active — wire to ContentPage.Loaded event or call from ctor
[RelayCommand]
public async Task LoadSettingsAsync()
{
    IsLoading = true;
    LoadError = null;
    try
    {
        var settings = await _device.GetSettingsAsync();
        // Map typed model fields to ViewModel properties
        WifiBand = settings.Wifi.Band;
        // ...
        _loadedWifi = settings.Wifi;
        _isDirty = false;
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }
    catch (Exception ex)
    {
        LoadError = $"Could not load settings: {ex.Message}";
    }
    finally { IsLoading = false; }
}
```

### Anti-Patterns to Avoid

- **Dirty state via per-property `bool` flags:** "IsDandBandDirty", "IsSensorsDirty" etc. — combinatorial explosion. Use `OnPropertyChanged` override or record equality instead.
- **`Ioc.Default.GetRequiredService` in View constructors:** Makes Views un-unit-testable and creates a hidden dependency on DI container state. Fix mandated by D-16/D-17.
- **`Dictionary<string, object>` for settings:** Loses type safety and makes all callers guess at keys. Replace with typed records per D-01/D-02.
- **Embedding `ConfirmDialog` XAML inside `SettingsPage`:** Use a separate `Window` subclass shown via `ShowDialog<bool>`. Embedding adds layout complexity and breaks backdrop dimming.
- **Putting sticky Save button inside ScrollViewer:** `ScrollViewer` clips and scrolls its children. A sticky element must live in the parent `Grid`, outside the `ScrollViewer`.
- **Loading settings in `SettingsPage()` constructor:** Constructor runs before UI renders. Use `ContentPage.Loaded` event or a command triggered from the page's `OnNavigatedTo` override if Avalonia 12 RC1 exposes one; otherwise trigger `LoadSettingsCommand` from `AppShellViewModel` when the Settings tab is selected.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Structural equality for dirty state | Custom equality comparer per settings category | C# `record` value equality | Records compare all fields by value for free |
| Observable collection of pending changes | Custom change tracking list | `[ObservableProperty]` + `OnPropertyChanged` guard | CommunityToolkit handles property change notification |
| Dialog from ViewModel | Direct `Window` reference in VM | `IDialogService` abstraction | Keeps ViewModel testable without UI thread |
| DI container in Views | `Ioc.Default.GetRequiredService` | Constructor DI via parent XAML binding | Decouples Views, enables headless testing |
| Boolean flags per setting for dirty check | 20+ `bool` dirty flags | `OnPropertyChanged` override + single `_isDirty` | O(1) vs O(N) property count scaling |

**Key insight:** The biggest implementation trap in settings pages is state synchronization between "what the device has" and "what the user has typed." Using immutable records for loaded snapshots and a single dirty flag in `OnPropertyChanged` is far more maintainable than tracking changes per property.

---

## Common Pitfalls

### Pitfall 1: `NotifyPropertyChangedFor` on HasUnsavedChanges creates infinite loop

**What goes wrong:** Marking every property with `[NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]` and then `HasUnsavedChanges` firing `SaveCommand.NotifyCanExecuteChanged()` can re-trigger property change events if not guarded.

**Why it happens:** `OnPropertyChanged` is called recursively if `HasUnsavedChanges` itself changes a property.

**How to avoid:** Use the `OnPropertyChanged` override approach with a guard: check `e.PropertyName != nameof(HasUnsavedChanges)` and `e.PropertyName != nameof(IsLoading)` before setting `_isDirty = true`.

**Warning signs:** UI freezes or stack overflow in debug during property binding.

### Pitfall 2: ToggleSwitch binding fires property change during LoadSettings

**What goes wrong:** When `LoadSettingsAsync` sets ViewModel properties to populate the form, `_isDirty` gets set to `true` immediately, making the Save button appear before the user touches anything.

**Why it happens:** `[ObservableProperty]` setters fire `OnPropertyChanged` even when set programmatically.

**How to avoid:** Use a loading guard: `if (_isLoading) return;` at the top of the dirty-check logic in `OnPropertyChanged` override. Set `_isLoading = true` before loading, `false` after.

**Warning signs:** Save button visible immediately when navigating to Settings tab.

### Pitfall 3: `IDeviceCommands.GetSettingsAsync` returns a composite object but VM loads all at once

**What goes wrong:** With D-07 (load all settings at once), `GetSettingsAsync` must return all 7 categories in one call. If the interface is refactored to return separate methods per category, wave plans break.

**How to avoid:** Define `DeviceSettings` as a composite record containing all 7 category records. `GetSettingsAsync` returns one `DeviceSettings`. `ApplySettingsAsync` accepts one `DeviceSettings`.

```csharp
public record DeviceSettings(
    WifiSettings Wifi,
    RecordingSettings Recording,
    ChannelSettings Channels,
    CameraSettings Camera,
    SensorSettings Sensors,
    SystemSettings System,
    OverlaySettings Overlays);
```

### Pitfall 4: DI Fix for Tab Pages breaks AppShellView XAML

**What goes wrong:** Changing `AppShellViewModel` constructor signature (adding 4 tab VM parameters) causes the existing DI registration in `AppServices.cs` to break unless all new parameters are also registered.

**Why it happens:** `AddTransient<AppShellViewModel>()` means DI must resolve all its constructor parameters.

**How to avoid:** After adding tab VM parameters to `AppShellViewModel`, verify all tab VMs are `AddTransient` in `AppServices`. Also update `AppShellView.axaml` to pass `DataContext` to each tab page before removing `Ioc.Default` from their code-behinds.

**Warning signs:** `InvalidOperationException: Unable to resolve service for type '...'` at startup.

### Pitfall 5: Avalonia's Slider default tick frequency may not match 1-5 integer semantics

**What goes wrong:** Avalonia `Slider` with `Minimum=1 Maximum=5` defaults to continuous double values. Setting a sensitivity to 2.7 is meaningless.

**How to avoid:** Set `IsSnapToTickEnabled="True"` and `TickFrequency="1"` on all sensitivity and volume sliders. Bind to `int` properties (CommunityToolkit handles the double-to-int coercion via the generated setter).

### Pitfall 6: Unsaved-changes guard requires navigation interception

**What goes wrong:** D-13 requires intercepting tab navigation away from Settings when dirty. Avalonia 12 RC1 `TabbedPage` does not natively support navigation guards (OnNavigatingFrom cancellation).

**How to avoid:** Implement the guard in `AppShellViewModel`: monitor the `TabbedPage.SelectedIndex` changing while `SettingsVm.HasUnsavedChanges` is true. Show the dialog from `AppShellViewModel` (which has `IDialogService`) and revert `SelectedIndex` if the user chooses Discard without saving. This means `AppShellViewModel` must be aware of the Settings VM's dirty state.

**Warning signs:** Users navigate away and lose changes silently.

---

## Code Examples

Verified patterns from codebase and official sources:

### IDeviceCommands Refactored Interface

```csharp
// Replaces Dictionary<string, object> approach (D-02)
public interface IDeviceCommands
{
    Task<DeviceSettings> GetSettingsAsync(CancellationToken ct = default);
    Task<bool> ApplySettingsAsync(DeviceSettings settings, CancellationToken ct = default);
    Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default);
    Task<bool> FactoryResetAsync(CancellationToken ct = default);
    Task<bool> WipeSdCardAsync(CancellationToken ct = default);  // new — D-15
}
```

### MockDashcamDevice.GetSettingsAsync (typed)

```csharp
// Replaces the sparse Dictionary stub (D-03)
public Task<DeviceSettings> GetSettingsAsync(CancellationToken ct = default)
    => Task.FromResult(new DeviceSettings(
        Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "DashcamAP", ""),
        Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
        Channels: new ChannelSettings(RecordingChannels.FrontAndRear),
        Camera: new CameraSettings(RearOrientation.Normal),
        Sensors: new SensorSettings(DrivingShockSensitivity: 3, ParkingShockSensitivity: 3, RadarSensitivity: 3),
        System: new SystemSettings(GpsEnabled: true, MicrophoneEnabled: true, SpeakerVolume: 3),
        Overlays: new OverlaySettings(DateEnabled: true, TimeEnabled: true,
                                      GpsPositionEnabled: false, SpeedEnabled: false,
                                      SpeedUnit: SpeedUnit.KilometersPerHour)));
```

### Danger Zone XAML Section

```xml
<!-- Visually separated at bottom (D-06) -->
<Border Margin="0,24,0,0"
        Background="#33FF4444"
        BorderBrush="#88FF4444"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16">
  <StackPanel Spacing="8">
    <TextBlock Text="Danger Zone"
               FontSize="16"
               FontWeight="Bold"
               Foreground="#FFDD2222"/>
    <TextBlock Text="These actions are permanent and cannot be undone."
               Opacity="0.7"
               FontSize="13"/>
    <Button Content="Factory Reset..."
            Command="{Binding FactoryResetCommand}"
            Classes="danger"
            HorizontalAlignment="Left"/>
    <Button Content="Wipe SD Card..."
            Command="{Binding WipeSdCardCommand}"
            Classes="danger"
            HorizontalAlignment="Left"/>
  </StackPanel>
</Border>
```

### NSubstitute Test for Save Flow

```csharp
// Source: NSubstitute 5.3.0 / FluentAssertions 8.9.0 — established project patterns
[Fact]
public async Task SaveCommand_WhenDirty_CallsApplySettingsAsync()
{
    var device = Substitute.For<IDashcamDevice>();
    var dialogService = Substitute.For<IDialogService>();
    var loaded = new DeviceSettings(/* defaults */);
    device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(loaded);
    device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>())
          .Returns(true);

    var vm = new SettingsViewModel(device, dialogService);
    await vm.LoadSettingsCommand.ExecuteAsync(null);

    vm.WifiBand = WifiBand.TwoPointFourGHz; // make dirty
    await vm.SaveCommand.ExecuteAsync(null);

    await device.Received(1).ApplySettingsAsync(
        Arg.Is<DeviceSettings>(s => s.Wifi.Band == WifiBand.TwoPointFourGHz),
        Arg.Any<CancellationToken>());
}
```

### Slider Integer Binding (XAML)

```xml
<!-- IsSnapToTickEnabled ensures only integer values are selected (Pitfall 5) -->
<Slider Minimum="1" Maximum="5"
        IsSnapToTickEnabled="True"
        TickFrequency="1"
        Value="{Binding DrivingShockSensitivity, Mode=TwoWay}"/>
```

### ConnectionStateToBrushConverter Pattern (reference for new converters)

```csharp
// Source: existing src/BlackBoxBuddy/Converters/ConnectionStateToBrushConverter.cs
// Follow same pattern for any new converters in this phase:
// - static readonly Instance property
// - implement IValueConverter
// - ConvertBack throws NotSupportedException
public class BoolToVisibilityConverter : IValueConverter { /* ... */ }
```

---

## Existing Code: What Must Change

| File | Current State | Required Change |
|------|---------------|-----------------|
| `Device/IDeviceCommands.cs` | `Dictionary<string, object>` API | Replace with typed `DeviceSettings` composite record; add `WipeSdCardAsync` |
| `Device/Mock/MockDashcamDevice.cs` | Sparse 3-key dict, no typed models | Implement typed `GetSettingsAsync`/`ApplySettingsAsync`/`WipeSdCardAsync` |
| `ViewModels/SettingsViewModel.cs` | Placeholder (Title property only) | Full implementation: all 20+ settings properties, dirty tracking, load/save/reset/wipe commands |
| `Views/SettingsPage.axaml` | Single TextBlock | Full scrollable sections + sticky Save bar |
| `Views/SettingsPage.axaml.cs` | `Ioc.Default.GetRequiredService<SettingsViewModel>()` | Remove IoC call; receive DataContext from parent |
| `Views/DashboardPage.axaml.cs` | `Ioc.Default.GetRequiredService<DashboardViewModel>()` | Remove IoC call; receive DataContext from parent (D-17) |
| `Views/RecordingsPage.axaml.cs` | `Ioc.Default.GetRequiredService<RecordingsViewModel>()` | Remove IoC call (D-17) |
| `Views/LiveFeedPage.axaml.cs` | `Ioc.Default.GetRequiredService<LiveFeedViewModel>()` | Remove IoC call (D-17) |
| `ViewModels/Shell/AppShellViewModel.cs` | Receives `IDeviceService` + `INavigationService` | Add tab ViewModel properties; inject tab VMs via constructor |
| `Views/Shell/AppShellView.axaml` | TabbedPage children have no explicit DataContext | Pass `DataContext="{Binding XxxVm}"` to each tab page |
| `AppServices.cs` | Registers ViewModels as transient | Register `IDialogService`; verify all AppShellViewModel constructor params registered |
| `App.axaml.cs` | Uses `Ioc.Default` for `AppShellViewModel` only | `Ioc.Default` is still acceptable in `App.axaml.cs` (bootstrapper, not testable) — no change needed |

---

## New Files Required

| File | Purpose |
|------|---------|
| `src/BlackBoxBuddy/Models/Settings/WifiSettings.cs` | `WifiBand` enum, `WifiMode` enum, `WifiSettings` record |
| `src/BlackBoxBuddy/Models/Settings/RecordingSettings.cs` | `DrivingMode` enum, `ParkingMode` enum, `RecordingSettings` record |
| `src/BlackBoxBuddy/Models/Settings/ChannelSettings.cs` | `RecordingChannels` enum, `ChannelSettings` record |
| `src/BlackBoxBuddy/Models/Settings/CameraSettings.cs` | `RearOrientation` enum, `CameraSettings` record |
| `src/BlackBoxBuddy/Models/Settings/SensorSettings.cs` | `SensorSettings` record (3 int properties) |
| `src/BlackBoxBuddy/Models/Settings/SystemSettings.cs` | `SystemSettings` record |
| `src/BlackBoxBuddy/Models/Settings/OverlaySettings.cs` | `SpeedUnit` enum, `OverlaySettings` record |
| `src/BlackBoxBuddy/Models/Settings/DeviceSettings.cs` | Composite record of all 7 category records |
| `src/BlackBoxBuddy/Services/IDialogService.cs` | `ShowConfirmAsync` interface |
| `src/BlackBoxBuddy/Services/DialogService.cs` | Production implementation using `Window.ShowDialog<bool>` |
| `src/BlackBoxBuddy/Views/ConfirmDialog.axaml` | Reusable confirm dialog Window |
| `src/BlackBoxBuddy/Views/ConfirmDialog.axaml.cs` | Code-behind; exposes Title/Message/ConfirmText |
| `tests/.../ViewModels/SettingsViewModelTests.cs` | Load/save/dirty/danger-zone coverage |

---

## State of the Art

| Old Approach | Current Approach | Impact |
|--------------|------------------|--------|
| `Dictionary<string, object>` settings | Strongly-typed records with enums | Compile-time safety; IDE autocomplete; no magic strings |
| `Ioc.Default` in View code-behind | ViewModel passed via DataContext from parent | Enables headless testing of Views; follows MVVM properly |
| No unsaved-changes guard | Dirty-state tracking + nav interception | Prevents accidental data loss |

---

## Open Questions

1. **ContentPage `OnNavigatedTo` / `OnNavigatingFrom` lifecycle in Avalonia 12 RC1**
   - What we know: `ContentPage` is a new control in Avalonia 12 RC1. Phase 1 decisions document that it uses `Header` (not `Title`) for tab label.
   - What's unclear: Whether `ContentPage` fires navigation lifecycle events (like `OnNavigatedTo`) that `SettingsViewModel` can hook to trigger `LoadSettingsAsync` automatically.
   - Recommendation: Trigger `LoadSettingsCommand` from the `SettingsPage.Loaded` event in code-behind as a fallback. If Avalonia 12 RC1 adds lifecycle events, prefer those. Investigate during Wave 0 of implementation.

2. **TabbedPage `SelectionChanged` event for unsaved-changes guard**
   - What we know: Avalonia 12 RC1's `TabbedPage` exists and is used in Phase 1.
   - What's unclear: Whether `TabbedPage` exposes a `SelectionChanging` (cancellable) vs `SelectionChanged` (post-fact) event.
   - Recommendation: If only `SelectionChanged` is available, intercept in `AppShellViewModel` and programmatically revert `SelectedIndex` when the user chooses "Discard." This requires `AppShellViewModel` to have a `SelectedTabIndex` two-way bound property.

3. **`IDialogService` registration and `Window` owner resolution**
   - What we know: `DialogService` requires a `Func<Window?>` owner provider.
   - What's unclear: Best way to resolve the current top-level `Window` in a DI-registered singleton service on both Desktop and Android (where there's no `Window`).
   - Recommendation: On Android, the confirm dialogs can fall back to a simple overlay `Panel` approach (similar to `ManualConnectionDialog` in Phase 1) rather than a separate `Window`. Abstract this behind `IDialogService` so SettingsViewModel is unaware of the difference.

---

## Environment Availability

Step 2.6: SKIPPED — this phase is purely code/model changes with no new external dependencies. All required tooling (dotnet SDK, Avalonia, test runner) was verified operational during Phase 1.

---

## Sources

### Primary (HIGH confidence)
- Codebase inspection — `IDeviceCommands.cs`, `MockDashcamDevice.cs`, `SettingsPage.axaml.cs`, `AppShellViewModel.cs`, `ViewLocator.cs`, `AppServices.cs`, `App.axaml.cs` — read directly from repo 2026-03-24
- `CLAUDE.md` — locked technology stack, patterns, and constraints
- `02-CONTEXT.md` — locked phase decisions D-01 through D-20
- C# 9+ `record` value equality — language specification feature; well-established
- CommunityToolkit.Mvvm `[ObservableProperty]`, `[RelayCommand]`, `[NotifyPropertyChangedFor]` — established project patterns from Phase 1 code

### Secondary (MEDIUM confidence)
- [Avalonia Docs: How to Work with Dialogs](https://docs.avaloniaui.net/docs/how-to/dialogs-how-to) — confirmed service-based dialog pattern; `ShowDialog<T>()` API; no built-in MessageBox
- [Avalonia Docs: ScrollViewer](https://docs.avaloniaui.net/docs/reference/controls/scrollviewer) — confirmed sticky elements must live outside ScrollViewer in parent Grid

### Tertiary (LOW confidence)
- WebSearch results on Avalonia 12 RC1 dialog patterns — limited 12 RC1-specific content found; patterns consistent with Avalonia 11 best practices which remain applicable

---

## Metadata

**Confidence breakdown:**
- Settings models (typed records): HIGH — C# language feature, no library dependency
- ViewModel dirty state pattern: HIGH — CommunityToolkit.Mvvm in production use in project
- Sticky Save bar layout: HIGH — confirmed by Avalonia ScrollViewer docs (must be outside)
- IDialogService pattern: HIGH — confirmed by official Avalonia dialog docs
- DI fix approach (AppShellViewModel holds tab VMs): HIGH — verified against actual codebase structure
- Unsaved-changes guard via TabbedPage: MEDIUM — depends on TabbedPage SelectionChanging availability in 12 RC1 (open question)
- ContentPage lifecycle events: MEDIUM — not verified; workaround documented

**Research date:** 2026-03-24
**Valid until:** 2026-06-01 (stable stack; Avalonia 12 RC1 pinned)
