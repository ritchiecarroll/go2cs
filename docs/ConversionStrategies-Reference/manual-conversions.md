# Manually-Converted Declarations

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#manually-converted-declarations)

Some Go declarations cannot be faithfully auto-converted because their semantics depend on hiding a managed pointer inside an integer. The canonical family is runtime's `guintptr`/`puintptr`/`muintptr` (`type guintptr uintptr` holding a `*g` the Go GC must not see): the CLR has the *opposite* constraint — a managed reference stored as a number is invisible to the .NET GC, so the referent can be collected or moved and the number is garbage. The managed conversion stores the `ж<T>` box **directly** and the numeric form never exists (model precedent: `core/sync/atomic`'s hand-rewritten `Pointer<T>`).

Two mechanisms deliver this, chosen by granularity:

* **Whole-file** (pre-existing): a hand-finished file marked `[module: GoManualConversion]` is never overwritten by the converter when it exists in place (`containsManualConversionMarker`), and is restored over auto output by the overlay on fresh (unseeded) reconversions. Right when the whole file is hand-owned (sync/atomic `type.cs`). The marked file's Go source is NOT dropped from conversion (2026-07-17 fix): it is still analyzed and visited with its package — its anonymous-struct lifts, package-var registrations, and other package-wide state must keep feeding the package's sibling files, or those emit corrupted (raw Go `struct{…}` text in selectors, package-var assignments re-declared as shadowing locals) — with emission redirected to a non-compiled `<name>.cs.auto` review sibling. Guarded by the `ManualConversionSiblingState` behavioral test.
* **Type-level** (`go2cs/manualTypeOperations.go`): the `manualConversionTypes`/`manualConversionFuncs` registry (keyed by package path and raw Go names) makes the converter skip emitting the listed **type declarations**, every **method on those types**, listed **adjacent free functions** (`setGNoWB`), and **`GoImplicitConv` assembly attributes** referencing the types — each replaced by a marker comment pointing at the package's `*_impl.cs`. Right when the types live in a large file (runtime2.go) that must otherwise keep receiving converter improvements. Each `manualConversionFuncs` entry also carries a **platform scope** — see *Hand-owns have a platform* below.

The hand implementation (`src/core/<pkg>/<file>_impl.cs`, e.g. `core/runtime/runtime2_impl.cs`) declares the same type/extension surface the auto call sites bind: value-receiver methods as `this T` extensions, pointer-receiver methods as `[GoRecv] this ref T`, and the conversion operators call sites need. For the guintptr family that surface is: `.ptr()` returns the stored box, `.set()` stores it, `.cas()` is a real `Interlocked.CompareExchange` on the reference slot (the Go original's `atomic.Casuintptr` maps to a throwing asm stub — the managed model makes it *work*), `== 0`/`= 0` bind zero-comparison/nil operators, and numeric escapes are deliberate and loud: converting a non-zero integer **panics** (a number can never faithfully become a managed reference), and converting *to* a number (print/`hex` diagnostics) yields a stable object-identity hash — an opaque token, never an address.

One call-site emission cooperates (`convCallExpr.go`): a conversion **to** a manual type from an `unsafe.Pointer` — `guintptr(unsafe.Pointer(newg))` — unwraps the inner conversion and emits the referent-preserving ctor form `new Δguintptr(newg)` instead of the numeric cast chain `(Δguintptr)(uintptr)new @unsafe.Pointer(newg)`, which would lose the referent at the `(uintptr)` hop.

**The runtime lock/note model (`core/runtime/lock_managed_impl.cs`).** Go's `mutex.key` is a tagged atomic slot — 0 unlocked, `locked` (1) held, or an `*m` address|locked heading a waiter chain through `m.nextwaitm`, parked on OS semaphores. The managed model hand-owns `mutexContended`/`lock2`/`unlock2`/`notewakeup`/`notesleep`/`notetsleep_internal` (via the same registry; thin wrappers and consts stay auto) and keeps the **same key protocol restricted to `{0, keyLocked}`**: the mutex is an `Interlocked` spinlock on the real `key` storage with `SpinWait` escalation standing in for the spin→yield→park ladder; the note is a signaled/clear latch (double-wakeup throw preserved; timeout at millisecond granularity). Deliberately not modeled, documented in place: the waiter queue (fairness), lock profiling, and the `m.locks`/preempt bookkeeping — `getg()` is a Go compiler intrinsic with no managed realization yet (a `[ThreadStatic]` g/m model is the future root that unlocks runtime-operational semantics; the bookkeeping returns to these bodies when it lands).

Go actually has **two** flavors of that protocol and picks one per GOOS: `lock_sema.go` (windows, darwin, plan9, aix …) parks on OS semaphores as described above, and `lock_futex.go` (linux, freebsd, dragonfly) uses a `{0,1,2}` slot and parks on a futex. Neither primitive survives conversion, so both collapse onto the identical managed answer — which is why the core above is **one flat, platform-neutral file** and not a copy per flavor. What genuinely differs is a single signature: `notetsleep_internal` is `(n, ns, gp, deadline)` in `lock_sema.go` and `(n, ns)` in `lock_futex.go`. Each flavor keeps only that declaration, four lines delegating to the shared `noteSleepDeadline`, in `runtime/{windows,darwin}/lock_sema_impl.cs` and `runtime/linux/lock_futex_impl.cs`. `keyLocked` is the managed spelling of the value Go calls `locked` on one flavor and `mutex_locked` on the other — both 1, and neither name is declared on the other flavor's platforms.

## The managed netpoller — the ten `runtime_poll*` contracts on .NET's completion machinery

**The fifth and deepest application of the managed-API-boundary pattern, and the one the sockaddr entry above predicted.** `internal/poll` declares **ten** bodyless `//go:linkname` entry points into the runtime's network poller (`fd_poll_runtime.cs:18–36`). The converter emits each as a bodyless `partial`, the `PartialStubGenerator` fills them with throwing stubs, and the first pollable `FD.Init` — which is *every socket the `net` package creates* — died in `serverInit.Do(runtime_pollServerInit)`. `os` is unaffected and always was: it passes `pollable: false` for every file, pipe and console, so `runtimeCtx == 0` short-circuits every pd call.

**Why the counterparts could not simply be wired.** They exist — `runtime/netpoll.cs:217` carries `poll_runtime_pollServerInit` with its linkname comment intact, and all nine others sit beside it — but the SHALLOW wall (`netpollinit` → `stdcall4(_CreateIoCompletionPort, …)` → `asmstdcall`, a stub) is not the real one. Behind it the bodies consume runtime mutexes with lock-rank bookkeeping, `pollcache` over `persistentalloc`, the runtime timer engine, `gopark` with a commit callback, `goready`, and g-pointer CAS protocols — every one an organ of the Go scheduler. The decisive fact is that Go's poller is only **half** an API: the other half (`netpoll(delta)`, `netpollBreak`, `netpollready`) is called by the scheduler itself from `findrunnable` and sysmon. Under go2cs nothing would ever pump it, so a perfectly-wired conversion would initialize an IOCP and then **block forever** — the thread Go dedicates to draining it IS the scheduler. The ten-contract boundary is the only cut through this subsystem that does not drag a scheduler across; `runtime/netpoll.cs` and `runtime/windows/netpoll_windows.cs` stay converted and **dead**, with zero runtime edits.

**The shape.** `internal/poll/windows/runtime_netpoll_impl.cs` (per-GOOS folder, riding the existing `$(GoTargetOS)/*.cs` glob — **no csproj change**) supplies the ten bodies over a `ManagedPollDesc`: one Monitor, a sticky `closing`, and per mode a `ready` flag, a sticky `expired` flag, a generation counter and a `System.Threading.Timer`. Four decisions are worth cribbing:

- **One Monitor, not Go's lock-free `rg`/`wg` CAS protocol.** Go splits `pollDesc` state across atomics because `netpollcheckerr` runs where the pd lock cannot be taken and `netpollblock` parks through `gopark`'s publication protocol. Neither constraint survives the boundary — every managed caller is an ordinary blocked thread — and the single-waiter-per-mode contract (*"pollDesc can hold only a single waiting goroutine for each mode"*, enforced above by `fdMutex`) bounds contention to one reader + one writer + one timer callback. The CAS choreography would be emulation of a mechanism whose reason evaporated, the same reduction `runtime_sema_impl.cs` made for the runtime semaphore.
- **Completion delivery is `ThreadPoolBoundHandle` — the CLR's own IOCP**, so there is no poller thread of ours, no shutdown story, and no raw-address routing table. `isPollServerDescriptor` therefore answers **false** for every fd: there is no exposed poll-server descriptor, and its only consumer is the test-only `IsPollDescriptor`. The association is made **lazily, at the first submit** — not at `pollOpen` — and that placement is load-bearing rather than incidental. Go's poller *rejects* completions it does not own (`pollOperationFromOverlappedEntry` compares the completion key against the `pollDesc` recorded in the operation and drops a mismatch, go.dev/issue/58870); `ThreadPoolBoundHandle` has no equivalent, because its callback resolves state *from* the `NativeOverlapped` it allocated, so a foreign overlapped is **misread rather than ignored**. Binding at open would make every pollable socket eligible for that, and `internal/poll` has a live producer: `FD.WSAIoctl` bypasses `execIO` and hands the kernel the *caller's* `syscall.Overlapped` — which, being an all-scalar struct in a standard box, genuinely pins and genuinely reaches the kernel. Binding at first submit removes the hazard structurally: a socket that only ever sees foreign overlapped IO is never associated at all. (Both production `FD.WSAIoctl` callers pass a `nil` overlapped, so no corpus path mixes the two on one socket.)
- **The load-bearing separation is wake-vs-readiness.** A completion sets `ready` and pulses; a deadline firing and `pollUnblock` pulse **without** setting it. Go encodes this as `pdReady`-vs-`pdNil`-wake, and it is what makes `waitCanceled`'s ignore-errors loop correct — that loop waits for a COMPLETION specifically, ignoring the very timeout that woke its caller. Readiness is also consumed *before* either error check, so a completion that raced the deadline is still delivered.
- **Deadlines are `Timer` + sticky flags + generations, not `CancellationToken`.** A CTS models a one-shot cancellation of a linked operation; a Go deadline is per-MODE, STICKY across future operations until re-set, REPLACEABLE while an op is in flight, and NON-ABANDONING (the timed-out op must still be cancelled *and harvested* by the same caller). Modeling all four with tokens reconstructs the flags+timer state machine anyway. The generation check is **not** optional hardening: `Timer.Change`/`Dispose` do not synchronize with an in-flight callback, so without it a cleared deadline can be expired by the callback of the deadline it replaced.

**Landed in stages.** S1 covers the listener lifecycle — contracts 1 (`pollServerInit`), 2 (`pollOpen`), 8 (`pollUnblock`), 3 (`pollClose`), plus a `pollSetDeadline` smoke — with **zero data flow**, guarded by the `NetListenSmoke` behavioral **output** test, which prints kernel-derived values rather than checking for absence of a fault (an ephemeral port was assigned, two live listeners differ, and, the strongest line, the port a closed listener released can be re-bound — only true if `pollClose` released the registration before `internal/poll` closed the socket; port numbers themselves are never printed, so the output is host-independent). S2 is the overlapped SUBMIT seam, below. Full design, and the eight ruled open questions: [`docs/phase4/DESIGN-netpoll-managed-poller.md`](../phase4/DESIGN-netpoll-managed-poller.md).

## The overlapped SUBMIT seam — a per-operation record owning native lifetime, and a golib rendezvous

Making `pollWait` wake up is half the netpoller arc. The other half is that the overlapped submissions `execIO` issues must actually reach the kernel and complete into memory the managed side can read — and three separate walls stand in the way. The hand-owns are `syscall/windows/zsyscall_windows_wsa_impl.cs` (`WSARecv`, `WSASend`, `AcceptEx`, `GetAcceptExSockaddrs`, `CancelIoEx`, `LoadConnectEx`; `ConnectEx` extended in `syscall_windows_impl.cs`) and `internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs` (`WSAGetOverlappedResult`), all displaced through `manualConversionFuncs`.

- **The native-layout wall, with async's twist.** `syscall.WSABuf` is `{uint32 Len; ж<byte> Buf}` — a managed reference where native `WSABUF` wants a `CHAR*` — so it takes the established blittable-mirror remedy. What async adds is that the mirror cannot be *a local at the call site*: the kernel retains the OVERLAPPED and the buffer pointers until COMPLETION, which is unbounded. `&o.o` is an interior field inside a reference-bearing container, which `ж.cs` states plainly it cannot hold still ("is left exactly as it was — a transient address") — the pipe-EOF defect with an unbounded window. And the OVERLAPPED doubles as the operation's kernel-side IDENTITY: `execIO` names the same `&o.o` at three call sites (submit, `CancelIoEx`, `WSAGetOverlappedResult`) and cancellation matches BY ADDRESS, so a fresh native copy per call would break cancellation outright.
- **The remedy is a per-operation RECORD keyed by `ж<Overlapped>`**, owning the `PreAllocatedOverlapped`/`NativeOverlapped`, the native `WSABUF` array or `AcceptEx` staging block, and — crucially — the `ж<byte>` **boxes** whose `GCHandle`s pin the caller's buffers (holding the address is not enough; the pin dies with the box). The key works because `ж<T>` equality for a struct-field reference is *(resolved source pointer, field identity)* and `SameSource` resolves an `of()` chain **recursively**, so both the three call sites of one `execIO` *and* two separate `FD.Read` calls on one FD resolve to ONE record. The bound worth carrying: equality is SOURCE-POINTER identity, so a future call site that re-BOXED the operation would silently mint a second record.
- **The overlapped is freed at the NEXT submit, never in the completion callback.** `execIO` harvests *after* its wait returns, and `WSAGetOverlappedResult` reads `Internal`/`InternalHigh` out of the very block the kernel wrote. Free-then-allocate at each rearm is also what `PreAllocatedOverlapped` requires (one outstanding allocation) and what covers `skipSyncNotif`, where a synchronously successful submit posts no packet and no callback ever runs.
- **The reference graph forces a PUSH through golib** (`core/golib/GoAsyncIO.cs`). `internal/poll` references `syscall`, so a completion callback cannot call back into the poller, and Go's own trick — reading the enclosing `operation` back out of the OVERLAPPED — is impossible because a `ж<T>` field reference does not expose the object it was taken from. The rendezvous is deliberately platform-neutral: a descriptor keys a readiness sink `Action<nint mode>`, plus opaque per-descriptor and per-operation state slots, plus one property (`IGoAsyncOperation.NativeAddress`) for the package that harvests. **Create-exactly-once must not be a bare `ConcurrentDictionary.GetOrAdd`** — its factory runs on several threads and one result is kept, which here means orphaned native allocations and, worse, one socket associated with a completion port twice; `Lazy<T>` with `ExecutionAndPublication` is the shape that holds. The sink must be REPLACEABLE (the kernel reissues descriptor numbers after close) and a signal for an unregistered descriptor a silent no-op (it runs on an IO thread, where an escaping exception ends the process).
- **The harvest calls the REAL OS routine, not the callback's result.** A completion callback's `errorCode` is a **Win32** code, while `execIO` and `net` branch on **WSA** ones (`ERROR_NETNAME_DELETED` where the suites expect `WSAECONNRESET`; `WSAEMSGSIZE`/`ERROR_MORE_DATA` are load-bearing in `execIO`'s own harvest branch). Asking Windows keeps the namespaces from being confused and narrows the cross-package contract to the operation's native address.
- **`GetAcceptExSockaddrs` carries no identity at all** — no handle, no overlapped, just the caller's buffer, which under go2cs is an unpinned reinterpret over a managed array (unusable as data *and* unusable as a key, its identity being physical). So `AcceptEx` parks its native staging block in a slot keyed by `golib.Goroutine.Current` and `GetAcceptExSockaddrs` consumes it. The premise is goroutine affinity — the corpus's only caller, `net.netFD.accept`, does accept-then-parse on one goroutine — and it is stated in the impl header rather than left implicit. Parking over a *different* record throws, naming both wrappers; re-parking the SAME record is a replace, because `FD.Accept` legitimately re-submits `fd.rop` after a `WSAECONNRESET`.
- **⚠ A hand-own writing a POINTER out-parameter needs `!= nil` and `ValueSlot`, not `IsNull` and `Value`.** `GetAcceptExSockaddrs` writes `**RawSockaddrAny`, i.e. a `ж<ж<RawSockaddrAny>>` whose held value is legitimately `nil` on entry. `IsNull` VALUE-PEEKS, so a `!Ꮡp.IsNull` guard reads false-nil and skips the write; `Value`'s nil check consults the same predicate, so the write itself panics. `Ꮡp != nil` is the pointer-identity predicate (what the converter's own emission uses) and `ValueSlot` is the documented deref for a reference-typed pointee.
- **`LoadConnectEx` is hand-owned for the ordinary struct-passing reason, not an async one**, and it is why *every* dial failed with `failed to find ConnectEx: An invalid argument was supplied`. `syscall.GUID` holds `Data4 [8]byte` as a managed `array<byte>`, so `ᏑWSAID_CONNECTEX.Reinterpret<GUID, byte>()` hands `WSAIoctl` a CLR auto-layout image with an object reference in it and Windows answers `WSAEINVAL` deterministically; the OUT buffer is an interior field of a struct holding an `error`, hence unpinnable. Both ends take the package's stack-mirror pattern, with the GUID value still read from the converted declaration.

Guarded at VALUE level by two behavioral output tests: `TcpLoopbackRoundTrip` (listen → dial → accept → 64 KiB echo → close on IPv4 and IPv6, payload compared by checksum, the accepted conn's remote address compared against the client's own local address — which is what proves the accept transcription and cannot be faked, the port being ephemeral — plus close-breaks-a-blocked-read and close-breaks-a-blocked-write) and `NetDeadlineMatrix` (eleven deadline interleavings). Neither prints a port, a timing or an error string, so both are host-independent. `GoAsyncIO` itself carries 9 GolibTests, two of them real contention tests for the create-exactly-once requirement.

## The Linux flavor's poller — the fallback first, then the readiness poller: epoll, one drain thread, and the Windows descriptor state machine

**The same ten contracts, the opposite answer, for the platform whose `os` ASKS.** The Windows netpoller above exists for sockets; its `os` never reaches `pollDesc.init` (every file, pipe and console is `pollable: false`). Linux's `os` does: `newFile` marks every `OpenFile`, `Pipe` and socket descriptor pollable (`os/linux/file_unix.cs:167`), sets `O_NONBLOCK`, and calls `FD.Init(…, true)` → `pollDesc.init` → `serverInit.Do(runtime_pollServerInit)` → `runtime_pollOpen`. With only `windows/` carrying bodies, the `PartialStubGenerator`'s throwing stubs ended the first `os.Open` of every Linux test run — the 2026-08-21 census measured **61 of the 67** Linux-residual roster rows dying on exactly that stack (`System.NotImplementedException: runtime_pollServerInit … at sync.Once.Do → poll.init → os.newFile → os.Open`) while the same rows validate on Windows.

**Go's own fallback is the contract.** `runtime.netpollopen` on Linux is one `epoll_ctl(EPOLL_CTL_ADD)`; for a regular file or a directory the kernel answers `EPERM`, `pollOpen` returns `(0, EPERM)`, `pollDesc.init` returns `errnoErr(EPERM)`, `FD.Init` sets `isBlocking = 1` and hands the error back, and `newFile` restores the blocking mode it had set and carries on — `file_unix.go`'s own comment: *"a file descriptor that is not supported by epoll/kqueue; for example, disk files on Linux systems."* From then on `runtimeCtx == 0` and every converted `pd` call short-circuits: `pollable()` false, `prepare` nil, `evict`/`waitCanceled`/`close` no-ops, `wait` unreachable (a blocking `read(2)` returns data or EOF, never `EAGAIN`), `SetDeadline` → `ErrNoDeadline`. `internal/poll/linux/runtime_netpoll_impl.cs` (per-GOOS, `[module: go.GoManualConversion]`, riding the same `$(GoTargetOS)/*.cs` glob — no csproj change, no os/poll edits, the Windows flavor untouched) answers *un-armable* for **every** descriptor: `runtime_pollServerInit` is a no-op that keeps the Windows flavor's sequencing flag; `runtime_pollOpen` returns `(0, EPERM)`, bound from the converted `syscall` package; the six ctx-taking bodies throw `InvalidOperationException` as unreachable-by-construction (no ctx is ever minted and every caller guards on it — a code returned from a body that cannot legitimately run would be a lie); `runtime_isPollServerDescriptor` is false (no epoll instance exists, and `os/exec`'s `TestMain` asks it of every open fd); `runtimeNano` is the Windows shape verbatim, owed on every GOOS because the flat file declares it. The L3 corpus guard admits the placement (no principal `runtime_netpoll.cs` exists and `netpoll` is not a GOOS suffix), and the multi-platform merge cannot refuse the two divergent per-GOOS copies, because a placement is only ever resolved for a companion whose principal was emitted.

**What degrades, stated.** Go arms pipes, FIFOs, ttys and sockets; this flavor cannot yet — there is no readiness poller (`epoll` has no CLR surface, and `fd_unix.go`'s wait-then-retry-syscall consumer wants a different managed mechanism from the Windows completion model: `DESIGN-netpoll-managed-poller.md` §8's explicit non-goal, a separate design) — so those descriptors take the blocking path with Go's own contract for files that do not support `SetDeadline`: a read blocks its goroutine's thread, `SetDeadline` returns `ErrNoDeadline`, and `Close` does not cancel an in-flight I/O (`os.File.Close`: *"On files that support SetDeadline, any pending I/O operations will be canceled"* — these do not). Two visible consequences, named rather than discovered: `os.Pipe`'s read end does not unblock a blocked `Read` on `Close` — the read returns when the write end closes (the `PipeCloseUnblocksRead` behavioral test names the shape; before this file it faulted at the stub; measured 2026-08-22 on the Linux flavor: Go prints `read unblocked: read |0: file already closed`, the converted program prints `read did NOT unblock` and exits cleanly once the writer closes); and a `net` `Dial`/`Listen` — once the socket path reaches `FD.Init` at all — returns `operation not permitted` (`netFD.init` propagates the error) rather than a `NotImplementedException`; measured on the Linux roster re-run, today's socket path dies EARLIER, in `SockaddrInet4.sockaddr()`'s `(*[2]byte)(unsafe.Pointer(&sa.raw.Port))` uintptr round-trip at `syscall.Bind`/`Connect` (the L10 sockaddr seam, whose Windows remedy is `syscall/windows/syscall_windows_impl.cs`, not yet mirrored into `syscall/linux/`), so the EPERM arm is not yet reachable from `net` — an honest error in waiting, until the socket half of Linux lands. The errno is deliberately not discriminated by `fstat`: every descriptor is un-armable here for one reason, `EPERM` is the errno Go's poller produces for the class being modeled, it is invisible on every `os` path (`newFile` tests `pollErr != nil` and discards the value), and a syscall per open would buy nothing. darwin inherits the same seam (its `newFile` keeps regular files and directories out of kqueue by `fstat`, but pipes and sockets reach the stubs) and the same remedy, file for file; that corpus does not build today, so no unmeasured copy ships.

**Measured (2026-08-22, the Linux roster re-run at `19b63567d`, Windows control on the i9 62/62 green):** of the census's 61 W1 rows, **28 flip to PASS** at their banked counts — every one of them had died at `runtime_pollServerInit` on its first fixture open — and the 33 that do not are attributed, row by row, to five further seams that sit BEHIND the poller, none of them this file: `syscall.Stat_t` passed by address as a non-blittable struct (every `Stat`/`Lstat`/`Fstat` misreads — 8 rows), the Linux process-launch wall reached through `testenv.MustHaveGoBuild` and the gc importer's `go list` (16), the still-stubbed `syscall.rawSyscallNoError` behind `Getuid`/`Getpid` (3), the un-mirrored L10 sockaddr seam at `syscall.Bind`/`Connect` (2: `encoding/json`'s one HTTP test, `crypto/tls`'s whole suite), the page-boundary mmap sub-class (2), the PE-not-ELF self-binary (1), and `mime`'s per-OS count (functionally validated at 18). The board entry of 2026-08-22 carries the per-row table and the pricing; the honest reading is that the poll seam was the FIRST wall on those rows, not the only one.


**SUPERSEDED IN PLACE, SAME FILE (2026-08-22, `claude/linux-poller-impl`, design `DESIGN-linux-readiness-poller.md`, RATIFIED).** The fallback above was the honest answer while `internal/poll` had no readiness mechanism; the poller is the answer now, and it lives in the same hand-owned file. `pollOpen`'s EPERM stops being a constant and becomes the KERNEL's — `epoll_ctl(EPOLL_CTL_ADD)` answers `EPERM` for a regular file or a directory (measured), precisely the class Go's own fallback covers — so the 28 rows the fallback flipped keep their exact behavior while pipes, FIFOs, ttys and sockets become pollable.

**The shape, and why it is not Go's runtime poller.** Go drains epoll from the SCHEDULER (`netpoll(delta)` -> `netpollready` -> `goready`), which go2cs does not have; the converted `runtime/linux/netpoll_epoll.cs` therefore stays dead and is used as a SPECIFICATION. The hand-own reproduces its mechanism outside the scheduler: one `epoll_create1(EPOLL_CLOEXEC)`, one background thread blocked in `epoll_wait(-1)` forever, and per event Go's own mode mapping (`EPOLLIN|EPOLLRDHUP|EPOLLHUP|EPOLLERR` -> `'r'`, `EPOLLOUT|EPOLLHUP|EPOLLERR` -> `'w'`, `events == EPOLLERR` -> the eventErr bit -> `pollErrNotPollable` on the read side only) setting `Ready` under the descriptor's lock and pulsing it. `gopark`/`goready` become `Monitor.Wait`/`Monitor.PulseAll` on that lock — which is exactly what the Windows flavor already does, so its whole descriptor state machine (`Ready`/`Expired`/generation-checked `Timer`s, `pollBlock`, `pollReset`, `pollSetDeadline`, `pollUnblock`) is COPIED rather than re-derived, per the `lock_sema`/`lock_futex` per-GOOS-authority precedent. A waiting goroutine parks its dedicated thread on a gate instead of blocking in `read(2)`: the same one thread, but now `evict` and deadlines can reach it.

**Edge-triggered, and the argument that no edge is lost.** Registration is Go's `EPOLLIN|EPOLLOUT|EPOLLRDHUP|EPOLLET`. Level-triggered would force `EPOLLONESHOT` plus an `EPOLL_CTL_MOD` per wait, or a spinning drain. ET is sound because of the CONSUMER's protocol, not the poller's: every wrapper in `internal/poll/linux/fd_unix.cs` is `prepare` -> syscall -> (`EAGAIN` && pollable ? `wait` -> retry), so (1) `prepare` (`pollReset`) clears a stale `Ready`, (2) the syscall then either succeeds — making the discarded edge redundant — or answers `EAGAIN`, which means the buffer is empty/full AT THAT INSTANT, so any later readiness is a new transition and therefore a new edge, and (3) `wait` consumes `Ready` before parking, so an edge that landed between the `EAGAIN` and the gate is not missed. `fdMutex` admits one reader and one writer, so no second consumer can take the data an edge announced.

**Tokens, not pointers, and the two orderings that follow.** `epoll_event.data` carries an opaque token minted by `Interlocked.Increment`, which is also the `ctx` handed back to `internal/poll` — the kernel cannot hold a managed reference, and the fd number is the wrong key because the kernel reissues it at close (Go defends the same hazard with `fdseq`). So: the table entry is inserted BEFORE `EPOLL_CTL_ADD` (a socket can deliver its only ET edge before `epoll_ctl` returns), and `EPOLL_CTL_DEL` runs BEFORE `close(2)` — which `FD.destroy` already guarantees by calling `pd.close()` ahead of `CloseFunc` — because the kernel drops a closed fd from an epoll set only when its LAST reference closes, and `os.File.Fd()`/`os/exec` dup descriptors into children. A stale event is inert by construction: its token no longer resolves.

**The kernel image is native, per the struct-passing rule.** `struct epoll_event` on linux/amd64 is PACKED — `uint32 events` at 0, `uint64 data` at 4, 12 bytes — and the file writes and reads it with `Marshal.WriteInt32`/`WriteInt64` over `AllocHGlobal` memory, handed to the keystone `syscall(2)` binding as a `uintptr`. No `ж<T>` address and no generated address-taking wrapper reaches the kernel, the same rule the sockaddr and `struct stat` mirrors landed under; and because the safe `Marshal` form needs no `unsafe`, `internal.poll.csproj`'s `<AllowUnsafeBlocks>` — ONE property serving every `$(GoTargetOS)` — stays `false` and the Windows build is untouched. An architecture other than amd64 is REFUSED at `pollServerInit` rather than misread (arm64's record is 16 bytes, unpacked).

**Deadlines are the Windows semantics minus one dimension.** Sticky per-mode expiry, generation-checked `Timer` callbacks, "a deadline in the past fires NOW against the current waiter", wake-without-ready for unblock, and the check order (closing > timeout > eventErr, readiness consumed first) are inherited verbatim. What Linux does NOT have is cancel-and-harvest: a Windows timeout wakes a thread still owning a kernel-pending overlapped operation (hence `CancelIoEx` + `pollWaitCanceled`), while a Linux timeout wakes a thread that owns nothing — the syscall already returned `EAGAIN` — so it simply returns `pollErrTimeout`. `pollWaitCanceled` has no Linux caller at all and is wired to the shared wait loop anyway, so the two flavors' machinery stays textually identical.

**No break eventfd, and `EINTR` is rarer than expected — both measured.** Go writes an eventfd to interrupt a `netpoll` the scheduler owns; nothing here needs to interrupt the drain thread (deadlines and unblocks reach waiters directly), and an `EPOLL_CTL_ADD` issued while another thread sat in `epoll_wait(-1)` was measured delivering the new descriptor's edge 1 ms later with no break write. So the poller owns ONE descriptor, which `isPollServerDescriptor` answers for truthfully — `os/exec`'s `TestExtraFiles` enumerates the parent's fds and skips it. `epoll_wait` is never restarted after a signal (`signal(7)`), so `EINTR` is retried; but measurement corrected the design's own expectation that this would be common — 0 EINTRs in 20 s of slices while the process spawned 22,819 children and ran 7,758 gen0 collections, because the CLR routes signals away from arbitrary threads. Any OTHER errno is Go's `throw("runtime: netpoll failed")`, realized as an unhandled exception on the drain thread: catch-and-continue would convert an invariant failure into a process that hangs on its next socket read.

**Measured (2026-08-22, at `00cc122c9`).** All four behavioral guards run by hand on the Linux flavor are stdout-IDENTICAL to `go run`: `PipeCloseUnblocksRead` prints Go's `read unblocked: read |0: file already closed` where the fallback printed `read did NOT unblock`; `TcpLoopbackRoundTrip` completes IPv4 and IPv6 round trips plus `CloseRead`/`CloseWrite` breaking blocked operations; `NetListenSmoke` binds, deadlines, closes and rebinds; and `NetDeadlineMatrix` passes all twelve assertions including both race arms. `encoding/json` flips to **PASS 491** on the Linux roster — `TestHTTPDecoding`'s `httptest` loopback round trip, the row the design's bill named first.

## The Linux `struct stat` mirror and the NoError raw bottom — the first Linux members of the struct-passing class

**The two seams the poll-seam measurement found standing directly behind the poller (R1 and R4 of the 2026-08-22 board entry), both in the Linux `syscall` flavor, both the shape of a class the Windows lane had already named.**

**R1 — `syscall.Stat_t` passed by address.** On linux/amd64 Go's `Stat_t` *is* the kernel's 144-byte `struct stat`, field for field, with `X__unused [3]int64` inline at the end, so the generated `Fstat`/`fstatat` wrappers pass `uintptr(unsafe.Pointer(stat))` and the kernel writes straight into it. The converted `Stat_t` cannot be that: the trailing array is a golib `array<int64>` MANAGED REFERENCE, so the struct is not blittable, the CLR lays it out itself (~128 bytes, its own field order), and `(uintptr)Ꮡstat` — golib pinning the box's value slot — hands the kernel that managed image. `fstat(2)`/`fstatat(2)` then write 144 bytes of native record over a field order that is not the kernel's **and 16 bytes past the object**. Nothing faults; it answers. Measured in an isolated probe on the Linux flavor: `os.Stat(dir)` returned `err == nil`, `IsDir() == false`, `Mode() == p---------` for a real directory, `Stat().Size()` read 0 for a 3,302-byte file, while `Readdirnames`/`ReadDir`/`Read` (dirent-typed, no stat) were correct — so every `filepath.Glob` answered nothing (glob swallows the I/O error and tests `IsDir`), every `Walk`/`WalkDir` visited only its root, `archive/zip` read "not a valid zip file" from a mis-sized archive, `MkdirAll` said "not a directory": eight roster rows wall-to-wall plus partials, the quiet-wrong-answer shape the struct-passing entry on the board calls the class's worst. The remedy is that class's established one, now on Linux: `syscall/linux/zsyscall_linux_amd64_impl.cs` hand-owns **`Fstat` and `fstatat`** over a `[StructLayout(LayoutKind.Sequential)]` `NativeStatLinuxAmd64` mirror (the reserved words `fixed int64[3]` inline), hands the keystone libc `syscall(2)` binding the mirror's address, and copies field-for-field into the converted `Stat_t` on success — errors and the nil-pointer `EFAULT` follow the Go original, and the path argument keeps the generated wrapper's own `BytePtrFromString` + byte-box pinning verbatim, because that part was never the defect. The two wrappers are displaced from the generated file by `manualConversionFuncs` under a NEW **`goosLinux`** scope: darwin declares both names too, with libc-backed bodies that are not defective and must keep them, and Windows declares neither — `TestLinuxOnlyEntriesAreScopedToLinux` is the mirror of the Windows-only guard. `Stat` and `Lstat` are NOT hand-owned (on linux/amd64 they are pure Go over `fstatat` and inherit the fix), and the mirror is deliberately linux/amd64's layout alone (another Linux GOARCH's `Stat_t` differs and would want its own `zsyscall_<arch>_impl.cs`, stated rather than generalized); `Statfs_t`/`Sysinfo_t`/`Utsname` are the same class and are taken when a suite reaches them, per the class doctrine.

**R4 — `rawSyscallNoError` was still an announcing stub.** Go declares it in `syscall_linux.go` and implements it in `asm_linux_amd64.s` as a bare `SYSCALL; MOVQ AX, r1; MOVQ DX, r2` with no errno handling, for the generated "NoError" family that cannot fail — `Getpid`, `Getppid`, `Gettid`, `Getuid`, `Geteuid`, `Getgid`, `Getegid`, `Umask` — and for `forkExec`'s own getpid/getppid. The Linux keystone hand-own had deliberately left it an announcing stub "until something genuinely needs them"; the roster re-run showed what the stub costs once the poll seam is open: `os.Getuid` → `NotImplementedException` at the top of `os/user.Current` (poisoning its `sync.Once`, so every later call NREs — `archive/tar`'s whole uid→name path), `time.interrupt`'s `Kill(Getpid())` in `TestSleep`, and `os/exec`'s test host dying in init before a single verdict. The body, beside `runtime_entersyscall` in `syscall_linux_impl.cs`, is the keystone binding once more with the errno word dropped — which is exactly what the asm returns, and not a swallowed error: the callers are syscalls that cannot fail.

Guarded by the `StatLayoutTruth` behavioral output test (a temp tree: `Stat`/`Lstat`/`File.Stat` type bits, sizes and a sane `ModTime`; `Glob`, `ReadDir` with `Info()`, `WalkDir` counts; `Getpid`/`Getppid`/`Getuid`/`Getgid` as callable predicates — every line a boolean or a count, so Windows and Linux print the same bytes).

**Measured (2026-08-22, the Linux roster re-run against the poll-seam lane's 122/161 baseline; Windows control on the i9 39/39 green):** every one of the 13 rows the two classes had been attributed to moved exactly as predicted — **7 flip to PASS at their banked counts** (`archive/zip`, `debug/dwarf`, `html/template`, `io/ioutil`, `io/fs`, `internal/diff`, `archive/tar`), **`path/filepath` validates at its Linux count** (54 of 54 matching; the banked 61 is Windows-shaped), and the other **5 are improved to a residual of a different class** — `time` 156/157 (the ZONEINFO caching test), `go/doc/comment` 10,058/10,059, `text/template` 51/52, `go/internal/srcimporter` 4/7 and `os/exec` now running 16/72 (the package-level death is gone), each remaining test on the exec wall. The board entry of 2026-08-22 ("the three bodies") carries the roster arithmetic.

## `Uname` — the struct-passing class taken to its limit, and the deferral rule it exposed

**The member.** `Utsname` is the class's purest case: not a record with one array field at the end,
but a struct that is *nothing but* inline arrays. The kernel's `struct utsname` on linux/amd64 is
six 65-byte character arrays back to back — 390 bytes, every one of them storage. The converted
`Utsname` is six golib `array<int8>` MANAGED REFERENCES and no characters at all, so
`(uintptr)Ꮡbuf` hands `uname(2)` a managed image with nowhere to put what it writes.

**The symptom, and why it read as innocent.** `os/exec`'s `TestFindExecutableVsNoexec` gates itself
on `unix.KernelVersion()`, whose entire body is `syscall.Uname` plus a parse of the `Release` field.
The parse read `(0, 0)`, so the test took **Go's own** skip — `requires Linux kernel v5.8 with
faccessat2(2) syscall` — on a kernel that is 6.18 and has the syscall: Go passed the test, the
converted side skipped it. Two things about that are worth carrying:

- **It is a defect, not a `platform-skip` disclosure.** That class admits a Go=pass/C#=skip pair
  only when the skipped-on property is one the deployment genuinely holds. Here the property is
  false — the kernel has `faccessat2`; the conversion merely could not read its own `uname`.
  Disclosing it would have laundered a fixable seam into a permanent one, which is precisely what
  the class's anti-laundering clause exists to prevent.
- **A smashed header reads as LENGTH ZERO, which is why the failure was quiet.** The `foreach` over
  `Release` iterated nothing and returned `(0, 0)` instead of faulting — so the run continued, and
  the clean skip looked like evidence that the kernel's write had never landed. It is the opposite:
  a zero-length read is what a smashed `array<T>` header looks like.

**The remedy** is the class's established one, third time on Linux after `Fstat`/`fstatat` and
`wait4`: a `[StructLayout(LayoutKind.Sequential)]` `NativeUtsnameLinux` with six `fixed int8[65]`
buffers, size-asserted at the boundary against the kernel's 390, handed to `RawSyscall` (the
wrapper it displaces is `//sysnb`, so there is no enter/exitsyscall bracket), then a field-for-field
copy back that reuses an already-correct backing store and copies the NUL and everything after it
verbatim — Go's caller receives the kernel's bytes, and the callers that want a string do their own
scan for the terminator. Registered `goosLinux` in `manualConversionFuncs`, because `Uname` is a
Linux-only declaration.

**⚠ The corpus half of a hand-own is not optional, and a `-tests` run will not reveal it.**
`manualConversionFuncs` displaces a wrapper at CONVERSION time; the committed corpus file still
carries the previously generated body until something regenerates it. The two coexisted here and
the Linux `syscall` package failed to compile with **CS0111** — while every `-tests` run stayed
green, because that pipeline converts *only the package under test* and never regenerates
`syscall`. `Fstat`, `fstatat` and `wait4` are absent from `zsyscall_linux_amd64.cs` for exactly this
reason (their regen removed them), so the generated `Uname` was deleted in the same commit rather
than deferred to the next `-stdlib` run. **The gate that catches this is a real build of the
package on its own target** (`dotnet build syscall.csproj -p:GoTargetOS=linux --no-incremental`);
no test-pipeline verdict can.

**What it did NOT fix, stated because the measurement was run.** The same suite's parallel-load
heap corruption is untouched: 4 compare rounds at the fixed tree, 4 crashes, with the `faccessat2`
skip gone in every one. The corruption is independently proven — a `verifyheap` over a crash dump
reports a contiguous run of errors (a zeroed method table, three members that are text where
pointers belong, a bogus syncblock index) — but its writer is a separate, still-open question.
`Uname` is banked on what it demonstrably fixes.

**The deferral rule this exposed.** The `Fstat` entry above ends "`Statfs_t`/`Sysinfo_t`/`Utsname`
are the same class and are taken when a suite reaches them, per the class doctrine" — and `Utsname`
sat on that list, reached, quietly answering `(0, 0)`, because "no roster row reached it" is a
statement about our COVERAGE rather than about execution. For members whose converted struct carries
`array<>` fields the failure is not only a wrong answer: the kernel writes over GC-TRACKED
REFERENCE SLOTS, which the collector then follows. That is a memory-safety failure whose crash can
surface arbitrarily far away, so for that sub-class "when reached" is the wrong bar and the
remaining members (`Sysinfo_t`, `Statfs_t`, `Timex`, `Flock_t`, `FdSet` — the last two being kernel
write-backs any converted program can reach) are worth closing proactively.

## The sockaddr family on Linux — L10's mirror, arm for arm, as the socket poller's prerequisite; and `Mmap`'s slice is a snapshot

**The sockaddr mirror.** The Linux flavor has the same two defects the Windows lane's L10 entry (above, *The SOCKET-ADDRESS family*) retired: `syscall_linux.go` writes the port through `(*[2]byte)(unsafe.Pointer(&sa.raw.Port))` — which converts to a length-zero `array<byte>` and panics `index out of range [0] with length 0` in `SockaddrInet4.sockaddr` — and `Bind`/`Connect` hand the kernel `unsafe.Pointer(&sa.raw)` while `accept4`/`getsockname`/`getpeername` hand it `&rsa` to fill, neither of which has a native layout (`RawSockaddrInet4.Addr/Zero`, `RawSockaddrAny.Data/Pad` are golib `array` references). Measured on the 2026-08-22 Linux roster re-run as R5: `encoding/json`'s `TestHTTPDecoding` and `crypto/tls`'s `TestMain` both died in the encoder before any socket call. `syscall/linux/sockaddr_linux_impl.cs` mirrors L10 exactly: the two INET encoders write the port arithmetically; `writeNativeSockaddr` builds the native `sockaddr_in`/`_in6`/`_un`/`_ll`/`_nl` image in a stack buffer (calling Go's own `sockaddr()` first, so there is ONE definition of what a Sockaddr means and the hand-own translates layout alone); `Bind`/`Connect` hand that address to the package's own generated address-taking `bind`/`connect`; `Getsockname`/`Getpeername`/`Accept4` go through the trampoline with a stack buffer — their generated wrappers take a typed `ж<RawSockaddrAny>` — and decode with `readNativeSockaddr`, Go's `anyToSockaddr` arm for arm over a native image; and `anyToSockaddr` itself becomes a flatten-then-decode of the managed struct (Family at 0, Data 2..15, Pad 16..111 = `SizeofSockaddrAny`), so any remaining auto caller decodes correctly once its own fill is. The registry carries the family under a new **`goosWindowsLinux`** scope for the names both flavors hand-own (the encoders, `Bind`/`Connect`/`Getsockname`/`Getpeername` — one entry, each flavor's per-GOOS file its own authority, the `lock_sema`/`lock_futex` precedent; darwin keeps its libc bodies) and `goosLinux` for `Accept4`/`anyToSockaddr`, guarded by `TestSockaddrFamilyIsScopedToEachHandOwningFlavor`. Not covered, named: `Recvfrom`/`Sendto`/`Recvmsg`/`Sendmsg` (UDP and ancillary), exactly L10's line.

**What it buys, stated honestly: the wall moves, the gate does not open.** A Linux socket is un-armable — the poller fallback above answers `EPERM` from `runtime_pollOpen` for every descriptor — so once `Bind`/`Connect` succeed, `net`'s `listenStream`/`dial` reach `FD.Init` → `pollDesc.init` and return `operation not permitted`: an honest error until a Linux readiness poller exists (the netpoll design's §8 non-goal, a separate design). The mirror is that poller's prerequisite, not a row flip, and the board entry of 2026-08-22 measures it as such.

**`Mmap`'s slice is a snapshot (W1b, rooted, not fixed here).** `syscall.Mmap` on Linux returns `unsafe.Slice((*byte)(unsafe.Pointer(addr)), length)`, and golib's `unsafe.Slice` over a NATIVE pointer SNAPSHOTS the bytes into a managed slice (stated in `unsafe.cs`: "writes through the resulting slice do not reach the native memory"). Probe, converted program: `Mmap(0, 0, 3·pagesize, …)` → a 12,288-byte slice, err nil; `Mprotect(b[:pagesize], PROT_NONE)` → `invalid argument` (the kernel was handed a managed element address); `Munmap(b)` → `invalid argument`; writes land in the copy. That is the whole of `crypto/sha1`'s `TestOutOfBoundsRead` and `bytes`' four `*NearPageBoundary` tests on Linux. The honest remedy is a native-backed `slice<T>` in golib — a `MemoryManager<T>` over the mapping, with `Ꮡ(b, i)` and the `uintptr` conversion yielding the native address and the mapping owning the lifetime — a slice-model change, not a per-GOOS hand-own, routed rather than taken in the sockaddr lane.

## The CryptoAPI chain seam — when a native record must be READ as a struct and HANDED BACK as a pointer

`crypto/x509`'s Windows system verifier is the one place in the corpus where the two syscall
boundary classes meet inside a single call, and the transferable finding is that **they are not
separable**. `zsyscall_windows_certchain_impl.cs` owns `CertGetCertificateChain`,
`CertAddCertificateContextToStore` and the two `CertFree*` routines that pair with them.

The **input** half is the ordinary struct-passing defect over an unusually bad record.
`CERT_CHAIN_PARA` is 80 native bytes and Go's declaration mirrors them exactly — which is why
`root_windows.go` writes `para.Size = unsafe.Sizeof(*para)` and means the native size by it — while
the converted struct holds `RequestedUsage.Usage.UsageIdentifiers` as `ж<ж<byte>>` and `CacheResync`
as `ж<Filetime>`, so the CLR lays it out reference-first at roughly a third of the size. The field
that decides the symptom is `dwUrlRetrievalTimeout` at native offset 56: a **blocking network
budget** read out of whatever managed bytes land there, which is why the observed failures are a
process-length hang (`crypto/tls`'s `TestQUICHandshakeError`) or an `SEHException` out of `Syscall9`
(`TestVerifyHostname`, and the offline verifier) rather than a clean error. `UsageIdentifiers` is
the part that is more than a layout copy — an array of C string pointers into NUL-terminated managed
byte slices, with no native form at either level — so it is transcribed into one native block for
exactly the duration of the call, per the mirror-is-a-local doctrine.

**Fixing only that was measured NOT to be enough**, and the reason generalizes past CryptoAPI: one
of this call's arguments is a field the CALLER reads out of a native record before the wrapper is
ever entered. `syscall.CertGetCertificateChain(0, storeCtx, verifyTime, storeCtx.Store, …)` reads
`hCertStore` at the *converted* `CertContext`'s offset, which under reference-first auto-layout is
where the native record keeps `cbCertEncoded` — so the store handle crypt32 receives is a
certificate LENGTH. With the parameter mirror in place and nothing else, the identical
`SEHException` remained. **When a wrapper's argument is a field of another wrapper's result, the
input fix and the read-back fix are one change.**

The read-back cannot take the `GetAddrInfoW` shape. That hand-own transcribes a native chain into
managed records and makes the free a NO-OP, which works because nothing native has to survive the
call; here the original pointer must go back to crypt32 three more times (as the next call's leaf,
to `CertVerifyCertificateChainPolicy`, and to the two `CertFree*` routines, which release
reference-counted memory). A managed view alone leaks a chain per verification; a native box alone
reads every field from the wrong offset. So each returned pointer becomes a **managed view that
remembers its native identity**: a real `ж<CertContext>`/`ж<CertChainContext>` the converted Go code
reads as an ordinary struct, with the address it was built from recorded beside it in a weak
`ConditionalWeakTable`. Every wrapper that must hand a pointer back asks that table first and falls
back to the box's own address, which is what lets `CertCreateCertificateContext` and
`CertEnumCertificatesInStore` stay generated — they produce plain native boxes nothing reads a field
through. The table is a **syscall-local** seam by design: `ж<T>` has no business knowing that one
pointee is reference-counted by crypt32, and the sync point is again something only the wrapper
knows. Note this is the third answer the corpus now has for a returned native pointer, and the
question that selects between them is not the struct's shape but **who else needs the pointer**:
publish a native box (`zsyscall_windows_ptrout_impl.cs`, for opaque handles), transcribe and free
eagerly (`zsyscall_windows_addrinfo_impl.cs`, when nothing native survives), or transcribe and
remember (here, when it must).

Guarded at VALUE level by the `SystemCertVerify` behavioral output test: a self-signed ECDSA leaf
verified with `Roots == nil` (an untrusted-root verdict every Windows host agrees on, with no
network and no dependence on the machine's stores), followed by the same CryptoAPI sequence driven
directly — the store handle read back through the context compared against the handle the store was
opened with, and the leaf's DER recovered through `CertChainElement → CertContext → EncodedCert` and
compared byte for byte against the original. A verdict alone could not carry this: a misread trust
status produces the same "unknown authority" answer, so the round trip is the evidence. Proven
failing-first — the pre-fix binary prints the first two lines and then dies with the SEHException.

**Still open, and named rather than papered over:** `CertVerifyCertificateChainPolicy`. Its
`CERT_CHAIN_POLICY_PARA` carries `pvExtraPolicyPara` as Go's opaque `syscall.Pointer`, minted in
`crypto/x509` as `unsafe.Pointer(sslPara)` over an `SSLExtraCertChainPolicyPara` whose `ServerName`
is itself a managed reference — so what reaches the boundary is a transient managed address with no
recoverable box behind it. That is a MINT-SITE problem (a `ManagedPointerTokens` registration at the
conversion, or a hand-own of `checkChainSSLServerPolicy`), not a boundary one, and it is reached
only when a chain is TRUSTED *and* the caller supplied a DNS name.

## A hand-owned file can declare that it needs `/unsafe`

`<AllowUnsafeBlocks>` is converter-generated from `usesUnsafeCode`, and `usesUnsafeCode` is an **emission** fact: it is set while visiting Go source, so it sees only C# the converter itself wrote. A hand-owned file is by definition code the converter did not write. That left a hole with no honest way through it — a package whose only need for `/unsafe` was hand-written could not express it, because the `.csproj` is regenerated on every transpile and any value set by hand is undone by the next reconvert overlay.

`[module: go.GoRequiresUnsafe]` closes it. The declaration lives in the file that HAS the requirement, and the emission unions it into the property:

```csharp
// core/time/time_impl.cs — `time`'s converted emission contains nothing unsafe at all
[module: go.GoRequiresUnsafe]

namespace go;
```

It is a **union**, and inert for the same reason the cross-platform union in `platformEmit.go` is: the property *grants* a capability rather than using one, so raising it moves no IL for code containing nothing unsafe. Together the two unions mean a `.csproj` says `true` when the converter's own emission needs it on **any** target, or when **any** hand-owned file in the package declares it.

Shape follows the [`GoManualConversion`](#manually-converted-declarations) precedent exactly — a module-scoped attribute class in `golib`, detected by scanning the file's header text — and shares that marker's **scan** rather than adding a second one, so it inherits the same comment lexer: a `/*` that is ordinary prose inside a `//` line comment opens no block, and a marker *mentioned* in a comment is not a declaration. Both marker names live in the canonical symbol table (`src/core/go2cs/symbols.json`), because each is one string spanning a C# attribute declaration and a Go regexp and belongs in neither.

The walk is the package's own files plus its per-GOOS source folders, **never recursive**: a converted package directory can hold nested packages (`internal/runtime` holds `syscall`, `atomic`, …) whose own `.csproj` answers for them, and the discriminator is layout L3's own — a per-GOOS folder holds no project file — so `internal/syscall/windows` is not read as its parent's Windows sources. The declaration is per **package** rather than per platform because a `.csproj` is one file serving every `$(GoTargetOS)`.

Like the hand-own marker itself, this reads the OUTPUT tree, so it inherits the same prerequisite: a reconvert must be **seeded** from the committed corpus, or there is nothing on disk to declare anything. That is already the standing reconvert ritual, so it adds no new rule.

Three packages declare it today, all of them at a kernel boundary: `internal/runtime/syscall` (the Linux `Syscall6` keystone) and `time` flip from `false`, and `syscall` states a requirement its converted emission already happened to satisfy — inheriting a requirement by luck is how it disappears. See [Every P/Invoke is source-generated](#every-pinvoke-is-source-generated) for what the flag is spent on.

## Hand-owns have a platform, and it is not the same question as a folder

Two different mechanisms answer two different questions, and conflating them is what produced the Linux corpus's whole class-(b) failure surface (`docs/phase4/DESIGN-multiplatform-corpus.md` §12, increments 3.5 and 3.5b).

* **Where a hand-owned FILE is built** is layout L3's question, answered by the file's *principal*: an `*_impl.cs` takes part in exactly the platform builds the `<name>.cs` it supplements does; a marked whole-file hand-own follows its `.cs.auto` review sibling. Every platform ⟹ flat; a subset ⟹ one copy per platform in the subset. Routed automatically by the reconvert merge and guarded by a walk of the real corpus (`platformHandOwn_test.go`).
* **Whether a DECLARATION is hand-owned at all** is the registry's question, and until 2026-08-08 it had no platform axis: `manualConversionFuncs` was keyed by name alone. A Go name is not unique across platforms — Go selects one of several files declaring the same function by build constraint — so one entry spoke for every flavor, turning each one's declaration into a placeholder while an implementation existed only where somebody had written one.

Entries now carry a `goosScope`, whose empty value (`goosAny`) means every target and is what nearly all ~120 entries use. Scoping is load-bearing in both directions:

| Entry | Scope | Why |
|:--|:--|:--|
| runtime's `mutexContended`, `lock2`, `unlock2`, `notewakeup`, `notesleep`, `notetsleep_internal` | `goosAny` | Both flavors need hand-owning; the arity difference is the *file's* problem, not the registry's |
| `os.(*File).readdir` | windows, darwin | Those flavors hand OS memory to a Go struct (`FILE_ID_BOTH_DIR_INFO` reinterpreted; libc `readdir_r(&dirent)`). `dir_unix.go`'s is **pure Go** over `internal/poll` and converts faithfully |
| `os.readReparseLink`, `syscall`'s five generated wrappers | windows | Declared only in Go's Windows sources; already inert elsewhere, now stated rather than re-derived |

The `os.(*File).readdir` row is the one that cost something: unscoped, the entry deleted the perfectly convertible unix body too, so every Linux `os` build carried a placeholder with nothing to link against — a hand-own gap **invented by the registry rather than by the Go source**. Scoping it out is why Linux needs no `readdir` hand-own at all, which is strictly better than writing one.

What a scope deliberately does **not** express is a per-platform *signature*. The registry decides whether a declaration is hand-owned; the hand-owned file decides what it looks like. Guarded by `manualConversionScope_test.go`, whose fixture is the `lock_sema`/`lock_futex` pair at their real 4-vs-2 arity, plus a typo guard — a scope naming an unknown GOOS matches nothing, which would silently turn a hand-own off everywhere and is otherwise unreportable, since "not hand-owned" is a legitimate answer for every other declaration.


### `crypto/subtle`'s word-at-a-time XOR

**`crypto/subtle`'s word-at-a-time XOR (`core/crypto/subtle/xor_generic.cs`, whole-file).** `xorBytes` XORs a machine WORD at a time by reinterpreting its three byte slices as `[]uintptr` (`unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), len(x)/wordSize)`). A `uintptr[]` view over a `byte[]` does not exist in the managed model — golib's `slice<T>` is a window on a real `T[]` — so the converted `words()` could only SNAPSHOT the bytes into a detached `slice<uintptr>`, and the word loop XORed the snapshot and dropped it: for every length that is a multiple of 8, `XORBytes` wrote **nothing**. The whole file is hand-owned (marked `[module: GoManualConversion]`) and does the same reinterpret the managed way, `MemoryMarshal.Cast<byte, ulong>` over the slices' own spans — a genuine aliasing view, so the word writes land in place — keeping Go's word-at-a-time behavior and the performance contract crypto/cipher's CTR and GCM modes depend on. Only Go's `supportsUnaligned`/`aligned` gate is dropped (it exists for architectures whose unaligned word loads fault). Full detail: *`unsafe.Slice` over MANAGED element storage ALIASES it*. Guarded by crypto/subtle's own suite (7/7, no disclosures, over the full 1..1024 x 8 x 8 x 8 alignment matrix).

### `sync/atomic.Value`

**`sync/atomic.Value` (`core/sync/atomic/value.cs`, whole-file).** Go's `atomic.Value` stores and loads an `any` atomically by reinterpreting the interface's internal two-word `(type, data)` layout: `(*efaceWords)(unsafe.Pointer(&v))`, then `atomic.LoadPointer`/`StorePointer`/`CompareAndSwapPointer` on the `typ` and `data` slots, with a `firstStoreInProgress` sentinel guarding the first store. That layout is a Go runtime detail with **no managed equivalent** — an `any` here is a single `System.Object` reference (one word), and reinterpreting a managed reference as a raw address to poke type/data words simply NREs (the same managed-referent-through-`unsafe.Pointer` wall as the guintptr family). The first *operational* hit was `internal/testlog`'s package-level `var logger atomic.Value`, loaded during `os.Getenv` — so `atomic.Value.Load()` NRE'd on the zero value before any store. The whole file is hand-rewritten (marked `[module: GoManualConversion]`) to store the `any` **directly** in the `Value.v` field and use `Volatile.Read`/`Interlocked.CompareExchange` for the acquire/release ordering and CAS the literal conversion cannot provide; the nil-store and inconsistent-type panics, and `CompareAndSwap`'s by-value comparison (`AreEqual`, matching Go's `i != old`), preserve the spec. Guarded by the `AtomicValue` behavioral test (Load-nil / Store / Swap / CompareAndSwap over typed string values, output-compared vs Go).

### `testing.AllocsPerRun`

**`testing.AllocsPerRun` (`core/testing/testing.cs`, Phase-4 testing shim).** Go's version counts **mallocs**: it pins `GOMAXPROCS(1)`, runs `f` once as a warmup, then `runs` more times, and returns the `runtime.MemStats.Mallocs` delta divided (as integers) by `runs`. The CLR exposes no malloc counter, so the shim measures allocated **bytes** on the calling thread instead (`GC.GetAllocatedBytesForCurrentThread()` — precise, and inherently thread-scoped, which stands in for the GOMAXPROCS pinning; like Go's, `f` is assumed single-threaded — allocations made by goroutines `f` spawns land on other threads and are not observed). The mapping is deliberately honest rather than count-approximating: **zero maps exactly** (0 bytes ⟺ 0 mallocs — and the stdlib tests that use AllocsPerRun overwhelmingly assert zero, e.g. sort's `TestSearchWrappersDontAlloc` and the strings/bytes no-alloc guards), while a nonzero result is the average allocated bytes per run, floored at 1 so amortized sub-byte-per-run allocation can never masquerade as the exact-zero case. A converted test asserting a specific nonzero *count* therefore diverges as a loud failure in the differential oracle instead of silently passing — the disclosed outcome. (`runs == 0` divides by zero, a runtime-error panic exactly where Go's own integer division panics.) The capability sits in the converter's supported list (`supportedTestCapabilities`, `testConversion.go`), so tests requiring it convert as *included*; guarded by `TestAllocsPerRunCapabilityIsSupported` (converter) and `TestingRuntimeTests.AllocsPerRunMapsZeroExactlyAndReportsBytesWhenAllocating` (shim).

**That no COUNT is available is measured, not assumed, and a nonzero result now says which unit it is in (r56d).** The value the shim returns is rendered by Go's own `"got %v allocs"` format, so a byte figure was reaching the page wearing the word *allocs* — `crypto/internal/nistec`'s row reads `got 21964011.0`, and nothing on it said that was 21 MB rather than 22 million objects. Since a disclosed divergence may never paper over a go2cs-owned defect, an invisible unit at the seam is itself the defect. The survey behind the claim (net9.0/9.0.18, x64) is recorded on the declaration: the whole public `GC` surface exposes byte totals only; `GetAllocatedBytesForCurrentThread` is exact (40.000 B/object over 1, 10, 1e3 and 1e5 allocations of a 40-byte type) yet cannot separate count from size, one `byte[40000]` and 1,000 40-byte objects both reading ≈40,000 B; `GCAllocationTick` is a byte-threshold *sample*, 378 events per 1,000,000 allocations (one per ≈105,820 B); `GCSampledObjectAllocation` — whose `ObjectCountForTypeSample` payload *would* be a count — raises **zero** events through an in-process `EventListener` in every configuration tried (High `0x200000`, Low `0x2000000`, both, and all keywords `0xFFFFFFFFFFFF`, at Verbose and Informational), with the GC keyword's own tick count as the live positive control; `System.Runtime`'s 27 EventCounters offer only `alloc-rate`, bytes per interval; and runtime events reach an in-process listener **asynchronously** — zero visible immediately after the measured loop, settling ≈117 ms later — so no event-derived figure could be returned by a synchronous call anyway. Accordingly a **nonzero** result records its unit once on the running test (`TestExecution.NoteMeasurementUnitOnce`), landing beside the assert's own message and riding the `TestEvent` into `results.json`; the **zero** case is deliberately left silent, because there the two units agree exactly (0 bytes ⟺ 0 allocations) and a test that passes on the zero answer keeps its output byte-identical to before the seam existed. A true count *is* obtainable from go2cs's own runtime rather than the CLR's — golib allocates essentially every Go-semantic object, so counting there mirrors what Go's `Mallocs` already is, a runtime-owned counter rather than a platform facility (proven in r56d: nistec's P256 body allocates 241,077 golib objects per run for its 21,963,547 bytes) — but it is deliberately not taken, since a count that silently omits allocation sites is worse than an honest byte figure and an audited-total census of golib's allocation sites is a design-with-user arc.

The strings suite exposed the two divergence classes this mapping discloses (full analysis:
`docs/phase4/StringsBytes-BlockerMap.md`, *AllocsPerRun divergence analysis*): **count-shape
asserts** (`TestBuilderAllocs` wants exactly 1 malloc; the shim reports bytes, so any nonzero
diverges loudly — by design), and **allocation-profile divergences**, where the zero-shape itself
is unsatisfiable because the managed model allocates where Go's compiler doesn't: an addressed
local (`var b Builder` + pointer-receiver calls) heap-boxes per run where Go stack-allocates
(`TestBuilderGrow`'s growLen=0 leg), and `string(r)` materializes a `byte[]` where Go uses a
stack buffer (`TestIndexRune`). Neither class is a shim defect — a malloc-counting shim would
fail the same asserts — and neither is faked.

**A third class is neither, and must not be filed as either: ELIMINABLE inefficiency.** Because zero
maps exactly, a want-zero assert is faithfully representable — so when one fails, the honest reading is
that the converted code genuinely allocates, and the question is *what*, not *whether the unit is
comparable*. `time`'s `TestUnmarshalTextAllocations` (`got 3784 allocs, want 0`) measured out at **3664
bytes/run** for `Time.UnmarshalText`, and profiling `parseRFC3339` — where nearly all of it lives —
attributes it to two shapes, both in shared machinery and both removable:

| Shape | Cost | Why | Status |
|:--|--:|:--|:--|
| `s[a:b]` on a `string \| []byte`-constrained value | 48 B each | `IByteSeq<T>`'s range indexer returned the **interface**, so the `@string`/`slice<byte>` struct result was boxed | **fixed** — self-referential `IByteSeq<TSelf, T>` |
| `[]byte(s)` on the same (`new slice<byte>(sΔ1)`) | 48 B each | boxed the type-parameter value again to reach the interface | **fixed** — `ToSlice` extension |
| `len(s)` on the same | 48 B each | the `len<T>(IByteSeq<T>)` overload took an interface parameter | **fixed** — `len<TSeq>(TSeq) where TSeq : IByteSeq` |
| `for i, c := range s` over a `slice<T>` | **136 B, fixed** | the range enumerator allocated once per loop, independent of length; the indexed form allocates **0** | **fixed** — struct enumerator |

Six `parseUint` calls at ~232 B each, the closure and delegate for `parseUint` itself (112 B), the
fractional-second scan (~1728 B in the same shapes) and `Date` (~240 B) accounted for the total. None of
it was CLR-necessary: Go monomorphizes the union-constrained generic and stack-allocates all of it, and
nothing here was boxing that a managed model *must* do — it was boxing that the `IByteSeq`
modeling and range lowering happened to do. **So this row was performance work, not a disclosure
candidate** (a disclosure is only for asserts the CLR provably cannot satisfy — see the campaign
charter §5), and it was resolved as such: the range enumerator became a struct (every converted
`for i, v := range s` in the corpus had been paying it), and the union-constraint boxing was removed
wholesale by the self-referential redesign described under
[Allocation-free union-constrained bodies](generic-constraints.md#allocation-free-union-constrained-bodies) — a
`parseRFC3339`-shaped body over `slice<byte>` measures **0 B/parse** where it measured 720.

### The disclosed-divergence manifest

**The disclosed-divergence manifest (2026-07-18 ruling — implemented).** These provably
unsatisfiable divergences are disclosed at TEST level, extending the declaration-level
"disclosed-unsupported" vocabulary: an affected package carries a hand-owned, repo-committed
`go2cs_test_disclosures.json` beside its converted sources (never generated — reviewed like
source; deliberately absent from `src/core/.gitignore`'s regenerated-artifact list),
pinning `{name, class, signature, reason}` per divergent test. The `-test-action compare` oracle
(`matchTerminalStatuses`, `testConversion.go`) reclassifies a Go=pass/C#=fail row as
**disclosed-divergent** ONLY when the exact test name is pinned AND the captured C# failure
output contains the pinned signature substring — the converted host attaches each test's
accumulated log text to its terminal event, which is what the signature matches against. The
signature pin is the integrity guard: a pinned test failing with ANY other signature (e.g. an
index-semantics leg regressing) is still a mismatch, a pinned test in any other status pair
(including C#=infrastructure-error) is still a mismatch, and a package with no manifest compares
strictly — sort and utf8 are unaffected. The validation summary discloses the reclassified rows
alongside the excluded declarations (`… 7 disclosed-divergent (alloc-profile), …`), subtracting
them from the validated count, and the nonzero C# host exit the disclosed failures cause is
forgiven only when `go test` itself was clean and every divergence matched its pin (zero
mismatches — a truncated host run surfaces as one-sided rows and stays fatal). An empty
signature (which would substring-match anything) and duplicate names are load-time errors, never
silent no-ops. Guards: `TestDisclosedDivergenceOracle` (signature match discloses / different
signature still fails / no manifest strict / direction+status pairs never widen) and
`TestDisclosureManifestLoading` (absent-file no-op; empty-signature and duplicate rejection).
First users: `bytes` (7 alloc-profile rows) and `strings` (3 alloc-count-semantics + 1
alloc-profile), validating as Phase-4 packages #3 and #4. `unicode/utf16` (package #5) is the first
to reuse the mechanism as a general tool rather than a bytes/strings special case: its lone
`TestAllocationsDecode` asserts `Decode` returns its non-escaping `[]rune` with **zero** allocations —
which Go reaches only through escape analysis (the test guards itself with `testenv.SkipIfOptimizationOff`),
and which the managed runtime provably cannot, since a returned `slice<rune>` is always a heap allocation.
It discloses one `alloc-profile` row (signature `"Decode allocated "`) while `TestDecode` independently
proves the decoded output is correct — the disclosure covers exactly the allocation profile, nothing else.

### A pin whose GO side is not deterministic — the `hostConditional` annotation

**A pin whose GO side is not deterministic — the `hostConditional` annotation (2026-08-20 coordinator
ruling — implemented).** A disclosure asserts *Go passes, C# provably cannot*, so it is only stable
while the **Go** side is stable. `crypto/tls`'s `TestBogoSuite` is the first pinned row whose Go side
is not: `go test` downloads the pinned boringssl module, builds BoringSSL's own runner from it, and
whether that runner passes, expands into its 3,243-row case matrix, or fails is decided per host and
per run by network reachability and by the 10-minute deadline Go's test binary carries by default.
That makes the pin brittle in **both** directions — the sweep goes red when Go starts failing (Go
fail / C# fail reads as an ordinary *agreed failure*, so the row leaves the disclosed set and
`crypto/tls` reports 401 + 1 where the roster banks 400 + 2), and red again when Go goes back to
passing on a quieter host. Nothing in a go2cs branch can move either reading: the baseline is
`go test -json` over GOROOT's own sources, where the BoGo shim is Go's own test binary.

> *At go1.24.13 (after this ruling):* the BoGo fan-out is 3,419 rows (1 parent + 1,022 pass + 2,396
> skip), `crypto/tls` banks 4759 + 1, and `TestBogoSuite` agrees pass/pass on its proof page, so the
> annotated entry discloses nothing there. The figures in this entry are its date's.

The ruling **refused** the broad remedy — accepting agreement-on-failure as satisfying a disclosure
*in general*, which would make every pin self-satisfying the moment its baseline broke for any
environmental reason, quietly decaying *Go passes, C# provably cannot* into *C# fails* — and adopted
a per-row annotation instead. A manifest entry may carry `hostConditional`, whose non-empty value IS
the marker **and** the one sentence naming the environmental dependency (so a row can never be marked
without saying what it depends on; a blank one is a load-time error, like an empty signature). An
annotated row is accepted in **exactly two shapes** and accounts as **disclosed — never as matching**
in both: Go pass / C# fail (the pinned divergence) and Go fail / C# fail (agreement, where the Go
premise itself fails). The roster arithmetic is therefore host-stable — `crypto/tls` reads 400 + 2 on
every machine. Go-side subtest children of an annotated row ride the existing withdrawal rule in the
second shape too, because shape (b) is precisely the shape in which Go DID reach its fan-out;
without that, `TestBogoSuite`'s 3,243 case rows land as one-sided mismatches on a run where the
converted side never moved. **The tolerance is confined to the half that was never deterministic:**
the signature pin governs both shapes, so a C# failure that moved is a strict mismatch under the same
wording the first shape uses, a C# side that starts passing leaves the disclosed set and the sweep's
`disclosed count moved` check fires, and an *unannotated* row gains no second shape at all. Only a
row with coordinator-accepted rooting evidence is annotated; `TestBogoSuite` is the first and only
member. The generated proof page renders the row's own note from the manifest — naming the
dependency and both accepted shapes, the model the roster's `internal/zstd` row sets — rather than
carrying a hand edit a regeneration would drop. Guards:
`TestHostConditionalDisclosureAccountsInBothShapes` (both shapes disclose; a moved C# signature is a
mismatch; a C# side that starts passing leaves the set; an unannotated row stays an ordinary
agreement), `TestHostConditionalRootWithdrawsGoOnlyDescendantsWhenGoFails` (the flood exclusion, with
the un-annotated control that floods), `TestHostConditionalRowRendersDisclosedWhenBothSidesFail` (the
page's totals, its disclosed marker and the note) and `TestHostConditionalMarkerMustNameItsDependency`.
Shape (b) cannot be forced on a host whose Go BoGo run passes, so those fixtures ARE its proof.

### The Windows directory-entry walk

**The Windows directory-entry walk (`os/dir_windows_impl.cs`, Phase-4 — os operational).** Go's
`(*File).readdir` walks the buffer `GetFileInformationByHandleEx` fills by REINTERPRETING it as a Go
struct — `info := (*windows.FILE_ID_BOTH_DIR_INFO)(entry)`, then
`unsafe.Slice(&info.FileName[0], info.FileNameLength/2)`. That struct is **managed-referent**: its
inline Go arrays (`ShortName [12]uint16`, the variable-length trailing `FileName [1]uint16`) convert to
golib `array<uint16>` OBJECT references — 8 bytes each where the OS wrote 24 and 2 bytes inline — so
the managed layout does not describe the buffer bytes at all. `&info.FileName[0]` addressed a
zero-length array (`System.IndexOutOfRangeException` on the FIRST directory read: `path/filepath.Glob`,
`os.ReadDir`, `os.File.Readdirnames`, and every test that walks a testdata directory), the fields after
`ShortName` sit at the wrong offsets, and merely *copying* the reinterpreted struct hands the GC a
fabricated object reference. No converter or golib change can rescue this — a managed array reference
can never be laid out like an inline OS array — so it is the *raw metal on non-native types* arm of the
[conversion fork](../../src/archived/Baseline-vs-FullConversion.md): the declaration is hand-owned.

Scope is one declaration, via the type-level registry (`manualConversionFuncs["os"]["File.readdir"]`);
everything else in `dir_windows.go` (`dirInfo.init`/`close`, the pool, `dirEntry`) stays auto and keeps
receiving converter improvements. The hand-owned form decodes the two entry layouts straight out of the
byte slice at their documented offsets and never materializes a managed struct over OS memory — every
read is an ordinary **bounds-checked managed** slice read, so there is no pointer, no pinning and no
`unsafe` block in the file (a short or truncated buffer surfaces as an index panic rather than reading
past the OS data). Offsets come from the **Go** declarations in `internal/syscall/windows`, which match
the buffer bytes for every field Go reads: Go widens Win32's `CCHAR ShortNameLength` to `uint32`, which
shifts only `ShortName` — a field neither Go nor the impl reads — while `FileID` (96) and `FileName`
(104) land identically either way. The auto `newFileStatFromFileIDBothDirInfo` /
`newFileStatFromFileFullDirInfo` remain emitted but are now unreachable (their parameter *is* the
unusable reinterpret); the impl builds the `fileStat` from the same offsets.
⚠ The registry key is name-keyed per package, so it also matches `os.(*File).readdir` in `dir_unix.go`
— a `-platforms linux/amd64` conversion of `os` would drop its (perfectly convertible) unix readdir.
Same platform caveat as runtime's `lock_sema` entries. No behavioral guard is expressible: the baseline
`src/core` has no `os` package, so the guard is the operational one — `go/doc/comment`'s `TestTestdata`
(`filepath.Glob` over `testdata/`) went from *zero subtests ran* to 54 enumerated.

### The testing shim's compile-only benchmark surface and `CoverMode`

**The testing shim's compile-only benchmark surface and `CoverMode` (`core/testing/testing.cs`).** Capability-excluded test and benchmark declarations still **compile** — exclusion gates the run registry, not emission — so every member their bodies reference must exist even though the code never executes (a broken emission inside an excluded test blocks the whole package build; see the strings/bytes blocker map, B6). The `B` surface (`N`, `Run`, `ReportAllocs`, `SetBytes`, `ResetTimer`, `StartTimer`, `StopTimer`, `Errorf`, `Fatal`, `Fatalf`) is therefore compile-only: safe non-throwing no-ops, with the params-taking members carrying explicit `ж<B>` overloads exactly as `T`'s do (ref-like `params` Spans are outside the RecvGenerator's synthesis). `testing.CoverMode()` returns `""` — not a stub-lie but Go's exact coverage-off value: the sole caller across the strings/bytes suites (strings' TestIndexRune) branches on `CoverMode() == ""` and so takes the same path as an uncovered `go test` run. Guarded by `TestingRuntimeTests.BenchmarkCompileSurfaceIsNoOpAndCoverModeReportsCoverageOff`, which compile-references every member through both receiver shapes and asserts the coverage-off semantic — removing any member fails the suite at build.

**The same rule extends to `testing.F` (2026-07-20).** A `Fuzz*` declaration is classified disclosed-unsupported in the manifest exactly as a benchmark is (`testConversion.go` already emitted the `fuzz`/"deferred to Phase 4D" entry), but its converted body still compiles into the test assembly — and `F` simply did not exist, so math/big's `func FuzzExpMont(f *testing.F)` (nat_test.go) failed the whole package build with CS0426 *the type name 'F' does not exist in the type 'testing_package'*. `F` now mirrors `B`: a compile-only struct whose members are safe non-throwing no-ops, with explicit `ж<F>` overloads on the params-taking members. Its member set is Go 1.23's full public surface for `*testing.F` — the `TB` members it inherits from the embedded `common`, plus its own `Add` and `Fuzz` — declared complete under the same anti-drift rule as `TB` above rather than trimmed to today's callers. `Fuzz` takes a **`System.Delegate`**: a Go fuzz target's signature is arbitrary (`*testing.T` followed by the fuzzed argument types), and the converted body is an explicitly-typed lambda, so C# infers its natural `Action<…>` and converts — no per-arity overload set is needed. Nothing is invoked and no seed corpus is retained, because there is no fuzzing engine to consume either. This is not math/big-specific: roughly seventeen stdlib packages ship fuzz targets (archive/tar, archive/zip, compress/gzip, encoding/csv, encoding/json, html, image/{gif,jpeg,png}, net/netip, time, syscall, …), every one of which would hit the identical build blocker. At Go 1.24.13 the set follows 1.24's surface: `Chdir` and `Context`, which Go 1.24 added to `TB` and `F` inherits from `common`, joined it (`src/core/testing/testing.cs:589-591`).

## Realizing an asm-backed arch layer with managed hardware intrinsics

Hand-owning an asm-backed declaration does not have to mean stubbing it. Where .NET exposes the *same* instructions the `.s` file issues — via `System.Runtime.Intrinsics` — the architecture layer can be ported for real, and the converted package gains genuine hardware acceleration rather than a fallback. **`hash/crc32` is the first of its kind (2026-07-24) and sets the pattern.**

Go's `crc32_amd64.go` declares three functions with no body — `castagnoliSSE42`, `castagnoliSSE42Triple` (the SSE4.2 `CRC32` instruction) and `ieeeCLMUL` (PCLMULQDQ carry-less multiply folding) — implemented in `crc32_amd64.s`. Converted literally they become bodyless `partial`s that the [`PartialStubGenerator`](source-generators.md#source-generators) fills with `NotImplementedException`, so `crc32.go` always took the slicing-by-8 fallback and the package's own `TestArchIEEE`/`TestArchCastagnoli` **skipped**. `crc32_amd64.cs` is hand-owned (`[module: go.GoManualConversion]`, whole-file) and the three functions are transcribed against `System.Runtime.Intrinsics.X86` — `Sse42.X64.Crc32` for the CRC32B/W/L/Q chain, `Pclmulqdq.CarrylessMultiply` + `Sse2` for the fold and Barrett reduction, `Sse41.Extract` for the final `PEXTRD`. Every other declaration in the file is the converted output verbatim.

Three rules make this a repeatable recipe rather than a one-off:

* **Probe capabilities LOCALLY; never flip `internal/cpu`'s global flags.** The Go guards read `cpu.X86.HasSSE42` / `HasPCLMULQDQ` / `HasSSE41`. Those flags are shared by **every** converted package's arch path, and the rest of those arch layers are still throwing stubs — setting them centrally would trade each package's working portable fallback for a `NotImplementedException`. The hand-owned file instead defines its own predicates over the `.IsSupported` properties of exactly the instruction sets it uses (`Sse42.X64.IsSupported`; `Pclmulqdq.IsSupported && Sse41.IsSupported`) and `archAvailable*` returns those. The claim then stays precisely true — "these instructions are available to *this* code" — and `internal/cpu` is untouched. When a probe is false, `crc32.go`'s own `archAvailable*` branch falls back to slicing-by-8, which is exactly how Go degrades on an architecture with no arch implementation.
* **Port the `.s` file, label for label, and say so.** The fold constants are the `.s` `DATA` pairs verbatim; a 128-bit load puts the offset-0 quadword in the low half, so `Vector128.Create(offset0, offset8)` reproduces each register image exactly. `PCLMULQDQ`'s `imm8` maps directly onto `CarrylessMultiply`'s `control` (bit 0 selects the *left* operand's quadword, bit 4 the *right*'s), so `PCLMULQDQ $0x11, X0, X1` is `CarrylessMultiply(x1, x0, 0x11)`. Each ported block carries the `.s` label it transcribes (`aligned`, `less_than_8`, `loopback64`, `remain64`, `remain16`, `finish`) so the two can be diffed by eye at upgrade time.
* **Divergences are deliberate, documented in place, and result-neutral.** The `.s` file walks the buffer to an 8-byte boundary before its `CRC32Q` loop; the managed port drops that step. It exists to align the loads, it cannot change the answer (CRC is a pure function of the initial value and the byte *sequence*, and the `CRC(I, ABC)` combining identity holds for any split), and it has no managed counterpart — a slice's backing array can be moved by the GC, so an address observed here is not an address the loads keep. Likewise the `.s` triple loop is a `DECQ`/`JNZ` do-while that would wrap on `rounds == 0`; the managed form is a counted loop, identical for every real input.

Note the csproj constraint this recipe was written under: the converter **regenerates each package's `.csproj` on every transpile**, and it sets `AllowUnsafeBlocks` from `usesUnsafeCode` alone -- which was `false` here, so a hand-owned file could not use `byte*`/`fixed`/`stackalloc` and any setting added by hand was clobbered on the next run. That is no longer the only option (a file may now [declare the requirement](#a-hand-owned-file-can-declare-that-it-needs-unsafe)), but this recipe is still the better answer where it applies: it needs no compiler flag at all. The loads go through `MemoryMarshal.GetReference(p.ToSpan())` plus `Unsafe.ReadUnaligned<T>` / `Unsafe.Add`, which need no compiler flag and read the slice's real backing window (offset included) with no copy.

The payoff is that the package's *own* test suite becomes the correctness oracle, which is what makes this pattern safe to repeat: `TestArchIEEE` and `TestArchCastagnoli` cross-check the intrinsics against the portable slicing-by-8 implementation over randomized buffers at 46 lengths chosen to straddle the `168*3=504` and `1344*3=4032` cutoffs, and enabling the arch path also routes `TestGolden`/`TestGoldenMarshal` through it against known vectors. All 8 `hash/crc32` Test functions match `go test`, with nothing skipped and no disclosed divergences (Phase-4 validated package; see `docs/Roadmap.md`). Confirmed by positive control — corrupting a fold constant and the `CRC32B` tail turns exactly the arch-dependent tests red while the portable-only ones stay green.

## A cross-package `//go:linkname` PULL emits a forwarder, not a throwing stub

A bodyless function carrying `//go:linkname <local> <pkgpath>.<func>` (a three-field directive naming another package) is a **PULL** — the function has no body of its own and links to another package's (often unexported) symbol. `golang.org/x/sys/windows`'s `LazyDLL`/`LazyProc` reach the Go runtime's DLL loaders this way:

```go
//go:linkname syscall_loadlibrary syscall.loadlibrary
func syscall_loadlibrary(filename *uint16) (handle Handle, err Errno)
```

Left as an ordinary bodyless declaration, it would emit a `partial` that the [`PartialStubGenerator`](source-generators.md#source-generators) turns into a **throwing** stub — dead DLL loading. The converter (`visitFuncDecl.go`) instead recognizes the directive and emits a **forwarder body** that calls the target, bridging any nominal `num:uintptr` type difference through `uintptr` (the linked signatures are structurally identical, so a mismatch is only between two such types):

```csharp
internal static (ΔHandle handle, Errno err) syscall_loadlibrary(ж<uint16> filename) {
    var (ᴛ1, ᴛ2) = syscall.loadlibrary(filename);
    return ((ΔHandle)(uintptr)ᴛ1, (Errno)(uintptr)ᴛ2);
}
```

Pointer/slice/string parameters (`filename`) pass through unchanged (the same golib type on both sides); a **uintptr-kind** parameter is passed `(uintptr)p` and a uintptr-kind result returned `(LocalType)(uintptr)r`. The bridge is scoped to uintptr-kind types (`uintptr` and named types whose underlying type is `uintptr`, e.g. `Handle`/`Errno`) — precisely the case where the two linked signatures name the same value under two different *nominal* C# types. A **sized** integer (`int32`/`int64`/…) is the same C# type on both sides and passes through bare; routing it through `uintptr` instead narrows it on 32-bit and does not even bind (`(uintptr)timeout` handed to an `int64` parameter — sync's `runtime.blockUntilEmptyFinalizerQueue` pull, CS1503).

The target alias is **whatever the importing file actually emitted** for the target package, not the bare last path segment: an explicit `import r "runtime"` is looked up in the file's recorded import aliases, and otherwise the canonical alias is taken through the same collision-rename the import machinery applies. That rename is not hypothetical — a file that pulls from `runtime` while any `go.runtime.*` namespace is in scope emits `using Δruntime = runtime_package;`, because a bare `runtime` alias would bind the **namespace**, and a forwarder spelled `runtime.<fn>` is then CS0234 (sync's `oncefunc_test.go`).

**Forwarding is gated on an explicit whitelist of hand-implemented targets** (`linknameForwardTargets` — `syscall.loadlibrary`/`loadsystemlibrary`/`getprocaddress`, the native P/Invokes in `core/syscall/dll_windows.cs`, plus `runtime.blockUntilEmptyFinalizerQueue`, the finalizer-queue drain that sync's and runtime's own tests pull and that `mfinal.cs` answers with `GC.WaitForPendingFinalizers`). This is not optional prudence: a linkname target is **indistinguishable at conversion time** from any other bodyless assembly/intrinsic Go function — `syscall.loadlibrary` and `runtime.reflectcall` are *both* bodyless `//go:` asm in Go — so only the whitelisted targets are known to have a real C# implementation to call. Every other linkname pull stays a bodyless stub, the pre-forwarder behavior: a method-receiver PUSH (`//go:linkname X reflect.(*rtype).Align`, reflect's `badlinkname.go` "pushes linknames of the methods"), a same-package pull (`//go:linkname unusedIfaceIndir reflect.ifaceIndir` inside reflect), and an unimplemented intrinsic (`//go:linkname call runtime.reflectcall`) would each otherwise emit an uncompilable forwarder (a nonexistent `reflect.(*rtype)`/`runtime.reflectcall` member, or a package alias that doesn't exist for the package's own name). Extend the whitelist when a new native linkname target gains a hand-written C# implementation — and remember the **accessibility** half: Go's linkname crosses the package boundary the way C# `public` does, so an unexported target (which the exported-ness rule emits `internal`) is invisible to the forwarder in the pulling *assembly* and must be widened where it is declared (`runtime.blockUntilEmptyFinalizerQueue` is `public` in the hand-owned `mfinal.cs` for exactly this reason). Guarded by `TestRecurseLinknameForwarder` (asserts the whitelisted `syscall.loadlibrary` forwarder body + the uintptr result bridge, and that a non-whitelisted `runtime.reflectcall` target stays a stub).

**A whitelisted target may be ORDINARY CONVERTED GO, and then the converter widens it itself.** Every
entry above answers a *hand-written* body. `time.registerLoadFromEmbeddedTZData` is the first that does
not: `time/zoneinfo_read.go` declares it with a real Go body, `time` authorizes the pull with the
matching one-arg handle, and `time/tzdata`'s `init()` calls it — the only edge between the two packages,
since `time/tzdata` imports `errors`, `syscall` and `unsafe` and never `time`. Two consequences follow
from that, and both are general:

* **The accessibility half is no longer a hand-own's responsibility.** `packageFuncAccess` emits a
  package-level free function `public` when it carries a one-arg linkname handle **and** its
  `<pkgpath>.<name>` is a forward target. The handle alone is deliberately not enough — Go 1.23 carries
  340 of them outside `cmd/`, and publicizing every one would widen the corpus's whole surface for pulls
  that are never emitted. The whitelist is the converter's own record of which pulls actually become a
  call, so gating on it moves exactly the symbols that need to move.
* **The forwarder must be able to name a package the file never imports.** The pull queues the target's
  path for a **project reference** (as the var-pull arm already did) and, when the file carries no import
  spec for it, emits the call **fully qualified** — `go.time_package.registerLoadFromEmbeddedTZData(…)`,
  which resolves inside `namespace go;` with no alias. A bare `time.` would have bound the `go.time`
  CHILD namespace — tzdata's own — rather than the package class (CS0234).

Until this landed, a blank `import _ "time/tzdata"` threw `NotImplementedException` out of a **module
initializer** and took the whole program down before `main`, which is exactly what the blank-import
`init` forcing made reachable. It also turns out to be what lets `time`'s test suite load zone data at
all: with tzdata registered, `loadLocation` falls back to the embedded database when the GOROOT
`lib/time/zoneinfo.zip` path a test hard-codes does not resolve from the C# host's working directory.

**A linkname target implemented as a golib BUILTIN forwards to a bare, unqualified call** (`linknameForwardBuiltins`, a sibling map of Go linkname target → golib builtin name). Some Go compiler intrinsics live in `runtime` and are linked into another package by symbol, but their go2cs implementation is a golib builtin — in scope UNQUALIFIED via each converted project's `using static go.builtin`, so the forwarder emits `<builtin>(args)` with no package qualifier (an empty alias is the sentinel `writeLinknameForwarder` reads to drop the `<alias>.` prefix). The canonical case is **`maps.Clone`**: Go implements its worker as `runtime.mapclone` (`//go:linkname mapclone maps.clone`) and the `maps` package pulls it as a bodyless `func clone(m any) any` carrying `//go:linkname clone maps.clone` — a *same-package-named* target (`maps.clone`) whose real definition is elsewhere, so it is neither a native whitelist target nor a normal pull, and was left a **throwing** `PartialStubGenerator` stub (every `maps.Clone`/`maps.Copy`/`maps.DeleteFunc` test threw `NotImplementedException: clone: external (assembly or cgo) function is not implemented`). It now forwards:

```csharp
internal static any clone(any m) {
    return mapclone(m);
}
```

`builtin.mapclone(any m)` is Go's `runtime.mapclone` at golib level: it recovers the boxed map's concrete key/value types through `IMap.CloneMap()` (a default interface method on `IMap<TKey, TValue>`, so both the concrete `map<K, V>` and the generated named-map wrappers get it with no source-generator change — no reflection) and returns a fresh `map<K, V>` populated from the source's entries. The clone's backing `Dictionary` is **independent** — Go's shallow clone (keys/values copied by ordinary assignment), so mutating the clone never touches the original — and a nil map clones to nil. This is what carries the `maps` package to full Phase-4 validation (14/14 tests vs `go test`; the 6 Clone/Copy/DeleteFunc tests previously threw). Extend `linknameForwardBuiltins` when another linkname intrinsic gains a golib builtin. Guarded by the `MapCloneLinkname` behavioral test — the exact `//go:linkname clone maps.clone` shape in a `main` package, cloning a `map[string]int`, mutating the clone (overwrite/add/delete) and asserting the original is unchanged, output-compared vs `go run`; proven to emit the throwing stub against the un-fixed converter.

**A pure-JMP ASSEMBLY TRAMPOLINE takes the same forwarder** (`asmTrampolines.go`, 2026-09-24). A bodyless
function whose platform-selected `.s` block holds ONE instruction, a jump to a Go function, is Go's own
statement that the two are one function under two names. golang.org/x/sys/unix builds `Syscall`,
`Syscall6`, `RawSyscall` and `RawSyscall6` that way (`JMP syscall·Syscall(SB)`), and before this rule each
was a throwing stub that go-isatty reached inside fatih/color's type initializer:

```csharp
public static (uintptr r1, uintptr r2, syscall.Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}
```

The rule is deliberately narrow. The block is selected with the conversion's own build context (target
platform, `-tags`, the loader toolchain's release tags, and `cgo` only when `-tags` names it, as for Go
files); a block containing a preprocessor conditional, or a block with any other instruction (x/sys/unix's
`SyscallNoError` issues a raw `SYSCALL`), keeps its stub. A jump
proves an identical FRAME and nothing more, so the local and target signatures must be
`types.Identical`: x/sys/unix's `gettimeofday` jumps to `syscall·gettimeofday` but takes its OWN
`*Timeval`, and forwarding it would be CS1503 that stops the whole package building. A cross-package target
must be exported (nothing widens the target package's surface) and must live in a package the trampoline's
package imports DIRECTLY, since only a direct import carries the type information the signature check
reads; a same-package target whose parameters
Phase A lowered to `ref` keeps its stub. It applies only OUTSIDE the converted standard library: the
corpus's own trampolines (internal/runtime/atomic and the hand-owned sync/atomic) are governed by hand-owns
and the stub census, and a forwarder there would collide with a hand-owned partial.

## A cross-package `//go:linkname` PUSH resolves per recorded disposition — forwarder or announced panic

The PULL above is one of two directions, and the converter long handled only that one. A **PUSH** runs the
other way: the *defining* package carries the body and names ANOTHER package's declaration as the symbol it
defines, while the consuming side is an ordinary bodyless func under a **one-argument** `//go:linkname
<thisFunc>` handle. `runtime/mgc.go` pushes into `unique`, `runtime/mheap.go` into `internal/weak`:

```go
// unique/handle.go — the CONSUMER: bodyless, one-arg handle (Go's authorization for the push)
//go:linkname runtime_registerUniqueMapCleanup
func runtime_registerUniqueMapCleanup(cleanup func())

// runtime/mgc.go — the PUSHER: an ordinary body naming the consumer's symbol
//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup
func unique_runtime_registerUniqueMapCleanup(f func()) { … }
```

Nothing linked the two, so the consumer's declaration fell to the [`PartialStubGenerator`](source-generators.md#source-generators)
and threw on first call — `unique.Make`'s `setupMake.Do(registerCleanup)` took `net/netip`'s initializer and
`encoding/gob`'s `TestNetIP` with it. The consuming side now resolves to one of **two** emissions, chosen by a
disposition recorded per pair in `linknamePushTargets` (`linknameOperations.go`), keyed by the consumer's own
`<pkgPath>.<symbol>`:

```csharp
// unique/handle.cs — FORWARDED: the pushed body is ordinary converted Go, so call it
//go:linkname runtime_registerUniqueMapCleanup
internal static void runtime_registerUniqueMapCleanup(Action cleanup) {
    Δruntime.unique_runtime_registerUniqueMapCleanup(cleanup);
}

// internal/weak/pointer.cs — UNHONORABLE: announce the pair, never fabricate a body
//go:linkname runtime_registerWeakPointer
internal static @unsafe.Pointer runtime_registerWeakPointer(@unsafe.Pointer _) {
    throw panic("go2cs: //go:linkname push runtime.internal_weak_runtime_registerWeakPointer -> internal/weak.runtime_registerWeakPointer is not honored: the pushed body walks mheap_ span metadata the managed model does not populate; internal/weak wants a hand-owned managed weak reference");
}
```

The accessibility half mirrors the pull's: a forwarder calls the pushing definition **across an assembly
boundary**, so `packageFuncAccess` emits that definition `public` from the reverse index `linknamePushSources`.
The pull arm requires the target's own one-arg handle as Go's authorization; a push carries its authorization on
the **pushing** side instead, so this arm reads the registry alone. Reaching the
pusher can be the only edge to its package, so the path is queued for a project reference exactly as the pull
queues its target (`linknameTargetAlias`, now shared by both arms, also resolves the file's actual using-alias —
`unique/handle.cs` spells `Δruntime`, not `runtime`).

**The consumer has TWO shapes, and a row records which one it is** (`bareDecl`, added 2026-08-08). The example
above is the **handle** shape — a one-arg `//go:linkname <thisFunc>` above the bodyless declaration, Go's modern
way of opening a symbol to a push, used by `unique` and `internal/weak`. The standard library also pushes into a
**bare** shape that predates that convention: a bodyless declaration with *no directive at all*, carrying only a
prose comment saying where the body lives. `syscall` and `os` still use it, and it is every bit as legal:

```go
// syscall/env_unix.go — the BARE consumer: no directive of its own
func runtime_envs() []string // in package runtime

// runtime/runtime.go — the PUSHER, where this pair's only directive lives
//go:linkname syscall_runtime_envs syscall.runtime_envs
func syscall_runtime_envs() []string { return append([]string{}, envs...) }
```

The matcher originally required the handle unconditionally (it returned early on `funcDecl.Doc == nil`, before
even reading the registry), so the bare shape could never resolve — it fell to the `PartialStubGenerator`, and
because `syscall.envs` is a package-level var *initialized* from that call, the throw came out of `syscall`'s
type initializer and took `os.init()`, and every Linux program that so much as touches `fmt`, with it. Nothing
on Windows could see it: `env_unix.go` is `//go:build unix || (js && wasm) || plan9 || wasip1`, so that
declaration does not exist in the Windows corpus, and the registry's original census was taken against a
Windows-only emission.

The shape is **recorded per row rather than inferred**, so the match fails closed in both directions: a handle
row will not forward a bare declaration, a bare row will not forward one carrying a handle, and a two-arg
directive (a PULL — a different mechanism) is rejected by both arms. Neither shape is verifiable from the
consumer's own syntax, which is the whole reason this registry is curated, so the shape belongs in the same
recorded judgment as the disposition (`linknamePushDeclMatches`, `visitFuncDecl.go`). The `syscall.runtime_envs`
row forwards honorably rather than plausibly: `runtime.envs` really is populated in the managed model, by the
hand-owned `runtime/goenvs_impl.cs` module initializer, so the forwarder hands back the real process
environment. Guarded two ways — `TestRecurseLinknamePush` carries the 2x2 over shape (`unauthorized` and `bare`
are *syntactically identical* and differ only in what their row records, which is what proves the recorded shape
is what decides), and `TestLinknamePushRegistryMatchesGoSource` checks every row against the real Go source in
GOROOT: the consumer's declaration exists, is bodyless, matches the recorded shape, and the pushing side really
does carry the two-arg directive the row vouches for.

**Why a curated registry rather than general detection.** The same reason the pull whitelist exists, plus one
more that is structural: **the converter never sees the pushing package's directives while converting the
consumer.** A package is converted from its own syntax; its dependencies contribute types, not comments — and
the pusher is not even guaranteed to be a dependency. So a bodyless one-arg-handle func is indistinguishable at
conversion time from an ordinary assembly stub, and the disposition has to be recorded. Go 1.23 carries ~200
pushes outside `cmd/`; the converted corpus exposes **eleven** of them as bodyless one-arg-handle declarations,
and linking those wholesale would be a regression dressed as a feature — `time`'s timer trio is already answered
by `time_impl.cs` and a converter-emitted body would collide with it, while `internal/syscall/windows`'s
`stdcall` wrappers and `internal/coverage/cfile`'s linker-section walk push bodies the managed model cannot run
at all. Each entry is therefore a judgment, recorded with its reasoning beside the key.

**The unhonorable arm is the inverse-atomic rule in emission form.** `internal/weak`'s two halves reach the span
allocator (`registerWeakPointer` → `getOrAddWeakHandle` → `spanOfHeap` → `throw("getWeakHandle on invalid
pointer")`; `makeStrongFromWeak` re-derives an object pointer from a heap address). A forwarder there would
either fault or — far worse — hand back a plausible-looking pointer derived from garbage, which is exactly the
"populate a field whose read cannot be honored" move the rule forbids. The panic is deliberately a **Go panic
naming both halves of the pair and the reason**, not the generator's `NotImplementedException`: the declaration
is not an unimplemented assembly stub, it is a real Go contract this conversion has decided it cannot keep, and
the first caller to hit it should land on the hand-own the row actually needs. That hand-own has since
LANDED — see [*`internal/weak.Pointer`*](#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one)
below — so these two rows no longer describe the deployed corpus, where `pointer.cs` is marked and never
regenerated; they describe what a conversion into a root that does *not* already carry the hand-own emits,
which must still be the loud pair. The reason string now names the file to reach for.

By contrast `unique`'s pushed body is **ordinary converted Go** — it makes a `chan struct{}` and starts a
goroutine that drains it and calls the callback — so the managed model runs the real thing: the registration
succeeds and the cleanup goroutine parks on the channel. Nothing signals it, because the converted runtime's
`clearpools()` is driven by Go's own GC, which does not run. That is Go's OWN behavior for a program whose GC
never fires (the intern map simply keeps its entries), not a fabricated answer — the distinction the two arms
turn on.

Guarded by `TestRecurseLinknamePush`, which runs the real `-recurse` converter over a two-package module fixture
with its dispositions injected for the test's duration, and asserts all four arms: the forwarder body, the
pushing definition's publicization, the pair-naming panic, and that both an unregistered handle and a registered
declaration *without* a handle stay bodyless stubs. (A behavioral project cannot reach this mechanism — the
registry is keyed by the consumer's import path, and the behavioral harness converts each package alone, so
neither a fixture path nor cross-package discovery is available to it. The pull forwarder is guarded at the same
layer, and for the same reason, by `TestRecurseLinknameForwarder`.)

**A push whose consumer is an EXTERNAL TEST package needs no new machinery — the key simply spells the test
package path** (2026-08-12). `runtime/metrics.go` pushes its test-only name reader into the package's own
test suite:

```go
// runtime/metrics/description_test.go — the CONSUMER, in package metrics_test (handle shape)
//go:linkname runtime_readMetricNames
func runtime_readMetricNames() []string

// runtime/metrics.go (package runtime) — the PUSHER
//go:linkname readMetricNames runtime/metrics_test.runtime_readMetricNames
func readMetricNames() []string { … }
```

The `-tests` conversion's `convertTestVariant` resets package state from the external variant's own
`packages.Package`, so `currentPackagePath` is already `runtime/metrics_test` while its files convert — the
registry row `"runtime/metrics_test.runtime_readMetricNames"` matches through the exact code path every
production consumer uses, and the emitted forwarder
(`return global::go.runtime_package.readMetricNames();`) replaced the throwing stub that held the package's
`TestNames` at 1 of 2. Any production package pushing into its own `_test` package takes the same shape. The
registry guard learned the location half: an external test package has no GOROOT directory of its own, so
`TestLinknamePushRegistryMatchesGoSource` resolves `<base>_test` to the base package's directory and scans
its `_test.go` files (only those whose package clause carries the `_test` suffix — in-package test files
belong to `<base>` itself).

The same package's `Read` entry point is the registry's second **measured** unhonorable row, and the
measurement is worth recording: a forwarder was tried first, and the pushed body ran — through the
hand-owned managed `metricsLock` — all the way to `readMetricsLocked`'s slice-header reconstruct
(`*(*[]metricSample)(unsafe.Pointer(&sl))` over a raw first-element address), which no managed pointer can
alias: the fabricated slice read garbage `@string` names. The deployed corpus routes around the seam instead
of through it — `runtime/metrics/sample.cs` is hand-owned and its `Read` marshals names in and computed
`(kind, scalar, pointer)` out through the public `runtime.readMetricsManaged` shim (`managed_impl.cs`, the
`registerPoolCleanup` pattern), preserving the batch semantics while the metrics table and every compute
closure stay auto-converted — so the `runtime_readMetrics` row exists for a conversion into a root WITHOUT
the hand-own, where the bodyless declaration reappears and must announce the wall rather than fabricate past
it. `runtime/metrics` validates 2 of 2 on this arrangement.

**`os/signal`'s six primitives — the push that needed an OS EDGE and a real BLOCK before it could be
honored** (2026-08-14). `os/signal` declares six symbols the runtime defines, all bare shape: five under one
`// Defined by the runtime package.` comment in `signal_unix.go` (whose build constraint includes windows),
and `signalWaitUntilIdle` in `signal.go`. `runtime/sigqueue.go` carries the matching two-argument directives.
Every one was a throwing `PartialStubGenerator` stub, which is what put `os/exec`'s `TestWaitInterrupt` rows
on the board: the `hang` helper's `signal.Ignore(os.Interrupt)` died with
`NotImplementedException: signal_ignore`.

The rows themselves are unremarkable — six `{source: "runtime.signal_…", bareDecl: true}` entries, and the
first forwarders with a BLANK parameter, which emit `signal_disable(_)` against the parameter the signature
already names `_`. What the row set is worth recording for is the `GetSystemDirectory` precedent applied
twice over, because the pushed bodies bottomed out in two dead ends rather than one:

1. **Nobody was queueing.** Go arms Windows signal delivery in `osinit` with
   `stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1)`, and neither half runs in the managed model —
   `osinit` is emitted already marked not-run, `stdcall` bottoms out in `asmstdcall`, and `compileCallback`
   builds a native thunk out of generated assembly. So the converted `ctrlHandler` was reachable code nobody
   called, and a forwarder alone would have made `signal.Notify` succeed and then never deliver.
2. **Nobody could wait.** `signal_recv` blocks in `notetsleepg(&sig.note, -1)`, whose Go prologue is
   `getg()` — still an unimplemented intrinsic — so the receive loop threw before it reached the note.

Both were closed in the same change, and the shape of each closure is the point. The OS edge is a hand-owned
`runtime/windows/signal_windows_impl.cs` that registers a real `SetConsoleCtrlHandler` whose
`[UnmanagedCallersOnly]` callback calls the **converted** `ctrlHandler` — it reimplements no mapping, no
bitset and no state machine, so `sigsend`, `signal_recv` and the `{sigIdle, sigReceiving, sigSending}` CAS
protocol all stay auto-converted in `runtime/sigqueue.cs`. `notetsleepg` joins the mutex/note family in
`manualConversionFuncs` and gains the family's only genuinely BLOCKING body
(`runtime/lock_managed_impl.cs`): a `Monitor.Wait` against a gate that `notewakeup` pulses after it flips the
key, rather than the `SpinWait` escalation its siblings use, because its two callers — `signal_recv` and
`profbuf`'s reader — idle indefinitely and polling either would burn a thread forever to observe nothing.
No park-accounting scope is taken: `DESIGN-cooperative-scheduler.md` §6 row 11 places the note protocol below
the goroutine model by ruling, and golib's `Goroutine.Park` is design-only regardless.

**Faithfulness is the whole mechanism, including where it looks wrong.** With both halves landed the pushed
bodies run end to end and Go's own Windows semantics fall out unaltered. `sigenable`/`sigdisable`/`sigignore`
really are empty on Windows — `signal_windows.go` says so under a literal "Following are not implemented" —
so the `wanted` bitset is the only gate there is: `Notify` makes ^C and ^BREAK deliver `os.Interrupt` and the
program survive, `Stop`/`Reset` restore the default, and `Ignore` clears `wanted` and sets `ignored`, which
leaves ^C **terminating the process** while `Ignored()` truthfully answers `true`. That last one reads like a
defect and is not: `os/signal`'s own `doc.go` documents only `Notify`, `Reset` and `Stop` under "# Windows",
because `Ignore` has no console-event lever to pull. It is Go's behavior, not a go2cs declared limit, and
reproducing it rather than "fixing" it is the requirement. Signals with no Windows source are the same story
— `ctrlHandler` can only ever produce `SIGINT` and `SIGTERM`, so a `Notify` for anything else simply never
fires, in Go and here alike. `Reset` not clearing the `ignored` bit is a third such subtlety, and the
`SignalPrimitives` behavioral test pins all of them against `go run` — including, indirectly, that
`signal_recv` really parks, since `Stop` blocks in `signalWaitUntilIdle` until the watcher goroutine
has reached `sigReceiving`.

Delivery itself was measured out of band, in `TestCtrlBreak`'s own shape: a child started with
`CREATE_NEW_PROCESS_GROUP` and sent `GenerateConsoleCtrlEvent(CTRL_BREAK_EVENT, childPid)`, run
against the converted child and against a native Go child built from the same source. Both print
`ready` then `got: interrupt` and exit 0. One trap that probe is worth remembering for: sending the
event on a fixed timer races a cold Debug child carrying the whole converted runtime closure, and
losing that race looks alarming rather than obviously benign — the child dies with `0xc000013a`
(STATUS_CONTROL_C_EXIT) and **no output at all**, because it had not yet reached `signal.Notify`, so
the `wanted` bit was unset, `sigsend` correctly returned false, the default handler killed it, and
its buffered stdout went with it. That is the machinery working exactly as specified. Wait for the
child to announce readiness rather than sleeping on a timer.

**`net.newUnixFile` — the row where the precondition was ALREADY true, and where the tempting substitution is
the wrong answer** (2026-08-28). `os/file_unix.go` pushes a hidden `*os.File` constructor into `net`:

```go
// os/file_unix.go (package os) — the PUSHER
//go:linkname net_newUnixFile net.newUnixFile
func net_newUnixFile(fd int, name string) *File {
	if fd < 0 {
		panic("invalid FD")
	}
	return newFile(fd, name, kindSock, true)
}

// net/fd_unix.go — the CONSUMER, bare shape
// Defined in os package.
func newUnixFile(fd int, name string) *os.File
```

Every other honorable row in this registry had to answer "what else must move before the forwarder is honest?"
— Go's `osinit` had not filled `sysDirectory`, `argslice` was empty, `metricsLock` could not be taken.
**Here nothing else had to move.** `os.newFile` is the same function `os.NewFile` and `os.Pipe` reach; it is
exercised on every row that opens anything, and the `kindSock` path differs from the `kindPipe` path only in
skipping the `SetNonblock` call — the descriptor is already non-blocking — while still registering with the
poller. So the row is one line, the forwarder is `return os.net_newUnixFile(fd, name);`, and
`packageFuncAccess` widens the pushing definition to `public` from the reverse index. No new project reference
and no cycle: `net` already imports `os` (`fd_unix.go`'s own `dup()` calls `os.NewSyscallError`), and `os`
imports no part of `net`.

**Why `os.NewFile` is not a substitute, which is what makes this a registry row rather than a hand-patch in
`net`.** `kind` is precisely the distinction Go's own comment on `net_newUnixFile` exists to preserve:
`kindSock` sets `f.nonblock = true` so a later `Fd()` hands back a **blocking** descriptor — the historical
behavior `net.conn.File` callers depend on — while `kindNewFile` on an already-non-blocking descriptor leaves
it non-blocking. Substituting would compile, run, and quietly change the descriptor's mode: a
plausible-looking wrong answer, which is the failure this project rules against. The seam is also why the
other tempting framing — "emit `InternalsVisibleTo` so the puller can see the private symbol" — is not reached
for: the push machinery already crosses the assembly boundary by publicizing exactly the one symbol Go opened
with its directive, rather than widening a package's whole private surface to another assembly.

What the stub was costing on Linux: `(*net.TCPListener).File()` → `netFD.dup()` bottoms out here, so
`os/exec`'s `TestExtraFilesRace` — which builds its `ExtraFiles` out of listener files — died on the
`PartialStubGenerator` throw as an infrastructure-error, the last named residual of that row. Windows never
surfaced it: `net/fd_unix.go` is `//go:build unix` and `os/file_unix.go` is
`//go:build unix || (js && wasm) || wasip1`, so neither declaration exists there at all — the same
platform-blindness that hid `syscall.runtime_envs` and `runtime.fcntl`. Guarded by
`TestLinknamePushRoutesNetNewUnixFile`, which re-derives both halves of the pair from GOROOT and then asserts
the routing, the honorable disposition, and — exercising `packageFuncAccess` rather than the index it reads —
the publicization that makes the cross-assembly call compile.

## A `//go:cgo_import_dynamic` trampoline gets a RECORD, so its address can be resolved rather than invented

`abi.FuncPCABI0(f)` asks for the program-counter of `f`, and the darwin syscall layer asks it of a
**trampoline**: a bodyless `func libc_getgroups_trampoline()` whose real body is one assembly
instruction jumping to a dynamically-imported C symbol. Converted literally the declaration is a
bodyless `partial` the [`PartialStubGenerator`](source-generators.md#source-generators) fills with a throw, and
`FuncPCABI0` itself answered `return default` — a zero that is *plausible*, unique and stable, and
fatal the moment `rawSyscall` jumps to it.

The two halves of `FuncPCABI0` want opposite things, and that is the whole design. A PC read BACK —
`runtime.Callers`, pprof, `textAddr` — wants a synthetic token that symbolizes and is never
dereferenced. A trampoline wants a REAL, callable address. So the converter publishes a
discriminator: one assembly attribute per pragma it can bind, in a `<CgoDynamicImports>` section of
`package_info.cs`.

```go
// crypto/x509/internal/macos/corefoundation.go
func x509_CFDataCreate_trampoline()

//go:cgo_import_dynamic x509_CFDataCreate CFDataCreate "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
```

```csharp
// crypto/x509/internal/macos/darwin/package_info.cs
[assembly: go.GoCgoImportDynamic("x509_CFDataCreate_trampoline", "CFDataCreate", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
```

`golib`'s `GoCgoDynamicImports` reaches the record from the ARGUMENT — the delegate's method, its
declaring type, that type's assembly — so no package publishes a registry, no initialization order
matters, and a package that declares no trampolines contributes nothing. A stub **with** a record is
class B and resolves through `NativeLibrary`; the same stub **without** one is class C — Go's own
assembly (`goexit`, `asyncPreempt`, `sigtramp`) — and stays a loud throw. A record that exists but
fails to resolve throws too, naming the symbol and library: "there is no record" and "the record is
wrong" are different answers, and collapsing them would let a typo read as class C.

**Three properties are measurements, not preferences** (Go 1.23.12, all 1650 `//go:cgo_import_dynamic`
records outside `cmd/` and `vendor/`):

* **The gate is that the library argument is an ABSOLUTE PATH.** Every darwin record names one
  (`/usr/lib/libSystem.B.dylib`, `/usr/lib/libresolv.9.dylib`, and the two
  `/System/Library/Frameworks/…` frameworks `crypto/x509/internal/macos` imports); every other
  platform names a BARE library — windows' 51 `kernel32.dll`, openbsd and solaris' `libc.so`, aix's
  `libc.a/shr_64.o` — or names none at all, as `runtime/race`'s 196 darwin records do. Selecting on
  the leading slash and selecting on "`.dylib` or a framework path" are two independent derivations
  of the same **345** records and they agree on every one, which is what makes the shape safe to read
  instead of a list a later Go release could add to. A `.dylib`-SUFFIX gate would be wrong in a way
  that is easy to miss: it drops exactly the 28 framework records.
* **The binding rule is `trampoline == local + "_trampoline"`, and its boundary is a package.** It
  holds for **297 of 297** declarations outside `runtime` and **0 of 43** inside it, where 37 bind on
  the SYMBOL instead (`pthread_attr_init_trampoline` ← `libc_pthread_attr_init pthread_attr_init`)
  and 6 — `osinit_hack`, `exit`, `nanotime`, `walltime`, `sigprocmask`, `raiseproc` — carry no darwin
  pragma at all. Those 6 are Go's own assembly and are correctly class C. The other 37 mint nothing:
  their correspondence lives in the `.s` file the converter does not read, and a rule that stripped
  `_trampoline`, then optionally stripped `libc_`, then matched the symbol would cover 334 of 340 and
  guess at the rest.
* **The `libc_<sym>` naming is a MAJORITY, not a rule.** `local == "libc_" + symbol` holds in **312**
  of the 345; the other 33 are 28 `x509_<sym>` (`crypto/x509/internal/macos`), 3 `libresolv_<sym>`,
  one `libc<sym>` with no underscore, and one genuine outlier — `libc_error` / `__error` — which is
  the single record in all 345 where the local does not even END with the symbol. That last row is
  why a name-derived cross-check between the pragma and the trampoline is not worth having: it would
  be correct 344 times and silently wrong once.

The emission is therefore **173 records per darwin target** — 126 `syscall`, 28
`crypto/x509/internal/macos`, 19 `internal/syscall/unix` — and the section is created **only when a
package has records**, which is the one deliberate departure from the
`<GoSourcePositionMaps>` section's "always emitted, so absence never has to be told apart from
emptiness". That reasoning holds where every package converts source files; here the population is
four packages of the corpus and darwin-only, so an unconditional section would put marker lines into
every `package_info.cs` of every platform flavor to record a property absent from ~99% of them — and
it makes the windows and linux emissions byte-identical **by construction** rather than by a diff
that happens to come back empty (both measured at 0 differing paths).

Both conversion drivers run the pre-pass. A `-tests` conversion recompiles the production sources
into the test assembly, so that assembly declares the same trampolines and needs the same records;
and because the section is rewritten from what the current run bound, a driver that never binds would
not merely skip the records — it would EMPTY a section the `-stdlib` emission had populated.
`TestBothDriversCollectCgoDynamicImports` pins that.

## `internal/concurrent.HashTrieMap` — a managed map where Go seeds itself from `MapType().Hasher`

> **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 surface it was written against).**
> The package moved to `internal/sync`, and the hand-own is now
> [`src/core/internal/sync/hashtriemap.cs`](../../src/core/internal/sync/hashtriemap.cs), whose header
> records the 1.24.13 surface: `NewHashTrieMap` is gone and the zero map is seeded by `init`/`initSlow`
> on first touch (still from `abi.TypeOf(m).MapType()`'s `Hasher`); `V` widened to `any`, so
> `CompareAndSwap` and `CompareAndDelete` panic up front for a non-comparable `V`; `keyEqual` is gone;
> seven methods were added (`Clear`, `CompareAndSwap`, `Delete`, `LoadAndDelete`, `Range`, `Store`,
> `Swap`); and `internal/sync` is no longer fully hand-owned, because `mutex.go` and `runtime.go`
> convert. At 1.24 the same map also backs every `sync.Map` by default (`goexperiment.synchashtriemap`).

`internal/concurrent` is the whole of `unique`'s storage, and `unique` is `net/netip`'s address interner —
so this one type sits in front of `unique`'s entire suite, `net`'s last package-initializer root and
`encoding/gob`'s `TestNetIP`. Go 1.23's implementation is a lock-free hash-trie, and **every bit of its
behavior comes from one runtime descriptor read**:

```go
func NewHashTrieMap[K, V comparable]() *HashTrieMap[K, V] {
	var m map[K]V
	mapType := abi.TypeOf(m).MapType()
	ht := &HashTrieMap[K, V]{
		root: newIndirectNode[K, V](nil), keyHash: mapType.Hasher,
		keyEqual: mapType.Key.Equal, valEqual: mapType.Elem.Equal,
		seed: uintptr(rand.Uint64()),
	}
	return ht
}
```

`Hasher` is a raw function pointer into the hashing machinery the compiler emits for `map[K]V`;
`Key.Equal`/`Elem.Equal` are its matching bit-compare thunks. All three take `unsafe.Pointer`s and mean
*"hash / compare the bytes AT this address"* — and **the managed reflection bridge cannot honor that
contract**. An address in the CLR names no value: two boxes holding equal strings sit at different
addresses, and a pointee containing references moves across a GC. An address-derived hash would therefore
stop `unique.Make("hello")` agreeing with itself, which is the exact inverse of the package's purpose.

Populating `Hasher` anyway — with anything plausible — is barred by the **inverse of the atomic rule**: a
descriptor field whose read cannot be honored must stay EMPTY, because a half-populated descriptor converts
a loud construction failure into a map that is silently wrong. (Reflection increment 8 rooted the row there
and reported it *not landable in the bridge*.) So the literal conversion compiles and can never run:
`NewHashTrieMap` threw inside the package initializer of every `unique` consumer, taking `net/netip` and
every dependent with it.

**The ruling (2026-08-03) is the `sync` precedent applied one level up: hand-own the whole file, and keep
the SEMANTICS rather than the mechanism.** `sync`'s Mutex/RWMutex/WaitGroup are reimplemented on
`SemaphoreSlim`/monitors because Go's sleeping semaphore is co-designed with the state machine and cannot be
emulated; here the coupling is to the descriptor surface instead of to the scheduler, but the fork is the
same one — the raw-metal arm of the S1 fork (see
[`Baseline-vs-FullConversion.md`](../../src/archived/Baseline-vs-FullConversion.md)). `src/core/internal/concurrent/hashtriemap.cs`
carries `[module: go.GoManualConversion]` and contains **no trie at all**. The exported API and its
concurrency contract are preserved exactly; the store is a `ConcurrentDictionary`, whose guarantees line up
member for member:

| Go member | Managed mechanism | Semantic note |
|:--|:--|:--|
| `NewHashTrieMap[K, V]()` | `Ꮡ(new HashTrieMap<K, V>(store: new mapStore<K, V>()))` | the store is a CLASS, so a by-value copy of the struct shares one map — exactly what Go's `root *indirect[K,V]` pointer gives |
| `(*HashTrieMap).Load(key)` | `TryGetValue`; miss returns `(*new(V), false)` as `@new<V>().ValueSlot` | `[GoRecv]`, so the RecvGenerator still mints the `ж<…>` overload `unique` binds |
| `(*HashTrieMap).LoadOrStore(key, value)` | `TryGetValue` → `TryAdd` retry loop | exactly one caller of a racing set observes `loaded == false`; `GetOrAdd` is a single call but cannot report WHICH outcome occurred, and `unique.Make` depends on that answer |
| `(*HashTrieMap).CompareAndDelete(key, old)` | `ContainsKey` gate → `TryRemove(KeyValuePair)` | the pair overload is an atomic compare-and-remove under `EqualityComparer<V>.Default`; the gate reproduces Go's order (a missing key returns false *without* comparing values) |
| `(*HashTrieMap).All()` | closure over the store's enumerator | ConcurrentDictionary's enumeration is **weakly consistent** — never throws on concurrent mutation, visits each live key once, promises no order — which is Go's documented contract verbatim, and is what lets `unique`'s cleanup pass `CompareAndDelete` while it walks |
| zero `HashTrieMap` | `storeOf` lazily installs the store (`Interlocked.CompareExchange`) | Go 1.23's zero value is unusable (nil `root`/`keyHash`); nothing depends on that panic, and the same `gateOf` idiom `sync.Mutex` uses removes a whole class of null dereference |
| `keyHash` + `keyEqual` | `EqualityComparer<K>.Default` | see below — verified to BE Go's `==` for every key shape the corpus interns |
| `valEqual` | `EqualityComparer<V>.Default`, guarded by `mustBeComparable` | Go's value comparison panics for an INTERFACE `V` holding an uncomparable dynamic type (`V comparable` admits `any` since Go 1.20, moving the check to run time); the guard mirrors that panic instead of letting the comparer answer a question Go refuses to. Inert for a non-interface `V`, resolved once per instantiation |

**The equality/hash bridge is the correctness question, and it was measured, not reasoned.** Go hashes and
compares keys by K's own `==`; the managed implementation uses `EqualityComparer<K>.Default`. For every key
shape the converted corpus actually interns these agree:

* **`ж<T>`** (`unique`'s own `map[*abi.Type]any`) implements `IEquatable<ж<T>>` as pointer IDENTITY with a
  matching identity hash, and `abi.TypeFor<T>()` interns one descriptor box per `System.Type` — so one Go
  type always presents one key, and a second `TypeFor` call finds the first call's entry.
* **A `[GoType]` struct** — `net/netip`'s `addrDetail{isV6 bool; zoneV6 string}`, the shape `unique`
  actually interns — carries a generated field-wise `Equals` over `==` plus a `HashCode.Combine` of the
  same fields, which is Go's struct `==` exactly. It does **not** implement `IEquatable<T>`, so
  `EqualityComparer<T>.Default` routes through the `object` override; that lands on the same comparison, at
  the cost of one box per lookup.
* **`@string`** compares and hashes by CONTENT, as Go's string `==` does — verified with two keys built
  from distinct backing storage.

**Two further walls sit BEHIND this one**, both uncovered by making `unique` reachable for the first time
and both outside this file:

1. ~~**A cross-assembly `//go:linkname` PUSH never links.**~~ **CLOSED** — see
   [*A cross-package `//go:linkname` PUSH resolves per recorded disposition*](#a-cross-package-golinkname-push-resolves-per-recorded-disposition--forwarder-or-announced-panic)
   above. The forwarder machinery handled the PULL direction only (a bodyless declaration naming another
   package's symbol); `runtime` pushes the other way —
   `//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup`
   (`mgc.go`), `//go:linkname internal_weak_runtime_registerWeakPointer internal/weak.runtime_registerWeakPointer`
   (`mheap.go`) — and the *consuming* package's bodyless declaration was left for the
   [`PartialStubGenerator`](source-generators.md#source-generators) to fill with `NotImplementedException`. `unique`'s
   registration now FORWARDS to runtime's converted body; `internal/weak`'s two halves stay unlinked **by
   ruling** (the pushed bodies walk `mheap_` span metadata) and announce the linkname pair rather than
   fabricate one. The remedy they name — a hand-owned managed weak reference — has since landed; see
   [*`internal/weak.Pointer`*](#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one)
   below.
2. **`abi.TypeFor<T>()` is silently WRONG for an interface `T`.** Its non-interface branch returns an
   interned descriptor; the interface branch is `TypeOf((*T)(nil)).Elem()`, and `Type.Elem()` for
   `Kind == Pointer` reinterprets the descriptor as a `PtrType` (`Ꮡt.Reinterpret<Type, PtrType>()`) and
   reads `.Elem` — which under the managed layout lands on the descriptor's `Equal` field. `TypeFor<any>()`
   and `TypeFor<error>()` therefore return a `System.Func<unsafe.Pointer, unsafe.Pointer, bool>`, not a
   `ж<abi.Type>` at all. Shared generics let that object be *stored* into
   `ConcurrentDictionary<ж<abi.Type>, any>` without a cast check, and the first real key comparison then
   dispatches `IEquatable<ж<abi.Type>>.Equals` on a delegate → `EntryPointNotFoundException`. The old trie
   never dispatched anything on a key's runtime type (it compared raw addresses through `keyEqual`), which
   is why a corpus-wide bridge defect could hide behind it. **Not hardened against here on purpose** —
   tolerating a type-unsafe key would be the same "plausible but fake" move the inverse-atomic rule
   forbids; the loud failure is the correct behavior and the fix belongs in `abi`.

**Guarding measurement.** `encoding/gob` holds at **95 of 106** and `TestNetIP`'s root moves from
`NewHashTrieMap` → `ArgumentException: Delegate to an instance method cannot have null 'this'` to the
linkname stub above (no row regresses; `TestNetIP` is the only gob row whose closure reaches `unique` at
all). `unique` itself goes **0 → 1 of 19** and, more usefully, stops being a one-root wall: its 15 identical
`TypeInitializationException` rows resolve into five distinct downstream roots (the two linkname pushes, a
`GCHandle: Object contains references` on `abi.Escape`, an `IndexOutOfRangeException` in `makeCloneSeq`'s
`slice<T>` enumeration, and the `TypeFor` hole above). The hand-owned file is also its package's only Go
file, which makes `internal/concurrent` fully hand-owned — see
[`Baseline-vs-FullConversion.md`](../../src/archived/Baseline-vs-FullConversion.md) for what that does to the package's
`.csproj`/`package_info.cs`/`README.md`, and for the seeded-reconvert proof in both directions.

## The Linux syscall bottom — ONE libc P/Invoke, and why `r2` is exact rather than approximate

Go reaches the Linux kernel through a single assembly function. `internal/runtime/syscall/asm_linux_amd64.s` loads the call number into `RAX` and `a1..a6` into `RDI, RSI, RDX, R10, R8, R9`, executes `SYSCALL`, and reports `RAX` and `RDX`:

```go
// internal/runtime/syscall/syscall_linux.go — no body, no linkname, no Go anywhere
func Syscall6(num, a1, a2, a3, a4, a5, a6 uintptr) (r1, r2, errno uintptr)
```

Everything funnels through it: `syscall`'s `RawSyscall`/`RawSyscall6`/`Syscall`/`Syscall6`, every generated wrapper in `zsyscall_linux_amd64.cs` (open, read, write, close, stat, getrlimit, …), and this package's own `EpollCreate1`/`EpollWait`/`EpollCtl`/`Eventfd`, which is how `internal/poll` and the netpoller reach the kernel. Converted, it is a bodyless partial, so the [`PartialStubGenerator`](source-generators.md#source-generators) filled it with a throw — and because `syscall`'s own `init()` calls `Getrlimit(RLIMIT_NOFILE)` before `os` is usable, that one throw stopped every Linux program before `fmt.Println` could emit a byte.

The hand-own (`core/internal/runtime/syscall/linux/syscall_linux_impl.cs`) binds **glibc's `syscall(2)`** rather than reproducing the instruction — a user ruling, taken over the alternative of mapping each wrapper onto a .NET API. One P/Invoke lights the whole generated surface at once; the per-call alternative is N hand-owns, each independently guessing at semantics the kernel already defines exactly.

```csharp
[DllImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static extern nint libc_syscall(nint number, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6);
```

Three details separate a faithful binding from a plausible one. Each was **measured on linux/amd64** (glibc 2.35, .NET 9) rather than argued from the ABI documents, because each is exactly the kind of claim that reads as obviously true and is expensive when it is not:

* **The variadic.** C declares `long syscall(long number, ...)`; this declares seven fixed native ints. That is correct under SysV AMD64 — integer-class variadic arguments ride the same registers as fixed ones, with the seventh spilling to the stack, which is precisely where glibc's hand-written `syscall.S` reads `a6`. Proven with a real six-argument call: `mmap(NULL, 4096, PROT_READ|PROT_WRITE, MAP_PRIVATE|MAP_ANONYMOUS, -1, 0)` returned a live mapping that `munmap` then released. (`AL`, which a true variadic call sets to the vector-register count, is unused by `syscall.S`.)
* **`r2`.** Go's contract returns `RDX`, which libc's wrapper cannot hand back — the reason the [run-layer finding](../phase4/FINDING-linux-run-layer.md) listed it as an open question. It does not need to: the Linux x86-64 convention clobbers only `RCX` and `R11`, so `RDX` still holds what entered the kernel, and the asm's `MOVQ DX, BX` observes `a3` unchanged. Returning `a3` is therefore not a stand-in for `r2` on this architecture — it *is* `r2`. Probed under the real Go runtime: `syscall.Syscall6(SYS_GETPID, …, a3=0xDEADBEEF, …)` returns `r2=0xdeadbeef`. The failure path zeroes `r2`, which the shim mirrors.
* **`errno`.** libc collapses the kernel's whole `[-4095, -1]` error band to a `-1` return and reports the positive errno out of band — the same number Go's asm produces by negating the raw return — and `SetLastError` lets the CLR capture it before managed code can perturb it. Probed: `openat(AT_FDCWD, NULL)` returns `-1` with `Marshal.GetLastPInvokeError() == 14` (EFAULT).

**One divergence is disclosed rather than papered over:** a syscall that legitimately returns `-1` as a *success* value is indistinguishable from a failure through libc, and would report a stale errno. Go's asm has no such ambiguity because it tests the raw return against the whole band. Nothing the converted corpus reaches behaves that way, and the only true fix is an instruction-level bottom the managed model cannot express.

**The pointer half needed nothing.** These wrappers pass addresses as `uintptr` — `Getrlimit` emits `RawSyscall(SYS_GETRLIMIT, (uintptr)resource, (uintptr)Ꮡrlim, 0)` — and golib's `ж<T>` → `uintptr` operator does not hand out a token: it calls `EnsureStableAddress()` to pin the managed storage and returns a real address, so the kernel genuinely reads and writes through it. The residual risk is per-struct **layout**, not addressing, and it is the same open class as the Windows non-blittable-wrapper census (see `zsyscall_windows_impl.cs`); `Rlimit` is two `uint64`s, so the first crosser was blittable and worked untouched.

**Portability, stated in the file for the arm64 increment:** `asm_linux_arm64.s` puts `a1..a6` in `X0..X5` with the number in `X8` and reads `r2` from `X1` — which holds `a2`, not `a3` — so an arm64 flavor must echo `a2` and must re-run the `r2` probe there rather than inherit this answer. The variadic shortcut is likewise a per-platform judgment (standard AAPCS64 passes variadic integer args in the same registers as named ones; Apple's arm64 ABI deliberately does not), and a musl target would likely need a `NativeLibrary.SetDllImportResolver` fallback.

The binding itself is `[LibraryImport]` rather than `[DllImport]` — the corpus-wide FFI convention, adopted for the whole surface at once; see [Every P/Invoke is source-generated](#every-pinvoke-is-source-generated) for what that buys and what it cost to reach.

## Every P/Invoke is source-generated

Every native binding in the converted corpus — all fifteen, across five hand-owned files and two operating systems — is `[LibraryImport]`. There is no `[DllImport]` left, and a new one is a mistake rather than a style choice.

The reason is one property of the two attributes and nothing else. `[DllImport]` answers a signature it cannot marshal by marshalling something *else*: a non-blittable struct becomes a temporary copy, and a kernel that writes through the pointer writes into a temporary the caller never reads. That is a wrong **answer**, produced silently, at run time. The source generator refuses to emit the call at all, so the same mistake is a compile error with a line number.

That distinction is not hypothetical here. It is the exact defect `zsyscall_windows_impl.cs` exists to repair — three times over, at 172, 592 and 568 bytes — where the converted `Timezoneinformation`, `win32finddata1` and `ProcessEntry32` hold their inline `WCHAR[]` buffers as managed `array<uint16>` references and the kernel wrote native records over smaller managed objects. The remedy each time was an explicitly blittable mirror plus a pointer, and until now that discipline was enforced only by a reviewer noticing. It is now enforced by the compiler, which is the whole return on the migration: the residual risk of routing Go's kernel boundary through managed structs is per-struct **layout**, and this makes layout a build-time question.

**What the property costs, and the mechanism that pays it.** `SYSLIB1062` requires `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` unconditionally — even for an all-`nint` signature — because the generated stub is written in terms of pointers. That property is converter-generated from `usesUnsafeCode`, an *emission* fact that observes only C# the converter wrote, so a hand-owned file was structurally unable to ask for it: setting it by hand is undone by the next reconvert overlay. [`[module: go.GoRequiresUnsafe]`](#a-hand-owned-file-can-declare-that-it-needs-unsafe) is that mechanism, and `time` is the package where it is genuinely load-bearing rather than merely honest — its converted emission contains nothing unsafe at all.

**The rejection census.** Twelve of the fifteen declarations the generator accepted **unchanged**, which is itself the finding: `exec_windows.cs`'s five and `zsyscall_windows_impl.cs`'s five were already all-pointer, all-blittable, because both files were written under exactly the discipline the generator checks. Three needed a different signature, and each rejection was a latent hazard rather than a formality:

| Declaration | Rejected for | Became |
|---|---|---|
| `LoadLibraryExW` (`dll_windows.cs`) | `CharSet` has no `[LibraryImport]` equivalent | `StringMarshalling = StringMarshalling.Utf16` — and *not* merely equivalent: UTF-16 marshalling of a `string` is a **pin**, so the stub hands Windows the string's own storage instead of a copy |
| `GetProcAddress` (`dll_windows.cs`) | `CharSet.Ansi` + `BestFitMapping=false` + `ThrowOnUnmappableChar=true`; the latter two are unsupported outright | `byte*`. The mechanical translation (`StringMarshalling.Custom` over `AnsiStringMarshaller`) would have kept the transcode while silently dropping the guard that made it safe — an unmappable rune stops throwing and becomes `'?'`, i.e. a lookup of a *different symbol*. There was never a transcode to preserve: Go passes this entry point a `*byte` with no codepage step anywhere, and the caller already holds that NUL-terminated buffer. The old form decoded it through the ANSI codepage and the marshaller re-encoded it back, lossy in both directions |
| `SetWaitableTimer` (`time_impl.cs`) | `SafeWaitHandle`, and `bool` in both directions | The handle parameter becomes `nint` with `DangerousAddRef`/`DangerousRelease` taken **visibly** in the caller — SafeHandle marshalling is a runtime service, and `[DllImport]` supplied the reference count invisibly, so nothing in the source said where it was taken. `bool` becomes `[MarshalAs(UnmanagedType.Bool)]` on the return and the parameter: its native width is a marshalling decision (Win32 `BOOL` is four bytes, C++ `bool` is one), the runtime marshaller's silent default happened to be right, and the generator refuses to guess. `UnmanagedType.Bool` *is* the four-byte `BOOL`, so the ABI is unchanged and the choice is now written down |

**One converter-side change was required, and it is the interesting one.** `go2cs-gen`'s `PartialStubGenerator` fills every bodyless `partial` method with a throwing stub, which is how the converter emits Go's asm/cgo functions. A `[LibraryImport]` declaration is also a bodyless `partial` method — and **source generators cannot observe each other's output**, so `PartialImplementationPart` is null from there and the declaration looks exactly like an unimplemented asm function. Stubbing it produced two implementing parts and failed the whole package with `CS0757`, for all twelve P/Invokes at once, and only once a hand-own adopted the form. The generator now skips any partial declaration carrying an attribute that obliges a *different* generator to implement it; the test is on the attribute, because the attribute is the obligation. A second such attribute (`JSImport`, `GeneratedComInterface`, …) is added to that set rather than worked around at the call site.

Guarded by the `LibraryImportPartial` behavioral test, which is deliberately a *compile* assertion — both failure modes (`SYSLIB1062` from a missing declaration, `CS0757` from an over-eager stub) are compile failures — carrying a hand-owned `[LibraryImport]` of exactly the corpus's shape, plus a module initializer that calls it, so a program that prints at all is one whose generated marshalling stub reached the kernel.

## The scheduler brackets are a faithful no-op, not an omission

`syscall_linux.go` pulls the pair that wraps every non-`Raw` kernel call:

```go
//go:linkname runtime_entersyscall runtime.entersyscall
func runtime_entersyscall()
//go:linkname runtime_exitsyscall runtime.exitsyscall
func runtime_exitsyscall()
```

Forwarding is the shape this converter reaches for first (see the PULL section above), and it is unavailable here — not marginally: runtime's `entersyscall` opens with `getcallerfp()` / `getcallerpc()` / `getcallersp()`, raw-metal frame intrinsics with no managed realization, and hands them to `reentersyscall`, which drives the P state machine across `sched`, `mp.oldp` and `casgstatus`. None of that state exists in the managed model, so a forwarder faults on the first intrinsic.

`core/syscall/linux/syscall_linux_impl.cs` implements both as empty. That is a *realization*, not a stub: the pair's whole job is to release the P around a blocking call so other goroutines can run on another M, and to reacquire one after. Both are `func()`, they compute nothing any caller consumes, and no converted code reads state they would set — while the obligation they discharge is discharged by the host instead, since a converted goroutine is a .NET thread, a blocking syscall blocks that thread as the CLR expects, and thread-pool injection keeps other work running. This is the same judgment `syscall_impl.cs` records for `runtimeSetenv`/`runtimeUnsetenv`, and it is *not* the fabricated-answer failure mode the project rules against — there is no answer to fabricate. The package's other bodyless declarations (`rawSyscallNoError`, `rawVforkSyscall`, `runtime_doAllThreadsSyscall`, `cgocaller`) are separate questions and stay announcing stubs.

The file lives in `linux/` rather than flat because the declarations it implements are linux-only; a flat implementing part would have no defining declaration on Windows. See *Hand-owns have a platform*.

## `runtime.argslice` — forwarding and populating are ONE change

`os.init()` on unix assigns `Args = runtime_args()`, a bare-shape PUSH whose pushed body is ordinary converted Go: `append([]string{}, argslice...)`. Adding the registry row alone would have *worked* and been wrong — `runtime.argslice` is filled by `goargs()` reading the argv vector off the initial stack, a raw address the CLR does not hand out, so `os.Args` would have come back **empty**: not an error, just a plausible-looking wrong answer.

`core/runtime/goargs_impl.cs` is the sibling of `goenvs_impl.cs` and closes that: a `[ModuleInitializer]` — the faithful stand-in for schedinit's slot, running once before any converted Go code in the assembly — fills `argslice` from `Environment.GetCommandLineArgs()`, which is the managed mirror of the same vector (measured under `dotnet prog.dll alpha beta`: `{".../prog.dll", "alpha", "beta"}` — the program followed by its arguments, exactly Go's shape; *not* `Environment.ProcessPath`, which names the host, and *not* Main's `args`, which omits element zero). The row and the companion therefore land together, or the pair announces a falsehood.

**Windows is untouched in both halves**, and by Go's own construction rather than by an exception: `goargs()` itself opens `if GOOS == "windows" { return }`, and the companion keeps that guard verbatim, so `argslice` stays unset exactly as in Go — which is what `runtime_boring.cs`'s `boring_runtime_arg0` already documents and depends on ("On Windows, argslice is not set").

## `runtime.sysDirectory` — the same pairing, one consumer shape further out

`internal/syscall/windows.GetSystemDirectory` is a PUSH from `runtime/os_windows.go`, and it is the HANDLE consumer shape rather than `argslice`'s bare one: `security_windows.go` carries its own one-arg `//go:linkname GetSystemDirectory` above the bodyless declaration, so the registry row records `bareDecl: false`. Everything else is the `argslice` lesson repeated — which is the point, since it shows the rule is about the STATE behind the push, not about either syntax.

The pushed body is one line (`unsafe.String(&sysDirectory[0], sysDirectoryLen)`), and the buffer behind it is filled by `initSysDirectory()` calling `stdcall2(_GetSystemDirectoryA, …)` from `osinit`. **Neither half runs in the managed model** — `osinit` is the runtime bootstrap the converter emits already marked not-run, and `stdcall` bottoms out in `asmstdcall`, a [`PartialStubGenerator`](source-generators.md#source-generators) throw — so the buffer stays all-zero and its length `0`. A forwarder alone would have returned `""`, turning `net`'s `hostsFilePath = windows.GetSystemDirectory() + "/Drivers/etc/hosts"` into `"/Drivers/etc/hosts"`.

`core/runtime/windows/os_windows_impl.cs` closes it the way `goargs_impl.cs` does: a `[ModuleInitializer]` fills the buffer from `Environment.GetFolderPath(SpecialFolder.System)`. Two details are reproduced rather than tidied. Go appends a separator (`sysDirectory[l] = '\\'; sysDirectoryLen = l + 1`), so the answer really does end in a backslash and `net`'s concatenation really does produce `C:\Windows\System32\/Drivers/etc/hosts`; and Go's `throw("Unable to determine system directory")` is mirrored rather than softened into a short answer that would read as real.

What the missing row cost is out of all proportion to one symbol, and worth recording as a shape to look for: the throw came out of a package-level VAR INITIALIZER, so it surfaced from `net_package`'s type initializer — **every `httptest` consumer died in `net`'s cctor**, whatever it was actually testing.

## Long-path awareness is process SETUP, and golib does what Go's `osinit` does

`runtime.osinit` is not only where `goenvs`/`goargs`/`initSysDirectory` run; it is also where every Go Windows binary opts its own process into long-path handling. `initLongPathSupport()` checks for Windows 10.0.15063 or later and then sets the undocumented `IsLongPathAwareProcess` bit in the PEB's bit field. ntdll's path canonicalizer consults that bit, so with it set a plain, un-prefixed path longer than `MAX_PATH` reaches the kernel intact.

A converted program is an ordinary .NET process and gets none of that. The divergence is measured, not theoretical: at a 434-character path, `Directory.SetCurrentDirectory` fails `ERROR_FILENAME_EXCED_RANGE` (206, `0x800700CE`) where Go's `os.Chdir` succeeds. `MkdirAll` works on both sides because `os.fixLongPath` prefixes `\\?\` explicitly; `Chdir` hands the plain path to `SetCurrentDirectoryW`, and `\\?\` is no escape hatch there — `SetCurrentDirectory` rejects the extended form outright.

`golib/builtin.WindowsLongPaths.cs` therefore sets the same bit from `InitializeGoLib`, golib's analogue of `osinit`, under the same version guard, and defensively: it is a parity measure rather than a prerequisite, since the `\\?\` fallback still works with the flag clear.

**Why not an `<ApplicationManifest>` carrying `longPathAware`.** It reaches the same PEB flag and was the first remedy proposed, but Windows honors a manifest's declaration only when the machine-wide policy `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled` is *also* 1. Go asks for neither the manifest nor the policy — so a manifested converted binary would still diverge from the Go binary on a default install, where that value is 0, and the manifest measures as a fix only on machines where the policy happens to be on. Doing what Go does is also the smaller change: no per-project manifest artifact and nothing in the emitted `.csproj`, which is what keeps the behavioral corpus and every banked `<pkg>.tests.csproj` byte-identical (CNR compares the emitted `.csproj`).

**What is deliberately left alone.** `initLongPathSupport` also sets `internal/syscall/windows.CanUseLongPaths`, which makes `os.fixLongPath` stop adding the prefix. That flag lives in a converted package golib cannot reference — golib is the root of the dependency graph — and `false` is the conservative side: the extended-prefix spelling still reaches the kernel with the PEB bit set, so the only difference is which spelling it sees.

Guarded by `syscall`'s own `TestGetwd_DoesNotPanicWhenPathIsLong`, which skipped on `Chdir failed: … The filename or extension is too long` until this landed, and passes on both sides now.

## Linux standard-descriptor hygiene is process SETUP too — Go's close-of-stdout must release the pipe

The Linux member of the same `osinit`-parity family. A Go process holds exactly one file descriptor per standard stream, so closing `os.Stdout` releases the last reference to a stdout pipe's write end and the parent reading it sees EOF **immediately** — the readiness-barrier idiom Go programs use and os/exec's own test suite is built on ("Wait for cmd to close stdout to signal that its handlers are installed", the `startHang` shape behind `TestWaitInterrupt/*`).

The .NET runtime breaks the invariant before user code runs: at startup on Linux it duplicates each standard descriptor — `fcntl(0/1/2, F_DUPFD_CLOEXEC)` landing at the first free slots, observed unconditionally on linux-x64 net10.0 via strace with an empty `Main` and zero `Console` touches. Those duplicates hold the underlying pipe description open for the life of the process, so a converted child's `os.Stdout.Close()` no longer EOFs the parent: the pipe releases only at child **exit**. Measured, not theoretical — the pipe-EOF-barrier witness reads EOF 1.1 ms after the child's close in native Go and 8.27 s (exactly the child's lifetime) in the unfixed conversion. The same duplicates are what a `/proc` fd census of a hung child shows as "leaked parent pipes", which is how this was first misdiagnosed as spawn-side fd leakage — the `posix_spawn` hand-own (`syscall/linux/exec_unix.cs`) produces a clean child; the write-end holder was the child's *own* runtime duplicate, born after exec (it carries `FD_CLOEXEC`, which no inherited descriptor can).

`golib/builtin.LinuxStdDescriptors.cs` therefore closes the duplicates from `InitializeGoLib`, first thing, Linux-gated: every fd above 2 whose `/proc/self/fd` target equals that of fd 0, 1 or 2 **and** which carries `FD_CLOEXEC`. Both conditions are load-bearing — an inherited descriptor can never carry `FD_CLOEXEC` (it would not have survived the exec), so a deliberately passed `ExtraFiles` duplicate of a standard stream is untouchable by construction; and at module-initialization time no managed user code has run, so every close-on-exec alias is the runtime's own. The timing is the safety contract: `System.Console` creates its *own* on-demand duplicates when first touched (sweeping after that point kills a live `SafeFileHandle` — measured as `ConsolePal` "Bad file descriptor"), and a full-lifecycle strace shows the runtime never operates on the startup duplicates again, so closing them at managed dawn orphans nothing.

Guarded by the `StdoutCloseEofBarrier` behavioral test, deliberately deadlock-shaped rather than timed: the child closes stdout and then blocks on stdin until the parent — who must first see the EOF — writes the release byte. A regression deadlocks both sides into the harness run-timeout instead of flaking on a threshold. The residual is documented in the golib file: `println` routes through `Console.Error`, whose on-demand duplicate of fd 2 would hold a *stderr* pipe the same way; no measured row needs stderr-close EOF propagation yet.

## `internal/weak.Pointer` — the CLR already has weak references, so the runtime handle becomes one

> **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 `internal/weak` it was written
> against).** The package is the public `weak`, and `Strong` became `Value`; the hand-own is
> [`src/core/weak/pointer.cs`](../../src/core/weak/pointer.cs), on the same `WeakReference` design. It is
> no longer its package's only Go file (`doc.go` converts), so `weak` re-emits its `.csproj`,
> `package_info.cs` and `README.md`, and the layer beneath it is `internal/sync.HashTrieMap`.

`internal/weak` is `unique`'s liveness model, one layer below `internal/concurrent.HashTrieMap` and in
front of the same consumers. The package's entire body is two `//go:linkname` declarations, and both
pushed bodies live in `runtime/mheap.go`:

```go
func Make[T any](ptr *T) Pointer[T] {
	ptr = abi.Escape(ptr)                                   // force the pointee onto the heap
	var u unsafe.Pointer
	if ptr != nil {
		u = runtime_registerWeakPointer(unsafe.Pointer(ptr))
	}
	runtime.KeepAlive(ptr)
	return Pointer[T]{u}
}

func (p Pointer[T]) Strong() *T { return (*T)(runtime_makeStrongFromWeak(p.u)) }
```

`registerWeakPointer` → `getOrAddWeakHandle` → `getWeakHandle` → `spanOfHeap` walks `mheap_` span
metadata to find or hang a `specialWeakHandle` off the span, and `makeStrongFromWeak` loads a word out of
that handle and **re-derives an object pointer from the address**. The managed model populates no span
metadata, and *"what object lives at this address?"* is a question the CLR does not answer at all — so the
pair is registered UNHONORABLE in `linknamePushTargets` and each half announces itself by name (see
[*A cross-package `//go:linkname` PUSH resolves per recorded disposition*](#a-cross-package-golinkname-push-resolves-per-recorded-disposition--forwarder-or-announced-panic)).
The announcement is what pointed at this hand-own; this is what it was pointing at.

**The ruling is the `sync.Mutex` / `internal/concurrent.HashTrieMap` precedent, and it fits better here
than anywhere it has been applied before, because the CLR has first-class weak references of its own.**
`src/core/internal/weak/pointer.cs` carries `[module: go.GoManualConversion]` and contains no span walk.
The contract translates clause for clause:

| Go's contract | Managed mechanism |
|:--|:--|
| `Make(ptr)` never fails; `Strong()` yields the ORIGINAL pointer while the referent is reachable, and nil once the collector has identified it unreachable — **before** a finalizer can resurrect it | `WeakReference<ж<T>>` over the `ж<T>` box, **SHORT** (`trackResurrection: false`); Go's handle likewise clears ahead of finalization |
| A weak pointer does not keep its referent alive | Nothing on the `Pointer<T>` → `handle<T>` → referent path is a strong reference |
| Weak handles are **unique and canonical per byte offset into an object**, so weak pointers made from pointers that compare equal compare equal — and pointers to different offsets within one object do not | A `ConditionalWeakTable` keyed on the referent ALLOCATION whose value is a `ConcurrentDictionary` keyed on the GO POINTER. `ж<T>`'s own `Equals`/`GetHashCode` ARE Go's pointer identity, including "two fields of one struct are different addresses" |
| Equality is retained after the referent is reclaimed | `Pointer<T>` holds the handle STRONGLY, so the handle outlives the referent and keeps answering — it just answers nil forever after |
| A weak pointer made after a resurrection is NEWLY UNIQUE | The table entry dies with the referent (that is what a `ConditionalWeakTable` key is), so a later `Make` mints a fresh handle |
| `abi.Escape(ptr)` — force the pointee out of the frame | Nothing to force: a `ж<T>` IS a heap allocation from construction, whatever its pointee's type |
| `runtime.KeepAlive(ptr)` | `GC.KeepAlive` — load-bearing, not decorative: the referent is reachable from `Make`'s frame only through the argument, and every use of it is finished before the return |

**Why the canonical table does not pin what it indexes — the one subtle claim.** A
`ConditionalWeakTable` is an EPHEMERON: its value is kept alive only while the KEY is independently
reachable, and edges *from* the value *to* the key do not count as reachability. The key is
`ж<T>.ReferentObject` — the same lifetime question `runtime.SetFinalizer` already keys on (`mfinal.cs`),
answered the same way: an element ref resolves to its backing storage, a field ref to the containing
allocation, and a standard heap box to itself. The value holds the `ж<T>` boxes strongly, as dictionary
keys, which is deliberate — for a field or element pointer the box is a per-expression view that would
otherwise die long before the struct does, and `Strong()` must keep returning it; the ephemeron makes
that safe. The handle holds only a `WeakReference`. Composing the three, **a box is reachable exactly
when its referent is**, so one plain `WeakReference` tracks the REFERENT's liveness for every pointer
shape, not merely the standard-box shape.

One shape is deliberately not modelled, and it is Go's error case too: a box that ALIASES A NATIVE
ADDRESS names unmanaged storage the collector does not own, so its managed reachability is not the Go
question. Go answers by faulting (`throw("getWeakHandle on invalid pointer")` — a non-heap address has no
span); here it would observe an eventual nil rather than a fabricated pointer, the safe direction.
Nothing in the converted corpus takes a weak pointer to one.

**A second, independent defect this closes.** `Pointer[T]` is written out rather than left to `[GoType]`,
because the generated struct equality is field-wise `==` **guarded on every type parameter carrying an
`IEqualityOperators` constraint** (`TypeGenerator`'s `hasEqualityOperators` → `AllGenericTypesHaveConstraint`),
and Go's `Pointer[T any]` carries none — so the emitted body was literally
`Equals(other) => false /* missing equality constraints */`. Two weak pointers to one object NEVER
compared equal, contradicting the type's own doc comment and silently defeating `unique.Make`'s
`m.CompareAndDelete(value, wp)`, which could therefore never match and never evict a dead entry.
Equality is the *whole reason* the runtime canonicalizes the handle, so it is hand-written here — and as
`IEquatable<Pointer<T>>`, which the generated form does not implement, so `EqualityComparer<Pointer<T>>.Default`
reaches it without boxing on every lookup. ⚠ **The gate itself is over-conservative and the defect is
corpus-wide**: it disqualifies a struct when ANY type parameter lacks the constraint, even when no
field's type mentions that parameter. `unique.Handle[T]` is the other confirmed victim — its single field
is a `ж<T>`, which defines `==` for every `T`, yet its generated `Equals` is `false` too, so
`unique.Handle` values never compare equal either. That is a generator fix rather than a hand-own, and is
left to its own arc.

**Guarding measurement.** `internal/weak`'s own suite now links and runs
(`go2cs -tests -test-action all "<GOROOT>/src/internal/weak" src/core/internal/weak`): **`TestPointerEquality`
PASSES against `go test`** — the canonicalization clause, the hardest one, validated end to end.
`TestPointer` and `TestPointerFinalizer` do not, and the reason is the roster's already-named
**`codegen-liveness`** class rather than the weak model: both hold the referent in a live C# local (`bt`)
across the `runtime.GC()` that is supposed to kill it, where Go's per-safepoint liveness maps drop it at
its last use. (Neither is *disclosable* — `TestPointerFinalizer` does not fail an assertion, it blocks
forever on `<-done` waiting for a finalizer that a still-rooted object can never queue — so
`internal/weak` does not bank.) A dedicated probe separates the two — a referent
created and dropped inside a `[MethodImpl(NoInlining)]` helper is reported collected, and only a weak
pointer that had `Strong()` called on it *earlier in the same frame* stays alive:

```
PASS  Strong() is nil once the referent is unreachable (never probed)
FAIL  Strong() is nil once the referent is unreachable (probed first)
```

with a self-keyed `ConditionalWeakTable` control and the two-level table control both collecting, so the
ephemeron reasoning above is confirmed rather than assumed. `unique` reads the same way from the other
side: every `TestHandle` subtest that gets far enough reports **only** `v0 != v1` (the `[GoType]` equality
gate above) and never `v0.Value() != v1.Value()` — i.e. both `Make` calls interned the *same* `ж<T>`, which
is exactly what canonical weak handles plus `LoadOrStore` are for.

`pointer.go` is this package's only Go file, so marking it makes `internal/weak` **fully hand-owned**: the
driver `continue`s on `unmarkedFileCount == 0` and stops re-emitting `internal.weak.csproj`,
`package_info.cs` and `README.md`, and no `pointer.cs.auto` review sibling is produced — the position
`internal/godebug` and `internal/concurrent` are already in. The marker census moves **39 → 40**.

## `unique.clone` — a raw-offset string walk the managed model cannot express, hand-owned after the `@string` window made it GC-fatal

Go's `unique.clone[T]` rewrites every string field of a just-interned value in place, addressing each
by raw ABI offset so an interned handle never keeps a large parent string alive:

```go
func clone[T comparable](value T, seq *cloneSeq) T {
	for _, offset := range seq.stringOffsets {
		ps := (*string)(unsafe.Pointer(uintptr(unsafe.Pointer(&value)) + offset))
		*ps = stringslite.Clone(*ps)
	}
	return value
}
```

The converted form — `(ж<@string>)(uintptr)((uintptr)Ꮡvalue + offset)` followed by a `.Value` write —
adds a **Go ABI** offset to the transient interior address of the movable `ж<T>` heap box, whose CLR
field layout is unrelated to Go's ABI (`EnsureStableAddress` cannot pin a box whose `T` contains
references, and for `[2]struct{a string}` the +16 offset is outside the 8-byte `array<T>` reference
that is the entire CLR value). Every such store landed on the box's OWN fields. While `@string` was a
single 8-byte reference the damage was a type-confused slot holding a valid object — silently wrong
values, nothing the collector trips over. When `@string` became an offset/length **window**
(`fc6d8c179`, r57c — 16 bytes: `byte[]` + two `int`s), the same store's integer tail began landing in
an adjacent GC-scanned reference slot, and the next collection — which `unique`'s own `drainMaps`
forces via `runtime.GC()` — walked a garbage pointer and fail-fasted the process with
`COR_E_EXECUTIONENGINE` (0x80131506). Bisected, and reproduced in ~25 lines against golib alone, by
the 2026-08-12 unique-bisect lane (the board's scout-batch-1 `unique` entry holds the full record).

`src/core/unique/clone.cs` therefore carries `[module: go.GoManualConversion]` — the standard S1
managed-referent remedy — with **only `clone<T>` departing from the conversion**. Its contract is
"makes a copy of value, and MAY update string values found in value with a cloned version": the
cloning is a retention optimization, never a semantic requirement, so the hand-own does the
`T == string` case exactly (a right-sized `stringslite.Clone`, no address arithmetic — worth more,
not less, now that `@string` windows share backing) and returns aggregate values unchanged. The one
observable divergence from Go is retention: an interned aggregate's strings keep sharing their
original backing arrays. Equality, identity and intern-map drainage — what `unique.Make` is *for* —
are unaffected. `makeCloneSeq` and the `cloneSeq` builders remain the verbatim conversion (pure
descriptor arithmetic, still validated by `TestMakeCloneSeq`), so a `clone.cs.auto` review sibling is
emitted on every reconvert as usual.

## `internal/cpu.getGOAMD64level` — a BUILD constant, so the honest answer is the baseline

Go declares `getGOAMD64level() int32` bodyless and implements it in `cpu_x86.s`, where it is not code at
all but a compile-time constant selected by the `GOAMD64_vN` define the toolchain sets from
`go env GOAMD64`:

```asm
TEXT ·getGOAMD64level(SB),NOSPLIT,$0-4
#ifdef GOAMD64_v4
	MOVL $4, ret+0(FP)
#else
#ifdef GOAMD64_v3
	MOVL $3, ret+0(FP)
#else
#ifdef GOAMD64_v2
	MOVL $2, ret+0(FP)
#else
	MOVL $1, ret+0(FP)
#endif
```

The question it answers is *which amd64 microarchitecture level was this BINARY built for*, never *which
level does this CPU support* — the two differ constantly, and Go depends on the difference: `doinit`'s
option table gates the `sse3`/`avx`/`avx512` GODEBUG knobs on `level < 2/3/4`, so a v1 build running on a
v3 machine keeps them switchable. go2cs emits portable C# with no GOAMD64 define, no
microarchitecture-gated emission and no instruction-set floor above the amd64 baseline, so the faithful
answer is the same constant Go's own assembly produces for a build without a `GOAMD64_vN` define: **1**.
That is a measured property of the emission rather than a placeholder value, and probing the host through
`System.Runtime.Intrinsics.X86` would answer a *different question* — the inverse-atomic rule's exact
prohibition, since the returned number would look truthful while meaning something else.

`cpu_x86_impl.cs` returns it under `[module: go.GoManualConversion]`, registered as
`manualConversionFuncs["internal/cpu"]["getGOAMD64level"]`, so the converter leaves the standard
placeholder comment where the bodyless partial was. A/B footprint: **one corpus file**. Demonstrated
consumer: `internal/cpu`'s own `TestDisableSSE3`, whose first statement is
`if GetGOAMD64level() > 1 { t.Skip(…) }` — against the unimplemented `PartialStubGenerator` stub that
guard was an infrastructure-error, and it was the package's only divergence (7 of 8). With the constant in
place the test reads 1, walks on into `runDebugOptionsTest`, and skips exactly where Go does:
`internal/cpu` validates **8 of 8**.

## `new(T)` is Go's ZERO value — and for a container kind that means the NIL one

`p := new([]int)` in Go yields a pointer to the zero slice, which is **nil**; likewise
`new(map[K]V)` and `new(chan T)`. golib's container structs each declare a parameterless constructor
that ALLOCATES (`map<K,V>` makes its backing dictionary, `slice<T>` takes the empty array), and
`Activator.CreateInstance<T>` honors a declared parameterless constructor — so `new(T)` handed back a
pointer to a non-nil EMPTY container.

It went unnoticed because the two differ only under `== nil`: `len()` agrees at 0, a range over either
yields nothing, and `encoding/json` marshals them by the same branch. It surfaced where the two
zero-FABRICATION paths finally met. `reflect.Zero`/`New` build a zero through `GoReflect.ZeroValueOf`,
which has always answered the NIL container for these kinds, so

```go
reflect.DeepEqual(new([]any), reflect.New(typ).Interface())
```

compared a nil slice against an empty one and was false — and that comparison is the precondition
`encoding/json`'s whole `TestUnmarshal` table checks before every subtest, which is how one
constructor blocked forty-odd verdicts at once. `builtin.@new<T>` now takes `default(T)` for the
slice/map/chan kinds and keeps running the constructor for every other kind, because that is what
materializes a struct's fixed-size ARRAY fields from the initializers the converter emits into it. The
two rules are one classification now, asked of the same `KindOf`.

## An `unsafe.Pointer` is compared BY ADDRESS, and a container's pointer names its STORAGE

Three separate identity rules meet in Go's own cycle detectors, and all three were wrong in the same
direction — too *fine*, so nothing ever compared equal to itself:

1. **`unsafe.Pointer`.** It is a `ж<uintptr>` whose VALUE is the address, and `ж<T>` compares and
   hashes a heap box by REFERENCE — right for every other pointer (the box IS the storage it names)
   and wrong for this one, which CARRIES an address while the converter mints a fresh box on every
   `uintptr → unsafe.Pointer` conversion. `ж<T>.Equals` is `virtual` for this single override, which
   makes `==`, `Equals` and a map-key lookup answer through one rule; an
   `operator ==(Pointer, Pointer)` would instead have made every existing
   `uintptr == unsafe.Pointer` comparison ambiguous (CS0034, measured in `runtime`'s `map.cs` and
   `mfinal.cs`).
2. **A MAP or SLICE reached through `reflect.Value.UnsafePointer`.** Go answers the STORAGE address —
   the hmap for a map, `&s[0]` for a slice — while the managed value is a HEADER STRUCT, freshly boxed
   on every read out of a slot, so two reads of one Go map tokened differently. The token now comes
   from the backing store (plus the window offset for a slice), which is the same root
   `deepValueEqual` keys its own cycle detection on, so the two walks cannot disagree about what "the
   same map" means.
3. **A struct field of INTERFACE type.** `go2cs-gen`'s memberwise `Equals` compared every member with
   C# `==`, which for an interface or `object` operand is reference identity where Go compares
   interface values by dynamic type and value. Since a struct's `Equals` is also what a map LOOKUP
   calls, such a struct could never be found under a key it had itself been stored under. Those
   members now route through `builtin.AreEqual`, the same relation the converter emits for a bare Go
   `==` between interface operands; `ж<T>`, named-pointer wrappers and delegates keep `==`, because
   pointer identity IS Go's pointer relation and a struct holding a func is not comparable in Go at
   all.

`encoding/json`'s encoder needs all three: it keys `e.ptrSeen` on `v.Interface()` for a pointer, on
`v.UnsafePointer()` for a map, and on `struct{ ptr any; len int }` for a slice. With any of them
answering by identity the lookup never matched an entry it had itself stored, no cycle was detected,
and `Marshal` of a self-referential value recursed until the process died — `0xc00000fd`, which is
uncatchable and takes every verdict the run had not yet produced with it. Go returns
`UnsupportedValueError: encountered a cycle`.

## A NaN map key is never equal to anything, itself included

BCL `Double.Equals` reports NaN equal to NaN, deliberately, so that a NaN stored in a collection can
be found again. Go applies `==` unchanged, so a NaN key is equal to nothing: `m[NaN] = 1` twice stores
TWO entries, and neither can ever be read back or deleted. `fmt`'s own `TestSprintf` reads the
difference out of `%v` of a map — `map[NaN:1]` against Go's `map[NaN:1 NaN:1]`.

`GoEqualityComparer.ForKeys` therefore supplies a per-representation, non-boxing comparer for the four
float representations, whose entire implementation is `==` — because C#'s float `==` IS the IEEE
relation Go's map applies. The hash stays the type's own, and a NaN that hashes consistently while
comparing unequal builds exactly the same-bucket/never-equal chain Go's map builds for it. Scoped to
the raw representations: a NAMED float type's wrapper, and a struct or array that CONTAINS a float,
still compare through their generated equality and inherit the BCL rule. No measured consumer reaches
those, and covering them would mean routing every struct-keyed map through the reflective relation.

## A COMPLEX constant expression must be FOLDED — .NET's mixed operators are not Go's arithmetic

The float sibling of this rule (see *Named Numeric Types and Constant Contexts*) folds a constant
expression only where a named untyped const forces it, on the stated ground that a pure-literal float
constant "computes exactly in C# double, so its readable operator form is kept". That ground does not
hold for COMPLEX, and the counter-example is ordinary: .NET's mixed real/complex operators are not
Go's constant arithmetic and are not even IEEE. `Complex.operator -(double, Complex)` computes the
imaginary part as `-right.Imaginary` rather than `left.Imaginary - right.Imaginary`, so

```csharp
1230000D - 0D.i()      // imaginary -0
```

where Go — which folds the untyped expression in exact rational arithmetic, and an exact zero has no
sign — yields `+0`. `fmt`'s `TestSprintf` reads the sign back out: `%#12.5g` of `1230000 - 0i` printed
`-0.0000i` against Go's `+0.0000i`.

So the rule is stated the way Go states it: a complex-kind constant operator expression HAS an exact
value and is emitted as that value, through the same `exactComplexConstString` a complex const
DECLARATION already uses. Keyed on `constant.Complex` — go/constant normalizes a real-valued result to
an Int/Float kind, so a real-only expression in a complex context is left to the float and integer
folds above it. Deliberately no `/* <gofmt> */` annotation, unlike the float fold: the folded text
re-renders in the same `re + imi` shape `convBasicLit` already emits, so the common case (`3 + 4i` →
`3F + 4F.i()`) is byte-identical to the operator rendering it replaces, and only the cases where the
two genuinely disagree move.

## Realizing the runtime TIMER contract (`Sleep` / `newTimer` / `stopTimer` / `resetTimer`)

`time`'s four timer entry points have no Go body — they are `//go:linkname`'d into `runtime/time.go` — so the converter emits them as bodyless `partial`s that the [`PartialStubGenerator`](source-generators.md#source-generators) fills with `NotImplementedException`. A stub throw on a timer path is uniquely destructive: it lands on whichever goroutine touched the timer, and an unrecovered panic in *any* goroutine terminates the process, so every package that so much as called `time.Sleep` once was unreachable. `time_impl.cs` supplies the four bodies (the same supplemental-companion mechanism as `math_impl.cs` and the clock reads above it), and the whole model is a single `_impl.cs` region — no converter or golib change.

**Service model: one heap, one thread — Go's own pre-per-P design.** Go keeps timers in per-P heaps run by whichever P first notices a deadline; the managed model is one deadline-ordered heap serviced by one dedicated background thread, the shape Go itself used before per-P timers (the old runtime `timerproc`). One thread is sufficient *and* ordering-faithful for a specific reason: the only two callbacks package `time` ever installs are `sendTime` (a **non-blocking** channel send) and `goFunc` (which only starts a goroutine), so no timer callback can occupy the service thread and delay a later deadline. Callbacks are collected under the lock and invoked after releasing it, in deadline order.

**Precision: the same OS object Go uses.** `System.Threading.Timer` is *not* the mechanism, because its resolution is the Windows timer tick — measured at ~15 ms on the development host, which would make a 1 ms Go timer fire 15× late and let two timers less than a tick apart fire **out of order**. Both `Sleep` and the service thread instead wait on a Windows high-resolution waitable timer (`CreateWaitableTimerExW` + `CREATE_WAITABLE_TIMER_HIGH_RESOLUTION`), which is precisely what the Go runtime creates for its own sub-tick sleeps (`runtime/os_windows.go` `createHighResTimer`/`usleep`), cached **per thread** exactly as Go caches it per M. The wait is driven off the same monotonic source `runtimeNano()` reads, so timer deadlines stay coherent with `Now()`/`Since`/`Sub`; a truncation in the 100 ns due-time unit can only wake early, and the deadline loop re-waits the remainder, preserving Go's "at least `d`" guarantee. Where no high-resolution timer exists the fallback waits coarsely to within a millisecond and spins the remainder — correct, but tick-quantized, which is the whole reason the high-resolution path is preferred. The interop is `[LibraryImport]` over `nint` and `ref long`, and this file is where the migration to it cost the most: the source generator marshals no `SafeHandle` and guesses at no `bool`, so the handle's reference count is now taken visibly in `Arm` and both `bool`s carry an explicit `UnmanagedType.Bool`. It also needs `/unsafe`, which `time`'s converted emission does not, so `time_impl.cs` carries a [`[module: go.GoRequiresUnsafe]`](#a-hand-owned-file-can-declare-that-it-needs-unsafe) declaration — the mechanism that exists because the csproj is regenerated on every transpile. See [Every P/Invoke is source-generated](#every-pinvoke-is-source-generated).

**Hidden state keyed by BOX IDENTITY.** Go's `runtime.timeTimer` carries the timer state in fields *after* the two `time` can see — sleep.go's "extra fields after the channel, reserved for the runtime and inaccessible to users". The managed equivalent hangs a `runtimeTimer` record (`when`/`period`/`f`/`arg`/`gen`) off the `ж<Timer>` box's **reference identity** through a `ConditionalWeakTable`: `stopTimer`/`resetTimer` are always handed the very box `newTimer` returned. Weak-keyed, so an unreferenced `Timer` stays collectible (Go 1.23 recovers unreferenced timers) — while an *armed* timer's state is independently kept alive by the service heap, which is what makes a bare `time.After(d)`, whose `Timer` is dropped on the spot and only the channel kept, still fire.

**The Stop/Reset contract and the fire race.** Both report `when > 0` — Go's `timer.stop`/`timer.modify` compute `pending` exactly that way — so *pending* means armed and not yet fired, and `when` doubles as the arm marker (0 = stopped, or a one-shot that fired). Every change to `when` bumps a generation counter, and heap entries carry the generation they were queued with; a `Stop` or `Reset` therefore cancels an already-queued firing **without** removing it from the heap (Go leaves the stale entry too, marked `timerModified`/`timerZombie`). One lock guards the heap *and* every timer field — Go's finer-grained per-timer + per-P scheme exists to scale across Ps, and with a single heap there is one lock and hence no lock-ordering hazard — so a `Stop`/`Reset` racing the firing callback can neither double-fire nor lose a re-arm: either the service thread already took the callback (and `when` is 0, so `pending` correctly reports false), or the generation bump invalidates its queued entry and the callback never runs. A `Ticker` re-arms by **whole periods** past a late firing (`next = when + period*(1 + delay/period)`), which keeps the tick *phase* aligned to the original schedule instead of drifting; combined with `sendTime`'s non-blocking send onto the cap-1 channel, a receiver too slow to keep up therefore **loses** ticks rather than seeing them queue — Go's documented "adjust the time interval or drop ticks to make up for slow receivers".

**ONE firing per timer per pass — the service pass reads the clock exactly once.** `serviceTimers` samples `now` once and threads that single value through the whole drain. That is the invariant, not an optimization, and Go does the same for the same reason: the scheduler samples the clock in `timers.check` and hands it down through `timers.run(now)` to `timer.unlockAndRun(now)`, never re-reading it inside a pass. The consequence is a theorem rather than a heuristic — *within one service pass every timer fires at most once*. A one-shot leaves the heap with `when` cleared; a periodic timer is re-armed to `next = when + period*(1 + delay/period)` with `delay = now - when >= 0`, and writing `delay = q*period + r` for `0 <= r < period` gives `next = when + period*(1 + q) = now + (period - r)`, which `r < period` makes **strictly greater than `now`** — so the re-peek always takes the "not yet due" branch and the drain ends. It holds for every period, down to 1 ns. Re-reading the clock per iteration broke the theorem and was a real defect (recorded r36, fixed r39): the advanced `when` lands one nanosecond ahead, a freshly read `now` has already passed it, and the same ticker fires again — for as long as consecutive reads of the ~100 ns monotonic source keep advancing. The burst is invisible while nobody is receiving (the non-blocking send onto a cap-1 channel drops all but one) but `time`'s own `TestChan` *is* receiving: the two stale values an async ticker is allowed became three or more, and `noTick` reported "extra tick" in **all three** `asynctimerchan` modes — which the then-standing asynchronous-timer-channel divergence (now implemented, below), scoped to the sync mode, never explained. The invariant does not rate-limit: a pass fires each due timer once and then waits until the new head deadline, which for a fast ticker is already past, so the wait returns at once and the next pass fires it again — exactly as Go's scheduler calls `check` again with a fresh `now`. The bound is on re-firing *within* a pass, which is what "drop ticks to make up for slow receivers" means. Nor can it delay anything: `next` depends on `now` only through the non-decreasing floor `delay/period`, so hoisting the read can only make the pass's `deadline` smaller or equal, and `waitUntil` recomputes `remaining` from a fresh clock — no timer can wake later than the per-iteration version would have woken it, and when `delay < period` (the common case) the deadline is identical either way. A timer coming due *during* a pass waits for the next one and that is not a delay either, because the head of the heap is the minimum `when` among live entries, so the deadline is already past by the end of the drain and the next pass starts at once. Two adjacent places are deliberately **not** Go, and the fidelity claim should not be read past them: Go's `check` releases the timer-set lock around *each* callback and re-validates the head between them, so a `Stop` landing mid-pass cancels the callbacks after it, where this drain commits the whole batch under one lock hold and then runs it; and Go keeps one heap entry per *timer* (repositioned in place, zombies swept) where this keeps one per *arm*, reclaiming a dead entry only when it reaches the head. Both predate the single clock sample and are narrowed by it, since a frozen `now` commits a smaller batch. A standing constraint follows from the same arithmetic: at the instant a ticker is stopped or reset at most **two** of its ticks can exist — one buffered, one committed but not yet sent. In *synchronous* mode both are now revoked outright (the drain takes the first, the `seq` check the second — see the next entry), so the guarantee no longer rests on that count; in `asynctimerchan={1,2}` it still does, and two is exactly what `drainAsync` drains, so the margin there is zero and any later change that lets two ticks for one timer be committed before their callbacks run re-breaks `noTick` in those modes without touching `time_impl.cs`.

**`tick.cs` is hand-owned, and revertibly so.** Go builds a `Ticker` by reinterpreting the `*Timer` the runtime returned — `(*Ticker)(unsafe.Pointer(newTimer(…)))`, plus the mirror-image casts in `Ticker.Stop`/`Reset` — because "Ticker and Timer have the same layout". Converted literally those three reinterprets **compile but cannot work**: each is a managed-box `uintptr` round-trip whose address escapes its `fixed` pin, and nothing references the `ж<Timer>` `newTimer` produced, so the ticker's storage is collected at the next GC (the retained-pointer worst case of the corpus-wide hazard in `docs/phase4/FINDING-managed-box-uintptr-lifetime.md`). Pinning is not available either — `Ticker` holds a `channel<Time>`, a managed reference, and `GCHandle.Alloc(…, Pinned)` rejects a type containing references — so the raw-address model has no sound form here. The file (marked `[module: go.GoManualConversion]`, whole-file) builds the `Ticker` directly and addresses its timer by box identity, which is the standing managed-referent ruling at the top of this section; every doc comment and the struct declaration are the converted output verbatim, and the file should be **deleted** in favor of its `tick.cs.auto` sibling the moment a general managed-reinterpret capability lands.

**Synchronous timer channels — the property, and the two mechanisms that keep it (#37196).** Go 1.23 made a chan-based `Timer`/`Ticker` channel *synchronous*, and the name describes a guarantee rather than a plumbing change: the channel is still created as `make(chan Time, 1)` — a tick can be produced with nobody receiving — and what changed is that its owner may take a tick **back**. The whole proposal is one sentence: *a `Stop` or `Reset` prevents any tick generated before the call from being received after it.* Equivalently, every value a receive on `t.C` observes was committed **after** the most recent `Stop`/`Reset`, so there are no stale values and the pre-1.23 "drain `t.C` when `Stop` returned false" idiom is unnecessary. Two consequences are directly observable: `Stop`/`Reset` report `pending == true` when they **revoke** an unreceived tick, not only when the timer was still armed; and `len(t.C)`/`cap(t.C)` are **0**, because a buffered value the next `Stop` may confiscate must not be advertised.

The guarantee needs three mechanisms — two of Go's, at the same two layers, plus one this model needs because it fires eagerly where Go fires lazily. **(1) Send lock + sequence, in `time_impl.cs`** — Go's `timer.sendLock`/`timer.seq`. A firing is decided under the heap lock but must be delivered with it released, so a `Stop`/`Reset` can land in between; the service pass therefore only *offers* a tick. It captures `seq` when it commits the firing (exactly where Go's `unlockAndRun` copies `t.seq` under `t.mu`), then takes that timer's `sendLock` and re-checks — a mismatch means a `Stop`/`Reset` intervened and the offer is **abandoned** (Go expresses the same thing by replacing `f` with a no-op). `Stop`/`Reset` bump `seq` while holding `sendLock`, so every offer is wholly before them or wholly after; it cannot straddle. `seq` is deliberately *not* the heap generation `gen`: `gen` is bumped by a firing as well, which is what makes it the heap's liveness token, and a delivery check has to survive its own firing. **(2) Buffer drain, in golib** — `channel<T>.DrainBuffer()`, Go's `runtime.timerchandrain`: the `seq` check stops offers not yet sent, and a tick *already in the buffer* is revoked by emptying it. Both `Stop` and `Reset` drain, and either reports `pending` true when it discarded something. **(3) The `offered` flag, with no Go counterpart.** Mechanisms 1 and 2 revoke correctly but cannot between them *answer* correctly: in the very window mechanism 1 exists to cover, the tick is in neither place a `Stop` looks — `when` is already 0 and the buffer is still empty — so `Stop` would revoke the tick and then report that there had been none. Go never reaches that state, because a sync-mode chan timer nobody is receiving from is not in a heap at all (`timer.needsAdd` requires `t.blocked > 0`) and therefore never fires; eager firing is what opens the window, so eager firing is what has to close it. `offered` is set under the heap lock where the firing is committed, cleared under `sendLock` where it resolves, and read by `Stop`/`Reset`, which hold both. Together the three cover every place a tick can be: scheduled, committed-but-unsent, or buffered. A tick handed **directly** to a parked receiver is none of them — but that receive already committed, and it committed while the sender held `sendLock`, hence strictly before the `Stop` waiting on that lock could return; the guarantee is about ticks received *after* the call, so a direct hand-off falls inside it rather than being an exception.

The channel side is a general hook, not a `time` special case: golib gains `IChannelTimer` (Go's `hchan.timer`), a channel installs its owner with `AttachTimer` before the timer is armed, and `channel<T>.Capacity`/`Length` return 0 while the owner answers `HidesBuffer` — asked **live**, because `GODEBUG=asynctimerchan` selects the model at every observation, exactly as `runtime.chanlen`/`chancap` re-read the setting. `IsUnbuffered` deliberately still reports the *physical* shape (a timer channel's send does not rendezvous), since Go exposes only `cap()`.

Two places are deliberately **not** Go. Upstream `Stop` drains after releasing `sendLock` and `Reset` drains before releasing it; this holds `sendLock` across the drain in **both**, because Go can afford the looser order only thanks to lazy heaping (a sync-mode chan timer is in a heap only while a receiver is blocked on it, so nothing can produce a tick between `Stop`'s unlock and its drain) and this model's service thread is always live and always eager — a `Reset` racing another goroutine's `Stop` could otherwise have its fresh tick drained by the `Stop`. And Go fires a sync-mode chan timer **lazily** (`runtime.maybeRunChan`, at receive time) where this fires eagerly from the service heap; that is what mechanism 3 exists for, and *with* it `Stop`'s answer is the same either way — but eager firing still costs Go 1.23's early GC of unreferenced timers, since an armed timer stays reachable from the service heap. `GODEBUG=asynctimerchan` selects the model as upstream: `0` (default) synchronous, `1` pre-1.23 asynchronous (package `time`'s own `syncTimer` withholds the channel, so none is ever registered), `2` asynchronous semantics over a registered channel — and modes 1 and 2 run the pre-existing model unchanged, every branch being gated on the live setting. Measured on `time`'s own `TestChan`: all six subtests (`asynctimerchan={0,1,2}` × Timer/Ticker) pass, where before only `{1,2}` did. Guarded end-to-end by the `SyncTimerChannel` behavioral project, whose stdout is byte-compared against `go run`: `Stop`/`Reset` `pending` after a fire, no stale tick, `len`/`cap` 0, `AfterFunc` untouched, and three things a single-shot test cannot see: 200 `Reset`-to-imminent timers that must still deliver (the counter-property that keeps the drain honest), 600 ticker `Stop`/`Reset`-vs-firing races that must revoke exactly nothing, and two 600-timer batches armed against ONE absolute deadline and stopped/reset at that instant, every one of which must report `pending`. The last is what exposed mechanism 3's absence, and it only samples the window because the batch and the caller's sleep share a deadline — give each timer its own relative duration and the caller wakes milliseconds after the last tick has already landed, so the window is never entered and a broken implementation passes.

**Reach.** Timers gate roughly 35 stdlib suites. `encoding/base64`'s `TestDecoderIssue3577` and four of `internal/singleflight`'s five tests flip to passing on this alone; `singleflight`'s fifth needs 1000 *simultaneously parked* goroutines, which was the separately-documented cooperative-scheduler limitation in golib's `goǃ` (a goroutine was a ThreadPool work item and held its thread while parked, so the 256-thread floor bounded how many could be parked at once), not a timer issue. The dedicated-thread executor retired that bound (`docs/phase4/DESIGN-cooperative-scheduler.md`).

## The runtime's PROCESS-CONTROL surface: implement the CONTRACT, never the mechanism

`runtime`'s public control API — `GC`, `GOMAXPROCS`, `Gosched`, `Stack`, `ReadMemStats`,
`LockOSThread`/`UnlockOSThread` — converts faithfully and **compiles**, then dies on its first call.
Each body drives machinery that has no managed counterpart: `GC()` → `gcStart` → `acquirem` →
`getg()`; `GOMAXPROCS(n)` → `stopTheWorldGC` → `semacquire` → `getg()`; `Gosched()` →
`mcall(gosched_m)`. `getg` and `mcall` are Go **compiler intrinsics** — a register read and a stack
switch — so the [`PartialStubGenerator`](source-generators.md#source-generators) fills them with a throw, and the throw
lands wherever the caller ran. On a goroutine (a managed thread) that is an unhandled exception and
**terminates the process**: sync's suite lost 28 of its 51 tests to one such throw.

The ruling is the one sync's `Mutex`/`notifyList` established: **where a Go mechanism has no managed
counterpart but its public contract does, reimplement the CONTRACT at the API boundary and never
emulate the mechanism.** Synthesizing a plausible `g`/`m` so the converted scheduler can walk it buys
nothing — the code underneath still wants a real run queue, a real heap and real stacks. The seven
declarations above are dropped from emission by the type-level registry
(`manualConversionFuncs["runtime"]`) and answered in a hand-owned `runtime/managed_impl.cs`;
everything below them stays auto-converted and simply becomes unreachable.

| Go API | Managed realization | Divergence |
|---|---|---|
| `Gosched()` | `Thread.Yield()` | none — same "offer the rest of the slice, then continue" contract |
| `GOMAXPROCS(n)` | remembered value, defaults to `Environment.ProcessorCount`; `n < 1` queries | does **not** cap parallelism — the CLR schedules goroutine threads. The universal save/restore idiom `defer runtime.GOMAXPROCS(runtime.GOMAXPROCS(n))` is exactly right |
| `GC()` | blocking compacting collect → `WaitForPendingFinalizers` → collect → **`GcPauseRecorder.Drain()`** | none observable; the second pass reclaims what finalizers released, which is the state a completed Go cycle leaves. The drain is what makes `NumGC` current on return |
| `Stack(buf, all)` | managed `StackTrace` text into `buf` | cannot show frames that already unwound, and `all` reports only the calling thread (see below) |
| `ReadMemStats(m)` | `GC.GetTotalMemory`/`GetTotalAllocatedBytes` + one `GcPauseRecorder` snapshot | allocator-internal fields (`Mallocs`/`Frees`/`HeapObjects`/`BySize`) and `GCCPUFraction` stay **zero** rather than invented; the pause history, `LastGC`, `PauseTotalNs`, `NumGC`, `NumForcedGC` and `HeapReleased` are **real** (see below) |
| `LockOSThread`/`UnlockOSThread` | no-ops | no-ops **by construction**: a goroutine already *is* a managed thread, so the guarantee they exist to provide holds unconditionally |

`Stack`'s divergence is worth stating precisely because it looks like a bug: Go keeps the panicking
frames alive until the panic completes, so `debug.Stack()` called from a deferred function *during* a
panic shows the panicking stack; a CLR exception pops those frames before the `finally` that runs the
defers, so the managed trace shows the deferred frame's stack instead (sync's
`TestOnceFuncPanicTraceback`).

**`runtime/debug`'s knobs are the same ruling one package over.** `runtime/debug/stubs.go` declares
`setGCPercent`/`setMemoryLimit`/`setMaxStack`/`setMaxThreads`/`setPanicOnFault`/`readGCStats`/
`freeOSMemory` with no body ("Implemented in package runtime"), bound by the Go linker to a runtime
function carrying the matching `//go:linkname … runtime/debug.<name>` **PUSH**. go2cs has no
cross-assembly linker, so each became a throwing stub — `debug.SetGCPercent`, the first line of
sync's `TestPool`. Forwarding to the converted runtime bodies would not help (they take the mheap
lock on the system stack and wait on a mark cycle), so `runtime/debug/stubs_impl.cs` answers the
contracts: the tuning knobs keep Go's documented GET/SET semantics (remember, return the previous
value, negative = query where Go says so) and have no effect on collection; `freeOSMemory` is a real
compacting collect; `setPanicOnFault` is `[ThreadStatic]` because it is per-goroutine in Go;
`readGCStats` reports a **real** per-pause history in the exact packed layout `ReadGCStats` unpacks.
`modinfo`/`WriteHeapDump`/`SetTraceback`/`runtime_setCrashFD` are inert, matching a binary built
without module or heap-dump support.

**Two assembly primitives DO have exact managed forms** (`runtime/stubs_impl.cs`).
`systemstack(fn)` is `fn()` — Go's own contract already says that a caller already on a system stack
"calls fn directly and returns", and in the managed model there is one stack per goroutine and no g0
to switch to, so that is the only branch. `procyield(n)` is `Thread.SpinWait((int)n)`. Everything
else in `runtime/stubs.go` deliberately keeps throwing, `getg`/`mcall` included: a loud, locatable
failure beats quietly operating on a fabricated goroutine descriptor.

**`internal/runtime/atomic` is the NATIVE half of the S1 fork and gets a real conversion, not a
stub** (`atomic_impl.cs`). Its ~40 declarations are all `.s` files, but they are plain memory atomics
over native scalars, and the CLR has an exact equivalent: `Xadd*`→`Interlocked.Add` (both return the
NEW value), `Xchg*`→`Interlocked.Exchange` and `And32`/`Or32`/`And64`/`Or64`→`Interlocked.And`/`Or`
(all return the OLD value), `Cas*`→`CompareExchange`, `Store*`/`StoreRel*`→`Volatile.Write` (a
release store in both models). Two widths have no intrinsic and take a shared latch instead: 8-bit
(`And8`/`Or8` — the CLR has no byte-width `Interlocked`) and the `unsafe.Pointer` family
(`Casp1`/`StorepNoWB`/`storePointer`/`casPointer` — golib models `unsafe.Pointer` as a class wrapping
a `uintptr`, so a CAS must compare the wrapped NUMBER and swap the REFERENCE, which no single
intrinsic expresses); `Xadduintptr`/`Anduintptr`/`Oruintptr` ride a CAS loop because `Interlocked`
offers `Exchange`/`CompareExchange` for `nuint` but not `Add`/`And`/`Or`. Leaving this package
stubbed poisons everything built on it — `runtime.SetMutexProfileFraction`, an otherwise perfectly
faithful conversion, died on `Store64` (sync's `TestMutex`).

### `unsafe.Pointer(uintptr(0))` must compare equal to `nil`

**`unsafe.Pointer(uintptr(0))` must compare equal to `nil`.** The converter bridges every
`unsafe.Pointer`-valued call through `uintptr`, because `unsafe` lives in its own assembly and can
carry no implicit conversion on the core pointer class. golib's round-trip therefore has to preserve
nil in *both* directions, and it did not: `Pointer(uintptr)` produced a non-nil box wrapping 0, and
`(uintptr)ptr` dereferenced a nil-constructed box instead of yielding 0. The symptom was silent and
far away — sync's `poolDequeue` reads each ring slot's `typ` word to decide whether it is free, every
empty slot came back "occupied", `pushHead` returned false forever and `TestPoolDequeue`/
`TestPoolChain` spun. `unsafe.cs` now marks the zero address nil (a protected `ж<T>(value, isNull)`
constructor holds the address *and* the nil flag) and tolerates a nil box on the way out.

### `runtime.Goexit` unwinds the goroutine with a `GoexitException`

**`runtime.Goexit` unwinds the goroutine with a `GoexitException`, and every one of Go's three
properties falls out of machinery that already existed.** Go specifies that Goexit ends the calling
goroutine only: its deferred calls all run, `recover()` inside them returns **nil** (a Goexit is not a
panic, and a defer cannot cancel it), and other goroutines are untouched. The managed form is a golib
`GoexitException` that is deliberately **not** a `PanicException` — `recover()`'s implementation keys
on `PanicException` (the frame's `GoFrame.IsPanic` filter, via `RuntimeErrorPanic.TryAsPanic`), so it
is blind to this type BY CONSTRUCTION and the recover path needs **zero** special handling. The defers
still run because `GoFrame.Run()` sits in a `finally`, draining the defer list during the unwind
exactly as it does for a panic, across frames. Every `go` statement dispatches through one **goroutine root**
(`golib.Goroutine.Start` → `Run`, the single site all 18 `builtin.goǃ` arity overloads funnel into),
which catches `GoexitException` and ends that thread silently; a `PanicException` reaching the same
point is deliberately NOT caught and keeps its Go-faithful fatal path (stderr report, exit 2 — guarded
by the `GoroutinePanicExitCode` behavioral test). `runtime.Goexit`'s body is hand-owned in the runtime
package's `managed_impl.cs` (`manualConversionFuncs`), since the converted body drives Go's own
`_panic` record and stack unwinder (`getcallerpc`/`nextDefer`/`goexit1` — all assembly). Guarded by
the `GoexitDefers` behavioral test (defers run across frames, `recover()` sees nil, other goroutines
still run, main continues) and by sync's `TestOnceFuncGoexit`, which this unblocked.

**Goexit from the MAIN goroutine stays gated — at runtime, not statically.** There, Go ends `main`
without returning while the *program continues* running its other goroutines, crashing with
"no goroutines" once they all exit; that needs a live-goroutine registry and a main-thread parking
protocol the managed model does not have (`docs/phase4/DESIGN-goexit.md` option C). The distinction is
not statically decidable — a function's call graph says nothing about which goroutine will run it — so
`Goroutine.OnGoroutine` (a `[ThreadStatic]` the root sets and restores, because goroutines run on
pooled threads) answers it at the call, and the main-goroutine case throws a loud
`NotSupportedException` naming the design doc rather than silently doing something else. Consequently
`unsupportedRuntimeCapabilities` (`testConversion.go`) is now **empty**: the mechanism remains — a test
whose transitive closure reaches a listed symbol converts as `unsupported`, disclosed by name, the
same gate an unsupported `testing.*` member uses — with `TestUnsupportedRuntimeCapabilityGate` as its
positive control so an empty list cannot masquerade as a working lookup. Add an entry only for
something *provably* unavailable, never merely unimplemented, and scan every validated package for the
symbol first: gating one **removes** tests from a banked package's run set, the mirror of the widening
trap.

### `-test-timeout` is the PACKAGE deadline, and it reaches both sides

**`-test-timeout` is the PACKAGE deadline, and it reaches both sides.** The flag used to bound only the
child *process*, leaving `go test` and the converted host each on its own **10-minute** default — so no
value of it could let a slower-than-Go suite finish. `hash/maphash` self-terminated at exactly 600 s
under `-test-timeout 40m`, reporting its still-running `TestSmhasherAvalanche` as an empty verdict that
reads exactly like a real failure (the very "slow ≠ hung" trap, one layer below where it was known).
The compare/run actions now pass the flag through as `go test -timeout` **and** the host's own
`-timeout`, and kill the child one minute later (`testChildTimeoutGrace`) purely as a safety net — the
in-process deadline must fire first, because it is the side that writes results. Threading it also
exposed that the host's duration parser accepted only a *single* unit, rejecting `time.Duration`'s own
`String()` output (`30m0s` → "invalid Go-style duration", exit 2 before a test ran); it now parses Go's
real grammar, a sequence of decimal-and-unit pairs over ns/us/µs/ms/s/m/h, so `1h30m` works as it does
for `go test`. maphash's C# suite needs ~15 min where Go's needs 7.6 s — a performance gap, recorded as
such, not a correctness one.

### The test host treats an escaping `GoexitException` as Go's `tRunner` does

**The test host treats an escaping `GoexitException` as Go's `tRunner` does**, and each test's
dedicated thread is marked a goroutine (`Goroutine.Enter`) because in Go a test body IS one. Go's
`FailNow` is *specified* as "mark failed, then `runtime.Goexit`", so a Goexit escaping a test body
means the test ended without completing — Go reports `errNilPanicOrGoexit` ("test executed panic(nil)
or runtime.Goexit") against that test. The host logs exactly that text and fails the one test, where
Go additionally panics the whole binary; keeping the run alive leaves the rest of the package
measurable. This is also the path `testing.T.FailNow` fidelity will take.

### An unhandled NON-panic exception on a goroutine fails ONE test

**An unhandled NON-panic exception on a goroutine fails ONE test instead of killing the host — and
that containment is TEST-HOST-ONLY.** A converted program keeps Go's fidelity (an unhandled failure in
a goroutine is process death), which is golib's default: nothing contains it and it reaches the
AppDomain backstop. A host that runs many independent Go programs in one process is different — the
crash discarded every result not yet written and blanked the tail of the package, so one defect read as
a mass infrastructure wall. `Goroutine.ContainUnhandledExceptions(policy)` lets such a host install a
containment policy, which golib consults through an exception **filter** (never a catch-and-rethrow, so
the uncontained path is bit-for-bit the old behavior — unhandled, stack intact, intervening `finally`
blocks unrun). A panic is never offered to a policy. The host attributes the failure to the right test
with an `AsyncLocal` set on the test thread: it flows with the `ExecutionContext` that `Thread.Start`
captures, which is exactly how golib dispatches a goroutine, so the attribution survives goroutines
spawning goroutines. If the crashed goroutine was the one that would
have unblocked its test, that test now waits for the package timeout — which still writes every result
gathered so far, where the crash wrote none.

### A PANIC on a goroutine still kills the host

**A PANIC on a goroutine still kills the host — but it no longer takes the run's evidence with it
(2026-08-14).** Containment stops at panics deliberately (above), so the fatal path stayed: golib's
backstop printed `panic: <value>` and exited 2. For a converted PROGRAM that is right and complete —
it is Go's own report. For a host it was the hardest possible diagnostic: no frame, no test name, and
every verdict already gathered discarded unwritten. `net/rpc/jsonrpc` is the case that made it
concrete — a `reflect`-allocated `[1]int` reply panicked on an rpc goroutine, the host died, and the
package recorded **0** verdicts where it had been recording 6, so a strictly BETTER corpus read as a
worse one and the one real failure read as three.

`Goroutine.ObserveUnhandledPanic(observer)` is the other half of the containment pair, and it
contains nothing: the observer runs from an exception filter that **always declines**, so the panic
still reaches the backstop with its stack intact and the intervening `finally` blocks unrun —
byte-identical to an unobserved run, which is what keeps Go's fidelity and the differential oracle's
view of it. What the filter buys is the moment BEFORE the unwind. The test host uses it to attribute
the panic to the test whose goroutine it was (the same `AsyncLocal`), report that test's terminal
`fail` carrying the full traceback (`PanicException.StackTrace`, which prefers the panic's ORIGIN over
the frames it unwound through), and flush the result and JUnit files. A goroutine panic now costs the
TAIL of a run rather than all of it — which is also what `go test` shows, since a Go binary that dies
this way has already streamed the verdicts it reached. Guard:
`GolibTests/GoroutineRootPanicTests`, over the root's whole policy — a panic observed and still
escaping, its fault site surviving, a mapped runtime-error panic, a non-panic exception still going to
containment and not to the observer, and `Goexit` reaching neither.

### The unhandled-exception backstop prints the whole exception chain

**The unhandled-exception backstop prints the whole exception chain for a NON-panic failure.** A real
panic keeps Go's report shape (`panic: <value>` on stderr, exit 2). Anything else is a *defect to
diagnose*, and `ex.Message` alone threw the evidence away: a `TypeInitializationException`'s own
message merely names the type and says "see inner exception", so the actual fault and its stack were
lost (a whole `gob` run's real cause was invisible this way). The backstop now writes `ex.ToString()`
for the non-panic case, carrying the full inner-exception chain and stacks.

## The GC measurement surface — one recorder, one ring, one snapshot

The recorder is `golib/runtime/GcPauseRecorder.cs`, landed 2026-08-21; the design and measurements are in
[`docs/phase4/DESIGN-readmemstats-surface.md`](../phase4/DESIGN-readmemstats-surface.md). `ReadMemStats`
and `readGCStats` used to answer independently, and both left the per-cycle facts zero — which made
`runtime/debug`'s `TestReadGCStats` fail on two *length* assertions comparing the two surfaces to each
other. The cheap way to make those pass is to report `NumGC = 0` on both; that is refused, because it
would destroy a fact the CLR genuinely measures in order to satisfy an assert. Instead one definition
is applied uniformly — **a Go GC cycle is a CLR gen2 collection**, which is what `NumGC` already meant —
and one recorder supplies the missing half:

* the mechanism is a **resurrecting finalizable sentinel**: an object nothing strongly references,
  whose finalizer records the collection and then calls `GC.ReRegisterForFinalize(this)`, so it wakes
  once per gen2 collection. `GC.RegisterForFullGCNotification` was refused (it requires background GC
  off, process-wide), an in-process EventPipe listener was refused (events arrive ~117 ms late), and
  polling from each read was refused (it loses every collection between two reads, which is a hole in
  a ring whose slots are indexed by cycle number);
* the ring is written in **Go's own order** — slot `observed % 256`, *then* the counter — so
  `MemStats`' documented "the most recent pause is at `PauseNs[(NumGC+255)%256]`" and `ReadGCStats`'
  backwards walk line up by construction rather than by agreement;
* **`NumGC` is the recorder's count, not `CollectionCount`**, so the two surfaces cannot disagree. It
  can lag the true gen2 count by at most one collection, for at most the finalizer's scheduling
  latency — understating, never inventing — and `runtime.GC()`/`debug.FreeOSMemory()` **drain** the
  recorder before returning, which closes the lag at the one boundary Go's tests read it across;
* **`HeapReleased = max(0, committedHighWater − currentCommitted)`** over `TotalCommittedBytes`. A
  *cumulative* decrease would be monotone; Go documents the field as a current quantity that falls
  when the heap reacquires, and the monotone form was measured drifting ~33.6 MB per release cycle;
* **`ReadMemStats` is allocation-free**, and that is a landing precondition rather than a nicety:
  `net/textproto`'s banked `TestReadMIMEHeaderAllocations` brackets each header read between two
  `ReadMemStats` calls. `GC.GetGCMemoryInfo()` allocates a `GCMemoryInfoData` box per call (288 B
  measured), so the committed/heap-size figures come from the recorder's own per-gen2 sample and the
  ring is copied into the caller's already-allocated `array<T>` backing. Guarded at **zero** by
  `GolibTests.GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation`;
* always on, armed from `runtime`'s (and `runtime/debug`'s) module initializer, with a
  `GO2CS_GC_PAUSE_HISTORY=0` escape hatch that restores the pre-recorder answers exactly. Measured
  cost: one finalizer run and one `GetGCMemoryInfo` call per gen2 collection — below the noise floor
  of a 1.25–1.64 ms collection.

Guarded by `GolibTests.GcPauseHistorySurfaceTests` (the two surfaces held against each other,
`HeapReleased` across a `FreeOSMemory`, `NumForcedGC`, and the refused-fields-stay-zero rule).

## `iter.Pull`'s coro — a symmetric handoff between two threads, and the goroutine count that had never been wired

**The narrowest cut yet through a scheduler primitive, and the one where the converted code stayed the specification.** `iter.Pull`/`Pull2` are built on two `//go:linkname` entries into the runtime, `newcoro` and `coroswitch` (`iter/iter.go:213–217`). Go describes a coro as *"a special channel that always has a goroutine blocked on it"*: `coroswitch(c)` makes the caller the blocked party and starts the party that was blocked, so control alternates and the two contexts are never runnable at once. `coroswitch_m` implements that by swapping stacks and ending in `gogo`.

**Everything else in `iter` converts faithfully, and that is why the hand-own is four methods rather than a package.** The yield closure, the `yieldNext` handshake, the `done` latch, and the deferred `recover` that turns a panic *or* a `runtime.Goexit` inside the sequence function into a `panicValue` the pulling side re-raises are ordinary Go, and `iter.cs` reproduces them line for line — panic texts (`iter.Pull: next called again before yield`) included. Only the control TRANSFER has no managed counterpart, so the seam is drawn exactly there. The converter already emits the two linkname declarations as bodyless `partial`s, so **no `manualConversionFuncs` entry is needed for them** — there is no Go body to displace — and `core/iter/iter_impl.cs` simply supplies the implementing halves. (Without it the `PartialStubGenerator` fills both with throwing stubs, which is why `iter.Pull` raised `NotImplementedException` on first use.)

The transfer itself is golib's (`go.golib.Coro`), not `iter`'s, because it is a runtime capability: Go declares it in `runtime/coro.go`, and the corpus carries the mechanically converted, permanently dead counterpart at `runtime/coro.cs`, whose body bottoms out in the `mcall`/`getg`/`newproc1`/`gogo` stubs. Four decisions are worth cribbing:

- **A thread and two one-permit semaphores, because the callee is arbitrary converted Go.** The sequence function needs a real stack, which under the CLR means a real thread — the same conclusion `Goroutine` reaches for goroutines. Capacity **one** is the point: a permit is a TURN, not a count, so a second release before the peer consumes the first would mean both sides were runnable, and `SemaphoreSlim` throws rather than letting that pass. The alternatives (an iterator state machine, a `Task`) both require the CALLEE to be written for them.
- **The coro goroutine is a real goroutine, registered before `newcoro` returns.** Go's `newcoro` creates the g synchronously and `NumGoroutine` counts it from that moment — `iter`'s own tests assert one extra goroutine on the statement *after* `Pull` returns. A thread left to register on its own time makes that count race, so `Coro.Start` waits for the handshake. **The exit side is the mirror and matters as much**: the identity is retired BEFORE the caller is released, so a puller can never observe a count that still includes a coro which has finished.
- **Panics and Goexit cross by not being special.** The body runs under `Goroutine.Run` — the same root every `go` statement uses — so a `GoexitException` ends the coro goroutine after its defers have run, a host containment policy still contains an infrastructure failure to one test, and an unrecovered panic keeps Go's fatal path. Nothing is re-implemented. What the caller sees is whatever the body recorded in the closure both sides share, which is Go's own mechanism rather than an emulation of one. The release sits in a `finally` so an escaping panic **crashes rather than hangs**: Go's outcome is process death either way, but a peer parked on a permit nobody will release wedges the run instead of reporting.
- **The token is keyed, not widened.** `iter` declares `type coro struct{}` — an empty struct whose only job is to be a token the two functions agree on. There is nowhere in it to put a rendezvous, and widening it would both diverge from Go's field set and change how `GoZeroSizeFacts` classifies it. A `ConditionalWeakTable` keyed on the `ж<coro>` box leaves the converted type exactly as Go declares it (`new(coro)` mints a fresh `StandardBox` per call, and boxes compare by IDENTITY — the property `sync`'s semaphore table already relies on), and the entry cannot outlive the token.

**`runtime.NumGoroutine` was wired in the same change, and it had never answered truthfully.** Its Go body is `gcount()`, which derives the live count by SUBTRACTION over scheduler state the managed model never populates — `allglen`, less `sched.gFree.n`, less `sched.ngsys`, less each P's `gFree.n`. Every term was zero, and `gcount`'s own `if n < 1 { n = 1 }` floor then turned the nonsense into a plausible-looking constant: **`NumGoroutine()` returned 1 for every program, forever** — exactly the shape of wrong that survives unnoticed, because a single-goroutine program's answer really is 1. golib's `Goroutine` registry had maintained the true count all along (it is what the SIGQUIT dump already printed), so this is a WIRING rather than an approximation: a `manualConversionFuncs` entry displaces the auto body and `managed_impl.cs` returns `Goroutine.Count`. Go's staleness caveat carries over unchanged and for the same reason; `gcount`'s floor does not, since the registry cannot report fewer than the caller's own goroutine.

Guarded on both sides: `iter`'s converted suite validates **28/28** against `go test` (goroutine accounting, double-next/double-yield, panic-through on `next` and on `stop`, Goexit across the boundary, immediate stop), and the `IterPullRendezvous` behavioral test pins the rendezvous semantics against `go run` in the corpus gate — deliberately printing no goroutine counts, since a count is stable in Go only under the stabilization loop that suite uses, and a flaky stdout comparison would fire across the whole corpus rather than in one package.

## `sync.Pool` — a managed-reference ring slot, and a thread-affine stand-in for the P pin

`sync.Pool` is the third shape of the same wall, and the most instructive: **the raw-metal type is not
a pointer-in-an-integer, it is the `any` itself.** Go's `poolDequeue` is a lock-free ring of `eface`
slots — the two-word `{type, value}` form of an interface — and its whole ownership protocol hangs on
the TYPE word: a slot is empty **iff** `typ == nil`, and a consumer publishes "done with this slot" by
atomically storing nil into `typ` alone, leaving `val` to be overwritten later. Under the CLR an `any`
is ONE reference, so the literal conversion reinterprets the two-word struct as an `any`
(`Unsafe.As`) and the `typ` word ends up doing double duty as both the type tag and the value. Two
failures follow immediately, and both were observed: a stored value read back through the
reinterpretation surfaces as its own type word (`panic: interface conversion: interface {} is
unsafe.Pointer, not int`), and the empty-slot sentinel — a nil `unsafe.Pointer` — is indistinguishable
from a *stored value of that type*, so `pushHead` and `popTail` disagree about who owns a slot and the
ring corrupts or wedges. `TestPoolChain` took the whole test host down with the panic above, which cost
the 14 tests that sort after it alphabetically.

The fork is confined to the **slot representation** and keeps every other line of Go's algorithm —
the packed `head`/`tail`, the fullness test, the CAS protocol, the single-producer/multi-consumer
contract, and the entire `poolChain` half:

```csharp
// eface is Go's two-word {type, value} representation of an `any`. Under the CLR an `any` IS a single
// managed reference, so the slot holds that reference directly.
[GoType] partial struct eface {
    internal any? val;
}
```

`null` is the empty-slot sentinel — a state no stored value can forge — and a private singleton stands
in for Go's typed-nil `dequeueNil(nil)` marker (a slot holding a *nil interface value* must still read
as occupied). That also collapses Go's two-step release into one: with a single word there is nothing
to tear, so **one** `Volatile.Write(ref slot.val, null)` both clears the value and hands the slot back
to the producer, where Go needs a value store followed by a publishing `atomic.StorePointer` on `typ`.
`TestPoolDequeue` proves the release protocol end to end — 2·10⁶ items through a **fixed** 16-element
ring, with the head/tail seeded 500 short of wrapping.

`Pool` itself is a whole-file hand-own for a different reason: its `[P]poolLocal` shard block is
reached by **pointer arithmetic** through an `unsafe.Pointer` (`indexLocal`), which is meaningless when
the block is a managed array, and `procPin` — the thing that gives each shard's dequeue its *single*
producer — has no P to pin to. Go's algorithm survives intact (private slot → the shard's shared chain
→ steal from other shards' tails → the victim cache; `poolCleanup` ageing local → victim → dropped);
three pieces are replaced:

| Go mechanism | Managed realization | Divergence |
|---|---|---|
| `[P]poolLocal` block + `indexLocal` pointer arithmetic | a `poolLocal[]` held directly, one heap object per shard | the cache-line pad against false sharing is gone — separate objects, nothing to pad |
| `procPin()` → P id, preemption off | **thread-affine** index: a thread draws a sticky id once, in arrival order, folded into the current `GOMAXPROCS`; `procUnpin` has nothing to undo | threads outnumber shards, so two threads CAN share one shard — the one thing a real pin rules out. Closed on Pool's side: the private slot is claimed/taken with a single `Interlocked` step, and the shard's shared-chain HEAD (single-producer by contract) is serialized by a per-shard producer gate. Stealing (`popTail`) stays lock-free, as designed |
| `poolCleanup` at the start of every GC cycle, world stopped | registered with the runtime exactly as Go registers it (`runtime_registerPoolCleanup` → `runtime.GC()` invokes the hook, mirroring `gcStart` → `clearpools`) | narrower trigger: **requested** collections age the pool, automatic CLR collections do not, so a program that never calls `runtime.GC()` retains its cached items longer than Go's would. And the swap runs on the caller's thread, not under STW, so a `Put` racing it can land in a shard that just became a victim and age one cycle early. Both sit inside Pool's contract — *any item may be removed at any time* — so they cost a cache hit, never correctness |

The registration path is worth noting because it is a **general** cross-assembly constraint, not a Pool
detail: sync reaches the runtime through `//go:linkname runtime_registerPoolCleanup`, whose target
`sync_runtime_registerPoolCleanup` the exported-ness rule makes `internal` to the runtime assembly — and
[a linkname forwarder cannot bind an internal target across assemblies](#a-cross-package-golinkname-pull-emits-a-forwarder-not-a-throwing-stub).
A one-line `public` shim in `runtime/managed_impl.cs` is the crossing point, the same remedy
`blockUntilEmptyFinalizerQueue` already uses in `mfinal.cs`.

**What the arc could NOT satisfy, stated precisely.** `TestPoolGC` asserts that after draining a Pool
and collecting, at least `N-1` of `N` finalizers have run — a budget of exactly **one** straggler still
reachable "on stack or elsewhere". A drained Pool here holds nothing (proven with a `WeakReference`
probe: run the drain in its own frame and **zero** of 100 survive), but the test's own frame spends the
budget twice under an **unoptimized** build: the fill loop's `v` keeps the last item, and the JIT's
MinOpts codegen keeps the *discarded* return value of the final `Get()` reachable from the calling
frame for the rest of the method. That second straggler reproduces with no Pool in sight — a factory
whose result is discarded in a loop leaves its last object alive in Debug — and the test passes in
`Release`, where the JIT's liveness is precise. So it is a codegen-liveness divergence of the
Debug-configured CLR, not a Pool defect; the honest classification is the disclosed-divergence class,
not a contortion of the drain order to make one assert land. That class is now
[named and pinned](#codegen-liveness--a-frame-holds-what-go-has-already-dropped) as
`codegen-liveness`, with `TestPoolGC`'s straggler count pinned exactly so a real Pool retention
regression cannot hide behind the disclosure.

## A pointer's REFERENT, not its box, answers every lifetime and identity question

A `ж<T>` is a *pointer*, and go2cs mints them freely: `Ꮡ(s, i)` allocates a fresh box on every call,
and `Ꮡx.of(T.Ꮡfield)` allocates one per field access. The box is therefore an **expression
temporary** whose lifetime says nothing about the storage it names. Anything that asks a question
about the *object* — when does it die, is this the same object — must ask it of the referent.
`INilPointer.ReferentObject` (golib `ж.cs`) is that projection, and `ж<T>` resolves it the way
`Equals` already resolves pointer identity:

| Pointer shape | `ReferentObject` |
|---|---|
| array/slice element (`&s[i]`) | the canonical backing storage (`CanonicalElement` — the `T[]`, never a per-call header/view) |
| struct field (`&x.f`), including a nested `of()` chain | the **root** allocation, resolved recursively through the per-call intermediate boxes |
| standard heap box (`&x`, `new(T)`), whatever the pointee's type | the box itself — it *is* the allocation |
| native alias (a `uintptr` round-trip) | the box itself — the address it wraps names no *managed* allocation, so there is nothing GC-keyed to resolve to (the one place this projection and `Equals`, which identifies such boxes by that address, part company) |

Two consumers depend on it, and both were broken without it:

* **`runtime.SetFinalizer`** keyed its `ConditionalWeakTable` registration on the boxed `obj`. Go
  attaches a finalizer to the *object* a pointer points at — `runtime.SetFinalizer(&buf[0], f)`
  finalizes `buf`'s allocation — so keying on the throwaway `ж<byte>` the argument expression
  allocated registered against a lifetime nothing in the program shared: the finalizer became due
  the moment the box died (or, under a JIT that roots the whole frame, could never become due at
  all). It now keys on the referent, so the registration tracks exactly the allocation Go would
  finalize, and two boxes for the same address correctly share one registration (Go's "finalizer
  already set").
* **`sync.Cond`'s `copyChecker`**, below.

**`Ꮡ(IArray<T>, index)` takes its target BY VALUE, deliberately.** It used to be `in IArray<T>`.
`in` on an *interface* parameter elides no copy — it is already one reference — but it forces the
caller's boxing temp (a `slice<T>`/`array<T>` header is a struct, so every call boxes one) to be
**address-exposed**, and an address-exposed slot is not lifetime-tracked: the JIT reports it live for
the whole enclosing method. One `Ꮡ(s, i)` therefore pinned `s`'s backing array to the caller's frame
until that method *returned*, in fully optimized code. Measured with a `WeakReference` probe against
a `DOTNET_TieredCompilation=0` build: the `in` form leaks the array, the by-value form releases it.

## `sync.Cond`'s copy detector, on reference identity rather than an address

Go's `copyChecker` is a `uintptr` that stores **its own address** and compares:

```go
if uintptr(*c) != uintptr(unsafe.Pointer(c)) &&
   !atomic.CompareAndSwapUintptr((*uintptr)(c), 0, uintptr(unsafe.Pointer(c))) &&
   uintptr(*c) != uintptr(unsafe.Pointer(c)) {
	panic("sync.Cond is copied")
}
```

Neither half survives conversion, and the two failure modes point in opposite directions:

* Storing an **address** is unsound on a moving collector. The GC relocates the box holding the
  `Cond`, so a compaction between two `Wait` calls would leave a stale word in a `Cond` nobody
  copied — a *spuriously panicking* condition variable, which is far worse than no check.
* The auto body is **inert**. The address-of-self operand converts to
  `@unsafe.Pointer.FromRef(ref c)` while the CAS destination converts to `Ꮡ((uintptr)(c))`, which
  boxes a **copy** of the value — so the compare-and-swap writes to a throwaway box, the checker is
  never initialized, and `check()` never panics however often the `Cond` is copied. `TestCondCopy`
  failed with `got <nil>, expect sync.Cond is copied`.

`cond_impl.cs` (registered as `manualConversionFuncs["sync"]["copyChecker.check"]`, so only this one
method is hand-owned — the `copyChecker` type and every `Cond` method stay auto) asks the same
question against the invariant the CLR does guarantee: the checker stores a token derived from its
`ReferentObject`'s GC-stable identity hash. Every `Cond` method reaches its checker as
`Ꮡc.of(Cond.Ꮡchecker)`, whose referent is the *root* allocation holding the `Cond` — the same object
on every access, including for a `Cond` embedded in a larger struct, which is exactly why the
referent projection must recurse. A struct copy carries the ORIGINATING allocation's token into a new
one, which is what `check()` sees.

Two deliberate simplifications, both in the safe direction:

* **No compare-and-swap.** Go needs one because concurrent first-users race to publish the word;
  here every racer computes the *same* token for the same allocation, so an aligned native-word store
  cannot publish a value any racer would disagree with.
* **Identity hashes are not unique**, so a copy between two colliding allocations goes unreported —
  a missed detection, never a false alarm. Likewise, two elements of one backing array both resolve
  to that array, so a copy between them is invisible (an element pointer's identity is the storage
  plus an index the checker has no room for). Recorded, not worked around: Go's own check is
  documented as best-effort.

## `runtime.Stack` renders a GO-shaped traceback, and recovers the panic site

A traceback is **observable output**: Go programs print it on a crash, and Go's own tests grep it by
package-qualified function name (`sync`'s `TestOnceFuncPanicTraceback` looks for
`sync_test.onceFuncPanic`). Two things made the CLR trace unusable for that.

**Frame names.** A converted package's frames live on a `<pkg>_package` class inside namespace `go`,
so the CLR renders `at go.sync_test_package.onceFuncPanic() in …oncefunc_test.cs:line 191` — which
contains `sync_test_package.onceFuncPanic`, never `sync_test.onceFuncPanic`. `Stack` now formats
frames itself, in Go's shape (`<pkg>.<Func>()` then a tab-indented `<file>:<line>`), mapping
`go.<a>.<b>_package` → `<a>/<b>` (Go names a package by its import path, which the namespace mirrors)
and a closure's `<Outer>b__N` on its display class → `Outer.funcN`, Go's own spelling for a function
literal. A frame that is **not** converted Go code — golib, the BCL, the test host — keeps its .NET
name rather than being given an invented Go one, and Go's `+0x<offset>` PC deltas are omitted.

**Frames that already unwound.** Go keeps a panicking goroutine's frames physically on the stack until
the panic completes, so a `debug.Stack()` inside a deferred function shows the panic site; a CLR
exception has unwound them before the `finally`-based defer runs. Worse, *both* ways a panic travels
destroy the trace: re-raising the same instance (`throw ex`) resets `Exception.StackTrace` to the
re-raise point, and Go's re-panic idiom — `defer func(){ p := recover(); panic(p) }()`, which is
precisely how `sync.OnceFunc` replays a panic on every call — creates a brand-new panic in the
deferred frame. So golib snapshots the origin **once**, at the first (innermost, deepest) catch, into
`PanicException.PanicTrace`, and a panic raised while handling another *inherits* it. `GoFrame.Run()`
tracks which panic a deferred sequence is handling in a strictly save/restore-scoped thread-local
(`HandledPanic`, surfaced as `GoFuncRoot.InFlightPanic`) — `recover()` clears `CapturedPanic`, but
Go's traceback keeps showing the panicking frames for the rest of the sequence, and the strict scoping
is what stops the value from ever going stale. `Stack` appends those frames *below* the live ones,
which is where Go's traceback puts them too, since the deferred call runs on top of `gopanic`. Cost on
the non-panicking path is zero: the CLR fills the trace at throw time anyway, and nothing is
snapshotted unless a panic is actually caught.

## `runtime.Stack(all: true)` enumerates every goroutine, and each one names the wait it is parked on

The `all` flag was **read and ignored**: the header was a literal `goroutine 1 [running]:` and the dump
carried the calling thread alone. Both halves were untrue for any program with more than one
goroutine, and the second was a *contract* violation rather than a cosmetic one — the bracketed word
in a header is Go's **wait reason**, the runtime publishes it at every `gopark`, and Go's own tests
grep it (`runtime/pprof`'s `awaitBlockedGoroutine` builds a regex around
`^goroutine \d+ \[sync\.Mutex\.Lock\]:`; `runtime`'s `TestNumGoroutine` counts headers and requires the
total to equal `NumGoroutine()`).

**Park accounting** is what makes the word answerable. golib's `Goroutine` carries ONE field — a
`WaitReason`, whose `Zero` value *is* "running", so the state and the reason cannot disagree — and
every blocking primitive names its wait around the wait it already performs
(`docs/phase4/DESIGN-cooperative-scheduler.md` §5.3):

```csharp
using (Goroutine.Park(WaitReason.ChanReceive))
    parked.Park.Wait();                       // the EXISTING primitive, untouched
```

The scope is **accounting only**. It relocates no wait, re-opens no park/claim protocol, and adds no
`goready` side: the waker already signals the primitive and the woken thread un-marks itself when the
scope disposes. Cost is one volatile store to park and one to unpark, no allocation (`ParkScope` is a
`readonly struct`), and nothing per-`ж` or per-object — so there is no corpus-wide byte cost. A thread
with no goroutine identity (a BCL callback, `time`'s timer service thread, the finalizer) gets an
inert scope and writes nothing; a nested park restores the enclosing reason rather than clearing it.

The reasons are Go's own strings, copied verbatim from `runtime2.go`'s `waitReasonStrings`, and the
enum carries only the ones a go2cs park site actually sets — a reason nothing can set is a word no
dump can print. Adopted sites and the reason each names:

| Wait | Reason | Go's counterpart |
|---|---|---|
| channel send / receive | `chan send`, `chan receive` | `gopark(chanparkcommit)` |
| blocked `select` | `select` | `gopark(selparkcommit)` |
| nil-channel op, `select{}` | `chan send (nil chan)`, `chan receive (nil chan)`, `select (no cases)` | same reasons |
| `sync.Mutex.Lock` | `sync.Mutex.Lock` | `sync_runtime_SemacquireMutex` |
| `sync.RWMutex.RLock` / `.Lock` | `sync.RWMutex.RLock`, `sync.RWMutex.Lock` | `sync_runtime_SemacquireRWMutexR` / `RWMutex` |
| `sync.WaitGroup.Wait` | `semacquire` | `sync_runtime_Semacquire` — Go has no `sync.WaitGroup.Wait` reason |
| `sync.Cond.Wait` | `sync.Cond.Wait` | `notifyListWait` → `goparkunlock` |
| `internal/poll` fdMutex | `semacquire` | `poll_runtime_Semacquire` |
| `internal/poll` netpoller | `IO wait` | `netpollblock` |
| `time.Sleep` | `sleep` | `gopark(resetForSleep)` |

**What the dump renders.** The calling goroutine first, with real frames, exactly as before; then one
blank-line-separated block per other live goroutine, in goid order, each with a truthful header — the
id golib's registry minted and the reason its park recorded. Where Go prints the other goroutine's
frames, go2cs prints ONE line, `[stack unavailable: go2cs does not capture another goroutine's
frames]`, deliberately shaped so nothing could mistake it for a frame (no tab-indented position line,
no package-qualified name). **The CLR has no supported cross-thread stack walk** — `Thread.Suspend` is
gone, and ClrMD or EventPipe would mean a process snapshot per call inside what is typically a spin
loop — so the honest answer is a sentence, and fabricating plausible frames for a stack that was never
walked is the one thing a traceback must never do. Capturing a goroutine's OWN stack at park time is
Stage B of the scheduler design, held behind the synthetic-PC registry that would symbolize it.

Three further limits are stated rather than approximated: `running` covers Go's `_Grunnable` as well
as `_Grunning` (there is no P and no run queue, so a thread waiting for a core is indistinguishable
from one on it); Go's ` (scan)`, `, N minutes` and `, locked to thread` header decorations are omitted
(no GC of ours, no park timestamp, and `LockOSThread` is a no-op here because every goroutine already
owns its thread); and `NumGoroutine()` and the enumeration are two reads of one registry, so they agree
at rest and can differ while a goroutine is registering or retiring — the same direction as the count's
already-documented early-climb/late-decay. (Guarded by the `GoroutineWaitState` behavioral test — four
goroutines parked on a mutex, a channel receive, a select and a WaitGroup, read back through a
*normalized* reading compared against `go run`, with a negative arm for a reason no goroutine has and a
release arm proving every reason clears; plus `GolibTests`'
`GoroutineParkAccountingTests` for the strings, the nesting, the inert host-thread park, and the
header-count-versus-`NumGoroutine` agreement.)

## EVERY reader of a panic's trace gets the origin — not just `runtime.Stack`

The snapshot above served exactly one consumer. Every *other* reader — the Phase-4 test host, an
unhandled-exception dump, a debugger — asked `Exception.StackTrace` and got the wreckage the section
above describes, because a panic reaches its reporter by being re-raised from `GoFrame.Run()`
(`throw CapturedPanic.Value`, once the deferred sequence declined to recover it) and re-raising a
stored instance resets the trace to the re-raise point. So **every panic in the corpus reported the
same deepest frame — the defer drain — regardless of where it actually faulted.** That reads as
a defect in the defer machinery, and it hid the real one: nine of `time`'s failures were filed as a
shared "nil pointer dereference in the defer drain" when they were one nil-receiver deref in
`Location.lookup` (see the normalization idiom above), invisible until the trace was honest.

Two changes, both at the layer that owns the fact:

1. **`PanicException.StackTrace` reports the panic SITE first, then the frames it unwound through** —
   the closest a CLR exception can come to Go's single uninterrupted traceback. `PanicTrace` already
   held the origin; overriding the property is what lets consumers see it without any of them knowing
   the panic machinery exists. With no origin recorded the base trace is returned unchanged, so the
   override only ever *adds*.
2. **The origin is snapshotted at the ADOPTION POINT, `RuntimeErrorPanic.TryAsPanic`** — the one place
   a .NET exception becomes a Go panic. A mapped runtime error (nil deref, divide by zero) is
   synthesized there and was never thrown at the fault, so only the incoming exception carries those
   frames; once `TryAsPanic` returns, they are gone. Doing it in each *adopter* covered only panics
   that passed through a frame — a function that never defers gets no frame, so its fault
   travelled raw to the reporter, which synthesized a brand-new panic with **no trace at all**. That
   was five of the nine `time` rows: `panic: …` and nothing else.

(Guarded by `GolibTests.PanicTracebackTests`: a synthesized runtime-error panic escaping a `GoFrame`, an
explicit `panic()` surviving the re-raise, the same runtime-error panic adopted with **no** frame
anywhere between fault and reporter, a recovered panic that must not be reported at all, and a panic
with no origin snapshot falling back to its intact base trace. Not a behavioral test: no converted Go
program reads a CLR stack trace, so there is nothing to output-compare — the Go-observable half is
`runtime.Stack`, covered above.)

## `runtime.Callers` / `Frames.Next` walk the managed stack projected to GO-LOGICAL frames

Go's programmatic traceback — `pc := make([]uintptr, N); n := runtime.Callers(skip, pc)` expanded by
`runtime.CallersFrames(pc[:n])` — bottoms out in machinery with **no managed form**: `Callers` →
`callers()` opens with `getcallersp()`/`getcallerpc()` (assembly intrinsics) and hands the raw stack
to the runtime `unwinder`, and `(*Frames).Next` resolves each PC through `findfunc`'s linker-built
`funcInfo` tables. Converted faithfully they compile, then die on the first step — `io`'s
`TestMultiReaderFlatten`/`TestMultiWriterSingleChainFlatten` (relative stack-depth asserts, the
flatten optimization's only observable) threw `getcallersp: … not implemented` from inside
`runtime.Callers` itself.

The fork is the PROCESS-CONTROL one (section above): both API **contracts** — "record the calling
goroutine's frames as opaque PCs" and "expand PCs to function/file/line" — the CLR answers natively,
so `Callers` and `Frames.Next` are hand-owned in `runtime`'s `managed_impl.cs`
(`manualConversionFuncs["runtime"]`) over `System.Diagnostics.StackTrace`, while **`getcallersp`
stays an honest `NotImplementedException` stub**: a caller's stack pointer has no managed answer, so
the chain is severed at the API boundary that does — the same severing rule the reflection bridge
applied at `methodName`. `CallersFrames` itself is pure Go (slice bookkeeping) and stays auto.

What makes the projection *semantically* faithful is the **Go-frame filter**: a managed frame counts
only when it is a function the **Go source declares** — a free function or `[GoRecv]` receiver on a
`go.*` `<pkg>_package` class, a method on a struct nested in one, or a function literal (its compiler
display class nests in the same scope). go2cs **dispatch machinery is invisible**, exactly as Go's
interface dispatch adds no frame: generated adapter shells (any `IGoAdapter`) and go2cs-gen's
synthesized forwarders (`[GeneratedCode("go2cs-gen", …)]`, e.g. RecvGenerator's `ж`-overloads) are
dropped, as is everything outside a package class (golib, the BCL, the test host). Without the
filter, one Go-level interface call contributes 2–3 CLR frames and every relative-depth assert is
off; with it, **depth deltas between two `Callers` calls on one goroutine match Go's logical model**
— `io`'s `readDepth == myDepth+2` holds bit-exactly. Absolute depth still reflects the managed
host's own frames below `main`/the test runner, which cancel in any same-goroutine delta.

PC values are **opaque interned tokens** (index+1 into a process-lifetime record table keyed on
module × method-token × IL offset; 0 stays the invalid sentinel), never addresses — stable across
recurrences so pc-equality behaves as in Go, resolvable by any later `CallersFrames` walk, like Go's
permanent PC space. `Frame.Function` uses the Go spelling (`goFrameName`: `io_test.TestCopy`,
`runtime/debug.Stack`, `Outer.funcN` for literals); `Frame.File`/`Line` name the **converted `.cs`**
source — the source that honestly exists; `Frame.Func` stays nil (contract-permitted) and
`FuncForPC` remains a stub, since a `*runtime.Func` has no managed referent. A PC the runtime never
minted resolves like Go's `!funcInfo.valid()` — skipped, not fatal — and `more` is computed
precisely over the remaining *resolvable* tokens, mirroring Go's two-frame prefetch. Known fidelity
edge, recorded: Go's `<autogenerated>` promoted-method wrappers can occupy PC slots in a raw Go
`Callers` capture, while here **all** synthesized wrappers (Go-side and go2cs-side alike) are
uniformly invisible — refine only if a consumer differential ever lands on it.

**The two test-variant suffixes are NOT symmetric — `_internal_test` is stripped, `_test` is kept
(2026-08-26).** `goFrameName` derives the reported import path from the emitted namespace plus
package-class name, and the `-tests` pipeline emits a package's two test variants as two separate
classes: `<pkg>_test_package` for an external suite (`package slog_test`) and
`<pkg>_internal_test_package` for an in-package one (`package slog`). Go treats those two cases
differently, measured against the go1.23.12 toolchain rather than reasoned about:

| the test file declares | Go names the frame | why |
|:--|:--|:--|
| `package callerprobe` (internal) | `callerprobe.TestInternalCallerName` | the file is compiled INTO the package under test, so there is no separate package to name |
| `package callerprobe_test` (external) | `callerprobe_test.TestExternalCallerName` | a genuinely separate Go package, and Go spells it |

So `_internal_test` is a go2cs emission detail that must be stripped back off, while `_test` is a
fact about the Go build that must survive. The tempting generalization — strip any trailing `_test`
— is wrong in a way a banked row already measures: `runtime/debug`'s own `TestStack` greps a
rendered traceback for `runtime/debug_test.(*T).ptrmethod`.

[`DESIGN-position-map.md`](../phase4/DESIGN-position-map.md) §8 records that these two suffix rules
retire from the FILE half (which is RECORDED, so nothing is derived from a namespace any more) but
"remain necessary and unchanged for the FUNCTION half"; the internal-test half of that rule was
never actually landed there, and the leak was **systemic to the `-tests` pipeline rather than
package-specific** — every converted suite that inspects caller info saw it. `log/slog`'s
`logger_test.go` asserts `wantFunc = "log/slog.TestCallDepth"` and got
`log/slog_internal_test.TestCallDepth`; the fix moves that row from 190 to 194 matching verdicts
(`TestCallDepth`, `TestJSONAndTextHandlers` and its `/Source` + `/Source/json` subtests, plus
`TestRecordSource`'s depth-1 case). (Guarded by `GolibTests/CallerFrameTestVariantNamingTests`,
which pins all THREE emitted shapes — production, internal-test, external-test — in one file, so
neither rule can be "fixed" into the other; measured failing-first, the guard reports exactly the
internal-test shape and leaves the other two green.)

**A frame set read at Release + `DOTNET_TieredCompilation=0` is the set of frames the JIT chose to
KEEP — literal-frame naming is inlining-dependent, and at the validation configuration of record the
frame can simply be gone (2026-09-04).** `Frame.Function` can only name a function literal while
that literal's own frame is on the stack, and the CLR's `StackTrace` does not report inlined frames
— the same fact `captureCallers`'s `[MethodImpl(MethodImplOptions.NoInlining)]` pins one layer down.
Tiering is what had been hiding it: a test method runs ONCE, so under default tiering it is jitted at
tier 0, where nothing is inlined and the literal frame is always there. The validation configuration
of record is **Release with tiering off**, i.e. full optimization from the first call — and there a
one-expression lambda is inlined into its enclosing method, leaving the `.funcN` suffix nothing to
attach to. Measured on `CallerFrameTestVariantNamingTests`, one build, both configurations: all
three literal shapes answered the ENCLOSING frame at TC0
(`litguard/probe.recordedOuterLiteralFrame` for a guard wanting `…recordedOuterLiteralFrame.func2`)
and were green under default tiering. The guard now carries
`[MethodImpl(MethodImplOptions.NoInlining)]` on the **lambdas** as well as on the enclosing methods
— an attribute on a lambda expression reaches its synthesized backing method (verified in an
isolated probe: `implFlags=NoInlining`) — so what it measures is the naming rule rather than an
inlining budget.

The naming rule itself is unchanged; what is recorded here is the **reading**. Anything that
consults a frame set at Release+TC0 — a relative-depth assert, a count of host frames, a traceback
grepped for a literal's name — is reading an inlining decision unless every frame it depends on is
pinned, and a frame inlined away is indistinguishable from one the Go-frame filter above declined to
report. Same family as reflect's `valueMethodName`, whose faithfully transcribed stack climb was
correct in Debug and could not work in Release, and which was retired for a `[CallerMemberName]`
thread precisely because a compile-time constant is the one answer no tiering or inlining decision
can move ([`DESIGN-reflection-bridge.md`](../phase4/DESIGN-reflection-bridge.md)); and the sibling of
the tier-0 liveness rule recorded further down this section, where the same "test methods run once,
so they are jitted at tier 0" fact makes a GC-lifetime local look permanently live. (The literal's
COUNTER is recorded rather than derived —
[`DESIGN-position-map.md`](../phase4/DESIGN-position-map.md) §8 — which is orthogonal to whether the
frame exists to be named at all.)

**`runtime.Caller` works by severing the FUNNEL, not by hand-owning another public API
(2026-08-07).** The 2026-07-31 landing above hand-owned the exported `Callers`, which left
`runtime.Caller` — the far more widely used of the pair — still dead: its auto body calls the
*lower-case* `callers`, and that is the declaration whose first statement is `getcallersp()`. Four
call sites go through it (`Caller`, `mprof`'s profile recorders, `proc`'s `createstack`,
`tracestack`), so `callers` is the single funnel and hand-owning **it** is one entry on
`manualConversionFuncs["runtime"]` that fixes all four — and leaves `Caller` auto-converted and
Go-shaped, which hand-owning `Caller` would not. Go's own comment on the declaration says the same
thing from the other side: "almost identical to `Callers`", linkname'd by the ecosystem, signature
frozen — an API boundary with a managed answer.

The two differ in exactly one way that the managed walk has to encode: `Callers(0, pc)` names *its
own* frame, while `callers(0, pcbuf)` names *its caller's* (Go's body starts the unwinder at
`getcallerpc()`/`getcallersp()`, one frame up). The walk therefore lives in a private
`captureCallers`, and each entry point passes its own frame budget — `Callers` → `skip + 1`,
`callers` → `skip + 2` — because **every hop is itself a Go-source frame by the filter above**: they
are all declared on `runtime_package`, so the walker cannot exclude them structurally. All three
carry `[MethodImpl(MethodImplOptions.NoInlining)]`: the CLR's `StackTrace` does not report inlined
frames where Go's unwinder does (through the compiler's inline trees), so an inlined hop would
silently shift every answer by one. Guarded by the `RuntimeCallerFrames` behavioral test, which
asserts *relations* between frames rather than absolute positions — chiefly that `callerLine()` and
`wrapGrand()` invoked **on one source line** must report the same line, an equality that breaks
under an off-by-one in either direction (positive control: `skip + 1` in `callers` flips three of
its eleven output lines). The suite builds Debug, where nothing is inlined, so the guard was also
run against a **Release** build of the same program — identical output, so the pins hold under the
optimizing JIT and not merely under the one that could not have broken them. `io`'s banked flatten
asserts re-validate at 59/59 across the refactor.

**What `Caller` can and cannot honor.** `pc` is the same opaque interned token `Callers` mints, and
`ok` is exact — including the past-the-stack case, which returns Go's zero values with `ok == false`
rather than inventing a frame. Measured against a Go control on the two things a caller actually
does with a `pc`: `runtime.CallersFrames([]uintptr{pc}).Next()` — the branch `log.Output` takes when
a `pc` is supplied — **resolves** to the same file/line `Caller` returned, while
`runtime.FuncForPC(pc)` returns **nil**, which is Go's own `!funcInfo.valid()` answer rather than an
error or a fabricated `*Func` (the inverse-atomic rule: a `*runtime.Func` has no managed referent, so
none is minted). One fidelity edge in the same probe: `Frame.Function` renders the *emitted* method
name through `goFrameName`, so the program's own top frame reads `main.Main`, not Go's `main.main` —
the same "a candidate's EMITTED name is not always its GO name" class recorded elsewhere.
`file`/`line` name the **converted `.cs`** position, consistent with
`Frame.File`/`Line` above and with the honest-answer rule: the running program's source *is* C#, and
no Go-position map is emitted today. That fully serves a caller asking "where am I" — `log`'s
`Lshortfile`/`Llongfile` prefix produces `log_test.cs:67: hello 23 world` where Go produces
`log_test.go:63: …`, and `testing/slogtest`'s `withSource` labels its cases with a real file:line
instead of panicking in a package initializer. It does **not** serve a test that asserts the *Go*
file's own geometry: `log`'s `TestAll` pins `` `.*/[A-Za-z0-9_\-]+\.go:(63|65):` `` — the `.go`
extension and the exact line numbers of the `Printf`/`Println` calls inside `log_test.go`. That is
**not** a disclosure candidate: unlike `alloc-profile` or `codegen-liveness` it is not unsatisfiable
at any layer go2cs owns — a Go-source position map (emitted `#line` directives, or a side-car map
consulted by `internCallerFrame`) would satisfy it exactly. It is a deferred capability, and `log`
stays unbanked until one lands rather than being disclosed around.

**The path is SPELLED Go's way even while it names the `.cs`.** Which source a frame's path points
at and how that path is written are separate questions, and only the first is deferred. Go records
source paths with **forward slashes on every platform**: measured on Windows,
`runtime.Caller`/`CallersFrames` there answer `C:/Program Files/Go/src/runtime/proc.go`, never the
host's native separator. The CLR hands back whatever the PDB holds, which on Windows is
backslash-separated, so until 2026-08-19 every converted program answered `C:\…\log_test.cs` where
Go would have answered `C:/…/log_test.go` — a divergence in the *spelling* on top of the deferred
one in the *target*. `goSourcePath` (`runtime/managed_impl.cs`) now applies Go's rule at the two
places a frame's file reaches a program — `internCallerFrame`, which every `Caller`/`Callers`/
`CallersFrames` answer is interned through, and `appendGoFrames`, which renders `runtime.Stack`'s
traceback. This is a fidelity fix, not a cosmetic one: the string is observable, Go's own suites
match patterns against it (`flag`'s `TestDefineAfterSet` asserts `` `.*/flag_test.go:.*` ``), and
converted `path/filepath` accepts either separator on Windows exactly as Go's does, so no consumer
pays for the normalization. It also makes `log`'s `Lshortfile` trim work, since that trim looks for
the last `/`. Guarded by `RuntimeCallerFrames` (five assertions across `Caller`, a
`Callers`/`CallersFrames` walk, and a rendered traceback; trivially true on a forward-slash host, so
the guard bites on Windows, where the two spellings differ).

Worth carrying into whichever lane takes the position-map arc: **`#line` by itself does not close it.**
Measured — a `#line 852 "C:/Program Files/Go/src/flag/flag_test.go"` region does make
`StackFrame.GetFileName()`/`GetFileLineNumber()` answer with that file and line, so the PDB route
needs no golib change; but Roslyn **resolves and normalizes the directive's path**, and no spelling
of it survives with forward slashes on Windows (absolute, unix-rooted and relative forms all came
back backslash-separated). `#line` therefore supplies the `.go` extension and the Go line, and the
separator normalization above supplies the rest — the two halves compose, and `#line` does not close
`.*/flag_test.go:.*` on its own. One qualifier measured in the same probe, so the claim is about the
directive and not about Roslyn generally: the **`PathMap` compiler option** *does* emit a
forward-slash path (`-p:PathMap=C:\Program Files\Go\src\=/goroot/` yielded
`/goroot/flag/flag_test.go`), so a `#line` emission could in principle carry the separator itself.
It is the worse instrument for this even so — it is a whole-compilation option that rewrites every
source path in the PDB, the emitted `.cs` positions included; it needs the GOROOT prefix as a
per-package build property; and it does nothing for `runtime.Stack` or for any frame reached without
a directive. `goSourcePath` covers all of those with no build-time configuration, which is why the
separator half is settled at the runtime rather than at the compiler.

Two further costs the arc should be priced with, both measured rather than argued: a CS diagnostic
inside a `#line` region reports its position in the **Go** file
(`C:\…\flag_test.go(854,17): error CS0029`), which relocates every compile diagnostic in the corpus
away from the emitted C# the census workflow reads; and a per-statement emission adds roughly
**28–47%** more lines to a converted file (measured on `flag/flag.cs`, `strings/strings.cs` and
`edwards25519/edwards25519.cs`), interleaved between every statement, against the project's
reads-like-Go goal. The side-car alternative pays neither of those but adds a file and a csproj item
per package.

## `codegen-liveness` — a frame holds what Go has already dropped

A second disclosed-divergence class alongside `alloc-profile`, first pinned by `sync` (packages
`TestOnceXGC` ×3 subtests and `TestPoolGC`). Go's GC consults **per-safepoint liveness maps**: a
local dies at its last use, even in the middle of a running function. The CLR's GC info is
conservative in two ways that a GC-lifetime test can see from inside its own frame:

* **Tier-0 / MinOpts codegen reports every frame local live for the whole method.** Every test method
  runs *once*, so it is jitted at tier 0 — in Release as well as Debug — and the pipeline builds the
  test host unoptimized on top of that. Measured: a bare `byte[]` local is still alive after a full
  blocking collect, and becomes collectible under `DOTNET_TieredCompilation=0`.
* **A by-value struct argument larger than a machine word is passed by hidden reference.** A
  `slice<T>` is four words, so the x64 ABI makes the caller materialize a stack temp and pass its
  address — address-exposed, therefore untracked, therefore reported live for the whole frame. This
  one holds in **fully optimized** code, which is what makes it provable rather than a build-flag
  artifact: a probe frame that registers a finalizer on `buf`'s storage releases it (finalizer fires)
  when nothing else touches `buf`, and pins it for the frame's lifetime when the frame merely passes
  `buf` by value to one function.

`TestOnceXGC` is unsatisfiable on the second point *at every layer go2cs owns*: its own body is
`f := fn(buf)`, and both `gcwaitfin()` checks happen inside that frame. `sync.OnceFunc` genuinely
does drop the wrapped function — measured directly, the backing array is released after the first
call — so the disclosure covers the CLR's frame conservatism, not a retention bug. The two real bugs
the investigation *did* find (SetFinalizer keying on the pointer box; `Ꮡ`'s `in` parameter pinning the
array) were fixed at their layers first; only what remained was disclosed.

The class's bar and its standing measurements live with the roster's disclosure classes in [Validated
Test Packages](../ValidatedTestPackages.md): the 2026-08-30 tier-0 A/B found the first point above
disappears under a Release publish with `DOTNET_TieredCompilation=0`, so a row that needs that
configuration says so on its own line, `execution: release-tc0` (`internal/weak` is the first). `sync`'s
three `TestOnceXGC` subtest pins, and how they count toward its Disclosed column, are recorded in
[DATA-alloc-pins-rand-sync](../phase4/DATA-alloc-pins-rand-sync.md);
[CENSUS-type-name-erasure](../phase4/CENSUS-type-name-erasure.md) §8 derives how `unique`'s GC rows move
between matched and `codegen-liveness` once its subtest names match Go's.

## `host-limit` — the third disclosed-divergence class: what the test HOST cannot BE

`alloc-profile` and `codegen-liveness` both name something the managed runtime cannot **measure**.
`host-limit` names something the converted test binary cannot **be** — a property of the deployment
shape rather than of an assertion. Ruled 2026-08-15 and first pinned by `os/exec`, whose 25 leaf rows
under `TestCommand` and `TestLookPathWindows` carry it (their 2 parents ride the disclosed-parent
aggregation rule, carrying no failure text of their own).

**The one capability named today: a relocatable single-file test executable.** Go's test binary is
statically linked, so a test may copy it and run the copy — `os/exec`'s `installExe` does exactly
that, and both of its `LookPath`/`Command` fixture tables are built on it. A converted test host is
an **apphost**: a stub bound at build time to a managed assembly of the same base name that must sit
beside it. Copy the one file and hostfxr answers `LibHostAppRootFindFailure` — `exit status
0x8000809a`, *"The application to execute does not exist"* — which is byte for byte what the tests
report, and is the pinned signature. Satisfying it means publishing every converted test host
self-contained single-file: ~70 MB and a publish rather than a build, per package.

**The bar, and why the class stays narrow.** A `host-limit` entry must pin a **structural** property
of the current deployment shape — provable from how the artifact is built, not from how far an
implementation has got — and never an unimplemented-but-fixable defect. The distinction is the same
one that keeps `log` unbanked rather than disclosed: `log`'s `TestAll` wants a `.go:63` position, and
a Go-source position map would satisfy it exactly, so it is a deferred capability and no disclosure.
Nothing about an apphost's binding is deferred work of that kind; it is what the artifact *is*.

**Why a disclosure rather than a capability gate**, given that `unsupportedRuntimeCapabilities` names
this very capability for `os_test.TestRemoveAllWithExecutedProcess`. The gate arm was built and
measured on `os/exec` before the ruling, and it fails on three counts recorded in
[BOARD-next-validation-candidates](../phase4/BOARD-next-validation-candidates.md): a gate keys on the
DECLARATION, so it withdrew 40 verdict rows where only 27 were failing, destroying 13 live agreeing
passes; it is self-defeating against a `TestMain` that asserts the whole suite ran, because greening
the suite is what arms that census (`helper command unused: "printpath"`, host exit 1, package
validates at no count); and it hides the very rows whose future passing is the signal the limit has
been lifted. A disclosure keeps every row running, visible and compared, so the class **retires
itself**: build the single-file host and these 25 rows start passing, the disclosed count stops
matching, and the sweep fails until the entries are removed. `os`'s gate entry predates the ruling
and `os` is not yet on the roster; its disposition is decided when it banks.

## `deferred` and `structural` — the two labels every allocation-count disclosure resolves into

Ruled 2026-09-05 (coordinator, owner-ratified the same day) and landed in the schema and the roster
guard together. They exist because `alloc-profile` had come to carry two different claims under one
word. An `AllocsPerRun`-style assertion measures **Go's escape analysis** — a compiler optimization
the CLR JIT does not perform — rather than a behavioural property: the value written, read or
returned is identical on both sides. So such an assertion is **never disclosed as CLR-structural
merely because it fails today**, which is ruling #1 of 2026-08-02 restated; but it may be **deferred
against a named plan to reach it**, which is the amendment.

| label | the claim | what the entry must carry |
|---|---|---|
| `deferred` | the CLR **can** meet the assertion in principle | `want`, `reading`, `plan` — all three, or the entry is refused |
| `structural` | **no** managed implementation can meet it, proved in the reason with the object Go keeps off the heap NAMED | no `plan` — naming one contradicts the claim |

**`deferred` is a commitment, not a quieter disclosure.** The owner's strengthening makes the plan a
hard requirement: an entry with no executable plan is refused by the guard, and a plan whose design
record is retired without a replacement fails the row at its next sweep exactly as a regression would.
One plan may serve **many** entries by **mechanism family** — a box minted per address-take, an
element take, an out-parameter, an intermediate buffer in a string conversion — so the requirement is
met by family rather than by a bespoke design per entry.

**The three fields, and why each is required.** `want` is the assertion's own bound, readable without
opening the test. `reading` is the measured current value **with the configuration named**, because a
reading taken at another configuration is not comparable (Release with tiering off is the measurement
of record) and one with no tree named cannot be re-checked. **Nothing compares it with the run yet**
(present tense, dated 2026-09-23, ledger 03:37 X(5)): the loader and check-roster-format's 2c arm only
check that the field is present, so until the sweep-side comparator of
[`DESIGN-allocation-counting.md`](../phase4/DESIGN-allocation-counting.md) §9 (v) lands, COORD compares
each deferred entry's unit note against its `reading` by hand at every sweep read, and a reading moving
**away** from the want is a finding. Every `reading` therefore LEADS with its per-run figure.
`plan` is the design record and increment that closes it.

**Which instrument's number goes in `reading`.** The converted host's own `AllocsPerRun` value, not a
lane's probe: that is what the assertion measures and what a regression must be measured in. A probe
over golib's own allocation sites counts a **different population** — a defer's delegate, a params
array and an interface box sit outside it — so a design record's ladder and the roster's printed
reading are two numbers, and an entry names both rather than letting either stand for the other.


**A structural FLOOR under a deferrable excess (ruled 2026-09-05, from the reflect census).** An
entry can have both at once, and neither label alone is honest about it: `reflect`'s
`TestDeepEqualAllocs` rows want 0 at a signature where boxing a value type into `object` allocates by
construction — a floor of two per run — while the readings run to 53 objects, an excess of ordinary
reducible bridge work. `structural` would bury 51 reducible objects; `deferred` would name a want no
plan can reach. So a **deferred** entry may carry a `floor`: an object count greater than its want,
with a `proof` sketch beside it. Its retirement condition becomes *the host's reading equals the
floor*, and at that point the entry **re-labels `structural`** with the proof already attached and its
plan discharged. The plan requirement is unchanged — the excess is what the plan retires — and a floor
is a CLAIM the census can falsify: a segment reading zero where a floor was predicted retires the
floor, not the entry. Refused: a floor on a `structural` entry (that label claims nothing is
reducible), a floor with no proof, a floor that does not exceed its want (nothing is deferred, so the
entry is simply structural), and a floor beside a want that does not LEAD with its number — refusing
an uncheckable pairing beats guessing which number in a sentence was meant. `floor: 0` means absent,
which is sound because a legal floor is always at least 1.

**The R3 shape: Go's whole budget lies outside the counted population (ruled 2026-09-23, ledger 03:37
X(4) and O2).** Some asserts budget a NONZERO number of allocations that Go spends entirely on objects
golib's counter never charges -- `log/slog`'s `2 pairs` wants exactly 2, and in Go those are the two
`any` conversions of its non-constant arguments, which the managed side performs as uncounted CLR boxes
(`9 kvs` is the EXCEPTION: its 10 includes one `Record.back` slice that both sides count, so once its
surplus is gone it reads COUNT 1 against an exact 10 and never reaches the BYTES arm; it carries NO
pre-declared end label, and its relabel is ruled when it arrives -- COORD's ruling, ledger 3942e083ad).
Such an entry is
`deferred` on its COUNTED SURPLUS (every counted object is excess, because Go's budget sits outside the
count) and carries a **pre-declared end label of `alloc-count-semantics`**: when the plan has removed
the surplus, the count reads zero while bytes are nonzero, the host takes the BYTES arm, and the entry
relabels to the incomparable-unit class rather than retiring. That end label is declared ONLY for this
shape; no want-0 COUNT entry pre-declares one (a flip to BYTES there is a relabel trigger ruled when it
happens), and whether a want-0 BYTES entry with a nameable uncounted residue belongs in
`alloc-count-semantics` is an open post-hop question.

**And a THIRD label stays, for a different reason.** `alloc-count-semantics` (8 entries at this ruling; 5 at go1.24.13) names an
assertion whose UNIT cannot be measured on the host — `reflect`'s `TestChanAlloc` wants 1 where our
counter is silent and the figure is bytes. That is not a bigger number, it is a different unit, so it
is neither deferrable nor structural. The three live labels are therefore `deferred` (measurable, can
be met, plan), `structural` (measurable, cannot be met, proof) and `alloc-count-semantics` (the unit
cannot be measured, stated). Only the bare `alloc-profile` label retires.

**Enforcement, in two places on purpose.** `loadTestDisclosures` refuses a malformed entry at compare
time, so a broken entry fails the sweep of the row that carries it; `check-roster-format.ps1` reads
**every** committed manifest in one pass, so a mislabelled entry is caught the day it lands rather
than at that row's next rebank. Both refusals name the missing field. The bare `alloc-profile` label
is **legacy**: it still loads, because rows re-classify at their own next rebank and never wholesale,
and an over-eager guard would take every unswept row down.

## The process ROOTS a converted program never gets from a Go bootstrap: `runtime.envs`, `os.runtime_rand`

Two of Go's cheapest facts about a running process arrive through machinery conversion cannot carry:
the environment is copied into `runtime.envs` by `goenvs()` during `schedinit`, and the temp-file name
source is `runtime.rand`, a per-M chacha8 PRNG the `os` package reaches by `//go:linkname`. Neither
producer survives — `schedinit` is Go's scheduler bootstrap and go2cs never runs it (every converted
runtime `init` carries the emitted comment *"not run; .NET is the runtime"*), and `runtime.rand` is
one more bodyless assembly declaration the [`PartialStubGenerator`](source-generators.md#source-generators) fills with a
throw. Both are supplied by hand-owned `*_impl.cs` companions with no `.go` counterpart, so a
reconvert never touches them.

**`runtime.envs` — `core/runtime/goenvs_impl.cs`.** `gogetenv`'s first act is to reject a nil
`environ()` with `throw("getenv before env init")`, so an unpopulated `envs` did not degrade the
lookup, it *killed the caller* — and then re-faulted inside its own traceback on the unimplemented
`getcallerpc`, presenting as a crash rather than as a missing value. The reach is not niche:
`runtime.GOROOT()` **is** `gogetenv("GOROOT")`, which is what `internal/testenv.GOROOT` calls, which
is what any Go test needing the toolchain calls (`path/filepath`'s `TestBug3486` is the first
operational hit). A `[ModuleInitializer]` is the faithful stand-in for `schedinit`'s slot — it runs
when the runtime assembly is first touched, before any converted Go code in it, exactly once — and
fills `envs` from `Environment.GetEnvironmentVariables()` in Go's own `"key=value"` wire form, which
is what `gogetenv` scans (`'='` at exactly `len(key)`) and what `syscall.Environ` hands back. The
SNAPSHOT semantics are Go's, not a simplification: `GOROOT`'s doc says *"the GOROOT environment
variable, if set at process start"*, so a later `os.Setenv` neither does nor should appear here.
Note this is the only *read* path that was broken — on Windows `syscall.Environ`/`Getenv` go straight
to `GetEnvironmentStringsW` and never consulted `envs`.

⚠ **`defaultGOROOT` is a LINK-TIME constant and stays empty — a separate, open root.** `runtime.GOROOT()`
falls back to `defaultGOROOT` when the environment does not set `GOROOT`, and Go's is written by
`cmd/link` at build time (measured: with `GOROOT` unset, a real Go binary still answers
`runtime.GOROOT() == "C:\\Program Files\\Go"`). A converted assembly has no linker to write it, so it
answers `""` — exactly Go's own `-trimpath` case, which `testenv.findGOROOT` handles by walking up
from the working directory for `GOROOT/src/go.mod` and, failing that, **skipping** the test. So
`TestBug3486` now reaches its own decision instead of infrastructure-erroring, but skips where Go
passes. Fabricating a value at runtime (probing `PATH` for the Go installation) would invent behavior
Go does not have; the honest remedies are all at build time — e.g. having the converter carry its
`-goroot` into assembly metadata — and that is a ruling, not a lane fix. Left empty and named.

**`os.runtime_rand` — `core/os/tempfile_impl.cs`.** The throwing stub took out every `os.CreateTemp`
and `os.MkdirTemp` before either could pick a name, which is what made `io`'s `OffsetWriter` tests
infrastructure-error and what stopped `path/filepath`'s symlink tests from reaching `testenv`'s own
decision to skip them. What Go asks of `runtime.rand` *here* is stated by `tempfile.go` itself —
"a good chance the file doesn't exist yet - keeps the number of tries in TempFile to a minimum" — so
only two properties matter, and statistical quality is not one: the sequence must differ **across
processes** (a fixed seed reintroduces exactly the predictability Go moved away from) and it must be
callable from any goroutine (`CreateTemp` takes no lock). `Random.Shared` answers both — OS-seeded at
first use, every member documented thread-safe — and `NextBytes` fills the full 64 bits rather than
`NextInt64`'s 63, so the value is a faithful `uint64` even though `nextRandom` keeps only the low 32.
It is deliberately **not** a cryptographic source: security against a local attacker comes from
`O_EXCL`/`Mkdir` failing, exactly as in Go.

**Reach, measured** — an A/B over the two `*_impl.cs` files against one binary, with `path/filepath`
bucketed **per test** because its unrelated `FindFirstFile` root kills the host mid-suite and makes
every later verdict read `C#=""`:

| Package | Before | After | What moved |
|:--|:--|:--|:--|
| `io` | 47 / 54 | **51 / 54** | `TestOffsetWriter_Seek`, `TestOffsetWriter_WriteAt` and `TestWriteAt_PositionPriorToBase` (all three `infrastructure-error`: *"runtime_rand: … is not implemented"*), plus `TestOffsetWriter_Write` and its subtests (`fail`) |
| `path/filepath` | 34 / 55 | **47 / 55** | 13 tests, every one of them the same shape: `skip` ↔ `skip` |

`path/filepath`'s entire gain is `testenv.MustHaveSymlink`'s bucket turning into **skips that match
Go's skips** — `runtime_rand` sat under `MustHaveSymlink`'s `os.MkdirTemp` probe, so C# could not
reach the point where Go decides it lacks the symlink privilege. What remains there is two roots,
neither of them this one: **seven** tests reach the `FindFirstFile` struct-marshalling defect (one
hard `AccessViolation`, six `IndexOutOfRangeException` in `PinnedBuffer` — the same clobbered
`WIN32_FIND_DATAW`), and **one** is `TestBug3486` on `defaultGOROOT` above.

The `envs` half has its own **positive control**: export `GOROOT` into the host's environment and
`TestBug3486` *passes*, nothing else changed. That is what isolates the residual to the empty
link-time constant rather than to the snapshot — and it is the same probe to re-run if a future
build-time `GOROOT` remedy is tried.

## `runtime.Pinner` — a pin BIT keyed by the referent allocation, and Go's two-level cgo walk over managed values

**What the seam is.** Go's `Pinner` makes one promise with two observables. The promise — an object
is "not moved or freed until `Unpin`" — exists so an address can be handed to non-GC-aware code, and
the ADDRESS half is already unconditional in golib: an address is only ever minted by the `ж<T>`
`uintptr`/`void*` conversions, which pin the storage for the box's whole life, and a reachable box is
never freed. So the first hand-own made `Pin`/`Unpin` no-ops — right about the half no test measures
and wrong about the two halves every test does: the **pin bit** (`isPinned`, read by the cgo argument
check) and the **lifetime hold** (a pinned object stays alive until `Unpin`). Meanwhile the converted
`isPinned` nil-dereferenced in `spanOf` (`mheap_.arenas` is never allocated) and the converted
`cgoCheckPointer` returned silently at `debug.cgocheck == 0` — Go's default of 1 is set by
`parsedebugvars` on the `schedinit` path the managed host never runs, the same silently-unreached
init as `internal/cpu`'s feature flags.

**The emitted form.** Five bodies are displaced through `manualConversionFuncs["runtime"]` —
`Pinner.Pin`, `Pinner.Unpin`, `isPinned`, `pinnerGetPinCounter`, `cgoCheckPointer` — into one flat
marked companion, `core/runtime/pinner_impl.cs`; `setPinned`, `unpin`, `pinnerGetPtr`,
`cgoCheckArg` and `cgoCheckUnknownPointer` stay converted and dead behind them (the `mfinal.cs`
"vestigial machinery" precedent). The pin is a COUNT in a `ConditionalWeakTable` keyed by the
pointer's `INilPointer.ReferentObject` — a standard box is its own referent, an element reference's
is its canonical backing, a field reference's is its source allocation — which is Go's per-object
span index one level down: pinning `&sl[0]` pins the whole backing, so `isPinned(&sl[1])` and the
slice header's array word both read pinned, and pinning an interface CELL does not pin the pointer it
holds. The table is weak; the hold is the pinner's own list of referents (Go's `refs`, one level
down), a partial-part field on the converted `pinner` struct so it rides with the box through
resurrection into the leak finalizer, which goes through the hand-owned `SetFinalizer` bridge and
calls the converted `pinnerLeakPanic` variable so a test's swap is observed. No CLR pin is taken, no
byte lands on `ж<T>` (a GolibTests arm asserts the box's field set), and nothing is written into
`ManagedPointerTokens` — that record is weak, per-projection and about the address-take, and
`TestPinnerSimple` takes `unsafe.Pointer(p)` (which registers) BEFORE asserting `!IsPinned`; the
record is only READ, through `Resolve`, for a bare number.

**The cgo check.** `cgoCheckPointer(ptr, arg)` is Go's rule verbatim: every Go pointer word at
level 1 must be pinned, every Go pointer word at level 2 must be pinned, level 3 is not inspected
(`cgoCheckArg` walks the argument's pointee by its `GoType` structure; `cgoCheckUnknownPointer`
reads a pointee's words without descending). A `NativeBox`, a native-backed slice and a number
nothing resolves are not Go pointers; a channel, a map and a closure are Go pointers to unpinnable
heap objects and always fail; a string literal's bytes are a heap `byte[]` here where Go's are
RODATA — the one divergence the runtime suite reaches, disclosed as `runtime-capability` on
`TestPinnerConstStringData`'s exact signature. `GODEBUG=cgocheck=0` disables the check, read once.

**One converter shape tolerated.** `internal/fmtsort`'s test init passes
`reflect.ValueOf(ch).UnsafePointer()` to `Pin`, and the converter wraps that `unsafe.Pointer`-typed
call result in `(uintptr)` on its way into the `any` parameter — so `Pin` accepts a `uintptr` as the
projected form of a pointer, resolving it through the record and no-op'ing on a miss. Routed to the
Q49 bridge class; the accommodation retires with the converter fix. Design, per-row classification
and the prediction on record: `docs/phase4/DESIGN-runtime-pinner.md`; guards: GolibTests
`RuntimePinnerTests` (every "passes" arm followed by the same check with the pin removed, which must
go red), and runtime's own `pinner_test.go` through the `-tests` pipeline.

## Moved sections

- <a id="the-chan-direction-is-carried-by-the-value--descriptor-cargo-exactly-like-an-arrays-length-2026-08-20"></a>Moved to [The CHAN direction is carried by the VALUE — descriptor cargo, exactly like an array's length (2026-08-20)](reflection/README.md#the-chan-direction-is-carried-by-the-value--descriptor-cargo-exactly-like-an-arrays-length-2026-08-20).
- <a id="a-struct-fields-type-only-array-dims--goarraydims--gomapkeydims-2026-08-20"></a>Moved to [A struct FIELD's TYPE-ONLY array dims — `[GoArrayDims]` / `[GoMapKeyDims]` (2026-08-20)](reflection/README.md#a-struct-fields-type-only-array-dims--goarraydims--gomapkeydims-2026-08-20).
- <a id="reflectvaluebytessetbytes-are-defined-over-the-element-kind-and-they-alias"></a>Moved to [`reflect.Value.Bytes`/`SetBytes` are defined over the element KIND, and they ALIAS](reflection/values.md#reflectvaluebytessetbytes-are-defined-over-the-element-kind-and-they-alias).
- <a id="reflectcopy-is-bridged-element-wise--the-auto-form-is-a-flat-two-header-memory-move"></a>Moved to [`reflect.Copy` is bridged element-wise — the auto form is a flat two-header memory move](reflection/values.md#reflectcopy-is-bridged-element-wise--the-auto-form-is-a-flat-two-header-memory-move).
- <a id="a-map-entry-is-a-slot--its-value-is-typed-by-the-maps-element-type-never-by-what-it-holds"></a>Moved to [A map ENTRY is a SLOT — its Value is typed by the map's ELEMENT type, never by what it holds](reflection/values.md#a-map-entry-is-a-slot--its-value-is-typed-by-the-maps-element-type-never-by-what-it-holds).
- <a id="the-value-side-of-the-same-rule-pointer-kind-is-not-pointer-box"></a>Moved to [The VALUE side of the same rule: pointer KIND is not pointer BOX](reflection/values.md#the-value-side-of-the-same-rule-pointer-kind-is-not-pointer-box).
- <a id="reflectvaluerecv--send-over-golibs-channel-and-why-they-could-not-land-alone"></a>Moved to [`reflect.Value.Recv` / `Send` over golib's channel, and why they could not land alone](reflection/values.md#reflectvaluerecv--send-over-golibs-channel-and-why-they-could-not-land-alone).
- <a id="the-reflectlite-mini-bridge-mirrors-reflect-one-layer-down--and-the-closure-that-validated-it-landed-five-rules-in-shared-machinery"></a>Moved to [The reflectlite MINI-BRIDGE mirrors reflect one layer down — and the closure that validated it landed five rules in SHARED machinery](reflection/values.md#the-reflectlite-mini-bridge-mirrors-reflect-one-layer-down--and-the-closure-that-validated-it-landed-five-rules-in-shared-machinery).
- <a id="reflecttypename--a-defined-type-has-a-name-even-when-its-underlying-type-is-a-composite"></a>Moved to [`reflect.Type.Name()` — a DEFINED type HAS a name even when its underlying type is a composite](reflection/types.md#reflecttypename--a-defined-type-has-a-name-even-when-its-underlying-type-is-a-composite).
- <a id="an-unnamed-func-type-renders-structurally-exactly-as-an-unnamed-struct-does"></a>Moved to [An UNNAMED func type renders STRUCTURALLY, exactly as an unnamed struct does](reflection/types.md#an-unnamed-func-type-renders-structurally-exactly-as-an-unnamed-struct-does).
- <a id="structfieldtag-is-a-real-read--the-converter-has-always-emitted-the-tag-nothing-had-ever-read-it"></a>Moved to [`StructField.Tag` is a REAL read — the converter has always emitted the tag, nothing had ever read it](reflection/types.md#structfieldtag-is-a-real-read--the-converter-has-always-emitted-the-tag-nothing-had-ever-read-it).
- <a id="an-embedded-field-is-an-embed-to-reflect-tag-included--and-gos-whole-embedding-contract-reads-it"></a>Moved to [An EMBEDDED field is an embed to `reflect`, tag included — and Go's whole embedding contract reads it](reflection/types.md#an-embedded-field-is-an-embed-to-reflect-tag-included--and-gos-whole-embedding-contract-reads-it).
- <a id="gos-assignability-rule-and-the-identity-walk-underneath-it"></a>Moved to [Go's ASSIGNABILITY rule, and the identity walk underneath it](reflection/types.md#gos-assignability-rule-and-the-identity-walk-underneath-it).
- <a id="abitypes-specializations-are-synthesized-not-downcast--structtype--arraytype"></a>Moved to [`abi.Type`'s SPECIALIZATIONS are synthesized, not downcast — `StructType()` / `ArrayType()`](reflection/types.md#abitypes-specializations-are-synthesized-not-downcast--structtype--arraytype).
- <a id="a-managed-reference-is-a-go-pointer-not-a-go-struct--the-reflection-bridges-descent-rule"></a>Moved to [A managed REFERENCE is a Go pointer, not a Go struct — the reflection bridge's descent rule](reflection/types.md#a-managed-reference-is-a-go-pointer-not-a-go-struct--the-reflection-bridges-descent-rule).
- <a id="reflectarrayof-composes-a-descriptor-it-does-not-reconstruct-a-linker-record"></a>Moved to [`reflect.ArrayOf` composes a descriptor; it does not reconstruct a linker record](reflection/types.md#reflectarrayof-composes-a-descriptor-it-does-not-reconstruct-a-linker-record).
- <a id="reflectstructof-mints-a-clr-value-type-and-then-changes-nothing-else"></a>Moved to [`reflect.StructOf` MINTS a CLR value type, and then changes nothing else](reflection/types.md#reflectstructof-mints-a-clr-value-type-and-then-changes-nothing-else).
- <a id="reflectsliceof-is-the-same-one-liner-as-pointerto"></a>Moved to [`reflect.SliceOf` is the same one-liner as `PointerTo`](reflection/types.md#reflectsliceof-is-the-same-one-liner-as-pointerto).

---

[← The standard-library conversion applies `-tags purego`](purego.md) · [Index](README.md) · [Comments →](comments.md)
