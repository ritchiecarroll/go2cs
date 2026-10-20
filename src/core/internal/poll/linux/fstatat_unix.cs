// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || wasip1
namespace go.@internal;

using unix = go.@internal.syscall.unix_package;
using Δsyscall = syscall_package;
using go.@internal.syscall;

partial class poll_package {

public static error Fstatat(this ж<FD> Ꮡfd, @string name, ж<Δsyscall.Stat_t> Ꮡs, nint flags) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => unix.Fstatat(Ꮡfd.Value.Sysfd, name, Ꮡs, flags));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

} // end poll_package
