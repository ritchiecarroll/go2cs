// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows || wasip1
namespace go.@internal;

using errors = errors_package;
using sync = go.sync_package;
using Δsyscall = syscall_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using go;

partial class poll_package {

// runtimeNano returns the current value of the runtime clock in nanoseconds.
//
//go:linkname runtimeNano runtime.nanotime
internal static partial int64 runtimeNano();

internal static partial void runtime_pollServerInit();

internal static partial (uintptr, nint) runtime_pollOpen(uintptr fd);

internal static partial void runtime_pollClose(uintptr ctx);

internal static partial nint runtime_pollWait(uintptr ctx, nint mode);

internal static partial void runtime_pollWaitCanceled(uintptr ctx, nint mode);

internal static partial nint runtime_pollReset(uintptr ctx, nint mode);

internal static partial void runtime_pollSetDeadline(uintptr ctx, int64 d, nint mode);

internal static partial void runtime_pollUnblock(uintptr ctx);

internal static partial bool runtime_isPollServerDescriptor(uintptr fd);

partial struct pollDesc {
    internal uintptr runtimeCtx;
}

internal static ж<sync.Once> ᏑserverInit = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once serverInit => ref ᏑserverInit.Value;

internal static error init(this ref pollDesc pd, ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    ᏑserverInit.Do(runtime_pollServerInit);
    var (ctx, errno) = runtime_pollOpen((uintptr)fd.Sysfd);
    if (errno != 0) {
        return errnoErr(((Δsyscall.Errno)(uintptr)errno));
    }
    pd.runtimeCtx = ctx;
    return default!;
}

internal static void close(this ref pollDesc pd) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollClose(pd.runtimeCtx);
    pd.runtimeCtx = 0;
}

// Evict evicts fd from the pending list, unblocking any I/O running on fd.
internal static void evict(this ref pollDesc pd) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollUnblock(pd.runtimeCtx);
}

internal static error prepare(this ref pollDesc pd, nint mode, bool isFile) {
    if (pd.runtimeCtx == 0) {
        return default!;
    }
    nint res = runtime_pollReset(pd.runtimeCtx, mode);
    return convertErr(res, isFile);
}

internal static error prepareRead(this ref pollDesc pd, bool isFile) {
    return pd.prepare((rune)'r', isFile);
}

internal static error prepareWrite(this ref pollDesc pd, bool isFile) {
    return pd.prepare((rune)'w', isFile);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string waitingForUnsupportedˢ = "waiting for unsupported file type"u8;

internal static error wait(this ref pollDesc pd, nint mode, bool isFile) {
    if (pd.runtimeCtx == 0) {
        return errors.New(waitingForUnsupportedˢ);
    }
    nint res = runtime_pollWait(pd.runtimeCtx, mode);
    return convertErr(res, isFile);
}

internal static error waitRead(this ref pollDesc pd, bool isFile) {
    return pd.wait((rune)'r', isFile);
}

internal static error waitWrite(this ref pollDesc pd, bool isFile) {
    return pd.wait((rune)'w', isFile);
}

internal static void waitCanceled(this ref pollDesc pd, nint mode) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollWaitCanceled(pd.runtimeCtx, mode);
}

internal static bool pollable(this ref pollDesc pd) {
    return pd.runtimeCtx != 0;
}

// Error values returned by runtime_pollReset and runtime_pollWait.
// These must match the values in runtime/netpoll.go.
internal static UntypedInt pollNoError => 0;

internal static UntypedInt pollErrClosing => 1;

internal static UntypedInt pollErrTimeout => 2;

internal static UntypedInt pollErrNotPollable => 3;

internal static error convertErr(nint res, bool isFile) {
    var exprᴛ1 = res;
    if (exprᴛ1 == pollNoError) {
        return default!;
    }
    if (exprᴛ1 == pollErrClosing) {
        return errClosing(isFile);
    }
    if (exprᴛ1 == pollErrTimeout) {
        return ErrDeadlineExceeded;
    }
    if (exprᴛ1 == pollErrNotPollable) {
        return ErrNotPollable;
    }

    println((@string)"unreachable: "u8, res);
    throw panic("unreachable");
}

// SetDeadline sets the read and write deadlines associated with fd.
public static error SetDeadline(this ж<FD> Ꮡfd, time.Time t) {
    return setDeadlineImpl(Ꮡfd, t, (rune)'r' + (rune)'w');
}

// SetReadDeadline sets the read deadline associated with fd.
public static error SetReadDeadline(this ж<FD> Ꮡfd, time.Time t) {
    return setDeadlineImpl(Ꮡfd, t, (rune)'r');
}

// SetWriteDeadline sets the write deadline associated with fd.
public static error SetWriteDeadline(this ж<FD> Ꮡfd, time.Time t) {
    return setDeadlineImpl(Ꮡfd, t, (rune)'w');
}

internal static error setDeadlineImpl(ж<FD> Ꮡfd, time.Time t, nint mode) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        int64 d = default!;
        if (!t.IsZero()) {
            d = (int64)time.Until(t);
            if (d == 0) {
                d = -1; // don't confuse deadline right now with no deadline
            }
        }
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        defer(() => Ꮡfd.decref(), ref ᒐ);
        if (fd.pd.runtimeCtx == 0) {
            return ErrNoDeadline;
        }
        runtime_pollSetDeadline(fd.pd.runtimeCtx, d, mode);
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// IsPollDescriptor reports whether fd is the descriptor being used by the poller.
// This is only used for testing.
//
// IsPollDescriptor should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/opencontainers/runc
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname IsPollDescriptor
public static bool IsPollDescriptor(uintptr fd) {
    return runtime_isPollServerDescriptor(fd);
}

} // end poll_package
