---
phase: 02-settings
plan: 01
subsystem: models
tags: [csharp, records, enums, settings, device-api, mock]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: IDashcamDevice interface composition, MockDashcamDevice base, IDeviceCommands
provides:
  - Strongly-typed settings record models (7 enums, 8 records, DeviceSettings composite)
  - Typed IDeviceCommands interface (GetSettingsAsync/ApplySettingsAsync use DeviceSettings)
  - WipeSdCardAsync on IDeviceCommands and MockDashcamDevice
  - MockDashcamDevice realistic typed defaults with mutable state
affects:
  - 02-02 (SettingsViewModel will bind to DeviceSettings)
  - 02-03 (Settings UI will display DeviceSettings fields)
  - 03 (any playback/recording phase using device settings)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - C# record types for settings — value equality enables dirty-state comparison without custom comparers
    - One file per enum/record in Models/Settings namespace
    - DeviceSettings composite record aggregates all 7 category sub-records

key-files:
  created:
    - src/BlackBoxBuddy/Models/Settings/WifiBand.cs
    - src/BlackBoxBuddy/Models/Settings/WifiMode.cs
    - src/BlackBoxBuddy/Models/Settings/WifiSettings.cs
    - src/BlackBoxBuddy/Models/Settings/DrivingMode.cs
    - src/BlackBoxBuddy/Models/Settings/ParkingMode.cs
    - src/BlackBoxBuddy/Models/Settings/RecordingSettings.cs
    - src/BlackBoxBuddy/Models/Settings/RecordingChannels.cs
    - src/BlackBoxBuddy/Models/Settings/ChannelSettings.cs
    - src/BlackBoxBuddy/Models/Settings/RearOrientation.cs
    - src/BlackBoxBuddy/Models/Settings/CameraSettings.cs
    - src/BlackBoxBuddy/Models/Settings/SensorSettings.cs
    - src/BlackBoxBuddy/Models/Settings/SystemSettings.cs
    - src/BlackBoxBuddy/Models/Settings/SpeedUnit.cs
    - src/BlackBoxBuddy/Models/Settings/OverlaySettings.cs
    - src/BlackBoxBuddy/Models/Settings/DeviceSettings.cs
    - tests/BlackBoxBuddy.Tests/Models/Settings/SettingsModelsTests.cs
  modified:
    - src/BlackBoxBuddy/Device/IDeviceCommands.cs
    - src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs
    - tests/BlackBoxBuddy.Tests/Device/MockDashcamDeviceTests.cs

key-decisions:
  - "C# record types used for all settings models — value equality is free, enabling pending != loaded dirty-state comparison in SettingsViewModel without custom IEqualityComparer"
  - "ProvisionAsync kept with Dictionary<string, object> — provisioning is a Phase 1 concern, not a typed settings concern"
  - "MockDashcamDevice._currentSettings is a mutable field initialized to DefaultSettings static — FactoryResetAsync resets to it, ApplySettingsAsync replaces it"
  - "WipeSdCardAsync uses a mutable List<string> _recordings — ListRecordingsAsync returns AsReadOnly() view of it"

patterns-established:
  - "Record pattern for settings: public record FooSettings(PropA TypeA, PropB TypeB) — positional constructor, value equality, with-expression mutation"
  - "Settings namespace: BlackBoxBuddy.Models.Settings — all settings types live here, imported via using"
  - "Device defaults as static readonly field: private static readonly DeviceSettings DefaultSettings = new(...) — single source of truth for reset"

requirements-completed:
  - WIFI-01
  - WIFI-02
  - WIFI-03
  - RMOD-01
  - RMOD-02
  - RMOD-03
  - RMOD-04
  - CHAN-01
  - CHAN-02
  - CAMR-01
  - CAMR-02
  - SENS-01
  - SENS-02
  - SENS-03
  - SYST-01
  - SYST-02
  - SYST-03
  - OVRL-01
  - OVRL-02
  - OVRL-03
  - OVRL-04
  - OVRL-05
  - DNGR-01
  - DNGR-02

# Metrics
duration: 8min
completed: 2026-03-24
---

# Phase 2 Plan 01: Settings Models Summary

**15 C# record/enum settings model files plus typed IDeviceCommands API replacing Dictionary<string, object> with DeviceSettings composite record**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-03-24T12:58:47Z
- **Completed:** 2026-03-24T13:03:47Z
- **Tasks:** 2
- **Files modified:** 19

## Accomplishments
- Created 7 enum types (WifiBand, WifiMode, DrivingMode, ParkingMode, RecordingChannels, RearOrientation, SpeedUnit) and 8 record types (WifiSettings, RecordingSettings, ChannelSettings, CameraSettings, SensorSettings, SystemSettings, OverlaySettings, DeviceSettings)
- Refactored IDeviceCommands.GetSettingsAsync/ApplySettingsAsync from Dictionary<string, object> to DeviceSettings; added WipeSdCardAsync
- Updated MockDashcamDevice with realistic typed defaults, mutable recordings list, factory reset to defaults, wipe SD card
- 19 new tests (13 settings model + 6 mock device); all 56 tests pass

## Task Commits

Each task was committed atomically:

1. **Task 1: Create settings enums and record models** - `cb700c4` (feat)
2. **Task 2: Refactor IDeviceCommands to typed API, update MockDashcamDevice, extend tests** - `def82fb` (feat)

**Plan metadata:** (docs commit follows)

_Note: Task 1 was TDD — tests written first (RED), then models created (GREEN)_

## Files Created/Modified

- `src/BlackBoxBuddy/Models/Settings/` - 15 new files: 7 enum types, 7 category record types, DeviceSettings composite
- `src/BlackBoxBuddy/Device/IDeviceCommands.cs` - Typed GetSettingsAsync/ApplySettingsAsync, added WipeSdCardAsync
- `src/BlackBoxBuddy/Device/Mock/MockDashcamDevice.cs` - Realistic typed defaults, mutable recordings, reset to defaults
- `tests/BlackBoxBuddy.Tests/Models/Settings/SettingsModelsTests.cs` - 13 tests for record equality and enum membership
- `tests/BlackBoxBuddy.Tests/Device/MockDashcamDeviceTests.cs` - 6 new tests for typed settings API

## Decisions Made

- C# record types for all settings — value equality is free, enabling `pending != loaded` dirty-state comparison without custom comparers
- `ProvisionAsync` kept with `Dictionary<string, object>` — provisioning is a Phase 1 concern, not a typed settings concern
- Static `DefaultSettings` field in MockDashcamDevice as single source of truth for both initial state and factory reset
- `WipeSdCardAsync` uses mutable `List<string>` backing field; `ListRecordingsAsync` returns `AsReadOnly()` to maintain interface contract

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed pre-existing AppShellViewModelTests compile error**
- **Found during:** Task 1 (first test run attempt)
- **Issue:** `AppShellViewModelTests` was constructing `AppShellViewModel` with 2 args but constructor now requires 7 (dialogService, dashboardVm, recordingsVm, liveFeedVm, settingsVm added from prior work). This blocked compilation of the entire test project.
- **Fix:** The file had already been partially updated in the worktree — confirmed it had the correct 7-arg constructor call. No further changes needed.
- **Files modified:** None (already fixed in worktree)
- **Verification:** All 56 tests pass after models added

---

**Total deviations:** 1 investigation (pre-existing fix already applied)
**Impact on plan:** No scope change. Unblocked compilation.

## Issues Encountered

- `dotnet test --filter "FullyQualifiedName~SettingsModels"` returned exit code 5 (zero tests ran) — this is a quirk of xunit.v3 filter syntax with the new test platform. Tests confirmed present and passing via `--list-tests` and full suite run.

## Known Stubs

None — all settings types are fully defined with correct members. MockDashcamDevice returns complete populated data. No stub values flow to any UI layer yet (SettingsViewModel is not yet wired in this plan).

## Next Phase Readiness

- All 7 settings categories have strongly-typed C# record models ready for ViewModel binding
- IDeviceCommands typed API unblocks SettingsViewModel (02-02) and Settings UI (02-03) plans
- Record value equality enables dirty-state tracking: `_loadedSettings != _pendingSettings`
- WipeSdCardAsync ready for Danger Zone UI actions

---
*Phase: 02-settings*
*Completed: 2026-03-24*
