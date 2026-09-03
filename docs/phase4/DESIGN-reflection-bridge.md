# DESIGN — Native reflection bridge (Phase 4 operational)

> **Status (2026-07-24): Phases 1, 2, and PHASE-3 INCREMENT 1 (the write-back half) are SHIPPED.**
> Increment 1 landed via the §6.1 chip (session `optimistic-bassi-496625`; design + adversarial-review
> ledger in [`DESIGN-reflection-bridge-phase3-plan.md`](DESIGN-reflection-bridge-phase3-plan.md)),
> validating **errors (#34, 61/61 vs `go test -json`)** as its demonstrated consumer.
> Companion to the Phase-4 operational campaign in [`../Roadmap.md`](../Roadmap.md).
>
> ⚠ **"Phase 1/2/3" below are THIS DOCUMENT's scope phases** (§ *Scope*), unrelated to the project's
> Phase-3 / Phase-4 milestones.
>
> **Implemented** — `manualConversionFuncs` in `src/go2cs/manualTypeOperations.go` is the authoritative
> list; derive it fresh rather than trusting this summary: the Kind classifier + type helpers (golib
> `GoReflect`: `KindOf`, `GoTypeName`, `ElementType`, `IsComparable`, `TryAdapterWrappedType`),
> `internal/abi.TypeOf` (`type_impl.cs` synthesizes the descriptor from the managed `System.Type`),
> `reflect.{ValueOf, unpackEface, valueInterface}`, the ~17 `Value` readers + `MapIter.{Next,Key,Value}`
> + `Value.{MapKeys, MapIndex}` (2026-08-09, § below),
> `rtype.{String, Name, Elem, Field, NumField}`, **canonical interned `Value.Type`/`toType`**
> (`canonType` — the map-key-ordering fix, § below), `deepValueEqual` (`deepequal_impl.cs`), the
> `internal/reflectlite` mini-bridge (`ValueOf`/`Len`/`Swapper`), and the `synthType.Equal`
> comparability signal (encoding/csv, 2026-07-21).
>
> **Phase-3 increment 1 (SHIPPED 2026-07-24, the chip):** the ONE new primitive — an **addressable
> Value carries the `ж<T>` box it aliases** (`addrBox` companion; reads go through the box lazily,
> `Set` writes through its slot ref via cached compiled accessors over `ValueSlot`) — plus
> `reflect.{Value.Set, Zero, methodName}` and the reflectlite errors.As surface
> (`Value.{Elem,IsNil,Set}`, `rtype.{Elem,Implements,AssignableTo}`, `methodName`), all over shared
> golib machinery (`GoReflect.{TryMarshalAssignable, GoImplements, ReadPointerSlot/WritePointerSlot,
> CanonicalNilPointer, GoDynamicTypeOf}`) so reflection and emitted `_<T>` asserts can never disagree
> about a method set. With it landed: **canonical typed-nil pointer boxing** (X2 — converter + golib
> `ж<T>.NilBox` + gen; see ConversionStrategies-Reference `Canonical typed-nil pointer boxing`),
> structural nil-pointer identity (`INilPointer`; heap-box-holding-nil ≠ nil pointer), the R10
> adapter unwrap (`GoDynamicTypeOf` — descriptors classify the Go dynamic type, so adapter-held and
> raw-box values intern to ONE canonical Type), the golib assert-fallback unwrap + ж-overload
> closing (X1) with the non-generic `TryTypeAssert(object, Type, out object)`, dyn-wrapper
> `IInterfaceAdapter` transparency + the `[GoRecv]` method-set fix (X3), Go-parity empty-name
> subtest numbering (X4), the oracle's two-phase address-token pairing (X5), and `%v`-of-
> method-bearing-pointer fidelity (handleMethods wins, never `&`-prefixed). The `getcallersp`
> `NotImplementedException` chain is severed at `methodName` — the semantic boundary where a
> managed answer exists.
>
> **Phase-3 increment 3 (SHIPPED 2026-07-26) — the type-relation mirrors + conversion.** The
> deferred "reflect-side `Implements`/`AssignableTo` mirrors" row landed with its demonstrated
> consumers (encoding/gob's init via `validUserType → implementsInterface`, go/token's
> `TestSerialization`, internal/fmtsort's `ct()` table): hand-owned `rtype.{Implements,
> AssignableTo, FieldByName}`, `PointerTo`, `Value.{Convert, Cap, SetLen}` — each severing a
> descriptor-SPECIALIZATION read (`Reinterpret<abi.Type, interfaceType/structType>`, the ptrType
> prototype, the cvt*/unsafe_New chain) that cannot exist behind a synthesized descriptor — plus
> the `StructField.Index` stamp (gob's `FieldByIndex` walk), golib **pointer order tokens**
> (`INilPointer.PointerOrderToken`: equal pointers token equally, same-storage elements order by
> index — fmtsort's map-key ordering), `runtime.Pinner.Pin/Unpin` as no-ops by construction, and
> `-tests` init-order relocation (the erasable `initᴛᴛtests` static-ctor hook; see
> ConversionStrategies-Reference *Package-Level Variable Initialization Order*). Validated:
> internal/fmtsort 3/3, go/token 31/31; gob runs its Encoder/Decoder engines end-to-end (full gob
> validation remains open).
>
> **Phase-3 increment 3a (SHIPPED 2026-07-26) — `Value.Addr`, `rtype.PkgPath`, and the EMPTY
> interface.** Found by running encoding/gob's own 98-Test suite (68 → 79 passing). Three roots,
> each general:
>
> - **`Value.Addr`** derived its pointer TYPE through `ptrTo → typesByString → typelinks()` — the
>   linker-built, string-sorted type table, a runtime intrinsic with no managed form (its stub
>   throws). Every `Addr` therefore threw, taking out ELEVEN gob GobEncoder round-trip tests. But
>   the bridge already *holds* the address: an addressable Value ALIASES the `ж<T>` box its storage
>   lives in (`addrBox`), so `Addr` surfaces that box as a Pointer-kind Value, and `Elem` on the
>   result re-enters the same box — Go's `v.Addr().Elem() == v` contract (#32772) by construction.
> - **`rtype.PkgPath`** read the descriptor's `TFlagNamed` bit and `uncommon().PkgPath` name-offset,
>   which a synthesized descriptor never populates, so it answered `""` for EVERY type — gob's
>   `Register` then keyed its wire-type registry on the bare `"N2"` instead of `"encoding/gob.N2"`
>   (`TestRegistrationNaming`). The managed nesting carries the package identity: the declaring
>   `<pkg>_package` class names the package and the enclosing namespace names its parent
>   directories (`GoReflect.GoPackagePath`). Not a strict inverse for a major-version directory
>   (`math/rand/v2` → recovers `"math/rand"`) or a module dependency renamed away from its path
>   segment; exact everywhere else.
> - **The EMPTY interface is `object`, which reports `IsInterface == false`.** `GoReflect`'s
>   `GoImplements` and `TryMarshalAssignable` both gated their interface arms on `IsInterface`, so
>   `AssignableTo(t, interface{})` and `Set` into an `any` slot answered NO for every type —
>   gob's `decodeInterface` rejected every concrete value it had just decoded, then
>   `reflect.Value.Set` rejected it again ("gob: int is not assignable to type interface {}" →
>   "reflect.Set: value of type int is not assignable to type interface {}"). Both now carry the
>   explicit `typeof(object)` arm `KindOf` and `TryConvertTo` already had. Cleared gob's
>   TestInterfaceBasic / TestInterfacePointer / TestNestedInterfaces.
>
> **✅ gob's build blocker is CLOSED (2026-08-02, r37-gob) and gob is MEASURED — the "79 of 98" above
> is superseded by 86 of 106.** It was build-blocked when increment 5 looked:
> `package_info_internal_test.cs` emitted
> `[assembly: GoImplement<gob_internal_test_package.Point, Pythagoras>]`, but `Pythagoras` is declared
> in the EXTERNAL test package (`example_interface_test.go`, `package gob_test`) and gob declares a
> SECOND, unrelated `Point` there — so the record anchored the internal variant's same-named type and
> left the interface unqualified in a file where it is not in scope: `CS0246`, no test host, all 106
> verdicts empty. That was test-project-model record anchoring (the `splitWhiteboxVariantRecords`
> family), **not** reflection surface: a BARE record name resolves in the variant that RECORDED it, and
> the bridge's declared-name set was being consulted across variants. Fixed and guarded — see
> `ConversionStrategies-Reference.md`, *A BARE record name resolves in the variant that RECORDED it*.
>
> **Measured, one run, zero empty verdicts: 86 of 106 match** (C# 81 pass + 5 skip vs Go 101 pass + 5
> skip; 19 capability-excluded, 0 disclosed). Full per-root census — seven roots, only one of them this
> arc's descriptor surface — is the `encoding/gob` section of
> [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md).
>
> **Rooted gob residues owned by THIS arc (open, NOT disclosure candidates) — 3 of the 20.** The
> managed `array<T>` type does not carry its LENGTH, so `reflect.Type.Elem()` of a `*[N]T` loses N and
> gob sees the wrong type where the wire says `[7]int` — `TestSingletons` and `TestIndirectSliceMapArray`
> (`Value.Elem`/`abi.TypeOf` recover dims from the LIVE value, but a type-only walk cannot).
> `TestIgnoreDepthLimit` is `reflect.ArrayOf` → the `typelinks` stub. A fourth is adjacent and worth this
> arc's attention even though it is not the descriptor surface: the five remaining GobEncoder tests share
> ONE root that is not reflection at all — their `GobDecode` bodies write through a reinterpreted
> named-type pointer, `fmt.Sscanf(s, "VALUE=%s", (*string)(v))`, and the reinterpret is emitted as a
> value conversion into a fresh box (`Ꮡ((@string)(v))`) because `reinterpretManagedEmission` is gated on
> a deref context or a raw-address source, neither of which a pointer conversion used as an ARGUMENT
> satisfies. The direct-field-write case (`ByteStruct`, `TestGobEncoderStructSingleton`) passes, so
> `Addr`'s write-back path is sound. `TestNetIP` was never blocked on `net`'s package init (that claim is
> retracted — `fd_windows` is not on its stack): it dies in `unique`'s initializer, where r37-gob fixed a
> dead deref alias in `internal/concurrent` and thereby exposed the root behind it —
> `abi.TypeOf(m).MapType()` over a zero map yields a descriptor with no `Hasher`, so `NewHashTrieMap`
> throws `Delegate to an instance method cannot have null 'this'`. That one IS this arc's surface. The
> rest are gob wire/typed-nil behaviours, all bucketed on the board.
>
> **Phase-3 increment 4 (SHIPPED 2026-07-31) — the `getcallersp` chain: `runtime.Callers` +
> `Frames.Next` managed.** The chip's io increment. The failing surface was never the flatten
> OPTIMIZATION (pure Go, converts fine) but the tests' MEASUREMENT of it: `runtime.Callers` →
> `callers()` opens with `getcallersp()` (assembly) and `Frames.Next` reads linker `funcInfo`
> tables. Both contracts are answered natively in `runtime/managed_impl.cs` over
> `System.Diagnostics.StackTrace` with a GO-LOGICAL frame projection — only source-declared Go
> functions count; adapter shells (`IGoAdapter`) and go2cs-gen forwarders (`[GeneratedCode]`) are
> invisible, exactly as Go's interface dispatch adds no frame — so relative depths match Go's
> logical model (io's `readDepth == myDepth+2` holds bit-exactly); PCs are opaque interned
> process-lifetime tokens. **`getcallersp` itself stays an honest stub** — the chain is severed at
> the API boundary where a managed answer exists, the `methodName` rule. (⚠ Follow-up 2026-08-07,
> r43g-caller: this increment hand-owned the EXPORTED `Callers` only, and `runtime.Caller` calls
> the *lower-case* `callers` — so `Caller` itself stayed dead, which is what kept `log` and
> `testing/slogtest` on the stub. `methodName` was unaffected, being hand-owned in its own right.
> The lower-case funnel is hand-owned too now, one entry, and `Caller` works while staying
> auto-converted.) io: 45/54 → **47/54**;
> the remaining seven verdicts keep their non-reflection owners (os arc, StringCheckCall, two
> alloc-profile disclosure rulings). Full design:
> ConversionStrategies-Reference `runtime.Callers / Frames.Next walk the managed stack projected
> to GO-LOGICAL frames`. (Same landing repairs the whitebox adapter-pair resolver — a bare cast
> of a variant-local type resolved to the first same-simple-name FOREIGN record
> (`bytes_BufferжReader` for io_test's own `Buffer`, CS1503 ×20), which had silently re-walled
> the io build the board records as closed; anchor-local records now win, guarded by
> `TestBareCastPrefersAnchorLocalRecordOverForeignSimpleNameMatch`.)
>
> **Phase-3 increment 5 (SHIPPED 2026-08-02) — `reflectlite`'s `rtype.String`.** The chip's
> `context` increment, and the quietest descriptor read yet: `String()` is
> `t.nameOff(t.Str).Name()`, a **name OFFSET** into the linker-built name blob that a synthesized
> descriptor never populates — so every `reflectlite.TypeOf(x).String()` in the corpus answered
> `""`, and answered it **without faulting**, because `""` is a legal name for an unnamed type.
> `context`'s `stringify` fallback printed `WithValue(, c1k1)` where Go prints
> `WithValue(context_test.key1, c1k1)`. Bridged in `internal/reflectlite/type_impl.cs` over
> `GoReflect.GoTypeName` — the same answer `reflect`'s long-hand-owned `rtype.String` and `%T`
> give, so the full bridge and the mini bridge cannot disagree about a type's name — with array
> dims threaded as on the `reflect` side. Blast radius is three call sites (the fix plus two panic
> messages); `rtype.Name` is untouched and still `""` (it gates on the `TFlagNamed` bit
> `synthesizeDescriptor` never sets), and `errors` never reaches `String()` at all. `context`:
> 36/38 → **37/38**, leaving only the measured `TestAllocs` alloc-count disclosure its banking
> commit owns. Full design: ConversionStrategies-Reference *The type NAME is a descriptor read
> too*. **Recorded next gap of the same shape: `rtype.Name`** — deliberately not fixed without a
> consumer that demonstrates it.
>
> **Phase-3 increment 6 (2026-08-02) — `rtype.NumMethod`, the method-set SIZE.** The chip's
> `time` increment, and the same silent-degradation class as increment 5: `NumMethod` counts
> `uncommon()` method tables a synthesized descriptor never populates, so it answered **0 for
> every concrete type** — silently, because 0 is most types' correct count. The consequence hid
> one hop away: `encoding/json`'s `indirect()` gates its `Unmarshaler`/`TextUnmarshaler`
> discovery on `v.Type().NumMethod() > 0`, so no custom `UnmarshalJSON`/`UnmarshalText` was EVER
> dispatched — every `json.Unmarshal` into `time.Time` fell through to the raw-struct path
> (`TestTimeJSON`), and `{}` decoded silently where `Time.UnmarshalJSON` rejects it
> (`TestUnmarshalInvalidTimes`). ONE root, no further layers: with the gate answering, the
> increment-1/3a machinery (field-alias `Addr`, `Interface`, the golib assert, write-back through
> the aliased box) carries the whole dispatch, struct fields included. Hand-owned in
> `reflect/value_impl.cs` over golib `GoReflect.GoMethodCount` →
> `TypeExtensions.GoMethodSetCount`, counted over `GetGoMethodSetCandidates` — the SAME candidate
> source `StructurallyImplements` and the shell binder resolve through, so the gate and the
> assert behind it cannot disagree about a method set; deduplicated by projected Go name (one
> pointer-receiver method has two emitted shapes), exported-only for concrete types, ALL methods
> for interfaces, 0 for the empty interface, adapter shells unwrapped per R10. Guard:
> `JsonUnmarshalerDispatch` (four dispatch shapes vs `go run`). Full design:
> ConversionStrategies-Reference *The method COUNT is a descriptor read too*. **Recorded next gap
> of the same shape: `rtype.Method(i)`** — still auto over the same absent tables, and a
> `NumMethod() > 0` gate now lets method-enumeration loops get further than before; the first
> consumer that walks one demonstrates it.
>
> **Phase-3 increment 6 + 7 (SHIPPED 2026-08-03 as ONE increment) — the METHOD TABLE:
> `NumMethod`, `Method(i)`, `MethodByName`, and the method VALUE.** Increment 6 shipped the COUNT
> alone (`rtype.NumMethod`, gate-clean, `time`'s `TestTimeJSON`/`TestUnmarshalInvalidTimes` as its
> demonstrated consumers) and was **reverted from master**: the all-package sweep caught `math/rand`
> and `math/rand/v2`'s `TestRegress` failing with `panic: reflect: Method index out of range` —
> the successor gap increment 6's own report had recorded, arriving one session later. **The durable
> lesson, worth generalizing beyond this bridge: a truthful count is a PROMISE that the table behind
> it can be indexed.** `NumMethod` reads `uncommon()` method tables a synthesized descriptor never
> populates, so it answered 0 for every concrete type; while it did, every method-ENUMERATION loop
> was unreachable and the auto `Method(i)` reading the *same* absent tables could not be observed.
> Making the count truthful is exactly what made them reachable. A descriptor read and the gate in
> front of it are one atomic increment.
>
> Landed together: `rtype.{NumMethod, Method, MethodByName}` and `Value.Method`, all over ONE ordered
> list (`TypeExtensions.GetGoMethodSetEntries`) whose `.Count` IS `NumMethod` — a size and an order
> can no longer be derived separately and disagree — built on the same `GetGoMethodSetCandidates`
> the duck-typing assert and shell binder resolve through, deduplicated by projected Go name
> (keeping the delegate-bindable shape), exported-only for concrete types, and sorted ordinally by
> Go method name (Go's own table order; a promoted embed sorts in place). **A method value is an
> ordinary BOUND DELEGATE**, which is the design's economy: binding the receiver at `Method(i)` time
> makes the result a Kind-Func Value, so `mv.Type()`, `NumIn`/`In`/`NumOut`/`Out` and
> `mv.Call(args)` are all existing bridge surface **unchanged** — the receiver is already gone from
> the signature, exactly Go's method-value contract — and `Value.MethodByName` needs no hand-own
> because it composes the other two. Binding is expression-compiled because
> `Delegate.CreateDelegate` cannot close over a value-type first argument (measured), which every Go
> value receiver is; one compile per `MethodInfo`, a closure per bind.
>
> Found on the way and fixed in the table builder: **a `this object` extension method is golib
> plumbing, never a Go method.** The candidate source's assignability safety net admits them for
> EVERY type, so `TryCastAsInteger(this object, out ulong)` was in every method table —
> nondeterministically, since a late assembly load re-runs the scan: the same binary reported
> `NumMethod` **4 or 6 for the same type** depending only on load order. The shipped-then-reverted
> count was therefore also wrong in a way nothing could observe.
>
> Consumers: `math/rand` **43/43** and `math/rand/v2` **36/36** at their exact banked counts, with
> `TestRegress` now genuinely walking (`*rand.Rand NumMethod: 16` in Go's order,
> `Intn(1000000000) = 526058514` matching `go run`; before the pair it read 0 and the test passed
> VACUOUSLY — zero of its 320 golden comparisons ran). `time`: 146 → **148 pass of 159**, the two
> increment-6 JSON rows re-landed; its 9 remaining failures are the timer-model item (`TestChan` ×8)
> and the `TestUnmarshalTextAllocations` disclosure ruling, neither this arc's. Guard:
> `tests/Behavioral/ReflectMethodTableWalk`. Full design: ConversionStrategies-Reference
> *…and the count and the WALK are ONE increment*.
>
> **Phase-3 increment 8 (SHIPPED 2026-08-03) — the ZERO test, and a read that had degraded to a
> CONSTANT.** The chip's `encoding/gob` increment. `Value.IsZero` is three descriptor reads over
> flat memory (an `Equal` pointer against the shared `zeroVal` buffer, a `TFlagRegularMemory`
> all-bits-zero scan, and `v.ptr == nil` for a non-`flagIndir` value); a synthesized descriptor
> populates none, and the bridge populates neither `v.ptr` nor `flagIndir`, so the Array and Struct
> arms **both** fell to `v.ptr == nil` and answered **true for every array and every struct whatever
> it held**. Silent, like every member of this family — `true` is correct for the zero value of the
> same type. Measured against `go run` before the fix: `[2]uint8{1,2}`, `NA{1,2}`, `inner{N:1}`,
> `outer{P:&n}` all `IsZero=true` in C#, `false` in Go.
>
> Three things landed together because each gates the next. (1) **`Len` unwraps a named string** —
> every other named container answers through the golib interface its wrapper implements, but a
> `type NS string` wrapper implements none, so `Len` fell to its `0` default and `IsZero`'s String
> arm (`Len() == 0`) called every non-empty named string zero. The increment-6 rule in a second
> form: *the arm is a GATE on `Len`*, so the gate and the read behind it are one increment.
> (2) **`IsZero`** becomes Go's own recursive definition with the memory shortcuts removed — a
> composite is zero exactly when every element or field is, which is precisely the walk the
> shortcuts stand in for (Go itself falls back to it for a non-comparable, non-regular-memory type),
> so it needs only `Index`/`Field`/`NumField`, already answered. (3) **`Value.Grow`** reads a
> `*unsafeheader.Slice` off the same never-populated `v.ptr` and therefore **nil-deref'd for every
> caller** — `reflect.ValueOf(&s).Elem().Grow(1)` on `[]byte` prints `4 8` in Go and panicked here;
> it is now a managed reallocation (golib `GoReflect.GrowSlice`) written back through the aliased
> box exactly as `SetLen` does. Growth *within* capacity writes nothing (Go reaches `growslice` only
> past the capacity, and a spurious write would detach another view sharing the backing store), and
> the landed capacity is deliberately unpinned — Go's `growslice` rounds to a size class, so only
> `len+n` is guaranteed. Guard: `tests/Behavioral/ReflectZeroAndGrow`, byte-identical to `go run`.
>
> **The `MapType().Hasher` row is rooted and deliberately NOT landed — populating it would be worse
> than the failure.** `unique`'s one-root wall (15 of 19) and `net`'s last cctor root is a map
> descriptor with `Hasher`/`Key`/`Elem` unpopulated. It looks like the same shape as every read above
> and is not: `Hasher(unsafe.Pointer, uintptr) uintptr` must hash *the value at an address*, and the
> address that call site produces cannot name a managed value. Measured three ways: two boxes holding
> equal `@string` values necessarily have **different** addresses, so an address-derived hash can
> never make `unique.Make("hello")` agree with itself; a box whose pointee contains a reference has
> no pinnable slot and its address **moved across a forced GC**; and the `unsafe.Pointer` handed to
> the delegate retains no link to its source box, its constructor taking a `uintptr`. Key/elem
> *types* are recoverable from the carried `System.Type`, but landing those alone is a regression:
> `Key.Equal` is the comparability SIGNAL (pointer identity, not value equality), so a half-populated
> descriptor converts a loud construction failure into a map that silently mislays every key —
> **the increment-6 lesson inverted: a descriptor field whose read cannot be honored must not be
> populated to look truthful.** The remedy is one layer down and outside this arc's declared files:
> `internal/concurrent.HashTrieMap` is a managed-referent raw-metal case whose CONTRACT (a concurrent
> map over comparable `K`) the CLR answers natively while its MECHANISM (hash bytes at an address) it
> cannot — a hand-owned `_impl.cs` on the `sync.Mutex` precedent. Needs a coordinator ownership
> ruling before it is written.
>
> **The map READ pair landed 2026-08-09 (r57b), with `go/ast` as its first validator.**
> `Value.MapKeys` and `Value.MapIndex` were the two map methods `MapRange`/`MapIter.*`/
> `SetMapIndex` left behind, and both still ran their converted bodies: each opens with
> `v.typ().Reinterpret<abi.Type, mapType>()` to read the key/element types off the embedded
> `abi.MapType`. That is NOT the managed-box aliasing case `toRType` relies on — a synthesized
> descriptor is a bare `abi.Type` with no `abi.MapType` behind it, and the emitted `mapType` holds
> that embed as a promoted REFERENCE box (`ᏑʗMapType`), so the reinterpret reads the descriptor's
> first word as an object. Both then index through `mapaccess`/`hiter`, which `MapRange` had already
> replaced. `MapKeys` is now `MapRange` collected; `MapIndex` is a golib comma-ok lookup
> (`GoReflect.TryGetMapEntry`, routed through `IMap<K,V>` rather than the non-generic `IDictionary`
> so the nil key in golib's dedicated slot is found like any other). One key/element typing rule now
> serves the walk and the lookup alike, and a miss returns the invalid zero Value that
> `text/template`'s `funcs.go` already tests with `IsValid()`.
>
> **Unnamed struct types render STRUCTURALLY (same arc).** `reflect.Type.String()`/`%T` of a
> converter-LIFTED anonymous struct reported the lift's C# name (`ast_internal_test.typeᴛ1`) where
> Go prints `struct { X int; y int }`, and golib's `EmptyStruct` where Go prints `struct {}`. The
> `[GoType("dyn")]` stamp is what distinguishes a lift from an ordinary declared struct, and the arm
> in `GoReflect.TypeNaming` builds the text from `GoFields` — the same projection `NumField`/`Field`
> and the value side read, so a type's reported NAME and the fields it hands out cannot disagree.
> Format verified against the toolchain (embedded field contributes its type alone; a tagged field
> appends the `strconv.Quote`d tag). This partially answers open question 3 on the READ side.
>
> **The two Value-layer gaps asn1 and edwards25519 measured, closed (L7, 2026-08-11).** Both are
> descriptor reads that had degraded to a CONSTANT, and both were rooted against the real code
> rather than the board's attribution — which was half right in the first case and pointed one layer
> off in the second.
>
> - **Exportedness — `StructField.PkgPath`.** `IsExported()` is nothing but `PkgPath == ""`, and the
>   hand-owned `rtype.Field(i)` left `PkgPath` unset, so it answered TRUE for every field of every
>   converted struct. `encoding/asn1` opens both its struct arms with `if !t.Field(i).IsExported()
>   { return StructuralError{"struct contains unexported fields"} }`, so `Marshal` returned a nil
>   error and `Unmarshal` ran past the refusal into the unexported field and panicked in
>   `mustBeAssignable`. That panic is the evidence for the actual root: the two halves of the
>   read-only model had degraded INDEPENDENTLY — `Value.Field` already stamped `flagStickyRO` from
>   `GoReflect.GoFields`, so `CanSet()` was correct and the write WAS refused; only the type-side
>   descriptor had no answer. The board's "flagRO propagation in `Value.Field`" therefore names the
>   half that was already right. `PkgPath` now derives from the same projection's `Exported` bit
>   plus `GoReflect.GoPackagePath`. `Offset` stays unpopulated on the r39d rule (a Go byte offset
>   exists to be added to a data pointer); **`Anonymous` is the recorded next gap**, and wants ONE
>   increment with the field ORDER beside it — go2cs-gen emits a promoted-embed backing box AFTER
>   the declared fields, so `struct{X; y; Inner; inner; Ptr}` walks as `X, y, Ptr, Inner, inner`
>   here where Go walks it in declaration order. Guard:
>   `tests/Behavioral/ReflectUnexportedFieldFlags`.
> - **Fixed-size-array synthesis — `[GoArrayDims]` on the parameter.** `testing/quick` allocates its
>   argument from the parameter type alone, and `reflect.TypeOf(f).In(0)` of a `[32]byte` parameter
>   answered a dims-less array (`Len()` 0, `String()` `"[]uint8"`), so `New`/`Zero` built a
>   zero-length one. The root is NOT in quick or in the bridge's array construction: it is that a
>   func parameter is the one position no dims source reaches — no value to measure, no field
>   initializer to read, and a delegate type (`Func<array<byte>, bool>`) shared by every
>   `func([N]byte) bool`. So the converter now stamps the dimension at the parameter and
>   `GoReflect.FuncParamDims` reads it back off the delegate INSTANCE
>   (`Delegate.Method.GetParameters()` — verified to resolve for method groups, non-capturing and
>   capturing lambdas, natural-typed lambdas and local functions), which `abi.TypeOf` carries as
>   descriptor cargo and `rtype.In(i)` consumes. The cargo joins both interning keys, or
>   `func([32]byte) bool` and `func([64]byte) bool` — distinct Go types over ONE managed delegate
>   type — would share a descriptor. RESULT dims are deliberately not carried (a multi-result Go
>   func returns a `ValueTuple`, which has no per-element attribute position; no measured consumer
>   reads `Out(i).Len()`), and a bridge-minted method value keeps the dims-less descriptor because
>   its expression-compiled target carries no attributes. Guard:
>   `tests/Behavioral/ReflectFuncArrayParamDims`. Corpus footprint over all 557 behavioral packages:
>   3 files, 6 declarations.
>
> **Phase-3 increment 9 (SHIPPED 2026-08-19) — VARIADIC `Value.Call`, the one member no reflective
> invoke could ever reach.** The variadic arm of `Call` was not a missing descriptor read like every
> increment above it; it was a shape the whole reflective invoke API excludes by construction. A
> converted Go variadic lowers its tail to `params Span<TArg>` (golib variadic.cs), `Span<T>` is a
> ref struct, and `Delegate.DynamicInvoke`, `MethodBase.Invoke` and `System.Linq.Expressions` all
> refuse one — the last of those being why increment 3's `Expression.Lambda` receiver binder does not
> generalize here. So the call is made in TYPED code: eighteen small generic trampolines, one per
> family arity, closed over the delegate's own parameter types by `MakeGenericMethod` and cached as
> ordinary delegates (`GoReflect.InvokeVariadic`, the `elementBoxViaAt` idiom). Inside a trampoline
> the tail is a `TArg[]` whose conversion to `Span<TArg>` is ordinary, so nothing is boxed and the
> tail ALIASES the caller's array. A non-family delegate — a variadic literal in an `any` slot, which
> IS the `FuncMap` shape — rebinds onto the family by RETARGETING through `Invoke`, never by
> re-binding its own target and method: a bridge-built method value is expression-compiled and a
> compiled lambda's `Method` is not a runtime `MethodInfo`, which `Delegate.CreateDelegate` rejects
> outright. `text/template`: 38/52 → **49 of 52**, the stub's own 13 verdicts cleared, with the three
> residual verdicts on four roots that are NOT this arc's (`Value.Index`/`Value.Slice` over a
> Kind-STRING Value, which Go supports and the bridge rejects; the then-ratified chan-direction
> disclosure class, surfacing as `reflect: recv on send-only channel` in `walkRange` — RETIRED
> 2026-08-20 when direction became descriptor cargo and Recv/Send were bridged with it; typed-nil
> method dispatch, `(*W)(nil).Error()` rendering `-<nil>-` where Go renders `-nilW-`; and a
> three-index `Value.Slice3` nil deref). Canaries held: fmt 63/63,
> internal/fmtsort 3/3, encoding/json 491/491, internal/reflectlite 27/27. Guards:
> `tests/Behavioral/ReflectVariadicCall` (eleven shapes vs `go run`) plus seven
> `GoReflectBridgeClosureTests` rows for the golib-only shapes — the three delegate identities, the
> tail's aliasing, the multi-return, the refusal of a non-variadic delegate, and all nine family
> arities of both families, since only 0/1/2 have a consumer in the corpus today — proven
> failing-first by five separate neuterings. Full design: ConversionStrategies-Reference
> *`reflect.Value.Call` over a variadic func value is TYPED dispatch*.
>
> **AMENDED 2026-08-29:** `MakeFunc` left this list — hand-owned as `Value.Call`'s exact inverse
> (`reflect/makefunc_impl.cs` over golib `GoReflect.MakeGoFuncDelegate`; fixed-arity full, variadic
> a loud refusal mirroring `CallSlice`'s stance), banked with `net/http/httptrace` 2|0 and guarded
> by behavioral `ReflectMakeFunc`. Full design: ConversionStrategies-Reference *`reflect.MakeFunc`
> is `Value.Call`'s exact inverse*.
>
> **NOT implemented — remaining Phase-3 surface:** `MakeFunc`; `CallSlice` (no demonstrated
> consumer — `text/template` calls `Call`, not `CallSlice`, so the stub's named next consumer is
> retired as measured-wrong); `Value.Index`/`Value.Slice` over a Kind-String Value
> (text/template, measured); `SetMapIndex` delete-on-invalid (encoding/json); the Go
> unnamed↔named `directlyAssignable` refinement beyond identity+wrapper (binary named-slice cases
> if they surface); `FieldByName`'s embedded-field depth search (a promoted name currently answers
> the not-found path); open question 3 (field-name/tag fidelity — `[GoTag]` is carried but not yet
> projected). Known limitation (recorded): named func types collapse to their structural
> `Func<>`/`Action<>` under `canonType` System.Type interning — carried named-func identity is
> needed if a consumer (gob's type registry) lands on it.

## The problem

Go's `reflect` is built on reading an interface's internal two-word layout through `unsafe.Pointer`.
An `any` value is an `eface = { *abi.Type type; unsafe.Pointer data }`; `reflect` reinterprets the
address of the interface as that struct, reads the `type` word (a pointer to the runtime **type
descriptor** `abi.Type`), and reads/writes the value through the `data` word as flat memory at
computed field/element offsets.

None of that exists in the managed world. A go2cs `any` is a `System.Object` **reference** — one
word, no adjacent type descriptor, no flat-memory value the GC will let you address by offset.
Reinterpreting the object reference as `{type,data}` reads garbage and NREs. Concretely, the first
operational hit is:

```
color.New(FgGreen).Println("…")
  → fmt.Sprint(a…) → fmt.doPrint → reflect.TypeOf(arg).Kind()   // spacing decision
  → internal/abi.TypeOf(any a):  ~(ж<EmptyInterface>)(uintptr)(Ꮡ(a))   // reinterpret → NRE
```

(Plain `fmt.Println` sidesteps this: `doPrintln` never calls `reflect.TypeOf`; only `doPrint` — used
by `Print`/`Sprint`/`Sprintf` fallbacks — does. That is why the `fmt.Println("hi")` milestone runs.)

## Key insight — the entire unsafe chain enters at THREE constructors

A full read of the `reflect`/`internal/abi`/`fmt` surface (see the surface map below) shows the whole
model bottoms out on **one** primitive — the eface `{type,data}` reinterpret — reached at exactly
three points:

| # | Function | File | What it does today |
|---|---|---|---|
| 1 | `internal/abi.TypeOf(any a)` | `internal/abi/type.cs:125` | `&a` → `*EmptyInterface` → read `.Type` word |
| 2 | `reflect.unpackEface(any i)` (→ `ValueOf`) | `reflect/value.cs:156` | `&i` → `*EmptyInterface` → read `.Type` + `.Data` |
| 3 | `reflect.toType(*abi.Type)` | `reflect/type.cs:3040` | wraps a descriptor pointer as an `rtype` |

Every downstream `Type`/`Value` method then reads *from those words* — `Type.Kind()` masks
`abi.Type.Kind_`; `Value.Int()` does `~(ж<int64>)(uintptr)(v.ptr)`; `Value.Field(i)` does
`add(v.ptr, offset)`; etc.

**So the bridge is: replace those three constructors so they carry a `System.Type` (+ the boxed
`object` for a `Value`) instead of two raw words. The ~30 downstream methods `fmt` needs then become
ordinary managed-reflection wrappers** (`obj.GetType()`, `FieldInfo.GetValue`, array indexing,
`Convert.ToInt64`, `IDictionary` enumeration) with no `unsafe` anywhere.

## Architecture — a native `reflect`/`abi` shim over `System.Type` + `System.Object`

Hand-own the reflection **entry points and the exercised methods** (whole-file
`[module: GoManualConversion]`, the established pattern — cf. native `sync`, `atomic.Value`), so:

- `reflect.TypeOf(x)` returns a `Type` carrying `x.GetType()` (a `System.Type`), **not** a
  `ж<abi.Type>` descriptor pointer.
- `reflect.ValueOf(x)` returns a `Value` carrying `(object box, System.Type)`.
- `internal/abi.TypeOf(x)` returns a compatible handle (or `reflect` stops calling it — TBD, see Q4).
- Each `Type`/`Value` method is reimplemented over managed reflection.

The one genuinely new primitive is the **Kind classifier** — `System.Type → reflect.Kind` — the root
of every method. It reads go2cs's own representations and attributes:

| Go Kind | go2cs C# representation | detect |
|---|---|---|
| Bool / Int* / Uint* / Float* | `bool`,`nint`,`int`,`long`,`byte`,`double`,… | `typeof` |
| Uintptr | `uintptr` (golib struct) | `typeof(uintptr)` |
| Complex128 | `System.Numerics.Complex` | `typeof` |
| String | `@string` | `typeof(@string)` |
| Slice / Array / Map / Chan / Pointer | `slice<T>`/`array<T>`/`map<K,V>`/`channel<T>`/`ж<T>` | open generic typedef |
| Func | `GoFunc` / delegate | `IsSubclassOf(Delegate)` |
| Struct | `[GoType]` value struct (no `num:`) | `[GoType]` + `IsValueType` |
| Interface | Go interface → C# interface | `IsInterface` |
| UnsafePointer | `@unsafe.Pointer` (`: ж<uintptr>`) | `typeof` |
| *named* `type Celsius float64` | `[GoType("num:float64")] struct Celsius` | Kind = underlying; `Name`/`String` from the type |

The metadata the shim recovers Go type info from is **already emitted**: `[GoType(def)]` (struct/
interface marker + `num:<kind>` for named numerics), `[GoTag]` (= `DescriptionAttribute`, the raw Go
struct-field tag), `[GoRecv]` extension methods (the method set), and the golib generic types. C#
`FieldInfo` enumeration returns fields in declared (= Go source) order.

## Scope — three phases, each independently useful

**Phase 1 — `TypeOf().Kind()` / `.String()` (unblocks the color sample + scalar `Print`/`Sprint`).**
`doPrint` calls `reflect.TypeOf(arg).Kind()` for *every* arg; the value itself formats via the fast
path or the `Stringer`/`error`/`Formatter` C# interface assertions in `handleMethods` — which use **no
reflection at all**. So a `Type` shim exposing `Kind()`, `String()`, and `Elem()` (byte-slice check),
plus the Kind classifier, makes `fmt.Print`/`Sprint`/`Sprintf` work for every scalar and every type
with a `String()` method. **~1 shim type, ~4 methods, +1 classifier.** This is the minimal color-sample
unblock.

**Phase 2 — full `printValue` walk (composites without a `String()` method format correctly).**
Add the `Value` shim (~21 methods actually exercised: `Kind, IsValid, CanInterface, Interface, Type,
Bool, Int, Uint, Float, Complex, String, IsNil, NumField, Field, Elem, Index, Len, Bytes, CanAddr,
UnsafePointer, Pointer`), `Type.{Elem, Field(i).Name}`, a `MapIter` (`Next/Key/Value` over
`IDictionaryEnumerator`, for `fmtsort`), and `StructField.{Name,Tag,Type}`. **~2 core shim types +
MapIter + StructField, ~30 methods**, all managed reflection. Makes `%v`/`%+v`/`%#v` of structs,
slices, maps, and pointers correct.

**Phase 3 — write-back & call (`Value.Set*`, `Value.Call`, `MakeFunc`, addressability) — OPEN; the
chip's scope.** Needed by `encoding/binary`, `encoding/gob`·`json`·`xml`, `testing/quick`,
`text/template`, and (transitively) `math/big`; not by `fmt`. Larger, and **best designed against a
concrete consumer** — which is exactly why the charter (§6.1) spawns this chip only once a package's
differential actually lands on this surface, and requires designing WITH the user + adversarial design
review before implementation. Carry the `getcallersp` stub and the adapter-type `Kind`/`Elem` unwrap
in the same chip.

## Open design questions (for review)

> **Resolved by what shipped (2026-07-22):** **Q1** — the native shim was confirmed and built; the
> descriptor-synthesis alternative was not pursued. **Q2** — both Phase 1 *and* Phase 2 were built.
> **Q4** — the answer was *both*, deliberately: the entry points and exercised methods are whole-file
> hand-owned `*_impl.cs`, while the reusable managed logic (Kind classification, Go type naming,
> element types, comparability, adapter unwrap) lives in golib `GoReflect` so `reflect`,
> `internal/abi`, `internal/reflectlite`, and golib's own `builtin` formatting all share one
> implementation. **Q3 is still open** and belongs to the Phase-3 chip. Q1–Q4 are kept below as the
> record of the decision.

1. **Approach — confirm the native shim.** Replace the 3 constructors + reimplement the exercised
   methods over `System.Type`/`System.Object` (recommended). The alternative — synthesize faithful
   `abi.Type` *descriptors* from `System.Type` and keep Go's converted `reflect` reading them via
   `unsafe` offsets — is judged infeasible (the offset reads themselves don't work in managed memory;
   it is strictly more surface than the shim).

2. **Scope to build now — Phase 1 only, or Phase 1 + 2?** Phase 1 unblocks the color sample and all
   scalar/Stringer formatting for the least work; Phase 2 makes composite formatting correct. Phase 3
   is deferred regardless.

3. **Field-name / tag fidelity.** `fmt`'s `%+v`/`%#v` (and later `json`) need the **exact Go** field
   name + tag. C# field names are the Go names except where escaped (`@string`) or Δ-collision-renamed;
   `[GoTag]` already carries the tag. Options: reverse-map the escapes at the shim, or have the
   converter emit a per-struct Go-name table (a small attribute) the shim reads. (Phase 2 concern.)

4. **Where the shim lives.** Hand-own `reflect` + `internal/abi.TypeOf` as whole-file
   `[module: GoManualConversion]` (consistent with native `sync`/`atomic.Value`), **or** put the
   managed logic in a golib `GoReflect` helper that the converted `reflect` delegates into (smaller
   hand-owned footprint, but a converted↔golib seam through the shim). Recommend whole-file hand-own of
   the entry points, since the value/type model is pervasively unsafe and not worth converting.

## Surface map (reference)

`abi.Type` fields: `Size_`, `Kind_` (the field everything keys off), `TFlag`, `Str`/`PtrToThis`
(offset-encoded), `PtrBytes`/`Hash`/`Equal`/`GCData` (unused by fmt). Kind enum (values 0–26) is
defined identically in `internal/abi/type.cs:40` and `reflect/type.cs:245`. `fmt` calls only
`Type.{String, Kind, Elem, Field(i).Name}` and the ~21 `Value` methods tabulated in Phase 2.
`handleMethods` (`fmt/print.cs:795`) formats `Stringer`/`error`/`GoStringer`/`Formatter` via C#
interface assertions — no reflection. Fast-path `printArg` types (no reflection):
`bool, int/8/16/32/64, uint/8/16/32/64, uintptr, float32/64, complex64/128, string, []byte,
reflect.Value`.

## Fix — reflect.Type must be canonical (map-key ordering)

Go's `reflect.Type` is a canonical interned descriptor: `TypeOf(x) == TypeOf(y)` exactly when `x`
and `y` share a dynamic type, so `aType == bType` is a pointer compare. `internal/fmtsort.compare`
(the map-key ordering used by `fmt`'s `%v`) relies on it: `if aType != bType { return -1 }`.

The bridge minted a **fresh** wrapper per access — `abi.TypeOf` allocates a new `abi.Type` box, and
both `Value.Type()` and `toType` then build a fresh `rtypeжΔType` (an `IжAdapter` compared by box
identity via `golib` `AreEqual`). So two Types describing the *same* type never compared equal →
`compare` returned `-1` for every pair → the stable sort **reversed** the keys
(`map[b:2 a:1]` instead of `map[a:1 b:2]`).

**Fix:** hand-own `Value.Type` and `toType` in `reflect/value_impl.cs` (registered in
`go2cs/manualTypeOperations.go` `manualConversionFuncs["reflect"]`) so both route through `canonType`,
which **interns** the `ΔType` wrapper in a `ConcurrentDictionary<System.Type, ΔType>` keyed on the
`abi.Type.sysType` the Phase-1 `synthType` stamped. Identity-equality then matches Go. The cache is
process-lifetime (type descriptors are permanent, like Go's). Interning by `System.Type` preserves
Go's named-type distinctness for free: a `type Celsius float64` is a distinct `[GoType("num:float64")]`
struct whose `System.Type` differs from `float64`, so `TypeOf(Celsius) != TypeOf(float64)`. `typeSlow`
(method-value Types) stays auto — not exercised by `fmt`.

## Fix — the bridge's per-type marker reads are memoized (2026-07-26)

The bridge recovers Go type identity from managed metadata, so it reads the converter's per-type
markers — `[GoType]` for a type's kind and underlying, `[GoLocalName]` for a lifted local type's
original Go name — from its hottest entry points: `KindOf` (under every `ValueOf`, `Value.Field`,
`Value.Elem`, `MakeSlice`, `rtype.FieldByName`, and `abi.TypeOf`), `GoTypeName` (under every `%T` and
`reflect.Type.String()`/`Name()`), and `TryConvertTo`/`TryUnwrapWrapperValue` (under the whole
`Value.Set`/`SetMapIndex`/`Call`/`Convert` marshalling surface). **Custom-attribute retrieval
materializes fresh attribute instances on every call** and none of those callers caches its own
result, so the cost was paid per VALUE rather than per type — measured at 165–558 ns and 72–448 bytes
per call across those entry points, of which the attribute read was the bulk. All four reads now route
through two per-type `ConcurrentDictionary` memos (`goTypeMarkerOf` / `goLocalNameOf`), which is
sound for the same reason `canonType`'s intern above is: type descriptors are permanent, and a loaded
type's own attributes cannot change. Full measurement and the two remaining non-attribute residuals
are in [`DESIGN-iface-shell-caching.md`](DESIGN-iface-shell-caching.md) §11.2, which is where the
audit that found them lives.

## Fix — a type descriptor's order token is its NAME, not its box identity (2026-08-10)

The section above made two `reflect.Type`s for one Go type compare **equal**. This one is about what
happens next, when they compare **unequal**: `internal/fmtsort.compare`'s `reflect.Interface` arm
orders interface-kinded map keys by their dynamic types, and it does that by comparing the two type
descriptors as POINTERS —

```go
c := compare(reflect.ValueOf(aVal.Elem().Type()), reflect.ValueOf(bVal.Elem().Type()))
```

`reflect.ValueOf(aType)` is a `*reflect.rtype`, so `compare` recurses into its `reflect.Pointer` arm
and returns `cmp.Compare(aVal.Pointer(), bVal.Pointer())`. That token is therefore not an internal
detail: it **is** the printed order of `fmt.Println(map[I]int{…})`.

Go answers with the descriptor's machine address — the linker's type-section layout. That ordering is
deliberately unspecified: fmtsort's own `TestInterface` says *"the relative ordering of types is
unspecified"* and asserts only that same-type keys form contiguous sorted subgroups. It is also not a
function of anything the managed side can observe; measured against Go 1.23.1, one program lays
`main.Mango` before `main.Zebra` before `main.Apple` while another lays the same three names in
alphabetical order.

The managed bridge has no addresses, so `reflect.Value.Pointer` answers `ж<T>.PointerOrderToken`,
which for a descriptor box composes `RuntimeHelpers.GetHashCode` of the underlying `abi.Type`. That
is **worse than unspecified**. CoreCLR draws an object's identity hash from a per-thread PRNG, so the
token is fixed for a given build but bears no relation to the type it describes, and the printed
order flips whenever an unrelated edit changes how many hashes are drawn before these two. That is a
coin flip re-tossed on every commit, and it landed tails: `tests/Behavioral/InterfaceInheritance`
(a `map[I]int` keyed by `main.T1`/`main.T2`, both rendering as `""`, so only the values reveal the
order) was graduated to output comparison green at `c87a7f145` and printed `map[:2 :1]` against Go's
`map[:1 :2]` by `c7b297c87`. No commit in that window broke it — bisecting names a bystander. The
demonstration is direct: adding a diagnostic that merely touches `reflect` BEFORE the `Println` moves
both tokens and restores the correct order.

**Fix:** `reflectPointerToken` (`reflect/value_impl.cs`) now routes a pointer to a type descriptor —
`ж<rtype>` or `ж<abi.Type>` — through `typeDescriptorOrderToken`, which packs the leading
`IntPtr.Size` bytes of the type's Go name big-endian, so comparing tokens arithmetically compares the
names lexically. The name is the one `Type.String()` prints (`GoReflect.GoTypeName` over the
descriptor's `sysType` + `arrayDims`), which gives the invariant worth stating: **types that print
alike token alike, and types that print differently order by that printed name.** Same-type grouping
is therefore exact, and the order is stable across builds, runs and unrelated edits — the one
property the address model cannot offer and the PRNG model actively destroys.

Two bounded residuals, both deliberate:

- **Names agreeing over the whole packed prefix tie** (8 bytes on a 64-bit host). `compare` then
  falls through to `compare(aVal.Elem(), bVal.Elem())`, the same arm Go reaches for two keys of one
  type; for two *different* types that arm returns `-1` either way, so the pair's relative order is
  settled by `slices.SortStableFunc`'s stability rather than by the comparison. Deterministic, which
  is the property being bought here.
- **Matching Go exactly is not on offer** for three or more distinct key types, since Go's order is
  its linker's. `InterfaceInheritance` agrees because `main.T1` precedes `main.T2` both ways; a
  program whose layout order diverges from name order would not, and a behavioral test asserting one
  would be asserting something Go does not promise.

Only two call sites in the corpus read `reflect.Value.Pointer`: `internal/fmtsort` (this ordering,
validated 3/3 including `TestInterface`'s grouping assertion) and
`vendor/golang.org/x/crypto/internal/alias`, which compares slice-element pointers and never reaches
the descriptor branch.

## Fix — a raw `GetType()` is not an eface type word (2026-09-02)

`NilFuncValue` carries Go's eface **type word** for a nil func, and the design's soundness
condition is that every observer resolves it through one of four bridge hooks —
`GoDynamicTypeOf`, `builtin.TryTypeAssert`, `TryMarshalAssignable`, `IsNilGoValue`. An observer
that calls `GetType()` directly sits outside that set and is wrong in BOTH directions at once: it
answers `typeof(NilFuncValue)` where a LIVE value of the same func type answers the delegate type
(a false inequality), and it answers the SAME class for nils of two DIFFERENT func types (a false
equality, which the per-type interning cannot reach because interning distinguishes instances, not
classes). The second is the dangerous one: nothing panics, a wrong-typed store is simply accepted.

A census of the production corpus (`git grep`, not bare `rg` — `src/core/.gitignore` under-counts
it) found 82 `GetType()` uses across 23 files, of which 74 are bridge machinery *implementing* the
hooks. The entire population of raw eface type-word comparisons in converted code is **four sites
in one file** — `sync/atomic/value.cs` 46/66/84/99, the `Store`/`Swap`/`CompareAndSwap`
consistency checks — and that file is hand-owned (`[module: go.GoManualConversion]`, checked
line-anchored). That is why the remedy is the hand-own's four comparisons rather than a converter
emission rule: there is no emission at these sites to change. Consumer: `internal/poll`'s
`TestSplicePipePool`, which stores a real `func(int)` and then `(func(int))(nil)`.

⚠ The census instrument's edge, stated rather than implied: it keys on `GetType()`. A type-word
comparison written another way — an `is` pattern, an `Equals`, a type captured into a variable and
compared later — would not appear. The indirect shapes were checked and only bridge-internal ones
were found, but a future site written that way is invisible to this census.

### The family this belongs to: a nil CONVERSION loses its type cargo

Three constructs, one emission site, one lesson — recorded together so the fourth is not derived
from scratch. Go writes a typed nil as a conversion, and the managed rendering of that conversion
drops whatever the Go TYPE carried that C# has no room for:

| construct | what is lost | the arm that restores it |
|---|---|---|
| `(<-chan T)(nil)` | the channel's DIRECTION | `chanDirNilValue` (chanDirectionCargo.go) |
| `(*[N]T)(nil)` | the array's LENGTH | `nilArrayPtrValue` |
| `(func())(nil)` | the func TYPE (eface type word) | `OrTypedNilFunc` → `NilFuncValue` |

Each renders as a cast of a null/default — the shape that carries nothing — so each needs an arm at
the nil-conversion interception in `convCallExpr` putting the cargo back. A NON-nil value of the
same type needs no arm: it carries its own type. If a fourth construct appears, it joins this table
rather than earning a fourth independent derivation.

## Fix — Go's panic-name climb is threaded, not walked (2026-09-03)

`Value`'s `mustBe*` family names the method a panic came from. Go resolves that name by **climbing
the stack**: `runtime.Callers` over `var pc [5]uintptr`, filtered to a frame whose symbol starts
`reflect.Value.` with an EXPORTED initial. The bridge transcribed that climb faithfully over a
managed `StackTrace`, reconstructing the `reflect.Value.` prefix from the frame's **first parameter
type** (a Go method on `Value` is emitted as a static extension, so the receiver is a parameter and
`DeclaringType` is the package, not `Value`).

**The transcription is correct in Debug and cannot work in Release.** The JIT inlines the exported
`Value` method into its caller, so the frame the walk looks for is not on the stack at all.
Measured as reflect's own `TestValuePanic` at `-test-config Release`: `Go="pass" C#="fail"`, the
stack showing `mustBe` invoked directly from the test's closure with no `Recv` frame between them,
and the panic reading `call of unknown method on string Value` where Go names the method. No amount
of care in the walk fixes this — it is asking the runtime for something the runtime has discarded.

The name is threaded instead, via `[CallerMemberName]` on the `mustBe*` parameters. That argument is
a **compile-time constant**, so no tiering or inlining decision can move it. `valueMethodName`
becomes a composer:

```csharp
return method.Length != 0 && char.IsUpper(method[0])
    ? (@string)("reflect.Value." + method)
    : "unknown method"u8;
```

**Go's frame filter survives as the uppercase test on the threaded name**, and that is what makes the
design self-correcting: an internal helper that threads nothing arrives with its own lowercase name
and lands on Go's OWN fallback — which is exactly what Go's climb produces when it finds no `Value`
method. Two helpers rely on that and need no code: `extendSlice`, and any future helper like it.

### The direction nothing was testing

The retired walk was also wrong the OTHER way, and no test covered it. Its receiver-type filter
matched **package-level functions that take a `Value` first**, which Go's symbol filter does not:

| construct | Go 1.23.12 prints | the walk printed |
|:--|:--|:--|
| `reflect.Append(nonSlice, …)` | `call of unknown method on int Value` | `call of reflect.Value.Append …` |
| `reflect.AppendSlice(nonSlice, …)` | `call of unknown method on int Value` | `call of reflect.Value.AppendSlice …` |
| `reflect.Select` (`mustBe*` arm) | `call of unknown method on string Value` | — (its first parameter is not a `Value`) |
| `reflect.Copy` (`mustBeExported` arm) | `unknown method using value obtained using unexported field` | `reflect.Value.Copy …` |

Go's frames there are `reflect.X`, never `reflect.Value.X`. `Append`, `AppendSlice`, `Copy` and
`Select` therefore thread the sentinel explicitly; `Copy`'s and `Select`'s own kind-arm `ValueError`s
already hardcode `reflect.Copy`/`reflect.Select` and match Go unchanged.

### Why five members are hand-owned

`value.cs` is generated and every `-tests` run regenerates it, so the attribute cannot live there:
the prototype was silently reverted mid-measurement. `flag.mustBe`, `flag.mustBeExported{,Slow}` and
`flag.mustBeAssignable{,Slow}` are registered displacements whose bodies are otherwise Go's,
unchanged — the threading is the whole delta. `Append`/`AppendSlice` join them only to carry the
sentinel.

Two consequences of moving a member out of `value.cs`, neither guessable: a **file-scoped `using`
alias** (`ꓸꓸꓸValue`) does not follow it, and a **hoisted string literal** the converter emits WITH a
function (`reflectAppendSliceˢ`) leaves the emission entirely. Both are re-declared in the hand-own.

### Not threaded, and why

`runes`/`setRunes`'s `mustBe*` are **unreachable**: each has exactly one caller
(`cvtRunesString`/`makeRunes`) reached only after `convertOp` validated the kind, and `makeRunes`
builds its receiver with `New(t).Elem()` two lines above. `panicNotBool`/`panicNotMap` are declared
and **never called** in this corpus — `Bool` and the map methods are hand-owned and check kinds
directly. `Value.call` (293 lines) has **zero callers**: `Call`/`CallSlice` are hand-owned and reach
`mustBe` from the exported frame. Reachability retired all four; none needed a parameter.

### The generator defect this exposed

`ΔValue` gets `mustBe` by **promotion** from the embedded `flag`, and `go2cs-gen`'s parameter harvest
dropped attributes and defaults — so the forwarder emitted `mustBe(this ΔValue, ΔKind, string)` with
no default and every `v.mustBe(Chan)` stopped binding, raising CS1929 in the CONSUMER. Both harvests
(syntax and symbol) now carry them, with the default stripped centrally in `GetCallParameters`
(`f(method = "")` is a syntax error). Guarded by `GenTests/PromotedParameterDefaultTests`. A dropped
default is loud; a dropped `[CallerMemberName]` would have been **silent** — the forwarder still
compiles and still has a name parameter, it just can no longer capture its caller.
