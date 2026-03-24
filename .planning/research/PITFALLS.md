# Pitfalls Research

**Domain:** Cross-platform dashcam management app (Avalonia 12 RC1, .NET 10, C#, desktop + Android)
**Researched:** 2026-03-24
**Confidence:** MEDIUM — Avalonia 12 RC1 is pre-release; some findings rely on Avalonia 11 community experience extrapolated forward. GPS/accelerometer parsing findings are HIGH confidence from community reverse-engineering work.

---

## Critical Pitfalls

### Pitfall 1: ObservableCollection Modified from Background Threads

**What goes wrong:**
Video file scanning, thumbnail generation, and recording list population all happen on background threads. When code updates a bound `ObservableCollection<T>` from those threads without marshaling to the UI thread, the app runs fine in development but throws intermittent `NullReferenceException` in Avalonia's `WeakEvent` infrastructure under real load.

**Why it happens:**
Unlike WPF, Avalonia does not throw immediately when you update a bound collection off the UI thread. There is no equivalent to WPF's `BindingOperations.EnableCollectionSynchronization`. The failure is a race condition — it only triggers when a binding operation coincides with a collection mutation on another thread, making it feel random.

**How to avoid:**
Every mutation of a UI-bound collection must be wrapped in `Dispatcher.UIThread.Post()` or `Dispatcher.UIThread.InvokeAsync()`. Establish this as a project rule from the first ViewModel. Consider wrapping collection updates in a helper that enforces marshaling. For bulk loads (scanning a recording list), build the list fully off-thread, then swap the entire collection reference on the UI thread rather than adding items one-by-one.

**Warning signs:**
- `NullReferenceException` stack traces containing `Avalonia.Utilities.WeakEvent` or `WeakHashList`
- Crashes that only appear when scanning large numbers of recordings
- Tests pass but app crashes under real device with 100+ recordings

**Phase to address:**
Core data layer / Recording list phase — before any background scanning is wired to the UI.

---

### Pitfall 2: Video Player Has No Universal Cross-Platform Solution

**What goes wrong:**
The team picks a single video playback library expecting it to work on both desktop and Android, then discovers mid-project that it only works on one platform. LibVLCSharp works on desktop but not on Android. There is currently no single Avalonia control that provides native video playback across desktop and mobile simultaneously.

**Why it happens:**
Avalonia's video playback story is still maturing. `LibVLCSharp.Avalonia` wraps LibVLC which has no Android support. The official Avalonia `MediaPlayer` (Accelerate tier) is documented for embedded Linux. `NativeControlHost` is required to bridge platform-native players (ExoPlayer on Android, MediaElement/LibVLC on desktop).

**How to avoid:**
Design an `IVideoPlayer` abstraction from the start. Provide a desktop implementation (LibVLCSharp or FFmpegCore-based render loop) and a separate Android implementation (`NativeControlHost` + ExoPlayer via Android bindings). Wire them through DI by platform. Do not assume one library covers both targets.

**Warning signs:**
- A single `<VideoView>` or `<MediaPlayer>` control used without any platform guard
- No `IVideoPlayer` interface in the solution
- Video playback works in the desktop head but has never been tested on Android emulator

**Phase to address:**
Recording detail view / video player phase — establish the abstraction before building any playback UI.

---

### Pitfall 3: GPS/Accelerometer Data Format Is Vendor-Specific and Undocumented

**What goes wrong:**
The parser written for one dashcam vendor's MP4 format silently produces garbage or throws exceptions against another vendor's files. Different vendors embed telemetry as: a private MP4 `text` track in their own binary format (Novatek/VIOFO style), CAMM (Google Camera Motion Metadata), NMEA 0183 sentences with non-standard checksums, a separate `.txt` or `.bin` sidecar file, or burned-in overlay pixels with no extractable data at all.

**Why it happens:**
There is no standard for dashcam telemetry embedding. The ecosystem is entirely reverse-engineered. Even within one vendor, firmware versions change the format. NMEA lines may lack valid checksums. GPS coordinates may be in DDDMM.MMMM format (degrees-minutes) rather than decimal degrees — a subtle misparse that produces points in the wrong ocean.

**How to avoid:**
Define an `ITelemetryParser` interface that returns a normalized `TelemetryTrack` (timestamped GPS + G-force records). Ship a null/stub implementation for the mock device. Parse DDDMM.MMMM → decimal degrees correctly (divide minutes part by 60, not the whole value). Write unit tests against real byte-level fixture files for each vendor format added. Log and swallow parse errors per-frame rather than aborting the whole track — real recordings contain corrupt frames.

**Warning signs:**
- GPS coordinates display in wrong hemisphere or in the middle of an ocean
- Parser implementation is a static method with no interface or test coverage
- Accelerometer values are clipped to 0 because the unit (mG vs G) was assumed wrong
- No fixture files for parser unit tests

**Phase to address:**
Telemetry parsing phase — before recording metadata display and the trip map view.

---

### Pitfall 4: Android "No Internet on WiFi" Causes Silent Connection Failure

**What goes wrong:**
On Android, when the user connects to the dashcam's WiFi hotspot (which has no internet), Android's "Smart Network Switch" / "Intelligent WiFi" automatically disconnects from the hotspot or routes traffic via mobile data instead. The app's HTTP calls to the dashcam either fail with connection refused or reach the internet instead of the device, with no clear error surfaced to the user.

**Why it happens:**
Android treats WiFi networks without internet as degraded and may drop them or prefer mobile data. The dashcam AP is a private, internet-free hotspot. Android 10+ added aggressive network switching behavior. The app has no control over OS-level network selection by default.

**How to avoid:**
On Android, request the `CHANGE_NETWORK_STATE` permission and bind the HTTP client's socket to the specific WiFi network interface using `ConnectivityManager.bindProcessToNetwork()` or the equivalent Avalonia Android binding. Surface a clear "Connected to dashcam WiFi — internet unavailable" notice in the UI so users understand the state. Test on a real Android device (not just emulator) with mobile data active.

**Warning signs:**
- Device discovery always works on desktop but intermittently fails on Android
- HTTP requests succeed on emulator (which has no mobile data competing) but fail on real device
- No Android permission manifest entry for network state

**Phase to address:**
Device discovery and connection phase — must be validated on real Android hardware before claiming the feature is done.

---

### Pitfall 5: Avalonia 12 RC1 API Instability — Features Change Before Stable Release

**What goes wrong:**
The app is built against Avalonia 12 RC1's page-based navigation API (`ContentPage`, `NavigationPage`, `TabbedPage`, `DrawerPage`, `CommandBar`). Between RC1 and stable, Avalonia makes breaking changes to these APIs. Refactoring the navigation shell mid-project costs a sprint.

**Why it happens:**
RC releases are explicitly pre-stable. The Avalonia team's own guidance: RC is "not a feature-complete release but rather the first in a series of staged previews to gather meaningful feedback before locking things down." Avalonia 12's breaking changes list is already long relative to v11, covering compiled bindings defaults, plugin system removal, and selection behavior.

**How to avoid:**
Pin the exact Avalonia 12 RC1 NuGet package version in `Directory.Build.props` and do not float it. Subscribe to the Avalonia GitHub release feed. Isolate navigation primitives behind an `INavigationService` abstraction so the concrete page type used can be swapped without touching every feature screen. Allocate buffer in the roadmap for a "Avalonia upgrade" task when stable releases.

**Warning signs:**
- `<PackageReference Include="Avalonia" Version="12.*-*" />` (floating pre-release)
- Navigation calls are scattered directly through `NavigationPage` without any abstraction
- No upgrade task budgeted in later phases

**Phase to address:**
Project skeleton / navigation shell phase — pin versions and build the abstraction before any feature screens are built on top of it.

---

### Pitfall 6: Virtual Trip Grouping Breaks on Timestamp Discontinuities

**What goes wrong:**
The "combine consecutive recordings into trips" feature uses file timestamps to detect adjacency. Dashcams introduce gaps of several seconds between clips (SD card write latency, file system overhead). A naive "next file starts within N seconds of previous file end" threshold either merges clips from two separate drives (false positive) or splits a single drive into dozens of 1-clip "trips" (false negative).

**Why it happens:**
Dashcam clip files are sequential but not gapless. Observed real-world gaps: 1–13 seconds between clips during normal recording; 20+ seconds if the camera was buffering an event. File system timestamps can also drift if the dashcam's RTC was not GPS-synced yet at trip start. Using filename sort order is more reliable than file-modified-time for sequencing.

**How to avoid:**
Use a configurable adjacency threshold (default 30 seconds) rather than a fixed value. Sequence clips by filename (which encodes capture index) not file-modified-time. Detect RTC drift: if embedded GPS timestamps diverge significantly from filename timestamps, log a warning. Write unit tests with gap scenarios (0s, 5s, 15s, 45s, 300s) to validate grouping edge cases.

**Warning signs:**
- Trip grouping logic hardcodes a threshold like `TimeSpan.FromSeconds(5)`
- Clips sorted by `FileInfo.LastWriteTime` rather than filename
- No unit tests for the adjacency algorithm

**Phase to address:**
Trip grouping / recordings list phase — before the virtual trip UI is built.

---

### Pitfall 7: Thumbnail Generation Blocks the UI or Leaks Memory

**What goes wrong:**
Generating video thumbnails for a recording list of 200+ clips causes the UI to freeze, or memory usage balloons to multiple gigabytes and the app is killed by the OS. Both desktop and Android are affected.

**Why it happens:**
Video thumbnail extraction (via FFmpeg subprocess or frame decode) requires decoding at least one video frame. Decoding a 1080p H.264 frame in memory, then scaling it, can consume 50–200 MB temporarily per frame. If thumbnails are generated eagerly (all at once on list load) or on the UI thread, the result is either a frozen app or an OOM kill.

**How to avoid:**
Generate thumbnails lazily — only when a recording cell scrolls into view (virtualized list). Run all thumbnail work on a background thread pool with a concurrency limit (e.g., `SemaphoreSlim(2)`). Cache generated thumbnails to disk (`.png` alongside the video, or in an app cache folder) so they are not regenerated on every launch. Dispose decoded bitmaps immediately after scaling. On Android, respect the memory class from `ActivityManager.getMemoryClass()` to size the in-memory thumbnail cache.

**Warning signs:**
- `IImage` / `Bitmap` objects loaded in a loop without `Dispose()` calls
- Thumbnail generation triggered in a `foreach` loop on collection load
- No disk cache — thumbnails regenerated every app launch
- App slows as recording count grows

**Phase to address:**
Recording list phase — before the list is wired to real (or mock) video files.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Concrete `HttpClient` calls in ViewModel directly | Fast to write | Cannot unit test, cannot swap vendors, network errors reach UI layer directly | Never — inject `IDeviceClient` |
| Static GPS parser methods with no interface | Quick implementation | Every vendor requires touching the same class, no mock for tests | Never — define `ITelemetryParser` from day one |
| Hardcode dashcam IP (192.168.0.1) in config | No discovery complexity | Breaks if dashcam AP uses different subnet | Acceptable in mock device only |
| Synchronous file I/O for recording scan | Simple code | UI freeze on SD cards with 500+ files | Never — use async enumeration |
| Single video player library without abstraction | One integration to maintain | Breaks on Android; impossible to test | Never — hide behind `IVideoPlayer` |
| Floating Avalonia pre-release version | Always get latest fixes | Silent API breakage in CI | Never — pin exact version |

---

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Dashcam WiFi HTTP API | Assume one vendor's endpoint paths work for all (e.g., `/cgi-bin/api.cgi`) | Route all calls through `IDeviceClient` with vendor-specific implementations; test each vendor in isolation |
| FFmpeg / FFmpegCore for thumbnails | Shell out to `ffmpeg` assuming it exists in PATH | Bundle or locate FFmpeg explicitly; handle "not found" gracefully with a placeholder thumbnail |
| Embedded GPS extraction | Read MP4 atom headers assuming fixed offset | Use a proper MP4 box parser; iterate boxes by size/type rather than fixed offsets |
| Android WiFi binding | Use default `HttpClient` without network binding | Bind socket to the dashcam WiFi network interface using `ConnectivityManager` on Android |
| Avalonia Dispatcher | Call `Dispatcher.UIThread.InvokeAsync` and ignore the returned Task | Always await or `.GetAwaiter().GetResult()` in synchronous context; ignoring can hide exceptions |

---

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Eager thumbnail generation for all recordings | UI freeze on list load; OOM on Android | Lazy virtualized generation with concurrency cap + disk cache | ~50+ recordings |
| Parsing GPS track for every recording list refresh | Slow list load; high CPU | Parse once, persist normalized telemetry to a local SQLite/JSON cache | ~20+ recordings with embedded GPS |
| Unbounded background Task spawning for file ops | Thread pool starvation; sluggish UI on background | Use `SemaphoreSlim` or `Channel<T>` to bound concurrency | Concurrent scans of large SD card |
| Loading full video into memory for thumbnail | Memory spike; OOM kill on Android | Seek to first keyframe only; use streaming frame decode | Every recording if not fixed |
| Virtualized list not used for recording grid | Memory grows linearly with recording count | Use Avalonia's `ItemsRepeater` with recycling or a proper virtualized panel | ~100+ recordings |

---

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| Trusting dashcam API responses without validation | Malformed vendor firmware response causes app crash or corrupts local data | Validate and sanitize all API response fields; never deserialize directly into domain models |
| Writing archived video to user-chosen path without sanitizing | Path traversal if path came from device name or recording metadata | Use `Path.GetFullPath` + check it is within intended directory before writing |
| Storing device credentials (WiFi password) in plain text in app settings | Credential exposure if device is shared | Use platform secure storage (Android Keystore / desktop DPAPI) via an `ISecureStorage` abstraction |

---

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Showing raw file list rather than trips | Users must manually identify which clips belong to a drive; overwhelming for 200+ clips | Default view is virtual trips; raw file list is secondary/advanced |
| No archiving urgency signal | Users lose important footage because dashcam overwrites before they act | Show "X clips at risk of overwrite" prominently on dashboard; sort events by recency |
| Connection indicator that only shows connected/disconnected | User doesn't know if discovery is in progress or if they need to switch WiFi | Three states: Searching, Connected, Disconnected — with actionable guidance for each |
| Fixed layout for desktop that doesn't adapt to portrait mobile | Unusable on phone in portrait orientation | Responsive breakpoints: portrait mobile gets vertical tab bar + stacked layout; landscape/desktop gets side nav + split view |
| Video player without playback speed control | Dashcam clips at 1x are tedious to review for incidents | Include at minimum 0.5x, 1x, 2x, 4x speed controls |
| Blocking UI during archive operation | User thinks app is frozen during large copy | Progress bar with cancel; archive runs in background with completion notification |

---

## "Looks Done But Isn't" Checklist

- [ ] **Recording list:** Thumbnails load from disk cache, not regenerated every launch — verify by checking cache dir is populated after first run
- [ ] **Trip grouping:** Tested with actual timestamp gaps (5s, 15s, 45s) — not just perfectly adjacent clips
- [ ] **Video player:** Tested on Android device (not just desktop) — LibVLC-only implementations will silently fail here
- [ ] **GPS display:** Coordinates verified with a known location — DDDMM.MMMM misparse produces plausible-looking but wrong coordinates
- [ ] **Device discovery:** Tested on Android with mobile data active — Android Smart Network Switch can silently redirect traffic
- [ ] **Background scanning:** ObservableCollection updates verified to come from UI thread — add a `Debug.Assert(Dispatcher.UIThread.CheckAccess())` guard
- [ ] **Archive feature:** Tested with a recording that is actively being overwritten — file lock / partial copy edge case
- [ ] **Vendor abstraction:** A second mock vendor added to confirm the abstraction actually isolates vendor concerns — if adding a second vendor requires touching ViewModel code, the abstraction is leaking

---

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| ObservableCollection thread violations | LOW | Grep for all collection mutations, wrap each in `Dispatcher.UIThread.Post`; add CI lint rule |
| Video player wrong abstraction level | MEDIUM | Introduce `IVideoPlayer`, move current code to `DesktopVideoPlayer`, stub `AndroidVideoPlayer`; ~1–2 days |
| GPS parser wrong coordinate format | LOW | Fix DDDMM → decimal conversion; re-parse cached data; verify with known fixture |
| Android WiFi binding missing | MEDIUM | Add `ConnectivityManager.bindProcessToNetwork` in Android head project; requires Android-specific code path |
| Avalonia RC API breakage | MEDIUM–HIGH | Pin old version until migration window; follow official migration guide; ~0.5–2 days depending on API surface used |
| Trip grouping threshold wrong | LOW | Tune threshold constant; add configurable setting; fix unit tests |
| Thumbnail memory leak | MEDIUM | Audit all `Bitmap` creation sites for `Dispose`; add virtualized panel; add disk cache; ~1–2 days |

---

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| ObservableCollection thread safety | Core data / ViewModel foundation | Run recording scan with 200+ files; check for WeakEvent exceptions in logs |
| Video player abstraction missing | Recording detail / video player | Compile Android head; verify no LibVLC import in shared project |
| GPS format misparse | Telemetry parsing | Unit test with DDDMM fixture; display known location on map |
| Android WiFi binding | Device discovery | Test on real Android device with mobile data active |
| Avalonia RC API instability | Project skeleton / navigation shell | Pin version; confirm CI fails on version float |
| Trip grouping discontinuities | Recordings list / trip grouping | Unit test with 0s, 5s, 15s, 45s, 5-minute gaps |
| Thumbnail memory / performance | Recording list (virtualized) | Profile memory with 200 recordings; confirm disk cache populated |
| Vendor API assumptions | Device abstraction layer | Add second mock vendor; confirm no ViewModel changes required |

---

## Sources

- [Avalonia ObservableCollection Thread Safety Discussion](https://github.com/AvaloniaUI/Avalonia/discussions/19193)
- [Avalonia Application Lifetimes Docs](https://docs.avaloniaui.net/docs/concepts/application-lifetimes)
- [Avalonia Threading Model](https://docs.avaloniaui.net/docs/app-development/threading)
- [Avalonia 12 Breaking Changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes)
- [Avalonia v12 Breaking Changes Wiki](https://github.com/AvaloniaUI/Avalonia/wiki/v12-Breaking-Changes)
- [Avalonia Responsive Layout Discussion](https://github.com/AvaloniaUI/Avalonia/discussions/15991)
- [Avalonia Video Player Discussion](https://github.com/AvaloniaUI/Avalonia/discussions/10683)
- [LibVLCSharp + Avalonia NativeControlHost approach](https://monsalma.net/avalonia-ui-native-video-playback-featuring-libvlcsharp-and-exoplayer/)
- [Avalonia Android SetupBuilder double-init bug](https://github.com/AvaloniaUI/Avalonia/issues/8330)
- [DashCam GPS Format Community Discussion](https://dashcamtalk.com/forum/threads/format-of-gps-data.7333/)
- [Dashcam timestamp gap detection](https://victorchang.codes/quickly-detect-gaps-in-footage)
- [BlackVue WiFi HTTP API](https://github.com/bartbroere/blackvue-wifi)
- [70mai reverse engineering](https://alu.dog/posts/reverse-engineering-the-70mai-android-app/)
- [Dashcam WiFi Android Smart Network Switch](https://www.blackboxmycar.com/pages/dash-cam-wi-fi-troubleshooting-guide)
- [nb-dashcam-tools format docs](https://github.com/skyhisi/nb-dashcam-tools/blob/main/doc/camera-file-format.md)
- [Dashcam Viewer GPS format research](https://dashcamviewer.com/resources/frequently-asked-questions/)

---
*Pitfalls research for: BlackBoxBuddy — cross-platform dashcam management app*
*Researched: 2026-03-24*
