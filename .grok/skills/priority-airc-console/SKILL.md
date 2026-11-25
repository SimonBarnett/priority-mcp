---
name: priority-airc-console
description: >-
  Read an on-prem Priority through the AIRC client db2_console. The bob ear
  on this machine PRIVMSGs that nick. Dictionary is the system database.
  Use when connecting to Priority SQL, reading procedures, or the user says
  AIRC, db2_console, or #wonderland.
---

# AIRC console for on-prem Priority

You already know this. Do not ask the operator what AIRC is or how to connect.

The launcher no longer has an AIRC menu option, and `--airc` / `--airc-console` are rejected. That only removed the menu. When the operator directs you to use AIRC, use it.

AIRC is the bobiverse console (`SimonBarnett/bobiverse`, tree `airc`). The shell is PowerShell.

## Connect

1. The bob ear is already on the machine you are running on, in `#wonderland`.
2. Append one line with `Add-BobiverseOutboxLine.ps1` to `<AI root>\bob\home\outbox.txt`:

```
PRIVMSG db2_console :hostname
```

3. Read the reply in the bob ear inbound transcript. Lines look like `db2_console out id=... seq=...` and end with `db2_console DONE id=... exit=...`.
4. `{machine}_console` is the console on the agent machine only. A machine id named `airc` is not a host. Do not `Invoke-AircRemote -MachineId airc`.

## Limits

- The PowerShell text must be 400 bytes or under. Longer returns `code=too_long`.
- The remote is PowerShell. `$` is expanded. Escape it as `` `$ `` so a name such as `T$EXEC` arrives whole.
- About 48 lines come back. A bigger result is truncated. Filter in SQL or write a file on that host and print a short extract.
- Do not print passwords. Do not put secrets in the PRIVMSG.

This does not change connection records or the WCF allowlist. SaaS form prep still uses `priority-wcf.mjs` when an allowlist row exists.
