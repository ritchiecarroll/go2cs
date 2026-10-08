// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1 || windows
namespace go.@internal;

using io = io_package;
using Δsyscall = syscall_package;

partial class poll_package {

// eofError returns io.EOF when fd is available for reading end of
// file.
internal static error eofError(this ref FD fd, nint n, error err) {
    if (n == 0 && err == default! && fd.ZeroReadIsEOF) {
        return io.EOF;
    }
    return err;
}

// Shutdown wraps syscall.Shutdown.
public static error Shutdown(this ж<FD> Ꮡfd, nint how) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return Δsyscall.Shutdown(fd.Sysfd, how);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Fchown wraps syscall.Fchown.
public static error Fchown(this ж<FD> Ꮡfd, nint uid, nint gid) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => Δsyscall.Fchown(Ꮡfd.Value.Sysfd, uid, gid));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Ftruncate wraps syscall.Ftruncate.
public static error Ftruncate(this ж<FD> Ꮡfd, int64 size) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => Δsyscall.Ftruncate(Ꮡfd.Value.Sysfd, size));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// RawControl invokes the user-defined function f for a non-IO
// operation.
public static error RawControl(this ж<FD> Ꮡfd, Action<uintptr> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        f((uintptr)fd.Sysfd);
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// ignoringEINTR makes a function call and repeats it if it returns
// an EINTR error. This appears to be required even though we install all
// signal handlers with SA_RESTART: see #22838, #38033, #38836, #40846.
// Also #20400 and #36644 are issues in which a signal handler is
// installed without setting SA_RESTART. None of these are the common case,
// but there are enough of them that it seems that we can't avoid
// an EINTR loop.
internal static error ignoringEINTR(Func<error> fn) {
    while (ᐧ) {
        var err = fn();
        if (!AreEqual(err, Δsyscall.EINTR)) {
            return err;
        }
    }
}

// ignoringEINTR2 is ignoringEINTR, but returning an additional value.
internal static (T, error) ignoringEINTR2<T>(Func<(T, error)> fn) {
    while (ᐧ) {
        var (v, err) = fn();
        if (!AreEqual(err, Δsyscall.EINTR)) {
            return (v, err);
        }
    }
}

} // end poll_package
