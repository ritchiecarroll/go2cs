// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using System.Collections.Generic;
using System.Runtime.InteropServices;
using go;

// Hand-finished conversion of signal_unix.go's OS-handler-INSTALL layer — sigenable/sigdisable/
// sigignore — over .NET PosixSignalRegistration, the DARWIN flavor of the os/signal bridge
// (docs/phase4/DESIGN-signal-posix-bridge-darwin.md, Q52; the linux flavor and the design of record it
// extends: runtime/linux/signal_posix_impl.cs, DESIGN-signal-posix-bridge.md v2). A DISTINCT basename
// by rule: the L3 layout places one hand-own per logical name only when an emitted principal exists
// and refuses two differing same-basename copies, and `signal_posix` has no principal.
//
// WHY. signal_enable/signal_disable/signal_ignore (sigqueue.go, auto-converted and UNTOUCHED) do the
// sig.wanted/ignored bookkeeping and then call one of these three to reach the kernel. The auto bodies
// install Go's own sigtramp via setsig -> sigaction — and on darwin `setsig` wants
// abi.FuncPCABI0(sigtramp), a program counter for an ASSEMBLY trampoline the kernel would jump to on
// delivery, which no libc body can supply: FuncPCABI0 panics by name there, which is exactly where
// both mac legs of SignalPrimitives died since increment 6 (train 27, exit 2 / stderr 20 / stdout 2).
// The door is the REVERSE direction of every darwin increment before it — the kernel calling INTO
// managed code on an arbitrary thread — and it collides with the CLR's own handler chain. The bridge
// installs no trampoline at all: .NET owns the kernel side and a managed callback makes Go's
// per-delivery decision, on a threadpool thread, never in a signal context.
//
// THE BRIDGE — PERSISTENT registrations carrying Go's sighandler DECISION, exactly the linux v2 shape
// (its header carries the measured history: v1's per-Notify install got the swallows fatally wrong —
// TestStop's pre-Notify SIGUSR1 took the whole host down). Go installs at initsig for every _SigNotify
// signal at PROCESS START and Notify/Stop toggle FORWARDING (sig.wanted), never installation; so the
// bridge registers ONCE, at runtime-assembly load, for the whole mapped set, and the handler decides
// per delivery:
//
//     sigsend(s) delivered   -> ctx.Cancel = true   (wanted: os/signal channel has it)
//     signal_ignored(s)      -> ctx.Cancel = true   (user- or inherited-ignored: swallow)
//     _SigThrow member       -> report, exit 2      (SIGQUIT alone; the faults stay the CLR's)
//     _SigKill member        -> ctx.Cancel = false  (die by the signal, like Go's dieFromSignal)
//     otherwise              -> ctx.Cancel = true   (_SigNotify-only: swallow, like Go)
//
// WHAT DIFFERS FROM LINUX, clause by clause (the design's §3 table): the NUMBER MAP is darwin's
// (USR1 30, USR2 31, CHLD 20, CONT 19, WINCH 28, INFO 29, and since increment 9 the job-control trio
// TSTP 18, TTIN 21, TTOU 22 — defs_darwin_amd64.cs; HUP/INT/QUIT/TERM share the classic numbers);
// the _SigKill/_SigThrow SETS are read from signal_darwin.go's sigtable
// (kill = HUP, INT, TERM; throw = QUIT, ABRT — ABRT stays the CLR's); the INHERITED-DISPOSITION SEED
// is read through increment 6's real sigaction body (GoSigactionQuery — the 16-byte darwin mirror,
// the handler word 1 == SIG_IGN) instead of a raw 160-byte glibc P/Invoke; and ensureSigM /
// pthread_sigmask (increment 5) are elided the same way, because the registration owns its own
// thread and mask. The install side of increment 6's sigaction serves the bridge since increment 9
// (the restore after an Ignore, below) and stays the truth for dieFromSignal's setsig(sig, _SIG_DFL)
// — the SIGPIPE death path, where darwin is AHEAD of linux (§3.4).
//
// INCREMENT 9 (Q64's darwin half, 2026-09-05 — ONE RULE with the linux flavour, C1's Q64): Go's
// Ignore is a KERNEL disposition. sigignore's first form only guaranteed the registration and let
// the handler swallow — a managed cancel that the kernel never consults at tcsetpgrp (the
// job-control mute-hang class) and that no exec'd child inherits, where Go's setsig(sig, _SIG_IGN)
// is both. It now installs the kernel SIG_IGN through the signal(2) P/Invoke below FOR THE
// CLR-FREE CLASS ONLY — the mapped signals no CLR handler owns (sigIsKernelIgnorable: SIGUSR2 and
// the job-control trio here; USR1/USR2 and the trio on linux, darwin's class differing by the one
// member SIGUSR1, the CLR's activation-injection signal on this platform) — because a kernel
// SIG_IGN on a CLR-owned signal would clobber a live CLR handler, the same fact that keeps SIGCHLD
// out of the eager set. The CLR-owned set keeps the swallow model, and its RESIDUAL is stated
// rather than left implicit: a child exec'd while such a signal is Ignore'd inherits SIG_DFL,
// where Go's child inherits SIG_IGN. For the class, the SIG_IGN is installed after SAVING the
// sigaction it displaces (.NET's own SA_SIGINFO handler for a registered signal; SIG_DFL or the
// inherited disposition for one not yet registered), and sigenable — Go's Notify, the ONLY call
// that undoes an Ignore: Reset does not, sigdisable reaches the kernel solely in its
// !sigInstallGoHandler arm — puts that sigaction back verbatim through increment 6's install arm
// before the registration is guaranteed, so .NET's handler is live again with its flags (the
// linux flavour reinstalls through its ignored-mask and a SIG_DFL clear before Create: the same
// contract, one mechanism per flavour, stated). The map gains the job-control trio (SIGTSTP 18,
// SIGTTIN 21, SIGTTOU 22), which Go's initsig deliberately skips (_SigDefault) and Notify installs
// on demand — the on-demand path sigenable already is. The unmapped residual is unchanged.
//
// THE RESIDUAL, per flavor: the synchronous faults the CLR owns (SIGILL/SIGTRAP/SIGBUS/SIGFPE/SIGSEGV
// — Go never delivers them to Notify either, so a no-op is Go-compatible by construction), SIGABRT
// (_SigNotify + _SigThrow, the CLR's abort path), SIGEMT/SIGSYS (_SigThrow, no Notify on Go's side),
// SIGPIPE (registers, does not deliver — probed on linux), SIGPROF (ruled a disclosed residual on
// both flavors until a profiler-signal design exists), SIGKILL/SIGSTOP. The DISPOSITION LOG of §2.3
// — the set of signals whose inherited handler is neither SIG_DFL nor SIG_IGN, i.e. the CLR's own
// catches — is written to stderr ONCE at init ONLY under GO2CS_SIGNAL_DISPOSITION_LOG=1: an
// unconditional line would put a byte on every converted program's stderr and break the very
// behavioral-stderr acceptance (stderr 0) the bridge is measured by. That gating is a stated
// refinement of the design, not a deviation from what it asks for.
//
// PLACEMENT. The three names are registered goosLinuxDarwin in manualConversionFuncs
// (manualTypeOperations.go), so a darwin -stdlib emission drops the auto bodies to placeholders and
// this file supplies them; the other ~1,400 lines of signal_unix.cs keep reconverting. Shared logic
// becomes a flat core file only AFTER this second flavor is measured (COORD 493a41bf7), not before.
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

    // libc signal(2), used to clear an inherited SIG_IGN so .NET will install its handler and — since
    // increment 9 — to install Go's Ignore as the KERNEL disposition. SIG_DFL is the null handler on
    // darwin as on linux; SIG_IGN is 1.
    private static readonly IntPtr SIG_DFL = IntPtr.Zero;
    private static readonly IntPtr SIG_IGN = (IntPtr)1;
    [DllImport("libc", EntryPoint = "signal", SetLastError = true)]
    private static extern IntPtr sys_signal(int signum, IntPtr handler);

    // The dispositions this process INHERITED, snapshotted at module init before anything repaints
    // them — the bridge's fwdSig: Go's sigdisable RESTORES the pre-Go disposition when the handler is
    // no longer needed, so a post-Stop SIGHUP under nohup lands on the restored SIG_IGN and the
    // process survives (TestNohup's shape). sig.ignored cannot carry this fact through a Notify
    // (signal_enable clears the bit, correctly), so the handler's die decision consults the snapshot.
    private static uint s_inheritedIgnoredMask;

    // The signals whose KERNEL disposition this bridge set to SIG_IGN on Go's behalf (sigignore), and
    // the sigaction each displaced, put back verbatim by sigenable — Go's fwdSig discipline applied
    // to the one handler the bridge cannot reinstall by itself, .NET's own. Both under s_sigPosixLock.
    private static uint s_kernelIgnoredMask;
    private static readonly (ulong handler, uint32 mask, int32 flags)[] s_displacedActions = new (ulong, uint32, int32)[32];

    // Darwin's signal numbers (defs_darwin_amd64.cs), the classic asynchronous range this bridge maps.
    // Named .NET members are translated by the runtime; a POSITIVE value rides .NET's raw-number
    // pass-through (the linux bridge's measured residual note: the enum members are NEGATIVE, a
    // positive value is the platform number).
    private const int sigHUP = 1, sigINT = 2, sigQUIT = 3, sigTERM = 15, sigTSTP = 18, sigCONT = 19,
                      sigCHLD = 20, sigTTIN = 21, sigTTOU = 22, sigWINCH = 28, sigINFO = 29,
                      sigUSR1 = 30, sigUSR2 = 31;

    // The CLR-FREE class — the mapped signals no CLR handler owns, so Go's kernel SIG_IGN can be
    // installed without clobbering a live CLR handler (the same fact that keeps SIGCHLD out of the
    // eager set): SIGUSR2 and the job-control trio. The linux flavour's class is USR1/USR2 + the
    // trio; darwin's differs by ONE member, because SIGUSR1 is the CLR's activation-injection signal
    // here (coreclr pal/src/exception/signal.cpp: INJECT_ACTIVATION_SIGNAL is SIGRTMIN where defined
    // and SIGUSR1 where not, and darwin defines no SIGRTMIN; installed at PAL init, accepting the
    // process's own activations) — a kernel SIG_IGN there would discard the runtime's GC-suspension
    // activations. Everything else mapped — HUP/INT/QUIT/TERM (the PAL's console and exit handlers),
    // CHLD (reaping), CONT/WINCH (terminal), INFO (default-ignore, so the swallow is exact) and
    // USR1 — keeps the swallow model, with the residual stated in the header.
    private static bool sigIsKernelIgnorable(uint32 sig)
    {
        switch ((int)sig)
        {
            case sigUSR2:
            case sigTSTP:
            case sigTTIN:
            case sigTTOU:
                return true;
            default:
                return false;
        }
    }

    // MapPosixSignal maps a darwin signal number to the .NET PosixSignal value that carries it, or
    // null for the residual.
    private static PosixSignal? MapPosixSignal(uint32 sig)
    {
        switch ((int)sig)
        {
            case sigHUP:   return PosixSignal.SIGHUP;
            case sigINT:   return PosixSignal.SIGINT;
            case sigQUIT:  return PosixSignal.SIGQUIT;
            case sigTERM:  return PosixSignal.SIGTERM;
            case sigTSTP:  return PosixSignal.SIGTSTP;   // job-control trio: on demand only (Go's _SigDefault)
            case sigCONT:  return PosixSignal.SIGCONT;
            case sigCHLD:  return PosixSignal.SIGCHLD;
            case sigTTIN:  return PosixSignal.SIGTTIN;
            case sigTTOU:  return PosixSignal.SIGTTOU;
            case sigWINCH: return PosixSignal.SIGWINCH;
            case sigINFO:  return (PosixSignal)sigINFO;  // darwin-only, raw platform number
            case sigUSR1:  return (PosixSignal)sigUSR1;  // raw platform number
            case sigUSR2:  return (PosixSignal)sigUSR2;  // raw platform number
            default:       return null;
        }
    }

    // Whether an UNWANTED, un-ignored delivery of sig kills the process — Go's _SigKill, read from
    // signal_darwin.go's sigtable: HUP/INT/TERM die; USR1/USR2/CHLD/CONT/WINCH/INFO and the
    // job-control trio TSTP/TTIN/TTOU are _SigNotify(+_SigIgn/_SigDefault) and are swallowed — for
    // the trio exactly Go's installed handler, which returns on an unwanted delivery: initsig never
    // installs it (so before a Notify the kernel default, stop, applies — and so it does here, the
    // trio being absent from the eager set), and after a Notify Go keeps it, as the registration
    // is kept.
    private static bool sigDiesByDefault(uint32 sig)
    {
        switch ((int)sig)
        {
            case sigHUP:
            case sigINT:
            case sigTERM:
                return true;
            default:
                return false;
        }
    }

    // Go's _SigThrow disposition, scoped to SIGQUIT ALONE (signal_darwin.go: `{_SigNotify +
    // _SigThrow, "SIGQUIT: quit"}`). Under the default GOTRACEBACK the runtime prints the traceback
    // and reaches exit(2) — an EXITED process with status 2, which is what os/exec's
    // TestWaitInterrupt/SIGQUIT asserts. SIGABRT carries the same flags in Go's table and stays OUT:
    // the CLR's abort path owns it, and a PosixSignalRegistration cannot take it from it.
    private static bool sigThrowsByDefault(uint32 sig)
    {
        return (int)sig == sigQUIT;
    }

    // The report Go writes before exiting on an unwanted _SigThrow, in the TRUTHFUL HALF the managed
    // runtime can produce (the linux bridge's ruling, 2026-08-28): the frames of the thread doing the
    // capturing — .NET's signal-dispatch thread, NOT a goroutine, and the header says so — the live
    // goroutine count the registry genuinely knows, and the goroutines whose stacks cannot be walked
    // declared MISSING rather than reconstructed. The dump is best effort; the exit status is not.
    private static void throwFromSignal(uint32 sig)
    {
        try
        {
            StringBuilder report = new();

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
    // if it does not exist yet. Go's signal.Notify OVERRIDES an inherited SIG_IGN (setsig installs
    // unconditionally); .NET's PosixSignalRegistration RESPECTS it and won't install a handler for a
    // signal it saw ignored, so an INHERITED SIG_IGN — and ONLY that — is cleared to SIG_DFL first.
    // The guard is load-bearing (the linux bridge measured it under strace): .NET installs its own
    // native handlers for the signals it tracks and refcounts its per-signal enable, so an
    // UNCONDITIONAL signal(sig, SIG_DFL) clobbers a live .NET install and the Create that follows is
    // a native no-op. Module init has already seeded sig.ignored from the same read, so nothing
    // observable is lost: an inherited-ignored SIGHUP keeps surviving (the handler swallows via
    // signal_ignored) exactly as under Go.
    private static void installPosixSignal(uint32 sig, PosixSignal ps)
    {
        int key = (int)sig;
        if (s_sigPosixRegs.ContainsKey(key))
        {
            return;
        }
        uint32 s = sig;
        if (((s_inheritedIgnoredMask >> (int)sig) & 1) != 0)
        {
            sys_signal((int)sig, SIG_DFL);
        }
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
                // Unwanted _SigThrow: Go does NOT die by this signal -- it reports and exits 2.
                // Cancel=true because THIS process owns the death.
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
    // does (getsig(i) == _SIG_IGN -> sigInitIgnored(i)) — read through increment 6's sigaction body,
    // the 16-byte darwin mirror, never a raw struct — then installs the persistent registrations for
    // the mapped set: Go's initsig moment, at runtime-assembly load. Seeding MUST precede installing,
    // because installing clears an inherited SIG_IGN disposition to let .NET take the signal.
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void InitPosixSignalBridge()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        StringBuilder? caught = Environment.GetEnvironmentVariable("GO2CS_SIGNAL_DISPOSITION_LOG") == "1" ? new StringBuilder() : null;

        // The classic asynchronous range. 9/17 are SIGKILL/SIGSTOP on darwin (their dispositions
        // cannot differ from default).
        for (int signum = 1; signum <= 31; signum++)
        {
            if (signum == 9 || signum == 17)
                continue;

            ulong handler;
            try
            {
                (handler, _, _) = GoSigactionQuery(signum);
            }
            catch
            {
                // Defensive: an unreadable disposition just leaves that signal reported non-ignored,
                // which is where the mask already was.
                continue;
            }

            if (handler == 1)
            {
                sigInitIgnored((uint32)signum);
                s_inheritedIgnoredMask |= 1u << signum;
            }
            else if (handler != 0 && caught is not null)
            {
                caught.Append(caught.Length == 0 ? "" : " ").Append(signum);
            }
        }

        if (caught is not null)
        {
            // The design's §2.3 log: the CLR's own catches, so the mac runners STATE their map.
            Console.Error.WriteLine("[go2cs] darwin signal bridge: inherited handlers on {" + caught + "}; inherited SIG_IGN mask 0x" + s_inheritedIgnoredMask.ToString("x"));
        }

        lock (s_sigPosixLock)
        {
            // SIGCHLD is deliberately ABSENT from the eager set, for the linux bridge's measured
            // reason (an eager SIGCHLD registration raced the CLR's own reaping under os/exec spawn
            // storms and segfaulted in runtime frames); its default action IS ignore, so eager swallow
            // and kernel default are indistinguishable, and signal.Notify(SIGCHLD) still installs on
            // demand through sigenable. SIGINFO is left to on-demand for the same reason (_SigIgn by
            // default). Same set as linux, darwin's numbers.
            foreach (uint32 sig in new uint32[] { sigHUP, sigINT, sigQUIT, sigTERM, sigCONT, sigWINCH, sigUSR1, sigUSR2 })
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
                return; // residual: no honest .NET carrier — stays disclosed
            }
            lock (s_sigPosixLock)
            {
                // Notify undoes an Ignore (Go's sigenable installs over the SIG_IGN sigignore left):
                // put back the sigaction the kernel SIG_IGN displaced — .NET's own handler, flags
                // and all, which signal(2) could not reinstall — BEFORE the registration is
                // guaranteed, so a first-time Create sees SIG_DFL rather than our SIG_IGN.
                restoreDisplacedDisposition(sig);
                atomic.Cas(ᏑhandlingSig.at<uint32>((nint)(sig)), 0, 1);
                installPosixSignal(sig, ps.Value);
            }
        }
    }

    // sigdisable disables the Go signal handler for the signal sig.
    // It is only called while holding the os/signal.handlers lock,
    // via os/signal.disableSignal and signal_disable. Kernel-side this is a deliberate NO-OP: Go
    // never uninstalls its runtime handler either — Stop/Reset semantics live entirely in the
    // wanted-bit the caller just cleared, which routes the next delivery to the handler's default
    // decision (die for the _SigKill set, swallow otherwise), Go's default-handling-after-Reset.
    // An Ignore is not undone here either: Go's Reset leaves sigignore's SIG_IGN in the kernel
    // (sigdisable reaches the kernel only in its !sigInstallGoHandler arm), and only a Notify
    // reinstalls — sigenable's restore.
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
    // cleared sig.wanted; Go then makes the KERNEL ignore the signal (setsig(sig, _SIG_IGN)) and,
    // since increment 9, so does the bridge: the disposition the SIG_IGN displaces is saved for the
    // Notify that undoes it, and the persistent registration — if one exists — simply stops being
    // reached, exactly as Go's sighandler stops being reached. The kernel's answer is what tcsetpgrp
    // consults and what an exec'd child inherits; the managed swallow was neither. The CLR-owned
    // set keeps the managed swallow (sigIsKernelIgnorable), its residual stated in the header.
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
            PosixSignal? ps = MapPosixSignal(sig);
            if (ps is null)
            {
                return;
            }
            lock (s_sigPosixLock)
            {
                atomic.Store(ᏑhandlingSig.at<uint32>((nint)(sig)), 0);
                if (!sigIsKernelIgnorable(sig))
                {
                    // The CLR-owned set keeps the managed swallow: the registration is guaranteed
                    // and the handler cancels via signal_ignored; kernel-side the CLR keeps its own
                    // handler, and the residual (a child inheriting SIG_DFL) is the header's.
                    installPosixSignal(sig, ps.Value);
                    return;
                }
                ignoreAtKernel(sig);
            }
        }
    }

    // ignoreAtKernel (called under s_sigPosixLock) installs SIG_IGN for sig as the KERNEL
    // disposition, saving what it displaces ONCE — Go's setsig(sig, _SIG_IGN) with the fwdSig
    // discipline the restore needs. Idempotent: a second Ignore keeps the first save, which is the
    // disposition that predates both.
    private static void ignoreAtKernel(uint32 sig)
    {
        uint bit = 1u << (int)sig;
        if ((s_kernelIgnoredMask & bit) != 0)
        {
            return;
        }
        s_displacedActions[(int)sig] = GoSigactionQuery((int)sig);
        sys_signal((int)sig, SIG_IGN);
        s_kernelIgnoredMask |= bit;
    }

    // restoreDisplacedDisposition (called under s_sigPosixLock) puts back, verbatim, the sigaction
    // an earlier ignoreAtKernel displaced — .NET's SA_SIGINFO handler for a registered signal, or
    // SIG_DFL / the inherited disposition for one that was not — through increment 6's install arm
    // (GoSigactionInstall, sigaction_impl.cs). A no-op when the kernel holds no SIG_IGN of ours.
    private static void restoreDisplacedDisposition(uint32 sig)
    {
        uint bit = 1u << (int)sig;
        if ((s_kernelIgnoredMask & bit) == 0)
        {
            return;
        }
        (ulong handler, uint32 mask, int32 flags) = s_displacedActions[(int)sig];
        GoSigactionInstall((int)sig, handler, mask, flags);
        s_kernelIgnoredMask &= ~bit;
    }
}
