# The managed netpoller — hand-owning the ten `runtime_poll*` contracts

> **STATUS: RATIFIED (§9, coordinator 2026-08-13) and IMPLEMENTED through S2.** S1 (the ten
> `runtime_poll*` contracts, `internal/poll/windows/runtime_netpoll_impl.cs`) landed 2026-08-13; S2 —
> the overlapped SUBMIT seam and its golib rendezvous — landed 2026-08-15 (see the LANDED notes in
> §4.3, the implementation findings in §4.6, and the S2 gate results in §7). S3 (UDP wrappers, the
> consumer re-measures) is open. The prose below is kept in its original PROPOSED voice, with each
> ruling, amendment, correction and landing recorded inline as a dated blockquote where it applies —
> that history is the point of the document, so nothing is rewritten in place. Commissioned by the board's
> sockaddr RESOLVED note ([`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md),
> "RESOLVED 2026-08-11 (lane L10)": *"that is a design arc with a deadline/unblock story to settle,
> not a wrapper repair — it wants its own DESIGN doc and a coordinator ruling before anyone
> starts"*) and the matching L10 correction in [`LANES.md`](LANES.md) §L10. Companions:
> `src/core/internal/poll/fd_poll_runtime.cs` (the ten stubs and all their callers),
> `src/core/internal/poll/windows/fd_windows.cs` (`execIO`, the contract's driver),
> `src/core/runtime/netpoll.cs` + `src/core/runtime/windows/netpoll_windows.cs` (the converted
> runtime counterparts this design deliberately leaves dead), `src/core/golib/ж.cs` (the address
> model the submit seam prices against), and the precedent files cited inline. Written against the
> corpus at `0133b6aa7` (2026-08-12), Go 1.23.1.

---

## 1. The wall, measured — what stops `net.Listen` today

With L10's sockaddr fixes merged, `syscall.Bind` succeeds and `net.Listen` on Windows walks on to
this, the exact stack the board banked:

```
System.NotImplementedException: runtime_pollServerInit: external (assembly or cgo) function is not implemented
  at internal/poll.runtime_pollServerInit  (PartialStubGenerator stub)
  at internal\poll.pollDesc.init -> internal\poll.FD.Init  ... fd_poll_runtime.cs:48
  at net.netFD.init -> net.listenStream                    ... sock_posix.cs:216
  at net.Listen                                            ... dial.cs:933
```

`internal/poll` declares **ten** bodyless `//go:linkname` entry points into the runtime's network
poller (`fd_poll_runtime.cs:18–36`). The converter emits each as a bodyless `partial` method; with
no body provided, the `PartialStubGenerator` fills them with throwing stubs, and the first pollable
`FD.Init` — every socket `net` creates (`net/windows/fd_windows.cs:59` passes `pollable: true`;
`os` passes `false` for every file/pipe/console, `os/windows/file_windows.cs:83`) — dies in
`serverInit.Do(runtime_pollServerInit)`.

The counterparts EXIST in the converted runtime — `runtime/netpoll.cs:217` carries
`poll_runtime_pollServerInit` with its `//go:linkname` comment intact, and all nine others are
beside it — but nothing wires a linkname across assemblies, and §3 shows that wiring them would
not be sufficient: the bodies bottom out in the runtime scheduler, which does not exist under the
CLR. The honest remedy is the managed-API-boundary pattern this repo has used four times already
(§3.3): **hand-own the ten CONTRACTS in `internal/poll` against .NET's own completion machinery,
and leave the runtime's poller exactly as converted — dead.**

This is a **Windows-first** design. The corpus's compile target is `$(GoTargetOS)=windows`; the
Linux `internal/poll` is a *readiness*-model consumer (`fd_unix.go` retries the syscall after
`pd.wait`) rather than a *completion*-model one, and gets its own design when the Linux corpus
compiles (§8).

## 2. The contract inventory — ten functions, every caller

All ten stubs live in `fd_poll_runtime.cs` (build tag `unix || windows || wasip1` — the file is
FLAT in the L3 layout, shared by every GOOS). The callers below are the complete set inside
`internal/poll`; nothing else in the corpus calls them (the runtime's own `poll_runtime_*`
twins are unreferenced exports).

| # | Stub (`fd_poll_runtime.cs`) | Contract (from `runtime/netpoll.cs`) | Called by | Reached from |
|---|---|---|---|---|
| 1 | `runtime_pollServerInit()` (:20) | Initialize the poller, once (`:217–232`, `netpollGenericInit`) | `pollDesc.init` via `serverInit.Do` (:48) | first pollable `FD.Init` — `net.Listen`, `net.Dial`, accepted conns |
| 2 | `runtime_pollOpen(fd) (uintptr, nint)` (:22) | Register fd; return an opaque nonzero ctx or errno (`:251–284`) | `pollDesc.init` (:49) | same |
| 3 | `runtime_pollClose(ctx)` (:24) | Unregister; only legal after unblock (`:292–308`) | `pollDesc.close` (:61) | `FD.destroy` (last decref) |
| 4 | `runtime_pollWait(ctx, mode) nint` (:26) | Block until IO-ready in mode, or return the deadline/closing code (`:355–374`) | `pollDesc.wait` → `waitRead`/`waitWrite` (:92–106) | `execIO` after `ERROR_IO_PENDING` (`fd_windows.cs:188`) |
| 5 | `runtime_pollWaitCanceled(ctx, mode)` (:28) | Block until IO-ready, IGNORING deadline/closing — Windows-only, after a `CancelIoEx` (`:377–382`) | `pollDesc.waitCanceled` (:108–113) | `execIO` cancel path (`fd_windows.cs:219`) |
| 6 | `runtime_pollReset(ctx, mode) nint` (:30) | Clear consumed readiness; fail fast if closing/expired (`:335–347`) | `pollDesc.prepare` → `prepareRead`/`prepareWrite` (:73–87) | `execIO` before every submit (`fd_windows.cs:164`) |
| 7 | `runtime_pollSetDeadline(ctx, d, mode)` (:32) | Arm/replace/clear the read and/or write deadline; `d` is a RELATIVE ns duration (`:385–467`) | `setDeadlineImpl` (:163–189) | `FD.SetDeadline`/`SetReadDeadline`/`SetWriteDeadline` — `net.Conn` deadlines |
| 8 | `runtime_pollUnblock(ctx)` (:34) | Mark closing; wake both waiters with `pollErrClosing`; stop timers (`:473–505`) | `pollDesc.evict` (:66–71) | `FD.Close` (`fd_windows.cs:406`) |
| 9 | `runtime_isPollServerDescriptor(fd) bool` (:36) | Report whether fd IS the poller's own descriptor (`:242–244`) | `IsPollDescriptor` (:203–205) | test-only; linkname-pinned public surface (go.dev/issue/67401) |
| 10 | `runtimeNano() int64` (:18, `//go:linkname runtimeNano runtime.nanotime`) | Monotonic ns, arbitrary epoch | (declared for the shared file; no Windows-path caller today) | — |

Three contract facts that pin the implementation:

- **The error codes are shared constants, not an enum to invent.** `pollNoError`/`pollErrClosing`/
  `pollErrTimeout`/`pollErrNotPollable` = 0/1/2/3, declared on BOTH sides with "These must match"
  comments (`fd_poll_runtime.cs:119–127`, `runtime/netpoll.cs:44–50`). `convertErr`
  (`fd_poll_runtime.cs:129–146`) maps them to `errClosing(isFile)` / `ErrDeadlineExceeded` /
  `ErrNotPollable`, and `execIO` **panics** on any wait error outside that set
  (`fd_windows.cs:207`) — so the managed implementation may return exactly these four values and
  nothing else. `pollErrNotPollable` is never produced on the Windows path (it comes from the
  unix `eventErr` bit); it stays reserved.
- **One waiter per mode, by contract.** *"Concurrent calls to netpollblock in the same mode are
  forbidden, as pollDesc can hold only a single waiting goroutine for each mode"*
  (`runtime/netpoll.cs:577–578`). `fdMutex` (already operational via the hand-owned
  `runtime_sema_impl.cs`) serializes readers and writers above this layer. The managed pollDesc
  therefore needs a single-slot gate per mode, not a queue.
- **The ctx token is opaque.** Go returns the `*pollDesc` as a uintptr; `internal/poll` only
  stores it (`pollDesc.runtimeCtx`, copied into `operation.runtimeCtx` at `fd_windows.cs:365–366`)
  and passes it back. The managed side mints its own nonzero tokens (a monotonic counter keying a
  `ConcurrentDictionary`) — no pointer, no pollcache, no ABA (§4.1).

**The driver.** `execIO` (`fd_windows.cs:156–232`) is the loop every network operation runs:
`prepare` (reset) → `submit(o)` — an overlapped WSA call handed `&o.o` — → on `ERROR_IO_PENDING`,
`wait` → on success, harvest via `windows.WSAGetOverlappedResult(…, wait: false)`; on
deadline/close, `CancelIoEx(fd, &o.o)` → `waitCanceled` → harvest, mapping `ERROR_OPERATION_ABORTED`
back to the deadline/close error. The submit lambdas across `fd_windows.cs` + `sendfile_windows.cs`
cover, by native entry point: `WSARecv` (Read :459, RawRead :1236), `WSARecvFrom` (ReadFrom family
:615/:653/:691), `WSASend` (Write :757, Writev :885, RawWrite), `WSASendto` (WriteTo family
:912–:1004), `ConnectEx` (:1025), `AcceptEx` (:1041, paired with `GetAcceptExSockaddrs` in
`net/windows/fd_windows.cs:253`), `WSARecvMsg`/`WSASendMsg` (ReadMsg/WriteMsg families
:1352–:1463+), and `TransmitFile` (`sendfile_windows.cs:71`). This census is what §4.3 stages.

## 3. The wall behind the wall — why this design routes AROUND the runtime, not through it

### 3.1 The asmstdcall wall is real…

Wiring stub #1 to the runtime's `poll_runtime_pollServerInit` reaches `netpollGenericInit` →
`netpollinit` → `stdcall4(_CreateIoCompletionPort, …)` (`runtime/windows/netpoll_windows.cs:106`)
→ `asmstdcall`, a `PartialStubGenerator` stub: the runtime's syscall path is hand-written
assembly Go, and go2cs has never implemented it — the whole corpus dispatches through the
hand-owned managed trampoline instead (`syscall/windows/dll_windows.cs:93–122`, an unmanaged
function-pointer `calli` switch). That single call could be patched — `CreateIoCompletionPort` has
a working converted wrapper (`zsyscall_windows.cs:602`) — which is precisely why the wall must be
named as more than `asmstdcall`.

### 3.2 …but it is the SHALLOW wall. The deep wall is the scheduler.

Behind `netpollinit`, the runtime bodies consume, in order of appearance
(`runtime/netpoll.cs`): `lockInit`/`lock`/`unlock` on runtime mutexes with lock-rank bookkeeping
(:223–225), `pollcache.alloc` over `persistentalloc` non-GC memory (:736), the `timer` struct and
its `modify`/`stop` engine (:420–445) — the runtime timer subsystem entire — `gopark` with a
commit callback (:605), `goready` (:568), g-pointer CAS protocols storing goroutine identity in a
uintptr (:557), and the `eface`/`_type` reinterpret of `makeArg` (:757–768). Every one of those is
an organ of the Go scheduler. None exists under the CLR, and none CAN exist: a go2cs goroutine is
a managed thread the CLR schedules (`runtime/managed_impl.cs`, "Honest divergences"), so there is
no g to park, no P to hand a ready g to, and no sysmon to pump `netpoll(delta)`.

That last point is the decisive one. Go's poller is only half an API — the other half
(`netpoll(delta)`, `netpollBreak`, `netpollready`, `netpollAnyWaiters`; the platform interface
listed at `runtime/netpoll.cs:15–40`) is called by **the scheduler itself** from `findrunnable`
and sysmon. Under go2cs nothing would ever pump it: a perfectly-wired conversion would initialize
an IOCP and then block forever, because the thread Go dedicates to draining it IS the scheduler.
Emulating that means writing a scheduler. The ten-contract boundary is the only cut through this
subsystem that does not drag the scheduler across.

### 3.3 The doctrine, and the four precedents

`runtime/managed_impl.cs` states the fork this repo already took, verbatim: *"where a Go mechanism
has no managed counterpart but its PUBLIC CONTRACT does, reimplement the CONTRACT at the API
boundary and never emulate the mechanism… Everything below these entry points stays auto-converted
and simply becomes unreachable."* Four landings prove it at increasing depth:

1. **`sync`'s Mutex/RWMutex/WaitGroup** (`sync/mutex.cs`) — the runtime sleeping semaphore
   reimplemented on `SemaphoreSlim`/monitors; the co-designed starvation handoff explicitly NOT
   emulated.
2. **`internal/poll`'s own `runtime_Semacquire`/`runtime_Semrelease`**
   (`internal/poll/runtime_sema_impl.cs`) — the EXACT mechanical shape this design reuses: an
   `_impl.cs` beside the converted package, `[module: go.GoManualConversion]`, bodies for the
   bodyless partials (which suppresses the `PartialStubGenerator` stubs), keyed by ж-box identity
   where address identity is the contract.
3. **`runtime`'s process-control surface** (`runtime/managed_impl.cs`) — ReadMemStats, the
   traceback/Callers surface, GC entry points: contracts on CLR primitives, converted machinery
   below left dead.
4. **`syscall`'s runtime-provided primitives** (`syscall/syscall_impl.cs`,
   `syscall/windows/dll_windows.cs`) — Exit/Getpagesize and the entire Syscall/SyscallN
   trampoline: the runtime's assembly replaced by a managed dispatcher.

**Durability of the cut.** The ten linknames are one of Go's most compatibility-pinned internal
surfaces — `IsPollDescriptor` carries the hall-of-shame linkname notice ("Do not remove or change
the type signature", go.dev/issue/67401, `fd_poll_runtime.cs:194–202`), and Go 1.23 itself
rewrote `netpoll_windows.go`'s internals (source-tagged completion keys, timer sources —
`runtime/windows/netpoll_windows.cs:18–66`) while the ten crossed unchanged. A seam that Go's own
churn respects is a seam worth owning.

## 4. The design — the ten contracts on .NET's completion machinery

### 4.1 The managed pollDesc

A new hand-owned file, `src/core/internal/poll/windows/runtime_netpoll_impl.cs` (per-GOOS folder;
rides the existing `<Compile Include="$(GoTargetOS)/*.cs" />` glob, `internal.poll.csproj:145` —
no csproj change), carrying `[module: go.GoManualConversion]` per the marker rules, provides the
ten partial bodies over:

```
sealed class ManagedPollDesc {
    object gate;                    // ONE lock; §5 explains why lock-free is not owed
    bool closing;                   // pollUnblock ran (sticky for the desc's lifetime)
    ModeState r, w;                 // per-mode:
      bool ready;                   //   an IO completion arrived and is unconsumed
      bool expired;                 //   deadline passed and not since re-set (STICKY, §5)
      long generation;              //   invalidates stale timer callbacks (Go's rseq/wseq)
      Timer? timer;                 //   armed only while a deadline is pending
    nuint fd;                       // for isPollServerDescriptor bookkeeping only
}
static ConcurrentDictionary<uintptr, ManagedPollDesc> table;   // ctx token -> desc
static long nextToken;                                          // tokens start at 1; 0 = "no ctx"
```

Contract bodies, mapped one-to-one:

- `runtime_pollServerInit` — initialize the delivery mechanism (§4.2); idempotent under the
  caller's `sync.Once`.
- `runtime_pollOpen(fd)` — mint a token, create the desc, associate the fd with the completion
  mechanism; on association failure return `(0, errno)` so `pollDesc.init`'s
  `errnoErr(syscall.Errno(errno))` path stays live. Go REUSES pollDescs from a cache and defends
  staleness with `fdseq` (`runtime/netpoll.cs:199–203, 319–321`); the managed side allocates a
  fresh desc per open and retires the token at close, so the ABA machinery has nothing to defend
  and is deliberately absent.
- `runtime_pollReset(ctx, mode)` — under the lock: if `closing` → 1; if `mode` expired → 2; else
  `ready = false`, return 0. (Order per `netpollcheckerr`, `runtime/netpoll.cs:539–554`.)
- `runtime_pollWait(ctx, mode)` — under the lock, loop: **consume readiness first** (`ready` →
  `ready = false`, return 0 — Go consumes `pdReady` before checking errors,
  `runtime/netpoll.cs:585–589`, which is what lets a completion that raced a deadline still be
  harvested); else if `closing` → 1; else if expired → 2; else `Monitor.Wait(gate)` and loop.
- `runtime_pollWaitCanceled(ctx, mode)` — same loop but IGNORE `closing`/`expired`: wait until
  `ready`, consume, return (void). Liveness is §5's cancellation argument.
- `runtime_pollSetDeadline(ctx, d, mode)` — §5.
- `runtime_pollUnblock(ctx)` — under the lock: `closing = true`, bump both generations, stop both
  timers, `Monitor.PulseAll`. Sticky forever; Go's re-open reset does not apply (fresh desc per
  open).
- `runtime_pollClose(ctx)` — dispose timers, remove the token from the table. (Go asserts
  "close w/o unblock" / "blocked read on closing polldesc" throws here,
  `runtime/netpoll.cs:295–305`; the managed body keeps the same asserts as `throw`-equivalent
  `InvalidOperationException`s — they guard `internal/poll`'s own sequencing, which is unchanged
  converted code.)
- `runtime_isPollServerDescriptor(fd)` — §4.2 (delivery-mechanism dependent; `false` under the
  recommendation).
- `runtimeNano()` — the `sync/runtime_impl.cs:248–251` shape verbatim: `Stopwatch.GetTimestamp()`
  scaled to ns against a static base. Monotonic, arbitrary epoch, exactly the `runtime.nanotime`
  contract.

The completion callback (§4.2) is the only writer of `ready`: under the lock, `ready = true`,
`PulseAll`. Timeout and close wake waiters WITHOUT setting `ready` — the two-layer separation
(wake vs readiness) is the load-bearing structure Go encodes in `pdReady`-vs-`pdNil`-wake, and it
is what makes `waitCanceled`'s ignore-errors loop correct.

**Why one Monitor instead of Go's lock-free CAS protocol.** Go splits `pollDesc` state across
atomics (`rg`/`wg`, `atomicInfo`) because `netpollcheckerr` runs where the pd lock cannot be taken
and `netpollblock` parks through `gopark`'s publication protocol (`runtime/netpoll.cs:83–95,
556–613`). Neither constraint survives the boundary: every managed caller is an ordinary blocked
thread, and the single-waiter-per-mode contract bounds convoying to one reader + one writer + one
timer callback per desc. A Monitor is the honest primitive; the CAS choreography would be emulation
of a mechanism whose reason evaporated. (Precedent: `runtime_sema_impl.cs` made the same reduction
for the runtime semaphore — "a bucket is just a count plus a FIFO waiter queue.")

### 4.2 Completion delivery — two candidate mechanisms

The poller's one irreducible job on Windows: when the kernel completes an overlapped operation
that `internal/poll` submitted, set `ready` on the right desc's mode. Go does it by owning an IOCP
and draining it from the scheduler (`GetQueuedCompletionStatusEx` in `netpoll()`); the completion
routes back via *"the overlapped is the first field of `operation`"* pointer arithmetic
(`fd_windows.cs:72–74`, `runtime/windows/netpoll_windows.cs:58–66`). The managed replacement has
two candidate shapes:

**(a) `ThreadPoolBoundHandle` — the CLR's own IOCP (RECOMMENDED).** At `pollOpen`, bind the socket
handle (`ThreadPoolBoundHandle.BindHandle(SafeHandle)`); per operation record (§4.3), allocate the
NativeOverlapped from a reusable `PreAllocatedOverlapped` whose callback closes over the record →
desc → mode. The CLR's IO thread pool dequeues the completion and runs the callback with
`(errorCode, numBytes, NativeOverlapped*)`; the callback signals `ready`. This is *".NET's own
completion-port machinery"* in the board's words, literally: no poller thread of ours, no
shutdown/teardown story, no key-routing table addressed by raw pointers, AOT-compatible, and the
same engine .NET's own `Socket` rides. `runtime_pollServerInit` reduces to state initialization;
`runtime_isPollServerDescriptor` returns `false` for every fd — there IS no exposed poll-server
descriptor (the contract's only consumer is the test-only `IsPollDescriptor`; a Go program
cannot legitimately hold the poller's fd on Windows anyway).

**(b) Own IOCP + a dedicated managed poller thread.** `runtime_pollServerInit` creates the port
(the converted `syscall.CreateIoCompletionPort` wrapper works — `zsyscall_windows.cs:602`);
`pollOpen` associates with a completion key; a background thread blocks in
`GetQueuedCompletionStatus(Ex)` and routes by key. Closer to Go's shape;
`isPollServerDescriptor` gets a real answer. Costs: a thread whose lifecycle nobody owns (host
shutdown, test-host isolation), a NativeOverlapped-to-record routing table keyed by raw native
addresses, and hand-rolled dequeue marshalling — all machinery (a) gets from the CLR for free.

Recommendation: **(a)**, with (b) documented as the fallback if a real `BindHandle` constraint
surfaces (e.g., a handle the CLR refuses to bind). Nothing in the contract layer changes between
them — the choice is confined to `pollServerInit`/`pollOpen` and the callback's plumbing — so a
later swap is not a redesign. **OQ1.**

> **RESOLVED at S1 (2026-08-13): mechanism (a) holds.** `ThreadPoolBoundHandle.BindHandle` accepts a
> Go-created socket handle against a real kernel — `net.Listen` completes and `NetListenSmoke` matches
> `go run` byte for byte. The fallback to (b) is not needed.
>
> **AMENDED at S2 (2026-08-14): the bind moves from `pollOpen` to the FIRST SUBMIT (lazy), inside the
> §4.3 record machinery.** This is the plumbing latitude this paragraph already grants ("confined to
> `pollServerInit`/`pollOpen` and the callback's plumbing"), taken for a reason S1 measured rather than
> a preference. Go's poller REJECTS completions it does not own: `pollOperationFromOverlappedEntry`
> (`runtime/netpoll_windows.go`) checks the completion key against the `pollDesc` pointer packed into
> it and returns nil on mismatch, citing go.dev/issue/58870 — the issue `internal/poll`'s own
> `TestWSASocketConflict` regression-guards. The CLR's `ThreadPoolBoundHandle` offers no equivalent:
> its callback resolves state FROM the `NativeOverlapped` it allocated, so a foreign overlapped
> arriving at that port is MISREAD rather than ignored.
>
> Binding at `pollOpen` therefore made every pollable socket eligible to receive a foreign completion,
> and `internal/poll` has a live path that produces one: `FD.WSAIoctl`
> (`windows/sockopt_windows.cs:11`) bypasses `execIO` entirely and hands the kernel the CALLER's
> `syscall.Overlapped` — which, being an all-scalar struct in a STANDARD box, really does pin and
> really does reach the kernel. Binding lazily removes the hazard structurally instead of detecting it:
> a socket that only ever sees foreign overlapped IO is never associated with the CLR's port, so its
> completions signal the caller's own event exactly as on an unregistered socket, and a socket doing
> our IO is bound at its first submit, after which every operation on it carries a CLR-allocated
> overlapped. The residual — one socket doing BOTH — is not reachable in the corpus: a census of every
> `FD.WSAIoctl` caller (`net/windows/fd_windows.cs:177` SIO_TCP_INITIAL_RTO,
> `net/windows/tcpsockopt_windows.cs:124` SIO_KEEPALIVE_VALS) shows both pass a **nil** overlapped, so
> no production path issues an asynchronous foreign operation at all.
>
> **AMENDED 2026-10-21 (P2; root verified and shape A accepted by G, ruled by COORD): every handle
> but a SOCKET is bound at `pollOpen` again, and a refused bind is `pollOpen`'s errno.** The S2 move
> was right for sockets and wrong for everything else, because Go's `netpollopen` can FAIL and
> `internal/poll` is built around that failure. The IOCP association belongs to the FILE OBJECT, so a
> second handle to an object that is already associated is refused (`ERROR_INVALID_PARAMETER`).
> Go returns the refusal from `pollOpen`, `os.newFile` ignores it (`os/file_windows.go:117-119`), the
> FD's `pd.runtimeCtx` stays 0, and `execIO` gives each of its operations its own event
> (`o.o.HEvent = h | 1`, `fd_windows.go:290-301`) which `waitIO` waits on. A shell's `2>&1` into a pipe
> reaches that state at every start since Go 1.25, when `os.NewFile` began probing `IsNonblock`: stdout
> and stderr are two handles to one pipe object. Bound lazily, the refusal had nowhere to go: the
> first stderr write threw `ArgumentException` out of `ThreadPoolBoundHandle.BindHandle`
> (`zsyscall_windows_wsa_impl.cs:183`) and ended the process with rc 2.
>
> So `runtime_pollOpen` now asks `syscall.GoAssociateAtOpen` for the bind BEFORE it publishes the token
> or the readiness sink, and returns `(0, ERROR_INVALID_PARAMETER)` on refusal having touched neither.
> Only BindHandle's `ArgumentException` is a refusal; it covers "already bound" and "not opened for
> overlapped IO", both of which belong on the event path. A socket, told apart by
> `getsockopt(SO_TYPE)` (GetFileType reports a socket as `FILE_TYPE_PIPE`), keeps the lazy bind
> above: the foreign-overlapped producer is a socket's. The submit seam takes the route PER SUBMIT
> from the caller's `HEvent`, which `execIO` sets from that FD's `pollable()`, so two FDs sharing one
> handle VALUE (`os.NewFile` twice on one handle) each keep their own route; nothing value-keyed decides
> it. Guarded by the windows behavioral row `PipeSharedFileObject`.

One converted behavior interacts here and is kept: `FD.Init` enables
`SetFileCompletionNotificationModes(FILE_SKIP_COMPLETION_PORT_ON_SUCCESS)` for TCP/UDP when safe
and sets `skipSyncNotif` (`fd_windows.cs:333–345`), so synchronously-completing operations post
NO completion packet and `execIO` returns without waiting (`:171–177`). Under (a) that is
supported (it is how .NET's own sockets run) but obligates the record lifecycle to a
"submit may retire with no callback" path — `FreeNativeOverlapped` on the sync-return branch,
re-arm on the next submit. The smaller-state-space alternative — suppress the mode and let every
completion post — is **OQ5**.

### 4.3 The submit seam — the second wall, and the WSA mirror family

Making `pollWait` wake up is HALF the arc. The other half is that the overlapped submissions
`execIO` issues must actually reach the kernel and complete into memory the managed side can
read. Today they cannot, and the board's syscall STRUCT-PASSING census
(`BOARD-next-validation-candidates.md` §"Open — the syscall STRUCT-PASSING seam") predicted
exactly this: `net` is the package that forces the remaining members. Async adds a dimension the
census's synchronous members never had. Three distinct sub-walls, priced separately:

**(1) The native-layout wall (the established class, new members).** `syscall.WSABuf` is
`{uint32 Len; ж<byte> Buf}` (`types_windows.cs:555–558`) — a managed reference where native
`WSABUF` wants a raw `CHAR*`. Every submit lambda passes `&o.buf` / `&o.bufs[0]` / `&o.msg`
(`WSAMsg` — worse: it embeds a `ж<WSABuf>` Buffers pointer and a control buffer). Same class as
`Timezoneinformation`/`win32finddata1`/`RawSockaddrInet4`, same remedy: blittable
`[StructLayout(Sequential)]` mirrors with an explicit field-for-field copy at the boundary.
`AcceptEx`'s output buffer is the class's decode-side: `acceptOne` hands it a
`slice<RawSockaddrAny>` reinterpreted as bytes (`fd_windows.cs:1041`), whose managed layout cannot
hold the native sockaddr block — the mirror must be a NATIVE staging buffer, decoded at harvest
with the L10 mirror helpers (`syscall/windows/syscall_windows_impl.cs` already owns the
`RawSockaddrInet4/6` ↔ native translation both directions).

**(2) The lifetime wall (what async ADDS to the class).** The census's fixed members marshal with
a mirror that is *"a LOCAL at the call site… trivially stable for exactly that long"*
(`syscall_windows_impl.cs` header). An overlapped operation breaks that premise twice over:

- The kernel retains the OVERLAPPED pointer and the buffer pointers until COMPLETION
  — **precision note (2026-08-23, measured against the documented contract by lane R while
  drafting §4.8's ⟨OQ-G⟩):** that retention is EXPLICIT for `lpOverlapped` ("must remain valid
  for the duration") and holds for the DATA buffers the WSABUF entries point at, but the WSABUF
  ARRAY itself is the opposite — the provider must "capture the WSABUF structures before
  returning", stack arrays expressly allowed — and `lpTo` is SILENT (no capture promise, no
  persistence requirement; undefined is worse than either answer). The corpus stages all three
  anyway — for the array that is conservative-by-CHOICE, not wrong-by-construction as §4.7's
  table overstates from this line; a future author should not over-build from the stronger
  reading — seconds,
  minutes, unbounded. golib's address model is explicit about what it can hold still
  (`ж.cs`, `EnsureStableAddress` remarks): a standard box of unmanaged `T` pins its value slot; an
  array/slice-element box pins the CANONICAL BACKING ARRAY (aliasing — the kernel writes the real
  bytes); but an interior field address inside a reference-bearing container — which is exactly
  what `&o.o` is, an `Overlapped` field inside `operation` inside `FD` inside a heap-boxed
  `netFD` — *"is left exactly as it was — a transient address"*, because `GCHandle` cannot pin an
  object containing references. Handing the kernel a transient interior address for an async op is
  the pipe-EOF defect (`ж.cs`'s own war story) with an unbounded window: heap corruption by
  design.
- The OVERLAPPED is the operation's kernel-side IDENTITY. `execIO` names the SAME `&o.o` in three
  separate wrapper calls — submit, `CancelIoEx` (`fd_windows.cs:212`), harvest
  (`WSAGetOverlappedResult`, `:220`) — and cancellation matches BY ADDRESS. Any scheme that
  produces a fresh native copy per call (the natural extension of the local-mirror pattern) breaks
  cancellation outright: `CancelIoEx` would target an address the kernel never saw.

**The remedy: a per-(FD, mode) operation RECORD owning native-lifetime state.** The hand-owned
WSA wrappers keep a table `ж<Overlapped> → OpRecord`. The key works because golib pointer equality
for field-reference boxes compares (source box, field identity) — `ж.cs:63–70` documents that this
exact property exists to serve *"the address-keyed runtime semaphores in the hand-owned
sync/internal-poll implementations"* — and each FD has exactly one read op (`rop`) and one write op
(`wop`) for its lifetime (`fd_windows.cs:242–244, 361–366`), so `&o.o` minted at any call site of
either resolves to the same record. The record owns: the `PreAllocatedOverlapped`/NativeOverlapped
(mechanism (a)) or a `NativeMemory.Alloc`'d OVERLAPPED (mechanism (b)); the native `WSABUF`
array/`WSAMSG`/sockaddr staging blocks; the pins (`ж<byte>` element boxes hold their backing-array
pins for the BOX's lifetime, and the record holds the boxes, covering the op's whole flight); and
the completion results (`errorCode`, `qty`) the callback deposits. `WSAGetOverlappedResult`'s
hand-own answers from the record; `CancelIoEx`'s hand-own targets the record's one true native
address. Out-parameters (`&o.qty`, `&o.flags`, `rsan`) are marshalled through call-local natives
and copied back after the call — never through interior pins — per the mirror-is-a-local doctrine,
which async does NOT break for out-params (they are written only during the synchronous portion
or at harvest, both call-bounded).

**(3) The displacement mechanism.** The wrappers to hand-own are converter-generated with real
bodies (`zsyscall_windows.cs:1516` for `WSARecv`, etc.), so unlike the ten bodyless stubs they
must be DISPLACED, not merely supplied. The established mechanism is `manualConversionFuncs`
(`src/go2cs/manualTypeOperations.go:92`, `goosWindows`-scoped) — a data-only converter map entry
per function that turns the generated body into a placeholder, with the hand-own in an `_impl.cs`
beside it; the sockaddr family (`:499–508`) is the freshest precedent. The affected converted
files regenerate once (the A/B footprint is exactly those files). **OQ2** confirms the mechanism;
**OQ3** confirms the staged scope:

| Stage | Wrappers (owning package) | Unlocks |
|---|---|---|
| S1–S2 (TCP core) | `WSARecv`, `WSASend`, `AcceptEx`, `GetAcceptExSockaddrs`, `ConnectEx`, `CancelIoEx` (`syscall`); `WSAGetOverlappedResult` (`internal/syscall/windows`, wrapper at `windows/zsyscall_windows.cs:521`) | listen/accept/dial/read/write/deadlines — every TCP consumer row |
| S3 (UDP) | `WSARecvFrom`, `WSASendto` (`syscall`); `WSASendtoInet4/6`, `WSARecvMsg`, `WSASendMsg` (`internal/syscall/windows`) | UDP-shaped suites, `ReadFrom`/`WriteTo`/`ReadMsg` paths |
| deferred until reached | `TransmitFile` (`sendfile_windows.cs:71`) | `net`'s sendfile fast path (has a non-sendfile fallback) |

Per the board's standing ruling for this class — *"Do them when a suite reaches them, not
speculatively"* — nothing outside a stage's gate lands with that stage. `LoadConnectEx`'s
`WSAIoctl` function-pointer lookup and the socket-creation path (`WSASocket`, `bind`, `listen`,
`setsockopt`) are synchronous and already work under the existing dispatcher + L10 mirrors; they
are NOT part of this arc's surface.

> **LANDED at S2b (2026-08-15). The S1–S2 row is implemented as written, with ONE correction to the
> paragraph above.**
>
> Implemented: `WSARecv`, `WSASend`, `AcceptEx`, `GetAcceptExSockaddrs`, `CancelIoEx` displaced into
> `syscall/windows/zsyscall_windows_wsa_impl.cs`; `WSAGetOverlappedResult` into
> `internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs`; `ConnectEx` EXTENDED in place
> (it was already hand-owned by L10, exactly as §4.4 predicted) to rearm the record's overlapped.
> `internal.syscall.windows.csproj` flips `AllowUnsafeBlocks` to `true`, also as predicted, and that
> flip is part of the intended footprint. The golib rendezvous is `core/golib/GoAsyncIO.cs`
> (9 GolibTests).
>
> ⚠ **CORRECTION — `LoadConnectEx` does NOT "already work", and it is the reason every dial died.**
> The claim above was an assumption, not a measurement, and the crypto/tls census contradicted it
> nine times over: `failed to find ConnectEx: An invalid argument was supplied`, one per loopback
> dial, each in ~2 ms. Its single `WSAIoctl` is defective at BOTH ends, and neither end is about
> asynchrony — this is the ordinary struct-passing class at a site nobody had censused:
>
> - **IN.** `ᏑWSAID_CONNECTEX.Reinterpret<GUID, byte>()`. `syscall.GUID` carries `Data4 [8]byte` as a
>   golib `array<byte>` MANAGED REFERENCE, so the struct is reference-bearing; `Reinterpret` refuses
>   to alias it, falls back to an unpinned raw-address box, and the 16 bytes Windows compares against
>   its extension-function table are a CLR auto-layout image with an object reference inside. Windows
>   answers `WSAEINVAL` deterministically, on every host, for every program that dials.
> - **OUT.** `ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡaddr).Reinterpret<uintptr, byte>()` is an interior
>   field address inside a struct that holds an `error`, hence unpinnable and transient — so even a
>   succeeding call could write the function pointer into memory the GC had moved.
>
> `LoadConnectEx` is therefore hand-owned too (same file, same stack-mirror pattern; the GUID VALUE
> is still read out of the converted declaration, so the constant keeps one definition). The lesson
> generalizes: "synchronous, therefore fine" is not a safe inference for any wrapper that hands the
> kernel the address of a converted struct — only a census is.

### 4.4 The submit seam, specified — findings from S2a that the implementation should not re-derive

Everything below was measured or read out of the corpus while landing S1/S2a. It is recorded because
each item cost a non-obvious investigation and each one constrains the implementation.

**The reference graph forces a PUSH, and golib is the only place the table can live.** The waiter is
`internal/poll`; the submissions are in `syscall` and `internal/syscall/windows`; `internal/poll`
REFERENCES both, so a completion callback cannot call back into the poller. The three candidate
resolutions and why two lose: a `public` seam on `syscall_package` would add a non-Go symbol to a
PUBLISHED package's API surface; reading the `operation` back from the overlapped (Go's own trick —
`pollOperationFromOverlappedEntry` casts the OVERLAPPED to the enclosing struct) is impossible here
because `ж<T>`'s `m_structFieldRef` is private with no accessor, so a field-reference box cannot be
walked back to its source object. What remains is a descriptor-keyed callback table in **golib**, the
one assembly both sides see. Keep it platform-neutral to belong there: `nuint handle → Action<nint
mode>`, plus an opaque `object` slot for the submitting package's per-descriptor state (naming
`ThreadPoolBoundHandle` in golib would drag Windows into it). The descriptor is the right key because
it is the one identity both sides independently hold — the poller gets it in `pollOpen`, a wrapper
gets it as its own first argument.

**The golib table has two MEASURED requirements that its obvious implementation does not meet
(S2b, prototyped as `GoAsyncIO` with 11 GolibTests; the code is not banked — §4.5 — but these are
priced findings the implementation must honor, not advice).** **(1) Create-exactly-once is
load-bearing, and `ConcurrentDictionary.GetOrAdd` does not provide it.** Its factory may run on
several threads with only one result kept; a contention test built **10** operation records where
the contract wants 1, and each record owns a `PreAllocatedOverlapped` plus native staging buffers,
so the nine discarded ones are a native leak with no owner. The per-descriptor slot has the same
requirement for a harder reason — that object is the `ThreadPoolBoundHandle` association, and
associating one socket twice is a kernel error. Use `Lazy<T>` with `ExecutionAndPublication`, or an
explicit lock over the entry; never bare `GetOrAdd`. **(2) The readiness sink must be REPLACEABLE
per descriptor**, because the kernel reissues descriptor numbers after a close: a registration that
refused to overwrite would leave a stale sink waking a `pollDesc` that no longer owns the fd. A
signal for an unregistered descriptor must be a silent no-op — it runs on a CLR IO thread-pool
thread, where an escaping exception ends the process, and a completion racing a close is a race the
contract permits.

**The record key is verified, not assumed.** `ConcurrentDictionary<ж<Overlapped>, OpRecord>` is sound:
`ж<T>.Equals` compares `SameSource(source1, source2) && fieldId1.Equals(fieldId2)` for a struct-field
reference, and `GetHashCode` returns `SourceIdentityHash(source)` — coarser than Equals (every field of
one `operation` hashes alike) but consistent with it, which is all a dictionary requires. So
`Ꮡo.of(operation.Ꮡo)` minted at `execIO`'s three separate call sites — submit, `CancelIoEx`, harvest —
resolves to ONE record. `ж.cs`'s own comment says this property exists to serve "the address-keyed
runtime semaphores in the hand-owned sync/internal-poll implementations", so the arc is reusing a
guarantee the corpus already depends on rather than leaning on an accident.

**Why `&o.o` genuinely cannot be handed to the kernel, confirmed at the source.** `Overlapped` is
all-scalar (`Internal`, `InternalHigh`, `Offset`, `OffsetHigh`, `HEvent`) and so is blittable on its
own — but that is not what decides it. `EnsureStableAddress` pins `PinnableStorage`, and for a
struct-field reference that recurses to the CONTAINER's storage: the `ж<operation>` box's `m_slot`,
which is **null**, because `operation` holds managed references (`ж<FD> fd`, `WSABuf.Buf`,
`slice<WSABuf> bufs`, `ж<RawSockaddrAny> rsa`) and the value constructor allocates a pinnable slot only
for a `T` free of them. So the address is transient exactly as §4.3 claims. Note the contrast that makes
the `FD.WSAIoctl` hazard real: a caller's own `var ov syscall.Overlapped` is a STANDARD box of a
blittable `T`, so it *does* get a slot, *does* pin, and *does* reach the kernel.

**Wrapper inventory, corrected.** `ConnectEx` is **already hand-owned** by the L10 sockaddr lane
(`syscall/windows/syscall_windows_impl.cs:363`, placeholder at `syscall_windows.cs:1110`) and today
passes `Ꮡoverlapped` straight through to the generated `connectEx` — it must be EXTENDED to use the
record's native overlapped, not newly displaced, and it needs no new `manualConversionFuncs` entry.
The genuinely new entries are `WSARecv`, `WSASend`, `AcceptEx`, `GetAcceptExSockaddrs`, `CancelIoEx`
(all `"syscall"`, `goosWindows`) and `WSAGetOverlappedResult` (a new `"internal/syscall/windows"` key).
All six generated bodies share one shape — marshal through `Syscall`/`Syscall9`, test `r1` against
`socket_error` (or `0` for the BOOL-returning ones), `errnoErr(e1)` — so the hand-owns should reproduce
that error handling verbatim rather than inventing one.

**Mode is known from the wrapper, not from the operation.** The callback must name a mode, and the
record cannot read `operation.mode` (previous point). It does not need to: `WSARecv`/`WSARecvFrom`/
`AcceptEx`/`WSARecvMsg` are always the READ operation and `WSASend`/`WSASendto`/`ConnectEx`/
`WSASendMsg` always the WRITE one, because each `FD` has exactly one `rop` and one `wop` for its
lifetime. The wrapper knows which it is by being itself.

**Where the bind goes.** First submit, inside the record machinery, per the §4.2 amendment — not
`pollOpen`. `AllocateNativeOverlapped` must come from that same binding, and `pollClose` disposes it
through the golib table's opaque slot before `internal/poll` closes the socket.

**`AllowUnsafeBlocks` splits between the two displacement packages, and the difference is owed work.**
`syscall.csproj` already emits `true`, so its mirrors and `NativeOverlapped*` need no marker.
`internal/syscall/windows` emits **`false`**, so its `WSAGetOverlappedResult` hand-own must carry
`[module: go.GoRequiresUnsafe]` if it touches a pointer type — and the regenerated `.csproj` flipping
to `true` is then part of that stage's intended A/B footprint, not drift. (The marker scan reads the
package directory plus its per-GOOS folders, so a hand-own in `windows/` is seen; see
*A hand-owned file can declare that it needs `/unsafe`* in ConversionStrategies-Reference.)

### 4.5 The submit seam, MEASURED — what S2b settled, and the one hole §4.4 did not know about

S2b took the accept decode (§7's blocker, now closed) and then priced the displacement itself against
the corpus rather than against the plan. Three of §4.4's assumptions are now VERIFIED rather than
assumed, one golib design defect was found and fixed in a prototype, and one item in the ruled spec
turns out not to be implementable as written. Everything below is measured; none of it should be
re-derived.

**VERIFIED — the record key really does resolve across execIO's three call sites.** §4.4 asserted this
from `ж.cs`'s comment; it is now traced through the code. All three of `execIO`'s
`Ꮡo.of(operation.Ꮡo)` sites (submit `fd_windows.cs:190`, `CancelIoEx` `:212`, harvest `:220`) mint a
FRESH `ж<Overlapped>`, but each carries the same source object — the one `ж<operation> Ꮡo` parameter
box, so `SameSource`'s `ReferenceEquals` holds — and the same identity token, because `operation.Ꮡo`
is a static accessor method group and `Delegate.Equals` compares method+target. `Equals` returns true
and `GetHashCode` returns `SourceIdentityHash(Ꮡo)` for all three. **The bound on this is worth
carrying**: equality is SOURCE-POINTER identity, not value identity — three DISTINCT `ж<operation>`
heap boxes over the same `operation` value would compare UNEQUAL. `execIO` never does that (the
submit lambda receives the same box via `submit(Ꮡo)`), but any future call site that re-boxes would
silently mint a second record.

**VERIFIED — a user buffer's pin is real, and the record must hold the BOX to keep it.**
`(void*)Ꮡ(someByteSlice, 0)` resolves through `PinnableStorage` → `CanonicalElement` → the slice's
backing `byte[]` → `PinnedBuffer.PinOnly` succeeds (blittable), so the address is genuinely pinned and
cannot move. The pin is a `GCHandle` held by **that one `ж<byte>` box**, released only by
`PinnedBuffer`'s finalizer — so the guarantee ends with the box, and `Ꮡ(s, 0)` mints a fresh temporary
per call. For `WSARecv`/`WSASend` this is satisfied by the corpus already: the box lives in
`operation.buf.Buf` (`InitBuf`, `fd_windows.cs:95`), a field of the `operation` inside the `FD`, so it
outlives the flight. The record must still hold a reference to it rather than only to the address, or
a future `InitBuf` on the same operation could drop the last reference to a box the kernel is still
writing through.

**VERIFIED — why `&o.o` cannot be handed to the kernel, and the exact fallback it takes.**
`Reinterpret<T, TDst>` for a reference-bearing `T` fails BOTH aliasing routes:
`ReinterpretAliasesStorage` is false (`IsReferenceOrContainsReferences<RawSockaddrAny>` is true
because `array<T>` is a struct over a `T[]`, and `LayoutCompatible` then compares 2 source fields
against 0 for `byte`), and `TryPinnedReinterpret` returns null because `GCHandle.Alloc(…, Pinned)`
throws on a non-blittable array. It therefore lands on `(ж<TDst>)(uintptr)box`, which produces a box
with a REAL interior address and **no pin at all** — stale the moment the `fixed` block exits. Note
the compile-level fact underneath: golib's `fixed (void* ptr = &value.Value)` over a managed `T` is
legal C# with `CS8500` suppressed project-wide, so nothing here is caught at build time.

**FIXED IN PROTOTYPE — the golib seam, and a defect worth not repeating.** §4.4 ruled the seam into
golib; S2b prototyped it as `GoAsyncIO` (descriptor → readiness sink; an opaque per-descriptor state
slot; an opaque per-operation slot keyed by the waiter's pointer) with 11 GolibTests, all passing.
The prototype is NOT banked, because dead public API in a published package is worse than a
specification — but two things it measured belong here. **(1) `ConcurrentDictionary.GetOrAdd` does not
guarantee single execution of its factory**: a contention test created 10 operation records where the
contract wants 1, and each record owns a `PreAllocatedOverlapped` and native staging buffers, so the
9 discarded ones are a native leak with no owner. The per-descriptor state has the same requirement
for a stronger reason — that object is the `ThreadPoolBoundHandle` association, and binding one socket
twice is a kernel error. Use `Lazy<T>` with `ExecutionAndPublication`, or an explicit lock; do not use
bare `GetOrAdd`. **(2) The readiness sink must be REPLACEABLE per descriptor**, because the kernel
reissues descriptor numbers after a close and a stale sink would wake a `pollDesc` that no longer owns
the fd.

**SIMPLER THAN PLANNED — `WSAGetOverlappedResult` needs only an ADDRESS from the record.** §4.4 had the
hand-own answering from results the callback deposits, which would have required the record's result
fields to be readable across the package boundary and would have re-derived Windows' own error
mapping. It is enough to look the record up, take the native OVERLAPPED's address, and call the REAL
`WSAGetOverlappedResult` on it through the existing `Syscall6` trampoline: the kernel wrote
`Internal`/`InternalHigh` at completion time, so `wait: false` answers correctly after the CLR has
already dequeued the packet, and — the reason this matters — it returns proper **WSA** error codes
(`WSAEMSGSIZE`, `ERROR_MORE_DATA`), which is what `execIO` and `net` branch on. A callback's Win32
`errorCode` is a DIFFERENT namespace (`ERROR_NETNAME_DELETED` where the suite expects
`WSAECONNRESET`), so deriving the harvest from it would have been a quiet fidelity loss. Consequence:
the golib contract between the two packages narrows to one property, the operation's native address.

**CENSUS — `CancelIoEx` has a second, NIL-overlapped caller.** `fd_windows.cs:403` calls
`CancelIoEx(fd.Sysfd, nil)` to cancel ALL IO on a handle (the pipe/file close path), alongside
`:212`'s per-operation form. The hand-own must pass a nil overlapped straight through rather than
looking anything up, and must answer `ERROR_NOT_FOUND` — which `execIO` already tolerates — for a
non-nil overlapped with no record, rather than falling back to 0 and cancelling everything on the
socket.

> **⚠ THE HOLE, and why S2b stops here rather than landing a half-displaced set.
> `GetAcceptExSockaddrs` carries NO identity the seam can key on, and §4.4 did not notice because it
> reasoned about the overlapped family only.** Its signature is
> `(buf, rxdatalen, laddrlen, raddrlen, *lrsa, *lrsalen, *rrsa, *rrsalen)` — no handle, no overlapped.
> Go recovers everything from `buf`, which is legitimate there because the accept buffer really is the
> caller's `[2]RawSockaddrAny` and the kernel wrote native bytes into it. Under go2cs neither half
> survives: `net` passes `Ꮡ(rawsa, 0).Reinterpret<RawSockaddrAny, byte>()`, which per the measurement
> above is an UNPINNED native-address box over a managed `RawSockaddrAny[]` whose layout is not the
> native one — so the address is unusable as data, AND its identity is physical rather than
> structural, so two evaluations of the same expression are equal only if the GC did not move the
> array in between. It is therefore not usable as a table key either. `AcceptEx` (which does have the
> overlapped, and so can own a native staging buffer in its record) has no way to hand that buffer to
> `GetAcceptExSockaddrs`, and `GetAcceptExSockaddrs` has no way to ask for it.
>
> Two shapes were viable. **(a) A scoped handoff**: `AcceptEx`'s hand-own parks its staging buffer in
> a slot that `GetAcceptExSockaddrs` consumes. Sound for the corpus's only caller — `net.netFD.accept`
> runs `FD.Accept` and then `GetAcceptExSockaddrs` on one goroutine — but it is a NEW coupling between
> two wrappers Go keeps independent, and nothing in the type system sees it. **(b) Transcribe at
> accept-completion into the managed `rawsa`**, making `GetAcceptExSockaddrs` a purely managed parse.
> Note what is NOT a problem under either: the out-parameters. `Ꮡlrsa`/`Ꮡrrsa` are pointers to managed
> `ж<RawSockaddrAny>` VARIABLES, so a hand-own may write FRESH managed boxes into them; nothing in
> `net` requires them to point into `buf`, since the only thing done with them is `.Sockaddr()` — which
> is now hand-owned to decode from the managed image.
>
> > **RULED (coordinator, 2026-08-14): shape (a), with three conditions that convert its one real cost
> > — invisible coupling — into visible, loud machinery.** Shape (b) is **rejected on its own record**:
> > the transcriber must still find the caller's array, so it is the same hole relocated, and its only
> > escape hatch (having `AcceptEx`'s record capture the managed `slice<RawSockaddrAny>`) is
> > self-refuted — `AcceptEx` receives the reinterpreted byte pointer and never sees the slice.
> >
> > 1. **Key the slot by GOROUTINE identity, not by a bare `[ThreadStatic]`.** The identity source is
> >    the scheduler arc's S1 registry, `go.golib.Goroutine` (`src/core/golib/runtime/Goroutine.cs`,
> >    merged), whose per-goroutine current-instance slot and `Id` are exactly this key; the
> >    implementing lane exposes an accessor if none is public yet. §7/OQ6's ownership split explicitly
> >    sanctions netpoll ADOPTING scheduler identity at its own option, and this is that option
> >    exercised. Under the dedicated-thread executor a goroutine is one thread for life, so
> >    `[ThreadStatic]` would work *today* — goroutine-keying makes the coupling's PREMISE explicit,
> >    survives any future executor change, and rides arc-owned identity rather than a runtime
> >    accident. **Say so in the impl header**: the scheduler arc is what makes this handoff sound, and
> >    that cross-arc dependency should be legible to whoever reads either arc next.
> > 2. **Single-occupancy, consume-exactly-once, fail BY NAME.** `AcceptEx` parking into an already
> >    occupied slot throws, naming both wrappers; `GetAcceptExSockaddrs` finding an empty slot throws
> >    likewise. A call sequence Go permits but the corpus never issues must fail LOUDLY, never be
> >    misread — the `NetShareAdd` declared-limit doctrine applied to a protocol instead of to a host
> >    capability.
> > 3. **Document the coupling where it lives**: the impl file's header states the handoff, its
> >    goroutine-affinity premise, and the corpus census that bounds it — one caller,
> >    `net.netFD.accept`, doing accept-then-parse on one goroutine.
>
> S2b's own scope call under that ruling: the seam is ALL-OR-NOTHING (S2a's analysis, unchanged) and
> the round-trip gate cannot be met without accept, so landing
> `WSARecv`/`WSASend`/`CancelIoEx`/`WSAGetOverlappedResult` alone would be exactly the half-displaced
> set the stage forbids. This lane stops at that boundary with the spec SETTLED rather than opening a
> displacement it could not also gate; the next lane implements §4.4 + §4.5 + the ruling above as
> written, against the §7 S2 gate pair (`TcpLoopbackRoundTrip` at VALUE level, plus the §5 deadline
> matrix).

### 4.6 The submit seam, LANDED — what implementing §4.4/§4.5 actually cost (2026-08-15)

Every specified item above held on contact. The list below is what the SPEC did not know, recorded on
the same terms it set: each cost a real investigation, and none of it should be re-derived.

**The record key is stable ACROSS CALLS, not merely across execIO's three sites — and that mattered
more than the property §4.5 verified.** §4.5 traced `Ꮡo.of(operation.Ꮡo)` through submit, cancel and
harvest *within one `execIO`*, where the same `ж<operation>` box is passed to all three. What the
implementation depends on additionally is that TWO SEPARATE `FD.Read` calls also resolve to one
record: each mints a fresh `Ꮡfd.of(FD.Ꮡrop)` intermediate, and only `SameSource`'s RECURSIVE pointer
resolution (`ж.cs`, "an `of()` chain mints a FRESH intermediate box on every access") makes those
equal. Had equality been object identity at that level, every read would have built a new record with
its own `PreAllocatedOverlapped` and native staging — a per-operation native leak that no test would
have shown as anything but memory growth.

**The overlapped must be freed at the NEXT submit, not in the completion callback.** `execIO` harvests
with `WSAGetOverlappedResult` *after* its wait returns, and that call reads `Internal`/`InternalHigh`
out of the very control block the kernel wrote, so freeing in the callback destroys the result before
anyone reads it. Free-then-allocate at each `Rearm` is also what `PreAllocatedOverlapped` requires (one
outstanding allocation at a time) and what covers the `skipSyncNotif` path, where a synchronously
successful submit posts no packet and no callback ever runs. One rule serves all three.

**⚠ `ж<T>.IsNull` and `ж<T>.Value` both VALUE-PEEK, and a hand-own writing a POINTER out-parameter is
exactly where that bites.** `GetAcceptExSockaddrs` writes `**RawSockaddrAny` out-params, i.e. a
`ж<ж<RawSockaddrAny>>` whose held value is legitimately `nil` on entry. Two consequences, both
measured as failures before they were understood:

- A `Ꮡp is not null && !Ꮡp.IsNull` guard reads FALSE-NIL for such a box (`IsNull` is true whenever the
  held reference is null), so the write was silently skipped and `net` then dereferenced a nil
  `lrsa`. The pointer-identity predicate is `Ꮡp != nil`, which is what the corpus's own generated code
  uses; `IsNull` is a dereference guard and nothing else.
- `Ꮡp.Value = box` then panics with a nil-pointer dereference on the very write that fills it in,
  because `Value`'s nil check consults the same value-peeking predicate. **`ValueSlot` is the
  documented form for this shape** and golib's own remarks name it: "a heap-boxed pointer… captured by
  a closure (a `ж<ж<T>>`)". Any future hand-own with a `**T` out-parameter owes both.

**The accept handoff, as built.** Shape (a) with the three ruled conditions, keyed on
`golib.Goroutine.Current` (exposed publicly for this; `Id` deliberately stays internal, so the key is
an opaque identity and not a number a program could build goroutine-local storage on). One refinement
the ruling's letter did not cover and the corpus forces: **re-parking the SAME operation record is a
REPLACE, not a fault.** `FD.Accept` re-submits `fd.rop` after a `WSAECONNRESET` with no intervening
parse, which is a sequence the corpus really does issue; the park's content is identical anyway (same
record, same staging block), so replacing is idempotent. Parking over a *different* record still
throws, naming both wrappers, which is the case the ruling is actually about — two distinct accepts
interleaved on one goroutine.

**What the record does NOT need to hold.** §4.4's "the record owns the completion results" line is
superseded by §4.5's own simplification and the implementation confirms it: the callback deposits
nothing at all. It signals a descriptor and a mode, and the harvest asks Windows. That keeps the
cross-package contract at one property (the native address) and keeps WSA-vs-Win32 error namespaces
from ever being confused.

### 4.7 The DATAGRAM submit, and how the golib seam extends to reach it — RATIFIED (coordinator, 2026-08-23) — all five ⟨OQ⟩s RULED per their §4.7.5 recommendations. On ⟨OQ-B⟩, the second opinion asked for: the duplication is accepted on PRINCIPLE, not tolerance — an ABI-pinned LAYOUT cannot drift between copies while remaining correct, because the OS is the single source of truth for its shape; duplicated logic drifts, duplicated ABI mirrors are re-derivations of the same external fact. Implementation may bind: primitives + their GolibTests first (⟨OQ-D⟩), send-only with the WSARecvFrom measurement taken not assumed (⟨OQ-C⟩), guard registered in the same change (⟨OQ-E⟩); parks merged-ready under the release-eve freeze in the wrappers’ merge-order slot

> **STATUS: PROPOSED. Nothing in this section is ratified**; §4.7.5 collects the questions that need
> a ruling before implementation. Commissioned by the coordinator (mailbox, 2026-08-23) after this
> lane HALTED an attempted implementation at the design line rather than force it through: *"Shape 1
> is REJECTED for the reason the file itself states — a Go-shaped public seam on `syscall`'s
> published surface is the thing its header exists to prevent… Shapes (3)-then-(2) are RATIFIED as
> the path: draft the amendment… post RATIFY?, and implement under it."* Companion:
> [`DESIGN-linux-udp.md`](DESIGN-linux-udp.md), whose S1 landed the Linux half of the same seam.

#### 4.7.1 What reached the wrappers, and what it measured

§7's S3 said the UDP wrappers *"land here gated by whichever suite reaches them first."* A suite has
now reached them: `UdpLoopbackRoundTrip`, written for the Linux datagram seam, runs **byte-identical
to `go run` on Linux** and on Windows dies at

```
System.NotImplementedException: WSASendtoInet4: external (assembly or cgo) function is not implemented
  at internal/syscall/windows.WSASendtoInet4        (PartialStubGenerator stub)
  at internal/poll.WriteToInet4                     fd_windows.cs:964
  at net.writeToInet4 -> net.UDPConn.WriteTo
```

An attempted implementation established three facts worth keeping, each measured rather than
reasoned:

1. **The stub side is trivial and the registry displaces it cleanly.** With bodies present,
   `internal.syscall.windows` builds 0 errors and a scope guard pins the two names to windows.
2. **The address side is already solved.** `syscall/windows/syscall_windows_impl.cs` predicted this
   arrival in its own header — *"WSASendto / wsaSendtoInet4 / wsaSendtoInet6 … writeNativeSockaddr is
   what they would need"* — and exposing it as `GoWriteNativeSockaddrInet4/6`, symmetrically with the
   Linux mirror's seam under `DESIGN-linux-udp.md` ⟨OQ-2⟩, works.
3. **The submit side is the wall, and it is THIS design's, not a wrapper's.** A UDP send is an
   overlapped submit. The ratified template is `WSASend` in
   `syscall/windows/zsyscall_windows_wsa_impl.cs`:

   ```csharp
   OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeWrite);
   NativeOverlapped* native  = operation.Rearm();
   NativeWSABuf*     buffers = stageBuffers(operation, Ꮡbufs, bufcnt);
   Syscall9(procWSASend.Addr(), 7, s, buffers, bufcnt, &sent, flags, (uintptr)native, …);
   ```

   All three helpers are **private to `syscall`'s WSA hand-own**, while the declarations they would
   serve live in `internal/syscall/windows`. §4.5's harvest narrowed the cross-package contract to
   *"one property, the operation's native address"*; a SUBMIT needs more than that, and this section
   is about exactly how much more.

A fourth fact, cheap and easy to lose: a hand-own must not initialise a `LazyProc` in a **field
initializer** that reads a generated sibling's field. C# orders static field initializers within a
type but **not across the files of a partial class**, so `= modws2_32.NewProc("WSASendTo")` ran while
`modws2_32` was still null and the first send died in `LazyDLL.Load()` with a nil dereference. Defer
with `??=`. (Measured by crash; on the board with the bash-glob gate trap.)

#### 4.7.2 The constraint that decides the shape

Two rules already ruled, which any answer must respect:

- **golib's seam is deliberately neutral.** `GoAsyncIO` is *"PLATFORM-NEUTRAL and mechanism-neutral: a
  descriptor, a mode, an address, and two opaque state slots. Naming Windows' completion machinery
  here would drag one platform's IO model into the shared runtime."* So golib must not learn what a
  `WSABUF` is, nor what a sockaddr is.
- **`syscall` must not grow a Go-shaped public seam.** Its own header states it, and the coordinator's
  rejection of shape 1 restated it: the record must not be exposed, and a published package does not
  gain non-Go symbols to serve another package's convenience. (The `Go…NativeSockaddr…` helpers are
  the boundary case that IS permitted, ruled under ⟨OQ-2⟩: they translate a LAYOUT, they expose no
  state, and both mirrors carry them symmetrically.)

Between those, the datagram submit needs exactly two things it cannot obtain today, and neither is
Windows-shaped when stated properly:

| need | why the caller cannot do it itself | neutral statement |
|:--|:--|:--|
| the operation's **native OVERLAPPED**, rearmed for a new submit | the record owns it (`PreAllocatedOverlapped`), it must survive until completion, and creating a second one for the same waiter is a native leak the S2b prototype measured | *"the native address associated with this (descriptor, waiter-address, mode), prepared for a new operation"* |
| **native staging memory owned by the operation** for the WSABUF array | §4.3(2)'s lifetime wall: the kernel retains buffer pointers until completion, so a stack image is wrong by construction; the record is the only thing with the right lifetime | *"N bytes of native memory whose lifetime is this operation's"* |

Neither mentions Winsock. The CALLER writes the `WSABUF` bytes into the staged memory itself, exactly
as it already writes the sockaddr image through the mirror's seam.

#### 4.7.3 The proposal

**Extend `GoAsyncIO` by two primitives, in its existing vocabulary, and leave everything else where it
is.** Names illustrative:

```csharp
// The submit-side counterpart of TryGetOperationAddress, which the harvest already uses.
// Creates the record on first use -- with Lazy<T>(ExecutionAndPublication), never bare GetOrAdd
// (S2b measured 10 records where the contract wants 1, each owning native resources) -- and
// returns the native control block prepared for a new operation.
public static nuint RearmOperation(nuint descriptor, object waiterKey, nint mode);

// N bytes of native memory whose lifetime is the operation's, released when the record is.
// The caller decides what the bytes mean; golib only owns the allocation.
public static nuint StageOperationBuffer(object waiterKey, int byteCount);
```

`syscall`'s existing `operationFor`/`Rearm`/`stageBuffers` become the in-package callers of the same
two primitives, so there is ONE record store and ONE staging owner rather than two that must agree.
That is the property that makes this an extension rather than a parallel mechanism: today
`internal/syscall/windows` harvests from the same store it cannot submit into, which is the asymmetry
this section removes.

`WSASendtoInet4` then reads, in full:

1. `GoWriteNativeSockaddrInet4(to, stackBuffer)` — the address image (already ratified, already works).
2. `RearmOperation(s, overlapped, modeWrite)` — the native OVERLAPPED.
3. `StageOperationBuffer(overlapped, sizeof(NativeWSABuf) * bufcnt)` — then write the WSABUFs into it.
4. `Syscall9(procWSASendTo.Addr(), 9, …)` with those three addresses.
5. The dead generated body's error mapping, verbatim: `socket_error` → `errnoErr(e1)`, and a
   `socket_error` with no errno → `EINVAL`, never success.

#### 4.7.4 Alternatives, priced

- **Move the whole submit machinery into golib** (records, staging, and the WSABUF mirror). Rejected
  on the neutrality rule: golib would learn Winsock's structures, which §4.4's ruling exists to
  prevent, and the mirror family would split across two assemblies.
- **Expose `syscall`'s record**, or a Go-shaped `WSASendtoNative` on `syscall`. Rejected by the
  coordinator and by the file's own header; recorded here so the next author does not re-propose it.
- **Duplicate the record store in `internal/syscall/windows`.** Two stores keyed by the same waiter
  pointer, one harvesting what the other submitted: the failure mode is a harvest that finds no record
  and answers `ERROR_NOT_FOUND` for a live operation. Rejected as the worst of the three.
- **Do nothing; leave Windows UDP unimplemented.** Defensible today — no roster row sends UDP on
  Windows — and it is what the arc has done so far. Its cost is that `UdpLoopbackRoundTrip` cannot be
  REGISTERED: registered it fails the Windows suite, unregistered it fails
  `check-solution-integrity.ps1` (both measured), so the Linux seam keeps its guard out of tree.

#### 4.7.5 Open questions

* **⟨OQ-A⟩ — the two primitives' shape.** Is `RearmOperation` + `StageOperationBuffer` the right cut,
  or should the seam expose a single "prepare a submit" call returning both? *Recommendation:* **two
  primitives.** They are separately meaningful (a submit with no staged buffers is legitimate), and
  the harvest's precedent is that the narrowest property wins.
* **⟨OQ-B⟩ — who owns the WSABUF mirror type.** The caller writes WSABUF bytes into staged memory, so
  the `NativeWSABuf` layout would exist in BOTH `syscall` and `internal/syscall/windows`.
  *Recommendation:* **accept the duplication of a 2-field layout**, documented in both, rather than
  publish a type from `syscall`. It is 8 bytes of shape, not behavior; the alternative re-opens the
  public-seam question for less.
* **⟨OQ-C⟩ — scope of the first landing.** `WSASendtoInet4/6` alone, or also the `WSARecvFrom` path?
  *Recommendation:* **send only.** `WSARecvFrom` already has a generated body that the guard's read
  path exercises; whether it carries the same struct-passing defect is a MEASUREMENT this arc should
  take before assuming, exactly as the Linux half did.
* **⟨OQ-D⟩ — does this want golib tests before the wrappers?** *Recommendation:* **yes** — the S2b
  prototype's two lessons (single-execution factory; replaceable sink) came from GolibTests, and the
  leak mode here is identical. Gate the primitives with their own tests first.
* **⟨OQ-E⟩ — the guard's registration.** Once Windows sends, `UdpLoopbackRoundTrip` registers and runs
  on both platforms. *Recommendation:* **land it in the same change**, since its whole point is to be
  the two-platform gate for this seam, and it is written and Linux-proven already.

### 4.8 The DECODE side — operation-owned native staging, decoded at HARVEST — RATIFIED (coordinator, 2026-08-23) — ⟨OQ-F⟩ callback, ⟨OQ-H⟩ demand order, ⟨OQ-I⟩ both-platform-in-one-change, all per recommendation. **⟨OQ-G⟩ AMENDED to FIX-BY-DEFAULT in this increment**: a use-after-return handed to the kernel is the struct-passing family’s LIFETIME sibling, and that class does not get empirical exoneration — the lane’s own words ("has not misbehaved, which proves nothing about a race") are the ruling’s grounds. The measurement that stands is the documented contract (the overlapped parameter-lifetime remarks), cited in the fix’s header; no race-hunt is owed, and the three-line staging lands with this increment unconditionally

> **STATUS: RATIFIED** (coordinator, mailbox 2026-08-23, at master `dba036de0`). ⟨OQ-F⟩, ⟨OQ-H⟩ and
> ⟨OQ-I⟩ were ruled as recommended; **⟨OQ-G⟩ was AMENDED to FIX-BY-DEFAULT** — *"a use-after-return
> handed to the kernel is the struct-passing family's LIFETIME sibling, and that class does not get
> empirical exoneration"* — so the staging lands unconditionally, citing the documented contract,
> with no race-hunt. §4.8.7 records each question with its ruling. **This section AMENDS §4.3**, whose sub-wall (1) already named this
> shape — *"`AcceptEx`'s output buffer is the class's decode-side… the mirror must be a NATIVE
> staging buffer, decoded at harvest"* — and left it unspecified. Commissioned by the coordinator
> (mailbox, 2026-08-23) on landing §4.7: *"the recv increment is YOURS, queued AFTER F1,
> amendment-first… stated so it covers `AcceptEx`'s output buffer too (the shape is shared; the
> amendment should say so once, not be re-derived when accept arrives)."* Companion: §4.7, whose two
> golib primitives this reuses rather than duplicates.

#### 4.8.1 What reached it, and what it measured

> **⚠ CORRECTED 2026-08-23 — §4.8.1's attribution is wrong; see the note in §4.7.6.** The recv panic
> is NOT the struct-passing class: it is `(ж<array<T>>)(uintptr)` reinterpreting native bytes as a
> managed array reference (61 corpus sites, 35 in `runtime`). **What this does NOT invalidate:** the
> §4.8.3 hook census, the §4.8.5 coverage table, and ⟨OQ-G⟩, which rests on the documented
> `WSASendTo` contract rather than on this attribution. **What it leaves open:** whether the receive
> needs this section's seam AT ALL — the panic fired before anything could observe whether the kernel
> filled the managed box, so there is no evidence either way until the byte-view defect is fixed.

§4.7 made Windows SEND datagrams. `UdpLoopbackRoundTrip` therefore stopped dying at the submit and
started dying at the READ:

```
panic: runtime error: index out of range [0] with length 0
  at internal/poll.rawToSockaddrInet4        fd_windows.cs:1300
  at internal/poll.(*FD).ReadFromInet4       fd_windows.cs:621
```

**The attribution is the load-bearing part, because the shape it panics in is innocent.**
`sockaddrInet4ToRaw` (`fd_windows.cs:1277`) performs the *identical*
`(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(raw.of(…ᏑPort)))` round-trip in the WRITE direction
and works today — every TCP connect exercises it. What differs is the BOX: the read hands
`@new<syscall.RawSockaddrAny>()` — a managed struct whose `Addr`/`Zero`/`Pad` are managed arrays —
to `WSARecvFrom` by address, and decodes it after the fact. Per this project's AV-vs-panic triage
rule (golib's `array<T>` indexer is bounds-checked, so a clean panic means an EMPTY array and an
AccessViolation means a CORRUPTED one), an empty array says nothing was ever materialised there.

Sixth confirmed member of the struct-passing class, and it is **pre-existing, not introduced**: it
was simply unreachable, because the send stub threw first and no Windows program ever completed a
UDP read. Same pattern as `LocalTimeZone` — binding a real implementation exposes what the stub hid.

#### 4.8.2 Why §4.7's remedy does not transfer

The send's remedy is a `stackalloc` image written BEFORE the call. It cannot work here for a reason
that is structural rather than incidental: **on the send the wrapper writes and the kernel reads; on
the receive the KERNEL writes and the wrapper reads — and for an overlapped operation the kernel
writes AFTER the wrapper has returned.** There is no moment inside the wrapper at which a decode
could run. This is §4.3's sub-wall (2), the lifetime wall, applied to the output direction: the
staging must belong to the OPERATION, and the decode must happen when the operation completes.

#### 4.8.3 Where the decode hook goes — measured, not chosen

`execIO` (`fd_windows.cs:150–232`) has exactly three exits after a submit:

| exit | line | harvests? |
|:--|:--|:--|
| submit returned success AND `fd.skipSyncNotif` | ~173 | **no** — returns `o.qty` immediately |
| normal completion after `pd.wait` | ~190 | yes — `windows.WSAGetOverlappedResult` |
| cancellation path after `waitCanceled` | ~220 | yes — `windows.WSAGetOverlappedResult` |

So **every asynchronous completion funnels through one call**, `windows.WSAGetOverlappedResult`,
which is already hand-owned as the netpoll harvest seam. That is the hook, and it is a measured
property of the generated code rather than a design preference. The synchronous-completion exit
needs its own handling: there the data is already present when the submit wrapper returns, so the
decode runs inline at the end of the submit.

#### 4.8.4 The cross-package problem, stated neutrally

The same split §4.7 hit, mirrored. The LAYOUT knowledge lives in `syscall` (the L10 mirror owns
`RawSockaddrInet4/6` ↔ native translation in both directions, and `readNativeSockaddr` is already
written); the HARVEST lives in `internal/syscall/windows`. Neither may reach into the other, and
golib must not learn what a sockaddr is.

Stated without naming Winsock, the receive needs one thing it cannot obtain today: **an operation
must be able to carry work it owes when it completes.** Proposed as a third primitive in
`GoAsyncIO`'s existing vocabulary — the staging primitive it pairs with already exists from §4.7:

```csharp
// Work this operation owes on completion. Stored opaquely; run at most once per submission.
public static void SetOperationCompletion(object key, Action<nint> onComplete);

// Runs whatever the operation owes, with the completed byte count. Idempotent per submission.
public static void CompleteOperation(object key, nint bytesTransferred);
```

`syscall`'s hand-owned `WSARecvFrom` then reads, in full:

1. `StageOperationBuffer(overlapped, GoNativeSockaddrLen)` — native, operation-owned (§4.7's primitive).
2. `SetOperationCompletion(overlapped, n => …)` — the closure holds the staged pointer and the
   managed `ж<RawSockaddrAny>` destination, and does the field-for-field copy through the mirror's
   own decode. **The layout never leaves `syscall`.**
3. `RearmOperation` + the `WSARecvFrom` submit with the staged address (§4.7's other primitive).
4. On synchronous completion, call `CompleteOperation` inline before returning.

`internal/syscall/windows`' `WSAGetOverlappedResult` calls `CompleteOperation(overlapped, qty)`
after a successful harvest, and knows nothing about what that work is.

#### 4.8.5 Coverage — the whole class, said ONCE

The census over `fd_windows.cs`. **Three shapes, seven sites, one mechanism** — this is the section
that should not be re-derived when accept arrives:

| shape | sites | what the kernel writes |
|:--|:--|:--|
| `WSARecvFrom` | `ReadFrom` (:617), `ReadFromInet4` (:655), `ReadFromInet6` (:693) | one sockaddr into `o.rsa`, length into `o.rsan` |
| `WSARecvMsg` | `ReadMsg` (:1330), `ReadMsgInet4` (:1364), `ReadMsgInet6` (:1399) | a sockaddr **and** a control buffer, through a `WSAMsg` that itself embeds a `ж<WSABuf>` |
| `AcceptEx` | `acceptOne` (:1033) | a single output block holding TWO sockaddrs, split by `GetAcceptExSockaddrs` — ⚠ **ALREADY DONE**, see below |

**⚠ CORRECTION (2026-08-23, at implementation): `AcceptEx` was ALREADY FIXED when this table was
written, and it is the TEMPLATE rather than a member of the backlog.** `AcceptEx` stages into the
record's own native block and `GetAcceptExSockaddrs` transcribes it into managed `RawSockaddrAny`
values field for field, precisely so `RawSockaddrAny.Sockaddr` — which flattens the managed struct
back to its 116-byte native image — has a faithful thing to read. Those two are documented in their
own headers as *"a pair; neither is meaningful alone"*. I listed accept as unfixed because I
censused the SHAPE (`rsan = 116` over a `slice<RawSockaddrAny>`) without checking whether the
wrappers around it were already hand-owned. The implementation is the third member of that pair, not
a new mechanism — which is also why §4.8's proposed flat-sockaddr follow-on dissolved.

All three are the same mechanism with a different decode closure: stage native at submit, copy back
at harvest. `AcceptEx` differs only in that its staged block is larger and its decode calls
`GetAcceptExSockaddrs` before translating; `WSARecvMsg` differs only in that it stages two blocks.
**Nothing in §4.8.3 or §4.8.4 changes for any of them.**

#### 4.8.6 Alternatives, priced

- **Hand-own `internal/poll`'s `ReadFromInet4/6` (and the other five) instead.** The managed
  destination is right there, so no cross-package seam is needed at all. Rejected on cost and drift:
  each of the seven carries lock/deadline/`execIO` logic that would be duplicated and would then
  diverge from Go's source at every upstream change, for seven functions rather than one primitive.
  Worth restating because it is the obvious first idea.
- **Decode lazily, at the point of use.** `rawToSockaddrInet4` is generated in `internal/poll` and
  has no idea an operation exists; making it aware means hand-owning it and every peer. Rejected.
- **Make `RawSockaddrAny` blittable so the kernel can write it directly.** This is the remedy that
  would delete the whole class, and it is out of scope here by orders of magnitude — it is a change
  to how golib lays out Go fixed arrays inside structs. Named so it is on the record, not proposed.
- **Do nothing; leave Windows UDP receive unimplemented.** Its cost is now concrete rather than
  hypothetical: ⟨OQ-E⟩ cannot close, so `UdpLoopbackRoundTrip` — written, Linux-proven, and the only
  two-platform gate this seam has — stays out of tree indefinitely.

#### 4.8.7 Open questions

* **⟨OQ-F⟩ — is the completion callback the right cut, or too big a concept for the seam?** §4.7's
  ⟨OQ-A⟩ ruled that the narrowest property wins. A delegate is wider than *"N bytes of native
  memory"*. The alternative is for golib to hand the harvest the staged POINTER and let
  `internal/syscall/windows` do the decode through a public `Go…ReadNativeSockaddr…` on the mirror —
  but that spreads the layout across two packages, which ⟨OQ-2⟩'s reasoning argues against.
  *Recommendation:* **the callback**, precisely because it keeps the layout in one package; the
  delegate is opaque to golib, which is the neutrality test that matters.
* **⟨OQ-G⟩ — ⚠ does §4.7's LANDED send have a latent lifetime bug I should fix in this increment?**
  Raised against my own work rather than found by a gate. `WSASendtoInet4` writes the sockaddr into a
  `stackalloc` buffer and hands its address to an OVERLAPPED `WSASendTo`. If the kernel retains
  `lpTo` until completion — as §4.3's lifetime wall describes for the buffer pointers — then that
  address dies at wrapper return and the send is handing the kernel a use-after-return. It has not
  misbehaved in testing, which proves nothing about a race.

  **MEASURED against the contract (2026-08-23), and the finding is an ASYMMETRY rather than a
  verdict.** `WSASendTo`'s documentation is explicit about lifetime in two places and silent in the
  third:

  | parameter | what the contract says |
  |:--|:--|
  | `lpBuffers` (the WSABUF ARRAY) | *"it is the Winsock service provider's responsibility to capture the WSABUF structures before returning from this call. This enables applications to build stack-based WSABUF arrays"* — captured, so a stack image is explicitly ALLOWED |
  | `lpOverlapped` | *"must be valid for the duration of the overlapped operation"* — persistence explicitly REQUIRED |
  | `lpTo` | **nothing.** No capture promise, no persistence requirement |

  So `lpTo`'s lifetime is **undefined by the contract**, which is materially worse than either
  answer would be: an implementation may capture it today and not tomorrow, and the failure mode is
  a silent wrong-destination or a read of freed stack. Depending on undefined lifetime at the kernel
  boundary is precisely the hazard §4.3's sub-wall (2) exists to refuse. *Recommendation, now
  evidence-backed rather than cautious:* **stage it** — move the send's sockaddr onto
  `StageOperationBuffer` in this increment. Three lines, the primitive already exists, and it turns
  an unprovable risk into a proven-safe one.

  **The same source corrects §4.3 on a point worth fixing while we are here.** §4.3's sub-wall (2)
  states flatly that *"the kernel retains the OVERLAPPED pointer and the buffer pointers until
  COMPLETION"*, and §4.7 reasoned from it that a stack image for the WSABUF array is *"wrong by
  construction"*. That is right about the DATA buffers and **not** established for the WSABUF ARRAY,
  which the Remarks say is captured before return precisely so that stack-based arrays are legal.
  Nothing shipped is unsafe — §4.7 staged the array anyway, which is the conservative reading and
  the one the corpus keeps — but the design's stated *reason* is stronger than the contract
  supports, and a future author trusting it would over-build. (The parameter list and the Remarks
  disagree with each other on this point; that disagreement is itself the argument for staging.)
* **⟨OQ-H⟩ — scope: all seven sites, or `WSARecvFrom` alone first?** *Recommendation:* **`WSARecvFrom`
  first (three sites), then `AcceptEx`, then `WSARecvMsg`.** `WSARecvFrom` is what ⟨OQ-E⟩'s guard
  needs, `AcceptEx` is what `net.Listen`'s accept path needs and has no guard yet, and `WSARecvMsg`
  has no consumer on the roster at all. The mechanism lands once; the sites follow demand, which is
  the board's standing rule (*fix a censused wrapper when a suite REACHES it, never speculatively*).
* **⟨OQ-I⟩ — what closes ⟨OQ-E⟩ exactly?** *Recommendation:* `UdpLoopbackRoundTrip` registers in the
  `WSARecvFrom` increment and must pass on **both** platforms in the same change, since a
  Linux-only pass is what it already had.
The attribution matters, because the shape at fault is used on BOTH directions and only one fails.
`sockaddrInet4ToRaw` (fd_windows.cs:1277) performs the *identical*
`(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(raw.of(…ᏑPort)))` round-trip in the WRITE direction
and works today — TCP exercises it. So the round-trip is NOT the defect. What differs is the BOX:
the read hands `@new<syscall.RawSockaddrAny>()` — a managed struct whose `Addr`/`Zero`/`Pad` are
managed arrays — straight to the kernel through `WSARecvFrom`, and decodes it afterwards. By this
project's own AV-vs-panic triage rule (golib's `array<T>` indexer is bounds-checked, so a clean panic
means an EMPTY array and an AccessViolation means a CORRUPTED one), an empty array says nothing was
ever materialised at that address. That is the struct-passing class, sixth confirmed instance, on the
DECODE side.

**It is pre-existing, not introduced.** It was simply unreachable: the send stub threw first, so no
Windows program ever completed a UDP read. This is the `LocalTimeZone` pattern exactly — binding a
real implementation exposes the defect the stub was hiding.

**2. ⟨OQ-E⟩ therefore cannot be satisfied yet, and the guard stays out of tree.** Its whole purpose
is to pass on both platforms; with the read broken it cannot. §4.7.4 priced this exact position as
the cost of doing nothing, and the send fix has now moved the guard from *"blocked on the submit"* to
*"blocked on the decode"* — one wall, not two. The source stays parked, Linux-proven, and registers
with the recv increment. **The remedy is NOT a same-lines extension of this change:** the received
address arrives asynchronously, so a native staging buffer must be decoded at HARVEST time, which is
§4.3's decode-side problem (the one `AcceptEx`'s output buffer also has) rather than this section's
submit-side one.

**3. The factory must be registered at ASSEMBLY LOAD, not on first use — measured by failure.**
`syscall` registered its operation factory lazily, from `operationFor`, which only a TCP-shaped
submit reaches. A program that only ever sends datagrams calls nothing in that file, so the first UDP
send failed with `GoAsyncIO: no operation factory registered`. A `[ModuleInitializer]` fixes it and
needs no new mechanism — the corpus already uses module initializers to run Go's package `init()`.

**4. That fix has a cost, and it is paid in the TESTS, not in production.** Registering at assembly
load means `syscall` claims the process-global factory in any process that loads it — including
GolibTests, which references `core/syscall`. Five of the six new primitive tests passed filtered and
failed in the full suite, which is the order-dependence signature: the tests could never win the
registration race against a module initializer. Relaxing the public one-factory rule to make them
pass would have destroyed the property the seam exists to enforce, so the rule stayed strict and the
swap became `internal`, granted to GolibTests through the `InternalsVisibleTo` golib already had.
That is the S2b *replaceable sink* lesson arriving a second time, and it generalises past this seam:
**process-global registration and fake-based tests are in tension, and the honest resolution is an
internal test seam, never a weakened public contract.**

A fifth, cheap and easy to lose (also on the board): a hand-own must not initialise a `LazyProc` in a
**field initializer** that reads a generated sibling's field. C# orders static field initializers
within a type but **not across the files of a partial class**, so `= modws2_32.NewProc("WSASendTo")`
ran while the generated `modws2_32` was still null and the first send died inside `LazyDLL.Load()`.
Defer with `??=`.

## 5. The deadline/unblock story — the hard part, priced honestly

This section is the reason the board said "a deadline/unblock story to settle, not a wrapper
repair." The semantics to reproduce, each read out of `runtime/netpoll.cs`:

1. **`d` is a relative ns duration** (`setDeadlineImpl` computes `time.Until(t)`,
   `fd_poll_runtime.cs:170`): `d > 0` arm; `d == 0` NO deadline (clear); `d < 0` already expired
   (`setDeadlineImpl` normalizes an exactly-now deadline to `-1`, `:171–173`). Go adds `nanotime()`
   and clamps overflow to `int64.max` (`netpoll.cs:395–402`); the managed mirror clamps the
   relative due-time to `Timer`'s ~49.7-day ceiling and re-arms on fire under a generation check —
   an honesty note, not a behavior change (Go's ceiling is ~292 years; both are "never" for a
   socket deadline).
2. **`mode` is `'r'`, `'w'`, or `'r'+'w'`** — the combined form sets BOTH deadlines (`:403–408`).
   Go's single-combo-timer optimization (`:410–414`) is NOT reproduced: two timers with the same
   due time are observationally equivalent, and the combo machinery (rtf selection,
   `netpollDeadline` firing both modes) exists to save a runtime timer, a resource the managed
   side is not short of.
3. **Expiry is STICKY per mode.** On fire, `rd/wd = -1` → the info bit → every subsequent
   `prepare`/`wait` in that mode returns `pollErrTimeout` (`netpolldeadlineimpl` `:656–698`,
   `netpollcheckerr` `:544–546`) until a LATER `SetDeadline` call rewrites that mode's deadline —
   to zero (clears), to the future (re-arms), or to the past (re-expires). Managed: the `expired`
   flag, cleared/re-set on every `pollSetDeadline` for the modes it names. `net`'s
   deadline-dependent tests assert this sticky-until-reset shape; getting it wrong reads as
   "connection permanently broken after one timeout" or as "timeout not sticky" — both
   behaviorally loud.
4. **A deadline set in the past fires NOW, against the CURRENT waiter** (`:448–458`): after
   updating state, wake the blocked mode(s) without setting `ready`. The waiter's loop re-checks
   and returns `pollErrTimeout`; `execIO` then runs the cancellation path. Same wake-without-ready
   rule for `pollUnblock` (`:473–505`).
5. **Stale timer callbacks must be inert.** Go guards with `rseq`/`wseq` — bumped on every
   deadline change and unblock — checked by the fired callback under the pd lock (`:425, 481,
   660–670`). The managed mirror is FORCED into the same shape by .NET semantics:
   `Timer.Change`/`Dispose` do not synchronize with an in-flight callback, so the callback
   re-validates its captured generation under the desc lock and returns if it lost. This is not
   optional hardening; without it, a reset deadline can expire a fresh one.
6. **Expiry does not abandon the in-flight operation.** After a timeout wake, `execIO` still owns
   a kernel-pending overlapped op: it issues `CancelIoEx` and then `waitCanceled` — which waits for
   completion-readiness ONLY, ignoring the very timeout that woke it (`netpollblock(waitio: true)`,
   `:377–382, 604`). Liveness holds because the kernel ALWAYS posts a completion for a cancelled
   overlapped operation (with `ERROR_OPERATION_ABORTED`), and if the op won the race and completed
   first, `CancelIoEx` returns `ERROR_NOT_FOUND` and the completion is already in flight
   (`execIO` handles both, `fd_windows.cs:212–231`). Under mechanism (a) both arrive through the
   CLR callback → `ready` → `waitCanceled` returns. The one configuration that would BREAK
   liveness is an operation whose completion packet was SKIPPED (`skipSyncNotif`) reaching the
   cancel path — it cannot: skip-on-success suppresses the packet only for a submit that returned
   synchronous success, and that path returns from `execIO` before any `wait`
   (`fd_windows.cs:171–177`); every path that CAN reach `wait` — `ERROR_IO_PENDING`, or sync
   success with the notification mode NOT skipped — has a completion packet guaranteed in flight.
   This invariant gets a dedicated test in the S2 matrix.
7. **Check order is fixed**: closing > timeout (> eventErr, unix-only) — `netpollcheckerr`
   `:539–554` — and **readiness-consumption beats both on entry** (`:585–589`): a completion that
   raced the deadline is still delivered to the caller, matching Go's preference for returning
   real IO over a same-instant timeout.

**Why not `CancellationToken`.** The task's framing asks this to be priced explicitly. A
CancellationTokenSource models a one-shot cancellation of a linked operation; Go's deadline is
none of those things — it is (i) per-MODE, not per-operation; (ii) STICKY across future
operations until explicitly re-set (a fired CTS cannot be un-cancelled; a fresh CTS per op loses
the stickiness that lives BETWEEN ops); (iii) REPLACEABLE while an op is in flight
(`Timer.Change` ↔ `seq++`, without disturbing the op); and (iv) NON-ABANDONING — the timed-out op
must still be cancelled-and-HARVESTED by the same caller (point 6), where CTS-style composition
wants to throw/abandon at the wait site. Modeling all four with tokens reconstructs the
flags+timer state machine anyway, plus an allocation per operation and a linked-registration
lifetime problem. The recommendation (**OQ4**) is the direct map: `System.Threading.Timer` +
sticky flags + generations under the desc lock — ~5 state fields and 2 timers per desc, with the
complexity concentrated where it genuinely lives: the interleavings. The S2 gate matrix (§7)
enumerates them adversarially rather than hoping.

**The priced risk.** The state machine is small; the race surface is not: completion vs timeout vs
`CancelIoEx` vs `Close`, times `skipSyncNotif`, times deadline-replaced-mid-wait. The budget for
this arc should assume the deadline matrix — not the happy-path round trip — is where the
iteration goes. The mitigations built into the design: one lock (no lock-free interleavings to
reason about), single-waiter-per-mode (no queue/fairness dimension), generation checks forced by
.NET timer semantics, and the wake-vs-ready separation lifted intact from Go.

## 6. Blast radius — what stays converted, what becomes hand-owned

**Stays converted, byte-for-byte (the point of the seam):**

- `internal/poll/fd_poll_runtime.cs` — every caller of the ten, `convertErr`, the SetDeadline
  plumbing. Untouched.
- `internal/poll/windows/fd_windows.cs` — `execIO`, `FD`, all ~20 exported IO methods, `Init`'s
  `skipSyncNotif` logic. Untouched. (This is the design's central economy: the alternative of
  hand-owning `FD` against `System.Net.Sockets.Socket` rewrites ~1,500 lines of it, forfeits
  `SyscallConn`/`RawConn` fidelity, and diverges every option/half-close/dual-stack behavior
  `net`'s tests measure. Rejected.)
- `internal/poll`'s other files, `net` entire (its one adjacent stub noted below), `os` entire
  (files are never pollable — `pollable: false` at `os/windows/file_windows.cs:83`, so
  `runtimeCtx == 0` short-circuits every pd call, `fd_poll_runtime.cs:57–117`).
- `runtime` entire — `netpoll.cs`, `windows/netpoll_windows.cs` stay converted-and-dead, per the
  doctrine's "becomes unreachable" clause. Zero runtime edits.

> **AMENDED 2026-09-01 — "zero runtime edits" holds for every consumer this design was about, and
> costs exactly ONE function outside them.** The scope above is internal/poll's callers. It does not
> cover `runtime`'s OWN test suite, which reaches the seam directly: `export_test.go` re-exports
> `netpollGenericInit` as `runtime.NetpollGenericInit`, and `netpoll_os_test.go` calls it from a
> package-level `init()`. So the first `-tests` run of `runtime` that got far enough to compile threw
> `getg: external (assembly or cgo) function is not implemented` in the test host's STATIC
> CONSTRUCTOR — `netpollGenericInit → netpollinit → stdcall4 → getg()` — before a single test
> executed, and every verdict came back empty. That is the whole-host mass-empty shape, not a test
> failure, which is why it is worth one narrow exception.
>
> The exception: `netpollGenericInit` joins `manualConversionFuncs["runtime"]` (`goosAny`) with a
> NO-OP body in the new flat `runtime/netpoll_impl.cs`. Blast radius, measured by the two-emission
> control (a baseline-converter emission and a changed-converter emission into two seeded roots,
> diffed against each other rather than against the committed tree): **two files per target** —
> `netpoll.cs` (12-line body → 1 placeholder line) and that target's `package_info.cs` (the ONE
> `GoPositionMap` entry for `runtime/netpoll.go`). Identical footprint on the windows and linux
> targets; the map line is byte-identical across all three.
>
> The no-op is an EQUIVALENCE, not a silencer, and both halves were measured. (1) Nothing reads the
> state it would publish: `netpollinited()` has six call sites and all six are `proc.cs` — the
> scheduler the managed host never enters — and the one read of `netpollInited` outside `netpoll.cs`
> is `time.cs`'s runtime timer heap, equally unreachable. (2) The honest behaviour is delivered by
> GO'S OWN GUARD: all three flavors open `netpoll(delay)` with "the poller object was never created →
> return an empty gList" (`iocphandle == _INVALID_HANDLE_VALUE` / `epfd == -1` / `kq == -1`), so
> declining to create it leaves each flavor in the branch Go wrote for exactly this condition — which
> is true here, since no goroutine is ever parked on the runtime's poller. `netpollInited` is
> deliberately left at zero: setting it would assert "the runtime poller is up" while the handle stays
> invalid, an incoherent pair for a later reader of that dead code.
>
> `netpollBreak` is deliberately NOT hand-owned. Its auto body reaches `stdcall4 → getg` and keeps
> throwing, so `proc_test.go`'s `TestNetpollBreak` fails as one loud, locatable row rather than going
> quietly green — `stubs_impl.cs`'s standing rule for an unported path, and the right outcome for a
> test whose premise (a poller wait a break interrupts) the managed model does not have. `netpoll`
> needs no hand-own at all; guard (2) already answers it.
>
> This also settles the tempting alternative for the record: a real managed `getg()` would NOT have
> fixed this path. `stdcall` bottoms out in `asmcgocall`, itself a bodyless partial with no `_impl`
> body anywhere in the corpus, so a fabricated `g` moves the crash exactly one frame deeper — the
> design's §3.1 "shallow wall" measured one level lower down.

**New hand-owned surface:**

| Artifact | Kind | Census effect |
|---|---|---|
| `internal/poll/windows/runtime_netpoll_impl.cs` — the ten bodies + `ManagedPollDesc` + delivery | new `_impl.cs`, `[module: go.GoManualConversion]`, no `.go` counterpart (never regenerated — the `runtime_sema_impl.cs` shape) | +1 marked file |
| `syscall/windows/zsyscall_windows_wsa_impl.cs` (name per OQ6) — WSA family mirrors + op records | new `_impl.cs` + `manualConversionFuncs` entries under `"syscall"` (`goosWindows`) | +1 marked file; displaced wrappers regenerate as placeholders |
| `internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs` — `WSAGetOverlappedResult` (+S3: `WSARecvMsg`/`WSASendMsg`/`WSASendtoInet4/6`) | same mechanism, new `"internal/syscall/windows"` key in `manualConversionFuncs` | +1 marked file |

Converter change: `manualConversionFuncs` map entries ONLY — data, not logic; no new converter
`.go` file (no `projitems` entry owed); `go test ./...` and CNR classify the one-time regen of the
displaced wrapper files as the change's intended A/B footprint. The hand-own census GROWS —
re-measure at the regen per the ritual, never carry forward (CLAUDE.md, corpus mechanics §1).

> **MEASURED at S2b (2026-08-15): 58 marked files**, up from 56 — the two new `_impl.cs` above.
> Re-measured with the line-anchored scan (`^\s*\[module:\s*(go\.)?GoManualConversion\]`), never
> carried forward: an unanchored `grep GoManualConversion` still over-counts, because `reflect` and
> `internal/reflectlite` MENTION the marker inside placeholder comments. `runtime_netpoll_impl.cs`
> was already counted at S1.
>
> One line of the table above needs correcting against what landed: the design listed `ConnectEx`
> among the wrappers to DISPLACE, and it is not — L10 had already hand-owned it, so it was EXTENDED
> in `syscall_windows_impl.cs` and needed no registry entry (§4.4 predicted exactly this and the
> implementation confirmed it). Two entries the design did NOT anticipate joined instead:
> `GetAcceptExSockaddrs` (in the table, but see §4.5's hole and its ruling) and **`LoadConnectEx`**,
> which was outside the arc's declared surface until it was measured broken (§4.3's correction).

**Adjacent walls this design deliberately does NOT claim** (so nobody reads "netpoll landed" as
"net validates"): `net`'s own `runtime_rand` bodyless stub (`net/dnsclient.cs:20`), the DNS
resolver stack, `GetIfEntry`/`FreeAddrInfoW` (both still on the struct-passing census —
`net.Interfaces`, DNS), and `crypto/x509`'s cert-store members. Each is a later, smaller arc with
this design's machinery as precedent.

## 7. Gates and staged landing

Stage discipline per the repo's standing rules: behavioral guards compare REAL VALUES against
`go run` (the `LocalTimeZone`/`SockaddrRoundTrip` doctrine — never absence-of-fault), outputs
deterministic (ephemeral ports and timings printed as derived invariants, never raw). Each stage
banks separately; a later stage blocked does not un-bank an earlier one.

**S0 — mechanism in place, nothing reaches it.** The three `_impl.cs` files + map entries +
displaced-wrapper regen land compiling. Gates: full behavioral suite (existing 528 outputs
unaffected — nothing exercises sockets today); `check-no-regression` clean EXCEPT the enumerated
displaced-wrapper files (the intended footprint, named in the commit); converter `go test ./...`;
`go2cs.slnx` + `go2cs-stdlib.slnx` build; filtered sweeps over the packages whose closure touches
the displaced files — **`syscall` 62/62 must hold** (the banked row most exposed), plus `os`,
`path/filepath`, `time` spot checks (their syscalls ride the untouched dispatcher, but the sweep
is cheap and the claim should be measured, not argued).

**S1 — `net.Listen` smoke.** New behavioral test `NetListenSmoke`: `Listen("tcp",
"127.0.0.1:0")`, assert/print invariants (addr network, port > 0, distinct second listener,
`Close`, listen-after-close on the same port), byte-compared against `go run`. Exercises contracts
1, 2, 3, 8, 9 with zero data flow. This is the first observable retreat of the wall: the board's
`internal/poll` row stops dying in `pollServerInit`.

**S2 — the round trip + the deadline matrix (the arc's real gate).** Two behavioral tests:

- `TcpLoopbackRoundTrip` — the test L10's spec commissioned and the netpoll wall blocked:
  listen → dial → accept → write → read → echo → close, both directions, IPv4 + IPv6. Exercises
  contracts 4, 5, 6 end-to-end plus `AcceptEx`/`ConnectEx`/`WSARecv`/`WSASend` mirrors.
- `NetDeadlineMatrix` — the §5 interleavings, adversarially: read blocks then deadline fires
  (`os.ErrDeadlineExceeded` surfaced through `net.Conn`); sticky (second read fails instantly);
  cleared by `SetReadDeadline(zero)` then succeeds; deadline-in-past fails without blocking;
  deadline REPLACED while blocked (old never fires, new does); write-mode independence;
  `'r'+'w'` combined; `Close` from another goroutine unblocks a blocked read with the closing
  error; completion-races-deadline delivers the data (point 7); the pending-op cancel/harvest
  invariant (point 6).

> **⚠ BLOCKER (S2b lane, measured 2026-08-14): `TcpLoopbackRoundTrip` is UNREACHABLE until the
> sockaddr DECODE is fixed, and it blocks the seam's most dangerous surface from value-level
> verification.** `net`'s accept path calls `RawSockaddrAny.Sockaddr()` on the `GetAcceptExSockaddrs`
> output (`net/windows/fd_windows.cs:255–256`), and that decode still carries the port ALIAS L10
> hand-owned away on the ENCODE side — `var p = (ж<array<byte>>)(uintptr)(new
> @unsafe.Pointer(pp.of(RawSockaddrInet4.ᏑPort)))` (`syscall/windows/syscall_windows.cs:953`). An
> `array<T>` rebuilt from a raw address materializes length ZERO, so `p[0]` panics. Measured directly
> rather than inferred, with a throwaway probe that constructs an `AF_INET` `RawSockaddrAny` and calls
> `Sockaddr()`: Go answers `decoded AF_INET port=0 addr=[0 0 0 0]`; C# answers
> `panic: runtime error: index out of range [0] with length 0`.
>
> L10 left this auto-converted **deliberately and correctly**: hand-owning it drops the three
> `[assembly: GoImplement<Sockaddr{Inet4,Inet6,Unix}, ΔSockaddr>(Pointer = true)]` records
> (`syscall/windows/package_info.cs:45–47`) that its body's casts are the only witness for, and a
> MEASURED reconvert of `net` against the shortened `package_info` showed `net` minting duplicate
> adapters — the second-identity regression. The real answer L10 named is the converter's POINTER
> method-set recording.
>
> > **UNBLOCKED at the record level (2026-08-14, lane D).** That converter increment has LANDED:
> > `recordSamePackageImplements` now records the POINTER method set as well as the value one, behind
> > the value form's five gates plus a both-sides-EXPORTED gate (a `(Pointer = true)` record is
> > consumed by NAMING the generated adapter, which `ImplementGenerator` scopes `public` only when
> > both participants are). The three `ΔSockaddr` pairs are now sourced from
> > `types.Implements(*T, Sockaddr)` instead of from `(*RawSockaddrAny).Sockaddr`'s casts, and the
> > exact probe that measured the regression now proves its absence: with that method suppressed
> > through `manualConversionFuncs`, `syscall`'s `package_info.cs` still carries all three records and
> > a reconvert of `net` still references `syscall.SockaddrInet4жΔSockaddr` rather than minting its
> > own. **What this does and does not unblock:** hand-owning `RawSockaddrAny.Sockaddr` is now safe
> > from the second-identity consequence, so the port-alias decode defect can be fixed the same way
> > L10 fixed its encode twin. It does NOT by itself fix that defect — the `array<T>`-from-raw-address
> > seam is still there in the auto conversion — so `TcpLoopbackRoundTrip` remains blocked until
> > someone takes the hand-own this increment made available.
>
> **What survives, precisely.** A census of every `.Sockaddr()` call site in `net` finds six: the two
> accept-path sites above, three in `interface_windows.cs` (`net.Interfaces`) and one in
> `dnsconfig_windows.cs` — the last four already named as adjacent walls in §6. **The DIAL path never
> touches it**, because `netFD`'s local/peer addresses come from `Getsockname`/`Getpeername`, which
> L10 hand-owned to decode natively. So a dialed client conn is fully usable (the kernel completes the
> handshake from the listen backlog with no accept on the other end — the shape `SockaddrRoundTrip`
> already relies on), which makes `NetDeadlineMatrix` achievable in full and exercises five of the six
> wrappers; `AcceptEx` is reachable too, but only up to a DEADLINE-cancelled accept, never a
> successful one.
>
> That is deliberately **not** treated as good enough to land the seam on. What no dial-only gate can
> prove is that bytes arrive correctly — the native `WSABUF` mirroring, the pinned user buffer and the
> transferred counts are exactly the "returns garbage without crashing" class the repo's standing rule
> covers: *verify at VALUE level, never at fault level*. A submit seam whose buffer marshalling is
> unproven is the wrong thing to bank, which is why this lane stops at the boundary rather than
> landing ~530 lines against a gate it knows it cannot meet.
>
> > **CLOSED 2026-08-14 (lane S2b, second attempt).** The decode is hand-owned:
> > `RawSockaddrAny.Sockaddr` joins `manualConversionFuncs["syscall"]` and
> > `syscall/windows/syscall_windows_impl.cs` flattens the managed struct back to its 116-byte native
> > image for the `readNativeSockaddr` the encoders already share. The record-survival witness this
> > note demanded was re-measured on the lane's own build, not inherited: with the body displaced,
> > `syscall`'s `package_info.cs` still carries all three `(Pointer = true)` records and a seeded
> > reconvert of `net` still references `syscall.Sockaddr{Inet4,Inet6,Unix}жΔSockaddr` at all seven
> > sites with zero local adapters — the whole A/B footprint being ONE file, `syscall_windows.cs`.
> > Guarded at value level by four new `SockaddrRoundTrip` lines (IPv4; an IPv6 address deliberately
> > crossing the `Addr.Data`/`Pad` boundary; AF_UNIX; an unknown family answering `EAFNOSUPPORT`),
> > byte-identical to `go run`.
> >
> > **`TcpLoopbackRoundTrip` remains blocked, on a DIFFERENT wall.** The decode was the accept path's
> > first obstacle, not its last: §4.5's `GetAcceptExSockaddrs` finding is the one that now holds it,
> > and it wants a ruling before the displacement can be written. What this closure buys is that the
> > blocker is no longer a *converter capability* question — it is a plumbing choice with two costed
> > shapes.
> >
> > > **BLOCKER FULLY CLOSED, S2 MET (2026-08-15, lane netpoll-s2b).** Both gate programs are banked
> > > and byte-identical to `go run`:
> > >
> > > - **`TcpLoopbackRoundTrip`** — listen → dial → accept → 64 KiB write → echo → read → close, on
> > >   IPv4 **and** IPv6, with the payload compared by CHECKSUM (not length), the accepted conn's
> > >   remote address compared against the client's own local address (which is what proves the
> > >   `AcceptEx` staging → `GetAcceptExSockaddrs` → `RawSockaddrAny.Sockaddr` transcription, and
> > >   cannot be faked — the port is ephemeral), plus close-breaks-a-blocked-READ and
> > >   close-breaks-a-blocked-WRITE.
> > > - **`NetDeadlineMatrix`** — eleven §5 interleavings, all matching Go on the FIRST run of the
> > >   converted host, and stable across repeats.
> > >
> > > One §5 expectation was corrected by measuring Go rather than trusting the write-up: an
> > > ALREADY-EXPIRED mode fails a read even when bytes are sitting in the socket buffer, because
> > > `execIO` calls `pd.prepare()` before it submits anything and prepare's order is
> > > closing > timeout > clear-readiness. §5 point 7's "readiness beats both" is about `pollWait`,
> > > the post-submit path — not about `prepare`. The guard now asserts both halves, in the
> > > directions Go actually takes them.

Gates: full behavioral suite; then the pipeline's own measure — **filtered sweep of
`internal/poll`: the board row's target is 19/19** (from 18/19, sole miss `runtime_pollServerInit`
— the row this design exists to close).

> **⚠ FINDING (implementation lane, 2026-08-13, measured at S1): that gate has an unrecorded
> PREREQUISITE — `internal/poll`'s converted test host does not currently BUILD, so the row is not
> measurable at any value, before or after this arc.** Measuring it at S1 (regression diligence: S1
> lets `TestWSASocketConflict` run past the `fd.Init` that used to kill it, so "further than it has
> ever run" is where a new hang would live) produced a C# compile error rather than a verdict:
> ```
> export_test.cs(13,62): error CS0123: No overload for 'consume' matches
>                        delegate 'Action<ж<slice<slice<byte>>>, long>'
> ```
> `export_test.go`'s `var Consume = consume` is a func VALUE of a function whose `*[][]byte`
> parameter the production emission now lowers to a C# `ref` (`consume(ref slice<slice<byte>> v,
> int64 n)`, `fd.cs:92`), while the test file still spells the delegate in the `ж<T>` box form. A
> `ref`-taking method cannot bind to that delegate. Both files are ordinary converted output that
> this arc does not touch — the netpoll hand-own supplies partial BODIES for the ten `runtime_poll*`
> methods and cannot alter `consume`'s signature — so this is the ref-lowering arc meeting
> func-value conversion, and it is **not this arc's to fix** (recorded and handed to the coordinator
> rather than reached across, per OQ6's ownership line). The board's "18 of 19" reading therefore
> predates the ref-lowering landing and should be treated as stale until the host compiles again.
>
> **ATTRIBUTED AND CHARTERED ELSEWHERE (coordinator, 2026-08-14).** This CS0123 is the first real
> corpus witness of the ж-box arc's §3.5 **func-value adapter gap** — a Go func VALUE aliasing a
> ref-lowered function — which is precisely the evidence class A3 recorded as missing. It gets its
> own lane, and this note is where that lane starts. The netpoll arc does not fix it and does not
> wait for it: per the same ruling, S2 runs on its other gates and reports this row as
> **blocked-with-cause**, citing this error. When the lane lands, whoever re-measures should
> re-derive the target rather than expecting 19/19 — see the COM4 host-dependence below.
>
> Two facts worth carrying to whoever picks it up. The Go side of the same run is itself **not
> clean on every host**: `TestSerialFdsAreInitialised/COM4` fails wherever a real COM4 exists, so
> the differential's Go baseline is host-dependent and the target may not be a round 19/19 —
> re-derive it at measure time rather than treating a non-round result as a miss.
>
> The other fact this measurement was meant to probe — the `FD.WSAIoctl` foreign-overlapped hazard —
> is **closed structurally at S2a and no longer needs probing**: the CLR association moved from
> `pollOpen` to the first submit (§4.2's amendment), so a socket that only ever sees foreign
> overlapped IO is never bound at all. That is why the unmeasurable row costs this arc nothing it
> cannot recover: the one thing the row was uniquely positioned to catch has been removed rather than
> merely watched for.

**S3 — consumer re-measures (the board rows behind the netpoll wall).** The RESOLVED note freezes
the walled set and this design inherits it as its unlock ledger: `net/smtp` (9/14, five rows on
this exact stack — the first re-measure, per the L10 spec's consumer-proof pattern), then ONE of
the L9-held socket rows (`net/http/httptest` recommended there), then the remainder as breadth
lanes: `net/http/cgi` (36/39), `net/http/httputil`, `net/http/cookiejar`, `net/rpc`. UDP wrappers
land here gated by whichever suite reaches them first. `net` itself stays a FUTURE arc — its
suite needs the §6 adjacent walls (DNS, interfaces, `runtime_rand`) and its census should be
taken fresh when this machinery exists, not promised now.

## 8. Non-goals

- **No Linux/darwin poller.** The Linux corpus does not yet build (`DESIGN-multiplatform-corpus.md`
  §12), and `fd_unix.go` consumes these contracts in a READINESS model (wait-then-retry-syscall)
  that wants a different managed mechanism (epoll has no CLR surface; candidates are
  `Socket.Poll`-shaped or a native epoll thread — a separate design when it is real). The ten
  bodies land in `windows/`; other GOOS keep today's throwing stubs.
- **No scheduler-facing netpoll surface** — `netpoll(delta)`, `netpollBreak`, `netpollready`,
  `netpollAnyWaiters` have no caller in managed land and are not implemented (§3.2).
- **No performance targets.** Correctness-first per Phase-4 doctrine; the design avoids known
  cliffs (PreAllocatedOverlapped reuse, zero-copy pinned user buffers, no per-op allocation on
  the happy path) but no benchmark gates this arc. A perf pass belongs after the ж-box arc's
  instruments, if ever.
- **No `net` operational campaign** — §6's adjacent-walls list is the boundary of the claim.
- **No os-file async IO** — files stay non-pollable, exactly as Go 1.23 has them on Windows.

## 9. Open questions — RULED (coordinator, 2026-08-13)

> **All eight recommendations are RATIFIED as written.** The arc is chartered; §4.2's
> `ThreadPoolBoundHandle` plumbing, §4.3.3's displacement mechanism and staging, §5's deadline
> semantics list, and §6's file placement are now the implementation contract. One clause stays
> live by design: OQ3's UDP-fold — if the S2 sweep shows UDP-shaped `internal/poll` tests failing
> on the stubs, UDP folds into S2 without re-ruling. Each item below retains its original
> recommendation text as the record of what was ratified and why.

- **OQ1 — Completion delivery mechanism** (§4.2): `ThreadPoolBoundHandle` (recommended) vs own
  IOCP + poller thread. The contract layer is identical under both; ruling picks the plumbing and
  the `isPollServerDescriptor` answer (`false` vs real handle).
- **OQ2 — Wrapper displacement mechanism** (§4.3.3): `manualConversionFuncs` entries + `_impl.cs`
  (recommended, the sockaddr precedent) vs whole-file hand-own of the generated `zsyscall`
  files (rejected here: freezing ~1,900 generated lines to own ~10 functions is rot the marker
  census then has to carry forever).
- **OQ3 — Stage scope of the WSA family** (§4.3.3 table): TCP-core seven wrappers for S1–S2, UDP
  five at S3, `TransmitFile` deferred-until-reached. Confirm the split — it is the
  do-when-reached ruling applied, but it leaves `ReadFrom`/`WriteTo` stubbed for one stage on a
  package (`internal/poll`) whose sweep S2 gates, so the S2 19/19 target implicitly assumes the
  suite's UDP-shaped tests are either absent or already-failing-for-other-reasons; if the S2
  sweep says otherwise, UDP folds into S2.
- **OQ4 — Deadline machinery** (§5): `System.Threading.Timer` + sticky flags + generations under
  the desc lock (recommended) vs CancellationToken composition (rejected with the four-point
  analysis). Ratifying this ratifies the §5 semantics list as the implementation contract.
- **OQ5 — `skipSyncNotif`** (§4.2): keep Go's skip-completion-on-success (recommended;
  Go-faithful, and the record lifecycle handles the no-callback retire) vs suppressing the mode
  for a smaller state space at a small syscall-per-op cost. A defect surfacing in the S2 matrix
  on the skip path is grounds to flip this without re-ruling the design.
- **OQ6 — File placement and naming** (§6 table): the three `_impl.cs` names/locations as
  proposed; specifically whether the poller core belongs in `internal/poll/windows/` (recommended
  — it is package-private contract surface, per-GOOS by nature) or in golib (rejected: golib is
  Go-semantics-generic; a Windows IOCP poller is neither).
- **OQ7 — Gate ownership at S3**: which consumer re-measures belong to THIS arc's bank versus
  fresh breadth lanes. Recommendation: this arc banks through the S2 gates + `internal/poll`
  19/19 + the `net/smtp` re-measure as its consumer proof; everything further is L-lane work
  against the then-current board.
- **OQ8 — The adjacent `net.runtime_rand` stub** (`net/dnsclient.cs:20`): flagged for the future
  `net` arc, one `runtimeNano`-class hand-own; NOT taken here. Confirm it stays out of scope so
  the S3 `net/smtp` re-measure attributes any DNS-path miss to the right wall.
