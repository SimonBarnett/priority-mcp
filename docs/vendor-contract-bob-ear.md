# Vendor contract: Priority agent for bob-ear

Contract version: 1

Canonical URL: https://github.com/SimonBarnett/priority-mcp/blob/main/docs/vendor-contract-bob-ear.md

## BOB-50

Agents > Priority starts `PriorityAgent.exe`.

Current published build: launcher-v2.0.0

Download: https://github.com/SimonBarnett/priority-mcp/releases/download/launcher-v2.0.0/PriorityAgent.exe

Checksum: https://github.com/SimonBarnett/priority-mcp/releases/download/launcher-v2.0.0/PriorityAgent.exe.sha256

Icon path for the tray: `launcher/priority.ico`

## BOB-55

The bob-ear MSI includes that same `PriorityAgent.exe` plus the embed unit `embed/bob-ear/` (PRI-06, contract version 1). The MSI does not include the Plan agent.

Depends-on: PRI-06 (SimonBarnett/priority-mcp#737).
