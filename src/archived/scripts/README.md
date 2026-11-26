# Archived scripts

Scripts moved out of `src/` because no CI job, other script, test or runbook step calls them. They are kept
for reference and still run from here: each one now finds `src/` two levels up.

| Script | What it did | Last cited by |
|---|---|---|
| `check-warning-entries.sh` | Checks the per-file warning entries the converter writes against the compiler's warnings, both ways | `docs/BuildingGo2cs.md` (now points here) |
| `syscall-keepalive-census.ps1` | Static census of the syscall funnel keep-alive emission | the BOARD and `docs/ConversionStrategies-Reference/pointers.md` (now points here) |
| `check-roster-format.bat` | Windows launcher for `src/check-roster-format.ps1` | nothing |
| `check-symbol-sync.bat` | Windows launcher for `src/check-symbol-sync.ps1` | nothing |
| `set-version.bat` | Windows launcher for `src/set-version.ps1` | nothing |
| `push-nuget.bat` | Windows launcher for `src/push-nuget.ps1` | nothing |
| `migrate-gorelease.bat` | Windows launcher for `src/migrate-gorelease.ps1` | only that script's own skip list |

To bring one back, `git mv` it to `src/` and undo its one-line root change (the `../..` it gained).
