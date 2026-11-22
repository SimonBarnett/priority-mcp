# PRI-05 MSI gaps vs launcher-v2.0.0

Checked release `launcher-v2.0.0` (Priority Agent launcher v2.0.0) on 2026-11-22.

## Assets

https://github.com/SimonBarnett/priority-mcp/releases/tag/launcher-v2.0.0

- `PriorityAgent.exe`
- `PriorityAgent.exe.sha256`

There is no `PriorityAgent.msi` on that tag.

## Verdict

N/A for `launcher-v2.0.0`. The release notes say the loose exe stays and an MSI is an additional asset only when packed. This tag did not pack one. That gap is recorded here so it is not an undocumented hole. Closing it means a later tag that publishes `PriorityAgent.msi`, which is outside this slice.

## Tray and log path

The launcher writes `%LOCALAPPDATA%\PriorityAgent\logs\launcher-YYYYMMDD.log` (`launcher/Program.cs` joins `PriorityAgent` and `logs`). A tray or exe consumer records the installed build on that path. `launcher-v2.0.0` does not add an MSI install line because no MSI was published.
