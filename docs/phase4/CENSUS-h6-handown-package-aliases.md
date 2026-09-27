# CENSUS — hand-own package aliases against the 1.24.13 release (H6, release-agnostic half)
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

**Point-in-time record.** Taken at master `44f858717` (train 45 landed) on 2026-09-08 by lane G, per
COORD's dispatch in the train-45 landing follow-up. Amend with dated blocks; never execute from it.

## What this measures, and what it deliberately does not

For every `[module: GoManualConversion]` hand-own in the corpus, every `using … _package;` alias is
mapped back to a Go import path and classified against the **go1.24.13** GOROOT: does that package
still exist, has it MOVED, or is it ABSENT. It needs **GOROOT and master only** — no build, no .NET,
no converter run — which is why it is the *release-agnostic* half.

It does **not** reach the EMISSION half: whether a given C# namespace still resolves after the hop
depends on what the corpus emits, not on what GOROOT contains. See *Limits* below, which records a
mechanism this census measured to be FALSE rather than leaving it to be assumed.

## Population

| | |
|---|---|
| marked files (line-anchored `^\s*\[module:\s*(go\.)?GoManualConversion\]`) | **145** |
| the same grep UNANCHORED | 225 — 80 false, the documented trap |
| `using … _package;` aliases across them | **94** |

The 145 matches lane R's independently-derived seed figure.

## Verdicts

| verdict | count |
|---|---|
| EXISTS at 1.24.13 | **92** |
| MOVED | **2** |
| ABSENT | **0** |

### The MOVED rows — the whole actionable set

| file | alias target | 1.24.13 |
|---|---|---|
| `runtime/mfinal.cs` | `runtime/internal/sys` | → `internal/runtime/sys` |
| `runtime/runtime2.cs` | `runtime/internal/sys` | → `internal/runtime/sys` |

**These are exactly the two files lane R identified by BUILDING the re-based ladder** (16 root errors,
identical on three flavours). Two instruments from opposite directions — a corpus build, and this
census with no compiler in it — reaching the same two files and the same alias.

**So the rung order after `runtime` is EMPTY**: 143 of the 145 marked files carry no moved or absent
package alias.

Free H3 datum: `runtime/internal/math` also moved (→ `internal/runtime/math`). No hand-own aliases
it, so it is not a rung, but it is a second member of the same relocation.

## Limits — including a mechanism measured FALSE

**The bare `using <ns>;` form is counted but does not appear above.** 403 of them exist across the
145 files; **zero name a directory lost at 1.24**.

⚠ **The tempting explanation for R's second alias is FALSE and this census says so rather than
guessing.** `runtime/internal` survives at 1.24: it loses `math` and `sys` but keeps
`startlinetest` and `wasitest`, **both still in `go list std`, and the corpus converts all four
today**. So the C# namespace does not go empty, and the `CS0246` R observes on `using runtime.@internal;`
is **not** emptiness. Its mechanism belongs to the emission half.

## Instrument, and the four defects found in it

Scripts: `g-h6census.sh`, `g-nstogo.sh`, `g-movedto.sh` (lane G scratchpad). Every defect below was

**Read layer, stated because a census that does not name one invites the mismatch.** Every
content read is a BLOB read — `git show <ref>:<path>` for each file, and `git grep <ref>` for
the marked-file list. No worktree file is opened, so there is no CRLF-versus-LF layer to get
wrong: the numbers here are properties of the commit `44f858717`, not of anybody's checkout.
(Prompted by lane R's `68c3b733f`, where comparing CRLF working files against LF blobs made
three files read as belonging to neither tree.)
found by **reading the rows**; the totals looked clean at each stage.

1. `/MOVED/` matches inside `REMOVED` — totals read "MOVED 32 / REMOVED 0" over rows that plainly
   said REMOVED. Cured by renaming the third verdict **ABSENT**, so the tokens are substring-disjoint;
   not by a smarter grep.
2. The corpus root namespace `go.` was not stripped, so `go.internal.bisect_package` mapped to
   `go/internal/bisect` and read ABSENT — **29 rows** of a bogus 62/32 split.
3. A naive `.`→`/` breaks a vendored domain: `…vendor.golang.org.x.sys.cpu_package` became
   `vendor/golang/org/x/sys/cpu`, a false ABSENT for a package present at BOTH releases. Cured with a
   resolver (naive path, then re-join each adjacent pair with a dot, take the first that exists).
4. The MOVED resolver first answered `cmd/internal/sys` — a real package and the wrong one — by taking
   the first same-basename directory. It now ranks candidates on shared path components, excludes
   toolchain trees, and prints `AMBIGUOUS{…}` rather than picking one.

**Controls**: the mapper reports `runtime/internal/sys` → `internal/runtime/sys` (R's own member), and
`crypto/internal/alias` → `crypto/internal/fips140/alias` (matching `claude/g-weak-rekey`'s subject).
Re-running the whole census reproduces 145 / 94 / 92-2-0 exactly.

## Full table

```
FILE                                                     ALIAS-TARGET                         VERDICT
crypto/internal/boring/bcache/cache.cs                   sync/atomic                          EXISTS
crypto/subtle/xor_generic.cs                             runtime                              EXISTS
debug/pe/symbol_impl.cs                                  internal/saferio                     EXISTS
debug/pe/symbol_impl.cs                                  encoding/binary                      EXISTS
debug/pe/symbol_impl.cs                                  errors                               EXISTS
debug/pe/symbol_impl.cs                                  fmt                                  EXISTS
debug/pe/symbol_impl.cs                                  io                                   EXISTS
internal/concurrent/hashtriemap_whitebox.cs              sync/atomic                          EXISTS
internal/godebug/godebug.cs                              internal/bisect                      EXISTS
internal/godebug/godebug.cs                              internal/godebugs                    EXISTS
internal/syscall/unix/darwin/net_darwin_impl.cs          internal/abi                         EXISTS
internal/syscall/unix/darwin/net_darwin_impl.cs          syscall                              EXISTS
internal/syscall/unix/darwin/user_darwin_impl.cs         internal/abi                         EXISTS
internal/syscall/unix/darwin/user_darwin_impl.cs         syscall                              EXISTS
internal/syscall/unix/linux/net_linux_impl.cs            syscall                              EXISTS
internal/syscall/windows/exec_windows_test.cs            fmt                                  EXISTS
internal/syscall/windows/exec_windows_test.cs            os/exec                              EXISTS
internal/syscall/windows/exec_windows_test.cs            io                                   EXISTS
internal/syscall/windows/exec_windows_test.cs            os                                   EXISTS
internal/syscall/windows/exec_windows_test.cs            syscall                              EXISTS
internal/syscall/windows/exec_windows_test.cs            testing                              EXISTS
internal/syscall/windows/registry/registry_test.cs       bytes                                EXISTS
internal/syscall/windows/registry/registry_test.cs       crypto/rand                          EXISTS
internal/syscall/windows/registry/registry_test.cs       internal/syscall/windows/registry    EXISTS
internal/syscall/windows/registry/registry_test.cs       os                                   EXISTS
internal/syscall/windows/registry/registry_test.cs       syscall                              EXISTS
internal/syscall/windows/registry/registry_test.cs       testing                              EXISTS
internal/syscall/windows/registry/windows/value.cs       errors                               EXISTS
internal/syscall/windows/registry/windows/value.cs       syscall                              EXISTS
internal/syscall/windows/registry/windows/value.cs       unicode/utf16                        EXISTS
internal/syscall/windows/windows/net_windows_impl.cs     syscall                              EXISTS
internal/syscall/windows/windows/syscall_windows_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_module_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_version_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs syscall                              EXISTS
net/windows/interface_windows_impl.cs                    internal/syscall/windows             EXISTS
net/windows/interface_windows_impl.cs                    os                                   EXISTS
net/windows/interface_windows_impl.cs                    syscall                              EXISTS
net/windows/lookup_windows.cs                            internal/syscall/windows             EXISTS
net/windows/lookup_windows.cs                            context                              EXISTS
net/windows/lookup_windows.cs                            os                                   EXISTS
net/windows/lookup_windows.cs                            syscall                              EXISTS
net/windows/lookup_windows.cs                            time                                 EXISTS
os/darwin/dir_darwin_impl.cs                             internal/poll                        EXISTS
os/darwin/dir_darwin_impl.cs                             io/fs                                EXISTS
os/darwin/dir_darwin_impl.cs                             sync/atomic                          EXISTS
os/darwin/dir_darwin_impl.cs                             syscall                              EXISTS
os/linux/wait_waitid.cs                                  syscall                              EXISTS
os/user/windows/lookup_windows_impl.cs                   internal/syscall/windows             EXISTS
os/user/windows/lookup_windows_impl.cs                   fmt                                  EXISTS
os/user/windows/lookup_windows_impl.cs                   syscall                              EXISTS
os/windows/dir_windows_impl.cs                           internal/syscall/windows             EXISTS
os/windows/dir_windows_impl.cs                           io/fs                                EXISTS
os/windows/dir_windows_impl.cs                           syscall                              EXISTS
os/windows/file_windows_impl.cs                          internal/syscall/windows             EXISTS
os/windows/file_windows_impl.cs                          syscall                              EXISTS
reflect/makefunc_impl.cs                                 internal/abi                         EXISTS
reflect/value_impl.cs                                    internal/abi                         EXISTS
reflect/value_impl.cs                                    strconv                              EXISTS
runtime/darwin/signal_posix_darwin_impl.cs               internal/runtime/atomic              EXISTS
runtime/debug/stubs_impl.cs                              time                                 EXISTS
runtime/linux/signal_posix_impl.cs                       internal/runtime/atomic              EXISTS
runtime/mem_persistent_impl.cs                           internal/goarch                      EXISTS
runtime/mem_persistent_impl.cs                           internal/runtime/atomic              EXISTS
runtime/mfinal.cs                                        internal/abi                         EXISTS
runtime/mfinal.cs                                        internal/goarch                      EXISTS
runtime/mfinal.cs                                        internal/runtime/atomic              EXISTS
runtime/mfinal.cs                                        runtime/internal/sys                 MOVED -> internal/runtime/sys
runtime/mranges_impl.cs                                  internal/goarch                      EXISTS
runtime/pprof/pprof_impl.cs                              internal/profilerecord               EXISTS
runtime/pprof/symtab_impl.cs                             runtime                              EXISTS
runtime/runtime2.cs                                      internal/abi                         EXISTS
runtime/runtime2.cs                                      internal/chacha8rand                 EXISTS
runtime/runtime2.cs                                      internal/goarch                      EXISTS
runtime/runtime2.cs                                      internal/runtime/atomic              EXISTS
runtime/runtime2.cs                                      runtime/internal/sys                 MOVED -> internal/runtime/sys
sync/once.cs                                             sync/atomic                          EXISTS
sync/poolqueue.cs                                        sync/atomic                          EXISTS
syscall/darwin/exec_libc2_impl.cs                        errors                               EXISTS
syscall/darwin/sockaddr_darwin_impl.cs                   internal/abi                         EXISTS
syscall/linux/exec_unix.cs                               internal/bytealg                     EXISTS
syscall/linux/exec_unix.cs                               errors                               EXISTS
syscall/windows/dll_windows.cs                           internal/syscall/windows/sysdll      EXISTS
syscall/windows/exec_windows.cs                          internal/bytealg                     EXISTS
syscall/windows/exec_windows.cs                          unicode/utf16                        EXISTS
syscall/windows/syscall_windows_impl.cs                  errors                               EXISTS
unique/clone.cs                                          internal/abi                         EXISTS
unique/clone.cs                                          internal/stringslite                 EXISTS
vendor/golang.org/x/crypto/sha3/xor.cs                   encoding/binary                      EXISTS
vendor/golang.org/x/crypto/sha3/xor.cs                   vendor/golang.org/x/sys/cpu          EXISTS
vendor/golang.org/x/net/route/darwin/sys_impl.cs         syscall                              EXISTS
```

---

## 2026-09-08 — AMENDMENT: THE CLASS THIS CENSUS COULD NOT SEE (frozen metadata meeting 1.24)

Routed by COORD (`ec1c08d54`) off R's re-based ladder, whose one non-`sync` residual is
`internal/weak/package_info.cs` CS0426 on `ΔMapType`.

**Why the census above missed it, structurally.** That census read the *marked files'* `using`
aliases (143 clean). These are UNMARKED metadata files inside packages where every production file
is marked, so `unmarkedFileCount == 0`, the driver `continue`s before `writeProjectFile`, and
`package_info.cs` / `.csproj` / `README.md` are never re-emitted. The population was bounded by the
instrument: a marker-keyed census cannot observe a file that carries no marker BY DESIGN.

### The class, re-derived at master (not taken from the dispatch)

Every PRODUCTION file marked in all four; the remaining `.cs` are test emission plus metadata.

| package | marked production files | alias block | affected |
|---|---|---|---|
| `crypto/internal/boring/bcache` | `cache.cs` | **EMPTY** | no |
| `internal/godebug` | `godebug.cs` | **EMPTY** | no |
| `internal/concurrent` | `hashtriemap.cs`, `hashtriemap_whitebox.cs` | 8 `abi` | **yes** |
| `internal/weak` | `pointer.cs` | 8 `abi` + `runtime.ΔError` | **yes** |

**The affected subset is 2 of 4, and it is exactly the two that RELOCATE.** The two with empty
alias blocks cannot break this way at all.

### Exactly one of the eight aliases breaks

`ArrayType`, `ChanDir`, `FuncType`, `InterfaceType`, `Kind`, `Name`, `StructType` all survive at
1.24.13. `MapType` is **ABSENT**, replaced by `OldMapType`/`SwissMapType`, with **no `type MapType =`
alias anywhere under any build tag** (checked). That is R's CS0426.

**A second row the alias block does not carry:** `internal.concurrent.tests.csproj` references
`runtime/internal/math`, which moves to `internal/runtime/math` at 1.24 (`runtime/internal/sys`
likewise; `startlinetest` and `wasitest` REMAIN under `runtime/internal/`, so the namespace does not
empty — the mechanism measured FALSE earlier in this record). `golib` is flagged by the raw
predicate and excluded BY NAME: it is hand-written and in `go list std` at no release, the
irrelevant hit `reconvert-deletions.ps1`'s own header documents.

### The finding that changes the remedy

Both affected members relocate, and both are RENAMES rather than deletions — principal file names
carried across, both successors still importing `internal/abi`:

- `internal/weak` → **`weak`** (public)
- `internal/concurrent` → **`internal/sync`**

**So a hand edit to the `ΔMapType` line would patch metadata for a package that does not exist at
1.24.**

### The orphan, and the deletion pass will not clear it

`reconvert-deletions.ps1` tests PROTECTED — carries the line-anchored marker — **FIRST, before
anything else**. Both old directories carry it, so they survive the hop as orphaned directories
still compiling frozen metadata against a moved `abi`. **The marker that protects a hand-own from
being clobbered also protects an ORPHANED hand-own from being cleaned up.** Their removal is
explicit work, not something the deletion pass will do.

### The hand-written code needs no edit

`abi`-alias uses across `pointer.cs`, `hashtriemap.cs`, `hashtriemap_whitebox.cs`: **0, 0, 0**
(`MapType` likewise 0). The block is minted from the package's IMPORTS wholesale, not from usage —
so the break is metadata that nothing consumes, and re-minting it touches no hand-written line.

### The destination is already pinned, and has a committed dependent

`linknameOperations.go:504-521` re-keyed both rows to `weak.*` for 1.24 and its `reason` strings
name **`weak/pointer.cs`**, the post-hop path, stating that relocating it "is H6/H9 work and these
rows depend on it".

### Sizing

- **(A) hand-edit the alias per file — REJECTED by measurement.** Wrong for both affected members
  (they relocate), leaves the orphan standing, and does not touch the csproj row.
- **(B) un-freeze the metadata for the class.** Fixes the alias block and the csproj references for
  all four automatically and at every future release — but does NOT move the marked file, so it
  would emit metadata at the new path while the hand-own stays at the old one.
- **(C) relocate the two directories, metadata re-minted at the new path.** The actual remedy.
  **B and C compose:** C moves the file, B keeps it correct at the next hop.

**A collision to name:** `internal/sync` at 1.24 is also where `sync`'s mutex implementation moves
(`mutex.go`, `runtime.go`) — C1/seat B's subject. The hand-own displaces `hashtriemap.go`, which is
present at the new path with its single `HashTrieMap` declaration, so there is no CS0102 against
those two files; but the directory is shared work and should be sequenced with C1.

### Limits

- **No 1.24 conversion was run.** The alias-block prediction is derived from the pinned 1.24.13
  sources and the frozen files, not from an emission. A conversion is what would settle it.
- csproj extraction covered production AND tests csproj per package with the row count asserted
  non-zero, because an empty extraction would have read as "no dangling references".
- The relocation claims are `go list std` membership plus file-name carry-over, not a content diff.

## Cross-check block -- C2, 2026-09-13

Read at this record's own ref **44f858717** and at **bd1d26faf** (current master as supplied to this lane),
with three further refs — **f4ced674d**, **fd09034f53**, **9355669f8** — used only to reconcile the
marked-file figure against lane R's. Read layer **BLOB ONLY**: `git show <ref>:<path>`, `git grep <ref>`,
`git ls-tree`, `git rev-parse`, `git cat-file`, `git merge-base`, `git rev-list`, `git log`,
`git diff <refA> <refB>`. No worktree file was opened, so no CRLF-vs-LF layer entered; nothing was built,
no converter and no `dotnet` were run (absent on this box), no file in the repo was created, edited or
deleted, and no history-modifying git verb was issued. Ancestry asserted, not assumed:
`git merge-base --is-ancestor 44f858717 bd1d26faf` → exit 0; `bd1d26faf` has one parent, `9355669f8`.

Classifier for every EXISTS / MOVED / ABSENT verdict in this block: the **real go1.24.13 toolchain**, a
module-cache SDK on linux fetched by `GOTOOLCHAIN=go1.24.13`, proved by its bare version line
**`go version go1.24.13 linux/amd64`** — the pin is stated that way deliberately, since a `go env GOROOT`
spelling is an argument and never a postable value. Its `go list std` returns **345** lines (asserted
non-empty before any verdict). `go list std` membership is the primary predicate; the SDK `src/` directory
test (directory exists **and** holds ≥1 `.go` file) is a recorded fallback, and every row records which
instrument answered it.

**Navigation, because this block is 591 lines against the record's 283 and that ratio deserves a warning
rather than a surprise.** §2 is the whole claim ledger and nothing below it is new information — §3, §4 and
§7 are its derivations. A reader who wants only what CHANGES should read **§5.1** (the denominator, the one
finding that alters how the record's reproducibility claim reads), **§5.4** and **§5.5** (the two refutations),
and **§10**. The length is deliberate: this box is ephemeral, so evidence not written into the record is
evidence lost, and the repo's own rule is to move provenance rather than delete it.

---

### 1. Where C2's instrument differs from G's

| Axis | G (this record) | C2 (this block) |
|---|---|---|
| Verdict predicate | GOROOT **directory** scans | `go list std` **membership**, SDK `src/` dir test as fallback |
| Instrument location | three off-git scripts in lane G's scratchpad (`g-h6census.sh`, `g-nstogo.sh`, `g-movedto.sh`) | predicate rebuilt from scratch and stated in full below; nothing read out of G's scripts |
| Alias extractor | unknown source; output only | independently written, deliberately placing **no** constraint on alias-name characters |
| Population | src/core scope, one ref | re-derived by C2's own line-anchored `git grep` at five refs |
| Read layer | blob | blob (same layer, so this is not an independence axis) |
| Agent / box | lane G | second lane, second box, plus an adversarial second derivation of C2's own numbers |

Three evidence classes, kept apart on purpose:

- **CONFIRMED BY A SECOND INSTRUMENT** — the verdict half. `go list std` membership is a different
  predicate from a GOROOT directory scan, and for the two MOVED files lane R's build is a third.
- **RE-DERIVED BY AN INDEPENDENT PREDICATE AT THE SAME READ LAYER** — the population half (145, 225/80,
  the alias count, the bare-using count). Both sides are blob `git grep`; C2's predicate was written
  independently and its regex *flavour* controlled (PCRE vs ERE/POSIX gave a byte-identical 145-path
  list, `diff` exit 0), but this is a second derivation, not a second instrument.
- **RE-READ ONLY** — locations and wordings quoted out of other documents (`REHEARSAL-h5-go124.md:169`,
  `:157`, `CENSUS-bucket3-darwin.md:403`, the skill citations). Where a re-read number was also re-derived
  by running its stated instrument at its stated ref, that is said explicitly; where it was not, it is
  marked NOT MEASURED.

---

### 2. Claim ledger

| Record claim | Record's figure | C2 @ 44f858717 | C2 @ bd1d26faf | Verdict | Class |
|---|---|---|---|---|---|
| Population: marked files, anchored `^\s*\[module:\s*(go\.)?GoManualConversion\]` over `src/core/**` | 145 | **145** | **146** | CONFIRMED at ref, +1 drift | re-derived |
| Population: same grep UNANCHORED | 225, "80 false" | **225**, gap **80** | **226**, gap **80** | CONFIRMED, gap invariant | re-derived |
| Population: `using … _package;` aliases across them | 94 | **141** raw / **94** plain-ASCII subset | 141 / 94 | REFINED — denominator | re-derived |
| Population: alias-bearing files | 48 | **66** | **66** | REFINED | re-derived |
| Verdicts | EXISTS 92 / MOVED 2 / ABSENT 0 | **92 / 2 / 0** over the 94; **139 / 2 / 0** over all 141 | 92 / 2 / 0 over the 94 | MOVED & ABSENT CONFIRMED; EXISTS and denominator REFINED | second instrument |
| MOVED set = `runtime/mfinal.cs` + `runtime/runtime2.cs`, `runtime/internal/sys` → `internal/runtime/sys` | 2 files | **2** files, **1** package | unchanged; both blobs byte-identical across the range | CONFIRMED | second instrument |
| "the rung order after `runtime` is EMPTY: 143 of the 145" | 143 / 145 | **143 / 145** | **144 / 146** | conclusion CONFIRMED, arithmetic drifts | second instrument |
| Limits: bare `using <ns>;` across the marked files | 403 | **403** (admission-dependent — §5.3) | **404** | CONFIRMED with a boundary refinement | re-derived |
| Limits: "zero name a directory lost at 1.24" | zero | **zero** | **zero** | CONFIRMED | second instrument |
| `runtime/internal` survives: keeps `startlinetest` + `wasitest`, "both still in `go list std`" | both | **both IN `go list std`**; `math` and `sys` both NOT in `go list std` and NO_DIR | — | CONFIRMED | second instrument |
| "…and the corpus converts all four today" | four | corpus converts **three** | — | REFUTED — §5.4 | second instrument |
| Free datum: `runtime/internal/math` moved to `internal/runtime/math`, no hand-own aliases it | 0 aliases | **0** over all **141** rows, both spellings, `grep` exit 1 | **0** over all 146 files | CONFIRMED on a larger population | second instrument |
| "The 145 matches lane R's independently-derived seed figure." | match | R's published figure is **142** at f4ced674d; 145 at 44f858717 | — | REFUTED as worded — §5.5 | re-read **and** re-derived at R's ref |
| The published 94-row Full table | table | multiset byte-identical (`sort`+`diff` exit 0); raw unsorted `diff` exit **1** on a 10-row within-file permutation | same | CONFIRMED as a multiset — §5.6 | re-derived |

Every population was asserted non-empty before its verdict: 145/146 marked files, 80 unanchored-only
files, 141 alias rows, 403/404 bare usings, 94 scored rows, 345 `go list std` lines.

---

### 3. Confirmed by a second instrument

**The MOVED set is exactly two files, and the relocation is real at 1.24.13.**
`awk -F'\t' 'NR>1 && $4=="MOVED"'` over C2's 141-row table prints two rows and only two:
`runtime/mfinal.cs` and `runtime/runtime2.cs`, both aliasing `runtime.@internal.sys_package` →
`runtime/internal/sys` → `internal/runtime/sys`. Both halves checked directly:
`runtime/internal/sys` NOT in `go list std` **and** NO_DIR (both instruments agree it is gone);
`internal/runtime/sys` IN `go list std`. Destination uniqueness proved, not assumed:
`find $GOSDK/src -type d -name sys` printed four candidates with their `.go` counts —
`cmd/vendor/golang.org/x/sys` (0), `cmd/internal/sys` (3), `vendor/golang.org/x/sys` (0),
`internal/runtime/sys` (10) — so after excluding `cmd/` trees and empty dirs exactly one survives and no
AMBIGUOUS marker was emitted. The row is invariant under both tie-break policies tested
(`excl cmd/` and `excl cmd/ + vendor/`). Non-EXISTS rows at bd1d26faf, unfiltered: 2 rows,
DISTINCT_NONEXISTS_FILES=2, DISTINCT_NONEXISTS_PKGS=1 (`runtime`). At bd1d26faf both files still carry the
alias — `using sys = runtime.@internal.sys_package;` at `mfinal.cs:20` and `runtime2.cs:21` — and
`git diff --name-only 44f858717 bd1d26faf -- <file>` prints 0 lines for each, so neither blob changed in
the range at all.

**This confirmation is over a population 50% larger than the record's.** The 47 alias rows the record's
extractor could not see (§5.1) contain no additional MOVED or ABSENT row: the corrected denominator adds
and removes no actionable file.

**`runtime/internal` does not go empty.** `ls $GOROOT/src/runtime/internal/` prints exactly
`startlinetest` and `wasitest`; `grep -qxF` against `go list std` puts **both** in it, so the record's
wording "both still in `go list std`" is literally correct and the SDK-directory fallback was not needed.
`runtime/internal/math` and `runtime/internal/sys` are both NOT in `go list std` and NO_DIR. On the corpus
side, `startlinetest/func_amd64.cs` and `startlinetest/package_info.cs` both declare
`namespace go.runtime.@internal;` and `partial class startlinetest_package` (lines 6/8 and 55/58), and the
csproj includes them with `<Compile Include="*.cs" Exclude="package_info.cs" />` and **no** `Condition` on
any `Compile` item, so the namespace is populated for every target even though the sole Go source
(`GoFiles=[func_amd64.go]`) is arch-gated. R's CS0246 on `using runtime.@internal;` is therefore not
emptiness — but see §5.4 for the size of that margin.

**The free H3 datum holds on the full population.** `runtime/internal/math` NOT in `go list std` and
NO_DIR; `internal/runtime/math` IN `go list std`, and it appears in `ls -1 $GOSDK/src/internal/runtime/`
beside `atomic`, `exithook`, `maps`, `sys`, `syscall`. Aliases naming it: **zero** over all 141 rows for
both the old and the new spelling (`grep -c` = 0, `grep` exit 1 in both directions), and zero over all 146
marked files at bd1d26faf. The MOVED resolver reached `internal/runtime/math` while having considered the
decoy top-level `math` (`candidates seen: ['math','internal/runtime/math']`), which is the record's own
defect-#4 pattern firing on a different row and being handled.

---

### 4. Re-derived by an independent predicate at the same read layer

- **145 / 146 marked files.** `git grep -P -l '^\s*\[module:\s*(go\.)?GoManualConversion\]' <ref> -- 'src/core/**'`,
  exit captured before any pipe (RC=0 at both refs), count taken with `wc -l < file`. Extension breakdown
  of the 145: **145 `.cs`, 0 other**. Decomposition **101 `*_impl.cs` companions + 44 whole-file
  rewrites = 145** at 44f858717, **102 + 44 = 146** at bd1d26faf. The `*.cs` restriction R used is
  non-load-bearing at every ref tested: the `src/core/**` and `src/core/**/*.cs` path lists are
  byte-identical, `diff` exit 0.
- **225 / 226 unanchored, gap 80 at both refs.** The anchored set is a proven strict subset
  (reverse `comm` = 0 at both refs). The 80-file set is **byte-identical** across the two refs (`cmp -s`
  exit 0): zero added, zero removed.
- **141 alias rows over 66 files**, at both refs (unchanged by the drift). Extraction completeness proved
  rather than assumed: **146** using-lines across the marked files mention `_package`, of which **141**
  are alias directives; all **5** leftovers were printed in full and are legitimately outside the
  predicate — 2 `using static` of a package class, 2 aliases to a *member* not a package
  (`using ꓸꓸꓸValue = System.Span<go.reflect_package.ΔValue>;` and
  `global using itab = go.@internal.abi_package.ITab;`), and 1 comment-suffixed bare using
  (`using go.@internal.runtime;   // …`). None names a moved or absent package.
- **The 141 is complete across both layers, not just the `.cs` layer.** No multi-line alias exists: the
  only `=`-bearing using lines in the marked files that do not end in `;` are three C# using-*statements*
  in method bodies (`testing/PackageAncestry.cs`, `testing/TestHost.cs` ×2), printed in full.
  `git grep -n -E '<Using[^>]*_package' 44f858717 -- 'src/core'` returns **0** matches: the MSBuild
  `<Using Include=… Alias=…>` mechanism is used in src/core only for primitive type aliases (509 csprojs
  declaring `uint64`/`uint32`/`int8`/`any`/`complex128`/`GoInitAttribute` and the like) and carries zero
  package aliases.
- **141 and 403 are not inflated by commented-out or preprocessor-dead usings** — a check neither G nor
  C2's first pass ran. A comment-aware state machine returns **0** hits inside a `/* */` block for both
  populations; real inline `/* */` sites across the marked files total **149** lines (first 20 printed,
  the remainder dropped), none spanning a using; there is exactly **one** `#if` in all 145 files
  (`src/core/testing/TestHost.cs:1204`, `#if DEBUG`), and that file's highest bare-using hit is line 22.
- **The record's published 94-row table reproduces as a multiset** — see §5.6 for the exit code.

---

### 5. Refinements

#### 5.1 ⚠ The alias denominator is 141, not 94 — a fifth instrument defect, and it is the one that sets the record's denominator

C2's extractor, written to place **no** constraint on alias-name characters, finds **141** alias
directives over **66** files at 44f858717 and the same 141 / 66 at bd1d26faf. The record's table holds
**94** rows over **48** files. The record's set is a **strict subset**: `comm` in the
in-record-not-in-mine direction = **0** rows and **0** mapping disagreements, and **0** per-row verdict
disagreements across all 94 shared rows. The gap is 47 rows, and the mechanism was measured, not guessed —
classifying all 141 by alias-**name shape** gives a perfect 3×2 separation with zero exceptions either
way:

| Alias-name shape | Rows | In the record's table |
|---|---|---|
| plain ASCII identifier | **94** | all 94 |
| leading `@` (C# verbatim), e.g. `using @unsafe = unsafe_package;` | **34** | none |
| non-ASCII collision escape `Δ`, e.g. `using Δsyscall = syscall_package;` | **13** | none |

34 + 13 = 47, closing the gap exactly, and the plain-ASCII bucket counted alone is 94 — an exact match to
the record's total. The Δ tally by name: `Δruntime` ×5, `Δsync` ×3, `Δsyscall` ×2, `Δio` ×2, `Δwindows` ×1.
A **by-target** exclusion is ruled out by command: the record's table carries the Δ rows' targets under
plain alias names (`runtime` 2 rows, `syscall` 22, `io` 2, `internal/syscall/windows` 5), so only a
name-shape rule can explain the Δ half; for the 34 `@unsafe` rows a by-name exclusion of `unsafe` is
indistinguishable from the shape rule (the record's table has 0 rows mentioning `unsafe`, and `unsafe`
never appears under a plain alias name in the 141). One rule explains all 47.

Corrected verdict arithmetic, printed: `139 EXISTS / 2 MOVED / 0 ABSENT` over 141
(`uniq -c` on the verdict column; no ABSENT row was emitted). Import-path tally of the 47 dropped rows:
34 `unsafe`, 5 `runtime`, 3 `sync`, 2 `syscall`, 2 `io`, 1 `internal/syscall/windows` — **all EXISTS**,
which is why MOVED=2 / ABSENT=0 and the actionable set survive the correction untouched.

Two whole files vanish from the record's population because every alias in them is Δ-named:
`src/core/internal/poll/linux/fd_writev_unix.cs` (sole using, line 31, `using Δsyscall = syscall_package;`)
and `src/core/internal/poll/windows/fd_windows_impl.cs` (line 112, `using Δsyscall = go.syscall_package;`);
neither appears anywhere in the record's 48-file set (`grep` exit 1). Which of the remaining 18-file gap
(66 − 48) each route accounts for is **NOT MEASURED**.

Why this is structurally dangerous rather than cosmetic: a `@`-escaped alias name occurs precisely when
the Go package's name is a C# keyword (`unsafe`, `internal`, `string`, `base`, `fixed`, `object`, `event`,
`lock`, …) and a `Δ` prefix occurs on a go2cs collision escape. Alias-name shape is uncorrelated with
whether the target package moved at 1.24, so the same extractor would have been equally blind to a MOVED
or ABSENT package whose name happens to be a C# keyword. The clean actionable set is luck, not design.
The record's own "four defects found in it" section reads as exhaustive; this is a fifth, and the record's
"re-running the whole census reproduces 145 / 94 / 92-2-0 exactly" is a reproducibility claim about a
blind instrument — re-running it reproduces the blind spot. The fix is a widening of one character class.

**The record's RHS mapper is not implicated.** Zero mapping disagreements and zero verdict disagreements
over all 94 rows it did see. The defect is in the alias-name character class only.

#### 5.2 The `src/core/**` scope is arithmetically load-bearing, and the record's two Population rows are scoped by different commands

Anchored with **no** pathspec: **148** at 44f858717 and **149** at bd1d26faf, versus 145 / 146 scoped
(`cut -d/ -f1-2 | sort | uniq -c` on the 148: 145 `src/core` + 2 `src/tests` + 1
`src/reconvert-deletions.ps1`). The three out-of-core anchored hits, named exactly rather than by pattern:

1. **`src/reconvert-deletions.ps1`** — one anchored match, line 32, inside the script's own PowerShell
   block comment (opener line 1, closer line 198, located by an unfiltered grep for both delimiters over
   the whole 1032-line file), reading
   `    [module: GoManualConversion] marker, because nothing ever CONVERTS into golib and there is no` —
   i.e. the marker text quoted in the instrument's own `.DESCRIPTION` help. Not C#, not a hand-own. An
   **unanchored** grep over that one file returns **six** lines (9, 32, 82, 462, 481, 764), of which only
   line 32 is anchored; line 462 is the instrument's `$HandOwnMarkerPattern` and 764 its reason string.
2. **`src/tests/Behavioral/ManualConversionSiblingState/state.cs`**
3. **`src/tests/Behavioral/ManualConversionSiblingState/state.cs.target`** — a behavioral-fixture pair
   carrying a **genuine** anchored `[module: go.GoManualConversion]` at line 8 of each, in `namespace go` /
   `partial class main_package`, inside a 29-line fixture (siblings: `main.go`, `state.go`, `main.cs`,
   `main.cs.target`, `state.cs.auto`, `package_info.cs`, `go.mod`, `.csproj`, `go2cs.ico`). The two are the
   **same git blob**, `9e5c55c82c441db3fb96da9a69df022f8fe14c89`, so the "three hits" are two distinct
   contents. The pair is a golden (input == expected target), i.e. the behavioral suite asserts a marked
   file is re-emitted byte-identically — the clobber property the marker exists to protect. It must not be
   swept up as a stray marker.

None of the three lies under `src/core` (`git ls-tree -r --name-only <ref> -- src/core/` grep exit 1,
zero matches) and the scoped set is a strict subset (reverse `comm` = 0 at both refs). Scope widening is
also provably **inert for the alias half**: all three carry **0** alias lines, so 141 either way.

A trap for the next re-derivation: the record's **145** row is derived with no pathspec and then narrowed
to src/core, while its **225** row is derived with the src/core scope. Both numbers are correct *under the
src/core scope* — unanchored with no pathspec is **307** files at 44f858717 — but the record does not say
its two Population rows were taken by differently scoped commands, and a re-derivation that runs the
unanchored grep unscoped lands on 307 and reads it as a defect. (The extension breakdown printed beside
that 307 does not sum to 307, so it is **NOT MEASURED** here; the 307 total and the 225 scoped figure are.)

#### 5.3 The 403 is reproducible under three different predicates, which means 403 does not identify the predicate

Three independently written bare-using predicates all land on 403 at 44f858717 by three different
admission policies. The full candidate set is **405** lines, decomposing as:

| Component | Lines @ 44f858717 | Named |
|---|---|---|
| bare `using <ns>;` with `;` at end of line, `using static` excluded | **401** | — |
| `using static …;` admitted by a `[^=;]*` predicate | **2** | `internal/syscall/windows/registry/registry_test.cs:35` (`using static go.@internal.syscall.windows.registry_internal_test_package;`), `vendor/golang.org/x/net/route/darwin/sys_impl.cs:48` (`using static go.vendor.golang.org.x.net.route_package;`) |
| comment-suffixed bare usings, dropped by an end-of-line `;` anchor | **2** | `runtime/stubs_impl.cs` (`using go.@internal.runtime;   // atomic.Uint32's Store is an extension method …`), `sync/poolqueue.cs` (`using go.sync; // atomic.Uint64/Pointer's operations are extension methods …`) |

401 + 2 static = 403. 401 + 2 comment-suffixed = 403. 401 + both = 405, minus the 2 `using static` = 403.
So the record's 403 is correct, and **the strictest reading of its own wording is 401**. Note also that
the 2 `using static` targets are **classes**, not namespaces, so 2 of the 403 cannot be resolved against
GOROOT at all — classify before resolving wholesale.

#### 5.4 The corpus converts three of the four `runtime/internal` packages, not four: the safety margin is 1, not 2

`git ls-tree --name-only 44f858717:src/core/runtime/internal/` lists `math`, `sys`, `startlinetest`,
`wasitest`, but per-directory `.cs` counts are **math=5, sys=10, startlinetest=2, wasitest=0**.
`git ls-tree -r --name-only 44f858717:src/core/runtime/internal/wasitest` prints exactly one entry:
`go2cs.ico`. No `.cs`, no `package_info.cs`, no `.csproj` — hence no `wasitest_package` class and no
contribution to the namespace. This is not a conversion gap awaiting a reconvert; the root cause is in Go:
`go list -f '{{.GoFiles}} {{.TestGoFiles}} {{.XTestGoFiles}}' runtime/internal/wasitest` returns
`GoFiles=[] TestGoFiles=[] XTestGoFiles=[host_test.go nonblock_test.go tcpecho_test.go]`, and
`ls $GOSDK/src/runtime/internal/wasitest/*.go | grep -v '_test\.go$'` exits 1 — `wasitest` is
external-test-only at 1.24.13 and there is nothing for the converter to emit.

The load-bearing conclusion stands: `go.runtime.@internal` is non-empty, so R's CS0246 is not emptiness.
What is refuted is the **redundancy**. The record names two survivors and there is one,
`startlinetest_package` — a 2× overstatement of the margin on the exact line that tells R where the error
does *not* come from, and the surviving member's only Go source is the arch-gated `func_amd64.go`. If
`startlinetest` is ever dropped from the corpus or from std, emptiness becomes a live mechanism and the
record as written reads as still having a spare.

#### 5.5 "The 145 matches lane R's independently-derived seed figure" is REFUTED as worded: the 145 supersedes R's 142

R's published figure is **142**, not 145. `REHEARSAL-h5-go124.md` is headed "Lane R, 2026-09-07" and at
lines 165-172 states, explicitly "against the authoritative marked-file set at master f4ced674d (git grep
line-anchored over src/core/**/*.cs)": `marked files 142 / *_impl.cs companions 98 / whole-file rewrites 44`
(`:169`). Running **R's exact instrument at R's exact ref** reproduces it to the row: **142 = 98 + 44**.
So the two instruments agree perfectly and the numbers differ only because they were taken at different
refs. The gap is exactly three files, named by `comm`, with zero removals:

- `src/core/internal/syscall/windows/windows/zsyscall_windows_version_impl.cs`
- `src/core/syscall/windows/syscall_windows_callback_impl.cs`
- `src/core/time/sleep_impl.cs`

All three are `*_impl.cs` companions. The `*.cs` restriction is **not** the cause (non-load-bearing at
every ref). Searched for any R-attributed 145: `git grep -n 'seed figure' bd1d26faf` exits 1 — the phrase
does not occur in the tree — and the only R/REHEARSAL marked-file row in the tree is the 142.

The record's substantive point survives: at a common ref the two derivations are identical, so two
instruments from opposite directions do converge. Only the sentence is wrong.

**The stable figure to quote is the whole-file-rewrite half, not the total.** One instrument, five refs:

| Ref | Date/label | total | `*_impl.cs` | whole-file |
|---|---|---|---|---|
| f4ced674d | R's ref, 2026-09-07 | 142 | 98 | **44** |
| fd09034f53 | `CENSUS-bucket3-darwin.md:403`'s ref | 142 | 98 | **44** |
| 44f858717 | this record's ref | 145 | 101 | **44** |
| 9355669f8 | 2026-09-12 | 146 | 102 | **44** |
| bd1d26faf | current master | 146 | 102 | **44** |

Every increment is an `*_impl.cs` companion, which lands roughly weekly; the 44 has not moved. Two lanes
quoting 142 and 145 will read it as disagreement and spend a battery reconciling instruments that are
identical. In-tree citations of each, re-read (not re-derived): **142** at `REHEARSAL-h5-go124.md:169`,
`CENSUS-h6-handown-go124.md:1115/1132/1580`, `CENSUS-bucket3-darwin.md:403`,
`.claude/skills/validation-bank/SKILL.md:352`; **145** in this record and `gate-forensics/SKILL.md:462`.
A **third** figure is also on the record and is a different instrument again:
`REHEARSAL-h5-go124.md:157` says the H6 census it is criticising has population **149**
(from `CENSUS-h6-handown-go124.md`) — RE-READ ONLY, not re-derived here. So is the **PROTECTED 151** at
`REHEARSAL-h5-go124.md:338`/§10.2, whose definition (markers **and** `*_impl.cs` companions over the
deletion pass's own population) was read but whose number was NOT MEASURED by C2.

⚠ The ref `9355669f8`, carried in lane dispatches as OLD-BASE, is **not** the older ref: it is dated
2026-09-12, is NOT an ancestor of 44f858717 (`git merge-base --is-ancestor` exit 1), IS an ancestor of
bd1d26faf (exit 0), and already reads 146 = 102 + 44. `panic_impl.cs` therefore landed between 44f858717
and 9355669f8. Any monotonicity or staleness argument that treats that label as chronological is inverted.

#### 5.6 The published 94-row table is identical as a multiset; the raw unsorted diff exits 1

Extracted from the record's fenced block (fences at lines 93/189, `sed -n '94,188p'` = 95 lines = 1 header
+ 94 rows) and normalised to `file|target|verdict`, the record's table compared against C2's independently
scored rows: **sorted `diff` exit 0, 0 diff lines, at both refs**. The **unsorted** `diff` exits **1** at
both refs, on 20 hunk lines = **10 rows**, every hunk a pure displacement (the identical row deleted at one
line and added at another). Whose order is which, settled independently:
`git show bd1d26faf:src/core/debug/pe/symbol_impl.cs | grep -nE '^[[:space:]]*using'` prints 52 `binary`,
53 `errors`, 54 `fmt`, 55 `io`, 56 `saferio` — source-file order — while the record's table puts `saferio`
first. So the record's own scratchpad instrument emitted a different within-file row order. Nothing about
the 94 rows, the 92/2/0 triple or the MOVED verdict string (`MOVED -> internal/runtime/sys`) differs.
State it as a **multiset** identity, with the sorted exit code; a re-derivation that runs an unsorted diff
and gets exit 1 has found a permutation, not a defect.

---

### 6. Drift at current master bd1d26faf

Range shape, measured: **49** commits (`git rev-list --count 44f858717..bd1d26faf`), **6** merges; **12**
touched `src/core` at all, **3** of those merges — that filter discards 37 of the 49 as not reaching
src/core. Parent-independent cross-check: `git diff --name-only 44f858717 bd1d26faf -- src/core/` = **733**
files, which is what the population and alias diffs were computed from.

**Exactly four denominators move, each by +1, and no verdict moves:**

| Figure | @ 44f858717 | @ bd1d26faf |
|---|---|---|
| marked files (anchored, src/core) | 145 | **146** |
| unanchored trap figure | 225 | **226** (gap stays 80) |
| bare `using <ns>;` | 403 | **404** |
| clean marked files ("143 of the 145") | 143 | **144 of the 146** |
| alias rows / plain-ASCII subset | 141 / 94 | **141 / 94** (unchanged) |
| verdict triple over the 94 | 92 / 2 / 0 | **92 / 2 / 0** |
| MOVED files / packages | 2 / 1 | **2 / 1** |

Set difference in **both** directions, not a count comparison: `comm -23` → ONLY_OLD = **0** (unfiltered,
nothing printed); `comm -13` → ONLY_NEW = **1**; `comm -12` → BOTH = **145**; 145 + 1 = 146 closes against
the measured population, so no offsetting pair hides behind the count. Zero marked files disappeared and
zero stopped being marked. Identity is by path; a re-marked rename would read as one disappearance plus one
addition, which did not occur.

The single addition is **`src/core/runtime/panic_impl.cs`**, and the +1 in the bare-using row is an
identified row rather than a difference of totals: ADDED = `src/core/runtime/panic_impl.cs|using go.golib;`,
REMOVED = empty.

- Provenance: added (status `A`) by **8fdbd4704** "runtime+golib: the fatal path severs onto the managed
  walk", later modified by **1800b04f8** (the license-metadata sweep). Not a rename or copy —
  `git show --name-status -M -C --find-copies-harder 8fdbd4704` still reports `A` with no source path. Not a
  redistribution of an existing hand-own either: `src/core/runtime/panic.cs`, whose 53 lines it displaces
  (`--numstat` → `2 53`), is NOT marked at either ref (anchored grep exit 1 at both). It does not exist at
  44f858717 at all (`git cat-file -e 44f858717:…` exit 128).
- Content at bd1d26faf: its complete using/marker block is two lines — 52 `using go.golib;` and 54
  `[module: go.GoManualConversion]`. **Zero** alias directives (alias regex exit 1); its single `_package`
  occurrence is line 58 `partial class runtime_package`, a class declaration, read in context. The file's
  own header states there is no `panic_impl.go`, so a `-stdlib` reconvert never regenerates it.
- The added bare-using token is `go.golib` — a golib C# namespace, not a Go import path — so the
  "zero name a directory lost at 1.24" half is untouched: the distinct-token set is identical at both refs.

**The largest churn over the record's own population is alias-invisible.** 1800b04f8 rewrote **95** of the
146 marked files. Across all marked files in the range, **96** changed and **482** lines changed, and
exactly **2** of those 482 lines contain the token `using`. A 95-file sweep over the census population
moved zero census rows. Only **3** of the 12 src/core commits touched a marked file at all, and two of
them are the same change: 8fdbd4704 and its merge 7d3d03284 ("Merge claude/c1-fatal-path-guard, train46
seat 3") report the identical 3 marked files (`runtime/managed_impl.cs`, `runtime/panic_impl.cs`,
`sync/mutex.cs`) — one change landed and then merged, not two independent hits.

**The second of the 2 `using` lines is a widened-predicate hazard worth naming now.**
`src/core/sync/mutex.cs` gained `using FatalReport = go.golib.FatalReport;` (OLD=0, NEW=1). It is
alias-form but its right-hand side is a golib **type**, not a `…_package` namespace, so it contributes 0
rows to this census's predicate; `mutex.cs` stays marked and stays out of the alias-bearing set at both
refs. If H6 is ever re-taken with a widened predicate (all `using X = Y;`), this row enters the population
and must be classified as non-Go — golib is hand-written C# and in `go list std` at no release — or it will
read as a false ABSENT, which is the record's own defect-#2/#3 family.

**The 2026-09-08 amendment's class did not drift, so this block opens no second front.**
`internal/weak`: 11 files / 1 marked. `internal/concurrent`: 13 files / 2 marked. Identical at both refs.
Corpus-wide reference sweeps identical too: **70** files mention `runtime/internal/sys` and **60** mention
`runtime/internal/math` at both refs, with the sys-mentioning file list diffing clean (exit 0). `ΔMapType`,
stated **with its scope** because this repo's own defect catalogue is why an unscoped count is unsafe:
**3 files inside the amendment's two directories** (`internal/concurrent` 2, `internal/weak` 1),
**23 files / 33 lines / 33 matches corpus-wide over src/core**, and **0 marked files** contain it — every
one of those figures identical at 44f858717 and bd1d26faf. Remedy C is therefore still sized as the
amendment sized it. The amendment's per-alias 1.24.13 scoring, its CS0426, its csproj dangling references,
its abi-alias usage counts and its orphan behaviour are **NOT MEASURED** here.

---

### 7. New facts for the record

**N1 — the 80 unanchored-only files are not merely "false"; 78 of them are the converter's own output.**
All **382** match lines across the 80 were extracted per file and classified: **380 PLACEHOLDER_COMMENT**
(377 func form, 3 type form, across 78 files), **1 INSIDE_STRING**, **1 LINE_COMMENT_OTHER**, and
**0 CANDIDATE_GENUINE** — the residual bucket printed unfiltered and empty (`grep` exit 1), which is what
makes "not one of the 80 carries a real marker" a measurement rather than an assumption. The placeholder
text is minted at `src/go2cs/visitFuncDecl.go:348` (`funcPlaceholderFormat`) and
`src/go2cs/visitTypeSpec.go:218`; run unfiltered, the provenance grep returns **four** lines — those two
minting sites plus `visitFuncDecl.go:353` (a lead constant) and
`src/go2cs/manualConversionDestination_test.go:811` (a guard regex over the emitted text). The two
non-placeholder rows are `src/core/go2cs/symbols.json:49` (`"goTrailing": "[module: go.GoManualConversion]
- hand-owned file; never convert over it",` — the converter's own symbols table holding the text it emits)
and `src/core/internal/concurrent/package_info.cs:65`. Extension split of the 80: 78 `.cs`, 1 `.json`,
1 `.auto` — the `.auto` being `src/core/runtime/runtime2.cs.auto`, the review sibling of a file that **is**
in the anchored 145. Shape: 33 files with 1 match line, 13 with 2, 8 with 3, 4 with 4, 1 with 5, 7 with 6,
4 with 7, and 1 each with 8/9(×2)/10/11/12/13/27/32/77; by top-level core package: runtime 34,
internal 16, syscall 10, reflect 6, os 4, vendor 2, and 1 each in time, sync, slices, net, math, go2cs,
debug, crypto (= 80). Top 12 by match count printed (`reflect/value.cs` 77, `reflect/type.cs` 32,
`syscall/windows/zsyscall_windows.cs` 27, `runtime/runtime2.cs.auto` 13, …); the remaining 68 names were
dropped, all of them placeholder-kind. **Consequence: the trap grows with the corpus.** Every new hand-own
adds placeholder lines to its generated sibling, so the unanchored-minus-anchored gap tracks the hand-own
count rather than being a fixed 80 of noise, the false population can never be cleaned up (only excluded by
the anchor), and an unanchored grep gets worse over time. Independently of G, the documentation
cross-check holds: `.claude/skills/corpus-reconvert/SKILL.md:110` names `src/core/reflect/value.cs` and
`src/core/internal/reflectlite/value.cs` as the placeholder false-alarm sites and both are in the 80
(`grep -xE` exit 0) — and that line is **outside** any HTML comment, so it does load on relevance.

**N2 — a naive C# block-comment state machine reads FALSE-INSIDE on this corpus.** C2's first exclusion
pass flagged 1 alias hit and 9 bare-using hits as sitting inside a `/* */` block. Reading the sources
falsified the instrument, not the population: each flagged file contains exactly one `/*` and no `*/`
anywhere, and both openers are inside `//` **line** comments —
`signal_posix_impl.cs:18` (`// is the os/exec-family wall: TestWaitInterrupt/*, TestSIGQUIT, TestSIGCHLD.`)
and `runtime_netpoll_impl.cs:62` (`// existing <Compile Include="$(GoTargetOS)/*.cs" /> glob …`). A
line-comment-aware machine returns 0 and 0. Converter-emitted doc comments quote Go test-name globs and
MSBuild globs, leaving an unmatched `/*` that swallows the rest of the file; an agent using such a machine
to exclude "commented-out" hits would silently subtract real rows.

**N3 — the record's own stated control `crypto/internal/alias → crypto/internal/fips140/alias` is not
reproducible from the rule as written.** Ranking same-leaf candidates on shared path components while
excluding only toolchain trees leaves a genuine **tie** at 1.24.13 between
`crypto/internal/fips140/alias` and `vendor/golang.org/x/crypto/internal/alias` (both share 3 components),
so a faithful implementation prints `AMBIGUOUS{…}` rather than the record's answer. It resolves uniquely
only if the `vendor/` tree is excluded too — an exclusion the record does not state, and one that is wrong
in general, since `vendor/golang.org/x/sys/cpu` is a legitimate alias target that EXISTS. Both real MOVED
rows are invariant under either policy, so no verdict in this block depends on it. A control is the part of
an instrument that must be reproducible from its written description; this one is under-documented in
exactly the way defect #4 itself was.

**N4 — the record's alias predicate is now fully reconstructed and independently re-runnable**, which it
was not before: G's scripts live only in lane G's scratchpad and are not in the repo. Two independent
reconstructions agree on the arithmetic to the digit — 141 total, minus the 34 `@`/`unsafe` rows, minus the
13 `Δ`-named rows, the two exclusion sets disjoint (union 47) = 94 — and the reconstructed 94-row table
matches the record's published block as a multiset at both refs. Anyone re-taking H6 after the hop can
rebuild the predicate from this block instead of trusting the document.

**N5 — the `_package` token appears in three other syntactic roles** inside the marked files (2
`using static` of a package class, 2 aliases to a type member, 1 `partial class runtime_package`
declaration), any of which a looser regex sweeps in as a fake alias row. The 146-vs-141 boundary above is
the audit trail for that.

**N6 — bare usings must be classified before they are resolved.** 45 distinct bare-using namespace tokens
across the marked files at 44f858717, identical set at bd1d26faf (zero added, zero removed), partitioning
completely as **24 DOTNET** (`System.*` plus `Microsoft.Win32.SafeHandles`), **16 GO_DIR**,
**2 USING_STATIC_CLASS**, **2 GOLIB_CS** (`go.golib`, `go.testing_runtime` — `testing_runtime` confirmed
absent from the 1.24.13 src tree, so a C# namespace and not a Go dir), **1 CORPUS_ROOT** (bare `go`).
Top-20 by count printed in full and all 45 tokens enumerated before classification; the tail of 25
per-token counts was not printed. A second pass at bd1d26faf that strips trailing comments and excludes
`using static` counts **43** distinct tokens, of which **17** resolve to a Go directory and **26** do not
(the 24 DOTNET plus `go.golib` and `go.testing_runtime`); 45 − 2 `using static` targets = 43 is arithmetic
on two measured counts, and 17 = the 16 GO_DIR plus bare `go`. The record's claim is confirmed in its
actual form — **none of the 26 was ever a Go directory, so none names a directory Go 1.24 LOST** — but a
statement of the form "only golib fails to resolve" is off by 26. One judgement call sits inside the
partition: bare `go` is classified CORPUS_ROOT, yet `$GOROOT/src/go` exists as a directory (`go/ast`,
`go/parser`, …) while `go` is not in `go list std`; it tests PRESENT under either classification, so an
agent re-deriving the split as 17 GO_DIR / 0 CORPUS_ROOT has not disagreed about anything material. The
hazard this guards against is concrete: feeding `go.@internal.runtime` to a **package** predicate
manufactures a bogus `MOVED -> internal/runtime` for what is a parent namespace and no Go package — which
is why the 403/404 were counted here and deliberately **not** run through the verdict classifier.
`internal/syscall/windows` is the mirror case: PRESENT as a directory but **not** in `go list std` on this
linux host, a selection fact a directory test cannot see.

**N7 — the 145/94 also appears in a skill, as provenance.**
`.claude/skills/gate-forensics/SKILL.md:462` reads
`(145 marked files, 94 converted-package aliases: EXISTS 92 / MOVED 2 / ABSENT 0)`, and that line sits
**inside** an HTML comment block — nearest opener 443, nearest closer 470, established by an open/close
state machine over all 148 delimiters in the 1906-line file, reporting inside_comment=1 at 461, 462 and
463. CLAUDE.md states block HTML comments are stripped before a file enters context but stay visible to a
human and to `Read`. The same is true of `.claude/skills/validation-bank/SKILL.md:352`'s 142 (comment
348-356). These are provenance to correct for a human reader, not live mis-citations — and the correction
is both halves: 145 → 146, and 94 → 141 aliases (94 with a plain-ASCII alias name).

---

### 8. Controls

No verdict in this block rests on an instrument that was not first made to go red.

| Control | Arm | Result |
|---|---|---|
| negative | one character altered in the marker regex | 0 matches, `grep` exit 1 — instrument proven able to red |
| negative | one character altered in the alias regex (`_package` → `_packages`) | 0 rows, exit 1 |
| negative | anchor removed from the marker regex | 145 → 225, number moved |
| negative | regex flavour swapped (PCRE `\s` → ERE `[[:space:]]` POSIX classes) | byte-identical 145-path list, `diff` exit 0 — the 145 is not a `\s` artifact |
| negative | a line dropped from the bd1d26faf marked list | set difference went 0 → 1 and named the dropped file |
| negative | synthetic alias rows through the classifier | `@internal.nosuchpkgxyz` → ABSENT; `runtime.@internal.math` → MOVED → internal/runtime/math (rejecting the decoy top-level `math`); `@internal.weak` → MOVED → weak; `@internal.concurrent` → ABSENT; `go.@internal.abi` → EXISTS; `crypto.@internal.alias` → AMBIGUOUS (N3) |
| negative | MOVED-resolver decoys, printed | `runtime/internal/sys`: 4 same-leaf candidates with `.go` counts, exactly one survives |
| positive | drop mapping step "strip leading `go.`" (the record's defect #2) | EXISTS 139 → 93, MOVED 2 → 48 |
| positive | drop dotted-segment recovery (defect #3) | EXISTS 139 → 138, MOVED 2 → 3, `vendor/golang.org/x/sys/cpu` flipping out of EXISTS |
| positive | wrong ref (9355669f8) | marked 148/145 → 149/146 — number moved |
| positive | perturb one verdict in the scored table | sorted `diff` exit 0 → 1 |
| positive | SDK sanity | `go env GOROOT` prints the pinned path exactly; `go list std` = 345 lines; `src/` = 76 entries; `runtime/internal` = exactly `startlinetest` + `wasitest` |
| positive | 1.24 relocation controls | MISSING: `runtime/internal/sys`, `runtime/internal/math`, `internal/concurrent`, `internal/weak`, `crypto/internal/alias`, a bogus dir. PRESENT: `internal/runtime/sys`, `internal/runtime/math`, `internal/sync`, `weak`, `crypto/internal/fips140/alias` |

Independence within C2: the second derivation was written from the task description without reading the
first pass's scripts, and derived its own population, extractor, mapper and classifier end to end. Its
`go list std` output is byte-identical to the first pass's (`cmp` exit 0) — expected, same pinned SDK —
but every downstream number was re-derived, and the row set matched in **both** `comm` directions (0 and
0), which is the check a count alone cannot make.

---

### 9. Limits — what C2 could not reach

- **The EMISSION half, in its entirety. NOT MEASURED.** Nothing was built: no conversion, no
  `-stdlib` reconvert, no `dotnet` (absent on this box). Whether any C# namespace still *resolves* after
  the hop, and what R's CS0246 on `using runtime.@internal;` actually **is**, are untested by
  construction — C2 measured only that emptiness is not the cause, and §5.4 narrows even that margin.
- **What existed at the PRE-hop Go release. NOT MEASURED.** Only the pinned go1.24.13 SDK is on this box,
  so every EXISTS/MOVED/ABSENT is a statement about 1.24.13 alone. "Lost at 1.24" is re-scored only in its
  checkable direction (does it resolve at 1.24.13), never as a before/after comparison of two GOROOTs.
- **Build-tag selection. NOT MEASURED.** Which files a given GOOS/GOARCH selects, and therefore which of
  the 16 Go-derived namespaces the corpus emits per target, was not measured;
  `internal/syscall/windows` is the case in point (PRESENT as a directory, not in `go list std` here).
- **API-level compatibility of the relocated packages. NOT MEASURED.** `internal/runtime/sys` and
  `internal/runtime/math` were confirmed by `go list std` membership plus leaf carry-over; no content or
  exported-symbol diff was taken, so a rename that also changed signatures would not show up.
- **G's instrument source. NOT MEASURED.** `g-h6census.sh`, `g-nstogo.sh` and `g-movedto.sh` are not in
  the repository. "The extractor requires a plain-ASCII alias name" is inferred from a perfect 3×2
  separation and a 34 + 13 = 47 closure, not read from code; whether widening one character class in those
  scripts would yield 141 is likewise NOT MEASURED.
- **Whether each of the 145/146 marked files is a legitimate hand-own. NOT MEASURED.** The population
  predicate is "carries the anchored marker". No file was audited for whether its marker is warranted, and
  the record's own amendment class of **unmarked** metadata files (`package_info.cs`, `.csproj`) is a
  population a marker-keyed instrument cannot see; it was not enumerated.
- **Non-alias references to moved packages. NOT MEASURED** beyond the two corpus-wide sweeps in §6. The
  alias predicate reads `using X = <ns>_package;` lines in `.cs` blobs; csproj `<ProjectReference>` rows,
  `using static` and type-member aliases are outside it. "No hand-own **aliases** it" is measured;
  "nothing references it" is not.
- **`src/core/runtime/panic_impl.cs`'s correctness. NOT MEASURED** — only that it exists, is marked,
  carries zero package aliases and has no Go source to be regenerated from. Its behaviour and the adding
  commit's own owed gates are untouched here.
- **Anything at a ref newer than bd1d26faf. NOT MEASURED.** This worktree is read-only and shared, so no
  fetch was performed; "current master" is bd1d26faf as supplied, and the safety floor's
  measure-after-a-fetch rule is unmet for that reason.
- **Whether lane R published a 145 outside the git tree. NOT MEASURED.** The tree at bd1d26faf was
  searched, `docs/phase4/MAILBOX.md` included; a post at another ref or on an out-of-tree channel was not.
- **The record's `git grep`-vs-`-l`-vs-`-o` counting modes outside the figures listed.** The path-filtered
  commit count 12 is git's default history-simplified count; the unsimplified set was not enumerated.
  NOT MEASURED.

---

### 10. Verdict

**AMENDMENT BLOCK, NOT A RE-TAKE**, derived from three printed results rather than judgement:

1. **The record's output is unchanged.** Its 94-row table is identical to C2's independently scored rows as
   a multiset at both refs (sorted `diff` exit 0, 0 diff lines — see §5.6 for the unsorted exit code), and
   the verdict triple is 92 / 2 / 0 at both refs.
2. **The actionable set is unchanged.** 2 files, 1 package, both blobs byte-unchanged across the range
   (`git diff --name-only` → 0 lines each), and confirmed by a second instrument over a population 50%
   larger than the record's, which adds no MOVED and no ABSENT row.
3. **Only denominators move.** marked 145 → 146, clean 143 → 144, bare-using 403 → 404, unanchored trap
   225 → 226, with zero removals (ONLY_OLD = 0) and the +1 in each case an identified row.

What the record should carry forward, with the corrected numbers:

- "`using … _package;` aliases across them = 94" → **141 alias directives over 66 files; 94 of them have a
  plain-ASCII alias name, which is the subset this record's extractor captured.**
- "EXISTS 92 / MOVED 2 / ABSENT 0" → **EXISTS 139 / MOVED 2 / ABSENT 0 over 141** (92 / 2 / 0 over the 94).
- "143 of the 145 marked files carry no moved or absent package alias" → **144 of the 146 at bd1d26faf**;
  and the informative shape is that **64 of the 66 alias-bearing files are clean** — the other 79/80 are
  clean by carrying no alias, not by surviving a check.
- "both still in `go list std`, and the corpus converts all four today" → **the corpus converts three;
  `runtime/internal/wasitest` is external-test-only at 1.24.13 and emits nothing. The margin under
  `go.runtime.@internal` is one member, `startlinetest_package`, not two.**
- "The 145 matches lane R's independently-derived seed figure." → **the 145 supersedes R's 142 by three
  `*_impl.cs` companions that landed between f4ced674d and 44f858717; at a common ref the two derivations
  are identical. The invariant to quote across refs is 44 whole-file rewrites.**
- Add the **fifth instrument defect** (alias-name character class) beside the four already documented, and
  the note that `crypto/internal/alias` → `crypto/internal/fips140/alias` needs a stated `vendor/`
  exclusion to reproduce.

## 2026-09-13 — R: the emission half (ARM A withdrawn, ARM B, ARM C) and two figures reconciled

Lane R's half, as its own dated block beside C2's, per COORD (`MAILBOX-archive-2026-09-13.md:161897`, the
pre-rotation mailbox body added to `claude/mailbox` at `5e70540f4`; archive cites below are shortened to `:N`).
**Posted** figures are quoted from the 2026-09-08 posts. **Re-measured** figures were taken on 2026-09-13 at
master `a02ac3df3` and at this record's ref `44f858717` over `git cat-file` blob exports, and on the H5 ladder
(the seeded go1.24.13 three-target `-stdlib` emission on R-LAPTOP). The ladder is a mutable tree, not a ref.
Since the 2026-09-08 runs, 42 of its `.cs` files outside `bin`/`obj`/`Generated` have taken later mtimes, and
the walked population has grown by 12 files. The instruments are off-git scripts in lane R's archived session
scratchpads, named with a sha256 prefix. A few facts come only from lane R's archived session log; they are
tagged *(R session log, off-git)*. No converter, build or `go test` was run for this block.

### 1. What this half measures, and why a GOROOT-side census cannot see it

G's half asks whether each alias still names a Go package that exists. This half is the complement: the package
exists and nothing moved, but a frozen hand-own disagrees with a name inside the C# corpus. The two shapes, as
R's posts state their build consequence (not built here):

- a `[GoValueClone(...)]` stamp naming a member its type does not declare, which is CS1061 because `go2cs-gen`
  emits a member access per name (`:159099-159100`);
- a frozen hand-own declaring a field that an emitted partial of the same type also declares, which is CS0102
  (`:160749-160750`).

A GOROOT-side census resolves import paths against a Go SDK and never reads a member name, so it has no
predicate for either shape.

### 2. ARM A: built, then withdrawn

The rule was that every alias target a hand-own spells must also be spelled by at least one emitted file
(`r-h6-emission-half.sh`, sha256 `0c1d90137611…`). As posted it reported 0 findings (`:159116-159117`), and its
own control showed that zero to be vacuous. The stale `runtime.@internal.sys_package` is spelled by 3 emitted
files (`:159118`), because the old `runtime/internal/sys` directory survives in the ladder as leftover seed. The
predicate cannot tell a stale spelling from a spelling used by a stale package, so it could not fire on its
motivating case. It was withdrawn, not reported. The release-side ground is covered by G's verdicts as widened
to 141 aliases by C2's §5.1 (139 EXISTS / 2 MOVED / 0 ABSENT).

A read-only port run on the ladder today reproduces the run: 146 hand-owns, 97 file-target rows, 97 spelled,
0 not spelled. It compared 3,817 emitted files (3,805 at the time, *R session log, off-git*), and the control
still reads 3, all under `runtime/internal/sys`.

### 3. ARM B: stamp members must exist in the type they stamp

**Rule.** In every marked file, each name inside `[GoValueClone(...)]` must be declared in the stamped type. This
is internal consistency, so no emission comparison is needed. Instrument: `r-h6-arm-b.py` (sha256 `823e35103cd9…`).

| Tree | Instrument | marked | stamps | names | undeclared | Provenance |
|---|---|---|---|---|---|---|
| `4c491cb20:src/core/runtime/runtime2.cs` (control) | as posted | 1 | 3 | 12 | 1, `m.Δtrace` | posted `:159105`; re-measured identical |
| same | type-name class widened | 1 | 4 | 17 | 1, `m.Δtrace` | re-measured |
| H5 ladder | as posted | 146 | 4 | 13 | 0 | posted `:159107`; re-measured identical |
| H5 ladder | widened | 146 | 6 | 19 | 0 | re-measured |
| `44f858717` / `a02ac3df3` | as posted | 145 / 146 | 0 | 0 | 0 | re-measured |
| `44f858717` / `a02ac3df3` | widened | 145 / 146 | 1 | 1 | 0 | re-measured |

**Correction to the posted arm.** The script's type-name class is ASCII-only (`[A-Za-z_][A-Za-z0-9_]*`,
`r-h6-arm-b.py:32`), so it drops every stamp on a `Δ`-prefixed type.

- At both git refs the only stamp is `[GoValueClone("children")] partial struct Δindirect<K, V>`
  (`src/core/internal/concurrent/hashtriemap_whitebox.cs:67`), so the script's 0 stamps there is vacuous.
- A copy with that one class widened to `[^\W\d]\w*` shows what the script missed: 2 stamps carrying 6 names on
  today's ladder, and `partial struct Δp` (5 names) on the control.
- Neither verdict moves: the census still reads 0 undeclared, and the control still fires on `m.Δtrace`.
- Reach control: a planted marked file carrying `[GoValueClone("children","Δmissing")] partial struct
  Δindirect<K, V>` reads 0 stamps under the script as posted, and 1 stamp / 2 names / 1 undeclared under the
  widened copy.
- Negative control: `runtime2.cs` alone at `a02ac3df3` reads 0 stamps.
- A separate label defect: the first output line, "marked files with a stamp", counts every marked file.

**Where it lives.** Train 47 row 2, `claude/coord-stamp-guard` at `ec1fe2745` (parent `44f858717`), file
`src/go2cs/valueCloneStampMembers_test.go`. Boarded at `:161861`; the row number is at `:163010`. The guard's
type-name class admits Unicode letters (test file line 55, `[\p{L}_][\p{L}\p{N}_]*`; read, not run). Its logged
1.23.12 population agrees with the widened reading: 145 hand-owned files, 1 stamp, 1 name (*R session log,
off-git*; the comment at lines 47-48 says "~145 marked files", one stamp, one name). Its 1.24 comment (line 49,
"4 stamps / 13 names") carries the ASCII script's ladder figure; the widened reading today is 6 / 19.

### 4. ARM C: no field declared by two compile-compatible partials of one type

**Rule.** No `(type, field)` pair may be declared twice within a package across files that compile together.
The red case is historical: master's `runtime2.cs` `note { key }` beside 1.24's emitted `note_other.cs`
(`:160742-160744`). Instrument: `r-armc-proto.py` (sha256 `b831445e2555…`; its header still says "VERSION 2").
Control: `r-armc-control.py` (sha256 `0c47f9af986c…`).

| Version | Change | Posted reading |
|---|---|---|
| v1 | brace depth over raw text, key = bare type name | 25, at both trees (`:160756`) |
| v2 | string literals and comments blanked before brace counting | 4, all in `net/http` (`:160763`) |
| v3 | key qualified by the enclosing type chain | 0, at both trees (`:160771-160775`) |

**The control** has six arms (`:160783-160788`):

- RED, the real `note.key` pair, must read 1.
- Five arms must read 0: ADMIT (disjoint members), EMPTY (the `package_info` shape), FLAVOUR (`windows/` beside
  `linux/`), DESYNC (a literal with a net-unbalanced closing brace) and NESTED (one name under two enclosing
  classes).
- `ARMC_NEUTER=1` disables literal blanking and must turn DESYNC red.

Re-run today: live, PASS (rc 0); neutered, DESYNC reads 1 while every other arm holds (rc 1). The re-run copy
differs from the archived control by one line: a print in place of temp-directory removal.

| Tree | files | packages | pairs | findings | Provenance |
|---|---|---|---|---|---|
| "master (44f858717)", proto | 3,759 | 306 | 40,484 (type,field) | 0 | posted `:160774` |
| the guard (Go port), as posted | 3,759 | 306 | 41,784 (type,member) | 0 | posted `:160964` |
| H5 ladder, 2026-09-08 | 3,949 | 357 | 41,347 | 0 | posted `:160775` |
| H5 ladder, today | 3,961 | 357 | 41,383 | 0 | re-measured |
| `44f858717` blob export | 3,746 | 306 | 40,477 | 0 | re-measured |
| `a02ac3df3` blob export | 3,749 | 306 | 40,477 | 0 | re-measured |

- **The posted "master" arm was not run over a blob export.** It walked 13 more files and 7 more pairs than
  today's export of `44f858717`, with the same verdict. Those 13 files are unidentified (NOT MEASURED), and the
  guard's own posted walk read the same 3,759.
- **Pair counts differ between the proto and its Go port**: 40,484 against 41,784 at the same file and package
  counts. The difference is NOT MEASURED.
- **Neutered, at either ref**, the proto reads 38,716 pairs and 4 findings: `encoding/xml`
  `xml_internal_test_package.version` (three file pairings), and `internal/trace/traceviewer`
  `traceviewer_package.type` (`http.cs` | `mmu.cs`). The traceviewer row was checked at source in R's post
  (`:160757-160758`). The three `encoding/xml` rows were not inspected; they are classed as desync only by the
  live-versus-neutered difference.
- **Scope limits:**
  - fields only, because methods overload legally;
  - flavour directories never compile together;
  - `Generated/` is excluded;
  - files outside any `.csproj` directory are not walked (2 at `a02ac3df3`: `src/core/GlobalUsings.cs`,
    `src/core/go2cs/Symbols.cs`).

**Where it lives.** Train 47 row 3, `claude/laneR-armc-guard` (row number at `:163011`), file
`src/go2cs/duplicatePartialMembers_test.go`. The seated tip was `bbd0afe43` (parent `44f858717`); since
2026-09-13 it is `49c309f8b`, a commit on top that changes only the license header (+4/-2).

### 5. Two figures reconciled

**(a) The 145.**

- **The record's sentence** ("The 145 matches lane R's independently-derived seed figure") names R's LADDER
  count: "The ladder carries **145** marked files" (`:156447`). That count was posted before G's census, and G
  matched it (`:156605`, `:156610`). The ladder is a mutable tree, not a ref.
- **The ref-level agreement came after.** R's standalone re-derivation, "**145 / 145, zero differences**" between
  master (then `44f858717`) and the ladder (`:157030-157032`), was prompted by G's number: "caught only because
  G's number disagreed with mine" (`:157033`). *R session log, off-git:* R's saved master list is the same path
  set as `git grep -l` of the anchored marker at `44f858717`.
- **C2's §9 item** "Whether lane R published a 145 outside the git tree" is answered: yes, on the mailbox at the
  two places above.
- **An R-attributed 145 is also IN the tree**, including the tree C2 searched:
  `a02ac3df3:.claude/skills/gate-forensics/SKILL.md:608` (`bd1d26faf:600`, inside an HTML comment, added by
  `86037ef2e`) reads "a MARKED set of 145. R's third narrowing of one census". C2's in-tree search keyed on the
  phrase "seed figure" and did not reach it, which is within the scope C2 declared.
- **What therefore does not survive** is two C2 sentences, not C2's measurement:
  - §5.5's "R's published figure is **142**, not 145". R published both: 142 at `f4ced674d`
    (`REHEARSAL-h5-go124.md:169`) and 145 at the ladder and at `44f858717`.
  - §10's carry-forward replacement, "the 145 supersedes R's 142 by three `*_impl.cs` companions", should not
    be adopted as a replacement for the record's sentence. The record's sentence stands, citing `:156447` (a
    ladder count) and `SKILL.md:608`.
- **What stands in C2's §5.5:** 142 at `f4ced674d`, reproduced to the row; the three companions between
  `f4ced674d` and `44f858717`; 44 whole-file rewrites as the cross-ref invariant; and C2's own statement that the
  substantive point survives.
- **Re-measured today:** 145 at `44f858717` and 146 at `a02ac3df3`. The path lists are byte-identical across
  `git grep` PCRE, `git grep` ERE and grep over the blob export. The one addition is
  `src/core/runtime/panic_impl.cs`.

**(b) The alias figures, side by side.** Every predicate reads line-anchored directives over the marked files,
except where the table says otherwise.

| Figure | Predicate | `44f858717` | `a02ac3df3` | H5 ladder | Published |
|---|---|---|---|---|---|
| 94 | alias name plain ASCII, no `@` | 94 | 94 | not run | G, this record; per-file counts `cmp` 0 vs the Full table |
| 141 | any alias name | 141 | 141 | not run | C2 §5.1; its 94 + 34 `@` + 13 non-ASCII split reproduced |
| 128 | ASCII name, `@` admitted | 128 | 128 | not run | not published |
| 97 | R's ARM A extraction: unanchored, unique target per file, `@`/`Δ` names excluded | NOT REPRODUCED (96) | NOT REPRODUCED (96) | 97 | R, `:159116` |

R's "different trees" account of 94 against 97 (`:159133-159134`) is NOT REPRODUCED as the whole cause.

- Two of the three extra rows come from the predicate, not the tree:
  - `hashtriemap_whitebox.cs:37`, a `//` comment containing `using sync = sync_package;`;
  - `runtime2.cs:4`, `global using itab = go.@internal.abi_package.ITab;`, a member alias read as a second abi
    target.
- Only one comes from the tree: the ladder's `runtime2.cs` adds `@internal.goexperiment_package`.
- The `sys` re-spelling nets 0 (2 rows out, 2 in).
- R's grep lines and a Python transcription of them give row-identical output at both refs.

### 6. Limits

- The ladder is not a ref. Its readings describe the tree as it is today, not the tree the 2026-09-08 runs saw.
- NOT MEASURED:
  - either guard's `go test` outcome at these refs (regexes read with `git show` only);
  - the effect of ARM B's declared-member regex differing from the guard's (test file line 99);
  - the proto-versus-guard pair-count difference in ARM C;
  - the posted master arm's 13 extra files;
  - ARM A's emitted-spelling half at any ref;
  - the integrity of the archived ladder tarballs.
- The comparison against G's table is per-file counts only; C2's §5.6 multiset check covers targets.
- `4c491cb20` (on `claude/c1-h6-rewrites`) is a control blob only, not an ancestor of `44f858717` or `a02ac3df3`
  (`merge-base --is-ancestor` exit 1 for both).
<!-- {% endraw %} -->
