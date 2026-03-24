<!-- GSD:project-start source:PROJECT.md -->
## Project

**BlackBoxBuddy**

A cross-platform desktop (PC) and mobile (Android) application for configuring automotive dashcams and managing their recordings. Everything runs locally — no cloud services. The app auto-discovers connected dashcams, guides users through device provisioning, and provides a unified interface for settings, live feeds, and footage management.

**Core Value:** Users can effortlessly manage their dashcam footage — browse recordings, combine them into trips, and archive important moments before the dashcam overwrites them.

### Constraints

- **Tech stack**: C# 14 / .NET 10, AvaloniaUI 12 RC1, CommunityToolkit.Mvvm, xunit.v3
- **Architecture**: MVVM with DRY, KISS, SOLID, TDD, BDD, DDD principles
- **Testing**: Comprehensive test suite using xunit.v3; mock devices for unit testing
- **Multi-vendor**: Device abstractions must support future vendor additions
- **UI toolkit**: Must use Avalonia 12's new page-based navigation system
- **Design**: Impeccable plugin guidelines; Stitch for prototyping; DevTools for verification
<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->
## Technology Stack

## Core Stack (Already Decided — Do Not Change)
| Technology | Version | Purpose | Notes |
|------------|---------|---------|-------|
| C# | 14 | Language | — |
| .NET | 10 | Runtime | |
| AvaloniaUI | 12.0.0-rc1 | Cross-platform UI framework | Pre-release; rc1 released 2026-03-19 |
| CommunityToolkit.Mvvm | 8.4.1 | MVVM scaffolding (ObservableObject, RelayCommand, source generators) | Already referenced |
| xunit.v3 (mtp-v2) | — | Unit test runner | Already referenced |
| Avalonia.Headless.XUnit | — | Headless UI testing with xUnit | Already referenced |
## Recommended Supporting Libraries
### Video Playback
| Library | Version | Platform | Purpose | Why |
|---------|---------|----------|---------|-----|
| `LibVLCSharp` | 3.9.6 | Desktop | Core VLC media engine bindings | Mature, widely used, supports .NET 10; handles all codecs dashcams produce (H.264/H.265 in MP4/MOV) |
| `LibVLCSharp.Avalonia` | 3.9.6 | Desktop | `VideoView` control (`NativeControlHost` subclass) | Official Avalonia integration; no extra wiring needed |
| `VideoLAN.LibVLC.Windows` | 3.0.23 | Windows | Native libvlc DLLs bundled for Windows | Required so users do not need VLC installed |
| `Xamarin.AndroidX.Media3.ExoPlayer` | 1.9.2.1 | Android | ExoPlayer engine | Android's first-party media player; no licensing issues; targets net10.0-android |
| `Xamarin.AndroidX.Media3.UI` | 1.9.2.1 | Android | `PlayerView` native control | Provides the surface for ExoPlayer output |
### GPS / Telemetry Extraction from Dashcam Video Files
| Chipset | Format | MP4 Atom |
|---------|--------|----------|
| Novatek (VIOFO, many budget cams) | Custom binary rows in `free` atoms; lookup table in `gps ` sub-atom of `moov` | Non-standard |
| BlackVue | Proprietary track | Non-standard |
| GoPro / newer standards | CAMM (Camera Motion Metadata) | `camm` track in `moov` |
| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `MetadataExtractor` | 2.9.2 | Extract standard MP4/MOV metadata (duration, creation date, codec) | Well-maintained; supports MP4 natively; reads standard QuickTime metadata atoms; **does not parse vendor GPS atoms** |
| Custom `IBinaryParser` per vendor | N/A (build in-house) | Parse Novatek `gps ` atom, BlackVue GPS track, CAMM track | No general-purpose .NET library covers vendor GPS — must implement `BinaryReader`-based parsers in the vendor abstraction layer |
### Responsive / Adaptive Layout
| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `Xaml.Behaviors.Avalonia` | 12.0.0-rc1 (pre-release) | `AdaptiveBehavior`, `AdaptiveClassSetter` — apply CSS classes based on control width/height thresholds | The official `Avalonia.Xaml.Behaviors` package is **deprecated**; `Xaml.Behaviors.Avalonia` is the replacement; rc1 aligns with Avalonia 12 RC1 |
### Dependency Injection
| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `Microsoft.Extensions.DependencyInjection` | 10.x (bundled with .NET 10) | Service registration and resolution | Already part of .NET SDK; no extra NuGet needed; integrates with `IServiceCollection` pattern CommunityToolkit.Mvvm's `Ioc.Default` uses |
### Testing
| Library | Version | Purpose | Why |
|---------|---------|---------|-----|
| `NSubstitute` | 5.3.0 (stable) or 6.0.0-rc.1 (pre-release) | Mocking interfaces in ViewModel unit tests | Cleaner, more readable syntax than Moq; no SponsorLink controversy; works with xunit.v3 |
| `FluentAssertions` | 8.9.0 | Assertion library | BDD-style `.Should().Be()` assertions; improves test readability; supports xUnit |
| `Bogus` | 35.6.5 | Fake data generation for test builders | Generate realistic recording metadata, GPS coordinates, timestamps in tests |
## Installation
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
## What NOT to Use
| Avoid | Why | Use Instead |
|-------|-----|-------------|
| `Vlc.DotNet` | Windows-only; WinForms/WPF only; no Avalonia integration | `LibVLCSharp.Avalonia` |
| `Avalonia.Xaml.Behaviors` | Deprecated; NuGet page redirects to replacement | `Xaml.Behaviors.Avalonia` |
| `ReactiveUI` | Default Avalonia template includes it but adds significant complexity; not needed when CommunityToolkit.Mvvm is already the MVVM layer | `CommunityToolkit.Mvvm` (already in stack) |
| `ExifTool` (CLI) | External process invocation; unreliable on Android; packaging complexity | `MetadataExtractor` + custom binary parser |
| `FFmpeg.AutoGen` (direct) | Raw P/Invoke bindings; no high-level API; requires expertise; overkill for metadata reads | `MetadataExtractor` for metadata; `LibVLCSharp` for playback |
| `MediaInfo.NET` or `MediaInfoWrapper` | Does not expose GPS metadata at all (confirmed by community reports) | `MetadataExtractor` + custom vendor parser |
## Stack Patterns by Variant
- Use plain `[Fact]` — no `[AvaloniaFact]` needed
- ViewModels must not import any Avalonia namespaces directly
- Inject all Avalonia-dependent services behind interfaces
- Use `[AvaloniaFact]` from `Avalonia.Headless.XUnit`
- Declare `[assembly: AvaloniaTestApplication(typeof(TestApp))]` once per test project
- `TestApp` should be a minimal `Application` subclass with `Avalonia.Themes.Fluent.FluentTheme`
- Implement `IGpsTrackExtractor : IVendorParser<GpsTrack>` per chipset
- Walk MP4 box tree with `BinaryReader` over a `FileStream` — do NOT load entire file to memory
- Cache parsed tracks; recordings are short (seconds) so full in-memory GPS arrays are fine after parsing
- Attach `AdaptiveBehavior` to the root `ContentPage` or layout panel
- Breakpoint at 600px width: below = mobile/portrait (bottom tab bar), above = desktop/landscape (left nav rail)
- Use `AdaptiveClassSetter` to toggle style classes; define matching `Style` blocks in XAML
## Version Compatibility
| Package | Compatible With | Notes |
|---------|-----------------|-------|
| `LibVLCSharp.Avalonia` 3.9.6 | Avalonia >= 11.0.4 | Dependency constraint states `>= 11.0.4`; Avalonia 12 RC1 is `12.0.0-rc1` — should satisfy but verify at package restore time. Avalonia 12 RC1 fixed NativeControlHost pixel rounding which may resolve historic VideoView-in-UserControl issues |
| `Xaml.Behaviors.Avalonia` 12.0.0-rc1 | Avalonia 12 RC1 | Version-aligned with Avalonia 12; use pre-release flag in NuGet restore |
| `MetadataExtractor` 2.9.2 | netstandard2.0, net8.0, net10.0 | Compatible with `net10.0` and `net10.0-android` — no issues |
| `NSubstitute` 5.3.0 | .NET Standard 2.0+ | Stable; 6.0.0-rc.1 is available but wait for stable release |
| `Xamarin.AndroidX.Media3.ExoPlayer` 1.9.2.1 | net9.0-android, net10.0-android | Confirmed net10.0-android support |
| `CommunityToolkit.Mvvm` 8.4.1 | .NET 8+, .NET Standard 2.0 | Compatible with net10.0 |
## Open Questions (Flag for Later Research)
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
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
