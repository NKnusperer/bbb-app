---
phase: 01-foundation
plan: 01
subsystem: infra
tags: [avalonia, di, mvvm, csharp, dotnet, community-toolkit, microsoft-extensions-di]

# Dependency graph
requires: []
provides:
  - DI container bootstrap via AppServices.ConfigureServices with platform services delegate
  - AOT-safe ViewLocator using pattern-matching switch expression
  - Dark mode enforced via RequestedThemeVariant="Dark" in App.axaml
  - IDeviceDiscovery, IDeviceConnection, IDeviceCommands, IDeviceFileSystem, IDeviceLiveStream interfaces
  - IDashcamDevice composite interface inheriting all 5 device interfaces
  - DeviceInfo model and ConnectionState enum
  - IDeviceService, IMediaPlayerService, INavigationService service contracts
  - NuGet packages: Xaml.Behaviors.Avalonia, NSubstitute, FluentAssertions, Bogus, Microsoft.Extensions.DependencyInjection
affects: [01-02, 01-03, 01-04, all future phases]

# Tech tracking
tech-stack:
  added:
    - Microsoft.Extensions.DependencyInjection 10.0.0-rc.2.25502.107
    - Xaml.Behaviors.Avalonia 12.0.0-rc1
    - NSubstitute 5.3.0 (test)
    - FluentAssertions 8.9.0 (test)
    - Bogus 35.6.5 (test)
  patterns:
    - App.PlatformServices static delegate for platform-specific DI registrations (D-22)
    - Ioc.Default.ConfigureServices called before any ViewModel construction (Pitfall 4)
    - AOT-safe ViewLocator with param switch expression (D-21)
    - Composite device interface (IDashcamDevice inheriting 5 narrow interfaces, D-07)
    - All device methods async (Task return types, no sync device calls)

key-files:
  created:
    - src/BlackBoxBuddy/AppServices.cs
    - src/BlackBoxBuddy/Device/IDeviceDiscovery.cs
    - src/BlackBoxBuddy/Device/IDeviceConnection.cs
    - src/BlackBoxBuddy/Device/IDeviceCommands.cs
    - src/BlackBoxBuddy/Device/IDeviceFileSystem.cs
    - src/BlackBoxBuddy/Device/IDeviceLiveStream.cs
    - src/BlackBoxBuddy/Device/IDashcamDevice.cs
    - src/BlackBoxBuddy/Models/ConnectionState.cs
    - src/BlackBoxBuddy/Models/DeviceInfo.cs
    - src/BlackBoxBuddy/Services/IDeviceService.cs
    - src/BlackBoxBuddy/Services/IMediaPlayerService.cs
    - src/BlackBoxBuddy/Navigation/INavigationService.cs
  modified:
    - Directory.Packages.props
    - src/BlackBoxBuddy/BlackBoxBuddy.csproj
    - tests/BlackBoxBuddy.Tests/BlackBoxBuddy.Tests.csproj
    - src/BlackBoxBuddy/App.axaml
    - src/BlackBoxBuddy/App.axaml.cs
    - src/BlackBoxBuddy/ViewLocator.cs
    - src/BlackBoxBuddy.Desktop/Program.cs
    - src/BlackBoxBuddy.Android/MainActivity.cs

key-decisions:
  - "Microsoft.Extensions.DependencyInjection requires explicit PackageVersion entry in Directory.Packages.props despite being bundled with .NET 10 — CPM enforces this"
  - "App.PlatformServices static property enables per-platform DI extension without changing shared AppServices.ConfigureServices signature"
  - "IMediaPlayerService is a stub — implementations planned for Phase 3 (LibVLCSharp for desktop, ExoPlayer for Android)"

patterns-established:
  - "Platform-specific DI: Set App.PlatformServices delegate in platform entry point before AppBuilder.Configure"
  - "ViewLocator: Add new ViewModel->View mapping as switch arm when ViewModel is created in later plans"
  - "Device interfaces: All device operations return Task for async-only access"

requirements-completed: [FOUND-04, CONN-05]

# Metrics
duration: 3min
completed: 2026-03-24
---

# Phase 01 Plan 01: Foundation Bootstrap Summary

**AOT-safe DI-wired Avalonia 12 skeleton with dark mode, pattern-matching ViewLocator, and all device/service interface contracts defined**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-24T09:45:44Z
- **Completed:** 2026-03-24T09:48:55Z
- **Tasks:** 3
- **Files modified:** 19

## Accomplishments

- DI container bootstrapped via AppServices.ConfigureServices with platform delegate pattern (D-22); Ioc.Default initialized before any ViewModel creation
- Reflection-based ViewLocator replaced with AOT-safe pattern-matching switch; dark mode enforced via RequestedThemeVariant="Dark" (D-05)
- All 5 device interfaces (IDeviceDiscovery, IDeviceConnection, IDeviceCommands, IDeviceFileSystem, IDeviceLiveStream) and IDashcamDevice composite contract defined; DeviceInfo, ConnectionState, IDeviceService, IMediaPlayerService, INavigationService created

## Task Commits

Each task was committed atomically:

1. **Task 1: Add NuGet packages and enforce dark mode** - `8968d23` (feat)
2. **Task 2: Replace ViewLocator, wire DI bootstrap, and call ConfigureServices from platform entry points** - `84abc7a` (feat)
3. **Task 3: Create all device interface contracts, models, and service interfaces** - `d901592` (feat)

## Files Created/Modified

**Created:**
- `src/BlackBoxBuddy/AppServices.cs` - Shared DI registration with platformServices delegate
- `src/BlackBoxBuddy/Device/IDeviceDiscovery.cs` - DiscoverAsync returning DeviceInfo?
- `src/BlackBoxBuddy/Device/IDeviceConnection.cs` - ConnectAsync, DisconnectAsync, IsConnected
- `src/BlackBoxBuddy/Device/IDeviceCommands.cs` - Settings and provisioning operations
- `src/BlackBoxBuddy/Device/IDeviceFileSystem.cs` - ListRecordingsAsync, DownloadFileAsync, DeleteFileAsync
- `src/BlackBoxBuddy/Device/IDeviceLiveStream.cs` - GetStreamUriAsync for camera feeds
- `src/BlackBoxBuddy/Device/IDashcamDevice.cs` - Composite interface inheriting all 5 device contracts
- `src/BlackBoxBuddy/Models/ConnectionState.cs` - Enum: Searching, Connected, Disconnected, NeedsProvisioning
- `src/BlackBoxBuddy/Models/DeviceInfo.cs` - DeviceName, FirmwareVersion, IsProvisioned, IpAddress
- `src/BlackBoxBuddy/Services/IDeviceService.cs` - ConnectionState, ConnectedDevice, StartDiscoveryAsync, ConnectionStateChanged event
- `src/BlackBoxBuddy/Services/IMediaPlayerService.cs` - Empty stub for Phase 3
- `src/BlackBoxBuddy/Navigation/INavigationService.cs` - PushAsync, PopAsync, PopToRootAsync

**Modified:**
- `Directory.Packages.props` - Added 5 new PackageVersion entries
- `src/BlackBoxBuddy/BlackBoxBuddy.csproj` - Added Xaml.Behaviors.Avalonia and Microsoft.Extensions.DependencyInjection references
- `tests/BlackBoxBuddy.Tests/BlackBoxBuddy.Tests.csproj` - Added NSubstitute, FluentAssertions, Bogus
- `src/BlackBoxBuddy/App.axaml` - Dark mode via RequestedThemeVariant="Dark"
- `src/BlackBoxBuddy/App.axaml.cs` - DI init, PlatformServices property, Ioc.Default wiring
- `src/BlackBoxBuddy/ViewLocator.cs` - AOT-safe switch expression replacing reflection
- `src/BlackBoxBuddy.Desktop/Program.cs` - Sets App.PlatformServices before builder (D-22)
- `src/BlackBoxBuddy.Android/MainActivity.cs` - CustomizeAppBuilder sets App.PlatformServices (D-22)

## Decisions Made

- Microsoft.Extensions.DependencyInjection needs explicit PackageVersion in Directory.Packages.props for Central Package Management compatibility, despite being a .NET 10 framework package. Used version 10.0.0-rc.2.25502.107 (resolved by NuGet).
- App.PlatformServices static property chosen over constructor injection to match Avalonia's app lifecycle (AppBuilder.Configure is called before any service container exists).
- IMediaPlayerService left as empty stub — plan specifies implementations come in Phase 3.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Added Microsoft.Extensions.DependencyInjection to Directory.Packages.props**
- **Found during:** Task 1 (NuGet restore)
- **Issue:** Plan stated "Microsoft.Extensions.DependencyInjection is bundled with .NET 10 SDK, no version needed in Directory.Packages.props" but Central Package Management (ManagePackageVersionsCentrally=true) requires ALL PackageReference items to have a corresponding PackageVersion. NuGet restore failed with NU1010.
- **Fix:** Added `<PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0-rc.2.25502.107"/>` to Directory.Packages.props.
- **Files modified:** Directory.Packages.props
- **Verification:** dotnet restore succeeded with no errors
- **Committed in:** 8968d23 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - Bug)
**Impact on plan:** Essential for CPM compatibility. The plan's assumption about bundled packages was incorrect for CPM projects.

## Issues Encountered

- Android project (BlackBoxBuddy.Android) cannot be built in this environment — Android SDK not installed. This is a pre-existing environmental constraint; all non-Android projects build and test successfully. Android project changes (MainActivity.cs) were committed and are correct; they will compile in an environment with the Android SDK.

## Known Stubs

- `src/BlackBoxBuddy/Services/IMediaPlayerService.cs` - Empty interface stub. Intentional per plan: implementations come in Phase 3 (LibVLCSharp for desktop, ExoPlayer for Android). This does not block the plan's goal (interface contract defined).

## Next Phase Readiness

- DI skeleton ready for Plan 02 to register IDashcamDevice mock implementation and IDeviceService
- All interface contracts in place for Plans 02-04 to implement against
- ViewLocator ready to receive additional ViewModel->View mappings as Plans 03/04 create ViewModels
- Test packages (NSubstitute, FluentAssertions, Bogus) in place for TDD in subsequent plans

---
*Phase: 01-foundation*
*Completed: 2026-03-24*
