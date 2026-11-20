namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

internal static syscall.Errno _ᴛ1ʗ;

internal static error /*err*/ fanotifyMark(nint fd, nuint flags, uint64 mask, nint dirFd, ж<byte> pathname) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_FANOTIFY_MARK, (uintptr)fd, (uintptr)flags, (uintptr)mask, (uintptr)dirFd, (uintptr)pathname, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fallocate(nint fd, uint32 mode, int64 off, int64 len) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_FALLOCATE, (uintptr)fd, (uintptr)mode, (uintptr)off, (uintptr)len, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int64 n, error err) Tee(nint rfd, nint wfd, nint len, nint flags) {
    int64 n = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_TEE, (uintptr)rfd, (uintptr)wfd, (uintptr)len, (uintptr)flags, 0, 0);
    n = (int64)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ Fadvise(nint fd, int64 offset, int64 length, nint advice) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_FADVISE64, (uintptr)fd, (uintptr)offset, (uintptr)length, (uintptr)advice, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fchown(nint fd, nint uid, nint gid) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FCHOWN, (uintptr)fd, (uintptr)uid, (uintptr)gid);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fstat(nint fd, ж<Stat_t> stat) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FSTAT, (uintptr)fd, (uintptr)stat, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fstatat(nint dirfd, @string path, ж<Stat_t> stat, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_NEWFSTATAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)stat, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fstatfs(nint fd, ж<Statfs_t> buf) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FSTATFS, (uintptr)fd, (uintptr)buf, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Ftruncate(nint fd, int64 length) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FTRUNCATE, (uintptr)fd, (uintptr)length, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static nint /*egid*/ Getegid() {
    nint egid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETEGID, 0, 0, 0);
    egid = (nint)r0;
    return egid;
}

public static nint /*euid*/ Geteuid() {
    nint euid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETEUID, 0, 0, 0);
    euid = (nint)r0;
    return euid;
}

public static nint /*gid*/ Getgid() {
    nint gid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETGID, 0, 0, 0);
    gid = (nint)r0;
    return gid;
}

public static error /*err*/ Getrlimit(nint resource, ж<Rlimit> rlim) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_GETRLIMIT, (uintptr)resource, (uintptr)rlim, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static nint /*uid*/ Getuid() {
    nint uid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETUID, 0, 0, 0);
    uid = (nint)r0;
    return uid;
}

public static error /*err*/ Ioperm(nint from, nint num, nint on) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_IOPERM, (uintptr)from, (uintptr)num, (uintptr)on);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Iopl(nint level) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_IOPL, (uintptr)level, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Lchown(@string path, nint uid, nint gid) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_LCHOWN, (uintptr)_p0, (uintptr)uid, (uintptr)gid);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Listen(nint s, nint n) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_LISTEN, (uintptr)s, (uintptr)n, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fd, error err) MemfdSecret(nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_MEMFD_SECRET, (uintptr)flags, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ Pause() {
    error err = default;

    var (_, _, e1) = Syscall(SYS_PAUSE, 0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) pread(nint fd, slice<byte> p, int64 offset) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PREAD64, (uintptr)fd, (uintptr)_p0, (uintptr)len(p), (uintptr)offset, 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) pwrite(nint fd, slice<byte> p, int64 offset) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PWRITE64, (uintptr)fd, (uintptr)_p0, (uintptr)len(p), (uintptr)offset, 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ Renameat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_RENAMEAT, (uintptr)olddirfd, (uintptr)_p0, (uintptr)newdirfd, (uintptr)_p1, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int64 off, error err) Seek(nint fd, int64 offset, nint whence) {
    int64 off = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_LSEEK, (uintptr)fd, (uintptr)offset, (uintptr)whence);
    off = (int64)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (off, err);
}

internal static (nint written, error err) sendfile(nint outfd, nint infd, ж<int64> offset, nint count) {
    nint written = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_SENDFILE, (uintptr)outfd, (uintptr)infd, (uintptr)offset, (uintptr)count, 0, 0);
    written = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (written, err);
}

internal static (nint prev, error err) setfsgid(nint gid) {
    nint prev = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SETFSGID, (uintptr)gid, 0, 0);
    prev = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (prev, err);
}

internal static (nint prev, error err) setfsuid(nint uid) {
    nint prev = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SETFSUID, (uintptr)uid, 0, 0);
    prev = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (prev, err);
}

public static error /*err*/ Shutdown(nint fd, nint how) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SHUTDOWN, (uintptr)fd, (uintptr)how, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int64 n, error err) Splice(nint rfd, ж<int64> roff, nint wfd, ж<int64> woff, nint len, nint flags) {
    int64 n = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_SPLICE, (uintptr)rfd, (uintptr)roff, (uintptr)wfd, (uintptr)woff, (uintptr)len, (uintptr)flags);
    n = (int64)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ Statfs(@string path, ж<Statfs_t> buf) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_STATFS, (uintptr)_p0, (uintptr)buf, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SyncFileRange(nint fd, int64 off, int64 n, nint flags) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_SYNC_FILE_RANGE, (uintptr)fd, (uintptr)off, (uintptr)n, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Truncate(@string path, int64 length) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_TRUNCATE, (uintptr)_p0, (uintptr)length, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Ustat(nint dev, ж<Ustat_t> ubuf) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_USTAT, (uintptr)dev, (uintptr)ubuf, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint fd, error err) accept4(nint s, ж<RawSockaddrAny> rsa, ж<_Socklen> addrlen, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_ACCEPT4, (uintptr)s, (uintptr)rsa, (uintptr)addrlen, (uintptr)flags, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static error /*err*/ bind(nint s, @unsafe.Pointer addr, _Socklen addrlen) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_BIND, (uintptr)s, (uintptr)addr, (uintptr)addrlen);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ connect(nint s, @unsafe.Pointer addr, _Socklen addrlen) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CONNECT, (uintptr)s, (uintptr)addr, (uintptr)addrlen);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint nn, error err) getgroups(nint n, ж<_Gid_t> list) {
    nint nn = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_GETGROUPS, (uintptr)n, (uintptr)list, 0);
    nn = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (nn, err);
}

internal static error /*err*/ setgroups(nint n, ж<_Gid_t> list) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_SETGROUPS, (uintptr)n, (uintptr)list, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ getsockopt(nint s, nint level, nint name, @unsafe.Pointer val, ж<_Socklen> vallen) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_GETSOCKOPT, (uintptr)s, (uintptr)level, (uintptr)name, (uintptr)val, (uintptr)vallen, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setsockopt(nint s, nint level, nint name, @unsafe.Pointer val, uintptr vallen) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_SETSOCKOPT, (uintptr)s, (uintptr)level, (uintptr)name, (uintptr)val, (uintptr)vallen, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint fd, error err) socket(nint domain, nint typ, nint proto) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_SOCKET, (uintptr)domain, (uintptr)typ, (uintptr)proto);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static error /*err*/ socketpair(nint domain, nint typ, nint proto, /*[2]*/ ж<array<int32>> fd) {
    error err = default;

    var (_, _, e1) = RawSyscall6(SYS_SOCKETPAIR, (uintptr)domain, (uintptr)typ, (uintptr)proto, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ getpeername(nint fd, ж<RawSockaddrAny> rsa, ж<_Socklen> addrlen) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_GETPEERNAME, (uintptr)fd, (uintptr)rsa, (uintptr)addrlen);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ getsockname(nint fd, ж<RawSockaddrAny> rsa, ж<_Socklen> addrlen) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_GETSOCKNAME, (uintptr)fd, (uintptr)rsa, (uintptr)addrlen);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) recvfrom(nint fd, slice<byte> p, nint flags, ж<RawSockaddrAny> from, ж<_Socklen> fromlen) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_RECVFROM, (uintptr)fd, (uintptr)_p0, (uintptr)len(p), (uintptr)flags, (uintptr)from, (uintptr)fromlen);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static error /*err*/ sendto(nint s, slice<byte> buf, nint flags, @unsafe.Pointer to, _Socklen addrlen) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall6(SYS_SENDTO, (uintptr)s, (uintptr)_p0, (uintptr)len(buf), (uintptr)flags, (uintptr)to, (uintptr)addrlen);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) recvmsg(nint s, ж<Msghdr> msg, nint flags) {
    nint n = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_RECVMSG, (uintptr)s, (uintptr)msg, (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) sendmsg(nint s, ж<Msghdr> msg, nint flags) {
    nint n = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SENDMSG, (uintptr)s, (uintptr)msg, (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (uintptr xaddr, error err) mmap(uintptr addr, uintptr length, nint prot, nint flags, nint fd, int64 offset) {
    uintptr xaddr = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_MMAP, (uintptr)addr, (uintptr)length, (uintptr)prot, (uintptr)flags, (uintptr)fd, (uintptr)offset);
    xaddr = (uintptr)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (xaddr, err);
}

internal static error /*err*/ futimesat(nint dirfd, @string path, /*[2]*/ ж<array<Timeval>> times) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_FUTIMESAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)times);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Utime(@string path, ж<Utimbuf> buf) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_UTIME, (uintptr)_p0, (uintptr)buf, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ utimes(@string path, /*[2]*/ ж<array<Timeval>> times) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_UTIMES, (uintptr)_p0, (uintptr)times, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ kexecFileLoad(nint kernelFd, nint initrdFd, nint cmdlineLen, @string cmdline, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(cmdline);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_KEXEC_FILE_LOAD, (uintptr)kernelFd, (uintptr)initrdFd, (uintptr)cmdlineLen, (uintptr)_p0, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nuint remaining, error err) Alarm(nuint seconds) {
    nuint remaining = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_ALARM, (uintptr)seconds, 0, 0);
    remaining = (nuint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (remaining, err);
}

} // end unix_package
