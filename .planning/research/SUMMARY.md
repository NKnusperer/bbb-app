# Project Research Summary

**Project:** BlackBoxBuddy
**Domain:** Cross-platform dashcam management app (desktop + Android)
**Researched:** 2026-03-24
**Confidence:** MEDIUM — core stack and architecture HIGH confidence; GPS parsing and Android WiFi behavior MEDIUM-LOW

## Executive Summary

BlackBoxBuddy is a local, single-device dashcam companion app built with Avalonia 12 RC1, .NET 10, and C#. The recommended approach is a four-layer MVVM architecture: Avalonia views, CommunityToolkit.Mvvm ViewModels, platform-agnostic services, and a vendor-abstraction device layer. The entire device layer should be coded against interfaces from day one, with a mock device driving all development until real hardware is in hand. This is not a controversial choice — it is the only approach that keeps the app testable and avoids the need to refactor when real device protocols land.

The primary differentiator over all existing dashcam apps (BlackVue, Thinkware, VIOFO, Dashcam Viewer) is virtual trip grouping: treating consecutive short clips as a single drive session with aggregated stats. No competitor does this. Every architectural and feature decision in this project should protect and enable this differentiator. GPS/telemetry parsing is the enabling technology for trip metadata (distance, avg speed, peak G-force), but it is vendor-proprietary, reverse-engineered, and must be deferred until a real device is available — the mock device makes v1 viable without it.

The two non-negotiable risks to mitigate from the start are: (1) video playback has no single cross-platform library — LibVLCSharp works on desktop, ExoPlayer on Android, and the abstraction layer must be in place before any playback UI is built; and (2) Avalonia 12 RC1 is pre-stable — exact package versions must be pinned immediately and navigation primitives must be isolated behind an `INavigationService` abstraction to survive the RC-to-stable transition. Ignoring either of these will cost a sprint to recover.

## Key Findings

### Recommended Stack

The core stack is already locked in: C# 14 / .NET 10, Avalonia 12 RC1, CommunityToolkit.Mvvm 8.4.1, xunit.v3, and Avalonia.Headless.XUnit. The additions required are: MetadataExtractor 2.9.2 for standard video metadata, Xaml.Behaviors.Avalonia 12.0.0-rc1 for responsive layout, LibVLCSharp 3.9.6 + LibVLCSharp.Avalonia for desktop video, Xamarin.AndroidX.Media3.ExoPlayer 1.9.2.1 for Android video, and NSubstitute 5.3.0 / FluentAssertions 8.9.0 / Bogus 35.6.5 for testing. ReactiveUI must be explicitly avoided — CommunityToolkit.Mvvm already fills that role, and ReactiveUI adds complexity without benefit.

**Core technologies:**
- LibVLCSharp 3.9.6 (desktop) + ExoPlayer 1.9.2.1 (Android): video playback — no single library covers both; abstraction mandatory
- MetadataExtractor 2.9.2: standard MP4 metadata (duration, timestamps, codec) — GPS parsing requires custom vendor-specific `BinaryReader` parsers
- Xaml.Behaviors.Avalonia 12.0.0-rc1: responsive layout via `AdaptiveBehavior` — the older `Avalonia.Xaml.Behaviors` package is deprecated
- NSubstitute 5.3.0: mocking — preferred over Moq (SponsorLink controversy; cleaner syntax)
- Microsoft.Extensions.DependencyInjection: DI container — bundled with .NET 10, integrates with `CommunityToolkit.Mvvm`'s `Ioc.Default`

### Expected Features

**Must have (table stakes):**
- Device auto-discovery via WiFi + manual fallback — every competitor has this; without it, nothing works
- Connection status indicator (three states: Searching / Connected / Disconnected) — users need actionable UI state at all times
- Live video preview with front/rear toggle — validates WiFi connectivity; expected by all users
- Recording list with event-type filtering (G-shock, radar, parking, normal) — primary reason users open the app
- Thumbnails + recording metadata (date, time, duration, size) — minimum for footage identification
- Recording detail with video playback (play, pause, scrub) — non-negotiable
- Archive / download recordings to local storage — the core job-to-be-done; dashcam overwrites within hours
- Settings configuration (resolution, modes, WiFi, shock/radar sensitivity, overlays) — users expect app control
- SD card usage indicator with overwrite urgency signal — affects user's archiving decisions
- Dark-mode responsive layout (portrait + landscape, desktop + Android) — per project constraints
- Mock / demo device — required for development; ships as user-facing demo mode

**Should have (differentiators):**
- Virtual trips (consecutive clip grouping with configurable adjacency threshold) — core differentiator; no competitor does this
- Provisioning wizard for new device setup — no competitor offers guided first-run
- Trip metadata: avg speed, peak G-force, distance (requires GPS parsing infrastructure; v1.x)
- GPS trace overlay in recording detail view (requires GPS parsing; v1.x)
- Radar sensor sensitivity configuration — power users; per PROJECT.md requirements

**Defer (v2+):**
- Real device communication (entire device layer is currently mocked; v1 validates UX)
- Multi-vendor support (architecture ready; actual integrations deferred)
- iOS support
- Multi-device session management
- Firmware OTA updates, settings import/export, light mode

### Architecture Approach

The recommended structure is a four-layer architecture living in a shared `BlackBoxBuddy` class library (UI + core), with thin `BlackBoxBuddy.Desktop` and `BlackBoxBuddy.Android` head projects providing only entry points, platform service registrations, and platform-specific service implementations. The device abstraction layer (`Device/`) uses interface segregation — `IDashcamDevice` composes five narrow interfaces (`IDeviceDiscovery`, `IDeviceConnection`, `IDeviceCommands`, `IDeviceFileSystem`, `IDeviceLiveStream`) — so adding a real vendor requires a new implementation folder without touching services or ViewModels. ViewModels depend only on injected service interfaces and must contain zero Avalonia or platform imports.

**Major components:**
1. Navigation Shell (`AppShellView` + `AppShellViewModel`) — adaptive layout host; portrait/landscape breakpoint at 600px; connection indicator; drawer nav on mobile, sidebar on desktop
2. Device Abstraction Layer (`IDashcamDevice` + narrow interfaces + `MockDashcamDevice`) — all device communication; the single extension point for future vendors
3. Services (`DeviceService`, `RecordingService`, `IMediaPlayerService`) — all business logic; platform-agnostic; fully testable against mock device
4. Feature Pages (Dashboard, Recordings, LiveFeed, Settings, Provisioning) — one ContentPage + ViewModel per screen; built after services exist
5. Platform video services (`VlcMediaPlayerService` on desktop, `ExoPlayerMediaService` on Android) — injected via DI; shared code never references a concrete player

### Critical Pitfalls

1. **ObservableCollection mutations from background threads** — Avalonia does not throw immediately; WeakEvent race conditions appear intermittently under real load. Prevention: enforce `Dispatcher.UIThread.Post()` for all collection mutations from day one; swap entire collection reference on UI thread after off-thread build.

2. **No single cross-platform video library** — LibVLCSharp does not work on Android. Prevention: define `IMediaPlayerService` in shared project before building any playback UI; register platform implementations via DI; never import LibVLC from shared project.

3. **Avalonia 12 RC1 API instability** — Navigation APIs (`ContentPage`, `NavigationPage`, `DrawerPage`) may break between RC1 and stable. Prevention: pin exact NuGet versions in `Directory.Build.props`; isolate navigation behind `INavigationService`; budget upgrade task in later phases.

4. **Android "no internet on dashcam WiFi" silent failure** — Android Smart Network Switch silently redirects traffic to mobile data when the dashcam AP has no internet. Prevention: bind `HttpClient` socket to WiFi interface via `ConnectivityManager.bindProcessToNetwork()`; test on real Android device with mobile data active.

5. **Virtual trip grouping breaks on timestamp discontinuities** — naive gap threshold either merges separate drives or splits one drive into dozens of 1-clip trips. Prevention: configurable threshold (default 30s); sequence by filename (not file-modified-time); unit test with 0s, 5s, 15s, 45s, 300s gap scenarios.

6. **Thumbnail generation blocks UI or leaks memory** — decoding 1080p H.264 frames for 200+ clips causes OOM on Android. Prevention: lazy virtualized generation (only on scroll into view); concurrency cap via `SemaphoreSlim(2)`; disk cache; immediate `Bitmap.Dispose()` after scaling.

7. **GPS/telemetry format is vendor-proprietary** — no .NET library covers all formats; DDDMM.MMMM vs decimal degrees misparse produces wrong hemisphere. Prevention: `ITelemetryParser` interface with null/stub for mock; unit test against real byte-level fixture files per vendor; log and swallow per-frame parse errors.

## Implications for Roadmap

Based on research, the build order is driven by component dependencies: device contracts must exist before services, services before ViewModels, navigation shell before feature pages. All phases should use the mock device so no phase is blocked on hardware.

### Phase 1: Project Foundation and Device Contracts
**Rationale:** Every other layer depends on these. Architecture research is explicit: "Define device contracts first — IDashcamDevice and sub-interfaces are depended on by every layer." Avalonia RC1 instability means the navigation shell must be pinned and abstracted before any feature screens are built on top of it.
**Delivers:** Pinned NuGet versions, DI wiring, `INavigationService` abstraction, `IDashcamDevice` + all sub-interfaces, `MockDashcamDevice`, responsive `AppShellView` with connection indicator, `ViewLocator` replaced with pattern-matching implementation (AOT-safe)
**Addresses:** Connection status indicator, responsive layout (portrait/landscape), dark mode
**Avoids:** Avalonia RC API instability (pin versions now); reflection-based ViewLocator (AOT incompatible)
**Research flag:** Standard patterns — well-documented Avalonia MVVM foundation; skip phase research

### Phase 2: Services and Mock Device Integration
**Rationale:** Services are the business logic layer that everything above depends on. Testing services against the mock device before building any UI surface validates the abstractions are correct and catches integration issues early.
**Delivers:** `DeviceService` (discovery, connection, provisioning state), `RecordingService` (file enumeration, virtual trip grouping, archive), `IMediaPlayerService` interface + stub, full unit test coverage against `MockDashcamDevice`
**Addresses:** Mock/demo device mode (required for all development); virtual trip grouping algorithm
**Avoids:** ObservableCollection thread-safety issues (establish marshaling patterns here before UI consumes collections); trip grouping timestamp discontinuities (unit test gap scenarios in this phase)
**Research flag:** Standard patterns — MVVM service layer; skip phase research

### Phase 3: Core UI — Recordings and Playback
**Rationale:** Recording list + detail + video playback are the highest-value user flows and gate all other features. Building them third means services exist and are tested, so UI development is not blocked.
**Delivers:** `RecordingsPage` with filtering, thumbnails, and metadata; `RecordingDetailPage` with video playback; `TripDetailPage`; `ArchiveService` with progress feedback; platform video implementations (LibVLC desktop + ExoPlayer Android)
**Addresses:** Recording list with filtering, thumbnails, recording metadata, video playback, archive to local storage, virtual trips UI
**Avoids:** Video player abstraction failure (IMediaPlayerService must be in place — Phase 1 — before this phase starts); thumbnail memory leak (virtualized list + disk cache from day one); cross-platform video split (desktop and Android tested in this phase, not after)
**Research flag:** Needs deeper research — LibVLCSharp + Avalonia 12 RC1 compatibility at package restore time; ExoPlayer `NativeControlHost` integration; verify `VideoView`-in-UserControl pixel rounding fix in RC1

### Phase 4: Device Settings and Provisioning
**Rationale:** Settings depend on `DeviceService` (Phase 2) and the navigation shell (Phase 1) but not on recording/playback infrastructure. The provisioning wizard is the first-run entry point that leads to settings — build them together.
**Delivers:** `SettingsPage` with all config groups (WiFi, recording modes, channels, shock/radar sensitivity, overlays, danger zone); `ProvisioningPage` with guided wizard; settings validation via `ObservableValidator`
**Addresses:** Device settings configuration, radar sensor sensitivity configuration, provisioning wizard
**Avoids:** Blocking UI during device communication (all commands use `AsyncRelayCommand` + `IsBusy`); settings written to `IDeviceCommands` interface only (no direct HTTP in ViewModels)
**Research flag:** Standard patterns — settings forms are well-understood; skip phase research

### Phase 5: Live Feed
**Rationale:** Live feed is the most platform-specific feature (RTSP/HTTP streaming via platform native players) and is last because it depends on `IMediaPlayerService` (Phase 3) and `DeviceService` (Phase 2). Deferred here to avoid blocking earlier phases on streaming integration complexity.
**Delivers:** `LiveFeedPage` with front/rear channel toggle, stream status, connection state feedback; RTSP/HTTP URI handed to platform player
**Addresses:** Live video preview with front/rear toggle
**Avoids:** No custom streaming code in app layer — platform players consume the URI directly; Android WiFi binding tested here (test on real device with mobile data active)
**Research flag:** Needs deeper research — dashcam RTSP stream format compatibility with LibVLC and ExoPlayer; Android network binding implementation via Xamarin.Android bindings

### Phase 6: Dashboard and SD Card Urgency
**Rationale:** Dashboard is a summary view over data produced by previous phases. Building it last means all data sources exist and can be surfaced without placeholder logic.
**Delivers:** `DashboardPage` with recent recordings summary, events feed (G-shock, radar, parking), SD card usage indicator with overwrite urgency signal, trip statistics summary
**Addresses:** SD card usage indicator, overwrite urgency UX, dashboard home screen
**Avoids:** Archiving urgency invisible to users — "X clips at risk of overwrite" must be prominent
**Research flag:** Standard patterns — aggregation view over existing services; skip phase research

### Phase 7: Telemetry Parsing and Trip Enrichment (v1.x — requires real hardware)
**Rationale:** GPS parsing is LOW confidence until real hardware is in hand and chip format is identified. Deferring to v1.x keeps v1 shippable without hardware. This phase enriches virtual trips with actual distance/speed/G-force data.
**Delivers:** `ITelemetryParser` per vendor chipset; GPS trace in recording detail; trip aggregated stats (avg speed, peak G, distance)
**Addresses:** GPS trace overlay, trip metadata aggregation, G-force display
**Avoids:** GPS coordinate format misparse (DDDMM.MMMM vs decimal degrees); static parser methods (must be behind interface with fixture-based unit tests)
**Research flag:** Needs deep research — run `exiftool -all -u` on real device MP4 to identify chipset format before implementing; vendor format is reverse-engineered, documentation quality is LOW

### Phase Ordering Rationale

- Interfaces before implementations: every phase gate produces a stable contract the next phase depends on. No phase needs to reach into a previous phase's implementation.
- Mock device enables all phases: no phase is blocked on real hardware. v1 is a fully functional app against the mock.
- Video abstraction in Phase 3, not Phase 5: LibVLC and ExoPlayer must be integrated at the same time as recording playback so Android is tested before the architecture is entrenched.
- Telemetry last: GPS parsing is LOW confidence and hardware-gated. All other differentiators (virtual trips, settings, live feed) ship without it.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (Recording and Playback):** LibVLCSharp 3.9.6 dependency constraint requires Avalonia >= 11.0.4; verify NuGet accepts 12.0.0-rc1 at package restore. If rejected, `LibVLCSharp.Avalonia.Unofficial` (jpmikkers) is the fallback. ExoPlayer `NativeControlHost` bridge pattern needs validation against real Android emulator.
- **Phase 5 (Live Feed):** Dashcam RTSP stream authentication and format varies by vendor. Android `ConnectivityManager.bindProcessToNetwork()` requires Xamarin.Android binding research before implementation.
- **Phase 7 (Telemetry):** Entire phase is LOW confidence until real hardware MP4 files are available. Do not start implementation without running `exiftool -all -u` on actual device output.

Phases with standard patterns (skip research):
- **Phase 1 (Foundation):** Avalonia MVVM, DI wiring, navigation shell — well-documented in official Avalonia docs.
- **Phase 2 (Services):** Standard service layer patterns over injected interfaces — no novel patterns.
- **Phase 4 (Settings):** Settings forms with `ObservableValidator` — documented CommunityToolkit.Mvvm pattern.
- **Phase 6 (Dashboard):** Aggregation view over existing services — no novel integration.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM-HIGH | Core stack locked in (HIGH); LibVLCSharp + Avalonia 12 RC1 compatibility unverified at package restore (MEDIUM); GPS parsing library gap is a known unknown |
| Features | MEDIUM | Competitor feature sets confirmed via official sources (HIGH); user pain points from community forums (MEDIUM); v1 feature scope is well-defined |
| Architecture | HIGH | Avalonia 12 page navigation, MVVM patterns, DI, interface segregation all confirmed via official docs; video pipeline specifics MEDIUM |
| Pitfalls | MEDIUM | Avalonia 11 community experience extrapolated to RC1; GPS/accelerometer format pitfalls HIGH confidence from community reverse-engineering; Android WiFi behavior confirmed by vendor support documentation |

**Overall confidence:** MEDIUM-HIGH — sufficient to begin roadmap creation and phase planning. The primary unknowns (GPS format, real device protocol, LibVLCSharp RC1 compatibility) are all deliberately deferred.

### Gaps to Address

- **LibVLCSharp + Avalonia 12 RC1 dependency constraint:** Verify at first package restore in Phase 3. If `Avalonia >= 11.0.4` constraint is not satisfied by `12.0.0-rc1`, use version float or unofficial package as fallback.
- **Linux video bundling:** `VideoLAN.LibVLC.Windows` only bundles Windows DLLs. Linux desktop requires `apt install libvlc-dev` at OS level or `VideoLAN.LibVLC.Linux` NuGet. Resolve before claiming Linux desktop support.
- **Real device GPS atom format:** Unknown until real hardware is in hand. Run `exiftool -all -u` on first MP4 from real device to identify chipset (Novatek / CAMM / BlackVue proprietary). Do not start Phase 7 without this.
- **Thumbnail extraction mechanism:** `MetadataExtractor` does not extract video frames. Decision between `LibVLCSharp.MediaPlayer.TakeSnapshot()` and FFMediaToolkit is deferred to Phase 3.
- **Android Keystore / DPAPI for device credentials:** If the real device requires a WiFi password, it must be stored via platform secure storage. Defer design until Phase 5 (real device protocol is out of scope for v1 mock).

## Sources

### Primary (HIGH confidence)
- [NuGet: LibVLCSharp.Avalonia 3.9.6](https://www.nuget.org/packages/LibVLCSharp.Avalonia) — version and Avalonia compatibility
- [NuGet: Xamarin.AndroidX.Media3.ExoPlayer 1.9.2.1](https://www.nuget.org/packages/Xamarin.AndroidX.Media3.ExoPlayer) — net10.0-android support
- [NuGet: MetadataExtractor 2.9.2](https://www.nuget.org/packages/MetadataExtractor/) — MP4/MOV support
- [NuGet: CommunityToolkit.Mvvm 8.4.1](https://www.nuget.org/packages/CommunityToolkit.Mvvm) — latest stable
- [NuGet: NSubstitute 5.3.0](https://www.nuget.org/packages/NSubstitute), [FluentAssertions 8.9.0](https://www.nuget.org/packages/fluentassertions/)
- [Avalonia Docs: ContentPage](https://docs.avaloniaui.net/controls/navigation/contentpage), [View Locator](https://docs.avaloniaui.net/docs/data-templates/view-locator), [Headless XUnit](https://docs.avaloniaui.net/docs/concepts/headless/headless-xunit)
- [GitHub: Avalonia 12 RC1 release notes](https://github.com/AvaloniaUI/Avalonia/releases) — NativeControlHost pixel rounding fix confirmed
- [CommunityToolkit.Mvvm docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

### Secondary (MEDIUM confidence)
- [Monsalma: Avalonia native video playback (Feb 2025)](https://monsalma.net/avalonia-ui-native-video-playback-featuring-libvlcsharp-and-exoplayer/) — LibVLCSharp + ExoPlayer dual-platform pattern
- [NuGet: Xaml.Behaviors.Avalonia 12.0.0-rc1](https://www.nuget.org/packages/Xaml.Behaviors.Avalonia) — Avalonia 12 AdaptiveBehavior
- [Avalonia ObservableCollection thread safety](https://github.com/AvaloniaUI/Avalonia/discussions/19193) — WeakEvent race condition confirmed
- [Avalonia 12 Breaking Changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes) — navigation API stability risk
- [Dashcam Viewer features](https://dashcamviewer.com/) — competitor trip grouping approach
- [BlackboxMyCar: dashcam WiFi Android troubleshooting](https://www.blackboxmycar.com/pages/dash-cam-wi-fi-troubleshooting-guide) — Smart Network Switch behavior confirmed
- [Google CAMM Spec](https://developers.google.com/streetview/publish/camm-spec) — CAMM track format

### Tertiary (LOW confidence)
- [DashCamTalk: Novatek GPS atom format](https://dashcamtalk.com/forum/threads/script-to-extract-gps-data-from-novatek-mp4.20808/) — community reverse-engineering; needs validation against real device
- [nb-dashcam-tools format docs](https://github.com/skyhisi/nb-dashcam-tools/blob/main/doc/camera-file-format.md) — vendor GPS format; version-specific
- [70mai reverse engineering](https://alu.dog/posts/reverse-engineering-the-70mai-android-app/) — vendor WiFi API pattern; may not apply to target device

---
*Research completed: 2026-03-24*
*Ready for roadmap: yes*
