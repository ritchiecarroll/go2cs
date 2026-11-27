// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal.syscall;

using syscall = syscall_package;
// import _ "unsafe" (nothing to initialize)

partial class unix_package {

//go:linkname RecvfromInet4 syscall.recvfromInet4
//go:noescape
public static partial (nint, error) RecvfromInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> from);

//go:linkname RecvfromInet6 syscall.recvfromInet6
//go:noescape
public static partial (nint n, error err) RecvfromInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> from);

//go:linkname SendtoInet4 syscall.sendtoInet4
//go:noescape
public static partial error /*err*/ SendtoInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> to);

//go:linkname SendtoInet6 syscall.sendtoInet6
//go:noescape
public static partial error /*err*/ SendtoInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> to);

//go:linkname SendmsgNInet4 syscall.sendmsgNInet4
//go:noescape
public static partial (nint n, error err) SendmsgNInet4(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> to, nint flags);

//go:linkname SendmsgNInet6 syscall.sendmsgNInet6
//go:noescape
public static partial (nint n, error err) SendmsgNInet6(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> to, nint flags);

//go:linkname RecvmsgInet4 syscall.recvmsgInet4
//go:noescape
public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet4(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> from);

//go:linkname RecvmsgInet6 syscall.recvmsgInet6
//go:noescape
public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet6(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> from);

} // end unix_package
