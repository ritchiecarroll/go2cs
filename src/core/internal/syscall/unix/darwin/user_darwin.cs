// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class unix_package {

//go:cgo_import_dynamic libc_getgrouplist getgrouplist "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getgrouplist_trampoline();

public static error Getgrouplist(ж<byte> Ꮡname, uint32 gid, ж<uint32> Ꮡgids, ж<int32> Ꮡn) {
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_getgrouplist_trampoline),
        (uintptr)Ꮡname, (uintptr)gid, (uintptr)Ꮡgids,
        (uintptr)Ꮡn, 0, 0);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

public static UntypedInt SC_GETGR_R_SIZE_MAX => 0x46;
public static UntypedInt SC_GETPW_R_SIZE_MAX => 0x47;

[GoType] partial struct Passwd {
    public ж<byte> Name;
    public ж<byte> ΔPasswd;
    public uint32 Uid; // uid_t
    public uint32 Gid; // gid_t
    public int64 Change;  // time_t
    public ж<byte> Class;
    public ж<byte> Gecos;
    public ж<byte> Dir;
    public ж<byte> Shell;
    public int64 Expire; // time_t
}

[GoType] partial struct Group {
    public ж<byte> Name;
    public ж<byte> Passwd;
    public uint32 Gid; // gid_t
    public ж<ж<byte>> Mem;
}

//go:cgo_import_dynamic libc_getpwnam_r getpwnam_r  "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getpwnam_r_trampoline();

// go2cs generated this placeholder — func Getpwnam is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_getpwuid_r getpwuid_r  "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getpwuid_r_trampoline();

// go2cs generated this placeholder — func Getpwuid is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_getgrnam_r getgrnam_r  "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getgrnam_r_trampoline();

// go2cs generated this placeholder — func Getgrnam is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_getgrgid_r getgrgid_r  "/usr/lib/libSystem.B.dylib"
internal static partial void libc_getgrgid_r_trampoline();

// go2cs generated this placeholder — func Getgrgid is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:cgo_import_dynamic libc_sysconf sysconf "/usr/lib/libSystem.B.dylib"
internal static partial void libc_sysconf_trampoline();

public static int64 Sysconf(int32 key) {
    var (val, _, errno) = syscall_syscall6X(abi.FuncPCABI0(libc_sysconf_trampoline),
        (uintptr)key, 0, 0, 0, 0, 0);
    if (errno != 0) {
        return -1;
    }
    return (int64)val;
}

} // end unix_package
