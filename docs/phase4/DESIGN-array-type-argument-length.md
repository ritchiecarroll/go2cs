# DESIGN: the length of an unnamed array type argument reaches a type-only descriptor

Status: REVISION 2 (P6, 2026-11-25). Revision 1 `62e34699e5`: P3 APPROVE WITH CHANGES (17:04Z); COORD ruled
section 7 (17:06Z: overload, scope abi + unique, tier 2). This revision folds P3's MUST 1 (3.5), SHOULDs 2 (3.3),
3 (3.1) and 4 (section 5, rows) and NIT 5 (section 5, risk). P3 re-reads, then code.
Base: master `1c556c35a5`. Retires half 2 of `unique`'s TestMakeAllocs disclosure and closes the type-only half of
the BOARD row of 2026-10-16 ("a type-only descriptor for an UNNAMED array type argument carries no length").

## 1. The gap

C# spells every Go `[N]E` as the one `array<E>`, so the length lives in the instance, never in the type. A
descriptor built from the TYPE alone, `abi.TypeFor[[2]string]()`, therefore has no length. Since H8
(`b8cbf9b047`) such a descriptor is marked size-unknown and `Size()`, `Len()` and `ArrayType()` refuse by name:

    panic: runtime error: the size of [?]string is not known (the length of [?]string is not carried for an unnamed array type argument)

`unique.Make[T]` reads `abi.TypeFor[T]().Size()` first (handle.go:35), so `Make([2]string{a, b})` panics. That is
the disclosed half 2 of TestMakeAllocs (legs "pair", "pair from stack", "pair to any"; the panic in the warm-up call
stops the test before the last two run). Before H8 the same call answered size 0, took Go's zero-size arm and
gave two different pairs one handle, which is why the refusal stays until the length can travel.

## 2. What already exists, and the one piece missing

- **The array-dims companion** (`src/go2cs/arrayDimsCompanion.go`, golib `GoArrayDims.cs`, C1, 2026-10-17). A
  generic that builds a zero value of its type parameter (`var x T`, `new(T)`) and that an own-package
  instantiation passes an unnamed array gains one extra C# type parameter `Tᴀ : IGoArrayDims`. Each call site
  supplies a phantom `ᴀ16`, `ᴀ2_3` or `ᴀnone`, and the body reads `GoArrayZero.Of<T, Tᴀ>()`. A forwarding generic
  is in scope by fixpoint. SCOPE: unexported functions only. An exported generic in the same shape is REFUSED at
  the call site (`arrayDimsRefusal`), because changing its arity moves every consumer assembly.
- **Descriptor interning keys on dims.** `synthType(st, arrayDims, …)` interns on `(System.Type, dimsKey)`
  (type_impl.cs:200), and `TypeOf(value)` for a live one-dimension array goes through `s_oneDimDescriptors` into
  the same `synthType`. So `synthType(typeof(array<@string>), [2])` and `TypeOf([2]string{…})` are ONE box, which
  is the identity `unique` needs: one map per Go type `[2]string`, a different one for `[3]string`.

Missing: (a) a type-only SINK that accepts the companion, and (b) a way for an EXPORTED generic to accept it
without moving its consumers.

## 3. The design

### 3.1 golib/abi: a companion overload of the sink

`internal/abi`'s hand-owned `type_impl.cs` gains, beside `TypeFor<T>()`:

```csharp
public static ж<Type> TypeFor<T, Tᴀ>() where Tᴀ : IGoArrayDims {
    if (Tᴀ.Dims.Length == 0) {
        return TypeFor<T>();
    }
    if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(array<>)) {
        throw panic($"abi.TypeFor: array dims [{string.Join(",", Tᴀ.Dims)}] for {typeof(T)}, which is not an array");
    }
    return synthType(typeof(T), Tᴀ.Dims);
}
```

`ᴀnone` answers exactly what `TypeFor<T>()` answers today, so no existing reading moves. With dims it answers the
interned box a live `[N]E` value of the same type answers. Dims with a `T` that is not `array<>` (a hand-written
or mismatched call) panic by name rather than intern a descriptor for a type that does not exist; the converter
never emits that pairing, because a dims phantom is supplied only for an unnamed array type argument. The
multi-dimension case (`[2][3]E`, phantom `ᴀ2_3`) is `array<array<E>>` with dims `[2, 3]`, the form `synthType`
already takes.

### 3.2 converter: a type-only sink is a companion sink

`zeroSinkTypeParams` is widened to `companionSinkTypeParams`: a type parameter also counts when the body passes it
as the type argument of `internal/abi.TypeFor`. The converter then emits the sink call as
`abi.TypeFor<T, Tᴀ>()` inside a companion-bearing body. The forwarding fixpoint is unchanged, so a generic that
passes its `T` to such a function forwards its own `Tᴀ`.

`reflect.TypeFor` and `reflect.TypeAssert` are the same shape (both reach `abi.TypeFor[T]` in Go, both hand-owned
here). They are NOT in this cut: their only array instantiations are reflect_test's `testTypeAssert[[2]int]` and
`typeAssertAllocs[[128]int]`, and the BOARD row's suspect about TypeAssert's identity waits for its own reading.
Adding them later is one hand-owned overload each plus one entry in the sink set.

### 3.3 an exported generic gains an ADDITIVE overload instead of a refusal

For an EXPORTED top-level generic whose body reaches a companion sink (zero value or type-only), the converter
emits BOTH arities:

```csharp
public static Handle<T> Make<T>(T value) => Make<T, ᴀnone>(value);

public static Handle<T> Make<T, Tᴀ>(T value) where Tᴀ : IGoArrayDims {
    // Find the map for type T.
    var typ = abi.TypeFor<T, Tᴀ>();
    …   // the converted body, unchanged apart from the sink call
}
```

The arity-1 method keeps its signature, so every compiled consumer binds as before; adding a method is not a
binary break (ApiCompat reads it as an addition, PublicSurface gains one line). A call site supplies the
arity-2 form ONLY when its type argument at a companion position is an unnamed array
(`Make<array<@string>, ᴀ2>(…)`). Every other call site keeps emitting `Make<T>(…)`, so no consumer's emission
moves. An arity-1 call with an array type argument (a consumer compiled before this change) still reaches the
dimless descriptor and still refuses by name: never a silently different map key.

**Forwarding crosses packages by the same rule.** A generic that passes its own type parameter to an arity-2
function at a companion position is itself a sink-reaching body: if it is unexported it gains a companion under the
existing own-package rule (`arrayDimsCompanionArg` already forwards `Tᴀ`), and if it is exported it gains the
additive overload and the record (3.5), whichever package it lives in. Its arity-1 form forwards `ᴀnone`. So an
exported `wrap.Wrap[T]` that returns `unique.Make(v)` carries dims from its own caller to `Make`.

Why the predicate is "the body reaches a sink" and not "some instantiation passes an array": the instantiation
is in ANOTHER assembly (unique's internal test host calls production `unique.Make`), so the production conversion
cannot see it. The production overload has to exist whether or not a caller needs it.

The refusal (`arrayDimsRefusal`) keeps its role for the cases the overload does not cover: a method (receiver
generic) and a generic type. Whether the zero-value shape of an exported FUNCTION also moves from refusal to
overload is a question this design raises and does not answer; today's census of that shape is 0.

### 3.4 identity rule (the false-green guard)

Store and load must key on the same descriptor. Within `unique`, `Make` stores with `abi.TypeFor<T, Tᴀ>()` and
`checkMapsFor` loads with `abi.TypeFor[T]()`. `checkMapsFor` is unexported and gains a companion only when an
own-package instantiation passes an array; today it is instantiated only with defined types (TestHandle), so it
keeps arity 1, and its loads key on the same descriptor `Make` stored, because a defined array type carries its
own length. If a future test instantiates `checkMapsFor[[2]string]`, the companion rule gives it `Tᴀ` by the same
fixpoint, and the load keys on the same box as the store. A mismatch can only arise through an arity-1 call with an
array, which refuses (3.3).

### 3.5 the call site in another assembly

Both of this design's own targets call from another assembly: the unique test host calls production
`unique.Make`, and the fixture is package `main`. Today every piece of the companion is own-package (the
companion, the refusal and the phantom types, which are declared beside the CALLEE from its own-package
instantiations). Two additions make the call site work across assemblies.

**(a) The phantom is declared in the CALLER's package.** A call site in package P that supplies `ᴀ2` to a foreign
arity-2 function makes P declare `private sealed class ᴀ2 : IGoArrayDims` once, beside the first calling function
by source position, through the same per-package dedupe that places today's callee-owned phantoms (a name P
already declares for its own companion is not declared twice). Two assemblies' `ᴀ2` are different C# types with
equal `Dims`. Identity still holds because `synthType` interns on the dims VALUES, not on the phantom type. Arm: a
GolibTests case declares two distinct `IGoArrayDims` classes with `[2]` and asserts `abi.TypeFor<array<@string>, A>()`
and `abi.TypeFor<array<@string>, B>()` are the same box.

**(b) The producer writes a record; the consumer reads it; no record means arity 1.** A package whose exported
generic gains the overload writes one assembly record per companion position into its `package_info.cs`:

```csharp
[assembly: GoArrayDimsCompanion("Make", 0)]
```

(function name, which Go makes unique per package; type-parameter index). golib gains the attribute, beside
`GoDynamicTypeLiftAttribute`. A consumer reads it from the imported package's `package_info.cs` the way
`GoDynamicTypeLift` records are read (`importOperations.go`), and `stdlib-metadata.txt` carries it for
`-recurse=nuget`, where there is no converted tree (`go generate .` owed in the cut, gated by
`TestStdLibMetadataInSync`). The consumer never re-derives "the body reaches a sink" from the callee's Go
source, so producer and consumer cannot disagree: a consumer converted against an older `go.unique` finds no
record, emits arity 1, and the call refuses by name at run time instead of failing CS0305. This also answers P3's
open point about whether the callee's syntax is loaded at every call site: the record makes it unnecessary. The
unique test host is a different assembly of the same Go package: it reads production's record, which the
`-tests` pipeline writes first (production converts before tests).

## 4. Population (go1.26.11, go/types over std plus tests; the cut re-measures it)

Exported generics whose body passes a type parameter to `abi.TypeFor` directly or by forwarding, in packages the
corpus converts: `unique.Make` (converted, gains the overload), `reflect.TypeFor`, `reflect.TypeAssert`,
`runtime.AddCleanup` (hand-owned; AddCleanup's managed body does not read TypeFor, so it does not change).
`encoding/json/v2`'s four `*Func` generics are in the shape but the package is not in the corpus. So the
converter-emitted overload population is ONE function.

Instantiations with an unnamed array type argument that reach a type-only sink (the BOARD row's census, amended
2026-10-17): 5, of which this cut moves the 2 `unique.Make[[2]string]` in handle_test.go. Production
instantiations: 0, unchanged.

## 5. Predictions (scored as worded at the cut)

1. `unique` row at the cut, linux, as the sweep runs it: TestMakeAllocs still fails and stays disclosed, now under
   half 1's signature alone (" allocs, want "): the pair legs run and read a surplus like the other legs. The panic
   is gone. Half 2 is removed from `go2cs_test_disclosures.json`; the row's matched and disclosed counts (27, 12)
   do not move, because TestMakeAllocs was one disclosed test before and is one after.
2. A new behavioral fixture `UnnamedArrayTypeArgDescriptor`: `unique.Make` over `[2]string{"a","b"}`,
   `[2]string{"a","c"}`, `[2]string{"a","b"}` again and `[3]string{"a","b","c"}`; prints handle equalities and
   `Value()`s, plus the rows below (one sub-library, `wrap`, for the exported forwarder). RED at base by the panic
   text; matches `go run` at the cut.
3. GolibTests identity arm: `abi.TypeFor<array<@string>, ᴀ2>()` is the same box as `abi.TypeOf` of a `[2]string`
   value, differs from the `ᴀ3` box, and `Size()` reads 32 on amd64; `TypeFor<T, ᴀnone>()` is the same box as
   `TypeFor<T>()` for a non-array `T`.
4. Corpus footprint (the seeded reconvert, four flavours): `unique/handle.cs` gains the arity-2 overload and its
   arity-1 forwarder; `unique/PublicSurface` gains one line; `internal/abi/type_impl.cs` (hand-owned) gains the
   overload; `unique/package_info.cs` gains one `GoArrayDimsCompanion` record; golib gains the attribute;
   `stdlib-metadata.txt` gains the record. No other production file moves. Test side: `unique/handle_test.cs` two pair-leg call sites move to
   arity 2.
5. CNR: NO REGRESSION apart from the new fixture; no behavioral program today instantiates a sink-reaching
   generic with an unnamed array (census owed in the cut).
6. ApiCompat: additions only (unique, internal/abi); no suppression.

Near-miss and let-through rows (in the fixture, each with its Go output as the oracle):

| Row | C# call | What `Make` reads | Expected |
|---|---|---|---|
| unnamed `[2]string`, `[3]string` | `Make<array<@string>, ᴀ2>`, `ᴀ3` | dims descriptors, two maps | matches Go (the cure) |
| `*[2]string` | arity 1, `Make<ж<array<@string>>>` | a pointer descriptor: size 8, no dims needed | matches Go, equal by pointer |
| `struct{ a [2]string }` | arity 1, the lifted struct | the struct's size from its stamped field dims | matches Go |
| defined `type pair [2]string` | arity 1, `Make<pair>` | the wrapper's own length | matches Go |
| method value `f := unique.Make[[2]string]` | binds `Make<array<@string>, ᴀ2>` | dims descriptor | matches Go |
| unexported wrapper in `main` | `wrap<array<@string>, ᴀ2>` forwards `Tᴀ` | dims descriptor | matches Go |
| exported `wrap.Wrap` in a sub-library | `Wrap<array<@string>, ᴀ2>`, record read across | dims descriptor | matches Go |

`[][2]string` is not a row: a slice is not comparable, so `Make` cannot take it.

**Prediction risk (P3 NIT 5).** The pair legs run the hand-owned `unique/clone.cs` over a sized array of strings
for the first time. Prediction 1 can miss on `clone`'s array arm rather than on the descriptor; a miss there is
classified before it is called a descriptor fault.

## 6. Red first and arms

- Red commit: the fixture (2) and the GolibTests arm (3), failing by name at base.
- Converter guard: a unit test that an exported sink-reaching generic emits both arities and a call site with an
  array type argument emits arity 2, while a non-array call site stays arity 1. Firing arms: drop the forwarder
  (consumer arity-1 calls fail to compile) and drop the call-site arm (the fixture panics again).
- Neuter arm for the identity guard: make the overload return `synthType(typeof(T), null)`; prediction 3 reds by
  name.
- Guard arm (3.1): `abi.TypeFor<nint, ᴀ2>()` panics by name; with the `array<>` check removed, it returns a box and
  the arm reds.
- Cross-assembly arms (3.5): two phantoms with equal dims, one box; delete the record line from a consumer's input
  and the call site emits arity 1 (a converter unit test reads the emission).

## 7. Rulings (COORD 2026-11-25 17:06Z, on P3's verdict)

1. OVERLOAD for an exported generic that reaches a type-only sink. The zero-value shape of an exported function
   keeps its refusal for now; one rule for exported functions is the end state, named as this rule's follow-up.
2. Scope: `abi.TypeFor` and `unique.Make` now; `reflect.TypeFor` and `reflect.TypeAssert` are their own seat
   with their own reading.
3. Tier 2, `[EditorBrowsable(Never)]`, in the ApiCompat baseline. Converted consumers BIND the arity-2 method,
   so it is binary-stable within a Go minor release like the rest of tier 2. The `internal/abi` overload is tier 3
   like the rest of that package.

Revision 1's questions, kept for the record:

1. Overload (3.3) versus refusal for exported generics: this design takes the overload because it moves no
   consumer. The alternative is to keep refusing and disclose the pair legs forever.
2. Scope: abi.TypeFor and unique.Make now, reflect.TypeFor and reflect.TypeAssert as a follow-up seat (3.2)?
3. `PublicSurface` tier for the arity-2 overload: it is public C# surface a consumer can bind to. Proposed tier 2
   (generated support surface, `[EditorBrowsable(Never)]` on the overload), so it carries no compatibility promise
   beyond the forwarder. The `internal/abi` overload is tier 3 like the rest of that package.
