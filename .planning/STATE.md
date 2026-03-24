# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-24)

**Core value:** Users can effortlessly manage their dashcam footage — browse recordings, combine them into trips, and archive important moments before the dashcam overwrites them.
**Current focus:** Phase 1 — Foundation

## Current Position

Phase: 1 of 4 (Foundation)
Plan: 0 of TBD in current phase
Status: Ready to plan
Last activity: 2026-03-24 — Roadmap created; ready to plan Phase 1

Progress: [░░░░░░░░░░] 0%

## Performance Metrics

**Velocity:**
- Total plans completed: 0
- Average duration: -
- Total execution time: -

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| - | - | - | - |

**Recent Trend:**
- Last 5 plans: -
- Trend: -

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- Mock device first, real protocol later — decouples all development from hardware
- Vendor abstraction layer — `IDashcamDevice` composes five narrow interfaces; new vendors added without touching services or ViewModels
- Avalonia 12 RC1 — page-based navigation; pin exact NuGet versions immediately; isolate behind `INavigationService`
- No single cross-platform video player — `IMediaPlayerService` must be defined in Phase 1 before any playback UI is built in Phase 3

### Pending Todos

None yet.

### Blockers/Concerns

- LibVLCSharp 3.9.6 has `Avalonia >= 11.0.4` constraint — verify it accepts `12.0.0-rc1` at first package restore in Phase 3; fallback is `LibVLCSharp.Avalonia.Unofficial` (jpmikkers)
- Android Smart Network Switch silently redirects traffic off dashcam WiFi — must bind `HttpClient` socket to WiFi interface via `ConnectivityManager.bindProcessToNetwork()` in Phase 4

## Session Continuity

Last session: 2026-03-24
Stopped at: Roadmap created; no plans written yet
Resume file: None
