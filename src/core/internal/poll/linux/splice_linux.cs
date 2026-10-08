// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using unix = go.@internal.syscall.unix_package;
using Δruntime = runtime_package;
using sync = go.sync_package;
using Δsyscall = syscall_package;
using @unsafe = unsafe_package;
using go;
using go.@internal.syscall;

partial class poll_package {

internal static UntypedInt spliceNonblock => 0x2;
internal static UntypedInt maxSpliceSize => /* 1 << 20 */ 1048576;

// Splice transfers at most remain bytes of data from src to dst, using the
// splice system call to minimize copies of data from and to userspace.
//
// Splice gets a pipe buffer from the pool or creates a new one if needed, to serve as a buffer for the data transfer.
// src and dst must both be stream-oriented sockets.
public static (int64 written, bool handled, error err) Splice(ж<FD> Ꮡdst, ж<FD> Ꮡsrc, int64 remain) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var src = ref Ꮡsrc.DerefOrNull();

        (var p, err) = getPipe();
        if (err != default!) {
            (written, handled, err) = (0, false, err); goto ᒐdone;
        }
        defer(putPipe, p, ref ᒐ);
        nint inPipe = default!;
        nint n = default!;
        while (err == default! && remain > 0) {
            nint max = maxSpliceSize;
            if ((int64)max > remain) {
                max = (nint)remain;
            }
            (inPipe, err) = spliceDrain((~p).wfd, Ꮡsrc, max);
            // The operation is considered handled if splice returns no
            // error, or an error other than EINVAL. An EINVAL means the
            // kernel does not support splice for the socket type of src.
            // The failed syscall does not consume any data so it is safe
            // to fall back to a generic copy.
            //
            // spliceDrain should never return EAGAIN, so if err != nil,
            // Splice cannot continue.
            //
            // If inPipe == 0 && err == nil, src is at EOF, and the
            // transfer is complete.
            handled = handled || (!AreEqual(err, Δsyscall.EINVAL));
            if (err != default! || inPipe == 0) {
                break;
            }
            p.Value.data += inPipe;
            (n, err) = splicePump(Ꮡdst, (~p).rfd, inPipe);
            if (n > 0) {
                written += (int64)n;
                remain -= (int64)n;
                p.Value.data -= n;
            }
        }
        if (err != default!) {
            goto ᒐdone;
        }
        (written, handled, err) = (written, true, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, handled, err);
}

// spliceDrain moves data from a socket to a pipe.
//
// Invariant: when entering spliceDrain, the pipe is empty. It is either in its
// initial state, or splicePump has emptied it previously.
//
// Given this, spliceDrain can reasonably assume that the pipe is ready for
// writing, so if splice returns EAGAIN, it must be because the socket is not
// ready for reading.
//
// If spliceDrain returns (0, nil), src is at EOF.
internal static (nint, error) spliceDrain(nint pipefd, ж<FD> Ꮡsock, nint max) {
    GoFrame ᒐ = default;
    try {
        ref var sock = ref Ꮡsock.DerefOrNull();

        {
            var err = Ꮡsock.readLock(); if (err != default!) {
                return (0, err);
            }
        }
        defer(Ꮡsock.readUnlock, ref ᒐ);
        {
            var err = sock.pd.prepareRead(sock.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            // In theory calling splice(2) with SPLICE_F_NONBLOCK could end up an infinite loop here,
            // because it could return EAGAIN ceaselessly when the write end of the pipe is full,
            // but this shouldn't be a concern here, since the pipe buffer must be sufficient for
            // this data transmission on the basis of the workflow in Splice.
            var (n, err) = splice(pipefd, sock.Sysfd, max, spliceNonblock);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (!AreEqual(err, Δsyscall.EAGAIN)) {
                return (n, err);
            }
            if (sock.pd.pollable()) {
                {
                    var errΔ1 = sock.pd.waitRead(sock.isFile); if (errΔ1 != default!) {
                        return (n, errΔ1);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// splicePump moves all the buffered data from a pipe to a socket.
//
// Invariant: when entering splicePump, there are exactly inPipe
// bytes of data in the pipe, from a previous call to spliceDrain.
//
// By analogy to the condition from spliceDrain, splicePump
// only needs to poll the socket for readiness, if splice returns
// EAGAIN.
//
// If splicePump cannot move all the data in a single call to
// splice(2), it loops over the buffered data until it has written
// all of it to the socket. This behavior is similar to the Write
// step of an io.Copy in userspace.
internal static (nint, error) splicePump(ж<FD> Ꮡsock, nint pipefd, nint inPipe) {
    GoFrame ᒐ = default;
    try {
        ref var sock = ref Ꮡsock.DerefOrNull();

        {
            var err = Ꮡsock.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        defer(Ꮡsock.writeUnlock, ref ᒐ);
        {
            var err = sock.pd.prepareWrite(sock.isFile); if (err != default!) {
                return (0, err);
            }
        }
        nint written = 0;
        while (inPipe > 0) {
            // In theory calling splice(2) with SPLICE_F_NONBLOCK could end up an infinite loop here,
            // because it could return EAGAIN ceaselessly when the read end of the pipe is empty,
            // but this shouldn't be a concern here, since the pipe buffer must contain inPipe size of
            // data on the basis of the workflow in Splice.
            var (n, err) = splice(sock.Sysfd, pipefd, inPipe, spliceNonblock);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            // Here, the condition n == 0 && err == nil should never be
            // observed, since Splice controls the write side of the pipe.
            if (n > 0) {
                inPipe -= n;
                written += n;
                continue;
            }
            if (!AreEqual(err, Δsyscall.EAGAIN)) {
                return (written, err);
            }
            if (sock.pd.pollable()) {
                {
                    var errΔ1 = sock.pd.waitWrite(sock.isFile); if (errΔ1 != default!) {
                        return (written, errΔ1);
                    }
                }
            }
        }
        return (written, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// splice wraps the splice system call. Since the current implementation
// only uses splice on sockets and pipes, the offset arguments are unused.
// splice returns int instead of int64, because callers never ask it to
// move more data in a single call than can fit in an int32.
internal static (nint, error) splice(nint @out, nint @in, nint max, nint flags) {
    var (n, err) = Δsyscall.Splice(@in, nil, @out, nil, max, flags);
    return ((nint)n, err);
}

partial struct splicePipeFields {
    internal nint rfd;
    internal nint wfd;
    internal nint data;
}

partial struct splicePipe {
    internal partial ref splicePipeFields splicePipeFields { get; }
    // We want to use a finalizer, so ensure that the size is
    // large enough to not use the tiny allocator.
    internal array<byte> _ = new(24 - /* unsafe.Sizeof(splicePipeFields{}) */ (uintptr)24 % 24);
}

// splicePipePool caches pipes to avoid high-frequency construction and destruction of pipe buffers.
// The garbage collector will free all pipes in the sync.Pool periodically, thus we need to set up
// a finalizer for each pipe to close its file descriptors before the actual GC.
internal static ж<sync.Pool> ᏑsplicePipePool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool splicePipePool => ref ᏑsplicePipePool.Value;
internal static void initᴛsplicePipePool() { splicePipePool = new sync.Pool(New: newPoolPipe); }

internal static any newPoolPipe() {
    // Discard the error which occurred during the creation of pipe buffer,
    // redirecting the data transmission to the conventional way utilizing read() + write() as a fallback.
    var p = newPipe();
    if (p == nil) {
        return default!;
    }
    Δruntime.SetFinalizer(p.OrTypedNil(), destroyPipe);
    return p.OrTypedNil();
}

// getPipe tries to acquire a pipe buffer from the pool or create a new one with newPipe() if it gets nil from the cache.
internal static (ж<splicePipe>, error) getPipe() {
    var v = ᏑsplicePipePool.Get();
    if (v == default!) {
        return (default!, Δsyscall.EINVAL);
    }
    return (v._<ж<splicePipe>>(), default!);
}

internal static void putPipe(ж<splicePipe> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // If there is still data left in the pipe,
    // then close and discard it instead of putting it back into the pool.
    if (p.data != 0) {
        Δruntime.SetFinalizer(Ꮡp.OrTypedNil(), default!);
        destroyPipe(Ꮡp);
        return;
    }
    ᏑsplicePipePool.Put(Ꮡp.OrTypedNil());
}

// newPipe sets up a pipe for a splice operation.
internal static ж<splicePipe> newPipe() {
    array<nint> fds = new(2);
    {
        var err = Δsyscall.Pipe2(fds[..], (nint)((nint)Δsyscall.O_CLOEXEC | (nint)Δsyscall.O_NONBLOCK)); if (err != default!) {
            return default!;
        }
    }
    // Splice will loop writing maxSpliceSize bytes from the source to the pipe,
    // and then write those bytes from the pipe to the destination.
    // Set the pipe buffer size to maxSpliceSize to optimize that.
    // Ignore errors here, as a smaller buffer size will work,
    // although it will require more system calls.
    unix.Fcntl(fds[0], Δsyscall.F_SETPIPE_SZ, maxSpliceSize);
    return Ꮡ(new splicePipe(splicePipeFields: new splicePipeFields(rfd: fds[0], wfd: fds[1])));
}

// destroyPipe destroys a pipe.
internal static void destroyPipe(ж<splicePipe> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    CloseFunc(p.rfd);
    CloseFunc(p.wfd);
}

} // end poll_package
