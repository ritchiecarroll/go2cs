# DESIGN — one native-call transition seam: enter-native / exit-native for every P/Invoke converted code makes

> **STATUS: DESIGN, for ruling (lane H, 2026-11-08; branch `claude/h-cgo-foundation`, cut from master `30ad20e17e`).**
> No code lands from this document. It is step 1 of the cgo foundation campaign the owner opened for the `.4` work
> queue (see [`PLAN-cgo-interop.md`](../PLAN-cgo-interop.md), its 2026-11-08 amendment). **The contract is
> co-owned with lane G**, the M:N design lead after `.3`: §2 names every M:N assumption this design makes, so it can
> be reviewed against M:N one line at a time. The M:N input is lane R's
> [`DESIGN-mn-scheduler-sizing.md`](../DESIGN-mn-scheduler-sizing.md) (PROPOSED), whose §3.7 ("native calls") and
> §4.E ("syscall handoff … nothing starts a replacement") this seam answers. Amended by dated blocks, never rewritten.
>
> **AMENDED 2026-11-08 (§12): G's M:N read is folded in (the contract HOLDS; M2 and M7 amended, a §4.1 rule added),
> and COORD ruled Q2 (Go's shape, with a once-per-transition assertion) and Q4 (`GoNativeCall`). Increments S-a..S-d
> are released, red-first, landing after `.3`.**
>
> **AMENDED 2026-11-08 (§13): S-a's cost reading is scored, and §5.2's absolute bounds are withdrawn in favour of the
> ratio to A0 (the lesson). §13.2 is the S-b2 brief for the linux and darwin rows.**
>
> **AMENDED 2026-11-08 (§6.3, dated block): S-c's inert callback arm is two cases, not one. The direct invoke is a
> deliberate divergence from Go, which throws there. The foreign thread is a gap: Go does not throw there.**

## 1. What this is, and why now

A Go program enters native code in exactly one way: through a call the runtime brackets. `syscall.Syscall` calls
`runtime_entersyscall()` and `runtime_exitsyscall()` around its trap; darwin's `syscall_syscall` brackets `libcCall`
the same way; `cgocall` does it for every `C.f()`. That bracket is where Go's scheduler releases the goroutine's P
for the length of the call (`entersyscall`), lets `sysmon` retake it from a long call, and reacquires one on return
(`exitsyscall`). A callback from C re-enters through `cgocallbackg`, which runs `exitsyscall` on the way in and
`reentersyscall` on the way out.

go2cs has no P to release today. Every goroutine owns a thread (reused since batch 31, never multiplexed), so a
blocking native call starves nothing (PLAN-cgo-interop §5.3). The owner has scheduled M:N after `.3`. Under M:N a
blocking native call holds a **carrier**, and the seam that releases it has to exist **before** that scheduler
exists. Otherwise every native crossing, hand-owned, converter-emitted or cgo-generated, has to be found again and
patched one at a time. This document designs that seam so that:

- under today's 1:1 model it is behaviour-neutral, and its cost is predicted ≈ 0 with a measurement plan that can
  falsify that (§5);
- under M:N it is exactly where a goroutine releases its carrier and reacquires one (§6);
- **converted code does not show it** (§8). It lives in a handful of funnel bottoms and, later, in generated cgo
  stubs. A converted call site keeps reading exactly as the Go it came from.

## 2. The M:N assumptions this design makes (for G's review)

Each is a claim about the M:N scheduler that does not exist yet. A "no" from G on any one changes the section named.

| # | Assumption | If it is false |
|---|---|---|
| **M1** | A goroutine inside a native call is pinned to its OS thread for the call's duration. A native frame cannot migrate, under any M:N on the CLR. | Nothing in §6 holds; there is no known CLR mechanism that would make it false. |
| **M2** | After exit-native the goroutine continues **on the same thread**. Its synchronous C# frames are below the call. Under sizing option B a goroutine moves between threads only at a park point (`await`), and a native call is never one: there is no async P/Invoke. | §6.2's exit slow path changes from "park the THREAD until a P frees" to "suspend the goroutine". |
| **M3** | "P" means a scheduling slot. There are `GOMAXPROCS` of them, and a thread inside a native call holds none. | The handoff in §6.1 has nothing to hand off; the seam degrades to accounting. |
| **M4** | Something plays sysmon: it retakes a P from a call that has run too long, and enter-native can also hand off **eagerly** for a call known to block (`entersyscallblock`). The seam carries that hint (§4.2). | The `MayBlock` hint is inert; correctness holds, latency does not. |
| **M5** | Exit-native's slow path may run managed code (a lock, a wait for a P), and that code can clobber errno / `GetLastError`. So errno is captured **inside** the seam, before exit-native returns control to anything. | Nothing: capturing early is correct under every scheduler. It is listed because it is a contract, not an optimisation. |
| **M6** | A `LockOSThread` goroutine runs on a dedicated thread (sizing §3.6). It still releases its P around a native call and reacquires one after, as a locked M does in Go. | A locked goroutine in a blocking call would hold a P; the scheduler loses one slot per such goroutine. |
| **M7** | A native→managed callback arrives on a thread that is **inside** enter-native. It must reacquire a P to run Go code and release it again before returning to native: `cgocallbackg`'s exitsyscall / reentersyscall pair. A callback on a thread go2cs never created (a C library's own thread) must first acquire goroutine identity (`Goroutine.Enter()`, the existing host-attach scope) and then a P (Go's `needm`). | Callbacks run Go code without a P: a second scheduler, which is a correctness defect under any P cap. |
| **M8** | A call bound with `[SuppressGCTransition]` never takes the seam. Such a call is non-blocking, takes no callback, and cannot throw, by the rule PLAN-cgo-interop §5.3 item 2 states. It is Go's `RawSyscall` class. | §7.1. A seam whose slow path can block, on a thread in cooperative GC mode, deadlocks GC suspension process-wide. |

## 3. Census — every native crossing in the product tree today, by class

Measured at master `30ad20e17e`, product code only: `src/core/**`, golib included, everything under `src/tests/`
and every `*_test.cs` excluded. Attribute sites are `[DllImport(` / `[LibraryImport(` occurrences (55 + 49 = **104
in 30 files**). The sizing note's "87 sites in 28 files" was taken at the 1.26 hop tip, so the two counts are of
different trees; neither is wrong. Function-pointer crossings are `delegate* unmanaged` occurrences (declarations
and invocations both, so an upper bound on call sites).

### 3.1 The funnels — where converted code enters native

Converted Go code never declares a P/Invoke. It calls a Go function that bottoms out in one of four funnels, and
that is where the seam goes.

| Funnel | Bottom | Converted callers (sites in emitted `.cs`, an approximate grep) | Bracket today | Errno |
|---|---|---|---|---|
| **F1 windows trampoline** | `syscall/windows/dll_windows.cs` `callNative` (19 fn-pointer arities): every `SyscallN` arity and `NativeStructMarshal.CallDeep` | ~196 `Syscall*`/`SyscallN` call sites in windows flavours | **golib's half only**: `Goroutine.EnterBlocked()`, then try / finally `ExitBlocked()` (:239/:269). The runtime `g` status does NOT move (Go's path is `cgocall` → `entersyscall`). | `GetLastSystemError()` read **inside** the window (:266), the M5 shape already |
| **F2 linux keystone** | `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs` `Syscall6`, `[LibraryImport("libc", EntryPoint = "syscall", SetLastError = true)]` | ~203 `Syscall`/`Syscall6`/`RawSyscall*` sites in linux flavours | Go's bracket is in the converted `syscall.Syscall` (`runtime_entersyscall()` / `runtime_exitsyscall()`), but **both bodies are EMPTY** (`syscall/linux/syscall_linux_impl.cs:72-78`). So no linux syscall reaches the runtime family. **Finding F-1, §9.** `RawSyscall*` has no bracket, as in Go. | SetLastError stub, read in the wrapper |
| **F3 darwin libcCall dispatcher** | `golib/GoLibcCall.cs` `Call`/`CallUnmarshalled` (`delegate* unmanaged[Cdecl]`, 0–9 args, plus the float, variadic and block shapes) | ~63 `libcCall`/`syscall_syscall*` sites in darwin flavours | **Go's own bracket**: the converted runtime calls `entersyscall()` where `sys_darwin.go` does, reaching the hand-owned family (`runtime/syscall_managed_impl.cs`), which moves `g` to `_Gsyscall` and calls `EnterBlocked` | `GoLibcErrnoRule` + `ReadErrno` after the call |
| **F4 cgocaller keystone** | `syscall/linux/cgocaller_linux_impl.cs`: nine `[LibraryImport]` credential calls behind `[UnmanagedCallersOnly]` shims reached through a function pointer | the linux credential wrappers (DESIGN-cgocaller-keystone §2) | none of its own (Go's `cgocall` brackets) | SetLastError, read by the shim |

The runtime's own `entersyscall()` call sites (7 windows, 7 linux, 10 darwin, flavour plus flat files) already
reach the hand-owned family wherever they run.

### 3.2 Direct P/Invokes in hand-owned files (stdlib `*_impl.cs`, the testing host)

| Class | Files (attribute sites) | Blocking? | Routes to |
|---|---|---|---|
| Readiness poller waits | `internal/poll/{linux,darwin}/runtime_netpoll_impl.cs` (epoll / kqueue / kevent: 3) | **yes, by design** | **scheduler-owned, not the seam.** Go calls netpoll from `findrunnable` with the P already handled. Under M:N this IS the netpoller (sizing §3.8, the cooperative scheduler's §7). G owns it. |
| Signal-note thread | `runtime/darwin/sys_darwin_signote_impl.cs` (pipe / read / write: 3), the signal bridges' `sigaction`/`signal`/`syscall` (linux 4 + 1, darwin 3) | the note read blocks, on its **own** non-goroutine thread | the seam is inert there (no goroutine identity, M7's attach does not apply). Short calls are Raw class. |
| Memory | `runtime/{linux,darwin,windows}/mem*_impl.cs`, `stubs2_impl.cs` (mmap / munmap / madvise / VirtualAlloc / VirtualFree / mincore / pipe2 / open / read / write / close: 15) | short (read/write on a runtime-internal fd excepted) | **Raw class** (no bracket): Go's `sysAlloc` family runs on the system stack with no entersyscall. The fd read/write join their Go caller's bracket where Go has one. |
| Process spawn / exec | `syscall/darwin/exec_libc2_impl.cs` (20), `syscall/windows/exec_windows.cs` (5) | `posix_spawn`, `execve` and `CreateProcessW` can block | **MayBlock** through the seam |
| File / process enumeration | `syscall/windows/zsyscall_windows_impl.cs` (5: FindFirstFileW, FindNextFileW, Process32*, GetTimeZoneInformation) | file I/O can block | through the seam, as F1 would have bracketed them |
| Library loading | `dll_windows.cs` LoadLibraryExW / GetProcAddress (2) | loader lock | through the seam (MayBlock): Go's `syscall.LoadLibrary` goes through `cgocall` |
| Test host | `testing/{TestHost,PackageAncestry,HostDescriptorLimit}.cs` (7) | n/a | out of scope: the host runs before and after goroutines, never as one |

### 3.3 golib's own P/Invokes

`builtin.{Darwin,Linux}StdDescriptors.cs` (5), `builtin.WindowsLongPaths.cs` (1), `runtime/GoTimer.cs`
(nanosleep, 1), `runtime/GoHighResolutionTimer.cs` (2), `runtime/GoCriticalTimer.cs` (3), `runtime/ThreadCpuTime.cs`
(10). All are short, except `nanosleep` and the timer waits. Those are **park sites** under M:N (sizing §3.1: a
sleep must return to the scheduler, not hold a thread in native). So golib's own calls are Raw class, and its
blocking waits belong to the park inventory, not to this seam.

### 3.4 Re-entry: native → managed while inside a native call

| Path | Mechanism | Today |
|---|---|---|
| `syscall.NewCallback` (windows) | `syscall_windows_callback_impl.cs`: a per-arity non-generic delegate through `Marshal.GetFunctionPointerForDelegate` (DESIGN-managed-newcallback) | the callback runs on the calling thread **while `m_blockedDepth > 0`**: a counted goroutine running Go code in a callback reads as blocked. **Finding F-3, §9.** |
| Console control handler | `runtime/windows/signal_windows_impl.cs` SetConsoleCtrlHandler (fn pointer) | arrives on an OS-created thread; no goroutine identity |
| cgocaller shims | `[UnmanagedCallersOnly]` shims in F4, reached through a function pointer from managed code | managed → "native" → managed on one thread; no Go code runs in between |
| Future `//export` | PLAN-cgo-interop Phase 5 | not built |

### 3.5 Future producer: cgo-generated stubs

PLAN-cgo-interop Phase 2 generates one `LibraryImport` extern plus one wrapper per `C.f` declaration, written by
the converter into real `.cs` files (never by go2cs-gen: §6 Phase 2's architectural constraint). **Every
generated wrapper calls the seam** (MayBlock by default), because each one is the `cgocall` of its call site. That
is the fifth producer, and the reason the seam has to be a golib API rather than a pattern copied into four files.

## 4. The contract

### 4.1 One golib type, four operations

A new golib static, `GoNativeCall` (namespace `go.golib`, beside `Goroutine`), is the ONLY place a transition is
implemented:

| Operation | Go counterpart | Called by |
|---|---|---|
| `Enter(NativeCallHint hint)` | `entersyscall` / `entersyscallblock` | every funnel bottom (§3.1) and every generated cgo wrapper, immediately before the native call |
| `Exit()` | `exitsyscall` | the same, in a `finally`, after errno has been read |
| `ReenterManaged()` | `cgocallbackg`'s `exitsyscall` | every native→managed entry point (§3.4) before any Go code runs |
| `ReturnToNative()` | `cgocallbackg`'s `reentersyscall` | the same, in a `finally`, before control returns to native |

The four are **depth-counted per goroutine** (the existing `m_blockedDepth`, which they subsume). So a callback
nests correctly and a funnel reached from inside a callback brackets again. A thread with no goroutine identity
makes all four inert, as `EnterBlocked` is today.

**The layering is Go's, not a new one.** golib sits below `runtime`, so golib cannot move the runtime's `g` status.
The runtime's hand-owned family (`runtime/syscall_managed_impl.cs`) keeps that job (`_Grunning` ↔ `_Gsyscall`, for
the goroutine profile and `readgstatus`) and calls `GoNativeCall.Enter/Exit` instead of `EnterBlocked/ExitBlocked`.
A funnel whose Go original reaches the runtime family (F2, F3, F4) reaches it again. A crossing below Go level
(golib's own, hand-owned direct P/Invokes) calls `GoNativeCall` directly and moves no `g` status, exactly as a Go
`asmcgocall` on the system stack moves none.

### 4.2 The hint

`NativeCallHint.MayBlock` (the default) is Go's `entersyscall`: hand the P off lazily, and let the sysmon role retake
it from a long call (M4). `NativeCallHint.Blocks` is `entersyscallblock`: hand it off at once. There is no
`NonBlocking` hint. A call that cannot block and cannot call back is Raw class and **does not call the seam at all**
(§7.1). That keeps the fast path out of every short call rather than making each short call pay for a branch that
says "do nothing".

### 4.3 The errno rule (M5)

The value the call left in errno / `GetLastError` is read **between the native return and `Exit()`**, by the code
that made the call: the `SetLastError = true` stub, `GetLastSystemError()` as F1 already does at :266, or
`GoLibcCall.ReadErrno`. Nothing that can run managed code sits between the call and that read. `Exit()` is
therefore free to block, take a lock or run a P/Invoke of its own without corrupting Go's `err`. This is today's F1
shape, made the rule for all five producers.

## 5. Under today's 1:1 model

### 5.1 What each operation does

- `Enter` / `ReturnToNative`: what `EnterBlocked` does today. A thread-static read, then a branch; the counted path
  (only while `main` is returning and counting this goroutine in `runningPanicDefers`) increments the depth and
  sequence that `MainExit` samples.
- `Exit` / `ReenterManaged`: what `ExitBlocked` does today. The same read; decrement when positive.
- No P exists, so nothing is released and nothing is reacquired. **Scheduling is unchanged by construction.** The
  only observable difference from master is the repair of findings F-1 and F-3 (§9), each owed a red-first arm of
  its own.

### 5.2 Cost: the prediction, and the measurement that can falsify it

**Prediction.** The uncounted path is one `[ThreadStatic]` load, one null/field test and a `try/finally` per call.
Against a real kernel transition it is inside the single-process spread on every binding host. Stated as a
falsifiable number: **the floor-over-processes delta (seam arm minus bare arm) is under 5% of the bare arm's floor
on every host and tiering configuration, and under 3 ns absolute.**

**Plan** (doctrine: `measurement-discipline` and `validation-bank` "When a reading expires"; golib-gen's alloc
instrument section for the protocol):

| Arm | What runs, 10⁷ iterations per process |
|---|---|
| A0 bare | a `delegate* unmanaged` call to a **real kernel transition** on the binding host: windows `SwitchToThread`, linux `syscall(SYS_getppid)` through libc, darwin `getppid`. A user-mode-only call (`GetCurrentProcessId`, a TEB read) is NOT an anchor: the cross-host anchor-artifact trap. |
| A1 seam, uncounted | A0 inside `Enter(MayBlock)` / try / errno read / finally `Exit()` |
| A2 seam, counted | A1 with the goroutine marked counted (the path `MainExit` arms) |
| A3 re-entry | A1 with a `ReenterManaged`/`ReturnToNative` pair inside, the callback depth path |
| C+ positive control | A1 with a deliberate 20 ns spin inside `Enter`: the instrument must read it, or it can read nothing |

Protocol: Release; **two tiering configurations** (default, and `TieredCompilation=0`), because a micro-bench can
invert on the tiering axis via OSR (measured 2026-09-08); at least five processes per arm; report the **floor over
processes** per arm as an absolute in ns, with A0's floor named as the base on that host. Never pool a percentage
across hosts with different anchors. The binding hosts are those M:N will be validated on: windows, linux and
darwin, x64 and arm64 where the fleet has them. The bench lives in GolibTests as an `[Ignore]`-by-default probe, run
by hand, like the existing clock-candidates probe.

## 6. Under M:N (contract for G; the scheduler is G's to build)

### 6.1 `Enter(hint)`: the goroutine releases its carrier

The goroutine's P moves to "in syscall" (M3). With `Blocks`, or if runnable work is queued and no idle worker
holds a P, the scheduler hands the P to another worker at once, waking or starting one. With `MayBlock` the P stays
nominally attached, and the sysmon role retakes it once the call outlives its threshold (M4). The thread itself goes
on into native code, holding no P.

### 6.2 `Exit()`: reacquire one, on this thread

1. **Fast path:** take back the P this goroutine released, if no one took it (one CAS, Go's `exitsyscallfast`).
2. Otherwise take any idle P.
3. Otherwise **park this thread** until a P frees (Go's `exitsyscall0` → `stopm`), then continue. By M2 the
   goroutine cannot continue anywhere else. If M2 is false under G's design, this step becomes "suspend the
   goroutine", and only this step changes.

### 6.3 `ReenterManaged()` / `ReturnToNative()`: callbacks (M7)

On a thread inside `Enter`, `ReenterManaged` is §6.2 (a P is needed to run Go code), and `ReturnToNative` is §6.1
with the saved hint. On a thread with no goroutine identity, `ReenterManaged` first opens `Goroutine.Enter()` (the
existing attach scope, PLAN-cgo-interop Phase 5) and then acquires a P. `ReturnToNative` releases both. A panic
crossing `ReturnToNative` is caught by the entry thunk before it (Phase 5's panic boundary). The seam never sees an
exception escape into native.

**Amendment 2026-11-08 — S-c's inert arm: one deliberate divergence, and one gap** *(a dated block, not a rewrite;
COORD's record item on S-c, `claude/h-native-seam-sc` `7e13ee7c8b`).*

S-c wires this pair into the `NewCallback` shims through the runtime's `syscallCallbackEnter` /
`syscallCallbackExit`. Both are **inert** wherever the goroutine's transition depth is not odd: no native call is
open to come back from. The callback then runs as plain Go code with nothing moved. That one predicate covers two
cases, and Go treats them differently. S-c's announce said "Go throws there" of both. That holds for the first
only.

1. **Direct invoke: a goroutine is present, but no native call is open** (a callback pointer called from managed
   code, as the control arm of `WindowsCallbackReentryTests` does). Go's `cgocallbackg` assumes a syscall is open:
   it calls `exitsyscall()` "coming out of cgo call" (`runtime/cgocall.go:350`, go1.26.10). `exitsyscall` throws
   `"exitsyscall: syscall frame is no longer valid"` when the caller's SP is above the saved syscall SP
   (`runtime/proc.go:4887-4889`), which it is when nothing entered. **go2cs does not throw. This is a deliberate
   divergence, not an oversight.** Two reasons, both H's reading:
   - Go's `throw` is a fatal error, not a recoverable panic. The shim is a marshalled delegate
     (`Marshal.GetFunctionPointerForDelegate`), entered from native code. An exception thrown there must unwind
     through native frames that never expected one. .NET supports that unwind only on Windows. Matching Go here
     buys, at best, a crash with a worse report than Go's.
   - Bracket coverage is partial until S-b2 and F-1 land. Any native caller not yet bracketed reaches the shim
     exactly this way, so a Go-faithful throw would turn every callback under it into a process abort. Inert
     keeps those callbacks working, at Go's status semantics for the unbracketed span: `_Grunning`
     throughout, which is what the goroutine already reads there.
2. **Foreign thread: no goroutine identity** (a callback on a thread a C library created). **Go does not throw.**
   `cgocallback` finds no `g` and takes an extra M through `needAndBindM`, then runs the callback as a goroutine on
   it (`runtime/asm_amd64.s:1057-1097`, go1.26.10: the `loadg` path to the `needm` label). go2cs's inert arm runs
   the callback as plain managed code with no goroutine identity. **That is a gap, not a divergence.** This
   section's second sentence already names the fix: open `Goroutine.Enter()` (the existing attach scope) for the
   callback's duration. S-c does not wire it. It is routed as a ruling question beside S-d, not folded into S-c.

### 6.4 What M:N must not change about the contract

- The four operations stay the only transition. A funnel never touches a P directly.
- `Exit()` still runs after errno is read (§4.3).
- Raw class still never enters (§7.1).

## 7. The three constraints PLAN-cgo-interop §5.3 carried, re-derived for the seam

### 7.1 `[SuppressGCTransition]` (§5.3 item 2)

A suppressed call keeps its thread in cooperative GC mode. If such a call took `Exit()`'s slow path (§6.2 step 3:
park the thread until a P frees), the GC could not suspend that thread, and the process deadlocks. So **Raw class
never enters the seam**, and the seam must be uncallable with a suppressed binding. That is enforced by rule, not by
type: a repoguard guard rejects any `[SuppressGCTransition]` method whose body or caller reaches `GoNativeCall`.
Census at master: **zero** `[SuppressGCTransition]` sites in product code. The guard therefore costs nothing today
and stops the first one from arriving wrong. Raw class equals Go's `RawSyscall` family: no bracket, short, no
callback.

### 7.2 Callbacks re-entering managed code (Phase 5)

§6.3. Today's one live instance is `NewCallback` (finding F-3). cgo's `#cgo nocallback fn` (`cmd/cgo/doc.go:454-461`,
go1.26.10) states that a C function never calls back into Go. A generated wrapper for such a function may add a
debug-mode assertion that no `ReenterManaged` occurs inside its `Enter`. It **does not** make the call Raw class:
`nocallback` says nothing about blocking. `#cgo noescape` (:444-451) is a pointer-retention hint for Phase 3's pin
scope, and does not apply to the seam.

### 7.3 errno and thread affinity (§5.3 item 3)

errno: §4.3. Thread affinity: by M1/M2 a goroutine never changes threads **during** a native call, nor between that
call's return and its errno read. Between two **separate** native calls a non-locked goroutine may change threads
under M:N, as in Go. A thread-affine C library (OpenSSL's error queue, `uselocale`, GTK) therefore needs
`runtime.LockOSThread`, **exactly as it does in Go**: cgo's semantics are preserved, not weakened. Under today's 1:1
model affinity holds trivially. That difference is a property of M:N, and G's design has to state it to users when
M:N ships.

## 8. Visible emission

The owner's tenet (the converted code reads like the Go) is weighed, not absolute, and behaviour comes first. **This
seam needs no visible emission at all**:

- converted call sites call `syscall.Syscall(...)`, `syscall.SyscallN(...)` and the darwin `syscall_syscall*`
  exactly as the Go does, and the seam is inside the funnel bottoms (hand-owned or golib);
- the only bracket text in converted output is Go's own `runtime_entersyscall()` line in `syscall_linux.go`, already
  emitted today because the Go source carries it;
- generated cgo stubs (§3.5) do carry the seam, but they are generated bindings, build outputs under
  PLAN-cgo-interop §5.2, not converted Go, and the `C.f(...)` call site that reaches them stays as written.

The converter emits **no** new construct for this design. The funnel KeepAlive (`syscallKeepAliveAnalysis.go`) is
unaffected: it holds a box across the call at the CALL SITE, and the seam brackets the native transition at the
BOTTOM. The two compose.

## 9. Findings the census turned up (each owed its own red-first arm before any fix)

- **F-1 (linux): Go's syscall bracket never reaches the runtime family.** `syscall/linux/syscall_linux_impl.cs:72-78`
  bodies `runtime_entersyscall` / `runtime_exitsyscall` as no-ops ("the CLR's thread model already provides what this
  exists to provide"). Go linknames both to `runtime.entersyscall` / `exitsyscall`. The consequences: a linux
  goroutine blocked in `read(2)` through `syscall.Syscall` never reads `_Gsyscall` (the goroutine profile, and
  `readgstatus`), and `MainExit`'s returning-main sample does not count it as blocked. Darwin does both; windows
  does the second only (F-2).
  Under M:N this would be the funnel that never releases its P. The fix is two forwarding bodies; the red is a linux
  arm (G-WSL or P1), since H has no linux host.
- **F-2 (windows): F1 moves golib's half but not the `g` status.** Go's path goes through `cgocall` → `entersyscall`,
  so a Go goroutine profile taken during a windows syscall shows "syscall", and ours shows "running". The fix routes
  `callNative` through the runtime family's public seam, or registers the status move with `GoNativeCall` (§10 Q2).
- **F-3 (windows): a `NewCallback` callback runs with the blocked depth still raised.** A counted goroutine running
  Go code inside a callback (an `EnumWindows` enumerator, say) reads as blocked to a returning `main`. The fix is
  §6.3's pair in the callback thunk. It is measurable today on H, the windows lane.

None of the three changes scheduling. Each changes an observation Go makes. Each is small, and each is a seat of its
own.

## 10. Proposed increments (each a seat; none before `.3` publishes)

1. **S-a, the type.** `GoNativeCall` in golib, with `EnterBlocked`/`ExitBlocked` re-expressed through it
   (byte-identical behaviour). The A0–A3 + C+ probe in GolibTests. The full GolibTests suite, unfiltered (golib
   change). The prediction in §5.2 is scored there.
2. **S-b, route the funnels.** F1/F3/F4 bottoms and the hand-owned MayBlock rows of §3.2 call `GoNativeCall`;
   findings F-1 and F-2 each red-first. Corpus footprint: hand-owned files only, no converter change, so CNR is
   byte-identical by construction (to be proven, not assumed).
3. **S-c, re-entry.** Finding F-3, red-first, on windows.
4. **S-d, the Raw-class guard** (§7.1) in repoguard, made to fail once on a planted site.
5. Then PLAN-cgo-interop Phase 2's generated wrappers call `GoNativeCall` from their first emission.

## 11. Questions for G (and rulings for COORD)

- **Q1 (G).** M2: under option B, is a native call ever reached from an async-lowered frame in a way that would let
  the goroutine resume elsewhere after `Exit()`? This design says no.
- **Q2 (G, COORD).** F-2's fix: should windows' `callNative` call the runtime family (a cross-assembly call into
  `runtime`, which `syscall` already references), or should the runtime register its `g`-status move with
  `GoNativeCall` once at init (an indirect call per transition)? The first is Go's shape; the second keeps golib the
  only funnel.
- **Q3 (G).** The sysmon threshold for `MayBlock` (Go: 20 µs, backing off to 10 ms). Is it G's scheduler constant,
  or does the seam carry a per-call override for generated cgo wrappers?
- **Q4 (COORD).** Naming. `GoNativeCall` follows `GoLibcCall` (golib's darwin dispatcher). The alternative is
  methods on `Goroutine`, beside today's `EnterBlocked`.

## 12. Amendment 2026-11-08 — G's M:N read, and COORD's rulings on Q2 and Q4

*A dated block, not a rewrite: §§1–11 stand as written, and where this block differs from them, this block is the
reading. Lane G, the co-owner, reviewed the contract against M:N and ruled nothing; **the contract holds.** COORD
ruled the two questions left to it and released the increments.*

### 12.1 G's read

**ACCEPTED: M1, M3, M4, M5, M6, M8.**

**M2, amended.** "The goroutine continues on the same thread after `Exit()`" is a **CLR / option-B constraint, not
Go's semantics**. Go's `exitsyscallNoP` (`proc.go:5067`; `globrunqput` `:5089`; `stopm` `:5114`, per G) resumes the
goroutine on whatever M schedules it next. §6.2 step 3 therefore becomes a **scheduler-supplied slow path**, in one
of two forms:

- **park this thread** until a P frees (option B, the async lowering; §6.2 as written);
- **suspend the goroutine and return the carrier** (a stackful scheduler, the owner's green-threads lead). This form
  is valid only with **no native frame below** the goroutine: see M7, as amended below.

This changes the wording, not the structure. The seam calls the slow path, and the scheduler supplies it.

**M7, amended.** Go **locks the goroutine to its thread for the whole callback**: `cgocallbackg` calls
`lockOSThread()` before `exitsyscall` (`cgocall.go:326-330`, per G). So between `ReenterManaged()` and
`ReturnToNative()` the goroutine is **implicitly locked**. A park inside a callback blocks that OS thread and never
unmounts the goroutine, because a native frame is below it. §6.3 reads this way, for every scheduler.

**§4.1, added: where the seam's state lives.** The seam's per-goroutine state (the transition depth, the saved hint,
the released P) lives **on the goroutine object**, reached through the current-goroutine lookup, and **never in a
thread-static**. Under M:N a goroutine's state must follow the goroutine, not the carrier. Today's `m_blockedDepth`
(`Goroutine.cs:206`) already does this.

**Q1, answered.** No under option B. Under a stackful scheduler the goroutine MAY resume elsewhere at `Exit()`'s slow
path, and that is safe because §4.3 reads errno **before** `Exit()`. The rule stays exactly as written.

**Q3, answered: a scheduler constant.** There is no per-call numeric override (Go's `retake`, `proc.go:6692`; sysmon's
backoff, `:6498`, per G). The hint (`Blocks` / `MayBlock`) is the only knob a call site has.

**For M:N, stated by G.** The seam does **not** lift the thread cap for goroutines blocked in native calls. Each one
holds an OS thread (M1), as Go's Ms do, up to `sched.maxmcount` (`proc.go:863`, per G). On macOS the cap still bounds
how many native calls can block **concurrently**. G's M:N design states it.

### 12.2 COORD's rulings

- **Q2 RULED: Go's shape.** The windows `callNative` calls the runtime's entersyscall family, which calls
  `GoNativeCall`. That gives **one path per transition**, traceable to Go, with the `g`-status move **before** the
  P release. The init-registered hook is dropped. **`GoNativeCall` must run exactly once per transition**, and an
  assertion enforces it (§12.3).
- **Q4 RULED:** the name `GoNativeCall` is approved.
- **Increments released:** S-a through S-d, each a red-first branch, **nothing landing before the `.3` publish**.
  F-1's linux red arm goes to G-WSL after `.3`, as G offered. F-2 and F-3 are H's, in that order, after S-a.

### 12.3 Consequences for §10, as H reads them for the cut

- **S-a: the type composes over golib's existing accounting; it does not replace it.** §10 S-a said `EnterBlocked` /
  `ExitBlocked` would be "re-expressed through" `GoNativeCall`. That is wrong. Those two also serve waits that are
  **not native transitions**: the execution tracer's reader and stop (`ExecutionTracer.cs:282-307`), golib's waits
  that cannot park. So `GoNativeCall` calls them, and they stay. S-a changes no caller, so it is behaviour-neutral by
  construction.
- **"Exactly once per transition" is a parity rule on one per-goroutine counter.** The counter is even while the
  goroutine runs managed code and odd while it is in native:
  - `Enter()` requires even, and `ReenterManaged()` requires odd; each adds one;
  - `ReturnToNative()` requires even and above zero, and `Exit()` requires odd; each subtracts one.

  A second `Enter()` for one transition (the windows trampoline AND the runtime family both entering, say) lands on
  odd and fails by name. The counter lives on the goroutine (§12.1's §4.1 addition), and a thread with no goroutine
  identity stays inert.
- **S-b carries F-2 by construction**, because the Q2 ruling is F-2's fix: `callNative` → runtime family → `GoNativeCall`.

## 13. Amendment 2026-11-08 — S-a's cost reading, the lesson it taught, and the S-b2 brief

*A dated block, not a rewrite.*

### 13.1 §5.2's prediction, scored (S-a, `claude/h-native-seam-sa` `af1dd6c059`)

The first reading was taken on H (windows x64), Release, with the opt-in `GoNativeCallCostProbe`: five processes ×
1,000,000 calls × 5 rounds, reported as the floor over processes in ns per call. The windows anchor is
`GetProcessTimes`, not the `SwitchToThread` §5.2 named, because a yield can switch threads and so adds
load-dependent noise to the base arm. The deviation was stated and accepted.

| Configuration | A0 | A1 | A2 | A3 | C+ | SPIN |
|---|---|---|---|---|---|---|
| default tiering | 465.57 | 471.51 | 472.32 | 475.99 | 519.76 | 38.23 |
| `TieredCompilation=0` | 468.85 | 466.89 | 471.03 | 479.26 | 510.38 | 37.71 |

- **The relative bound held:** every seam arm is within +2.2% of A0, on both configurations.
- **The absolute bounds missed:** A1 − A0 = +5.94 ns at default against "under 3 ns"; A2 − A0 = +6.75 against
  "under 5"; A3 − A1 = +4.48 / +12.37 against "under 3".
- **The positive control read:** C+ − A1 = 48.25 / 43.49 against SPIN 38.23 / 37.71.

**The lesson (COORD's ruling).** A0's own per-process spread on that host was 10.71 / 5.80 ns, the same size as every
delta. A bound in absolute nanoseconds below the instrument's resolution can be neither met nor missed meaningfully.
**Bound a transition's cost by its ratio to a real kernel transition on the binding host, and state A0's spread
beside the figure.** §5.2's absolute clauses are withdrawn. Its "under 5% of A0" clause is the criterion of record.

S-b folds the seam's two current-goroutine reads per operation into one (`Goroutine.EnterBlockedOn` /
`ExitBlockedOn`) and re-runs the probe at `8565d61e5a`, same protocol:

| Configuration | A0 | A1 | A2 | A3 | C+ | SPIN | A0's spread |
|---|---|---|---|---|---|---|---|
| default tiering | 459.71 | 469.93 | 471.60 | 479.10 | 509.87 | 38.25 | 37.24 |
| `TieredCompilation=0` | 468.62 | 467.96 | 470.77 | 478.45 | 513.40 | 38.24 | 22.10 |

The prediction written before the run was "A1 − A0 at default not above S-a's +5.94 ns". It **MISSED as
worded** (+10.22). This run's A0 floor came in 5.9 ns lower than S-a's, and A0's own spread was 37 ns, so the
delta moved with the base, not with the seam. A1's absolute floor fell 1.6 ns (471.51 → 469.93). The fold's
effect is **below this instrument's resolution on H**, as §13.1's lesson predicts for any absolute bound. The
criterion of record held: every seam arm is within **4.2% of A0** (A3 at default), and the positive control read
(C+ − A1 = 39.94 / 45.44 against SPIN 38.25 / 38.24). One goroutine lookup per transition stays, because it is the
obvious shape, not because a number showed it.

### 13.2 S-b2: the brief, for a linux or darwin lane picking it up cold

**Routing (COORD).** Linux goes to G-WSL, beside F-1. Darwin goes through C1's darwin CI. Both run after `.3`. H
cannot build or run these flavours.

**What S-b did on windows** (`claude/h-native-seam-sb`), which is the pattern to copy:

- The runtime's hand-owned entersyscall family (`runtime/syscall_managed_impl.cs`) moves the `g` status, then calls
  `GoNativeCall.Enter(hint)`. Its exit refuses a non-`_Gsyscall` status first, then calls `GoNativeCall.Exit()`,
  then moves the status back.
- The runtime has public `syscallEntersyscall()` / `syscallEntersyscallblock()` / `syscallExitsyscall()`
  forwarders for the `syscall` package.
- Every hand-owned site whose Go original reaches `cgocall` or `entersyscall` is bracketed with them, with the last
  error read **inside** the window:

  ```text
  runtime_package.syscallEntersyscall();
  try { result = <native call>; lastError = Marshal.GetLastSystemError(); }
  finally { runtime_package.syscallExitsyscall(); }
  ```

**The S-b2 sites:**

| Flavour | Site | Go original's bracket | Note |
|---|---|---|---|
| darwin | `syscall/darwin/exec_libc2_impl.cs`, the `posix_spawn*` and `execve` path | `forkExec` → the runtime's `syscall_rawSyscall` family for the child; the parent's `posix_spawn` is a go2cs seam, not a Go call | **decide per call** whether Go's shape is Raw (no bracket) or `syscall_syscall` (bracketed); read `GOROOT/src/syscall/exec_libc2.go` first |
| linux | `syscall/linux/cgocaller_linux_impl.cs`, the nine credential `[LibraryImport]`s behind `[UnmanagedCallersOnly]` shims | Go's `cgocaller` → `runtime.cgocall` → `entersyscall` | the shims run managed code between the function-pointer call and the import: bracket the IMPORT, inside the shim, and read errno there |
| linux | F-1: `syscall/linux/syscall_linux_impl.cs:72-78`, the empty `runtime_entersyscall` / `runtime_exitsyscall` bodies | Go linknames them to `runtime.entersyscall` / `exitsyscall` | forward them to the runtime forwarders; this one fix brackets every linux `syscall.Syscall` |

**Amendment 2026-11-09 (the cgocaller row, as built and ruled):** S-b2 (`claude/g-sb2-linux-cgocaller` `0eceb47998`)
brackets the shim CALL inside `cgocaller`, not the import inside the shim. That is where Go's `cgocall` puts its
`entersyscall` (`runtime/cgocall.go:110-114` and `:167`, go1.26.10). `Setgroups` now goes through `cgocaller` as Go's
does, so one path covers all nine setters. The arity is checked before `Enter`, so a refusal never leaves a
transition open, and the shims still read errno inside the window. COORD ruled the placement right.

**The red for each.** Where a site can call a function pointer (darwin's `libcCall` family, the cgocaller shims),
use S-b's shape: point the call at an `[UnmanagedCallersOnly]` probe that reads
`runtime_package.GoCurrentGStatus()` and `GoNativeCall.TransitionDepth` from INSIDE the native window, and expect
`_Gsyscall` and 1. F-1 cannot: the linux keystone takes a syscall NUMBER, not an address, and a blocked goroutine's
status cannot be read from another thread through any seam that exists today. G-WSL designs that red. The
candidates are a goroutine dump's header for a goroutine blocked in `read(2)` on a pipe (if the converted traceback
reads the status) and a new test seam reading a goroutine's status by id.

**Watch for double entry.** `GoNativeCall` refuses a second `Enter` by name. If a linux or darwin path already
reaches the runtime family through converted Go code (darwin's `libcCall` callers do, through `entersyscall()` in
the converted runtime), bracketing its bottom again **fails loudly**. That loud failure is the intended assertion,
not a test bug. Census each funnel's callers first.

**Amendment 2026-11-09 (the darwin rows, as built; H, `claude/h-sb2-darwin`, approved by COORD):**

*The census, read at master `fb885891b3`, judged against Go's own `syscall` vs `rawSyscall` shape.*

- **Already bracketed, so no change: every `libcCall` caller.** The converted darwin runtime's `syscall_syscalln`
  calls `entersyscall()`, which is S-b's hand-owned family (`runtime/syscall_managed_impl.cs`), so converted
  `syscall` and `os` callers such as `closedir` are inside the window today. That census is the double-entry check
  above.
- **Bracketed now, 3 hand-owned sites that call libc directly where Go makes the call through `syscall()`:**
  - `syscall/darwin/exec_libc2_impl.cs` `posix_spawn`: the parent blocks until the child has exec'd, the wait Go's
    parent spends in `readlen` on the exec pipe, a bracketed read.
  - The same file's `kill` on the Foreground-failure path. Go's `kill` is `syscall()` (`zsyscall_darwin_amd64.go:403`,
    go1.26.10).
  - `os/darwin/dir_darwin_impl.cs` `readdir_r`. Go's `syscall.readdir_r` is `syscall()` (`zsyscall_darwin_amd64.go:1260`).
- **Not bracketed, by Go's shape:**
  - `execve` in `Exec` and `pipe`: Go makes both as `rawSyscall` (`zsyscall_darwin_amd64.go:1789` and `:370`).
  - The other 16 of exec's 20 imports: the `posix_spawn_*` setup calls, `sigemptyset`/`sigaddset`, `realpath`/`free`,
    and the Foreground `ioctl` and `pthread_sigmask`. They are libc bookkeeping with no Go counterpart, or calls Go
    makes as `rawSyscall` in its fork child (`exec_libc2.go:86-185`).
  - The runtime and `internal/poll` darwin hand-owns (15 imports: `mmap`, `sigaction`, `sigprocmask`, the signal note,
    the netpoller's kqueue). These are runtime-internal `libcCall`s, which Go makes without `entersyscall`.

**The shape.** Each bracketed call goes through one helper that takes the native function: `posixSpawnCall`,
`killCall` and `readdirRCall`. The helper makes the call between `syscallEntersyscall()` and `syscallExitsyscall()`
and returns the result from inside the window. Production passes the libc function. The public seams
`GoPosixSpawnCall` and `GoKillCall` (`syscall`) and `GoReaddirRCall` (`os`) pass a test's function.

`posix_spawn` and `kill` moved from `DllImport` to `unmanaged[Cdecl]` function pointers bound by
`GoCgoDynamicImports.Resolve` (`NativeLibrary.TryLoad`/`TryGetExport`). `readdir_r` keeps its existing delegate
binding, because `os` builds with `AllowUnsafeBlocks=false`.

**Native AOT: UNMEASURED (COORD's ruling).** The argument: each rebound call is a raw `unmanaged[Cdecl]` pointer over
blittable arguments (`int`, `int*`, `IntPtr`), so there is no marshalling stub to generate, and the address is
resolved at run time through `NativeLibrary`, which Native AOT supports. No run shows it. `aot-smoke` takes 4 to 6
hours on its four RIDs, and its consumer (`PackageSymbols`) never spawns a process or reads a directory, so a green
run would not show these calls resolving. The retirement read is the first darwin AOT consumer that spawns a process.

**The reds, read on darwin CI.** `GolibTests`' `DarwinNativeTransitionTests` (a darwin-only file) points each helper
at an `[UnmanagedCallersOnly]` probe that reads `GoCurrentGStatus()` and `GoNativeCall.TransitionDepth` from inside the
call. This is S-b's shape, and it gives the `kill` bracket a red as well.

At the red commit `8b9559c91d` (run 42651065436), all three failed on both osx-x64 and osx-arm64, each at the
`_Gsyscall` assertion (expected 3, actual 2), after its premise held.

A fourth test drives the real `kill` path end to end: a Foreground spawn whose `Ctty` is `/dev/null` makes the parent's
`TIOCSPGRP` fail with `ENOTTY`, and the seam must kill and reap the child and return `ENOTTY`.

Its first form used a pipe for `Ctty`, and on both RIDs it read a pid where `ENOTTY` was predicted. A BSD pipe
**accepts** `TIOCSPGRP`, which sets its SIGIO owner, so the transfer succeeded and the child ran. That was a defect in
the test's premise, not in the seam, and the test now uses `/dev/null`.
