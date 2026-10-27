// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package linux provides the syscall primitives required for the runtime.
namespace go.@internal.runtime.syscall;

using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class linux_package {

// TODO(https://go.dev/issue/51087): Move remaining syscalls to this package.

// Syscall6 calls system call number 'num' with arguments a1-6.
public static partial (uintptr r1, uintptr r2, uintptr errno) Syscall6(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

public static (int32 fd, uintptr errno) EpollCreate1(int32 flags) {
    var (r1, _, e) = Syscall6(SYS_EPOLL_CREATE1, (uintptr)flags, 0, 0, 0, 0, 0);
    return ((int32)r1, e);
}

internal static readonly ж<uintptr> Ꮡ_zero = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr _zero => ref ((StaticBox<uintptr>)Ꮡ_zero).ValueSlot;

public static (int32 n, uintptr errno) EpollWait(int32 epfd, slice<EpollEvent> events, int32 maxev, int32 waitms) {
    @unsafe.Pointer ev = default!;
    if (len(events) > 0){
        ev = @unsafe.Pointer.FromPinnedBox(Ꮡ(events, 0));
    } else {
        ev = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var ᴋ0 = ev;
        var (r1, _, e) = Syscall6(SYS_EPOLL_PWAIT, (uintptr)epfd, (uintptr)ᴋ0, (uintptr)maxev, (uintptr)waitms, 0, 0);
    System.GC.KeepAlive(ᴋ0);
    return ((int32)r1, e);
}

public static uintptr /*errno*/ EpollCtl(int32 epfd, int32 op, int32 fd, ж<EpollEvent> Ꮡevent) {
    var ᴋ1 = Ꮡevent;
        var (_, _, e) = Syscall6(SYS_EPOLL_CTL, (uintptr)epfd, (uintptr)op, (uintptr)fd, (uintptr)ᴋ1, 0, 0);
    System.GC.KeepAlive(ᴋ1);
    return e;
}

public static (int32 fd, uintptr errno) Eventfd(int32 initval, int32 flags) {
    var (r1, _, e) = Syscall6(SYS_EVENTFD2, (uintptr)initval, (uintptr)flags, 0, 0, 0, 0);
    return ((int32)r1, e);
}

public static (nint fd, uintptr errno) Open(ж<byte> Ꮡpath, nint mode, uint32 perm) {
    // Use SYS_OPENAT to match the syscall package.
    nint dfd = AT_FDCWD;
    var ᴋ2 = Ꮡpath;
        var (r1, _, e) = Syscall6(SYS_OPENAT, (uintptr)dfd, (uintptr)ᴋ2, (uintptr)(mode | (nint)O_LARGEFILE), (uintptr)perm, 0, 0);
    System.GC.KeepAlive(ᴋ2);
    return ((nint)r1, e);
}

public static uintptr /*errno*/ Close(nint fd) {
    var (_, _, e) = Syscall6(SYS_CLOSE, (uintptr)fd, 0, 0, 0, 0, 0);
    return e;
}

public static (nint n, uintptr errno) Read(nint fd, slice<byte> p) {
    @unsafe.Pointer p0 = default!;
    if (len(p) > 0){
        p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var ᴋ3 = p0;
        var (r1, _, e) = Syscall6(SYS_READ, (uintptr)fd, (uintptr)ᴋ3, (uintptr)len(p), 0, 0, 0);
    System.GC.KeepAlive(ᴋ3);
    return ((nint)r1, e);
}

public static (nint n, uintptr errno) Pread(nint fd, slice<byte> p, int64 offset) {
    @unsafe.Pointer p0 = default!;
    if (len(p) > 0){
        p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    uintptr r1 = default!;
    uintptr e = default!;
    var exprᴛ1 = goarch.GOARCH;
    if (exprᴛ1 == "386"u8) {
        var ᴋ4 = p0;
                (r1, _, e) = Syscall6(SYS_PREAD64, (uintptr)fd, (uintptr)ᴋ4, (uintptr)len(p), (uintptr)offset, (uintptr)(offset >> 32), 0);
        System.GC.KeepAlive(ᴋ4);
    }
    else if (exprᴛ1 == "arm"u8 || exprᴛ1 == "mipsle"u8) {
        var ᴋ5 = p0;
                (r1, _, e) = Syscall6(SYS_PREAD64, (uintptr)fd, (uintptr)ᴋ5, (uintptr)len(p), 0, (uintptr)offset, (uintptr)(offset >> 32));
        System.GC.KeepAlive(ᴋ5);
    }
    else if (exprᴛ1 == "mips"u8) {
        var ᴋ6 = p0;
                (r1, _, e) = Syscall6(SYS_PREAD64, (uintptr)fd, (uintptr)ᴋ6, (uintptr)len(p), 0, (uintptr)(offset >> 32), (uintptr)offset);
        System.GC.KeepAlive(ᴋ6);
    }
    else { /* default: */
        var ᴋ7 = p0;
                (r1, _, e) = Syscall6(SYS_PREAD64, (uintptr)fd, (uintptr)ᴋ7, (uintptr)len(p), (uintptr)offset, 0, 0);
        System.GC.KeepAlive(ᴋ7);
    }
    return ((nint)r1, e);
}

partial struct Utsname {
    public array<byte> Sysname = new(65);
    public array<byte> Nodename = new(65);
    public array<byte> Release = new(65);
    public array<byte> Version = new(65);
    public array<byte> Machine = new(65);
    public array<byte> Domainname = new(65);
}

public static uintptr /*errno*/ Uname(ж<Utsname> Ꮡbuf) {
    var ᴋ8 = Ꮡbuf;
        var (_, _, e) = Syscall6(SYS_UNAME, (uintptr)ᴋ8, 0, 0, 0, 0, 0);
    System.GC.KeepAlive(ᴋ8);
    return e;
}

} // end linux_package
