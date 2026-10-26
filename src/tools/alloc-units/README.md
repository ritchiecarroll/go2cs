# alloc-units

The allocation-unit extractor that the H10 reading run uses (hop readiness A14; `docs/GoCorpusMigration.md` H10 step
3 (a), §7 lessons 31, 63 and 70). It reads the unit note that the converted test host attaches to a test when
`testing.AllocsPerRun` measures something nonzero, and prints one row per test. It only reads results files and
writes to stdout.

The note says whether the figure the test printed is an allocation COUNT per run (golib's counter charged
something) or BYTES per run (the counter charged nothing). Go's own failure text cannot tell them apart. The host
writes the note at most once per test, for the test's FIRST nonzero `AllocsPerRun`, so a later call in the same
test has no recorded unit anywhere.

Build from this directory with the pinned toolchain (`GOROOT` = the go1.24.13 root, `GOTOOLCHAIN=local`,
`CGO_ENABLED=0`):

```
go build -o alloc-units.exe .
```

Run it over each row's package directory after a `-tests -test-action all` run, before the row's artifacts are
cleared:

```
alloc-units.exe -entries reading-owed.txt <worktree>/src/core/net/netip <worktree>/src/core/strconv ...
```

`-entries` takes one `<package> <test>` pair per line, and `#` starts a comment at the start of a line or after whitespace, so a subtest name's `#NN` suffix (`TestSizeAllocs/[]binary.Struct#01`) is kept. Each entry gets exactly one row.
Without `-entries`, every test that carries a note is reported. An argument can also name a
`go2cs_test_results.json` directly. A sibling `go2cs_test_comparison.json`, when present, supplies the Go verdict.

| column | meaning |
|---|---|
| `package`, `test` | the entry |
| `cs`, `go` | the C# terminal verdict from the results file, and the Go verdict from the comparison record (`-` if absent) |
| `unit` | `COUNT` or `BYTES` from the entry's own note; `none` = a terminal verdict and no note (every `AllocsPerRun` returned exact zero, or the test made none); `parent-only` = no note on the entry, but a parent test has one (`note_on` names it and the figures are the parent's); `capped` = the host dropped log records for the test and no note survived, so the unit is unknown; `not-run` = no terminal verdict, or that package's results were not given |
| `per_run` | the figure `AllocsPerRun` returned: `max(1, figure / runs)` in integer division, where the figure is the count for COUNT and the bytes for BYTES |
| `count`, `bytes`, `runs` | the note's totals (`count` is `-` for BYTES) |
| `note_on` | `self`, or the parent test that carries the note |
| `note` | the host's note, verbatim |

A summary line (`COUNT n, BYTES n, none n, ...`) goes to stderr.

**It refuses rather than guess** (exit 1, naming the file and the test):

- a results file with no events or a schema other than 1;
- a test with two notes or two terminal events;
- a log line that starts like a note but matches neither form the host writes.

The last refusal guards against host drift. If `src/core/testing/testing.cs` changes the note's wording, the
extractor stops instead of reading every entry as `none`. `main_test.go` reads the two note literals from
`testing.cs` itself and fails if they no longer parse.

The host writes figures with `N0`, so they carry the culture's group separator (`2,800`, `2.800`, `2 800`). All of
these parse.

**Proof.**

- `go test ./...` covers the host's own literals under three separators and `per_run` against `AllocsPerRun`'s
  arithmetic.
- It covers a planted arm for every unit and every refusal.
- It reads two real results files the host wrote on linux at master `226afb810a`: `unicode/utf16`
  `TestAllocationsDecode` (COUNT, 10 objects and 2,800 bytes over 10 runs, 1 per run) and
  `log/slog/internal/buffer` `TestAlloc` (BYTES, 880 over 5 runs, 176 per run). Both equal the readings their
  disclosures cite from the i7 reading run at `bb54ff0920`.
- Red-proved on 2026-10-12. Each of 20 sites in `main.go` was regressed alone in a copy, and each made a named test
  fail. Three rewordings of the note in a copy of `testing.cs` each failed `TestTheHostsOwnNoteLiteralsParse`.
