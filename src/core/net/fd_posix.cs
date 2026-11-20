// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using poll = @internal.poll_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using static net_package;

partial class net_package {

// Network file descriptor.
partial struct netFD {
    internal poll.FD pfd;
    // immutable until Close
    internal nint family;
    internal nint sotype;
    internal bool isConnected; // handshake completed or use of association with peer
    internal @string net;
    internal Addr laddr;
    internal Addr raddr;
}

} partial class net_package {

internal static @string nameᴍ(ref netFD fd) {
    @string ls = default;
    @string rs = default;
    if (fd.laddr != nil) {
        ls = fd.laddr.String();
    }
    if (fd.raddr != nil) {
        rs = fd.raddr.String();
    }
    return fd.net + ":"u8 + ls + "->"u8 + rs;
}

internal static void setAddrᴍ(ж<netFD> Ꮡfd, Addr laddr, Addr raddr) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    fd.laddr = laddr;
    fd.raddr = raddr;
    // TODO Replace with runtime.AddCleanup.
    runtime.SetFinalizer(Ꮡfd.OrTypedNil(), ((Func<ж<netFD>, error>)(netᴛnetFD.Close)));
}

internal static error Closeᴍ(ж<netFD> Ꮡfd) {
    // TODO Replace with runtime.AddCleanup.
    runtime.SetFinalizer(Ꮡfd.OrTypedNil(), default);
    return Ꮡfd.Ꮡpfd.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string shutdownˢᴍnetFD => ref shutdownˢᵇᴍnetFD;
private static readonly @string shutdownˢᵇᴍnetFD = "shutdown"u8;

internal static error shutdownᴍ(ж<netFD> Ꮡfd, nint how) {
    var err = Ꮡfd.Ꮡpfd.Shutdown(how);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(shutdownˢᴍnetFD, err);
}

internal static error closeReadᴍ(ж<netFD> Ꮡfd) {
    return Ꮡfd.shutdown(syscall.SHUT_RD);
}

internal static error closeWriteᴍ(ж<netFD> Ꮡfd) {
    return Ꮡfd.shutdown(syscall.SHUT_WR);
}

internal static (nint n, error err) Readᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.Read(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readSyscallName, err));
}

internal static (nint n, syscall.Sockaddr sa, error err) readFromᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default;
    syscall.Sockaddr sa = default;
    error err = default;

    (n, sa, err) = Ꮡfd.Ꮡpfd.ReadFrom(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, sa, wrapSyscallError(readFromSyscallName, err));
}
internal static (nint n, error err) readFromInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> from) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.ReadFromInet4(p, from);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readFromSyscallName, err));
}

internal static (nint n, error err) readFromInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> from) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.ReadFromInet6(p, from);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readFromSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, syscall.Sockaddr sa, error err) readMsgᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    syscall.Sockaddr sa = default;
    error err = default;

    (n, oobn, retflags, sa, err) = Ꮡfd.Ꮡpfd.ReadMsg(p, oob, flags);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, sa, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    error err = default;

    (n, oobn, retflags, err) = Ꮡfd.Ꮡpfd.ReadMsgInet4(p, oob, flags, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    error err = default;

    (n, oobn, retflags, err) = Ꮡfd.Ꮡpfd.ReadMsgInet6(p, oob, flags, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint nn, error err) Writeᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint nn = default;
    error err = default;

    (nn, err) = Ꮡfd.Ꮡpfd.Write(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (nn, wrapSyscallError(writeSyscallName, err));
}

internal static (nint n, error err) writeToᴍ(ж<netFD> Ꮡfd, slice<byte> p, syscall.Sockaddr sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteTo(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, error err) writeToInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteToInet4(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, error err) writeToInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteToInet6(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsgᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsg(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsgInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsgInet4(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsgInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsgInet6(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static error SetDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetDeadline(t);
}

internal static error SetReadDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetReadDeadline(t);
}

internal static error SetWriteDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetWriteDeadline(t);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this ref netFD fd) => net_package.nameᴍ(ref fd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setAddr(this ж<netFD> Ꮡfd, Addr laddr, Addr raddr) => net_package.setAddrᴍ(Ꮡfd, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<netFD> Ꮡfd) => net_package.Closeᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error shutdown(this ж<netFD> Ꮡfd, nint how) => net_package.shutdownᴍ(Ꮡfd, how);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error closeRead(this ж<netFD> Ꮡfd) => net_package.closeReadᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error closeWrite(this ж<netFD> Ꮡfd) => net_package.closeWriteᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Readᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, syscall.Sockaddr sa, error err) readFrom(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.readFromᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) readFromInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> from) => net_package.readFromInet4ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) readFromInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> from) => net_package.readFromInet6ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, syscall.Sockaddr sa, error err) readMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => net_package.readMsgᴍ(Ꮡfd, p, oob, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> sa) => net_package.readMsgInet4ᴍ(Ꮡfd, p, oob, flags, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> sa) => net_package.readMsgInet6ᴍ(Ꮡfd, p, oob, flags, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint nn, error err) Write(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Writeᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeTo(this ж<netFD> Ꮡfd, slice<byte> p, syscall.Sockaddr sa) => net_package.writeToᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeToInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> sa) => net_package.writeToInet4ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeToInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> sa) => net_package.writeToInet6ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => net_package.writeMsgᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> sa) => net_package.writeMsgInet4ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> sa) => net_package.writeMsgInet6ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetReadDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetWriteDeadlineᴍ(Ꮡfd, t);
}
// </GlyphForwarders>
