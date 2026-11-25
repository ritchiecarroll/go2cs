# DESIGN — M:N row 19, the movability seat: statements an awaited call can move ahead of

Owner, 2026-11-25: the movability seat is in .5, design first ("1 in .5"). A hash constructor never parks ("2 yes").
The `&&`/`||` lowering is in ("Yes, rewrite - may be more common in user code"). Design P4, P3 reads this text, G reviews the code. This text
comes first; no generator line is written before P3 has read it. P3's review (APPROVE WITH CHANGES, 2026-11-25) is
folded in: three MUSTs and the SHOULDs and NITs.

## 1. The problem, as measured

A call the twin must await, evaluated after something ref-like in its statement, either moves ahead of the statement
(`var ᴛtwinN = await f(); <refs derived again>; <statement reading ᴛtwinN>`) or stays plain and is counted ref-blocked
(TwinGenerator.Rewriter.cs `RefLikeBefore`; the census row `// B file|line|caller|callee|reason|blocker`). It cannot
await in place, because C# would hold the ref-like value or the ref across the await (CS4007, CS9217, CS8178).

A call moves only when everything evaluated before it in its statement is a constant, a pure read, or a call that has
already moved, and when its statement is an expression statement, a local declaration or a return. Row 19's census
(linux, go2cs-stdlib.slnx with GoStdlibTwins, purged, `--no-incremental`, 358 reports) measured what that still
blocks:

| Tree | Ref-blocked rows |
|---|---|
| operand order edce26dc89c (train 12 merged), current generator | 88 |
| same, rules 1 and 2 of the row 19 cut applied | 54 |
| master 08e113cb0b0, current generator | 123 |
| master, the row 19 cut a36d60dea0d | 89 |

Master reads 35 rows more than operand order because operand order's converter change is not on master. Those 35 are
listed in 6.2. The census records only a call's FIRST blocker, so a site that clears its first blocker can still fail
on a second. Each prediction below states that risk.

## 2. The order rule (carried over from operand order, unchanged)

Go orders calls, method calls, receives and the logical operators. A variable's read is unordered against a call, and
gc reads it after the calls (go1.26.11, `src/tools/mnspike2/MnGoOrder`). A constant has no order. So, in one statement:

- **Calls run in Go order.** A call the twin awaits keeps its place among the statement's calls. A call before it that
  does not park runs first, as in Go.
- **Reads stay in place.** Rule S derives the live refs again after each moved call, so a read after the move reads
  the call's writes, as gc does.
- **Constants go anywhere.**

The seat adds no new order rule. It makes more statement shapes satisfy the existing one.

## 3. The rules

Each rule names what it changes in `RefLikeBefore`, why the move keeps Go's order, and the sites it is predicted to
clear (section 6 lists them). Each rule takes a red-first behavioral row in `TwinOrderTests`, like the row 19 cut's: a
twin built, run, and compared with Go's value, plus the ref-blocked row that must disappear.

**Where a statement may move or be lowered (P3, MUST 3).** M5, M6, M9 and every move only apply to a statement that
is a direct member of a statement list: a block's statements or a switch section's statements. Two shapes are
excluded, because each would compile and run wrong:
- An `else if`: the inner `if` is the else clause of the outer one. A call moved ahead of it would run before the
  FIRST condition, and when that condition is true Go never runs it.
- A brace-less embedded statement (the body of an `if`, `else`, `while`, `for`, `foreach`, `using` or `lock` without
  braces): `if (c) return a && f();` lowered in place gives `if (c) if (!(a)) return false; return f();`, and the
  last return leaves the `if`.
Such a site stays ref-blocked by name. Rows: an `else if` near-miss and a brace-less near-miss, each still blocked by
name with Go's value. `RefLikeBefore`'s `m_current` test probably gives this already; the rows decide.

**M1. An earlier call spills in Go order.** Today a call evaluated before the awaited call blocks it
(`earlier:InvocationExpression`). Instead, that call is spilled first into its own temporary,
`var ᴛN = g(…);`, plain (it does not park) or awaited (it does), and the awaited call moves after it. The spills come
out in text order, which is C#'s evaluation order and Go's call order. A pure read between them stays in place and
reads after both, as in gc. The receiver of an instance call is handled the same way: a by-ref receiver that reaches a
ref (class 3, h2_bundle's `f.debugWriteLoggerf(…, Ꮡf.OrTypedNil(), http2summarizeFrame(fr))`) is read after the
spills, through the ref Rule S derives again.
A call is spilled only when all of these hold. Otherwise it stays a blocker, as today:
- **By value.** Its method does not return by ref (`ref` or `ref readonly`), and its result is not used by ref: not a
  `ref`, `in` or `out` argument, not a store target, not the receiver of a struct instance member. A spill copies the
  value, so a write through the parameter would land in the copy and compile silently (P3, MUST 1: net/http
  client.cs:776, `refererForURL(…, ref (req.URL).DerefOrNull(), req.Header.Get(…))`).
- **Not ref-like.** Its result type is not a ref struct, which cannot live across the await (CS4007).
- **Always reached.** The path from the statement to the call goes only through node kinds `RefLikeBefore`'s walk
  already treats as unconditional: argument lists, parentheses, casts, member-access receivers, object-creation
  arguments, tuple elements, and the operands of a non-logical binary operator. A call under `?.`, `?:`, `??`, the
  right operand of `&&` or `||`, a switch expression arm or a lambda body may not run, and is not spilled.
- **Recursive.** What is evaluated before each spilled call must itself pass: a constant, a pure read, or a call that
  passes these same tests.
Rows: the spill in Go order, checked by value; a near-miss with an earlier ref-returning call used by ref, which stays
blocked by name and keeps Go's value.
As coded, a call is spilled only when it is itself the earlier operand (under parentheses or casts at most): an
argument, a tuple element, a receiver, or a binary operator's left operand. A call nested inside a larger earlier
operand (`other() + d`) still blocks, by name, which is row 7's residual row. A plan pass runs before each statement
is visited and marks the calls to spill; a parking call so marked spills even where nothing ref-like pins it, so it
keeps its place before the later call. Near-miss rows: a ref-returning call passed by `ref`, and a call returning a
ref struct.

**M2. An adapter construction over pure reads is a pure read.** `new bytes_BufferжWriter(Ꮡbuf)` and
`new fmt_StateᴠWriter(s)` are the converter's form of a Go interface conversion of a variable, `io.Writer(&buf)`. They
allocate a wrapper and read their arguments. Nothing else runs. The rule admits an object creation whose type is a
generated adapter (a type go2cs-gen emits for an interface implementation; how the rewriter recognizes one is settled
in the code against go2cs-gen's own output, never by the glyph in the name) and whose every argument is a pure read. Any other object creation stays a blocker, including the
slice-to-array conversion that gc runs before the call.
Probe (run, 3.3): a Go interface conversion of a variable, followed by a call that reassigns the variable
(`fmt.Fprintf(w, "%v", f())` with f setting w). If gc reads w BEFORE f, M2 is wrong and is dropped. The probe goes in
MnGoOrder beside row 7's. P3 probed the VALUE adapter shape (`new fmt_StateᴠWriter(s)` copies its operand): see 3.3.
M2's row covers a pointer adapter and a value adapter.

**M3. Tuple elements are ordered like arguments.** `return (default, errors.New("…"u8 + rt.String()));` blocks on
`in:TupleExpression`. A tuple literal's elements evaluate left to right, as a Go multi-value return's operands do, so
the rewriter treats a `TupleExpressionSyntax` exactly like an argument list: each earlier element passes through
`Earlier()`. A tuple ASSIGNMENT target (`(a, b) = …`) is unchanged and stays under the store rules.

**M4. A compound store into a plain local moves like a plain store.** `s += " "u8 + t.ReturnType.String();` blocks on
`in:AddAssignmentExpression`. In Go, `s += f()` is `s = s + f()`, and the read of s is unordered against f(). The rule
extends the existing assignment case: a compound assignment whose left side is a plain local or parameter (not a ref,
not a field, not an element) moves, and the left side is read after the call.
Probe (run, 3.3): `s += f()` with f writing s. The rule's direction (read after) is what gc does for `b + f()`
in row 7. If gc reads s first in the compound form, M4 is dropped.

**M5. A throw statement is movable.** `throw panic(errors.New("…"u8 + v.Type().String()));` blocks on
`stmt:ThrowStatement`. A throw evaluates its operand and then throws, as an expression statement evaluates and
discards. Moving the awaited call ahead of it changes nothing that is observable. `ThrowStatementSyntax` joins the
statement kinds `RefLikeBefore` accepts.

**M6. An if condition is movable.** `if (w.Bool(val.Kind() == constant.Complex))` blocks on `stmt:IfStatement`. The
condition runs once, before either branch. A call evaluated unconditionally in it (not inside the right operand of
`&&`, `||` or `??`, not inside a conditional expression, not inside a lambda) moves ahead of the `if`, and the
condition reads the temporary. This is the same move the row 7 inline re-derivation already makes for a ref read,
applied to a ref-like value. The body is a nested statement and is unaffected. As coded, the if is movable only
when a ref-like value is evaluated before the call: a call blocked only by a ref read after it keeps row 7's inline
re-derivation, unchanged. Row 19 rule 2's boundary row (a u8 literal handed to a ref struct parameter) moves from an
if header, which M6 clears, to a while header, which runs again each iteration and stays blocked by name.

**M7. A converted Go constant is a constant.** `r.trace(nodeTVˢ, node, node.Pos())` blocks on `earlier:IdentifierName`
because the converter hoists a Go string constant into a static `ref readonly` property over a static readonly backing
field (`nodeTVˢ => ref nodeTVˢᵇ`), and a typed Go constant into a static expression-bodied property
(`VarKind LocalVar => 2`). Roslyn reports no constant value for either. The rule admits, as a constant: a static
property of the package class whose getter is `ref` to a static readonly field or a literal, and a `default` literal.
Nothing else changes. Today `IsPureRead` admits a property only when it returns by ref, and `ref readonly` is not
that, so this shape fell through.

**M8 (proposed, strike if out of scope). A deconstruction into new locals moves like a local declaration.**
`var (r, errΔ1) = d.CharsetReader(…)` blocks on `store-left:DeclarationExpression`. Its targets are fresh locals, as a
single-variable declaration's is, so it moves as a declaration does. One site. A deconstruction that mixes a new local
with an existing target is not this shape and stays blocked; it takes a near-miss row.

### 3.1 Ruling 2: a hash constructor never parks

The five crypto/tls class-5 sites are `tls13.ExpandLabel(() => recvʗ1.New(), …)`. `crypto.Hash.New` calls a
registered constructor through a func value, so the generator marks it as able to park, the lambda becomes a paired
literal, and the call inside it blocks (`ref-read-after:unrederivable(c)|in:ParenthesizedLambdaExpression`).

The ruling uses the existing does-not-park table in TwinGenerator.ParkCuts.cs, not a rewriter case. Two forms exist:

- **W, a weak member (recommended):** one `StdlibWeak` row, `("go.crypto_package", "go.crypto_package.Hash.New")`. A
  call to `Hash.New` never makes its caller a twin, in every package. The five lambdas then have no other reason to
  park, are not paired, and their call sites are no longer await sites. A caller that is a twin for another reason
  (crypto/rsa's `encryptOAEP(opts.Hash.New(), …)`) still awaits it, which is harmless. The row is checked for
  staleness in crypto's own build (GO2CS0019), like every table row.
- **C, cut sites:** five `StdlibCuts` rows, `(crypto/tls caller, crypto.Hash.New)`, one per enclosing function. It is
  narrower and matches "not a park at all" literally. It has a second cost: it is unclear today whether a site inside a
  lambda keys to the enclosing method in `MarkCuts`, and the red-first row would have to settle that.

W is one row and states the owner's sentence as written ("a hash constructor"). It rides this seat as its own commit
with its own red-first row: a lambda calling a weak member is not paired, and ExpandLabel's call site does not block.
Falsifier: after the row, if `ExpandLabel` itself becomes an await site blocked on `unrederivable(c)`, the class moves
rather than clears, and the post says so.

### 3.2 Ruling 3: `&&` and `||` with an await in the right operand (M9)

Owner, 2026-11-25: "Yes, rewrite - may be more common in user code." So this is a general rule, not gob's case.

**M9. A logical operator whose right operand must await is lowered to an `if`.** Today a call in the right operand of
`&&` or `||` cannot move, because it may not run at all, and if the statement holds something ref-like it stays plain
(`in:LogicalAndExpression`). The rewrite turns the operator into the branch Go already means:

Every form goes through a fresh temporary `ᴛ`. It is written `var` below; the code declares it with the operator
expression's own type, so a left operand of another type is converted as the operator would convert it:
- `&&`: `var ᴛ = a; if (ᴛ) { ᴛ = f(); }`, then the original statement with `ᴛ` in place of the operator.
- `||`: `var ᴛ = a; if (!ᴛ) { ᴛ = f(); }`, then the same.
So `return a && f();` becomes `var ᴛ = a; if (ᴛ) { ᴛ = f(); } return ᴛ;`, and `t = a || f();` becomes
`var ᴛ = a; if (!ᴛ) { ᴛ = f(); } t = ᴛ;`. The store target is written once, after the right operand, so a right operand
that reads the target sees its OLD value, as in Go (P3, MUST 2: `ok = n > 0 && check(ok)`). The return form needs no
`false` literal, so a named bool result type checks as before.

The left operand is evaluated once, first, as before. The right operand runs only on the value that would have run it
in Go, so short-circuit and Go order are unchanged. After the rewrite the right operand stands in its own store
statement, inside braces, so it is a direct member of a statement list and its call moves or awaits in place under the
existing rules (and M1 through M8).

Scope:
- The rule fires only when the right operand holds a call that must await and cannot stay in place. A right operand
  that awaits nothing, or whose await can stay in place, is left as a plain expression.
- A chain is lowered from the outside in: `a && b && f()` parses as `(a && b) && f()`, so the left operand `a && b`
  becomes the temporary's initializer whole, and `a || (b && f())` lowers the outer `||` first and then the inner `&&` in the
  statement the outer rewrite produced.
- Other positions (an `if` condition, an argument, a conditional expression) are not rewritten in this seat. A site
  there stays ref-blocked by name. So `(a && f()) || c` is not lowered: the awaiting `&&` is an operand, not the
  statement's operator, and the site stays blocked by name.
- The rule refuses (the site stays blocked by name) when the operator is a user-defined `&&` or `||` rather than the
  built-in bool one, and when the left operand declares a pattern or `out` variable that the right operand uses
  (`a is T x && f(x)`): moving the left operand into its own statement would fail definite assignment.

Rows (COORD 2026-11-25 11:46Z), each red first and checked by value:
- `&&` and `||`, each in a return and in a local store.
- A nested chain: `a && b && f()` and `a || (b && f())`.
- A near-miss whose right operand awaits nothing: it stays a plain expression (the twin's text shows no `if`).
- A right operand with a side effect, and one that panics, each with a left operand that short-circuits: the side
  effect must not happen and the panic must not be raised, checked by value against Go's result.
- A store whose right operand reads the target (`ok = n > 0 && check(ok)`): the old value is passed, as in Go.
- The `else if` and brace-less near-misses from the statement-list rule.

Size: about 40 to 60 generator lines plus the rows. It clears gob's three class-6 rows.

As coded, "cannot stay in place" is decided by running the existing walk twice for each parking call in the right
operand: the call is ref-blocked where it stands, and it moves or awaits in place when the walk stops at the right
operand (the store the lowering writes), with M1 planning its earlier calls, or it sits under an inner operator that is
itself lowered. The near-miss whose right operand awaits nothing has no awaiting twin body at all; the one whose await
stays in place keeps `a == 1 && (await …) == 3`.

### 3.3 The two gc probes, run 2026-11-25 (go1.26.11, linux/amd64)

Run in scratch before this note was read. They go into `src/tools/mnspike2/MnGoOrder` with the seat's code.

*As coded:* `src/tools/mnspike2/MnGoOrder/row19` (Go only; `go run ./row19`, then with `-gcflags=-l`). Re-run
2026-11-25 on go1.26.11: every M4 line reads after the call both ways (`new+f`, 110), every M2 line reads after the
call both ways except the closure-captured pointer local, which reads `first` inlined and `second` with `-l`.

**M4 holds.** `s += f()` with f writing s reads s AFTER the call in every form tried: a package variable with f
inlinable and with f marked `//go:noinline`, a closure-captured string local, and a closure-captured int local
(`n := 1; n += g()` with g setting n to 100 and returning 10 gives 110). The same with `-gcflags=-l`.

**M2 holds as Go-legal, but gc's own order varies with inlining.** The spec leaves the order of an operand's
conversion against a later call unspecified, and gc shows both orders:
- A variable of interface type (`fmt.Fprintf(w, "%s", setW())` with setW reassigning w, a package variable or a
  closure-captured local) is read AFTER the call.
- A package variable of concrete pointer type converted to `io.Writer` at the call is read AFTER the call.
- A closure-captured LOCAL of concrete pointer type converted to `io.Writer` is read BEFORE the call when gc inlines
  the closure, and AFTER it with `-gcflags=-l`.

P3's probe of the value shape (go1.26.11): `use(v, set())`, with v a struct converted to `fmt.Stringer` and set()
writing v, reads the NEW value for a package variable and for a captured local, and so does an int converted to
`any`. One exception: a four-word captured struct read the old value with inlining on, and the new one with `-l`.

So M2's direction (read after) is gc's order whenever the later call is not inlined, and a legal Go order always. The
twin's later call is a parking call and is never inlined into the statement, so M2 gives gc's non-inlined order. M2's
row compares against that value, and its comment names the inlined case as the place where gc itself differs.

## 4. What does not change

- Default mode (owner D4): only the twin generator changes. With GoTwins off it emits nothing, so plain bodies are
  untouched, and CNR cannot move (the train covers it).
- No rule copies a value out of a ref and back. Every move is the existing spill plus Rule S.
- `element-index` (a struct indexer taking `this` by ref) stays a blocker. It is operand order's subject on master and
  not this seat's.
- The plain path keeps C# order. Where it differs from gc (P3's finding on the row 19 cut: `use((@string)f(), h())`
  reads "old" in C#, "Nld" in gc), that is operand order's list or KnownIssues, not this seat's.
- M1 moves more twin sites to gc's order while the plain body keeps C#'s, so a result can differ by mode at more
  places. The seat adds one KnownIssues line saying so, to stand until operand order lands.

## 5. Gates

Generator class (train-assembly section 6): the red-first rows, GenTests in full, the behavioral `--mn` subset, the
twin census on both bases (6.3), repoguard and the identifier census. Each rule commit is red first and green with its
own rule alone, as the row 19 cut's rows were. The two gc probes (M2, M4) run before their rules are written. If a
probe contradicts its rule, the rule is dropped and the post says so; it is never bent to fit.

## 6. Predicted census

### 6.1 On operand order's tree with the row 19 cut (54 rows): the state after both seats land

The 54 rows by first blocker, and the rule that clears each first blocker:

| Group | Rows | First blocker | Rule | Sites |
|---|---|---|---|---|
| A | 10 | adapter construction | M2 (then M1 where a call follows) | go/types errors.cs:79, selection.cs:188; math/big ftoa.cs:520, intconv.cs:96; net/http/httptest server.cs:329; net/http/pprof pprof.cs:562, 567 (two rows); net/http fs.cs:189; runtime/pprof pprof.cs:588 |
| B | 7 | tuple element | M3 (conn.cs also M1) | crypto/tls conn.cs:1299, 1308; encoding/binary binary.cs:347, 528, 555; encoding/gob type.cs:614, 74 |
| C | 3 | compound store | M4 | debug/dwarf type.cs:346; encoding/gob type.cs:473; net net.cs:536 |
| D | 2 | throw | M5 | hash/maphash maphash_purego.cs:207; math/big int.cs:1055 |
| E | 2 | if condition | M6 | internal/pkgbits encoder.cs:359; httpproxy proxy.cs:160 |
| F | 12 | earlier call | M1 | crypto/x509 verify.cs:495, 501 (one row each); flag flag.cs:331; fmt scan.cs:1264; go/internal/gcimporter ureader.cs:402, 584; go/parser parser.cs:1288; net/http h2_bundle.cs:1796, 1946, 8761; net udpsock_posix.cs:106, 135 |
| G | 5 | converted constant | M7 (x509 also M1) | crypto/x509 verify.cs:495, 501 (`time.RFC3339`); go/parser resolver.cs:313; go/types assignments.cs:678; mime/multipart writer.cs:181 |
| H | 5 | none of the rules | (M8 clears mime) | crypto/tls common.cs:1011 (collection element), 1120 (element-access receiver); internal/fuzz fuzz.cs:1067 (range over a call's result); mime encodedword.cs:360 (deconstruction, M8); text/template exec.cs:990 (inside a moving call) |
| 5 | 5 | lambda | ruling 2 | crypto/tls handshake_client_tls13.cs:1000, handshake_server_tls13.cs:1048, key_schedule.cs:33, 47, 64 |
| 6 | 3 | `&&` right operand | M9 (ruling 3) | encoding/gob decode.cs:1248, 1255 (two rows) |

**Prediction: 54 → 5**, with M1–M7 clearing groups A–G (41 rows), ruling 2 clearing class 5 (5) and M9 clearing
class 6 (3). The 5 left are group H. With M8, 4.

What can miss, ranked by what a miss would mean:
1. A second blocker. The census shows only the first. A site in A–G can fail on a second blocker that no rule covers.
   That reads as a smaller drop with a NEW first blocker on the same row, and the post names each one.
2. A probe failing (M2 or M4). Both were run (3.3) and both rules hold, so this risk is closed. Had either failed,
   groups A or C would have stayed.
3. Ruling 2 moving the block instead of clearing it (3.1's falsifier). Then 5 rows change blocker instead of
   disappearing.
4. Build errors. A move that holds a ref or a ref-like value across an await fails to compile (CS4007, CS9217,
   CS8178) in the 358-report build. The prediction is 0 build errors.
5. A silent wrong result. The first version of this note said a wrong move could not be silent. P3 found three that
   can: a spill of a by-ref call, an M9 store whose right operand reads the target, and a move out of an `else if` or
   a brace-less body. Each now has a rule that excludes it and a row that checks it by value. A wrong result outside
   those three would come from a shape not yet named, and the by-value rows are what would catch it.

### 6.2 On master with the row 19 cut (89 rows)

The same 54 rows, plus the 35 that operand order clears. The movability rules reach some of those 35 by first blocker:

| First blocker | Rows | Rule |
|---|---|---|
| element-index | 14 | none (operand order's) |
| adapter construction | 3 of 7 object creations: go/types errors.cs:109, internal/coverage cfile emit.cs:199, encodemeta encodefile.cs:123 | M2 |
| other object creations | 4: go/types builtins.cs:912, 922; internal/coverage decodemeta decodefile.cs:182; internal/trace event.cs:1598 | none |
| converted constant | 3: net/http/cgi child.cs:250; net/http/fcgi child.cs:131, 146 | M7 |
| default literal | 1: net/smtp auth.cs:124 | M7 |
| throw | 1: go/types expr.cs:1348 | M5 |
| if condition | 1: go/build build.cs:965 | M6 |
| tuple | 1: runtime mgcwork.cs:223 (a tuple assignment through a ref) | none: M3 leaves tuple assignment targets alone |
| earlier call | 1: html/template escape.cs:378 | M1 |
| element access, member access (a by-ref argument), unary minus over a call | 4 + 1 + 1 | none |

**Prediction on master: 89 → 89 − 49 − 10 = 30** (49 from 6.1, rulings 2 and 3 included; 10 from this table). The same
miss risks apply.

### 6.3 How the seat measures

Two census arms per base, the generator before and after, same build each time: master with the row 19 cut, and the
same with operand order merged locally (not pushed). Each arm reports total rows, rows by first blocker, and the rows
gained or lost against the other arm, matched by file, function, reason and blocker. A row the seat ADDS is a finding,
posted before anything else.

### 6.4 As measured, 2026-11-25 (linux, 358 reports each arm, 0 build errors each arm)

The arms swap only the two generator files. Base A is master 08e113cb0b0 with the row 19 cut (033b8767567); base B is
that with operand order (edce26dc89c) merged locally and not pushed. Before = the generator at 033b8767567, after =
e751dd9aec6. No arm ADDS a row.

| Base | Before | After | Predicted |
|---|---|---|---|
| B (operand order + cut) | 54 | 5 | 5, or 4 with M8 |
| A (master + cut) | 89 | 25 | 30 |

Scored against 6.1. M8 cleared mime. The 4 left from group H are common.cs:1011, 1120, fuzz.cs:1067 and exec.cs:990,
as predicted. The fifth is a miss of kind 1 (a second blocker): of class 6's three gob rows, decode.cs:1255 stays, now
blocked `inside-moving-call`, because the right operand of `&&` is a parking call whose own argument parks. Group E
cleared although M6 was narrowed to the ref-like case: both of its if headers are ref-like.

Scored against 6.2: 25 against 30 is five better, for named reasons. element-index cleared one row (h2_bundle.cs:10709,
a call inside a literal left plain, the report fix below). Of the four "other object creations", three cleared
(builtins.cs:912 is a pointer adapter over a pure read, M2; event.cs:1598 and decodefile.cs:182 cleared too and were
not traced line by line); builtins.cs:922 stays, `earlier:ObjectCreationExpression`. runtime mgcwork.cs:223 cleared: the tuple is the RIGHT
side of `(w.wbuf1, w.wbuf2) = (w.wbuf2, getempty())`, so its elements are M3's, and the store through the ref local is
C2's existing spill; 6.2 misread it as a target. What stays (25): 13 element-index, 4 element access, gob's
second-blocker row, the member-access argument passed by ref (net/http client.cs:776, P3's MUST 1 shape, which must
stay), the unary minus, builtins.cs:922 and the four of group H.

The first after arm on base A read 33. Two of its misses were the generator's, fixed in e751dd9aec6 with rows:
ruling 2's five crypto/tls rows were report residue (the emission was right, `ExpandLabelꞏtwin(() => recv.New(), …)`
with the literal plain, but PairLiteral's not-parks undo kept the rows its discarded visit recorded), and net
udpsock_posix.cs:102 and 131 stayed because M1 kept a by-value struct result used as a receiver as a blocker. Ruling
2's falsifier (the five rows changing blocker) did not fire: the rows disappear.

## 7. Size

M1 is the largest piece: a spill list in text order in place of the current single-call move, with Rule S after each
spill. Estimated at 120 to 180 generator lines. M9 is 40 to 60. M2–M8 are each a case in `RefLikeBefore` or `IsPureRead`, about 10 to
30 lines each. The rows add about 8 functions to `TwinOrderTests` (one per rule), the hash row, and M9's ten or so functions. The two gc probes are
small Go programs in MnGoOrder. Estimate: four to five lane-days including both census bases.
