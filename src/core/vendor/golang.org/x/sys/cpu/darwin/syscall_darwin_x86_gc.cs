// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Minimal copy of x/sys/unix so the cpu package can make a
// system call on Darwin without depending on x/sys/unix.
//go:build darwin && amd64 && gc
global using Errno = go.syscall_package.Errno;

namespace go.vendor.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class cpu_package {

partial struct _C_int /*num:int32*/;

// adapted from unix.Uname() at x/sys/unix/syscall_darwin.go L419
internal static error darwinOSRelease(/*[256]*/ ж<array<byte>> release) {
    // from x/sys/unix/zerrors_openbsd_amd64.go
    UntypedInt CTL_KERN = 0x1;
    UntypedInt KERN_OSRELEASE = 0x2;

    var mib = slice<_C_int>([CTL_KERN, KERN_OSRELEASE]);
    ref var n = ref heap<uintptr>(/* unsafe.Sizeof(*release) */ (uintptr)256, out var Ꮡn);

    return sysctl(mib, release.at<byte>(0), Ꮡn, nil, 0);
}

internal static readonly ж<uintptr> Ꮡ_zero = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr _zero => ref ((StaticBox<uintptr>)Ꮡ_zero).ValueSlot; // Single-word zero for use when we need a valid pointer to 0 bytes.

// from x/sys/unix/zsyscall_darwin_amd64.go L791-807
internal static error sysctl(slice<_C_int> mib, ж<byte> old, ж<uintptr> oldlen, ж<byte> @new, uintptr newlen) {
    @unsafe.Pointer _p0 = default;
    if (len(mib) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(mib, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    {
        var (_, _, err) = syscall_syscall6(
            libc_sysctl_trampoline_addr,
            (uintptr)_p0,
            (uintptr)len(mib),
            (uintptr)old,
            (uintptr)oldlen,
            (uintptr)@new,
            (uintptr)newlen); if (err != 0) {
            return err;
        }
    }

    return default;
}

internal static uintptr libc_sysctl_trampoline_addr = (uintptr)global::go.GoCgoDynamicImports.ResolveOrZero("sysctl", "/usr/lib/libSystem.B.dylib");

// adapted from internal/cpu/cpu_arm64_darwin.go
internal static bool darwinSysctlEnabled(slice<byte> name) {
    ref var @out = ref heap<int32>((int32)0, out var Ꮡout);
    ref var nout = ref heap<uintptr>(/* unsafe.Sizeof(out) */ (uintptr)4, out var Ꮡnout);
    {
        var ret = sysctlbyname(Ꮡ(name, 0), Ꮡout.Reinterpret<int32, byte>(), Ꮡnout, nil, 0); if (ret != nil) {
            return false;
        }
    }
    return @out > 0;
}

//go:cgo_import_dynamic libc_sysctl sysctl "/usr/lib/libSystem.B.dylib"
internal static uintptr libc_sysctlbyname_trampoline_addr = (uintptr)global::go.GoCgoDynamicImports.ResolveOrZero("sysctlbyname", "/usr/lib/libSystem.B.dylib");

// adapted from runtime/sys_darwin.go in the pattern of sysctl() above, as defined in x/sys/unix
internal static error sysctlbyname(ж<byte> name, ж<byte> old, ж<uintptr> oldlen, ж<byte> @new, uintptr newlen) {
    {
        var (_, _, err) = syscall_syscall6(
            libc_sysctlbyname_trampoline_addr,
            (uintptr)name,
            (uintptr)old,
            (uintptr)oldlen,
            (uintptr)@new,
            (uintptr)newlen,
            0); if (err != 0) {
            return err;
        }
    }

    return default;
}

//go:cgo_import_dynamic libc_sysctlbyname sysctlbyname "/usr/lib/libSystem.B.dylib"

// Implemented in the runtime package (runtime/sys_darwin.go)
/*linkname*/ internal static partial (uintptr r1, uintptr r2, Errno err) syscall_syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall6((uintptr)fn, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, (uintptr)a5, (uintptr)a6);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (Errno)(uintptr)ᴛ3);
}

//go:linkname syscall_syscall6 syscall.syscall6

} // end cpu_package
