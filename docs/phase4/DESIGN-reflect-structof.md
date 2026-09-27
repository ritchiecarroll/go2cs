# DESIGN — `reflect.StructOf`: runtime struct synthesis in the managed reflect bridge
<!-- {% raw %} — Jekyll/Liquid guard: this doc contains {{ sequences (Go composite-literal syntax) that Liquid would otherwise parse or silently eat. Keep the matching endraw as the final line. -->

> **STATUS: RATIFIED WITH AMENDMENTS (coordinator, 2026-08-25). NOT IMPLEMENTED.** The first draft was
> measured against the live bridge by an adversarial review lane (probes on `golib.dll`, real
> `TypeBuilder` mints, both TFMs), which confirmed mechanism A and returned **nine amendments — three
> of them CONFIRMED-DEFECT class** that would otherwise have been found mid-implementation. All nine
> are folded in below, and the eight open questions are now **ruled** (§10) except OQ-4, which
> survives as one named experiment. Amendment provenance is marked ⚠ **AM-n** at each site so the
> ratified text can be read against the draft it replaces; §11.1 tabulates all nine.
>
> **Commissioned by** the board's pricing of `encoding/gob`'s last verdict — *"the last verdict costs
> one small hand-own (`reflect.ArrayOf`, roughly free over the dims cargo) **plus** `reflect.StructOf`,
> which is runtime struct synthesis over `System.Reflection.Emit` and a feature arc in its own right"*
> ([`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md), lane
> `claude/map-key-elem-cargo`, 2026-08-20).
>
> **Companions:** [`DESIGN-reflection-bridge.md`](DESIGN-reflection-bridge.md) (the bridge's
> architecture and the `canonType` interning fix), [`DESIGN-reflection-bridge-phase3-plan.md`](DESIGN-reflection-bridge-phase3-plan.md)
> (increment 2 — `New`/`MakeSlice`/`MakeMap`), [`DESIGN-position-map.md`](DESIGN-position-map.md)
> (§4.6's `package_info.cs` interaction), and the `reflect.ArrayOf` lane (`claude/reflect-arrayof`),
> which §7.1 composes with and which **merges first** (OQ-1).
>
> **Sources of record:** `src/core/internal/abi/type_impl.cs`, `src/core/reflect/value_impl.cs`,
> `src/core/golib/GoReflect.{cs,TypeLayout,TypeNaming,FieldAccess,MethodSets}.cs`,
> `src/go2cs/manualTypeOperations.go`. Line numbers are pinned at this note's banking commit.

---

## 1. What Go promises, and the subset the corpus exercises

### 1.1 The contract

```go
func StructOf(fields []StructField) Type
```

Go returns *the* struct type containing `fields` — interned, so `StructOf(f) == StructOf(f)` holds as
Go type identity. Each `StructField` supplies `Name`, `Type`, and optionally `Tag`, `PkgPath` (required
and only permitted for an unexported field) and `Anonymous` (embedding). Go panics, by its own
messages, on: a field with no name, an invalid name, no type, an unexported field missing `PkgPath`,
an anonymous field *with* `PkgPath` set, a duplicate field name, and a struct whose size would exceed
the virtual address space. Its doc comment carries one documented gap of its own —
*"StructOf currently does not support promoted methods of embedded fields"*
(`src/core/reflect/type.cs:2056`, and Go's `type.go:2354`).

Two properties of the contract are load-bearing here and easy to miss:

1. **Interning is observable.** `encoding/gob` keys `types map[reflect.Type]gobType` and
   `enc.sent map[reflect.Type]typeId` on the returned `Type`. A `StructOf` that minted a fresh
   descriptor per call would make every recursion a cache miss and every mutually-recursive type an
   infinite regress. Interning is not an optimization; it is the contract. (§4.1 records the one
   place go2cs's interning is narrower than Go's.)
2. **The returned type is *storable*.** `reflect.New(StructOf(...))` must produce a real addressable
   zero value whose fields can be read, written and handed back out through `Interface()`.

### 1.2 The census — every `StructOf` site that exists

**Converted corpus** (`git grep -n StructOf -- src/core`, 33 matching lines):

| Class | Count | Where |
|:--|:--:|:--|
| the auto-converted declaration, its panic strings and comments | 20 | `src/core/reflect/type.cs` (`StructOf` at :2058, `runtimeStructField` at :2435) |
| `runtime.stringStructOf` — an unrelated name | 10 | `runtime/{map_faststr,print,string,tracestring}.cs`, `runtime/{windows,linux,darwin}/arena.cs` |
| `calcStructOffset` in a Go source citation | 2 | `go/types/{sizes,gcsizes}.cs` |
| **real call sites** | **3** | below |

The three real call sites, and what each is:

| Call site | What it is | Reachable? |
|:--|:--|:--|
| `src/core/reflect/type.cs:1571` — `funcTypes[n] = StructOf(...)` in `initFuncTypes` | Go builds a fake struct to *describe a func's argument frame in memory* for `funcLayout` | **No.** `Value.Call` is hand-owned (`value_impl.cs`) and invokes the boxed delegate; the frame layout has no managed meaning |
| `src/core/reflect/type.cs:2260` — `New(StructOf(...))` inside `StructOf` itself | Go builds `struct{S structType; U uncommonType; M [n]Method}` to get *an rtype followed in memory by a method array* | **No.** Same class: a linker-layout reconstruction. Reached only from the `len(methods) != 0` arm — embedded fields with methods, which Go's own doc comment says StructOf does not support |
| `src/core/encoding/json/bench_test.cs:613` — `types[i] = reflect.StructOf(fs)` | `BenchmarkTypeFieldsCache`, minting `maxTypes` distinct single-field structs (10⁶, or 10³ on a builder) | **Compiles, never runs.** Phase-4D defers every `Example`/`Benchmark` declaration from the run registry (`src/go2cs/testConversion.go:1342`), so it is a *latent* consumer — and §5.1 shows it is the one that prices the mechanism |

Both in-`reflect` callers are **self-hosting layout tricks**. This is the same argument the board made
for `ArrayOf` — *"the auto form dies in `typesByString` only because it is reconstructing Go's
linker-allocated `arrayType` record, which the managed bridge never needs to"* — and it means a
hand-owned `StructOf` **is not obliged to satisfy reflect's own two callers**. Worth stating plainly,
because a first read of the census suggests StructOf is load-bearing inside reflect and it is not.

**GOROOT 1.23.12**, outside `reflect` itself and outside `cmd/` (which go2cs does not convert):
**5 call sites in 2 packages** — `encoding/gob/gobencdec_test.go` (4, all inside one test) and
`encoding/json/bench_test.go` (1). There is no third consumer in the standard library.

*(Both census tables were re-verified exact by the review lane and are unamended.)*

### 1.3 The subset the corpus actually exercises

| Contract feature | Exercised? |
|:--|:--|
| exported, named, non-embedded field | **yes** — every site |
| a field whose type is itself synthesized (`ArrayOf`, `StructOf`) | **yes** — gob, 101 levels deep |
| `StructField.Index` populated by the caller | yes (json's bench; ignored by Go, recomputed) |
| interning / `Type` as a map key | **yes** — gob's `types`, `enc.sent` |
| `reflect.New` over the result, then `.Interface()` | **yes** — gob |
| field tags | no |
| unexported fields + `PkgPath` | no |
| embedded (`Anonymous`) fields | no |
| embedded fields *with methods* | no — and Go does not support them either |
| directional-channel field types | no |

That table is the design's scope statement, and §6 turns it into a contract subset with a loud panic
at its boundary. It is **not** a licence to implement gob's shape: the difference between scoping to a
contract subset and scoping to a test is the whole of §6.3.

### 1.4 gob's residual, named — and what it does *not* witness

`TestIgnoreDepthLimit`, `<GOROOT>/src/encoding/gob/gobencdec_test.go:801`. It builds a 101-deep nested
array with `ArrayOf`, wraps it in a `StructOf`, encodes it, and asserts the decoder rejects the depth
with the exact error `"invalid nesting depth"` (`decode.go:911-919`); it then repeats the assertion
with a 101-deep nested *struct* built entirely from `StructOf`. The board measured `encoding/gob` at
**105 of 106 matching verdicts** with this one row divergent (lane `claude/map-key-elem-cargo`,
2026-08-20, on Go 1.23.1).

**That denominator survives the corpus hop.** Measured this lane: all **21** files of `encoding/gob`
are byte-identical between Go 1.23.1 and Go 1.23.12 (`diff` over the directory, 0 of 21 differing,
with `runtime/proc.go` as the positive control confirming the comparison can go red). So unlike the
four rows the hop re-derived, gob's 106 needs no re-derivation before it is used as row math — though
§9 still re-measures it, because a *measurement* banks a row, never an argument that nothing moved.

**What the test demands of the bridge, read off gob's own source:**

| gob code | Demands |
|:--|:--|
| `buildTypeInfo` → `name := rt.String()` | `Type.String()` renders the synthetic struct |
| `newTypeObject`, struct arm (`type.go:535-563`) | `NumField()`, `Field(i)` with truthful `.Name`, `.PkgPath`, `.Type`, `.Tag`; `isSent` reads `.Name` for exportedness |
| `getType` → `types[rt]` (`map[reflect.Type]gobType`) | the descriptor is a correct, interned **map key** |
| `compileEnc` → `encInstr{…, f.Index, …}` | `StructField.Index` is `[]int{i}` |
| `encodeStruct` (`encode.go:306-330`) → `value.NumField()`, `value.FieldByIndex(instr.index)` | `Value.NumField`, `Value.FieldByIndex` over a synthesized instance |
| `encodeArray` (`encode.go:333-353`) → `value.Index(i)` ×101 | real storage 101 levels deep, walked by the ordinary `Value.Index` |
| `validUserType` → `implementsInterface` (GobEncoder/Decoder/BinaryMarshaler/…) | `Type.Implements` answers **false**, and does not throw, for a type no generator ever saw (§7.3) |

⚠ **AM-6 — the test is CORROBORATION, not proof, and the draft over-claimed it.** The assertion is on
the **decoder's** error, produced from the wire **type graph**; the test **discards the encoder's
error** (`enc.Encode(badStruct.Interface())`, return value unused). So `TestIgnoreDepthLimit`
witnesses neither array **lengths** nor element **storage**: a `StructOf` that lost every dimension —
emitting `[0][0]…int` — would still produce a 101-deep wire type graph and still go green. The bottom
two rows of that table are what an **honest implementation owes**, not what this test checks.

The load-bearing proofs are therefore elsewhere, and §9 makes them gates in their own right:

1. the **`ReflectStructOf` behavioral test compared against `go run`** — the only gate that checks the answer against Go rather than against our own expectation; and
2. the **GolibTests dims round-trip** (`ArrayOf`-composed field → `Field(0).Type.Len()` → the stored element).

The gob row corroborates those. It does not substitute for them, and a design that treated the row as
the proof would be exactly the fabrication §6.3 rules out.

---

## 2. The mechanism today, and exactly where the auto form dies

### 2.1 How the bridge mints a type descriptor

Three layers, and the bottom one is a CLR `System.Type`:

| Layer | Type | Where |
|:--|:--|:--|
| descriptor | `ж<abi.Type>` | `src/core/internal/abi/type.cs:19` |
| reflect wrapper | `ж<rtype>` | `src/core/reflect/type.cs:283` |
| client-facing | `ΔType` (concretely `rtypeжΔType`) | minted at `src/core/reflect/value_impl.cs:1492` |

The *truth* is none of those. It is the `System.Type` carried on the descriptor as companion cargo
(`src/core/internal/abi/type_impl.cs:54-60`):

```csharp
partial struct Type {
    [GoReflectCompanion] public System.Type? sysType;
    [GoReflectCompanion] public nint[]? arrayDims;
    [GoReflectCompanion] public nint[]?[]? funcParamDims;
    [GoReflectCompanion] public GoChanDir chanDir;
    [GoReflectCompanion] public nint[]? keyDims;
}
```

`abi.synthType(System.Type, dims, funcParamDims, chanDir, keyDims)` (`type_impl.cs:90`) is the sole
mint, interning on `(System.Type, descriptorDimsKey(...))` in `s_descriptors` (`:80`);
`reflect.canonType` (`value_impl.cs:1468-1492`) interns the `ΔType` wrapper on the **same** key.
Everything a struct descriptor answers is derived, memoized per `System.Type`, from CLR metadata plus
converter-emitted attributes **and — the point AM-1 turns on — from a cached zero instance**:

- **Kind** — `GoReflect.KindOf` (`GoReflect.cs:129-220`); its last arm is literally `if (t.IsValueType) return Struct;` (`:204`), and anything reference-typed that falls through is classified `Pointer` (`:219`).
- **Fields** — `GoReflect.GoFields(Type)` (`GoReflect.FieldAccess.cs:322`), memoized in `s_goFields`, projecting real `FieldInfo`s to Go names, tags (`[GoTag]`), embeddedness, declaration order — and dims, by the route §4.3 corrects.
- **Offsets/size/align** — `GoReflect.GoFieldOffsets` → `structLayoutOf` (`GoReflect.TypeLayout.cs:139, 190-233`): one memoized pass computing **Go amd64** offsets over the Go-projected field list, never `Marshal.OffsetOf`.
- **Name/String** — `GoReflect.GoTypeName` / `HasGoName` (`GoReflect.TypeNaming.cs:195, 233-274`); a `[GoType("dyn")]` stamp with no `[GoLocalName]` makes `HasGoName` answer **false**, so `Name()` is `""` and `String()` renders structurally through `goStructTypeString` (`:360-391`).
- **PkgPath** — `GoPackagePath(t)` = `GoPackageClassPath(t.DeclaringType)` (`TypeNaming.cs`), derived purely from the declaring class's **namespace plus package-class name**. §4.4 uses that. ⚠ `rtype.PkgPath` (`value_impl.cs:1547`) has **no `HasGoName` gate**, so a type's `PkgPath()` and its `StructField.PkgPath` can answer differently — §9 gates the pair.
- **Storage** — `reflect.New` (`value_impl.cs:1079-1092`) = `GoReflect.NewPointerBox(st, GoReflect.ZeroValueOf(st, …))`, whose struct arm is `Activator.CreateInstance(t)` (`ValueMarshalling.cs:329`).
- **Addressable field access** — `Value.Field` (`value_impl.cs:784-823`) obtains a write-through alias via `GoReflect.FieldAliasBox` → `buildFieldAccessor` (`FieldAccess.cs:573-603`), which emits a **`DynamicMethod`** doing `Castclass` → `callvirt ValueSlot` → a chain of `Ldflda` on real `FieldInfo`s.

**Not one of those looks at where a `System.Type` came from.** That single observation is what makes §4
cheap and §3.2 expensive.

### 2.2 Why the current machinery cannot satisfy StructOf

Because every entry point is a *function of an existing `System.Type`*, and `StructOf` is asked to
produce a Go type for which **no `System.Type` exists**. There is no synthesis direction in the bridge
at all. `PointerTo` (`value_impl.cs:2131-2137`) is the one hand-owned type constructor, and it works
precisely because `typeof(ж<>).MakeGenericType(st)` hands it an existing CLR type; the same idiom
serves `abi.synthesizeArrayType`'s internal `typeof(slice<>).MakeGenericType(elem)`
(`type_impl.cs:411-429`), `rtype.Elem` and `Value.Slice`. A struct has no generic container to
instantiate.

Three constraints follow, and they are **forced, not chosen**:

1. **The descriptor must carry a real CLR `System.Type`.** `canonType` `Debug.Assert`s on a null `sysType` (`value_impl.cs:1481-1484`), and that assert *kills the process* (0x80131623) — documented at `value_impl.cs:1735` as having already happened once, via `rtype.FieldByIndex`.
2. **It must be a CLR value type**, or `KindOf` calls it `Pointer`.
3. **Its fields must be real `FieldInfo`s reachable by `Ldflda`** through `ж<T>.ValueSlot`, or `Value.Field` cannot produce a write-through alias and `Activator.CreateInstance` cannot make a zero value.

### 2.3 Where the auto form dies, precisely

`typelinks` is a bodyless partial (`src/core/reflect/type.cs:1322`) that `go2cs-gen`'s
`PartialStubGenerator` fills with a throw (`src/gen/go2cs-gen/PartialStubGenerator.cs:111`), so the
runtime message is exactly:

```
typelinks: external (assembly or cgo) function is not implemented
```

`typesByString` calls it on its first line (`type.cs:1356`), and **seven** auto type constructors call
`typesByString`:

| Function | call site | status |
|:--|:--|:--|
| `ptrTo` | `type.cs:1163` | bypassed — `PointerTo` and `Value.Addr` are hand-owned |
| `ChanOf` | `type.cs:1458` | dead |
| `MapOf` | `type.cs:1502` | dead |
| `FuncOf` | `type.cs:1670` | dead |
| `SliceOf` | `type.cs:1941` | dead |
| **`StructOf`** | **`type.cs:2333`** | **dead** |
| `ArrayOf` | `type.cs:2510` | dead — the concurrent lane's target |

So the *observed* failure is `typelinks`, and it is a red herring in the same way the board said it was
for `ArrayOf`: `StructOf` reaches `typesByString` only in its "look in known types" step, and even if
that step were satisfied, everything after it — `structTypeFixedN` prototypes, GC-program construction,
`resolveReflectName` into the linker's name blob, `unsafe_New` — is Go's runtime reconstructing linker
output. **The auto form is unreachable in principle, not blocked by one stub.** A hand-own is the only
route, which is why §3 does not consider "implement `typelinks`" as an option.

---

## 3. The mechanism space, priced

Four shapes were considered. The axes that decide it: how many places in the bridge grow a **second
path** (the repo's dominant reflection failure mode — a synthetic path answering a question by a
different rule than the converted path, and therefore what a green row proves); **cost per synthesized
type**; and **blast radius** at the gate.

| | new forks in the bridge | cost / type (measured, §5.1) | ceiling | verdict |
|:--|:--|:--|:--|:--|
| **A. ⭐ `TypeBuilder` synthesis** — mint a real CLR struct; the descriptor is derived from it exactly as for a converted struct | **zero** | 925 µs (net10, stamped) | 10⁶ types ≈ 15.4 min / ~2.7 GB | **recommended** (§8) |
| **B. Descriptor-only synthetic type** — an `abi.Type` with computed offsets and no backing CLR type | **≈ 10**, listed in §3.2 | ~1 µs | none | rejected on fork count |
| **C. Generic-arity-backed struct** — `SynthStruct<T1…Tn>` in golib, instantiated by `MakeGenericType`; names/tags/dims ride as new descriptor cargo | **2–3** (`GoFields`, `GoTypeName`, the identity key) | ~1 µs after first instantiation | arity cap | **the escape hatch** (§6.4) |
| **D. Implement `typelinks`** | — | — | — | not a candidate (§2.3) |

### 3.1 A — `TypeBuilder` synthesis

Mint one process-wide `AssemblyBuilder`/`ModuleBuilder`; per synthesized Go struct emit a `TypeBuilder`
with `TypeAttributes.SequentialLayout | Sealed`, parent `typeof(ValueType)`, one public field per Go
field in Go declaration order, plus:

| Element | Carries | Read by |
|:--|:--|:--|
| `[GoType("dyn")]` on the type | "this is a Go-anonymous struct" | `HasGoName` → `Name()==""`; `GoTypeName` → structural `String()`. **Forced**, and §5.1 measures that it costs +73–78% of the mint |
| **a parameterless constructor** initializing every array-kinded field (§4.3) | an array field's **dimensions**, recovered from the zero instance | `GoReflect.FieldArrayDims` — ⚠ **AM-1**, the route the draft got wrong |
| `[GoTag("…")]` on a field (aliased to `DescriptionAttribute`) | the Go struct tag | `GoReflect.goTagOf` |
| `[GoArrayDims(…)]` on a field | a **pointer**- or **map-elem**-hop field's Go dimensions | `GoReflect.FieldStampedDims` |
| `[GoMapKeyDims(…)]` on a field | a map field's key dimensions | `GoReflect.FieldMapKeyDims` |
| `ʗ`-prefixed CLR field name for an embedded field (§4.2) | `StructField.Anonymous` | `GoReflect.collectGoFields` |
| nesting inside a synthesized `<pkg>_package` container (§4.4) | the Go import path for unexported fields' `PkgPath` | `GoPackageClassPath` |

Then `abi.synthType(builtType)` and everything downstream — `GoFields`, `structLayoutOf`,
`GoFieldOffsets`, `structFieldOf`, `FieldAliasBox`, `ZeroValueOf`, `haveIdenticalUnderlyingType`,
`GoTypeName`, `canonType` — runs **unmodified**, because none of it asks where a `Type` came from.

**What works:** everything in §1.4's demand table, by construction.
**What breaks:** nothing measured. Two ceilings: §5.1's per-type cost, and the **~12+** `Type`-keyed
caches that permanently root every synthesized type (§10, OQ-5) — fine at gob's 101, not at json's 10⁶.
**Blast radius:** one entry in `manualConversionFuncs`, one new golib file, one hand-own in
`reflect/*_impl.cs`. No converter *emission* change, so no corpus regen and no golden movement — but
the `reflect` package's own `type.cs` **and `package_info.cs`** do change (§4.6).
**Honest-implementation test:** the row must pass with gob's encoder walking the synthetic type through
the *same* `GoFields`/`structLayoutOf`/`FieldAliasBox` machinery every converted struct uses. If any of
those grew a synthetic branch, the green row would prove the branch rather than the bridge. Under A
they cannot, because there is nothing to branch on.

### 3.2 B — descriptor-only synthetic type

An `abi.Type` with `sysType == null`, a computed field table and computed offsets; values backed by
some `GoSyntheticStruct { object[] fields }` container.

**What works:** unlimited types at ~1 µs each; the only shape Native-AOT-clean by construction; offsets
are already Go-computed so the layout half is nearly free.

**What breaks** — each item a place that must grow a second path:

1. `canonType`'s null-`sysType` assert (`value_impl.cs:1481`) — today a **process kill**.
2. `GoReflect.KindOf` — must answer `Struct` from the descriptor, not the CLR type.
3. `GoReflect.GoFields` — `Type`-keyed and memoized; a synthetic type has no `FieldInfo`s at all.
4. `structLayoutOf` / `GoFieldOffsets` / `GoSizeOf` / `GoAlignOf` — same key problem.
5. `GoTypeName` / `HasGoName` / `goStructTypeString` — `String()` rendered from the descriptor.
6. `reflect.New` → `ZeroValueOf` → `Activator.CreateInstance` — needs a synthetic allocator.
7. `Value.Field` → `FieldAliasBox` → the `Ldflda` `DynamicMethod` — needs a synthetic slot-alias.
8. `Value.Set` / `Set*` → `WritePointerSlot` over `ж<T>.ValueSlot` — needs a synthetic write path.
9. `Value.Interface()` → the packed object's `GetType()` is what `abi.TypeOf`/`GoDynamicTypeOf` reads back. **Every synthetic struct would report the same CLR type**, so all of them collapse to one descriptor unless the *value* carries its own Go type — a change to the deepest assumption in the bridge.
10. `haveIdenticalUnderlyingType` / `Implements` / `AssignableTo` — the struct arm compares field tables that no longer come from one source.

**Blast radius:** the whole bridge, plus the `internal/reflectlite` mirror — the exact shape whose
divergences the board has recorded repeatedly (`rtype.String` answering `""` for every type;
`haveIdenticalUnderlyingType` returning `true` for any two structs). **The honest-implementation risk is
not that B cannot be made to work — it is that ten forks give ten places for the synthetic answer and
the converted answer to disagree, and gob's row would only ever exercise the synthetic one.** That
decides against B, not the line count.

### 3.3 C — generic-arity-backed struct

Declare in golib `readonly struct SynthStruct<T1>`, `<T1,T2>`, … up to some arity *n*, each with public
fields `F1…Fn`; `StructOf` instantiates `typeof(SynthStruct<,>).MakeGenericType(clrFieldTypes)` and
carries the Go **names**, **tags**, **embeddedness** and per-field **dims** as new descriptor cargo,
widening `descriptorDimsKey` the way `keyDims` widened it in the map-dims arc.

**What works:** real `FieldInfo`s (so `FieldAliasBox`, `Ldflda`, `Activator` and `Set` all work
unmodified — B's items 1, 2, 6, 7, 8, 9 all vanish); no `AssemblyBuilder`; near-zero per-type cost
after the first instantiation of each arity, which is what makes it the only shape that survives
json's 10⁶.

**What breaks:** two forks remain — `GoFields` must prefer descriptor cargo over `FieldInfo` metadata
for these types (and `GoFields` is `Type`-keyed, so the cargo cannot reach it without a signature
change or a parallel entry point), and `GoTypeName` likewise. Field *identity* moves into the cargo,
so two Go structs differing only in a field **name** share one CLR type and are distinguished only by
the widened key — correct, but it makes the key load-bearing for correctness rather than for caching.
And the arity is capped; beyond it there must be a loud panic, never a truncation.

**Blast radius:** golib (the `SynthStruct` family plus the cargo), `abi` (the key), `GoFields`,
`GoTypeName`. Real, but an order less than B.

### 3.4 What is FORCED and what is CHOSEN

**Forced** (by mechanism, not preference): a real `System.Type` on the descriptor; a CLR **value**
type; real `FieldInfo`s for `Ldflda`; **interning on Go struct identity** (gob's `map[reflect.Type]`
keys) with the **dims in the key** (§4.1); **a parameterless constructor** for array-kinded fields
(§4.3); `ʗ`-prefixed CLR names for embedded fields (§4.2); `[GoType("dyn")]` on the type, because
without it `Name()` answers a CLR name and `String()` is wrong — and it is the single largest term in
the mint cost (§5.1).

**Chosen** (and reversible): `TypeBuilder` over C's generic family; one dynamic assembly per process;
non-collectible `Run` (§10, OQ-2); the increment-1 contract subset (§6). §6.4 makes the reversibility
structural.

---

## 4. Mechanism A, in detail

### 4.1 The interning key — and the mint it must hold

Go interns on struct identity: the ordered list of (name, pkgpath, type, tag, embedded). The converter
already proves this key is sufficient for the *managed* half — its compile-time anonymous-struct dedup
keys on `go/types`' `types.String()` **including field tags**, package-scoped
(`src/go2cs/visitStructType.go:84-90`), and the board records the consequence of getting it wrong:
*"repeated textual occurrences of `struct{ A Struct }` … must lift to a SINGLE C# type, or
reflect.Type identity splits per occurrence"*.

⚠ **AM-2 — a `System.Type` is not enough, and the draft's key was under-specified.** `ArrayOf(1,int)`
and `ArrayOf(2,int)` are **one** CLR type, `array<nint>`; the length lives only in descriptor cargo.
A shape key built from field CLR types alone would therefore intern `struct{F [1]int}` and
`struct{F [2]int}` to the same synthesized type. The key must render each field's **dims**, **keyDims**
and **chanDir** exactly as `abi.descriptorDimsKey` does (`internal/abi/type_impl.cs:100-130`) — reusing
that renderer rather than restating it, so the two can never diverge.

⚠ **AM-4 — the intern must hold the MINT, not just the lookup.** `ConcurrentDictionary.GetOrAdd` runs
its factory **concurrently** on racing threads and keeps one result; the losers' work is discarded.
Here the work is `DefineType`, and a duplicate type name **throws** — measured by the review lane at
**3 of 4 threads failing**. So the mint is guarded by a lock or a `Lazy<T>` value (`GetOrAdd` returning
a `Lazy<Type>` whose `Value` is forced outside the factory), never by a bare `GetOrAdd` over the mint
itself.

With that, `synthType` and `canonType` intern the descriptor and the wrapper on top, so
`StructOf(f) == StructOf(f)` holds at all three layers. This is the runtime analogue of
`structLookupCache` (`reflect/type.cs:1964`) and of the converter's deterministic-winner registry
(`src/go2cs/dynamicTypeOperations.go:91-97`).

⚠ **AM-8 — one narrowing, recorded rather than discovered.** go2cs's `StructOf` interns against **other
`StructOf` results only**. A converter-lifted anonymous struct of the same shape is a *different* CLR
type, so:

```go
type S = struct{ F int }
StructOf([]StructField{{Name: "F", Type: TypeFor[int]()}}) == TypeOf(S{})   // Go: true.  go2cs: FALSE
```

`haveIdenticalUnderlyingType` still answers **true** for the pair, so `AssignableTo`, `ConvertibleTo`,
`Convert` and assignment all behave; only `==` on the `Type` splits. This is the same class as the
board's cross-context anonymous-lift identity split, and it is a *narrowing to record*, not a
disclosure — no measured consumer compares a synthesized type to a literal's type, and §12 keeps it
visible for whoever meets it.

### 4.2 Field order, and embedded-field naming

⚠ **AM-3 — the draft's all-fields-constructor proposal is DROPPED, and a real requirement replaces it.**
The draft proposed emitting an all-fields constructor so `reorderToGoDeclarationOrder`
(`FieldAccess.cs:394`) would be consistent. The review lane measured it a **no-op** — a `TypeBuilder`
type emitting fields already in Go order needs no reorder whichever branch runs — and it would have
added a fragile bijection between constructor parameter names and field names for nothing.

What *is* required is naming. `collectGoFields` decides `StructField.Anonymous` from the **`ʗ` prefix**
on the CLR field name (`Symbols.cs:29`, `CapturedVarMarker`), not from any attribute. So an embedded
field must be emitted as `ʗ<GoName>`, or `Anonymous` is silently `false`.

**The guard must assert `.Anonymous`, not `Type.String()`** — an embedded field and a same-named
regular field render **identically** in `String()`, so a `String()`-based guard cannot go red on this
defect and would be one more green that proves nothing.

### 4.3 Dims — where the ArrayOf lane composes, and the draft's largest error

⚠ **AM-1 — CONFIRMED DEFECT.** The draft asserted that `[GoArrayDims]` on the `FieldBuilder` is "the
only route by which `GoFields` recovers a field's dims". **It is not the route for an array-kinded
field at all.** `collectGoFields` reads an array field's dimensions from a **cached zero instance** of
the declaring struct — `GoReflect.FieldArrayDims` over `Activator.CreateInstance`
(`GoReflect.FieldAccess.cs:388`; `GoReflect.TypeLayout.cs:316-323`; `value_impl.cs:2116-2120`) —
because in converted code the converter emits the length as a **field initializer** (`= new(N)`) that
the generated parameterless constructor runs. The board said as much in the map-dims arc:
*"`FieldArrayDims` reads `= new(N)` back off `Activator.CreateInstance(declaringType)`"*, and the
`[GoArrayDims]` stamp exists precisely for the **pointer** and **map-element** hops, *"on a pointer
field that instance holds a nil pointer with no pointee to measure"*. The draft inverted which route
carries which case.

A `TypeBuilder` struct has no field initializers, so its zero instance measures **zero-length arrays**,
and every synthesized array field would report length 0 — silently, since 0 is a legal length. This is
exactly the defect §1.4's AM-6 shows gob's row **cannot catch**.

**The synthesizer therefore MUST emit a parameterless constructor** that initializes every
array-kinded field, per field:

```
call   object GoReflect::MakeSizedArray(System.Type fieldType, nint[] dims, nint depth=0)
unbox.any <fieldType>
stfld  <field>
```

`Activator.CreateInstance` on a struct with a parameterless constructor calls it, so `FieldArrayDims`
then measures the true lengths. **The `[GoArrayDims]`/`[GoMapKeyDims]` stamps stay as well** — they are
what the pointer and map-key hops read, and the two routes cover disjoint cases. Note this constructor
is *not* the one AM-3 dropped: that was an all-fields constructor for ordering; this is a parameterless
one for dims.

**Measured (review lane): 89 ms** for the first instantiation of gob's 101-deep field — a one-time cost
paid at first `Activator.CreateInstance`, on top of §5.1's mint cost.

**Channel direction takes the same route**, and now nearly free: `GoReflect.FieldChanDir`
(`TypeLayout.cs:441`) also reads a cached zero instance, because the converter emits the direction as a
field initializer (`= channel<@string>.SendOnly`). Since AM-1 makes the constructor mandatory anyway,
carrying direction is one more `stfld` in a constructor that already exists — which is why §10's OQ-8
ruling narrows it now but expects it re-priced immediately.

### 4.4 PkgPath for unexported fields — variant B

`GoPackagePath(t)` is `GoPackageClassPath(t.DeclaringType)`, which derives the import path from the
declaring class's **namespace plus package-class name** (`GoReflect.TypeNaming.cs`):

```csharp
return ns[(EmissionRootNamespace.Length + 1)..].Replace('.', '/') + "/" + pkg;
```

⚠ **The obvious spelling is wrong, measured.** `DefineType("go.encoding.gob.gob_package")` puts the
container in namespace `go.encoding.gob`, yielding `"encoding/gob"` **+ `"/gob"`** = `"encoding/gob/gob"`.

**Variant B, ruled (OQ-6):** the container class is `<pkg>_package` in namespace `go.<parent-path>` —
i.e. `DefineType("go.encoding.gob_package")` for import path `encoding/gob`. And a container is minted
**only when a field actually carries a `PkgPath`**; an all-exported synthesized struct needs none, which
keeps the common case free of a second `DefineType`. Go itself requires all unexported fields of one
`StructOf` call to share a single pkgpath, so one container per type is the right granularity.

⚠ `rtype.PkgPath` (`value_impl.cs:1547`) carries **no `HasGoName` gate**, so a synthesized type's own
`PkgPath()` and its `StructField.PkgPath` are answered by different rules and can disagree. §9 gates the
pair explicitly rather than assuming they agree.

### 4.5 Where the code lands

| File | Change |
|:--|:--|
| `src/go2cs/manualTypeOperations.go` | one entry, `"StructOf": goosAny`, in the `reflect` map |
| `src/core/golib/GoStructSynthesis.cs` *(new)* | the `AssemblyBuilder`/`ModuleBuilder`/`TypeBuilder` minting, the **parameterless-ctor emission** (§4.3), the dims-bearing shape key (§4.1), the mint-holding intern (§4.1), the pkgpath containers (§4.4) |
| `src/core/reflect/value_impl.cs` | `StructOf` — validate per Go's messages, resolve each field's CLR type + cargo, call the synthesizer, `synthType` → `toType` |
| `src/tests/Behavioral/ReflectStructOf` *(new)* | the `go run`-compared guard (§9) — **and its `.csproj` must be registered in `src/go2cs.slnx`** |

Nothing in the converter's **emission** moves, so no corpus regen is owed for the arc itself.

### 4.6 What the hand-own moves in `reflect`'s own emitted files

⚠ **AM-9.** Registering `StructOf` in `manualConversionFuncs` drops it from the convert set and emits a
placeholder comment in its place — which **shifts every subsequent line of `reflect/type.cs`**, and
therefore moves `reflect/package_info.cs`'s **position-map records**
([`DESIGN-position-map.md`](DESIGN-position-map.md)). This is live evidence from the concurrent
`lane-arrayof`, not a projection.

It is the **benign** shape of the `package_info.cs` → `stdlib-metadata.txt` merge preflight CLAUDE.md
added on 2026-08-25: that preflight stops a merge whose `package_info.cs` moved without a matching
`stdlib-metadata.txt` change, and here the two are genuinely decoupled, because **`stdlib-metadata.txt`
carries no `GoPositionMap` lines** — it records `GoImplement`/`GoImplicitConv` records, and this arc
moves none.

**Benign is not "skip it".** `go generate .` in `src/go2cs` must be **run** and **confirmed a no-op**,
with `TestStdLibMetadataInSync` passing as the assertion — never inferred from a clean diff, and never
skipped on the argument above. Three banked regens missed that step in two days for want of exactly
this rule.

---

## 5. Measurements

⚠ **AM-5 — the draft's "this box has no .NET 10 SDK" claim is STRUCK. It was false**: SDK **10.0.400**
is installed side-by-side at `C:\Users\<user>\dotnet10` (the default `dotnet` on `PATH` resolves to
9.0.317, which is what the draft mistook for the whole picture). Both TFMs are measurable here, the
review lane measured both, and §5.1 is replaced with its matrix. The draft's 582 µs figure is
**superseded** — it was an unstamped mint, i.e. not a shippable configuration.

### 5.1 Cost per synthesized type — the review lane's matrix

Eight cells: 2 TFMs × stamped/unstamped × `Run`/`RunAndCollect`, each in an **isolated process** (which
is what corrected the draft's working-set reading). The headline:

| | net9.0 | net10.0 |
|:--|--:|--:|
| **with `[GoType("dyn")]` — the FORCED configuration** | **875 µs** | **925 µs** |
| stamp overhead vs unstamped | **+73%** | **+78%** |
| unstamped *(derived from the band; not a shippable configuration)* | ~506 µs | ~520 µs |
| working set, 10k types, isolated (`Run` / `RunAndCollect`) | **27.0 / 27.0 MB** | — |

Three readings that matter:

1. **The stamp is the dominant term, and it is forced.** `[GoType("dyn")]` is required for correct `Name()`/`String()` (§3.4) and costs +73–78% of the mint. Any cost model built on an unstamped number — as the draft's was — understates the mechanism by nearly half.
2. **The TFM is not the variable.** 875 → 925 µs is ~6% across a whole major runtime version, against the stamp's ~75%. Nothing here is net10-specific.
3. **`Run` and `RunAndCollect` are indistinguishable on working set** — 27.0 vs 27.0 MB in isolated processes. The draft's "+8.5 vs +24.1 MB" was an **in-process measurement-order artifact** (the second mode measured after the first had already warmed the runtime) and is struck; it must not be cited, including as the justification for OQ-2, which §10 re-argues on other grounds.

**Extrapolated at the forced configuration:** gob's 101 struct types ≈ **93 ms** of minting, plus AM-1's
**89 ms** first-instantiation of the 101-deep field — under 0.2 s total, irrelevant beside the suite's
runtime. **json's `BenchmarkTypeFieldsCache` at 10⁶ distinct types ≈ 15.4 minutes and ~2.7 GB**, and
interning cannot help because its types differ by field name by construction. That is the mechanism's
ceiling, stated rather than discovered later (§10, OQ-3).

### 5.2 The 101-deep composition loads

`MakeGenericType` + `Activator.CreateInstance` at **every** level of a nested generic `readonly struct`
(the shape `array<T>` is — `src/core/golib/array.cs:45`) reached depth **130 in 9.4 ms cold on net10.0**
(the draft's "3 ms" was a warm net9 reading and is corrected); the depth-130 type's `FullName` is 10,820
characters. A `TypeBuilder` struct with a field of the 101-deep type built and instantiated in under a
millisecond. So neither the CLR's generic nesting nor `SequentialLayout` over a deep field type is a
wall — worth asking, since 101 levels of value-type instantiation is not an ordinary shape.

### 5.3 Dynamic-code posture

`RuntimeFeature.IsDynamicCodeSupported` is `True` under the JIT and `False` under Native AOT, where
`DefineDynamicAssembly` throws `PlatformNotSupportedException`. **This is not a new class for go2cs.**
golib already emits IL and compiles expression trees at run time in three places:

- `GoReflect.FieldAccess.cs:575` — `DynamicMethod` + `ILGenerator`, the `Ldflda` field-alias accessor **the reflect bridge's entire addressable-field path depends on**
- `ж.Contracts.cs:143` — `DynamicMethod` + `ILGenerator`, `FieldRef<T>.Create<TElem>`
- `GoReflect.MethodSets.cs:311` — `Expression.Lambda(...).Compile()`, under a file-scoped `#pragma warning disable IL3050`

So the reflect bridge is *already* outside a strict AOT profile, and `StructOf` widens the same
dependency rather than introducing one. What remains genuinely unmeasured is whether the perf suite's
`PerfAot` profile (`src/tests/Performance/Directory.Build.targets:11`, `TrimMode=partial`) reaches those
sites today — the one experiment §10's OQ-4 keeps open.

---

## 6. The contract subset — and the line between scoping and fabricating

### 6.1 What increment 1 implements

Exported and unexported named fields; caller-supplied `PkgPath` (§4.4); tags; `Anonymous` (embedded)
fields without methods, emitted with the **`ʗ`-prefixed CLR name** AM-3 requires; any field type the
bridge can already name, including synthesized ones, with **dims carried by the parameterless
constructor** (§4.3); interning; `New`/`Field`/`Set`/`Interface` over the result.

### 6.2 What it panics on, with Go's own messages

Every validation Go performs (`type.cs:2075-2447`): no name, invalid name, no type, unexported without
`PkgPath`, anonymous with `PkgPath`, duplicate field, size overflow. Plus **embedded fields with
methods**, where Go's own `StructOf` documents no support and its implementation reaches the
`uncommonType` layout trick §1.2 rules out — panicking there matches Go's documented behavior rather
than diverging from it. Directional-channel field types are accepted but carry no direction (§4.3, and
§10's OQ-8 ruling expects that re-priced once the constructor exists). Both narrowings, plus §4.1's
interning narrowing, are recorded in `ConversionStrategies-Reference.md` when this lands — not left
implicit.

### 6.3 Why this is scoping, not fabricating

The doctrine is the `host-limit` bar: an entry must name *"a structural property of the deployment
shape, never an unimplemented-but-fixable defect"*, and the charter's *"a disclosure is only for asserts
the CLR provably cannot satisfy"*. **Nothing in this design is a disclosure**, and the board was explicit
that gob's row is not disclosable — *"there is no class to disclose under"*. A priced arc is never a
disclosure, and this note prices an arc.

The scoping line, stated so it can be checked: **it is legitimate to implement a contract subset with a
loud panic at its boundary, and illegitimate to implement the shape of a test.**

⚠ **AM-6 makes that check concrete rather than rhetorical.** `TestIgnoreDepthLimit` asserts on the
decoder's error over the wire type graph and discards the encoder's error, so **a `StructOf` that lost
every array dimension would still turn the row green** — which is precisely the defect AM-1 found in
this note's own draft. A design that pointed at the gob row as its proof would therefore have shipped a
dims-losing implementation and called it validated. The proofs that can actually go red are the
`go run`-compared `ReflectStructOf` behavioral test and the GolibTests dims round-trip; the gob row is
corroboration, and §9 orders the gates accordingly.

The other half of the check is unchanged: gob's encoder must reach the synthetic type through
`GoFields`, `structLayoutOf` and `FieldAliasBox` — the same code paths a converted struct uses — so a
green row proves the bridge and not a bypass. Under mechanism A there is no other machinery available,
which is a stronger guarantee than a promise.

### 6.4 Reversibility is structural

All CLR-type minting sits behind **one** function in `GoStructSynthesis.cs`:

```csharp
internal static Type SynthesizeStructType(ReadOnlySpan<GoSynthField> fields, string pkgPath)
```

Swapping mechanism A for mechanism C later is then one file's body plus the cargo widening, with the
hand-own, the interning, the validation and the guards untouched. That is what makes §5.1's ceiling a
deferred cost rather than a trap, and it is the basis of the OQ-3 ruling.

---

## 7. Interactions

### 7.1 The `reflect.ArrayOf` lane (`claude/reflect-arrayof`)

**Composition, not conflict — and per the OQ-1 ruling, `ArrayOf` merges first and `StructOf` runs as its
own lane afterwards.** `ArrayOf` mints `synthType(typeof(array<>).MakeGenericType(elem), [n, …elemDims])`;
`StructOf` consumes the result as a field type and reproduces its dims **in the synthesized struct's
parameterless constructor** (§4.3), stamping `[GoArrayDims]`/`[GoMapKeyDims]` as well for the pointer and
map-key hops. Three consequences worth agreeing before either lands:

- `StructOf` reads a field type's dims through the **descriptor**, so `ArrayOf` must place them there and nowhere else. It already does.
- gob's row needs **both**, and neither flips it alone — worth restating, because `ArrayOf` landing green invites the reading that gob is nearly banked. It is not: `TestIgnoreDepthLimit` wraps its array in a `StructOf` on the very next line, and its second half is 101 `StructOf` calls with no array at all.
- **The merge result must be swept**, not either tip — CLAUDE.md's banked-row protection rule, whose whole point is that a lane's proof binds its own tree. §4.6's `package_info.cs` movement is shared between the two lanes and is first observable at the union.

### 7.2 The zero-size-field LAYOUT arc (Ruling A, `sync/atomic`)

A synthesized struct's offsets come from `structLayoutOf` — the same memoized Go-amd64 walk that answers
`StructField.Offset` for every converted struct and that Ruling A made `ж<T>.PointerOrderToken`
alignment-truthful against (*"a token whose LOW BITS mirror the Go-computed layout … answers `p & 7` from
the SAME metadata that answers `Offset`"*). Under mechanism A a synthesized struct **inherits that arc for
free and forks nothing**, provided the emitted CLR field order equals the Go field order (§4.2) and no
field is elided.

One boundary to state rather than trip over: C# has no zero-size struct, so a synthesized field whose Go
type is `struct{}` occupies a CLR byte while `structLayoutOf` gives it Go's zero width. That divergence is
**pre-existing and already ruled** — it is exactly the `readonly`-emission remedy the layout arc landed
(*"the field stays DECLARED, so reflect's walk, `NumField()` and `StructField.Offset` still match Go, while
the one unfaithful operation becomes unexpressible"*). Increment 1 emits such a field `initonly` for the
same reason, and no measured consumer synthesizes one.

### 7.3 `GoImplement` / witness machinery

A synthesized type has an **empty Go method set**, which is the correct Go answer: `reflect.StructOf` does
not support promoted methods of embedded fields, so no `StructOf` result has methods.

The question that matters is whether *asking* is safe, because gob asks about every type it sees —
`validUserType` tests `GobEncoder`, `GobDecoder`, `BinaryMarshaler`, `BinaryUnmarshaler` on the way in.
It is: `rtype.Implements` → `GoReflect.GoImplements` →
`ifaceType.IsAssignableFrom(valueType) || valueType.StructurallyImplements(ifaceType)`
(`GoReflect.ValueMarshalling.cs:132-141`), and the runtime tier is **fail-soft by design** —
`AdapterBinder` *"answers `false` for anything it cannot build … A MISS is normal control flow"*
(`AdapterBinder.cs:70-76`). A type no generator ever saw is precisely the case that tier exists for
(*"a dynamic type in an assembly converted AFTER the interface's own"*). So `Implements` answers `false`
without throwing, and `interface{}` — emitted as `object` — is satisfied by the special case `GoImplements`
already carries.

**This is a gate item, not an assumption.** A synthesized type is the first CLR type in the system with
*no* extension-method registration at all, and "answers false" is a claim to be measured (§9).

### 7.4 The `[GoType("dyn")]` machinery

The stamp is required for `Name()`/`String()` (§3.1) and is the mint's dominant cost (§5.1). There are
exactly four other runtime readers of `"dyn"`. Three are naming (`GoTypeName`, the interface arm,
`HasGoName`). The fourth is `Type.IsDynamicType()` (`golib/runtime/TypeExtensions.cs:163-169`), which gates
**struct-to-struct dynamic conversion** in `builtin.TryTypeAssert`.

**Ruled intended (OQ-7):** a synthesized struct *is* a Go anonymous struct, so enrolling it in that
conversion is the correct behavior, not an over-reach. Because it arrives as a side effect of a stamp taken
for a naming reason, it gets its own guard rather than an argument: **a GolibTests row converting between a
synthesized `dyn` type and a converter-lifted `dyn` type of the same shape**, asserting the conversion
succeeds in both directions. That row is also the only gate that would catch §4.1's AM-8 narrowing turning
into something worse than an `==` split.

---

## 8. Recommendation and acceptance

**Take mechanism A**, scoped per §6, behind the single seam of §6.4, as its own lane **after
`claude/reflect-arrayof` merges** (OQ-1).

**Acceptance measurements — the row math.** Roster today: **162 / 215 packages, 18,598 matching verdicts,
85 disclosed** (`docs/ValidatedTestPackages.md`, 2026-08-25). `encoding/gob` measured **105 of 106** on
Go 1.23.1, one divergent row, zero disclosed; its 21 sources are byte-identical at Go 1.23.12 (§1.4), so
the denominator is unmoved. With `ArrayOf` and `StructOf` both landed:

| | before | after |
|:--|--:|--:|
| roster rows | 162 | **163** |
| matching verdicts | 18,598 | **18,704** (+106) |
| disclosed | 85 | **85** (unchanged — nothing here is a disclosure) |
| `encoding/gob` | 105 / 106, `failing` | **106 / 106, banks** |

The +106 is the whole suite, not the delta: a row's contribution is its verdict count, and gob's is 106
(98 top-level `Test` functions plus subtests; 19 `Example`/`Benchmark` declarations excluded by Phase-4D).
**This arithmetic is a projection and does not bank anything** — the measurement is the pipeline run in §9,
and if the re-measured denominator differs the projection loses, not the measurement. Per AM-6, the gob row
is also **not** the arc's correctness proof; §9 gates 2 and 3 are.

**Other consumers of StructOf, measured (§1.2):** there are none beyond gob on the frontier.
`encoding/json`'s single site is `BenchmarkTypeFieldsCache`, already banked at 491 verdicts and unaffected
because Phase-4D never runs it; reflect's own two callers are self-hosting layout tricks a hand-own replaces.
So **`StructOf` buys exactly one row today** — worth saying, because it sets the honest expectation for an
arc of this size and is part of why `ArrayOf` goes first on its own merits (reachable from user code, and
roughly free over the dims cargo).

---

## 9. Gates

Change class: **golib + hand-own + one converter-registry entry** — the widest class the repo has. The list
is not negotiable downward, and gates 2 and 3 are the arc's *correctness* proofs (AM-6); gate 9 is
corroboration.

1. Converter `go test ./...` from `src/go2cs` — `projitemsIntegrity_test` and `TestStdLibMetadataInSync`.
2. **`GolibTests`**, extended with: the shape-key intern test (`StructOf(f) == StructOf(f)`; two shapes differing only in a **tag** distinct; two differing only in an **array length** distinct — the AM-2 case); **the dims round-trip** (`ArrayOf`-composed field → `Field(0).Type.Len()` → stored element, the AM-1 guard); the **embedded-field `.Anonymous`** assertion (AM-3 — asserted on `.Anonymous`, never on `Type.String()`); the **`Implements`-answers-false-without-throwing** test (§7.3); the **`PkgPath` pair** — synthesized `Type.PkgPath()` against `StructField.PkgPath`, which are answered by different rules (§4.4); the **concurrent-mint** test (N threads racing one shape, AM-4); and the **`dyn`-to-`dyn` conversion** row between a synthesized and a converter-lifted type of the same shape (OQ-7).
3. A new behavioral test **`ReflectStructOf`**, compared against `go run` — the only gate that checks the answer against **Go**. Covers `New`/`Field`/`Set`/`Interface` round-tripping, `Type.String()`, and array-field lengths. **Register its `.csproj` in `src/go2cs.slnx`** and run **`check-solution-integrity.ps1`** — the harness builds by path, so an unregistered project passes every suite and only breaks the solution build.
4. `check-no-regression.ps1` — full, expecting byte-identical `.cs` and `.csproj` across the behavioral corpus (nothing here changes emission).
5. Full behavioral suite via `run-behavioral.ps1`.
6. Full `src/go2cs.slnx` build — the only gate compiling the non-generated solution members, owed by any golib API change.
7. Full `go2cs-stdlib.slnx` build at `-p:GoTargetOS=windows` **and** `-p:GoTargetOS=linux`, `--no-incremental`, with `bin`/`obj`/`Generated` purged between target switches.
8. **The reflect-bridge canary set** — the five largest banked reflect consumers **by verdict count, re-derived from `docs/ValidatedTestPackages.md` at gate time, never carried forward from this note**. (CLAUDE.md's rule; the escape it was written about happened precisely because a canary set predated the newest bank.)
9. The `encoding/gob` pipeline: `go2cs -tests -test-action all -test-timeout 10m "<GOROOT>/src/encoding/gob" src/core/encoding/gob`, expecting **106 / 106**. Pass the timeout explicitly — the hand-invoked default is 2m, five times smaller than the sweep's.
10. **`go generate .` in `src/go2cs`, run and confirmed a no-op**, with `TestStdLibMetadataInSync` passing as the assertion — owed because the hand-own placeholder moves `reflect/package_info.cs`'s position-map records (§4.6). Benign is not "skip it".
11. Full `run-validated-sweep.ps1` before banking, and — per CLAUDE.md's banked-row protection rule — a **post-merge filtered sweep of `encoding/gob` at the merge result**, not at the lane tip.
12. **The OQ-4 experiment** (§10): one reflect-exercising perf benchmark published `-p:PerfAot=true`, run **solo**. Named, scoped and expected to close in one line.
13. A **positive control** on the new guards before trusting any of them: regress one synthesized-field dim deliberately, confirm the guard names exactly that site, restore, confirm byte-identical. A green that cannot go red is not a measurement — and AM-6 is this arc's proof that the principle has teeth.

Commit policy on a bank: gob's converted test sources join `src/core/encoding/gob` per the
validated-package commit policy, and the arc's decisions — including §4.1's interning narrowing and
§6.2's two scope narrowings — are recorded in `ConversionStrategies-Reference.md` in the same change.

---

## 10. Rulings (coordinator, 2026-08-25)

The draft's eight open questions are **ruled**. Only OQ-4 remains open, as one named experiment.

- **OQ-1 — Staffing. RULED: its own lane, after `claude/reflect-arrayof` merges.** They compose (§7.1) and gob needs both, but `ArrayOf` is independently useful and an order smaller; sequencing keeps the smaller fix off the larger one's critical path.
- **OQ-2 — `Run` vs `RunAndCollect`. RULED: `Run`** — but **not** on the draft's justification, which AM-5 struck as a measurement-order artifact (isolated processes: 27.0 vs 27.0 MB, indistinguishable). The correct reason is that **collectibility is unreachable**: ~12+ `Type`-keyed caches root every synthesized type for the process's life, so a collectible `AssemblyLoadContext` could never actually unload one. `RunAndCollect` would buy nothing and imply a lifetime story the bridge does not have.
- **OQ-3 — The 10⁶ ceiling. RULED: defer; the §6.4 seam is sufficient.** Restated at the forced configuration: **≈15.4 min / ~2.7 GB**. It is Phase-4D-deferred today, so nothing is owed; mechanism C is designed when Phase 4D actually runs benchmarks, and the seam is what makes that a swap rather than a rewrite.
- **OQ-4 — Native AOT. STAYS OPEN, as ONE named experiment.** Publish a single reflect-exercising perf benchmark with `-p:PerfAot=true` and run it **solo**. The predicted result — that the bridge is *already* outside the supported profile, since `FieldAccess.cs:575`'s `DynamicMethod` is on the addressable-field path — closes it in one line and means `StructOf` owes no new annotation arc. A contrary result means it does. Either way it is a measurement (gate 12), not an opinion.
- **OQ-5 — Cache growth. RULED: defer with OQ-3**, and the draft's "five caches" is corrected: the review lane enumerated **~12+** `Type`-keyed caches, among them `s_goFields`, `s_structLayouts`, `s_zeroInstances`, `s_descriptors` and `s_canonTypeCache`, plus the naming, method-set, field-accessor and adapter-binding caches. The **count** is the point — every one of them roots a synthesized type permanently, which is both why growth is unbounded (OQ-3) and why collectibility is moot (OQ-2).
- **OQ-6 — PkgPath. RULED: nesting, VARIANT B** — class `<pkg>_package` in namespace `go.<parent-path>`, because the obvious spelling is measurably wrong (`DefineType("go.encoding.gob.gob_package")` yields `"encoding/gob/gob"`). Mint a container **only when a field actually carries a `PkgPath`**. Details in §4.4.
- **OQ-7 — The `dyn` stamp's side effect. RULED: intended.** A synthesized struct is a Go anonymous struct, so enrolment in `Type.IsDynamicType()`'s struct-to-struct conversion is correct. Recorded, and given the §7.4 guard row rather than an argument.
- **OQ-8 — Directional-channel fields. RULED: narrow now, and re-price immediately.** AM-1 makes the parameterless constructor mandatory, and `FieldChanDir` reads the same zero instance, so carrying direction becomes one more `stfld` in a constructor that already exists — near-free, where the draft priced it as a separate mechanism. Increment 1 ships without it; the re-pricing is expected in the implementation lane, not deferred to a future note.

---

## 11. Adversarial review (charter §7)

### 11.1 The review lane's findings

The draft was measured against the live bridge — probes on `golib.dll`, real `TypeBuilder` mints, both
TFMs — and returned nine amendments, **three of CONFIRMED-DEFECT class** that would have surfaced
mid-implementation:

| | Draft claimed | Measured | Where |
|:--|:--|:--|:--|
| **AM-1** | `[GoArrayDims]` is "the only route `GoFields` reads" for a field's dims | **False for array-kinded fields** — dims come from a **zero instance**, so a `TypeBuilder` struct without a parameterless constructor reports **length 0 for every synthesized array**, silently | §4.3 |
| **AM-4** | `ConcurrentDictionary` interning | `GetOrAdd` runs factories concurrently; duplicate `DefineType` **throws** — **3 of 4 threads failed** | §4.1 |
| **AM-3** | embedding needs an all-fields constructor | The constructor is a **no-op**; what embedding actually needs is the **`ʗ` name prefix**, and a `String()`-based guard **cannot go red** on its absence | §4.2 |
| AM-2 | shape key over CLR field types | `array<nint>` cannot separate `[1]int` from `[2]int` | §4.1 |
| AM-5 | "no .NET 10 SDK on this box"; 582 µs/type; +8.5 vs +24.1 MB | 10.0.400 installed side-by-side; **925 µs** stamped; working set **indistinguishable** in isolated processes | §5.1 |
| AM-6 | gob's row demands storage 101 deep | The test **discards the encode error** and asserts on the decoder's wire type graph — a **dims-losing `StructOf` still goes green** | §1.4, §6.3 |
| AM-7 | — | Three gates missing: solution registration, the AOT experiment, the `PkgPath` pair | §9 |
| AM-8 | — | `StructOf` interns only against `StructOf` results; `== TypeOf(literal)` splits | §4.1, §12 |
| AM-9 | no emitted-file movement beyond `type.cs` | `reflect/package_info.cs`'s position map moves too | §4.6 |

**AM-1 and AM-6 are one lesson, and it is the sharpest thing in this document.** The draft got the dims
route backwards *and* over-claimed the test that was supposed to catch it — so the two errors covered for
each other, and the arc would have shipped a `StructOf` reporting zero-length arrays with a green gob row
attesting to it. That is the exact shape of the false-green routes CLAUDE.md catalogues: not a gate that
failed, but a gate that could not fail. It is why §9 promotes the `go run`-compared behavioral test and the
dims round-trip above the row, and why gate 13's positive control is spelled with dims as its example.

### 11.2 Objections answered in place

**"`typelinks` is the blocker; implement it and every constructor works."** No. `typesByString` is one step
of `StructOf`, and everything after it reconstructs linker output the managed model does not have (§2.3).
The draft's first framing treated the stub as the wall — the same misreading the board corrected for `ArrayOf`.

**"A is the heavy option; B is the clean one."** B *looks* cleaner because it has no `AssemblyBuilder`. It is
the heavy option measured the way this repo has learned to measure: by how many places the synthetic answer
and the converted answer can disagree — ten (§3.2) against zero. The bridge's recorded failure history is
almost entirely of that kind (`rtype.String` answering `""` for every type; `haveIdenticalUnderlyingType`
returning `true` for any two structs; the reflectlite mirror drifting). A green gob row under B would exercise
only the synthetic path — the shape of proof this campaign has learned not to accept.

**"Reflection.Emit is an AOT regression."** Measured (§5.3): golib already depends on `DynamicMethod` in the
reflect bridge's addressable-field path, so the dependency exists and `StructOf` widens it. Real, and smaller
than the board's phrasing implied — but not a dismissal, which is why OQ-4 survives as a measurement.

**"101 levels of nested generic value types will not load."** Reasonable fear; measured false — depth 130 in
9.4 ms cold on net10 (§5.2). Recorded because it is the kind of question that otherwise gets argued rather
than run.

**"Scoping to a subset is how a row gets faked."** It is, if the subset is drawn around the test. §6.3 draws
the line where the repo's doctrine draws it and — after AM-6 — gives a check that can actually go red.

---

## 12. Deliberately not in scope, and narrowings to carry

**Not in scope:** `SliceOf`, `MapOf`, `ChanOf` and `FuncOf` — all dead auto code on `typesByString` (§2.3),
all with no measured consumer, and all cheaper than `StructOf` once someone needs them (each is one
`MakeGenericType` plus cargo, the `PointerTo` shape). `MakeFunc`. Promoted methods on synthesized embedded
fields — Go does not support them either. Any change to the `internal/reflectlite` mirror: it has no
`StructOf`, and a lane that needs one there owes its own measurement.

**Narrowings this arc ships with, recorded so they are met knowingly rather than re-discovered:**

1. **Interning is `StructOf`-local** (§4.1, AM-8). `StructOf(f) == TypeOf(structLiteral)` is `false` in go2cs and `true` in Go; `haveIdenticalUnderlyingType` still answers `true`, so only `==` splits. Same class as the board's cross-context anonymous-lift identity split. No measured consumer compares the two.
2. **Directional-channel fields carry no direction** (§4.3, §6.2) — narrowed by the OQ-8 ruling and expected to be re-priced in the implementation lane, since the constructor AM-1 forces makes it nearly free.
3. **Embedded fields with methods panic** (§6.2) — matching Go's own documented gap rather than diverging from it.

<!-- {% endraw %} -->
