# Expression Switch Statements

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#expression-switch-statements)
Go expression-based `switch` statements are flexible: cases do not fall through automatically (no `break` needed), and the `fallthrough` keyword runs the next case body bypassing its expression. Based on the [Manual Tour of Go Conversions](https://github.com/ritchiecarroll/go2cs/tree/master/src/archived/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to `if / else if / else` is the best choice for most cases. When every case label is a C# **compile-time constant** and there is no `fallthrough`, a traditional C# `switch` works. "Constant" here means a C# `const` — a literal, a computed literal expression (`a + b`), or a *typed* basic-type const — not merely a Go constant. A case label that references a plain variable, a struct field (`case frame.fp`), an *untyped* / named-type / cross-package const emitted as `static readonly` (`case goarch.PtrSize`), or an address-of expression (`case &g`) is **not** a C# constant, so a C# `switch` case label there is invalid (CS9135 / CS0150). Such switches fall back to the `if / else if` form comparing the tag with `==` (a temp captures the tag: `var exprᴛ1 = tag; if (exprᴛ1 == frame.fp) …`). The same constant-vs-runtime-value test also chooses `is` (constant pattern) vs `==` for a single-value case within the if-else form. A Go `break` inside a case exits the *switch* (skipping the rest of the case); in the `if / else if` form there is no enclosing C# switch for it to target (CS0139), so a case body that contains such a break is wrapped in a `do { … } while (false)` — the break exits that one-shot loop, i.e. the case. The wrap is emitted only for a case whose body actually has a switch-targeting `break` (one not caught by a nested loop/switch/select), so every other case is unchanged. (A `break` inside a *nested* loop within the case still targets that loop, as in Go.) For cases that use `fallthrough`, the cases are expanded to standalone `if` statements with a local fall-through flag and `goto` to handle break-style exits — the most complex (and least pretty) scenario. In that if-chain form a **trailing `default:` reached via fallthrough** is emitted as a *guarded* `if (fallthrough || !match) { … }` — the guard is needed so the default does not run after a matched-but-non-fallthrough case, but C# cannot prove it always executes. So when such a guarded-default switch is the last statement of a **value-returning** function and every case is terminal, C# reports CS0161 ("not all code paths return a value") even though the Go `default` makes the switch exhaustive (runtime `startpanic_m`). Because a guarded-terminal-default switch cannot be legally followed by reachable Go code (it always returns/exits), the converter emits an unreachable `return default!;` after the if-chain to satisfy C#'s definite-return analysis — gated on the enclosing function/literal actually returning a value (via its own return signature), so a `void` function or a switch that isn't terminal is unaffected. (Guarded by the `SwitchFallthroughDefaultReturn` behavioral test; cleared runtime's CS0161.) A comparison case may use a C# relational/constant pattern (`case {} when x is < 0`) only when the compared-to operand is a C# compile-time constant; for a variable (`case x == y`) or a `static readonly` const (untyped/cross-package), it falls back to a `when` guard (`case {} when x == y`) — a relational pattern there is invalid (CS9135).

## A switch on a `static readonly` constant tag lowers to if-else
A switch TAG that is itself a constant emitted as `static readonly` -- an untyped const's `UntypedInt` wrapper (`switch goarch.PtrSize`, reflect abi.go) or a `uintptr`-struct const -- cannot govern a C# switch: the int case labels are not constants OF the wrapper struct type, and the `is` constant-pattern lowering fails the same way (CS9135). The recorded tag type is no help (go/types records the untyped constant's DEFAULT type in tag position), so the gate is on the object resolution: a constant-valued tag that is not a true C# `const` forces the if-else form (wrapper `==` operators) and disables the `is` pattern:
```csharp
var exprᴛ1 = CrossPkgLib.Precision;
if (exprᴛ1 == 1) {
```
A variable tag stays switchable. Guarded by `CrossPkgUser` / `CrossPkgLib`.

## A leading constant-true case stays opaque to the compiler
Go's `switch { case true: ... case cond: ... }` (time parseStrictRFC3339 deliberately disabling its strict checks) compiles the LATER cases as dead code; a foldable `when true` makes C# reject them outright (CS8120). A constant-true case condition on a NON-LAST clause therefore emits the golib `ᐧᐧ` marker -- a `static readonly bool` the compiler cannot fold:
```csharp
case {} when ᐧᐧ: {
```
The marker is deliberately SEPARATE from the const `ᐧ` switch governor: that const's foldability is itself load-bearing (`case ᐧ when ...` label patterns need a constant, and an infinite `for (...; ᐧ ;...)` relies on the fold for reachability proofs -- CS9135/CS0161 when it was made readonly in place). Guarded by `ExprSwitch`.

## No constant pattern against a named-numeric wrapper
A constant expression whose CONTEXTUAL type is a wrapper struct -- golib `uintptr` or any `[GoType("num:...")]` named numeric (time's `Duration`) -- can never be a C# constant, so no constant/relational pattern can compare against it: `d is >= 0` types the literal 0 as Duration (CS9135). The lowering keeps the plain operator form (`d >= 0`, the wrapper's operators). Guarded by `ExprSwitch` (the `pace` switch).

## An index-expression case label falls back to equality
A case label that INDEXES a package-level array/slice variable (`case Typ[UntypedNil]:` — go/types
operand.go, where `Typ` is the universe `*Basic` array) is a runtime value, never a C# constant. The
single-value `is` form is doubly broken there: C# parses `exprᴛ1 is Typ[UntypedNil]` in pattern position
as an array TYPE (CS0246 + CS0270). `canUsePatternMatch` rejects an `*ast.IndexExpr` label the same way
it rejects a non-constant identifier/selector, so the clause takes the `==`/`AreEqual` comparison the
multi-value arm already produced:
```csharp
if (AreEqual(exprᴛ1, Typ[UntypedNil])) {   // NOT `exprᴛ1 is Typ[UntypedNil]`
```
(Guarded by the `IndexExprCaseLabel` behavioral test — single- and multi-label clauses indexing a
package-level array var, output-compared vs Go.)

## Literal case labels under a named-type tag compare through a cast

A tagged switch whose tag type is a NAMED (non-interface) type — net/http's
`func (code socksReply) String()` switching on `code` — renders the tag as a `[GoType]` wrapper
struct. An untyped-LITERAL label adopts the tag's named type in Go (go/types records it on the label
expression), but its C# render is a bare literal of the UNDERLYING type, which can neither be a
constant pattern (`exprᴛ1 is 0x01` — CS9135, constant pattern against the wrapper) nor compare bare
(`exprᴛ1 == 0x01` is ambiguous between the wrapper's `==` and the underlying's built-in `==`, both
reachable through the wrapper's two-way implicit operators — the same ambiguity family as the
named-string consts). Two converter pieces:

1. the pattern-match decision excludes any named-wrapper tag (`tagIsNamedWrapper`, beside the
   existing `namedTypes`/`tagIsStaticReadonlyConst` gates — those could not catch the mixed
   const-ident + literal switch, because the per-label screening short-circuits once `allConst`
   goes false and never reaches its named-type check);
2. a CONSTANT label that is not an ident/selector/conversion-call (those already render AT the
   wrapper type) casts to the tag type:

```csharp
var exprᴛ1 = code;
if (exprᴛ1 == socksStatusSucceeded) {     // named-const label — no cast
    return "succeeded"u8;
}
if (exprᴛ1 == (socksReply)(0x01)) {       // literal label — cast to the tag type
    return "general SOCKS server failure"u8;
}
```

This also repairs the ALL-literal switch over a named type, which previously emitted the ambiguous
bare `==` form. Full-stdlib footprint: 12 files (socks_bundle, archive/zip, encoding/xml,
go/printer, go/types, internal/poll, syscall zsyscall shims). (Guarded by `NamedNumericSwitchLiteral`
— a mixed named-const + literal switch including a multi-label clause, and an all-literal switch,
output-compared vs Go.)

## A trailing `default` in a switch WITH `fallthrough` is guarded on `!match`
A Go `switch` with no fallthroughs lowers to a plain `if / else if / … / else { default }` chain, where
the trailing `else` correctly runs the default only when no case matched. But a `fallthrough` **breaks
the chain**: the case that `fallthrough` targets is emitted as a SEPARATE, `!match`-guarded `if`
(`if (fallthrough || !match && <labels>) { … }`) so it can be entered both by falling through and by a
direct match. A trailing `default` after such a case was emitted as that `if`'s bare `else` — which
fires whenever the fallthrough-target `if` is false, i.e. **after any matched NON-fallthrough case**, not
only when nothing matched. fmt's `printValue` is exactly this shape:

```go
switch f.Kind() {
case reflect.Int, …: p.fmtInteger(…)          // a matched non-fallthrough case
case reflect.Pointer: … ; fallthrough
case reflect.Chan, reflect.Func, reflect.UnsafePointer: p.fmtPointer(f, verb)
default: p.unknownType(f)                        // wrongly ran after fmtInteger
}
```

so formatting an `int` slice element ran `fmtInteger` AND then `unknownType` (→ reflect name
resolution → a `resolveNameOff` stub → panic), breaking `%v` of every composite. The trailing default
is now emitted `else if (!match) { /* default: */ }` (matchVarName). This is byte-equivalent to the
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
shapes reach the chain, and the corpus held one live instance of each:

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
source-order-sensitive. It was emitted `if (fallthrough || !matchᴛ1)`, and for a slice or array target
`matchᴛ1` was still `false` (only the `Interface` arm had run), so the error arm fired and the
`Array, Slice` arm below it was unreachable. `json.Unmarshal` therefore failed **every** JSON array
whose target was not a bare `interface{}` — `json: cannot unmarshal array into Go value of type
[1]interface {}` for a fixed-size array (which is how `net/rpc/jsonrpc` passes its `[1]any` params),
and the identical error for every `[]T`. The default now reads the **precomputed** any-case-match
already built for the mirror shape — the OR of every case condition in the switch, materialized ahead
of the chain — so it fires only when nothing matches:

```csharp
var exprᴛ1 = v.Kind();
var matchᴛ1 = false;
var matchᴛ2 = exprᴛ1 == reflect.ΔInterface || (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice);
if (exprᴛ1 == reflect.ΔInterface) { matchᴛ1 = true; … fallthrough = true; }
if (fallthrough || !matchᴛ2) { /* default: */ … }
if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) { matchᴛ1 = true; … }
```

**Participating in no fallthrough at all.** For a non-constant-label tagged switch the converter
already normalized this shape by *moving* the default clause to the end. A **constant**-label switch
that has fallthroughs elsewhere also lowers to the chain, and that reorder gate did not cover it — so
the un-moved default emitted as a **bare block**, with no condition at all, and ran unconditionally.
`internal/bisect`'s `parsePattern` leads with `default: return …parseError`, so every bisect pattern
parse returned "invalid pattern syntax" and all six digit arms below it were dead code.
(`regexp/syntax`'s `parseEscape` has the identical shape and survived only by luck: its default body
re-tests `!isalnum(c)`, which happens to exclude every one of its own case labels.)

These are now guarded **in place** — `if (!matchᴛ2) { /* default: */ … }` — rather than reordered.
Widening the reorder gate was tried and rejected: reordering moves the clause's *statements*, but
comments attach by source position, so `parseEscape`'s default-clause commentary migrated up into the
octal arm above it. Guarding in place fixes the same defect with no code motion, keeps the emitted C#
in Go's clause order, and costs nothing structurally — emitting an `if` instead of a bare block is
precisely what lets the following clause keep its `else`, which is the CS8641 problem the reorder
exists to avoid in the first place.

Guarded by `JsonFixedArrayUnmarshal`, which unmarshals JSON arrays into `[1]any`, `[2]int`, `[3]string`,
nested `[2][2]int` and a struct array — including the over-length (truncate) and under-length
(zero-fill) cases — and, in the other direction, asserts that the `default` arm still produces Go's
exact error for the targets it genuinely owns, so the fix cannot be an over-broad one that drops the
guard.

## A clause's `else` may only be dropped when EVERY preceding clause terminates
In the if-chain lowering the converter omits the `else` before a clause when the preceding clause
ended in a `return` — the chain is then unnecessary, since control cannot reach the later clause
anyway. That decision was driven by a *shallow* "the last statement emitted was a return" flag, which
reports only the **immediately** preceding clause. Dropping the `else` is sound only when **every**
preceding clause terminates.

For a `case` the mistake is invisible: its condition cannot match a value an earlier case already
matched, so the unchained `if` simply evaluates to false. For `default:` it is silently fatal —
`default` has no condition and therefore *always* runs. os's `(*Process).wait` is exactly this shape:

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

The `break` case is not a Go terminating statement, but the returning `WAIT_FAILED` case set the flag,
so the default emitted as an **unguarded** block that ran straight after the success path:

```csharp
if (exprᴛ2 == syscall.WAIT_OBJECT_0) { do { break; } while (false); }
else if (exprᴛ2 == syscall.WAIT_FAILED) { … return; }
{ /* default: */ … return errors.New("os: unexpected result from WaitForSingleObject"); }
```

Every child-process wait therefore failed — and it compiled cleanly the whole time. The decision now
uses the accumulated `allCasesTerminal` check that the trailing-`return default!;` logic already
relies on (genuine Go terminating-statement analysis, so an `if { return }` with no `else` is
correctly non-terminating). The original condition is kept as one arm of the test, making the change
strictly *add* an `else` where one was missing and never remove one, so no already-correct emission
changes. Guarded by `SwitchFallthroughDefaultReturn`'s `waitShape` (non-constant case labels force the
if-chain; verified to fail without the fix).

## A `break` in a case that also `fallthrough`s must skip the fallthrough
A case body containing a Go `break` is wrapped in `do { … } while (false)` so the `break` has a C#
target in the if-chain lowering (the case above shows the wrapper). A case ending in `fallthrough`
raises a `fallthrough` flag the next clause's guard reads. **A case with both** put the flag *after*
the wrapper:

```csharp
if (exprᴛ1 == stdISO8601ColonTZ || …) { matchᴛ1 = true;
    do {
        if (len(value) >= 1 && value[0] == (rune)'Z') { value = value[1..]; z = ΔUTC; break; }
    } while (false);
    fallthrough = true;                    // ← reached by the break as well
}
```

so the `break` — which in Go exits the **switch** — fell through instead. `time.Parse` is exactly this
shape: RFC3339's `Z07:00` layout element consumes a literal `Z` and breaks, and the fallthrough handed
the remaining text to the numeric-offset arm, which rejected it. Go reports
`extra text: "07:00"`; C# reported `cannot parse "Z07:00" as "Z07:00"`, and every `Z`-terminated
RFC3339 value routed through `Time.UnmarshalText` failed the same way.

Because Go's spec requires `fallthrough` to be the final non-empty statement in its clause, "control
reached the end of the body" *is* "the fallthrough statement was reached" — which is what lets the flag
be raised at the end rather than at the statement. In a break-wrapped case that equivalence holds only
**inside** the wrapper, so the assignment moves there and a `break` skips it:

```csharp
    do {
        if (…) { …; break; }
        fallthrough = true;
    } while (false);
```

A case with a `fallthrough` and no switch-`break` is unaffected (no wrapper exists), as is a case whose
only `break` belongs to a nested loop — `caseBodyHasSwitchBreak` already stops at a nested
loop/switch/select/closure, so no wrapper is emitted and the flag stays where it was. Guarded by
`SwitchBreakBeforeFallthrough` (the `Z`-consuming break-then-fallthrough arm, a fallthrough arm with no
break, a nested-loop `break` that must still fall through, and a break with no fallthrough behind it —
output-compared vs Go).

## A `continue` in a break-wrapped case targets the LOOP, not the wrapper
The `do { … } while (false)` switch-break wrapper is itself a C# **iteration statement**, and C# binds
`continue` to the innermost enclosing one — so a Go `continue` (meaning: continue the enclosing `for`)
inside a wrapped case continued the *wrapper* instead, which exited on its `false` condition and fell
through past the switch into the rest of the loop body. The Go `continue`'s intent was silently
discarded; the wrapper retargeted `break` but never considered `continue` — the symmetric twin of the
fallthrough hazard above. `net/http`'s `ParseSetCookie` is the live corpus shape: the `max-age` and
`expires` cases each hold both a switch-`break` (the malformed-attribute bail-out) and a loop-`continue`
(the parsed-attribute accept), so a *successfully parsed* Max-Age/Expires ran its case to completion and
then fell through into `c.Unparsed = append(c.Unparsed, …)` — Expires **and** RawExpires parsed
correctly, yet the raw attribute also landed in `Unparsed`, which is only possible when the case body
runs to completion and falls out.

Such a `continue` now lowers to a `goto` targeting a labeled empty statement at the very **end of the
enclosing loop's body**, where control reaches the loop's post-statement and condition exactly as
`continue` would:

```csharp
for (nint i = 0; i < len(words); i++) {
    var exprᴛ1 = words[i];
    if (exprᴛ1 == "skip"u8) {
        do {
            if (i == 0) {
                fmt.Println(aBreakingAtˢ, i);
                break;                     // Go break-of-switch — exits the wrapper (its purpose)
            }
            fmt.Println(aContinuingAtˢ, i);
            goto continueᴛ1;               // Go continue-of-loop — a bare C# continue would bind the wrapper
        } while (false);
    }
    else if (exprᴛ1 == "stop"u8) {
        fmt.Println(aStopAtˢ, i);
    }

    fmt.Println(aAfterSwitchˢ, i);
continueᴛ1:;
}
```

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

## Labeled switches declare their break target

- **Labeled switches declare their break target** (`break_BigSwitch:;` after the switch —
  both switch visitors now mirror visitForStmt, CS0159). Guarded by `SwitchBreakInCase`
  (`pick`).

## Empty-interface switch tags compare via AreEqual

and **empty-interface switch tags compare via AreEqual**
  (`switch err := recover(); err { case ErrLarge: }`, CS0019).

---

[← Defer / Panic / Recover](defer-panic-recover.md) · [Index](README.md) · [Type Switch Statements →](type-switch.md)
