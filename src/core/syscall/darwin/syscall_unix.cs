// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using errorspkg = errors_package;
using asan = @internal.asan_package;
using bytealg = @internal.bytealg_package;
using itoa = @internal.itoa_package;
using msan = @internal.msan_package;
using oserror = @internal.oserror_package;
using race = @internal.race_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸoserror() {
    builtin.initPackage(typeof(@internal.oserror_package));
}

public static nint Stdin = 0;
public static nint Stdout = 1;
public static nint Stderr = 2;

internal const bool darwin64Bit = /* (runtime.GOOS == "darwin" || runtime.GOOS == "ios") && sizeofPtr == 8 */ true;
internal const bool netbsd32Bit = /* runtime.GOOS == "netbsd" && sizeofPtr == 4 */ false;

// clen returns the index of the first NULL byte in n or len(n) if n contains no NULL byte.
internal static nint clen(slice<byte> n) {
    {
        nint i = bytealg.IndexByte(n, 0); if (i != -1) {
            return i;
        }
    }
    return len(n);
}

// Mmap manager, for use by operating system-specific implementations.
[GoType] partial struct mmapper {
    public partial ref sync_package.Mutex Mutex { get; }
    internal map<ж<byte>, slice<byte>> active; // active mappings; key is last byte in mapping
    internal Func<uintptr, uintptr, nint, nint, nint, int64, (uintptr, error)> mmap;
    internal Func<uintptr, uintptr, error> munmap;
}

internal static (slice<byte> data, error err) Mmap(this ж<mmapper> Ꮡm, nint fd, int64 offset, nint length, nint prot, nint flags) {
    slice<byte> data = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        if (length <= 0) {
            (data, err) = (default!, EINVAL); goto ᒐdone;
        }
        // Map the requested memory.
        var (addr, errno) = m.mmap(0, (uintptr)length, prot, flags, fd, offset);
        if (errno != default!) {
            (data, err) = (default!, errno); goto ᒐdone;
        }
        // Use unsafe to turn addr into a []byte.
        var b = @unsafe.Slice((ж<byte>)(uintptr)((@unsafe.Pointer)addr), length);
        // Register mapping in m and return it.
        var p = Ꮡ(b, cap(b) - 1);
        Ꮡm.of(mmapper.ᏑMutex).Lock();
        defer(Ꮡm.of(mmapper.ᏑMutex).Unlock, ref ᒐ);
        m.active[p] = b;
        (data, err) = (b, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (data, err);
}

internal static error /*err*/ Munmap(this ж<mmapper> Ꮡm, slice<byte> data) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        if (len(data) == 0 || len(data) != cap(data)) {
            err = EINVAL; goto ᒐdone;
        }
        // Find the base of the mapping.
        var p = Ꮡ(data, cap(data) - 1);
        Ꮡm.of(mmapper.ᏑMutex).Lock();
        defer(Ꮡm.of(mmapper.ᏑMutex).Unlock, ref ᒐ);
        var b = m.active[p];
        if (b == default! || Ꮡ(b, 0) != Ꮡ(data, 0)) {
            err = EINVAL; goto ᒐdone;
        }
        // Unmap the memory and update m.
        {
            var errno = m.munmap((uintptr)Ꮡ(b, 0), (uintptr)len(b)); if (errno != default!) {
                err = errno; goto ᒐdone;
            }
        }
        delete(m.active, p);
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

[GoType("num:uintptr")] partial struct Errno;

public static @string Error(this Errno e) {
    if (0 <= (nint)(uintptr)e && (nint)(uintptr)e < len(errors)) {
        @string s = errors[e];
        if (s != ""u8) {
            return s;
        }
    }
    return "errno "u8 + itoa.Itoa((nint)(uintptr)e);
}

public static bool Is(this Errno e, error target) {
    var exprᴛ1 = target;
    if (AreEqual(exprᴛ1, oserror.ErrPermission)) {
        return e == EACCES || e == EPERM;
    }
    if (AreEqual(exprᴛ1, oserror.ErrExist)) {
        return e == EEXIST || e == ENOTEMPTY;
    }
    if (AreEqual(exprᴛ1, oserror.ErrNotExist)) {
        return e == ENOENT;
    }
    if (AreEqual(exprᴛ1, errorspkg.ErrUnsupported)) {
        return e == ENOSYS || e == ENOTSUP || e == EOPNOTSUPP;
    }

    return false;
}

public static bool Temporary(this Errno e) {
    return e == EINTR || e == EMFILE || e == ENFILE || e.Timeout();
}

public static bool Timeout(this Errno e) {
    return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
}

// Do the interface allocations only once for common
// Errno values.
internal static error errEAGAIN = ((Errno)EAGAIN);

internal static error errEINVAL = ((Errno)EINVAL);

internal static error errENOENT = ((Errno)ENOENT);

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
internal static error errnoErr(Errno e) {
    var exprᴛ1 = e;
    if (exprᴛ1 == (Errno)(0)) {
        return default!;
    }
    if (exprᴛ1 == EAGAIN) {
        return errEAGAIN;
    }
    if (exprᴛ1 == EINVAL) {
        return errEINVAL;
    }
    if (exprᴛ1 == ENOENT) {
        return errENOENT;
    }

    return e;
}

[GoType("num:nint")] partial struct ΔSignal;

public static void Signal(this ΔSignal s) {
}

public static @string String(this ΔSignal s) {
    if (0 <= s && (nint)s < len(signals)) {
        @string str = signals[s];
        if (str != ""u8) {
            return str;
        }
    }
    return "signal "u8 + itoa.Itoa((nint)s);
}

public static (nint n, error err) Read(nint fd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = read(fd, p);
    if (race.Enabled) {
        if (n > 0) {
            race.WriteRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
        }
        if (err == default!) {
            race.Acquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
        }
    }
    if (msan.Enabled && n > 0) {
        msan.Write(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    if (asan.Enabled && n > 0) {
        asan.Write(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    return (n, err);
}

public static (nint n, error err) Write(nint fd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    if (faketime && (fd == 1 || fd == 2)){
        n = faketimeWrite(fd, p);
        if (n < 0) {
            (n, err) = (0, errnoErr(((Errno)(uintptr)(-n))));
        }
    } else {
        (n, err) = write(fd, p);
    }
    if (race.Enabled && n > 0) {
        race.ReadRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
    }
    if (msan.Enabled && n > 0) {
        msan.Read(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    if (asan.Enabled && n > 0) {
        asan.Read(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    return (n, err);
}

public static (nint n, error err) Pread(nint fd, slice<byte> p, int64 offset) {
    nint n = default!;
    error err = default!;

    (n, err) = pread(fd, p, offset);
    if (race.Enabled) {
        if (n > 0) {
            race.WriteRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
        }
        if (err == default!) {
            race.Acquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
        }
    }
    if (msan.Enabled && n > 0) {
        msan.Write(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    if (asan.Enabled && n > 0) {
        asan.Write(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    return (n, err);
}

public static (nint n, error err) Pwrite(nint fd, slice<byte> p, int64 offset) {
    nint n = default!;
    error err = default!;

    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    (n, err) = pwrite(fd, p, offset);
    if (race.Enabled && n > 0) {
        race.ReadRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), n);
    }
    if (msan.Enabled && n > 0) {
        msan.Read(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    if (asan.Enabled && n > 0) {
        asan.Read(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (uintptr)n);
    }
    return (n, err);
}

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return [EAFNOSUPPORT].
public static bool SocketDisableIPv6;

[GoType] partial interface Sockaddr {
    (@unsafe.Pointer ptr, _Socklen len, error err) sockaddr(); // lowercase; only we can define Sockaddrs
}

[GoType] partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr = new(4);
    internal RawSockaddrInet4 raw;
}

[GoType] partial struct SockaddrInet6 {
    public nint Port;
    public uint32 ZoneId;
    public array<byte> Addr = new(16);
    internal RawSockaddrInet6 raw;
}

[GoType] partial struct SockaddrUnix {
    public @string Name;
    internal RawSockaddrUnix raw;
}

// go2cs generated this placeholder — func Bind is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Connect is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Getpeername is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static (nint value, error err) GetsockoptInt(nint fd, nint level, nint opt) {
    error err = default!;

    ref var n = ref heap(new int32(), out var Ꮡn);
    ref var vallen = ref heap<_Socklen>(out var Ꮡvallen);
    vallen = ((_Socklen)4);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), Ꮡvallen);
    return ((nint)n, err);
}

// go2cs generated this placeholder — func Recvfrom is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (nint n, error err) recvfromInet4(nint fd, slice<byte> p, nint flags, ref SockaddrInet4 from) {
    nint n = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var socklen = ref heap(new _Socklen(), out var Ꮡsocklen);
    socklen = SizeofSockaddrAny;
    {
        (n, err) = recvfrom(fd, p, flags, Ꮡrsa, Ꮡsocklen); if (err != default!) {
            return (n, err);
        }
    }
    var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>();
    var port = (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(pp.of(RawSockaddrInet4.ᏑPort)));
    from.Port = ((nint)port.Value[0] << (int)(8)) + (nint)port.Value[1];
    from.Addr = pp.Value.Addr.Clone();
    return (n, err);
}

internal static (nint n, error err) recvfromInet6(nint fd, slice<byte> p, nint flags, ref SockaddrInet6 from) {
    nint n = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var socklen = ref heap(new _Socklen(), out var Ꮡsocklen);
    socklen = SizeofSockaddrAny;
    {
        (n, err) = recvfrom(fd, p, flags, Ꮡrsa, Ꮡsocklen); if (err != default!) {
            return (n, err);
        }
    }
    var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet6>();
    var port = (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(pp.of(RawSockaddrInet6.ᏑPort)));
    from.Port = ((nint)port.Value[0] << (int)(8)) + (nint)port.Value[1];
    from.ZoneId = pp.Value.Scope_id;
    from.Addr = pp.Value.Addr.Clone();
    return (n, err);
}

internal static (nint n, nint oobn, nint recvflags, error err) recvmsgInet4(nint fd, slice<byte> p, slice<byte> oob, nint flags, ref SockaddrInet4 from) {
    nint n = default!;
    nint oobn = default!;
    nint recvflags = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    (n, oobn, recvflags, err) = recvmsgRaw(fd, p, oob, flags, Ꮡrsa);
    if (err != default!) {
        return (n, oobn, recvflags, err);
    }
    var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>();
    var port = (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(pp.of(RawSockaddrInet4.ᏑPort)));
    from.Port = ((nint)port.Value[0] << (int)(8)) + (nint)port.Value[1];
    from.Addr = pp.Value.Addr.Clone();
    return (n, oobn, recvflags, err);
}

internal static (nint n, nint oobn, nint recvflags, error err) recvmsgInet6(nint fd, slice<byte> p, slice<byte> oob, nint flags, ref SockaddrInet6 from) {
    nint n = default!;
    nint oobn = default!;
    nint recvflags = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    (n, oobn, recvflags, err) = recvmsgRaw(fd, p, oob, flags, Ꮡrsa);
    if (err != default!) {
        return (n, oobn, recvflags, err);
    }
    var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet6>();
    var port = (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(pp.of(RawSockaddrInet6.ᏑPort)));
    from.Port = ((nint)port.Value[0] << (int)(8)) + (nint)port.Value[1];
    from.ZoneId = pp.Value.Scope_id;
    from.Addr = pp.Value.Addr.Clone();
    return (n, oobn, recvflags, err);
}

public static (nint n, nint oobn, nint recvflags, Sockaddr from, error err) Recvmsg(nint fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default!;
    nint oobn = default!;
    nint recvflags = default!;
    Sockaddr from = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    (n, oobn, recvflags, err) = recvmsgRaw(fd, p, oob, flags, Ꮡrsa);
    // source address is only specified if the socket is unconnected
    if (rsa.Addr.Family != AF_UNSPEC) {
        (from, err) = anyToSockaddr(Ꮡrsa);
    }
    return (n, oobn, recvflags, from, err);
}

public static error /*err*/ Sendmsg(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default!;

    (_, err) = SendmsgN(fd, p, oob, to, flags);
    return err;
}

// go2cs generated this placeholder — func SendmsgN is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (nint n, error err) sendmsgNInet4(nint fd, slice<byte> p, slice<byte> oob, ж<SockaddrInet4> Ꮡto, nint flags) {
    error err = default!;

    (var ptr, var salen, err) = Ꮡto.sockaddr();
    if (err != default!) {
        return (0, err);
    }
    return sendmsgN(fd, p, oob, ptr, salen, flags);
}

internal static (nint n, error err) sendmsgNInet6(nint fd, slice<byte> p, slice<byte> oob, ж<SockaddrInet6> Ꮡto, nint flags) {
    error err = default!;

    (var ptr, var salen, err) = Ꮡto.sockaddr();
    if (err != default!) {
        return (0, err);
    }
    return sendmsgN(fd, p, oob, ptr, salen, flags);
}

internal static error /*err*/ sendtoInet4(nint fd, slice<byte> p, nint flags, ж<SockaddrInet4> Ꮡto) {
    error err = default!;

    (var ptr, var n, err) = Ꮡto.sockaddr();
    if (err != default!) {
        return err;
    }
    return sendto(fd, p, flags, ptr, n);
}

internal static error /*err*/ sendtoInet6(nint fd, slice<byte> p, nint flags, ж<SockaddrInet6> Ꮡto) {
    error err = default!;

    (var ptr, var n, err) = Ꮡto.sockaddr();
    if (err != default!) {
        return err;
    }
    return sendto(fd, p, flags, ptr, n);
}

// go2cs generated this placeholder — func Sendto is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static error /*err*/ SetsockoptByte(nint fd, nint level, nint opt, byte valueʗp) {
    ref var value = ref heap(valueʗp, out var Ꮡvalue);

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), 1);
}

public static error /*err*/ SetsockoptInt(nint fd, nint level, nint opt, nint value) {
    ref var n = ref heap(new int32(), out var Ꮡn);

    n = (int32)value;
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡn), 4);
}

public static error /*err*/ SetsockoptInet4Addr(nint fd, nint level, nint opt, [GoArrayDims(4)] array<byte> valueʗp) {
    ref var value = ref heap(valueʗp.Clone(), out var Ꮡvalue);

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)), 4);
}

public static error /*err*/ SetsockoptIPMreq(nint fd, nint level, nint opt, ж<IPMreq> Ꮡmreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡmreq), SizeofIPMreq);
}

public static error /*err*/ SetsockoptIPv6Mreq(nint fd, nint level, nint opt, ж<IPv6Mreq> Ꮡmreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡmreq), SizeofIPv6Mreq);
}

public static error SetsockoptICMPv6Filter(nint fd, nint level, nint opt, ж<ICMPv6Filter> Ꮡfilter) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡfilter), SizeofICMPv6Filter);
}

public static error /*err*/ SetsockoptLinger(nint fd, nint level, nint opt, ж<Linger> Ꮡl) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡl), SizeofLinger);
}

public static error /*err*/ SetsockoptString(nint fd, nint level, nint opt, @string s) {
    @unsafe.Pointer p = default!;
    if (len(s) > 0) {
        p = @unsafe.Pointer.FromPinnedBox(Ꮡ(slice<byte>(s), 0));
    }
    return setsockopt(fd, level, opt, p, (uintptr)len(s));
}

public static error /*err*/ SetsockoptTimeval(nint fd, nint level, nint opt, ж<Timeval> Ꮡtv) {
    ref var tv = ref Ꮡtv.DerefOrNull();

    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡtv), /* unsafe.Sizeof(*tv) */ (uintptr)16);
}

public static (nint fd, error err) Socket(nint domain, nint typ, nint proto) {
    nint fd = default!;
    error err = default!;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (-1, EAFNOSUPPORT);
    }
    (fd, err) = socket(domain, typ, proto);
    return (fd, err);
}

public static (array<nint> fd, error err) Socketpair(nint domain, nint typ, nint proto) {
    array<nint> fd = new(2);
    error err = default!;

    ref var fdx = ref heap(new array<int32>(2), out var Ꮡfdx);
    err = socketpair(domain, typ, proto, Ꮡfdx);
    if (err == default!) {
        fd[0] = (nint)fdx[0];
        fd[1] = (nint)fdx[1];
    }
    return (fd, err);
}

public static (nint written, error err) Sendfile(nint outfd, nint infd, ж<int64> Ꮡoffset, nint count) {
    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    return sendfile(outfd, infd, ref (Ꮡoffset).DerefOrNull(), count);
}

internal static ж<int64> ᏑioSync = new StandardBox<int64>(default(int64));
internal static ref int64 ioSync => ref ᏑioSync.Value;

} // end syscall_package
