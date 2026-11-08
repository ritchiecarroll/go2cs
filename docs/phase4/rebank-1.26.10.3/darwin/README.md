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
