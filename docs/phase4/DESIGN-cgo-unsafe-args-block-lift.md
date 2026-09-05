# DESIGN — the whole `cgo_unsafe_args` parameter block, lifted (Q56): darwin's `&first` libcCall shape placed by one rule

> **Status:** design, cut on the landed master `bc8973259` (2026-09-05, lane C2, dispatched by COORD eb3739373
> after the Q41 address-path correction 394548a64 / c6720db06). Nothing here is cut; the cut follows the ruling.
> **Companions:** `DESIGN-darwin-run-layer-2.md` §7 (the keystone dispatcher this design corrects),
> `runtime/darwin/libccall_impl.cs` (the dispatch bottom and the census in its header),
> `runtime/darwin/sigaction_impl.cs` (increment 6, the first remedy of the class), `DESIGN-signal-posix-bridge.md`
> (the linux `os/signal` bridge Q52 extends to darwin — it routes AROUND two of the sites below).

## 0. The defect, in one paragraph

Go reaches darwin's libc through per-function assembly trampolines. Fifty `libcCall` sites in the pinned
go1.23.12 `runtime/sys_darwin.go` hand a trampoline `unsafe.Pointer(&x)`; twelve pass a lifted `&args` struct —
the ONLY shape the keystone design modelled ("the layout lives in the caller's lifted args struct", §7.1) — three
pass `nil`, and **thirty-five pass `&<first parameter>`** under `//go:cgo_unsafe_args`, where `&sig` is the head of a
contiguous `(sig, new, old)` block that exists only on a Go stack and the trampoline unpacks the block BY OFFSET
(`sys_darwin_arm64.s:255-257`: `8(R0)` new, `16(R0)` old, `0(R0)` sig). The converted form boxes the first
parameter ALONE — `libcCall(fn, FromPinnedBox(Ꮡsig))` — so `GoLibcCall.DispatchArgsStruct` walks the pointee
TYPE's fields (a `uint32` has one), places ONE register, and `GoLibcCall.Call`'s one-argument arm leaves the second
and third registers as the caller-saved state left them. libc reads `new` from and writes `old` through STALE
REGISTERS. That is Q41's arm64 death (a 16-byte sigaction read-back written through a stale third register into
the managed stack's frame link; the walker died on it) and, on x64, the same dispatch surviving by luck. Increment 6
hand-owned `sigaction` at the seam; this design retires the CLASS.

## 1. The census, and a fourth shape found while sizing it

Two derivations agree over `sys_darwin.go` at go1.23.12 (the grep of `unsafe.Pointer(&…)` forms and the signature
walk): 50 sites = 12 lifted `&args` + 3 `nil` + 35 `&first`. The 35 split by what the converted dispatch does today:

| shape | what the dispatcher does | sites (Go signature order) |
|---|---|---|
| **(a) SILENT-stale** — a plain-integer first parameter with parameters behind it | places the first, the rest ride stale registers | `sigaction(sig, new, old)` [hand-owned, inc 6], `sigprocmask(how, new, old)` [hand-owned, inc 5], `read(fd, p, n)` and `write1(fd, p, n)` [hand-owned, inc 4], `setitimer(mode, new, old)`, `kevent(kq, ch, nch, ev, nev, ts)`, `pthread_kill(t, sig)`, `syscall_syscall9(fn, a1…a9)` |
| **(b) SILENT-unwritten** — one parameter whose trampoline writes the RESULT through the pointer | places the field(s), never writes back | `walltime()` (`&t timespec`: the trampoline calls `clock_gettime(CLOCK_REALTIME, &t)` — today it calls `clock_gettime(0, 0)` and `walltime` returns `(0, 0)`), `pthread_self()` (`&t`: the trampoline stores AX into the block — today `pthread_self` returns 0), `nanotime1()` [hand-owned] |
| **(c) LOUD** — a pointer first parameter | refuses: "the argument box does not point at a struct" (a `ж<T>` or `unsafe.Pointer` pointee is a class) | `pthread_attr_init/getstacksize/setdetachstate`, `pthread_create`, `munmap`, `madvise`, `mlock`, `open`, `sigaltstack`, `sysctl`, `sysctlbyname`, `pthread_mutex_init/lock/unlock`, `pthread_cond_init/wait/timedwait_relative_np/signal` (the lock family is hand-owned by `lock_sema_impl.cs` and never reaches the dispatcher), `pipe` [hand-owned, inc 4] |
| **correct** — one integer parameter | places it | `raise`, `closefd`, `exit`, `usleep`, `raiseproc` |

**Shape (d), found by this census and not in the Q41 correction: the C RESULT is discarded.** Go's `asmcgocall`
returns the trampoline's AX as `libcCall`'s `int32`, and **twenty** converted sites read it — `var ret = libcCall(…)`
or `return libcCall(…)` (`pthread_attr_*`, `pthread_create`, `open`, `closefd`, `sysctl`, `sysctlbyname`, `kqueue`,
`kevent`, the `pthread_mutex_*`/`pthread_cond_*` family, `issetugid`, `mach_vm_region`, `proc_regionfilename`) —
while the hand-owned `libcCall` returns 0 unconditionally, on the header's claim that "no caller in sys_darwin.cs
reads libcCall's result (each reads its args struct instead)". That claim was measured over the LIFTED family,
whose trampolines store AX into a `ret` field, and it is false for the `&first` and `nil` shapes: `open` answers fd
0, `kqueue` answers kq 0 (stdin), `kevent` answers zero events, `closefd` answers success, `issetugid` answers
false, whatever libc said. Silent, and independent of the register problem.

## 2. Reachability from the managed host — which rows meet which site

The managed host never runs `schedinit`, `osinit`, `mstart`, `minit`, `newosproc`, `sysmon` or the preemption
machinery; reachability is decided by what the CONVERTED packages call, and the darwin behavioral census (trains
23–25: 12 failing rows on both legs at the same doors) is the instrument that says where the deaths are today.

| site | Go callers (darwin-selected files) | reached from the managed host at master? | row(s) that meet it | payoff of the lift alone |
|---|---|---|---|---|
| `sigaction` (a) | `setsig`/`getsig`/`setsigstack` ← `sigenable`, `sigignore`, `initsig`, `minitSignalStack` | YES — `signal.Notify`/`Ignore` (`SignalPrimitives`; `os/signal`'s own suite) | `SignalPrimitives` (both mac legs now print the `sigtramp` door AFTER increment 6) | none beyond inc 6 — and Q52's darwin bridge ROUTES AROUND `sigenable`/`sigignore` (the linux bridge's shape), leaving `sigaction` reachable only from `initsig`/`minitSignalStack`, which the host never runs |
| `sigprocmask` (a) | `ensureSigM`'s goroutine, `sigblock`, `msigsave`/`msigrestore` | YES — `ensureSigM` on the first `Notify` | `SignalPrimitives` | none beyond inc 5; also routed around by the Q52 bridge (ensureSigM elided on linux) |
| `setitimer` (a) | `setProcessCPUProfiler`/`setThreadCPUProfiler` (`signal_unix.go:308/310`) ← `runtime/pprof.StartCPUProfile` | YES on any row that starts a CPU profile | `runtime/pprof`'s CPU-profile rows (SUB-Q43's darwin reading pending; unbanked on darwin) — and the profile signal itself is Q52's door | the lift places `mode`; `new`/`old` are `*itimerval`, whose `timeval` carries a `pad_cgo_0 [4]byte` → reference-bearing → the MIRROR's (Q44's worklist already names darwin `Timeval`) — a `setitimer_impl.cs` of increment 6's shape, ~40 lines |
| `kevent` (a) | `netpollinit` (`netpoll_kqueue.go:55`, `:107` `netpoll`), `netpollBreak`, `netpollopen`/`netpollclose` (`netpoll_kqueue_event.go`) ← every `net` listener/dialer | YES — every `net` row | the five net rows at `C# 134 "Fatal error."` (`NetDeadlineMatrix`, `NetListenSmoke`, `TcpLoopbackRoundTrip`, `UdpLoopbackRoundTrip`, `UdpWriteMsgAddrPort`) — measured door (§2.1): they die BEFORE any `kevent`, in the SYSCALL family's `SockaddrInet4.sockaddr()`; `kevent` sits behind that door | the lift places `kq`/`nch`/`nev`; `ch`/`ev` are `*keventt` (`udata *byte` → reference-bearing → mirror), `ts *timespec` is reference-free (pinnable, placeable). With shape (d) fixed, `kqueue()` answers the real kq. The net rows need `kqueue` (d) + `kevent` mirror — darwin increment 7's candidate |
| `pthread_kill` (a) | `signalM` (`os_darwin.go:475`) ← `preemptM`, `sigprof`'s thread targeting | NO — no preemption, no sysmon in the host | none at master | fully correct after the lift (two integers); structural |
| `syscall_syscall9` (a) | the runtime side of `syscall.syscall9`'s linkname | NO — the `syscall` package's wrappers are realized in the keystone's own path (C# signatures ARE the layout, §7.1) | none | a per-symbol form: the block's FIRST field is the callee (`fn(a1…a9)`), ten inputs > `MaxArgs` 9 — recorded, not lifted |
| `walltime` (b) | `time_now` (`timestub.go:27`) — but the converted `time.now` is hand-owned in `time_impl.cs` | NO at master | none | the block-address form (see §3.3) |
| `pthread_self` (b) | `minit` (`os_darwin.go:335`) | NO | none | the result-store form (§3.3) |
| `open` (c) | `create_file_unix.go` (`debug.SetCrashOutput`), `debug/stack.go:58`, `runtime.readRandom`'s device fallback | YES on rows that call `debug.SetCrashOutput` / crash-output paths (`runtime/debug`, `runtime`'s own suite, unbanked on darwin) | none measured yet | `name *byte` is an ELEMENT box into a byte array — reference-free storage, pinnable — so the pointer-as-`uintptr` field of §3.2 places it; with (d) fixed `open` answers the real fd. Fully correct after the lift |
| `sysctl` / `sysctlbyname` (c) | `getncpu`/`getPageSize`/`getcpucount` (`osinit`), `internal/cpu`'s darwin `doinit` ← `sysctlbyname` | NO from the host (`osinit` never runs; `internal/cpu` is hand-owned as a `[ModuleInitializer]` on x86) | the `IpAdapterAddresses` row's `sysctl` door is the SYSCALL package's `sysctl` (`syscall_syscall6`, the lifted family) — a different site | every pointer parameter is an element or scalar box over reference-free storage → fully placeable after the lift; structural today |
| `munmap` / `madvise` / `mlock` (c) | `sysFree`/`sysUnused`/`sysMap` (`mem_darwin.go`), `minit` | only through the memory family's persistentalloc paths (C1's linux increment 6 names the rows; darwin is the same population, unbanked) | none measured at master | `addr unsafe.Pointer` is a `Pointer` CLASS holding a `uintptr` — the `(uintptr)` field of §3.2 places it; the lifted `mmap_args` sibling has the same `Pointer` field and is the memory design's to widen (§5) |
| `sigaltstack` (c) | `minitSignalStack`, `sigaltstack` in `signal_unix.go:562/1322` | NO | none | `*stackt` (`ss_sp *byte` → reference-bearing) → mirror; unreached, left alone |
| `pthread_attr_*`, `pthread_create` (c) | `newosproc` | NO — the host never creates an M | none | `*pthreadattr` (`[56]int8` → reference-bearing) → mirror; unreached, left alone |
| `pthread_mutex_*` / `pthread_cond_*` (c) | `semacreate`/`semasleep`/`semawakeup` | NO — hand-owned by `lock_sema_impl.cs` above the dispatcher | none | none needed |
| `raise`, `raiseproc`, `exit`, `usleep`, `closefd` | crash paths (`dieFromSignal`, `raisebadsignal`, `throw` → `freezetheworld` → `usleep`, `exit`) | YES on every throw path | every row that throws | already correct; `closefd`/`exit` gain nothing, `usleep`'s dispatch is right |

**Reading the table honestly:** at master the lift alone moves NO measured row. The rows it can move — `net`'s
five (behind `kqueue`'s result and `kevent`'s mirror), `runtime/pprof`'s CPU rows (behind `setitimer`'s mirror AND
Q52's `SIGPROF` delivery) — each need a per-site mirror the lift cannot supply, and the two silent-(b) members have
no consumer. What the lift IS: the retirement of a class whose next member surfaces the day any of these paths is
reached (the memory family, the preemption family, `SetCrashOutput`, `sysctl` under a future `osinit` arc), and the
correction that makes `libcCall`'s result mean something. That is a correctness property of the darwin run layer,
not a row count, and the acceptance table (§7) says so in numbers: 0 rows predicted to move on the lift alone.

### 2.1 The net rows' door, measured (run 33957011659 on `bc8973259`, `behavioral-stderr` on the five rows)

All five rows die the same way on BOTH mac legs (osx-arm64 and osx-x64, read separately), before any socket is bound: `exit 134`, `Fatal error.
System.AccessViolationException … at go.array<byte>.get_Item ← syscall.sockaddr(ж<SockaddrInet4>) ←
SockaddrInet4жSockaddr.sockaddr ← syscall.Bind ← net.listenStream` (the two UDP rows through `listenDatagram`);
each leaves a `SIGABRT / EXC_CRASH` report on each leg (ten reports, ten identical managed traces). The door is darwin's AUTO-converted `SockaddrInet4.sockaddr()`
(`syscall/darwin/syscall_bsd.cs`): Go's `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))` converts to
`(ж<array<byte>>)(uintptr)(FromPinnedBox(Ꮡsa.of(Ꮡraw).of(ᏑPort)))` — a `(uintptr)` bridge from a field of
`RawSockaddrInet4`, whose `Addr [4]byte`/`Zero [8]int8` fields make it REFERENCE-BEARING, so the number
resolves to no box and `explicit operator ж<T>(uintptr)` mints a `NativeBox<array<byte>>` over an unpinned
interior address; `p.Value[0]` then reads through it and faults. This is the STRUCT-PASSING class in the
syscall family — Q44's worklist names darwin's `SockaddrInet4` — and it is the same door the linux roster met
on 2026-08-22 (`index out of range [0] with length 0` in the same method, R5), answered there by
`syscall/linux/sockaddr_linux_impl.cs` (the `writeNativeSockaddr` mirror). The registry scopes that hand-own
`goosWindowsLinux` on purpose: "darwin declares the same names and keeps its auto bodies until a darwin lane
measures them". This run is that measurement. **The net rows are therefore NOT Q56's population**: the lift
cannot reach them, and `kqueue`'s zero result and `kevent`'s stale pointers (§2) are the doors BEHIND this
one. The remedy is a darwin twin of the linux sockaddr mirror — `syscall/darwin/sockaddr_darwin_impl.cs`, the
registry scope widened to all three flavours — sized as darwin increment 7 in the announce, not in this design.

## 3. The rule

### 3.1 Recognition (converter)

A `FuncDecl` whose `Doc` carries the `//go:cgo_unsafe_args` directive (the same comment-group scan
`linknameOperations.go` performs for `//go:linkname`) and whose body contains a call to `libcCall` whose second
argument is `unsafe.Pointer(&<ident>)` where `<ident>` is:

- the function's FIRST parameter → shapes (a)/(c): the block is the parameter list;
- a local or a named result → shape (b): the block is that variable, and the trampoline's FORM (§3.3) decides how
  its address is used.

`nil` and `&args` (a local of anonymous struct type) are the existing shapes and are untouched. The directive is
the contract: Go guarantees the parameters are laid out contiguously on the stack in declaration order exactly
BECAUSE of it, so a function without the directive that happens to pass `&first` is not this shape.

### 3.2 Emission — the block

For shapes (a)/(c) the converter synthesizes, beside the function, a lifted struct in the corpus's existing
`[GoType("dyn")]` form with the anonymous-args naming the corpus already carries (`fcntl_args`, `mmap_args`):

```csharp
[GoType("dyn")] internal partial struct sigaction_args {       // synthesized: Go's ABI0 parameter block
    internal uint32 sig;
    internal uintptr @new;                                      // *usigactiont → the box's address (§3.2 pointer rule)
    internal uintptr old;
}
```

and the body becomes the lifted family's shape:

```csharp
internal static void sigaction(uint32 sig, ж<usigactiont> Ꮡnew, ж<usigactiont> Ꮡold) {
    ref var args = ref heap(new sigaction_args(sig, (uintptr)Ꮡnew.OrTypedNil(), (uintptr)Ꮡold.OrTypedNil()), out var Ꮡargs);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(sigaction_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡnew.OrTypedNil());      // Go's own KeepAlive lines, already emitted
    KeepAlive(Ꮡold.OrTypedNil());
}
```

**Field rule, in Go's ABI0 order:** an integer parameter keeps its Go width (`DispatchArgsStruct` zero-extends a
32-bit field with `MOVL` semantics and passes a 64-bit one whole — the trampolines' own `MOVL`/`MOVQ` split);
a **pointer-typed parameter becomes a `uintptr` field minted through golib's address model** — `(uintptr)box`:
0 for a nil box, the PINNED address for reference-free storage (a scalar box, an element box into a `[]byte` or
`[]uint32`, a `timespec`), the managed-pointer TOKEN (Q44) for reference-bearing storage, so libc receives a
non-address and answers `EFAULT` instead of reading reordered managed memory; an `unsafe.Pointer` parameter is
already a number (`Pointer` is a class over `uintptr`) and is carried as that number. Results are NOT fields: the
C result is `libcCall`'s return (§3.4). Layout in bytes is never materialized for these shapes — the dispatcher reads
fields by reflection and places registers in order — so alignment and padding do not arise; the block's ORDER and
WIDTHS are the whole contract, and they are Go's.

`OrTypedNil()` is what the emitted `KeepAlive` lines already use for a possibly-nil box; the `(uintptr)` conversion
of a nil box is 0 by golib's operator.

### 3.3 Shape (b): a per-symbol FORM table, two entries

The `&first` rule cannot see how a trampoline USES the pointer; the two shape-(b) members use it two different
ways, and neither is "fields to registers":

| symbol | trampoline | form |
|---|---|---|
| `walltime` | `MOVQ DI, SI; MOVL $CLOCK_REALTIME, DI; CALL libc_clock_gettime` | **block-address-as-argument**: `clock_gettime(0, &block)` — the block (`timespec`, reference-free, pinnable) is passed BY ADDRESS as the second argument, with a constant first |
| `pthread_self` | `CALL libc_pthread_self; MOVQ AX, 0(BX)` | **result-stored-into-block**: the C result is written into the block's first field |

Rather than teach the dispatcher these forms from the struct alone (it cannot), `libccall_impl.cs` carries a
two-entry table keyed by symbol name — the "per-symbol layout record" its own refusal message has named since the
keystone landed — read before the default fields-to-registers dispatch. Both members are unreached at master (§2),
so the table is correctness for the day they are, at the cost of two lines; `nanotime1` is already hand-owned.

### 3.4 Shape (d): `libcCall` returns the C result

`GoLibcCall.DispatchArgsStruct` returns `void` and the hand-owned `libcCall` returns 0. The fix is one line each:
`DispatchArgsStruct` returns the register `Call` handed back, and `libcCall` returns `unchecked((int32)r)` — exactly
what `asmcgocall` hands Go. The lifted family is unaffected (its callers read `args.ret`, which the dispatcher
already fills). `kqueue()` then answers the real descriptor, `open` the real fd, `closefd` libc's verdict.

### 3.5 What `DispatchArgsStruct` needs beyond that: nothing

The synthesized block satisfies the dispatcher's existing protocol — a reference-free value type whose leading
fields are integers (every pointer became a `uintptr` field) — so `ManagedPointerTokens.Resolve` recovers the box
through `FromPinnedBox` exactly as it does for `fcntl_args` today, and `ToRegister`'s refusal of managed references
never fires on a lifted block. The refusal stays in place for the Go-authored lifted structs that DO carry
references (`mmap_args`, `mach_vm_region_args`, `proc_regionfilename_args`), which is §5's corollary.

## 4. What the rule retires, and what it cannot

**Retired:** shape (a)'s stale registers at every site, hand-owned or not (`setitimer`, `kevent`, `pthread_kill`,
`syscall_syscall9` recorded); shape (b)'s unwritten results through the two-entry form table; shape (d)'s zero
result at twenty sites; shape (c)'s refusal at every site whose pointees are reference-free (`open`, `sysctl`,
`sysctlbyname`, `munmap`/`madvise`/`mlock`) — those become fully dispatchable.

**Cannot, and stays the mirror's:** a pointee that is reference-bearing has no address to hand libc — `usigactiont`
(inc 6, done), `itimerval` (`setitimer`), `keventt` (`kevent`), `stackt` (`sigaltstack`), `pthreadattr` (the
`pthread_attr_*`/`pthread_create` family). Under Q44's token they get `EFAULT`, an improved failure and not a fix;
the remedy per site is increment 6's shape — a native encode at the seam (`writeNativeSockaddr`'s) — and the lift
leaves increments 4–6's hand-owns exactly where they are: `read`, `write1`, `pipe`, `sigprocmask`, `sigaction`
stay displaced through `manualConversionFuncs`, their placeholders untouched (a displaced function has no body for
the rule to rewrite), their headers re-read at the cut: `sigaction_impl.cs`'s SCOPE paragraph names the four
`&first` siblings as "keep their generated bodies … a separate ruling" and is amended to name this design;
`libccall_impl.cs`'s MEASURED CORRECTION paragraph gains shape (d) and the form table.

## 5. Corollary for the memory family (recorded, not this cut)

`mmap_args` — a Go-authored lifted struct — carries two `unsafe.Pointer` FIELDS, so the dispatcher refuses it as
reference-bearing today; the same `(uintptr)` field rule applied to `unsafe.Pointer` fields of Go's OWN lifted
structs would place them (the `Pointer` class is a number). That widening belongs to the darwin memory family
(C1's linux increment 6 is the design of record for the memory primitives; darwin's `sysAlloc`/`sysMap` are the
same population one flavour over) and is named here so it is not rediscovered.

## 6. Footprint, predicted by site (two-seeded three-target, both arms from `git archive` at go1.23.12)

- **windows / linux arms: 0 files.** No `//go:cgo_unsafe_args` `&first` libcCall site exists outside darwin's
  `sys_darwin.go` in the corpus's three targets (`os_aix.go`, `os3_solaris.go`, `sys_openbsd*.go` carry the shape
  but are not targets).
- **darwin arm: `runtime/darwin/sys_darwin.cs`** — 31 sites rewritten (35 minus the four displaced by increments
  4–6: `read`, `write1`, `sigprocmask`, `sigaction`), each gaining one synthesized `[GoType("dyn")] <name>_args`
  declaration (4–7 lines by arity) and one block-construction line, the `libcCall` line changing its pointer argument
  only: predicted **+220 ± 30 / −0 ± 5** (the `KeepAlive` lines and the trampoline partials unchanged).
  `runtime/darwin/package_info.cs` +1/−1 (the `sys_darwin.go` position-map line for the grown file — increment 5's
  and 6's shape) and, if the `[GoType("dyn")]` lifts mint `GoImplement`/witness records, the corresponding lines
  (predicted 0: the existing `_args` structs mint none). `libccall_impl.cs` (hand-own) +~20 for the form table and
  the return; golib `GoLibcCall.cs` +2/−1 for the return value. `stdlib-metadata.txt` unchanged.
- Marker gate 0 rewritten on every arm; the hand-owns' placeholders byte-identical.

## 7. Guard and acceptance

**Converter guard** (the `syscallFunnelSet_test.go` fixture family): a synthetic module carrying a
`//go:cgo_unsafe_args` function of each shape — (a) three integers, (c) a `*byte` and a `*timespec` parameter, (b)
a `&local` — asserting the emitted block declaration, the `(uintptr)` fields, the `FromPinnedBox(Ꮡargs)` argument
and the untouched `KeepAlive` lines; a function WITHOUT the directive passing `&first` must be left alone (the
negative arm).

**Dispatcher guard** (GolibTests, linux flavour, glibc — the arrangement `LibcCallDispatchTests` and the increment
5/6 contract tests use), one arm per shape, each with the register-placement DISCRIMINATING by construction:
- (a) `kill(getpid(), 0)` through a two-field block: 0 only if BOTH registers were placed (a stale second register
  answers `EINVAL`/`ESRCH` — the control that could not pass by accident);
- (c) `clock_gettime(CLOCK_REALTIME, (uintptr)Ꮡts)` with a reference-free `timespec` box as the second field:
  `tv_sec` must read back non-zero THROUGH the box, proving the pinned address was the one libc wrote;
- (d) `getpid()` through the dispatcher must equal `Environment.ProcessId` — the return value surfaces;
- (b) the form table: `pthread_self` via the result-store form returns a non-zero id equal to a direct P/Invoke.
Each arm neutered once (the second field dropped; the return discarded) must go RED by name before the guard is
believed.

**Darwin acceptance rows** (behavioral-full census, both mac legs, scored against this table BEFORE the run):

| row | door at master (trains 23–25) | predicted after the lift ALONE | predicted after the lift + the named mirror |
|---|---|---|---|
| `SignalPrimitives` | `FuncPCABI0(sigtramp)` (both legs print it since inc 6) | unchanged | unchanged — Q52's door |
| the five `net` rows | `C# 134` AccessViolation in `SockaddrInet4.sockaddr()` — the syscall family's darwin door, measured (§2.1) | unchanged (the lift cannot reach it) | unchanged until the darwin sockaddr mirror lands; only then do `kqueue` (d) and `kevent`'s mirror become the rows' next doors |
| `IpAdapterAddresses` | `sysctl` (the syscall package's, lifted family) | unchanged | unchanged (not this class) |
| every other row | unchanged | unchanged | unchanged |
| **count of rows predicted to move on the lift alone** | | **0** | |

A row moving that this table says stays is a finding; a row the table says moves and does not is the cut's
falsifier. The behavioral-stderr stage on the named rows is the placement instrument, the crash-report block the
arm64 reader.

## 8. Order and gates for the cut (after the ruling)

1. `GoLibcCall.DispatchArgsStruct` returns the result; `libcCall` returns it (§3.4) — golib + one hand-own line.
2. The converter rule (§3.1–3.2) with its fixture guard RED-then-green.
3. The form table (§3.3) in `libccall_impl.cs`.
4. Two-seeded three-target diff read at the per-target staging roots (Q48 is landed; the merge is unblocked), the
   footprint checked against §6 BY SITE; the hand-owns' placeholders byte-identical; `go generate .` unchanged.
5. Darwin `runtime`, `os/signal`, `net` closures `-p:GoTargetOS=darwin --no-incremental` after a purge.
6. The dispatcher guard both configurations, count-matched, its neuters fired.
7. The behavioral-full darwin census on both legs, scored against §7's table; the five net rows through
   behavioral-stderr for the door.

Seat: one train, announced before push; the `kevent`/`setitimer` mirrors are their own increments and are not
folded in.
