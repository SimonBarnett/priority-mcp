---
name: priority-harvest-intake
description: File Priority exe and systray lessons through Bob intake to SimonBarnett/priority-mcp.
---

# Priority harvest intake (PRI-04)

Priority exe lessons, tray lessons, and Priority-generic playbooks go through Bob intake.

- URL: `POST https://bob.ntsa.uk/bob/v1/intake`
- Repo: `-Repo SimonBarnett/priority-mcp`
- Tools: `tools/Invoke-FomprepHarvest.ps1` and `tools/Report-FomprepIntakeIssue.ps1`

Do not file those lessons on `SimonBarnett/bobiverse` or `SimonBarnett/a-search`.
Fleet and IRC defects still go to `SimonBarnett/bobiverse`.
Never put a token, password, or key in a filing.
