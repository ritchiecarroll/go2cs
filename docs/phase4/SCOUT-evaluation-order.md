# SCOUT — evaluation order: gc runs a statement's calls before its plain reads, C# runs left to right

> **State: MEASURED (2026-11-12), lane C2, for COORD.** Measurement only. Nothing here changes the converter, golib,
> the generator or `src/core`. The question comes from H's BOARD row of 2026-11-12 (cgo Phase 2's `ordered` arm,
> `scaled(p, mutate(&p))`: go `200004`, go2cs `2004`), which COORD routed as a .4 parity candidate, scout first.
> Corpus Go is go1.26.10. The census reads the Go sources the corpus is converted from: the go1.26.10 standard
> library, and the behavioral tree at master `4490a52244`, whose Go sources are byte-identical to `4efe149858`. The
> probe ran on linux/amd64.

## 1. Answer

- **gc's unit is the statement.** Its order pass moves every call, every call-like builtin and every receive in a
  statement into a temporary ahead of the statement, in source order. The plain reads (variables, field selections,
  array elements, derefs, arithmetic) stay in the expression and run after all of those calls (§2). go2cs emits the
  expression as written, and C# evaluates it strictly left to right. So a plain read that comes before a call in
  the source sees the value BEFORE the call in go2cs and AFTER it in gc.
- **13 of 16 statement forms diverge** on a probe that puts a writing call after a plain read (§3). These include
  call arguments, binary operands, composite literals, compound assignment, the right side of a multi-assignment,
  send, conversion, string concatenation, single-value return, an assignment's left-side slice, and a multi-value
  return of a package-level variable. Only the multi-value return of a local is already handled, by
  `returnOperandOrder.go`.
- **The corpus has no instance outside the existing spill.** Every site the census finds was read by hand, or is in
  a class closed by analysis. None is a divergence go2cs does not already handle (§4). The only real instances are
  the two multi-value returns the converter already spills: `crypto/x509.ParseOID` and `runtime.newInlineUnwinder`,
  plus the behavioral guard `MultiValueReturnOrder`. H's repro is a probe built to show the order.
- **Option A, spilling the calls into temporaries as gc does, reproduces gc on all 16 forms.** Measured: a
  hand-edited copy of the converted probe prints gc's output line for line (§5). Each spilled call costs one line,
  `var ᴛN = call;`, which keeps the Go expression readable.
- **Recommendation (§6): option A, gated by the predicate `returnOperandOrder.go` already uses, widened to every
  operand list in a statement, plus a one-level read of the callee's body.** Its corpus footprint is about 15 std
  production sites per target and 3 behavioral sites. Every one of them is semantically neutral, because none is a
  real divergence. Its value is parity for code the corpus does not hold (user packages and cgo callers), not a
  fix to the corpus.

## 2. What gc does

`cmd/compile/internal/walk`, go1.26.10:

- `order.go` `expr1`, the `case ir.OCALLFUNC, ir.OCALLINTER, ir.OCAP, ir.OCOMPLEX, ir.OCOPY, ir.OIMAG, ir.OLEN,
  ir.OMAKE*, ir.OMAX, ir.OMIN, ir.ONEW, ir.OREAL, ir.ORECOVER, ir.OSTR2BYTES, ir.OSTR2RUNES` arm (line 1338). It
  orders the call's own operands (`o.call`), then `copyExpr`s the result into a temporary that is assigned in a
  statement emitted BEFORE the current one. The only exception is a result assigned straight to a named variable
  (`x = f()`). `OAPPEND` (line 1376) and slice expressions (line 1392) are copied the same way.
- `order.go` `case ir.OANDAND, ir.OOROR` (line 1296) becomes `r = LHS; if r { r = RHS }`, so the right side
  (calls included) stays conditional and runs after the left.
- `order.go` `stmt`, the `case ir.OASOP` arm (line 703): `x op= f()` orders `f()` first and reads `x` when the
  statement runs.
- A map literal is filled entry by entry, so its values are not reordered against each other.
- `expr.go` `walkCall1` (line 637) then moves any argument for which `mayCall` (`walk.go` line 277) is true into a
  temporary before the call. `mayCall` covers index, deref, `.` through a pointer, division and modulus, all of
  which can panic. This orders panics, and changes no value a plain read sees.

So the context of a plain read is its whole statement: the reads of a call's arguments happen when that call is
made, after the calls nested inside its arguments. The spec allows this. It orders "function calls, method calls,
receive operations, and binary logical operations" left to right, and leaves the order of plain operands against
them unspecified. gc resolves that freedom the same way every time.

## 3. The probe: 16 forms, gc against go2cs

Every line reads a variable, then calls a function that writes it, in one statement. go2cs is the converter at
`4efe149858`, building with the golib of that commit.

| # | Form | Go | gc | go2cs |
|---|---|---|---|---|
| 1 | call args, `&V` | `scaled(p, mutate(&p))` | 200004 | 2004 |
| 2 | binary, captured closure | `n + bump()` | 53 | 4 |
| 3 | binary, global | `g + setG()` | 8 | 2 |
| 4 | call args, pointer method | `sum3(t.n, t.bump(), t.n)` | 1121 | 121 |
| 5 | composite literal | `[]int{g, setG(), g}` | 7 1 7 | 1 1 7 |
| 6 | index operand | `xs[reslice()]` | 9 | 9 (same) |
| 7 | assignment LHS slice | `ys[1] = swap()` (swap reassigns `ys`) | 5 4 | 5 5 |
| 8 | compound assignment | `g += setG()` | 8 | 2 |
| 9 | multi-assign RHS | `a, b := g, setG()` | 7 1 | 1 1 |
| 10 | send | `ch <- g + setG()` | 8 | 2 |
| 11 | map literal | `map[string]int{"x": g, "y": setG()}` | 1 1 | 1 1 (same) |
| 12 | conversion operand | `int64(g) + int64(setG())` | 8 | 2 |
| 13 | single-value return | `return g + setG()` | 8 | 2 |
| 14 | `&&` right side | `g == 1 && setG() == 1, g` | true 7 | true 7 (same) |
| 15 | string concatenation | `str + cat()` | Zb | ab |
| 16 | multi-value return, global | `return g, setG()` | 7 1 | 1 1 |

Form 6 matches because C#'s indexer takes the slice when it indexes, after the index call. Form 16 diverges because
the existing return spill deliberately leaves out package-level variables (`returnOperandOrder.go`, header, "SCOPE").
Form 7 shows that an assignment's left side counts too: C# fixes the target slice before the right side runs.

## 4. The census

### 4.1 The instrument

A `go/types` program reads every function body in the Go sources and walks each statement in C#'s evaluation
order. It records the plain reads and the calls. A **site** is a plain read of variable V that C# evaluates before a
call in the same context, where that call can write V. The context and the hoisting rules follow §2:

- builtins, conversions and slice expressions are hoisted calls that write nothing;
- `&&`/`||` keep their right side;
- an assignment's index, deref and pointer-selector bases are read before the right side;
- `x op= f()` reads x first;
- map literals are skipped.

Each site gets the strongest evidence that the call can write V:

- **T1, visible in the statement:**
  - the call passes `&V`, or calls a pointer method on V (`addr-arg`, `ptr-method`);
  - the read goes through a pointer, slice or map rooted at V, and the call receives that same reference as
    receiver or argument (`same-root-ref`);
  - the callee is a closure that assigns V (`closure-lit`, `closure-local`);
  - V is package-level and the call reaches, over the static call graph, a function that writes it
    (`callee-writes-global`, `reach-writes-global`).
- **T2, possible:**
  - V's address is taken elsewhere in the function, passed (`addr-passed`) or stored (`addr-stored`);
  - V is written by a closure whose value escapes (`captured-escaping`);
  - V is package-level and the call reaches a dynamic call or a body-less function, while other code writes V
    (`global-unknown-callee`, `global-addr-escapes`).
- **T3, heap:** the read goes through a pointer, slice or map, and the call is any call that does not receive the
  same reference.

Two refinements remove sites that cannot diverge:

- **The global walk is path-aware and closed-world.** A write to `trace.stackTab` does not reach a read of
  `trace.seqGC`. A global whose only writer is the reading function's own body, which nothing re-enters, is cleared.
- **Each T1 site with a local arm gets a one-level callee verdict.** `write` means the callee's body writes the
  part of V that is read. `nowrite` means it only reads through the parameter. `unknown` means there is no Go body,
  or the parameter is handed on. Taking the parameter's address counts as `write`.

**Controls.** Each refinement has a positive and a negative arm, and all were run before every census:

- the 16-form probe: all 13 divergent forms are flagged, plus form 6, which is a known over-count; the matching
  maplit and `&&` forms are not flagged;
- a negative file: a read after the call, a variable nothing writes, an unescaped local, a global read after the
  call, and a captured variable no closure writes all give 0 hits. `p.b + mutate(&p)` is T1 with
  `callee:nowrite`, because `mutate` writes `p.a`;
- a transitive file: a global written two calls deep is flagged; a read-only callee and a write to a different
  field are cleared; an escaped pointer and an interface call stay T2; a global written only in `main`'s own body
  is cleared;
- an escape file: an address passed and retained, an address stored, a method value and an escaping writer are
  each flagged; an in-place writer and a deferred writer are cleared;
- a same-root file: a pointer receiver, a pointer argument and a slice argument are flagged; a different root and
  a call that does not receive the reference stay T3; a write to another field gives `nowrite`; a hand-on gives
  `unknown`.

**Load.**

- std ran with tests on linux, windows and darwin, all amd64 (tags `purego,math_big_pure_go`), with 0 load errors:
  968, 966 and 966 packages.
- The behavioral corpus ran as one workspace over all 959 modules on the same three targets. Each module that does
  not type-check on one target, because it calls another target's syscalls, does type-check on another. So the
  union covers the corpus.

### 4.2 Counts

Sites per target (production / `_test.go`):

| Tier | std linux | std windows | std darwin | behavioral (each target) |
|---|---|---|---|---|
| T1 | 104 / 11 | 106 / 11 | 104 / 11 | 20 |
| T2 | 121 / 70 | 126 / 70 | 117 / 68 | 20 |
| T3 | 697 / 216 | 708 / 206 | 695 / 212 | 59 to 61 |

The T1 local arms with their callee verdicts, std production on linux (windows has two more `nowrite`):

| arm | write | nowrite | unknown |
|---|---|---|---|
| addr-arg | 0 | 2 | 2 |
| ptr-method | 2 | 5 | 2 |
| same-root-ref | 0 | 81 | 10 |

There are no closure arms and no global arms in std production. All 11 test sites are `nowrite`. In behavioral, 8
sites are `write`, 9 `nowrite` and 2 `unknown`, and 1 is `reach-writes-global`.

For scale: the sites where any plain read precedes any writing call in the same context, with no evidence that the
call writes the read, number 4,999 contexts in std production, 3,644 in std tests and 672 in behavioral (linux).
That is the footprint of an unconditional rewrite (§5, option C).

### 4.3 Triage: how many are real

| Class | Sites read | Real |
|---|---|---|
| std T1, all arms, three targets (union) | 19 statement-local + 99 same-root, all read | 0 new. `ParseOID` and `newInlineUnwinder` are multi-value returns already spilled |
| std T1 `callee:write` | 2: `regexp/syntax/compile.go:75`, `crypto/x509/oid.go:30` | 0 new. compile.go reads the pointer `c.p`, while `c.inst` appends to `c.p.Inst`; oid.go is `ParseOID` |
| std T2 globals | 25, all read | 0. No callee writes the global (array element stores, `print` traces, `fmt` calls) |
| std T2 stored or escaping, production | 30, all read | 0. No call can reach the escaped pointer or closure (`systemstack` and `Do(func…)` collectors) |
| std T2 stored or escaping, tests | 41, all read | 0. Append collectors and `fmt` calls |
| std T2 `addr-passed`, production | a sample of 18 of 72 | 0 |
| std T3, production | a sample of 25 of 747 (before the same-root split) | 0 |
| behavioral, all tiers but T3 | all read | 0 new. 5 are `MultiValueReturnOrder`, which the existing spill passes; `NamedTypeOverStruct:91` and `NamedArrayWrapper:169` take `&b.w` and `&s[i]` but write nothing that is read |

The two classes closed by sampling are the unproven part. In `addr-passed` the address is only ever passed
straight to calls, so a divergence needs one of those callees to retain the pointer and a later call in the same
statement to write through it. In T3 the call does not receive the reference, so a divergence needs a write
through an alias the statement does not name. 0 of 25 bounds the T3 rate below about 12% at 95% confidence. It
does not show the rate is zero.

## 5. Fix options and their cost

All options use the same emission unit. The probe's C# was edited by hand and built with the probe's golib, and
each variant was compared with gc's output.

**A. Spill the calls into temporaries in order (gc's rewrite).**

```csharp
var ᴛ1 = mutate(ref p);
println("args-addr", scaled(p, ᴛ1));      // 200004, as gc
var ᴛ1 = setG();
g += ᴛ1;                                  // 8, as gc
```

It applies to every form in §3. Measured: all 16 forms print gc's output. It is the shape `returnOperandOrder.go`
already emits (`var ᴛ1 = o.unmarshalOIDText(oid); return (o, ᴛ1);`), so a reader of the converted stdlib has seen
it. The cost is one line per spilled call. Every call up to the last hazardous one spills, so the calls keep their
order among themselves. The footprint depends on the gate:

| Gate | std production per target | std tests | behavioral |
|---|---|---|---|
| A0: statement-local evidence, the T1 local arms (the predicate the return spill uses today, widened to every operand list) | about 102 sites (104 T1, less the 2 already spilled) | 11 | 14 |
| A1: A0 plus a one-level callee read (spill on `write` or `unknown` only) | about 15 sites | 0 | 3 |
| A2: A1 plus same-package global writes (direct and transitive within the package) | +0 | +0 | +1 (`DeepSelectRecursion`, an array-element store that cannot diverge) |

A spill needs a statement boundary before the statement. Five contexts lack one:

- a `for` condition or post statement (it runs again on every iteration);
- the right side of `&&`/`||` (it is conditional);
- an `else if` condition;
- a `switch` case expression (evaluated lazily, one case at a time);
- a package-level variable initializer.

For those, the expression form `call is var ᴛN ? expr(ᴛN) : default` spills inside the expression. Measured on
forms 2 and 14: it gives gc's output, and it compiles in a condition position. It reads worse, so it belongs only
where a statement cannot go.

**B. Move the plain reads after the calls.** C# has one in-expression way to do this: named arguments, which C#
evaluates in the order they are written. `scaled(k: mutate(ref p), p: p)` prints 200004 (measured). This covers
form 1 only. Binary operands, composite literals, compound assignment, send, conversion, concatenation and return
have no such reordering, so B needs A anyway for 11 of the 13 forms. It also reorders the visible argument list
away from Go's, which costs more readability than one temporary.

**C. Apply gc's rewrite unconditionally.** This spills every call that shares a statement with a plain read: 4,999
std production contexts, 3,644 test contexts and 672 behavioral contexts (§4.2), roughly one changed line each,
across most of the corpus. It gives exact parity with no predicate to maintain, at the cost of the owner's
readability and parity rule for every converted file.

## 6. Recommendation

**Option A, gated as A1.** That is the access-path predicate `returnOperandOrder.go` already owns, generalized from
the multi-value return to every operand list of a statement, plus a one-level read of a same-package callee's body.
Cross-package callees and body-less ones stay conservative, so they spill. Closures bound once to a local are
statement-local evidence, and they cover H's second repro.

- **Footprint:** about 15 std production sites per target and 3 behavioral sites change emission by one line each,
  and none of them changes behaviour.
- **Gate:** one behavioral guard per divergent form, taking the 13 forms of §3 as written, plus a `for`-condition
  and an `&&`-right-side arm for the expression spill. Each guard has to be made red against today's converter
  first.
- **Order of work:**
  1. statement contexts and local variables (forms 1, 2, 4, 5, 7, 9, 12, 13, 15);
  2. package-level variables (forms 3, 8, 10, 16), which need the callee walk;
  3. the expression contexts, which have no statement boundary.
- **Not recommended:** C, because of its footprint, and B, because it covers only form 1 and reorders what the
  reader sees.

Nothing in the corpus needs this to be correct today, so the seat's urgency is set by where parity matters: H's cgo
exit gate is the first consumer that observed it.

## 7. Predictions, scored as posted (mailbox `cbf7a224e`, before the census)

| | Prediction | Measured | Score |
|---|---|---|---|
| P1 | T1 small: std production 20-80, tests 10-60, behavioral 5-20; mostly call args and binary, then LHS, complit rare | Under the posted T1 definition (no same-root arm): std production 13, tests 3, behavioral 9. With the same-root arm: 104 / 11 / 20. Forms: call args 31+12+7, LHS 25, binary 6, complit 3 | **MISSED** on std production and tests (below the range under the posted definition); MET on behavioral; form order MET for call args and LHS, MISSED for binary (6, not "most") |
| P2 | T2 is two orders of magnitude larger: thousands, mostly globals beside cross-package calls | 216 production and 335 test on linux before the refinements, 121 / 70 after. Globals were 94 production, 38 of them beside a cross-package callee | **MISSED**: one order of magnitude, and not mostly cross-package |
| P3 | T3 is the largest tier, tens of thousands | 697 to 788 production per target | Largest tier MET; magnitude **MISSED** |
| P4 | Of T1, at most half are real; many write an unrelated field | 0 real beyond the 2 already spilled; 88 of 104 `nowrite` | **MET** |
| P5 | H's two repros classify as T1 `addr-arg` and T1 captured closure | Probe lines 23 and 28: `addr-arg`, `closure-local` | **MET** |
| P6 | Gated spill costs 1-3 lines per T1 site; the reads-after alternative reads worse | 1 line per spilled call; B covers only form 1 | **MET** |
| red | Multi-target std loads fail, so linux runs first | All three targets loaded with 0 errors | did not fire |

P1 and P2 missed for the same reason. The prediction counted sites by how a variable is reachable, but the
instrument counts only reads that a LATER call in the same context could write, which is much narrower. The
same-root arm was not in the posted design. It was added after the T3 sample, because the converter's own return
predicate already treats `n.field` against `n.mutate()` as a conflict, and the census had filed that shape under T3.
