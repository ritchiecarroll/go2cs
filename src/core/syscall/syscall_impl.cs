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

    // Windows' page size is 4096 on every architecture Go supports; runtime.getpagesize reports
    // the value the OS gave it at startup, which is this constant in practice.
    public static partial nint Getpagesize()
    {
        return 4096;
    }

    // In Go these keep the RUNTIME's private copy of the environment in sync with the process
    // environment that syscall.Setenv/Unsetenv just changed via SetEnvironmentVariable. go2cs has
    // no such second copy — syscall.Getenv/Environ read the live process environment through
    // GetEnvironmentVariableW/GetEnvironmentStringsW every time — so there is nothing to mirror
    // and the correct behavior is to do nothing.
    internal static partial void runtimeSetenv(@string k, @string v)
    {
    }

    internal static partial void runtimeUnsetenv(@string k)
    {
    }
}
