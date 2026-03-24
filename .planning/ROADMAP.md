# Roadmap: BlackBoxBuddy

## Overview

BlackBoxBuddy is built in four phases: first a solid app shell with device contracts and mock device so nothing is ever blocked on hardware; then device settings so the dashcam is fully configurable; then the core recording pipeline — list, detail, virtual trips, and archive — which delivers the primary user value; and finally live feed and the dashboard summary screen that ties everything together.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Foundation** - App shell, device abstractions, mock device, connection flow, and provisioning wizard
- [ ] **Phase 2: Settings** - Full device configuration — WiFi, recording modes, channels, sensors, overlays, and danger zone
- [ ] **Phase 3: Recordings** - Recording list with filtering, detail view with playback, virtual trips, and archive to local storage
- [ ] **Phase 4: Live Feed and Dashboard** - Live camera feed with front/rear toggle and dashboard summary of recent activity

## Phase Details

### Phase 1: Foundation
**Goal**: Users can launch the app, see connection status at all times, connect to a dashcam (auto or manual), and complete first-time device provisioning
**Depends on**: Nothing (first phase)
**Requirements**: FOUND-01, FOUND-02, FOUND-03, FOUND-04, FOUND-05, FOUND-06, CONN-01, CONN-02, CONN-03, CONN-04, CONN-05, PROV-01, PROV-02
**Success Criteria** (what must be TRUE):
  1. App opens with a dark-mode responsive shell that adapts from portrait (vertical nav) to landscape (horizontal nav) on both desktop and Android
  2. Connection indicator is always visible in the nav bar and shows one of three states: Searching, Connected, or Disconnected
  3. On startup the app attempts auto-discovery and connects to the mock device without user intervention
  4. If auto-discovery fails, user can tap the connection indicator to manually configure the connection
  5. When an unconfigured device is discovered, the app navigates to a provisioning wizard that walks the user through essential setup
**Plans**: 4 plans

Plans:
- [x] 01-01-PLAN.md — DI bootstrap, AOT-safe ViewLocator, NuGet packages, device interface contracts
- [ ] 01-02-PLAN.md — MockDashcamDevice implementation, DeviceService with auto-discovery, unit tests
- [ ] 01-03-PLAN.md — App shell UI with TabbedPage, connection indicator, responsive layout, placeholder pages
- [ ] 01-04-PLAN.md — Connection flow (auto-discovery, manual connection), provisioning wizard, visual verification

**UI hint**: yes

### Phase 2: Settings
**Goal**: Users can configure every aspect of their dashcam — WiFi, recording behavior, sensors, video overlays, and perform danger-zone operations — all persisted to the device
**Depends on**: Phase 1
**Requirements**: WIFI-01, WIFI-02, WIFI-03, RMOD-01, RMOD-02, RMOD-03, RMOD-04, CHAN-01, CHAN-02, CAMR-01, CAMR-02, SENS-01, SENS-02, SENS-03, SYST-01, SYST-02, SYST-03, OVRL-01, OVRL-02, OVRL-03, OVRL-04, OVRL-05, DNGR-01, DNGR-02
**Success Criteria** (what must be TRUE):
  1. User can navigate to a Settings screen and configure WiFi band and AP mode/client mode
  2. User can set driving mode (Standard or Racing) and parking mode (Standard or Event Only)
  3. User can adjust all sensor sensitivities (driving shock, parking shock, radar) using a 1-5 scale
  4. User can toggle any video overlay (date, time, GPS, speed) and choose speed unit (km/h or mph)
  5. User can perform factory reset or SD card wipe from a clearly labelled Danger Zone section after a confirmation prompt
**Plans**: TBD
**UI hint**: yes

### Phase 3: Recordings
**Goal**: Users can browse all recordings, filter by event type, watch any clip, view full metadata, see consecutive clips grouped as virtual trips, and archive important footage before it is overwritten
**Depends on**: Phase 1
**Requirements**: RECD-01, RECD-02, RECD-03, RECD-04, RECD-05, RECD-06, RECD-07, RECD-08, RECD-09, RECD-10, RECD-11, RDTL-01, RDTL-02, RDTL-03, RDTL-04, RDTL-05, RDTL-06, RDTL-07, RDTL-08, TRIP-01, TRIP-02, TRIP-03, ARCH-01, ARCH-02, ARCH-03
**Success Criteria** (what must be TRUE):
  1. User can view a list of all recordings, each showing thumbnail, date/time, duration, file size, average speed, peak G-force, and distance
  2. User can filter the recording list to show only Radar, G-Shock, or Parking events
  3. User can tap a recording to open a detail view with a working video player and full metadata
  4. Consecutive recordings are automatically grouped into virtual trips with aggregated stats, accessible from the same detail UI as a single recording
  5. User can archive a single recording or an entire trip to local device storage
**Plans**: TBD
**UI hint**: yes

### Phase 4: Live Feed and Dashboard
**Goal**: Users can watch a live camera feed from either camera and see a dashboard summary of recent recordings, trips, and events
**Depends on**: Phase 3
**Requirements**: LIVE-01, LIVE-02, LIVE-03, DASH-01, DASH-02, DASH-03
**Success Criteria** (what must be TRUE):
  1. User can navigate to a Live Feed screen and see a real-time video stream from the front camera
  2. User can toggle between front and rear camera feeds without leaving the Live Feed screen
  3. User can view a Dashboard showing recent recordings, recent trips, and recent events at a glance
**Plans**: TBD
**UI hint**: yes

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 0/4 | Planning complete | - |
| 2. Settings | 0/TBD | Not started | - |
| 3. Recordings | 0/TBD | Not started | - |
| 4. Live Feed and Dashboard | 0/TBD | Not started | - |
