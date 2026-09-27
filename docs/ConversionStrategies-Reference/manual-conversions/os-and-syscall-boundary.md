# Manually-Converted Declarations: The OS and Syscall Boundary

[Reference index](../README.md) · [Manually-Converted Declarations](../manual-conversions.md) · [Summary of this topic](../../ConversionStrategies.md#manually-converted-declarations)

This page covers the hand-owned seam between Go's OS packages and the operating system: the netpollers, the native calls that pass a struct, and the raw syscall bottom.

## Pollers

### The managed netpoller — the ten `runtime_poll*` contracts on .NET's completion machinery

**The fifth and deepest application of the managed-API-boundary pattern, and the one the sockaddr entry above predicted.** `internal/poll` declares **ten** bodyless `//go:linkname` entry points into the runtime's network poller (`fd_poll_runtime.cs:18–36`). The converter emits each as a bodyless `partial`, the `PartialStubGenerator` fills them with throwing stubs, and the first pollable `FD.Init` — which is *every socket the `net` package creates* — died in `serverInit.Do(runtime_pollServerInit)`. `os` is unaffected and always was: it passes `pollable: false` for every file, pipe and console, so `runtimeCtx == 0` short-circuits every pd call.

**Why the counterparts could not simply be wired.** They exist — `runtime/netpoll.cs:217` carries `poll_runtime_pollServerInit` with its linkname comment intact, and all nine others sit beside it — but the SHALLOW wall (`netpollinit` → `stdcall4(_CreateIoCompletionPort, …)` → `asmstdcall`, a stub) is not the real one. Behind it the bodies consume runtime mutexes with lock-rank bookkeeping, `pollcache` over `persistentalloc`, the runtime timer engine, `gopark` with a commit callback, `goready`, and g-pointer CAS protocols — every one an organ of the Go scheduler. The decisive fact is that Go's poller is only **half** an API: the other half (`netpoll(delta)`, `netpollBreak`, `netpollready`) is called by the scheduler itself from `findrunnable` and sysmon. Under go2cs nothing would ever pump it, so a perfectly-wired conversion would initialize an IOCP and then **block forever** — the thread Go dedicates to draining it IS the scheduler. The ten-contract boundary is the only cut through this subsystem that does not drag a scheduler across; `runtime/netpoll.cs` and `runtime/windows/netpoll_windows.cs` stay converted and **dead**, with zero runtime edits.

**The shape.** `internal/poll/windows/runtime_netpoll_impl.cs` (per-GOOS folder, riding the existing `$(GoTargetOS)/*.cs` glob — **no csproj change**) supplies the ten bodies over a `ManagedPollDesc`: one Monitor, a sticky `closing`, and per mode a `ready` flag, a sticky `expired` flag, a generation counter and a `System.Threading.Timer`. Four decisions are worth cribbing:

- **One Monitor, not Go's lock-free `rg`/`wg` CAS protocol.** Go splits `pollDesc` state across atomics because `netpollcheckerr` runs where the pd lock cannot be taken and `netpollblock` parks through `gopark`'s publication protocol. Neither constraint survives the boundary — every managed caller is an ordinary blocked thread — and the single-waiter-per-mode contract (*"pollDesc can hold only a single waiting goroutine for each mode"*, enforced above by `fdMutex`) bounds contention to one reader + one writer + one timer callback. The CAS choreography would be emulation of a mechanism whose reason evaporated, the same reduction `runtime_sema_impl.cs` made for the runtime semaphore.
- **Completion delivery is `ThreadPoolBoundHandle` — the CLR's own IOCP**, so there is no poller thread of ours, no shutdown story, and no raw-address routing table. `isPollServerDescriptor` therefore answers **false** for every fd: there is no exposed poll-server descriptor, and its only consumer is the test-only `IsPollDescriptor`. The association is made **lazily, at the first submit** — not at `pollOpen` — and that placement is load-bearing rather than incidental. Go's poller *rejects* completions it does not own (`pollOperationFromOverlappedEntry` compares the completion key against the `pollDesc` recorded in the operation and drops a mismatch, go.dev/issue/58870); `ThreadPoolBoundHandle` has no equivalent, because its callback resolves state *from* the `NativeOverlapped` it allocated, so a foreign overlapped is **misread rather than ignored**. Binding at open would make every pollable socket eligible for that, and `internal/poll` has a live producer: `FD.WSAIoctl` bypasses `execIO` and hands the kernel the *caller's* `syscall.Overlapped` — which, being an all-scalar struct in a standard box, genuinely pins and genuinely reaches the kernel. Binding at first submit removes the hazard structurally: a socket that only ever sees foreign overlapped IO is never associated at all. (Both production `FD.WSAIoctl` callers pass a `nil` overlapped, so no corpus path mixes the two on one socket.)
- **The load-bearing separation is wake-vs-readiness.** A completion sets `ready` and pulses; a deadline firing and `pollUnblock` pulse **without** setting it. Go encodes this as `pdReady`-vs-`pdNil`-wake, and it is what makes `waitCanceled`'s ignore-errors loop correct — that loop waits for a COMPLETION specifically, ignoring the very timeout that woke its caller. Readiness is also consumed *before* either error check, so a completion that raced the deadline is still delivered.
- **Deadlines are `Timer` + sticky flags + generations, not `CancellationToken`.** A CTS models a one-shot cancellation of a linked operation; a Go deadline is per-MODE, STICKY across future operations until re-set, REPLACEABLE while an op is in flight, and NON-ABANDONING (the timed-out op must still be cancelled *and harvested* by the same caller). Modeling all four with tokens reconstructs the flags+timer state machine anyway. The generation check is **not** optional hardening: `Timer.Change`/`Dispose` do not synchronize with an in-flight callback, so without it a cleared deadline can be expired by the callback of the deadline it replaced.

**Landed in stages.** S1 covers the listener lifecycle — contracts 1 (`pollServerInit`), 2 (`pollOpen`), 8 (`pollUnblock`), 3 (`pollClose`), plus a `pollSetDeadline` smoke — with **zero data flow**, guarded by the `NetListenSmoke` behavioral **output** test, which prints kernel-derived values rather than checking for absence of a fault (an ephemeral port was assigned, two live listeners differ, and, the strongest line, the port a closed listener released can be re-bound — only true if `pollClose` released the registration before `internal/poll` closed the socket; port numbers themselves are never printed, so the output is host-independent). S2 is the overlapped SUBMIT seam, below. Full design, and the eight ruled open questions: [`docs/phase4/DESIGN-netpoll-managed-poller.md`](../../phase4/DESIGN-netpoll-managed-poller.md).

### The overlapped SUBMIT seam — a per-operation record owning native lifetime, and a golib rendezvous

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

### The Linux flavor's poller — the fallback first, then the readiness poller: epoll, one drain thread, and the Windows descriptor state machine

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

## Struct-passing native calls

### The Linux `struct stat` mirror and the NoError raw bottom — the first Linux members of the struct-passing class

**The two seams the poll-seam measurement found standing directly behind the poller (R1 and R4 of the 2026-08-22 board entry), both in the Linux `syscall` flavor, both the shape of a class the Windows lane had already named.**

**R1 — `syscall.Stat_t` passed by address.** On linux/amd64 Go's `Stat_t` *is* the kernel's 144-byte `struct stat`, field for field, with `X__unused [3]int64` inline at the end, so the generated `Fstat`/`fstatat` wrappers pass `uintptr(unsafe.Pointer(stat))` and the kernel writes straight into it. The converted `Stat_t` cannot be that: the trailing array is a golib `array<int64>` MANAGED REFERENCE, so the struct is not blittable, the CLR lays it out itself (~128 bytes, its own field order), and `(uintptr)Ꮡstat` — golib pinning the box's value slot — hands the kernel that managed image. `fstat(2)`/`fstatat(2)` then write 144 bytes of native record over a field order that is not the kernel's **and 16 bytes past the object**. Nothing faults; it answers. Measured in an isolated probe on the Linux flavor: `os.Stat(dir)` returned `err == nil`, `IsDir() == false`, `Mode() == p---------` for a real directory, `Stat().Size()` read 0 for a 3,302-byte file, while `Readdirnames`/`ReadDir`/`Read` (dirent-typed, no stat) were correct — so every `filepath.Glob` answered nothing (glob swallows the I/O error and tests `IsDir`), every `Walk`/`WalkDir` visited only its root, `archive/zip` read "not a valid zip file" from a mis-sized archive, `MkdirAll` said "not a directory": eight roster rows wall-to-wall plus partials, the quiet-wrong-answer shape the struct-passing entry on the board calls the class's worst. The remedy is that class's established one, now on Linux: `syscall/linux/zsyscall_linux_amd64_impl.cs` hand-owns **`Fstat` and `fstatat`** over a `[StructLayout(LayoutKind.Sequential)]` `NativeStatLinuxAmd64` mirror (the reserved words `fixed int64[3]` inline), hands the keystone libc `syscall(2)` binding the mirror's address, and copies field-for-field into the converted `Stat_t` on success — errors and the nil-pointer `EFAULT` follow the Go original, and the path argument keeps the generated wrapper's own `BytePtrFromString` + byte-box pinning verbatim, because that part was never the defect. The two wrappers are displaced from the generated file by `manualConversionFuncs` under a NEW **`goosLinux`** scope: darwin declares both names too, with libc-backed bodies that are not defective and must keep them, and Windows declares neither — `TestLinuxOnlyEntriesAreScopedToLinux` is the mirror of the Windows-only guard. `Stat` and `Lstat` are NOT hand-owned (on linux/amd64 they are pure Go over `fstatat` and inherit the fix), and the mirror is deliberately linux/amd64's layout alone (another Linux GOARCH's `Stat_t` differs and would want its own `zsyscall_<arch>_impl.cs`, stated rather than generalized); `Statfs_t`/`Sysinfo_t`/`Utsname` are the same class and are taken when a suite reaches them, per the class doctrine.

**R4 — `rawSyscallNoError` was still an announcing stub.** Go declares it in `syscall_linux.go` and implements it in `asm_linux_amd64.s` as a bare `SYSCALL; MOVQ AX, r1; MOVQ DX, r2` with no errno handling, for the generated "NoError" family that cannot fail — `Getpid`, `Getppid`, `Gettid`, `Getuid`, `Geteuid`, `Getgid`, `Getegid`, `Umask` — and for `forkExec`'s own getpid/getppid. The Linux keystone hand-own had deliberately left it an announcing stub "until something genuinely needs them"; the roster re-run showed what the stub costs once the poll seam is open: `os.Getuid` → `NotImplementedException` at the top of `os/user.Current` (poisoning its `sync.Once`, so every later call NREs — `archive/tar`'s whole uid→name path), `time.interrupt`'s `Kill(Getpid())` in `TestSleep`, and `os/exec`'s test host dying in init before a single verdict. The body, beside `runtime_entersyscall` in `syscall_linux_impl.cs`, is the keystone binding once more with the errno word dropped — which is exactly what the asm returns, and not a swallowed error: the callers are syscalls that cannot fail.

Guarded by the `StatLayoutTruth` behavioral output test (a temp tree: `Stat`/`Lstat`/`File.Stat` type bits, sizes and a sane `ModTime`; `Glob`, `ReadDir` with `Info()`, `WalkDir` counts; `Getpid`/`Getppid`/`Getuid`/`Getgid` as callable predicates — every line a boolean or a count, so Windows and Linux print the same bytes).

**Measured (2026-08-22, the Linux roster re-run against the poll-seam lane's 122/161 baseline; Windows control on the i9 39/39 green):** every one of the 13 rows the two classes had been attributed to moved exactly as predicted — **7 flip to PASS at their banked counts** (`archive/zip`, `debug/dwarf`, `html/template`, `io/ioutil`, `io/fs`, `internal/diff`, `archive/tar`), **`path/filepath` validates at its Linux count** (54 of 54 matching; the banked 61 is Windows-shaped), and the other **5 are improved to a residual of a different class** — `time` 156/157 (the ZONEINFO caching test), `go/doc/comment` 10,058/10,059, `text/template` 51/52, `go/internal/srcimporter` 4/7 and `os/exec` now running 16/72 (the package-level death is gone), each remaining test on the exec wall. The board entry of 2026-08-22 ("the three bodies") carries the roster arithmetic.

### `Uname` — the struct-passing class taken to its limit, and the deferral rule it exposed

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

### The sockaddr family on Linux — L10's mirror, arm for arm, as the socket poller's prerequisite; and `Mmap`'s slice is a snapshot

**The sockaddr mirror.** The Linux flavor has the same two defects the Windows lane's L10 entry (above, *The SOCKET-ADDRESS family*) retired: `syscall_linux.go` writes the port through `(*[2]byte)(unsafe.Pointer(&sa.raw.Port))` — which converts to a length-zero `array<byte>` and panics `index out of range [0] with length 0` in `SockaddrInet4.sockaddr` — and `Bind`/`Connect` hand the kernel `unsafe.Pointer(&sa.raw)` while `accept4`/`getsockname`/`getpeername` hand it `&rsa` to fill, neither of which has a native layout (`RawSockaddrInet4.Addr/Zero`, `RawSockaddrAny.Data/Pad` are golib `array` references). Measured on the 2026-08-22 Linux roster re-run as R5: `encoding/json`'s `TestHTTPDecoding` and `crypto/tls`'s `TestMain` both died in the encoder before any socket call. `syscall/linux/sockaddr_linux_impl.cs` mirrors L10 exactly: the two INET encoders write the port arithmetically; `writeNativeSockaddr` builds the native `sockaddr_in`/`_in6`/`_un`/`_ll`/`_nl` image in a stack buffer (calling Go's own `sockaddr()` first, so there is ONE definition of what a Sockaddr means and the hand-own translates layout alone); `Bind`/`Connect` hand that address to the package's own generated address-taking `bind`/`connect`; `Getsockname`/`Getpeername`/`Accept4` go through the trampoline with a stack buffer — their generated wrappers take a typed `ж<RawSockaddrAny>` — and decode with `readNativeSockaddr`, Go's `anyToSockaddr` arm for arm over a native image; and `anyToSockaddr` itself becomes a flatten-then-decode of the managed struct (Family at 0, Data 2..15, Pad 16..111 = `SizeofSockaddrAny`), so any remaining auto caller decodes correctly once its own fill is. The registry carries the family under a new **`goosWindowsLinux`** scope for the names both flavors hand-own (the encoders, `Bind`/`Connect`/`Getsockname`/`Getpeername` — one entry, each flavor's per-GOOS file its own authority, the `lock_sema`/`lock_futex` precedent; darwin keeps its libc bodies) and `goosLinux` for `Accept4`/`anyToSockaddr`, guarded by `TestSockaddrFamilyIsScopedToEachHandOwningFlavor`. Not covered, named: `Recvfrom`/`Sendto`/`Recvmsg`/`Sendmsg` (UDP and ancillary), exactly L10's line.

**What it buys, stated honestly: the wall moves, the gate does not open.** A Linux socket is un-armable — the poller fallback above answers `EPERM` from `runtime_pollOpen` for every descriptor — so once `Bind`/`Connect` succeed, `net`'s `listenStream`/`dial` reach `FD.Init` → `pollDesc.init` and return `operation not permitted`: an honest error until a Linux readiness poller exists (the netpoll design's §8 non-goal, a separate design). The mirror is that poller's prerequisite, not a row flip, and the board entry of 2026-08-22 measures it as such.

**`Mmap`'s slice is a snapshot (W1b, rooted, not fixed here).** `syscall.Mmap` on Linux returns `unsafe.Slice((*byte)(unsafe.Pointer(addr)), length)`, and golib's `unsafe.Slice` over a NATIVE pointer SNAPSHOTS the bytes into a managed slice (stated in `unsafe.cs`: "writes through the resulting slice do not reach the native memory"). Probe, converted program: `Mmap(0, 0, 3·pagesize, …)` → a 12,288-byte slice, err nil; `Mprotect(b[:pagesize], PROT_NONE)` → `invalid argument` (the kernel was handed a managed element address); `Munmap(b)` → `invalid argument`; writes land in the copy. That is the whole of `crypto/sha1`'s `TestOutOfBoundsRead` and `bytes`' four `*NearPageBoundary` tests on Linux. The honest remedy is a native-backed `slice<T>` in golib — a `MemoryManager<T>` over the mapping, with `Ꮡ(b, i)` and the `uintptr` conversion yielding the native address and the mapping owning the lifetime — a slice-model change, not a per-GOOS hand-own, routed rather than taken in the sockaddr lane.

### The CryptoAPI chain seam — when a native record must be READ as a struct and HANDED BACK as a pointer

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
[conversion fork](../../../src/archived/Baseline-vs-FullConversion.md): the declaration is hand-owned.

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

## The raw syscall bottom

### The Linux syscall bottom — ONE libc P/Invoke, and why `r2` is exact rather than approximate

Go reaches the Linux kernel through a single assembly function. `internal/runtime/syscall/asm_linux_amd64.s` loads the call number into `RAX` and `a1..a6` into `RDI, RSI, RDX, R10, R8, R9`, executes `SYSCALL`, and reports `RAX` and `RDX`:

```go
// internal/runtime/syscall/syscall_linux.go — no body, no linkname, no Go anywhere
func Syscall6(num, a1, a2, a3, a4, a5, a6 uintptr) (r1, r2, errno uintptr)
```

Everything funnels through it: `syscall`'s `RawSyscall`/`RawSyscall6`/`Syscall`/`Syscall6`, every generated wrapper in `zsyscall_linux_amd64.cs` (open, read, write, close, stat, getrlimit, …), and this package's own `EpollCreate1`/`EpollWait`/`EpollCtl`/`Eventfd`, which is how `internal/poll` and the netpoller reach the kernel. Converted, it is a bodyless partial, so the [`PartialStubGenerator`](../source-generators.md#source-generators) filled it with a throw — and because `syscall`'s own `init()` calls `Getrlimit(RLIMIT_NOFILE)` before `os` is usable, that one throw stopped every Linux program before `fmt.Println` could emit a byte.

The hand-own (`core/internal/runtime/syscall/linux/syscall_linux_impl.cs`) binds **glibc's `syscall(2)`** rather than reproducing the instruction — a user ruling, taken over the alternative of mapping each wrapper onto a .NET API. One P/Invoke lights the whole generated surface at once; the per-call alternative is N hand-owns, each independently guessing at semantics the kernel already defines exactly.

```csharp
[DllImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static extern nint libc_syscall(nint number, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6);
```

Three details separate a faithful binding from a plausible one. Each was **measured on linux/amd64** (glibc 2.35, .NET 9) rather than argued from the ABI documents, because each is exactly the kind of claim that reads as obviously true and is expensive when it is not:

* **The variadic.** C declares `long syscall(long number, ...)`; this declares seven fixed native ints. That is correct under SysV AMD64 — integer-class variadic arguments ride the same registers as fixed ones, with the seventh spilling to the stack, which is precisely where glibc's hand-written `syscall.S` reads `a6`. Proven with a real six-argument call: `mmap(NULL, 4096, PROT_READ|PROT_WRITE, MAP_PRIVATE|MAP_ANONYMOUS, -1, 0)` returned a live mapping that `munmap` then released. (`AL`, which a true variadic call sets to the vector-register count, is unused by `syscall.S`.)
* **`r2`.** Go's contract returns `RDX`, which libc's wrapper cannot hand back — the reason the [run-layer finding](../../phase4/FINDING-linux-run-layer.md) listed it as an open question. It does not need to: the Linux x86-64 convention clobbers only `RCX` and `R11`, so `RDX` still holds what entered the kernel, and the asm's `MOVQ DX, BX` observes `a3` unchanged. Returning `a3` is therefore not a stand-in for `r2` on this architecture — it *is* `r2`. Probed under the real Go runtime: `syscall.Syscall6(SYS_GETPID, …, a3=0xDEADBEEF, …)` returns `r2=0xdeadbeef`. The failure path zeroes `r2`, which the shim mirrors.
* **`errno`.** libc collapses the kernel's whole `[-4095, -1]` error band to a `-1` return and reports the positive errno out of band — the same number Go's asm produces by negating the raw return — and `SetLastError` lets the CLR capture it before managed code can perturb it. Probed: `openat(AT_FDCWD, NULL)` returns `-1` with `Marshal.GetLastPInvokeError() == 14` (EFAULT).

**One divergence is disclosed rather than papered over:** a syscall that legitimately returns `-1` as a *success* value is indistinguishable from a failure through libc, and would report a stale errno. Go's asm has no such ambiguity because it tests the raw return against the whole band. Nothing the converted corpus reaches behaves that way, and the only true fix is an instruction-level bottom the managed model cannot express.

**The pointer half needed nothing.** These wrappers pass addresses as `uintptr` — `Getrlimit` emits `RawSyscall(SYS_GETRLIMIT, (uintptr)resource, (uintptr)Ꮡrlim, 0)` — and golib's `ж<T>` → `uintptr` operator does not hand out a token: it calls `EnsureStableAddress()` to pin the managed storage and returns a real address, so the kernel genuinely reads and writes through it. The residual risk is per-struct **layout**, not addressing, and it is the same open class as the Windows non-blittable-wrapper census (see `zsyscall_windows_impl.cs`); `Rlimit` is two `uint64`s, so the first crosser was blittable and worked untouched.

**Portability, stated in the file for the arm64 increment:** `asm_linux_arm64.s` puts `a1..a6` in `X0..X5` with the number in `X8` and reads `r2` from `X1` — which holds `a2`, not `a3` — so an arm64 flavor must echo `a2` and must re-run the `r2` probe there rather than inherit this answer. The variadic shortcut is likewise a per-platform judgment (standard AAPCS64 passes variadic integer args in the same registers as named ones; Apple's arm64 ABI deliberately does not), and a musl target would likely need a `NativeLibrary.SetDllImportResolver` fallback.

The binding itself is `[LibraryImport]` rather than `[DllImport]` — the corpus-wide FFI convention, adopted for the whole surface at once; see [Every P/Invoke is source-generated](mechanism.md#every-pinvoke-is-source-generated) for what that buys and what it cost to reach.

---

[← Manually-Converted Declarations](../manual-conversions.md) · [Index](../README.md)
