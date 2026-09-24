// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Linux system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.
namespace go;

using itoa = @internal.itoa_package;
using runtimesyscall = @internal.runtime.syscall_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using @internal;
using go.sync;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class syscall_package {

// Pull in entersyscall/exitsyscall for Syscall/Syscall6.
//
// Note that this can't be a push linkname because the runtime already has a
// nameless linkname to export to assembly here and in x/sys. Additionally,
// entersyscall fetches the caller PC and SP and thus can't have a wrapper
// inbetween.

//go:linkname runtime_entersyscall runtime.entersyscall
internal static partial void runtime_entersyscall();

//go:linkname runtime_exitsyscall runtime.exitsyscall
internal static partial void runtime_exitsyscall();

// N.B. For the Syscall functions below:
//
// //go:uintptrkeepalive because the uintptr argument may be converted pointers
// that need to be kept alive in the caller.
//
// //go:nosplit because stack copying does not account for uintptrkeepalive, so
// the stack must not grow. Stack copying cannot blindly assume that all
// uintptr arguments are pointers, because some values may look like pointers,
// but not really be pointers, and adjusting their value would break the call.
//
// //go:norace, on RawSyscall, to avoid race instrumentation if RawSyscall is
// called after fork, or from a signal handler.
//
// //go:linkname to ensure ABI wrappers are generated for external callers
// (notably x/sys/unix assembly).

//go:uintptrkeepalive
//go:nosplit
//go:norace
//go:linkname RawSyscall
public static (uintptr r1, uintptr r2, Errno err) RawSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    return RawSyscall6(trap, a1, a2, a3, 0, 0, 0);
}

//go:uintptrkeepalive
//go:nosplit
//go:norace
//go:linkname RawSyscall6
public static (uintptr r1, uintptr r2, Errno err) RawSyscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    Errno err = default!;

    uintptr errno = default!;
    (r1, r2, errno) = runtimesyscall.Syscall6(trap, a1, a2, a3, a4, a5, a6);
    err = ((Errno)errno);
    return (r1, r2, err);
}

//go:uintptrkeepalive
//go:nosplit
//go:linkname Syscall
public static (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    Errno err = default!;

    runtime_entersyscall();
    // N.B. Calling RawSyscall here is unsafe with atomic coverage
    // instrumentation and race mode.
    //
    // Coverage instrumentation will add a sync/atomic call to RawSyscall.
    // Race mode will add race instrumentation to sync/atomic. Race
    // instrumentation requires a P, which we no longer have.
    //
    // RawSyscall6 is fine because it is implemented in assembly and thus
    // has no coverage instrumentation.
    //
    // This is typically not a problem in the runtime because cmd/go avoids
    // adding coverage instrumentation to the runtime in race mode.
    (r1, r2, err) = RawSyscall6(trap, a1, a2, a3, 0, 0, 0);
    runtime_exitsyscall();
    return (r1, r2, err);
}

//go:uintptrkeepalive
//go:nosplit
//go:linkname Syscall6
public static (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    Errno err = default!;

    runtime_entersyscall();
    (r1, r2, err) = RawSyscall6(trap, a1, a2, a3, a4, a5, a6);
    runtime_exitsyscall();
    return (r1, r2, err);
}

internal static partial (uintptr r1, uintptr r2) rawSyscallNoError(uintptr trap, uintptr a1, uintptr a2, uintptr a3);

internal static partial (uintptr r1, Errno err) rawVforkSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3);

/*
 * Wrapped
 */
public static error /*err*/ Access(@string path, uint32 mode) {
    return Faccessat(_AT_FDCWD, path, mode, 0);
}

public static error /*err*/ Chmod(@string path, uint32 mode) {
    return Fchmodat(_AT_FDCWD, path, mode, 0);
}

public static error /*err*/ Chown(@string path, nint uid, nint gid) {
    return Fchownat(_AT_FDCWD, path, uid, gid, 0);
}

public static (nint fd, error err) Creat(@string path, uint32 mode) {
    return Open(path, (nint)((nint)(UntypedInt)(O_CREAT | O_WRONLY) | (nint)O_TRUNC), mode);
}

public static (nint fd, error err) EpollCreate(nint size) {
    if (size <= 0) {
        return (-1, EINVAL);
    }
    return EpollCreate1(0);
}

internal static bool isGroupMember(nint gid) {
    var (groups, err) = Getgroups();
    if (err != default!) {
        return false;
    }
    foreach (var (_, g) in groups) {
        if (g == gid) {
            return true;
        }
    }
    return false;
}

internal static bool isCapDacOverrideSet() {
    uintptr _CAP_DAC_OVERRIDE = 1;
    ref var c = ref heap(new caps(), out var Ꮡc);
    c.hdr.version = _LINUX_CAPABILITY_VERSION_3;
    var ᴋ0 = Ꮡc.of(caps.Ꮡhdr);
    var ᴋ1 = Ꮡc.at(caps.Ꮡdata, 0);
        var (_, _, err) = RawSyscall(SYS_CAPGET, (uintptr)ᴋ0, (uintptr)ᴋ1, 0);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    return err == 0 && (uint32)(c.data[0].effective & capToMask(_CAP_DAC_OVERRIDE)) != 0;
}

//sys	faccessat(dirfd int, path string, mode uint32) (err error)
//sys	faccessat2(dirfd int, path string, mode uint32, flags int) (err error) = _SYS_faccessat2
public static error /*err*/ Faccessat(nint dirfd, @string path, uint32 mode, nint flags) {
    if (flags == 0) {
        return faccessat(dirfd, path, mode);
    }
    // Attempt to use the newer faccessat2, which supports flags directly,
    // falling back if it doesn't exist.
    //
    // Don't attempt on Android, which does not allow faccessat2 through
    // its seccomp policy [1] on any version of Android as of 2022-12-20.
    //
    // [1] https://cs.android.com/android/platform/superproject/+/master:bionic/libc/SECCOMP_BLOCKLIST_APP.TXT;l=4;drc=dbb8670dfdcc677f7e3b9262e93800fa14c4e417
    if (runtime.GOOS != "android"u8) {
        {
            var errΔ1 = faccessat2(dirfd, path, mode, flags); if (!AreEqual(errΔ1, ENOSYS) && !AreEqual(errΔ1, EPERM)) {
                return errΔ1;
            }
        }
    }
    // The Linux kernel faccessat system call does not take any flags.
    // The glibc faccessat implements the flags itself; see
    // https://sourceware.org/git/?p=glibc.git;a=blob;f=sysdeps/unix/sysv/linux/faccessat.c;hb=HEAD
    // Because people naturally expect syscall.Faccessat to act
    // like C faccessat, we do the same.
    if ((nint)(flags & (nint)~((UntypedInt)(_AT_SYMLINK_NOFOLLOW | _AT_EACCESS))) != 0) {
        return EINVAL;
    }
    ref var st = ref heap(new Stat_t(), out var Ꮡst);
    {
        var errΔ2 = fstatat(dirfd, path, Ꮡst, (nint)(flags & (nint)_AT_SYMLINK_NOFOLLOW)); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    mode &= (uint32)(7);
    if (mode == 0) {
        return default!;
    }
    // Fallback to checking permission bits.
    nint uid = default!;
    if ((nint)(flags & (nint)_AT_EACCESS) != 0){
        uid = Geteuid();
        if (uid != 0 && isCapDacOverrideSet()) {
            // If CAP_DAC_OVERRIDE is set, file access check is
            // done by the kernel in the same way as for root
            // (see generic_permission() in the Linux sources).
            uid = 0;
        }
    } else {
        uid = Getuid();
    }
    if (uid == 0) {
        if ((uint32)(mode & 1) == 0) {
            // Root can read and write any file.
            return default!;
        }
        if ((uint32)(st.Mode & 73) != 0) {
            // Root can execute any file that anybody can execute.
            return default!;
        }
        return EACCES;
    }
    uint32 fmode = default!;
    if ((uint32)uid == st.Uid){
        fmode = (uint32)(((st.Mode >> (int)(6))) & 7);
    } else {
        nint gid = default!;
        if ((nint)(flags & (nint)_AT_EACCESS) != 0){
            gid = Getegid();
        } else {
            gid = Getgid();
        }
        if ((uint32)gid == st.Gid || isGroupMember((nint)st.Gid)){
            fmode = (uint32)(((st.Mode >> (int)(3))) & 7);
        } else {
            fmode = (uint32)(st.Mode & 7);
        }
    }
    if ((uint32)(fmode & mode) == mode) {
        return default!;
    }
    return EACCES;
}

//sys	fchmodat(dirfd int, path string, mode uint32) (err error)
//sys	fchmodat2(dirfd int, path string, mode uint32, flags int) (err error) = _SYS_fchmodat2
public static error Fchmodat(nint dirfd, @string path, uint32 mode, nint flags) {
    // Linux fchmodat doesn't support the flags parameter, but fchmodat2 does.
    // Try fchmodat2 if flags are specified.
    if (flags != 0) {
        var err = fchmodat2(dirfd, path, mode, flags);
        if (AreEqual(err, ENOSYS)) {
            // fchmodat2 isn't available. If the flags are known to be valid,
            // return EOPNOTSUPP to indicate that fchmodat doesn't support them.
            if ((nint)(flags & ~(nint)((nint)((nint)_AT_SYMLINK_NOFOLLOW | (nint)_AT_EMPTY_PATH))) != 0){
                return EINVAL;
            } else 
            if ((nint)(flags & (nint)((nint)((nint)_AT_SYMLINK_NOFOLLOW | (nint)_AT_EMPTY_PATH))) != 0) {
                return EOPNOTSUPP;
            }
        }
        return err;
    }
    return fchmodat(dirfd, path, mode);
}

//sys	linkat(olddirfd int, oldpath string, newdirfd int, newpath string, flags int) (err error)
public static error /*err*/ Link(@string oldpath, @string newpath) {
    return linkat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath, 0);
}

public static error /*err*/ Mkdir(@string path, uint32 mode) {
    return Mkdirat(_AT_FDCWD, path, mode);
}

public static error /*err*/ Mknod(@string path, uint32 mode, nint dev) {
    return Mknodat(_AT_FDCWD, path, mode, dev);
}

public static (nint fd, error err) Open(@string path, nint mode, uint32 perm) {
    return openat(_AT_FDCWD, path, (nint)(mode | (nint)O_LARGEFILE), perm);
}

//sys	openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)
public static (nint fd, error err) Openat(nint dirfd, @string path, nint flags, uint32 mode) {
    return openat(dirfd, path, (nint)(flags | (nint)O_LARGEFILE), mode);
}

public static error Pipe(slice<nint> p) {
    return Pipe2(p, 0);
}

//sysnb pipe2(p *[2]_C_int, flags int) (err error)
public static error Pipe2(slice<nint> p, nint flags) {
    if (len(p) != 2) {
        return EINVAL;
    }
    ref var pp = ref heap(new array<_C_int>(2), out var Ꮡpp);
    var err = pipe2(Ꮡpp, flags);
    if (err == default!) {
        p[0] = (nint)(int32)pp[0];
        p[1] = (nint)(int32)pp[1];
    }
    return err;
}

//sys	readlinkat(dirfd int, path string, buf []byte) (n int, err error)
public static (nint n, error err) Readlink(@string path, slice<byte> buf) {
    return readlinkat(_AT_FDCWD, path, buf);
}

public static error /*err*/ Rename(@string oldpath, @string newpath) {
    return Renameat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath);
}

public static error Rmdir(@string path) {
    return unlinkat(_AT_FDCWD, path, _AT_REMOVEDIR);
}

//sys	symlinkat(oldpath string, newdirfd int, newpath string) (err error)
public static error /*err*/ Symlink(@string oldpath, @string newpath) {
    return symlinkat(oldpath, _AT_FDCWD, newpath);
}

public static error Unlink(@string path) {
    return unlinkat(_AT_FDCWD, path, 0);
}

//sys	unlinkat(dirfd int, path string, flags int) (err error)
public static error Unlinkat(nint dirfd, @string path) {
    return unlinkat(dirfd, path, 0);
}

public static error /*err*/ Utimes(@string path, slice<Timeval> tv) {
    if (len(tv) != 2) {
        return EINVAL;
    }
    return utimes(path, array<Timeval>.AliasPointer(Ꮡ(tv, 0), 2));
}

//sys	utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)
public static error /*err*/ UtimesNano(@string path, slice<Timespec> ts) {
    if (len(ts) != 2) {
        return EINVAL;
    }
    return utimensat(_AT_FDCWD, path, array<Timespec>.AliasPointer(Ꮡ(ts, 0), 2), 0);
}

public static error /*err*/ Futimesat(nint dirfd, @string path, slice<Timeval> tv) {
    if (len(tv) != 2) {
        return EINVAL;
    }
    return futimesat(dirfd, path, array<Timeval>.AliasPointer(Ꮡ(tv, 0), 2));
}

public static error /*err*/ Futimes(nint fd, slice<Timeval> tv) {
    // Believe it or not, this is the best we can do on Linux
    // (and is what glibc does).
    return Utimes("/proc/self/fd/"u8 + itoa.Itoa(fd), tv);
}

public const bool ImplementsGetwd = true;

//sys	Getcwd(buf []byte) (n int, err error)
public static (@string wd, error err) Getwd() {
    error err = default!;

    array<byte> buf = new(4096); /* PathMax */
    (var n, err) = Getcwd(buf[0..]);
    if (err != default!) {
        return ("", err);
    }
    // Getcwd returns the number of bytes written to buf, including the NUL.
    if (n < 1 || n > len(buf) || buf[n - 1] != 0) {
        return ("", EINVAL);
    }
    // In some cases, Linux can return a path that starts with the
    // "(unreachable)" prefix, which can potentially be a valid relative
    // path. To work around that, return ENOENT if path is not absolute.
    if (buf[0] != (rune)'/') {
        return ("", ENOENT);
    }
    return (((@string)(buf[0..(int)(n - 1)])), default!);
}

public static (slice<nint> gids, error err) Getgroups() {
    slice<nint> gids = default!;
    error err = default!;

    (var n, err) = getgroups(0, nil);
    if (err != default!) {
        return (default!, err);
    }
    if (n == 0) {
        return (default!, default!);
    }
    // Sanity check group count. Max is 1<<16 on Linux.
    if (n < 0 || n > (1 << (int)(20))) {
        return (default!, EINVAL);
    }
    var a = new slice<_Gid_t>(n);
    (n, err) = getgroups(n, Ꮡ(a, 0));
    if (err != default!) {
        return (default!, err);
    }
    gids = new slice<nint>(n);
    foreach (var (i, v) in a[0..(int)(n)]) {
        gids[i] = (nint)(uint32)v;
    }
    return (gids, err);
}

internal static @unsafe.Pointer cgo_libc_setgroups; // non-nil if cgo linked.

// go2cs generated this placeholder — func Setgroups is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoType("num:uint32")] partial struct WaitStatus;

// Wait status is 7 bits at bottom, either 0 (exited),
// 0x7F (stopped), or a signal number that caused an exit.
// The 0x80 bit is whether there was a core dump.
// An extra number (exit code, signal causing a stop)
// is in the high bits. At least that's the idea.
// There are various irregularities. For example, the
// "continued" status is 0xFFFF, distinguishing itself
// from stopped via the core dump bit.
internal static UntypedInt mask => 0x7F;
internal static UntypedInt core => 0x80;
internal static UntypedInt exited => 0x00;
internal static UntypedInt stopped => 0x7F;
internal static UntypedInt shift => 8;

public static bool Exited(this WaitStatus w) {
    return (WaitStatus)(w & (uint32)mask) == exited;
}

public static bool Signaled(this WaitStatus w) {
    return (WaitStatus)(w & (uint32)mask) != stopped && (WaitStatus)(w & (uint32)mask) != exited;
}

public static bool Stopped(this WaitStatus w) {
    return (WaitStatus)(w & 0xFF) == stopped;
}

public static bool Continued(this WaitStatus w) {
    return w == 0xFFFF;
}

public static bool CoreDump(this WaitStatus w) {
    return w.Signaled() && (WaitStatus)(w & (uint32)core) != 0;
}

public static nint ExitStatus(this WaitStatus w) {
    if (!w.Exited()) {
        return -1;
    }
    return (nint)((nint)(uint32)((w >> (int)(shift))) & 0xFF);
}

public static ΔSignal Signal(this WaitStatus w) {
    if (!w.Signaled()) {
        return -1;
    }
    return ((ΔSignal)(nint)((uint32)((WaitStatus)(w & (uint32)mask))));
}

public static ΔSignal StopSignal(this WaitStatus w) {
    if (!w.Stopped()) {
        return -1;
    }
    return (ΔSignal)(((ΔSignal)(nint)((uint32)((w >> (int)(shift))))) & 0xFF);
}

public static nint TrapCause(this WaitStatus w) {
    if (w.StopSignal() != SIGTRAP) {
        return -1;
    }
    return ((nint)(uint32)((w >> (int)(shift))) >> (int)(8));
}

//sys	wait4(pid int, wstatus *_C_int, options int, rusage *Rusage) (wpid int, err error)
public static (nint wpid, error err) Wait4(nint pid, ж<WaitStatus> Ꮡwstatus, nint options, ж<Rusage> Ꮡrusage) {
    nint wpid = default!;
    error err = default!;

    ref var wstatus = ref Ꮡwstatus.DerefOrNull();
    ref var status = ref heap(new _C_int(), out var Ꮡstatus);
    (wpid, err) = wait4(pid, Ꮡstatus, options, Ꮡrusage);
    if (Ꮡwstatus != nil) {
        wstatus = ((WaitStatus)(uint32)(int32)status);
    }
    return (wpid, err);
}

public static error /*err*/ Mkfifo(@string path, uint32 mode) {
    return Mknod(path, (uint32)(mode | (uint32)S_IFIFO), 0);
}

// go2cs generated this placeholder — func sockaddr is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sockaddr is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrUnix> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    @string name = sa.Name;
    nint n = len(name);
    if (n > len(sa.raw.Path)) {
        return (default!, 0, EINVAL);
    }
    if (n == len(sa.raw.Path) && name[0] != (rune)'@') {
        return (default!, 0, EINVAL);
    }
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = (int8)name[i];
    }
    // length is family (uint16), name, NUL.
    var sl = ((_Socklen)2);
    if (n > 0) {
        sl += ((_Socklen)(uint32)n) + 1;
    }
    if (sa.raw.Path[0] == (rune)'@' || (sa.raw.Path[0] == 0 && sl > 3)) {
        // Check sl > 3 so we don't change unnamed socket behavior.
        sa.raw.Path[0] = 0;
        // Don't count trailing NUL for abstract address.
        sl--;
    }
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.of(SockaddrUnix.Ꮡraw)), sl, default!);
}

[GoType] partial struct SockaddrLinklayer {
    public uint16 Protocol;
    public nint Ifindex;
    public uint16 Hatype;
    public uint8 Pkttype;
    public uint8 Halen;
    public array<byte> Addr = new(8);
    internal RawSockaddrLinklayer raw;
}

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrLinklayer> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Ifindex < 0 || sa.Ifindex > 0x7fffffff) {
        return (default!, 0, EINVAL);
    }
    sa.raw.Family = AF_PACKET;
    sa.raw.Protocol = sa.Protocol;
    sa.raw.Ifindex = (int32)sa.Ifindex;
    sa.raw.Hatype = sa.Hatype;
    sa.raw.Pkttype = sa.Pkttype;
    sa.raw.Halen = sa.Halen;
    sa.raw.Addr = sa.Addr.Clone();
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.of(SockaddrLinklayer.Ꮡraw)), SizeofSockaddrLinklayer, default!);
}

[GoType] partial struct SockaddrNetlink {
    public uint16 Family;
    public uint16 Pad;
    public uint32 Pid;
    public uint32 Groups;
    internal RawSockaddrNetlink raw;
}

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrNetlink> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_NETLINK;
    sa.raw.Pad = sa.Pad;
    sa.raw.Pid = sa.Pid;
    sa.raw.Groups = sa.Groups;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.of(SockaddrNetlink.Ꮡraw)), SizeofSockaddrNetlink, default!);
}

// go2cs generated this placeholder — func anyToSockaddr is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static (nint nfd, Sockaddr sa, error err) Accept(nint fd) {
    return Accept4(fd, 0);
}

// go2cs generated this placeholder — func Accept4 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Getsockname is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static (array<byte> value, error err) GetsockoptInet4Addr(nint fd, nint level, nint opt) {
    ref var value = ref heap(new array<byte>(4), out var Ꮡvalue);
    error err = default!;

    ref var vallen = ref heap<_Socklen>(out var Ꮡvallen);
    vallen = ((_Socklen)4);
    err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)), Ꮡvallen);
    return (value.Clone(), err);
}

// go2cs generated this placeholder — func GetsockoptIPMreq is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func GetsockoptIPMreqn is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func GetsockoptIPv6Mreq is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static (ж<IPv6MTUInfo>, error) GetsockoptIPv6MTUInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new IPv6MTUInfo(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>(out var Ꮡvallen);
    vallen = ((_Socklen)SizeofIPv6MTUInfo);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

// go2cs generated this placeholder — func GetsockoptICMPv6Filter is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static (ж<Ucred>, error) GetsockoptUcred(nint fd, nint level, nint opt) {
    ref var value = ref heap(new Ucred(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>(out var Ꮡvallen);
    vallen = ((_Socklen)SizeofUcred);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

// go2cs generated this placeholder — func SetsockoptIPMreqn is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func recvmsgRaw is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (nint n, error err) sendmsgN(nint fd, slice<byte> p, slice<byte> oob, @unsafe.Pointer ptr, _Socklen salen, nint flags) {
    nint n = default!;
    error err = default!;

    ref var msg = ref heap(new Msghdr(), out var Ꮡmsg);
    msg.Name = (ж<byte>)(uintptr)(ptr);
    msg.Namelen = (uint32)salen;
    ref var iov = ref heap(new Iovec(), out var Ꮡiov);
    if (len(p) > 0) {
        iov.Base = Ꮡ(p, 0);
        iov.SetLen(len(p));
    }
    ref var dummy = ref heap(new byte(), out var Ꮡdummy);
    if (len(oob) > 0) {
        if (len(p) == 0) {
            nint sockType = default!;
            (sockType, err) = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
            if (err != default!) {
                return (0, err);
            }
            // send at least one normal byte
            if (sockType != SOCK_DGRAM) {
                iov.Base = Ꮡdummy;
                iov.SetLen(1);
            }
        }
        msg.Control = Ꮡ(oob, 0);
        msg.SetControllen(len(oob));
    }
    msg.Iov = Ꮡiov;
    msg.Iovlen = 1;
    {
        (n, err) = sendmsg(fd, Ꮡmsg, flags); if (err != default!) {
            return (0, err);
        }
    }
    if (len(oob) > 0 && len(p) == 0) {
        n = 0;
    }
    return (n, default!);
}

// BindToDevice binds the socket associated with fd to device.
public static error /*err*/ BindToDevice(nint fd, @string device) {
    return SetsockoptString(fd, SOL_SOCKET, SO_BINDTODEVICE, device);
}

//sys	ptrace(request int, pid int, addr uintptr, data uintptr) (err error)
//sys	ptracePtr(request int, pid int, addr uintptr, data unsafe.Pointer) (err error) = SYS_PTRACE
internal static (nint count, error err) ptracePeek(nint req, nint pid, uintptr addr, slice<byte> @out) {
    error err = default!;

    // The peek requests are machine-size oriented, so we wrap it
    // to retrieve arbitrary-length data.
    // The ptrace syscall differs from glibc's ptrace.
    // Peeks returns the word in *data, not as the return value.
    ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
    // Leading edge. PEEKTEXT/PEEKDATA don't require aligned
    // access (PEEKUSER warns that it might), but if we don't
    // align our reads, we might straddle an unmapped page
    // boundary and not get the bytes leading up to the page
    // boundary.
    nint n = 0;
    if (addr % (uintptr)sizeofPtr != 0) {
        err = ptracePtr(req, pid, addr - addr % (uintptr)sizeofPtr, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != default!) {
            return (0, err);
        }
        n += copy(@out, buf[(int)(addr % (uintptr)sizeofPtr)..]);
        @out = @out[(int)(n)..];
    }
    // Remainder.
    while (len(@out) > 0) {
        // We use an internal buffer to guarantee alignment.
        // It's not documented if this is necessary, but we're paranoid.
        err = ptracePtr(req, pid, addr + (uintptr)n, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != default!) {
            return (n, err);
        }
        nint copied = copy(@out, buf[0..]);
        n += copied;
        @out = @out[(int)(copied)..];
    }
    return (n, default!);
}

public static (nint count, error err) PtracePeekText(nint pid, uintptr addr, slice<byte> @out) {
    return ptracePeek(PTRACE_PEEKTEXT, pid, addr, @out);
}

public static (nint count, error err) PtracePeekData(nint pid, uintptr addr, slice<byte> @out) {
    return ptracePeek(PTRACE_PEEKDATA, pid, addr, @out);
}

internal static (nint count, error err) ptracePoke(nint pokeReq, nint peekReq, nint pid, uintptr addr, slice<byte> data) {
    error err = default!;

    // As for ptracePeek, we need to align our accesses to deal
    // with the possibility of straddling an invalid page.
    // Leading edge.
    nint n = 0;
    if (addr % (uintptr)sizeofPtr != 0) {
        ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
        err = ptracePtr(peekReq, pid, addr - addr % (uintptr)sizeofPtr, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != default!) {
            return (0, err);
        }
        n += copy(buf[(int)(addr % (uintptr)sizeofPtr)..], data);
        var word = (Ꮡbuf.at<byte>(0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr - addr % (uintptr)sizeofPtr, word);
        if (err != default!) {
            return (0, err);
        }
        data = data[(int)(n)..];
    }
    // Interior.
    while (len(data) > sizeofPtr) {
        var word = (Ꮡ(data, 0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr + (uintptr)n, word);
        if (err != default!) {
            return (n, err);
        }
        n += sizeofPtr;
        data = data[(int)(sizeofPtr)..];
    }
    // Trailing edge.
    if (len(data) > 0) {
        ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
        err = ptracePtr(peekReq, pid, addr + (uintptr)n, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != default!) {
            return (n, err);
        }
        copy(buf[0..], data);
        var word = (Ꮡbuf.at<byte>(0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr + (uintptr)n, word);
        if (err != default!) {
            return (n, err);
        }
        n += len(data);
    }
    return (n, default!);
}

public static (nint count, error err) PtracePokeText(nint pid, uintptr addr, slice<byte> data) {
    return ptracePoke(PTRACE_POKETEXT, PTRACE_PEEKTEXT, pid, addr, data);
}

public static (nint count, error err) PtracePokeData(nint pid, uintptr addr, slice<byte> data) {
    return ptracePoke(PTRACE_POKEDATA, PTRACE_PEEKDATA, pid, addr, data);
}

internal static UntypedInt _NT_PRSTATUS => 1;

public static error /*err*/ PtraceGetRegs(nint pid, ж<PtraceRegs> Ꮡregsout) {
    ref var regsout = ref Ꮡregsout.DerefOrNull();

    ref var iov = ref heap(new Iovec(), out var Ꮡiov);
    iov.Base = Ꮡregsout.Reinterpret<PtraceRegs, byte>();
    iov.SetLen((nint)/* unsafe.Sizeof(*regsout) */ (uintptr)216);
    return ptracePtr(PTRACE_GETREGSET, pid, (uintptr)_NT_PRSTATUS, @unsafe.Pointer.FromPinnedBox(Ꮡiov));
}

public static error /*err*/ PtraceSetRegs(nint pid, ж<PtraceRegs> Ꮡregs) {
    ref var regs = ref Ꮡregs.DerefOrNull();

    ref var iov = ref heap(new Iovec(), out var Ꮡiov);
    iov.Base = Ꮡregs.Reinterpret<PtraceRegs, byte>();
    iov.SetLen((nint)/* unsafe.Sizeof(*regs) */ (uintptr)216);
    return ptracePtr(PTRACE_SETREGSET, pid, (uintptr)_NT_PRSTATUS, @unsafe.Pointer.FromPinnedBox(Ꮡiov));
}

public static error /*err*/ PtraceSetOptions(nint pid, nint options) {
    return ptrace(PTRACE_SETOPTIONS, pid, 0, (uintptr)options);
}

public static (nuint msg, error err) PtraceGetEventMsg(nint pid) {
    nuint msg = default!;
    error err = default!;

    ref var data = ref heap(new _C_long(), out var Ꮡdata);
    err = ptracePtr(PTRACE_GETEVENTMSG, pid, 0, @unsafe.Pointer.FromPinnedBox(Ꮡdata));
    msg = (nuint)(int64)data;
    return (msg, err);
}

public static error /*err*/ PtraceCont(nint pid, nint signal) {
    return ptrace(PTRACE_CONT, pid, 0, (uintptr)signal);
}

public static error /*err*/ PtraceSyscall(nint pid, nint signal) {
    return ptrace(PTRACE_SYSCALL, pid, 0, (uintptr)signal);
}

public static error /*err*/ PtraceSingleStep(nint pid) {
    return ptrace(PTRACE_SINGLESTEP, pid, 0, 0);
}

public static error /*err*/ PtraceAttach(nint pid) {
    return ptrace(PTRACE_ATTACH, pid, 0, 0);
}

public static error /*err*/ PtraceDetach(nint pid) {
    return ptrace(PTRACE_DETACH, pid, 0, 0);
}

//sys	reboot(magic1 uint, magic2 uint, cmd int, arg string) (err error)
public static error /*err*/ Reboot(nint cmd) {
    return reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, cmd, ""u8);
}

public static (nint n, error err) ReadDirent(nint fd, slice<byte> buf) {
    return Getdents(fd, buf);
}

internal static (uint64, bool) direntIno(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Ino) */ (uintptr)0, /* unsafe.Sizeof(Dirent{}.Ino) */ (uintptr)8);
}

internal static (uint64, bool) direntReclen(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Reclen) */ (uintptr)16, /* unsafe.Sizeof(Dirent{}.Reclen) */ (uintptr)2);
}

internal static (uint64, bool) direntNamlen(slice<byte> buf) {
    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - (uint64)/* unsafe.Offsetof(Dirent{}.Name) */ (uintptr)19, true);
}

//sys	mount(source string, target string, fstype string, flags uintptr, data *byte) (err error)
public static error /*err*/ Mount(@string source, @string target, @string fstype, uintptr flags, @string data) {
    error err = default!;

    // Certain file systems get rather angry and EINVAL if you give
    // them an empty string of data, rather than NULL.
    if (data == ""u8) {
        return mount(source, target, fstype, flags, nil);
    }
    (var datap, err) = BytePtrFromString(data);
    if (err != default!) {
        return err;
    }
    return mount(source, target, fstype, flags, datap);
}

// Sendto
// Recvfrom
// Socketpair
/*
 * Direct access
 */
//sys	Acct(path string) (err error)
//sys	Adjtimex(buf *Timex) (state int, err error)
//sys	Chdir(path string) (err error)
//sys	Chroot(path string) (err error)
//sys	Close(fd int) (err error)
//sys	Dup(oldfd int) (fd int, err error)
//sys	Dup3(oldfd int, newfd int, flags int) (err error)
//sysnb	EpollCreate1(flag int) (fd int, err error)
//sysnb	EpollCtl(epfd int, op int, fd int, event *EpollEvent) (err error)
//sys	Fallocate(fd int, mode uint32, off int64, len int64) (err error)
//sys	Fchdir(fd int) (err error)
//sys	Fchmod(fd int, mode uint32) (err error)
//sys	Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys	fcntl(fd int, cmd int, arg int) (val int, err error)
//sys	Fdatasync(fd int) (err error)
//sys	Flock(fd int, how int) (err error)
//sys	Fsync(fd int) (err error)
//sys	Getdents(fd int, buf []byte) (n int, err error) = SYS_GETDENTS64
//sysnb	Getpgid(pid int) (pgid int, err error)
public static nint /*pid*/ Getpgrp() {
    nint pid = default!;

    (pid, _) = Getpgid(0);
    return pid;
}

//sysnb	Getpid() (pid int)
//sysnb	Getppid() (ppid int)
//sys	Getpriority(which int, who int) (prio int, err error)
//sysnb	Getrusage(who int, rusage *Rusage) (err error)
//sysnb	Gettid() (tid int)
//sys	Getxattr(path string, attr string, dest []byte) (sz int, err error)
//sys	InotifyAddWatch(fd int, pathname string, mask uint32) (watchdesc int, err error)
//sysnb	InotifyInit1(flags int) (fd int, err error)
//sysnb	InotifyRmWatch(fd int, watchdesc uint32) (success int, err error)
//sysnb	Kill(pid int, sig Signal) (err error)
//sys	Klogctl(typ int, buf []byte) (n int, err error) = SYS_SYSLOG
//sys	Listxattr(path string, dest []byte) (sz int, err error)
//sys	Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys	Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
//sys	Nanosleep(time *Timespec, leftover *Timespec) (err error)
//sys	PivotRoot(newroot string, putold string) (err error) = SYS_PIVOT_ROOT
//sysnb prlimit1(pid int, resource int, newlimit *Rlimit, old *Rlimit) (err error) = SYS_PRLIMIT64
//sys	read(fd int, p []byte) (n int, err error)
//sys	Removexattr(path string, attr string) (err error)
//sys	Setdomainname(p []byte) (err error)
//sys	Sethostname(p []byte) (err error)
//sysnb	Setpgid(pid int, pgid int) (err error)
//sysnb	Setsid() (pid int, err error)
//sysnb	Settimeofday(tv *Timeval) (err error)

// Provided by runtime.syscall_runtime_doAllThreadsSyscall which stops the
// world and invokes the syscall on each OS thread. Once this function returns,
// all threads are in sync.
//
//go:uintptrescapes
internal static partial (uintptr r1, uintptr r2, uintptr err) runtime_doAllThreadsSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

// AllThreadsSyscall performs a syscall on each OS thread of the Go
// runtime. It first invokes the syscall on one thread. Should that
// invocation fail, it returns immediately with the error status.
// Otherwise, it invokes the syscall on all of the remaining threads
// in parallel. It will terminate the program if it observes any
// invoked syscall's return value differs from that of the first
// invocation.
//
// AllThreadsSyscall is intended for emulating simultaneous
// process-wide state changes that require consistently modifying
// per-thread state of the Go runtime.
//
// AllThreadsSyscall is unaware of any threads that are launched
// explicitly by cgo linked code, so the function always returns
// [ENOTSUP] in binaries that use cgo.
//
//go:uintptrescapes
public static (uintptr r1, uintptr r2, Errno err) AllThreadsSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;

    if (cgo_libc_setegid != nil) {
        return (minus1, minus1, ENOTSUP);
    }
    (r1, r2, var errno) = runtime_doAllThreadsSyscall(trap, a1, a2, a3, 0, 0, 0);
    return (r1, r2, ((Errno)errno));
}

// AllThreadsSyscall6 is like [AllThreadsSyscall], but extended to six
// arguments.
//
//go:uintptrescapes
public static (uintptr r1, uintptr r2, Errno err) AllThreadsSyscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;

    if (cgo_libc_setegid != nil) {
        return (minus1, minus1, ENOTSUP);
    }
    (r1, r2, var errno) = runtime_doAllThreadsSyscall(trap, a1, a2, a3, a4, a5, a6);
    return (r1, r2, ((Errno)errno));
}

// linked by runtime.cgocall.go
//
//go:uintptrescapes
internal static partial uintptr cgocaller(@unsafe.Pointer _Δp0, params ꓸꓸꓸuintptr ʗp);

internal static @unsafe.Pointer cgo_libc_setegid; // non-nil if cgo linked.

internal static uintptr minus1 => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);

public static error /*err*/ Setegid(nint egid) {
    error err = default!;

    if (cgo_libc_setegid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(SYS_SETRESGID, minus1, (uintptr)egid, minus1); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setegid, (uintptr)egid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_seteuid; // non-nil if cgo linked.

public static error /*err*/ Seteuid(nint euid) {
    error err = default!;

    if (cgo_libc_seteuid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(SYS_SETRESUID, minus1, (uintptr)euid, minus1); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_seteuid, (uintptr)euid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setgid; // non-nil if cgo linked.

public static error /*err*/ Setgid(nint gid) {
    error err = default!;

    if (cgo_libc_setgid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETGID, (uintptr)gid, 0, 0); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setgid, (uintptr)gid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setregid; // non-nil if cgo linked.

public static error /*err*/ Setregid(nint rgid, nint egid) {
    error err = default!;

    if (cgo_libc_setregid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETREGID, (uintptr)rgid, (uintptr)egid, 0); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setregid, (uintptr)rgid, (uintptr)egid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setresgid; // non-nil if cgo linked.

public static error /*err*/ Setresgid(nint rgid, nint egid, nint sgid) {
    error err = default!;

    if (cgo_libc_setresgid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETRESGID, (uintptr)rgid, (uintptr)egid, (uintptr)sgid); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setresgid, (uintptr)rgid, (uintptr)egid, (uintptr)sgid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setresuid; // non-nil if cgo linked.

public static error /*err*/ Setresuid(nint ruid, nint euid, nint suid) {
    error err = default!;

    if (cgo_libc_setresuid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETRESUID, (uintptr)ruid, (uintptr)euid, (uintptr)suid); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setresuid, (uintptr)ruid, (uintptr)euid, (uintptr)suid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setreuid; // non-nil if cgo linked.

public static error /*err*/ Setreuid(nint ruid, nint euid) {
    error err = default!;

    if (cgo_libc_setreuid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETREUID, (uintptr)ruid, (uintptr)euid, 0); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setreuid, (uintptr)ruid, (uintptr)euid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

internal static @unsafe.Pointer cgo_libc_setuid; // non-nil if cgo linked.

public static error /*err*/ Setuid(nint uid) {
    error err = default!;

    if (cgo_libc_setuid == nil){
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETUID, (uintptr)uid, 0, 0); if (e1 != 0) {
                err = errnoErr(e1);
            }
        }
    } else 
    {
        var ret = cgocaller(cgo_libc_setuid, (uintptr)uid); if (ret != 0) {
            err = errnoErr(((Errno)ret));
        }
    }
    return err;
}

//sys	Setpriority(which int, who int, prio int) (err error)
//sys	Setxattr(path string, attr string, data []byte, flags int) (err error)
//sys	Sync()
//sysnb	Sysinfo(info *Sysinfo_t) (err error)
//sys	Tee(rfd int, wfd int, len int, flags int) (n int64, err error)
//sysnb	Tgkill(tgid int, tid int, sig Signal) (err error)
//sysnb	Times(tms *Tms) (ticks uintptr, err error)
//sysnb	Umask(mask int) (oldmask int)
//sysnb	Uname(buf *Utsname) (err error)
//sys	Unmount(target string, flags int) (err error) = SYS_UMOUNT2
//sys	Unshare(flags int) (err error)
//sys	write(fd int, p []byte) (n int, err error)
//sys	exitThread(code int) (err error) = SYS_EXIT
//sys	readlen(fd int, p *byte, np int) (n int, err error) = SYS_READ
// mmap varies by architecture; see syscall_linux_*.go.
//sys	munmap(addr uintptr, length uintptr) (err error)
internal static ж<mmapper> mapper;
internal static void initᴛmapper() { mapper = Ꮡ(new mmapper(
    active: new map<ж<byte>, slice<byte>>(),
    mmap: mmap,
    munmap: munmap
)); }

public static (slice<byte> data, error err) Mmap(nint fd, int64 offset, nint length, nint prot, nint flags) {
    return mapper.Mmap(fd, offset, length, prot, flags);
}

public static error /*err*/ Munmap(slice<byte> b) {
    return mapper.Munmap(b);
}

//sys	Madvise(b []byte, advice int) (err error)
//sys	Mprotect(b []byte, prot int) (err error)
//sys	Mlock(b []byte) (err error)
//sys	Munlock(b []byte) (err error)
//sys	Mlockall(flags int) (err error)
//sys	Munlockall() (err error)
public static error /*err*/ Getrlimit(nint resource, ж<Rlimit> Ꮡrlim) {
    // prlimit1 is the same as prlimit when newlimit == nil
    return prlimit1(0, resource, nil, Ꮡrlim);
}

// setrlimit sets a resource limit.
// The Setrlimit function is in rlimit.go, and calls this one.
internal static error /*err*/ setrlimit(nint resource, ж<Rlimit> Ꮡrlim) {
    return prlimit1(0, resource, Ꮡrlim, nil);
}

// prlimit changes a resource limit. We use a single definition so that
// we can tell StartProcess to not restore the original NOFILE limit.
//
// golang.org/x/sys linknames prlimit.
// Do not remove or change the type signature.
//
//go:linkname prlimit
public static error /*err*/ prlimit(nint pid, nint resource, ж<Rlimit> Ꮡnewlimit, ж<Rlimit> Ꮡold) {
    error err = default!;

    err = prlimit1(pid, resource, Ꮡnewlimit, Ꮡold);
    if (err == default! && Ꮡnewlimit != nil && resource == RLIMIT_NOFILE && (pid == 0 || pid == Getpid())) {
        ᏑorigRlimitNofile.Store(nil);
    }
    return err;
}

} // end syscall_package
