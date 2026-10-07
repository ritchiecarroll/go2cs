# Manually-Converted Declarations

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#manually-converted-declarations)

Some Go declarations cannot be faithfully auto-converted because their semantics depend on hiding a managed pointer inside an integer. The canonical family is runtime's `guintptr`/`puintptr`/`muintptr` (`type guintptr uintptr` holding a `*g` the Go GC must not see): the CLR has the *opposite* constraint — a managed reference stored as a number is invisible to the .NET GC, so the referent can be collected or moved and the number is garbage. The managed conversion stores the `ж<T>` box **directly** and the numeric form never exists (model precedent: `core/sync/atomic`'s hand-rewritten `Pointer<T>`).

Two mechanisms deliver this, chosen by granularity:

* **Whole-file** (pre-existing): a hand-finished file marked `[module: GoManualConversion]` is never overwritten by the converter when it exists in place (`containsManualConversionMarker`), and is restored over auto output by the overlay on fresh (unseeded) reconversions. Right when the whole file is hand-owned (sync/atomic `type.cs`). The marked file's Go source is NOT dropped from conversion (2026-07-17 fix): it is still analyzed and visited with its package — its anonymous-struct lifts, package-var registrations, and other package-wide state must keep feeding the package's sibling files, or those emit corrupted (raw Go `struct{…}` text in selectors, package-var assignments re-declared as shadowing locals) — with emission redirected to a non-compiled `<name>.cs.auto` review sibling. Guarded by the `ManualConversionSiblingState` behavioral test.
* **Type-level** (`go2cs/manualTypeOperations.go`): the `manualConversionTypes`/`manualConversionFuncs` registry (keyed by package path and raw Go names) makes the converter skip emitting the listed **type declarations**, every **method on those types**, listed **adjacent free functions** (`setGNoWB`), and **`GoImplicitConv` assembly attributes** referencing the types — each replaced by a marker comment pointing at the package's `*_impl.cs`. Right when the types live in a large file (runtime2.go) that must otherwise keep receiving converter improvements. Each `manualConversionFuncs` entry also carries a **platform scope** — see *Hand-owns have a platform* below.

The hand implementation (`src/core/<pkg>/<file>_impl.cs`, e.g. `core/runtime/runtime2_impl.cs`) declares the same type/extension surface the auto call sites bind: value-receiver methods as `this T` extensions, pointer-receiver methods as `[GoRecv] this ref T`, and the conversion operators call sites need. For the guintptr family that surface is: `.ptr()` returns the stored box, `.set()` stores it, `.cas()` is a real `Interlocked.CompareExchange` on the reference slot (the Go original's `atomic.Casuintptr` maps to a throwing asm stub — the managed model makes it *work*), `== 0`/`= 0` bind zero-comparison/nil operators, and numeric escapes are deliberate and loud: converting a non-zero integer **panics** (a number can never faithfully become a managed reference), and converting *to* a number (print/`hex` diagnostics) yields a stable object-identity hash — an opaque token, never an address.

One call-site emission cooperates (`convCallExpr.go`): a conversion **to** a manual type from an `unsafe.Pointer` — `guintptr(unsafe.Pointer(newg))` — unwraps the inner conversion and emits the referent-preserving ctor form `new Δguintptr(newg)` instead of the numeric cast chain `(Δguintptr)(uintptr)new @unsafe.Pointer(newg)`, which would lose the referent at the `(uintptr)` hop.

**The runtime lock/note model (`core/runtime/lock_managed_impl.cs`).** Go's `mutex.key` is a tagged atomic slot — 0 unlocked, `locked` (1) held, or an `*m` `address|locked` heading a waiter chain through `m.nextwaitm`, parked on OS semaphores. The managed model hand-owns `mutexContended`/`lock2`/`unlock2`/`notewakeup`/`notesleep`/`notetsleep_internal` (via the same registry; thin wrappers and consts stay auto) and keeps the **same key protocol restricted to `{0, keyLocked}`**: the mutex is an `Interlocked` spinlock on the real `key` storage with `SpinWait` escalation standing in for the spin→yield→park ladder; the note is a signaled/clear latch (double-wakeup throw preserved; timeout at millisecond granularity). Deliberately not modeled, documented in place: the waiter queue (fairness), lock profiling, and the `m.locks`/preempt bookkeeping — `getg()` is a Go compiler intrinsic with no managed realization yet (a `[ThreadStatic]` g/m model is the future root that unlocks runtime-operational semantics; the bookkeeping returns to these bodies when it lands).

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


**`crypto/subtle`'s word-at-a-time XOR (`core/crypto/subtle/xor_generic.cs`, whole-file).** `xorBytes` XORs a machine WORD at a time by reinterpreting its three byte slices as `[]uintptr` (`unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), len(x)/wordSize)`). A `uintptr[]` view over a `byte[]` does not exist in the managed model — golib's `slice<T>` is a window on a real `T[]` — so the converted `words()` could only SNAPSHOT the bytes into a detached `slice<uintptr>`, and the word loop XORed the snapshot and dropped it: for every length that is a multiple of 8, `XORBytes` wrote **nothing**. The whole file is hand-owned (marked `[module: GoManualConversion]`) and does the same reinterpret the managed way, `MemoryMarshal.Cast<byte, ulong>` over the slices' own spans — a genuine aliasing view, so the word writes land in place — keeping Go's word-at-a-time behavior and the performance contract crypto/cipher's CTR and GCM modes depend on. Only Go's `supportsUnaligned`/`aligned` gate is dropped (it exists for architectures whose unaligned word loads fault). Full detail: *`unsafe.Slice` over MANAGED element storage ALIASES it*. Guarded by crypto/subtle's own suite (7/7, no disclosures, over the full 1..1024 x 8 x 8 x 8 alignment matrix).

**`sync/atomic.Value` (`core/sync/atomic/value.cs`, whole-file).** Go's `atomic.Value` stores and loads an `any` atomically by reinterpreting the interface's internal two-word `(type, data)` layout: `(*efaceWords)(unsafe.Pointer(&v))`, then `atomic.LoadPointer`/`StorePointer`/`CompareAndSwapPointer` on the `typ` and `data` slots, with a `firstStoreInProgress` sentinel guarding the first store. That layout is a Go runtime detail with **no managed equivalent** — an `any` here is a single `System.Object` reference (one word), and reinterpreting a managed reference as a raw address to poke type/data words simply NREs (the same managed-referent-through-`unsafe.Pointer` wall as the guintptr family). The first *operational* hit was `internal/testlog`'s package-level `var logger atomic.Value`, loaded during `os.Getenv` — so `atomic.Value.Load()` NRE'd on the zero value before any store. The whole file is hand-rewritten (marked `[module: GoManualConversion]`) to store the `any` **directly** in the `Value.v` field and use `Volatile.Read`/`Interlocked.CompareExchange` for the acquire/release ordering and CAS the literal conversion cannot provide; the nil-store and inconsistent-type panics, and `CompareAndSwap`'s by-value comparison (`AreEqual`, matching Go's `i != old`), preserve the spec. Guarded by the `AtomicValue` behavioral test (Load-nil / Store / Swap / CompareAndSwap over typed string values, output-compared vs Go).

**The `internal/reflectlite` mini-bridge (`value_impl.cs` + `swapper_impl.cs`, Phase-4 reflection
bridge).** `sort.Slice`/`SliceStable`/`SliceIsSorted` route through reflectlite —
`ValueOf(x).Len()` and `Swapper(x)` — and the auto forms reinterpret the interface's eface
`{type,data}` words, so the first touch dereferenced a nil `ж<abi.Type>` (sort's `TestSlice`, the
first operational hit: `unpackEface` → `abi.Kind` → NRE). The fix mirrors the full `reflect`
bridge (see `reflect/value_impl.cs` and `docs/phase4/DESIGN-reflection-bridge.md`) for exactly the
mini-surface sort exercises: `ValueOf`/`unpackEface` build the `Value` over a companion
`partial struct Value { object boxed }` field — `typ_` takes the Phase-1 synthetic `abi.Type` and
the flag takes the Kind bits, so `Kind()`/`IsValid()` keep working from the auto `value.cs`
unchanged — `Value.Len` reads the boxed value through the golib container interfaces
(`@string`/`IArray`/`IMap`), and `Swapper` swaps through golib's non-generic `ISlice` indexer
(which applies the slice window offset, so swaps land on the shared backing store exactly like
Go's). The four declarations are skipped by the converter via the `manualConversionFuncs`
registry (`"internal/reflectlite"` in `go2cs/manualTypeOperations.go`); the rest of reflectlite —
including `packEface`/`Interface()` (used by `errors.As`) — stays auto and is NOT yet operational.
Verified by the sort differential: `TestSlice` flips to pass — `sort.Slice` sorts through the
managed Swapper, and its closing `SliceIsSorted` check reads length through the same `ValueOf` path. Go's version counts **mallocs**: it pins `GOMAXPROCS(1)`, runs `f` once as a warmup, then `runs` more times, and returns the `runtime.MemStats.Mallocs` delta divided (as integers) by `runs`. The CLR exposes no malloc counter, so the shim measures allocated **bytes** on the calling thread instead (`GC.GetAllocatedBytesForCurrentThread()` — precise, and inherently thread-scoped, which stands in for the GOMAXPROCS pinning; like Go's, `f` is assumed single-threaded — allocations made by goroutines `f` spawns land on other threads and are not observed). The mapping is deliberately honest rather than count-approximating: **zero maps exactly** (0 bytes ⟺ 0 mallocs — and the stdlib tests that use AllocsPerRun overwhelmingly assert zero, e.g. sort's `TestSearchWrappersDontAlloc` and the strings/bytes no-alloc guards), while a nonzero result is the average allocated bytes per run, floored at 1 so amortized sub-byte-per-run allocation can never masquerade as the exact-zero case. A converted test asserting a specific nonzero *count* therefore diverges as a loud failure in the differential oracle instead of silently passing — the disclosed outcome. (`runs == 0` divides by zero, a runtime-error panic exactly where Go's own integer division panics.) The capability sits in the converter's supported list (`supportedTestCapabilities`, `testConversion.go`), so tests requiring it convert as *included*; guarded by `TestAllocsPerRunCapabilityIsSupported` (converter) and `TestingRuntimeTests.AllocsPerRunMapsZeroExactlyAndReportsBytesWhenAllocating` (shim).

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
| `s[a:b]` on a <code>string &#124; &#91;&#93;byte</code>-constrained value | 48 B each | `IByteSeq<T>`'s range indexer returned the **interface**, so the `@string`/`slice<byte>` struct result was boxed | **fixed** — self-referential `IByteSeq<TSelf, T>` |
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

**The reflect TYPE-RELATION mirrors + Convert (Phase-3 continuation, 2026-07-26).** Go's descriptor
model reaches its type relations by **descriptor specialization**: when `Kind() == Interface` the
`*abi.Type` IS an `interfaceType` allocation, so `implements()` does
`Reinterpret<abi.Type, interfaceType>` and walks `.Methods`; `ptrTo` builds a `ptrType` prototype
through an eface reinterpret; `FieldByName` reinterprets to `structType` and walks `.Fields`. Behind
a **synthesized** descriptor none of that layout exists — the reinterpret produces a struct whose
promoted-embed box is default, and the first read throws from `ж.ValueSlot` ("Cannot get reference
to value…", the encoding/gob type-initializer crash). Reinterpret-specialization is therefore a
class of descriptor reads that can never be honored behind the bridge; each surface severs at its
semantic boundary onto the SAME golib machinery emitted asserts use: `rtype.Implements` /
`rtype.AssignableTo` over `GoReflect.GoImplements` (mirroring the reflectlite increment-1 forms),
`PointerTo` synthesizing the managed `ж<T>` pointer type (canonical via `toType`),
`rtype.FieldByName` over the shared `GoFields` projection (top-level names; the embedded-field
depth search is deferred with a named consumer — a promoted name answers Go's not-found path),
and `Value.Convert` over `GoReflect.TryConvertTo` — THE convertibility relation (the recorded
R-13 remedy), severing the `cvtInt → makeInt → unsafe_New` stub chain (R-14; internal/fmtsort's
package-level `ct()` table). `Value.Cap`/`Value.SetLen` join over the golib container interfaces
(gob's `decodeSlice` probes `Cap() < n` then re-lengths the header — SetLen writes the re-windowed
slice back through the aliased box), and the hand-owned `rtype.Field` stamps the single-hop
`StructField.Index` — an empty Index made the auto `FieldByIndex` return the struct ITSELF, so
gob's `encodeStruct` walked every wireType field as the whole struct. Demonstrated consumers:
encoding/gob's init + Encoder/Decoder engines (a struct round-trips end-to-end), go/token's
`TestSerialization` (FileSet through gob, 31/31), internal/fmtsort (3/3). Registered in
`manualConversionFuncs["reflect"]`; the banked fmtsort/go-token suites are the operational guards.

**The type NAME is a descriptor read too — `reflectlite`'s `rtype.String` (2026-08-02).** The same
class as the specialization reads above, at its quietest. Go's `rtype.String()` is
`t.nameOff(t.Str).Name()`: `Str` is a **name offset** into the linker-built name blob, resolved by
pointer arithmetic from the descriptor's own address. A synthesized descriptor has no blob and no
`Str`, so the mini-bridge's `String()` answered `""` for every type — and answered it **silently**,
because `""` is a legal name for an unnamed Go type, so nothing panicked and no read faulted; the
empty string simply propagated into whatever the caller was building. `context`'s `stringify`
fallback (the arm for a key type with no `String()` method) printed
`context.Background.WithValue(, c1k1)` where Go prints
`context.Background.WithValue(context_test.key1, c1k1)`. That is the failure mode worth recording:
a descriptor read that cannot be honored does not always throw — this one degraded to a
plausible-looking empty field, and only a differential caught it.

`reflect`'s own `rtype.String` has been hand-owned over `GoReflect.GoTypeName` since Phase 1; this
is the identical answer for the mini-bridge (`internal/reflectlite/type_impl.cs`, registered as
`manualConversionFuncs["internal/reflectlite"]["rtype.String"]`), so the full bridge and the mini
bridge cannot disagree about what a type is called. Array dims ride along exactly as on the
`reflect` side — a descriptor that knows its length renders Go's `[N]T` rather than `[]T`. The
managed nesting supplies the package qualifier: `key1` is declared in class
`context_test_package`, stamped `[GoPackage("context_test")]`, so `GoQualifiedName` recovers
`context_test.key1` — including for a `-tests` external test package, whose Go-visible package name
is the stamp's authority rather than the class name.

**Blast radius, measured rather than assumed:** within `reflectlite` only three call sites reach
`String()` — `stringify`/`contextName` (the fix), and the `assignTo` and `elem()` **panic
messages**, which merely become legible. No comparison logic consumes it: `rtype.Name` still
answers `""` for every type because it gates on `HasName()`, which reads the `TFlagNamed` bit that
`synthesizeDescriptor` never sets, and the `haveIdenticalType`/`directlyAssignable` chain that
would consume `Name()` is dead behind the hand-owned `Implements`/`AssignableTo`. `errors` — the
mini-bridge's other consumer — never reaches `String()` at all (`Comparable`/`Kind`/`Implements`/
`AssignableTo`/`Elem`/`Set`/`IsNil` only), and re-validates unchanged at 61/61. Demonstrated
consumer: `context`'s `TestValues`, 36/38 → **37/38** (the remaining failure is `TestAllocs`, the
measured alloc-count disclosure). `rtype.Name` is the recorded next gap of this shape, deliberately
NOT fixed without a consumer that demonstrates it.

**The method COUNT is a descriptor read too — `rtype.NumMethod`, the gate on json's Unmarshaler
discovery (2026-08-02).** The same silent-degradation class as the NAME read above. Go's
`rtype.NumMethod` counts `uncommon()` method tables — trailing descriptor allocations the linker
lays out after the `abi.Type`, which a synthesized descriptor never populates — so it answered
**0 for every concrete type**, and answered it silently, because 0 is the correct count for most
types and nothing downstream faults on it. The consequence hid one hop away: `encoding/json`'s
`indirect()` only ATTEMPTS its `Unmarshaler`/`TextUnmarshaler` interface assert behind
`v.Type().NumMethod() > 0`, so no custom `UnmarshalJSON`/`UnmarshalText` was **ever** dispatched —
every `json.Unmarshal` into `time.Time` fell through to the raw-struct path and died with
`json: cannot unmarshal string into Go value of type time.Time` (time's `TestTimeJSON`; in
`TestUnmarshalInvalidTimes` the miss inverted the failure — `{}` decoded *silently* where
`Time.UnmarshalJSON` rejects it). The marshal side never had the problem: `newTypeEncoder` gates on
`Implements`, hand-owned since the type-relation increment.

Severed at the same semantic boundary as every read of this class: the hand-owned `rtype.NumMethod`
(`reflect/value_impl.cs`) answers over golib `GoReflect.GoMethodCount` →
`TypeExtensions.GoMethodSetCount`, which counts over `GetGoMethodSetCandidates` — the **same
candidate source** the structural probe (`StructurallyImplements`) and the duck-typing shell binder
resolve through, so the NumMethod gate and the interface assert behind it can never disagree about
a method set (a count from any other source could answer 0 for a set the assert would bind, and the
gate would silently re-skip the dispatch this fixes). Candidates are deduplicated by **projected Go
name** — one Go pointer-receiver method reaches the registry in two emitted shapes (the
RecvGenerator's `ж<X>` overload and the original `[GoRecv]` `this ref X` extension) — and
exported-ness is judged Go's way (first rune uppercase) on the projection, after the same leading
collision-marker strip `GoMethodNameMatches` applies. Go's kind split is preserved: an interface
type counts ALL its methods (`GetInterfaceMethodNames`, instance members only — the golib static
`As<T>` helpers stay invisible), the empty interface (`object`) counts 0, a concrete type counts
exported only, with `ж<X>` seeing X's value- AND pointer-receiver methods and a plain X only the
value-receiver ones; an adapter shell answers as the Go dynamic type it stands for, mirroring the
`KindOf`/`ElementType` unwrap (R10). The count is memoized per (element, pointer-ness) and cleared
on assembly load with the candidate cache that feeds it.

One root, four symptom shapes — the guard (`tests/Behavioral/JsonUnmarshalerDispatch`) locks all
four against `go run`: unmarshal into `&t` directly (the Pointer-kind gate), whole-value dispatch
of a non-string JSON value (the error path), a user-declared named type with a pointer-receiver
`UnmarshalJSON` (dispatch is not stdlib-specific), and unmarshal into struct FIELDS (the
`Name() != "" && CanAddr()` → `Addr()` route through the field-alias box). Demonstrated consumer:
time's `TestTimeJSON` and `TestUnmarshalInvalidTimes`. `rtype.Method(i)` stays auto and still reads
the same absent tables — the recorded next gap of this shape: a `NumMethod() > 0` gate now lets a
method-ENUMERATION loop (`for i := range t.NumMethod() { t.Method(i) }`) get further than before,
and the first consumer that walks one demonstrates it.

**…and the count and the WALK are ONE increment — `Type.Method(i)`, `Value.Method(i)`,
`MethodByName` (2026-08-03).** The paragraph above shipped alone and was **reverted**: the
recorded successor gap arrived one session later, on the very next all-package sweep. `math/rand`
and `math/rand/v2`'s `TestRegress` enumerate every generator method — `typ.Method(i).Name`,
`rv.Method(i)`, `mv.Type()`'s `NumIn`/`In`, `mv.Call(args)` — against a golden output table, and
both went from validated to `panic: reflect: Method index out of range`. The lesson is general and
worth stating as a rule: **a truthful count is a PROMISE that the table behind it can be indexed.**
While `NumMethod` answered 0 the enumeration loops were unreachable and their auto `Method(i)`
(which reads the same absent `uncommon()` tables) could not be observed; making the count truthful
is exactly what made them reachable. A descriptor read and the gate in front of it belong in one
increment.

The whole table is now ONE list — `TypeExtensions.GetGoMethodSetEntries` — and `NumMethod` is its
`.Count`, so a size and an order can no longer be derived separately and disagree. It is built over
`GetGoMethodSetCandidates`, the same candidate source `StructurallyImplements` and `AdapterBinder`
resolve through, then: deduplicated by projected Go name (keeping the shape a delegate can bind —
a `[GoRecv] this ref X` receiver cannot be a `Func<>` parameter, and the RecvGenerator's `ж<X>`
overload always sits beside it), exported-only for a concrete type, and **sorted ORDINALLY by Go
method name**, which is Go's own method-table order (verified against `go run`: a promoted embedded
method sorts *in place*, it is not appended).

A **method value is an ordinary bound delegate**, and that is the design's whole economy. Go
carries `v.Method(i)` as the receiver's own Value plus a `flagMethod` bit with the index packed
into the flag, then rebuilds the signature (`typeSlow`) and re-resolves the receiver
(`methodReceiver`) on every use — all descriptor reads. The bridge instead BINDS the receiver into
a managed delegate at `Method(i)` time, so the result is a Kind-Func Value and everything
downstream is reuse rather than new surface: `mv.Type()` is the ordinary canonical Type of a
delegate, `NumIn`/`In`/`NumOut`/`Out` are the existing `TryFuncShape` readers, and `mv.Call(args)`
is the existing `Value.Call` **unchanged** — with the receiver already absent from the signature,
which is precisely Go's method-value contract. `Type.Method(i).Func` is the same delegate UNBOUND
(receiver first), Go's contract for the type side, and `Value.MethodByName` needs no hand-own at
all: it composes the hand-owned `rtype.MethodByName` and `Value.Method`.

Binding is **expression-compiled, and that is not a preference**: `Delegate.CreateDelegate(type,
firstArgument, method)` cannot close over a VALUE-type first argument (measured — `ArgumentException`)
and Go value receivers are by-value structs, so the closed-delegate form fails for every
value-receiver method, `time.Time`'s entire method set included. Each `MethodInfo` is compiled once
into a `Func<object?, Delegate>` factory (a nested lambda — the outer takes the receiver, the inner
IS the bound Go-signature delegate), so a bind costs a closure allocation, not a compile. A
value-receiver method reached through `*X` is handed a COPY of the pointee, Go's rule.

**A `this object` extension method is golib plumbing, never a Go method** — and filtering it is a
correctness fix, not tidiness. `GetGoMethodSetCandidates`' assignability safety net (there for
promotion and base relationships) is satisfied by EVERY type when the receiver is `object`, so
`TypeExtensions.TryCastAsInteger(this object, out ulong)` was entering every type's method table —
and entering it **nondeterministically**, because the candidate scan is redone whenever a late
assembly load clears the caches: the same binary reported `NumMethod` **4 or 6 for the same type**
depending only on which assemblies had loaded by the first read (it reproduced under the behavioral
runner's redirected stdout and not under a console). That also means the shipped-then-reverted
count was wrong in a way nothing could observe. It is filtered in the method-TABLE builder rather
than in the shared candidate source, whose admission rule the duck-typing assert and the shell
binder also read: a Go METHOD SET is a stricter question than "could this extension method dispatch
on this value?".

Guard: `tests/Behavioral/ReflectMethodTableWalk` locks the surface against `go run` — count/walk
agreement and sorted order, a value type's set excluding pointer-receiver methods, a bound
pointer-receiver call MUTATING the receiver across calls, a value-receiver method bound through a
pointer, a no-result method, `MethodByName` round-tripping to the same index (and both absent-name
forms), the unbound `Method.Func` called receiver-first, promoted-embed ordering, and an interface
table whose method value dispatches to the dynamic value. Demonstrated consumers: `math/rand`
**43/43** and `math/rand/v2` **36/36** re-validate at their exact banked counts with `TestRegress`
now genuinely walking (the converted bridge reports `*rand.Rand NumMethod: 16` in Go's order and
`Intn(1000000000) = 526058514`, matching `go run` — where before the pair it reported 0 and the
test passed VACUOUSLY, executing none of its 320 golden comparisons); `time` goes 146 → **148**
pass of 159 as the two increment-6 JSON rows re-land. Recorded next gaps of this shape: `MakeFunc`,
variadic `Call`/`CallSlice`, and `reflectlite`'s `rtype.Name`.

**A ZERO test is a descriptor read too, and this one had degraded to a CONSTANT — `Value.IsZero`,
`Value.Grow`, and the named-string `Len` (2026-08-03).** Go's `IsZero` is three reads over flat
memory: an `Equal` function pointer compared against the shared `zeroVal` buffer, a
`TFlagRegularMemory` all-bits-zero scan, and — when the value is not `flagIndir` — plain
`v.ptr == nil`. A synthesized descriptor populates none of them, and the bridge never populates
`v.ptr` or `flagIndir` at all, so the Array and Struct arms both fell straight to that last test and
answered **true for every array and every struct, whatever it held**. Measured against `go run`
before the fix: `[2]uint8{1,2}`, `NA{1,2}`, `inner{N:1}`, `outer{P:&n}` — every one of them
`IsZero=true` in C# and `false` in Go. This is the `""`-type-name and `NumMethod`-0 family again,
and the worst-behaved member of it so far: `true` is the correct answer for the zero value of the
same type, so nothing faults and nothing looks wrong.

A **fourth** read failed independently and had to land with it. `IsZero`'s String arm is
`v.Len() == 0`, and `Len` answered through the golib container interfaces — a named slice is an
`IArray`, a named map an `IMap` — but a `type NS string` wrapper implements none of them, so it fell
to the `0` default. Every non-empty named string therefore reported itself both length-0 and zero.
`String()` had always unwrapped such a wrapper; `Len` now does the same, gated on Kind String.
That pairing is the increment-6 rule in its second form: **`IsZero`'s String arm is a GATE on `Len`,
so the gate and the read behind it are one increment** — fixing the arms without `Len` would have
left named strings silently zero, and fixing `Len` without the arms would have changed nothing.

The managed `IsZero` is Go's own recursive definition with the memory shortcuts *removed*: a
composite is zero exactly when every element (Array) or field (Struct) is, scalars test against
their zero, and the nilable kinds are `IsNil`. That is precisely the walk the shortcuts stand in for
— Go falls back to it itself when a type is not comparable and not regular-memory — so it needs no
descriptor state beyond `Index`/`Field`/`NumField`, which the bridge already answers. Go's blank-field
skip (`Name != "_"`) is preserved.

`Value.Grow` shares the root and the remedy shape: it reads a `*unsafeheader.Slice` off the same
never-populated `v.ptr`, so it **nil-deref'd for every caller** — `reflect.ValueOf(&s).Elem().Grow(1)`
on a `[]byte` prints `4 8` in Go and panicked here. It is now an ordinary managed reallocation
(golib `GoReflect.GrowSlice`) written back through the aliased box exactly as `SetLen` does, coerced
into a named slice wrapper's slot through the single convertibility relation. Two details are
load-bearing: growth **within** the existing capacity writes nothing at all, because Go reaches
`growslice` only past the capacity and a spurious write would detach any other view still sharing the
backing store; and the capacity landed on is unspecified in Go (its `growslice` rounds to a size
class), so only `len+n` is guaranteed and the guard test asserts `cap >= n`, never an exact figure.

Guard: `tests/Behavioral/ReflectZeroAndGrow` pins all four against `go run` — raw and named strings,
raw and named arrays (zero and non-zero, including an array OF named strings), the nil-vs-empty
distinction for slices and maps, structs made non-zero through a nested named-string field alone,
zeroness reached through an interface, and Grow from a nil slice / within capacity / past it /
with `Grow(0)`. Demonstrated consumer: `encoding/gob`, whose `gobEncodeOpFor` skips a field on
`!state.sendZero && v.IsZero()` — so the encoder was omitting non-zero named-string and array fields
from the wire entirely, visible as `v = "", want "forty-two"` on the value fields while the pointer
fields of the same type passed.

**Rooted and deliberately NOT landed: `MapType().Hasher` and `Key.Equal` cannot be honored at all.**
The remaining `unique`/`net` wall is a map descriptor whose `Hasher`/`Key`/`Elem` are unpopulated, so
`concurrent.NewHashTrieMap`'s delegate construction fails on the first field it touches. Populating
them looks like the same shape as the reads above and **is not**, because the contract differs in
kind: `Hasher(unsafe.Pointer, uintptr) uintptr` must hash *the value at an address*, and the address
the call site produces cannot name a managed value. Measured, three ways: two boxes holding equal
`@string` values necessarily have **different** addresses (so an address-derived hash can never make
`unique.Make("hello")` agree with itself — the package's entire purpose); a box whose pointee
contains a reference has no pinnable slot and its address **moved across a forced GC**; and the
`unsafe.Pointer` handed to the delegate carries no link back to its source box by construction, its
constructor taking a `uintptr`. The key and elem *types* ARE recoverable from the descriptor's
carried `System.Type` — but populating those alone would be actively worse than the present failure:
`Key.Equal` is the comparability SIGNAL (a pointer-identity compare, not a value compare), so a
half-populated descriptor turns a loud construction failure into a map that silently mislays every
key. That is the increment-6 lesson inverted — **a descriptor field whose read cannot be honored must
not be populated to look truthful** — and it is why this row is handed on rooted rather than half
landed. The remedy is one layer down and outside this arc's files: `internal/concurrent.HashTrieMap`
is a managed-referent raw-metal case whose *contract* (a concurrent map over comparable `K`) the CLR
answers natively while its *mechanism* (hash the bytes at an address) it cannot, so it wants a
hand-owned `_impl.cs` on the `sync.Mutex` precedent.

**Pointer order tokens — `Value.Pointer()`/`UnsafePointer()` (golib `PointerOrderToken`).** Go
programs order pointers *arithmetically* (`cmp.Compare(a.Pointer(), b.Pointer())` —
internal/fmtsort's map-key ordering of `*T`/`chan`/`unsafe.Pointer` keys), so the bridge's token
must be more than stable-per-instance: **equal Go pointers must token equally, and same-storage
element pointers must order by element index** (Go's `&a[0] < &a[1] < &a[2]`). `INilPointer` gains
the `PointerOrderToken` surface (a DIM default of per-instance identity): `ж<T>` answers nil → 0,
a native alias → its real address, an array/slice-element reference → the canonical backing
storage's identity in the high bits with the ABSOLUTE element index below (the same
`CanonicalElement` reduction pointer equality uses), a struct-field reference → source identity ×
field-identity token, a heap box → the referent's identity; `unsafe.Pointer` (whose VALUE is a
real pinned address) overrides with the address itself; `channel<T>` answers through its shared
core's identity, so every struct copy/boxing of one channel reports ONE token — what makes
sorting channels by `Pointer()` self-consistent (fmtsort's `makeChans` pre-sorts by the same
key). Tokens are order keys consistent with pointer equality, never an identity substitute
(distinct storages can collide); generated named pointer/channel wrappers keep the DIM default —
a recorded fidelity residual with no consumer. The banked fmtsort suite (TestCompare/TestOrder)
is the operational guard.

**…except a TYPE DESCRIPTOR pointer, which orders by the type's NAME (2026-08-10).** The same
`Value.Pointer()` carries one ordering that is visible in ordinary program output rather than only
in a map of pointers: fmtsort's `reflect.Interface` arm orders interface-kinded map keys by dynamic
type, and it does that by comparing the two descriptors as pointers —
`compare(reflect.ValueOf(a.Elem().Type()), reflect.ValueOf(b.Elem().Type()))` recurses into the
`ΔPointer` arm — so this token *is* the printed order of `fmt.Println(map[I]int{…})`. Go answers with
the linker's type-section address, which is unspecified by its own admission (fmtsort's
`TestInterface`: "the relative ordering of types is unspecified", asserting only that same-type keys
group) and is not a function of anything the managed side can see. The identity-hash fallback above is
*worse* than unspecified for this case — CoreCLR draws an object's identity hash from a per-thread
PRNG, so the token is fixed per build but unrelated to the type, and the printed order flips whenever
an unrelated edit shifts how many hashes are drawn first. `reflectPointerToken` therefore routes a
`ж<rtype>`/`ж<abi.Type>` through `typeDescriptorOrderToken`, which packs the leading `IntPtr.Size`
bytes of the descriptor's Go name (the one `Type.String()` prints) big-endian, so comparing tokens
arithmetically compares the names lexically: **types that print alike token alike, and types that
print differently order by that printed name** — stable across builds, runs and unrelated edits.
Names agreeing over the whole packed prefix tie and fall through to fmtsort's concrete-value arm
(Go's own "no good answer" `-1`, settled deterministically by `SortStableFunc`'s stability); matching
Go's layout order for three or more key types is not on offer and would not be a property Go
promises. Guarded by the banked fmtsort suite (TestInterface's grouping) and the
`InterfaceInheritance` behavioral test's output comparison, which is what caught the PRNG model
landing tails. Full derivation:
[`docs/phase4/DESIGN-reflection-bridge.md`](../phase4/DESIGN-reflection-bridge.md).

**EXPORTEDNESS is a descriptor read too, and the value side had been right about it all along —
`StructField.PkgPath` (2026-08-11).** `reflect.StructField.IsExported()` is nothing but
`f.PkgPath == ""`, and Go fills `PkgPath` with the declaring package's import path for an
unexported field (`type.go`: `if !p.Name.IsExported() { f.PkgPath = t.PkgPath.Name() }`). The
hand-owned `rtype.Field(i)` left it unset — the field's own comment said so, on the reading that no
truthful read backed it — so `IsExported()` answered **true for every field of every converted
struct**. Silent, like every member of this family: `""` is the correct `PkgPath` for the exported
fields that are most fields, so nothing faulted and nothing looked wrong.

The consequence is a **guard that can never fire**. `encoding/asn1` opens both its struct arms —
`parseField` and `makeField` — with

```go
for i := 0; i < structType.NumField(); i++ {
    if !structType.Field(i).IsExported() {
        return StructuralError{"struct contains unexported fields"}
    }
}
```

so `Marshal(unexported{X: 5, y: 1})` returned a **nil** error where Go returns that structural
error, and `Unmarshal` ran straight past the refusal into `parseField(val.Field(i), …)` on the
unexported field, where `SetInt` reached `mustBeAssignable` and panicked
(`TestUnexportedStructField`). Note what that panic proves: the two halves of the read-only model
had degraded **independently**. `Value.Field` already stamped `flagStickyRO` for an unexported
field — from `GoReflect.GoFields`, the same projection the type side walks — so `CanSet()`/
`CanInterface()` were correct and the write was correctly refused; it was only the TYPE-side
descriptor that had no answer, which is why a package that *probes* settability got no warning
while a package that simply *writes* got a clean Go-shaped panic. `PkgPath` now derives from the
same projection's `Exported` bit plus `GoReflect.GoPackagePath` (the package identity the managed
nesting carries, already `rtype.PkgPath`'s source), so a probe of the type and a write through the
value cannot disagree about a field.

Two neighbouring `StructField` members stay unpopulated, for two different reasons worth keeping
apart. **`Offset`** is the r39d rule — a descriptor field whose read cannot be honored must not be
populated to look truthful: a Go byte offset exists to be added to a data pointer, and managed
storage has no such pointer. (`abi.StructType` does populate `Offset`, and correctly: its consumers
— `unique`'s clone sequencer, `internal/reflectlite` — read it as layout metadata, never as an
address to walk.) **`Anonymous`** is the opposite case: it IS knowable, since an embedded field
arrives through golib's promoted-embed box hop, but no measured consumer demands it and the
recorded next gap of that shape is larger — go2cs-gen emits the promoted-embed backing box AFTER
the declared fields, so `struct{X; y; Inner; inner; Ptr}` walks as `X, y, Ptr, Inner, inner` here
where Go walks it in declaration order. Field ORDER and `Anonymous` want one increment together,
with a consumer that demonstrates them.

Guard: `tests/Behavioral/ReflectUnexportedFieldFlags`, byte-identical to `go run` — the indexed
walk's `IsExported`/`PkgPath`/`Tag`, `FieldByName` carrying the same flags (including the blank
field, which Go also reports unexported), a field-for-field assertion that the type side and the
value side AGREE (`v.Field(i).CanSet() == t.Field(i).IsExported()`), and the consumer shape itself:
a decoder that probes before writing must be able to refuse with a returned error rather than a
panic. Demonstrated consumer: `encoding/asn1`'s `TestUnexportedStructField`.

**A func PARAMETER is the one position an array's LENGTH cannot be recovered from — `[GoArrayDims]`
(2026-08-11).** A Go array's length is part of its type, and it is the one part the managed emission
cannot carry: `[32]byte` renders as golib `array<byte>`, and C# has no const generic parameter to
hold the 32. The bridge has always answered that by recovering the dimension from a live source
instead, and the two it had covered every position that mattered — a VALUE measures itself
(`GoReflect.ArrayDimsOfValue`), and a struct FIELD reads it off a cached zero instance of the
declaring struct, because the converter emits the dimension as a field initializer (`= new(32)`)
that the generated parameterless constructor runs.

A func parameter has neither. There is no value at a type-only position, no initializer to read,
and the emitted delegate type is a bare `Func<array<byte>, bool>` that `func([32]byte) bool` and
`func([64]byte) bool` **share**. So `reflect.TypeOf(f).In(0)` answered a dims-less array descriptor:
`Len()` 0 and `String()` `"[]uint8"` — which does not even read as an array — and `reflect.New`/
`reflect.Zero` of it built a **zero-length** array. `testing/quick` is the consumer that shows what
that costs, because its generator allocates the argument from the parameter type alone
(`v := reflect.New(concrete).Elem()`, then `for i := 0; i < v.Len(); i++`): every property test over
a fixed-size array ran against the EMPTY value. `crypto/internal/edwards25519`'s
`TestScalarSetCanonicalBytes` indexed `in[len(in)-1]` and panicked with `index out of range [-1]
with length 0`; its sibling `TestScalarSetUniformBytes` reported `failed on input [0]uint8{}`, which
names the empty array outright.

The datum therefore has to live at the parameter, and it does: the converter stamps
`[GoArrayDims(32)]` there (outermost dimension first — `[2][3]int` ⇒ `[GoArrayDims(2, 3)]`), from
the single `generateParametersSignature` all three signature builders share, so declarations,
methods, func literals, func types and interface methods are all covered by one emission point.

```go
f1 := func(in [32]byte, sc Scalar) bool { … }      // edwards25519's scalar_test.go
```
```csharp
var f1 = ([GoArrayDims(32)] array<byte> @in, Scalar sc) => { … };
```

`GoReflect.FuncParamDims` reads it back off the delegate **INSTANCE** —
`Delegate.Method.GetParameters()`, which resolves to the real declaration for every shape go2cs
emits (a declared func used as a method group, a non-capturing lambda, a capturing lambda's
display-class method, a natural-typed lambda, a local function) — and `abi.TypeOf` stamps it as
descriptor cargo beside `arrayDims`, so `reflect.Type.In(i)` hands out an array type that knows its
length. The cargo joins BOTH interning keys (`abi.descriptorDimsKey`, shared with reflect's
`canonType`) for the reason the array dims are already in them: `func([32]byte) bool` and
`func([64]byte) bool` are distinct Go types over one managed delegate type, so interning them
together would let whichever arrived first answer `In(0).Len()` for both.

Three boundaries are deliberate. A **defined** (named) array type is not stamped — its managed form
is a generated wrapper, not `array<T>`, so dims cargo could not be consumed even if carried — while
an **alias** for an array is, because a Go alias *is* its target type. **Result** dims are not
carried at all: a multi-result Go func returns a `ValueTuple`, which has no per-element attribute
position, and no measured consumer reads `Out(i).Len()`. And a delegate whose target method's
parameter list does not line up one-for-one with `Invoke`'s — an open instance delegate carries the
receiver as an extra leading parameter, and the bridge's own method values are expression-compiled
closures with no attributes — is answered `null` rather than mis-indexed, which is the r39d rule in
its usual form: a dims-less descriptor is a state the bridge already handles, a mis-indexed one is
not.

**The same datum two hops further out — a METHOD's parameters, and a `*[N]T` (2026-08-14).**
`net/rpc` is the consumer that found both hops missing, and it found them the hard way. Its server
allocates every reply argument from the method type alone —
`replyv = reflect.New(mtype.ReplyType.Elem())`, where `ReplyType` came from `mtype.In(2)` — so a
service method declared `func (BuiltinTypes) Array(i int, reply *[1]int) error` needs the `1` to
survive two hops the func-value route does not have:

1. **The func type comes from the method TABLE, not from a delegate instance.**
   `reflect.Type.Method(i).Type` is synthesized in `value_impl.cs` from `GoMethodFuncType` over the
   `MethodInfo`'s parameters, and nothing in that path ever holds a delegate for
   `GoReflect.FuncParamDims` to read. `GoReflect.MethodParamDims(t, i)` reads the same
   `[GoArrayDims]` stamps straight off those `ParameterInfo`s, and `Method(i)` carries them as the
   descriptor's `funcParamDims`. It owes no arity guard, unlike the delegate route: the delegate
   type is synthesized FROM that parameter list, receiver included, so the indices line up with
   `In(i)` by construction — which is Go's own shape for a method type, receiver first.
2. **The array sits behind a POINTER.** A callee that writes its result through a parameter takes a
   `*T`, so the type-only position whose length must survive is the *pointee's*. The converter now
   stamps a parameter's pointee dims (one hop; that is all a Go signature spells here), and a
   pointer descriptor's dims pass through `Elem()` **unshifted** — there is nothing else they could
   describe, a pointer having no length of its own — while an array's dims still shift, its element
   consuming the outer one. The stamp had to be added to `visitFuncDecl`'s REBUILT signature path as
   well, and that is where it actually fires: having a pointer parameter is itself what triggers the
   rebuild, so a `*[N]T` parameter never reaches `generateParametersSignature` at all.

Without either hop, `reflect.New(In(2).Elem())` built a zero-length array and the callee's first
write panicked `index out of range [0] with length 0` — on an rpc goroutine, which took the entire
converted-test host down with it (`net/rpc/jsonrpc`'s `TestBuiltinTypes`; the same run's other eight
tests then recorded no verdict at all, which is what made the panic read as three failures instead
of one).

Guard: `tests/Behavioral/ReflectFuncArrayParamDims`, byte-identical to `go run` — `In(0)`'s
`String`/`Kind`/`Len` and the `New`/`Zero` lengths across a literal, a multi-parameter literal, a
nested `[2][3]int`, a declared func used as a value and a func with no array parameter at all; the
distinctness of `[32]byte` and `[64]byte` as `In(0)` types; the inner dimension surviving `Elem()`;
the struct-field route still answering; and quick's generation loop in miniature (allocate from the
parameter type, fill through `Index(i).Set`, `Call`), so a zero-length synthesis shows up as the
callee's wrong answer rather than a silent pass. It carries rpc's shape too, since 2026-08-14:
`Method(i).Type.In(2).Elem().Len()` for a `*[3]int` reply, `reflect.New` of it, and the callee
writing through the pointer — a length the descriptor does not know is not merely mis-reported
there, it PANICS. Converter unit guard: `TestGoArrayDimsAttribute` (both shapes, including the
boundaries: a pointer to a DEFINED array type, to a slice, and a double pointer are all unstamped).
Corpus footprint of the 2026-08-14 half, measured by re-transpiling all 592 behavioral packages:
**5 declarations in 5 files, one line each** — four `*[N]T` parameters, plus one `[4]byte` VALUE
parameter (`DeferTypelessReturns`' `first`) that had been silently unstamped all along, its function
taking the rebuilt path because it heap-boxes. Nothing else moved.

**The `reflect.DeepEqual` bridge (`reflect/deepequal_impl.cs`, Phase-4 — blocker-map R5).** Go's
`deepValueEqual` keys its cycle-detection `visited` map on the values' internal data words (`v.ptr` /
`v.pointer()`) — eface addresses the managed bridge never populates — so the first slice/map/pointer
comparison converted the null `unsafe.Pointer` slot and NREd (`deepequal.cs:74` → unsafe `op_Implicit`;
first operational hits: strings and bytes `TestSplit`/`TestSplitAfter`). The converter skips **only**
`deepValueEqual` (`manualConversionFuncs["reflect"]`; `DeepEqual` itself stays auto — its body only
touches the bridged `ValueOf`/`Type`/`AreEqual`), and the hand-owned form re-implements the recursion
arm-for-arm with Go's switch over the bridge's **boxed** values: elementwise arrays/slices with the
`[]byte` fast path (`Span.SequenceEqual` standing in for `bytealg.Equal`); the nil-vs-empty slice
distinction read from the REAL backing (`m_array` null ⟺ the golib `default` = nil — the public
`slice<T>.Source` materializes a detached copy, so the impl reads `m_array`/`m_low` via cached
reflection, the same pattern as the bridge's `IsNull`/`Value` property reads); struct fields via
`goStructFields`; maps compared key-by-key through the backing `Dictionary` (same-map identity
short-circuits — Go's "same map object" rule — and a missing key fails exactly like Go's invalid
`MapIndex`); pointer identity as `ж<T>`-box reference equality (one box per variable ≙ Go's address
equality, so the same slice/pointer is deeply equal to itself even holding NaN); IEEE float semantics
(C# `==` on `double` — NaN ≠ NaN, like Go); funcs never deeply equal unless both nil. Cycle detection
mirrors Go's `hard()` step on managed identity: (pointer box | map `Dictionary` | slice backing array
+ `Low`) pairs in a reference-identity set, added before recursing so in-progress checks are assumed
true — self-referential structures terminate.

**Two defects the guard could not see (2026-07-26), one of them the guard itself.** The recursion read
the Value's raw `boxed` field, but an **ADDRESSABLE** Value — a slice element, an array element, a
struct field — carries its value behind `addrBox` (the `ж<T>` it aliases) and leaves `boxed` **null**,
so every such read saw null on *both* sides and the identity short-circuits fired:
`DeepEqual([][]byte{[]byte("ab")}, [][]byte{[]byte("ac")})` was **true** — each element's backing read
as null, matched "same initial entry of the same underlying array", and the elementwise walk never
ran. The recursion now reads `live` (identical to `boxed` for a non-addressable Value) once per side
and threads it through every arm. Second, `mapBacking` probed for a field assignable to the
**non-generic** `IDictionary`, which a generated named-map wrapper does not have — it holds a
`map<K,V>` **struct**, whose own backing store is one level deeper — so both sides of a *named*-map
comparison resolved to null, `ReferenceEquals(m1, m2)` matched them as "the same map object", and two
named maps of equal length were deeply equal **regardless of contents** (`identityRoot` was blind the
same way, so a named-map cycle was never detected either). The probe now takes that second step.

Neither showed up because the guard's own comparison was **vacuous**: it printed with the builtin
`println`, which writes to **stderr**, and the runners compare stderr by FIRST LINE only (Go's panic
reports carry a machine-specific stack trace, so a full stderr comparison can never match). 46 of its
47 assertions were unchecked. The guard now prints with `fmt.Println` — stdout, compared in full — and
is extended to 47 cases: the original slice/struct/map/pointer/cycle set plus named maps (equal,
differing value, differing key, differing length, self, nil-vs-empty), a named map as a struct field, a
named map OF slices, and nil-map-key parity on both plain and named `map[any]int`. Counter-proven: pre-
fix the `[][]byte` case and six named-map cases printed the wrong answer. (`Solitaire` is the only other
output-compared project that prints solely through `println`; its comparison is vacuous for the same
reason — recorded as a follow-up, together with tightening the runners' stderr rule to a full compare
whenever the exit code is not a panic.) The guard project references the
full-conversion `reflect` (the baseline stub has none): its `Directory.Build.targets` redirects the
emitted `core\reflect` reference to `core\reflect` — the Performance-suite pattern for
settings the per-transpile csproj regeneration would otherwise clobber. Surfaced by the guard's output
comparison: golib's `print`/`println` now render a `bool` as gc's runtime printer does (`true`/`false`,
not the BCL `True`/`False`).

**A third defect: the FUNC arm asked the wrong question (2026-07-31).** Go's rule is "func values are
deeply equal if both are nil; otherwise they are not deeply equal," and the arm implemented only the
second half — an unconditional `return false` — on the reasoning that two nil funcs would already have
matched through the `invalid == invalid` rule at the top. That reasoning holds for a **top-level** nil
func boxed as `any` (the null object, whose Value is invalid), and for nothing else: a nil func reached
as a struct **field** — or a slice/array element, or a map value — is typed by its *static* func type,
which by design makes it a **VALID** nil Value (see `Value.Field`). So any two structs carrying a nil
func field were reported unequal, and asking `IsNil()` of both values is the fix. The observable form is
a whole-struct comparison that can never succeed no matter how carefully the test normalizes the rest:
`compress/flate`'s `TestWriterReset` nils `fill`/`step`/`bulkHasher`/`bestSpeed`, copies `hashMatch`, and
substitutes `tokens`/`window` precisely so `DeepEqual` can compare everything else — and it failed at
**all ten** compression levels. That was the package's only failing test; with the arm corrected
`compress/flate` validates **64 / 64**. The diagnosis is worth recording because every *individual*
field compared equal (through `Field(i).Interface()`, which re-boxes and so re-enters the invalid-value
path) while the enclosing struct did not — the discrepancy between the two is what named the arm. The
`DeepEqual` guard gains nine cases: nil-func struct fields equal, one side non-nil, a struct with a
non-nil func field compared to **itself** (Go says not equal), differing non-func fields, nil funcs as
slice elements and as map values, and the top-level nil/non-nil pair that the old arm did handle.
Counter-proven by neutering the arm back to `return false`: the guard's output comparison fails.

**A fourth defect, the same shape one level out: the MAP arm read its entries UNTYPED (2026-08-17).**
The walk builds each entry's Value from the backing `Dictionary` — it cannot use the `MapRange`
iterator, because golib keeps a nil KEY in a side slot no iterator can see — and it built them with
`makeReflectValue`, i.e. from the stored object's dynamic type, where every other arm in the bridge
types a slot-derived Value by the slot's DECLARED type. An entry physically holding C# `null` therefore
came back as the **invalid** Value, so a nil element compared equal to a MISSING key and unequal to the
canonical typed nil that a reflective write stores. `encoding/json`'s `TestUnmarshal` rows #56–#63
decode into the 40-field `All` fixture and compare it against the table literal; `All.MapP` carries a
nil element, the decoder writes the box, the literal writes null, and eight subtests plus the aggregate
failed on that alone while `Marshal` of both sides produced byte-identical JSON. Entries are typed by
`Elem()` of the map's type now (`mapElemValue`), which is what `MapIndex` and `MapIter.Value` have
always done — so a lookup, a range and a `DeepEqual` over one map finally describe its elements
identically. The full rule, and why the container's verdict disagreed with every one of its elements,
is *A map ENTRY is a SLOT* below. Guarded by `ReflectBridgeClosure`'s *map nil element* rows;
counter-proven by reverting the arm, which reports `false false` where Go reports `true true`.

**A fifth defect, and it is the named-map one with SLICE substituted throughout (2026-08-19).**
`sliceData` — the probe that answers a Value's backing array and window offset, which is both the
`&x[0] == &y[0]` short-circuit's input and `identityRoot`'s cycle key — read `m_array`/`m_low` off the
boxed object's own type. A generated NAMED-slice wrapper (`type S []E`, e.g. `xml.CharData`,
`xml.Comment`, `net.IP`) has neither: it holds a `slice<E>` **struct** one level down, exactly as the
named-MAP wrapper holds its `map<K,V>`. So both sides resolved to `(null, 0)`, `ReferenceEquals(data1,
data2)` matched them as "the same initial entry of the same underlying array", and two named slices of
equal length were deeply equal **regardless of contents**; a nil named slice compared equal to an empty
one (the nil/empty rule tests the same two nulls); and `identityRoot` was blind the same way, so a
named-slice cycle was never detected. The probe now takes the same second step `mapBacking` takes,
gated on the type being slice-KINDED so a struct that merely HAS a slice field can never be mistaken
for one, and terminating because the nested value is a strictly smaller struct that carries the pair
itself.

The observable form is worth recording because it points at the wrong package. `encoding/xml`'s
`TestCopyTokenCharData`/`TestCopyTokenComment` clone a token's buffer, mutate the ORIGINAL, and assert
the two are no longer deeply equal; the failure message is *"CopyToken(CharData) uses same buffer"*,
which reads as a copy that failed to copy. It is not — `bytes.Clone` allocates a fresh backing array
and the `slice<byte>`→`CharData` conversion aliases correctly. The values were wrongly EQUAL, never
wrongly SHARING, and the owner was `reflect`.

**And the `[]byte` fast path was a second, independent gap in the same arm** — real, but NOT what made
the values compare equal (measured by A/B: fixing only the fast path leaves all eight wrong rows wrong;
fixing only `sliceData` makes all of them right, because the identity short-circuit fires FIRST and the
fast path is never reached). Go's `[]byte` special case is selected by the element **KIND**, never by
the slice's or the element's name — a raw `[]byte`, a defined slice type over `byte`, and a slice over
a defined byte element all route through `bytealg.Equal` — while the managed arm tested `live is
slice<byte>`, which only the first satisfies. It now asks `GoReflect.TryByteSliceView`, the same
element-kind alias `Value.Bytes`/`SetBytes` are built on (see *the `[]byte` VIEW of any Uint8-element
slice*), so all three shapes take one route in both APIs; both sides are the same Go type by the
`AreEqual` check above, so one view test settles both.

Guarded by the `DeepEqual` project's twenty new named-slice rows — the `CopyToken` shape verbatim
(clone, mutate the original, re-compare) plus a named byte slice through an interface, as a slice
element and as a map value; nil vs empty vs self; a named slice over a DEFINED byte element and one
over `string` so the fix cannot be byte-specific; and a self-referential `type recur []any` cycle,
which terminates only once the unwrap reaches the real backing array. Counter-proven failing-first:
**eight** of the twenty printed the wrong answer, every one of them wrongly `true`.

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

**The testing shim's compile-only benchmark surface and `CoverMode` (`core/testing/testing.cs`).** Capability-excluded test and benchmark declarations still **compile** — exclusion gates the run registry, not emission — so every member their bodies reference must exist even though the code never executes (a broken emission inside an excluded test blocks the whole package build; see the strings/bytes blocker map, B6). The `B` surface (`N`, `Run`, `ReportAllocs`, `SetBytes`, `ResetTimer`, `StartTimer`, `StopTimer`, `Errorf`, `Fatal`, `Fatalf`) is therefore compile-only: safe non-throwing no-ops, with the params-taking members carrying explicit `ж<B>` overloads exactly as `T`'s do (ref-like `params` Spans are outside the RecvGenerator's synthesis). `testing.CoverMode()` returns `""` — not a stub-lie but Go's exact coverage-off value: the sole caller across the strings/bytes suites (strings' TestIndexRune) branches on `CoverMode() == ""` and so takes the same path as an uncovered `go test` run. Guarded by `TestingRuntimeTests.BenchmarkCompileSurfaceIsNoOpAndCoverModeReportsCoverageOff`, which compile-references every member through both receiver shapes and asserts the coverage-off semantic — removing any member fails the suite at build.

**The same rule extends to `testing.F` (2026-07-20).** A `Fuzz*` declaration is classified disclosed-unsupported in the manifest exactly as a benchmark is (`testConversion.go` already emitted the `fuzz`/"deferred to Phase 4D" entry), but its converted body still compiles into the test assembly — and `F` simply did not exist, so math/big's `func FuzzExpMont(f *testing.F)` (nat_test.go) failed the whole package build with CS0426 *the type name 'F' does not exist in the type 'testing_package'*. `F` now mirrors `B`: a compile-only struct whose members are safe non-throwing no-ops, with explicit `ж<F>` overloads on the params-taking members. Its member set is Go 1.23's full public surface for `*testing.F` — the `TB` members it inherits from the embedded `common`, plus its own `Add` and `Fuzz` — declared complete under the same anti-drift rule as `TB` above rather than trimmed to today's callers. `Fuzz` takes a **`System.Delegate`**: a Go fuzz target's signature is arbitrary (`*testing.T` followed by the fuzzed argument types), and the converted body is an explicitly-typed lambda, so C# infers its natural `Action<…>` and converts — no per-arity overload set is needed. Nothing is invoked and no seed corpus is retained, because there is no fuzzing engine to consume either. This is not math/big-specific: roughly seventeen stdlib packages ship fuzz targets (archive/tar, archive/zip, compress/gzip, encoding/csv, encoding/json, html, image/{gif,jpeg,png}, net/netip, time, syscall, …), every one of which would hit the identical build blocker. At Go 1.24.13 the set follows 1.24's surface: `Chdir` and `Context`, which Go 1.24 added to `TB` and `F` inherits from `common`, joined it (`src/core/testing/testing.cs:589-591`).

**A hand-owned struct declares the zero-value constructor the generator would have given it.** An empty composite literal of a struct from another package converts to `new T(nil)`: `&bytes.Buffer{}` becomes `Ꮡ(new bytes.Buffer(nil))`, which binds the `T(NilType _)` constructor go2cs-gen writes for every converted struct. testing's `T`, `M`, `B`, `PB` and `F` are hand-written C# structs with no generator behind them, so each declares `public X(NilType _) { }` itself, and `&testing.T{}` (testify's require tests construct one) compiles like any other foreign literal. A hand-owned struct that a converted package may construct by an empty literal owes the same constructor. Guarded by the `TestingStructZeroLiteral` behavioral test.

## Realizing an asm-backed arch layer with managed hardware intrinsics

Hand-owning an asm-backed declaration does not have to mean stubbing it. Where .NET exposes the *same* instructions the `.s` file issues — via `System.Runtime.Intrinsics` — the architecture layer can be ported for real, and the converted package gains genuine hardware acceleration rather than a fallback. **`hash/crc32` is the first of its kind (2026-07-24) and sets the pattern.**

Go's `crc32_amd64.go` declares three functions with no body — `castagnoliSSE42`, `castagnoliSSE42Triple` (the SSE4.2 `CRC32` instruction) and `ieeeCLMUL` (PCLMULQDQ carry-less multiply folding) — implemented in `crc32_amd64.s`. Converted literally they become bodyless `partial`s that the [`PartialStubGenerator`](source-generators.md#source-generators) fills with `NotImplementedException`, so `crc32.go` always took the slicing-by-8 fallback and the package's own `TestArchIEEE`/`TestArchCastagnoli` **skipped**. `crc32_amd64.cs` is hand-owned (`[module: go.GoManualConversion]`, whole-file) and the three functions are transcribed against `System.Runtime.Intrinsics.X86` — `Sse42.X64.Crc32` for the CRC32B/W/L/Q chain, `Pclmulqdq.CarrylessMultiply` + `Sse2` for the fold and Barrett reduction, `Sse41.Extract` for the final `PEXTRD`. Every other declaration in the file is the converted output verbatim.

Three rules make this a repeatable recipe rather than a one-off:

* **Probe capabilities LOCALLY; never flip `internal/cpu`'s global flags.** The Go guards read `cpu.X86.HasSSE42` / `HasPCLMULQDQ` / `HasSSE41`. Those flags are shared by **every** converted package's arch path, and the rest of those arch layers are still throwing stubs — setting them centrally would trade each package's working portable fallback for a `NotImplementedException`. The hand-owned file instead defines its own predicates over the `.IsSupported` properties of exactly the instruction sets it uses (`Sse42.X64.IsSupported`; `Pclmulqdq.IsSupported && Sse41.IsSupported`) and `archAvailable*` returns those. The claim then stays precisely true — "these instructions are available to *this* code" — and `internal/cpu` is untouched. When a probe is false, `crc32.go`'s own `archAvailable*` branch falls back to slicing-by-8, which is exactly how Go degrades on an architecture with no arch implementation.
* **Port the `.s` file, label for label, and say so.** The fold constants are the `.s` `DATA` pairs verbatim; a 128-bit load puts the offset-0 quadword in the low half, so `Vector128.Create(offset0, offset8)` reproduces each register image exactly. `PCLMULQDQ`'s `imm8` maps directly onto `CarrylessMultiply`'s `control` (bit 0 selects the *left* operand's quadword, bit 4 the *right*'s), so `PCLMULQDQ $0x11, X0, X1` is `CarrylessMultiply(x1, x0, 0x11)`. Each ported block carries the `.s` label it transcribes (`aligned`, `less_than_8`, `loopback64`, `remain64`, `remain16`, `finish`) so the two can be diffed by eye at upgrade time.
* **Divergences are deliberate, documented in place, and result-neutral.** The `.s` file walks the buffer to an 8-byte boundary before its `CRC32Q` loop; the managed port drops that step. It exists to align the loads, it cannot change the answer (CRC is a pure function of the initial value and the byte *sequence*, and the `CRC(I, ABC)` combining identity holds for any split), and it has no managed counterpart — a slice's backing array can be moved by the GC, so an address observed here is not an address the loads keep. Likewise the `.s` triple loop is a `DECQ`/`JNZ` do-while that would wrap on `rounds == 0`; the managed form is a counted loop, identical for every real input.

Note the csproj constraint this recipe was written under: the converter **regenerates each package's `.csproj` on every transpile**, and it sets `AllowUnsafeBlocks` from `usesUnsafeCode` alone -- which was `false` here, so a hand-owned file could not use `byte*`/`fixed`/`stackalloc` and any setting added by hand was clobbered on the next run. That is no longer the only option (a file may now [declare the requirement](#a-hand-owned-file-can-declare-that-it-needs-unsafe)), but this recipe is still the better answer where it applies: it needs no compiler flag at all. The loads go through `MemoryMarshal.GetReference(p.ToSpan())` plus `Unsafe.ReadUnaligned<T>` / `Unsafe.Add`, which need no compiler flag and read the slice's real backing window (offset included) with no copy.

The payoff is that the package's *own* test suite becomes the correctness oracle, which is what makes this pattern safe to repeat: `TestArchIEEE` and `TestArchCastagnoli` cross-check the intrinsics against the portable slicing-by-8 implementation over randomized buffers at 46 lengths chosen to straddle the `168*3=504` and `1344*3=4032` cutoffs, and enabling the arch path also routes `TestGolden`/`TestGoldenMarshal` through it against known vectors. All 8 `hash/crc32` Test functions match `go test`, with nothing skipped and no disclosed divergences (Phase-4 validated package; see `docs/ValidatedTestPackages.md`). Confirmed by positive control — corrupting a fold constant and the `CRC32B` tail turns exactly the arch-dependent tests red while the portable-only ones stay green.

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

`builtin.mapclone(any m)` is Go's `runtime.mapclone` at golib level: it recovers the boxed map's concrete key/value types through `IMap.CloneMap()` (a default interface method on `IMap<TKey, TValue>` for the concrete `map<K, V>`, which a generated named-map wrapper overrides to keep its own type — see below; no reflection either way) and returns a fresh `map<K, V>` populated from the source's entries. The clone's backing `Dictionary` is **independent** — Go's shallow clone (keys/values copied by ordinary assignment), so mutating the clone never touches the original — and a nil map clones to nil. This is what carries the `maps` package to full Phase-4 validation (14/14 tests vs `go test`; the 6 Clone/Copy/DeleteFunc tests previously threw). Extend `linknameForwardBuiltins` when another linkname intrinsic gains a golib builtin. Guarded by the `MapCloneLinkname` behavioral test — the exact `//go:linkname clone maps.clone` shape in a `main` package, cloning a `map[string]int`, mutating the clone (overwrite/add/delete) and asserting the original is unchanged, output-compared vs `go run`; proven to emit the throwing stub against the un-fixed converter.

**A NAMED map clones to its own type.** The default above returns a plain `map<K, V>` for every
receiver, but `maps.Clone[M ~map[K]V](m M) M` asserts its worker's result back to `M`
(`clone(m).(M)`), so cloning a defined map type panicked: logrus's `maps.Clone(entry.Data)` on
`type Fields map[string]interface{}` read "interface {} is map, not logrus.Fields" on a goroutine and
cost the module 102 infrastructure errors. go2cs-gen's Map template now implements `IMap.CloneMap` on
every generated wrapper: a nil wrapper clones to the wrapper's own `default`, any other to
`new Named(new map<K, V>(…))` over the wrapper's map. The map is read through the same expression every
other member uses, which for a self-containing wrapper (`type Tree map[string]Tree`, held in a reference
holder) is `Value` and never the holder's storage. (Guarded by the `NamedMapClone` behavioral output
test — an ordinary named map, a nil one and a self-containing one, each read back through `%T`, with
the clone's storage independent and a `.(Fields)` assertion true where `.(map[string]any)` is false,
against Go.)

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
defines, while the consuming side is an ordinary bodyless func under a **one-argument**
`//go:linkname <thisFunc>` handle. `runtime/mgc.go` pushes into `unique`, `runtime/mheap.go` into `internal/weak`:

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

## `StructField.Tag` is a REAL read — the converter has always emitted the tag, nothing had ever read it

The converter emits a tagged field's Go struct tag verbatim at the declaration:

```go
NamedCurveOID asn1.ObjectIdentifier `asn1:"optional,explicit,tag:0"`
```
```csharp
[GoTag(@"asn1:""optional,explicit,tag:0""")]
public asn1.ObjectIdentifier NamedCurveOID;
```

`GoTagAttribute` aliases `System.ComponentModel.DescriptionAttribute`, so the text survives into
metadata. It has done so since tags were first emitted — and until 2026-08-09 **nothing in the
corpus read it**: golib's Go-field projection (`GoReflect.GoFieldInfo`) carried no tag, and the
reflection bridge's `rtype.Field` left `StructField.Tag` at its zero value. Every converted
struct therefore reported as UNTAGGED, and every tag-driven decoder in the standard library —
`encoding/json`, `encoding/xml`, `encoding/asn1` — saw a type with no tags at all.

The failure that surfaced it is subtle rather than loud, which is the point: `encoding/asn1`
omits an `optional` field whose value `DeepEqual`s its zero, so with the tag invisible
`crypto/x509`'s `marshalECPrivateKeyWithOID(k, nil)` MARSHALLED the nil `NamedCurveOID` instead
of omitting it, and `makeObjectIdentifier` rejected the empty arc list — `asn1: structure
error: invalid object identifier`, the whole of `crypto/ecdsa`'s `TestEqual`. Nothing about the
message points at reflection.

`GoFieldInfo` gains `Tag`, read from the declaration's attribute (the promoted-embed arm reads
it off the backing box field, since Go allows a tag on an embedded field); `rtype.Field`
surfaces it as `StructTag`. `Offset`, `PkgPath` and `Anonymous` stay **unpopulated**: no
truthful read backs them here, and a descriptor field whose read cannot be honored must not be
made to look truthful. (Guarded by the `ReflectStructTagCopy` behavioral test — raw tag text,
`Tag.Get` for two keys, `Tag.Lookup`'s absent-vs-empty distinction, and untagged fields
answering `""`, output-compared vs Go.)

## `reflect.Copy` is bridged element-wise — the auto form is a flat two-header memory move

`reflect.Copy` reinterprets BOTH operands' data words as `unsafeheader.Slice` headers
(`*(*unsafeheader.Slice)(dst.ptr)`) and hands them to `typedslicecopy`. That is a raw memory
move with no managed form, and on the bridge's never-populated `ptr` slot it dereferences a nil
`ж` outright. `encoding/asn1`'s `parseField` copies every parsed `[]byte` into its destination
through it, so this was `crypto/x509`'s `ParsePKCS8PrivateKey` and therefore the second half of
`crypto/ecdsa`'s `TestEqual` — reached only once the tag fix above let the marshal succeed.

The bridge copies element-wise through the same golib container interfaces every other bridged
container method uses, which keeps the ALIASING exact rather than approximating it: a slice
VALUE windows the backing store it shares with its parent, so an indexer write is a write the
parent sees — precisely what `typedslicecopy` does to the same memory. Kind and element-type
validation mirror Go's, including the documented special case where `src` may be a `String`
when `dst`'s element type is `byte`; a nil container on either side copies nothing, matching
Go's zero-length header. (Guarded by the `ReflectStructTagCopy` behavioral test — slice←slice
truncating at either side, slice←string, an addressable array through `Elem()`, a nil
destination, and a window slice whose copy must be visible in the ORIGINAL backing array.)

## `reflect.Type.Name()` — a DEFINED type HAS a name even when its underlying type is a composite

Go's rule is about DEFINEDNESS, not shape: `Name()` reports the type's name within its package for
any defined type and `""` only for a type that was never defined — `[]int`, `map[string]int`, `*T`,
`chan int`, `interface {}`, `struct { … }`. `type testSET []int` is defined, so its `Name()` is
`"testSET"` even though its underlying type is a slice.

The bridge had that backwards. Go's own `rtype.Name()` gates on the descriptor's `TFlagNamed` bit
(`abi.Type.HasName()`), which a **synthesized** `abi.Type` never carries, so the hand-owned
`rtype.Name()` substituted a shape test — `GoReflect.ElementType(st) is not null`, i.e. "does this
type have an element type?". That is true of a defined container exactly as it is of an unnamed one,
so every `type S []T` / `[N]T` / `map[K]V` / `chan T` / `*T` in the corpus reported no name at all.

The tell was already in the same descriptor: `PkgPath()` reads the SAME managed nesting and answered
`"main"` for those types while `Name()` answered `""` — a pair Go's own model cannot produce, since
a type with a package path is by definition a defined type.

The visible symptom was one byte. `encoding/asn1`'s `getUniversalType` distinguishes a SET from a
SEQUENCE on the type's name and nothing else:

```go
if strings.HasSuffix(t.Name(), "SET") {
    return false, TagSet, true, true
}
return false, TagSequence, true, true
```

so `Marshal(testSET([]int{10}))` produced `300302010a` where Go writes `310302010a` — `0x30`
SEQUENCE for `0x31` SET, with no error, no panic and no other divergence anywhere in the encoding.

The gate is now `GoReflect.HasGoName`, the managed stand-in for `TFlagNamed`. It mirrors
`GoTypeName` ARM FOR ARM, because `Name()` IS that method's output with the package qualifier
trimmed — the two disagreeing would let a type report a name it does not have, or hide one it does.
False for exactly the arms that render Go structurally: the raw golib containers matched by open
generic definition (`slice<>`/`array<>`/`map<,>`/`channel<>`/`ж<>`), `object` (`interface {}`),
`EmptyStruct` (`struct {}`), an anonymous-struct lift (`[GoType("dyn")]` without a `[GoLocalName]`,
which would make it a named function-local type), and the pointer-sourced adapter that stands for
`*T`. True everywhere else — including the predeclared scalars, since Go's `int` IS a named type.

The distinction the fix turns on is that a DEFINED container is not a golib container: the converter
emits it as its own wrapper type that merely IMPLEMENTS the container interface, which is why the
open-generic-definition test separates the two where an element-type probe cannot:

```go
type intSET []int
type byteArray [4]byte
type stringMap map[string]int
type intChan chan int
type intPtr *int
```
```csharp
[GoType("[]nint")] partial struct intSET;
[GoType("[4]byte")] partial struct byteArray;
[GoType("map[@string, nint]")] partial struct stringMap;
[GoType("chan nint")] partial struct intChan;
[GoType("ж<nint>")] partial class intPtr;
```

Three further answers change with it, all in the same direction and none of them a value Go can
produce: `interface {}`, `struct {}` and a lifted anonymous struct used to return their STRUCTURAL
spelling from `Name()` (there is no dot to trim, so the whole string came back) and now correctly
return `""`. `String()` was never affected — it has no such gate and rendered all of these correctly
throughout, which is why the defect stayed invisible to `%T` and to the `ReflectliteTypeName` guard.

(Guarded by the `ReflectStructTagCopy` behavioral test, which pairs each of the five named shapes
with its unnamed control and re-runs asn1's own `HasSuffix(Name(), "SET")` decision. Measured on
`encoding/asn1`'s converted suite: **37 of 38**, up from 35 — it closes `TestMarshal` #37 and also
`TestCertificate`, whose "sequence tag mismatch" and empty RDN name had been left unattributed on
the board and are the same root, since its `RDNSequence` is a `[]RelativeDistinguishedNameSET`.)

**The follow-on this section recorded is now CLOSED.** It read: `abi.Type.HasName()` itself is still
`false` for every synthesized descriptor, so `internal/reflectlite.rtype.Name()` — the ordinary
converted Go body, which gates on it — answers `""` for EVERY type; and populating the bit would
ALSO change `directlyAssignable`'s `T.HasName() && V.HasName()` short-circuit, "a corpus-wide
assignability change, not a naming one". That reading was exactly right, and the assignability
change is precisely why the bit was worth carrying. `synthesizeDescriptor` now stamps `TFlagNamed`
from the same `GoReflect.HasGoName` gate this section installed, so the descriptor bit and the name
a `Type` reports come from ONE predicate, and `reflectlite`'s `Name()` becomes truthful with it. See
[*Go's ASSIGNABILITY rule, and the identity walk underneath
it*](#gos-assignability-rule-and-the-identity-walk-underneath-it).

## Go's ASSIGNABILITY rule, and the identity walk underneath it

`reflect.Type.AssignableTo` was hand-owned as **identity on the carried `System.Type`, or
interface-implements**. Distinct Go types are distinct managed types, so identity gets named-type
distinctness for free — but it is strictly narrower than Go's rule, which also admits a value whose
type has the same UNDERLYING type as the destination provided **at least one of the two is not a
defined type**:

```go
type userDefinedBytes []byte
var u userDefinedBytes
u = []byte{1, 2, 3}   // legal: []byte is undefined, userDefinedBytes is defined
```

`database/sql`'s `TestUserDefinedBytes` is the measured consumer, and its symptom is a data bug
rather than an error. `convertAssignRows` tries two arms in order:

```csharp
if (sv.IsValid() && sv.Type().AssignableTo(dv.Type())) {
    case slice<byte> b: dv.Set(reflect.ValueOf(bytes.Clone(b)));   // arm 1 — CLONES
}
if (dv.Kind() == sv.Kind() && sv.Type().ConvertibleTo(dv.Type())) {
    dv.Set(sv.Convert(dv.Type()));                                 // arm 2 — SHARES the array
}
```

Go takes arm 1 and copies. The identity rule rejected the pair, so the converted run fell through to
arm 2 and handed the caller a **view over the driver's own array** — the test's own words, "got
potentially dirty driver memory".

**The hand-own is retired.** Go's body runs: `directlyAssignable(uu.t, t.t) || implements(uu.t, t.t)`.
It could not run before, because three things it stands on had no answer — and each had to land in
the same change, or the fix would have traded one wrong answer for a wider one.

**(1) `HasName()` had to become truthful FIRST.** `directlyAssignable`'s first gate is
`if T.HasName() && V.HasName() || T.Kind() != V.Kind() { return false }`. With the bit never set,
that gate passed for every pair — so retiring the hand-own without it would have called two DISTINCT
defined types over one underlying type assignable, which Go rejects:

```go
type myBytes []byte
type myOtherBytes []byte
// Go: myBytes is NOT assignable to myOtherBytes — both are defined.
```

**(2) `implements` — the FREE function — had to be bridged, not just the method.** `rtype.Implements`
was already hand-owned over `GoReflect.GoImplements`, but Go's own `directlyAssignable`,
`AssignableTo`, `convertOp` and `Value.assignTo` all route through the free `implements(T, V)`, whose
auto form reinterprets the descriptor as an `interfaceType` and reads `.Methods` off a promoted-embed
box that is default behind a synthesized descriptor — it **throws** for any non-empty interface. It
now answers from the same `GoReflect.GoImplements` probe the emitted `_<T>` asserts use, so a method
set cannot be answered one way by a type assertion and another by reflection.

**(3) `haveIdenticalUnderlyingType`'s downcast arms had to be fixed WITH it.** This is THE seat of
Go's type-identity relation — `ConvertibleTo` reaches it through `convertOp`, `AssignableTo` through
`directlyAssignable`. Five of its eight arms already worked: the scalar arm needs nothing, and
Array/Map/Pointer/Slice recurse through the `Elem()`/`Key()`/`Len()` that `internal/abi` synthesizes
(previous section). The **struct**, **func** and **interface** arms reached their operands by the
prefix-downcast idiom instead — and they did not fail loudly. They read ZERO of everything and
returned **true**:

| Arm | What it read | What it therefore reported |
|---|---|---|
| Struct | `len(structType.Fields)` → 0 for both operands, so the field loop never ran | any two structs IDENTICAL — including different field TYPES, a renamed field, and a different field COUNT |
| Func | `funcType.InCount`/`OutCount` → 0 for both | any two func types IDENTICAL |
| Interface | `len(interfaceType.Methods)` → 0 for both, which is Go's own "both empty ⇒ identical" | any two interface types IDENTICAL |

A false positive in an identity relation is the most dangerous shape this bridge produces, because
every caller reads it as permission. It was already live through `ConvertibleTo`; routing
`AssignableTo` through the same walk would have widened it to assignment.

All three arms are now bridged in `reflect`'s `value_impl.cs`, and the struct arm sits at the
**`reflect`** level on purpose rather than in `internal/abi`. abi's synthesized `StructType()`
deliberately leaves `StructField.Name` the zero `ΔName` — a `ΔName` is a pointer into the linker's
name blob and every reader walks it with raw-address arithmetic — so the field NAMES and TAGS Go's
identity walk compares are not there to be had one layer down. `reflect` already owns the named-field
projection (`rtype.Field`, over `GoReflect.GoFields`), and the walk reads that SAME projection, so
the fields a type hands out and the fields its identity is decided by cannot disagree. Every clause
Go compares is compared: field count, the struct's `PkgPath` (set when the struct holds an unexported
field), and per field the name, the type, the **tag when `cmpTags`** — the single place assignability
and convertibility diverge — the offset, and **embeddedness**, for which `GoReflect.GoFieldInfo`
gained an `Embedded` flag, since `struct{T}` and `struct{T T}` agree on everything else (an embed's
Go field name IS its type name).

Two residuals are stated rather than hidden. The **interface** arm proves "methodless" only for
`object` (Go's `any`), so a DEFINED empty interface with a managed type of its own is answered *not
identical* — the conservative direction, since a false negative degrades a caller to "this needs a
conversion" while a false positive hands it a silent wrong assignment. And a **defined methodless
func type has no managed identity at all**: the converter renders it inline as its base delegate, so
`type myFunc func(int) bool` and `func(int) bool` are one managed type, and the named/unnamed pairs
every other kind can assert cannot be produced for funcs.

### The CHAN direction is carried by the VALUE — descriptor cargo, exactly like an array's length (2026-08-20)

`ChanDir()` is the fourth member of the downcast family, and the one whose datum is not merely
unpopulated but **not in the managed type at all**. A Go channel type emits as golib's `channel<T>`
whatever its direction, distinguished only by the `/*<-*/` marker comment the type renderer places
for the reader:

```go
var recv <-chan int
var send chan<- int
var both chan int
```
```csharp
/*<-*/channel<nint> recv;   // all three land on ONE managed type
channel/*<-*/<nint> send;
channel<nint> both;
```

That is the same shape a fixed-size array's LENGTH has, and it is now solved the same way: the
direction rides on the **value** and reaches `reflect` as non-identity descriptor cargo. The datum
sits on the `channel<T>` struct rather than on its heap core — direction belongs to the Go TYPE, not
to the channel object, so two values of different directions may share one core, and the NIL channel
of a directional type has no core at all yet still has a direction (`channel<T>.SendOnly` /
`.RecvOnly`, the direction-carrying nil).

Three converter emission sites stamp it, and they are the three places a directional channel value is
BORN — the same finite set the array dims occupy, position for position:

| Position | Array length | Channel direction |
|:--|:--|:--|
| the made/constructed value | `new(32)` | `new channel<nint>(0, GoChanDir.Send)` |
| a struct FIELD's zero | `= new(4)` field initializer | `= channel<@string>.SendOnly` field initializer |
| behind a POINTER | `GoReflect.PointeeArrayDims` | `GoReflect.PointeeChanDir` |
| a func PARAMETER | `[GoArrayDims(32)]` | *not carried — see the boundaries* |

```go
ch := make(chan<- int)                       // text/template's TestIssue43065
p  := new(chan<- string)                     // reflectlite's TestSetValue row
type holder struct{ x chan<- string }        // reflectlite's TestTypes row
```
```csharp
var ch = new channel/*<-*/<nint>(0, GoChanDir.Send);
var p = Ꮡ(channel/*<-*/<@string>.SendOnly);
[GoType] partial struct holder {
    internal channel/*<-*/<@string> x = channel/*<-*/<@string>.SendOnly;
}
```

`GoReflect.ChanDirOfValue` / `PointeeChanDir` / `FieldChanDir` read it back — the field route off a
cached zero instance of the declaring struct, exactly as `FieldArrayDims` does — and `abi.TypeOf`
stamps it on the descriptor. The cargo joins BOTH interning keys (`abi.descriptorDimsKey`, shared
with reflect's `canonType`) for the reason the array dims are in them: `chan<- int` and `chan int`
are distinct Go types over one managed `channel<int>`, so interning them together would let whichever
arrived first answer `ChanDir()` and `String()` for both. `Type.String()` renders the arrow from the
same cargo (`GoTypeName(t, dims, chanDir)`), and a POINTER hands its pointee's direction down
**unshifted** through `Elem()` — a pointer has no direction of its own — which is the hop
`new(chan<- string)` takes.

**Unstamped is not a failure state.** A channel nothing stamped answers `BothDir`, which is what this
accessor reported for every channel before the cargo existed and remains the honest answer for a type
nothing narrowed. That is what keeps the change additive: only directional sites move.

Four boundaries are deliberate, and each is the same shape as one the array dims already draw:

- **A NARROWING conversion is not carried.** `var s chan<- int = ch` makes a value of a new Go type,
  but the narrowing is a plain struct copy with no construction to hook; stamping it would mean an
  explicit call at every assignment, argument and return of a directional channel in the corpus (89
  such positions, measured) for a datum no consumer reads. `reflect.TypeOf(s)` therefore still
  reports `chan int` there — the r39d rule, and the same boundary the func-param dims draw at
  results.
- **A DEFINED channel type is not stamped** (`type closeWaiter chan struct{}`), for the reason a
  defined ARRAY type carries no dims: its managed form is a go2cs-gen wrapper struct rather than
  `channel<T>`, so there is no field to carry the cargo. An ALIAS for a channel type IS its target
  and is stamped.
- **A func PARAMETER is not stamped.** The `[GoArrayDims]` position exists for arrays because
  `testing/quick` and `net/rpc` allocate from a parameter type; nothing measured reads
  `reflect.TypeOf(f).In(i).ChanDir()`.
- **A type PARAMETER instantiated at a channel type** routes through `ISupportMake`, which has no
  direction-taking form.
- **`reflect.ChanOf(dir, elem)` is untouched and still answers `BothDir`.** It is the auto
  conversion, and it fabricates a descriptor with no value behind it by reinterpreting a boxed
  `channel<unsafe.Pointer>` as Go's linker-allocated `chanType` record — a path the managed bridge
  cannot honor at all, of which the direction is the smallest part. Its behavior is unchanged by the
  cargo (it answered `BothDir` before and after), and no measured consumer reaches it; a lane that
  needs it owes the whole descriptor, not just the arrow.

The downcast this replaced read a direction out of the memory following the descriptor's value slot,
**non-deterministically**, so `reflect.MakeChan`'s `ChanDir() != BothDir` guard and the identity
walk's chan arm each answered differently run to run.

**Landing it retired the `chan-direction` disclosure class.** `internal/reflectlite`'s suite read the
direction three ways — `TestAssignableTo`'s `<-chan int → chan int` row (want false), and
`TestTypes`/`TestSetValue` stringifying a `chan<- string` slot (want `chan<- string`) — and all three
are birth positions, which is why a value-carried direction reaches them: two go through
`new(<directional chan>)` and one through a struct field's zero. All three pass, the manifest is
gone, and the class's own self-retirement text is spent.

**Two latent defects the cargo exposed, both fixed here.** `internal/reflectlite`'s hand-owned
`haveIdenticalUnderlyingType` chan arm had dropped Go's FIRST rule — *"x is a bidirectional channel
value, T is a channel type, and V and T have identical element types"* — keeping only the
strict-equality rule. It was invisible while every `ChanDir()` answered `BothDir`, because both rules
then agreed for every pair; with real directions it makes `var r <-chan int = make(chan int)` report
unassignable. And `reflect.Value.Len()` had no `IChannel` arm at all, so every channel Value reported
length 0 while `Cap()` answered correctly one method away — silent for the same reason the
named-string arm was, 0 being a real length.

### `reflect.Value.Recv` / `Send` over golib's channel, and why they could not land alone

Both auto forms open with the same downcast one layer down, and behind a synthesized descriptor the
reinterpreted `.Dir` reads **zero** — so `0 & RecvDir == 0` held for every channel and a plain
bidirectional `chan string` was refused as send-only. Past that test neither could have worked
either: both hand a `uintptr` channel address and an `unsafe.Pointer` element slot to `chanrecv` /
`chansend0`, external stubs the `PartialStubGenerator` fills with `NotImplementedException`. Both are
hand-owned over golib's `channel<T>`, reached through `IChannel`'s type-erased `ChanRecv`/`ChanSend`
(the bridge holds a BOXED channel, never a `channel<T>`), and bridging them removes the last live
caller of both stubs.

**The direction guard must fire BEFORE the receive, and that ordering is the whole reason the two
halves are one change.** A working `recv` behind a direction that always reads bidirectional converts
`text/template`'s `range` over a send-only channel from a fast, attributable error into an unbounded
hang — measured by the near-miss-finish lane at **51 verdicts lost** to a package deadline against
the 1 the bridge buys, which is why that lane wrote the bridge, measured it, and reverted it.

`send` assigns its argument through `marshalIntoSlot`, the rule `Value.Call` already used for a call
argument — Go's `assignTo` cannot serve here, because its managed form returns a Value carrying only
the never-populated raw `ptr` slot and drops the boxed companion the bridge actually reads, so the
channel received a bare null. Keeping both boundaries on one renderer is what makes a channel send
and a call argument box a typed nil the same way.

(Guarded by the `ReflectChanDirection` behavioral test, byte-identical to `go run`: all three birth
positions read back through `TypeOf`/`Elem`/`Field`, Go's four assignability answers, the recv bridge
over `text/template`'s own `count(5)` helper, blocking and non-blocking `Send`/`Recv`/`TrySend`/
`TryRecv`, a closed channel's drain-then-zero comma-ok, both direction panics by their Go messages,
a defined-channel-type control, and — timeout-bounded so a regression prints a named line instead of
wedging the suite — the `walkRange` shape itself. Proven failing-first by neutering each half
separately: with the cargo neutered every `dir=` reads `chan`, all four assignability answers flip to
`true`, and the send-only range prints `HUNG -- the direction guard did not fire before Recv`; with
the recv bridge's guard neutered to the auto form's zero, `range count(5)` panics
`reflect: recv on send-only channel`.)

(Guarded by the `ReflectConvertAssignable` behavioral test, extended from the `ConvertibleTo`
recursions to Go's full assignability rule: both gates of the unnamed↔named clause including the
two-defined-types negative, the interface clause in both directions, the struct arm against a
differing field type / a renamed field / a differing field count / a tag that conversion ignores and
assignment honors, the func arm's parameter and result discrimination, and the chan rows the bridge
can truthfully produce. 34 rows, compared line for line against `go run`, measured failing-first —
the struct and func arms reported `true` where Go reports `false`.
`GolibTests.GoStructLayoutTests.EmbeddedField_IsDistinguishableFromADeclaredFieldOfTheSameNameAndType`
pins the projection flag the struct arm stands on.)

### A struct FIELD's TYPE-ONLY array dims — `[GoArrayDims]` / `[GoMapKeyDims]` (2026-08-20)

The array-length cargo had a hole, and it was in the position that decodes: a field's dims came from
the declaring type's **zero instance**, which reaches an array the field IS and nothing an array is
BEHIND.

```go
type T1 struct {                              // encoding/gob's TestEndToEnd
    Marr map[[2]string][2]*float64
    N    *[3]float64
}
type Indirect struct{ A ***[3]int }           // encoding/gob's TestIndirectSliceMapArray
```

`FieldArrayDims` reads `= new(N)` back off `Activator.CreateInstance(declaringType)`. On `Marr` that
instance holds a **nil map**, and a populated one would help no more — a map entry is a value, while
`Key()`/`Elem()` answer for the TYPE. On `N` and `A` it holds a **nil pointer**, with no pointee to
measure. Both hops are ordinary at a **decode target**, which is exactly a struct nothing has
populated yet, so the datum has to be in the emitted C#. That is the same conclusion the func
PARAMETER position reached, and it takes the same carrier — an attribute:

```csharp
[GoType] partial struct T1 {
    [GoArrayDims(2), GoMapKeyDims(2)]
    public map<array<@string>, array<ж<float64>>> Marr;
    [GoArrayDims(3)]
    public ж<array<float64>> N;
}
[GoType] partial struct Indirect {
    [GoArrayDims(3)]
    public ж<ж<ж<array<nint>>>> A;            // ONE stamp, any pointer depth
}
```

The two attributes are named for **the accessor each feeds**, which is also what the descriptor's own
slots have always meant:

| Cargo slot | Attribute | What it is | Handed down by |
|:--|:--|:--|:--|
| `abi.Type.arrayDims` | `[GoArrayDims]` | an ARRAY's own dims (head consumed), a POINTER's pointee's, a MAP's element's | `Elem()` — tail for an array, **unshifted** for a pointer and a map |
| `abi.Type.keyDims` | `[GoMapKeyDims]` | a MAP's key's dims | `Key()` |

So nothing about `arrayDims` changed meaning; a MAP simply joined the POINTER in the unshifted arm it
already had, and `Key()` — a map type's second accessor, which had no slot at all — got one.
`keyDims` joins **both** interning keys (`abi.descriptorDimsKey`, shared with reflect's `canonType`)
for the third time and the third instance of one reason: `map[[2]string]V` and `map[[3]string]V` are
distinct Go types over one managed `map<array<@string>, V>`, so interning them together would let
whichever arrived first answer `Key().Len()` for both. `Type.String()` renders from the same pair
(`GoTypeName(t, dims, chanDir, keyDims)`), which is what turns `map[[]string][]*float64` back into
`map[[2]string][2]*float64` and `***[]int` back into `***[3]int`.

**Unstamped is not a failure state**, exactly as with the chan direction: a field nothing stamped
carries null and every accessor answers the dimension-less array it answered before, so only the
shapes above move.

**The pointer hop had to land on the VALUE side too, and that was found by measurement.** With the
stamps in, `encoding/gob`'s `TestEndToEnd` passed and `TestIndirectSliceMapArray`'s root moved one
frame — out of the type-compatibility rejection and into
`panic: reflect: reflect.Set using unaddressable value`, inside `growSlice`. The cause is that
`reflect.Value.Elem()` recovered a pointee's dims from the LIVE value alone
(`ArrayDimsOfValue(ReadPointerSlot(box))`), while `rtype.Elem` hands the descriptor's cargo down
unshifted. gob's `decIndirect` walks a `***[3]int` target by allocating each level from
`value.Type().Elem()`, so a hop reading the live value reads the nil pointer it is standing on,
allocates a **zero-length** array from the dimension-less descriptor, and the next hop measures that
zero as the truth. `Value.Elem()` now prefers the carried dims and falls back to the live
measurement — and it descends them through EVERY pointer hop, not only the one whose pointee is the
array, because `***[3]int`'s intermediate pointees are pointers. The live route still answers where
the descriptor is silent (`ValueOf(&[100]T{}).Elem().Type()` carries 100). This is the same lesson
the chan-direction cargo's fourth position taught, one layer over: **a value the bridge hands out
must describe itself the way the descriptor does.**

Four boundaries are deliberate:

- **A field that IS an array is NOT stamped.** Its `= new(N)` initializer already carries the length,
  through a route that also survives a value copy; stamping it would duplicate the datum and churn
  every array-bearing struct in the corpus for nothing.
- **A DEFINED array or map type is not stamped** (`type Row [3]int` behind a pointer,
  `type Set map[[2]string]bool`) — the same one-sentence boundary the chan direction draws, for the
  same reason: its managed form is a go2cs-gen wrapper rather than `array<T>`/`map<K,V>`. An ALIAS
  for one IS its target and is stamped (`types.Unalias` resolves it).
- **A SECOND nesting level is not carried** — `[][2]int`, `map[K]map[[2]string]V`, a func field's
  parameters. The cargo has exactly one `Elem()` slot and one `Key()` slot, so a second level has
  nowhere to live and no measured consumer asks (the r39d rule). `reflect.Type.String()` still
  renders `[][2]int` as `[][]int`, unchanged.
- **A func PARAMETER of map type is not stamped.** `[GoArrayDims]` reaches parameters already, but
  nothing measured reads `reflect.TypeOf(f).In(i).Key().Len()`.

(Guarded by the `FieldDimsCargo` behavioral test, byte-identical to `go run`: gob's own field shapes
read back through `Field(i).Type`, both map accessors, `reflect.New(mtyp.Key())` and
`reflect.New(mtyp.Elem())` — the exact pair `decodeMap` performs before it fills an entry — a
three-hop pointer chain on the type side, gob's `decIndirect` walk verbatim on the value side, each
map accessor exercised alone, the untouched initializer route, and the defined-type and
slice-element boundaries. Converter unit guard: `TestFieldDimsCargo`, 20 rows including every
boundary above. Proven failing-first by neutering each of the three halves separately: with the
converter stamp neutered every carried length reads 0 and every type string loses its dims — which is
`length mismatch in decodeArray` one frame down — while the initializer route is untouched; with the
bridge's `Key()` and map-`Elem()` arms neutered the field type strings still render correctly, off
the descriptor's own cargo, and only the accessor answers collapse; with `Value.Elem()`'s descriptor
precedence neutered ONLY the `decIndirect` line moves, landing on `[0]int 0` — the zero-length array
`growSlice` panics on.)

## The reflectlite MINI-BRIDGE mirrors reflect one layer down — and the closure that validated it landed five rules in SHARED machinery

`internal/reflectlite` is Go's reflect one layer down, and its bridge follows one doctrine:
**mirror `reflect`'s hand-owns over the SAME golib machinery, so the two layers cannot disagree**
— `valueInterface` mirrors `packInterfaceValue`, `rtype.PkgPath` answers `GoReflect.GoPackagePath`,
and the `AssignableTo` hand-own retired to Go's own literal body over bridged `implements` +
`haveIdenticalUnderlyingType` exactly as `reflect`'s did (the identity walk's helpers are compact
mirrors in `type_impl.cs`, reading the same `GoFields`/`TryFuncShape`/`GoImplements` projections).
The suite validates **30 of 30 matched, 0 disclosed** (plus 2 skip-parity rows) since the
chan-direction cargo landed on 2026-08-20 and retired the three it had, and closing it surfaced
five rules that belong to EVERY consumer of the bridge, not to reflectlite:

- **Field order is Go DECLARATION order, not CLR metadata order.** go2cs-gen mints every embed's
  `ʗ` backing field in a GENERATED partial, and partial parts concatenate — so a struct whose Go
  declaration embeds first carries its embed LAST in metadata. `GoFields` (and therefore the
  offsets table, `Field(i)`, fmt's `%v` walk, json's member order) reorders by the generator's
  ALL-FIELDS constructor, whose parameters carry the declaration order — applied exactly when an
  embed is present and the parameter names form a bijection with the projected field names.
  (`GoStructLayoutTests.FieldOrder_IsGoDeclarationOrder_NotMetadataOrder`.)
- **Go's UNEXPORTED-method rule joined the structural implements probe.** Two same-named
  unexported methods from different packages are DIFFERENT methods, so `ast.Expr`'s `exprNode()`
  marker is satisfiable only from `go/ast` — `StructurallyImplements` compares the interface
  method's declaring package against the candidate extension method's declaring package class
  (`GoReflect.GoPackageClassPath`; a `-tests` whitebox bridge class carries the production
  [GoPackage] stamp and counts as the production package). Stated residual: a PROMOTED method's
  candidate is the generator's wrapper in the embedding package, so a cross-package embed
  satisfying a foreign marker interface answers false — the conservative direction.
  (`GoUnexportedMethodPackageTests`.)
- **A nil FUNC crossing into interface space keeps its type** — `GoReflect.CanonicalNilFunc`, the
  delegate-shaped half of the one-nil-encoding rule. A delegate's nil IS `null`: right in every
  func-typed slot, type-erasing inside an interface, where Go packs (type=func-type, value=nil).
  The carrier is minted ONLY at the eface boundary (both packers) and resolved away by every
  read-back path — dynamic type, type assertion (succeeds against exactly its own delegate type
  with the null delegate), marshalling (stores as null), nilness.
  (`PointerNilPredicateTests.CanonicalNilFuncCarriesItsTypeAndResolvesAwayOnEveryReadBack`.)
- **One nilness, one home**: `GoReflect.IsNilGoValue` (moved from reflect's private
  `isNilGoValue`) is the single boxed-value nilness — structural pointer predicate, map
  representational nilness, the generated `== nil` operator probe — read by `reflect.Value.IsNil`,
  reflectlite's mirror, and the interface arm of both.
- **`GoTypeName` grew three arms** the suite measured: a generic INSTANTIATION renders Go's
  bracket form with IMPORT-PATH-qualified arguments (`B[internal/reflectlite_test.A]` — what
  `Name()`'s trim-at-the-last-dot-outside-brackets recovers), an anonymous-INTERFACE lift renders
  structurally exactly as the struct lift does, and a pointer descriptor's dims thread to the
  pointee (`*[10]int`, the same unshifted-cargo rule `Elem()` applies).
  (`GoTypeDefinednessTests`.)

**The `*_impl_test.cs` convention** is the piece that made the export_test surface hand-ownable:
`export_test.go` hands the suite raw `Value{typ, ptr, flag}` construction over descriptor
downcasts — unbridgeable literally — so `Field`/`TField`/`Zero` are registry hand-owns whose
implementation lives in `export_impl_test.cs`, the first TEST-file companion. The `_test.cs`
suffix keeps it under the production csproj's EXISTING test-artifact exclusion (no template
change, no corpus churn); `testConversion` globs `*_impl_test.cs` into the tests project's
compile items and the conversion digest. The companion mirrors `reflect`'s hand-owned
`Value.Field`/`Zero` over `GoFields`/`FieldAliasBox`/`ZeroValueOf`, and `TField` hands the
LITERAL `StructFieldType` the SYNTHESIZED `abi.StructType()` — the specialization section below —
so the type-side and value-side walks read one projection.

(Two converter emission rules landed with the closure, guarded by `structFieldEmission_test.go`:
a PARENTHESIZED array field type — `x ([32]int32)` — keeps its `= new(N)` initializer through
`ast.Unparen`, and the delegate lowering's parameter name-stripping shares
`convertToCSResultList`'s rule — a leading token is a name only when it is a plain identifier
that is not a type-leading keyword, so a bare `chan *T` parameter keeps its channel layer.)

## `abi.Type`'s SPECIALIZATIONS are synthesized, not downcast — `StructType()` / `ArrayType()`

Go's `(*structType)(unsafe.Pointer(t))` is the **prefix-downcast** idiom: the linker really allocated a
`structType` and handed out a pointer to its embedded `Type` header, so casting back reaches the
sub-record. The section on `Reinterpret` above names this as the one case the managed arm deliberately
does not cover — nothing sits behind a `ж<abi.Type>` but an `abi.Type` — and these are the two sites
where converted code took that cast anyway.

The failure is not the contained wrong read the address route usually gives. `Reinterpret` correctly
**refuses** to alias managed storage for a reference-bearing pair (aliasing would fabricate object
references), so it fell through to the raw address and read `ΔStructType`'s fields out of the memory
that follows the descriptor's value slot. Probed on `abi.TypeFor[testStringStruct]()`:

```
Fields.Length   8830452760576     <- an address fragment read as a slice length
Fields.Capacity 16                <- the descriptor's OWN Size_, bleeding through the shifted view
```

`m_array` happened to land on a real heap object, so indexing it threw `IndexOutOfRangeException`
rather than access-violating — a caught CLR type-safety break. That is **six of `unique`'s nineteen
rows**, thrown on the first iteration of `unique.buildStructCloneSeq`, and `internal/reflectlite`'s
`NumField`/`Len` read the same garbage.

Both specializations are therefore hand-owned in `internal/abi/type_impl.cs` (registered in
`manualConversionFuncs` as `Type.StructType` / `Type.ArrayType`, so the converter emits a placeholder
comment for the Go bodies) and **synthesized from the descriptor's carried `System.Type`**, exactly as
the descriptor itself is:

| Field | Answer |
|---|---|
| `StructType.Fields[i].Typ` | `synthType` of the projected Go field type, dims-stamped from the declaring zero instance |
| `StructType.Fields[i].Offset` | the field's **Go** (amd64) byte offset |
| `ArrayType.Len` / `.Elem` / `.Slice` | the descriptor's carried array dims, the element descriptor, and `[]T`'s |

The offsets are Go's numbers, not the CLR's — a Go `string` is 16 bytes where `@string` is an 8-byte
reference — and they come from the SAME walk that stamps a descriptor's `Size_` and `Align_`
(`GoReflect.GoFieldOffsets`, `GoSizeOf` and `GoAlignOf` all read one memoized `structLayoutOf` pass),
so a field's `Offset`, its struct's `Size_` and its `Align_` cannot disagree. `unique`'s `cloneSeq` values are the demonstrated consumer:
`struct{ z float64; b string }` → offsets `[8]`, `[2]struct{ a string }` → `[0 16]`, `[3]string` →
`[0 16 32]`, each matching `go test` exactly.

Two things are deliberately **not** invented, following the r39d rule that a descriptor field whose
read cannot be honored must not be populated to look truthful. A descriptor with no `System.Type`, or
a struct holding a field whose Go size is unknowable (one unknown size makes every later offset a
guess), answers Go's **nil** — which every Go caller already tests. And `StructField.Name` /
`StructType.PkgPath` stay the zero `ΔName`: a `ΔName` is a pointer into the linker's name blob and
every reader of one walks it with `addChecked` raw-address arithmetic, the same route that produced
the garbage above, whereas Go's own `ΔName.Name()` answers `""` for a nil `Bytes` — so the zero value
is a state the format *defines* rather than a fabrication. A named field descriptor already comes
from `reflect`'s hand-owned `rtype.Field` over `GoReflect.GoFields`, and no converted caller of
`abi.StructType` reads a field name.

**`Elem()` and `Key()` are the same idiom one level in, and they are synthesized too.** `Elem()`
downcasts the header to the `sliceType`/`arrayType`/`chanType`/`mapType`/`ptrType` behind it and reads
that record's `Elem` field; `Key()` does it for a `mapType`. They inherited the defect exactly, and
answered **nil** for every slice, array, chan, map and pointer descriptor in the corpus.

Nil is what made this one *fatal* rather than merely wrong. The specializations above return nil for
an unknowable layout and every Go caller tests it — but nothing tests `Elem()`. `reflect`'s
`haveIdenticalType` recurses straight into `nameFor(t)`, which reads the descriptor's carried
`System.Type` and nil-dereferences, so the whole of `ConvertibleTo`/`AssignableTo` died for any
operand that was not a scalar. Both are now hand-owned (`Type.Elem` / `Type.Key` in
`manualConversionFuncs`) over `GoReflect.ElementType` / `KeyType` — the **same** golib resolution
`reflect`'s own hand-owned `rtype.Elem` / `rtype.Key` use one layer up, so the descriptor layer and
the `reflect` layer cannot disagree about what an element type is. The dims cargo threads by the same
rule `rtype.Elem` applies: an array descriptor's element takes the **tail** of `[outer]…[inner]`,
while a pointer's dims are the pointee's already and pass through unshifted. Kinds with no element
still answer Go's nil, which is Go's own answer for them.

Same defect class, **still open** and deliberately not chased here: `MapType()`, `FuncType()`,
`InterfaceType()` and the free `Len()` reinterpret the same way — each awaiting a measured consumer,
since a synthesized `ΔMapType` would have to populate runtime-map fields (`Hasher`, `KeySize`, the
indirect-key/elem flags) that have no managed answer at all.

Guarded by `GolibTests.GoStructLayoutTests` (Go offsets and sizes for the exact shapes `unique`'s
`TestMakeCloneSeq` exercises, plus alignment padding — removing the per-field alignment rounding fails
`FieldOffsets_ApplyGoAlignmentPadding`), and measured by `unique`'s own suite: **1 → 4 of 19 matched**,
with all six `IndexOutOfRangeException` rows gone and the three `TestHandle` ones moved on to the
`internal/weak` linkname root behind them. `Elem()`/`Key()` are guarded by the
`ReflectConvertAssignable` behavioral test — `ConvertibleTo`/`AssignableTo` across all six
element-bearing kinds, named and unnamed, compared line for line against `go run` — with the golib
resolution they stand on pinned by `GolibTests.PointerNilPredicateTests`'
`ElementAndKeyResolveForEveryKindTheDescriptorMustServe`.

## A managed REFERENCE is a Go pointer, not a Go struct — the reflection bridge's descent rule

`GoReflect.KindOf` classifies a managed `Type` to a Go `reflect.Kind`, and exactly one of those kinds
means *look inside*: `Struct`. `GoSizeOf`, `GoAlignOf` and `IsComparable` all stop at every other
kind, because that is Go's own layout rule — a pointer, slice, map, chan, interface or func field is
a fixed-size header (8/24/8/8/16/8 bytes on amd64) whatever it refers to, and only struct and array
fields recurse. Go can afford that rule because a Go struct containing itself by value is a type Go
itself rejects, so the descent is finite by the source language's own definition.

`KindOf`'s fallback answered `Struct` for any managed **reference** type it did not otherwise
recognize, and that broke the rule in the one direction that matters. The converter emits every Go
struct as a C# **value** type — the entire converted corpus carries seven `[GoType] partial class`
declarations and all seven are named-POINTER types (`type P *T`), classified `Pointer` structurally
before the fallback is reached — so a reference type arriving there is never a Go struct at all. It
is an opaque managed handle: the backing object a hand-owned shim holds in place of Go's own
representation. Answering `Struct` sent the walks into the CLR's own private fields and from there
into the BCL object graph, which has no rule against cycles:

```
Named -> Mutex -> SemaphoreSlim -> TaskNode -> TaskNode -> ...
```

`sync.Mutex` is the corpus entry point — hand-owned on a lazily-created `SemaphoreSlim` because Go's
runtime sleeping semaphore has no managed form — and `SemaphoreSlim`'s async wait queue is a linked
list, so `TaskNode.Next` is a `TaskNode` and the descent never ends. `go/types`' `TestSizeof` asks
`reflect` for the size of `Named`, which holds a `sync.Mutex`, and the run died there with a
`StackOverflowException`: uncatchable, so it took the whole process and reported the 44 tests
alphabetically after it as **absent** verdicts rather than as one failure.

The rule is therefore stated positively: **a managed reference is one pointer word wide and is never
descended into.** `KindOf` reports `Pointer` for it, which is both finite and Go's own answer —
`sync.Mutex` is 8 bytes in Go, and the shim that stands in for it is 8 bytes here, which is why
`go/types`' `Named` computes to Go's exact 112. Termination now rests on a rule that holds in the
target language rather than the source one: only `Struct` and `Array` recurse, `Struct` is answered
for value types alone, and C# forbids a value type from containing itself transitively (CS0523).

Two consequences worth carrying:

- **A legal self-referential Go type still gets a real size, not a bail-out.** `type node struct {
  value string; next *node }` terminates at the pointer and answers offsets `[0 16]`, size 24. A
  cycle *detector* would have answered "unknown" here; the classification answers correctly, which
  is why the fix is the classification and not a guard.
- **The depth cap underneath is a safety net, not the algorithm.** `structLayoutOf` refuses to
  recurse past 128 levels and answers "size unknown" (the r39d rule — a descriptor field that cannot
  be read truthfully stays unpopulated). No legal graph can reach it; tripping it would mean the
  classification is wrong again, and the point is that the next such defect costs a wrong number
  instead of a dead process and a run's worth of unmeasurable verdicts.

Guarded by `GolibTests.GoStructLayoutTests` — `ManagedReferenceField_IsOneWord_NotAStructToDescendInto`,
`CyclicManagedReferenceGraph_Terminates` and `SelfReferentialThroughPointer_IsFiniteAndCorrect`, the
first two of which are guards against a stack overflow no assertion can catch, so reaching the
assert at all is the guard. Measured by `go/types`: 513 verdicts with 44 absent, then **557 of 557
agreeing with `go test`**.

### The VALUE side of the same rule: pointer KIND is not pointer BOX

Classifying the handle `Pointer` settled what the *layout* walks do with it, and left a second
question the *value* walks have to ask before dereferencing one: **is there a slot behind it at
all?** For an opaque handle there is not — what it refers to has no Go representation — so "one word
wide, do not descend into it" is also **"no slot, do not read through it."** The two are one rule
asked at two layers, and only the first half had been stated.

`reflect.Value.Elem` asked the wrong question. It resolved a pointee with `GoReflect.ElementType`
and, on `null`, fell through to a "detached read" through `GoReflect.ReadPointerSlot` — which
classifies the box shape itself and threw `Not a pointer box type: go.sync_package+RWState`. That is
every `reflect.DeepEqual` over a struct holding a `sync` primitive: `DeepEqual` reaches `Pointer`
kind, calls `Elem` on both sides, and dies (`crypto/tls`'s `TestCloneNonFuncFields` is the measured
case, and `sync.Mutex`'s `SemaphoreSlim` gate, `sync.RWMutex`'s `RWState` and `sync.WaitGroup`'s
`WaitGroupState` are all the same shape).

The classification now lives in **one** place — `GoReflect.TryPointerBoxElement`, which
`slotAccessorShape` is refactored onto, so "can I read through this?" and "what will I read?" can
never be answered by two different probes. A caller holding a `Pointer`-kind value asks first;
`reflect.Value.Elem` and its `internal/reflectlite` twin answer the **invalid Value** for a handle,
which is the answer they already give for a nil pointer.

That blindness is deliberate and it is *Go's own answer*, not a concession. Go's `sync.RWMutex` is
state **words**, and a used-then-released lock is back at its zero state, so two of them are deeply
equal — which is exactly what two handles now compare as, whether or not the shim has lazily created
one of them. What the walk still sees is any **real** Go state beside the handle: a `sync.Once` that
has run differs from a fresh one, because `done` is an ordinary field and not part of the handle.
Guarded by `GolibTests.PointerNilPredicateTests` —
`OpaqueManagedHandleIsPointerKindButNotAPointerBox` asserts *both* halves in one test, since either
alone would let the other drift back, and `PointerBoxShapesResolveTheirPointee` pins the positive
side so the fix cannot turn real dereferences into nil — and by the `DeepEqual` behavioral test's
`guarded` struct, which compares a locked-then-released `Mutex`/`RWMutex` pair against `go run`.

## An EMBEDDED field is an embed to `reflect`, tag included — and Go's whole embedding contract reads it

`reflect.StructField.Anonymous` is not a cosmetic flag. It is the signal every Go encoder uses to
decide whether a field's own fields are *flattened* into the enclosing object: `encoding/json`'s
`typeFields`, and the same walk in `encoding/xml`, `encoding/gob` and `text/template`, treat a field
as an embed exactly when `Anonymous` is set and no name tag overrides it.

The bridge left it unpopulated, on the recorded ground that no consumer demanded it. One does.
Reported `false`, every embed became an ORDINARY field named after its type:

```
{"S1":{"X":2},"S2":{"X":4}}      // where Go writes {}
{"S":"B","BugA":{"S":"A"}}       // where Go writes {"S":"B"}
json: unknown field "Level1b"    // where Go reports "extra"
```

It is a real READ, not a reconstruction: the converter emits an embed as a partial property over a
marker-prefixed backing box, and golib's field projection records that shape as
`GoFieldInfo.Embedded` — the same flag `reflect`'s struct-identity walk already compares (Go's
`haveIdenticalUnderlyingType` ends each field with `tf.Embedded() != vf.Embedded()`).

**An embed's TAG lives at a different declaration site, and that is the second half.** The converter
stamps `[GoTag]` on the emitted partial PROPERTY:

```csharp
[GoTag(@"json:""e,omitempty""")]
public partial ref ж<Embed0b> Embed0b { get; }
```

while `go2cs-gen` mints the backing field the property returns a ref to — *without* carrying the
attribute across:

```csharp
private global::go.ж<…Embed0b> ʗEmbed0b;    // no [GoTag]
```

The projection reads FIELDS, so every embedded field came back untagged — silently, because `""` is
the right answer for most embeds. `GoReflect`'s embed arm now asks the PROPERTY first (the
declaration) and keeps the field as a fallback, so a tagged embed reports its tag with no generator
change: `encoding/json`'s `TestMarshalEmbeds` emitted `"Embed0b":{…}` where Go emits `"e":{…}`, and
marshalled the `json:"-"` embed it must omit entirely.

**Recorded, not fixed: field ORDER.** `go2cs-gen` emits the promoted-embed backing field in a
generated partial, i.e. AFTER the declaring part's plain fields, so `Host{X; y; Inner; inner; Ptr}`
projects as `X, y, Ptr, Inner, inner` where Go walks it in declaration order. No measured consumer
observes it — `encoding/json`'s dominance rules are decided by DEPTH and tag, never by declaration
order, and its one order-sensitive test (`TestMarshalEmbeds`) declares its single plain field first,
so the two orders coincide. The shape that will expose it is a struct interleaving plain and embedded
fields whose *key order* is compared, and the remedy is declaration-order cargo (the way array dims
are carried), not a re-sort in the projection. Guarded by the `ReflectBridgeClosure` behavioral test,
which looks its fields up BY NAME for exactly this reason — a guard that asserted the current order
would pin the gap as a contract.

## An UNNAMED func type renders STRUCTURALLY, exactly as an unnamed struct does

`GoReflect.TypeNaming` had no delegate handling at all, so a Go func value printed the CLR delegate
family standing in for it — `fmt`'s own `TestSprintf` reads it back:

```
(Action`1)(0x26d47ab)          // was
(func(*testing.T))(0xPTR)      // Go
```

A func type is named or unnamed by the same test every other type uses, and for converted code that
test is exact: a Go DEFINED func type is emitted as its own `delegate` nested in the declaring
`<pkg>_package` class (`http.HandlerFunc`), while an unnamed one lands on a BCL/golib delegate family
(`Action<ж<T>>`, `Func<…>`, the variadic `Funcꓸꓸꓸ`/`Actionꓸꓸꓸ`, or C#'s natural delegate type) that
no converted package declares. So `GoTypeName` renders the second group from `TryFuncShape` in Go's
own format — `func(` inputs `)`, then nothing / ` T` / ` (T, U)` for zero, one and several results,
with a variadic tail written `...T` — and `HasGoName` answers `false` for it, arm for arm, which is
what keeps `Name()` and the descriptor's `TFlagNamed` agreeing with the string.

**The variadic tail is recognized by SHAPE, not by the delegate family's name.** The golib families
carry a name marker and the detection used to rely on it, but a declared `func(string, ...int)` used
as a method group in an `any` position acquires C#'s NATURAL delegate type instead, whose name carries
no marker — so it reported non-variadic and `In(1)` handed back a raw `Span<int>`, rendering
``func(string, Span`1)``. A `Span<T>` parameter cannot arise any other way in converted code (Go has
no such type, and the converter emits one only for a variadic tail), so testing for it subsumes the
name test rather than widening it.

Residual, stated rather than hidden: a defined **methodless** func type has no managed identity — the
converter renders it inline as its base delegate family — so it is indistinguishable from an unnamed
one here and reports the unnamed answer. That is the same "describe the type the bridge can actually
build a descriptor for" rule `ChanDir` settles on. A defined func type carrying a method does get its
own delegate and keeps its name. Guarded by `ReflectBridgeClosure` (six unnamed shapes and one named,
against `go run`) and `GolibTests.GoReflectBridgeClosureTests`.

## `reflect.Value.Bytes`/`SetBytes` are defined over the element KIND, and they ALIAS

Go's `Bytes()` accepts any slice whose element kind is `Uint8` — `[]byte`, `[]renamedByte` where
`type renamedByte byte`, and a defined slice type over either — plus an addressable byte ARRAY, and it
reaches the storage by re-typing the slice HEADER. `SetBytes` assigns one back the same way. Neither
ever copies, and that is not an implementation detail: `Bytes()` is how a caller *writes into* a
reflected byte slice.

Two of the three slice shapes were already aliased (a raw `slice<byte>` is itself; a defined slice
type over plain byte answers through its `ISlice<byte>` view, which shares its backing). The third — a
DEFINED byte ELEMENT — had no route at all: `slice<renamedByte>` holds a `renamedByte[]` of one-field
wrapper structs, an unrelated instantiation with no conversion to `slice<byte>`, so the catch-all cast
threw `InvalidCastException` out of a core reflect API (`encoding/json`'s `TestSliceOfCustomByte` and
`TestEncodeRenamedByteSlice`; `fmt`'s `%x` of a `[]renamedUint8` and five siblings of one table-driven
test).

The route is `slice<T>.AliasOfElement`, a bridge primitive that re-spells a slice's element type while
carrying its window across unchanged, and its whole safety argument is the gate in front of it: the
two element types must be ONE representation under two Go names — **both value types, both free of
managed references, both exactly one byte wide** — asked of the managed types directly and never
inferred from the `[GoType]` token. Those are the same three facts the blessed
`ReinterpretAliasesStorage` gate asks of a pointee pair. Under them the two backing objects are
byte-for-byte the same shape and differ only in their method table, and every access golib makes
through a `slice<T>` addresses the data from the STATIC element type and the array's own length field
(`Span<T>`'s array constructor skips its covariance check outright for a value-typed `T`). What the
pun does not survive is a runtime type test on the array OBJECT — `Array.Copy`'s element check,
`backing is byte[]`, `backing.GetType()` — so nothing may reach one through the result, which is why
it is an internal primitive rather than a public conversion.

**`SetBytes` was worse than incomplete, and for a reason with nothing to do with named types.** Its
auto body is `*(*[]byte)(v.ptr) = x`, which converts to a store through
`(ж<slice<byte>>)(uintptr)(v.ptr)` — `v.ptr` is the Go data word this bridge never populates, so the
store went through a box over address 0 and landed nowhere, for EVERY byte slice including a plain
`[]byte`. Silently: `encoding/json`'s `literalStore` decodes base64 into a fresh buffer and hands it
over with exactly this call, so every `[]byte` field decoded as empty and `TestLargeByteSlice`
reported a 2000-byte round trip diverging at byte 0. It is hand-owned now and writes where every other
setter writes, through the addressable Value's aliased box.

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

**Its other half is descriptor cargo.** A POINTER descriptor carries its POINTEE's array dims
unshifted — the rule `Elem()` already applies when it hands them down — but nothing populated it, so
`reflect.TypeOf(new([3]int)).Elem()` described a dimension-LESS array and `reflect.New` of it
allocated a zero-length one, giving the fresh value a different Type from the one it mirrors.
`abi.TypeOf` measures it now, through `GoReflect.PointeeArrayDims`.

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

## A map ENTRY is a SLOT — its Value is typed by the map's ELEMENT type, never by what it holds

Every Value the bridge hands out of a container is *slot-derived*: a struct field, a slice element, an
array element, a func result and a `MapIndex` lookup all build their Value from the **declared** type
of the place the value sits (`makeTypedValue`), not from the object found there (`makeReflectValue`,
which is Go's rule only for `ValueOf` and interface `Elem`, where the type genuinely does come from the
value). The distinction is invisible until the slot holds nothing: a null read through the declared
type is a VALID nil Value of that type, while the same null read dynamically is the **invalid zero
Value** — a different thing entirely, and the one Go reserves for "this slot does not exist".

`deepValueEqual`'s map arm was the last read that skipped it. It walks the backing `Dictionary`
directly — it must, because golib keeps a nil KEY in a side slot no iterator can see — and it built
each entry's Value from the stored object. That is harmless while both sides spell nil the same way,
and the two sides do not:

```go
want := map[string]*Small{"19": {Tag: "tag19"}, "20": nil}  // literal: the entry physically holds C# null
json.Unmarshal(data, &got)                                  // decoded: the reflective write stores the canonical nil box
reflect.DeepEqual(got, want)                                // false — invalid Value vs valid nil pointer
```

Two spellings of one nil is not itself a defect: `packInterfaceValue` re-encodes a null pointer slot as
`ж<T>.NilBox` precisely so a typed nil survives being handed out as an interface, and the write path has
always stored that box. A type-blind READ is what makes them observably different — and in the same
stroke it makes a nil element compare EQUAL to a missing key, since both answer the invalid Value.
Typing the entry by `Elem()` of the map's own type collapses both: two nil elements meet at the kind's
nil rule (pointer — neither box is a real address; interface — `IsNil() == IsNil()`), a missing key
still fails on `Contains`, and a nil element still separates from a present non-nil one.

It is the whole of `encoding/json`'s last divergence. `TestUnmarshal` rows **#56–#63** decode into the
40-field `All` fixture and compare with one top-level `DeepEqual`; `Marshal` of both sides produced
**byte-identical** JSON, so nothing in the failure text pointed at a field. A field-by-field walk did:
`All.MapP` — `map[string]*Small{"19": …, "20": nil}` — reported false **at the map** while every element
compared equal beneath it, because that walk re-boxed each element through `Interface()` and so
re-entered the dynamic path on both sides. A container's verdict disagreeing with its own contents' is
the signature of a slot read that lost its type, and it is worth recognizing on sight: the same
discrepancy named the FUNC arm of this very function earlier (see *Manually-Converted Declarations*).

Guarded by `ReflectBridgeClosure`'s *map nil element* rows — a map built through `SetMapIndex` compared
against the same map written as a literal, both directions, plus the separations that must survive (a
nil element vs a different key, vs a present non-nil element) and the interface-valued flavour, where a
nil entry is the nil interface rather than a typed nil.

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

**Two of those knobs later moved back to runtime (2026-09-28, M1).** `setGCPercent` and
`setMemoryLimit` forward to runtime's own bodies through two public crossings
(`SetGCPercentManaged`, `SetMemoryLimitManaged`). Once `systemstack` was `fn()` and the heap lock
managed, those bodies run as written; `gcWaitOnMark` returns at once with no mark phase. So GOGC and
GOMEMLIMIT have Go's one home, `gcController`, and `runtime/metrics`' `/gc/gogc:percent` and
`/gc/gomemlimit:bytes` read the values the knobs set. `gcController` starts from the environment as
Go's `gcinit` does: `goenvs_impl.cs`'s module initializer, schedinit's slot, fills `envs` and then
runs `gcinitController`, in Go's order and in one initializer, because C# does not order two. The
knobs still have no effect on the CLR's collection.

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

**`unsafe.Pointer(uintptr(0))` must compare equal to `nil`.** The converter bridges every
`unsafe.Pointer`-valued call through `uintptr`, because `unsafe` lives in its own assembly and can
carry no implicit conversion on the core pointer class. golib's round-trip therefore has to preserve
nil in *both* directions, and it did not: `Pointer(uintptr)` produced a non-nil box wrapping 0, and
`(uintptr)ptr` dereferenced a nil-constructed box instead of yielding 0. The symptom was silent and
far away — sync's `poolDequeue` reads each ring slot's `typ` word to decide whether it is free, every
empty slot came back "occupied", `pushHead` returned false forever and `TestPoolDequeue`/
`TestPoolChain` spun. `unsafe.cs` now marks the zero address nil (a protected `ж<T>(value, isNull)`
constructor holds the address *and* the nil flag) and tolerates a nil box on the way out.

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

**The test host treats an escaping `GoexitException` as Go's `tRunner` does**, and each test's
dedicated thread is marked a goroutine (`Goroutine.Enter`) because in Go a test body IS one. Go's
`FailNow` is *specified* as "mark failed, then `runtime.Goexit`", so a Goexit escaping a test body
means the test ended without completing — Go reports `errNilPanicOrGoexit` ("test executed panic(nil)
or runtime.Goexit") against that test. The host logs exactly that text and fails the one test, where
Go additionally panics the whole binary; keeping the run alive leaves the rest of the package
measurable. This is also the path `testing.T.FailNow` fidelity will take.

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

**The unhandled-exception backstop prints the whole exception chain for a NON-panic failure.** A real
panic keeps Go's report shape (`panic: <value>` on stderr, exit 2). Anything else is a *defect to
diagnose*, and `ex.Message` alone threw the evidence away: a `TypeInitializationException`'s own
message merely names the type and says "see inner exception", so the actual fault and its stack were
lost (a whole `gob` run's real cause was invisible this way). The backstop now writes `ex.ToString()`
for the non-panic case, carrying the full inner-exception chain and stacks.

## Stop-the-world: the CONTRACT model, and the regions that run without Ps

Go's `stopTheWorld` takes `worldsema` and then stops every P (preempt them all, retake those in
syscalls, wait for `sched.stopwait`); `startTheWorld` restarts them, and the pair records the pause
into `sched`'s four `/sched/pauses` histograms. A dozen runtime entry points stop the world: `GC`,
`GOMAXPROCS`, `ReadMemStats`, `Stack(all)`, `GoroutineProfile`, `debug.WriteHeapDump`,
`trace.Start`, `syscall.AllThreadsSyscall`, and the test exports behind `ReadMemStatsSlow`,
`ReadMetricsSlow`, `CountPagesInUse` and the debug log. There are no Ps here (`m.p` is nil by
construction), so the converted stop died on its first P while holding `worldsema`, and every later
caller parked on the leaked permit.

The owner ruled the **contract model plus managed region bodies** (2026-09-28). `stopTheWorld` takes
`worldsema` and records a stopping sample; `startTheWorld` records the total sample and releases
`worldsema` with handoff. **Other goroutines are NOT suspended** — the CLR cannot do that safely, and a
real goroutine barrier was ruled too invasive — so a caller that reads state it expects the world to
hold still reads it live. What the pair keeps is Go's mutual exclusion among stoppers and Go's pause
accounting, which is what `TestSchedPauseMetrics` measures. The stop itself is the acquisition, so the
stopping time is what taking `worldsema` took, and `stoppingCPUTime` is 0. The samples are recorded
through cells boxed once per histogram, so `ReadMemStats` stays allocation-free.

Every stopped-world region the runtime reaches is either **managed** or **refuses by name BEFORE the
world is stopped** — never inside it:

| Region | Realization |
|---|---|
| `flushallmcaches` | a no-op: there are no Ps, so no mcaches to flush |
| the debug log (`dlogImpl`, `printDebugLogImpl`, `printDebugLogPC`) | managed memory in place of `sysAllocOS` reinterprets; the prepend-only logger list is a CAS on its reference slot; a PC is symbolized through the caller records `runtime.Caller` and `FuncForPC` read (`runtime/debuglog_impl.cs`) |
| `readMetricsLocked` | a REVERSED crossing: its caller hands it the raw address of a `[]runtime/metrics.Sample`, a type runtime cannot name, so `runtime/metrics` registers the piece it lacks at module initialization and the values land in the caller's own samples |
| `GoroutineProfile` | the COUNT path is kept, pair and all (`(n, false)` when the slice is too short); the FILL path needs every goroutine's stack and refuses by name before any semaphore |
| `debug.WriteHeapDump` | the pair and a well-formed **minimal** dump: the header, the params record, the EOF tag, and no objects (below) |
| `syscall.AllThreadsSyscall` (linux) | refused before the world: there are no Ms to signal, so a successful stop would run the call on one thread and report success |
| `trace.Start` | golib's managed execution tracer starts inside the pair on every target (`<goos>/trace_impl.cs`); a start refused because a trace is running is refused before the world, as Go's is |

**The heap dump is truthful and empty rather than invented.** Go's dump walks its own heap arenas,
spans and goroutine stacks; the managed model has none of them, since the CLR owns the heap. The
params record says what this host can say: pointer byte order and size, no Go arena (`0, 0`),
`GOARCH`, the Go release the corpus was converted from, and `ncpu`. A reader of the Go dump format
sees a valid dump of a heap with nothing in it.

**The leak class has a safety net.** A region that throws while holding `worldsema` or `metricsSema`
would leave it held for every later caller; Go fatals there ("panic during preemptoff"). golib's
`RuntimeErrorPanic.PanicObserved` runs on the throwing goroutine whenever a Go frame's panic filter
examines an exception, before the frames between the throw and the handler unwind, and the runtime
registers the release of both locks there, keyed on the recorded holder. `startTheWorld` and
`metricsUnlock` release only by compare-and-swap on that holder, so a lock is never released twice.

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

### Package initialization's frames are `init`, `init.funcN` and `init.N`

Go names the frames of package initialization by role, not by the C# member that carries them, and
`goFrameName` (the one site every reader goes through: the traceback and its `created by` line,
`runtime.Callers` / `CallersFrames` / `FuncForPC().Name()`, a func value's name) maps them:

| Go source | Emitted as | Frame name (Go 1.24.13) |
|---|---|---|
| a package-level var initializer | the package class's static constructor (`.cctor`) | `pkg.init` |
| a func literal inside one | a lambda in that static constructor | `pkg.init.func1`, `pkg.init.func2`, … |
| the package's `init` functions | `[GoInit]` methods `init`, `initΔ1`, `initΔ2`, … | `pkg.init.0`, `pkg.init.1`, `pkg.init.2`, … |
| a func literal inside an `init` function | a lambda in that method | `pkg.init.2.func1` |

Go numbers `init` functions in the order its compiler sees them — files in filename order, then
declaration order — and the converter's `Δ` counter follows that same order (measured on a three-file
probe), so `initΔK` → `init.K` is exact. Only a module-initializer method is renamed: a Go method or
helper spelled `init` keeps its name, and the converter's import hooks (`initᴛᴛimport…`) never match.

**Limit, stated rather than guarded.** A literal in a var initializer has no recorded counter (it is
outside every function declaration), so its `N` is Roslyn's 0-based lambda ordinal within the static
constructor, plus one. That is exact when a package's var initializers sit in one file; where they span
files, the order Roslyn merges them into the one static constructor and Go's package-wide counter may
differ.

**Guards.** `InitFrameNameTests` (GolibTests) pins each shape against hand-written stand-ins, with a
plain method named `init` as the control; the `InitFrameNames` behavioral project compares a three-file
program's frame names with `go run`.

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

The class's bar and its standing measurements live with the roster's disclosure classes in
[Validated Test Packages](../ValidatedTestPackages.md), key figures visible and derivations in the
provenance comments beside them. The 2026-08-30 tier-0 A/B found the first point above disappears under
a Release publish with `DOTNET_TieredCompilation=0`, and that configuration has been the default since
2026-09-02, so no row carries `execution: release-tc0` (`internal/weak` was the first to need it). `sync`'s
three `TestOnceXGC` subtest pins, and how they count toward its Disclosed column, are recorded in
[DATA-alloc-pins-rand-sync](../phase4/DATA-alloc-pins-rand-sync.md);
[CENSUS-type-name-erasure](../phase4/CENSUS-type-name-erasure.md) §8 derives how `unique`'s GC rows move
between matched and `codegen-liveness` once its subtest names match Go's.

## `host-limit` — the third disclosed-divergence class: what the test HOST cannot BE

`alloc-profile` and `codegen-liveness` both name something the managed runtime cannot **measure**.
`host-limit` names something the converted test binary cannot **be** — a property of the deployment
shape rather than of an assertion. Ruled 2026-08-15 and first pinned by `os/exec`, whose 25 leaf rows
under `TestCommand` and `TestLookPathWindows` carried it (their 2 parents rode the disclosed-parent
aggregation rule, carrying no failure text of their own).

**The founding capability, retired 2026-08-27: a relocatable single-file test executable.** Go's
test binary is statically linked, so a test may copy it and run the copy — `os/exec`'s `installExe`
does exactly that, and both of its `LookPath`/`Command` fixture tables are built on it. A converted
test host was then an **apphost**: a stub bound at build time to a managed assembly of the same base
name that must sit beside it. Copy the one file and hostfxr answered `LibHostAppRootFindFailure` —
`exit status 0x8000809a`, *"The application to execute does not exist"* — which was byte for byte
what the tests reported, and was the pinned signature. Satisfying it meant publishing every
converted test host self-contained single-file (~70 MB and a publish rather than a build, per
package). The `-tests` host has published that way since 2026-08-27, so the copy runs, the 27
`os/exec` verdicts pass, and those entries were removed.

**The live entries** name other properties of the shape, and not every one retires through a shape
change. They are in `crypto/tls` (`TestBogoSuite`: BoringSSL's runner spawns the host once per case
inside Go's own 10-minute test-binary wall, and the run is bound by throughput rather than
start-up), `os` (`TestRemoveAllNoFcntl`, linux: a CLR-hosted child's own start-up `fcntl` calls
exceed the test's budget), `os/exec` (`TestCredentialNoSetGroups`) and `syscall` (`TestExecPtrace`),
where `posix_spawn` cannot express the credential or ptrace step, and `syscall` again
(`TestPrlimitFileLimit`, linux: the managed runtime's open-descriptor floor, which no named change
lowers). The roster's `host-limit` class lists each with its measurement and what retires it.

**The bar, and why the class stays narrow.** A `host-limit` entry must pin a **structural** property
of the current deployment shape — provable from how the artifact is built, not from how far an
implementation has got — and never an unimplemented-but-fixable defect. The distinction is the same
one that kept `log` unbanked rather than disclosed while it waited: `log`'s `TestAll` wants a `.go:63`
position, and a Go-source position map satisfied it exactly, so it was a deferred capability and no
disclosure (`log` has since banked on the position map).
Nothing about an apphost's binding is deferred work of that kind; it is what the artifact *is*.

**Why a disclosure rather than a capability gate**, when `unsupportedRuntimeCapabilities` then named
this very capability for `os_test.TestRemoveAllWithExecutedProcess`. The gate arm was built and
measured on `os/exec` before the ruling, and it fails on three counts recorded in
[BOARD-next-validation-candidates](../phase4/BOARD-next-validation-candidates.md): a gate keys on the
DECLARATION, so it withdrew 40 verdict rows where only 27 were failing, destroying 13 live agreeing
passes; it is self-defeating against a `TestMain` that asserts the whole suite ran, because greening
the suite is what arms that census (`helper command unused: "printpath"`, host exit 1, package
validates at no count); and it hides the very rows whose future passing is the signal the limit has
been lifted. A disclosure keeps every row running, visible and compared, so each entry **retires
itself**: once the shape gains its property the rows start passing, the disclosed count stops
matching, and the sweep fails until the entries are removed, which is how the founding entry left
on 2026-08-27, when the host began publishing single-file and `os/exec`'s rows passed. `os`'s gate
entry predated the ruling and outlived the capability, withdrawing `TestRemoveAllWithExecutedProcess`
from both runs; it retired with the withdrawal gate change (see
`TestNoEntryWithdrawsATestOnTheSingleFileHostCapability`), and the test now runs and reports a
verdict like any other row.

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

## `reflect.ArrayOf` composes a descriptor; it does not reconstruct a linker record

`ArrayOf(n, elem)` builds an array TYPE at run time, for a type no declaration in the program
produced. Go's own body cannot be converted usefully, and it does not degrade: before it assembles
its `arrayType` record (`Str`/`Hash`/`GCData`/`PtrBytes`/`Equal`, plus a `SliceOf` for the record's
`Slice` field) it looks the type up **by name** through `typesByString` → `typelinks()`, the
linker-built type table, which has no managed form and is a `NotImplementedException` stub. So every
call threw whatever it was asked for — `encoding/gob`'s `TestIgnoreDepthLimit` reports it as an
`infrastructure-error` rather than a failure — and the throw says nothing about the request: it is
the reconstruction of a **linker** record, which the managed bridge never needs.

golib's `array<T>` **is** the array type. The one part of a Go array type the managed emission
cannot hold is its LENGTH (C# has no const generic parameter for the `4` in `[4]byte`), and that is
exactly what the descriptor's **dims cargo** already carries for every declared array. So the whole
construction is the `(managed type, dims)` pair `abi.TypeOf` reaches from a live `[n]T` value:

```csharp
// reflect/value_impl.cs — the hand-own, beside its sibling constructor PointerTo
public static ΔType ArrayOf(nint length, ΔType elem) {
    if (length < 0) {
        throw panic("reflect: negative length passed to ArrayOf");
    }
    System.Type? st = sysTypeOfReflectType(elem);
    ...
    nint[]? elemDims = arrayDimsOfReflectType(elem);
    nint[] dims = new nint[1 + (elemDims is null ? 0 : elemDims.Length)];
    dims[0] = length;
    elemDims?.CopyTo(dims, 1);
    ...
    return toType(abi.synthType(typeof(array<>).MakeGenericType(st), dims));
}
```

**Interning is what makes it a round trip rather than a look-alike.** `canonType` keys the `ΔType`
wrapper on the managed type PLUS the dims rendering, so `ArrayOf(3, TypeOf(byte))` and
`TypeOf([3]byte{})` are the SAME canonical `reflect.Type` **by identity** — and `Len`/`Elem`/`Size`/
`Align`/`String`/`New`/`Zero` then agree because they read one descriptor, not because each was
separately made to agree.

**The dims COMPOSE**, and that is not a nested-array special case. The slot means *what `Elem()`
hands down*: an array consumes the head and passes the tail, while a pointer's and a map's dims pass
through unshifted. So `[n][3]byte` and `[n]*[3]int` are both spelled `[n, 3]`, and each accessor
takes back its own share. Repeated composition is therefore free, which is the shape `gob`'s
depth-limit test builds 101 deep.

What an array has **no slot** to hand down is a channel's DIRECTION or a map KEY's dims:
`abi.Type.Elem` descends those through a POINTER only, so `[n]chan<- T` describes `[n]chan T` here.
That is the cargo model's shape rather than this function's — a DECLARED `[n]chan<- T` reads back
exactly the same way — so it is recorded, not worked around (the r39d rule).

Guarded by the **`ReflectArrayOf`** behavioral test, whose every row is that identity claim. The
registry entry is `manualConversionFuncs["reflect"]["ArrayOf"]` (`manualTypeOperations.go`), which is
what turns the auto body into the placeholder the hand-own fills. Implementing `ArrayOf` alone does
**not** flip `gob`'s last verdict — `TestIgnoreDepthLimit` wraps its 101-deep array in a
`reflect.StructOf`, which is runtime struct synthesis over `System.Reflection.Emit` and a feature arc
of its own.

⚠ The guard's nested rows compare against a declared **variable**, not an empty composite literal,
and the difference is load-bearing. `var x [2][3]uint8` emits as `new(2, () => new(3))` — the inner
dimension is in the initializer, which is the source the bridge recovers a declared array's length
from. The empty literal `[2][3]uint8{}` emits as `new array<uint8>[]{}.array(2)`, whose two elements
are `default(array<uint8>)`, i.e. length ZERO — so the inner dimension is dropped and
`reflect.TypeOf(lit).Elem().Len()` answers `0` where Go answers `3`. That is a converter EMISSION
gap, older than and independent of this hand-own (it needs no reflection to reach), and it is
recorded here rather than papered over.

## `reflect.StructOf` MINTS a CLR value type, and then changes nothing else

`StructOf(fields)` is `ArrayOf`'s sibling one order of magnitude up. `PointerTo` and `ArrayOf` hand
`MakeGenericType` an **existing** managed type, because `ж<T>` and `array<T>` *are* the Go type; a
struct has no generic container to instantiate, so `StructOf` is the one caller that asks for a Go
type nothing declared and a real CLR **value type has to be minted** for it — with
`System.Reflection.Emit`, in golib's `GoStructSynthesis`.

The auto body dies where `ArrayOf`'s does, and one stub earlier than expected: measured, the first
throw is `addReflectOff` (from `runtimeStructField` → `resolveReflectType`), not `typelinks`. That is
the point rather than a detail — **everything past the validation loop is Go's runtime reconstructing
linker output** (`structTypeFixedN` prototypes, GC-program construction, `resolveReflectName` into
the linker's name blob, `unsafe_New`), so which stub is reached first is incidental. Both of
`reflect`'s *own* callers of `StructOf` are themselves such reconstructions — a fake struct
describing a func's argument frame (`initFuncTypes`), and `struct{S structType; U uncommonType; M
[n]Method}` to obtain an rtype followed in memory by a method array — so a hand-own owes them
nothing.

**What makes the mechanism honest is that nothing downstream is new.** Once the CLR type exists,
`abi.synthType` describes it exactly as it describes a converted struct, and `GoFields`,
`structLayoutOf`/`GoFieldOffsets`, `structFieldOf`, `FieldAliasBox`, `ZeroValueOf`,
`haveIdenticalUnderlyingType`, `GoTypeName` and `canonType` all run **unmodified** — not one of them
asks where a `System.Type` came from. A descriptor-only synthetic type would instead have grown a
second path in about ten places, and a green row would then prove the second path rather than the
bridge.

The mint carries five things, and each answers exactly one downstream reader:

| Emitted | Read by | Why it cannot be dropped |
|:--|:--|:--|
| `[GoType("dyn")]` on the type | `HasGoName`, `GoTypeName` | a `StructOf` result is a Go ANONYMOUS struct: `Name()` must be `""` and `String()` must render structurally |
| a **parameterless constructor** seeding every array-kinded field | `GoReflect.FieldArrayDims` | an array field's Go LENGTH |
| `[GoTag("…")]` on a field | `goTagOf` | `StructField.Tag` |
| `[GoArrayDims]` / `[GoMapKeyDims]` on a field | `FieldStampedDims` / `FieldMapKeyDims` | the pointer-hop and map hops |
| a `ʗ`-prefixed CLR field name | `collectGoFields` | `StructField.Anonymous` |

**The constructor is the piece that is easy to get backwards, so it is worth stating flatly.**
`collectGoFields` reads an *array* field's dims from a cached **zero instance** —
`Activator.CreateInstance(declaringType)` — because in converted code the converter emits the length
as a field **initializer** (`= new(4)`) that the generated parameterless constructor runs. The
`[GoArrayDims]` stamp is *not* that route: it exists for the pointer and map-element hops, where a
zero instance holds a nil pointer or an empty map and has nothing to measure. A `TypeBuilder` struct
has no field initializers, so **without an emitted constructor every synthesized array field would
report length 0** — silently, `0` being a legal Go length, and mis-sizing the struct as well, since
`structLayoutOf` sizes an array field from the same vector. Both routes are therefore emitted; they
cover disjoint cases, under exactly the rule `fieldCargoDims` applies to declared fields.

**Interning is the contract, not an optimization.** `encoding/gob` keys
`map[reflect.Type]gobType` and `enc.sent map[reflect.Type]typeId` on the result, so a fresh
descriptor per call makes every recursion a cache miss and every mutually recursive type an infinite
regress. Two properties of the shape key are load-bearing:

- it cannot be built from `System.Type`s. `[1]int` and `[2]int` are ONE `array<nint>` and
  `chan<- T` and `chan T` are ONE `channel<T>` — length and direction live only as descriptor cargo —
  so each field contributes its `abi.descriptorDimsKey` rendering, **reused rather than restated**,
  so a shape key and the descriptor it stands for cannot separate the same two types differently;
- the intern holds the **mint**, under a lock rather than a `ConcurrentDictionary` factory.
  `GetOrAdd` runs its factory concurrently and discards the losers' work, but the work here is
  `DefineType`, and a duplicate type name **throws** (measured on a bare probe: 3 of 4 racing threads
  failed).

**`PkgPath` nests.** `GoPackagePath(t)` is `GoPackageClassPath(t.DeclaringType)`, which reads the
declaring class's namespace plus its name with `_package` trimmed — so a struct with an unexported
field is minted **inside** a synthesized container class `<pkg>_package` in namespace
`go.<parent-path>`: `go.encoding.gob_package` for `encoding/gob`. The obvious spelling
`go.encoding.gob.gob_package` is measurably wrong and yields `"encoding/gob/gob"`. The container is
minted only when a field actually carries a `PkgPath`, so the common all-exported case pays nothing.

**Narrowings this hand-own ships with, recorded so they are met knowingly:**

1. **Interning is `StructOf`-local.** A converter-lifted anonymous struct of the same shape is a
   different CLR type, so `StructOf(f) == TypeOf(struct{F int}{})` is `false` here and `true` in Go —
   the same class as the cross-context anonymous-lift identity split. `haveIdenticalUnderlyingType`
   still answers `true` for the pair, so `AssignableTo`, `ConvertibleTo`, `Convert` and assignment
   all behave; only `==` on the `Type` splits. The one shape exempt is `struct{}`, whose managed form
   golib already declares (`EmptyStruct` *is* Go's empty struct), so the degenerate call reaches the
   type a declaration produces.
2. **A directional-channel field keeps its identity but not its description.** The direction is in
   the shape key, so `struct{C chan<- T}` and `struct{C chan T}` are distinct types; the minted field
   is a plain `channel<T>`, so `Field(i).Type.ChanDir()` answers bidirectional. Carrying it is one
   more seeded field in a constructor that already exists.
3. **Embedded fields with methods panic**, matching Go's own documented gap (*"StructOf currently
   does not support promoted methods of embedded fields"*) — and using Go's exact message wherever
   Go's own condition matches.
4. **The type's own `PkgPath()` and its `StructField.PkgPath` are answered by different rules.**
   `structFieldOf` uses `f.Exported ? "" : GoPackagePath(st)` and is exactly right; `rtype.PkgPath()`
   reads the same call with no `HasGoName` gate, so a synthesized struct that needed a container
   reports a package path for the TYPE where Go answers `""` for an unnamed type. That is
   pre-existing and corpus-wide (every converter-lifted anonymous struct behaves the same way), so it
   is pinned rather than changed here.

Guarded by the **`ReflectStructOf`** behavioral test — the only gate that checks the answer against
Go rather than against our own expectation — and by `GolibTests`' `GoStructSynthesisTests`, which
pins the three mechanisms that fail *silently*: the constructor, the shape key, and the `ʗ` prefix.
The last of those is asserted on `.Anonymous` and never on `Type.String()` on purpose: an embedded
field and a same-named regular field render **identically**, so a `String()`-based check could not go
red on the defect it exists to catch. The registry entry is
`manualConversionFuncs["reflect"]["StructOf"]`.

## `reflect.SliceOf` is the same one-liner as `PointerTo`

`SliceOf(elem)` dies in the same `typesByString` → `typelinks()` lookup and needs nothing but the
generic instantiation `typeof(slice<>).MakeGenericType(elem)` — the `PointerTo` shape exactly. The
only decision it carries is what dims to hand the descriptor, and the answer is **none**: a declared
`[]T` descriptor carries `null`, because `abi.TypeOf` measures dims for an ARRAY value and a POINTER's
pointee only. Passing the element's dims through would break the identity that makes the constructed
and the declared type one `reflect.Type`, and would not help either — `rtype.Elem`'s non-pointer,
non-map arm consumes the head of the dims vector, so a one-element vector hands down nothing. So
`SliceOf(ArrayOf(3, byte))` describes `[][3]byte` with the element's length unknown, which is exactly
what `TypeOf([][3]byte{})` reads back today. That residual belongs to the cargo model — a slice type
has no dims slot — not to this constructor.

⚠ Two PRE-EXISTING residuals meet here and neither belongs to this constructor, so the guard
asserts identity and deliberately does not print the name. go2cs renders a dims-less `array<T>` as
`[]T` by design (`GoReflect.TypeNaming`: *"length is not carried on the managed type"*), so
`fmt.Println(reflect.TypeOf([][3]uint8{}))` already prints `[][]uint8` at master, and
`reflect.TypeOf([][3]uint8{}).Elem() == reflect.TypeOf([3]uint8{})` is already `false` — both with no
reflection constructor in sight. One root: a slice type has no dims slot, so nothing survives the
`Elem()` hop. Widening the cargo there is an arc of its own.

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

---

[← The standard-library conversion applies `-tags purego`](purego.md) · [Index](README.md) · [Comments →](comments.md)
