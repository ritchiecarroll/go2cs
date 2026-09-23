# H10 reading run -- AllocsPerRun unit notes at Go 1.24.13 (i7)

**Point-in-time evidence record** (i7, 2026-09-23 00:37-02:09). Banks nothing and rules nothing: it is
the reading C1's relabel (`docs/phase4/CENSUS-alloc-label-relabel-go1.24.13.md`, `c8e6ca9034`) and
COORD's RULING OWED entries are decided from (ledger 2026-09-22 16:39).

`readings-go1.24.13.tsv` -- one line per alloc-class manifest entry of the 27 reading-owed rows, plus a
header: 127 lines = 1 + **126 entries**, the census's reading-owed count exactly (116 `alloc-profile` +
10 `alloc-count-semantics`; the manifests at the tree carry the same 126 names; the census's two
`TestParsePrefixAllocs/<IP-prefix subtest>` placeholders are the manifest's two `TestParsePrefixAllocs/`
subtests). **One redaction**: the IPv4-prefix subtest's name is written `TestParsePrefixAllocs/«ipv4»/24`
(the security order's strict IPv4 arm refuses a spelled quad even as Go's own test data); the
manifest carries exactly one IPv4-shaped `TestParsePrefixAllocs` subtest, so the mapping is unique.
Columns:

    row  entry  current_label  census_proposal  go  cs  unit  reading  note_text

- `current_label` is the manifest class at the tree; `census_proposal` is C1's proposal for the entry.
- `go` / `cs` are the comparison record's verdict pair.
- `unit`: `COUNT` or `BYTES` from the host's own note; `none` = the test reached a terminal verdict and
  carries no note (every AllocsPerRun it made returned exact zero, or it made none); `not-run` = no
  terminal verdict in the C# results (or the row was not run).
- `reading` = the per-run figure the host reported, `max(1, N/R)` integer division exactly as
  `AllocsPerRun` computes it, then the note's totals, then the test's OWN failure text after `test:`
  (lines joined with ` | `).
- `note_text` = the host's note verbatim (empty for `none` / `not-run`).

## Tree, mode, instrument

| | |
|:--|:--|
| tree | `claude/version-go1.24.13` = `bb54ff0920` (the batch-8b STAMP), a linked worktree on branch `i7/h10-readings-09230037` |
| converter | `go build` of `src/go2cs` at the tree under the pins below; `go version go2cs.exe` = go1.24.13 |
| mode | `go2cs -tests -test-action all -test-config Release`, `-test-timeout 20m` (no row run here carries a floor) |
| tiering | OFF, asserted per row from the RUN's own records: the host's `go2cs_test_results.json` `environment.tiered=false` and the comparison record's `environment` (`configuration=Release`, `tiered=false`, `oracleGoVersion=go version go1.24.13 windows/amd64`) -- except the one row below |
| wrapper | `src/run-h10-recon.ps1` from `claude/coord-instrument-h10` = `d095fe8108`, blob `f17cc5b437`, run from a per-run copy OUTSIDE the tree, one row per invocation, `-AllowBranch` |
| wrapper siblings | the wrapper reads `src/run-validated-sweep.ps1` (floors, `ConvertTo-GoDuration`) and `src/_roster.ps1` (execution pins) from `-Tree`, so both were overlaid in the worktree with `d095fe8108`'s blobs (`da3913368c`, `ea8fd4cdc6`) for the run and restored after it; all three blobs re-proven with `git hash-object --no-filters` before every row |
| pins | machine-scope GOROOT unset; `<sdk>\go1.24.13\bin` first on PATH; `go version` = go1.24.13 and `go env GOROOT` = `<sdk>\go1.24.13` (backslash spelling), both read from `C:\`; resolved `go` under the pinned root; `GOTOOLCHAIN=local`, `CGO_ENABLED=0`, `GOFLAGS` empty; `DOTNET_ROOT=<dotnet10>` with that root first on PATH (`dotnet --version` 10.0.400); pwsh 7.4.6 (the dotnet-tool apphost) launched from a shell with `DOTNET_ROOT` unset and the dotnet10 root off PATH, pins set inside and read back by OUTPUT every row |
| hygiene | per row: disk >= 25 GB, no `go2cs.exe` running anywhere, tree == overlays only; the row's test artifacts cleared before it; after it, the full results/comparison copied out, the results tail read FIRST (no deadline marker on any row), EVERY dirty path restored (counted by class below), `bin`/`obj` purged (0 left) |

**`log/slog` carries the roster pin `execution: release-tiered`**, and the d095 wrapper honours roster
pins, so its wrapper run was TIERED (host record `tiered=true`). The ruled mode is tiering OFF, so the
row ran a second time with the converter invoked directly on the wrapper's own argv composition minus
the pin (`-tests -test-action all -test-config Release -test-timeout 20m -go2cspath <worktree>\src
<sdk>\go1.24.13\src\log\slog <worktree>\src\core\log\slog`; host record `tiered=false`). **The TSV
carries the TC0 arm.** Both arms: 197 verdicts, 19 disclosed, matched; all 17 entries COUNT with
IDENTICAL counts; only the byte totals differ (e.g. `TestAlloc/Info` 64,480 B TC0 vs 68,640 B tiered).

## Per row

`word` / `verdicts` / `wall_s` are the wrapper's own TSV fields; `disclosed` is the comparison record's
list length; restored = the paths `-test-action all` dirtied, all put back.

| row | word | verdicts | matched | disclosed | wall_s | restored after the row |
|:--|:--|--:|:--|--:|--:|:--|
| *control:* `crypto/internal/fips140test` | PASS | 2260 | yes | 7 | 302 | index 1, `acvp_capabilities.json` 1 |
| `net/netip` | PASS | 211 | yes | 57 | 157 | index 1 |
| `log/slog` (wrapper, tiered by its pin) | PASS | 197 | yes | 19 | 240 | index, page, prod .cs 1 |
| `log/slog` (direct, TC0 -- the TSV's arm) | PASS | 197 | yes | 19 | 235 | index, prod .cs 1 |
| `strconv` | PASS | 55 | yes | 11 | 160 | index, prod .cs 1, package_info 1 |
| `encoding/binary` | PASS | 140 | yes | 6 | 126 | index |
| `bytes` | PASS | 83 | yes | 6 | 142 | index |
| `strings` | PASS | 69 | yes | 4 | 147 | index, test artifact 1 |
| `slices` | PASS | 120 | yes | 3 | 138 | index |
| `database/sql` | PASS | 140 | yes | 2 | 225 | index, test artifact 1 |
| `math/big` | PASS | 230 | yes | 1 | 166 | index, prod .cs 3, test artifacts 2 |
| `sync` | PASS | 46 | yes | 6 | 142 | index |
| `bufio` | PASS | 80 | yes | 1 | 128 | index |
| `context` | PASS | 57 | yes | 1 | 133 | index |
| `crypto/ed25519` | PASS | 9 | yes | 1 | 153 | index |
| `crypto/md5` | PASS | 11 | yes | 1 | 140 | index |
| `crypto/rsa` | PASS | 568 | yes | 1 | 285 | index, test artifacts 2 |
| `crypto/sha1` | PASS | 12 | yes | 1 | 132 | index |
| `crypto/sha256` | PASS | 22 | yes | 1 | 145 | index |
| `crypto/sha512` | PASS | 35 | yes | 1 | 137 | index |
| `io` | PASS | 60 | yes | 1 | 133 | index |
| `log` | PASS | 8 | yes | 1 | 127 | index |
| `log/slog/internal/buffer` | PASS | 1 | yes | 1 | 131 | index |
| `mime` | PASS | 17 | yes | 1 | 125 | index, test artifact 1 |
| `net/http/internal` | PASS | 14 | yes | 1 | 123 | index |
| `os` | PASS | 1105 | yes | 2 | 176 | index, page, README badge, prod .cs 3, package_info 1, test artifacts 2 |
| `testing` | **DIVERGED** | 55 (derived) | no | 15 | 156 | test artifacts 8 |
| `unicode/utf16` | PASS | 8 | yes | 1 | 127 | index |
| `net` | **not run** | -- | -- | -- | -- | -- |

`testing` diverges on three names outside the alloc set, undisclosed: `TestBenchmarkBLoopIterationCorrect`,
`TestBenchmarkBNIterationCorrect`, `TestTempDirInCleanup` (Go pass, C# fail). Its one alloc entry reads normally.

**`net` was not run.** Immediately before it, Go's own `go test -count=1 -timeout 40m -v net` (pins as
above, from a directory outside any module; 02:07:25-02:08:54, rc 1) failed `TestLookupCNAME` (the
tolerated drift) AND `TestLookupNoSuchHost` -- `LookupHost_NXDOMAIN/default_resolver` only, the system
resolver returning a temporary `getaddrinfow` error for `invalid.invalid.` (the go and cgo resolver arms
passed). `TestNSLookupMX/CNAME/NS/TXT` and `TestLookupLocalPTR` PASSED. By the brief's rule any failure
beyond `TestLookupCNAME` skips the row, so net's two entries are `not-run`.

## How the unit was extracted

The host writes it in `src/core/testing/testing.cs` `AllocsPerRun`: at zero bytes it returns 0 with no
note; otherwise it reports the golib counter's COUNT when the counter charged anything and falls back to
BYTES when it charged nothing, and calls `TestExecution.NoteMeasurementUnitOnce`
(`src/core/testing/TestExecution.cs`), which appends the note to the RUNNING TEST's log output **at most
once per test**. The log output rides the test's terminal event in `go2cs_test_results.json`
(`events[].output`), so the extractor reads each entry's events by exact test name and matches the two
forms the host emits:

    go2cs: testing.AllocsPerRun counted N go2cs-runtime object allocations (B bytes) over R run(s) — ... LOWER BOUND on the true object count.
    go2cs: testing.AllocsPerRun measured B allocated BYTES over R run(s) — ... not comparable to a Go malloc count.

(the `...` is the fixed prose between; the regex spans it). It refuses a test carrying two notes (the host never writes two), and reports a note found on a
subtest's PARENT rather than on the entry (none occurred). The Go side contributes its verdict only:
the pipeline keeps no Go test output.

**Control (floor 13), before the 27 rows.** `crypto/internal/fips140test` ran once through the same
wrapper (banked nothing); the extractor reproduced all six of its alloc entries' 1.24.13 readings as
quoted in that row's own committed manifest reasons -- `TestEdwards25519Allocations` 7,500 / 100 runs,
`TestNISTECAllocations/P224` 84,800, `/P256` 161,490, `/P384` 125,680, `/P521` 170,860 (10 runs each),
`TestXAESAllocations` 1,990 / 10 runs -- unit COUNT, counts and run totals exact, 6 of 6. Planted arms
built from the two note literals in `testing.cs` itself: a BYTES note, a COUNT note, a noteless pass, a
skip, an absent test and a parent-only note each classified as named, and a planted two-note output was
REFUSED.

**The once-per-test limit.** The note is the FIRST nonzero `AllocsPerRun` of a test; a later call's
unit is not recorded anywhere. Entries whose failure text reports a figure other than the noted one, or
several failing calls, so that the noted unit covers only the first:
`context` `TestAllocs` (noted 1/run COUNT; the failing call reads 9, unit unnoted), `testing`
`TestAllocsPerRun` (noted 1/run; failing calls read 2), `slices` `TestConcat` (noted **BYTES** 168 B/run
on `Concat([[]])`; the later calls read 2, unit unnoted), `strings` `TestBuilderGrow`, `log/slog`
`TestAlloc/attrs1` and `TestTextHandlerAlloc`, `bytes` `TestGrow`, `math/big` `TestNewIntAllocs`,
`unicode/utf16` `TestAllocationsDecode`. (`bytes` `TestNewBufferShallow` and `TestWriteAppend` print no
figure at all.)

## What it reads

| unit | entries |
|:--|--:|
| COUNT | 115 |
| BYTES | 5 -- `sync` `TestMapClearOneAllocation`, `TestMapRangeNoAllocations`; `slices` `TestConcat` (first call only); `database/sql` `TestGrabConnAllocs`; `log/slog/internal/buffer` `TestAlloc` |
| none | 4 -- `encoding/binary` `TestSizeAllocs/complex64`, `/complex128`, `/binary.Struct` (C# PASSES on windows, exact zero; the three are scoped `[linux, darwin]` since the re-sign); `math/big` `TestMulUnbalanced` (not an `AllocsPerRun` test: it measures `runtime.MemStats.TotalAlloc`, and its failure text reads 10,506,112 bytes) |
| not-run | 2 -- `net` `TestAllocs`, `TestTCPReadWriteAllocs` (row not run) |

Against the census's premise (an `alloc-count-semantics` entry stays only if the note says BYTES; every
other proposal presumes a COUNT):

- **Seven of the ten `alloc-count-semantics` entries read COUNT**: `strings` `TestBuilderAllocs`,
  `TestBuilderGrow`, `TestBuilderGrowSizeclasses`; `slices` `TestGrow`; `context` `TestAllocs` (first
  call only); `io` `TestPipeAllocations`; `testing` `TestAllocsPerRun` (first call only). Three read
  BYTES and keep the premise: `sync` x2, `slices` `TestConcat` (first call only).
- **Two `alloc-profile` entries read BYTES**: `database/sql` `TestGrabConnAllocs` (proposed RULING OWED
  on a deferred-with-floor premise) and `log/slog/internal/buffer` `TestAlloc` (proposed RULING OWED;
  its own reason already said golib charged none of the bytes, and 1.24.13 still says so).
- **Four read `none`** and two `not-run`, listed above.
- `math/big` `TestNewIntAllocs` (RETIRES AT THE LINUX REFRESH): Go's own test FAILS here too
  (`go=fail`), as the census states; C# reads 1/run COUNT.
