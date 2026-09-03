# DESIGN — descriptor cargo at element positions

> **Status.** Design record. No code. Two increments sized below; the arc's cut waits on this file.
> **Root sentence:** *cargo is applied at the position that owns it and dropped on the way to the
> element.*

## 1. What "cargo" means here

A Go type carries facts the managed type does not. `array<T>` has no length, `channel<T>` has no
direction, and a synthesized struct descriptor has no field list — so those facts travel beside the
`System.Type` as **descriptor cargo**: `arrayDims`, `GoChanDir`, `keyDims`, `structType.Fields`.

The machinery for this already exists and works. `canonType` interns on dims (so `[4]byte` and
`[8]byte` are distinct Go types), `[GoArrayDims]` stamps struct fields, and `GoTypeName` takes all
four cargo slots as parameters.

What does not work is the **hand-off**, and it fails in TWO places — measured 2026-09-03, and the
second one corrects this record's own first framing:

**(a) The container CONSTRUCTOR never records the element's cargo.** This is the root.

    ArrayOf(6, uint8).String()      [6]uint8     <- the element KNOWS its length
    ArrayOf(6, uint8).Len()         6
    SliceOf(that).String()          [][]uint8    <- lost INSIDE SliceOf
    SliceOf(that).Elem().String()   []uint8
    SliceOf(that).Elem().Len()      0

`SliceOf` is handed a type that knows `6` and produces one whose element does not. Instrumenting
`GoTypeName`'s slice arm confirms it from the other side: the cargo arriving there is
**`dims=null`** on BOTH routes — `ValueOf(sliceOfArrays)` and the explicit
`SliceOf(ArrayOf(6, uint8))`.

**(b) The RENDERER would drop it even if it were there.** Every consumer applies the cargo at the
position that owns it and recurses into the element with the cargo-less overload.

**Neither half alone suffices**, and the ordering matters for anyone executing this: fixing (b)
first threads a `null` and changes nothing, which would read as "the fix does not work" and invite a
hunt in the wrong layer. (a) is the necessary first half. The code's shape strongly suggests (b) is
the whole story — the slice arm sits one line above a map arm that threads correctly — and that
reading is wrong.

## 2. The three measured instances

All measured against go1.23.12 on the converted corpus; none inferred.

### 2.1 Array dims at element positions

| shape | Go | converted |
|:--|:--|:--|
| `[6]uint8` | `[6]uint8` | `[6]uint8` |
| `[2][3]int` | `[2][3]int` | `[2][3]int` |
| **`[][6]uint8`** | `[][6]uint8` | **`[][]uint8`** |
| **`[][3]int`** | `[][3]int` | **`[][]int`** |
| **`[][2][3]int`** | `[][2][3]int` | **`[][][]int`** |
| **`map[[2]int][]int`** | `map[[2]int][]int` | **`map[[]int][]int`** |
| **`[]*[4]byte`** | `[]*[4]uint8` | **`[]*[]uint8`** |
| `[]Grid` (NAMED array) | `[]main.Grid` | `[]main.Grid` |
| `Elem().String()` / `.Len()` | `[6]uint8` / `6` | `[]uint8` / `0` |

Three things this table settles.

A nested array at TOP level is CORRECT, so the `arrayDims[1..]` recursion works and nothing in it
needs designing. `[][2][3]int` loses BOTH levels, so a fix applied at one level would render
`[][3]int` and look plausible while still being wrong. And a NAMED array element is correct, because
a defined type is named through the named-type path and never consults cargo — so the defect's
boundary is **unnamed arrays in element position**, not "slice of array".

### 2.2 Struct Fields on a synthesized descriptor

`reflect.TestFuncLayout`, the one signature whose parameter is a struct:

    funcLayout(...).size=0, argsize=0, retOffset=0, stack=[], gc=[]
    want                     32        32           32         [0 0 1 1]  [0 0 1 1]

Rooted four layers down, a measurement at each:

| layer | measurement |
|:--|:--|
| `funcLayout` | every field derives from `newAbiDesc` — not the root |
| `InSlice()` | **`len=1`** for the failing signature — refuted the first hypothesis |
| `addArg` | struct goes to `regs`; every other argument of every other signature goes to `STACK` |
| `regAssign` Struct arm | **`size=32` (correct), `Fields.Length=0`** — the root |

    else if (exprᴛ1 == Struct) {
        var st = Ꮡt.Reinterpret<abi.Type, structType>();
        foreach (var (i, _) in (~st).Fields) { ... }   // empty -> body never runs
        return true;                                   // -> "all fields in registers", zero steps
    }

An empty field list makes the loop vacuous and the `return true` claims success, so
`@in.stackBytes` stays 0 and five assertions fail from one silent win.

### 2.3 Channel direction at the element

`reflect.TestTypes`:

    #20  have "chan<- chan string"   want "chan<- <-chan string"
    #21  have "<-chan chan string"   want "<-chan <-chan string"
    #22  have "chan chan string"     want "chan (<-chan string)"

Both channel arms of `GoTypeName` render their element with the cargo-less overload:

    string elem = GoTypeName(t.GetGenericArguments()[0]);           // direction-carrying arm
    if (gd == typeof(channel<>)) return "chan " + GoTypeName(a[0]); // plain arm

## 3. The core table — AUTO paths that read a blob

For each: what it reads, and **which surface already holds the right answer**. The last column is
what makes the fix tractable — in every measured case the datum exists on the delegate- or
instance-derived surface, and only the blob-reading path is blind.

| path | reads | surface that already knows |
|:--|:--|:--|
| `GoTypeName` slice/channel/map arms | element cargo (dropped) | the caller's own cargo, one frame up |
| `regAssign` Struct arm | `structType.Fields` | `TypeOf(S).NumField()` = 4, `.Size()` = 32 |
| `regAssign` Array arm | `arrayType.Len` | `Type().Len()` on a dims-carrying descriptor |
| `addTypeBits` | the same struct/array blobs | as above |
| `funcLayout` | `newAbiDesc`'s result | `In(i).Size()` returned 32 here |
| `Elem()` / `Len()` on a synthesized descriptor | element cargo | **nothing** — see §6 |
| `FuncOf(...)`'s rendered signature | element cargo | `FuncOf([3]int)` prints `func([]int)` |
| `%v` of a `reflect.Type` | — | prints its box, not `String()` — unrooted, §6 |

`InSlice()` is deliberately NOT in this table: it was measured CORRECT (`len=1`) and is the one
blob-reading path confirmed populated. Recording that is the point — the boundary is not "all blobs",
and a design that assumed it would be wrong in the safe direction but wrong.

## 4. Two rules that are not cargo

Carried here as their own rules so they are not rediscovered as consequences.

**4.1 Parenthesisation.** Go parenthesises a directional element under a bidirectional parent:
`chan (<-chan string)`, not `chan <-chan string`. A rendering rule, independent of threading; it must
land with the channel work or `#22` stays red after `#20`/`#21` go green.

**4.2 Unexported interface method qualification.** Go qualifies an interface's UNEXPORTED method
names with their package in the type string — `interface { reflect_test.a(...); reflect_test.b() }`.
This is name qualification, not cargo, and lives in the interface type-name path. It is
**increment two's own line item with its own row** (`TestTypes` case 34).

## 5. Rulings

**R1 — "empty means cannot see" must never be a successful arm.** Until the cargo is populated, the
struct and array arms of `regAssign` throw, naming the descriptor and the arm, so a mis-assignment
cannot pass silently. This lands with increment one, not before: a red row is red either way and the
honest one is the throw.

**R2 — ANSWERED 2026-09-03: the array-parameter row CANNOT be provoked, and that IS the result.**
The Array arm has the identical `if (Len == 0) return true;` shape, so it was predicted to fail as
the struct does. It does not fail — it is never reached.

`funcLayout`'s only live caller is `export_test`'s `FuncLayout` wrapper. The auto `MakeFunc` whose
line 63 calls it is displaced by the registry, and every other route needs `flagMethod` — which
**nothing in the package assigns**. Verified rather than quoted: all 19 `flagMethod` references
(`value.cs` x13, `value_impl.cs` x2, `makefunc*` x4) are READS, guards and shifts.
`makefunc_impl.cs` says the same in its own comment ("no Value ever takes that path").

Measured with a standalone probe against a directly-built `reflect` — deliberately NOT through
`-tests`, which re-converts `abi.cs` and would wipe the instrument before the build (the false-empty
that cost one wrong root earlier the same day).

**Consequence, and it is a downgrade of this arc's own claim.** The Struct arm's vacuous-true is
exercised ONLY by `TestFuncLayout`; the Array arm not at all. So `TestFuncLayout`'s 2 rows are
**test-only reachability** — fixing them makes the row honest, it does not fix a production
behaviour. The dims and channel-direction rows are NOT in that position: `Type().String()` and `%T`
are printed by production code corpus-wide. **The two halves do not share a justification and must
not borrow one from each other.**

R1 is unaffected and re-read in that light: making the arms loud costs production nothing, because
nothing production reaches them, and converts a future silent pass into a throw. **Cheap insurance,
not a fix** — and worth landing on exactly that basis, stated so nobody later reads it as the latter.

## 6. Open, and honestly so

- **`Elem()` on a TYPE has no instance to read.** Every other consumer can be fed from a value; a
  pure type operation cannot. Whether the cargo reaches it decides whether `Elem().Len()` is fixable
  at all, and it is the first thing increment one must establish.
- **`%v` of a `reflect.Type` prints its box** — `funcLayout(0x21ea5fa88d0, <nil>)` where Go prints the
  type. Same family (a Type that knows its name where something prints its address), but the
  mechanism is unrooted and it is NOT claimed as a fourth instance.
- **Whether `DeepEqual`'s descriptor compare is a consumer.** It compares by identity and `canonType`
  already interns on dims, so it is probably NOT affected. Unmeasured; increment one confirms or
  refutes rather than assuming.

## 7. Increments and gates

**Increment 1 — cargo to element positions** (array dims, channel direction, map key and value),
R1's loud arms, R2's probe.

**Increment 2 — the two §4 rules**: parenthesisation, and unexported interface method qualification.

Acceptance is the behavioral guard `SliceOfArrayTypeName` — written, RED, parked with its
`go2cs.slnx` registration verified at 705 projects: nine shapes plus the `Elem()` pair, `%T` and
`Type().String()` on each, compared against `go run`. Its nested rows are what stop a one-level fix
landing.

Gates, because this is golib on the boxing path: `go2cs.slnx`, GolibTests, the five
largest-reflect-importer canaries derived at gate time (`crypto/tls`, `net/http`, `go/types`,
`encoding/json`, `net` — derived 2026-09-03, control `encoding/json` IN / `cmp` OUT), the behavioral
**Output** phase (not only Compile — a `%T` change shows first in the stdout comparisons against
`go run`), the `nistec` **cost canary** against its recorded wall, and union CNR.

Rows: `TestDeepEqualAllocs` (2 — fix-then-disclose, the family entry earned from its OWN
results-file signature, never from resemblance), `TestFuncLayout` (2), `TestTypes` (1, gated on BOTH
increments).
