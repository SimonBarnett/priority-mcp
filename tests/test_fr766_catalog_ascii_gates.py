"""FR #766: CAT-T59/T60/T61 gated surfaces stay ASCII-safe (PS 5.1)."""
from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

GATED = (
    ROOT / "v2" / "apps" / "mcp-catalog" / "catalog" / "priority-create-table" / "SKILL.md",
    ROOT / "docs" / "skill-sources" / "programming" / "CREATE_TABLE.md",
    ROOT / ".grok" / "skills" / "priority-create-table" / "SKILL.md",
    ROOT / "AGENTS.md",
    ROOT / "tools" / "Sync-PriorityGrokSkills.ps1",
    ROOT / ".grok" / "skills" / "harvest-agent-skills" / "SKILL.md",
)


def _non_ascii_lines(path: Path) -> list[str]:
    hits: list[str] = []
    data = path.read_bytes()
    if data.startswith(b"\xef\xbb\xbf"):
        hits.append("BOM")
    text = data.decode("utf-8-sig")
    for i, line in enumerate(text.splitlines(), 1):
        for ch in line:
            if ord(ch) > 127:
                hits.append(f"L{i}:U+{ord(ch):04X}")
                break
    return hits


def test_fr766_gated_files_exist():
    missing = [str(p.relative_to(ROOT)) for p in GATED if not p.is_file()]
    assert missing == [], missing


def test_fr766_gated_files_ascii_no_bom():
    bad: dict[str, list[str]] = {}
    for path in GATED:
        hits = _non_ascii_lines(path)
        if hits:
            bad[str(path.relative_to(ROOT))] = hits
    assert bad == {}, bad
