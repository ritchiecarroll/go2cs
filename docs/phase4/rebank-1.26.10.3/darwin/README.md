# 1.26.10.3 RE-BANK at 72043c1a66 -- the darwin leg (C1)
<!-- {% raw %} — Jekyll/Liquid guard: this record quotes a `go list -f` template ({{if ...}}) that Liquid would publish as empty text. -->

The darwin reading of the 1.26.10.3 re-bank, both mac RIDs (osx-arm64 on macos-15, osx-x64 on macos-15-intel), at
master `72043c1a6639d460bf98a9cab270dac6ec743fd6`. Plan: `docs/PLAN-rebank-1-26-10-3.md` sections 4 and 4.1, as
RULED and AMENDED. The procedure is `docs/GoCorpusMigration.md`'s darwin leg, as run for 1.26.10.2.

- **Dispatches.** C1-1 ... C1-15, the shard lists derived from `docs/phase4/data/rebank-1.26.10.2/legs.tsv` (os darwin,
  lane C1, seq order: 236 rows). Each is an os-matrix `sweep-shard` dispatch, goos darwin, from one of the refs
  `claude/c1-tip-72043c1a66-1` ... `-5`, whose head is the tip. A run whose headSha is not the tip is not evidence.
- **One TSV per dispatch**, `<dispatch>.tsv`, one line per row per RID, with the 1.26.10.2 column header unchanged and
  three columns appended: `run` (the run id), `wall_s` (the row's wall time on that RID, in seconds) and `cells`
  (`implied` or `read`, below). `annotate_darwin.py` reads by header name, so the appended columns are ignored by it.

## Which cells are read, and from where (COORD's rulings, 2026-11-08: form (b) plus a narrow (a); cells form (3))

This box cannot fetch run artifacts (their blob host is outside its network), so the cells are read from each job's
log. Checked twice before the first commit:
- Run over 1.26.10.2's C1-10 (run 41693997921), this form reproduces the first 14 columns of the
artifact-read `docs/phase4/rebank-1.26.10.2/darwin/C1-10.tsv` exactly, all 38 lines.
- `annotate_darwin.py`, dry run (no `--write`) over this directory's C1-1.tsv alone, against the tip: 20 rows named,
  0 change. With one count planted (`cmp` 4 -> 5 on both RIDs) it reports exactly that row,
  `darwin: 4 -> darwin: 5`, and the tree is untouched. The `cells` column is ignored by it.

| column | source |
|:--|:--|
| `shard`, `row`, `RID` | the dispatch |
| `status` | READ from the log: the sweep's per-row line (PASS -> `validated`, FAIL, CVAC = comparison-validated-at-count, NOT MEASURED) |
| `matched` | READ from the log: the count on that line |
| `verdicts`, `disclosed`, `divergent`, `new` | on a line whose `cells` is `implied` (a PASS against an existing darwin segment): the tip's expectation, `verdicts` = expected + disclosed, `disclosed` = `divergent` = the segment's disclosed count, `new` = 0. The PASS verdict asserts equality to that expectation, so these numbers are IMPLIED by the verdict, not read. On a line whose `cells` is `read`, they are `relay` until COORD fills them from the run's artifacts in a commit on top of this ref |
| `cells` | `implied` as above; `read` where every filled cell comes from the log or an artifact |
| `agree` | derived: `yes` when both RIDs' log lines agree (status and count), else `no` |
| `win_expected`, `win_disclosed` | the roster's windows columns at the tip (`_roster.ps1`, `Get-ValidatedRosterRows`) |
| `darwin_seg` | the roster's darwin segment at the tip for this RID (`Get-RosterRowExpectation -Rid`), `expected/disclosed`, or `-` where the row has none |
| `win_differs` | derived: `yes` when the darwin segment differs from the windows columns |
| `run`, `wall_s` | READ from the log |
| `absent` | (optional; COORD 2026-11-10) the verdicts this RID LACKS against the other RID's reading, ';'-separated, read from the two comparison artifacts; `-` or empty for none. The arch-count form banks a RID-dependent verdict count only when the shorter RID names every missing verdict here (check-roster-format.ps1 Get-ArchCountReading) |

## The fiat constant-time disassembly on osx-arm64 (`../fiat-osx-arm64/`)

The listings sit beside this directory, not in it: `annotate_darwin.py` refuses any subdirectory of the evidence
directory that is not a declared wave.

The plan's amendment: both compiled forms of the hand-owned field multiplies, P-256's `p256Mul` and `p256Square`
(`p256_field_impl.cs`) and P-384's `p384Mul` and `p384Square` (`p384_impl.cs`). The read is the one the win-x64
listings got: no data-dependent branch on a limb value.

- **ReadyToRun** (`r2r-osx-arm64-*.txt`): crossgen2 10.0.14 cross-targeting osx-arm64 (`-O`, the default version
  bubble), over fiat built Release for the darwin flavour at the tip, against the osx-arm64 runtime pack 10.0.14.
  - `p256Mul` and `p384Mul` are identical to the listings read at `dab0675bd` (DATA-p256-field.md and
    DATA-p384-field.md sections 11, as corrected) once relocation numbers are masked. Their conditional branches are
    12 / 18 `cbz` null tests of the byref arguments' wrapper references and one `bge` frame-zeroing loop counter; no
    `csel` or `cset`, no division. The verdict carries: constant time.
  - `p256Square` and `p384Square` are 32-byte tail calls: `mov x2, x1` (the element passed as both operands), then an
    indirect branch through an import cell to the curve's `Mul`. No conditional branch.
- **The JIT** (`jit-osx-arm64-*.txt`): the os-matrix golib-tests `;jit=` hook on the hosted osx-arm64 runner, run
  42477223005 at the tip, the P256/P384 field differential tests, `DOTNET_TieredCompilation=0`, so FullOpts code.
  - `p256Mul` and `p384Mul` match the section-12 listings once embedded immediates (process-specific handle addresses)
  and method ids are masked: the same instructions, the same sizes (6,108 and 13,972 bytes) and the same conditional
  branches by class. Section 12's classification carries: no branch tests a limb value.
  - Neither `Square` was compiled on its own: each is a tail call that its callers inline, so the JIT listed no body
    for it.
  - The tier-1 form (tiering on, with dynamic PGO) was not read. The hook pins `TieredCompilation=0`, and reading
    tier 1 needs a change to the workflow, which is not at the tip.

## Findings, as ruled

- **(s) structural -- the converted darwin flavour reports `runtime.GOARCH = amd64` against an arm64 oracle**
  (COORD, 2026-11-08: named on C1-5, widened to its root on C1-10, one class). Every GOARCH-sensitive test shape
  diverges on osx-arm64: so far ENUMERATING (gcimporter), CURRENT-PLATFORM (buildinfo) and a GATED SKIP
  (net/http/pprof). Since batch 208 the Go oracle there runs at
  GOARCH=arm64, while the converted darwin flavour IS darwin/amd64 (layout L3): the C# side's child `go` tools resolve
  amd64. The shape is Go-absent / C#-pass, which no disclosure admits, so the row cannot bank arch-strict until an
  arm64 flavour exists or a comparator arm for a GOARCH-derived subtest set is ruled (after 1.26.10.3, beside the
  route-2 question).
  - `go/internal/gcimporter` (C1-5, run 42481111673). `TestImportStdLib` builds its list with
    `go list -f "{{if .GoFiles}}{{.ImportPath}}{{end}}" std` (gcimporter_test.go:370). On osx-arm64 the record reads
    "matched 634 of 634 Go / 636 C# verdicts; disclosed 0; undisclosed divergences 2", and the two are
    `TestImportStdLib/runtime/race/internal/amd64v1` and `TestImportStdLib/internal/runtime/startlinetest`
    (Go="" C#="pass"). osx-x64 validates at its segment, 636. At 1.26.10.2 both RIDs read 636 / 636, against an
    oracle that was then amd64.
  - `debug/buildinfo` (C1-3, run 42477307357), the mirror image. `TestReadFile` tests the CURRENT platform,
    `platform{runtime.GOOS, runtime.GOARCH}` (buildinfo_test.go:48). The C# side's `runtime.GOARCH` is the flavour's
    amd64, and the oracle's is arm64. On osx-arm64 the record reads "matched 193 of 211 Go / 211 C# verdicts; disclosed
    0; undisclosed divergences 34":
    - 17 `TestReadFile/darwin_amd64/...` lines with Go absent or skip and C# pass;
    - 17 `TestReadFile/darwin_arm64/...` lines with Go pass and C# absent or skip.
    osx-x64 validates at its segment, 211. At 1.26.10.2 both RIDs read 211 / 211.
  - `net/http/pprof` (C1-10, run 42486625026), a GOARCH-GATED SKIP, the same root. `TestDeltaProfile` reads
    `if strings.HasPrefix(runtime.GOARCH, "arm") { testenv.SkipFlaky(t, 50218) }` (pprof_test.go:158), so the arm64
    oracle skips while the C# side (GOARCH amd64) runs it and passes. The osx-arm64 record: "matched 14 of 15 Go / 15
    C# verdicts; undisclosed divergences 1", `TestDeltaProfile` Go="skip" C#="pass". osx-x64 validates at its
    segment, 15. At 1.26.10.2 both RIDs read 15 / 15. Under (s): the widening was confirmed as one class.
  - BANK, for every row in this class: none banks darwin at 1.26.10.3; each loses its 1.26.10.2 segment, with a note
    naming this class.
- **(c) the converted runtime's goroutine-per-OS-thread model meets darwin's thread cap** (C1-8, run 42485762191).
  `internal/synctest`'s `TestWaitGroupManyBubbles` holds up to 100 x 100 goroutines alive at once. Each is an OS thread
  here, and on both mac RIDs thread creation fails: x64's record names "unhandled OutOfMemoryException ... at
  System.Threading.Thread.StartCore() at go.golib.GoroutineThreadPool.Worker.Start". The binary dies, and
  `TestHappensBefore` and `TestWeak` lose their verdicts. Records: "matched 56 of 59 Go / 59 C#" (arm64) and "/ 58
  C#" (x64). Linux validates 59. The cap's value on the runner was not read. The row stays darwin-unbanked at
  1.26.10.3.
- **`internal/trace`, unbanked as ruled** (route 1, scored by the divergence line).
  - osx-arm64: `TestTraceCPUProfile/Default` and `/AsyncPreemptOff` both read Go=skip C#=pass.
  - osx-x64: `/AsyncPreemptOff` reads Go=skip C#=pass, and `/Default` reads Go=pass C#=skip, the reverse of D-W1's
    shape.
- **`net`, unbanked as expected** (C1-15, run 42486683115).
  - osx-arm64 reads ORACLE ("oracle unstable on this host"): two runs, oracle-side failures only. Final record:
    "matched 550 of 555 Go / 555 C# verdicts; disclosed 4; undisclosed divergences 1". The one is
    `TestLookupNoSuchHost/LookupNS_NXDOMAIN/forced_cgo_resolver` Go=fail C#=pass, and `TestDialerFallbackDelay`'s
    darwin entry fires.
  - osx-x64 FAILS: "matched 539 of 555; disclosed 3; undisclosed divergences 13". That is 12 NXDOMAIN lines,
    Go=fail C#=pass, plus `TestDialerFallbackDelay` case #1, "got 307.0874ms; want <= 295ms", which the entry's
    "; want <= 395ms" signature (case #0) does not absorb. Not re-read, so (u).
  - The DNS read (COORD 2026-11-08; run 42513405068 on `claude/c1-darwin-net-dns` cd0a6413e, whose corpus is
    30ad20e17e's): an opt-in step timed NXDOMAIN for `invalid.invalid` through Go's resolvers, switched the job to two
    public resolvers, and timed again.
    - BEFORE the switch, the image's own resolver answered every MX/NS/TXT/host lookup with NXDOMAIN in 5 to 91 ms on
      both RIDs. So the leg's NXDOMAIN failures are NOT a resolver that cannot answer this name at rest: their cause
      is (u), something under the test's own conditions.
    - AFTER the switch, all 12 MX/NS/TXT x mode lookups answered NXDOMAIN in 8 to 36 ms.
    - net: osx-x64 validated 552 + 3; osx-arm64 FAILED 550 of 555 on one NEW converted-side line,
      `TestUDPZeroByteBuffer` Go=pass C#=fail, "recvfrom: address family not supported by protocol family", a (u)
      on the post-1.26.10.3 list.
    - RULED (b): `net` stays darwin-unbanked at 1.26.10.3, with no second read. The dns option is HELD, unlanded,
      until the NXDOMAIN cause is rooted.
- **The prediction, scored as it goes.** Predicted: the 229 segmented rows read PASS at their segment on both RIDs.
  MISS on `go/internal/gcimporter`, `debug/buildinfo` and `net/http/pprof` (above). Predicted `internal/synctest` validated 59 on both
  RIDs: MISS, it fails on both (above). `internal/trace`: unbanked as predicted, with the specifics missed (above).
  Predicted `runtime` and `runtime/pprof` validated on both RIDs: HIT, both. Predicted `syscall` failing exactly
  `TestGetdirentries/n=10`, `/n=1000` and the parent on both RIDs: HIT (unbanked, as ruled). Predicted `testing`
  82 + 3 on both RIDs: it banks (HIT), but at 80 + 5 on both RIDs (the split MISSED; the total, 85, matches).
  Predicted `net` unbanked (HIT), with x64 validating at 552 + 3 (MISS: x64 fails on the resolver and on a second
  timing case). The predicted total moves from 233 to 229,
  three BELOW the plan's 232 to 234.

## The tally (C1, at DONE)

- 15 dispatches, all at the tip, 236 rows, 472 lines, every row named once. Row-wall totals: osx-arm64 21,179 s,
  osx-x64 36,897 s.
- **229 of 236 rows read banked under the arch-strict rule** (both RIDs validated, equal matched and disclosed-divergent,
  or crypto/subtle's arch-strict form):
  - 226 at their existing darwin segment;
  - 3 NEW segments: `runtime` 11062 + 58, `runtime/pprof` 146 (its disclosed half is COORD's relay) and `testing`
    80 + 5.
- **7 do not bank:**
  - (s), GOARCH: `go/internal/gcimporter`, `debug/buildinfo` and `net/http/pprof`, each losing its 1.26.10.2 segment;
  - (c), the thread cap: `internal/synctest`;
  - as ruled: `internal/trace`, `syscall` and `net`.
- **Predicted 233; read 229, three below the plan's 232 to 234.** Two causes:
  - (s) explains three of the four misses;
  - (c) explains `internal/synctest`.
  - Every other row read as predicted, and no count moved for any row that banks at its old segment.
- `annotate_darwin.py` can run over this directory once the relay fills `runtime/pprof`'s `relay` cells: it reads
  `divergent` and `new` as integers for every validated row.

## The SIGINT census re-read at 30ad20e17e (`reread/`, COORD 2026-11-08, after batch 257)

Ten rows on both RIDs at master `30ad20e17e8c823b80855ba74bcebc2faa01f289`, from `claude/c1-tip-30ad20e17e-1` and `-2`,
in `reread/`. That is a wave the guard declares, so `annotate_darwin.py` takes these readings as the rows' latest. The
re-dispatch stem rule lets a re-read named `SIG-1R` supersede `SIG-1`.
- SIG-1 (run 42497815515): `runtime`.
  - osx-arm64 validated at 11062 + 58, as at 72043c1a66.
  - osx-x64 FAILS, with one undisclosed line: `TestPingPongHog` Go="fail" C#="pass", the Go ORACLE failing a
    scheduler-fairness timing test. Record: "matched 11061 of 11120 Go / 11120 C# verdicts; disclosed 64;
    undisclosed divergences 1".
  - The sweep did not re-run it as an oracle flake, because a "fatal error: " text appears outside any named test's
    verdict.
  - One DECLARED re-read followed, as SIG-1R (run 42504958622, same tip), with the rule fixed before it read: validate on
    both RIDs and `runtime` banks from SIG-1R, or the oracle fails again and it is (h) with `runtime` unbanked. It
    VALIDATES on both RIDs at 11062 + 58 ("matched 11062 of 11120 Go / 11120 C# verdicts; disclosed 64; undisclosed
    divergences 0"), the Go side passing `TestPingPongHog`. So `runtime` banks from SIG-1R, and its stem supersedes
    SIG-1's in the wave.
- SIG-2 (run 42497834152), as at 72043c1a66 on both RIDs:
  - database/sql, internal/fuzz, net/http, os, os/exec, os/signal and go/internal/gccgoimporter at their segments;
  - runtime/pprof validated at 146;
  - syscall FAIL on exactly the `TestGetdirentries` residue.

## The net re-read with the dns option at e45fd4feb5 (`reread/NET-DNS.tsv`, COORD 2026-11-09, after batch 271)

One row, net, on both RIDs at master `e45fd4feb57e05707f2a82ed6e54c0bb99d77339`, from os-matrix sweep-shard run
42617546338. The filter was `net;dns=<Cloudflare's and Google's public resolvers>`: the dns option replaces the hosted
image's resolver on every enabled network service and flushes the caches before the sweep. It is the post-.3 bank
amendment COORD ruled "bank as read".

The re-read closed both .3 blockers:
- osx-arm64's `TestUDPZeroByteBuffer`: the converted recvfrom source decode, batch 271.
- The NXDOMAIN keys: the image's resolver can answer `invalid.invalid` A/AAAA/MX/NS/TXT with SERVFAIL after about
  30 s (DNS capture run 42601126242; BOARD 2026-11-09, C1).

Both RIDs validate with no `TestLookupNoSuchHost` line:
- osx-x64: "matched 551 of 555 Go / 555 C# verdicts; disclosed 4; undisclosed divergences 0". The fourth is
  `TestDialerFallbackDelay`'s darwin timing entry.
- osx-arm64: "matched 552 of 555 Go / 555 C# verdicts; disclosed 3; undisclosed divergences 0".

That entry fired on osx-arm64 at .3 (run 42513405068) and on osx-x64 here: a disclosed timing class on either RID.
The counts meet the arch-strict rule (equal verdicts, nothing undisclosed, 551 + 4 = 552 + 3), so the guard derives
`darwin: 551 + 4 (osx-x64; osx-arm64: 552 + 3)`. `disclosed` and `divergent` are the summary's disclosed count (net
names no host-fatal test) and `new` is its undisclosed count; every cell is read from the job log.

## The syscall re-read with the reinterpret seam at d86dfc19ba (`reread/SIG-2R.tsv`, COORD 2026-11-09, ruled BANK)

One row, syscall, on both RIDs at `claude/c1-reinterpret-seam` `d86dfc19ba880545687baec40cbe997ed665d5cb`, from
os-matrix sweep-shard run 42664200286 (filter `syscall`). The ref is the seam's own head, not master: the reading needs
the reinterpret seam (docs/phase4/DESIGN-reinterpret-seam.md), ACCEPTED for batch 279. The segment is only true of a
tree that carries the seam, so this record lands with it or after it.

At .3 (C1-12, then SIG-2) syscall failed on both RIDs on exactly the `TestGetdirentries` residue: 29 matched, 1
disclosed, 3 undisclosed (the parent and `/n=10`, `/n=1000`). The test decodes each record with
`copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data)`. `syscall.Dirent` holds `Name [1024]int8`,
a managed `array<sbyte>`, so the view was an order token and the copy panicked on arm 2a. The seam emits it as
`copy(@unsafe.GoImage(Ꮡdirent).slice(), data)`, which copies through the struct's Go-layout image.

Both RIDs validate:
- osx-arm64 (278 s): "matched 32 of 33 Go / 33 C# verdicts; disclosed 1; undisclosed divergences 0".
- osx-x64 (500 s): the same line.

The sweep reports CVAC (no darwin expectation; windows column 66), and the summary's status is `validated`. A matched
verdict is Go's verdict, and Go passes the three `TestGetdirentries` keys, so the three lines that moved from
undisclosed to matched are passes on the C# side. The manifest's one darwin-scoped entry is `TestExecPtrace`
(host-limit). The counts meet the arch-strict rule (both RIDs validated at equal counts, nothing undisclosed), so the
guard derives `darwin: 32 + 1` with no note. `disclosed` and `divergent` are the summary's disclosed count, and `new`
is its undisclosed count. The stem `SIG-2R` extends `SIG-2`, so for this row it supersedes SIG-2 in the wave. Every
cell is read from the job log.

## The batch 279 re-read at 9b8af3bd30 (`reread/SIG-1R2.tsv`, `SIG-2R2.tsv`, `GOARCH-279.tsv`, COORD 2026-11-10)

Five rows on both RIDs at master `9b8af3bd3017e52c1e1f161dd9fbf0dc2a58ebb9`, from ONE os-matrix sweep-shard, run
42740799606, filter `runtime,debug/buildinfo,net/http/pprof,go/internal/gcimporter,syscall`. Batch 279 landed:
- G's (F) and GOARCH seats (runtime.GOARCH reads the process architecture);
- the reinterpret seam.
This is the first read with every cell from the run's ARTIFACTS: each row's `go2cs_test_comparison.json`, downloaded
with `gh run download`, which this box can now reach. The job log agrees on every count.

The file split follows the re-dispatch stem rule. In the reread wave a row's latest file must extend its earlier stem:
`SIG-1R2` extends `SIG-1R` for runtime, and `SIG-2R2` extends `SIG-2R` for syscall. `SIG-2R` is the 2026-11-09 read at
the seam's head, above, which batch 280 landed. The other three rows have no earlier reread file, so they sit in
`GOARCH-279`.

| row | osx-x64 | osx-arm64 | banks |
|:--|:--|:--|:--|
| runtime | 11062 + 58 (PASS) | 11063 + 57 (DISC), 0 undisclosed | `11062 + 58 (osx-x64; osx-arm64: 11063 + 57)`, arch-strict |
| debug/buildinfo | 211 (PASS) | 211 (PASS) | `211` |
| net/http/pprof | 15 (PASS) | 15 (PASS) | `15` |
| syscall | 32 + 1 (CVAC) | 32 + 1 (CVAC) | `32 + 1` |
| go/internal/gcimporter | 636 of 636 (CVAC) | 634 of 634 (CVAC) | none yet (held for a ruling) |

- runtime: G's (F) makes TestFunctionAlignmentTraceback MATCH on osx-arm64, so its darwin disclosure fires on osx-x64
  only (64 disclosures against 63). The entry stays, and its osx-arm64 non-firing is the expected orphan. The job log
  prints no orphan line; the DISC row ("disclosed 57 vs the darwin expectation 58") is where it shows. Arch-strict:
  equal verdicts (11120), 0 new, 11063 + 57 = 11062 + 58.
- debug/buildinfo: the count is equal, while the subtest NAMES differ by RID (TestReadFile/darwin_amd64/... against
  .../darwin_arm64/...), since GOARCH now reads the process architecture.
- go/internal/gcimporter: osx-arm64 lacks exactly two verdicts, TestImportStdLib/internal/runtime/startlinetest and
  TestImportStdLib/runtime/race/internal/amd64v1, the two amd64-only std packages. No verdict differs. The guard's
  arch-strict form needs EQUAL verdicts, so a RID-dependent verdict COUNT with nothing divergent has no form. The row
  carries no segment, which the guard accepts, until COORD rules on one.
  - AMENDED 2026-11-10 (COORD ruling: the arch-count form): GOARCH-279.tsv gains the `absent` column, and osx-arm64's
    gcimporter line names those two verdicts in it. The guard and annotate_darwin.py now bank the shape, refusing any
    count difference the column does not wholly name, and gcimporter takes `636 (osx-x64; osx-arm64: 634)`.

`disclosed` is the comparison's disclosure-list length and `divergent` the non-matching verdicts. `new` is the summary's
undisclosed count. `win_differs` compares the reading with the windows columns, since these rows carried no darwin
segment.

<!-- {% endraw %} -->
