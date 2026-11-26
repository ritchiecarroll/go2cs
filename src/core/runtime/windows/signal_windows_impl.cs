// signal_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// Parts of this file are inspired by Go, whose notices they carry (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - enableWER: INSPIRED BY src/runtime/signal_windows.go, Go 1.26.11, func enableWER. The same error-mode change
//   through a P/Invoke call.
// - the console control handler: INSPIRED BY src/runtime/os_windows.go, Go 1.26.11, func ctrlHandler. The same
//   event-to-signal mapping, installed through the host's console handler.
// - the handler install helpers: original go2cs code.

// The runtime's console control handler, installed the way a managed program can install it - the
// OS EDGE of Windows signal delivery, and deliberately nothing more.
//
// Windows has no POSIX signals. What it has is console control events, and Go's whole
// Windows signal story is one function: os_windows.go's ctrlHandler maps CTRL_C_EVENT and
// CTRL_BREAK_EVENT to _SIGINT, CTRL_CLOSE/LOGOFF/SHUTDOWN_EVENT to _SIGTERM, hands the result to
// sigqueue's sigsend, and returns 1 (handled - the process lives) or 0 (unhandled - Windows runs
// the default handler and the process dies). signal_windows.go's sigenable/sigdisable/sigignore are
// EMPTY on this platform, under a literal "Following are not implemented" comment: the sig.wanted
// bitset that sigsend consults is the only gate there is.
//
// All of that converts faithfully and is already in the corpus. The one thing that does not survive
// is the ARMING. Go arms it in osinit:
//
//     var fn any = ctrlHandler
//     ctrlHandlerPC := compileCallback(*efaceOf(&fn), true)
//     stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1)
//
// and neither half runs in the managed model - osinit is Go's runtime bootstrap, which go2cs emits
// already marked not-run, and stdcall bottoms out in asmstdcall, a throwing stub. compileCallback
// is worse than unavailable: it builds a native thunk out of generated assembly. So the converted
// ctrlHandler was reachable code nobody ever called, and signal.Notify would have succeeded and
// then silently never delivered - the plausible-looking wrong answer this project rules against,
// which is why arming it and forwarding os/signal's linkname pushes are ONE change and not two
// (the GetSystemDirectory precedent; see linknameOperations.go).
//
// WHAT THIS FILE OWNS is exactly the edge: a real SetConsoleCtrlHandler registration whose callback
// calls the CONVERTED ctrlHandler. It re-implements no mapping, no bitset and no state machine -
// sigsend, signal_recv, signal_enable/disable/ignore/ignored and signalWaitUntilIdle all stay
// auto-converted in runtime/sigqueue.cs, and Go's Windows semantics fall out of them unaltered,
// including the ones a POSIX reading gets wrong. Notify makes ^C and ^BREAK deliver os.Interrupt
// and the program survive; Stop and Reset restore the default; Ignore clears wanted and sets
// ignored, so ^C goes back to TERMINATING the process while Ignored() truthfully answers true.
// That last one reads like a bug and is not: os/signal's own doc.go documents only Notify, Reset
// and Stop under "# Windows", because Ignore has no console-event lever to pull. Reproducing it is
// the point.
//
// A managed FUNCTION POINTER, not a delegate. [UnmanagedCallersOnly] over a static method compiles
// to a plain native entry point, so there is no managed delegate object whose lifetime has to be
// rooted against the GC for the life of the process - the classic way this registration breaks.
//
// NO EXCEPTION MAY CROSS THIS BOUNDARY. The callback is invoked by Windows on a thread it creates
// for the event; letting a managed exception unwind into native code is undefined behavior, so the
// body catches everything and each arm is a considered answer rather than a swallow - see below.
//
// A module initializer is the faithful stand-in for osinit's slot, exactly as goenvs_impl.cs,
// goargs_impl.cs and os_windows_impl.cs argue at length: it runs when the runtime assembly is first
// touched, before any converted Go code in it, exactly once. Go arms this handler unconditionally
// for every program, so this does too, and a program that never calls Notify is unaffected: with no
// wanted bit set sigsend returns false, the callback returns 0, and Windows runs the default
// handler precisely as it would have.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files. It sits in
// the windows/ folder because its principal signal_windows.cs does (layout L3).

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

[module: go.GoManualConversion]
// UNSAFE: this file's own code needs /unsafe, so it says so: the converter raises runtime.csproj's
// <AllowUnsafeBlocks> on a hand-owned declaration (projectFileWriter.go allowUnsafeBlocks), and it no longer
// emits an unsafe function in this package that would raise it by accident.
[module: go.GoRequiresUnsafe]

namespace go;

partial class runtime_package
{
    [LibraryImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler", SetLastError = true)]
    private static unsafe partial int SetConsoleCtrlHandlerNative(delegate* unmanaged<uint, int> handlerRoutine, int add);

    // enableWER is Go's (signal_windows.go): re-enable Windows Error Reporting without the fault UI --
    // read the process error mode and clear SEM_NOGPFAULTERRORBOX. Hand-owned (manualConversionFuncs,
    // goosWindows) because the converted body reaches the mode through stdcall0 -> asmstdcall, whose
    // generated stub throws NotImplementedException, and setTraceback("wer") calls it from
    // parsedebugvars at MODULE INIT whenever GOTRACEBACK=wer is in the environment (runtime's
    // TestWERDialogue re-execs itself exactly so). A raw exception there would take down the runtime
    // assembly and every assembly that depends on it. Go's act is these two calls; so is this.
    internal static void enableWER()
    {
        uint errormode = GetErrorModeNative();

        if ((errormode & SemNoGpFaultErrorBox) != 0)
            SetErrorModeNative(errormode ^ SemNoGpFaultErrorBox);
    }

    private const uint SemNoGpFaultErrorBox = 0x0002; // _SEM_NOGPFAULTERRORBOX

    [LibraryImport("kernel32.dll", EntryPoint = "GetErrorMode")]
    private static partial uint GetErrorModeNative();

    [LibraryImport("kernel32.dll", EntryPoint = "SetErrorMode")]
    private static partial uint SetErrorModeNative(uint uMode);

    [ModuleInitializer]
    internal static unsafe void ᴛInstallConsoleCtrlHandler()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Go ignores the result too. A process with no console (a detached or GUI process) simply
        // has no control events to receive, which is not an error and not something to announce.
        SetConsoleCtrlHandlerNative(&ᴛConsoleCtrlRoutine, 1);
    }

    // PHANDLER_ROUTINE: BOOL WINAPI HandlerRoutine(DWORD dwCtrlType). Windows calls this on its own
    // thread, so returning is what decides the process's fate - nonzero means handled.
    [UnmanagedCallersOnly]
    private static int ᴛConsoleCtrlRoutine(uint ctrlType)
    {
        try
        {
            return ctrlHandler(((uint32)ctrlType)) != 0 ? 1 : 0;
        }
        catch (NotImplementedException)
        {
            // The ONLY statement inside ctrlHandler that can reach an unimplemented stub is block(),
            // on the _SIGTERM arm, and only AFTER sigsend has already queued the signal: block() is
            // gopark -> acquirem -> getg(), still an intrinsic with no managed realization. Go's
            // intent at that exact point is spelled out in its own comment - "Windows terminates the
            // process after this handler returns. Block indefinitely to give signal handlers a
            // chance to clean up" - and parking THIS thread achieves from the OS's side precisely
            // what parking the g achieves in Go: the callback never returns, the program's own
            // handlers get their window, and Windows tears the process down when its close/shutdown
            // timeout expires. Reporting handled matches Go's `return 1` on the same path.
            Thread.Sleep(Timeout.Infinite);
            return 1;
        }
        catch (Exception ex)
        {
            // Nothing else in ctrlHandler or sigsend can throw except runtime.throw on a corrupt
            // sig.state, which is a real bug rather than a missing capability. It cannot be allowed
            // to unwind into Windows, and it must not be swallowed silently either, so announce it
            // on stderr and DECLINE the event - the default handler then runs, which is the
            // conservative answer and the one the program would have gotten with no handler at all.
            try
            {
                Console.Error.WriteLine($"go2cs: console control handler failed for event {ctrlType}: {ex}");
            }
            catch
            {
                // stderr itself is gone; there is nowhere left to report.
            }

            return 0;
        }
    }
}
