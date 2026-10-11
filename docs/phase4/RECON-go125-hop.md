# RECON — the Go 1.25 hop (hop C), scope and shape

> **A read-only recon, taken 2026-10-11 by the i9 lane at COORD's request, for the owner's scoping call.**
> **The runbook leads.** [`../GoCorpusMigration.md`](../GoCorpusMigration.md) is the procedure. This record
> sizes the hop and offers shapes for it. It executes nothing and rules nothing.
>
> It builds on [`NOTES-next-hop.md`](NOTES-next-hop.md), COORD's pre-hop notes of 2026-09-29, which
> measured go1.24.13 against go1.25.13. **This record re-measures every count on a second toolchain and
> a second box.** Where a reading agrees with the notes, that is stated as corroboration. Where it does
> not, the difference is named.
>
> The shape follows [`RECON-go1.24-hop.md`](RECON-go1.24-hop.md): headline, method, the bills, what this
> record does not establish, then the recommendation.

---

## 0. Headline

- **The target is go1.25.14, and the series is closed.** go1.25.14 (2026-08-19) is the last 1.25
  point release. Go 1.26.9 and 1.27.2 shipped on 2026-10-08, so **1.25 is already outside upstream's
  two-release support window.** The hop lands the corpus on a frozen series. That is an argument for
  speed and against 1.25-only polish. It is not an argument for skipping the hop: OQ-18 keeps 1.25 and
  1.26 as separate hops.
- **No language change. The converter itself is ready to start.** At master `20279db5f6`, `src/go2cs`
  builds under go1.25.12 (rc 0) and is clean under its `go vet`. Both of 1.25's new analyzers,
  `waitgroup` and `hostport`, were confirmed live by a plant. Its use of the APIs that 1.25 deprecates
  or changes:
  - `parser.ParseDir`: one site, in a test;
  - `ast.FilterPackage`, `ast.PackageExports`, `ast.MergePackageFiles`, `token.FileSet.Iterate`: none;
  - `CoreType`: none in code, one mention in a comment.

  The go/types and go/ast changes are additive.
- **The roster bill is concentrated.**
  - 77 of the 225 rows (16.8% of verdicts) have no upstream `.go` change at all in their selected
    files.
  - 63 rows have a moved test-function set: +279 functions, −22. Those 63 rows carry **77.1% of the
    banked verdicts**.
  - 17 rows have a substantive hand-own in their own package. `runtime` alone has 24 of the 58.
- **Everything waits on `runtime`.** All 223 surviving rows reach three packages new in 1.25 through
  `runtime`'s own imports: `internal/runtime/gc`, `internal/runtime/strconv` and
  `internal/trace/tracev2`. **No row can validate until 1.25's `runtime` converts and compiles.** That
  holds in every hop shape, so H5–H7 cannot be staged.
- **"225/225 again" is the wrong finish line.** The go1.25.14 population of record, already on master
  (`data/population-go1.25.14.txt`, C2, 2026-09-30), has **237** testable packages against 1.24.13's 230.
  It adds 10 rows and removes 3. If the five exclusions carry, the implementable set is about **232**.
  A full hop is about 232/232: the 223 surviving rows plus 9 new candidates.
- **Three shapes, §3. Recommended: shape C**, the shape hop B actually ran. It is a full hop that
  publishes at the H10 close with a short demotion list, each row named. It needs one owner ruling,
  on P2 and OQ-10, scoped to that named list. The staged shape B saves less than its row split
  suggests, because the H6 work on the reserved rows' hand-owns sits on stage 1's critical path anyway
  (§3.2).

---

## 1. Method, and the controls

**Axes.** Every count states its axes (runbook §7, lesson 11).

- **The two GOROOTs:**
  - FROM: `~/sdk/go1.24.13`, the corpus pin.
  - TO: the module-cache toolchain `golang.org/toolchain@v0.0.1-go1.25.12.windows-amd64` (VERSION
    `go1.25.12`), read in place and never written.
  - Each side ran its own `go` binary with `GOROOT` set to its own root and `GOTOOLCHAIN=local`.
    `go version` printed `go1.24.13` and `go1.25.12` respectively.
- **The default axis:** `GOOS=windows`, `GOARCH=amd64`, `CGO_ENABLED=0`, default GOEXPERIMENT, run
  from a directory with no module.
- **Row axes:** row-level file comparisons use each toolchain's own `go list` selection (`GoFiles`,
  `TestGoFiles` and `XTestGoFiles`) under the corpus tags `purego,math_big_pure_go`.

**Why go1.25.12 and not .14.** This box has no go1.25.14, and fetching it needs the owner's OK (NOTES
B0). The three 1.25 toolchains cached here are .0, .11 and .12. Per go.dev's release history, .13 and
.14 changed only:
- security fixes to the `go` command, `crypto/tls`, `encoding/asn1`, `encoding/xml`, `html/template`,
  `net/http` and `net/url`;
- bug fixes to the compiler, the runtime, `crypto/tls` and `os`.

Counts on those rows can move by a little at the real target. Every ⟲ step re-measures at .14 anyway
(runbook §2).

**Controls:**
- **Package census.** It reproduces NOTES §3.8 exactly, on a different toolchain (.12 against the
  notes' .13): the same 13 added and 8 removed on windows, and the same totals on all three OSes.
- **The constrained row census.** The first pass compared whole package directories. It inflated
  `encoding/json` by +7,436 test lines, which were the `goexperiment.jsonv2` files. The second pass,
  on `go list` selections, drops them. That is a positive control that the build constraints were
  honoured. Every row figure below is from the second pass.
- **Hand-own census.** `src/handown-census.ps1`, unmodified, run at master. Its class sums self-check
  against the marker census. The worktree read `git status` 0 afterwards.
- **vet.** A planted file fired both new analyzers, so "clean" on the converter is a reading and not
  an inert run.
- **Relocations.** The two rows the census finds without a package, `go/ast/internal/tests` and
  `internal/trace/internal/oldtrace`, are exactly the two banked sources in
  `data/relocations-go1.25.14.tsv`. The census also sees `runtime` lose `TestAtoi` and `TestAtoi32`,
  matching that file's `runtime → internal/runtime/strconv` line, which C2 derived by test body.

---

## 2. What moved, go1.24.13 → go1.25.12, measured

### 2.1 Standard-library packages (H3 input)

| OS | go1.24.13 | go1.25.12 | added | removed |
|:--|--:|--:|--:|--:|
| windows | 346 | 351 | 13 | 8 |
| linux | 344 | 350 | 14 | 8 |
| darwin | 345 | 350 | 14 | 9 |

- **Added on all three OSes:**
  - `testing/synctest`, the one new public package;
  - `crypto/internal/fips140cache`;
  - `internal/runtime/{cgroup,gc,startlinetest,strconv,wasitest}`;
  - `internal/testhash`;
  - `internal/trace/internal/{testgen,tracev1}` and `internal/trace/tracev2`;
  - `math/big/internal/asmgen`;
  - `net/http/internal/httpcommon`.

  Linux also adds `internal/cgrouptest`, and darwin adds `internal/routebsd`.
- **Removed:**
  - `go/ast/internal/tests`;
  - `internal/trace/event` and `internal/trace/event/go122`;
  - `internal/trace/internal/oldtrace`;
  - `internal/trace/internal/testgen/go122`;
  - `log/slog/internal/slogtest`;
  - `runtime/internal/{startlinetest,wasitest}`.

  Darwin also loses `vendor/golang.org/x/net/route`.
- **Renames, matched by name only.** These are not verified by body except where
  `relocations-go1.25.14.tsv` already did so (†):
  - `runtime/internal/{startlinetest,wasitest}` → `internal/runtime/…`;
  - `internal/trace/internal/oldtrace` → `internal/trace/internal/tracev1` †;
  - `internal/trace/internal/testgen/go122` → `internal/trace/internal/testgen`;
  - `internal/trace/event` → `internal/trace/tracev2` (the closest name; unverified).
- **The population of record** (`data/population-go1.25.14.txt`, genpopulation; on master since
  `5b011e7cbf`) has 237 testable packages. Against 1.24.13's 230:
  - **added (10):** `crypto/fips140`, `crypto/internal/fips140/subtle`,
    `crypto/internal/fips140cache`, `internal/runtime/cgroup`, `internal/runtime/strconv`,
    `internal/runtime/wasitest`, `internal/trace/internal/tracev1`, `internal/trace/tracev2`,
    `math/big/internal/asmgen`, `testing/synctest`;
  - **removed (3):** `go/ast/internal/tests`, `internal/trace/internal/oldtrace`,
    `runtime/internal/wasitest`.
- **GOEXPERIMENT baseline** (`internal/buildcfg/exp.go`): loses `CoverageRedesign` and
  `SpinbitMutex`, and gains `Dwarf5`. The flag files `exp_{coverageredesign,spinbitmutex}_*` go;
  `exp_{dwarf5,greenteagc,jsonv2}_*` arrive. This is H5c's CS0102 class from hop B, as NOTES §3.7
  predicted.

### 2.2 Language and spec (H4)

- **None for programs.** The release notes say the spec drops "core types" in favour of prose.
  `go/types` keeps its behaviour, and the converter names `CoreType` only in a comment
  (`convCallExpr.go:3023`).
- **The compiler's nil-check fix changes observable behaviour.** Go now panics where 1.21–1.24 ran on.
  The C# side never had the bug, so differential rows can flip in either direction (NOTES §3.1; R11:
  classify closures as carefully as breaks).
- **More slice backing stores go on the stack.** This can tighten upstream `AllocsPerRun` asserts. The
  census finds 127 `AllocsPerRun(` sites in 48 rows' 1.25 test files, against 117 sites in 46 rows at
  1.24.13. That is lesson 31's reading-run path.

### 2.3 go/types, go/ast, go/token, go/parser — the converter's front end

Measured as the exported declaration lines of `go doc -all`, diffed between the two toolchains:

| Package | Added | Removed | Notes |
|:--|:--|:--|:--|
| `go/types` | `type VarKind`, `(*Var).Kind`, `(*Var).SetKind`, `VarKind.String`, `LookupSelection` | — | additive |
| `go/ast` | `PreorderStack` | — | `FilterPackage`, `PackageExports`, `MergePackageFiles`, `MergeMode` deprecated (still present) |
| `go/token` | `(*FileSet).AddExistingFiles` | — | `(*FileSet).Iterate` renamed its parameter (signature unchanged in effect) |
| `go/parser` | — | — | `ParseDir` deprecated (still present) |
| `go/constant`, `go/build`, `go/format`, `go/printer`, `go/scanner` | — | — | — |

**Converter exposure** at master (`src/go2cs`, non-test unless stated):
- `ParseDir`: 1 site, in `manualConversionVars_test.go:52`, a test.
- `FilterPackage`, `PackageExports`, `MergePackageFiles` and `FileSet.Iterate`: 0.
- `ast.Object` appears in 2 files, in its emitted-type handling, which is unchanged.
- **The module pins:** `go.mod` reads `go 1.24.13`, `golang.org/x/tools v0.42.0` and
  `golang.org/x/mod v0.33.0`. H1 step 3's ceiling rule (NOTES A12, Q9) stands unchanged.
- **The converter builds under go1.25.12 in 10 s and vets clean.** This is the H1 bill this record can
  read without converting the corpus. Running go1.25's `go vet` on the converter is H1's own item
  (lesson 8), and it reads zero findings today.

### 2.4 Runtime, reflect, unsafe, sync, testing — what golib emulates or hand-owns

- **`runtime`'s selected production files:**
  - +4,470 / −2,222 lines;
  - 78 changed files, 4 added, 2 removed;
  - **24 substantive hand-owns**, the largest block of H6 (§2.6).
- **Behaviour changes a row can observe** (release notes; NOTES §3.4):
  - container-aware, periodically re-read `GOMAXPROCS`, and the new `SetDefaultGOMAXPROCS`;
  - `panic: X [recovered, repanicked]`;
  - concurrent and parallel `AddCleanup` cleanups;
  - the `checkfinalizers` GODEBUG;
  - the trace flight recorder.

  **The panic text arrives by reconversion.** `runtime/panic.cs` is converted code; it prints
  ` [recovered]` at `:717`. It is correct only if golib sets the `repanicked` state where Go does.
  golib's panic carrier is `golib/PanicException.cs`, so this is an H6 and golib item that the
  reconversion alone does not settle.
- **`reflect`:** adds `TypeAssert[T]`. `reflect/value.go` behind `value_impl.cs` is substantive.
- **`unsafe`:** no API change. The only unsafe-adjacent item is stack-allocated slices exposing
  incorrect `unsafe.Pointer` use. That is a Go-side behaviour, so it is a differential classification
  item, not a golib item.
- **`sync`:**
  - adds `WaitGroup.Go`;
  - 4 substantive hand-owns;
  - the WaitGroup bubble association that `testing/synctest` needs (NOTES §3.3).
- **`testing`, the hand-owned host:**
  - `T`, `B` and `F` gain `Attr` and `Output`;
  - `AllocsPerRun` panics under parallel tests;
  - the `testingSynctestTest` hook.

  **Receiver-typed demand** (lesson 13): `.Attr(` appears in 2 sites, both in the `testing` row's own
  tests. `.Output()` appears in 48 sites over 16 rows, but almost all of those are `exec.Cmd.Output()`;
  that is the name over-match the lesson warns about.

  **`testing/synctest` is demanded by two banked rows:** `synctest.Test(` appears in `net/http`
  (1,387 verdicts) and `encoding/json` (532). The `wg.Go(func` name appears in 9 sites over 5 rows
  (`encoding/json`, `internal/synctest`, `net/http`, `os`, `sync`). A row whose tests call
  `WaitGroup.Go` needs it in `sync`.
- **`unique`, `iter`, `os`, `io/fs`, `hash`, `crypto/*`:**
  - `os.Root` gains 12 methods, and `File.Fd` changes receiver name;
  - `io/fs` gains `ReadLinkFS`, `Lstat` and `ReadLink`;
  - `hash` gains `Cloner` and `XOF`;
  - `testing/fstest.MapFS` gains `Lstat` and `ReadLink`.

  The detail is in NOTES §3.5. This record re-measured the declaration deltas and found them as listed
  there.

### 2.5 Test-suite changes in the 225 roster rows

Each row's selected files were compared between the two GOROOTs. The rows are classed exclusively, in
this order: relocated, then test-function set moved, then both, then production only, then tests only,
then untouched.

| Class | Rows | Banked verdicts | Share |
|:--|--:|--:|--:|
| **R** — package gone (relocated; banks through its successor) | 2 | 6 | 0.0% |
| **U** — no selected `.go` file differs | 77 | 11,756 | 16.8% |
| **T** — test files only, same test-function set | 14 | 425 | 0.6% |
| **P** — production files only | 32 | 1,371 | 2.0% |
| **B** — both, same test-function set | 37 | 2,430 | 3.5% |
| **F** — the test-function set moved (+279 / −22 in total) | 63 | 53,789 | 77.1% |
| **Total** | **225** | **69,777** | |

- **The largest test-function movers:**
  - `os` +76;
  - `internal/synctest` +22 / −1;
  - `math/big` +15 / −5;
  - `runtime` +15 / −3;
  - `net/http` +13 / −1;
  - `runtime/trace` +10;
  - `net` +9;
  - `testing` +8;
  - `bytes`, `io/fs` and `strings` +6 each.

  F is dominated by weight, not by change: `crypto/cipher` (27,272 verdicts) is F on +3 functions,
  with a 2-line production change.
- **The removed names** include relocations. Examples: `runtime`'s `TestAtoi`, which moves to
  `internal/runtime/strconv`, and `internal/trace`'s `TestOldtrace`. The rest are retirements
  upstream:
  - `crypto/elliptic` `TestP256CombinedMult`, with the hidden methods removed;
  - `crypto/x509` `TestLongChain` and `TestPathologicalChain`;
  - `math/big`'s six `TestFun*` and `TestSqr`;
  - others.

  Each is an H10 relocation-or-retirement reading and not a converter question.
- **The largest production deltas:**
  - `runtime` +4,470 / −2,222;
  - `internal/trace` +1,273 / −1,156;
  - `unicode` +1,411 / −21, the tables (a strict-compare row: NOTES Q5);
  - `os` +1,359 / −536;
  - `go/types` +987 / −590;
  - `math/big` +976 / −830;
  - `internal/syscall/windows` +801 / −119;
  - `crypto/tls` +678 / −492;
  - `runtime/trace` +666 / −30;
  - `net/http` +486 / −616.
- **The new-package closure** (`go list -deps -test`, both test variants):
  - every one of the 223 rows reaches `internal/runtime/gc`, `internal/runtime/strconv` and
    `internal/trace/tracev2`;
  - 24 rows reach `crypto/internal/fips140cache` (the crypto, x509 and net/http families);
  - 21 rows reach `internal/testhash` (every `hash/*`, and the `crypto` hashes and ciphers);
  - 17 rows reach `net/http/internal/httpcommon`;
  - 3 rows reach `internal/trace/internal/tracev1` (`internal/trace`, `runtime`, `runtime/trace`);
  - 2 rows reach `testing/synctest` (`encoding/json`, `net/http`);
  - 1 row reaches `internal/runtime/startlinetest` (`runtime`).

  All of these are corpus packages that H5 must convert in any case. None is optional for its rows.

### 2.6 Hand-own exposure (H6)

`handown-census.ps1`, run go1.24.13 → go1.25.12 at master `20279db5f6`:

| Class | Count |
|:--|--:|
| Marked files | **185** |
| touched-substantive (the H6 review list) | **58** |
| touched-trivial | 4 |
| untouched | 59 |
| no upstream counterpart | 64 |

**Substantive, by upstream package:**
- `runtime` 24;
- `syscall` 4 and `sync` 4;
- `internal/syscall/windows` 3;
- `vendor/golang.org/x/net/route` 2 (darwin);
- 2 each: `os`, `net`, `internal/syscall/windows/registry`, `internal/poll`;
- 1 each: `unique`, `testing`, `reflect`, `iter`, `internal/testenv`, `internal/synctest`,
  `internal/runtime/syscall`, `internal/godebug`, `internal/chacha8rand`, `internal/abi`, and three
  `crypto/internal/fips140/*` files.

**Against NOTES §3.10** (184 marked, 58 / 4 / 58 / 64, taken at `a1f133c3a9` against go1.25.13): the
substantive total agrees at 58, but the set differs by name.
- The notes count `runtime` 25 "including `runtime/pprof`'s `pprof_impl.cs`". That file is not a
  tracked file at `20279db5f6`. `runtime/pprof` now holds `proflabel_impl.cs`, `symtab_impl.cs` and
  three `proto_*_impl.cs`, and none of them is substantive here.
- The darwin route copy counts as 2 files here (`sys_impl.cs`, `syscall_impl.cs`), where the notes
  had 1.
- The marked total moved by +1.

H6 is ⟲ and re-measures at .14 regardless. The point for scoping is that **H6 is about 26% larger than
hop B's** (46 substantive). It is the hop's largest converter-side review item, and **33 of the 58 are
`runtime`, `sync`, `syscall` and `testing`, which every row's test host runs on.**

---

## 3. Scoping options

### 3.1 The fixed part, common to every shape

- **The corpus is one unit.** Every row's closure runs through 1.25's `runtime` and its three new
  dependencies (§2.5), and through the hand-owned `testing` host. So H1–H9 run in full in every shape:
  - toolchain;
  - pin;
  - census;
  - converter work;
  - seeded reconvert;
  - H6 over all 58;
  - compile parity on three OSes;
  - re-emission;
  - behavioral goldens.
- **Staging can only defer H10.** That is the re-bank and triage of individual rows, plus the second
  H11/H12 ritual it implies.
- **Owner hands needed before H1 in every shape:**
  - **B0**, the go1.25.14 toolchain on every box. Not cached here.
  - **B4**, a qualified many-core host for `crypto/tls`'s BoGo wall. This box is banned from TLS rows
    by owner order.

### 3.2 The shapes

Effort is stated in **lane-days, as estimates.** The basis:
- **Hop B**, tag to tag: 2026-09-07 → `nuget-1.24.13.1` on 2026-09-24, 16.4 days with six lanes. It
  published at **218 banked + 6 candidates** and reached 225/225 about a week later (around
  2026-10-01). Calendar × lanes gives an **upper bound** of about 100 lane-days to its first release
  and about 150 to 225/225. That is not logged effort: runbook §7 lesson 3 measured 59% of fleet
  output as protocol work.
- **The PLAN sizing table** (`PLAN-corpus-upgrade.md` §6) puts hop C at 3–5 gate cycles and 2 full
  sweeps, against hop B's 5–8 and 2.
- **What makes hop C cheaper:** no language delta.
- **What makes it dearer:** H6 is about 26% larger (§2.6), and 63 rows carrying 77% of the verdicts
  moved their test sets (§2.5).

| Shape | What ships first | First release, ESTIMATE | Total to full roster, ESTIMATE | Rulings needed |
|:--|:--|:--|:--|:--|
| **A — full hop** | About 232/232 (223 surviving rows plus 9 new candidates), then publish | 75–120 lane-days; 2.5–4 weeks at 7 lanes | the same | none new |
| **B — staged** | Stage 1: H1–H9 in full, H10 on **198 rows** (49,735 verdicts), publish with an honest roster. Stage 2: the **25 reserved rows** (20,036 verdicts) and the 9 candidates, then a second publish | 55–85 lane-days; 2–3 weeks | 85–140 lane-days: two rituals, two proof-page cycles | P2 / OQ-10 (absolute rows ≥ prior: 198 < 225); H11 monotonicity at two publishes; how an unbanked row's page reads (lesson 77) |
| **C — full hop, named demotions** (hop B's own shape) | Every row that banks by the H10 close; at most about 5 rows demoted **by name**, each with a costed recon-basis row (lessons 34, 51); publish | 65–105 lane-days; 2–3.5 weeks | +10–30 lane-days for the demoted rows | P2 / OQ-10, scoped to the named list |

**The reserved set** (B's stage 2, and C's pool of candidate demotions) is 25 rows. It is the 17 rows
with a substantive hand-own in their own package, plus the two `testing/synctest` consumers, the trace
and runtime family, `crypto/tls` (its host) and `unicode` (strict tables):
- `runtime`, `runtime/debug`, `runtime/pprof`, `runtime/trace`, `internal/trace`;
- `os`, `net`, `net/http`, `crypto/tls`, `encoding/json`;
- `sync`, `syscall`, `testing`, `reflect`, `unique`, `iter`, `unicode`;
- `internal/{abi,chacha8rand,godebug,poll,synctest,testenv}`;
- `internal/syscall/windows`, `internal/syscall/windows/registry`.

**Risks, by shape:**
- **A:**
  - The release waits on the slowest row. `runtime` has 10,819 verdicts, a full windows row read in
    about 6,545 s at Release TC0, 24 substantive hand-owns, and +15 / −3 test functions. `crypto/tls`
    waits on its BoGo host (B4); in hop B that host held the close for about 15 h. `os` has +76 test
    functions.
  - Hop B's own record says the last rows cost the most: the close took a further week after its
    release.
  - On a closed series, every week spent at 1.25 is a week not spent toward hop D.
- **B:**
  - It saves less than its 198/25 split suggests. H6 over `runtime`, `sync`, `testing` and `syscall`
    (33 of the 58) is on stage 1's critical path, because every stage-1 row runs on them. Stage 2
    defers only the reserved rows' H10 triage and re-bank.
  - It carries two release rituals.
  - It needs the largest ruling: a published roster below the prior count, on 198 rows.
  - Stage 2 can be overtaken by hop D.
- **C:**
  - It needs a demotion cap ruled in advance, so that "a few" does not become stage 2 by drift.
  - Each demotion owes a costed recon-basis row and named disclosures (lessons 48, 51).

### 3.3 Recommendation

**Shape C.** Rule the demotion cap and the P2/OQ-10 exception now, with the reserved list in §3.2 as
the only pool demotions may come from. Plan for A: run every row. Publish at the H10 close whatever has
banked, if the cap holds.

C keeps one ritual and one proof-page cycle. It is the shape hop B actually ran, publishing at
218 + 6. It puts the slowest rows (`runtime`, `crypto/tls`, `os`) off the release's critical path
without moving the compile and H6 work, which no shape can move. Choose B only if an external date
needs a 1.25 corpus earlier than C's lower bound. The record cannot see such a date; it is the owner's
to name.

---

## 4. What the 1.24 hop's lessons change here (runbook §7)

The lessons that change this hop's plan, rather than merely apply to it:

- **Lesson 13, the testing host goes first, sized by receiver-typed sites.** Measured here: the new
  `Attr` and `Output` members have 2 receiver sites, both in the `testing` row. The host's real bill
  is the `AllocsPerRun` parallel panic, with Go's exact counter accounting, and the `testingSynctestTest`
  hook. Two banked rows need that hook (`net/http`, `encoding/json`). Land both at H4. They are
  stage-1 work in every shape.
- **Lessons 2 and 43, instruments derive the release and are red-proved.** At `20279db5f6`, three of
  NOTES' four named instruments still hard-code 1.24:
  - `src/run-h10-recon.ps1:841–843`, the `go1\.24\.13` pin denials (NOTES A7);
  - `src/check-h6-completeness.ps1:76` (`AUDIT-h6-handown-go124.md`; NOTES A8);
  - `docs/phase4/hopA-inputs/shardmap.py:94` (`population-go1.24.13.txt`; NOTES A9).

  `src/check-roster-format.ps1:906` now derives the population path from the pin, so NOTES A13 reads
  as done at master. **A7, A8 and A9 are owed:** A8 before H6, and A7 and A9 before H10's recon leg.
- **Lesson 47, toolchain-adjacent pins land before the outgoing final release, never inside the hop.**
  For hop C that is the x/tools and x/mod step (NOTES A12, Q9). It is gated by H1 step 2, so it stays
  H1's own commit with its own CNR. It cannot land early, and it is planned as an H1 item.
- **Lesson 52, the population is committed at H3 and checked by an independent enumeration.** The
  go1.25.14 population and relocations are already on master (C2, 2026-09-30). **This record is the
  independent check** for the two banked relocations and `runtime → internal/runtime/strconv` (§1).
  H3 still owes the enumeration at a .14 toolchain.
- **Lessons 34, 48 and 51: demote by name, with a costed recon-basis row.** These lessons are what
  make shape C honest. §3.2's reserved list is the pool.
- **Lessons 30 and 64, take a linux reading early.** NOTES §3.9's non-Windows linkname
  (`internal/syscall/unix.Utimensat`) surfaces on linux first. This record is windows-only and did not
  re-measure linknames.
- **Lesson 40, qualify the BoGo host by measurement at Readiness.** This box cannot be that host.
  B4 is open.
- **Lesson 36, the recon's per-row TSVs are the shard basis.** This record's row census is a
  change census, not a wall census. The H10 recon leg still owes the walls.
- **Lessons 19 and 60, declared-set guards fail by design at the next hop.** NOTES §3.9 names them
  (`registerUniqueMapCleanup`, `mapKeyError`). Fix them in the same commit as the converter change,
  never as a regression hunt.
- **Lessons 4 and 48, before the pin moves, commit every unbanked reading and freeze the proof
  pages.** No change from hop B. It is listed because shapes B and C publish rosters with unbanked
  rows, and these lessons are what keep those rows honest.

---

## 5. What this record does NOT establish

- **No corpus conversion or compile was run at 1.25.** The compile ladder, NOTES C1's seeded
  `-stdlib` scratch conversion, is the next recon step and the heavy one. Every bill here is an
  upstream-delta reading.
- **Nothing was measured at go1.25.14.** .13 and .14 change `net/http`, `crypto/tls`, `os` and others
  (§1).
- **Linux and darwin were not measured** beyond the `go list std` census. That includes the linkname
  delta and the per-OS row selections.
- **The effort figures are estimates.** Their basis is hop B's calendar and lane count, and PLAN's
  gate-cycle table, not logged effort.
- **Whether golib already emulates** the 1.25 runtime behaviours (`[recovered, repanicked]`,
  container `GOMAXPROCS`, concurrent cleanups) is located (§2.4) but not tested.

---

## Appendix — reproducing this

The scripts are kept on the i9 lane's logs share; each is read-only.
1. `go list std` per OS on both toolchains.
2. The per-row comparison over each toolchain's `go list` file selection (corpus tags).
3. The `go doc -all` declaration diff.
4. `src/handown-census.ps1 -FromGoRoot <1.24.13> -ToGoRoot <1.25.x>`.
5. The new-package closure: `go list -deps -test` per row, intersected with the added set.
6. The new-API name census over the rows' selected test files.
7. `go build` and `go vet` of `src/go2cs` under the target toolchain, output to a scratch path, with a
   planted vet control.

<!-- Measured 2026-10-11 on the i9 (windows/amd64) at master 20279db5f6, worktree on branch
     claude/i9-go125-recon. FROM go1.24.13 (VERSION 2026-02-03); TO go1.25.12 (VERSION 2026-07-01), the
     module-cache toolchain, read-only. Release history and notes: https://go.dev/doc/devel/release and
     https://go.dev/doc/go1.25, read 2026-10-11. Hop B figures: NOTES-next-hop.md §4 and F2; ValidatedTestPackages.md
     "The 230 at the H10 close" (218 banked + 6 candidates + 6 exclusion rows = 230) and the 2026-10-01 header.
     Lessons: GoCorpusMigration.md §7 (3, 2/43, 13, 19/60, 30/64, 34, 36, 40, 47, 48, 51, 52, 77). PLAN-corpus-upgrade.md
     §6 sizing table; §8 OQ-10 and the P1-P5 parity gates. -->
