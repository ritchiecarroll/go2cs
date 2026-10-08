// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class syscall_package {

internal static UntypedInt _SYS_setgroups => /* SYS_SETGROUPS */ 116;
internal static UntypedInt _SYS_clone3 => 435;
internal static UntypedInt _SYS_faccessat2 => 439;
internal static UntypedInt _SYS_fchmodat2 => 452;

//sys	Dup2(oldfd int, newfd int) (err error)
//sys	Fchown(fd int, uid int, gid int) (err error)
//sys	Fstat(fd int, stat *Stat_t) (err error)
//sys	Fstatfs(fd int, buf *Statfs_t) (err error)
//sys	Ftruncate(fd int, length int64) (err error)
//sysnb	Getegid() (egid int)
//sysnb	Geteuid() (euid int)
//sysnb	Getgid() (gid int)
//sysnb	Getuid() (uid int)
//sysnb	InotifyInit() (fd int, err error)
//sys	Ioperm(from int, num int, on int) (err error)
//sys	Iopl(level int) (err error)
//sys	Listen(s int, n int) (err error)
//sys	Pause() (err error)
//sys	pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys	pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys	Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys	Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys	Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
//sys	sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys	Setfsgid(gid int) (err error)
//sys	Setfsuid(uid int) (err error)
//sys	Shutdown(fd int, how int) (err error)
//sys	Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys	Statfs(path string, buf *Statfs_t) (err error)
//sys	SyncFileRange(fd int, off int64, n int64, flags int) (err error)
//sys	Truncate(path string, length int64) (err error)
//sys	Ustat(dev int, ubuf *Ustat_t) (err error)
//sys	accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error)
//sys	bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sys	connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sys	fstatat(fd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
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
//sys	EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
public static error /*err*/ Stat(@string path, ж<Stat_t> Ꮡstat) {
    return fstatat(_AT_FDCWD, path, Ꮡstat, 0);
}

public static error /*err*/ Lchown(@string path, nint uid, nint gid) {
    return Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW);
}

public static error /*err*/ Lstat(@string path, ж<Stat_t> Ꮡstat) {
    return fstatat(_AT_FDCWD, path, Ꮡstat, _AT_SYMLINK_NOFOLLOW);
}

//sys	futimesat(dirfd int, path string, times *[2]Timeval) (err error)

//go:noescape
public static partial Errno /*err*/ gettimeofday(ж<Timeval> tv);

public static error /*err*/ Gettimeofday(ж<Timeval> Ꮡtv) {
    var errno = gettimeofday(Ꮡtv);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

public static (Time_t tt, error err) Time(ж<Time_t> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var tv = ref heap(new Timeval(), out var Ꮡtv);
    var errno = gettimeofday(Ꮡtv);
    if (errno != 0) {
        return (0, errno);
    }
    if (Ꮡt != nil) {
        t = ((Time_t)tv.Sec);
    }
    return (((Time_t)tv.Sec), default!);
}

//sys	Utime(path string, buf *Utimbuf) (err error)
//sys	utimes(path string, times *[2]Timeval) (err error)
internal static Timespec setTimespec(int64 sec, int64 nsec) {
    return new Timespec(Sec: sec, Nsec: nsec);
}

internal static Timeval setTimeval(int64 sec, int64 usec) {
    return new Timeval(Sec: sec, Usec: usec);
}

public static uint64 PC(this ref PtraceRegs r) {
    return r.Rip;
}

public static void SetPC(this ref PtraceRegs r, uint64 pc) {
    r.Rip = pc;
}

public static void SetLen(this ref Iovec iov, nint length) {
    iov.Len = (uint64)length;
}

public static void SetControllen(this ref Msghdr msghdr, nint length) {
    msghdr.Controllen = (uint64)length;
}

public static void SetLen(this ref Cmsghdr cmsg, nint length) {
    cmsg.Len = (uint64)length;
}

} // end syscall_package
