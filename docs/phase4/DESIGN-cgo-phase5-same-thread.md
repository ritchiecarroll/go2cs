# DESIGN — cgo Phase 5, the same-thread slice: `//export` callbacks that C calls synchronously

> **STATUS: DESIGN, posted before code (lane H, 2026-11-27; branch `claude/h-cgo-p5-design`, docs only, cut from master
> 317b273e58).** COORD asked for it on its own branch so G can read §5 now. No golib code is written until R's F1 cut
> lands on master. The slice is built on top of Phase 4 (`claude/h-cgo-p4-sidebuild`) and Phase 4b, which carry the
> side-built library its red uses.
>
> **Placement (the owner, 2026-11-27):** the same rule as 4b. It joins .5 only if it fits before the M:N freeze. If it
> is very close when M:N finishes, the owner decides then; otherwise it rides .6. The foreign-thread case stays out.
>
> **Review:** G reads §5 (the M:N interaction) and OQ-1, then the usual seat review. The owner rules OQ-1 with G's
> read.

## 1. Target shape: go-sqlite3 v1.14.53 (census of 2026-11-27, read only)

`callback.go` is in every build of the package, on every OS and with or without `libsqlite3`. It carries nine
`//export` trampolines. Its preamble only DECLARES, as cmd/cgo requires of a file with `//export`
(`cmd/cgo/doc.go:324-329`). `sqlite3.go`'s preamble also declares the trampolines' C prototypes.

The trampolines are never called by name from C in the default build. Go hands their addresses to C as function
values, and SQLite calls through those pointers:

```go
C.sqlite3_commit_hook(c.db, (*[0]byte)(C.commitHookTrampoline), newHandle(c, callback))
rv := sqlite3CreateFunction(c.db, cname, C.int(numArgs), C.int(opts), newHandle(c, &fi), C.callbackTrampoline, nil, nil)
```

So the slice is two shapes, which must land together:

- **E1, the export itself.** A Go function marked `//export f` gets a native entry point C can call.
- **E2, the function value.** `C.f`, where f is an export, is used as a value, typically `(*[0]byte)(C.f)` or a
  parameter of C function-pointer type. Today it is refused: "a C function's address used as a value: not supported
  yet" (cgoFuncValue).

**Out of this slice:**
- C calling an export BY NAME. That needs a C-side symbol for the export inside the side library (OQ-2). go-sqlite3's
  default build has no such call; `sqlite3_opt_unlock_notify.c` (tag `sqlite_unlock_notify`) does.
- Any callback on a thread Go did not enter C from, which is the foreign-thread case.

## 2. Same-thread, defined

A callback is **same-thread** when it runs on the OS thread of a goroutine that is, at that moment, inside a cgo call
the converted code made. Here "inside" means golib's native-call depth for that goroutine is odd: the call wrapper
called the runtime's entersyscall family, which calls `GoNativeCall.Enter`, and has not yet returned.

SQLite runs every default-build callback inside the `sqlite3_*` call that triggers it, on the calling thread:
- scalar and aggregate functions, and collation
- the commit, rollback and update hooks
- the authorizer

That is SQLite's documented design, read from the code paths, and NOT yet measured. The exit gate measures it.

## 3. E1: the export's entry point

For each `//export f` the converter emits, in the pseudo-package, an `[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]` static thunk
`fᴍexport` over the C types of cmd/cgo's prototype for f. The C primitives are the ones the Phase 2 externs use
(`cgoScalar.primitive`). Pointers cross as `nint`.

The thunk's body, in order:

1. **Same-thread check.** If `Goroutine.Current` is null, or its native depth is even, the thread is not inside a cgo
   call of this process's Go code. The thunk ends the program by name: "cgo callback on a thread go2cs did not enter C
   from (cgo Phase 5 takes same-thread callbacks only)". Go would run the callback (needm). This slice refuses, loudly,
   rather than run Go code with no goroutine.
2. **`GoNativeCall.ReenterManaged()`**, Go's `cgocallbackg` exitsyscall. Under M:N this is where a P is taken back
   (§5).
3. **The arguments, C to Go.** Scalars convert as the Phase 2 wrappers' `fromPrimitive`. A C pointer becomes a box
   minted over C memory (`GoCgoArgs.PointerMemberBack`, the native-box door Phase 3 uses for results). An
   `unsafe.Pointer` becomes its number. A Go pointer the callback receives back (the `newHandle` uintptr in
   go-sqlite3) is a number Go code converts itself; go-sqlite3 uses handle tables, not raw Go pointers, as cgo's
   pointer rules require.
4. **The Go function**, called as the converted code names it.
5. **The result, Go to C.** `toPrimitive`. A Go pointer result is refused at conversion, as cgo forbids returning one.
6. **`finally`: `GoNativeCall.ReturnToNative()`**, Go's `reentersyscall`.

**Panics (OQ-1).**

*What Go does* (GOROOT 1.26.11, `src/runtime/cgocall.go`): Go does NOT abort on a callback's panic.
- `cgocallbackg1` defers `unwindm` before it calls the export (lines 438-439).
- If the export panics, the panic unwinds the goroutine stack and, in lock step, the M's g0 stack, past the C frames
  that called back. `unwindm` restores `m.g0.sched.sp`, ends the `cgocall` accounting and undoes the callback's
  `lockOSThread` (lines 489-519; the file's header comment, lines 60-72).
- The panic arrives at the Go code that made the C call. A `recover()` there stops it, and the program goes on.
- The C frames in between never run again, and C cleanup they would have done is skipped. A lock SQLite held, for
  example, stays held.

*What go2cs cannot do:* an exception cannot cross an `[UnmanagedCallersOnly]` frame (the runtime ends the process),
and .NET has no portable way to unwind native frames.

*Which go-sqlite3 callbacks can panic* (v1.14.53, read):
- All nine trampolines can panic, through user code: scalar and aggregate functions, collations, the commit,
  rollback, update and pre-update hooks, and the authorizer.
- Each also type-asserts its handle lookup unchecked (`lookupHandle(...).(*functionInfo)`, `.(func() int)` and so on),
  so an unknown handle panics too.
- No trampoline and no other file in the package calls `recover()`, so a panic in a user callback always reaches the
  Go caller under go.

*The options, stated honestly:*
- **(i) Fatal at the thunk.** The thunk catches the panic and ends the program with the panic's Go-format report plus
  a line naming the callback.
  - Simple.
  - A PARITY DIVERGENCE whenever the Go caller recovers: go runs on, go2cs dies.
  - Where no one recovers, both end the program. The exit code and the panic line can be made to match; the
    traceback's C part cannot.
- **(ii) Record, return zero, re-panic after C returns.**
  - The thunk catches the panic and records it on the goroutine, as a pending panic for the innermost cgo call. It
    returns the C result's zero value to C.
  - Every further callback into this goroutine during the same C call returns zero at once and runs no Go code,
    which approximates "Go abandoned the callback".
  - When the C call returns, its wrapper, after `Exit`, rethrows the recorded panic with its original trace. A
    `recover()` in the Go caller then sees it, as under go.
  - Closer to Go's observable behaviour, and recover parity holds. The divergence moves into C: C runs on after the
    callback returns zero, where under go it never runs again.
  - Is that safe? For go-sqlite3 it is arguably safer than go's own behaviour. A zero result is an ordinary callback
    answer that SQLite handles: a commit hook's 0 lets the commit proceed, a collation's 0 means equal, an
    authorizer's 0 is SQLITE_OK. SQLite's locks and state stay consistent, where go's unwind leaves SQLite mid-call.
    But it is not Go's semantics. A C library whose zero result means "do something irreversible" would do it.
    Generally safe, therefore, only when zero is a benign answer, which the converter cannot know.
- **(iii) Nothing else found.** Native unwinding past C frames is not available. Windows SEH could unwind native
  frames, but a managed exception may not cross `[UnmanagedCallersOnly]`. Choosing per export (a declared "panic
  result") would need input from the user. That is a possible extension of (ii), not a third mechanism.

*Recommendation:* (ii), with its divergence written into the refusal-and-disclosure text and a row measuring recover
parity against go. Use (i) only if G finds that the pending-panic state cannot be kept per goroutine under M:N. The
owner rules OQ-1 with G's read (COORD, 2026-11-27).

Rows for either choice, compared with `go run`:
- an unrecovered panic, where both end the program;
- a panic the Go caller recovers, where (i) diverges and (ii) matches;
- a second callback in the same C call after a panic, where (ii) returns zero without running Go code.

## 4. E2: the export as a function value

cmd/cgo declares `C.f`, used as a value, as `_Cfpvar_fp_f` (kind cgoFuncValue), whose address is the export's C
wrapper. go2cs gives that value the thunk's address:

```csharp
(nint)(delegate* unmanaged[Cdecl]<nint, int, nint, void>)&callbackTrampolineᴍexport
```

The value is typed as cmd/cgo types it (`*[0]byte` and its conversions), through the void-pointer door, as a number
that is C memory. That is true: the thunk is native-callable code, and no Go storage backs it. A function value whose
C function is NOT an export keeps today's refusal.

## 5. The M:N interaction (for G)

Under GO2CS_MN=1, a multiplexed goroutine entering a cgo call with `MayBlock` keeps its P. The monitor retakes the P
from a call that lasts two ticks (DESIGN-mn-n5-invisible-blocking.md 3b). A callback can arrive after that retake.

| Question | Go's answer | This slice's need |
|---|---|---|
| Does the callback hold a P while Go code runs? | Yes. `cgocallbackg` calls `exitsyscall`, which takes a P, waiting if none is free. | `ReenterManaged` must take a P back when the call's P was handed off or retaken. Today it only adjusts the depth and the blocked marks. This is F1's seam, so Phase 5 waits for F1. **Superseded by §9 (a):** a P comes back only for a Blocks call that handed it off; a retaken MayBlock call's callback runs P-less. |
| Can the goroutine migrate threads during the callback? | No. `cgocallbackg` locks it to its M (`lockOSThread`) for the callback, since C frames are on that thread's stack. | The goroutine is pinned to its thread for the callback's duration, as `LockOSThread` pins it. G: does golib's M:N need an explicit pin here, or does a goroutine whose thread is inside native frames never move by construction? **Answered by §9 (b):** no pin is needed, by construction. |
| Can the callback block (channel, mutex, sleep)? | Yes. The M is held, and the P is released while it blocks. | A block inside a callback parks the goroutine with C frames beneath it. The thread cannot run other goroutines meanwhile. Under 1:1 that is today's behaviour. Under M:N it costs one M for the duration, as in Go. G to confirm the scheduler tolerates an M held this way. **Answered by §9 (c):** tolerated, as in Go. |
| What happens when the callback returns to C? | `reentersyscall` releases the P again. | `ReturnToNative` must release, or hand off, the P as `Enter` with the call's hint does. **Superseded by §9 (d):** no release. ReturnToNative is MayBlock and keeps the P. |
| Nested Go→C→Go→C | Allowed, with depth alternating. | The depth parity already refuses out-of-order transitions. One row nests a C call inside a callback. |

The 1:1 model (the default) needs none of the P handling. `ReenterManaged` and `ReturnToNative` already keep the
depth and the blocked marks, so the slice can be measured under 1:1 first and under GO2CS_MN=1 after F1.

## 6. Refusals, by name

| Shape | Refusal |
|---|---|
| a callback on a thread no goroutine entered C from | ends the program by name at run time (§3, step 1) |
| an export whose signature carries a Go string, slice, map, interface or func | refused at conversion, as cgo's own export rules restrict them (_GoString_ only through its typedef) |
| an export returning a Go pointer | refused at conversion |
| C calling an export by name | refused at conversion, naming the C file and the export (OQ-2) |
| a file with both `//export` and preamble definitions | cmd/cgo refuses this itself (doc.go:324-329) |

## 7. Red first, and the gates

1. **Red:** `TestCgoPhase5ASameThreadCallbackRunsAsGoDoes`. One package:
   - a preamble static `int call_twice(int (*f)(int), int v) { return f(f(v)); }`, a side-built Phase 4 helper;
   - an `//export twice_go` in another file;
   - main passing `(*[0]byte)(C.twice_go)` to it, plus a hook-style `void*` handle round trip.

   Red today: E2's refusal. Arm 2 (cgo-e2e) compares with `go run` on all four flavours.
2. **Rows:**
   - the unrecovered-panic report (OQ-1), compared with `go run`'s crash: exit code, plus the panic value line
   - a nested Go→C→Go→C call
   - a C pointer argument read in Go
3. **Showcase gate:** go-sqlite3 v1.14.53 converts and passes a chosen set of its own tests against `go test`. This
   needs Phase 4b too, and the go-sqlite3 cross-file gap (BOARD, 2026-11-27).
4. **Seat gates as Phase 4:**
   - unfiltered go test, and GolibTests R/D plus TestLicensing for the golib part
   - repoguard
   - converter-cgo and cgo-e2e ×4, with `^TestCgoPhase5` added to cgo-e2e's list
   - one GO2CS_MN=1 leg after F1, named as such

## 8. Open questions

- **OQ-1: the panic policy** (§3). Go unwinds a callback's panic through C to a recoverable Go caller (cgocall.go
  438-439, 489-519). Option (i), fatal at the thunk, diverges whenever that caller recovers. Option (ii), record,
  return zero, re-panic after C returns, keeps recover parity and lets C run on. Recommendation: (ii). The owner rules
  with G's read.
- **OQ-2: C calling an export by name.** It needs a C symbol in the side library that forwards through a pointer
  go2cs sets at load, with the prototype from cmd/cgo's `_cgo_export.h`. Recommendation: the next increment, after
  this one is measured.
- **OQ-3: foreign threads** (needm). Out, by ruling.
- **OQ-4: what F1 makes `ReenterManaged` do under M:N.** G and R's answer bounds §5's table. The note is amended when
  F1 lands.

## 9. G's rulings on §5 and OQ-1 (2026-11-27)

G read §5 and OQ-1 at 87e2d633c8, against master 317b273e58 (GoNativeCall; GoScheduler's EnterBlocked/ExitBlocked,
retake and step end), F1 as approved, and Go 1.26.11 `cgocall.go`. The rulings stand as written here, and the §5
table rows they supersede are marked there.

**§5:**
- **(a) A P on re-entry.** After F1, `ReenterManaged` takes a P back ONLY for a call that handed its P off
  (`Blocks`). A `MayBlock` call that the monitor RETOOK runs its callback without a P. F1's Q-b, which would cover
  that case, is OUT of .5, as a BOARD row for after .5.
  - It is tolerable: the callback's Go code runs beside GOMAXPROCS others.
  - A park inside the callback heals it. EnterBlocked swaps Retaken 1 -> 0 without a release, and the wake's
    ExitBlocked takes a P.
- **(b) Pinning.** None is needed, by construction. A multiplexed goroutine changes carrier only between steps, at a
  twin's await. The C call is a synchronous P/Invoke inside a step, and the callback runs the synchronous Go function
  on that stack, so the step cannot end and the goroutine cannot move until C returns. A user's
  LockOSThread/UnlockOSThread inside a callback is DESIGN-mn-pin's, as anywhere.
- **(c) A blocking callback** is tolerated, as in Go.
  - A park in synchronous code takes the plain-park path: EnterBlocked releases the P (unless it was retaken) and
    backfills a carrier. The thread is held; the P is not.
  - While the callback runs Go code, the depth is even, so the monitor sees running Go: 3c injection, and 3a sensing
    for a managed wait.
- **(d) ReturnToNative releases nothing.** F1's (2) ruled it MayBlock, since Go's `reentersyscall` is `entersyscall`,
  not `entersyscallblock`. The P stays, and the monitor retakes it if the rest of C runs for two ticks.
- **(e) Nesting** rides the depth parity. After F1, every EnterBlocked meets one ExitBlocked, at any depth.

**A trap in step 1 (§3).** `GoNativeCall.TransitionDepth` reads -1 when there is no goroutine, and -1 is ODD. The
same-thread check tests depth < 0 FIRST. Otherwise `ReenterManaged` goes inert and Go code runs with no goroutine.
Test threads do carry a goroutine (Goroutine.Enter).

**Rows added.** These run under GO2CS_MN=1 at GOMAXPROCS=1, after F1, and are counted by F1's blocked= and p-free=
keys:
- **(r1)** A callback blocks on a channel another goroutine fills, once under a Blocks outer call and once under a
  MayBlock outer call. It completes, and both counts return to base.
- **(r2)** C holds a MayBlock call past two ticks, then calls back (the retaken path). It completes, and the counts
  are at base.
- The nested row also runs under GO2CS_MN=1.

**OQ-1.** G reads (ii) as workable under M:N and recommends it to the owner, on five conditions:
1. **The state.** By (b), the goroutine is fixed for the whole C call, so per-goroutine state is exact. There is one
   slot on the Goroutine, beside m_nativeDepth: the captured ExceptionDispatchInfo, and the depth of the C call it
   belongs to. It is never a thread-static (the seam design's 12.1).
2. **One slot covers nesting.** An inner record drains when the inner C call returns, before the panic reaches the
   outer thunk, which records it again for the outer call. The slot is asserted empty when a panic is recorded.
3. **The drain is in GoNativeCall.Exit, not in the cgo wrapper.**
   - Exit is the one seam every native call passes, a syscall funnel's and a consumer's own bracket included.
   - A wrapper-only drain would leave a stale slot, which short-circuits the next call at that depth.
   - Exit rethrows after the depth and P bookkeeping.
   - Exit throwing is new to the frozen contract. The owner's OQ-1 ruling is its recorded decision, landed as a dated
     amendment to DESIGN-native-call-seam and to Exit's documentation.
4. **The short-circuit runs BEFORE ReenterManaged.** It runs no Go code, so it must not wait for a P to return zero.
5. **The thunk catches every exception.** Not every runtime panic in converted Go is a PanicException, and anything
   that escapes [UnmanagedCallersOnly] ends the process. A GoexitException (t.FailNow inside a callback) uses the same
   slot. It gets a row of its own against go run, because Go's Goexit through C frames is not yet probed.

**Still held:** no golib code before F1 lands. r1, r2 and the GO2CS_MN=1 leg run on F1's master. The owner rules
OQ-1 with this read.

**The owner's ruling on OQ-1 (2026-11-27, relayed by COORD, his words: "Option ii, recommended path").** Callback
panics take option (ii), on G's five conditions above:
- one slot on the Goroutine, keyed by the depth of the C call;
- the slot drained in GoNativeCall.Exit, so a recorded panic is rethrown at every native-call return;
- the short-circuit taken before ReenterManaged;
- every exception caught, Goexit included;
- rows against go run: an unrecovered panic, a recovered panic, and a second callback after a panic.

GoNativeCall.Exit throwing changes the frozen contract. The Phase 5 seat's first commit lands this ruling as a dated
amendment to DESIGN-native-call-seam, quoting the owner. Code still waits for F1 on master.

