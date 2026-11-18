// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static syscall_package;

partial class syscall_package {

internal static UntypedInt _SYS_setgroups => /* SYS_SETGROUPS */ 159;
internal static UntypedInt _SYS_clone3 => 435;
internal static UntypedInt _SYS_faccessat2 => 439;
internal static UntypedInt _SYS_fchmodat2 => 452;

//sys	EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error) = SYS_EPOLL_PWAIT
//sys	Fchown(fd int, uid int, gid int) (err error)
//sys	Fstat(fd int, stat *Stat_t) (err error)
//sys	fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error)
public static error Fstatat(nint fd, @string path, ж<Stat_t> stat, nint flags) {
    return fstatat(fd, path, stat, flags);
}

//sys	Fstatfs(fd int, buf *Statfs_t) (err error)
//sys	Ftruncate(fd int, length int64) (err error)
//sysnb	Getegid() (egid int)
//sysnb	Geteuid() (euid int)
//sysnb	Getgid() (gid int)
//sysnb	Getuid() (uid int)
//sys	Listen(s int, n int) (err error)
//sys	pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys	pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys	Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys	Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys	sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys	Setfsgid(gid int) (err error)
//sys	Setfsuid(uid int) (err error)
//sys	Shutdown(fd int, how int) (err error)
//sys	Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
public static error /*err*/ Stat(@string path, ж<Stat_t> stat) {
    return fstatat(_AT_FDCWD, path, stat, 0);
}

public static error /*err*/ Lchown(@string path, nint uid, nint gid) {
    return Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW);
}

public static error /*err*/ Lstat(@string path, ж<Stat_t> stat) {
    return fstatat(_AT_FDCWD, path, stat, _AT_SYMLINK_NOFOLLOW);
}

//sys	Statfs(path string, buf *Statfs_t) (err error)
//sys	SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE2
//sys	Truncate(path string, length int64) (err error)
//sys	accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error)
//sys	bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sys	connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sysnb	getgroups(n int, list *_Gid_t) (nn int, err error)
//sys	getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
//sys	setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
//sysnb	socket(domain int, typ int, proto int) (fd int, err error)
//sysnb	socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)
//sysnb	getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
//sysnb	getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
//sys	recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
//sys	sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
//sys	recvmsg(s int, msg *Msghdr, flags int) (n int, err error)
//sys	sendmsg(s int, msg *Msghdr, flags int) (n int, err error)
//sys	mmap(addr uintptr, length uintptr, prot int, flags int, fd int, offset int64) (xaddr uintptr, err error)
partial struct sigset_t {
    public array<uint64> X__val = new(16);
}

//sys	pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *sigset_t) (n int, err error) = SYS_PSELECT6
// go2cs generated this placeholder — func Select is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//sysnb	Gettimeofday(tv *Timeval) (err error)
internal static Timespec setTimespec(int64 sec, int64 nsec) {
    return new Timespec(Sec: sec, Nsec: nsec);
}

internal static Timeval setTimeval(int64 sec, int64 usec) {
    return new Timeval(Sec: sec, Usec: usec);
}

internal static error /*err*/ futimesat(nint dirfd, @string path, /*[2]*/ ж<array<Timeval>> Ꮡtv) {
    ref var tv = ref Ꮡtv.DerefOrNull();

    if (Ꮡtv == nil) {
        return utimensat(dirfd, path, ж<array<Timespec>>.NilBoxOfDims(2L), 0);
    }

    var ts = slice<Timespec>([
        NsecToTimespec(TimevalToNsec(tv[0])),
        NsecToTimespec(TimevalToNsec(tv[1]))
    ]);
    return utimensat(dirfd, path, array<Timespec>.AliasPointer(Ꮡ(ts, 0), 2), 0);
}

public static (Time_t, error) Time(ж<Time_t> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var tv = ref heap(new Timeval(), out var Ꮡtv);
    var err = Gettimeofday(Ꮡtv);
    if (err != nil) {
        return (0, err);
    }
    if (Ꮡt != nil) {
        t = (Time_t)tv.Sec;
    }
    return ((Time_t)tv.Sec, default);
}

public static error Utime(@string path, ж<Utimbuf> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    var tv = slice<Timeval>([
        new(Sec: buf.Actime),
        new(Sec: buf.Modtime)
    ]);
    return Utimes(path, tv);
}

internal static error /*err*/ utimes(@string path, /*[2]*/ ж<array<Timeval>> Ꮡtv) {
    ref var tv = ref Ꮡtv.DerefOrNull();

    if (Ꮡtv == nil) {
        return utimensat(_AT_FDCWD, path, ж<array<Timespec>>.NilBoxOfDims(2L), 0);
    }

    var ts = slice<Timespec>([
        NsecToTimespec(TimevalToNsec(tv[0])),
        NsecToTimespec(TimevalToNsec(tv[1]))
    ]);
    return utimensat(_AT_FDCWD, path, array<Timespec>.AliasPointer(Ꮡ(ts, 0), 2), 0);
}

} partial class syscallᴛPtraceRegs {

public static uint64 PC(this ref PtraceRegs r) { return r.Pc; }

public static void SetPC(this ref PtraceRegs r, uint64 pc) { r.Pc = pc; }

} partial class syscallᴛIovec {

public static void SetLen(this ref Iovec iov, nint length) {
    iov.Len = (uint64)length;
}

} partial class syscallᴛMsghdr {

public static void SetControllen(this ref Msghdr msghdr, nint length) {
    msghdr.Controllen = (uint64)length;
}

} partial class syscallᴛCmsghdr {

public static void SetLen(this ref Cmsghdr cmsg, nint length) {
    cmsg.Len = (uint64)length;
}

} partial class syscall_package {

public static (nint fd, error err) InotifyInit() {
    return InotifyInit1(0);
}

//sys	ppoll(fds *pollFd, nfds int, timeout *Timespec, sigmask *sigset_t) (n int, err error)
public static error Pause() {
    var (_, err) = ppoll(nil, 0, nil, nil);
    return err;
}

} // end syscall_package
