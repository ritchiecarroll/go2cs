// goargs_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's command-line snapshot, taken the way a managed program can take it — the exact
// sibling of goenvs_impl.cs beside it, for the exact same reason.
//
// Go fills `runtime.argslice` in goargs() during schedinit, reading the argv vector the kernel
// placed on the initial stack (`argslice[i] = gostringnocopy(argv_index(argv, i))`). Neither half
// survives conversion: schedinit is the scheduler bootstrap go2cs never runs, and `argv` is a
// raw stack address the CLR does not hand out — so `argslice` stayed nil.
//
// Nil is not a harmless absence here. `os.init()` on unix assigns `Args = runtime_args()`, and
// runtime's push side (`//go:linkname os_runtime_args os.runtime_args`) returns
// `append([]string{}, argslice...)` — which over a nil argslice is an EMPTY, entirely
// plausible-looking os.Args. That is the failure mode this project has ruled against: a contract
// that cannot be honored must announce itself rather than answer plausibly. The difference is that
// this one CAN be honored — the CLR knows the command line exactly — so the honest answer is to
// populate the field, after which the pushed body is ordinary converted Go returning real data.
//
// WINDOWS IS DELIBERATELY EXCLUDED, and the guard is Go's own rather than an adaptation: goargs()
// opens with `if GOOS == "windows" { return }`, because Windows fills os.Args from
// exec_windows.go instead and leaves argslice unset. runtime_boring.cs's boring_runtime_arg0
// documents and depends on that ("On Windows, argslice is not set, and it's too much work to find
// argv0" — it returns "" when len(argslice) == 0). Populating it there would be a divergence from
// Go dressed as an improvement, and would silently change an existing Windows answer.
//
// WHAT [0] IS. Go's os.Args[0] is argv[0] — the program as invoked. Environment.GetCommandLineArgs()
// is the managed mirror of that vector: measured under `dotnet hello.dll alpha beta` it returns
// {".../hello.dll", "alpha", "beta"}, i.e. the managed program followed by its arguments, which is
// the same shape and the same content Go reports. Note it is NOT Environment.ProcessPath, which
// names the HOST (`.../dotnet`) rather than the program, and NOT the args array handed to Main,
// which omits element zero.
//
// A module initializer is the faithful stand-in for schedinit's slot for the reasons goenvs_impl.cs
// states at length: it runs when the runtime assembly is first touched, before any converted Go
// code in it, exactly once. The snapshot semantics are Go's own — argv is fixed at process start.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛInitArgs()
    {
        // goargs()'s own guard, kept verbatim — see the Windows note above.
        if (GOOS == "windows"u8)
        {
            return;
        }

        string[] args = Environment.GetCommandLineArgs();
        slice<@string> snapshot = new slice<@string>(args.Length);

        for (nint i = 0; i < args.Length; i++)
        {
            snapshot[i] = args[i];
        }

        // The [0] note above documents GetCommandLineArgs as argv's managed mirror — MEASURED
        // under `dotnet hello.dll`, and TRUE there. Re-measured 2026-08-22 under an APPHOST
        // launch on Linux: element zero is STILL the managed assembly path
        // (".../argvprobe.dll") while the kernel's argv[0] — Environment.ProcessPath — is the
        // apphost (".../argvprobe"). Go's os.Args[0] is the program as invoked, and every
        // self-re-exec idiom in Go's own suites (`exec.Command(os.Args[0], ...)` — sync's
        // TestMutexMisuse, flag's TestExitCode) depends on it naming something EXECUTABLE; the
        // .dll answer sent each of them into `fork/exec ...dll: permission denied`. So in
        // apphost mode argv[0] is ProcessPath — the kernel's own argv[0], faithful AND
        // re-execable. Under the `dotnet hello.dll` muxer, ProcessPath names the HOST
        // (".../dotnet"), where the mirror's {program, args...} shape remains the better of two
        // imperfect answers — that mode keeps the documented behavior, unchanged.
        string? processPath = Environment.ProcessPath;

        if (args.Length > 0 && !string.IsNullOrEmpty(processPath))
        {
            string processName = System.IO.Path.GetFileNameWithoutExtension(processPath);

            if (!string.Equals(processName, "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                // ProcessPath is the executable, which is NOT always argv[0]: exec(2) takes the path
                // and the argv vector separately, and a caller may set argv[0] to anything. Go's
                // self-re-exec idiom for a child that must know it is the child does exactly that --
                // logrus's alt_exit_test sets cmd.Path = os.Executable() and cmd.Args[0] to a token,
                // and the child recognizes itself ONLY by os.Args[0] == token. Answering ProcessPath
                // there made every child a parent: each re-executed again, without bound (measured
                // 2026-10-09 on linux: the logrus run was OOM-killed). So the kernel's own argv[0]
                // is read where the platform exposes it, and ProcessPath stays the fallback.
                snapshot[0] = KernelArgv0(args.Length) ?? processPath;
            }
        }

        argslice = snapshot;
    }

    // The kernel's argv[0] for this process, or null when it cannot be read with confidence.
    // argc is the managed vector's length: in apphost mode the kernel vector has the same length
    // (only element zero differs), so a vector of any other length is not the one we expect and
    // is not trusted.
    private static string? KernelArgv0(int argc)
    {
        try
        {
            if (GOOS == "linux"u8)
            {
                // /proc/self/cmdline is argv as exec(2) received it, each element NUL-terminated.
                byte[] cmdline = System.IO.File.ReadAllBytes("/proc/self/cmdline");
                int fields = 0;

                foreach (byte b in cmdline)
                {
                    if (b == 0)
                        fields++;
                }

                if (fields != argc)
                    return null;

                int end = Array.IndexOf(cmdline, (byte)0);
                return System.Text.Encoding.UTF8.GetString(cmdline, 0, end);
            }

            if (GOOS == "darwin"u8)
            {
                // There is no /proc; libSystem keeps the C runtime's argc/argv, which are the
                // vector exec(2) received.
                IntPtr pargc = DarwinNSGetArgc();

                if (pargc == IntPtr.Zero || Marshal.ReadInt32(pargc) != argc)
                    return null;

                IntPtr pargv = DarwinNSGetArgv();

                if (pargv == IntPtr.Zero)
                    return null;

                IntPtr argv = Marshal.ReadIntPtr(pargv);

                if (argv == IntPtr.Zero)
                    return null;

                return Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(argv));
            }
        }
        catch (Exception)
        {
            // Any failure to read the kernel vector falls back to ProcessPath, the previous answer.
        }

        return null;
    }

    [DllImport("libc", EntryPoint = "_NSGetArgc")]
    private static extern IntPtr DarwinNSGetArgc();

    [DllImport("libc", EntryPoint = "_NSGetArgv")]
    private static extern IntPtr DarwinNSGetArgv();
}
