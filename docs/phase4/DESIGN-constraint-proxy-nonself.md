# Constraint proxies without a self parameter (TS readiness C35)

> Design note for review before any code is cut (COORD, 2026-11-10 15:21Z). Seat branch `claude/r-ts-c35`,
> base master `38caf3558a`. Every claim below names the probe or census that measured it; the probes are
> single-package scratch programs converted with the master converter.

## 1. The defect

A generic whose type parameter is constrained by a **method-set interface that is not self-referential**,
called with a **pointer** type argument, does not compile:

```go
type optionParser interface { ParseOption(key string, value any) bool; UnknownOptionDiagnostic() string }
func convertMapToOptions[O optionParser](m map[string]any, keys []string, parser O) O { … }
p := convertMapToOptions(m, keys, &compilerOptionsParser{})   // tsc internal/tsoptions
fmt.Println(p.seen)                                              // the caller reads the CONCRETE type
```

The C# declaration carries `where O : optionParser`; the call is emitted bare, C# infers `O = ж<compilerOptionsParser>`,
and the golib box (a generic class) implements no user interface: **CS0311**. Measured at `38caf3558a`:

| Probe | Constraint | Type argument | Result |
|---|---|---|---|
| c35 probe | unexported local interface | `*T` | CS0311 ×2 |
| exported | exported local interface | `*T` | CS0311 ×2 |
| stringer | `fmt.Stringer` (foreign) | `*T` | CS0311 ×2 |
| valuearg (control) | unexported local interface | `T` (value receiver) | green |

So the class is general (the BOARD's 2026-10-16 C1 class), not about export.

## 2. What exists today, and why none of it covers C35

`constraintProxyFor` (constraintOperations.go) builds a **constraint proxy** only when (1) the argument is a pointer to a
named type, (2) the constraint is an INSTANTIATED generic method-set interface, and (3) it is SELF-REFERENTIAL
(`P nistPoint[P]`). C35's `O optionParser` fails (2) and (3).

Three other routes make std's own pointer-argument instantiations compile; the census (§6) found std's production reach
is exactly these, and each depends on WHERE the type parameter appears:

- **Elided constraint.** `errors.AsType[E error]` is emitted with no `where` clause (`AsType<ж<fs.PathError>>` binds).
- **Func-result projection** (`funcResultProjection`): a type parameter reached ONLY as a func result
  (`hkdf.Key[H hash.Hash](h func() H, …)`) renders as the constraint itself, and the delegate is projected through the
  pointer adapter: `Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, e => new sha256_DigestжHash(e)), …)`.
- **Element-wise widening** of a `[]N` argument (`go/ast walkList[N Node](v, list []N)`): `widen<ж<Ident>, Node>(n.Names, …)`
  copies the slice into adapters. A copy, which is exact only because walkList never writes the slice.

None of these can serve a type parameter that is a DIRECT value parameter and a RESULT: widening `O` to the interface
would compile the call, but the result would then be the interface, and the caller's `p.seen` reads a field of the
concrete type (Go returns `*compilerOptionsParser`). **Widening is rejected for that reason.**

## 3. The design: the existing proxy, with no self parameter

The self-referential proxy (go2cs-gen `ImplementGenerator.EmitConstraintProxy`, template
`ConstraintProxyImplTemplate`) is a sealed class that holds the element's box and implements the constraint:

```csharp
// The shape today's self-referential proxy has, written for C35's pair (the name is settled in 3.1).
internal sealed class compilerOptionsParserжoptionParserᴾ : optionParser, IжAdapter
{
    private readonly ж<compilerOptionsParser> m_box;
    public compilerOptionsParserжoptionParserᴾ(ж<compilerOptionsParser> box) => m_box = box;
    public object? Box => m_box;
    public static implicit operator compilerOptionsParserжoptionParserᴾ(ж<compilerOptionsParser> box) => new(box);
    public static implicit operator ж<compilerOptionsParser>(compilerOptionsParserжoptionParserᴾ proxy) => proxy.m_box;
    bool optionParser.ParseOption(@string key, any value) => m_box.ParseOption(key, value);
    …
    // Equals/GetHashCode: pointer identity over m_box (ReferenceEquals), as today.
}
```

With no self parameter there is nothing to marshal: every method forwards its arguments and result unchanged, and
nothing is refused. The pieces:

### 3.1 Generator (go2cs-gen)

- `EmitConstraintProxy` today returns unless the interface has exactly ONE type parameter, which it treats as the self
  parameter. Add the **zero-type-parameter** case: `InterfaceRef` is the interface itself (no closing over the proxy),
  `selfParameter` is null, and `RenderWithProxy`/`MarshalAcrossProxy`/`UnforwardablePositions` become identity for a
  null self (each already handles "type does not mention self" as identity; a null self mentions nowhere).
- Embedded interfaces forward exactly as today (`AllInterfaces`, each member qualified by its declaring interface).
- The record is the existing `[assembly: GoImplement<element, iface>(ConstraintProxy = true)]`, with a non-generic
  `iface` (today's record always closes `iface<placeholder>`).
- **The name cannot be today's proxy name.** The POINTER ADAPTER for a same-package element is named
  `struct-key + ж + interface` (ImplementGenerator line ~1319; go/ast emits `new IdentжNode(e)`), which is exactly
  `element-simple + ж + interface-simple`. A self-referential proxy never met this, because its interface is generic and
  the adapter for a generic interface is a different name; a non-generic one would collide with the adapter whenever the
  same pair also converts to the interface (CS0101). Two resolutions, for review:
  - **(a) Distinct proxy name (recommended):** `element + ж + interface + ᴾ` (a letter, valid in a C# identifier). The
    adapter, which sits on the hot path of every pointer-to-interface conversion in the corpus, is untouched; two classes
    for one pair coexist, and golib's type-assert machinery unwraps both through `IжAdapter.Box`.
  - (b) One class: give the pointer adapter the two implicit operators when the pair is proxied, and spell the adapter
    as the type argument. Fewer classes, but it couples the proxy to the adapter's `NilBox` substitution (a nil box
    becomes the canonical typed-nil box, so `returned == original` fails for a nil argument) and changes a template every
    interface conversion uses.
- A **generic but not self-referential** constraint (`O Parser[string]`, the census's OPEN/CLOSED classes) needs the
  interface closed over its CONCRETE arguments and a name that encodes them. The census found **zero** such production
  sites in std and four OPEN test-only sites (crypto/mlkem_test). **Proposal: out of scope for this seat, refused by name
  (§5)**, not silently left on the box.

### 3.2 Converter

- **Gate** (`constraintProxyFor`): admit a non-generic method-set constraint (`iface.NumMethods() > 0`,
  `IsMethodSet()`), still requiring a pointer to a named type. Record the proxy as today (local interface: this package
  records; foreign interface: the owner's published proxy when there is one, else this package records a concrete proxy
  over the foreign interface, which needs the interface to be accessible, the same rule as the pointer adapter).
- **Position gate, so existing routes stay byte-identical.** The proxy is used only when the type parameter appears as a
  DIRECT value position of the signature (a parameter or result of type exactly `O`). A type parameter reached only as a
  func result keeps funcResultProjection; one reached only inside `[]N` keeps the element-wise widening; an elided
  constraint stays elided. The census predicts this leaves every std production site unchanged (§6).
- **Call site**: spell the proxy as the type argument, exactly as the self-referential route does
  (`convertMapToOptions<compilerOptionsParserжoptionParserᴾ>(m, keys, box)` under 3.1(a)); the box converts implicitly at
  the argument.
- **Result round-trip (new for BOTH routes).** When the call's Go result type is exactly the proxied type parameter, the
  call is cast back to the box: `((ж<compilerOptionsParser>)convertMapToOptions<…>(…))`. The implicit operator returns
  the proxy's own `m_box`, so this is **the same pointer**, not a copy: a write through the result is a write through the
  caller's original pointer. A multi-value result (`(O, error)`) casts the tuple element at deconstruction. This also
  repairs a pre-existing gap measured in the SELF-REFERENTIAL route: `dup[T Cloner[T]](x T) T` then `dup(a).n` is CS1929
  today (`ImplжCloner` has no field `n`), so the self-referential route has never supported reading a field of its result.
  Including it here is what makes "the result round-trips" true for the whole proxy family; the nistec census rows (310
  test-side SELF sites) are the regression control, since none of them reads a field today (they compile).

## 4. Nil

Go's zero value of `O` (a pointer type) is nil, and a nil pointer is a valid receiver.

- **Box to proxy.** `implicit operator proxy(ж<E> box)` constructs a proxy even for a null box, so a nil argument is a
  non-null proxy whose `m_box` is null; forwarders call the box extension with a null box, which is Go's nil-receiver
  call. Unchanged from today.
- **Proxy to box.** `proxy.m_box` throws on a NULL proxy, and a null proxy is exactly what generic code produces for
  `var zero O; return zero` (`default(proxy)`). Go returns a nil `*E`. **Fix in the template, for both routes:** the
  operator becomes `proxy is null ? null! : proxy.m_box`. A red row must exercise `return zero` and compare the result
  with nil.
- **Not changed:** the pointer adapter's `NilBox` substitution (adapters are not type arguments, and this design does
  not make them so).

## 5. Refused by name

Each refusal is a converter diagnostic naming the call, the constraint and the type argument, as the self-referential
route's refusals already do (constraintProxyForwarding.go):

- a **generic, non-self-referential** constraint (`O Parser[string]`): out of scope (§3.1), refused rather than
  emitted as a CS0311;
- a constraint that embeds **`comparable`** (not a method set, so not admitted; `==` on proxies would need operator
  overloads the template does not have);
- a proxied type parameter in an **aliasing position of the CALLEE's signature that the call must convert**: a `[]O`,
  `map[K]O`, `chan O` or `*O` parameter or result whose argument is a `[]*E` (and so on). A converted copy would
  compile and silently break Go's aliasing; the existing widening copies `[]N` only for go/ast's read-only walkList,
  and this design does not extend that. (A `func(O)` position re-wraps exactly, as the self-referential route does.)

## 6. Census (std, 3 targets) and prediction

Scratch census through go/packages, never committed: every generic instantiation (functions and named types) whose type
argument is a pointer to a named type and whose type parameter's constraint is a method-set interface with at least one
method. Positive controls: the four probes above (NONGEN 1 each) and a self-referential probe (SELF 2), all exact.

| | NONGEN | OPEN | SELF |
|---|---|---|---|
| production, windows / linux / darwin | 15 / 15 / 15 | 0 | 4 / 4 / 4 |
| test files, windows / linux / darwin | 30 / 33 / 33 | 4 | 310 / 310 / 310 |

The 15 production NONGEN sites are `hash.Hash` ×9 (func-result projection), `go/ast.Node` ×4 (`[]N` widening) and `error`
×2 (elided): all on the routes §3.2 keeps. **Prediction to be posted with the cut, from this census:** std production
footprint 0 / 0 / 0; the result cast touches no std production site (none reads a field of a proxied result, since they
compile today); CNR byte-identical except the new red-first project(s).

## 7. The red that must run (COORD condition 2)

`NamedConstraintPointerArg` (name provisional): call through the constraint, return `O`, read a field of the concrete
type, **mutate through the returned pointer and observe the write through the caller's original pointer** (pointer
identity, `returned == original`), return the zero `O` and compare it with nil, a foreign constraint (`fmt.Stringer`), an
embedded constraint interface, a `func(O)` callback parameter, a value-argument control, and the self-referential
result-field row from §3.2. Each row prints, so the Output phase compares C# with Go.

## 8. Gates

Red first; GenTests (generator change, route #7: the golib/gen twins); GolibTests is not needed unless the template
change reaches golib (it does not: the template is gen-owned); the two-seeded three-target footprint; CNR; `go test ./...`;
the package-compat line (no golib public surface touched).
