// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

internal static Timespec setTimespec(int64 sec, int64 nsec) {
    return new Timespec(Sec: sec, Nsec: nsec);
}

internal static Timeval setTimeval(int64 sec, int64 usec) {
    return new Timeval(Sec: sec, Usec: (int32)usec);
}

//sys	Fstat(fd int, stat *Stat_t) (err error) = SYS_fstat64
//sys	Fstatfs(fd int, stat *Statfs_t) (err error) = SYS_fstatfs64
//sysnb	Gettimeofday(tp *Timeval) (err error)
//sys	Lstat(path string, stat *Stat_t) (err error) = SYS_lstat64
//sys	Stat(path string, stat *Stat_t) (err error) = SYS_stat64
//sys	Statfs(path string, stat *Statfs_t) (err error) = SYS_statfs64
//sys   fstatat(fd int, path string, stat *Stat_t, flags int) (err error) = SYS_fstatat64
//sys   ptrace(request int, pid int, addr uintptr, data uintptr) (err error)
public static void SetKevent(ж<Kevent_t> Ꮡk, nint fd, nint mode, nint flags) {
    ref var k = ref Ꮡk.DerefOrNull();

    k.Ident = (uint64)fd;
    k.Filter = (int16)mode;
    k.Flags = (uint16)flags;
}

[GoRecv] public static void SetLen(this ref Iovec iov, nint length) {
    iov.Len = (uint64)length;
}

[GoRecv] public static void SetControllen(this ref Msghdr msghdr, nint length) {
    msghdr.Controllen = (uint32)length;
}

[GoRecv] public static void SetLen(this ref Cmsghdr cmsg, nint length) {
    cmsg.Len = (uint32)length;
}

internal static (nint written, error err) sendfile(nint outfd, nint infd, ref int64 offset, nint count) {
    nint written = default!;
    error err = default!;

    ref var length = ref heap(new uint64(), out var Ꮡlength);

    length = (uint64)count;
    var ᴋ0 = Ꮡlength;
        var (_, _, e1) = syscall6(abi.FuncPCABI0(libc_sendfile_trampoline), (uintptr)infd, (uintptr)outfd, (uintptr)(offset), (uintptr)ᴋ0, 0, 0);
    System.GC.KeepAlive(ᴋ0);
    written = (nint)length;
    if (e1 != 0) {
        err = e1;
    }
    return (written, err);
}

internal static partial void libc_sendfile_trampoline();

//go:cgo_import_dynamic libc_sendfile sendfile "/usr/lib/libSystem.B.dylib"

// Implemented in the runtime package (runtime/sys_darwin_64.go)
internal static partial (uintptr r1, uintptr r2, Errno err) syscallX(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

public static partial (uintptr r1, uintptr r2, Errno err) Syscall9(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9);

} // end syscall_package
