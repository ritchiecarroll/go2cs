// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Darwin system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

public static partial (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3);

public static partial (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

public static partial (uintptr r1, uintptr r2, Errno err) RawSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3);

public static partial (uintptr r1, uintptr r2, Errno err) RawSyscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

internal static uintptr dupTrampoline = abi.FuncPCABI0(libc_dup2_trampoline);

[GoType] partial struct SockaddrDatalink {
    public uint8 Len;
    public uint8 Family;
    public uint16 Index;
    public uint8 Type;
    public uint8 Nlen;
    public uint8 Alen;
    public uint8 Slen;
    public array<int8> Data = new(12);
    internal RawSockaddrDatalink raw;
}

// Translate "kern.hostname" to []_C_int{0,1,2,3}.
internal static (slice<_C_int> mib, error err) nametomib(@string name) {
    slice<_C_int> mib = default!;
    error err = default!;

    uintptr siz = /* unsafe.Sizeof(mib[0]) */ 4;
    // NOTE(rsc): It seems strange to set the buffer to have
    // size CTL_MAXNAME+2 but use only CTL_MAXNAME
    // as the size. I don't know why the +2 is here, but the
    // kernel uses +2 for its own implementation of this function.
    // I am scared that if we don't include the +2 here, the kernel
    // will silently write 2 words farther than we specify
    // and we'll get memory corruption.
    ref var buf = ref heap(new array<_C_int>(14), out var Ꮡbuf);
    ref var n = ref heap<uintptr>(out var Ꮡn);
    n = (uintptr)CTL_MAXNAME * siz;
    var p = Ꮡbuf.at<_C_int>(0).Reinterpret<_C_int, byte>();
    (var bytes, err) = ByteSliceFromString(name);
    if (err != default!) {
        return (default!, err);
    }
    // Magic sysctl: "setting" 0.3 to a string name
    // lets you read back the array of integers form.
    {
        err = sysctl(new _C_int[]{0, 3}.slice(), p, Ꮡn, Ꮡ(bytes, 0), (uintptr)len(name)); if (err != default!) {
            return (default!, err);
        }
    }
    return (buf[0..(int)(n / siz)], default!);
}

internal static (uint64, bool) direntIno(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Ino) */ (uintptr)0, /* unsafe.Sizeof(Dirent{}.Ino) */ (uintptr)8);
}

internal static (uint64, bool) direntReclen(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Reclen) */ (uintptr)16, /* unsafe.Sizeof(Dirent{}.Reclen) */ (uintptr)2);
}

internal static (uint64, bool) direntNamlen(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Namlen) */ (uintptr)18, /* unsafe.Sizeof(Dirent{}.Namlen) */ (uintptr)2);
}

public static error /*err*/ PtraceAttach(nint pid) {
    return ptrace(PT_ATTACH, pid, 0, 0);
}

public static error /*err*/ PtraceDetach(nint pid) {
    return ptrace(PT_DETACH, pid, 0, 0);
}

//sysnb pipe(p *[2]int32) (err error)
public static error /*err*/ Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return EINVAL;
    }
    ref var q = ref heap(new array<int32>(2), out var Ꮡq);
    err = pipe(Ꮡq);
    if (err == default!) {
        p[0] = (nint)q[0];
        p[1] = (nint)q[1];
    }
    return err;
}

public static (nint n, error err) Getfsstat(slice<Statfs_t> buf, nint flags) {
    nint n = default!;
    error err = default!;

    @unsafe.Pointer _p0 = default!;
    uintptr bufsize = default!;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
        bufsize = /* unsafe.Sizeof(Statfs_t{}) */ (uintptr)2168 * (uintptr)len(buf);
    }
    var ᴋ0 = _p0;
        var (r0, _, e1) = syscall(abi.FuncPCABI0(libc_getfsstat_trampoline), (uintptr)ᴋ0, bufsize, (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    n = (nint)r0;
    if (e1 != 0) {
        err = e1;
    }
    return (n, err);
}

internal static partial void libc_getfsstat_trampoline();

//go:cgo_import_dynamic libc_getfsstat getfsstat "/usr/lib/libSystem.B.dylib"
// utimensat should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/tetratelabs/wazero
//
// See go.dev/issue/67401.
//
//go:linkname utimensat
//sys	utimensat(dirfd int, path string, times *[2]Timespec, flags int) (err error)
/*
 * Wrapped
 */
//sys	kill(pid int, signum int, posix int) (err error)
public static error /*err*/ Kill(nint pid, ΔSignal signum) {
    return kill(pid, (nint)signum, 1);
}

/*
 * Exposed directly
 */
//sys	Access(path string, mode uint32) (err error)
//sys	Adjtime(delta *Timeval, olddelta *Timeval) (err error)
//sys	Chdir(path string) (err error)
//sys	Chflags(path string, flags int) (err error)
//sys	Chmod(path string, mode uint32) (err error)
//sys	Chown(path string, uid int, gid int) (err error)
//sys	Chroot(path string) (err error)
//sys	Close(fd int) (err error)
//sys	closedir(dir uintptr) (err error)
//sys	Dup(fd int) (nfd int, err error)
//sys	Dup2(from int, to int) (err error)
//sys	Exchangedata(path1 string, path2 string, options int) (err error)
//sys	Fchdir(fd int) (err error)
//sys	Fchflags(fd int, flags int) (err error)
//sys	Fchmod(fd int, mode uint32) (err error)
//sys	Fchown(fd int, uid int, gid int) (err error)
//sys	Flock(fd int, how int) (err error)
//sys	Fpathconf(fd int, name int) (val int, err error)
//sys	Fsync(fd int) (err error)
//  Fsync is not called for os.File.Sync(). Please see internal/poll/fd_fsync_darwin.go
//sys	Ftruncate(fd int, length int64) (err error)
//sys	Getdtablesize() (size int)
//sysnb	Getegid() (egid int)
//sysnb	Geteuid() (uid int)
//sysnb	Getgid() (gid int)
//sysnb	Getpgid(pid int) (pgid int, err error)
//sysnb	Getpgrp() (pgrp int)
//sysnb	Getpid() (pid int)
//sysnb	Getppid() (ppid int)
//sys	Getpriority(which int, who int) (prio int, err error)
//sysnb	Getrlimit(which int, lim *Rlimit) (err error)
//sysnb	Getrusage(who int, rusage *Rusage) (err error)
//sysnb	Getsid(pid int) (sid int, err error)
//sysnb	Getuid() (uid int)
//sysnb	Issetugid() (tainted bool)
//sys	Kqueue() (fd int, err error)
//sys	Lchown(path string, uid int, gid int) (err error)
//sys	Link(path string, link string) (err error)
//sys	Listen(s int, backlog int) (err error)
//sys	Mkdir(path string, mode uint32) (err error)
//sys	Mkfifo(path string, mode uint32) (err error)
//sys	Mknod(path string, mode uint32, dev int) (err error)
//sys	Mlock(b []byte) (err error)
//sys	Mlockall(flags int) (err error)
//sys	Mprotect(b []byte, prot int) (err error)
//sys	msync(b []byte, flags int) (err error)
//sys	Munlock(b []byte) (err error)
//sys	Munlockall() (err error)
//sys	Open(path string, mode int, perm uint32) (fd int, err error)
//sys	Pathconf(path string, name int) (val int, err error)
//sys	pread(fd int, p []byte, offset int64) (n int, err error)
//sys	pwrite(fd int, p []byte, offset int64) (n int, err error)
//sys	read(fd int, p []byte) (n int, err error)
//sys	readdir_r(dir uintptr, entry *Dirent, result **Dirent) (res Errno)
//sys	Readlink(path string, buf []byte) (n int, err error)
//sys	Rename(from string, to string) (err error)
//sys	Revoke(path string) (err error)
//sys	Rmdir(path string) (err error)
//sys	Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_lseek
//sys	Select(n int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (err error)
//sys	Setegid(egid int) (err error)
//sysnb	Seteuid(euid int) (err error)
//sysnb	Setgid(gid int) (err error)
//sys	Setlogin(name string) (err error)
//sysnb	Setpgid(pid int, pgid int) (err error)
//sys	Setpriority(which int, who int, prio int) (err error)
//sys	Setprivexec(flag int) (err error)
//sysnb	Setregid(rgid int, egid int) (err error)
//sysnb	Setreuid(ruid int, euid int) (err error)
//sysnb	setrlimit(which int, lim *Rlimit) (err error)
//sysnb	Setsid() (pid int, err error)
//sysnb	Settimeofday(tp *Timeval) (err error)
//sysnb	Setuid(uid int) (err error)
//sys	Symlink(path string, link string) (err error)
//sys	Sync() (err error)
//sys	Truncate(path string, length int64) (err error)
//sys	Umask(newmask int) (oldmask int)
//sys	Undelete(path string) (err error)
//sys	Unlink(path string) (err error)
//sys	Unmount(path string, flags int) (err error)
//sys	write(fd int, p []byte) (n int, err error)
//sys	writev(fd int, iovecs []Iovec) (cnt uintptr, err error)
//sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
//sys   munmap(addr uintptr, length uintptr) (err error)
//sysnb fork() (pid int, err error)
//sysnb execve(path *byte, argv **byte, envp **byte) (err error)
//sysnb exit(res int) (err error)
//sys	sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error)
//sys   unlinkat(fd int, path string, flags int) (err error)
//sys   openat(fd int, path string, flags int, perm uint32) (fdret int, err error)
//sys	getcwd(buf []byte) (n int, err error)
[GoInit] internal static void initΔ1() {
    execveDarwin = execve;
}

internal static (uintptr dir, error err) fdopendir(nint fd) {
    uintptr dir = default!;
    error err = default!;

    var (r0, _, e1) = syscallPtr(abi.FuncPCABI0(libc_fdopendir_trampoline), (uintptr)fd, 0, 0);
    dir = r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (dir, err);
}

internal static partial void libc_fdopendir_trampoline();

//go:cgo_import_dynamic libc_fdopendir fdopendir "/usr/lib/libSystem.B.dylib"
internal static (nint n, error err) readlen(nint fd, ж<byte> Ꮡbuf, nint nbuf) {
    nint n = default!;
    error err = default!;

    var ᴋ1 = Ꮡbuf;
        var (r0, _, e1) = syscall(abi.FuncPCABI0(libc_read_trampoline), (uintptr)fd, (uintptr)ᴋ1, (uintptr)nbuf);
    System.GC.KeepAlive(ᴋ1);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint n, error err) Getdirentries(nint fd, slice<byte> buf, ж<uintptr> Ꮡbasep) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var basep = ref Ꮡbasep.DerefOrNull();

        // Simulate Getdirentries using fdopendir/readdir_r/closedir.
        // We store the number of entries to skip in the seek
        // offset of fd. See issue #31368.
        // It's not the full required semantics, but should handle the case
        // of calling Getdirentries or ReadDirent repeatedly.
        // It won't handle assigning the results of lseek to *basep, or handle
        // the directory being edited underfoot.
        (var skip, err) = Seek(fd, 0, 1);
        /* SEEK_CUR */
        if (err != default!) {
            (n, err) = (0, err); goto ᒐdone;
        }
        // We need to duplicate the incoming file descriptor
        // because the caller expects to retain control of it, but
        // fdopendir expects to take control of its argument.
        // Just Dup'ing the file descriptor is not enough, as the
        // result shares underlying state. Use openat to make a really
        // new file descriptor referring to the same directory.
        (var fd2, err) = openat(fd, "."u8, O_RDONLY, 0);
        if (err != default!) {
            (n, err) = (0, err); goto ᒐdone;
        }
        (var d, err) = fdopendir(fd2);
        if (err != default!) {
            Close(fd2);
            (n, err) = (0, err); goto ᒐdone;
        }
        defer(closedir, d, ref ᒐ);
        int64 cnt = default!;
        while (ᐧ) {
            ref var entry = ref heap(new Dirent(), out var Ꮡentry);
            ref var entryp = ref heap<ж<Dirent>>(out var Ꮡentryp);
            var e = readdir_r(d, Ꮡentry, Ꮡentryp);
            if (e != 0) {
                (n, err) = (n, errnoErr(e)); goto ᒐdone;
            }
            if (entryp == nil) {
                break;
            }
            if (skip > 0) {
                skip--;
                cnt++;
                continue;
            }
            nint reclen = (nint)entry.Reclen;
            if (reclen > len(buf)) {
                // Not enough room. Return for now.
                // The counter will let us know where we should start up again.
                // Note: this strategy for suspending in the middle and
                // restarting is O(n^2) in the length of the directory. Oh well.
                break;
            }
            // Copy entry into return buffer.
            copy(buf, @unsafe.Slice(Ꮡentry.Reinterpret<Dirent, byte>(), reclen));
            buf = buf[(int)(reclen)..];
            n += reclen;
            cnt++;
        }
        // Set the seek offset of the input fd to record
        // how many files we've already returned.
        (_, err) = Seek(fd, cnt, 0);
        /* SEEK_SET */
        if (err != default!) {
            goto ᒐdone;
        }
        (n, err) = (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

// Implemented in the runtime package (runtime/sys_darwin.go)
internal static partial (uintptr r1, uintptr r2, Errno err) syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

internal static partial (uintptr r1, uintptr r2, Errno err) syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

internal static partial (uintptr r1, uintptr r2, Errno err) syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

internal static partial (uintptr r1, uintptr r2, Errno err) rawSyscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

internal static partial (uintptr r1, uintptr r2, Errno err) rawSyscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

internal static partial (uintptr r1, uintptr r2, Errno err) syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

} // end syscall_package
