---
name: priority-winhtml-download
description: >
  WINHTML last-arg path conventions on Priority SaaS vs on-prem: prefer
  ../../system/mail/<name> when a DOWNLOAD or MAILMSG step follows; PDF vs
  MHTML/HTML flags; Excel is WINACTIVF not WINHTML. Use when empty DOWNLOAD
  after WINHTML, cloud-move document procedures, or /priority-winhtml-download.
---

# WINHTML output paths + DOWNLOAD

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Grab from catalog MCP `https://mcp-priority.ntsa.uk/mcp` (`get_skill` with `name=priority-winhtml-download`).

Authority: `docs/skill-sources/programming/WINHTML_DOWNLOAD_PATHS.md`.

## When

Authoring or diagnosing procedures that `EXECUTE WINHTML` then DOWNLOAD or MAILMSG, especially after a SaaS / cloud move when the user sees empty output.

## Hard rules

1. **Read the real last path arg.** Empty DOWNLOAD after WINHTML usually means the write path is bare `../../<file>` (Priority root) while delivery expects an attachable location.
2. **Prefer `../../system/mail/<name>`** when a DOWNLOAD or MAILMSG `DATA` step must surface the file to the user. Keep the same basename/extension the DOWNLOAD step names.
3. **Paths are Priority-root relative** (`../../…`). Do not invent absolute drive letters in SaaS SQLI for WINHTML output.
4. **WINHTML format flags:** `-pdf` (PDF), `-pdfsign` with `-pdf` (signed PDF), `-m` (MHTML `.mht`), `-o` (HTML). Match the extension in the path to the flag.
5. **Excel is not WINHTML.** Report Excel export uses `WINACTIVF … '-X', …`. Use that path when the deliverable is `.xls` / `.xlsx`, then still place the file where DOWNLOAD / MAILMSG can see it.
6. **SaaS is stricter than on-prem.** Root-relative files that worked on a LAN share often fail user DOWNLOAD after cloud move; re-test delivery after host changes.

## Related

- Authority doc: `docs/skill-sources/programming/WINHTML_DOWNLOAD_PATHS.md`
- Procedure step text: `PROC_TEXT_VIA_EPROG_SUBFORM.md` / `priority-saas-wcf` `proc text`
- Procedure style: **priority-procedure-style**
- SaaS client: **priority-saas-wcf**
