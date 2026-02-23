# ReportCardBuilder – UI Design Rules

> These are the laws of the UI. Follow them on every screen so the app feels
> cohesive no matter who built which page.

---

## 1. Color Usage

| Token | Use for |
|---|---|
| `Primary` (#1B4F72) | Primary actions, active nav items, links |
| `Primary Light` (#2E86C1) | Hover states, info chips, secondary emphasis |
| `Primary Pale` (#D6EAF8) | Info alert backgrounds, subtle highlights |
| `Secondary / Accent` (#E67E22) | Call-to-action buttons, logo icon, avatar backgrounds |
| `Surface` (#F4F6F9) | Page background, table header rows |
| `Surface Card` (#FFFFFF) | Card/panel backgrounds |
| `Border` (#DDE3EC) | All dividers, input borders, table lines |
| `Success` (#1E8449) | Active badges, completion states |
| `Warning` (#D4AC0D) | Caution badges, locked state indicators |
| `Error` (#C0392B) | Validation errors, danger actions |

**Rules:**
- Never use the accent orange for anything other than the logo, avatars, and explicit CTAs
- Never use raw black (`#000`) anywhere — always use `TextPrimary` (#1A2B3C)
- Sidebar background is always `Primary` (#1B4F72) — never white or grey

---

## 2. Typography

| Use | Style |
|---|---|
| Page titles (H1/H2) | DM Serif Display, normal weight |
| Section headings (H3–H4) | DM Sans, 700 weight |
| Body / labels | DM Sans, 400–500 weight |
| Table headers | DM Sans, 700, uppercase, 11px, letter-spacing |
| Overline / nav section labels | DM Sans, 700, uppercase, 10–11px, muted |
| Button text | DM Sans, 600, **no ALL CAPS** |
| Validation errors | DM Sans, 11px, Error color |

**Rules:**
- DM Serif Display is **only** for H1/H2 — never for body text or buttons
- Button text is sentence case — never `SAVE STUDENT`, always `Save Student`
- Don't use font sizes below 11px anywhere

---

## 3. Spacing

| Context | Value |
|---|---|
| Page padding (content area) | `pa-6` (24px) |
| Card internal padding | `pa-5` (20px) |
| Card header padding | `16px 20px` |
| Form field gap | `16px` |
| Section gap (between cards) | `20px` |
| Inline icon + label gap | `8–10px` |
| Nav item padding | `9px 12px` |

**Rules:**
- Always use MudBlazor spacing helpers (`pa-`, `ma-`, `gap-`) rather than raw pixel values in style attributes
- Content area max width: `MaxWidth.ExtraExtraLarge` — don't constrain admin screens unnecessarily

---

## 4. Cards & Panels

All content lives inside `<MudPaper>` or `<MudCard>` with:
- `Elevation="1"` for standard cards
- `Elevation="3"` for dialogs and popovers only
- `Class="rounded-lg"` (matches `DefaultBorderRadius: 8px`)
- A card header section (title + optional action button) separated from content by a `<MudDivider />`

**Rules:**
- Never put raw content directly on the page background — always wrap in a card
- Card headers always have a `MudText Typo.h6` title and optional `MudText Typo.caption` subtitle
- Action buttons in card headers are always `Size.Small`

---

## 5. Buttons

| Variant | When to use |
|---|---|
| `Variant.Filled` + `Color.Primary` | Primary action (one per card/section max) |
| `Variant.Filled` + `Color.Secondary` | Strong CTA (generate report, bulk promote) |
| `Variant.Outlined` + `Color.Default` | Secondary / cancel actions |
| `Variant.Text` + `Color.Primary` | Inline links ("Manage →", "View →") |
| `Variant.Filled` + `Color.Error` | Destructive actions (delete, inactivate) — always behind a confirm dialog |

**Rules:**
- Maximum **one** filled primary button per page section
- Destructive actions always show a `<MudDialog>` confirmation — never fire immediately
- Icon buttons (`MudIconButton`) are for topbar and utility actions only — not inside forms

---

## 6. Tables

All data tables use `<MudDataGrid>` or `<MudTable>` with:
- `Dense="false"` (standard row height)
- `Hover="true"`
- `Striped="false"` (border lines are enough)
- `Elevation="0"` (card provides the elevation)
- Column headers: uppercase, 11px, muted color, `font-weight:700`
- Row actions: use `Variant.Text` link-style buttons ("Manage →")

**Rules:**
- Every table must have a loading state (`Loading="@_isLoading"`)
- Every table must handle the empty state with a friendly message + icon
- Pagination is required for any list that could exceed 20 rows

---

## 7. Forms

- All forms use `<EditForm>` with `<DataAnnotationsValidator>` and `<MudTextField>` / `<MudSelect>`
- Required fields are marked with `Required="true"` — MudBlazor handles the asterisk
- Validation messages appear inline below the field (never in a toast)
- Form action buttons always sit at the **bottom right** of the form: `[Cancel] [Save]`
- Cancel is always `Variant.Outlined`, Save is always `Variant.Filled Color.Primary`

---

## 8. Badges & Status Chips

| State | Color | Icon |
|---|---|---|
| Active | `Color.Success`, `Variant.Filled` | ● |
| Locked | `Color.Warning`, `Variant.Filled` | 🔒 |
| Inactive | `Color.Default`, `Variant.Outlined` | — |
| Template | `Color.Info`, `Variant.Outlined` | — |
| Custom | `Color.Secondary`, `Variant.Outlined` | — |

Use `<MudChip>` with `Size.Small` for all status indicators in tables.

---

## 9. Alerts & Feedback

| Type | Component | When |
|---|---|---|
| Info | `<MudAlert Severity.Info>` | Non-urgent context (upcoming term, next steps) |
| Warning | `<MudAlert Severity.Warning>` | Action needed before proceeding |
| Error | `<MudAlert Severity.Error>` | Operation failed |
| Success | `<MudSnackbar>` (toast) | Action completed successfully — auto-dismisses |

**Rules:**
- Persistent alerts (`MudAlert`) live at the top of the content area, below the page header
- Transient feedback (save success, delete confirmed) always uses `MudSnackbar` — never a persistent alert
- Error messages from the API always surface in a `MudAlert Severity.Error` — never silently swallowed

---

## 10. Navigation & Routing

- Sidebar nav sections are labeled with overline text (`Admin`, `People`, `Reports`)
- Active nav item is highlighted automatically by MudBlazor's `NavLink` match
- The **Working Year** chip in the topbar is always visible — clicking it opens the year selector
- Breadcrumbs update on every page to reflect current location
- Page transitions: none (keep it snappy for an admin tool)

---

## 11. The "Never Do" List

- ❌ No ALL CAPS button text
- ❌ No raw black (`#000`) anywhere
- ❌ No content directly on the page background (always in a card)
- ❌ No destructive actions without a confirm dialog
- ❌ No tables without loading + empty states
- ❌ No inline styles for spacing (use MudBlazor helpers)
- ❌ No purple, teal, or random accent colors — stick to the palette
- ❌ No more than one primary filled button per section
