# Programming skill-sources MANIFEST

Harvest date: 2026-09-24  
Owner: Eshbel → Bob build / Eshbel hostile MRB  
FR: `docs/feature-request-eshbel-priority-programming-harvest-2026-09-24.md`

| File | Catalog skill |
|------|----------------|
| PROCEDURE_STYLE.md | priority-procedure-style | `:$.` this form, `:$$.` parent, `:$$$.` grandparent |
| FORMAT_ADDTO_DUMMY_DEBUG.md | priority-format-addto-debug | Form trigger debug: FORMAT ADDTO FROM DUMMY on one TEXTLINE; classic nvarchar(68); chronological wipe proof |
| SQL_UDATE_USER.md | priority-sql-udate-user |
| FORMPREP_SHADOW_TABLES.md | priority-formprep-shadow-tables |
| RECALC_CONCURRENCY.md | priority-recalc-concurrency |
| VERSION_REVISION.md | priority-version-revision-discipline |
| SDK_FEATURE_MAP.md | (docs only; optional catalog later) |

| DICTIONARY_SQL.md | priority-dictionary-sql | IDENTITY-safe dictionary inserts; EXECPREPLOCK / name_missing |
| CREATE_TABLE.md | priority-create-table | Physical CREATE + IGNORE_DUP_KEY; COLUMNS.SIZE never 0 |
| ODATA_REST_CLOUD.md | priority-odata-dev | Cloud REST PAT, DNAME, RESTFLAG→400, EFORM FTRIG text, typed DOCSTATS |
| SAAS_OIDC_LOGIN.md | priority-saas-oidc-login | SaaS WebUI OIDC PKCE + Accounts email MFA → Bearer token |
| SAAS_WCF_CLIENT.md | priority-saas-wcf | Allowlist-gated SaaS Web SDK client (accessToken); probe/session/form |
| IGNORE_DUP_KEY.md | priority-dictionary-sql / priority-create-table | Tabula unique indexes must use IGNORE_DUP_KEY=ON |
| FORMCLMNSA_JOINS.md | priority-dictionary-sql / priority-formprep | Child FORMCLMNSA `= :$$.COL`; joins survive Form Prep |
| FORMTRIG_VAR_TYPES.md | priority-procedure-style | One type per :VAR; INT 0 vs REAL 0.0; TRIG -1 is CHOOSE-FIELD, PRE-FORM is 7 |
| FORMPREP_SHADOW_TABLES.md | priority-formprep-shadow-tables | T$$ shadow; parent d{parent}.crs access denied (5); TYPE=F is not console prepare |
| INDEXES_TYPE_A_IDENTITY.md | priority-dictionary-sql | IDENTITY columns keep INDEXES TYPE=A (8102 if U) |
| MSG154_OPTIMISTIC_LOCK.md | priority-form-engineering / procedure-style | msg 154: mid-edit :$1 race; nullable form cols; PRE-UPDATE resync hygiene; FORMTRIGTEXT 68-char lines. Custom work-order close balance writes route to priority-service-desk-record-modified-custom-close instead. |

| EMPTY_NAMED_TRIGGER_STUB.md | Empty FORMTRIG named stub (0 FORMTRIGTEXT); restore from good instance |
| PREPARE_PROCEDURE.md | priority-procedure-prep | TYPE=P/R via EXEC + REPPREPDIRECT2; gate UPD=N + d{T$EXEC}.prp mtime |
| WINHTML_DOWNLOAD_PATHS.md | priority-winhtml-download | WINHTML last-arg paths; prefer ../../system/mail when DOWNLOAD/MAILMSG; PDF vs Excel |
| SAAS_PREP_GATE_ODATA_404.md | priority-saas-wcf (docs) | prepare ok needs walkOk + preparedClaim; odata_http_404 gate |
| PROC_TEXT_VIA_EPROG_SUBFORM.md | priority-saas-wcf (docs) | EPROG→PROG→PROGTEXT when PROG unprepared / EXEC TEXT empty |
