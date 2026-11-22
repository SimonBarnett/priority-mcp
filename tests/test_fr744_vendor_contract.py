"""PRI-08: bob-ear vendor contract names URL and version for BOB-50 and BOB-55."""
from pathlib import Path

DOC = (Path(__file__).resolve().parents[1] / "docs" / "vendor-contract-bob-ear.md").read_text(encoding="utf-8")


def test_fr744_vendor_contract_path():
    assert "Contract version: 1" in DOC
    assert "https://github.com/SimonBarnett/priority-mcp/blob/main/docs/vendor-contract-bob-ear.md" in DOC
    assert "BOB-50" in DOC
    assert "BOB-55" in DOC
    assert "launcher-v2.0.0" in DOC
    assert "PriorityAgent.exe" in DOC
    assert "embed/bob-ear/" in DOC
