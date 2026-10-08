// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm)
namespace go.@internal;

using Δsyscall = syscall_package;
using go;

partial class poll_package {

partial struct SysFile {
    // Writev cache.
    internal ж<slice<Δsyscall.Iovec>> iovecs;
}

internal static void init(this ref SysFile s) {
}

internal static error destroy(this ref SysFile s, nint fd) {
    // We don't use ignoringEINTR here because POSIX does not define
    // whether the descriptor is closed if close returns EINTR.
    // If the descriptor is indeed closed, using a loop would race
    // with some other goroutine opening a new descriptor.
    // (The Linux kernel guarantees that it is closed on an EINTR error.)
    return CloseFunc(fd);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dupˢ = "dup"u8;

// dupCloseOnExecOld is the traditional way to dup an fd and
// set its O_CLOEXEC bit, using two system calls.
internal static (nint, @string, error) dupCloseOnExecOld(nint fd) {
    GoFrame ᒐ = default;
    try {
        Δsyscall.ᏑForkLock.RLock();
        defer(Δsyscall.ᏑForkLock.RUnlock, ref ᒐ);
        var (newfd, err) = Δsyscall.Dup(fd);
        if (err != default!) {
            return (-1, dupˢ, err);
        }
        Δsyscall.CloseOnExec(newfd);
        return (newfd, "", default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Fchdir wraps syscall.Fchdir.
public static error Fchdir(this ж<FD> Ꮡfd) {
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
        return Δsyscall.Fchdir(fd.Sysfd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// ReadDirent wraps syscall.ReadDirent.
// We treat this like an ordinary system call rather than a call
// that tries to fill the buffer.
public static (nint, error) ReadDirent(this ж<FD> Ꮡfd, slice<byte> buf) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        while (ᐧ) {
            var (n, err) = ignoringEINTRIO(Δsyscall.ReadDirent, fd.Sysfd, buf);
            if (err != default!) {
                n = 0;
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            // Do not call eofError; caller does not expect to see io.EOF.
            return (n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Seek wraps syscall.Seek.
public static (int64, error) Seek(this ж<FD> Ꮡfd, int64 offset, nint whence) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        return Δsyscall.Seek(fd.Sysfd, offset, whence);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

} // end poll_package
