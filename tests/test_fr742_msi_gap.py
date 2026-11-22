"""PRI-05: launcher-v2.0.0 MSI gap is documented as N/A."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOC = (ROOT / "docs" / "pri-05-msi-gaps-vs-launcher-v2.md").read_text(encoding="utf-8")
PROGRAM = (ROOT / "launcher" / "Program.cs").read_text(encoding="utf-8")


def test_fr742_launcher_v2_msi_gap_is_recorded():
    assert "launcher-v2.0.0" in DOC
    assert "PriorityAgent.exe" in DOC
    assert "PriorityAgent.exe.sha256" in DOC
    assert "no `PriorityAgent.msi`" in DOC
    assert "N/A" in DOC
    assert r"%LOCALAPPDATA%\PriorityAgent\logs" in DOC
    assert 'Path.Combine(localAppData, "PriorityAgent", "logs")' in PROGRAM
