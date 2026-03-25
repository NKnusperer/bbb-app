---
phase: 03-recordings
plan: 01
subsystem: models-and-data-layer
tags: [models, recording, mock, bogus, libvlcsharp, media-player]
dependency_graph:
  requires: []
  provides:
    - Recording record type with all 10 D-01 fields
    - EventType enum (None/Radar/GShock/Parking)
    - CameraChannel enum (Front/Rear/Both)
    - TripGroup record with computed aggregates
    - IDeviceFileSystem.ListRecordingsAsync returns IReadOnlyList<Recording>
    - IMediaPlayerService full playback API
    - MockDashcamDevice with 18 Bogus-generated recordings and embedded MP4
    - sample.mp4 EmbeddedResource for mock video streaming (D-09)
    - LibVLCSharp/VideoLAN.LibVLC.Windows NuGet packages
  affects:
    - src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs
    - src/BlackBoxBuddy/Device/IDeviceFileSystem.cs
    - src/BlackBoxBuddy/Services/IMediaPlayerService.cs
tech_stack:
  added:
    - LibVLCSharp 3.9.6
    - LibVLCSharp.Avalonia 3.9.6
    - VideoLAN.LibVLC.Windows 3.0.23
  patterns:
    - C# records for domain models with value equality
    - Bogus Faker with seed 42 for deterministic mock data
    - EmbeddedResource for bundled test assets
    - BGRA8888 byte arrays for thumbnail data
key_files:
  created:
    - src/BlackBoxBuddy/Models/Recording.cs
    - src/BlackBoxBuddy/Models/EventType.cs
    - src/BlackBoxBuddy/Models/CameraChannel.cs
    - src/BlackBoxBuddy/Models/TripGroup.cs
    - src/BlackBoxBuddy/Device/Mock/MockThumbnailGenerator.cs
    - src/BlackBoxBuddy/Assets/sample.mp4
    - tests/BlackBoxBuddy.Tests/Models/RecordingTests.cs
    - tests/BlackBoxBuddy.Tests/Device/MockDashcamDeviceRecordingsTests.cs
  modified:
    - src/BlackBoxBuddy/Device/IDeviceFileSystem.cs
    - src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs
    - src/BlackBoxBuddy/Services/IMediaPlayerService.cs
    - src/BlackBoxBuddy/BlackBoxBuddy.csproj
    - src/BlackBoxBuddy.Desktop/BlackBoxBuddy.Desktop.csproj
    - Directory.Packages.props
decisions:
  - "Recording uses C# record type for value equality — enables dirty-state comparison without custom comparers"
  - "TripGroup uses computed properties (not stored) — clips list is the source of truth; aggregates recalculate on demand"
  - "MockThumbnailGenerator uses EventType-coded BGRA8888 colors — visual debugging aid for event type without decoding video"
  - "Bogus seed 42 ensures deterministic 18-recording dataset — tests are repeatable and not flaky"
  - "sample.mp4 bundled as EmbeddedResource — no file system dependency; works in test environments without disk access"
  - "IMediaPlayerService uses object for player handle — concrete LibVLC MediaPlayer type lives in Desktop project; interface must be platform-agnostic"
  - "First 6 recordings spaced 20s apart — satisfies consecutive-within-30s test requirement for trip grouping tests"
metrics:
  duration: "~5 minutes"
  completed: "2026-03-25"
  tasks_completed: 2
  files_created: 8
  files_modified: 6
---

# Phase 03 Plan 01: Data Models and Mock Foundation Summary

**One-liner:** Recording/EventType/CameraChannel/TripGroup models with Bogus-generated MockDashcamDevice (18 recordings), BGRA8888 thumbnails, embedded sample.mp4 (D-09), and LibVLCSharp NuGet packages.

## Tasks Completed

| Task | Name | Commit | Key Files |
|------|------|--------|-----------|
| 1 | Create data models and refactor IDeviceFileSystem | 6e78835 | Recording.cs, EventType.cs, CameraChannel.cs, TripGroup.cs, IDeviceFileSystem.cs, IMediaPlayerService.cs, RecordingTests.cs |
| 2 | Expand MockDashcamDevice, bundle sample.mp4, add NuGet packages | 229f766 | MockDashcamDevice.cs, MockThumbnailGenerator.cs, sample.mp4, Directory.Packages.props, *.csproj, MockDashcamDeviceRecordingsTests.cs |

## What Was Built

### Data Models

- **Recording** — C# positional record with 10 fields: `FileName`, `DateTime`, `Duration`, `FileSize`, `AvgSpeed`, `PeakGForce`, `Distance`, `EventType`, `CameraChannel`, `ThumbnailData`
- **EventType** — enum: `None=0`, `Radar=1`, `GShock=2`, `Parking=3`
- **CameraChannel** — enum: `Front`, `Rear`, `Both`
- **TripGroup** — record wrapping `IReadOnlyList<Recording>` with computed `TotalDuration`, `TotalDistance`, `AvgSpeed` (weighted by duration), `PeakGForce`, `StartTime`, `EndTime`

### Refactored Interfaces

- `IDeviceFileSystem.ListRecordingsAsync` now returns `Task<IReadOnlyList<Recording>>` instead of `IReadOnlyList<string>`
- `IMediaPlayerService` expanded with full playback API: `CreatePlayer`, `Play`, `Pause`, `Stop`, `Seek`, `SetRate`, `NextFrame`, `PreviousFrame`, `DisposePlayer`

### Mock Data Layer

- `MockThumbnailGenerator` — generates 160x90 BGRA8888 solid-color byte arrays, color-coded by EventType (green=None, amber=Radar, red=GShock, orange=Parking)
- `MockDashcamDevice` — refactored with Bogus Faker (seed 42) generating 18 deterministic recordings:
  - 6 consecutive (20s apart) for trip grouping tests
  - 12 spread across 3 days with weighted event types (50% None, 20% Radar, 20% GShock, 10% Parking)
  - `DownloadFileAsync` streams the embedded `sample.mp4` resource

### Embedded Asset

- `sample.mp4` — 3-second 320x180 black H.264 video with silent AAC audio generated via ffmpeg; bundled as EmbeddedResource at logical name `BlackBoxBuddy.Assets.sample.mp4`

### NuGet Packages

- `Directory.Packages.props`: LibVLCSharp 3.9.6, LibVLCSharp.Avalonia 3.9.6, VideoLAN.LibVLC.Windows 3.0.23
- `BlackBoxBuddy.csproj`: Bogus (already in Directory.Packages.props, now referenced in shared project)
- `BlackBoxBuddy.Desktop.csproj`: LibVLCSharp, LibVLCSharp.Avalonia, VideoLAN.LibVLC.Windows (Windows-only condition)

## Test Results

- 105 total tests, 0 failed
- New tests: 8 RecordingTests + 7 MockDashcamDeviceRecordingsTests = 15 new tests
- All existing tests still pass (no regressions from interface refactor)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Consecutive timestamp spacing adjusted from 60s to 20s**
- **Found during:** Task 2 test run
- **Issue:** Plan spec said "timestamps 60 seconds apart" but test requirement said "within 30 seconds" — contradiction; 60s apart fails the ≤30s assertion
- **Fix:** Changed consecutive recording interval from 60s to 20s — still consecutive enough for trip grouping, satisfies the explicit test criteria
- **Files modified:** src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs
- **Commit:** 229f766

**2. [Rule 1 - Bug] FluentAssertions method name corrected**
- **Found during:** Task 2 test compile
- **Issue:** Used `BeLessOrEqualTo` (non-existent) instead of `BeLessThanOrEqualTo`
- **Fix:** Renamed to correct FluentAssertions API
- **Files modified:** tests/BlackBoxBuddy.Tests/Device/MockDashcamDeviceRecordingsTests.cs
- **Commit:** 229f766

## Known Stubs

None — all data is wired with real Bogus-generated values and the embedded MP4 asset.

## Self-Check: PASSED
