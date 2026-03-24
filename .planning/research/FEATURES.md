# Feature Research

**Domain:** Dashcam companion app (local, desktop + Android)
**Researched:** 2026-03-24
**Confidence:** MEDIUM — competitor feature sets confirmed via official docs and community sources; user pain points from community forums and app store reviews

---

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Device auto-discovery via WiFi | Every major app (BlackVue, Thinkware, VIOFO) does this; users don't want manual IP entry | MEDIUM | Dashcams broadcast a WiFi AP; app scans and connects. Fallback to manual entry needed |
| Live video preview (front + rear toggle) | All competitor apps offer this; users verify camera angle and check surroundings | MEDIUM | WiFi direct stream; rear toggle is expected with dual-channel cams |
| Recording list with filtering | Core reason users open the app — find specific footage | LOW | Filter by type: event (G-shock), radar, parking, normal. File list by date |
| Thumbnail per recording | Visual scanability; all competitors show it | MEDIUM | Generated from first frame or embedded thumbnail in MP4 |
| Recording metadata (date, time, duration, size) | Standard file browser minimum; users need this to identify clips | LOW | Embedded in file; surface in list and detail views |
| Video playback with basic controls | Non-negotiable; users need to review footage | MEDIUM | Play, pause, scrub. Front/rear sync for dual-channel |
| Archive / download to local storage | Primary user job-to-be-done; dashcam overwrites old footage constantly | MEDIUM | Copy file from SD card to phone/desktop storage. Urgency is real — overwrite happens within hours on small cards |
| Device settings configuration | Users expect to configure resolution, parking mode, sensitivity from the app rather than navigating on-device menus | MEDIUM | Settings are numerous; group logically. Write to device via WiFi API |
| SD card usage indicator | Users need to know how full the card is; affects overwrite urgency | LOW | Simple capacity bar; surface on dashboard or connection indicator |
| Connection status indicator | Users must know if device is connected or not before acting | LOW | Persistent in nav bar; affects which features are available |
| Dark mode UI | Dashcam users often operate in vehicles (low-light); all apps offer dark or auto themes | LOW | PROJECT.md already commits to dark-only for v1 |

### Differentiators (Competitive Advantage)

Features that set the product apart. Not required, but valued.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Virtual trips (consecutive clip grouping) | Competitor apps show individual 1–3 min clips; treating them as a single drive session is far more natural | MEDIUM | Group clips with no gap > N seconds (configurable default; Dashcam Viewer uses 60s). Compute trip stats (distance, duration, avg speed, peak G). This is BlackBoxBuddy's core differentiator |
| Trip metadata (avg speed, peak G-force, distance) | Aggregated stats give context no competitor provides at the trip level | MEDIUM | GPS and accelerometer data is embedded in video files; must parse to aggregate. Surfaces insight without raw data dumps |
| Recording detail with full metadata | Competitors show minimal metadata in list view; a rich detail screen (speed, G, GPS position overlay) helps users assess incident severity before deciding to archive | MEDIUM | GPS trace + G-force graph in detail view adds real value post-incident |
| Provisioning wizard for new devices | Competitors assume devices are already configured; a guided first-run wizard removes friction | LOW | Detect unconfigured state, step through WiFi + basic settings. One-time flow |
| Multi-vendor abstraction | Competitors each only support their own hardware; a unified interface for multiple vendors is a long-term moat | HIGH | Requires clean device abstraction layer from day one. No user-visible in v1, but critical architectural investment |
| Radar sensor sensitivity configuration | Most consumer dashcam apps don't expose radar sensor tuning; power users want it | LOW | Already in PROJECT.md requirements; surface in settings with clear labels |
| Mock / demo mode | Allows users to explore the app without a connected device; also critical for development | LOW | Pre-populated with synthetic trips and events; no live hardware needed |
| Responsive layout (portrait + landscape, desktop + mobile) | Most dashcam apps are mobile-only or desktop-only with no layout adaptation | MEDIUM | Vertical nav bar in portrait, horizontal in landscape; Avalonia 12 handles this with page-based navigation |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Cloud sync / remote access | Users want footage accessible from anywhere | Introduces server infrastructure, privacy risk, ongoing cost, network complexity. BlackVue and Thinkware cloud services get poor reviews for reliability. PROJECT.md explicitly excludes this | Excellent local archive UX: one-tap save, clear storage status, fast download |
| Real-time push notifications (parking events) | Users want to know when their parked car is hit | Requires cloud relay or persistent network connection to vehicle; creates battery drain and complex architecture for v1 | Event-flagged recordings surfaced prominently on next app open |
| Automatic video editing / highlight reels | Compelling feature; AI-generated summaries look attractive | ML inference on video is heavyweight, device-dependent, and complex to do well. High failure rate kills trust | Clean trip view with peak G-force and event markers; let users find their own highlights |
| Light mode | Some users prefer it | Adds design and testing surface area for no user benefit in the primary use context (vehicle, often at night) | Dark mode only for v1 as per PROJECT.md |
| iOS support in v1 | Large market | Doubles platform-specific testing and Avalonia quirk exposure; defers shipping | Android + desktop v1; iOS added when core is stable |
| Multi-device simultaneous connection | Fleet or multi-cam users | Adds significant session management complexity; most users have one dashcam | Queue-based single device session; multi-device deferred to v2 |
| In-app video trimming and export | Users want to prepare clips for insurance/police | Complexity of video encoding pipeline (FFmpeg or platform codec); scope risk for v1 | Archive the full clip; users trim with their existing video tools |
| Automatic SD card formatting / management | Convenience for new card setup | Risk of data loss if triggered accidentally; hard to undo | Explicit "Danger Zone" with confirmation dialogs (already in PROJECT.md as factory reset + wipe) |

---

## Feature Dependencies

```
[Device Connection]
    └──required by──> [Live View]
    └──required by──> [Recording List]
    └──required by──> [Settings Configuration]
    └──required by──> [SD Card Indicator]

[Recording List]
    └──required by──> [Recording Detail View]
    └──required by──> [Archive to Local Storage]
    └──required by──> [Virtual Trips]

[Recording Detail View]
    └──enhanced by──> [GPS + Speed Overlay in Playback]
    └──enhanced by──> [G-Force Data Display]

[Virtual Trips]
    └──requires──> [Recording List] (trip = grouped recordings)
    └──enhanced by──> [Trip Metadata: avg speed, peak G, distance]
    └──required by──> [Archive Trip to Local Storage]

[GPS + Speed Overlay]
    └──requires──> [GPS data parsing from video file]

[Provisioning Wizard]
    └──requires──> [Device Connection] (detects unconfigured state)
    └──leads to──> [Settings Configuration]

[SD Card Indicator]
    └──feeds──> [Archive Urgency UX] (warn when card is nearly full)

[Mock Device]
    └──enables──> [All features in development/testing without hardware]
```

### Dependency Notes

- **Device Connection required by most features:** The app must establish a WiFi session before any device-facing feature works. SD card mode (direct file access) is the offline fallback.
- **Virtual Trips requires Recording List:** Trip grouping is a presentation layer over individual recordings; the list must work first.
- **GPS parsing gates several differentiators:** Trip distance, avg speed, GPS overlay all depend on successfully parsing GPS data embedded in MP4 files. This is a non-trivial implementation step and should be treated as its own task.
- **Provisioning Wizard leads to Settings Configuration:** The wizard is a one-time guided path through the same settings surfaces used day-to-day; the settings infrastructure must exist before the wizard can use it.

---

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate the concept.

- [ ] Device auto-discovery + manual fallback — without connection, nothing works
- [ ] Connection status indicator in nav bar — users need to know the app state at all times
- [ ] Live video preview (front + rear toggle) — immediate value on first connect; validates the WiFi connection works
- [ ] Provisioning wizard for unconfigured devices — first-run experience must be guided
- [ ] Recording list filtered by event type — primary reason users open the app
- [ ] Recording thumbnails + metadata (date, time, duration, size) — minimum for footage identification
- [ ] Recording detail view with video playback — users must be able to watch footage
- [ ] Archive recordings to local storage — the core job-to-be-done; footage is at risk of overwrite
- [ ] Virtual trips (consecutive clip grouping) — the primary differentiator; defines the product's identity
- [ ] Settings configuration (recording modes, channels, WiFi, shock/radar sensitivity, overlays) — complete control panel required
- [ ] SD card usage indicator — overwrite urgency must be communicated
- [ ] Mock/demo device — required for development; ships as a user-facing demo mode
- [ ] Dark mode responsive layout (portrait + landscape, desktop + Android) — as per project constraints

### Add After Validation (v1.x)

Features to add once core is working.

- [ ] GPS trace display in recording detail — adds incident context; requires GPS parsing infrastructure
- [ ] Trip metadata aggregation (avg speed, peak G, distance) — enhances virtual trips once GPS parsing is in place
- [ ] Firmware OTA update via app — useful but not essential on day one; requires device protocol support
- [ ] Settings export/import — power user convenience; low user impact in v1

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] Multi-vendor device support (Thinkware, BlackVue, VIOFO abstractions) — architecture should be ready; actual vendor integrations deferred until real device protocols are implemented
- [ ] Real device communication protocol (replace mock) — entire device layer is currently mocked; v1 validates the UX
- [ ] Multi-device session management — one device at a time is sufficient for v1
- [ ] iOS support — Android + desktop first
- [ ] Light mode — dark-only for v1

---

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Device auto-discovery + connection | HIGH | MEDIUM | P1 |
| Recording list + filtering | HIGH | LOW | P1 |
| Video playback (recording detail) | HIGH | MEDIUM | P1 |
| Archive to local storage | HIGH | LOW | P1 |
| Virtual trips | HIGH | MEDIUM | P1 |
| Live view (front + rear) | HIGH | MEDIUM | P1 |
| Settings configuration | HIGH | MEDIUM | P1 |
| Thumbnails + metadata | MEDIUM | MEDIUM | P1 |
| Provisioning wizard | MEDIUM | LOW | P1 |
| SD card indicator | MEDIUM | LOW | P1 |
| Connection indicator in nav | HIGH | LOW | P1 |
| Mock/demo device | MEDIUM | MEDIUM | P1 (dev requirement) |
| GPS trace in detail view | MEDIUM | HIGH | P2 |
| Trip aggregated stats (speed, G, distance) | MEDIUM | HIGH | P2 |
| Firmware OTA update | LOW | MEDIUM | P2 |
| Settings export/import | LOW | LOW | P3 |
| Multi-vendor support | HIGH (long term) | HIGH | P3 |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have, future consideration

---

## Competitor Feature Analysis

| Feature | BlackVue App | Thinkware CONNECTED/Link | VIOFO App | Dashcam Viewer (3rd party desktop) | BlackBoxBuddy |
|---------|--------------|--------------------------|-----------|--------------------------------------|---------------|
| WiFi direct connection | Yes | Yes (WiFi + Bluetooth) | Yes | N/A (SD card reader) | Yes |
| Live view | Yes | Yes | Yes | No | Yes |
| Recording list + playback | Yes | Yes | Yes | Yes (full-featured) | Yes |
| Thumbnails | Yes | Yes | Yes | Yes | Yes |
| Archive / download | Yes | Yes | Yes (batch download) | Yes | Yes |
| GPS map in playback | Yes | Yes | Yes | Yes (rich) | v1.x |
| Trip grouping | No (individual clips) | No | No | Yes (auto-binned) | Yes (core differentiator) |
| Trip aggregated stats | No | No | No | Basic | Yes (v1.x) |
| Settings configuration | Yes | Yes | Yes | No | Yes |
| Firmware OTA | No (cloud only) | No (PC software) | Yes | No | v1.x |
| Provisioning wizard | No | No | No | No | Yes |
| Cloud / remote access | Yes (paid) | Yes (paid) | No | No | Explicitly excluded |
| Multi-vendor | No | No | No | Some (community effort) | Architecture ready (v2) |
| Responsive desktop + mobile | No (separate apps) | No (separate apps) | Mobile only | Desktop only | Yes (Avalonia cross-platform) |
| Mock/demo mode | No | No | No | No | Yes |

---

## Sources

- [BlackVue App — Google Play](https://play.google.com/store/apps/details?id=comb.blackvuec)
- [BlackVue Cloud features overview](https://www.blackvuecloud.com/)
- [BlackVue PC Viewer — Playing and Managing Recordings](https://helpcenter.blackvue.com/hc/en-us/articles/360056125292--BlackVue-PC-Viewer-Playing-and-Managing-Video-Recordings-SD-Card-Viewer-)
- [Thinkware CONNECTED App — App Store](https://apps.apple.com/us/app/thinkware-connected/id1502055879)
- [Thinkware CONNECTED guide — BlackboxMyCar](https://www.blackboxmycar.com/pages/your-ultimate-guide-to-the-thinkware-connected-cloud-app)
- [Thinkware Dash Cam Link guide — BlackboxMyCar](https://www.blackboxmycar.com/pages/thinkware-dash-cam-link-mobile-app-guide)
- [VIOFO App — new features blog post](https://www.viofo.com/blogs/viofo-car-dash-camera-guide-faq-and-news/get-to-know-the-new-viofo-app-smart-control-for-your-dash-cam)
- [VIOFO App — Google Play](https://play.google.com/store/apps/details?id=com.viofo.dashcam)
- [Garmin Drive App features](https://www.garmin.com/en-US/p/666925/)
- [Dashcam Viewer features — dashcamviewer.com](https://dashcamviewer.com/)
- [Dashcam footage backup — TechRadar](https://www.techradar.com/vehicle-tech/dash-cams/dash-cams-overwrite-the-oldest-footage-to-make-room-for-new-recordings-heres-how-you-can-preserve-crucial-clips)
- [What's missing from dashcams — DashCamTalk forum](https://dashcamtalk.com/forum/threads/whats-missing-from-dashcams.47099/)
- [What to look for in dash cam WiFi — BlackboxMyCar](https://www.blackboxmycar.com/pages/what-to-look-for-in-your-dash-cam-wi-fi)
- [VIOFO OTA firmware update guide](https://www.viofo.com/blogs/viofo-car-dash-camera-guide-faq-and-news/viofo-app-ota-firmware-update-guide)

---
*Feature research for: dashcam management app (BlackBoxBuddy)*
*Researched: 2026-03-24*
