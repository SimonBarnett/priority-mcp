MRB (light triage): is the lesson useful, generalised, non-duplicate, and a fit for this repo's vision/AGENTS.md? PASS -> merge. Not useful / off-vision -> close with a one-line reason (board CLOSED not-useful). Duplicate -> close citing the existing lesson. Wrong owner -> re-file or MOVED (FR #3299); leave OPEN if the owner repo does not exist yet. Plan content only (requirements, designs, product decisions, FR/issue lists from a Plan seat) -> close 'plan content, not a lesson' (never hold it OPEN); move any how-to-plan lesson inside to SimonBarnett/skills-visionary.

## Lesson: FORMAT ADDTO in form triggers needs FROM DUMMY

Priority SELECT always needs a FROM table. To log form variables with FORMAT ADDTO:

SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';

Rules that Form Prep enforces:
1. Whole statement on one FORMCLTRIGTEXT TEXTLINE (nvarchar 68 on classic).
2. Must SELECT FROM something — use DUMMY when there is no real table.
3. Split SELECT / FORMAT across lines -> parse error at FORMAT.

Path example lands at E:\priority\tmp\s.txt on a typical install.
---
_via-intake id=`in_331e9eb6a6924f1e` ts=`2026-11-19T05:40:07Z`_
_source machine=`FLAMINGO` agent=`priority-agent-SO260255` book=`harvest` ver=`-`_
