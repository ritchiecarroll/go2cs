# Goroutines and the Runtime Scheduler

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#goroutines)

This page covers how a Go `go` statement lowers: how its callee and arguments are evaluated and wrapped before the goroutine starts. The scheduler, timer and synchronization contracts golib implements are on [Runtime Contracts](manual-conversions/runtime-contracts.md).

## The go statement

### A value-returning goroutine callee is wrapped in a discarding lambda
Go's `go f(…)` discards `f`'s result. Every `goǃ` runtime overload takes a **void** `Action<…>`
delegate, so a value-returning callee passed as a bare method group binds no overload (CS0407 "no
overload matches the delegate" — x/net/nettest `conntest.go`'s `go chunkedCopy(c2, c2)`, where
`chunkedCopy(io.Writer, io.Reader) error` returns `error`). `visitGoStmt` resolves the callee
signature and, when it returns a value, keeps the invocation inside a lambda so the result is
discarded — an expression-bodied lambda over a value-returning call converts to `Action` (the same
form the variadic path, e.g. `go fmt.Println(…)`, already emits):

```csharp
go chunkedCopy(c2, c2)          -> goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), c2, c2);   // param callee
go q.conn.HandshakeContext(ctx) -> goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx);      // selector method
go c.Close()                    -> goǃ(() => c.Close());                             // nullary callee
```

This parallels the **defer** case. Both `defer` and `goǃ` carry seventeen `Func<…, TResult>` twins
alongside their `Action` rungs, so a value-returning method group *with arguments* binds either
directly; neither has a nullary `Func<TResult>` rung, so a nullary value-returning callee takes the
`() => call()` discard on both sides (see *Deferred calls whose callee returns a value* below). The
converter's remaining discarding wraps cover the shapes a method group cannot express here — a
value-returning callee reached through a selector or parameter, and the CS1113
value-receiver-extension case below, whose reason is delegate *creation* rather than the callee's
result. Func-literal callees and `void`-returning method groups are untouched (`goǃ(() => { … })`,
`goǃ(emit, out)`).

The runtime's `Func` twins are what make one shape expressible at all: a func-literal callee that
*returns a value*. `go func(ln Listener) (retErr error) { … }(ln)` (net `sendfile_test`) emits its
literal with an explicit `error` return type, because a named result set by a `defer` needs one, and
no `Action<Listener>` overload accepts it (CS8934). Wrapping it in the converter would mean
suppressing the literal's own return type and rewriting its trailing `return retErr;` — rewriting a
correct emission to fit a runtime gap. The `Func` siblings fix that shape, and every other one, at
the seam where Go's rule actually lives: `go f(…)` discards results, for **any** `f`. A void lambda
or method group cannot bind `Func<TResult>` at all, so no existing call site is affected. (Guarded by
the `GoStmtValueReturn` behavioral test — value-returning nullary, single-, multi-param, multi-result
and func-literal goroutine callees, output-compared vs Go.)

A **VALUE-receiver method callee** forces the same lambda forms even when void and
arity-matching: every Go named type emits a C# struct and the method an extension on it, and C#
forbids constructing a delegate from an extension method over a **value-type receiver** (CS1113 —
net/http/httputil's `go spc.copyToBackend(errc)`, `switchProtocolCopier`). So
`goǃ(spcʗ1.copyToBackend, errc)` becomes `goǃ(ᴛ1 => spcʗ1.copyToBackend(ᴛ1), errc)` and a nullary
`go vs.ping()` keeps its invocation (`goǃ(() => vsʗ1.ping())`). The receiver snapshot (`spcʗ1`)
still evaluates at go-statement time. An INTERFACE-receiver method group is excluded (a genuine C#
instance method binds delegates fine), and pointer receivers keep the box-group machinery
(`pointerReceiverBoxMethodGroup` — `ж<T>` is a class, so its group is delegate-legal). (Guarded by
`GoStmtReceiverLambda`'s `valueSender` arms — value-receiver argument and nullary go-statements
with blocking-receive completion proof, output-compared vs Go.)

### The go-statement sibling of the receiver-capture family

The **go-statement sibling** of the receiver-capture family: a `go` statement calling a **value-returning** method through the enclosing method's pointer receiver — `go q.conn.HandshakeContext(ctx)` inside `func (q *QUICConn) Start` (crypto/tls quic.go, CS1628) — is FORCED into the synthesized-lambda emission because `goǃ` has only void `Action` overloads (the x/net/nettest CS0407 form): `goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx)`. That lambda references the receiver exactly like the method-value cases above, but neither closure predicate sees it — there is no `*ast.FuncLit` and no method-VALUE expression, only a go-call whose lowering *will* synthesize one. The capture-mode pre-pass therefore also promotes on `bodyHasGoStmtLambdaCapturingReceiver`, which mirrors `visitGoStmt`'s lambda-form decision (a nullary call synthesizes a lambda only for a value-returning or named-func-type callee; a call with arguments does so when the callee returns a value or the arity mismatches — variadic never matches) and fires when the CALLEE expression references the receiver (arguments render outside the lambda, as `goǃ` call arguments). With the method direct-ж, the go-stmt capture analysis' existing box-ref marking of the receiver (`varIsDerefdPointerParam`) takes effect and the lambda renders the chain through the box: `goǃ(ᴛ1 => Ꮡq.Value.conn.HandshakeContext(ᴛ1), ctx)`. The method-group emissions are excluded and unchanged — a void matching-arity callee (os/exec's `go c.watchCtx(resultc)`) binds the receiver chain at delegate-creation time, outside any lambda; a `defer` sibling needs no equivalent because any function-level defer already promotes via `bodyWrappedInDeferContext`. **Known divergence (Phase-4 item):** the synthesized lambda reads the receiver chain (`Ꮡq.Value.conn`) at **goroutine-run time**, whereas Go evaluates the method-value receiver at **go-statement time** — `go q.conn.M(x); q.conn = other` deterministically calls the OLD conn in Go but races toward the NEW one here (arguments are statement-time in both). The same lazy-chain window already exists for every synthesized go-lambda over a non-receiver chain (the CS0407 discard form); the faithful fix for the whole class is hoisting the receiver-chain prefix into a statement-time temp before the lambda, which would also avoid the direct-ж signature flip — direct-ж was chosen here for machinery reuse under the compile-first milestone. (Guarded by `GoStmtReceiverLambda` — `go e.tally.bump(delta)` (argument arm) and `go e.tally.report()` (nullary arm), both value-returning through a pointer field of the receiver, with the goroutine's writes read back through the original receiver chain, output-compared vs Go.)

---

[Index](README.md)
