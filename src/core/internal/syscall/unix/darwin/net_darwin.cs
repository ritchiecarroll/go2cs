// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class unix_package {

public static UntypedInt AI_CANONNAME => 0x2;
public static UntypedInt AI_ALL => 0x100;
public static UntypedInt AI_V4MAPPED => 0x800;
public static UntypedInt AI_MASK => 0x1407;
public static UntypedInt EAI_AGAIN => 2;
public static UntypedInt EAI_NODATA => 7;
public static UntypedInt EAI_NONAME => 8;
public static UntypedInt EAI_SERVICE => 9;
public static UntypedInt EAI_SYSTEM => 11;
public static UntypedInt EAI_OVERFLOW => 14;
public static UntypedInt NI_NAMEREQD => 4;

[GoType] partial struct Addrinfo {
    public int32 Flags;
    public int32 Family;
    public int32 Socktype;
    public int32 Protocol;
    public uint32 Addrlen;
    public ж<byte> Canonname;
    public ж<syscall.RawSockaddr> Addr;
    public ж<Addrinfo> Next;
}

//go:cgo_ldflag "-lresolv"

//go:cgo_import_dynamic libc_getaddrinfo getaddrinfo "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getaddrinfo_trampoline();

// go2cs generated this placeholder — func Getaddrinfo is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_freeaddrinfo freeaddrinfo "/usr/lib/libSystem.B.dylib"
internal static partial void libc_freeaddrinfo_trampoline();

// go2cs generated this placeholder — func Freeaddrinfo is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_getnameinfo getnameinfo "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getnameinfo_trampoline();

public static (nint, error) Getnameinfo(ж<syscall.RawSockaddr> Ꮡsa, nint salen, ж<byte> Ꮡhost, nint hostlen, ж<byte> Ꮡserv, nint servlen, nint flags) {
    var (gerrno, _, errno) = syscall_syscall9(abi.FuncPCABI0(libc_getnameinfo_trampoline),
        (uintptr)Ꮡsa,
        (uintptr)salen,
        (uintptr)Ꮡhost,
        (uintptr)hostlen,
        (uintptr)Ꮡserv,
        (uintptr)servlen,
        (uintptr)flags,
        0,
        0);
    error err = default!;
    if (errno != 0) {
        err = errno;
    }
    return ((nint)gerrno, err);
}

//go:cgo_import_dynamic libc_gai_strerror gai_strerror "/usr/lib/libSystem.B.dylib"
internal static partial void libc_gai_strerror_trampoline();

public static @string GaiStrerror(nint ecode) {
    var (r1, _, _) = syscall_syscall(abi.FuncPCABI0(libc_gai_strerror_trampoline),
        (uintptr)ecode,
        0, 0);
    return GoString((ж<byte>)(uintptr)((@unsafe.Pointer)r1));
}

// Implemented in the runtime package.
internal static partial @string gostring(ж<byte> _);

public static @string GoString(ж<byte> Ꮡp) {
    return gostring(Ꮡp);
}

//go:linkname syscall_syscall syscall.syscall
internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

//go:linkname syscall_syscallPtr syscall.syscallPtr
internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3);

//go:linkname syscall_syscall6 syscall.syscall6
internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

//go:linkname syscall_syscall6X syscall.syscall6X
internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

//go:linkname syscall_syscall9 syscall.syscall9
internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall9(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9);

[GoType] partial struct ResState {
    internal array<uintptr> unexported = new(69);
}

//go:cgo_import_dynamic libresolv_res_9_ninit res_9_ninit "/usr/lib/libresolv.9.dylib"
internal static partial void libresolv_res_9_ninit_trampoline();

public static error ResNinit(ж<ResState> Ꮡstate) {
    var (_, _, errno) = syscall_syscall(abi.FuncPCABI0(libresolv_res_9_ninit_trampoline),
        (uintptr)Ꮡstate,
        0, 0);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

//go:cgo_import_dynamic libresolv_res_9_nclose res_9_nclose "/usr/lib/libresolv.9.dylib"
internal static partial void libresolv_res_9_nclose_trampoline();

public static void ResNclose(ж<ResState> Ꮡstate) {
    syscall_syscall(abi.FuncPCABI0(libresolv_res_9_nclose_trampoline),
        (uintptr)Ꮡstate,
        0, 0);
}

//go:cgo_import_dynamic libresolv_res_9_nsearch res_9_nsearch "/usr/lib/libresolv.9.dylib"
internal static partial void libresolv_res_9_nsearch_trampoline();

public static (nint, error) ResNsearch(ж<ResState> Ꮡstate, ж<byte> Ꮡdname, nint @class, nint typ, ж<byte> Ꮡans, nint anslen) {
    var (r1, _, errno) = syscall_syscall6(abi.FuncPCABI0(libresolv_res_9_nsearch_trampoline),
        (uintptr)Ꮡstate,
        (uintptr)Ꮡdname,
        (uintptr)@class,
        (uintptr)typ,
        (uintptr)Ꮡans,
        (uintptr)anslen);
    if (errno != 0) {
        return (0, errno);
    }
    return ((nint)(int32)r1, default!);
}

} // end unix_package
