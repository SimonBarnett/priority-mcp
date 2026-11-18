# SaaS Form Prep gate when OData dictionary returns 404 (FR #698)

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

## Problem

On Priority SaaS, `prepare form|procedure` can finish the WCF walk (`ended`, `formprepErrs: 0`, success message) while `preparedClaim` stays false because the OData prep gate got `odata_http_404` (or similar) for the configured dictionary entity. Company `odataBase` may answer the service document while `FORMS` / `EXECPREPLOCK`-style entities are missing on that company.

## Rules

1. **`ok` requires `walkOk` and `preparedClaim`.** Never treat walk success alone as prepared.
2. **`prepare_gate_unreachable` / `gate.status: odata_http_404`** means the dictionary re-read failed. Fix `odataPrepPath` / `odataPrepFallbacks`, or accept that this company cannot gate via OData (`--no-gate` only when the operator knowingly waives the CAST IRON claim).
3. **Do not re-prepare in a loop** hoping `preparedClaim` flips while the entity stays 404.
4. **Procedure Steps (`PROG`)** may stay unprepared as a top-level form after a green walk. For read-only step SQLI archaeology, use `EPROG` → `PROG` → `PROGTEXT` (`PROC_TEXT_VIA_EPROG_SUBFORM.md`).

## Allowlist knobs

- `odataBase` — company OData root (required for the gate).
- `odataPrepPath` / `odataPrepEntity` — primary entity (default `FORMS`).
- `odataPrepFallbacks` — array or comma-separated list tried after HTTP 404/405 on the primary.

## Related

- `v2/apps/priority-saas-wcf/lib/gate.mjs` / `prepare.mjs`
- `PROC_TEXT_VIA_EPROG_SUBFORM.md`
