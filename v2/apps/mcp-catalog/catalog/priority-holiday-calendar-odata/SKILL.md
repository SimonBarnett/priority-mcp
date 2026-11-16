---
name: priority-holiday-calendar-odata
description: >-
  Book or investigate a Priority holiday request and a Priority calendar entry via OData:
  discover ZEMG_HOLREQUESTS and TODOLIST from the service document, copy the user's
  existing rows only if GET works, never POST when the form returns 400 "API cannot be
  run for this form", and do not substitute TRANSORDER_q hours for a holiday request.
  Also maps generic leave and appointment hours codes (annual leave, sick, doctors/hospital).
  Use when /priority-holiday-calendar-odata, "book holiday", "holiday form",
  "Priority calendar", or "book Friday off".
---

# Priority holiday request and calendar via OData

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Grab from catalog MCP `https://mcp-priority.ntsa.uk/mcp` (`get_skill` with `name=priority-holiday-calendar-odata`).

Authoritative harvest: `docs/skill-sources/hours/priority-holiday-calendar-odata.md` (2026-11-16).

## Auth / endpoint (do not hardcode secrets)

- Medatech OData base: `https://priority.medatecherp.co.uk/odata/Priority/tabula.ini/live/` (other instances: operator allowlist).
- Username is the session employee's OData login (example: `SimonB`). Password from env `PRIORITY_ODATA_PASSWORD` - never printed, logged or committed.
- Live writes only after the owner approves a draft. Read first. Do not invent fields.

## 1. Discover forms (do not fetch full $metadata first)

Full `/$metadata` can time out. GET the service document (the collection root, Accept `application/json`) and filter entity names.

Holiday / absence names on Medatech live (16 Nov 2026):

- `ZEMG_HOLREQUESTS` - Medatech Employee Holidays request form (and `ZEMG_HOLREQUESTSA`, statuses `ZEMG_HOLREQSTATS`, groups `ZEMG_HOLGROUPS`, extra reasons `ZEMG_EXTRAREASONS`).
- Related, different purpose: `EMG_ABSENTS`, `USERSBABSENTCHART`, `HOLIDAYS`, `ABSENTCODES`.
- There is no `ZEMG_HOLREQUESTS_q` (or other `*_q` holiday query) in the service document.

Priority calendar / diary names:

- There is no `CALENDAR` or `USERCALENDAR` entity set. The Calendar UI records appointments on the Tasks form.
- Entity sets: `TODOLIST`, `TODOLISTB`, `GENDIARY`, `DAILYDIARY`, `WEEKLYDIARY`, `DIARYYEARS`.

## 2. Probe before any write

GET `<EntitySet>?$top=1`.

- HTTP 200: read the user's existing rows (filter on their login / employee fields as the rows show), copy that pattern, draft, get approval, POST once, read back.
- HTTP 400 with message `API cannot be run for this form.`: the set is listed but **not API-enabled**. Stop. Do not POST. Do not retry with guessed query names. Tell the owner the form must be used in the Priority UI (or an admin must enable the form for the API) before an agent can book it.

**Medatech live, 16 Nov 2026:** every holiday-request, absence, diary and `TODOLIST` set above returned that 400. No SimonB (or other) precedent rows could be read, so no request and no calendar row were created via OData.

## 3. Do not substitute timesheet hours for a holiday request

`TRANSORDER_q` is reporting hours, not the holiday form and not the calendar.

- A holiday **request** (when the API is enabled) is `ZEMG_HOLREQUESTS`. Approved requests populate Employee Absences (module guide).
- A Priority **calendar** entry (when the API is enabled) is the Tasks / diary form, not Outlook, unless the owner explicitly asks for Outlook.
- After leave is taken, hours are often booked to **A3000012** (Proj Annual Leave), WBS **1**, FLAG **N**, description `Holiday` or `Annual Leave`. A full weekday is typically **8.0** hours (or the shortfall to 8.0 if other lines exist). Confirm against that user's recent `A3000012` rows. Post hours only when the owner asks for hours.
- Booking `TRANSORDER_q` does not create a holiday request or a Priority calendar appointment.

## 4. Appointment and absence hours (TRANSORDER_q only)

When the owner asks to book **hours** for leave, sickness, or an appointment, search their historical `TRANSORDER_q` rows first. If two codes could apply, list the candidates and wait; do not guess. Do not put clinical detail in the description or in this skill.

Typical Medatech internal projects (confirm on the instance; WBS titles from `ACTDES`):

| Project | Name | WBS | FLAG | Use |
|---------|------|-----|------|-----|
| P9000001 | Proj Doctors/Hospital | 1 | blank (omit FLAG; do not send N) | Doctors / GP / bloods / pharmacy / optician (`ACTDES` Doctors) |
| P9000001 | Proj Doctors/Hospital | 2 | blank | Dentist (`ACTDES` Dentist) |
| P9000001 | Proj Doctors/Hospital | 3 | blank | Hospital / clinic (`ACTDES` Hospital) |
| A3000012 | Proj Annual Leave | 1 | N | Holiday, annual leave; sometimes a personal appointment booked as leave |
| A3000013 | Proj Sick | 1 | N | Illness, not a routine appointment |
| A3000011 | Proj Statutory Holidays | 1 | N | Bank holidays |

`P9000001` rows historically have FLAG unset. POST the body without a `FLAG` property (same pattern as other hours posts, minus FLAG).

Also listed in `docs/skill-sources/hours/wbs-fallback-map.md`.

## Do not

- Print or commit the OData password.
- POST to a form that returned 400 "API cannot be run for this form".
- Invent a holiday `*_q` entity that is not in the service document.
- Book `TRANSORDER_q` hours when the owner asked only for a holiday request or a Priority calendar entry.
- Add an Outlook event when they asked for the Priority calendar (and do not decline or cancel existing meetings).
- Record personal medical details in the repo, a skill, or a hours description beyond a short generic label such as "Doctor's appointment".
