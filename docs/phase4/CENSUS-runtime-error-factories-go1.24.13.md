# CENSUS — the runtime.Error factories behind TestRuntimePanicWithRuntimeError (go1.24.13)

> **Record type:** CENSUS (point-in-time, read-only). Amend with dated blocks; never rewrite; never execute from.
> **Lane:** C1 (cloud; Go only, no .NET SDK, so nothing here was RUN on the CLR: every claim is read at the
> tree, and each prediction is marked as one).
> **Asked by:** the 17:27 RULING (ledger, mailbox 3938d7b090), item (7) second half. This work was held from P1
> and goes to a live lane.
> **Read at:** master `1aebd6a885`; Go sources at GOROOT go1.24.13 `src/`.

## 0. The answer in one paragraph

Go's test recovers six runtime panics and asserts that each value implements `runtime.Error`
(runtime/crash_test.go:433-467). In Go, five of them are `runtime.plainError` and one is `runtime.boundsError`.
Here, five are golib `new PanicException("…")` STRINGS, which recover as `string`, and one is a raw .NET
`ArgumentOutOfRangeException`. That exception is not adopted by `RuntimeErrorPanic.TryAsPanic`, so
`recover()` never sees it and it escapes `panicValue`.

The fix already has a precedent in the tree: `RuntimeErrorPanic.IntegerDivideByZeroValue`, a golib hook that
runtime fills from `panicvalues_impl.cs` (the inverted dependency, because golib sits under runtime). The cut is
two more hooks of that shape: a plainError factory and a boundsError factory. It also needs about 9 golib
throw-site edits and one runtime registration file. The panic TEXT stays byte-identical, because
`PanicException.Message` already renders an `error` value by its `Error()`.

## 1. What Go's test expects (GOROOT runtime/crash_test.go:433, and Go's own raise sites)

| # | Case | Go raise site | Go value (type) | `Error()` text |
|---|---|---|---|---|
| 0 | `var m map[uint64]bool; m[1234] = true` | internal/runtime/maps `panic(errNilAssign)` (runtime_fast64_swiss.go:197); `errNilAssign` is runtime's `maps_errNilAssign` | `runtime.plainError` | `assignment to entry in nil map` |
| 1 | `close(ch); close(ch)` | chan.go:422 | `runtime.plainError` | `close of closed channel` |
| 2 | `close(ch); ch <- struct{}{}` | chan.go:226 | `runtime.plainError` | `send on closed channel` |
| 3 | `s := make([]int, 2); _ = s[2]` | panic.go:115 `goPanicIndex` | `runtime.boundsError{x: 2, signed: true, y: 2, code: boundsIndex}` | `runtime error: index out of range [2] with length 2` |
| 4 | `n := -1; _ = make(chan bool, n)` | chan.go:88 `makechan` | `runtime.plainError` | `makechan: size out of range` |
| 5 | `close((chan bool)(nil))` | chan.go:416 | `runtime.plainError` | `close of nil channel` |

`plainError` and `boundsError` both carry the `RuntimeError()` marker (runtime/error.go:110, :173). The
converted runtime already has both types and every `boundsErrorCode` (runtime/error.cs:111-170), and its
converted `goPanicIndex` already constructs one (panic.cs:103). **Nothing in the runtime package is missing. What
is missing is golib's path to it.**

## 2. What this tree does instead: the call sites (golib at 1aebd6a885)

The test's emission (runtime/crash_test.cs, the converted file) is exactly the Go shape:

- `m[1234] = true` on a `default!` map;
- `close(ch)`;
- `ch.ᐸꟷ(...)`;
- `_ = s[2]`;
- `_ = new channel<bool>(n)`;
- `close((channel<bool>)(default!))`.

`panicValue` is a `defer`/`recover()` frame whose catch filter is `GoFrame.IsPanic` → `TryAsPanic`
(GoFrame.cs:300).

| # | golib site (file:line → member) | Raises today | `recover()` yields |
|---|---|---|---|
| 0 | map.cs:297 `this[key].set`; also :358 `Add`, :374 `Set` | `new PanicException("assignment to entry in nil map")` | `string` (builtin.cs:262 boxes it as `@string`) |
| 1 | channel.cs:633 `ChanCore.Close()` | `new PanicException("close of closed channel")` | `string` |
| 2 | channel.cs:462 and :519 `ChanCore.Send(in T, bool)`; the SELECT twins :990 `SelectRuntime.Run` and :1044 `PollPassLocked` | `new PanicException("send on closed channel")` | `string` |
| 3 | `RuntimeErrorPanic.IndexOutOfRange` (golib/runtime/RuntimeErrorPanic.cs:115), reached from slice.cs:517 (`this[int]`, this case) and 9 other indexers (slice.cs:129,569; array.cs:286,297; string.cs:236,247; sstring.cs:90; sslice.cs:74,85) | `new PanicException(string.Format(…))` | `string` |
| 4 | channel.cs:1216 `channel(nint size)` `ArgumentOutOfRangeException.ThrowIfNegative(size)`; the same at :1236 `channel(nint, GoChanDir)`; **no check at all** at :1304 `channel(nint, ChanCargo?)` | a .NET `ArgumentOutOfRangeException`, which `TryAsPanic` (RuntimeErrorPanic.cs:231-259) does NOT map | nothing: the exception escapes `recover()` and ends the test |
| 5 | channel.cs:1378 `channel<T>.Close()` | `new PanicException("close of nil channel")` | `string` |

`channel<T>.Make` (channel.cs:1880, the `ISupportMake` route) funnels into the :1216 constructor, so it is
covered by the same edit.

## 3. The cut, sized (for the lane that takes it)

**golib `RuntimeErrorPanic`** gets two hooks beside `IntegerDivideByZeroValue`, with the same contract:
registered by runtime, and a plain-message fallback when runtime never loads.

- `public static Func<string, object>? PlainErrorValue { get; set; }`, and a factory
  `PanicException PlainError(string message)` → `new PanicException(PlainErrorValue?.Invoke(message) ?? message)`.
- `public static Func<long, long, bool, byte, object>? BoundsErrorValue { get; set; }` (x, y, signed, code), used
  by `IndexOutOfRange`: `BoundsErrorValue?.Invoke(index, length, true, 0 /* boundsIndex */) ?? <today's string>`.
  All 10 indexers change with it, and none of them is edited.
- `PanicException MakeChanSizeOutOfRange()` → `PlainError("makechan: size out of range")`.

**golib throw sites:**

- map.cs 3 sites → `RuntimeErrorPanic.PlainError(...)`.
- channel.cs 6 sites (462, 519, 633, 990, 1044, 1378) → `RuntimeErrorPanic.PlainError(...)`.
- The 3 channel constructors → `if (size < 0) throw RuntimeErrorPanic.MakeChanSizeOutOfRange();`. This
  REPLACES `ThrowIfNegative` at :1216 and :1236 and ADDS the missing check at :1304.

**runtime `panicvalues_impl.cs`** (hand-owned, `[module: GoManualConversion]`, no .go twin, so a reconvert never
emits over it): two more lines in `ᴛRegisterRuntimePanicValues`, both reaching the converted types.

- `PlainErrorValue = static m => (error)(plainError)(@string)m;`
- `BoundsErrorValue = static (x, y, s, c) => (error)new boundsError(x: x, signed: s, y: (nint)y, code: (boundsErrorCode)c);`

**Footprint:** golib 3 files (RuntimeErrorPanic.cs, map.cs, channel.cs) plus runtime 1 hand-owned file. No
converter change, so there is no reconvert footprint and no golden moves from emission. About 25 code lines.

**Text is unchanged, by construction.** `PanicException.Message` renders an `error` value as its `Error()`
(PanicException.cs:39-91). `plainError.Error()` is the bare string, which is exactly today's golib text (no
`runtime error:` prefix, as in Go). `boundsError.Error()` for boundsIndex, signed, is
`runtime error: index out of range [2] with length 2`, which is today's `IndexOutOfRangeMessage`. So every
signature, golden and `Message` assertion keeps its bytes. GolibTests quotes one of these texts
(TestExecutionOutputCapTests.cs:69, `panic: send on closed channel`), and it keeps them too.

## 4. Who else changes: the census, so no move is a surprise

**Go code that branches on a recovered value's TYPE** (GOROOT `src/`, excluding `cmd/` and `testdata/`):

- `.(runtime.Error)` / `case runtime.Error` appears at 9 sites. They are the target test; math/bits'
  bits_test.go:1038-1103, which is overflowError/divideError and already served by the divide hook; and
  **text/template** parse/parse.go:209 and exec.go:169.
- **text/template's change.** Both re-panic a `runtime.Error`. Today a golib runtime panic inside a template
  parse reaches `*errp = e.(error)` with a `string`, which is a SECOND panic (an interface conversion). After
  the cut it re-panics the original, which is Go's behavior. This is only on a path where a runtime error is
  already happening.
- `.(string)` on a value named `r`, `e`, `err` or `recover()` matches 20 lines. 8 of them are not recovered
  values: net/rpc, net/http's `wantErr`, gccgoimporter, pkix, x509 verify, and the registry test. The other 12
  each read a panic that their own package raises EXPLICITLY as a string: reflect all_test.go:2066 and
  `getError` at 8062/8095, time `checkZeroPanicString`, sync/atomic `shouldPanic` and the hammers
  (2146/2413/2872), sync.Cond, bufio, tabwriter, math/big natconv, and database/sql. None of them recovers a
  map, channel or index panic.

**Behavioral:** 51 Go sources call `recover()`. Of those that branch on the value's type (TypeSwitchImpureTag,
PanicRecover, WindowsNewCallback, TypeAssert, TypedNilInterface), every one recovers an explicit `panic(v)`. The
rest print the value, which keeps its text. **Predicted: no golden moves.**

**Siblings that stay strings after this cut** (named, same hook shape, NOT needed for this row):

- errorString family:
  - `NilPointerDereference`: Go's `memoryError`;
  - `MakeSliceLen/CapOutOfRange`: slice.go:29/33;
  - growslice at slice.cs:1426: slice.go:191;
  - hash of unhashable at GoEqualityComparer.cs:91/235: alg.go:144;
  - `ComparingUncomparableType`: alg.go:374;
  - `RangeFunctionContinued`: `rangeDoneError`.
- boundsError, other codes: `SliceBoundsOutOfRange` (11 callers; Go picks among 8 codes, so its hook needs a
  code mapping) and `ArrayConversionLength` (boundsConvert).
- plainError, the synctest texts: channel.cs:361/474/551/707/744/1112 (Go chan.go:194/321/538/702).
- `*runtime.TypeAssertionError`: error.cs:284 and builtin.cs:2497 (`interface conversion: …`). It carries
  `*_type` fields, so it is a different shape.

A lane that wants the whole family in one seat can take these; the row needs only the two hooks.

## 5. Predictions and falsifiers (for the cut; nothing here was run)

- **P1.** runtime -tests, windows and linux: `TestRuntimePanicWithRuntimeError` fail → PASS, and no other runtime
  verdict moves. Case 4 no longer ends the test early, so all six cases report.
- **P2.** GolibTests +N arms, red first on master. A plain-error arm per site class (map set/`Add`/`Set`, close
  closed, close nil, send closed, select send closed, makechan negative on each of the three constructors)
  asserts the recovered value's type is the registered one. It is red on master: a `string`, and a raw
  `ArgumentOutOfRangeException` for makechan. An index arm does the same for `boundsError`. Because GolibTests
  cannot name runtime's unexported types, the arms either assert "not a string, and its `Error()` equals the
  text" or go through a Go-prefixed public helper in runtime (the pattern pinner_impl.cs uses).
- **P3.** No banked row's verdict moves, and CNR is not needed: there is no converter change. The full
  behavioral suite gives 0 golden moves (§4).
- **The registration-timing bound, inherited from the divide hook and stated rather than assumed.** A program
  whose first touch of the runtime module comes AFTER the panic gets the string fallback. The test host
  touches runtime long before, so the row is unaffected. One behavioral probe (the six cases in a `main`
  that imports runtime only for `runtime.Error`) measures it for a plain program; if it reads `string`, that
  is a finding for the hook design, not a regression.
- **Falsifiers.** Any `Message` or golden text that moves (§3 says none can). Any banked row that moves. Any
  `recover().(string)` consumer of a map, channel or index panic that the §4 census missed.

## 6. Adjacent (named, not this row)

- **`channel(nint, ChanCargo?)` (channel.cs:1304) has no negative check.** `make(chan chan int, -1)` silently
  builds a core with a negative `Dataqsiz` and no buffer, where Go panics. The §3 edit closes it.
- **`ChanCore(nint size)` narrows `Dataqsiz = (int)size`** (channel.cs:399). A size above `int.MaxValue`
  truncates. Go panics only if `elemsize*size` overflows `maxAlloc`, so `make(chan struct{}, 1<<40)` is VALID
  in Go. Not sized here.
- **An unsigned index** (`this[ulong]` → `(nint)index`) reports a negative signed index where Go's boundsError
  has `signed: false`. The hook takes `signed`, so a follow-up can pass it.
