// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using bytes = bytes_package;
using sort = sort_package;
using sync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using static unix_package;

partial class unix_package {

public static readonly ж<nint> ᏑStdin = new StaticBox<nint>(0);
public static ref nint Stdin => ref ((StaticBox<nint>)ᏑStdin).ValueSlot;
public static readonly ж<nint> ᏑStdout = new StaticBox<nint>(1);
public static ref nint Stdout => ref ((StaticBox<nint>)ᏑStdout).ValueSlot;
public static readonly ж<nint> ᏑStderr = new StaticBox<nint>(2);
public static ref nint Stderr => ref ((StaticBox<nint>)ᏑStderr).ValueSlot;

internal static error errEAGAIN = ((syscall.Errno)syscall.EAGAIN);
internal static error errEINVAL = ((syscall.Errno)syscall.EINVAL);
internal static error errENOENT = ((syscall.Errno)syscall.ENOENT);

internal static readonly ж<sync.Once> ᏑsignalNameMapOnce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once signalNameMapOnce => ref ((StaticBox<sync.Once>)ᏑsignalNameMapOnce).ValueSlot;
internal static map<@string, syscall.Signal> signalNameMap;

internal static error errnoErr(syscall.Errno e) {
    switch (e) {
    case var ᴛ when ᴛ == (syscall.Errno)(0): {
        return default;
    }
    case var ᴛ when ᴛ == EAGAIN: {
        return errEAGAIN;
    }
    case var ᴛ when ᴛ == EINVAL: {
        return errEINVAL;
    }
    case var ᴛ when ᴛ == ENOENT: {
        return errENOENT;
    }}

    return e;
}

public static @string ErrnoName(syscall.Errno e) {
    nint i = sort.Search(len(errorList), (nint iΔ1) => errorList[iΔ1].num >= e);
    if (i < len(errorList) && errorList[i].num == e) {
        return errorList[i].name;
    }
    return "";
}

public static @string SignalName(syscall.Signal s) {
    nint i = sort.Search(len(signalList), (nint iΔ1) => signalList[iΔ1].num >= s);
    if (i < len(signalList) && signalList[i].num == s) {
        return signalList[i].name;
    }
    return "";
}

public static syscall.Signal SignalNum(@string s) {
    ᏑsignalNameMapOnce.Do(() => {
        signalNameMap = new map<@string, syscall.Signal>(len(signalList));
        foreach (var (_, signal) in range(signalList.ΔRangeSnapshot())) {
            signalNameMap[signal.name] = signal.num;
        }
    });
    return signalNameMap[s];
}

internal static nint clen(slice<byte> n) {
    nint i = bytes.IndexByte(n, 0);
    if (i == -1) {
        i = len(n);
    }
    return i;
}

partial struct mmapper {
    /*embed*/ public sync_package.Mutex Mutex;
    internal map<ж<byte>, slice<byte>> active;
    internal Func<uintptr, uintptr, nint, nint, nint, int64, (uintptr, error)> mmap;
    internal Func<uintptr, uintptr, error> munmap;
}

} partial class unixᴛmmapper {

internal static (slice<byte> data, error err) Mmap(this ж<mmapper> Ꮡm, nint fd, int64 offset, nint length, nint prot, nint flags) {
    slice<byte> data = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        if (length <= 0) {
            (data, err) = (default, EINVAL); goto ᒐdone;
        }

        var (addr, errno) = m.mmap(0, (uintptr)length, prot, flags, fd, offset);
        if (errno != nil) {
            (data, err) = (default, errno); goto ᒐdone;
        }

        var b = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)addr), length);

        var p = Ꮡ(b, cap(b) - 1);
        Ꮡm.of(mmapper.ᏑMutex).Lock();
        defer(Ꮡm.of(mmapper.ᏑMutex).Unlock, ref ᒐ);
        m.active[p] = b;
        (data, err) = (b, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (data, err);
}

internal static error /*err*/ Munmap(this ж<mmapper> Ꮡm, slice<byte> data) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        if (len(data) == 0 || len(data) != cap(data)) {
            err = EINVAL; goto ᒐdone;
        }

        var p = Ꮡ(data, cap(data) - 1);
        Ꮡm.of(mmapper.ᏑMutex).Lock();
        defer(Ꮡm.of(mmapper.ᏑMutex).Unlock, ref ᒐ);
        var b = m.active[p];
        if (b == nil || Ꮡelem(b, 0) != Ꮡelem(data, 0)) {
            err = EINVAL; goto ᒐdone;
        }

        var errno = m.munmap((uintptr)Ꮡ(b, 0), (uintptr)len(b)); if (errno != nil) {
            err = errno; goto ᒐdone;
        }
        delete(m.active, p);
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

} partial class unix_package {

public static (slice<byte> data, error err) Mmap(nint fd, int64 offset, nint length, nint prot, nint flags) {
    return mapper.of(mremapMmapper.Ꮡmmapper).Mmap(fd, offset, length, prot, flags);
}

public static error /*err*/ Munmap(slice<byte> b) {
    return mapper.of(mremapMmapper.Ꮡmmapper).Munmap(b);
}

public static (@unsafe.Pointer ret, error err) MmapPtr(nint fd, int64 offset, @unsafe.Pointer addr, uintptr length, nint prot, nint flags) {
    error err = default;

    (var xaddr, err) = mapper.mmap((uintptr)addr, length, prot, flags, fd, offset);
    return ((@unsafe.Pointer)xaddr, err);
}

public static error /*err*/ MunmapPtr(@unsafe.Pointer addr, uintptr length) {
    return mapper.munmap((uintptr)addr, length);
}

public static (nint n, error err) Read(nint fd, slice<byte> p) {
    nint n = default;
    error err = default;

    (n, err) = read(fd, p);
    if (raceenabled) {
        if (n > 0) {
            raceWriteRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
        }
        if (err == nil) {
            raceAcquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
        }
    }
    return (n, err);
}

public static (nint n, error err) Write(nint fd, slice<byte> p) {
    nint n = default;
    error err = default;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    (n, err) = write(fd, p);
    if (raceenabled && n > 0) {
        raceReadRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
    }
    return (n, err);
}

public static (nint n, error err) Pread(nint fd, slice<byte> p, int64 offset) {
    nint n = default;
    error err = default;

    (n, err) = pread(fd, p, offset);
    if (raceenabled) {
        if (n > 0) {
            raceWriteRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
        }
        if (err == nil) {
            raceAcquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
        }
    }
    return (n, err);
}

public static (nint n, error err) Pwrite(nint fd, slice<byte> p, int64 offset) {
    nint n = default;
    error err = default;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    (n, err) = pwrite(fd, p, offset);
    if (raceenabled && n > 0) {
        raceReadRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
    }
    return (n, err);
}

public static readonly ж<bool> ᏑSocketDisableIPv6 = new StaticBox<bool>(default(bool));
public static ref bool SocketDisableIPv6 => ref ((StaticBox<bool>)ᏑSocketDisableIPv6).ValueSlot;

partial interface Sockaddr {
    (@unsafe.Pointer ptr, _Socklen len, error err) sockaddr();
}

partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr = new(4);
    internal RawSockaddrInet4 raw;
}

partial struct SockaddrInet6 {
    public nint Port;
    public uint32 ZoneId;
    public array<byte> Addr = new(16);
    internal RawSockaddrInet6 raw;
}

partial struct SockaddrUnix {
    public @string Name;
    internal RawSockaddrUnix raw;
}

public static error /*err*/ Bind(nint fd, Sockaddr sa) {
    error err = default;

    (var ptr, var n, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return bind(fd, ptr, n);
}

public static error /*err*/ Connect(nint fd, Sockaddr sa) {
    error err = default;

    (var ptr, var n, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return connect(fd, ptr, n);
}

public static (Sockaddr sa, error err) Getpeername(nint fd) {
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var len = ref heap(new _Socklen(), out var Ꮡlen);
    len = SizeofSockaddrAny;
    err = getpeername(fd, Ꮡrsa, Ꮡlen); if (err != nil) {
        return (sa, err);
    }
    return anyToSockaddr(fd, Ꮡrsa);
}

public static (byte value, error err) GetsockoptByte(nint fd, nint level, nint opt) {
    error err = default;

    ref var n = ref heap(new byte(), out var Ꮡn);
    ref var vallen = ref heap<_Socklen>((_Socklen)1, out var Ꮡvallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), Ꮡvallen);
    return (n, err);
}

public static (nint value, error err) GetsockoptInt(nint fd, nint level, nint opt) {
    error err = default;

    ref var n = ref heap(new int32(), out var Ꮡn);
    ref var vallen = ref heap<_Socklen>((_Socklen)4, out var Ꮡvallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), Ꮡvallen);
    return ((nint)n, err);
}

public static (array<byte> value, error err) GetsockoptInet4Addr(nint fd, nint level, nint opt) {
    ref var value = ref heap(new array<byte>(4), out var Ꮡvalue);
    error err = default;

    ref var vallen = ref heap<_Socklen>((_Socklen)4, out var Ꮡvallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)), Ꮡvallen);
    return (value.Clone(), err);
}

public static (ж<IPMreq>, error) GetsockoptIPMreq(nint fd, nint level, nint opt) {
    ref var value = ref heap(new IPMreq(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofIPMreq, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<IPv6Mreq>, error) GetsockoptIPv6Mreq(nint fd, nint level, nint opt) {
    ref var value = ref heap(new IPv6Mreq(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofIPv6Mreq, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<IPv6MTUInfo>, error) GetsockoptIPv6MTUInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new IPv6MTUInfo(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofIPv6MTUInfo, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<ICMPv6Filter>, error) GetsockoptICMPv6Filter(nint fd, nint level, nint opt) {
    ref var value = ref heap(new ICMPv6Filter(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofICMPv6Filter, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<Linger>, error) GetsockoptLinger(nint fd, nint level, nint opt) {
    ref var linger = ref heap(new Linger(), out var Ꮡlinger);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofLinger, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡlinger), Ꮡvallen);
    return (Ꮡlinger, err);
}

public static (ж<Timeval>, error) GetsockoptTimeval(nint fd, nint level, nint opt) {
    ref var tv = ref heap(new Timeval(), out var Ꮡtv);
    ref var vallen = ref heap<_Socklen>((_Socklen)(uint32)/* unsafe.Sizeof(tv) */ (uintptr)16, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡtv), Ꮡvallen);
    return (Ꮡtv, err);
}

public static (uint64 value, error err) GetsockoptUint64(nint fd, nint level, nint opt) {
    error err = default;

    ref var n = ref heap(new uint64(), out var Ꮡn);
    ref var vallen = ref heap<_Socklen>((_Socklen)8, out var Ꮡvallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), Ꮡvallen);
    return (n, err);
}

public static (nint n, Sockaddr from, error err) Recvfrom(nint fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var len = ref heap(new _Socklen(), out var Ꮡlen);
    len = SizeofSockaddrAny;
    (n, err) = recvfrom(fd, p, flags, Ꮡrsa, Ꮡlen); if (err != nil) {
        return (n, from, err);
    }
    if (rsa.Addr.Family != AF_UNSPEC) {
        (from, err) = anyToSockaddr(fd, Ꮡrsa);
    }
    return (n, from, err);
}

public static (nint n, nint oobn, nint recvflags, Sockaddr from, error err) Recvmsg(nint fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    Sockaddr from = default;
    error err = default;

    array<Iovec> iov = new(1);
    if (len(p) > 0) {
        iov[0].Base = Ꮡ(p, 0);
        iov[0].SetLen(len(p));
    }
    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    (n, oobn, recvflags, err) = recvmsgRaw(fd, iov[..], oob, flags, Ꮡrsa); if (err != nil) {
        return (n, oobn, recvflags, from, err);
    }
    if (rsa.Addr.Family != AF_UNSPEC) {
        (from, err) = anyToSockaddr(fd, Ꮡrsa);
    }
    return (n, oobn, recvflags, from, err);
}

public static (nint n, nint oobn, nint recvflags, Sockaddr from, error err) RecvmsgBuffers(nint fd, slice<slice<byte>> buffers, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    Sockaddr from = default;
    error err = default;

    var iov = new slice<Iovec>(len(buffers));
    foreach (var i in range(len(buffers))) {
        if (len(buffers[i]) > 0) {
            iov[i].Base = Ꮡ(buffers[i], 0);
            iov[i].SetLen(len(buffers[i]));
        } else {
            iov[i].Base = Ꮡ_zero.Reinterpret<uintptr, byte>();
        }
    }
    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    (n, oobn, recvflags, err) = recvmsgRaw(fd, iov, oob, flags, Ꮡrsa); if (err != nil) {
        return (n, oobn, recvflags, from, err);
    }
    if (rsa.Addr.Family != AF_UNSPEC) {
        (from, err) = anyToSockaddr(fd, Ꮡrsa);
    }
    return (n, oobn, recvflags, from, err);
}

public static error /*err*/ Sendmsg(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default;

    (_, err) = SendmsgN(fd, p, oob, to, flags);
    return err;
}

public static (nint n, error err) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default;

    array<Iovec> iov = new(1);
    if (len(p) > 0) {
        iov[0].Base = Ꮡ(p, 0);
        iov[0].SetLen(len(p));
    }
    @unsafe.Pointer ptr = default;
    _Socklen salen = default;
    if (to != nil) {
        (ptr, salen, err) = to.sockaddr();
        if (err != nil) {
            return (0, err);
        }
    }
    return sendmsgN(fd, iov[..], oob, ptr, salen, flags);
}

public static (nint n, error err) SendmsgBuffers(nint fd, slice<slice<byte>> buffers, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default;

    var iov = new slice<Iovec>(len(buffers));
    foreach (var i in range(len(buffers))) {
        if (len(buffers[i]) > 0) {
            iov[i].Base = Ꮡ(buffers[i], 0);
            iov[i].SetLen(len(buffers[i]));
        } else {
            iov[i].Base = Ꮡ_zero.Reinterpret<uintptr, byte>();
        }
    }
    @unsafe.Pointer ptr = default;
    _Socklen salen = default;
    if (to != nil) {
        (ptr, salen, err) = to.sockaddr();
        if (err != nil) {
            return (0, err);
        }
    }
    return sendmsgN(fd, iov, oob, ptr, salen, flags);
}

public static error /*err*/ Send(nint s, slice<byte> buf, nint flags) {
    return sendto(s, buf, flags, nil, 0);
}

public static error /*err*/ Sendto(nint fd, slice<byte> p, nint flags, Sockaddr to) {
    error err = default;

    @unsafe.Pointer ptr = default;
    _Socklen salen = default;
    if (to != nil) {
        (ptr, salen, err) = to.sockaddr();
        if (err != nil) {
            return err;
        }
    }
    return sendto(fd, p, flags, ptr, salen);
}

public static error /*err*/ SetsockoptByte(nint fd, nint level, nint opt, byte valueʗp) {
    ref var value = ref heap(valueʗp, out var Ꮡvalue);

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), 1);
}

public static error /*err*/ SetsockoptInt(nint fd, nint level, nint opt, nint value) {
    ref var n = ref heap(new int32(), out var Ꮡn);

    n = (int32)value;
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), 4);
}

public static error /*err*/ SetsockoptInet4Addr(nint fd, nint level, nint opt, /*[4]*/ array<byte> valueʗp) {
    ref var value = ref heap(valueʗp.Clone(), out var Ꮡvalue);

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)), 4);
}

public static error /*err*/ SetsockoptIPMreq(nint fd, nint level, nint opt, ж<IPMreq> mreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(mreq), SizeofIPMreq);
}

public static error /*err*/ SetsockoptIPv6Mreq(nint fd, nint level, nint opt, ж<IPv6Mreq> mreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(mreq), SizeofIPv6Mreq);
}

public static error SetsockoptICMPv6Filter(nint fd, nint level, nint opt, ж<ICMPv6Filter> filter) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(filter), SizeofICMPv6Filter);
}

public static error /*err*/ SetsockoptLinger(nint fd, nint level, nint opt, ж<Linger> l) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(l), SizeofLinger);
}

public static error /*err*/ SetsockoptString(nint fd, nint level, nint opt, @string s) {
    @unsafe.Pointer p = default;
    if (len(s) > 0) {
        p = @unsafe.Pointer.FromPinnedBox(Ꮡ(slice<byte>(s), 0));
    }
    return setsockopt(fd, level, opt, p, (uintptr)len(s));
}

public static error /*err*/ SetsockoptTimeval(nint fd, nint level, nint opt, ж<Timeval> tv) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(tv), /* unsafe.Sizeof(*tv) */ (uintptr)16);
}

public static error /*err*/ SetsockoptUint64(nint fd, nint level, nint opt, uint64 valueʗp) {
    ref var value = ref heap(valueʗp, out var Ꮡvalue);

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), 8);
}

public static (nint fd, error err) Socket(nint domain, nint typ, nint proto) {
    nint fd = default;
    error err = default;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (-1, EAFNOSUPPORT);
    }
    (fd, err) = socket(domain, typ, proto);
    return (fd, err);
}

public static (array<nint> fd, error err) Socketpair(nint domain, nint typ, nint proto) {
    array<nint> fd = new(2);
    error err = default;

    ref var fdx = ref heap(new array<int32>(2), out var Ꮡfdx);
    err = socketpair(domain, typ, proto, Ꮡfdx);
    if (err == nil) {
        fd[0] = (nint)fdx[0];
        fd[1] = (nint)fdx[1];
    }
    return (fd, err);
}

internal static readonly ж<int64> ᏑioSync = new StaticBox<int64>(default(int64));
internal static ref int64 ioSync => ref ((StaticBox<int64>)ᏑioSync).ValueSlot;

public static void CloseOnExec(nint fd) { fcntl(fd, F_SETFD, FD_CLOEXEC); }

public static error /*err*/ SetNonblock(nint fd, bool nonblocking) {
    error err = default;

    (var flag, err) = fcntl(fd, F_GETFL, 0);
    if (err != nil) {
        return err;
    }
    if (((flag & (nint)O_NONBLOCK) != 0) == nonblocking) {
        return default;
    }
    if (nonblocking) {
        flag |= (nint)(O_NONBLOCK);
    } else {
        flag &= (nint)(~O_NONBLOCK);
    }
    (_, err) = fcntl(fd, F_SETFL, flag);
    return err;
}

public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    return syscall.Exec(argv0, argv, envv);
}

public static error Lutimes(@string path, slice<Timeval> tv) {
    if (tv == nil) {
        return UtimesNanoAt(AT_FDCWD, path, default, AT_SYMLINK_NOFOLLOW);
    }
    if (len(tv) != 2) {
        return EINVAL;
    }
    var ts = slice<Timespec>([
        NsecToTimespec(TimevalToNsec(tv[0])),
        NsecToTimespec(TimevalToNsec(tv[1]))
    ]);
    return UtimesNanoAt(AT_FDCWD, path, ts, AT_SYMLINK_NOFOLLOW);
}

internal static bool emptyIovecs(slice<Iovec> iov) {
    foreach (var i in range(len(iov))) {
        if (iov[i].Len > 0) {
            return false;
        }
    }
    return true;
}

public static error Setrlimit(nint resource, ж<Rlimit> rlim) {
    return syscall.Setrlimit(resource, rlim.Reinterpret<Rlimit, syscall.Rlimit>());
}

} // end unix_package
