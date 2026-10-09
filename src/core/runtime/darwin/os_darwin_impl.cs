// os_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The values Go's darwin sysargs and osinit set and the managed runtime never reached (2026-10-09).
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly. It is the darwin twin of the populate halves in
// windows/os_windows_impl.cs and linux/os_linux_impl.cs.
//
// WHY. On darwin, os.Executable reads os.executablePath, and Go fills that from runtime.sysargs:
// the kernel passes the executable's path in the apple[] strings after envp, and sysargs copies it
// (stripping the "executable_path=" prefix) into runtime.executablePath, which a
// `//go:linkname executablePath os.executablePath` makes the SAME variable as os's. The managed host
// runs neither rt0 nor sysargs, and the converter emitted the two names as unrelated fields (the
// pull points up, out of runtime into a package that imports it), so os read an empty string and
// answered "cannot find executable path" on every darwin run.
//
// WHAT THIS SETS.
//   executablePath -- the storage side of the inverted alias (linknameVarAliasTargets
// in src/go2cs/linknameOperations.go), which os's declaration now forwards to -- from
// Environment.ProcessPath: the full path of the executable the process was started from, which
// for an apphost-launched program is the program itself and is the path the kernel recorded. That
// matches what /proc/self/exe answers on linux and GetModuleFileName on windows, including under the
// `dotnet <app>.dll` muxer, where all three name the host. A null ProcessPath leaves the field
// empty, so os.Executable reports Go's own error rather than inventing a path.
//
//   physPageSize -- Go's darwin osinit sets it from getPageSize() (sysctl hw.pagesize) before any Go
// code; osinit never runs here, so it read 0 on darwin while linux and windows set it in their own
// companions. That is the state that killed linux's page-allocator rows (alignUp(n, physPageSize)
// answering 0, then "failed to reserve page summary memory"), and syscall.Getpagesize now returns
// it, so os.Getpagesize answered 0. Environment.SystemPageSize is the same hw.pagesize: 4096 on
// x86-64, 16384 on Apple silicon. physHugePageSize stays 0, as Go leaves it on darwin.
//
// WHAT STAYS UNTOUCHED. The rest of darwin's sysargs is the argv walk that finds apple[]; os.Args
// itself comes from goargs_impl.cs. The rest of osinit is ncpu, which the converted runtime2.cs
// already reads from Environment.ProcessorCount, and osinit_hack, a libc warm-up with no managed
// counterpart.
//
// The [ModuleInitializer] runs before any member of this module is touched, so the forwarding
// property in os cannot observe the field before it is written.

using System;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛInitPhysPageSize()
    {
        physPageSize = (uintptr)(nuint)Environment.SystemPageSize;
    }

    [ModuleInitializer]
    internal static void ᴛInitExecutablePath()
    {
        string? processPath = Environment.ProcessPath;

        if (!string.IsNullOrEmpty(processPath))
            executablePath = processPath;
    }
}
