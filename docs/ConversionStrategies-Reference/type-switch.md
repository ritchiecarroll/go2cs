# Type Switch Statements

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#type-switch-statements)
For a Go type-switch, C#'s type-pattern `switch` works well. The runtime exposes the dynamic type via `.type()`, and the empty interface is `any`:

```go
func do(i interface{}) {
    switch v := i.(type) {
    case int:
        fmt.Printf("Twice %v is %v\n", v, v*2)
    case string:
        fmt.Printf("%q is %v bytes long\n", v, len(v))
    default:
        fmt.Printf("I don't know about type %T!\n", v)
    }
}
```

converts to:

<!-- illustration: not converter output -->
```csharp
internal static void @do(any i) {
    switch (i.type()) {
    case nint v: {
        fmt.Printf("Twice %v is %v\n"u8, v, v * 2);
        break;
    }
    case @string v: {
        fmt.Printf("%q is %v bytes long\n"u8, v, len(v));
        break;
    }
    default: {
        var v = i;
        fmt.Printf("I don't know about type %T!\n"u8, v);
        break;
    }}
}
```

**`case int:` matches a Go `int` and nothing else.** A Go `int` maps to C# `nint` and `uint` to `nuint`, so `case int:` emits one label, `case nint`, and `case uint:` one label, `case nuint`. Go matches the dynamic type exactly, so an `int32`, a `rune` (≡ `int32`) or a `uint32` held in the interface does NOT take that arm: it goes to its own `case int32:`/`case uint32:` when the switch lists one, and to `default` otherwise, as in Go. That holds because every Go `int` reaches an interface boxed as `nint`, untyped constants included (`do(1)` emits `@do((nint)1)`; see [Empty Interface](empty-interface.md#an-untyped-constant-boxed-as-any-boxes-at-gos-default-type)). Guarded by the `TypeSwitchIntWidthCases` behavioral test (`int`, `int32`, `int64`, `uint`, `uint32`, `uint64`, `rune` and untyped constants over `case int`/`case uint`/`default`, output-compared vs Go).<!-- history: until 1.26.10.2 (seat #15) every `case nint` label was followed by a synthetic `case int32` (and `case nuint` by `case uint32`) running the same body, skipped only when the switch listed int32/rune/uint32 itself. It dated from 79ccad9d65 (2024-12-01), when an untyped `whatAmI(1)` emitted a bare C# `1` that boxed as System.Int32; once 96b13eec20 (2026-07-12) boxed untyped ints as nint it only misrouted int32(5), rune(5) and uint32(5) into Go's int/uint arm. A census of hand-owned code found no site boxing a C# int where Go has int. -->

**Duplicate-mapped cases — the identical-body merge.** Go type distinctions can *vanish* in the C# type map, making a later case unreachable (CS8120). `uint` + `uintptr` does not collide: `uintptr` is golib's distinct `uintptr` struct (see [Constant Values](constants.md#constant-values)), so both cases emit their own labels, each dynamic type routing to its own body exactly as in Go.<!-- history: the canonical example was uint + uintptr under the old System.UIntPtr alias --> The merge machinery serves any pair of Go types that shares a C# type — two methodless named func types with one signature, which both render as their base delegate (`type Left func(bool) bool` and `type Right func(bool) bool` are both `Func<bool, bool>`): a duplicate-mapped case merges **only when its Go body is byte-identical** to the first occurrence's — the earlier label already routes both dynamic types to that shared body, so the merge is exact. A marker comment replaces the duplicate label:

<!-- illustration: not converter output -->
```csharp
switch (raw.type()) {
case Func<bool, bool>: {
    return "either";
}
/* case Func<bool, bool>: merged with an earlier case mapping to the same C# type (identical body) */}
```

If the bodies **differ**, both labels are kept and the CS8120 stands: a compile error is preferable to silently routing one Go case's values into another case's body. Duplicate detection keys on the resolved C# type (`rune`→`int32`, `byte`→`uint8`) per switch statement. Guarded by `TestTypeSwitchCollapsedFuncCase` (an identical-body pair that merges, a differing-body pair that keeps both labels, and two different delegates that never merge); the `TypeSwitch` behavioral test keeps `uint` + `uintptr` with different bodies, each value hitting its own case.

**Multi-type cases stack labels and bind at the tag's interface type.** Go binds a type-switch case
variable at the listed CONCRETE type only when the clause lists exactly **one** type; a multi-type clause
(`case *Alias, *Named:`) binds it at the TAG's static (interface) type. Splitting such a clause into one
concrete-bound C# case per listed type would break every body use in an interface-typed context — as an
argument (`isGeneric(t)` with `t` a `ж<Alias>`, CS1503), an interface assignment (CS0266), and an
extension-method receiver (CS1929).<!-- 18 errors in go/types alone under the split emission -->
A multi-type clause emits **stacked labels binding only a discard, over one shared body** and
re-binds the variable to the guard expression — the same re-bind the `default` arm uses — so the body
compiles at the interface type exactly as in Go:

<!-- illustration: adapted, owed a real excerpt -->
```csharp
switch (x.typ.type()) {
case ж<Alias> _:
case ж<Named> _: {
    var t = x.typ;          // t: Type (the tag's interface type), as in Go
    if (isGeneric(t)) { … }
    break;
}
case ж<ΔSignature> t: {     // single-type case keeps the concrete binding
```

The `_` designation is load-bearing, not stylistic: it forces the label into PATTERN context. A bare
`case int8:` label resolves the identifier as an EXPRESSION first, where `using static go.builtin` finds
the same-named conversion FUNCTION (`int8(…)`) — a method group, neither constant nor type — failing
CS8917 (encoding/binary's `Size`/`intDataSize` stacks).<!-- caught by the census build, not the behavioral
corpus, whose labels happened not to collide --> `case nil` stacks as `case null:`, a dynamic-interface
label stacks in its non-binding `{} ᴛn when ᴛn._<Iface>(out var _)` form. The
duplicate-mapped-case merge applies per label (a merged label leaves its marker comment above the stack).
An UNBOUND multi-type clause (`switch x.(type)`) stacks the same way with no re-bind, over one body. The re-bind
re-evaluates the guard EXPRESSION at body entry — harmless
for a pure tag (nothing can mutate it between dispatch and entry), and an IMPURE tag is hoisted into
a one-time temporary first (see *The type-switch tag evaluates exactly once* below). (Guarded by the `TypeSwitchMultiCase` behavioral test — bound multi-type
cases over values and pointers with interface-dispatched body uses, `nil` stacked with a concrete type, an
unbound multi-type clause, and an `int` stacked with other types, output-compared vs Go.)
**Runtime dispatch — `.type()` unwraps the interface adapters.** The case patterns match against
whatever object the switch operand `.type()` returns, so it must surface the Go DYNAMIC value, not
the C#-only wrapper classes the runtime uses to carry it. A non-empty interface value created from
a Go pointer is a generated `IжAdapter` wrapping the receiver box (`var v shape = &c` emits
`new circleжshape(Ꮡc)`; see [Interfaces](interfaces.md#interfaces)), and an interface-to-interface
assignment can wrap the source in an `IInterfaceAdapter`. `.type()` therefore unwraps —
`IInterfaceAdapter.Value` chains first, then `IжAdapter.Box` (or a value adapter's
`IValueAdapter.Value`) — mirroring the type-assert machinery
in `_<T>`, so a Go `case *circle:` (emitted `case ж<circle> t:`) matches the adapter-wrapped value
exactly as it matches the raw box that an EMPTY interface (`any`) holds directly. The bound `t`
IS the original receiver box, so writes through it (`t.Value.r += 10`) alias the original object,
matching Go's interface-holds-the-pointer semantics; `case nil` (emitted `case null:`) still sees
the nil interface unchanged. An interface holding a **nil `*T`** (in Go a non-nil interface that
matches `case *T:` binding a nil `t`) takes the same path: the pointer adapter wraps the canonical
typed-nil box `ж<T>.NilBox`, never a null, so `.type()` unwraps it, `case *T:` matches with a nil
pointee, and `case null:` does not.<!-- history: earlier, a nil *T stayed wrapped (a null Box) and fell to `default`;
AdapterImplTemplate's constructor substitutes ж<T>.NilBox. --> (Guarded by
the `TypeSwitchPointerAdapter` behavioral test — pointer-receiver implementations of a non-empty
interface dispatched through single-type, multi-type, no-bind, and write-through cases, plus
value-receiver, `nil`, and raw-box-in-`any` controls; and the `TypeSwitchNilPointer` behavioral test — a nil
`*T` in a non-empty interface and in `any`, a binding and a multi-type case, and a pointer-receiver call on the
nil pointee.)

**Both unwrap tiers sit behind ONE `IGoAdapter` probe.**<!-- 2026-07-26 --> `.type()` is evaluated once
per type switch, and testing `IInterfaceAdapter` and then `IжAdapter` separately would cost two
*failing* interface type tests for every ordinary Go value, which is what the overwhelming majority
of type switches dispatch on. Measured on windows/amd64 (.NET 9), that is ~2.9 ns each on the JIT,
against a failing sealed-class test too small to measure: a failing interface `isinst` walks the
type's interface map. The adapter interfaces (`IInterfaceAdapter`, `IжAdapter` and
`IValueAdapter`) share an empty base marker, `IGoAdapter`, and `.type()` probes it once to
gate every unwrap, which took the type switch from **16.0 ns to 4.5 ns per iteration** on `PerfIface`
(windows/amd64, .NET 9). The `string` → `@string` arm sits after the `IжAdapter` arm, which cannot change an answer
because `string` is sealed and implements neither marker. See
[Interfaces](interfaces.md#interfaces) for the same gate on the type-assert side and the full measurement.
<!-- Platform: the 2026-07-26 measurements were taken with .NET SDK 9.0.316 and a
win-x64 runtime target, per docs/phase4/DESIGN-iface-shell-caching.md §10. interfaces.md's decomposition
of the same campaign reports the type switch as 16.0 → 11.0 → 4.4 ns across two changes. -->

**Named-interface case labels dispatch by method set through the adapter registry.** An
INTERFACE-typed case label — named (`case fmt.Stringer:`, `case error:`) or anonymous — must match
by Go METHOD-SET semantics, and after the unwrap above the operand is the raw receiver box, which
never nominally implements a C# interface (the generated pointer adapter does). A plain C# type
pattern (`case Stringer t:`) would therefore miss every pointer-sourced implementation. Every interface
label, named or anonymous, emits the `when`-guard form —
`case {} Δx when Δx._<Stringer>(out var x):` — routing dispatch through golib's type-assert
machinery, which resolves in order: a **nominal** implementer (value structs made partial, adapter
instances, duck-type wrappers) matches directly; a raw **box** `ж<X>` (or an adapter asserting to a
*different* interface, via its `Box`) re-wraps through `go.AdapterRegistry` — each generated
pointer adapter registers `(typeof(ж<X>), typeof(Iface)) → box => new XжIface(box)` through its package's
registration method, which a `[ModuleInitializer]` queues and golib runs on the registry's first lookup
that misses (see
[Run-time registrations wait for the first lookup that misses](source-generators.md#run-time-registrations-wait-for-the-first-lookup-that-misses)),
so the lookup is a dictionary hit and a compiled factory, reflection-free and Native-AOT-safe (the
queued registration also roots the adapter against trimming); an *anonymous*
interface falls back to its runtime duck-typing shell (built by golib's `AdapterBinder`)<!-- this said "its generated
`ᴛAs` duck-typing
conversion" until 2026-07-25, when anonymous interfaces moved onto the same shells named ones use -->.
The `out var x` binds at
the CASE interface type exactly as Go binds the case variable, the re-wrapped adapter forwards to
the original box (writes through the binding alias the original object), label order is preserved
(C# tests patterns top-to-bottom, and a `when`-guarded pattern never makes a later label
unreachable), and `case nil` is unaffected (`{}` never matches null). The type-assert core is
non-throwing (`TryTypeAssert`), so a non-matching label — the NORMAL control flow in a type
switch — costs no exception; this also makes a nil-interface `v, ok := x.(T)` return `ok=false`
(Go semantics) instead of faulting. A named interface with no registered adapter for the pair
resolves at run time through `AdapterBinder`, to the same duck-typing shell an anonymous interface uses (see
[Interfaces](interfaces.md#every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert)),
so the pairs the registry cannot hold still match by method set: a (struct, iface) pair with **no
conversion site anywhere** in the program, a **generic** struct's adapter (an open registration key
is unrepresentable), and FOREIGN **value**
adapters (`ᴠ`-composed), which do not register. Only a pair no shell can bind is a MISS
(`NamedInterfaceAdapterIdentity`'s `case Greeter:` matches a pair nothing converts at compile time).
(Guarded by the `TypeSwitchNamedInterfaceCase` behavioral test — pointer-adapter value, raw
box-in-`any`, value-struct implementer, `case error:` in both adapter-carried and raw-box forms,
non-matching control, label-order precedence both directions, a multi-type clause of two interface
labels, interface-tag-to-interface-label dispatch, and write-through aliasing, output-compared
vs Go.)

---

[← Expression Switch Statements](expression-switch.md) · [Index](README.md) · [Struct Types →](struct-types.md)
