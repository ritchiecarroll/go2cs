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
partial struct Δsliceᴛ { … }                          // Go `type slice struct{…}`
internal static Δsliceᴛ Δslice(this ref builder b, …) // Go `func (*builder) slice(…)`
```

Only the type side is renamed; the method (and every call site and go2cs-gen-generated pointer-receiver overload) stays `Δslice`. This is deliberate: the go2cs-gen generators compute method names independently, so renaming the *method* would desync them — renaming the *type* keeps the converter and generators in agreement (the generators read the type name from the emitted C# syntax/attributes). This mirrors the Go runtime's `type slice struct{…}` (the GC slice header) versus `func (*userArena) slice(…)`.

A **struct field** named like a colliding package-level identifier is *not* renamed: a field is struct-scoped (`g.trace` does not collide with a package type/method `trace` in C#), so the field declaration keeps its core-sanitized name (`trace`). The box-field accessor static the `TypeGenerator` emits for it is therefore `g.Ꮡtrace` (the `Ꮡ`-prefixed declared member name). The converter's `&g.field` address form (`Ꮡg.of(g.Ꮡtrace)`) must use that **declared** field name — it derives the accessor member from `getCoreSanitizedIdentifier` plus the type-colliding rename, *not* from the general identifier path that applies the package-level collision `Δ`-rename. Using the latter would emit `g.ᏑΔtrace`, which has no matching generated static (CS0117). Reserved-word fields keep their `Δ` (the field really is declared `Δarray` for a field named `array`), so the accessor is `Ꮡ`+the declared name in every case. (Guarded by the `CollisionFieldBoxAccessor` behavioral test; runtime hit this on `g`/`m`/`p`'s `trace`/`stack`/`p` fields, ~20 CS0117.) The generated accessor's **accessibility** matches the *field's* (its exportedness), not the field type's name — an exported field `Fun [1]uintptr` (C# `array<nuint> Fun`) yields a **public** `ᏑFun`, so another package's `other.of(ITab.ᏑFun)` can reach it; deriving the scope from the type's simple name (`array` → lowercase → `internal`) would make the cross-package accessor unreachable (CS0117 in runtime's `iface.go` walking `abi.ITab.Fun`).

One case *does* rename the field: when its name equals its **enclosing type's** name *and* that type is itself `Δ`-renamed for a type-vs-method collision. internal/trace's `type Label struct{ Label string }` sits alongside `func (e Event) Label() Label`, so the type becomes `ΔLabel`; the field, whose name equals the type, is renamed to differ (CS0542 — a member cannot share its type's name). The existing rename prefixed a single `Δ`, but that yields `ΔLabel` — *equal* to the renamed type, so the collision persisted. `typeCollidingFieldName` now **doubles** the marker (`ΔΔLabel`) when the name is a package-level collision, exactly as it already did for the keyword-family case (a reserved-word type is `Δ`-renamed too). Deterministic from the name, so the field declaration, the keyed composite-literal key, and every access site all agree:

```csharp
partial struct ΔLabel {                 // Go `type Label struct{ Label string }`
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

### A nested closure must not clobber the enclosing closure's capture state
The per-lambda conversion state — `conversionInLambda` (are we inside a closure body?) plus the capture-name maps (`currentLambdaVars`/`currentLambdaVarObjs`) — is what makes closure-body emission rewrite captured references to their box/copy forms: a captured local `s` reads as `sʗ1`, and the current method's **direct-ж receiver** (`func (s *Stmt) …` emitted `this ж<Stmt> Ꮡs`, whose body alias `ref var s = ref Ꮡs.Value` is a `ref`-local that **cannot** be captured by a C# closure) reads through its box as `Ꮡs.Value`. That state was *set* on entering a closure but **reset to `false`/`nil` on exit**, not restored — so a closure that contains an **inner** closure had its state wiped the moment the inner one finished, and every reference in the *outer* closure body **after** the inner one fell back to the bare, un-rewritten name. For a receiver field-read that is a bare ref-local capture — `database/sql (*Stmt).QueryContext`'s `s.db.retry(func(){ …; rows.releaseConn = func(err){…}; if s.cg != nil { … } })`, where `s.cg` sits after the inner `releaseConn` closure — the emission was `s.cg` (CS8175, "cannot use ref local `s` inside an anonymous method/lambda"); the equivalent captured-local case silently split a variable between its bare form and its `ʗ1` copy within one closure. The fix makes `enterLambdaConversion`/`exitLambdaConversion` a proper **LIFO save/restore stack** (`conversionStack`): entering pushes the current state and installs fresh state; exiting **restores the enclosing closure's** state instead of resetting. A closure at top level still restores to `false`/empty (unchanged), so the change is inert except where a closure body continues after a nested closure — there the receiver box-read (`Ꮡs.Value.cg`, `Ꮡs.Value.cg.txCtx()`) and the captured-local copy name are now applied consistently across the whole body. (Guarded by the `NestedLambdaReceiverField` behavioral test — a direct-ж receiver method whose closure holds a nested closure followed by a non-call receiver field read, a field-method call, and another field read, all verified to render `Ꮡs.Value.<field>` and output-compared vs Go; cleared `database/sql`'s 2×CS8175 and re-baselined `DeferValueFieldPtrReceiver` whose defer-then-body sequence exercises the same restore.)

### The capture snapshot is a STATEMENT, so every position that can hold a func literal owes it a hoist target
`var sʗ1 = s;` is a declaration statement, and C# has no statement slot inside an argument list — so a
capturing literal in expression position must send its snapshot to a **hoist sink** the enclosing
statement flushes ahead of itself. `convFuncLit` consults two, in order: the explicit
`LambdaContext.deferredDecls` builder that `go`/`defer`/`return` thread through the expression
contexts, then the ambient `v.hoistedDecls` that the assignment, expression-statement, `if`, `for`,
`range` and var-spec forms install. With neither, the decls emit **inline** and the file stops
parsing — `CS1003 ',' expected` + `CS1026 ')' expected` + `CS1002 ';' expected` + `CS1513 '}' expected`,
per site, the first of which reads as a defect in whatever token happens to follow.

Two positions had no sink, and between them they were the entire parse wall that kept `net/http`'s
1,352-verdict suite from ever running (28 diagnostics, 7 clusters, 2 of 35 converted test files):

- **A CONVERSION is transparent to the hoist.** `HandlerFunc(func(rw, req){ … conn … })` handed to
  `go Serve(ls, …)` (serve_test) is a *type conversion* whose operand is the literal. The conversion
  fork of `convCallExpr` rendered that operand with **no expression contexts at all**, so the wrapper
  made the literal invisible to the sink the `go` statement had already provided, and the snapshot
  landed inside the delegate-creation argument list `new Δhttp.HandlerFunc(var connʗ1 = conn; …)`.
  The fix adopts the ambient target for the conversion operand exactly as the `&composite` and
  composite-literal arms of `convExpr` already do — gated on a non-nil sink, so every other
  conversion keeps rendering with the nil contexts it always had. It matters only where a statement
  supplies the *explicit* builder and no ambient one, i.e. the `go`, `defer` and `return` forms; the
  statement forms that install `v.hoistedDecls` were already served by the second lookup.
- **A channel SEND supplied no sink of either kind.** `handlerc <- HandlerFunc(func(w, r){ … ts … })`
  (client_test) broke for the same reason with the wrapper, and a **bare** capturing literal sent to a
  channel broke without one. `visitSendStmt` now installs `v.hoistedDecls` and writes it before the
  send, the same shape `visitExprStmt` uses — under the same `useNewLine` test, because a `SendStmt`
  is a `SimpleStmt` and can also be a `for`/`if` init-or-post clause, where there is no statement slot
  to hoist into and the enclosing statement's own sink must stand.

The general rule the two share: **a conversion, an adapter wrap, or any other expression wrapper must
not be able to hide a func literal from the enclosing statement's hoist.** (Guarded by the
`CaptureHoistThroughConversion` behavioral test — ten shapes covering `go`, `defer`, channel send,
interface-element send, plain call, return and assignment positions, wrapped and bare, all
output-compared against `go run`; the seven affected shapes reproduce the CS1003/CS1026/CS1002
cluster with the fix reverted.)

## Test-variant name coherence: production names are pinned, test-side method declarators Δ-rename

The `-tests` pipeline re-analyzes the package over the **whole variant universe** (production files + `_test.go` files) but only **emits** the test files — the production `.cs` on disk were converted from the production-only universe and recompile into the test assembly as-is. Production symbol names are therefore **immutable** in a test-variant analysis: any collision a test file introduces must resolve by `Δ`-renaming the **test-side declarator**, never the production element. Two shapes (strings/sort blockers B2/B9), both resolved in `performNameCollisionAnalysis`:

- **A test-file METHOD over a production element's name (B2).** strings' export_test.go declares `func (r *Replacer) Replacer() any` — the ordinary type-vs-method resolution (above) would Δ-rename the *type*, but the production `replace.cs` on disk keeps `Replacer`, so the assembly split into two disagreeing halves (CS0102 `strings_package` already contains `Replacer` + CS0246 `ΔReplacer`). When the colliding method declarators are **all** test-declared and the element is production-declared, the element keeps its bare name (and no exported alias is registered) and the **method** Δ-renames instead; a FuncDecl colliding with a same-package element is necessarily a method (Go keeps method names in a separate namespace — any other same-scope reuse is a Go compile error). When a *production* method also carries the name, the production universe had the same collision and already renamed the element on disk, so the normal path stays consistent.
- **A test-file METHOD shadowing a dot-imported function the variant calls unqualified (B9).** Go keeps method names and dot-imported function names in separate namespaces, but both land in the C# package class's member-lookup scope, and an enclosing class's method group always wins over `using static` imports — sort_test.go's dot-imported `Sort(data)` bound example_keys_test.go's `By.Sort` extension (CS1501 ×14, plus 5 downstream method-group CS1503s the wave-2 probe had attributed to B10). A test-declared method whose name matches a foreign function the variant references **unqualified** (only unqualified sites conflict — SelectorExpr Sels are excluded, so a qualified `sort.Sort(ps)` never triggers; an unqualified foreign-function reference can only come from a dot-import) Δ-renames, and the dot-imported call keeps its bare emission, now binding through `using static`.

- **A test-file FREE FUNCTION whose emitted signature matches a production METHOD's receiver (2026-07-20).** A method emits as a C# extension method, so its receiver becomes the leading `this` parameter — and `this` does **not** participate in C# signature identity. math/big's `func (z nat) norm() nat` (nat.go) and `func norm(x nat) nat` (int_test.go) are legal Go in separate namespaces, but both emit as `norm(nat)` in `big_package`: CS0111. `resolveReceiverParameterCollisions` compares each same-named pair's *emitted* parameter list — the method's receiver type followed by its parameters, against the free function's parameters — and Δ-renames the test-side declarator. Discrimination is exact: an extra parameter (`func trim(x nat, n int)`) or a different first parameter (`func keep(n int)`) emits distinctly and keeps its plain name, as do generic declarations (type parameters keep the C# signatures distinct) and a variadic/non-variadic mismatch. Two methods can never collide this way (Go forbids redeclaring one method on one type) and two free functions cannot share a package scope, so a method/free-function pair is the only shape. When *both* sides are test-declared the FREE FUNCTION is the one renamed, so the outcome does not depend on declaration order and two colliding declarators never both become `Δ`-prefixed; a collision between two **production** declarators is deliberately left alone, since it would equally break the production-only conversion and is a different fix than test-variant coherence (the 302-package corpus compiles clean, so no instance exists).

The rename registry is **object-keyed** (`testMethodRenames map[types.Object]bool`) — the same-named production type/function keeps its plain emission at every other site — and **session-scoped**, initialized once per `-tests` conversion rather than per variant: both variants come from one `go/packages` load, so the external variant's references to an internal-variant method (the export_test pattern) resolve by object identity to entries registered during the internal pass. The declaration renames in `visitFuncDecl`, and every reference follows through `convIdent` — a METHOD name through its isMethod arm (all selector emissions funnel there), and a package-level **free function** referenced as a call target or function value through the trailing identifier path, which the receiver/first-parameter case above made reachable (without it the declarator renamed while its call sites still emitted the bare name: CS0103). The go2cs-gen `RecvGenerator` reads the emitted name, so generated `ж`-receiver overloads follow automatically. Real emissions from the probes:

```csharp
public static any ΔReplacer(this ж<Replacer> Ꮡr) { … }   // strings export_test.go `func (r *Replacer) Replacer() any` — type stays bare
@string got = fmt.Sprintf("%T"u8, tc.r.ΔReplacer());     // EXTERNAL-variant call site (replace_test.cs) follows the internal rename

public static void ΔSort(this By by, slice<Planet> planets) { … } // sort example_keys_test.go `func (by By) Sort(planets []Planet)`
new By(mass).ΔSort(planets);                                       // test-method call sites follow
Sort(data);                                                        // dot-imported production call keeps its bare emission
```

Production conversions have no `_test.go` files in their universe, so the analysis is inert there ([CNR](../Glossary.md#cnr) byte-identical ×402). (Guarded by `TestTestVariantPinsProductionTypeAgainstTestMethodCollision` — declaration, internal call site, external call site, and the pinned type — and `TestTestVariantRenamesTestMethodShadowingDotImportedFunction` — rename + bare dot-imported call, with never-referenced and qualified-only same-named methods as discrimination controls.)

## A NESTED package's production `GoImplement` record anchors to the production metadata file

An EXTERNAL test variant's collected `GoImplement` records are split across **two** anchor files, because the go2cs-gen `ImplementGenerator` hosts its output in the **first class** of the attribute-bearing file: a record whose generated adapter must be a member of the test package class goes to `package_info_external_test.cs` (whose first class is `<pkg>_test_package`), while a record that generates a partial/adapter on the **production** class stays with the production-anchored `package_test_info.cs`. `isTestAnchoredImplementRecord` decides which, by testing the implementer name against the production class's qualifier.

The anchor is a **file-level** property — `GetFirstClassName` reads the first `ClassDeclarationSyntax` in the compilation unit — so two required anchors mean two files; this is a workaround for the generator's positional contract, not an intrinsic need. The external unit is written only when the variant actually records test-anchored attributes, so utf8-class packages keep their single-file shape. It carries **no** `[GoPackage]` (the attribute-bearing partial stays in `package_test_info.cs`, CS0579) and **no** `global using` aliases (they must be declared once per compilation, CS1537).

⚠ **The `_test.cs` suffix on `package_info_external_test.cs` is load-bearing.** It is what excludes the file from the *production* project, via the shared `csproj-template.xml` `<Compile Remove="*_test.cs;…">` glob and `productionCSFiles`. `package_test_info.cs` does *not* match that glob, which is why it needed its own explicit entry in the template — and why the external unit was originally named `package_info_test.cs` (2026-07-18) to ride the glob for free. That name was a near-anagram of `package_test_info.cs`, and the two sorted adjacent to `package_info.cs` in every converted package directory; it was renamed to `package_info_external_test.cs` on 2026-07-21 ("external test package" is Go's own term for `package <name>_test`). Any future rename must keep the `_test.cs` suffix, or add an entry to the shared template — which re-emits, and so churns, every behavioral `.csproj`.

The live records qualify the implementer **namespace-relative — WITHOUT the `go.` root**: math/rand's external `rand_test` casting `*rand.Rand` to `io.Reader` records `ж<math.rand_package.Rand>`. The test formerly compared only the BARE (`rand_package.`) and fully-rooted (`go.math.rand_package.`) forms, so a **top-level** package matched by accident — its relative qualifier *is* the bare form (`sort_package.IntSlice`) — while every **nested** package (`math/*`, `text/*`, `net/*`, `encoding/*`, `container/*`, `hash/*`, `crypto/*`) matched neither and mis-anchored to the test file. The generator then emitted a SHORT `StructName` (non-foreign structs are emitted unqualified) inside `rand_test_package`, producing `ж<Rand>` where no `Rand` is in scope — while the converter's own cast site already assumed production anchoring (`new rand.RandжReader(r)` against `using rand = go.math.rand_package;`), so the pair failed as two CS0246s in the generated `.g.cs`. The relative qualifier is now recognized alongside the other two, matching the function's stated contract.

⚠ **`package_info_external_test.cs` and `package_test_info.cs` are MERGE-PRESERVING.** After any change to this routing, DELETE both files *and* the package's `Generated/` directory before re-running the pipeline — otherwise a stale record persists in **both** anchors and masks the result.

## Test suites REFERENCE the production project instead of recompiling it

The original `-tests` model — recompile the production `.cs` into the test assembly — duplicates the production types. That is harmless until another referenced assembly surfaces one of those types in its API: `strings.ToLowerSpecial(unicode.SpecialCase, …)` and `hash.Hash : io.Writer` name the type in the production assembly, while the test source binds a distinct recompiled copy. No compile-set adjustment repairs that identity split.

`-tests` therefore selects among **three test-project models** (`selectTestProjectModel`, recorded as `testProjectModel` in the manifest):

- **`reference`** — a black-box-only suite references the colocated production project and emits only the external test package into the test assembly.
- **`whitebox-reference`** — a suite with an internal variant still references production, but emits the internal `_test.go` declarations into a separate friend-assembly bridge. This keeps the production assembly as the sole identity while preserving access to Go-unexported members.
- **`recompile`** — the original same-assembly shape, retained only as the deterministic fallback when test-contributed metadata genuinely has to mutate a closed production type.

Both reference models bind the package under test as an ordinary imported package: exported aliases and implementation metadata load from the colocated `package_info.cs`, its types render package-qualified, and `isSameAssemblyPkg` is false. Their `package_test_info.cs` is a **test-class-only metadata anchor**; it never declares a local production partial.

The white-box extension has five coupled parts:

1. The normal production scan already reads build-selected same-package `_test.go` files to stabilize alias-shadow spelling. That same cheap scan now reports whether an internal test file exists. Only then does the production `.csproj` emit `<InternalsVisibleTo Include="$(AssemblyName).tests" />`; packages with no internal tests remain byte-stable.
2. Internal test files emit into `<name>_internal_test_package`, with `using static <namespace>.<name>_package`. Production declarations remain untouched. Test-host registrations retain the Go package name in the manifest but target this bridge class.
3. `go/packages` loads production, internal and external test variants together. An external test reference is routed to the bridge only when its `go/types.Object` belongs to the production import path **and** its declaration position is in `_test.go`; production objects and same-spelled unrelated declarations keep their ordinary route. This is how `io_test` reaches `ErrInvalidWrite` from `export_test.go` without source rewriting or a generated alias contract.
4. Test-contributed implementation adapters are owned by test metadata anchors — a MIXED suite has **two**. The generators host output in the FIRST class of the attribute-bearing file, and a mixed white-box assembly has two classes generated code must merge into, so the B4/B5 two-file split returns in mirror image: records whose generated partial must merge with a **bridge-declared** type (a BARE record name in the internal variant's declared-name set — `splitWhiteboxVariantRecords`) anchor in **`package_info_internal_test.cs`**, whose first — and only — class is the bridge (also the bridge's single `static` declaration and its `[GoPackage]` carrier); every other record — production-qualified, foreign, or external-declared — stays in `package_test_info.cs` under the external test class. Anchoring a bridge implementer in the external class would generate a phantom empty type there instead of merging with the real declaration. Deferred adapter markers are redirected only when the exact `(struct, interface)` pair appears in a test anchor (`emittedAdapterPair`); the anchored reference is composed `<anchor>.<member>` where the member comes from the **record's** spelling (`anchoredAdapterMemberName` — `adapterStructKey` normalizes a qualified production struct to the generator's foreign `<pkg>_<Simple>` form and leaves a variant-local name bare, exactly the generator's local-vs-foreign naming split; composing from the cast site's spelling instead emitted `ParseErrorжerror` where the generator wrote `csv_ParseErrorжerror`), and each pair remembers **which** anchor file recorded it. Imported production adapters keep pointing to their defining assembly. The generator also recognizes a collision-renamed embedded value property (`ΔBuffer` for embedded `bytes.Buffer`) as the same promotion hop, and scans the current compilation for friend-bridge box-receiver extensions by **simple name** when — and only when — the struct has no local declaration (the bridge spells its box parameter through the imported alias, `this ж<Replacer>`, and the metadata-only case is precisely the one whose discovery compilation is null).
5. The metadata seed imports the production, bridge and external-test classes as needed, but its first and only declaration remains the selected test anchor. An internal-only suite's bridge is both the test class and the bridge, so the seed imports it exactly once (a second, global import of the same class is CS8933). This keeps go2cs-gen's positional anchor contract deterministic for mixed and internal-only suites.
   **A MIXED suite's `package_info_internal_test.cs` is therefore written UNCONDITIONALLY, records or not (2026-08-14).** The file is not only a metadata anchor: it is the bridge class's ONLY `public static partial` declaration. Every converted SOURCE file opens its package class bare — `partial class registry_internal_test_package {` — exactly as production and external-test sources do, with the modifier living in the metadata file; see [`package_info.cs`'s `TypeAccessibility` section](source-generators.md#package_infocss-typeaccessibility-section-pins-each-types-accessibility-in-source) for the same division of labour applied to types. Writing the unit only when the variant contributed bridge-anchored `GoImplement`/`GoImplicitConv` records therefore left a record-less bridge with no `static` declaration anywhere, and an internal test file declaring a method on a production type — which converts to an EXTENSION method — is then **CS1106**. `internal/syscall/windows/registry`'s whole 6-verdict suite sat behind one such line, `func (k Key) SetValue(name string, valtype uint32, data []byte) error` in its `export_test.go`. Mixed suites that appear to escape it do so *incidentally*: `sort`, `bytes` and `strings` each happen to have a go2cs-gen `RecvGenerator` file that re-declares the class `public static partial` — a generator supplying a modifier the emitter owes. A record-less bridge writes an anchor whose sections are all empty, which is what the production and external-test seeds already do in the same situation. Measured: registry moves from build-blocked to **4 of 6** (residuals `TestValues`, a raw-address array reinterpret materializing a zero-length `array<T>`, and `TestGetMUIStringValue`); guarded by `TestWhiteboxBridgeUnitIsWrittenWithoutBridgeRecords`.
6. The friend grant is **inserted after template rendering**, never as a template verb: a user-supplied `-csproj` template keeps its historical verb count and renders exactly as before (`insertFriendAssemblyAccess`, anchored on the first closing `PropertyGroup`). And the reference models' anchored metadata writes treat the anchor class as the local type scope (`metadataAnchorLocalTypes`), while the recompile model's anchored writes keep the historical production-local qualification — there the production class genuinely is local to the assembly.

**A production ALIAS whose right-hand side is ANONYMOUS is carried across with its `global using` (2026-08-14).** Go's `type CorpusEntry = struct{Parent string; Path string; Data []byte; …}` (internal/fuzz's `fuzz.go`) has no C# spelling of its own, so the production conversion LIFTS the anonymous struct to a real nested type and reaches it through a compilation-scoped alias — `global using CorpusEntry = go.@internal.fuzz_package.CorpusEntryᴛ1;` at the top of `fuzz.cs`. `global using` is scoped to ONE compilation, and a reference-model test project is a second one that does not recompile the production sources, so neither half crossed: nothing visited the declaration, nothing claimed the lift, and every test-side reference fell through to `t.String()` and emitted **raw Go syntax into a C# file** —

```csharp
internal Func<struct{Parent string; Path string; Data []byte; Values []any; Generation int; IsSeed bool}, error> fn;
```

— CS1031/CS1525/CS1003 cascades in `minimize_test.cs` and `worker_test.cs`, with all 52 of the package's verdicts behind them. `seedProductionAliasLifts` now reads the production package's own `package_info.cs` (which the test conversion already opens for its `GoImplement` pairs) and seeds **both halves together**: the alias into `importedTypeAliases`, so the test metadata file re-emits the `global using`, and the anonymous TYPE into `productionAliasLiftedTypes`, so every renderer spells `CorpusEntry` (`liftedNameFor`, consulted wherever `liftedTypeMap` was). Keying by go/types identity is exact here — production and test variants are type-checked in one `go/packages` load, so the alias's right-hand side and every test-side reference are the same `*types.Struct`.

Narrow on both axes, deliberately — but on the SECOND axis only. Publication is the standing precondition: only an alias the production `package_info.cs` **publishes** is seeded, so a type is never rendered under a name the test compilation cannot resolve, and an unexported alias to an anonymous struct publishes nothing and keeps the pre-existing route.

**The anonymous-RHS restriction was WRONG and is retired (2026-08-19).** Its reasoning — "a named RHS already renders through its own qualified name" — does not hold wherever the production conversion declares the alias as a compilation-scoped `global using` in the FILE that declares it, because the renderer then spells the BARE alias name in production and test alike. `html/template`'s `type FuncMap = template.FuncMap` is exactly that shape:

```csharp
// template.cs, line 4 — the production conversion's own declaration
global using FuncMap = go.text.template_package.FuncMap;
```

A reference-model test project compiles `*_test.cs` ONLY, so `template.cs` is not in that compilation and the bare name resolves nowhere — `new FuncMap(new map<@string, any>{…})` in `clone_test.cs`, `escape_test.cs` and `exec_test.cs` is **CS0246 ×6**, with all 243 of the package's verdicts behind it. The test metadata file did already declare the CROSS-package two-hop spelling (`global using templateꓸFuncMap = go.text.template_package.FuncMap;`), which is what makes this a name-resolution gap rather than a missing import.

So a NAMED right-hand side is seeded too — and only the **name** half of it. The type half stays anonymous-RHS-only, because a named RHS has its own qualified spelling and is already rendered through it, so recording it in `productionAliasLiftedTypes` would re-spell references that already compile. The two kinds therefore need different halves of "reachable", and `recordType` in `seedProductionAliasLifts` is that distinction. The set this widens to is small and exact: across Go 1.23's converted standard library, an EXPORTED `type X = <named>` exists in four packages only — `html/template` (`FuncMap`), `os` (`DirEntry`/`PathError`/`FileInfo`/`FileMode`), `internal/reflectlite` (`Kind`) and `debug/buildinfo` (`BuildInfo`) — and a `_test.go` cannot redeclare such a name in the package's own scope, which is where the recorded collision concern was. Guarded by `TestSeedProductionAliasLiftsCarriesLiftAndAliasTogether`, which now pins BOTH halves of the named case (the alias seeded, the type not) alongside the anonymous case and the unpublished-alias control.

`internal/fuzz` builds clean afterwards (0 errors, from four parse-error families) and then stops one layer further out, on an infrastructure root that is **not** this one and is worth recording precisely: its `worker_test.go` `TestMain` calls `flag.Parse()`, and the converted `flag.CommandLine` has never been told about the host's own `--json` / `--result` / `--junit` / `-timeout` arguments, so the run dies with `flag provided but not defined: -json` before any test executes. In Go, `testing.M` registers those flags on `flag.CommandLine` before `TestMain` runs, which is what makes the same `flag.Parse()` legal there. This also **corrects** the board's attribution of the identical symptom on `go/internal/srcimporter` ("the process the host launches is not the go2cs test host"): the process IS the host — what it lacks is the flag registration.

**That second layer is now closed, host-only (2026-08-14) — `src/core/testing/TestFlagBridge.cs`, and `internal/fuzz` banks 52/52 behind it.** The host declares its OWN command line on the converted `flag.CommandLine` immediately before it invokes a converted `TestMain`, which is exactly what `testing.Init()` does for `-test.*` in Go and exactly what was missing: nothing is consumed or hidden from the parse, the names are simply *defined*, so `flag.Parse()` recognizes them. The full `-test.*` set is registered — with this run's real values for `test.run`/`test.parallel`/`test.v`/`test.short`/`test.count`/`test.timeout`/`test.shuffle` — rather than only the spellings the host was handed, because converted tests READ those flags back (`os/exec` gates on `flag.Lookup("test.run").Value.String()`, `runtime` on `flag.Lookup("test.parallel").Value.(flag.Getter).Get().(int)`); registering less would trade a parse error for a nil dereference, and it is why the typed registrars are used and not `flag.Func`, whose `funcValue` has an empty `String()` and is no `Getter`. The `flag` package is bound **late, by name**, not by project reference: Go's `testing` imports `flag`, but the generated test csproj sets `DisableTransitiveProjectReferences=true` (load-bearing against CS0576), so a `testing` → `flag` reference does not deploy `flag.dll` beside the 124 of 141 test hosts whose package does not import `flag` — measured, along with a +33% build cost on every test project — and it is not needed, because the converted `flag` package is present in a test compilation **iff** that package imports it, which is precisely when `flag.CommandLine` is observable at all. A name the test package already defined is skipped, since the converted `FlagSet.Var` panics on redefinition. Go's `M.Run` also parses when `flag.Parsed()` is false; that is deliberately NOT mirrored, because no member of the class needs it and an unconditional parse would newly reach `ExitOnError` for packages that merely reference `flag`.

**A THIRD layer of the same concern — the host now STOPS parsing at the first non-flag argument, which is Go's rule (2026-08-14, lane `claude/host-argv-stop`; `src/core/testing/TestOptions.cs`).** The bridge above made a converted `flag.Parse()` *recognize* the host's arguments; this makes the host recognize the PROGRAM's. `flag.(*FlagSet).parseOne` stops at the first token that is not at least two characters long and starting with `-`, leaving it and everything after it for the program — and a Go test binary is a program: its `TestMain` may take arguments, and `os/exec` drives its entire helper protocol that way, re-executing the test binary as `exec.Command(exePath(t), "cat")` and dispatching on `flag.Args()[0]`. The converted host's `TestOptions.Parse` had no stopping rule at all; its `default:` arm threw `unsupported converted test option: cat`, so every helper child died at startup with exit 2 before `TestMain` was entered, and the PARENT then reported the downstream symptom (`echo: want "foo bar baz\n", got ""`, `ExitCode got 2, want 42`) — one host defect reading as twenty unrelated failures across **26** of `os/exec`'s comparison rows. Nothing else was needed to make the remainder visible: the converted `os` package fills `os.Args` from the real command line independently of this parser (on Windows through `syscall.GetCommandLine` + `commandLineToArgv`, on unix through `runtime.argslice`/`goargs_impl.cs`), so the host's whole obligation is to stop, and to leave the trailing tokens untouched on the way past. **Stopping is not ignoring**, and the difference is load-bearing in both directions: `exe cat -n` must leave `-n` to the child rather than parse it as a host flag, while an unrecognized `-flag` appearing BEFORE any non-flag is the host's own command line being wrong — Go errors there too, so it still exits 2 (with Go's own wording, `flag provided but not defined: -x`, since this host stands in for a Go test binary and its stderr is read beside one). The rest of `parseOne` is mirrored for the same reason: a lone `-` is a non-flag by the length test, `--` terminates the flags and is itself consumed, `---x`/`-=x` are `bad flag syntax`, a non-boolean flag takes the NEXT token as its value even when that token looks like a flag (`-run -v` filters on `-v`), and one or two leading dashes name the same flag — the `--json` ≡ `-json` equivalence `TestFlagBridge` already assumed when it republishes these options under their undashed names. (Guarded by `TestingRuntimeTests.FlagParsingStopsAtTheFirstNonFlagAndLeavesTheRestToTheProgram`, which pins all of it through the host's public surface: recognized flags in both dash spellings, the stop, a trailing `-v` that must neither be consumed nor rejected, `-`/`--`, and the unknown-flag and bad-syntax errors before a stop.) `os/exec`'s root A is closed by this — 23 of the 26 rows joined the agreeing set — but the package does NOT bank: 27 rows remain on the declared `relocatable single-file test executable` host limit, and the 3 residual `TestWaitInterrupt` rows revealed a THIRD root underneath, `os/signal`'s runtime primitives being unimplemented partial stubs. See the board.

**A FOURTH layer, which CORRECTS one sentence of the third — the package's OWN flags participate, because the package is now initialized before the host decides (2026-08-17, lane `claude/tls-finish`; `src/core/testing/TestOptions.cs`, `TestHost.cs`, `TestFlagBridge.cs`).** The layer above says "an unrecognized `-flag` appearing BEFORE any non-flag is the host's own command line being wrong". That is right only when nobody else could own the name, and there is a whole class where somebody does: a package that declares its own `flag.Bool`/`flag.String` at package level. `crypto/tls` is the corpus's example — BoGo re-executes the test binary as its TLS shim (`-shim-path=os.Args[0] -shim-extra-flags=-bogo-mode`), `-bogo-mode` is a `flag.Bool` in `handshake_test.go`, and all **3,242** BoGo cases died at startup on `flag provided but not defined: -bogo-mode` before `TestMain` ran. **The mechanism is ORDER, not tolerance.** Go's test binary reaches exactly ONE `flag.Parse()`, and by then `testing.Init()` has defined the `-test.*` set *and* the package's own package-level variable initializers have run, so both vocabularies live in a single flag set — an unknown name is still an error there, just a later and better-informed one. The converted host's parse necessarily runs earlier than the package's initialization, so it was answering a question it could not yet answer. Three changes restore Go's order: (1) `TestOptions.Parse`'s `default:` arm records the name in `UnrecognizedFlag` and STOPS, exactly as a non-flag token stops it — stopping rather than skipping is forced, because nothing at that point knows a foreign flag's ARITY and `-port 5000`'s value is indistinguishable from a program argument, so everything from the unrecognized name onward is the program's (which does mean a HOST flag placed after a package flag is left to `flag.Parse()` rather than read here; the pipeline places the host's own flags first, and Go's single parse makes the ordering irrelevant on its side); (2) `TestHost.Run` runs the package's own initialization the way Go runs it before main — `RuntimeHelpers.RunClassConstructor` over the declaring types of the registry's delegates, which ARE the converted package's classes — but only when the parse actually met an unrecognized name, so every other run keeps initialization exactly where it was; (3) the verdict is then taken against the converted `flag.CommandLine` (`TestFlagBridge.IsDefined`), after the package's flags and the host's bridged flags are both declared, and an undefined name is still `flag provided but not defined: -x` with flag's own `ExitOnError` code. **The rejection moved; it did not go away** — and it did not move at all for the 124 of 141 test projects whose package does not import `flag`, where `IsDefined` answers false because no flag can exist rather than as a fallback. Ordering the package's initialization BEFORE `TestFlagBridge.Register` also makes that registrar's redefinition guard real for the first time (it skips a name the package already defined; previously the package had not yet run, so the guard could never fire). ⚠ **Creating a delegate over a static method does NOT run its declaring type's static constructor** — `ldftn`+`newobj` is neither a static-field access nor an invocation — so the generated host's `registry.Add("TestX", pkg_test_package.TestX, …)` lines leave the package uninitialized until the first test BODY runs; that measured fact is what makes step (2) load-bearing rather than defensive. Still deliberately NOT mirrored: `M.Run`'s `if !flag.Parsed() { flag.Parse() }`, for the reason the second layer already gives. (Guarded by `TestingRuntimeTests.APackageRegisteredFlagParticipatesAndAnUndefinedOneIsStillRejected`, which stands a class whose *static constructor* declares a flag in for the package under test — written with an explicit static ctor so the CLR's precise non-`beforefieldinit` rules apply — and pins all four claims: the package's flag participates, a flag before it is still the host's, a flag after it belongs to the program, and an undefined name is still exit 2. It is the one test needing the converted `flag` package present, so `BehavioralTests.csproj` references it; `testing.csproj` still must not, and does not.) The payoff is measured at value level: the converted host answers `-bogo-mode -is-handshaker-supported` with `No` byte-identically to Go's test binary, and a filtered BoGo case (`-bogo-filter Client-Verify-ECDSA-TLS1`) **passes** — the converted TLS stack completing a handshake against BoringSSL's own runner. See the board for why `crypto/tls` still does not bank.

**Fallback is based on mutation, not merely on a production-qualified record.** Pointer/value adapters and the shared `T → ж<T>` boxing route are relocatable. A structural conversion involving a production type, or a numeric conversion whose two operands are both production types, would require an operator on a closed referenced type; `recordsRequireProductionMutation` returns `errProductionAnchoredRecords`, and the already-loaded variants are re-emitted once under `recompile`. The older black-box `recordsRequireProductionAnchor` gate remains conservative for the ordinary reference model.

The model is abstract, not an `io` patch. `io` is the first mixed-suite proof: its project now builds with one `io_package.Writer` identity, `export_test.go` lives in `io_internal_test_package`, external references bind the bridge by Go object identity, and test-owned adapters coexist with imported `io` adapters. Focused guards cover model selection, conditional internal-test detection, host targeting, mutation fallback, emitted-pair adapter ownership, alias-shadow stability, and collision-renamed embedded-value promotion.
**What the bridge owes the compiler once production is a REFERENCE.** A 62-package regeneration sweep found five defects that share one cause: the bridge is the same GO package as production, so every same-package test in the converter reads a production declaration as LOCAL — while its C# now lives in a closed referenced assembly. Each was fixed at the layer that made the wrong assumption.

- **A bridge member HIDES the production `using static`.** The bridge binds production through `using static <pkg>_package`, and C# member lookup stops at the first enclosing type carrying the name — so any member the bridge declares hides *every* same-named production member, overload resolution included. container/heap's `func (h *myHeap) Pop() any` hid `heap_package.Pop(Interface)`, and the suite's own `Pop(h)`/`Push(h, i)` bound the extension by value (CS1620 ×8). Such a reference is emitted production-class-qualified (`go.container.heap_package.Pop(…)`), the remedy `packageBuiltinShadows` already applies to a shadowed `using static go.builtin`. The shadow set is the internal variant's package-level declarations **plus its methods** — those emit as static extension members of the same class — keyed on raw Go names, and consulted only for a production-declared package-level object.
- **A production type is FOREIGN for a VALUE implement.** "Local" is what selects go2cs-gen's partial-struct realization, which folds the interface into the type's own declaration; a closed referenced type instead gets a per-interface `ᴠ` VALUE ADAPTER. Both consequences matter and only together: the cast site must CONSTRUCT the adapter, and the record must be EXEMPT from the interface-inheritance prune (sound only for the one-type-one-interface-list partial shape). encoding/binary's `TestByteOrder` casts `BigEndian` to a function-local `byteOrder` that EMBEDS the production `ByteOrder`, so the subsumed `bigEndian → ByteOrder` pair was pruned as covered — true while a merged partial carried it, false against a referenced assembly, and every `Read(r, BigEndian, data)` was CS1503. The arm mirrors the existing both-foreign one, `importedValueImplements` check included, so a pair production already records stays a bare implicit conversion.
- **A value adapter must name the anchor its record lands in.** A mixed white-box suite has two metadata anchors, and the value-adapter reference qualified through the external test class unconditionally — right for a production↔production pair, wrong for one whose BRIDGE-declared interface anchors it at the bridge (CS0426). The reference now applies `splitWhiteboxVariantRecords`' own bridge-declared-name predicate to the record's two participants, folding in the live LIFTED claims while the bridge is the variant under conversion. (The pointer form already reached the same answer through its deferred marker's `emittedAdapterPairAnchors`; a value adapter's name is composed inline and has no marker to resolve.)
- **A bridge-declared LIFTED name must reach the record split.** That split keys on the record's EMITTED name against a set built from go/types `TypeName` defs, which carry the GO-SOURCE name — so encoding/hex's `type r struct{ io.Reader }` inside `TestEncoderDecoder` was collected as `r` while its record named `TestEncoderDecoder_r`. The record anchored in the external class and the generator declared a PHANTOM empty type there; every symptom followed from that one type being empty (CS0103 on the promoted embed it does not declare, CS0034 because a phantom carries no `[GoType]` and so no TypeGenerator `(T,T)` `==` to bind exactly, CS1503 at the cast site). The internal variant's live lift claims are unioned in right after it converts, while they still stand.
- **A BARE record name resolves in the variant that RECORDED it, never across variants.** The bridge's declared-name set is a set of SIMPLE names, and the two `-tests` variants are separate Go packages, free to declare the same one: encoding/gob declares `Point` in `codec_test.go` (`package gob`) and again in `example_interface_test.go` (`package gob_test` — the one whose `Hypotenuse` implements `Pythagoras`). Each variant's records are split as that variant converts, and every cross-variant reference is routed by `go/types.Object` identity to a CLASS-QUALIFIED spelling — `whiteboxBridgeNamedType` renders an internal-test type the external suite names as `global::<ns>.<pkg>_internal_test_package.T`, and `whiteboxProductionObject` does the mirror while the bridge converts — so a bare name recorded by the external suite is external-declared *by construction*, whatever the bridge spells the same way. Matching it against the bridge's set regardless anchored the EXTERNAL pair `Point → Pythagoras` in `package_info_internal_test.cs`, where `Pythagoras` is not in scope: `CS0246`, no test host built, and all 106 of gob's verdicts read empty — a missing host masquerading as mass runtime failure. The set is therefore consulted only while splitting the BRIDGE variant's own records (`splitWhiteboxVariantRecords`' `bridgeVariant`), and the emission mirror that names an adapter through the anchor its record will land in carries the identical gate (`whiteboxBridgeDeclaredType`), so the two cannot disagree about a pair. Write-time qualification is not a substitute: `qualifyAmbiguousTestTypeRefs` roots an ambiguous bare name at the file it is ALREADY being written into, so a mis-anchored record comes out merely qualified to the wrong variant's class (`gob_internal_test_package.Point`). Guarded by `TestSplitWhiteboxVariantRecordsResolvesBareNamesInTheRecordingVariant`, over a fixture module that declares `Point` in both variants and asserts the collision itself through the real `go/types` scan (`collectWhiteboxBridgeTypeNames`) before exercising either split.
- **The friend bridge's box receivers survive into the pointer adapter.** sync's `export_test.go` declares `PushHead`/`PopTail` on the production `*poolDequeue`/`*poolChain`; the converter emits them as direct-ж primaries, so `poolDequeueжPoolDequeue` must forward `m_box.PushHead(val)`. Two generator defects stacked: the bridge scan matched the receiver's parameter TEXT against one composed spelling (`ж<poolDequeue>`) when the bridge qualifies it however its own file needs — `this ж<global::go.sync_package.poolChain>` once a `go/*` package in the closure shadows the root namespace — and the foreign-struct arm then re-derived every member's receiver from the referenced assembly's METADATA, where a bridge-contributed method does not exist, clobbering the binding back to `m_box.Value` (CS1929 ×5). The scan is now on the ж argument's last dotted segment (well-defined because it runs only where the struct has no local declaration), and a bridge-bound member is left alone.

A sixth, in `golib`, was masked behind those compile failures: **`reflect` reads a package class's `[GoPackage]` stamp, not its NAME.** `GoReflect` reconstructed a type's Go package by trimming `_package` off the declaring class name — a heuristic the bridge breaks by design, since `binary_internal_test_package` hosts declarations Go-declared in `package binary`. Both readers (`GoTypeName`'s qualifier and `PkgPath`) now prefer the stamp, with the name-trim kept only for a hand-written class carrying none; the two agree for every ordinary converted package. encoding/binary catches it through Go's own asserts twice: `TestNoFixedSize` compares the error text `… not fixed-sized in type *binary.Person`, and `TestSizeAllocs` NAMES its subtests from `reflect.TypeOf(v)`, so a whole subtest set appeared under invented names with no Go counterpart.

**Reference closure (the declaration-edge rule).** The test project's references are the direct-import set, plus the alias scan (B2c), plus — because binding *any* referenced type in C# requires the assemblies that type's **own declaration** names — the **declaration closure** of that set (`declarationClosureImports`, both project models). Two declaration edges carry it: an interface's **base interfaces**, and a struct's **field types**. *Interface bases:* Go interfaces satisfy structurally *and* compose by embedding; C# interfaces are nominal, so the converter carries both shapes as C# inheritance at each interface declaration (`getStructuralInterfaceBases`): `hash.Hash` **embeds** io.Writer, io/fs's `fs.File` lists `Read`/`Close` explicitly and does *not* embed io, and both emit a converted declaration that NAMES an io base. Such a base edge belongs to the **declaring package's** import graph, so it appears in no test import and no alias `using`; `DisableTransitiveProjectReferences` (B2b) then hides the declaring assembly's own io reference, and every site that names the interface fails CS0012:

- the emitted **conversion record** — hash/maphash's `[assembly: GoImplement<Hash, hash_package.Hash64>(Pointer = true)]` and crypto/hmac's `GoImplement<hmac, hash_package.Hash>`, whose closures reach `hash` but never `io` (`'io_package.Writer' is defined in an assembly that is not referenced`);
- the **go2cs-gen adapter** realizing that record, whose class declaration lists the interface; and
- every converted production/test **source** that names it (`(fs.File, error) Open(...)`, hmac's `justHash`).

*Struct fields at a composite literal:* the converter renders a Go composite literal as `new T(Field: …)` — a call to the **fieldwise constructor** go2cs-gen generates for a `[GoType]` struct, whose parameter list spells out **every** field's type — so binding that call needs every field type's assembly. `testing/quick`'s `Config` holds a `Rand *rand.Rand`, so `image/draw`'s `quick.CheckEqual(orig, sqDiff, &quick.Config{MaxCountScale: 10})` fails `CS0012: The type 'rand_package.Rand' is defined in an assembly that is not referenced` *at the `new quick.Config(…)` expression*, with `math/rand` in no import list on either side. No interface closure can ever reach it — `Rand` is a **struct**, so the shape is invisible to a base-interface walk; it is the same missing-declaration-edge defect one type-kind over.

The alias scan cannot reach either class: the named type *does* bind by name — its own package **is** referenced — and what is missing is a package named inside that type's own C# declaration. Three gates keep the closure minimal, because over-inclusion is its own defect — every extra reference is churn across the banked corpus plus a chance at a duplicate-type conflict:

- **Seed only from the files the test assembly COMPILES** (`referencedTypeSeeds` walks the retained syntax rather than iterating the `TypesInfo` maps, which also makes seed order deterministic). A Phase-4D compile-excluded Example/Benchmark-only file is analyzed — so its declarations still reach the manifest — but no C# is emitted for it, so it names nothing the compilation must bind. Seeding from one handed `compress/gzip` five references (`context`, `crypto/tls`, `mime/multipart`, `net/http`, `net/url`) reached through `http.Request`'s fields, from an `example_test.go` that is not compiled at all, and handed `go/token` a `go/ast` reference the same way.
- **Start the interface walk from named types, never from whole packages,** and follow bases **transitively** (`b.B : a.A : io.Writer` needs both `a` and `io`, because a base's own declaration must bind in turn). C# needs a base's assembly only when the derived interface is BOUND, and walking every exported interface of every referenced package would hand `io` to nearly the whole corpus through `fmt.State`'s structural io.Writer base — a reference no project that merely calls `fmt.Sprintf` requires.
- **Fire the struct-field edge only where a composite literal CONSTRUCTS the struct** — that, not mere value use, is what demands the field types, and one level suffices (the generated constructor's parameters default unless supplied, and a *nested* literal is itself a seed). Measured against the corpus: eleven banked packages hold `sync.Once`, `sync.Map` and `reflect.Value` **values** (strconv's package-level `atofOnce`, encoding/binary's `reflect.ValueOf`) and compile clean today with no reference to `sync/atomic` or `internal/abi` — so a "named by value" rule would add eleven references that nothing needs. `os.File`'s single `*file` field likewise never drags internal/poll, syscall and the rest of os's private graph in. An **EMPTY** literal carries the edge only for a struct declared in a **ROOT** package, and the boundary is ACCESSIBILITY rather than a package list: `T{}` renders `new T(nil)` — go2cs-gen's dedicated nil constructor, which names no field — but the FIELDWISE overload stays a resolution candidate wherever it is visible, and binding a candidate means binding its parameter types. That constructor is `internal` for any struct with an unexported field, so outside its assembly and friends it is not a candidate at all (which is exactly why mime's `once = sync.Once{}` and testing/quick's `return reflect.Value{}, false` need nothing), while a root package's struct IS visible that way — recompiled into the test assembly, or reached through the white-box `InternalsVisibleTo` grant. math/rand/v2's `*p = ChaCha8{}` failed `CS0012 … 'chacha8rand_package.State' … assembly that is not referenced` at the `new ChaCha8(nil)` expression, with internal/chacha8rand named in no import list on either side.

A **root's own package is never an addition**, and the external variant makes that load-bearing rather than theoretical: `go/packages` names it `<pkg>_test`, which resolves to no importable package at all, so a `bytes_test` struct literal whose field type is declared beside it fails the whole conversion with F14b's loud `resolve test project dependency "bytes_test": package bytes_test is not in std`. Every root's `PkgPath` therefore seeds the already-referenced set (its types compile into the test assembly, or bind through the production project reference the template already carries).

`testing` is skipped as a walk **source** (`closureWalkable`): it binds to the hand-owned `core/testing` shim per F15b, whose C# declarations are authored by hand and share only *names* with Go's — Go's `testing.T` embeds a `common` holding `io.Writer`, `time.Time`, `sync.RWMutex` and a dozen more, none of which the shim's two-field `T` declares, so inferring C# edges from the Go declaration there is simply invalid. Every `-tests` compilation names `testing.T`, making this the widest over-inclusion the struct rule could possibly cause; nothing is lost, since the shim's reference is fixed in the project template.

Each interface step runs the same `types.Implements` candidate match the converter uses at the declaration site (identical exported / non-alias / non-generic / method-set / strictly-fewer-methods gates) — deliberately taken *before* that function's covered-by-embed skip and minimal-covering-set prune, so the result is a superset of the emitted base list and no emitted base's assembly can be missing. Only the declaring package's own imports are scanned, so a same-package base needs no separate visit (an interface implements its base's bases too, so those candidates are found directly), and the output is a sorted set, so the map-ordered walk stays deterministic. It is a pure project-reference concern (the manifest dependency list stays import-derived), empty for a package whose named types carry no foreign declaration edge (unicode/utf8/path/cmp/itoa → unchanged), and `{io}` for io/fs, hash/maphash and crypto/hmac. **Measured minimality:** regenerating every banked package's `.tests.csproj` and diffing is the instrument, and it is what rejected each looser rule in turn — an un-file-scoped seed drifted `compress/gzip` (context, crypto/tls, mime/multipart, net/http, net/url) and `go/token` (go/ast); a "named by value" struct edge drifted eleven more (encoding/binary, errors, hash/crc64, internal/fmtsort, math/rand, math/rand/v2, mime, os/signal, strconv, strings, testing/quick); an "any composite literal" edge — the empty form *unscoped* — still drifted three (encoding/binary, mime, testing/quick). Every one of those gates remains a **zero-drift** rule. The **root-scoped** empty-literal edge is measured the same way, and it is the one edge that is deliberately not zero: at the 63-package roster (2026-07-31), converting every package twice on one binary with that edge neutered and restored, it changes exactly **one** project — `math/rand/v2`, by exactly **one** line, `<ProjectReference Include="$(go2csPath)core/internal/chacha8rand/internal.chacha8rand.csproj" />`. That is the root set itself and nothing else: the three foreign-struct negatives (`mime`, `testing/quick`, `encoding/binary`) that rejected the unscoped form stay byte-identical, as does every other banked package. (Run that probe with the converter's exit status checked — 63/63 conversions must succeed on **both** sides: a conversion that *fails* writes no csproj, so an ignored failure reads exactly like "no drift", and that false-clean is how the `<pkg>_test` self-reference below first hid.) Guarded by the `TestSelectTestProjectModel`, `TestRecordsRequireProductionAnchorGatesReferenceModel`, `TestWriteTestProjectReferenceModelBindsProductionProject`, `TestReferenceModelSeedAnchorsTestClassOnly`, and `TestDeclarationClosureImportsSurfacesForeignDeclarationEdges` (foreign-package interface base, own-package structural base, transitive `b → a → io`, the `fmt.State` narrowing with its positive control, the empty-closure case, the `*rand.Rand` struct field at a composite literal **with its by-value-only negative**, a func-typed field's signature, the one-level field boundary, the compile-excluded-file negative with its own positive control, the external-variant self-reference negative, and the `testing`-is-never-walked negative) converter unit tests; unicode (28 tests, incl. `TestSpecialCaseNoMapping` and the `testing.Benchmark`-driven `TestCalibrate`) is the first package it validated under the reference model, and crypto/hmac (172 tests) the first the generalized foreign-package rule unblocked. (io/fs compiles clean under this closure but does not yet *validate*: its `TestCVE202230630` globs a 10 001-separator pattern whose faithful `globWithLimit` recursion overflows .NET's fixed thread stack before Go's `depth > 10000` guard fires — a Go-growable-stack vs .NET-fixed-stack divergence in the test host, orthogonal to the reference closure.)

**The third closure edge — a MEMBER ACCESS (2026-08-03, r38-gob).** The two edges above are edges of a named
TYPE's declaration; the third is the edge of an **access**: resolving `x.M` requires BINDING `x`'s type, and
when `x` is declared in another package that type is spelled nowhere in this compilation — not in an import,
not in an alias `using`. `unique` is the witness: `handle.go` declares `var cleanupMu sync.Mutex`, the
white-box suite calls `cleanupMu.Lock()`, and the test project referenced no `sync` — `handle_test.cs` failed
`CS0012 … 'sync_package.Mutex'` twice, no host linked, and the package's suite had never been measured. Adding
the production package's whole import list instead is the looser rule the reference model exists to avoid (it
would have added `internal/stringslite` and `unsafe` here, and far more elsewhere). The seed is every
`*ast.SelectorExpr`'s BASE type, `namedTypesIn`-expanded, then `reach()`ed and `enqueue()`d like a field edge;
a package-QUALIFIED selector (`sync.Mutex`, `lib.F`) is not this shape at all — its base is a `PkgName`, which
has no type — and the import that spells it already carries the reference.

**Measured minimality — two restrictions, each of which the roster REJECTED a looser form of.** The probe is
the one this section prescribes (regenerate every banked package's `.tests.csproj` and diff, with the
converter's exit status checked on both sides: 73/73 conversions must succeed, since a failed conversion
writes no csproj and reads exactly like "no drift"). (1) *The receiver, not every named declaration.* "The
type of every var/const/func the compilation NAMES" is equally true of C#'s binding rules in the abstract, and
drifts **23 of 73** — `bufio` into compress/bzip2 and image/gif, `internal/abi` + `internal/reflectlite` into
errors, three references into hash/crc32 — all of which compile clean today with none of it. Naming a
declaration does not force its signature to be materialized; accessing a member of it forces the receiver's.
(2) *`_test.go` files only, scoped per FILE.* Under the reference model the production sources are not in this
compilation — they are in the referenced assembly, which carries its own references — and seeding from them
too still drifts **13** (crc32's `castagnoliOnce.Do`, math's `cpu.X86`, …). Per-file rather than per-package
because go/packages loads the INTERNAL test variant with the production files alongside its own, so a
package-level gate lets every production receiver back in. With both restrictions the roster is **zero-drift**:
unique's single `<ProjectReference Include="$(go2csPath)core/sync/sync.csproj" />` is the only line that
changes across all 73. Under the **recompile** model the edge is a no-op by construction — that model adds
`production.Imports` wholesale, so a production receiver can never be an addition. Guarded by
`TestDeclarationClosureImportsSurfacesMemberAccessEdges` (the unique shape through real test variants, plus
the named-but-not-accessed, production-source-only and package-qualified negatives). One more rule fell out
of the family's own `testing`-never-walked negative: `testing` must never be an ADDITION either, for the same
reason it is never a walk SOURCE — its reference is fixed in the project template, which is why the caller
strips `"testing"` from the import-derived set rather than passing it through as already-referenced, so
`reach()` now honours `closureWalkable` too.

**The fourth and fifth closure edges — a ZERO-VALUE DECLARATION and a RECORDED INTERFACE BASE (2026-08-07, r43f).**
`log` and `go/scanner` sat on the build-blocked list with one CS0012 family each, and each named a demand the
first three edges structurally cannot see.

*`log` — the zero-value declaration.* `TestNonNewLogger` writes `var l Logger`. There is no composite literal
anywhere in it, so the struct-field edge's `*ast.CompositeLit` seed never fires — but Go's zero value of a
struct is not `default(T)` in the emission. The converter renders the declaration as a **constructor call**:
`ref var l = ref heap(new Logger(), out var Ꮡl)` for the address-taken shape (`escapeAnalysisOperations`),
`new Logger()` otherwise (`visitValueSpec`). C# overload resolution must materialize every **accessible**
constructor's signature before it can choose one, and under the white-box `InternalsVisibleTo` grant the
package-under-test's `internal` fieldwise overload is accessible — so `log_test.cs` failed
`CS0012 … 'atomic_package.Pointer<>' … assembly 'sync.atomic'` at `new Logger()`, `Logger`'s three `atomic.*`
fields being spelled in no import list on either side. This is the **empty-literal edge's exact demand by
another route**, so it feeds the same seed (`constructedEmpty`) under the same ROOT/accessibility gate, scoped
to `_test.go` files for the member-access edge's reason. `math/rand/v2`'s `*p = ChaCha8{}` and log's
`var l Logger` are now one rule.

*`go/scanner` — the recorded interface base.* A converted **concrete** type names no interface in its own
emitted declaration — `partial struct ErrorList /*[]ж<ΔError>*/;` names none at all. Its bases arrive
as the **VALUE-form `[assembly: GoImplement<T, I>]` records** its package emits, which the go2cs-gen
`ImplementGenerator` realizes as `partial struct ErrorList : global::go.sort_package.Interface` **inside the
declaring assembly**. The metadata type therefore *declares* that base, and binding **any** member on it makes
the compiler resolve the base list: `list.Sort()`, `len(list)`, `Ꮡlist.RemoveMultiples()` and the generated
`ErrorList`→`error` value adapter's own `m_value.Equals(…)` all failed
`CS0012 … 'sort_package.Interface'`, thirteen times across the suite, with `sort` in no test import and no
alias `using`. The edge hangs off the **member-access seed** the third edge already computes — it is what
binding that receiver additionally costs — and interfaces are excluded from it because the base walk covers
them already.

**Measured minimality — and here the instrument rejected the rule C#'s binding rules appear to justify.**
"The interfaces the receiver's type implements, taken from the declaring package's imports" is the natural
`go/types` statement of the edge, mirroring `interfaceBaseCandidates` one type-kind over. It drifts **16 of
the 96** banked projects. The reason is that a `GoImplement` record exists only where the converter converted
a **cast**, so Go satisfaction wildly over-approximates the emitted base list: `os.File` satisfies
`syscall.Conn` and handed `syscall` to thirteen projects (compress/{flate,gzip,lzw,zlib}, image and four
image codecs, io, math/rand/v2, regexp, strconv) — while `os` records `File` only against `io/fs.File` and
`io.Writer`, and both **POINTER-form**, which generates an adapter *class* (`FileжWriter`) rather than a base
on the type and so demands nothing of a member binding; `bytes.Buffer` satisfies most of `io` and handed `io`
to `sort` and `unicode/utf8` though `bytes` emits **no records at all**; `internal/buildcfg`'s `Stringer`
handed it `fmt` from an equally empty set. All sixteen compile clean today with none of it. So the **records
are the gate** and satisfaction merely supplies the candidate universe: `packageImplementBases` parses the
package's own freshly-written `package_info.cs` (value form only — `parseExportedValueImplementLines` already
drops the pointer form), keys the interfaces by the implementing type, and the walk fires only where the
receiver's type *and* the candidate's package both match a record. Keyed **per type**, because os's one
genuine `syscall` record is for `rawConn`, not `File`. With that gate the roster is **zero-drift**: converting
all 96 banked packages on both binaries changes not one line of one `.tests.csproj`. The lookup is scoped to
the package under test, whose `package_info.cs` this run has just written; a foreign type's base list is its
own package's record set in its own `package_info.cs`, and no measured case has ever demanded one — widening
is the same lookup pointed at that file.

*`go/types` — the widening, measured.* That last sentence held until `go/types`' own suite, whose host build
produced exactly one error: `check_test.cs(78,66): CS0012 … 'sort_package.Interface'`, at `len(list)` in
`if list, _ := err.(scanner.ErrorList); len(list) > 0`. The type is `go/scanner`'s `ErrorList` — the very
type the same-package edge was built for, one package over. `go/scanner` **is** referenced (the suite imports
it) so `ErrorList` binds; what does not resolve is the base list `go/scanner`'s **own** record set realizes on
it, and `sort` sits in `go/types`' PRODUCTION project references (the package imports it) and in no test
import. So the records are now read **per declaring package**: a root type's from the `package_info.cs` this
run just wrote, a foreign type's from that package's, resolved through the same `getImportPackageInfo` route
the `<ImportedTypeAliases>` block already reads a dependency's metadata by (layout L3's per-GOOS placement
included) and memoized per import path. An unreadable or absent file yields no edge, exactly as it does for
the package under test. The gate itself is untouched — same value-form records, same per-type key, same
candidate match — which is why the widening does not reopen the 16-of-96 question: it points one lookup at a
different file.

*The other half: the member bindings that spell no selector.* The same measurement moved the edge's SEED as
well. `check_test.go` never writes `list.Sort()` — it binds `ErrorList` only through `len(list)` and
`range list` — so the member-access seed (`x.M`, the third edge's) never saw it. Each of those lowers to a
member on the value's type all the same: golib's generic `len`, the emitted enumeration, an indexer. The seed
therefore also carries the types a compiled `_test.go` binds a member on through a **builtin call, a range, or
an index/slice** (`memberBound`), and that set feeds the implemented-interface edge **only** — never
`reach`/`enqueue`, because the value is spelled by a package the suite already imports. The boundary the
member-access edge measured stays exactly where it was, and is pinned by the same negatives: a test that
merely NAMES a value of such a type, or PASSES IT ALONG to a function with an exact parameter type
(`var r Rows; Order(r)`), binds nothing on it and surfaces nothing. Seeding this edge from every named type
instead would cross that line — the existing negative fails immediately — which is why the shape is the
member binding rather than the mention. `go/types`' reference set gains exactly one line, `core/sort`, and the
host builds clean. (Guarded by `TestDeclarationClosureImportsSurfacesForeignImplementedBases` — the foreign
positive plus four negatives (no resolver, empty record set, a record keyed to another type, and the
name-and-pass boundary) and the range/index forms — and by
`TestForeignImplementBasesResolverReadsDeclaringPackageInfo`, which pins the value-form-only parse, the
memoization, and the silent-nothing on an absent file. Both fail against the pre-fix converter, and each half
of the fix fails it independently.)

Guarded by the `TestDeclarationClosureImportsSurfacesZeroValueVarDeclarations` (the log shape through real
test variants, plus the foreign-struct accessibility negative and the production-source scoping negative) and
`TestDeclarationClosureImportsSurfacesImplementedInterfaceBases` (the scanner shape, plus the three negatives
that pin the gate: **no records ⇒ no edge** even where `types.Implements` says yes, a record keyed to another
type, and the receiver restriction) converter unit tests. `go/scanner` **validates 11/11** on the fix.
`log` builds and runs for the first time — seven of its nine test functions agree with `go test` — but does
**not** bank: two roots stand behind the closure one, `runtime.Caller`'s unimplemented `getcallersp` stub
(`TestAll`, the same row `testing/slogtest` carries) and `TestDiscard`'s exact allocation-count assert
(the established `alloc-profile` class). ⚠ The `Caller` root closed on 2026-08-07 and `log` **still** does
not bank — `TestAll` asserts the GO source file's own extension and line numbers, which the converted
program does not carry; see *`runtime.Caller` works by severing the FUNNEL* below.

**The PROMOTED-METHOD edge (2026-10-04).** A struct a TEST source declares with an embedded struct compiles into the test
assembly together with the forwarders go2cs-gen emits there for every method that embed promotes (its "Promoted Struct
Receivers", read from the embed's metadata), and each forwarder's signature spells the method's parameter and result
types. go-cmp's `cmp/internal/function` suite declares `type myType struct{ bytes.Buffer }` and imports `bytes` but not
`io`; `bytes.Buffer`'s `ReadFrom`/`WriteTo` take an `io.Reader`/`io.Writer`, and the compile failed
`CS0234 … 'io_package' … namespace 'go'` ×4 in the generated `myType.g.cs`. The edge (`embedders` seeds, walked with
`embeddedStructs`) reaches every named type in the promoted methods' signatures, with the generator's own membership:
exported methods always, unexported ones only when the embed shares the embedding struct's package. It is
`_test.go`-scoped for the member-access edge's reason (a production struct's forwarders are generated in the production
assembly), and embedded INTERFACES are not walked. **Measured zero-drift** across the 229 committed `.tests.csproj`
(a census of every test-declared embed against each project's committed references: 0 additions), with the census made
to fire by deleting `io` from every reference set (3 hits: `go/types`, `io`, `net/http`, each a test type embedding
`bytes.Buffer` in a project that already references `io`). Guarded by
`TestDeclarationClosureImportsSurfacesPromotedMethodSignatures` (red before the edge) and
`TestDeclarationClosureImportsPromotedEdgeIsEmbedAndTestScoped` (a NAMED field and a PRODUCTION embed surface nothing).

**The PROMOTED-FIELD edge** is the same generator's other half. Besides a forwarder per promoted method, go2cs-gen
writes a ref accessor per promoted FIELD of the embed ("Promoted Struct Field Accessors") in the test assembly, and each
accessor spells the field's type. testify's suite tests embed `suite.Suite`, whose unexported `mu sync.RWMutex` is a
legal promoted field in the white-box variant (it is the same Go package); `sync` is imported only by the production
`suite.go`, so the accessors failed `CS0234 … 'sync_package'` in all 13 generated suite files. The edge walks each
embed's direct fields with the same membership rule as the methods: exported fields always, unexported ones only when
the embed shares the embedding struct's package. A foreign embed's unexported fields add nothing, which is why go-cmp's
`myType{ bytes.Buffer }` still surfaces `io` alone. The standard library has three test-declared embeds the edge reaches
(`net`'s `lookupCustomResolver` needs `internal/singleflight`; `runtime`'s `GCController` and `TraceMap` need
`internal/cpu` for a padding field), and each of those test projects already references the package through another
edge. Guarded by `TestDeclarationClosureImportsSurfacesPromotedFieldTypes`.

## Under the RECOMPILE model the test half CONTINUES the production emission (the `productionSeed`)

The `recompile` model is the only one where the converted `_test.go` files land in the **same C# class**
as production sources this run does not rewrite. Everything the converter numbers or claims per package
is therefore a shared, immutable name supply, and every counter that restarts for the test emission pass
re-mints a name already on disk. Two such supplies were already pinned — lifted type names
(`productionLiftedTypeNames`) and hoisted big-constant ordinals (`productionHoistedConstOrdinals`) — and
the same rule turned out to be owed by three more. All five now travel in one `productionSeed` struct,
captured in `convertTestVariants` from the production run's live state (it ran moments earlier in the
same process) **before** the first variant's `resetPackageState`, and installed by `convertTestVariant`
for the **INTERNAL** variant under the recompile model alone:

| Supply | Emitted name | The collision |
|:--|:--|:--|
| lifted type names | `Δtypeᴛ1` | two lifts of differently-shaped anonymous structs (encoding/gob) |
| hoisted big-constant ordinals | `maskᶜ1` | two `const mask = <big>` hoists |
| import force hooks | `initᴛᴛimportꓸcryptoꓸsha256` | production and test files repeating one import |
| the blank-identifier counter | `_ᴛ1ʗ` | a blank package-level `_` in each half |
| `func init()` ordinals | `init` / `initΔ1` | production and test files each declaring `func init()` |

The last three are `crypto/x509`'s, and they are ordinary Go, not exotica. `x509.go` and `x509_test.go`
both `import _ "crypto/sha256"` and `_ "crypto/sha512"` — a test repeating a production blank import is
what a test that exercises those registrations does — and each half emitted the same
`[GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256()` into `x509_package`: CS0111. The
`x509_package` class likewise already held `_ᴛ1ʗ` for `pem_decrypt.go`'s blank const heading an iota
block when `oid_test.go`'s `var _ encoding.BinaryMarshaler = OID{}` re-minted it (CS0102), and
`root_windows.go`'s `func init()` when `x509_test.go`'s own `init` claimed the bare name again (CS0111).

The import hook is the one whose OWNERSHIP is worth stating rather than merely its uniqueness:
exactly one hook per (assembly, imported package) — Go initializes an imported package once per program
and a .NET module constructor runs once per assembly — and the **production** half owns it whenever its
file is in the compilation, because that file is the one a `-tests` run cannot rewrite. The seed is
skipped for the EXTERNAL variant and for both reference models for one reason, stated once: there the
names land in a different class (`<pkg>_test_package`, the friend bridge) or a different assembly
(production, referenced), so they may be reused freely and seeding would only churn banked emissions.

Guarded by `TestTestVariantPinsProductionBlankImportForces`,
`TestTestVariantContinuesProductionBlankIdentifierCounter` and
`TestTestVariantContinuesProductionInitOrdinals`, each pinning both directions — unseeded the test half
legitimately takes the first name, seeded it must step past the production one.

## A recompile-model test project compiles the production sources — so it owes their references and their per-GOOS half

Two more `crypto/x509` roots, both of the same shape: the recompile model makes the production `.cs`
compile items of the test project (`writeTestProject`), and two places that enumerate or probe those
files did not describe what actually compiles.

**The B2c alias scan read only the test-emitted files.** The tests csproj sets
`DisableTransitiveProjectReferences`, so every assembly the compilation names must be a DIRECT reference,
and the alias scan is what finds the ones no import list mentions (see the reference-closure rule above).
Under the recompile model a production file's `using` aliases are references the TEST project owns — and
they were never scanned. The omission hides in the ordinary case, because a production file's aliases are
usually its own package's direct imports, which the import-derived set already carries; it bites where the
alias names a package reached only transitively. `x509.cs` and `pem_decrypt.cs` emit
`using hash = hash_package;` because `crypto.Hash.New()` RETURNS `hash.Hash` — `hash` is in no import list
of `crypto/x509` and in no reference of its own production csproj, which compiles anyway precisely because
it does *not* disable transitive references. The test build failed **CS0246 ×2 inside the production
files**. `testProjectAliasScanFiles` now names the scan set as "what the test project compiles", with the
production half included under the recompile model and excluded under the reference models (there those
sources compile in their own project and their aliases are that project's concern). Guarded by
`TestAliasScanCoversRecompiledProductionSources`, which pins the model gate as well as the find.

**The enumeration and the static-ctor probe were both flat-only, and layout L3 is not flat.** An L3
package keeps its platform-varying sources in `<goos>/` and its production csproj compiles one folder via
`$(GoTargetOS)/*.cs`; a test project lists compile items explicitly, so the same selection has to be made
when enumerating them (`productionCSFiles`) and when asking whether a production `package_init.cs` exists
(`platformLayoutPath`, the probe that decides whether the test side implements the erasable
`initᴛᴛtests()` hook or declares a static constructor of its own). `crypto/x509` is the corpus's only L3
package on the recompile model — every other L3 suite takes a reference model, where the production
ASSEMBLY carries its per-GOOS half — so neither gap had ever been exercised. Together they cost 187
errors reported against the TEST files rather than the missing folder (`Verify`, `VerifyOptions`' fields,
`loadSystemRoots`, `domainToReverseLabels`, every error type's `Error()`), plus a second
`static x509_package()` beside the real one. Guarded by
`TestProductionCSFilesTakeTheTargetPlatformFolder` (target folder taken, non-target folder not, per-GOOS
`package_init.cs` included, flat package unchanged) and `TestProductionInitProbeFollowsPlatformLayout`.

## An Example/Benchmark-ONLY test file is dropped from the compile set (Phase-4D file exclusion)

`Example` and `Benchmark` declarations are uniformly **Phase-4D-deferred** — `discoverTestDeclarations` records them in the manifest with status `unsupported` ("… execution is deferred to Phase 4D") and the differential oracle filters them from both sides (`eligibleTerminalTestResults` admits only `included` `test`-kind declarations). The **option-a ruling** (2026-07-24) extends that deferral from the *declaration* to the *file*: a `_test.go` file is dropped from the `-tests` conversion/compile set (`selectCompileExcludedTestFiles`) iff **both**

1. **every RUNNABLE declaration it contributes is a Phase-4D-deferred `func Example*`/`func Benchmark*`** — imports do not count as declarations, and (since 2026-08-15) neither do pure `type` declarations and methods; any top-level `var`/`const`, or any other plain func (a `Test`/`TestMain`/`Fuzz` func, an `init`, or a *mis-signatured* Example/Benchmark), disqualifies the whole file (conservative by design; `TestMain`/`Fuzz` are deliberately out of scope). The Example/Benchmark classification is the **exact** `isPhase4DExcludedTestFunc` predicate `discoverTestDeclarations` uses (no receiver, no results, no type params, and either a zero-parameter `Example*` or a single-`*testing.B`-parameter `Benchmark*`), so a file qualifies only when it truly contributes nothing to the run registry; **and**
2. **no RETAINED test file references any object the file declares**, resolved by go/types **object identity** across the loaded variant set (never filename or text) — a promotion **fixpoint** over both variants, so an Example a retained test takes by value (`[]func(){ExampleWired}`) keeps its file compiled, and a candidate promoted back to retained can in turn pull further candidates in.

The predicate is pure `go/ast`+`go/types`: go/token's `example_test.go` declares **only** `func Example_retrievePositionInfo()` at top level (its `type p = token.Pos` / `const bad` / `func ok` live inside a raw-string literal fed to `parser.ParseFile`), so a text scan would wrongly disqualify it while the AST predicate correctly qualifies it. Excluding it is the **demonstrated consumer**: that external `package token_test` file, recompiled into go/token's mixed whitebox+blackbox test assembly, names `token.FileSet`/`ΔPos`/`Token` — types the referenced `go.parser`/`go.ast` assemblies surface from the *production* `go.token` assembly while the recompile makes a *second*, local copy — CS0012. With the file excluded, go/token compiles.

**Metadata representation — discovery stays intact; only emission + compile-membership are dropped.** The excluded file's Example/Benchmark declarations still appear in the manifest under their existing disclosed-unsupported status, and its source is listed in `testSources` with a distinct `example-benchmark-only` status. This is *required* for oracle exactness, not cosmetic: `go test` **runs** an Example that carries an `// Output:` comment (go/token's does), so it appears in the raw `go test -json` stream, and the **F6 census gate** (`manifestCensusGaps`, computed over the *unfiltered* Go results) fails the comparison for any run declaration the manifest does not account for. Dropping the declaration would trip the census; keeping discovery — and dropping only the file's `.cs` emission and its `.tests.csproj` compile item — keeps every already-filtered Example filtered and the differential oracle exact. The predicate lives in `convertTestVariants` (both project models honor it): discovery runs over every test file, emission over the non-excluded subset.

**Blast radius (banked packages).** Because the policy changes which files a *banked* package compiles, a GOROOT scan of every committed test suite (with go/token as the positive control) found **18** packages with a qualifying file — each re-validated with identical `Test` counts and rebanked to remove the excluded `*_test.cs`, its `.tests.csproj` compile item, the project references + `using` aliases the excluded file **exclusively** pulled in (verified by import analysis, e.g. math/cmplx's fmt, encoding/hex's os/io.fs/log), and — where the excluded external file was the sole contributor to the external anchor — the orphaned `package_info_external_test.cs`. One subtlety is guarded operationally: the metadata writer **merges** with the committed anchor, so a `GoImplement`/`GoImplicitConv` record contributed *only* by the excluded file (math/rand's `GoImplement<text.tabwriter.Writer, io.Writer>`, from an Example that casts a `*tabwriter.Writer`) survives the merge as a stale record referencing a now-unreferenced assembly (CS0234); regenerating the anchor from a clean state (as a whole-corpus reconvert would) drops it. The 17 packages with no qualifying file stay byte-identical.

**Condition (1) admits pure TYPE declarations and METHODS** (2026-08-15, the `crypto/tls` lane). The original wording — every top-level declaration is an Example/Benchmark — is the shape go/token's `example_test.go` happens to have, and `crypto/tls`'s is the same file in every way that matters: the package's ONLY black-box file, every runnable thing in it an Example. It differs in one respect — its Examples need an `io.Reader` to hand `Config.Rand`, so it declares `type zeroSource struct{}` and one `Read` method — and that single helper kept the whole file compiled, producing precisely the failure this ruling exists to prevent: `http.Transport`'s `TLSClientConfig` field names `tls_package.Config` in the PRODUCTION assembly while the recompile makes a second local copy, so the field is unnameable — **CS0012 ×3** at `example_test.cs` 88/99/198, three of `crypto/tls`'s four build errors. Adding the production reference cannot fix that (the two `Config`s stay distinct types and CS0012 merely becomes CS0029); the file must not be compiled. A type declaration and its methods are admissible because they have no RUN-TIME behavior of their own — nothing executes at package init — and any use by a retained file is a reference condition (2) already resolves. That last clause is load-bearing and is why the type and method objects are now recorded in `declared`: widening condition (1) without it would have silently disarmed condition (2) for exactly the declarations it just admitted. Everything else stays disqualifying, deliberately: a `var`/`const` initializer can carry side effects and a plain helper func can be `init()`, neither of which any reference edge would reveal.

Guarded by the `TestSelectCompileExcludedTestFilesDropsExampleAndBenchmarkOnly` (positive: external Example-only + internal Benchmark-only), `TestSelectCompileExcludedTestFilesDropsExampleWithHelperType` (the crypto/tls widened-arm positive), `TestSelectCompileExcludedTestFilesKeepsHelperTypeUsedByRetainedTest` (condition 2 over the widened arm — the disarm this change had to avoid), `TestSelectCompileExcludedTestFilesKeepsExampleWithTopLevelVar` (condition 1 negative), `TestSelectCompileExcludedTestFilesKeepsReferencedExample` (condition 2 fixpoint), and `TestSelectCompileExcludedTestFilesKeepsTestMainAndFuzzOnly` converter unit tests.

## A package-qualifier `using` in a converted TEST SOURCE contributes a project reference

Test projects set `DisableTransitiveProjectReferences=true`, so an assembly the package reaches only *transitively* is invisible to the test compile (CS0234). `aliasReferenceImports` covers this by scanning `using` ALIASES for namespace tokens of packages in the transitive import closure and adding a direct project reference for each. It formerly scanned only the two **metadata** files, despite its contract covering a file-local package-qualifier `using`; it now scans the converted `*_test.cs` **outputs** as well.

The shape this misses otherwise has no textual import at all: math/rand's `default_test.go` does not import `os/exec`, but `testenv.Command(…)` **returns** `*exec.Cmd`, so the emitted `default_test.cs` binds `cmd.Value.Env` and `cmd.CombinedOutput()` through the os/exec assembly while `exec.` never appears in any import list. Scanning the emitted source finds the qualifier `using` and emits `$(go2csPath)core/os/exec/os.exec.csproj`. The manifest's dependency list stays import-derived — alias targets are purely a project-reference concern — and the scan is additive, so a package whose sources introduce no new qualifier is unchanged.

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
`go/types` (`selectorBasePackageObj`, consulted by `aliasResolvedSelector`), so a shadowing binding is
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

## A module hosted under `go.` shadows the root namespace the way a `go/*` import does

A package's C# namespace is its import path, so a module whose first host label is `go` — `go.yaml.in/yaml/v3`,
`go.uber.org/zap`, `go.opentelemetry.io/otel` — lands under `go.go`, exactly as the standard library's `go/ast`
and `go/types` do. Any compilation that references one has a `go.go` member in namespace `go`, and C# binds the
leading `go` of a using target inner-to-outer, so inside any `go.*` namespace a bare root-qualified alias binds to
it and fails (CS0234). The converter already roots those targets with `global::` when a `go/*` package sits in
the import closure; a `go.`-hosted module in the closure now raises the same flag:

```go
import (
	"runtime/debug"
	"unicode/utf8"
	"go.shadowlib.in/lib"
)
```
```csharp
namespace go.GoHostModuleShadow;

using debug = global::go.runtime.debug_package;
using utf8 = global::go.unicode.utf8_package;
using lib = global::go.go.shadowlib.@in.lib_package;
using global::go.runtime;
using global::go.unicode;
```

The qualification is per package and only where it is needed: a package whose transitive import closure holds no
`go/*` package and no `go.`-hosted module emits its aliases exactly as before. The host is not renamed instead,
because a module's namespace is its import path, and a package already converted under that path would keep it.
This is what kept testify (which imports `go.yaml.in/yaml/v3`), and through it logrus and cobra's `doc` package,
from compiling. (Guarded by the `GoHostModuleShadow` behavioral project, a consumer package that imports a
`go.`-hosted module beside `runtime/debug` and `unicode/utf8`: four CS0234 before the change.)

## A file that reaches Go names through `using static` binds .NET types by alias, never by namespace

Two emissions need a .NET namespace on demand: a frame kept for `runtime.Callers` or a goroutine's
creator is marked `[MethodImpl(MethodImplOptions.NoInlining)]` (`System.Runtime.CompilerServices`), and a
struct with a zero-size field is laid out at Go's offsets with `[StructLayout]` / `[FieldOffset]`
(`System.Runtime.InteropServices`). Where a file reaches Go names bare through a `using static`, the
namespace is **not** imported; the file gets one alias directive per type the emission names instead:

```csharp
using static global::go.go.types_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
```

The attribute text the reader sees does not change. A file with no `using static` keeps the plain
`using System.Runtime.CompilerServices;`.

**Why.** A `using static` member and a namespace-imported type sit at one level of C# name lookup, so a Go
name the namespace also declares is ambiguous (CS0229): go/types' `stdlib_test.go` reaches go/types'
`Unsafe` through its dot-import, and broke the day `TestStdlib` took the attribute. An alias directive
outranks both, so it can never collide with a Go name.

**Which files.** Two triggers. A file with its own Go dot-import. And every file compiled into a test
project whose production is a referenced assembly: that project seeds a `global using static` of the
production class (and of the white-box bridge) at compilation-unit level, OUTSIDE the file's
file-scoped namespace — where a namespace import inside the file would not collide with a Go name, it
would silently **win**, binding the .NET type. That case has no compile error to catch it, which is why
the trigger is the project, not the file.

**Guards.** `usingStaticNamespaceAlias_test.go` pins each trigger (a dot-import first and behind an
ordinary import, a white-box and a plain reference-model test project). The behavioral project
`UsingStaticNamespaceAlias` compiles and runs both alias sets beside bare `Unsafe`, `Closure` and
`Marshal`.

## A function called with a value whose type has a same-named method is cast to its parameter type

Go resolves `Sort(x)` to the package function `Sort(data Interface)` even when `x`'s type has a method
named `Sort`. In C# that method is the extension `Sort(this StringSlice x)`, an ordinary static member of
the same package class as the function. A bare `StringSlice` argument makes the extension the better
overload, because an exact match beats the boxing conversion to `Interface`, so the call binds the method.
sort's own convenience methods are exactly this shape, and they called themselves:

```go
func (x StringSlice) Sort() { Sort(x) }
```
```csharp
public static void Sort(this StringSlice x) {
    Sort((Interface)(x));
}
```

The converter casts the first argument to the function's interface parameter type wherever the argument's
type, or a named type in the function's package whose underlying type it is, declares a method of the same
name taking one fewer parameter. An argument emitted through an interface adapter (`new …ᴠInterface(x)`)
already binds the function and is left alone. So is a simple-name call reached through `using static`,
because C# does not import extension methods as simple names. Before the change, every `.Sort()` on
`IntSlice`, `Float64Slice` or `StringSlice` overflowed the stack, which is how pflag's flag sorting, and
cobra through it, failed. A bare `sort.Sort(sort.StringSlice(v))` in user code did too. (Guarded by the
`SortMethodSelfCapture` behavioral project: the three methods, the bare qualified call, and a user package's
own `Sort` function beside a `words.Sort()` method. It overflowed on the first arm before the change.)

---

[← Multi-Assignment and Evaluation Order](multi-assignment.md) · [Index](README.md) · [Multi-Result Values and Comma-Ok Forms →](multi-result-and-comma-ok.md)
