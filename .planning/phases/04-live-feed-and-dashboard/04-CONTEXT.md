# Phase 4: Live Feed and Dashboard - Context

**Gathered:** 2026-03-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Live camera feed with front/rear toggle using the existing `IDeviceLiveStream` interface, and a dashboard summary screen showing recent recordings, trips, and events. This phase also resolves the LibVLCSharp.Avalonia incompatibility with Avalonia 12 by inlining a compatible VideoView control. This phase does NOT add new recording features, settings, or device discovery capabilities.

</domain>

<decisions>
## Implementation Decisions

### LibVLCSharp.Avalonia Compatibility Fix
- **D-01:** Remove the `LibVLCSharp.Avalonia` NuGet package — it is incompatible with Avalonia 12 RC1
- **D-02:** Inline the VideoView.cs source file from LibVLCSharp 3.x and make it compatible with Avalonia 12. Source: `https://code.videolan.org/videolan/LibVLCSharp/-/raw/3.x/src/LibVLCSharp.Avalonia/VideoView.cs?ref_type=heads`
- **D-03:** This fix unblocks desktop video playback for both live feed AND recording detail (Phase 3 VideoView integration)

### Live Feed Controls
- **D-04:** Minimal controls only — camera toggle (front/rear). No seek, speed, frame-step, or playback controls (not applicable to live streams)
- **D-05:** Front/rear camera toggle uses a segmented control below the video area. Matches the filter chip pattern from RecordingsPage
- **D-06:** No video overlay — keep the live stream clean and distraction-free
- **D-07:** Connection loss replaces the video area with a full-screen placeholder state ("Connection lost" with retry button). Segmented control remains visible to try the other camera

### Dashboard Layout
- **D-08:** Scrollable page with 3 distinct sections: Recent Recordings, Recent Trips, Recent Events
- **D-09:** Each section shows 3-5 items as compact cards (thumbnail + date/time + duration + event type badge)
- **D-10:** Section headers include a "See All" action link

### Dashboard Interaction
- **D-11:** Tapping a dashboard item navigates directly to RecordingDetailPage (same navigation as tapping in Recordings list)
- **D-12:** "See All" switches to the Recordings tab. For the Events section, auto-applies the relevant event type filter

### Claude's Discretion
- Exact segmented control styling and placement
- Compact card dimensions and responsive breakpoints for dashboard
- "See All" link styling and placement within section headers
- Connection loss placeholder design (icon, text, retry button style)
- How many items per dashboard section (3-5 range)
- Loading states for dashboard and live feed
- Error handling for stream connection failures
- VideoView.cs Avalonia 12 migration approach (API changes to address)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project context
- `.planning/PROJECT.md` — Project vision, constraints, key decisions
- `.planning/REQUIREMENTS.md` — Full v1 requirements (LIVE-01..03, DASH-01..03 for this phase)
- `.planning/ROADMAP.md` — Phase 4 goal, dependencies, success criteria

### Prior phase context
- `.planning/phases/01-foundation/01-CONTEXT.md` — Foundation decisions (navigation, device abstractions, DI patterns)
- `.planning/phases/02-settings/02-CONTEXT.md` — Settings decisions (DI refactor, testing approach)
- `.planning/phases/03-recordings/03-CONTEXT.md` — Recordings decisions (playback, detail view, archive patterns)

### LibVLCSharp VideoView source (critical)
- `https://code.videolan.org/videolan/LibVLCSharp/-/raw/3.x/src/LibVLCSharp.Avalonia/VideoView.cs?ref_type=heads` — VideoView.cs to inline and port to Avalonia 12

### Device interface contracts
- `src/BlackBoxBuddy/Device/IDeviceLiveStream.cs` — Live stream interface (`GetStreamUriAsync(cameraId)`)
- `src/BlackBoxBuddy/Device/IDashcamDevice.cs` — Composite device interface
- `src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs` — Mock implementation (returns mock RTSP URIs)

### Existing playback infrastructure
- `src/BlackBoxBuddy/Services/IMediaPlayerService.cs` — Media player abstraction
- `src/BlackBoxBuddy.Desktop/Services/DesktopMediaPlayerService.cs` — LibVLCSharp desktop implementation
- `src/BlackBoxBuddy/ViewModels/RecordingDetailViewModel.cs` — Player lifecycle pattern to reuse

### Existing skeleton pages
- `src/BlackBoxBuddy/Views/DashboardPage.axaml` — Skeleton page to be built out
- `src/BlackBoxBuddy/Views/LiveFeedPage.axaml` — Skeleton page to be built out
- `src/BlackBoxBuddy/ViewModels/DashboardViewModel.cs` — Stub ViewModel to be built out
- `src/BlackBoxBuddy/ViewModels/LiveFeedViewModel.cs` — Stub ViewModel to be built out

### Navigation and shell
- `src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs` — Tab navigation, injects all 4 VMs
- `src/BlackBoxBuddy/Views/Shell/AppShellView.axaml` — TabbedPage with Dashboard (tab 0), Live Feed (tab 2)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `IMediaPlayerService` + `DesktopMediaPlayerService`: Full LibVLCSharp playback service — reuse for live stream playback
- `RecordingDetailViewModel`: Player lifecycle pattern (CreatePlayer, Play, Stop, DisposePlayer) — adapt for live stream
- `RecordingsViewModel`: Recording list with filtering and trip grouping — dashboard can query this data
- `IDeviceService` / `DeviceService`: Connection state management — live feed depends on connected state
- `INavigationService` / `NavigationService`: For navigating from dashboard items to RecordingDetailPage
- `ConnectionStateToBrushConverter`: Pattern for value converters
- `TripGroupingService`: Groups recordings into trips — dashboard needs this for Recent Trips section

### Established Patterns
- CommunityToolkit.Mvvm source generators (`[ObservableProperty]`, `[RelayCommand]`)
- Constructor-based DI — no `Ioc.Default` in Views or ViewModels
- C# record types for data models
- ContentPage-based navigation within TabbedPage
- FluentTheme Dark variant
- `ViewLocator` pattern-matching switch for VM-to-View mapping

### Integration Points
- `AppShellViewModel` already injects `DashboardViewModel` and `LiveFeedViewModel` as constructor params
- `AppServices.cs` already registers both VMs as transient
- `ViewLocator.cs` already maps both VMs to their pages
- `MockDashcamDevice.GetStreamUriAsync` returns `rtsp://192.168.1.254/live/{cameraId}` URIs
- `IDeviceFileSystem.ListRecordingsAsync` provides recording data for dashboard sections
- Tab indices: Dashboard=0, Recordings=1, LiveFeed=2, Settings=3

</code_context>

<specifics>
## Specific Ideas

- LibVLCSharp.Avalonia NuGet package is incompatible with Avalonia 12 RC1 — must inline VideoView.cs and port it. This is a prerequisite for both live feed AND fixing recording detail video playback
- Dashboard "See All" for Events should auto-apply the event type filter when switching to the Recordings tab — requires cross-tab communication via AppShellViewModel
- Live feed is conceptually simpler than recording playback — no seek, no speed control, no frame stepping. Just stream + camera toggle + connection state handling
- The mock device returns fake RTSP URIs — live feed can be developed and tested structurally even without a real stream source

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 04-live-feed-and-dashboard*
*Context gathered: 2026-03-25*
