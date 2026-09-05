# DESIGN — the os/signal PosixSignalRegistration bridge (Linux)

> Status: **implementing** (lane `claude/laneR-signal-arc`, 2026-08-27). Routed by the coordinator
> after the signal-wall probe confirmed the class is an ARC, not a disclosure. Scope ruled
> install-layer-only; residual keeps its refusal.

## The wall this dissolves

os/exec's signal family — `TestWaitInterrupt/*`, `TestSIGQUIT`, `TestSIGCHLD` — and os/signal's own
suite die on one root: the converted runtime's raw Linux signal syscalls **`rt_sigaction` and
`rt_sigprocmask` are unimplemented PartialStubGenerator stubs**. `signal.Notify`/`signal.Ignore`
reach the kernel through
`os/signal → signal_enable → runtime.sigenable → setsig → sysSigaction → rt_sigaction`, and
`sigenable`/`sigdisable` first hand off to `ensureSigM`'s goroutine over `rt_sigprocmask`. Both throw
— the second on a background goroutine ("unhandled exception outside any test"), which is why the
suite hangs rather than fails cleanly. Rooted 2026-08-27; the twelve-reproduction os/exec heap
corruption was a *different* bug, cured separately by B2's kind split — this is the ordinary residual
behind it.

Why it is a wall and not a bug: the CLR **owns** signal handling on Linux (its own
SIGSEGV/SIGCHLD/SIGTERM handlers; signals for GC and thread suspension), and there is no native Go
`sigtramp` to install via `sigaction`. Faithfully converting Go's own signal machinery lands on
syscalls the managed host cannot host.

## The probe that routed it (2026-08-27)

A standalone net10.0 program confirmed `System.Runtime.InteropServices.PosixSignalRegistration`
delivers every primitive the failing tests exercise, on Linux:

| Primitive | os/signal need | Probe result |
|---|---|---|
| delivery-on-send | `signal.Notify` | SIGINT/SIGQUIT/SIGTERM fire the handler on `kill(self)` |
| Cancel suppression | `signal.Ignore` / handler-installed | `ctx.Cancel=true` survives a would-be-fatal signal |
| SIGCHLD on child exit | os/exec + `TestSIGCHLD` | fires ~2 ms after a spawned child exits, alongside .NET's own reaping |
| N handlers per signal | many `Notify` channels | two registrations on one signal both fire |

The wall bisects at the **PosixSignal enum boundary**: the async-notify subset becomes an arc; the
raw `rt_sigaction` semantics (masks, `SA_ONSTACK`, `SA_SIGINFO` fault detail, handler forwarding,
synchronous in-context execution, real-time signals) stay the honest disclosure.

## The design — install layer only

`signal_enable`/`signal_disable`/`signal_ignore` (`sigqueue.go`) do the `sig.wanted`/`ignored`
bookkeeping and then call one of `sigenable`/`sigdisable`/`sigignore` to reach the kernel. **Only
those three are hand-owned**; everything above and below stays auto:

```
os/signal.Notify → signal_enable [AUTO: sets sig.wanted] → sigenable [HAND-OWN: register]
                                                                 ↓ (a signal arrives)
   PosixSignalRegistration handler → { ctx.Cancel = true; sigsend(sig); }
                                                                 ↓
   sigsend [AUTO: re-checks sig.wanted] → signal_recv [AUTO] → the os/signal channel [AUTO]
```

**One handler serves both Notify and Ignore.** `sigsend` already gates on `sig.wanted`, so:

- After `Notify` (`wanted=1`) the handler's `sigsend` delivers to the channel.
- After `Ignore` (`wanted=0`) `sigsend` drops the signal and `ctx.Cancel` has already suppressed the
  default disposition — which *is* `SIG_IGN`'s observable behavior.

The Notify/Ignore distinction lives entirely in the untouched `sigqueue.go` bookkeeping. The install
layer does not need to know which it is.

**`sigdisable`/Reset disposes the registration**, not merely detaches it: Go's Stop/Reset returns the
signal to *default* handling, and disposing the last `PosixSignalRegistration` restores the previous
(default) disposition — so a `default-death-after-Reset` assertion (the SIGQUIT family's shape) holds.

**`ensureSigM` is elided, not reimplemented.** Its `enableSigChan`/`maskUpdatedChan` handshake was the
protocol of the `rt_sigprocmask` goroutine; `PosixSignalRegistration` owns its own delivery thread and
signal mask. The auto members remain in `signal_unix.cs`, now unreferenced.

## The residual (stays refused)

`.NET`'s `PosixSignal` is a fixed enum. Signals with no member — SIGUSR1/2, SIGPIPE, the real-time
signals — cannot be registered; `MapPosixSignal` returns null and the install is a no-op, so any test
needing them stays the honest `rt_sigaction` disclosure with the probe as evidence. SIGKILL/SIGSTOP
are uncatchable in both runtimes by design.

Mapped set (Linux/amd64 numbers, mirrored by `defs_linux_amd64.cs`): SIGHUP 1, SIGINT 2, SIGQUIT 3,
SIGTERM 15, SIGCHLD 17, SIGCONT 18, SIGWINCH 28.

## Placement and gates

- **Converter:** `sigenable`/`sigdisable`/`sigignore` registered `goosLinux` in
  `manualConversionFuncs` (`manualTypeOperations.go`), the `getGOAMD64level` model — a Linux `-stdlib`
  emission drops the auto bodies to placeholders; `runtime/linux/signal_posix_impl.cs` supplies them
  with the `[module: GoManualConversion]` marker. The other ~1,440 lines of `signal_unix.cs` keep
  reconverting. Darwin's copy stays auto until its own arc.
- **Gates before banking:** the seeded **Linux-target reconvert + marker gate** (prove the converter
  reproduces the placeholders on a linux emission, the wait4 ritual — not only that Windows CNR is
  inert); the os/exec signal-family re-measure (retire the interim named-refusals as they pass); the
  os/signal suite measure (unbanked — may mint a new row on both platforms); GPG.

## v2 amendment (2026-08-27, the os/signal measure) — PERSISTENT registrations carrying Go's sighandler decision

The v1 install-on-Notify/dispose-on-Stop model measured wrong against os/signal's own suite, and in
a way v1's design could not express: **Go installs its runtime handler for every `_SigNotify` signal
at process start (initsig); Notify/Stop toggle FORWARDING (`sig.wanted`), never installation.** The
observable v1 missed: an **unwanted** SIGUSR1/SIGWINCH must be *swallowed* (TestStop sends both
before Notify and after Stop and the process must survive — under v1 the pre-Notify SIGUSR1 hit the
kernel default and killed the whole test host, exit 138, leaving every later test unmeasured), while
an unwanted SIGHUP/SIGINT/SIGQUIT/SIGTERM still dies (`_SigKill` — the default-death shape v1 got
right via disposal). v2 registers ONCE per mapped signal at runtime-assembly load and the handler
makes Go's decision per delivery: `sigsend` delivered → suppress; `signal_ignored` → suppress;
`_SigKill` member → let the default kill; otherwise suppress. `sigdisable` becomes kernel-side
no-op (Go never uninstalls either); Stop/Reset semantics live in the wanted-bit.

Three companion findings, all probe- or measure-backed:
- **The "fixed enum" residual framing was wrong.** .NET's Unix implementation passes a POSITIVE
  `PosixSignal` value through as the raw platform number: `Create((PosixSignal)10)` registers,
  SIGUSR1 delivers, `ctx.Cancel` suppresses the default death. SIGUSR1/SIGUSR2 join the mapped set.
  The honest residual: the CLR-owned synchronous faults, SIGPIPE (registers but does not deliver —
  probed), SIGPROF, the real-time range, SIGKILL/SIGSTOP.
- **Inherited dispositions seed `sig.ignored` at module init** via read-only
  `sigaction(sig, NULL, &old)` — Go's `initsig → sigInitIgnored` analogue; a pure read conflicts
  with nothing the CLR owns. This is what makes a child under nohup answer `Ignored(SIGHUP) == true`
  (TestDetectNohup's second half) and *survive* an uncaught SIGHUP (TestNohup's nohup family) — the
  seed and the handler's ignored-check compose.
- **The test HOST owed `m.Run`'s `flag.Parse()`** (TestFlagBridge.Parse, testing package): a custom
  test flag registered but its value never landed for a TestMain-less package, so TestDetectNohup's
  child re-exec recursed unboundedly. Host-side fix, measured first.

## Q64 amendment (2026-09-05, the job-control trio) — `sigignore` installs the KERNEL `SIG_IGN` for the CLR-free class

**The gap.** Go's `sigignore` is `setsig(sig, _SIG_IGN)` for every `_SigNotify` signal — a KERNEL
disposition. The v2 bridge modelled Ignore as a swallow **in the handler** (`signal_ignored(s)` →
`ctx.Cancel = true`) and `MapPosixSignal` had no entry for SIGTSTP 20 / SIGTTIN 21 / SIGTTOU 22, so
`signal.Ignore(SIGTTOU)` installed **nothing** and the disposition stayed `SIG_DFL`. Two things read
the disposition and neither can see a .NET handler:

- the kernel's `tty_check_change`, which lets a process in a **background** process group run
  `tcsetpgrp`/`TIOCSPGRP` only if SIGTTOU is ignored or blocked, and otherwise delivers SIGTTOU to
  that group — default action **STOP**;
- **exec**, which inherits `SIG_IGN` into a child but resets a handler to `SIG_DFL`.

So `syscall.TestForeground` (`signal.Ignore(SIGTTIN, SIGTTOU)` … `Tcsetpgrp(ttyFD, fpgrp)` after the
child has taken the foreground) STOPPED the converted host under a controlling tty: the mute class in
a `T`-state costume — no handler, no deadline, no results file, the row reading `C#=""`. Q55's
`Setpgid` is correct and merely **exposed** a latent gap (it puts the host in a background group);
GNU `timeout` without `--foreground` exposes the same one. No fleet sweep runs under a tty, which is
why the roster never saw it.

**The fix, and why it is NARROWED.** `sigignore` now installs the real kernel `SIG_IGN` through the
`sys_signal` P/Invoke the file already carried — but only for the **CLR-FREE class**: SIGUSR1 (10),
SIGUSR2 (12) and the job-control trio (20/21/22). The CLR installs handlers of its own for SIGCHLD
(reaping) and SIGINT/SIGCONT/SIGWINCH/SIGTERM (console, shutdown); writing `SIG_IGN` over those would
clobber a live CLR install — the same fact that keeps SIGCHLD out of the eager registration set. For
them the swallow model stays, since it is the correct **delivery** observable that `os/signal`'s own
suite asserts. `MapPosixSignal` gains 20/21/22 for the Notify path, and a new `s_bridgeIgnoredMask`
lets `installPosixSignal` clear a bridge-set `SIG_IGN` back to `SIG_DFL` before `Create` (which is a
no-op over a live `SIG_IGN`), so a **Notify after an Ignore** reinstalls delivery as Go's does; only
the bridge bit is cleared, because the inherited bit feeds the handler's die decision.

**RESIDUAL (stated, not implicit).** A child of this process inherits `SIG_DFL`, not `SIG_IGN`, for an
Ignore'd **CLR-owned** signal. No banked test exercises it, and closing it would mean taking the
signal away from the CLR. Recorded here and at `sigignore`'s else branch.

**Acceptance.** Go ships this seam's own guards, so they are the acceptance rather than a
hand-written probe: `syscall.TestForeground`/`TestForegroundSignal` under a real controlling tty
(direct **and** through the `-test-action compare` pipeline), the exec'd-child disposition probe
reading `1/1/1/1` on linux with an empty Output diff, and the banked `os/signal` (28 + 2) and
`os/exec` (87 + 1) rows as no-regression. The negative control flips
`sigIgnoreInstallsKernelDisposition` off and the probe reads `0` for the trio. A GolibTests arm was
considered and DROPPED: GolibTests references golib only and cannot reach the runtime bridge, so such
an arm would test libc rather than this code.

**The darwin flavour is the same rule** (C2's increment 9), with darwin's 18/21/22 and the same
CLR-free boundary and residual, so the two read as one rule.
