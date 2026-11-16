# Hours WBS fallback map (easy to update)

Used by `hours-classify` and `hours-draft` **only when** `<SCxxxxxx-or-SOxxxxxx>/project.md`
is missing. It may hint a Medatech booking id or WBS. It is not a folder-name
map and must not be copied into webhook `project`. Prefer `project.md` in the
authoritative SC or SO folder. Do not invent an SC/SO from this table.

| Work | Priority project (example) | WBS |
|------|----------------------------|-----|
| Clarkson Evans Day Works (with PE-xx ticket) | PR230001 | 5 |
| Wider-team / Clarkson catchups | PR230001 | 1000 |
| Trutex | PR16000050 | (per map / project.md) |
| Internal comms | PR17000011 | (per map / project.md) |
| Recording hours | PR17000010 | (per map / project.md; typically WBS 1) |
| Annual leave / holiday hours (not the holiday-request form) | A3000012 | 1 (FLAG N) |
| Sickness hours | A3000013 | 1 (FLAG N) |
| Statutory / bank holiday hours | A3000011 | 1 (FLAG N) |
| Doctors / GP / similar appointment hours | P9000001 | 1 (FLAG blank; ACTDES Doctors) |
| Dentist appointment hours | P9000001 | 2 (FLAG blank; ACTDES Dentist) |
| Hospital / clinic appointment hours | P9000001 | 3 (FLAG blank; ACTDES Hospital) |

Leave, sickness and appointment hours: see also `priority-holiday-calendar-odata`. Those projects are timesheet codes, not a holiday request or Priority calendar entry. If two codes could apply, list them and wait.

Extend this table via PR when new customers/projects appear. Do not hard-code
maps inside agents. Never put secrets here.
