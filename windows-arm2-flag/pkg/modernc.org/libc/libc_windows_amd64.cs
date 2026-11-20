// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using largeInteger = go.modernc.org.libc_package.largeIntegerᴛ1;

namespace go.modernc.org;

using windows = golang.org.x.sys.windows_package;
using math = math_package;
using os = os_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using errno = go.modernc.org.libc.errno_package;
using types = go.modernc.org.libc.sys.types_package;
using System.Runtime.InteropServices;
using go.golib;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using golang.org.x.sys;
using ꓸꓸꓸany = Span<any>;

partial class libc_package {

public static partial int32 Xsigaction(ж<TLS> t, int32 signum, uintptr act, uintptr oldact) {
    if (__ccgo_strace) {
        trc("t=%v signum=%v oldact=%v, (%v:)", t.OrTypedNil(), signum, oldact, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfcntl64(ж<TLS> t, int32 fd, int32 cmd, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v cmd=%v args=%v, (%v:)", t.OrTypedNil(), cmd, args, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xlstat64(ж<TLS> t, uintptr pathname, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v statbuf=%v, (%v:)", t.OrTypedNil(), statbuf, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xstat64(ж<TLS> t, uintptr pathname, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v statbuf=%v, (%v:)", t.OrTypedNil(), statbuf, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfstat64(ж<TLS> t, int32 fd, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v statbuf=%v, (%v:)", t.OrTypedNil(), fd, statbuf, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xmmap(ж<TLS> t, uintptr addr, typesꓸSize_t length, int32 prot, int32 flags, int32 fd, typesꓸOff_t offset) {
    if (__ccgo_strace) {
        trc("t=%v addr=%v length=%v fd=%v offset=%v, (%v:)", t.OrTypedNil(), addr, length, fd, offset, origin(2));
    }
    return Xmmap64(t, addr, length, prot, flags, fd, offset);
}

public static partial uintptr Xmmap64(ж<TLS> t, uintptr addr, typesꓸSize_t length, int32 prot, int32 flags, int32 fd, typesꓸOff_t offset) {
    if (__ccgo_strace) {
        trc("t=%v addr=%v length=%v fd=%v offset=%v, (%v:)", t.OrTypedNil(), addr, length, fd, offset, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xmremap(ж<TLS> t, uintptr old_address, typesꓸSize_t old_size, typesꓸSize_t new_size, int32 flags, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v old_address=%v new_size=%v flags=%v args=%v, (%v:)", t.OrTypedNil(), old_address, new_size, flags, args, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xftruncate64(ж<TLS> t, int32 fd, typesꓸOff_t length) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v length=%v, (%v:)", t.OrTypedNil(), fd, length, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸOff_t Xlseek64(ж<TLS> t, int32 fd, typesꓸOff_t offset, int32 whence) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v offset=%v whence=%v, (%v:)", t.OrTypedNil(), fd, offset, whence, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var (n, err) = windows.Seek(f.Handle, offset, (nint)whence);
    if (err != nil) {
        if (dmesgs) {
            dmesg("%v: fd %v, off %#x, whence %v: %v", origin(1), f._fd, offset, whenceStr(whence), n);
        }
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: fd %v, off %#x, whence %v: ok", origin(1), f._fd, offset, whenceStr(whence));
    }
    return n;
}

public static partial int32 Xutime(ж<TLS> t, uintptr filename, uintptr times) {
    if (__ccgo_strace) {
        trc("t=%v times=%v, (%v:)", t.OrTypedNil(), times, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 Xalarm(ж<TLS> t, uint32 seconds) {
    if (__ccgo_strace) {
        trc("t=%v seconds=%v, (%v:)", t.OrTypedNil(), seconds, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸTime_t Xtime(ж<TLS> t, uintptr tloc) {
    if (__ccgo_strace) {
        trc("t=%v tloc=%v, (%v:)", t.OrTypedNil(), tloc, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgetrlimit64(ж<TLS> t, int32 resource, uintptr rlim) {
    if (__ccgo_strace) {
        trc("t=%v resource=%v rlim=%v, (%v:)", t.OrTypedNil(), resource, rlim, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmkdir(ж<TLS> t, uintptr path, typesꓸMode_t mode) {
    if (__ccgo_strace) {
        trc("t=%v path=%v mode=%v, (%v:)", t.OrTypedNil(), path, mode, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xsymlink(ж<TLS> t, uintptr target, uintptr linkpath) {
    if (__ccgo_strace) {
        trc("t=%v linkpath=%v, (%v:)", t.OrTypedNil(), linkpath, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xutimes(ж<TLS> t, uintptr filename, uintptr times) {
    if (__ccgo_strace) {
        trc("t=%v times=%v, (%v:)", t.OrTypedNil(), times, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xunlink(ж<TLS> t, uintptr pathname) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v, (%v:)", t.OrTypedNil(), pathname, origin(2));
    }

    var err = windows.DeleteFile(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)pathname));
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %q: ok", origin(1), GoString(pathname));
    }

    return 0;
}

public static partial int32 Xrmdir(ж<TLS> t, uintptr pathname) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v, (%v:)", t.OrTypedNil(), pathname, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmknod(ж<TLS> t, uintptr pathname, typesꓸMode_t mode, typesꓸDev_t dev) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v mode=%v dev=%v, (%v:)", t.OrTypedNil(), pathname, mode, dev, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xlink(ж<TLS> t, uintptr oldpath, uintptr newpath) {
    if (__ccgo_strace) {
        trc("t=%v newpath=%v, (%v:)", t.OrTypedNil(), newpath, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xpipe(ж<TLS> t, uintptr pipefd) {
    if (__ccgo_strace) {
        trc("t=%v pipefd=%v, (%v:)", t.OrTypedNil(), pipefd, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xdup2(ж<TLS> t, int32 oldfd, int32 newfd) {
    if (__ccgo_strace) {
        trc("t=%v oldfd=%v newfd=%v, (%v:)", t.OrTypedNil(), oldfd, newfd, origin(2));
    }
    return dup2(t, oldfd, newfd);
}

public static partial typesꓸSsize_t Xreadlink(ж<TLS> t, uintptr path, uintptr buf, typesꓸSize_t bufsize) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v bufsize=%v, (%v:)", t.OrTypedNil(), buf, bufsize, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xfopen64(ж<TLS> t, uintptr pathname, uintptr mode) {
    if (__ccgo_strace) {
        trc("t=%v mode=%v, (%v:)", t.OrTypedNil(), mode, origin(2));
    }

    @string m = strings.ReplaceAll(GoString(mode), "b", "");
    nint flags = default;
    switch (ᐧ) {
    case {} when m == "r"u8: {
        flags = os.O_RDONLY;
        break;
    }
    case {} when m == "r+"u8: {
        flags = os.O_RDWR;
        break;
    }
    case {} when m == "w"u8: {
        flags = (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC);
        break;
    }
    case {} when m == "w+"u8: {
        flags = (nint)((nint)(nint)(os.O_RDWR | os.O_CREATE) | os.O_TRUNC);
        break;
    }
    case {} when m == "a"u8: {
        flags = (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_APPEND);
        break;
    }
    case {} when m == "a+"u8: {
        flags = (nint)((nint)(nint)(os.O_RDWR | os.O_CREATE) | os.O_APPEND);
        break;
    }
    default: {
        throw panic(m);
        break;
    }}

    var (h, err) = windows.Open(GoString(pathname), (nint)flags, (uint32)438);
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }

    var (p, _) = wrapFdHandle(h);
    if (p != 0) {
        return p;
    }
    _ = windows.Close(h);
    t.setErrno((nint)(errno.ENOMEM));
    return 0;
}

public static partial int32 Xrecv(ж<TLS> t, uint64 sockfd, uintptr buf, int32 len, int32 flags) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v buf=%v flags=%v, (%v:)", t.OrTypedNil(), sockfd, buf, flags, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xsend(ж<TLS> t, uint64 sockfd, uintptr buf, int32 len, int32 flags) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v buf=%v flags=%v, (%v:)", t.OrTypedNil(), sockfd, buf, flags, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xshutdown(ж<TLS> t, uint64 sockfd, int32 how) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v how=%v, (%v:)", t.OrTypedNil(), sockfd, how, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgetpeername(ж<TLS> t, uint64 sockfd, uintptr addr, uintptr addrlen) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v addr=%v addrlen=%v, (%v:)", t.OrTypedNil(), sockfd, addr, addrlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgetsockname(ж<TLS> t, uint64 sockfd, uintptr addr, uintptr addrlen) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v addrlen=%v, (%v:)", t.OrTypedNil(), sockfd, addrlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint64 Xsocket(ж<TLS> t, int32 domain, int32 type1, int32 protocol) {
    if (__ccgo_strace) {
        trc("t=%v protocol=%v, (%v:)", t.OrTypedNil(), protocol, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xbind(ж<TLS> t, uint64 sockfd, uintptr addr, int32 addrlen) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v addr=%v addrlen=%v, (%v:)", t.OrTypedNil(), sockfd, addr, addrlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xconnect(ж<TLS> t, uint64 sockfd, uintptr addr, int32 addrlen) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v addr=%v addrlen=%v, (%v:)", t.OrTypedNil(), sockfd, addr, addrlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xlisten(ж<TLS> t, uint64 sockfd, int32 backlog) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v backlog=%v, (%v:)", t.OrTypedNil(), sockfd, backlog, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint64 Xaccept(ж<TLS> t, uint64 sockfd, uintptr addr, uintptr addrlen) {
    if (__ccgo_strace) {
        trc("t=%v sockfd=%v addr=%v addrlen=%v, (%v:)", t.OrTypedNil(), sockfd, addr, addrlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial int64 XDefWindowProcW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int64 XSendMessageTimeoutW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial size_t Xstrspn(ж<TLS> tls, uintptr s, uintptr c) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v s=%v c=%v, (%v:)", tls.OrTypedNil(), s, c, origin(2));
        }
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);

        uintptr a = s;
        @unsafe.Pointer.StoreArray<size_t>((@unsafe.Pointer)bp, 4, new array<size_t>(4){[0] = (uint64)0});

        if (!((int32)(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)c)) != 0)) {
            return (uint64)0;
        }
        if (!((int32)(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)(c + 1))) != 0)) {
            for (; (int32)(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)s)) == (int32)(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)c)); s++) {
            }
            return (size_t)(((int64)s - (int64)a) / 1);
        }

        for (; ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)c) != 0 && AssignOrPtrUint64(bp + (uintptr)(((size_t)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)c))) / ((uint64)8 * (uint64)/* unsafe.Sizeof(size_t(0)) */ (uintptr)8)) * 8, ((size_t)(uint64)1) << (int)(((size_t)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)c))) % ((uint64)8 * (uint64)/* unsafe.Sizeof(size_t(0)) */ (uintptr)8))) != 0; c++) {
        }
        for (; ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)s) != 0 && (size_t)(~@unsafe.Pointer.To<size_t>((@unsafe.Pointer)(bp + (uintptr)(((size_t)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)s))) / ((uint64)8 * (uint64)/* unsafe.Sizeof(size_t(0)) */ (uintptr)8)) * 8)) & (((size_t)(uint64)1) << (int)(((size_t)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)s))) % ((uint64)8 * (uint64)/* unsafe.Sizeof(size_t(0)) */ (uintptr)8)))) != 0; s++) {
        }
        return (size_t)(((int64)s - (int64)a) / 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial float64 Xstrtod(ж<TLS> t, uintptr s, uintptr p) {
    if (__ccgo_strace) {
        trc("tls=%v s=%v p=%v, (%v:)", t.OrTypedNil(), s, p, origin(2));
    }
    var (_, r2, err) = procStrtod.Call((uintptr)s, (uintptr)p);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return math.Float64frombits((uint64)r2);
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct largeIntegerᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] public uint32 FLowPart;
    [FieldOffset(4)] public int32 FHighPart;
}


[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct largeIntegerᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<largeIntegerᴛ1_Fu>))] public readonly int Fu;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int64>))] public readonly long FQuadPart;
    [FieldOffset(0)] public largeIntegerᴛ1_Fu F__ccgo0_0;
}

public static partial int32 XSetFilePointerEx(ж<TLS> t, uintptr hFile, largeInteger liDistanceToMove, uintptr lpNewFilePointer, uint32 dwMoveMethod) {
    var d = (int64)liDistanceToMove.F__ccgo0_0.FHighPart << 32 | (int64)liDistanceToMove.F__ccgo0_0.FLowPart;
    if (__ccgo_strace) {
        trc("t=%v hFile=%v liDistanceToMove=%v lpNewFilePointer=%v dwMoveMethod=%v, (%v:)", t.OrTypedNil(), hFile, d, lpNewFilePointer, dwMoveMethod, origin(2));
    }
    var (r0, _, e1) = procSetFilePointerEx.Call(hFile, (uintptr)d, lpNewFilePointer, (uintptr)dwMoveMethod);
    if (r0 == 0) {
        t.setErrno(e1);
    }
    return (int32)r0;
}

public static partial uint64 XGetTickCount64(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, _) = procGetTickCount64.Call();
    return (uint64)r0;
}

} // end libc_package
