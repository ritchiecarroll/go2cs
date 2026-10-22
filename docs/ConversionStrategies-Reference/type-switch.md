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

```csharp
internal static void @do(any i) {
    switch (i.type()) {
    case nint v: {
        fmt.Printf("Twice %v is %v\n"u8, v, v * 2);
        break;
    }
    case int32 v: {
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

**Go `int`/`uint` cases and the synthetic concrete case.** A Go `int` maps to C# `nint`. The converter boxes an untyped constant passed as an interface at `nint` (`do(1)` emits `@do((nint)1)`; see [Empty Interface](empty-interface.md#an-untyped-constant-boxed-as-any-boxes-at-gos-default-type)), but a C# `int` boxed into an interface by hand-written code has dynamic type `int32`, not `nint`. So a `case int:` emits its native form **plus** a synthetic concrete `case int32:` (and `case uint:` adds `case uint32:`) running the same Go body, to catch both boxings; a single-type case repeats the arm, as above, because its variable is bound at each label's own type. The exception: if the *same* switch also lists an explicit `case int32:`/`case uint32:` (or `case rune:` ≡ int32), the synthetic is **skipped** — emitting it would duplicate the explicit case (CS8120 "unreachable case") and, being emitted first, would steal the explicit case's values and run the wrong body. With the synthetic suppressed, a typed `int` value (`nint`) hits `case int:` and a typed `int32` value hits `case int32:`, distinctly. (Runtime's `printpanicval` switches over `int, int8, …, int32, …, uint, …, uint32, …`; guarded by the `TypeSwitch` behavioral test.)

**Duplicate-mapped cases — the identical-body merge.** Go type distinctions can *vanish* in the C# type map, making a later case unreachable (CS8120). The canonical example was `uint` + `uintptr` under the old `System.UIntPtr` alias — **now moot: `uintptr` is a distinct golib struct** (see [Constant Values](constants.md#constant-values)) and both cases emit their own labels, each dynamic type routing to its own body exactly as in Go. The merge machinery remains for any pair of Go types that still shares a C# type — today two methodless named func types with one signature, which both render as their base delegate (`type Left func(bool) bool` and `type Right func(bool) bool` are both `Func<bool, bool>`): a duplicate-mapped case merges **only when its Go body is byte-identical** to the first occurrence's — the earlier label already routes both dynamic types to that shared body, so the merge is exact. A marker comment replaces the duplicate label:

```csharp
switch (raw.type()) {
case Func<bool, bool>: {
    return "either";
}
/* case Func<bool, bool>: merged with an earlier case mapping to the same C# type (identical body) */}
```

If the bodies **differ**, both labels are kept and the CS8120 stands: a compile error is preferable to silently routing one Go case's values into another case's body. Duplicate detection keys on the resolved C# type (`rune`→`int32`, `byte`→`uint8`) per switch statement; the synthetic `int32`/`uint32` cases register too, so an explicit later duplicate of a synthetic with the same body also merges. Guarded by `TestTypeSwitchCollapsedFuncCase` (an identical-body pair that merges, a differing-body pair that keeps both labels, and two different delegates that never merge); the `TypeSwitch` behavioral test keeps `uint` + `uintptr` with different bodies, each value hitting its own case.

**Multi-type cases stack labels and bind at the tag's interface type.** Go binds a type-switch case
variable at the listed CONCRETE type only when the clause lists exactly **one** type; a multi-type clause
(`case *Alias, *Named:`) binds it at the TAG's static (interface) type. The old emission split such a
clause into one concrete-bound C# case per listed type (duplicating the body), so every body use in an
interface-typed context broke — as an argument (`isGeneric(t)` with `t` a `ж<Alias>`, CS1503), an
interface assignment (CS0266), and an extension-method receiver (CS1929 — 18 errors in go/types alone).
A multi-type clause now emits **stacked labels binding only a discard, over one shared body** and
re-binds the variable to the guard expression — the same re-bind the `default` arm uses — so the body
compiles at the interface type exactly as in Go:

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
CS8917 (encoding/binary's `Size`/`intDataSize` stacks; caught by the census build, not the behavioral
corpus, whose labels happened not to collide). `case nil` stacks as `case null:`, a dynamic-interface
label stacks in its non-binding `{} ᴛn when ᴛn._<Iface>(out var _)` form, and the synthetic
`int32`/`uint32` companions of `case int:`/`case uint:` stack with the same discard. The
duplicate-mapped-case merge applies per label (a merged label leaves its marker comment above the stack).
An UNBOUND multi-type clause (`switch x.(type)`) stacks the same way with no re-bind — the body is no
longer duplicated per label. The re-bind re-evaluates the guard EXPRESSION at body entry — harmless
for a pure tag (nothing can mutate it between dispatch and entry), and an IMPURE tag is hoisted into
a one-time temporary first (see *The type-switch tag evaluates exactly once* below). (Guarded by the `TypeSwitchMultiCase` behavioral test — bound multi-type
cases over values and pointers with interface-dispatched body uses, `nil` stacked with a concrete type, an
unbound multi-type clause, and the synthetic-int stacking, output-compared vs Go.)
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
pointee, and `case null:` does not.<!-- Earlier, a nil *T stayed wrapped (a null Box) and fell to `default`; AdapterImplTemplate's constructor now substitutes ж<T>.NilBox. --> (Guarded by
the `TypeSwitchPointerAdapter` behavioral test — pointer-receiver implementations of a non-empty
interface dispatched through single-type, multi-type, no-bind, and write-through cases, plus
value-receiver, `nil`, and raw-box-in-`any` controls; and the `TypeSwitchNilPointer` behavioral test — a nil
`*T` in a non-empty interface and in `any`, a binding and a multi-type case, and a pointer-receiver call on the
nil pointee.)

**Both unwrap tiers sit behind ONE `IGoAdapter` probe (2026-07-26).** `.type()` is evaluated once
per type switch, and it used to test `IInterfaceAdapter` and then `IжAdapter` separately — two
*failing* interface type tests for every ordinary Go value, which is what the overwhelming majority
of type switches dispatch on. Measured on windows/amd64 (.NET 9), that is ~2.9 ns each on the JIT,
against a failing sealed-class test too small to measure: a failing interface `isinst` walks the
type's interface map. The adapter interfaces (`IInterfaceAdapter`, `IжAdapter`, and the later
`IValueAdapter`) share an empty base marker, `IGoAdapter`, and `.type()` probes it once to
gate every unwrap, dropping the type switch from **16.0 ns to 4.5 ns per iteration** on `PerfIface`.
The unwrap ORDER and results are unchanged; the `string` → `@string` arm moved after the `IжAdapter`
arm, which cannot change an answer because `string` is sealed and implements neither marker. See
[Interfaces](interfaces.md#interfaces) for the same gate on the type-assert side and the full measurement.
<!-- Platform: the 2026-07-26 measurements were taken with .NET SDK 9.0.316 and a
win-x64 runtime target, per docs/phase4/DESIGN-iface-shell-caching.md §10. interfaces.md's decomposition
of the same campaign reports the type switch as 16.0 → 11.0 → 4.4 ns across two changes. -->

**Named-interface case labels dispatch by method set through the adapter registry.** An
INTERFACE-typed case label — named (`case fmt.Stringer:`, `case error:`) or anonymous — must match
by Go METHOD-SET semantics, and after the unwrap above the operand is the raw receiver box, which
never nominally implements a C# interface (the generated pointer adapter does). A plain C# type
pattern (`case Stringer t:`) therefore missed every pointer-sourced implementation. All interface
labels now emit the `when`-guard form the anonymous labels already used —
`case {} Δx when Δx._<Stringer>(out var x):` — routing dispatch through golib's type-assert
machinery, which resolves in order: a **nominal** implementer (value structs made partial, adapter
instances, duck-type wrappers) matches directly; a raw **box** `ж<X>` (or an adapter asserting to a
*different* interface, via its `Box`) re-wraps through `go.AdapterRegistry` — each generated
pointer adapter registers `(typeof(ж<X>), typeof(Iface)) → box => new XжIface(box)` from a
`[ModuleInitializer]`, so the lookup is a dictionary hit and a compiled factory, reflection-free
and Native-AOT-safe (the initializer also roots the adapter against trimming); an *anonymous*
interface falls back to its runtime duck-typing shell<!-- this said "its generated `ᴛAs` duck-typing
conversion" until 2026-07-25, when anonymous interfaces moved onto the same shells named ones use -->.
The `out var x` binds at
the CASE interface type exactly as Go binds the case variable, the re-wrapped adapter forwards to
the original box (writes through the binding alias the original object), label order is preserved
(C# tests patterns top-to-bottom, and a `when`-guarded pattern never makes a later label
unreachable), and `case nil` is unaffected (`{}` never matches null). The type-assert core is
non-throwing (`TryTypeAssert`), so a non-matching label — the NORMAL control flow in a type
switch — costs no exception; this also makes a nil-interface `v, ok := x.(T)` return `ok=false`
(Go semantics) instead of faulting. A named interface with no registered adapter for the pair
falls through to the same runtime duck-typing shell an anonymous interface uses (see
[Interfaces](interfaces.md#every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert)),
so the pairs the registry cannot hold still match by method set: a (struct, iface) pair with **no
conversion site anywhere** in the program, a **generic** struct's adapter (an open registration key
is unrepresentable and a generic class cannot host a module initializer), and FOREIGN **value**
adapters (`ᴠ`-composed), which do not register. Only a pair no shell can bind is a MISS
(`NamedInterfaceAdapterIdentity`'s `case Greeter:` matches a pair nothing converts at compile time).
(Guarded by the `TypeSwitchNamedInterfaceCase` behavioral test — pointer-adapter value, raw
box-in-`any`, value-struct implementer, `case error:` in both adapter-carried and raw-box forms,
non-matching control, label-order precedence both directions, a multi-type clause of two interface
labels, interface-tag-to-interface-label dispatch, and write-through aliasing, output-compared
vs Go.)

---

[← Expression Switch Statements](expression-switch.md) · [Index](README.md) · [Struct Types →](struct-types.md)
