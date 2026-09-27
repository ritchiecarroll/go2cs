<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->
# Reflection: Values

[Reference index](../README.md) · [Reflection](README.md) · [Summary of this topic](../../ConversionStrategies.md#reflection-reflect)

This page covers the value side of the [reflection bridge](README.md): how a `reflect.Value` reads and writes the boxed value, calls functions, keeps a typed nil, orders pointers and compares values deeply.

## Reading and writing values

### The reflection bridge answers a read where the answer EXISTS — four members that did not (2026-08-19)

Four unrelated reads through `reflect` degraded rather than answered, and each degraded to a value that reads as a real answer, which is what kept them invisible. Recorded together because the discipline is one: a bridged member either honors the read or refuses it by name — never a plausible-looking substitute. (This is the r39d rule applied in the direction it is usually NOT applied: a descriptor field left unpopulated because it *seemed* unanswerable, where the answer was in fact already computed one layer down.)

| Member | Was | Is |
|:--|:--|:--|
| `Value.Index` on a **string** | `panic: reflect: call of reflect.Value.Index on string Value` — Go's message for a kind that does not support indexing AT ALL | the i'th BYTE as a non-addressable `uint8` Value, Go's own arm |
| `Value.Slice` on a **string** | the same ValueError | a string of the receiver's OWN type, so a NAMED string stays named (Go returns `Value{v.typ(), …}`) |
| `Value.Slice3` | `invalid memory address or nil pointer dereference` — the auto conversion reinterprets the never-populated `ptr` slot as a raw `unsafeheader.Slice` and edits it in place | hand-owned over the same golib window machinery `Slice` uses, so the two- and three-index forms cannot disagree about what a window is |
| `StructField.Offset` | `0` — a REAL answer for a field at the front of a struct, so an unpopulated descriptor read as a LAYOUT failure | the Go (amd64) offset, from the same memoized layout walk `internal/abi`'s `StructType()` already publishes |

`Offset` is the one worth stating at length, because it had a stated reason for staying empty: *a Go byte offset exists only to be added to a data pointer, and managed storage has no such pointer*. That is true of `Offset` as an ADDRESS and false of `Offset` as layout METADATA, which is the only way anything has ever read it here — `unique`'s clone sequencer and `internal/reflectlite` through `abi`, and now `sync/atomic`'s `TestAutoAligned64`, which asserts `TypeOf(&struct{_ uint32; i Int64}{}).Elem().Field(1).Offset == 8`. Reading it off `GoReflect.GoFieldOffsets` is what makes the two Go-specific layout rules come out right where a naive `Marshal.OffsetOf` would not: a Go **zero-size** field occupies nothing (its C# surrogate is one byte), and an `align64`-bearing field is padded to its 8-byte boundary. The r39d rule still bites where it should — a struct holding a field whose Go size is unknowable makes every later offset a guess, `GoFieldOffsets` answers `null` for the whole struct there, and every field keeps the zero rather than a plausible number.

`text/template`'s `index` and `slice` builtins are the measured consumers of the first three (`{{index `x` 0}}`, `{{slice .S 1 2}}`, `{{slice .SICap 6 10 10}}`), and closing them took the package from 49 to 50 of 52. Guarded by `ReflectStringWindow` (every arm plus Go's own out-of-range, wrong-kind and reversed-bound panic texts, and a byte-slice control that the string arms did not disturb the container path) and by `ReflectStructTagCopy` (offsets across a zero-size/aligned/tail layout, and a promoted field whose offset is relative to its own declaring struct).

### A ZERO test is a descriptor read too — `Value.IsZero`, `Value.Grow`

**A ZERO test is a descriptor read too, and this one had degraded to a CONSTANT — `Value.IsZero`,
`Value.Grow`, and the named-string `Len` (2026-08-03).** Go's `IsZero` is three reads over flat
memory: an `Equal` function pointer compared against the shared `zeroVal` buffer, a
`TFlagRegularMemory` all-bits-zero scan, and — when the value is not `flagIndir` — plain
`v.ptr == nil`. A synthesized descriptor populates none of them, and the bridge never populates
`v.ptr` or `flagIndir` at all, so the Array and Struct arms both fell straight to that last test and
answered **true for every array and every struct, whatever it held**. Measured against `go run`
before the fix: `[2]uint8{1,2}`, `NA{1,2}`, `inner{N:1}`, `outer{P:&n}` — every one of them
`IsZero=true` in C# and `false` in Go. This is the `""`-type-name and `NumMethod`-0 family again,
and the worst-behaved member of it so far: `true` is the correct answer for the zero value of the
same type, so nothing faults and nothing looks wrong.

A **fourth** read failed independently and had to land with it. `IsZero`'s String arm is
`v.Len() == 0`, and `Len` answered through the golib container interfaces — a named slice is an
`IArray`, a named map an `IMap` — but a `type NS string` wrapper implements none of them, so it fell
to the `0` default. Every non-empty named string therefore reported itself both length-0 and zero.
`String()` had always unwrapped such a wrapper; `Len` now does the same, gated on Kind String.
That pairing is the increment-6 rule in its second form: **`IsZero`'s String arm is a GATE on `Len`,
so the gate and the read behind it are one increment** — fixing the arms without `Len` would have
left named strings silently zero, and fixing `Len` without the arms would have changed nothing.

The managed `IsZero` is Go's own recursive definition with the memory shortcuts *removed*: a
composite is zero exactly when every element (Array) or field (Struct) is, scalars test against
their zero, and the nilable kinds are `IsNil`. That is precisely the walk the shortcuts stand in for
— Go falls back to it itself when a type is not comparable and not regular-memory — so it needs no
descriptor state beyond `Index`/`Field`/`NumField`, which the bridge already answers. Go's blank-field
skip (`Name != "_"`) is preserved.

`Value.Grow` shares the root and the remedy shape: it reads a `*unsafeheader.Slice` off the same
never-populated `v.ptr`, so it **nil-deref'd for every caller** — `reflect.ValueOf(&s).Elem().Grow(1)`
on a `[]byte` prints `4 8` in Go and panicked here. It is now an ordinary managed reallocation
(golib `GoReflect.GrowSlice`) written back through the aliased box exactly as `SetLen` does, coerced
into a named slice wrapper's slot through the single convertibility relation. Two details are
load-bearing: growth **within** the existing capacity writes nothing at all, because Go reaches
`growslice` only past the capacity and a spurious write would detach any other view still sharing the
backing store; and the capacity landed on is unspecified in Go (its `growslice` rounds to a size
class), so only `len+n` is guaranteed and the guard test asserts `cap >= n`, never an exact figure.

Guard: `tests/Behavioral/ReflectZeroAndGrow` pins all four against `go run` — raw and named strings,
raw and named arrays (zero and non-zero, including an array OF named strings), the nil-vs-empty
distinction for slices and maps, structs made non-zero through a nested named-string field alone,
zeroness reached through an interface, and Grow from a nil slice / within capacity / past it /
with `Grow(0)`. Demonstrated consumer: `encoding/gob`, whose `gobEncodeOpFor` skips a field on
`!state.sendZero && v.IsZero()` — so the encoder was omitting non-zero named-string and array fields
from the wire entirely, visible as `v = "", want "forty-two"` on the value fields while the pointer
fields of the same type passed.

### `reflect.Value.Bytes`/`SetBytes` are defined over the element KIND, and they ALIAS

Go's `Bytes()` accepts any slice whose element kind is `Uint8` — `[]byte`, `[]renamedByte` where
`type renamedByte byte`, and a defined slice type over either — plus an addressable byte ARRAY, and it
reaches the storage by re-typing the slice HEADER. `SetBytes` assigns one back the same way. Neither
ever copies, and that is not an implementation detail: `Bytes()` is how a caller *writes into* a
reflected byte slice.

Two of the three slice shapes were already aliased (a raw `slice<byte>` is itself; a defined slice
type over plain byte answers through its `ISlice<byte>` view, which shares its backing). The third — a
DEFINED byte ELEMENT — had no route at all: `slice<renamedByte>` holds a `renamedByte[]` of one-field
wrapper structs, an unrelated instantiation with no conversion to `slice<byte>`, so the catch-all cast
threw `InvalidCastException` out of a core reflect API (`encoding/json`'s `TestSliceOfCustomByte` and
`TestEncodeRenamedByteSlice`; `fmt`'s `%x` of a `[]renamedUint8` and five siblings of one table-driven
test).

The route is `slice<T>.AliasOfElement`, a bridge primitive that re-spells a slice's element type while
carrying its window across unchanged, and its whole safety argument is the gate in front of it: the
two element types must be ONE representation under two Go names — **both value types, both free of
managed references, both exactly one byte wide** — asked of the managed types directly and never
inferred from the `[GoType]` token. Those are the same three facts the blessed
`ReinterpretAliasesStorage` gate asks of a pointee pair. Under them the two backing objects are
byte-for-byte the same shape and differ only in their method table, and every access golib makes
through a `slice<T>` addresses the data from the STATIC element type and the array's own length field
(`Span<T>`'s array constructor skips its covariance check outright for a value-typed `T`). What the
pun does not survive is a runtime type test on the array OBJECT — `Array.Copy`'s element check,
`backing is byte[]`, `backing.GetType()` — so nothing may reach one through the result, which is why
it is an internal primitive rather than a public conversion.

**`SetBytes` was worse than incomplete, and for a reason with nothing to do with named types.** Its
auto body is `*(*[]byte)(v.ptr) = x`, which converts to a store through
`(ж<slice<byte>>)(uintptr)(v.ptr)` — `v.ptr` is the Go data word this bridge never populates, so the
store went through a box over address 0 and landed nowhere, for EVERY byte slice including a plain
`[]byte`. Silently: `encoding/json`'s `literalStore` decodes base64 into a fresh buffer and hands it
over with exactly this call, so every `[]byte` field decoded as empty and `TestLargeByteSlice`
reported a 2000-byte round trip diverging at byte 0. It is hand-owned now and writes where every other
setter writes, through the addressable Value's aliased box.

### `reflect.Copy` is bridged element-wise — the auto form is a flat two-header memory move

`reflect.Copy` reinterprets BOTH operands' data words as `unsafeheader.Slice` headers
(`*(*unsafeheader.Slice)(dst.ptr)`) and hands them to `typedslicecopy`. That is a raw memory
move with no managed form, and on the bridge's never-populated `ptr` slot it dereferences a nil
`ж` outright. `encoding/asn1`'s `parseField` copies every parsed `[]byte` into its destination
through it, so this was `crypto/x509`'s `ParsePKCS8PrivateKey` and therefore the second half of
`crypto/ecdsa`'s `TestEqual` — reached only once the tag fix above let the marshal succeed.

The bridge copies element-wise through the same golib container interfaces every other bridged
container method uses, which keeps the ALIASING exact rather than approximating it: a slice
VALUE windows the backing store it shares with its parent, so an indexer write is a write the
parent sees — precisely what `typedslicecopy` does to the same memory. Kind and element-type
validation mirror Go's, including the documented special case where `src` may be a `String`
when `dst`'s element type is `byte`; a nil container on either side copies nothing, matching
Go's zero-length header. (Guarded by the `ReflectStructTagCopy` behavioral test — slice←slice
truncating at either side, slice←string, an addressable array through `Elem()`, a nil
destination, and a window slice whose copy must be visible in the ORIGINAL backing array.)

### A map ENTRY is a SLOT — its Value is typed by the map's ELEMENT type, never by what it holds

Every Value the bridge hands out of a container is *slot-derived*: a struct field, a slice element, an
array element, a func result and a `MapIndex` lookup all build their Value from the **declared** type
of the place the value sits (`makeTypedValue`), not from the object found there (`makeReflectValue`,
which is Go's rule only for `ValueOf` and interface `Elem`, where the type genuinely does come from the
value). The distinction is invisible until the slot holds nothing: a null read through the declared
type is a VALID nil Value of that type, while the same null read dynamically is the **invalid zero
Value** — a different thing entirely, and the one Go reserves for "this slot does not exist".

`deepValueEqual`'s map arm was the last read that skipped it. It walks the backing `Dictionary`
directly — it must, because golib keeps a nil KEY in a side slot no iterator can see — and it built
each entry's Value from the stored object. That is harmless while both sides spell nil the same way,
and the two sides do not:

```go
want := map[string]*Small{"19": {Tag: "tag19"}, "20": nil}  // literal: the entry physically holds C# null
json.Unmarshal(data, &got)                                  // decoded: the reflective write stores the canonical nil box
reflect.DeepEqual(got, want)                                // false — invalid Value vs valid nil pointer
```

Two spellings of one nil is not itself a defect: `packInterfaceValue` re-encodes a null pointer slot as
`ж<T>.NilBox` precisely so a typed nil survives being handed out as an interface, and the write path has
always stored that box. A type-blind READ is what makes them observably different — and in the same
stroke it makes a nil element compare EQUAL to a missing key, since both answer the invalid Value.
Typing the entry by `Elem()` of the map's own type collapses both: two nil elements meet at the kind's
nil rule (pointer — neither box is a real address; interface — `IsNil() == IsNil()`), a missing key
still fails on `Contains`, and a nil element still separates from a present non-nil one.

It is the whole of `encoding/json`'s last divergence. `TestUnmarshal` rows **#56–#63** decode into the
40-field `All` fixture and compare with one top-level `DeepEqual`; `Marshal` of both sides produced
**byte-identical** JSON, so nothing in the failure text pointed at a field. A field-by-field walk did:
`All.MapP` — `map[string]*Small{"19": …, "20": nil}` — reported false **at the map** while every element
compared equal beneath it, because that walk re-boxed each element through `Interface()` and so
re-entered the dynamic path on both sides. A container's verdict disagreeing with its own contents' is
the signature of a slot read that lost its type, and it is worth recognizing on sight: the same
discrepancy named the FUNC arm of this very function earlier (see *Manually-Converted Declarations*).

Guarded by `ReflectBridgeClosure`'s *map nil element* rows — a map built through `SetMapIndex` compared
against the same map written as a literal, both directions, plus the separations that must survive (a
nil element vs a different key, vs a present non-nil element) and the interface-valued flavour, where a
nil entry is the nil interface rather than a typed nil.

### The VALUE side of the same rule: pointer KIND is not pointer BOX

Classifying the handle `Pointer` settled what the *layout* walks do with it, and left a second
question the *value* walks have to ask before dereferencing one: **is there a slot behind it at
all?** For an opaque handle there is not — what it refers to has no Go representation — so "one word
wide, do not descend into it" is also **"no slot, do not read through it."** The two are one rule
asked at two layers, and only the first half had been stated.

`reflect.Value.Elem` asked the wrong question. It resolved a pointee with `GoReflect.ElementType`
and, on `null`, fell through to a "detached read" through `GoReflect.ReadPointerSlot` — which
classifies the box shape itself and threw `Not a pointer box type: go.sync_package+RWState`. That is
every `reflect.DeepEqual` over a struct holding a `sync` primitive: `DeepEqual` reaches `Pointer`
kind, calls `Elem` on both sides, and dies (`crypto/tls`'s `TestCloneNonFuncFields` is the measured
case, and `sync.Mutex`'s `SemaphoreSlim` gate, `sync.RWMutex`'s `RWState` and `sync.WaitGroup`'s
`WaitGroupState` are all the same shape).

The classification now lives in **one** place — `GoReflect.TryPointerBoxElement`, which
`slotAccessorShape` is refactored onto, so "can I read through this?" and "what will I read?" can
never be answered by two different probes. A caller holding a `Pointer`-kind value asks first;
`reflect.Value.Elem` and its `internal/reflectlite` twin answer the **invalid Value** for a handle,
which is the answer they already give for a nil pointer.

That blindness is deliberate and it is *Go's own answer*, not a concession. Go's `sync.RWMutex` is
state **words**, and a used-then-released lock is back at its zero state, so two of them are deeply
equal — which is exactly what two handles now compare as, whether or not the shim has lazily created
one of them. What the walk still sees is any **real** Go state beside the handle: a `sync.Once` that
has run differs from a fresh one, because `done` is an ordinary field and not part of the handle.
Guarded by `GolibTests.PointerNilPredicateTests` —
`OpaqueManagedHandleIsPointerKindButNotAPointerBox` asserts *both* halves in one test, since either
alone would let the other drift back, and `PointerBoxShapesResolveTheirPointee` pins the positive
side so the fix cannot turn real dereferences into nil — and by the `DeepEqual` behavioral test's
`guarded` struct, which compares a locked-then-released `Mutex`/`RWMutex` pair against `go run`.

## Channels

### `reflect.Value.Recv` / `Send` over golib's channel, and why they could not land alone

Both auto forms open with the same downcast one layer down, and behind a synthesized descriptor the
reinterpreted `.Dir` reads **zero** — so `0 & RecvDir == 0` held for every channel and a plain
bidirectional `chan string` was refused as send-only. Past that test neither could have worked
either: both hand a `uintptr` channel address and an `unsafe.Pointer` element slot to `chanrecv` /
`chansend0`, external stubs the `PartialStubGenerator` fills with `NotImplementedException`. Both are
hand-owned over golib's `channel<T>`, reached through `IChannel`'s type-erased `ChanRecv`/`ChanSend`
(the bridge holds a BOXED channel, never a `channel<T>`), and bridging them removes the last live
caller of both stubs.

**The direction guard must fire BEFORE the receive, and that ordering is the whole reason the two
halves are one change.** A working `recv` behind a direction that always reads bidirectional converts
`text/template`'s `range` over a send-only channel from a fast, attributable error into an unbounded
hang — measured by the near-miss-finish lane at **51 verdicts lost** to a package deadline against
the 1 the bridge buys, which is why that lane wrote the bridge, measured it, and reverted it.

`send` assigns its argument through `marshalIntoSlot`, the rule `Value.Call` already used for a call
argument — Go's `assignTo` cannot serve here, because its managed form returns a Value carrying only
the never-populated raw `ptr` slot and drops the boxed companion the bridge actually reads, so the
channel received a bare null. Keeping both boundaries on one renderer is what makes a channel send
and a call argument box a typed nil the same way.

(Guarded by the `ReflectChanDirection` behavioral test, byte-identical to `go run`: all three birth
positions read back through `TypeOf`/`Elem`/`Field`, Go's four assignability answers, the recv bridge
over `text/template`'s own `count(5)` helper, blocking and non-blocking `Send`/`Recv`/`TrySend`/
`TryRecv`, a closed channel's drain-then-zero comma-ok, both direction panics by their Go messages,
a defined-channel-type control, and — timeout-bounded so a regression prints a named line instead of
wedging the suite — the `walkRange` shape itself. Proven failing-first by neutering each half
separately: with the cargo neutered every `dir=` reads `chan`, all four assignability answers flip to
`true`, and the send-only range prints `HUNG -- the direction guard did not fire before Recv`; with
the recv bridge's guard neutered to the auto form's zero, `range count(5)` panics
`reflect: recv on send-only channel`.)

(Guarded by the `ReflectConvertAssignable` behavioral test, extended from the `ConvertibleTo`
recursions to Go's full assignability rule: both gates of the unnamed↔named clause including the
two-defined-types negative, the interface clause in both directions, the struct arm against a
differing field type / a renamed field / a differing field count / a tag that conversion ignores and
assignment honors, the func arm's parameter and result discrimination, and the chan rows the bridge
can truthfully produce. 34 rows, compared line for line against `go run`, measured failing-first —
the struct and func arms reported `true` where Go reports `false`.
`GolibTests.GoStructLayoutTests.EmbeddedField_IsDistinguishableFromADeclaredFieldOfTheSameNameAndType`
pins the projection flag the struct arm stands on.)

## Calls and functions

### `reflect.Value.Call` over a variadic func value is TYPED dispatch — no reflective invoke can carry the tail

The `params Span<T>` tail above is what makes a converted variadic callable and readable from Go
AND from C#. It also puts the value permanently out of reach of every reflective invoke path:
`Span<T>` is a **ref struct**, and `Delegate.DynamicInvoke` and `MethodInfo.Invoke` both marshal
their arguments through an `object?[]` a ref struct cannot enter. `System.Linq.Expressions`
refuses one outright as well, so the method-value binder's `Expression.Lambda` approach
(GoReflect.MethodSets.cs) does not generalize either. `reflect.Value.Call` therefore threw
`NotImplementedException` for every variadic func value — which is 13 of `text/template`'s 52
verdicts, since its whole `FuncMap` feature calls user functions exactly that way.

The call is made in **typed code** instead (`GoReflect.InvokeVariadic`, GoReflect.TypeLayout.cs).
One small generic trampoline per family arity — eighteen, the closed set golib's variadic.cs
declares — is closed over the delegate's own parameter types by `MakeGenericMethod` and cached as
an ordinary delegate, the `elementBoxViaAt` idiom GoReflect.FieldAccess.cs already uses:

```csharp
private static object? callVariadicFunc1<T1, TArg, TResult>(Delegate d, object?[] a, Array t)
{ return ((Funcꓸꓸꓸ<T1, TArg, TResult>)d)((T1)a[0]!, new Span<TArg>((TArg[])t)); }
```

Inside the trampoline the tail is a `TArg[]` and its conversion to `Span<TArg>` is ordinary, so
nothing is boxed and the tail ALIASES the array rather than copying it. Two consequences worth
stating: a panic inside the callee propagates natively (a direct call wraps nothing in a
`TargetInvocationException`, unlike the fixed-arity `DynamicInvoke` path beside it), and a fixed
prefix beyond the family's eight throws a named `NotImplementedException` rather than mis-indexing.

**The delegate being called is not always the family type, and the rebind is what makes that
total.** A variadic func literal in an `any` slot — a `map[string]any` FuncMap value, the exact
`text/template` shape — takes C#'s NATURAL delegate type instead, the same identity difference
`TryFuncShape` had to stop reading off the type NAME. Those rebind onto the family by RETARGETING
through `Invoke` (`Delegate.CreateDelegate(familyType, del, "Invoke")`), never by re-binding the
original's own target and method: a delegate the BRIDGE itself built is expression-compiled — a
variadic method value from `Value.Method` is exactly that — and a compiled lambda's `Method` is not
a runtime `MethodInfo`, which `CreateDelegate` rejects with "MethodInfo must be a runtime MethodInfo
object". Retargeting also carries a multicast invocation list intact. The family's type arguments
are built FROM the delegate's own `Invoke` signature, so the two agree by construction.

Go's `Call` contract shapes the arity rule too: `Call` itself builds the tail slice (`CallSlice` is
the form that takes it pre-built), so the last `In()` is the tail SLICE, every argument from that
position on is assignable to its ELEMENT, and there is no upper bound — only a lower one of
`NumIn()-1`. (Guarded two ways: behavioral `ReflectVariadicCall` output-compares eleven shapes
against `go run` — declared func, empty tail, no fixed params, `...any`, multi-return, no-result,
two fixed params, a variadic METHOD value, and three `map[string]any` literals — while
`GoReflectBridgeClosureTests` pins the three delegate identities, the tail's aliasing, the refusal
of a non-variadic delegate, and every family arity of both families, which are golib-only shapes no
Go program can construct. The arity row matters because only 0, 1 and 2 fixed parameters have a
consumer in the corpus today: 3 through 8 would otherwise be discovered by whichever package
reached them first.)

### `reflect.MakeFunc` is `Value.Call`'s exact inverse — a compiled delegate over the descriptor's carried System.Type (2026-08-29)

Go's `MakeFunc` is runtime machinery end to end: it reinterprets the descriptor into a `funcType`
sub-record, asks `funcLayout` for a stack map, and pairs an assembly stub (`makeFuncStub`) with a
closure context the scheduler calls back through. None of that exists behind a managed-backed
descriptor — `abi.synthType` mints every one as a plain `heap<Type>` box with the CLR
`System.Type` as cargo, so the `Reinterpret<abi.Type, funcType>()` recovers a **zero box** and
`funcLayout` panics `reflect: funcLayout of non-func type <nil>`. First operational hit:
`net/http/httptrace`'s `compose`, which walks `ClientTrace`'s func-typed fields and MakeFuncs a
composed hook for every pair both traces set.

The hand-owned form (`reflect/makefunc_impl.cs`, displaced via the `manualConversionFuncs`
registry) runs the marshalling that `Value.Call` runs, in the opposite direction. Where `Call`
marshals a `slice<Value>` into a delegate's `DynamicInvoke`, `MakeFunc` builds a delegate of
**exactly** the descriptor's carried delegate type whose invocation boxes its CLR arguments,
types each one by the func's STATIC parameter type (`makeTypedValue` — an interface-typed
parameter reports Kind Interface, a nil pointer is a VALID typed-nil Value, and a `[N]byte`
parameter carries the descriptor's `funcParamDims` cargo, the one route a fixed array parameter's
length reaches reflect at all), runs `fn`, and marshals the result Values back out under the SAME
assignability renderer Call's arguments use (`marshalIntoSlot` — one rule for both directions).
A Go multi-return packs into the delegate's own declared `ValueTuple`. The delegate itself comes
from golib's `GoReflect.MakeGoFuncDelegate` — expression-compiled once per delegate type into a
factory (outer lambda takes the `Func<object?[], object?>` invoker, inner IS the typed delegate),
the same memoization rule the method-value binder follows — so the result is callable DIRECTLY as
a typed Go func (`t.DNSStart(info)`), through `Value.Call`, and through composition with itself.

The returned Value rides `typ`'s **own** descriptor box rather than a fresh `synthType`, so the
dims cargo survives and `Type()` interns back to the caller's wrapper: `MakeFunc(t, fn).Type() == t`
by identity. A VARIADIC func type is a loud `NotImplementedException`, not a wrong delegate: its
tail is `params Span<T>`, a byref-like type no expression tree can carry — the route that exists is
the reverse of `InvokeVariadic`'s typed family trampolines above, unbuilt for want of a
demonstrated consumer, exactly as `Value.CallSlice` records. `makeMethodValue`'s identical
`funcLayout` read deliberately stays auto: it is reachable only through `flagMethod`, which the
bridge never sets (`Value.Method` binds the receiver into an ordinary delegate instead). With
MakeFunc live, `reflect/iter.cs`'s rangefunc `Seq`/`Seq2` funcs gain their real implementation
path too. (Guarded by behavioral `ReflectMakeFunc`: the docs swap example invoked directly, the
httptrace compose shape, multi-return, canonical `Type()` identity, `Call` over a made func, an
interface-typed parameter, a typed-nil pointer argument, and a `[4]byte` parameter whose `Len()`
proves the dims cargo threads through. Banked consumer: `net/http/httptrace` 2|0.)

## Typed nil

### `reflect.Value.Interface()` is a boundary into interface space, so it packs the typed nil too

The rule above says the canonical instance is minted where the type becomes *observable* — at the
boundary into interface space — and pointer slots themselves keep plain `null`, because their
statically-typed world never needs the type carried. `reflect.Value.Interface()` is one of those
boundaries, and it is the one a *slot read* arrives at: the Value's data came out of a slice
element, an array element, a struct field, a map value or a `reflect.New(...).Elem()`, all of which
hold `null` for a nil `*T`. Handing that `null` straight out erases the type at the one call whose
entire job is to preserve it.

Go's own form makes the obligation explicit. `packEface` builds an interface from a **type** and a
**data word**, so a pointer-kinded Value with a nil data word packs as a non-nil interface holding
`(type=*T, value=nil)`. Managed storage has no data word to keep the type beside, so the bridge
reconstructs it from the Value's static type — which `makeTypedValue` recorded when the Value was
built — and re-encodes a null pointer-kinded read as that type's canonical typed nil:

```go
in := make([]*Int, 1)              // one zero-filled *Int element
v := reflect.ValueOf(in).Index(0)
i := v.Interface()                 // (*Int)(nil), NOT nil
data, err := i.(GobEncoder).GobEncode()   // assertion SUCCEEDS; nil receiver dispatches
```

The consumer that proves it is `encoding/gob`: `encodeGobEncoder` is literally
`v.Interface().(GobEncoder).GobEncode()`, and `big.Int.GobEncode` opens with `if x == nil` because
Go guarantees it will be reached that way. With the type erased, `i == nil` is true, `%T` prints
`<nil>`, the assertion takes its failure arm, and the nil-receiver method never runs — which is the
whole of math/big's `TestGobEncodingNilIntInSlice` / `TestGobEncodingNilRatInSlice`.

Two boundaries of the rule, both load-bearing:

- **Pointer kinds only.** An interface- or func-typed slot holding `null` *is* the nil interface /
  nil func, and Go packs that as the nil eface. Re-encoding those would invert the bug.
- **No new representation.** The value handed out is `ж<T>.NilBox` — the same singleton
  `reflect.Zero` of a pointer kind already yields (`GoReflect.ZeroValueOf`) and every emitted
  `nil`→`*T` conversion already mints. So the packed value compares equal to a language-level
  `(*T)(nil)` and asserts through the ordinary witness machinery. A slot whose static type resolves
  to no canonical nil keeps its `null`, so the rule can only *add* type information, never
  substitute a wrong one.

The fabrication path (`reflect.Zero`) and the write path were already on this encoding; this is the
**read** path joining them, so there is one nil encoding system-wide rather than two.

Guarded by `ReflectTypedNilInterface`, which runs typed nil → `Interface()` → `== nil` → type assert
→ nil-receiver dispatch across every slot kind that funnels through `makeTypedValue`, each paired
with a non-nil sibling so a blanket substitution fails as loudly as the erasure did, and pins
`Elem()` of a typed nil as still **invalid** (how a walker tells a typed nil from a pointer to a zero
value).

⚠ **The emission side is a separate, open question.** A typed nil crossing into an interface in
ordinary converted code — not through reflection — still collapses, because the pointer slot really
does hold `null` and the conversion site is not always able to see that it needs the box. Closing
that changes what `== nil` means for every converted interface and is a design decision, not a fix;
`encoding/gob`'s own `TestNilPointerInsideInterface` is its standing witness.

### A typed nil keeps its type across BOTH interface-space boundaries

`Value.Interface()` has packed a nil pointer as Go's typed nil since the `packEface` work — a non-nil `any` carrying `(type=*T, value=nil)` — and two other paths had not joined that one nil encoding. Both surfaced as `text/template` rendering a value Go renders through its method:

1. **`Value.Call` into an INTERFACE-typed parameter.** Go's assignment to such a parameter BUILDS an eface, and an eface keeps the type half. `marshalCallArg` read the slot's raw `null` and handed that across, so the callee's `reflect.ValueOf(arg)` answered the INVALID zero Value — `{{html .NIL}}` over a nil `*int` printed `&lt;no value&gt;` where Go prints `&lt;nil&gt;`. It packs through `packInterfaceValue` when — and only when — the destination is interface space; a concrete parameter type builds no eface and is untouched.
2. **A nil RECEIVER dispatched through the runtime duck-typing SHELL tier.** Go's method set belongs to the TYPE, so `(*W)(nil).Error()` dispatches normally and the method decides what nil means (`if w == nil { return "nilW" }` is an idiom, not an edge case). golib's `error<T>` read `m_target_ptr.Value` BEFORE choosing between its `ж`-receiver and by-value overloads, so it threw on exactly that value — for a pointee the `ж` overload it then selected never needed. The throw was invisible: `fmt`'s `handleMethods` wraps every `Error()` call in Go's own `catchPanic`, which prints `<nil>` for a nil-pointer argument, so the symptom was a wrong RENDERING and the defect reproduced ONLY where the pair resolved through the runtime shell rather than through a generated nominal adapter — i.e. only when nothing in the program converts that type to that interface explicitly. Dereferencing on the by-value path is kept, because Go dereferences there too and a nil pointer must still panic.

Guarded by `ReflectTypedNilInterface`, which carries a type reached only through the shell tier and the concrete-parameter control for the `Call` arm.

## Pointer order tokens

### Pointer order tokens — `Value.Pointer()`/`UnsafePointer()`

**Pointer order tokens — `Value.Pointer()`/`UnsafePointer()` (golib `PointerOrderToken`).** Go
programs order pointers *arithmetically* (`cmp.Compare(a.Pointer(), b.Pointer())` —
internal/fmtsort's map-key ordering of `*T`/`chan`/`unsafe.Pointer` keys), so the bridge's token
must be more than stable-per-instance: **equal Go pointers must token equally, and same-storage
element pointers must order by element index** (Go's `&a[0] < &a[1] < &a[2]`). `INilPointer` gains
the `PointerOrderToken` surface (a DIM default of per-instance identity): `ж<T>` answers nil → 0,
a native alias → its real address, an array/slice-element reference → the canonical backing
storage's identity in the high bits with the ABSOLUTE element index below (the same
`CanonicalElement` reduction pointer equality uses), a struct-field reference → source identity ×
field-identity token, a heap box → the referent's identity; `unsafe.Pointer` (whose VALUE is a
real pinned address) overrides with the address itself; `channel<T>` answers through its shared
core's identity, so every struct copy/boxing of one channel reports ONE token — what makes
sorting channels by `Pointer()` self-consistent (fmtsort's `makeChans` pre-sorts by the same
key). Tokens are order keys consistent with pointer equality, never an identity substitute
(distinct storages can collide); generated named pointer/channel wrappers keep the DIM default —
a recorded fidelity residual with no consumer. The banked fmtsort suite (TestCompare/TestOrder)
is the operational guard.

### A TYPE DESCRIPTOR pointer orders by the type's NAME

**…except a TYPE DESCRIPTOR pointer, which orders by the type's NAME (2026-08-10).** The same
`Value.Pointer()` carries one ordering that is visible in ordinary program output rather than only
in a map of pointers: fmtsort's `reflect.Interface` arm orders interface-kinded map keys by dynamic
type, and it does that by comparing the two descriptors as pointers —
`compare(reflect.ValueOf(a.Elem().Type()), reflect.ValueOf(b.Elem().Type()))` recurses into the
`ΔPointer` arm — so this token *is* the printed order of `fmt.Println(map[I]int{…})`. Go answers with
the linker's type-section address, which is unspecified by its own admission (fmtsort's
`TestInterface`: "the relative ordering of types is unspecified", asserting only that same-type keys
group) and is not a function of anything the managed side can see. The identity-hash fallback above is
*worse* than unspecified for this case — CoreCLR draws an object's identity hash from a per-thread
PRNG, so the token is fixed per build but unrelated to the type, and the printed order flips whenever
an unrelated edit shifts how many hashes are drawn first. `reflectPointerToken` therefore routes a
`ж<rtype>`/`ж<abi.Type>` through `typeDescriptorOrderToken`, which packs the leading `IntPtr.Size`
bytes of the descriptor's Go name (the one `Type.String()` prints) big-endian, so comparing tokens
arithmetically compares the names lexically: **types that print alike token alike, and types that
print differently order by that printed name** — stable across builds, runs and unrelated edits.
Names agreeing over the whole packed prefix tie and fall through to fmtsort's concrete-value arm
(Go's own "no good answer" `-1`, settled deterministically by `SortStableFunc`'s stability); matching
Go's layout order for three or more key types is not on offer and would not be a property Go
promises. Guarded by the banked fmtsort suite (TestInterface's grouping) and the
`InterfaceInheritance` behavioral test's output comparison, which is what caught the PRNG model
landing tails. Full derivation:
[`docs/phase4/DESIGN-reflection-bridge.md`](../../phase4/DESIGN-reflection-bridge.md).

### A pointer `reflect` handed out as an `unsafe.Pointer` must convert BACK — the order token is remembered, not redefined

A Go pointer to managed storage has no machine address to report, so every projection of one to a scalar answers with a stable **order token** instead: `INilPointer.PointerOrderToken`, whose own remarks say plainly that tokens "are order keys, never an identity substitute". `reflect.Value.Pointer` and `reflect.Value.UnsafePointer` both project through it (`reflect/value_impl.cs`'s `reflectPointerToken`), and that contract is exactly right for the consumers it was written for — `fmt`'s `%p`, and `internal/fmtsort`'s ordering of pointer-keyed map entries, which compares two tokens arithmetically.

It is not sufficient for the other direction Go permits: converting the scalar back to a pointer and dereferencing it. `go/types`' own test suite does precisely that to reach the unexported `Config._Trace` field:

```go
func boolFieldAddr(conf *Config, name string) *bool {
	v := reflect.Indirect(reflect.ValueOf(conf))
	return (*bool)(v.FieldByName(name).Addr().UnsafePointer())
}

*boolFieldAddr(&conf, "_Trace") = manual && testing.Verbose()   // check_test.go:166
```

which converts to

```csharp
internal static ж<bool> boolFieldAddr(ж<types.Config> Ꮡconf, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(Ꮡconf.OrTypedNil()));
    return (ж<bool>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}
```

`ж<T>`'s `explicit operator ж<T>(uintptr)` builds a **native-address** box over whatever number it is handed (`m_nativeAddr`), so the `.Value` store writes a `bool` at the numeric value of an order token — an access violation when that page is not mapped, and silent heap corruption when it is. It killed `go/types`' converted test host outright at the first test that reaches the idiom, `TestCheck/blank.go`, with 542 verdicts behind it; the fault arrives as a bare `System.AccessViolationException` in `testFilesImpl` with no frame below it, because the faulting store is inlined at the call site.

The information needed to do better is never actually lost. `reflect.Value.Addr` surfaces the **real aliasing box** — an addressable Value already carries `addrBox`, minted by `GoReflect.FieldAliasBox` — and only the projection to a scalar discards it. So golib's `ManagedPointerTokens` (`golib/ж.PointerTokens.cs`) remembers the association the projection drops: the token that was handed out, and the box it named. `reflectPointerToken` registers on the way out, and the `uintptr → ж<T>` operator consults the table **first**, so a token that came from there recovers its own box and aliases the original storage exactly as Go's pointer would. Everything else keeps the pre-existing native-address route unchanged.

Two properties are deliberate. **The token VALUE does not change.** The obvious alternative — minting self-identifying handles from a reserved numeric range — would also change what `%p` prints and what order pointer-keyed maps print in, because those read the very same token; carrying the association out of band instead leaves every existing observable byte-identical, and costs one dictionary probe on a conversion that was already allocating an object. **Entries are weak, and verified on resolve**: the table must never be the reason a box stays alive, and a resolve re-derives the box's current token and requires equality, so a stale entry cannot answer. The type-descriptor path (`typeDescriptorOrderToken`, which packs type NAMES so fmtsort orders them lexically) returns before registration and is untouched — those tokens are shared by every descriptor with the same name and are not identities to recover.

Corpus footprint of the idiom: in all of GOROOT it is `go/types`' `check_test.go` (bool and string), its `cmd/compile/internal/types2` twin, and four descriptor-walking sites in `reflect/type.go`; `sync/atomic`'s two uses only test alignment (`p&7 != 0`) and never dereference. (Guarded by the `ReflectFieldAddrWrite` behavioral **output** test — writes to a bool, a string and an int field through `Addr().UnsafePointer()`, each read back through the ORIGINAL struct to prove aliasing rather than a detached copy, two derivations of one field compared for pointer equality, and the untouched neighbours asserted unchanged, all vs `go run`. It faults with an access violation on pre-fix golib.)

## Deep equality

### The `reflect.DeepEqual` bridge

**The `reflect.DeepEqual` bridge (`reflect/deepequal_impl.cs`, Phase-4 — blocker-map R5).** Go's
`deepValueEqual` keys its cycle-detection `visited` map on the values' internal data words (`v.ptr` /
`v.pointer()`) — eface addresses the managed bridge never populates — so the first slice/map/pointer
comparison converted the null `unsafe.Pointer` slot and NREd (`deepequal.cs:74` → unsafe `op_Implicit`;
first operational hits: strings and bytes `TestSplit`/`TestSplitAfter`). The converter skips **only**
`deepValueEqual` (`manualConversionFuncs["reflect"]`; `DeepEqual` itself stays auto — its body only
touches the bridged `ValueOf`/`Type`/`AreEqual`), and the hand-owned form re-implements the recursion
arm-for-arm with Go's switch over the bridge's **boxed** values: elementwise arrays/slices with the
`[]byte` fast path (`Span.SequenceEqual` standing in for `bytealg.Equal`); the nil-vs-empty slice
distinction read from the REAL backing (`m_array` null ⟺ the golib `default` = nil — the public
`slice<T>.Source` materializes a detached copy, so the impl reads `m_array`/`m_low` via cached
reflection, the same pattern as the bridge's `IsNull`/`Value` property reads); struct fields via
`goStructFields`; maps compared key-by-key through the backing `Dictionary` (same-map identity
short-circuits — Go's "same map object" rule — and a missing key fails exactly like Go's invalid
`MapIndex`); pointer identity as `ж<T>`-box reference equality (one box per variable ≙ Go's address
equality, so the same slice/pointer is deeply equal to itself even holding NaN); IEEE float semantics
(C# `==` on `double` — NaN ≠ NaN, like Go); funcs never deeply equal unless both nil. Cycle detection
mirrors Go's `hard()` step on managed identity: (pointer box | map `Dictionary` | slice backing array
+ `Low`) pairs in a reference-identity set, added before recursing so in-progress checks are assumed
true — self-referential structures terminate.

**Two defects the guard could not see (2026-07-26), one of them the guard itself.** The recursion read
the Value's raw `boxed` field, but an **ADDRESSABLE** Value — a slice element, an array element, a
struct field — carries its value behind `addrBox` (the `ж<T>` it aliases) and leaves `boxed` **null**,
so every such read saw null on *both* sides and the identity short-circuits fired:
`DeepEqual([][]byte{[]byte("ab")}, [][]byte{[]byte("ac")})` was **true** — each element's backing read
as null, matched "same initial entry of the same underlying array", and the elementwise walk never
ran. The recursion now reads `live` (identical to `boxed` for a non-addressable Value) once per side
and threads it through every arm. Second, `mapBacking` probed for a field assignable to the
**non-generic** `IDictionary`, which a generated named-map wrapper does not have — it holds a
`map<K,V>` **struct**, whose own backing store is one level deeper — so both sides of a *named*-map
comparison resolved to null, `ReferenceEquals(m1, m2)` matched them as "the same map object", and two
named maps of equal length were deeply equal **regardless of contents** (`identityRoot` was blind the
same way, so a named-map cycle was never detected either). The probe now takes that second step.

Neither showed up because the guard's own comparison was **vacuous**: it printed with the builtin
`println`, which writes to **stderr**, and the runners compare stderr by FIRST LINE only (Go's panic
reports carry a machine-specific stack trace, so a full stderr comparison can never match). 46 of its
47 assertions were unchecked. The guard now prints with `fmt.Println` — stdout, compared in full — and
is extended to 47 cases: the original slice/struct/map/pointer/cycle set plus named maps (equal,
differing value, differing key, differing length, self, nil-vs-empty), a named map as a struct field, a
named map OF slices, and nil-map-key parity on both plain and named `map[any]int`. Counter-proven: pre-
fix the `[][]byte` case and six named-map cases printed the wrong answer. (`Solitaire` is the only other
output-compared project that prints solely through `println`; its comparison is vacuous for the same
reason — recorded as a follow-up, together with tightening the runners' stderr rule to a full compare
whenever the exit code is not a panic.) The guard project references the
full-conversion `reflect` (the baseline stub has none): its `Directory.Build.targets` redirects the
emitted `core\reflect` reference to `core\reflect` — the Performance-suite pattern for
settings the per-transpile csproj regeneration would otherwise clobber. Surfaced by the guard's output
comparison: golib's `print`/`println` now render a `bool` as gc's runtime printer does (`true`/`false`,
not the BCL `True`/`False`).

**A third defect: the FUNC arm asked the wrong question (2026-07-31).** Go's rule is "func values are
deeply equal if both are nil; otherwise they are not deeply equal," and the arm implemented only the
second half — an unconditional `return false` — on the reasoning that two nil funcs would already have
matched through the `invalid == invalid` rule at the top. That reasoning holds for a **top-level** nil
func boxed as `any` (the null object, whose Value is invalid), and for nothing else: a nil func reached
as a struct **field** — or a slice/array element, or a map value — is typed by its *static* func type,
which by design makes it a **VALID** nil Value (see `Value.Field`). So any two structs carrying a nil
func field were reported unequal, and asking `IsNil()` of both values is the fix. The observable form is
a whole-struct comparison that can never succeed no matter how carefully the test normalizes the rest:
`compress/flate`'s `TestWriterReset` nils `fill`/`step`/`bulkHasher`/`bestSpeed`, copies `hashMatch`, and
substitutes `tokens`/`window` precisely so `DeepEqual` can compare everything else — and it failed at
**all ten** compression levels. That was the package's only failing test; with the arm corrected
`compress/flate` validates **64 / 64**. The diagnosis is worth recording because every *individual*
field compared equal (through `Field(i).Interface()`, which re-boxes and so re-enters the invalid-value
path) while the enclosing struct did not — the discrepancy between the two is what named the arm. The
`DeepEqual` guard gains nine cases: nil-func struct fields equal, one side non-nil, a struct with a
non-nil func field compared to **itself** (Go says not equal), differing non-func fields, nil funcs as
slice elements and as map values, and the top-level nil/non-nil pair that the old arm did handle.
Counter-proven by neutering the arm back to `return false`: the guard's output comparison fails.

**A fourth defect, the same shape one level out: the MAP arm read its entries UNTYPED (2026-08-17).**
The walk builds each entry's Value from the backing `Dictionary` — it cannot use the `MapRange`
iterator, because golib keeps a nil KEY in a side slot no iterator can see — and it built them with
`makeReflectValue`, i.e. from the stored object's dynamic type, where every other arm in the bridge
types a slot-derived Value by the slot's DECLARED type. An entry physically holding C# `null` therefore
came back as the **invalid** Value, so a nil element compared equal to a MISSING key and unequal to the
canonical typed nil that a reflective write stores. `encoding/json`'s `TestUnmarshal` rows #56–#63
decode into the 40-field `All` fixture and compare it against the table literal; `All.MapP` carries a
nil element, the decoder writes the box, the literal writes null, and eight subtests plus the aggregate
failed on that alone while `Marshal` of both sides produced byte-identical JSON. Entries are typed by
`Elem()` of the map's type now (`mapElemValue`), which is what `MapIndex` and `MapIter.Value` have
always done — so a lookup, a range and a `DeepEqual` over one map finally describe its elements
identically. The full rule, and why the container's verdict disagreed with every one of its elements,
is *A map ENTRY is a SLOT* below. Guarded by `ReflectBridgeClosure`'s *map nil element* rows;
counter-proven by reverting the arm, which reports `false false` where Go reports `true true`.

**A fifth defect, and it is the named-map one with SLICE substituted throughout (2026-08-19).**
`sliceData` — the probe that answers a Value's backing array and window offset, which is both the
`&x[0] == &y[0]` short-circuit's input and `identityRoot`'s cycle key — read `m_array`/`m_low` off the
boxed object's own type. A generated NAMED-slice wrapper (`type S []E`, e.g. `xml.CharData`,
`xml.Comment`, `net.IP`) has neither: it holds a `slice<E>` **struct** one level down, exactly as the
named-MAP wrapper holds its `map<K,V>`. So both sides resolved to `(null, 0)`, `ReferenceEquals(data1,
data2)` matched them as "the same initial entry of the same underlying array", and two named slices of
equal length were deeply equal **regardless of contents**; a nil named slice compared equal to an empty
one (the nil/empty rule tests the same two nulls); and `identityRoot` was blind the same way, so a
named-slice cycle was never detected. The probe now takes the same second step `mapBacking` takes,
gated on the type being slice-KINDED so a struct that merely HAS a slice field can never be mistaken
for one, and terminating because the nested value is a strictly smaller struct that carries the pair
itself.

The observable form is worth recording because it points at the wrong package. `encoding/xml`'s
`TestCopyTokenCharData`/`TestCopyTokenComment` clone a token's buffer, mutate the ORIGINAL, and assert
the two are no longer deeply equal; the failure message is *"CopyToken(CharData) uses same buffer"*,
which reads as a copy that failed to copy. It is not — `bytes.Clone` allocates a fresh backing array
and the `slice<byte>`→`CharData` conversion aliases correctly. The values were wrongly EQUAL, never
wrongly SHARING, and the owner was `reflect`.

**And the `[]byte` fast path was a second, independent gap in the same arm** — real, but NOT what made
the values compare equal (measured by A/B: fixing only the fast path leaves all eight wrong rows wrong;
fixing only `sliceData` makes all of them right, because the identity short-circuit fires FIRST and the
fast path is never reached). Go's `[]byte` special case is selected by the element **KIND**, never by
the slice's or the element's name — a raw `[]byte`, a defined slice type over `byte`, and a slice over
a defined byte element all route through `bytealg.Equal` — while the managed arm tested `live is
slice<byte>`, which only the first satisfies. It now asks `GoReflect.TryByteSliceView`, the same
element-kind alias `Value.Bytes`/`SetBytes` are built on (see *the `[]byte` VIEW of any Uint8-element
slice*), so all three shapes take one route in both APIs; both sides are the same Go type by the
`AreEqual` check above, so one view test settles both.

Guarded by the `DeepEqual` project's twenty new named-slice rows — the `CopyToken` shape verbatim
(clone, mutate the original, re-compare) plus a named byte slice through an interface, as a slice
element and as a map value; nil vs empty vs self; a named slice over a DEFINED byte element and one
over `string` so the fix cannot be byte-specific; and a self-referential `type recur []any` cycle,
which terminates only once the unwrap reaches the real backing array. Counter-proven failing-first:
**eight** of the twenty printed the wrong answer, every one of them wrongly `true`.

## The reflectlite bridge

### The `internal/reflectlite` mini-bridge

**The `internal/reflectlite` mini-bridge (`value_impl.cs` + `swapper_impl.cs`, Phase-4 reflection
bridge).** `sort.Slice`/`SliceStable`/`SliceIsSorted` route through reflectlite —
`ValueOf(x).Len()` and `Swapper(x)` — and the auto forms reinterpret the interface's eface
`{type,data}` words, so the first touch dereferenced a nil `ж<abi.Type>` (sort's `TestSlice`, the
first operational hit: `unpackEface` → `abi.Kind` → NRE). The fix mirrors the full `reflect`
bridge (see `reflect/value_impl.cs` and `docs/phase4/DESIGN-reflection-bridge.md`) for exactly the
mini-surface sort exercises: `ValueOf`/`unpackEface` build the `Value` over a companion
`partial struct Value { object boxed }` field — `typ_` takes the Phase-1 synthetic `abi.Type` and
the flag takes the Kind bits, so `Kind()`/`IsValid()` keep working from the auto `value.cs`
unchanged — `Value.Len` reads the boxed value through the golib container interfaces
(`@string`/`IArray`/`IMap`), and `Swapper` swaps through golib's non-generic `ISlice` indexer
(which applies the slice window offset, so swaps land on the shared backing store exactly like
Go's). The four declarations are skipped by the converter via the `manualConversionFuncs`
registry (`"internal/reflectlite"` in `go2cs/manualTypeOperations.go`); the rest of reflectlite —
including `packEface`/`Interface()` (used by `errors.As`) — stays auto and is NOT yet operational.
Verified by the sort differential: `TestSlice` flips to pass — `sort.Slice` sorts through the
managed Swapper, and its closing `SliceIsSorted` check reads length through the same `ValueOf` path.

### The reflectlite MINI-BRIDGE mirrors reflect one layer down — and the closure that validated it landed five rules in SHARED machinery

`internal/reflectlite` is Go's reflect one layer down, and its bridge follows one doctrine:
**mirror `reflect`'s hand-owns over the SAME golib machinery, so the two layers cannot disagree**
— `valueInterface` mirrors `packInterfaceValue`, `rtype.PkgPath` answers `GoReflect.GoPackagePath`,
and the `AssignableTo` hand-own retired to Go's own literal body over bridged `implements` +
`haveIdenticalUnderlyingType` exactly as `reflect`'s did (the identity walk's helpers are compact
mirrors in `type_impl.cs`, reading the same `GoFields`/`TryFuncShape`/`GoImplements` projections).
The suite validates **30 of 30 matched, 0 disclosed** (plus 2 skip-parity rows) since the
chan-direction cargo landed on 2026-08-20 and retired the three it had, and closing it surfaced
five rules that belong to EVERY consumer of the bridge, not to reflectlite:

- **Field order is Go DECLARATION order, not CLR metadata order.** go2cs-gen mints every embed's
  `ʗ` backing field in a GENERATED partial, and partial parts concatenate — so a struct whose Go
  declaration embeds first carries its embed LAST in metadata. `GoFields` (and therefore the
  offsets table, `Field(i)`, fmt's `%v` walk, json's member order) reorders by the generator's
  ALL-FIELDS constructor, whose parameters carry the declaration order — applied exactly when an
  embed is present and the parameter names form a bijection with the projected field names.
  (`GoStructLayoutTests.FieldOrder_IsGoDeclarationOrder_NotMetadataOrder`.)
- **Go's UNEXPORTED-method rule joined the structural implements probe.** Two same-named
  unexported methods from different packages are DIFFERENT methods, so `ast.Expr`'s `exprNode()`
  marker is satisfiable only from `go/ast` — `StructurallyImplements` compares the interface
  method's declaring package against the candidate extension method's declaring package class
  (`GoReflect.GoPackageClassPath`; a `-tests` whitebox bridge class carries the production
  [GoPackage] stamp and counts as the production package). Stated residual: a PROMOTED method's
  candidate is the generator's wrapper in the embedding package, so a cross-package embed
  satisfying a foreign marker interface answers false — the conservative direction.
  (`GoUnexportedMethodPackageTests`.)
- **A nil FUNC crossing into interface space keeps its type** — `GoReflect.CanonicalNilFunc`, the
  delegate-shaped half of the one-nil-encoding rule. A delegate's nil IS `null`: right in every
  func-typed slot, type-erasing inside an interface, where Go packs (type=func-type, value=nil).
  The carrier is minted ONLY at the eface boundary (both packers) and resolved away by every
  read-back path — dynamic type, type assertion (succeeds against exactly its own delegate type
  with the null delegate), marshalling (stores as null), nilness.
  (`PointerNilPredicateTests.CanonicalNilFuncCarriesItsTypeAndResolvesAwayOnEveryReadBack`.)
- **One nilness, one home**: `GoReflect.IsNilGoValue` (moved from reflect's private
  `isNilGoValue`) is the single boxed-value nilness — structural pointer predicate, map
  representational nilness, the generated `== nil` operator probe — read by `reflect.Value.IsNil`,
  reflectlite's mirror, and the interface arm of both.
- **`GoTypeName` grew three arms** the suite measured: a generic INSTANTIATION renders Go's
  bracket form with IMPORT-PATH-qualified arguments (`B[internal/reflectlite_test.A]` — what
  `Name()`'s trim-at-the-last-dot-outside-brackets recovers), an anonymous-INTERFACE lift renders
  structurally exactly as the struct lift does, and a pointer descriptor's dims thread to the
  pointee (`*[10]int`, the same unshifted-cargo rule `Elem()` applies).
  (`GoTypeDefinednessTests`.)

#### The `*_impl_test.cs` convention

**The `*_impl_test.cs` convention** is the piece that made the export_test surface hand-ownable:
`export_test.go` hands the suite raw `Value{typ, ptr, flag}` construction over descriptor
downcasts — unbridgeable literally — so `Field`/`TField`/`Zero` are registry hand-owns whose
implementation lives in `export_impl_test.cs`, the first TEST-file companion. The `_test.cs`
suffix keeps it under the production csproj's EXISTING test-artifact exclusion (no template
change, no corpus churn); `testConversion` globs `*_impl_test.cs` into the tests project's
compile items and the conversion digest. The companion mirrors `reflect`'s hand-owned
`Value.Field`/`Zero` over `GoFields`/`FieldAliasBox`/`ZeroValueOf`, and `TField` hands the
LITERAL `StructFieldType` the SYNTHESIZED `abi.StructType()` — the specialization section below —
so the type-side and value-side walks read one projection.

(Two converter emission rules landed with the closure, guarded by `structFieldEmission_test.go`:
a PARENTHESIZED array field type — `x ([32]int32)` — keeps its `= new(N)` initializer through
`ast.Unparen`, and the delegate lowering's parameter name-stripping shares
`convertToCSResultList`'s rule — a leading token is a name only when it is a plain identifier
that is not a type-leading keyword, so a bare `chan *T` parameter keeps its channel layer.)

---

[← Types](types.md) · [Index](../README.md)
<!-- {% endraw %} -->
