---
phase: 04-live-feed-and-dashboard
plan: 02
subsystem: ui
tags: [avalonia, live-feed, xaml, viewmodel-binding, videoview, camera-toggle, code-behind]

requires:
  - phase: 04-live-feed-and-dashboard
    plan: 01
    provides: LiveFeedViewModel, VideoView.cs, DashboardViewModel

provides:
  - LiveFeedPage XAML with video area, loading/connection-loss state placeholders, and camera segmented toggle
  - LiveFeedPage code-behind with tab lifecycle (IsVisibleProperty.Changed.AddClassHandler), VideoView dynamic creation, and segment styling

affects:
  - 04-03 (DashboardPage XAML — parallel wave 2 plan)

tech-stack:
  added: []
  patterns:
    - IsVisibleProperty.Changed.AddClassHandler<T> for tab lifecycle instead of GetObservable().Subscribe() (requires System.Reactive which is not in project)
    - VideoView created via Type.GetType + Activator.CreateInstance reflection to avoid compile-time Desktop project dependency from shared project
    - Segment styling via code-behind PropertyChanged subscription (simpler than IMultiValueConverter for two-button case)

key-files:
  created: []
  modified:
    - src/BlackBoxBuddy/Views/LiveFeedPage.axaml
    - src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs
    - src/BlackBoxBuddy/Views/DashboardPage.axaml.cs

key-decisions:
  - "IsVisibleProperty.Changed.AddClassHandler<LiveFeedPage> used for tab lifecycle — GetObservable().Subscribe(Action<T>) requires System.Reactive which is not referenced; AddClassHandler is the pure Avalonia alternative"
  - "VideoView created via reflection (Type.GetType + Activator.CreateInstance) — avoids compile-time dependency on BlackBoxBuddy.Desktop from shared BlackBoxBuddy project"

requirements-completed: [LIVE-01, LIVE-02, LIVE-03]

duration: ~3min
completed: 2026-03-25
---

# Phase 04 Plan 02: LiveFeedPage XAML and Code-Behind Summary

**LiveFeedPage implemented with video area, loading/connection-loss state placeholders, camera segmented toggle (Front/Rear), tab lifecycle management via IsVisibleProperty.Changed.AddClassHandler, and Desktop VideoView wiring via reflection**

## Performance

- **Duration:** ~3 min
- **Started:** 2026-03-25T10:13:12Z
- **Completed:** 2026-03-25T10:16:21Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- Replaced stub `LiveFeedPage.axaml` with full layout: `Grid RowDefinitions="*,Auto"`, `#111111` video panel, `ContentControl VideoViewHost`, loading placeholder (ProgressBar + "Connecting to camera..."), connection-loss placeholder (PathIcon + "Connection lost" + "Retry Connection" button), and camera segmented toggle (Front/Rear in pill border `CornerRadius=22`, `Height=44`)
- All bindings: `IsStreamActive`, `IsLoading`, `IsConnectionLost`, `ToggleCameraCommand`, `RetryCommand` — all wired to `LiveFeedViewModel`
- Replaced stub `LiveFeedPage.axaml.cs` with full code-behind: tab lifecycle via `IsVisibleProperty.Changed.AddClassHandler`, `StartLiveFeedCommand`/`StopLiveFeedCommand` on tab appear/disappear, VideoView dynamic creation via reflection, `SetMediaPlayerOnVideoView` via reflection, segment styling (#2196F3 active, transparent inactive) updated on `SelectedCamera` changes
- Fixed `DashboardPage.axaml.cs` which had the same `GetObservable().Subscribe()` build error (Rule 1 auto-fix — the parallel 04-03 agent committed this fix)

## Task Commits

1. **Task 1: LiveFeedPage XAML layout** - `3251c91` (feat)
2. **Task 2: LiveFeedPage code-behind + DashboardPage fix** - `c4f9a7a` (feat, committed by parallel 04-03 agent)

## Files Created/Modified

- `src/BlackBoxBuddy/Views/LiveFeedPage.axaml` - Full layout with all UI states and camera toggle
- `src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs` - Tab lifecycle, VideoView wiring, segment styling
- `src/BlackBoxBuddy/Views/DashboardPage.axaml.cs` - Fixed GetObservable -> AddClassHandler (Rule 1)

## Decisions Made

- `IsVisibleProperty.Changed.AddClassHandler<T>` is the correct Avalonia-only pattern for observing property changes — `GetObservable().Subscribe(Action<T>)` requires `System.Reactive` which is not referenced in this project
- VideoView created via reflection to keep the shared project free of compile-time Desktop dependencies — same pattern established in Phase 3 for NativeControlHost

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] GetObservable().Subscribe() fails without System.Reactive**
- **Found during:** Task 2 (build verification)
- **Issue:** `GetObservable(IsVisibleProperty).Subscribe(Action<bool>)` requires `System.Reactive` for the `Subscribe(Action<T>)` extension method; Avalonia 12 RC1 does not include this transitively without ReactiveUI
- **Fix:** Replaced with `IsVisibleProperty.Changed.AddClassHandler<LiveFeedPage>((page, _) => ...)` — pure Avalonia approach, no extra packages needed. Applied same fix to `DashboardPage.axaml.cs` (pre-existing same bug)
- **Files modified:** `src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs`, `src/BlackBoxBuddy/Views/DashboardPage.axaml.cs`
- **Commit:** `c4f9a7a` (parallel 04-03 agent committed this fix)

---

**Total deviations:** 1 auto-fixed (Rule 1 - missing System.Reactive for Subscribe extension)
**Impact on plan:** Necessary correction — the Subscribe pattern works in ReactiveUI-based Avalonia setups but not in this project's stack. AddClassHandler achieves identical behavior.

## Known Stubs

None — LiveFeedPage is fully implemented. All three states (streaming, loading, connection-lost) are wired to real ViewModel properties. VideoView is created dynamically at runtime for Desktop platform.

## Self-Check: PASSED
- LiveFeedPage.axaml FOUND: `src/BlackBoxBuddy/Views/LiveFeedPage.axaml`
- LiveFeedPage.axaml.cs FOUND: `src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs`
- Commit 3251c91 FOUND (Task 1)
- Commit c4f9a7a FOUND (Task 2)
- dotnet build BlackBoxBuddy.csproj: 0 errors
- dotnet build BlackBoxBuddy.Desktop.csproj: 0 errors

---
*Phase: 04-live-feed-and-dashboard*
*Completed: 2026-03-25*
