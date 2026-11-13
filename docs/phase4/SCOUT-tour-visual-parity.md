# SCOUT: four Tour visual-parity items (C1, 2026-11-13)

> **Status: SCOUT, design only.** The owner asked these four while testing the Tour of go2cs, and COORD bundled them into
> one doc so one implementing lane can take them together (inbox C1 20261113T050022Z, T050548Z, T052245Z and T053044Z).
> The first item asked for `docs/phase4/SCOUT-multivalue-spread.md`; with four items this file takes a broader name.
>
> - **Read at:** master `3da19d0e2e`.
> - **Tools:** the Go side is go/types (Go 1.26.10, linux/amd64); the C# side is .NET SDK 10.0.403, C# 14.
> - **Evidence:** everything is in `docs/phase4/data/tour-visual-parity/`. Each probe builds with `probe.csproj`.
> - **Paths in the census outputs:** `$TOUR` is golang.org/x/website's `_content/tour` (the lessons the Tour runs),
>   `$GOROOT` is Go's, and `$REPO` is this repository.
> - **Not done:** nothing was converted or built from the corpus. Corpus counts read the committed tree.
>
> | item | the owner's example | today | the target |
> |:--|:--|:--|:--|
> | 1 | `fmt.Println(split(17))` | `var (ᴛ1, ᴛ2) = split(17); fmt.Println(ᴛ1, ᴛ2);` | `fmt.Println(split(17));` |
> | 2 | `var c, python, java bool` | three declarations | `internal static bool c, python, java;` |
> | 3 | `switch os := runtime.GOOS; os { … }` | `var exprᴛ1 = os;` | no temp, or `osᴛ1` |
> | 4 | `primes := [6]int{2, 3, 5, 7, 11, 13}` | `new nint[]{…}.array()` | `array<nint>([2, 3, 5, 7, 11, 13])` |

## 1. The multi-value spread `f(g())`

**Today.** `src/go2cs/convExprList.go:276-337` hoists the inner call into `var (ᴛ1, ᴛ2) = …;` and passes the temps.
`defer f(g())` and `go f(g())` have their own path: the tuple is evaluated eagerly and expanded inside the deferred
lambda, as in `defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ)` (`visitDeferStmt.go:419`).

### 1.1 Census

`spread/spreadcensus-main.go`. A site is a call whose only argument is a call that returns two or more values.

| corpus | sites | `...any` only | `...T` | fixed arity | method / interface / func value | callee itself calls |
|:--|--:|--:|--:|--:|--:|--:|
| the Tour | 6 | 6 (all `fmt.Println`) | 0 | 0 | 0 | 0 |
| behavioral | 60 | 25 (24 `fmt.Println`, 1 `showAll`) | 0 | 33 | 2 | 1 |
| std, non-test | 23 | 0 | 0 | 17 | 6 | 0 |
| std, test | 373 | 9 (`fmt.Println`) | 0 | 175 | 189 | 172 |

- **Which callees.**
  - The Tour's 6 are `basics/named-results.go`, `methods/images.go` (arity 4), `methods/exercise-errors.go` ×2 and
    `solutions/errors.go` ×2.
  - std's non-test fixed-arity sites are `time.Unix` ×4, `template.Must` ×3, `compInfo` ×4, `fipsError2` ×4, `fixCount`
    and `registerCover`.
  - Its 6 method sites are `affineFromJacobian` ×2, `newIf`, `newRange`, `newWith` and `d.Compose`.
- **No site at all spreads into `...T`.**
- **Package functions split by callee package:**
  - std: 164 same-package and 36 cross-package (7 of those in non-test code).
  - behavioral: 34 and 24.
  - the Tour: 0 and 6.
- **Contexts in behavioral:** 25 statement, 11 expression, 11 assignment, 10 `defer`, 2 `go` and 1 `return`.
- **Variadic declarations in std non-test code:**
  - 61 functions take only `...any` (38 exported);
  - 132 take a fixed prefix and then `...any`;
  - 43 take `...T`;
  - 57 take a prefix and then `...T`.

**User code like the Tour is fmt.Println.** std's own spreads are fixed arity, and nearly all in tests.

### 1.2 Evaluation order, measured against Go

`spread/go-order-main.go` under gc go1.26.10:

| Go | Go's result | today's temp form | an inline C# form |
|:--|:--|:--|:--|
| `pick()(g())`, a callee that calls | `pick`, then `g` | `g` then `pick`: **wrong today** | `pick`, then `g` |
| `p.scaled(g2())`, a value receiver `g2` changes | 9234 (p read after `g2`) | 9234 | 1234: **wrong** |
| `h(g3())`, a func variable `g3` reassigns | 12 (h read after `g3`) | 12 | 7: **wrong** |

(The C# columns are from `spread/probe-Program.cs`, which reproduces each.)

- **What this means for the inline form.** A callee that is a package-level FUNCTION evaluates nothing, so the inline
  form is exactly Go's order there. Every Tour site is that shape.
- **Methods and func values keep the temp.** gc reads a receiver or a func variable after the argument's calls. That is
  H's rule (BOARD, 2026-11-12, "gc runs the calls in an operand list before it reads a plain variable there"), which C2's
  spill work follows. An inline C# form reads the receiver first, so it would break that rule.
- **A finding, separate from the visual change.** Today's temp form breaks Go's spec order when the callee expression
  itself calls (`pick()(g())`). There are 172 such sites, all in std tests (the curried `expectPresent(t, s, i)(m.Load(i))`
  family), plus 1 in behavioral.
  - The fix is to evaluate the callee value into a temp BEFORE `g()`.
  - It is outside the owner's ask. It is recorded here and goes to the BOARD with this doc.

### 1.3 Options

All four measured in `spread/probe-Program.cs`: 16 PASS, and the two order rows are explicit reproductions.

- **A. Spread at run time** (the variadic expands a single `ITuple` argument). **Rejected.**
  - For converted Go code it is sound: Go has no tuple values. golib states that no converted Go type is emitted as a
    `ValueTuple` (`GoReflect.TypeLayout.cs:75` and `:1503`), and reflect destructures a multi-return internally
    (`reflect/value_impl.cs:2167`). It never hands one to Go code as an `any`.
  - But a deliberate single `ValueTuple` from C# code is spread too (reproduced). It covers only callees whose only
    parameter is `...any`. It adds a type test and a box per call, and the spread is invisible in the source.
- **B. Per-arity generic overloads, generated** for every function or method whose only parameter is `...any`, of the
  form `Println<T1, T2>((T1, T2) t) => Println(t.Item1, t.Item2)` for arities 2 to 7. **Recommended.** Measured:
  - `fmt.Println(split(17))` binds the generic overload: an identity conversion beats boxing into the params slot.
  - Arity 4 binds (the Tour's `images.go`).
  - One argument and zero arguments still bind the params form.
  - The method-group conversion the converter emits (`Funcꓸꓸꓸ<any, (nint, error)> f1 = fmt.Println;`) and a `var`
    natural type both still compile.
  - A deliberate C# tuple stays ONE value.
  - **The cost** is generated code only: 6 members per such function, which is 366 for std's 61. None of it is in the
    committed corpus.
  - B does not extend to `...T` without a constraint per element type, and no site needs that.
- **C. Hand-owned overloads in fmt only** (the owner's idea): covers the Tour (6 of 6) and 24 of behavioral's 25. B
  subsumes it, and C would leave `log.*` and user functions (`showAll`) as special cases.
- **D. Fixed-arity callees, an overload per callee.**
  - **Same package:** the converter knows its own spread sites and emits `f((A, B) t) => f(t.Item1, t.Item2)` beside
    the callee, with its usual per-argument conversions in the body.
  - **Another package:** a C# 14 static extension in the CALLER's package,
    `extension(time_package) { static … Unix((long, long) t) … }`. It binds through the `using` alias (measured).
  - **An interface method:** an instance extension on the interface (measured).
  - **Cost:** one member per (callee, shape) used. That is 10 same-package and 7 cross-package in std non-test code.

### 1.4 Recommendation and ladder

1. **S1 (B):** a package-level function whose only parameter is `...any`. This covers the Tour 6 of 6, behavioral 25,
   and std 9 test sites.
   - go2cs-gen emits the overloads.
   - The converter stops hoisting when the callee is a package function name and its only parameter is `...any`.
2. **S2 (D, same package, fixed arity):** std 164 sites (10 non-test), behavioral 33.
3. **S3 (D, cross package, fixed arity, static extensions):** std 27 (7 non-test: `time.Unix` ×4, `template.Must`
   ×3), behavioral 0.
   - The other 9 cross-package std sites, and all 24 cross-package behavioral sites, are `fmt.Println`, which S1
     covers.
4. **S4, independent:** the order fix for a callee that calls (std 172 test sites, behavioral 1).

Methods, interface methods and func values keep the temp. `defer` and `go` keep their machinery; a later step can
pass the tuple to the S1/S2 overload inside the lambda.

**Predictions:**
- **P1.1** The Tour's `basics/named-results.go` reads `fmt.Println(split(17));`.
- **P1.2** S1 moves exactly the 25 behavioral and 9 std test sites counted above, and no output changes.
- **P1.3** S2 moves no method or func-value site.

**Stop lines:**
- **X1.1** Any site whose callee is not a package-level function name changes.
- **X1.2** Any behavioral output changes.
- **X1.3** An overload makes a call ambiguous (CS0121).

## 2. One declaration for a multi-name spec

**Today.** `visitValueSpec.go` writes one declaration per name: `nint i = default!; nint v = default!;`
(ArrayRangeSnapshot's golden, `:147-148`).

**C#.** One declaration with several declarators is legal for static fields, `static readonly` and `const`, and for
locals with an explicit type (all measured in `vp/probe-Program.cs`). Locals declared with `var` cannot take several
declarators: CS0819, measured in `vp/mustfail-build.txt`. The converter's local form already spells the type.

### 2.1 Census

`vp/vpcensus-main.go`, item 2: every `var` or `const` spec with two or more names.

| corpus | specs | would merge | declarations saved |
|:--|--:|--:|--:|
| the Tour | 6 | 5 | 6 |
| behavioral | 51 | 28 | 35 |
| std, non-test | 274 | 213 (209 local `var`, 4 package `var`) | 282 |
| std, test | 283 | 104 | 143 |

**What blocks a merge, measured on the Go side** (std non-test / std test / behavioral):

| blocker | std non-test | std test | behavioral |
|:--|--:|--:|--:|
| a name address-taken or captured by a closure: its emission is a heap box (`ref var odd = ref heap<…>(out var Ꮡodd);`, ClosureWriteVisibility's golden `:260`) | 54 | 132 | 5 |
| a tuple initializer: already one deconstruction (`var (a, b) = f();`) or the package tuple holder | 3 | 14 | 10 |
| names of different types (`var a, b = 1, "x"`) | 0 | 19 | 7 |
| an untyped const: each name's C# type is decided by its own uses | 3 | 32 | 5 |
| a blank name | 4 | 8 | 5 |
| exported and unexported names in one spec | 0 | 0 | 0 |
| a comment inside the spec | 0 | 0 | 0 |

**Blockers only the emission shows.** Each must be checked against the emitted declaration, not Go:
- a lazy package var (`lazyPackageVars`, a ref property over a holder);
- an initializer relocated into the init method (`collectMovedInitVars`);
- a hoisted tuple-holder field;
- a manual-conversion var: a shared spec is already refused (`visitValueSpec.go:57-75`).

A Δ rename is not a blocker: it changes only the name.

### 2.2 The rule

Emit one declaration for a RUN of consecutive names from ONE Go spec when each name's declaration HEAD is identical:
- the same modifiers;
- the same attributes;
- the same C# type;
- the same storage: a plain field or local, not lazy, relocated, hoisted or heap-boxed.

The initializers stay per name (`T a = x, b = y;`).
- The implementer computes the head per name from the emission, so a blocker this census cannot see splits the run by
  itself.
- Separate Go specs are never merged.
- Static initializers run in textual order whether merged or not, so init order cannot move.

**Predictions:**
- **P2.1** The Tour's `basics/variables.go` reads `internal static bool c, python, java;`.
- **P2.2** Per target, std non-test code loses between 282 and 213 declaration lines; the lower bound is if the
  emission-only blockers take a run apart.
- **P2.3** behavioral loses at most 35, and no output changes.

**Stop lines:**
- **X2.1** A merged declaration whose names differ in any attribute or modifier.
- **X2.2** A position-map or comment line lost.
- **X2.3** Any behavioral output changes.

## 3. A switch tag's temp

**Today.** `visitSwitchStmt.go:355` names the temp `exprᴛN`. The tag keeps a temp in two cases:
- a STRING tag, because C# would govern the switch through @string's conversion to System.String (the comment
  above that line);
- a switch the pattern form cannot take: one with fallthrough, or one with a label that is not a constant.

### 3.1 Census

- **Emitted, in the committed tree:**
  - `src/core`: 662 `var exprᴛN = …;` lines in 315 files (118 of them test files).
  - behavioral goldens: 33.
  - The right-hand sides: 304 plain identifiers, 270 selector chains (`runtime.GOOS`, `tc.Expected`), 42 calls,
    25 index expressions and 21 casts.
  - COORD's message counted 551 "plain identifier" lines. With the patterns `@?<letter>\w*` (an identifier) and the
    same joined by dots (a selector chain), this read finds 304 and 270, which is 574 together. The 551 was likely a
    looser identifier shape.
- **Go side** (`vp/`, item 3): 340 non-test and 322 test switches take a temp.
  - That is 662 in all, exactly the emitted count.
  - By cause: 505 string tag, 63 fallthrough, 94 non-constant label.
  - Identifier tags that are variables: 182 non-test and 113 test, which is 295 against the 304 emitted identifier
    lines. 7 of the difference are constant tags; 2 are NOT RESOLVED (flavour files are a likely cause).
  - **Level 2 eligible:** the tag is a local or parameter, not address-taken, not captured, and every label is free
    of calls and receives. That is 168 non-test and 99 test switches.
  - **Level 2 refused:** 9 by a label that calls, 16 captured, 1 address-taken, 2 package vars.
  - The Tour has 1 eligible switch, behavioral 21.

### 3.2 Soundness of level 2 (no temp)

Go evaluates the tag once. Reading the local in every label instead is the same value if nothing can write the local
between the first and the last label test:
- **The labels:** they make no calls and no receives.
- **Other writers:** no other goroutine or pointer can reach the local, because it is not captured and not address-taken.
- **The bodies:** a body runs only after the last test that can still run. The fallthrough chain short-circuits later
  tests through its `match` and fallthrough flags, and the precomputed any-match flag is computed before any body.

### 3.3 Naming (level 1)

- **The name:** the temp takes the tag's C# spelling with any `@` stripped (`osᴛ1`, `stringᴛ1`, `ΔLockᴛ1`; all valid
  C# names, measured).
- **The counter:** `getTempVarName` keeps a counter per prefix for each function (`visitFuncDecl.go:2401`), so two
  switches on `os` in one function give `osᴛ1` and `osᴛ2`.
- **Collisions:** a collision would need a Go identifier that already ends in `ᴛ` and digits. The converter already
  assumes there is none.

### 3.4 Recommendation

Do both:
- **Level 2 wherever it is eligible:** no temp, which is the owner's preferred reading. Std has 267 sites.
- **Level 1 for the other identifier tags:** about 30 sites keep a temp, named after the tag.
- **Selector, call and other tags keep `exprᴛN`.** Naming them after their last segment (`GOOSᴛ1`) is possible, but it
  hides what was read.

**Predictions:**
- **P3.1** The Tour's `flowcontrol/switch.go` reads `case {} when os == "darwin"u8`.
- **P3.2** Per target, the identifier lines go to 0, taken by 267 removals plus about 30 renames, and the other 358
  lines are untouched.
- **P3.3** No output changes.

**Stop lines:**
- **X3.1** A temp removed where a label calls or receives, or where the tag is captured, address-taken or a package var.
- **X3.2** Any behavioral output changes.

## 4. Array literals as collection expressions

**Today.**
- A slice literal reads `slice<nint>([2, 3, 5])`, through `builtin.slice<T>(T[])` (`builtin.cs:3881`; the
  `ReadOnlySpan` overload at `:3902` is for `u8` text).
- An array literal is `new nint[]{…}.array()`: `convCompositeLit.go:702` sets the suffix, and `:737` is the
  named-array arm.
- The extensions are `ArrayExtensions.array` (`array.cs`): `T[]`, `T[]` plus a length, plus an element factory,
  `IEnumerable` plus a length, and `SparseArray` plus a length and a factory.

### 4.1 Census

- **Emitted, `src/core`:**
  - 1,800 `}.array()`;
  - 1,491 of them the one-line `new X[]{…}.array()` form;
  - 256 sized `.array(N…)`;
  - 64 `SparseArray`;
  - in 289 files, 115 of them test files.
- **Emitted, behavioral:** 274, 245, 82 and 18, in 107 files.
- **Go side, std non-test:** 909 array literals.
  - 415 are unnamed (or an alias) and unkeyed at full length: 244 written and 171 elided inside an outer literal.
  - 348 + 25 + 13 are named array types.
  - 68 and more are keyed or sparse.
  - 10 are shorter than their length; 22 are empty.
- **std test:** 1,226, of which 966 are unnamed and unkeyed at full length.
- **The Tour:** 7. **Behavioral:** 315.

### 4.2 The rule

- **Unnamed or alias array types, unkeyed:**
  - at full length: `array<T>([…])`;
  - shorter than the length: `array<T>(N, […])`.

  This needs two new golib builtin overloads, `array<T>(T[])` and `array<T>(nint, T[])`. They target `T[]`, as
  `slice<T>` does, so a call inside an element keeps its own Go line (`sliceLiteralTakesBuiltin`'s note).
- **Measured** (`vp/probe-Program.cs`):
  - `array<nint>([…])` binds the builtin method beside the `go.array<T>` type, as `slice<nint>` does.
  - The sized form fills the rest with zeros.
  - Each evaluation gets a fresh array.
  - **The IL is today's apart from the called method:** 44 bytes against 44 for an `nint` literal, and 24 against 24 for
    a byte literal, which keeps its static-data blob. One byte of the method body differs.
- **What this does not touch.** The hoist mode and the lazy static-data work do not move: they decide where a literal
  is evaluated, and this changes only the call that wraps the array.
- **Kept as they are, named:**
  - keyed and sparse literals (`SparseArray`);
  - named array types (the wrapper constructor, `new d(…)`), which a later step could write as `new d(array<rune>([…]))`;
  - the element-factory form (a managed element that needs a fresh zero per slot);
  - arrays of arrays, whose elements are themselves array literals;
  - the empty literal (`new(N)`).
- **Alias arrays (`aliasArrayComposite`):** an alias denotes the identical type, so it takes the builtin form, as
  `slice<T>` does.

**Predictions:**
- **P4.1** The Tour's `moretypes/array.go` reads `var primes = array<nint>([2, 3, 5, 7, 11, 13]);`.
- **P4.2** Per target, at most the 1,491 one-line sites move, and no sized, sparse or factory site does.
- **P4.3** No output changes, and the IL of a byte table keeps its blob.

**Stop lines:**
- **X4.1** A sparse, named, factory or nested site changes.
- **X4.2** Any behavioral output changes.
- **X4.3** A static-data table's IL grows.

## 5. Seats

- **Each item is a separate seat, and all four move corpus bytes.**
- **Items 2, 3 and 4 are converter seats.** Item 4 also adds two golib overloads. Each is about one lane-day,
  including its three-target reconvert.
- **Item 1** has three steps. S1 is go2cs-gen plus the converter, about one and a half lane-days. S2 and S3 are about
  one lane-day each.
- **Order:** item 3, then item 4, then item 2, then item 1's S1. Item 3 has the most Tour-visible sites, and item 1
  needs a generator change.
- **S4 (the order fix) is a correctness seat.** It is outside the owner's ask, and it belongs with C2's spill work.
