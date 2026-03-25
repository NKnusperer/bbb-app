# Retrospective: BlackBoxBuddy

## Milestone: v1.0 — MVP

**Shipped:** 2026-03-25
**Phases:** 4 | **Plans:** 17 | **Timeline:** 11 days

### What Was Built
- Dark-mode responsive app shell with adaptive layout, auto-discovery, manual connection, provisioning wizard
- Full device settings (20+ properties) with dirty-state tracking, danger zone, unsaved-changes navigation guard
- Recording management with filtering, virtual trip grouping, detail view with VLC video playback, multi-select archive
- Live camera feed (front/rear toggle) with connection-loss handling and in-app VideoView rendering
- Dashboard with recent recordings, trips, events and cross-tab navigation

### What Worked
- Mock device first approach enabled all 4 phases without hardware
- Wave-based parallel execution for Phase 4 (2 XAML plans ran concurrently)
- CommunityToolkit.Mvvm source generators reduced boilerplate significantly
- C# record types for settings gave value equality for free (dirty-state tracking)
- Inlining VideoView.cs from LibVLCSharp avoided NuGet compatibility issues with Avalonia 12 RC1

### What Was Inefficient
- VideoView rendering required 6 fix commits to get right — NativeControlHost timing, visibility, and wiring issues
- NavigationService was creating generic ContentPages instead of using ViewLocator — latent bug from Phase 1
- RecordingDetail shown as inline overlay (not NavigationService push) — required separate VideoView wiring path
- ROADMAP.md phase checkboxes got out of sync with actual completion state

### Patterns Established
- `VideoViewHelper` for cross-platform NativeControlHost creation via reflection
- `IsVisibleProperty.Changed.AddClassHandler<T>` for tab lifecycle (no System.Reactive dependency)
- `Task.Delay(1)` before VLC Play() to yield for NativeControlHost layout pass
- DashboardViewModel manual construction with Action callbacks for cross-tab wiring
- `Loaded` event for first-tab initialization (AddClassHandler doesn't fire when IsVisible never changes)

### Key Lessons
- NativeControlHost needs a layout pass before its native handle exists — always create VideoViews early and yield before playback
- When a recording can be opened from Dashboard (tab 0) before Recordings tab (tab 1) loads, PropertyChanged events fire before subscribers exist — check initial state when subscribing
- Avalonia 12 RC1 TabbedPage keeps all pages alive but only measures visible ones

## Cross-Milestone Trends

| Metric | v1.0 |
|--------|------|
| Phases | 4 |
| Plans | 17 |
| LOC | 8,336 |
| Tests | 192 |
| Timeline | 11 days |
