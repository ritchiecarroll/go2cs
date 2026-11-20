// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || wasip1
namespace go.@internal;

using Δsyscall = syscall_package;
using static poll_package;

partial class poll_package {

} partial class poll_package {

// Fsync wraps syscall.Fsync.
internal static error Fsyncᴍ(ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return ignoringEINTR(() => Δsyscall.Fsync(Ꮡfd.Value.Sysfd));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fsync(this ж<FD> Ꮡfd) => poll_package.Fsyncᴍ(Ꮡfd);
}
// </GlyphForwarders>
