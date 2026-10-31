# End-regen kit (R, lane branch `claude/r-endregen-kit`, not master)

This kit does the measuring half of the end regen: `docs/PLAN-parity-train.md` §3.4 steps 1 and 2, and
`docs/phase4/DESIGN-ladder-identity.md` Steps C and D. It emits the standard library at a tip on three targets
and measures that emission against the committed corpus. It never touches a repository working tree and never
applies anything.

```bash
source <env with GOROOT spelled as `go env GOROOT` prints it>; export GOTOOLCHAIN=local
bash endregen-kit/endregen.sh <tip> [/c/go2cs-tmp/endregen]
```

It runs from any checkout that holds `<tip>`, and writes to `<work>/run-<tip10>/`:

| file | what it is |
|:--|:--|
| `report/SUMMARY.txt` | every step's line: preflight, seed counts, converter sha256, emit (rc, wall, written), the NUL check, MEASURE and MARKER GATE per target, and pcensus |
| `report/files-<goos>.tsv` | each file that differs from committed, CR-normalised: kind, change, hunks, +/- |
| `report/diff-<goos>.patch` | the difflib patch, for reading only |
| `report/gitdiff-<goos>.patch` | the git patch pcensus reads (`--no-index --ignore-cr-at-eol`) |
| `report/markers.tsv` | the path-precise marker gate: every committed `[module: GoManualConversion]` file, held or clobbered, per target |
| `report/pcensus-<goos>.txt` | `pcensus classify` over the git patch, all items |
| `report/refusals-<goos>.txt` | the refusals grouped by reason, file kind and line shape, then attributed. #53's accessor form and `.csproj` template are named; every other run is tagged with the mechanism tells it carries, and an UNTAGGED run is a finding candidate |

The steps:

1. **Preflight.** At least 25 GB free (read with `stat -f`, never `df`), no live `go2cs.exe`, `go env GOROOT`
   equal to `$GOROOT`, go1.26.10, `GOTOOLCHAIN=local`.
2. **Seed.** One `git archive <tip>` (`src`, `docs/validation`, `docs/ValidatedTestPackages.md`) is extracted as
   `committed/`, the baseline, which is never converted into, and as one root per target. Each root's `.cs` count
   must equal the tip's tracked `src/core` `.cs` count. That count is read with `ls-tree -z`, because `ls-tree`
   quotes non-ASCII paths.
3. **Converter.** Built from the same archive by the ladder recipe (`go build -trimpath -buildvcs=false`). Its
   sha256/16 must equal the last rung's "after" (Step C).
4. **Emit.** `go2cs -stdlib -comments -platforms <goos>/amd64 -go2cspath <root>/src`, windows then linux then
   darwin, one at a time. Each emit must exit rc 0, write at least 500 `.cs`, and leave no NUL position sentinel.
5. **Measure** (`endregen_measure.py`).
6. **Attribute** (`pcensus classify`, then `endregen_refusals.py`).

The controls run before the first dry run, on a synthetic tree:
- **Positive:** a clobbered hand-own and a NUL file give exit 1, with both named.
- **Negative:** a CR-only difference, and then the plants reverted, give exit 0, and the CR-only file is not
  counted.

A "written" count is a CHANGED count: the converter skips a byte-identical rewrite.
