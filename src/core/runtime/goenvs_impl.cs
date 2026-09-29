// goenvs_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's environment snapshot, taken the way a managed program can take it.
//
// Go fills `runtime.envs` in goenvs() during schedinit, before any Go code runs: on Windows that is
// os_windows.go walking the block GetEnvironmentStringsW returns, on Unix runtime1.go reading the
// envp the kernel placed above argv. Neither path survives conversion — schedinit is Go's scheduler
// bootstrap and go2cs never runs it (every converted runtime init carries the comment "not run;
// .NET is the runtime"), and the converted goenvs() reads that raw environment block through
// pointer arithmetic over an address the CLR does not hand out.
//
// So `envs` stayed nil, and gogetenv — whose first act is to reject a nil environ() with
// throw("getenv before env init") — killed every caller. That is not a niche path: runtime.GOROOT()
// IS gogetenv("GOROOT"), which is what testenv.GOROOT calls, which is what any Go test needing the
// toolchain calls. The throw then re-faulted inside its own traceback on the unimplemented
// getcallerpc, so the failure presented as a crash rather than as a missing value.
//
// A module initializer is the faithful stand-in for schedinit's slot: it runs when the runtime
// assembly is first touched, before any converted Go code in it, exactly once. The SNAPSHOT
// semantics are Go's own, not a simplification — GOROOT's own doc says "the GOROOT environment
// variable, if set at process start", and Go's setenv_c only mirrors into the C environment when
// cgo is loaded — so a later os.Setenv does not, and should not, appear here.
//
// Unlike this package's other module initializer (panicvalues_impl.cs, which hands golib a lambda
// precisely to AVOID touching a static during module init), writing `envs` necessarily forces
// runtime_package's type initializer here. There is no way around it — environ() reads the field
// directly, so the value has to be in the field before any converted code runs — and it is safe in
// practice: the field has no initializer of its own to overwrite it, and forcing this cctor at
// module load only makes a lazy initialization eager, not different.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files.

using System;
using System.Collections;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛInitEnvs()
    {
        IDictionary variables = Environment.GetEnvironmentVariables();
        slice<@string> snapshot = new slice<@string>(variables.Count);
        nint i = 0;

        // Go keeps each entry in its "key=value" wire form rather than as a pair: gogetenv matches a
        // key by finding '=' at exactly len(key) and slicing past it, and syscall.Environ hands the
        // same strings straight back to callers.
        foreach (DictionaryEntry variable in variables)
        {
            snapshot[i++] = $"{variable.Key}={variable.Value}";
        }

        envs = snapshot;
        defaultGOROOT = Environment.GetEnvironmentVariable(GoDefaultGorootVariable) ?? "";

        // schedinit's NEXT step in Go is parsedebugvars, and it belongs here for the same reason envs
        // does: it reads the environment (gogetenv, so envs must be set first) and must precede any
        // Go code. Without it every dbgvar default stayed zero, and a GODEBUG or GOTRACEBACK present
        // at PROCESS START was never parsed -- only a later os.Setenv reached the runtime (through
        // syscall_runtimeSetenv's reparse). It also retires mprof_impl.cs's profstackdepth patch,
        // which applied one default by hand and would now override a GODEBUG=profstackdepth=0.
        //
        // This runs inside a module initializer, where a raw exception becomes a
        // TypeInitializationException that takes down the runtime assembly and everything that
        // depends on it. parsedebugvars' own failures are Go fatals already (cgocheck > 1 throws);
        // anything ELSE is converted into one here, reason and all, which is the exit Go would take.
        try
        {
            parsedebugvars();
        }
        catch (Exception ex)
        {
            @throw($"parsedebugvars at startup: {ex.GetType().FullName}: {ex.Message}");
        }

        // schedinit's order continues: parsedebugvars above, then gcinit, whose pacer reads GOGC and GOMEMLIMIT through
        // gogetenv. It runs in this initializer because C# does not order one module initializer
        // against another (managed_impl.cs, gcinitController).
        gcinitController();
    }

    /// <summary>
    /// The environment variable that carries this program's link-time GOROOT, which cmd/link bakes
    /// into Go's binary as runtime.defaultGOROOT.
    /// </summary>
    /// <remarks>
    /// There is no linker here, so the -tests pipeline hands the host the GOROOT it converted from
    /// through this variable, and the module initializer above copies it into defaultGOROOT once, as
    /// the link would have. It is deliberately NOT the GOROOT variable: Go's own tests run children
    /// with <c>GOROOT=</c> (runtime/debug's TestStack) and expect runtime.GOROOT() to fall back to the
    /// link-time root, and an ambient GOROOT can name an unrelated Go install. Unset (a program run
    /// outside the pipeline, like a Go binary built with -trimpath), defaultGOROOT stays empty.
    /// </remarks>
    public const string GoDefaultGorootVariable = "GO2CS_DEFAULT_GOROOT";

    // TEST SEAMS (pinner_impl.cs's pattern: GolibTests is outside the InternalsVisibleTo grant). They re-run
    // the start sequence over a given environment, as a process started with it would: envs, then
    // parsedebugvars. The call is RAW, not through the fatal wrapper above, so a failure reaches the arm as
    // the exception it is instead of ending the test host. The caller restores by passing back what
    // GoStartEnvironment returned.
    public static string[] GoStartEnvironment()
    {
        string[] environment = new string[(int)envs.Length];

        for (int i = 0; i < environment.Length; i++)
            environment[i] = envs[i].ToString();

        return environment;
    }

    public static void GoParseDebugVarsAtStart(string[] environment)
    {
        slice<@string> snapshot = new slice<@string>(environment.Length);

        for (int i = 0; i < environment.Length; i++)
            snapshot[i] = environment[i];

        envs = snapshot;
        parsedebugvars();
    }

    public static int GoDebugProfStackDepth => debug.profstackdepth;

    public static int GoDebugCgoCheck => debug.cgocheck;
}
