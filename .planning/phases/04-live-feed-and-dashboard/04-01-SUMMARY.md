---
phase: 04-live-feed-and-dashboard
plan: 01
subsystem: ui
tags: [avalonia, libvlcsharp, mvvm, viewmodel, di, videoview, live-feed, dashboard]

requires:
  - phase: 03-recordings
    provides: RecordingsViewModel, IMediaPlayerService, ITripGroupingService, IDashcamDevice, TripGroup, Recording models

provides:
  - VideoView.cs Avalonia 12 RC1 compatible NativeControlHost for LibVLCSharp
  - LiveFeedViewModel with stream lifecycle (start, stop, toggle camera, retry, dispose)
  - DashboardViewModel with data aggregation (top-5 recordings, trips, events) and Action callbacks
  - AppShellViewModel rewired to construct DashboardViewModel manually with cross-tab wiring
  - DashboardViewModel removed from DI (same pattern as ManualConnectionViewModel)

affects:
  - 04-02 (LiveFeedPage XAML — binds to LiveFeedViewModel)
  - 04-03 (DashboardPage XAML — binds to DashboardViewModel)

tech-stack:
  added: []
  patterns:
    - DashboardViewModel constructed manually by AppShellViewModel with Action<int> switchTab, Action<EventType?> applyFilter, Action<Recording> openRecording, Action<TripGroup> openTrip callbacks
    - VideoView.cs inlined in Desktop project — replaces LibVLCSharp.Avalonia NuGet; uses TopLevel.GetTopLevel(this) for Avalonia 12 RC1 compatibility
    - LibVLCSharp.Avalonia NuGet removed from both BlackBoxBuddy.csproj and BlackBoxBuddy.Desktop.csproj

key-files:
  created:
    - src/BlackBoxBuddy.Desktop/Controls/VideoView.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/LiveFeedViewModelTests.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/DashboardViewModelTests.cs
  modified:
    - src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs
    - src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs
    - src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs
    - src/BlackBoxBuddy/AppServices.cs
    - src/BlackBoxBuddy/BlackBoxBuddy.csproj
    - src/BlackBoxBuddy.Desktop/BlackBoxBuddy.Desktop.csproj
    - tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs

key-decisions:
  - "VideoView.cs inlined in Desktop project to avoid LibVLCSharp.Avalonia NuGet constraint — LibVLCSharp.Avalonia uses VisualRoot which is removed in Avalonia 12 RC1; TopLevel.GetTopLevel(this) is the Avalonia 12 API"
  - "DashboardViewModel constructed manually in AppShellViewModel (not DI) — requires per-instance Action callbacks for tab switching and filter application that DI cannot supply"
  - "LibVLCSharp.Avalonia removed from BlackBoxBuddy.csproj (shared project) and BlackBoxBuddy.Desktop.csproj — inlined VideoView.cs replaces it; LibVLCSharp core kept"

patterns-established:
  - "Action callback pattern for cross-ViewModel navigation: DashboardViewModel receives switchTab/applyFilter/openRecording/openTrip Actions from AppShellViewModel at construction time"
  - "IDisposable pattern on LiveFeedViewModel: Dispose calls StopLiveFeed + DisposePlayer to clean up media resources when tab is left"

requirements-completed: [LIVE-01, LIVE-02, LIVE-03]

duration: ~10min
completed: 2026-03-25
---

# Phase 04 Plan 01: VideoView Port + LiveFeed/Dashboard ViewModels Summary

**Avalonia 12 RC1 compatible VideoView inlined, LibVLCSharp.Avalonia NuGet removed, LiveFeedViewModel with full stream lifecycle and DashboardViewModel with Action-callback cross-tab wiring implemented and tested**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-03-25T10:05:00Z
- **Completed:** 2026-03-25T10:10:30Z
- **Tasks:** 1 (all steps in single TDD task)
- **Files modified:** 10

## Accomplishments
- Ported VideoView.cs from LibVLCSharp 3.x to Avalonia 12 RC1 by replacing `VisualRoot` with `TopLevel.GetTopLevel(this)` throughout; inlined in `BlackBoxBuddy.Desktop.Controls`
- Removed `LibVLCSharp.Avalonia` NuGet from both the shared and Desktop project files — no namespace collision possible with Avalonia 12
- Implemented `LiveFeedViewModel` with camera selection (default "front"), stream start/stop/toggle/retry, `IDisposable` cleanup, and `IsLoading`/`IsConnectionLost`/`IsStreamActive` observable state
- Implemented `DashboardViewModel` with `LoadDashboardAsync` loading top-5 recordings, trips, and events (events filtered and ordered by DateTime desc), plus `SeeAll*` and `Open*` commands wired to `Action` callbacks
- Rewired `AppShellViewModel` to construct `DashboardViewModel` manually with cross-tab callbacks; removed it from DI registration
- Fixed `AppShellViewModelTests` to use updated constructor signatures (Rule 1 — auto-fix)
- Added 21 new unit tests: 10 for `LiveFeedViewModelTests`, 11 for `DashboardViewModelTests` — all passing

## Task Commits

1. **Task 1: Port VideoView, implement ViewModels, tests** - `5f2433a` (feat)

## Files Created/Modified
- `src/BlackBoxBuddy.Desktop/Controls/VideoView.cs` - Avalonia 12 RC1 compatible VideoView using `TopLevel.GetTopLevel(this)`
- `src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs` - Full stream lifecycle implementation with IDisposable
- `src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs` - Data aggregation ViewModel with Action callbacks
- `src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs` - Rewired to construct DashboardViewModel manually
- `src/BlackBoxBuddy/AppServices.cs` - Removed DashboardViewModel from DI
- `src/BlackBoxBuddy/BlackBoxBuddy.csproj` - Removed LibVLCSharp.Avalonia reference
- `src/BlackBoxBuddy.Desktop/BlackBoxBuddy.Desktop.csproj` - Removed LibVLCSharp.Avalonia reference
- `tests/BlackBoxBuddy.Tests/ViewModels/LiveFeedViewModelTests.cs` - 10 unit tests
- `tests/BlackBoxBuddy.Tests/ViewModels/DashboardViewModelTests.cs` - 11 unit tests
- `tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs` - Updated constructor calls

## Decisions Made
- VideoView.cs inlined to sidestep `LibVLCSharp.Avalonia` Avalonia version constraint; the 3.x package's `VideoView.cs` uses `VisualRoot` which is removed from Avalonia 12 RC1 — inlining with `TopLevel.GetTopLevel(this)` replaces that API
- DashboardViewModel constructed manually (not DI) because it requires per-instance lambda callbacks for cross-tab navigation; same established pattern as `ManualConnectionViewModel`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Updated AppShellViewModelTests constructor calls**
- **Found during:** Task 1 (after AppShellViewModel constructor signature changed)
- **Issue:** `AppShellViewModelTests` called `new DashboardViewModel()` (no-arg stub) and `new LiveFeedViewModel()` (no-arg stub), and passed `dashboardVm` to `AppShellViewModel` — all invalid after implementation
- **Fix:** Updated test constructor to mock `IDashcamDevice` and `ITripGroupingService`, pass them to `AppShellViewModel`, and construct `LiveFeedViewModel` with its three required parameters
- **Files modified:** `tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs`
- **Verification:** All AppShellViewModelTests pass
- **Committed in:** 5f2433a (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - bug in existing tests)
**Impact on plan:** Necessary correction — test file used stub constructors that no longer existed. No scope creep.

## Issues Encountered
- Pre-existing test failure in `ArchiveServiceTests.ArchiveTripAsync_ReportsProgressAsFractionOfCompletedClips` — not caused by this plan, not fixed (out of scope per deviation rules). 192/193 tests pass.

## Known Stubs
None — all ViewModels are fully implemented with real logic. No placeholder data flows to UI rendering.

## Next Phase Readiness
- `LiveFeedViewModel` and `DashboardViewModel` are fully implemented and tested
- `VideoView.cs` is Avalonia 12 RC1 compatible and ready for use in `LiveFeedPage.axaml`
- Wave 2 plans (04-02, 04-03) can focus purely on XAML page implementations that bind to these ViewModels
- No blockers for 04-02 or 04-03

## Self-Check: PASSED
- VideoView.cs: FOUND
- LiveFeedViewModel.cs: FOUND
- DashboardViewModel.cs: FOUND
- LiveFeedViewModelTests.cs: FOUND
- DashboardViewModelTests.cs: FOUND
- Commit 5f2433a: FOUND

---
*Phase: 04-live-feed-and-dashboard*
*Completed: 2026-03-25*
