# DATA: start-up method sets, arm A2 (`error<T>` asks only its own type's homes)

Record, 2026-11-14, lane P5. Arm A2 of `docs/phase4/DESIGN-startup-method-sets.md` (on
`claude/p5-startup-method-sets` until it lands), cut off master `651c4ad147` as its own seat, as COORD's design
review ruled. Measured on one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400), Release under the JIT,
`GoTargetOS=linux`.

## The change

`error<T>`'s static constructor found the pointer `Error` with `typeof(ж<T>).GetExtensionMethod("Error")`. A box
target normalises to the box's generic definition, so that one question read every extension method of every
loaded package. It now asks `ErrorBinding.PointerMethod`:

- a non-generic `T`: `TypeExtensions.GetBoxExtensionMethod`, the method named `Error` whose receiver is exactly
  `ж<T>`, read from the hosts of `T`'s own homes;
- a generic `T`: the method-set candidate source (`AdapterBinder.ResolveReceiverMethods`), closed over `T`'s type
  arguments, which is what the old code fell back to when its first answer was an open generic method.

The by-value lookup is unchanged. `GetExtensionMethods(Type)` and every other box question still read the whole
scan (must-fix M2 of the review).

## The guard, and the one difference it discloses

`src/tests/GolibTests/ErrorBoxLookupTests.cs` compares what `error<T>` BINDS (the method, when `error<T>`'s own
pointer delegate type accepts it), not the raw `MethodInfo`.

| test | what it holds |
|:--|:--|
| `EveryReachableBindingIsTheWholeScans` | every non-generic type of every converted `src/core` assembly GolibTests reaches, plus the fixture: 6,998 types asked, 49 bind a pointer `Error`, 0 differ from the old lookup |
| `APointersErrorIsReadFromItsOwnPackage` | the type's own method is found and no other package's host class is read |
| `ATypeWithoutAPointerErrorBindsNone` | the old lookup answered another type's box `Error` here, which the delegate refused; both bind nothing |
| `AGenericTypesPointerErrorDoesNotDependOnScanOrder` | THE DIFFERENCE, below |

**The difference.** A generic type's pointer `Error` is declared open (`Error<K>(this ж<wrap<K>>)`), so no receiver
equals the asked box. The old lookup then took the first box `Error` of the whole scan. It bound the type's own
method only when that method happened to be the first one scanned, and bound nothing otherwise (another type's
method does not fit the delegate). The new lookup binds the type's own method in every scan order. So a generic
type whose only `Error` has a pointer receiver could fail `error<T>` construction with "does not implement
error.Error" depending on which packages were loaded and in what order; it no longer can. The reachable guard
does not cover generic instantiations (no closed box exists for a definition); the fixture test is their case,
in both orders.

Red first: with `PointerMethod` put back to the whole-scan lookup, the three fixture tests fail by name (an
unrelated package read; another type's method answered; the generic type unbound when the other package is scanned
first) and the reachable test stays green, as it must when both sides are the same lookup. Restored byte-identical
(sha256).

## Measurement against the amendment's prediction

Probe build (the design's `b3-probe.patch`, never merged) of the program `wide`, one run each:

| | master | A2 | predicted |
|:--|--:|--:|:--|
| host classes read | 152 | 41 | |
| extension methods read | 5,931 | 3,138 | 3,100 to 3,500 |
| `ExtensionMethodsOf`, inclusive | 2 calls, 3,560 methods, 46 ms (87 ms in this session) | 1 call, 0 methods, 5.2 ms | under 6 ms |

Wall time, interleaved pairs (A then B, order swapped each pair, probe off, 4 warm-up pairs discarded). This
session's VM is slower than the one the prediction was made on (`wide` 825 to 850 ms against 612 to 651 ms), so
read the deltas against the A/A control beside them, not against the earlier absolute numbers.

| program | pairs | master median (min) | A2 median (min) | paired difference, median (quartiles) | A2 faster |
|:--|--:|--:|--:|:--|--:|
| wide | 100 | 840 (749) | 792 (705) | -65.5 ms (-96.1 to -3.5) | 76/100 |
| wide | 40 | 851 (776) | 824 (721) | -41.8 ms (-118.3 to +7.4) | 29/40 |
| wide, A/A control | 100 | 825 (753) | 845 (752) | +14.3 ms (-27.7 to +46.7) | 41/100 |
| wide, A/A control | 40 | 846 (778) | 842 (764) | -8.6 ms (-49.9 to +32.1) | 24/40 |
| hello | 40 | 476 (440) | 463 (434) | -5.7 ms (-41.2 to +15.7) | 22/40 |
| stringer | 40 | 624 (570) | 613 (560) | -6.1 ms (-36.6 to +19.6) | 22/40 |

Scored as worded: methods read HIT; `ExtensionMethodsOf` HIT; hello and stringer within noise HIT. Wall on `wide`
was predicted at -25 to -45 ms. The readings are -44 ms by minimum, -48 ms by median and -42 to -66 ms by paired
median, with the A/A control at -9 to +14 ms. That is at the top of the predicted range or a little past it, on a
VM about 30% slower than the predicting one. Neither falsifier fired (a gain under 15 ms, or more than 4,000
methods read). The program's output is byte-identical before and after.

## Gates, this VM (linux/amd64)

| gate | configuration | result |
|:--|:--|:--|
| GolibTests | Release, `GoTargetOS=linux` | 2,688 total: 2,646 passed, 0 failed, 42 skipped |
| GolibTests | Debug, `GoTargetOS=linux` | 2,688 total: 2,621 passed, 0 failed, 67 skipped |
| GenTests | Release | 526 passed, 0 failed |
| `go test ./internal/repoguard` | `GO2CS_GUARD_NO_DOTNET=1` | ok |

Not run on this VM, and why: the full behavioral runner and the end-to-end build of `go2cs.slnx` (13 GB free
against the battery's 25 GB floor); `go test ./...` beyond repoguard and CNR (no Go source changed). The hosted
`os-matrix` legs for this tip are reported in the lane's post, not here.

## What this does not show

- No converter or generator source changed, so no emission can differ.
- Native AOT: `error<T>` reaches the same `GetExtensionMethodsHomedIn` path under both; no AOT publish was
  measured here.
- The gain is on a program that takes the `error<T>` pointer path at start-up. A program that never constructs
  `error<T>` for a pointer-held type sees none (hello, stringer).
