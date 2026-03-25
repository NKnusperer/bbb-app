---
phase: 01-foundation
plan: 03
subsystem: ui
tags: [avalonia, tabbed-navigation, mvvm, dependency-injection, adaptive-layout]

# Dependency graph
requires:
  - phase: 01-foundation plan 01
    provides: ViewModelBase, IDeviceService, INavigationService interface, AppServices DI bootstrap, ConnectionState enum, DeviceInfo model

provides:
  - AppShellView: TabbedPage root shell with 4 ContentPage tabs and persistent ConnectionIndicator overlay
  - AppShellViewModel: shell state management subscribing to IDeviceService.ConnectionStateChanged
  - ConnectionIndicator UserControl: state-specific color dot (yellow/green/orange/red) + status text
  - ConnectionStateToBrushConverter: maps ConnectionState enum to SolidColorBrush for Ellipse fill
  - NavigationService: stub INavigationService implementation (Plan 04 will wire NavigationPage)
  - 4 placeholder ViewModels: DashboardViewModel, RecordingsViewModel, LiveFeedViewModel, SettingsViewModel
  - 4 placeholder ContentPages: DashboardPage, RecordingsPage, LiveFeedPage, SettingsPage
  - ViewLocator mappings for all 5 new ViewModels
  - DI registrations: INavigationService + 5 transient ViewModels in AppServices
  - App.axaml.cs: resolves AppShellViewModel from DI, sets AppShellView as main window content

affects: [02-mock-device, 03-recordings, 04-connection]

# Tech tracking
tech-stack:
  added:
    - Xaml.Behaviors.Avalonia 12.0.0-rc1 (via meta-package already in project): AdaptiveBehavior, AdaptiveClassSetter from Avalonia.Xaml.Interactions.Responsive namespace
  patterns:
    - IValueConverter (ConnectionStateToBrushConverter) for enum-to-brush mapping
    - Page.Icon with PathGeometry for icon-only tab display
    - AdaptiveBehavior with portrait/landscape CSS class switching at 600px width
    - AppShellViewModel subscribes to service events in constructor, unsubscription deferred to Plan 04

key-files:
  created:
    - src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs
    - src/BlackBoxBuddy/Views/Shell/AppShellView.axaml
    - src/BlackBoxBuddy/Views/Shell/AppShellView.axaml.cs
    - src/BlackBoxBuddy/Views/Shell/ConnectionIndicator.axaml
    - src/BlackBoxBuddy/Views/Shell/ConnectionIndicator.axaml.cs
    - src/BlackBoxBuddy/Converters/ConnectionStateToBrushConverter.cs
    - src/BlackBoxBuddy/Navigation/NavigationService.cs
    - src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs
    - src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs
    - src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs
    - src/BlackBoxBuddy/ViewModels/SettingsViewModel.cs
    - src/BlackBoxBuddy/Views/DashboardPage.axaml
    - src/BlackBoxBuddy/Views/DashboardPage.axaml.cs
    - src/BlackBoxBuddy/Views/RecordingsPage.axaml
    - src/BlackBoxBuddy/Views/RecordingsPage.axaml.cs
    - src/BlackBoxBuddy/Views/LiveFeedPage.axaml
    - src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs
    - src/BlackBoxBuddy/Views/SettingsPage.axaml
    - src/BlackBoxBuddy/Views/SettingsPage.axaml.cs
  modified:
    - src/BlackBoxBuddy/ViewLocator.cs
    - src/BlackBoxBuddy/AppServices.cs
    - src/BlackBoxBuddy/App.axaml.cs
    - src/BlackBoxBuddy/Views/MainWindow.axaml

key-decisions:
  - "ContentPage uses Header (not Title) property for tab strip label; Icon property accepts PathGeometry for icon-only tabs"
  - "AdaptiveBehavior namespace in Avalonia 12 RC1 is Avalonia.Xaml.Interactions.Responsive (not Avalonia.Xaml.Interactions.Core as plan template showed)"
  - "ConnectionState-to-color mapping implemented as IValueConverter rather than multiple IsVisible ellipses — cleaner XAML"
  - "NavigationService is a stub; full implementation deferred to Plan 04 when provisioning wizard requires NavigationPage"

patterns-established:
  - "Shell pattern: UserControl wraps TabbedPage + overlay (ConnectionIndicator in Grid.Row=0)"
  - "Color binding pattern: use IValueConverter to map enums to brushes"
  - "Adaptive layout: ia:Interaction.Behaviors + AdaptiveBehavior on root UserControl"

requirements-completed: [FOUND-01, FOUND-02, FOUND-03, FOUND-04, FOUND-05, FOUND-06]

# Metrics
duration: 20min
completed: 2026-03-24
---

# Phase 01 Plan 03: App Shell Summary

**Dark-mode TabbedPage shell with 4 icon-only tabs, persistent ConnectionIndicator with state-specific colors (yellow/green/orange/red), AdaptiveBehavior 600px breakpoint, and NavigationService wired via DI**

## Performance

- **Duration:** ~20 min
- **Started:** 2026-03-24T10:00:00Z
- **Completed:** 2026-03-24T10:20:00Z
- **Tasks:** 2
- **Files modified:** 23 (19 created, 4 modified)

## Accomplishments
- TabbedPage root shell with 4 icon-only ContentPage tabs (Dashboard, Recordings, Live Feed, Settings) using PathGeometry icons
- ConnectionIndicator UserControl with state-specific Ellipse fill via ConnectionStateToBrushConverter (Searching=yellow, Connected=green, NeedsProvisioning=orange, Disconnected=red)
- AppShellViewModel subscribing to IDeviceService.ConnectionStateChanged, exposing ConnectionState and ConnectionStatusText
- AdaptiveBehavior at 600px breakpoint setting portrait/landscape CSS classes on AppShellView
- NavigationService registered in DI; placeholder implementation ready for Plan 04

## Task Commits

Each task was committed atomically:

1. **Task 1: Create placeholder ViewModels and pages for all 4 tabs** - `2653f0e` (feat)
2. **Task 2: Build AppShellView with TabbedPage, connection indicator, responsive layout, and NavigationService** - `4687858` (feat)

**Plan metadata:** committed with SUMMARY (docs)

## Files Created/Modified

- `src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs` - Shell state VM subscribing to IDeviceService events
- `src/BlackBoxBuddy/Views/Shell/AppShellView.axaml` - TabbedPage root with ConnectionIndicator overlay + AdaptiveBehavior
- `src/BlackBoxBuddy/Views/Shell/ConnectionIndicator.axaml` - Connection status dot + text with command binding
- `src/BlackBoxBuddy/Converters/ConnectionStateToBrushConverter.cs` - Maps ConnectionState to color brushes
- `src/BlackBoxBuddy/Navigation/NavigationService.cs` - Stub INavigationService (Plan 04 wires NavigationPage)
- `src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs` - Placeholder Dashboard VM
- `src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs` - Placeholder Recordings VM
- `src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs` - Placeholder Live Feed VM
- `src/BlackBoxBuddy/ViewModels/SettingsViewModel.cs` - Placeholder Settings VM
- `src/BlackBoxBuddy/Views/DashboardPage.axaml` - Dashboard ContentPage
- `src/BlackBoxBuddy/Views/RecordingsPage.axaml` - Recordings ContentPage
- `src/BlackBoxBuddy/Views/LiveFeedPage.axaml` - Live Feed ContentPage
- `src/BlackBoxBuddy/Views/SettingsPage.axaml` - Settings ContentPage
- `src/BlackBoxBuddy/ViewLocator.cs` - Added AppShellViewModel, Dashboard, Recordings, LiveFeed, Settings mappings
- `src/BlackBoxBuddy/AppServices.cs` - Registered INavigationService + 5 transient ViewModels
- `src/BlackBoxBuddy/App.axaml.cs` - Resolves AppShellViewModel from DI, sets AppShellView as root
- `src/BlackBoxBuddy/Views/MainWindow.axaml` - Removed embedded MainView (content set programmatically)

## Decisions Made

- Used `IValueConverter` (ConnectionStateToBrushConverter) for the connection state color mapping rather than multiple `IsVisible`-bound Ellipses — cleaner XAML, single binding
- `ContentPage.Icon` takes a `PathGeometry` object for the tab icon; `Header` (not `Title`) is the correct property for tab strip labels in Avalonia 12 RC1's `Page` base class
- `AdaptiveBehavior` and `AdaptiveClassSetter` live in `Avalonia.Xaml.Interactions.Responsive` namespace (from `Xaml.Behaviors.Interactions.Responsive` package), not `Avalonia.Xaml.Interactions.Core` as the plan template indicated
- NavigationService is a stub (returns Task.CompletedTask) — Plan 04 will resolve it via NavigationPage when provisioning flow is built

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed incorrect XAML namespace for AdaptiveBehavior**
- **Found during:** Task 2 (AppShellView XAML compilation)
- **Issue:** Plan template used `using:Avalonia.Xaml.Interactions.Core` for AdaptiveBehavior/AdaptiveClassSetter — this namespace doesn't exist in Avalonia 12 RC1
- **Fix:** Changed to `using:Avalonia.Xaml.Interactions.Responsive` (correct namespace from strings inspection of Xaml.Behaviors.Interactions.Responsive.dll)
- **Files modified:** `src/BlackBoxBuddy/Views/Shell/AppShellView.axaml`
- **Verification:** dotnet build exits 0
- **Committed in:** 4687858 (Task 2 commit)

**2. [Rule 1 - Bug] Fixed ContentPage.Title → ContentPage uses Header/Icon (not Title)**
- **Found during:** Task 2 (AppShellView XAML compilation)
- **Issue:** Plan template used `Title="Dashboard"` on ContentPage — but in Avalonia 12 RC1, `Page` base class uses `Header` for tab strip labels, `Title` does not exist as a XAML-settable property on ContentPage
- **Fix:** Removed `Title` attributes; set `Icon` with PathGeometry for icon-only tabs (plan goal: no text labels in tabs)
- **Files modified:** `src/BlackBoxBuddy/Views/Shell/AppShellView.axaml`
- **Verification:** dotnet build exits 0
- **Committed in:** 4687858 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (2 bugs in plan template XAML)
**Impact on plan:** Both fixes required for compilation. No scope creep. All plan goals achieved.

## Known Stubs

- `src/BlackBoxBuddy/Navigation/NavigationService.cs`: `PushAsync`, `PopAsync`, `PopToRootAsync` all return `Task.CompletedTask` — intentional stub. Plan 04 wires Avalonia 12 NavigationPage when provisioning wizard is built.
- `src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs`: `Title` property only — Phase 2 fills with real dashboard content
- `src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs`: `Title` property only — Phase 3 adds recordings list
- `src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs`: `Title` property only — Phase 3 adds live feed
- `src/BlackBoxBuddy/ViewModels/SettingsViewModel.cs`: `Title` property only — Phase 2 adds device settings

These stubs are intentional per plan design — Phase 1 establishes the shell skeleton; content phases fill them.

## Issues Encountered

None — build was clean after fixing the two XAML namespace/property errors in the plan templates.

## Next Phase Readiness

- App shell is complete and launchable on desktop
- Plan 02 (Mock Device) needs to register `IDeviceService` in AppServices before the app can fully initialize at runtime (currently AppShellViewModel constructor will fail with DI resolution error at startup)
- All placeholder pages ready for content from Plans 02-04
- NavigationService stub is wired; Plan 04 can replace with full NavigationPage implementation

---
*Phase: 01-foundation*
*Completed: 2026-03-24*
