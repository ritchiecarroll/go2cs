// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using Δsyscall = syscall_package;
using static poll_package;

partial class poll_package {

} partial class poll_package {

// SetsockoptInt wraps the setsockopt network call with an int argument.
internal static error SetsockoptIntᴍ(ж<FD> Ꮡfd, nint level, nint name, nint arg) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptInt(fd.Sysfd, level, name, arg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
internal static error SetsockoptInet4Addrᴍ(ж<FD> Ꮡfd, nint level, nint name, /*[4]*/ array<byte> arg) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        arg = arg.Clone();
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// SetsockoptLinger wraps the setsockopt network call with a Linger argument.
internal static error SetsockoptLingerᴍ(ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.Linger> l) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return err;
        }
        ᒐd1 = true;
        return Δsyscall.SetsockoptLinger(fd.Sysfd, level, name, l);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// GetsockoptInt wraps the getsockopt network call with an int argument.
internal static (nint, error) GetsockoptIntᴍ(ж<FD> Ꮡfd, nint level, nint name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        var err = Ꮡfd.incref(); if (err != nil) {
            return (-1, err);
        }
        ᒐd1 = true;
        return Δsyscall.GetsockoptInt(fd.Sysfd, level, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name, nint arg) => poll_package.SetsockoptIntᴍ(Ꮡfd, level, name, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptInet4Addr(this ж<FD> Ꮡfd, nint level, nint name, /*[4]*/ array<byte> arg) => poll_package.SetsockoptInet4Addrᴍ(Ꮡfd, level, name, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptLinger(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.Linger> l) => poll_package.SetsockoptLingerᴍ(Ꮡfd, level, name, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) GetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name) => poll_package.GetsockoptIntᴍ(Ꮡfd, level, name);
}
// </GlyphForwarders>
