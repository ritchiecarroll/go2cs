# Delegates to Value Receiver Instances

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#delegates-to-value-receiver-instances)

**A Go METHOD EXPRESSION** — `(*timers).run`, the *unbound* method as a func value whose first parameter is the receiver (runtime `time.go`'s `abi.FuncPCABIInternal((*timers).run)`) — selects a method off a **type**. Emitting the selector naively renders the type in value position (`(ж<timers>).run` — CS0119 + CS1503). Go types the expression as the func signature with the receiver prepended, so the converter renders that signature as the concrete delegate type and casts the method's static form to it: `(Func<ж<timers>, int64, int64>)(run)`. For a `[GoRecv]` method the `RecvGenerator`'s ж-overload matches the delegate exactly; a value-receiver method expression (`counter.get`) casts to its value-typed delegate (`(Func<counter, nint>)(get)`); a direct-ж method's primary form matches directly. (Guarded by the `MethodExpression` behavioral test — pointer- and value-receiver method expressions assigned, passed inline, and *invoked*, with mutations accumulating through the receiver box, values vs Go.)

A method expression on a **FOREIGN type** — `(*http.Request).Write` (net/http/httputil persist.go), or `(*Reader).ReadBytes` in an EXTERNAL test (`package bufio_test`) that dot-imports `bufio` — must additionally **qualify** the method's static form: the `[GoRecv]`/extension static (and its `RecvGenerator` ж-overload) lives in the *defining* package's class, so the bare name is CS0103 (a `using static` imports an extension method only for `recv.M()` invocation, never as a bare method group — so `(Func<…>)(ReadBytes)` cannot bind it; a same-named local method would instead mis-bind, CS0123). The qualifier is derived from the method's OWN package via `go/types` (`importQualifier(obj.Pkg().Name())`, with the file's import-alias override) — identical to how `getAliasQualifiedTypeName` qualifies the receiver type inside the delegate (`bufio.Reader`) — rather than by peeling the Go source spelling: a **dot-imported** type is a BARE ident (`Reader`), not a `pkg.T` selector, so the old source-peel silently dropped the qualifier and emitted the bare name. The result is `(Func<ж<http.Request>, io.Writer, error>)(http.Write)` / `(Func<ж<bufio.Reader>, byte, (slice<byte>, error)>)(bufio.ReadBytes)`. A same-package method expression keeps the bare name — the static is in scope — so existing emissions are unchanged. (Guarded by the `CrossPkgUser` extension — pointer-receiver `(*CrossPkgLib.Sensor).Calibrate` (write observed through the original receiver) plus value-receiver `Sensor.Hot` / `Celsius.Add` foreign method expressions, each invoked through its func value, output-compared vs Go — and by the `MethodExprDotImport` behavioral test, which dot-imports a sibling package and uses `(*Reader).Read` / `(*Reader).Peek` as func values; the pre-fix converter fails it with bare `(Read)`/`(Peek)` — CS0103 — exactly the bufio `TestUnreadByteOthers` failure.)

The **bound method value** — `d.compute = metricReader(read).compute` (runtime `metrics.go`), `types.MethodVal` used as a *value* — forwards through a lambda that captures the receiver expression and carries the **method's own parameters**, explicitly typed: `(ж<statAggregate> p1, ж<metricValue> p2) => ((metricReader)read).compute(p1, p2)`. The previous emission hardcoded arity zero (`() => x.m()`), mismatching any non-nullary target delegate (CS1593). One documented divergence: the receiver expression is evaluated *inside* the lambda (per call), where Go binds it once at method-value creation — acceptable for the compile milestone and the simple receivers observed. (Guarded by the `MethodExpression` extension — a bound `c.add` invoked repeatedly, mutations accumulating through the bound receiver, values vs Go.)

An **INTERFACE-receiver** method value in assignment context is exempt from that lambda: an interface method is a genuine C# instance method, so a plain method **group** over the evaluated receiver expression both compiles and matches Go's bind-once semantics exactly — `f = conf.Sizes.Alignof` (go/types sizes.go, `conf` the `ref` receiver) emits `f = conf.Sizes.Alignof;`, evaluating `conf.Sizes` once at delegate creation. The synthesized lambda there was doubly wrong: it re-evaluated the receiver per call *and* captured `conf` — capturing a `ref` receiver is CS1628. This mirrors the value-context rule below, which already leaves interface receivers on the plain emission; whole-stdlib footprint of the change: sizes.cs ×6, database/sql convert.cs, debug/buildinfo, net/http h2_bundle — every hunk a lambda collapsing to its method group. (Guarded by the `IfaceFieldMethodValueBind` behavioral test — a method value on an interface field of a pointer receiver with the field REBOUND after the value is taken, proving bind-once, output-compared vs Go.)

A **POINTER-receiver method value in a value context** — passed as a call argument rather than assigned: `s.nonDefaultOnce.Do(s.register)`, `registerMetric(…, s.nonDefault.Load)` (internal/godebug) — cannot use the bare selector: the `[GoRecv]` emission is an extension method whose first parameter is a **value type**, and C# cannot create a delegate from that (CS1113/CS1061). Go binds the receiver **address** once at method-value creation (`s.register` ≡ `(&s).register`), so the converter emits exactly that binding as a **box-bound method group** over the `RecvGenerator`'s ж-overload (class-typed, delegate-legal): `Ꮡs.register` for the receiver itself, `Ꮡs.of(Setting.ᏑnonDefault).Load` for a receiver value-field chain (the `&x.field` machinery renders the real field box). Unlike the assignment-context lambda above, this form matches Go's bind-once semantics exactly. A method whose body contains such a method value on its own receiver (or a value-field chain of it) is promoted to **direct-ж** by the capture-mode pre-pass (`bodyHasPointerMethodValueOnReceiver`) so the receiver box `Ꮡrecv` exists in scope. (Guarded by the `ReceiverFieldMethodCall` extension — method values on the receiver, on a receiver value field, and on a boxed local's field, passed as func values and invoked with mutations landing on the real storage, values vs Go.)

The **VALUE-receiver** analog captures rather than binds. When a value-receiver method value roots at the enclosing method's receiver — `kdf.hash.New` (crypto/internal/hpke's `hkdfKDF`, whose `hash` field is a `crypto.Hash` and `New` a value-receiver method; also crypto/tls's `c.hash.New`) — the emitted method is an extension over a **value** receiver, which likewise has no C# delegate (CS1113), so the converter synthesizes a wrapping lambda carrying the method's own parameters: `() => kdf.hash.New()`. But that lambda **captures the receiver**, and a non-direct-ж pointer-receiver method renders `this ref hkdfKDF kdf` whose `ref var kdf = ref Ꮡkdf.Value` alias cannot be captured by a C# closure (**CS1628** — "cannot use ref/in/out parameter inside a lambda"). So a method whose body contains such a method value is promoted to **direct-ж** by the capture-mode pre-pass (`bodyCapturesReceiverInValueMethodValue`, the value-receiver sibling of `bodyHasPointerMethodValueOnReceiver`), giving it a receiver box `Ꮡkdf` that the synthesized lambda references as a capturable reference: `() => Ꮡkdf.Value.hash.New()`. Two supporting pieces make the receiver render through its box inside the *synthesized* lambda (which has no `*ast.FuncLit` node): the capture-analysis walk now marks the **field-chain root** receiver box-ref, not only a bare-ident receiver (`kdf.hash.New` roots at `kdf`, not a bare ident); and the value-receiver synthesis renders the receiver expression in a lambda-conversion context (`conversionInLambda`) so `convIdent` emits the `Ꮡkdf.Value` box form. Same documented divergence as the other method-value forms — the receiver expression re-evaluates *inside* the lambda (per call), where Go's value-receiver method value binds a copy of it once at creation; acceptable for the compile milestone (the closure sees the same receiver instance, matching the pointer-receiver semantics of the enclosing method). This also cleared the identical latent CS1628 in crypto/tls's `key_schedule.cs` (`expandLabel`/`extract`/`finishedHash` each pass `c.hash.New` to `hkdf`/`hmac`, and the direct-ж fixpoint promoted their callers `deriveSecret`/`trafficKey`/… with call sites adapting to `Ꮡc.expandLabel` / `Ꮡsuite.trafficKey`). (Guarded by the `ReceiverCapturedInClosure` extension — a pointer-receiver method capturing its receiver through a value-receiver method value on a value field-chain (`w.id.render`) and on the bare receiver (`w.tag`), alongside the pre-existing func-literal capture, all invoked and output-compared vs Go; whole-stdlib reconvert diff: exactly hpke + the two crypto/tls files changed, nothing else.)

The **go-statement sibling** of the receiver-capture family: a `go` statement calling a **value-returning** method through the enclosing method's pointer receiver — `go q.conn.HandshakeContext(ctx)` inside `func (q *QUICConn) Start` (crypto/tls quic.go, CS1628) — is FORCED into the synthesized-lambda emission because `goǃ` has only void `Action` overloads (the x/net/nettest CS0407 form): `goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx)`. That lambda references the receiver exactly like the method-value cases above, but neither closure predicate sees it — there is no `*ast.FuncLit` and no method-VALUE expression, only a go-call whose lowering *will* synthesize one. The capture-mode pre-pass therefore also promotes on `bodyHasGoStmtLambdaCapturingReceiver`, which mirrors `visitGoStmt`'s lambda-form decision (a nullary call synthesizes a lambda only for a value-returning or named-func-type callee; a call with arguments does so when the callee returns a value or the arity mismatches — variadic never matches) and fires when the CALLEE expression references the receiver (arguments render outside the lambda, as `goǃ` call arguments). With the method direct-ж, the go-stmt capture analysis' existing box-ref marking of the receiver (`varIsDerefdPointerParam`) takes effect and the lambda renders the chain through the box: `goǃ(ᴛ1 => Ꮡq.Value.conn.HandshakeContext(ᴛ1), ctx)`. The method-group emissions are excluded and unchanged — a void matching-arity callee (os/exec's `go c.watchCtx(resultc)`) binds the receiver chain at delegate-creation time, outside any lambda; a `defer` sibling needs no equivalent because any function-level defer already promotes via `bodyWrappedInDeferContext`. **Known divergence (Phase-4 item):** the synthesized lambda reads the receiver chain (`Ꮡq.Value.conn`) at **goroutine-run time**, whereas Go evaluates the method-value receiver at **go-statement time** — `go q.conn.M(x); q.conn = other` deterministically calls the OLD conn in Go but races toward the NEW one here (arguments are statement-time in both). The same lazy-chain window already exists for every synthesized go-lambda over a non-receiver chain (the CS0407 discard form); the faithful fix for the whole class is hoisting the receiver-chain prefix into a statement-time temp before the lambda, which would also avoid the direct-ж signature flip — direct-ж was chosen here for machinery reuse under the compile-first milestone. (Guarded by `GoStmtReceiverLambda` — `go e.tally.bump(delta)` (argument arm) and `go e.tally.report()` (nullary arm), both value-returning through a pointer field of the receiver, with the goroutine's writes read back through the original receiver chain, output-compared vs Go.)

A conversion to a **named func type** — `metricReader(read)` where `type metricReader func() uint64` — targets a C# **delegate declaration** (`internal delegate uint64 metricReader();`). Distinct delegate types have no cast conversion (a `(metricReader)read` from `Func<ulong>` is CS0030); C# converts via **delegate creation**: `new metricReader(read)`, which accepts a compatible delegate or method group. The general conversion branch special-cases a named target whose underlying is a `*types.Signature`. Composed with the bound-value lambda this renders the full runtime `metrics.go` registration: `d.compute = (ж<statAggregate> p1, ж<metricValue> p2) => new metricReader(read).compute(p1, p2)`. (Guarded by the `MethodExpression` extension — a named-func-type conversion with a bound method invoked through a func field, values vs Go.)

A **GENERIC defined function type** — Go 1.23 `iter`'s `type Seq2[K, V any] func(yield func(K, V) bool)` — emits a **generic delegate**: `public delegate void Seq2<K, V>(Func<K, V, bool> yield);`. Two converter details make this work: the type parameters live on the NAMED type, not the `*ast.FuncType`'s signature, so the delegate declaration derives its generic definition from the TypeSpec's defined type (as the struct/array paths do — deriving from the signature emitted a non-generic `Seq2` whose `K`/`V` were undefined, CS0246/CS0308); and a **conversion to a generic instantiation** (`Seq2Like[string, int](fn)`) peels `IndexListExpr` (multi-parameter — the single-parameter `IndexExpr` already peeled) and resolves the *instantiated* target from the Fun expression's type (the TypeName resolves to the uninstantiated generic, against which convertibility fails), then routes through the same delegate-creation form: `new Seq2Like<@string, nint>((@string k, nint v) => …)`. The instantiated-target override is gated to uninstantiated-generic named targets so pointer conversions (`(*uint64)(p)`, whose Fun type is the full `*T` with the `*` re-applied separately) are untouched. (Guarded by the `GenericTypeInstantiation` extension — a generic defined func type declared, instantiated with two type arguments, and invoked both through a generic function and directly, values vs Go; clears the `iter` package's five wave-1 errors.)

In Go a function is a value; a value-receiver method can be assigned to a variable, and the variable captures **its own copy** of the receiver value at the moment of assignment. This surprises many non-Go programmers:

```go
package main

import "fmt"

type data struct {
    name string
}

func (d data) printName() {
    fmt.Println("Name =", d.name)
}

func main() {
    d := data{name: "James"}
    f1 := d.printName
    f1()
    d.name = "Gretchen"
    f1()
}
```
This prints `Name = James` twice ([run it](https://play.golang.org/p/d-A5re1dfs8)) — `f1` bound a copy of `d`, so the later mutation is not observed. To preserve this semantic, the converter copies the receiver value into the delegate's capture rather than capturing the variable by reference, so the delegate executes against the snapshot taken at assignment time.

## A method value reassigned via `=` hoists its receiver capture
The receiver-snapshot decl above is emitted as a full statement (`var dʗ1 = d;`) before the lambda. In a `:=` **declaration** this hoists naturally, but a plain `=` **reassignment** to a pre-declared variable — database/sql's `checker = nvc.CheckNamedValue` — already wrote the LHS and `=` operator by the time the snapshot is generated, so writing it inline split the assignment into three token-broken pieces (CS1002). The converter routes the snapshot to the statement hoist buffer so it precedes the whole statement:

```go
var checker func(*driver.NamedValue) error
checker = nvc.CheckNamedValue   // reassign a method value
```
```csharp
var nvcʗ1 = nvc;
checker = nvcʗ1.CheckNamedValue;
```

This matches the `:=`-define path (which already hoists) and also covers a reassignment inside a tagless `switch` case. (Guarded by the `MethodValueReassignCapture` behavioral test.) `CheckNamedValue` here is an *interface* method, so the assignment binds a plain method group over the hoisted snapshot (see the interface-receiver rule above); a concrete-receiver method value keeps the param-carrying lambda form, referencing the snapshot the same way.

## A POINTER-receiver method value binds the ADDRESS in assignment context too
Go's `sw.Closesocket`, where `Closesocket` has a `*Switch` receiver and `sw` is addressable, **is**
`(&sw).Closesocket` — the address is taken once, at method-value creation (the same spec rule the
heap-box arm above rests on). The VALUE-context arm — a method value passed as a call *argument* —
already synthesized that `&` and bound the method group over the box. The **assignment**-context arm
did not: it rendered the receiver expression plainly and forwarded through the param-carrying lambda,
so the lambda body called the `[GoRecv]` `ж<T>` extension with a struct **value** receiver. net's six
socket-hook installs in `main_windows_test.go` were five CS1929 plus one CS1501, the latter because
the value receiver made a *different*, differently-arity'd overload the compiler's best candidate:

```go
poll.CloseFunc = sw.Closesocket      // sw is a package-level socktest.Switch
listenFunc     = sw.Listen
```
```csharp
poll.CloseFunc = Ꮡsw.Closesocket;    // was (syscallꓸHandle p1) => sw.Closesocket(p1)
listenFunc     = Ꮡsw.Listen;
```

The arm takes the value-context emission **wholesale** — a method GROUP over the box — rather than
only re-pointing the forwarding lambda's receiver, because two things were wrong, not one:

- The **lambda** is unnecessary here. A pointer-receiver method value binds once and aliases, so the
  group is both simpler and strictly more faithful than the lambda's documented per-call receiver
  re-evaluation. (The lambda remains right for a *value* receiver, where C# cannot build a delegate
  over a value-type extension at all.)
- The receiver **snapshot** was the deeper error, and only a local receiver exposes it. The snapshot
  exists to preserve a value receiver's bind-a-COPY semantics, which a pointer receiver does not
  have — the escape analysis has already ruled the other way by heap-boxing the local (the rule
  *A pointer-receiver METHOD VALUE heap-boxes its receiver* above) — and it *renames* the receiver, so
  the synthesized `&` produced `Ꮡcʗ1`, a box nothing declares (**CS0103**). `visitAssignStmt` now
  skips the snapshot for this shape at both of its method-value sites
  (`methodValueBindsReceiverAddress`).

net's own sites never showed the second half: `sw` is package-level, so no capture snapshot is taken.
The local-receiver form was broken before this change too — as CS1929 rather than CS0103 — so the
guard moved the error rather than introducing it. A receiver expression that is *already* a pointer
keeps both its existing pointer-context rendering and its snapshot: Go copies the pointer there, which
is exactly what the snapshot models. (Guarded by `MethodValueReassignCapture`'s `counter.bump` arm — a
pointer-receiver method value assigned to a pre-declared func var, with the mutation read back through
the original local, output-compared vs Go.)

## A VALUE-receiver method value snapshots its receiver in EVERY position
The receiver snapshot above exists because Go binds a value receiver by **copy at evaluation** — `x.M`
saves the receiver when the method value is created, not when the resulting func is called. The two
`visitAssignStmt` sites did that for assignment contexts. Every *other* position — a composite-literal
element, a call argument — reached `convSelectorExpr`'s param-carrying lambda instead, which rendered
the receiver **live** and so re-read it per call. The comment there recorded that as a caveat; it is
observable, and it presents as three unrelated-looking symptoms depending only on how the enclosing
slot happened to render the variable:

```go
x := frame{Name: "a"}
parts := []func() string{ x.label, func() string { return x.Name } }
x.Name = "b"
fmt.Println(parts[0](), parts[1]())   // Go: a b
```
```csharp
// before — the receiver read from the box at CALL time, printing "b b"
var parts = new Func<@string>[]{ () => Ꮡx.Value.label(), () => Ꮡx.Value.Name }.slice();

// after — the method value binds its own copy; the closure still sees the variable
var xʗ1 = x;
var parts = new Func<@string>[]{ () => xʗ1.label(), () => Ꮡx.Value.Name }.slice();
```

Spelling the same shape `[]any{…}` renders the receiver as the bare ref-local alias instead
(`() => x.label()`), which a lambda cannot capture at all — **CS8175**, the loud member of the family
and the one a `runtime -tests` conversion surfaced. Both close with the one snapshot; a fix that
instead routes the receiver through the box closes the loud member by converting it into the silent
one.

Three properties make the snapshot correct rather than merely compiling:

- **It is per-EVALUATION, never shared.** Two method values over one variable in different statements
  bind different receivers (`p`, then `q` after a write), so each mints its own snapshot. Converging
  every capture of one variable onto a single name — the shape a persistent capture-name registry
  would impose — would print `p p`.
- **It diverges from a sibling closure over the same variable, deliberately.** In the example above the
  method value must report the pre-write receiver and the closure the post-write one, from one
  statement. The snapshot's initializer is therefore rendered *outside* lambda context while the
  wrapper body binds the snapshot name, which also bypasses `convIdent`'s box rewrite — renaming
  inside it yields `Ꮡxʗ1.Value`, a box nothing declares (**CS0103**).
- **It is gated on a statement-level sink existing.** The declaration goes to `v.hoistedDecls`, the
  same buffer `convFuncLit` drains into for a func literal "on the RHS or inside a composite-literal
  element of it", which is what gives a nested element a valid declaration position. Where no sink
  exists the previous rendering stands: never apply a rename you cannot also declare.

The **assignment** position needed the same treatment for a different immediate reason. There the
snapshot is asked of the capture machinery, and it is delivered — until something heap-boxes the
variable, at which point `processPotentialCapture` returns early on `boxRefVars` ("must NOT be
snapshot-captured"). That early return is correct for a CLOSURE, which has to observe later writes
through the shared box, and wrong for a value-receiver method value, which must not:

```go
x := frame{Name: "a"}
f := func() string { return x.Name }   // heap-boxes x
m := x.label
x.Name = "b"
fmt.Println(m(), f())                  // Go: a b   —   C# was: b b
```

So that site mints its own temp too, gated on the variable actually *being* box-ref, so the ordinary
path keeps producing the one snapshot it already produces correctly — two would be a second copy of a
single evaluation. It is gated on a value, non-interface receiver besides: a pointer receiver binds the
ADDRESS and must not be copied, and marking one for snapshot is what turns this fix into CS1003/CS1002
across production files. A census of the whole standard library found **54** assignment-context
method-value sites, of which 8 are box-ref and **all 8 are pointer-receiver** (`database/sql`,
`go/parser` ×4, `go/types` ×2, `net`) — so this arm fires nowhere in the corpus today and its emission
is byte-identical. Both silent members are reachable and neither is currently reached: they close a
shape one refactor away, not an observed wrong answer.

## The receiver EXPRESSION is evaluated exactly once, for every kind and every shape
The rule above is about the receiver's *value*; this one is about the *expression*. Go saves the
receiver when the method value is created, so `f().M` calls `f` exactly once and `a.b.M` reads the path
exactly once. Every wrapper-lambda emission deferred that expression into the lambda, re-doing it on
each invocation. **Two independent mechanisms**, and the second is invisible if you only look for the
first:

- **M1 — the expression is deferred.** Re-executes calls and re-reads paths per invocation. It is
  **kind-independent**: measured red on a value receiver (`makeFrame().label`, Go calls it once, the
  conversion called it per invocation) *and* on a pointer receiver (`makePtr().bump`, likewise). The
  pointer case is reachable in exactly one shape — a call *returning a pointer*, since a value result
  is not addressable — which is why "pointer receivers emit method groups, so they already evaluate
  once" is true of every shape but that one, and false overall.
- **M2 — the root-ident snapshot aliases.** The capture machinery snapshots the root ident of the
  receiver expression, which is sufficient for a base with VALUE semantics (a struct, a slice header)
  and useless for one with REFERENCE semantics: copying a pointer or a map header still reads the same
  object. `p15.f.label` and `m13["k"].label` both re-read live state through the copy.

So a field chain is correct over a struct and broken over a pointer, with identical syntax — the
discriminator is the base's storage, not the shape.

The cut hoists the receiver into a statement-level temp and binds the temp. The temp is kind-correct
**by construction**: whatever the arm already rendered is exactly what Go saves — a value copy, the
bound `Ꮡ` address, or the interface value — so nothing derives the temp's content from the kind, which
is where a shape-first version would snapshot a value and bind *its* address. The initializer is
re-rendered in the ENCLOSING context, not reused from the caller: the caller's string is produced
in-lambda, so a captured base reads as the capture machinery's snapshot name, and that snapshot is
declared into the same hoist buffer *after* this temp — emitting `var recvʗ1 = h6ʗ1.f;` above
`var h6ʗ1 = h6;`, **CS0841** on every captured base.

A bare ident receiver is left alone, which is not an exception: a local read has no side effect and
nothing to alias, and the ident paths already produce a once-evaluated temp of their own. Most sites
the cut rewrites were therefore *already correct, by accident of value semantics*; it makes them
correct by construction, which is why its diff is wider than its behavioural yield.

Restricted to a plain ident receiver of non-pointer type — the shape an emission-attached census of the
whole standard library found (19 sites: `bytes` ×3, `strings` ×3, `encoding/json` ×8, `crypto/tls` ×3,
`crypto/internal/hpke` ×2, of which 12 have an ident receiver). A pointer base auto-deref'd to a value
receiver needs the *deref* snapshotted rather than the pointer, and takes the rule in the next section.
(Guarded by `MethodValueReceiverSnapshot`, which output-compares all five positions — typed element,
`any` element, cross-statement independence, call argument, and the box-ref assignment — against
`go run`.)

## A VALUE receiver reached through a POINTER expression hoists the POINTEE's copy
Go's implicit dereference: `h.p.label` with `p *frame` and a value-receiver `label` IS `(*h.p).label`,
so what the method value saves is **the pointee's copy**, taken at evaluation. Two consequences follow,
and they are separate questions — a later write *through* the pointer is not visible through the method
value, and *repointing* the pointer afterwards is not either.

The evaluate-once rule above renders the receiver as the enclosing context does, which for this shape is
the **box**, and binding a `ж<T>` where the emitted extension wants a `T` does not compile:

```go
h := holder{p: &frame{Name: "a"}}   // p is *frame; label has a VALUE receiver
fieldV := h.p.label
h.p.Name = "A"
fmt.Println(fieldV(), h.p.label())  // Go: a A
```
```csharp
// before — the receiver expression rendered as the box, per call
var hʗ1 = h;
var fieldV = () => hʗ1.p.label();   // CS1929: ж<frame> offered to label(frame)

// after — the temp holds the DEREF, so it is the pointee's copy at evaluation
var recvʗ1 = ~h.p;
var fieldV = () => recvʗ1.label();
h.p.Value.Name = "A"u8;
```

`operator ~` returns `T` **by value**, so a value receiver's copy semantics come out of the dereference
itself rather than out of a rule about it: a mutation inside the method reaches the copy and never the
pointee, and the temp is pinned to the pointee that was there at evaluation. The check runs *before* the
bare-ident early return, because for a pointer ident the once-evaluated rendering already in hand is the
BOX — a different value from the pointee, so hoisting it is not the "second copy of one evaluation" that
return exists to prevent.

Two narrowings, each matching a rule the CALL path (`(~z).make(n)`) already proved: a deref-**aliased**
receiver expression — a pointer parameter, or the enclosing method's pointer receiver — already renders
as the value, so a second `~` would dereference a non-pointer (CS0023) and the temp takes the plain
rendering (`var recvʗ1 = p;`); and a **promoted** method reaches its receiver through the `.of(…)` hop
machinery, so it keeps its existing emission. Both the assignment arm and the value (call-argument) arm
take the hoist — the same defect stood in both, and fixing one of them is not the fix.

Footprint, measured as two seeded whole-stdlib emissions diffed against **each other** (never against
the committed tree, which is a moving baseline): **zero** in `src/core` — 0 changed files and 0 hunks
across 6004 files per side, 0 unreadable, with both conversions exiting 0 and both emissions asserted
to carry the run's own mtimes. The shape is unreached in the production corpus, which is why it was
declined rather than guessed at when the evaluate-once family landed. It is *not* unreached in the behavioral corpus: `ReceiverCapturedInClosure`'s
`viaBareMethodValue` is exactly it — a pointer RECEIVER ident under a value-receiver `tag` — and its
golden re-baselines from `call(() => Ꮡw.Value.tag())` to a once-evaluated `var recvʗ1 = w;`. Its sibling
`viaFieldMethodValue` (`w.id.render`, a *value* field) is untouched, which is the narrowing working:
what matters is the type of the receiver EXPRESSION, not of the base it is reached through. No output
moves there — nothing writes between creation and call — so it is a correct-by-construction change with
no locally observable consumer, stated as such. (Guarded by `MethodValuePointeeCopy`, which
output-compares nine positions against `go run`: pointer field, repointed pointer, pointer local ident,
call argument, a call-shaped pointer receiver counted for evaluate-once, a method with parameters, the
value receiver's own mutation-does-not-reach-the-pointee direction, a pointer parameter, and a
pointer-typed slice element. Eight of the nine positions are `CS1929` on the pre-cut converter — nine
errors, the call-argument position carrying two sites — and the ninth, the pointer parameter, compiles
there and silently reports the pointee as it stands at CALL time.)

## A bare function value in `:=` takes its named delegate type, not `var`
Go's short-declaration from a bare function value whose type is a **named** func type — text/template/parse's `state := lexText`, where `lexText` is `func(*lexer) stateFn` and `type stateFn func(*lexer) stateFn` (the classic self-referential state machine) — infers the local as the *unnamed* signature. The converter cannot emit `var state = lexText;` (a C# method group has no `var`-inferable delegate type — CS8917), and typing the local structurally as `Func<ж<lexer>, stateFn>` makes it a **distinct** C# delegate from the `stateFn` the method group produces and that each `state = state(l)` reassignment yields (CS0029). It declares the local with the matching package named delegate instead:

```go
state := lexText
for state != nil {
    state = state(l)
}
```
```csharp
stateFn state = lexText;
while (state != default!) {
    state = state(l);
}
```

A `:=` from a method group whose signature matches no package named func type keeps the existing path. (Guarded by the `NamedFuncTypeStateMachine` behavioral test.)

## A DISCARDED function value is cast, never declared
One statement form over, the same typeless right-hand side needs the opposite treatment. Go writes `_ = someFunc` to force a symbol to be referenced — `debug/elf`'s `file_test.go` does exactly `_ = net.ResolveIPAddr // force dynamic linkage`. A C# discard infers its type **from its RHS**, and the two func-value forms cannot supply one: a method group, and a lambda (a func literal, or the method **value** that converts to one). Both are CS8183, *"cannot infer the type of implicitly-typed discard"* — the discard analogue of the `var` problem above, and notably NOT the same answer, because C# 10 does give a method group a natural type for `var f = pair;` while still refusing it for a discard.

A blank LHS is a discard, never a declaration, so the type goes on the **RHS** as a cast:

```go
_ = pair          // func(string, int) (string, error)
_ = sink          // func(int)
_ = lexText       // matches the named func type stateFn
_ = c.bump        // method value
```
```csharp
_ = (Func<@string, nint, (@string, error)>)(pair);
_ = (Action<nint>)(sink);
_ = (stateFn)(lexText);
_ = (Func<nint, nint>)((nint p1) => cʗ1.bump(p1));
```

A package named func type whose underlying signature matches is preferred over the structural `Func<…>`/`Action<…>` render so the reader sees the Go type's own name; for a discard either is sound, since nothing observes the value. The parentheses around the RHS are load-bearing for the lambda form — `(Func<…>)(nint p1) => …` does not parse as a cast. A variadic signature takes the golib delegate family (`Funcꓸꓸꓸ<@string, any, @string>` for `fmt.Sprintf`), and a func-typed **variable** is left alone: it already has a C# type.

Routing the named-type case through the *declaration* branch instead was the pre-fix behavior and is worse than the missing cast it fixed: `stateFn _ = lexText;` declares a local literally named `_`, which turns every other discard in the same scope into an assignment to it (CS0841 before it, CS0123/CS0029 after) and collides outright with a second one (CS0128). The blank test therefore short-circuits every declaration arm, not just the `var` one. (Guarded by the `BlankIdentifierCollision` extension — all seven RHS shapes plus the func-typed-variable and `:=`-named-delegate controls, proven failing-first at 11 diagnostics in 7 classes.)

## Two `unsafe.Pointer`s compare as BOXES, not as addresses
`unsafe.Pointer` is the one Go pointer that *carries* an address rather than *being* one, and its C# form says so: golib's `Pointer : ж<uintptr>`, whose `Value` **is** the address. That makes it the one type where the box and its value are both plausible comparands, and Go's rule picks the box — `p == q` is pointer identity.

The box is also the only one that works. `Pointer` overrides `ж<T>.Equals` to compare `PointerOrderToken` (`IsNull ? 0 : Value.Value`), so equality, hashing and ordering are one fact about the address; the base `==`/`!=` operators route through it, and it is nil-safe by construction. Comparing `.Value` instead bypasses all of that: it is right by accident for two non-nil pointers, and it **throws** on a nil one, because a nil `unsafe.Pointer` local is `default!` — a C# null reference.

```go
k := LoadPointer(&x.i)
if k != p { … }            // p ranges over testPointers(), whose first element is nil
```
```csharp
if (k != p) { … }          // NOT k.Value != p.Value — that NREs on the nil element
```

The pointer context is therefore suppressed for an equality comparison with `unsafe.Pointer` on both sides (`convBinaryExpr`), so `convIdent`'s `x.Value` arm — which is correct where an *address* is genuinely wanted — does not fire. The scope is exact: Go admits no other pairing without a conversion (`unsafe.Pointer == *T` and `== uintptr` are type errors), and comparison against untyped `nil` has its own arm and is unaffected. Corpus footprint is seven `runtime` sites (`alg`, `map`, `map_fast32/64`, `mbarrier`, `traceback`, `pprof/map`), each a deref-compare collapsing to a box compare, all verified compiling. (Guarded by the `ManagedAtomicPointer` extension — same-address, distinct-address and nil operands on both sides, a fresh conversion of one address compared against an earlier one, the nil-first table walk, and a selector-versus-ident pairing; the pre-fix converter fails it on both Target and Output, exiting 2 on the nil operand.)

## A method VALUE is named as Go's `-fm` wrapper; a method EXPRESSION by its receiver type
`runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name()` is how Go code names a function value (go-cmp's `cmp/internal/function.NameOf`, every `t.Run(FuncForPC(fn.Pointer()).Name(), …)` in reflect's own suite). Go gives a method **two** functions. The method expression `T.M` / `(*T).M` is the method itself, named with its receiver type. Every method **value** `x.M` runs through one autogenerated wrapper, `T.M-fm`, whatever receiver bound it. One taken through an interface is named by the interface. The emission is unchanged; the runtime names what the emission already holds:

```go
nameOf((*T).pointerMethod)   // main.(*T).pointerMethod
nameOf(p.pointerMethod)      // main.(*T).pointerMethod-fm   (also (&t).pointerMethod, t.pointerMethod)
nameOf(s.String)             // fmt.Stringer.String-fm        (s is a fmt.Stringer)
```
```csharp
nameOf(((Func<ж<T>, nint>)(pointerMethod)));   // RecvGenerator's `this ж<T>` forwarder, unbound
nameOf(p.pointerMethod);                        // the same forwarder, closed over p
nameOf(s.String);                               // the generated interface implementation, closed over the value
```

Three runtime rules make the names Go's. **A bound delegate tokens separately.** Reflect's `Value.Pointer()` keys a func value by its target method, so every method value of one method shares one token, which is Go's collapse. A delegate whose receiver is its target (`ManagedPointerTokens.IsMethodValue`) keys by a per-method object instead, so the method value and the method expression are two functions, and the method value's record takes `-fm` at `<autogenerated>:1`. A function literal is excluded (its method is compiler-generated, `<…>`), and so is a reflect `Value.Method(i)` func (a `DynamicMethod`), which keeps the shared token reflect's `TestMethodValue` asserts. **A forwarder is named as the method it forwards to.** The `this ж<T>` overload a pointer-receiver func value binds is go2cs-gen output, so it is never a Go frame; the record names and positions it as the `[GoRecv] this ref T` method. **An interface implementation is named by its interface**, found through `GetInterfaceMap`.

Go's `main` is emitted as C#'s entry point `Main`, and a Go function that is itself named `Main` is shadowed to `ΔMain`. So a receiver-less, parameterless `Main` in a package class is Go's `main`. Its own frame reads `main.main` and a literal inside it `main.main.funcN`, in a traceback, from `runtime.Caller`, and through `FuncForPC`.

(Guarded by the `MethodValueFuncNames` behavioral test — 14 names against `go run`, 8 of them wrong before the change.)

A VALUE-receiver method value (`t.valueMethod`, Go `main.T.valueMethod-fm`) is the one shape whose emission holds no method to name. It is a lambda over a copy of the receiver (`() => tʗ1.valueMethod()`), so on its own it reads as its enclosing function's literal. The converter therefore **records** it, invisibly: the file's `GoPositionMap` record takes a fifth argument, `<goLine>=<pkg.Recv.Method>` per method value, beside the literal names:

```csharp
[assembly: go.GoPositionMap("main.go", "main.cs", "AA0q…", "41-41:1", "38=main.T.valueMethod;39=main.T.valueMethod;45=main.T.valueMethod;51=main.T.walk")]
```

The runtime names a lambda `<pkg.Recv.Method>-fm` when both facts agree: its Go line carries an entry, and its body's single call targets a method of that name. Go spells the wrapper by the method's **declaring** receiver, so a promoted `s.M` of an embedded `E` is `main.E.M-fm`, and a generic receiver is `G[...]`. The runtime compares the method name only, because a promoted method's lambda calls the embedding type's forwarder. Go also **hides** the wrapper's frame: `runtime.Callers` from inside the method shows the caller next. `Frames.Next` skips a frame that resolves to `-fm`, and the panic traceback elides it the way it elides a method-expression wrapper.

Two residuals, disclosed. A literal on the **same line** whose whole body is a call to the **same method** reads as the method value; Go names it `Outer.funcN`. And `runtime.Callers` still **counts** the wrapper's pc where Go's does not: the record needs the PDB, and a capture never reads it, so the frame is dropped at expansion, not at capture. (Guarded by the `MethodValueFmRecord` behavioral test — two method values, a same-method literal on its own line as the control, the receiver copy, and a `Callers` walk through `call(t.walk)`; 3 of its 7 lines differ on the runtime without the record.)

---

[← Type Aliasing](type-aliasing.md) · [Index](README.md) · [Defer / Panic / Recover →](defer-panic-recover.md)
