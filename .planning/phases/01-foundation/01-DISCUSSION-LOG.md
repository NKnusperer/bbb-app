# Phase 1: Foundation - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-24
**Phase:** 01-foundation
**Areas discussed:** Navigation shell, Device abstraction, Connection flow, Provisioning wizard, DI and app bootstrap
**Mode:** Auto (all areas auto-selected, recommended defaults chosen)

---

## Navigation Shell

| Option | Description | Selected |
|--------|-------------|----------|
| TabbedPage + Auto placement | Avalonia 12 TabbedPage with TabPlacement=Auto, icon-only, connection indicator as persistent element | ✓ |
| Custom shell with manual breakpoints | Build adaptive layout from scratch with Bounds-based switching | |
| DrawerPage navigation | Hamburger menu pattern with drawer | |

**User's choice:** [auto] TabbedPage + Auto placement (recommended default)
**Notes:** Research confirmed TabbedPage supports TabPlacement="Auto" in Avalonia 12 RC1. Breakpoint at 600px for portrait/landscape. Connection indicator lives outside tab content area.

---

## Device Abstraction

| Option | Description | Selected |
|--------|-------------|----------|
| Interface segregation (5 narrow) | IDeviceDiscovery, IDeviceConnection, IDeviceCommands, IDeviceFileSystem, IDeviceLiveStream composed into IDashcamDevice | ✓ |
| Fat interface | Single IDashcamDevice with all methods | |
| Capability-based | Feature flags per device, single interface with optional methods | |

**User's choice:** [auto] Interface segregation (recommended per Architecture research)
**Notes:** Architecture research explicitly recommends this pattern. Avoids NotImplementedException sprawl when future vendors have differing capabilities.

---

## Connection Flow

| Option | Description | Selected |
|--------|-------------|----------|
| 3-state indicator + 5s auto-discovery | Searching/Connected/Disconnected states, 5-second auto-discovery timeout, then manual fallback | ✓ |
| Immediate manual connection | Skip auto-discovery, always require manual setup | |
| Background persistent discovery | Continuously scan in background, connect when found | |

**User's choice:** [auto] 3-state indicator + 5s auto-discovery (recommended default)
**Notes:** Tap on indicator opens manual connection (CONN-03) and SD Card mode (CONN-04). Manual entry accepts IP/hostname.

---

## Provisioning Wizard

| Option | Description | Selected |
|--------|-------------|----------|
| Linear 3-step wizard | Welcome → WiFi setup → Confirmation, using NavigationPage | ✓ |
| Single-page setup | All provisioning on one scrollable page | |
| Branching wizard | Different paths based on device capabilities | |

**User's choice:** [auto] Linear 3-step wizard (recommended default)
**Notes:** "Unconfigured" = mock device isProvisioned=false. After provisioning, navigate to Dashboard.

---

## DI and App Bootstrap

| Option | Description | Selected |
|--------|-------------|----------|
| MS.Extensions.DI + Ioc.Default | Microsoft DI with CommunityToolkit.Mvvm Ioc.Default, pattern-matching ViewLocator | ✓ |
| Pure CommunityToolkit | Manual service creation without DI container | |
| Third-party DI (Autofac, etc.) | External DI framework | |

**User's choice:** [auto] MS.Extensions.DI + Ioc.Default (recommended default)
**Notes:** Shared ConfigureServices called from both head projects. ViewLocator replaced with switch expression. INavigationService abstraction for RC API stability.

---

## Claude's Discretion

- Icon set selection (Material, Fluent, or custom)
- Loading animation style
- Additional responsive breakpoints
- Error handling patterns
- Test naming conventions

## Deferred Ideas

None — all discussion stayed within Phase 1 scope.
