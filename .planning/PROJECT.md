# BlackBoxBuddy

## What This Is

A cross-platform desktop (PC) and mobile (Android) application for configuring automotive dashcams and managing their recordings. Everything runs locally — no cloud services. The app auto-discovers connected dashcams, guides users through device provisioning, and provides a unified interface for settings, live feeds, and footage management.

## Core Value

Users can effortlessly manage their dashcam footage — browse recordings, combine them into trips, and archive important moments before the dashcam overwrites them.

## Requirements

### Validated

- [x] Auto-discover dashcam on startup; fall back to manual connection if discovery fails — v1.0
- [x] Guide unconfigured devices through provisioning — v1.0
- [x] Configure WiFi (2.4/5 GHz, AP client or AP mode) — v1.0
- [x] Configure recording modes (driving: standard/racing; parking: standard/event-only) — v1.0
- [x] Configure recording channels (front only, front+rear) — v1.0
- [x] Configure rear camera orientation (0/180 degrees) — v1.0
- [x] Configure shock sensor sensitivity (driving 1-5, parking 1-5) — v1.0
- [x] Configure radar sensor sensitivity (1-5) — v1.0
- [x] Configure system settings (GPS, microphone, speaker volume) — v1.0
- [x] Configure video overlays (date, time, GPS position, speed in km/h or mph) — v1.0
- [x] Danger zone: factory reset, wipe SD card — v1.0
- [x] Mock/demo device for development and testing — v1.0
- [x] Dashboard showing recent recordings, trips, and events — v1.0
- [x] Live video feed from front or rear camera with toggle — v1.0
- [x] List recordings filterable by event type (radar, g-shock, parking) — v1.0
- [x] Show recording metadata: thumbnail, date/time, duration, file size, avg speed, peak G-force, distance — v1.0
- [x] Recording detail view with video player and full metadata — v1.0
- [x] Combine consecutive recordings into virtual trips — v1.0
- [x] Archive recordings/trips to local storage — v1.0
- [x] Dark mode only, responsive layout (desktop + mobile) — v1.0
- [x] Icon-only nav bar (vertical in portrait, horizontal in landscape) with device connection indicator — v1.0
- [x] SD Card mode entry point from connection indicator — v1.0

### Active

(None — next milestone requirements defined via `/gsd:new-milestone`)

### Out of Scope

- Cloud sync or remote access — local-only by design; privacy-first positioning
- Light mode — dark mode only for v1; revisit based on user feedback
- iOS support — Android and desktop only for v1
- Real device communication protocol — using mock device; deferred to v2
- Multi-device simultaneous connection — one device at a time for v1
- AI-powered event detection — dashcam hardware handles detection
- Video editing / trimming — out of core value; users archive raw footage

## Context

Shipped v1.0 with 8,336 LOC (C# + AXAML) across 185 files.
Tech stack: C# 14 / .NET 10, AvaloniaUI 12 RC1, CommunityToolkit.Mvvm, LibVLCSharp, xunit.v3.
192 unit tests passing (1 pre-existing ArchiveService test failure tracked).
All functionality uses a mock dashcam device — real hardware protocol deferred to v2.

## Constraints

- **Tech stack**: C# 14 / .NET 10, AvaloniaUI 12 RC1, CommunityToolkit.Mvvm, xunit.v3
- **Architecture**: MVVM with DRY, KISS, SOLID, TDD, BDD, DDD principles
- **Testing**: Comprehensive test suite using xunit.v3; mock devices for unit testing
- **Multi-vendor**: Device abstractions must support future vendor additions
- **UI toolkit**: Must use Avalonia 12's new page-based navigation system

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Mock device first, real protocol later | Decouple app development from hardware specifics | Good — enabled full v1.0 without hardware |
| Dark mode only for v1 | Simplify initial design; dashcam users often in low-light | Good — consistent aesthetic |
| AvaloniaUI 12 RC1 | Page-based navigation system ideal for mobile + desktop | Good — ContentPage/TabbedPage worked well |
| Vendor abstraction layer | Multiple device vendors planned for future | Good — IDashcamDevice composes 5 narrow interfaces |
| Local-only, no cloud | Privacy-first; dashcam data stays on user's device | Good — simplifies architecture |
| VideoView inlined from LibVLCSharp | LibVLCSharp.Avalonia uses removed VisualRoot API | Good — TopLevel.GetTopLevel port works on Avalonia 12 |
| DashboardViewModel manual construction | Requires per-instance Action callbacks for cross-tab wiring | Good — same pattern as ManualConnectionViewModel |
| C# record types for settings | Value equality enables dirty-state comparison without custom comparers | Good — clean OnPropertyChanged override pattern |
| RecordingDetail as inline overlay | Avoids NavigationService push; keeps recording list context | Good — faster UX, simpler state management |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition:**
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone:**
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-25 after v1.0 milestone*
