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
