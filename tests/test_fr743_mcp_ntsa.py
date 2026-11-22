"""PRI-03: Priority skills load from https://mcp.ntsa.uk/mcp."""
import json
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
URL = "https://mcp.ntsa.uk/mcp"
SUPPORT = (ROOT / "launcher" / "LauncherSupport.cs").read_text(encoding="utf-8")
PROGRAM = (ROOT / "launcher" / "Program.cs").read_text(encoding="utf-8")
DOC = (ROOT / "docs" / "pri-03-mcp-ntsa-evidence.md").read_text(encoding="utf-8")


def test_fr743_config_points_at_hosted_mcp():
    assert f'HostedMcpUrl = "{URL}"' in SUPPORT
    assert "LauncherSupport.HostedMcpUrl" in PROGRAM
    assert "register-hosted" in PROGRAM
    assert URL in DOC
    assert "mcp-priority" in DOC


def test_fr743_initialize_smoke():
    body = json.dumps({
        "jsonrpc": "2.0",
        "id": 1,
        "method": "initialize",
        "params": {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {"name": "pri-03", "version": "0"},
        },
    }).encode()
    req = urllib.request.Request(
        URL,
        data=body,
        headers={"Content-Type": "application/json", "Accept": "application/json, text/event-stream"},
    )
    with urllib.request.urlopen(req, timeout=20) as resp:
        raw = resp.read()
        assert resp.status == 200
    payload = json.loads(raw)
    assert payload["result"]["serverInfo"]["name"] == "mcp-priority"
