# Phase 3: Recordings - Research

**Researched:** 2026-03-24
**Domain:** Avalonia 12 RC1 list/detail UI, LibVLCSharp video playback, virtual trip grouping, archive to local storage
**Confidence:** HIGH (core patterns verified against existing codebase and official docs)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Replace `IDeviceFileSystem.ListRecordingsAsync` return type from `IReadOnlyList<string>` to `IReadOnlyList<Recording>` where `Recording` is a record type containing: FileName, DateTime, Duration, FileSize, AvgSpeed, PeakGForce, Distance, EventType, CameraChannel, ThumbnailData
- **D-02:** EventType is a simple enum: `None` (normal driving), `Radar`, `GShock`, `Parking` — no flags, single event per recording
- **D-03:** Mock device generates 15-20 recordings with realistic fake data using Bogus — mix of event types, spread across several days, with consecutive sequences for virtual trip testing
- **D-04:** Mock thumbnails are generated in-memory as small solid-color bitmap blocks (160x90), color-coded by event type — no embedded image assets needed
- **D-05:** Card row layout — each recording is a horizontal card with thumbnail on the left, metadata stacked on the right (date/time, duration, file size on one line; speed, G-force, distance on another), event type shown as a colored badge/chip
- **D-06:** Event type filtering via horizontal toggle chip bar at top: All | Radar | G-Shock | Parking — single-select (radio behavior)
- **D-07:** Virtual trips shown inline with grouping — trip header card with aggregated stats, individual clips nested/indented below. Tapping trip header opens trip detail; tapping individual clip opens that clip's detail
- **D-08:** Fixed sort order: newest recordings first, no sort controls
- **D-09:** Bundle a small (~2-5 sec) real MP4 file as an embedded resource for mock device playback — enables real LibVLCSharp integration testing
- **D-10:** Recording detail navigated via NavigationPage push with back button (standard mobile navigation pattern using Avalonia 12's NavigationPage.PushAsync)
- **D-11:** Dual player side-by-side layout when both front and rear recordings exist — side-by-side on desktop, stacked vertically on mobile
- **D-12:** Extended player controls: play/pause, seek bar, current time/total duration, fullscreen toggle, playback speed (0.5x, 1x, 2x), frame-by-frame step
- **D-13:** Consecutive recordings grouped into virtual trips using a time gap threshold — recordings are consecutive if the next one starts within 30 seconds of the previous ending
- **D-14:** Trip detail uses the same UI as single recording detail (TRIP-02) with aggregated metadata (total duration, total distance, average speed, peak G-force across all clips)
- **D-15:** Archive downloads to a default app directory (~/BlackBoxBuddy/Archives/ on desktop, app external storage on Android) with progress bar — no file picker by default
- **D-16:** Multi-select mode for archiving: long-press or "Select" button enters multi-select, user checks specific clips, then taps "Archive Selected" — also has "Select All" for full trip archive
- **D-17:** Batch download for multi-clip archive — selected clips downloaded into a subfolder with overall progress bar
- **D-18:** Archived recordings visually marked in the list with a checkmark/"Archived" badge

### Claude's Discretion

- Exact card styling, spacing, and responsive breakpoints for recording cards
- LibVLCSharp VideoView integration details and platform-specific player setup
- Loading states for recording list and detail view
- Error handling for failed downloads or playback issues
- Exact frame-step implementation (LibVLC API specifics)
- Archive folder naming convention and file organization
- Progress bar styling and cancel-download support
- Mock data distribution (how many of each event type, date spread, trip sequences)

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RECD-01 | User can view list of all recordings | Recording data model + ObservableCollection + ListBox in RecordingsPage |
| RECD-02 | User can filter recordings by Radar Event type | Filter chip bar + ICollectionView or filtered ObservableCollection |
| RECD-03 | User can filter recordings by G-Shock Event type | Same filter mechanism |
| RECD-04 | User can filter recordings by Parking Event type | Same filter mechanism |
| RECD-05 | Each recording shows video thumbnail in list view | ThumbnailData byte[] → WriteableBitmap or BitmapImage binding |
| RECD-06 | Each recording shows date and time | DateTime property on Recording record |
| RECD-07 | Each recording shows duration | TimeSpan Duration on Recording record |
| RECD-08 | Each recording shows file size | long FileSize on Recording record |
| RECD-09 | Each recording shows average speed | double AvgSpeed on Recording record |
| RECD-10 | Each recording shows peak G-force | double PeakGForce on Recording record |
| RECD-11 | Each recording shows traveled distance | double Distance on Recording record |
| RDTL-01 | User can play back recording with video player | LibVLCSharp VideoView + IMediaPlayerService implementation |
| RDTL-02 | Recording detail shows video thumbnail | Same ThumbnailData binding as list |
| RDTL-03 | Recording detail shows date and time | Pass Recording model to detail VM |
| RDTL-04 | Recording detail shows duration | Pass Recording model to detail VM |
| RDTL-05 | Recording detail shows file size | Pass Recording model to detail VM |
| RDTL-06 | Recording detail shows average speed | Pass Recording model to detail VM |
| RDTL-07 | Recording detail shows peak G-force | Pass Recording model to detail VM |
| RDTL-08 | Recording detail shows traveled distance | Pass Recording model to detail VM |
| TRIP-01 | App automatically combines consecutive recordings into virtual trips | ITripGroupingService using 30-sec gap threshold |
| TRIP-02 | Virtual trip uses same UI as single recording detail | RecordingDetailViewModel accepts either Recording or Trip |
| TRIP-03 | Virtual trip shows aggregated metadata | Aggregate LINQ over Trip.Clips collection |
| ARCH-01 | User can archive a single recording to local storage | IArchiveService.ArchiveAsync(Recording) with progress |
| ARCH-02 | User can archive a virtual trip to local storage | IArchiveService.ArchiveTripAsync(Trip) batch download |
| ARCH-03 | Archive operation downloads content from dashcam to device storage | IDeviceFileSystem.DownloadFileAsync → file write with progress |
</phase_requirements>

---

## Summary

Phase 3 is the core value delivery phase. It builds on the two completed phases (foundation navigation + settings) and introduces the most complex UI in the app: a heterogeneous scrollable list (mixed standalone recordings and trip groups), a pushed NavigationPage detail with dual video players, and a batch archive download flow.

The architecture has two main new concerns beyond what Phases 1 and 2 established. First, the recording list must display two kinds of items — standalone `Recording` entries and `TripGroup` aggregates — in a single `ListBox`. Avalonia handles this cleanly with a custom `IDataTemplate` that selects different card templates by item type. Second, video playback requires `LibVLCSharp.Avalonia` on desktop, which must be added to `Directory.Packages.props` and wired into the existing `IMediaPlayerService` stub. The `NavigationService` stub left from Phase 1 must be fully implemented here since D-10 requires `NavigationPage.PushAsync` for the detail view.

The key risk is the `LibVLCSharp.Avalonia` 3.9.6 package constraint `Avalonia >= 11.0.4`. The project uses `12.0.0-rc1`, which satisfies the constraint numerically, but NuGet's pre-release version comparison is the blocker flagged in STATE.md. The fallback is `LibVLCSharp.Avalonia.Unofficial` (jpmikkers). Plan for this contingency in the first wave that adds the package.

**Primary recommendation:** Implement bottom-up — data model refactor first, then grouping service, then list UI, then detail + video, then archive. This matches dependency order and keeps each wave independently verifiable.

---

## Standard Stack

### Core (all already in Directory.Packages.props or project files)

| Library | Version | Purpose | Notes |
|---------|---------|---------|-------|
| AvaloniaUI | 12.0.0-rc1 | UI framework, ListBox, DataTemplates, NavigationPage | Already pinned |
| CommunityToolkit.Mvvm | 8.4.1 | `[ObservableProperty]`, `[RelayCommand]`, `ObservableCollection` | Already referenced |
| Bogus | 35.6.5 | Generate 15-20 rich mock `Recording` objects | Already in test project; also needed in main project for MockDashcamDevice |
| Xaml.Behaviors.Avalonia | 12.0.0-rc1 | `AdaptiveBehavior` for dual-player layout breakpoints | Already referenced |

### New Packages to Add

| Library | Version | Project | Purpose |
|---------|---------|---------|---------|
| `LibVLCSharp` | 3.9.6 | `BlackBoxBuddy.Desktop.csproj` | Core VLC engine bindings |
| `LibVLCSharp.Avalonia` | 3.9.6 | `BlackBoxBuddy.Desktop.csproj` | `VideoView` Avalonia control |
| `VideoLAN.LibVLC.Windows` | 3.0.23 | `BlackBoxBuddy.Desktop.csproj` (Windows only) | Bundles native libvlc DLLs |
| `Bogus` | 35.6.5 | `BlackBoxBuddy.csproj` (shared) | MockDashcamDevice uses Bogus at runtime |

**Add to Directory.Packages.props:**
```xml
<PackageVersion Include="LibVLCSharp" Version="3.9.6"/>
<PackageVersion Include="LibVLCSharp.Avalonia" Version="3.9.6"/>
<PackageVersion Include="VideoLAN.LibVLC.Windows" Version="3.0.23"/>
```

Bogus is already in `Directory.Packages.props` (version 35.6.5) for the test project. Adding `<PackageReference Include="Bogus"/>` to `BlackBoxBuddy.csproj` reuses the same pinned version.

**Add to BlackBoxBuddy.Desktop.csproj:**
```xml
<PackageReference Include="LibVLCSharp"/>
<PackageReference Include="LibVLCSharp.Avalonia"/>
<PackageReference Include="VideoLAN.LibVLC.Windows"
    Condition="$([MSBuild]::IsOSPlatform('Windows'))"/>
```

**Add to BlackBoxBuddy.csproj (shared):**
```xml
<PackageReference Include="Bogus"/>
```

### Fallback for LibVLCSharp + Avalonia 12 RC1

If `LibVLCSharp.Avalonia` 3.9.6 rejects `12.0.0-rc1` at package restore, use:
```xml
<PackageVersion Include="LibVLCSharp.Avalonia.Unofficial" Version="3.9.6.1"/>
```
Source: `jpmikkers/LibVLCSharp.Avalonia.Unofficial` on GitHub. Same `VideoView` API surface; just a community-patched build for Avalonia compatibility.

---

## Architecture Patterns

### Recommended Project Structure (new files only)

```
src/BlackBoxBuddy/
├── Models/
│   ├── Recording.cs               # D-01: record type (FileName, DateTime, Duration, ...)
│   ├── EventType.cs               # D-02: enum None | Radar | GShock | Parking
│   ├── TripGroup.cs               # D-07/D-13: record with Clips + aggregated stats
│   └── CameraChannel.cs           # enum Front | Rear | Both
├── Device/
│   └── IDeviceFileSystem.cs       # REFACTOR: ListRecordingsAsync → IReadOnlyList<Recording>
│   └── Mock/
│       └── MockDashcamDevice.cs   # EXPAND: 15-20 rich Recording objects via Bogus
├── Services/
│   ├── IMediaPlayerService.cs     # EXPAND: define Play(Uri), Pause, Stop, Seek, Rate, NextFrame
│   ├── DesktopMediaPlayerService.cs # NEW: LibVLCSharp implementation
│   ├── ITripGroupingService.cs    # NEW: Groups List<Recording> → List<IListItem>
│   ├── TripGroupingService.cs     # NEW: 30-second gap algorithm
│   ├── IArchiveService.cs         # NEW: ArchiveAsync, ArchiveTripAsync with progress
│   └── ArchiveService.cs          # NEW: DownloadFileAsync → write to ~/BlackBoxBuddy/Archives/
├── ViewModels/
│   ├── RecordingsViewModel.cs     # BUILD OUT: filter, grouping, loading, select mode
│   ├── RecordingDetailViewModel.cs # NEW: accepts Recording or TripGroup
│   └── RecordingListItemViewModel.cs # NEW: wrapper for list items (recording or trip header)
└── Views/
    ├── RecordingsPage.axaml       # BUILD OUT: filter chips + ListBox with DataTemplate
    ├── RecordingDetailPage.axaml  # NEW: dual VideoView layout + controls
    └── Converters/
        └── EventTypeToBrushConverter.cs # NEW: matches ConnectionStateToBrushConverter pattern
```

### Pattern 1: Heterogeneous Recording List (mixed items)

**What:** A `ListBox` (or `ItemsControl`) whose items collection contains two concrete types — `RecordingListItemViewModel` (standalone clip) and `TripGroupViewModel` (trip header + nested clips). Avalonia selects different `DataTemplate` objects per item type via a custom `IDataTemplate` implementation.

**When to use:** Whenever a list must render fundamentally different card shapes for different item kinds.

**Example:**
```csharp
// Source: Avalonia Data Templates docs + existing ViewLocator pattern
public class RecordingListDataTemplate : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        RecordingListItemViewModel => new RecordingCard(),
        TripGroupViewModel        => new TripGroupCard(),
        _                         => null
    };

    public bool Match(object? data)
        => data is RecordingListItemViewModel or TripGroupViewModel;
}
```

```xml
<!-- RecordingsPage.axaml -->
<ListBox ItemsSource="{Binding DisplayItems}"
         SelectionMode="Single">
    <ListBox.ItemTemplate>
        <local:RecordingListDataTemplate/>
    </ListBox.ItemTemplate>
</ListBox>
```

The `DisplayItems` collection is an `ObservableCollection<ViewModelBase>` rebuilt when the filter changes or the device data reloads. No `ICollectionView` / `CollectionViewSource` is needed — rebuilding the flat collection on filter change is simpler and sufficient for 15-20 items.

### Pattern 2: NavigationPage Push for Detail View

**What:** `RecordingsPage` wraps itself in a `NavigationPage` so tapping a recording pushes `RecordingDetailPage` onto the navigation stack with a built-in back button.

**Important:** The current `NavigationService.PushAsync` stub always returns `Task.CompletedTask`. It must be fully implemented in Phase 3. The implementation must hold a reference to the active `NavigationPage` set by the shell at startup.

**Example:**
```csharp
// NavigationService.cs — full implementation
public class NavigationService : INavigationService
{
    private NavigationPage? _navigationPage;

    public void SetNavigationPage(NavigationPage page)
        => _navigationPage = page;

    public Task PushAsync(ViewModelBase viewModel)
        => _navigationPage?.PushAsync(
               new ContentPage { DataContext = viewModel })
           ?? Task.CompletedTask;

    public Task PopAsync()
        => _navigationPage?.PopAsync() ?? Task.CompletedTask;
}
```

```xml
<!-- RecordingsPage.axaml — wrap content in NavigationPage -->
<NavigationPage xmlns="https://github.com/avaloniaui" ...>
    <!-- recordings list content -->
</NavigationPage>
```

**How NavigationPage.PushAsync is wired:** The shell creates a `NavigationPage` as the root of each tab and passes it to `NavigationService.SetNavigationPage(...)` during initialization.

### Pattern 3: LibVLCSharp VideoView in Detail Page

**What:** `LibVLC` (singleton per app), `MediaPlayer` (one per video stream, created per detail page), `VideoView` (XAML control bound to `MediaPlayer` via a property).

**Lifecycle rule:** Create one `LibVLC` instance for the desktop app lifetime (in `DesktopMediaPlayerService`). Create a `MediaPlayer` when detail opens; dispose it when detail closes. The `VideoView` does NOT dispose the `MediaPlayer` automatically — explicit `Dispose()` is required.

**Example:**
```csharp
// DesktopMediaPlayerService.cs
public class DesktopMediaPlayerService : IMediaPlayerService, IDisposable
{
    private readonly LibVLC _libVlc = new LibVLC();

    public MediaPlayer CreatePlayer()
        => new MediaPlayer(_libVlc);

    public void Dispose() => _libVlc.Dispose();
}
```

```csharp
// RecordingDetailViewModel.cs
public partial class RecordingDetailViewModel : ViewModelBase, IDisposable
{
    private readonly MediaPlayer _mediaPlayer;

    [ObservableProperty]
    private MediaPlayer mediaPlayer; // bound in XAML

    public RecordingDetailViewModel(IMediaPlayerService svc, Recording recording)
    {
        _mediaPlayer = svc.CreatePlayer();
        MediaPlayer = _mediaPlayer;
        var media = new Media(_libVlc, new Uri(recording.StreamUri));
        _mediaPlayer.Media = media;
    }

    [RelayCommand]
    private void PlayPause()
        => (_mediaPlayer.IsPlaying ? _mediaPlayer.Pause : _mediaPlayer.Play)();

    [RelayCommand]
    private void StepFrame() => _mediaPlayer.NextFrame();

    // Rate: 0.5f, 1.0f, 2.0f
    public void SetRate(float rate) => _mediaPlayer.SetRate(rate);

    public void Dispose() => _mediaPlayer.Dispose();
}
```

```xml
<!-- RecordingDetailPage.axaml — requires xmlns:vlc="using:LibVLCSharp.Avalonia" -->
<vlc:VideoView MediaPlayer="{Binding MediaPlayer}"
               HorizontalAlignment="Stretch"
               VerticalAlignment="Stretch"/>
```

**Key LibVLC API facts (verified):**
- `MediaPlayer.Play()` / `MediaPlayer.Pause()` — playback control
- `MediaPlayer.NextFrame()` — frame-by-frame step (requires player to be paused)
- `MediaPlayer.SetRate(float rate)` — playback speed; 0.5f, 1.0f, 2.0f are valid values
- `MediaPlayer.Time` — current position in milliseconds (long)
- `MediaPlayer.Length` — total duration in milliseconds (long)
- `MediaPlayer.Position` — float 0.0–1.0 for seek bar binding
- `MediaPlayer.TakeSnapshot(uint num, string filePath, uint width, uint height)` — saves frame to file; pass 0,0 for original size
- Events fire on a background thread — marshal to UI thread via `Dispatcher.UIThread.Post()`

### Pattern 4: Virtual Trip Grouping Algorithm

**What:** A pure service that takes a sorted `IReadOnlyList<Recording>` and produces a flat list of `IListItem` (either `RecordingListItemViewModel` for standalone clips or `TripGroupViewModel` for groups).

**Algorithm:**
```csharp
// TripGroupingService.cs
private const int GapThresholdSeconds = 30;

public IReadOnlyList<IListItem> Group(IReadOnlyList<Recording> recordings)
{
    var sorted = recordings.OrderByDescending(r => r.DateTime).ToList();
    var result = new List<IListItem>();
    var i = 0;

    while (i < sorted.Count)
    {
        var group = new List<Recording> { sorted[i] };

        while (i + 1 < sorted.Count)
        {
            var current = sorted[i];
            var next = sorted[i + 1];
            // Consecutive: current ends within 30s of next starting
            // (sorted descending, so current is newer than next)
            var currentEnd = current.DateTime + current.Duration;
            var gap = next.DateTime + next.Duration - currentEnd;
            // gap is negative for consecutive clips (next ends before current starts)
            // Recalculate: sorted newest-first means sorted[i].DateTime > sorted[i+1].DateTime
            var gapBetween = sorted[i + 1].DateTime + sorted[i + 1].Duration;
            var startOfCurrent = sorted[i].DateTime;
            if ((startOfCurrent - gapBetween).TotalSeconds <= GapThresholdSeconds)
            {
                group.Add(sorted[++i]);
            }
            else break;
        }

        result.Add(group.Count > 1
            ? new TripGroupViewModel(group)
            : new RecordingListItemViewModel(group[0]));
        i++;
    }
    return result;
}
```

**Note:** The planner should restate this algorithm cleanly. The key invariant is: recordings sorted newest-first, a gap of <= 30 seconds between the end of recording[i+1] and the start of recording[i] means they belong to the same trip.

### Pattern 5: Archive with Progress

**What:** `IArchiveService` wraps `IDeviceFileSystem.DownloadFileAsync` and writes the stream to disk. Uses `IProgress<double>` for progress reporting back to the ViewModel.

**Example:**
```csharp
// IArchiveService.cs
public interface IArchiveService
{
    Task ArchiveAsync(Recording recording, IProgress<double>? progress = null,
        CancellationToken ct = default);
    Task ArchiveTripAsync(TripGroup trip, IProgress<double>? progress = null,
        CancellationToken ct = default);
    string GetArchiveDirectory();
}
```

```csharp
// ArchiveService.cs — desktop path
public string GetArchiveDirectory()
    => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "BlackBoxBuddy", "Archives");
```

Progress for batch: report `(completedFiles / totalFiles) + (currentFileBytes / totalBytes) / totalFiles`.

### Pattern 6: Mock Thumbnail Generation (in-memory bitmap)

**What:** Generate a `byte[]` PNG of a solid-color 160x90 block, color-coded by `EventType`. No embedded assets required. Use `SkiaSharp` or raw BMP/PNG byte construction.

**Recommendation:** Use a hardcoded 160x90 raw BGRA byte array (all pixels the same color). Convert to `WriteableBitmap` in the view via a value converter. This avoids any additional NuGet dependency.

```csharp
// MockThumbnailGenerator.cs
public static byte[] GenerateSolidColor(int width, int height, Color color)
{
    // Raw BGRA bytes — Avalonia WriteableBitmap accepts PixelFormat.Bgra8888
    var pixels = new byte[width * height * 4];
    for (int i = 0; i < pixels.Length; i += 4)
    {
        pixels[i]     = color.B;
        pixels[i + 1] = color.G;
        pixels[i + 2] = color.R;
        pixels[i + 3] = color.A;
    }
    return pixels;
}
```

```csharp
// EventType-to-color mapping (used by mock generator and EventTypeToBrushConverter)
static Color ColorFor(EventType type) => type switch
{
    EventType.Radar   => Color.FromArgb(255, 33, 150, 243),  // blue
    EventType.GShock  => Color.FromArgb(255, 244, 67, 54),   // red
    EventType.Parking => Color.FromArgb(255, 255, 152, 0),   // amber
    _                 => Color.FromArgb(255, 76, 175, 80),   // green (normal)
};
```

In XAML, bind `ThumbnailData` (byte[]) through a `BytesToBitmapConverter` to `Image.Source`. The converter uses `WriteableBitmap` from `Avalonia.Media.Imaging`.

### Pattern 7: Bogus Mock Data Generation

**What:** `MockDashcamDevice._recordings` refactored to use `Faker<Recording>` with Bogus. Generates 15-20 recordings spread across 3 days with consecutive sequences for trip testing.

```csharp
// MockDashcamDevice.cs — updated _recordings initialization
private static IReadOnlyList<Recording> GenerateMockRecordings()
{
    var baseDate = DateTime.Now.Date.AddDays(-3);
    var faker = new Faker<Recording>()
        .CustomInstantiator(f =>
        {
            var dt = f.Date.Between(baseDate, DateTime.Now);
            var eventType = f.Random.WeightedRandom(
                new[] { EventType.None, EventType.Radar, EventType.GShock, EventType.Parking },
                new[] { 0.5f, 0.2f, 0.2f, 0.1f });
            var channel = f.Random.ArrayElement(
                new[] { CameraChannel.Front, CameraChannel.Rear });
            var duration = TimeSpan.FromSeconds(f.Random.Double(60, 180));
            return new Recording(
                FileName: $"/recordings/{dt:yyyy-MM-dd_HH-mm-ss}_{channel.ToString().ToLower()}.mp4",
                DateTime: dt,
                Duration: duration,
                FileSize: (long)(duration.TotalSeconds * f.Random.Double(800_000, 1_200_000)),
                AvgSpeed: f.Random.Double(0, 120),
                PeakGForce: f.Random.Double(0.1, 3.5),
                Distance: duration.TotalSeconds * f.Random.Double(5, 30) / 3600,
                EventType: eventType,
                CameraChannel: channel,
                ThumbnailData: MockThumbnailGenerator.GenerateSolidColor(160, 90, ColorFor(eventType)));
        });

    return faker.Generate(f.Random.Int(15, 20));
}
```

**Consecutive sequences for trip testing:** Manually prepend 5-6 recordings with timestamps exactly 60 seconds apart (within the 30-second gap threshold) so the grouping algorithm has predictable test cases.

### Anti-Patterns to Avoid

- **MediaPlayer event callbacks on UI thread:** LibVLCSharp fires `Playing`, `TimeChanged`, `PositionChanged` events on a background thread. Never update `ObservableProperty` fields from these handlers directly — always use `Dispatcher.UIThread.Post(() => { ... })`.
- **Multiple LibVLC instances:** Create exactly one `LibVLC` instance per app lifetime (in `DesktopMediaPlayerService` singleton). Multiple instances cause native resource conflicts.
- **Forgetting MediaPlayer.Dispose():** The `VideoView` does NOT dispose its `MediaPlayer`. The detail ViewModel must implement `IDisposable` and be disposed when the page is popped.
- **Binding ThumbnailData byte[] directly:** Avalonia `Image.Source` cannot bind raw `byte[]`. Always go through a value converter that produces `IImage`.
- **ICollectionView for filtering:** Avalonia does not have a built-in `CollectionViewSource`. Rebuild `DisplayItems` collection on filter change instead.
- **Using `[AvaloniaFact]` for ViewModel tests:** RecordingsViewModel and RecordingDetailViewModel have no Avalonia dependency; use plain `[Fact]`.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Video playback | Custom FFmpeg pipeline | `LibVLCSharp` + `VideoView` | Codec support, hardware decoding, subtitle rendering all included |
| Frame stepping | Custom seek-by-frame math | `MediaPlayer.NextFrame()` | LibVLC handles frame-accurate stepping; manual math is error-prone with variable frame rates |
| Mock fake data | Manual hardcoded Recording arrays | `Bogus` `Faker<Recording>` | Bogus generates deterministic-seeded randomized data; already a project dependency |
| Adaptive layout | Manual Width/Height checks in code-behind | `AdaptiveBehavior` + `AdaptiveClassSetter` | Already used in AppShellView; same pattern for dual-player portrait/landscape breakpoint |
| Archive progress | Manual byte-count tracking | `IProgress<double>` + `Stream.CopyToAsync` pattern | `IProgress<T>` is the standard .NET pattern; thread-safe by design |

**Key insight:** LibVLC handles all the hard parts of video playback (codec negotiation, hardware decode, seek accuracy). The only custom code needed is: wiring the `MediaPlayer` to the `VideoView`, and marshalling the position-changed events to the UI thread for the seek bar.

---

## Common Pitfalls

### Pitfall 1: LibVLCSharp event handlers block UI thread

**What goes wrong:** `MediaPlayer.TimeChanged` fires 25+ times per second on a native callback thread. Binding to an `ObservableProperty` from that thread crashes with a cross-thread dispatcher exception, or silently drops updates.

**Why it happens:** LibVLC's native event system uses non-UI threads by design; there is no automatic marshalling.

**How to avoid:**
```csharp
_mediaPlayer.TimeChanged += (_, e) =>
    Dispatcher.UIThread.Post(() => CurrentTimeMs = e.Time);
```

**Warning signs:** App crash or frozen seek bar during video playback.

### Pitfall 2: NavigationService stub is not yet implemented

**What goes wrong:** `RecordingsViewModel` calls `_navigationService.PushAsync(detailVm)` — which currently does nothing (`Task.CompletedTask`). Detail page never appears.

**Why it happens:** NavigationService was stubbed in Phase 1 because no navigation was needed until Phase 3.

**How to avoid:** Fully implement `NavigationService` in the first plan of Phase 3. The shell must set the active `NavigationPage` reference after it initializes.

**Warning signs:** Tapping a recording card silently does nothing.

### Pitfall 3: LibVLCSharp.Avalonia 3.9.6 NuGet compatibility with Avalonia 12 RC1

**What goes wrong:** Package restore fails because `LibVLCSharp.Avalonia` 3.9.6 declares `Avalonia >= 11.0.4` as a dependency and NuGet's pre-release semver comparison may reject `12.0.0-rc1`.

**Why it happens:** Pre-release version identifiers (`-rc1`) are lower than release in semver. NuGet may evaluate `12.0.0-rc1` as less than the *next* major version anchor differently on different NuGet client versions.

**How to avoid:** Test package restore in Wave 1. If it fails, immediately switch to `LibVLCSharp.Avalonia.Unofficial` 3.9.6.1 (jpmikkers). Same `VideoView` API; no code changes needed.

**Warning signs:** `error NU1107` or version conflict on `dotnet restore`.

### Pitfall 4: VideoView inside a UserControl / Panel loses its surface

**What goes wrong:** `VideoView` (a `NativeControlHost` subclass) renders to a native OS surface. When hosted inside certain container layouts in older Avalonia versions, the native surface is not created. Avalonia 12 RC1 specifically fixed NativeControlHost pixel rounding issues, so this should be less of a problem, but the `VideoView` should be sized explicitly and not placed inside a `ScrollViewer`.

**How to avoid:** Give `VideoView` an explicit size or stretch it inside a `Grid`. Avoid wrapping it in a `ScrollViewer`. Test on the actual target platform.

**Warning signs:** Black VideoView rectangle, no video despite MediaPlayer events firing.

### Pitfall 5: MockDashcamDevice.DownloadFileAsync returns empty stream for archive testing

**What goes wrong:** The existing `DownloadFileAsync` returns `new MemoryStream(new byte[1024])` — a 1 KB empty stream. The archive service will write a 1 KB dummy file. This is fine for testing the archive flow but the test MP4 asset (D-09) must be returned for playback testing.

**How to avoid:** Add a condition in `MockDashcamDevice.DownloadFileAsync` — if the requested path matches the embedded test MP4 resource path, return the embedded resource stream; otherwise return the 1 KB stub.

### Pitfall 6: Disposing ViewModels that hold MediaPlayer

**What goes wrong:** `RecordingDetailViewModel` creates a `MediaPlayer` in its constructor. If the ViewModel is not disposed when the page is popped (e.g., because DI created it as `Transient`), the native VLC thread keeps running, eventually causing memory leaks or crash-on-app-exit.

**How to avoid:** The ViewModel must implement `IDisposable`. The `RecordingDetailPage` code-behind must call `((IDisposable)DataContext).Dispose()` when the page is navigating away. Or the `NavigationService` handles disposal after pop.

---

## Code Examples

### Embedding a small test MP4 as an embedded resource

```xml
<!-- BlackBoxBuddy.csproj -->
<ItemGroup>
    <EmbeddedResource Include="Assets\test-clip.mp4"/>
</ItemGroup>
```

```csharp
// MockDashcamDevice.cs — return real stream for test asset
public Task<Stream> DownloadFileAsync(string path, CancellationToken ct = default)
{
    if (path.EndsWith("test-clip.mp4"))
    {
        var asm = typeof(MockDashcamDevice).Assembly;
        var stream = asm.GetManifestResourceStream(
            "BlackBoxBuddy.Assets.test-clip.mp4")!;
        return Task.FromResult<Stream>(stream);
    }
    return Task.FromResult<Stream>(new MemoryStream(new byte[1024]));
}
```

### Writing a ThumbnailData byte[] to WriteableBitmap

```csharp
// BytesToBitmapConverter.cs
public class BytesToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] pixels || pixels.Length == 0)
            return null;
        // Assumes 160x90 BGRA8888 raw pixel data
        var bmp = new WriteableBitmap(
            new PixelSize(160, 90),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
        using var fb = bmp.Lock();
        Marshal.Copy(pixels, 0, fb.Address, pixels.Length);
        return bmp;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

### EventType to brush converter (matches ConnectionStateToBrushConverter pattern)

```csharp
// EventTypeToBrushConverter.cs
public class EventTypeToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is EventType t ? t switch
        {
            EventType.Radar   => new SolidColorBrush(Color.Parse("#2196F3")),
            EventType.GShock  => new SolidColorBrush(Color.Parse("#F44336")),
            EventType.Parking => new SolidColorBrush(Color.Parse("#FF9800")),
            _                 => new SolidColorBrush(Color.Parse("#4CAF50")),
        } : Brushes.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

### Registering new services in AppServices.cs

```csharp
// AppServices.cs additions
services.AddSingleton<ITripGroupingService, TripGroupingService>();
services.AddSingleton<IArchiveService, ArchiveService>();
services.AddTransient<RecordingDetailViewModel>();
// IMediaPlayerService registered in platform projects (Desktop/Android)
// Desktop: Program.cs → App.PlatformServices = s => s.AddSingleton<IMediaPlayerService, DesktopMediaPlayerService>()
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Avalonia.Xaml.Behaviors` | `Xaml.Behaviors.Avalonia` | Avalonia 12 | Deprecated; must use replacement package |
| `ProgressRing` | `ProgressBar IsIndeterminate=True` | Avalonia 12 RC1 | No ProgressRing control in Avalonia 12 RC1 |
| `TextBox.Watermark` | `TextBox.PlaceholderText` | Avalonia 12 RC1 | Watermark property deprecated |
| `ICollectionView` / `CollectionViewSource` | Rebuild `ObservableCollection` on filter change | Avalonia (no WPF equivalent) | Avalonia has no CollectionViewSource; filtering done in ViewModel |

**Deprecated/outdated for this project:**
- `NavigationService` stub: returns `Task.CompletedTask` — must be fully implemented in Phase 3
- `IMediaPlayerService` stub: empty interface — must define play/pause/seek/rate/frame API
- `MockDashcamDevice._recordings`: `List<string>` — must become `IReadOnlyList<Recording>`

---

## Open Questions

1. **NavigationPage per tab vs. shared NavigationPage**
   - What we know: Avalonia 12 `NavigationPage` can wrap a `TabbedPage` tab or be the root. The CONTEXT.md says "NavigationPage push with back button" for detail.
   - What's unclear: Whether each `ContentPage` tab becomes a `NavigationPage` root automatically, or whether the shell must explicitly nest `RecordingsPage` inside a `NavigationPage`.
   - Recommendation: Test at implementation time. The simplest approach is wrapping `RecordingsPage`'s content in a `NavigationPage` within the tab definition in `AppShellView.axaml`.

2. **IMediaPlayerService interface on Android (Phase 3 scope)**
   - What we know: D-09 says bundle a real MP4 for desktop. Android uses ExoPlayer. Phase 3 context does not explicitly exclude Android playback.
   - What's unclear: Whether the Android `IMediaPlayerService` implementation is in scope for Phase 3 or deferred.
   - Recommendation: Implement `IMediaPlayerService` on desktop only in Phase 3. Stub the Android registration to throw `PlatformNotSupportedException`. Phase 4 (live feed) can flesh out the Android player.

3. **Seek bar binding direction (two-way vs. one-way)**
   - What we know: `MediaPlayer.Position` is a float 0.0–1.0; `TimeChanged` fires on a background thread.
   - What's unclear: Whether two-way binding on a `Slider` causes a feedback loop (user drags → sets Position → fires TimeChanged → updates Position → moves slider).
   - Recommendation: Use one-way binding from ViewModel to Slider, and a separate `PointerReleased` command to seek. This is the standard LibVLCSharp WPF pattern and transfers directly to Avalonia.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 10 SDK | All compilation | Assumed yes (Phase 1+2 built) | net10.0 | — |
| LibVLCSharp (NuGet) | Desktop video playback | To be added | 3.9.6 | Unofficial fork if version conflict |
| libvlc native (Windows) | VideoLAN.LibVLC.Windows | Bundled via NuGet | 3.0.23 | — |
| libvlc native (Linux) | Desktop playback on Linux | Not bundled | OS-level | `apt install libvlc-dev` |

**Missing dependencies with no fallback:** None for Windows desktop development.

**Missing dependencies with fallback:** `LibVLCSharp.Avalonia` 3.9.6 → fallback is `LibVLCSharp.Avalonia.Unofficial` 3.9.6.1.

---

## Sources

### Primary (HIGH confidence)
- Existing codebase (`AppServices.cs`, `NavigationService.cs`, `ViewLocator.cs`, `MockDashcamDevice.cs`) — verified by direct read
- `Directory.Packages.props` — all pinned versions confirmed by direct read
- LibVLCSharp best practices: https://docs.videolan.me/libvlcsharp/docs/best_practices.html — singleton LibVLC, explicit Dispose
- LibVLCSharp API (DeepWiki): https://deepwiki.com/videolan/libvlcsharp/3-core-api-reference — Play, Pause, NextFrame, Time, Length, Rate confirmed
- `MediaPlayer.TakeSnapshot` signature: https://www.fuget.org/packages/LibVLCSharp/3.3.0 — `(uint num, string filePath, uint width, uint height)` confirmed
- Avalonia ContentPage / NavigationPage docs: https://docs.avaloniaui.net/controls/navigation/contentpage — Header/Icon/Navigation property confirmed

### Secondary (MEDIUM confidence)
- Monsalma LibVLCSharp Avalonia tutorial (Feb 2025): https://monsalma.net/avalonia-ui-native-video-playback-featuring-libvlcsharp-and-exoplayer/ — VideoView setup pattern, INativeMediaPlayerService interface shape
- Bogus documentation: https://github.com/bchavez/Bogus — `Faker<T>`, `RuleFor`, `Generate(n)` API confirmed
- Avalonia Data Templates docs: https://docs.avaloniaui.net/docs/basics/data/data-templates — IDataTemplate for heterogeneous lists

### Tertiary (LOW confidence)
- STATE.md blocker note: "LibVLCSharp 3.9.6 has `Avalonia >= 11.0.4` constraint — verify it accepts `12.0.0-rc1`" — flagged from prior phase research; not verified against current NuGet client behavior

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all new packages are documented in CLAUDE.md with verified NuGet sources from Phase 1 research; versions confirmed in Directory.Packages.props
- Architecture patterns: HIGH — patterns derived from existing codebase conventions (ViewLocator, converters, DI, MVVM source generators); LibVLCSharp API verified against official docs
- Pitfalls: HIGH — LibVLC threading and disposal pitfalls verified against official best practices docs; Avalonia-specific pitfalls verified against existing phase notes in STATE.md

**Research date:** 2026-03-24
**Valid until:** 2026-04-24 (Avalonia 12 is pre-release; check for RC2 before starting implementation)
