// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using Δsyscall = syscall_package;
using static poll_package;

partial class poll_package {

} partial class poll_package {

// SetsockoptIPMreq wraps the setsockopt network call with an IPMreq argument.
[GoMethodBody("SetsockoptIPMreq")] internal static error SetsockoptIPMreqᴍ(ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreq> mreq) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptIPMreq(fd.Sysfd, level, name, mreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// SetsockoptIPv6Mreq wraps the setsockopt network call with an IPv6Mreq argument.
[GoMethodBody("SetsockoptIPv6Mreq")] internal static error SetsockoptIPv6Mreqᴍ(ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPv6Mreq> mreq) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptIPv6Mreq(fd.Sysfd, level, name, mreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

}
