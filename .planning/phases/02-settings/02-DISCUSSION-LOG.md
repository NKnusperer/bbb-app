# Phase 2: Settings - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-24
**Phase:** 02-settings
**Areas discussed:** Settings data model, Save behavior, Settings organization, Danger zone UX, Unsaved changes guard

---

## Settings Data Model

| Option | Description | Selected |
|--------|-------------|----------|
| Strongly-typed models | Create typed C# records per category with mapper layer; IDeviceCommands stays dictionary-based | |
| Dictionary pass-through | Keep Dictionary<string, object> end-to-end; ViewModel reads/writes by string key | |
| Typed interface on device | Change IDeviceCommands to expose typed methods per category | |

**User's choice:** Strongly-typed models (initially), then updated to: **IDeviceCommands itself should use strongly-typed models — drop dictionary approach entirely.**
**Notes:** User explicitly overrode the initial "mapper layer" approach. The device interface should accept/return typed models directly, not dictionaries.

---

## Save Behavior — Loading Strategy

| Option | Description | Selected |
|--------|-------------|----------|
| All at once | Single GetSettingsAsync call on page entry, map to all typed models | ✓ |
| Lazy per category | Load each category when user scrolls/taps into it | |

**User's choice:** All at once
**Notes:** None

---

## Save Behavior — Persistence

| Option | Description | Selected |
|--------|-------------|----------|
| Explicit Save button | User makes changes, taps Save to persist. Batches changes. | ✓ |
| Auto-save on change | Each toggle/selection immediately calls ApplySettingsAsync | |
| Save per section | Each category has its own Save button | |

**User's choice:** Explicit Save button
**Notes:** None

---

## Save Behavior — Button Placement

| Option | Description | Selected |
|--------|-------------|----------|
| Floating/sticky at bottom | Save button at bottom, visible only when dirty | ✓ |
| In CommandBar at top | Use Avalonia 12's CommandBar at top of Settings page | |

**User's choice:** Floating/sticky at bottom
**Notes:** Button only visible when there are unsaved changes (dirty state)

---

## Settings Organization

| Option | Description | Selected |
|--------|-------------|----------|
| Single scroll with sections | One scrollable page with visual section headers | ✓ |
| Accordion/Expanders | Collapsible sections, one or few expanded at a time | |
| Category list + sub-pages | List of categories with navigation to sub-pages | |

**User's choice:** Single scroll with sections
**Notes:** None

---

## Danger Zone UX

| Option | Description | Selected |
|--------|-------------|----------|
| Confirmation dialog | Modal dialog with warning text and Cancel/Confirm buttons | ✓ |
| Type-to-confirm | User must type confirmation phrase before action proceeds | |
| Two-step button | First tap changes button text, second tap executes | |

**User's choice:** Confirmation dialog
**Notes:** None

---

## Unsaved Changes Guard

| Option | Description | Selected |
|--------|-------------|----------|
| Warn with dialog | Show "Unsaved changes" dialog with Discard / Save & Leave | ✓ |
| Discard silently | Navigate away, unsaved changes lost | |
| You decide | Claude picks based on Avalonia 12 capabilities | |

**User's choice:** Warn with dialog
**Notes:** None

---

## Cross-Cutting Decisions (User-Initiated)

### Dependency Injection Refactor
**User note:** "Views currently resolve ViewModels via a static service locator in the constructor body (Ioc.Default.GetRequiredService). This is an anti-pattern. We should use proper constructor-based dependency injection instead. No static service locator calls in Views or ViewModels."

### Comprehensive Testing
**User note:** "Tests currently only cover ViewModels and Services. Make sure to create tests for every part of the application if a unit contains any meaningful logic. This applies for ValueConverters, Views, and Controls too. Automated testing is very important."

---

## Claude's Discretion

- Exact section header styling and spacing
- Control types for each setting
- Settings mapper implementation details
- Loading indicator while settings are being fetched
- Error handling for unexpected/missing settings
- Exact dialog wording for danger zone confirmations

## Deferred Ideas

None — discussion stayed within phase scope
