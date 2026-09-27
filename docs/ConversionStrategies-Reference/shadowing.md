# Short Variable Redeclaration (Shadowing)

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#short-variable-redeclaration-shadowing)

When using Go's short variable declaration syntax, e.g., `x := 2`, a variable can be redeclared in a lesser (nested) scope. The inner declaration "shadows" the outer one: the inner instance is manipulated while the outer value is preserved, and once the inner scope ends the outer variable still holds its original value.

C# forbids a local (or a lambda parameter) from shadowing an enclosing local of the same name (CS0136). So rather than the older save/restore approach, the converter **renames** the shadowing inner variable with a `Δ` disambiguation suffix (`x` → `xΔ1`, `xΔ2`, …) and rewrites all references within that scope to the renamed identifier. The outer variable is untouched, so its value is naturally preserved. For example:

```go
func sumWithLenLocal(buf []int) int {
    total := 0
    len := len(buf)       // a local named like the built-in, shadowing it
    for i := 0; i < len; i++ {
        total += i
    }
    return total + len
}
```

converts to:

```csharp
internal static nint sumWithLenLocal(slice<nint> buf) {
    nint total = 0;
    nint lenΔ1 = len(buf);          // renamed; the built-in call stays `len(...)`
    for (nint i = 0; i < lenΔ1; i++) {
        total += i;
    }
    return total + lenΔ1;
}
```

The same `Δ` mechanism handles a local shadowing a called built-in (as above), a nested-block variable shadowing a function-level one, an IIFE/closure parameter colliding with an outer local, and a type-switch guard (`switch x := x.(type)`) whose variable shadows an enclosing one — the guard is renamed within the switch (`case T xΔ1:`) while references after the switch still resolve to the enclosing variable, matching Go's scoping.

The nested-block detection holds across a **closed sibling block**: a declaration that *follows* a nested block inside the same enclosing block (runtime `procresize`'s second `trace := traceAcquire()` after an inner `if {…}` that declared its own `trace`) is still checked against enclosing scopes. The shadow tracker's processing flag is shared across the nesting levels of a block tracker, so an inner block's cleanup must *restore* it for the still-open enclosing block rather than clear it — clearing it made the follow-on declaration skip the check and collide with the function-level local (both emitted `Δtrace` — the LAST runtime compile error, CS0136). Composition with the collision rename is suffix-based: the function-level local keeps the base name (`Δtrace` after its collision prefix), the shadows number independently of the prefix (`traceΔ1`, `traceΔ2` — a shadow name no longer collides, so it takes no `Δ` prefix). (Guarded by the `GlobalShadowedByLocal` extension `nestedBlockShadow` — the three bindings verified by value vs Go.)

A local shadowing a **same-named package function it calls in its own initializer** — `signame := signame(gp.sig)` (runtime `panic.go`) — renames the same way. Go starts the shadow *after* the initializer, so the call resolves to the function; C# scopes the local over its own initializer, so an unrenamed call would bind the (non-invocable) string local (CS0149). Detection is object-accurate: any identifier `go/types` resolves to the *function* while a same-named local exists means Go bound it where the local was not yet in scope — the old position guard ("call before the declaration") excluded exactly the own-initializer case. (Guarded by the `BuiltinShadowLocal` extension — a package `signame` shadowed in its own initializer, values vs Go.)

A **block-scoped `const`** that shadows an enclosing parameter or variable is renamed the same way. `func f(ns int64) { …; const ns = 10e6; use(ns) }` (runtime `notetsleep_internal`) is legal in Go but the inner `const ns` and the param `ns` both emit as `ns` in C# (CS0136). A const is tracked separately from variables — its `go/types` object is a `*types.Const`, not the `*types.Var` the scope stack records — so the shadow-rename pass had ignored it; it now records a shadowing const (detected by the same by-name enclosing-scope check) and rewrites its declaration and every use to `nsΔ1`, leaving the enclosing `ns` untouched. Only a *shadowing* const is renamed (a plain block const keeps its name, no churn). (Guarded by the `ConstShadowsParam` behavioral test — the inner uses bind the const value, the outer uses bind the param.)

Renaming depends on correctly identifying which declarations are *function-level* — the set a nested variable of the same name must avoid (C# forbids the nested one even when the function-level one is declared *later*). A `for init; …` loop's `:=` variable, and a range `:=` key/value, are scoped to their own statement, **not** the function body, so they are deliberately excluded from that set. Recording a for-loop variable as function-level (it is encountered first, in source order) would mask the real function-level variable of the same name declared afterward — `for b := …{} for b := …{} … b := newBucket(…)` — leaving *all three* emitted as `b` and colliding (CS0136). With the for-loop variables correctly treated as inner scopes, they are renamed `bΔ1`/`bΔ2` while the function-level `b` keeps its name. (Guarded by the `ForVarMasksFuncLevel` behavioral test; runtime hit this in `stkbucket`.)

The same forward-collision rule applies at **every block level, not just the function body**. C# CS0136 fires whenever a name is declared in two scopes where one encloses the other, *regardless of declaration order* — so a nested variable must be renamed if the same name is declared anywhere in an enclosing block, whether that declaration appears before or after it in source. The scope-stack walk only records declarations already seen (backward), and the function-level forward set covers only the function body; a variable declared **later in an intermediate enclosing block** would otherwise be missed. To close that gap, each block scope (function body, `if`/`for`/`range`/`switch`/`select` bodies, bare blocks, and `case`/comm-clause bodies) is pre-scanned for its directly-declared names (`:=` and `var`, excluding a control statement's own init `:=`, which is scoped to that statement) when the scope is pushed, so forward declarations are visible to the shadow check. For example, the runtime's `runGCProg` has two `for off := …` loops followed by `off := n - nbits` *in the same enclosing `for {}` body* — the block-level `off` encloses both loops, so the loop variables are renamed `offΔ1`/`offΔ2` while the block-level `off` keeps its name. (Guarded by the `ForVarMasksBlockLevel` behavioral test — distinct from `ForVarMasksFuncLevel`, where the later same-named variable is function-level; this cleared 5 runtime CS0136 in `runGCProg`/`mprof`/`runtime1`/`time`.)

The mirror image — a local shadowing a package-level **GLOBAL** — is resolved the other way: the *global reference* is qualified rather than the local renamed. C# locals are function-scoped, so a local `trace := traceAcquire()` shadows a same-named global `var trace` throughout the function, and an *earlier* read of the global binds to the not-yet-declared local (CS0841; the wrong variable regardless). Renaming the local is the fragile, entangled path (it interacts with collision renames and the shadow-rename counter); instead a use whose ident resolves to a package-level var **of this package** — while a same-named function-level local is declared — is emitted qualified with the package static class: `runtime_package.Δtrace.minPageHeapAddr`, which a local can never shadow. This is the same package-class qualifier the box-field accessor uses for a shadowed owning type (below). Runtime's `traceallocfree.traceSnapshotMemory` reads the global `trace.minPageHeapAddr` before its local `trace := traceAcquire()` (both collision-renamed `Δtrace`); the qualifier is gated so an ordinary global (no shadowing local) and the local's own uses (which resolve to the local, not the package scope) keep their bare, Go-like form — no churn. (Guarded by the `GlobalShadowedByLocal` behavioral test — a collision-renamed global and a plain global each read before a same-named local; cleared runtime's last CS0841.)

**That qualifier names the class that DECLARES the global, which under the white-box test model is not always the production one.** In a `whitebox-reference` internal variant, the emission unit is the bridge class (`md5_internal_test_package`), and a package-level declaration contributed by an internal `_test.go` lives *there* — the production class holds only the production declarations. Qualifying unconditionally with `<pkg>_package` therefore names nothing whenever the shadowed global comes from a test file (`CS0117`): `crypto/md5`'s `benchmarkSize` opens with the idiomatic `buf := buf`, whose package-level `buf` is declared in `md5_test.go`, and it emitted `md5_package.buf`. The qualifier is now chosen by where the object is declared — the bridge class for a `_test.go` declaration, the production class otherwise — so both halves of a white-box compilation stay addressable from the same function. Production emission is untouched (the override is empty outside the test variant), so this is zero-churn for the corpus; `crypto/md5`'s banked suite is its operational guard.

**A package-level CONST is shadowed the same way, and had neither half of the defence.** `q := big.NewInt(q)` — `crypto/internal/mlkem768`'s `TestZetas`/`TestGammas` over `const q = 3329` — is legal Go because a short variable declaration's scope begins *after* its own ValueSpec, so the initializer still reads the constant; C# scopes the local to the whole block and the initializer binds to the local it is declaring (CS0841 with an inferred `var`, CS0165 "use of unassigned local variable" when the declaration states its type). The local-rename half above cannot reach it: the pre-scan that drives it records only objects that are `*types.Var` and live in the package's global **var** map, so a const-shadowing local is never renamed. The const arm therefore qualifies, exactly as the global-var arm does — but it consults the WIDER local set. `funcLevelDecls` holds only declarations made directly in the function body, while the same shape inside an `if`/`for` init is not function-level; the const arm asks instead whether the name is declared *anywhere* in the function (`funcScopeVarNames` — nested blocks and func literals included), so both depths are covered. Qualifying a reference that no local actually shadows costs verbosity and never changes meaning, which is what makes the wider set the safe side to err on; a function that merely reads the const, declaring no such local, keeps the bare `q`. Two CS0841 were all that stood between `crypto/internal/mlkem768`'s converted suite and a build. (Guarded by extensions to the same `GlobalShadowedByLocal` behavioral test — a self-referencing initializer at function level, the same shape in an `if` init, the `var`-inferred form that reproduces the exact CS0841, and an unshadowed control that must keep the bare name.)

Two subtleties complete this for loops whose variable's box hoists before the loop. **A hoisted loop-variable box is block-scoped in C#, one per name per container.** A loop variable that escapes to the heap *and whose box is emitted before the loop* — today that is a string/int/chan/func **range** variable, or the legacy fallback of a `for i := …` clause variable referenced by a *clause* func literal; every other case boxes per-iteration inside the body (slice/array/map ranges via the deferred range-var box, and `for` clause variables via the per-iteration carrier rewrite — Go 1.22 semantics, see [Labeled Control Flow and Loop Variables](labels-and-loop-variables.md#labeled-control-flow-and-loop-variables)) — is emitted as a `ref var i = ref heap<…>(out var Ꮡi)` declaration hoisted into the *enclosing container* (function body, block, or switch/select clause) — see [Pointers](pointers.md#pointers) — so other loops in that container that reuse `i` genuinely collide with it, unlike the ordinary all-loop-scoped case above. Loop variables are therefore grouped **per container and name**: the first whose box *actually claims a container-level name* is the keeper and keeps its name; every other direct-child loop variable with that name in the same container is force-shadow-renamed. The claim test mirrors the emission exactly — the var escapes AND is not inherently heap-allocated (a pointer/slice/map/chan/interface/func var is already a reference and gets no box) AND the box actually hoists (per the split above). A group with no claiming var is untouched, so ordinary same-named sibling loops keep their Go names — a claiming sibling would otherwise emit a *duplicate hoisted box* in the same scope (CS0128), and a non-claiming sibling's loop-scoped variable (or deferred in-body box) nests inside the block that owns the box name (CS0136). (The historical motivating cases — runtime `typesEqual`'s `for i := 0` pair inside one switch case and `runqputslow`'s three `for i := …` loops — now box per-iteration inside their bodies and no longer claim container names at all; `EscapedLoopVarSiblingIndex` keeps guarding the sibling grouping and was re-baselined to the per-iteration shape.) A function-body-level keeper is additionally recorded as function-level (so non-loop uses elsewhere shadow-rename as before), but never masks a real function-level declaration — preserving the `ForVarMasks…` invariant above. A name group with no escaped variable is untouched (loop-scoped in C# too — no churn).

**Escape analysis marks only the arg's storage ROOT, not every identifier in a pointer argument.** Passing an expression to a pointer parameter escapes the storage the pointer refers to — the *peeled root* of a literal `&expr` (through parens, field selectors, index expressions, and derefs), or the bare identifier itself. An identifier appearing merely in a *subexpression* of the argument contributes a value, not its own address: in `xs[i].link(&xs[i+1])` or `typesEqual(tin[i], vin[i], seen)` the container (`xs`/`tin`'s elements) escapes but the index `i` does not. The old contains-anywhere check heap-boxed every such loop index — a spurious allocation on a hot path (Go keeps these in registers), gratuitous `Ꮡi` machinery in the emitted code, and the very duplicate-hoist collisions the grouping above then had to resolve (`typesEqual`'s pair now emits two plain `for (nint i = 0; …)` loops, no boxes, no renames). A direct `&i` anywhere — including nested inside a larger argument — is still caught independently by the address-of analysis. **And a renamed variable used as an LHS index/map key is rewritten there too.** An assignment `a[i] = …` / `m[ns] = …` / `p.f[k] = …` reassigns the *root* (`a`/`m`/`p`); the index/key expression is a separate value, so a shadow-renamed variable used there (`a[iΔ1]`, `m[nsΔ1]`) must be rewritten by descending the target's index/selector/deref chain and renaming each index. Missing this is a *silent* bug — the LHS key kept the enclosing variable's name, so `m[ns] = nsΔ1*100` wrote to the wrong key with no compile error — as well as a CS0136/CS0165 once the loop variable itself is renamed. (Both guarded by the `EscapedLoopVarSiblingIndex` behavioral test — the array case would not compile and the map case would silently return the wrong value without the pair, its `boxedSiblings` extension covers two genuinely-escaping siblings in one switch case (both take `&i`; first keeps the name, second renames), and its `caseSiblings` extension proves the index-only pair stays UNBOXED; cleared the 2 `runqputslow` CS0136, a CS0841, and the 2 `typesEqual` CS0128.) The target-chain descent also visits a **method-call receiver** in the chain — `x.ptr().Value.next = …` (runtime `stackpoolalloc`, where the loop `x` is renamed `xΔ1` because a func-body `x` is declared after the loop). The `x` is buried inside the `x.ptr()` call, past the selector/index steps, so without visiting the call the use kept the raw `x` — read before its (later) declaration → CS0841, or a silent wrong bind. Visiting the whole call renames its receiver and argument identifiers (the call's result is the navigated base, so the descent stops there). (Guarded by the `ShadowedVarMethodCallLHS` behavioral test — write-through through the method verified vs Go; cleared the `stack.cs` CS0841.) **A TYPE ASSERTION in that chain is a navigation step too, and its absence stopped the descent dead.** `n.Values[0].(*ast.CompositeLit).Type.(*ast.ArrayType).Len = nil` — `go/types`' `generate_test.go`, inside `ast.Inspect(f, func(n ast.Node) bool { switch n := n.(type) { case *ast.ValueSpec: … } })`, where the case variable shadows the literal's `n` parameter and is renamed `nΔ1`. The assertion was in no arm of the descent switch, so *everything* below it went unvisited: the chain's root ident kept its raw name and bound the enclosing `ast.Node` parameter, emitting `(~n).Values[0]…` — `~` applied to an interface, CS0023 — while the `if` condition one line above, an ordinary expression, renamed correctly. `getIdentifier` does not see through an assertion either, so unlike a paren-rooted target there is no `reassignVar` fallback to catch the root. The descent now visits the assertion's **operand subtree** whole (the treatment the method-call arm already uses): it carries every ident that needs the rename — the root and any index below it — while the asserted *type* carries none. Where the raw name still type-checks this was a silent wrong bind rather than a compile error, which is why the guard compares values against Go rather than merely compiling. (Guarded by the `AssignThroughTypeAssert` behavioral test — the `go/types` witness, a renamed loop index *below* the assertion in the same chain, the compound-assign form, an assertion at the very root of the target in a `default` arm, and a chain-read control that already worked; the pre-fix converter regresses five emission sites, four of them CS0023.)

The reverse collision — a package **method named like a built-in** — needs the opposite treatment. In Go a method `func (b *pageBits) clear()` and the universe `clear` built-in coexist: the method is only ever reached as `b.clear()`, while a free `clear(s)` is always the built-in. But the method is emitted as a `clear(this ref pageBits)` extension on the package's static class, and C# member lookup binds that same-class member for an *unqualified* free `clear(s)` call — shadowing the using-static `go.builtin.clear` and failing (`CS1620`/`CS1503`). So a built-in call whose name the package also declares as a method/function is emitted **qualified** — `builtin.clear(s)` — which resolves to the golib built-in regardless of the same-class shadow; the method call stays `b.clear()`. (This also required golib to gain the Go 1.21 `clear` built-in itself, in slice/span/map forms — plus an `IMap<TKey, TValue>` overload for a **named map type's value**: the generated wrapper implements `IMap<K,V>` and forwards to the shared underlying map, so `clear(h)` on an `http.Header`-style named map empties the caller's storage, and a nil named map stays a no-op (net/http/httputil's `clear(h)`, CS0411 without it). Guarded by the `ClearBuiltinShadow` behavioral test — including a named-map value cleared through an alias and a nil named map; runtime hit the original shadowing on `pageBits.clear`/`sweepClass.clear`, ~11 errors.)

For a **function-literal parameter** that shadows an enclosing local, the rename must reach the parameter *declaration* itself, not just the body: `run(func(n int){ … n … })` where an outer `n` is in scope emits `run((nint nΔ1) => { … nΔ1 … })`. The body's uses already resolve to `nΔ1`; if the signature still declared the bare `n` (the raw name), the body's `nΔ1` would be undeclared (CS0103). The parameter name in the emitted lambda signature therefore comes from the same shadow-aware identifier mapping as the body (the raw name when nothing is shadowed, so plain function types and non-shadowing parameters are unchanged). (Guarded by the `ClosureParamShadow` behavioral test; the runtime hit this pervasively on `mcall`/`systemstack(func(gp *g){…})` where the closure's `gp` shadows an outer `gp`, ~40 CS0103.)

Conversely, a **local that shadows a *pointer parameter*** must not inherit the parameter's special emission. A deref-aliased pointer parameter is `ж<T> Ꮡp` with `ref var p = ref Ꮡp.Value`, so passing it whole to a `*T`-expecting function emits its box `Ꮡp`. But a *local* `t` shadowing a `t *T` parameter (`func mapKeyError2(t *_type, …){ … var t *_type; … }`) is a plain pointer local — passing it should stay `use(tΔ2)`, not `use(ᏑtΔ2)` (the spurious `&` references an undefined `ᏑtΔ2` box → CS0103). The bug was that the "is this a parameter?" check matched by *name*, so the shadowing local was misclassified; it now verifies the resolved object is genuinely one of the function's parameter objects, not just a name match. (Guarded by the `ShadowedPointerParam` behavioral test; runtime hit this on `mapKeyError2`/`interhash`'s inner `var t *_type`, ~11 CS0103.)

## Type-vs-Method Name Collisions

Go keeps types and methods in separate namespaces, so a package may legally declare both a type `foo` and a method `foo` on some receiver. In C# both land in the same package class — the nested type and the `[GoRecv]` extension method — where a type and a method cannot share a name (CS0102). The converter resolves this by `Δ`-prefixing the **type** (`Δfoo`) while the method keeps its core-sanitized name (`foo`), so they no longer collide.

This needs an extra step when the colliding name is also a **golib reserved word** (`slice`, `array`, `channel`, `map`, …). Such a name is `Δ`-prefixed *anyway* — to avoid the golib runtime type (`slice<T>` etc.) — so the method too becomes `Δslice`, and the plain `Δ` no longer separates type from method. In that case the converter appends the type marker `ᴛ` to the **type** only, giving it a name distinct from the method:

```csharp
[GoType] partial struct Δsliceᴛ { … }                          // Go `type slice struct{…}`
[GoRecv] internal static Δsliceᴛ Δslice(this ref builder b, …) // Go `func (*builder) slice(…)`
```

Only the type side is renamed; the method (and every call site and go2cs-gen-generated pointer-receiver overload) stays `Δslice`. This is deliberate: the go2cs-gen generators compute method names independently, so renaming the *method* would desync them — renaming the *type* keeps the converter and generators in agreement (the generators read the type name from the emitted C# syntax/attributes). This mirrors the Go runtime's `type slice struct{…}` (the GC slice header) versus `func (*userArena) slice(…)`.

A **struct field** named like a colliding package-level identifier is *not* renamed: a field is struct-scoped (`g.trace` does not collide with a package type/method `trace` in C#), so the field declaration keeps its core-sanitized name (`trace`). The box-field accessor static the `TypeGenerator` emits for it is therefore `g.Ꮡtrace` (the `Ꮡ`-prefixed declared member name). The converter's `&g.field` address form (`Ꮡg.of(g.Ꮡtrace)`) must use that **declared** field name — it derives the accessor member from `getCoreSanitizedIdentifier` plus the type-colliding rename, *not* from the general identifier path that applies the package-level collision `Δ`-rename. Using the latter would emit `g.ᏑΔtrace`, which has no matching generated static (CS0117). Reserved-word fields keep their `Δ` (the field really is declared `Δarray` for a field named `array`), so the accessor is `Ꮡ`+the declared name in every case. (Guarded by the `CollisionFieldBoxAccessor` behavioral test; runtime hit this on `g`/`m`/`p`'s `trace`/`stack`/`p` fields, ~20 CS0117.) The generated accessor's **accessibility** matches the *field's* (its exportedness), not the field type's name — an exported field `Fun [1]uintptr` (C# `array<nuint> Fun`) yields a **public** `ᏑFun`, so another package's `other.of(ITab.ᏑFun)` can reach it; deriving the scope from the type's simple name (`array` → lowercase → `internal`) would make the cross-package accessor unreachable (CS0117 in runtime's `iface.go` walking `abi.ITab.Fun`).

One case *does* rename the field: when its name equals its **enclosing type's** name *and* that type is itself `Δ`-renamed for a type-vs-method collision. internal/trace's `type Label struct{ Label string }` sits alongside `func (e Event) Label() Label`, so the type becomes `ΔLabel`; the field, whose name equals the type, is renamed to differ (CS0542 — a member cannot share its type's name). The existing rename prefixed a single `Δ`, but that yields `ΔLabel` — *equal* to the renamed type, so the collision persisted. `typeCollidingFieldName` now **doubles** the marker (`ΔΔLabel`) when the name is a package-level collision, exactly as it already did for the keyword-family case (a reserved-word type is `Δ`-renamed too). Deterministic from the name, so the field declaration, the keyed composite-literal key, and every access site all agree:

```csharp
[GoType] partial struct ΔLabel {                 // Go `type Label struct{ Label string }`
    public @string ΔΔLabel;                      // field name == type name, doubled to differ
}
… new ΔLabel(ΔΔLabel: e.label, …)                // composite key
… l.ΔΔLabel                                      // access
```
(Guarded by `FieldNameTypeMethodCollision` — a `Label` field in a `Label` struct with a colliding `Label()` method, read/written through a value, the method result, and a composite literal.)

The double must also apply **across packages**. `typeCollidingFieldName` keys the double on the current package's `nameCollisions` map, which is populated only for the package being converted — so a **cross-package** access of such a field (internal/trace/testtrace reading a `Label`'s field) emitted the SINGLE-marker `l.ΔLabel` against the declaration's double `ΔΔLabel` — CS1061. The access site now consults the FIELD'S OWN package: `fieldTypeIsRenamed` derives the enclosing named type from the selector and asks `packageHasMethodNamed(type.pkg, type.name)` (a cached per-package scan of every func/method name — a type-vs-method collision Δ-renames the type), threading the result through a new `fieldTypeIsRenamed` ident context so `convIdent` upgrades the single marker to the double for the foreign case (the in-package case already doubled via `nameCollisions`, and its result is left untouched). [CNR](../Glossary.md#cnr) byte-identical (the pattern is absent from the single-package corpus except the guard). This is the FIELD-access counterpart to the cross-package renamed **type-reference** substitution (getCSharpTypeName / getScopeCheckedTypeName, further below) — that one covers naming the renamed *type*, this covers accessing its *field*; internal/trace/testtrace needs both. (The type-reference half — `trace.Time`/`Event`/`Stack` in a func signature, or `*time.Location` as a box element — is **also resolved**: a fresh full reconvert renders `traceꓸTime`/`ж<timeꓸLocation>` correctly through the `convertToCSFullTypeName`→`getAliasedTypeName` path described in *Foreign renamed types reference the recorded imported-type alias* below. It was mis-diagnosed as a still-open root off a stale [overlay](../Glossary.md#overlay) whose `importedTypeAliases` were not populated.) (Guarded by the `CrossPkgUser`/`CrossPkgLib` extension — `CrossPkgLib.Marker`, a `Marker` field in a `Marker` struct alongside a `Sensor.Marker()` method, its field read across the assembly boundary through an inferred-type value; vs Go.)

The same struct-scoped rule applies to a **keyed composite-literal field name**. `Frame{funcInfo: f}`, where the field `funcInfo` is named like a colliding package type/method (declared unrenamed as `funcInfo`), must emit the C# initializer key `funcInfo:` — the package-level `Δ`-rename that `convExpr` would apply yields `ΔfuncInfo:`, which is not a parameter name of the generated constructor (CS1739). `convKeyValueExpr` therefore emits a struct-field key whose name collides at package level via `getCoreSanitizedIdentifier` (the declared name), not the general identifier path. (Same `CollisionFieldBoxAccessor` test; runtime hit this on `Frame{funcInfo: …}` in `symtab`.)

The *type* half of the same accessor (`receiver.of(Type.Ꮡfield)`) needs care too. Go code routinely names a local after its own type — `m := getg().m`, where `m` is a `*m` — so taking the address of one of its fields (`&m.park`) emits `m.of(m.Ꮡpark)`, in which the bare type reference `m` binds to the **variable** `m` (a `ж<m>`, which has no `Ꮡpark`) instead of the type (CS1061). Because a converted struct is nested in its package's static class, the converter qualifies the type with that class — `m.of(runtime_package.m.Ꮡpark)` — which a same-named local cannot shadow. A bare `m` (binds the variable) and a `go.m` (the struct is not a direct member of the `go` namespace) both fail; the package-class qualifier is the correct form. This is applied **only on a collision** (the `.of()` receiver variable's name equals the type's simple name), so every other box accessor keeps its un-namespaced, Go-like form — no golden churn. (Guarded by the `VarNamedAsType` behavioral test; runtime hit this on `m`/`Δp` locals taking field addresses, ~9 CS1061.)

The same collision fires when the receiver is that variable's **lambda capture**. Inside a closure the captured variable renames to its capture copy (`mʗ1`), so the receiver-equality check alone misses it — but the *enclosing* local `m` is still visible to the C# lambda, so the accessor's bare owning-type reference binds to it all the same: runtime `rwmutex.lockSlow`'s `systemstack(func() { …; notesleep(&m.park) })` emitted `mʗ1.of(m.Ꮡpark)` → CS1061. `boxAccessorType` therefore also qualifies when the receiver is the type name plus the capture marker (`typeName + ʗ…`), yielding `mʗ1.of(runtime_package.m.Ꮡpark)`. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `capturedLocalNamedAfterType`, a type-named local field-addressed inside a capturing closure, write-through verified vs Go; cleared runtime rwmutex's 2 CS1061, 91 → 89.)

The type half also needs the **type-vs-method collision rename** (above). When the accessor's owning type is itself a colliding name — `type funcInfo` versus a method `func (f *Func) funcInfo()`, so the type is declared `ΔfuncInfo` — taking the address of one of its fields must use the renamed type (`Ꮡ(f).of(ΔfuncInfo.Ꮡnfuncdata)`); a bare `funcInfo.Ꮡnfuncdata` binds to the package's static `funcInfo` method group (CS0119). The `boxAccessorType` helper applies the `Δ`-rename to a bare same-package collision name before its receiver-shadow check (the renamed name no longer matches a raw-named local, so the two disambiguations compose). (Guarded by an extension to `CollisionFieldBoxAccessor` — a global whose type is the collision type; runtime hit this in `symtab`'s `pcdatastart`/`funcdata`.)

A **collision-renamed owning type is qualified unconditionally**, not just when it equals the `.of()` receiver — because a Go local named after its type is renamed to the *same* `Δ`-name, so such a local **anywhere in the function** shadows a bare `Δp.Ꮡfield` (C# locals are function-scoped). Runtime's malloc `persistentalloc1` does `persistent = &mp.p.ptr().palloc` and then declares a local `p` further down (renamed `Δp`); the accessor `(~mp).p.ptr().of(Δp.Ꮡpalloc)` bound its bare `Δp` to that later local — CS0841 (use-before-declaration), and CS1061 regardless (the local's type has no `Ꮡpalloc`). The receiver (`(~mp).p.ptr()`) is not the colliding local, so the receiver-name check missed it. `boxAccessorType` now qualifies whenever the type name is `Δ`-prefixed (a type is never shadow-renamed — types are package-level — so a `Δ`-prefixed accessor type is always a collision rename), emitting `(~mp).p.ptr().of(runtime_package.Δp.Ꮡpalloc)`. Qualifying is value-identical to the bare form when nothing shadows, so it is safe to apply to every collision-type accessor. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `localShadowsCollisionType`, a local named after the collision type declared after the accessor; cleared runtime malloc's CS0841 plus two mheap `Δp.Ꮡgcw` CS1061 of the same shape, 148 → 145.)

The three receiver-shadow arms above (`.of()` receiver equals the type, its capture, its box) and the collision-rename arm are all **special cases of one general rule**: a box accessor's bare owning-type spelling `Type.Ꮡfield` binds to **any** same-named variable that C# has in scope, and C# scopes a local to its whole enclosing block regardless of where it is declared or whether it participates in this accessor at all. So `boxAccessorType` also qualifies whenever a variable of the type's name is declared **anywhere in the current function** — receiver, parameters, results, or a local at any nesting depth (func literals included), collected into `funcScopeVarNames` during variable analysis. The general case, unrelated to any type-vs-method collision, is vendored poly1305 under `-tags purego`: `mac_noasm.go` declares `type mac struct{ macGeneric }`, and `func (h *MAC) Sum(b []byte)` declares `var mac [TagSize]byte`, so reaching the promoted-embed method `h.mac.Sum(&mac)` spelled `Ꮡ(h.mac).of(mac.ᏑmacGeneric)` in which `mac` bound to the `array<byte>` local (CS1061 ×2). It errored precisely in `Sum`/`Verify` (which declare that local) and **not** in `Write` (which does not) — confirming the diagnosis. Qualifying is always value-correct: Go guarantees the reference is unambiguous, and inside a scope that shadows the type the type is simply unreachable by that bare name, so every bare occurrence the emitter produces is meant as the type. Emitted forms (from the `LocalShadowsEmbedHopType` guard):

```csharp
Ꮡ(h.acc).of(acc.Ꮡinner).Add(p);                                             // Write: no local named `acc` — stays bare
Ꮡ(h.acc).of(main_package.acc.Ꮡinner).Store(Ꮡacc);                           // Sum: `var acc [4]byte` shadows the type — qualified
Ꮡ(d.deep).of(main_package.deep.Ꮡacc).of(main_package.acc.Ꮡinner).Store(Ꮡdeep); // Verify: both hop types shadowed (nested-block locals)
```

This is a **pre-existing** shadow class the converter always had latent; adopting `-tags purego` for the standard-library conversion (see *The standard-library conversion applies `-tags purego`* below) is what first reached the poly1305 code that exercises it — unblocking poly1305 and its 17 dependents (`chacha20poly1305` → `crypto/tls` → `net/http`, `net/rpc`, `net/smtp`, `expvar`). (Guarded by `LocalShadowsEmbedHopType`, whose `Write`/`Sum`/`Verify` discriminate qualified-vs-bare by whether the method declares the shadowing local; `CollisionFieldBoxAccessor`'s `boxRefCapturedValueNamedAfterType` golden was re-baselined — it previously compiled `Ꮡw.of(w.Ꮡpark)` only because C#'s identical-simple-name rule happened to bind the type; it now qualifies uniformly like every other shadowed accessor, a strict improvement with identical runtime output.)

A related case is the **box name of a shadow-renamed receiver/parameter**. A deref-aliased pointer (a receiver or a `*T` parameter) is emitted as `ref var <name> = ref Ꮡ<raw>.Value` — the `Ꮡ` companion always keeps the **raw** Go name, even when the value alias is shadow-renamed for a collision (`func (p *cpuProfile) add()` where `p` collides with the type `p` → `ref var Δp = ref Ꮡp.Value`). When a pointer-receiver (capture-mode) method is then called on that receiver/parameter, the call routes through the box, and that box reference must use the raw name `Ꮡp` — the value alias `Δp` would yield `ᏑΔp`, which is not in scope (CS0103). The converter builds the box from the raw identifier name (not the shadow-renamed value form), but only when they differ — so non-renamed receivers are unaffected (no churn).

The same raw-box-name rule applies when such a shadow-renamed pointer is **captured by a closure** (where the value alias is referenced through its box, since the `ref`-local can't be captured — see the *box-ref* section below). A value use inside the closure becomes `Ꮡp.Value.n` and a field-address use `Ꮡp.of(T.Ꮡn)` — both rooted at the raw box name `Ꮡp`, never the renamed `ᏑΔp`. The field-address form (`&p.field`) routes through the box-ref address path rather than the generic pointer-variable path: that generic path would prepend `Ꮡ` onto the closure's box-deref read (`Ꮡp.Value`), yielding a double-boxed `ᏑᏑp.Value` (CS0103). Because the captured pointer's box `Ꮡp` *is* the `ж<T>`, the field address is simply `Ꮡp.of(T.Ꮡfield)` — the same form as a captured value struct. (Guarded by the `RenamedReceiverBox` behavioral test, which exercises a shadow-renamed receiver calling a capture-mode method, plus a shadow-renamed pointer parameter both read through and field-addressed inside a closure; runtime hit this on `p`/`Δp` receivers calling methods like `p.addExtra()` and on closures capturing such pointers, ~12+ CS0103.)

A closure that captures an outer variable is emitted with a snapshot copy declared before the lambda — `var sʗ1 = s;` — and uses of the captured variable inside the lambda are rewritten to that capture name `sʗ1`. The capture-name mapping is keyed by **name**, which breaks on a **self-shadowing initializer inside the closure**: runtime `mgcsweep`'s `systemstack(func() { s := spanOf(uintptr(unsafe.Pointer(s.largeType))); … })` declares an inner `s` whose initializer reads the *outer* captured `s`. Both the captured use (the RHS `s.largeType`) and the distinct inner binding were mapped to the same `sʗ3`, so the inner declaration emitted `var sʗ3 = …(~sʗ3)…` — its RHS binding to the not-yet-initialized inner variable (CS0841). The fix records the captured **object** alongside the name, and applies the capture name only when an ident resolves to that exact outer object; the inner binding falls through to its own (shadow-renamed) name. The emission is `var sΔ1 = spanOf(…(~sʗ3)…)` — the inner `s` shadow-renamed to `sΔ1` (distinct from the capture `sʗ3`), its RHS correctly reading the captured `sʗ3`, and later uses of the inner `s` using `sΔ1`. Because the object check passes for every non-shadowing capture (the ident *is* the captured variable), it changes nothing outside this self-shadow case (zero golden churn). (Guarded by the `ClosureSelfShadowCapture` behavioral test — a captured pointer with an inner `s := f(s)` in a `systemstack`-shaped call-argument closure, output verified vs Go; cleared runtime `mgcsweep`'s CS0841.)

The same rule applies to an **escaping local** whose address is taken — `var p _panic; … preprintpanics(&p)` in runtime's `gopanic`, where `p` collides with the type `p`. The heap allocation is `ref var Δp = ref heap(new _panic(), out var Ꮡp)`, so the box is `Ꮡp` (raw) and `&p` must emit `Ꮡp`, not `ᏑΔp`. **Crucially, the box-name rule is keyed to the rename *kind*, because the two kinds name their boxes differently:** a type-**collision** rename prepends the marker (`p` → `Δp`) but keeps the raw box (`Ꮡp`), whereas a nested-scope **shadow** rename appends the marker plus a counter (`i` → `iΔ1`, `iΔ2`) and keeps the *shadow* box (`ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1)`, so `&i` correctly emits `ᏑiΔ1`). The converter therefore rewrites to the raw name *only* when the alias is exactly `Δ`+rawname (the collision form); a shadow-renamed or non-renamed var keeps its existing box name. (Guarded by the `CollisionRenamedLocalBox` behavioral test, with `ForVariants`/`NestedVarShadow` covering the shadow-rename form left unchanged.)

<a id="a-nested-closure-must-not-clobber-the-enclosing-closures-capture-state"></a>Moved to [A nested closure must not clobber the enclosing closure's capture state](functions-and-closures.md#a-nested-closure-must-not-clobber-the-enclosing-closures-capture-state).

<a id="the-capture-snapshot-is-a-statement-so-every-position-that-can-hold-a-func-literal-owes-it-a-hoist-target"></a>Moved to [The capture snapshot is a STATEMENT, so every position that can hold a func literal owes it a hoist target](functions-and-closures.md#the-capture-snapshot-is-a-statement-so-every-position-that-can-hold-a-func-literal-owes-it-a-hoist-target).

<a id="test-variant-name-coherence-production-names-are-pinned-test-side-method-declarators-δ-rename"></a>Moved to [Test-variant name coherence: production names are pinned, test-side method declarators Δ-rename](test-conversion.md#test-variant-name-coherence-production-names-are-pinned-test-side-method-declarators-δ-rename).

<a id="a-nested-packages-production-goimplement-record-anchors-to-the-production-metadata-file"></a>Moved to [A NESTED package's production `GoImplement` record anchors to the production metadata file](test-conversion.md#a-nested-packages-production-goimplement-record-anchors-to-the-production-metadata-file).

<a id="test-suites-reference-the-production-project-instead-of-recompiling-it"></a>Moved to [Test suites REFERENCE the production project instead of recompiling it](test-conversion.md#test-suites-reference-the-production-project-instead-of-recompiling-it).

<a id="under-the-recompile-model-the-test-half-continues-the-production-emission-the-productionseed"></a>Moved to [Under the RECOMPILE model the test half CONTINUES the production emission (the `productionSeed`)](test-conversion.md#under-the-recompile-model-the-test-half-continues-the-production-emission-the-productionseed).

<a id="a-recompile-model-test-project-compiles-the-production-sources--so-it-owes-their-references-and-their-per-goos-half"></a>Moved to [A recompile-model test project compiles the production sources — so it owes their references and their per-GOOS half](test-conversion.md#a-recompile-model-test-project-compiles-the-production-sources--so-it-owes-their-references-and-their-per-goos-half).

<a id="an-examplebenchmark-only-test-file-is-dropped-from-the-compile-set-phase-4d-file-exclusion"></a>Moved to [An Example/Benchmark-ONLY test file is dropped from the compile set (Phase-4D file exclusion)](test-conversion.md#an-examplebenchmark-only-test-file-is-dropped-from-the-compile-set-phase-4d-file-exclusion).

## Shadowing the names go2cs itself spells (`nil`, golib names, emitter-spelled type names, C# keywords)

A census (2026-07-16) of every non-function predeclared Go identifier, the golib public top-level type surface, and the C# keyword list — checked against the three name-protection mechanisms (`keywords` `@`-escape, `reserved` `Δ`-rename, and the shadow analyses) with a minimal transpile-and-run repro per candidate — found five real gaps, each fixed and guarded by the `ReservedNameShadows` behavioral test:

- **A local named `nil`** (`nil := 5`, legal Go) was emitted as the nil-literal rendering — `nint default! = 5;`, a syntax error — because the ident conversion matched on the *name*. It now checks the resolved object (mirroring the `true`/`false` handling): only a use resolving to the universe `*types.Nil` renders as the literal (`default!`, or golib `nil` in pointer contexts); a shadowing object falls through to normal rendering (`nint nil = 5;` — `nil` is not a C# keyword, and Go's own scoping guarantees no nil-literal use while shadowed).
- **User types named `builtin` or `sstring`** shadowed golib names the emitter references even when the Go source never spells them — the qualified `builtin.len(…)` calls (emitted when a package method shadows a built-in) bound the nested user struct (CS1501), and the string([]byte) elision's `sstring` views bound the user type (CS0030). Both names joined the `reserved` set: the user types decline to `Δbuiltin`/`Δsstring`.
- **User types named `any`, `rune`, `nint`, or `nuint`** shadow spellings the *emitter itself* produces: `interface{}` renders `any` (`slice<any>` bound the user struct, CS0029), an untyped rune-constant default spells `rune` (`c := 'x'` emits `rune c = 'x';`, CS0030), and Go `int`/`uint` map to the C# native-int contextual keywords (`partial struct nint { internal nint d; }` is a CS0523 layout cycle, and `@` cannot fix a name-identity problem). These names must **never** enter the string-based `reserved` set — legitimate emissions re-enter the same sanitizers, so corpus-wide `slice<rune>(…)` would corrupt to `slice<Δrune>` and re-fed delegate compositions corrupt `Func<…, nint, nint>` to `Δnint`. Instead `performNameCollisionAnalysis` registers a package-level TYPE bearing one of these names (`emitterSpelledTypeNames`) in the package-scoped `nameCollisions` map: every ident with that name *in that package* is `Δ`-renamed — exactly mirroring Go's package-scoped shadowing — with zero effect on any other package ([CNR](../Glossary.md#cnr) byte-identical).
- **`required` and `scoped`** are C# 11 contextual keywords banned as *type* names (CS9029/CS9062, surfacing in both the converted declaration and the TypeGenerator's output). Both joined the `keywords` `@`-escape set like `file` (`partial struct @required` — the `@` escape is valid in every position and the generator carries it through). `record`, `partial`, and the other contextual keywords compile clean as type names on C# 13/net9 (verified empirically) and stay unescaped.
- The keyword set carried the typo **`__argslist`**, which covered nothing: a Go local named `__arglist` hit the real (undocumented) Roslyn keyword and failed to parse (CS1002). Corrected — the local now emits `nint @__arglist = 5;`.

Census rows verified fine with **no action needed** (each proven by a transpile-run-compare repro): locals named any predeclared identifier (`nil`, `iota`, `error`, `any`, `comparable`, `rune`, the numeric type names — Go-consistent shadowing carries over); user types named `error` or a predeclared type name in the common self-consistent cases; embedded predeclared fields (`struct{ float64; rune; any; int; string }` — the color-color form `internal rune rune;` compiles, with keyword-mapped embeds escaping only the field NAME: `internal nint @int;`); the golib `Defer`/`Recover` delegates (never spelled in emitted code — the defer machinery's lambda parameters are inferred); and a local named `heap` alongside heap-boxing machinery (`heap<nint>(out var Ꮡx)` is a *generic* invocation, which a non-generic local simple name cannot shadow) — **half right, corrected 2026-08-28**: the generic form is genuinely immune for exactly the reason given, but the ARGUMENT-CARRYING form `heap(new T(), out var Ꮡx)` carries no type argument and does collide, which this row's repro never reached (see *A declaration named `heap` qualifies the boxing intrinsic* below). Known residuals, documented rather than fixed (unreachable under default flags in any constructed repro, or pathological): a user TYPE named a numeric alias name (`float64`, `int32`, `uintptr`, `complex64`, …) in a package where an emission would be *forced* to spell that predeclared name through inference; and `type string struct{}` (the golib `@string` spelling is escape-identical to the keyword-escaped user name).

## A parameter that shadows an imported package is renamed at its declaration too
A function parameter whose name equals an imported package the function references — crypto/rsa's `func emsaPSSEncode(…, hash hash.Hash)`, where `hash` shadows the `hash` package named in the signature type `hash.Hash` — is shadow-renamed by the variable analysis (`hash` → `hashΔ1`) so it does not bind the `using hash = hash_package;` alias. Every **usage** already rendered the renamed name (convIdent reads `v.varNames`), but the parameter **declaration** was emitted from the raw `param.Name()`, so the signature kept `hash.Hash hash` while its uses were `hashΔ1` — CS0103 at every use (40 sites in crypto/rsa, 27 in testing/quick's `rand`). The declaration now resolves through the same `v.varNames` map, so it matches the usages:

```go
func emsaPSSEncode(mHash []byte, emBits int, salt []byte, hash hash.Hash) { … hash.Size() … }
```
```csharp
internal static (…) emsaPSSEncode(slice<byte> mHash, nint emBits, slice<byte> salt, hash.Hash hashΔ1) { … hashΔ1.Size() … }
```

A non-shadowed parameter maps to its own raw name (no churn). (Guarded by the `PackageShadowParam` behavioral test.)

A shadow-renamed **pointer** parameter completes the same rule on two more paths. A `*T` parameter is deref-aliased as `ref var <value> = ref Ꮡ<raw>.Value`, so its box companion `Ꮡ<raw>` always keeps the **raw** Go name even when the value alias is shadow-renamed — `func decrypt(rand io.Reader, …)` where `rand` shadows the `math/rand`-style alias becomes `ref var randΔ1 = ref Ꮡrand.Value`. **(A)** An address-of or by-pointer pass of that parameter must therefore use the raw box name `Ꮡrand`, not `Ꮡ`+value-alias `ᏑrandΔ1` (which is not in scope, CS0103) — `boxBaseName` returns the raw name for a pointer *parameter* specifically (unlike an escaping shadow-renamed *local*, whose box *is* the shadow form `ᏑiΔ1`). **(B)** When a function has **both** a pointer parameter and a shadow-renamed value parameter, its signature is rebuilt through a separate `updatedSignature` path (not the `generateParametersSignature` path fixed above), which had kept emitting the value param's raw name — so `EncryptOAEP(hash.Hash hash, …)` diverged from its `hashΔ1` uses again. That path now resolves value-param names through `v.varNames` too, matching the primary fix. Together these cleared 50 errors (crypto/rsa 23 + testing/quick 27). (Guarded by the `PackageShadowPointerParam` behavioral test.)

## A declaration shadowing a BUILT-IN makes the call an ordinary call
Go permits shadowing a universe built-in at any scope, after which a call through that name is an
ordinary call to the declaration, **not** the built-in — math/big's own tests declare
`make := func(z *Int) *Int { … }` as a function-local and then call `make(test.z)`. The converter's
built-in handling is keyed on the identifier's **name**, so such a call was emitted with built-in
semantics. Every built-in arm is now gated on the identifier actually resolving to the universe
built-in (`identIsUniverseBuiltin` — go/types records a genuine built-in as a `*types.Builtin`
object; anything else is a shadowing declaration), and a shadowed call falls through to the ordinary
call path:

```go
make := func(n int) int { return n * 2 }
fmt.Println(make(21))
```
```csharp
var make = (nint n) => n * 2;
fmt.Println(make(21));            // was: fmt.Println(new nint()) — the argument dropped entirely
```

Seven built-ins had a name-keyed emission arm and so were affected: `make` (→ `new nint()`), `new`
(→ `@new<nint>()` — both drop the argument, CS1503/CS1929), `panic` (→ the *statement* `throw
panic(x)` in expression position, CS8115), `print`/`println` (a spurious variadic `interface{}`
cast), and `len`/`cap` **when the argument is a pointer-to-named-array** (a spurious `.Value` deref
from the auto-deref arm). `close`, `min`/`max` and `recover` already carried the `*types.Builtin`
check; `append`'s arm self-bails on a non-slice argument; the remaining built-ins (`copy`, `delete`,
`clear`, `complex`, `real`, `imag`) have no dedicated arm and already fell through. Two *analysis*
paths shared the hole and were closed the same way: `isTerminatingStmt` treated a shadowed
`panic(…)` as terminating (mis-deciding a switch case's `break`), and the capture-mode scan treated
a shadowed `recover(…)` as forcing the function's defer frame.

Note this is the **opposite** direction from `packageBuiltinShadows` (see *Type-vs-Method Name
Collisions*): there the call genuinely *is* the built-in and a same-named package method shadows the
C# `using static go.builtin`, so the call is emitted **qualified** as `builtin.<name>(…)`. Here the
call is not a built-in at all. (Guarded by the `BuiltinShadowLocal` behavioral test.)

## A declaration named `heap` qualifies the boxing intrinsic

Every collision above is between two things the *Go source* names. This one the converter **invents**:
`heap` is not a Go built-in, it is go2cs's own boxing helper (`golib`'s
`heap(value, out var Ꮡname)` / `heap<T>(out var Ꮡname)`, in scope in every converted file through
`using static go.builtin`), so a Go program may legally name anything `heap` with nothing in its source
hinting at a conflict.

The failure is the same CS0149 a shadowed built-in produces — a C# local or parameter wins simple-name
lookup outright over a `using static` member — but it lands at a line the Go source did not write: the
boxing prologue the converter emits for an address-taken local. It therefore reads as an emitter defect
at the box site rather than as a name collision. `internal/trace`'s
`func heapDebugString(heap []*batchCursor) string`, whose `strings.Builder` local needs a box, was the
whole of that package's 92-verdict build wall:

```csharp
ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);   // CS0149: Method name expected
```

The remedy is the **opposite** of the shadow-renames elsewhere in this section: `heap` is the name the
Go program chose and nothing about it is ambiguous in Go, so the identifier is preserved and the
INTRINSIC is qualified instead — `builtin.heap(…)` — and only where a `heap` declaration is actually in
scope, so the corpus stays byte-identical everywhere else. `heapIntrinsicName` supplies the spelling at
all fourteen emission sites; `declaresHeapIntrinsicIdent` answers per function declaration (walking
nested function literals, and OR-ed with a literal's own declarations in `convFuncLit`), and
`packageDeclaresHeapIntrinsicIdent` covers the one package-level shape that can also collide.

**What does and does not shadow is decided by C#'s invocable-member rule, and every boundary below was
measured rather than reasoned into place.** A simple name used as the target of an invocation ignores
type members that are not invocable, so only a genuine method group — or a nearer *local* declaration
space — can displace the `using static` import:

* **A local or parameter named `heap` DOES shadow.** Both argument-carrying shapes collide: an
  address-taken struct local, and an address-taken value parameter itself named `heap` (which adds
  `CS0841: cannot use local variable 'heap' before it is declared`). The invocable-member filter
  applies to type members, not to the local declaration space.
* **A type-argument-carrying call is immune.** `heap<nint>(out var Ꮡx)` is a generic invocation, and a
  simple name followed by a type-argument list considers only generic methods — a local is never a
  candidate. This is why the 2026-07-16 census cleared the case, and why a guard built on a *scalar*
  local (which takes exactly this form) proves nothing.
* **A package-level TYPE or VAR named `heap` does NOT shadow**, because neither emits an invocable
  member. The first version of this check tested "any non-`PkgName` object" and was falsified by its
  own A/B: `GlobalCapturedInClosure` declares `type heap` at package level, and with the fix reverted
  it still compiled — the broad form was only over-qualifying, changing a golden no defect required.
  The check is narrowed to `*types.Func`, the one package-level shape that is a real method group.
  Nothing in the corpus declares that, so the positive case is reasoned from the lookup rule rather
  than reproduced; the negative side is guarded.
* **An import ALIAS named `heap` does NOT shadow.** `import "container/heap"` renders as
  `using heap = go.container.heap_package;`, and a using-alias does not displace a `using static`
  method group in an invocation — proven by `container/heap`'s own banked `example_pq_test.cs`, which
  carries the alias and two heap-box emissions and compiles. `*types.PkgName` is excluded for that
  reason.

(Guarded from both directions: `BuiltinShadowLocal` carries the two colliding shapes plus a
non-shadowing function that must keep the bare `heap<arr>(…)`; `GlobalCapturedInClosure` carries the
package-level-type control that must keep the bare `heap(new heap(), …)`.)

## A local that shadows a PACKAGE name is not a package qualifier

Go lets a variable, parameter or receiver take the name of an imported package; from its declaration
onward the identifier denotes the variable, and the package is simply unreachable in that scope. The
standard library's own test code does this freely — `format_test.go` has both
`func checkTime(time Time, …)` and `time := Unix(0, 1233810057012345600)` inside `TestFormat`.

Every emission out of `convSelectorExpr` used to be passed through `getAliasedTypeName`, the
QUALIFIED-NAME resolver. That function reads its argument as `<package>.<member>` and rewrites either
half — a collision-renamed foreign member (`time.Second` → `time.ΔSecond`), a Δ-shadowed import
qualifier (`color.RGBA` → `Δcolor.RGBA`), or a type alias (`color.RGBA` → `colorꓸRGBA`). Applied to a
rendered *expression*, it fired on any base that merely **shared a name** with an imported package, so
one function produced three different wrong answers depending only on which rewrite matched the member:

```go
func checkTime(time Time, test *ParseTest, t *testing.T) {
    if time.Year() != 2010 { … }      // Year is not renamed
    if time.Month() != February { … } // Month is a renamed TYPE
    if time.Hour() != 21 { … }        // Hour is a renamed CONST
```

```csharp
Δtime.Year()      // the import alias — the variable vanished
timeꓸMonth()      // the type alias — a type used as a method
time.ΔHour()      // the const rename applied to the METHOD name
```

The resolver is now gated on the selector's base actually **denoting a package**, asked through
`go/types` (`selectorBaseIsPackage`, consulted by `aliasResolvedSelector`), so a shadowing binding is
excluded by construction rather than by name. A non-package base could never resolve through the alias
maps anyway — they are keyed `<package>.<member>` — so the gate states the property once instead of
per emission site. This cleared 33 errors across five codes (CS7036, CS1061, CS1955, CS8130, CS1501) in
`time`'s converted test suite alone. (Guarded by the `PackageNameShadowing` behavioral test, whose
`describe(time time.Time)` calls all three member kinds on the shadowing parameter.)

## A collision-renamed member keeps the file's RENAMED qualifier

A package that collision-renames an exported const or var publishes it with the `const:` marker, which
tells a consumer to keep the reference **qualified through the package** (`time.ΔSecond`) rather than
alias it to a type. That arm carried the qualifier through verbatim — the raw Go package name — while
the file's actual `using` may be Δ-renamed because a same-named child namespace is visible (see
*importAliasOperations.go*). Both halves have to move together:

```go
import (
    "time"
    _ "time/tzdata"   // puts the `go.time` CHILD NAMESPACE in the assembly
)
… time.Nanosecond …
```

```csharp
using Δtime = time_package;   // `time` alone would bind the go.time namespace
…
time.ΔNanosecond              // WRONG — CS0234, `go.time` has no ΔNanosecond
Δtime.ΔNanosecond             // emitted now
```

The qualifier is run through `importQualifier` when — and only when — it is a single segment, so an
already `_package`- or `global::`-qualified spelling is untouched.

## A DOT-imported collision-renamed member has no selector to carry the rename

`import . "time"` makes every exported member a bare identifier, which is what `time`'s external test
files use. A foreign **type** still resolves correctly in that form, because `foreignAliasedTypeName`
works from `go/types` rather than from the source spelling; a **const or var** had no equivalent, so
`Second`, `Minute`, `Hour`, `Nanosecond`, `UTC` and `Local` — every one Δ-renamed in `time` because a
`Time` method shares its name — emitted raw and bound nothing (CS0103 ×176 across five files).
`convIdent` now resolves such a reference through the same recorded `GoTypeAlias` entries the qualified
path uses (`dotImportedRenamedMember`), and emits the renamed member **bare**: a dot import renders as
`using static <pkg>_package`, which exposes it under exactly that name. Only `const:`-marked entries are
honored — a type entry resolves to a `pkgꓸName` global-using alias, which is the type layer's business.

---

[← Multi-Assignment and Evaluation Order](multi-assignment.md) · [Index](README.md) · [Multi-Result Values and Comma-Ok Forms →](multi-result-and-comma-ok.md)
