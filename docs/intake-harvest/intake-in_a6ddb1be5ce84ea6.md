MRB (light triage): is the lesson useful, generalised, non-duplicate, and a fit for this repo's vision/AGENTS.md? PASS -> merge. Not useful / off-vision -> close with a one-line reason (board CLOSED not-useful). Duplicate -> close citing the existing lesson. Wrong owner -> re-file or MOVED (FR #3299); leave OPEN if the owner repo does not exist yet. Plan content only (requirements, designs, product decisions, FR/issue lists from a Plan seat) -> close 'plan content, not a lesson' (never hold it OPEN); move any how-to-plan lesson inside to SimonBarnett/skills-visionary.

## Lesson: debug form LOCNAME wipes with FORMAT ADDTO FROM DUMMY

### Syntax that Form Preps (classic nvarchar(68) TEXTLINE)
Whole statement on ONE TEXTLINE; SELECT must have FROM (use DUMMY for form vars):

SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';

Lands at E:\priority\tmp\s.txt on a typical tree. Path must fit 68 chars with the rest of the line.

### Failures
- SELECT on one line, FORMAT ADDTO on the next -> parse error at or near symbol FORMAT.
- SELECT of :$.vars with no FROM -> Form Prep rejects (must SELECT FROM; use DUMMY).
- FORMCLTRIGTEXT.TEXT nvarchar(68): longer INSERT -> Msg 8152 truncation.

### How to use
Instrument ACTNAME POST, WARHS POST, WARHSNAME POST (around #INCLUDE BUF*), and CHECK-FIELD. Chronological ADDTO rows show which step first writes LOCNAME to 0.

### Related wipe pattern (warehouse transfer)
Nested WARHSNAME -> WAREHOUSES/BUF1 can default bin to 0. Also LOCNAME can already be 0 at start of WARHS POST (before BUF21). Re-assert stock bin AFTER BUF21 and AFTER BUF1 with a stock join (ACT/WARHSBAL/WAREHOUSES, BALANCE>0, LOCNAME<>'0'). Do not use WHERE WARHS=:$.WARHS alone after BUF1 (form WAREHOUSES buffer returns 0).
---
_via-intake id=`in_a6ddb1be5ce84ea6` ts=`2026-11-19T06:12:21Z`_
_source machine=`FLAMINGO` agent=`priority-agent-SO260255` book=`harvest` ver=`-`_
