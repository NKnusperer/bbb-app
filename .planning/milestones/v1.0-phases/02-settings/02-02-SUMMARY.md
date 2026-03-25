---
phase: 02-settings
plan: 02
subsystem: shell-di
tags: [dialog-service, dependency-injection, refactor, mvvm]
dependency_graph:
  requires: []
  provides: [IDialogService, ConfirmDialog, AppShellViewModel-tab-vms, Button.destructive-style]
  affects: [02-03, 02-04, 02-05]
tech_stack:
  added: []
  patterns:
    - Owner-provider lambda pattern for DialogService constructor (avoids Avalonia dependency in service layer)
    - AppShellViewModel as DI hub — all tab VMs injected and exposed as public properties
    - DataContext passed from parent XAML bindings (no code-behind Ioc.Default)
key_files:
  created:
    - src/BlackBoxBuddy/Services/IDialogService.cs
    - src/BlackBoxBuddy/Services/DialogService.cs
    - src/BlackBoxBuddy/Views/ConfirmDialog.axaml
    - src/BlackBoxBuddy/Views/ConfirmDialog.axaml.cs
  modified:
    - src/BlackBoxBuddy/App.axaml
    - src/BlackBoxBuddy/ViewModels/Shell/AppShellViewModel.cs
    - src/BlackBoxBuddy/Views/Shell/AppShellView.axaml
    - src/BlackBoxBuddy/Views/DashboardPage.axaml.cs
    - src/BlackBoxBuddy/Views/RecordingsPage.axaml.cs
    - src/BlackBoxBuddy/Views/LiveFeedPage.axaml.cs
    - src/BlackBoxBuddy/Views/SettingsPage.axaml.cs
    - src/BlackBoxBuddy/AppServices.cs
    - tests/BlackBoxBuddy.Tests/ViewModels/AppShellViewModelTests.cs
decisions:
  - IDialogService registered as singleton (not transient) because DialogService holds the owner-provider lambda which relies on ApplicationLifetime state
  - Button.destructive style defined once in App.axaml — avoids duplication between ConfirmDialog and SettingsPage (Plan 04)
  - AppShellViewModel exposes IDialogService as public property for downstream Plans (03, 05) to consume
metrics:
  duration: 3min
  completed_date: "2026-03-24"
  tasks_completed: 2
  files_changed: 13
---

# Phase 02 Plan 02: DI Refactor and Dialog Infrastructure Summary

IDialogService + ConfirmDialog Window with destructive/accent button styles, Ioc.Default removed from all View code-behinds, AppShellViewModel now injects all 4 tab VMs and IDialogService via constructor DI.

## Tasks Completed

| Task | Description | Commit |
|------|-------------|--------|
| 1 | Create IDialogService, DialogService, ConfirmDialog, shared Button.destructive style | ea3297b |
| 2 | DI refactor — inject tab VMs via AppShellViewModel, remove Ioc.Default from Views | 8dd7f10 |

## What Was Built

**IDialogService** (`src/BlackBoxBuddy/Services/IDialogService.cs`): Interface with `Task<bool> ShowConfirmAsync(title, message, confirmText, cancelText, isDestructive)`. The `isDestructive` flag controls whether the confirm button uses the red destructive or blue accent style — supporting both danger zone dialogs and unsaved-changes warnings.

**DialogService** (`src/BlackBoxBuddy/Services/DialogService.cs`): Production implementation using an owner-provider lambda (`Func<Window?>`) to decouple from Avalonia's application lifecycle at registration time. Calls `ShowDialog<bool>(owner)` to block until user responds.

**ConfirmDialog** (`src/BlackBoxBuddy/Views/ConfirmDialog.axaml`): Reusable `Window` with `SizeToContent="WidthAndHeight"`, `WindowStartupLocation="CenterOwner"`, min 320px / max 440px width. Named controls (`TitleBlock`, `MessageBlock`, `ConfirmButton`, `CancelButton`) populated via `SetContent()` in code-behind. Destructive mode replaces `Classes="accent"` with `Classes="destructive"`.

**Shared Button.destructive style** (`src/BlackBoxBuddy/App.axaml`): Single `Style Selector="Button.destructive"` definition with red (`#FFF44336`) background and white foreground. Applies globally — both ConfirmDialog and SettingsPage (Plan 04) consume it without duplication.

**DI Refactor**: AppShellViewModel constructor now accepts `IDialogService`, `DashboardViewModel`, `RecordingsViewModel`, `LiveFeedViewModel`, and `SettingsViewModel` in addition to the existing `IDeviceService` and `INavigationService`. All tab VMs exposed as public properties. AppShellView.axaml passes `DataContext="{Binding XxxVm}"` to each tab page. All 4 page code-behinds reduced to `InitializeComponent()` only.

## Deviations from Plan

None — plan executed exactly as written.

## Test Results

All 50 existing tests pass after refactor. AppShellViewModelTests updated to construct AppShellViewModel with the new 7-parameter signature using NSubstitute mocks for interfaces and `new XxxViewModel()` for concrete tab VMs.

## Known Stubs

None. All files wire to real implementations; no placeholder data flows to UI rendering.

## Self-Check: PASSED
