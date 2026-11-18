MRB (light triage): is the lesson useful, generalised, non-duplicate, and a fit for this repo's vision/AGENTS.md? PASS -> merge. Not useful / off-vision -> close with a one-line reason (board CLOSED not-useful). Duplicate -> close citing the existing lesson. Wrong owner -> re-file or MOVED (FR #3299); leave OPEN if the owner repo does not exist yet. Plan content only (requirements, designs, product decisions, FR/issue lists from a Plan seat) -> close 'plan content, not a lesson' (never hold it OPEN); move any how-to-plan lesson inside to SimonBarnett/skills-visionary.

Generic Priority lesson (strip customer names in downstream harvest).

When a column POST-FIELD does SELECT INTO that sets both :$.WARHSNAME and :$.LOCNAME, WARHSNAME/POST-FIELD often #INCLUDEs WAREHOUSES/BUF1. BUF1 sets :$.LOCNAME to the literal '0' when the warehouse has a bin named 0 and LOCNAME is still empty at fire time. That clobbers or pre-empts the stock bin from the same parent SELECT.

Fix pattern: omit LOCNAME from the first SELECT INTO (fill SERIALNAME/QUANT/WARHSNAME/CUSTNAME), then SELECT WAREHOUSES.LOCNAME INTO :$.LOCNAME alone with the same WARHSBAL filters so BUF1 does not run again.

Evidence path was warehouse-transfer ACTNAME ZAMB_POST-FIELD style fill. Form Prep still required after dictionary edit.
---
_via-intake id=`in_4a77bda269f44670` ts=`2026-11-18T14:41:17Z`_
_source machine=`flamingo` agent=`Report-FomprepIntakeIssue` book=`harvest` ver=`-`_
