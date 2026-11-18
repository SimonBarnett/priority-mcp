MRB (light triage): is the lesson useful, generalised, non-duplicate, and a fit for this repo's vision/AGENTS.md? PASS -> merge. Not useful / off-vision -> close with a one-line reason (board CLOSED not-useful). Duplicate -> close citing the existing lesson. Wrong owner -> re-file or MOVED (FR #3299); leave OPEN if the owner repo does not exist yet. Plan content only (requirements, designs, product decisions, FR/issue lists from a Plan seat) -> close 'plan content, not a lesson' (never hold it OPEN); move any how-to-plan lesson inside to SimonBarnett/skills-visionary.

## Skill / lesson
Name hint: priority-proc-text-eprog-subform (or extend priority-saas-wcf / proc text)

## What worked
Read-only export of multi-step procedure SQLI on Priority SaaS:
1. loginSession with OIDC Bearer (API License required)
2. formStart EPROG
3. setSearchFilter ENAME = procedure
4. getRows
5. startSubForm('PROG')
6. For SQLI/INPUT: startSubForm('PROGTEXT'), concatenate TEXT rows

Top-level formStart('PROG') may say unprepared; subform path still works.

## Already landed
docs/skill-sources/programming/PROC_TEXT_VIA_EPROG_SUBFORM.md (PR #695)
Intake harvest PRs #693/#694

## Ask
Promote into catalog leaflet + wire proc text fallback (see FR #696).
---
_via-intake id=`in_5b2a2d05f0ea4936` ts=`2026-11-18T13:11:39Z`_
_source machine=`flamingo` agent=`Report-FomprepIntakeIssue` book=`harvest` ver=`-`_
