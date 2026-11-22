# PRI-03 skills from mcp.ntsa.uk

The launcher default MCP load path is `https://mcp.ntsa.uk/mcp` (`LauncherSupport.HostedMcpUrl`). When `priority-catalog` is not registered, `EnsureGrokMcp` runs `grok mcp add --transport http priority-catalog` with that URL. `--mcp-local` is the opt-out.

Smoke on 2026-11-22: JSON-RPC `initialize` to `https://mcp.ntsa.uk/mcp` returned HTTP 200. `serverInfo.name` was `mcp-priority` and `serverInfo.version` was `1.1.1`.
