# Packages That Do Not Type-Check

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md)

A package the Go type checker could not fully resolve **still converts**, best-effort, and the run continues. This is not a tolerance the standard-library conversion needs — every stdlib package type-checks — but `-recurse` converts whatever an end-user module's closure reaches, and some of that legitimately does not resolve on the converting host: an app package naming a symbol that only exists behind a build tag, a cgo-only file, a third-party package whose own import failed. `go/packages` returns these WITH errors rather than failing the load.

The rules, in the order they matter:

* **Report, don't abort.** `processConversion` logs one `WARNING: <import path> did not fully type-check; converting best-effort — code depending on the following is emitted untyped: <errors>` naming the package and listing the loader's errors, then converts. A load *failure* (the package could not be read at all) is different — that is returned as an error, and the batch drivers record the package as failed and move on.
* **An untyped expression is not a crash.** `go/types` records **no type at all** for an expression whose operand went invalid (`Checker.record` returns early for `mode == invalid`), so `types.Info.TypeOf` hands back a nil **interface**, not `Typ[Invalid]`. Every converter site that reaches a type through `TypeOf`/`getType` on an arbitrary source expression must therefore tolerate nil — `underlyingOf(t)` (`astTypeSyntax.go`) is the guarded `t.Underlying()` for exactly this, and it returns nil so the caller's `.(*types.X)` assertion simply reports not-ok and the fallback path runs. A type obtained from the type SYSTEM (a signature's parameter, a named type's RHS) is never nil and needs no wrapper.
* **The emitted C# for an untyped region cannot compile, and that is correct.** Nothing names the missing symbol, so the region emits as `invalid type u = default!` and similar. The surrounding declarations convert normally; the goal is that one unresolved symbol costs its own statements, not its file, not its package, and not the run.
* **A fault is contained per package.** `ModuleConverter.convertAll` and `StdLibConverter.convertAllPackages` each wrap a package's conversion in `recover`, so an unconvertible package fails alone. Any pass that spawns goroutines must re-raise a worker's panic on the caller's goroutine or that containment is silently void — a panic unwinds only its own goroutine and ends the process. `performEscapeAnalysis` captures the first worker panic **with its stack** (`debug.Stack()` before the frame is lost, so the report names the faulting converter line rather than the re-raise site) and re-panics after `Wait`. Issue #33 was exactly this hole: a nil-type dereference in the escape analysis killed a whole `-recurse` run partway through. <!-- Issue #33: a 1,726-package -recurse run died at [736/1726]. -->

Guarded by `untypedPackageConversion_test.go` — one test converts a package mixing six invalid-operand shapes with healthy code and requires the healthy declarations to survive; the other injects a fault into the escape analysis and requires the caller to receive it.

---

[← Deterministic Output](deterministic-output.md) · [Index](README.md)
