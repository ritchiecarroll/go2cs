# PLAN — the visual-parity train inside go.* 1.26.10.2

> **STATUS: RULED 2026-10-27.** Drafted at master `5931bad29b`. The owner accepted the six decisions in §6 as
> recommended, and COORD ruled the deferred-corpus window (§3.1), including the §3.5 deviation. The per-seat COORD
> rulings listed at the end of §6 are made seat by seat, before each seat's code.
> **The decision it serves (owner, 2026-10-27):** 1.26.10.2 carries all 17 items of the 2026-10-16 visual-parity
> review, the schedule slips as needed, and ONE full re-bank runs at the final tip. Medium items are wanted,
> above all L1 (collection expressions) and P1/P2 (field access and field address).
> **Sources read:** `go2cs-proposals/visual-parity-review.md` (whole), `docs/PLAN-release-1-26-10-2.md`,
> `docs/PLAN-comment-and-spacing-parity.md` (§0, §1, §4-§7), the `corpus-reconvert` and `train-assembly` skills,
> `GoCorpusMigration.md` H4a, `ConversionStrategies-Reference/pointers.md` (the extension-property section),
> golib `ж.cs` (`of`, `field`, `nativeFieldView`), the COORD notes on default rulings, the FieldView dispatches,
> the freeze fixed-point regen record at `f75e0c53ec`, and the test-refresh-1262 ruling.

---

## 0. The answer in one screen

- **13 seats (#20-#32), one instrument seat, then ONE end regen and ONE re-bank.** E1 is seat #19 (H), the golib
  AOT fix is #18 (G), and FieldView's AOT regression is R's open diagnosis. All three land or rule before the train's
  base is taken.
- **Merge strategy: deferred corpus.** Each seat lands converter, golib, gen, tests, docs and the goldens it moves,
  and records its corpus footprint as a measured RUNG (patch hash, numstat, per-item census), but commits no
  emitted `src/core` file. The end regen re-emits the corpus once (production on three targets, plus every roster
  row's `-tests` emission) and must equal `committed ⊕ rung #20 ⊕ … ⊕ rung #32` byte for byte (CR-normalised).
  Any residue is a finding. Section 3 checks this against the runbook and the skills.
- **Schedule:** about 26 lane-days of seat work, 7 lanes in parallel, serial landing on the i7. **Best case: publish
  Fri 2026-11-06. Likely: Fri 2026-11-13** (range Nov 12-16), with the owner's PIN as the last step.
- **Six owner decisions** remain open (section 6). Each has a COORD default, so no seat waits on one to start
  cutting.

---

## 1. The seats

**Footprints.** The table is a regex census at `5931bad29b`: one copy per file (layout L3), 3,221 production and
1,148 converted-test `.cs` files, and 925 goldens. It is ±10%, and it is a CEILING for items with an eligibility rule
(N1, C1, Z1, L1, R5), because the rule excludes sites the regex counts. Each seat's prediction of record is its own
two-seeded three-target `-stdlib` measurement (`corpus-reconvert` §1). These numbers only size the work. They run
lower than the review's, which were taken over a differently scoped file set, so they are re-measured here and not
carried over.

| Item | Pattern (regex) | Core prod | Core test | Goldens |
|:--|:--|--:|--:|--:|
| N1 | `[!=]= default!` | 9,105 / 928 files | 10,066 / 652 | 565 / 180 |
| D1 | `default!` | 34,094 / 1,369 | 17,778 / 762 | 1,571 / 385 |
| P1 | `(~p).f` + `p.Value.f` | 10,436 + 8,235 / ~900 | 6,694 + 2,226 | 529 + 302 / ~220 |
| P2 | `.of(T.Ꮡf)` (not `.Value`) | 7,497 / 519 | 801 / 81 | 280 / 79 |
| C1 | `(rune)'` | 7,949 / 347 | 1,966 / 169 | 171 / 53 |
| R5 | non-receiver `ж<…> Ꮡx` params | 5,065 / 793 | 10,369 / 866 | 200 / 105 |
| L1 | `new T[]{…}.slice()`, single level | 1,467 / 325 | 8,443 / 594 | 682 / 273 |
| Z1 | `new T(nil)` | 915 / 271 | 950 / 163 | 280 / 111 |
| H2 | `ref var x = ref heap<T>(out var` | 933 / 308 | 873 / 229 | 235 / 122 |
| S1 | `var exprᴛN =` (and `} while (false);`) | 1,309 (113) | 376 (7) | 50 (11) |
| N2 | `(basic)(basic)x` double casts | 383 / 143 | 62 / 30 | 20 / 11 |
| for(;;) | `while (ᐧ)` + `for (…; ᐧ ;…)` | 713 + 94 (core total) | 279 | 15 files |
| shift-assign | `<<=`/`>>= (int)(N)` | 257 / 91 | 69 / 18 | 2 |
| F1 | `}))(` | 6 | 96 / 41 | 64 / 36 |
| WithDirection | `WithDirection(` | 44 / 15 | 49 / 26 | 21 / 7 |
| E1 (#19, in flight) | `} else\n if (` + `){` at EOL | 834 + 3,863 | 408 + 1,216 | 5 + 78 |

The test-side column matters. It is the committed converted tests that the end regen refreshes (§3.4), and R5 and
L1 land mostly there.

### #22 — the parity census instrument (C1; tooling; lands first)

- **What:** `pcensus`, standard-library Go, one measure per item above. It counts the shapes in a corpus root and
  classifies each `-`/`+` line pair of a diff by the seat mechanism that explains it. A pair no mechanism explains
  is UNCLASSIFIED. It is the comment-parity train's `ccensus` pattern again: planted good and bad input, each
  planted site named exactly once.
- **Used by:** every seat's prediction (the count delta it predicts), the golden re-baseline check after each rebase
  (§3.3), the rung record (§3.2), and the end regen's third reading (`corpus-reconvert` §3: "the third reading from a
  different instrument").
- **Red-first:** the planted bad renderings fail, each naming its site; the good rendering reads zero.
- **Effort:** 1 lane-day. **Risk:** an instrument fault reads as a seat fault, so every arm is planted.

### #20 — N1 (stage 1) + D1: `err != nil`, and `default!` becomes `default` (P1, G reviews)

- **Changes:** converter only. The universe-`nil` arm of `convIdent.go` and `convBinaryExpr.go` gives N1. D1 is the
  `default!` literal at every site that emits it: 24 non-test converter files (`convBinaryExpr`, `convCallExpr`,
  `convCompositeLit`, `convIdent`, `visitAssignStmt`, `visitReturnStmt`, `visitSwitchStmt`, `visitValueSpec` and
  others). golib already has `operator ==/!=(object?, NilType)`.
- **Footprint:** the widest seat: ~52K `default!` lines across prod and test, and ~390 goldens. N1's lines are a
  subset of the same lines, so both ride one re-emit and one golden churn.
- **Design questions, before code:**
  1. **N1's operand predicate**, spelled in the brief: interface (including `error` and `any`) and func operands
     emit `nil`. Type-parameter operands keep the `IsNil` path. Pointers already emit `nil`. Slices, maps and chans
     are stage 2 (#24).
  2. **D1's scope:** every converter-emitted `default!`. Hand-owned files and go2cs-gen templates are left alone
     (COORD default; the gen templates are not visible emission).
- **Red-first:** behavioral guard `NilCompareShapes`, with the Output phase against `go run`: an error from a func
  returning a typed nil pointer, a nil func, `recover() != nil`, and an `any` holding a nil map. The Target phase
  is red at the base. A converter unit test pins the type-parameter exclusion.
- **Risk:** low. A wrong operand is a compile error (no operator), except type parameters, which are excluded.
  **Effort:** 1.5 lane-days.
- **Lands first among emission seats.** It touches 24 converter files and ~390 goldens, so every later seat rebases
  across it exactly once.

### #21 — Z1: `T{}` emits `new T()` (P1, after #20)

- **Changes:** converter, `convCompositeLit.go`. **Struct types only.** A named slice or map `T{}` is non-nil and
  keeps its form, because the `(NilType)` ctor of `InheritedTypeTemplate` means nil.
- **Footprint:** up to ~1.9K sites (prod + test) and ~111 goldens, fewer after the struct filter.
- **Design question:** none beyond the struct-only rule. A generic struct `G<T>{}` is included.
- **Red-first:** a golden arm for a struct, a generic struct, and a named slice `IntSlice{}` that must stay non-nil
  (Output phase prints `IntSlice{} == nil` as false).
- **Risk:** low. **Effort:** 0.5. **Order:** before #30 (L1), which edits the same function.

### #23 — C1: rune literals drop `(rune)` where C# takes a char (P2)

- **Changes:** converter: `convBasicLit.go`, plus the rune casts in `convCallExpr.go` and `visitAssignStmt.go`.
- **Footprint:** up to ~9.9K sites and 53 goldens. Byte targets change from `(byte)((rune)'x')` to `(byte)'x'`.
- **Design question (COORD):** the keep-cast list, stated as the rule. Keep the cast for byte, int8 and int16
  targets and on byte-switch labels (CS0266), for `any`/interface boxing (`%T` must read int32), for generic
  inference and overload resolution, for `var r = 'a'`, for runes above 0xFFFF, and for named and `uintptr` targets
  without a char conversion.
- **Red-first:** behavioral `RuneLiteralContexts`, in the Output phase. `fmt.Println('a')` prints 97 and `%T` int32,
  a byte switch, a rune above U+FFFF, a generic `max('a', r)`, and a rune parameter. Unit tests for each keep arm.
- **Risk:** low. Every wrong choice is a compile error, except the boxing and overload arms, which the rule keeps.
  **Effort:** 1.5.

### #24 — N1 stage 2: slice, map and chan compared to `nil` (P2, after #23)

- **Changes:** golib proofs first, then the converter widening of #20's predicate.
- **Design question (owner, §6.6):** in or out of .2. COORD's default is in, gated on the proof. The proof is a
  GolibTests arm for every construction kind of slice (managed, native-backed, `unsafe.Slice`, a reinterpreted
  view, `make`, empty literal, nil), map and chan, showing `IsNil` ⇔ `header == default`. One disagreeing kind
  (the review names native-backed slices) either keeps that operand type on `default` or is fixed in golib first.
- **Red-first:** the proof arms. The seat lands only on green. **Risk:** medium. **Effort:** 1.5. P2 holds the
  native-view knowledge from seat #16.

### #25 — R5: un-aliased pointer parameters keep their Go name (C2; heavy legs on the i7)

- **Changes:** converter. The alias decision ("does this function emit `ref var t = ref Ꮡt.DerefOrNull()`?") is
  computed in ANALYSIS, before the body is emitted. Every site that spells the box name consults it. Receivers stay
  `Ꮡl`.
- **Footprint:** ~15.4K params (prod + test), about 27K or more body uses, mostly `Ꮡt.Errorf` in tests, and ~105
  goldens.
- **Design questions:**
  1. **The convention (owner, §6.1).**
  2. **COORD:** the pass is wired into the ONE analysis sequence both the `-stdlib` and `-tests` drivers call
     (`golib-gen.md`: a pass wired into one driver binds nothing under the other).
  3. Interaction with `ʗ` capture snapshots and named pointer results: stated by test, not by argument.
- **Red-first:** a behavioral guard `PointerParamNames`: a param with no alias, a param with an alias (it keeps
  `Ꮡ`), a nested scope reusing the name (shadow rename), a closure capturing the param, and a `testing.T` method
  call. The Target phase is red at the base.
- **Risk:** low-med. The emission is a rename, but the decision must be made before emission. **Effort:** 2.5.
- **Order:** before #31 and #32, so P1 and P2 are cut and measured on the final parameter names (`b.Ꮡoff`, not
  `Ꮡb.Ꮡoff`).

### #26 — N2: a named numeric converts to any basic numeric in one cast (C2, after #25)

- **Changes:** go2cs-gen emits explicit operators from each `num:` named numeric to every basic numeric type.
  The converter drops the inner cast.
- **Footprint:** ~450 sites and 11 goldens.
- **Design question (COORD, G reviews):** the operator set, and where a new operator makes an existing cast
  ambiguous (CS0457) or changes which conversion binds in generic code. **The gate that answers it is the OLD
  corpus**: `go2cs-stdlib.slnx` compiles the committed corpus against the new gen, so any ambiguity the operators
  create reds there, named by site.
- **Red-first:** a GenTests arm on a uint8-underlying enum converting to `nint`, `float64` and `int64`, plus the
  IotaEnum golden.
- **Risk:** medium. **Effort:** 1.5.

### #27 — the small four: F1, WithDirection, `for (;;)`, compound shift-assign (H, after #19)

- **Changes:**
  - **F1:** golib adds `func(Action)` and the `Func<…>`/`Action<…>` arities. The converter emits
    `func(() => {…})()`.
  - **WithDirection:** golib adds a short spelling beside `WithDirection`, which stays for compatibility and
    reflection.
  - **`for (;;)`:** `visitForStmt` emits `for (;;)` for `while (ᐧ)`, and `for (init; ; post)` for `for (init; ᐧ ; post)`.
  - **Shift-assign:** the `(int)(N)` cast on a constant count is dropped in compound `<<=`/`>>=` (`visitAssignStmt`,
    `convKeyValueExpr`).
- **Footprint:** about 1,100 loops, 326 shift sites, ~100 IIFEs and ~93 directions. Goldens: ~36 F1, 15 loops,
  7 directions, 2 shifts.
- **Design questions (COORD):**
  1. F1's value-returning form: COORD's default is `func(nint () => …)()`, which reads like Go `func() int {…}()`,
     over `func<nint>(…)`.
  2. The direction spelling: COORD's default is `ch.RecvOnly()`/`ch.SendOnly()`. `ch.Recv()` reads like a receive
     operation.
  3. Shift-assign keeps the cast where the count is not a constant or the left operand is a named numeric (the
     gen count overloads from batch 43).
- **Red-first:** goldens PanicRecover (F1), SelectStatement (direction) and ForVariants (loops), plus a shift guard
  over named and basic left operands and a `%T` check on F1's value form. The loop arm proves no CS0161 on an
  int-returning method that ends in `for (;;)`.
- **Risk:** low. **Effort:** 1.5.

### #28 — S1: a non-constant tag switch becomes a C# pattern switch (H, after #27)

- **Changes:** converter, `visitSwitchStmt.go`. The shape is
  `switch (tag) { case var ᴛ when ᴛ == L: … break; … default: … }`. The `exprᴛN` temp goes, and so do the
  `do { } while (false)` wrapper and the `continueᴛN` label. Switches with `fallthrough` keep the if-chain.
  - **Rulings (COORD, 2026-10-28):** R1 one label per Go clause, joined with `||`; R2 each label keeps the chain's
    operator, so a constant-pattern clause is `case A or B:`; R3 `ᴛ` outermost, `ᴛ2`, `ᴛ3` by nesting depth.
  - **A2 (COORD, 2026-10-28, from the seat's compile arm):** a tag whose Go UNDERLYING type is string (named string
    types and `sstring` included) keeps its temp under the expressionless governor:
    `var exprᴛ1 = tag; switch (ᐧ) { case {} when exprᴛ1 == L: … }`. C# picks a switch's governing type through a
    user-defined implicit conversion when exactly one reaches a classic governing type, and golib's `@string`
    has one, to `System.String`: `switch (tag)` would compare decoded UTF-16 text, under which two different
    invalid UTF-8 byte strings are equal. A named numeric is re-governed to its underlying integer, which compares
    the same values, so it keeps the pattern switch.
- **Footprint:** ≤1.7K tag temps (not all are switches), 120 wrappers and 24 goldens.
- **Design questions:**
  1. **Scope (owner, §6.4).**
  2. **COORD:** a probe confirming that `when` guards evaluate in label order and stop at the first match when every
     label is `var ᴛ`, with side-effecting labels. Without that proof the seat does not cut.
  3. Every case body ends in `break`/`return`/`continue`/`goto`. The converter appends `break;` where Go's body
     ends normally.
- **Red-first:** SwitchBreakContinueWrapper, plus a new `SwitchLabelSideEffectOrder` guard (Output phase) and
  ExprSwitch and SwitchDefaultOrder unchanged in behaviour.
- **Risk:** medium. **Effort:** 2.

### #29 — H2: fold the heap declaration and its initializer (H, after #28; P1 takes it if H runs late)

- **Changes:** converter, `visitAssignStmt.go`/`visitValueSpec.go`. It emits
  `ref var a = ref heap<nint>(10, out var Ꮡa);` for a single-statement `x := rhs` whose box is not hoisted.
  - **Rulings (COORD, 2026-10-28):** R1 the type argument is always explicit, `heap<T>(value, …)`, never the
    inferred `heap(10, …)` first written here: inference mis-types (`10` reads int, not nint; an UntypedInt; a named
    wrapper), and the explicit form is the zero box's own spelling with the value inserted. R2 single-LHS `:=`
    defines only: a multi-LHS define (Go evaluates every RHS before assigning), a tuple deconstruction and a
    for-init box hoisted ahead of its loop keep the split.
  - **Measure-why (the seat's):** the split was structural, from the initial port (the `:=` path wrote the zero box,
    then the shared value emitter wrote `x = value;`), never semantic; golib's `heap<T>(in T, out ж<T>)` copies
    the value into the box as the assignment did, and the fold drops one `GoZero<T>()` per site.
- **Footprint:** ~1.8K pairs and 122 goldens.
- **Design question:** **measure why the split exists before cutting** (`train-assembly` §2: a dispatch naming an
  unmeasured mechanism is a measure-why order). The candidates are the anonymous-struct lift order, multi-value
  forms, closures, and `[GoValueClone]` structs, which hold array fields.
- **Red-first:** a guard over an array-field struct, a closure taking the address, a multi-value `:=`, and an
  anonymous struct.
- **Risk:** low-med. **Effort:** 1.5.

### #30 — L1: slice literals as collection expressions (R, after FieldView)

- **Changes:** golib: `[CollectionBuilder(typeof(slice), nameof(Create))]` on `slice<T>`, and
  `Create(ReadOnlySpan<T>)` returns a NON-NIL slice with `cap == len`. Converter: emit `[…]` in target-typed
  positions.
- **Footprint:** up to ~9.9K single-level sites (most in test tables), nested literals on top, and 273 goldens.
- **Design questions:**
  1. **Positions (owner, §6.3).** COORD's default: arguments, returns, fields, elements of a collection expression,
     typed `var x []T = …`, and `x := []T{…}` as `slice<T> x = […]`.
  2. **COORD:** the exclusion list as a rule. A converted Go callee has exactly one C# signature, and only those
     targets take `[…]`. golib builtins and generated overload sets keep today's form. So do generic inference
     sites, fixed arrays, keyed or sparse literals, and `var` with no stated type.
  3. **COORD:** named slice types (`type IntSlice []int`). Either gen adds a builder to the generated type, or they
     keep today's form. COORD's default is to keep it in .2 and size the gen arm.
  4. **Cost:** non-primitive elements gain one copy in `Create`. Measure it in the perf suite (JIT and AOT, i9),
     and measure compile time on the largest test table.
- **Red-first:** behavioral `CollectionExpressionSlices`, in the Output phase: `[]int{}` non-nil, `len`/`cap`, nested,
  `%T`, a keyed literal kept, a fixed array kept, an overloaded-callee site kept.
- **Risk:** medium (overloads; size). **Effort:** 3. R carries the perf instrument from FieldView.

### #31 — P1: `p.f` through the generated extension properties (G, after #18 and the FieldView ruling)

- **Changes:** go2cs-gen and the converter. Today's mechanism is the per-FILE `[GoFieldAccess(typeof(T), …)]`
  record, with go2cs-gen emitting one `extension(ж<T> ʗp)` block per type into an `internal static class
  GoFieldAccess` of the using compilation. The collision list is `fieldAccessCollisions.txt`. P1 widens that
  mechanism from the `unsafe.Pointer` class to every Go selector through a pointer to a struct.
- **Footprint:** ~27.6K sites (prod + test), ~220 goldens, and one record line per affected file (~900 files).
- **Design questions, all before code:**
  1. **The property body and its cost (COORD, from R's FieldView finding).** Today's `field` is a type test plus a
     delegate call through `T.Ꮡf` on every access. Under Native AOT that is the class of construct FieldView
     regressed on. COORD's default is a specialized body:
     `ref ʗp is NativeBox<T> n ? ref n.FieldRef(T.Ꮡf) : ref ʗp.Value.f`. The managed path is a direct field ref
     with no delegate, and it is gated on a hot-loop micro, JIT and AOT, against today's `(~p).f`/`p.Value.f`.
  2. **Semantics:** on a managed box this equals today's `p.Value.f`. On a NATIVE box it reads at the Go offset,
     where today's form reads the CLR slot. That is a correctness gain, stated in the seat, with its guard.
     `(~p).f` today copies the whole struct; the ref form does not.
  3. **The record's visibility (COORD; owner FYI).** A visible `[GoFieldAccess(…)]` line per file, or one
     per-package record. A per-package record must account for the test assembly recompiling production files.
     The record also keys the gen output, which is what keeps the OLD corpus untouched (§3.1).
  4. Collisions: the existing list, promoted-method shadowing, and hand-owned structs keep today's spelling.
     Getters are `[AggressiveInlining]`, so CallerLine and RuntimeCallerFrames see no extra frame.
  5. Build time: extension blocks per type per using compilation. Measure the stdlib build delta for the largest
     packages.
- **Red-first:**
  - behavioral `PointerFieldSelectors`: a field named `Value`, a promoted-method shadow (`Len`), a generic struct, a
    chain (`e.prev.next.x = …`), a parallel swap (ParallelAssignmentHazard), and a native-backed box read in the
    Output phase;
  - GenTests arms for the record forms;
  - the FieldAccessReinterpret golden unchanged.
- **Risk:** medium-high: the largest emission, a hot path, and perf. **Effort:** 4.

### #32 — P2: `&b.off` becomes `b.Ꮡoff` (G, after #31; R reviews perf)

- **Changes:** go2cs-gen adds `ж<F> Ꮡf => ʗp.of(T.Ꮡf)` beside P1's `ref F f`. The converter emits `b.Ꮡoff`.
- **Footprint:** ~8.3K sites (prod + test) and 79 goldens. It is FieldView's own hot shape.
- **Design questions:**
  1. **The spelling (owner, §6.2).**
  2. It is the same call and the same cached view as today, so it inherits R's `of()` fix. The perf gate is
     PerfFieldView, JIT and AOT, no worse than the base.
- **Red-first:** PointerToPointer and the container/list shapes in a guard. **Risk:** low once #31 lands.
  **Effort:** 1.

**Effort total:** ≈ 23 lane-days of cuts, plus ≈ 3 lane-days of rebase and golden regeneration (13 × 0.25).

---

## 2. Dependencies and landing order

**Hard edges:**
- #18, #19 and the FieldView ruling come BEFORE the train base (B0).
- #22 (the instrument) comes before any emission seat lands.
- #20 comes first among emission seats (24 converter files, ~390 goldens).
- #21 → #30, both in `convCompositeLit`.
- #23 → #24, same lane.
- #25 → #31 → #32, for parameter names and gen machinery.
- #27 → #28 → #29, same lane, two of them in `visitAssignStmt`.
- The FieldView finding comes before #31's design question 1 is ruled.

**Proposed landing order:**
#22 → #20 → #21 → #27 → #23 → #25 → #24 → #28 → #26 → #29 → #30 → #31 → #32 → **the end regen** → the re-bank.

Merge order is a property of seams (`train-assembly` §8). Two adjacent seats with disjoint converter files may swap
by readiness, but the hard edges never move. P1 and P2 sit late because they wait on FieldView and are the
biggest. Cutting proceeds in parallel at once: only MERGE order is serial.

---

## 3. The merge strategy: the deferred-corpus window

### 3.1 The rule

From B0 until the end regen, **no seat commits an emitted `src/core` file**. That covers converted `.cs`, `.cs.auto`,
`package_info.cs`, `.csproj` and `README.md`, production and test alike. Changes to golib and hand-owned files
(`[module: GoManualConversion]`, `*_impl.cs`) are allowed, **additive and compatible with the OLD corpus**. A
golib or gen removal waits for a cleanup seat after the regen. The land script enforces this:
`git diff --name-only <base>..<seat> -- src/core ':!src/core/golib'` must list only marked or `_impl` files.

Any other emission change that lands inside the window follows the same rule and becomes a rung of its own. That
includes a FieldView converter fix and any hotfix.

### 3.2 Each seat's rung (the measured, attributable footprint)

**At landing**, after the seat is rebased onto the previous rung's tip, the i7 runs:
1. A two-seeded `-stdlib -comments` emission on three targets: E(previous rung tip) against E(seat tip), from one
   binary per arm, with the four write assertions (`corpus-reconvert` §2).
2. The rung patch `P(#n) = E(prev) → E(seat)`, CR-normalised, its numstat per target, and its SHA-256.
3. `pcensus` deltas per item, with the seat's prediction scored as worded.
4. The rung's emission BUILT on all three targets. Windows builds the full stdlib solution; linux and darwin build
   with `-p:GoTargetOS`. This is the FORWARD gate: new corpus with new golib and gen.

**Recorded:**
- `docs/phase4/data/parity-1262/<seat>.tsv` (rung tip SHA, base SHA, converter sha256, go version, per-target
  numstat, patch SHA-256, census deltas, prediction and score);
- a BOARD row per seat;
- the patch itself on the LAN share by SHA-256. It is too large to commit, and it can be rebuilt from the rung tip
  with the pinned binary.

**This is the H4a "unbanked intended drift" ledger row** for that seat's deferral (GoCorpusMigration H4a: a deferral
to the next deliberate regen is complete only with its ledger row).

The lane's own measurement at its branch base is its PREDICTION. The rung of record is the i7's, at the true base.
Serial landing means each rung's base contains every earlier seat, so **compositions between seats are inside the
rungs by construction**.

### 3.3 Goldens: serial, and mechanical

Goldens cannot be deferred: CNR is the per-seat gate, and the behavioral `main.cs` equals its `.cs.target` (the
same blob). The method:

1. **Each seat branch carries its goldens as ONE final commit** ("goldens: re-baseline"), apart from source.
2. **On rebase**, drop that commit (`rebase --onto` past it). Never resolve a golden conflict by editing.
3. Rebuild the converter, then run CNR. The CHANGED set is the seat's emission at the new base. Run
   `--update-targets` for exactly that set (forcing the emission: the route #2 restore trap). Assert that the
   re-baseline diff is non-empty and that each golden byte-equals its on-disk emission.
4. **Check by set and by line:**
   - CHANGED equals the seat's recorded set, plus the projects master gained since the base ("a constant amount
     lower": explained, not a shortfall);
   - `pcensus` classifies every changed line pair as this seat's mechanism;
   - a pair that only a COMPOSITION explains (two seats' rules on one line) is legitimate. It is NAMED in the
     landing commit, never hand-edited.
5. The union CNR at the landed tip reads NO REGRESSION.

**This is mechanical**, because a golden is a function of (converter, test source, imported `package_info.cs`), and
the seat changes only the first. The one non-mechanical input is a seat that changes `package_info.cs` content,
which goldens read from the committed corpus. A seat that does so says so before it cuts (P1's record placement is
the candidate), and the post-regen union CNR is the gate (§3.5).

### 3.4 The end regen (the deliberate regen, H4a's second shape made concrete)

The regen runs after #32, with converter, gen and golib FROZEN (floor 4).

1. **Production, three targets:** a seeded emission at the final tip, merged as L3, called E(final).
2. **The ladder identity:** `committed ⊕ P(#20) ⊕ … ⊕ P(#32) == E(final)` per file, CR-normalised. The base half
   was true at `f75e0c53ec` (0 production differences) and is re-verified at B0 before #20 lands. Any residue is a
   FINDING: a rung recorded wrong, or an emission change that bypassed §3.1. It is attributed and never applied
   blind.
3. **Tests:** the `-tests` emission of every roster row into the same seeded root (the 2026-10-27 lesson: a
   `-stdlib`-only regen leaves converted tests stale). The test diff is classified by `pcensus`: the rung
   mechanisms, plus the standing test-refresh-1262 shapes a, b and c. UNCLASSIFIED is a finding. Three sample rows
   (strings, encoding/json, net/http) are also emitted at each rung tip as a positive control for the classifier.
4. **Apply** by the overlay (`corpus-reconvert` §4; this is a regen, not a converter train):
   - the path-precise marker gate, refreshed `.cs.auto` siblings, and the deletion pass;
   - `go generate .` in `src/go2cs` (`TestStdLibMetadataInSync`);
   - the comment-parity ratchet re-baselined as a STATED fixup (a rise is a finding, attributed by rung);
   - `docsamples -recite` and the corpus-cited doc samples refreshed (C2's doc-sample audit guard runs here).
5. **The full battery:**
   - go test, GenTests, GolibTests, CNR (it must read NO REGRESSION; a move here is a `package_info` composition,
     re-baselined as a named assembly fixup);
   - the full behavioral suite and both solutions;
   - three-target builds, the darwin census workflow, the perf suite under AOT (FieldView included), and the
     alloc-assert rows.
6. **Commit as one regen series:** production, tests, metadata. Each commit message carries the ladder arithmetic
   and the rung table. Push, then the re-bank starts at that tip.

**If Go publishes 1.26.11 before step 1**, the regen runs at the new pin as the patch hop's reconvert. It gains one
extra rung, "go-version" (the final converter at the old pin against the new pin), and the release is 1.26.11.1.
That is still one regen and one re-bank.

### 3.5 The candidate against the runbook and the skills

| Gate or rule | Needs the committed corpus to match the converter between seats? | Under the window |
|:--|:--|:--|
| CNR | No. It transpiles behavioral sources only, but reads imported `package_info.cs` from `src/core` | Per seat; the post-regen CNR catches a `package_info` composition |
| `go2cs-stdlib.slnx` | **It compiles the committed (OLD) corpus with the live golib and gen** | Per seat, as the BACKWARD gate: every golib and gen change is additive. N2's ambiguity and L1's builder are read here |
| Full behavioral suite | New test emission linked against the OLD corpus | Per seat when golib or gen moves. It is a compatibility check of the mixed state; that state never ships |
| Rung build, three targets | New corpus with new golib and gen | Per seat (§3.2 step 4), the FORWARD gate |
| `TestStdLibMetadataInSync` | Corpus against embedded metadata, both unchanged | Green per seat; regenerated at the regen |
| `TestCommentParityCensus` | It reads the committed corpus | Green per seat; re-baselined at the regen |
| `TestDocsSamplesHoldTheirLines` | A sample that no longer matches becomes a passing NOTE, so drift is silent | Seats update Reference rule text and golden-cited samples; corpus-cited samples are refreshed at the regen |
| route #7: `-tests` convert+build of `errors` and `reflect` | No | Per seat, at the rebased tip |
| `train-assembly` §5: "a seat whose footprint lands in banked packages owes those rows' sweeps as its own gate" | — | **Deviation, needs a COORD ruling.** The one re-bank is every seat's sweep gate, with ladder bisection on a red (§3.6), and with rung canaries for the riskiest seats |
| `corpus-reconvert` §3: the hunk rule binds a converter train | — | Satisfied in substance. Each rung IS the two-seeded delta at its true base, and the final apply is a regen over a verified fixed point, where overlay equals the hunks |
| `corpus-reconvert` §4: position maps belong to the regen | — | Consistent: the maps re-encode once, at the regen |
| PLAN-release-1-26-10-2 §2: "each seat carries its own `src/core` footprint" | — | **Amended by the ruling** for seats #20-#32 and anything landing inside the window |

### 3.6 Risks and the gates that keep the window honest

1. **A mixed-state gap.** New emission may need something the old corpus lacks: a cross-assembly generated
   member, or a `package_info` shape. This is caught both ways by the behavioral suite (new against old) and the
   rung build (new against new). P1's gen is keyed on the per-file record, which the OLD corpus does not carry, so
   the old corpus stays untouched by construction.
2. **A late behavioral red at the re-bank**, with 13 seats mixed together.
   - The corpus at any rung k is `committed ⊕ P(#20..k)`, so a bisect is a checkout plus an overlay per rung
     (`train-assembly` §6: "the intermediates already exist").
   - **Rung canaries on the i9** for #20, #25, #28, #30 and #31: overlay the rung's emission into a scratch clone and
     sweep a canary set (net/http, os, strings, bytes, strconv, encoding/json, container/list, sort) at about 1-2 h
     each.
3. **Silent subtraction in converter rebases** (D1 touches 24 files). Follow the `merge-hazards` and `train-assembly`
   §4 rules: both changes present BY NAME after the rebase, and a build plus a symbol count, never "0 conflicts".
4. **The i7 is the single rung box.** About 1.5 h of emission and build per rung, plus a 2-3 h battery, so 2-3
   landings a day at most. H's box runs batteries in parallel. The rung of record stays on the i7 (one binary
   identity, `corpus-reconvert` §2).
5. **Commits to `src/core` during the window by habit.** The land-script check in §3.1 refuses them.
6. **Alloc and perf.** P1 removes a struct copy, L1 adds one for non-primitive elements, and H2 removes a GoZero.
   Perf and alloc rows are read at P1, P2 and L1 (i9 AOT) and again at the re-bank.
7. **Docs.** A `ConversionStrategies` text that describes a shape (`err != default!`) is wrong from the seat's
   landing until the regen. Each seat updates its Reference rule in the same seat, and the summary samples are
   refreshed at the regen.

---

## 4. Lanes

| Lane | Box and limits | Seats (in order) | Why |
|:--|:--|:--|:--|
| **G** | design lead | #18 → **#31 P1** → **#32 P2**; reviews #20 (N1 predicate), #26 (gen operators) and #30 (golib builder) | Owns the extension-property design and the gen; the hardest seat |
| **H** | Windows box, heavy gates OK | #19 E1 → **#27** small four → **#28 S1** → **#29 H2** | Statement-shape emitters next to E1's `visitIfStmt`; runs its own batteries and backs up the i7 |
| **R** | R-LAPTOP (+ R-WSL) | FieldView diagnosis and fix → **#30 L1**; perf review of #31 and #32 | Holds the AOT and perf instrument that L1, P1 and P2 are gated on |
| **P1** | Sonnet-class, mechanical | **#20 N1+D1** (predicate spelled in the brief) → **#21 Z1**; takes #29 if H runs late | Wide but rule-exact edits; G reviews at the bytes |
| **P2** | linux container, ~30 GB; heavy legs on the i7 | **#23 C1** → **#24 N1 stage 2** | Converter seats as #4, #10 and #16; native-view knowledge for the stage-2 proof |
| **C2** | cloud, limited disk; the i7 runs its heavy legs | **#25 R5** → **#26 N2** | Analysis-pass and gen experience (plain literals, analyzer A1-A3) |
| **C1** | darwin harness | **#22 pcensus** first; the darwin census at #31 and at the regen; the darwin re-bank leg | Wrote the comment-parity census, the same instrument pattern |
| **i9** | batteries and AOT, no code | rung canaries; AOT perf for #30, #31 and #32; a windows re-bank shard | — |
| **i7 (COORD)** | rung box | every rung (§3.2), the landings, the end regen and attribution | One binary identity for the ladder |

---

## 5. Schedule

**Seat cuts** (parallel; days from dispatch on Wed 2026-10-28):
- G finishes #18 (~0.5 d), then P1 (4 d, with mechanics starting before the FieldView ruling), then P2 (1 d): ready
  about day 6.
- R: FieldView (~1.5 d), then L1 (3 d): about day 5.
- H: #19 (~0.5 d), then #27 (1.5 d), then S1 (2 d), then H2 (1.5 d): about day 5.5.
- C2: R5 (2.5 d), then N2 (1.5 d), plus i7 leg latency: about day 5.
- P1: N1+D1 (1.5 d), then Z1 (0.5 d): about day 2.
- P2: C1 (1.5 d), then stage 2 (1.5 d): about day 3.5.
- C1: pcensus by day 1.

**Serial landing:** 13 landings at 2-3 a day from day 2. The last landing is about day 7 in the best case and
day 9-10 likely (review rounds; P1's size; at least one red to fix forward).

**Then:**
- **End regen:** 1 day best, 2 likely. Three-target emission ~1 h, every roster row's `-tests` emission
  (time measured on the first rung; budget 3 h), attribution, apply, full battery.
- **Re-bank:**
  - the windows sweep is ~2 h sharded across the i9, H, G-LAPTOP and R-LAPTOP;
  - the linux leg (R-WSL, P1, P2) and the darwin leg (C1) are ~5 h each, in parallel; the legs.tsv deal and the
    walls carry over with the new tip;
  - then the annotation seat;
  - 1 day best, 2-3 likely, because a change this wide surfaces some reds, and each one is bisected by rung and
    fixed forward.
- **Release prep and the ritual rehearsal:** 1 day. **Then the owner's PIN.**

| | Last landing | Regen | Re-bank done | Publish (PIN) |
|:--|:--|:--|:--|:--|
| **Best** | Tue Nov 3 | Wed Nov 4 | Thu Nov 5 | **Fri Nov 6** |
| **Likely** | Fri Nov 6 - Sat Nov 7 | Mon Nov 9 | Wed Nov 11 | **Fri Nov 13** (range Nov 12-16) |

A Go 1.26.11 release inside the window moves nothing but the regen's pin (§3.4). A late FieldView root cause moves
only #31 and #32. Everything else lands without them.

---

## 6. Owner decisions (RULED 2026-10-27: the owner accepted every default below)

1. **R5: the parameter-name convention.** `Ꮡ` marks a parameter only when a deref alias also exists, so
   `t.Errorf(…)`. **Yes.**
2. **P2: the spelling.** `b.Ꮡoff` (with R5) instead of `Ꮡb.of(Buffer.Ꮡoff)`. **Yes, `b.Ꮡoff`.**
3. **L1: positions.** Also typed locals for `x := []T{…}`, written `slice<T> x = [1, 2, 3];` (the type moves to the
   left, as C# requires). **Yes:** arguments, returns, fields, nested, and typed locals. Overloaded callees,
   generic inference sites, fixed arrays and keyed literals keep today's form.
4. **S1: scope.** Every non-`fallthrough` tag switch becomes a pattern switch, or only the switches where it removes
   the `do/while (false)` wrapper or a `continueᴛ` label. **Every non-fallthrough tag switch**, for one consistent
   shape whose block structure matches Go's.
5. **Item 17's second half, the generic constraint repeated on every method.** The review parked it (L effort, a
   C# 14 extension-block rewrite), and the census does not list it. Does "all 17" include it? **It stays parked**,
   and is sized as its own item after .2.
6. **N1 stage 2 (slice, map, chan compared to `nil`).** **In .2, gated on the golib proof** (#24). If one
   construction kind disagrees, that operand type keeps `default` and the gap is named.

**COORD rulings this plan needs (not the owner's):**
- the deferred-corpus window (§3.1), including the §3.5 deviation from sweeps-as-own-gate;
- P1's property body (after FieldView) and the record's visibility;
- C1's keep-cast list;
- L1's exclusion rule and named slice types;
- F1's value form and the direction spelling;
- H2's measure-why result;
- the rung canary set.

The stopped re-bank kit (the legs.tsv deal and the briefs) is re-dispatched unchanged, at the regen's tip.
