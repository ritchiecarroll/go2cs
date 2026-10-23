# DATA: the go1.26.10 deferred-alloc pins, measured at the hop tip

**Point-in-time record, 2026-10-22, lane H.** Measured at hop tip `8c091fc3aa` (batch 19) on
**windows/amd64**. This is the measured basis for the deferred allocation disclosures the go1.26.10
re-bank carries, under the owner's ruling that Go's stack-allocation parity is POST-HOP. It records what
these rows measured on this platform at this ref and nothing more: it carries no corpus-wide figure, no
other platform's reading, and writes no manifest. Every figure below is windows/amd64.

## Method and pin

- **Rows:** `crypto/sha256`, `crypto/internal/fips140test`, `math/big`, `log/slog`, `strings`, `bytes`. Each was
  run in full through `src/run-validated-sweep.ps1 -Filter <row> -Exact -TestTimeout 40m`, one row at a time,
  in a dedicated detached worktree at `8c091fc3aa`. The sweep has no per-test filter, so every figure comes
  from a full-row run.
- **Configuration:** the sweep defaults: `Release`, tiered JIT off, `CGO_ENABLED=0` (the sweep pins it).
- **Toolchain pin**, asserted in two parts before any row ran: bare `go version` reported
  `go version go1.26.10 windows/amd64`, the resolved `go.exe` lived under `GOROOT`, `go env GOROOT` equalled
  `GOROOT`, and `GOTOOLCHAIN=local`. The converted side ran on .NET 10.0.13. The oracle line in every
  comparison file reads `go version go1.26.10 windows/amd64`.
- **Sources of each figure:** a verdict and its disclosure status come from the row's
  `go2cs_test_comparison.json`. The counter line and assertion text come from the row's
  `go2cs_test_results.json`. **C# per run** is the go2cs counter's total divided by its run count. The counter
  note prints once per test, for that test's first `AllocsPerRun` call, so a test with several cases gives
  its per-case figures in its own assertion lines.
- **Go's limit** is read from the go1.26.10 test source at the cited line. Where a target passed or failed
  differently in the go1.24.13 bank, the go1.24.13 source was diffed against go1.26.10 to say which side moved.
- **Raw evidence** is kept on lane H at `C:\go2cs-h\reports\allocs\raw`: the twelve comparison and results
  files, two per row. It is deliberately not committed.

Every go2cs counter line also carries the standard note that the figure is a LOWER BOUND: it counts golib's
allocation sites only, and allocations the C# compiler emits in converted code, or that the BCL makes
internally, are outside it.

## The table

Bytes per run are the counter's byte total divided by its run count. "Counter" quotes the figures of the go2cs
line, `go2cs: testing.AllocsPerRun counted <N> go2cs-runtime object allocations (<B> bytes) over <R> run(s)`.

| row · test | platform | Go's limit (go1.26.10) | counter: N, B, R | C# per run | Go | C# | assertion text, verbatim |
|:--|:--|:--|:--|:--|:--|:--|:--|
| `crypto/sha256` · `TestAllocations` | windows/amd64 | 0 | 540, 37,120 B, 10 | 54 allocs, 3,712 B | pass | fail (disclosed) | `sha256_test.go:370: allocs = 54, want 0` |
| `crypto/sha256` · `TestAllocatonsWithTypeAsserts` | windows/amd64 | 0 | 400, 80,000 B, 100 | 4 allocs, 800 B | pass | fail | `sha256_test.go:485: allocs = 4; want = 0` |
| `crypto/internal/fips140test` · `TestMLDSAAllocations` | windows/amd64 | at most 5 (`expected` = 5, plus 1 in one configuration) | 408,117, 190,760,736 B, 100 | 4,081.17 allocs, 1,907,607 B | pass | fail | `mldsa_test.go:180: expected 5 allocations, got 4081.0` |
| `math/big` · `TestNewIntAllocs` | windows/amd64 | 0 | 100, 20,800 B, 100 | 1 alloc, 208 B | pass | fail | 7 cases, each `int_test.go:1960: x.Add(x, NewInt(<v>)), wanted 0 allocations, got 1.000000`, for v = 0, ±7, ±1073741824, ±1125899906842624 |
| `math/big` · `TestSqrAlloc` | windows/amd64 | under 1 (fails at `n >= 1`) | 40, 4,240 B, 10 | 4 allocs, 424 B | pass | fail | `escape_test.go:50: Mul(len 80 with itself) allocates 4.00 objects` (only length 80 fails) |
| `math/big` · `TestMulUnbalanced` | windows/amd64 | allocated bytes at most 10 times the inputs' size | no counter line (the test measures `runtime.MemStats` bytes, not `AllocsPerRun`) | ratio 14 | pass | fail | `nat_test.go:361: multiplication uses too much memory (5701504 > 14 times the size of inputs)` |
| `log/slog` · `TestAlloc/2_pairs_disabled_inline` | windows/amd64 | 0 | 10, 920 B, 5 | 2 allocs, 184 B | pass | fail | `logger_test.go:358: got 2 allocs, want 0` |
| `log/slog` · `TestJSONAllocs/attrs` | windows/amd64 | 1 | 140, 75,200 B, 5 | 28 allocs, 15,040 B | pass | fail | `json_handler_test.go:164: got 28 allocs, want 1` |
| `log/slog` · `TestJSONAllocs/message` | windows/amd64 | 0 | 60, 44,520 B, 5 | 12 allocs, 8,904 B | pass | fail | `json_handler_test.go:157: got 12 allocs, want 0` |
| `strings` · `TestReplace` | windows/amd64 | at most 1 per case | 30, 2,000 B, 10 | 3 allocs, 200 B (first case) | pass | fail | 13 cases `strings_test.go:1480: Replace(…) allocates 3.00 objects`, and `Replace("☺☻☹", "", "<>", -1) allocates 6.00 objects` |
| `bytes` · `TestReplace` | windows/amd64 | at most 1 per case | 10, 320 B, 10 | 1 alloc, 32 B | pass | **pass** | none; it passes on windows/amd64 and needs no disclosure here |

## Which side moved, where the go1.24.13 bank differs

- **`crypto/sha256` · `TestAllocations`:** the go1.24.13 disclosure states 46 allocations per run. At
  `8c091fc3aa` on windows/amd64 it is **54**, so a disclosure carrying the old figure would be stale.
- **`math/big` · `TestNewIntAllocs`:** the test source is identical at go1.24.13 and go1.26.10. At go1.24.13 Go
  **also** failed it (bank: fail on both sides). At go1.26.10 Go passes, so the Go side moved (escape analysis
  now keeps the `NewInt` value off the heap), and the converted side, at 1 allocation per run on windows/amd64,
  did not.
- **`math/big` · `TestMulUnbalanced`:** passed on both sides at go1.24.13, with the same `ratio > 10` threshold.
  go1.26.10 rewrote multiplication: the new `natmul.go` takes scratch space from a stack via `getStack()`, and
  the test now passes that stack (`nat(nil).mul(stk, x, y)`). The converted go1.26.10 code heap-allocates
  what Go keeps on that stack, for a ratio of 14 on windows/amd64. This is a Go-side stack-scratch change, not
  a regression of the converted code on unchanged source.
- **`log/slog` · `TestAlloc/2_pairs_disabled_inline`:** passed on both sides at go1.24.13, when its assertion was
  `wantAllocs(t, 2, …)`. go1.26.10 tightened it to `wantAllocs(t, 0, …)`. The converted side still makes 2
  allocations per run on windows/amd64, which met the old threshold.
- **`strings` · `TestReplace`:** passed on both sides at go1.24.13, when `TestReplace` had no allocation
  assertion. The `AllocsPerRun` check is new at go1.26.10.
- **`bytes` · `TestReplace`:** carries the same new check and passes it on windows/amd64.
- **New tests at go1.26.10**, with no go1.24.13 row: `TestAllocatonsWithTypeAsserts` (the misspelling is
  Go's own), `TestMLDSAAllocations`, `TestSqrAlloc`, and `TestJSONAllocs` with its two subtests.

## Other divergences in these rows

These are the Go/C# mismatches in the six rows beyond the targets above, all on windows/amd64. A mismatch the
committed manifest already discloses is not listed.

- **`crypto/internal/fips140test` · `TestVersion`:** Go pass, C# **skip**, undisclosed, and not an allocation
  test. The converted host reports `fips_test.go:70: no build info`: `debug.ReadBuildInfo()` returns
  `ok = false` there. The test is new at go1.26.10. The re-bank needs either build info in the converted host or
  a disclosure.
- **`crypto/internal/fips140test` · `TestNISTECAllocations` (parent):** undisclosed in the go1.26.10 comparison.
  The go1.24.13 bank disclosed it as `aggregate`, the roll-up of its disclosed `P224`, `P256`, `P384` and `P521`
  subtests, and the re-bank must carry that aggregate disclosure again.
- **`log/slog` · `TestAlloc` and `TestJSONAllocs` (parents):** undisclosed roll-ups of their failing subtests:
  `TestAlloc/2_pairs_disabled_inline` above, and both `TestJSONAllocs` subtests. Each needs an aggregate
  disclosure beside its subtests' deferred pins.
- **`crypto/sha256`, `math/big`, `strings`:** no mismatch beyond the targets.
- **`bytes`:** no mismatch beyond the six the manifest already discloses; its comparison status reads
  `validated`.

**Amended 2026-10-23 (lane H): the two parent bullets above are corrected by
[`DATA-hop126-rollup-disclosures.md`](DATA-hop126-rollup-disclosures.md), which reads the rows again at batch 24.**

- A parent never takes an entry of its own. The pipeline discloses it as `aggregate` once every failing child
  under it is disclosed, so "the re-bank must carry that aggregate disclosure" and "an aggregate disclosure beside
  its subtests' deferred pins" both name work that does not exist.
- `TestNISTECAllocations` already aggregates at `ddebd3bed1` and owes nothing.
- `TestAlloc` and `TestJSONAllocs` aggregate once their three undisclosed children are disclosed. The successor
  record drafts those three entries, and one for `TestMLDSAAllocations`, whose reading rose from 4,081 to 4,642
  per run between batch 19 and batch 24.

<!-- Provenance. Lane H, 2026-10-22. Worktree detached at 8c091fc3aa; the six rows' walls on windows/amd64 were
     crypto/sha256 249 s, strings 90 s, bytes 76 s, log/slog 272 s, math/big 156 s, crypto/internal/fips140test
     356 s. Each sweep exited 1 because the banked counts are still the go1.24.13 record, which this record does
     not read as a divergence. After the runs the worktree's 60 tracked changes were reverted, its 9 generated
     untracked test files deleted, and the worktree removed. Go thresholds were read from the go1.26.10 sources:
     sha256_test.go:370/:485, mldsa_test.go:149-180, int_test.go:1960, escape_test.go:43-50, nat_test.go:349-361,
     logger_test.go:358, json_handler_test.go:157-165, strings_test.go:1475-1480, bytes_test.go:1794-1796. The
     go1.24.13 comparisons are math/big/nat_test.go:195 (same threshold, no stack argument),
     log/slog/logger_test.go:260-270 (wantAllocs 2), and strings_test.go's TestReplace (no AllocsPerRun). -->
