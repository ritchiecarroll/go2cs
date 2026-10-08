// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go.@internal;

using itoa = go.@internal.itoa_package;
using unix = go.@internal.syscall.unix_package;
using io = io_package;
using atomic = go.sync.atomic_package;
using Δsyscall = syscall_package;
using go.@internal;
using go.@internal.syscall;
using go.sync;

partial class poll_package {

// FD is a file descriptor. The net and os packages use this type as a
// field of a larger type representing a network connection or OS file.
partial struct FD {
    // Lock sysfd and serialize access to Read and Write methods.
    internal fdMutex fdmu;
    // System file descriptor. Immutable until Close.
    public nint Sysfd;
    // Platform dependent state of the file descriptor.
    public partial ref SysFile SysFile { get; }
    // I/O poller.
    internal pollDesc pd;
    // Semaphore signaled when file is closed.
    internal uint32 csema;
    // Non-zero if this file has been set to blocking mode.
    internal uint32 isBlocking;
    // Whether this is a streaming descriptor, as opposed to a
    // packet-based descriptor like a UDP socket. Immutable.
    public bool IsStream;
    // Whether a zero byte read indicates EOF. This is false for a
    // message based socket connection.
    public bool ZeroReadIsEOF;
    // Whether this is a file rather than a network socket.
    internal bool isFile;
}

// Init initializes the FD. The Sysfd field should already be set.
// This can be called multiple times on a single FD.
// The net argument is a network name from the net package (e.g., "tcp"),
// or "file".
// Set pollable to true if fd should be managed by runtime netpoll.
public static error Init(this ж<FD> Ꮡfd, @string net, bool pollable) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    fd.SysFile.init();
    // We don't actually care about the various network types.
    if (net == "file"u8) {
        fd.isFile = true;
    }
    if (!pollable) {
        fd.isBlocking = 1;
        return default!;
    }
    var err = fd.pd.init(Ꮡfd);
    if (err != default!) {
        // If we could not initialize the runtime poller,
        // assume we are using blocking mode.
        fd.isBlocking = 1;
    }
    return err;
}

// Destroy closes the file descriptor. This is called when there are
// no remaining references.
internal static error destroy(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    // Poller may want to unregister fd in readiness notification mechanism,
    // so this must be executed before CloseFunc.
    fd.pd.close();
    var err = fd.SysFile.destroy(fd.Sysfd);
    fd.Sysfd = -1;
    runtime_Semrelease(Ꮡfd.of(FD.Ꮡcsema));
    return err;
}

// Close closes the FD. The underlying file descriptor is closed by the
// destroy method when there are no remaining references.
public static error Close(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (!fd.fdmu.increfAndClose()) {
        return errClosing(fd.isFile);
    }
    // Unblock any I/O.  Once it all unblocks and returns,
    // so that it cannot be referring to fd.sysfd anymore,
    // the final decref will close fd.sysfd. This should happen
    // fairly quickly, since all the I/O is non-blocking, and any
    // attempts to block in the pollDesc will return errClosing(fd.isFile).
    fd.pd.evict();
    // The call to decref will call destroy if there are no other
    // references.
    var err = Ꮡfd.decref();
    // Wait until the descriptor is closed. If this was the only
    // reference, it is already closed. Only wait if the file has
    // not been set to blocking mode, as otherwise any current I/O
    // may be blocking, and that would block the Close.
    // No need for an atomic read of isBlocking, increfAndClose means
    // we have exclusive access to fd.
    if (fd.isBlocking == 0) {
        runtime_Semacquire(Ꮡfd.of(FD.Ꮡcsema));
    }
    return err;
}

// SetBlocking puts the file into blocking mode.
public static error SetBlocking(this ж<FD> Ꮡfd) {
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
        // Atomic store so that concurrent calls to SetBlocking
        // do not cause a race condition. isBlocking only ever goes
        // from 0 to 1 so there is no real race here.
        atomic.StoreUint32(Ꮡfd.of(FD.ᏑisBlocking), 1);
        return Δsyscall.SetNonblock(fd.Sysfd, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Darwin and FreeBSD can't read or write 2GB+ files at a time,
// even on 64-bit systems.
// The same is true of socket implementations on many systems.
// See golang.org/issue/7812 and golang.org/issue/16266.
// Use 1GB instead of, say, 2GB-1, to keep subsequent reads aligned.
internal static UntypedInt maxRW => /* 1 << 30 */ 1073741824;

// Read implements io.Reader.
public static (nint, error) Read(this ж<FD> Ꮡfd, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        if (len(p) == 0) {
            // If the caller wanted a zero byte read, return immediately
            // without trying (but after acquiring the readLock).
            // Otherwise syscall.Read returns 0, nil which looks like
            // io.EOF.
            // TODO(bradfitz): make it wait for readability? (Issue 15735)
            return (0, default!);
        }
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        if (fd.IsStream && len(p) > maxRW) {
            p = p[..(int)(maxRW)];
        }
        while (ᐧ) {
            var (n, err) = ignoringEINTRIO(Δsyscall.Read, fd.Sysfd, p);
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
            err = fd.eofError(n, err);
            return (n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// Pread wraps the pread system call.
public static (nint, error) Pread(this ж<FD> Ꮡfd, slice<byte> p, int64 off) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    // Call incref, not readLock, because since pread specifies the
    // offset it is independent from other reads.
    // Similarly, using the poller doesn't make sense for pread.
    {
        var errΔ1 = Ꮡfd.incref(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    if (fd.IsStream && len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    nint n = default!;
    error err = default!;
    while (ᐧ) {
        (n, err) = Δsyscall.Pread(fd.Sysfd, p, off);
        if (!AreEqual(err, Δsyscall.EINTR)) {
            break;
        }
    }
    if (err != default!) {
        n = 0;
    }
    Ꮡfd.decref();
    err = fd.eofError(n, err);
    return (n, err);
}

// ReadFrom wraps the recvfrom network call.
public static (nint, Δsyscall.Sockaddr, error) ReadFrom(this ж<FD> Ꮡfd, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, default!, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, default!, err);
            }
        }
        while (ᐧ) {
            var (n, sa, err) = Δsyscall.Recvfrom(fd.Sysfd, p, 0);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                n = 0;
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, sa, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// ReadFromInet4 wraps the recvfrom network call for IPv4.
public static (nint, error) ReadFromInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> Ꮡfrom) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            var (n, err) = unix.RecvfromInet4(fd.Sysfd, p, 0, Ꮡfrom);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                n = 0;
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// ReadFromInet6 wraps the recvfrom network call for IPv6.
public static (nint, error) ReadFromInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> Ꮡfrom) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            var (n, err) = unix.RecvfromInet6(fd.Sysfd, p, 0, Ꮡfrom);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                n = 0;
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// ReadMsg wraps the recvmsg network call.
public static (nint, nint, nint, Δsyscall.Sockaddr, error) ReadMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, 0, 0, default!, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, 0, 0, default!, err);
            }
        }
        while (ᐧ) {
            var (n, oobn, sysflags, sa, err) = Δsyscall.Recvmsg(fd.Sysfd, p, oob, flags);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                // TODO(dfc) should n and oobn be set to 0
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, oobn, sysflags, sa, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// ReadMsgInet4 is ReadMsg, but specialized for syscall.SockaddrInet4.
public static (nint, nint, nint, error) ReadMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet4> Ꮡsa4) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, 0, 0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, 0, 0, err);
            }
        }
        while (ᐧ) {
            var (n, oobn, sysflags, err) = unix.RecvmsgInet4(fd.Sysfd, p, oob, flags, Ꮡsa4);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                // TODO(dfc) should n and oobn be set to 0
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, oobn, sysflags, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// ReadMsgInet6 is ReadMsg, but specialized for syscall.SockaddrInet6.
public static (nint, nint, nint, error) ReadMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet6> Ꮡsa6) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (0, 0, 0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (0, 0, 0, err);
            }
        }
        while (ᐧ) {
            var (n, oobn, sysflags, err) = unix.RecvmsgInet6(fd.Sysfd, p, oob, flags, Ꮡsa6);
            if (err != default!) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                // TODO(dfc) should n and oobn be set to 0
                if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            err = fd.eofError(n, err);
            return (n, oobn, sysflags, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// Write implements io.Writer.
public static (nint, error) Write(this ж<FD> Ꮡfd, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        nint nn = default!;
        while (ᐧ) {
            nint max = len(p);
            if (fd.IsStream && max - nn > maxRW) {
                max = nn + (nint)maxRW;
            }
            var (n, err) = ignoringEINTRIO(Δsyscall.Write, fd.Sysfd, p.slice(nn, max));
            if (n > 0) {
                if (n > max - nn) {
                    // This can reportedly happen when using
                    // some VPN software. Issue #61060.
                    // If we don't check this we will panic
                    // with slice bounds out of range.
                    // Use a more informative panic.
                    throw panic("invalid return from write: got " + itoa.Itoa(n) + " from a write of " + itoa.Itoa(max - nn));
                }
                nn += n;
            }
            if (nn == len(p)) {
                return (nn, err);
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (nn, err);
            }
            if (n == 0) {
                return (nn, io.ErrUnexpectedEOF);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// Pwrite wraps the pwrite system call.
public static (nint, error) Pwrite(this ж<FD> Ꮡfd, slice<byte> p, int64 off) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        // Call incref, not writeLock, because since pwrite specifies the
        // offset it is independent from other writes.
        // Similarly, using the poller doesn't make sense for pwrite.
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        nint nn = default!;
        while (ᐧ) {
            nint max = len(p);
            if (fd.IsStream && max - nn > maxRW) {
                max = nn + (nint)maxRW;
            }
            var (n, err) = Δsyscall.Pwrite(fd.Sysfd, p.slice(nn, max), off + (int64)nn);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (n > 0) {
                nn += n;
            }
            if (nn == len(p)) {
                return (nn, err);
            }
            if (err != default!) {
                return (nn, err);
            }
            if (n == 0) {
                return (nn, io.ErrUnexpectedEOF);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// WriteToInet4 wraps the sendto network call for IPv4 addresses.
public static (nint, error) WriteToInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> Ꮡsa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            var err = unix.SendtoInet4(fd.Sysfd, p, 0, Ꮡsa);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (0, err);
            }
            return (len(p), default!);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteToInet6 wraps the sendto network call for IPv6 addresses.
public static (nint, error) WriteToInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> Ꮡsa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            var err = unix.SendtoInet6(fd.Sysfd, p, 0, Ꮡsa);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (0, err);
            }
            return (len(p), default!);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteTo wraps the sendto network call.
public static (nint, error) WriteTo(this ж<FD> Ꮡfd, slice<byte> p, Δsyscall.Sockaddr sa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, err);
            }
        }
        while (ᐧ) {
            var err = Δsyscall.Sendto(fd.Sysfd, p, 0, sa);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (0, err);
            }
            return (len(p), default!);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteMsg wraps the sendmsg network call.
public static (nint, nint, error) WriteMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, Δsyscall.Sockaddr sa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, 0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, 0, err);
            }
        }
        while (ᐧ) {
            var (n, err) = Δsyscall.SendmsgN(fd.Sysfd, p, oob, sa, 0);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (n, 0, err);
            }
            return (n, len(oob), err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteMsgInet4 is WriteMsg specialized for syscall.SockaddrInet4.
public static (nint, nint, error) WriteMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet4> Ꮡsa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, 0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, 0, err);
            }
        }
        while (ᐧ) {
            var (n, err) = unix.SendmsgNInet4(fd.Sysfd, p, oob, Ꮡsa, 0);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (n, 0, err);
            }
            return (n, len(oob), err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteMsgInet6 is WriteMsg specialized for syscall.SockaddrInet6.
public static (nint, nint, error) WriteMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet6> Ꮡsa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, 0, err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return (0, 0, err);
            }
        }
        while (ᐧ) {
            var (n, err) = unix.SendmsgNInet6(fd.Sysfd, p, oob, Ꮡsa, 0);
            if (AreEqual(err, Δsyscall.EINTR)) {
                continue;
            }
            if (AreEqual(err, Δsyscall.EAGAIN) && fd.pd.pollable()) {
                {
                    err = fd.pd.waitWrite(fd.isFile); if (err == default!) {
                        continue;
                    }
                }
            }
            if (err != default!) {
                return (n, 0, err);
            }
            return (n, len(oob), err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// Accept wraps the accept network call.
public static (nint, Δsyscall.Sockaddr, @string, error) Accept(this ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (-1, default!, "", err);
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return (-1, default!, "", err);
            }
        }
        while (ᐧ) {
            var (s, rsa, errcall, err) = accept(fd.Sysfd);
            if (err == default!) {
                return (s, rsa, "", err);
            }
            var exprᴛ1 = err;
            if (AreEqual(exprᴛ1, Δsyscall.EINTR)) {
                continue;
            }
            else if (AreEqual(exprᴛ1, Δsyscall.EAGAIN)) {
                if (fd.pd.pollable()) {
                    {
                        err = fd.pd.waitRead(fd.isFile); if (err == default!) {
                            continue;
                        }
                    }
                }
            }
            else if (AreEqual(exprᴛ1, Δsyscall.ECONNABORTED)) {
                continue;
            }

            // This means that a socket on the listen
            // queue was closed before we Accept()ed it;
            // it's a silly error, so try again.
            return (-1, default!, errcall, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// Fchmod wraps syscall.Fchmod.
public static error Fchmod(this ж<FD> Ꮡfd, uint32 mode) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => Δsyscall.Fchmod(Ꮡfd.Value.Sysfd, mode));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Fstat wraps syscall.Fstat
public static error Fstat(this ж<FD> Ꮡfd, ж<Δsyscall.Stat_t> Ꮡs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => Δsyscall.Fstat(Ꮡfd.Value.Sysfd, Ꮡs));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// dupCloexecUnsupported indicates whether F_DUPFD_CLOEXEC is supported by the kernel.
internal static ж<atomic.Bool> ᏑdupCloexecUnsupported = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool dupCloexecUnsupported => ref ᏑdupCloexecUnsupported.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fcntlˢ = "fcntl"u8;

// DupCloseOnExec dups fd and marks it close-on-exec.
public static (nint, @string, error) DupCloseOnExec(nint fd) {
    if (Δsyscall.F_DUPFD_CLOEXEC != 0 && !ᏑdupCloexecUnsupported.Load()) {
        var (r0, err) = unix.Fcntl(fd, Δsyscall.F_DUPFD_CLOEXEC, 0);
        if (err == default!) {
            return (r0, "", default!);
        }
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, Δsyscall.EINVAL) || AreEqual(exprᴛ1, Δsyscall.ENOSYS)) {
            ᏑdupCloexecUnsupported.Store(true);
        }
        else { /* default: */
            return (-1, fcntlˢ, err);
        }

    }
    // Old kernel, or js/wasm (which returns
    // ENOSYS). Fall back to the portable way from
    // now on.
    return dupCloseOnExecOld(fd);
}

// Dup duplicates the file descriptor.
public static (nint, @string, error) Dup(this ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return (-1, "", err);
            }
        }
        ᒐd1 = true;
        return DupCloseOnExec(fd.Sysfd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// On Unix variants only, expose the IO event for the net code.

// WaitWrite waits until data can be written to fd.
public static error WaitWrite(this ref FD fd) {
    return fd.pd.waitWrite(fd.isFile);
}

// WriteOnce is for testing only. It makes a single write call.
public static (nint, error) WriteOnce(this ж<FD> Ꮡfd, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        return ignoringEINTRIO(Δsyscall.Write, fd.Sysfd, p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// RawRead invokes the user-defined function f for a read operation.
public static error RawRead(this ж<FD> Ꮡfd, Func<uintptr, bool> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareRead(fd.isFile); if (err != default!) {
                return err;
            }
        }
        while (ᐧ) {
            if (f((uintptr)fd.Sysfd)) {
                return default!;
            }
            {
                var err = fd.pd.waitRead(fd.isFile); if (err != default!) {
                    return err;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// RawWrite invokes the user-defined function f for a write operation.
public static error RawWrite(this ж<FD> Ꮡfd, Func<uintptr, bool> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        {
            var err = fd.pd.prepareWrite(fd.isFile); if (err != default!) {
                return err;
            }
        }
        while (ᐧ) {
            if (f((uintptr)fd.Sysfd)) {
                return default!;
            }
            {
                var err = fd.pd.waitWrite(fd.isFile); if (err != default!) {
                    return err;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// ignoringEINTRIO is like ignoringEINTR, but just for IO calls.
internal static (nint, error) ignoringEINTRIO(Func<nint, slice<byte>, (nint, error)> fn, nint fd, slice<byte> p) {
    while (ᐧ) {
        var (n, err) = fn(fd, p);
        if (!AreEqual(err, Δsyscall.EINTR)) {
            return (n, err);
        }
    }
}

} // end poll_package
