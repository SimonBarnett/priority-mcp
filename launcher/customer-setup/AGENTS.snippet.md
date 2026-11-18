## Hours and reporting

Hours are reported only through the Bob hours webhook
`https://bob.ntsa.uk/bob/v1/hours` (contract: SimonBarnett/bobiverse
`jeeves/docs/webhooks.md`, [hours webhook FR #3450](https://github.com/SimonBarnett/bobiverse/blob/main/jeeves/docs/webhooks.md#hours-webhook-fr-3450--no-secret)), following
SimonBarnett/priority-mcp `launcher/agent-rules.md`. PriorityAgent loads
those rules. Never store or report hours in this repo: no hours files,
folders, tables or hours entry ids.

## This repo's data

`project` folders are evidenced `SCxxxxxx` or `SOxxxxxx` ids. Booking id and
WBS are `priority_project` and `wbs` in that folder's `project.md`. They are
not the work sentence. Secrets stay out of git.

Keep [`company.md`](company.md) current: it is the company knowledge file,
linked from the README. Agents add what they learn about the company there.
[`connections.md`](connections.md) is secret-free and points at
`C:\passwords\<client>-connections.txt` on MarchHare. Never dump that file
to stdout.
