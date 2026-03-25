# BlackBoxBuddy

## What This Is

A cross-platform desktop (PC) and mobile (Android) application for configuring automotive dashcams and managing their recordings. Everything runs locally — no cloud services. The app auto-discovers connected dashcams, guides users through device provisioning, and provides a unified interface for settings, live feeds, and footage management.

## Core Value

Users can effortlessly manage their dashcam footage — browse recordings, combine them into trips, and archive important moments before the dashcam overwrites them.

## Requirements

### Validated

- [x] Auto-discover dashcam on startup; fall back to manual connection if discovery fails — Phase 1
- [x] Guide unconfigured devices through provisioning — Phase 1
- [x] Configure WiFi (2.4/5 GHz, AP client or AP mode) — Phase 2
- [x] Configure recording modes (driving: standard/racing; parking: standard/event-only) — Phase 2
- [x] Configure recording channels (front only, front+rear) — Phase 2
- [x] Configure rear camera orientation (0/180 degrees) — Phase 2
- [x] Configure shock sensor sensitivity (driving 1-5, parking 1-5) — Phase 2
- [x] Configure radar sensor sensitivity (1-5) — Phase 2
- [x] Configure system settings (GPS, microphone, speaker volume) — Phase 2
- [x] Configure video overlays (date, time, GPS position, speed in km/h or mph) — Phase 2
- [x] Danger zone: factory reset, wipe SD card — Phase 2
- [x] Mock/demo device for development and testing — Phase 1
- [x] Dashboard showing recent recordings, trips, and events — Phase 4
- [x] Live video feed from front or rear camera with toggle — Phase 4

### Active

- [ ] List recordings filterable by event type (radar, g-shock, parking)
- [ ] Show recording metadata: thumbnail, date/time, duration, file size, avg speed, peak G-force, distance
- [ ] Recording detail view with video player and full metadata
- [ ] Combine consecutive recordings into virtual trips with same UI as single recordings
- [ ] Archive recordings/trips to local storage
- [ ] Dark mode only, responsive layout (desktop + mobile)
- [ ] Icon-only nav bar (vertical in portrait, horizontal in landscape) with device connection indicator
- [ ] SD Card mode entry point from connection indicator

### Out of Scope

- Cloud sync or remote access — local-only by design
- Light mode — dark mode only for v1
- iOS support — Android and desktop only for v1
- Real device communication protocol — using mock device for now
- Multi-device simultaneous connection — one device at a time for v1

## Context

- GPS and accelerometer data are embedded in the video files (not separate metadata)
- Dashcam recordings are short clips (seconds each); virtual trips combine consecutive clips
- Dashcams overwrite old recordings, making archiving time-sensitive
- The skeleton project (BlackBoxBuddy.slnx) already exists with dependencies configured
- Must support multiple vendors in the future — abstractions needed now
- Avalonia UI 12 RC1 introduces page-based navigation (ContentPage, NavigationPage, TabbedPage, DrawerPage, CommandBar)
- Community Toolkit MVVM for data binding and commands
- Impeccable design plugin for UI design guidelines
- Stitch MCP server for UI prototyping; Avalonia DevTools MCP for implementation verification

## Constraints

- **Tech stack**: C# 14 / .NET 10, AvaloniaUI 12 RC1, CommunityToolkit.Mvvm, xunit.v3
- **Architecture**: MVVM with DRY, KISS, SOLID, TDD, BDD, DDD principles
- **Testing**: Comprehensive test suite using xunit.v3; mock devices for unit testing
- **Multi-vendor**: Device abstractions must support future vendor additions
- **UI toolkit**: Must use Avalonia 12's new page-based navigation system
- **Design**: Impeccable plugin guidelines; Stitch for prototyping; DevTools for verification

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Mock device first, real protocol later | Decouple app development from hardware specifics | — Pending |
| Dark mode only for v1 | Simplify initial design; dashcam users often in low-light | — Pending |
| AvaloniaUI 12 RC1 | Page-based navigation system ideal for mobile + desktop | — Pending |
| Vendor abstraction layer | Multiple device vendors planned for future | — Pending |
| Local-only, no cloud | Privacy-first; dashcam data stays on user's device | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-25 after Phase 4 completion*
