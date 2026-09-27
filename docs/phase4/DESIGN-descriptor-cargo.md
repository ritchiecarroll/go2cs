# DESIGN — descriptor cargo at element positions
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

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

**R1 — an arm that cannot read its cargo must say so, not succeed.** Both arms of `regAssign` throw,
naming the descriptor and the arm, so a mis-assignment cannot pass silently.

The original wording was *"empty means cannot see"*, and it was **false for the legal empties**:
`struct{}` is ubiquitous and legitimately has no fields, `[0]T` is legal Go, and Go's own comment on
the array arm says its vacuous `true` is correct for that case. A throw on emptiness alone is a
regression wearing insurance's clothes. The predicates are therefore narrow, and each was measured
against the legal value it must pass BEFORE it was written to throw:

| arm | predicate | passes untouched | throws |
|:--|:--|:--|:--|
| Struct | `Fields.Length == 0 && Size() > 0` | `struct{}` (0 fields, 0 size) | `reflect_test.S` (0 fields, **32** bytes) |
| Array | `Len == 0 && arrayDims is null` | `[0]T` (`arrayDims [0]`) | unknown-length element (`arrayDims null`) |

The array arm was briefly deferred on the belief that no discriminator existed — `Len` and `Size` are
both 0 on both shapes. That was an arm-level reading, not a model-level one: the datum is on
`arrayDims`, whose declaration states the rule (*"Null = unknown ([0]T is [0])"*). The deferral is
withdrawn; see the retraction in §8.2.

**Reachability, so this is not read as a production fix:** R2 shows `funcLayout` is reached only from
`export_test`, so neither arm is on a live path. This is insurance against a future silent pass.

**Blocked, not by design:** displacing `regAssign` — a `[GoRecv]` method with a REF receiver — makes
the converter emit a box-form call (`Ꮡa.regAssign`) into a `ref` body where no box exists (CS0103).
Every prior reflect displacement took a value receiver, so the shape was unexercised. A converter fix
is dispatched separately; the parked `abi_impl.cs` carrying both arms is its acceptance, and R1 lands
when it does.

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

## 7. The interning key, censused (2026-09-03)

`canonType`'s key is **`(System.Type, dimsKey)`**, where

    dimsKey = abi.descriptorDimsKey(arrayDims, funcParamDims, chanDir, keyDims)

Four cargo slots, and the comment beside them already states this arc's problem in another kind's
words: `funcParamDims` exists because `func([32]byte) bool` and `func([64]byte) bool` are ONE managed
delegate type, so "the first to intern would answer `In(0).Len()` for both".

**So the positional model can already express per-element cargo. The slots are not the gap.**

### The measured asymmetry

| kind | CONSTRUCTED route | DECLARED route | identity | who carries the cargo |
|:--|:--|:--|:--|:--|
| slice | `[][]uint8`, no dims | `[][]uint8`, no dims | TRUE | **neither** |
| pointer | `*[]uint8`, `Elem().Len()`=0 | `*[6]uint8`, `Elem().Len()`=6 | **FALSE** | **declared only** |
| map key | `map[[2]int]int`, `Key().Len()`=2 | `map[[]int]int`, `Key().Len()`=0 | **FALSE** | **constructed only** |
| func param | `In(0).Len()`=0 both | — | TRUE | **neither** (`FuncOf` never fills the slot) |

### Why — three mechanisms, three coverages, not four choices

This table first read as four sites each having chosen locally, which is how it was recorded.
**That was a description standing in for a cause.** There are three sources of cargo, each documented,
each with its own coverage, and every asymmetry above is an intersection:

| source | covers | so |
|:--|:--|:--|
| `abi.TypeOf` measures from a value | *"an ARRAY value and a POINTER's pointee ONLY"* (its declaration) | pointer's DECLARED route carries; slice's does not |
| constructors receive `ΔType`s | `ArrayOf`, `PointerTo`, `MapOf` | map key's CONSTRUCTED route carries |
| `fieldDimsCargo` stamps a field | pointer, map — **not slice, not chan** (its walk has no slice case) | field-derived slices carry nothing |

Nobody chose four times. Pointer is covered by the value-measuring source and not the field one; map
key by the constructor source; slice and chan by none. **Pointer and map key look like mirror images
because they are covered by DIFFERENT sources, not because two authors disagreed.**

The practical consequence is that the repair is two-sided: golib alone cannot fix a kind whose
converter-side source never stamps it, and the converter alone cannot fix a renderer that drops what
it is handed.

Two consequences fall out:

**Identity fails in BOTH directions.** Slice and func param are UNDER-distinct — `[][6]uint8` and
`[][8]uint8` intern as one Type (§2.4), which is what defeats `DeepEqual`'s type guard. Pointer and
map key are OVER-distinct — the constructed and declared forms of the SAME Go type are two Types,
which breaks the property `SliceOf`'s comment is protecting, on kinds that comment does not cover.

**`SliceOf`'s "record none" is internally consistent and locally right.** It chose symmetry
(both routes carry nothing) over asymmetry, which is why slice identity holds where pointer's and map
key's do not. The residual it names is real; what the census adds is that the OTHER kinds did not
make the same choice, so no global invariant exists to appeal to.

### What gob actually keys on — measured, and it adds a gate

`encoding/gob` keys **directly on `reflect.Type` identity**:

    var userTypeCache sync.Map                       // map[reflect.Type]*userTypeInfo
    var types = make(map[reflect.Type]gobType, 32)

So the `SliceOf` comment names a real dependant, not a hypothetical one — gob is the consumer whose
behaviour changes if two Go types stop being one `reflect.Type`, or start being two.

**And `encoding/gob` is a BANKED row: 106 verdicts, green today, with the collapse present.** Two
things follow, and they point opposite ways:

- The collapse does **not** break gob today. Its suite passes with `[][6]uint8` and `[][8]uint8`
  interning as one Type, which means gob's 106 tests never exercise the collapsing shapes. **gob is
  therefore a canary against BREAKING what works, not a detector of the defect** — it cannot go red
  on the current bug, only on a repair that damages identity.
- Any model increment owes an `encoding/gob` sweep. It is NOT in the five largest reflect importers
  (106 verdicts, well below `net` at 472), so the rank-derived canary set would miss it. It belongs
  on mechanism, exactly as `net/http` did for the promoted-forwarder change: **the arc alters type
  identity, and gob is the banked consumer that keys on type identity.**

### Why this argues for the tree-shaped model

A container descriptor referencing its element's CANONICAL descriptor gets both properties by
construction rather than by discipline:

- `ArrayOf(6,u8)` and `ArrayOf(8,u8)` are already distinct descriptors, so any container keyed on its
  element inherits that distinctness — `[][6]` != `[][8]` **without** a per-kind rule.
- Both construction routes reach the same element descriptor, so
  `SliceOf(elem) == TypeOf([]T{})` holds **without** each kind choosing a side.

The positional vector is not wrong; it is per-kind, and a per-kind mechanism is exactly what produced
three different local answers.

## 8. The tree model

### 8.1 The rule, in one sentence

**A container descriptor references its ELEMENT'S CANONICAL DESCRIPTOR, and interns on it.**

That is the whole change. Everything below is consequence.

### 8.2 Why both properties fall out, rather than being maintained

| property | how the tree gets it |
|:--|:--|
| `[][6]` != `[][8]` | `ArrayOf(6,u8)` and `ArrayOf(8,u8)` are **already** distinct descriptors (measured: distinct, named right, `Len()` 4/8). A container keyed on its element inherits that distinctness with no per-kind rule. |
| `SliceOf(elem) == TypeOf([]T{})` | Both routes reach **one** element descriptor, so both produce one container descriptor. No kind has to choose a side. |

**RETRACTED 2026-09-03 — a third row stood here and it was invented.** It claimed that
"unknown" != "zero" is something the tree buys and the positional model cannot express.
**The positional model expresses it today.** `arrayDims` distinguishes them by its own
declaration -- *"Null = unknown ([0]T is [0])"* -- and the two measure as DISTINCT reflect.Types:

    ArrayOf(0, uint8)                   Len 0 / Size 0 / arrayDims [0]     String "[0]uint8"
    SliceOf(ArrayOf(6,uint8)).Elem()    Len 0 / Size 0 / arrayDims null    String "[]uint8"
    equal?  FALSE

What could not tell them apart was `regAssign`'s ARRAY ARM, which reads `Len` and `Size` and never
`arrayDims` -- an arm consulting two accessors when the datum is on a third, not a limit of the
model. The tree is carried by the two properties above and no others.

**Consequence for R1 (§5):** its array arm is implementable now, on `Len == 0 && arrayDims is null`,
beside the struct arm's `Fields.Length == 0 && Size() > 0`. Its deferral to this increment rested on
the retracted claim and is withdrawn.
### 8.3 What happens to the positional vector's existing consumers

The vector does not have to be removed, and the section deliberately does not propose removing it.

**`Elem()`'s head-consumption.** Today `Elem()` on a non-pointer, non-map descriptor consumes the
head of `arrayDims` and hands the tail down — measured working: `ArrayOf(2, ArrayOf(3,int))` renders
`[2][3]int`, `.Elem()` gives `[3]int` with `Len()` 3. Under the tree, `Elem()` **returns the element
descriptor** instead of deriving one, so the head-consumption becomes dead for kinds that carry an
element reference. It must remain for any kind that does not, and the increment must state which
those are rather than assuming none.

**`canonType`'s key.** Today `(System.Type, dimsKey)` over four slots. The tree adds the element
descriptor's identity to the key for container kinds. Note this REPLACES rather than supplements the
per-kind slots for those kinds — keeping both would let a container intern two ways and reintroduce
the split that `pointer` and `map key` show today.

**The `[GoArrayDims]` field stamp.** Unaffected and still required. It answers "what is the length of
THIS struct field's array type", which is a question about a field, not about a container's element,
and no element reference exists to carry it.

**`funcParamDims` — the falsifier, ANSWERED 2026-09-03: the tree subsumes it. PASSES.**

It is the one slot already shaped like per-element cargo, so it is the model's sharpest test. Its
declaration states why it exists: *"the parameter position is the one place no other dims source
reaches — a `[32]byte` parameter has no value to measure and no field initializer to read, and the
emitted delegate type is a bare `Func<array<byte>, bool>` that `func([32]byte) bool` and
`func([64]byte) bool` share — so the converter stamps `[GoArrayDims]` on the parameter and
`GoReflect.FuncParamDims` reads it back off the delegate INSTANCE."*

Under the tree the **SOURCE is unchanged** — still the `[GoArrayDims]` stamp read off the delegate —
and only the **STORAGE** changes, from a positional `nint[]?[]?` to a reference to each parameter's
canonical descriptor. Three checks:

- **Expressiveness.** A per-parameter dims VECTOR (`[2,3]` for a `[2][3]int` parameter) is subsumed by
  a reference to `ArrayOf(2, ArrayOf(3,int))`, which carries the same lengths and more (kind, size,
  element type). Strictly richer, never poorer.
- **The descriptors exist.** `ArrayOf(32,u8)` and `ArrayOf(64,u8)` are measured distinct, so the
  references the tree needs are already constructible and already distinguishing.
- **`FuncOf` gets easier, not harder.** It receives its parameters AS `ΔType`s, so referencing them is
  natural — where today it does not populate `funcParamDims` at all (measured:
  `FuncOf([32]byte)bool == FuncOf([64]byte)bool`, `In(0).Len()` 0 on both).

**The one limitation carries over unchanged and is not made worse:** RESULT dims are unavailable
under either model, because a multi-result Go func returns a `ValueTuple` with no per-element
attribute position. The tree does not fix that; it also does not introduce it.

So the section survives its own falsifier. Had it not, this line would say so instead.

### 8.4 What must be measured before the increment cuts

- **What gob keys on** — measured (§7): `reflect.Type` identity directly, and gob is banked at 106
  green WITH the collapse, so it is a canary against damage and not a detector.
- **`DeepEqual`'s compare — MEASURED 2026-09-03, and it corrects §6's guess.** §6 listed it as
  "probably NOT affected, since it compares descriptors by identity". It does not compare descriptors
  at all: `deepValueEqualBoxed` opens with

      if (!AreEqual(v1.Type(), v2.Type())) { return false; }

  a CANONICAL TYPE comparison, mirroring Go's own `if v1.Type() != v2.Type()`. So `DeepEqual` is a
  **Type-identity consumer**, which means (i) it is directly affected by this arc, (ii) the tree FIXES
  it — the guard starts firing where the collapse silenced it — and (iii) it cannot be broken subtly
  by descriptor-internals changes, because it never reaches into them. The one consumer measured as
  the arc's victim is also the one whose repair path is the simplest.
- **The `pointer` and `map key` OVER-distinct rows**: the tree must make them equal, and they are the
  two rows that would silently stay broken if the increment only addressed the collapse.

### 8.5 The risk this section is most wary of

The tree changes type IDENTITY, and identity has a banked consumer that **cannot detect the current
bug but can be broken by the repair** (gob, §7). So the increment's acceptance is not "the names are
right" — the name guard would pass a repair that split the canonical type in two. It is the
**identity guard** (`CanonicalTypeIdentity`, written, currently RED on 3 of 9 rows) plus the gob
sweep. Names are the symptom and must not be the gate.

## 9. Increments and gates

**Increment 1 — cargo to element positions** (array dims, channel direction, map key and value),
R1's loud arms, R2's probe.

**Increment 2 — the two §4 rules**: parenthesisation, and unexported interface method qualification.

Acceptance is the behavioral guard `SliceOfArrayTypeName` — written, RED, parked with its
`go2cs.slnx` registration verified at 705 projects: nine shapes plus the `Elem()` pair, `%T` and
`Type().String()` on each, compared against `go run`. Its nested rows are what stop a one-level fix
landing.

Gates, because this is golib on the boxing path: `go2cs.slnx`, GolibTests, the five
largest-reflect-importer canaries derived at gate time (`crypto/tls`, `net/http`, `go/types`,
`encoding/json`, `net` — derived 2026-09-03 from PARSED IMPORTS, controls `encoding/json` IN / `cmp` OUT /
`go/doc/comment` OUT -- the third is load-bearing, see §10.3), the behavioral
**Output** phase (not only Compile — a `%T` change shows first in the stdout comparisons against
`go run`), the `nistec` **cost canary** against its recorded wall, and union CNR.

Rows: `TestDeepEqualAllocs` (2 — fix-then-disclose, the family entry earned from its OWN
results-file signature, never from resemblance), `TestFuncLayout` (2), `TestTypes` (1, gated on BOTH
increments).

## 10. Increment A, measured (2026-09-03)

Increment A is the `Elem()` hand-down fix: slices and channels leave the *consuming* arm and pass
their dims down unshifted beside pointers and maps, and the converter stamps a slice's and a
channel's element dims at struct-field positions.

### 10.1 The corpus footprint is one stamp, on a real production shape

A two-seeded diff — both roots seeded at 3679 `.cs`, both emissions writing 1656 fresh files, PRE
built from `e8c078637` and CUT from `b3caf3fa0` — differs in **two paths**:

```
internal/trace/internal/oldtrace/parser.cs        + [GoArrayDims(524288)]
internal/trace/internal/oldtrace/package_info.cs  (its position-map consequence)
```

The stamped field is `buckets []*[eventsBucketSize]Event` — a slice of pointer to array, the shape
`elementArrayDims` unwraps. Every hunk is the predicted class and there is no hunk outside it.

The part worth recording is not the size but the **location**: every prior instance of this defect
was in a probe or guard written to find it. `internal/trace`'s event buckets are production corpus
code, so the collapse was reachable outside the shapes built to provoke it.

### 10.2 The production diff is structurally blind to three banked rows

`-stdlib` never writes test emission, so the two-seeded diff cannot see a stamp that lands in a
`_test.go`-derived file. A syntactic census (`go/parser`, struct-field positions only —
`visitStructType.go:401` is the sole call site of `emitFieldDimsAttributes`) over all 202 banked rows
finds four test-side sites in three of them:

| row | file | field | type |
|:--|:--|:--|:--|
| `debug/dwarf` | `entry_test.go:45`, `:135` | `ranges` | `[][2]uint64` |
| `debug/elf` | `file_test.go:550` | `pcRanges` | `[][2]uint64` |
| `net` | `iprawsock_test.go:132` | `argLists` | `[][2]string` |

The census is a deliberately **independent derivation** — syntax, not the converter's `go/types`
predicate — and over production files it reproduces §10.1's single site exactly, at the line the
emission stamped. Positive control fires on that site; negative control (`unicode/utf8`) silent.

`debug/dwarf` then closed the loop end to end: the census predicted two stamps, the emission
delivered exactly two `[GoArrayDims(2)]`, and the row swept **40/40**, its banked count.

### 10.3 The importer derivation must parse imports, not match text

The canary set in §9 was first derived by a line-anchored grep for `"reflect"`. It returned 88 banked
importers **topped by `go/doc/comment` at 10059 verdicts** — a package whose `std.go:35` carries
`"reflect",` as a *list element*, and which CLAUDE.md names as exactly this over-match.

Both controls in place at the time — `encoding/json` IN, `cmp` OUT — **passed**, because both vary
the axis "imports it or doesn't" and neither varies "mentions it as data". A control only tests the
axis it varies.

Re-derived from parsed import declarations: 86 rows. The grep had wrongly admitted two, both `go/*`
packages carrying stdlib name lists — `go/doc/comment` and `go/internal/gccgoimporter`. The third
control pins the axis the first pair could not see, and is why §9 now names three.

## 11. Increment B, PREDICTED before it is cut (2026-09-03)

Recorded before the measurement so it can be wrong in public.

The parked `SliceOfArrayTypeName` guard exercises **value** sites — `reflect.TypeOf([][6]uint8{{}})`
on composite literals — not struct fields. Increment A's converter half stamps only field positions
(`visitStructType.go:401` is `emitFieldDimsAttributes`' sole call site), so **A alone should fix none
of that guard's previously-red slice/chan rows.** A's golib half (the `Elem()` hand-down and the
name-threading through the slice/chan arms) is *necessary* machinery but has nothing to carry until B
seeds the value site.

Per-row prediction, A-only:

| row | predicted | why |
|:--|:--|:--|
| `[6]uint8`, `[2][3]int` | PASS | top-level arrays already carry dims via `= new(N)` |
| `[]Grid` | PASS | a DEFINED array type renders by name; no dims needed |
| `[][6]uint8`, `[][3]int`, `[][2][3]int` | FAIL | value-site seeding is B |
| `[]*[4]byte`, `chan [3]int` | FAIL | same |
| `map[[2]int][]int` | FAIL | same, key side |
| `Elem().String()`/`Len()` line | FAIL | the type-side question B exists to answer |

So the two parked guards stay parked through A by design, and land with B. If A turns out to fix a
slice/chan row here, the prediction is wrong in an interesting way — it would mean a value-site route
already seeds dims somewhere this design has not accounted for, and that route must be found and
written down before B is cut on an assumption it contradicts.

### 10.4 The gate ledger at the seating tip (2026-09-03)

Increment A was rebased onto master `9bb83df3e` as `claude/reflect-cargo-inc1-m18`: the two source
conflicts were one duplicate commit (`919662458` == master's `e8800ae2a`, the `valueMethodName` seat)
dropped by `rebase --onto`; the doc's add/add resolved to this text. Verified by arithmetic — 11 commits
over master, the doc byte-equal to the posted tip's, A's applied delta identical to `b3caf3fa0`'s over
its nine non-doc files.

**Canaries, all seven rows swept on R-LAPTOP with the increment in place** (verdicts read from the
proof pages the sweep rewrites, never from the run log):

| row | swept | banked | reading |
|:--|--:|--:|:--|
| `debug/dwarf` | 40 · 0 | 40 | census hit — its two predicted `[GoArrayDims(2)]` landed, `entry_test.go:45`, `:135` |
| `debug/elf` | 31 · 0 | 31 | census hit |
| `encoding/json` | 491 · 0 | 491 | canary |
| `go/types` | 557 · 0 | 557 | canary |
| `net` | 472 · 2 | 472 | canary and census hit — the one predicted stamp landed on `argLists [][2]string` |
| `crypto/tls` | 400 · 2 | 3643 | validated on the sweep's host-limit arm: 3643 − the 3243-verdict `TestBogoSuite` block; the banked count stands |
| `net/http` | died | 1343 | **unreadable on any host today** — see below |

`net/http` was attempted twice on this host at this tip and died mid-stream both times (656 s at
950/1345; 286 s at 791/1345) on `crypto/aes: invalid buffer overlap` in `gcm.cs:361 counterCrypt` beneath
the TLS 1.3 client's `readServerCertificate`, on a goroutine of a test built to race handshakes — the i7's
own signature, four deaths across two host classes. That is a corpus defect in the AES-GCM overlap guard
under concurrent handshakes, **owned by C2** (COORD, 2026-09-03), on a path that touches no descriptor
cargo: **not attributable to A**, and by the same ruling not a gate A waits on. The four-death table and
the preserved records are on the mailbox.

**Gates at `2720a3977`:** converter suite `ok 208.289s` (a first run reported `FAIL` at 1835 s — a
laptop suspension spanning the run consumed its own 30-minute deadline; re-run awake, green).
Dotnet gates, same tip: G1 stdlib solution 0 errors across 307 projects (windows default); G2 GolibTests 507 passed / 3 failed, the three identity-verified as `CreateSymbolicLink` refusals
("a required privilege is not held by the client") in `FixtureLinkStagingTests` — a host privilege,
unchanged from the lane tip, not code; G3 behavioral OUTPUT on
`FieldDimsCargo` PASS — Transpile, Compile, Target and Output all green ([Output] running C# vs Go, comparing exit code + stdout... 1 compared, 0 failed PASS (1 pr); G4 reflect's `-tests` assembly convert and build exit 0, `reflect.tests.dll` written fresh at 09:23:58 (converter built by the behavioral runner at 09:20 from this tip); G5 `nistec` cost canary as a same-host
A/B, PRE (`e8c078637`) PASS 2195 at 174 s cold / 145 s warm vs A PASS 2195 at 90 s warm — A faster than PRE on both readings, so no cost regression; a third A reading is in flight to characterize the favorable delta, which is more likely variance than merit.

> **Amended 2026-09-03, third `nistec` reading.** A second warm A run read **154 s** against the
> first's 90 s, with PRE at 145 s warm / 174 s cold. The spread *within* A (64 s) exceeds the A-vs-PRE
> gap, so the "A faster on both readings" sentence above was the fast tail of run-to-run variance, not
> merit. Corrected characterization: **A is within noise of PRE; no cost regression** — the gate's
> verdict is unchanged, its wording was over-read. Three readings, one host, all warm but the first PRE.

> **§11 MEASURED 2026-09-03 at the seated tip `6dcbd7211`** — the parked `SliceOfArrayTypeName` guard,
> Go and C# binaries run directly and diffed row by row (positive control: the outputs differ):
> PASS `[6]uint8`, `[2][3]int`, `[]Grid`; FAIL `[][6]uint8` → `[][]uint8`, `[][3]int` → `[][]int`,
> `[][2][3]int` → `[][][]int`, `map[[2]int][]int` → `map[[]int][]int`, `[]*[4]byte` → `[]*[]uint8`,
> `chan [3]int` → `chan []int`, and `Elem()` of `[][6]uint8` → `[]uint8` with `Len()` 0. **Ten of ten as
> predicted.** A alone fixes no value-site row, and no unaccounted seeding route exists — the premise B
> cuts on. `Len()` answering 0 for an unknown is the `null = unknown` model read at its consumer.

## 12. Increment B — the value site and the constructors, designed before it is cut (2026-09-03)

### 12.1 What the source says the two routes are

`abi.TypeOf(any)` (`internal/abi/type_impl.cs:242`) is the VALUE route: it classifies the boxed
value's dynamic type and measures what it can — `ArrayDimsOfValue` for an array, `PointeeArrayDims`
for a pointer, `ChanDirOfValue` for a channel — then interns through `synthType(dyn, dims,
paramDims, chanDir)`, the four-argument overload, so a value never carries `keyDims`. A slice, map or
channel value therefore reaches the descriptor with NO element cargo at all, and `[][6]uint8{{}}`
interns as `slice<array<byte>>` with nothing to distinguish it from `[][8]uint8{{}}`.

The CONSTRUCTED route drops cargo the other way: `SliceOf(t)` is
`synthType(typeof(slice<>).MakeGenericType(st))` — the element's descriptor is consulted only for its
System.Type, its `arrayDims`/`chanDir`/`keyDims` discarded; `ChanOf` passes `null` dims; `PointerTo`
passes nothing (§7's pointer row: constructed `*[]uint8`, declared `*[6]uint8`); `MapOf` already
carries the KEY's dims (`arrayDimsOfReflectType(key)` as `keyDims`) and drops the element's. `ArrayOf`
is the one constructor that passes dims, which is why top-level arrays were never wrong.

Both routes intern on the same key — `descriptorDimsKey` = `join(arrayDims) + "@dir" + "#keyDims"` —
so identity between them falls out of feeding the SAME cargo in, exactly as §8.1 states.

### 12.2 The fork, and the arm taken

Two ways to seed a value-site slice/map descriptor: **(a) carry the cargo on the value** — a dims slot
on `slice<T>`/`map<K,V>` stamped by the converter at every literal and `make`; **(b) measure a present
element or key**, the way `ArrayDimsOfValue` already measures a nested array through its first element
(and already states `null` for an empty outer: "nested dims unknowable from an empty outer").

(a) is complete but costs +8 B on every slice header corpus-wide for a shape that is rare, and needs a
converter arm at every construction site. (b) is the arm `abi.TypeOf` already uses one kind over, costs
nothing, needs no converter change, and fixes every measured consumer — the `TestDeepEqualAllocs`,
`TestFuncLayout` and `TestTypes` rows all name NON-EMPTY literals. **B takes (b)** and states two
boundaries rather than working around them:

- **An EMPTY slice or map of unnamed arrays still collapses** (`[][6]uint8{}` with no element has
  nothing to measure). Recorded here; closed by increment C = arm (a) with its cost stated.
- **The number increment C must justify: +8 B on every slice header in the corpus** (the golib
  instance-state rule's exact shape), against two boundaries no measured consumer reaches. Ratified
  2026-09-03 (COORD): C inherits that measured cost, not a premise.
- **A channel VALUE's element length cannot be measured** (its buffer is not peekable), so `chan [3]int`
  by value stays `chan []int` until C carries it on the channel the way direction already rides
  (`channel<T>.m_direction`, stamped by the converter's `chanDirectionCargo` seam). The `ChanElemDims`
  guard holds that row, parked red by design. `ChanOf`'s CONSTRUCTED route is fixed in B.

### 12.3 The cut

1. golib `GoReflect`: `SliceElemArrayDims(value)` — first element's dims through `ArrayDimsOfValue` /
   `PointeeArrayDims`; `MapKeyArrayDims(value)` / `MapElemArrayDims(value)` — first entry's key/value
   dims; all `null` when empty or nil. The map's first entry is reached through its non-generic
   enumerator and one reflection read of the boxed pair — `IMap` has more than one implementer, and a
   path only `TypeOf` of a map-of-arrays value takes does not justify widening the interface
   (amended at the cut, 2026-09-03; the design above said "gains a first-entry accessor").
2. `abi.TypeOf(any)`: the switch gains the Slice and Map arms and calls the five-argument `synthType`
   with `keyDims`.
3. reflect constructors: `SliceOf`, `ChanOf`, `PointerTo` pass the element's `arrayDims`/`chanDir`/
   `keyDims` unshifted (the same rule A's `Elem()` hands them down by); `MapOf` adds the element's
   `arrayDims` beside the key dims it already carries.

### 12.4 Predictions, per row, before the cut

- `CanonicalTypeIdentity` (9 rows, all non-empty): **9 of 9 green** after B; today rows 1–9 are red
  except where both routes happen to collapse alike.
- `SliceOfArrayTypeName` (8 shapes + the `Elem()` line, chan row moved out, `[]*[4]byte` made present):
  **all green** after B.
- `ChanElemDims`: constructed row green, value row **red by boundary** — the increment-C marker.
- The `pointer` and `map key` over-distinct rows of §7 become **equal** (rows 3 and 4/5 above).

### 12.5 Acceptance

The identity guard, `encoding/gob` at 106 (a Type-identity consumer banked green WITH the collapse — a
canary against damage), the five importer canaries derived at gate time, the `nistec` cost canary as a
same-host A/B (golib on the boxing path), a two-seeded reconvert diff (expected: ZERO corpus footprint —
B changes no emission), union CNR, and the standard converter-suite / stdlib / GolibTests / behavioral
OUTPUT battery. Names are the symptom; identity is the gate.

> **§12.4 amended at the first guard run, 2026-09-03 — one prediction WRONG, and instructively.**
> The `ChanElemDims` "constructed row green" prediction asserted `ChanOf(BothDir, ArrayOf(3,int)) ==
> TypeOf(chan [3]int)`. It cannot be green in B: `ChanOf` now carries the element's dims while the
> VALUE side is the stated boundary, so the two descriptors differ *because* the constructed one is
> right. **An identity row against a boundary side is a boundary row.** The constructed route's own
> property — `String()` `chan [3]int`, `Elem().Len()` 3 — is what B asserts; the identity is C's row.
> Also at that run: two guards failed to COMPILE on `[]*[4]byte{{}}`, Go's elided-`&` literal for a
> pointer-to-array element, which the converter emits as an uninstantiable type (CS0144) — a
> converter gap outside B, routed as its own item; the guards use the explicit `&[4]byte{}` form.

> **§12.4 amended again at the second and third guard runs, 2026-09-03.** Two of the name guard's red
> rows were MINE, not B's: `map[[2]int][]int{}` and the `Elem()` line's `[][6]uint8{}` were written
> with EMPTY literals — the very boundary §12.2 states — so they measured the boundary, not the fix;
> both now use a present entry/element. The nested `[][2][3]int{{}}` row read `[2][0]` because the
> converter emits a ZERO nested-array element with inner arrays of length 0 (runtime truth of a wrong
> value, `new array<nint>[]{}.array(2)`); the rows use populated inner arrays and the emission gap is
> routed with its sibling, the elided-`&` pointer element. A third detour, also routed: a `Printf` whose
> FORMAT STRING holds a comma inside parentheses (`ArrayOf(3,int)`) is emitted with its literal split
> and a stray cast (CS1003/CS1010) — first mis-diagnosed as the `.String()` first argument, then pinned
> by elimination (plain locals still mangle; `Println` with the same text converts); the chan guard
> prints that row with `Println`. **Three converter defects found by
> guards written to measure reflect; none is B's, all three have their own item.**

> **§12 amended, B.1 (2026-09-03): a slice element contributes no dims.** B's `elemArrayDims` handed a
> map's first value or a slice's first element to `ArrayDimsOfValue`, which accepts any `IArray` — and
> `ISlice : IArray` — so a `[]string` value's runtime LENGTH became an array dimension on the container's
> descriptor. `TypeOf(map[string][]string)` then depended on the first enumerated entry's length, and
> `DeepEqual` of one map inserted in two orders read false — `net/http`'s `http.Header` failure at the
> train-20 head, predicted from the code and confirmed by rows 10–12 before the fix. The guard's nine
> rows all had ARRAY elements: they tested the axis B changed, not the axis its predicate could reach.
> Fix: `elemArrayDims` returns null for an `ISlice`; `ArrayDimsOfValue` refuses one at its door — the
> same `ISlice : IArray` predicate trap the array-range `Clone` defect hit in `array.cs` this week.
## 13. R1 re-sized at the cut: the arms read through the accessors (2026-09-03)

**The root, measured from source, and it moves R1 from "loud" to "right".** The converted `regAssign`
reads the descriptor by Go's `unsafe.Pointer` idiom — `Reinterpret<abi.Type, structType>()` /
`<abi.Type, arrayType>()` — and a SYNTHESIZED `abi.Type` has no inline record behind that view: the
struct view reads `Fields.Length` 0 for every synthesized struct (§2.2's `reflect_test.S`, Size 32,
Fields 0), the array view `Len` 0 for every synthesized array. The accessors `abi.StructType()` and
`abi.ArrayType()` already synthesize exactly those records — `Fields` from `GoReflect.GoFields` with
offsets and per-field cargo, `Len` from `arrayDims` — one memoized call away. §2.2's "struct Fields
on a synthesized descriptor" was never a missing datum; it was a bypassed accessor.

**R1 as cut:** the companion reads through the accessors, so a declared type answers correctly, and
throws only where the accessor has nothing (nil, or a fieldless-but-sized struct; a nil `ArrayType`
or `Len` 0 with null dims). `struct{}` and `[0]T` pass. This folds §2.2's struct instance into R1.

**Predictions, revised before the measurement:** `TestFuncLayout`'s two rows — previously predicted
"loud, not green" — are now predicted **GREEN**, since the Struct arm reads the four fields of
`reflect_test.S` the accessor synthesizes. BROKEN {} on the reflect record against B's tip (train
20's state) and the canaries at banked remain the seat condition. If a row stays red, the reason is
in the accessor's record (offsets, per-field dims) rather than in the arm, and that is where the
next cut goes.

### 13.1 Blocked again, one emission path over (2026-09-03, at the measurement)

The "Blocked, not by design" note above recorded that displacing `regAssign` — a `[GoRecv]` method with
a REF receiver — made the converter emit a box-form call (`Ꮡa.regAssign`) into a `ref` body with no
box, CS0103, and that a converter fix was dispatched separately. That fix (`7857e252b`, in master since
train 18) holds for the `-stdlib` emission: the two-seeded diff of R1's registration measured exactly
the body's removal, `abi.cs` −81/+1, with the call sites unchanged in value form. It does **not** hold
for the `-tests` emission: `go2cs -tests -test-action build` on `reflect` re-emits `abi.cs` with
`if (!Ꮡa.regAssign(Ꮡt, 0))` at line 145 — `core\reflect\abi.cs:145 CS0103 'Ꮡa'` — and the test assembly
does not build, while the production assembly builds clean at the same tip. A production-only diff is
blind to test-side emission by construction; this is that blindness measured at R1's own seam.

**Consequence:** R1's seat condition (BROKEN {} against train 20's reflect record) cannot be measured
until the `-tests` path reads the same receiver-form predicate the `-stdlib` path reads — routed to the
coordinator as a suggestion (owner rule). R1's commit stands as cut; its converter suite is green with
the displacement witness inside.

**The BEFORE for the two asks, preserved from B's seated tip (train 20's state):** reflect 388 tests,
311 pass · 76 fail · 1 skip, 19 mismatches against Go; `TestFuncLayout` parent FAIL, `func(reflect_test.S)`
FAIL (the §2.2 row), the other seven subtests pass. The AFTER waits on the test assembly.

### 13.2 §13.1 retracted: there is no `-tests`-path twin (2026-09-03, same day)

The measurement tree §13.1 was read from — B's seated tip plus R1 — forked from A's seated tip before
train 18 landed `7857e252b`, so it contained no ref-receiver displacement fix on ANY emission path
(`merge-base --is-ancestor 7857e252b HEAD` → no); its CS0103 was the original bug. On the seat tree,
master `93a131a3f` + R1, the `-tests` conversion of `reflect` emits `if (!a.regAssign(Ꮡt, 0))` — value
form, box-form count 0, placeholder present: the `-tests` path reads the same predicate the `-stdlib`
path reads. The corrected measurement base is master + B MERGED (the content train 20 lands), and the
PRE record preserved from B's bare tip is replaced by one taken at that merge. The lesson is a rule: a
measurement tree must contain every seat the cut depends on, asserted by ancestry before any gate.

## 14. R1.1 — the residual behind R1, rooted and closed at the same seam (2026-09-03)

**Reading that sized it.** R1's arms on master `93a131a3f` + B `ab7ce0534` (PRE `c20342cc3`) and + R1
`0ea282661` (CUT): both `{pass 311, fail 76, skip 1}`, 19 mismatches, **BROKEN {} · FIXED {}**. The
target row `TestFuncLayout/func(reflect_test.S)` went from five failed assertions (`size`, `argsize`,
`retOffset` = 0 want 32; `stack`, `gc` = [] want [0 0 1 1]) to the two bitmaps alone — the accessor
route reads the fields, so the sizes are right, and nothing downstream of them is.

**Site.** Both bitmaps are one function: `newAbiDesc` fills `abid.stackPtrs` through
`addTypeBits(stackPtrs, stkStep.stkOff, arg)` (`abi.go:422/455`), and the frame type's `gc` IS that
bitvector (`funcLayout`: `x.GCData = &abid.stackPtrs.data[0]`). `addTypeBits`' Array and Struct arms
raw-reinterpret the descriptor (`Reinterpret<abi.Type, arrayType|structType>`), reading `Len 0` / no
`Fields` on a synthesized descriptor — regAssign's defect one call over. The `Pointers()` gate ahead of
it is not the blocker: B's synthesis stamps `PtrBytes` when known (`type_impl.cs:198`).

**Cut.** The same displacement: `"addTypeBits": goosAny` beside `abiSeq.regAssign`, the companion in
`abi_impl.cs` reading `Ꮡt.ArrayType()` / `Ꮡt.StructType()`, loud panic on an unreadable descriptor,
scalar arms verbatim; `type.cs` placeholder applied as two-seeded hunks (PRE = R1's converter).

**Prediction (COORD, before the arm ran).** Both remaining assertions clear (`stack` and `gc` read
`[0 0 1 1]`); BROKEN {} against the standing PRE record; FIXED ⊇ that row; the two-seeded footprint
two more paths in `reflect`.

**Measured.** Reflect `-tests` on the PRE tree's content + R1 + R1.1 (`0dfc95e21`): build **0 errors**; `{pass 313, fail 74, skip 1}`, **17 mismatches** (from 19). Against PRE `{311, 76, 1}`: **BROKEN {} · FIXED {`TestFuncLayout`, `TestFuncLayout/func(reflect_test.S)`}**; against R1's own record: BROKEN {} · FIXED the same two — the prediction hit on every point. Footprint: two paths in `reflect` — `type.cs` +1/−40 applied as hunks (delta lines byte-identical to the emission), `package_info.cs`'s position-map line excluded by rule. Pre-rebase commit `799727fa0`.

**Class after R1.1, sized per site** (raw `Reinterpret<abi.Type, structType|arrayType>` in emitted reflect
production; second-derived — an Explore agent's read and independent greps agree). Eight functions, eleven
sites, and **seven of the eight are DEAD in the C# port**: every Go caller is itself a placeholder whose
hand-owned body never calls back into the emitted helper —

| Emitted site(s) | Sole Go caller(s) | Caller in C# | Live? |
|---|---|---|---|
| `isReflexive` / `needKeyUpdate` / `hashMightPanic` (Array+Struct arms, `type.cs`) | `MapOf` (`type.go:1862/1865/1868`) | placeholder `type.cs:1404`, hand-owned `value_impl.cs:3300` on `synthType(typeof(map<,>)…)` | dead |
| `typeptrdata` (Struct arm) | `StructOf` only (`type.go:2643`; `ArrayOf`/`FuncOf` compute `PtrBytes` inline) | placeholder `type.cs:1773`, hand-owned `value_impl.cs:3376` | dead |
| `bytesSlow` (Array arm, `value.cs`) | `Value.Bytes` | hand-owned `value_impl.cs:890` (Array arm off `array<byte>` + `CanAddr()`) | dead |
| `lenNonSlice` (Array arm, `value.cs`) | `Value.Len` | hand-owned `value_impl.cs:326` (`IArray.Length` / `arrayDims`); `Cap` `:574`, `SetLen` `:618` likewise | dead |
| `FieldByNameFunc`'s embedded walk (`type.cs:1094`) | `rtype.FieldByNameFunc` → `structType.FieldByNameFunc` | live entry `type.cs:628`, which raw-reinterprets `rtype → structType` at `:634` BEFORE the walk (so the walk is never reached on a synthesized descriptor) | live, unreached; **zero Go tests** call `FieldByNameFunc` |
| `rtype → structType` reinterpret at the EXPORTED entries — `Field` / `FieldByIndex` / `FieldByName` / `FieldByNameFunc` / `NumField` (`type.go:748/756/764/772/796`) | Go's public `Type` API | `Field`, `FieldByIndex`, `FieldByName`, `NumField` are placeholders (hand-owned in `value_impl.cs`); `FieldByNameFunc` is the LIVE emitted entry (`type.cs:628`, reinterpret at `:634`) — an entry live with its walk dead beneath it | live, unreached: **0 calls to `FieldByNameFunc` in Go's reflect tests** — a measured boundary, not a surprise, for whoever un-hand-owns a sibling |

None of the sixteen remaining mismatching tests has any of the eleven sites on its path (TestBytes runs
through the hand-owned `Bytes`; TestIsZero / TestSetLenCap / TestDeepEqualAllocs through the hand-owned
`Len` / `Cap` / `SetLen`; TestFieldByName through `value_impl.cs:3689` + `promotedFieldByName`;
TestImplicitMapConversion is `MapIndex` / `SetMapIndex`, not `MapOf`). Nothing here is worth a cut: the
dead seven are latent (they compile, and un-hand-owning `MapOf` / `StructOf` / `Len` / `Bytes` would make
them silently wrong — `isReflexive` on empty `Fields` answers `true`, `typeptrdata` answers 0), and this
table is their record, because a comment in an emitted file would not survive a regen. The last row is the shape the census did not cover, named as a row so the next lane meets it as a measured boundary.

## 15. Increment C — the length is carried from where it is known, not recovered from where it is not (2026-09-04)

**The defect this closes.** `reflect.SliceOf(reflect.ArrayOf(3, byteT))` was not identity-equal to
`reflect.TypeOf([][3]uint8{})`. The declared side is an EMPTY literal: increment B measures a
container's element cargo off a PRESENT element (§12.2), an empty slice has none, so the value route
interned under key `""` while the constructed route interned under `"3"` — two descriptors, two boxes,
and `reflect.Type` identity is box identity. Increment B made the OBSERVED case right and left the
unobservable one wrong; that is a trade, not a break, and `ReflectArrayOf` is where it surfaced.

**Why no runtime-only fix exists.** `[3]uint8` and `[4]uint8` are ONE managed type, `array<uint8>` —
an array's length is a CONSTRUCTOR ARGUMENT here, not a type parameter. A container with no elements
therefore holds no observation of its element's length, and every restructuring of the interning key
(keying on the element DESCRIPTOR, on structural equality, on the element's managed type alone)
reduces to the same missing observation. Identity cannot be made tolerant of "unobserved" either: it
is `ж<T>` box equality, so tolerance would change POINTER equality corpus-wide and would not even be
transitive. **The fact is not lost, it is DROPPED** — the converter knows the length statically at
every site that creates such a slice, and `[][3]uint8{}` was emitted as `new array<uint8>[]{}.slice()`
with the 3 nowhere in it.

**The cut.** `GoReflect.WithElemDims` records the element dims against the slice's BACKING ARRAY in a
`ConditionalWeakTable`; `SliceElemArrayDims` consults the record first and keeps observation as the
fallback, so nothing already right becomes wrong. The converter wraps the three creation kinds —
composite literal, `make`, and slice expression. The backing array is the identity the dims belong to:
a reslice shares it and inherits the record without being wrapped. `ISliceBacking` answers that
backing without knowing the element type, because `IArray.Source` materializes a DETACHED COPY and
would have made every write land on a throwaway object and every read miss — a fix that silently does
nothing.

**Why the side table and not the +8 B field, measured.** A type-aware census over `std` (840 packages,
positive control 27,143) found **130** slice-of-array creation sites — 87 composite literals, 38
`make`, 5 slice expressions, 0 involving a generic type parameter — against **27,143** plain-slice
creation sites. `slice<T>` is 40 bytes on x64, so a dims field is +20% on every slice VALUE in the
corpus, paid by ~209 sites for every 1 it serves. The field buys exactly ONE row class over the side
table (the nil slice) and no case reaches it. Ruled: zero bytes; the cost is one lookup on the
`TypeOf` path and no slice operation touches it.

**`Array.Empty<T>()` is the one rule.** It is a SINGLETON shared by every length, so recording against
it would make `[][3]uint8` and `[][4]uint8` collide on one key. The write path SUBSTITUTES a fresh
zero-length backing rather than refusing — `make([][3]uint8, 0)` is a legal Go program and must not
throw. Measured on this runtime: `new T[0]` allocates a distinct object per call and is not folded to
the singleton, and a table keyed on two fresh empties keeps them apart.

**KNOWN BOUNDARIES, recorded rather than remembered.** (a) A **nil slice** has no backing object to key
on; (b) a slice whose backing a **reallocating `append`** replaced loses the record (observation covers
it whenever elements exist); (c) **map- and pointer-of-array** containers have no creation-site record
at all. All three keep the observation-only answer, and no case in the corpus or on the roster reaches
any of them. The remedy for (a) and (b) is the +8 B field above, measured and declined; for (c) it is
the same treatment applied to those creation sites the day a case appears. `GolibTests.SliceElemDimsTests`
asserts the nil case at TODAY's answer, so a remedy cannot land without that assertion being updated
deliberately, and the behavioral guard carries all three in a documented non-printing block — a
behavioral project is a stdout comparison, so a row whose C# answer differs would red the project.

**The `[][N]byte` ambiguity, for the record beside §12's bar.** A per-assembly dims REGISTRY was
proposed and withdrawn on measurement: across the stdlib 11 of 15 element types carry exactly one array
length, but `uintptr` carries three, `uint16` and `string` two each, and `byte`/`uint8` — the busiest —
carries **three at creation sites (4096, 32, 6)**. A registry would have been right by luck on the shape
most likely to be asked. The side table is right by construction on all 130.

**Guards.** Six `GolibTests` rows (the record, the two-lengths case, the `Array.Empty` substitution
positive-controlled by removing it, observation still answering a populated slice, a window inheriting
through the array's own backing, and the nil boundary at today's answer) plus the behavioral project
`ReflectEmptyContainerIdentity`, 10 rows against Go's own output: the observed rows that must stay
right, the empty-literal assertion, and an ambiguous package where `[][3]` and `[][4]` both answer
correctly and remain distinct types.

## 16. §12.4's THIRD detour WITHDRAWN — the `Printf` mangling is not a converter defect (2026-09-04, SUB-Q2)

> Appended as a dated block rather than edited into §12.4, which stands as written: Increment C is
> code-complete on a branch that touches that section, and an in-section hunk is the adjacent-edit
> merge trap this file should not pay for a correction.

§12.4's amendment of 2026-09-03 counts **three** converter defects found by guards written to measure
reflect. The third — "a `Printf` whose FORMAT STRING holds a comma inside parentheses (`ArrayOf(3,int)`)
is emitted with its literal split and a stray cast (CS1003/CS1010)" — **does not reproduce, and the
count is two.**

**Measured at two converters, not one.** Master `26ff0c45b`, and a converter built from `src/go2cs` at
**`2211c1d8e`** — the parent of `c70293a20`, i.e. the exact binary the guard runs behind that amendment
were raised against. Three inputs: `ChanElemDims.go` verbatim with the `Println` sidestep replaced by the
`Printf` it names; a twenty-row format-text matrix; a seven-row argument-shape matrix. **Six conversions,
all exit 0, and the base and master emissions are byte-identical on all three (`diff` empty ×3).** The
reported line emits as one verbatim literal at BOTH binaries, with no split and no stray cast:

    fmt.Printf("constructed row: ChanOf(BothDir, ArrayOf(3,int)) String()=%s Elem().Len()=%d\n"u8, name, n);

**Both stated triggers fall, each refuted by rows written for it.** The surviving diagnosis (a comma
inside parentheses in the format) is refuted by `(a,b)`, `(a, b)`, nested `f(g(1,2))`, empty `()`, a verb
straight after `)`, the escaped-quote form, `100%%` beside parens, one/none/three arguments, and single-
and multi-line call forms. The FIRST diagnosis it replaced — a `.String()` method call as the first
variadic argument — is refuted by its own three rows plus the Stringer-value and second-position
controls. **Two rows an elimination inside one file could not have reached also convert clean, and they
are the ones that would fail if the mechanism were what was proposed: UNBALANCED `open(` and `close)`,
which is precisely what a paren-depth scan over a literal's CONTENTS would desynchronize on.**

**The standing corpus says the same and cost one grep.** `src/core` carries thousands of emitted formats
of this shape compiling 307/307 — `bufio_test.cs`'s `"first ReadSlice(,) = %q, %v"u8` (a comma ALONE
inside parentheses), `archive/tar/strconv_test.cs`'s `"formatPAXTime(%ds, %dns): got %q, want %q"u8` in
the multi-line call form, `bytes/buffer_test.cs`'s nested `"%s: buf.Len() == %d, len(buf.Bytes()) == %d"u8`.
A trigger that real could not leave that population green.

**What the reporting tree saw is not reconstructible from here and no mechanism is invented for it.**
One reading is worth carrying, because it generalizes: **`CS1010 "Newline in constant"` beside `CS1003`
is the signature of a TEXT-CORRUPTED `.cs`, never of an emission decision.** The converter emits a Go
source newline escape as the TWO-CHARACTER C# escape at both binaries, and a raw newline inside a C#
literal is not a form any path here produces. That is the r41 overlapping-conversion family — whose
documented signature is ONE
corrupted file, syntax errors, reading exactly like a converter regression — or a stale binary; both are
live while a lane runs behavioral gates over its own tree.

**Two lessons this block banks.** An elimination performed inside ONE file is a hypothesis, not a pin —
the second diagnosis was reached by varying two rows of one guard, and the corpus refutes it in one
grep. And **the negative result banks in CODE at the gate, not only in prose**:
`src/tests/Behavioral/PrintfFormatCommaParen` carries all twenty-four rows with
`[GoTestMatchingConsoleOutput]` and a golden pinning the emitted `"…"u8` form, so the next report of
this shape is a filtered behavioral run rather than an investigation. **No fix was written**, deliberately:
machinery that cannot be made to fail under its own control is a false-green seed.

**Owed elsewhere, ruled by COORD and recorded here so it is not lost:** `ChanElemDims.go` still carries a
comment asserting this defect, and the honest correction there is not a comment edit but RESTORING the
`Printf` line the sidestep replaced — which moves that guard's golden and belongs to its owner, after
Increment C lands, with this guard's matching row as the reference.
## 17. Increment D — the channel VALUE carries both cargos on one field, because its element cannot be measured (2026-09-04)

> Numbered 16 in the increment’s own commits and posts; landed as 17 because SUB-Q2’s §12.4 withdrawal block took §16 on train 25 (coordinator merge resolution, train 27). References to §16 inside this section mean this section.

**The defect this closes, in two halves that turned out to be one.** A channel's element is not a
present value — its buffer is not peekable — so nothing can be measured off a channel value: not the
element's array length (the `ChanElemDims` row, red since increment B, and left red by increment C,
which landed scoped to slices after its side table was ruled), and not a NESTED channel's direction
(increment 2b's value half — `TestChanOf`'s `var right chan (<-chan T)` and the `typeTests` field
rows, both VALUE positions). Increment C's remedy cannot transfer: a side table keyed on the object
cannot key a NIL channel, and two of the three stamp positions (a field's initializer-borne zero, a
zero var) produce exactly that. So both facts ride the value, from the position that knew them
statically, on ONE field change — two changes on one struct in two increments would pay the layout
cost and walk the stamp sites twice (the coordinator's unification ruling).

**The representation.** `channel<T>`'s one-byte `GoChanDir m_direction` becomes `ChanCargo? m_cargo`
— a sealed, immutable class carrying `GoChanDir[]? DirChain` (outermost first, never empty on a live
instance) and `nint[]? ElemDims`. `null` is the unstamped channel, so `Unstamped` and `Both` keep the
canonical spelling they had; the two scalar directions are interned, so a directional nil channel
allocates nothing and the cargo costs one allocation only where a chain or dims are actually
stamped. `Direction` reads the chain's head; `Cargo` exposes the whole thing to the bridge through
`IChannel`.

**The cost fork, MEASURED rather than argued (all solo).** `Unsafe.SizeOf<channel<int>>()`: BEFORE
**16** (one core reference, one byte enum, seven bytes of padding); CONTROL — one extra `object?`
field, nothing else — **24**, with the `TheChannelValueDoesNotGrow` row RED as it must be; restore
byte-identical; AFTER, the cargo field in place — **16**. The reference rides in the padding. The
control is what makes the AFTER green an assertion: before it the row was a baseline that passed
either way. A layout read, decided by the field set at JIT time, so load cannot move it — the
loaded-vs-solo rule for alloc rows does not apply, and it was run solo anyway. The pre-ruled fork
read (a): the header carries it and the question closes on correctness at no cost.

**The creation-site census, prediction first, and the prediction was wrong by the whole band.**
`go/types` over every stamp position the converter owns (make, zero var, field, array element, new,
nil-conv, named result) for a channel whose element is a channel or an array: production std **0**
(predicted 16, band 6–45; the falsifier written for "0 in production" fired and was confirmed by an
independent text census — 0 nested spellings, the one regex hit being `go/types`'s own comment that
`chan (<-chan T)` requires parentheses, and 0 `chan [N]T`); with tests **9**, seven in `reflect`'s
own suite and two `make(chan chan struct{})` helpers in `net/http_test` that nothing reflects over.
The plain-channel control read 225 against a floor of 1,000 set from feel, and resolved the OTHER
way: the instrument's `make` arm (129) sits under a build-tag-blind text bound (162) and both guard
controls read 1/1/1, so the count stood and the floor was the error — a control's floor is derived
from a text bound before it is committed to. The consequence for the design: D's converter half has
NO production consumer, its measuring gate is the TEST emission of `reflect`, and the `-stdlib` diff
is a negative arm whose zero needs a mechanism.

**The emission rule — the mechanism.** The cargo form (`Type.Nil(ChanCargo.Of(chain, dims))` at a
zero form; `ChanCargo.Of(…)` as `make`'s constructor argument) is emitted ONLY when the NORMALIZED
chain has more than one entry or the element carries dims. A scalar direction with no dims keeps
`.SendOnly` / `.RecvOnly` / `GoChanDir.Send` byte for byte; a bare bidirectional channel still emits
nothing. Normalization in the converter (`chanDirChain`) is the same rule as `abi.normalizeChanDirChain`
— trailing bidirectional entries trim, interior ones stay, all-bidirectional is absent — and the
walk stops at a DEFINED channel type at any level, which is a go2cs-gen wrapper with no field to
carry cargo. nil-conv joined the dims stamp set on day one: `(chan [100]T)(nil)` is the only
channel-of-array creation site in the whole std tree.

**The measuring gate, predicted before it ran and held exactly.** Working the seven `reflect` sites
through the rule moved the prediction from seven lines to FIVE — `chan<- chan string` (`:87`) and
`chan<- chan T` (`:6153`) normalize to a scalar chain and stay on the pre-D path — and the two-seeded
test-emission census read **5 removed / 5 added in `all_test.cs`**, exactly `:88`, `:89`, `:90`,
`:6154`, `:7265`, 0 position-map lines, 0 other files; `net/http` 0 differing files. PRE was rebuilt
from the seated tip's own sources and asserted not byte-identical to CUT before either arm ran.

**Two defects the guard found, fixed here, disclosed.** (1) With the value-route rows added, one of
22 diverged: `struct{ x chan<- <-chan int }` rendered `chan<- chan int`. The emitted `.cs` carried
the full `[Send, Recv]` chain in the initializer; the loss was `reflect`'s own hand-owned field paths
— `StructField`'s descriptor and both `Value.Field` arms — still built from the scalar `f.ChanDir`.
They read the cargo now, through a chain form of `makeTypedValue` with the scalar form forwarding.
(2) After that fix the row rendered `chan<- (<-chan int)`; Go prints it bare. Increment 2b's
parenthesisation rule keyed on the ELEMENT alone ("wrap when its rendering begins with `<`"),
derived from five oracle rows that never put a directional head over a receive element. Go's rule
(`go/types` `typestring.go`: only the `SendRecv` arm sets parens) wraps ONLY under the bare
bidirectional `chan`, because only `chan <-chan T` re-parses; under `chan<-` or `<-chan` the arrow is
already bound. The seated 2b therefore carries a latent constructed-route defect for `chan<- <-chan T`
and `<-chan <-chan T` — `typeTests` lines 88 and 89 — that none of its rows exercised and no banked
production row can reach; rows 6 and 7 pin both spellings from both routes now. Also on the way:
`abi.Elem()` named pointer and map alone as the unshifted-dims kinds, so a slice's or a channel's
element dims were shifted off there — moved to the `KindCarriesElementCargo` predicate `reflect`'s
own `Elem()` already applied.

**Guards.** `ChanDirectionChain`: 22 rows byte-identical to `go run` — Go's seven spellings,
`Elem()`'s tail, two identity rows, and the value route through zero-var, `make`, nil-conv, struct
field and `new`'s pointee, plus dims on three shapes — with 2b's two control arms re-firing
unchanged. `ChanElemDims`: `[GoTestMatchingConsoleOutput]` turned ON and its value row GREEN
(`chan [3]int` / `Elem().Len()=3`), the header corrected in the previous commit now true; before D
its Target arm compared emitted text and could not see the dimension the guard is named for.
`ChanCargoTests` (five golib facts: interning, the head, dims on the same cargo, the boxed read and
its null, equality ignoring cargo) and `ChanDirChainTests` (the 16 B row now a before/after
assertion). The `Chan*` family: Output 12 compared / 0 failed.

**Gates.** The bank battery's readings are appended as a dated block when it prints: the `-stdlib`
negative arm (predicted 0 files) with its flipped-gate positive control, `reflect -tests all` for the
acceptance rows (`TestTypes`, `TestChanOf`), the converter suite, CNR, GolibTests, `go2cs.slnx`
(owed: golib gained public API), the stdlib solution, the full behavioral suite, the five re-derived
canaries, and `nistec` as the cost canary — `channel<T>`'s field moved for every channel value in
the corpus.

**Gates, read 2026-09-04 (the bank battery at `c3d8bb388`, its sequel at the same code tree).** Least-evidence
legs first, every verdict grepped from its own full log.

| leg | reading |
|:--|:--|
| the `-stdlib` NEGATIVE arm (two seeded roots, PRE = the seated tip's converter rebuilt from its sources) | **0 files, 0 lines** — zero production movement, as the emission rule predicts by construction |
| its POSITIVE control (the cargo gate flipped: cargo form for every directional channel) | **12 files / 44 lines**, cargo in 13 files; each of the three arms emitted 1,656 files this run |
| `reflect -tests all` (the acceptance row, explicit runtime root) | **`TestChanOf` pass / pass — both assertions; `TestTypes` pass / pass** — the five `typeTests` spellings; `TestChanOfDir`, `TestAll`, `TestTypeOf`, `TestFuncLayout` pass; 315 of 388 rows agreeing; 73 rows in 29 parent tests disagreeing — the standing unbanked set (`TestDeepEqualAllocs`, `TestGCBits`, `TestStructOf`, `TestSlice`, the alloc and map-iterator rows, …), which the union set-diff is the instrument for |
| converter suite `-count=1` | `ok` 284.8 s |
| CNR | **`CHANGED`: exactly one file**, `ArrayValueCopySites.cs` — `make(chan [3]int, 1)` at its `.go:238`, a channel-of-array `make`; the std census could not see the behavioral corpus, and CNR censused it: one site, one line, golden re-baselined, all four phases green |
| GolibTests | 561 / 3 — the three FixtureLinkStaging host-privilege reds on the ledger; +5 = `ChanCargoTests` |
| `go2cs.slnx` (owed: golib gained public API) | **0 errors** |
| stdlib solution | **0 errors** |
| FULL behavioral | 675 / 675 / 674 + 1 / **649 pass · 0 fail · 26 skip** — the +1 in Output is `ChanElemDims` compared for the first time; the one Target red is the `ArrayValueCopySites` golden above, since re-baselined |
| canaries, re-derived at gate time | `crypto/tls` PASS 400 (host-limit disclosed) · `net/http` 0 of 1,345 rows disagreeing, FAIL on the process-level leak check only · `go/types` PASS 557 · `encoding/json` PASS 491 · `net` PASS 472 |
| `nistec` COST canary | PASS 2,195; **wall 84 s against 85 s** on this box before D — no spread |

**The `abi.Elem()` row, measured after the battery.** `assignable chan [3]int -> chan [4]int` false, `-> chan [3]int`
true, `convertible -> chan [4]int` false — against Go, 25 rows identical. Its control, the fix reverted: **both**
`[3]->[4]` rows flip to true (assignable AND convertible — `ConvertibleTo` walks the same channel arm of
`haveIdenticalUnderlyingType`), the `[3]->[3]` row does not move, restore byte-identical, green again. The first
prediction named one row; the control was sharper than the prediction, by the one path it did not count.

**Instrument errors on the way, each caught by its own guard and re-run.** The negative arm's first run built its
control converter to a POSIX-spelled `-o` path with path conversion off — the binary landed under `C:\c\`, the arm
invoked a path that did not exist, and the emitted-vs-seeded count is what said so; the script was also edited
while bash was executing it, which garbled the run. The acceptance leg's first run published against the
machine-global deploy root (present, stale, fifteen projects) because no explicit `-go2cspath` was passed and
self-location correctly declines a root that has `core/golib`. The sequel's exe diff first ran a stale Debug
build matched by a glob ahead of the runner's Release build. A one-hour clock step landed inside the full
behavioral leg; that leg's 4,381 s is the runner's own stopwatch, and no wall reading spans the step.
<!-- {% endraw %} -->
