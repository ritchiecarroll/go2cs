# DATA — M:N D2/T6: the linux roster against a twinned standard library (hosted, linux-x64)

**Lane R, 2026-11-25.** COORD GO 20261125T080536Z, re-rule 095355Z (option (i)), GO wave 0 102335Z. Wave 0 ran at
`ef7d136de7`, the same cut before the proof fix. Every other run is at `7d8e14c094`, which is master `08e113cb0b`
(train 11) plus two files on `claude/r-t6-linux`: `.github/workflows/os-matrix.yml` (the `;twins=` sweep-shard option
and its proof) and `src/go2cs/internal/repoguard/stdlibTwinsDefaultOff_test.go`. That instrument stays on its branch,
and this record lands alone off master `1c556c35a5` (COORD 20261125T155057Z). The deal is the 1.26.11.1 bank's linux
rows, sharded as
`docs/phase4/data/rebank-1.26.11.1/hosted-280a01952c/shards.tsv` (AR-1..AR-17). Those planned walls are the
linux-arm64 deal, and these runs are linux-x64, so a plain arm's wall is the only wall baseline here.

## 1. What was asked, and the reading in one line

D2/T6: sweep the roster's 236 linux rows in default mode (`GO2CS_MN` unset) against a standard library built WITH twins
(`GoStdlibTwins=true`; the test host stays plain). Read each row against the bank and against the plain pair. EQUAL means
the same matching count AND the same disclosed set. A non-EQUAL row is re-read plain, then twinned, at the same tip. A
twinned miss that reproduces while plain holds is a STOP-LINE.

**Reading: the twinned standard library changes no verdict on any of the 236 linux rows.** 221 rows are EQUAL to the bank.
Of the other 15:
- 14 fail the SAME way with twins off at the same tree. They are master drift since the bank (280a01952c to 08e113cb0b,
  1,218 commits).
- 1, internal/runtime/gc/scan, reads differently only because the Go oracle's runner CPU differed. Its C# results are
  identical with and without twins.
Every twinned row whose host built carries twin names. No STOP-LINE.

## 2. The instrument

- `;twins=1` sets `GoStdlibTwins: 'true'` for the sweep step. `;twins=0` leaves it empty and makes the step the pair's
  plain half. With no option a linux shard is untouched. Workflow-side, the option is refused unless the OS is linux.
- The proof reads every `go.*.dll` beside the host (less `go.runtime`/`go.sync`/`go.time`, whose twins are hand-written
  and in every build) plus the test assembly. It counts names of the form `<base>ꞏtwin`, skipping the bases `Lock` and
  `Sleep`: sync.Mutex's hand-written `Lockꞏtwin` is PROMOTED by struct embedding into plain io, os, syscall and reflect
  (`Lockꞏtwin`, `<N>__Lockꞏtwin`), and time's `Sleepꞏtwin` likewise.
- Enforced on request only:
  - `;twins=0`: every row must read 0.
  - `;twins=1`: the SHARD total must be above 0. A row whose whole closure cannot park reads 0 even when twinned, while
    a build the property never reached reads 0 everywhere.
- `READING t6-env` asserts `GO2CS_MN` is empty and prints `GoStdlibTwins`.
- Host model: from the regenerated `*.tests.csproj`, a ProjectReference to the package's own project means `reference`,
  otherwise `recompile`. With no own project, `unknown` (section 5).
- Guard: `TestStdlibTwinsDefaultOff` refuses every `${{ }}` value for `GoStdlibTwins` anywhere, except os-matrix's one
  exact expression. Made to fail twice (it named os-matrix) and restored byte-identical each time.

## 3. Runs (every ref at 7d8e14c094 except wave 0's; each read back remote == local)

| wave | shard(s) | twins | run | rows |
|---|---|---|---|---|
| 0 | AR-17p / AR-17t (at ef7d136de7) | 0 / 1 | 45594957540 / 45594964882 | 17 |
| 1 | AR-2, AR-6, AR-7, AR-8, AR-11 | 1 | 45608077976, 45608082992, 45608088763, 45608095337, 45608101529 | 67 |
| 1 | AR-17p (fixed proof) | 0 | 45608107662 | 17 |
| 2 | AR-1, 3, 4, 5, 9, 10, 12, 13, 14, 15, 16 | 1 | 45620340043 .. 45621461080 | 152 |
| re-read | W1Rp / W1Rt (context, net/url, testing, sync) | 0 / 1 | 45614740131 / 45614745341 | 4 |
| re-read | RTp / RTt (runtime) | 0 / 1 | 45621931464 / 45621939251 | 1 |
| re-read | W2Rp / W2Rt (10 rows, section 6) | 0 / 1 | 45632007896 / 45632015338 | 10 |

The scores are in [`data/mn-d2-t6-linux/`](data/mn-d2-t6-linux/): one TSV per shard or pair, and `score.py`, which wrote
them from each run's `gh run download` and its job log. In a wave without a plain half, the plain columns read ABSENT
and the twinned row is scored against the bank's word.

## 4. Twins proof

- Shard totals: AR-17 96,049; AR-2 15,544; AR-6 114,655; AR-7 84,464; AR-8 106,465; AR-11 92,971; AR-1 5,449;
  AR-3 13,851; AR-4 85,134; AR-5 99,622; AR-9 123,597; AR-10 84,865; AR-12 89,557; AR-13 93,803; AR-14 99,871;
  AR-15 94,280; AR-16 101,571. In all: 1,401,748.
- Per row, the maximum is 19,133. **No twinned row whose host built reads 0.**
- NIT 1, the twins=1 rows that read no count, with the reason: context, net/url and sort. Each one's test host does not
  compile, the same error with twins off (section 6), so no proof line was written.
- Twin names in the test assembly itself (twinned rows):
  go/doc 8, math/big 140, sync 2, runtime 12, net/http 26, go/types 4, net/rpc/jsonrpc 4, time 4, reflect 30,
  image/draw 8, io 10, net 63, net/http/cgi 6.
  The host is plain by option (i), so these are names the test sources themselves declare: the hand-owned or
  generator-visible shapes the plain build also writes. Not read further.
- Plain halves: AR-17p read 0 on every row, twice. The first time was on the unfixed proof (wave 0's red: 8-18 per row,
  all promoted `Lockꞏtwin`/`Sleepꞏtwin`). The second was on the fixed proof, green. Every re-read's plain arm read 0.

## 5. Host model

- 233 of 242 test hosts reference their own package. Of the 9 that recompile it, 6 are in the linux deal: text/tabwriter
  in wave 1 and 5 in wave 2. Readings:
  - text/tabwriter, net/netip and crypto/sha3 read `recompile`;
  - embed/internal/embedtest, crypto/internal/fips140test and internal/coverage/test read `unknown`.
- They read `unknown` because they are test-only packages with no project of their own, so the readout finds no
  ProjectReference target. Such a host builds its package by construction.
- Every other row whose host built reads `reference`. Wave 0's 17 rows predate the readout and are reference by their
  committed hosts.

## 6. Non-EQUAL rows: attribution

Each was re-read plain and twinned at 7d8e14c094. Twinned EQUAL to plain on every row means the property is clean, and
the row is master drift, attributed to a finding.

| row | twinned reading | plain re-read | twinned re-read | attribution |
|---|---|---|---|---|
| context | FAIL: host CS1503 at `goǃ(pcancelʗ1, cause)` | same | same | master drift; finding 1 (P2) |
| net/url | FAIL: host CS0426 `typeof(encoding.json_package)` | same | same | master drift; finding 2 (i9) |
| sync | FAIL: TestPoolGC Go=pass C#=fail | same, matched 47 | same, matched 47 | master drift; finding 3 (P1) |
| testing | DISC 83 (2 disclosed vs 3; TestTBHelper passes) | same | same | master drift; finding 4 (P1) |
| runtime | FAIL: host exit 2, `panic: finalizer did not start` (TestStackGrowth) | same; 227 divergences | same; the same 227 | master drift; finding 3's family |
| sort | FAIL: host CS0051, `variadic<sort_test_package.lessFunc>` less accessible than `OrderedBy` | same | same | master drift (train 11's wall E); on train 13 with P2 |
| weak | FAIL: TestPointer | same, matched 5 | same, matched 5 | master drift; finalizer/GC family (unrooted) |
| runtime/pprof | FAIL: TestGoroutineProfileConcurrency/finalizer_present | same, matched 146 | same, matched 146 | master drift; finalizer/GC family (unrooted) |
| crypto/internal/fips140cache | FAIL: TestCache hits the 15m per-test deadline | same, matched 0 | same, matched 0 | master drift; finalizer/GC family (unrooted) |
| unique | FAIL: TestMakeAllocs, TestHandle | same, matched 16 | same, matched 16 | master drift; finalizer/GC family (unrooted) |
| reflect | FAIL: TestConvert | same, matched 444 | same, matched 444 | master drift (unrooted) |
| log/slog | FAIL: TestCallDepth | same, matched 227 | same, matched 227 | master drift (unrooted); it fails plain too, so not twins' frames |
| syscall | DISC 50 (9 disclosed vs 17) | same | same | master drift: 8 disclosures no longer fire |
| internal/zstd | DISC 535 (1 disclosed vs 0) | same | same | master drift: a new disclosure fires |
| internal/runtime/gc/scan | FAIL (Go 9,261 rows vs C# 3,095) | **PASS 3095** | FAIL, as in wave 2 | **runner CPU, finding 5**: not the property, not master drift |

- runtime: plain 4,215 s, twinned 4,521 s on re-read and 4,675 s in wave 1.
- runtime: wave 1's read differs from the re-read pair by one divergence row, in an aborted run.
- gc/scan, the one row where plain and twinned read differently:
  - The C# results are identical in all three reads: 3,095 rows, the same verdicts and the same disclosed set.
  - The Go oracle differs. On the twinned runs' runners Go ran TestExpandAVX512, TestScanSpanPackedAVX512 and
    TestFilterNilAVX512, 6,166 rows, every one an AVX512 family. On the plain run's runner it logged
    `expand_amd64_test.go:16: no AVX512` and skipped.
  - C# logs "no AVX512" on every runner.
  - The guard is `scan.CanAVX512()`. Hosted linux runners vary in CPU, so the row passes only on a runner without
    AVX-512.

## 6a. Findings this reading produced (plain, master 08e113cb0b, outside T6)

Routing is COORD's as of 20261125T155057Z. The unrouted-at-reading items each have a BOARD row (2026-11-25, R).

1. context: `go f(x)` on a named func-type value binds no `goǃ` rung since every defined func type is its own delegate
   (d5796d7f4d). P2, reported cured.
2. net/url: `typeof(encoding.json_package)` inside `url_package` binds net/url's own `type encoding` (CS0426). i9,
   reported cured.
3. sync TestPoolGC fails in C#. P1/P2, the optimized-host list.
   - runtime (TestStackGrowth's finalizer watchdog), weak, runtime/pprof and crypto/internal/fips140cache read as one
     finalizer and GC family. That is a reading, not rooted. P2, for an optimized-host A/B.
   - unique (TestMakeAllocs, TestHandle) goes to P6.
4. testing TestTBHelper now passes, so its disclosure is orphaned. Disclosure counts go to P1: this one, syscall's 8 that
   no longer fire, and internal/zstd's 1 new one.
5. internal/runtime/gc/scan: the converted `scan.CanAVX512()` reads false on hardware that has AVX-512, so the row fails
   on any AVX-512 runner, whatever the stdlib build. R: root it first, then cure it, so the verdict does not depend on
   the runner.
- Also: sort's CS0051 goes to i9, as a cut. reflect TestConvert and log/slog TestCallDepth go to i9.

## 7. Predictions, scored as worded (each written before its run)

- Wave 0 (in DELTA 2, 20261125T095355Z): plain half 17/17 EQUAL with 0 on every row; twinned 17/17 EQUAL to the bank and
  to plain, shard > 0. Twinned: MET. Plain 17/17 EQUAL: MET. Plain "0 on every row": MISSED. The premise missed the
  promoted hand-written twins, and the proof was fixed (7d8e14c094).
- Wave 1 (20261125T115144Z): every row EQUAL to the bank: MISSED on 5 (4 + runtime), all master drift. Shard totals > 0:
  MET. text/tabwriter recompile, the other 66 reference: MET, net of the 2 compile rows. AR-17p 17/17 PASS, 0 per row,
  green: MET. The likeliest red, AR-2 on wall: half right. AR-2 was the red, but its cause was the watchdog panic, which
  plain reproduces.
- Wave 2 (20261125T131806Z): EQUAL except master drift: MISSED on gc/scan. Its miss is runner CPU, a third cause the
  prediction did not name. The other 9 non-EQUAL rows are master drift, as allowed. Shard totals > 0: MET. Recompile on exactly 5 named
  rows: MISSED as worded (2 recompile, 3 unknown, section 5). Likeliest red, AR-1 internal/poll or AR-3 crypto/tls:
  MISSED; both were PASS.
- runtime re-read: no prediction made. The three outcomes were stated beforehand (20261125T131806Z), and the outcome was
  "plain dies the same way".

## 8. Instrument notes

- `gh run view <id> --log` returned a truncated log for AR-12, with no proof lines. The job log from
  `gh api repos/<o>/<r>/actions/jobs/<job>/logs --allow-escape-sequences` had all of them. Every shard's log was
  re-checked: one proof line per row, and its shard line.
- A local hand build of a committed test host is not an instrument for this reading. The sweep reconverts the test
  sources, and the committed ones did not build by hand (CS1929 on bufio.tests), so the count is read only in the
  hosted sweep.
