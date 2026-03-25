# Phase 4: Live Feed and Dashboard - Research

**Researched:** 2026-03-25
**Domain:** AvaloniaUI 12 RC1 live stream playback (LibVLCSharp VideoView port), MVVM dashboard aggregation, cross-tab navigation
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Remove the `LibVLCSharp.Avalonia` NuGet package — it is incompatible with Avalonia 12 RC1
- **D-02:** Inline the VideoView.cs source file from LibVLCSharp 3.x and make it compatible with Avalonia 12. Source: `https://code.videolan.org/videolan/LibVLCSharp/-/raw/3.x/src/LibVLCSharp.Avalonia/VideoView.cs?ref_type=heads`
- **D-03:** This fix unblocks desktop video playback for both live feed AND recording detail (Phase 3 VideoView integration)
- **D-04:** Minimal controls only — camera toggle (front/rear). No seek, speed, frame-step, or playback controls
- **D-05:** Front/rear camera toggle uses a segmented control below the video area. Matches the filter chip pattern from RecordingsPage
- **D-06:** No video overlay — keep the live stream clean and distraction-free
- **D-07:** Connection loss replaces the video area with a full-screen placeholder state ("Connection lost" with retry button). Segmented control remains visible
- **D-08:** Scrollable page with 3 distinct sections: Recent Recordings, Recent Trips, Recent Events
- **D-09:** Each section shows 3-5 items as compact cards (thumbnail + date/time + duration + event type badge)
- **D-10:** Section headers include a "See All" action link
- **D-11:** Tapping a dashboard item navigates directly to RecordingDetailPage
- **D-12:** "See All" switches to the Recordings tab. For Events section, auto-applies the relevant event type filter

### Claude's Discretion

- Exact segmented control styling and placement
- Compact card dimensions and responsive breakpoints for dashboard
- "See All" link styling and placement within section headers
- Connection loss placeholder design (icon, text, retry button style)
- How many items per dashboard section (3-5 range)
- Loading states for dashboard and live feed
- Error handling for stream connection failures
- VideoView.cs Avalonia 12 migration approach (API changes to address)

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| LIVE-01 | User can view live video feed from front camera | `IDeviceLiveStream.GetStreamUriAsync("front")` returns RTSP URI; `IMediaPlayerService.Play` with that URI on a VideoView player handle |
| LIVE-02 | User can view live video feed from rear camera | Same pattern with `cameraId = "rear"` |
| LIVE-03 | User can toggle between front and rear camera feeds without leaving the screen | `SelectedCamera` observable property; `ToggleCameraCommand` stops current player, requests new URI, starts new player — stream restart pattern |
| DASH-01 | Dashboard shows recent recordings | `IDeviceFileSystem.ListRecordingsAsync` → take top-N standalone recordings; bind to compact card list |
| DASH-02 | Dashboard shows recent trips | `ITripGroupingService.Group` result → filter TripGroup items → take top-N |
| DASH-03 | Dashboard shows recent events | Filter recordings where `EventType != None` → take top-N; existing `EventTypeToBrushConverter` for badges |
</phase_requirements>

---

## Summary

Phase 4 has two independent deliverables: (1) live stream playback on the LiveFeedPage and (2) a dashboard summary on DashboardPage. Both pages already exist as stub ContentPages; both ViewModels are registered stubs. The bulk of new work is ViewModel and XAML, with one critical infrastructure task: porting VideoView.cs from LibVLCSharp 3.x to Avalonia 12.

The VideoView.cs port is a prerequisite for live stream playback. The source code has been fully retrieved and analysed. The key Avalonia 12 breaking change affecting VideoView is that `VisualRoot is not Window` is no longer a reliable pattern — `VisualRoot` is now a general `Visual` (not necessarily a `Window`). The fix is to replace `VisualRoot is not Window visualRoot` guards with `TopLevel.GetTopLevel(this) as Window` calls. The `CreateNativeControlCore` / `DestroyNativeControlCore` signatures are unchanged.

The dashboard requires no new services — it reuses `IDeviceFileSystem.ListRecordingsAsync`, `ITripGroupingService.Group`, and `INavigationService`. Cross-tab "See All" communication is mediated by `AppShellViewModel` using the same Action callback pattern established in Phase 1 for `ManualConnectionViewModel`.

**Primary recommendation:** Port VideoView first (Wave 0), then implement LiveFeedViewModel + LiveFeedPage, then implement DashboardViewModel + DashboardPage with cross-tab communication last.

---

## Standard Stack

### Core (no new packages — all already present)
| Library | Version | Purpose | Notes |
|---------|---------|---------|-------|
| AvaloniaUI | 12.0.0-rc1 | UI framework | Already in project |
| LibVLCSharp | 3.9.6 | VLC media engine (RTSP playback) | Already in Desktop project; VideoView.cs inlined from this source |
| CommunityToolkit.Mvvm | 8.4.1 | MVVM source generators | Already in project |
| NSubstitute | 5.3.0 | Mocking for tests | Already in test project |
| FluentAssertions | 8.9.0 | BDD assertions | Already in test project |

### No New NuGet Packages Required
Per D-01, `LibVLCSharp.Avalonia` NuGet package is removed. The VideoView.cs source file is inlined directly into the Desktop project. No other new packages are needed for this phase.

**Important:** `LibVLCSharp.Avalonia` must be removed from `BlackBoxBuddy.csproj` (the shared project added it in Phase 3 for XAML resolution). After inlining VideoView.cs into `BlackBoxBuddy.Desktop`, the shared project reference must also be removed.

---

## Architecture Patterns

### Recommended Project Structure for Phase 4
```
src/BlackBoxBuddy.Desktop/
├── Controls/
│   └── VideoView.cs          ← inlined + ported from LibVLCSharp 3.x
├── Services/
│   └── DesktopMediaPlayerService.cs  ← unchanged
src/BlackBoxBuddy/
├── ViewModels/
│   ├── LiveFeedViewModel.cs   ← fully implement
│   └── DashboardViewModel.cs  ← fully implement
├── Views/
│   ├── LiveFeedPage.axaml     ← fully implement (+ .axaml.cs)
│   └── DashboardPage.axaml    ← fully implement (+ .axaml.cs)
tests/BlackBoxBuddy.Tests/
└── ViewModels/
    ├── LiveFeedViewModelTests.cs   ← new
    └── DashboardViewModelTests.cs  ← new
```

### Pattern 1: VideoView.cs Avalonia 12 Port

**What:** The existing VideoView.cs uses `VisualRoot is not Window visualRoot` guards in `InitializeNativeOverlay`, `ShowNativeOverlay`, and `Parent_DetachedFromVisualTree`. In Avalonia 12, `VisualRoot` is no longer guaranteed to be a `Window` — use `TopLevel.GetTopLevel(this) as Window` instead.

**Key methods needing attention:**

```csharp
// BEFORE (Avalonia 11 — VisualRoot pattern):
private void InitializeNativeOverlay()
{
    if (!this.IsAttachedToVisualTree()) return;
    if (VisualRoot is not Window visualRoot) return;   // ← BREAKS in Avalonia 12
    // ...
    visualRoot.LayoutUpdated += VisualRoot_UpdateOverlayPosition;
}

// AFTER (Avalonia 12 — TopLevel.GetTopLevel pattern):
private void InitializeNativeOverlay()
{
    if (!this.IsAttachedToVisualTree()) return;
    if (TopLevel.GetTopLevel(this) is not Window visualRoot) return;   // ← FIXED
    // ...
    visualRoot.LayoutUpdated += VisualRoot_UpdateOverlayPosition;
}
```

Apply the same substitution to `ShowNativeOverlay` and `Parent_DetachedFromVisualTree`. The `GetVisualParent()` extension method and `OnAttachedToVisualTree`/`OnDetachedFromVisualTree` overrides remain valid in Avalonia 12.

**CreateNativeControlCore / DestroyNativeControlCore signatures are unchanged** — no migration needed for those methods.

**File location:** `src/BlackBoxBuddy.Desktop/Controls/VideoView.cs`
**Namespace:** `BlackBoxBuddy.Desktop.Controls` (not `LibVLCSharp.Avalonia`)

Since D-06 specifies no overlay content on live feed, the floating content window mechanism is not exercised during live stream. The port is still needed for the VideoView to function as a NativeControlHost surface.

### Pattern 2: LiveFeedViewModel Lifecycle

**What:** Live feed is simpler than recording playback — no seek or frame-step. The lifecycle is: tab appear → start stream, tab disappear → stop stream.

```csharp
// Source: derived from RecordingDetailViewModel (already in codebase)
public partial class LiveFeedViewModel : ViewModelBase, IDisposable
{
    private readonly IDashcamDevice _device;
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IDeviceService _deviceService;
    private object? _player;
    private CancellationTokenSource? _streamCts;

    [ObservableProperty] private string _selectedCamera = "front";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isConnectionLost;
    [ObservableProperty] private bool _isStreamActive;

    public object? Player => _player;

    // Called from View code-behind on page appear (tab navigation in)
    [RelayCommand]
    private async Task StartLiveFeedAsync()
    {
        _streamCts = new CancellationTokenSource();
        IsLoading = true;
        IsConnectionLost = false;
        try
        {
            var uri = await _device.GetStreamUriAsync(SelectedCamera, _streamCts.Token);
            if (uri is null) { IsConnectionLost = true; return; }
            if (_player is null) _player = _mediaPlayerService.CreatePlayer();
            _mediaPlayerService.Play(_player, uri);
            IsStreamActive = true;
        }
        catch (OperationCanceledException) { /* tab switched away */ }
        catch { IsConnectionLost = true; }
        finally { IsLoading = false; }
    }

    // Called from View code-behind on page disappear (tab navigation away)
    [RelayCommand]
    private void StopLiveFeed()
    {
        _streamCts?.Cancel();
        if (_player is not null) _mediaPlayerService.Stop(_player);
        IsStreamActive = false;
        IsLoading = false;
    }

    [RelayCommand]
    private async Task ToggleCameraAsync(string cameraId)
    {
        SelectedCamera = cameraId;   // optimistic UI
        StopLiveFeed();
        await StartLiveFeedAsync();
    }

    [RelayCommand]
    private async Task RetryAsync() => await StartLiveFeedAsync();

    public void Dispose()
    {
        StopLiveFeed();
        if (_player is not null) _mediaPlayerService.DisposePlayer(_player);
    }
}
```

**Tab lifecycle hooks:** Avalonia `ContentPage` does not have built-in OnAppearing/OnDisappearing. Use `TabbedPage.SelectionChanged` event from `AppShellViewModel`, or attach to the `IsVisible` changed event in the View code-behind. The simplest approach: override `OnAttachedToVisualTree` and `OnDetachedFromVisualTree` in `LiveFeedPage.axaml.cs` to call `ViewModel.StartLiveFeedCommand` and `ViewModel.StopLiveFeedCommand`.

### Pattern 3: DashboardViewModel Data Aggregation

**What:** Dashboard pulls the same recording data used by RecordingsViewModel, applies top-N slicing, and exposes three separate collections. Data is loaded once per session (`IsDashboardLoaded` guard).

```csharp
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;
    private readonly INavigationService _navigationService;
    private readonly Action<int> _switchTab;          // callback to AppShellViewModel
    private readonly Action<EventType?> _applyFilter; // callback to RecordingsViewModel

    [ObservableProperty] private bool _isDashboardLoaded;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDeviceConnected;

    // Three section collections — ObservableCollection<object> for type-heterogeneous binding
    public ObservableCollection<object> RecentRecordings { get; } = new();
    public ObservableCollection<object> RecentTrips { get; } = new();
    public ObservableCollection<object> RecentEvents { get; } = new();

    [RelayCommand]
    private async Task LoadDashboardAsync(CancellationToken ct = default)
    {
        if (IsDashboardLoaded) return;
        IsLoading = true;
        try
        {
            var all = await _device.ListRecordingsAsync(ct);
            var grouped = _tripGroupingService.Group(all);

            // Recent Recordings: top 5 standalone Recording items
            RecentRecordings.Clear();
            foreach (var item in grouped.OfType<Recording>().Take(5))
                RecentRecordings.Add(item);

            // Recent Trips: top 5 TripGroup items
            RecentTrips.Clear();
            foreach (var item in grouped.OfType<TripGroup>().Take(5))
                RecentTrips.Add(item);

            // Recent Events: top 5 recordings with EventType != None
            RecentEvents.Clear();
            var events = all.Where(r => r.EventType != EventType.None)
                            .OrderByDescending(r => r.DateTime)
                            .Take(5);
            foreach (var item in events) RecentEvents.Add(item);

            IsDashboardLoaded = true;
        }
        catch (OperationCanceledException) { }
        finally { IsLoading = false; }
    }

    // "See All" commands — delegate tab switch + filter to AppShellViewModel callbacks
    [RelayCommand] private void SeeAllRecordings() => _switchTab(1); // Recordings tab
    [RelayCommand] private void SeeAllTrips() => _switchTab(1);
    [RelayCommand] private void SeeAllEvents() { _switchTab(1); _applyFilter(EventType.GShock); }

    // Card tap → navigate to detail (same pattern as RecordingsViewModel.OpenRecording)
    [RelayCommand] private void OpenRecording(Recording r) => /* push detail + switch tab */;
}
```

### Pattern 4: Cross-Tab Communication via AppShellViewModel

**What:** `DashboardViewModel` needs to switch tabs and apply filters on RecordingsViewModel. The established Phase 1 pattern uses Action callbacks passed at construction time (not DI — same as `ManualConnectionViewModel`).

```csharp
// In AppShellViewModel constructor — wire up callbacks
DashboardVm = new DashboardViewModel(
    deviceService, tripGroupingService, navigationService,
    switchTab: idx => SelectedTabIndex = idx,
    applyFilter: filter => RecordingsVm.SetFilterCommand.Execute(filter));
```

**AppShellViewModel change:** `DashboardViewModel` can no longer be registered as a transient in DI (it needs per-instance callbacks). Change to manual construction in `AppShellViewModel`, the same way `ManualConnectionViewModel` is handled. Remove `DashboardViewModel` from `AppServices.cs` DI registration and construct it directly in `AppShellViewModel`.

Alternatively, if DI must be preserved, define `IDashboardCallbacks` interface and inject it — but the Action callback pattern is simpler and consistent with established conventions.

### Pattern 5: LiveFeedPage XAML Structure

Per UI-SPEC, the page uses a `Grid RowDefinitions="*,Auto"`:
- Row 0: Video area `Panel` (black background, VideoView + loading placeholder + connection-loss placeholder via IsVisible swap)
- Row 1: Fixed-height segmented toggle bar

```xml
<!-- Source: UI-SPEC.md + Phase 3 VideoView pattern -->
<Grid RowDefinitions="*,Auto">
  <!-- Video area -->
  <Panel Grid.Row="0" Background="#111111">
    <!-- VideoView: bound to ViewModel.Player — code-behind sets MediaPlayer handle -->
    <controls:VideoView x:Name="VideoViewControl" IsVisible="{Binding IsStreamActive}"/>
    <!-- Loading placeholder -->
    <StackPanel IsVisible="{Binding IsLoading}" ...>
      <ProgressBar IsIndeterminate="True" Width="160" Height="4"/>
      <TextBlock Text="Connecting to camera..."/>
    </StackPanel>
    <!-- Connection loss placeholder -->
    <StackPanel IsVisible="{Binding IsConnectionLost}" ...>
      <PathIcon Data="{StaticResource CameraOffGeometry}" Width="48" Height="48" Opacity="0.5"/>
      <TextBlock Text="Connection lost" FontSize="18" FontWeight="SemiBold"/>
      <TextBlock Text="Check your dashcam connection and try again." FontSize="14" Opacity="0.7"/>
      <Button Content="Retry Connection" Command="{Binding RetryCommand}"
              Padding="24,8" CornerRadius="8" HorizontalAlignment="Center"/>
    </StackPanel>
  </Panel>

  <!-- Camera toggle — outside video Panel so it survives connection loss -->
  <Border Grid.Row="1" Height="44" Margin="16,8">
    <Grid ColumnDefinitions="*,*" ...>
      <Button Content="Front" Command="{Binding ToggleCameraCommand}" CommandParameter="front" .../>
      <Button Content="Rear"  Command="{Binding ToggleCameraCommand}" CommandParameter="rear"  .../>
    </Grid>
  </Border>
</Grid>
```

**VideoView wiring in code-behind:** The VideoView control needs the LibVLC `MediaPlayer` handle set via code-behind after the control is loaded (same pattern as RecordingDetailPage). The ViewModel exposes `Player` as `object?`; the code-behind casts to `LibVLCSharp.Shared.MediaPlayer` and sets `VideoViewControl.MediaPlayer`.

### Anti-Patterns to Avoid

- **Registering DashboardViewModel in DI with callbacks:** DI cannot supply per-instance Action callbacks. Construct DashboardViewModel manually in AppShellViewModel (same as ManualConnectionViewModel).
- **Using `VisualRoot is Window`:** Broken in Avalonia 12. Always use `TopLevel.GetTopLevel(this) as Window`.
- **Loading dashboard on every tab visit:** Use `IsDashboardLoaded` guard — load once per session. Stale data is acceptable per interaction contract (D-08 and UI-SPEC).
- **Clearing dashboard on disconnect:** Per UI-SPEC interaction contract, items remain visible on disconnect — do not clear and re-show empty state.
- **Starting live stream in ViewModel constructor:** Stream must start only when page appears (tab navigation in), not when VM is constructed. Construction happens at app startup for all tabs.
- **Calling `_mediaPlayerService.Stop` without also calling `DisposePlayer` on tab-away:** Stop pauses playback; the player handle remains allocated. Only `DisposePlayer` releases GPU/decoder resources. Call Stop on tab-away, DisposePlayer only in `Dispose()`.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Video surface embedding | Custom Win32 HWND wrapper | VideoView.cs (inlined NativeControlHost subclass) | NativeControlHost already handles platform-specific handle acquisition across Win/Linux/Mac |
| RTSP stream setup | Raw socket + RTP demuxing | `IMediaPlayerService.Play(player, rtspUri)` → LibVLCSharp handles RTSP internally | LibVLC supports RTSP natively; no setup needed beyond passing the URI |
| Recording data for dashboard | Separate dashboard data service | Reuse `IDeviceFileSystem.ListRecordingsAsync` + `ITripGroupingService.Group` | Same data source used by RecordingsViewModel; no duplication |
| Event type badge rendering | New badge component | Reuse `EventTypeToBrushConverter` + visibility converter from RecordingsPage | Already defined and tested |
| Tab navigation | Custom navigation events | `Action<int> switchTab` callback injected into DashboardViewModel | Established pattern from ManualConnectionViewModel |

**Key insight:** Phase 4 is almost entirely recombination of existing infrastructure. The only genuinely new code is the VideoView.cs port and the two ViewModels with their AXAML pages.

---

## Common Pitfalls

### Pitfall 1: VideoView "VisualRoot is not Window" Guard

**What goes wrong:** `InitializeNativeOverlay` and `ShowNativeOverlay` use `VisualRoot is not Window visualRoot` to get the hosting window for the floating overlay Window. In Avalonia 12, `VisualRoot` is no longer typed as `Window` in the visual tree — the guard returns false, overlay initialization silently exits, and the VideoView renders as a black rectangle but never acquires a native handle.
**Why it happens:** Avalonia 12 restructured TopLevel so it is not necessarily at the visual tree root. The `VisualRoot` property returns `Visual`, not `Window`.
**How to avoid:** Replace all `VisualRoot is not Window visualRoot` with `TopLevel.GetTopLevel(this) is not Window visualRoot`. Apply to: `InitializeNativeOverlay`, `ShowNativeOverlay`, `Parent_DetachedFromVisualTree`.
**Warning signs:** VideoView renders but shows black; no crash, no exception.

### Pitfall 2: LibVLCSharp.Avalonia NuGet Still Referenced After Inline

**What goes wrong:** If `LibVLCSharp.Avalonia` remains in `BlackBoxBuddy.csproj` after VideoView.cs is inlined, there is a namespace collision and two VideoView types visible to XAML.
**Why it happens:** Phase 3 added `LibVLCSharp.Avalonia` to the shared project for XAML type resolution. The package must be removed when the inlined version replaces it.
**How to avoid:** Remove `LibVLCSharp.Avalonia` from all .csproj files. Add it to the `<PackageReference Remove="...">` list or simply delete the reference. Verify with `dotnet build` that the `LibVLCSharp.Avalonia` namespace is no longer imported anywhere.
**Warning signs:** Ambiguous type error for `VideoView` at build time.

### Pitfall 3: RTSP Stream Not Starting on Tab Appear

**What goes wrong:** `StartLiveFeedCommand` is executed in `OnAttachedToVisualTree`, but `OnAttachedToVisualTree` fires once at initial layout, not on every tab switch. Switching away and back does not re-trigger it.
**Why it happens:** Avalonia `ContentPage` inside `TabbedPage` is attached to the visual tree once at app startup; it is not detached/reattached on tab switches — only `IsVisible` changes.
**How to avoid:** Subscribe to `TabbedPage.SelectionChanged` in `AppShellView.axaml.cs` or handle tab change in `AppShellViewModel.OnSelectedTabIndexChanged`. Raise a public method or command on `LiveFeedViewModel` when tab index matches Live Feed (index 2). Alternatively, use `IsVisibleProperty.Changed` handler in `LiveFeedPage.axaml.cs`.
**Warning signs:** Live feed only starts on first tab visit; switching away and back shows stale/stopped stream.

### Pitfall 4: DashboardViewModel Constructed by DI — Cannot Receive Action Callbacks

**What goes wrong:** `DashboardViewModel` is currently registered as `services.AddTransient<DashboardViewModel>()`. If it requires `Action<int> switchTab` and `Action<EventType?> applyFilter` callbacks in its constructor, DI cannot supply them — it will throw `MissingMethodException` at resolution time.
**Why it happens:** DI containers cannot resolve delegate parameters.
**How to avoid:** Remove `DashboardViewModel` from `AppServices.cs`. Construct it manually inside `AppShellViewModel` (matching the `ManualConnectionViewModel` pattern). Update `AppShellViewModel` constructor to accept the dependencies needed for DashboardViewModel construction.
**Warning signs:** `InvalidOperationException: Unable to resolve service for type 'System.Action'...` at app startup.

### Pitfall 5: Live Player Handle Not Set on VideoView Before Stream Starts

**What goes wrong:** `IMediaPlayerService.Play` is called but the VideoView has not received the native `MediaPlayer` handle. VLC decodes the stream but renders to nothing.
**Why it happens:** The VideoView.MediaPlayer property must be set in code-behind (after the VideoView is loaded into the visual tree) before calling Play. The ViewModel only creates the player handle (`_player = _mediaPlayerService.CreatePlayer()`); it does not know about the VideoView control.
**How to avoid:** In `LiveFeedPage.axaml.cs`, after the VideoView is loaded (or in `InitializeComponent` + `Loaded` event), cast `ViewModel.Player` to `LibVLCSharp.Shared.MediaPlayer` and set `VideoViewControl.MediaPlayer = (MediaPlayer)ViewModel.Player`. This is identical to the RecordingDetailPage pattern established in Phase 3.
**Warning signs:** Stream connects (no IsConnectionLost shown) but video area remains black.

### Pitfall 6: Dashboard "See All" Events Using Wrong EventType Filter

**What goes wrong:** UI-SPEC specifies that "See All" for Recent Events applies `EventType.GShock` filter. Using `null` (clear filter) or `EventType.Radar` will show the wrong filtered set.
**Why it happens:** The dashboard "Recent Events" section is conceptually "highest-severity events" — the UI-SPEC resolves D-12 to GShock as the canonical filter.
**How to avoid:** In `SeeAllEventsCommand`: call `_switchTab(1)` then `_applyFilter(EventType.GShock)`. The existing `RecordingsViewModel.SetFilterCommand` accepts `EventType?`.
**Warning signs:** "See All" on Events lands on Recordings tab with no filter or wrong filter applied.

---

## Code Examples

### VideoView.cs — Complete Ported Source (Avalonia 12 compatible)

The source was retrieved from `https://github.com/videolan/libvlcsharp/blob/3.x/src/LibVLCSharp.Avalonia/VideoView.cs`. The only changes required for Avalonia 12 are:

1. Replace `VisualRoot is not Window visualRoot` with `TopLevel.GetTopLevel(this) is not Window visualRoot` in three locations
2. Change namespace from `LibVLCSharp.Avalonia` to `BlackBoxBuddy.Desktop.Controls`
3. Remove `using Avalonia.Metadata;` if `[Content]` attribute causes compile errors (verify at build time)

The `CreateNativeControlCore(IPlatformHandle parent)` and `DestroyNativeControlCore(IPlatformHandle control)` signatures are identical in Avalonia 11 and 12 — no changes needed.

```csharp
// Key change — apply in InitializeNativeOverlay, ShowNativeOverlay, Parent_DetachedFromVisualTree:

// BEFORE:
if (VisualRoot is not Window visualRoot) return;

// AFTER:
if (TopLevel.GetTopLevel(this) is not Window visualRoot) return;
```

### LiveFeedPage.axaml.cs — Player Wiring

```csharp
// Source: RecordingDetailPage pattern (established Phase 3)
using LibVLCSharp.Shared;

public partial class LiveFeedPage : ContentPage
{
    public LiveFeedPage() => InitializeComponent();

    private LiveFeedViewModel? Vm => DataContext as LiveFeedViewModel;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Wire player handle once — stream starts via tab-appear mechanism
        if (Vm?.Player is MediaPlayer mp)
            VideoViewControl.MediaPlayer = mp;
    }
}
```

### DashboardViewModel Registration Change

```csharp
// AppServices.cs — REMOVE this line:
// services.AddTransient<DashboardViewModel>();

// AppShellViewModel constructor — ADD manual construction:
DashboardVm = new DashboardViewModel(
    sp.GetRequiredService<IDashcamDevice>(),
    sp.GetRequiredService<IDeviceService>(),
    sp.GetRequiredService<ITripGroupingService>(),
    sp.GetRequiredService<INavigationService>(),
    switchTab: idx => SelectedTabIndex = idx,
    applyFilter: filter => RecordingsVm.SetFilterCommand.Execute(filter));
```

### Dashboard Section Header XAML Pattern

```xml
<!-- Source: UI-SPEC.md section header pattern -->
<Grid ColumnDefinitions="*,Auto" Margin="0,0,0,8">
  <TextBlock Text="Recent Recordings" FontSize="18" FontWeight="SemiBold"/>
  <Button Grid.Column="1"
          Content="See All"
          Command="{Binding SeeAllRecordingsCommand}"
          FontSize="14"
          Foreground="#2196F3"
          Background="Transparent"
          BorderThickness="0"
          Padding="0"
          CornerRadius="0"/>
</Grid>
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `VisualRoot is Window` guard | `TopLevel.GetTopLevel(this) as Window` | Avalonia 12 | VideoView.cs must use new pattern |
| `LibVLCSharp.Avalonia` NuGet package | Inlined VideoView.cs in Desktop project | This phase (D-01/D-02) | No NuGet dependency; full source control over the control |
| `AvaloniaLocator.Current.GetService<T>()` | Not applicable (VideoView does not use AvaloniaLocator) | Avalonia 11 → 12 | No action needed for VideoView port |

**Deprecated/outdated:**
- `LibVLCSharp.Avalonia` NuGet: incompatible with Avalonia 12 RC1 per D-01 — use inlined source instead

---

## Open Questions

1. **Tab-appear / tab-disappear lifecycle for LiveFeedPage**
   - What we know: `ContentPage` inside `TabbedPage` is not detached on tab switch — `IsVisible` changes instead
   - What's unclear: Whether `IsVisible` changed event fires reliably in Avalonia 12 RC1 for pages inside TabbedPage, or whether subscribing to `AppShellViewModel.SelectedTabIndex` is more reliable
   - Recommendation: Implement using `IsVisibleProperty.Changed` handler in `LiveFeedPage.axaml.cs` first; if unreliable, fallback to `AppShellViewModel.OnSelectedTabIndexChanged` signalling the VM directly

2. **VideoView floating content with no overlay (D-06)**
   - What we know: The VideoView.cs floating content mechanism is only activated when `Content != null`. Since D-06 specifies no overlay, this code path is never triggered
   - What's unclear: Whether the floating content window mechanism causes errors even when `Content` is null in Avalonia 12
   - Recommendation: Inline VideoView with full floating content support intact; test with `Content=null` and verify no exceptions. If issues arise, strip the floating content mechanism entirely since it is not needed for live feed or recording detail

3. **DashboardViewModel construction pattern — DI vs manual**
   - What we know: The Action callback constructor parameters cannot be satisfied by DI
   - What's unclear: Whether `AppShellViewModel` should be registered differently or if `DashboardViewModel` should use a post-construction `Initialize(Action, Action)` method to avoid the DI registration change
   - Recommendation: Use manual construction in `AppShellViewModel` matching the ManualConnectionViewModel pattern — it is the simplest and most consistent approach

---

## Environment Availability

Step 2.6: SKIPPED — this phase adds no new external tools, services, CLI utilities, or runtimes. All dependencies (`LibVLCSharp`, native VLC DLLs via `VideoLAN.LibVLC.Windows`) are already present in the project from Phase 3.

---

## Project Constraints (from CLAUDE.md)

All of the following CLAUDE.md directives apply to this phase:

| Directive | Impact on Phase 4 |
|-----------|-------------------|
| C# 14 / .NET 10 / AvaloniaUI 12 RC1 | All new code targets these versions |
| CommunityToolkit.Mvvm — `[ObservableProperty]`, `[RelayCommand]` source generators | LiveFeedViewModel and DashboardViewModel must use these patterns |
| No Avalonia namespaces in ViewModels | VideoView lives in Desktop project; VMs must not import `Avalonia.*` directly |
| xunit.v3 `[Fact]` — no `[AvaloniaFact]` for pure ViewModel tests | LiveFeedViewModel and DashboardViewModel tests use plain `[Fact]` |
| Constructor-based DI — no `Ioc.Default` in Views or ViewModels | DashboardViewModel uses Action callbacks, not service location |
| `ViewLocator` pattern-matching switch | No change needed — both VMs are already mapped |
| FluentTheme Dark variant exclusively | No light mode styling |
| `ProgressBar IsIndeterminate="True"` for loading indicators (not ProgressRing) | Loading states use ProgressBar per Phase 2 decision |
| `AdaptiveBehavior` breakpoint at 600px width | LiveFeedPage video area uses `*` height at >= 600px, fixed 300px at < 600px |
| No ReactiveUI | Use CommunityToolkit.Mvvm only |

---

## Sources

### Primary (HIGH confidence)
- VideoView.cs source retrieved from `https://github.com/videolan/libvlcsharp/blob/3.x/src/LibVLCSharp.Avalonia/VideoView.cs` — full source code analysed
- Avalonia 12 breaking changes: `https://docs.avaloniaui.net/docs/avalonia12-breaking-changes` — TopLevel.GetTopLevel(Visual) migration confirmed
- Avalonia 12 RC1 release notes: `https://github.com/AvaloniaUI/Avalonia/releases/tag/12.0.0-rc1` — NativeControlHost pixel rounding fix in PR #20786 confirmed
- Project codebase: `IDeviceLiveStream.cs`, `IMediaPlayerService.cs`, `RecordingDetailViewModel.cs`, `AppShellViewModel.cs`, `RecordingsViewModel.cs`, `MockDashcamDevice.cs`, `AppServices.cs` — all read and analysed directly

### Secondary (MEDIUM confidence)
- jpmikkers/LibVLCSharp.Avalonia.Unofficial — confirmed UserControl embedding was the primary compatibility issue addressed; overlays also fixed
- Avalonia v12 breaking changes wiki: `https://github.com/AvaloniaUI/Avalonia/wiki/v12-Breaking-Changes` — VisualRoot/TopLevel restructure pattern confirmed

### Tertiary (LOW confidence)
- Specific behaviour of `IsVisible` changes for TabbedPage child ContentPages in Avalonia 12 RC1 — not verified against official documentation; flagged as Open Question

---

## Metadata

**Confidence breakdown:**
- VideoView.cs port approach: HIGH — source code retrieved, breaking change identified, fix is mechanical
- LiveFeedViewModel pattern: HIGH — directly derived from RecordingDetailViewModel in codebase
- DashboardViewModel pattern: HIGH — directly derived from RecordingsViewModel + established Action callback pattern
- Cross-tab communication: HIGH — established ManualConnectionViewModel pattern, clearly documented in accumulated decisions
- Tab lifecycle (start/stop on appear/disappear): MEDIUM — IsVisible approach is plausible but not verified against Avalonia 12 RC1 TabbedPage behaviour

**Research date:** 2026-03-25
**Valid until:** 2026-04-25 (stable domain; Avalonia 12 RC1 is pinned in project)
