# DESIGN — the os/signal bridge, darwin flavour (Q52): how a converted `os/signal` delivers a kernel signal INTO managed code on a mac, and what that closes on linux

> **Status:** design, cut on the landed master `bc8973259` (2026-09-05, lane C2, dispatched by COORD eb3739373; the
> linux half coordinated with C1 — 8366aa910 — and steered by COORD 493a41bf7 / a97d779d2). Nothing here is cut;
> the cut follows the ruling. **Companions:** `DESIGN-signal-posix-bridge.md` (C1's linux bridge, v1 + v2 — the
> design of record this document EXTENDS, not replaces), `runtime/linux/signal_posix_impl.cs` (the linux body),
> `runtime/darwin/sigprocmask_impl.cs` and `runtime/darwin/sigaction_impl.cs` (increments 5 and 6 — the darwin
> seams this bridge routes AROUND for `Notify`/`Ignore` and USES for the disposition seed and the death path),
> `DESIGN-cgo-unsafe-args-block-lift.md` (Q56 — places `sigaction`'s parameter block; this design stands on it),
> the train-24/25 darwin census blocks and the Q41 record (mailbox 2720b6258 / 394548a64 / c6720db06).

## 0. The door, in one paragraph

Since increment 6 BOTH mac legs of `SignalPrimitives` die at the same statement the same way: `signal.Notify` →
`signal_enable` → `sigenable` → `setsig(sig, sighandler)` → `abi.FuncPCABI0(sigtramp)` (os_darwin.go:390) — a
program counter for an ASSEMBLY trampoline the kernel would jump to on delivery. No libc body can supply it: the
next darwin door is the REVERSE direction of every darwin increment so far — the kernel calling INTO managed code
on an arbitrary thread — and it collides with the CLR's own handler chain. The linux run layer met the same door
on 2026-08-27 and answered it with a bridge that never installs a trampoline at all: `.NET`'s
`PosixSignalRegistration` owns the kernel side and a managed callback makes Go's per-delivery decision. **This
design is that bridge's darwin flavour**, with every clause where darwin differs measured or named, and the
linux residual restated from C1's record rather than re-derived.

## 1. The population — who reaches `setsig`/`sigenable`, per flavour

`setsig` is reached from exactly three places in the pinned go1.23.12 runtime: `initsig` (process start, the
never-run `schedinit` path), `sigenable`/`sigignore`/`sigdisable` (the install layer `signal.Notify`/`Ignore`/
`Stop`/`Reset` drive through `sigqueue.go`), and `dieFromSignal`/`crash` (`setsig(sig, _SIG_DFL)` before
`raise`). What reaches them from the converted corpus:

| row | path | windows | linux | darwin |
|---|---|---|---|---|
| `os/signal` (Go's own suite) | every `Notify`/`Ignore`/`Stop`/`Reset` | banked, 1 verdict (the Ctrl+Break console path — a different bridge) | the linux bridge's own measure (v2 was cut against this suite's `TestStop`/`TestNohup`/`TestDetectNohup` shapes) | UNBANKED — no darwin roster row exists yet; the `sweep-shard` stage of the os-matrix is the instrument |
| `SignalPrimitives` (behavioral, the only behavioral importer of `os/signal`) | `Ignore`, `Notify`, `Stop`, `Ignore`, `Reset` | passes | passes (the bridge) | BOTH legs `exit 2; stderr 20; stdout 2` at the `sigtramp` door since increment 6 (before it: arm64 mute `exit 138`) |
| `os/exec` (116 banked) | `TestWaitInterrupt/*`, `TestSIGQUIT`, `TestSIGCHLD` — the wall the linux bridge dissolved | banked | the bridge's acceptance | unbanked on darwin |
| `net` (472 banked, 2 disclosed) | `os.sigpipe` → `runtime.os_sigpipe` → `dieFromSignal(SIGPIPE)` on EPIPE to fd 1/2 — the DEATH path, not Notify; and the five behavioral net rows sit behind the syscall family's `sockaddr` door on darwin (Q56 §2.1) | n/a | SIGPIPE non-delivery is the bridge's residual; the death path throws at `rt_sigaction` (linux residual, §4) | after the sockaddr mirror lands, the death path runs through increment 6's REAL `sigaction` and `raise` — Go's death reproduced (§3.4) |
| `runtime` (its own suite: `TestSignalForwardingExternal`, `TestSignalIgnoreSIGTRAP`, the crash-handler family) | `sigenable`, `sigInstallGoHandler`, `sigaltstack` | unbanked | C1's rows (getg increments) | unbanked |
| `runtime/pprof` (CPU profile rows) | `setProcessCPUProfiler` → `setsig(SIGPROF)` → then `setitimer` | n/a | the `rt_sigaction` stub behind C1's `TestCPUProfile` — the SIGPROF path, RULED a disclosed residual on both flavours (493a41bf7; the profiler-signal design is its own item) | same residual; `setitimer`'s mirror (Q56 §2) is moot until a profiler-signal design exists |
| `syscall` / `os` (`Process.Signal`, `Kill`) | the SEND side (`kill(2)`); receive is `os/signal`'s | banked | banked | unbanked; `kill` is a lifted-struct keystone call, dispatchable |

**What the population says:** on darwin the door is reached by ONE behavioral row today and would be reached by
three roster rows (`os/signal`, `os/exec`, `runtime`) the day a darwin sweep exists; the SIGPIPE death path is a
fourth consumer behind an unrelated door. The linux side is DONE for `Notify`/`Ignore`/`Stop`/`Reset` (C1's v2
bridge is the linux answer — this design adds no linux code) and carries two residuals restated in §4.

## 2. The mechanism, priced — three candidates, one chosen (the same one linux chose, for the same reasons)

### 2.1 `PosixSignalRegistration` (chosen)

`.NET`'s Unix signal plumbing (`System.Native`, shared by linux and macOS): `PosixSignalRegistration.Create(signal,
handler)` installs the runtime's own native handler for that signal, which writes to a pipe read by the runtime's
signal-handling thread, which dispatches the managed `handler(PosixSignalContext)` on a THREADPOOL thread —
never on the interrupted thread, never inside a signal context. `ctx.Cancel = true` suppresses the default
disposition (the kernel's death or stop) for that delivery. The linux v2 bridge measured, on 2026-08-27 and
again by C1's Q46 disposition read, everything the darwin flavour needs:

- **Raw signal numbers pass through** on Unix: `Create((PosixSignal)10)` registered and delivered SIGUSR1 on linux
  (the enum's named members are NEGATIVE and translated by the runtime; a POSITIVE value is the platform number).
  On darwin the same call takes darwin's numbers — `SIGUSR1` is **30**, `SIGUSR2` **31**, `SIGCHLD` **20**,
  `SIGCONT` **19**, `SIGWINCH` **28**, `SIGINFO` **29** (darwin-only, `_SigNotify + _SigIgn`) — so the number map
  is a per-flavour TABLE, which is why the file is per-flavour (§5).
- **What `Notify`'s contract needs** is exactly what the callback can do: `sigsend(sig)` (auto, re-checks
  `sig.wanted`, enqueues to `signal_recv`'s queue and wakes it through the note) — safe from any thread, and the
  whole delivery machinery below it (`signal_recv`, the note wakeup, the os/signal channel) stays auto and was
  measured working on linux.
- **Go's per-delivery decision** in the callback, verbatim from the linux v2: `sigsend` delivered → cancel;
  `signal_ignored(sig)` → cancel; `_SigKill` member (darwin: HUP 1, INT 2, TERM 15 — read from
  `signal_darwin.go`'s `sigtable`, not linux's) → let the default kill; otherwise (a `_SigNotify`-only row:
  USR1/USR2/CHLD/CONT/WINCH/INFO/URG/IO/…) → cancel, i.e. swallow, as Go's own handler does.
- **Persistent registrations at runtime-assembly load** for the whole mapped set (v2's lesson: Go installs at
  `initsig`, Notify/Stop toggle FORWARDING), so an unwanted SIGUSR1 before any `Notify` is swallowed rather than
  killing the host (TestStop's shape) and an unwanted SIGHUP still dies.
- **The inherited-disposition seed** (`sigInitIgnored` for signals the process was born ignoring — the nohup
  family): linux reads it with a raw `sigaction(sig, NULL, &old)` P/Invoke over a 160-byte glibc struct; darwin
  reads it through **increment 6's body** — `GoSigactionQuery(sig)` answers the handler word (1 = `SIG_IGN`)
  through the 16-byte mirror. This is the first consumer of increment 6 outside the row it was cut for.

### 2.2 A native handler that enqueues to a managed channel (rejected, with the reason)

Installing a `delegate* unmanaged` / `[UnmanagedCallersOnly]` method as the `sa_sigaction` handler through
increment 6's real `sigaction` is expressible in the converted runtime. It is not admissible: the CLR does not
support managed code running in a signal-handler context (a delivery landing on a thread the GC has suspended, or
inside the runtime's own handler chain, deadlocks or corrupts — the same reason the CLR's PAL forwards its own
signals to a dedicated thread). It would also have to coexist with the CLR's installed handlers on the crash
signals, which `PosixSignalRegistration` already arbitrates. Priced and refused; if a future profiler-signal design
needs the interrupted thread's PC, this is the door it will have to price again, and it is not `Notify`'s.

### 2.3 Honest disclosure for the signals the CLR owns (the residual, per flavour)

Go itself never delivers the synchronous signals to `Notify` (SIGSEGV/SIGBUS/SIGFPE/SIGILL/SIGTRAP become
run-time panics, "the runtime will not deliver them to Notify"), so the CLR owning them is Go-compatible by
construction: `signal.Notify(c, syscall.SIGSEGV)` is a no-op on both. The rest of the residual, restated:
`SIGABRT` (`_SigNotify + _SigThrow`; the CLR's abort path owns it — a registration would take a disposition
the CLR uses to die), `SIGPIPE` (registers, does not deliver — probed on linux; §3.4 covers the death path
separately), `SIGPROF` (ruled a disclosed residual on both flavours until a profiler-signal design exists),
`SIGKILL`/`SIGSTOP` (uncatchable everywhere), `SIGEMT`/`SIGSYS` (`_SigThrow`, no `Notify` on Go's side either),
and the real-time range (none on darwin). C1's live disposition map on linux (8366aa910) is the model the darwin
bridge's first acceptance run READS rather than presumes: the bridge logs, once at init, the set of signals whose
pre-existing handler is neither `SIG_DFL` nor `SIG_IGN` (the CLR's own catches, through `GoSigactionQuery`), so the
mac runners state their map in the run log.

## 3. The darwin flavour, clause by clause

| clause | linux (C1's file) | darwin (this design) | why it differs |
|---|---|---|---|
| file | `runtime/linux/signal_posix_impl.cs` | `runtime/darwin/signal_posix_darwin_impl.cs` — a DISTINCT basename (a97d779d2's rule: L3 places one hand-own per logical name only when an emitted principal exists, and refuses two differing copies; `signal_posix` has no principal today, and the day one exists a same-basename pair would be refused) | the Q48 refusal shape |
| registry | `"sigenable"/"sigdisable"/"sigignore": goosLinux` | the same three names scoped `goosDarwin` — three placeholders in `runtime/darwin/signal_unix.cs`, the other ~1,440 lines reconverting | each flavour its own body, one registry line each |
| number map | linux numbers (USR1 10, USR2 12, CHLD 17, CONT 18, WINCH 28) | darwin numbers (USR1 30, USR2 31, CHLD 20, CONT 19, WINCH 28, INFO 29) from `defs_darwin_amd64.cs`; named members (`SIGHUP`/`SIGINT`/`SIGQUIT`/`SIGTERM`/`SIGCHLD`/`SIGCONT`/`SIGWINCH`) translated by the runtime | different ABI numbers |
| `_SigKill` / `_SigThrow` sets | linux `sigtab_linux_generic.go` | darwin `signal_darwin.go`: kill = HUP, INT, TERM; throw = QUIT, ABRT (also EMT, SYS, ILL, TRAP — not mapped) | per-OS `sigtable` |
| inherited-disposition seed | raw `sigaction` P/Invoke, 160-byte glibc struct | `GoSigactionQuery` — increment 6's 16-byte mirror, the handler word == 1 | the struct is 16 bytes on darwin and already marshalled |
| `ensureSigM` / `sigprocmask` | elided | elided the same way — increment 5's `pthread_sigmask` body stays for any path that still masks (`sigblock`/`msigsave`, unreached) | the registration owns its own thread and mask |
| `sigaction`'s install side | never installed (the CLR's handler is the handler) | never installed by the bridge — increment 6's body stays the truth for `dieFromSignal`'s `setsig(sig, _SIG_DFL)` and for the seed | one seam, two consumers |
| delivery thread | .NET's signal thread → threadpool | identical (`System.Native` is one implementation) | — |

### 3.4 The SIGPIPE death path, the one place darwin is AHEAD of linux

`os.File.Write` to fd 1/2 answering `EPIPE` calls `os.sigpipe()` → `runtime.os_sigpipe` → `dieFromSignal(SIGPIPE)`:
`setsig(SIGPIPE, _SIG_DFL)` (a REAL install through increment 6), `sigprocmask` unblock (increment 5), `raise(SIGPIPE)`
(a single-integer libcCall, dispatched correctly since the keystone). On darwin every call on that path has a real body
today, so the process dies by SIGPIPE exactly as Go's does — the CLR's `SIG_IGN` on SIGPIPE (C1's map) is overwritten
by the `SIG_DFL` install one line before the raise. On linux the same path throws at the `rt_sigaction` stub (C1's
residual). Predicted, not measured: an acceptance arm in §6 measures it.

## 4. The linux half — what this design changes there: nothing, and two residuals restated from C1's record

The linux bridge is the linux answer; this design adds no linux code. Restated so the two flavours read as one
record: (i) `SIGPROF` — `TestCPUProfile`'s `rt_sigaction` is `setProcessCPUProfiler → setsig(SIGPROF)`, one call
before `setitimer`; a profiler-signal design of its own, disclosed on both flavours (COORD's ruling on the profiler
family); (ii) the death path — `dieFromSignal`'s `setsig(sig, _SIG_DFL)` reaches `sysSigaction`'s `rt_sigaction`
stub on linux, so a SIGPIPE-to-stdout death throws instead of dying; the darwin flavour shows the shape a linux
`rt_sigaction` body would buy (increment 6's mirror, the linux `sigactiont` being 32 bytes with `sa_restorer` and a
64-bit mask — a C1 item, sized here as ~the same 60 lines). C1's Q46 disposition map is the linux baseline the
darwin init log is compared against.

## 5. Placement and the converter footprint

- **Registry:** `"sigenable"`, `"sigdisable"`, `"sigignore"` gain `goosDarwin` beside their `goosLinux` (one entry each,
  scope widened — the `goosWindowsLinux`-style two-flavour scope the sockaddr family uses). Bodied functions, so the
  registry is the door (increments 5 and 6's shape).
- **Two-seeded three-target footprint, predicted by site:** windows 0; linux 0 (the linux placeholders are already
  there); darwin `runtime/darwin/signal_unix.cs` −3 bodies / +3 placeholders (`sigenable` −24, `sigdisable` −18,
  `sigignore` −16, each → 1 placeholder line: **+3/−58 ± 6**) and `runtime/darwin/package_info.cs` +1/−1 (the
  `signal_unix.go` position-map line for the shrunk file). Marker gate +1 (the new hand-own).
- **The hand-own** `runtime/darwin/signal_posix_darwin_impl.cs`: the linux file's shape — the registration
  dictionary, `MapPosixSignal` with darwin's table, `sigDiesByDefault`/`sigThrowsByDefault` from darwin's
  `sigtable`, `InitPosixSignalBridge` seeding through `GoSigactionQuery`, the three bodies, and the init-time
  disposition log of §2.3. ~350 lines, most of them C1's clauses re-read against darwin's tables. Shared logic
  becomes a flat core file only AFTER the second flavour is measured (COORD 493a41bf7), not before.
- **Increments 5 and 6 stay** exactly as landed; their headers gain one sentence each naming this bridge as the
  reason `Notify`/`Ignore` no longer reach them.

## 6. Prediction and acceptance — before any run

**`SignalPrimitives`, both mac legs** (the `behavioral-stderr` stage, the crash-report block armed): today
`exit 2; stderr 20; stdout 2` on both. After the bridge: **`exit 0; stderr 0; stdout 6`**, the six lines
byte-equal to `go run`'s (`initially ignored: false` / `after Ignore: true` / `after Notify: false` / `after Stop:
false` / `after Ignore again: true` / `after Reset: false`), NO crash report on either leg. The Q41 arm64 question
restated against it: both legs printed the same door after increment 6, and both print the same six lines after
the bridge — the mute leg is closed as a class, not only as a row; a report on arm64 alone would reopen it and is
the falsifier. `Stop`'s `signalWaitUntilIdle` parks on `signal_recv`'s note (lock_sema, hand-owned) exactly as on
linux — the one clause a darwin-specific hang could hide in, and the reason the run budget stays at 120 s.

**The darwin `os/signal` suite** through the os-matrix `sweep-shard` stage on both mac runners (the first darwin
roster candidate): predicted the linux bridge's shape — the `Notify`/`Stop`/`Ignore`/`Reset` family passes, the
nohup family passes through the `GoSigactionQuery` seed, `TestSignalTrace`/SIGPROF-shaped rows disclosed as the
residual — with the exact counts stated only after the linux row's record is read at the cut (a prediction copies
no number it has not read).

**The disposition log** (§2.3) on both mac runners: the set of CLR-caught signals printed once; predicted to match
C1's linux map on the classic range (INT/QUIT/ILL/TRAP/ABRT/BUS/FPE/SEGV/TERM/CHLD/CONT/TSTP/WINCH/USR1/USR2
caught, PIPE ignored) — a differing set is a finding, not a failure.

**The SIGPIPE death arm** (§3.4): a behavioral guard writing to a closed stdout pipe must die by SIGPIPE on darwin
(exit 141) as `go run` does; on linux it stays the `rt_sigaction` residual until C1's body lands (the guard is
platform-exclusive to darwin until then).

**Gates for the cut:** converter `go test` RED-by-name on the registration alone then green; the two-seeded
three-target diff read at the per-target staging roots against §5; `go generate .`; darwin `runtime` and
`os/signal` closures after a purge; `check-solution-integrity`; the behavioral-stderr acceptance on both legs;
the sweep-shard `os/signal` run on both legs; announce before push; seat as one train.
