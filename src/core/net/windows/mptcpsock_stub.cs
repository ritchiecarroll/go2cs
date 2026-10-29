// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using context = context_package;

partial class net_package {

internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> laddr, ж<TCPAddr> raddr) {
    return Ꮡsd.dialTCP(ctx, laddr, raddr);
}

internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> laddr) {
    return Ꮡsl.listenTCP(ctx, laddr);
}

internal static bool isUsingMultipathTCP(ref netFD fd) {
    return false;
}

} // end net_package
