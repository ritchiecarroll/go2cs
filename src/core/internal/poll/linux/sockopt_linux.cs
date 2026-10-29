// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// SetsockoptIPMreqn wraps the setsockopt network call with an IPMreqn argument.
public static error SetsockoptIPMreqn(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreqn> mreq) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != nil) {
                return err;
            }
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptIPMreqn(fd.Sysfd, level, name, mreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

} // end poll_package
