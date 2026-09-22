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

---

# AMENDMENT 2026-09-22 — the 46 CLOSED on three output records

COORD supplied the three output records from the graft's `go2cs_test_results.json` (4,537 events;
profile paths scrubbed). All 52 now carry a label. **Two of the three hypotheses this record left
standing were WRONG, and both are corrected below rather than quietly replaced.**

## 1. The 44 — NOT the launch environment. `fips140/check`'s linker symbol.

The child's combined output, via the parent's `t.Logf`:

```
FIPS 140-3 self-test passed: SHA2-256
FIPS 140-3 self-test passed: cSHAKE128
FIPS 140-3 self-test passed: SHA2-512
FIPS 140-3 self-test passed: HMAC-SHA2-256
panic: fips140: no verification checksum found
    crypto/internal/fips140/check/check.go:64   ← .cctor
TestConditionals did not complete successfully
```

**The re-exec worked.** The child launched, ran package inits, and printed Go's own `passed:` lines in
Go's own format — so the four links this record cleared were cleared correctly, and **the one it left
standing (a .NET apphost unable to start under a one-entry environment) is REFUTED**: the child started
fine. Record 2 refutes it a second time, independently — `acvptool` launched the same published exe
with `os.Environ()` plus one variable and reached its protocol.

**CLASS: converter/golib defect.** Site, exactly:

| where | what |
|:--|:--|
| `src/core/crypto/internal/fips140/check/check.cs:35` | `//go:linkname Linkinfo go:fipsinfo` — the target is **synthesized by the Go linker** (`cmd/link/internal/ld/fips.go`) |
| `check.cs:52` | `public static Linkinfoᴛ1 Linkinfo = new();` — a **zero-valued** struct, because no C# build step writes that symbol |
| `check.cs:70-71` | `if (Linkinfo.Magic[0] != 0xff || … || Linkinfo.Sum == zeroSum) throw panic("fips140: no verification checksum found")` — reached whenever `fips140.Enabled` |

The converter class, stated generally: **a `//go:linkname` whose target is produced by the Go LINKER
emits a zero-valued definition, and the failure surfaces far from the site as a runtime panic in a
`.cctor`.** `check.cs:87-94` compounds it — the body walks `Linkinfo.Sects` and HMACs the section
bounds, none of which a managed assembly supplies.

**A DESIGN QUESTION, COORD's to rule, not this lane's.** The remedy is either (a) a hand-owned `check`
that satisfies verification — defensible, since the integrity self-check is a property of the Go BUILD
and the CAST semantics under test are untouched by it, and it unblocks 44 verdicts — or (b) a
structural disposition, if declaring verification satisfied is judged a manufactured no-op under the
"no managed answer exists" rule. Escalated as a reads-like-Go question; this record does not choose.

**The asymmetry is now EVIDENCED, not hypothesised.** The four names that logged before the panic are
exactly the four this record found matched-under-`Failures`: `SHA2-256`, `cSHAKE128`, `SHA2-512`,
`HMAC-SHA2-256`. They are unconditional CASTs, running at package init **before** `check`'s `.cctor`
panics. So under `TestCASTFailures` with `failfipscast=<one of those four>`, `cast.cs:50-54` substitutes
the simulated error and `fatal` fires with `self-test failed: <name>` and a non-zero exit — every
assertion the subtest makes is satisfied, so it **passes on both sides and does not diverge**. The
other 19 are conditional CASTs that only run inside `TestConditionals`, which the panic prevents. The
prior block's suspected mechanism is confirmed; it was right to record it unclaimed until now.

## 2. `TestACVP` — NEITHER pre-derived branch. An empty `go:embed` payload.

Both branches this record pre-derived were wrong: the test got far past `os.Stat` and far past
`FetchModule`. `acvptool` was built and run, fetched both module versions, and **reached our published
test exe as its module wrapper**. What failed:

```
failed to get config from middle: failed to parse config response from wrapper:
    unexpected end of JSON input
```

`acvp_test.go:53-57` runs `processingLoop(bufio.NewReader(os.Stdin), os.Stdout)`; `:256-262`
answers the mandatory `getConfig` command with a single byte string, `[][]byte{capabilitiesJson}`; and
`capabilitiesJson` is filled by **`//go:embed acvp_capabilities.json`** at `:84-85`.

`json.Unmarshal` returns *"unexpected end of JSON input"* on **empty** input. The wrapper framed and
returned a reply — so the protocol loop ran — and its payload was **zero bytes**. That isolates the
defect to the embedded payload: `capabilitiesJson` is empty in the converted emission.

**The wrapper-mode dispatch WORKED**, which narrows this further: `acvp_test.go:45-47` enters
`wrapperMain()` from `TestMain` when `ACVP_WRAPPER=1`, and the converter does emit that wiring for a
package declaring `TestMain` — `src/core/os/exec/go2cs_test_host.cs:80`,
`registry.SetTestMain(exec_test_package.TestMain)`, is the precedent in the tree.

**CLASS: converter/golib defect** — the `//go:embed` of a test-only data file yielding an empty payload
(the resource not emitted, or the data file absent from the publish directory). One verdict. Not
`deferred`, not `structural`, and — as this record already established at `testConversion.go:6620-41` —
not `host-fatal`.

## 3. `TestXAESAllocations` — STRUCTURAL, one pin authored

The meter:

```
go2cs: testing.AllocsPerRun counted 1,990 go2cs-runtime object allocations (157,600 bytes)
over 10 run(s) … an allocation COUNT per run from go2cs's own runtime counter (golib's
allocation sites), the structural mirror of runtime.MemStats.Mallocs … golib sites only,
so a LOWER BOUND.
expected zero allocations, got 199.0
```

Applying the label ladder (owner-delegated ruling 2026-09-05; a want of ZERO and a want of ONE are the
same question):

- **Not the incomparable-unit arm.** The counter SAW the allocations, so the figure is a COUNT of
  objects — Go's own unit — not the byte-derived figure the shim reports when its counter saw none.
  `alloc-count-semantics` does not apply.
- **No floor hazard.** 199.0 against a want of 0 is not a value equal to both want and floor, and the
  raw numbers are in hand (1,990 objects / 10 runs / 157,600 bytes).
- **Same meter + a stated proof → `structural`.** The proof, from this row's OWN meter and body
  (`xaes_test.go:19-37`): the measured closure allocates five local slices per run — key 32, nonce 24,
  plaintext 16, aad 16, ciphertext cap 32 — plus the per-call AES/GCM and XAES-KDF state, none of
  which escapes, so Go's escape analysis keeps every one off the heap while golib necessarily boxes
  them. No named mechanism REMOVES those allocations (golib has no stack allocation for a slice), so
  the `deferred` arm is unavailable; and because the figure is a LOWER BOUND it can only rise, never
  approach the want.

**Pin:** one, authored. `class` `alloc-profile`, signature `expected zero allocations, got ` —
verbatim with the two pre-ruled families, and derived here from this row's own signature, never from
the family.

**One ruling tension, declared rather than buried.** The legacy caveat holds that the want-zero
`alloc-profile` pins in `bytes`/`bufio` predate ruling #1 and stand as LEGACY "to be re-examined, not
as precedent to extend." Against that: the 2026-09-05 ladder explicitly admits `structural` for a
want-zero assert on a stated proof, and the governing family precedent — `TestEdwards25519Allocations`
and `TestNISTECAllocations`, the same signature, the same package, BANKED — is live roster practice.
This record applies the ladder and the family precedent. COORD may overrule; if it does, the verdict
becomes a defect claim against golib's allocation floor, not a pin.

## The arithmetic after this amendment

| disposition | verdicts | pins |
|:--|--:|--:|
| `structural`, pre-ruled (`TestNISTECAllocations/P*` + `TestEdwards25519Allocations`) | 5 | 5 |
| `structural`, pre-ruled (`TestNISTECAllocations` parent, riding the disclosed-parent aggregation) | 1 | 0 |
| `structural`, newly authored (`TestXAESAllocations`) | 1 | 1 |
| **converter/golib defect** (`TestCAST*` 44 + `TestACVP` 1) | 45 | 0 |
| | **52** | **6** |

**The expected pin count moves from 5 to 6, and disclosed verdicts to 7.** `TestXAESAllocations` has
no 1.23.12 pin to inherit — neither source in the disposition table at
`docs/ValidatedTestPackages.md:554-557` is XAES — so it is a NEW authored pin at 1.24.13. The roster
sentence "five pins expected" is therefore superseded for the merged manifest: **six**.

**45 of the 52 are defects, not disclosures** — fixable, and so pinned by nothing. Both trace to a
single site each (`check.cs`'s linker symbol; the `go:embed` payload), and the 44 are gated on COORD's
ruling above.

## What this amendment does NOT claim

Still no compile, no BUILD, no row, no gate, no .NET leg. Every site above is a READ of Go source at
the pinned toolchain, of emission at `origin/claude/version-go1.24.13` (`0adf2e4318`), or of the three
output records COORD supplied. The `check` remedy is escalated, not chosen. The `go:embed` sub-cause
(resource not emitted vs. data file not published) is narrowed to two candidates, not resolved — that
needs the graft worktree's own emission, which is not on any pushed ref.
