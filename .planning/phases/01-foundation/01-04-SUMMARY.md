---
phase: 01-foundation
plan: 04
subsystem: ui
tags: [avalonia, mvvm, connection-flow, provisioning, navigation, nsubstitute, fluentassertions]

# Dependency graph
requires:
  - phase: 01-02
    provides: IDeviceService, DeviceService, MockDashcamDevice, INavigationService
  - phase: 01-03
    provides: AppShellViewModel skeleton, ConnectionIndicator, AppShellView with TabbedPage

provides:
  - Auto-discovery on startup via StartDiscoveryCommand in App.axaml.cs
  - Manual connection dialog (ManualConnectionDialog.axaml) with IP entry and SD Card mode entry
  - ManualConnectionViewModel with ConnectAsync, CanConnect, Cancel, EnterSdCardMode
  - AppShellViewModel: IsManualConnectionVisible overlay toggle, NeedsProvisioning -> PushAsync navigation
  - 3-step provisioning wizard (ProvisioningPage.axaml): Welcome, WiFi Setup, Confirmation
  - ProvisioningViewModel with Next/Back/CompleteAsync, SelectedWifiMode, PopToRootAsync on success
  - IDeviceService.ProvisionAsync and DeviceService implementation delegating to device layer
  - AppShellViewModelTests (7 facts), ProvisioningViewModelTests (11 facts)

affects: [02-recordings, 03-media, 04-mobile]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Manual dialog VM constructed by parent VM with onClose callback — not registered in DI"
    - "Overlay pattern: IsManualConnectionVisible bool + Panel layering in AXAML for modal-like dialog"
    - "NeedsProvisioning state change triggers async PushAsync via _ = Task discard"
    - "Auto-discovery via _ = shellVm.StartDiscoveryCommand.ExecuteAsync(null) after shell setup"
    - "PlaceholderText (not Watermark) for TextBox in Avalonia 12 RC1"

key-files:
  created:
    - src/BlackBoxBuddy/ViewModels/Shell/ManualConnectionViewModel.cs
    - src/BlackBoxBuddy/ViewModels/Provisioning/ProvisioningViewModel.cs
    - src/BlackBoxBuddy/Views/Shell/ManualConnectionDialog.axaml
    - src/BlackBoxBuddy/Views/Shell/ManualConnectionDialog.axaml.cs
    - src/BlackBoxBuddy/Views/Provisioning/ProvisioningPage.axaml
    - src/BlackBoxBuddy/Views/Provisioning/ProvisioningPage.axaml.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/ProvisioningViewModelTests.cs
  modified:
    - src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs
    - src/BlackBoxBuddy/App.axaml.cs
    - src/BlackBoxBuddy/AppServices.cs
    - src/BlackBoxBuddy/ViewLocator.cs
    - src/BlackBoxBuddy/Services/IDeviceService.cs
    - src/BlackBoxBuddy/Services/DeviceService.cs
    - src/BlackBoxBuddy/Views/Shell/AppShellView.axaml

key-decisions:
  - "ManualConnectionViewModel constructed manually by AppShellViewModel with onClose Action callback — DI cannot supply per-instance callbacks"
  - "Manual connection shown as overlay (Panel + Border) in AppShellView rather than separate page/dialog — simpler than NavigationPage push for a modal"
  - "ProvisioningViewModel registered as transient in DI; AppShellViewModel constructs it directly with new() for NeedsProvisioning navigation since it needs services from DI anyway"
  - "TextBox.Watermark is deprecated in Avalonia 12 RC1; use PlaceholderText"

requirements-completed: [CONN-02, CONN-03, CONN-04, PROV-01, PROV-02]

# Metrics
duration: 25min
completed: 2026-03-24
---

# Phase 1 Plan 04: Connection Flow and Provisioning Wizard Summary

**Connection flow with auto-discovery, manual IP entry dialog with SD Card mode entry, and 3-step provisioning wizard (Welcome/WiFi Setup/Confirmation) wired to IDeviceService.ProvisionAsync**

## Performance

- **Duration:** ~25 min
- **Started:** 2026-03-24T10:00:00Z
- **Completed:** 2026-03-24T10:25:00Z
- **Tasks:** 2 auto + 1 checkpoint (auto-approved)
- **Files modified:** 15 (7 created, 8 modified)

## Accomplishments

- Auto-discovery runs on startup via `StartDiscoveryCommand.ExecuteAsync` in App.axaml.cs
- Tapping connection indicator when disconnected shows ManualConnectionDialog overlay with IP entry (TextBox) and SD Card Mode button
- NeedsProvisioning state change triggers navigation to ProvisioningViewModel via PushAsync
- 3-step wizard: Welcome (device name/firmware), WiFi Setup (AP/client mode, SSID/password), Confirmation (All Set)
- CompleteAsync calls _deviceService.ProvisionAsync then PopToRootAsync to return to Dashboard
- IDeviceService.ProvisionAsync added to interface and implemented in DeviceService
- 37 total tests pass (25 existing + 7 AppShellViewModel + 11 ProvisioningViewModel)

## Task Commits

1. **Task 1: Connection flow** - `499d507` (feat)
2. **Task 2: Provisioning wizard** - `97047ab` (feat)
3. **Deviation fix: Deprecated Watermark** - `9ffdda8` (fix)

## Files Created/Modified

- `src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs` - Added IsManualConnectionVisible, ManualConnectionViewModel, NeedsProvisioning navigation, updated ConnectionIndicatorTapped
- `src/BlackBoxBuddy/ViewModels/Shell/ManualConnectionViewModel.cs` - ConnectAsync (with CanConnect guard), Cancel, EnterSdCardMode
- `src/BlackBoxBuddy/ViewModels/Provisioning/ProvisioningViewModel.cs` - 3-step wizard state machine with CompleteAsync -> ProvisionAsync -> PopToRootAsync
- `src/BlackBoxBuddy/Views/Shell/ManualConnectionDialog.axaml` - IP entry TextBox, Connect/Cancel/SD Card Mode buttons
- `src/BlackBoxBuddy/Views/Shell/ManualConnectionDialog.axaml.cs` - Code-behind for ManualConnectionDialog
- `src/BlackBoxBuddy/Views/Shell/AppShellView.axaml` - Added Panel overlay with ManualConnectionDialog bound to IsManualConnectionVisible
- `src/BlackBoxBuddy/Views/Provisioning/ProvisioningPage.axaml` - ContentPage with 3-panel step system (Welcome/WiFi Setup/All Set)
- `src/BlackBoxBuddy/Views/Provisioning/ProvisioningPage.axaml.cs` - Code-behind
- `src/BlackBoxBuddy/Services/IDeviceService.cs` - Added Task<bool> ProvisionAsync
- `src/BlackBoxBuddy/Services/DeviceService.cs` - Implemented ProvisionAsync delegating to _device.ProvisionAsync
- `src/BlackBoxBuddy/App.axaml.cs` - Trigger StartDiscoveryCommand after shell setup
- `src/BlackBoxBuddy/AppServices.cs` - Added ProvisioningViewModel as transient
- `src/BlackBoxBuddy/ViewLocator.cs` - Added ManualConnectionViewModel and ProvisioningViewModel mappings
- `tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs` - 7 tests
- `tests/BlackBoxBuddy.Tests/ViewModels/ProvisioningViewModelTests.cs` - 11 tests

## Decisions Made

- **Manual dialog VM is not DI-registered:** ManualConnectionViewModel requires a per-instance `onClose` Action callback that cannot be injected. Constructed directly by AppShellViewModel: `new ManualConnectionViewModel(_deviceService, () => IsManualConnectionVisible = false)`.
- **Overlay approach for manual connection:** Using Panel + Border with IsVisible binding rather than NavigationPage push. Simpler, avoids needing NavigationPage to be the root.
- **PlaceholderText vs Watermark:** Avalonia 12 RC1 deprecated TextBox.Watermark in favor of PlaceholderText. Applied to both dialog TextBoxes.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Replaced deprecated TextBox.Watermark with PlaceholderText**
- **Found during:** Task 1 and 2 build/verification
- **Issue:** Avalonia 12 RC1 deprecates TextBox.Watermark; generates AVLN5001 warnings
- **Fix:** Replaced `Watermark=` with `PlaceholderText=` in ManualConnectionDialog.axaml and ProvisioningPage.axaml
- **Files modified:** src/BlackBoxBuddy/Views/Shell/ManualConnectionDialog.axaml, src/BlackBoxBuddy/Views/Provisioning/ProvisioningPage.axaml
- **Verification:** No AVLN5001 warnings after fix; build succeeds
- **Committed in:** 9ffdda8

---

**Total deviations:** 1 auto-fixed (Rule 1 - deprecation bug)
**Impact on plan:** Minor fix to keep code current with Avalonia 12 RC1 API. No scope creep.

## Issues Encountered

- `Raise.EventWith<ConnectionState>()` in NSubstitute requires EventArgs subtype — used `Raise.Event<EventHandler<ConnectionState>>()` instead for enum event args. Standard NSubstitute pattern for non-EventArgs event argument types.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Complete Phase 1 foundation is done: app shell, connection flow, provisioning wizard
- All 37 tests pass; app builds cleanly
- Phase 2 (recordings) can begin — IDeviceFileSystem already has ListRecordingsAsync in MockDashcamDevice
- NavigationService still uses stub implementation (PushAsync/PopAsync/PopToRootAsync are no-ops) — provisioning navigation calls complete the state machine correctly but won't visually navigate until NavigationPage is wired in a future phase

---
*Phase: 01-foundation*
*Completed: 2026-03-24*

## Self-Check: PASSED

- All 7 created files verified present on disk
- All 3 task commits (499d507, 97047ab, 9ffdda8) verified in git log
- 37 tests pass (0 failures)
