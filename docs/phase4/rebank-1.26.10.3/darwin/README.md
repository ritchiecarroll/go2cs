# 1.26.10.3 RE-BANK at 72043c1a66 -- the darwin leg (C1)

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
- **The prediction, scored as it goes.** Predicted: the 229 segmented rows read PASS at their segment on both RIDs.
  MISS on `go/internal/gcimporter`, `debug/buildinfo` and `net/http/pprof` (above). Predicted `internal/synctest` validated 59 on both
  RIDs: MISS, it fails on both (above). `internal/trace`: unbanked as predicted, with the specifics missed (above).
  Predicted `runtime` and `runtime/pprof` validated on both RIDs: HIT, both. Predicted `syscall` failing exactly
  `TestGetdirentries/n=10`, `/n=1000` and the parent on both RIDs: HIT (unbanked, as ruled). Predicted `testing`
  82 + 3 on both RIDs: it banks (HIT), but at 80 + 5 on both RIDs (the split MISSED; the total, 85, matches). The predicted total moves from 233 to 229,
  three BELOW the plan's 232 to 234.
