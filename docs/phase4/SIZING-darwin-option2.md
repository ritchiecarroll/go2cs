# SIZING — the darwin run layer, option 2 (`FuncPCABI0` + the syscall keystone)

**⚠ LIMITS, FIRST, BECAUSE THEY BOUND EVERY NUMBER BELOW.** There is **no Apple hardware in the fleet**.
This is a **reading of the code** at `a02ac3df3` (the corpus of record until H5) plus the two mac
runners' **compile** census — it is **not a run**, and nothing here has been executed on darwin. The
sharper form of the limit, which matters more than the absence of a box: **the linux number this record
is asked to use as its template was DISCOVERED BY RUNNING.** The linux run layer's cost is known to be
three hand-owned files because four failures were hit, one at a time, by launching a converted program
and reading the exception it died in. That method is unavailable here. So what follows sizes the
**bounded, structural** part of option 2 — which is genuinely bounded, and that is the finding — and
states plainly that the **unbounded** part cannot be sized from a container, only discovered on a Mac.

**Ruled at** mailbox `3fe51ddbe` §2 (lane C2, 2026-09-13), train 49, docs only, no code.
**Companion records:** [`DESIGN-darwin-run-layer.md`](DESIGN-darwin-run-layer.md) (the design and its
ABI reading), [`FINDING-darwin-run-layer.md`](FINDING-darwin-run-layer.md) (why a converted darwin
program dies before `Main`), [`FINDING-linux-run-layer.md`](FINDING-linux-run-layer.md) (the template).

<!-- Provenance. Every count below was taken at a02ac3df3 in a worktree whose src/core is byte-identical
     to that commit (asserted with `git diff --stat a02ac3df3 HEAD -- src/core`, empty).
     Counted with `git grep`, never bare `rg`: rg honors src/core/.gitignore and under-counts, which
     DESIGN-darwin-run-layer.md §1 already records.
     The bodyless-partial predicate is that document's own, and it was CALIBRATED before use: run
     against the darwin flavor it reproduces §1.1's readings exactly -- 288 total, and per package
     syscall 147 / runtime 55 / internal/syscall/unix 37 / internal/poll 12 / os 4. Reproducing a known
     reading is what makes the linux column below a measurement rather than a new number nobody can
     check.
     TWO MEASUREMENT TRAPS HIT AND CORRECTED WHILE TAKING THESE, recorded because both produced
     plausible output:
       (1) the calibrated pattern was "improved" to use (?:...) non-capturing groups. `git grep -E` is
           POSIX ERE and REJECTS those, so git errored, the output was empty, and the script reported
           0 declarations and 0 implementations for BOTH flavors. A zero from a failed command reads
           exactly like a zero from a clean tree. Fixed by restoring the calibrated pattern and adding
           an assert that the parsed declaration list is NON-EMPTY before any ratio is computed.
       (2) the keystone call-site count for `syscall` read 132 and 26 of those were
           `initᴛᴛimportꓸsyscall()` -- GoInit import initializers, not keystone calls. The separator is
           a non-ASCII character, so it satisfied a `[^A-Za-z_.]` boundary. Caught by LOOKING at five
           matched lines instead of trusting the count; the real figure is 111, and the inspection is
           what produced the better predicate (a keystone call passes abi.FuncPCABI0(<x>_trampoline),
           which is the actual coupling). -->

---

## 1. The template: what the linux run layer actually cost

From [`FINDING-linux-run-layer.md`](FINDING-linux-run-layer.md), and it is the most useful number in
this document:

| | linux |
|:--|--:|
| `PartialStubGenerator` stubs in the build's reachable closure | **284** |
| hand-owned files the run layer actually needed | **3** |
| converter registry rows | **2** |
| changes to any converted `.cs` | **0** |

The three files: `internal/runtime/syscall/linux/syscall_linux_impl.cs` (the keystone),
`syscall/linux/syscall_linux_impl.cs` (scheduler-bracket no-ops), `runtime/goargs_impl.cs`
(`argslice` from the CLR). The two rows: `linknameForwardTargets["runtime.fcntl"]`,
`linknamePushTargets["os.runtime_args"]`.

**284 → 3.** That record states the reason in its own words: the census is *"a superset, not a work
list: most are never called on any given path (`runtime`'s 170 are largely scheduler/GC internals the
managed model deliberately never runs)."*

**So a stub census is not a cost estimate, and the darwin stub count below must not be read as one.**
That is the single most important thing this sizing has to say, and it is measured rather than argued.

**How the 3 were found:** four failures, in sequence, each surfaced by running a converted program and
reading where it died — a wiring root cause, then the `internal/runtime/syscall.Syscall6` wall, then
`runtime_args` at `os.init()`, then `runtime_entersyscall` at `syscall.Syscall`. **Each one was
invisible until the one before it was fixed.** No reading of the corpus produced that list, and no
reading of the corpus would have.

---

## 2. The darwin seams, counted at `a02ac3df3` — with linux beside them

### 2.1 The three packages the ruling names

| package | darwin `.cs` | linux `.cs` | darwin bodyless | linux bodyless | delta |
|:--|--:|--:|--:|--:|--:|
| `runtime` | 57 | 62 | 55 | 44 | **+11** |
| `syscall` | 33 | 34 | 147 | 25 | **+122** |
| `internal/syscall/unix` | 17 | 21 | 37 | 8 | **+29** |

**Whole flavor:** darwin **288** bodyless partials, linux **95**, windows **53**.

⚠ **The file counts are nearly equal and the bodyless counts are 3× apart.** darwin has *fewer* `.cs`
files than linux in two of the three packages. The asymmetry is not volume of code — it is that
**darwin's syscall entry points are libc assembly trampolines in Go**, emitted as bodyless partials,
where linux's are direct kernel-ABI calls behind a single bottom. `syscall` alone carries +122 of the
+162 total gap.

### 2.2 The hand-own split (the `.auto`-versus-`_impl` question)

There is **no `.auto.cs` convention in this corpus** — `find src/core -name '*.auto.cs'` returns 0. The
real three-way split is *plain converted* (regenerable wholesale), `*_impl.cs` *companions* (which
supplement bodyless partials), and `[module: GoManualConversion]` *whole-file replacements*:

| package | GOOS | total `.cs` | `_impl.cs` | `GoManualConversion` |
|:--|:--|--:|--:|--:|
| `runtime` | darwin | 57 | 7 | 15 |
| `runtime` | linux | 62 | 7 | 14 |
| `syscall` | darwin | 33 | 3 | 7 |
| `syscall` | linux | 34 | 6 | 11 |
| `internal/syscall/unix` | darwin | 17 | 2 | 4 |
| `internal/syscall/unix` | linux | 21 | 1 | 2 |

**darwin is already hand-owned to roughly linux's depth in `runtime` and is THINNER in `syscall`**
(3 companions against 6, 7 markers against 11) — which is where its gap is largest. That direction is
consistent with a flavor that compiles but has never run.

### 2.3 Stub coverage, and the fact that reframes it

| | darwin | linux |
|:--|--:|--:|
| bodyless partial declarations (whole flavor) | 288 | 95 |
| `*_impl.cs` companion files | 15 | 17 |
| distinct names given a real body in a companion | 18 | 20 |
| **declarations still filled by a throwing stub** | **243** | **62** |

⚠ **Linux ships a working, green, validated run layer with 62 declarations still stub-filled.** They
are on paths its rows never take. **That is the empirical ceiling on how much a stub count can tell
you**, and it is why §1's 284 → 3 is the right lens for darwin's 243.

### 2.4 The keystone and its callers

Ten keystone declarations in `syscall/darwin` (`Syscall`, `Syscall6`, `Syscall9`, `syscall`,
`syscall6`, `syscall6X`, `syscallX`, `syscallPtr`, `rawSyscall`, `rawSyscall6`), 2 declaration sites
each. Call sites across the whole darwin flavor:

| keystone | call sites | | keystone | call sites |
|:--|--:|:-|:--|--:|
| `syscall` | 111 | | `syscallX` | 5 |
| `rawSyscall` | 62 | | `syscall6X` | 4 |
| `syscall6` | 20 | | `syscallPtr` | 4 |
| `Syscall` / `Syscall6` / `Syscall9` | 2 each | | `rawSyscall6` | 3 |
| | | | **TOTAL** | **215** |

And the coupling that decides the design:

| | count |
|:--|--:|
| `abi.FuncPCABI0(…)` call sites in the darwin flavor | **263** |
| **distinct trampolines whose address is taken** | **208** |

**THE LEVERAGE, stated as a ratio because that is what a sizing is for: 11 implementations — 10
keystones plus one real `FuncPCABI0` — stand under 215 keystone call sites, 263 `FuncPCABI0` sites and
208 distinct trampolines.** Option 2 is not "implement 243 stubs" and it is not "implement one thing".
It is **eleven**, and every one of the 208 trampolines reaches libSystem through them.

This is also why option 2 is the shape and not a `LibraryImport`-per-symbol approach: 208 distinct
trampolines against 11 implementations is a ~19:1 reduction, and the trampoline→symbol map needed to
make it work is already derivable from the committed corpus (`DESIGN-darwin-run-layer.md` §1.2 measured
123 `cgo_import_dynamic` comment lines surviving into `zsyscall_darwin_amd64.cs`, with the trampoline
NAME deriving the symbol exactly — zero mismatches — giving two independent sources for one map and a
free cross-check).

---

## 3. Where darwin diverges from the linux template

| | linux | darwin |
|:--|:--|:--|
| the bottom | **1** entry (`internal/runtime/syscall.Syscall6`) | **10** keystones, by result width and rawness |
| how a call reaches the kernel | direct kernel ABI | through a **libc trampoline** whose ADDRESS the runtime must resolve |
| `FuncPCABI0` | not on the path | **required**, and today returns `0` |
| what a missing bottom does | `NotImplementedException` at first use | dies in a **module initializer, before `Main`** |
| arch coverage committed | — | **amd64 only**; 9 arch-specific files, 0 `_arm64` |

**Two consequences for the sizing.**

**(a) The keystone work is ~10× linux's, and it is still bounded.** Ten declarations over one
parameterized helper, not ten independent implementations (`DESIGN-darwin-run-layer.md` §1.3).

**(b) `FuncPCABI0` has no linux analogue, and it is the half nobody would notice was missing.** It
compiles today and returns a plausible `0`. A run layer that implements the ten keystones and leaves
`FuncPCABI0` returning `0` hands every one of the 208 trampolines the address `0`.

**And the arm64 debt is NOT part of option 2, priced separately so a green arm64 run is never mistaken
for done:** `osx-arm64` compiles amd64 constant tables today — **9 amd64-specific files under `darwin/`
and zero `_arm64`**, re-measured here. The keystone design does not change with the arch; the **tables**
do, and that is a layout question (`-platforms darwin/arm64` under L3, a per-GOARCH dimension *within* a
GOOS that the layout does not have) which should be ruled on its own.

> **Re-measurement note.** `DESIGN-darwin-run-layer.md` §5.2 records **8** here. The difference is
> naming, not drift: that list has 8 distinct *names* and counts `defs_darwin_amd64.cs (×2 packages)`
> once, and `signal_amd64.cs` carries no `darwin` in its filename. Counted as *files under a `darwin/`
> directory whose name contains `amd64`*, `a02ac3df3` reads **9**. Both readings are right about
> different questions; stated because a reader comparing the two documents should not have to work it
> out, which is this repository's own rule about carried counts.

---

## 4. What the mac runners can and cannot tell us

- **`goos=darwin stage=census` runs daily at 04:41 UTC and fans out to BOTH mac runners** (arm64 and
  x64) in one dispatch. It is a **compile** gate.
- **darwin has compiled clean since 2026-08-23** (census run `32649840220`, zero errors).
- **The run question is already SETTLED negative, and re-observing it is waste:** the first darwin
  `behavioral-smoke` compiled and golden-matched 20/20, then failed all twenty at Output with
  `exit code mismatch: C# 2 vs Go 0` — the module-initializer death. `CIMatrix.md` records that until
  the run layer exists this reports uniformly and is "a known state, not a new finding, and not worth a
  runner hour to re-observe."
- ⚠ **The census's latency is a real cost in any sequenced plan:** a one-line `CS0266` in
  `os/darwin/dir_darwin_impl.cs` was found **seven days** after it landed, by the first darwin dispatch
  since. Daily-only feedback means a darwin implementation loop is paced in days unless dispatched
  by hand, and `macos-15` (arm64) is a 3-core runner (the 1.5× multiplier in the matrix's own costing).

---

## 5. The sequenced plan, and the first measurable step

Each step ends in a **reading somebody can check**, and the sequence is ordered so the earliest steps
need no Mac at all.

| # | step | needs a Mac? | the measurable outcome |
|--:|:--|:--|:--|
| **1** | **Derive the trampoline→symbol map from the committed corpus, two independent ways — from the `cgo_import_dynamic` comments and from the trampoline NAMES — and assert they agree.** | **no** | a checked-in map plus a standing guard; the count of symbols agreeing, and **0** disagreeing. Refuses if the two derivations differ. |
| 2 | A real `FuncPCABI0`: trampoline identity → symbol → `NativeLibrary.GetExport`. | no (compiles), yes (runs) | it returns non-zero for every one of the 208 trampolines, asserted against the step-1 map |
| 3 | One parameterized keystone helper + the 10 declarations. | no (compiles) | darwin census still clean on both runners; marker-census delta posted |
| 4 | First `behavioral-smoke` on both mac legs. | **yes** | the 20/20 Output failure either clears or moves; **the shape of the new wall IS the deliverable** |
| 5 | Iterate the linux loop's shape: run, read where it died, fix, repeat. | **yes** | one named failure per iteration, as linux's four were |
| 6 | The arm64 table debt, ruled separately. | yes | `-platforms darwin/arm64` under L3 |

**STEP 1 IS THE FIRST MEASURABLE STEP AND IT IS AVAILABLE TODAY, on this container, with no Apple
hardware and no converter change.** It is the whole reason to name it first: it converts the largest
single unknown in the design (does the map exist and is it self-checking?) into a committed artifact
with a guard, and `DESIGN-darwin-run-layer.md` §1.2 already measured that both derivations exist and
agree on the 123 symbols it could see. Step 1 extends that from 123 to the full 208 and makes the
cross-check standing.

**Everything from step 4 down is unsizeable from here** and this record does not pretend otherwise.
Linux needed four iterations; darwin starts behind a module-initializer death rather than a
first-use exception, which is a worse position to iterate from because nothing runs at all until the
keystone exists.

---

## 6. The bottom line

| | |
|:--|:--|
| **bounded and sizeable** | 11 implementations (10 keystones + `FuncPCABI0`), ~1–3 new `*_impl.cs` companions, **0** new `GoManualConversion` markers, **0** converted-`.cs` changes, **0** converter changes expected |
| **not sizeable from here** | how many failures follow the keystone. Linux took 4, found only by running |
| **priced separately** | the arm64 table debt (a layout ruling, not a run-layer one) |
| **available today, no Mac** | step 1 — the doubly-derived, self-checking trampoline→symbol map |

**The honest headline: option 2's structural cost is small and bounded — eleven implementations under
215 call sites — and its residual risk is entirely in the iteration nobody can run from this fleet.**
The 243 stub-filled declarations are **not** the work list; linux proves that directly, shipping green
with 62 of its own still unfilled.

<!-- Readings in this document, all at a02ac3df3 unless stated:
     288/95/53 flavor bodyless partials (predicate calibrated against DESIGN §1.1's 288 and its
       per-package 147/55/37/12/4 -- exact reproduction);
     per-package darwin/linux .cs 57/62, 33/34, 17/21; bodyless 55/44, 147/25, 37/8;
     _impl 7/7, 3/6, 2/1; GoManualConversion 15/14, 7/11, 4/2;
     companions 15/17, distinct names bodied 18/20, still stub-filled 243/62;
     keystone call sites 215 total (111/62/20/5/4/4/3/2/2/2 after excluding 26 GoInit initializers);
     abi.FuncPCABI0 sites 263; distinct trampolines 208; find -name '*.auto.cs' = 0.
     From FINDING-linux-run-layer.md: 284 closure stubs, 3 hand-owned files, 2 registry rows, 4
     failures in sequence. From CIMatrix.md: daily 04:41 UTC darwin census, both mac runners per
     dispatch, clean since 2026-08-23 (run 32649840220), the 7-day CS0266 latency, the 1.5x macOS
     multiplier, and the settled behavioral-smoke 20/20 Output failure. From DESIGN-darwin-run-layer.md
     §1.2-1.3: 123 cgo_import_dynamic lines surviving into the emitted C#, 0 name/symbol mismatches,
     ten keystones over one parameterized helper; §5.2: 8 darwin arch NAMES all _amd64 (9 files at this base, reconciled in §3), 0 _arm64. -->
