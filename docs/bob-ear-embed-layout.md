# bob-ear embed layout (PRI-06)

Contract path: `embed/bob-ear/`

A bobiverse MSI includes that directory as one unit. Do not point the installer at `v2/apps/mcp-catalog` source.

| Path | Role |
| --- | --- |
| `embed/bob-ear/manifest.json` | Product name, contract version, and the file list |
| `embed/bob-ear/www/index.html` | Static entry the ear hosts |

The manifest `files` array is relative to `embed/bob-ear/`. Every listed file is in the tree.
