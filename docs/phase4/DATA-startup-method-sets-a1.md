# DATA: start-up method sets, arm A1 (receiver keys), with `error<T>`'s box lookup

Record, 2026-11-18, lane P5. Arm A1 of `docs/phase4/DESIGN-startup-method-sets.md` (amendments of 2026-11-13 and
2026-11-18), cut off master + batch 311 (`f36b6b0d48`) as the .5 seat COORD approved on 2026-11-17. Measured on one
cloud linux/amd64 VM (4 vCPU, .NET SDK 10.0.400), Release under the JIT, `GoTargetOS=linux`. Data and instruments:
`docs/phase4/data/startup-method-sets-a1/`.

## The change

golib only (`TypeExtensions.ExtensionMethodRegistry.cs`, one line of `TypeExtensions.GoMethodSets.cs`).

- The metadata walk records, for each extension host class, the **receiver key** (module and type-definition token)
  of every receiver its methods extend. A receiver the assembly defines is keyed from its signature; any other is
  loaded, as the walk already did to home it. A class keeps no key list (and is read for every question its homes
  admit, as before) when one of its receivers is a type parameter, an array, a pointer, a type of the core library
  or of golib, or a type the assembly derives from something other than `object`, `ValueType` or `Enum`.
- A question's keys are those of its element, of each type it derives from and of each interface it implements. A
  class is read only when its homes admit the question **and** its keys meet the question's or it has no key list.
  The readers then run their unchanged filters on what was read; the keys never decide an answer.
- Keyed readers: `CollectGoMethodSetCandidates`; `ExtensionMethodsOf` for a target that is not a box; and
  `GetBoxExtensionMethod`, `error<T>`'s own lookup, keyed by the pointee. `GetExtensionMethods(Type)` on a box stays
  whole-scan (M2).
- One function (`ScopedReceiver`) reads a receiver through a by-ref and through every pointer box, box subclasses
  included, as the method-set rule does; the home and the key both come from it. This moves the HOME of a receiver
  typed as a box subclass from golib to the pointee's assembly (below).
- Native AOT is unchanged: its listing carries no receivers, so every listed class has no key list (M1, BOARD row of
  this date).

## The guard

`src/tests/GolibTests/ExtensionReceiverScopeTests.cs` gains a fixture assembly, `l3fx.keys`, with one host class
per receiver shape design M3 names: `Nullable<point>`, a generic base (`basecls<int>`, extended by `derived`), a
generic interface (`gshape<int>`, implemented by `gcircle`), and each public pointer-box subclass over `point`
(`StandardBox`, `SlottedStandardBox`, `StaticBox`, `NativeBox`, `ElemRefBox`). M3 named a `FieldRefBox<T>`; golib
has none.

| test | what it holds |
|:--|:--|
| `EveryFixtureAnswerIsTheFullScans` (extended) | every fixture type, plus `Nullable<point>` and `point` in each box, answers both method sets and the extension list exactly as the whole scan does |
| `EveryReachableAnswerIsTheFullScans` (unchanged) | the same over every converted `src/core` assembly GolibTests reaches, with the fixture |
| `TheWalkKeysEachReceiverAsTheMethodSetRuleReadsIt` (new) | the walk's keys: none for `Nullable<point>`, the definitions for the generic base and interface, `point` (and `point`'s home) for each box subclass |
| `AQuestionReadsOnlyTheHostsOfItsKeys` (new) | each shape is reached by the question the rule matches it to, and a class homed with `point` whose receivers are not `point`'s is not read |
| `TheBoxLookupReadsOnlyTheHostsOfItsPointeesKeys` (new) | `GetBoxExtensionMethod(point, ж<point>, "Scale")` answers the bridge's `Scale` and does not read that class |

Red arms, each a one-site edit of the golib source, built and run against the four guard classes
(`ExtensionReceiverScope`, `ExtensionScanMetadata`, `ExtensionScanScope`, `ExtensionHostList`, 16 tests), then
restored byte-identical (sha256):

| arm | edit | red |
|:--|:--|:--|
| R1 (K3) | a question's interface keys dropped | 3 tests; reachable: 318 answers differ |
| R2 | a question's base-type keys dropped | 3 tests; reachable: 3 differ, all `derived` (fixture) |
| R3 | core-library receivers keyed (`Nullable<point>` by `Nullable<>`) | 4 tests; reachable: 3 differ, all `point` (fixture) |
| R4 | the home read through `ж<T>` only, as before the cut | 4 tests; reachable: 1 differs, `*point` (fixture) |
| R5 | the box lookup's pointee key dropped | `TheBoxLookupReadsOnlyTheHostsOfItsPointeesKeys` |
| R6 | keys not applied (homes only) | the two read-narrowing tests |

R2 to R4 redden the reachable test only through the fixture: no converted assembly declares a receiver of those
shapes, so no converted answer depends on them. R4 is the finding in the change list: before the cut, a receiver
typed `NativeBox<T>` (or any box subclass) was homed in golib and a question about `T` skipped its class.

## Scored against the re-prediction posted before the cut (P5, 2026-11-18 01:03Z)

Probe and wall arms: `base` is master + 311, `cut` is master + 311 + this seat's golib diff, both with the same
minimal probe (`cut/probe.py`). Program output byte-identical in every arm; the candidate counts are identical (2 in
BigAst, 197 in wide).

| | base | cut | predicted | score |
|:--|--:|--:|:--|:--|
| K1 BigAst host classes read | 222 | 1 | 1, falsifier more than 3 | MET |
| K1 BigAst methods read | 114,922 | 2 | at most 10 | MET |
| K1 BigAst `CollectGoMethodSetCandidates` | 360.2 ms | 102.0 ms | 90 to 120 ms | MET |
| K1 BigAst wall, 30 rounds, paired median (quartiles) | | -297.2 ms (-359.4 to -226.3), 30/30 faster; A/A +1.9 (-56.4 to +71.2) | -200 to -330 ms | MET |
| K2 wide host classes read | 333 | 14 | 10 to 20, falsifier more than 50 | MET |
| K2 wide methods read | 3,138 | 207 | under 400 | MET |
| K2 wide wall, 100 rounds, paired median (quartiles) | | -35.1 ms (-71.2 to +15.8), 69/100 faster; A/A -5.7 (-29.7 to +35.7) | median -20 to -70 ms | MET |
| K2 wide wall, upper quartile | | +15.8 ms | below 0 | **MISSED** |

The K2 upper-quartile miss is scored as worded. A second 100-round sample, taken after scoring and not a rescore,
read -31.4 ms (-72.3 to +9.2), 67/100 faster, A/A +9.3: the median gain repeats, and single pairs on this VM spread
wider than the gain. Step 1's one-binary arm read -46.9 ms (-86.8 to -10.0) for the same change.

K3 (equivalence and red arms): MET, above. K4 (footprint): 0 generator lines, 0 converter lines, 0 corpus lines;
golib, GolibTests and records only. K5 (TypeScript hello on P4's box) and K6 (hosted linux/arm64) are owed after the
push and are reported in the lane's posts.

## Gates, this VM (linux/amd64)

| gate | configuration | result |
|:--|:--|:--|
| GolibTests | Release, `GoTargetOS=linux` | 3,036 total: 2,985 passed, 0 failed, 51 skipped |
| GolibTests | Debug, `GoTargetOS=linux` | 3,036 total: 2,960 passed, 0 failed, 76 skipped; no Release-only skip; the 25 Debug-only skip rows (24 tests) each sit in a file carrying the JIT-optimizer gate (frame, allocation and stress classes), none in the extension-scan classes |
| GenTests | Release | 572 passed, 0 failed |
| `go test ./internal/repoguard` | `GO2CS_GUARD_NO_DOTNET=1`, at the committed head | ok |

Not run on this VM, and why: the full behavioral runner (a golib seat's train leg; 14 GB free against the battery's
25 GB floor); CNR and the full converter `go test ./...` (no Go source and no converter change, so no emission can
differ); the end-to-end `go2cs.slnx` build (no public golib API changed: the new members are internal).

## What this does not show

- Native AOT: no change by construction (M1); no AOT publish was measured.
- The gain on a program with none of these questions at start-up; the TypeScript hello (K5) is the next reading.
