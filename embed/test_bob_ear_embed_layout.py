"""PRI-06: the bob-ear embed path and its files exist."""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / "docs" / "bob-ear-embed-layout.md"
EMBED = ROOT / "embed" / "bob-ear"


def main() -> int:
    text = DOC.read_text(encoding="utf-8")
    assert "embed/bob-ear/" in text
    manifest = json.loads((EMBED / "manifest.json").read_text(encoding="utf-8"))
    assert manifest["msiInclude"] == "embed/bob-ear/"
    assert manifest["contract"] == "docs/bob-ear-embed-layout.md"
    for rel in manifest["files"]:
        path = EMBED / rel
        assert path.is_file(), path
        assert rel.replace("\\", "/") in text or rel == "manifest.json"
    assert (EMBED / "www" / "index.html").is_file()
    print("ok")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
