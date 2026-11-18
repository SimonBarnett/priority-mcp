MRB (light triage): is the lesson useful, generalised, non-duplicate, and a fit for this repo's vision/AGENTS.md? PASS -> merge. Not useful / off-vision -> close with a one-line reason (board CLOSED not-useful). Duplicate -> close citing the existing lesson. Wrong owner -> re-file or MOVED (FR #3299); leave OPEN if the owner repo does not exist yet. Plan content only (requirements, designs, product decisions, FR/issue lists from a Plan seat) -> close 'plan content, not a lesson' (never hold it OPEN); move any how-to-plan lesson inside to SimonBarnett/skills-visionary.

## What worked
Priority SaaS OIDC email MFA completes with POST https://<host>/accounts/mfa/checkotp (not /accounts/login/checkotp which 404s). Then follow redirect chain for code and POST /auth/oauth2/token with PKCE verifier.

## Ask
Ensure priority-saas-oidc-login / SAAS_OIDC_LOGIN.md call out /accounts/mfa/checkotp as the OTP endpoint and warn that wrong path 404 does not consume OTP.
---
_via-intake id=`in_7f366fe5cf954f08` ts=`2026-11-18T13:11:46Z`_
_source machine=`flamingo` agent=`Report-FomprepIntakeIssue` book=`harvest` ver=`-`_
