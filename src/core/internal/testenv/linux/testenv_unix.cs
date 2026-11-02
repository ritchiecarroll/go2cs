// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal;

using errors = errors_package;
using fs = io.fs_package;
using Δsyscall = syscall_package;
using io;

partial class testenv_package {

// Sigquit is the signal to send to kill a hanging subprocess.
// Send SIGQUIT to get a stack trace.
public static readonly ж<syscallꓸSignal> ᏑSigquit = new StaticBox<syscallꓸSignal>(Δsyscall.SIGQUIT);
public static ref syscallꓸSignal Sigquit => ref ((StaticBox<syscallꓸSignal>)ᏑSigquit).ValueSlot;

internal static bool syscallIsNotSupported(error err) {
    if (err == nil) {
        return false;
    }

    {
        var (errno, ok) = errors.AsType<Δsyscall.Errno>(err); if (ok) {
            switch (errno) {
            case var ᴛ when ᴛ == Δsyscall.EPERM || ᴛ == Δsyscall.EROFS: {
                // User lacks permission: either the call requires root permission and the
                // user is not root, or the call is denied by a container security policy.
                return true;
            }
            case var ᴛ when ᴛ == Δsyscall.EINVAL: {
                // Some containers return EINVAL instead of EPERM if a system call is
                // denied by security policy.
                return true;
            }}
        }
    }

    if (errors.Is(err, fs.ErrPermission) || errors.Is(err, errors.ErrUnsupported)) {
        return true;
    }

    return false;
}

} // end testenv_package
