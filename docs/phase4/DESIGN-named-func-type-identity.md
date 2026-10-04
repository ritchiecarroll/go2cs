# DESIGN — named func type identity: the collapse rule, and what a type switch on it should compile to

> **Status: PROPOSAL (lane R, 2026-10-03), commissioned by COORD as mapstructure's class B2.** No
> converter change rides this note. It states today's rule and why it exists, what the rule costs at
> the one boundary where Go observes a func type's NAME, three options, and a census of where that
> boundary is reached in GOROOT and in the outside modules lane R holds. Measured at master
> `8f46a9adae`.
>
> Related: [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md) (the atlas
> batch); [`../ConversionStrategies-Reference.md`](../ConversionStrategies-Reference.md) (how each
> Go construct maps).

## 1. The defect that commissioned it

mapstructure v1.5.0's `DecodeHookExec` dispatches on three named func types:

```go
type DecodeHookFuncType func(reflect.Type, reflect.Type, interface{}) (interface{}, error)
type DecodeHookFuncKind func(reflect.Kind, reflect.Kind, interface{}) (interface{}, error)
type DecodeHookFuncValue func(from reflect.Value, to reflect.Value) (interface{}, error)

switch f := typedDecodeHook(raw).(type) {
case DecodeHookFuncType:  ...
case DecodeHookFuncKind:  ...
case DecodeHookFuncValue: ...
}
```

The emission names the Go type in each case (`case DecodeHookFuncType f:`), but the type was never
declared — its declaration emits only `// type DecodeHookFuncType is a methodless func type —
rendered inline as its base delegate`. Three CS0246, the package's other 3 of 43 build errors (the 40
are class B1, seated separately). A two-case repro (`B2Switch`, held locally) reproduces it
identically at master and on the A1+A3 merge.

## 2. Today's rule, and why it exists

`methodlessNamedFuncSignature` (typeNameResolution.go) collapses a named func type to its base
delegate (`Func<…>` / `Action<…>`) everywhere, and skips its declaration, when the type is:

- **methodless** — a type WITH methods keeps a distinct delegate (its method set is meaningful), and
  the generator emits a value adapter where it meets an interface (flag's `funcValueᴠValue`);
- **non-generic** — a generic one keeps its name (`Seq<V>`), the type parameter must stay in scope;
- **a leaf** — its signature names no other named func type (a self-referential `type stateFn
  func(*machine) stateFn` has no finite base-delegate form).

The reason is assignability. Go lets a value move between a named func type and its unnamed
underlying type with no conversion (`database/sql`'s `grabConn` returns `releaseConn`, `queryDC`
takes `func(error)`). C# delegate types are NOMINAL, are not interconvertible, and cannot declare a
user-defined conversion; a distinct `ΔreleaseConn` delegate made every such flow CS1503 / CS0029.
Collapsing makes those flows identities, exactly as Go types them statically.

**The rule is already applied to type ASSERTIONS** — the corpus emits archive/zip's
`ci.(Compressor)` as `ci._<Func<io.Writer, (io.WriteCloser, error)>>()` — **but not to type-switch
CASES**, which still spell the Go name. That inconsistency is the whole of B2's compile error.

## 3. What the rule costs

A named type is a distinct type in Go's DYNAMIC typing, and the collapse erases it. Every observable
of that identity reads differently once a value is stored in an interface (`B2Reflect`, held
locally, converted at the A1+A3 merge):

| Go program | Go | converted C# today |
|---|---|---|
| `fmt.Printf("%T", any(H(f)))` | `main.H` | `func(int) int` |
| `reflect.TypeOf(any(H(f))).Name()` | `H` | *(the empty string)* |
| `reflect.TypeOf(H(f)) == reflect.TypeOf(f)` | `false` | `true` |
| `_, ok := any(H(f)).(func(int) int)` | `false` | `true` |

And a switch that names two collapsed types with the SAME signature, or one collapsed type beside its
unnamed underlying type, distinguishes them in Go (`B2Collide`: `A B unnamed`) and cannot in C#,
where the cases become one delegate type.

## 4. Census

**Instrument.** A go/packages walk (x/tools v0.42.0) with `Tests: true` and the corpus build tags
(`purego,math_big_pure_go`), whose predicate is `methodlessNamedFuncSignature` at `8f46a9adae`
transcribed. Three columns: a type-switch **case** naming a collapsed type (with any other case in the
same switch whose underlying type is identical — a **collision**); a type **assertion** to one; and a
**box** — a value of a collapsed type flowing into an interface slot (argument, return, assignment,
declared variable, conversion, composite element, struct field, send), the boundary where the
identity becomes observable. Boxes are keyed per line and type, so two same-type boxes on one line
count once: the box column is a lower bound.

**Controls.** `B2Switch` reads 2 cases; mapstructure reads its 3 cases at `decode_hooks.go:46/48/50`;
`B2Collide` reads both planted collisions, its assertion and both of its boxes.

| Population | packages | collapsed types declared | cases | collisions | asserts | boxes |
|---|---:|---:|---:|---:|---:|---:|
| GOROOT std, windows/amd64 | 924 | 70 | **0** | 0 | 4 | 10 |
| GOROOT std, linux/amd64 | 926 | 73 | **0** | 0 | 4 | 10 |
| GOROOT std, darwin/amd64 | 923 | 70 | **0** | 0 | 4 | 10 |
| mitchellh/mapstructure v1.5.0 | 3 | 3 | **3** | 0 | 0 | 4 |
| gorilla/mux v1.8.1 | 4 | 2 | 0 | 0 | 0 | 0 |
| golang-jwt/jwt/v5 v5.3.1 | 9 | 4 | 0 | 0 | 0 | 0 |
| joho/godotenv v1.5.1 | 5 | 0 | 0 | 0 | 0 | 0 |
| dustin/go-humanize v1.1.0 | 7 | 0 | 0 | 0 | 0 | 0 |
| pkg/errors v0.9.1 | 1 | — | — | — | — | — |

pkg/errors has no go.mod and loads with one error here, so its row is NOT MEASURED, not zero. The std
hit set is identical on all three GOOS. Its 14 sites are three shapes, all `sync.Map` caches plus one
test table:

- archive/zip `register.go`: `compressors.Store(Deflate, Compressor(func(…) {…}))` and
  `LoadOrStore(method, comp)` / `ci.(Compressor)` (and the Decompressor twin) — 6 boxes, 2 asserts. Store and load are the same type, so the erased
  name is never consulted.
- encoding/json `encode.go`: `encoderCache.LoadOrStore(t, encoderFunc(…))` / `.(encoderFunc)` —
  2 boxes, 2 asserts, same shape.
- reflect `all_test.go:4539-4540`: TestConvert's `func()` ↔ `MyFunc` pair — 2 boxes. **The only std
  site that observes the identity**: it converts between a named func type and its unnamed
  underlying type through reflect, which the collapse makes the same type.

**Reading.** No std package has a type switch on a collapsed type; mapstructure's three are the
whole measured population, and none of them collide. Every other std use stores and loads ONE type
through an interface, where the erasure is invisible.

## 5. Options

**A — apply the rule at the case.** Emit a case on a collapsed type as its base delegate, as the
assertion path already does: `case Func<reflect.Type, reflect.Type, any, (any, error)> f:`. One
predicate in the type-switch case emission; the declaration comment already names the Go type for
the reader, and the case could carry a trailing `/* DecodeHookFuncType */`.
*Cost to readers:* a long delegate where Go wrote a name — the same trade every other use of these
types already makes. *Semantics:* identical to the assertion path; the §3 losses stay. A colliding
switch becomes a C# compile error (a case subsumed by an earlier one), so the unfaithful case is LOUD,
not silent. *Footprint:* 0 std sites (census), mapstructure's 3.

**B — a nominal wrapper type.** Declare the named type as a `[GoType]` struct holding the delegate,
with implicit conversions to and from the base delegate — the treatment named non-func types already
get. Faithful everywhere. *Cost:* C# applies no user-defined conversion to a lambda or a method group,
so every literal and method group assigned to such a type needs an explicit `new Func<…>(…)`, and every
call through a value needs an invoke spelling; this is the churn the collapse exists to remove, across
all 70+ declared std types and all of their uses, not just the 14 boundary sites.

**C — tag at the interface boundary.** Keep the collapse wherever a value is statically typed, and
attach the name only where Go can observe it: a box into an interface emits a generated sealed tag
(`new DecodeHookFuncTypeᴛ(f)`) carrying the delegate and the Go type descriptor. A case or assertion
on the name matches the tag and unwraps it; one on the unnamed func type matches only an untagged
delegate; reflect and `%T` read the tag's `[GoType]`. It is the same pattern as the value adapter the
generator already emits for a named func type WITH methods. Faithful for every observable in §3.
*Cost to readers:* confined to the box sites (census: 10 std, 4 mapstructure) and the unwrap in
case/assertion emission. *Cost to build:* a generator-emitted tag per boxed collapsed type, and the
unwrap in the assertion helper.

## 6. Recommendation

**A now, C if a consumer needs identity.** A closes B2 with a change to the one path that disagrees
with the existing rule, has no std footprint, and fails loudly on the one shape it cannot represent.
The identity loss is not new: it is already the behaviour of every assertion on these types. C is the
faithful design and the census sizes it small; its first named consumer would be reflect's TestConvert
pair, and it should wait for that consumer. B is not recommended — it pays the full cost of the
problem the collapse solved to fix a boundary that C reaches at a fraction of the sites.

<!-- Evidence, 2026-10-03, lane R on R-LAPTOP. Census tool and its control probes (B2Switch, B2Collide,
     B2Reflect) held locally in the lane's scratch tree, not committed. The predicate was transcribed from
     typeNameResolution.go methodlessNamedFuncSignature / signatureReferencesNamedFuncType at 8f46a9adae.
     std was loaded with `go/packages` pattern `std`, Tests: true, GOOS=windows|linux|darwin GOARCH=amd64,
     -tags=purego,math_big_pure_go; loadErrors=0 on every std leg. The B2Reflect C# column was read from a
     conversion and run at the A1+A3 merge (b95dd8d6e6 + bad4c4c8a5 over 8f46a9adae); neither seat touches
     the collapse rule. -->
