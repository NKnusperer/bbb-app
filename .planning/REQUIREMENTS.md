# Requirements: BlackBoxBuddy

**Defined:** 2026-03-24
**Core Value:** Users can effortlessly manage their dashcam footage — browse recordings, combine them into trips, and archive important moments before the dashcam overwrites them.

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### Foundation

- [x] **FOUND-01**: App shell uses adaptive layout — vertical nav in portrait, horizontal nav in landscape
- [x] **FOUND-02**: Navigation bar uses icon-only centered icons
- [x] **FOUND-03**: Device connection indicator visible in nav bar at all times (Searching / Connected / Disconnected)
- [x] **FOUND-04**: App uses dark mode exclusively
- [x] **FOUND-05**: All content visible regardless of screen size or aspect ratio (responsive)
- [x] **FOUND-06**: Extended and compact views adapt to available screen space

### Device Connection

- [x] **CONN-01**: Dashcam is automatically discovered on app startup
- [x] **CONN-02**: If auto-discovery fails, user can manually configure the connection
- [x] **CONN-03**: Connection indicator serves as entry point to manual connection setup
- [x] **CONN-04**: Connection indicator provides entry point to SD Card mode
- [x] **CONN-05**: Mock/demo device available for development and testing

### Provisioning

- [x] **PROV-01**: If discovered device is unconfigured, user is guided through provisioning wizard
- [x] **PROV-02**: Provisioning wizard walks user through essential device setup steps

### Dashboard

- [ ] **DASH-01**: Dashboard shows recent recordings
- [ ] **DASH-02**: Dashboard shows recent trips
- [ ] **DASH-03**: Dashboard shows recent events

### Settings — WiFi

- [x] **WIFI-01**: User can configure WiFi band (2.4 GHz or 5 GHz)
- [x] **WIFI-02**: User can connect device to an existing access point
- [x] **WIFI-03**: User can configure device to act as an access point

### Settings — Recording Modes

- [x] **RMOD-01**: User can set driving mode to Standard (front + rear @ 30 fps)
- [x] **RMOD-02**: User can set driving mode to Racing (front only @ 60 fps)
- [x] **RMOD-03**: User can set parking mode to Standard (radar monitoring, medium sensitivity)
- [x] **RMOD-04**: User can set parking mode to Event Only (vibration-triggered only)

### Settings — Recording Channels

- [x] **CHAN-01**: User can configure recording to front camera only
- [x] **CHAN-02**: User can configure recording to front and rear cameras

### Settings — Camera

- [x] **CAMR-01**: User can set rear camera orientation to 0 degrees
- [x] **CAMR-02**: User can set rear camera orientation to 180 degrees

### Settings — Sensors

- [x] **SENS-01**: User can configure driving shock sensor sensitivity (1-5)
- [x] **SENS-02**: User can configure parking shock sensor sensitivity (1-5)
- [x] **SENS-03**: User can configure radar sensor sensitivity (1-5)

### Settings — System

- [x] **SYST-01**: User can enable/disable GPS
- [x] **SYST-02**: User can enable/disable microphone
- [x] **SYST-03**: User can set speaker volume (disabled, 1-5)

### Settings — Video Overlays

- [x] **OVRL-01**: User can enable/disable date overlay
- [x] **OVRL-02**: User can enable/disable time overlay
- [x] **OVRL-03**: User can enable/disable GPS position overlay
- [x] **OVRL-04**: User can enable/disable speed overlay
- [x] **OVRL-05**: User can choose speed display unit (km/h or mph)

### Settings — Danger Zone

- [x] **DNGR-01**: User can perform factory reset on device
- [x] **DNGR-02**: User can wipe SD card

### Live Feed

- [ ] **LIVE-01**: User can view live video feed from front camera
- [ ] **LIVE-02**: User can view live video feed from rear camera
- [ ] **LIVE-03**: User can toggle between front and rear camera feeds

### Recordings

- [x] **RECD-01**: User can view list of all recordings
- [ ] **RECD-02**: User can filter recordings by Radar Event type
- [ ] **RECD-03**: User can filter recordings by G-Shock Event type
- [ ] **RECD-04**: User can filter recordings by Parking Event type
- [x] **RECD-05**: Each recording shows video thumbnail in list view
- [x] **RECD-06**: Each recording shows date and time
- [x] **RECD-07**: Each recording shows duration
- [x] **RECD-08**: Each recording shows file size
- [x] **RECD-09**: Each recording shows average speed
- [x] **RECD-10**: Each recording shows peak G-force
- [x] **RECD-11**: Each recording shows traveled distance

### Recording Detail

- [ ] **RDTL-01**: User can play back recording with video player
- [ ] **RDTL-02**: Recording detail shows video thumbnail
- [ ] **RDTL-03**: Recording detail shows date and time
- [ ] **RDTL-04**: Recording detail shows duration
- [ ] **RDTL-05**: Recording detail shows file size
- [ ] **RDTL-06**: Recording detail shows average speed
- [ ] **RDTL-07**: Recording detail shows peak G-force
- [ ] **RDTL-08**: Recording detail shows traveled distance

### Virtual Trips

- [ ] **TRIP-01**: App automatically combines consecutive recordings into virtual trips
- [ ] **TRIP-02**: Virtual trip uses same UI as single recording detail
- [ ] **TRIP-03**: Virtual trip shows aggregated metadata (duration, distance, avg speed, peak G-force)

### Archive

- [ ] **ARCH-01**: User can archive a single recording to local storage
- [ ] **ARCH-02**: User can archive a virtual trip to local storage
- [ ] **ARCH-03**: Archive operation downloads content from dashcam to device storage

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Telemetry

- **TELM-01**: GPS trace overlay in recording detail view (requires real hardware GPS parsing)
- **TELM-02**: Real-time GPS data display during live feed

### Multi-Vendor

- **VEND-01**: Support for additional dashcam vendors beyond mock device
- **VEND-02**: Real device communication protocol implementation

### Platform

- **PLAT-01**: iOS support
- **PLAT-02**: Multi-device simultaneous connection

### Advanced

- **ADVN-01**: Firmware OTA updates
- **ADVN-02**: Settings import/export
- **ADVN-03**: Light mode theme

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Cloud sync / remote access | Local-only by design; privacy-first positioning |
| Real-time chat / social sharing | Not relevant to dashcam management |
| AI-powered event detection | High complexity; dashcam hardware handles detection |
| Map view with GPS traces | Requires real GPS parsing; deferred to v2 with telemetry |
| Video editing / trimming | Out of core value; users archive raw footage |
| Multi-device simultaneous management | One device at a time for v1 simplicity |
| Custom recording schedules | Dashcam handles scheduling natively |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| FOUND-01 | Phase 1 | Complete |
| FOUND-02 | Phase 1 | Complete |
| FOUND-03 | Phase 1 | Complete |
| FOUND-04 | Phase 1 | Complete |
| FOUND-05 | Phase 1 | Complete |
| FOUND-06 | Phase 1 | Complete |
| CONN-01 | Phase 1 | Complete |
| CONN-02 | Phase 1 | Complete |
| CONN-03 | Phase 1 | Complete |
| CONN-04 | Phase 1 | Complete |
| CONN-05 | Phase 1 | Complete |
| PROV-01 | Phase 1 | Complete |
| PROV-02 | Phase 1 | Complete |
| WIFI-01 | Phase 2 | Complete |
| WIFI-02 | Phase 2 | Complete |
| WIFI-03 | Phase 2 | Complete |
| RMOD-01 | Phase 2 | Complete |
| RMOD-02 | Phase 2 | Complete |
| RMOD-03 | Phase 2 | Complete |
| RMOD-04 | Phase 2 | Complete |
| CHAN-01 | Phase 2 | Complete |
| CHAN-02 | Phase 2 | Complete |
| CAMR-01 | Phase 2 | Complete |
| CAMR-02 | Phase 2 | Complete |
| SENS-01 | Phase 2 | Complete |
| SENS-02 | Phase 2 | Complete |
| SENS-03 | Phase 2 | Complete |
| SYST-01 | Phase 2 | Complete |
| SYST-02 | Phase 2 | Complete |
| SYST-03 | Phase 2 | Complete |
| OVRL-01 | Phase 2 | Complete |
| OVRL-02 | Phase 2 | Complete |
| OVRL-03 | Phase 2 | Complete |
| OVRL-04 | Phase 2 | Complete |
| OVRL-05 | Phase 2 | Complete |
| DNGR-01 | Phase 2 | Complete |
| DNGR-02 | Phase 2 | Complete |
| RECD-01 | Phase 3 | Complete |
| RECD-02 | Phase 3 | Pending |
| RECD-03 | Phase 3 | Pending |
| RECD-04 | Phase 3 | Pending |
| RECD-05 | Phase 3 | Complete |
| RECD-06 | Phase 3 | Complete |
| RECD-07 | Phase 3 | Complete |
| RECD-08 | Phase 3 | Complete |
| RECD-09 | Phase 3 | Complete |
| RECD-10 | Phase 3 | Complete |
| RECD-11 | Phase 3 | Complete |
| RDTL-01 | Phase 3 | Pending |
| RDTL-02 | Phase 3 | Pending |
| RDTL-03 | Phase 3 | Pending |
| RDTL-04 | Phase 3 | Pending |
| RDTL-05 | Phase 3 | Pending |
| RDTL-06 | Phase 3 | Pending |
| RDTL-07 | Phase 3 | Pending |
| RDTL-08 | Phase 3 | Pending |
| TRIP-01 | Phase 3 | Pending |
| TRIP-02 | Phase 3 | Pending |
| TRIP-03 | Phase 3 | Pending |
| ARCH-01 | Phase 3 | Pending |
| ARCH-02 | Phase 3 | Pending |
| ARCH-03 | Phase 3 | Pending |
| LIVE-01 | Phase 4 | Pending |
| LIVE-02 | Phase 4 | Pending |
| LIVE-03 | Phase 4 | Pending |
| DASH-01 | Phase 4 | Pending |
| DASH-02 | Phase 4 | Pending |
| DASH-03 | Phase 4 | Pending |

**Coverage:**
- v1 requirements: 68 total
- Mapped to phases: 68
- Unmapped: 0

---
*Requirements defined: 2026-03-24*
*Last updated: 2026-03-24 after roadmap creation*
