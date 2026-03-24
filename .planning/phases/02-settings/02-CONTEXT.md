# Phase 2: Settings - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Full device configuration UI — WiFi, recording modes, channels, camera orientation, sensors, system settings, video overlays, and danger-zone operations (factory reset, SD card wipe). All settings are read from and persisted to the connected device via `IDeviceCommands`. This phase does NOT add new device discovery, navigation structure, or recording features.

</domain>

<decisions>
## Implementation Decisions

### Settings Data Model
- **D-01:** Strongly-typed C# models per settings category (records/classes with enums): `WifiSettings`, `RecordingSettings`, `ChannelSettings`, `CameraSettings`, `SensorSettings`, `SystemSettings`, `OverlaySettings`
- **D-02:** `IDeviceCommands` interface changes to use strongly-typed models directly — drop the `Dictionary<string, object>` approach for both `GetSettingsAsync` and `ApplySettingsAsync`
- **D-03:** MockDashcamDevice must be updated to return/accept the new typed models with realistic default values for all settings categories

### Settings Page Organization
- **D-04:** Single scrollable page with visual section headers per category (WiFi, Recording, Channels, Camera, Sensors, System, Overlays, Danger Zone)
- **D-05:** All sections visible by scrolling — no accordion, no sub-pages
- **D-06:** Danger Zone section visually separated at the bottom with distinct styling to signal destructiveness

### Settings Loading
- **D-07:** All settings loaded at once via a single call on Settings page entry
- **D-08:** Typed models mapped from device response and bound to ViewModel properties

### Save Behavior
- **D-09:** Explicit Save button — sticky/floating at bottom of the page
- **D-10:** Save button only visible when there are unsaved changes (dirty state tracking)
- **D-11:** All pending changes batched into a single `ApplySettingsAsync` call
- **D-12:** Success/failure feedback shown after save attempt

### Unsaved Changes Guard
- **D-13:** If user navigates away with unsaved changes, show a warning dialog: "You have unsaved settings." with Discard / Save & Leave options

### Danger Zone UX
- **D-14:** Factory reset and SD card wipe use confirmation dialogs with clear warning text, Cancel and destructive-action Confirm buttons
- **D-15:** `IDeviceCommands` needs a new `WipeSdCardAsync` method (currently only has `FactoryResetAsync`)

### Dependency Injection
- **D-16:** No static service locator calls (`Ioc.Default.GetRequiredService`) in Views or ViewModels — use proper constructor-based dependency injection everywhere
- **D-17:** This is a cross-cutting fix: existing Phase 1 code that uses `Ioc.Default.GetRequiredService` in View constructors must be refactored as part of this phase

### Testing
- **D-18:** Comprehensive test coverage for every unit with meaningful logic — not just ViewModels and Services
- **D-19:** Tests must cover ValueConverters, Views (via Avalonia.Headless.XUnit), and Controls in addition to ViewModels and Services
- **D-20:** Automated testing is a priority — no untested business logic

### Claude's Discretion
- Exact section header styling and spacing
- Control types for each setting (ComboBox, ToggleSwitch, Slider, RadioButton, etc.)
- Settings mapper implementation details (extension methods, dedicated service, etc.)
- Loading indicator while settings are being fetched
- Error handling when device returns unexpected/missing settings
- Exact dialog wording for danger zone confirmations

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project context
- `.planning/PROJECT.md` — Project vision, constraints, key decisions
- `.planning/REQUIREMENTS.md` — Full v1 requirements with IDs (WIFI-01..03, RMOD-01..04, CHAN-01..02, CAMR-01..02, SENS-01..03, SYST-01..03, OVRL-01..05, DNGR-01..02 for this phase)
- `.planning/ROADMAP.md` — Phase 2 goal, dependencies, success criteria

### Phase 1 context
- `.planning/phases/01-foundation/01-CONTEXT.md` — Foundation decisions (navigation, device abstractions, DI patterns)

### Device interface contracts
- `src/BlackBoxBuddy/Device/IDeviceCommands.cs` — Current interface to be refactored with typed models
- `src/BlackBoxBuddy/Device/IDashcamDevice.cs` — Composite device interface
- `src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs` — Mock implementation to be expanded

### Existing settings code
- `src/BlackBoxBuddy/ViewModels/SettingsViewModel.cs` — Placeholder to be built out
- `src/BlackBoxBuddy/Views/SettingsPage.axaml` — Placeholder to be built out

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ViewModelBase` (ViewModels/ViewModelBase.cs): Base class extending ObservableObject — SettingsViewModel will extend this
- `SettingsPage.axaml` + `SettingsViewModel.cs`: Already wired as a tab in the TabbedPage — just need to fill in content
- `IDeviceService` / `DeviceService`: Manages device connection state — SettingsViewModel will depend on this to get the connected device

### Established Patterns
- CommunityToolkit.Mvvm source generators (`[ObservableProperty]`, `[RelayCommand]`) — use for all settings properties and save command
- FluentTheme Dark variant — all controls will inherit dark styling
- ContentPage-based navigation — Settings is already a ContentPage tab

### Integration Points
- `IDeviceCommands.GetSettingsAsync` / `ApplySettingsAsync` — must be refactored from dictionary to typed models
- `IDeviceCommands.FactoryResetAsync` — exists; need to add `WipeSdCardAsync`
- `MockDashcamDevice` — must implement all new typed settings methods with realistic defaults
- View constructors using `Ioc.Default.GetRequiredService` — must be refactored to constructor injection

</code_context>

<specifics>
## Specific Ideas

- The DI refactor (D-16/D-17) is a cross-cutting fix that improves testability across the entire app, not just Settings
- Settings page layout follows the single-scroll-with-sections pattern from the mockup — all 7+ categories visible by scrolling
- Dirty state tracking enables both the floating Save button visibility and the unsaved changes navigation guard

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 02-settings*
*Context gathered: 2026-03-24*
