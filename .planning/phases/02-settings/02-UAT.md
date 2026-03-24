---
status: diagnosed
phase: 02-settings
source: [02-01-SUMMARY.md, 02-02-SUMMARY.md, 02-03-SUMMARY.md, 02-04-SUMMARY.md, 02-05-SUMMARY.md]
started: 2026-03-24T14:00:00Z
updated: 2026-03-24T14:15:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Settings Page Loads Successfully
expected: Run `dotnet run --project src/BlackBoxBuddy`. Navigate to Settings tab. A loading indicator (progress bar) appears briefly, then 8 settings sections appear in a scrollable view: WiFi, Recording, Channels, Camera, Sensors, System, Overlays, Danger Zone.
result: issue
reported: "Not settings visible, only a black view."
severity: blocker

### 2. WiFi Settings Controls
expected: WiFi section shows Band radio buttons (2.4 GHz / 5 GHz), Mode radio buttons (AP / Client), Network Name text field, and Password text field. All pre-populated with mock device defaults.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 3. Recording and Channels Controls
expected: Recording section shows Driving Mode (Standard/Racing) and Parking Mode (Standard/Event Only) radio buttons. Channels section shows Front Only / Front + Rear radio buttons. Camera section shows Rear Orientation Normal/Flipped radio buttons.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 4. Sensor Sliders Snap to Integers
expected: Sensors section shows 3 sliders: Driving Shock (1-5), Parking Shock (1-5), Radar Sensitivity (1-5). Dragging any slider snaps to whole numbers only — no decimal values shown.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 5. System Toggles and Volume
expected: System section shows GPS toggle, Microphone toggle, and Speaker Volume slider (0-5). Toggles can be flipped on/off.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 6. Speed Unit Visibility Toggle
expected: Overlays section shows Date/Time/GPS Position/Speed overlay toggles. When Speed Overlay is ON, Speed Unit radio buttons (km/h / mph) are visible. When Speed Overlay is OFF, Speed Unit radio buttons disappear.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 7. Danger Zone Styling
expected: Danger Zone section has a red border and light red background tint. Factory Reset and Wipe SD Card buttons are styled red (destructive style).
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 8. Danger Zone Confirmation Dialogs
expected: Click Factory Reset button. A confirmation dialog appears with destructive (red) confirm button. Click cancel — nothing happens. Click Factory Reset again and confirm — settings reload to defaults. Same pattern for Wipe SD Card.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 9. Save Bar Appears on Dirty State
expected: Change any setting (e.g., flip a toggle or move a slider). A sticky Save bar appears at the bottom of the screen. Click Save — bar disappears and changes are persisted. Change a setting again — Save bar reappears.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

### 10. Unsaved Changes Navigation Guard
expected: Change any setting (so Save bar is visible). Click a different tab (e.g., Dashboard). An "Unsaved Settings" dialog appears with "Save & Leave" and "Discard Changes" options. Choose "Discard Changes" — navigates to the other tab, Save bar gone when returning to Settings. Repeat: make a change, switch tabs, choose "Save & Leave" — changes are saved, then navigates to the other tab.
result: blocked
blocked_by: prior-phase
reason: "Settings page renders black — blocked by Test 1"

## Summary

total: 10
passed: 0
issues: 1
pending: 0
skipped: 0
blocked: 9

## Gaps

- truth: "Settings page loads and displays 8 scrollable sections after loading indicator"
  status: failed
  reason: "User reported: Not settings visible, only a black view."
  severity: blocker
  test: 1
  root_cause: "ContentPage has Template=null in Avalonia 12 RC1 FluentTheme. Without a ControlTemplate, no ContentPresenter is created, so Content exists in the logical tree but is never added to the visual tree. All 4 pages (Dashboard, Recordings, LiveFeed, Settings) have this issue — their child controls all have 0x0 bounds. Confirmed via DevTools: SettingsPage.Bounds=2880x1014 but Grid child Bounds=0x0. DashboardPage.TextBlock also has 0x0 bounds."
  artifacts:
    - path: "src/BlackBoxBuddy/Views/SettingsPage.axaml"
      issue: "Uses ContentPage which has no default template in FluentTheme"
    - path: "src/BlackBoxBuddy/Views/DashboardPage.axaml"
      issue: "Same ContentPage template issue"
    - path: "src/BlackBoxBuddy/Views/RecordingsPage.axaml"
      issue: "Same ContentPage template issue"
    - path: "src/BlackBoxBuddy/Views/LiveFeedPage.axaml"
      issue: "Same ContentPage template issue"
    - path: "src/BlackBoxBuddy/Views/Shell/AppShellView.axaml"
      issue: "TabbedPage hosts ContentPage children that lack templates"
  missing:
    - "Add a ContentPage ControlTemplate with ContentPresenter to App.axaml styles, OR convert all 4 pages from ContentPage to UserControl"
  debug_session: ""
