# Phase 3: Recordings - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-24
**Phase:** 03-recordings
**Areas discussed:** Recording data model, Recording list layout, Video playback approach, Virtual trips & archive

---

## Recording Data Model

### Device metadata return approach

| Option | Description | Selected |
|--------|-------------|----------|
| Rich Recording model | Replace IReadOnlyList<string> with IReadOnlyList<Recording> record type with all metadata fields. Mock device generates fake data using Bogus. | ✓ |
| Separate metadata call | Keep paths, add GetRecordingMetadataAsync(path) per file. Two-step fetch. | |
| You decide | Claude picks | |

**User's choice:** Rich Recording model
**Notes:** None

### Event type categorization

| Option | Description | Selected |
|--------|-------------|----------|
| Enum with None | EventType enum: None, Radar, GShock, Parking. Simple filter buttons. | ✓ |
| Flags enum | [Flags] enum for multiple event types per recording. | |
| You decide | Claude picks | |

**User's choice:** Enum with None
**Notes:** None

### Mock thumbnails

| Option | Description | Selected |
|--------|-------------|----------|
| Generated color blocks | Small solid-color bitmap thumbnails (160x90) in-memory, color-coded by event type. | ✓ |
| Embedded resource images | Bundle static placeholder thumbnail images as embedded resources. | |
| Null thumbnails with placeholder | Mock returns null, UI shows generic icon. | |
| You decide | Claude picks | |

**User's choice:** Generated color blocks
**Notes:** None

### Mock recording count

| Option | Description | Selected |
|--------|-------------|----------|
| 15-20 recordings | Mix of event types, spread across days, consecutive sequences for trips. | ✓ |
| 5-8 recordings | Minimal set. | |
| 50+ recordings | Stress-test. | |

**User's choice:** 15-20 recordings
**Notes:** None

---

## Recording List Layout

### List display style

| Option | Description | Selected |
|--------|-------------|----------|
| Card rows | Horizontal card with thumbnail left, metadata right, event badge. | ✓ |
| Compact list rows | Dense text-only rows, no thumbnails in list. | |
| Grid/tile layout | Thumbnail-first grid of tiles. | |

**User's choice:** Card rows
**Notes:** None

### Event type filtering

| Option | Description | Selected |
|--------|-------------|----------|
| Toggle chip bar | Horizontal chips: All, Radar, G-Shock, Parking. Single-select. | ✓ |
| Dropdown/ComboBox | Single dropdown selector. | |
| You decide | Claude picks | |

**User's choice:** Toggle chip bar
**Notes:** None

### Trip display in list

| Option | Description | Selected |
|--------|-------------|----------|
| Inline with grouping | Trip header card with aggregated stats, individual clips nested below. | ✓ |
| Flat list only | All recordings flat, trips only visible in detail view. | |
| Separate trips tab | Second tab/toggle for Clips vs Trips. | |

**User's choice:** Inline with grouping
**Notes:** None

### Sorting

| Option | Description | Selected |
|--------|-------------|----------|
| Newest first | Fixed sort, most recent at top. No controls. | ✓ |
| Sort toggle | Toggle between newest-first and oldest-first. | |
| You decide | Claude picks | |

**User's choice:** Newest first
**Notes:** None

---

## Video Playback Approach

### Mock device video strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Embedded test video | Bundle small (~2-5 sec) real MP4 as embedded resource. Real LibVLCSharp testing. | ✓ |
| Player placeholder only | Static image or "No video available". Defer LibVLCSharp integration. | |
| You decide | Claude picks | |

**User's choice:** Embedded test video
**Notes:** None

### Detail view navigation

| Option | Description | Selected |
|--------|-------------|----------|
| NavigationPage push | Push RecordingDetailPage onto stack with back button. | ✓ |
| In-page expansion | Master-detail inline expansion. | |
| You decide | Claude picks | |

**User's choice:** NavigationPage push
**Notes:** None

### Dual camera display

| Option | Description | Selected |
|--------|-------------|----------|
| Single player, channel toggle | One video at a time with Front/Rear toggle button. | |
| Dual player side-by-side | Both front and rear simultaneously. Side-by-side on desktop, stacked on mobile. | ✓ |
| Front only, rear linked | Always show front, rear as linked item. | |

**User's choice:** Dual player side-by-side
**Notes:** More ambitious but makes sense for incident review.

### Player controls

| Option | Description | Selected |
|--------|-------------|----------|
| Standard controls | Play/pause, seek bar, time display, fullscreen toggle. | |
| Extended controls | Standard plus playback speed (0.5x, 1x, 2x) and frame-by-frame step. | ✓ |
| You decide | Claude picks | |

**User's choice:** Extended controls
**Notes:** Valuable for reviewing collision/incident footage.

---

## Virtual Trips & Archive

### Trip grouping logic

| Option | Description | Selected |
|--------|-------------|----------|
| Time gap threshold | Consecutive if next starts within 30 seconds of previous ending. | ✓ |
| Same-minute grouping | Group by hour block or sequential filenames. | |
| You decide | Claude picks | |

**User's choice:** Time gap threshold (30 seconds)
**Notes:** None

### Archive UX

| Option | Description | Selected |
|--------|-------------|----------|
| Default location with option | Download to ~/BlackBoxBuddy/Archives/ with progress bar. No file picker by default. | ✓ |
| File picker every time | Folder picker dialog each time. | |
| You decide | Claude picks | |

**User's choice:** Default location with option
**Notes:** None

### Trip archive approach

| Option | Description | Selected |
|--------|-------------|----------|
| Batch download | Archive Trip downloads all clips into trip subfolder. | ✓ (modified) |
| Individual clip archive | Archive clips one by one. | |
| You decide | Claude picks | |

**User's choice:** Batch download — but modified: user most likely wants to archive a few consecutive recordings related to an incident, not the whole trip. Multi-select mode is essential.
**Notes:** User clarified that incident-focused archiving (selecting specific clips) is more important than full trip archiving.

### Archive state indication

| Option | Description | Selected |
|--------|-------------|----------|
| Archived badge | Checkmark or "Archived" badge on saved recordings. | ✓ |
| No indicator | No tracking of archive state in the list. | |
| You decide | Claude picks | |

**User's choice:** Archived badge
**Notes:** None

### Multi-select pattern

| Option | Description | Selected |
|--------|-------------|----------|
| Multi-select mode | Long-press or "Select" button enters multi-select. Check clips, "Archive Selected". "Select All" for full trip. | ✓ |
| Individual archive buttons | Each clip has own Archive button. | |
| You decide | Claude picks | |

**User's choice:** Multi-select mode
**Notes:** None

---

## Claude's Discretion

- Card styling, spacing, responsive breakpoints
- LibVLCSharp integration details
- Loading states and error handling
- Frame-step implementation specifics
- Archive folder naming and file organization
- Progress bar styling
- Mock data distribution

## Deferred Ideas

None — discussion stayed within phase scope
