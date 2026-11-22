"""PRI-07: consumers use the existing launcher/priority.ico."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ICO = ROOT / "launcher" / "priority.ico"
README = (ROOT / "launcher" / "README.md").read_text(encoding="utf-8")
DOC = (ROOT / "docs" / "pri-07-priority-icon-path.md").read_text(encoding="utf-8")
CSPROJ = (ROOT / "launcher" / "PriorityAgent.csproj").read_text(encoding="utf-8")


def test_fr741_icon_path_for_tray_exe_docs():
    raw = ICO.read_bytes()
    assert raw[:4] == b"\x00\x00\x01\x00"
    assert "launcher/priority.ico" in README
    assert "launcher/priority.ico" in DOC
    assert "Tray" in DOC and "exe" in DOC
    assert "<ApplicationIcon>priority.ico</ApplicationIcon>" in CSPROJ
