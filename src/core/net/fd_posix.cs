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

[GoMethodBody("name")] internal static @string nameᴍ(ref netFD fd) {
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

[GoMethodBody("setAddr")] internal static void setAddrᴍ(ж<netFD> Ꮡfd, Addr laddr, Addr raddr) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    fd.laddr = laddr;
    fd.raddr = raddr;
    // TODO Replace with runtime.AddCleanup.
    runtime.SetFinalizer(Ꮡfd.OrTypedNil(), ((Func<ж<netFD>, error>)(netᴛnetFD.Close)));
}

[GoMethodBody("Close")] internal static error Closeᴍ(ж<netFD> Ꮡfd) {
    // TODO Replace with runtime.AddCleanup.
    runtime.SetFinalizer(Ꮡfd.OrTypedNil(), default);
    return Ꮡfd.Ꮡpfd.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string shutdownˢᴍnetFD => ref shutdownˢᵇᴍnetFD;
private static readonly @string shutdownˢᵇᴍnetFD = "shutdown"u8;

[GoMethodBody("shutdown")] internal static error shutdownᴍ(ж<netFD> Ꮡfd, nint how) {
    var err = Ꮡfd.Ꮡpfd.Shutdown(how);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(shutdownˢᴍnetFD, err);
}

[GoMethodBody("closeRead")] internal static error closeReadᴍ(ж<netFD> Ꮡfd) {
    return Ꮡfd.shutdown(syscall.SHUT_RD);
}

[GoMethodBody("closeWrite")] internal static error closeWriteᴍ(ж<netFD> Ꮡfd) {
    return Ꮡfd.shutdown(syscall.SHUT_WR);
}

[GoMethodBody("Read")] internal static (nint n, error err) Readᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.Read(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readSyscallName, err));
}

[GoMethodBody("readFrom")] internal static (nint n, syscall.Sockaddr sa, error err) readFromᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default;
    syscall.Sockaddr sa = default;
    error err = default;

    (n, sa, err) = Ꮡfd.Ꮡpfd.ReadFrom(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, sa, wrapSyscallError(readFromSyscallName, err));
}
[GoMethodBody("readFromInet4")] internal static (nint n, error err) readFromInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> from) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.ReadFromInet4(p, from);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readFromSyscallName, err));
}

[GoMethodBody("readFromInet6")] internal static (nint n, error err) readFromInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> from) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.ReadFromInet6(p, from);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(readFromSyscallName, err));
}

[GoMethodBody("readMsg")] internal static (nint n, nint oobn, nint retflags, syscall.Sockaddr sa, error err) readMsgᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    syscall.Sockaddr sa = default;
    error err = default;

    (n, oobn, retflags, sa, err) = Ꮡfd.Ꮡpfd.ReadMsg(p, oob, flags);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, sa, wrapSyscallError(readMsgSyscallName, err));
}

[GoMethodBody("readMsgInet4")] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    error err = default;

    (n, oobn, retflags, err) = Ꮡfd.Ꮡpfd.ReadMsgInet4(p, oob, flags, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

[GoMethodBody("readMsgInet6")] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    error err = default;

    (n, oobn, retflags, err) = Ꮡfd.Ꮡpfd.ReadMsgInet6(p, oob, flags, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

[GoMethodBody("Write")] internal static (nint nn, error err) Writeᴍ(ж<netFD> Ꮡfd, slice<byte> p) {
    nint nn = default;
    error err = default;

    (nn, err) = Ꮡfd.Ꮡpfd.Write(p);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (nn, wrapSyscallError(writeSyscallName, err));
}

[GoMethodBody("writeTo")] internal static (nint n, error err) writeToᴍ(ж<netFD> Ꮡfd, slice<byte> p, syscall.Sockaddr sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteTo(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

[GoMethodBody("writeToInet4")] internal static (nint n, error err) writeToInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteToInet4(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

[GoMethodBody("writeToInet6")] internal static (nint n, error err) writeToInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.WriteToInet6(p, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writeToSyscallName, err));
}

[GoMethodBody("writeMsg")] internal static (nint n, nint oobn, error err) writeMsgᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsg(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

[GoMethodBody("writeMsgInet4")] internal static (nint n, nint oobn, error err) writeMsgInet4ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsgInet4(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

[GoMethodBody("writeMsgInet6")] internal static (nint n, nint oobn, error err) writeMsgInet6ᴍ(ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> sa) {
    nint n = default;
    nint oobn = default;
    error err = default;

    (n, oobn, err) = Ꮡfd.Ꮡpfd.WriteMsgInet6(p, oob, sa);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

[GoMethodBody("SetDeadline")] internal static error SetDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetDeadline(t);
}

[GoMethodBody("SetReadDeadline")] internal static error SetReadDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetReadDeadline(t);
}

[GoMethodBody("SetWriteDeadline")] internal static error SetWriteDeadlineᴍ(ж<netFD> Ꮡfd, time.Time t) {
    return Ꮡfd.Ꮡpfd.SetWriteDeadline(t);
}

}
