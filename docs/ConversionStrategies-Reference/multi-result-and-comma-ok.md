# Multi-Result Values and Comma-Ok Forms

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#multi-result-values-and-comma-ok-forms)
Many Go functions return either a single value or a "value, ok"/"value, error" tuple, where only the declared return arity selects the behavior. You cannot differentiate C# overloads by return type alone, so the runtime types expose a second overload distinguished by an extra discard argument. For map access, the "comma-ok" read routes through a two-value indexer using the discard sentinel `ꟷ`:

```csharp
var v1 = m["Answer"];            // single value: zero value if the key is absent
var (v2, ok) = m["Answer", ꟷ];   // comma-ok: (value, present?)
```

These two forms can behave differently — case in point, [type assertions](https://golang.org/ref/spec#Type_assertions): the single-value form panics on failure, while the comma-ok form returns safely with a boolean success result. Type assertions convert similarly, through a generated `_<T>()` accessor:

```csharp
var t = i._<MyType>();              // panics on failure
var (t, ok) = i._<MyType>(ᐧ);       // comma-ok, safe
```

The types that support these tuple-returns are defined in the [`golib`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/golib) library; ordinary user-code tuple returns convert as normal C# tuples without special handling.

**A package-level `var a, b = f()` reads ValueTuple components.** C# static field initializers cannot deconstruct a tuple, so the per-name field emission assigned the WHOLE result tuple to the first field (CS0029 — edwards25519's `var identity, _ = new(Point).SetBytes(…)`). With exactly one non-blank name the component read is appended to the inline call (`internal static ж<Point> identity = …SetBytes(…).Item1;` — blank names keep their uninitialized `_ᴛNʗ` fields, and the call still runs once). With two or more non-blank names the call is evaluated ONCE into a hidden tuple field and each name reads its component (`internal static (nint, @string) tupleᴛ1ʗ = pair(); internal static nint n = tupleᴛ1ʗ.Item1;` — C# static initializers run in textual order, so the reads follow the temp). Gated to package scope, no explicit type, one call initializer typed as a tuple; in-function `var x, y = f()` keeps the existing path. (Guarded by the `GlobalTupleVarDecl` behavioral test — both shapes plus a call-count probe proving single evaluation, output-compared vs Go.)

## A grouped var spec with one multi-result call deconstructs
A grouped `var (name, offset, abs = t.locabs() ...)` spec is not a `:=`, so the assignment tuple machinery never saw it -- the per-name path assigned the WHOLE result tuple to the first name and silently DEFAULTED the rest (time appendFormat read a zero abs; a silent-wrongness class beyond the CS0029 that exposed it). Function-local specs now emit the C# tuple deconstruction, matching the `:=` form; package-level specs use the once-evaluated hidden-field component reads:
```csharp
var (ln, ls) = pair();
```
Guarded by `GlobalTupleVarDecl` (both levels, with a call-count check proving single evaluation).

**The function-local gate asks whether a name has a BOX, not whether it "escapes" (2026-07-31).**
That branch is gated to specs no name of which needs a `ref heap<T>` box declaration, and it read the
raw `identEscapesHeap` flag — which the escape analysis **blanket-sets** for every *inherently*
heap-allocated local (pointer, slice, map, chan, **interface**, **func**), because those are already
references and get no box unless their address is genuinely taken. So the gate rejected specs that are
entirely plain, and every tuple with an interface or func result fell back to the very per-name path
this branch exists to replace:

```csharp
context.Context ctx = context.WithCancel(context.Background());   // the WHOLE tuple  (CS0029)
Action cancel = default!;                                          // silently defaulted
```

`identHasHeapBox` is the predicate that answers the gate's actual question, and it is what the branch
now calls. This is the trap `paramAddressTakenNeedsBox` already documents from the other side — *a
verdict the box gate then refuses leaves `identEscapesHeap` set with no box behind it* — and it stayed
hidden because `(int, string)`-shaped tuples, the ones anyone reaches for when probing, work fine.
net's `var ctx, cancel = context.WithCancel(context.Background())` is the corpus site. (Guarded by the
`GlobalTupleVarDecl` extension — a local `var si, fi = ifaceAndFunc()` returning an interface and a
func, both read back.)

## A forwarded multi-value call deconstructs when tuple elements need interface conversion
`return newRawConn(f)` forwards a `(*rawConn, error)` tuple into a `(syscall.RawConn, error)`
result list — C# tuple conversions do not consult user conversions element-wise (CS0266). The
converter deconstructs into temps and converts each element through the usual interface
machinery (which also records the `GoImplement` pairing):

```csharp
var (ᴛ1, ᴛ2) = makeRelay();
return (new relayжReporter(ᴛ1), ᴛ2);
```

Elements whose actual type is itself an interface are left alone (structural inheritance
covers those). Guarded by `CrossPkgUser` (`getReporter` forwarding `makeRelay`).

## A tuple deconstruction into INTERFACE variables hoists the call

**A tuple deconstruction into INTERFACE variables hoists the call when a component needs converting.** Reassigning a multi-value call into pre-declared interface locals (`c, err = sd.dialTCP(…)` with `var c Conn`) can require a per-component interface conversion C#'s tuple assignment cannot perform implicitly — a `ж<TCPConn>` component satisfies `Conn` only through its generated pointer adapter, an *explicit* conversion (CS0266 ×11 in net's dial.go). Mirroring the return-statement tuple arm, the call is hoisted into temp markers and each component converts in a tuple literal:

```csharp
var (ᴛ1, ᴛ2) = Ꮡsd.dialTCP(ctx, laΔ1, raΔ1);
(c, err) = (new TCPConnжConn(ᴛ1), ᴛ2);
```

The arm fires only for a statement-position deconstruction (one call RHS, several LHS) where some non-empty-interface target's tuple component is a non-identical, non-interface type; all other deconstructions keep the direct form. (Guarded by the `InterfaceCasting` extension `makeCounter` — a `(*Counter, error)` call deconstructed into an `Incrementer` — runtime-verified against Go.)

## A multi-value call spread into a call's parameters in an assignment hoists into temps
Go lets a MULTI-VALUE call fill the parameters of an enclosing call — `r := t.newRange(t.parseControl("range"))`,
where `parseControl` returns five values feeding `newRange`'s five parameters. C# has no splat, so the inner
call is deconstructed into markers and passed expanded:

```csharp
var (ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10) = Ꮡt.parseControl("range"u8);
var r = Ꮡt.newRange(ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10);
```

`convExprList` already performs this expansion, but only when the call's `deferredDecls` hoist target is
non-nil — passing the whole tuple as one argument is otherwise CS7036 (text/template/parse's `rangeControl`).
The return-form threads that target (visitReturnStmt); the assignment forms do too, on BOTH lowering
branches: the single-declare block and the mixed/escaping block (a pointer-result local that is heap-boxed is
not counted in `declaredCount`, so it takes the latter — the `newRange` case above). A **statement-level**
`f(g())` (a bare expression statement, not an assignment) carries no `deferredDecls` of its own, so the
expansion now falls back to the enclosing `ExprStmt`'s `v.hoistedDecls` buffer — testing's
`registerCover2(deps.InitRuntimeCoverage())`, where `InitRuntimeCoverage` returns three values:

```csharp
var (ᴛ1, ᴛ2, ᴛ3) = deps.InitRuntimeCoverage();
registerCover2(ᴛ1, ᴛ2, ᴛ3);
```

The hoisted `var (…) = …;` lands in the statement's existing hoist buffer, emitted before the statement.
Byte-identical corpus-wide except where the pattern occurs (and a harmless renumber of any later temps, since
the per-file marker index is monotonic). Guarded by `TupleSpreadIntoCall` (a value result, an escaping pointer
result, and a statement-level spread).

A **PACKAGE-LEVEL var initializer** has no statement sink at all — `var debug = template.Must(
template.New("RPC debug").Parse(debugText))` (net/rpc debug.go; also internal/trace/traceviewer) passed the
whole `(ж<Template>, error)` tuple as `Must`'s one argument (CS7036). There the spill becomes a hidden
once-evaluated static tuple FIELD (`v.globalDeclHoist`, flushed by visitValueSpec before the var's own
field — C# static field initializers run in textual order, the same holder shape `visitPackageTupleVarSpec`
emits for `var a, b = f()`), and the arguments read its components:

```csharp
internal static (nint, nint) tupleᴛ1ʗ = parts();
internal static nint g = combine(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);
```

Guarded by the `TupleSpreadIntoCall` extension (a package-level `var` spreading a two-value call into a
wrapping call, value read back in main).

---

[← Short Variable Redeclaration (Shadowing)](shadowing.md) · [Index](README.md) · [Slices and Arrays →](slices-and-arrays.md)
