// syscall_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the syscall primitives Go's RUNTIME provides rather than the
// syscall package itself (syscall.go: "Getpagesize and Exit are provided by the runtime", plus the
// runtimeSetenv/runtimeUnsetenv linknames). go2cs emits each as a bodyless `partial` method, and
// without a body here the PartialStubGenerator fills them with throwing stubs — so `syscall.Exit`
// died with "Exit: external (assembly or cgo) function is not implemented" instead of terminating
// the process. That is the last step of os.Exit, so no converted program could exit deliberately;
// it surfaced in the os/exec child-process work, where a CHILD that called os.Exit(7) reported the
// stub panic instead of its own status.

using System;

// Hand-owned (no syscall_impl.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned operational files in this package.
[module: go.GoManualConversion]

namespace go;

partial class syscall_package
{
    // Go's runtime exits the process immediately with the given status. Environment.Exit is the
    // managed equivalent (it is what golib's unrecovered-panic handler already uses to report
    // exit code 2) and flushes the console writers on the way out, which Go's buffered stdout
    // does as well.
    public static partial void Exit(nint code)
    {
        Environment.Exit((int)code);
    }

    // Go's runtime provides this (runtime.syscall_Getpagesize): it returns physPageSize, the page size
    // the OS reported at startup. A constant 4096 stood here, which is wrong on Apple silicon (16384)
    // and on any 64 KiB-page arm64 linux; the call now crosses into the runtime through its public
    // shim (managed_impl.cs, the runtimeSetenv pattern below), so os.Getpagesize answers what Go does.
    public static partial nint Getpagesize()
    {
        return runtime_package.syscallGetpagesize();
    }

    // Go's runtime provides these (runtime.syscall_runtimeSetenv/Unsetenv): they mirror the change
    // into the C environment when cgo is loaded, and when the key is GODEBUG they reparse the
    // runtime's atomic debug settings. The first half is inert here (no cgo), but the second is not:
    // with a do-nothing body `t.Setenv("GODEBUG", "panicnil=1")` never reached debug.panicnil, so
    // runtime's TestPanicNil/GODEBUG=panicnil=1 still saw a *PanicNilError. The runtime symbols are
    // `internal` under the exported-ness rule, so the call crosses through the runtime's public
    // shims (managed_impl.cs, the registerPoolCleanup pattern), which forward unchanged.
    internal static partial void runtimeSetenv(@string k, @string v)
    {
        runtime_package.syscallRuntimeSetenv(k, v);
    }

    internal static partial void runtimeUnsetenv(@string k)
    {
        runtime_package.syscallRuntimeUnsetenv(k);
    }
}
