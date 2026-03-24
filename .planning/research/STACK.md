# Stack Research

**Domain:** Cross-platform dashcam management app (desktop + Android)
**Researched:** 2026-03-24
**Confidence:** MEDIUM-HIGH (core stack confirmed via official NuGet; GPS parsing from dashcam-proprietary atom formats is LOW confidence)

---

## Core Stack (Already Decided — Do Not Change)

| Technology | Version | Purpose | Notes |
|------------|---------|---------|-------|
| C# | 14 | Language | — |
| .NET | 10 | Runtime | |
| AvaloniaUI | 12.0.0-rc1 | Cross-platform UI framework | Pre-release; rc1 released 2026-03-19 |
| CommunityToolkit.Mvvm | 8.4.1 | MVVM scaffolding (ObservableObject, RelayCommand, source generators) | Already referenced |
| xunit.v3 (mtp-v2) | — | Unit test runner | Already referenced |
| Avalonia.Headless.XUnit | — | Headless UI testing with xUnit | Already referenced |

These are locked in by the project skeleton. The research below covers **what to add**.

---

## Recommended Supporting Libraries

### Video Playback

The challenge: LibVLCSharp.Avalonia's `VideoView` inherits `NativeControlHost` and works on desktop (Windows, Linux, macOS), but **does not work on Android**. Android requires ExoPlayer via a second `NativeControlHost` subclass. The pattern is a shared `INativeMediaPlayerService` interface with platform-specific implementations injected at startup.

| Library | Version | Platform | Purpose | Why |
|---------|---------|----------|---------|-----|
| `LibVLCSharp` | 3.9.6 | Desktop | Core VLC media engine bindings | Mature, widely used, supports .NET 10; handles all codecs dashcams produce (H.264/H.265 in MP4/MOV) |
| `LibVLCSharp.Avalonia` | 3.9.6 | Desktop | `VideoView` control (`NativeControlHost` subclass) | Official Avalonia integration; no extra wiring needed |
| `VideoLAN.LibVLC.Windows` | 3.0.23 | Windows | Native libvlc DLLs bundled for Windows | Required so users do not need VLC installed |
| `Xamarin.AndroidX.Media3.ExoPlayer` | 1.9.2.1 | Android | ExoPlayer engine | Android's first-party media player; no licensing issues; targets net10.0-android |
| `Xamarin.AndroidX.Media3.UI` | 1.9.2.1 | Android | `PlayerView` native control | Provides the surface for ExoPlayer output |

**Architecture for video playback:**

```
BlackBoxBuddy (shared) — defines IMediaPlayerControl (Avalonia Control)
BlackBoxBuddy.Desktop  — LibVLCSharp VideoView implementation
BlackBoxBuddy.Android  — ExoPlayer NativeControlHost implementation
```

The shared project uses `ContentControl` bound to a platform-injected `IMediaPlayerControl`. Platform entry points (`Program.cs` / `MainActivity.cs`) register the concrete implementation via DI before the app starts.

**Confidence:** MEDIUM — pattern confirmed by a February 2025 tutorial and official NuGet release dates. Avalonia 12 RC1 fixed pixel rounding in `NativeControlHost` (see RC1 release notes), which may resolve historical VideoView-in-UserControl issues. Verify at implementation time.

---

### GPS / Telemetry Extraction from Dashcam Video Files

Dashcam GPS is **not standard EXIF**. It is embedded as vendor-proprietary MP4 atoms. There are two common formats used by dashcam chipsets:

| Chipset | Format | MP4 Atom |
|---------|--------|----------|
| Novatek (VIOFO, many budget cams) | Custom binary rows in `free` atoms; lookup table in `gps ` sub-atom of `moov` | Non-standard |
| BlackVue | Proprietary track | Non-standard |
| GoPro / newer standards | CAMM (Camera Motion Metadata) | `camm` track in `moov` |

**No single .NET library parses all three.** The approach must be layered:

| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `MetadataExtractor` | 2.9.2 | Extract standard MP4/MOV metadata (duration, creation date, codec) | Well-maintained; supports MP4 natively; reads standard QuickTime metadata atoms; **does not parse vendor GPS atoms** |
| Custom `IBinaryParser` per vendor | N/A (build in-house) | Parse Novatek `gps ` atom, BlackVue GPS track, CAMM track | No general-purpose .NET library covers vendor GPS — must implement `BinaryReader`-based parsers in the vendor abstraction layer |

**Why MetadataExtractor for non-GPS metadata?** It gives duration, timestamps, codec info, and thumbnail extraction with minimal code. Use it for everything except GPS.

**For GPS,** implement `IGpsTrackExtractor` in the vendor abstraction layer. Extract raw atom bytes via `System.IO.FileStream` + `BinaryReader`, walking the MP4 box hierarchy. The Novatek format is well-documented in community reverse-engineering threads. This is intentionally deferred to the milestone that implements real device communication (currently out of scope — mock device is used first).

**Confidence (GPS extraction):** LOW — no battle-tested .NET library found for vendor GPS atoms. The parsing strategy is sound (community references confirm the format), but implementation complexity is unknown until the real device is in hand.

**Confidence (MetadataExtractor for standard metadata):** HIGH — official NuGet page confirms MP4/MOV support; version 2.9.2 targets .NET Standard 2.0 (compatible with net10.0).

---

### Responsive / Adaptive Layout

Avalonia 12 does not have CSS-style media queries. Responsiveness is achieved through `AdaptiveBehavior` from the behaviors library.

| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `Xaml.Behaviors.Avalonia` | 12.0.0-rc1 (pre-release) | `AdaptiveBehavior`, `AdaptiveClassSetter` — apply CSS classes based on control width/height thresholds | The official `Avalonia.Xaml.Behaviors` package is **deprecated**; `Xaml.Behaviors.Avalonia` is the replacement; rc1 aligns with Avalonia 12 RC1 |

**Usage pattern:** Attach `AdaptiveBehavior` to a root panel. Define `AdaptiveClassSetter` entries with `MinWidth`/`MaxWidth` breakpoints. Apply `Styles` triggered by those pseudo-classes to switch between portrait (vertical nav rail) and landscape (horizontal nav bar) layouts. This is the standard Avalonia pattern for responsive desktop+mobile.

**Confidence:** MEDIUM — `AdaptiveBehavior` confirmed in official Avalonia.Xaml.Behaviors source; rc1 package for Avalonia 12 confirmed on NuGet (2026-03-19).

---

### Dependency Injection

| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `Microsoft.Extensions.DependencyInjection` | 10.x (bundled with .NET 10) | Service registration and resolution | Already part of .NET SDK; no extra NuGet needed; integrates with `IServiceCollection` pattern CommunityToolkit.Mvvm's `Ioc.Default` uses |

Use `CommunityToolkit.Mvvm`'s `Ioc.Default` (`Microsoft.Extensions.DependencyInjection` under the hood) as the service locator for ViewModels. Register platform-specific services (e.g., `IMediaPlayerControl`) in each platform entry point before `BuildAvaloniaApp()` runs.

**Confidence:** HIGH — part of .NET SDK.

---

### Testing

| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `NSubstitute` | 5.3.0 (stable) or 6.0.0-rc.1 (pre-release) | Mocking interfaces in ViewModel unit tests | Cleaner, more readable syntax than Moq; no SponsorLink controversy; works with xunit.v3 |
| `FluentAssertions` | 8.9.0 | Assertion library | BDD-style `.Should().Be()` assertions; improves test readability; supports xUnit |
| `Bogus` | 35.6.5 | Fake data generation for test builders | Generate realistic recording metadata, GPS coordinates, timestamps in tests |

**ViewModel testing strategy (no UI thread needed):**
ViewModels have no Avalonia dependency. Use plain `[Fact]` / `[Theory]` xunit.v3 tests. Inject mock `IDeviceService`, `IRecordingRepository`, etc. via constructor. No `[AvaloniaFact]` needed.

**UI / control testing strategy:**
Use `[AvaloniaFact]` from `Avalonia.Headless.XUnit`. The `[AvaloniaTestApplication]` assembly attribute wires to `App.axaml.cs`. Use `window.KeyTextInput()`, `window.MouseDown()` / `window.MouseUp()` for interaction simulation. Run without a display (CI-safe).

**Confidence:** HIGH — all packages confirmed on NuGet; pattern confirmed in official Avalonia headless testing docs.

---

## Installation

```xml
<!-- BlackBoxBuddy.csproj — shared project additions -->
<PackageReference Include="MetadataExtractor" Version="2.9.2" />
<PackageReference Include="Xaml.Behaviors.Avalonia" Version="12.0.0-rc1" />

<!-- BlackBoxBuddy.Desktop.csproj additions -->
<PackageReference Include="LibVLCSharp" Version="3.9.6" />
<PackageReference Include="LibVLCSharp.Avalonia" Version="3.9.6" />
<PackageReference Include="VideoLAN.LibVLC.Windows" Version="3.0.23"
                  Condition="$([MSBuild]::IsOSPlatform('Windows'))" />

<!-- BlackBoxBuddy.Android.csproj additions -->
<PackageReference Include="Xamarin.AndroidX.Media3.ExoPlayer" Version="1.9.2.1" />
<PackageReference Include="Xamarin.AndroidX.Media3.UI" Version="1.9.2.1" />

<!-- BlackBoxBuddy.Tests.csproj additions -->
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="FluentAssertions" Version="8.9.0" />
<PackageReference Include="Bogus" Version="35.6.5" />
```

Note: `LibVLCSharp.Avalonia` requires Avalonia >= 11.0.4. Verify compatibility with 12.0.0-rc1 when adding the package — the dependency constraint may need a float (`Version="3.9.*"`) if NuGet rejects the transitive version.

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Desktop video | LibVLCSharp.Avalonia | FFMediaToolkit (FFmpeg.AutoGen) | FFMediaToolkit is a decoder-focused library; lacks a ready-made Avalonia control; requires shipping FFmpeg binaries separately; more plumbing for the same result |
| Desktop video | LibVLCSharp.Avalonia | Vlc.DotNet | Vlc.DotNet is Windows-only (WinForms/WPF); not Avalonia-compatible |
| Android video | ExoPlayer (Media3) | LibVLCSharp on Android | LibVLCSharp VideoView explicitly does not work on Android; ExoPlayer is Google's official recommendation |
| Metadata | MetadataExtractor | TagLib# | TagLib# reads audio/video tags (title, artist, album) but has weaker support for MP4 container structure inspection; MetadataExtractor better for technical video metadata |
| Metadata | MetadataExtractor | ExifTool (CLI) | Would require shell process invocation; cross-platform path resolution fragile; not suitable for in-process use |
| Behaviors | Xaml.Behaviors.Avalonia | Avalonia.Xaml.Behaviors | Deprecated — the NuGet page for `Avalonia.Xaml.Behaviors` now recommends migrating to `Xaml.Behaviors.Avalonia` |
| Mocking | NSubstitute | Moq | Moq added SponsorLink telemetry in 4.20 (later removed but trust damaged); NSubstitute syntax is cleaner for interface-heavy code |
| Assertions | FluentAssertions | Shouldly | FluentAssertions has broader feature coverage and wider community adoption; either is acceptable |

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| `Vlc.DotNet` | Windows-only; WinForms/WPF only; no Avalonia integration | `LibVLCSharp.Avalonia` |
| `Avalonia.Xaml.Behaviors` | Deprecated; NuGet page redirects to replacement | `Xaml.Behaviors.Avalonia` |
| `ReactiveUI` | Default Avalonia template includes it but adds significant complexity; not needed when CommunityToolkit.Mvvm is already the MVVM layer | `CommunityToolkit.Mvvm` (already in stack) |
| `ExifTool` (CLI) | External process invocation; unreliable on Android; packaging complexity | `MetadataExtractor` + custom binary parser |
| `FFmpeg.AutoGen` (direct) | Raw P/Invoke bindings; no high-level API; requires expertise; overkill for metadata reads | `MetadataExtractor` for metadata; `LibVLCSharp` for playback |
| `MediaInfo.NET` or `MediaInfoWrapper` | Does not expose GPS metadata at all (confirmed by community reports) | `MetadataExtractor` + custom vendor parser |

---

## Stack Patterns by Variant

**For ViewModel unit tests (no UI):**
- Use plain `[Fact]` — no `[AvaloniaFact]` needed
- ViewModels must not import any Avalonia namespaces directly
- Inject all Avalonia-dependent services behind interfaces

**For UI / control integration tests:**
- Use `[AvaloniaFact]` from `Avalonia.Headless.XUnit`
- Declare `[assembly: AvaloniaTestApplication(typeof(TestApp))]` once per test project
- `TestApp` should be a minimal `Application` subclass with `Avalonia.Themes.Fluent.FluentTheme`

**For GPS parsing (when real device arrives):**
- Implement `IGpsTrackExtractor : IVendorParser<GpsTrack>` per chipset
- Walk MP4 box tree with `BinaryReader` over a `FileStream` — do NOT load entire file to memory
- Cache parsed tracks; recordings are short (seconds) so full in-memory GPS arrays are fine after parsing

**For responsive layout (desktop + mobile):**
- Attach `AdaptiveBehavior` to the root `ContentPage` or layout panel
- Breakpoint at 600px width: below = mobile/portrait (bottom tab bar), above = desktop/landscape (left nav rail)
- Use `AdaptiveClassSetter` to toggle style classes; define matching `Style` blocks in XAML

---

## Version Compatibility

| Package | Compatible With | Notes |
|---------|-----------------|-------|
| `LibVLCSharp.Avalonia` 3.9.6 | Avalonia >= 11.0.4 | Dependency constraint states `>= 11.0.4`; Avalonia 12 RC1 is `12.0.0-rc1` — should satisfy but verify at package restore time. Avalonia 12 RC1 fixed NativeControlHost pixel rounding which may resolve historic VideoView-in-UserControl issues |
| `Xaml.Behaviors.Avalonia` 12.0.0-rc1 | Avalonia 12 RC1 | Version-aligned with Avalonia 12; use pre-release flag in NuGet restore |
| `MetadataExtractor` 2.9.2 | netstandard2.0, net8.0, net10.0 | Compatible with `net10.0` and `net10.0-android` — no issues |
| `NSubstitute` 5.3.0 | .NET Standard 2.0+ | Stable; 6.0.0-rc.1 is available but wait for stable release |
| `Xamarin.AndroidX.Media3.ExoPlayer` 1.9.2.1 | net9.0-android, net10.0-android | Confirmed net10.0-android support |
| `CommunityToolkit.Mvvm` 8.4.1 | .NET 8+, .NET Standard 2.0 | Compatible with net10.0 |

---

## Open Questions (Flag for Later Research)

1. **LibVLCSharp + Avalonia 12 RC1:** The `VideoView` dependency constraint is `Avalonia >= 11.0.4`. NuGet should accept 12.0.0-rc1, but test at first package restore. If there are rendering regressions, `LibVLCSharp.Avalonia.Unofficial` (jpmikkers) is the community fallback.

2. **Linux video:** `VideoLAN.LibVLC.Windows` only bundles Windows DLLs. Linux desktop requires libvlc installed at the OS level (e.g., `apt install libvlc-dev`). Consider `VideoLAN.LibVLC.Linux` if bundling is needed.

3. **GPS atom format for the real device:** Not yet identified (mock device used for now). When real hardware arrives, capture an MP4 file and run `exiftool -all -u` to identify the chipset/format before implementing the parser.

4. **Thumbnail extraction:** `MetadataExtractor` does not extract video frames. If thumbnail generation is needed (for recording list), either use LibVLCSharp's snapshot API (`MediaPlayer.TakeSnapshot()`) or FFMediaToolkit. Decision deferred until the recording list milestone.

---

## Sources

- [NuGet: LibVLCSharp.Avalonia 3.9.6](https://www.nuget.org/packages/LibVLCSharp.Avalonia) — version and Avalonia compatibility confirmed (HIGH)
- [NuGet: LibVLCSharp 3.9.6](https://www.nuget.org/packages/LibVLCSharp) — .NET 10 support confirmed (HIGH)
- [NuGet: VideoLAN.LibVLC.Windows 3.0.23](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows) — latest stable (HIGH)
- [NuGet: VideoLAN.LibVLC.Android 3.6.5](https://www.nuget.org/packages/VideoLAN.LibVLC.Android) — not used directly; ExoPlayer preferred for Android (HIGH)
- [Monsalma: Avalonia UI native video playback (Feb 2025)](https://monsalma.net/avalonia-ui-native-video-playback-featuring-libvlcsharp-and-exoplayer/) — LibVLCSharp + ExoPlayer pattern (MEDIUM)
- [NuGet: Xamarin.AndroidX.Media3.ExoPlayer 1.9.2.1](https://www.nuget.org/packages/Xamarin.AndroidX.Media3.ExoPlayer) — net10.0-android confirmed (HIGH)
- [NuGet: MetadataExtractor 2.9.2](https://www.nuget.org/packages/MetadataExtractor/) — MP4/MOV support confirmed (HIGH)
- [NuGet: Xaml.Behaviors.Avalonia 12.0.0-rc1](https://www.nuget.org/packages/Xaml.Behaviors.Avalonia) — Avalonia 12 aligned pre-release (MEDIUM)
- [Avalonia Docs: ContentPage](https://docs.avaloniaui.net/controls/navigation/contentpage) — page navigation system confirmed (HIGH)
- [Avalonia Docs: Headless XUnit](https://docs.avaloniaui.net/docs/concepts/headless/headless-xunit) — [AvaloniaFact] and [AvaloniaTestApplication] confirmed (HIGH)
- [GitHub: Avalonia Releases](https://github.com/AvaloniaUI/Avalonia/releases) — 12.0.0-rc1 released 2026-03-19 (HIGH)
- [NuGet: NSubstitute 5.3.0](https://www.nuget.org/packages/NSubstitute) — stable version confirmed (HIGH)
- [NuGet: FluentAssertions 8.9.0](https://www.nuget.org/packages/fluentassertions/) — latest stable confirmed (HIGH)
- [NuGet: Bogus 35.6.5](https://www.nuget.org/packages/bogus/) — latest stable confirmed (HIGH)
- [NuGet: CommunityToolkit.Mvvm 8.4.1](https://www.nuget.org/packages/CommunityToolkit.Mvvm) — latest stable confirmed (HIGH)
- [DashCamTalk: Novatek GPS atom format](https://dashcamtalk.com/forum/threads/script-to-extract-gps-data-from-novatek-mp4.20808/) — vendor GPS atom structure (LOW — community reverse engineering)
- [Google CAMM Spec](https://developers.google.com/streetview/publish/camm-spec) — CAMM track format (MEDIUM — official spec but niche)

---

*Stack research for: BlackBoxBuddy — cross-platform dashcam management app*
*Researched: 2026-03-24*
