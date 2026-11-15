using PriorityAgent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var checks = new List<(string Name, Action Run)>
{
    ("project wcf overrides customer; legacy airc warns", TestWcfConfigPrecedenceAndLegacy),
    ("saas allowlist loads and renders priority-wcf.mjs rules", TestSaasAllowlistAndRules),
    ("FR-045: rules example uses whoami and --request; empty USERLOGIN accepts saved; no 'No default login is used'", TestFr045RulesWhoamiAndSavedPrompt),
    ("session instances file has no secrets", TestSessionInstancesNoSecrets),
    ("airc source files and template are gone", TestAircFilesGone),
    ("README documents WCF client only", TestReadmeWcfOnly),
    ("FR-620: README documents Up/Down/Enter and wait glyphs", TestFr620ReadmeInteractiveUx),
    ("agent-rules use WCF_SECTION not AIRC_SECTION", TestAgentRulesWcfPlaceholder),
    ("command line size guard still works", TestCommandLineSize),
    ("dirty clone plus stub uses the dirty clone", TestDirtyClonePreferredOverStub),
    ("dirty clone only is usable", TestDirtyCloneOnly),
    ("no real clone lists every rejected candidate", TestNoRealCloneFatalListsCandidates),
    ("AI root prefers existing C:\\AI over later drives", TestAiRootPrefersExistingC),
    ("AI root skips missing D: and creates on first ready drive", TestAiRootCreatesOnFirstReady),
    ("AI root search lists AI and repos across ready drives", TestAiRootBuildSearchRoots),
    ("saved MEDATECH empty answer binds MEDATECH; prompt has [MEDATECH] not OS user", TestSavedLoginEmptyAnswer),
    ("--hours-user OTHER wins over saved MEDATECH", TestHoursUserWinsOverSaved),
    ("MRB-434: space login and access_token row are not suggested", TestInvalidSavedLoginIgnored),
    ("session metadata userLoginSource saved on empty Enter", TestSessionSourceSaved),
    ("--hours-user matching whoami records flag", TestSessionSourceFlagMatchesWhoami),
    ("MRB-439 hostile: typed login is client even when equals saved", TestSessionSourceTypedIsClient),
    ("MRB-439 hostile: identity has no token keys", TestSessionIdentityNoSecrets),
    ("MRB-444 hostile: hours-routing has on_behalf_of not user and no token keys", TestHoursRoutingNoSecrets),
    ("FR-387: --hours-user OTHERUSER vs whoami MEDATECH exits 2 and does not start grok", TestHoursUserDisagreesWithWhoami),
    ("MRB-438: matching --hours-user continues; missing whoami does not block", TestHoursUserMatchesOrAbsentWhoami),
    ("FR-004 redirected stdin plus saved login binds that login", TestRedirectedSavedLoginBinds),
    ("FR-004 redirected stdin and no saved login exits unavailable", TestRedirectedNoSavedUnavailable),
    ("FR-568 grok.exe under .grok\\bin is present", TestFr568GrokDetect),
    ("FR-568 agent.exe under .grok\\bin is not Cursor", TestFr568CursorRejectsGrokAgent),
    ("FR-568 fake installer is interactive user not LocalSystem", TestFr568InstallerInteractiveUser),
    ("FR-568 OpenCode URL from fixture html no live network", TestFr568OpenCodeParseFixture),
    ("MRB-575 OpenCode plan uses $env:USERPROFILE and creates .opencode\\bin", TestMrb575OpenCodePlanPath),
    ("FR-570: resolve exe path matches grok/cursor/opencode choice", TestFr570ResolveExeMatchesChoice),
    ("FR-570: Cursor/OpenCode argv omit Grok MCP flags", TestFr570CursorOpenCodeOmitGrokFlags),
    ("FR-570: unknown help starts with empty argv (work dir only)", TestFr570UnknownHelpWorkDirOnly),
    ("FR-570: hours relaunch uses chosen CLI argv shape not hardcoded grok", TestFr570HoursRelaunchChosenCli),
    ("FR-569: zero CLIs interactive offers Grok Cursor OpenCode refuse", TestFr569ZeroInteractiveOffersMenu),
    ("FR-569: refuse prints not-found and does not download", TestFr569RefuseNoDownload),
    ("FR-569: exactly one installed launches it", TestFr569ExactlyOneLaunches),
    ("FR-569: two or more ask which; fuel is not used", TestFr569ManyAskWhichNotFuel),
    ("FR-569: missing pick asks Install it now before download", TestFr569MissingAsksInstall),
    ("FR-569: redirected stdin does not prompt or download", TestFr569RedirectedNoPromptNoDownload),
    ("FR-569: missing grok.exe is not the only error path", TestFr569CursorOnlyLaunches),
    ("MRB-577 hostile: Cursor probe is agent.cmd not Cursor.exe GUI", TestMrb577CursorAgentNotGui),
    ("MRB-577 hostile: .grok\\bin\\agent.exe is rejected as Cursor", TestMrb577RejectGrokAgentAsCursor),
    ("MRB-577 hostile: which-prompt forbids token fuel (exact)", TestMrb577WhichForbidsFuelExact),
    ("MRB-577 hostile: hours relaunch exe is the chosen CLI", TestMrb577HoursRelaunchChosenExe),
    ("FR-609: TTY Down Down Enter selects index 2", TestFr609PickListDownDownEnter),
    ("FR-609: redirected stdin 1 selects index 0", TestFr609PickListRedirectedOne),
    ("FR-609: redirected stdin 2 selects index 1", TestFr609PickListRedirectedTwo),
    ("FR-609: Esc cancels with null", TestFr609PickListEscCancels),
    ("FR-613: wait spinner preview has >=2 distinct glyphs", TestFr613WaitSpinnerGlyphs),
    ("FR-613: redirected / Quiet WithWait writes nothing", TestFr613WaitQuietNoWrites),
    ("FR-613: Program sets Quiet before --mcp-catalog serve", TestFr613McpCatalogSetsQuiet),
    ("FR-619: hours fetch TTY spinner settles done on success", TestFr619HoursFetchTtyDone),
    ("FR-619: hours fetch failure keeps detail and does not print done", TestFr619HoursFetchFailNoDone),
    ("FR-619: hours fetch source wraps the GET; relaunch SKIP stays", TestFr619HoursFetchWired),
    ("FR-618: MCP register and doctor wrap grok waits; doctor text stays", TestFr618McpRegisterDoctorWired),
    ("FR-618: grok inspect skills check uses WithWait", TestFr618SkillsInspectWired),
    ("FR-616: fuel report TTY spinner settles done on success", TestFr616FuelReportTtyDone),
    ("FR-616: fuel report failure keeps detail and does not print done", TestFr616FuelReportFailNoDone),
    ("FR-616: fuel report source wraps GET; --no-fuel-check and Continue stay", TestFr616FuelReportWired),
    ("MRB-633 hostile: WithWait on throw clears line and does not print done", TestMrb633WaitThrowNoDone),
    ("MRB-633 hostile: WithWait TTY emits glyphs then settle done", TestMrb633WaitTtyGlyphsAndSettle),
    ("FR-611: project PickList Down Enter selects first folder", TestFr611ProjectPickListFirstFolder),
    ("FR-611: project resolve customer-level / folder / New / cancel", TestFr611ProjectResolveRows),
    ("FR-611: redirected project pick 2 selects first folder", TestFr611ProjectRedirectedFirstFolder),
    ("FR-610: customer PickList Down Enter selects first customer", TestFr610CustomerPickListFirstCustomer),
    ("FR-610: customer resolve General / customer / New / cancel", TestFr610CustomerResolveRows),
    ("FR-610: redirected customer pick 2 selects first customer", TestFr610CustomerRedirectedFirst),
};

int failures = 0;
foreach (var (name, run) in checks)
{
    try { run(); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); }
}
return failures == 0 ? 0 : 1;

static void TestWcfConfigPrecedenceAndLegacy()
{
    string root = NewTemp();
    try
    {
        string customer = Path.Combine(root, "customer"), project = Path.Combine(customer, "project");
        Directory.CreateDirectory(project);
        File.WriteAllText(Path.Combine(customer, ".priority-agent.json"),
            """{"airc":true,"airc_console":"flamingo_console","wcf":[{"id":"cust","host":"cust.example.invalid","tenant":"tcust","company":"CCUST","user":"u@c","tokenFile":"C:\\\\passwords\\\\cust.json","allowLive":false}]}""");
        File.WriteAllText(Path.Combine(project, ".priority-agent.json"),
            """{"airc":false,"wcf":[{"id":"proj","host":"proj.example.invalid","tenant":"tproj","company":"CPROJ","user":"u@p","tokenFile":"C:\\\\passwords\\\\proj.json","allowLive":false}]}""");
        WcfConfig config = WcfSupport.LoadConfig(project, customer);
        Assert(config.Instances.Count == 1 && config.Instances[0].Id == "proj", "project wcf must replace customer");
        Assert(config.Instances[0].Host == "proj.example.invalid" && config.Instances[0].Tenant == "tproj", "project saas fields lost");
        Assert(config.LegacyAircWarnings.Count >= 1, "legacy airc fields must warn");
        Assert(config.LegacyAircWarnings.Any(w => w.Contains("ignored", StringComparison.OrdinalIgnoreCase)),
            "legacy warning must say ignored");

        WcfConfig customerOnly = WcfSupport.LoadConfig(null, customer);
        Assert(customerOnly.Instances.Count == 1 && customerOnly.Instances[0].Id == "cust", "customer wcf lost");
        Assert(customerOnly.LegacyAircWarnings.Count >= 1, "customer legacy airc must warn");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestSaasAllowlistAndRules()
{
    string root = NewTemp();
    try
    {
        string dir = Path.Combine(root, "cust");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, ".priority-agent.json"), """
            {
              "wcf": [
                { "id": "saas", "host": "p.priority-connect.online", "tenant": "ten", "company": "CO", "user": "u@e", "tokenFile": "C:\\\\passwords\\\\t.json", "allowLive": false },
                { "id": "legacy", "serverUrl": "https://prioritydev.example.invalid/", "mode": "direct" },
                { "id": "bad", "host": "", "tenant": "x" }
              ]
            }
            """);
        WcfConfig config = WcfSupport.LoadConfig(null, dir);
        Assert(config.Instances.Count == 1 && config.Instances[0].Id == "saas", $"expected 1 saas instance, got {config.Instances.Count}");
        Assert(config.LegacyAircWarnings.Any(w => w.Contains("#6026", StringComparison.Ordinal) || w.Contains("ignored", StringComparison.OrdinalIgnoreCase)),
            "legacy serverUrl row must warn");

        string file = WcfSupport.WriteSessionInstancesFile(root, config.Instances);
        string rules = WcfSupport.RenderSessionRules("before{{WCF_SECTION}}after", config.Instances, file);
        Assert(rules.Contains("priority-wcf.mjs", StringComparison.Ordinal)
            && rules.Contains("priority-saas-wcf", StringComparison.Ordinal)
            && rules.Contains("bob.ntsa.uk/bob/v1/intake", StringComparison.Ordinal)
            && rules.Contains("Report-FomprepIntakeIssue", StringComparison.Ordinal)
            && rules.Contains("Invoke-FomprepHarvest", StringComparison.Ordinal)
            && rules.Contains("priority-mcp", StringComparison.Ordinal)
            && rules.Contains("SO/SC", StringComparison.Ordinal)
            && rules.Contains("gh issue create", StringComparison.Ordinal)
            && rules.Contains("Never", StringComparison.Ordinal)
            && rules.Contains("WINRUN", StringComparison.Ordinal)
            && rules.Contains("#6026", StringComparison.Ordinal)
            && !rules.Contains("DCCLISTEN", StringComparison.Ordinal)
            && !rules.Contains("{{WCF_SECTION}}", StringComparison.Ordinal)
            && !rules.Contains("{{AIRC_SECTION}}", StringComparison.Ordinal),
            "rules must name fom-prep client + harvest destinations and forbid old paths");
        Assert(rules.StartsWith("before", StringComparison.Ordinal) && rules.EndsWith("after", StringComparison.Ordinal),
            "template bookends lost");
        Assert(rules.Contains("whoami", StringComparison.Ordinal)
            && rules.Contains("--request", StringComparison.Ordinal),
            "FR-045: RenderSessionRules example must use whoami and --request");
        Assert(!rules.Contains("No default login is used", StringComparison.Ordinal),
            "FR-045: rules must not say No default login is used");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestFr045RulesWhoamiAndSavedPrompt()
{
    string rules = WcfSupport.RenderSessionRules(
        "{{WCF_SECTION}}",
        new[] { new WcfInstance("dev", "p.priority-connect.online", "ten", "CO", "u@e", "C:\\tok\\t.json", 3, false, "") },
        null);
    Assert(rules.Contains("whoami", StringComparison.Ordinal), "rendered section must mention whoami");
    Assert(rules.Contains("--request", StringComparison.Ordinal), "rendered section must mention --request");
    Assert(
        rules.Contains("Priority USERLOGIN", StringComparison.Ordinal)
        && (rules.Contains("empty", StringComparison.OrdinalIgnoreCase) || rules.Contains("Enter", StringComparison.Ordinal))
        && rules.Contains("saved", StringComparison.OrdinalIgnoreCase),
        "section must state an empty Priority USERLOGIN answer accepts the saved login");
    Assert(!rules.Contains("No default login is used", StringComparison.Ordinal),
        "rendered section must not contain the sentence No default login is used");
}

static void TestSessionInstancesNoSecrets()
{
    string root = NewTemp();
    try
    {
        var instances = new[]
        {
            new WcfInstance("dev", "p.priority-connect.online", "ten", "CO", "u@e", "C:\\\\passwords\\\\t.json", 3, false, ""),
        };
        string path = WcfSupport.WriteSessionInstancesFile(root, instances);
        string text = File.ReadAllText(path);
        Assert(!Regex.IsMatch(text, @"\bpassword\b|credentialTarget|Bearer\s+[A-Za-z0-9_-]{20,}|access_token|\bsecret\b",
            RegexOptions.IgnoreCase), "session wcf file must not look like secrets");
        using var doc = JsonDocument.Parse(text);
        Assert(doc.RootElement.GetProperty("client").GetString()!.Contains("priority-wcf.mjs", StringComparison.Ordinal),
            "session file must name priority-wcf.mjs");
        Assert(doc.RootElement.GetProperty("instances")[0].GetProperty("host").GetString()
            == "p.priority-connect.online", "host missing from session file");
        Assert(doc.RootElement.GetProperty("instances")[0].GetProperty("tenant").GetString() == "ten",
            "tenant missing from session file");
        Assert(!doc.RootElement.GetProperty("instances")[0].TryGetProperty("serverUrl", out _),
            "session file must not use serverUrl schema");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestAircFilesGone()
{
    string launcherDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "launcher"));
    if (!Directory.Exists(launcherDir))
        launcherDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "launcher"));
    Assert(Directory.Exists(launcherDir), $"launcher dir not found near tests ({launcherDir})");
    Assert(WcfSupport.AircSourceFilesGone(launcherDir),
        "AircSupport.cs / AircConsoleDiscovery.cs / airc-dcc.md must be removed from launcher/");
    Assert(!File.Exists(Path.Combine(launcherDir, "AircSupport.cs")), "AircSupport.cs still present");
    Assert(File.Exists(Path.Combine(launcherDir, "WcfSupport.cs")), "WcfSupport.cs missing");
}

static void TestReadmeWcfOnly()
{
    string readme = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "launcher-README.md"));
    Assert(readme.Contains("priority-wcf.mjs", StringComparison.Ordinal)
        && readme.Contains("priority-saas-wcf", StringComparison.Ordinal)
        && readme.Contains("tokenFile", StringComparison.Ordinal),
        "README must document fom-prep priority-wcf.mjs allowlist");
    Assert(!Regex.IsMatch(readme, "[\\x00-\\x08\\x0B\\x0C\\x0E-\\x1F]"),
        "README must not contain C0 control characters (BEL/BS/VT)");
    Assert(readme.Contains("host, tenant, company", StringComparison.Ordinal)
        && readme.Contains("bin/priority-wcf.mjs", StringComparison.Ordinal)
        && readme.Contains("v2/apps/priority-saas-wcf", StringComparison.Ordinal),
        "README SaaS field names / client path must be intact");
    Assert(!readme.Contains("\"mode\": \"direct\"", StringComparison.Ordinal)
        && !readme.Contains("\"mode\": \"relay\"", StringComparison.Ordinal),
        "README must not show bobiverse direct/relay as the client route");
    Assert(!readme.Contains("airc_consoles", StringComparison.Ordinal)
        && !readme.Contains("Does this session need airc", StringComparison.Ordinal)
        && !readme.Contains("bobiverse-airc-commands", StringComparison.Ordinal),
        "README must not document the old airc discovery / session skills path");
    Assert(readme.Contains("removed", StringComparison.OrdinalIgnoreCase)
        || readme.Contains("ignored", StringComparison.OrdinalIgnoreCase),
        "README should mention legacy airc ignore / removed flags");
}

static void TestFr620ReadmeInteractiveUx()
{
    string launcherDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "launcher"));
    string readmePath = Path.Combine(launcherDir, "README.md");
    if (!File.Exists(readmePath))
        readmePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "launcher", "README.md"));
    Assert(File.Exists(readmePath), "launcher/README.md not found");
    string readme = File.ReadAllText(readmePath);
    Assert(readme.Contains("## Interactive console UX", StringComparison.Ordinal), "README needs an interactive UX section");
    Assert(readme.Contains("Up/Down/Enter", StringComparison.Ordinal), "README must name Up/Down/Enter");
    Assert(readme.Contains("`|` `/` `-` `\\`", StringComparison.Ordinal), "README must name the wait glyphs");
    Assert(readme.Contains("Non-interactive flags are unchanged", StringComparison.Ordinal)
        && readme.Contains("--customer", StringComparison.Ordinal)
        && readme.Contains("--no-fuel-check", StringComparison.Ordinal),
        "README must say non-interactive flags stay");
    Assert(readme.Contains("--mcp-catalog", StringComparison.Ordinal), "README must keep MCP stdout out of the spinner");
}

static void TestAgentRulesWcfPlaceholder()
{
    string rules = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "agent-rules.md"));
    Assert(rules.Contains("{{WCF_SECTION}}", StringComparison.Ordinal), "agent-rules need {{WCF_SECTION}}");
    Assert(!rules.Contains("{{AIRC_SECTION}}", StringComparison.Ordinal), "agent-rules must drop {{AIRC_SECTION}}");
    Assert(Regex.IsMatch(rules, @"rules-version:\s*(\d+)")
        && int.Parse(Regex.Match(rules, @"rules-version:\s*(\d+)").Groups[1].Value) >= 7,
        "rules-version must bump with WCF section");
}

static void TestCommandLineSize()
{
    string root = NewTemp();
    try
    {
        string rules = WcfSupport.RenderSessionRules("{{WCF_SECTION}}",
            new[] { new WcfInstance("dev", "p.priority-connect.online", "ten", "CO", "u@e", "C:\\tok\\t.json", 3, false, "") },
            Path.Combine(root, "wcf-instances.json"));
        var args = LauncherSupport.BuildGrokArguments(root, Guid.NewGuid().ToString(), rules,
            "Priority session " + new string('x', 500), false);
        int length = LauncherSupport.WindowsCommandLineLength("C:\\Program Files\\Grok\\grok.exe", args);
        Assert(length < LauncherSupport.MaxWindowsCommandLineLength, "WCF rules blew the argv budget");
        try { LauncherSupport.ValidateWindowsCommandLine("grok.exe", new[] { new string('x', 32_001) }, "test launch"); Assert(false, "expected throw"); }
        catch (InvalidOperationException) { /* expected */ }
    }
    finally { DeleteTempDirectory(root); }
}

static void TestDirtyClonePreferredOverStub()
{
    var sightings = new[]
    {
        new CloneSighting(@"D:\ai\priority-mcp", CloneKind.Stub),
        new CloneSighting(@"C:\ai\priority-mcp", CloneKind.Dirty),
    };
    string? chosen = CloneResolve.SelectUsable(sightings, out string? reason);
    Assert(chosen == @"C:\ai\priority-mcp", "dirty real clone should win over an empty stub");
    Assert(reason != null && reason.Contains("dirty", StringComparison.OrdinalIgnoreCase), "reason should say dirty / skip pull");
}

static void TestDirtyCloneOnly()
{
    var sightings = new[] { new CloneSighting(@"C:\ai\priority-mcp", CloneKind.Dirty) };
    string? chosen = CloneResolve.SelectUsable(sightings, out string? reason);
    Assert(chosen == @"C:\ai\priority-mcp", "lone dirty clone must be usable");
    Assert(reason != null && reason.Contains("skip pull", StringComparison.OrdinalIgnoreCase), "dirty clone must skip pull");
}

static void TestNoRealCloneFatalListsCandidates()
{
    var sightings = new[]
    {
        new CloneSighting(@"D:\ai\priority-mcp", CloneKind.Stub),
        new CloneSighting(@"C:\ai\priority-mcp", CloneKind.Missing),
        new CloneSighting(@"D:\repos\priority-mcp", CloneKind.WrongOrigin),
    };
    Assert(CloneResolve.SelectUsable(sightings, out _) == null, "no usable clone should be selected");
    string fatal = CloneResolve.FatalUnavailable("SimonBarnett/priority-mcp", sightings);
    Assert(fatal.Contains("empty stub", StringComparison.OrdinalIgnoreCase)
        && fatal.Contains("missing", StringComparison.OrdinalIgnoreCase)
        && fatal.Contains("origin is not this repo", StringComparison.OrdinalIgnoreCase)
        && fatal.Contains("D:\\ai\\priority-mcp", StringComparison.Ordinal)
        && fatal.Contains("C:\\ai\\priority-mcp", StringComparison.Ordinal),
        "FATAL must name every candidate and why it was rejected");
}

static void TestAiRootPrefersExistingC()
{
    var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"C:\AI" };
    string created = "";
    string root = AiRoot.Resolve(
        driveReady: c => c is 'C' or 'E',
        exists: p => existing.Contains(p),
        create: p => created = p);
    Assert(string.Equals(root, Path.GetFullPath(@"C:\AI"), StringComparison.OrdinalIgnoreCase),
        "should prefer existing C:\\AI when C and E are ready and D is missing");
    Assert(created == "", "must not create when an AI root already exists");
}

static void TestAiRootCreatesOnFirstReady()
{
    string created = "";
    string root = AiRoot.Resolve(
        driveReady: c => c is 'C' or 'E',
        exists: _ => false,
        create: p => created = p);
    Assert(string.Equals(created, @"C:\AI", StringComparison.OrdinalIgnoreCase),
        "with no existing AI folder, create on first ready drive (C) even when E also exists");
    Assert(string.Equals(root, Path.GetFullPath(@"C:\AI"), StringComparison.OrdinalIgnoreCase),
        "resolved root should be C:\\AI");
}

static void TestAiRootBuildSearchRoots()
{
    string[] roots = AiRoot.BuildSearchRoots(
        @"C:\AI",
        driveReady: c => c is 'C' or 'E',
        exists: p => p.Equals(@"C:\AI", StringComparison.OrdinalIgnoreCase)
            || p.Equals(@"E:\AI", StringComparison.OrdinalIgnoreCase)
            || p.Equals(@"E:\repos", StringComparison.OrdinalIgnoreCase));
    Assert(roots.Length >= 3, "expected preferred + E:\\AI + E:\\repos");
    Assert(string.Equals(roots[0], Path.GetFullPath(@"C:\AI"), StringComparison.OrdinalIgnoreCase),
        "preferred clone root must be first");
    Assert(roots.Any(r => r.Equals(Path.GetFullPath(@"E:\AI"), StringComparison.OrdinalIgnoreCase)),
        "must include other ready AI roots");
    Assert(roots.Any(r => r.Equals(Path.GetFullPath(@"E:\repos"), StringComparison.OrdinalIgnoreCase)),
        "must include existing repos roots");
    Assert(!roots.Any(r => r.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase)),
        "must not invent paths on drives that are not ready");
}

static void TestSavedLoginEmptyAnswer()
{
    string root = NewTemp();
    string? previous = Environment.GetEnvironmentVariable(LastUserFile.EnvPath);
    try
    {
        string path = Path.Combine(root, "last-user.json");
        File.WriteAllText(path,
            """{"saas1":{"userLogin":"MEDATECH","company":"CO","identifiedAt":"2026-11-12T00:00:00Z","source":"whoami"}}""");
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, path);
        string? saved = LastUserFile.ReadUserLogin("saas1");
        Assert(saved == "MEDATECH", "saved login must be MEDATECH");
        string prompt = LastUserFile.FormatPrompt(saved);
        Assert(prompt.Contains("[MEDATECH]", StringComparison.Ordinal), prompt);
        string os = Environment.UserName ?? "";
        if (!os.Equals("MEDATECH", StringComparison.OrdinalIgnoreCase))
            Assert(!prompt.Contains(os, StringComparison.OrdinalIgnoreCase), "prompt must not contain OS user " + os);
        string? requested = LastUserFile.ChooseRequestedUser(null, "", saved);
        var bound = BobHours.BindSessionOwner(null, requested);
        Assert(bound.Ok && bound.UserLogin == "MEDATECH", bound.Detail + "/" + bound.UserLogin);
    }
    finally
    {
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, previous);
        DeleteTempDirectory(root);
    }
}

static void TestHoursUserWinsOverSaved()
{
    string? requested = LastUserFile.ChooseRequestedUser("OTHER", "", "MEDATECH");
    Assert(requested == "OTHER", "flag must win, got " + requested);
    var bound = BobHours.BindSessionOwner(null, requested);
    Assert(bound.Ok && bound.UserLogin == "OTHER", bound.Detail);
}

static void TestInvalidSavedLoginIgnored()
{
    string root = NewTemp();
    string? previous = Environment.GetEnvironmentVariable(LastUserFile.EnvPath);
    try
    {
        string path = Path.Combine(root, "last-user.json");
        File.WriteAllText(path,
            """{"saas1":{"userLogin":"bad login","access_token":"nope"},"saas2":{"userLogin":"OKUSER"}}""");
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, path);
        Assert(LastUserFile.ReadUserLogin("saas1") == null, "space login must not be suggested");
        string prompt = LastUserFile.FormatPrompt(LastUserFile.ReadUserLogin("saas1"));
        Assert(!prompt.Contains("[", StringComparison.Ordinal), prompt);
        Assert(LastUserFile.ReadUserLogin("saas2") == "OKUSER", "valid sibling key must still read");
        Assert(LastUserFile.ChooseRequestedUser(null, "", LastUserFile.ReadUserLogin("saas1")) == null,
            "empty Enter must not bind a refused login");
    }
    finally
    {
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, previous);
        DeleteTempDirectory(root);
    }
}

static void TestSessionSourceSaved()
{
    string root = NewTemp();
    try
    {
        Assert(SessionLoginMeta.ClassifySource(null, "", "MEDATECH") == SessionLoginMeta.SourceSaved,
            "empty Enter must classify saved");
        SessionLoginMeta.WriteIdentity(root, "MEDATECH", SessionLoginMeta.SourceSaved);
        using var id = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "identity.json")));
        Assert(id.RootElement.GetProperty("priorityUserLogin").GetString() == "MEDATECH", "login");
        Assert(id.RootElement.GetProperty("userLoginSource").GetString() == "saved", "source");
        var routing = new BobHours.CustomerHoursConfig(true, "", "cust", "SC1", "", "", "ok");
        SessionLoginMeta.WriteHoursRoutingLogin(root, "MEDATECH", routing);
        using var hours = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "hours-routing.json")));
        Assert(hours.RootElement.GetProperty("on_behalf_of").GetString() == "MEDATECH", "hours login");
        Assert(!hours.RootElement.TryGetProperty("user", out _), "must not rename webhook user field");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestSessionSourceFlagMatchesWhoami()
{
    Assert(SessionLoginMeta.ClassifySource("MEDATECH", "", "MEDATECH") == SessionLoginMeta.SourceFlag,
        "matching --hours-user must still be flag");
}

static void TestSessionSourceTypedIsClient()
{
    Assert(SessionLoginMeta.ClassifySource(null, "MEDATECH", "MEDATECH") == SessionLoginMeta.SourceClient,
        "typed answer must be client even when it matches saved");
    Assert(SessionLoginMeta.ClassifySource(null, "OTHER", "MEDATECH") == SessionLoginMeta.SourceClient,
        "typed OTHER must be client");
}

static void TestSessionIdentityNoSecrets()
{
    string root = NewTemp();
    try
    {
        SessionLoginMeta.WriteIdentity(root, "MEDATECH", SessionLoginMeta.SourceFlag);
        string text = File.ReadAllText(Path.Combine(root, "identity.json"));
        Assert(!text.Contains("access_token", StringComparison.Ordinal)
            && !text.Contains("refresh_token", StringComparison.Ordinal)
            && !text.Contains("password", StringComparison.OrdinalIgnoreCase),
            "identity must not look like secrets: " + text);
        Assert(text.Contains("\"userLoginSource\":\"flag\"", StringComparison.Ordinal)
            || text.Contains("\"userLoginSource\": \"flag\"", StringComparison.Ordinal),
            "source flag missing: " + text);
    }
    finally { DeleteTempDirectory(root); }
}

static void TestHoursRoutingNoSecrets()
{
    string root = NewTemp();
    try
    {
        var routing = new BobHours.CustomerHoursConfig(true, "", "cust", "SC1", "", "", "ok");
        SessionLoginMeta.WriteHoursRoutingLogin(root, "MEDATECH", routing);
        string text = File.ReadAllText(Path.Combine(root, "hours-routing.json"));
        Assert(text.Contains("on_behalf_of", StringComparison.Ordinal), text);
        Assert(!text.Contains("\"user\"", StringComparison.Ordinal), "must not rename webhook user field: " + text);
        Assert(!text.Contains("access_token", StringComparison.Ordinal)
            && !text.Contains("password", StringComparison.OrdinalIgnoreCase),
            "hours-routing must not look like secrets: " + text);
    }
    finally { DeleteTempDirectory(root); }
}

static void TestHoursUserDisagreesWithWhoami()
{
    string root = NewTemp();
    string? previous = Environment.GetEnvironmentVariable(LastUserFile.EnvPath);
    try
    {
        string path = Path.Combine(root, "last-user.json");
        string original = """{"saas1":{"userLogin":"MEDATECH","company":"CO","identifiedAt":"2026-11-12T00:00:00Z","source":"whoami"}}""";
        File.WriteAllText(path, original);
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, path);
        string? whoami = LastUserFile.ReadUserLogin("saas1");
        Assert(whoami == "MEDATECH", "whoami fixture must be MEDATECH");
        LastUserFile.GrokStartCount = 0;
        var check = LastUserFile.CheckFlagAgainstWhoami("OTHERUSER", whoami);
        Assert(!check.Ok && check.ExitCode == 2, check.Detail);
        Assert(check.Detail.Contains("OTHERUSER", StringComparison.Ordinal)
            && check.Detail.Contains("MEDATECH", StringComparison.Ordinal), check.Detail);
        if (check.Ok)
            LastUserFile.NoteGrokStart();
        Assert(LastUserFile.GrokStartCount == 0, "grok start count must stay 0");
        Assert(File.ReadAllText(path) == original, "last-user.json must still contain MEDATECH");
        Assert(File.ReadAllText(path).Contains("MEDATECH", StringComparison.Ordinal), "MEDATECH must remain");
        Assert(!File.ReadAllText(path).Contains("OTHERUSER", StringComparison.Ordinal), "flag must not be written");
    }
    finally
    {
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, previous);
        DeleteTempDirectory(root);
    }
}

static void TestHoursUserMatchesOrAbsentWhoami()
{
    var match = LastUserFile.CheckFlagAgainstWhoami("MEDATECH", "MEDATECH");
    Assert(match.Ok && match.ExitCode == 0, match.Detail);
    var absent = LastUserFile.CheckFlagAgainstWhoami("OTHERUSER", null);
    Assert(absent.Ok && absent.ExitCode == 0, "no whoami pair must not exit 2: " + absent.Detail);
    var noFlag = LastUserFile.CheckFlagAgainstWhoami(null, "MEDATECH");
    Assert(noFlag.Ok, noFlag.Detail);
}

static void TestRedirectedSavedLoginBinds()
{
    string root = NewTemp();
    string? previous = Environment.GetEnvironmentVariable(LastUserFile.EnvPath);
    try
    {
        string path = Path.Combine(root, "last-user.json");
        File.WriteAllText(path,
            """{"saas1":{"userLogin":"MEDATECH","company":"CO","identifiedAt":"2026-11-12T00:00:00Z","source":"whoami"}}""");
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, path);
        string? saved = LastUserFile.ReadUserLogin("saas1");
        Assert(saved == "MEDATECH", "saved login must be MEDATECH");
        // Redirected stdin: never Ask/ReadLine; bind saved via ResolveLaunchUser.
        string? requested = LastUserFile.ResolveLaunchUser(null, inputRedirected: true, saved, typedAnswer: null);
        Assert(requested == "MEDATECH", "redirected+saved must exit prompt step with MEDATECH, got " + requested);
        var bound = BobHours.BindSessionOwner(null, requested);
        Assert(bound.Ok && bound.UserLogin == "MEDATECH", bound.Detail + "/" + bound.UserLogin);
        string? flagWins = LastUserFile.ResolveLaunchUser("OTHER", inputRedirected: true, saved, null);
        Assert(flagWins == "OTHER", "--hours-user must still win when redirected");
    }
    finally
    {
        Environment.SetEnvironmentVariable(LastUserFile.EnvPath, previous);
        DeleteTempDirectory(root);
    }
}

static void TestRedirectedNoSavedUnavailable()
{
    string? requested = LastUserFile.ResolveLaunchUser(null, inputRedirected: true, savedLogin: null, typedAnswer: null);
    Assert(requested == null, "redirected with no saved and no --hours-user must not invent a login");
    var bound = BobHours.BindSessionOwner(null, requested);
    Assert(!bound.Ok, "must fail BindSessionOwner");
    Assert(bound.Detail.Contains("Priority USERLOGIN unavailable", StringComparison.Ordinal),
        "exit-2 detail must say Priority USERLOGIN unavailable, got " + bound.Detail);
    // Simulated launcher exit code when bind fails (Program returns 2).
    int exitCode = (!bound.Ok || string.IsNullOrEmpty(bound.UserLogin)) ? 2 : 0;
    Assert(exitCode == 2, "launcher must exit 2");
}

static void TestFr568GrokDetect()
{
    string root = NewTemp();
    try
    {
        string bin = Path.Combine(root, ".grok", "bin");
        Directory.CreateDirectory(bin);
        File.WriteAllText(Path.Combine(bin, "grok.exe"), "MZ");
        var r = CliDetect.DetectGrok(root);
        Assert(r.Present && r.Path != null && r.Path.EndsWith("grok.exe", StringComparison.OrdinalIgnoreCase), "grok.exe must count");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestFr570ResolveExeMatchesChoice()
{
    string root = NewTemp();
    try
    {
        string home = Path.Combine(root, "home");
        string local = Path.Combine(root, "local");
        Directory.CreateDirectory(Path.Combine(home, ".grok", "bin"));
        Directory.CreateDirectory(Path.Combine(local, "cursor-agent"));
        Directory.CreateDirectory(Path.Combine(local, "opencode"));
        string grokExe = Path.Combine(home, ".grok", "bin", "grok.exe");
        string cursorCmd = Path.Combine(local, "cursor-agent", "agent.cmd");
        string openExe = Path.Combine(local, "opencode", "opencode.exe");
        string grokAgentTrap = Path.Combine(home, ".grok", "bin", "agent.exe");
        File.WriteAllText(grokExe, "fake");
        File.WriteAllText(cursorCmd, "@echo off");
        File.WriteAllText(openExe, "fake");
        File.WriteAllText(grokAgentTrap, "trap");

        Assert(CliLaunch.TryParseKind("grok", out CliKind gk) && gk == CliKind.Grok, "parse grok");
        Assert(CliLaunch.TryParseKind("cursor", out CliKind ck) && ck == CliKind.Cursor, "parse cursor");
        Assert(CliLaunch.TryParseKind("opencode", out CliKind ok) && ok == CliKind.OpenCode, "parse opencode");
        Assert(!CliLaunch.TryParseKind("claude", out _), "unknown kind must fail");

        string? resolvedGrok = CliLaunch.ResolveExecutable(CliKind.Grok, home, new[] { grokExe });
        Assert(resolvedGrok != null && resolvedGrok.Equals(Path.GetFullPath(grokExe), StringComparison.OrdinalIgnoreCase),
            "grok choice must resolve to grok.exe override");
        string? resolvedCursor = CliLaunch.ResolveExecutable(CliKind.Cursor, home, new[] { cursorCmd });
        Assert(resolvedCursor != null && resolvedCursor.Equals(Path.GetFullPath(cursorCmd), StringComparison.OrdinalIgnoreCase),
            "cursor choice must resolve to cursor-agent\\agent.cmd, got " + resolvedCursor);
        Assert(!resolvedCursor!.Contains(".grok", StringComparison.OrdinalIgnoreCase),
            "cursor must never pick %USERPROFILE%\\.grok\\bin\\agent.exe");
        string? resolvedOpen = CliLaunch.ResolveExecutable(CliKind.OpenCode, home, new[] { openExe });
        Assert(resolvedOpen != null && resolvedOpen.Equals(Path.GetFullPath(openExe), StringComparison.OrdinalIgnoreCase),
            "opencode choice must resolve to opencode.exe");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestFr568CursorRejectsGrokAgent()
{
    string root = NewTemp();
    try
    {
        string bin = Path.Combine(root, ".grok", "bin");
        Directory.CreateDirectory(bin);
        File.WriteAllText(Path.Combine(bin, "agent.exe"), "MZ");
        var r = CliDetect.DetectCursor(root, Path.Combine(root, "AppData", "Local"));
        Assert(!r.Present, "Cursor must be missing when only Grok agent.exe exists");
        Assert(r.Reason.Contains("reject", StringComparison.OrdinalIgnoreCase), r.Reason);
    }
    finally { DeleteTempDirectory(root); }
}

static void TestFr568InstallerInteractiveUser()
{
    var seen = new List<CliInstallPlan>();
    var sys = CliInstall.Run(CliKind.Grok, p => { seen.Add(p); return 0; }, localSystem: true, user: "SYSTEM");
    Assert(!sys.Ran && sys.LocalSystem, "LocalSystem must not run installer");
    Assert(seen.Count == 0, "SYSTEM must not invoke runner");
    var user = CliInstall.Run(CliKind.Cursor, p => { seen.Add(p); return 0; }, localSystem: false, user: "simon");
    Assert(user.Ran && !user.LocalSystem && user.User == "simon", "interactive user must run");
    Assert(seen.Count == 1 && seen[0].Arguments.Contains("cursor.com/install", StringComparison.Ordinal), "cursor official irm");
    Assert(!seen[0].Arguments.Contains("scoop", StringComparison.OrdinalIgnoreCase), "no Scoop");
}

static void TestFr568OpenCodeParseFixture()
{
    string html = """
        <html><a href="https://github.example.invalid/sst/opencode/releases/download/v1.0.0/opencode-windows-x64.exe">win</a></html>
        """;
    string? url = CliInstall.ParseOpenCodeWindowsBinaryUrl(html);
    Assert(url != null && url.Contains("opencode-windows", StringComparison.OrdinalIgnoreCase), "parse windows binary from page fixture");
    int net = 0;
    var r = CliInstall.Run(
        CliKind.OpenCode,
        p => { net++; Assert(p.Arguments.Contains(url!, StringComparison.Ordinal), "plan must use parsed url"); return 0; },
        localSystem: false,
        user: "simon",
        openCodeBinaryUrl: url);
    Assert(r.Ran && net == 1, "fake runner only");
}

static void TestMrb575OpenCodePlanPath()
{
    var plan = CliInstall.Plan(CliKind.OpenCode, "https://github.example.invalid/opencode-windows-x64.exe");
    Assert(plan.Arguments.Contains("$env:USERPROFILE", StringComparison.Ordinal), "PowerShell profile expansion");
    Assert(!plan.Arguments.Contains("%USERPROFILE%", StringComparison.Ordinal), "cmd %USERPROFILE% is literal in PowerShell");
    Assert(plan.Arguments.Contains("New-Item", StringComparison.OrdinalIgnoreCase) &&
           plan.Arguments.Contains("$env:USERPROFILE\\.opencode\\bin", StringComparison.Ordinal),
        "must create .opencode\\bin, not Split-Path parent only");
    Assert(!plan.Arguments.Contains("Split-Path", StringComparison.OrdinalIgnoreCase), "Split-Path of bin only creates .opencode");
}

static void TestFr570CursorOpenCodeOmitGrokFlags()
{
    string work = @"C:\work\project";
    string helpCursor = "Usage: agent --workspace <dir> --trust --print --resume";
    string helpOpen = "Usage: opencode --dir <path> [prompt]";
    var cursorArgs = CliLaunch.BuildArguments(CliKind.Cursor, work, "sess", "rules", "brief", false, helpCursor);
    var openArgs = CliLaunch.BuildArguments(CliKind.OpenCode, work, "sess", "rules", "brief", false, helpOpen);
    Assert(!CliLaunch.ArgsContainGrokOnlyFlag(cursorArgs), "cursor argv has Grok flag: " + string.Join(' ', cursorArgs));
    Assert(!CliLaunch.ArgsContainGrokOnlyFlag(openArgs), "opencode argv has Grok flag: " + string.Join(' ', openArgs));
    Assert(cursorArgs.Contains("--workspace") && cursorArgs.Contains(work), "cursor should use --workspace when help lists it");
    Assert(openArgs.Contains("--dir") && openArgs.Contains(work), "opencode should use --dir when help lists it");
    Assert(!cursorArgs.Contains("--cwd") && !cursorArgs.Contains("--session-id") && !cursorArgs.Contains("--rules"),
        "cursor must omit Grok MCP launch flags");
    Assert(!openArgs.Contains("--cwd") && !openArgs.Contains("--session-id") && !openArgs.Contains("--rules"),
        "opencode must omit Grok MCP launch flags");
    CliLaunch.AssertNoGrokOnlyFlags(cursorArgs);
    CliLaunch.AssertNoGrokOnlyFlags(openArgs);
}

static void TestFr570UnknownHelpWorkDirOnly()
{
    string work = @"C:\work\only";
    var cursorEmpty = CliLaunch.BuildArguments(CliKind.Cursor, work, "s", "r", "brief", false, helpText: "");
    var openEmpty = CliLaunch.BuildArguments(CliKind.OpenCode, work, "s", "r", "brief", false, helpText: null);
    var openUnknown = CliLaunch.BuildArguments(CliKind.OpenCode, work, "s", "r", "brief", false, helpText: "no flags documented");
    Assert(cursorEmpty.Count == 0, "empty cursor help => argv empty (PSI.WorkingDirectory only), got " + cursorEmpty.Count);
    Assert(openEmpty.Count == 0, "null opencode help => argv empty");
    Assert(!openUnknown.Contains("--dir") && !openUnknown.Contains("--cwd"),
        "unknown opencode help must not invent workspace flags");
    Assert(!CliLaunch.ArgsContainGrokOnlyFlag(cursorEmpty) && !CliLaunch.ArgsContainGrokOnlyFlag(openUnknown),
        "unknown-help argv must stay free of Grok flags");
}

static void TestFr570HoursRelaunchChosenCli()
{
    string work = @"C:\work\hours";
    string session = Guid.NewGuid().ToString();
    string rules = "rules-body";
    string hoursPrompt = "report your hours now";
    string helpCursor = "Usage: agent --workspace <dir> --trust --resume";
    var grokResume = CliLaunch.BuildArguments(CliKind.Grok, work, session, rules, "brief", false, null,
        hoursRelaunch: true, hoursPrompt: hoursPrompt);
    var cursorResume = CliLaunch.BuildArguments(CliKind.Cursor, work, session, rules, "brief", false, helpCursor,
        hoursRelaunch: true, hoursPrompt: hoursPrompt);
    var openResume = CliLaunch.BuildArguments(CliKind.OpenCode, work, session, rules, "brief", false, "",
        hoursRelaunch: true, hoursPrompt: hoursPrompt);

    Assert(grokResume.Contains("--resume") && grokResume.Contains(session) && grokResume.Contains("--cwd"),
        "Grok hours relaunch keeps RunGrok resume shape");
    Assert(cursorResume.Contains("--resume") || cursorResume.Contains(hoursPrompt),
        "cursor hours relaunch must use cursor argv, not grok.exe");
    Assert(!CliLaunch.ArgsContainGrokOnlyFlag(cursorResume), "cursor hours argv must omit Grok flags");
    Assert(openResume.Count == 0 || !CliLaunch.ArgsContainGrokOnlyFlag(openResume),
        "opencode hours relaunch with empty help is work-dir-only / no Grok flags");
    // Hardcoded grok.exe must not appear in non-Grok argv (FR #570 hours loop uses ChosenExe).
    Assert(!cursorResume.Any(a => a.Contains("grok.exe", StringComparison.OrdinalIgnoreCase)),
        "cursor hours argv must not hardcode grok.exe");
    Assert(!openResume.Any(a => a.Contains("grok.exe", StringComparison.OrdinalIgnoreCase)),
        "opencode hours argv must not hardcode grok.exe");
}


static FoundCli Fake(AgentCliKind kind, string name) =>
    new(kind, Path.Combine(Path.GetTempPath(), "fake-cli", name));

static void TestFr569ZeroInteractiveOffersMenu()
{
    var r = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: false, pick: null, installNow: null);
    Assert(r.Action == CliChooserAction.OfferZeroMenu, "zero interactive must offer the four choices");
    Assert(r.Message.Contains("Grok", StringComparison.Ordinal)
        && r.Message.Contains("Cursor", StringComparison.Ordinal)
        && r.Message.Contains("OpenCode", StringComparison.Ordinal)
        && r.Message.Contains("refuse", StringComparison.OrdinalIgnoreCase),
        "menu must name Grok, Cursor, OpenCode, and refuse");
    Assert(!r.Download && r.ExitCode == 0, "offer is not a download and not an exit yet");
}

static void TestFr569RefuseNoDownload()
{
    var r = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: false, pick: AgentCliKind.Refuse, installNow: null);
    Assert(r.Action == CliChooserAction.Refuse, "refuse must refuse");
    Assert(r.Message == CliChooser.NotFoundLine, "refuse must print the not-found line");
    Assert(r.ExitCode == 1 && !r.Download, "refuse exits 1 and never downloads");
}

static void TestFr569ExactlyOneLaunches()
{
    var only = Fake(AgentCliKind.OpenCode, "opencode.exe");
    var r = CliChooser.Decide(new[] { only }, stdinRedirected: true, pick: null, installNow: null);
    Assert(r.Action == CliChooserAction.Launch && r.Launch == only, "exactly one must launch that CLI");
    Assert(!r.Download, "launch is not a download");
}

static void TestFr569ManyAskWhichNotFuel()
{
    var grok = Fake(AgentCliKind.Grok, "grok.exe");
    var cursor = Fake(AgentCliKind.Cursor, "agent.cmd");
    var r = CliChooser.Decide(new[] { grok, cursor }, stdinRedirected: false, pick: null, installNow: null);
    Assert(r.Action == CliChooserAction.OfferWhich, "two installed must ask which");
    Assert(r.Message.Contains("Do not pick by token fuel", StringComparison.Ordinal),
        "which-prompt must forbid token fuel, got: " + r.Message);
    Assert(r.Launch == null && !r.Download, "which-prompt must not auto-launch or download");
    var picked = CliChooser.Decide(new[] { grok, cursor }, stdinRedirected: false, pick: AgentCliKind.Cursor, installNow: null);
    Assert(picked.Launch?.Kind == AgentCliKind.Cursor, "user pick wins; fuel does not");
}

static void TestFr569MissingAsksInstall()
{
    var r = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: false, pick: AgentCliKind.Grok, installNow: null);
    Assert(r.Action == CliChooserAction.AskInstallNow, "missing pick must ask before download");
    Assert(r.Message == CliChooser.InstallNowPrompt, "prompt must be Install it now?");
    Assert(!r.Download, "the ask must happen before any download");
    var no = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: false, pick: AgentCliKind.Grok, installNow: false);
    Assert(no.Action == CliChooserAction.Refuse && !no.Download && no.ExitCode == 1, "No must not download");
    var yes = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: false, pick: AgentCliKind.Grok, installNow: true);
    Assert(!yes.Download, "yes still must not download (installer is FR-003)");
}

static void TestFr569RedirectedNoPromptNoDownload()
{
    var none = CliChooser.Decide(Array.Empty<FoundCli>(), stdinRedirected: true, pick: null, installNow: null);
    Assert(none.Action == CliChooserAction.Refuse && none.ExitCode == 1 && !none.Download,
        "redirected zero must not prompt or download");
    var grok = Fake(AgentCliKind.Grok, "grok.exe");
    var cursor = Fake(AgentCliKind.Cursor, "cursor.exe");
    var many = CliChooser.Decide(new[] { grok, cursor }, stdinRedirected: true, pick: null, installNow: null);
    Assert(many.Action == CliChooserAction.Refuse && !many.Download,
        "redirected many must not prompt, pick by fuel, or download");
}

static void TestFr569CursorOnlyLaunches()
{
    var cursor = Fake(AgentCliKind.Cursor, "agent.cmd");
    var r = CliChooser.Decide(new[] { cursor }, stdinRedirected: false, pick: null, installNow: null);
    Assert(r.Action == CliChooserAction.Launch && r.Launch?.Kind == AgentCliKind.Cursor,
        "a missing grok.exe must not be the only success path");
}

static void TestMrb577CursorAgentNotGui()
{
    string root = NewTemp();
    string local = Path.Combine(root, "Local");
    try
    {
        var paths = CliChooser.CursorAgentCandidatePaths(local);
        Assert(paths.All(p => p.Contains("cursor-agent", StringComparison.OrdinalIgnoreCase)),
            "Cursor candidates must live under cursor-agent");
        Assert(paths.Any(p => p.EndsWith("agent.cmd", StringComparison.OrdinalIgnoreCase)),
            "must include agent.cmd");
        Assert(!paths.Any(p => p.EndsWith("Cursor.exe", StringComparison.OrdinalIgnoreCase)),
            "must not probe Cursor.exe GUI");
        Assert(CliChooser.IsRejectedCursorPath(Path.Combine(local, "Programs", "cursor", "Cursor.exe")),
            "Cursor.exe GUI must be rejected");
        Directory.CreateDirectory(Path.Combine(local, "Programs", "cursor"));
        File.WriteAllText(Path.Combine(local, "Programs", "cursor", "Cursor.exe"), "gui");
        Assert(CliChooser.ResolveCursorAgent(local) == null,
            "GUI-only install must not resolve as Cursor agent");
        Directory.CreateDirectory(Path.Combine(local, "cursor-agent"));
        string cmd = Path.Combine(local, "cursor-agent", "agent.cmd");
        File.WriteAllText(cmd, "@echo off");
        string? resolved = CliChooser.ResolveCursorAgent(local);
        Assert(resolved != null && resolved.Equals(Path.GetFullPath(cmd), StringComparison.OrdinalIgnoreCase),
            "agent.cmd must resolve, got " + resolved);
    }
    finally { DeleteTempDirectory(root); }
}

static void TestMrb577RejectGrokAgentAsCursor()
{
    string root = NewTemp();
    try
    {
        string trap = Path.Combine(root, ".grok", "bin", "agent.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(trap)!);
        File.WriteAllText(trap, "trap");
        Assert(CliChooser.IsRejectedCursorPath(trap), ".grok\\bin\\agent.exe must be rejected");
        string? resolved = CliChooser.ResolveCursorAgent(Path.Combine(root, "Local"), new[] { trap });
        Assert(resolved == null, "override that is the grok agent trap must not resolve as Cursor");
    }
    finally { DeleteTempDirectory(root); }
}

static void TestMrb577WhichForbidsFuelExact()
{
    Assert(CliChooser.WhichPrompt.Contains("Do not pick by token fuel", StringComparison.Ordinal),
        CliChooser.WhichPrompt);
}

static void TestMrb577HoursRelaunchChosenExe()
{
    var cursor = Fake(AgentCliKind.Cursor, "agent.cmd");
    var grok = Fake(AgentCliKind.Grok, "grok.exe");
    Assert(CliChooser.HoursRelaunchExe(cursor) == cursor.Path, "cursor hours must relaunch cursor path");
    Assert(CliChooser.HoursRelaunchExe(grok) == grok.Path, "grok hours must relaunch grok path");
    Assert(CliChooser.HoursRelaunchExe(cursor) != grok.Path, "cursor hours must not hardcode grok.exe");
}

static void TestFr609PickListDownDownEnter()
{
    var items = new[] { "alpha", "bravo", "charlie", "delta" };
    var io = new ScriptedPickIO(
        redirected: false,
        keys: new[]
        {
            new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });
    int? picked = ConsolePick.PickList(items, "Pick one:", io);
    Assert(picked == 2, $"expected index 2, got {picked}");
    Assert(io.Writes.Any(w => w.Contains(">  3. charlie", StringComparison.Ordinal)
            || w.Contains("> 3. charlie", StringComparison.Ordinal)),
        "highlight row for charlie should be written");
}

static void TestFr609PickListRedirectedOne()
{
    var items = new[] { "alpha", "bravo", "charlie" };
    var io = new ScriptedPickIO(redirected: true, lines: new[] { "1" });
    int? picked = ConsolePick.PickList(items, "Pick one:", io);
    Assert(picked == 0, $"expected index 0 for input 1, got {picked}");
}

static void TestFr609PickListRedirectedTwo()
{
    var items = new[] { "alpha", "bravo", "charlie" };
    var io = new ScriptedPickIO(redirected: true, lines: new[] { "2" });
    int? picked = ConsolePick.PickList(items, "Pick one:", io);
    Assert(picked == 1, $"expected index 1 for input 2, got {picked}");
}

static void TestFr609PickListEscCancels()
{
    var items = new[] { "alpha", "bravo" };
    var io = new ScriptedPickIO(
        redirected: false,
        keys: new[] { new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false) });
    int? picked = ConsolePick.PickList(items, "Pick one:", io);
    Assert(picked is null, "Esc must cancel with null");
}

static void TestFr613WaitSpinnerGlyphs()
{
    var frames = ConsoleWait.PreviewFrames("fetch", 4);
    Assert(frames.Count == 4, "expected 4 frames");
    var glyphs = frames.Select(f => f[^1]).Distinct().ToList();
    Assert(glyphs.Count >= 2, $"expected >=2 distinct glyphs, got {glyphs.Count}: {string.Join(',', glyphs)}");
    Assert(glyphs.All(g => ConsoleWait.Glyphs.Contains(g)), "glyphs must be from |/-\\ set");
}

static void TestFr613WaitQuietNoWrites()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        var redirected = new ScriptedWaitIO(redirected: true);
        int n = ConsoleWait.WithWait("clone", () => 7, redirected, TimeSpan.FromMilliseconds(1));
        Assert(n == 7, "action result");
        Assert(redirected.Writes.Count == 0, "redirected must not write spinner");

        ConsoleWait.Quiet = true;
        var tty = new ScriptedWaitIO(redirected: false);
        n = ConsoleWait.WithWait("clone", () => 9, tty, TimeSpan.FromMilliseconds(1));
        Assert(n == 9, "quiet action result");
        Assert(tty.Writes.Count == 0, "Quiet must not write spinner (MCP stdout)");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr619HoursFetchTtyDone()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        var result = HoursListFetch.Run(() =>
        {
            Thread.Sleep(80);
            return new HoursFetchResult(true, "{\"ok\":true}", "");
        }, tty);
        Assert(result.Ok && result.Json == "{\"ok\":true}", "success payload");
        string joined = string.Concat(tty.Writes);
        Assert(ConsoleWait.Glyphs.Any(g => joined.Contains(g)), "TTY hours fetch must show a spinner glyph");
        Assert(joined.Contains("hours fetch done", StringComparison.Ordinal), "success settles done");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr619HoursFetchFailNoDone()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        const string detail = "hours GET HTTP 503; delivery unverified; no retry";
        var result = HoursListFetch.Run(() => new HoursFetchResult(false, "", detail), tty);
        Assert(!result.Ok && result.Detail == detail && result.Json.Length == 0, "failure detail preserved");
        string joined = string.Concat(tty.Writes);
        Assert(!joined.Contains("done", StringComparison.Ordinal), "failed hours fetch must not print done");
        var redirected = new ScriptedWaitIO(redirected: true);
        result = HoursListFetch.Run(() => new HoursFetchResult(false, "x", detail), redirected);
        Assert(!result.Ok && result.Detail == detail, "redirected failure detail");
        Assert(redirected.Writes.Count == 0, "redirected hours fetch must not write");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr619HoursFetchWired()
{
    string[] candidates =
    {
        Path.GetFullPath(Path.Combine("..", "launcher", "Program.cs")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "launcher", "Program.cs")),
        Path.GetFullPath("launcher/Program.cs"),
    };
    string? path = candidates.FirstOrDefault(File.Exists);
    Assert(path is not null, "launcher/Program.cs not found for hours fetch pin");
    string src = File.ReadAllText(path!);
    Assert(src.Contains("HoursListFetch.Run", StringComparison.Ordinal), "hours GET must use HoursListFetch");
    Assert(src.Contains("case \"--check-hours\"", StringComparison.Ordinal), "--check-hours stays");
    Assert(src.Contains("HOURS OVERRIDE: session", StringComparison.Ordinal), "SKIP override log stays");
    Assert(src.Contains("Relaunching the agent to report hours", StringComparison.Ordinal), "relaunch prompt stays");
}

static string ReadLauncherProgram()
{
    string[] candidates =
    {
        Path.GetFullPath(Path.Combine("..", "launcher", "Program.cs")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "launcher", "Program.cs")),
        Path.GetFullPath("launcher/Program.cs"),
    };
    string? path = candidates.FirstOrDefault(File.Exists);
    Assert(path is not null, "launcher/Program.cs not found");
    return File.ReadAllText(path!);
}

static void TestFr618McpRegisterDoctorWired()
{
    string src = ReadLauncherProgram();
    int method = src.IndexOf("static void EnsureGrokMcp(", StringComparison.Ordinal);
    Assert(method >= 0, "EnsureGrokMcp missing");
    int next = src.IndexOf("static string? FindNode(", StringComparison.Ordinal);
    Assert(next > method, "FindNode missing after EnsureGrokMcp");
    string window = src[method..next];
    int listWait = window.IndexOf("WithWait(\"mcp register\"", StringComparison.Ordinal);
    int listCapture = window.IndexOf("\"mcp\", \"list\"", StringComparison.Ordinal);
    int addWait = window.LastIndexOf("WithWait(\"mcp register\"", StringComparison.Ordinal);
    int addCall = window.IndexOf("\"mcp\", \"add\"", StringComparison.Ordinal);
    int doctorWait = window.IndexOf("WithWait(\"mcp doctor\"", StringComparison.Ordinal);
    int doctorCapture = window.IndexOf("\"mcp\", \"doctor\"", StringComparison.Ordinal);
    Assert(listWait >= 0 && listCapture > listWait, "mcp list must run inside WithWait");
    Assert(addWait >= 0 && addCall > addWait, "mcp add must run inside WithWait");
    Assert(doctorWait >= 0 && doctorCapture > doctorWait, "mcp doctor must run inside WithWait");
    Assert(window.Contains("grok mcp doctor {McpName}:", StringComparison.Ordinal), "doctor failure/success text unchanged");
    Assert(window.Contains("handshake failed", StringComparison.Ordinal), "doctor failure check stays");
    Assert(src.Contains("ConsoleWait.Quiet = true", StringComparison.Ordinal), "--mcp-catalog still sets Quiet");
}

static void TestFr618SkillsInspectWired()
{
    string src = ReadLauncherProgram();
    int method = src.IndexOf("static void GrokInspect(", StringComparison.Ordinal);
    Assert(method >= 0, "GrokInspect missing");
    string window = src.Substring(method, Math.Min(700, src.Length - method));
    int wait = window.IndexOf("WithWait(\"skills\"", StringComparison.Ordinal);
    int capture = window.IndexOf("\"inspect\"", StringComparison.Ordinal);
    Assert(wait >= 0 && capture > wait, "grok inspect must run inside WithWait");
    Assert(window.Contains("grok inspect sees priority-* skills", StringComparison.Ordinal), "skills inspect message stays");
}

static void TestFr616FuelReportTtyDone()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        var result = FuelReportFetch.Run(() =>
        {
            Thread.Sleep(80);
            return new FuelFetchResult(true, "{\"machines\":{}}", "");
        }, tty);
        Assert(result.Ok && result.Json.Contains("machines", StringComparison.Ordinal), "success payload");
        string joined = string.Concat(tty.Writes);
        Assert(ConsoleWait.Glyphs.Any(g => joined.Contains(g)), "TTY fuel report must show a spinner glyph");
        Assert(joined.Contains("fuel report done", StringComparison.Ordinal), "success settles done");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr616FuelReportFailNoDone()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        const string detail = "A task was canceled.";
        var result = FuelReportFetch.Run(() => new FuelFetchResult(false, "", detail), tty);
        Assert(!result.Ok && result.Detail == detail && result.Json.Length == 0, "failure detail preserved");
        string joined = string.Concat(tty.Writes);
        Assert(!joined.Contains("done", StringComparison.Ordinal), "failed fuel report must not print done");
        var redirected = new ScriptedWaitIO(redirected: true);
        result = FuelReportFetch.Run(() => new FuelFetchResult(false, "x", detail), redirected);
        Assert(!result.Ok && result.Detail == detail, "redirected failure detail");
        Assert(redirected.Writes.Count == 0, "redirected fuel report must not write");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr616FuelReportWired()
{
    string src = ReadLauncherProgram();
    Assert(src.Contains("FuelReportFetch.Run", StringComparison.Ordinal), "fuel GET must use FuelReportFetch");
    Assert(src.Contains("case \"--no-fuel-check\"", StringComparison.Ordinal), "--no-fuel-check stays");
    Assert(src.Contains("Continue anyway?", StringComparison.Ordinal), "Continue anyway prompt stays");
    Assert(src.Contains("if (!NoFuel) FuelCheck()", StringComparison.Ordinal), "--no-fuel-check skips FuelCheck");
}

static void TestFr613McpCatalogSetsQuiet()
{
    string[] candidates =
    {
        Path.GetFullPath(Path.Combine("..", "launcher", "Program.cs")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "launcher", "Program.cs")),
        Path.GetFullPath("launcher/Program.cs"),
    };
    string? path = candidates.FirstOrDefault(File.Exists);
    Assert(path is not null, "launcher/Program.cs not found for Quiet pin");
    string src = File.ReadAllText(path!);
    Assert(src.Contains("ConsoleWait.Quiet = true", StringComparison.Ordinal), "mcp-catalog path must set ConsoleWait.Quiet");
    Assert(src.Contains("--mcp-catalog", StringComparison.Ordinal), "expected --mcp-catalog branch");
}

static void TestMrb633WaitThrowNoDone()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        bool threw = false;
        try
        {
            ConsoleWait.WithWait("clone", () => { throw new InvalidOperationException("boom"); }, tty, TimeSpan.FromMilliseconds(1));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Assert(threw, "action exception must propagate");
        string joined = string.Concat(tty.Writes);
        Assert(!joined.Contains("done", StringComparison.Ordinal), "must not print done when action throws");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestMrb633WaitTtyGlyphsAndSettle()
{
    bool prev = ConsoleWait.Quiet;
    try
    {
        ConsoleWait.Quiet = false;
        var tty = new ScriptedWaitIO(redirected: false);
        int n = ConsoleWait.WithWait("fetch", () =>
        {
            Thread.Sleep(50);
            return 3;
        }, tty, TimeSpan.FromMilliseconds(5));
        Assert(n == 3, "action result");
        string joined = string.Concat(tty.Writes);
        Assert(joined.Contains("fetch done", StringComparison.Ordinal), "settle must print fetch done");
        var glyphHits = ConsoleWait.Glyphs.Count(g => joined.Contains(g));
        Assert(glyphHits >= 1, "TTY path must emit at least one spinner glyph");
    }
    finally { ConsoleWait.Quiet = prev; }
}

static void TestFr611ProjectPickListFirstFolder()
{
    var folders = new[] { "SC100", "SC200" };
    var items = ProjectFolderPick.BuildItems(folders);
    // index 0 = customer level; Down once -> first folder; Enter
    var io = new ScriptedPickIO(
        redirected: false,
        keys: new[]
        {
            new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });
    int? picked = ConsolePick.PickList(items, "Pick project:", io);
    Assert(ProjectFolderPick.TryResolve(folders, picked, out string? project, out bool haitchNew), "resolve must succeed");
    Assert(!haitchNew && project == "SC100", $"expected SC100, got project={project} haitch={haitchNew}");
}

static void TestFr611ProjectResolveRows()
{
    var folders = new[] { "SC100", "SO9" };
    Assert(ProjectFolderPick.BuildItems(folders).Count == 4, "customer-level + 2 folders + New");
    Assert(ProjectFolderPick.TryResolve(folders, 0, out string? p0, out bool h0) && p0 is null && !h0, "0 = customer level");
    Assert(ProjectFolderPick.TryResolve(folders, 1, out string? p1, out bool h1) && p1 == "SC100" && !h1, "1 = first folder");
    Assert(ProjectFolderPick.TryResolve(folders, 2, out string? p2, out bool h2) && p2 == "SO9" && !h2, "2 = second folder");
    Assert(ProjectFolderPick.TryResolve(folders, 3, out string? p3, out bool h3) && p3 is null && h3, "3 = New/Haitch");
    Assert(!ProjectFolderPick.TryResolve(folders, null, out _, out _), "null index cancels");
    Assert(!ProjectFolderPick.TryResolve(folders, 99, out _, out _), "out of range cancels");
}

static void TestFr611ProjectRedirectedFirstFolder()
{
    var folders = new[] { "SC100", "SC200" };
    var items = ProjectFolderPick.BuildItems(folders);
    // redirected 1-based: 1=customer level, 2=SC100
    var io = new ScriptedPickIO(redirected: true, lines: new[] { "2" });
    int? picked = ConsolePick.PickList(items, "Pick project:", io);
    Assert(ProjectFolderPick.TryResolve(folders, picked, out string? project, out bool haitchNew), "resolve");
    Assert(project == "SC100" && !haitchNew, $"expected SC100 from redirected 2, got {project}");
}

static void TestFr610CustomerPickListFirstCustomer()
{
    var names = new[] { "rees", "other" };
    var items = CustomerPick.BuildItems(names);
    var io = new ScriptedPickIO(
        redirected: false,
        keys: new[]
        {
            new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });
    int? picked = ConsolePick.PickList(items, "Pick customer:", io);
    Assert(CustomerPick.TryResolve(names, picked, out int customerIndex, out bool haitchNew), "resolve must succeed");
    Assert(!haitchNew && customerIndex == 0 && names[customerIndex] == "rees", $"expected rees, index={customerIndex} haitch={haitchNew}");
}

static void TestFr610CustomerResolveRows()
{
    var names = new[] { "rees", "other" };
    Assert(CustomerPick.BuildItems(names).Count == 4, "General + 2 customers + New");
    Assert(CustomerPick.TryResolve(names, 0, out int i0, out bool h0) && i0 < 0 && !h0, "0 = General");
    Assert(CustomerPick.TryResolve(names, 1, out int i1, out bool h1) && i1 == 0 && !h1, "1 = first customer");
    Assert(CustomerPick.TryResolve(names, 2, out int i2, out bool h2) && i2 == 1 && !h2, "2 = second customer");
    Assert(CustomerPick.TryResolve(names, 3, out int i3, out bool h3) && i3 < 0 && h3, "3 = New/Haitch");
    Assert(!CustomerPick.TryResolve(names, null, out _, out _), "null index cancels");
    Assert(!CustomerPick.TryResolve(names, 99, out _, out _), "out of range cancels");
}

static void TestFr610CustomerRedirectedFirst()
{
    var names = new[] { "rees", "other" };
    var items = CustomerPick.BuildItems(names);
    var io = new ScriptedPickIO(redirected: true, lines: new[] { "2" });
    int? picked = ConsolePick.PickList(items, "Pick customer:", io);
    Assert(CustomerPick.TryResolve(names, picked, out int customerIndex, out bool haitchNew), "resolve");
    Assert(customerIndex == 0 && names[customerIndex] == "rees" && !haitchNew, $"expected rees from redirected 2, index={customerIndex}");
}

static string NewTemp()
{
    string path = Path.Combine(Path.GetTempPath(), "priority-agent-tests-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
}

static void DeleteTempDirectory(string path)
{
    try
    {
        if (!Directory.Exists(path)) return;
        foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            try { File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly); }
            catch (Exception ex) { Console.Error.WriteLine($"WARN cleanup attributes for {file}: {ex.Message}"); }
        }
        foreach (string directory in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
        {
            try { File.SetAttributes(directory, File.GetAttributes(directory) & ~FileAttributes.ReadOnly); }
            catch (Exception ex) { Console.Error.WriteLine($"WARN cleanup attributes for {directory}: {ex.Message}"); }
        }
        Directory.Delete(path, true);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"WARN cleanup {path}: {ex.Message}");
    }
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
