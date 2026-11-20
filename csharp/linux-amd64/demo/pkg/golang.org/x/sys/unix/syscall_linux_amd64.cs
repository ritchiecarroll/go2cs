// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;
using static unix_package;

partial class unix_package {

public static error /*err*/ Lstat(@string path, ж<Stat_t> stat) {
    return Fstatat(AT_FDCWD, path, stat, AT_SYMLINK_NOFOLLOW);
}

public static (nint n, error err) Select(nint nfd, ж<FdSet> r, ж<FdSet> w, ж<FdSet> e, ж<Timeval> Ꮡtimeout) {
    ref var timeout = ref Ꮡtimeout.DerefOrNull();

    ж<Timespec> ts = default;
    if (Ꮡtimeout != nil) {
        ts = Ꮡ(new Timespec(Sec: timeout.Sec, Nsec: timeout.Usec * 1000));
    }
    return pselect6(nfd, r, w, e, ts, nil);
}

public static error /*err*/ Stat(@string path, ж<Stat_t> stat) {
    return Fstatat(AT_FDCWD, path, stat, 0);
}

public static error /*err*/ Gettimeofday(ж<Timeval> tv) {
    var errno = gettimeofday(tv);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static (Time_t tt, error err) Time(ж<Time_t> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var tv = ref heap(new Timeval(), out var Ꮡtv);
    var errno = gettimeofday(Ꮡtv);
    if (errno != 0) {
        return (0, errno);
    }
    if (Ꮡt != nil) {
        t = (Time_t)tv.Sec;
    }
    return ((Time_t)tv.Sec, default);
}

internal static Timespec setTimespec(int64 sec, int64 nsec) {
    return new Timespec(Sec: sec, Nsec: nsec);
}

internal static Timeval setTimeval(int64 sec, int64 usec) {
    return new Timeval(Sec: sec, Usec: usec);
}

} partial class unixᴛPtraceRegs {

public static uint64 PC(this ref PtraceRegs r) { return r.Rip; }

public static void SetPC(this ref PtraceRegs r, uint64 pc) { r.Rip = pc; }

} partial class unixᴛIovec {

public static void SetLen(this ref Iovec iov, nint length) {
    iov.Len = (uint64)length;
}

} partial class unixᴛMsghdr {

public static void SetControllen(this ref Msghdr msghdr, nint length) {
    msghdr.Controllen = (uint64)length;
}

public static void SetIovlen(this ref Msghdr msghdr, nint length) {
    msghdr.Iovlen = (uint64)length;
}

} partial class unixᴛCmsghdr {

public static void SetLen(this ref Cmsghdr cmsg, nint length) {
    cmsg.Len = (uint64)length;
}

} partial class unixᴛRawSockaddrNFCLLCP {

public static void SetServiceNameLen(this ref RawSockaddrNFCLLCP rsa, nint length) {
    rsa.Service_name_len = (uint64)length;
}

} partial class unix_package {

public static error KexecFileLoad(nint kernelFd, nint initrdFd, @string cmdline, nint flags) {
    nint cmdlineLen = len(cmdline);
    if (cmdlineLen > 0) {
        cmdlineLen++;
    }
    return kexecFileLoad(kernelFd, initrdFd, cmdlineLen, cmdline, flags);
}

} // end unix_package
