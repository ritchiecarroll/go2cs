# CENSUS — `crypto/internal/fips140test`: the 52 rehearsal-graft divergences, classed

**Point-in-time record, 2026-09-22, lane C1.** A READING. No cut, no gate, no row. Records are
amended with dated blocks, never rewritten, and never executed from.

## Provenance

Read at `claude/coord-h10-rehearsal-record` **`ac26e6a992a4cbe6f4325c021e7318687d0c511c`**:

| artifact | blob |
|:--|:--|
| `docs/phase4/h10-rehearsal/graft/diverged-52.tsv` | `06befa97dc16afdb6ef546641936f56fdfc52542` |
| `docs/phase4/h10-rehearsal/graft/divergence-summary.txt` | `2f416a6091ad8a041d2292cec2688c163afa4b62` |
| `docs/phase4/h10-rehearsal/graft/graft-fips140test.tsv` | `00f7b5e26ce1f29ff3fda8ddc39ca1738edfd882` |

Row line: `DIVERGED`, 2267 verdicts, rc 1, 179 s, tree `0adf2e4318`, wrapper blob `5d07919168`,
`windows/amd64`, `configuration=Release tiered=False`, oracle `go1.24.13`. Go names 2267 = C# names
2267; **disclosed 0**; all 52 pairs are `pass -> fail`; a clean completion, not a deadline kill.

Go source read at the pinned toolchain (`go1.24.13`, `GOTOOLCHAIN` from the proxy) —
`crypto/internal/fips140test/{cast,xaes,acvp}_test.go`. Emission read at
`origin/claude/version-go1.24.13` (`0adf2e4318`); `crypto/internal/fips140test` itself is **not
converted on that branch** (only `go2cs.ico` is present), so the test-side emission exists solely in
the rehearsal worktree. No .NET leg, no BUILD, no row — this box has no .NET SDK.

## The 52, re-derived from the TSV

`52` rows, every one `pass -> fail`, re-counted from the blob rather than taken from the summary:

| family | verdicts | shape |
|:--|--:|:--|
| `TestCASTPasses` | 24 | 1 parent + 23 subtests |
| `TestCASTFailures` | 20 | 1 parent + 19 subtests |
| `TestNISTECAllocations` | 5 | 1 parent + 4 curve subtests |
| `TestEdwards25519Allocations` | 1 | leaf |
| `TestXAESAllocations` | 1 | leaf |
| `TestACVP` | 1 | leaf |

= 52. `TestCAST*` totals 44, matching the summary's family line.

**An asymmetry worth recording:** `allCASTs` holds 23 names (with `"ML-KEM PCT"` listed **twice** —
hence `ML-KEM_PCT` and `ML-KEM_PCT#01`), and both tests loop over all 23. `TestCASTPasses` diverges on
all 23; `TestCASTFailures` diverges on only 19. The four that diverge under `Passes` but **not** under
`Failures` are `HMAC-SHA2-256`, `SHA2-256`, `SHA2-512`, `cSHAKE128`. Under `Failures` those four
**matched** — Go did not pass them either. Consistent with their CASTs being unconditional (run at
package init, so `failfipscast` aborts before `TestConditionals` starts), but the record does not
claim that mechanism: it is unmeasured here, and a non-divergence is outside this classing.

## The table

`pin` names the entry in the merged `fips140test/go2cs_test_disclosures.json`. The manifest is
**hand-owned** (`src/core/.gitignore:15`), READ by the converter and never written by a run, so pins
are **AUTHORED from divergence evidence, never minted** (ledger ruling 2026-09-21 23:52).

| test | verdicts | class | pin |
|:--|--:|:--|:--|
| `TestCASTPasses` + 23 subtests | 24 | **converter/golib defect** — ONE mechanism with `TestCASTFailures`; site narrowed below, confirmation owed | NONE |
| `TestCASTFailures` + 19 subtests | 20 | **converter/golib defect** — same mechanism | NONE |
| `TestNISTECAllocations/P{224,256,384,521}` | 4 | **structural** — pre-ruled; `alloc-profile`, `expected zero allocations, got `; 8,484–17,090 golib boxes where Go's escape analysis stack-allocates every point and fiat field element | 4 pins, re-pinned under the renamed declaration |
| `TestNISTECAllocations` (parent) | 1 | **structural** — rides the disclosed-parent aggregation | NONE NEEDED |
| `TestEdwards25519Allocations` | 1 | **structural** — pre-ruled; same signature; 98 golib boxes per run | 1 pin, re-pinned |
| `TestXAESAllocations` | 1 | **alloc want-zero, label PENDING ITS METER** — `deferred` with a want/reading/plan, or `structural` with a floor proof; the meter decides and is not in hand | NONE yet — to be authored |
| `TestACVP` | 1 | **PENDING one output line** — two pre-derived branches below | NONE yet — to be authored |

**Five pins, six verdicts cleared, and no discrepancy.** `docs/ValidatedTestPackages.md:554-557`
lists sources `crypto/internal/edwards25519` at **1** pin and `crypto/internal/nistec` at **4**, while
the 1.23.12 `fips140test` row counts **5 disclosed** ("the four curve subtests plus their aggregate
parent"). Both are right: `src/go2cs/testConversion.go:3620-3621` records the precedent — *"pins 25
leaf rows … their 2 parents ride the disclosed-parent aggregation, and os/exec banks at 74 matched +
27 disclosed"* (25 + 2 = 27). A parent whose diverged children are all disclosed is **counted
disclosed without an entry of its own**. So the merged manifest carries **5 pins** (1 + 4) and clears
**6 of the 52 verdicts** (1 + 4 leaves + 1 parent). The "4 vs 5" is the pin/disclosed distinction, not
a miscount. `class` and `signature` are expected to survive verbatim: the signature never moved, only
the declaration's name did.

That leaves **46 verdicts unpinned**, of which **44 are one mechanism**.

## The 44: what the reading eliminated, and what stands

Both tests (`cast_test.go:135` and `:160`) do the same thing: `testenv.MustHaveExec(t)`, skip unless
`fips140.Supported()`, then **re-exec the test binary itself** —
`testenv.Command(t, testenv.Executable(t), "-test.run=…TestConditionals…", "-test.v")` with
`cmd.Env = append(cmd.Env, "GODEBUG=fips140=debug")` (Passes) or
`GODEBUG=failfipscast=<name>,fips140=on` (Failures) — and assert on the **subprocess's combined
output**: `"completed successfully"`, `"passed: <name>\n"`, `"self-test failed: <name>"`.

**Both parents fail**, and a parent's first assertion is the subprocess one
(`cast_test.go:145-147`). So the earliest link is already broken and all 44 follow from it. Four
candidate links were checked and found FAITHFUL — none of them is the defect:

1. **The production CAST mechanism.** `fips140/cast.cs:24` reads `godebug.Value("#failfipscast")`;
   `:50-54` and `:85-89` substitute the simulated error and call `fatal("FIPS 140-3 self-test failed: "
   + name + …)`; the `debug` arms print `"FIPS 140-3 self-test passed:"` / `"FIPS 140-3 PCT passed:"`
   with `name` as a second `println` argument, which yields the `passed: <name>\n` the test matches.
2. **`Enabled` / `debug`.** `fips140/fips140.cs:18-25` reads `godebug.Value("#fips140")` and sets both
   (`"on"` → Enabled, `"debug"` → Enabled + debug).
3. **GODEBUG reaching the child at all.** `internal/godebug` is a **hand-owned** implementation that
   reads `Environment.GetEnvironmentVariable("GODEBUG")` live (`godebug.cs:200`) and re-parses when the
   raw text changes. Not a snapshot that could miss the subprocess's setting.
4. **The `-test.run` / `-test.v` flag surface.** `testing/TestFlagBridge.cs:194-200` registers both and
   `testing/TestOptions.cs:185,194` parses them; the corpus already re-execs itself this exact way in
   `testing/{testing,helper,panic,flag}_test.cs`. The spelling is supported and exercised.

**What still stands**, and cannot be settled by reading: the re-exec's **environment and launch**.
`cmd.Env` is nil before the `append`, in Go too, so the child is handed a **one- or two-entry
environment** — `GODEBUG` plus the `SYSTEMROOT` that `os/exec` adds (the special case IS carried:
`os/exec/exec.cs:176`, `:1378-1397`). A Go binary starts fine that way. Whether the converted **.NET
test apphost** does — it may need `PATH`/`DOTNET_ROOT` to locate a runtime — is the open question, and
it would fail uniformly across both arms and both parents, which is exactly the observed shape.

**One output line settles all 44.** Both tests do `t.Logf("%s", out)`, so the subprocess's own combined
output is in the results tail and names which link broke. ASKED of COORD: the `TestCASTPasses`
output line.

## `TestACVP` — both branches pre-derived

`acvp_test.go` (1.24.13) in order: `os.Stat("acvp_test.config.json")` → `t.Fatalf` if absent;
`cryptotest.FetchModule(t, bsslModule, …)` → a **network module fetch**; `testenv.Command(t, goTool,
"build", …"./util/fipstools/acvp/acvptool")` → **builds a Go binary with the Go toolchain**; a second
`FetchModule`; then the tool is run against the module shim.

- Failed at the `os.Stat` → **`deferred`**, with a plan: the harness must place the package's data
  files beside the test exe or set the working directory. `cryptotest/fetchmodule.cs` IS converted on
  the version branch, so the fetch path exists.
- Failed at `FetchModule` or the toolchain build → **`structural`**, on the stated proof that the
  converted corpus has no Go module graph and no Go toolchain, so a test that builds and runs a Go
  program from source cannot be reproduced.

**Not `host-fatal`.** `testConversion.go:6620-6641` hands `hostFatalSkipExpression` verbatim to
**both** sides, so a host-fatal entry produces no verdict on either — it is a mutual skip, deliberately
excluded from `matchTerminalStatuses` so it cannot become a second way to disclose a failure. Go
**passed** `TestACVP` on the i7, so classing it host-fatal would delete a real Go pass. ASKED: the
`TestACVP` output line.

## `TestXAESAllocations` — signature certain, label not

`xaes_test.go:19-37`: `cryptotest.SkipTestAllocations(t)`, then `testing.AllocsPerRun(10, …)` over an
`xaesSeal` + `xaesOpen` round trip, and `allocs > 0` → `t.Errorf("expected zero allocations, got
%0.1f", allocs)`. The signature is therefore **certain from source**: `alloc-profile`, `expected zero
allocations, got ` — the same string as the two pre-ruled families.

The **label is not**. A same-family disclosure is written only from the row's OWN results-file
signature, never from the family, and the alloc label is decided by the **meter**, not by the bound: a
named removable mechanism → `deferred` plus a plan; a stated floor proof → `structural`. The count is
in the results tail. ASKED: the `TestXAESAllocations` output line.

## What this record does NOT claim

No compile, no BUILD, no row, no gate — this box has no .NET SDK and no PowerShell, and every claim
above is a READ of source, emission or the graft TSVs. The 44's class is named as a mechanism with its
site narrowed, not confirmed; `TestACVP` and `TestXAESAllocations` carry no label yet. Three output
lines close all 46.
