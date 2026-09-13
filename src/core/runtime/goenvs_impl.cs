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
    }
}
