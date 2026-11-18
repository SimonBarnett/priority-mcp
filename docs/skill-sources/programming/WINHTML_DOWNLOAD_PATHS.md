# WINHTML output paths + DOWNLOAD / MAILMSG (SaaS and on-prem)

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Harvested 2026-11-18 from empty-DOWNLOAD diagnosis after cloud move (customer identifiers stripped). Pairs with intake harvest tip on Prefer `../../system/mail/<file>`.

## Problem

Custom procedures that `EXECUTE WINHTML … '-pdf', :OUT` (or `-m` / `-o`) then a **DOWNLOAD** or **MAILMSG … DATA** step can finish with **empty user-visible output** after a move to Priority SaaS / cloud. The SQLI often still builds `:OUT = '../../' + NAME + '.pdf'` (Priority **root**). That path is valid in SDK examples for a file that stays on the server, but DOWNLOAD / MAILMSG need a file the mail/attach pipeline can surface.

## Path conventions

Paths in WINHTML’s last file argument are **Priority-root relative** (SDK examples use `../../orders.pdf`, `../../SOMEFILENAME.pdf`).

| Intent | Path pattern | Notes |
|--------|--------------|-------|
| Server-side scratch / root file | `../../<name>.pdf` | SDK document-execution default; may not reach the user via DOWNLOAD on SaaS |
| Attachable / mail / DOWNLOAD | `../../system/mail/<name>.pdf` | Prefer when a DOWNLOAD or MAILMSG step must surface the file |
| Load / interface dirs | `../../system/load/…`, `../../tmpDir` | Other SDK patterns; not a substitute for mail when the user must receive the file |

**Rule:** when diagnosing empty DOWNLOAD after WINHTML, **read the real last path argument** in the SQLI / PROGRAMSTEXT. If it is bare `../../` + name and a DOWNLOAD or MAILMSG follows, change the write target to `../../system/mail/<name>` (same basename/extension the DOWNLOAD step expects).

`../../system/mail` holds form attachments, Priority mail, and their attachments (Medatech ops docs: Empty Mailboxes / Clear “mail” Directory). Do not hand-delete that tree in production maintenance procedures.

## SaaS vs on-prem

- **On-prem:** root-relative `../../file.pdf` often remains reachable for local operators or mapped shares; DOWNLOAD may still work depending on client path mapping.
- **SaaS / cloud:** client DOWNLOAD and mail attach paths are narrower. Prefer **`../../system/mail/<file>`** whenever the next step is DOWNLOAD or MAILMSG. Re-test DOWNLOAD after any host move.

Do not invent absolute Windows drive letters (`C:\…`) in SaaS SQLI for WINHTML output.

## Format flags (WINHTML vs Excel)

WINHTML renders **HTML document** procedures (Procedure Generator HTML Document). Common last-arg flags:

| Flag | Output | Typical extension in path |
|------|--------|---------------------------|
| `-pdf` | PDF | `.pdf` |
| `-pdfsign` | Digitally signed PDF (with `-pdf`) | `.pdf` |
| `-m` | MHTML | `.mht` |
| `-o` | HTML | `.htm` / `.html` |

Example (SDK shape; prefer mail dir when DOWNLOAD follows):

```text
:OUT = '../../system/mail/' + :CERTNO + '.pdf';
EXECUTE WINHTML '-d', 'WWWSHOWORDER', 'ORDERS', :TMPORDERS, '-pdf', :OUT;
MAILMSG 5 TO USER :USER DATA :OUT;
```

**Excel** report export is a different program path: `WINACTIVF … '-X', '<path-without-suffix>', <TEMPLATE>` (SDK). Do not expect WINHTML `-pdf` / `-m` / `-o` to produce `.xls` / `.xlsx`. If the procedure must hand the user an Excel file via DOWNLOAD, write with the Excel exporter and still place the file where DOWNLOAD / MAILMSG can see it (mail dir when that is the delivery step).

## Related

- SDK document execution: `docs/skill-sources/medatech-network/sop/SDK for Priority.md` (WINHTML `-pdf` / `-m` / `-o`, MAILMSG `DATA`)
- Procedure step archaeology: `PROC_TEXT_VIA_EPROG_SUBFORM.md`
- Procedure style: `priority-procedure-style`
- SaaS WCF client: `priority-saas-wcf`
