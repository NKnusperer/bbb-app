---
phase: 03-recordings
plan: 04
subsystem: recording-detail-playback
tags: [viewmodel, xaml, libvlcsharp, video-playback, archive, cancel, tdd]
dependency_graph:
  requires:
    - 03-01 (Recording/TripGroup/CameraChannel models, IMediaPlayerService interface)
    - 03-02 (IArchiveService, INavigationService)
    - 03-03 (RecordingDetailViewModel stub, RecordingsViewModel)
  provides:
    - RecordingDetailViewModel full implementation with all playback controls
    - CancelArchiveCommand with CancellationTokenSource
    - RecordingDetailPage with VideoView, thumbnail overlay, player controls, metadata
    - DesktopMediaPlayerService (LibVLCSharp) registered in Desktop DI
    - ViewLocator RecordingDetailViewModel mapping
  affects:
    - 03-05 (UI styling and navigation wiring)
tech_stack:
  added:
    - LibVLCSharp.Avalonia added to shared BlackBoxBuddy.csproj for VideoView XAML resolution
  patterns:
    - TDD RED/GREEN for RecordingDetailViewModel
    - object? player handle pattern for platform-agnostic MediaPlayer exposure
    - CancellationTokenSource _archiveCts for cancellable archive operations
    - Panel overlay pattern for thumbnail before playback
    - Code-behind lifecycle: Loaded->InitializePlayback, Unloaded->Dispose
key_files:
  created:
    - src/BlackBoxBuddy/ViewModels/RecordingDetailViewModel.cs
    - src/BlackBoxBuddy.Desktop/Services/DesktopMediaPlayerService.cs
    - src/BlackBoxBuddy/Views/RecordingDetailPage.axaml
    - src/BlackBoxBuddy/Views/RecordingDetailPage.axaml.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/RecordingDetailViewModelTests.cs
  modified:
    - src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs (adds IMediaPlayerService optional param)
    - src/BlackBoxBuddy.Desktop/Program.cs (registers DesktopMediaPlayerService)
    - src/BlackBoxBuddy/ViewLocator.cs (adds RecordingDetailViewModel -> RecordingDetailPage)
    - src/BlackBoxBuddy/BlackBoxBuddy.csproj (adds LibVLCSharp.Avalonia reference)
    - tests/BlackBoxBuddy.Tests/ViewModels/RecordingsViewModelTests.cs (adds IMediaPlayerService mock)
decisions:
  - "DesktopMediaPlayerService placed in Desktop project (not shared) because LibVLCSharp is a Desktop-only dependency — shared project only gets LibVLCSharp.Avalonia for VideoView XAML resolution"
  - "RecordingsViewModel takes IMediaPlayerService as optional parameter (default null) — forwards to RecordingDetailViewModel at navigation time; no AppServices lookup"
  - "VideoView and thumbnail in Panel overlay pattern: thumbnail shown when !IsPlaying, VideoView when IsPlaying"
  - "InitializePlayback() is a no-op public method called from code-behind Loaded — players created in constructor, playback starts on user press, surface ready when called"
  - "LibVLCSharp.Avalonia added to shared project so VideoView XAML compiles; DesktopMediaPlayerService stays in Desktop project"
metrics:
  duration: "~8 minutes"
  completed: "2026-03-25"
  tasks_completed: 2
  files_created: 5
  files_modified: 5
---

# Phase 03 Plan 04: RecordingDetailPage and Video Playback Summary

**One-liner:** RecordingDetailViewModel with PlayPause/Seek/Rate/CancelArchive commands, RecordingDetailPage with LibVLCSharp VideoView and thumbnail overlay (RDTL-02), dual-camera layout, and DesktopMediaPlayerService registered in Desktop DI.

## Tasks Completed

| Task | Name | Commit | Key Files |
|------|------|--------|-----------|
| 1 | Full RecordingDetailViewModel with TDD | 725edf5 | RecordingDetailViewModel.cs, RecordingDetailViewModelTests.cs, RecordingsViewModel.cs |
| 2 | RecordingDetailPage XAML and DesktopMediaPlayerService | 8270a0c | RecordingDetailPage.axaml, RecordingDetailPage.axaml.cs, DesktopMediaPlayerService.cs, Program.cs, ViewLocator.cs |

## What Was Built

### RecordingDetailViewModel

Full MVVM ViewModel replacing the Plan 03 stub:
- **Constructor overloads**: `(Recording, ...)` for single recording mode, `(TripGroup, ...)` for trip mode
- **Observable properties**: `IsTrip`, `IsDualCamera`, `IsPlaying`, `PlaybackRate` (default 1.0f), `SeekPosition`, `CurrentTimeMs/TotalDurationMs`, `IsFullscreen`, `IsArchived`, `ArchiveProgress`, `IsArchiving`
- **Computed metadata**: `DisplayDateTime`, `DisplayDuration`, `DisplayFileSize`, `DisplayAvgSpeed`, `DisplayPeakGForce`, `DisplayDistance`, `PageTitle`, `CurrentClips`, `ClipCount`
- **Player exposure**: `FrontPlayer` and `RearPlayer` as `object?` for VideoView binding
- **Commands**: `PlayPauseCommand`, `NextFrameCommand`, `PreviousFrameCommand`, `SetRateCommand`, `SeekToCommand`, `ToggleFullscreenCommand`, `ArchiveRecordingCommand`, `CancelArchiveCommand`, `GoBackCommand`
- **CancelArchiveCommand**: cancels `_archiveCts` CancellationTokenSource to abort in-progress archive
- **IDisposable**: disposes CTS and media players on page unload

### DesktopMediaPlayerService

LibVLCSharp implementation at `src/BlackBoxBuddy.Desktop/Services/DesktopMediaPlayerService.cs`:
- `Core.Initialize()` in constructor (required LibVLC initialization)
- `CreatePlayer()` returns `new MediaPlayer(_libVlc)`
- `PreviousFrame()` implemented as seek-back 33ms (LibVLC has no PreviousFrame API)
- Registered in Desktop Program.cs: `services.AddSingleton<IMediaPlayerService, DesktopMediaPlayerService>()`

### RecordingDetailPage XAML

- **Structure**: Grid with Row 0 = video area (outside ScrollViewer), Row 1 = player controls + metadata (inside ScrollViewer)
- **Thumbnail overlay (RDTL-02)**: Panel with VideoView (IsVisible=IsPlaying) and Image (IsVisible=!IsPlaying) + play button overlay
- **Dual-camera**: Side-by-side Grid with Front/Rear panels when IsDualCamera
- **Player controls**: seek bar with time labels, 44px play/pause/prev-next/fullscreen buttons, speed buttons 0.5x/1x/2x
- **Archive overlay**: progress bar + "Stop Archiving" button wired to CancelArchiveCommand, visible when IsArchiving
- **Metadata grid**: 6 rows — date/time, duration, file size, avg speed, peak G-force, distance
- **Trip clips section**: ItemsControl visible when IsTrip showing CurrentClips
- **Archive buttons**: separate buttons for single recording ("Archive Recording") and trip ("Archive Entire Trip")
- Code-behind: Loaded->InitializePlayback, Unloaded->Dispose, SeekSlider PointerReleased->SeekToCommand

### ViewLocator + AppServices

- ViewLocator: `RecordingDetailViewModel => new Views.RecordingDetailPage()`
- RecordingsViewModel: optional `IMediaPlayerService?` parameter forwarded to RecordingDetailViewModel on navigation

## Test Results

22 new tests in RecordingDetailViewModelTests.cs covering:
- Single recording and trip constructor behavior
- Metadata formatting (DisplayDateTime, DisplayDuration, DisplayFileSize, DisplayAvgSpeed, DisplayPeakGForce, DisplayDistance)
- IsDualCamera for CameraChannel.Both
- PlayPauseCommand toggling
- SetRateCommand updating PlaybackRate
- NextFrame/PreviousFrame calling media service
- SeekToCommand calling media service Seek
- ArchiveRecordingCommand (single recording and trip)
- CancelArchiveCommand cancelling ongoing archive
- Dispose calling DisposePlayer

Total tests: 158 (157 passing, 1 pre-existing ArchiveServiceTests failure unrelated to this plan)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] DesktopMediaPlayerService moved to Desktop project**
- **Found during:** Task 2 implementation
- **Issue:** Plan specified `src/BlackBoxBuddy/Services/DesktopMediaPlayerService.cs` (shared project), but `LibVLCSharp` package is only referenced in the Desktop project — the class uses `LibVLCSharp.Shared` types
- **Fix:** Placed service at `src/BlackBoxBuddy.Desktop/Services/DesktopMediaPlayerService.cs`; added `LibVLCSharp.Avalonia` to shared project so VideoView XAML compiles
- **Files modified:** BlackBoxBuddy.Desktop.csproj (implicitly), BlackBoxBuddy.csproj (added LibVLCSharp.Avalonia)
- **Commit:** 8270a0c

**2. [Rule 1 - Bug] RecordingsViewModelTests updated after IMediaPlayerService added to RecordingsViewModel**
- **Found during:** Task 1 GREEN phase
- **Issue:** Existing OpenRecordingCommand/OpenTripCommand tests failed because _mediaPlayerService was null causing early return
- **Fix:** Added IMediaPlayerService mock to RecordingsViewModelTests constructor
- **Files modified:** tests/BlackBoxBuddy.Tests/ViewModels/RecordingsViewModelTests.cs
- **Commit:** 725edf5

## Known Stubs

None — all properties are computed from real Recording/TripGroup data. Archive button shows real conditional text. VideoView binding to FrontPlayer/RearPlayer is live (players created in constructor). CancelArchiveCommand is fully wired.

## Self-Check: PASSED
