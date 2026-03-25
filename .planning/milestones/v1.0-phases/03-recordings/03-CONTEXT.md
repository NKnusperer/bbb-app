# Phase 3: Recordings - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Recording list with event-type filtering, recording detail view with dual-camera video playback and extended controls, automatic virtual trip grouping of consecutive clips, and archive to local storage with multi-select. This phase delivers the core user value: browse, review, and preserve dashcam footage.

</domain>

<decisions>
## Implementation Decisions

### Recording Data Model
- **D-01:** Replace `IDeviceFileSystem.ListRecordingsAsync` return type from `IReadOnlyList<string>` to `IReadOnlyList<Recording>` where `Recording` is a record type containing: FileName, DateTime, Duration, FileSize, AvgSpeed, PeakGForce, Distance, EventType, CameraChannel, ThumbnailData
- **D-02:** EventType is a simple enum: `None` (normal driving), `Radar`, `GShock`, `Parking` — no flags, single event per recording
- **D-03:** Mock device generates 15-20 recordings with realistic fake data using Bogus — mix of event types, spread across several days, with consecutive sequences for virtual trip testing
- **D-04:** Mock thumbnails are generated in-memory as small solid-color bitmap blocks (160x90), color-coded by event type — no embedded image assets needed

### Recording List Layout
- **D-05:** Card row layout — each recording is a horizontal card with thumbnail on the left, metadata stacked on the right (date/time, duration, file size on one line; speed, G-force, distance on another), event type shown as a colored badge/chip
- **D-06:** Event type filtering via horizontal toggle chip bar at top: All | Radar | G-Shock | Parking — single-select (radio behavior)
- **D-07:** Virtual trips shown inline with grouping — trip header card with aggregated stats, individual clips nested/indented below. Tapping trip header opens trip detail; tapping individual clip opens that clip's detail
- **D-08:** Fixed sort order: newest recordings first, no sort controls

### Video Playback
- **D-09:** Bundle a small (~2-5 sec) real MP4 file as an embedded resource for mock device playback — enables real LibVLCSharp integration testing
- **D-10:** Recording detail navigated via NavigationPage push with back button (standard mobile navigation pattern using Avalonia 12's NavigationPage.PushAsync)
- **D-11:** Dual player side-by-side layout when both front and rear recordings exist — side-by-side on desktop, stacked vertically on mobile
- **D-12:** Extended player controls: play/pause, seek bar, current time/total duration, fullscreen toggle, playback speed (0.5x, 1x, 2x), frame-by-frame step

### Virtual Trips
- **D-13:** Consecutive recordings grouped into virtual trips using a time gap threshold — recordings are consecutive if the next one starts within 30 seconds of the previous ending
- **D-14:** Trip detail uses the same UI as single recording detail (TRIP-02) with aggregated metadata (total duration, total distance, average speed, peak G-force across all clips)

### Archive
- **D-15:** Archive downloads to a default app directory (~/BlackBoxBuddy/Archives/ on desktop, app external storage on Android) with progress bar — no file picker by default
- **D-16:** Multi-select mode for archiving: long-press or "Select" button enters multi-select, user checks specific clips (especially around incidents), then taps "Archive Selected" — also has "Select All" for full trip archive
- **D-17:** Batch download for multi-clip archive — selected clips downloaded into a subfolder with overall progress bar
- **D-18:** Archived recordings visually marked in the list with a checkmark/"Archived" badge so users know which footage is safe from dashcam overwrite

### Claude's Discretion
- Exact card styling, spacing, and responsive breakpoints for recording cards
- LibVLCSharp VideoView integration details and platform-specific player setup
- Loading states for recording list and detail view
- Error handling for failed downloads or playback issues
- Exact frame-step implementation (LibVLC API specifics)
- Archive folder naming convention and file organization
- Progress bar styling and cancel-download support
- Mock data distribution (how many of each event type, date spread, trip sequences)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project context
- `.planning/PROJECT.md` — Project vision, constraints, key decisions
- `.planning/REQUIREMENTS.md` — Full v1 requirements (RECD-01..11, RDTL-01..08, TRIP-01..03, ARCH-01..03 for this phase)
- `.planning/ROADMAP.md` — Phase 3 goal, dependencies, success criteria

### Prior phase context
- `.planning/phases/01-foundation/01-CONTEXT.md` — Foundation decisions (navigation, device abstractions, DI patterns)
- `.planning/phases/02-settings/02-CONTEXT.md` — Settings decisions (DI refactor, testing approach)

### Device interface contracts
- `src/BlackBoxBuddy/Device/IDeviceFileSystem.cs` — File system interface to be refactored (ListRecordingsAsync must return Recording records)
- `src/BlackBoxBuddy/Device/IDashcamDevice.cs` — Composite device interface
- `src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs` — Mock implementation to be expanded with rich recording data
- `src/BlackBoxBuddy/Services/IMediaPlayerService.cs` — Empty stub to be implemented

### Existing recordings code
- `src/BlackBoxBuddy/ViewModels/RecordingsViewModel.cs` — Placeholder to be built out
- `src/BlackBoxBuddy/Views/RecordingsPage.axaml` — Placeholder to be built out

### Technology stack (from CLAUDE.md)
- LibVLCSharp.Avalonia 3.9.6 — Desktop video playback
- VideoLAN.LibVLC.Windows 3.0.23 — Windows native VLC binaries
- Bogus 35.6.5 — Fake data generation for mock recordings

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ViewModelBase` (ViewModels/ViewModelBase.cs): Base class for all ViewModels
- `IDeviceService` / `DeviceService`: Manages device connection state — RecordingsViewModel depends on this
- `IDialogService` / `DialogService` / `ConfirmDialog`: Reusable for archive confirmation dialogs
- `ConnectionStateToBrushConverter`: Pattern for creating value converters (EventType-to-color converter)
- `EnumBooleanConverter`: Pattern for enum-to-boolean bindings
- `INavigationService` / `NavigationService`: For pushing RecordingDetailPage onto navigation stack

### Established Patterns
- CommunityToolkit.Mvvm source generators (`[ObservableProperty]`, `[RelayCommand]`) for all properties and commands
- ContentPage-based navigation within TabbedPage
- Constructor-based DI — no `Ioc.Default` in Views or ViewModels
- C# record types for data models (used for settings records in Phase 2)
- FluentTheme Dark variant — all controls inherit dark styling

### Integration Points
- `IDeviceFileSystem.ListRecordingsAsync` — must be refactored from string list to Recording records
- `IMediaPlayerService` — must be implemented with LibVLCSharp for desktop
- `NavigationService.PushAsync` — for navigating to recording detail
- `ViewLocator` — must handle new RecordingDetailPage ↔ RecordingDetailViewModel mapping
- `AppServices.cs` — register new services (archive service, recording service)
- `MockDashcamDevice._recordings` — must be expanded from 3 strings to 15-20 rich Recording objects

</code_context>

<specifics>
## Specific Ideas

- Users primarily want to archive clips around incidents, not entire trips — multi-select mode is essential for picking the relevant consecutive clips
- Dual side-by-side video players for front+rear give users the full picture during incident review
- Extended controls (speed change, frame-step) are specifically valuable for reviewing collision/incident footage
- The inline trip grouping in the recording list means the list is a mixed collection of standalone recordings and trip groups — the ViewModel needs to handle both item types
- 30-second time gap threshold for trip grouping should work for most dashcams that record in 1-3 minute clips

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 03-recordings*
*Context gathered: 2026-03-24*
