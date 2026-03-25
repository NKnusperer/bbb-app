# Phase 4: Live Feed and Dashboard - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-25
**Phase:** 04-live-feed-and-dashboard
**Areas discussed:** Live feed controls, Dashboard layout, Dashboard interaction, Live feed status overlay

---

## Live Feed Controls

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal | Camera toggle (front/rear) only. No seek, speed, or frame-step. Clean, distraction-free view. | ✓ |
| Minimal + mute | Camera toggle plus mute/unmute button for audio stream. | |
| Dual view | Both cameras simultaneously (side-by-side desktop, stacked mobile). | |

**User's choice:** Minimal — camera toggle only
**Notes:** None

### Camera Toggle UX

| Option | Description | Selected |
|--------|-------------|----------|
| Segmented control | Two-segment toggle bar (Front / Rear) below video. Matches filter chip pattern. | ✓ |
| Tap video to switch | Single tap on video toggles cameras. Less discoverable. | |
| Swipe to switch | Swipe left/right. Gesture-based, native feel on mobile. | |

**User's choice:** Segmented control
**Notes:** None

---

## Dashboard Layout

| Option | Description | Selected |
|--------|-------------|----------|
| Three sections | Scrollable page with Recent Recordings, Recent Trips, Recent Events. 3-5 compact cards each. Section headers with "See All". | ✓ |
| Unified timeline | Single chronological feed mixing all items with type badges. | |
| Summary cards + list | Top-level stat cards followed by compact recent activity list. | |

**User's choice:** Three sections
**Notes:** None

### Item Detail Level

| Option | Description | Selected |
|--------|-------------|----------|
| Compact card | Thumbnail + date/time + duration + event type badge. | ✓ |
| Full metadata | Same detail as RecordingsPage cards (all metadata fields). | |
| Text-only rows | No thumbnails. Date/time, duration, event type as text. | |

**User's choice:** Compact card
**Notes:** None

---

## Dashboard Interaction

| Option | Description | Selected |
|--------|-------------|----------|
| Open detail view | Navigate directly to RecordingDetailPage for that item. | ✓ |
| Jump to recordings tab | Switch to Recordings tab with item highlighted. | |
| Inline preview | Expand card inline with small thumbnail player. | |

**User's choice:** Open detail view
**Notes:** None

### "See All" Behavior

| Option | Description | Selected |
|--------|-------------|----------|
| Switch to Recordings tab | Navigate to Recordings tab. Events section auto-applies filter. | ✓ |
| Push filtered list page | Push new page showing only that category. Back returns to dashboard. | |

**User's choice:** Switch to Recordings tab
**Notes:** None

---

## Live Feed Status Overlay

| Option | Description | Selected |
|--------|-------------|----------|
| Camera label only | Small "Front" or "Rear" label in corner. | |
| Camera + status | Camera label plus connection indicator and "LIVE" badge. | |
| Full HUD | Camera label, LIVE badge, connection quality, timestamp. | |
| No overlay | No overlay at all — clean video. | ✓ |

**User's choice:** No video overlay. Keep it simple.
**Notes:** User explicitly wanted zero overlay for simplicity.

### Connection Loss

| Option | Description | Selected |
|--------|-------------|----------|
| Replace video with state | Full-screen placeholder: "Connection lost" with retry button. Segmented control stays visible. | ✓ |
| Overlay on frozen frame | Semi-transparent overlay on last frame with "Reconnecting..." spinner. | |

**User's choice:** Replace video with state
**Notes:** None

---

## Additional Context (User-Provided)

**LibVLCSharp.Avalonia Compatibility:**
LibVLCSharp.Avalonia NuGet package does not work with Avalonia 12. The VideoView.cs source is available at:
`https://code.videolan.org/videolan/LibVLCSharp/-/raw/3.x/src/LibVLCSharp.Avalonia/VideoView.cs?ref_type=heads`

Decision: Remove the NuGet package, inline VideoView.cs, and port it to Avalonia 12 compatibility.

## Claude's Discretion

- Segmented control styling
- Compact card dimensions and responsive breakpoints
- Connection loss placeholder design
- Dashboard loading states
- VideoView.cs Avalonia 12 migration approach

## Deferred Ideas

None — discussion stayed within phase scope
