# Reflection

[Reference index](../README.md) · [Summary of this topic](../../ConversionStrategies.md#reflection-reflect)

Go's `reflect` works over the boxed managed value and a type descriptor built from its `System.Type`. Hand-owned `_impl.cs` files in `reflect`, `internal/reflectlite` and `internal/abi` bridge to golib's `GoReflect`, and the converter stamps, as descriptor cargo, the facts a CLR type does not carry.

## Pages

| Page | Covers |
|:--|:--|
| [Types](types.md) | names and strings, method tables, struct fields and tags, type relations and identity, types built at run time |
| [Values](values.md) | reading and writing values, channels, calls, typed nil, pointer order tokens, deep equality, the reflectlite bridge |

## Descriptor cargo

### A func PARAMETER's array LENGTH cannot be recovered — `[GoArrayDims]`

**A func PARAMETER is the one position an array's LENGTH cannot be recovered from — `[GoArrayDims]`
(2026-08-11).** A Go array's length is part of its type, and it is the one part the managed emission
cannot carry: `[32]byte` renders as golib `array<byte>`, and C# has no const generic parameter to
hold the 32. The bridge has always answered that by recovering the dimension from a live source
instead, and the two it had covered every position that mattered — a VALUE measures itself
(`GoReflect.ArrayDimsOfValue`), and a struct FIELD reads it off a cached zero instance of the
declaring struct, because the converter emits the dimension as a field initializer (`= new(32)`)
that the generated parameterless constructor runs.

A func parameter has neither. There is no value at a type-only position, no initializer to read,
and the emitted delegate type is a bare `Func<array<byte>, bool>` that `func([32]byte) bool` and
`func([64]byte) bool` **share**. So `reflect.TypeOf(f).In(0)` answered a dims-less array descriptor:
`Len()` 0 and `String()` `"[]uint8"` — which does not even read as an array — and `reflect.New`/
`reflect.Zero` of it built a **zero-length** array. `testing/quick` is the consumer that shows what
that costs, because its generator allocates the argument from the parameter type alone
(`v := reflect.New(concrete).Elem()`, then `for i := 0; i < v.Len(); i++`): every property test over
a fixed-size array ran against the EMPTY value. `crypto/internal/edwards25519`'s
`TestScalarSetCanonicalBytes` indexed `in[len(in)-1]` and panicked with `index out of range [-1]
with length 0`; its sibling `TestScalarSetUniformBytes` reported `failed on input [0]uint8{}`, which
names the empty array outright.

The datum therefore has to live at the parameter, and it does: the converter stamps
`[GoArrayDims(32)]` there (outermost dimension first — `[2][3]int` ⇒ `[GoArrayDims(2, 3)]`), from
the single `generateParametersSignature` all three signature builders share, so declarations,
methods, func literals, func types and interface methods are all covered by one emission point.

```go
f1 := func(in [32]byte, sc Scalar) bool { … }      // edwards25519's scalar_test.go
```
```csharp
var f1 = ([GoArrayDims(32)] array<byte> @in, Scalar sc) => { … };
```

`GoReflect.FuncParamDims` reads it back off the delegate **INSTANCE** —
`Delegate.Method.GetParameters()`, which resolves to the real declaration for every shape go2cs
emits (a declared func used as a method group, a non-capturing lambda, a capturing lambda's
display-class method, a natural-typed lambda, a local function) — and `abi.TypeOf` stamps it as
descriptor cargo beside `arrayDims`, so `reflect.Type.In(i)` hands out an array type that knows its
length. The cargo joins BOTH interning keys (`abi.descriptorDimsKey`, shared with reflect's
`canonType`) for the reason the array dims are already in them: `func([32]byte) bool` and
`func([64]byte) bool` are distinct Go types over one managed delegate type, so interning them
together would let whichever arrived first answer `In(0).Len()` for both.

Three boundaries are deliberate. A **defined** (named) array type is not stamped — its managed form
is a generated wrapper, not `array<T>`, so dims cargo could not be consumed even if carried — while
an **alias** for an array is, because a Go alias *is* its target type. **Result** dims are not
carried at all: a multi-result Go func returns a `ValueTuple`, which has no per-element attribute
position, and no measured consumer reads `Out(i).Len()`. And a delegate whose target method's
parameter list does not line up one-for-one with `Invoke`'s — an open instance delegate carries the
receiver as an extra leading parameter, and the bridge's own method values are expression-compiled
closures with no attributes — is answered `null` rather than mis-indexed, which is the r39d rule in
its usual form: a dims-less descriptor is a state the bridge already handles, a mis-indexed one is
not.

**The same datum two hops further out — a METHOD's parameters, and a `*[N]T` (2026-08-14).**
`net/rpc` is the consumer that found both hops missing, and it found them the hard way. Its server
allocates every reply argument from the method type alone —
`replyv = reflect.New(mtype.ReplyType.Elem())`, where `ReplyType` came from `mtype.In(2)` — so a
service method declared `func (BuiltinTypes) Array(i int, reply *[1]int) error` needs the `1` to
survive two hops the func-value route does not have:

1. **The func type comes from the method TABLE, not from a delegate instance.**
   `reflect.Type.Method(i).Type` is synthesized in `value_impl.cs` from `GoMethodFuncType` over the
   `MethodInfo`'s parameters, and nothing in that path ever holds a delegate for
   `GoReflect.FuncParamDims` to read. `GoReflect.MethodParamDims(t, i)` reads the same
   `[GoArrayDims]` stamps straight off those `ParameterInfo`s, and `Method(i)` carries them as the
   descriptor's `funcParamDims`. It owes no arity guard, unlike the delegate route: the delegate
   type is synthesized FROM that parameter list, receiver included, so the indices line up with
   `In(i)` by construction — which is Go's own shape for a method type, receiver first.
2. **The array sits behind a POINTER.** A callee that writes its result through a parameter takes a
   `*T`, so the type-only position whose length must survive is the *pointee's*. The converter now
   stamps a parameter's pointee dims (one hop; that is all a Go signature spells here), and a
   pointer descriptor's dims pass through `Elem()` **unshifted** — there is nothing else they could
   describe, a pointer having no length of its own — while an array's dims still shift, its element
   consuming the outer one. The stamp had to be added to `visitFuncDecl`'s REBUILT signature path as
   well, and that is where it actually fires: having a pointer parameter is itself what triggers the
   rebuild, so a `*[N]T` parameter never reaches `generateParametersSignature` at all.

Without either hop, `reflect.New(In(2).Elem())` built a zero-length array and the callee's first
write panicked `index out of range [0] with length 0` — on an rpc goroutine, which took the entire
converted-test host down with it (`net/rpc/jsonrpc`'s `TestBuiltinTypes`; the same run's other eight
tests then recorded no verdict at all, which is what made the panic read as three failures instead
of one).

Guard: `tests/Behavioral/ReflectFuncArrayParamDims`, byte-identical to `go run` — `In(0)`'s
`String`/`Kind`/`Len` and the `New`/`Zero` lengths across a literal, a multi-parameter literal, a
nested `[2][3]int`, a declared func used as a value and a func with no array parameter at all; the
distinctness of `[32]byte` and `[64]byte` as `In(0)` types; the inner dimension surviving `Elem()`;
the struct-field route still answering; and quick's generation loop in miniature (allocate from the
parameter type, fill through `Index(i).Set`, `Call`), so a zero-length synthesis shows up as the
callee's wrong answer rather than a silent pass. It carries rpc's shape too, since 2026-08-14:
`Method(i).Type.In(2).Elem().Len()` for a `*[3]int` reply, `reflect.New` of it, and the callee
writing through the pointer — a length the descriptor does not know is not merely mis-reported
there, it PANICS. Converter unit guard: `TestGoArrayDimsAttribute` (both shapes, including the
boundaries: a pointer to a DEFINED array type, to a slice, and a double pointer are all unstamped).
Corpus footprint of the 2026-08-14 half, measured by re-transpiling all 592 behavioral packages:
**5 declarations in 5 files, one line each** — four `*[N]T` parameters, plus one `[4]byte` VALUE
parameter (`DeferTypelessReturns`' `first`) that had been silently unstamped all along, its function
taking the rebuilt path because it heap-boxes. Nothing else moved.

### The CHAN direction is carried by the VALUE — descriptor cargo, exactly like an array's length (2026-08-20)

`ChanDir()` is the fourth member of the downcast family, and the one whose datum is not merely
unpopulated but **not in the managed type at all**. A Go channel type emits as golib's `channel<T>`
whatever its direction, distinguished only by the `/*<-*/` marker comment the type renderer places
for the reader:

```go
var recv <-chan int
var send chan<- int
var both chan int
```
```csharp
/*<-*/channel<nint> recv;   // all three land on ONE managed type
channel/*<-*/<nint> send;
channel<nint> both;
```

That is the same shape a fixed-size array's LENGTH has, and it is now solved the same way: the
direction rides on the **value** and reaches `reflect` as non-identity descriptor cargo. The datum
sits on the `channel<T>` struct rather than on its heap core — direction belongs to the Go TYPE, not
to the channel object, so two values of different directions may share one core, and the NIL channel
of a directional type has no core at all yet still has a direction (`channel<T>.SendOnly` /
`.RecvOnly`, the direction-carrying nil).

Three converter emission sites stamp it, and they are the three places a directional channel value is
BORN — the same finite set the array dims occupy, position for position:

| Position | Array length | Channel direction |
|:--|:--|:--|
| the made/constructed value | `new(32)` | `new channel<nint>(0, GoChanDir.Send)` |
| a struct FIELD's zero | `= new(4)` field initializer | `= channel<@string>.SendOnly` field initializer |
| behind a POINTER | `GoReflect.PointeeArrayDims` | `GoReflect.PointeeChanDir` |
| a func PARAMETER | `[GoArrayDims(32)]` | *not carried — see the boundaries* |

```go
ch := make(chan<- int)                       // text/template's TestIssue43065
p  := new(chan<- string)                     // reflectlite's TestSetValue row
type holder struct{ x chan<- string }        // reflectlite's TestTypes row
```
```csharp
var ch = new channel/*<-*/<nint>(0, GoChanDir.Send);
var p = Ꮡ(channel/*<-*/<@string>.SendOnly);
[GoType] partial struct holder {
    internal channel/*<-*/<@string> x = channel/*<-*/<@string>.SendOnly;
}
```

`GoReflect.ChanDirOfValue` / `PointeeChanDir` / `FieldChanDir` read it back — the field route off a
cached zero instance of the declaring struct, exactly as `FieldArrayDims` does — and `abi.TypeOf`
stamps it on the descriptor. The cargo joins BOTH interning keys (`abi.descriptorDimsKey`, shared
with reflect's `canonType`) for the reason the array dims are in them: `chan<- int` and `chan int`
are distinct Go types over one managed `channel<int>`, so interning them together would let whichever
arrived first answer `ChanDir()` and `String()` for both. `Type.String()` renders the arrow from the
same cargo (`GoTypeName(t, dims, chanDir)`), and a POINTER hands its pointee's direction down
**unshifted** through `Elem()` — a pointer has no direction of its own — which is the hop
`new(chan<- string)` takes.

**Unstamped is not a failure state.** A channel nothing stamped answers `BothDir`, which is what this
accessor reported for every channel before the cargo existed and remains the honest answer for a type
nothing narrowed. That is what keeps the change additive: only directional sites move.

Four boundaries are deliberate, and each is the same shape as one the array dims already draw:

- **A NARROWING conversion is not carried.** `var s chan<- int = ch` makes a value of a new Go type,
  but the narrowing is a plain struct copy with no construction to hook; stamping it would mean an
  explicit call at every assignment, argument and return of a directional channel in the corpus (89
  such positions, measured) for a datum no consumer reads. `reflect.TypeOf(s)` therefore still
  reports `chan int` there — the r39d rule, and the same boundary the func-param dims draw at
  results.
- **A DEFINED channel type is not stamped** (`type closeWaiter chan struct{}`), for the reason a
  defined ARRAY type carries no dims: its managed form is a go2cs-gen wrapper struct rather than
  `channel<T>`, so there is no field to carry the cargo. An ALIAS for a channel type IS its target
  and is stamped.
- **A func PARAMETER is not stamped.** The `[GoArrayDims]` position exists for arrays because
  `testing/quick` and `net/rpc` allocate from a parameter type; nothing measured reads
  `reflect.TypeOf(f).In(i).ChanDir()`.
- **A type PARAMETER instantiated at a channel type** routes through `ISupportMake`, which has no
  direction-taking form.
- **`reflect.ChanOf(dir, elem)` is untouched and still answers `BothDir`.** It is the auto
  conversion, and it fabricates a descriptor with no value behind it by reinterpreting a boxed
  `channel<unsafe.Pointer>` as Go's linker-allocated `chanType` record — a path the managed bridge
  cannot honor at all, of which the direction is the smallest part. Its behavior is unchanged by the
  cargo (it answered `BothDir` before and after), and no measured consumer reaches it; a lane that
  needs it owes the whole descriptor, not just the arrow.

The downcast this replaced read a direction out of the memory following the descriptor's value slot,
**non-deterministically**, so `reflect.MakeChan`'s `ChanDir() != BothDir` guard and the identity
walk's chan arm each answered differently run to run.

**Landing it retired the `chan-direction` disclosure class.** `internal/reflectlite`'s suite read the
direction three ways — `TestAssignableTo`'s `<-chan int → chan int` row (want false), and
`TestTypes`/`TestSetValue` stringifying a `chan<- string` slot (want `chan<- string`) — and all three
are birth positions, which is why a value-carried direction reaches them: two go through
`new(<directional chan>)` and one through a struct field's zero. All three pass, the manifest is
gone, and the class's own self-retirement text is spent.

**Two latent defects the cargo exposed, both fixed here.** `internal/reflectlite`'s hand-owned
`haveIdenticalUnderlyingType` chan arm had dropped Go's FIRST rule — *"x is a bidirectional channel
value, T is a channel type, and V and T have identical element types"* — keeping only the
strict-equality rule. It was invisible while every `ChanDir()` answered `BothDir`, because both rules
then agreed for every pair; with real directions it makes `var r <-chan int = make(chan int)` report
unassignable. And `reflect.Value.Len()` had no `IChannel` arm at all, so every channel Value reported
length 0 while `Cap()` answered correctly one method away — silent for the same reason the
named-string arm was, 0 being a real length.

### A struct FIELD's TYPE-ONLY array dims — `[GoArrayDims]` / `[GoMapKeyDims]` (2026-08-20)

The array-length cargo had a hole, and it was in the position that decodes: a field's dims came from
the declaring type's **zero instance**, which reaches an array the field IS and nothing an array is
BEHIND.

```go
type T1 struct {                              // encoding/gob's TestEndToEnd
    Marr map[[2]string][2]*float64
    N    *[3]float64
}
type Indirect struct{ A ***[3]int }           // encoding/gob's TestIndirectSliceMapArray
```

`FieldArrayDims` reads `= new(N)` back off `Activator.CreateInstance(declaringType)`. On `Marr` that
instance holds a **nil map**, and a populated one would help no more — a map entry is a value, while
`Key()`/`Elem()` answer for the TYPE. On `N` and `A` it holds a **nil pointer**, with no pointee to
measure. Both hops are ordinary at a **decode target**, which is exactly a struct nothing has
populated yet, so the datum has to be in the emitted C#. That is the same conclusion the func
PARAMETER position reached, and it takes the same carrier — an attribute:

```csharp
[GoType] partial struct T1 {
    [GoArrayDims(2), GoMapKeyDims(2)]
    public map<array<@string>, array<ж<float64>>> Marr;
    [GoArrayDims(3)]
    public ж<array<float64>> N;
}
[GoType] partial struct Indirect {
    [GoArrayDims(3)]
    public ж<ж<ж<array<nint>>>> A;            // ONE stamp, any pointer depth
}
```

The two attributes are named for **the accessor each feeds**, which is also what the descriptor's own
slots have always meant:

| Cargo slot | Attribute | What it is | Handed down by |
|:--|:--|:--|:--|
| `abi.Type.arrayDims` | `[GoArrayDims]` | an ARRAY's own dims (head consumed), a POINTER's pointee's, a MAP's element's | `Elem()` — tail for an array, **unshifted** for a pointer and a map |
| `abi.Type.keyDims` | `[GoMapKeyDims]` | a MAP's key's dims | `Key()` |

So nothing about `arrayDims` changed meaning; a MAP simply joined the POINTER in the unshifted arm it
already had, and `Key()` — a map type's second accessor, which had no slot at all — got one.
`keyDims` joins **both** interning keys (`abi.descriptorDimsKey`, shared with reflect's `canonType`)
for the third time and the third instance of one reason: `map[[2]string]V` and `map[[3]string]V` are
distinct Go types over one managed `map<array<@string>, V>`, so interning them together would let
whichever arrived first answer `Key().Len()` for both. `Type.String()` renders from the same pair
(`GoTypeName(t, dims, chanDir, keyDims)`), which is what turns `map[[]string][]*float64` back into
`map[[2]string][2]*float64` and `***[]int` back into `***[3]int`.

**Unstamped is not a failure state**, exactly as with the chan direction: a field nothing stamped
carries null and every accessor answers the dimension-less array it answered before, so only the
shapes above move.

**The pointer hop had to land on the VALUE side too, and that was found by measurement.** With the
stamps in, `encoding/gob`'s `TestEndToEnd` passed and `TestIndirectSliceMapArray`'s root moved one
frame — out of the type-compatibility rejection and into
`panic: reflect: reflect.Set using unaddressable value`, inside `growSlice`. The cause is that
`reflect.Value.Elem()` recovered a pointee's dims from the LIVE value alone
(`ArrayDimsOfValue(ReadPointerSlot(box))`), while `rtype.Elem` hands the descriptor's cargo down
unshifted. gob's `decIndirect` walks a `***[3]int` target by allocating each level from
`value.Type().Elem()`, so a hop reading the live value reads the nil pointer it is standing on,
allocates a **zero-length** array from the dimension-less descriptor, and the next hop measures that
zero as the truth. `Value.Elem()` now prefers the carried dims and falls back to the live
measurement — and it descends them through EVERY pointer hop, not only the one whose pointee is the
array, because `***[3]int`'s intermediate pointees are pointers. The live route still answers where
the descriptor is silent (`ValueOf(&[100]T{}).Elem().Type()` carries 100). This is the same lesson
the chan-direction cargo's fourth position taught, one layer over: **a value the bridge hands out
must describe itself the way the descriptor does.**

Four boundaries are deliberate:

- **A field that IS an array is NOT stamped.** Its `= new(N)` initializer already carries the length,
  through a route that also survives a value copy; stamping it would duplicate the datum and churn
  every array-bearing struct in the corpus for nothing.
- **A DEFINED array or map type is not stamped** (`type Row [3]int` behind a pointer,
  `type Set map[[2]string]bool`) — the same one-sentence boundary the chan direction draws, for the
  same reason: its managed form is a go2cs-gen wrapper rather than `array<T>`/`map<K,V>`. An ALIAS
  for one IS its target and is stamped (`types.Unalias` resolves it).
- **A SECOND nesting level is not carried** — `[][2]int`, `map[K]map[[2]string]V`, a func field's
  parameters. The cargo has exactly one `Elem()` slot and one `Key()` slot, so a second level has
  nowhere to live and no measured consumer asks (the r39d rule). `reflect.Type.String()` still
  renders `[][2]int` as `[][]int`, unchanged.
- **A func PARAMETER of map type is not stamped.** `[GoArrayDims]` reaches parameters already, but
  nothing measured reads `reflect.TypeOf(f).In(i).Key().Len()`.

(Guarded by the `FieldDimsCargo` behavioral test, byte-identical to `go run`: gob's own field shapes
read back through `Field(i).Type`, both map accessors, `reflect.New(mtyp.Key())` and
`reflect.New(mtyp.Elem())` — the exact pair `decodeMap` performs before it fills an entry — a
three-hop pointer chain on the type side, gob's `decIndirect` walk verbatim on the value side, each
map accessor exercised alone, the untouched initializer route, and the defined-type and
slice-element boundaries. Converter unit guard: `TestFieldDimsCargo`, 20 rows including every
boundary above. Proven failing-first by neutering each of the three halves separately: with the
converter stamp neutered every carried length reads 0 and every type string loses its dims — which is
`length mismatch in decodeArray` one frame down — while the initializer route is untouched; with the
bridge's `Key()` and map-`Elem()` arms neutered the field type strings still render correctly, off
the descriptor's own cargo, and only the accessor answers collapse; with `Value.Elem()`'s descriptor
precedence neutered ONLY the `decIndirect` line moves, landing on `[0]int 0` — the zero-length array
`growSlice` panics on.)

---

[Index](../README.md)
