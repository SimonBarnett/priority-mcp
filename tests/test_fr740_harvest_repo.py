"""PRI-04: harvest cites SimonBarnett/priority-mcp via bob intake."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
AGENTS = (ROOT / "AGENTS.md").read_text(encoding="utf-8")
RULES = (ROOT / "launcher" / "agent-rules.md").read_text(encoding="utf-8")
SKILL = (ROOT / ".grok" / "skills" / "priority-harvest-intake" / "SKILL.md").read_text(encoding="utf-8")


def test_fr740_intake_repo_is_priority_mcp():
    for text in (AGENTS, RULES, SKILL):
        assert "https://bob.ntsa.uk/bob/v1/intake" in text
        assert "-Repo SimonBarnett/priority-mcp" in text
        assert "SimonBarnett/a-search" in text
        assert "default -Repo SimonBarnett/bobiverse" not in text
        assert "default -Repo SimonBarnett/a-search" not in text
