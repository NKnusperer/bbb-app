# Architecture Research

**Domain:** Cross-platform dashcam management desktop + mobile app
**Researched:** 2026-03-24
**Confidence:** HIGH (Avalonia 12 navigation, MVVM patterns, device abstraction) / MEDIUM (video pipeline specifics)

## Standard Architecture

### System Overview

```
┌────────────────────────────────────────────────────────────────────┐
│                          UI Layer                                  │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              Avalonia 12 Page Navigation Shell              │    │
│  │  DrawerPage (portrait) / NavigationPage (landscape/desktop) │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐   │    │
│  │  │Dashboard │ │Recordings│ │ LiveFeed │ │  Settings    │   │    │
│  │  │  Page    │ │  Page    │ │  Page    │ │  Page        │   │    │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────────┘   │    │
│  └────────────────────────────────────────────────────────────┘    │
│                          ViewLocator (IDataTemplate)               │
├────────────────────────────────────────────────────────────────────┤
│                       ViewModel Layer                              │
│  ┌──────────┐ ┌────────────┐ ┌──────────┐ ┌──────────────────┐    │
│  │Dashboard │ │Recordings  │ │LiveFeed  │ │ Settings         │    │
│  │ViewModel │ │ViewModel   │ │ViewModel │ │ ViewModels       │    │
│  └────┬─────┘ └─────┬──────┘ └────┬─────┘ └────────┬─────────┘    │
│       └─────────────┴─────────────┴────────────────┘              │
│                             │                                      │
├─────────────────────────────┼──────────────────────────────────────┤
│                       Service Layer                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │ DeviceService   │  │ RecordingService │  │ MediaPlayerSvc   │   │
│  │ (discovery,     │  │ (browse, filter, │  │ (video playback  │   │
│  │  connection,    │  │  archive, trips) │  │  abstraction)    │   │
│  │  provisioning)  │  └─────────────────┘  └──────────────────┘   │
│  └────────┬────────┘                                               │
│           │                                                        │
├───────────┼────────────────────────────────────────────────────────┤
│           │              Device Abstraction Layer                  │
│           ▼                                                        │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │                    IDashcamDevice                          │    │
│  │  IDeviceDiscovery  IDeviceConnection  IDeviceCommands      │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │  MockDashcamDevice    │    VendorXDashcamDevice (future)   │    │
│  └────────────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Communicates With |
|-----------|----------------|-------------------|
| Navigation Shell | Adaptive layout host; vertical nav in portrait, horizontal in landscape; connection indicator | Top-level ViewModels |
| DashboardPage + VM | Recent recordings, trips summary, events feed | RecordingService, DeviceService |
| RecordingsPage + VM | Browseable list with filters; detail drill-down; archive trigger | RecordingService, MediaPlayerService |
| LiveFeedPage + VM | Camera stream toggle (front/rear), stream status | DeviceService, MediaPlayerService |
| SettingsPage + VM | Device config forms (WiFi, recording modes, sensors, overlays); danger zone | DeviceService |
| DeviceService | Discovery, connection lifecycle, provisioning flow, status polling | IDashcamDevice |
| RecordingService | Enumerate files from device, build virtual trip groups, archive to local storage, surface metadata | IDashcamDevice, local filesystem |
| MediaPlayerService | Platform-appropriate video playback (LibVLC on desktop, ExoPlayer on Android); thumbnail generation | INativeMediaPlayerService |
| IDashcamDevice | Vendor-agnostic contract: discovery, connect, commands, file enumeration, live stream URI | Implemented by MockDevice / future real vendors |
| ViewLocator | Maps ViewModel types to View types using pattern matching (not reflection) | All ViewModels and Views |

## Recommended Project Structure

```
src/
├── BlackBoxBuddy/                  # Shared UI + core (Avalonia class library)
│   ├── App.axaml(.cs)              # Application bootstrap
│   ├── ViewLocator.cs              # IDataTemplate ViewModel→View mapper
│   ├── Assets/                     # Icons, images, fonts
│   ├── Views/                      # .axaml views (one per ContentPage)
│   │   ├── Shell/
│   │   │   ├── AppShellView.axaml  # DrawerPage or NavigationPage root
│   │   │   └── NavBarView.axaml    # Icon-only nav component
│   │   ├── Dashboard/
│   │   │   └── DashboardPage.axaml
│   │   ├── Recordings/
│   │   │   ├── RecordingsPage.axaml
│   │   │   ├── RecordingDetailPage.axaml
│   │   │   └── TripDetailPage.axaml
│   │   ├── LiveFeed/
│   │   │   └── LiveFeedPage.axaml
│   │   ├── Settings/
│   │   │   └── SettingsPage.axaml
│   │   └── Provisioning/
│   │       └── ProvisioningPage.axaml
│   ├── ViewModels/                 # One ViewModel per page + sub-ViewModels
│   │   ├── ViewModelBase.cs        # ObservableObject base (CommunityToolkit)
│   │   ├── Shell/
│   │   │   └── AppShellViewModel.cs
│   │   ├── Dashboard/
│   │   │   └── DashboardViewModel.cs
│   │   ├── Recordings/
│   │   │   ├── RecordingsViewModel.cs
│   │   │   ├── RecordingDetailViewModel.cs
│   │   │   └── TripDetailViewModel.cs
│   │   ├── LiveFeed/
│   │   │   └── LiveFeedViewModel.cs
│   │   ├── Settings/
│   │   │   └── SettingsViewModel.cs
│   │   └── Provisioning/
│   │       └── ProvisioningViewModel.cs
│   ├── Services/                   # Platform-agnostic service interfaces + impls
│   │   ├── IDeviceService.cs
│   │   ├── DeviceService.cs
│   │   ├── IRecordingService.cs
│   │   ├── RecordingService.cs
│   │   ├── IMediaPlayerService.cs  # Thin abstraction over platform video
│   │   └── IArchiveService.cs
│   ├── Device/                     # Vendor abstraction layer
│   │   ├── IDashcamDevice.cs       # Core vendor contract
│   │   ├── IDeviceDiscovery.cs
│   │   ├── IDeviceConnection.cs
│   │   ├── IDeviceCommands.cs      # Settings read/write
│   │   ├── IDeviceFileSystem.cs    # Recording enumeration
│   │   ├── IDeviceLiveStream.cs    # Live feed URI
│   │   ├── Mock/
│   │   │   └── MockDashcamDevice.cs
│   │   └── DeviceRegistry.cs      # Maps discovered device type to impl
│   └── Models/                     # Domain models
│       ├── Recording.cs
│       ├── Trip.cs
│       ├── RecordingMetadata.cs    # GPS, G-force, speed, etc.
│       ├── DeviceConfig.cs
│       └── DeviceInfo.cs
│
├── BlackBoxBuddy.Desktop/          # Desktop entry point
│   ├── Program.cs                  # AppBuilder + LibVLC init
│   └── Services/
│       └── VlcMediaPlayerService.cs  # LibVLCSharp.Avalonia impl
│
├── BlackBoxBuddy.Android/          # Android entry point
│   ├── MainActivity.cs             # ExoPlayer service registration
│   └── Services/
│       └── ExoPlayerMediaService.cs
│
tests/
└── BlackBoxBuddy.Tests/
    ├── ViewModels/                  # ViewModel unit tests (Avalonia Headless)
    ├── Services/                    # Service tests with MockDashcamDevice
    └── Device/                      # Device abstraction tests
```

### Structure Rationale

- **Device/ folder:** Isolates vendor contracts so adding a real device requires only a new implementation folder, no changes to services or ViewModels.
- **Services/ in shared project:** All business logic is platform-agnostic. Platform-specific services (video, file paths) are registered via DI in each platform entry point.
- **Views/ and ViewModels/ mirrored folders:** Feature-based sub-folders (Dashboard/, Recordings/, etc.) keep related files co-located and match Avalonia 12's ContentPage-per-screen model.
- **Platform projects only contain:** Entry point, platform service registrations, and platform-specific service implementations. No business logic.

## Architectural Patterns

### Pattern 1: Vendor Device Abstraction via Interface Segregation

**What:** Split `IDashcamDevice` into narrow role interfaces (`IDeviceDiscovery`, `IDeviceConnection`, `IDeviceCommands`, `IDeviceFileSystem`, `IDeviceLiveStream`). Each vendor implementation only implements the interfaces relevant to its capabilities.

**When to use:** Now. Even with only a mock device, this prevents a single fat interface that breaks when vendor B has no live stream capability.

**Trade-offs:** Slightly more interfaces to manage; pays back immediately when adding real vendor support or writing focused unit tests per capability.

**Example:**
```csharp
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

public interface IDeviceCommands
{
    Task<DeviceConfig> GetConfigAsync(CancellationToken ct);
    Task ApplyConfigAsync(DeviceConfig config, CancellationToken ct);
}
```

### Pattern 2: Platform Video Service via Constructor Injection

**What:** Define `IMediaPlayerService` in the shared project. Each platform project provides a concrete implementation (`VlcMediaPlayerService` on desktop using LibVLCSharp, `ExoPlayerMediaService` on Android). Register via DI in each platform's entry point.

**When to use:** From phase 1. The shared code calls only `IMediaPlayerService.CreatePlayerControl()` and `Play(uri)`.

**Trade-offs:** Requires DI setup in App.axaml.cs from the start; adds constructor parameters to ViewModels. Worth it — the alternative (platform `#if` blocks scattered through ViewModels) is untestable.

**Example:**
```csharp
// Shared project
public interface IMediaPlayerService
{
    Control CreatePlayerControl();
    Task PlayAsync(string uri, CancellationToken ct);
    void Stop();
}

// Desktop project Program.cs
services.AddSingleton<IMediaPlayerService, VlcMediaPlayerService>();

// Android MainActivity.cs
services.AddSingleton<IMediaPlayerService, ExoPlayerMediaService>();
```

### Pattern 3: Adaptive Shell Layout via Window Width Class

**What:** Subscribe to `Window.Bounds` changes. Apply a CSS-style class (`portrait` or `landscape`) to the root element. In XAML styles, the nav bar uses `vertical` orientation when `portrait` and `horizontal` when `landscape`. The nav container switches between `DrawerPage` (mobile portrait) and a persistent sidebar (desktop landscape).

**When to use:** From the first UI phase. Baking in responsiveness from the start avoids a costly layout rewrite later.

**Trade-offs:** Requires a bounds-change subscription and style class toggling in code-behind or the shell ViewModel. Avalonia does not have built-in breakpoints (unlike Bootstrap), so this is manual.

**Example:**
```csharp
// AppShellViewModel.cs
[ObservableProperty]
private string _layoutClass = "portrait";

private void OnBoundsChanged(Rect bounds) =>
    LayoutClass = bounds.Width >= 600 ? "landscape" : "portrait";
```
```xml
<!-- AppShellView.axaml — nav bar orientation driven by style class -->
<StackPanel Classes.vertical="{Binding LayoutClass, Converter={...}EqualTo, ConverterParameter=portrait}"
            Classes.horizontal="{Binding LayoutClass, Converter={...}EqualTo, ConverterParameter=landscape}">
```

### Pattern 4: Virtual Trip Grouping as a Pure Model Transform

**What:** Recordings are physical short clips. Trips are logical groupings of consecutive clips. Build trips by sorting recordings by timestamp and grouping those whose start time falls within a configurable gap threshold of the previous clip's end time. This is a pure function over a list of `Recording` objects — no database, no stored state.

**When to use:** In `RecordingService.GetTripsAsync()`. Trips are derived, never persisted on device (archiving exports the video files, not the grouping metadata).

**Trade-offs:** Recomputed on each load; fast enough for typical dashcam clip counts (hundreds, not millions).

## Data Flow

### Device Discovery and Connection Flow

```
App Startup
    ↓
DeviceService.DiscoverAsync()
    ↓
IDeviceDiscovery.DiscoverAsync()  ← Mock returns fake DeviceInfo immediately
    ↓ (success)
DeviceService raises DeviceConnected event
    ↓
AppShellViewModel.OnDeviceConnected()
    → Updates connection indicator
    → Navigates to DashboardPage if previously on provisioning
    ↓ (failure / null)
AppShellViewModel navigates to ProvisioningPage
```

### Recording Browse and Archive Flow

```
RecordingsViewModel.LoadAsync()
    ↓
RecordingService.GetRecordingsAsync()
    ↓
IDeviceFileSystem.ListFilesAsync()  ← returns list of file descriptors + metadata
    ↓
RecordingService groups into virtual Trips
    ↓
RecordingsViewModel receives IReadOnlyList<IRecordingGroup>
    ↓ (user selects archive)
RecordingService.ArchiveAsync(group, destinationPath)
    ↓
IDeviceFileSystem.DownloadFileAsync() per clip → local disk
```

### Live Feed Flow

```
LiveFeedViewModel.StartAsync()
    ↓
DeviceService.GetLiveStreamUriAsync(channel: Front|Rear)
    ↓
IDeviceLiveStream.GetStreamUriAsync()  ← returns rtsp:// or http:// URI
    ↓
IMediaPlayerService.PlayAsync(uri)
    ↓
Platform player (VLC / ExoPlayer) renders into VideoView control
    ↓
LiveFeedViewModel.IsStreaming = true
```

### Settings Write Flow

```
SettingsViewModel command triggered (e.g., ApplyWifiSettings)
    ↓
SettingsViewModel validates input → if invalid, surface error via ObservableValidator
    ↓
DeviceService.ApplyConfigAsync(partialConfig)
    ↓
IDeviceCommands.ApplyConfigAsync()  ← Mock persists in-memory
    ↓
DeviceService.GetConfigAsync()  ← re-read to confirm
    ↓
SettingsViewModel updates bound properties
```

### MVVM State Flow

```
User Action (button click / form field)
    ↓
RelayCommand / ObservableProperty (CommunityToolkit source generators)
    ↓
ViewModel calls Service (async, with CancellationToken)
    ↓
Service calls IDashcamDevice interface
    ↓
Response flows back as returned values or events
    ↓
ViewModel updates ObservableProperty
    ↓
Avalonia data binding propagates change to View automatically
```

## Build Order Implications

The component dependency graph drives the recommended build sequence:

1. **Device contracts first** — `IDashcamDevice` and its sub-interfaces are depended on by every layer. Define them (with `MockDashcamDevice`) before writing any service or ViewModel.
2. **Services second** — `DeviceService` and `RecordingService` can be fully tested against the mock once interfaces exist. MediaPlayerService interface is defined here; implementation deferred to platform projects.
3. **Shell + navigation third** — `AppShellViewModel` with connection indicator and page routing can be built once services exist to report device state.
4. **Feature pages in dependency order:**
   a. `ProvisioningPage` (first screen user sees; blocks on device state)
   b. `DashboardPage` (needs RecordingService summary data)
   c. `SettingsPage` (needs DeviceService config read/write)
   d. `RecordingsPage + detail` (needs RecordingService full enumeration + metadata)
   e. `LiveFeedPage` (needs MediaPlayerService; platform-specific; last to integrate)
5. **Platform video implementations last** — LibVLC (desktop) and ExoPlayer (Android) integrations are self-contained platform project work that slots in against the existing `IMediaPlayerService` contract.

## Scaling Considerations

This is a local, single-device, single-user desktop/mobile app. Traditional server-scaling concerns do not apply. The relevant "scaling" is UI complexity as features grow.

| Concern | Current Approach | When to Revisit |
|---------|-----------------|-----------------|
| Navigation depth | Flat NavigationPage push/pop | If drill-down exceeds 3 levels, consider tabbed sub-navigation |
| ViewModel complexity | One VM per page | If a page VM exceeds ~300 lines, extract child ViewModels |
| RecordingService perf | In-memory trip grouping | If clip counts exceed ~10,000, add background indexing with progress feedback |
| Device protocol | Mock only | When real device SDK lands, add new `IDeviceXxx` implementation behind registry |
| Test coverage | Headless Avalonia + xunit.v3 | Maintain test coverage at each phase gate before adding new features |

## Anti-Patterns

### Anti-Pattern 1: Reflection-Based ViewLocator in Production

**What people do:** Ship the default template ViewLocator that uses `Type.GetType(name)` via reflection.

**Why it's wrong:** Incompatible with Native AOT trimming (which .NET 10 encourages), no compile-time safety, silent failures at runtime when a view is renamed.

**Do this instead:** Replace with pattern-matching switch expression or DI-combined pattern matching as documented in [Avalonia View Locator docs](https://docs.avaloniaui.net/docs/data-templates/view-locator). The skeleton already has the reflection-based version — replace it early.

### Anti-Pattern 2: Platform Logic in Shared ViewModels

**What people do:** Use `#if ANDROID` / `#if DESKTOP` preprocessor blocks inside ViewModels or Services in the shared project.

**Why it's wrong:** Breaks testability (you can't test the shared project in isolation), violates DI principles, and causes merge conflicts when platform behavior diverges.

**Do this instead:** Define an interface in the shared project. Register platform-specific implementations in each platform project's DI setup. ViewModels only know the interface.

### Anti-Pattern 3: Fat IDashcamDevice with Optional Methods

**What people do:** Put all possible vendor capabilities on a single interface with `NotImplementedException` throws for features a vendor doesn't support.

**Why it's wrong:** Callers cannot know at compile time which capabilities a device supports. Leads to defensive runtime checks everywhere. Adding vendor B breaks existing tests.

**Do this instead:** Interface segregation — `IDashcamDevice` composes narrow interfaces. Service code capability-checks via `device is IDeviceLiveStream liveStream` before calling live-feed methods.

### Anti-Pattern 4: Storing Trips on the Device or in a Local Database

**What people do:** Persist trip grouping metadata to a local SQLite database to avoid recomputing on load.

**Why it's wrong:** Trips are a derived view over timestamped clips. The clips themselves are the source of truth and are on the device. A local DB becomes a cache with a staleness/sync problem when the dashcam overwrites recordings.

**Do this instead:** Compute trips as a pure in-memory transform in `RecordingService.GetTripsAsync()` every time the recordings list is loaded. Persist nothing except the user's explicit archives.

### Anti-Pattern 5: Blocking UI Thread for Device Communication

**What people do:** Call `IDashcamDevice` methods synchronously from RelayCommand handlers.

**Why it's wrong:** Dashcam communication is over WiFi; any call can take hundreds of milliseconds. Blocking the UI thread produces a frozen, unresponsive app.

**Do this instead:** All device interface methods return `Task<T>`. Commands use `AsyncRelayCommand` from CommunityToolkit. ViewModels expose `IsLoading` / `IsBusy` observable properties for UI feedback.

## Integration Points

### Internal Boundaries

| Boundary | Communication Pattern | Notes |
|----------|----------------------|-------|
| ViewModel → Service | Constructor-injected interface, `async/await` | Never direct device calls from ViewModel |
| Service → IDashcamDevice | Constructor-injected interface, `async/await` with `CancellationToken` | Service owns connection lifecycle |
| AppShell → Child ViewModels | NavigationPage push/pop; shared AppShellViewModel holds connection state | Child VMs receive device state via injected `IDeviceService` |
| Shared → Platform services | DI registration in platform entry points; shared code depends only on interface | LibVLC init happens in `Program.cs`; ExoPlayer init in `MainActivity.cs` |
| RecordingService → Local filesystem | `System.IO` abstractions (`IFileSystem` wrapper recommended for testability) | Archive destination paths resolved per platform in service registration |

### External Device Integration

| Integration | Pattern | Notes |
|-------------|---------|-------|
| WiFi device discovery | `IDeviceDiscovery` via UDP broadcast or mDNS (Zeroconf); `novotnyllc/Zeroconf` library is a well-maintained .NET option | Mock implementation returns a fixed `DeviceInfo` immediately |
| Device HTTP API (future) | `IDeviceCommands` implemented with `HttpClient` behind `IHttpClientFactory` | One `HttpClient` per device connection, not per request |
| Live stream (future) | `IDeviceLiveStream` returns an `rtsp://` or `http://` URI; VLC and ExoPlayer both consume these natively | No custom streaming code needed in the app layer |
| Local archive storage | `IArchiveService` wraps `System.IO.File.Copy` / stream operations | Path resolution per platform via `Environment.SpecialFolder` or Android-specific APIs |

## Sources

- [Avalonia ContentPage docs](https://docs.avaloniaui.net/controls/navigation/contentpage)
- [Avalonia TabbedPage docs](https://docs.avaloniaui.net/controls/navigation/tabbedpage)
- [Avalonia View Locator docs](https://docs.avaloniaui.net/docs/data-templates/view-locator)
- [Avalonia MVVM pattern docs](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/avalonia-ui-and-mvvm)
- [Avalonia page-based navigation issue](https://github.com/AvaloniaUI/Avalonia/issues/10557)
- [LibVLCSharp + ExoPlayer Avalonia cross-platform video integration](https://monsalma.net/avalonia-ui-native-video-playback-featuring-libvlcsharp-and-exoplayer/)
- [LibVLCSharp.Avalonia NuGet](https://www.nuget.org/packages/LibVLCSharp.Avalonia)
- [CommunityToolkit.Mvvm docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Avalonia DI guide](https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-implement-dependency-injection)
- [Zeroconf .NET library](https://github.com/novotnyllc/Zeroconf)
- [Avalonia adaptive layout discussion](https://github.com/AvaloniaUI/Avalonia/discussions/6130)

---
*Architecture research for: BlackBoxBuddy — cross-platform dashcam management app*
*Researched: 2026-03-24*
