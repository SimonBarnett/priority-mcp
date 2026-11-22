"""PRI-02 / MRB #752: FuelCheck must not swallow stopped: no tokens."""
from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROG = ROOT / "launcher" / "Program.cs"
GATE = ROOT / "launcher" / "NoTokenGate.cs"


def test_pri02_fuelcheck_rethrows_stopped_no_tokens():
    text = PROG.read_text(encoding="utf-8")
    fuel = text[text.index("static void FuelCheck()") : text.index("static FuelFetchResult FetchFuelReportOnce()")]
    assert 'throw new Exception("stopped: no tokens")' in fuel
    assert "NoTokenGate.ShowDialog()" in fuel
    assert 'StartsWith("stopped:"' in fuel
    assert "Continue anyway?" in fuel
    catch_idx = fuel.index("catch (Exception ex)")
    rethrow_idx = fuel.index('StartsWith("stopped:"')
    continue_idx = fuel.index("Continue anyway?")
    assert catch_idx < rethrow_idx < continue_idx


def test_pri02_notokengate_apply_contract():
    text = GATE.read_text(encoding="utf-8")
    assert "ShouldQuit" in text
    assert "remaining <= 0" in text
    assert 'Body = "No tokens left on this box' in text