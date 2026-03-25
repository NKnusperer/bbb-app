# Phase 1: Foundation - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

App shell with adaptive dark-mode layout (vertical nav in portrait, horizontal in landscape), device abstraction layer with mock device, auto-discovery with manual fallback, connection status indicator, and provisioning wizard for unconfigured devices. This phase delivers the structural foundation everything else builds on.

</domain>

<decisions>
## Implementation Decisions

### Navigation Shell
- **D-01:** Use Avalonia 12's TabbedPage with TabPlacement="Auto" (bottom on mobile, left/top on desktop) for the main navigation structure
- **D-02:** Icon-only nav bar with centered icons — no text labels. Pages: Dashboard, Recordings, Live Feed, Settings
- **D-03:** Connection indicator is a persistent element outside the tab content area (header/footer), always visible regardless of active page
- **D-04:** Breakpoint at 600px width determines portrait vs landscape layout adaptation
- **D-05:** Dark mode enforced — set RequestedThemeVariant="Dark" on Application

### Device Abstraction
- **D-06:** Interface segregation pattern — 5 narrow interfaces: IDeviceDiscovery, IDeviceConnection, IDeviceCommands, IDeviceFileSystem, IDeviceLiveStream
- **D-07:** IDashcamDevice composes all 5 interfaces as a unified device handle
- **D-08:** MockDashcamDevice implements all interfaces with configurable delay and failure simulation for testing
- **D-09:** Mock device generates realistic sample data: recordings with metadata, device info, connection states
- **D-10:** Device abstraction lives in a Device/ folder in the shared project with interfaces and mock implementation

### Connection Flow
- **D-11:** Three connection indicator states: Searching (animated), Connected (shows device name), Disconnected (tap to retry/configure)
- **D-12:** Auto-discovery runs for 5 seconds on startup before falling back to manual
- **D-13:** After timeout, indicator shows "No device found" with tap action to open manual connection
- **D-14:** Manual connection allows IP/hostname entry for direct device connection
- **D-15:** Connection indicator tap opens manual connection setup (CONN-03) and SD Card mode entry (CONN-04)

### Provisioning Wizard
- **D-16:** Linear 3-step wizard: (1) Welcome + device info display, (2) WiFi setup (AP mode or join network), (3) Confirmation/done
- **D-17:** "Unconfigured" defined as: mock device reports isProvisioned=false
- **D-18:** Wizard uses NavigationPage with forward/back navigation gestures
- **D-19:** After provisioning completes, navigate to Dashboard

### DI and App Bootstrap
- **D-20:** Use Microsoft.Extensions.DependencyInjection with CommunityToolkit.Mvvm's Ioc.Default
- **D-21:** Replace reflection-based ViewLocator with pattern-matching switch expression (AOT-safe)
- **D-22:** Shared ConfigureServices method called from both Desktop Program.cs and Android entry point
- **D-23:** All services registered as singletons (device service, discovery service); ViewModels as transient
- **D-24:** INavigationService abstraction wrapping Avalonia 12 page navigation to isolate from RC API changes

### Claude's Discretion
- Exact icon choices for nav bar (Material Icons, Fluent, or custom)
- Loading animation style for Searching state
- Exact responsive breakpoint thresholds beyond the 600px primary
- Internal error handling patterns and logging approach
- Test structure and naming conventions

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

No external specs — requirements are fully captured in decisions above and in the following project artifacts:

### Project context
- `.planning/PROJECT.md` — Project vision, constraints, key decisions
- `.planning/REQUIREMENTS.md` — Full v1 requirements with IDs (FOUND-01..06, CONN-01..05, PROV-01..02 for this phase)
- `.planning/research/SUMMARY.md` — Research synthesis with architecture recommendations
- `.planning/research/ARCHITECTURE.md` — Detailed architecture patterns, component boundaries, build order
- `.planning/research/PITFALLS.md` — Critical pitfalls to avoid (ViewLocator replacement, ObservableCollection threading, RC1 version pinning)
- `.planning/research/STACK.md` — Stack recommendations and library versions

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ViewModelBase` (ViewModels/ViewModelBase.cs): Already extends ObservableObject — keep as base class for all ViewModels
- `App.axaml.cs`: Handles both IClassicDesktopStyleApplicationLifetime and IActivityApplicationLifetime — good multi-platform pattern to extend with DI

### Established Patterns
- CommunityToolkit.Mvvm source generators ([ObservableProperty]) already in use — continue this pattern
- FluentTheme is the active theme — switch to Dark variant
- Separate Desktop/Android head projects with shared library — maintain this structure

### Integration Points
- `App.axaml.cs:OnFrameworkInitializationCompleted` — DI container initialization goes here
- `ViewLocator.cs` — Must be replaced with pattern-matching implementation
- `Program.cs` (Desktop) — Add DI service registration
- `AndroidApp.cs` / `MainActivity.cs` — Add DI service registration for Android
- Test project references shared project — add service mocks and ViewModel tests

</code_context>

<specifics>
## Specific Ideas

- Research recommends TabbedPage with TabPlacement="Auto" for adaptive nav — this is an Avalonia 12 RC1 feature that handles portrait/landscape automatically
- ViewLocator replacement is flagged as critical by both Architecture and Pitfalls research — it blocks AOT compilation
- Pin exact Avalonia 12 RC1 NuGet versions in Directory.Build.props to survive RC-to-stable transition
- INavigationService abstraction protects against Avalonia 12 navigation API changes between RC and stable

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-foundation*
*Context gathered: 2026-03-24*
