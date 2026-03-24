# Phase 1: Foundation - Research

**Researched:** 2026-03-24
**Domain:** Avalonia 12 RC1 — navigation shell, DI wiring, device abstraction layer, adaptive layout
**Confidence:** HIGH — all core findings verified against official Avalonia 12 RC1 docs and NuGet

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Navigation Shell**
- D-01: Use Avalonia 12's TabbedPage with TabPlacement="Auto" (bottom on mobile, left/top on desktop) for the main navigation structure
- D-02: Icon-only nav bar with centered icons — no text labels. Pages: Dashboard, Recordings, Live Feed, Settings
- D-03: Connection indicator is a persistent element outside the tab content area (header/footer), always visible regardless of active page
- D-04: Breakpoint at 600px width determines portrait vs landscape layout adaptation
- D-05: Dark mode enforced — set RequestedThemeVariant="Dark" on Application

**Device Abstraction**
- D-06: Interface segregation pattern — 5 narrow interfaces: IDeviceDiscovery, IDeviceConnection, IDeviceCommands, IDeviceFileSystem, IDeviceLiveStream
- D-07: IDashcamDevice composes all 5 interfaces as a unified device handle
- D-08: MockDashcamDevice implements all interfaces with configurable delay and failure simulation for testing
- D-09: Mock device generates realistic sample data: recordings with metadata, device info, connection states
- D-10: Device abstraction lives in a Device/ folder in the shared project with interfaces and mock implementation

**Connection Flow**
- D-11: Three connection indicator states: Searching (animated), Connected (shows device name), Disconnected (tap to retry/configure)
- D-12: Auto-discovery runs for 5 seconds on startup before falling back to manual
- D-13: After timeout, indicator shows "No device found" with tap action to open manual connection
- D-14: Manual connection allows IP/hostname entry for direct device connection
- D-15: Connection indicator tap opens manual connection setup (CONN-03) and SD Card mode entry (CONN-04)

**Provisioning Wizard**
- D-16: Linear 3-step wizard: (1) Welcome + device info display, (2) WiFi setup (AP mode or join network), (3) Confirmation/done
- D-17: "Unconfigured" defined as: mock device reports isProvisioned=false
- D-18: Wizard uses NavigationPage with forward/back navigation gestures
- D-19: After provisioning completes, navigate to Dashboard

**DI and App Bootstrap**
- D-20: Use Microsoft.Extensions.DependencyInjection with CommunityToolkit.Mvvm's Ioc.Default
- D-21: Replace reflection-based ViewLocator with pattern-matching switch expression (AOT-safe)
- D-22: Shared ConfigureServices method called from both Desktop Program.cs and Android entry point
- D-23: All services registered as singletons (device service, discovery service); ViewModels as transient
- D-24: INavigationService abstraction wrapping Avalonia 12 page navigation to isolate from RC API changes

### Claude's Discretion
- Exact icon choices for nav bar (Material Icons, Fluent, or custom)
- Loading animation style for Searching state
- Exact responsive breakpoint thresholds beyond the 600px primary
- Internal error handling patterns and logging approach
- Test structure and naming conventions

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| FOUND-01 | App shell uses adaptive layout — vertical nav in portrait, horizontal nav in landscape | TabbedPage with TabPlacement="Auto" handles bottom-on-mobile / top-on-desktop automatically; D-01 |
| FOUND-02 | Navigation bar uses icon-only centered icons | TabbedPage child ContentPage Icon property; icon-only achieved by omitting Header text on tab items |
| FOUND-03 | Device connection indicator visible in nav bar at all times (Searching / Connected / Disconnected) | Persistent control outside TabbedPage content area; implemented as AppShellViewModel observable state |
| FOUND-04 | App uses dark mode exclusively | RequestedThemeVariant="Dark" on Application element in App.axaml |
| FOUND-05 | All content visible regardless of screen size or aspect ratio (responsive) | AdaptiveBehavior from Xaml.Behaviors.Avalonia; Window.Bounds subscription; 600px breakpoint |
| FOUND-06 | Extended and compact views adapt to available screen space | AdaptiveClassSetter toggles layout style classes; views define styles for each class |
| CONN-01 | Dashcam is automatically discovered on app startup | DeviceService.StartDiscoveryAsync() called from AppShellViewModel.OnNavigatedTo / app init; 5s timeout (D-12) |
| CONN-02 | If auto-discovery fails, user can manually configure the connection | ManualConnectionDialog opened when indicator tapped after timeout (D-13/D-14) |
| CONN-03 | Connection indicator serves as entry point to manual connection setup | Tap handler on indicator control calls INavigationService to open manual connection UI (D-15) |
| CONN-04 | Connection indicator provides entry point to SD Card mode | Same tap handler shows SD Card mode option alongside manual connection (D-15) |
| CONN-05 | Mock/demo device available for development and testing | MockDashcamDevice implements IDashcamDevice with configurable behavior (D-08/D-09) |
| PROV-01 | If discovered device is unconfigured, user is guided through provisioning wizard | DeviceService checks isProvisioned flag on DeviceInfo; navigates to ProvisioningPage if false (D-17) |
| PROV-02 | Provisioning wizard walks user through essential device setup steps | Linear 3-step NavigationPage wizard: Welcome, WiFi setup, Confirmation (D-16) |
</phase_requirements>

---

## Summary

Phase 1 establishes the structural foundation everything else builds on: a pinned and DI-wired Avalonia 12 RC1 app shell, an AOT-safe ViewLocator, the full device abstraction layer with mock device, auto-discovery with manual fallback, and the provisioning wizard. All decisions are locked; no alternatives need to be explored.

The existing project skeleton is clean and directly extends to the target architecture. The shared `BlackBoxBuddy` project already has `ViewModelBase : ObservableObject`, a reflection-based ViewLocator (needs replacement), `App.axaml.cs` handling both desktop and Android lifetimes (the DI hook point), and a passing test suite using `Avalonia.Headless.XUnit` + `xunit.v3.mtp-v2`. Avalonia 12 RC1 NuGet versions are already centrally managed and pinned in `Directory.Packages.props`.

The one structural gap is that `Directory.Packages.props` does not yet include `Xaml.Behaviors.Avalonia` (for responsive layout) or `NSubstitute`/`FluentAssertions`/`Bogus` (for the test project). These must be added before implementing responsive layout and before writing meaningful tests. A `IMediaPlayerService` interface stub must also land in this phase (it is a service contract consumers will depend on) — even though no implementation is needed until Phase 3.

**Primary recommendation:** Wire DI and replace ViewLocator first (those two changes unlock all other work), then build device interfaces and mock device, then build the shell view with connection indicator, then the provisioning wizard last.

---

## Standard Stack

### Core (already in Directory.Packages.props — do not change versions)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Avalonia | 12.0.0-rc1 | Cross-platform UI framework | Pinned; RC1 released 2026-03-19 |
| Avalonia.Themes.Fluent | 12.0.0-rc1 | FluentTheme (dark mode) | Active theme in App.axaml |
| Avalonia.Fonts.Inter | 12.0.0-rc1 | Font bundling | Already in project |
| Avalonia.Android | 12.0.0-rc1 | Android head project support | Already in Android project |
| Avalonia.Desktop | 12.0.0-rc1 | Desktop head project support | Already in Desktop project |
| Avalonia.Headless.XUnit | 12.0.0-rc1 | UI testing without display | Already in Tests project |
| CommunityToolkit.Mvvm | 8.4.1 | MVVM scaffolding + source generators | Already in shared project |
| xunit.v3.mtp-v2 | 3.2.2 | Test runner (Microsoft.Testing.Platform v2) | Already pinned; tests passing |
| Microsoft.Extensions.DependencyInjection | (bundled with .NET 10) | DI container | Part of SDK; no NuGet needed |

### Supporting (must be added to Directory.Packages.props)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Xaml.Behaviors.Avalonia | 12.0.0-rc1 | AdaptiveBehavior + AdaptiveClassSetter for responsive layout | Shared project — responsive shell |
| NSubstitute | 5.3.0 | Mocking interfaces in unit tests | Tests project |
| FluentAssertions | 8.9.0 | BDD-style assertion library | Tests project |
| Bogus | 35.6.5 | Realistic fake data for test fixtures (recordings, device info) | Tests project |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Xaml.Behaviors.Avalonia | Avalonia.Xaml.Behaviors | Deprecated — NuGet page for old package redirects to Xaml.Behaviors.Avalonia |
| NSubstitute | Moq | Moq added SponsorLink in v4.20; NSubstitute syntax cleaner for interface-heavy code |
| Pattern-matching ViewLocator | Reflection-based ViewLocator | Reflection version is AOT-incompatible and has [RequiresUnreferencedCode] attribute already |

**Adding to Directory.Packages.props:**
```xml
<!-- Responsive layout -->
<PackageVersion Include="Xaml.Behaviors.Avalonia" Version="12.0.0-rc1"/>
<!-- Testing -->
<PackageVersion Include="NSubstitute" Version="5.3.0"/>
<PackageVersion Include="FluentAssertions" Version="8.9.0"/>
<PackageVersion Include="Bogus" Version="35.6.5"/>
```

**Adding package references to csproj files:**
```xml
<!-- BlackBoxBuddy.csproj (shared project) -->
<PackageReference Include="Xaml.Behaviors.Avalonia"/>

<!-- BlackBoxBuddy.Tests.csproj -->
<PackageReference Include="NSubstitute"/>
<PackageReference Include="FluentAssertions"/>
<PackageReference Include="Bogus"/>
```

---

## Architecture Patterns

### Recommended Project Structure

```
src/
├── BlackBoxBuddy/                  # Shared UI + core (target: net10.0)
│   ├── App.axaml(.cs)              # Bootstrap — DI init goes in OnFrameworkInitializationCompleted
│   ├── ViewLocator.cs              # REPLACE with pattern-matching switch expression
│   ├── Assets/                     # Icons, images
│   ├── Device/                     # Vendor abstraction layer (NEW this phase)
│   │   ├── IDashcamDevice.cs       # Composes all 5 narrow interfaces
│   │   ├── IDeviceDiscovery.cs
│   │   ├── IDeviceConnection.cs
│   │   ├── IDeviceCommands.cs
│   │   ├── IDeviceFileSystem.cs
│   │   ├── IDeviceLiveStream.cs
│   │   └── Mock/
│   │       └── MockDashcamDevice.cs
│   ├── Models/                     # Domain models (NEW this phase)
│   │   ├── DeviceInfo.cs           # IsProvisioned, DeviceName, FirmwareVersion
│   │   └── ConnectionState.cs      # Enum: Searching, Connected, Disconnected
│   ├── Services/                   # Platform-agnostic services (NEW this phase)
│   │   ├── IDeviceService.cs
│   │   ├── DeviceService.cs
│   │   └── IMediaPlayerService.cs  # Stub interface only — implementations in Phase 3
│   ├── Navigation/                 # Navigation abstraction (NEW this phase)
│   │   └── INavigationService.cs
│   ├── Views/
│   │   ├── Shell/
│   │   │   └── AppShellView.axaml  # TabbedPage root + connection indicator overlay
│   │   └── Provisioning/
│   │       └── ProvisioningPage.axaml  # 3-step wizard NavigationPage
│   └── ViewModels/
│       ├── ViewModelBase.cs        # Already exists — keep as-is
│       ├── Shell/
│       │   └── AppShellViewModel.cs  # Connection state, layout class, tab selection
│       └── Provisioning/
│           └── ProvisioningViewModel.cs
│
├── BlackBoxBuddy.Desktop/          # Desktop entry (target: net10.0, OutputType: WinExe)
│   └── Program.cs                  # Add DI registration; call shared ConfigureServices
│
└── BlackBoxBuddy.Android/          # Android entry (target: net10.0-android)
    └── MainActivity.cs             # Add DI registration; call shared ConfigureServices

tests/
└── BlackBoxBuddy.Tests/            # target: net10.0
    ├── Device/
    │   └── MockDashcamDeviceTests.cs  # Verify mock behavior (delay, failure simulation)
    └── ViewModels/
        └── AppShellViewModelTests.cs  # Connection state transitions
```

### Pattern 1: AOT-Safe ViewLocator (MUST replace existing)

**What:** Replace the reflection-based `ViewLocator.Build()` with an explicit pattern-matching switch expression. Each ViewModel type maps to a concrete View type at compile time.

**When to use:** First task in this phase — blocks all ViewModel/View wiring that follows.

**Example:**
```csharp
// Source: https://docs.avaloniaui.net/docs/data-templates/view-locator
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        AppShellViewModel => new AppShellView(),
        ProvisioningViewModel => new ProvisioningPage(),
        DashboardViewModel => new DashboardPage(),
        RecordingsViewModel => new RecordingsPage(),
        LiveFeedViewModel => new LiveFeedPage(),
        SettingsViewModel => new SettingsPage(),
        _ => new TextBlock { Text = $"No view for {param?.GetType().Name}" }
    };

    public bool Match(object? data) => data is ViewModelBase;
}
```

### Pattern 2: DI Bootstrap — Shared ConfigureServices

**What:** Single `ConfigureServices` static method in the shared project called from both Desktop `Program.cs` and Android `MainActivity.cs`. Returns a built `IServiceProvider`. Platform-specific services are registered by each caller before passing to the shared method.

**When to use:** In `App.axaml.cs:OnFrameworkInitializationCompleted` — DI container must be ready before any ViewModel is instantiated.

**Example:**
```csharp
// BlackBoxBuddy/AppServices.cs (shared static helper)
public static class AppServices
{
    public static IServiceProvider ConfigureServices(
        Action<IServiceCollection>? platformServices = null)
    {
        var services = new ServiceCollection();

        // Platform-specific registrations first
        platformServices?.Invoke(services);

        // Shared singletons
        services.AddSingleton<IDashcamDevice, MockDashcamDevice>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // Transient ViewModels
        services.AddTransient<AppShellViewModel>();
        services.AddTransient<ProvisioningViewModel>();

        return services.BuildServiceProvider();
    }
}

// App.axaml.cs — OnFrameworkInitializationCompleted
var provider = AppServices.ConfigureServices();
Ioc.Default.ConfigureServices(provider);

// Desktop Program.cs — before BuildAvaloniaApp
var provider = AppServices.ConfigureServices(services =>
{
    // desktop-specific registrations (IMediaPlayerService -> VlcMediaPlayerService in Phase 3)
});
```

### Pattern 3: TabbedPage Shell with Connection Indicator

**What:** Root shell is a `TabbedPage` with `TabPlacement="Auto"`. Connection indicator is layered outside the tab content via a `Panel` or `Grid` overlay — not inside any ContentPage. The indicator is permanently visible regardless of which tab is selected.

**When to use:** AppShellView is the root view displayed by App.axaml.cs.

**Verified API:**
- `TabbedPage` in namespace `Avalonia.Controls`
- `TabPlacement` enum: `Auto` | `Top` | `Bottom` | `Left` | `Right`
- `TabPlacement="Auto"` = bottom on mobile, top on desktop (confirmed in official Avalonia 12 docs)
- Child `ContentPage` sets `Icon` property for the tab icon and `Header` for the tab label
- To hide the text label (D-02), either leave `Header` unset or style the tab item template to show only the icon

**Example:**
```xml
<!-- Source: https://docs.avaloniaui.net/controls/navigation/tabbedpage -->
<TabbedPage xmlns="https://github.com/avaloniaui"
            TabPlacement="Auto">
    <ContentPage Icon="{StaticResource DashboardIcon}">
        <!-- Dashboard content -->
    </ContentPage>
    <ContentPage Icon="{StaticResource RecordingsIcon}">
        <!-- Recordings content -->
    </ContentPage>
    <ContentPage Icon="{StaticResource LiveFeedIcon}">
        <!-- Live Feed content -->
    </ContentPage>
    <ContentPage Icon="{StaticResource SettingsIcon}">
        <!-- Settings content -->
    </ContentPage>
</TabbedPage>
```

**For the connection indicator overlay**, wrap the `TabbedPage` in a `Grid` or `Panel` and place the indicator in a separate row/overlay that is always visible.

### Pattern 4: INavigationService Abstraction

**What:** Thin interface wrapping Avalonia 12 `NavigationPage` push/pop methods. Isolates all Avalonia navigation API calls so a breaking change in RC1→stable only requires updating one concrete implementation.

**When to use:** Any ViewModel that needs to navigate between pages calls `INavigationService`, never calls `NavigationPage` directly.

**Verified NavigationPage API (Avalonia 12 RC1):**
```
PushAsync(Page)        — add page to stack
PopAsync()             — remove current page
PopToRootAsync()       — return to root
PopToPageAsync(Page)   — pop until target is top
ReplaceAsync(Page)     — swap current page (no history entry)
PushModalAsync(Page)   — modal overlay
PopModalAsync()        — dismiss modal
```

**Example:**
```csharp
// Shared project
public interface INavigationService
{
    Task PushAsync(ViewModelBase viewModel);
    Task PopAsync();
    Task PopToRootAsync();
}

// Concrete implementation wraps NavigationPage
// — updated only here if Avalonia changes the API between RC1 and stable
```

### Pattern 5: Device Abstraction — Interface Segregation

**What:** `IDashcamDevice` inherits all 5 narrow interfaces. Services capability-check via `device is IDeviceLiveStream liveStream` before calling stream methods. MockDashcamDevice implements all 5 interfaces with configurable latency and failure injection.

**Example:**
```csharp
// Source: .planning/research/ARCHITECTURE.md
public interface IDashcamDevice :
    IDeviceDiscovery,
    IDeviceConnection,
    IDeviceCommands,
    IDeviceFileSystem,
    IDeviceLiveStream { }

public interface IDeviceDiscovery
{
    Task<DeviceInfo?> DiscoverAsync(CancellationToken ct);
}

public interface IDeviceConnection
{
    Task<bool> ConnectAsync(string host, CancellationToken ct);
    Task DisconnectAsync(CancellationToken ct);
}
```

### Pattern 6: Auto-Discovery with 5-Second Timeout

**What:** `DeviceService` wraps `IDeviceDiscovery.DiscoverAsync()` with a `CancellationTokenSource` tied to a 5-second timeout. On success, updates `ConnectionState.Connected`. On timeout/null result, sets `ConnectionState.Disconnected` with a "No device found" message.

**Example:**
```csharp
public async Task StartDiscoveryAsync()
{
    ConnectionState = ConnectionState.Searching;
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    try
    {
        var info = await _device.DiscoverAsync(cts.Token);
        if (info is null)
        {
            ConnectionState = ConnectionState.Disconnected;
        }
        else
        {
            ConnectedDevice = info;
            ConnectionState = info.IsProvisioned
                ? ConnectionState.Connected
                : ConnectionState.NeedsProvisioning;
        }
    }
    catch (OperationCanceledException)
    {
        ConnectionState = ConnectionState.Disconnected;
    }
}
```

### Anti-Patterns to Avoid

- **Reflection-based ViewLocator in production:** The existing ViewLocator.cs already carries `[RequiresUnreferencedCode]` — this must be replaced before any other feature work. AOT trimming will silently break it.
- **Platform #if blocks in shared ViewModels:** Use interface + DI. No `#if ANDROID` in shared project.
- **Direct NavigationPage calls in ViewModels:** Always go through `INavigationService`.
- **Synchronous device calls:** All `IDashcamDevice` methods return `Task<T>`; use `AsyncRelayCommand`.
- **Floating Avalonia pre-release version:** Versions are already pinned in `Directory.Packages.props` — do not add version floats for any new package references.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| MVVM property notification | Custom `INotifyPropertyChanged` impl | `[ObservableProperty]` + `ObservableObject` (CommunityToolkit) | Source-generator approach; zero boilerplate; already in project |
| Async command with busy state | Custom ICommand wrapper | `AsyncRelayCommand` (CommunityToolkit) | Handles IsRunning, cancellation, exception propagation |
| Responsive layout breakpoints | Manual Bounds event subscription | `AdaptiveBehavior` + `AdaptiveClassSetter` (Xaml.Behaviors.Avalonia) | Declarative XAML; handles resize events; standard Avalonia 12 pattern |
| Mocking interfaces in tests | Manual stub classes | NSubstitute 5.3.0 | Reduces test boilerplate to zero; handles return values, call verification |
| Fake test data | Hardcoded test fixtures | Bogus 35.6.5 | Generates realistic, varied data; prevents brittle tests tied to specific values |
| DI container | Custom service locator | `Microsoft.Extensions.DependencyInjection` + `Ioc.Default` | Already the standard; bundled with .NET 10; integrates with CommunityToolkit |

**Key insight:** The most tempting hand-roll is the connection indicator state machine. Don't build a custom FSM — an `enum ConnectionState` exposed as `[ObservableProperty]` on `AppShellViewModel` with a `switch` expression in `DeviceService` is all that's needed.

---

## Common Pitfalls

### Pitfall 1: Reflection-Based ViewLocator Blocks AOT

**What goes wrong:** The existing `ViewLocator.cs` uses `Type.GetType(name)` and `Activator.CreateInstance()`. With .NET 10's AOT trimming, these calls cause silent runtime failures when the app is published with trimming enabled. The `[RequiresUnreferencedCode]` attribute on the class is the warning sign.

**Why it happens:** The default Avalonia project template ships a reflection-based ViewLocator. It works in Debug builds but breaks in Release/AOT builds.

**How to avoid:** Replace it with the pattern-matching switch expression as the first task of this phase — before any other View/ViewModel pairs are created.

**Warning signs:** Class carries `[RequiresUnreferencedCode]` attribute. Views registered nowhere except by string name convention.

### Pitfall 2: Avalonia RC1 Navigation API Changes Before Stable

**What goes wrong:** `ContentPage`, `NavigationPage`, `TabbedPage`, `DrawerPage` APIs may change between RC1 and stable release. Scattered direct calls to `NavigationPage.PushAsync()` throughout ViewModels means a broad refactor when stable lands.

**Why it happens:** RC = release candidate, not final. The Avalonia team is still gathering feedback.

**How to avoid:** Pin exact versions (already done in `Directory.Packages.props`). Implement `INavigationService` abstraction so concrete navigation calls are in one place.

**Warning signs:** Any ViewModel file containing `using Avalonia.Controls;` for navigation types.

### Pitfall 3: ObservableCollection Mutations from Background Threads

**What goes wrong:** Device discovery and connection polling happen on background threads. Any `ObservableCollection<T>` mutation from those threads causes intermittent `NullReferenceException` in Avalonia's WeakEvent infrastructure — not an immediate throw.

**Why it happens:** Avalonia does not have WPF's `BindingOperations.EnableCollectionSynchronization`. Failures are race conditions that appear non-deterministically.

**How to avoid:** Wrap all collection mutations in `Dispatcher.UIThread.Post()` or `Dispatcher.UIThread.InvokeAsync()`. Establish this as a project pattern in Phase 1 before any background work is wired.

**Warning signs:** `NullReferenceException` stack traces containing `Avalonia.Utilities.WeakEvent`.

### Pitfall 4: App.axaml.cs DI Initialization Race

**What goes wrong:** If `Ioc.Default.ConfigureServices()` is called after the first ViewModel tries to resolve a service, the container is uninitialized and throws.

**Why it happens:** `OnFrameworkInitializationCompleted` is called after `Initialize()`. If any XAML DataContext is set in `Initialize()` (or in a constructor), it fires before the container is ready.

**How to avoid:** Call `Ioc.Default.ConfigureServices()` as the first statement in `OnFrameworkInitializationCompleted`, before any window/view is created. Never set DataContext in `Initialize()`.

**Warning signs:** `InvalidOperationException: The IoC container has not been configured yet.`

### Pitfall 5: TabbedPage Icon-Only Tab Strip — Missing Header Styling

**What goes wrong:** TabbedPage auto-generates tab items that show both Icon and Header text. Without explicitly styling the tab item template to hide text, tabs show icons with empty or "null" labels beside them.

**Why it happens:** The default TabbedPage tab item template renders both Icon and Header. If Header is unset, it may render as an empty string placeholder rather than nothing.

**How to avoid:** Either set an explicit `ItemTemplate` / `TabItemTemplate` that renders only the `Icon`, or verify that leaving `Header` unset produces clean icon-only tabs (test on both desktop and Android). Check official Avalonia 12 docs or GitHub discussions for the confirmed approach.

**Warning signs:** Visible text labels or blank text areas next to tab icons at runtime.

### Pitfall 6: Android Head Project Missing DI Registration

**What goes wrong:** `MainActivity.cs` currently only extends `AvaloniaMainActivity` with no service registration. If `ConfigureServices` is only called from `Desktop/Program.cs`, the Android build compiles but crashes on first service resolution.

**Why it happens:** The Android entry point is separate from the Desktop entry point. Each must independently call the shared `ConfigureServices`.

**How to avoid:** Update `MainActivity.cs` to call `AppServices.ConfigureServices()` before `BuildAvaloniaApp()`. Add a `Loaded`/`OnCreate` override that wires DI.

**Warning signs:** App works on Desktop, crashes on Android with `InvalidOperationException` from DI.

---

## Code Examples

### App.axaml — Dark Mode + ViewLocator

```xml
<!-- Source: https://docs.avaloniaui.net/docs/concepts/application-lifetimes -->
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="using:BlackBoxBuddy"
             x:Class="BlackBoxBuddy.App"
             RequestedThemeVariant="Dark">
  <Application.DataTemplates>
    <local:ViewLocator />
  </Application.DataTemplates>
  <Application.Styles>
    <FluentTheme />
  </Application.Styles>
</Application>
```

### Responsive Layout with AdaptiveBehavior

```xml
<!-- Source: https://www.nuget.org/packages/Xaml.Behaviors.Avalonia -->
<!-- Breakpoint at 600px: portrait class below, landscape class above -->
<Panel xmlns:i="using:Avalonia.Xaml.Interactions.Core"
       xmlns:ia="using:Avalonia.Xaml.Interactivity">
  <ia:Interaction.Behaviors>
    <i:AdaptiveBehavior>
      <i:AdaptiveClassSetter MaxWidth="599" ClassName="portrait" />
      <i:AdaptiveClassSetter MinWidth="600" ClassName="landscape" />
    </i:AdaptiveBehavior>
  </ia:Interaction.Behaviors>
</Panel>
```

### MockDashcamDevice — Configurable Delay + Failure

```csharp
// Configurable for different test scenarios
public class MockDashcamDevice : IDashcamDevice
{
    private readonly TimeSpan _discoveryDelay;
    private readonly bool _simulateFailure;
    private bool _isProvisioned;

    public MockDashcamDevice(
        TimeSpan? discoveryDelay = null,
        bool simulateFailure = false,
        bool isProvisioned = true)
    {
        _discoveryDelay = discoveryDelay ?? TimeSpan.FromMilliseconds(100);
        _simulateFailure = simulateFailure;
        _isProvisioned = isProvisioned;
    }

    public async Task<DeviceInfo?> DiscoverAsync(CancellationToken ct)
    {
        await Task.Delay(_discoveryDelay, ct);
        if (_simulateFailure) return null;
        return new DeviceInfo
        {
            DeviceName = "Mock Dashcam Pro",
            FirmwareVersion = "1.0.0-mock",
            IsProvisioned = _isProvisioned
        };
    }
    // ... IDeviceConnection, IDeviceCommands, etc.
}
```

### Connection State Enum + Observable

```csharp
public enum ConnectionState
{
    Searching,
    Connected,
    Disconnected,
    NeedsProvisioning
}

// AppShellViewModel.cs
public partial class AppShellViewModel : ViewModelBase
{
    [ObservableProperty]
    private ConnectionState _connectionState = ConnectionState.Searching;

    [ObservableProperty]
    private string _connectedDeviceName = string.Empty;
}
```

### Test Structure — ViewModel Unit Test Pattern

```csharp
// No [AvaloniaFact] needed — ViewModels have no Avalonia dependency
public class AppShellViewModelTests
{
    [Fact]
    public async Task StartsInSearchingState()
    {
        var mockDevice = new MockDashcamDevice(discoveryDelay: TimeSpan.Zero);
        var deviceService = new DeviceService(mockDevice);
        var vm = new AppShellViewModel(deviceService, Substitute.For<INavigationService>());

        Assert.Equal(ConnectionState.Searching, vm.ConnectionState);
    }

    [Fact]
    public async Task TransitionsToConnectedAfterSuccessfulDiscovery()
    {
        var mockDevice = new MockDashcamDevice(isProvisioned: true);
        var deviceService = new DeviceService(mockDevice);
        var vm = new AppShellViewModel(deviceService, Substitute.For<INavigationService>());

        await deviceService.StartDiscoveryAsync();

        Assert.Equal(ConnectionState.Connected, vm.ConnectionState);
    }
}
```

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | All build/run | Yes | 10.0.201 | — |
| dotnet run (test runner) | Tests | Yes | via .NET SDK | — |
| Android SDK | Android head project | No | — | Skip Android build in CI; Desktop + Tests build cleanly |
| Avalonia 12.0.0-rc1 NuGet | All Avalonia packages | Yes (pinned in Directory.Packages.props) | 12.0.0-rc1 | — |
| Xaml.Behaviors.Avalonia | Responsive layout | Not yet in Directory.Packages.props | 12.0.0-rc1 | Must be added |
| NSubstitute, FluentAssertions, Bogus | Test project | Not yet in Directory.Packages.props | See Stack section | Must be added |

**Missing dependencies with no fallback:**
- None — all missing items are NuGet packages that can be added

**Missing dependencies with fallback:**
- Android SDK: Desktop project and Tests project build and run successfully without it. Android project will not build locally; this is expected and acceptable.

**Test run command (current):**
```bash
dotnet run --project tests/BlackBoxBuddy.Tests/BlackBoxBuddy.Tests.csproj
```
Tests currently pass (1 test, 1 succeeded).

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Reflection-based ViewLocator | Pattern-matching switch expression | Avalonia 12 / .NET AOT era | AOT safe; compile-time safe; must be done in this phase |
| Avalonia.Xaml.Behaviors | Xaml.Behaviors.Avalonia | Avalonia 12 migration | Old package deprecated; new package version-aligned with Avalonia 12 |
| ReactiveUI in MVVM | CommunityToolkit.Mvvm | Ongoing | ReactiveUI adds complexity; CommunityToolkit source generators cover all needs |
| Manual INotifyPropertyChanged | [ObservableProperty] source generator | CommunityToolkit.Mvvm 8.x | Zero boilerplate; compile-time safe |

**Deprecated/outdated:**
- `Avalonia.Xaml.Behaviors`: NuGet page redirects to `Xaml.Behaviors.Avalonia` — do not reference old package
- Reflection ViewLocator: current `ViewLocator.cs` has `[RequiresUnreferencedCode]` — signals it must be replaced

---

## Open Questions

1. **TabbedPage icon-only tab strip — exact styling approach**
   - What we know: `ContentPage.Icon` sets the tab icon; `TabPlacement="Auto"` positions it correctly
   - What's unclear: The exact XAML needed to suppress text labels when `Header` is unset — whether an empty Header produces a clean icon-only tab or needs an explicit tab item template override
   - Recommendation: Build a minimal test shell with one tab and inspect at runtime before building the full 4-tab shell. The Avalonia DevTools (already wired via `AttachDeveloperTools()` in App.axaml.cs) can inspect the visual tree.

2. **Connection indicator placement relative to TabbedPage**
   - What we know: Indicator must be outside the tab content area; it must be always visible
   - What's unclear: Whether a `Grid` wrapping `TabbedPage` + overlay row is cleanest, or whether Avalonia 12 `TabbedPage` exposes a header/footer slot for persistent UI
   - Recommendation: Check `TabbedPage` template in Avalonia source; if no header slot exists, use a `DockPanel` or `Grid` with the indicator in a fixed row/panel above or below the `TabbedPage`.

3. **INavigationService concrete implementation against NavigationPage**
   - What we know: `NavigationPage.PushAsync(Page)` is the verified API; provisioning wizard uses this
   - What's unclear: How to get a reference to the active `NavigationPage` from an injected singleton service without tightly coupling to the shell
   - Recommendation: Implement as a singleton that the AppShell registers itself with on creation (`INavigationService.SetNavigationPage(nav)`). This is a common Avalonia pattern for apps without a full navigation framework.

---

## Project Constraints (from CLAUDE.md)

| Directive | Applies To |
|-----------|------------|
| Tech stack: C# 14 / .NET 10, AvaloniaUI 12 RC1, CommunityToolkit.Mvvm, xunit.v3 | All code |
| Architecture: MVVM with DRY, KISS, SOLID, TDD, BDD, DDD | All code |
| Testing: Comprehensive test suite using xunit.v3; mock devices for unit testing | Test files |
| Multi-vendor: Device abstractions must support future vendor additions | Device layer |
| UI toolkit: Must use Avalonia 12's new page-based navigation system | Navigation |
| Design: Impeccable plugin guidelines; Stitch for prototyping; DevTools for verification | UI work |
| GSD Workflow: All file changes via GSD command; no direct repo edits outside workflow | Process |

**Enforcement in this phase:**
- All ViewModels must have zero Avalonia imports (SOLID / testability)
- All device calls must be `async Task<T>` with `CancellationToken` (SOLID)
- `MockDashcamDevice` registers as the `IDashcamDevice` implementation (mock-first, multi-vendor ready)
- DevTools already attached in `App.axaml.cs` under `#if DEBUG` — keep this for UI inspection

---

## Sources

### Primary (HIGH confidence)
- [Avalonia TabbedPage docs](https://docs.avaloniaui.net/controls/navigation/tabbedpage) — TabPlacement="Auto" behavior confirmed
- [Avalonia NavigationPage docs](https://docs.avaloniaui.net/controls/navigation/navigationpage) — PushAsync/PopAsync API confirmed
- [Avalonia ContentPage docs](https://docs.avaloniaui.net/controls/navigation/contentpage) — Icon + Header properties confirmed
- [Avalonia ViewLocator docs](https://docs.avaloniaui.net/docs/data-templates/view-locator) — pattern-matching AOT-safe replacement confirmed
- [CommunityToolkit.Mvvm Ioc docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/ioc) — Ioc.Default + IServiceCollection pattern confirmed
- `Directory.Packages.props` in repo — Avalonia 12.0.0-rc1 versions verified as pinned
- `App.axaml.cs` in repo — existing lifecycle patterns and DI hook point verified
- `ViewLocator.cs` in repo — reflection-based; [RequiresUnreferencedCode] confirmed present
- `BlackBoxBuddy.Tests.csproj` in repo — test project using Avalonia.Headless.XUnit + xunit.v3.mtp-v2 confirmed

### Secondary (MEDIUM confidence)
- [NuGet: Xaml.Behaviors.Avalonia 12.0.0-rc1](https://www.nuget.org/packages/Xaml.Behaviors.Avalonia) — Avalonia 12 aligned pre-release; AdaptiveBehavior API
- [.planning/research/ARCHITECTURE.md](/.planning/research/ARCHITECTURE.md) — IDashcamDevice interface segregation pattern
- [.planning/research/PITFALLS.md](/.planning/research/PITFALLS.md) — ObservableCollection threading, ViewLocator AOT, RC1 instability
- [.planning/research/STACK.md](/.planning/research/STACK.md) — library versions and compatibility matrix

### Tertiary (LOW confidence)
- None in scope for this phase — all foundation patterns are well-documented standard Avalonia 12 MVVM

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages confirmed in official sources; versions already pinned in repo
- Architecture: HIGH — Avalonia 12 navigation APIs, DI pattern, MVVM pattern all confirmed in official docs
- Pitfalls: HIGH — ViewLocator pitfall evidenced in existing code ([RequiresUnreferencedCode]); threading pitfall confirmed in Avalonia GitHub discussions; RC1 instability mitigated by already-pinned versions

**Research date:** 2026-03-24
**Valid until:** 2026-04-23 (stable; Avalonia RC1 docs are unlikely to change but monitor Avalonia releases)
