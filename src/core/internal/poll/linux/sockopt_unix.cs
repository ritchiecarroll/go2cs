// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal;

using Δsyscall = syscall_package;
using static poll_package;

partial class poll_package {

} partial class poll_package {

// SetsockoptByte wraps the setsockopt network call with a byte argument.
internal static error SetsockoptByteᴍ(ж<FD> Ꮡfd, nint level, nint name, byte arg) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptByte(fd.Sysfd, level, name, arg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptByte(this ж<FD> Ꮡfd, nint level, nint name, byte arg) => poll_package.SetsockoptByteᴍ(Ꮡfd, level, name, arg);
}
// </GlyphForwarders>
