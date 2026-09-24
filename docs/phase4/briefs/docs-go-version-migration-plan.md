# Docs Go-version migration plan: Go 1.23 → Go 1.24.13 (the docs-migration seat's edit plan)

- **Read at:** `claude/version-go1.24.13` @ `fa18863b94` (the release content). Master is `074a12c4ae`. None of master's 13 extra commits touches a file in this plan. Every citation is `path:line@fa18863b94` unless a ref is named.
- **Owner direction (2026-09-24):** user-facing docs that mention Go 1.23 move to Go 1.24. This includes the README's real-world-module walkthrough. Milestones, news history and dated records stay as they are. The edits land with the approved release announcement, **before the `nuget-1.24.13.1` tag** (ritual element 1, runbook :3201-3209).
- **Inputs:** the classifier's 250-occurrence table, the checkers' 46 defects, and three finder notes (sweep, read, tooling).
- **How this plan was checked:** every defect was re-read at the source before being applied. Section 8 lists each verdict. Planner corrections that no input carried are marked **[P]**.
- **Honesty note:** the planner's work was read-only against the repo. `git status --porcelain` still shows only the pre-existing `?? du.exe.stackdump`. Two slips happened and were cleaned up:
  - one `git show … > /tmp/mg.ps1` redirect wrote outside the allowed path; it was deleted at once;
  - five census scratch files were written under `docsver/`; they were deleted because this file is the only one the task names.

---

## 0. Rules for the seat (timing, branch, gates, ownership)

1. **Branch and timing.**
   - The edits land on the branch RN-12 names, in the same stack as the C5 announcement and before the tag.
   - go2cs.net serves `master:/docs`, and `docs/README.md` is both the site home page and the README GitHub renders (there is no root README). None of this is visible to visitors until the RN-13 cutover.
   - Under RN-7's default, the cutover follows the publish. So every sentence that depends on the publish is true by the time a visitor can see it, for example `go.<pkg> 1.24.13.*` restoring from nuget.org, or the `validation/1.24.13.1` links.
   - If COORD rules the cutover *before* the publish, the walkthrough and the side-by-side table would name packages that do not exist yet. Re-sequence in that case.
2. **Ownership boundaries. Do not double-edit.**
   - `src/core/testing/README.md` and `src/core/unsafe/README.md` belong to the brief's C2 (RN-17: compose and prove against a converted sibling). They are listed in §2 only so the reviewer sees the whole set.
   - The featured NEWS block belongs to C5 (OWNER-ASK 1).
   - `src/push-nuget.ps1` is under the RN-6 release-census seat, so its comment example is deferred (§6).
   - `src/_roster.ps1` may also be touched by RN-6. Check for a collision before editing it.
3. **Never hand-edit generated output.** That means package READMEs under `src/core` (except testing/unsafe) and `docs/validation/**` proof pages. `docs/Performance.md` is AUTO-COPIED: make its edits in `src/tests/Performance/README.md`, which runs 2 lines behind (Performance.md :N = README :N-2), then mirror them or re-run the copy.
4. **Gates after the edits:**
   - the roster guard (brief A6, pwsh 7 shape): rc 0 and `0 of N`, with 2e keeping the NEWS block equal to NEWS.md's newest header;
   - `check-roster-format.ps1` rc 0;
   - the index dry run (`regen-validation-index.py`) shows no change;
   - a **bare** `migrate-gorelease.ps1 -From 1.23.12 -To 1.24.13` census under Windows PowerShell 5.1. Predicted after this plan plus OWNER-ASK 6: every live DOC-STATEMENT site reads `migrated`, the CLAUDE.md row and the roster row read `retired`, and there are no mismatches;
   - repoguard (`TestNoFleetIdentifiersInTrackedFiles`) for any `.ps1`, `.go` or `.props` edit. `TestContextBudget` is owed if CLAUDE.md or `.claude/**` is touched. CNR is not owed for docs or for `.ps1` comments.
5. **Staging.** Stage by explicit path only. Never use `git add -A`. After staging, assert that `git status --porcelain | grep '^ D'` is empty.

---

## 1. Summary

**Counts per class.** Units: MIGRATE and RERUN are line or block edits; OWNER-ASK is asks, with the lines they cover; GENERATED-CHECK is consolidated findings; KEEP is the classifier's occurrence count after adjustment.

| Class | Count | Change from the classifier |
|---|--:|---|
| MIGRATE | **66** edits (5 are optional comment-only; 3 belong to C2) | 53 − 8 moved to KEEP (Roadmap :31, roster :504/:584, Reference :19256/:19260/:19307/:19479 now handled by inserted dated blocks, push-nuget :21 deferred) + 2 from RERUN (README :126, :444) + 19 new from defects and the planner |
| RERUN-THEN-MIGRATE | **12** | 12 − 2 (README :126, :444) + Background :12 + Performance :181 |
| OWNER-ASK | **9** asks, covering 25 lines | 11 lines + README :39, :530, converter.md :650/:799, Background :35, migrate-gorelease :389, 8 class-(c) comment lines |
| GENERATED-CHECK | **27** findings | 39 classifier entries consolidated; added: the tls manifest, the anchor-page links, the TestGCMAsm pin, Reference :936 |
| KEEP-HISTORICAL | **~168** occurrences | 135 − 2 + 7 moved in + about 28 added (perf and harness `go.mod` floors 16, reconvert-deletions.ps1 2, roster :36-42/:71/:121-122, run-validated-sweep :630, inbound anchors 3, and others) |

**Files edited by this seat now (14):**
- docs/README.md
- docs/Roadmap.md
- docs/Background.md
- docs/ConversionStrategies.md
- docs/ConversionStrategies-Reference.md
- docs/ValidatedTestPackages.md
- docs/Glossary.md
- docs/CleanupBacklog.md (optional)
- src/tour/README.md
- src/version.props (comments only)
- src/set-version.ps1 (comment only)
- src/_roster.ps1 (comment only)
- src/core/testing/README.md (C2 owns)
- src/core/unsafe/README.md (C2 owns)

**Files edited after the reruns (+2):**
- src/tests/Performance/README.md
- docs/Performance.md (mirror)

**Files edited only if the matching OWNER-ASK is approved:**
- ask 1: docs/NEWS.md and the docs/README.md block
- ask 4: src/tour/{tour.go, pipeline.go, tour_test.go, pipeline_integration_test.go, runtime_test.go, go.mod}
- ask 5: CLAUDE.md and .claude/rules/converter.md
- ask 6: src/migrate-gorelease.ps1 (**needed in the same commit as README :172**)
- ask 7: docs/GoCorpusMigration.md
- ask 8: src/check-roster-format.ps1, src/_roster.ps1 and src/core/crypto/tls/go2cs_test_disclosures.json

**The three things most likely to go wrong:**
- (a) Editing README :172 without the instrument split in OWNER-ASK 6. The census goes red, and next hop's `-Apply` refuses.
- (b) Landing the walkthrough text before re-proving it at 1.24.13 (RERUN R2).
- (c) Rewriting dated blocks in place. The HashTrieMap and weak sections, and the roster's H10 relocation map, get **appended dated amendments**, not rewrites.

---

## 2. MIGRATE: exact edits per file (old → new)

### 2.1 `docs/README.md`: 18 edits

**Side-by-side table (:111-120).** These are DOC-STATEMENT anchors. All six Go files exist in go1.24.13, and all six `.cs` files exist at the tip. Timing: the C# column links `blob/master/src/core`, which holds the 1.23.12 conversion until the cutover. See §0.1.

- :111
  - old: `next to their original **Go 1.23.12** source, in order of increasing richness:`
  - new: `next to their original **Go 1.24.13** source, in order of increasing richness:`
- :113
  - old: `| Package | Go 1.23.12 source | Converted C# | What it shows |`
  - new: `| Package | Go 1.24.13 source | Converted C# | What it shows |`
- :115-120. The only change on each line is `https://github.com/golang/go/blob/go1.23.12/` → `https://github.com/golang/go/blob/go1.24.13/`. New lines:
  - :115 `` | `errors` | [errors.go](https://github.com/golang/go/blob/go1.24.13/src/errors/errors.go) | [errors.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/errors/errors.cs) | Error values and an unexported type satisfying the `error` interface. | ``
  - :116 `` | `cmp` | [cmp.go](https://github.com/golang/go/blob/go1.24.13/src/cmp/cmp.go) | [cmp.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/cmp/cmp.cs) | Generics with an ordered-type constraint. | ``
  - :117 `` | `unicode/utf8` | [utf8.go](https://github.com/golang/go/blob/go1.24.13/src/unicode/utf8/utf8.go) | [utf8.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8.cs) | Constants keeping Go's hex/binary literal formatting; arrays and structs. | ``
  - :118 `` | `sort` | [search.go](https://github.com/golang/go/blob/go1.24.13/src/sort/search.go) | [search.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/sort/search.cs) | Binary search driven by a `func(int) bool` closure. | ``
  - :119 `` | `strings` | [reader.go](https://github.com/golang/go/blob/go1.24.13/src/strings/reader.go) | [reader.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/strings/reader.cs) | A struct with receiver methods, tuple returns, and interface implementation. | ``
  - :120 `` | `container/list` | [list.go](https://github.com/golang/go/blob/go1.24.13/src/container/list/list.go) | [list.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/container/list/list.cs) | A doubly-linked list — pointers and receiver methods. | ``

**Features lead (:126).** Reclassified from RERUN: this needs a git census, not a Go run. The owner's standing rule keeps corpus figures out of durable prose, so the count goes abstract. OWNER-ASK 2 may also scope "full Go language surface".
- old: `go2cs converts the full Go language surface — the same converter that emits the 302 packages above:`
- new: `go2cs converts the full Go language surface — the same converter that emits the whole standard library above:`

**Requirements (:172).** `src/go2cs/go.mod:3` reads `go 1.24.13`. The `+` is dropped on purpose:
- A go2cs built with a newer local Go emits `<GoStdLibVersion>{that release}.*` (moduleConverter.go:733-739, from `goVersion()` at readme.go:93-101).
- `checkNuGetStdLibCompatibility` (toolchainResolution.go:240-245) compares only `version.Lang`, so it does not refuse a patch mismatch.

**Must land with OWNER-ASK 6(b)**, or the Try-it anchor matches twice.
- old: `- **[Go 1.23+](https://go.dev/dl/)** — the converter is a Go program, and it uses the Go toolchain to load`
- new: `- **[Go 1.24.13](https://go.dev/dl/)** — the converter is a Go program, and it uses the Go toolchain to load`

**Real-world-module walkthrough (:283-304), the owner-named item.**
- The `fatih/color` v1.18.0 pin stays. Go 1.25 is still the threshold (v1.19+ and current `x/sys` require go 1.25), and MVS keeps v1.18.0's own `x/sys` requirement.
- The whole walkthrough is re-proved by R2 before these lines land.
- :283
  - old: `Next, pin the app to a **Go 1.23-compatible** dependency set and confirm it builds as Go.`
  - new: `Next, pin the app to a **Go 1.24-compatible** dependency set and confirm it builds as Go.`
- :285 **[P]** uses the exact release for consistency with :172 and :289. A `≤ 1.24` bound would admit a `go 1.24.14+` directive that a local 1.24.13 refuses.
  - old: ``> **NOTE:** _go2cs is built with **Go 1.23**, so its type-checker only reads modules whose `go` directive — and their dependencies' — is **≤ 1.23**. `fatih/color` v1.19+ and current `golang.org/x/sys` require Go 1.25, which would fail step 2 with_ `package requires newer Go version go1.25`_; pin as shown._``
  - new: ``> **NOTE:** _go2cs is built with **Go 1.24.13**, so its type-checker only reads modules whose `go` directive — and their dependencies' — is **≤ 1.24.13**. `fatih/color` v1.19+ and current `golang.org/x/sys` require Go 1.25, which would fail step 2 with_ `package requires newer Go version go1.25`_; pin as shown._``
- :289. The checker's defect is applied: an older 1.24.x patch emits `1.24.x.*`, which no published package matches. Whether NuGet's nearest-match fallback would then rescue the restore was **not verified** (no network). The exact release is the documented path either way.
  - old: `First pin the toolchain, so Go uses the 1.23 you have instead of fetching the newer one a dependency asks`
  - new: `First pin the toolchain, so Go uses the Go 1.24.13 you have instead of fetching the newer one a dependency asks`
- :303 (the `#` column is kept)
  - old: `go get github.com/fatih/color@v1.18.0   # a Go 1.23-era release (v1.19+ requires Go 1.25)`
  - new: `go get github.com/fatih/color@v1.18.0   # a Go 1.24-compatible release (v1.19+ requires Go 1.25)`
- :304
  - old: `go mod tidy                             # download color + its (Go 1.23-era) dependencies`
  - new: `go mod tidy                             # download color + its (Go 1.24-compatible) dependencies`

**Project layout (:444).** Reclassified from RERUN; abstract, as at :126.
- old: `` | `src/core/` | The converted Go standard library — 302 packages, with `unsafe` and `testing` hand-written rather than converted. Everything (tests, tour, NuGet) builds against this one tree. | ``
- new: `` | `src/core/` | The converted Go standard library — every package, with `unsafe` and `testing` hand-written rather than converted. Everything (tests, tour, NuGet) builds against this one tree. | ``

**Try-it prerequisite (:481).** DOC-STATEMENT anchor. The `-tests` toolchain pin refuses any toolchain that is not the exact one.
- old: ``**[Go 1.23.12](https://go.dev/dl/)** (for the reference `go test` run), the``
- new: ``**[Go 1.24.13](https://go.dev/dl/)** (for the reference `go test` run), the``

**Disclosed divergence (:509-511 → 5 lines; :512 `tests, each affected package pins …` follows unchanged).**
- Why the paragraph changes: at 1.24 the Try-it package itself carries a `deferred` disclosure, `TestRuneCountNonASCIIAllocation` (`src/core/unicode/utf8/go2cs_test_disclosures.json`).
- **[P]** The checker's replacement drops `alloc-profile` from its list of cannot-satisfy kinds, but 42 `alloc-profile` entries remain at the tip. Its own utf8 example is also an escape-analysis want-zero assert, the same shape as the "cannot" kind it names. This wording therefore assigns no specific kind to "cannot".
- old:
  ```
  A few packages carry a **disclosed divergence**: a Go test asserting something a managed runtime provably
  cannot satisfy — an exact allocation count (Go's `testing.AllocsPerRun`, reached through compiler escape
  analysis), or a collectibility check Go answers from per-safepoint liveness maps. Rather than skip those
  ```
- new:
  ```
  A few packages carry a **disclosed divergence**: a Go test asserting something the converted runtime does
  not satisfy — an allocation count Go meets through compiler escape analysis, or a collectibility check Go
  answers from per-safepoint liveness maps. Some of these a managed runtime provably cannot satisfy; others
  — marked `deferred`, such as `unicode/utf8`'s own zero-allocation `TestRuneCountNonASCIIAllocation` — it
  can, and each of those is pinned against the named plan that will retire it. Rather than skip those
  ```

### 2.2 `docs/Roadmap.md`: 3 edits

- **Insert a present-tense status paragraph above :8.** This replaces the classifier's rewrite of :31: the defect is applied, and :31 stays as a dated amendment. The inserted text goes between :7 (blank) and :8, inside the blockquote. Fill `<release date>` at landing. It makes no compile claim; that lives at :874 (R7).
  ```
  > **Status (<release date>): the corpus is on Go 1.24.13**, published as `1.24.13.1`. The hop re-derived
  > every roster row from Go 1.24.13's own test sources; the [roster](ValidatedTestPackages.md)'s header
  > carries the current figures. The dated status below records the Phase-3 milestone.
  >
  ```
- :105. DOC-STATEMENT anchor.
  - old: ``- **Converter-improvement loop (proven end-to-end):** edit `src/go2cs/*.go` → `go build` (Go 1.23.12) →``
  - new: ``- **Converter-improvement loop (proven end-to-end):** edit `src/go2cs/*.go` → `go build` (Go 1.24.13) →``
- :851. Git census at the tip: 342 package projects in `src/go2cs-stdlib.slnx`, and "the 37" at :854 re-derived unchanged. So about 305 are platform-neutral out of about 340. The abstract alternative is "(roughly nine in ten)".
  - old: `2. **Platform-neutral converted packages** (~270 of ~305) contain no platform-varying code at all —`
  - new: `2. **Platform-neutral converted packages** (~305 of ~340) contain no platform-varying code at all —`

### 2.3 `docs/Background.md`: 2 edits (owner voice; wording kept minimal)

- :11. DOC-STATEMENT anchor. This is true on nuget.org once 1.24.13.1 publishes.
  - change: ``(versioned `1.23.12.<build>` from`` → ``(versioned `1.24.13.<build>` from``
  - The rest of the line is unchanged.
- :18. DOC-STATEMENT anchor. The wording `packages whose Go {OLD} sources actually define` is **kept** so that the anchor survives. The stricter corpus-axis wording from the roster :664 defect is not used here, for that reason.
  - change: `pushing through the 215 testable Go packages` → `pushing through the 230 testable Go packages`
  - change: `the converted packages whose Go 1.23.12 sources actually define tests` → `the converted packages whose Go 1.24.13 sources actually define tests`
  - The rest of the line is unchanged.

### 2.4 `docs/ConversionStrategies.md`: 5 edits

- :17. DOC-STATEMENT anchor. `src/core` at the tip is the 1.24.13 conversion. A separate audit of whether each pasted snippet matches current emission is advisable but not part of this seat.
  - old: `> Go 1.23.12) wherever possible, paired with their original Go source. A few use small illustrative`
  - new: `> Go 1.24.13) wherever possible, paired with their original Go source. A few use small illustrative`
- :131. Re-derived by the doc's own methods at both refs:
  - csproj files with `$(GoTargetOS)/*.cs`: 37 at both refs;
  - `go2cs-stdlib.slnx` projects: 307 → 344;
  - csproj files with `ItemGroup Condition="'$(GoTargetOS)'…"`: 21 → 22.
  - **[P]** The 37 includes 5 test csproj, but the slnx does not list test projects. That mixed rule dates from 1.23 and is kept for continuity. The strictly like-for-like pair would be 32 of 342, and 20.
  - old: ``vary: **37** of 307. A package whose *imports* also differ by platform — **21** of them, `os` reaching``
  - new: ``vary: **37** of 344. A package whose *imports* also differ by platform — **22** of them, `os` reaching``
- :1639. `internal/concurrent` does not exist at go1.24.13. The same `newIndirectNode[K, V](nil)` store is at `internal/sync/hashtriemap.go:50` and `:527`.
  - old: ``` `*File` methods return `ErrInvalid` instead of panicking, and why `internal/concurrent`'s ```
  - new: ``` `*File` methods return `ErrInvalid` instead of panicking, and why `internal/sync`'s ```
- :1834. Change the **link text only**; keep the anchor. The Reference heading is kept and gets a dated block instead (§2.5). So :1834, Reference :19014 and :19340, and `src/archived/Baseline-vs-FullConversion.md:209` all keep resolving. The tip's `src/core/weak/pointer.cs` has the same `WeakReference`/`ConditionalWeakTable` design this sentence describes.
  - old: `` [`internal/weak.Pointer`](ConversionStrategies-Reference.md#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one) ``
  - new: `` [`weak.Pointer`](ConversionStrategies-Reference.md#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one) ``
- :1854-1856 → 4 lines; :1857 `a slice<T>/array<T> is a window …` follows unchanged.
  - Verified in go1.24.13:
    - `crypto/internal/fips140/subtle/xor_generic.go:55` (`[]byte` viewed as `[]uintptr`);
    - `crypto/internal/fips140/sha3/sha3.go:30` (`a [1600 / 8]byte`) and `keccakf.go:43-56` (viewed as `*[25]uint64`), so the direction is **reversed** from x/crypto;
    - `vendor/golang.org/x/crypto/sha3` is absent.
  - The tip hand-owns `xor_generic.cs` and `keccakf.cs`/`keccakf_impl.cs`, both using `MemoryMarshal.Cast`.
  - old:
    ```
    family, and it needs no OS at all.** `crypto/subtle` views a `[]byte` as `[]uintptr` to XOR a word at a
    time; `golang.org/x/crypto/sha3` views its `[25]uint64` sponge state as `[200]byte` to absorb and
    squeeze. Both are ordinary managed storage on both sides, and neither view exists in the managed model —
    ```
  - new:
    ```
    family, and it needs no OS at all.** `crypto/internal/fips140/subtle` views a `[]byte` as `[]uintptr` to
    XOR a word at a time; `crypto/internal/fips140/sha3` views its `[200]byte` sponge state as `[25]uint64`
    to run the Keccak permutation. Both are ordinary managed storage on both sides, and neither view exists
    in the managed model —
    ```

### 2.5 `docs/ConversionStrategies-Reference.md`: 15 edits

The instrument classes this file MUST-NOT-CHANGE, which is correct for *substitution*. These are deliberate hand edits. In dated sections, the edit is an **appended dated amendment, never a rewrite**.

- :298. Present-tense list naming two packages that are gone at 1.24.
  - **Default wording: assert no new total.**
    - A crude marker census could not reproduce the doc's set. At both refs it also flags `crypto/internal/{fips140/,}alias`, `crypto/internal/boring/bcache`, `internal/reflectlite` and the vendored `x/crypto/internal/alias`.
    - Separately, `1a328f3ee8` rewrote `internal/godebug/package_info.cs` (see GENERATED-CHECK G27).
    - Restate a total only after running the driver's own predicate: `conversionDriver.go:316-334`, where every build-matching Go file's `.cs` carries the marker.
  - change the first sentence
    - from: ``Seven hand-owned `core` files are never re-emitted (`golib`, `testing`, `unsafe`, `internal/godebug`, `internal/concurrent`, `internal/weak`, and `core/Directory.Build.props`), so they carry the form by hand.``
    - to: ``The hand-owned `core` files that are never re-emitted — `golib`, `testing`, `unsafe`, `internal/godebug` and `core/Directory.Build.props` among them — carry the form by hand. (At Go 1.23.12 the list also named `internal/concurrent` and `internal/weak`; their Go 1.24 successors `internal/sync` and `weak` convert ordinary files beside the hand-owned one, so both emit their own `.csproj`.)``
  - The rest of the line is unchanged.
- :587-590. Same basis. At the tip, `weak/doc.cs`, `internal/sync/mutex.cs` and `internal/sync/runtime.cs` are unmarked, so both successors emit their project file (`conversionDriver.go:316-318`, `:334`).
  - old:
    ```
    Six `.csproj` are hand-owned and carry the policy by hand rather than by emission — `core/unsafe` and
    `core/testing` (skip-listed packages), `core/internal/godebug`, `core/internal/weak` and
    `core/internal/concurrent` (whose only Go file is fully hand-owned, so `unmarkedFileCount == 0` makes the
    driver `continue` before `writeProjectFile`), and `core/golib`. golib keeps a *shorter*, deliberately
    ```
  - new:
    ```
    Hand-owned `.csproj` carry the policy by hand rather than by emission — `core/unsafe` and
    `core/testing` (skip-listed packages), `core/internal/godebug` (whose only Go file is fully hand-owned,
    so `unmarkedFileCount == 0` makes the driver `continue` before `writeProjectFile`), and `core/golib`; at
    Go 1.23.12 `core/internal/weak` and `core/internal/concurrent` were two more, and their Go 1.24
    successors `weak` and `internal/sync` emit their own. golib keeps a *shorter*, deliberately
    ```
- :613. The tip's generated READMEs emit `io@go1.24.13#Reader`. The emission is toolchain-driven (readmeDocLinks.go).
  - change: `` `https://pkg.go.dev/io@go1.23.1#Reader` `` → `` `https://pkg.go.dev/io@go1.24.13#Reader` ``
- :630-634. Change `@go1.23.1` → `@go1.24.13` on each line. New lines:
  - :630 `` | `ImportPath` | `https://pkg.go.dev/io@go1.24.13` | ``
  - :631 `` | `ImportPath`, `Name` | `https://pkg.go.dev/io@go1.24.13#Reader` | ``
  - :632 `` | `ImportPath`, `Recv`, `Name` | `https://pkg.go.dev/io@go1.24.13#Writer.Write` | ``
  - :633 `` | `Name` | `https://pkg.go.dev/<current>@go1.24.13#Name` | ``
  - :634 `` | `Recv`, `Name` | `https://pkg.go.dev/<current>@go1.24.13#Recv.Name` | ``
  - The instrument keeps these MUST-NOT-CHANGE, so they will lag again next hop. A `go<release>` placeholder would stop that; this is optional.
- :934-935. Line :934 is part of this edit because it says "the three".
  - :934 old tail: `…record: the three` → new tail: `…record: the`
  - :935 old: `` **hand-owned-by-consequence** packages (`internal/concurrent`, `internal/godebug`, `internal/weak`) ``
  - :935 new: `` **hand-owned-by-consequence** packages (`internal/godebug`; at Go 1.23.12 also `internal/concurrent` and `internal/weak`) ``
  - :936's "never re-emit a `package_info.cs` at all" is contradicted for `internal/godebug` by `1a328f3ee8` (G27). Do not restate it without re-deriving it.
- :15613 **[P]** sits inside a dated entry (`sha3.xorIn`/`copyOut`, 2026-08-16, :15603). **Append** at the end of the paragraph:
  - ``*(At Go 1.24.13 `golang.org/x/crypto/sha3` is no longer vendored and this file is gone; the standard library's Keccak lives in `crypto/internal/fips140/sha3`, whose hand-owned `keccakf.cs`/`keccakf_impl.cs` take the reverse view — the `[200]byte` state as `[25]uint64` — by the same `MemoryMarshal.Cast` remedy. The MSTest guard's disposition is recorded at `src/tests/GolibTests/GolibTests.csproj:193-196`.)*``
- **Insert after :18216.** This dated section (`hostConditional`, 2026-08-20 coordinator ruling, :18206) cites 3,243 at :18210/:18229 and 401 + 1 / 400 + 2 at :18214/:18226. Those lines are **kept**, with one dated note inserted between :18216 and :18217. The figures come from `src/run-validated-sweep.ps1:629-630`, `docs/ValidatedTestPackages.md:273` and `docs/validation/current/crypto.tls.md:42`.
  ```

  > *At go1.24.13 (after this ruling):* the BoGo fan-out is 3,419 rows (1 parent + 1,022 pass + 2,396
  > skip), `crypto/tls` banks 4759 + 1, and `TestBogoSuite` agrees pass/pass on its proof page, so the
  > annotated entry discloses nothing there. The figures in this entry are its date's.
  ```
- :18837. The paragraph is dated (2026-07-20), so the defect is applied and the paragraph is **not rewritten**. **Append** at its end, after `…would hit the identical build blocker.`:
  - `` At Go 1.24.13 the set follows 1.24's surface: `Chdir` and `Context`, which Go 1.24 added to `TB` and `F` inherits from `common`, joined it (`src/core/testing/testing.cs:589-591`). ``
- **Insert after :19257, the blank line under the `### internal/concurrent.HashTrieMap` heading.** The heading, :19260 ("Go 1.23's implementation") and :19307 ("Go 1.23's zero value") are **kept**. They accurately describe the quoted 1.23 code block (:19263-19273, `NewHashTrieMap`) and the table (:19300-19309). The heading anchor is also linked from `src/archived/Baseline-vs-FullConversion.md:209`. Sources for the block:
  - `src/core/internal/sync/hashtriemap.cs:45-63` (the "Go 1.24.13 surface" header);
  - go1.24.13 `internal/sync/hashtriemap.go:30-56`, `:199-522`;
  - `sync/hashtriemap.go:5` and `internal/buildcfg/exp.go:83`.
  ```
  > **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 surface it was written against).**
  > The package moved to `internal/sync`, and the hand-own is now
  > [`src/core/internal/sync/hashtriemap.cs`](../src/core/internal/sync/hashtriemap.cs), whose header
  > records the 1.24.13 surface: `NewHashTrieMap` is gone and the zero map is seeded by `init`/`initSlow`
  > on first touch (still from `abi.TypeOf(m).MapType()`'s `Hasher`); `V` widened to `any`, so
  > `CompareAndSwap` and `CompareAndDelete` panic up front for a non-comparable `V`; `keyEqual` is gone;
  > seven methods were added (`Clear`, `CompareAndSwap`, `Delete`, `LoadAndDelete`, `Range`, `Store`,
  > `Swap`); and `internal/sync` is no longer fully hand-owned, because `mutex.go` and `runtime.go`
  > convert. At 1.24 the same map also backs every `sync.Map` by default (`goexperiment.synchashtriemap`).

  ```
- **Insert after :19480, the blank line under the `### internal/weak.Pointer` heading.** The heading is **kept** (four inbound links resolve to it). The body describes the 1.23 package (`Strong()`, `src/core/internal/weak/pointer.cs`, "fully hand-owned", the marker census). Sources: go1.24.13 `weak/pointer.go:83` and `weak/doc.go`; the tip's `src/core/weak/{pointer.cs,doc.cs,weak.csproj,package_info.cs,README.md}`.
  ```
  > **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 `internal/weak` it was written
  > against).** The package is the public `weak`, and `Strong` became `Value`; the hand-own is
  > [`src/core/weak/pointer.cs`](../src/core/weak/pointer.cs), on the same `WeakReference` design. It is
  > no longer its package's only Go file (`doc.go` converts), so `weak` re-emits its `.csproj`,
  > `package_info.cs` and `README.md`, and the layer beneath it is `internal/sync.HashTrieMap`.

  ```
- :21529. The section was ruled 2026-09-05 (:21459). Census of `"class": "alloc-count-semantics"` across the committed manifests: 8 at `nuget-1.23.12.3`, 5 at the tip.
  - old: ``**And a THIRD label stays, for a different reason.** `alloc-count-semantics` (8 entries) names an``
  - new: ``**And a THIRD label stays, for a different reason.** `alloc-count-semantics` (8 entries at this ruling; 5 at go1.24.13) names an``

### 2.6 `docs/ValidatedTestPackages.md`: 10 edits (run `check-roster-format.ps1` after)

- :4. DOC-STATEMENT anchor. Every banked row's first proof page reads Go 1.24.13 (:903-905).
  - old: ``Each package below has its own Go 1.23.12 `_test.go` suite converted to C#, built against the``
  - new: ``Each package below has its own Go 1.24.13 `_test.go` suite converted to C#, built against the``
- :68 **[P]**. "One entry holds the class" is false: the tip has 4 `host-limit` entries (crypto/tls, os/exec, and syscall ×2), and 1.23.12.3 had 3.
  - old: `  retire itself when the shape gains its named property. **One entry holds the class**:`
  - new: `  retire itself when the shape gains its named property. **Its founding entry**:`
- :170-171. The `crypto/rand` example is false at the tip: the row reads `314 | 1` with `linux: 314 + 1`, the same on both OSes. The `path/filepath` half (61 on Windows, `linux: 54`) holds.
  - old :170: ``set per `GOOS` — build-tagged tests, `GOOS`-keyed skips, capability gates — so `crypto/rand` offers``
  - new :170: ``set per `GOOS` — build-tagged tests, `GOOS`-keyed skips, capability gates — so `path/filepath` offers``
  - old :171: ``302 eligible verdicts on Linux where Windows offers 298, and `path/filepath` 54 where Windows offers``
  - new :171: `54 eligible verdicts on Linux where Windows offers`
- :172. DOC-STATEMENT anchor. :172 still begins `61. The **Tests** …`, which completes :171.
  - change: `the Windows record for the Go 1.23.12 era` → `the Windows record for the Go 1.24.13 era`
- :239. The note is dated ("SCOPED OFF WINDOWS AT go1.24.13 (2026-09-22 …)"), so this is an **append, not a rewrite**. The linux re-read is `971d919113` (2026-09-23, "the vgetrandom rows' Linux annotations re-read WITH the seat"). **[P]** The wording deliberately adds **no new literal `linux: N` token**: the cell already carries two, and the guard's annotation parse must stay unambiguous.
  - after the fragment `…annotation still claims it until the linux axis re-runs at 1.24.13.`, insert:
  - ``` ⚠ **RE-READ ON LINUX AT go1.24.13 (2026-09-23, `971d919113`):** the linux axis now reads the annotation below, with nothing disclosed, which retires the `13 + 1` reading.```
  - See G26 for the possible orphan disclosure this exposes.
- :244. Prose only; counts and links are untouched. At go1.24.13, `crypto/ed25519` imports `crypto/internal/fips140/ed25519`.
  - change: ``Ed25519 over the converted `crypto/internal/edwards25519` —`` → ``Ed25519 over the converted `crypto/internal/fips140/ed25519` (on `crypto/internal/fips140/edwards25519`) —``
- :267. Prose only. At go1.24.13, `crypto/rsa` imports `math/big`, `crypto/internal/fips140/rsa` and `crypto/internal/fips140/bigmod`.
  - change: ``RSA end to end over the converted `math/big` and `crypto/internal/bigmod` —`` → ``RSA end to end over the converted `math/big`, `crypto/internal/fips140/rsa` and `crypto/internal/fips140/bigmod` —``
- **Insert a banner after :442, the heading `## The H10 relocation map`.** This replaces the classifier's edits at :504 and :584. The defect is applied: those lines sit inside a section drafted 2026-09-20 and a ruling of 2026-09-20 (:573). The banner also covers :574 and :657-658. Place it between :443 (blank) and :444 (the HTML comment), followed by a blank line.
  ```
  > **Recorded 2026-09-20, before the H10 close.** Since the close (2026-09-23) the banked columns carry
  > each row's go1.24.13 figures, and master carries the 1.23.12 anchor only until the version cutover.

  ```
- :664-666. This takes the defect's corpus-axis definition, matching :138-140. The tip's exclusion ledger is 1 E1 (runtime/internal/wasitest), 1 E3 (internal/unsafeheader) and 4 E4 (runtime/trace, net/internal/cgotest, internal/copyright, crypto/internal/fips140deps). All six are in `population-go1.24.13.txt`. This line never matched the instrument's plural anchor.
  - old:
    ```
    The naive denominator above — 215 — counts every converted package whose Go 1.23.12 sources define
    a `Test` function. Six of those cannot be validated *at all* — five because a property of the target
    stands in the way that no amount of converter effort changes, and one (E4) because its comparison runs
    ```
  - new:
    ```
    The naive denominator above — 230 — counts every converted package whose Go 1.24.13 test files, on the
    corpus axis (windows/amd64, `-tags purego,math_big_pure_go`), declare a `Test` function. Six of those
    cannot be validated *at all* — two because a property of the target stands in the way that no amount of
    converter effort changes, and four (E4) because their comparison runs
    ```
  - :667 `cleanly and validates nothing. …` follows unchanged.
  - Optional: :687 "Four classes are in evidence" lists E1-E4, while only E1, E3 and E4 have rows. It reads fine as "defined", so leave it.
- :671-683 (13 lines) → 10 lines. The block is present tense and false at the tip:
  - "the figures above are the Go 1.23.12 anchor", when they are 1.24.13's;
  - "the five packages left here" includes `unique`, which banked at 1.24.13 (`21 | 1`, :439), and the other four are among the six candidates at :922-931.
  - The frozen snapshot `docs/validation/1.23.12.3/ValidatedTestPackages.md` exists, and its :143 reads 204 / 215.
  - The anchor `#the-230-at-the-h10-close-go12413-2026-09-23` is the heading at :895.
  - new:
    ```
    > **The Go 1.23.12 record is closed.** By owner ruling of 2026-09-07 the corpus moved to Go 1.24.13
    > rather than driving 1.23.12 to 100%, so that release's figures — 204 / 215 — are the **Go 1.23.12
    > anchor**, frozen in [its snapshot](validation/1.23.12.3/ValidatedTestPackages.md) rather than carried
    > as a running total; the figures above are Go 1.24.13's own. The reasoning changes what the
    > percentage *means*, so it is worth stating: the metric is **package-based, not content-based**, and
    > a row is all-or-nothing — a package matching most of its verdicts still scores **zero**, exactly as
    > one matching none of them does. Of the five packages that record left unbanked, `unique` has since
    > banked at Go 1.24.13; `reflect`, `runtime`, `runtime/pprof` and `net/http/pprof` are among the six
    > candidates in [The 230 at the H10 close](#the-230-at-the-h10-close-go12413-2026-09-23), each with
    > where it stands.
    ```

### 2.7 `docs/Glossary.md`: 1 edit (:57-59)

- :57 old: `The Phase-3 progress metric: how many of the ~302 auto-converted stdlib projects **emit their own`
- :57 new: `The Phase-3 progress metric: how many of the auto-converted stdlib projects **emit their own`
- :59 old: ``` `N / 302`. The metric is **packages-compiling, not error count** — clearing an error family can ```
- :59 new: ``` `N / total` (302 at the Phase-3 milestone, Go 1.23.1). The metric is **packages-compiling, not error count** — clearing an error family can ```
- :58 is unchanged.

### 2.8 `docs/CleanupBacklog.md`: 1 edit (optional, low priority)

- **Append after :297**, keeping the 4-space indent. Item 20's "still true" clause (:291-296) names two packages that are gone at 1.24.
  - `    *(<date>, Go 1.24.13: `internal/concurrent` and `internal/weak` no longer exist; their successors `internal/sync` and `weak` convert ordinary files beside the hand-owned one, so the class is re-derived rather than restated here.)*`

### 2.9 `src/core/testing/README.md` and `src/core/unsafe/README.md`: 3 edits (**owned by brief C2 / RN-17**)

These are listed here so the reviewer sees the full set. The seat that owns them composes the lines from `readmeValidationBadge.go` (`readmeDocsBadgeLine` :302-334, `readmeGoSourceBadgeLine` :227-256) and proves them against a converted sibling, re-composing as the control. The text below is the expected result. The Tests link and the `512BD4` C# badge are **left for push-nuget** (:883, :952-953) to retarget at the publish.

- testing :5 new: ``[![Tests](https://img.shields.io/badge/Tests-53%2F68_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/testing.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/testing@go1.24.13)\``
- testing :6 new: ``[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/testing) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/testing)``
  - Proof sibling: `src/core/testing/fstest/README.md:5-6`.
- unsafe :5 → **2 lines [P]**. The classifier kept unsafe's one-line layout on the grounds that it "matches converted none_to_validate READMEs (e.g. macos)". But macos is a stale seed survivor. At 1.24.13 the emitter's `none_to_validate` form is two lines, as in `src/core/crypto/fips140/README.md:5-6`. The new text:
  ```
  [![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/unsafe@go1.24.13)\
  [![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/unsafe) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/unsafe)
  ```
  C2 decides, with this as the controlled comparison.

### 2.10 `src/tour/README.md`: 3 edits

- :38. The tour builds go2cs from the checkout (`go.mod` 1.24.13), and a newer Go runs into the same patch-mismatch problem as README :172. Note: `migrate-gorelease.ps1:538` classes this file MUST-NOT-CHANGE, so the line will rot next hop unless OWNER-ASK 6(c) adds a site.
  - old: `- Go 1.23.1 or later`
  - new: ``- Go 1.24.13 (the release `src/go2cs/go.mod` pins)``
- :39. Adjacent fact: converted projects target `net10.0`.
  - old: `- .NET SDK 9.0 or later`
  - new: `- .NET SDK 10.0 or later`
- :92. The defect is applied: the example is release-neutral, so it neither rots nor names an unpublished version. The flag's default is empty (`main.go:73`). `runtime.go:74-84` reads `GO2CS_NUGET_VERSION`, then composes the version from `src/version.props`.
  - old: ``- `-nuget-version=1.23.1.2`: package version to restore``
  - new: ``- `-nuget-version=<version>`: package version to restore (default: `GO2CS_NUGET_VERSION`, else `<GoStdLibVersion>.<GoBuildNumber>` from `src/version.props`)``

### 2.11 Comment-only examples: 5 edits (optional; repoguard owed for `.ps1`)

Land these before the release pre-flight (C6), so the tree is clean. `version.props` is also rewritten by the publish (`GoBuildNumber`), so do not carry this edit on a second branch.

- `src/version.props`
  - :8: `(e.g. 1.23.1)` → `(e.g. 1.24.13)`
  - :11: `"1.23.1.1"` → `"1.24.13.1"`
  - :17: `the 3-part "1.23.1"` → `the 3-part "1.24.13"`
- `src/set-version.ps1:5`: `e.g. 1.23.1` → `e.g. 1.24.13`
- `src/_roster.ps1:17`. The defect is applied: the wording is release-neutral, so it stops rotting (the instrument's :524 keeps it MUST-NOT-CHANGE). Check for a collision with the RN-6 seat first.
  - old: `                   Columns 2 and 3 are the WINDOWS record for the Go 1.23.1 era, per the per-OS`
  - new: `                   Columns 2 and 3 are the WINDOWS record for the pinned Go release (<GoStdLibVersion>), per the per-OS`

---

## 3. RERUN-THEN-MIGRATE (12): what to run under Go 1.24.13 and what to update

**Common environment:**
- go2cs built from the release tip under go1.24.13;
- `GOTOOLCHAIN=local`, `CGO_ENABLED=0`;
- GOROOT spelled exactly as `go env GOROOT` prints it (backslashes);
- one conversion per output root;
- never on the i9 while a battery runs.

- **R1. README :326-342, the `main.cs` sample (:335 is representative).**
  - Run: walkthrough step 2 (`go2cs -recurse=nuget . csharp`) on the colorapp.
  - Update: paste the emitted `main.cs` verbatim, and diff it against :326-342. Nothing in the tree pins this text; the hop moved the converter.
- **R2. README :375 ("compiles clean … and runs") and Background :12 ("it restores, compiles and runs").**
  - Before the tag: run steps 1-4 end to end. Add the pre-flight or rehearsal's locally packed 1.24.13.1 nupkgs (`src/artifacts/nupkg`) as a NuGet source, because `1.24.13.*` is not on nuget.org yet.
  - Confirm the build and the run output. The screenshot at :373 does not change.
  - Also re-proved by this run: the "v1.19+ / current x/sys require Go 1.25" clause at :285 and :303, and the v1.18.0 pin.
  - After the publish: a nuget.org-only restore smoke test.
  - Update: no text change if all of this holds. **§2.1's walkthrough edits do not land without this proof.**
- **R3. README :394-398, the `-recurse=module` transcript.**
  - The source module (`myapp`, google.golang.org/api) is not identified, and the counts depend on the release: the 1.24 TLS/HTTP closures pull in `crypto/internal/fips140/*`.
  - Run: `go2cs -recurse=module . csharp` on that module if it can be found. **Otherwise**, run it on the walkthrough's colorapp and paste that real closure line and third-party list.
  - Last resort: mark the block as illustrative.
- **R4. README :461 (count plus compile claim).**
  - Target text (the DOC-STATEMENT anchor `packages, Go {OLD}\) compiles cleanly` still matches): `standard library (342 packages, Go 1.24.13) compiles cleanly as .NET assemblies.`
  - The rule, stated once: package projects in `src/go2cs-stdlib.slnx`, i.e. 344 minus `golib` and `go2cs-gen`, with hand-owned `testing` and `unsafe` included. This is the convention README :444 used.
  - Evidence to cite: the P1 compile record discharged at the H10 close STAMP (brief "Release facts"), or the release pre-flight's `dotnet build src/go2cs-stdlib.slnx`.
- **R5. README :498, the Try-it expected line.**
  - Predicted from `testConversion.go:8642-8643` and `docs/validation/current/unicode.utf8.md`: 14 = 15 Go verdicts − 1 disclosed; 47 excluded declarations, against 37 at 1.23.12.3.
  - Predicted line: `Validated 14 tests against go test (0 skipped identically on both sides, 1 disclosed-divergent (deferred), 47 disclosed-unsupported declarations excluded).`
  - Run the documented command (:490-492) once with go1.24.13 at that GOROOT, and paste the observed line.
- **R6. The performance claims.**
  - README :522-524 ("maps … at **parity with Go or faster in both C# variants**");
  - Background :22 and :24;
  - Performance.md :15 and :181-182, whose sources are `src/tests/Performance/README.md:13` and `:179-180`.
  - All rest on the go1.23.1 table (README:100, 2026-08-25). Go 1.24's Swiss-table maps change Go's side of the Map row.
  - Run: `src/tests/Performance/run-performance.ps1 --update-readme` with go1.24.13 pinned. Do the JIT column first (`--no-aot`); the AOT column takes hours per publish (Performance.md :221-224).
  - **Interim, before the tag, if the rerun has not happened:** label the claims on the pages that do not state their environment. The Performance page carries its own stamp, so it stays until re-measured.
    - README :524: `faster in both C# variants**. Most …` → `faster in both C# variants** (as measured against go1.23.1, 2026-08-25). Most …`
    - Background :22: `running at parity with Go or better` → `running at parity with Go or better (measured against go1.23.1)`
    - Background :24: `run at parity with Go *or faster*` → `run at parity with Go *or faster* (as measured against go1.23.1)`
- **R7. Roadmap :874, the progress row.**
  - Update from the compile record in R4: `` | Full packages compiling | `src/go2cs-stdlib.slnx` (344 projects at Go 1.24.13) | ✅ **342 / 342** packages (<date>, `<sha>`) | ``
- **R8. Background :39** carries a compile claim, so it is kept RERUN-gated on R4's evidence (the defect was only partly applied).
  - Target: `**What isn't true yet.** All 302 packages of the converted standard library compile as .NET assemblies` → `**What isn't true yet.** Every package of the converted standard library compiles as a .NET assembly`. The rest of the line is unchanged.

That is 12 items: README :335, :375, :394, :461, :498, :523; Background :12, :22 (with :24), :39; Performance :15, :181; Roadmap :874.

---

## 4. GENERATED-CHECK: does any tool or template still emit 1.23 where it should not?

**Answer:** the converter, gen and golib hard-code no Go release in any emitted text. The one exception is G13, a failure-path panic string. The 1.23 left in generated output comes from four sources:
- seed survivors the windows-axis reconvert does not re-emit;
- hand-owned files no regen reaches;
- disclosure-manifest `reason` prose that the proof-page renderer prints verbatim;
- the designed pre-publish badge state.

- **G1** `docs/validation/current/math.big.md:265`. The renderer (`validationProofPages.go:348`) prints the manifest's `reason` and never its `reading`.
  - The reason says "58x … 2026-08-29, go1.23.12". The manifest's own `reading` holds the 1.24.13 figure: "ratio 26 … readings-go1.24.13.tsv line 106". So a 1.24.13 page shows a stale figure.
  - Fix in `src/core/math/big/go2cs_test_disclosures.json` `reason` (keep `signature` byte-identical), or teach the renderer to print `reading`. Never edit the page.
- **G2** `docs/validation/current/net.netip.md:303` and 53 more: 54 reasons read "Measured …, 2026-08-29, go1.23.12/windows-amd64".
  - For the first entry the 1.24.13 `reading` agrees (2 per run). The other 53 were not checked.
  - Same remedy as G1.
- **G3/G4** `src/core/crypto/tls/go2cs_test_disclosures.json:14-15`. The prose carries 3,242/3,243 rows, 5,481 spawns and 2.61 s/~69x, all 1.23.12-era.
  - Nothing renders today, because the entry does not disclose at 1.24.13 (`crypto.tls.md:42` pass/pass).
  - Decision is OWNER-ASK 8.
- **G5-G7** Seed survivors (RN-5). Their generated Go-side badges lag:
  - `src/core/crypto/x509/internal/macos/README.md:5` at @1.23.1 (and the old one-line layout);
  - `src/core/internal/runtime/syscall/README.md:5` at @1.23.1;
  - `src/core/vendor/golang.org/x/net/route/README.md:5` at x/net `v0.25.1-0.20240603202750-6249541f2a6c`, plus a `go1.23.1` Source link. go1.24.13's vendor manifest pins `v0.32.1-0.20250304185419-76f9bf3279ef`.
  - Remedy: brief C2's default, or a converter seat.
- **G8** Generated package READMEs, the designed state. At the tip:
  - 342 of 349 `src/core/**/README.md` carry the C# badge `Source-@1.23.12.3`;
  - 214 link Tests to `go2cs.net/validation/1.23.12.3/`;
  - 320 already read @1.24.13 on the Go-side badges.
  - push-nuget.ps1:883 and :952-953 retarget the 342 and the 214 at the publish. **After the publish, assert zero `1.23.12.3` under `src/core/**/README.md`.**
- **G9** The same state produces dead links until the publish:
  - the C# badge of about 51 packages new or relocated at 1.24 names `tree/nuget-1.23.12.3/src/core/<pkg>`, a path absent at that tag (e.g. `src/core/weak/README.md:6`, `crypto/sha3`, `internal/sync`);
  - 21 Tests links name pages absent from the 1.23.12.3 snapshot (brief C1 list; e.g. `src/core/weak/README.md:5`, `src/core/crypto/internal/fips140/edwards25519/README.md:5`).
  - This contradicts the premise of the runbook's "unpublished line" (:3165-3179). It clears at the publish.
- **G10** `push-nuget.ps1:883/:952-953` have no pattern for the `00ADD8` Go-side badges. So no release ever fixes the hand-owned testing/unsafe badges or the three seed survivors.
- **G11** `src/tour/tour.go:21` (`tourGoVersion = "1.23.1"`, emitted at :199) and `src/tour/pipeline.go:173` emit `go 1.23.1` into Tour module text. Decision is OWNER-ASK 4.
- **G12** `src/tour/runtime.go:82` composes `1.24.13.0` from `version.props` until push-nuget commits `GoBuildNumber=1`. This matters only if master gets `version.props` before the publish.
- **G13** `src/go2cs/syscallKeepAliveAnalysis.go:301` is an **emitted** panic string ("Go 1.23.12 contains no call of this shape"); :286 has the same claim in a comment.
  - A go1.24.13 grep finds the same two integer-only sites (`runtime/memmove_linux_amd64_test.go:49`, `runtime/race/race_windows_test.go:33`). Confirm with the analyzer's own funnel predicate.
  - Then either name 1.24.13 or go release-neutral. This is a converter-source edit and needs converter gates; low priority, post-release.
- **G14** Emitter comment examples, optional refresh; they are never emitted:
  - readme.go:88;
  - readmeDocLinks.go:55-59, :98;
  - readmeValidationBadge.go:109, 116, 216, 219, 225, 288, 296;
  - moduleConverter.go:663;
  - toolchainResolution.go:219-220, :236.
  - If any is touched, keep each comment block internally consistent. A converter rebuild still fires.
- **G15** Emission confirmed as derived:
  - `readme.go:93` `goVersion()` (`go env GOVERSION`) feeds the Docs and Source badges, doc links, the `-recurse=nuget` default (`moduleConverter.go:739`, which emits `1.24.13.*` at the tip) and the proof-page fallback;
  - `toolchainResolution.go:223` → go1.24;
  - `testConversion.go:926`, the proof headline;
  - `readmeValidationBadge.go:505` (PublishedStamp);
  - `internal/buildcfg/windows/zbootstrap.cs:30` and `src/core/VERSION` = go1.24.13;
  - the README template (`readme.go:204-219`) and the csproj, package_info and gen templates carry no version.
- **G16** CI derives the version: `os-matrix.yml:152-165` (read at :158, used at :275) from `version.props`, and `licensing.yml:38` from `src/go2cs/go.mod` (`go 1.24.13`). No workflow hard-codes a Go release.
- **G17** The site: `docs/_config.yml:2` (classic Jekyll over `docs/`) and `docs/CNAME:1` (go2cs.net). It serves `master:/docs`, inferred from RN-12 and `readmeValidationBadge.go:77`. There is no version text in the layouts or config.
- **G18** `migrate-gorelease.ps1:903` sweeps with `git grep -F <fromRelease>`. It is blind to minor-only spellings (`Go 1.23+`, `1.23-era`, `≤ 1.23`, `go1.23`) and to older-patch ones (`1.23.1`). That is how it missed the whole walkthrough, README :172, the tour README, and the unsafe, macos and syscall badges.
- **G19** `migrate-gorelease.ps1:867`: the REVIEW scan lists lines only for DOC-STATEMENT files. Unclassified files only warn.
- **G20** `migrate-gorelease.ps1:485` (`^src/core/.+/README\.md$` → DERIVED-BY-REGEN) wins over :497. It labels the hand-owned testing and unsafe READMEs "moves by regen", which is false (runbook :3180).
- **G21** `migrate-gorelease.ps1:538` classes `src/tour/README.md` MUST-NOT-CHANGE as a "captured example". But :38 is a present-tense requirement.
- **G22** `migrate-gorelease.ps1:513` (converter `.go` = illustrative comment) is right except for G13.
- **G23** `migrate-gorelease.ps1:465-475`: the six live history anchors are `{OLD}`-templated but spelled at 1.23.1 in the tree, so with `-From 1.23.12` they print "not present" and guard nothing.
- **G24** `migrate-gorelease.ps1:1103-1110`: `-Apply` resets `<GoBuildNumber>` to 0 unless `-KeepBuildNumber` is passed. That is harmless before the publish, but it **turns 1 back into 0 after it**, so pass `-KeepBuildNumber` on any post-publish run.
- **G25** The ten inheritance-anchor proof pages under `docs/validation/current/` are **not orphans**. Each is linked once from the roster; master's `f147fe5973` teaches the index tool the same. So they are KEEP, and they must ride into the 1.24.13.1 snapshot. But:
  - their :12 link names `tree/master/src/core/<retired pkg>` (e.g. `internal.weak.md:12` → `src/core/internal/weak`);
  - `crypto.internal.edwards25519.md:78` and `crypto.internal.nistec.md:2223` link removed manifests.
  - All of these 404 after the cutover. Options: accept (provenance), or point the snapshot copies at `nuget-1.23.12.3`. This is for COORD, not this seat.
- **G26** crypto/cipher `TestGCMAsm`: the manifest pin is scoped to `["linux","darwin"]`, but the linux annotation (`971d919113`) shows nothing disclosed. This may be an **orphan disclosure** (the orphan-disclosure-check class). Route it to the validation lane.
- **G27** **[P]** Reference :936 says the hand-owned-by-consequence packages "never re-emit a `package_info.cs`". But `1a328f3ee8` (the hop's final seeded regen) rewrote `src/core/internal/godebug/package_info.cs` (an init-import line). Re-derive before any sentence restates the set (see §2.5 :298).

---

## 5. OWNER-ASK (9 asks)

1. **Featured NEWS block: `docs/README.md:12-39`** (:12 header, :14, :32, :34, :35, :37, :39).
   - Needs the owner-approved 1.24.13.1 announcement text: RN-11, brief C5, ritual element 1, before the tag.
   - The block contradicts itself at the tip. :14-19 give Go 1.24.13 figures (218/230, 56,974 verdicts, 97.3%), while :34 calls them "the Go 1.23.12 anchor".
   - :35 says the corpus "now moves" to 1.24.13. :37 says 1.23.12 "ships one final NuGet release first", but it already shipped (`nuget-1.23.12.3`).
   - :39's NEWS link closes :37's sentence, so it stays or goes with the approved text.
   - Guard 2e keeps this block equal to NEWS.md's newest header.
2. **"Full Go language surface" versus Go 1.24 generic type aliases: `docs/README.md:126` and `:134`.**
   - Go 1.24 enables `type A[P any] = …` by default.
   - At fa18863b94 no Behavioral test declares one, go1.24.13's non-test std has none, and the converter has no alias type-parameter handling.
   - Ask: prove them with a behavioral guard before the release, or scope :126 (e.g. "the full Go language surface through Go 1.23, plus …").
3. **Policy sentence: `docs/README.md:530`** "Newer Go and .NET versions are planned; a validated baseline comes first."
   - This release moved to Go 1.24 at a 97.6% baseline, by the 2026-09-07 ruling (Roadmap.md:30).
   - Owner-voice reword. Suggested: "Newer Go and .NET releases follow the same way: each hop re-derives every roster row from that release's own test sources — see the [Roadmap](Roadmap.md)."
   - The checker's "one variable per hop" is not used, because the 2026-08-25 hop moved .NET 10 and Go 1.23.12 together (README:555).
4. **Tour workspace Go floor: `src/tour/pipeline.go:173`.**
   - It writes `go 1.23.1` into every Tour lesson's go.mod, and `tour.go:21` writes the same into the upstream Tour's module text. So Go 1.24 language features typed into the Tour are refused.
   - Ask: raise to 1.24 for this minor hop, or keep 1.23.1.
   - Raising it is a code change: tour.go:21, pipeline.go:173, tour_test.go:71, pipeline_integration_test.go:126, and optionally runtime_test.go:224 and src/tour/go.mod:3. Gate: `go test` in src/tour.
5. **Doctrine lines that go stale at the release (RN-10, TestContextBudget).**
   - `CLAUDE.md:68-69` "the corpus is mid-hop / to **Go 1.24.13**". Suggested edit, same line count:
     - ":68 … is live via the `-tests` pipeline, and the corpus is on"
     - ":69 **Go 1.24.13**. Roster, counts and campaign state are in"
   - `.claude/rules/converter.md:650` and `:799` ("`GOTOOLCHAIN` stays UNSET (auto) on the 1.23.12 pin"). `migrate-gorelease.ps1:567-569` ordered them re-read at H5, and H5 has closed.
   - Ask: land them on the version branch with the release, or at the cutover, and say by whom.
6. **`migrate-gorelease.ps1` instrument edits (RN-10), in the same commit as the docs edits.**
   - (a) Retire the roster site at :403 (`packages whose Go {OLD} sources define`, 0 matches at fa18863b94) with `Retired='e43b8f3cda'`. That commit deleted the anchored line, `074a12c4ae:docs/ValidatedTestPackages.md:139`.
     - Roster :664 is singular and never matched, so the brief's "reworded :664" diagnosis is wrong.
     - Re-anchoring at :664 would pair 1.24.13 with the 1.23.12 figure 215.
   - (b) README :172's edit makes the Try-it anchor (:389, Expect 1) match twice. The result is census `mismatch` now and an `-Apply` refusal next hop. Split it with **ASCII-only lookaheads**, because the script is BOM-less and runs under PS 5.1, which would misread a literal em dash:
     - Try-it site: Find `\*\*\[Go {OLD}\]\(https://go\.dev/dl/\)\*\*(?= \(for the reference)`
     - new Requirements site: Find `\*\*\[Go {OLD}\]\(https://go\.dev/dl/\)\*\*(?= — the converter is a Go program)`
     - both: Replace `**[Go {NEW}](https://go.dev/dl/)**`, Expect 1.
     - Minimal alternative: Expect = 2 at :391, with a Note naming both sites.
   - (c) Optional: add a DOC-STATEMENT site for `src/tour/README.md` (`(?m)^- Go {OLD}(?= )`) and drop :538's MUST-NOT-CHANGE rule. Add a hand-edit rule for `^src/core/(testing|unsafe)/README\.md$` ahead of :485.
   - Ask: approve (a) and (b) at H12, or hold README :172 to the Expect = 2 form.
7. **Runbook wording: `docs/GoCorpusMigration.md:3185-3186`** ("The Go version appears in prose in the top-level docs, the roadmap, the roster and CLAUDE.md's architecture row"). The row was retired by 56ff452a5, and the 2026-09-20 amendment (:3187-3192) says the line "wants rewording at the next pass".
   - Ask: reword now (H12-9), or route it to the owner-ordered lessons-learned seat. The suggested text is in §7.
8. **BoGo figures in code comments and in the crypto/tls manifest (RN-10 classes (c) and (a)).**
   - Present tense, stating 3,242/3,243 (go1.23.12):
     - `src/_roster.ps1:1142, 1168, 1240, 1242, 1400, 1401`;
     - `src/check-roster-format.ps1:343`.
   - Already fine: `:501` is dated (2026-09-01), and `src/run-validated-sweep.ps1:629-630` already names both releases correctly. That line is the model for the fix.
   - Manifest prose: `src/core/crypto/tls/go2cs_test_disclosures.json:14-15` (G3/G4; it does not render today).
   - Ask: edit at H12, or leave them as dated readings. Editing owes repoguard; CNR is not owed for `.ps1` comments. Every `signature` and `hostConditionalSignature` must stay byte-identical.
9. **Owner-voice staleness (adjacent; not a version string): `docs/Background.md:35`** "Two classes qualify -- an exact allocation count … and a test asserting … collectible".
   - The tip's manifests carry eleven classes: runtime-capability 85, deferred 131, alloc-profile 42, platform-skip 19, host-identity 17, host-fatal 11, codegen-liveness 8, alloc-count-semantics 5, host-limit 4, cgo-configuration 4, structural 1.
   - Suggested: "Each class is named, with its bar, on the roster — from an exact allocation count, where … , to a test asserting …".

---

## 6. KEEP-HISTORICAL (brief, for the reviewer)

- **Dated records and history:**
  - `docs/NEWS.md` (21 hits; the new entry goes above :11);
  - README Milestones :540-557; README :344 (the release Linux first shipped in; a history anchor);
  - Roadmap :8-12 (Phase-3 status), :31 (the dated 2026-09-07 amendment; the defect is applied and the new status line goes above :8 instead), :127, :659, :699, :715, :824, and :854 ("the 37", re-derived unchanged at both refs);
  - `docs/phase4/**` (1,051), `docs/validation/**` snapshots, `docs/news/*`;
  - `docs/PLAN-*` (hop-campaign 30, corpus-upgrade 29, linux-operation 12, nugetgo 5, rebank 1, cgo 1, bflat 1);
  - `docs/phase3/*`;
  - `docs/doctrine/JOURNAL-2026-09-12.md` (frozen by blob identity);
  - `docs/GoCorpusMigration.md` (94 hits; runbook worked instances);
  - `src/archived/**`.
- **Release-labelled censuses and measurements in `ConversionStrategies-Reference.md`:** :3434, :3623, :4416, :6855, :7082, :7681, :7682, :8171, :9887, :9922, :9964, :12198, :15503, :18888, :18999, :19213, :21244. Also ConversionStrategies.md :1244 and :1547, and Glossary :211 and :334 (Hop A = Go 1.23.12 is accurate).
- **Feature attributions** that name the release which introduced a feature: README :153 and :157; Reference :802, :869, :877 (an explicit hypothetical), :8472, :8725, :9382, :20657, :20665, :20671; ConversionStrategies.md :999 and :1001; Architecture.md :74; roster :435.
- **Kept, with an appended dated amendment instead of a rewrite:**
  - Reference :18210/:18214/:18226/:18229, :18837, :19256/:19260/:19307 and :19479 (their headings keep their anchors; inbound links at CS :1834, Reference :19014 and :19340, and `src/archived/Baseline-vs-FullConversion.md:209` are unchanged);
  - roster :239, :504, :574, :584 and :657-658 (the banner after :442).
- **Roster provenance and records:** :36-42 (class definitions; a measurement, not a release statement), :71 (a dated 2026-08-28 measurement, 3,242 cases then; optionally add "at go1.23.12"), :121-122 (a founding-row record; net/http has since been demoted and the class has no live entry at 1.24.13), :146, :162, :251, :254-256, :259, :311, :359-360, :363, :423, :440, :508, :524, :805, :931, :938, :963, :989.
- **Proof pages:**
  - the ten inheritance anchors under `docs/validation/current/` (provenance; see G25);
  - `go.build.md:88` ← `src/core/go/build/go2cs_test_disclosures.json:8` and `sync.md:82` ← `src/core/sync/go2cs_test_disclosures.json:26` (dated readings; edit the manifest if ever, never the page);
  - `go.types.md:560-561` (Go's fixture name);
  - `validation/index.md:20-33`.
- **Performance:** `Performance.md:102/:276` and their sources at README :100/:274 (environment stamps). `src/tests/Performance/README.md:220` ("~three hundred packages", measured at the .NET 10 hop; optionally make it "several hundred" at the source and mirror it).
- **Module floors (inert for release tags; `migrate-gorelease.ps1:523/:530-535`):**
  - `src/tour/go.mod:3` (moves only with OWNER-ASK 4), `src/utilities/go.mod:3` (1.23.2), `src/tools/comparison-classifier/go.mod:3`;
  - 14 `src/tests/Performance/*/go.mod` at `go 1.23.1`, plus PerfRefLower and `src/tests/PackageTests/ConvertedTestHarness` at `go 1.23`. These are the configuration the published tables were measured with; revisit them when R6 re-measures;
  - never the ~700 Behavioral fixtures.
- **Worked examples in scripts** (valid syntax instances; optional refresh by the lessons-learned seat):
  - `migrate-gorelease.ps1:81, 111-112, 115, 119, 581, 585` and `.bat:6-7`;
  - `handown-census.ps1:24/:28` (a pair);
  - `reconvert-deletions.ps1:260/:266` and `.bat:10`, which are **already** the 1.23.12→1.24.13 pair and consistent;
  - `release-nuget.bat`;
  - `push-nuget.ps1:21` (**deferred**: RN-6's seat owns push-nuget.ps1 now).
- **Other:**
  - `.github/workflows/os-matrix.yml:65/:285` ("1.23.1.x anchors" names the .NET 9 control line; whether to retire the 9.0.x option is a .NET question);
  - `docs/TargetAtlas.html:216-301` (third-party modules' own go directives);
  - `docs/CleanupBacklog.md:276, 374, 378, 382`;
  - false positives: `news/2026-07-26…:150` ("1.23×") and `archived/…progress.txt:298` ("1.23s");
  - `.claude/**` except the lines routed to OWNER-ASK 5.

---

## 7. Relationship to `migrate-gorelease.ps1` and RN-10; the lesson for the next hop

**What the instrument did at this hop.**
- Its DOC-STATEMENT class anchors 14 regexes across six files (CLAUDE.md retired).
- After OWNER-ASK 6(a) and 6(b), the live anchors cover **17 occurrences in 5 files**:
  - README :111, :113, :115-120, :172, :461, :481;
  - roster :4, :172;
  - Roadmap :105;
  - Background :11, :18;
  - ConversionStrategies :17.
- That is the mechanical core of §2, about a quarter of the MIGRATE edits. The seat can land them by `-Apply` or by hand; the post-edit census reads `migrated` either way.
- Two of them are figure-bearing, and substituting only the release makes them false: README :461 (302 → R4) and Background :18 (215 → 230). Fix those figures in the same commit.
- Everything else in §2 is invisible to the tool by construction:
  - minor-only spellings (the walkthrough, README :172's "1.23+");
  - older-patch spellings (tour :38, the unsafe badges, all six history anchors);
  - files outside the six DOC-STATEMENT files, which get no line-level REVIEW;
  - release *relocations* that contain no version string at all (internal/concurrent → internal/sync, internal/weak → weak, x/crypto/sha3 → fips140/sha3);
  - figures (302, 215, the 37-of-307 ratio, 3,243, "8 entries");
  - "mid-hop" wording.

**What a re-anchored tool would cover next hop** (`-From 1.24.13 -To 1.25.x`), after 6(a)-(c):
- the 17 occurrences above, plus tour :38 (with 6(c));
- the full-release spellings this plan introduces in DOC-STATEMENT files (README :285 and :289 "Go 1.24.13"), which show in its REVIEW list.
- It would still miss:
  - README :283/:303/:304 ("Go 1.24-compatible");
  - the relocation class;
  - every figure;
  - prose outside the six files.

Recommended instrument work for the lessons-learned seat:
- (i) a **minor-only discovery pass**: `git grep -nE "(Go |go|≤ ?)<MAJOR.MINOR>([^.0-9]|$)"` over user-facing paths, reported for REVIEW;
- (ii) an **older-patch pass** keyed on `<MAJOR.MINOR>.`, not only the exact `-From`;
- (iii) REVIEW listing for **every** user-facing file, not just DOC-STATEMENT ones;
- (iv) the 6(c) path-class fixes;
- (v) history anchors re-spelled as literal, non-templated presence checks, or retired with a note, so they stop going dark;
- (vi) `-KeepBuildNumber` implied whenever `<GoBuildNumber>` > 0;
- (vii) a **relocation pass** fed by the hop's successor map (the H10 relocation map): grep user-facing docs for every retired import path.

**Suggested rewording of runbook :3185-3186 (OWNER-ASK 7):**
> - The Go version appears in prose under `docs/`: `docs/README.md` (the page GitHub renders as the repository README and Pages serves as the site home), the roadmap, the roster, `Background.md` and the strategy docs. It also appears in the hand-owned `testing`/`unsafe` READMEs and `src/tour/README.md`. `migrate-gorelease.ps1`'s DOC-STATEMENT class anchors only full-release spellings of the outgoing pin. Its fixed-string sweep cannot see minor-only spellings (`Go 1.23+`, `≤ 1.23`, `1.23-era`, `go1.23`), older-patch ones (`1.23.1`) or relocated package names. This rung therefore also needs a read, and the hop's successor map is the input for the relocation half.

**Lesson for the next hop (one paragraph).**
> A version hop's prose migration is only partly a substitution. At this hop about a quarter of the needed edits were full-release spellings the instrument could anchor. The rest came from four other sources:
> - minor-only and older-patch spellings the fixed-string sweep cannot see (the owner-named walkthrough among them);
> - package relocations that contain no version digits at all (internal/concurrent, internal/weak, x/crypto/sha3);
> - figures that move with the release (302, 215, 3,243, "8 entries");
> - dated sections whose present-tense clauses go stale without being wrong on their date.
>
> So:
> - Start from the hop's own successor map and a minor-level grep, not from `-From` alone.
> - Retire an anchor by the commit that deleted its prose rather than re-aiming it at a look-alike sentence.
> - Keep instrument regexes ASCII-only, using lookaheads, because the script runs under PS 5.1.
> - Amend dated blocks with dated notes instead of rewriting them.
> - Make every example either release-neutral or anchored, so it does not silently rot one hop later.
> - Sequence the walkthrough and restore claims after a real run against the new release's packages, because the docs land before the tag and the packages only exist after the publish.

---

## 8. Defect ledger: each checker defect, re-verified at the source

| # | Site | Verdict | Note |
|--:|---|---|---|
| 1 | README :172 vs Try-it anchor | **APPLIED, modified** | Verified :389/:391 and :696-699. Anchors are ASCII-only with lookaheads; the script is BOM-less under PS 5.1 (OWNER-ASK 6b) |
| 2 | README :289 | **APPLIED** | Verified readme.go:93-101, moduleConverter :739, toolchainResolution :240-245. The NU1101 outcome was not verified (possible NuGet nearest-match); exactness stands |
| 3 | README :39 | **APPLIED** → OWNER-ASK 1 | :39 closes :37's sentence |
| 4 | README :530 | **APPLIED, modified** → OWNER-ASK 3 | "One variable per hop" dropped (README:555) |
| 5 | roster :664 | **APPLIED** | Verified :138-140/:159-161; all six exclusions are in the population file. Not applied to Background :18 (anchor) |
| 6 | roster :584/:504 | **APPLIED** | Banner after :442; :574 and :657-658 covered |
| 7 | Reference :19260 | **APPLIED (a)** | Keep and insert a dated block; the BvF :209 inbound link is verified |
| 8 | Reference :19479 | **APPLIED (preferred)** | `Value` verified at weak/pointer.go:83 |
| 9, 26-28 | Reference :298/:587-590/:935 | **APPLIED, modified** | Dated wording; no new total. The predicate census was inconclusive (G27) |
| 10, 20 | Reference :18210-18229 | **APPLIED, modified** | One dated note after :18216; figures verified (rvs :629-630, roster :273, tls.md :42) |
| 11, 21 | Reference :21529 | **APPLIED** | 5 at the tip, 8 at nuget-1.23.12.3, verified |
| 12, 23 | tls manifest :14-15 | **APPLIED** → G3/G4 + OWNER-ASK 8 | |
| 13, 22 | roster :36-42/:71/:121-122 | **APPLIED** (all KEEP) | Adjacent :68 found and migrated [P] |
| 14 | README :126/:444, Background :39 | **APPLIED, partly** | :126/:444 → MIGRATE (abstract); Background :39 kept RERUN (it carries a compile claim) |
| 15 | _roster.ps1 :17; tour :92 | **APPLIED** | :92 default also names `GO2CS_NUGET_VERSION` (runtime.go:74-76) |
| 16 | Reference :18837; roster :239; Roadmap :31 | **APPLIED** | Dated appends. The roster append avoids a literal `linux: N`; the Roadmap status line goes above :8 |
| 17 | README :509 | **APPLIED, modified** | alloc-profile still has 42 entries, so "cannot" is not tied to a kind |
| 18 | Background :12 | **APPLIED** | Tied to R2 |
| 19 | Perf README :220/:13 | **APPLIED** (edit location) | :220 KEEP; :13/:179 RERUN (the page states its own environment) |
| 24 | run-validated-sweep :630 and the (c) comments | **PARTLY REJECTED** | :630 already names both releases correctly; the other lines → OWNER-ASK 8 |
| 25 | Glossary :57/:59 | **APPLIED** | Abstract, with 302 labelled as the Phase-3 figure |
| 29 | Reference :19295/:19362/:19509/:19557/:19579-19582 | **APPLIED** via the dated blocks | |
| 30 | Reference :15613 | **APPLIED, modified** | Dated append (the entry is 2026-08-16); the MSTest disposition is cited, not restated |
| 31 | inbound anchors | **MOOT** | Headings kept |
| 32 | roster :244/:267 | **APPLIED, modified** | Imports verified in go1.24.13 (ed25519 → fips140/ed25519; rsa → math/big, fips140/rsa, fips140/bigmod) |
| 33 | CleanupBacklog :291-296 | **APPLIED** (optional dated append) | |
| 34-36 | migrate-gorelease/.bat/handown-census examples | **REJECTED as MIGRATE** → KEEP | Valid worked examples; optional refresh by the lessons-learned seat |
| 37 | version.props :11/:17 | **APPLIED** | |
| 38 | reconvert-deletions.ps1 :260/:266 | **REJECTED** → KEEP | Already the 1.23.12→1.24.13 pair, consistent with .bat :10 |
| 39 | perf and harness go.mod | **APPLIED as a class decision** → KEEP | Revisit with R6 |
| 40 | tour_test.go :71 etc. | **APPLIED** → OWNER-ASK 4 | |
| 41 | generated README census | **APPLIED** → G8/G9 | 342/349 and 214 re-verified |
| 42 | anchor pages :12 | **APPLIED** → G25 | |
| 43 | manifest → page mapping | **APPLIED** → KEEP | |
| 44 | converter.md :799 (+ :650) | **APPLIED** → OWNER-ASK 5 | |
| 45 | emitter comment siblings | **APPLIED** → G14 (optional) | |
| 46 | tour README :39 | **APPLIED** | |

Planner additions [P]:
- the PS 5.1 em-dash hazard in instrument anchors;
- the unsafe README's two-line emitter layout;
- roster :68's false "one entry";
- README :285 made exact;
- the CS :131 rule mix;
- Reference :934 ("the three") and :936 (G27);
- `push-nuget.ps1:21` deferred behind RN-6;
- R3's fallback to the colorapp;
- R6's interim labels confined to pages that do not state their environment.
