# Expression Switch Statements

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#expression-switch-statements)
Go expression-based `switch` statements are flexible: cases do not fall through automatically (no `break` needed), and the `fallthrough` keyword runs the next case body bypassing its expression. Based on the [Manual Tour of Go Conversions](https://github.com/ritchiecarroll/go2cs/tree/master/src/archived/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to `if / else if / else` is the best choice for most cases. When every case label is a C# **compile-time constant** and there is no `fallthrough`, a traditional C# `switch` works. "Constant" here means a C# `const` — a literal, a computed literal expression (`a + b`), or a *typed* basic-type const — not merely a Go constant. A case label that references a plain variable, a struct field (`case frame.fp`), an *untyped* / named-type / cross-package const emitted as `static readonly` (`case goarch.PtrSize`), or an address-of expression (`case &g`) is **not** a C# constant, so a C# `switch` case label there is invalid (CS9135 / CS0150). Such a switch, when it has no `fallthrough`, is still a C# `switch`, a **pattern switch** that binds the tag in each clause and tests it in a `when` guard: `switch (tag) { case var ᴛ when ᴛ == frame.fp: { … break; } … }`. C# tries the guards in label order, stops at the first that holds, and tries `default` last wherever it is written, which is Go's order label for label. A clause's labels join with `||`, and the same constant-vs-runtime-value test chooses each clause's operator: a clause whose labels are all C# constants keeps a constant pattern (`case 3 or 4:`), any other compares with `==` (or `AreEqual` for an interface tag). A pattern switch nested in another's clause names its variable `ᴛ2`, `ᴛ3`, since the outer `ᴛ` is in scope there. A Go `break` in a clause exits the switch and a `continue` the enclosing loop, exactly as written. (Guarded by `SwitchLabelSideEffectOrder`, whose labels log every evaluation, output-compared vs Go.) A **string** tag keeps a temp under the expressionless governor instead, `var exprᴛ1 = tag; switch (ᐧ) { case {} when exprᴛ1 == "get"u8: … }` ([below](#a-string-tag-keeps-its-temp-under-the-expressionless-governor)). A switch **with** `fallthrough` still lowers to an `if / else if` chain over a temp (`var exprᴛ1 = tag; if (exprᴛ1 == frame.fp) …`): its cases are expanded to standalone `if` statements with a local fall-through flag — the most complex (and least pretty) scenario. In that chain a Go `break` inside a case has no enclosing C# switch to target (CS0139), so a case body that contains such a break is wrapped in a `do { … } while (false)` — the break exits that one-shot loop, i.e. the case. The wrap is emitted only for a case whose body actually has a switch-targeting `break` (one not caught by a nested loop/switch/select), so every other case is unchanged. (A `break` inside a *nested* loop within the case still targets that loop, as in Go.) In that if-chain form a **trailing `default:` reached via fallthrough** is emitted as a *guarded* `if (fallthrough || !match) { … }` — the guard is needed so the default does not run after a matched-but-non-fallthrough case, but C# cannot prove it always executes. So when such a guarded-default switch is the last statement of a **value-returning** function and every case is terminal, C# reports CS0161 ("not all code paths return a value") even though the Go `default` makes the switch exhaustive (runtime `startpanic_m`). Because a guarded-terminal-default switch cannot be legally followed by reachable Go code (it always returns/exits), the converter emits an unreachable `return default!;` after the if-chain to satisfy C#'s definite-return analysis — gated on the enclosing function/literal actually returning a value (via its own return signature), so a `void` function or a switch that isn't terminal is unaffected. (Guarded by the `SwitchFallthroughDefaultReturn` behavioral test.) A comparison case may use a C# relational/constant pattern (`case {} when x is < 0`) only when the compared-to operand is a C# compile-time constant; for a variable (`case x == y`) or a `static readonly` const (untyped/cross-package), it falls back to a `when` guard (`case {} when x == y`) — a relational pattern there is invalid (CS9135).

## A string tag keeps its temp under the expressionless governor
C# chooses a switch's GOVERNING type through a user-defined implicit conversion whenever exactly one reaches a
classic governing type (an integer, `char`, `bool`, `string` or an enum), even in a pattern switch. golib's
`@string` has one, to `System.String`, so `switch (tag)` over a Go string would bind `ᴛ` as a decoded .NET
string: `ᴛ == "get"u8` does not compile, and where a label does compile (`ᴛ == s`, `s` an `@string`) the guard
compares UTF-16 text, under which two different invalid UTF-8 byte strings both decode to U+FFFD and compare
equal. A tag whose Go underlying type is string (a named string type and `sstring` included) therefore keeps its
temp, which holds the tag's own type, and the switch governs on `ᐧ`, the form an expressionless `switch {` takes:
<!-- illustration: not converter output -->
```csharp
var exprᴛ1 = cmd;
switch (ᐧ) {
case {} when exprᴛ1 == "get"u8: {
    return 1;
}
case {} when exprᴛ1 == alt: {
    return 2;
}}
```
The `==` is golib's byte-wise `@string` equality, as in the chain, and the clauses keep the pattern switch's block
structure, Go's default position and bare `break`/`continue`. A named numeric is re-governed too, to its
underlying integer (a `typeof` probe reads `byte` for a `num:uint8` wrapper), which compares the same values, so
it keeps the pattern switch. Guarded by `SwitchLabelSideEffectOrder`'s `bytesNotText`, which switches on two
different invalid UTF-8 strings: a decoded compare takes the wrong arm in both, and the Output phase names them.

## A switch on a `static readonly` constant tag compares in `when` guards
A switch TAG that is itself a constant emitted as `static readonly` -- an untyped const's `UntypedInt` wrapper (`switch goarch.PtrSize`, reflect abi.go) or a `uintptr`-struct const -- cannot govern a C# switch on constant labels: the int case labels are not constants OF the wrapper struct type, and a constant pattern fails the same way (CS9135). The recorded tag type is no help (go/types records the untyped constant's DEFAULT type in tag position), so the gate is on the object resolution: a constant-valued tag that is not a true C# `const` takes the pattern switch with every clause in a `when` guard (the wrapper's `==` operators) and no constant pattern:
<!-- source: src/tests/Behavioral/CrossPkgUser/main.cs:238-239 -->
```csharp
switch (CrossPkgLib.Precision) {
case var ᴛ when ᴛ == 1: {
```
A variable tag stays switchable. Guarded by `CrossPkgUser` / `CrossPkgLib`.

## A leading constant-true case stays opaque to the compiler
Go's `switch { case true: ... case cond: ... }` (time parseStrictRFC3339 deliberately disabling its strict checks) compiles the LATER cases as dead code; a foldable `when true` makes C# reject them outright (CS8120). A constant-true case condition on a NON-LAST clause therefore emits the golib `ᐧᐧ` marker -- a `static readonly bool` the compiler cannot fold:
<!-- source: src/tests/Behavioral/ExprSwitch/ExprSwitch.cs:253 -->
```csharp
case {} when ᐧᐧ: {
```
The marker is deliberately SEPARATE from the const `ᐧ` switch governor: that const's foldability is itself load-bearing (`case ᐧ when ...` label patterns need a constant, and an infinite `for (...; ᐧ ;...)` relies on the fold for reachability proofs -- CS9135/CS0161 when it was made readonly in place). Guarded by `ExprSwitch`.

## No constant pattern against a named-numeric wrapper
A constant expression whose CONTEXTUAL type is a wrapper struct -- golib `uintptr` or any `/*num:...*/` named numeric (time's `Duration`) -- can never be a C# constant, so no constant/relational pattern can compare against it: `d is >= 0` types the literal 0 as Duration (CS9135). The lowering keeps the plain operator form (`d >= 0`, the wrapper's operators). Guarded by `ExprSwitch` (the `pace` switch).

## An index-expression case label falls back to equality
A case label that INDEXES a package-level array/slice variable (`case Typ[UntypedNil]:` — go/types
operand.go, where `Typ` is the universe `*Basic` array) is a runtime value, never a C# constant. The
constant-pattern form is doubly broken there: C# parses `case Typ[UntypedNil]:` in pattern position
as an array TYPE (CS0246 + CS0270). `canUsePatternMatch` rejects an `*ast.IndexExpr` label the same way
it rejects a non-constant identifier/selector, so the clause takes the `==`/`AreEqual` comparison the
multi-value arm already produced:
<!-- illustration: not converter output -->
```csharp
case var ᴛ when AreEqual(ᴛ, Typ[UntypedNil]): {   // NOT `case Typ[UntypedNil]:`
```
(Guarded by the `IndexExprCaseLabel` behavioral test — single- and multi-label clauses indexing a
package-level array var, output-compared vs Go.)

## Literal case labels under a named-type tag compare through a cast

A tagged switch whose tag type is a NAMED (non-interface) type — net/http's
`func (code socksReply) String()` switching on `code` — renders the tag as a generated wrapper
struct. An untyped-LITERAL label adopts the tag's named type in Go (go/types records it on the label
expression), but its C# render is a bare literal of the UNDERLYING type, which can neither be a
constant pattern (`case 0x01:` — CS9135, constant pattern against the wrapper) nor compare bare
(`ᴛ == 0x01` is ambiguous between the wrapper's `==` and the underlying's built-in `==`, both
reachable through the wrapper's two-way implicit operators — the same ambiguity family as the
named-string consts). Two converter pieces:

1. the pattern-match decision excludes any named-wrapper tag (`tagIsNamedWrapper`, beside the
   existing `namedTypes`/`tagIsStaticReadonlyConst` gates — those could not catch the mixed
   const-ident + literal switch, because the per-label screening short-circuits once `allConst`
   goes false and never reaches its named-type check);
2. a CONSTANT label that is not an ident/selector/conversion-call (those already render AT the
   wrapper type) casts to the tag type:

<!-- illustration: adapted, owed a real excerpt -->
```csharp
switch (code) {
case var ᴛ when ᴛ == socksStatusSucceeded: {     // named-const label — no cast
    return succeededˢ;
}
case var ᴛ when ᴛ == (socksReply)(0x01): {       // literal label — cast to the tag type
    return generalSocksServerˢ;
}
```
<!-- from src/core/net/http/socks_bundle.cs (`socksReply.String`); the `ˢ` names are the hoisted string literals. -->

The same cast applies to an ALL-literal switch over a named type, which would otherwise take the
ambiguous bare `==` form. (Guarded by `NamedNumericSwitchLiteral`
— a mixed named-const + literal switch including a multi-label clause, and an all-literal switch,
output-compared vs Go.)
<!-- at landing: full-stdlib footprint 12 files: socks_bundle, archive/zip, encoding/xml, go/printer, go/types,
internal/poll and the syscall zsyscall shims. -->

## A trailing `default` in a switch WITH `fallthrough` is guarded on `!match`
A Go `switch` with no fallthroughs is a C# switch, whose `default` runs only when no case matched. A
`fallthrough` takes the `if / else if / … / else { default }` chain, and it also **breaks the chain**: the case that `fallthrough` targets is emitted as a SEPARATE, `!match`-guarded `if`
(`if (fallthrough || !match && <labels>) { … }`) so it can be entered both by falling through and by a
direct match. A trailing `default` after such a case cannot be that `if`'s bare `else` — which
would fire whenever the fallthrough-target `if` is false, i.e. **after any matched NON-fallthrough case**, not
only when nothing matched. fmt's `printValue` is exactly this shape:

```go
switch f.Kind() {
case reflect.Int, …: p.fmtInteger(…)          // a matched non-fallthrough case
case reflect.Pointer: … ; fallthrough
case reflect.Chan, reflect.Func, reflect.UnsafePointer: p.fmtPointer(f, verb)
default: p.unknownType(f)                        // must not run after fmtInteger
}
```

so a bare `else` would format an `int` slice element with `fmtInteger` AND then `unknownType`,
breaking `%v` of every composite. The trailing default is emitted `else if (!match) { /* default: */ }`
(matchVarName; `fmt/print.cs`'s `printValue` carries it). This is byte-equivalent to the
bare `else` in a pure else-if chain (the default is reached only when `!match` either way) and correct
in the broken chain, so it is a safe general lowering. Like the fallthrough-reached default, the guarded
form leaves C# unable to prove exhaustiveness, so a value-returning terminal switch still gets its
trailing `return default!;`. Guarded by `SwitchFallthroughDefault` (a `fallthrough`+`default` switch
where a matched non-fallthrough case must NOT run the default, output-compared vs Go).

## A NON-TRAILING `default` is guarded on the PRECOMPUTED any-case-match, never the running flag
Go allows `default` in **any** clause position and still picks it only when no case matches — position
is presentation, not semantics. The if-chain lowering emits clauses in **source order**, so a default
that is not last is guarded by a predicate that has only seen the arms emitted *before* it. Every clause
after such a default is then dead code, and the default body runs for values those clauses own. Two
shapes reach the chain, and the corpus held a live instance of each:

**Fallen into.** `encoding/json`'s `decode.go` `array` is the witness:

```go
switch v.Kind() {
case reflect.Interface:
	if v.NumMethod() == 0 { … return nil }
	fallthrough
default:
	d.saveError(&UnmarshalTypeError{Value: "array", Type: v.Type(), …})
	d.skip()
	return nil
case reflect.Array, reflect.Slice:
	break
}
```

The default participates in fallthrough, so it **cannot** be reordered — the `fallthrough` link is
source-order-sensitive. Guarded by the running match flag, for a slice or array target the flag is
still `false` there (only the `Interface` arm has run), so the error arm would fire and the
`Array, Slice` arm below it would be unreachable: `json.Unmarshal` would fail **every** JSON array
whose target is not a bare `interface{}` — `json: cannot unmarshal array into Go value of type
[1]interface {}` for a fixed-size array (which is how `net/rpc/jsonrpc` passes its `[1]any` params),
and the identical error for every `[]T`. The default therefore reads the **precomputed** any-case-match
built for the mirror shape — the OR of every case condition in the switch, materialized ahead
of the chain — so it fires only when nothing matches:

<!-- illustration: not converter output -->
```csharp
var exprᴛ1 = v.Kind();
var matchᴛ2 = exprᴛ1 == reflect.ΔInterface || (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice);
if (exprᴛ1 == reflect.ΔInterface) { … fallthrough = true; }
if (fallthrough || !matchᴛ2) { /* default: */ … }
if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) { … }
```
<!-- condensed from src/core/encoding/json/decode.cs (`array`). -->

**Participating in no fallthrough at all.** A non-constant-label tagged switch with no `fallthrough` at
all is a pattern switch, where `default` stays in Go's position and C# tries it last. One that has
fallthroughs elsewhere *moves* such a default clause to the end. A **constant**-label switch
that has fallthroughs elsewhere also lowers to the chain, outside that reorder gate, and an un-moved
default emitted as a **bare block** would run unconditionally: `internal/bisect`'s `parsePattern`
leads with `default: return …parseError`, so every bisect pattern parse would return "invalid pattern
syntax" and all six digit arms below it would be dead code. (`regexp/syntax`'s `parseEscape` has the
identical shape, and its default body re-tests `!isalnum(c)`, which happens to exclude every one of its
own case labels.)

Such a default is guarded **in place** — `if (!matchᴛ2) { /* default: */ … }` — rather than reordered.
Widening the reorder gate would not do: reordering moves the clause's *statements*, but
comments attach by source position, so `parseEscape`'s default-clause commentary would migrate up into
the octal arm above it. Guarding in place needs no code motion, keeps the emitted C#
in Go's clause order, and costs nothing structurally — emitting an `if` instead of a bare block is
precisely what lets the following clause keep its `else`, which is the CS8641 problem the reorder
exists to avoid in the first place.

Guarded by `JsonFixedArrayUnmarshal`, which unmarshals JSON arrays into `[1]any`, `[2]int`, `[3]string`,
nested `[2][2]int` and a struct array — including the over-length (truncate) and under-length
(zero-fill) cases — and, in the other direction, asserts that the `default` arm still produces Go's
exact error for the targets it genuinely owns, so the guard cannot be an over-broad one.

## A clause's `else` may only be dropped when EVERY preceding clause terminates
Since seat #28 this governs only a switch with `fallthrough`: the shape below has none, so it now
emits as a pattern switch, and the excerpt from `src/core` shows the chain until the corpus is
regenerated. In the if-chain lowering the converter omits the `else` before a clause when the preceding clause
ended in a `return` — the chain is then unnecessary, since control cannot reach the later clause
anyway. Dropping the `else` is sound only when **every** preceding clause terminates; a *shallow* "the
last statement emitted was a return" flag reports only the **immediately** preceding clause.

For a `case` the difference is invisible: its condition cannot match a value an earlier case already
matched, so an unchained `if` simply evaluates to false. For `default:` it is silently fatal —
`default` has no condition and therefore *always* runs. os's `(*Process).wait` (Windows) is exactly this shape:

```go
switch s {
case syscall.WAIT_OBJECT_0:      // a bare `break` — falls OUT of the switch
	break
case syscall.WAIT_FAILED:
	return nil, NewSyscallError("WaitForSingleObject", e)
default:
	return nil, errors.New("os: unexpected result from WaitForSingleObject")
}
```

The `break` case is not a Go terminating statement, so the default keeps its `else`:

<!-- source: src/core/os/windows/exec_windows.cs:42-52 -->
```csharp
        if (exprᴛ2 == syscall.WAIT_OBJECT_0) {
            do {
                break;
            } while (false);
        }
        else if (exprᴛ2 == syscall.WAIT_FAILED) {
            (ps, err) = (default!, NewSyscallError(waitForSingleObjectˢ, e)); goto ᒐdone;
        }
        else { /* default: */
            (ps, err) = (default!, errors.New(osUnexpectedResultFromˢ)); goto ᒐdone;
        }
```

With the shallow flag, the returning `WAIT_FAILED` case would drop that `else`, and the default would
run as an unguarded block straight after the success path: every child-process wait would fail, and
the code would compile cleanly. The decision uses the accumulated `allCasesTerminal` check that the
trailing-`return default!;` logic relies on (genuine Go terminating-statement analysis, so an
`if { return }` with no `else` is correctly non-terminating), with the immediately-preceding-return
condition kept as one arm of the test, so the rule can only *add* an `else`. Guarded by
`SwitchFallthroughDefaultReturn`'s `waitShape` (non-constant case labels force the if-chain; it fails
without the rule).

## A `break` in a case that also `fallthrough`s must skip the fallthrough
A case body containing a Go `break` is wrapped in `do { … } while (false)` so the `break` has a C#
target in the if-chain lowering (the case above shows the wrapper). A case ending in `fallthrough`
raises a `fallthrough` flag the next clause's guard reads. **A case with both** cannot put the flag
*after* the wrapper:

<!-- illustration: not converter output -->
```csharp
if (exprᴛ1 == stdISO8601ColonTZ || …) { matchᴛ1 = true;
    do {
        if (len(value) >= 1 && value[0] == (rune)'Z') { value = value[1..]; z = ΔUTC; break; }
    } while (false);
    fallthrough = true;                    // ← would be reached by the break as well
}
```

or the `break` — which in Go exits the **switch** — would fall through instead. `time.Parse` is
exactly this shape: RFC3339's `Z07:00` layout element consumes a literal `Z` and breaks, and a
fallthrough would hand the remaining text to the numeric-offset arm, which rejects it (C# reporting
`cannot parse "Z07:00" as "Z07:00"` for every `Z`-terminated RFC3339 value routed through
`Time.UnmarshalText`).

Because Go's spec requires `fallthrough` to be the final non-empty statement in its clause, "control
reached the end of the body" *is* "the fallthrough statement was reached" — which is what lets the flag
be raised at the end rather than at the statement. In a break-wrapped case that equivalence holds only
**inside** the wrapper, so the assignment goes there and a `break` skips it:

<!-- source: src/core/time/format.cs:1312-1319 -->
```csharp
            do {
                if (len(value) >= 1 && value[0] == (rune)'Z') {
                    value = value[1..];
                    z = ΔUTC;
                    break;
                }
                fallthrough = true;
            } while (false);
```

A case with a `fallthrough` and no switch-`break` is unaffected (no wrapper exists), as is a case whose
only `break` belongs to a nested loop — `caseBodyHasSwitchBreak` already stops at a nested
loop/switch/select/closure, so no wrapper is emitted and the flag sits at the end of the body. Guarded by
`SwitchBreakBeforeFallthrough` (the `Z`-consuming break-then-fallthrough arm, a fallthrough arm with no
break, a nested-loop `break` that must still fall through, and a break with no fallthrough behind it —
output-compared vs Go).

## A `continue` in a break-wrapped case targets the LOOP, not the wrapper
A pattern switch needs none of this: its `break` exits the switch and its `continue` binds the loop,
both emitted bare. The wrapper, and so this rule, remain for a switch with `fallthrough`.

The `do { … } while (false)` switch-break wrapper is itself a C# **iteration statement**, and C# binds
`continue` to the innermost enclosing one — so a bare C# `continue` for a Go `continue` (meaning:
continue the enclosing `for`) inside a wrapped case would continue the *wrapper* instead, which exits
on its `false` condition and falls through past the switch into the rest of the loop body — the
symmetric twin of the fallthrough hazard above. `net/http`'s `ParseSetCookie` is the live corpus shape:
the `max-age` and `expires` cases each hold both a switch-`break` (the malformed-attribute bail-out)
and a loop-`continue` (the parsed-attribute accept), so a *successfully parsed* Max-Age/Expires would
run its case to completion and then fall through into `c.Unparsed = append(c.Unparsed, …)`.

Such a `continue` therefore lowers to a `goto` targeting a labeled empty statement at the very **end of the
enclosing loop's body**, where control reaches the loop's post-statement and condition exactly as
`continue` would. Section (h) of the guard keeps a `fallthrough`, and so the chain:

<!-- source: src/tests/Behavioral/SwitchBreakContinueWrapper/main.cs:136-160 -->
```csharp

    for (nint i = 0; i < len(words); i++) {
        var exprᴛ8 = words[i];
        var matchᴛ1 = false;
        if (exprᴛ8 == "skip"u8) { matchᴛ1 = true;
            do {
                if (i == 0) {
                    fmt.Println("h: breaking at", i);
                    break;
                }
                fmt.Println("h: continuing at", i);
                goto continueᴛ9;
            } while (false);
        }
        else if (exprᴛ8 == "stop"u8) {
            fmt.Println("h: stop at", i);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ8 == "keep"u8) { matchᴛ1 = true;
            fmt.Println("h: stop-or-keep at", i);
        }

        fmt.Println("h: after switch", i);
continueᴛ9:;
    }
```

The `break` exits the `do { … } while (false)` wrapper, which is Go's break-of-switch and the wrapper's purpose. The
`goto continueᴛ9` is Go's continue-of-loop: a bare C# `continue` would bind to the wrapper. Section (a) is the same
switch without the `fallthrough`: there the `break` and the `continue` are emitted bare inside the C# switch.

Mechanics, each load-bearing:

- **The label is minted per loop** (`continueᴛN`, the standard temp-name convention; nested loops each
  get their own) and **emitted only when some wrapped case actually targets it** — the loop's body
  suffix carries a marker that resolves to the labeled empty statement or to nothing once the body has
  been emitted, so every wrapper site *without* a loop-continue stays byte-identical (no label, no
  goto; an unconditional label would also draw CS0164).
- **The label precedes the per-iteration copy-backs.** A Go 1.22+ transformed loop (a body closure
  captures the clause variable) re-declares the variable from a carrier each pass and must copy the
  final value back before the post clause; the bare-`continue` emission writes those copy-backs inline
  at the continue site, but the goto path instead flows *through* them — the label sits with the
  `continue_<label>:` target, ahead of the copy-backs — so a wrapped continue in such a loop cannot
  leave the carrier stale (which would re-run the same index).
- **A continue belonging to a nested real loop inside the wrapped case stays a bare `continue`.** The
  emitter keeps a stack of continue targets — loop entries (for/range) and wrapper entries — and only a
  continue whose *innermost* entry is a wrapper takes the goto, targeting the nearest loop entry
  beneath; a for/range loop nested in the case body pushes its own entry and its continues bind it
  natively, exactly as Go requires.
- **Labeled `continue L` is untouched** — it already lowers to `goto continue_L`, which passes through
  the wrapper correctly, and the range/`foreach` form takes the same end-of-body label as `for`.

Guarded by `SwitchBreakContinueWrapper`: the defect shape in a `for` and in a `range` loop, an
unwrapped-continue control, a break-only wrapped control (byte-identity), a nested inner loop whose
continue must keep binding inward, a labeled `continue outer` through a wrapper, and the
per-iteration-capture loop whose wrapped continue must flow through the carrier copy-back — all
output-compared vs Go.

---

[← Defer / Panic / Recover](defer-panic-recover.md) · [Index](README.md) · [Type Switch Statements →](type-switch.md)
