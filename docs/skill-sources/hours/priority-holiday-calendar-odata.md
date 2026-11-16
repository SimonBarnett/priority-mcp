# Priority holiday request and calendar via OData

Harvest date: 2026-11-16
From: Haitch (agent for Simon Barnett)
Instance checked: Medatech live OData `https://priority.medatecherp.co.uk/odata/Priority/tabula.ini/live/`
No secrets. No personal medical detail.

Catalog: `v2/apps/mcp-catalog/catalog/priority-holiday-calendar-odata`

## What was asked

Book Friday 20 Nov 2026 off on the Priority holiday form, and add a full-day Holiday entry to the Priority calendar (not Outlook). Do not post a `TRANSORDER_q` hours row unless asked.

## Discovery

Full `/$metadata` timed out (~120s). The service document (collection root, 2027 entity sets) is the reliable catalogue.

Holiday-related sets: `ZEMG_HOLREQUESTS`, `ZEMG_HOLREQUESTSA`, `ZEMG_HOLREQSTATS`, `ZEMG_HOLGROUPS`, `ZEMG_EXTRAREASONS`, plus `EMG_ABSENTS`, `USERSBABSENTCHART`, `HOLIDAYS`, `ABSENTCODES`. No holiday `*_q` query set.

Calendar-related sets: no `CALENDAR` / `USERCALENDAR`. Appointments live on the Tasks form (`TODOLIST`, `TODOLISTB`) and diaries (`GENDIARY`, `DAILYDIARY`, `WEEKLYDIARY`, `DIARYYEARS`). This matches the Medatech "Calendar Script" / Tasks form documentation.

## API gap (do not work around)

GET `?$top=1` on every holiday-request, absence, diary and `TODOLIST` set returned HTTP 400:

`API cannot be run for this form.`

The forms are in the service document but are not API-enabled. Existing user rows could not be read, so no OData POST was made. A human can book in the Priority UI; an admin can enable the form for the API, after which agents should copy the user's existing rows.

## Hours vs request vs calendar

`TRANSORDER_q` is timesheet hours only. Observed full-day holiday hours (not a request): project `A3000012` (Proj Annual Leave), WBS `1`, FLAG `N`, description `Holiday` or `Annual Leave`, typically 8.0h. That does not create a holiday request or a calendar appointment.

Appointment / absence hours codes (generic; confirm on the instance; search the user's history; do not guess if two codes fit):

- `P9000001` Proj Doctors/Hospital, FLAG omitted: WBS 1 Doctors, WBS 2 Dentist, WBS 3 Hospital (`ACTDES` on the WBS).
- `A3000012` WBS 1 FLAG N: annual leave / holiday (and sometimes appointments booked as leave).
- `A3000013` WBS 1 FLAG N: sickness.
- `A3000011` WBS 1 FLAG N: statutory / bank holidays.

## Module guide (UI, not OData)

`docs/skill-sources/medatech-network/customer-guides/user-guides/Human Resources/Employee Holidays Users Guide - Rich.md` describes Human Resources > Employee Holidays, holiday requests and authorisation, and that approved requests populate Employee Absences. It does not enable the OData API.
