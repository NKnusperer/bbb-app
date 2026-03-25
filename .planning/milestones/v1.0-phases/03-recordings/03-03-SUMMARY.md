---
phase: 03-recordings
plan: "03"
subsystem: recordings-ui
tags: [viewmodel, xaml, recordings, filtering, trip-groups, tdd]
dependency_graph:
  requires: ["03-01", "03-02"]
  provides: ["RecordingsViewModel", "RecordingsPage", "RecordingDetailViewModel stub"]
  affects: ["03-04"]
tech_stack:
  added: ["FileSizeConverter", "EventTypeToStringConverter", "EventTypeToVisibilityConverter"]
  patterns: ["ObservableCollection<object> for heterogeneous lists", "DataTemplate type-dispatch in ListBox", "TDD with NSubstitute for ViewModel"]
key_files:
  created:
    - src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs
    - src/BlackBoxBuddy/ViewModels/RecordingDetailViewModel.cs
    - src/BlackBoxBuddy/Converters/RecordingConverters.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/RecordingsViewModelTests.cs
  modified:
    - src/BlackBoxBuddy/Views/RecordingsPage.axaml
    - src/BlackBoxBuddy/Views/RecordingsPage.axaml.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs
decisions:
  - "RecordingsViewModel accepts IDeviceService (for connection state) separately from IDashcamDevice (for recording list) — cleaner separation since IDeviceService owns ConnectionStateChanged event"
  - "ObservableCollection<object> for DisplayItems to support heterogeneous Recording/TripGroup items with ListBox DataTemplate type dispatch"
  - "SetFilterCommand uses EventType? parameter — null means 'All', non-null means specific filter; ApplyFilter called on any change"
  - "RecordingDetailViewModel created as minimal stub with Recording/TripGroup constructor overloads; full implementation in Plan 04"
  - "Filter chip buttons use fixed Background colors (not data-bound) for simplicity; active state visual will be enhanced in future iteration"
metrics:
  duration: "~7 minutes"
  completed: "2026-03-25T07:43:40Z"
  tasks_completed: 2
  files_created: 4
  files_modified: 3
---

# Phase 03 Plan 03: RecordingsViewModel and RecordingsPage Summary

RecordingsViewModel with filter/grouping/loading/navigation wired to a full RecordingsPage XAML with DataTemplates for Recording cards and TripGroup headers with nested clips.

## Tasks Completed

| Task | Description | Commit | Files |
|------|-------------|--------|-------|
| 1 | RecordingsViewModel with TDD (13 tests) | 403b764 | RecordingsViewModel.cs, RecordingDetailViewModel.cs, RecordingsViewModelTests.cs, AppShellViewModelTests.cs |
| 2 | RecordingsPage XAML with filter chips, cards, trip groups | 3327d91 | RecordingsPage.axaml, RecordingsPage.axaml.cs, RecordingConverters.cs |

## What Was Built

**RecordingsViewModel** (`src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs`):
- Constructor injection: `IDashcamDevice`, `IDeviceService`, `ITripGroupingService`, `IArchiveService`, `INavigationService`
- Observable properties: `DisplayItems` (ObservableCollection<object>), `SelectedFilter` (EventType?), `IsLoading`, `IsEmpty`, `IsDeviceConnected`, `HasActiveFilter`, `ErrorMessage`
- `LoadRecordingsCommand` (async): calls `_device.ListRecordingsAsync()`, calls `ApplyFilter()`, sets `IsLoading` and `ErrorMessage`
- `SetFilterCommand`: sets `SelectedFilter`, calls `ApplyFilter()`
- `OpenRecordingCommand` / `OpenTripCommand`: create `RecordingDetailViewModel`, call `_navigationService.PushAsync()`
- `ApplyFilter()`: filters `_allRecordings` by `SelectedFilter`, passes to `_tripGroupingService.Group()`, rebuilds `DisplayItems`, updates `IsEmpty` and `HasActiveFilter`
- `OnConnectionStateChanged`: auto-reloads when device connects

**RecordingDetailViewModel stub** (`src/BlackBoxBuddy/ViewModels/RecordingDetailViewModel.cs`):
- Minimal stub with `Recording?` and `TripGroup?` constructors; full implementation in Plan 04

**RecordingsPage.axaml** (`src/BlackBoxBuddy/Views/RecordingsPage.axaml`):
- Filter chip bar with All/Radar/G-Shock/Parking buttons wired to `SetFilterCommand`
- Loading state: indeterminate `ProgressBar` + "Loading recordings..."
- Empty state: two variants — no device connected ("Your dashcam recordings will appear here") and active filter with no matches ("No matches for this filter")
- `ListBox` with `ItemsSource="{Binding DisplayItems}"` and type-dispatching `DataTemplates`
- Recording card: 160x90 thumbnail, event badge (hidden for None), date/time, duration, file size (MB), speed (km/h), G-force, distance
- TripGroup card: trip header with aggregate stats + nested clips at 24px indent

**RecordingConverters.cs** (`src/BlackBoxBuddy/Converters/RecordingConverters.cs`):
- `FileSizeConverter`: `long` bytes → "X.X MB"
- `EventTypeToStringConverter`: EventType → "Radar" / "G-Shock" / "Parking" / ""
- `EventTypeToVisibilityConverter`: EventType → bool (false for None, for badge visibility)
- `NullFilterActiveBrushConverter`: filter chip active state helper

## Test Results

13 tests added in `RecordingsViewModelTests.cs`, all passing:
- `LoadRecordingsAsync_PopulatesDisplayItemsViaGroupingService`
- `LoadRecordingsAsync_SetsIsLoadingDuringExecution`
- `LoadRecordingsAsync_SetsErrorMessageOnFailure`
- `WhenSelectedFilterIsNull_AllRecordingsPassedToGroupingService`
- `WhenSelectedFilterIsRadar_OnlyRadarRecordingsPassedToGroupingService`
- `WhenSelectedFilterChanges_DisplayItemsIsRebuilt`
- `IsEmpty_TrueWhenDisplayItemsIsEmpty`
- `IsEmpty_FalseWhenDisplayItemsHasItems`
- `HasActiveFilter_FalseInitially`
- `HasActiveFilter_TrueWhenFilterIsSet`
- `HasActiveFilter_FalseAfterClearingFilter`
- `OpenRecordingCommand_PushesRecordingDetailViewModel`
- `OpenTripCommand_PushesRecordingDetailViewModelInTripMode`
- `IsDeviceConnected_FalseWhenDeviceServiceReportsDisconnected`
- `IsDeviceConnected_TrueWhenDeviceServiceReportsConnected`

Total test suite: 136/137 passing (1 pre-existing `ArchiveServiceTests` failure, unrelated to this plan).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed AppShellViewModelTests compilation after RecordingsViewModel constructor change**
- **Found during:** Task 1 GREEN phase
- **Issue:** `AppShellViewModelTests.cs` constructed `new RecordingsViewModel()` (parameterless) but the new implementation requires 5 constructor arguments
- **Fix:** Updated test setup to construct `RecordingsViewModel` with NSubstitute mocks for all 5 dependencies
- **Files modified:** `tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs`
- **Commit:** 403b764

## Known Stubs

| Stub | File | Reason |
|------|------|--------|
| `RecordingDetailViewModel` | `src/BlackBoxBuddy/ViewModels/RecordingDetailViewModel.cs` | Minimal stub with Recording/TripGroup constructors only; full video player + metadata implementation in Plan 04 |
| Filter chip active state | `RecordingsPage.axaml` | Filter buttons use fixed background colors; dynamic active/inactive visual state will be enhanced when XAML DataTriggers approach is clarified for Avalonia 12 RC1 |

## Self-Check: PASSED

All files verified to exist. Both task commits found in git log:
- 403b764: feat(03-03): implement RecordingsViewModel with filtering, trip grouping, and navigation
- 3327d91: feat(03-03): build RecordingsPage XAML with filter chips, heterogeneous card list, trip groups
