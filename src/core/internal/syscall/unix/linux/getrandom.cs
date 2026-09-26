// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux
namespace go.@internal.syscall;

using atomic = sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using sync;

partial class unix_package {

//go:linkname vgetrandom runtime.vgetrandom
//go:noescape
[global::System.Diagnostics.StackTraceHidden] internal static (nint ret, bool supported) vgetrandom(slice<byte> p, uint32 flags) {
    var (ᴛ1, ᴛ2) = go.runtime_package.vgetrandom(p, flags);
    return (ᴛ1, ᴛ2);
}

internal static ж<atomic.Bool> ᏑgetrandomUnsupported = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool getrandomUnsupported => ref ᏑgetrandomUnsupported.Value;

[GoType("num:uintptr")] partial struct GetRandomFlag;

// GetRandom calls the getrandom system call.
public static (nint n, error err) GetRandom(slice<byte> p, GetRandomFlag flags) {
    var (ret, supported) = vgetrandom(p, (uint32)(uintptr)flags);
    if (supported) {
        if (ret < 0) {
            return (0, ((syscall.Errno)(uintptr)(-ret)));
        }
        return (ret, default!);
    }
    if (ᏑgetrandomUnsupported.Load()) {
        return (0, syscall.ENOSYS);
    }
    var ᴋ0 = @unsafe.SliceData(p);
        var (r1, _, errno) = syscall.Syscall(getrandomTrap, (uintptr)ᴋ0, (uintptr)len(p), (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    if (errno != 0) {
        if (errno == syscall.ENOSYS) {
            ᏑgetrandomUnsupported.Store(true);
        }
        return (0, errno);
    }
    return ((nint)r1, default!);
}

} // end unix_package
