namespace PriorityAgent;

/// <summary>Medatech ERP CredMan UX for new-customer setup (FR #813 acceptance #3).</summary>
public static class MedatechCredManSession
{
  // Enter alone with saved username → bind saved username, no password prompt.
  // Password prompt (masked) only when stored password missing OR operator typed a new username.
  public static MedatechCredManDecision Resolve(
      string? lastUsedUsername,
      string? typedUsername,
      bool enteredEmptyAtUsernamePrompt,
      bool hasStoredPassword)
  {
      string? bound = string.IsNullOrWhiteSpace(typedUsername) ? lastUsedUsername : typedUsername.Trim();
      if (string.IsNullOrWhiteSpace(bound))
          return new MedatechCredManDecision(false, null, true, "Medatech username required.");

      bool usernameWasTyped = !string.IsNullOrWhiteSpace(typedUsername);
      bool skipPassword = enteredEmptyAtUsernamePrompt && !usernameWasTyped && hasStoredPassword;
      bool promptPassword = !skipPassword && ShouldAskPassword(hasStoredPassword, usernameWasTyped);

      return new MedatechCredManDecision(true, bound, promptPassword, promptPassword ? null : "reuse stored password");
  }

    public static bool ShouldAskPassword(bool hasStoredPassword, bool usernameWasTyped)
    {
        if (!hasStoredPassword) return true;
        if (usernameWasTyped) return true;
        return false;
    }

    public static string PasswordPromptMasked() => "Medatech password (* hidden): ";

    public static string FormatUsernamePrompt(string? lastUsed)
    {
        if (!string.IsNullOrWhiteSpace(lastUsed))
            return "Medatech username [" + lastUsed + "]: ";
        return "Medatech username: ";
    }
}

public sealed record MedatechCredManDecision(bool Ok, string? Username, bool ShouldPromptPassword, string? Detail);