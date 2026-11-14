# A pointer returned from a call keeps what it carries

**Status: PLAN, posted before code (2026-11-13).** COORD ruled this seat on 2026-11-13: one seat of its own, ahead of
cgo Phase 3's increment 3.1, in the next corpus-moving slot after the 1.26.11 hop. Branch
`claude/h-unsafe-pointer-call-keeps-box`, cut from master `3da19d0e2e`.

## 1. The defect

Go's `unsafe.Pointer` keeps its referent alive. go2cs models that by having a pointer CARRY the box it was minted
from, and the box holds the pin that keeps its storage where the pointer's number says it is.

The converter prefixes every call whose result type is `unsafe.Pointer` with `(uintptr)`, wherever nothing else is
constructing from it:

```csharp
internal static partial @unsafe.Pointer mint(slice<byte> b) { return (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 4); }
@unsafe.Pointer q = (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 8);
```

The cast turns the returned pointer into a number, and the number converts back to a fresh pointer that carries
nothing. Nothing then holds the pin. After a collection the storage may move, and a write through the pointer no
longer reaches it.

The prefix entered on 2025-04-03 (`435ca3c06d`), for the pointer cast, which needs a `uintptr` between an
`unsafe.Pointer` and a typed pointer. It predates the carrying model by more than a year. A pointer cast and
`uintptr(...)` each build their own cast around the call, so the prefix is redundant there too (the corpus has lines
reading `(uintptr)(uintptr)f(...)`).

## 2. What was read before this plan (H, 2026-11-13)

**It is live outside cgo, and silent.** A program with no cgo takes `p` from a function that returns
`unsafe.Add(unsafe.Pointer(&b[0]), 4)` and `q` from the same expression directly, churns the heap, collects twice,
writes through both and reads `b` back. 20 rounds a run, 3 runs an arm, converted for windows/amd64:

| Arm | Rounds wrong of 20 |
|---|---|
| Go | 0 |
| converted as emitted | 20, in 3 runs of 3. Exit 0, no message |
| control A: the same emission with the churn and the collections removed | 0, in 3 of 3 |
| control B: the same emission with the cast removed by hand, against golib with `unsafe.Add` keeping what its pointer carried | 0, in 3 of 3 |

Control B against MASTER's golib was not run. It is this seat's first reading (PB-2).

**Nothing fails to compile without it, on windows.** A converter that omits the prefix on every true call (a
conversion keeps its emission) converted the standard library for windows/amd64 beside the unmodified one, each
into its own seeded root: both exit 0, 0 degraded. The 16 affected packages, built with the changed files, read 0
errors each. Not probed: the other three targets' compile, the test emission, and any run-time behaviour.

**The footprint.** By the Go source the converter reads (a call, not a conversion, whose result type is
`unsafe.Pointer`, classed by its consumer):

| Target | Calls | Consumed as `unsafe.Pointer` | Packages | Operand of a pointer cast | Operand of `uintptr(...)` | Discarded |
|---|---|---|---|---|---|---|
| windows/amd64 | 885 | 602 | 16 | 269 | 12 | 2 |
| linux/amd64 | 882 | 596 | 14 | 275 | 9 | 2 |
| darwin/amd64 | 874 | 593 | 15 | 269 | 10 | 2 |
| linux/arm64 | 882 | 596 | 14 | 275 | 9 | 2 |

Of windows' 602: `runtime` 317, `internal/runtime/maps` 127, `reflect` 112. `unsafe.Add` is 5 of them. The other
597 are ordinary functions and methods.

By the emission, windows/amd64, the two seeded conversions diffed: 77 files, 616 lines removed and 616 added, every
removed line carrying the cast, 0 position-map lines. The two derivations agree in size and are not reconciled
line by line.

## 3. The change

- **Converter.** A TRUE call whose result type is `unsafe.Pointer` gets no `(uintptr)` prefix. A conversion, a
  pointer cast and `uintptr(...)` keep their own emission.
- **golib, in the same seat.** `unsafe.Add` keeps what its pointer carried: over an `unsafe.Pointer` that carries a
  box it returns a pointer carrying a native box AT the new value, which retains what the pointer carried; typed
  `unsafe.Add` over a native box retains it the same way. A token, and C memory alone, keep the plain number. The
  two close together or not at all: without the converter's half the carried box is cast away, and without golib's
  half `unsafe.Add` has already dropped it.
- **The corpus** is regenerated for the change's own lines, on all four targets, in the slot COORD ruled.

> **Amended 2026-11-14 (interior-pointer root, DESIGN-interior-pointer-root.md 4.1 and 10.1).** "A token keeps the
> plain number" no longer holds for what the pointer carries. The moved pointer's number is still the token plus the
> offset and no address is formed, but the pointer now carries the box the token names, because the token resolves
> only while that box is alive. `UnsafeAddCarryTests.AddOverATokenKeepsTheTokensOwnRoute` asserts the new form. C
> memory alone still keeps the plain number.

## 4. Predictions, to be scored as worded

- **PB-1 (red first).** The lifetime program, tracked as a behavioral program with three shapes (a function
  returning `unsafe.Add`, `unsafe.Add` in the expression, and a function returning its own pointer), fails its
  output comparison at master: Go prints 0 wrong for each shape, the converted program 20 for each.
- **PB-2.** Control B against master's golib (the cast removed by hand, nothing else changed): the two `unsafe.Add`
  shapes stay wrong, and the function returning its own pointer reads 0 wrong.
- **PB-3.** With both halves the program prints Go's lines, in 3 runs of 3.
- **PB-4.** The corpus footprint on windows/amd64 is 77 files, 616 lines removed and 616 added, 0 position-map
  lines, at the tree the probe read. The seat's base is a later master, so the number is re-read there and any
  difference is attributed to what landed between. linux/amd64, darwin/amd64 and linux/arm64 are MEASURED before
  they are scored; by the census's ratio each is 605 to 612 lines, and their file counts are not predicted.
- **PB-5.** Fewer than 12 behavioral golden files move. Five are named now, by a text pattern:
  `ReflectValueSingles`, `UnsafePointerArgPassing`, `UnsafePointerInferredNoImport` (two files) and
  `UnsafePointerParamAssign`. The full list is named before `check-no-regression` runs.
- **PB-6.** GolibTests' total rises by the new arms only, and no existing row changes.
- **PB-7.** The full behavioral suite passes, with no NOT MEASURED line.
- **PB-8 (the weakest).** Every banked count in the ruled sweep holds: the 16 packages' rows, the reflect canaries
  (`crypto/cipher`, `runtime`, `crypto/tls`, `net/http`, `go/types`), and every row with an allocation assert (the
  48 rows plus `crypto/tls`). At about 600 sites a pointer that was a bare number now carries its box, so the five
  readers of a pointer's retained source see a box where they saw nothing, and an asserted body stops allocating a
  fresh pointer for the round trip. A body that gets CHEAPER can still fail an exact-equality assert.

## 5. Gates

The two-seeded `-stdlib` comparison on all four targets; `check-no-regression`, with the goldens it moves named
before it runs; the full behavioral suite; the full, unfiltered GolibTests against the base's results; the full
`go test ./...`; and the ruled sweep of PB-8.

## 6. Stop lines

- **SB-1.** A banked count moves, cheaper or dearer: stop and post the row. Nothing is re-baselined.
- **SB-2.** A golden moves that was not named before `check-no-regression` ran.
- **SB-3.** A corpus line moves that is not the prefix's removal, or a position-map line moves.
- **SB-4.** A package stops compiling on any target, or in the test emission: stop and post the site. No hand patch.
- **SB-5.** After the fix the lifetime program reads anything but Go's lines in any run.
- **SB-6.** The seat needs a golib change beyond `unsafe.Add` keeping what its pointer carried.
- **SB-7.** The 1.26.11 hop lands under the seat: every corpus reading is re-read on the hopped corpus, and nothing
  measured before the hop is scored after it.

— H (2026-11-13)

## 7. Record 2026-11-13: the seat as built and measured, before the corpus slot

**The trees.** The plan is `9c2a83995a`; the red `51271c47c5` (master `3da19d0e2e`); the fix `bb015c6f3f`; the goldens
`b575224ef9`; three converter guards and one provenance note `2ae993dd24`; master's batches 297 and 298 merged in at
`1a6a97997a` (parents `2ae993dd24` and `65404e5e63`, no rebase, no conflicts). Every gate below was read at
`1a6a97997a`. A battery started before the merge was stopped and is not scored.

**The red (PB-1), at master `3da19d0e2e`.** The behavioral program `UnsafePointerCallResultLifetime` takes three
pointers into a slice's storage, each from a call (`unsafe.Add` in a return, `unsafe.Add` into a variable, and a
function returning its own pointer), makes the collector run, writes through them, and exits 1 if a write did not
reach the slice. Go prints 0 wrong for each and exits 0. Through the behavioral runner the converted program passes
Transpile, Compile and Target and FAILS Output: it dies with `Fatal error. Internal CLR error. (0x80131506)`, in 3
direct runs of 3. The stray writes do not only miss the slice. They corrupt the managed heap.

**One shape at a time (PB-2), against master's golib.** Hand-edited copies of the same emission, the other shapes'
writes made directly so the heap survives to be read, 3 runs each:

| Shape | As emitted | The cast removed by hand |
|---|---|---|
| a function returning its own pointer | 20 wrong of 20 | 0 wrong, exit 0 |
| `unsafe.Add` in a return | 20 wrong of 20 | 20 wrong of 20 |
| `unsafe.Add` into a variable | 20 wrong of 20 | 20 wrong of 20 |

The cast's removal alone closes a function returning its own pointer. `unsafe.Add` has already dropped what its
pointer carried, so it needs golib's half. The two halves close different shapes.

**Two forms of the program did not read red, and why that matters.** With all three pointers taken in the checking
frame itself, the program PASSED at master: with 20,000 allocations a round, and again with 200,000. Crossing the
two programs with two library trees found the axis: the PROGRAM, not the library. Each pointer is now minted in its
own function. Why a third pointer in the same frame keeps the writes landing is NOT ROOTED. What it shows is that
this defect's visibility depends on what else a frame happens to keep alive. **A validated row or a behavioral
golden can pass at master for exactly that reason. So the sweep after this seat may surface rows that change for
the BETTER, and those are posted, not explained away** (COORD, 2026-11-13).

**The tree already knew one member.** Two converter guards (Q49, 2026-09-05) record
`f(g(unsafe.Pointer(&x)))` with `g` returning `unsafe.Pointer`: the cast stripped the box, a collection under the
call retired its provenance entry, and runtime's `int32Hash` refused by name. The remedy then was local, a
`GC.KeepAlive` of the frame-minted box after the statement. It does not reach a pointer returned out of the frame,
a variable, or a stored result. This seat removes the cause. The drain is still emitted and still checked.
**OWED:** whether that drain is now redundant, read after the sweep.

**The green (PB-3).** With both halves the runner passes all four phases, and 3 direct runs of 3 print 0 wrong for
each shape and exit 0. `UnsafeAddCarryTests` (pointers taken as the converter emits them) reads 5 of 5. Negative
control: with master's `unsafe.cs` put back, its two lifetime arms fail by name and its three controls pass; the
fix was restored byte-identical. The native-root test `unsafe.Add` needs is cgo Phase 3 increment 3.1's own two
hunks, byte for byte.

**The goldens (PB-5).** A scan of each behavioral program's Go source named 18 programs before
`check-no-regression` ran. Ten of them moved, in eleven golden files, 29 lines, each a line that loses the prefix
and nothing else. The other eight regenerated byte-identical: the scan names more than the prefix reached. No
golden outside the 18 moved.

**Existing tests that asserted the old emission, found by the first `go test ./...` and not before.**
`TestDiscardedUnsafePointerResultStatement` asserted that a consumed result KEEPS the cast; it now asserts the
opposite, and still asserts the discarded call is a bare statement. The two Q49 guards found the call by its text,
cast included. One provenance note cited converter lines that the change moved down by nine. The plan named
goldens and not these. A converter seat now searches the TESTS for the emitted text it changes, before the change.

**The gates at `1a6a97997a`, on H, one stage at a time.**

| Gate | Reading |
|---|---|
| `go test ./...` | rc 0, 11 packages ok, 0 cached |
| GolibTests, full | base (master `65404e5e63`) 2,586 passed, 43 skipped of 2,629; seat 2,591 passed, 43 skipped of 2,634. The TRX difference is exactly the five new arms |
| `check-no-regression` | rc 0, byte-identical across all 1,045 behavioral packages |
| the full behavioral suite | rc 0, PASS, 959 projects: Transpile, Compile and Target 959 of 959; Output 929 passed, 0 failed, 30 skipped, 0 timed out. No NOT MEASURED line. The tree was clean after it |

**The footprint (PB-4), measured and NOT applied.** Two seeded `-stdlib` conversions per target, A = master
`65404e5e63`'s converter and B = the seat's, each into its own root. Every conversion exited 0 with 0 degraded
packages. Nothing was written into `src/core`.

| Target | Files | Lines removed and added | Removed lines without the prefix | Position-map lines |
|---|---|---|---|---|
| windows/amd64 | 77 | 616 | 0 | 0 |
| linux/amd64 | 76 | 612 | 0 | 0 |
| darwin/amd64 | 75 | 609 | 0 | 0 |
| linux/arm64 | 76 | 612 | 0 | 0 |

**The scores, as worded.**

| Prediction | Score |
|---|---|
| PB-1 | MISSED, in the worse direction: the converted program does not print 20 for each shape. It dies with an internal CLR error before it prints |
| PB-2 | MET on both halves |
| PB-3 | MET |
| PB-4 | windows/amd64 MET exactly at the merged base (77 files, 616 lines, 0 position-map lines). The other three were measured before they were scored: 612, 609 and 612 lines, inside the 605 to 612 the census's ratio gave. Their file counts were not predicted |
| PB-5 | MET as first worded (11 existing golden files, fewer than 12) and MET as restated before the measurement (moves only within the 18 named programs) |
| PB-6 | MET |
| PB-7 | MET |
| PB-8 | NOT MEASURED: the sweep runs in the corpus slot |

**Stop lines.** None fired at `1a6a97997a`. SB-7 stands: the 1.26.11 hop has not landed, and every corpus reading
above is re-read on the hopped corpus before it is scored there.

**Owed at the slot, in one change:** the corpus regenerated for the change's own lines on all four targets; the
conversion-strategy reference (`pointers.md`, the section on bridged `unsafe.Pointer` arguments), whose sample is
read from the corpus and still shows the cast, with the new rule beside it; and the ruled sweep (the 16 packages'
rows, the reflect canaries, and every row with an allocation assert).

— H (2026-11-13)

## 8. Record 2026-11-13: the corpus slot, as applied and measured (batch 301)

**The trees.** On top of section 7's `f036dcf0ce`: master merged twice, at `79e6f116a1` (through batch 299,
`6dc4bae09b`) and at `9b6562b21c` (batch 300, `651c4ad147`), no rebase, no conflicts; the application `41067a0fb7`;
one docs sample `0ddca05b39`. The footprint was re-read at `9b6562b21c`. The build and `go test ./...` were read at
`41067a0fb7`, every other gate and the sweep at `0ddca05b39`.

**The footprint, re-read at the merged base.** Two seeded `-stdlib` conversions per target, A = master
`651c4ad147`'s converter and B = the seat's. Eight conversions exited 0 with 0 degraded packages. The reading is
section 7's, line for line: windows/amd64 77 files and 616 lines, linux/amd64 76 and 612, darwin/amd64 75 and 609,
linux/arm64 76 and 612; 0 removed lines without the prefix, 0 position-map lines, 0 files in one root only.

**The application (`41067a0fb7`), by the hunk rule, one target at a time.** Each file is
`git merge-file` of the committed file with the two emissions, and is proved twice before it is written: the applied
delta equals the emission's delta, and the file's residual drift against the emission is the same set before and
after.

| Target | Files in the footprint | Applied | Already holding the change | Conflicts | Proof failures |
|---|---|---|---|---|---|
| windows/amd64 | 77 | 77 | 0 | 0 | 0 |
| linux/amd64 | 76 | 12 | 64 | 0 | 0 |
| darwin/amd64 | 75 | 11 | 64 | 0 | 0 |
| linux/arm64 | 76 | 8 | 68 | 0 | 0 |

The union is 108 files, 758 lines removed and 758 added, and every one is the `(uintptr)` prefix leaving a call that
returns an `unsafe.Pointer`. By package: `runtime` 80 files, `internal/runtime/maps` 8, `reflect` 4,
`internal/runtime/gc/scan`, `sync` and `time` 2 each, and 1 each in `encoding/json`, `fmt`, `internal/reflectlite`,
`internal/runtime/atomic`, `internal/sync`, `internal/syscall/execenv`, `net`, `sync/atomic`, `syscall`, `unique`
and `weak`. That is 17 packages; 16 have a roster row (`internal/syscall/execenv` has none).

**The hand-owned files.** Seven of the 108 are `.cs.auto` review siblings of hand-owned files, 24 sites between
them. A sibling is the converter's own emission, so it takes the hunk. Its hand-owned file is not regenerated, so
each site was read there:

| Sibling | Sites | The same line in the hand-owned file |
|---|---|---|
| `runtime/mfinal.cs.auto` | 1 | 1: line 315, `frame = (uintptr)mallocgc(framesz, nil, true);`. Carried in the same commit as the sibling's hunk, re-derived from it: `frame` is an `unsafe.Pointer` and `mallocgc` returns one on all four targets |
| `internal/sync/hashtriemap.cs.auto` | 11 | 0 |
| `sync/atomic/value.cs.auto` | 7 | 0 |
| `sync/pool.cs.auto` | 2 | 0 |
| `sync/poolqueue.cs.auto` | 1 | 0 |
| `time/tick.cs.auto` | 1 | 0 |
| `weak/pointer.cs.auto` | 1 | 0 |

The other 23 sites have no counterpart to carry: the hand-owned file does not hold the line.

**OWED, as its own census after cgo Phase 3 increment 3.1's re-gate:** every hand-written cast of a call's result to
`uintptr` where the call returns an `unsafe.Pointer`, in the hand-owned files and packages, TYPED and not textual.
The starting population is the 29 text hits in `runtime`'s hand-written companion files. A text hit is not a
finding: a call that returns a number is cast correctly.

**The build at `41067a0fb7`.** The 17 packages on four targets, build output purged before each target: 68 of 68
exit 0 with 0 errors. `runtime` builds on all four, which was the condition for the hand-own line.

**The reference.** `pointers.md` gains the rule (*A call that returns an `unsafe.Pointer` is emitted as the call
itself*), sampled from the behavioral program's golden, and its corpus sample of `runtime/alg.cs` loses the cast.
One provenance note that cites `pointers.md` by line was re-cited, 41 lines down.

**The owed question of section 7 (the Q49 drain): NOT redundant.** Its corpus case is `noescape`, which takes its
pointer through a `uintptr` and back. That is how Go hides a pointer from escape analysis, and the pointer it
returns carries nothing whatever the converter emits around the call. The drain stays, and `pointers.md` now says
why.

**The gates.** One stage at a time on H, build output purged after each, 0 tracked deletions, the tree clean.

| Gate | Tree | Reading |
|---|---|---|
| `go test ./...` | `41067a0fb7` | **rc 1.** 10 packages ok, 0 cached; `repoguard` FAILED in `TestDocsSamplesHoldTheirLines`: a second `pointers.md` sample, of `internal/runtime/atomic/types.cs`, still showed the cast on a line the application rewrote |
| `repoguard`, alone, `-count=1` | `0ddca05b39` | ok. The full `go test ./...` was not re-run on H at this tip: the difference is one docs line, and COORD's union battery reads it at the batch tip |
| GolibTests, full | `0ddca05b39` | base (master `651c4ad147`) 2,590 passed, 43 skipped of 2,633; seat 2,595 passed, 43 skipped of 2,638. The TRX difference is exactly the five `UnsafeAddCarryTests` arms |
| `check-no-regression` | `0ddca05b39` | rc 0, byte-identical across all 1,057 behavioral packages |
| the full behavioral suite | `0ddca05b39` | rc 0, PASS, 966 projects: Transpile, Compile and Target 966 of 966; Output 936 passed, 0 failed, 30 skipped, 0 timed out. No NOT MEASURED line. 966 is section 7's 959 plus the 7 projects batches 299 and 300 registered |

**The miss, and what it changed.** Eight docs samples in six pages are sourced from the 109 applied files. One
quoted a rewritten line, and the plan had listed only the sample it meant to change. **Before a corpus change is
applied, every docs sample sourced from the touched files is listed** (COORD, standing practice from this seat).

**The sweep, windows/amd64 half, on H at `0ddca05b39`.** 57 rows, one at a time, whole rows, the default
configuration: the 16 packages' rows, the five reflect canaries, and the rows with an allocation assert. The run
list was posted before the first row ran.

| Row | Verdicts | Row | Verdicts | Row | Verdicts |
|---|---|---|---|---|---|
| `bufio` | 80 | `bytes` | 84 | `context` | 62 |
| `crypto/cipher` | 27,289 | `crypto/ed25519` | 9 | `crypto/md5` | 14 |
| `crypto/rand` | 314 | `crypto/sha1` | 13 | `crypto/sha256` | 29 |
| `crypto/sha3` | 54 | `crypto/sha512` | 44 | `encoding/binary` | 143 |
| `encoding/json` | 550 | `fmt` | 62 | `hash/maphash` | 66 |
| `internal/reflectlite` | 30 | `internal/runtime/atomic` | 16 | `internal/runtime/gc/scan` | 19 |
| `internal/runtime/maps` | 19 | `internal/sync` | 106 | `internal/zstd` | 536 |
| `io` | 60 | `log` | 24 | `log/slog` | 228 |
| `log/slog/internal/buffer` | 1 | `mime` | 17 | `net/http/internal` | 14 |
| `net/netip` | 211 | `net/rpc` | 15 | `net/textproto` | 27 |
| `os` | 6,826 | `path` | 9 | `path/filepath` | 67 |
| `slices` | 122 | `sort` | 63 | `strconv` | 39 |
| `strings` | 68 | `sync` | 48 | `sync/atomic` | 108 |
| `syscall` | 66 | `testing` | 82 | `time` | 178 |
| `unicode/utf16` | 8 | `unicode/utf8` | 14 | `unique` | 27 |
| `weak` | 9 | `math/big` | 249 | `encoding/gob` | 106 |
| `crypto/rsa` | 590 | `crypto/internal/fips140test` | 2,270 | `database/sql` | 141 |
| `reflect` | 445 | `net` | 505 | `go/types` | 597 |
| `net/http` | 1,526 | `runtime` | 11,083 | `crypto/tls` | 1,393 |

57 of 57 PASS, each at its banked count. Every row's comparison record reads `validated`, matched, with 0 errors.
Two rows are named:

- **`crypto/tls` reads 1,393 = 4,920 banked − 3,527.** `TestBogoSuite` is over the deadline on H: the standing,
  disclosed host limit, unchanged by this seat. The hosted halves carry it.
- **`runtime` reads 11,083, and its `TestFinalizer*` tests read their banked verdicts:** 18 verdicts (6 tests and
  the 12 subtests of `TestFinalizerOrCleanupDeadlock`), pass in Go and pass converted. The row's disclosure file is
  untouched.

No row changed, for better or for worse. The linux (amd64 and arm64) and darwin halves run hosted on the same tip
and the same 57 rows (runs 43475901068 and 43475907692); COORD scores them, and they are not scored here.

**Found in the sweep's leftovers, not this seat's.** The `-tests` pass of `net/http` re-emitted one production
line of `net/http/h2_bundle.cs` (`processHeaders`, the handler passed to `scheduleHandler`): the committed file
wraps it as `new Action<…>(handler)`, and the converter emits `NilSafeDelegateConversion<…>(handler)`. Both arms of
this seat's footprint emit the second form, so it is drift the committed corpus already carried at master
`651c4ad147`, from the converter change that made a func-value wrap nil-safe. The hunk rule left it alone, as it
must. It was restored with the rest of the sweep's leftovers and is posted to COORD.

**The scores, as worded.**

| Prediction | Score |
|---|---|
| The footprint at `9b6562b21c` reads as it did at `1a6a97997a` | MET, on all four targets |
| `go test ./...` at the applied tip: rc 0, all ok, 0 cached | **MISSED**: `repoguard` failed on one docs sample |
| GolibTests: the seat's total is the base's plus the five arms, skipped unchanged | MET |
| `check-no-regression`: byte-identical | MET |
| The full behavioral suite passes, no NOT MEASURED line, 959 plus what batches 299 and 300 registered | MET |
| PB-8, windows/amd64 half: all 57 rows at their banked counts, `TestFinalizer*` at their banked verdicts | MET |
| PB-8, linux and darwin halves | NOT SCORED HERE: COORD's readings |

**Stop lines.** None fired on H. SB-1: no banked count moved. SB-3: no corpus line moved that is not the prefix's
removal, and no position-map line. SB-4: every package compiles on every target, and the 57 rows' test emissions
build. SB-7 stands as in section 7.

— H (2026-11-13)
