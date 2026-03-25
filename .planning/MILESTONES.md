# Milestones

## v1.0 MVP (Shipped: 2026-03-25)

**Phases completed:** 4 phases, 17 plans, 19 tasks

**Key accomplishments:**

- AOT-safe DI-wired Avalonia 12 skeleton with dark mode, pattern-matching ViewLocator, and all device/service interface contracts defined
- Dark-mode TabbedPage shell with 4 icon-only tabs, persistent ConnectionIndicator with state-specific colors (yellow/green/orange/red), AdaptiveBehavior 600px breakpoint, and NavigationService wired via DI
- Connection flow with auto-discovery, manual IP entry dialog with SD Card mode entry, and 3-step provisioning wizard (Welcome/WiFi Setup/Confirmation) wired to IDeviceService.ProvisionAsync
- 15 C# record/enum settings model files plus typed IDeviceCommands API replacing Dictionary<string, object> with DeviceSettings composite record
- IDialogService
- SettingsViewModel with 20+ observable properties, dirty-state tracking via OnPropertyChanged override, and 18 passing unit tests covering load/save/danger-zone behaviors
- 1. [Rule 1 - Bug] ProgressRing does not exist in Avalonia 12 RC1
- Navigation Guard
- One-liner:
- TripGroupingService (30s gap, newest-first), ArchiveService (~/BlackBoxBuddy/Archives/ with IProgress), NavigationService (NavigationPage push/pop), EventTypeToBrushConverter, BytesToBitmapConverter, all DI-registered — 122 tests pass.
- RecordingsViewModel
- One-liner:
- Multi-select archive flow with batch progress, cancel support, archived badges, and overlay-based detail navigation
- Avalonia 12 RC1 compatible VideoView inlined, LibVLCSharp.Avalonia NuGet removed, LiveFeedViewModel with full stream lifecycle and DashboardViewModel with Action-callback cross-tab wiring implemented and tested
- LiveFeedPage implemented with video area, loading/connection-loss state placeholders, camera segmented toggle (Front/Rear), tab lifecycle management via IsVisibleProperty.Changed.AddClassHandler, and Desktop VideoView wiring via reflection
- DashboardPage XAML with three data-bound sections (Recent Recordings, Recent Trips, Recent Events), compact card templates, See All navigation, loading/empty states, and tab-lifecycle code-behind

---
