# Priority Agent launcher

`PriorityAgent.exe` is a Windows console launcher. It starts a **Grok coding agent for Priority ERP work**
with the right context.

## Interactive console UX

**PickList (Up/Down/Enter).** On a cursor-capable console, `ConsolePick.PickList` draws the menu once. Up/Down (wrapping) and Home/End update only the old and new selected rows in place; selection has a coloured background and a `>` marker. Enter selects it; Esc cancels (returns null). Both leave the cursor below the menu. Redirected input/output, unsupported terminals, or menus too tall for the window use a single numbered list and line input instead. The customer menu rows are General, then customer names, then New (New still Ask Haitch). The project / service-call menu uses the same picker. The CLI menu remains a later FR. `/airc` (FR #635) uses the same PickList for connected airc hosts.

**Clear between questions (FR #687).** On an interactive TTY (stdin/stdout not redirected), each `PickList` and each `Ask` clears the console before showing the next prompt so prior answers do not scroll into one long screen. Redirected stdin/stdout and `--mcp-catalog` never clear.

Non-interactive flags are unchanged: `--customer`, `--general`, and `--project` skip the picker. With stdin redirected, a numeric line still picks (1 = General, 2 = first customer). Do not print the picker on `--mcp-catalog` JSON-RPC stdout.

**Wait spinner (`|` `/` `-` `\`).** `ConsoleWait.WithWait` cycles those glyphs on one TTY line while work runs, then settles to "done". A failed hours-list GET, fuel-report GET, or `ListCustomers` gh api does not print "done". Output stays quiet when stdin/stdout is redirected, or when `--mcp-catalog` sets `ConsoleWait.Quiet` before serve.

Call sites today: `ListCustomers` (gh api topic list, including `--list`), hours list GET (`hours fetch`), Bob fuel report GET (`fuel report`), `/airc` host roster fetch (`airc hosts`), MCP setup (`grok mcp list` / `add` / `doctor`), and skills registration (`grok inspect`). `--no-fuel-check` still skips fuel entirely. Continue-anyway stays after the spinner for unknown/unavailable fuel. A known 0% reading shows a no-token dialog and quits (PRI-02); it does not offer continue. `--check-hours`, relaunch, and SKIP logging are unchanged. Clone and fetch waits are later FRs.

### `/airc` (FR #635)

`PriorityAgent.exe /airc` shows a wait spinner, then lists airc hosts currently connected on IRC (live roster via bob digest `airc_console_details`: host, shop channel, and domain/workgroup when present). Esc cancels. No hosts → a clear message. Roster unreachable → a clear error that says to retry with `/airc`. Choosing a host only records the choice under `%LOCALAPPDATA%\PriorityAgent\airc-selection.json` (session use is FR #636). Offline tests can set `PRIORITY_AGENT_AIRC_DIGEST_JSON`. Never logs IRC passwords.

A per-machine **MSI** is an extra asset next to the existing `PriorityAgent.exe` pre-release (the loose
exe stays). The MSI installs to `C:\Program Files\Priority Agent` and creates Start Menu and Desktop
shortcuts named **Priority Agent**. An interactive install offers **Grok**, **Cursor**, **OpenCode**,
or **Not now**. `msiexec /qn` completes without showing that prompt and does not download a CLI.

### Desktop shortcut for the loose exe (FR #668)

The shipped GitHub Release is the bare `PriorityAgent.exe` (no MSI required). After you place the exe
somewhere durable (for example a tools folder), run:

```
PriorityAgent.exe --install
```

That creates or updates a **per-user** Desktop shortcut named `Priority Agent` that targets this exe
(using the current user's Desktop folder from the OS — no machine-specific path is hard-coded). A
second `--install` overwrites the same `.lnk` (idempotent; no duplicate shortcuts). Rollback:

```
PriorityAgent.exe --uninstall
```

`launcher\build.ps1 -Install` copies the build into `-Dest` and then runs the same `--install` path.

## What it does on every start

1. It **fast-forwards SimonBarnett/priority-mcp**.
   - **AI root:** unless `--root` / `PRIORITY_AGENT_ROOT` is set, the launcher looks for `X:\AI` on ready
     drives in order **C: → D: → E: … Z:**. If none exists, it **creates** `\AI` on the first ready drive
     in that same order (so a machine with only `C:` gets `C:\AI`; a missing `D:` is never chosen).
   - Preferred clone path is `<AI root>\priority-mcp`. Search roots after that: every existing `X:\AI`
     and `X:\repos` on ready drives. A directory counts as a clone only when it has a `.git` folder and
     `origin` matches `SimonBarnett/priority-mcp`.
   - **Empty stub:** if the preferred path exists but is **not** a git clone (for example only a `.grok`
     folder, no `.git`), the launcher skips it and uses another clean clone under a searched root.
     Agents must use the resolved `{{FOMPREP_DIR}}` / session cwd for `git pull` --- never hardcode a drive letter.
   - Operator choices when the preferred path is a stub: (1) rename/remove the stub and reclone into the
     resolved AI root, or (2) set `PRIORITY_AGENT_ROOT` / `--root` to the folder that holds the real clone.
   - It does `fetch --prune`, then `merge --ff-only`, but only when the clone is on `main` and clean.
     **FR #791:** when a recent successful pull left `HEAD==origin/branch` (stamp
     `.git/priority-agent-last-pull.json`, default TTL 45 minutes via
     `PRIORITY_AGENT_CLONE_PULL_TTL_MIN`), warm starts **skip** the network fetch.
     `--force-pull` always fetches. A valid clone with uncommitted changes is still usable: the
     launcher warns and skips the pull (FR #240). It never resets local work and never aborts only
     because the clone is dirty. An empty stub (no `.git`) is skipped when another real clone exists.
     If no real clone exists, FATAL lists each candidate path and why it was rejected.
2. It **loads the Priority MCP and skills** for every context. The registration is idempotent and user level:
   - **Default (FR #669):** the **hosted** catalog at **`https://mcp.ntsa.uk/mcp`**.
     `grok mcp add --transport http priority-catalog https://mcp.ntsa.uk/mcp`.
     An existing hosted URL (including that one) is **left unchanged** when an interactive session starts.
   - **Local stdio (opt-in only):** `--mcp-local` or `PRIORITY_AGENT_MCP_LOCAL=1` registers a local stdio
     server from the priority-mcp clone:
     `grok mcp add priority-catalog -- node <clone>\v2\apps\mcp-catalog\stdio.mjs`.
     That is the repo's own zero-dependency stdio entry point (no npm install needed). It serves `list_catalog`,
     `get_skill`, `get_instance_schema` and `get_runner_files` from `v2/apps/mcp-catalog/catalog`.
     - It needs Node 18+. It looks in `PRIORITY_AGENT_NODE`, `D:\tools\node`, PATH and Program Files, and checks each
       node's version (MarchHare's PATH node is v8, which is too old).
     - If there's no suitable node, or `stdio.mjs` is missing, it falls back to the same server built into the exe
       (`PriorityAgent.exe --mcp-catalog`).
     Then it runs `grok mcp doctor priority-catalog`.
   - It adds `<priority-mcp>/.grok/skills` to `[skills] paths` in `%USERPROFILE%\.grok\config.toml`.
     A timestamped `.bak-priorityagent-*` backup is written before any change.
   - This is needed because Grok only reads project skills from the cwd's own repo, and a
     customer repo is a different repo. This makes the Priority skills visible to **all** Grok sessions on the machine.
3. It **loads the customer repos**: SimonBarnett repos tagged with the GitHub topic **`priority-customer`**.
   It uses the REST API through `gh api`, with no search-index lag. `--list` prints the numbered
   table (name, clone path, description). Interactive mode skips that dump and goes straight to
   **Pick customer** (FR #675 — no duplicate list).
4. You pick one of three contexts:
   - **General Priority**: cwd is the priority-mcp clone.
   - **Customer**: cwd is the customer clone. A repo new to the machine is cloned into `<AI root>\<repo>`;
     an existing clone under any searched `X:\AI` or `X:\repos` is reused.
   - **Project / service call**: one of the customer repo's top-level folders, skipping dot-folders.
     cwd is that folder.
5. It loads **Priority SaaS WCF allowlist** rows (FR #348): default %USERPROFILE%\.priority-saas-wcf\instances.json (env PRIORITY_SAAS_WCF_INSTANCES), or a wcfInstancesPath / SaaS-shaped list in .priority-agent.json (project overrides customer). Each row has id, host, tenant, company, user, tokenFile, allowLive (see v2/apps/priority-saas-wcf/instances.example.json). Credentials and tokens are never stored there. The launcher writes a session-only wcf-instances.json and injects instructions for bin/priority-wcf.mjs + skill priority-saas-wcf (not bobiverse #6026 priority_wcf.py). Legacy airc / airc_console fields still load without error and print a warning that they are ignored. --airc, --no-airc and --airc-console are rejected with a clear "removed; use the WCF client" message.
6. **New customer / new project** never creates anything. It prints `Ask Haitch: new customer repo for X`
   or `Ask Haitch: new project folder Y in <repo>`, copies the line to the clipboard and exits. Haitch creates it
   from `launcher/customer-setup`. A project folder must already be an authoritative `SCxxxxxx` or `SOxxxxxx`.
   The Medatech booking id and WBS go in that folder's `project.md`, not in the folder name.
7. It checks fuel like `ce-dayworks/scripts/Start-DayWorks.ps1`.
   - Source: the Bob report `machines.<machine>.pcent["grok-build"]` (xAI weekly
     remaining), then `machines.<machine>.weekly`, then legacy `pcent["grok-chat"]`
     (Cursor Sand). Hosts without Cursor often publish only `grok-build` (FR #677).
   - Above 0 means the pool is used. 0 or unknown gives a warning and asks before continuing.
   - It never handles keys.
8. It **starts the chosen agent CLI** in the same console (issue #569 / FR #570 / FR-005):
   - **Agent CLI chooser (issue #569):** same rules as the MSI chooser. None installed and
     interactive: offer Grok, Cursor, OpenCode, or refuse. Refuse prints the not-found line
     and exits 1 with no download. Exactly one installed: launch it. Two or more with Grok
     installed: launch Grok (FR #674; Priority Agent always uses Grok; never by token fuel).
     Two or more without Grok: ask which (never by token fuel). A missing pick asks
     "Install it now?" before any download. Redirected stdin does not prompt and does not
     download (except launching Grok when it is among several). Cursor is
     `%LOCALAPPDATA%\cursor-agent\agent.cmd` (never the Cursor.exe GUI or
     `%USERPROFILE%\.grok\bin\agent.exe`; FR #568).
   - **`--cli grok|cursor|opencode`** (env `PRIORITY_AGENT_CLI`) skips the chooser and
     starts that CLI. The selected work directory is always `ProcessStartInfo.WorkingDirectory`.
   - **Grok** keeps `RunGrok`: `grok --cwd <context> --session-id <new uuid> --trust --rules <agent rules> "<context brief>"`
     plus MCP registration / fuel checks.
   - **Cursor** starts `%LOCALAPPDATA%\cursor-agent\agent.cmd` (never `%USERPROFILE%\.grok\bin\agent.exe`).
     **OpenCode** starts a standalone `opencode.exe`. Neither receives Grok MCP /
     `--cwd` / `--session-id` / `--rules` flags. The launcher reads `agent --help` /
     `opencode --help` before adding workspace flags; if those flags are unknown, it
     starts with the work directory only. Hours relaunch always starts the chosen exe.
   - The rules come from `launcher/agent-rules.md`. That file is the only reporting
     policy. The exe embeds a copy of it at build time. On launch, `LoadRules` uses the
     clone file when its `rules-version` is at least the embedded copy. Customer repos
     are not given a copy. The `{{...}}` placeholders are filled in.
   - The brief names the customer, project, paths and the files to read first.
9. **Hours are mandatory.** When the agent exits, the launcher looks for one **closed** entry on
   `GET https://bob.ntsa.uk/bob/v1/hours` (bobiverse FR #3450 / #3673). The query is
   `user` and `status=closed`. It does **not** send `idempotency_key` as a query
   parameter. `from` and `to`, when sent, are Europe/London dates from the persisted
   entry `start` through the London day of the check, so an older session is included.
   A naive timestamp is that clock time in UTC, matching bobhours. If `hours-entry.json`
   has no `start`, the date bounds are omitted. The match is client-side: persisted receiver id, `priority-agent-<full session uuid>`,
   the Priority USERLOGIN in `on_behalf_of`, the expected customer and project, `status` `closed`,
   and a real start/end.
   - `?user=` and `on_behalf_of` are the USERLOGIN stored in that session's `identity.json`
     at launch (`--hours-user`, or a prompt when a terminal is attached). `identity.json`
     also records `userLoginSource` as `client`, `saved`, or `flag`. When a last-user
     row exists for the selected WCF instance (`%USERPROFILE%\.priority-agent\last-user.json`),
     that login binds without asking (FR #681). With no saved login the interactive prompt
     is `Priority USERLOGIN for this session:`. When stdin is redirected (FR #386), a saved
     row is bound instead of prompting; with no saved login and no `--hours-user` the
     launcher exits 2 (`Priority USERLOGIN unavailable`) and never blocks on ReadLine.
     `--hours-user` still wins. If `--hours-user` is set and
     differs from the whoami login, the launcher prints both logins and exits 2 without
     starting Grok and without rewriting last-user.json (FR #387). The launcher does
     not read the OS account name, a shared environment default, or the connections file.
     A later launch for someone else does not rewrite an existing session.
     A successful `priority-wcf whoami` (FR #384) writes that same last-user.json
     (UTF-8 without BOM) after resolving Priority USERLOGIN via login result or USERS;
     it refuses a login that fails `BobHours.IsPriorityUserLogin` and never stores
     `access_token`, `refresh_token`, or `password` keys.
   - The returned id is persisted in the session `hours-entry.json`. Tests set
     `PRIORITY_AGENT_HOURS_JSON` to a list fixture. When that variable is set, the check does
     not contact the network.
   - A non-success GET is **delivery unverified**. The launcher tries once and does not retry.
     Do not treat a merged rules file or this source change as an installed exe conversion.
     `LoadRules` uses `launcher/agent-rules.md` from the clone when its `rules-version` is at
     least the copy embedded in `PriorityAgent.exe`. The embedded copy changes only when the
     exe is rebuilt and published, which this change does not do.
   - If the entry is missing, open, `auto_closed`, or for a different key, the launcher shows a
     loud red warning and relaunches the **same chosen CLI** (Grok: `grok --resume <id>`;
     Cursor/OpenCode: that CLI's argv, never a hardcoded `grok.exe`) until the closed
     entry matches, or the operator types `SKIP` (logged).
   - `log.md` stays a work narrative with no hours in it. It is not the hours store. The check does not read a git hours ref.

Logs go to `%LOCALAPPDATA%\PriorityAgent\logs`. Per-session `launch.json` and the rendered rules go to `...\sessions`.

## WCF configuration (FR #348)

The client is fom-prep v2/apps/priority-saas-wcf (bin/priority-wcf.mjs), skill priority-saas-wcf.
Allowlist default: %USERPROFILE%\.priority-saas-wcf\instances.json (override PRIORITY_SAAS_WCF_INSTANCES).
Copy from v2/apps/priority-saas-wcf/instances.example.json:

```json
{
  "instances": [
    {
      "id": "saas-dev",
      "host": "p.priority-connect.online",
      "tenant": "exampletenant",
      "company": "exampledname",
      "user": "user@example.com",
      "tokenFile": "C:\\passwords\\example-saas-oauth-tokens.json",
      "language": 3,
      "allowLive": false
    }
  ]
}
```


Optional project/customer .priority-agent.json may set wcfInstancesPath to that file, or embed the same
SaaS-shaped wcf / instances array (project overrides customer). **Never** put passwords or tokens in
these files. Tokens come from tokenFile or process env PRIORITY_SAAS_TOKEN (see priority-saas-oidc-login).

Rows that look like bobiverse #6026 (serverUrl + mode direct|relay) are ignored with a warning.

Legacy keys airc and airc_console are ignored with a warning. The launcher does not open DCC and
does not send airc shell PRIVMSGs. Agent rules require priority-wcf.mjs for Priority work and require
a harvest through Bobiverse intake (tools\Report-FomprepIntakeIssue.ps1 / Invoke-FomprepHarvest.ps1)
after every client session.


## Agent rules (summary; full text in `agent-rules.md`)

- **Harvest rule**: knowledge goes to the level it belongs to.
  - General Priority skills go to **priority-mcp as a branch + PR**.
  - Customer-installation skills and insights go to the **customer repo**.
  - Project and service-call specifics go to **that project folder** (including `log.md`).
- **Secrets**:
  - Connection details live in `C:\passwords\<repo>-connections.txt` on the machine the agent runs on (always local;
    this overrides any AGENTS.md naming another machine). If the file is missing, the agent asks Haitch for it.
    `<repo>` where `<repo>` is the GitHub repo name in lowercase
    (for example `rees-connections.txt` or `ce-priority-connections.txt`).
    Configure with `--passwords-dir` or the `PRIORITY_AGENT_PASSWORDS` env var.
  - The agent reads the file when it needs a connection and must **never commit, echo, quote or summarise** it.
  - The launcher only checks that the file exists. It never reads it.
- **Customer systems**: agents may connect to the selected customer's systems with the connections file **without asking first**.
  They must not message the customer's people without asking Simon.
- **Hours (mandatory)**: one closed webhook entry per session, matched client-side as in step 9.
  - Human-equivalent time, with evidence links in the description or `tickets`. Haitch proposes
    bookings and Simon approves them. The agent does not post Priority.
  - `launcher/hours-template.md` is a legacy markdown table. It is not the hours store and it is
    not the customer-repo template. Use `launcher/customer-setup` for new repos.
  - Hours are never stored or reported in a customer repo: no `hours.json`, hours files or
    folders, and no hours in `log.md`. The Bob hours webhook is the only hours store.

## Use

Double-click the **Priority Agent** desktop shortcut, or:

```
PriorityAgent.exe                                  interactive menu
PriorityAgent.exe --list                           customers (topic priority-customer)
PriorityAgent.exe --list --customer rees           + its project folders
PriorityAgent.exe --customer rees --project SC263182
PriorityAgent.exe --general
PriorityAgent.exe --dry-run --customer rees --project SC263182   show everything, change nothing, print the launch command
PriorityAgent.exe --model gpt-5 --context 200000 --effort low --dry-run --general   PRI-01 settings line before launch command
PriorityAgent.exe --smoke --customer rees --project SC263182     real setup + one brief no-tools single-turn launch
PriorityAgent.exe --cli cursor --customer rees --project SC263182
PriorityAgent.exe --check-hours C:\AI\rees --session <uuid>      run only the hours check (path under the resolved AI root)
PriorityAgent.exe --mcp-catalog [<catalog dir>]                 fallback stdio catalog MCP (env PRIORITY_CATALOG_DIR)
```

Other options: `--root <dir>` (clone root; default: first existing `X:\AI` on C:->Z:, else create `\AI` on the first ready drive; env `PRIORITY_AGENT_ROOT`),
`--cli grok|cursor|opencode` (env `PRIORITY_AGENT_CLI`; when omitted the issue #569 chooser runs),
`--passwords-dir <dir>`, `--no-fuel-check`, `--no-pause`.

Requirements: `git`, `gh` (logged in as SimonBarnett). With none installed and an interactive console
the launcher offers Grok, Cursor, OpenCode, or refuse (exit 1, no download). Exactly one installed
is launched. Two or more with Grok: Grok is launched (FR #674). Two or more without Grok: the
operator picks; token fuel is not used. For `--cli grok`: Grok CLI
(`%USERPROFILE%\.grok\bin\grok.exe`). For `--cli cursor`: `%LOCALAPPDATA%\cursor-agent\agent.cmd`
(never the Cursor.exe GUI or `%USERPROFILE%\.grok\bin\agent.exe`). For `--cli opencode`:
`%USERPROFILE%\.opencode\bin\opencode.exe` or a standalone `opencode.exe` under LocalAppData/home.
Official installers run as the interactive user only (FR #568); tests fake those commands and do
not hit the network.

Optional execute plugins (formprep, compile, install, odata) need node and `instances.json`, so the launcher doesn't install them:
`grok plugin marketplace add SimonBarnett/priority-mcp`, then `grok plugin install priority-formprep --trust` (and so on).

## Build

Requires the .NET 8 SDK. It builds on Windows or Linux; the icon is embedded either way.

```
# From the repository root:
dotnet publish launcher/PriorityAgent.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o launcher/out
# -> launcher/out/PriorityAgent.exe (self-contained, single file, ~35 MB)
dotnet run --project launcher.Tests/PriorityAgent.Tests.csproj -c Release
# The launcher acceptance checks are a console harness (dotnet test does not discover tests).
cd launcher
.\build.ps1 -Install                                      # Windows: build + copy to D:\tools\PriorityAgent + desktop shortcut
```

The Windows workflow also runs the published executable through dry-run integration scenarios with isolated
profiles and fake GitHub/Grok CLIs, checking config preservation, WCF session instance files, and connection-file
canary handling. The `Launcher release` workflow runs on launcher pull requests and `main` pushes for validation;
it publishes a GitHub **pre-release** with `PriorityAgent.exe` and its `.sha256` file for `launcher-v*` tags or a
manual dispatch from `main`. When an MSI is packed, it is an **additional asset** beside that loose exe
pre-release; the exe is not replaced. A `launcher-vX.Y.Z` tag stamps banner / FileVersion `X.Y.Z` / `X.Y.Z.0` and
ProductVersion `X.Y.Z+<commit>` (FR #267). Local `launcher\build.ps1 -Install` defaults to `1.0.0` unless you pass `-Version`;
it then runs `PriorityAgent.exe --install` for the Desktop shortcut (FR #668). Download
it from [GitHub Releases](https://github.com/SimonBarnett/priority-mcp/releases), verify the checksum,
place the exe, and run `PriorityAgent.exe --install` once. Releases remain pre-release until separately approved.

## Icon

Consumers (tray, exe, docs) reuse the existing file `launcher/priority.ico`. Do not draw a new mark.
`PriorityAgent.csproj` sets `ApplicationIcon` to `priority.ico` beside the project, which is that same file.
It is the Priority client's own icon, copied from `C:\Program Files (x86)\Priority\favicon.ico`
on MarchHare. It has 16, 24, 32, 48, 128 and 256 px frames and is the same "p" logo as `WINMENU.exe`, the
Priority desktop shortcut target. Priority is a trademark of Priority Software.
