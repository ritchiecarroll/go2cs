// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using System.Collections.Generic;
using System.Runtime.InteropServices;
using go;

// Hand-finished conversion of signal_unix.go's OS-handler-INSTALL layer — sigenable/sigdisable/
// sigignore — over .NET PosixSignalRegistration, the Linux flavor of the os/signal bridge.
//
// WHY. signal_enable/signal_disable/signal_ignore (sigqueue.go, auto-converted and UNTOUCHED) do the
// sig.wanted/ignored bookkeeping and then call one of these three to reach the kernel. The auto bodies
// install Go's own sigtramp via setsig -> sysSigaction -> rt_sigaction, and sigenable/sigdisable first
// hand off to ensureSigM's goroutine over rt_sigprocmask. Both syscalls are unimplemented external
// stubs on the CLR — the CLR OWNS Linux signal handling (its own SIGSEGV/SIGCHLD/SIGTERM handlers,
// signals for GC/thread suspension), and there is no native Go trampoline to install — so every
// signal.Notify/Ignore threw (rt_sigaction) or threw on a background goroutine (rt_sigprocmask). That
// is the os/exec-family wall: TestWaitInterrupt/*, TestSIGQUIT, TestSIGCHLD.
//
// THE BRIDGE, v2 — PERSISTENT registrations carrying Go's sighandler DECISION. Go does not install
// handlers per-Notify: initsig installs the runtime's handler for every _SigNotify signal at PROCESS
// START, and Notify/Stop toggle FORWARDING (sig.wanted), never installation. The observable
// consequences are what os/signal's own suite asserts: an unwanted SIGUSR1/SIGWINCH is SWALLOWED by
// the runtime handler (TestStop sends both before Notify and after Stop, and the process must
// survive), while an unwanted SIGHUP/SIGINT/SIGTERM/SIGQUIT still DIES (_SigKill — the
// default-death-after-Reset shape TestNohup's uncaught family pins). v1 installed on Notify and
// disposed on Stop, which got the deaths right and the swallows fatally wrong — TestStop's
// pre-Notify SIGUSR1 hit the kernel default and took the whole test host down (exit 138), leaving
// every later test unmeasured. v2 therefore registers ONCE, at runtime-assembly load, for the whole
// mapped set, and the handler makes Go's decision per delivery:
//
//     sigsend(s) delivered   -> ctx.Cancel = true   (wanted: os/signal channel has it)
//     signal_ignored(s)      -> ctx.Cancel = true   (user- or inherited-ignored: swallow)
//     _SigKill member        -> ctx.Cancel = false  (die by the signal, like Go's dieFromSignal)
//     otherwise              -> ctx.Cancel = true   (_SigNotify-only: swallow, like Go)
//
// sigsend re-checks sig.wanted itself, which is why the ONE handler serves Notify, Ignore and Stop
// alike: the auto sigqueue bookkeeping above this file remains the single source of truth. The whole
// delivery machinery below sigsend — signal_recv, the note wakeup, the channel — stays auto.
// sigdisable therefore does NOTHING kernel-side (Go never uninstalls either); sigenable/sigignore
// merely ensure the persistent registration exists (a no-op after module init).
//
// ensureSigM and its enableSigChan/maskUpdatedChan handshake are ELIDED, not reimplemented: they were
// the protocol of the rt_sigprocmask goroutine, and PosixSignalRegistration owns its own delivery
// thread and mask. Those members remain in the auto file, now unreferenced.
//
// THE RESIDUAL. v1 called this the PosixSignal enum boundary; that was wrong. The enum members carry
// NEGATIVE values, and .NET's Unix implementation deliberately passes a POSITIVE value through as the
// raw platform signal number — probe-measured 2026-08-27: Create((PosixSignal)10) registers, SIGUSR1
// delivers, ctx.Cancel suppresses the default death. The honest residual is the set the raw cast
// cannot serve: the synchronous faults the CLR owns (SIGILL/SIGABRT/SIGBUS/SIGFPE/SIGSEGV — Go's
// _SigPanic/_SigThrow world), SIGPIPE (registers but does not deliver — .NET handles EPIPE
// internally; probe-measured timeout), SIGPROF (sigenable's own guard), the real-time range, and
// SIGKILL/SIGSTOP (uncatchable everywhere). Tests needing those stay the rt_sigaction disclosure.
//
// PLACEMENT. The three names are registered goosLinux in manualConversionFuncs (manualTypeOperations.go),
// so a Linux -stdlib emission drops the auto bodies to placeholders and this file supplies them; the
// other ~1,440 lines of signal_unix.cs keep reconverting. Darwin's copy stays auto until its own arc.
// Design: docs/phase4/DESIGN-signal-posix-bridge.md (v2 amendment dated 2026-08-27).
//
// Q64 amendment (2026-09-05): sigignore installs the KERNEL SIG_IGN -- Go's setsig(sig, _SIG_IGN) --
// for the CLR-FREE class (SIGUSR1/SIGUSR2 and the job-control trio SIGTSTP/SIGTTIN/SIGTTOU), because
// the kernel's tty layer and an exec'd child consult the DISPOSITION, which a PosixSignalRegistration
// does not carry. Without it a background-pgrp host under a controlling tty was STOPPED by SIGTTOU at
// syscall.TestForeground's restore ioctl -- the mute class in a T-state costume: no handler, no
// deadline, no results file. The CLR-owned signals keep the swallow model; the residual that leaves
// (a child inheriting SIG_DFL for an Ignore'd CLR-owned signal) is stated at sigignore's else branch.
[module: GoManualConversion]

namespace go;

using System;
using System.Diagnostics;
using System.Text;
using go.golib;
using atomic = @internal.runtime.atomic_package;
using @internal;
using @internal.runtime;

partial class runtime_package
{
    // The persistent registrations, keyed by system signal number. Created at module init for the
    // whole mapped set and NEVER disposed — Go never uninstalls its runtime handler either. The lock
    // guards the once-only install against a converted caller reaching signal_enable concurrently.
    private static readonly object s_sigPosixLock = new object();
    private static readonly Dictionary<int, PosixSignalRegistration> s_sigPosixRegs = new Dictionary<int, PosixSignalRegistration>();

    // libc signal(2), used only to clear an inherited SIG_IGN so .NET will install its handler.
    // SIG_DFL is the null handler on Linux.
    private static readonly IntPtr SIG_DFL = IntPtr.Zero;
    [DllImport("libc", EntryPoint = "signal", SetLastError = true)]
    private static extern IntPtr sys_signal(int signum, IntPtr handler);

    // The libc sigaction READ side (act = NULL), used only to observe dispositions this process
    // INHERITED — a pure read conflicts with nothing the CLR owns, unlike the install side the
    // bridge exists to route. glibc's struct sigaction leads with the sa_handler union on linux-x64;
    // 160 bytes generously covers the 152-byte struct. SIG_IGN is (void*)1.
    [DllImport("libc", EntryPoint = "sigaction", SetLastError = false)]
    private static extern int sys_sigaction_read(int signum, IntPtr act, IntPtr oldact);

    private static readonly IntPtr SIG_IGN_HANDLER = (IntPtr)1;

    // The dispositions this process INHERITED, snapshotted at module init before anything repaints
    // them. This is the bridge's fwdSig: Go's sigdisable RESTORES the pre-Go disposition when the
    // handler is no longer needed (sigInstallGoHandler false -> setsig(fwdSig)), so a post-Stop
    // SIGHUP under nohup lands on the restored SIG_IGN and the process survives — TestNohup's
    // nohup/2 shape. sig.ignored cannot carry this fact through a Notify (signal_enable clears the
    // bit, correctly), so the handler's die decision consults the snapshot instead of re-dying on a
    // signal the process was born ignoring.
    private static uint s_inheritedIgnoredMask;

    // The signals this bridge itself set to the kernel SIG_IGN in sigignore (the CLR-FREE class
    // below). Consulted by installPosixSignal, which clears the disposition back to SIG_DFL before
    // .NET Creates a handler -- Create is a no-op over a live SIG_IGN -- so a Notify AFTER an Ignore
    // reinstalls delivery, as Go's does. Distinct from s_inheritedIgnoredMask, whose bits feed the
    // handler's die decision and are never cleared.
    private static uint s_bridgeIgnoredMask;

    // MapPosixSignal maps a Linux/amd64 signal number to the .NET PosixSignal value that carries it,
    // or null for the residual. Numbers are the stable Linux ABI values mirrored by
    // defs_linux_amd64.cs. Positive values ride .NET's raw-number pass-through (see THE RESIDUAL in
    // the file header); the named members are kept for the signals that have them.
    private static PosixSignal? MapPosixSignal(uint32 sig)
    {
        switch ((int)sig)
        {
            case 1:  return PosixSignal.SIGHUP;
            case 2:  return PosixSignal.SIGINT;
            case 3:  return PosixSignal.SIGQUIT;
            case 10: return (PosixSignal)10;  // SIGUSR1, raw platform number
            case 12: return (PosixSignal)12;  // SIGUSR2, raw platform number
            case 15: return PosixSignal.SIGTERM;
            case 17: return PosixSignal.SIGCHLD;
            case 18: return PosixSignal.SIGCONT;
            case 20: return (PosixSignal)20;  // SIGTSTP, raw platform number
            case 21: return (PosixSignal)21;  // SIGTTIN, raw platform number
            case 22: return (PosixSignal)22;  // SIGTTOU, raw platform number
            case 28: return PosixSignal.SIGWINCH;
            default: return null;
        }
    }

    // Whether an UNWANTED, un-ignored delivery of sig kills the process — Go's _SigKill (SIGQUIT
    // carries _SigThrow, but without a Go traceback to print the observable is the same death by
    // signal). Linux sigtab rows for the mapped set: HUP/INT/QUIT/TERM die; USR1/USR2/CHLD/CONT/
    // WINCH are _SigNotify-only and are swallowed.
    private static bool sigDiesByDefault(uint32 sig)
    {
        switch ((int)sig)
        {
            case 1:
            case 2:
            case 15:
                return true;
            default:
                return false;
        }
    }

    // Go's _SigThrow disposition. sigtab_linux_generic.go marks SIGQUIT
    // `{_SigNotify + _SigThrow, "SIGQUIT: quit"}` -- NOT _SigKill -- and signal_unix.go's
    // sighandler acts on exactly that difference: the `if flags&_SigKill != 0 { dieFromSignal(sig) }`
    // arm is skipped, the comment "_SigThrow means that we should exit now" applies, the traceback
    // is printed, and under the DEFAULT GOTRACEBACK the runtime reaches `exit(2)` (panic.go).
    // Dying BY the signal happens only under GOTRACEBACK=crash. So a Go program sent SIGQUIT is an
    // ordinary EXITED process with status 2 that left a goroutine dump on stderr -- which is
    // precisely what os/exec's TestWaitInterrupt/SIGQUIT asserts (ps.Exited(), ExitCode() == 2,
    // and stderr containing a blank line followed by "goroutine ").
    //
    // Scoped to SIGQUIT ALONE. Go's _SigThrow set also carries the synchronous faults
    // (SIGILL/SIGABRT/SIGFPE/SIGSEGV/SIGBUS); those stay OUT because the CLR owns them, they are
    // the residual this file's header already declares, and a PosixSignalRegistration cannot take
    // them from it. SIGQUIT is the member a program actually receives asynchronously.
    private static bool sigThrowsByDefault(uint32 sig)
    {
        return (int)sig == 3;
    }

    // The report Go writes before exiting on an unwanted _SigThrow, in the TRUTHFUL HALF the
    // managed runtime can actually produce (coordinator ruling, 2026-08-28: the truthful half,
    // never a fabricated whole).
    //
    // WHAT IS HONEST HERE, precisely. Go dumps EVERY goroutine's stack. The CLR has no supported
    // cross-thread stack walk -- the same limit runtime.Stack's `all` parameter already documents
    // and declines in managed_impl.cs -- so the only frames capturable are those on the thread
    // doing the capturing, and here that is .NET's signal-dispatch thread, NOT a goroutine.
    // This is therefore the one place the corpus must NOT reuse its usual "goroutine 1 [running]:"
    // header: runtime.Stack writes that because ITS caller genuinely is the goroutine, whereas
    // writing it here would name frames as a goroutine's that demonstrably are not. The header
    // says what the block actually is; the live goroutine COUNT is reported because the registry
    // genuinely knows it; and the goroutines whose stacks cannot be walked are declared MISSING
    // rather than reconstructed.
    //
    // The dump is best effort; the exit status is not. A report that cannot be written must still
    // leave an EXITED process with status 2, because that is the half of Go's contract this can
    // always honor.
    private static void throwFromSignal(uint32 sig)
    {
        try
        {
            StringBuilder report = new();

            // Go's own text for this signal, then the BLANK line its traceback follows.
            report.Append("SIGQUIT: quit\n\n");
            report.Append("goroutine (signal handler) [running]:\n");
            appendGoFrames(report, new StackTrace(fNeedFileInfo: true));

            int live = Goroutine.Count;

            if (live > 0)
            {
                report.Append("[go2cs] ").Append(live)
                      .Append(live == 1 ? " goroutine was live; " : " goroutines were live; ")
                      .Append("the managed runtime cannot capture another thread's stack\n")
                      .Append("        in-process, so their frames are absent rather than reconstructed.\n");
            }

            Console.Error.Write(report.ToString());
            Console.Error.Flush();
        }
        catch
        {
            // Nothing further can be said; the exit below is the part that must happen.
        }

        Environment.Exit(2);
    }
    // installPosixSignal (called under s_sigPosixLock) creates the PERSISTENT registration for sig
    // if it does not exist yet. The handler makes Go's sighandler decision per delivery — see the
    // file header. Go's signal.Notify OVERRIDES an inherited SIG_IGN (setsig installs
    // unconditionally); .NET's PosixSignalRegistration RESPECTS it and won't install a handler for a
    // signal it saw ignored, so an INHERITED SIG_IGN — and ONLY that — is cleared to SIG_DFL first.
    // The guard is load-bearing, not an optimization (strace-measured 2026-08-27): .NET installs its
    // own native handlers for the signals it tracks (SIGINT for Console, SIGCONT for terminal
    // reinit, SIGCHLD for process reaping) and its per-signal enable is refcounted, so an
    // UNCONDITIONAL signal(sig, SIG_DFL) here CLOBBERED a live .NET install and the following
    // Create was a native no-op — the disposition stayed SIG_DFL and delivery never reached any
    // handler (TestSIGCONT's timeout; TestNotifyContextNotifications' child dying from a SIGINT it
    // had asked for). For the inherited-IGN case the clear is safe by the same fact: .NET respected
    // that SIG_IGN and installed nothing there to clobber. Module init has already seeded
    // sig.ignored from the same read, so nothing observable is lost: an inherited-ignored SIGHUP
    // keeps surviving (the handler swallows via signal_ignored) exactly as under Go.
    private static void installPosixSignal(uint32 sig, PosixSignal ps)
    {
        int key = (int)sig;
        if (s_sigPosixRegs.ContainsKey(key))
        {
            return;
        }
        uint32 s = sig;
        if ((((s_inheritedIgnoredMask | s_bridgeIgnoredMask) >> (int)sig) & 1) != 0)
        {
            sys_signal((int)sig, SIG_DFL);
        }

        // No longer ignored once a delivery handler exists. Only the BRIDGE bit is cleared: the
        // inherited bit is what the handler's die decision consults and must survive.
        s_bridgeIgnoredMask &= ~(1u << (int)sig);
        s_sigPosixRegs[key] = PosixSignalRegistration.Create(ps, ctx =>
        {
            if (sigsend(s))
            {
                ctx.Cancel = true;      // wanted: delivered to the os/signal queue
            }
            else if (signal_ignored(s))
            {
                ctx.Cancel = true;      // user- or inherited-ignored: swallow
            }
            else if (sigThrowsByDefault(s) && ((s_inheritedIgnoredMask >> (int)s) & 1) == 0)
            {
                // Unwanted _SigThrow: Go does NOT die by this signal -- it reports and
                // exits 2. Cancel=true because THIS process owns the death; letting the
                // kernel default also fire would replace the very exit status the report
                // is about.
                ctx.Cancel = true;
                throwFromSignal(s);
            }
            else if (sigDiesByDefault(s) && ((s_inheritedIgnoredMask >> (int)s) & 1) == 0)
            {
                ctx.Cancel = false;     // unwanted _SigKill: die by the signal, like Go
            }
            else
            {
                // Unwanted _SigNotify-only, or a _SigKill signal the process was born ignoring
                // (Go restores that inherited SIG_IGN on Stop — fwdSig): swallow, like Go.
                ctx.Cancel = true;
            }
        });
    }

    // Seeds runtime.sig.ignored with the dispositions this process INHERITED, the way Go's initsig
    // does (getsig(i) == _SIG_IGN -> sigInitIgnored(i)), then installs the persistent registrations
    // for the whole mapped set — Go's initsig moment, at runtime-assembly load: before any user code
    // can ask os/signal.Ignored, send a signal, or Notify. Seeding MUST precede installing, because
    // installing clears an inherited SIG_IGN disposition to let .NET take the signal.
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void InitPosixSignalBridge()
    {
        if (!OperatingSystem.IsLinux())
            return;

        IntPtr old = Marshal.AllocHGlobal(160);
        try
        {
            // The classic asynchronous range. 9/19 are SIGKILL/SIGSTOP (their dispositions cannot
            // differ from default); 32/33 are NPTL-reserved and not observable signals.
            for (int signum = 1; signum <= 31; signum++)
            {
                if (signum == 9 || signum == 19)
                    continue;

                if (sys_sigaction_read(signum, IntPtr.Zero, old) != 0)
                    continue;

                if (Marshal.ReadIntPtr(old) == SIG_IGN_HANDLER)
                {
                    sigInitIgnored((uint32)signum);
                    s_inheritedIgnoredMask |= 1u << signum;
                }
            }
        }
        catch
        {
            // Defensive: an unreadable disposition just leaves that signal reported non-ignored,
            // which is where the mask already was.
        }
        finally
        {
            Marshal.FreeHGlobal(old);
        }

        lock (s_sigPosixLock)
        {
            // SIGCHLD (17) is deliberately ABSENT from the eager set, and the omission is
            // measured, not stylistic: with an eager SIGCHLD registration, os/exec's full
            // parallel suite SEGFAULTED in bundled-native runtime frames (crash-on-first-run,
            // reproducible; core's faulting thread parked in coreclr's own crash machinery, the
            // interrupted context unsymbolized runtime internals) — every child exit fired the
            // extra managed dispatch concurrently with the CLR's own SIGCHLD reaping under spawn
            // storms. Without it: four consecutive full-suite runs clean (2026-08-28). The
            // omission costs NO Go observable — SIGCHLD's default action IS ignore, so eager
            // swallow and kernel default are indistinguishable — and signal.Notify(SIGCHLD)
            // still installs on demand through sigenable (os/exec's TestSIGCHLD is the measured
            // consumer). The suspected runtime-internal dispatch/reap race is noted for
            // upstream; the bridge simply declines to enter it for a signal that gains nothing.
            foreach (uint32 sig in new uint32[] { 1, 2, 3, 10, 12, 15, 18, 28 })
            {
                PosixSignal? ps = MapPosixSignal(sig);
                if (ps is not null)
                {
                    installPosixSignal(sig, ps.Value);
                }
            }
        }
    }

    // sigenable enables the Go signal handler to catch the signal sig.
    // It is only called while holding the os/signal.handlers lock,
    // via os/signal.enableSignal and signal_enable. The persistent registration already exists for
    // the mapped set; this merely guarantees it (idempotent) and keeps handlingSig's book. The
    // wanted-bit the caller just set is what flips the handler's decision to delivery.
    internal static void sigenable(uint32 sig)
    {
        if (sig >= (uint32)len(sigtable))
        {
            return;
        }
        // SIGPROF is handled specially for profiling.
        if (sig == _SIGPROF)
        {
            return;
        }
        var t = Ꮡsigtable.at<sigTabT>((nint)(sig));
        if ((int32)((~t).flags & (int32)_SigNotify) != 0)
        {
            PosixSignal? ps = MapPosixSignal(sig);
            if (ps is null)
            {
                return; // residual: no honest .NET carrier — stays the rt_sigaction refusal
            }
            lock (s_sigPosixLock)
            {
                atomic.Cas(ᏑhandlingSig.at<uint32>((nint)(sig)), 0, 1);
                installPosixSignal(sig, ps.Value);
            }
        }
    }

    // sigdisable disables the Go signal handler for the signal sig.
    // It is only called while holding the os/signal.handlers lock,
    // via os/signal.disableSignal and signal_disable. Kernel-side this is a deliberate NO-OP: Go
    // never uninstalls its runtime handler either — Stop/Reset semantics live entirely in the
    // wanted-bit the caller just cleared, which routes the next delivery to the handler's
    // default decision (die for the _SigKill set, swallow otherwise), exactly Go's
    // default-handling-after-Reset observable.
    internal static void sigdisable(uint32 sig)
    {
        if (sig >= (uint32)len(sigtable))
        {
            return;
        }
        if (sig == _SIGPROF)
        {
            return;
        }
        var t = Ꮡsigtable.at<sigTabT>((nint)(sig));
        if ((int32)((~t).flags & (int32)_SigNotify) != 0)
        {
            lock (s_sigPosixLock)
            {
                atomic.Store(ᏑhandlingSig.at<uint32>((nint)(sig)), 0);
            }
        }
    }

    // sigignore ignores the signal sig.
    // It is only called while holding the os/signal.handlers lock,
    // via os/signal.ignoreSignal and signal_ignore. The caller has already set sig.ignored and
    // cleared sig.wanted, which is the whole observable: the persistent handler swallows the next
    // delivery via signal_ignored. Guaranteeing the registration here (idempotent) is what makes
    // Ignore suppress a signal whose default would kill.
    internal static void sigignore(uint32 sig)
    {
        if (sig >= (uint32)len(sigtable))
        {
            return;
        }
        if (sig == _SIGPROF)
        {
            return;
        }
        var t = Ꮡsigtable.at<sigTabT>((nint)(sig));
        if ((int32)((~t).flags & (int32)_SigNotify) != 0)
        {
            lock (s_sigPosixLock)
            {
                atomic.Store(ᏑhandlingSig.at<uint32>((nint)(sig)), 0);

                if (sigIgnoreInstallsKernelDisposition(sig))
                {
                    // Go's sigignore IS setsig(sig, _SIG_IGN) -- a KERNEL disposition, and the kernel is
                    // what consults it: tty_check_change lets a process in a BACKGROUND process group
                    // run tcsetpgrp/TIOCSPGRP only if SIGTTOU is ignored (SIG_IGN) or blocked, else it
                    // delivers SIGTTOU to that group, whose default action is STOP. exec inherits the
                    // disposition too. A .NET PosixSignalRegistration is a HANDLER, not SIG_IGN: the
                    // kernel does not treat it as ignored and exec resets it to SIG_DFL -- so the
                    // swallow-in-handler model left this class at SIG_DFL and syscall.TestForeground's
                    // restore ioctl STOPPED a background-pgrp host under a controlling tty (Q64; Q55's
                    // Setpgid is correct and merely EXPOSED the latent gap). Drop any live registration
                    // first -- the eager SIGUSR1 one -- so a later sigenable can re-Create a handler.
                    if (s_sigPosixRegs.TryGetValue((int)sig, out PosixSignalRegistration existing))
                    {
                        existing.Dispose();
                        s_sigPosixRegs.Remove((int)sig);
                    }

                    sys_signal((int)sig, SIG_IGN_HANDLER);
                    s_bridgeIgnoredMask |= 1u << (int)sig;
                }
                else
                {
                    // A CLR-OWNED signal. Setting the kernel SIG_IGN here would clobber a live CLR
                    // handler -- the same fact that keeps SIGCHLD out of the eager set -- so keep the
                    // swallow model, which is the DELIVERY observable os/signal's own suite asserts.
                    // RESIDUAL, stated rather than left implicit: a child of this process inherits
                    // SIG_DFL, not SIG_IGN, for such a signal after an Ignore. No banked test exercises
                    // it, and closing it would mean taking the signal away from the CLR.
                    PosixSignal? ps = MapPosixSignal(sig);
                    if (ps is not null)
                    {
                        installPosixSignal(sig, ps.Value);
                    }
                }
            }
        }
    }

    // The signals whose Ignore installs the KERNEL SIG_IGN rather than the swallow-in-handler model:
    // the user signals a child reads back across exec (SIGUSR1 10, SIGUSR2 12) and the job-control
    // trio the tty layer consults at tcsetpgrp (SIGTSTP 20, SIGTTIN 21, SIGTTOU 22). Every one is
    // CLR-FREE -- the CLR installs no handler of its own on them -- so SIG_IGN cannot clobber a live
    // CLR install, unlike SIGCHLD (reaping) or SIGINT/SIGCONT/SIGWINCH (console). Go's Ignore is
    // SIG_IGN for EVERY _SigNotify signal; the bridge narrows the kernel-disposition install to the
    // class it can serve without taking a signal away from the CLR, and states the residual for the
    // rest at sigignore's else branch.
    private static bool sigIgnoreInstallsKernelDisposition(uint32 sig)
        => sig == 10 || sig == 12 || sig == 20 || sig == 21 || sig == 22;
}
