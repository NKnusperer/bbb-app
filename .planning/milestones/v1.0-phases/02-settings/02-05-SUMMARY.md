---
phase: 02-settings
plan: 05
subsystem: shell-navigation-guard
tags: [navigation-guard, unsaved-changes, dialog, mvvm, settings]
dependency_graph:
  requires: [02-03, 02-04]
  provides: [UnsavedChangesGuard, SelectedTabIndex, DiscardChanges]
  affects: []
tech_stack:
  added: []
  patterns:
    - CommunityToolkit.Mvvm partial void OnXxxChanged hook for observable property side-effects
    - Async navigation guard with _isHandlingNavigation flag to prevent re-entrance
    - SettingsViewModel.DiscardChanges() resets _isDirty without saving
key_files:
  created: []
  modified:
    - src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs
    - src/BlackBoxBuddy/ViewModels/SettingsViewModel.cs
    - src/BlackBoxBuddy/Views/Shell/AppShellView.axaml
    - tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs
decisions:
  - CommunityToolkit.Mvvm partial void OnSelectedTabIndexChanged used for tab change detection — cleanest integration with existing ObservableProperty pattern
  - _isHandlingNavigation bool flag prevents re-entrance when reverting tab index programmatically during dialog display
  - DiscardChanges() is a public method on SettingsViewModel (not a command) — consumed by AppShellViewModel directly
metrics:
  duration: 5min
  completed_date: "2026-03-24"
  tasks_completed: 2
  files_changed: 4
---

# Phase 02 Plan 05: Unsaved-Changes Navigation Guard Summary

Tab navigation guard in AppShellViewModel intercepts Settings tab departure when dirty state is active, showing Unsaved Settings dialog with Save & Leave / Discard Changes options; SettingsViewModel gains DiscardChanges() for the discard path.

## Tasks Completed

| Task | Description | Commit |
|------|-------------|--------|
| 1 | Implement unsaved-changes navigation guard in AppShellViewModel + 5 new tests | 97555ce |
| 2 | Visual verification of complete Settings flow (auto-approved) | — |

## What Was Built

**Navigation Guard** (`AppShellViewModel.cs`): `SelectedTabIndex` observable property added with TwoWay binding to `TabbedPage.SelectedIndex`. `OnSelectedTabIndexChanged` partial method (generated hook for `[ObservableProperty]`) detects departure from Settings tab (index 3). When `SettingsVm.HasUnsavedChanges` is true, immediately reverts to Settings tab then shows dialog via `HandleUnsavedChangesNavigationAsync`.

**HandleUnsavedChangesNavigationAsync**: Uses `_isHandlingNavigation` flag to prevent recursive triggering when reverting SelectedTabIndex. Calls `IDialogService.ShowConfirmAsync` with dialog title "Unsaved Settings", body "You have unsaved settings. What would you like to do?", confirm "Save & Leave", cancel "Discard Changes". If save chosen: calls `SettingsVm.SaveCommand.ExecuteAsync`; if discard: calls `SettingsVm.DiscardChanges()`. After either path, navigates to the requested tab index.

**DiscardChanges()** (`SettingsViewModel.cs`): Public void method that resets `_isDirty = false`, fires `OnPropertyChanged(nameof(HasUnsavedChanges))`, and calls `SaveCommand.NotifyCanExecuteChanged()`.

**XAML binding** (`AppShellView.axaml`): Added `SelectedIndex="{Binding SelectedTabIndex, Mode=TwoWay}"` to `TabbedPage` element.

**Tests** (`AppShellViewModelTests.cs`): 5 new tests added:
1. `OnSelectedTabIndexChanged_FromSettingsWithUnsavedChanges_ShowsDialog` — dialog invoked when leaving dirty Settings tab
2. `OnSelectedTabIndexChanged_FromSettingsWithNoUnsavedChanges_DoesNotShowDialog` — no dialog when settings are clean
3. `OnSelectedTabIndexChanged_FromNonSettingsTab_DoesNotShowDialog` — no dialog when not leaving from Settings
4. `HandleUnsavedChanges_UserChoosesSave_CallsSaveCommand` — save path calls ApplySettingsAsync, clears dirty state
5. `HandleUnsavedChanges_UserChoosesDiscard_CallsDiscardChanges` — discard path clears dirty state without saving

## Test Results

All 89 tests pass (0 failures). AppShellViewModel tests: 10 total (5 existing + 5 new).

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None. All navigation guard logic wires to real IDialogService and SettingsViewModel implementations; no placeholders.

## Self-Check: PASSED
