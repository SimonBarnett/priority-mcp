using System.Text.Json;

namespace PriorityAgent;

/// <summary>FR #813: new customer → Plan MCP + CredMan + OData connections + dual POST /gitcust.</summary>
public static class NewCustomerGitcustFlow
{
    public delegate string? AskLine(string prompt);
    public delegate void LogLine(string message);

    public sealed class RunContext
    {
        public required string PasswordsDir { get; init; }
        public required string Owner { get; init; }
        public required string Topic { get; init; }
        public required AskLine Ask { get; init; }
        public required LogLine Info { get; init; }
        public required LogLine Err { get; init; }
        public GitcustClient? Gitcust { get; init; }
        public Func<string, string, Task<string>>? FetchODataCustomerJson { get; init; }
        public string? LastMedatechUsername { get; init; }
        public bool HasStoredMedatechPassword { get; init; }
        public string? PlanMcpUrl { get; init; }
        public bool PlanMode { get; init; }
        public IList<GitcustPostRecord>? PostAudit { get; init; }
    }

    public static int Run(RunContext ctx)
    {
        string planMcp = ctx.PlanMcpUrl ?? PlanSkillDagMcp.ResolvePlanMcpFromEnvironment();
        bool planMode = ctx.PlanMode || PlanSkillDagMcp.PlanModeFromEnvironment();
        if (!PlanSkillDagMcp.UsesPlanMcp(planMcp, planMode))
        {
            ctx.Err("New customer requires Plan mode with Plan skill DAG MCP at " + PlanSkillDagMcp.PlanMcpUrl);
            return 2;
        }
        ctx.Info("Plan MCP: " + planMcp + " skill: " + PlanSkillDagMcp.CustomerSetupSkillId);

        string? custNo = ctx.Ask("Medatech customer number: ")?.Trim();
        if (string.IsNullOrWhiteSpace(custNo))
        {
            ctx.Err("Medatech customer number is required.");
            return 2;
        }

        string? usernamePrompt = MedatechCredManSession.FormatUsernamePrompt(ctx.LastMedatechUsername);
        string? typedUser = ctx.Ask(usernamePrompt)?.Trim();
        bool emptyEnter = string.IsNullOrEmpty(typedUser);
        var cred = MedatechCredManSession.Resolve(ctx.LastMedatechUsername, typedUser, emptyEnter, ctx.HasStoredMedatechPassword);
        if (!cred.Ok || string.IsNullOrWhiteSpace(cred.Username))
        {
            ctx.Err(cred.Detail ?? "CredMan username unresolved.");
            return 2;
        }
        if (cred.ShouldPromptPassword)
        {
            string? _ = ctx.Ask(MedatechCredManSession.PasswordPromptMasked());
            ctx.Info("Medatech password collected (not logged).");
        }
        else
            ctx.Info("Medatech CredMan: reusing stored password.");

        string? odataBase = ODataAllowlist.ResolveBase();
        if (odataBase is null)
        {
            ctx.Err("OData base is missing or not allowlisted.");
            return 2;
        }

        string company = ctx.Ask("OData company code (Priority company): ")?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(company))
        {
            ctx.Err("Company code required for OData CUSTOMERS lookup.");
            return 2;
        }

        string customerKey = ctx.Ask("Customer repo key (lowercase, e.g. rees): ")?.Trim().ToLowerInvariant() ?? "";
        if (string.IsNullOrWhiteSpace(customerKey))
        {
            ctx.Err("Customer key required.");
            return 2;
        }

        JsonElement payload;
        try
        {
            string json = ctx.FetchODataCustomerJson is not null
                ? ctx.FetchODataCustomerJson(odataBase, custNo).GetAwaiter().GetResult()
                : """{"CUSTNAME":"demo","ZEMG_CUSTCONECT_SUBFORM":[]}""";
            using var doc = JsonDocument.Parse(json);
            payload = doc.RootElement.Clone();
        }
        catch (Exception ex)
        {
            ctx.Err("OData customer fetch failed: " + ex.Message);
            return 2;
        }

        var writer = new CustomerConnectionsPasswordFile(ctx.PasswordsDir, customerKey);
        CustomerConnectionsWriteResult writeResult = writer.WriteFromOData(payload);
        ctx.Info(writeResult.redactedLog);

        writer.AppendSessionRoute("airc", new Dictionary<string, string> { ["console"] = customerKey + "_console" });

        string repoFull = ctx.Owner + "/" + customerKey;
        ctx.Info("customer-setup (Plan): create repo " + repoFull + " with topic " + ctx.Topic);

        var client = ctx.Gitcust ?? new GitcustClient(log: msg => ctx.Info(msg));
        string firstFolder = ctx.Ask("First subfolder (SC/SO id, e.g. SC263999): ")?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(firstFolder))
        {
            ctx.Err("First subfolder required for dual POST.");
            return 2;
        }
        string folderTitle = ctx.Ask("Subfolder title (free text): ")?.Trim() ?? firstFolder;
        string taskKind = firstFolder.StartsWith("SO", StringComparison.OrdinalIgnoreCase) ? "so" : "sc";

        async Task PostBothAsync()
        {
            using var postCustomer = GitcustContracts.BuildPostCustomer(customerKey, repoFull, custNo);
            var r1 = await client.PostAsync(postCustomer).ConfigureAwait(false);
            r1.EnsureSuccessStatusCode();
            ctx.PostAudit?.Add(new GitcustPostRecord("customer", GitcustRedaction.RedactPostBodyForLog(postCustomer.RootElement.GetRawText()), 1));

            using var postTask = GitcustContracts.BuildPostFirstSubfolder(customerKey, taskKind, firstFolder, folderTitle);
            var r2 = await client.PostAsync(postTask).ConfigureAwait(false);
            r2.EnsureSuccessStatusCode();
            ctx.PostAudit?.Add(new GitcustPostRecord("subfolder", GitcustRedaction.RedactPostBodyForLog(postTask.RootElement.GetRawText()), 2));
        }

        try
        {
            PostBothAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            ctx.Err("gitcust POST failed: " + ex.Message);
            return 2;
        }

        ctx.Info("gitcust dual POST complete (customer then " + firstFolder + "). Run the launcher again when the repo exists.");
        return 0;
    }
}